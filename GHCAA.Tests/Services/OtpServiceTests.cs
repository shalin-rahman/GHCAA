using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace GHCAA.Tests.Services;

[TestFixture]
public class OtpServiceTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IEmailService> _mockEmail = null!;
    private Mock<IConfiguration> _mockConfig = null!;
    private Mock<ILogger<OtpService>> _mockLogger = null!;
    private OtpService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockEmail = new Mock<IEmailService>();
        _mockConfig = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<OtpService>>();

        _mockConfig.Setup(x => x["OtpSettings:ExpiryMinutes"]).Returns("10");

        _service = new OtpService(_context, _mockEmail.Object, _mockConfig.Object, _mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task GenerateAndSendOtpAsync_ShouldCreateOtpInDatabase()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var code = await _service.GenerateAndSendOtpAsync(email);

        // Assert
        code.Should().NotBeNullOrEmpty();
        code.Should().HaveLength(6);
        code.Should().MatchRegex(@"^\d{6}$");

        var otp = await _context.Otps.FirstOrDefaultAsync(o => o.Email == email);
        otp.Should().NotBeNull();
        otp!.Code.Should().Be(code);
        otp.Email.Should().Be(email);
        otp.IsVerified.Should().BeFalse();
    }

    [Test]
    public async Task GenerateAndSendOtpAsync_ShouldSetCorrectExpiryTime()
    {
        // Arrange
        var email = "test@example.com";
        var beforeGeneration = DateTime.UtcNow;

        // Act
        await _service.GenerateAndSendOtpAsync(email);
        var afterGeneration = DateTime.UtcNow;

        // Assert
        var otp = await _context.Otps.FirstOrDefaultAsync(o => o.Email == email);
        otp.Should().NotBeNull();
        otp!.ExpiryAt.Should().BeAfter(beforeGeneration.AddMinutes(9));
        otp.ExpiryAt.Should().BeBefore(afterGeneration.AddMinutes(11));
    }

    [Test]
    public async Task GenerateAndSendOtpAsync_ShouldSendEmail()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        await _service.GenerateAndSendOtpAsync(email);

        // Assert
        _mockEmail.Verify(x => x.SendEmailAsync(
            email,
            "Your GHC Alumni OTP",
            It.Is<string>(body => body.Contains("verification code")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GenerateAndSendOtpAsync_WithCustomExpiryMinutes_ShouldUseCustomValue()
    {
        // Arrange
        _mockConfig.Setup(x => x["OtpSettings:ExpiryMinutes"]).Returns("5");
        var service = new OtpService(_context, _mockEmail.Object, _mockConfig.Object, _mockLogger.Object);
        var email = "test@example.com";
        var beforeGeneration = DateTime.UtcNow;

        // Act
        await service.GenerateAndSendOtpAsync(email);
        var afterGeneration = DateTime.UtcNow;

        // Assert
        var otp = await _context.Otps.FirstOrDefaultAsync(o => o.Email == email);
        otp!.ExpiryAt.Should().BeAfter(beforeGeneration.AddMinutes(4));
        otp.ExpiryAt.Should().BeBefore(afterGeneration.AddMinutes(6));
    }

    [Test]
    public async Task GenerateAndSendOtpAsync_WithInvalidConfigValue_ShouldUseDefaultExpiry()
    {
        // Arrange
        _mockConfig.Setup(x => x["OtpSettings:ExpiryMinutes"]).Returns("invalid");
        var service = new OtpService(_context, _mockEmail.Object, _mockConfig.Object, _mockLogger.Object);
        var email = "test@example.com";
        var beforeGeneration = DateTime.UtcNow;

        // Act
        await service.GenerateAndSendOtpAsync(email);
        var afterGeneration = DateTime.UtcNow;

        // Assert
        var otp = await _context.Otps.FirstOrDefaultAsync(o => o.Email == email);
        otp!.ExpiryAt.Should().BeAfter(beforeGeneration.AddMinutes(9));
        otp.ExpiryAt.Should().BeBefore(afterGeneration.AddMinutes(11));
    }

    [Test]
    public async Task VerifyOtpAsync_WithValidOtp_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        var code = await _service.GenerateAndSendOtpAsync(email);

        // Act
        var result = await _service.VerifyOtpAsync(email, code);

        // Assert
        result.Should().BeTrue();
        var otp = await _context.Otps.FirstOrDefaultAsync(o => o.Email == email && o.Code == code);
        otp!.IsVerified.Should().BeTrue();
    }

    [Test]
    public async Task VerifyOtpAsync_WithInvalidCode_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        await _service.GenerateAndSendOtpAsync(email);

        // Act
        var result = await _service.VerifyOtpAsync(email, "000000");

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task VerifyOtpAsync_WithExpiredOtp_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var otp = new Otp
        {
            Email = email,
            Code = "123456",
            ExpiryAt = DateTime.UtcNow.AddMinutes(-1),
            IsVerified = false
        };
        await _context.Otps.AddAsync(otp);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.VerifyOtpAsync(email, "123456");

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task VerifyOtpAsync_WithAlreadyVerifiedOtp_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var code = await _service.GenerateAndSendOtpAsync(email);
        await _service.VerifyOtpAsync(email, code);

        // Act
        var result = await _service.VerifyOtpAsync(email, code);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task VerifyOtpAsync_WithWrongEmail_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var code = await _service.GenerateAndSendOtpAsync(email);

        // Act
        var result = await _service.VerifyOtpAsync("wrong@example.com", code);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task VerifyOtpAsync_WithMultipleOtps_ShouldVerifyMostRecent()
    {
        // Arrange
        var email = "test@example.com";
        
        // Generate first OTP
        var oldCode = await _service.GenerateAndSendOtpAsync(email);
        await Task.Delay(100); // Ensure different timestamps
        
        // Generate second OTP
        var newCode = await _service.GenerateAndSendOtpAsync(email);

        // Act
        var result = await _service.VerifyOtpAsync(email, newCode);

        // Assert
        result.Should().BeTrue();
        var verifiedOtp = await _context.Otps
            .Where(o => o.Email == email && o.Code == newCode)
            .FirstOrDefaultAsync();
        verifiedOtp!.IsVerified.Should().BeTrue();
    }
}
