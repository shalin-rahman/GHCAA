using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using GHCAA.Infrastructure.Data;
using NUnit.Framework;

namespace GHCAA.Tests.Data
{
    // MigrationBootstrapper.SelfHealFalselyBaselinedMigrationsAsync verifies whether a migration
    // recorded as "applied" actually created what it claims to, so a rolled-back DDL change hidden
    // behind a same-transaction seed-insert collision gets reapplied instead of staying silently
    // missing (see the AddApprovalWorkflowToGalleryAndJobs incident this generalizes from). Since
    // the house style moved to raw-SQL migrations (CREATE ... IF NOT EXISTS, ON CONFLICT DO
    // NOTHING), those migrations show up as a single opaque SqlOperation rather than typed
    // AddColumnOperation/CreateTableOperation objects, so this session extended the method with a
    // regex scan of SqlOperation.Sql to still pull out CREATE TABLE / ADD COLUMN targets.
    //
    // The full self-heal path (including the information_schema existence check) only runs
    // against Postgres — SqlQueryRaw's "current_schema()"/information_schema.* text is Postgres-
    // specific and has no equivalent on the Sqlite/InMemory providers this test project uses, so it
    // can't be exercised end-to-end here. These tests instead pin down the regexes themselves
    // (reached via reflection, since they're a private implementation detail) against the exact
    // shapes of raw SQL this repo's migrations emit, so a future edit to either pattern can't
    // silently stop detecting a schema-mixing migration.
    [TestFixture]
    public class MigrationBootstrapperRegexTests
    {
        private static Regex GetPrivateRegex(string fieldName)
        {
            var field = typeof(MigrationBootstrapper).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            field.Should().NotBeNull($"MigrationBootstrapper should still declare a private static field named '{fieldName}'");
            return (Regex)field!.GetValue(null)!;
        }

        [Test]
        public void CreateTableRegex_ShouldMatch_PlainCreateTable()
        {
            var regex = GetPrivateRegex("CreateTableRegex");
            var sql = "CREATE TABLE \"SiteContents\" (\"Id\" serial NOT NULL, \"Title\" text NOT NULL);";

            var match = regex.Match(sql);

            match.Success.Should().BeTrue();
            match.Groups["table"].Value.Should().Be("SiteContents");
        }

        [Test]
        public void CreateTableRegex_ShouldMatch_IfNotExistsVariant_WithoutQuotedIdentifier()
        {
            var regex = GetPrivateRegex("CreateTableRegex");
            var sql = "CREATE TABLE IF NOT EXISTS Jobs (Id int PRIMARY KEY);";

            var match = regex.Match(sql);

            match.Success.Should().BeTrue();
            match.Groups["table"].Value.Should().Be("Jobs");
        }

        [Test]
        public void CreateTableRegex_ShouldFindEveryTable_AcrossAMultiStatementMigration()
        {
            var regex = GetPrivateRegex("CreateTableRegex");
            var sql = "CREATE TABLE IF NOT EXISTS \"Galleries\" (\"Id\" serial); " +
                      "CREATE TABLE IF NOT EXISTS \"GalleryComments\" (\"Id\" serial);";

            var tables = regex.Matches(sql).Select(m => m.Groups["table"].Value).ToList();

            tables.Should().BeEquivalentTo(new[] { "Galleries", "GalleryComments" });
        }

        [Test]
        public void CreateTableRegex_ShouldNotMatch_UnrelatedSql()
        {
            var regex = GetPrivateRegex("CreateTableRegex");
            var sql = "INSERT INTO \"EmailTemplates\" (\"Id\", \"Code\") VALUES (1, 'X') ON CONFLICT DO NOTHING;";

            regex.IsMatch(sql).Should().BeFalse();
        }

        [Test]
        public void AddColumnRegex_ShouldMatch_PlainAddColumn()
        {
            var regex = GetPrivateRegex("AddColumnRegex");
            var sql = "ALTER TABLE \"Jobs\" ADD COLUMN \"ApprovalStatus\" integer NOT NULL DEFAULT 0;";

            var match = regex.Match(sql);

            match.Success.Should().BeTrue();
            match.Groups["table"].Value.Should().Be("Jobs");
            match.Groups["column"].Value.Should().Be("ApprovalStatus");
        }

        [Test]
        public void AddColumnRegex_ShouldMatch_IfNotExistsVariant_WithoutQuotedIdentifiers()
        {
            var regex = GetPrivateRegex("AddColumnRegex");
            var sql = "ALTER TABLE Gallery ADD COLUMN IF NOT EXISTS ApprovedBy int;";

            var match = regex.Match(sql);

            match.Success.Should().BeTrue();
            match.Groups["table"].Value.Should().Be("Gallery");
            match.Groups["column"].Value.Should().Be("ApprovedBy");
        }

        [Test]
        public void AddColumnRegex_ShouldFindEveryColumn_AcrossAMultiStatementMigration()
        {
            var regex = GetPrivateRegex("AddColumnRegex");
            var sql = "ALTER TABLE \"Jobs\" ADD COLUMN IF NOT EXISTS \"ApprovalStatus\" integer; " +
                      "ALTER TABLE \"Jobs\" ADD COLUMN IF NOT EXISTS \"ApprovedBy\" integer;";

            var columns = regex.Matches(sql).Select(m => (m.Groups["table"].Value, m.Groups["column"].Value)).ToList();

            columns.Should().BeEquivalentTo(new[] { ("Jobs", "ApprovalStatus"), ("Jobs", "ApprovedBy") });
        }

        [Test]
        public void AddColumnRegex_ShouldNotMatch_PlainCreateTableSql()
        {
            var regex = GetPrivateRegex("AddColumnRegex");
            var sql = "CREATE TABLE IF NOT EXISTS \"SiteContents\" (\"Id\" serial NOT NULL);";

            regex.IsMatch(sql).Should().BeFalse();
        }

        // Mirrors the exact scenario this session's extension exists to catch: a raw-SQL migration
        // that both creates a table (schema) and seeds a row (data, e.g. ON CONFLICT DO NOTHING) in
        // the same opaque SqlOperation — a shape a typed-op check can't see at all, but the regex
        // scan pulls the CREATE TABLE target out of regardless of what data statement sits next to it.
        [Test]
        public void CreateTableRegex_ShouldDetectSchemaMixingMigration_WhenCreateTableIsFollowedByDataSeedSql()
        {
            var createTableRegex = GetPrivateRegex("CreateTableRegex");
            var mixedSql =
                "CREATE TABLE IF NOT EXISTS \"SiteContents\" (\"Id\" serial NOT NULL, \"Slug\" text NOT NULL);" +
                "INSERT INTO \"SiteContents\" (\"Id\", \"Slug\") VALUES (1, 'about') ON CONFLICT DO NOTHING;";

            var matches = createTableRegex.Matches(mixedSql);

            matches.Should().ContainSingle();
            matches[0].Groups["table"].Value.Should().Be("SiteContents");
        }
    }
}
