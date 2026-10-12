using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddBallotOrderAndTestElection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Spec 023 FR-046 (37.13b). Null on nominations from before this column; the ballot then sorts them by name.
            migrationBuilder.AddColumn<int>(
                name: "BallotOrder",
                table: "Nominations",
                type: "integer",
                nullable: true);

            // Spec 023 FR-047 (37.13d).
            migrationBuilder.AddColumn<bool>(
                name: "IsTest",
                table: "Elections",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // 37.1t. One live election per EC period. 9 is ElectionPhase.Archived. A rehearsal
            // (IsTest) is left out so it cannot block the real election. A database where two
            // live elections already share a period cannot take the index, so it is skipped with
            // a warning rather than stopping start-up. CreateAsync still refuses a new one.
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Elections_ECPeriodId_Live";""");
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "Elections" WHERE "Phase" <> 9 AND NOT "IsTest"
                               GROUP BY "ECPeriodId" HAVING COUNT(*) > 1) THEN
                        RAISE WARNING 'IX_Elections_ECPeriodId_Live not created: two live elections share an EC period.';
                    ELSE
                        CREATE UNIQUE INDEX IF NOT EXISTS "IX_Elections_ECPeriodId_Live"
                            ON "Elections" ("ECPeriodId") WHERE "Phase" <> 9 AND NOT "IsTest";
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Elections_ECPeriodId_Live";""");

            migrationBuilder.DropColumn(
                name: "BallotOrder",
                table: "Nominations");

            migrationBuilder.DropColumn(
                name: "IsTest",
                table: "Elections");
        }
    }
}
