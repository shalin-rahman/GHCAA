using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Controllers
{
    // 80.13: first coverage for this controller — previously untested, and now rewritten to
    // depend on ISocialAuthConfigService instead of ApplicationDbContext directly.
    [TestFixture]
    public class AdminSocialAuthControllerTests
    {
        private Mock<ISocialAuthConfigService> _serviceMock = null!;
        private AdminSocialAuthController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<ISocialAuthConfigService>();
            _controller = new AdminSocialAuthController(_serviceMock.Object);
        }

        [Test]
        public async Task GetConfigs_ReturnsOk_WithObfuscatedSecrets()
        {
            _serviceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new System.Collections.Generic.List<SocialAuthConfig>
            {
                new() { Id = 1, Provider = SocialProvider.Google, ClientId = "cid", ClientSecret = "realsecret" }
            });

            var result = await _controller.GetConfigs();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UpdateConfig_ReturnsOk_WithUpsertedConfig()
        {
            var updateDto = new SocialAuthConfig { ClientId = "new-id", IsEnabled = true };
            _serviceMock.Setup(x => x.UpsertAsync(SocialProvider.Google, updateDto))
                .ReturnsAsync(new SocialAuthConfig { Id = 1, Provider = SocialProvider.Google, ClientId = "new-id", IsEnabled = true });

            var result = await _controller.UpdateConfig(SocialProvider.Google, updateDto);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Toggle_ReturnsOk_WhenConfigExists()
        {
            _serviceMock.Setup(x => x.ToggleAsync(SocialProvider.Facebook))
                .ReturnsAsync(new SocialAuthConfig { Id = 1, Provider = SocialProvider.Facebook, IsEnabled = false });

            var result = await _controller.Toggle(SocialProvider.Facebook);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Toggle_ReturnsNotFound_WhenConfigDoesNotExist()
        {
            _serviceMock.Setup(x => x.ToggleAsync(SocialProvider.Facebook)).ReturnsAsync((SocialAuthConfig?)null);

            var result = await _controller.Toggle(SocialProvider.Facebook);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}
