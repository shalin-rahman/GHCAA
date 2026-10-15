using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services
{
    // 80.13: extracted from PaymentConfigController so it doesn't touch ApplicationDbContext
    // directly. GatewaysController's GetEnabledByGatewayAsync usage is also covered here.
    [TestFixture]
    public class PaymentConfigServiceTests : TestBase
    {
        private PaymentConfigService _service = null!;

        [SetUp]
        public void ServiceSetup()
        {
            _context.PaymentConfigurations.RemoveRange(_context.PaymentConfigurations);
            _context.SaveChanges();
            _service = new PaymentConfigService(_context);
        }

        [Test]
        public async Task GetActiveMethodsAsync_ReturnsOnlyEnabled_OrderedBySortOrder()
        {
            _context.PaymentConfigurations.AddRange(
                new PaymentConfiguration { DisplayName = "B", IsEnabled = true, SortOrder = 2, Method = PaymentMethod.Nagad },
                new PaymentConfiguration { DisplayName = "A", IsEnabled = true, SortOrder = 1, Method = PaymentMethod.BKash },
                new PaymentConfiguration { DisplayName = "C", IsEnabled = false, SortOrder = 0, Method = PaymentMethod.Rocket });
            await _context.SaveChangesAsync();

            var result = await _service.GetActiveMethodsAsync();

            Assert.That(result.Select(r => r.DisplayName), Is.EqualTo(new[] { "A", "B" }));
        }

        [Test]
        public async Task CreateAsync_PersistsConfig_WithCreatedAtSet()
        {
            var created = await _service.CreateAsync(new PaymentConfiguration { DisplayName = "New", Method = PaymentMethod.BankTransfer });

            Assert.That(created.CreatedAt, Is.Not.EqualTo(default(System.DateTime)));
            Assert.That(await _context.PaymentConfigurations.CountAsync(), Is.EqualTo(1));
        }

        // 62.56g: Method + Gateway is unique; Method alone is not.
        [Test]
        [Category("FR-42")]
        public async Task CreateAsync_Throws_WhenMethodAndGatewayTaken()
        {
            await _service.CreateAsync(new PaymentConfiguration { DisplayName = "bKash", Method = PaymentMethod.BKash });

            Assert.ThrowsAsync<DuplicateKeyException>(() =>
                _service.CreateAsync(new PaymentConfiguration { DisplayName = "bKash again", Method = PaymentMethod.BKash }));
            Assert.That(await _context.PaymentConfigurations.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        [Category("FR-42")]
        public async Task CreateAsync_AllowsSameMethod_OnAnotherGateway()
        {
            await _service.CreateAsync(new PaymentConfiguration { DisplayName = "Card A", Method = PaymentMethod.CreditCard, Gateway = PaymentGateway.SSLCommerz });

            await _service.CreateAsync(new PaymentConfiguration { DisplayName = "Card B", Method = PaymentMethod.CreditCard, Gateway = PaymentGateway.Stripe });

            Assert.That(await _context.PaymentConfigurations.CountAsync(), Is.EqualTo(2));
        }

        [Test]
        [Category("FR-42")]
        public async Task UpdateAsync_Throws_WhenGatewayMovesOntoAnotherRowsKey()
        {
            await _service.CreateAsync(new PaymentConfiguration { DisplayName = "Card A", Method = PaymentMethod.CreditCard, Gateway = PaymentGateway.SSLCommerz });
            var other = await _service.CreateAsync(new PaymentConfiguration { DisplayName = "Card B", Method = PaymentMethod.CreditCard, Gateway = PaymentGateway.Stripe });

            Assert.ThrowsAsync<DuplicateKeyException>(() =>
                _service.UpdateAsync(other.Id, new PaymentConfiguration { DisplayName = "Card B", Gateway = PaymentGateway.SSLCommerz }, allowSecretUpdate: false));
        }

        [Test]
        [Category("FR-42")]
        public async Task UpdateAsync_KeepingItsOwnKey_Succeeds()
        {
            var created = await _service.CreateAsync(new PaymentConfiguration { DisplayName = "bKash", Method = PaymentMethod.BKash });

            var updated = await _service.UpdateAsync(created.Id, new PaymentConfiguration { DisplayName = "bKash renamed" }, allowSecretUpdate: false);

            Assert.That(updated!.DisplayName, Is.EqualTo("bKash renamed"));
        }

        [Test]
        public async Task UpdateAsync_DoesNotChangeSecrets_WhenSecretUpdateNotAllowed()
        {
            var original = new PaymentConfiguration { DisplayName = "Old", GatewaySecretKey = "OldSec", Method = PaymentMethod.CreditCard };
            _context.PaymentConfigurations.Add(original);
            await _context.SaveChangesAsync();

            var updated = await _service.UpdateAsync(original.Id, new PaymentConfiguration { DisplayName = "New", GatewaySecretKey = "AttemptedNew" }, allowSecretUpdate: false);

            Assert.That(updated!.DisplayName, Is.EqualTo("New"));
            Assert.That(updated.GatewaySecretKey, Is.EqualTo("OldSec"));
        }

        [Test]
        public async Task UpdateAsync_ChangesSecrets_WhenSecretUpdateAllowedAndNotPlaceholder()
        {
            var original = new PaymentConfiguration { DisplayName = "Old", GatewaySecretKey = "OldSec", Method = PaymentMethod.CreditCard };
            _context.PaymentConfigurations.Add(original);
            await _context.SaveChangesAsync();

            var updated = await _service.UpdateAsync(original.Id, new PaymentConfiguration { DisplayName = "New", GatewaySecretKey = "RealNewSecret" }, allowSecretUpdate: true);

            Assert.That(updated!.GatewaySecretKey, Is.EqualTo("RealNewSecret"));
        }

        [Test]
        public async Task UpdateAsync_ReturnsNull_WhenConfigDoesNotExist()
        {
            var updated = await _service.UpdateAsync(999, new PaymentConfiguration(), allowSecretUpdate: false);

            Assert.That(updated, Is.Null);
        }

        [Test]
        public async Task ToggleAsync_FlipsIsEnabled()
        {
            var config = new PaymentConfiguration { DisplayName = "T", IsEnabled = true, Method = PaymentMethod.BKash };
            _context.PaymentConfigurations.Add(config);
            await _context.SaveChangesAsync();

            var result = await _service.ToggleAsync(config.Id);

            Assert.That(result!.IsEnabled, Is.False);
        }

        [Test]
        public async Task DeleteAsync_RemovesConfig_ReturnsTrue()
        {
            var config = new PaymentConfiguration { DisplayName = "D", Method = PaymentMethod.BKash };
            _context.PaymentConfigurations.Add(config);
            await _context.SaveChangesAsync();

            var deleted = await _service.DeleteAsync(config.Id);

            Assert.That(deleted, Is.True);
            Assert.That(await _context.PaymentConfigurations.CountAsync(), Is.EqualTo(0));
        }

        [Test]
        public async Task DeleteAsync_ReturnsFalse_WhenConfigDoesNotExist()
        {
            var deleted = await _service.DeleteAsync(999);

            Assert.That(deleted, Is.False);
        }

        [Test]
        public async Task HasAnyAsync_ReflectsCurrentState()
        {
            Assert.That(await _service.HasAnyAsync(), Is.False);

            _context.PaymentConfigurations.Add(new PaymentConfiguration { DisplayName = "X", Method = PaymentMethod.BKash });
            await _context.SaveChangesAsync();

            Assert.That(await _service.HasAnyAsync(), Is.True);
        }

        [Test]
        public async Task SeedDefaultsAsync_CreatesSixDefaultMethods()
        {
            var defaults = await _service.SeedDefaultsAsync();

            Assert.That(defaults, Has.Count.EqualTo(6));
            Assert.That(await _context.PaymentConfigurations.CountAsync(), Is.EqualTo(6));
        }

        [Test]
        public async Task GetEnabledByGatewayAsync_ReturnsMatch_WhenEnabled()
        {
            _context.PaymentConfigurations.Add(new PaymentConfiguration
            {
                DisplayName = "SSL",
                Method = PaymentMethod.CreditCard,
                Gateway = PaymentGateway.SSLCommerz,
                IsEnabled = true
            });
            await _context.SaveChangesAsync();

            var result = await _service.GetEnabledByGatewayAsync(PaymentGateway.SSLCommerz);

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task GetEnabledByGatewayAsync_ReturnsNull_WhenDisabled()
        {
            _context.PaymentConfigurations.Add(new PaymentConfiguration
            {
                DisplayName = "SSL",
                Method = PaymentMethod.CreditCard,
                Gateway = PaymentGateway.SSLCommerz,
                IsEnabled = false
            });
            await _context.SaveChangesAsync();

            var result = await _service.GetEnabledByGatewayAsync(PaymentGateway.SSLCommerz);

            Assert.That(result, Is.Null);
        }
    }
}
