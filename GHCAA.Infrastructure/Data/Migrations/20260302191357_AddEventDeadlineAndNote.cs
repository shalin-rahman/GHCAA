using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventDeadlineAndNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNote",
                table: "AlumniEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationDeadline",
                table: "AlumniEvents",
                type: "timestamp with time zone",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminNote",
                table: "AlumniEvents");

            migrationBuilder.DropColumn(
                name: "RegistrationDeadline",
                table: "AlumniEvents");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 55, DateTimeKind.Utc).AddTicks(182));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 55, DateTimeKind.Utc).AddTicks(1162));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 55, DateTimeKind.Utc).AddTicks(1164));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdateDate",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 56, DateTimeKind.Utc).AddTicks(3607));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 598, DateTimeKind.Utc).AddTicks(6318));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 598, DateTimeKind.Utc).AddTicks(8058));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 598, DateTimeKind.Utc).AddTicks(8062));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 598, DateTimeKind.Utc).AddTicks(8064));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 598, DateTimeKind.Utc).AddTicks(8066));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 598, DateTimeKind.Utc).AddTicks(8068));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$G7.EABGSSKHqD7Mpffh2Je3GzGJTp0mQEs0D6KigmPaA0f5J2hFAm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$QpeEP.gPueRwQWnby7/tvOR7KeCaRnadiGsHXgP/rOMdpx8.z0NZu");
        }
    }
}
