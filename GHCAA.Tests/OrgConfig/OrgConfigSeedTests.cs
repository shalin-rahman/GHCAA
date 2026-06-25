using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GHCAA.Tests.OrgConfig
{
    [TestFixture]
    public class OrgConfigSeedTests
    {
        [Test]
        public async Task DefaultConfig_ShouldHave_RequiredLocales()
        {
            // Arrange
            var dbContextOptions = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<GHCAA.Infrastructure.Data.ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "EmptyDbForSeedTest_" + Guid.NewGuid().ToString())
                .Options;
            var dbContext = new GHCAA.Infrastructure.Data.ApplicationDbContext(dbContextOptions);
            var mockCache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
            var service = new OrgConfigService(dbContext, mockCache);

            // Act
            var config = await service.GetConfigAsync();

            // Assert
            config.Localization.Locales.Should().ContainKey("en");
            config.Localization.Locales.Should().ContainKey("bn");
        }

        [Test]
        public async Task DefaultConfig_ShouldHave_7_MembershipTypeLabels()
        {
            // Arrange
            var dbContextOptions = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<GHCAA.Infrastructure.Data.ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "EmptyDbForSeedTest2_" + Guid.NewGuid().ToString())
                .Options;
            var dbContext = new GHCAA.Infrastructure.Data.ApplicationDbContext(dbContextOptions);
            var mockCache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
            var service = new OrgConfigService(dbContext, mockCache);

            // Act
            var config = await service.GetConfigAsync();
            var enPack = config.Localization.Locales["en"];

            // Assert
            enPack.MembershipTypeLabels.Should().HaveCount(7);
            enPack.MembershipTypeLabels.Should().ContainKey("Guest");
        }

        [Test]
        public async Task DefaultConfig_Features_ShouldBe_Correct()
        {
            // Arrange
            var dbContextOptions = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<GHCAA.Infrastructure.Data.ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "EmptyDbForSeedTest3_" + Guid.NewGuid().ToString())
                .Options;
            var dbContext = new GHCAA.Infrastructure.Data.ApplicationDbContext(dbContextOptions);
            var mockCache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
            var service = new OrgConfigService(dbContext, mockCache);

            // Act
            var config = await service.GetConfigAsync();

            // Assert
            config.Features.EnableGamification.Should().BeFalse();
            config.Features.EnableSocialAuth.Should().BeFalse();
            config.Features.EnableEvents.Should().BeTrue();
            config.Features.EnableJobHub.Should().BeTrue();
        }
    }
}
