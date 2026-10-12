using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddKeyDestroyedOnceIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 37.13e. One KeyDestroyed row per election. A database that already holds two cannot
            // take the index, so it is skipped with a warning rather than stopping start-up. The
            // service check and lock still refuse a second record.
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ElectionAuditEntries_KeyDestroyed_Once";""");
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "ElectionAuditEntries" WHERE "Action" = 'KeyDestroyed'
                               GROUP BY "ElectionId" HAVING COUNT(*) > 1) THEN
                        RAISE WARNING 'IX_ElectionAuditEntries_KeyDestroyed_Once not created: an election has two KeyDestroyed rows.';
                    ELSE
                        CREATE UNIQUE INDEX IF NOT EXISTS "IX_ElectionAuditEntries_KeyDestroyed_Once"
                            ON "ElectionAuditEntries" ("ElectionId", "Action") WHERE "Action" = 'KeyDestroyed';
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ElectionAuditEntries_KeyDestroyed_Once";""");
        }
    }
}
