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
    public void Setup()
    {
        _service = new FinancialLedgerService(_context);
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

        var result = await _service.UpdateRecordAsync(update);

        result.Year.Should().Be(2024);
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
}
