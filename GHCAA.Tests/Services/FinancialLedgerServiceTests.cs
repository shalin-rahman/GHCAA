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
