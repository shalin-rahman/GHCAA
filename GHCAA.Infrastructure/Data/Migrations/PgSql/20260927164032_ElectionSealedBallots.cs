using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class ElectionSealedBallots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Pending rows hold plain choices. They cannot be sealed here without the returning
            // officer's key, so count or clear them first.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "PendingBallots") THEN
                        RAISE EXCEPTION 'PendingBallots is not empty. Count or clear the open election before this migration.';
                    END IF;
                END $$;
                """);

            migrationBuilder.CreateTable(
                name: "BallotReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ElectionId = table.Column<int>(type: "integer", nullable: false),
                    TrackingCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BallotReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BallotReceipts_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Serial numbers become receipts, written in random order so the receipt rows do not
            // follow the ballot rows.
            migrationBuilder.Sql("""
                INSERT INTO "BallotReceipts" ("Id", "ElectionId", "TrackingCode")
                    SELECT gen_random_uuid(), "ElectionId", "SerialNumber" FROM "Ballots" ORDER BY random();
                """);

            migrationBuilder.DropIndex(
                name: "IX_PendingBallots_ElectionId_TrackingCode",
                table: "PendingBallots");

            migrationBuilder.DropIndex(
                name: "IX_Ballots_ElectionId_SerialNumber",
                table: "Ballots");

            migrationBuilder.DropColumn(
                name: "TrackingCode",
                table: "PendingBallots");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "Ballots");

            migrationBuilder.RenameColumn(
                name: "ChoicesJson",
                table: "PendingBallots",
                newName: "SealedChoices");

            migrationBuilder.AddColumn<string>(
                name: "BallotKeyFingerprint",
                table: "Elections",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BallotPublicKey",
                table: "Elections",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_PendingBallots_ElectionId",
                table: "PendingBallots",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Ballots_ElectionId",
                table: "Ballots",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_BallotReceipts_ElectionId_TrackingCode",
                table: "BallotReceipts",
                columns: new[] { "ElectionId", "TrackingCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sealed choices cannot turn back into plain ones.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "PendingBallots") THEN
                        RAISE EXCEPTION 'PendingBallots is not empty. Count the open election before rolling back.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropIndex(
                name: "IX_PendingBallots_ElectionId",
                table: "PendingBallots");

            migrationBuilder.DropIndex(
                name: "IX_Ballots_ElectionId",
                table: "Ballots");

            migrationBuilder.DropColumn(
                name: "BallotKeyFingerprint",
                table: "Elections");

            migrationBuilder.DropColumn(
                name: "BallotPublicKey",
                table: "Elections");

            migrationBuilder.RenameColumn(
                name: "SealedChoices",
                table: "PendingBallots",
                newName: "ChoicesJson");

            migrationBuilder.AddColumn<string>(
                name: "TrackingCode",
                table: "PendingBallots",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "Ballots",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            // Receipts and ballots are no longer linked, so each ballot takes any receipt of its
            // election. A ballot left without one gets a fresh value so the unique index holds.
            migrationBuilder.Sql("""
                WITH b AS (SELECT "Id", "ElectionId", row_number() OVER (PARTITION BY "ElectionId" ORDER BY random()) AS n FROM "Ballots"),
                     r AS (SELECT "ElectionId", "TrackingCode", row_number() OVER (PARTITION BY "ElectionId" ORDER BY random()) AS n FROM "BallotReceipts")
                UPDATE "Ballots" SET "SerialNumber" = r."TrackingCode"
                    FROM b JOIN r ON r."ElectionId" = b."ElectionId" AND r.n = b.n
                    WHERE "Ballots"."Id" = b."Id";
                UPDATE "Ballots" SET "SerialNumber" = gen_random_uuid()::text WHERE "SerialNumber" IS NULL;
                ALTER TABLE "Ballots" ALTER COLUMN "SerialNumber" SET NOT NULL;
                """);

            migrationBuilder.DropTable(
                name: "BallotReceipts");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 27, 12, 13, 44, 208, DateTimeKind.Utc).AddTicks(5300));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 27, 12, 13, 44, 208, DateTimeKind.Utc).AddTicks(5243));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 27, 12, 13, 44, 208, DateTimeKind.Utc).AddTicks(5186));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 27, 12, 13, 44, 208, DateTimeKind.Utc).AddTicks(5123));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 27, 12, 13, 44, 208, DateTimeKind.Utc).AddTicks(5046));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 27, 12, 13, 44, 208, DateTimeKind.Utc).AddTicks(4841));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 27, 12, 13, 44, 208, DateTimeKind.Utc).AddTicks(4734));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 27, 12, 13, 44, 207, DateTimeKind.Utc).AddTicks(7777));

            migrationBuilder.CreateIndex(
                name: "IX_PendingBallots_ElectionId_TrackingCode",
                table: "PendingBallots",
                columns: new[] { "ElectionId", "TrackingCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ballots_ElectionId_SerialNumber",
                table: "Ballots",
                columns: new[] { "ElectionId", "SerialNumber" },
                unique: true);
        }
    }
}
