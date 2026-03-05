using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialDayThemes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpecialDayThemes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BackgroundColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    TextColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    AnnouncementText = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialDayThemes", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 5, 10, 45, 47, 859, DateTimeKind.Utc).AddTicks(4290));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 5, 10, 45, 47, 859, DateTimeKind.Utc).AddTicks(5318));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 5, 10, 45, 47, 859, DateTimeKind.Utc).AddTicks(5320));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdateDate",
                value: new DateTime(2026, 3, 5, 10, 45, 47, 860, DateTimeKind.Utc).AddTicks(5684));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 10, 45, 48, 406, DateTimeKind.Utc).AddTicks(2259));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 10, 45, 48, 406, DateTimeKind.Utc).AddTicks(3526));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 10, 45, 48, 406, DateTimeKind.Utc).AddTicks(3529));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 10, 45, 48, 406, DateTimeKind.Utc).AddTicks(3531));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 10, 45, 48, 406, DateTimeKind.Utc).AddTicks(3533));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 10, 45, 48, 406, DateTimeKind.Utc).AddTicks(3536));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$B2gnAojWq1Q2E5annACDS.VhtBfrjJyFF/3QyX/5q/1Nw90oYMURK");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$.DTVH2OaW50nSLp0.FSg0.CDs22llqHLjjyXpi/hZ.1bc1scmpGSW");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpecialDayThemes");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 2, 19, 13, 56, 45, DateTimeKind.Utc).AddTicks(83));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 2, 19, 13, 56, 45, DateTimeKind.Utc).AddTicks(981));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 2, 19, 13, 56, 45, DateTimeKind.Utc).AddTicks(983));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdateDate",
                value: new DateTime(2026, 3, 2, 19, 13, 56, 46, DateTimeKind.Utc).AddTicks(1012));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 19, 13, 56, 561, DateTimeKind.Utc).AddTicks(1418));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 19, 13, 56, 561, DateTimeKind.Utc).AddTicks(2645));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 19, 13, 56, 561, DateTimeKind.Utc).AddTicks(2649));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 19, 13, 56, 561, DateTimeKind.Utc).AddTicks(2651));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 19, 13, 56, 561, DateTimeKind.Utc).AddTicks(2653));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 19, 13, 56, 561, DateTimeKind.Utc).AddTicks(2656));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$0ZO8YtuWvKUkGqtdkVCGFuXAJFujW1Ttzxj7EQ8djnA/m9tYL4ruu");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$bgS883RYYSiJE8oWvTYqne65D5Ld2CQIOitsDrHVh8DVO0I1a3Sm6");
        }
    }
}
