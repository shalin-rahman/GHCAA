using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddSiteContentAndNoticeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentFileName",
                table: "NewsPosts",
                type: "character varying(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "NewsPosts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PostType",
                table: "NewsPosts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SiteContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Group = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BodyHtml = table.Column<string>(type: "text", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByAdminId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteContents", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SiteContents",
                columns: new[] { "Id", "BodyHtml", "DisplayOrder", "Group", "IsActive", "Key", "LastModified", "Title", "UpdatedByAdminId" },
                values: new object[,]
                {
                    { 1, "<p>Govt. Haraganga College, Munshiganj, has stood as a centre of learning since 1938, founded through the initiative of Ashutosh Ganguly with the support of Sher-e-Bangla A. K. Fazlul Huq. Generations of students have passed through its halls and carried its name into public service, business, academia and the arts.</p><p>The Govt. Haraganga College Alumni Association exists to keep that lineage connected.</p>", 1, "about", true, "about-origin", null, "Our Origin", null },
                    { 2, "<p>The organisation is named the <strong>Govt. Haraganga College Alumni Association (GHCAA)</strong>. Its members are known as <strong>HARAGANGIAN</strong>. The Association was founded on <strong>29 November 2025</strong>.</p><p>The Association is non-political, non-religious, inclusive and non-profit. It is a patron of education and culture and philanthropic in purpose. It does not affiliate with any political party or religious body, and welcomes all alumni without distinction.</p><p><strong>Motto:</strong> Sharing Heritage, Aligning Lives, Integrating Networks<br/><em>&#2405;&#2469;&#2495;&#2489;&#2509;&#2479;&#2503;&#2480; &#2476;&#2495;&#2472;&#2495;&#2478;&#2479;&#2492;, &#2460;&#2496;&#2476;&#2472;&#2503;&#2480; &#2488;&#2478;&#2472;&#2509;&#2476;&#2479;&#2492; &#2451; &#2488;&#2434;&#2489;&#2468;&#2495;&#2480; &#2488;&#2503;&#2468;&#2497;&#2476;&#2472;&#2509;&#2471;&#2472;</em></p>", 2, "about", true, "about-association", null, "The Association", null },
                    { 3, "<p>Every element of the Association's logo carries meaning:</p><ul><li><strong>Historic building</strong> &ndash; the college and its heritage</li><li><strong>Open book</strong> &ndash; knowledge and learning</li><li><strong>Torch</strong> &ndash; enlightenment and guidance</li><li><strong>Mortarboard</strong> &ndash; academic achievement</li><li><strong>Handshake</strong> &ndash; fellowship and mutual support among alumni</li><li><strong>Red and gold border, &ldquo;Est. 2025&rdquo;</strong> &ndash; the founding of the Association</li></ul><p>The Association's flag is white, signifying peace, harmony, non-violence and purity.</p><p>Use of the Association's name, logo, flag or other branding requires prior permission from the Executive Committee.</p>", 3, "about", true, "about-logo", null, "Logo &amp; Flag", null },
                    { 4, "<ul><li>To build and maintain a lasting bond among the alumni of Govt. Haraganga College.</li><li>To preserve and promote the heritage, history and traditions of the College.</li><li>To support the academic development of current students of the College.</li><li>To provide mentorship, career guidance and networking opportunities to students and alumni.</li><li>To assist alumni and students facing financial hardship through scholarships and welfare support.</li><li>To organise educational, cultural, social and sporting activities.</li><li>To encourage philanthropic and community service initiatives.</li><li>To strengthen cooperation between the Association, the College administration and its teachers.</li><li>To maintain an accurate register of alumni and facilitate communication among them.</li><li>To uphold transparency, accountability and good governance within the Association.</li><li>To undertake any other lawful activity consistent with these objectives.</li></ul>", 4, "about", true, "about-objectives", null, "Purpose &amp; Objectives", null },
                    { 5, "<p>The Association's secretariat operates from the Govt. Haraganga College campus in Munshiganj. Reach us by email, phone, or the form below and a member of the Executive Committee will respond.</p>", 1, "contact", true, "contact-intro", null, "Get in Touch", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiteContents_Key",
                table: "SiteContents",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteContents");

            migrationBuilder.DropColumn(
                name: "AttachmentFileName",
                table: "NewsPosts");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "NewsPosts");

            migrationBuilder.DropColumn(
                name: "PostType",
                table: "NewsPosts");
        }
    }
}
