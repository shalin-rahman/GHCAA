using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddApprovalOpenKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OpenKey",
                table: "ElectionApprovals",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            // Rows that still block a new request get their key, built as ElectionApproval.KeyFor does
            // (6 is Count, 7 is EmergencyRevoke). If two requests for one key slipped in before this
            // column existed, only the oldest gets it. Revokes that had already expired are left out,
            // because the old code may have written their expired row already. The appointment id is
            // read with a pattern rather than a json cast, so one bad payload cannot stop the migration.
            migrationBuilder.Sql("""
                UPDATE "ElectionApprovals" a SET "OpenKey" = k."Key"
                FROM (
                    SELECT DISTINCT ON ("Key") "Id", "Key"
                    FROM (
                        SELECT "Id", "RequestedAt",
                               CASE WHEN "Action" = 7
                                    THEN "ElectionId" || ':7:' || substring("PayloadJson" from '"AppointmentId"\s*:\s*(\d+)')
                                    ELSE "ElectionId" || ':' || "Action" END AS "Key"
                        FROM "ElectionApprovals"
                        WHERE "RejectedAt" IS NULL AND "ExpiresAt" > now()
                          AND ("ExecutedAt" IS NULL
                               OR ("Action" = 6 AND "ApprovedAt" IS NOT NULL AND "ConsumedAt" IS NULL))
                    ) s
                    WHERE "Key" IS NOT NULL
                    ORDER BY "Key", "RequestedAt", "Id"
                ) k
                WHERE a."Id" = k."Id";
                """);

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 14, 49, 26, 70, DateTimeKind.Utc).AddTicks(9699));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 14, 49, 26, 70, DateTimeKind.Utc).AddTicks(9641));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 14, 49, 26, 70, DateTimeKind.Utc).AddTicks(9588));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 14, 49, 26, 70, DateTimeKind.Utc).AddTicks(9527));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 14, 49, 26, 70, DateTimeKind.Utc).AddTicks(9438));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 14, 49, 26, 70, DateTimeKind.Utc).AddTicks(9243));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 14, 49, 26, 70, DateTimeKind.Utc).AddTicks(9144));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 14, 49, 26, 70, DateTimeKind.Utc).AddTicks(2818));

            migrationBuilder.CreateIndex(
                name: "IX_ElectionApprovals_OpenKey",
                table: "ElectionApprovals",
                column: "OpenKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ElectionApprovals_OpenKey",
                table: "ElectionApprovals");

            migrationBuilder.DropColumn(
                name: "OpenKey",
                table: "ElectionApprovals");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 12, 42, 46, 852, DateTimeKind.Utc).AddTicks(7496));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 12, 42, 46, 852, DateTimeKind.Utc).AddTicks(7437));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 12, 42, 46, 852, DateTimeKind.Utc).AddTicks(7364));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 12, 42, 46, 852, DateTimeKind.Utc).AddTicks(7301));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 12, 42, 46, 852, DateTimeKind.Utc).AddTicks(7227));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 12, 42, 46, 852, DateTimeKind.Utc).AddTicks(7029));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 12, 42, 46, 852, DateTimeKind.Utc).AddTicks(6916));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 12, 42, 46, 851, DateTimeKind.Utc).AddTicks(9896));
        }
    }
}
