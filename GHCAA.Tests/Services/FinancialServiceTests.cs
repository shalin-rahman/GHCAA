using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq;

namespace GHCAA.Tests.Services;

[TestFixture]
public class FinancialServiceTests : TestBase
{
    private FinancialService _service = null!;
    private Mock<ICommunicationService> _communicationMock = null!;
    private Mock<INotificationService> _notificationMock = null!;

    [SetUp]
    public void Setup()
    {
        _communicationMock = new Mock<ICommunicationService>();
        _notificationMock = new Mock<INotificationService>();
        _service = new FinancialService(_context, _communicationMock.Object, _notificationMock.Object);
    }

    [Test]
    public async Task RecordPaymentAsync_ShouldAddPaymentAndReturnDto()
    {
        var member = new Member { FullName = "Payer", Email = "fsp@e.com", NID = "FSP1", FatherName = "F", MotherName = "M", MobileNo = "FSP1", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", SubjectGroup = "S", ProfessionalSector = "I", Designation = "D" };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var dto = new CreatePaymentHistoryDto { MemberId = member.Id, Amount = 500, TransactionId = "TRX-FSP-100", PaidAt = DateTime.UtcNow, Notes = "Test Payment" };
        var result = await _service.RecordPaymentAsync(dto);

        result.Should().NotBeNull();
        result.TransactionId.Should().Be("TRX-FSP-100");
        result.Amount.Should().Be(500);

        var dbPayment = await _context.PaymentHistories.FirstOrDefaultAsync(p => p.TransactionId == "TRX-FSP-100");
        dbPayment.Should().NotBeNull();
        dbPayment!.MemberId.Should().Be(member.Id);
    }

    [Test]
    public async Task GetMemberPaymentHistoryAsync_ShouldReturnDtoList()
    {
        var member = new Member { FullName = "History User", Email = "fsh@e.com", NID = "FSH1", FatherName = "F", MotherName = "M", MobileNo = "FSH1", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", SubjectGroup = "S", ProfessionalSector = "I", Designation = "D" };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _context.PaymentHistories.Add(new PaymentHistory { MemberId = member.Id, TransactionId = "FSH-T1", Amount = 100, PaidAt = DateTime.UtcNow.AddDays(-1), Status = Enums.PaymentStatus.Completed });
        _context.PaymentHistories.Add(new PaymentHistory { MemberId = member.Id, TransactionId = "FSH-T2", Amount = 200, PaidAt = DateTime.UtcNow, Status = Enums.PaymentStatus.Pending });
        await _context.SaveChangesAsync();

        var result = await _service.GetMemberPaymentHistoryAsync(member.Id);

        result.Should().HaveCount(2);
        result.First().TransactionId.Should().Be("FSH-T2"); // Ordered by Date Descending
    }

    [Test]
    public async Task GenerateAnnualDuesAsync_ShouldCreateDuesForActiveMembers()
    {
        var member = new Member { FullName = "Active User", Status = Enums.MembershipStatus.Active, MembershipType = Enums.MembershipType.General, Email = "fsg@e.com", NID = "FSG1", FatherName = "F", MotherName = "M", MobileNo = "FSG1", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", SubjectGroup = "S", ProfessionalSector = "I", Designation = "D" };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        await _service.GenerateAnnualDuesAsync(2024);

        var due = await _context.MembershipDues.FirstOrDefaultAsync(d => d.MemberId == member.Id);
        due.Should().NotBeNull();
        due!.Amount.Should().Be(1000);
        due.Year.Should().Be(2024);
    }

    [Test]
    public async Task MarkDueAsPaidAsync_ShouldUpdateStatus()
    {
        var member = new Member { FullName = "M", Email = "fsm@e.com", NID = "FSM1", MobileNo = "FSM1", FatherName="F", MotherName="M", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", SubjectGroup="S", ProfessionalSector="I", Designation="D" };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var due = new MembershipDue { MemberId = member.Id, Year = 2025, Amount = 1000, DueDate = DateTime.UtcNow, IsPaid = false };
        _context.MembershipDues.Add(due);
        var payment = new PaymentHistory { MemberId = member.Id, Amount = 1000, TransactionId = "FSM-T123", Status = Enums.PaymentStatus.Completed, PaidAt = DateTime.UtcNow };
        _context.PaymentHistories.Add(payment);
        await _context.SaveChangesAsync();

        var result = await _service.MarkDueAsPaidAsync(due.Id, payment.Id);

        result.Should().BeTrue();
        var updated = await _context.MembershipDues.FindAsync(due.Id);
        updated!.IsPaid.Should().BeTrue();
        updated.PaymentHistoryId.Should().Be(payment.Id);
    }

    [Test]
    public async Task AddMembershipFeeConfig_ShouldCreateNewConfig()
    {
        var dto = new CreateMembershipFeeConfigDto { MembershipType = "General", Amount = 1500, EffectiveDate = DateTime.UtcNow.AddDays(1), Description = "New Fee" };
        var result = await _service.AddMembershipFeeConfigAsync(dto, 1);

        result.Should().NotBeNull();
        result.Amount.Should().Be(1500);

        var dbConfig = await _context.MembershipFeeConfigs.FindAsync(result.Id);
        dbConfig.Should().NotBeNull();
        dbConfig!.MembershipType.Should().Be(Enums.MembershipType.General);
    }

    [Test]
    public async Task GetApplicableMembershipFeeAsync_ShouldReturnCorrectFee()
    {
        // Seed adds General=1000 for effective 2023-01-01; add a 2025 future fee
        _context.MembershipFeeConfigs.Add(new MembershipFeeConfig { MembershipType = Enums.MembershipType.General, Amount = 2000, EffectiveDate = new DateTime(2025, 1, 1), Description = "Future Fee" });
        await _context.SaveChangesAsync();

        var fee2024 = await _service.GetApplicableMembershipFeeAsync(Enums.MembershipType.General, 2024);
        var fee2025 = await _service.GetApplicableMembershipFeeAsync(Enums.MembershipType.General, 2025);

        fee2024.Should().Be(1000);
        fee2025.Should().Be(2000);
    }
}
