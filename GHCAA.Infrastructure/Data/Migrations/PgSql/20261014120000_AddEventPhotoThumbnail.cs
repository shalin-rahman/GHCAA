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

            // The seeder only fills an empty table, so a live database never picks up the thumbnail
            // paths added to photos.json. These match the shipped seed photos by exact path and
            // touch nothing on a database that never had them.
            migrationBuilder.Sql("""UPDATE "EventPhotos" SET "ThumbnailPath" = '/assets/gallery/1st-grand-reunion/thumbs/album_1st-grand-reunion_01.jpeg' WHERE "PhotoPath" = '/assets/gallery/1st-grand-reunion/album_1st-grand-reunion_01.jpeg' AND "ThumbnailPath" IS NULL;""");
            migrationBuilder.Sql("""UPDATE "EventPhotos" SET "ThumbnailPath" = '/assets/gallery/bangla-new-year/thumbs/album_bangla-new-year_01.jpg' WHERE "PhotoPath" = '/assets/gallery/bangla-new-year/album_bangla-new-year_01.jpg' AND "ThumbnailPath" IS NULL;""");
            migrationBuilder.Sql("""UPDATE "EventPhotos" SET "ThumbnailPath" = '/assets/gallery/iftar-2026/thumbs/album_iftar-2026_02.jpg' WHERE "PhotoPath" = '/assets/gallery/iftar-2026/album_iftar-2026_02.jpg' AND "ThumbnailPath" IS NULL;""");
            migrationBuilder.Sql("""UPDATE "EventPhotos" SET "ThumbnailPath" = '/assets/gallery/iftar-2026/thumbs/album_iftar-2026_03.jpg' WHERE "PhotoPath" = '/assets/gallery/iftar-2026/album_iftar-2026_03.jpg' AND "ThumbnailPath" IS NULL;""");
            migrationBuilder.Sql("""UPDATE "EventPhotos" SET "ThumbnailPath" = '/assets/gallery/iftar-2026/thumbs/album_iftar-2026_04.jpg' WHERE "PhotoPath" = '/assets/gallery/iftar-2026/album_iftar-2026_04.jpg' AND "ThumbnailPath" IS NULL;""");
            migrationBuilder.Sql("""UPDATE "EventPhotos" SET "ThumbnailPath" = '/assets/gallery/iftar-2026/thumbs/album_iftar-2026_05.jpg' WHERE "PhotoPath" = '/assets/gallery/iftar-2026/album_iftar-2026_05.jpg' AND "ThumbnailPath" IS NULL;""");
            migrationBuilder.Sql("""UPDATE "EventPhotos" SET "ThumbnailPath" = '/assets/gallery/new-principal/thumbs/album_new-principal_01.jpg' WHERE "PhotoPath" = '/assets/gallery/new-principal/album_new-principal_01.jpg' AND "ThumbnailPath" IS NULL;""");
            migrationBuilder.Sql("""UPDATE "EventPhotos" SET "ThumbnailPath" = '/assets/gallery/new-principal/thumbs/album_new-principal_02.jpeg' WHERE "PhotoPath" = '/assets/gallery/new-principal/album_new-principal_02.jpeg' AND "ThumbnailPath" IS NULL;""");
            migrationBuilder.Sql("""UPDATE "EventPhotos" SET "ThumbnailPath" = '/assets/gallery/new-principal/thumbs/album_new-principal_03.jpg' WHERE "PhotoPath" = '/assets/gallery/new-principal/album_new-principal_03.jpg' AND "ThumbnailPath" IS NULL;""");
            migrationBuilder.Sql("""UPDATE "EventPhotos" SET "ThumbnailPath" = '/assets/gallery/principal-abul-kasem-kulkharni/thumbs/album_principal-abul-kasem-kulkharni_01.jpg' WHERE "PhotoPath" = '/assets/gallery/principal-abul-kasem-kulkharni/album_principal-abul-kasem-kulkharni_01.jpg' AND "ThumbnailPath" IS NULL;""");
            migrationBuilder.Sql("""UPDATE "EventPhotos" SET "ThumbnailPath" = '/assets/gallery/principal-abul-kasem-kulkharni/thumbs/album_principal-abul-kasem-kulkharni_02.jpeg' WHERE "PhotoPath" = '/assets/gallery/principal-abul-kasem-kulkharni/album_principal-abul-kasem-kulkharni_02.jpeg' AND "ThumbnailPath" IS NULL;""");
            migrationBuilder.Sql("""UPDATE "EventPhotos" SET "ThumbnailPath" = '/assets/gallery/principal-abul-kasem-kulkharni/thumbs/album_principal-abul-kasem-kulkharni_03.jpeg' WHERE "PhotoPath" = '/assets/gallery/principal-abul-kasem-kulkharni/album_principal-abul-kasem-kulkharni_03.jpeg' AND "ThumbnailPath" IS NULL;""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""ALTER TABLE "EventPhotos" DROP COLUMN IF EXISTS "ThumbnailPath";""");
        }
    }
}
