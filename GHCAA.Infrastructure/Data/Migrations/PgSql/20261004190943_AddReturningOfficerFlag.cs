using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddReturningOfficerFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReturningOfficer",
                table: "ElectionAppointments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 4, 19, 9, 41, 424, DateTimeKind.Utc).AddTicks(5939));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 4, 19, 9, 41, 424, DateTimeKind.Utc).AddTicks(5845));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 4, 19, 9, 41, 424, DateTimeKind.Utc).AddTicks(5762));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 4, 19, 9, 41, 424, DateTimeKind.Utc).AddTicks(5671));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 4, 19, 9, 41, 424, DateTimeKind.Utc).AddTicks(5559));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 4, 19, 9, 41, 424, DateTimeKind.Utc).AddTicks(5282));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 4, 19, 9, 41, 424, DateTimeKind.Utc).AddTicks(5142));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 4, 19, 9, 41, 423, DateTimeKind.Utc).AddTicks(6349));

            migrationBuilder.CreateIndex(
                name: "IX_ElectionAppointments_ElectionId_ReturningOfficer",
                table: "ElectionAppointments",
                column: "ElectionId",
                unique: true,
                filter: "\"IsReturningOfficer\" = true AND \"RevokedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ElectionAppointments_ElectionId_ReturningOfficer",
                table: "ElectionAppointments");

            migrationBuilder.DropColumn(
                name: "IsReturningOfficer",
                table: "ElectionAppointments");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 44, 9, 876, DateTimeKind.Utc).AddTicks(5610));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 44, 9, 876, DateTimeKind.Utc).AddTicks(5543));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 44, 9, 876, DateTimeKind.Utc).AddTicks(5478));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 44, 9, 876, DateTimeKind.Utc).AddTicks(5408));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 44, 9, 876, DateTimeKind.Utc).AddTicks(5324));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 44, 9, 876, DateTimeKind.Utc).AddTicks(5127));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 44, 9, 876, DateTimeKind.Utc).AddTicks(5015));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 44, 9, 875, DateTimeKind.Utc).AddTicks(7829));
        }
    }
}
