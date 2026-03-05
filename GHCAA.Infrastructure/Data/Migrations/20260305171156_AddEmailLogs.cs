using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipientEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    TemplateCode = table.Column<string>(type: "text", nullable: true),
                    SentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    InitiatedByMemberId = table.Column<int>(type: "integer", nullable: true),
                    TargetAudience = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailLogs", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 5, 17, 11, 55, 232, DateTimeKind.Utc).AddTicks(9857));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 5, 17, 11, 55, 233, DateTimeKind.Utc).AddTicks(930));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 5, 17, 11, 55, 233, DateTimeKind.Utc).AddTicks(933));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdateDate",
                value: new DateTime(2026, 3, 5, 17, 11, 55, 234, DateTimeKind.Utc).AddTicks(2583));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 17, 11, 55, 819, DateTimeKind.Utc).AddTicks(8530));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 17, 11, 55, 820, DateTimeKind.Utc).AddTicks(88));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 17, 11, 55, 820, DateTimeKind.Utc).AddTicks(92));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 17, 11, 55, 820, DateTimeKind.Utc).AddTicks(94));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 17, 11, 55, 820, DateTimeKind.Utc).AddTicks(97));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 17, 11, 55, 820, DateTimeKind.Utc).AddTicks(101));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$vcukc.0F.Y2hvHPuFf8L0.ErZyqQo.TwHNd9shgbqDjEbbSk2O.Ly");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$WJgG9GNtOVE2UqmX/hvNtOfTAXJJfx9pRzZQRTo8gJTMoqtjsaHVi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailLogs");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 5, 10, 52, 2, 79, DateTimeKind.Utc).AddTicks(5842));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 5, 10, 52, 2, 79, DateTimeKind.Utc).AddTicks(7099));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 5, 10, 52, 2, 79, DateTimeKind.Utc).AddTicks(7102));

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
    }
}
