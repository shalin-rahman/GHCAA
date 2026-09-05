using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <summary>
    /// Work Package 37.3. Three new tables for fundraising campaigns: Campaigns, CampaignPledges
    /// (MemberId nullable — a guest donor has no Member row) and DonorRecognitionTiers. No seed
    /// data (a new campaign is authored by an admin, not shipped with the app).
    ///
    /// Hand-written rather than scaffolded: `dotnet ef migrations add` also emitted an UpdateData
    /// against unrelated Users.SecurityStamp/EmailTemplates.LastUpdated rows (see
    /// gotcha_pending_model_changes_seed) — none of which belongs in this migration.
    /// </summary>
    public partial class AddFundraisingCampaigns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Story = table.Column<string>(type: "text", nullable: false),
                    CoverImagePath = table.Column<string>(type: "text", nullable: true),
                    TargetAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    StartsOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DonorRecognitionTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonorRecognitionTiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignPledges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<int>(type: "integer", nullable: false),
                    MemberId = table.Column<int>(type: "integer", nullable: true),
                    DonorName = table.Column<string>(type: "text", nullable: false),
                    DonorEmail = table.Column<string>(type: "text", nullable: true),
                    DonorPhone = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    AmountReceived = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsAnonymous = table.Column<bool>(type: "boolean", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    PledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinancialRecordId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignPledges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignPledges_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignPledges_FinancialRecords_FinancialRecordId",
                        column: x => x.FinancialRecordId,
                        principalTable: "FinancialRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CampaignPledges_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPledges_CampaignId",
                table: "CampaignPledges",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPledges_FinancialRecordId",
                table: "CampaignPledges",
                column: "FinancialRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignPledges_MemberId",
                table: "CampaignPledges",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Slug",
                table: "Campaigns",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignPledges");

            migrationBuilder.DropTable(
                name: "DonorRecognitionTiers");

            migrationBuilder.DropTable(
                name: "Campaigns");
        }
    }
}
