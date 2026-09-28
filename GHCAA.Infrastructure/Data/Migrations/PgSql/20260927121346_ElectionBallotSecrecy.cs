using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class ElectionBallotSecrecy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TieRule",
                table: "Elections",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Spec 023 FR-001. Old ballots are copied into GUID-keyed tables in random order, so
            // neither the key nor the row order says which ballot was cast first. The seat moves
            // from the ballot to each vote because one ballot now holds every seat.
            migrationBuilder.Sql("""
                CREATE TABLE "BallotIdMap" AS
                    SELECT "Id" AS "OldId", gen_random_uuid() AS "NewId" FROM "Ballots";
                CREATE TABLE "BallotsCopy" AS
                    SELECT m."NewId" AS "Id", b."ElectionId", b."SerialNumber", b."IsSpoiled"
                    FROM "Ballots" b JOIN "BallotIdMap" m ON m."OldId" = b."Id"
                    ORDER BY gen_random_uuid();
                CREATE TABLE "BallotVotesCopy" AS
                    SELECT gen_random_uuid() AS "Id", m."NewId" AS "BallotId", b."ElectionSeatId", v."NominationId"
                    FROM "BallotVotes" v
                    JOIN "Ballots" b ON b."Id" = v."BallotId"
                    JOIN "BallotIdMap" m ON m."OldId" = b."Id"
                    ORDER BY gen_random_uuid();
                DROP TABLE "BallotVotes";
                DROP TABLE "Ballots";
                DROP TABLE "BallotIdMap";
                """);

            migrationBuilder.CreateTable(
                name: "Ballots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ElectionId = table.Column<int>(type: "integer", nullable: false),
                    SerialNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IsSpoiled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ballots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ballots_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BallotVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BallotId = table.Column<Guid>(type: "uuid", nullable: false),
                    ElectionSeatId = table.Column<int>(type: "integer", nullable: false),
                    NominationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BallotVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BallotVotes_Ballots_BallotId",
                        column: x => x.BallotId,
                        principalTable: "Ballots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BallotVotes_ElectionSeats_ElectionSeatId",
                        column: x => x.ElectionSeatId,
                        principalTable: "ElectionSeats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BallotVotes_Nominations_NominationId",
                        column: x => x.NominationId,
                        principalTable: "Nominations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                INSERT INTO "Ballots" ("Id", "ElectionId", "SerialNumber", "IsSpoiled")
                    SELECT "Id", "ElectionId", "SerialNumber", "IsSpoiled" FROM "BallotsCopy";
                INSERT INTO "BallotVotes" ("Id", "BallotId", "ElectionSeatId", "NominationId")
                    SELECT "Id", "BallotId", "ElectionSeatId", "NominationId" FROM "BallotVotesCopy";
                DROP TABLE "BallotVotesCopy";
                DROP TABLE "BallotsCopy";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Ballots_ElectionId_SerialNumber",
                table: "Ballots",
                columns: new[] { "ElectionId", "SerialNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BallotVotes_BallotId_ElectionSeatId_NominationId",
                table: "BallotVotes",
                columns: new[] { "BallotId", "ElectionSeatId", "NominationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BallotVotes_ElectionSeatId",
                table: "BallotVotes",
                column: "ElectionSeatId");

            migrationBuilder.CreateIndex(
                name: "IX_BallotVotes_NominationId",
                table: "BallotVotes",
                column: "NominationId");

            migrationBuilder.CreateTable(
                name: "PendingBallots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ElectionId = table.Column<int>(type: "integer", nullable: false),
                    TrackingCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ChoicesJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingBallots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingBallots_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingBallots_ElectionId_TrackingCode",
                table: "PendingBallots",
                columns: new[] { "ElectionId", "TrackingCode" },
                unique: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Pending ballots have no place in the old shape. Refuse rather than lose votes: close
            // polling and count first, which moves them into Ballots.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "PendingBallots") THEN
                        RAISE EXCEPTION 'PendingBallots is not empty. Move pending ballots before rolling back.';
                    END IF;
                    RAISE NOTICE 'Rollback drops % ballot(s) that have no votes, blank or spoiled.',
                        (SELECT count(*) FROM "Ballots" b WHERE NOT EXISTS (SELECT 1 FROM "BallotVotes" v WHERE v."BallotId" = b."Id"));
                END $$;
                """);

            migrationBuilder.DropTable(
                name: "PendingBallots");

            // The old shape has one ballot per seat, so each ballot is split by seat. A seat left
            // blank had no row before either. A ballot with no votes at all, spoiled ones included,
            // has no seat to go under and is dropped. The old time columns get the polling close time,
            // because the real times are gone.
            migrationBuilder.Sql("""
                ALTER TABLE "BallotVotes" RENAME TO "BallotVotesCopy";
                ALTER TABLE "Ballots" RENAME TO "BallotsCopy";
                ALTER TABLE "BallotVotesCopy" DROP CONSTRAINT "FK_BallotVotes_Ballots_BallotId";
                ALTER TABLE "BallotVotesCopy" DROP CONSTRAINT "FK_BallotVotes_ElectionSeats_ElectionSeatId";
                ALTER TABLE "BallotVotesCopy" DROP CONSTRAINT "FK_BallotVotes_Nominations_NominationId";
                ALTER TABLE "BallotVotesCopy" DROP CONSTRAINT "PK_BallotVotes";
                ALTER TABLE "BallotsCopy" DROP CONSTRAINT "FK_Ballots_Elections_ElectionId";
                ALTER TABLE "BallotsCopy" DROP CONSTRAINT "PK_Ballots";
                DROP INDEX "IX_BallotVotes_BallotId_ElectionSeatId_NominationId";
                DROP INDEX "IX_BallotVotes_ElectionSeatId";
                DROP INDEX "IX_BallotVotes_NominationId";
                DROP INDEX "IX_Ballots_ElectionId_SerialNumber";
                """);

            migrationBuilder.DropColumn(
                name: "TieRule",
                table: "Elections");

            migrationBuilder.CreateTable(
                name: "Ballots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ElectionId = table.Column<int>(type: "integer", nullable: false),
                    ElectionSeatId = table.Column<int>(type: "integer", nullable: false),
                    SerialNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IsSpoiled = table.Column<bool>(type: "boolean", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ballots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ballots_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BallotVotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BallotId = table.Column<int>(type: "integer", nullable: false),
                    NominationId = table.Column<int>(type: "integer", nullable: false),
                    CastAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BallotVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BallotVotes_Ballots_BallotId",
                        column: x => x.BallotId,
                        principalTable: "Ballots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BallotVotes_Nominations_NominationId",
                        column: x => x.NominationId,
                        principalTable: "Nominations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                CREATE TABLE "BallotSeatMap" AS
                    SELECT DISTINCT v."BallotId" AS "NewBallotId", v."ElectionSeatId", b."ElectionId", b."SerialNumber", b."IsSpoiled"
                    FROM "BallotVotesCopy" v JOIN "BallotsCopy" b ON b."Id" = v."BallotId";
                ALTER TABLE "BallotSeatMap" ADD COLUMN "OldId" integer GENERATED BY DEFAULT AS IDENTITY;
                INSERT INTO "Ballots" ("Id", "ElectionId", "ElectionSeatId", "SerialNumber", "IsSpoiled", "IssuedAt")
                    OVERRIDING SYSTEM VALUE
                    SELECT m."OldId", m."ElectionId", m."ElectionSeatId", left(m."SerialNumber", 68) || '-' || m."ElectionSeatId", m."IsSpoiled", e."PollingClosesOn"
                    FROM "BallotSeatMap" m JOIN "Elections" e ON e."Id" = m."ElectionId";
                INSERT INTO "BallotVotes" ("BallotId", "NominationId", "CastAt")
                    SELECT m."OldId", v."NominationId", e."PollingClosesOn"
                    FROM "BallotVotesCopy" v
                    JOIN "BallotSeatMap" m ON m."NewBallotId" = v."BallotId" AND m."ElectionSeatId" = v."ElectionSeatId"
                    JOIN "Elections" e ON e."Id" = m."ElectionId";
                SELECT setval(pg_get_serial_sequence('"Ballots"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Ballots"), 0) + 1, false);
                DROP TABLE "BallotSeatMap";
                DROP TABLE "BallotVotesCopy";
                DROP TABLE "BallotsCopy";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Ballots_ElectionId_SerialNumber",
                table: "Ballots",
                columns: new[] { "ElectionId", "SerialNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BallotVotes_BallotId",
                table: "BallotVotes",
                column: "BallotId");

            migrationBuilder.CreateIndex(
                name: "IX_BallotVotes_NominationId",
                table: "BallotVotes",
                column: "NominationId");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 23, 19, 41, 59, 532, DateTimeKind.Utc).AddTicks(7065));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 23, 19, 41, 59, 532, DateTimeKind.Utc).AddTicks(7006));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 23, 19, 41, 59, 532, DateTimeKind.Utc).AddTicks(6948));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 23, 19, 41, 59, 532, DateTimeKind.Utc).AddTicks(6886));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 23, 19, 41, 59, 532, DateTimeKind.Utc).AddTicks(6809));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 23, 19, 41, 59, 532, DateTimeKind.Utc).AddTicks(6608));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 23, 19, 41, 59, 532, DateTimeKind.Utc).AddTicks(6494));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 23, 19, 41, 59, 531, DateTimeKind.Utc).AddTicks(9149));
        }
    }
}
