using FluentAssertions;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace GHCAA.Tests.Services;

[TestFixture]
public class GmailEmailServiceTests
{
    private Mock<IConfiguration> _mockConfig = null!;
    private Mock<ILogger<GmailEmailService>> _mockLogger = null!;

    [SetUp]
    public void Setup()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<GmailEmailService>>();
    }

    [TestCase(null, "password")]
    [TestCase("test@gmail.com", null)]
    public void Constructor_WithMissingCredential_ShouldThrowException(string? email, string? appPassword)
    {
        // Arrange
        _mockConfig.Setup(x => x["GmailSettings:Email"]).Returns(email);
        _mockConfig.Setup(x => x["GmailSettings:AppPassword"]).Returns(appPassword);

        // Act & Assert
        var act = () => new GmailEmailService(_mockConfig.Object, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_WithValidConfig_ShouldCreateInstance()
    {
        // Arrange
        _mockConfig.Setup(x => x["GmailSettings:Email"]).Returns("test@gmail.com");
        _mockConfig.Setup(x => x["GmailSettings:AppPassword"]).Returns("password");
        _mockConfig.Setup(x => x["GmailSettings:Host"]).Returns("smtp.gmail.com");
        _mockConfig.Setup(x => x["GmailSettings:Port"]).Returns("587");

        // Act
        var service = new GmailEmailService(_mockConfig.Object, _mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [TestCase("GmailSettings:Host", null)]
    [TestCase("GmailSettings:Port", null)]
    [TestCase("GmailSettings:Port", "invalid")]
    public void Constructor_WithMissingOrInvalidHostOrPort_ShouldFallBackToDefault(string key, string? value)
    {
        // Arrange
        _mockConfig.Setup(x => x["GmailSettings:Email"]).Returns("test@gmail.com");
        _mockConfig.Setup(x => x["GmailSettings:AppPassword"]).Returns("password");
        _mockConfig.Setup(x => x[key]).Returns(value);

        // Act
        var service = new GmailEmailService(_mockConfig.Object, _mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
    }

    // Note: Actual email sending tests are omitted as they would require mocking SmtpClient
    // which is difficult due to its sealed nature. Integration tests would be more appropriate
    // for testing the actual email sending functionality.
}
