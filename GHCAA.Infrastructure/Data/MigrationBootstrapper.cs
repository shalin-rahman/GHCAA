using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace GHCAA.Infrastructure.Data
{
    // Applies pending EF Core migrations at startup. Preprod/production originally built their
    // schema via Database.EnsureCreated() (schema-from-model, no __EFMigrationsHistory table), so
    // every migration added since the first successful boot was silently never applied — the
    // no-op check only asks "do any tables exist", not "does the schema match the model". On a
    // database with no history table yet, this walks every migration in order and tries to apply
    // it for real: if Postgres reports the object it creates already exists, that migration's
    // effect predates migration tracking, so it's marked applied without re-running it; any other
    // failure is a genuine blocker and aborts. This replaces an earlier version that assumed only
    // the single newest migration could be pending — wrong whenever more than one migration had
    // landed since the database was first provisioned, which silently left older migrations'
    // columns/tables missing (e.g. the SiteContents table and the Jobs/Gallery approval-workflow
    // columns behind the 2026-08-27 /api/jobs and /api/gallery 500s).
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
                if (created)
                {
                    var historyRepository = ctx.GetService<IHistoryRepository>();
                    await ctx.Database.ExecuteSqlRawAsync(historyRepository.GetCreateIfNotExistsScript());
                    foreach (var id in allMigrations)
                    {
                        await ctx.Database.ExecuteSqlRawAsync(historyRepository.GetInsertScript(new HistoryRow(id, "9.0.0")));
                    }

                    logger.LogWarning(
                        "Migration history baselined (fresh database): {BaselineCount} migration(s) marked applied.",
                        allMigrations.Count);
                    return;
                }

                await BaselineLegacyDatabaseAsync(ctx, logger, allMigrations);
                return;
            }

            await ctx.Database.MigrateAsync();
        }

        // Legacy database: tables already exist but no migration was ever recorded as applied.
        // Attempt each migration in order via the real migrator; a "such object already exists"
        // error from Postgres means that migration's effect predates tracking, so record it as
        // applied and move on. Any other error is genuine and stops the run — it needs a human.
        private static async Task BaselineLegacyDatabaseAsync(ApplicationDbContext ctx, ILogger logger, List<string> allMigrations)
        {
            var migrator = ctx.GetService<IMigrator>();
            var historyRepository = ctx.GetService<IHistoryRepository>();
            await ctx.Database.ExecuteSqlRawAsync(historyRepository.GetCreateIfNotExistsScript());

            int baselined = 0, applied = 0;
            foreach (var id in allMigrations)
            {
                try
                {
                    await migrator.MigrateAsync(id);
                    applied++;
                }
                catch (Exception ex) when (IsAlreadyExists(ex))
                {
                    await ctx.Database.ExecuteSqlRawAsync(historyRepository.GetInsertScript(new HistoryRow(id, "9.0.0")));
                    baselined++;
                }
            }

            logger.LogWarning(
                "Migration history baselined (legacy EnsureCreated database): {BaselineCount} migration(s) marked applied without running, {AppliedCount} applied for real.",
                baselined,
                applied);
        }

        private static bool IsAlreadyExists(Exception ex)
        {
            for (var e = ex; e != null; e = e.InnerException)
            {
                if (e is PostgresException pg && pg.SqlState is "42P07" or "42701" or "42P06" or "42710" or "23505")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
