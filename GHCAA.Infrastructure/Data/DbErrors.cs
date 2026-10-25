using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Data
{
    public static class DbErrors
    {
        // True only when the save broke a unique index, which means another write got there first.
        // Any other save error (a dropped connection, a bad value) is a real fault.
        // SQLite reports the same clash as error 19 with extended code 2067 in the tests.
        public static bool IsUniqueViolation(DbUpdateException ex) => ex.InnerException switch
        {
            Npgsql.PostgresException pg => pg.SqlState == Npgsql.PostgresErrorCodes.UniqueViolation,
            Microsoft.Data.Sqlite.SqliteException lite => lite.SqliteExtendedErrorCode == SqliteUniqueConstraint,
            _ => false
        };

        // PostgreSQL 40001: a Serializable transaction lost to another one running at the same
        // time. Nothing was written, so the whole transaction can run again. The error arrives
        // bare from a commit and wrapped in DbUpdateException from SaveChanges.
        public static bool IsSerializationFailure(Exception ex) =>
            (ex as Npgsql.PostgresException ?? ex.InnerException as Npgsql.PostgresException)?.SqlState
                == Npgsql.PostgresErrorCodes.SerializationFailure;

        private const int SqliteUniqueConstraint = 2067;
    }
}
