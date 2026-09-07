using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using System.Collections.Generic;
using System.Linq;

namespace GHCAA.Tests.Services;

[TestFixture]
public class FinancialLedgerServiceTests : TestBase
{
    private FinancialLedgerService _service = null!;

    [SetUp]
    public async Task Setup()
    {
        _service = new FinancialLedgerService(_context);

        // GetSummaryAsync now also reads PaymentHistories (46.5); clear the seed fixture's rows so
        // income totals are deterministic instead of drifting with whatever demo data ships.
        _context.PaymentHistories.RemoveRange(_context.PaymentHistories);
        await _context.SaveChangesAsync();
    }

    [Test]
    public async Task UpdateRecordAsync_ShouldUpdateAllFields()
    {
        var existing = new FinancialRecord
        {
            Year = 2023,
            RecordType = Enums.FinancialRecordType.Income,
            FinancialCategory = Enums.FinancialCategory.MembershipFee,
            Date = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Amount = 100,
            Description = "Old Description",
            Reference = "OLD-REF"
        };
        _context.FinancialRecords.Add(existing);
        await _context.SaveChangesAsync();

        var update = new FinancialRecord
        {
            Id = existing.Id,
            Year = 2024,
            RecordType = Enums.FinancialRecordType.Expense,
            FinancialCategory = Enums.FinancialCategory.Utilities,
            Date = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc),
            Amount = 750.50m,
            Description = "New Description",
            Reference = "NEW-REF"
        };

        var result = await _service.UpdateRecordAsync(update, adminId: 1);

        result.Should().NotBeNull();
        result!.Year.Should().Be(2024);
        result.RecordType.Should().Be(Enums.FinancialRecordType.Expense);
        result.FinancialCategory.Should().Be(Enums.FinancialCategory.Utilities);
        result.Date.Should().Be(new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc));
        result.Amount.Should().Be(750.50m);
        result.Description.Should().Be("New Description");
        result.Reference.Should().Be("NEW-REF");

        var saved = await _context.FinancialRecords.FindAsync(existing.Id);
        saved!.Year.Should().Be(2024);
        saved.RecordType.Should().Be(Enums.FinancialRecordType.Expense);
        saved.FinancialCategory.Should().Be(Enums.FinancialCategory.Utilities);
        saved.Date.Should().Be(new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc));
        saved.Amount.Should().Be(750.50m);
        saved.Description.Should().Be("New Description");
        saved.Reference.Should().Be("NEW-REF");
    }

    [Test]
    public async Task GetSummaryAsync_ShouldCalculateCorrectTotals()
    {
        _context.FinancialRecords.AddRange(new List<FinancialRecord>
        {
            new FinancialRecord { Year = 2024, RecordType = Enums.FinancialRecordType.Income, Amount = 1000, FinancialCategory = Enums.FinancialCategory.Donation, Date = DateTime.UtcNow, Description = "D1" },
            new FinancialRecord { Year = 2024, RecordType = Enums.FinancialRecordType.Income, Amount = 500, FinancialCategory = Enums.FinancialCategory.MembershipFee, Date = DateTime.UtcNow, Description = "D2" },
            new FinancialRecord { Year = 2024, RecordType = Enums.FinancialRecordType.Expense, Amount = 300, FinancialCategory = Enums.FinancialCategory.Utilities, Date = DateTime.UtcNow, Description = "E1" },
            new FinancialRecord { Year = 2023, RecordType = Enums.FinancialRecordType.Income, Amount = 2000, FinancialCategory = Enums.FinancialCategory.Donation, Date = DateTime.UtcNow, Description = "Old" }
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetSummaryAsync(2024);

        result.TotalIncome.Should().Be(1500);
        result.TotalExpense.Should().Be(300);
        result.NetBalance.Should().Be(1200);
    }

    [Test]
    public async Task GetSummaryAsync_IncludesCompletedMemberPayments_NotJustLedgerRecords()
    {
        // 46.5: membership/event fees never write a FinancialRecord, only a PaymentHistory row —
        // a year with real fee income but no manually-entered ledger rows must not report near-zero.
        _context.FinancialRecords.Add(new FinancialRecord
        {
            Year = 2024,
            RecordType = Enums.FinancialRecordType.Income,
            Amount = 500,
            FinancialCategory = Enums.FinancialCategory.Donation,
            Date = new DateTime(2024, 3, 1),
            Description = "Grant"
        });
        _context.PaymentHistories.AddRange(
            new PaymentHistory { TransactionId = "TXN-1", Amount = 1000, PaidAt = new DateTime(2024, 1, 15), Status = Enums.PaymentStatus.Completed, FinancialCategory = Enums.FinancialCategory.MembershipFee },
            new PaymentHistory { TransactionId = "TXN-2", Amount = 200, PaidAt = new DateTime(2024, 6, 1), Status = Enums.PaymentStatus.Completed, FinancialCategory = Enums.FinancialCategory.Event },
            new PaymentHistory { TransactionId = "TXN-3", Amount = 999, PaidAt = new DateTime(2024, 2, 1), Status = Enums.PaymentStatus.Pending, FinancialCategory = Enums.FinancialCategory.MembershipFee },
            new PaymentHistory { TransactionId = "TXN-4", Amount = 999, PaidAt = new DateTime(2023, 12, 1), Status = Enums.PaymentStatus.Completed, FinancialCategory = Enums.FinancialCategory.MembershipFee }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetSummaryAsync(2024);

        // 500 (ledger grant) + 1000 (completed membership fee) + 200 (completed event fee).
        // The Pending and prior-year payments must not count.
        result.TotalIncome.Should().Be(1700);
        result.Details.Should().Contain(d => d.Type == "Income (Payment History)" && d.FinancialCategory == "MembershipFee" && d.Total == 1000);
        result.Details.Should().Contain(d => d.Type == "Income (Payment History)" && d.FinancialCategory == "Event" && d.Total == 200);
    }
}
