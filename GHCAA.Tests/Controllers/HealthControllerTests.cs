using GHCAA.API.Controllers;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace GHCAA.Tests.Controllers
{
    // 80.13: first coverage for this controller — previously untested, and now rewritten to
    // depend on IDatabaseHealthService instead of ApplicationDbContext directly.
    [TestFixture]
    public class HealthControllerTests
    {
        private Mock<IDatabaseHealthService> _dbHealthMock = null!;
        private Mock<IEmailService> _emailMock = null!;
        private IConfiguration _config = null!;
        private Mock<ILogger<HealthController>> _loggerMock = null!;
        private HealthController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _dbHealthMock = new Mock<IDatabaseHealthService>();
            _emailMock = new Mock<IEmailService>();
            _config = new ConfigurationBuilder().Build();
            _loggerMock = new Mock<ILogger<HealthController>>();
            _controller = new HealthController(_dbHealthMock.Object, _emailMock.Object, _config, _loggerMock.Object);
        }

        [Test]
        public async Task GetHealth_ReturnsDegraded_WhenDatabaseCannotConnect()
        {
            _dbHealthMock.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.GetHealth(CancellationToken.None);

            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo(503));
        }

        [Test]
        public async Task GetHealth_ReturnsDegraded_WhenDatabaseProbeThrows()
        {
            _dbHealthMock.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new System.Exception("connection failure"));

            var result = await _controller.GetHealth(CancellationToken.None);

            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo(503));
        }

        [TestCase("Brevo", "key", "sender@example.com", 200)]
        [TestCase("brevo", "key", "sender@example.com", 200)]
        [TestCase("Brevo", null, "sender@example.com", 503)]
        [TestCase("Brevo", "key", null, 503)]
        public async Task GetHealth_WithBrevoProvider_ChecksBrevoSettings(string provider, string? apiKey, string? sender, int expected)
        {
            // Gmail keys are left unset on purpose: with Brevo picked they must not matter.
            var controller = HealthyController(new Dictionary<string, string?>
            {
                ["EmailSettings:Provider"] = provider,
                ["BrevoSettings:ApiKey"] = apiKey,
                ["BrevoSettings:SenderEmail"] = sender
            });

            var result = await controller.GetHealth(CancellationToken.None);

            Assert.That(StatusOf(result), Is.EqualTo(expected));
        }

        [TestCase(null, 200)]
        [TestCase("Gmail", 200)]
        public async Task GetHealth_WithGmailProvider_ChecksGmailSettings(string? provider, int expected)
        {
            var controller = HealthyController(new Dictionary<string, string?>
            {
                ["EmailSettings:Provider"] = provider,
                ["GmailSettings:Email"] = "test@gmail.com",
                ["GmailSettings:AppPassword"] = "password"
            });

            var result = await controller.GetHealth(CancellationToken.None);

            Assert.That(StatusOf(result), Is.EqualTo(expected));
        }

        [Test]
        public async Task GetHealth_WithGmailProvider_ReturnsDegraded_WhenOnlyBrevoIsSet()
        {
            var controller = HealthyController(new Dictionary<string, string?>
            {
                ["BrevoSettings:ApiKey"] = "key",
                ["BrevoSettings:SenderEmail"] = "sender@example.com"
            });

            var result = await controller.GetHealth(CancellationToken.None);

            Assert.That(StatusOf(result), Is.EqualTo(503));
        }

        // Database and storage pass, so the status code only depends on the email settings.
        private HealthController HealthyController(Dictionary<string, string?> settings)
        {
            _dbHealthMock.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            settings["FileStorage:BasePhysicalPath"] = Path.GetTempPath();
            var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            return new HealthController(_dbHealthMock.Object, _emailMock.Object, config, _loggerMock.Object);
        }

        private static int? StatusOf(IActionResult result) => result switch
        {
            OkObjectResult ok => ok.StatusCode,
            ObjectResult obj => obj.StatusCode,
            _ => null
        };
    }
}
