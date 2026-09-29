using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddElectionPersonas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElectionPersonas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Permissions = table.Column<long>(type: "bigint", nullable: false),
                    MinCount = table.Column<int>(type: "integer", nullable: false),
                    MaxCount = table.Column<int>(type: "integer", nullable: true),
                    ShowOnPublicBoard = table.Column<bool>(type: "boolean", nullable: false),
                    TakesOverFromAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    DeclarationText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectionPersonas", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 29, 17, 33, 23, 403, DateTimeKind.Utc).AddTicks(8355));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 29, 17, 33, 23, 403, DateTimeKind.Utc).AddTicks(8296));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 29, 17, 33, 23, 403, DateTimeKind.Utc).AddTicks(8244));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 29, 17, 33, 23, 403, DateTimeKind.Utc).AddTicks(8184));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 29, 17, 33, 23, 403, DateTimeKind.Utc).AddTicks(8107));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 29, 17, 33, 23, 403, DateTimeKind.Utc).AddTicks(7925));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 29, 17, 33, 23, 403, DateTimeKind.Utc).AddTicks(7821));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 29, 17, 33, 23, 403, DateTimeKind.Utc).AddTicks(978));

            migrationBuilder.CreateIndex(
                name: "IX_ElectionPersonas_Name",
                table: "ElectionPersonas",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElectionPersonas");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 27, 16, 40, 31, 125, DateTimeKind.Utc).AddTicks(6117));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 27, 16, 40, 31, 125, DateTimeKind.Utc).AddTicks(6050));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 27, 16, 40, 31, 125, DateTimeKind.Utc).AddTicks(5993));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 27, 16, 40, 31, 125, DateTimeKind.Utc).AddTicks(5922));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 27, 16, 40, 31, 125, DateTimeKind.Utc).AddTicks(5829));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 27, 16, 40, 31, 125, DateTimeKind.Utc).AddTicks(5621));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 27, 16, 40, 31, 125, DateTimeKind.Utc).AddTicks(5460));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 27, 16, 40, 31, 124, DateTimeKind.Utc).AddTicks(6857));
        }
    }
}
