using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace GHCAA.Tests.Services;

[TestFixture]
public class MemberServiceTests
{
    private ApplicationDbContext _context = null!;
    private Microsoft.Data.Sqlite.SqliteConnection _connection = null!;
    private Mock<IFileStorageService> _mockStorage = null!;
    private Mock<IFileUploadRepository> _mockFileRepo = null!;
    private Mock<IOtpService> _mockOtp = null!;
    private Mock<IEmailService> _mockEmail = null!;
    private Mock<IUserService> _mockUserService = null!;
    private Mock<ILogger<MemberService>> _mockLogger = null!;
    private Mock<IActivityService> _mockActivityService = null!;
    private MemberService _service = null!;

    private Mock<ICommunicationService> _mockCommunication = null!;

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

        _mockStorage = new Mock<IFileStorageService>();
        _mockFileRepo = new Mock<IFileUploadRepository>();
        _mockOtp = new Mock<IOtpService>();
        _mockEmail = new Mock<IEmailService>();
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<MemberService>>();
        _mockActivityService = new Mock<IActivityService>();
        _mockCommunication = new Mock<ICommunicationService>();

        _service = new MemberService(
            _context,
            _mockStorage.Object,
            _mockFileRepo.Object,
            _mockOtp.Object,
            _mockEmail.Object,
            _mockUserService.Object,
            _mockCommunication.Object,
            _mockLogger.Object,
            _mockActivityService.Object
        );
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
    }

    private MemberRegistrationDto CreateValidDto()
    {
        return new MemberRegistrationDto
        {
            FullName = "John Doe",
            FatherName = "Father Name",
            MotherName = "Mother Name",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            BloodGroup = "APositive",
            NID = "1234567890",
            MobileNo = "01712345678",
            Email = "test@example.com",
            PresentAddress = "Present Address",
            PermanentAddress = "Permanent Address",
            EmergencyContactName = "Emergency Contact",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01812345678",
            HSCAdmissionYear = 2005,
            GHCAdmissionYear = 2005,
            LastDegreeFromGHC = "HSC",
            SubjectGroup = "Science",
            GHCLastCertificatePassingYear = 2007,
            ProfessionalSector = "IT",
            Designation = "Developer"
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

    [Test]
    public async Task RegisterAsync_WithValidData_ShouldCreateMember()
    {
        // Arrange
        var dto = CreateValidDto();
        _mockOtp.Setup(x => x.GenerateAndSendOtpAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
    }

    [Test]
    public async Task RegisterAsync_WithDuplicateEmail_ShouldThrowException()
    {
        // Arrange
        var dto = CreateValidDto();
        await _context.Members.AddAsync(new Member 
        { 
            Email = dto.Email, 
            NID = "9999999999", 
            MobileNo = "01999999999",
            FullName = "Other User",
            FatherName = "Father",
            MotherName = "Mother",
            PresentAddress = "Address",
            PermanentAddress = "Address",
            EmergencyContactName = "Contact",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01999999999",
            SubjectGroup = "Science",
            ProfessionalSector = "IT",
            Designation = "Developer"
        });
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = async () => await _service.RegisterAsync(dto, null, null, null);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Member with same Email, NID, or Mobile already exists.");
    }

    [Test]
    public async Task RegisterAsync_WithDuplicateNID_ShouldThrowException()
    {
        // Arrange
        var dto = CreateValidDto();
        await _context.Members.AddAsync(new Member 
        { 
            Email = "other@example.com", 
            NID = dto.NID, 
            MobileNo = "01999999999",
            FullName = "Other User",
            FatherName = "Father",
            MotherName = "Mother",
            PresentAddress = "Address",
            PermanentAddress = "Address",
            EmergencyContactName = "Contact",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01999999999",
            SubjectGroup = "Science",
            ProfessionalSector = "IT",
            Designation = "Developer"
        });
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = async () => await _service.RegisterAsync(dto, null, null, null);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Member with same Email, NID, or Mobile already exists.");
    }

    [Test]
    public async Task RegisterAsync_WithDuplicateMobile_ShouldThrowException()
    {
        // Arrange
        var dto = CreateValidDto();
        await _context.Members.AddAsync(new Member 
        { 
            Email = "other@example.com", 
            NID = "9999999999", 
            MobileNo = dto.MobileNo,
            FullName = "Other User",
            FatherName = "Father",
            MotherName = "Mother",
            PresentAddress = "Address",
            PermanentAddress = "Address",
            EmergencyContactName = "Contact",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01999999999",
            SubjectGroup = "Science",
            ProfessionalSector = "IT",
            Designation = "Developer"
        });
        await _context.SaveChangesAsync();

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

        _mockOtp.Setup(x => x.GenerateAndSendOtpAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
        _mockFileRepo.Verify(x => x.AddAsync(
            It.Is<FileUpload>(f => f.UploadType == Enums.FileUploadType.Photo && f.MemberId == memberId),
            It.IsAny<CancellationToken>()), Times.Once);
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

        _mockOtp.Setup(x => x.GenerateAndSendOtpAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("123456");

        // Act
        var memberId = await _service.RegisterAsync(dto, null, certificate, null);

        // Assert
        var member = await _context.Members.FindAsync(memberId);
        member!.CertificatePath.Should().Be(expectedPath);
        _mockFileRepo.Verify(x => x.AddAsync(
            It.Is<FileUpload>(f => f.UploadType == Enums.FileUploadType.Certificate),
            It.IsAny<CancellationToken>()), Times.Once);
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

        _mockOtp.Setup(x => x.GenerateAndSendOtpAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("123456");

        // Act
        var memberId = await _service.RegisterAsync(dto, null, null, paymentProof);

        // Assert
        var member = await _context.Members.FindAsync(memberId);
        member!.PaymentProofPath.Should().Be(expectedPath);
        _mockFileRepo.Verify(x => x.AddAsync(
            It.Is<FileUpload>(f => f.UploadType == Enums.FileUploadType.PaymentProof),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RegisterAsync_ShouldGenerateAndSendOtp()
    {
        // Arrange
        var dto = CreateValidDto();
        _mockOtp.Setup(x => x.GenerateAndSendOtpAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync("123456");

        // Act
        await _service.RegisterAsync(dto, null, null, null);

        // Assert
        _mockOtp.Verify(x => x.GenerateAndSendOtpAsync(dto.Email, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetStatusAsync_WithValidMemberId_ShouldReturnStatus()
    {
        // Arrange
        var member = new Member
        {
            FullName = "Test Member",
            Email = "test@example.com",
            NID = "1234567890",
            MobileNo = "01712345678",
            Status = Enums.MembershipStatus.Active,
            FatherName = "Father",
            MotherName = "Mother",
            PresentAddress = "Address",
            PermanentAddress = "Address",
            EmergencyContactName = "Contact",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01999999999",
            SubjectGroup = "Science",
            ProfessionalSector = "IT",
            Designation = "Developer"
        };
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetStatusAsync(member.Id);

        // Assert
        result.Should().NotBeNull();
        result.MemberId.Should().Be(member.Id);
        result.Message.Should().Contain("Active");
        result.EmailSent.Should().BeTrue();
    }

    [Test]
    public async Task VerifyEmailAsync_WithValidOtp_ShouldVerifyEmail()
    {
        // Arrange
        var email = "test@example.com";
        var otpCode = "123456";
        var member = new Member
        {
            FullName = "Test Member",
            Email = email,
            NID = "1234567890",
            MobileNo = "01712345678",
            EmailVerified = false,
            FatherName = "Father",
            MotherName = "Mother",
            PresentAddress = "Address",
            PermanentAddress = "Address",
            EmergencyContactName = "Contact",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01999999999",
            SubjectGroup = "Science",
            ProfessionalSector = "IT",
            Designation = "Developer"
        };
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        _mockOtp.Setup(x => x.VerifyOtpAsync(email, otpCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.VerifyEmailAsync(email, otpCode);

        // Assert
        result.Should().BeTrue();
        var updatedMember = await _context.Members.FirstAsync(m => m.Email == email);
        updatedMember.EmailVerified.Should().BeTrue();
    }

    [Test]
    public async Task VerifyEmailAsync_WithInvalidOtp_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var otpCode = "999999";
        var member = new Member
        {
            FullName = "Test Member",
            Email = email,
            NID = "1234567890",
            MobileNo = "01712345678",
            EmailVerified = false,
            FatherName = "Father",
            MotherName = "Mother",
            PresentAddress = "Address",
            PermanentAddress = "Address",
            EmergencyContactName = "Contact",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01999999999",
            SubjectGroup = "Science",
            ProfessionalSector = "IT",
            Designation = "Developer"
        };
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        _mockOtp.Setup(x => x.VerifyOtpAsync(email, otpCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.VerifyEmailAsync(email, otpCode);

        // Assert
        result.Should().BeFalse();
        var updatedMember = await _context.Members.FirstAsync(m => m.Email == email);
        updatedMember.EmailVerified.Should().BeFalse();
    }

    [Test]
    public async Task VerifyEmailAsync_WithNonExistentEmail_ShouldReturnFalse()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var otpCode = "123456";

        _mockOtp.Setup(x => x.VerifyOtpAsync(email, otpCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.VerifyEmailAsync(email, otpCode);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task GetProfileAsync_WithValidMember_ShouldReturnProfile()
    {
        // Arrange
        var member = new Member
        {
            FullName = "Test Member",
            Email = "test@example.com",
            MobileNo = "01712345678",
            FatherName = "Father",
            MotherName = "Mother",
            NID = "1234567890",
            PresentAddress = "Present",
            PermanentAddress = "Permanent",
            EmergencyContactName = "EC",
            EmergencyContactRelation = "Brother",
            EmergencyContactPhone = "01812345678",
            SubjectGroup = "Science",
            ProfessionalSector = "IT",
            Designation = "Software Engineer"
        };
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetProfileAsync(member.Id);

        // Assert
        result.Should().NotBeNull();
        result!.FullName.Should().Be(member.FullName);
        result.Email.Should().Be(member.Email);
    }

    [Test]
    public async Task UpdateProfileAsync_WithValidData_ShouldUpdateMember()
    {
        // Arrange
        var member = new Member
        {
            FullName = "Test Member",
            Email = "test@example.com",
            MobileNo = "01712345678",
            FatherName = "Father",
            MotherName = "Mother",
            NID = "1234567890",
            PresentAddress = "Old Address",
            PermanentAddress = "Permanent",
            EmergencyContactName = "EC",
            EmergencyContactRelation = "Brother",
            EmergencyContactPhone = "01812345678",
            SubjectGroup = "Science",
            ProfessionalSector = "IT",
            Designation = "Software Engineer",
            IsMobilePublic = false
        };
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateProfileDto
        {
            PresentAddress = "New Address",
            PermanentAddress = "Perm Address",
            ProfessionalSector = "New IT",
            Designation = "Senior Dev",
            SubjectGroup = "Science",
            LastDegreeFromGHC = "Bachelor",
            GHCLastCertificatePassingYear = 2007,
            IsMobilePublic = true,
            IsEmailPublic = true,
            IsAddressPublic = true
        };

        // Act
        var result = await _service.UpdateProfileAsync(member.Id, updateDto);

        // Assert
        result.Should().BeTrue();
        var updatedMember = await _context.Members.FindAsync(member.Id);
        updatedMember!.PresentAddress.Should().Be("New Address");
        updatedMember.Designation.Should().Be("Senior Dev");
        updatedMember.IsMobilePublic.Should().BeTrue();
    }

    [Test]
    public async Task GetStatusAsync_WithInvalidMemberId_ShouldThrowException()
    {
        // Act & Assert
        var act = async () => await _service.GetStatusAsync(999);
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

        _mockOtp.Setup(x => x.GenerateAndSendOtpAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("123456");

        // Act
        var memberId = await _service.RegisterAsync(dto, photo, certificate, paymentProof);

        // Assert
        var member = await _context.Members.FindAsync(memberId);
        member!.PhotoPath.Should().NotBeNullOrEmpty();
        member.CertificatePath.Should().NotBeNullOrEmpty();
        member.PaymentProofPath.Should().NotBeNullOrEmpty();
        _mockFileRepo.Verify(x => x.AddAsync(It.IsAny<FileUpload>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Test]
    public async Task ApproveMemberAsync_WithValidMember_ShouldApproveAndGenerateMembershipNumber()
    {
        // Arrange
        var member = new Member
        {
            FullName = "Test Member",
            Email = "test@example.com",
            NID = "1234567890",
            MobileNo = "01712345678",
            Status = Enums.MembershipStatus.Applied,
            GHCLastCertificatePassingYear = 2007,
            FatherName = "Father",
            MotherName = "Mother",
            PresentAddress = "Address",
            PermanentAddress = "Address",
            EmergencyContactName = "Contact",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01999999999",
            SubjectGroup = "Science",
            ProfessionalSector = "IT",
            Designation = "Developer"
        };
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        var adminId = 1;
        var defaultPassword = "Test1234";
        _mockUserService.Setup(x => x.GenerateDefaultPassword()).Returns(defaultPassword);
        _mockUserService.Setup(x => x.CreateUserAccountAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = 1, Username = "GHC-2007-0001", MemberId = member.Id });

        // Act
        var result = await _service.ApproveMemberAsync(member.Id, adminId);

        // Assert
        result.MembershipNumber.Should().Be("GHC-2007-0001");
        result.DefaultPassword.Should().Be(defaultPassword);
        var updatedMember = await _context.Members.FindAsync(member.Id);
        updatedMember!.Status.Should().Be(Enums.MembershipStatus.Active);
        updatedMember.MembershipNumber.Should().Be("GHC-2007-0001");
        updatedMember.ApprovedDate.Should().NotBeNull();
        updatedMember.ApprovedBy.Should().Be(adminId);
        _mockUserService.Verify(x => x.CreateUserAccountAsync(member.Id, "GHC-2007-0001", defaultPassword, It.IsAny<CancellationToken>()), Times.Once);
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
            GHCLastCertificatePassingYear = 2007,
            FatherName = "Father",
            MotherName = "Mother",
            PresentAddress = "Address",
            PermanentAddress = "Address",
            EmergencyContactName = "Contact",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01999999999",
            SubjectGroup = "Science",
            ProfessionalSector = "IT",
            Designation = "Developer"
        };
        var member2 = new Member
        {
            FullName = "Member 2",
            Email = "member2@example.com",
            NID = "2222222222",
            MobileNo = "01722222222",
            Status = Enums.MembershipStatus.Applied,
            GHCLastCertificatePassingYear = 2007,
            FatherName = "Father",
            MotherName = "Mother",
            PresentAddress = "Address",
            PermanentAddress = "Address",
            EmergencyContactName = "Contact",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01999999999",
            SubjectGroup = "Science",
            ProfessionalSector = "IT",
            Designation = "Developer"
        };
        await _context.Members.AddRangeAsync(member1, member2);
        await _context.SaveChangesAsync();

        _mockUserService.Setup(x => x.GenerateDefaultPassword()).Returns("Pass1234");
        _mockUserService.Setup(x => x.CreateUserAccountAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int mid, string un, string pw, CancellationToken ct) => new User { Id = mid, Username = un, MemberId = mid });

        // Act
        var result1 = await _service.ApproveMemberAsync(member1.Id, 1);
        var result2 = await _service.ApproveMemberAsync(member2.Id, 1);

        // Assert
        result1.MembershipNumber.Should().Be("GHC-2007-0001");
        result2.MembershipNumber.Should().Be("GHC-2007-0002");
    }

    [Test]
    public async Task ApproveMemberAsync_WithDifferentYears_ShouldGenerateSeparateSerials()
    {
        // Arrange
        var member2007 = new Member
        {
            FullName = "Member 2007",
            Email = "member2007@example.com",
            NID = "1111111111",
            MobileNo = "01711111111",
            Status = Enums.MembershipStatus.Applied,
            GHCLastCertificatePassingYear = 2007,
            FatherName = "Father",
            MotherName = "Mother",
            PresentAddress = "Address",
            PermanentAddress = "Address",
            EmergencyContactName = "Contact",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01999999999",
            SubjectGroup = "Science",
            ProfessionalSector = "IT",
            Designation = "Developer"
        };
        var member2008 = new Member
        {
            FullName = "Member 2008",
            Email = "member2008@example.com",
            NID = "2222222222",
            MobileNo = "01722222222",
            Status = Enums.MembershipStatus.Applied,
            GHCLastCertificatePassingYear = 2008,
            FatherName = "Father",
            MotherName = "Mother",
            PresentAddress = "Address",
            PermanentAddress = "Address",
            EmergencyContactName = "Contact",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01999999999",
            SubjectGroup = "Science",
            ProfessionalSector = "IT",
            Designation = "Developer"
        };
        await _context.Members.AddRangeAsync(member2007, member2008);
        await _context.SaveChangesAsync();

        _mockUserService.Setup(x => x.GenerateDefaultPassword()).Returns("Pass1234");
        _mockUserService.Setup(x => x.CreateUserAccountAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int mid, string un, string pw, CancellationToken ct) => new User { Id = mid, Username = un, MemberId = mid });

        // Act
        var result2007 = await _service.ApproveMemberAsync(member2007.Id, 1);
        var result2008 = await _service.ApproveMemberAsync(member2008.Id, 1);

        // Assert
        result2007.MembershipNumber.Should().Be("GHC-2007-0001");
        result2008.MembershipNumber.Should().Be("GHC-2008-0001");
    }

    [Test]
    public async Task ApproveMemberAsync_WithNonExistentMember_ShouldThrowException()
    {
        // Act & Assert
        var act = async () => await _service.ApproveMemberAsync(999, 1);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Member with ID 999 not found");
    }

    [Test]
    public async Task ApproveMemberAsync_WithAlreadyApprovedMember_ShouldThrowException()
    {
        // Arrange
        var member = new Member
        {
            FullName = "Test Member",
            Email = "test@example.com",
            NID = "1234567890",
            MobileNo = "01712345678",
            Status = Enums.MembershipStatus.Active, // Already approved
            GHCLastCertificatePassingYear = 2007,
            MembershipNumber = "GHC-2007-0001",
            FatherName = "Father",
            MotherName = "Mother",
            PresentAddress = "Address",
            PermanentAddress = "Address",
            EmergencyContactName = "Contact",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01999999999",
            SubjectGroup = "Science",
            ProfessionalSector = "IT",
            Designation = "Developer"
        };
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = async () => await _service.ApproveMemberAsync(member.Id, 1);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Member must have 'Applied' status to be approved*");
    }

    [Test]
    public async Task ApproveMemberAsync_ShouldStampApprovalMetadata()
    {
        // Arrange
        var member = new Member
        {
            FullName = "Test Member",
            Email = "test@example.com",
            NID = "1234567890",
            MobileNo = "01712345678",
            Status = Enums.MembershipStatus.Applied,
            GHCLastCertificatePassingYear = 2007,
            FatherName = "Father",
            MotherName = "Mother",
            PresentAddress = "Address",
            PermanentAddress = "Address",
            EmergencyContactName = "Contact",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01999999999",
            SubjectGroup = "Science",
            ProfessionalSector = "IT",
            Designation = "Developer"
        };
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        var adminId = 42;
        var beforeApproval = DateTime.UtcNow;

        _mockUserService.Setup(x => x.GenerateDefaultPassword()).Returns("Pass1234");
        _mockUserService.Setup(x => x.CreateUserAccountAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = 1, Username = "GHC-2007-0001", MemberId = member.Id });

        // Act
        await _service.ApproveMemberAsync(member.Id, adminId);

        // Assert
        var updatedMember = await _context.Members.FindAsync(member.Id);
        updatedMember!.ApprovedBy.Should().Be(adminId);
        updatedMember.ApprovedDate.Should().NotBeNull();
        updatedMember.ApprovedDate.Should().BeOnOrAfter(beforeApproval);
        updatedMember.ApprovedDate.Should().BeOnOrBefore(DateTime.UtcNow);
    }
}
