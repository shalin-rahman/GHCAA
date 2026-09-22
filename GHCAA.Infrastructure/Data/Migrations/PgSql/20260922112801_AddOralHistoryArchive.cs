using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddOralHistoryArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArchiveCollections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Decade = table.Column<int>(type: "integer", nullable: true),
                    PublicationState = table.Column<int>(type: "integer", nullable: false),
                    ModerationState = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByMemberId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveCollections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArchiveCollectionId = table.Column<int>(type: "integer", nullable: false),
                    Narrator = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Transcript = table.Column<string>(type: "text", nullable: true),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Decade = table.Column<int>(type: "integer", nullable: true),
                    LinkedMemberId = table.Column<int>(type: "integer", nullable: true),
                    FileUploadId = table.Column<int>(type: "integer", nullable: true),
                    MediaUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PublicationState = table.Column<int>(type: "integer", nullable: false),
                    ModerationState = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByMemberId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchiveItems_ArchiveCollections_ArchiveCollectionId",
                        column: x => x.ArchiveCollectionId,
                        principalTable: "ArchiveCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArchiveItems_FileUploads_FileUploadId",
                        column: x => x.FileUploadId,
                        principalTable: "FileUploads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ArchiveItems_Members_LinkedMemberId",
                        column: x => x.LinkedMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 22, 11, 28, 0, 160, DateTimeKind.Utc).AddTicks(2464));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 22, 11, 28, 0, 160, DateTimeKind.Utc).AddTicks(2400));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 22, 11, 28, 0, 160, DateTimeKind.Utc).AddTicks(2343));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 22, 11, 28, 0, 160, DateTimeKind.Utc).AddTicks(2277));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 22, 11, 28, 0, 160, DateTimeKind.Utc).AddTicks(2186));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 22, 11, 28, 0, 160, DateTimeKind.Utc).AddTicks(1951));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 22, 11, 28, 0, 160, DateTimeKind.Utc).AddTicks(1805));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 22, 11, 28, 0, 159, DateTimeKind.Utc).AddTicks(2941));

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveItems_ArchiveCollectionId",
                table: "ArchiveItems",
                column: "ArchiveCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveItems_FileUploadId",
                table: "ArchiveItems",
                column: "FileUploadId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveItems_LinkedMemberId",
                table: "ArchiveItems",
                column: "LinkedMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveItems_PublicationState_ModerationState",
                table: "ArchiveItems",
                columns: new[] { "PublicationState", "ModerationState" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveItems_Transcript",
                table: "ArchiveItems",
                column: "Transcript");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchiveItems");

            migrationBuilder.DropTable(
                name: "ArchiveCollections");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 22, 11, 4, 56, 225, DateTimeKind.Utc).AddTicks(6771));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 22, 11, 4, 56, 225, DateTimeKind.Utc).AddTicks(6709));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 22, 11, 4, 56, 225, DateTimeKind.Utc).AddTicks(6655));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 22, 11, 4, 56, 225, DateTimeKind.Utc).AddTicks(6594));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 22, 11, 4, 56, 225, DateTimeKind.Utc).AddTicks(6511));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 22, 11, 4, 56, 225, DateTimeKind.Utc).AddTicks(6326));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 22, 11, 4, 56, 225, DateTimeKind.Utc).AddTicks(6206));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 22, 11, 4, 56, 224, DateTimeKind.Utc).AddTicks(9547));
        }
    }
}
