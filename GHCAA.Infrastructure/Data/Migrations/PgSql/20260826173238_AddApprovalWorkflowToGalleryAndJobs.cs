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
            migrationBuilder.Sql(@"ALTER TABLE ""JobOpportunities"" ADD COLUMN IF NOT EXISTS ""RejectionReason"" text;");

            migrationBuilder.Sql(@"ALTER TABLE ""JobOpportunities"" ADD COLUMN IF NOT EXISTS ""Status"" integer NOT NULL DEFAULT 0;");

            migrationBuilder.Sql(@"ALTER TABLE ""EventPhotos"" ADD COLUMN IF NOT EXISTS ""RejectionReason"" text;");

            migrationBuilder.Sql(@"ALTER TABLE ""EventPhotos"" ADD COLUMN IF NOT EXISTS ""Status"" integer NOT NULL DEFAULT 0;");

            migrationBuilder.Sql(@"ALTER TABLE ""EventPhotos"" ADD COLUMN IF NOT EXISTS ""UploadedByMemberId"" integer;");

            migrationBuilder.Sql(@"ALTER TABLE ""EventGalleries"" ADD COLUMN IF NOT EXISTS ""OwnerMemberId"" integer;");

            migrationBuilder.Sql(@"ALTER TABLE ""EventGalleries"" ADD COLUMN IF NOT EXISTS ""RejectionReason"" text;");

            migrationBuilder.Sql(@"ALTER TABLE ""EventGalleries"" ADD COLUMN IF NOT EXISTS ""Status"" integer NOT NULL DEFAULT 0;");

            migrationBuilder.Sql(@"ALTER TABLE ""AlumniEvents"" ADD COLUMN IF NOT EXISTS ""RequiresRegistration"" boolean NOT NULL DEFAULT FALSE;");

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

            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_EventGalleries_OwnerMemberId"" ON ""EventGalleries"" (""OwnerMemberId"");");

            migrationBuilder.Sql(@"DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'FK_EventGalleries_Members_OwnerMemberId'
                ) THEN
                    ALTER TABLE ""EventGalleries"" ADD CONSTRAINT ""FK_EventGalleries_Members_OwnerMemberId""
                        FOREIGN KEY (""OwnerMemberId"") REFERENCES ""Members"" (""Id"") ON DELETE SET NULL;
                END IF;
            END $$;");
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

            migrationBuilder.Sql(@"ALTER TABLE ""JobOpportunities"" DROP COLUMN IF EXISTS ""RejectionReason"";");

            migrationBuilder.Sql(@"ALTER TABLE ""JobOpportunities"" DROP COLUMN IF EXISTS ""Status"";");

            migrationBuilder.Sql(@"ALTER TABLE ""EventPhotos"" DROP COLUMN IF EXISTS ""RejectionReason"";");

            migrationBuilder.Sql(@"ALTER TABLE ""EventPhotos"" DROP COLUMN IF EXISTS ""Status"";");

            migrationBuilder.Sql(@"ALTER TABLE ""EventPhotos"" DROP COLUMN IF EXISTS ""UploadedByMemberId"";");

            migrationBuilder.Sql(@"ALTER TABLE ""EventGalleries"" DROP COLUMN IF EXISTS ""OwnerMemberId"";");

            migrationBuilder.Sql(@"ALTER TABLE ""EventGalleries"" DROP COLUMN IF EXISTS ""RejectionReason"";");

            migrationBuilder.Sql(@"ALTER TABLE ""EventGalleries"" DROP COLUMN IF EXISTS ""Status"";");

            migrationBuilder.Sql(@"ALTER TABLE ""AlumniEvents"" DROP COLUMN IF EXISTS ""RequiresRegistration"";");

        }
    }
}
