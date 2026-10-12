using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using static GHCAA.Domain.Enums;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GHCAA.Infrastructure.Services;

// A table printed under the summary of a form, built from stored records.
public sealed record FormTable(string Heading, IReadOnlyList<string> Columns, IReadOnlyList<IReadOnlyList<string>> Rows);

public sealed class ElectionDocumentService(
    ApplicationDbContext db,
    IOrgConfigService orgConfigService,
    IElectionService? elections = null) : IElectionDocumentService
{
    private readonly IElectionService _elections = elections ?? new ElectionService(db);

    // Forms the code cannot fill from stored records yet, so they print the generic line.
    // ER-11 needs the scrutiny rules, ER-26 and ER-28 need recount records, and ER-01, ER-02
    // and ER-33 need the regulation text. Tracked in docs/TODO.md.

    private static readonly HashSet<string> SupportedForms = new(StringComparer.OrdinalIgnoreCase)
    {
        "ER-01", "ER-02", "ER-07", "ER-09", "ER-10", "ER-11", "ER-13", "ER-14",
        "ER-19", "ER-23", "ER-26", "ER-28", "ER-29", "ER-30", "ER-31", "ER-33"
    };

    public async Task<byte[]?> GeneratePdfAsync(
        int electionId,
        string formCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = formCode.Trim().ToUpperInvariant();
        if (!SupportedForms.Contains(normalizedCode))
            return null;

        var election = await db.Elections
            .Include(x => x.Seats)
            .Include(x => x.VoterRoll)
            .Include(x => x.ECPeriod)
            .SingleOrDefaultAsync(x => x.Id == electionId, cancellationToken);
        if (election is null)
            return null;

        var tables = await LoadTablesAsync(election, normalizedCode, cancellationToken);
        var org = await orgConfigService.GetConfigAsync();
        var generatedAt = DateTime.UtcNow;
        var verificationUrl = $"{org.Contact.PortalBaseUrl.TrimEnd('/')}/verify/election/{election.Id}/{normalizedCode}";
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
                page.DefaultTextStyle(x => x.FontSize(10));
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
        var election = await db.Elections.AsNoTracking().Include(x => x.Seats).SingleOrDefaultAsync(x => x.Id == electionId, ct);
        return election == null ? [] : await LoadTablesAsync(election, formCode.Trim().ToUpperInvariant(), ct);
    }

    private async Task<IReadOnlyList<FormTable>> LoadTablesAsync(Election election, string code, CancellationToken ct)
    {
        switch (code)
        {
            case "ER-09" or "ER-10" or "ER-13" or "ER-14":
                return [await NominationsTableAsync(election, code, ct)];
            case "ER-23":
                return await CountTablesAsync(election, ct);
            case "ER-29" or "ER-30" or "ER-31":
                return await DeclarationTablesAsync(election, ct);
            case "ER-19":
                var board = await _elections.GetBoardAsync(election.Id, ct);
                return [new FormTable("Appointed officials",
                    ["Group", "Role", "Name", "Accepted"],
                    (board?.Groups ?? []).SelectMany(g => g.Members.Select(m => (IReadOnlyList<string>)[g.GroupName, m.PersonaName, m.DisplayName, $"{m.AcceptedAt:yyyy-MM-dd}"])).ToArray())];
            default:
                return [];
        }
    }

    private async Task<FormTable> NominationsTableAsync(Election election, string code, CancellationToken ct)
    {
        var rows = await db.Nominations.AsNoTracking().Where(n => n.ElectionId == election.Id).ToListAsync(ct);
        rows = code switch
        {
            "ER-13" => rows.Where(n => n.Status == NominationStatus.Withdrawn).ToList(),
            "ER-14" => rows.Where(n => n.Status == NominationStatus.Accepted).ToList(),
            _ => rows
        };
        var ids = rows.SelectMany(n => new[] { n.CandidateMemberId, n.ProposerMemberId, n.SeconderMemberId }).Distinct().ToList();
        var names = await db.Members.AsNoTracking().Where(m => ids.Contains(m.Id)).ToDictionaryAsync(m => m.Id, m => m.FullName, ct);
        string Name(int id) => names.GetValueOrDefault(id) ?? string.Empty;
        var seats = election.Seats.ToDictionary(s => s.Id, s => ElectionService.SeatTitle(s.Position));
        string Seat(Nomination n) => seats.GetValueOrDefault(n.ElectionSeatId) ?? string.Empty;
        var ordered = rows.OrderBy(Seat).ThenBy(n => n.BallotOrder ?? int.MaxValue).ThenBy(n => Name(n.CandidateMemberId)).ToList();

        return code switch
        {
            "ER-09" => new FormTable("Nominations", ["Candidate", "Seat", "Proposer", "Seconder", "Filed"],
                ordered.Select(n => (IReadOnlyList<string>)[Name(n.CandidateMemberId), Seat(n), Name(n.ProposerMemberId), Name(n.SeconderMemberId), $"{n.SubmittedAt:yyyy-MM-dd HH:mm}"]).ToArray()),
            "ER-10" => new FormTable("Candidate consent", ["Candidate", "Seat", "Consent filed"],
                ordered.Select(n => (IReadOnlyList<string>)[Name(n.CandidateMemberId), Seat(n), $"{n.SubmittedAt:yyyy-MM-dd HH:mm}"]).ToArray()),
            "ER-13" => new FormTable("Withdrawn nominations", ["Candidate", "Seat", "Withdrawn"],
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
        foreach (var seat in election.Seats.OrderBy(s => s.Position).ThenBy(s => s.Id))
        {
            var rows = results.Where(r => r.ElectionSeatId == seat.Id).OrderByDescending(r => r.VoteCount)
                .ThenBy(r => names.GetValueOrDefault(r.NominationId))
                .Select(r => (IReadOnlyList<string>)[names.GetValueOrDefault(r.NominationId) ?? string.Empty, r.VoteCount.ToString()]).ToList();
            if (rows.Count == 0) continue;
            rows.Add(["Abstain", (cast - spoiled - perSeat.GetValueOrDefault(seat.Id)).ToString()]);
            tables.Add(new FormTable(ElectionService.SeatTitle(seat.Position), ["Candidate", "Votes"], rows));
        }
        return tables;
    }

    private async Task<IReadOnlyList<FormTable>> DeclarationTablesAsync(Election election, CancellationToken ct)
    {
        var results = await db.ElectionResults.AsNoTracking().Where(r => r.ElectionId == election.Id).ToListAsync(ct);
        var names = await CandidateNamesAsync(election.Id, ct);
        var seats = election.Seats.ToDictionary(s => s.Id, s => ElectionService.SeatTitle(s.Position));
        string Seat(ElectionResult r) => seats.GetValueOrDefault(r.ElectionSeatId) ?? string.Empty;
        return
        [
            new FormTable("Declaration", ["Item", "Value"],
            [
                ["Tie rule", election.TieRule.ToString()],
                ["Declared on", election.DeclaredOn is { } on ? $"{on:yyyy-MM-dd}" : "Not declared"]
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
        "ER-07" => "Voter Roll Certification",
        "ER-09" => "Nomination Paper",
        "ER-10" => "Candidate Consent",
        "ER-11" => "Scrutiny Checklist",
        "ER-13" => "Withdrawal Record",
        "ER-14" => "Final Candidate List",
        "ER-19" => "Poll Integrity Certificate",
        "ER-23" => "Count Sheet",
        "ER-26" => "Recount Request",
        "ER-28" => "Recount Report",
        "ER-29" => "Final Result Sheet",
        "ER-30" => "Result Certification",
        "ER-31" => "Result Declaration",
        "ER-33" => "Handover Certificate",
        _ => "Election Record"
    };

    private static string FormSummary(string formCode, int voterCount, int seatCount) => formCode switch
    {
        "ER-07" => $"This certificate records the frozen voter-roll snapshot containing {voterCount} voter records.",
        "ER-19" => "This certificate records the polling integrity state for the election.",
        "ER-23" or "ER-28" or "ER-29" or "ER-30" or "ER-31" => "This record is generated from the persisted count and declaration workflow.",
        _ => $"This record covers {seatCount} configured election seat(s) and is generated from the persisted election record."
    };
}
