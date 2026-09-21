using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddElectionEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Elections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ECPeriodId = table.Column<int>(type: "integer", nullable: false),
                    Phase = table.Column<int>(type: "integer", nullable: false),
                    AnnouncedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NominationOpensOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NominationClosesOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScrutinyOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WithdrawalClosesOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PollingOpensOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PollingClosesOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeclaredOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Elections_ECPeriods_ECPeriodId",
                        column: x => x.ECPeriodId,
                        principalTable: "ECPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ballots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ElectionId = table.Column<int>(type: "integer", nullable: false),
                    ElectionSeatId = table.Column<int>(type: "integer", nullable: false),
                    SerialNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "ElectionResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ElectionId = table.Column<int>(type: "integer", nullable: false),
                    ElectionSeatId = table.Column<int>(type: "integer", nullable: false),
                    NominationId = table.Column<int>(type: "integer", nullable: false),
                    VoteCount = table.Column<int>(type: "integer", nullable: false),
                    IsElected = table.Column<bool>(type: "boolean", nullable: false),
                    IsTie = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElectionResults_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ElectionSeats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ElectionId = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    SeatCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectionSeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElectionSeats_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoterRolls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ElectionId = table.Column<int>(type: "integer", nullable: false),
                    MemberId = table.Column<int>(type: "integer", nullable: false),
                    IsEligible = table.Column<bool>(type: "boolean", nullable: false),
                    IneligibilityReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FrozenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VotedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoterRolls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoterRolls_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VoterRolls_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Nominations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ElectionId = table.Column<int>(type: "integer", nullable: false),
                    ElectionSeatId = table.Column<int>(type: "integer", nullable: false),
                    CandidateMemberId = table.Column<int>(type: "integer", nullable: false),
                    ProposerMemberId = table.Column<int>(type: "integer", nullable: false),
                    SeconderMemberId = table.Column<int>(type: "integer", nullable: false),
                    Statement = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    PhotoPath = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WithdrawnAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nominations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nominations_ElectionSeats_ElectionSeatId",
                        column: x => x.ElectionSeatId,
                        principalTable: "ElectionSeats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nominations_Elections_ElectionId",
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

            migrationBuilder.CreateTable(
                name: "ScrutinyDecisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NominationId = table.Column<int>(type: "integer", nullable: false),
                    OfficerMemberId = table.Column<int>(type: "integer", nullable: false),
                    Accepted = table.Column<bool>(type: "boolean", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrutinyDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScrutinyDecisions_Members_OfficerMemberId",
                        column: x => x.OfficerMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScrutinyDecisions_Nominations_NominationId",
                        column: x => x.NominationId,
                        principalTable: "Nominations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 20, 18, 39, 42, 768, DateTimeKind.Utc).AddTicks(5837));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 20, 18, 39, 42, 768, DateTimeKind.Utc).AddTicks(5765));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 20, 18, 39, 42, 768, DateTimeKind.Utc).AddTicks(5700));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 20, 18, 39, 42, 768, DateTimeKind.Utc).AddTicks(5621));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 20, 18, 39, 42, 768, DateTimeKind.Utc).AddTicks(5516));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 20, 18, 39, 42, 768, DateTimeKind.Utc).AddTicks(5165));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 20, 18, 39, 42, 768, DateTimeKind.Utc).AddTicks(5034));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 20, 18, 39, 42, 767, DateTimeKind.Utc).AddTicks(6163));

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

            migrationBuilder.CreateIndex(
                name: "IX_ElectionOfficers_ElectionId_MemberId_Role",
                table: "ElectionOfficers",
                columns: new[] { "ElectionId", "MemberId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectionOfficers_MemberId",
                table: "ElectionOfficers",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionResults_ElectionId_ElectionSeatId_NominationId",
                table: "ElectionResults",
                columns: new[] { "ElectionId", "ElectionSeatId", "NominationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Elections_ECPeriodId_IsActive",
                table: "Elections",
                columns: new[] { "ECPeriodId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ElectionSeats_ElectionId_Position",
                table: "ElectionSeats",
                columns: new[] { "ElectionId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nominations_ElectionId",
                table: "Nominations",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Nominations_ElectionSeatId_CandidateMemberId",
                table: "Nominations",
                columns: new[] { "ElectionSeatId", "CandidateMemberId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScrutinyDecisions_NominationId",
                table: "ScrutinyDecisions",
                column: "NominationId");

            migrationBuilder.CreateIndex(
                name: "IX_ScrutinyDecisions_OfficerMemberId",
                table: "ScrutinyDecisions",
                column: "OfficerMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_VoterRolls_ElectionId_MemberId",
                table: "VoterRolls",
                columns: new[] { "ElectionId", "MemberId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoterRolls_MemberId",
                table: "VoterRolls",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BallotVotes");

            migrationBuilder.DropTable(
                name: "ElectionOfficers");

            migrationBuilder.DropTable(
                name: "ElectionResults");

            migrationBuilder.DropTable(
                name: "ScrutinyDecisions");

            migrationBuilder.DropTable(
                name: "VoterRolls");

            migrationBuilder.DropTable(
                name: "Ballots");

            migrationBuilder.DropTable(
                name: "Nominations");

            migrationBuilder.DropTable(
                name: "ElectionSeats");

            migrationBuilder.DropTable(
                name: "Elections");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 16, 13, 12, 40, 952, DateTimeKind.Utc).AddTicks(1297));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 16, 13, 12, 40, 952, DateTimeKind.Utc).AddTicks(1221));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 16, 13, 12, 40, 952, DateTimeKind.Utc).AddTicks(1146));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 16, 13, 12, 40, 952, DateTimeKind.Utc).AddTicks(1060));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 16, 13, 12, 40, 952, DateTimeKind.Utc).AddTicks(952));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 16, 13, 12, 40, 952, DateTimeKind.Utc).AddTicks(640));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 16, 13, 12, 40, 952, DateTimeKind.Utc).AddTicks(340));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 16, 13, 12, 40, 951, DateTimeKind.Utc).AddTicks(664));
        }
    }
}
