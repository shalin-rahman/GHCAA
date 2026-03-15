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
using NUnit.Framework;

namespace GHCAA.Tests.Services;

[TestFixture]
public class MemberService_LinkedIn_Tests : TestBase
{
    private Mock<IFileStorageService> _mockStorage = null!;
    private Mock<IFileUploadRepository> _mockFileRepo = null!;
    private Mock<IOtpService> _mockOtp = null!;
    private Mock<IEmailService> _mockEmail = null!;
    private Mock<IUserService> _mockUserService = null!;
    private Mock<ILogger<MemberService>> _mockLogger = null!;
    private Mock<IActivityService> _mockActivityService = null!;
    private Mock<INotificationService> _mockNotificationService = null!;
    private Mock<ICommunicationService> _mockCommunication = null!;
    private Mock<IConfiguration> _mockConfig = null!;
    private MemberService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockStorage = new Mock<IFileStorageService>();
        _mockFileRepo = new Mock<IFileUploadRepository>();
        _mockOtp = new Mock<IOtpService>();
        _mockEmail = new Mock<IEmailService>();
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<MemberService>>();
        _mockActivityService = new Mock<IActivityService>();
        _mockCommunication = new Mock<ICommunicationService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockConfig = new Mock<IConfiguration>();

        _service = new MemberService(
            _context,
            _mockStorage.Object,
            _mockFileRepo.Object,
            _mockOtp.Object,
            _mockEmail.Object,
            _mockUserService.Object,
            _mockCommunication.Object,
            _mockLogger.Object,
            _mockActivityService.Object,
            _mockNotificationService.Object,
            _mockConfig.Object
        );
    }

    private Member CreateTestMember(string email, string nid)
    {
        return new Member 
        { 
            FullName = "Test", Email = email, NID = nid, MobileNo = nid, 
            FatherName = "F", MotherName = "M", PresentAddress="A", PermanentAddress="A", 
            EmergencyContactName="C", EmergencyContactRelation="R", EmergencyContactPhone="P",
            HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="N", HighestCertificatePassingYear=2010,
            GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="N", GHCLastCertificatePassingYear=2010,
            ProfessionalSector="IT", Designation="Dev",
            Gender = Enums.Gender.Male, BloodGroup = Enums.BloodGroup.APositive
        };
    }

    [Test]
    public async Task UpdateProfile_WithNoGHCRecord_ShouldThrowException()
    {
        // Arrange
        var member = CreateTestMember("test1@ex.com", "111");
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        var dto = new UpdateProfileDto
        {
            AcademicHistory = new List<AcademicRecordDto>
            {
                new AcademicRecordDto { InstitutionName = "Other University", Degree = "BSc", Subject="CS", PassingYear = 2020, IsGHC = false }
            }
        };

        // Act & Assert
        var act = async () => await _service.UpdateProfileAsync(member.Id, dto);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("At least one academic record must be from Govt. Haraganga College.");
    }

    [Test]
    public async Task UpdateProfile_WithGHCRecord_ShouldSucceed()
    {
        // Arrange
        var member = CreateTestMember("test2@ex.com", "222");
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        var dto = new UpdateProfileDto
        {
            AcademicHistory = new List<AcademicRecordDto>
            {
                new AcademicRecordDto { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject="S", PassingYear = 2010, IsGHC = true },
                new AcademicRecordDto { InstitutionName = "Another Uni", Degree = "BSc", Subject="CS", PassingYear = 2015, IsGHC = false }
            },
            ProfessionalHistory = new List<ProfessionalRecordDto>
            {
                new ProfessionalRecordDto { OrganizationName = "Tech Corp", Designation = "Lead", StartDate = DateTime.UtcNow.AddYears(-2), IsCurrent = true }
            }
        };

        // Act
        var result = await _service.UpdateProfileAsync(member.Id, dto);

        // Assert
        result.Should().BeTrue();
        var updated = await _context.Members
            .Include(m => m.AcademicHistory)
            .Include(m => m.ProfessionalHistory)
            .FirstAsync(m => m.Id == member.Id);

        updated.AcademicHistory.Should().HaveCount(2);
        updated.AcademicHistory.Should().Contain(a => a.InstitutionName == "Govt. Haraganga College");
        updated.ProfessionalHistory.Should().HaveCount(1);
        updated.ProfessionalHistory.First().OrganizationName.Should().Be("Tech Corp");
    }

    [Test]
    public async Task RegisterAsync_WithHistory_ShouldSaveCorrectly()
    {
        // Arrange
        var dto = new MemberRegistrationDto
        {
            FullName = "New Member",
            Email = "new@ex.com",
            NID = "333",
            MobileNo = "333",
            Gender = "Male",
            BloodGroup = "APositive",
            FatherName = "F", MotherName = "M", PresentAddress="A", PermanentAddress="A", EmergencyContactName="C", EmergencyContactRelation="R", EmergencyContactPhone="P",
            HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="N", HighestCertificatePassingYear=2010,
            GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="N", GHCLastCertificatePassingYear=2010,
            ProfessionalSector="IT", Designation="Dev",
            AcademicHistory = new List<AcademicRecordDto>
            {
                new AcademicRecordDto { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject="S", PassingYear = 2010, IsGHC = true }
            }
        };

        _mockOtp.Setup(x => x.GenerateAndSendOtpAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("123");

        // Act
        var id = await _service.RegisterAsync(dto, null, null, null);

        // Assert
        id.Should().BeGreaterThan(0);
        var member = await _context.Members.Include(m => m.AcademicHistory).FirstAsync(m => m.Id == id);
        member.AcademicHistory.Should().HaveCount(1);
        member.AcademicHistory.First().InstitutionName.Should().Be("Govt. Haraganga College");
    }
}
