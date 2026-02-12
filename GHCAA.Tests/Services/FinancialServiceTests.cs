using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GHCAA.Tests.Services;

[TestFixture]
public class FinancialServiceTests
{
    private ApplicationDbContext _context = null!;
    private Microsoft.Data.Sqlite.SqliteConnection _connection = null!;
    private FinancialService _service = null!;
    private Mock<ICommunicationService> _communicationMock = null!;

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

        _communicationMock = new Mock<ICommunicationService>();
        _service = new FinancialService(_context, _communicationMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
    }

    [Test]
    public async Task GenerateAnnualDuesAsync_ShouldCreateDuesForActiveMembers()
    {
        // Arrange
        var member = new Member 
        { 
            FullName = "Active User", 
            Status = Enums.MembershipStatus.Active,
            MembershipType = Enums.MembershipType.General,
            Email = "a@e.com", NID = "1", FatherName = "F", MotherName = "M", MobileNo = "01", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", SubjectGroup = "S", ProfessionalSector = "I", Designation = "D"
        };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        // Act
        await _service.GenerateAnnualDuesAsync(2024);

        // Assert
        var due = await _context.MembershipDues.FirstOrDefaultAsync(d => d.MemberId == member.Id);
        due.Should().NotBeNull();
        due!.Amount.Should().Be(1000);
        due.Year.Should().Be(2024);
    }

    [Test]
    public async Task MarkDueAsPaidAsync_ShouldUpdateStatus()
    {
        // Arrange
        var member = new Member { FullName = "M", Email = "m@e.com", NID = "1", MobileNo = "1", FatherName="F", MotherName="M", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", SubjectGroup="S", ProfessionalSector="I", Designation="D" };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var due = new MembershipDue { MemberId = member.Id, Year = 2024, Amount = 1000, DueDate = DateTime.UtcNow, IsPaid = false };
        _context.MembershipDues.Add(due);

        var payment = new PaymentHistory { MemberId = member.Id, Amount = 1000, TransactionId = "T123", Status = Enums.PaymentStatus.Completed, PaidAt = DateTime.UtcNow };
        _context.PaymentHistories.Add(payment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.MarkDueAsPaidAsync(due.Id, payment.Id);

        // Assert
        result.Should().BeTrue();
        var updated = await _context.MembershipDues.FindAsync(due.Id);
        updated!.IsPaid.Should().BeTrue();
        updated.PaymentHistoryId.Should().Be(payment.Id);
    }
}
