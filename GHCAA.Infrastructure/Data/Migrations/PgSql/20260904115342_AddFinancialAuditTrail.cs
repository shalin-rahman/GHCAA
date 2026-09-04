using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <summary>
    /// Work Package 82.16. Adds the update/delete audit trail to the two entities that record
    /// money: FinancialRecord (the association's ledger) and PaymentHistory (a member's payments).
    /// Before this, both could be edited or hard-deleted leaving no trace of the previous value and
    /// no record of who acted.
    ///
    /// Purely additive: five nullable columns and one boolean defaulting to false, per table.
    /// Existing rows read as never-updated and not-deleted, which is correct for them.
    ///
    /// The scaffolder also emitted one DeleteData and several hundred UpdateData operations against
    /// seeded rows. Those were removed by hand. They are the known non-deterministic HasData seed
    /// churn (see the gotcha_pending_model_changes_seed note and every prior migration in this
    /// folder, none of which carries seed operations either) — applying them would have deleted a
    /// live SiteContents row and rewritten seeded content that nothing in this change touches.
    /// </summary>
    public partial class AddFinancialAuditTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PaymentHistories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByAdminId",
                table: "PaymentHistories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PaymentHistories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PaymentHistories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedByAdminId",
                table: "PaymentHistories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "FinancialRecords",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByAdminId",
                table: "FinancialRecords",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "FinancialRecords",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "FinancialRecords",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedByAdminId",
                table: "FinancialRecords",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Dropping these discards the audit trail itself. That is the correct behaviour for a
            // schema rollback, but it is worth saying out loud: after a down-migration there is no
            // longer any record of who edited or deleted a financial row.
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "PaymentHistories");
            migrationBuilder.DropColumn(name: "UpdatedByAdminId", table: "PaymentHistories");
            migrationBuilder.DropColumn(name: "IsDeleted", table: "PaymentHistories");
            migrationBuilder.DropColumn(name: "DeletedAt", table: "PaymentHistories");
            migrationBuilder.DropColumn(name: "DeletedByAdminId", table: "PaymentHistories");

            migrationBuilder.DropColumn(name: "UpdatedAt", table: "FinancialRecords");
            migrationBuilder.DropColumn(name: "UpdatedByAdminId", table: "FinancialRecords");
            migrationBuilder.DropColumn(name: "IsDeleted", table: "FinancialRecords");
            migrationBuilder.DropColumn(name: "DeletedAt", table: "FinancialRecords");
            migrationBuilder.DropColumn(name: "DeletedByAdminId", table: "FinancialRecords");
        }
    }
}
