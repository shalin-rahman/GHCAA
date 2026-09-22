using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddScholarshipProgramme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScholarshipFunds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    NamedAfter = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TargetAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScholarshipFunds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScholarshipCalls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScholarshipFundId = table.Column<int>(type: "integer", nullable: false),
                    AcademicYear = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OpensOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosesOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SlotCount = table.Column<int>(type: "integer", nullable: false),
                    AwardAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    EligibilityCriteria = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScholarshipCalls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScholarshipCalls_ScholarshipFunds_ScholarshipFundId",
                        column: x => x.ScholarshipFundId,
                        principalTable: "ScholarshipFunds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScholarshipApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScholarshipCallId = table.Column<int>(type: "integer", nullable: false),
                    ApplicantName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ApplicantEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    ApplicantPhone = table.Column<string>(type: "text", nullable: false),
                    InstitutionName = table.Column<string>(type: "text", nullable: false),
                    Class = table.Column<string>(type: "text", nullable: false),
                    GuardianName = table.Column<string>(type: "text", nullable: false),
                    HouseholdIncome = table.Column<decimal>(type: "numeric", nullable: false),
                    NeedStatement = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    MeritStatement = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReferenceCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScholarshipApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScholarshipApplications_ScholarshipCalls_ScholarshipCallId",
                        column: x => x.ScholarshipCallId,
                        principalTable: "ScholarshipCalls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScholarshipAwards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScholarshipApplicationId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    AwardedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DisbursementStatus = table.Column<int>(type: "integer", nullable: false),
                    FinancialRecordId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScholarshipAwards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScholarshipAwards_FinancialRecords_FinancialRecordId",
                        column: x => x.FinancialRecordId,
                        principalTable: "FinancialRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScholarshipAwards_ScholarshipApplications_ScholarshipApplic~",
                        column: x => x.ScholarshipApplicationId,
                        principalTable: "ScholarshipApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScholarshipDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScholarshipApplicationId = table.Column<int>(type: "integer", nullable: false),
                    FileUploadId = table.Column<int>(type: "integer", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScholarshipDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScholarshipDocuments_FileUploads_FileUploadId",
                        column: x => x.FileUploadId,
                        principalTable: "FileUploads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScholarshipDocuments_ScholarshipApplications_ScholarshipApp~",
                        column: x => x.ScholarshipApplicationId,
                        principalTable: "ScholarshipApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScholarshipReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScholarshipApplicationId = table.Column<int>(type: "integer", nullable: false),
                    ReviewerMemberId = table.Column<int>(type: "integer", nullable: false),
                    NeedScore = table.Column<decimal>(type: "numeric", nullable: false),
                    MeritScore = table.Column<decimal>(type: "numeric", nullable: false),
                    Comments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScholarshipReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScholarshipReviews_Members_ReviewerMemberId",
                        column: x => x.ReviewerMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScholarshipReviews_ScholarshipApplications_ScholarshipAppli~",
                        column: x => x.ScholarshipApplicationId,
                        principalTable: "ScholarshipApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_ScholarshipApplications_ReferenceCode",
                table: "ScholarshipApplications",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScholarshipApplications_ScholarshipCallId",
                table: "ScholarshipApplications",
                column: "ScholarshipCallId");

            migrationBuilder.CreateIndex(
                name: "IX_ScholarshipAwards_FinancialRecordId",
                table: "ScholarshipAwards",
                column: "FinancialRecordId",
                unique: true,
                filter: "\"FinancialRecordId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ScholarshipAwards_ScholarshipApplicationId",
                table: "ScholarshipAwards",
                column: "ScholarshipApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScholarshipCalls_ScholarshipFundId",
                table: "ScholarshipCalls",
                column: "ScholarshipFundId");

            migrationBuilder.CreateIndex(
                name: "IX_ScholarshipDocuments_FileUploadId",
                table: "ScholarshipDocuments",
                column: "FileUploadId");

            migrationBuilder.CreateIndex(
                name: "IX_ScholarshipDocuments_ScholarshipApplicationId_FileUploadId",
                table: "ScholarshipDocuments",
                columns: new[] { "ScholarshipApplicationId", "FileUploadId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScholarshipReviews_ReviewerMemberId",
                table: "ScholarshipReviews",
                column: "ReviewerMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ScholarshipReviews_ScholarshipApplicationId_ReviewerMemberId",
                table: "ScholarshipReviews",
                columns: new[] { "ScholarshipApplicationId", "ReviewerMemberId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScholarshipAwards");

            migrationBuilder.DropTable(
                name: "ScholarshipDocuments");

            migrationBuilder.DropTable(
                name: "ScholarshipReviews");

            migrationBuilder.DropTable(
                name: "ScholarshipApplications");

            migrationBuilder.DropTable(
                name: "ScholarshipCalls");

            migrationBuilder.DropTable(
                name: "ScholarshipFunds");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -8,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 21, 3, 22, 41, 705, DateTimeKind.Utc).AddTicks(2754));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -7,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 21, 3, 22, 41, 705, DateTimeKind.Utc).AddTicks(2692));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -6,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 21, 3, 22, 41, 705, DateTimeKind.Utc).AddTicks(2638));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -5,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 21, 3, 22, 41, 705, DateTimeKind.Utc).AddTicks(2572));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -4,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 21, 3, 22, 41, 705, DateTimeKind.Utc).AddTicks(2483));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -3,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 21, 3, 22, 41, 705, DateTimeKind.Utc).AddTicks(2065));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -2,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 21, 3, 22, 41, 705, DateTimeKind.Utc).AddTicks(1924));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: -1,
                column: "LastUpdated",
                value: new DateTime(2026, 9, 21, 3, 22, 41, 704, DateTimeKind.Utc).AddTicks(4955));
        }
    }
}
