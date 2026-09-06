using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddQueryPathIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 82.22: Notification's own query filters on MemberId then sorts by CreatedAt on every
            // portal load; FinancialRecord's ledger screens filter on Year/RecordType/FinancialCategory.
            // Hand-written rather than the full scaffold: the auto-scaffold also picked up unrelated
            // seed-data churn (see gotcha_pending_model_changes_seed), which does not belong here.
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Notifications_MemberId"";");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_Notifications_MemberId_CreatedAt"" ON ""Notifications"" (""MemberId"", ""CreatedAt"");");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_FinancialRecords_Year_RecordType_FinancialCategory"" ON ""FinancialRecords"" (""Year"", ""RecordType"", ""FinancialCategory"");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_FinancialRecords_Year_RecordType_FinancialCategory"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Notifications_MemberId_CreatedAt"";");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_Notifications_MemberId"" ON ""Notifications"" (""MemberId"");");
        }
    }
}
