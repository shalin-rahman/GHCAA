using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Data
{
    // Applies pending EF Core migrations at startup. Preprod/production originally built their
    // schema via Database.EnsureCreated() (schema-from-model, no __EFMigrationsHistory table), so
    // every migration added since the first successful boot was silently never applied — the
    // no-op check only asks "do any tables exist", not "does the schema match the model". On a
    // database with no history table yet, this baselines history to every already-existing
    // migration before calling Migrate(), so only genuinely pending migrations run instead of
    // replaying the whole history against tables that already exist.
    public static class MigrationBootstrapper
    {
        public static async Task EnsureMigratedAsync(ApplicationDbContext ctx, ILogger logger)
        {
            if (!await ctx.Database.CanConnectAsync())
            {
                return;
            }

            var allMigrations = ctx.Database.GetMigrations().ToList();
            if (allMigrations.Count == 0)
            {
                return;
            }

            var applied = (await ctx.Database.GetAppliedMigrationsAsync()).ToHashSet();
            if (applied.Count == 0)
            {
                // EnsureCreatedAsync returns true only if it just built a brand-new database
                // (schema-from-current-model); false means tables already existed — a legacy
                // database from an earlier EnsureCreated() boot, predating migration tracking.
                var created = await ctx.Database.EnsureCreatedAsync();
                var baseline = created
                    ? allMigrations
                    : allMigrations.Take(allMigrations.Count - 1).ToList();

                if (baseline.Count > 0)
                {
                    var historyRepository = ctx.GetService<IHistoryRepository>();
                    await ctx.Database.ExecuteSqlRawAsync(historyRepository.GetCreateIfNotExistsScript());
                    foreach (var id in baseline)
                    {
                        await ctx.Database.ExecuteSqlRawAsync(historyRepository.GetInsertScript(new HistoryRow(id, "9.0.0")));
                    }

                    logger.LogWarning(
                        "Migration history baselined ({Mode}): {BaselineCount} migration(s) marked applied, {PendingCount} pending.",
                        created ? "fresh database" : "legacy EnsureCreated database",
                        baseline.Count,
                        allMigrations.Count - baseline.Count);
                }
            }

            await ctx.Database.MigrateAsync();
        }
    }
}
