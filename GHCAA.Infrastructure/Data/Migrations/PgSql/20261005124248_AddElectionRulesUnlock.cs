using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddElectionRulesUnlock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElectionRulesUnlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpenedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedByUserId = table.Column<int>(type: "integer", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectionRulesUnlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElectionRulesUnlocks_Users_ClosedByUserId",
                        column: x => x.ClosedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElectionRulesUnlocks_Users_OpenedByUserId",
                        column: x => x.OpenedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 12, 42, 46, 852, DateTimeKind.Utc).AddTicks(7496));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 12, 42, 46, 852, DateTimeKind.Utc).AddTicks(7437));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 12, 42, 46, 852, DateTimeKind.Utc).AddTicks(7364));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 12, 42, 46, 852, DateTimeKind.Utc).AddTicks(7301));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 12, 42, 46, 852, DateTimeKind.Utc).AddTicks(7227));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 12, 42, 46, 852, DateTimeKind.Utc).AddTicks(7029));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 12, 42, 46, 852, DateTimeKind.Utc).AddTicks(6916));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 5, 12, 42, 46, 851, DateTimeKind.Utc).AddTicks(9896));

            migrationBuilder.CreateIndex(
                name: "IX_ElectionRulesUnlocks_ClosedAt_ExpiresAt",
                table: "ElectionRulesUnlocks",
                columns: new[] { "ClosedAt", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ElectionRulesUnlocks_ClosedByUserId",
                table: "ElectionRulesUnlocks",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionRulesUnlocks_OpenedByUserId",
                table: "ElectionRulesUnlocks",
                column: "OpenedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElectionRulesUnlocks");

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
        }
    }
}
