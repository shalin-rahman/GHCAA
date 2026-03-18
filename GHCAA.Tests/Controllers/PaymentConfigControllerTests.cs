using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class PaymentConfigControllerTests
    {
        private ApplicationDbContext _dbContext;
        private PaymentConfigController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ApplicationDbContext(options);
            _controller = new PaymentConfigController(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        private void SetUserRole(string role)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.Role, role)
            }, "TestAuthentication"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task GetActiveMethods_ReturnsOnlyEnabled()
        {
            _dbContext.PaymentConfigurations.AddRange(new List<PaymentConfiguration>
            {
                new() { DisplayName = "P1", IsEnabled = true, Method = Domain.Enums.PaymentMethod.BKash },
                new() { DisplayName = "P2", IsEnabled = false, Method = Domain.Enums.PaymentMethod.Nagad }
            });
            await _dbContext.SaveChangesAsync();

            var result = await _controller.GetActivePaymentMethods(CancellationToken.None);
            var okResult = result as OkObjectResult;
            
            Assert.That(okResult, Is.Not.Null);
            var items = okResult.Value as IEnumerable<object>;
            Assert.That(items, Is.Not.Null);
            // Count using extension or cast
            int count = 0;
            foreach (var _ in items) count++;
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetAllConfigs_ObfuscatesSecretsForAdmin()
        {
            SetUserRole("Admin");
            _dbContext.PaymentConfigurations.Add(new PaymentConfiguration 
            { 
                DisplayName = "Gateway", 
                GatewaySecretKey = "super-secret",
                GatewayPublicKey = "pub-key",
                Method = Domain.Enums.PaymentMethod.CreditCard
            });
            await _dbContext.SaveChangesAsync();

            var result = await _controller.GetAllConfigs(CancellationToken.None);
            var okResult = result as OkObjectResult;
            var configs = okResult.Value as List<PaymentConfiguration>;

            Assert.That(configs[0].GatewaySecretKey, Is.EqualTo("********"));
            Assert.That(configs[0].GatewayPublicKey, Is.EqualTo("********"));
        }

        [Test]
        public async Task GetAllConfigs_ShowsSecretsForSuperAdmin()
        {
            SetUserRole("SuperAdmin");
            _dbContext.PaymentConfigurations.Add(new PaymentConfiguration 
            { 
                DisplayName = "Gateway", 
                GatewaySecretKey = "super-secret",
                GatewayPublicKey = "pub-key",
                Method = Domain.Enums.PaymentMethod.CreditCard
            });
            await _dbContext.SaveChangesAsync();

            var result = await _controller.GetAllConfigs(CancellationToken.None);
            var okResult = result as OkObjectResult;
            var configs = okResult.Value as List<PaymentConfiguration>;

            Assert.That(configs[0].GatewaySecretKey, Is.EqualTo("super-secret"));
            Assert.That(configs[0].GatewayPublicKey, Is.EqualTo("pub-key"));
        }

        [Test]
        public async Task UpdateConfig_SuperAdminCanChangeSecrets()
        {
            SetUserRole("SuperAdmin");
            var original = new PaymentConfiguration { DisplayName = "Old", GatewaySecretKey = "OldSec", Method = Domain.Enums.PaymentMethod.CreditCard };
            _dbContext.PaymentConfigurations.Add(original);
            await _dbContext.SaveChangesAsync();

            var updateDto = new PaymentConfiguration { DisplayName = "New", GatewaySecretKey = "NewSec" };
            var result = await _controller.UpdateConfig(original.Id, updateDto, CancellationToken.None);

            var updated = await _dbContext.PaymentConfigurations.FindAsync(original.Id);
            Assert.That(updated.GatewaySecretKey, Is.EqualTo("NewSec"));
        }

        [Test]
        public async Task UpdateConfig_AdminCannotChangeSecrets()
        {
            SetUserRole("Admin");
            var original = new PaymentConfiguration { DisplayName = "Old", GatewaySecretKey = "OldSec", Method = Domain.Enums.PaymentMethod.CreditCard };
            _dbContext.PaymentConfigurations.Add(original);
            await _dbContext.SaveChangesAsync();

            // Admin sends obfuscated string (which would happen from UI)
            var updateDto = new PaymentConfiguration { DisplayName = "New", GatewaySecretKey = "********" };
            await _controller.UpdateConfig(original.Id, updateDto, CancellationToken.None);

            var updated = await _dbContext.PaymentConfigurations.FindAsync(original.Id);
            Assert.That(updated.GatewaySecretKey, Is.EqualTo("OldSec"));
        }
        [Test]
        public async Task GetActivePaymentMethods_ReturnsProperlyMappedObjects()
        {
            _dbContext.PaymentConfigurations.Add(new PaymentConfiguration 
            { 
                DisplayName = "Wallet", 
                IsEnabled = true, 
                Method = Domain.Enums.PaymentMethod.BKash,
                WalletNumber = "017"
            });
            await _dbContext.SaveChangesAsync();

            var result = await _controller.GetActivePaymentMethods(CancellationToken.None);
            var okResult = result as OkObjectResult;
            
            Assert.That(okResult, Is.Not.Null);
            var items = okResult.Value as IEnumerable<object>;
            Assert.That(items, Is.Not.Null);
            
            // Check mapping by serializing/deserializing to a JObject or just checking properties via reflection if needed
            // But for simple test, we can just check if we can get the values
            foreach (var item in items)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(item);
                var doc = System.Text.Json.JsonDocument.Parse(json);
                Assert.That(doc.RootElement.GetProperty("DisplayName").GetString(), Is.EqualTo("Wallet"));
                Assert.That(doc.RootElement.GetProperty("WalletNumber").GetString(), Is.EqualTo("017"));
                Assert.That(doc.RootElement.GetProperty("IsOnline").GetBoolean(), Is.False);
            }
        }

        [Test]
        public async Task SeedDefaults_ShouldCreateInitialConfigs()
        {
            SetUserRole("Admin");
            var result = await _controller.SeedDefaults(CancellationToken.None);
            
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var items = await _dbContext.PaymentConfigurations.CountAsync();
            Assert.That(items, Is.GreaterThan(0));
            
            var bkash = await _dbContext.PaymentConfigurations.FirstOrDefaultAsync(p => p.DisplayName == "bKash");
            Assert.That(bkash, Is.Not.Null);
            Assert.That(bkash!.RequiresReceipt, Is.True);
        }
    }
}
