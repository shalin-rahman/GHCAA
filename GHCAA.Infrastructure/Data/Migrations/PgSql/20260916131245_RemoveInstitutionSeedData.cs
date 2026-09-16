using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class RemoveInstitutionSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Members, Users, UserRoles and the other Tier 3 tables (docs/SEED_CLASSIFICATION.md)
            // are no longer declared via HasData, so EF scaffolded this migration with a DeleteData
            // call for every row still in the model snapshot. Those rows are GHC's real, live data,
            // not throwaway demo data, so this migration must not touch them. Its only job is to
            // stop the model from re-asserting this data next time a migration is scaffolded — the
            // rows themselves are now seeded at runtime by InstitutionDataSeeder instead. Left as a
            // no-op on purpose.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Mirrors Up() — no-op for the same reason. Reverting this migration must not re-insert
            // Tier 3 rows into a live database; InstitutionDataSeeder owns that data now.
        }
    }
}
