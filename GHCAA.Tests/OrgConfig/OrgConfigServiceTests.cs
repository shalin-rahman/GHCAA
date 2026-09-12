using System.Text.Json;
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
        // No ORG_PROFILE means OrgConfigService builds from its hardcoded BuildGhcaaDefaults(), not
        // from a profile pack — but OrgConfigGoldenSnapshotTests already proves the two are
        // byte-identical, so reading the real pack here keeps this test tied to that source of
        // truth instead of a bare string literal that could silently drift from it.
        private static string RepoRoot()
        {
            var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "profiles")))
                dir = dir.Parent;
            dir.Should().NotBeNull();
            return dir!.FullName;
        }

        private static readonly BrandingDto GhcPackBranding = JsonSerializer.Deserialize<OrgConfigDto>(
            File.ReadAllText(Path.Combine(RepoRoot(), "profiles", "ghc", "org-config.json")),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Branding;

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
            config.Branding.ShortName.Should().Be(GhcPackBranding.ShortName);
        }

        [Category("FR-42")]
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

        // 59.2: a stored OrganizationConfig row (created once, then round-tripped whole on every
        // admin Branding/Workflow save — no admin UI ever edits Localization directly) could freeze
        // in a Localization value from before a later source-code copy fix, serving stale text
        // forever. GetConfigAsync now always overlays Localization from BuildGhcaaDefaults()
        // regardless of what's stored, so a corrected string in code is served immediately.
        [Test]
        public async Task GetConfigAsync_OverridesStaleStoredLocalization_WithCurrentSourceDefaults()
        {
            // Arrange: persist a config row with a deliberately corrupted Bengali tagline, as if it
            // had been captured before a later source-code fix.
            var dbContext = GetDbContext("TestDb_StaleLocalization");
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new OrgConfigService(dbContext, cache);

            var initialConfig = await service.GetConfigAsync();
            var staleBnPack = initialConfig.Localization.Locales["bn"] with { Tagline = "STALE_CORRUPTED_TAGLINE" };
            var staleLocales = new Dictionary<string, LocalePackDto>(initialConfig.Localization.Locales) { ["bn"] = staleBnPack };
            var staleConfig = initialConfig with
            {
                Branding = initialConfig.Branding with { ShortName = "STILL_ADMIN_EDITABLE" },
                Localization = initialConfig.Localization with { Locales = staleLocales }
            };
            await service.UpdateConfigAsync(staleConfig, "admin-1");

            // Act
            var fetchedConfig = await service.GetConfigAsync();

            // Assert: Localization self-heals to the current source value...
            fetchedConfig.Localization.Locales["bn"].Tagline.Should().Be("ঐতিহ্যের বিনিময়, জীবনের সমন্বয় ও সংহতির সেতুবন্ধন");
            // ...while genuinely admin-editable sections (Branding) are left untouched.
            fetchedConfig.Branding.ShortName.Should().Be("STILL_ADMIN_EDITABLE");
        }

        [Test]
        public async Task UpdateConfigAsync_RoundTripsSupportedDateFormat()
        {
            var dbContext = GetDbContext("TestDb_DateFormatRoundTrip");
            var service = new OrgConfigService(dbContext, new MemoryCache(new MemoryCacheOptions()));
            var config = await service.GetConfigAsync();

            await service.UpdateConfigAsync(
                config with { Localization = config.Localization with { DateFormat = "MM/dd/yyyy" } },
                "admin-1");

            (await service.GetConfigAsync()).Localization.DateFormat.Should().Be("MM/dd/yyyy");
        }

        [Test]
        public async Task GetConfigAsync_DefaultsDateFormatToDdMmYyyy()
        {
            var dbContext = GetDbContext("TestDb_DateFormatDefault");
            var service = new OrgConfigService(dbContext, new MemoryCache(new MemoryCacheOptions()));

            (await service.GetConfigAsync()).Localization.DateFormat.Should().Be("dd-MM-yyyy");
        }

        [Test]
        public async Task UpdateConfigAsync_InvalidDateFormat_FallsBackToDefault()
        {
            var dbContext = GetDbContext("TestDb_DateFormatInvalid");
            var service = new OrgConfigService(dbContext, new MemoryCache(new MemoryCacheOptions()));
            var config = await service.GetConfigAsync();

            await service.UpdateConfigAsync(
                config with { Localization = config.Localization with { DateFormat = "yyyy-MM-dd" } },
                "admin-1");

            (await service.GetConfigAsync()).Localization.DateFormat.Should().Be("dd-MM-yyyy");
        }

        [Test]
        public async Task UpdateConfigAsync_DoesNotRewriteExistingDateValues()
        {
            var dbContext = GetDbContext("TestDb_DateValuesRemainStable");
            var service = new OrgConfigService(dbContext, new MemoryCache(new MemoryCacheOptions()));
            var storedStart = new DateTime(2024, 5, 17, 14, 35, 12, DateTimeKind.Utc);
            var storedEnd = storedStart.AddHours(2);

            dbContext.AlumniEvents.Add(new GHCAA.Domain.Models.AlumniEvent
            {
                Title = "Existing event",
                Description = "Existing date values must survive display-format changes.",
                Location = "Campus",
                StartDate = storedStart,
                EndDate = storedEnd
            });
            await dbContext.SaveChangesAsync();

            var config = await service.GetConfigAsync();
            await service.UpdateConfigAsync(
                config with { Localization = config.Localization with { DateFormat = "MM/dd/yyyy" } },
                "admin-1");

            var eventRecord = await dbContext.AlumniEvents.SingleAsync();
            eventRecord.StartDate.Should().Be(storedStart);
            eventRecord.EndDate.Should().Be(storedEnd);
        }
    }
}
