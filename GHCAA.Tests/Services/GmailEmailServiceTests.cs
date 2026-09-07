using FluentAssertions;
using GHCAA.Infrastructure.Options;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace GHCAA.Tests.Services;

[TestFixture]
public class GmailEmailServiceTests
{
    private Mock<ILogger<GmailEmailService>> _mockLogger = null!;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<GmailEmailService>>();
    }

    [TestCase(null, "password")]
    [TestCase("test@gmail.com", null)]
    public void Constructor_WithMissingCredential_ShouldThrowException(string? email, string? appPassword)
    {
        var settings = Options.Create(new GmailSettingsOptions { Email = email, AppPassword = appPassword });

        var act = () => new GmailEmailService(settings, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_WithValidConfig_ShouldCreateInstance()
    {
        var settings = Options.Create(new GmailSettingsOptions
        {
            Email = "test@gmail.com",
            AppPassword = "password",
            Host = "smtp.gmail.com",
            Port = 587
        });

        var service = new GmailEmailService(settings, _mockLogger.Object);

        service.Should().NotBeNull();
    }

    [Test]
    public void Constructor_WithDefaultHostAndPort_ShouldCreateInstance()
    {
        // GmailSettingsOptions.Host/Port already default to smtp.gmail.com/587 when the section
        // omits them — this is what used to be the "fall back to default" behavior of the raw
        // IConfiguration reads.
        var settings = Options.Create(new GmailSettingsOptions
        {
            Email = "test@gmail.com",
            AppPassword = "password"
        });

        var service = new GmailEmailService(settings, _mockLogger.Object);

        service.Should().NotBeNull();
    }

    // Note: Actual email sending tests are omitted as they would require mocking SmtpClient
    // which is difficult due to its sealed nature. Integration tests would be more appropriate
    // for testing the actual email sending functionality.
}
