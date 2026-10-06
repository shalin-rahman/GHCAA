using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services
{
    // 80.13: extracted from AdminSocialAuthController/AuthController so neither touches
    // ApplicationDbContext directly.
    [TestFixture]
    public class SocialAuthConfigServiceTests : TestBase
    {
        private SocialAuthConfigService _service = null!;
        private bool _socialAuthOn;

        [SetUp]
        public void ServiceSetup()
        {
            _socialAuthOn = true;
            var orgConfig = new Mock<IOrgConfigService>();
            orgConfig.Setup(o => o.GetConfigAsync())
                .ReturnsAsync(() => new OrgConfigDto { Features = new FeatureToggleDto { EnableSocialAuth = _socialAuthOn } });
            _service = new SocialAuthConfigService(_context, orgConfig.Object);
        }

        // 7.17, spec 012 FR-034.
        [Test]
        public async Task GetUsableAsync_ReturnsOnlyEnabledConfigsWithKeys()
        {
            _context.SocialAuthConfigs.Add(new SocialAuthConfig { Provider = SocialProvider.Google, ClientId = "g-id", IsEnabled = true });
            _context.SocialAuthConfigs.Add(new SocialAuthConfig { Provider = SocialProvider.Facebook, ClientId = "f-id", ClientSecret = "f-secret", IsEnabled = false });
            await _context.SaveChangesAsync();

            var usable = await _service.GetUsableAsync();

            Assert.That(usable, Has.Count.EqualTo(1));
            Assert.That(usable[0].Provider, Is.EqualTo(SocialProvider.Google));
        }

        [Test]
        public async Task GetUsableAsync_FeatureOff_IsEmptyEvenWithEnabledRows()
        {
            _socialAuthOn = false;
            _context.SocialAuthConfigs.Add(new SocialAuthConfig { Provider = SocialProvider.Google, ClientId = "g-id", IsEnabled = true });
            await _context.SaveChangesAsync();

            Assert.That(await _service.GetUsableAsync(), Is.Empty);
            Assert.That(await _service.FindUsableAsync(SocialProvider.Google), Is.Null);
        }

        [Test]
        public async Task FindUsableAsync_MissingKeys_IsNull()
        {
            _context.SocialAuthConfigs.Add(new SocialAuthConfig { Provider = SocialProvider.Google, ClientId = " ", IsEnabled = true });
            _context.SocialAuthConfigs.Add(new SocialAuthConfig { Provider = SocialProvider.Facebook, ClientId = "f-id", IsEnabled = true });
            await _context.SaveChangesAsync();

            Assert.That(await _service.FindUsableAsync(SocialProvider.Google), Is.Null);
            Assert.That(await _service.FindUsableAsync(SocialProvider.Facebook), Is.Null);
        }

        [Test]
        public async Task FindUsableAsync_FacebookWithSecret_IsReturned()
        {
            _context.SocialAuthConfigs.Add(new SocialAuthConfig { Provider = SocialProvider.Facebook, ClientId = "f-id", ClientSecret = "f-secret", IsEnabled = true });
            await _context.SaveChangesAsync();

            Assert.That((await _service.FindUsableAsync(SocialProvider.Facebook))?.ClientId, Is.EqualTo("f-id"));
        }

        [Test]
        public async Task UpsertAsync_CreatesNewConfig_WhenNoneExistsForProvider()
        {
            var result = await _service.UpsertAsync(SocialProvider.Google, new SocialAuthConfig
            {
                ClientId = "cid",
                ClientSecret = "secret",
                IsEnabled = true
            });

            Assert.That(result.Provider, Is.EqualTo(SocialProvider.Google));
            Assert.That(result.ClientId, Is.EqualTo("cid"));
            Assert.That(await _context.SocialAuthConfigs.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task UpsertAsync_UpdatesExistingConfig_WhenOneAlreadyExistsForProvider()
        {
            _context.SocialAuthConfigs.Add(new SocialAuthConfig { Provider = SocialProvider.Google, ClientId = "old", IsEnabled = false });
            await _context.SaveChangesAsync();

            var result = await _service.UpsertAsync(SocialProvider.Google, new SocialAuthConfig { ClientId = "new", IsEnabled = true });

            Assert.That(result.ClientId, Is.EqualTo("new"));
            Assert.That(result.IsEnabled, Is.True);
            Assert.That(await _context.SocialAuthConfigs.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task ToggleAsync_FlipsIsEnabled_WhenConfigExists()
        {
            _context.SocialAuthConfigs.Add(new SocialAuthConfig { Provider = SocialProvider.Facebook, ClientId = "f-id", IsEnabled = true });
            await _context.SaveChangesAsync();

            var result = await _service.ToggleAsync(SocialProvider.Facebook);

            Assert.That(result!.IsEnabled, Is.False);
        }

        [Test]
        public async Task ToggleAsync_ReturnsNull_WhenConfigDoesNotExist()
        {
            var result = await _service.ToggleAsync(SocialProvider.Google);

            Assert.That(result, Is.Null);
        }
    }
}
