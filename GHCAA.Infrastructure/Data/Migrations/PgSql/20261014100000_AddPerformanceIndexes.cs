using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Raw SQL with IF NOT EXISTS, because a database built by EnsureCreated already has the
            // model indexes and the migration must still apply there.
            // 98.11: the member list ranks a page by counting members with more points.
            migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_Members_ContributionPoints" ON "Members" ("ContributionPoints");""");
            // 98.14: the daily retention sweep deletes EmailLogs by SentDate and ActivityLogs by Timestamp.
            migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_EmailLogs_SentDate" ON "EmailLogs" ("SentDate");""");
            migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_ActivityLogs_Timestamp" ON "ActivityLogs" ("Timestamp");""");
            // 98.13: password reset matches email with lower() on both sides, which the plain
            // unique index on Members.Email cannot serve. EF cannot model an expression index, so
            // these two live only here. Not unique: old rows may differ only by case.
            migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_Members_Email_Lower" ON "Members" (lower("Email"));""");
            migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_Users_Email_Lower" ON "Users" (lower("Email"));""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Users_Email_Lower";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Members_Email_Lower";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ActivityLogs_Timestamp";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_EmailLogs_SentDate";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Members_ContributionPoints";""");
        }
    }
}
