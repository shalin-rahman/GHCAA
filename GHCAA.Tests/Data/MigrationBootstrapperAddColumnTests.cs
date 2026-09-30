using System.Reflection;
using FluentAssertions;
using GHCAA.Infrastructure.Data;
using NUnit.Framework;

namespace GHCAA.Tests.Data
{
    // The column repair itself reads information_schema, so it only runs against Postgres. These pin
    // the SQL it would send for a missing column, which is where a wrong default would hurt.
    [TestFixture]
    public class MigrationBootstrapperAddColumnTests
    {
        private static string? Build(string storeType, bool nullable, string? defaultSql = null)
        {
            var method = typeof(MigrationBootstrapper).GetMethod("BuildAddColumnSql", BindingFlags.NonPublic | BindingFlags.Static);
            method.Should().NotBeNull();
            return (string?)method!.Invoke(null, new object?[] { "AcademicRecords", "IsOrgProfile", storeType, nullable, defaultSql });
        }

        [Test]
        public void NullableColumn_IsAddedWithoutDefault()
        {
            Build("text", nullable: true).Should()
                .Be("ALTER TABLE \"AcademicRecords\" ADD COLUMN IF NOT EXISTS \"IsOrgProfile\" text");
        }

        [Test]
        public void NotNullBoolean_GetsFalseDefault()
        {
            Build("boolean", nullable: false).Should()
                .Be("ALTER TABLE \"AcademicRecords\" ADD COLUMN IF NOT EXISTS \"IsOrgProfile\" boolean NOT NULL DEFAULT FALSE");
        }

        [TestCase("integer", "0")]
        [TestCase("numeric(18,2)", "0")]
        [TestCase("character varying(200)", "''")]
        [TestCase("timestamp with time zone", "'0001-01-01 00:00:00+00'")]
        [TestCase("timestamp without time zone", "'0001-01-01 00:00:00'")]
        [TestCase("uuid", "'00000000-0000-0000-0000-000000000000'")]
        public void NotNullColumn_GetsTheClrZeroValue(string storeType, string expectedDefault)
        {
            Build(storeType, nullable: false).Should().EndWith($"NOT NULL DEFAULT {expectedDefault}");
        }

        [Test]
        public void ModelDefaultSql_WinsOverTheZeroValue()
        {
            Build("timestamp with time zone", nullable: false, defaultSql: "now()").Should().EndWith("NOT NULL DEFAULT now()");
        }

        [Test]
        public void NotNullColumn_WithUnknownStoreType_ReturnsNull()
        {
            Build("jsonb", nullable: false).Should().BeNull();
        }
    }
}
