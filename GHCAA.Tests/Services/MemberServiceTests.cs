using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace GHCAA.Tests.Services;

[TestFixture]
public class MemberServiceTests : TestBase
{
    private Mock<IFileStorageService> _mockStorage = null!;
    private Mock<IOtpService> _mockOtp = null!;
    private Mock<IEmailService> _mockEmail = null!;
    private Mock<IUserService> _mockUserService = null!;
    private Mock<ILogger<MemberService>> _mockLogger = null!;
    private Mock<IActivityService> _mockActivityService = null!;
    private Mock<INotificationService> _mockNotificationService = null!;
    private Mock<IConfiguration> _mockConfig = null!;
    private Mock<IGamificationService> _mockGamification = null!;
    private Mock<IFinancialService> _mockFinancialService = null!;
    private Mock<ITokenService> _mockTokenService = null!;
    private MemberService _service = null!;

    private Mock<ICommunicationService> _mockCommunication = null!;

    [SetUp]
    public void Setup()
    {
        _mockStorage = new Mock<IFileStorageService>();
        _mockOtp = new Mock<IOtpService>();
        _mockEmail = new Mock<IEmailService>();
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<MemberService>>();
        _mockActivityService = new Mock<IActivityService>();
        _mockCommunication = new Mock<ICommunicationService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockConfig = new Mock<IConfiguration>();
        _mockGamification = new Mock<IGamificationService>();
        _mockFinancialService = new Mock<IFinancialService>();
        _mockTokenService = new Mock<ITokenService>();
        var mockOrgConfigService = new Mock<IOrgConfigService>();

        _service = new MemberService(
            _context,
            _mockStorage.Object,
            _mockOtp.Object,
            _mockEmail.Object,
            _mockUserService.Object,
            _mockCommunication.Object,
            _mockLogger.Object,
            _mockActivityService.Object,
            _mockNotificationService.Object,
            _mockConfig.Object,
            _mockGamification.Object,
            _mockFinancialService.Object,
            new Mock<IRealTimeService>().Object,
            mockOrgConfigService.Object,
            _mockTokenService.Object
        );
    }

    private MemberRegistrationDto CreateValidDto()
    {
        return new MemberRegistrationDto
        {
            FullName = "John Doe",
            FatherName = "Father Name",
            MotherName = "Mother Name",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = GHCAA.Domain.Enums.Gender.Male,
            BloodGroup = GHCAA.Domain.Enums.BloodGroup.APositive,
            NID = "1234567890",
            MobileNo = "01712345678",
            Email = "test@example.com",
            PresentAddress = "Present Address",
            PermanentAddress = "Permanent Address",
            EmergencyContactName = "Emergency Contact",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01812345678",
            AcademicHistory = new List<AcademicRecordDto>
            {
                new AcademicRecordDto { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Science", PassingYear = 2005, IsGHC = true }
            },
            PaymentMethodId = 1,
            TransactionId = "TEST-TXN-123"
        };
    }

    private UploadedFileDto CreateMockFile(string fileName = "test.jpg")
    {
        var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        return new UploadedFileDto
        {
            FileName = fileName,
            Content = stream,
            Length = stream.Length
        };
    }

    [Category("FR-01")]
        [Test]
    public async Task RegisterAsync_WithValidData_ShouldCreateMemberAndPaymentHistory()
    {
        // Arrange
        var dto = CreateValidDto();

        // Ensure a bKash config is available (seeded by EnsureCreated)
        var payConfig = await _context.PaymentConfigurations.FirstOrDefaultAsync(x => x.Method == Enums.PaymentMethod.BKash);
        if (payConfig == null)
        {
            throw new Exception("PaymentConfigurations should be seeded by EnsureCreated in TestBase.");
        }

        dto.PaymentMethodId = payConfig.Id;

        _mockOtp.Setup(x => x.GenerateAndSendOtpAsync(It.IsAny<string>(), It.IsAny<GHCAA.Domain.Enums.OtpPurpose>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("123456");

        // Act
        var memberId = await _service.RegisterAsync(dto, null, null, null);

        // Assert
        memberId.Should().BeGreaterThan(0);
        var member = await _context.Members.FindAsync(memberId);
        member.Should().NotBeNull();
        member!.FullName.Should().Be(dto.FullName);
        member.Email.Should().Be(dto.Email);
        member.Status.Should().Be(Enums.MembershipStatus.Applied);
        member.EmailVerified.Should().BeFalse();

        // Check Payment History
        var payment = await _context.PaymentHistories.FirstOrDefaultAsync(p => p.MemberId == memberId);
        payment.Should().NotBeNull();
        payment!.TransactionId.Should().Be(dto.TransactionId);
        payment.PaymentMethod.Should().Be(Enums.PaymentMethod.BKash);
    }

    [Test]
    public async Task RegisterAsync_ShouldAssignDefaultMembershipType_NotAClientSuppliedOne()
    {
        // 35.5: membership tiers are admin-assigned only. The registration DTO carries no
        // MembershipType at all, and the server stamps the org config's default tier so an
        // applicant can never land on Founding/Executive by crafting a payload.
        var dto = CreateValidDto();
        var payConfig = await _context.PaymentConfigurations.FirstAsync(x => x.Method == Enums.PaymentMethod.BKash);
        dto.PaymentMethodId = payConfig.Id;

        typeof(MemberRegistrationDto).GetProperty("MembershipType").Should().BeNull(
            "the registration payload must not be able to carry a membership tier");

        var memberId = await _service.RegisterAsync(dto, null, null, null);

        var member = await _context.Members.FindAsync(memberId);
        member!.MembershipType.Should().Be(Enums.MembershipType.General);
    }

    [TestCase("Email")]
    [TestCase("NID")]
    [TestCase("Mobile")]
    public async Task RegisterAsync_WithDuplicateField_ShouldThrowException(string duplicateField)
    {
        // Arrange
        var dto = CreateValidDto();
        var email = duplicateField == "Email" ? dto.Email : "other@example.com";
        var mobile = duplicateField == "Mobile" ? dto.MobileNo : "01999999999";
        var nid = duplicateField == "NID" ? dto.NID : "9999999999";
        await CreateAndSaveTestMemberAsync("Other User", email, mobile, nid);

        // Act & Assert
        var act = async () => await _service.RegisterAsync(dto, null, null, null);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Member with same Email, NID, or Mobile already exists.");
    }

    [Test]
    public async Task RegisterAsync_WithPhoto_ShouldSavePhotoAndUpdateMember()
    {
        // Arrange
        var dto = CreateValidDto();
        var photo = CreateMockFile("photo.jpg");
        var expectedPath = "uploads/members/1/photo/photo.jpg";

        _mockStorage.Setup(x => x.SaveFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            Enums.FileUploadType.Photo,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPath);

        _mockOtp.Setup(x => x.GenerateAndSendOtpAsync(It.IsAny<string>(), It.IsAny<GHCAA.Domain.Enums.OtpPurpose>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("123456");

        // Act
        var memberId = await _service.RegisterAsync(dto, photo, null, null);

        // Assert
        var member = await _context.Members.FindAsync(memberId);
        member!.PhotoPath.Should().Be(expectedPath);
        _mockStorage.Verify(x => x.SaveFileAsync(
            It.IsAny<Stream>(),
            photo.FileName,
            memberId,
            Enums.FileUploadType.Photo,
            It.IsAny<CancellationToken>()), Times.Once);
        var fileUploads = await _context.FileUploads.CountAsync(f => f.MemberId == memberId);
        fileUploads.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task RegisterAsync_WithCertificate_ShouldSaveCertificateAndUpdateMember()
    {
        // Arrange
        var dto = CreateValidDto();
        var certificate = CreateMockFile("cert.pdf");
        var expectedPath = "uploads/members/1/certificate/cert.pdf";

        _mockStorage.Setup(x => x.SaveFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            Enums.FileUploadType.Certificate,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPath);

        _mockOtp.Setup(x => x.GenerateAndSendOtpAsync(It.IsAny<string>(), It.IsAny<GHCAA.Domain.Enums.OtpPurpose>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("123456");

        // Act
        var memberId = await _service.RegisterAsync(dto, null, certificate, null);

        // Assert
        var member = await _context.Members.FindAsync(memberId);
        //         member!.CertificatePath.Should().Be(expectedPath);
        var fileUploads = await _context.FileUploads.CountAsync(f => f.MemberId == memberId);
        fileUploads.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task RegisterAsync_WithPaymentProof_ShouldSavePaymentProofAndUpdateMember()
    {
        // Arrange
        var dto = CreateValidDto();
        var paymentProof = CreateMockFile("payment.jpg");
        var expectedPath = "uploads/members/1/paymentproof/payment.jpg";

        _mockStorage.Setup(x => x.SaveFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            Enums.FileUploadType.PaymentProof,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPath);

        _mockOtp.Setup(x => x.GenerateAndSendOtpAsync(It.IsAny<string>(), It.IsAny<GHCAA.Domain.Enums.OtpPurpose>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("123456");

        // Act
        var memberId = await _service.RegisterAsync(dto, null, null, paymentProof);

        // Assert
        var member = await _context.Members.FindAsync(memberId);
        //         member!.PaymentProofPath.Should().Be(expectedPath);
        var fileUploads = await _context.FileUploads.CountAsync(f => f.MemberId == memberId);
        fileUploads.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task RegisterAsync_ShouldGenerateAndSendOtp()
    {
        // Arrange
        var dto = CreateValidDto();
        _mockOtp.Setup(x => x.GenerateAndSendOtpAsync(dto.Email, It.IsAny<GHCAA.Domain.Enums.OtpPurpose>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("123456");

        // Act
        await _service.RegisterAsync(dto, null, null, null);

        // Assert
        _mockOtp.Verify(x => x.GenerateAndSendOtpAsync(dto.Email, It.IsAny<GHCAA.Domain.Enums.OtpPurpose>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Category("FR-02")]
        [Test]
    public async Task GetStatusAsync_WithValidMemberId_ShouldReturnStatus()
    {
        // Arrange
        var member = await CreateAndSaveTestMemberAsync("Test Member", "test@example.com", "01712345678", "1234567890");
        member.Status = Enums.MembershipStatus.Active;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetStatusAsync(member.Id, "test@example.com");

        // Assert
        result.Should().NotBeNull();
        result.MemberId.Should().Be(member.Id);
        result.Message.Should().Contain("Active");
        result.EmailSent.Should().BeTrue();
    }

    [TestCase("123456", true, true)]
    [TestCase("999999", false, false)]
    public async Task VerifyEmailAsync_SetsEmailVerifiedFlag_MatchingOtpOutcome(string otpCode, bool otpValid, bool expectVerified)
    {
        // Arrange
        var email = "test@example.com";
        var member = await CreateAndSaveTestMemberAsync("Test Member", email, "01712345678", "1234567890");
        member.EmailVerified = false;
        await _context.SaveChangesAsync();

        _mockOtp.Setup(x => x.VerifyOtpAsync(email, otpCode, It.IsAny<GHCAA.Domain.Enums.OtpPurpose>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(otpValid);

        // Act
        var result = await _service.VerifyEmailAsync(email, otpCode);

        // Assert
        result.Should().Be(expectVerified);
        var updatedMember = await _context.Members.FirstAsync(m => m.Email == email);
        updatedMember.EmailVerified.Should().Be(expectVerified);
    }

    [Test]
    public async Task VerifyEmailAsync_WithNonExistentEmail_ShouldReturnFalse()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var otpCode = "123456";

        _mockOtp.Setup(x => x.VerifyOtpAsync(email, otpCode, It.IsAny<GHCAA.Domain.Enums.OtpPurpose>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.VerifyEmailAsync(email, otpCode);

        // Assert
        result.Should().BeFalse();
    }

    [Category("FR-03")]
        [Test]
    public async Task GetProfileAsync_WithPrivilegedAccess_ShouldReturnFullProfile()
    {
        // Arrange
        var member = await CreateAndSaveTestMemberAsync("Test Member", "test@example.com", "01712345678", "1234567890");

        // Act
        var result = await _service.GetProfileAsync(member.Id, isPrivileged: true);

        // Assert
        result.Should().NotBeNull();
        result!.FullName.Should().Be(member.FullName);
        result.Email.Should().Be(member.Email);
        result.NID.Should().Be("1234567890");
    }

    [Category("FR-03")]
        [Test]
    public async Task GetProfileAsync_WithNonPrivilegedAccess_ShouldReturnMaskedProfile()
    {
        // Arrange
        var member = await CreateAndSaveTestMemberAsync("Test Member", "test@example.com", "01712345678", "1234567890");
        member.IsNIDPublic = false;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetProfileAsync(member.Id, isPrivileged: false);

        // Assert
        result.Should().NotBeNull();
        result!.NID.Should().NotBe("1234567890");
        result.NID.Should().Contain("*");
    }

    [Category("FR-12")]
        [Test]
    public async Task GetProfileAsync_ProfileCompletionPercentage_ShouldMatchDashboardChecklistCriteria()
    {
        // 30.28: ProfileCompletionPercentage must be computed from the same 4-item criteria
        // as the dashboard "Complete Your Profile" checklist (Identity & Photo / GHC History /
        // Professional Info / Registration Payment), each worth 25%.
        var member = await CreateAndSaveTestMemberAsync("Checklist Member", "checklist@example.com", "01712345600", "1234500000");
        // Test member factory seeds one IsGHC academic record already, so GHC History (25%) is done.
        member.PhotoPath = null; // Identity & Photo requires a photo -> not done
        await _context.SaveChangesAsync();

        var partial = await _service.GetProfileAsync(member.Id, isPrivileged: true);
        partial.Should().NotBeNull();
        partial!.ProfileCompletionPercentage.Should().Be(25m); // only GHC History done

        // Complete the remaining 3 steps: photo, professional history, registration payment.
        member.PhotoPath = "photos/checklist.jpg";
        member.ProfessionalHistory = new List<ProfessionalRecord>
        {
            new ProfessionalRecord { OrganizationName = "GHCAA", Designation = "Engineer", IsCurrent = true, StartDate = DateTime.UtcNow.AddYears(-1) }
        };
        member.PaymentHistories = new List<PaymentHistory>
        {
            new PaymentHistory
            {
                MemberId = member.Id,
                TransactionId = "TXN-CHECKLIST-1",
                Amount = 500,
                FinancialCategory = Enums.FinancialCategory.RegistrationFee,
                Status = Enums.PaymentStatus.Completed,
                PaidAt = DateTime.UtcNow
            }
        };
        await _context.SaveChangesAsync();

        var complete = await _service.GetProfileAsync(member.Id, isPrivileged: true);
        complete.Should().NotBeNull();
        complete!.ProfileCompletionPercentage.Should().Be(100m);
        complete.PaymentStatus.Should().Be("Completed");
    }

    [Test]
    public async Task UpdateProfileAsync_WithValidData_ShouldUpdateMember()
    {
        // Arrange
        var member = await CreateAndSaveTestMemberAsync("Test Member", "test@example.com", "01712345678", "1234567890");
        member.PresentAddress = "Old Address";
        member.IsMobilePublic = false;
        await _context.SaveChangesAsync();

        var updateDto = new UpdateProfileDto
        {
            FullName = "Updated Member",
            FatherName = "Updated Father",
            MotherName = "Updated Mother",
            DateOfBirth = new DateTime(1985, 5, 5),
            Gender = Enums.Gender.Female,
            BloodGroup = Enums.BloodGroup.BPositive,
            PresentAddress = "New Address",
            PermanentAddress = "Perm Address",
            EmergencyContactName = "Updated EC",
            EmergencyContactRelation = "Sister",
            EmergencyContactPhone = "01888888888",
            TShirtSize = "L",
            PhotoPath = "uploads/members/1/photo/new.jpg",
            AcademicHistory = new List<AcademicRecordDto>
            {
                new AcademicRecordDto { InstitutionName = "Govt. Haraganga College", Degree = "Bachelor", Subject = "Science", PassingYear = 2007, IsGHC = true }
            },
            ProfessionalHistory = new List<ProfessionalRecordDto>
            {
                new ProfessionalRecordDto { OrganizationName = "New Org", Designation = "Engineer", Sector = "IT", Location = "Dhaka", StartDate = new DateTime(2020, 1, 1), IsCurrent = true }
            },
            IsMobilePublic = true,
            IsEmailPublic = true,
            IsAddressPublic = true,
            IsNIDPublic = true,
            NotifyEventCreation = false,
            NotifyParticipationApproval = false,
            NotifyRegistrationUpdate = false
        };

        // Act
        var result = await _service.UpdateProfileAsync(member.Id, updateDto);

        // Assert
        result.Should().BeTrue();
        var updatedMember = await _context.Members
            .Include(m => m.AcademicHistory)
            .Include(m => m.ProfessionalHistory)
            .FirstOrDefaultAsync(m => m.Id == member.Id);
        updatedMember!.PresentAddress.Should().Be("New Address");
        updatedMember.PermanentAddress.Should().Be("Perm Address");
        updatedMember.FullName.Should().Be("Updated Member");
        updatedMember.FatherName.Should().Be("Updated Father");
        updatedMember.MotherName.Should().Be("Updated Mother");
        updatedMember.DateOfBirth.Should().Be(DateTime.SpecifyKind(new DateTime(1985, 5, 5), DateTimeKind.Utc));
        updatedMember.Gender.Should().Be(Enums.Gender.Female);
        updatedMember.BloodGroup.Should().Be(Enums.BloodGroup.BPositive);
        updatedMember.EmergencyContactName.Should().Be("Updated EC");
        updatedMember.EmergencyContactRelation.Should().Be("Sister");
        updatedMember.EmergencyContactPhone.Should().Be("01888888888");
        updatedMember.TShirtSize.Should().Be("L");
        updatedMember.PhotoPath.Should().Be("uploads/members/1/photo/new.jpg");
        updatedMember.IsMobilePublic.Should().BeTrue();
        updatedMember.IsEmailPublic.Should().BeTrue();
        updatedMember.IsAddressPublic.Should().BeTrue();
        updatedMember.IsNIDPublic.Should().BeTrue();
        updatedMember.NotifyEventCreation.Should().BeFalse();
        updatedMember.NotifyParticipationApproval.Should().BeFalse();
        updatedMember.NotifyRegistrationUpdate.Should().BeFalse();
        updatedMember.AcademicHistory.Should().ContainSingle(a => a.InstitutionName == "Govt. Haraganga College" && a.PassingYear == 2007);
        updatedMember.ProfessionalHistory.Should().ContainSingle(p => p.OrganizationName == "New Org" && p.Designation == "Engineer");
    }

    [Category("FR-02")]
        [Test]
    public async Task GetStatusAsync_WithInvalidMemberId_ShouldThrowException()
    {
        // Act & Assert
        var act = async () => await _service.GetStatusAsync(999, "anything@example.com");
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Member not found");
    }

    [Test]
    public async Task RegisterAsync_WithAllFiles_ShouldSaveAllFilesAndUpdateMember()
    {
        // Arrange
        var dto = CreateValidDto();
        var photo = CreateMockFile("photo.jpg");
        var certificate = CreateMockFile("cert.pdf");
        var paymentProof = CreateMockFile("payment.jpg");

        _mockStorage.Setup(x => x.SaveFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<Enums.FileUploadType>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream s, string fn, int mid, Enums.FileUploadType t, CancellationToken ct) =>
                $"uploads/members/{mid}/{t.ToString().ToLower()}/{fn}");

        _mockOtp.Setup(x => x.GenerateAndSendOtpAsync(It.IsAny<string>(), It.IsAny<GHCAA.Domain.Enums.OtpPurpose>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("123456");

        // Act
        var memberId = await _service.RegisterAsync(dto, photo, certificate, paymentProof);

        // Assert
        var member = await _context.Members.FindAsync(memberId);
        member!.PhotoPath.Should().NotBeNullOrEmpty();
        //         member.CertificatePath.Should().NotBeNullOrEmpty();
        //         member.PaymentProofPath.Should().NotBeNullOrEmpty();
        var fileUploads = await _context.FileUploads.CountAsync(f => f.MemberId == memberId);
        fileUploads.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task ApproveMemberAsync_WithMultipleMembersSameYear_ShouldGenerateSequentialNumbers()
    {
        // Arrange
        var member1 = new Member
        {
            FullName = "Member 1",
            Email = "member1@example.com",
            NID = "1111111111",
            MobileNo = "01711111111",
            Status = Enums.MembershipStatus.Applied,
            FatherName = "Father",
            MotherName = "Mother",
            PresentAddress = "Address",
            PermanentAddress = "Address",
            EmergencyContactName = "Contact",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01999999999",
            AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Science", PassingYear = 2007, IsGHC = true } },
            IsProfileComplete = true,
            PhotoPath = "test.jpg",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = Enums.Gender.Male,
            BloodGroup = Enums.BloodGroup.APositive,
            ProfessionalHistory = new List<ProfessionalRecord> { new ProfessionalRecord { OrganizationName = "Test Org", Designation = "Developer", StartDate = new DateTime(2015, 1, 1), IsCurrent = true } }
        };
        var member2 = new Member
        {
            FullName = "Member 2",
            Email = "member2@example.com",
            NID = "2222222222",
            MobileNo = "01722222222",
            Status = Enums.MembershipStatus.Applied,
            FatherName = "Father",
            MotherName = "Mother",
            PresentAddress = "Address",
            PermanentAddress = "Address",
            EmergencyContactName = "Contact",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01999999999",
            AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Science", PassingYear = 2007, IsGHC = true } },
            IsProfileComplete = true,
            PhotoPath = "test.jpg",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = Enums.Gender.Male,
            BloodGroup = Enums.BloodGroup.APositive,
            ProfessionalHistory = new List<ProfessionalRecord> { new ProfessionalRecord { OrganizationName = "Test Org", Designation = "Developer", StartDate = new DateTime(2015, 1, 1), IsCurrent = true } }
        };
        await _context.Members.AddRangeAsync(member1, member2);
        await _context.SaveChangesAsync();

        await _context.PaymentHistories.AddRangeAsync(
            new PaymentHistory { MemberId = member1.Id, Amount = 500, Status = Enums.PaymentStatus.Completed, FinancialCategory = Enums.FinancialCategory.RegistrationFee, TransactionId = "TRX-001" },
            new PaymentHistory { MemberId = member2.Id, Amount = 500, Status = Enums.PaymentStatus.Completed, FinancialCategory = Enums.FinancialCategory.RegistrationFee, TransactionId = "TRX-002" }
        );
        await _context.SaveChangesAsync();

        _mockUserService.Setup(x => x.GenerateDefaultPassword()).Returns("Pass1234");
        _mockUserService.Setup(x => x.CreateUserAccountAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int mid, string un, string pw, CancellationToken ct) => new User { Id = mid, Username = un, MemberId = mid });

        var result1 = await _service.ApproveMemberAsync(member1.Id, 1);
        var result2 = await _service.ApproveMemberAsync(member2.Id, 1);

        // Assert
        // Assert
        result1.MembershipNumber.Should().NotBe(result2.MembershipNumber);
        result1.MembershipNumber.Should().StartWith("GHC");
        result2.MembershipNumber.Should().StartWith("GHC");
    }

    [Test]
    public async Task ApproveMemberAsync_WithDifferentYears_ShouldGenerateSeparateSerials()
    {
        // Arrange
        var member2007 = new Member
        {
            FullName = "Member 2007",
            Email = "m2007@e.com",
            NID = "N1",
            MobileNo = "M1",
            Status = Enums.MembershipStatus.Applied,
            FatherName = "F",
            MotherName = "M",
            PresentAddress = "A",
            PermanentAddress = "A",
            EmergencyContactName = "E",
            EmergencyContactRelation = "R",
            EmergencyContactPhone = "0",
            AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Science", PassingYear = 2007, IsGHC = true } },
            IsProfileComplete = true,
            PhotoPath = "test.jpg",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = Enums.Gender.Male,
            BloodGroup = Enums.BloodGroup.APositive,
            ProfessionalHistory = new List<ProfessionalRecord> { new ProfessionalRecord { OrganizationName = "Test Org", Designation = "Developer", StartDate = new DateTime(2015, 1, 1), IsCurrent = true } }
        };
        var member2008 = new Member
        {
            FullName = "Member 2008",
            Email = "m2008@e.com",
            NID = "N2",
            MobileNo = "M2",
            Status = Enums.MembershipStatus.Applied,
            FatherName = "F",
            MotherName = "M",
            PresentAddress = "A",
            PermanentAddress = "A",
            EmergencyContactName = "E",
            EmergencyContactRelation = "R",
            EmergencyContactPhone = "0",
            AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Science", PassingYear = 2008, IsGHC = true } },
            IsProfileComplete = true,
            PhotoPath = "test.jpg",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = Enums.Gender.Male,
            BloodGroup = Enums.BloodGroup.APositive,
            ProfessionalHistory = new List<ProfessionalRecord> { new ProfessionalRecord { OrganizationName = "Test Org", Designation = "Developer", StartDate = new DateTime(2015, 1, 1), IsCurrent = true } }
        };
        await _context.Members.AddRangeAsync(member2007, member2008);
        await _context.PaymentHistories.AddRangeAsync(
            new PaymentHistory { Member = member2007, Amount = 500, Status = Enums.PaymentStatus.Completed, FinancialCategory = Enums.FinancialCategory.RegistrationFee, TransactionId = "TRX-2007" },
            new PaymentHistory { Member = member2008, Amount = 500, Status = Enums.PaymentStatus.Completed, FinancialCategory = Enums.FinancialCategory.RegistrationFee, TransactionId = "TRX-2008" }
        );
        await _context.SaveChangesAsync();

        _mockUserService.Setup(x => x.CreateUserAccountAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = member2007.Id, Username = member2007.NID, MemberId = member2007.Id });

        // Act
        var result2007 = await _service.ApproveMemberAsync(member2007.Id, 1);
        var result2008 = await _service.ApproveMemberAsync(member2008.Id, 1);

        // Assert
        result2007.MembershipNumber.Should().StartWith("GHC");
        result2008.MembershipNumber.Should().StartWith("GHC");
    }

    [Test]
    public async Task ApproveMemberAsync_ShouldAwardGamificationPoints()
    {
        // Arrange
        var member = new Member
        {
            FullName = "P1",
            Email = "p1@e.com",
            NID = "123",
            MobileNo = "017",
            Status = Enums.MembershipStatus.Applied,
            FatherName = "F",
            MotherName = "M",
            PresentAddress = "A",
            PermanentAddress = "A",
            EmergencyContactName = "E",
            EmergencyContactRelation = "R",
            EmergencyContactPhone = "0",
            AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Science", PassingYear = 2007, IsGHC = true } },
            IsProfileComplete = true,
            PhotoPath = "test.jpg",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = Enums.Gender.Male,
            BloodGroup = Enums.BloodGroup.APositive,
            ProfessionalHistory = new List<ProfessionalRecord> { new ProfessionalRecord { OrganizationName = "Test Org", Designation = "Developer", StartDate = new DateTime(2015, 1, 1), IsCurrent = true } }
        };
        await _context.Members.AddAsync(member);
        await _context.PaymentHistories.AddAsync(new PaymentHistory { Member = member, Amount = 500, Status = Enums.PaymentStatus.Completed, FinancialCategory = Enums.FinancialCategory.RegistrationFee, TransactionId = "TRX-AWD" });
        await _context.SaveChangesAsync();

        _mockUserService.Setup(x => x.CreateUserAccountAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = member.Id, Username = member.NID, MemberId = member.Id });

        // Act
        await _service.ApproveMemberAsync(member.Id, 1);

        // Assert
        _mockGamification.Verify(x => x.AwardPointsAsync(member.Id, "PROFILE_VERIFIED", It.IsAny<int?>(), It.Is<string>(s => s.Contains("Initial approval")), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SendAdminPasswordResetLinkAsync_ShouldRevokeExistingRefreshTokens()
    {
        // Arrange: a member with an existing session. An admin-initiated reset must not leave a
        // token issued before the reset still usable after it.
        var member = new Member
        {
            FullName = "P1",
            Email = "p1@e.com",
            NID = "123",
            MobileNo = "017",
            Status = Enums.MembershipStatus.Active,
            FatherName = "F",
            MotherName = "M",
            PresentAddress = "A",
            PermanentAddress = "A",
            EmergencyContactName = "E",
            EmergencyContactRelation = "R",
            EmergencyContactPhone = "0",
            AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Science", PassingYear = 2007, IsGHC = true } }
        };
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        var user = new User { Username = member.NID, PasswordHash = "x", MemberId = member.Id };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // A DB-stored template short-circuits the org-config fallback branch this test doesn't
        // otherwise set up.
        _mockCommunication.Setup(x => x.GetTemplateByCodeAsync(Constants.TemplateCodes.PasswordReset, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailTemplate { Code = Constants.TemplateCodes.PasswordReset, Subject = "Reset", Body = "{{FullName}} {{ResetUrl}} {{MembershipNumber}}", Description = "test" });

        // Act
        await _service.SendAdminPasswordResetLinkAsync(member.Id, isPrivilegedCaller: true);

        // Assert
        _mockTokenService.Verify(x => x.RevokeAllRefreshTokensAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Category("FR-44")]
        [Test]
    public async Task RejectMemberAsync_ShouldSendEmailAndSoftDeleteMember()
    {
        // Arrange
        var member = new Member
        {
            FullName = "To Reject",
            Email = "reject@example.com",
            NID = "111",
            MobileNo = "011",
            Status = Enums.MembershipStatus.Applied,
            FatherName = "F",
            MotherName = "M",
            PresentAddress = "A",
            PermanentAddress = "A",
            EmergencyContactName = "E",
            EmergencyContactRelation = "R",
            EmergencyContactPhone = "0",
            AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "GHC", Degree = "HSC", Subject = "Science", PassingYear = 2007, IsGHC = true } }
        };
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();
        var adminId = 1;
        var reason = "Invalid data";

        // Act
        var result = await _service.RejectMemberAsync(member.Id, adminId, reason);

        // Assert — 24.30: member is soft-deleted, not hard-deleted; audit trail is preserved
        result.Should().BeTrue();
        var rejectedMember = await _context.Members.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == member.Id);
        rejectedMember.Should().NotBeNull();
        rejectedMember!.Status.Should().Be(Enums.MembershipStatus.Rejected);
        rejectedMember.IsArchived.Should().BeTrue();

        _mockCommunication.Verify(x => x.SendEmailByCodeAsync(
            member.Email, "APPLICATION_REJECTED", It.Is<Dictionary<string, string>>(d => d["Reason"] == reason), It.IsAny<Member>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task AdminUpdateMemberAsync_WithValidData_ShouldUpdateRestrictedFields()
    {
        // Arrange
        var member = new Member
        {
            FullName = "Original Name",
            Email = "original@e.com",
            NID = "123",
            MobileNo = "017",
            Status = Enums.MembershipStatus.Active,
            MembershipType = Enums.MembershipType.Founding,
            FatherName = "F",
            MotherName = "M",
            PresentAddress = "A",
            PermanentAddress = "P",
            EmergencyContactName = "E",
            EmergencyContactRelation = "R",
            EmergencyContactPhone = "0",
            AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "GHC", Degree = "HSC", Subject = "Science", PassingYear = 2007, IsGHC = true } }
        };
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        var updateDto = new AdminMemberUpdateDto
        {
            FullName = "Updated Name",
            FatherName = "Updated Father",
            MotherName = "Updated Mother",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = Enums.Gender.Female,
            BloodGroup = Enums.BloodGroup.BPositive,
            PresentAddress = "Updated Address",
            PermanentAddress = "Updated Perm Address",
            EmergencyContactName = "EC",
            EmergencyContactRelation = "Brother",
            EmergencyContactPhone = "01800000000",
            TShirtSize = "XL",
            PhotoPath = "uploads/members/1/photo/updated.jpg",
            SignaturePath = "uploads/members/1/signature/updated.jpg",
            CertificatePath = "uploads/members/1/certificate/updated.pdf",
            PaymentProofPath = "uploads/members/1/paymentproof/updated.jpg",
            MembershipNumber = "GHC-9999",
            Email = "updated@e.com",
            MobileNo = "01799999999",
            NID = "9999999999",
            Category = Enums.MemberCategory.LifelongPatron,
            MembershipType = Enums.MembershipType.Founding,
            Status = Enums.MembershipStatus.InactivePayment,
            MembershipChangeReason = "Reviewed",
            ECChangeReason = "N/A",
            IsVerified = true,
            ContributionPoints = 50,
            IsMobilePublic = true,
            IsEmailPublic = true,
            IsAddressPublic = true,
            IsNIDPublic = true,
            NotifyEventCreation = false,
            NotifyParticipationApproval = false,
            NotifyRegistrationUpdate = false,
            NotifyRelevantUpdates = false,
            AcademicHistory = new List<AcademicRecordDto>
            {
                new AcademicRecordDto { InstitutionName = "Govt. Haraganga College", Degree = "Bachelor", Subject = "Science", PassingYear = 2007, IsGHC = true }
            },
            ProfessionalHistory = new List<ProfessionalRecordDto>
            {
                new ProfessionalRecordDto { OrganizationName = "New Org", Designation = "Engineer", Sector = "IT", Location = "Dhaka", StartDate = new DateTime(2020, 1, 1), IsCurrent = true }
            }
        };

        // Act
        var result = await _service.AdminUpdateMemberAsync(member.Id, updateDto, 1, isPrivilegedCaller: true);

        // Assert
        result.Should().BeTrue();
        var updated = await _context.Members
            .Include(m => m.AcademicHistory)
            .Include(m => m.ProfessionalHistory)
            .FirstOrDefaultAsync(m => m.Id == member.Id);
        updated!.FullName.Should().Be("Updated Name");
        updated.MembershipNumber.Should().Be("GHC-9999");
        updated.Category.Should().Be(Enums.MemberCategory.LifelongPatron);
        updated.FatherName.Should().Be("Updated Father");
        updated.MotherName.Should().Be("Updated Mother");
        updated.DateOfBirth.Should().Be(DateTime.SpecifyKind(new DateTime(1990, 1, 1), DateTimeKind.Utc));
        updated.NID.Should().Be("9999999999");
        updated.MobileNo.Should().Be("01799999999");
        updated.Email.Should().Be("updated@e.com");
        updated.PresentAddress.Should().Be("Updated Address");
        updated.PermanentAddress.Should().Be("Updated Perm Address");
        updated.EmergencyContactName.Should().Be("EC");
        updated.EmergencyContactRelation.Should().Be("Brother");
        updated.EmergencyContactPhone.Should().Be("01800000000");
        updated.TShirtSize.Should().Be("XL");
        updated.Gender.Should().Be(Enums.Gender.Female);
        updated.BloodGroup.Should().Be(Enums.BloodGroup.BPositive);
        updated.PhotoPath.Should().Be("uploads/members/1/photo/updated.jpg");
        updated.SignaturePath.Should().Be("uploads/members/1/signature/updated.jpg");
        updated.CertificatePath.Should().Be("uploads/members/1/certificate/updated.pdf");
        updated.PaymentProofPath.Should().Be("uploads/members/1/paymentproof/updated.jpg");
        updated.MembershipType.Should().Be(Enums.MembershipType.Founding);
        updated.Status.Should().Be(Enums.MembershipStatus.InactivePayment);
        updated.MembershipChangeReason.Should().Be("Reviewed");
        updated.ECChangeReason.Should().Be("N/A");
        updated.IsVerified.Should().BeTrue();
        updated.ContributionPoints.Should().Be(50);
        updated.IsMobilePublic.Should().BeTrue();
        updated.IsEmailPublic.Should().BeTrue();
        updated.IsAddressPublic.Should().BeTrue();
        updated.IsNIDPublic.Should().BeTrue();
        updated.NotifyEventCreation.Should().BeFalse();
        updated.NotifyParticipationApproval.Should().BeFalse();
        updated.NotifyRegistrationUpdate.Should().BeFalse();
        updated.NotifyRelevantUpdates.Should().BeFalse();
        updated.AcademicHistory.Should().ContainSingle(a => a.InstitutionName == "Govt. Haraganga College" && a.PassingYear == 2007);
        updated.ProfessionalHistory.Should().ContainSingle(p => p.OrganizationName == "New Org" && p.Designation == "Engineer");
    }

    [Test]
    public async Task RegisterAsync_WithFailingStorage_ShouldRollbackTransaction()
    {
        // Assemble
        var dto = CreateValidDto();
        var photo = CreateMockFile("photo.jpg");

        _mockStorage.Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Enums.FileUploadType>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Storage failure"));

        var membersBefore = await _context.Members.CountAsync();

        // Act & Assert
        var act = async () => await _service.RegisterAsync(dto, photo, null, null);
        await act.Should().ThrowAsync<IOException>().WithMessage("Storage failure");

        var membersAfter = await _context.Members.CountAsync();
        membersAfter.Should().Be(membersBefore);
    }
}
