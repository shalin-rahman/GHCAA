using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
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
            logger.LogWarning("MigrationBootstrapper: DEBUG entered EnsureMigratedAsync");
            // A serverless Postgres provider (e.g. Neon) can be asleep on a cold boot: the first
            // connection attempt can time out/fail while the database wakes up, even though a
            // later attempt seconds later succeeds. A single failed CanConnectAsync used to return
            // here silently — no log line — which meant migrations were skipped for the entire
            // boot with no trace of why. Retry a few times with a short delay, and always log if
            // every attempt fails, so a skipped migration run is never invisible again.
            var connected = false;
            for (var attempt = 1; attempt <= 3 && !connected; attempt++)
            {
                connected = await ctx.Database.CanConnectAsync();
                if (!connected && attempt < 3)
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
            }

            if (!connected)
            {
                logger.LogWarning("MigrationBootstrapper: could not connect to the database after 3 attempts — migrations were not checked/applied this boot.");
                return;
            }

            var allMigrations = ctx.Database.GetMigrations().ToList();
            logger.LogWarning("MigrationBootstrapper: DEBUG allMigrations.Count={Count}", allMigrations.Count);
            if (allMigrations.Count == 0)
            {
                return;
            }

            var applied = (await ctx.Database.GetAppliedMigrationsAsync()).ToHashSet();
            logger.LogWarning("MigrationBootstrapper: DEBUG applied.Count={Count}", applied.Count);
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

            await SelfHealFalselyBaselinedMigrationsAsync(ctx, logger, applied);
            await ctx.Database.MigrateAsync();
        }

        // ctx.Database.MigrateAsync() below trusts __EFMigrationsHistory and will never revisit a
        // migration recorded there — including one whose "already applied" row is a false positive.
        // That happens when a migration mixes schema changes (AddColumn/CreateTable) with a data-seed
        // op (InsertData/UpdateData/DeleteData) in the same transaction: EF runs a migration's Up()
        // operations as one transaction, so if the seed op collides (duplicate key/etc.), Postgres
        // rolls back the DDL right along with it — but IsAlreadyExists() below still matches the
        // collision and baselines the whole migration as applied. See
        // AddApprovalWorkflowToGalleryAndJobs / 2026-08-27's /api/jobs, /api/gallery and /api/events
        // 500s for the incident this generalizes from (that migration's seed insert is now guarded
        // against colliding, but this check covers every migration with the same risky shape, present
        // or future, not just that one). For each migration marked applied that mixes a schema op with
        // a data op, verify its AddColumn/CreateTable targets actually exist; if any are missing,
        // drop that migration's history row so the MigrateAsync call below reapplies it for real.
        private static async Task SelfHealFalselyBaselinedMigrationsAsync(ApplicationDbContext ctx, ILogger logger, HashSet<string> applied)
        {
            var migrationsAssembly = ctx.GetService<IMigrationsAssembly>();
            var activeProvider = ctx.Database.ProviderName!;

            foreach (var migrationId in applied)
            {
                if (!migrationsAssembly.Migrations.TryGetValue(migrationId, out var migrationType))
                {
                    continue;
                }

                var operations = migrationsAssembly.CreateMigration(migrationType, activeProvider).UpOperations;

                var hasSchemaOp = operations.Any(op => op is AddColumnOperation or CreateTableOperation);
                var hasDataOp = operations.Any(op => op is InsertDataOperation or UpdateDataOperation or DeleteDataOperation);
                if (!hasSchemaOp || !hasDataOp)
                {
                    continue;
                }

                foreach (var op in operations)
                {
                    var (table, column) = op switch
                    {
                        AddColumnOperation addColumn => (addColumn.Table, addColumn.Name),
                        CreateTableOperation createTable => (createTable.Name, null),
                        _ => (null, null)
                    };

                    if (table is null)
                    {
                        continue;
                    }

                    var exists = column is null
                        ? await ctx.Database.SqlQueryRaw<int>(
                            "SELECT 1 FROM information_schema.tables WHERE table_name = {0}", table).AnyAsync()
                        : await ctx.Database.SqlQueryRaw<int>(
                            "SELECT 1 FROM information_schema.columns WHERE table_name = {0} AND column_name = {1}", table, column).AnyAsync();

                    if (!exists)
                    {
                        await ctx.Database.ExecuteSqlInterpolatedAsync(
                            $"DELETE FROM \"__EFMigrationsHistory\" WHERE \"MigrationId\" = {migrationId}");
                        logger.LogWarning(
                            "Migration {MigrationId} was recorded as applied but {Table} is missing — history row removed so it will reapply.",
                            migrationId, column is null ? table : $"{table}.{column}");
                        break;
                    }
                }
            }
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
