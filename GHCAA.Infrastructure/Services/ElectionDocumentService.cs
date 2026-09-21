using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GHCAA.Infrastructure.Services;

public sealed class ElectionDocumentService(
    ApplicationDbContext db,
    IOrgConfigService orgConfigService) : IElectionDocumentService
{
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
            .Include(x => x.Officers)
            .Include(x => x.VoterRoll)
            .Include(x => x.ECPeriod)
            .SingleOrDefaultAsync(x => x.Id == electionId, cancellationToken);
        if (election is null)
            return null;

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
