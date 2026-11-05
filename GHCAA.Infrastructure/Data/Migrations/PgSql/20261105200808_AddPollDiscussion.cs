using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddPollDiscussion : Migration
    {
        // 84.71: categories shared by polls and meetings (spec 028 FR-17). Poll.Category stores the
        // value, so the keys must not change once polls use them.
        internal static readonly (int Id, string Value, string Label)[] CategoryRows =
        {
            (5101, "Governance", "Governance"),
            (5102, "Finance", "Finance"),
            (5103, "Events", "Events"),
            (5104, "AlumniAffairs", "Alumni affairs"),
            (5105, "Other", "Other"),
            (5106, "Election", "Election"),
            (5107, "Membership", "Membership"),
            (5108, "Fundraising", "Fundraising"),
            (5109, "MentorshipCareers", "Mentorship and careers"),
            (5110, "ScholarshipWelfare", "Scholarship and welfare"),
            (5111, "Communication", "Communication"),
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A poll or meeting thread has no forum category, so the column becomes nullable.
            migrationBuilder.Sql("""
                ALTER TABLE "ForumTopics" ALTER COLUMN "CategoryId" DROP NOT NULL;
                ALTER TABLE "ForumTopics" ADD COLUMN IF NOT EXISTS "ContextType" character varying(20) NULL;
                ALTER TABLE "ForumTopics" ADD COLUMN IF NOT EXISTS "ContextId" integer NULL;
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_ForumTopics_ContextType_ContextId"
                    ON "ForumTopics" ("ContextType", "ContextId");

                ALTER TABLE "Polls" ADD COLUMN IF NOT EXISTS "AllowComments" boolean NOT NULL DEFAULT TRUE;
                ALTER TABLE "Polls" ADD COLUMN IF NOT EXISTS "Category" character varying(50) NOT NULL DEFAULT 'Other';
                ALTER TABLE "Polls" ADD COLUMN IF NOT EXISTS "AnnouncementId" integer NULL;
                CREATE INDEX IF NOT EXISTS "IX_Polls_Category" ON "Polls" ("Category");

                ALTER TABLE "NewsPosts" ADD COLUMN IF NOT EXISTS "MembersOnly" boolean NOT NULL DEFAULT FALSE;
                """);

            // Same pattern as SeedJobCategoryLookups: skip a key that already exists, then move the
            // identity sequence past the explicit Ids.
            var values = string.Join(",\n", CategoryRows.Select((r, i) =>
                $"({r.Id}, 'DiscussionCategory', '{r.Value}', '{r.Label}', TRUE, {i + 1})"));
            migrationBuilder.Sql($"""
                INSERT INTO "Lookups" ("Id", "LookupGroup", "Value", "Label", "IsActive", "DisplayOrder")
                VALUES
                {values}
                ON CONFLICT ("LookupGroup", "Value") DO NOTHING;
                """);
            migrationBuilder.Sql("""
                SELECT setval(pg_get_serial_sequence('"Lookups"', 'Id'),
                    GREATEST((SELECT MAX("Id") FROM "Lookups"), 1));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var rows = string.Join(", ", CategoryRows.Select(r => $"({r.Id}, '{r.Value}', '{r.Label}')"));
            migrationBuilder.Sql($"""
                DELETE FROM "Lookups"
                WHERE "LookupGroup" = 'DiscussionCategory' AND ("Id", "Value", "Label") IN ({rows});
                """);

            // Context threads have no category and cannot survive NOT NULL, so they go first.
            migrationBuilder.Sql("""
                DELETE FROM "ForumPosts" WHERE "TopicId" IN (SELECT "Id" FROM "ForumTopics" WHERE "ContextType" IS NOT NULL);
                DELETE FROM "ForumTopics" WHERE "ContextType" IS NOT NULL;
                DROP INDEX IF EXISTS "IX_ForumTopics_ContextType_ContextId";
                ALTER TABLE "ForumTopics" DROP COLUMN IF EXISTS "ContextType";
                ALTER TABLE "ForumTopics" DROP COLUMN IF EXISTS "ContextId";
                ALTER TABLE "ForumTopics" ALTER COLUMN "CategoryId" SET NOT NULL;

                DROP INDEX IF EXISTS "IX_Polls_Category";
                ALTER TABLE "Polls" DROP COLUMN IF EXISTS "AllowComments";
                ALTER TABLE "Polls" DROP COLUMN IF EXISTS "Category";
                ALTER TABLE "Polls" DROP COLUMN IF EXISTS "AnnouncementId";

                ALTER TABLE "NewsPosts" DROP COLUMN IF EXISTS "MembersOnly";
                """);
        }
    }
}
