using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

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
    }
}
