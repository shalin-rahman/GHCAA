using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class SeedJobCategoryLookups : Migration
    {
        // 84.77: the job hub categories move from the client constants into lookup rows so an admin
        // can edit them. The keys are the ones existing job posts already store, so they must not change.
        internal static readonly (int Id, string Value, string Label)[] Rows =
        {
            (5001, "IT", "IT & Software Development"),
            (5002, "Finance", "Finance & Banking"),
            (5003, "Engineering", "Engineering & Construction"),
            (5004, "Marketing", "Marketing & Sales"),
            (5005, "Education", "Education & Research"),
            (5006, "Health", "Healthcare & Pharma"),
            (5007, "PublicSector", "Govt. & Public Sector"),
            (5008, "Mentorship", "Mentorship & Career Guidance"),
            (5009, "Other", "Other Opportunities"),
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Raw SQL rather than InsertData so a second run, or a key an admin already added by
            // hand, is skipped. Only that index is the target, so an unrelated row already using
            // one of these Ids fails the migration instead of silently dropping a category.
            var values = string.Join(",\n", Rows.Select((r, i) =>
                $"({r.Id}, 'JobCategory', '{r.Value}', '{r.Label}', TRUE, {i + 1})"));
            migrationBuilder.Sql($"""
                INSERT INTO "Lookups" ("Id", "LookupGroup", "Value", "Label", "IsActive", "DisplayOrder")
                VALUES
                {values}
                ON CONFLICT ("LookupGroup", "Value") DO NOTHING;
                """);

            // The rows carry explicit Ids, so the identity sequence has to move past them or the
            // next admin-created lookup collides with 5001.
            migrationBuilder.Sql("""
                SELECT setval(pg_get_serial_sequence('"Lookups"', 'Id'),
                    GREATEST((SELECT MAX("Id") FROM "Lookups"), 1));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Only rows that still match what Up inserted. A row an admin added or relabelled stays.
            var rows = string.Join(", ", Rows.Select(r => $"({r.Id}, '{r.Value}', '{r.Label}')"));
            migrationBuilder.Sql($"""
                DELETE FROM "Lookups"
                WHERE "LookupGroup" = 'JobCategory' AND ("Id", "Value", "Label") IN ({rows});
                """);
        }
    }
}
