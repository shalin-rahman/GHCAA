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

        private const int SqliteUniqueConstraint = 2067;
    }
}
