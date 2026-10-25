using FluentAssertions;
using GHCAA.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GHCAA.Tests.Data;

// 37.14v: only a unique-index clash may be read as "already exists". Any other save error has to
// reach the caller as a fault.
[TestFixture]
[Category("FR-25")]
public sealed class DbErrorsTests
{
    private static DbUpdateException Wrap(Exception inner) => new("save failed", inner);

    [Test]
    public void PostgresUniqueViolation_IsAClash()
    {
        var pg = new PostgresException("duplicate key", "ERROR", "ERROR", PostgresErrorCodes.UniqueViolation);

        DbErrors.IsUniqueViolation(Wrap(pg)).Should().BeTrue();
    }

    [Test]
    public void OtherPostgresError_IsNotAClash()
    {
        var pg = new PostgresException("value too long", "ERROR", "ERROR", PostgresErrorCodes.StringDataRightTruncation);

        DbErrors.IsUniqueViolation(Wrap(pg)).Should().BeFalse();
    }

    [Test]
    public void SqliteUniqueConstraint_IsAClash()
    {
        DbErrors.IsUniqueViolation(Wrap(new SqliteException("UNIQUE constraint failed", 19, 2067))).Should().BeTrue();
    }

    [Test]
    public void OtherSqliteConstraint_IsNotAClash()
    {
        // 787 is a foreign key failure. It shares error 19 with the unique clash.
        DbErrors.IsUniqueViolation(Wrap(new SqliteException("FOREIGN KEY constraint failed", 19, 787))).Should().BeFalse();
    }

    [Test]
    public void NoProviderError_IsNotAClash()
    {
        DbErrors.IsUniqueViolation(new DbUpdateException("save failed")).Should().BeFalse();
        DbErrors.IsUniqueViolation(Wrap(new TimeoutException())).Should().BeFalse();
    }

    // 84.53: a vote that lost a Serializable clash runs again. SaveChanges wraps the error and a
    // failed commit throws it bare, so both shapes count.
    [Test]
    public void PostgresSerializationFailure_IsRetryable_WrappedOrBare()
    {
        var pg = new PostgresException("could not serialize access", "ERROR", "ERROR", PostgresErrorCodes.SerializationFailure);

        DbErrors.IsSerializationFailure(Wrap(pg)).Should().BeTrue();
        DbErrors.IsSerializationFailure(pg).Should().BeTrue();
    }

    [Test]
    public void OtherErrors_AreNotRetryable()
    {
        var clash = new PostgresException("duplicate key", "ERROR", "ERROR", PostgresErrorCodes.UniqueViolation);

        DbErrors.IsSerializationFailure(Wrap(clash)).Should().BeFalse();
        DbErrors.IsSerializationFailure(Wrap(new SqliteException("database is locked", 5))).Should().BeFalse();
        DbErrors.IsSerializationFailure(new DbUpdateException("save failed")).Should().BeFalse();
    }
}
