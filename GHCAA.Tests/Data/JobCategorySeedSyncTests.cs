using FluentAssertions;
using GHCAA.Infrastructure.Data.Migrations.PgSql;
using System.Reflection;
using System.Text.Json;

namespace GHCAA.Tests.Data
{
    // Some lookup rows live twice: lookups.json, which feeds HasData and so the model snapshot and
    // SQLite, and the raw SQL in a PgSql migration. If they differ, the next `ef migrations add`
    // writes DeleteData or UpdateData for the rows that migration seeded.
    [TestFixture]
    public class JobCategorySeedSyncTests
    {
        [Test]
        public void LookupsJson_JobCategoryRows_MatchTheMigration()
            => AssertJsonMatches("JobCategory", typeof(SeedJobCategoryLookups), "Rows");

        [Test]
        public void LookupsJson_DiscussionCategoryRows_MatchTheMigration()
            => AssertJsonMatches("DiscussionCategory", typeof(AddPollDiscussion), "CategoryRows");

        private static void AssertJsonMatches(string group, Type migration, string fieldName)
        {
            var field = migration.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            field.Should().NotBeNull($"{migration.Name} should still declare {fieldName}");
            var fromMigration = ((ValueTuple<int, string, string>[])field!.GetValue(null)!)
                .Select((r, i) => (Id: r.Item1, Value: r.Item2, Label: r.Item3, Order: i + 1)).ToList();

            using var doc = JsonDocument.Parse(File.ReadAllText(LookupsPath()));
            var fromJson = doc.RootElement.EnumerateArray()
                .Where(e => e.GetProperty("LookupGroup").GetString() == group)
                .Select(e => (
                    Id: e.GetProperty("Id").GetInt32(),
                    Value: e.GetProperty("Value").GetString()!,
                    Label: e.GetProperty("Label").GetString()!,
                    Order: e.GetProperty("DisplayOrder").GetInt32()))
                .OrderBy(r => r.Id).ToList();

            fromJson.Should().Equal(fromMigration,
                $"lookups.json needs the same {group} rows (Id, Value, Label, DisplayOrder) as {migration.Name}");
        }

        private static string LookupsPath()
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "GHCAA.Infrastructure")))
                dir = dir.Parent;
            dir.Should().NotBeNull("the repo root should be above the test output directory");
            return Path.Combine(dir!.FullName, "GHCAA.Infrastructure", "Data", "Seed", "lookups.json");
        }
    }
}
