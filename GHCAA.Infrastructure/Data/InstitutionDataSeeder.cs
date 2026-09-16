using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Data
{
    /// <summary>
    /// Seeds Tier 3 data (docs/SEED_CLASSIFICATION.md) — one institution's own members, accounts
    /// and history, never every institution's starting data. This used to be baked into migrations
    /// via <c>HasData</c>, which committed 631 real alumni records (including password hashes)
    /// straight into git. It now loads at boot instead, from whichever seed file the current
    /// profile resolves — never from a literal in this file.
    /// </summary>
    ///
    /// <remarks>
    /// Idempotent per table: a table already holding rows is left untouched. There is no upsert/diff
    /// across these 14 FK-linked tables — a Tier 3 table is either genuinely empty (new
    /// institution) or already real (GHC today), never partially reconciled.
    /// </remarks>
    public static class InstitutionDataSeeder
    {
        /// <summary>
        /// One row per Tier 3 file, in FK-safe seeding order — Members before Users/EC/records,
        /// ECPeriods before ECMembers, Users before UserRoles/News (AuthorId), EventGalleries before
        /// EventPhotos. This is the single place that says what's Tier 3 and how to seed it: the
        /// guard test in GHCAA.Tests, the "unregistered demo-data file" check below, and SyncAsync's
        /// own seeding loop all read this same list, so a 15th file later means one line here, not
        /// three places kept in sync by hand.
        /// </summary>
        public static readonly IReadOnlyList<InstitutionDataRegistryEntry> Registry = new List<InstitutionDataRegistryEntry>
        {
            For<Member>("members.json", "Members"),
            For<User>("users.json", "Users", ignoreQueryFilters: true),
            new("user_roles.json", "UserRoles", null, SeedUserRolesAsync), // shadow entity — see UserConfiguration.cs
            For<ECPeriod>("ec_periods.json", "ECPeriods"),
            For<ECMember>("ec_members.json", "ECMembers"),
            For<AcademicRecord>("academic_records.json", "AcademicRecords"),
            For<ProfessionalRecord>("professional_records.json", "ProfessionalRecords"),
            For<PaymentHistory>("payment_histories.json", "PaymentHistories"),
            For<SavedPaymentMethod>("saved_payment_methods.json", "SavedPaymentMethods"),
            For<EventGallery>("galleries.json", "EventGalleries"),
            For<EventPhoto>("photos.json", "EventPhotos"),
            For<AlumniEvent>("events.json", "AlumniEvents"),
            For<JobOpportunity>("jobs.json", "JobOpportunities"),
            For<NewsPost>("news.json", "NewsPosts"),
        };

        private static InstitutionDataRegistryEntry For<T>(string jsonFileName, string tableName, bool ignoreQueryFilters = false) where T : class =>
            new(jsonFileName, tableName, typeof(T),
                (context, realDataDirectory, logger, ct) =>
                    SeedIfEmptyAsync<T>(context, tableName, jsonFileName, realDataDirectory, logger, ct, ignoreQueryFilters));

        /// <summary>
        /// Idempotent. Safe to call on every boot. <paramref name="realDataDirectory"/> is an
        /// operator-supplied folder holding the 14 files above outside the git tree (resolved from
        /// AppSettings:RealDataPath by the caller, same convention as the SuperAdmin bootstrap
        /// password path) — when a file exists there it wins over the profile's demo-data copy.
        /// </summary>
        public static async Task SyncAsync(ApplicationDbContext context, ILogger logger, string? realDataDirectory, CancellationToken ct = default)
        {
            WarnAboutUnregisteredDemoDataFiles(realDataDirectory, logger);

            foreach (var entry in Registry)
            {
                await entry.SeedAsync(context, realDataDirectory, logger, ct);
            }
        }

        private static List<T> LoadSeedFile<T>(string fileName, string? realDataDirectory)
        {
            if (!string.IsNullOrWhiteSpace(realDataDirectory))
            {
                var overridePath = Path.Combine(realDataDirectory, fileName);
                var overrideRecords = ApplicationDbContext.LoadSeedFromPath<T>(overridePath);
                if (overrideRecords.Count > 0) return overrideRecords;
            }

            return ApplicationDbContext.LoadSeed<T>(fileName);
        }

        private static async Task SeedIfEmptyAsync<T>(
            ApplicationDbContext context, string tableName, string jsonFileName, string? realDataDirectory,
            ILogger logger, CancellationToken ct, bool ignoreQueryFilters = false) where T : class
        {
            var set = context.Set<T>();
            var existsQuery = ignoreQueryFilters ? set.IgnoreQueryFilters() : set;
            if (await existsQuery.AnyAsync(ct))
            {
                logger.LogInformation("Institution data seed: {Table} already populated, skipping {File}.", tableName, jsonFileName);
                return;
            }

            var records = LoadSeedFile<T>(jsonFileName, realDataDirectory);
            if (records.Count == 0)
            {
                logger.LogWarning("Institution data seed: {File} produced no records — {Table} stays empty.", jsonFileName, tableName);
                return;
            }

            await set.AddRangeAsync(records, ct);
            await context.SaveChangesAsync(ct);
            await ResetIdentitySequenceAsync(context, tableName, ct);

            logger.LogInformation("Institution data seed: inserted {Count} rows into {Table} from {File}.", records.Count, tableName, jsonFileName);
        }

        private static async Task SeedUserRolesAsync(ApplicationDbContext context, string? realDataDirectory, ILogger logger, CancellationToken ct)
        {
            const string tableName = "UserRoles";
            const string jsonFileName = "user_roles.json";

            var set = context.Set<Dictionary<string, object>>(tableName);
            if (await set.AnyAsync(ct))
            {
                logger.LogInformation("Institution data seed: {Table} already populated, skipping {File}.", tableName, jsonFileName);
                return;
            }

            var records = LoadSeedFile<Dictionary<string, object>>(jsonFileName, realDataDirectory);
            if (records.Count == 0)
            {
                logger.LogWarning("Institution data seed: {File} produced no records — {Table} stays empty.", jsonFileName, tableName);
                return;
            }

            // UserRoles is a shadow many-to-many join (UserConfiguration.cs), not a typed entity —
            // its two FK columns come through as loosely-typed values off the JSON, same parsing
            // the old HasData call used.
            foreach (var record in records)
            {
                set.Add(new Dictionary<string, object>
                {
                    ["RolesId"] = int.Parse(record["RolesId"]?.ToString() ?? "0"),
                    ["UsersId"] = int.Parse(record["UsersId"]?.ToString() ?? "0"),
                });
            }

            await context.SaveChangesAsync(ct);
            logger.LogInformation("Institution data seed: inserted {Count} rows into {Table} from {File}.", records.Count, tableName, jsonFileName);
        }

        private static async Task ResetIdentitySequenceAsync(ApplicationDbContext context, string tableName, CancellationToken ct)
        {
            await context.Database.ExecuteSqlRawAsync(
                $"SELECT setval(pg_get_serial_sequence('\"{tableName}\"', 'Id'), " +
                $"(SELECT COALESCE(MAX(\"Id\"), 1) FROM \"{tableName}\"));", ct);
        }

        private static void WarnAboutUnregisteredDemoDataFiles(string? realDataDirectory, ILogger logger)
        {
            var registeredFiles = Registry.Select(r => r.JsonFileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var directoriesToCheck = new List<string?> { realDataDirectory, ApplicationDbContext.FindActiveDemoDataDirectory() };

            foreach (var dir in directoriesToCheck.Where(Directory.Exists))
            {
                foreach (var file in Directory.EnumerateFiles(dir!, "*.json"))
                {
                    var name = Path.GetFileName(file);
                    if (!registeredFiles.Contains(name))
                    {
                        logger.LogWarning(
                            "Institution data seed: {File} in {Directory} is not in InstitutionDataSeeder.Registry — it will never be loaded.",
                            name, dir);
                    }
                }
            }
        }
    }

    public sealed record InstitutionDataRegistryEntry(
        string JsonFileName,
        string TableName,
        Type? EntityType,
        Func<ApplicationDbContext, string?, ILogger, CancellationToken, Task> SeedAsync);
}
