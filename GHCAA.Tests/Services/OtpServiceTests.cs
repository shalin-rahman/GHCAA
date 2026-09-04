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
        _mockConfig.Setup(x => x["Jwt:Key"]).Returns("otp-service-test-dummy-hmac-key-please-32chars");
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
        // 24.20: Code is stored as HMAC-SHA256 hex (64 chars), not the returned plaintext code.
        otp!.Code.Should().HaveLength(64).And.MatchRegex(@"^[0-9a-f]{64}$");
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

    // "invalid" falls back to the 10-minute default; "5" is honored as a real custom value.
    [TestCase("5", "otp4@example.com", 4, 6)]
    [TestCase("invalid", "otp5@example.com", 9, 11)]
    public async Task GenerateAndSendOtpAsync_ShouldRespectConfiguredExpiry_OrFallBackToDefault(
        string configValue, string email, int minMinutes, int maxMinutes)
    {
        _mockConfig.Setup(x => x["OtpSettings:ExpiryMinutes"]).Returns(configValue);
        var service = new OtpService(_context, _mockCommunication.Object, _mockConfig.Object, _mockLogger.Object);
        var before = DateTime.UtcNow;
        await service.GenerateAndSendOtpAsync(email);
        var after = DateTime.UtcNow;

        var otp = await _context.Otps.FirstOrDefaultAsync(o => o.Email == email);
        otp!.ExpiryAt.Should().BeAfter(before.AddMinutes(minMinutes));
        otp.ExpiryAt.Should().BeBefore(after.AddMinutes(maxMinutes));
    }

    [Test]
    public async Task VerifyOtpAsync_WithValidOtp_ShouldReturnTrue()
    {
        var email = "otp6@example.com";
        var code = await _service.GenerateAndSendOtpAsync(email);
        var result = await _service.VerifyOtpAsync(email, code);

        result.Should().BeTrue();
        // 24.20: Code stored as HMAC hash; query by email + IsVerified flag instead.
        var otp = await _context.Otps.FirstOrDefaultAsync(o => o.Email == email && o.IsVerified == true);
        otp.Should().NotBeNull();
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

        // 24.20: Code stored as HMAC hash; find by email + IsVerified flag.
        var verifiedOtp = await _context.Otps.Where(o => o.Email == email && o.IsVerified == true).FirstOrDefaultAsync();
        verifiedOtp.Should().NotBeNull();
        verifiedOtp!.IsVerified.Should().BeTrue();
    }
}
