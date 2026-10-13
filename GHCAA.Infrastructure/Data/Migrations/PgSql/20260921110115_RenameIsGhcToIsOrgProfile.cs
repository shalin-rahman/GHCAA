using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    // This migration shipped without its Designer file, so it had no [Migration] attribute and EF
    // never ran it. RepairMissingColumnsAsync then added IsOrgProfile as an empty column beside
    // IsGHC, no record was flagged as the org record, and the directory showed "Batch: N/A".
    // The SQL below handles both shapes: IsGHC alone is renamed, and IsGHC next to the added
    // column is copied over and dropped. A database without IsGHC is left alone.
    /// <inheritdoc />
    [DbContext(typeof(PgSqlApplicationDbContext))]
    [Migration("20260921110115_RenameIsGhcToIsOrgProfile")]
    public partial class RenameIsGhcToIsOrgProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.columns
                               WHERE table_schema = current_schema() AND table_name = 'AcademicRecords' AND column_name = 'IsGHC') THEN
                        IF EXISTS (SELECT 1 FROM information_schema.columns
                                   WHERE table_schema = current_schema() AND table_name = 'AcademicRecords' AND column_name = 'IsOrgProfile') THEN
                            UPDATE "AcademicRecords" SET "IsOrgProfile" = TRUE WHERE "IsGHC" AND NOT "IsOrgProfile";
                            ALTER TABLE "AcademicRecords" DROP COLUMN "IsGHC";
                        ELSE
                            ALTER TABLE "AcademicRecords" RENAME COLUMN "IsGHC" TO "IsOrgProfile";
                        END IF;
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsOrgProfile",
                table: "AcademicRecords",
                newName: "IsGHC");
        }
    }
}
