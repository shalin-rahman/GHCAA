using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Tests.Services;

[TestFixture]
public class OtpServiceTests : TestBase
{
    private Mock<ICommunicationService> _mockCommunication = null!;
    private Mock<IConfiguration> _mockConfig = null!;
    private Mock<ILogger<OtpService>> _mockLogger = null!;
    private OtpService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockCommunication = new Mock<ICommunicationService>();
        _mockConfig = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<OtpService>>();
        _mockConfig.Setup(x => x["OtpSettings:ExpiryMinutes"]).Returns("10");
        _service = new OtpService(_context, _mockCommunication.Object, _mockConfig.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GenerateAndSendOtpAsync_ShouldCreateOtpInDatabase()
    {
        var email = "otp1@example.com";
        var code = await _service.GenerateAndSendOtpAsync(email);

        code.Should().NotBeNullOrEmpty();
        code.Should().HaveLength(6);
        code.Should().MatchRegex(@"^\d{6}$");

        var otp = await _context.Otps.FirstOrDefaultAsync(o => o.Email == email);
        otp.Should().NotBeNull();
        otp!.Code.Should().Be(code);
        otp.IsVerified.Should().BeFalse();
    }

    [Test]
    public async Task GenerateAndSendOtpAsync_ShouldSetCorrectExpiryTime()
    {
        var email = "otp2@example.com";
        var beforeGeneration = DateTime.UtcNow;
        await _service.GenerateAndSendOtpAsync(email);
        var afterGeneration = DateTime.UtcNow;

        var otp = await _context.Otps.FirstOrDefaultAsync(o => o.Email == email);
        otp!.ExpiryAt.Should().BeAfter(beforeGeneration.AddMinutes(9));
        otp.ExpiryAt.Should().BeBefore(afterGeneration.AddMinutes(11));
    }

    [Test]
    public async Task GenerateAndSendOtpAsync_ShouldSendEmail()
    {
        var email = "otp3@example.com";
        await _service.GenerateAndSendOtpAsync(email);

        _mockCommunication.Verify(x => x.SendEmailByCodeAsync(
            email, "OTP_EMAIL", It.IsAny<Dictionary<string, string>>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GenerateAndSendOtpAsync_WithCustomExpiryMinutes_ShouldUseCustomValue()
    {
        _mockConfig.Setup(x => x["OtpSettings:ExpiryMinutes"]).Returns("5");
        var service = new OtpService(_context, _mockCommunication.Object, _mockConfig.Object, _mockLogger.Object);
        var email = "otp4@example.com";
        var before = DateTime.UtcNow;
        await service.GenerateAndSendOtpAsync(email);
        var after = DateTime.UtcNow;

        var otp = await _context.Otps.FirstOrDefaultAsync(o => o.Email == email);
        otp!.ExpiryAt.Should().BeAfter(before.AddMinutes(4));
        otp.ExpiryAt.Should().BeBefore(after.AddMinutes(6));
    }

    [Test]
    public async Task GenerateAndSendOtpAsync_WithInvalidConfigValue_ShouldUseDefaultExpiry()
    {
        _mockConfig.Setup(x => x["OtpSettings:ExpiryMinutes"]).Returns("invalid");
        var service = new OtpService(_context, _mockCommunication.Object, _mockConfig.Object, _mockLogger.Object);
        var email = "otp5@example.com";
        var before = DateTime.UtcNow;
        await service.GenerateAndSendOtpAsync(email);
        var after = DateTime.UtcNow;

        var otp = await _context.Otps.FirstOrDefaultAsync(o => o.Email == email);
        otp!.ExpiryAt.Should().BeAfter(before.AddMinutes(9));
        otp.ExpiryAt.Should().BeBefore(after.AddMinutes(11));
    }

    [Test]
    public async Task VerifyOtpAsync_WithValidOtp_ShouldReturnTrue()
    {
        var email = "otp6@example.com";
        var code = await _service.GenerateAndSendOtpAsync(email);
        var result = await _service.VerifyOtpAsync(email, code);

        result.Should().BeTrue();
        var otp = await _context.Otps.FirstOrDefaultAsync(o => o.Email == email && o.Code == code);
        otp!.IsVerified.Should().BeTrue();
    }

    [Test]
    public async Task VerifyOtpAsync_WithInvalidCode_ShouldReturnFalse()
    {
        var email = "otp7@example.com";
        await _service.GenerateAndSendOtpAsync(email);
        var result = await _service.VerifyOtpAsync(email, "000000");
        result.Should().BeFalse();
    }

    [Test]
    public async Task VerifyOtpAsync_WithExpiredOtp_ShouldReturnFalse()
    {
        var email = "otp8@example.com";
        _context.Otps.Add(new Otp { Email = email, Code = "123456", ExpiryAt = DateTime.UtcNow.AddMinutes(-1), IsVerified = false });
        await _context.SaveChangesAsync();
        var result = await _service.VerifyOtpAsync(email, "123456");
        result.Should().BeFalse();
    }

    [Test]
    public async Task VerifyOtpAsync_WithAlreadyVerifiedOtp_ShouldReturnFalse()
    {
        var email = "otp9@example.com";
        var code = await _service.GenerateAndSendOtpAsync(email);
        await _service.VerifyOtpAsync(email, code);
        var result = await _service.VerifyOtpAsync(email, code);
        result.Should().BeFalse();
    }

    [Test]
    public async Task VerifyOtpAsync_WithWrongEmail_ShouldReturnFalse()
    {
        var email = "otp10@example.com";
        var code = await _service.GenerateAndSendOtpAsync(email);
        var result = await _service.VerifyOtpAsync("wrong@example.com", code);
        result.Should().BeFalse();
    }

    [Test]
    public async Task VerifyOtpAsync_WithMultipleOtps_ShouldVerifyMostRecent()
    {
        var email = "otp11@example.com";
        var oldCode = await _service.GenerateAndSendOtpAsync(email);
        await Task.Delay(100);
        var newCode = await _service.GenerateAndSendOtpAsync(email);
        var result = await _service.VerifyOtpAsync(email, newCode);
        result.Should().BeTrue();

        var verifiedOtp = await _context.Otps.Where(o => o.Email == email && o.Code == newCode).FirstOrDefaultAsync();
        verifiedOtp!.IsVerified.Should().BeTrue();
    }
}
