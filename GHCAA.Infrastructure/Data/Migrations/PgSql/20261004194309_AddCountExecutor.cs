using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddCountExecutor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExecutedByUserId",
                table: "ElectionApprovals",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 4, 19, 43, 8, 195, DateTimeKind.Utc).AddTicks(3323));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 4, 19, 43, 8, 195, DateTimeKind.Utc).AddTicks(3265));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 4, 19, 43, 8, 195, DateTimeKind.Utc).AddTicks(3213));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 4, 19, 43, 8, 195, DateTimeKind.Utc).AddTicks(3150));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 4, 19, 43, 8, 195, DateTimeKind.Utc).AddTicks(3073));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 4, 19, 43, 8, 195, DateTimeKind.Utc).AddTicks(2866));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 4, 19, 43, 8, 195, DateTimeKind.Utc).AddTicks(2757));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 4, 19, 43, 8, 194, DateTimeKind.Utc).AddTicks(5509));

            migrationBuilder.CreateIndex(
                name: "IX_ElectionApprovals_ExecutedByUserId",
                table: "ElectionApprovals",
                column: "ExecutedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ElectionApprovals_Users_ExecutedByUserId",
                table: "ElectionApprovals",
                column: "ExecutedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ElectionApprovals_Users_ExecutedByUserId",
                table: "ElectionApprovals");

            migrationBuilder.DropIndex(
                name: "IX_ElectionApprovals_ExecutedByUserId",
                table: "ElectionApprovals");

            migrationBuilder.DropColumn(
                name: "ExecutedByUserId",
                table: "ElectionApprovals");

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
        }
    }
}
