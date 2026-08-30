using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class FixAssociationContentMergeLogoFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The "Logo & Flag" About-page block duplicated the same logo.png image for both
            // "emblem" and "flag" (there's no separate flag asset), so it read as two near-
            // identical boxes. Per instruction, its crest/flag description is folded into "The
            // Association" block instead and the standalone block is dropped. Matched by Key
            // (not Id) and safe to re-run: the UPDATE is a no-op once applied, and the DELETE is
            // a no-op if the row is already gone.
            //
            // Same statement also fixes the Bengali motto: the numeric character references for
            // the first two glyphs were wrong (&#2405;&#2469; decoded to "॥থ" instead of "ঐত"),
            // so the tagline rendered as "॥থিহ্যের বিনিময়..." instead of "ঐতিহ্যের বিনিময়...".
            // Hand-written raw SQL rather than scaffolded UpdateData/DeleteData: `dotnet ef
            // migrations add` here always also emits non-deterministic churn on unrelated
            // Users.SecurityStamp/PasswordHash and EmailTemplates.LastUpdated rows (see
            // gotcha_pending_model_changes_seed) — none of which belongs in this migration.
            migrationBuilder.Sql(@"
UPDATE ""SiteContents"" SET ""BodyHtml"" =
'<p>The <strong>Govt. Haraganga College Alumni Association (GHCAA)</strong> was founded on <strong>29 November 2025</strong> to give the college''s graduates a single place to find one another. Its members are known as <strong>HARAGANGIAN</strong>.</p><p>The Association is non-political, non-religious and not-for-profit. It takes no side in party politics and aligns with no religious body; membership is open to every former student of the college without distinction. Its work is philanthropic, and directed at education and culture.</p><p><strong>Motto:</strong> Sharing Heritage, Aligning Lives, Integrating Networks<br/><em>ঐতিহ্যের বিনিময়, জীবনের সমন্বয় ও সংহতির সেতুবন্ধন</em></p><p>Every element of the Association''s crest carries meaning: the historic building for the college and its heritage, the open book for knowledge and learning, the torch for enlightenment and guidance, the mortarboard for academic achievement, the handshake for fellowship and mutual support among alumni, and the red and gold border with &ldquo;Est. 2025&rdquo; for the founding of the Association. The Association''s flag is white, signifying peace, harmony, non-violence and purity.</p><p>Use of the Association''s name, crest, flag or other branding requires prior permission from the Executive Committee.</p>'
WHERE ""Key"" = 'about-association';");

            migrationBuilder.Sql(@"DELETE FROM ""SiteContents"" WHERE ""Key"" = 'about-logo';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Content rollback isn't meaningful here (the merge is a one-way editorial change,
            // and the deleted row's Id would collide with IDENTITY on re-insert); admins can
            // re-split the content via Admin -> Site Content if ever needed.
        }
    }
}
