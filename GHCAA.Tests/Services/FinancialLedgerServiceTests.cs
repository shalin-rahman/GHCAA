using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GHCAA.Tests.Services;

[TestFixture]
public class FinancialLedgerServiceTests
{
    private ApplicationDbContext _context = null!;
    private Microsoft.Data.Sqlite.SqliteConnection _connection = null!;
    private FinancialLedgerService _service = null!;

    [SetUp]
    public void Setup()
    {
        _connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _service = new FinancialLedgerService(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
    }

    [Test]
    public async Task GetSummaryAsync_ShouldCalculateCorrectTotals()
    {
        // Arrange
        _context.FinancialRecords.AddRange(new List<FinancialRecord>
        {
            new FinancialRecord { Year = 2024, RecordType = Enums.FinancialRecordType.Income, Amount = 1000, Category = Enums.FinancialCategory.Donation, Date = DateTime.UtcNow, Description = "D1" },
            new FinancialRecord { Year = 2024, RecordType = Enums.FinancialRecordType.Income, Amount = 500, Category = Enums.FinancialCategory.MembershipFee, Date = DateTime.UtcNow, Description = "D2" },
            new FinancialRecord { Year = 2024, RecordType = Enums.FinancialRecordType.Expense, Amount = 300, Category = Enums.FinancialCategory.Utilities, Date = DateTime.UtcNow, Description = "E1" },
            new FinancialRecord { Year = 2023, RecordType = Enums.FinancialRecordType.Income, Amount = 2000, Category = Enums.FinancialCategory.Donation, Date = DateTime.UtcNow, Description = "Old" }
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetSummaryAsync(2024);

        // Assert
        result.TotalIncome.Should().Be(1500);
        result.TotalExpense.Should().Be(300);
        result.NetBalance.Should().Be(1200);
    }
}
