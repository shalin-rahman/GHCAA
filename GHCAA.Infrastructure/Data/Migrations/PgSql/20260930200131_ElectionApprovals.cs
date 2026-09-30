using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class ElectionApprovals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElectionApprovals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ElectionId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    PayloadJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    RequestedByUserId = table.Column<int>(type: "integer", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedByUserId = table.Column<int>(type: "integer", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedByUserId = table.Column<int>(type: "integer", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectionApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElectionApprovals_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ElectionApprovals_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElectionApprovals_Users_RejectedByUserId",
                        column: x => x.RejectedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElectionApprovals_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 30, 20, 1, 30, 345, DateTimeKind.Utc).AddTicks(8641));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 30, 20, 1, 30, 345, DateTimeKind.Utc).AddTicks(8580));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 30, 20, 1, 30, 345, DateTimeKind.Utc).AddTicks(8523));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 30, 20, 1, 30, 345, DateTimeKind.Utc).AddTicks(8459));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 30, 20, 1, 30, 345, DateTimeKind.Utc).AddTicks(8380));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 30, 20, 1, 30, 345, DateTimeKind.Utc).AddTicks(8190));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 30, 20, 1, 30, 345, DateTimeKind.Utc).AddTicks(8078));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 30, 20, 1, 30, 345, DateTimeKind.Utc).AddTicks(328));

            migrationBuilder.CreateIndex(
                name: "IX_ElectionApprovals_ApprovedByUserId",
                table: "ElectionApprovals",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionApprovals_ElectionId_Action_ExecutedAt_RejectedAt",
                table: "ElectionApprovals",
                columns: new[] { "ElectionId", "Action", "ExecutedAt", "RejectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ElectionApprovals_RejectedByUserId",
                table: "ElectionApprovals",
                column: "RejectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionApprovals_RequestedByUserId",
                table: "ElectionApprovals",
                column: "RequestedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElectionApprovals");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 30, 2, 1, 36, 792, DateTimeKind.Utc).AddTicks(5463));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 30, 2, 1, 36, 792, DateTimeKind.Utc).AddTicks(5404));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 30, 2, 1, 36, 792, DateTimeKind.Utc).AddTicks(5350));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 30, 2, 1, 36, 792, DateTimeKind.Utc).AddTicks(5288));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 30, 2, 1, 36, 792, DateTimeKind.Utc).AddTicks(5209));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 30, 2, 1, 36, 792, DateTimeKind.Utc).AddTicks(5033));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 30, 2, 1, 36, 792, DateTimeKind.Utc).AddTicks(4932));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 30, 2, 1, 36, 791, DateTimeKind.Utc).AddTicks(8108));
        }
    }
}
