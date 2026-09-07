using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class RenameIsDeletedToIsArchived : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 82.30: FinancialRecord, PaymentHistory and ECMember called the same Class A
            // soft-delete flag IsDeleted while Member, User, Poll and Campaign already called it
            // IsArchived. Renaming these three (not the other four) is the cheaper direction, since
            // Member already has a composite index built on IsArchived (Member(Status, IsArchived),
            // WP 24.37) that a rename the other way would have to rebuild. Plain column renames:
            // Postgres updates the two filtered unique indexes on PaymentHistories (TransactionId,
            // GatewayPaymentId) automatically, since their predicates reference the column by
            // attnum, not by name — no index needs to be dropped or recreated here.
            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "FinancialRecords",
                newName: "IsArchived");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "PaymentHistories",
                newName: "IsArchived");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "ECMembers",
                newName: "IsArchived");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsArchived",
                table: "FinancialRecords",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "IsArchived",
                table: "PaymentHistories",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "IsArchived",
                table: "ECMembers",
                newName: "IsDeleted");
        }
    }
}
