using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddConfigUniqueKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 62.56g. Unique keys the config pack import matches rows on. A database that already
            // holds two rows with the same key cannot take that index, so it is skipped with a
            // warning naming the Ids rather than stopping start-up, which would also lock the admin
            // out of the page that removes the extra row. The service checks still refuse new
            // duplicates. IF NOT EXISTS covers a database built by EnsureCreated.
            CreateUniqueIndexUnlessDuplicates(migrationBuilder,
                "SpecialDayThemes", "IX_SpecialDayThemes_Title_StartDate",
                "\"Title\", \"StartDate\"");
            CreateUniqueIndexUnlessDuplicates(migrationBuilder,
                "MembershipFeeConfigs", "IX_MembershipFeeConfigs_Category_MembershipType_EffectiveDate",
                "\"Category\", \"MembershipType\", \"EffectiveDate\"");
            // Method alone is not unique: Credit Card has one row per gateway.
            CreateUniqueIndexUnlessDuplicates(migrationBuilder,
                "PaymentConfigurations", "IX_PaymentConfigurations_Method_Gateway",
                "\"Method\", \"Gateway\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_PaymentConfigurations_Method_Gateway";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_MembershipFeeConfigs_Category_MembershipType_EffectiveDate";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_SpecialDayThemes_Title_StartDate";""");
        }

        private static void CreateUniqueIndexUnlessDuplicates(
            MigrationBuilder migrationBuilder, string table, string index, string columns)
        {
            migrationBuilder.Sql($$"""
                DO $$
                DECLARE dup text;
                BEGIN
                    SELECT string_agg(ids, '; ') INTO dup FROM (
                        SELECT string_agg("Id"::text, ',' ORDER BY "Id") AS ids
                        FROM "{{table}}" GROUP BY {{columns}} HAVING COUNT(*) > 1) d;
                    IF dup IS NOT NULL THEN
                        RAISE WARNING '{{index}} not created: "{{table}}" has rows with the same key (Ids %). Remove the extra rows; the index then needs a manual CREATE UNIQUE INDEX.', dup;
                    ELSE
                        CREATE UNIQUE INDEX IF NOT EXISTS "{{index}}" ON "{{table}}" ({{columns}});
                    END IF;
                END $$;
                """);
        }
    }
}
