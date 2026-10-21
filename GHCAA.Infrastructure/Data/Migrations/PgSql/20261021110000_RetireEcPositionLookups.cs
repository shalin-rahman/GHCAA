using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class RetireEcPositionLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 95.2: the EcPositions table now holds the position names, and AddEcPositions already
            // copied any label renamed here. Nothing reads the ECPosition lookup group any more.
            migrationBuilder.Sql("""
                DELETE FROM "Lookups" WHERE "LookupGroup" = 'ECPosition' AND "Id" BETWEEN 4001 AND 4015;
                """);

            // Rows an admin repaired after AddEcPositions skipped a key get it now.
            AddEcPositions.AddPositionKeyUnlessOrphans(migrationBuilder, "ECMembers", "FK_ECMembers_EcPositions_Position");
            AddEcPositions.AddPositionKeyUnlessOrphans(migrationBuilder, "ElectionSeats", "FK_ElectionSeats_EcPositions_Position");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Puts the labels back from the current position names so a rename is not lost.
            // The keys stay; AddEcPositions owns them.
            migrationBuilder.Sql("""
                INSERT INTO "Lookups" ("Id", "LookupGroup", "Value", "Label", "IsActive", "DisplayOrder")
                SELECT 4000 + p."Id", 'ECPosition', p."Key", p."Name", TRUE, p."Id"
                FROM "EcPositions" p
                WHERE p."Id" BETWEEN 1 AND 15
                ON CONFLICT DO NOTHING;
                """);
        }
    }
}
