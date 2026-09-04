using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <summary>
    /// Work Package 82.32. PaymentHistory.MemberId was non-nullable, but EventRegistration.MemberId
    /// is already nullable "for non-members" — a guest paying an event fee for an event with
    /// AllowNonMembers had no Member row, and GatewaysController.InitiatePayment attributed the
    /// payment to a fabricated Id 0, which does not exist and threw a foreign-key DbUpdateException.
    ///
    /// Also fixes the two unique indexes on PaymentHistories to exclude soft-deleted rows. Before
    /// this, deleting a wrong or duplicate payment via 82.16's new soft delete left its
    /// TransactionId/GatewayPaymentId permanently unusable — the ordinary case that endpoint exists
    /// for, not an edge case.
    ///
    /// Hand-written rather than scaffolded: `dotnet ef migrations add` also emitted ~630 UpdateData
    /// operations against unrelated Users.SecurityStamp and EmailTemplates.LastUpdated rows (see
    /// gotcha_pending_model_changes_seed) — none of which belongs in this migration.
    /// </summary>
    public partial class FixPaymentHistoryGuestAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentHistories_GatewayPaymentId",
                table: "PaymentHistories");

            migrationBuilder.DropIndex(
                name: "IX_PaymentHistories_TransactionId",
                table: "PaymentHistories");

            migrationBuilder.AlterColumn<int>(
                name: "MemberId",
                table: "PaymentHistories",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistories_GatewayPaymentId",
                table: "PaymentHistories",
                column: "GatewayPaymentId",
                unique: true,
                filter: "\"GatewayPaymentId\" IS NOT NULL AND \"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistories_TransactionId",
                table: "PaymentHistories",
                column: "TransactionId",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentHistories_GatewayPaymentId",
                table: "PaymentHistories");

            migrationBuilder.DropIndex(
                name: "IX_PaymentHistories_TransactionId",
                table: "PaymentHistories");

            // Reversing MemberId to NOT NULL is not attempted: any guest payment recorded while
            // this migration was applied has no member to default it to, and defaulting to 0 is
            // the exact bug this migration exists to remove.
            migrationBuilder.AlterColumn<int>(
                name: "MemberId",
                table: "PaymentHistories",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int?),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistories_GatewayPaymentId",
                table: "PaymentHistories",
                column: "GatewayPaymentId",
                unique: true,
                filter: "\"GatewayPaymentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistories_TransactionId",
                table: "PaymentHistories",
                column: "TransactionId",
                unique: true);
        }
    }
}
