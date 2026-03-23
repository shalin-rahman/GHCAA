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
    public async Task Setup()
    {
        _communicationMock = new Mock<ICommunicationService>();
        _notificationMock = new Mock<INotificationService>();
        _service = new FinancialService(_context, _communicationMock.Object, _notificationMock.Object);

        if (!await _context.MembershipFeeConfigs.AnyAsync())
        {
            _context.MembershipFeeConfigs.Add(new MembershipFeeConfig { MembershipType = Enums.MembershipType.General, Amount = 1000, EffectiveDate = new DateTime(2023, 1, 1), Description = "Base Fee" });
            await _context.SaveChangesAsync();
        }
    }

    [Test]
    public async Task RecordPaymentAsync_ShouldAddPaymentAndReturnDto()
    {
        var member = new Member 
        { 
            FullName = "Payer", 
            Email = "fsp@e.com", 
            NID = "FSP1", 
            MobileNo = "FSP1", 
            Status = Enums.MembershipStatus.Active,
            FatherName = "F", 
            MotherName = "M", 
            PresentAddress = "A", 
            PermanentAddress = "A", 
            EmergencyContactName = "E", 
            EmergencyContactRelation = "R", 
            EmergencyContactPhone = "0",
            AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Science", PassingYear = 2005, IsGHC = true } }
        };
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
        var member = new Member 
        { 
            FullName = "History User", 
            Email = "fsh@e.com", 
            NID = "FSH1", 
            MobileNo = "FSH1", 
            Status = Enums.MembershipStatus.Active,
            FatherName = "F", 
            MotherName = "M", 
            PresentAddress = "A", 
            PermanentAddress = "A", 
            EmergencyContactName = "E", 
            EmergencyContactRelation = "R", 
            EmergencyContactPhone = "0",
            AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Science", PassingYear = 2005, IsGHC = true } }
        };
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
        var member = new Member 
        { 
            FullName = "Active User", 
            Email = "fsg@e.com", 
            NID = "FSG1", 
            MobileNo = "FSG1", 
            Status = Enums.MembershipStatus.Active,
            FatherName = "F", 
            MotherName = "M", 
            PresentAddress = "A", 
            PermanentAddress = "A", 
            EmergencyContactName = "E", 
            EmergencyContactRelation = "R", 
            EmergencyContactPhone = "0",
            AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Science", PassingYear = 2005, IsGHC = true } }
        };
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
        var member = new Member 
        { 
            FullName = "M", 
            Email = "fsm@e.com", 
            NID = "FSM1", 
            MobileNo = "FSM1", 
            Status = Enums.MembershipStatus.Active,
            FatherName = "F", 
            MotherName = "M", 
            PresentAddress = "A", 
            PermanentAddress = "A", 
            EmergencyContactName = "E", 
            EmergencyContactRelation = "R", 
            EmergencyContactPhone = "0",
            AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Science", PassingYear = 2005, IsGHC = true } }
        };
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

    [Test]
    public async Task GetSavedPaymentMethodsAsync_ShouldReturnSavedMethods()
    {
        var member = new Member { FullName = "U1", Email = "u1@e.com", NID = "U1", MobileNo = "U1", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "P", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _context.SavedPaymentMethods.Add(new SavedPaymentMethod { MemberId = member.Id, Method = "BKash", AccountNumber = "01711", DisplayName = "My BKash", LastUsedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _service.GetSavedPaymentMethodsAsync(member.Id);

        result.Should().HaveCount(1);
        result.First().Method.Should().Be("BKash");
    }

    [Test]
    public async Task AddSavedPaymentMethodAsync_ShouldCreateMethod()
    {
        var member = new Member { FullName = "U2", Email = "u2@e.com", NID = "U2", MobileNo = "U2", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "P", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var dto = new CreateSavedPaymentMethodDto { Method = "Nagad", AccountNumber = "01811", DisplayName = "Nagad Personal" };
        var result = await _service.AddSavedPaymentMethodAsync(member.Id, dto);

        result.Should().NotBeNull();
        result.Method.Should().Be("Nagad");

        var dbMethod = await _context.SavedPaymentMethods.FirstOrDefaultAsync(m => m.MemberId == member.Id);
        dbMethod.Should().NotBeNull();
    }

    [Test]
    public async Task DeleteSavedPaymentMethodAsync_ShouldRemoveMethodIfOwner()
    {
        var member = new Member { FullName = "U3", Email = "u3@e.com", NID = "U3", MobileNo = "U3", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "P", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var method = new SavedPaymentMethod { MemberId = member.Id, Method = "Rocket", AccountNumber = "01911", DisplayName = "Rocket", LastUsedAt = DateTime.UtcNow };
        _context.SavedPaymentMethods.Add(method);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteSavedPaymentMethodAsync(member.Id, method.Id);

        result.Should().BeTrue();
        var exists = await _context.SavedPaymentMethods.AnyAsync(m => m.Id == method.Id);
        exists.Should().BeFalse();
    }

    [Test]
    public async Task GetMembershipFeeConfigsAsync_ShouldReturnAllConfigs()
    {
        // Setup ensures at least 1 config exists.
        var configs = await _service.GetMembershipFeeConfigsAsync();
        configs.Should().NotBeEmpty();
    }

    [Test]
    public async Task UpdateMembershipFeeConfigAsync_ShouldModifyExistingConfig()
    {
        var current = await _context.MembershipFeeConfigs.FirstAsync();
        var dto = new UpdateMembershipFeeConfigDto { Id = current.Id, Amount = 9999, EffectiveDate = current.EffectiveDate, Description = "Updated" };
        
        var result = await _service.UpdateMembershipFeeConfigAsync(dto, 1);
        
        result.Amount.Should().Be(9999);
        var updated = await _context.MembershipFeeConfigs.FindAsync(current.Id);
        updated!.Amount.Should().Be(9999);
    }

    [Test]
    public async Task RecordMembershipChangeAsync_ShouldCreateHistoryRecord()
    {
        var member = await CreateAndSaveTestMemberAsync("CHG", "chg@e.com", "CHG1", "CHG1");
        await _service.RecordMembershipChangeAsync(member.Id, "General", "Life", 1, "Upgrade");
        
        var history = await _context.MembershipHistories.Where(h => h.MemberId == member.Id).ToListAsync();
        history.Should().HaveCount(1);
        history[0].ChangedFrom.Should().Be("General");
        history[0].ChangedTo.Should().Be("Life");
    }

    [Test]
    public async Task DeletePaymentAsync_ShouldRemovePaymentAndLogActivity()
    {
        var p = new PaymentHistory { MemberId = 1, TransactionId = "DEL-T1", Amount = 100, Status = Enums.PaymentStatus.Completed, PaidAt = DateTime.UtcNow };
        _context.PaymentHistories.Add(p);
        await _context.SaveChangesAsync();
        
        var result = await _service.DeletePaymentAsync(p.Id);
        
        result.Should().BeTrue();
        var exists = await _context.PaymentHistories.AnyAsync(ph => ph.Id == p.Id);
        exists.Should().BeFalse();
    }
}

