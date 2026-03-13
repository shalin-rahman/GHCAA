using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GHCAA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "PaymentHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptPath",
                table: "PaymentHistories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "EventRegistrations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PaymentConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    WalletNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AccountHolderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BranchName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RoutingNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Gateway = table.Column<int>(type: "integer", nullable: false),
                    GatewayPublicKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GatewaySecretKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GatewayCallbackUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Instructions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RequiresReceipt = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresReference = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentConfigurations", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AcademicRecords",
                columns: new[] { "Id", "AdmissionYear", "Degree", "InstitutionName", "IsGHC", "MemberId", "PassingYear", "Result", "Subject" },
                values: new object[,]
                {
                    { 1, 2013, "HSC", "Govt. Haraganga College", true, 1, 2015, null, "Science" },
                    { 2, 2014, "HSC", "Govt. Haraganga College", true, 101, 2016, null, "Humanities" }
                });

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 1,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 12, 23, 9, 44, 682, DateTimeKind.Utc).AddTicks(8723));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 2,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 12, 23, 9, 44, 683, DateTimeKind.Utc).AddTicks(998));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 3,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 12, 23, 9, 44, 683, DateTimeKind.Utc).AddTicks(1001));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 4,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 12, 23, 9, 44, 683, DateTimeKind.Utc).AddTicks(1004));

            migrationBuilder.InsertData(
                table: "ProfessionalRecords",
                columns: new[] { "Id", "Designation", "EndDate", "IsCurrent", "Location", "MemberId", "OrganizationName", "Sector", "StartDate" },
                values: new object[,]
                {
                    { 1, "Senior Software Architect", null, true, "Dhaka", 1, "GlobalTech Solutions", "Information Technology (IT) & Software", new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "Communications Manager", null, true, "Dhaka", 101, "Alumni Corp", "Advertising & Media", new DateTime(2021, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentConfigurations");

            migrationBuilder.DeleteData(
                table: "AcademicRecords",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AcademicRecords",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ProfessionalRecords",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ProfessionalRecords",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "PaymentHistories");

            migrationBuilder.DropColumn(
                name: "ReceiptPath",
                table: "PaymentHistories");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "EventRegistrations");

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 1,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 12, 19, 31, 31, 4, DateTimeKind.Utc).AddTicks(7716));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 2,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 12, 19, 31, 31, 4, DateTimeKind.Utc).AddTicks(8979));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 3,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 12, 19, 31, 31, 4, DateTimeKind.Utc).AddTicks(8981));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 4,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 12, 19, 31, 31, 4, DateTimeKind.Utc).AddTicks(8982));
        }
    }
}
