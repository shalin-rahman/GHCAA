using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services;

[TestFixture]
public class CommunicationServiceTests : TestBase
{
    private Mock<IEmailService> _mockEmail = null!;
    private Mock<ILogger<CommunicationService>> _mockLogger = null!;
    private Mock<IOrgConfigService> _mockOrgConfig = null!;
    private CommunicationService _service = null!;

    private GHCAA.Application.DTOs.OrgConfigDto _mockConfig = null!;

    [SetUp]
    public void Setup()
    {
        _mockEmail = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<CommunicationService>>();
        _mockOrgConfig = new Mock<IOrgConfigService>();

        _mockConfig = new GHCAA.Application.DTOs.OrgConfigDto
        {
            Branding = new GHCAA.Application.DTOs.BrandingDto { FullName = "GHC Alumni Association", ShortName = "GHCAA" },
            Contact = new GHCAA.Application.DTOs.ContactDto { SupportEmail = "support@ghcaa.org", PortalBaseUrl = "https://ghcaa.example/portal" }
        };
        _mockOrgConfig.Setup(x => x.GetConfigAsync()).ReturnsAsync(_mockConfig);

        _service = new CommunicationService(_context, _mockEmail.Object, _mockLogger.Object, _mockOrgConfig.Object);
    }

    [Test]
    public async Task SendIndividualEmailAsync_ShouldReplacePlaceholders()
    {
        // Arrange
        var member = new Member
        {
            FullName = "John Doe",
            Email = "john@example.com",
            NID = "123",
            MobileNo = "01",
            FatherName = "F",
            MotherName = "M",
            PresentAddress = "A",
            PermanentAddress = "A",
            EmergencyContactName = "E",
            EmergencyContactRelation = "R",
            EmergencyContactPhone = "0"
        };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var template = new EmailTemplate
        {
            Code = "TEST_CODE",
            Subject = "Hi {{FullName}}",
            Body = "Welcome to batch {{PassingYear}}!",
            Description = "Test individual email"
        };
        _context.EmailTemplates.Add(template);
        await _context.SaveChangesAsync();

        // Act
        await _service.SendIndividualEmailAsync(member.Id, "TEST_CODE");

        // Assert
        _mockEmail.Verify(x => x.SendEmailAsync(
            "john@example.com",
            "Hi John Doe",
            It.Is<string>(b => b.Contains("Welcome to batch")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Category("FR-29")]
        [Test]
    public async Task SendBatchEmailAsync_ShouldSendMultipleEmails()
    {
        // Arrange
        var year = 1942;
        var members = new List<Member>
        {
            new Member { FullName = "A", Email = "a@e.com", NID = "1", MobileNo = "0", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", AcademicHistory = new List<AcademicRecord> { new AcademicRecord { IsGHC = true, PassingYear = year, InstitutionName = "GHC", Degree = "HSC", Subject = "Science" } } },
            new Member { FullName = "B", Email = "b@e.com", NID = "2", MobileNo = "01", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", AcademicHistory = new List<AcademicRecord> { new AcademicRecord { IsGHC = true, PassingYear = year, InstitutionName = "GHC", Degree = "HSC", Subject = "Science" } } }
        };
        _context.Members.AddRange(members);
        _context.EmailTemplates.Add(new EmailTemplate { Code = "BATCH", Subject = "S", Body = "B", Description = "Batch test template" });
        await _context.SaveChangesAsync();

        // Act
        await _service.SendBatchEmailAsync(new List<int> { year }, "BATCH");

        // Assert — body will have footer appended, so use Contains match
        _mockEmail.Verify(x => x.SendEmailAsync(It.IsAny<string>(), "S", It.Is<string>(b => b.Contains("B")), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Category("FR-29")]
        [Test]
    public async Task SendBatchCustomEmailAsync_ShouldSendToCorrectYear()
    {
        // Arrange
        var year = 1942;
        var members = new List<Member>
        {
            new Member { FullName = "2005-A", Email = "2005a@e.com", NID = "1", MobileNo = "0", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", AcademicHistory = new List<AcademicRecord> { new AcademicRecord { IsGHC = true, PassingYear = year, InstitutionName = "GHC", Degree = "HSC", Subject = "Science" } } },
            new Member { FullName = "2010-B", Email = "2010b@e.com", NID = "2", MobileNo = "01", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", AcademicHistory = new List<AcademicRecord> { new AcademicRecord { IsGHC = true, PassingYear = 2010, InstitutionName = "GHC", Degree = "HSC", Subject = "Science" } } }
        };
        _context.Members.AddRange(members);
        await _context.SaveChangesAsync();

        // Act
        await _service.SendBatchCustomEmailAsync(new List<int> { year }, "Manual Subject", "Manual Body");

        // Assert
        _mockEmail.Verify(x => x.SendEmailAsync("2005a@e.com", "Manual Subject", It.Is<string>(b => b.Contains("Manual Body")), It.IsAny<CancellationToken>()), Times.Once);
        _mockEmail.Verify(x => x.SendEmailAsync("2010b@e.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Category("FR-29")]
        [Test]
    public async Task SendTypeCustomEmailAsync_ShouldSendToCorrectMembershipType()
    {
        // Arrange
        var members = new List<Member>
        {
            new Member { FullName = "Exec", Email = "exec@e.com", MembershipType = MembershipType.Executive, NID = "1", MobileNo = "0", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" },
            new Member { FullName = "General", Email = "general@e.com", MembershipType = MembershipType.General, NID = "2", MobileNo = "01", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" }
        };
        _context.Members.AddRange(members);
        await _context.SaveChangesAsync();

        // Act
        await _service.SendTypeCustomEmailAsync(new List<string> { "Executive" }, "Type Subject", "Type Body");

        // Assert
        _mockEmail.Verify(x => x.SendEmailAsync("exec@e.com", "Type Subject", It.Is<string>(b => b.Contains("Type Body")), It.IsAny<CancellationToken>()), Times.Once);
        _mockEmail.Verify(x => x.SendEmailAsync("general@e.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task SendEmailByCodeAsync_ShouldSubstituteOrgLevelVariables()
    {
        // Org variables must resolve on the SendEmailByCodeAsync path (used by SendIndividualEmailAsync),
        // not just the fuller SendTemplatedEmailAsync path — both now share BuildTemplateVariables.
        var member = new Member
        {
            FullName = "Org Var Tester",
            Email = "orgvar@e.com",
            NID = "1",
            MobileNo = "0",
            FatherName = "F",
            MotherName = "M",
            PresentAddress = "A",
            PermanentAddress = "A",
            EmergencyContactName = "E",
            EmergencyContactRelation = "R",
            EmergencyContactPhone = "0"
        };
        _context.Members.Add(member);
        _context.EmailTemplates.Add(new EmailTemplate
        {
            Code = "ORG_VARS",
            Subject = "Welcome to {{OrgName}}",
            Body = "Contact us: {{SupportEmail}} or visit {{PortalUrl}} ({{CurrentYear}})",
            Description = "Org var test"
        });
        await _context.SaveChangesAsync();

        await _service.SendIndividualEmailAsync(member.Id, "ORG_VARS");

        _mockEmail.Verify(x => x.SendEmailAsync(
            "orgvar@e.com",
            $"Welcome to {_mockConfig.Branding.FullName}",
            It.Is<string>(b => b.Contains(_mockConfig.Contact.SupportEmail) && b.Contains(_mockConfig.Contact.PortalBaseUrl) && b.Contains(DateTime.UtcNow.Year.ToString())),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SendTemplatedEmailAsync_ShouldHtmlEncodeMemberSuppliedValues()
    {
        // A member with markup in FullName must not have it injected raw into the email HTML.
        var year = 1999;
        var member = new Member
        {
            FullName = "<script>alert(1)</script>",
            Email = "xss@e.com",
            NID = "1",
            MobileNo = "0",
            FatherName = "F",
            MotherName = "M",
            PresentAddress = "A",
            PermanentAddress = "A",
            EmergencyContactName = "E",
            EmergencyContactRelation = "R",
            EmergencyContactPhone = "0",
            AcademicHistory = new List<AcademicRecord> { new AcademicRecord { IsGHC = true, PassingYear = year, InstitutionName = "GHC", Degree = "HSC", Subject = "Science" } }
        };
        _context.Members.Add(member);
        _context.EmailTemplates.Add(new EmailTemplate { Code = "XSS_TEST", Subject = "S", Body = "Hello {{FullName}}", Description = "XSS test" });
        await _context.SaveChangesAsync();

        await _service.SendBatchEmailAsync(new List<int> { year }, "XSS_TEST");

        _mockEmail.Verify(x => x.SendEmailAsync(
            "xss@e.com",
            "S",
            It.Is<string>(b => !b.Contains("<script>") && b.Contains("&lt;script&gt;")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SmsChannelTemplate_ShouldRoundTripThroughCreateUpdateGet()
    {
        // Abstraction check: an Sms-channel template must persist and read back correctly even
        // though no send path is wired for it yet.
        var created = await _service.CreateTemplateAsync(new EmailTemplate
        {
            Code = "SMS_TEST",
            Channel = MessageChannel.Sms,
            Subject = "",
            Body = "Your OTP is {{OtpCode}}",
            Description = "SMS OTP test"
        });

        created.Channel.Should().Be(MessageChannel.Sms);

        created.Body = "Your OTP code is {{OtpCode}}";
        var updated = await _service.UpdateTemplateAsync(created);
        updated.Channel.Should().Be(MessageChannel.Sms);
        updated.Body.Should().Be("Your OTP code is {{OtpCode}}");

        var fetched = await _service.GetTemplateByCodeAsync("SMS_TEST");
        fetched.Should().NotBeNull();
        fetched!.Channel.Should().Be(MessageChannel.Sms);
    }
}
