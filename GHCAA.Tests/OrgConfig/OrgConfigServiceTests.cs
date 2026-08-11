using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;

namespace GHCAA.Tests.OrgConfig
{
    [TestFixture]
    public class OrgConfigServiceTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName + "_" + Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Test]
        public async Task GetConfigAsync_EmptyDb_ReturnsDefaults()
        {
            // Arrange
            var dbContext = GetDbContext("TestDb_Empty");
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new OrgConfigService(dbContext, cache);

            // Act
            var config = await service.GetConfigAsync();

            // Assert
            config.Should().NotBeNull();
            config.OrgId.Should().Be("ghcaa");
            config.Branding.ShortName.Should().Be("GHCAA");
        }

        [Test]
        public async Task UpdateConfigAsync_PersistsToDb_AndInvalidatesCache()
        {
            // Arrange
            var dbContext = GetDbContext("TestDb_Update");
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new OrgConfigService(dbContext, cache);

            var initialConfig = await service.GetConfigAsync();
            var modifiedConfig = initialConfig with
            {
                Branding = initialConfig.Branding with { ShortName = "NEW_TEST_ORG" }
            };

            // Act
            await service.UpdateConfigAsync(modifiedConfig, "admin-1");

            // Assert - Check via service (cache should be invalidated so it fetches from DB)
            var fetchedConfig = await service.GetConfigAsync();
            fetchedConfig.Branding.ShortName.Should().Be("NEW_TEST_ORG");

            // Assert - Check direct DB record
            var dbRecord = await dbContext.OrganizationConfigs.FirstOrDefaultAsync();
            dbRecord.Should().NotBeNull();
            dbRecord!.ConfigJson.Should().Contain("NEW_TEST_ORG");
            dbRecord.UpdatedByAdminId.Should().Be("admin-1");
        }

        [Test]
        public async Task GetConfigAsync_CanLookup_BengaliLocale()
        {
            // Arrange
            var dbContext = GetDbContext("TestDb_Bengali");
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new OrgConfigService(dbContext, cache);

            // Act
            var config = await service.GetConfigAsync();

            // Assert
            var bnPack = config.Localization.Locales["bn"];
            bnPack.Should().NotBeNull();
            bnPack.MemberNickname.Should().Be("হারাগঙ্গিয়ান");
        }
    }
}
