using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class SeedEcPositionLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The EmailTemplates.LastUpdated UpdateData calls that `dotnet ef migrations add`
            // scaffolds here are spurious churn from that seed's DateTime.UtcNow default
            // re-evaluating at generation time (see gotcha_pending_model_changes_seed) — not part
            // of this change, so they're left out of this migration.
            migrationBuilder.InsertData(
                table: "Lookups",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "Label", "LookupGroup", "Value" },
                values: new object[,]
                {
                    { 4001, null, 1, true, "President", "ECPosition", "President" },
                    { 4002, null, 2, true, "Vice President", "ECPosition", "VicePresident" },
                    { 4003, null, 3, true, "General Secretary", "ECPosition", "GeneralSecretary" },
                    { 4004, null, 4, true, "Office Secretary", "ECPosition", "OfficeSecretary" },
                    { 4005, null, 5, true, "Joint Secretary 1", "ECPosition", "JointSecretary1" },
                    { 4006, null, 6, true, "Joint Secretary 2", "ECPosition", "JointSecretary2" },
                    { 4007, null, 7, true, "Treasurer", "ECPosition", "Treasurer" },
                    { 4008, null, 8, true, "Media, Cultural & Sports Secretary", "ECPosition", "MediaCulturalAndSportsSecretary" },
                    { 4009, null, 9, true, "Organizational Secretary", "ECPosition", "OrganizationalSecretary" },
                    { 4010, null, 10, true, "Information & Technology Secretary", "ECPosition", "InformationAndTechnologySecretary" },
                    { 4011, null, 11, true, "Member-1", "ECPosition", "Member1" },
                    { 4012, null, 12, true, "Member-2", "ECPosition", "Member2" },
                    { 4013, null, 13, true, "Law Secretary", "ECPosition", "LawSecretary" },
                    { 4014, null, 14, true, "Immediate Past President", "ECPosition", "ImmediatePastPresident" },
                    { 4015, null, 15, true, "Institutional Representative", "ECPosition", "InstitutionalRepresentative" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4001);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4002);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4003);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4004);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4005);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4006);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4007);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4008);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4009);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4010);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4011);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4012);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4013);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4014);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4015);
        }
    }
}
