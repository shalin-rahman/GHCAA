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
    private CommunicationService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockEmail = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<CommunicationService>>();
        _service = new CommunicationService(_context, _mockEmail.Object, _mockLogger.Object);
    }

    [Test]
    public async Task SendIndividualEmailAsync_ShouldReplacePlaceholders()
    {
        // Arrange
        var member = new Member
        {
            FullName = "John Doe",
            Email = "john@example.com",
            NID = "123", MobileNo = "01", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="None", GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="None", ProfessionalSector = "I", Designation = "D"
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

    [Test]
    public async Task SendBatchEmailAsync_ShouldSendMultipleEmails()
    {
        // Arrange
        var members = new List<Member>
        {
            new Member { FullName = "A", Email = "a@e.com", GHCLastCertificatePassingYear = 2005, NID = "1", MobileNo = "0", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="None", GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="None", ProfessionalSector = "I", Designation = "D" },
            new Member { FullName = "B", Email = "b@e.com", GHCLastCertificatePassingYear = 2005, NID = "2", MobileNo = "01", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="None", GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="None", ProfessionalSector = "I", Designation = "D" }
        };
        _context.Members.AddRange(members);
        _context.EmailTemplates.Add(new EmailTemplate { Code = "BATCH", Subject = "S", Body = "B", Description = "Batch test template" });
        await _context.SaveChangesAsync();

        // Act
        await _service.SendBatchEmailAsync(new List<int> { 2005 }, "BATCH");

        // Assert — body will have footer appended, so use Contains match
        _mockEmail.Verify(x => x.SendEmailAsync(It.IsAny<string>(), "S", It.Is<string>(b => b.Contains("B")), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public async Task SendBatchCustomEmailAsync_ShouldSendToCorrectYear()
    {
        // Arrange
        var members = new List<Member>
        {
            new Member { FullName = "2005-A", Email = "2005a@e.com", GHCLastCertificatePassingYear = 2005, NID = "1", MobileNo = "0", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="None", GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="None", ProfessionalSector = "I", Designation = "D" },
            new Member { FullName = "2010-B", Email = "2010b@e.com", GHCLastCertificatePassingYear = 2010, NID = "2", MobileNo = "01", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="None", GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="None", ProfessionalSector = "I", Designation = "D" }
        };
        _context.Members.AddRange(members);
        await _context.SaveChangesAsync();

        // Act
        await _service.SendBatchCustomEmailAsync(new List<int> { 2005 }, "Manual Subject", "Manual Body");

        // Assert
        _mockEmail.Verify(x => x.SendEmailAsync("2005a@e.com", "Manual Subject", It.Is<string>(b => b.Contains("Manual Body")), It.IsAny<CancellationToken>()), Times.Once);
        _mockEmail.Verify(x => x.SendEmailAsync("2010b@e.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task SendTypeCustomEmailAsync_ShouldSendToCorrectMembershipType()
    {
        // Arrange
        var members = new List<Member>
        {
            new Member { FullName = "Exec", Email = "exec@e.com", MembershipType = MembershipType.Executive, NID = "1", MobileNo = "0", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="None", GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="None", ProfessionalSector = "I", Designation = "D" },
            new Member { FullName = "General", Email = "general@e.com", MembershipType = MembershipType.General, NID = "2", MobileNo = "01", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="None", GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="None", ProfessionalSector = "I", Designation = "D" }
        };
        _context.Members.AddRange(members);
        await _context.SaveChangesAsync();

        // Act
        await _service.SendTypeCustomEmailAsync(new List<string> { "Executive" }, "Type Subject", "Type Body");

        // Assert
        _mockEmail.Verify(x => x.SendEmailAsync("exec@e.com", "Type Subject", It.Is<string>(b => b.Contains("Type Body")), It.IsAny<CancellationToken>()), Times.Once);
        _mockEmail.Verify(x => x.SendEmailAsync("general@e.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
