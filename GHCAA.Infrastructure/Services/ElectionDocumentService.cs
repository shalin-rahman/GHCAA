using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using System.Text.Json;
using System.Text.RegularExpressions;
using GHCAA.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services;

// A table of stored records printed on a filled form.
public sealed record FormTable(string Heading, IReadOnlyList<string> Columns, IReadOnlyList<IReadOnlyList<string>> Rows);

// A form as it prints: its code and title, and the layout with tokens filled in.
public sealed record PrintableForm(string Code, string Title, ElectionFormLayout Layout);

// 37.14s. Every form in ElectionContents prints at any time. Blank gives the org header, ruled
// lines and boxes to fill by hand. Filled adds the election facts, the stored records and a
// QR code that leads to the verify page.
public sealed class ElectionDocumentService(
    ApplicationDbContext db,
    IOrgConfigService orgConfigService,
    IElectionService? elections = null,
    ILogger<ElectionDocumentService>? logger = null,
    IFileStorageService? storage = null,
    IWebHostEnvironment? environment = null) : IElectionDocumentService
{
    private readonly IElectionService _elections = elections ?? new ElectionService(db);

    // A text item that ends "(3 ruled lines)" prints the label and that many lines to write on,
    // as the :: lines block does on the web page. A bare "3 lines" prints only the lines.
    private static readonly Regex RuledLinesHint = new(@"^(?:(?<label>.*?)\s*\((?<count>\d+) ruled lines?\)|(?<count>\d+) lines?)\s*$", RegexOptions.Compiled);
    private const int DefaultBlankRows = 5;
    private const int MaxRuledLines = 10;

    private void RegisterFonts()
    {
        if (ElectionPdfFonts.EnsureRegistered() is { } error)
            logger?.LogWarning(error, "Noto Sans Bengali could not be registered; Bengali letters in election PDFs print as boxes.");
    }

    // The web route in app.routes.ts (verify/election/:id/:code) must match, or every printed QR code breaks.
    public static string VerificationPath(int electionId, string formCode) => $"/verify/election/{electionId}/{formCode}";

    public async Task<ElectionDocumentVerifyDto?> VerifyAsync(int electionId, string formCode, CancellationToken cancellationToken = default)
    {
        // A form unpublished after it was printed still verifies.
        var form = await ElectionContentService.FindAsync(db, ElectionContentKind.Form, formCode, publishedOnly: false, cancellationToken);
        if (form is null)
            return null;
        // FR-047. A rehearsal's forms are not public, so its QR code reads as unknown.
        var election = await db.Elections.AsNoTracking().Where(x => x.Id == electionId && !x.IsTest)
            .Select(x => new { x.Id, x.Title, x.Phase }).SingleOrDefaultAsync(cancellationToken);
        return election is null
            ? null
            : new ElectionDocumentVerifyDto($"E-{election.Id:D6}", election.Id, election.Title, election.Phase, form.Key, form.Title);
    }

    // The published form with its tokens filled, or null. Public so a test can read the layout without opening the PDF.
    public async Task<PrintableForm?> LoadFormAsync(string formCode, CancellationToken ct = default)
    {
        var row = await ElectionContentService.FindAsync(db, ElectionContentKind.Form, formCode, publishedOnly: true, ct);
        if (row is null)
            return null;
        var tokens = await ElectionContentTokens.LoadAsync(db, await orgConfigService.GetConfigAsync(), ct);
        ElectionFormLayout? layout;
        try
        {
            layout = JsonSerializer.Deserialize<ElectionFormLayout>(tokens.ResolveLayout(row.Body), ElectionContentTokens.LayoutJson);
        }
        catch (JsonException ex)
        {
            // Only an admin edit is checked, so a hand-edited row can still hold a broken layout.
            logger?.LogWarning(ex, "Election form {Code} has a layout that does not parse.", row.Key);
            return null;
        }
        return layout is null ? null : new PrintableForm(row.Key, row.Title, layout);
    }

    public async Task<byte[]?> GenerateBlankPdfAsync(string formCode, CancellationToken cancellationToken = default)
    {
        var form = await LoadFormAsync(formCode, cancellationToken);
        return form is null ? null : await RenderAsync(form, filled: null, cancellationToken);
    }

    public async Task<byte[]?> GeneratePdfAsync(
        int electionId,
        string formCode,
        ElectionPrintMode? mode = null,
        CancellationToken cancellationToken = default)
    {
        var form = await LoadFormAsync(formCode, cancellationToken);
        if (form is null)
            return null;

        var election = await db.Elections.AsNoTracking()
            .Include(x => x.Seats).ThenInclude(s => s.EcPosition)
            .Include(x => x.ECPeriod)
            .SingleOrDefaultAsync(x => x.Id == electionId, cancellationToken);
        if (election is null)
            return null;

        var org = await orgConfigService.GetConfigAsync();
        if ((mode ?? org.Elections.DefaultPrintMode) == ElectionPrintMode.Blank)
            return await RenderAsync(form, filled: null, cancellationToken);

        var rollCount = await db.VoterRolls.CountAsync(v => v.ElectionId == electionId, cancellationToken);
        var tables = await LoadTablesAsync(election, form.Code, cancellationToken);
        var fields = await LoadFieldValuesAsync(election, form.Code, cancellationToken);
        return await RenderAsync(form, new FilledRecord(election, rollCount, tables, fields), cancellationToken);
    }

    private sealed record FilledRecord(
        Election Election,
        int RollCount,
        IReadOnlyList<FormTable> Tables,
        IReadOnlyDictionary<string, string> Fields);

    private async Task<byte[]> RenderAsync(PrintableForm form, FilledRecord? filled, CancellationToken ct)
    {
        var org = await orgConfigService.GetConfigAsync();
        var logo = await LoadLogoAsync(org.Branding.LogoUrl, ct);
        RegisterFonts();

        string? verificationUrl = null;
        byte[]? qrBytes = null;
        if (filled is not null)
        {
            verificationUrl = org.Contact.PortalBaseUrl.TrimEnd('/') + VerificationPath(filled.Election.Id, form.Code);
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(verificationUrl, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            qrBytes = qrCode.GetGraphic(12);
        }

        var contactLine = string.Join(" | ", org.Contact.PhoneNumbers.Append(org.Contact.SupportEmail).Where(x => !string.IsNullOrWhiteSpace(x)));
        var layout = form.Layout;
        // Stored records take the place of the form's own empty tables.
        var skipLayoutTables = filled is { Tables.Count: > 0 };

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(42);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(ElectionPdfFonts.Primary, ElectionPdfFonts.Bengali));
                page.Header().Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        if (logo is not null)
                            row.ConstantItem(48).PaddingRight(10).Image(logo).FitArea();
                        row.RelativeItem().Column(orgBlock =>
                        {
                            orgBlock.Item().Text(org.Branding.FullName).Bold().FontSize(16).FontColor(org.Branding.PrimaryColor);
                            if (!string.IsNullOrWhiteSpace(org.Contact.CampusAddress))
                                orgBlock.Item().Text(org.Contact.CampusAddress).FontSize(8);
                            if (contactLine.Length > 0)
                                orgBlock.Item().Text(contactLine).FontSize(8);
                        });
                    });
                    column.Item().PaddingTop(4).Text($"{form.Code} | {form.Title}").FontSize(9).FontColor(org.Branding.AccentColor);
                });
                page.Content().PaddingTop(16).Column(column =>
                {
                    column.Item().AlignCenter().Text(form.Title).Bold().FontSize(18);
                    if (filled is null)
                        AddFieldGrid(column, layout.Header);
                    else
                        AddFacts(column, filled.Election, filled.RollCount);

                    foreach (var section in layout.Sections)
                    {
                        if (skipLayoutTables && section.Kind == "table")
                            continue;
                        AddSection(column, section, filled?.Fields);
                    }

                    foreach (var table in filled?.Tables ?? [])
                        AddTable(column, table);

                    AddSignatures(column, layout.Signatures, layout.Seal);
                });
                page.Footer().Row(row =>
                {
                    if (verificationUrl is not null && qrBytes is not null)
                    {
                        row.RelativeItem().Text($"Verification: {verificationUrl}").FontSize(7);
                        row.ConstantItem(55).Image(qrBytes);
                    }
                    else
                    {
                        row.RelativeItem();
                    }
                    row.ConstantItem(45).AlignRight().Text(text =>
                    {
                        text.Span("Page ").FontSize(7);
                        text.CurrentPageNumber().FontSize(7);
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    // The logo prints only when it is a PNG or JPEG the server can read. An absolute URL or an
    // SVG is left out rather than fetched or converted.
    private async Task<byte[]?> LoadLogoAsync(string? logoUrl, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(logoUrl)
            || Uri.TryCreate(logoUrl, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https")
            return null;
        var bytes = await StoredFileReader.ReadAsync(storage, environment?.WebRootPath, logoUrl, ct);
        return bytes is not null && IsPngOrJpeg(bytes) ? bytes : null;
    }

    public static bool IsPngOrJpeg(byte[] bytes) =>
        bytes.Length > 4 && (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47
                             || bytes[0] == 0xFF && bytes[1] == 0xD8);

    private static void AddFacts(ColumnDescriptor column, Election election, int rollCount)
    {
        column.Item().PaddingTop(14).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(155);
                columns.RelativeColumn();
            });
            AddRow(table, "Election reference", $"E-{election.Id:D6}");
            AddRow(table, "Election title", election.Title);
            AddRow(table, "Phase", election.Phase.ToString());
            if (!string.IsNullOrWhiteSpace(election.NoticeReference))
                AddRow(table, "Notice reference", election.NoticeReference);
            AddRow(table, "Nomination window", Window(election.NominationOpensOn, election.NominationClosesOn));
            AddRow(table, "Polling window", Window(election.PollingOpensOn, election.PollingClosesOn));
            AddRow(table, "Voter-roll status", rollCount == 0 ? "Not frozen" : $"Frozen; {rollCount} records");
            AddRow(table, "Seat count", election.Seats.Count.ToString());
            AddRow(table, "Generated at", DateTime.UtcNow.ToString("O"));
        });
    }

    // Header fields, two to a row, each a label beside a line to write on.
    private static void AddFieldGrid(ColumnDescriptor column, IReadOnlyList<string> labels)
    {
        if (labels.Count == 0)
            return;
        column.Item().PaddingTop(10).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
            });
            foreach (var label in labels)
                table.Cell().PaddingRight(12).PaddingTop(8).Element(cell => RuledField(cell, label));
            if (labels.Count % 2 == 1)
                table.Cell();
        });
    }

    // A stored value prints on the line; without one the line stays blank for hand.
    private static void RuledField(IContainer container, string label, string? value = null) =>
        container.Row(row =>
        {
            row.AutoItem().PaddingRight(6).AlignBottom().Text(label + ":").FontSize(9);
            row.RelativeItem().MinHeight(16).BorderBottom(0.75f).BorderColor(Colors.Grey.Darken1)
                .AlignBottom().Text(value ?? string.Empty).FontSize(9);
        });

    private static void RuledLines(ColumnDescriptor column, int count)
    {
        for (var i = 0; i < count; i++)
            column.Item().Height(20).BorderBottom(0.75f).BorderColor(Colors.Grey.Darken1);
    }

    private static void AddSection(ColumnDescriptor column, ElectionFormSection section, IReadOnlyDictionary<string, string>? values)
    {
        if (!string.IsNullOrWhiteSpace(section.Name))
            column.Item().PaddingTop(14).Text(section.Name).Bold();
        switch (section.Kind)
        {
            case "fields":
                foreach (var item in section.Items)
                    column.Item().PaddingTop(8).Element(cell => RuledField(cell, item, values?.GetValueOrDefault(item)));
                break;
            case "checklist":
                foreach (var item in section.Items)
                    column.Item().PaddingTop(6).Row(row =>
                    {
                        row.ConstantItem(11).Height(11).Border(0.75f).BorderColor(Colors.Grey.Darken2);
                        row.RelativeItem().PaddingLeft(8).Text(item);
                    });
                break;
            case "declaration":
                foreach (var item in section.Items)
                    column.Item().PaddingTop(6).Text(item).Italic();
                break;
            case "table":
                AddBlankTable(column, section);
                break;
            default:
                foreach (var item in section.Items)
                {
                    if (ParseLinesHint(item) is not { } hint)
                    {
                        column.Item().PaddingTop(6).Text(item);
                        continue;
                    }
                    if (hint.Label.Length > 0)
                        column.Item().PaddingTop(8).Text(hint.Label);
                    RuledLines(column, hint.Lines);
                }
                break;
        }
    }

    // Public for tests. The label and line count of a lines hint, or null for plain text.
    public static (string Label, int Lines)? ParseLinesHint(string item) =>
        RuledLinesHint.Match(item) is { Success: true } hint
            ? (hint.Groups["label"].Value, Math.Clamp(int.Parse(hint.Groups["count"].Value), 1, MaxRuledLines))
            : null;

    // Column headings, the preset first-column labels, then empty rows to fill by hand.
    private static void AddBlankTable(ColumnDescriptor column, ElectionFormSection section)
    {
        var rows = section.Rows ?? [];
        var blank = section.Blank ?? (rows.Count == 0 ? DefaultBlankRows : 0);
        column.Item().PaddingTop(6).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                foreach (var _ in section.Items) columns.RelativeColumn();
            });
            foreach (var heading in section.Items)
                table.Cell().Background(Colors.Grey.Lighten3).Border(0.75f).BorderColor(Colors.Grey.Darken1).Padding(4).Text(heading).Bold().FontSize(9);
            foreach (var label in rows.Concat(Enumerable.Repeat(string.Empty, blank)))
                for (var i = 0; i < section.Items.Count; i++)
                    table.Cell().Border(0.75f).BorderColor(Colors.Grey.Darken1).MinHeight(20).Padding(4).Text(i == 0 ? label : string.Empty).FontSize(9);
        });
    }

    // Three signature blocks to a row, each a line with its label under it, then the seal box.
    private static void AddSignatures(ColumnDescriptor column, IReadOnlyList<string> signatures, bool seal)
    {
        if (signatures.Count > 0)
        {
            column.Item().PaddingTop(18).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });
                foreach (var label in signatures)
                    table.Cell().PaddingRight(14).PaddingTop(30).Column(block =>
                    {
                        block.Item().BorderTop(0.75f).BorderColor(Colors.Grey.Darken2).PaddingTop(3).Text(label).FontSize(8);
                        block.Item().PaddingTop(10).Element(cell => RuledField(cell, "Date"));
                    });
                for (var i = signatures.Count % 3; i is > 0 and < 3; i++)
                    table.Cell();
            });
        }
        if (seal)
            column.Item().PaddingTop(18).AlignRight().Width(90).Height(90).Border(0.75f).BorderColor(Colors.Grey.Darken1)
                .AlignCenter().AlignMiddle().Text("Seal").FontSize(8).FontColor(Colors.Grey.Darken1);
    }

    public async Task<byte[]?> GenerateBallotReceiptPdfAsync(
        int electionId,
        string trackingCode,
        DateTime castAtUtc,
        CancellationToken cancellationToken = default)
    {
        var title = await db.Elections.Where(x => x.Id == electionId).Select(x => x.Title).SingleOrDefaultAsync(cancellationToken);
        if (title is null)
            return null;
        var org = await orgConfigService.GetConfigAsync();
        var castAt = AssociationTime.FromUtc(castAtUtc);
        RegisterFonts();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(ElectionPdfFonts.Primary, ElectionPdfFonts.Bengali));
                page.Header().Text(org.Branding.FullName).Bold().FontSize(14).FontColor(org.Branding.PrimaryColor);
                page.Content().PaddingTop(18).Column(column =>
                {
                    column.Item().Text("Voting receipt").Bold().FontSize(18);
                    column.Item().PaddingTop(4).Text(title).FontSize(12);
                    column.Item().PaddingTop(18).Text("Tracking code").FontSize(9).FontColor(Colors.Grey.Darken2);
                    column.Item().Text(trackingCode).Bold().FontSize(20);
                    column.Item().PaddingTop(12).Text("Cast at").FontSize(9).FontColor(Colors.Grey.Darken2);
                    column.Item().Text($"{castAt:yyyy-MM-dd HH:mm} (Bangladesh time)");
                    column.Item().PaddingTop(24).Text("The tracking code shows the ballot was counted. It is published without choices, and this receipt does not show them either.")
                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void AddTable(ColumnDescriptor column, FormTable data)
    {
        column.Item().PaddingTop(18).Text(data.Heading).Bold();
        if (data.Rows.Count == 0)
        {
            column.Item().PaddingTop(4).Text("No records.");
            return;
        }
        column.Item().PaddingTop(4).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                foreach (var _ in data.Columns) columns.RelativeColumn();
            });
            foreach (var heading in data.Columns)
                table.Cell().Background(Colors.Grey.Lighten3).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(4).Text(heading).Bold().FontSize(9);
            foreach (var row in data.Rows)
                foreach (var cell in row)
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(4).Text(cell).FontSize(9);
        });
    }

    // Which stored records each form shows. Public so a test can read the rows without opening the PDF.
    public async Task<IReadOnlyList<FormTable>> LoadTablesAsync(int electionId, string formCode, CancellationToken ct = default)
    {
        var election = await db.Elections.AsNoTracking().Include(x => x.Seats).ThenInclude(s => s.EcPosition).SingleOrDefaultAsync(x => x.Id == electionId, ct);
        return election == null ? [] : await LoadTablesAsync(election, formCode.Trim().ToUpperInvariant(), ct);
    }

    private async Task<IReadOnlyList<FormTable>> LoadTablesAsync(Election election, string code, CancellationToken ct)
    {
        switch (code)
        {
            case "ER-10" or "ER-11" or "ER-18" or "ER-19":
                return [await NominationsTableAsync(election, code, ct)];
            case "ER-27":
                return await CountTablesAsync(election, ct);
            case "ER-27C" or "ER-29" or "ER-30":
                return await DeclarationTablesAsync(election, code, ct);
            case "ER-25":
                // 37.14z. Same rule as ER-04 and ER-05, not the public board, so a copy printed
                // after access ends still lists who served.
                var officials = await ServingAppointmentsAsync(election.Id, null, ct);
                return [await PollRecordTableAsync(election, ct), new FormTable("Appointed officials",
                    ["Group", "Role", "Name", "Accepted"],
                    officials.Select(a => (IReadOnlyList<string>)[a.Persona?.GroupName ?? string.Empty, a.Persona?.Name ?? string.Empty, a.DisplayName, DateOrNotYet(a.AcceptedAt)]).ToArray())];
            case "ER-04" or "ER-05":
                return [await AppointmentsTableAsync(election, code, ct)];
            case "ER-23" or "ER-24":
                return [await PollOpenCloseTableAsync(election, code, ct)];
            case "ER-17":
                return [await ScrutinyTableAsync(election, ct)];
            case "ER-28" or "ER-28A" or "ER-28B":
                return await RecountTablesAsync(election, code, ct);
            case "ER-34":
                return [await HandoverTableAsync(election, ct)];
            default:
                return [];
        }
    }

    // Public so a test can read the field values without opening the PDF.
    public async Task<IReadOnlyDictionary<string, string>> LoadFieldValuesAsync(int electionId, string formCode, CancellationToken ct = default)
    {
        var election = await db.Elections.AsNoTracking().SingleOrDefaultAsync(x => x.Id == electionId, ct);
        return election == null
            ? new Dictionary<string, string>()
            : await LoadFieldValuesAsync(election, formCode.Trim().ToUpperInvariant(), ct);
    }

    // 37.14q. A ruled field whose label matches one here prints the stored value. Labels are the
    // ones in forms.json, so a renamed label falls back to a blank line rather than failing. A
    // field with nothing stored stays blank for hand, and labels about one person (Name,
    // Membership No.) are never filled, since one form serves many people.
    private async Task<IReadOnlyDictionary<string, string>> LoadFieldValuesAsync(Election e, string code, CancellationToken ct)
    {
        var values = new Dictionary<string, string>
        {
            ["Election Date"] = Day(e.PollingOpensOn, ""),
            ["Polling Time"] = Known(e.PollingOpensOn) is null || Known(e.PollingClosesOn) is null
                ? "" : $"{e.PollingOpensOn:HH:mm} to {e.PollingClosesOn:HH:mm} UTC",
            ["Polling Venue"] = TextOr(e.Venue, ""),
            ["Nomination Opens"] = Stamp(e.NominationOpensOn, ""),
            ["Nomination Closes"] = Stamp(e.NominationClosesOn, ""),
            ["Scrutiny"] = Stamp(e.ScrutinyOn, ""),
            ["Withdrawal Deadline"] = Stamp(e.WithdrawalClosesOn, ""),
            ["Final Candidate List"] = Stamp(e.FinalListOn, ""),
            ["Campaign Period"] = Known(e.CampaignOpensOn) is null && Known(e.CampaignClosesOn) is null
                ? "" : $"{Stamp(e.CampaignOpensOn)} to {Stamp(e.CampaignClosesOn)}",
            // Only the start of the silence is stored; it runs until polling opens.
            ["Silence Period"] = Known(e.SilenceStartsAt) is null ? "" : $"From {Stamp(e.SilenceStartsAt)}",
            ["Campaign Opens"] = Stamp(e.CampaignOpensOn, ""),
            ["Campaign Closes"] = Stamp(e.CampaignClosesOn, ""),
        };

        switch (code)
        {
            case "ER-08":
                var total = await db.VoterRolls.CountAsync(v => v.ElectionId == e.Id, ct);
                if (total > 0)
                {
                    var eligible = await db.VoterRolls.CountAsync(v => v.ElectionId == e.Id && v.IsEligible, ct);
                    values["Total Members"] = total.ToString();
                    values["Eligible Voters"] = eligible.ToString();
                    values["Ineligible Members"] = (total - eligible).ToString();
                }
                break;
            // ER-29 said "Rejected Ballots" before the 37.16a rename; older layouts still do.
            case "ER-24" or "ER-29":
                var cast = await db.Ballots.CountAsync(b => b.ElectionId == e.Id, ct);
                var spoiled = await db.Ballots.CountAsync(b => b.ElectionId == e.Id && b.IsSpoiled, ct);
                values["Registered Voters"] = (await db.VoterRolls.CountAsync(v => v.ElectionId == e.Id && v.IsEligible, ct)).ToString();
                values["Votes Cast"] = (cast - spoiled).ToString();
                values["Spoiled Ballots"] = values["Rejected Ballots"] = spoiled.ToString();
                break;
            case "ER-26":
                values["Electronic Votes Recorded"] = (await db.Ballots.CountAsync(b => b.ElectionId == e.Id, ct)).ToString();
                values["Audit Log Entries"] = (await new ElectionAuditWriter(db).VerifyAsync(e.Id, ct)).Entries.ToString();
                values["Audit Chain Head Hash"] = (await _elections.GetBoardAsync(e.Id, ct))?.AuditHeadHash ?? "";
                break;
            // Owner rules 2026-10-23: total votes leave the spoiled ballots out, and the line is
            // "Spoiled Ballots" (TODO 37.16a). A database seeded earlier keeps the old "Rejected Ballots"
            // label until an admin edits the layout, so that label gets the same count.
            case "ER-27" or "ER-27C":
                var ballots = await db.Ballots.CountAsync(b => b.ElectionId == e.Id, ct);
                var spoiledBallots = await db.Ballots.CountAsync(b => b.ElectionId == e.Id && b.IsSpoiled, ct);
                values["Spoiled Ballots"] = values["Rejected Ballots"] = spoiledBallots.ToString();
                if (code == "ER-27C")
                {
                    values["Total Votes"] = (ballots - spoiledBallots).ToString();
                    await AddWinnerLinesAsync(e.Id, values, ct);
                }
                break;
        }

        return values.Where(kv => kv.Value.Length > 0).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    // ER-27C has one Winner and one Runner-up line. They are only filled for a single-seat election;
    // with more seats the lines point to the result table instead. Tied names share a line. The seat
    // count comes from the election, not the results, so a seat with no results yet still counts.
    private async Task AddWinnerLinesAsync(int electionId, Dictionary<string, string> values, CancellationToken ct)
    {
        if (await db.ElectionSeats.CountAsync(s => s.ElectionId == electionId, ct) > 1)
        {
            values["Winner"] = values["Runner-up"] = "See result per seat";
            return;
        }
        var results = await db.ElectionResults.AsNoTracking().Where(r => r.ElectionId == electionId).ToListAsync(ct);
        if (results.Count == 0)
            return;
        var names = await CandidateNamesAsync(electionId, ct);
        string Names(IEnumerable<ElectionResult> rows) =>
            string.Join(", ", rows.Select(r => names.GetValueOrDefault(r.NominationId) ?? string.Empty).Order());
        values["Winner"] = Names(results.Where(r => r.IsElected));
        var others = results.Where(r => !r.IsElected).ToList();
        if (others.Count > 0)
        {
            var top = others.Max(o => o.VoteCount);
            values["Runner-up"] = Names(others.Where(r => r.VoteCount == top));
        }
    }

    // 37.14g. What the platform itself can certify about the poll. The seals, queue and
    // supervision lines on the paper form are still ticked by hand.
    private async Task<FormTable> PollRecordTableAsync(Election election, CancellationToken ct)
    {
        var eligible = await db.VoterRolls.CountAsync(x => x.ElectionId == election.Id && x.IsEligible, ct);
        var voted = await db.VoterRolls.CountAsync(x => x.ElectionId == election.Id && x.VotedAt != null, ct);
        var chain = await new ElectionAuditWriter(db).VerifyAsync(election.Id, ct);
        return new FormTable("Poll record", ["Item", "Recorded"],
        [
            ["Polling opened", Stamp(election.PollingOpensOn)],
            ["Polling closed", Stamp(election.PollingClosesOn)],
            ["Eligible voters", eligible.ToString()],
            ["Voters who cast a ballot", voted.ToString()],
            ["Ballot seal key fingerprint", election.BallotKeyFingerprint ?? "No key registered"],
            ["Audit trail", chain.Ok ? $"Intact, {chain.Entries} entries" : $"Broken at entry {chain.FirstBrokenSeq} of {chain.Entries}"],
        ]);
    }

    // 37.14p. ER-04 lists the Election Commission, ER-05 every appointee, since officials sign the
    // neutrality declaration too. Revoked appointments are left out. So is one that expired before
    // it was accepted: it can no longer be accepted, so "Not yet" would be false. An accepted one
    // stays after its access ends, since that person served. Witness lines stay for hand.
    private async Task<FormTable> AppointmentsTableAsync(Election election, string code, CancellationToken ct)
    {
        var ordered = await ServingAppointmentsAsync(election.Id,
            code == "ER-04" ? Constants.Elections.PersonaGroups.ElectionCommission : null, ct);
        var memberIds = ordered.Where(a => a.MemberId != null).Select(a => a.MemberId!.Value).Distinct().ToList();
        var numbers = await db.Members.AsNoTracking().Where(m => memberIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, m => m.MembershipNumber, ct);
        string Number(ElectionAppointment a) =>
            a.MemberId is { } id ? numbers.GetValueOrDefault(id) ?? string.Empty : "Not a member";

        return code == "ER-04"
            ? new FormTable("Commissioner acceptance", ["Name", "Membership No.", "Position", "Appointed", "Accepted"],
                ordered.Select(a => (IReadOnlyList<string>)[a.DisplayName, Number(a), a.Persona?.Name ?? string.Empty, DateOrNotYet(a.AppointedAt), DateOrNotYet(a.AcceptedAt)]).ToArray())
            : new FormTable("Neutrality declarations", ["Name", "Membership No.", "Election Role", "Declaration signed"],
                ordered.Select(a => (IReadOnlyList<string>)[a.DisplayName, Number(a), a.Persona?.Name ?? string.Empty, DateOrNotYet(a.DeclarationSignedAt)]).ToArray());
    }

    private async Task<List<ElectionAppointment>> ServingAppointmentsAsync(int electionId, string? group, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var query = db.ElectionAppointments.AsNoTracking().Include(a => a.Persona)
            .Where(a => a.ElectionId == electionId && a.RevokedAt == null
                        && (a.AcceptedAt != null || a.ExpiresAt == null || a.ExpiresAt > now));
        if (group is not null)
            query = query.Where(a => a.Persona!.GroupName == group);
        var rows = await query.ToListAsync(ct);
        return rows.OrderBy(a => a.Persona?.GroupName).ThenBy(a => a.Persona?.Name).ThenBy(a => a.DisplayName).ToList();
    }

    // "MinuteBooks" reads "Minute books", which is also how forms.json words the checklist.
    private static string ItemLabel<T>(T item) where T : struct, Enum
    {
        var words = Regex.Replace(item.ToString(), "(?<=[a-z])(?=[A-Z])", " ");
        return words[..1] + words[1..].ToLowerInvariant();
    }

    // 37.14q. ER-17. One row per nomination. A tick never set counts as not ticked.
    private async Task<FormTable> ScrutinyTableAsync(Election election, CancellationToken ct)
    {
        var nominations = await db.Nominations.AsNoTracking().Include(n => n.ScrutinyChecks)
            .Where(n => n.ElectionId == election.Id).ToListAsync(ct);
        var ids = nominations.Select(n => n.Id).ToList();
        var reasons = (await db.ScrutinyDecisions.AsNoTracking().Where(d => ids.Contains(d.NominationId)).ToListAsync(ct))
            .GroupBy(d => d.NominationId).ToDictionary(g => g.Key, g => g.OrderBy(d => d.DecidedAt).ThenBy(d => d.Id).Last().Reason);
        var names = await CandidateNamesAsync(election.Id, ct);
        var seats = election.Seats.ToDictionary(s => s.Id, s => ElectionService.SeatTitle(s));
        var items = Enum.GetValues<ScrutinyCheckItem>();

        IReadOnlyList<string> Row(Nomination n)
        {
            var ticked = n.ScrutinyChecks.Where(c => c.IsChecked).Select(c => c.Item).ToHashSet();
            var missing = items.Where(i => !ticked.Contains(i)).Select(ItemLabel).ToList();
            var decision = n.Status switch
            {
                NominationStatus.Accepted => "Accepted",
                NominationStatus.Rejected => "Rejected",
                NominationStatus.Withdrawn => "Withdrawn",
                _ => NotYet
            };
            return [names.GetValueOrDefault(n.Id) ?? string.Empty, seats.GetValueOrDefault(n.ElectionSeatId) ?? string.Empty,
                $"{ticked.Count} of {items.Length}", missing.Count == 0 ? "None" : string.Join(", ", missing),
                decision, TextOr(reasons.GetValueOrDefault(n.Id), "")];
        }

        return new FormTable("Scrutiny", ["Candidate", "Position", "Ticked", "Not ticked", "Decision", "Reason"],
            nominations.OrderBy(n => seats.GetValueOrDefault(n.ElectionSeatId)).ThenBy(n => names.GetValueOrDefault(n.Id))
                .Select(Row).ToArray());
    }

    // 37.14q. ER-28 lists the requests, ER-28A the decisions, ER-28B one comparison per recount
    // carried out. The reference is the request's own id, which never changes.
    private async Task<IReadOnlyList<FormTable>> RecountTablesAsync(Election election, string code, CancellationToken ct)
    {
        var requests = await db.ElectionRecountRequests.AsNoTracking().Include(r => r.Figures)
            .Where(r => r.ElectionId == election.Id).OrderBy(r => r.ReceivedAt).ToListAsync(ct);
        var seats = election.Seats.ToDictionary(s => s.Id, s => ElectionService.SeatTitle(s));
        string Seat(ElectionRecountRequest r) => seats.GetValueOrDefault(r.ElectionSeatId) ?? string.Empty;
        static string Ref(ElectionRecountRequest r) => $"R-{r.Id:D4}";

        if (code == "ER-28")
            return [new FormTable("Recount requests", ["Reference", "Received", "Position", "Applicant", "Grounds", "Explanation"],
                requests.Select(r => (IReadOnlyList<string>)[Ref(r), Stamp(r.ReceivedAt), Seat(r), r.ApplicantName, r.Grounds, TextOr(r.Explanation, "")]).ToArray())];
        if (code == "ER-28A")
            return [new FormTable("Recount decisions", ["Reference", "Applicant", "Position", "Decision", "Reason", "Decided"],
                requests.Select(r => (IReadOnlyList<string>)[Ref(r), r.ApplicantName, Seat(r),
                    r.Allowed switch { true => "Approved", false => "Rejected", null => NotYet }, TextOr(r.DecisionReason, ""), Stamp(r.DecidedAt, NotYet)]).ToArray())];

        var recounted = requests.Where(r => r.Allowed == true && r.Figures.Count > 0).ToList();
        if (recounted.Count == 0)
            return [new FormTable("Recount comparison", ["Candidate", "Original Count", "Recount", "Difference"], [])];
        var names = await CandidateNamesAsync(election.Id, ct);
        var original = await db.ElectionResults.AsNoTracking().Where(x => x.ElectionId == election.Id)
            .ToDictionaryAsync(x => x.NominationId, x => x.VoteCount, ct);
        return recounted.Select(r => new FormTable($"{Ref(r)}, {Seat(r)}, recounted {Stamp(r.RecountedAt)}",
            ["Candidate", "Original Count", "Recount", "Difference"],
            r.Figures.OrderByDescending(f => f.VoteCount).ThenBy(f => names.GetValueOrDefault(f.NominationId)).Select(f =>
            {
                var before = original.GetValueOrDefault(f.NominationId);
                return (IReadOnlyList<string>)[names.GetValueOrDefault(f.NominationId) ?? string.Empty, before.ToString(),
                    f.VoteCount.ToString(), (f.VoteCount - before).ToString("+0;-0;0", System.Globalization.CultureInfo.InvariantCulture)];
            }).ToArray())).ToArray();
    }

    // 37.14q. ER-34. The ticks sit on the term the election fills, which is the term being handed
    // over to. Every item prints, so an item nobody has ticked shows as not handed over.
    private async Task<FormTable> HandoverTableAsync(Election election, CancellationToken ct)
    {
        var period = await db.ECPeriods.AsNoTracking().Where(p => p.Id == election.ECPeriodId).Select(p => p.Title).SingleOrDefaultAsync(ct);
        var checks = await db.ECPeriodHandoverChecks.AsNoTracking().Where(c => c.ECPeriodId == election.ECPeriodId)
            .ToDictionaryAsync(c => c.Item, ct);
        return new FormTable($"Handover to {TextOr(period)}", ["Item", "Handed over", "Recorded"],
            Enum.GetValues<HandoverCheckItem>().Select(i => checks.GetValueOrDefault(i) is { } c
                ? (IReadOnlyList<string>)[ItemLabel(i), c.IsChecked ? "Yes" : "No", Stamp(c.SetAt)]
                : [ItemLabel(i), "No", NotRecorded]).ToArray());
    }

    private const string NotRecorded = "Not recorded";
    private const string NotYet = "Not yet";

    // An unset DateTime is year 0001. Treat it as missing so it never reaches paper.
    private static DateTime? Known(DateTime? at) => at is { } value && value != default ? value : null;

    private static string Day(DateTime? at, string missing = NotRecorded) =>
        Known(at) is { } value ? $"{value:yyyy-MM-dd}" : missing;

    // Stored times are UTC, so the stamp says so.
    private static string Stamp(DateTime? at, string missing = NotRecorded) =>
        Known(at) is { } value ? $"{value:yyyy-MM-dd HH:mm} UTC" : missing;

    private static string Window(DateTime? opens, DateTime? closes) =>
        Known(opens) is null && Known(closes) is null ? NotRecorded : $"{Day(opens)} to {Day(closes)}";

    private static string TextOr(string? value, string missing = NotRecorded) =>
        string.IsNullOrWhiteSpace(value) ? missing : value;

    private static string DateOrNotYet(DateTime? at) => Day(at, NotYet);

    // 37.14p. Only what the platform stores. Seal numbers, ballot serials, observers, agents and
    // the ballot box checks have no record, so those lines stay for hand.
    private async Task<FormTable> PollOpenCloseTableAsync(Election election, string code, CancellationToken ct)
    {
        var eligible = await db.VoterRolls.CountAsync(x => x.ElectionId == election.Id && x.IsEligible, ct);
        var venue = TextOr(election.Venue);
        if (code == "ER-23")
        {
            return new FormTable("Poll opening record", ["Item", "Recorded"],
            [
                ["Polling opened", Stamp(election.PollingOpensOn)],
                ["Venue", venue],
                ["Registered voters", eligible.ToString()],
                ["Ballot seal key fingerprint", election.BallotKeyFingerprint ?? "No key registered"],
            ]);
        }
        var cast = await db.Ballots.CountAsync(b => b.ElectionId == election.Id, ct);
        var spoiled = await db.Ballots.CountAsync(b => b.ElectionId == election.Id && b.IsSpoiled, ct);
        // Printed before the close, the counts are a running total, so the form says so.
        var closes = Stamp(election.PollingClosesOn);
        var open = election.PollingClosesOn > DateTime.UtcNow;
        List<IReadOnlyList<string>> rows =
        [
            ["Polling closed", open ? $"{NotYet}; closes {closes}" : closes],
            ["Venue", venue],
            ["Registered voters", eligible.ToString()],
            ["Ballots cast, spoiled included", cast.ToString()],
            ["Spoiled ballots", spoiled.ToString()],
        ];
        if (open)
            rows.Add(["Counts", "So far; polling is still open"]);
        return new FormTable("Poll closing record", ["Item", "Recorded"], rows);
    }

    private async Task<FormTable> NominationsTableAsync(Election election, string code, CancellationToken ct)
    {
        var rows = await db.Nominations.AsNoTracking().Where(n => n.ElectionId == election.Id).ToListAsync(ct);
        rows = code switch
        {
            "ER-18" => rows.Where(n => n.Status == NominationStatus.Withdrawn).ToList(),
            "ER-19" => rows.Where(n => n.Status == NominationStatus.Accepted).ToList(),
            _ => rows
        };
        var ids = rows.SelectMany(n => new[] { n.CandidateMemberId, n.ProposerMemberId, n.SeconderMemberId }).Distinct().ToList();
        var names = await db.Members.AsNoTracking().Where(m => ids.Contains(m.Id)).ToDictionaryAsync(m => m.Id, m => m.FullName, ct);
        string Name(int id) => names.GetValueOrDefault(id) ?? string.Empty;
        var seats = election.Seats.ToDictionary(s => s.Id, s => ElectionService.SeatTitle(s));
        string Seat(Nomination n) => seats.GetValueOrDefault(n.ElectionSeatId) ?? string.Empty;
        var ordered = rows.OrderBy(Seat).ThenBy(n => n.BallotOrder ?? int.MaxValue).ThenBy(n => Name(n.CandidateMemberId)).ToList();

        return code switch
        {
            "ER-10" => new FormTable("Nominations", ["Candidate", "Seat", "Proposer", "Seconder", "Filed"],
                ordered.Select(n => (IReadOnlyList<string>)[Name(n.CandidateMemberId), Seat(n), Name(n.ProposerMemberId), Name(n.SeconderMemberId), Stamp(n.SubmittedAt)]).ToArray()),
            "ER-11" => new FormTable("Candidate consent", ["Candidate", "Seat", "Consent filed"],
                ordered.Select(n => (IReadOnlyList<string>)[Name(n.CandidateMemberId), Seat(n), Stamp(n.ConsentAt)]).ToArray()),
            "ER-18" => new FormTable("Withdrawn nominations", ["Candidate", "Seat", "Withdrawn"],
                ordered.Select(n => (IReadOnlyList<string>)[Name(n.CandidateMemberId), Seat(n), Stamp(n.WithdrawnAt)]).ToArray()),
            _ => new FormTable("Final candidate list", ["Seat", "Ballot order", "Candidate"],
                ordered.Select(n => (IReadOnlyList<string>)[Seat(n), n.BallotOrder?.ToString() ?? string.Empty, Name(n.CandidateMemberId)]).ToArray())
        };
    }

    // The counts as stored. Abstentions are valid ballots that named nobody for the seat.
    private async Task<IReadOnlyList<FormTable>> CountTablesAsync(Election election, CancellationToken ct)
    {
        var results = await db.ElectionResults.AsNoTracking().Where(r => r.ElectionId == election.Id).ToListAsync(ct);
        var cast = await db.Ballots.CountAsync(b => b.ElectionId == election.Id, ct);
        var spoiled = await db.Ballots.CountAsync(b => b.ElectionId == election.Id && b.IsSpoiled, ct);
        var tables = new List<FormTable>
        {
            new("Ballots", ["Cast", "Spoiled", "Valid"], [[cast.ToString(), spoiled.ToString(), (cast - spoiled).ToString()]])
        };
        if (results.Count == 0) return tables;

        var names = await CandidateNamesAsync(election.Id, ct);
        var perSeat = (await db.BallotVotes.Where(v => v.Ballot!.ElectionId == election.Id && !v.Ballot.IsSpoiled)
                .Select(v => new { v.ElectionSeatId, v.BallotId }).Distinct().ToListAsync(ct))
            .GroupBy(x => x.ElectionSeatId).ToDictionary(g => g.Key, g => g.Count());
        foreach (var seat in election.Seats.OrderBy(s => s.EcPosition?.DisplayOrder ?? s.Position).ThenBy(s => s.Id))
        {
            var rows = results.Where(r => r.ElectionSeatId == seat.Id).OrderByDescending(r => r.VoteCount)
                .ThenBy(r => names.GetValueOrDefault(r.NominationId))
                .Select(r => (IReadOnlyList<string>)[names.GetValueOrDefault(r.NominationId) ?? string.Empty, r.VoteCount.ToString()]).ToList();
            if (rows.Count == 0) continue;
            rows.Add(["Abstain", (cast - spoiled - perSeat.GetValueOrDefault(seat.Id)).ToString()]);
            tables.Add(new FormTable(ElectionService.SeatTitle(seat), ["Candidate", "Votes"], rows));
        }
        return tables;
    }

    // 37.14d. The Final Result Sheet (ER-27C) carries the same head hash as the public board, so
    // a printed sheet can be checked against the audit trail later.
    private async Task<IReadOnlyList<IReadOnlyList<string>>> AuditHeadRowAsync(int electionId, string code, CancellationToken ct)
    {
        if (code != "ER-27C") return [];
        var head = (await _elections.GetBoardAsync(electionId, ct))?.AuditHeadHash;
        return [["Audit chain head", head ?? "Not declared"]];
    }

    private async Task<IReadOnlyList<FormTable>> DeclarationTablesAsync(Election election, string code, CancellationToken ct)
    {
        var results = await db.ElectionResults.AsNoTracking().Where(r => r.ElectionId == election.Id).ToListAsync(ct);
        var names = await CandidateNamesAsync(election.Id, ct);
        var seats = election.Seats.ToDictionary(s => s.Id, s => ElectionService.SeatTitle(s));
        string Seat(ElectionResult r) => seats.GetValueOrDefault(r.ElectionSeatId) ?? string.Empty;
        return
        [
            new FormTable("Declaration", ["Item", "Value"],
            [
                ["Tie rule", election.TieRule.ToString()],
                ["Declared on", Day(election.DeclaredOn, "Not declared")],
                .. await AuditHeadRowAsync(election.Id, code, ct)
            ]),
            new FormTable("Result per seat", ["Seat", "Candidate", "Votes", "Outcome"],
                results.OrderBy(Seat).ThenByDescending(r => r.VoteCount)
                    .Select(r => (IReadOnlyList<string>)[Seat(r), names.GetValueOrDefault(r.NominationId) ?? string.Empty, r.VoteCount.ToString(), r.IsTie ? "Tie" : r.IsElected ? "Elected" : "Not elected"]).ToArray())
        ];
    }

    // A signed form must still name a candidate whose member record was archived later.
    private async Task<Dictionary<int, string>> CandidateNamesAsync(int electionId, CancellationToken ct) =>
        await (from n in db.Nominations.AsNoTracking()
               join m in db.Members.AsNoTracking().IgnoreQueryFilters() on n.CandidateMemberId equals m.Id
               where n.ElectionId == electionId
               select new { n.Id, m.FullName }).ToDictionaryAsync(x => x.Id, x => x.FullName, ct);

    private static void AddRow(TableDescriptor table, string label, string value)
    {
        table.Cell().Background(Colors.Grey.Lighten3).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Text(label).Bold();
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Text(value);
    }
}
