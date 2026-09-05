using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
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

        var storageMock = new Mock<IFileStorageService>();
        var realTimeMock = new Mock<IRealTimeService>();
        var loggerMock = new Mock<ILogger<FinancialService>>();
        var configMock = new Mock<IConfiguration>();
        var userMock = new Mock<IUserService>();
        var activityMock = new Mock<IActivityService>();
        var gamificationMock = new Mock<IGamificationService>();
        var orgConfigMock = new Mock<IOrgConfigService>();

        _service = new FinancialService(
            _context,
            _communicationMock.Object,
            _notificationMock.Object,
            storageMock.Object,
            realTimeMock.Object,
            loggerMock.Object,
            configMock.Object,
            userMock.Object,
            activityMock.Object,
            gamificationMock.Object,
            orgConfigMock.Object,
            new Mock<IServiceProvider>().Object);

        if (!await _context.MembershipFeeConfigs.AnyAsync())
        {
            _context.MembershipFeeConfigs.Add(new MembershipFeeConfig { MembershipType = Enums.MembershipType.General, Amount = 1000, EffectiveDate = new DateTime(2023, 1, 1), Description = "Base Fee" });
            await _context.SaveChangesAsync();
        }
    }

    // Full-detail active member (Status + AcademicHistory) shared by the tests below that touch
    // membership-fee/dues logic, as opposed to CreateMinimalMemberAsync used for FK-only cases.
    private async Task<Member> CreateActiveMemberWithHistoryAsync(string fullName, string email, string nid)
    {
        var member = new Member
        {
            FullName = fullName,
            Email = email,
            NID = nid,
            MobileNo = nid,
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
        return member;
    }

    [Category("FR-21")]
        [Category("FR-22")]
        [Test]
    public async Task RecordPaymentAsync_ShouldAddPaymentAndReturnDto()
    {
        var member = await CreateActiveMemberWithHistoryAsync("Payer", "fsp@e.com", "FSP1");

        var dto = new CreatePaymentHistoryDto { MemberId = member.Id, Amount = 500, TransactionId = "TRX-FSP-100", PaidAt = DateTime.UtcNow, Notes = "Test Payment" };
        var result = await _service.RecordPaymentAsync(dto);

        result.Should().NotBeNull();
        result.TransactionId.Should().Be("TRX-FSP-100");
        result.Amount.Should().Be(500);

        var dbPayment = await _context.PaymentHistories.FirstOrDefaultAsync(p => p.TransactionId == "TRX-FSP-100");
        dbPayment.Should().NotBeNull();
        dbPayment!.MemberId.Should().Be(member.Id);
    }

    // 82.32: a guest event payment (AllowNonMembers) reaches this with no MemberId at all.
    // GatewaysController used to pass `memberId ?? 0`, which threw a foreign-key DbUpdateException
    // because Member Id 0 does not exist. This pins that a null MemberId is recorded, not defaulted,
    // and that recording one does not attempt member-scoped notification/receipt-storage side
    // effects that would themselves throw for a member that does not exist.
    [Category("FR-22")]
        [Test]
    public async Task RecordPaymentAsync_WithNullMemberId_RecordsGuestPaymentWithoutThrowing()
    {
        var dto = new CreatePaymentHistoryDto { MemberId = null, Amount = 200, TransactionId = "TRX-GUEST-1", PaidAt = DateTime.UtcNow, Notes = "Guest event fee" };

        var act = async () => await _service.RecordPaymentAsync(dto);
        await act.Should().NotThrowAsync();

        var dbPayment = await _context.PaymentHistories.FirstOrDefaultAsync(p => p.TransactionId == "TRX-GUEST-1");
        dbPayment.Should().NotBeNull();
        dbPayment!.MemberId.Should().BeNull();

        _notificationMock.Verify(x => x.CreateNotificationAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Enums.NotificationType>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetMemberPaymentHistoryAsync_ShouldReturnDtoList()
    {
        var member = await CreateActiveMemberWithHistoryAsync("History User", "fsh@e.com", "FSH1");

        _context.PaymentHistories.Add(new PaymentHistory { MemberId = member.Id, TransactionId = "FSH-T1", Amount = 100, PaidAt = DateTime.UtcNow.AddDays(-1), Status = Enums.PaymentStatus.Completed });
        _context.PaymentHistories.Add(new PaymentHistory { MemberId = member.Id, TransactionId = "FSH-T2", Amount = 200, PaidAt = DateTime.UtcNow, Status = Enums.PaymentStatus.Pending });
        await _context.SaveChangesAsync();

        var result = await _service.GetMemberPaymentHistoryAsync(member.Id);

        result.Should().HaveCount(2);
        result.First().TransactionId.Should().Be("FSH-T2"); // Ordered by Date Descending
    }

    [Category("FR-20")]
        [Test]
    public async Task GenerateAnnualDuesAsync_ShouldCreateDuesForActiveMembers()
    {
        var member = await CreateActiveMemberWithHistoryAsync("Active User", "fsg@e.com", "FSG1");

        await _service.GenerateAnnualDuesAsync(2024);

        var due = await _context.MembershipDues.FirstOrDefaultAsync(d => d.MemberId == member.Id);
        due.Should().NotBeNull();
        due!.Amount.Should().Be(1000);
        due.Year.Should().Be(2024);
    }

    [Test]
    public async Task MarkDueAsPaidAsync_ShouldUpdateStatus()
    {
        var member = await CreateActiveMemberWithHistoryAsync("M", "fsm@e.com", "FSM1");

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

    // Minimal member for tests that only need a valid MemberId FK, not membership status/history.
    private async Task<Member> CreateMinimalMemberAsync(string suffix)
    {
        var member = new Member { FullName = suffix, Email = $"{suffix.ToLowerInvariant()}@e.com", NID = suffix, MobileNo = suffix, FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "P", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
        return member;
    }

    [Test]
    public async Task GetSavedPaymentMethodsAsync_ShouldReturnSavedMethods()
    {
        var member = await CreateMinimalMemberAsync("U1");

        _context.SavedPaymentMethods.Add(new SavedPaymentMethod { MemberId = member.Id, Method = "BKash", AccountNumber = "01711", DisplayName = "My BKash", LastUsedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _service.GetSavedPaymentMethodsAsync(member.Id);

        result.Should().HaveCount(1);
        result.First().Method.Should().Be("BKash");
    }

    [Test]
    public async Task AddSavedPaymentMethodAsync_ShouldCreateMethod()
    {
        var member = await CreateMinimalMemberAsync("U2");

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
        var member = await CreateMinimalMemberAsync("U3");

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

    [Category("FR-25")]
        [Category("FR-44")]
        [Test]
    public async Task DeletePaymentAsync_ShouldRemovePaymentAndLogActivity()
    {
        var p = new PaymentHistory { MemberId = 1, TransactionId = "DEL-T1", Amount = 100, Status = Enums.PaymentStatus.Completed, PaidAt = DateTime.UtcNow };
        _context.PaymentHistories.Add(p);
        await _context.SaveChangesAsync();

        var result = await _service.DeletePaymentAsync(p.Id, adminId: 1);

        result.Should().BeTrue();

        // 82.32: since 82.16, "delete" is soft — PaymentHistoryConfiguration's query filter hides
        // the row, it does not remove it. The plain AnyAsync() this used to assert with would pass
        // identically if DeletePaymentAsync stamped nothing at all, since the filter alone accounts
        // for the false. IgnoreQueryFilters() checks what the row actually holds.
        var stillThere = await _context.PaymentHistories.IgnoreQueryFilters().FirstOrDefaultAsync(ph => ph.Id == p.Id);
        stillThere.Should().NotBeNull("a payment must never be physically removed");
        stillThere!.IsDeleted.Should().BeTrue();
        stillThere.DeletedByAdminId.Should().Be(1);
    }
}

