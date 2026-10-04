using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddBallotSealVersionAndCountApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Elections that exist now were sealed without the election id, so they stay on
            // version 1. New elections get 2 from the model (TODO 37.13i).
            migrationBuilder.AddColumn<int>(
                name: "BallotSealVersion",
                table: "Elections",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // After a Down and a fresh Up, an election that already holds version 2 ballots must
            // stay on 2, or its count would refuse every ballot.
            migrationBuilder.Sql("""
                UPDATE "Elections" e SET "BallotSealVersion" = 2
                WHERE EXISTS (SELECT 1 FROM "PendingBallots" p
                              WHERE p."ElectionId" = e."Id" AND p."SealedChoices" LIKE 'v2:%');
                """);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsumedAt",
                table: "ElectionApprovals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 44, 9, 876, DateTimeKind.Utc).AddTicks(5610));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 44, 9, 876, DateTimeKind.Utc).AddTicks(5543));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 44, 9, 876, DateTimeKind.Utc).AddTicks(5478));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 44, 9, 876, DateTimeKind.Utc).AddTicks(5408));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 44, 9, 876, DateTimeKind.Utc).AddTicks(5324));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 44, 9, 876, DateTimeKind.Utc).AddTicks(5127));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 44, 9, 876, DateTimeKind.Utc).AddTicks(5015));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 44, 9, 875, DateTimeKind.Utc).AddTicks(7829));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BallotSealVersion",
                table: "Elections");

            migrationBuilder.DropColumn(
                name: "ConsumedAt",
                table: "ElectionApprovals");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 12, 54, 494, DateTimeKind.Utc).AddTicks(4739));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 12, 54, 494, DateTimeKind.Utc).AddTicks(4684));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 12, 54, 494, DateTimeKind.Utc).AddTicks(4631));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 12, 54, 494, DateTimeKind.Utc).AddTicks(4571));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 12, 54, 494, DateTimeKind.Utc).AddTicks(4493));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 12, 54, 494, DateTimeKind.Utc).AddTicks(4282));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 12, 54, 494, DateTimeKind.Utc).AddTicks(4176));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 10, 3, 20, 12, 54, 493, DateTimeKind.Utc).AddTicks(7914));
        }
    }
}
