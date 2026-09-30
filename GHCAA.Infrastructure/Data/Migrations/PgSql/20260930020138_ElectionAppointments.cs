using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class ElectionAppointments : Migration
    {
        private const string PostgresProvider = "Npgsql.EntityFrameworkCore.PostgreSQL";

        // Spec 023 (37.12d). ElectionOfficers rows become accepted appointments, since those people were
        // already acting. Personas match by name from the old ElectionRole enum (0 Returning Officer,
        // 1 Assistant Returning Officer, 2 Polling Officer, 3 Scrutineer). The personas table has no
        // unique name index, so the four are added with NOT EXISTS rather than ON CONFLICT. The boot
        // seeder then adds the rest by name. The old table did not record who appointed an officer, so
        // AppointedByUserId is the first SuperAdmin, or the officer when there is none. An officer whose
        // member has no login is not copied; they could not sign in to act anyway. Roles 1 to 3 are seeded
        // with fixed ids, so the Roles sequence is moved past them before ElectionOfficial is inserted.
        private const string CopyOfficersUpSql = """
            INSERT INTO "ElectionPersonas" ("Name", "GroupName", "Description", "Permissions", "MinCount", "MaxCount",
                "ShowOnPublicBoard", "TakesOverFromAdmin", "DeclarationText", "SortOrder", "IsActive", "CreatedAt", "UpdatedAt")
            SELECT v.name, 'Officials', v.description, v.permissions, v.min_count, v.max_count, TRUE, FALSE,
                E'I hereby solemnly declare that I shall:\n\nRemain impartial in every act and decision.\nNot campaign for or against any candidate.\nNot favour any candidate, panel or group.\nNot disclose confidential information.\nPerform my duties honestly and to the best of my ability.',
                v.sort_order, TRUE, now(), now()
            FROM (VALUES
                ('Returning Officer', 'Holds the ballot key and runs the count.', 2387::bigint, 1, 1::int, 40),
                ('Assistant Returning Officer', 'Helps the Returning Officer with nominations.', 17::bigint, 0, NULL::int, 50),
                ('Polling Officer', 'Staffs a polling station on election day.', 1::bigint, 0, NULL::int, 70),
                ('Scrutineer', 'Checks nominations for eligibility.', 17::bigint, 0, NULL::int, 80)
            ) AS v(name, description, permissions, min_count, max_count, sort_order)
            WHERE EXISTS (SELECT 1 FROM "ElectionOfficers")
              AND NOT EXISTS (SELECT 1 FROM "ElectionPersonas" p WHERE p."Name" = v.name);

            WITH member_user AS (
                SELECT DISTINCT ON ("MemberId") "MemberId", "Id" AS "UserId" FROM "Users"
                WHERE "MemberId" IS NOT NULL ORDER BY "MemberId", "IsActive" DESC, "Id"
            ), appointer AS (
                SELECT MIN(ur."UsersId") AS "Id" FROM "UserRoles" ur JOIN "Roles" r ON r."Id" = ur."RolesId" WHERE r."Name" = 'SuperAdmin'
            )
            INSERT INTO "ElectionAppointments" ("ElectionId", "PersonaId", "UserId", "MemberId", "DisplayName", "Email", "Phone",
                "AppointedByUserId", "AppointedAt", "AcceptedAt", "DeclarationSignedAt", "DeclarationTextSnapshot")
            SELECT o."ElectionId", p."Id", mu."UserId", o."MemberId", LEFT(m."FullName", 150), LEFT(m."Email", 256), LEFT(m."MobileNo", 30),
                COALESCE((SELECT "Id" FROM appointer), mu."UserId"), now(), now(), now(), p."DeclarationText"
            FROM "ElectionOfficers" o
            JOIN "Members" m ON m."Id" = o."MemberId"
            JOIN member_user mu ON mu."MemberId" = o."MemberId"
            JOIN "ElectionPersonas" p ON p."Id" = (
                SELECT MIN(p2."Id") FROM "ElectionPersonas" p2
                WHERE p2."Name" = CASE o."Role" WHEN 0 THEN 'Returning Officer' WHEN 1 THEN 'Assistant Returning Officer'
                    WHEN 2 THEN 'Polling Officer' WHEN 3 THEN 'Scrutineer' END);

            SELECT setval(pg_get_serial_sequence('"Roles"', 'Id'), GREATEST((SELECT MAX("Id") FROM "Roles"), 1));
            INSERT INTO "Roles" ("Name")
            SELECT 'ElectionOfficial'
            WHERE EXISTS (SELECT 1 FROM "ElectionAppointments")
              AND NOT EXISTS (SELECT 1 FROM "Roles" WHERE "Name" = 'ElectionOfficial');
            INSERT INTO "UserRoles" ("RolesId", "UsersId")
            SELECT DISTINCT r."Id", a."UserId"
            FROM "ElectionAppointments" a JOIN "Roles" r ON r."Name" = 'ElectionOfficial'
            WHERE NOT EXISTS (SELECT 1 FROM "UserRoles" ur WHERE ur."RolesId" = r."Id" AND ur."UsersId" = a."UserId");

            UPDATE "ScrutinyDecisions" d SET "DecidedByUserId" = mu."UserId"
            FROM (SELECT DISTINCT ON ("MemberId") "MemberId", "Id" AS "UserId" FROM "Users"
                  WHERE "MemberId" IS NOT NULL ORDER BY "MemberId", "IsActive" DESC, "Id") mu
            WHERE mu."MemberId" = d."OfficerMemberId";

            DO $$ BEGIN
                IF EXISTS (SELECT 1 FROM "ScrutinyDecisions" WHERE "DecidedByUserId" IS NULL) THEN
                    RAISE EXCEPTION 'ElectionAppointments: a scrutiny decision was made by a member with no login. Link that member to a user, then rerun.';
                END IF;
            END $$;
            """;

        // Down refuses to run over a decision made by a non-member official, because the old column
        // can only hold a member id. The ElectionOfficial role and the personas stay in place.
        private const string CopyOfficersDownSql = """
            INSERT INTO "ElectionOfficers" ("ElectionId", "MemberId", "Role")
            SELECT DISTINCT a."ElectionId", u."MemberId",
                CASE p."Name" WHEN 'Returning Officer' THEN 0 WHEN 'Assistant Returning Officer' THEN 1
                    WHEN 'Polling Officer' THEN 2 WHEN 'Scrutineer' THEN 3 END
            FROM "ElectionAppointments" a
            JOIN "Users" u ON u."Id" = a."UserId"
            JOIN "ElectionPersonas" p ON p."Id" = a."PersonaId"
            WHERE u."MemberId" IS NOT NULL AND a."AcceptedAt" IS NOT NULL AND a."RevokedAt" IS NULL
              AND p."Name" IN ('Returning Officer', 'Assistant Returning Officer', 'Polling Officer', 'Scrutineer');

            UPDATE "ScrutinyDecisions" d SET "OfficerMemberId" = u."MemberId"
            FROM "Users" u WHERE u."Id" = d."DecidedByUserId";

            DO $$ BEGIN
                IF EXISTS (SELECT 1 FROM "ScrutinyDecisions" WHERE "OfficerMemberId" IS NULL) THEN
                    RAISE EXCEPTION 'ElectionAppointments Down: a scrutiny decision was made by a non-member official and cannot be kept.';
                END IF;
            END $$;
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElectionAppointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ElectionId = table.Column<int>(type: "integer", nullable: false),
                    PersonaId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MemberId = table.Column<int>(type: "integer", nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    AppointedByUserId = table.Column<int>(type: "integer", nullable: false),
                    AppointedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeclarationSignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeclarationTextSnapshot = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SignedFromIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedByUserId = table.Column<int>(type: "integer", nullable: true),
                    RevokedReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectionAppointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElectionAppointments_ElectionPersonas_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "ElectionPersonas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElectionAppointments_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ElectionAppointments_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElectionAppointments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ElectionAppointments_ElectionId_PersonaId_UserId",
                table: "ElectionAppointments",
                columns: new[] { "ElectionId", "PersonaId", "UserId" },
                unique: true,
                filter: "\"RevokedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionAppointments_MemberId",
                table: "ElectionAppointments",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionAppointments_PersonaId",
                table: "ElectionAppointments",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionAppointments_UserId",
                table: "ElectionAppointments",
                column: "UserId");

            migrationBuilder.AddColumn<int>(
                name: "DecidedByUserId",
                table: "ScrutinyDecisions",
                type: "integer",
                nullable: true);

            if (ActiveProvider == PostgresProvider)
                migrationBuilder.Sql(CopyOfficersUpSql);

            migrationBuilder.DropForeignKey(
                name: "FK_ScrutinyDecisions_Members_OfficerMemberId",
                table: "ScrutinyDecisions");

            migrationBuilder.DropIndex(
                name: "IX_ScrutinyDecisions_OfficerMemberId",
                table: "ScrutinyDecisions");

            migrationBuilder.DropColumn(
                name: "OfficerMemberId",
                table: "ScrutinyDecisions");

            migrationBuilder.AlterColumn<int>(
                name: "DecidedByUserId",
                table: "ScrutinyDecisions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScrutinyDecisions_DecidedByUserId",
                table: "ScrutinyDecisions",
                column: "DecidedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScrutinyDecisions_Users_DecidedByUserId",
                table: "ScrutinyDecisions",
                column: "DecidedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropTable(
                name: "ElectionOfficers");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElectionOfficers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ElectionId = table.Column<int>(type: "integer", nullable: false),
                    MemberId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectionOfficers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElectionOfficers_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ElectionOfficers_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ElectionOfficers_ElectionId_MemberId_Role",
                table: "ElectionOfficers",
                columns: new[] { "ElectionId", "MemberId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectionOfficers_MemberId",
                table: "ElectionOfficers",
                column: "MemberId");

            migrationBuilder.AddColumn<int>(
                name: "OfficerMemberId",
                table: "ScrutinyDecisions",
                type: "integer",
                nullable: true);

            // Rollback keeps only accepted, unrevoked appointments of members to the four personas the old
            // enum had. Appointments to any other persona, and every non-member appointee, are lost.
            if (ActiveProvider == PostgresProvider)
                migrationBuilder.Sql(CopyOfficersDownSql);

            migrationBuilder.DropForeignKey(
                name: "FK_ScrutinyDecisions_Users_DecidedByUserId",
                table: "ScrutinyDecisions");

            migrationBuilder.DropIndex(
                name: "IX_ScrutinyDecisions_DecidedByUserId",
                table: "ScrutinyDecisions");

            migrationBuilder.DropColumn(
                name: "DecidedByUserId",
                table: "ScrutinyDecisions");

            migrationBuilder.AlterColumn<int>(
                name: "OfficerMemberId",
                table: "ScrutinyDecisions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScrutinyDecisions_OfficerMemberId",
                table: "ScrutinyDecisions",
                column: "OfficerMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScrutinyDecisions_Members_OfficerMemberId",
                table: "ScrutinyDecisions",
                column: "OfficerMemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropTable(
                name: "ElectionAppointments");

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
        }
    }
}
