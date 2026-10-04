using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddCommitteeVacancyReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EndNote",
                table: "ECMembers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndReason",
                table: "ECMembers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 12, 54, 494, DateTimeKind.Utc).AddTicks(4739));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 12, 54, 494, DateTimeKind.Utc).AddTicks(4684));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 12, 54, 494, DateTimeKind.Utc).AddTicks(4631));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 12, 54, 494, DateTimeKind.Utc).AddTicks(4571));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 12, 54, 494, DateTimeKind.Utc).AddTicks(4493));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 12, 54, 494, DateTimeKind.Utc).AddTicks(4282));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 12, 54, 494, DateTimeKind.Utc).AddTicks(4176));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 12, 54, 493, DateTimeKind.Utc).AddTicks(7914));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndNote",
                table: "ECMembers");

            migrationBuilder.DropColumn(
                name: "EndReason",
                table: "ECMembers");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 2, 19, 31, 34, 202, DateTimeKind.Utc).AddTicks(2606));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 2, 19, 31, 34, 202, DateTimeKind.Utc).AddTicks(2544));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 2, 19, 31, 34, 202, DateTimeKind.Utc).AddTicks(2488));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 2, 19, 31, 34, 202, DateTimeKind.Utc).AddTicks(2428));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 2, 19, 31, 34, 202, DateTimeKind.Utc).AddTicks(2346));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 2, 19, 31, 34, 202, DateTimeKind.Utc).AddTicks(2160));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 2, 19, 31, 34, 202, DateTimeKind.Utc).AddTicks(2053));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 2, 19, 31, 34, 201, DateTimeKind.Utc).AddTicks(5491));
        }
    }
}
