using System.Text.RegularExpressions;
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
        // Raw-SQL migrations (the house style since the idempotency pass — CREATE ... IF NOT EXISTS,
        // ON CONFLICT DO NOTHING, etc.) show up as a single opaque SqlOperation, not typed
        // AddColumn/CreateTable/InsertData ops — so the type-based risky-shape check below can't see
        // them at all. Pull CREATE TABLE / ADD COLUMN targets out of the raw SQL text with a regex
        // instead so those migrations are still covered.
        private static readonly Regex CreateTableRegex = new(
            @"CREATE\s+TABLE\s+(?:IF\s+NOT\s+EXISTS\s+)?""?(?<table>[A-Za-z_][A-Za-z0-9_]*)""?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex AddColumnRegex = new(
            @"ALTER\s+TABLE\s+""?(?<table>[A-Za-z_][A-Za-z0-9_]*)""?\s+ADD\s+COLUMN\s+(?:IF\s+NOT\s+EXISTS\s+)?""?(?<column>[A-Za-z_][A-Za-z0-9_]*)""?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

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

                var targets = new List<(string Table, string? Column)>();
                foreach (var op in operations)
                {
                    switch (op)
                    {
                        case AddColumnOperation addColumn:
                            targets.Add((addColumn.Table, addColumn.Name));
                            break;
                        case CreateTableOperation createTable:
                            targets.Add((createTable.Name, null));
                            break;
                        case SqlOperation sqlOp when sqlOp.Sql is not null:
                            // A raw-SQL migration mixing DDL with a data statement is exactly the
                            // risky shape this method exists to catch, and we can't type-check that
                            // from a single opaque string — so any CREATE TABLE/ADD COLUMN found here
                            // is always treated as needing verification, regardless of what else is
                            // in the migration.
                            foreach (Match m in CreateTableRegex.Matches(sqlOp.Sql))
                                targets.Add((m.Groups["table"].Value, null));
                            foreach (Match m in AddColumnRegex.Matches(sqlOp.Sql))
                                targets.Add((m.Groups["table"].Value, m.Groups["column"].Value));
                            break;
                    }
                }

                if (targets.Count == 0)
                {
                    continue;
                }

                // Typed-op migrations only count as risky when a schema op is mixed with a data op —
                // preserves the original scope for those. Raw-SQL targets found via regex are always
                // checked, since we can't tell whether the surrounding SqlOperation also seeds data.
                var hasTypedSchemaOp = operations.Any(op => op is AddColumnOperation or CreateTableOperation);
                var hasTypedDataOp = operations.Any(op => op is InsertDataOperation or UpdateDataOperation or DeleteDataOperation);
                var hasSqlOp = operations.Any(op => op is SqlOperation);
                if (!hasSqlOp && (!hasTypedSchemaOp || !hasTypedDataOp))
                {
                    continue;
                }

                foreach (var (table, column) in targets)
                {
                    var exists = column is null
                        ? await ctx.Database.SqlQueryRaw<int>(
                            "SELECT 1 AS \"Value\" FROM information_schema.tables WHERE table_schema = current_schema() AND table_name = {0}", table).AnyAsync()
                        : await ctx.Database.SqlQueryRaw<int>(
                            "SELECT 1 AS \"Value\" FROM information_schema.columns WHERE table_schema = current_schema() AND table_name = {0} AND column_name = {1}", table, column).AnyAsync();

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
                    // Per-migration detail (not just the aggregate count below) is exactly the
                    // diagnostic this class of incident needs — a 23505 unique-violation baseline is
                    // the specific shape that has previously masked a rolled-back DDL change.
                    var sqlState = ex is PostgresException pg ? pg.SqlState
                        : ex.InnerException is PostgresException innerPg ? innerPg.SqlState : "unknown";
                    logger.LogInformation(
                        "Migration {MigrationId} baselined without running (SqlState {SqlState}): effect already exists.",
                        id, sqlState);
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
