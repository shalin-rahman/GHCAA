using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services;

// A table printed under the summary of a form, built from stored records.
public sealed record FormTable(string Heading, IReadOnlyList<string> Columns, IReadOnlyList<IReadOnlyList<string>> Rows);

public sealed class ElectionDocumentService(
    ApplicationDbContext db,
    IOrgConfigService orgConfigService,
    IElectionService? elections = null,
    ILogger<ElectionDocumentService>? logger = null) : IElectionDocumentService
{
    private readonly IElectionService _elections = elections ?? new ElectionService(db);

    private void RegisterFonts()
    {
        if (ElectionPdfFonts.EnsureRegistered() is { } error)
            logger?.LogWarning(error, "Noto Sans Bengali could not be registered; Bengali letters in election PDFs print as boxes.");
    }

    // Forms the code cannot fill from stored records yet, so they print the generic line.
    // ER-17 needs the scrutiny rules, ER-28 and ER-28B need recount records, and ER-01, ER-02
    // and ER-34 need the regulation text. Tracked in docs/TODO.md.
    // Codes follow the 2026-10-20 form inventory (37.14n). The old platform codes were dropped
    // with no aliases, so an old code that has no new meaning now returns null.

    private static readonly HashSet<string> SupportedForms = new(StringComparer.OrdinalIgnoreCase)
    {
        "ER-01", "ER-02", "ER-08", "ER-10", "ER-11", "ER-17", "ER-18", "ER-19",
        "ER-25", "ER-27", "ER-27C", "ER-28", "ER-28B", "ER-29", "ER-30", "ER-34"
    };

    // The web route in app.routes.ts (verify/election/:id/:code) must match, or every printed QR code breaks.
    public static string VerificationPath(int electionId, string formCode) => $"/verify/election/{electionId}/{formCode}";

    public async Task<ElectionDocumentVerifyDto?> VerifyAsync(int electionId, string formCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = formCode.Trim().ToUpperInvariant();
        if (!SupportedForms.Contains(normalizedCode))
            return null;
        // FR-047. A rehearsal's forms are not public, so its QR code reads as unknown.
        var election = await db.Elections.AsNoTracking().Where(x => x.Id == electionId && !x.IsTest)
            .Select(x => new { x.Id, x.Title, x.Phase }).SingleOrDefaultAsync(cancellationToken);
        return election is null
            ? null
            : new ElectionDocumentVerifyDto($"E-{election.Id:D6}", election.Id, election.Title, election.Phase, normalizedCode, FormTitle(normalizedCode));
    }

    public async Task<byte[]?> GeneratePdfAsync(
        int electionId,
        string formCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = formCode.Trim().ToUpperInvariant();
        if (!SupportedForms.Contains(normalizedCode))
            return null;

        var election = await db.Elections
            .Include(x => x.Seats).ThenInclude(s => s.EcPosition)
            .Include(x => x.VoterRoll)
            .Include(x => x.ECPeriod)
            .SingleOrDefaultAsync(x => x.Id == electionId, cancellationToken);
        if (election is null)
            return null;

        var tables = await LoadTablesAsync(election, normalizedCode, cancellationToken);
        var org = await orgConfigService.GetConfigAsync();
        RegisterFonts();
        var generatedAt = DateTime.UtcNow;
        var verificationUrl = org.Contact.PortalBaseUrl.TrimEnd('/') + VerificationPath(election.Id, normalizedCode);
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(verificationUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        var qrBytes = qrCode.GetGraphic(12);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(42);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(ElectionPdfFonts.Primary, ElectionPdfFonts.Bengali));
                page.Header().Column(column =>
                {
                    column.Item().Text(org.Branding.FullName).Bold().FontSize(16).FontColor(org.Branding.PrimaryColor);
                    column.Item().Text($"{normalizedCode} | Official Election Record").FontSize(9).FontColor(org.Branding.AccentColor);
                });
                page.Content().PaddingTop(20).Column(column =>
                {
                    column.Item().AlignCenter().Text(FormTitle(normalizedCode)).Bold().FontSize(20);
                    column.Item().PaddingTop(16).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(155);
                            columns.RelativeColumn();
                        });
                        AddRow(table, "Election reference", $"E-{election.Id:D6}");
                        AddRow(table, "Election title", election.Title);
                        AddRow(table, "Phase", election.Phase.ToString());
                        AddRow(table, "Nomination window", $"{election.NominationOpensOn:yyyy-MM-dd} to {election.NominationClosesOn:yyyy-MM-dd}");
                        AddRow(table, "Polling window", $"{election.PollingOpensOn:yyyy-MM-dd} to {election.PollingClosesOn:yyyy-MM-dd}");
                        AddRow(table, "Voter-roll status", election.VoterRoll.Count == 0 ? "Not frozen" : $"Frozen; {election.VoterRoll.Count} records");
                        AddRow(table, "Seat count", election.Seats.Count.ToString());
                        AddRow(table, "Generated at", generatedAt.ToString("O"));
                    });
                    column.Item().PaddingTop(22).Text(FormSummary(normalizedCode, election.VoterRoll.Count, election.Seats.Count));
                    foreach (var table in tables)
                        AddTable(column, table);
                    column.Item().PaddingTop(24).Text("Signatory / evidence state").Bold();
                    column.Item().PaddingTop(5).Text("Generated from persisted election records. Officer signatures and uploaded evidence remain subject to the recorded workflow state.");
                });
                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text($"Verification: {verificationUrl}").FontSize(7);
                    row.ConstantItem(55).Image(qrBytes);
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
                var board = await _elections.GetBoardAsync(election.Id, ct);
                return [await PollRecordTableAsync(election, ct), new FormTable("Appointed officials",
                    ["Group", "Role", "Name", "Accepted"],
                    (board?.Groups ?? []).SelectMany(g => g.Members.Select(m => (IReadOnlyList<string>)[g.GroupName, m.PersonaName, m.DisplayName, $"{m.AcceptedAt:yyyy-MM-dd}"])).ToArray())];
            default:
                return [];
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
            ["Polling opened", $"{election.PollingOpensOn:yyyy-MM-dd HH:mm} UTC"],
            ["Polling closed", $"{election.PollingClosesOn:yyyy-MM-dd HH:mm} UTC"],
            ["Eligible voters", eligible.ToString()],
            ["Voters who cast a ballot", voted.ToString()],
            ["Ballot seal key fingerprint", election.BallotKeyFingerprint ?? "No key registered"],
            ["Audit trail", chain.Ok ? $"Intact, {chain.Entries} entries" : $"Broken at entry {chain.FirstBrokenSeq} of {chain.Entries}"],
        ]);
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
                ordered.Select(n => (IReadOnlyList<string>)[Name(n.CandidateMemberId), Seat(n), Name(n.ProposerMemberId), Name(n.SeconderMemberId), $"{n.SubmittedAt:yyyy-MM-dd HH:mm}"]).ToArray()),
            "ER-11" => new FormTable("Candidate consent", ["Candidate", "Seat", "Consent filed"],
                ordered.Select(n => (IReadOnlyList<string>)[Name(n.CandidateMemberId), Seat(n), n.ConsentAt is { } consent ? $"{consent:yyyy-MM-dd HH:mm}" : "Not recorded"]).ToArray()),
            "ER-18" => new FormTable("Withdrawn nominations", ["Candidate", "Seat", "Withdrawn"],
                ordered.Select(n => (IReadOnlyList<string>)[Name(n.CandidateMemberId), Seat(n), $"{n.WithdrawnAt:yyyy-MM-dd HH:mm}"]).ToArray()),
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
                ["Declared on", election.DeclaredOn is { } on ? $"{on:yyyy-MM-dd}" : "Not declared"],
                .. await AuditHeadRowAsync(election.Id, code, ct)
            ]),
            new FormTable("Result per seat", ["Seat", "Candidate", "Votes", "Outcome"],
                results.OrderBy(Seat).ThenByDescending(r => r.VoteCount)
                    .Select(r => (IReadOnlyList<string>)[Seat(r), names.GetValueOrDefault(r.NominationId) ?? string.Empty, r.VoteCount.ToString(), r.IsTie ? "Tie" : r.IsElected ? "Elected" : "Not elected"]).ToArray())
        ];
    }

    private async Task<Dictionary<int, string>> CandidateNamesAsync(int electionId, CancellationToken ct) =>
        await (from n in db.Nominations.AsNoTracking()
               join m in db.Members.AsNoTracking() on n.CandidateMemberId equals m.Id
               where n.ElectionId == electionId
               select new { n.Id, m.FullName }).ToDictionaryAsync(x => x.Id, x => x.FullName, ct);

    private static void AddRow(TableDescriptor table, string label, string value)
    {
        table.Cell().Background(Colors.Grey.Lighten3).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Text(label).Bold();
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Text(value);
    }

    private static string FormTitle(string formCode) => formCode switch
    {
        "ER-01" => "Election Notice",
        "ER-02" => "Election Calendar",
        "ER-08" => "Voter Roll Certification",
        "ER-10" => "Nomination Paper",
        "ER-11" => "Candidate Consent",
        "ER-17" => "Scrutiny Checklist",
        "ER-18" => "Withdrawal Record",
        "ER-19" => "Final Candidate List",
        "ER-25" => "Poll Integrity Certificate",
        "ER-27" => "Count Sheet",
        "ER-27C" => "Final Result Sheet",
        "ER-28" => "Recount Request",
        "ER-28B" => "Recount Report",
        "ER-29" => "Result Certification",
        "ER-30" => "Result Declaration",
        "ER-34" => "Handover Certificate",
        _ => "Election Record"
    };

    private static string FormSummary(string formCode, int voterCount, int seatCount) => formCode switch
    {
        "ER-08" => $"This certificate records the frozen voter-roll snapshot containing {voterCount} voter records.",
        "ER-25" => "The officials named below certify that polling has closed and the ballot box was kept sealed. The platform figures in the poll record are taken from the voter roll and the audit trail. Each ballot was sealed with the returning officer's key when it was cast and stays sealed until the count.",
        "ER-27" or "ER-27C" or "ER-28B" or "ER-29" or "ER-30" => "This record is generated from the persisted count and declaration workflow.",
        _ => $"This record covers {seatCount} configured election seat(s) and is generated from the persisted election record."
    };
}
