using FluentAssertions;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data.Migrations.PgSql;
using System.Reflection;
using System.Text.RegularExpressions;

namespace GHCAA.Tests.Data
{
    // The 15 seeded positions live twice: EcPositionIds.Seed() for SQLite and new databases, and the
    // raw SQL in the AddEcPositions migration, which must not call live code. This keeps them equal.
    [TestFixture]
    [Category("FR-34")]
    public class EcPositionSeedSyncTests
    {
        private static readonly Regex Row = new(
            @"\((\d+), '([^']*)', '([^']*)', '([^']*)', (\d+), (\d+), (TRUE|FALSE), (TRUE|FALSE), (TRUE|FALSE), (TRUE|FALSE)\)");

        [Test]
        public void MigrationSeedValues_MatchDomainSeed()
        {
            var field = typeof(AddEcPositions).GetField("SeedValues", BindingFlags.NonPublic | BindingFlags.Static);
            field.Should().NotBeNull("AddEcPositions should still declare a private const SeedValues");
            var sql = (string)field!.GetRawConstantValue()!;

            static bool B(string s) => s == "TRUE";
            var fromSql = Row.Matches(sql).Select(m => (
                Id: int.Parse(m.Groups[1].Value), Key: m.Groups[2].Value, Name: m.Groups[3].Value, NameBn: m.Groups[4].Value,
                Order: int.Parse(m.Groups[5].Value), Seats: int.Parse(m.Groups[6].Value), Active: B(m.Groups[7].Value),
                Board: B(m.Groups[8].Value), Head: B(m.Groups[9].Value), Secretary: B(m.Groups[10].Value))).ToList();

            var fromDomain = EcPositionIds.Seed().Select(p => (
                p.Id, p.Key, p.Name, NameBn: p.NameBn ?? string.Empty, Order: p.DisplayOrder, Seats: p.SeatCount,
                Active: p.IsActive, Board: p.ShowOnBoard, Head: p.SignsAsHead, Secretary: p.SignsAsSecretary)).ToList();

            fromSql.Should().HaveCount(15);
            fromSql.Should().Equal(fromDomain);
        }
    }
}
