using System.Threading.Tasks;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
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

        [SetUp]
        public void ServiceSetup()
        {
            _service = new SocialAuthConfigService(_context);
        }

        [Test]
        public async Task GetEnabledAsync_ReturnsOnlyEnabledConfigs()
        {
            _context.SocialAuthConfigs.Add(new SocialAuthConfig { Provider = SocialProvider.Google, ClientId = "g-id", IsEnabled = true });
            _context.SocialAuthConfigs.Add(new SocialAuthConfig { Provider = SocialProvider.Facebook, ClientId = "f-id", IsEnabled = false });
            await _context.SaveChangesAsync();

            var enabled = await _service.GetEnabledAsync();

            Assert.That(enabled, Has.Count.EqualTo(1));
            Assert.That(enabled[0].Provider, Is.EqualTo(SocialProvider.Google));
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
