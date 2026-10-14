using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddEventPhotoThumbnail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 98.3: raw SQL with IF NOT EXISTS, because a database built by EnsureCreated already
            // has the column and the migration must still apply there.
            migrationBuilder.Sql("""ALTER TABLE "EventPhotos" ADD COLUMN IF NOT EXISTS "ThumbnailPath" text;""");

            // Thumbnail paths for rows that already exist are filled at boot by
            // InstitutionDataSeeder.BackfillPhotoThumbnailsAsync from the active profile, so no
            // profile's photo paths are written here.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""ALTER TABLE "EventPhotos" DROP COLUMN IF EXISTS "ThumbnailPath";""");
        }
    }
}
