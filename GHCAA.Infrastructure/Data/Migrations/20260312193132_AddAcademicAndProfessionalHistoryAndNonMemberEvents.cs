using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicAndProfessionalHistoryAndNonMemberEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventRegistrations_Members_MemberId",
                table: "EventRegistrations");

            migrationBuilder.AlterColumn<int>(
                name: "MemberId",
                table: "EventRegistrations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "GuestEmail",
                table: "EventRegistrations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestMobile",
                table: "EventRegistrations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestName",
                table: "EventRegistrations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNonMember",
                table: "EventRegistrations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowNonMembers",
                table: "AlumniEvents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AcademicRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MemberId = table.Column<int>(type: "integer", nullable: false),
                    InstitutionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Degree = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Subject = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AdmissionYear = table.Column<int>(type: "integer", nullable: true),
                    PassingYear = table.Column<int>(type: "integer", nullable: false),
                    IsGHC = table.Column<bool>(type: "boolean", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcademicRecords_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfessionalRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MemberId = table.Column<int>(type: "integer", nullable: false),
                    OrganizationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Designation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sector = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessionalRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfessionalRecords_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AlumniEvents",
                keyColumn: "Id",
                keyValue: 1,
                column: "AllowNonMembers",
                value: false);

            migrationBuilder.UpdateData(
                table: "EventGalleries",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsFeatured",
                value: true);

            migrationBuilder.UpdateData(
                table: "EventGalleries",
                keyColumn: "Id",
                keyValue: 2,
                column: "IsFeatured",
                value: true);

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

            migrationBuilder.UpdateData(
                table: "SpecialDayThemes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AnnouncementText", "BackgroundColor", "EndDate", "StartDate", "TextColor" },
                values: new object[] { "Happy 55th Independence Day! Celebrating our glorious history.", "#d63031", new DateTime(2026, 3, 30, 23, 59, 59, 0, DateTimeKind.Utc), new DateTime(2026, 3, 5, 0, 0, 0, 0, DateTimeKind.Utc), "#ffffff" });

            migrationBuilder.CreateIndex(
                name: "IX_AcademicRecords_MemberId",
                table: "AcademicRecords",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionalRecords_MemberId",
                table: "ProfessionalRecords",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventRegistrations_Members_MemberId",
                table: "EventRegistrations",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventRegistrations_Members_MemberId",
                table: "EventRegistrations");

            migrationBuilder.DropTable(
                name: "AcademicRecords");

            migrationBuilder.DropTable(
                name: "ProfessionalRecords");

            migrationBuilder.DropColumn(
                name: "GuestEmail",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "GuestMobile",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "GuestName",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "IsNonMember",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "AllowNonMembers",
                table: "AlumniEvents");

            migrationBuilder.AlterColumn<int>(
                name: "MemberId",
                table: "EventRegistrations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "EventGalleries",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsFeatured",
                value: false);

            migrationBuilder.UpdateData(
                table: "EventGalleries",
                keyColumn: "Id",
                keyValue: 2,
                column: "IsFeatured",
                value: false);

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 1,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 6, 6, 56, 42, 778, DateTimeKind.Utc).AddTicks(3853));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 2,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 6, 6, 56, 42, 778, DateTimeKind.Utc).AddTicks(5249));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 3,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 6, 6, 56, 42, 778, DateTimeKind.Utc).AddTicks(5250));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 4,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 6, 6, 56, 42, 778, DateTimeKind.Utc).AddTicks(5252));

            migrationBuilder.UpdateData(
                table: "SpecialDayThemes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AnnouncementText", "BackgroundColor", "EndDate", "StartDate", "TextColor" },
                values: new object[] { "Celebrating 55 Years of Victory! Happy Independence Day to all Haragangians.", "#213921", new DateTime(2026, 3, 27, 23, 59, 59, 0, DateTimeKind.Utc), new DateTime(2026, 3, 20, 0, 0, 0, 0, DateTimeKind.Utc), "#dc2626" });

            migrationBuilder.AddForeignKey(
                name: "FK_EventRegistrations_Members_MemberId",
                table: "EventRegistrations",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
