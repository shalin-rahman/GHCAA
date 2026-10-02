using FluentAssertions;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using NUnit.Framework;

namespace GHCAA.Tests.Data
{
    // Preprod crash loop of 2026-10-01: self-heal saw ElectionOfficers missing (ElectionAppointments
    // drops it), removed AddElectionEngine's history row, and the re-run failed on "Elections". These
    // need no database; the context is only used to build the model and the migration operations.
    [TestFixture]
    public class MigrationBootstrapperSelfHealTests
    {
        private const string AddElectionEngine = "20260920183944_AddElectionEngine";

        private static PgSqlApplicationDbContext NewPgSqlContext()
        {
            var options = new DbContextOptionsBuilder<PgSqlApplicationDbContext>()
                .UseNpgsql("Host=unused;Database=unused", o => o.MigrationsAssembly("GHCAA.Infrastructure"))
                .Options;
            return new PgSqlApplicationDbContext(options);
        }

        private static Dictionary<string, HashSet<string>> ModelColumns(DbContext ctx) =>
            ctx.GetService<IDesignTimeModel>().Model.GetRelationalModel().Tables
                .Where(t => t.Schema is null && !t.IsExcludedFromMigrations)
                .ToDictionary(t => t.Name, t => t.Columns.Select(c => c.Name).ToHashSet(StringComparer.Ordinal), StringComparer.Ordinal);

        private static IReadOnlyList<MigrationOperation> UpOperations(DbContext ctx, string migrationId)
        {
            var assembly = ctx.GetService<IMigrationsAssembly>();
            return assembly.CreateMigration(assembly.Migrations[migrationId], ctx.Database.ProviderName!).UpOperations;
        }

        [Test]
        public void AddElectionEngine_DoesNotCheckTheDroppedElectionOfficersTable()
        {
            using var ctx = NewPgSqlContext();

            var targets = MigrationBootstrapper.StillMappedTargets(UpOperations(ctx, AddElectionEngine), ModelColumns(ctx));

            targets.Should().NotContain(t => t.Table == "ElectionOfficers");
            targets.Should().Contain(("Elections", null));
        }

        [Test]
        public void ElectionBallotSecrecy_DoesNotCheckItsScratchTables()
        {
            using var ctx = NewPgSqlContext();
            var id = ctx.Database.GetMigrations().Single(m => m.EndsWith("_ElectionBallotSecrecy"));

            var targets = MigrationBootstrapper.StillMappedTargets(UpOperations(ctx, id), ModelColumns(ctx));

            targets.Select(t => t.Table).Should().NotContain(new[] { "BallotIdMap", "BallotsCopy", "BallotVotesCopy" });
            targets.Should().Contain(("Ballots", null));
        }

        [Test]
        public void StillMappedTargets_DropsAColumnTheModelNoLongerHas()
        {
            var model = new Dictionary<string, HashSet<string>> { ["Members"] = new() { "Id", "Name" } };
            var ops = new MigrationOperation[]
            {
                new AddColumnOperation { Table = "Members", Name = "Name", ClrType = typeof(string) },
                new AddColumnOperation { Table = "Members", Name = "IsGhc", ClrType = typeof(bool) },
                new SqlOperation { Sql = "ALTER TABLE \"Members\" ADD COLUMN IF NOT EXISTS \"Name\" text; CREATE TABLE \"Tmp\" (x int);" },
            };

            MigrationBootstrapper.StillMappedTargets(ops, model).Should().BeEquivalentTo(new (string, string?)[] { ("Members", "Name") });
        }

        [Test]
        public void HistoryGap_ReturnsOnlyUnappliedMigrationsOlderThanTheNewestApplied()
        {
            var all = new[] { "1_A", "2_B", "3_C", "4_D", "5_E" };
            var applied = new HashSet<string> { "1_A", "3_C", "4_D" };

            MigrationBootstrapper.HistoryGap(all, applied).Should().Equal("2_B");
        }

        [Test]
        public void HistoryGap_IsEmptyWhenNothingIsApplied()
        {
            MigrationBootstrapper.HistoryGap(new[] { "1_A", "2_B" }, new HashSet<string>()).Should().BeEmpty();
        }

        [Test]
        public void HistoryGap_IsEmptyWhenOnlyNewMigrationsArePending()
        {
            var all = new[] { "1_A", "2_B", "3_C" };

            MigrationBootstrapper.HistoryGap(all, new HashSet<string> { "1_A", "2_B" }).Should().BeEmpty();
        }
    }
}
