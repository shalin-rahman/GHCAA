using FluentAssertions;
using GHCAA.Application.DTOs;
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
    public async Task RecordPaymentAsync_ShouldAddPaymentAndReturnDto()
    {
        // Arrange
        var member = new Member 
        { 
            FullName = "Payer", Email = "p@e.com", NID = "1", FatherName = "F", MotherName = "M", MobileNo = "01", 
            PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", 
            EmergencyContactPhone = "0", SubjectGroup = "S", ProfessionalSector = "I", Designation = "D" 
        };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var dto = new CreatePaymentHistoryDto
        {
            MemberId = member.Id,
            Amount = 500,
            TransactionId = "TRX100",
            PaidAt = DateTime.UtcNow,
            Notes = "Test Payment"
        };

        // Act
        var result = await _service.RecordPaymentAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.TransactionId.Should().Be("TRX100");
        result.Amount.Should().Be(500);

        var dbPayment = await _context.PaymentHistories.FirstOrDefaultAsync(p => p.TransactionId == "TRX100");
        dbPayment.Should().NotBeNull();
        dbPayment!.MemberId.Should().Be(member.Id);
    }

    [Test]
    public async Task GetMemberPaymentHistoryAsync_ShouldReturnDtoList()
    {
        // Arrange
        var member = new Member 
        { 
            FullName = "History User", Email = "h@e.com", NID = "2", FatherName = "F", MotherName = "M", MobileNo = "02", 
            PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", 
            EmergencyContactPhone = "0", SubjectGroup = "S", ProfessionalSector = "I", Designation = "D" 
        };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _context.PaymentHistories.Add(new PaymentHistory { MemberId = member.Id, TransactionId = "T1", Amount = 100, PaidAt = DateTime.UtcNow.AddDays(-1), Status = Enums.PaymentStatus.Completed });
        _context.PaymentHistories.Add(new PaymentHistory { MemberId = member.Id, TransactionId = "T2", Amount = 200, PaidAt = DateTime.UtcNow, Status = Enums.PaymentStatus.Pending });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetMemberPaymentHistoryAsync(member.Id);

        // Assert
        result.Should().HaveCount(2);
        result.First().TransactionId.Should().Be("T2"); // Ordered by Date Descending
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

    [Test]
    public async Task AddMembershipFeeConfig_ShouldCreateNewConfig()
    {
        // Arrange
        var dto = new CreateMembershipFeeConfigDto
        {
            MembershipType = "General",
            Amount = 1500,
            EffectiveDate = DateTime.UtcNow.AddDays(1),
            Description = "New Fee"
        };

        // Act
        var result = await _service.AddMembershipFeeConfigAsync(dto, 1);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(1500);

        var dbConfig = await _context.MembershipFeeConfigs.FindAsync(result.Id);
        dbConfig.Should().NotBeNull();
        dbConfig!.MembershipType.Should().Be(Enums.MembershipType.General);
    }

    [Test]
    public async Task GetApplicableMembershipFeeAsync_ShouldReturnCorrectFee()
    {
        // Seeding is done by EnsureCreated, ensuring default General fee is 1000
        // Let's add a future fee
        _context.MembershipFeeConfigs.Add(new MembershipFeeConfig 
        { 
            MembershipType = Enums.MembershipType.General, 
            Amount = 2000, 
            EffectiveDate = new DateTime(2025, 1, 1), 
            Description = "Future Fee" 
        });
        await _context.SaveChangesAsync();

        // Act
        // Current fee (2024) should be default 1000 (from seed)
        var fee2024 = await _service.GetApplicableMembershipFeeAsync(Enums.MembershipType.General, 2024);
        
        // Future fee (2025) should be 2000
        var fee2025 = await _service.GetApplicableMembershipFeeAsync(Enums.MembershipType.General, 2025);

        // Assert
        fee2024.Should().Be(1000);
        fee2025.Should().Be(2000);
    }
}
