using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmailTemplatesWithDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "EmailTemplates",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "LastUpdated" },
                values: new object[] { "Sent during registration and password reset", new DateTime(2026, 3, 5, 10, 52, 2, 79, DateTimeKind.Utc).AddTicks(5842) });

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "LastUpdated" },
                values: new object[] { "Sent after admin approves registration", new DateTime(2026, 3, 5, 10, 52, 2, 79, DateTimeKind.Utc).AddTicks(7099) });

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "LastUpdated" },
                values: new object[] { "Sent after event registration approval", new DateTime(2026, 3, 5, 10, 52, 2, 79, DateTimeKind.Utc).AddTicks(7102) });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdateDate",
                value: new DateTime(2026, 3, 5, 10, 52, 2, 80, DateTimeKind.Utc).AddTicks(8306));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 10, 52, 2, 583, DateTimeKind.Utc).AddTicks(1809));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 10, 52, 2, 583, DateTimeKind.Utc).AddTicks(3078));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 10, 52, 2, 583, DateTimeKind.Utc).AddTicks(3081));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 10, 52, 2, 583, DateTimeKind.Utc).AddTicks(3084));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 10, 52, 2, 583, DateTimeKind.Utc).AddTicks(3086));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 10, 52, 2, 583, DateTimeKind.Utc).AddTicks(3090));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$TPYZxS1G3TSMBj2taptvoOCun9Ooyqp/RghIbuGGWFWZZw/.2m/Ii");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$m39S2eEjr44HaTGJEj9XluwihNh3dGjtECz2C6GU875werROmPwKG");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "EmailTemplates");

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
    }
}
