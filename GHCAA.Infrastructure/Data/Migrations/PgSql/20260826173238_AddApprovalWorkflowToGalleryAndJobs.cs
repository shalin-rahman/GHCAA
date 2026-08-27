using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddApprovalWorkflowToGalleryAndJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "JobOpportunities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "JobOpportunities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "EventPhotos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "EventPhotos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UploadedByMemberId",
                table: "EventPhotos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerMemberId",
                table: "EventGalleries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "EventGalleries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "EventGalleries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresRegistration",
                table: "AlumniEvents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AlumniEvents",
                keyColumn: "Id",
                keyValue: 1,
                column: "RequiresRegistration",
                value: true);

            migrationBuilder.UpdateData(
                table: "AlumniEvents",
                keyColumn: "Id",
                keyValue: 2,
                column: "RequiresRegistration",
                value: true);

            migrationBuilder.UpdateData(
                table: "AlumniEvents",
                keyColumn: "Id",
                keyValue: 3,
                column: "RequiresRegistration",
                value: true);

            migrationBuilder.UpdateData(
                table: "AlumniEvents",
                keyColumn: "Id",
                keyValue: 4,
                column: "RequiresRegistration",
                value: true);

            // Guard against a duplicate-key rollback aborting this whole migration (and, with it,
            // the genuinely new AddColumn/CreateIndex/AddForeignKey operations above) if Id=6 or
            // this Key was ever left behind by an earlier partial application attempt.
            migrationBuilder.Sql(
                "DELETE FROM \"SiteContents\" WHERE \"Id\" = 6 OR \"Key\" = 'about-college-today';");

            migrationBuilder.InsertData(
                table: "SiteContents",
                columns: new[] { "Id", "BodyHtml", "DisplayOrder", "Group", "IsActive", "Key", "LastModified", "Title", "UpdatedByAdminId" },
                values: new object[] { 6, "<p>Govt. Haraganga College is a public institution in Munshiganj Sadar teaching at both higher-secondary and degree level, with <strong>16 honours departments</strong> and a faculty of more than <strong>80 teachers</strong>. Beyond the classroom, students run a range of clubs covering culture, sport, debate and community service.</p><p>In its 87 years the college has educated over <strong>one hundred thousand students</strong>, who have gone on into public service, medicine, engineering, teaching, business, journalism and the arts, at home and abroad. It is that shared history &mdash; not a shared graduating year &mdash; that this Association exists to keep alive.</p><ul><li><strong>Campus</strong> &mdash; College Road, Munshiganj Sadar, Munshiganj-1500, Bangladesh</li><li><strong>EIIN</strong> &mdash; 111160 &nbsp;&middot;&nbsp; <strong>College Code</strong> &mdash; 5701</li><li><strong>Principal</strong> &mdash; Professor Md. Kamaruzzaman Khan</li><li><strong>College website</strong> &mdash; <a href=\"https://www.haragangacollege.edu.bd/en\" target=\"_blank\" rel=\"noopener noreferrer\">haragangacollege.edu.bd</a></li></ul><p><em>College facts are drawn from the institution's official website. For admissions, results, routines and academic notices, please refer to the college directly &mdash; this Association is an independent alumni body and does not administer college affairs.</em></p>", 2, "about", true, "about-college-today", null, "The College Today", null });

            migrationBuilder.CreateIndex(
                name: "IX_EventGalleries_OwnerMemberId",
                table: "EventGalleries",
                column: "OwnerMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventGalleries_Members_OwnerMemberId",
                table: "EventGalleries",
                column: "OwnerMemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventGalleries_Members_OwnerMemberId",
                table: "EventGalleries");

            migrationBuilder.DropIndex(
                name: "IX_EventGalleries_OwnerMemberId",
                table: "EventGalleries");

            migrationBuilder.DeleteData(
                table: "SiteContents",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "JobOpportunities");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "JobOpportunities");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "EventPhotos");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EventPhotos");

            migrationBuilder.DropColumn(
                name: "UploadedByMemberId",
                table: "EventPhotos");

            migrationBuilder.DropColumn(
                name: "OwnerMemberId",
                table: "EventGalleries");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "EventGalleries");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EventGalleries");

            migrationBuilder.DropColumn(
                name: "RequiresRegistration",
                table: "AlumniEvents");

        }
    }
}
