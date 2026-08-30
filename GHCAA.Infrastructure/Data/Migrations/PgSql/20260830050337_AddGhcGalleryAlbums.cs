using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddGhcGalleryAlbums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO ""EventGalleries"" (""Id"", ""Title"", ""Description"", ""EventDate"", ""Location"", ""CreatedAt"", ""CreatedByAdminId"", ""IsActive"", ""IsFeatured"", ""OwnerMemberId"", ""Status"", ""RejectionReason"")
VALUES
    (2, 'GHC 1st Grand Reunion', 'Photo memories from the first grand reunion of GHC alumni.', '2026-01-24T09:00:00Z', 'College Campus Ground', '2026-08-30T00:00:00Z', 1, true, false, NULL, 2, NULL),
    (3, 'Bangla New Year', 'Celebrating Pohela Boishakh with the GHC alumni community.', '2026-04-14T00:00:00Z', NULL, '2026-08-30T00:00:00Z', 1, true, false, NULL, 2, NULL),
    (4, 'Campus', 'Scenes from the Gyaneswari High College campus.', '2026-08-29T00:00:00Z', NULL, '2026-08-30T00:00:00Z', 1, true, false, NULL, 2, NULL),
    (5, 'Iftar 2026', 'Iftar gathering with GHC alumni and guests.', '2026-03-14T07:21:00Z', 'Darbar Party Center', '2026-08-30T00:00:00Z', 1, true, false, NULL, 2, NULL),
    (6, 'New Principal', 'Welcoming the new principal of Gyaneswari High College.', '2026-08-26T00:00:00Z', NULL, '2026-08-30T00:00:00Z', 1, true, false, NULL, 2, NULL),
    (7, 'Principal Abul Kasem Kulkharni', 'Tribute photos of Principal Abul Kasem Kulkharni.', '2026-08-16T00:00:00Z', NULL, '2026-08-30T00:00:00Z', 1, true, false, NULL, 2, NULL)
ON CONFLICT (""Id"") DO NOTHING;

INSERT INTO ""EventPhotos"" (""Id"", ""EventGalleryId"", ""PhotoPath"", ""Caption"", ""UploadedAt"", ""UploadedByMemberId"", ""Status"", ""RejectionReason"")
VALUES
    (2, 2, '/assets/gallery/1st-grand-reunion/album_1st-grand-reunion_01.jpeg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (3, 3, '/assets/gallery/bangla-new-year/album_bangla-new-year_01.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (4, 4, '/assets/gallery/campus/album_campus_01.webp', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (5, 4, '/assets/gallery/campus/album_campus_02.webp', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (6, 4, '/assets/gallery/campus/album_campus_03.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (7, 4, '/assets/gallery/campus/album_campus_04.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (8, 4, '/assets/gallery/campus/album_campus_05.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (9, 4, '/assets/gallery/campus/album_campus_06.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (10, 4, '/assets/gallery/campus/album_campus_07.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (11, 4, '/assets/gallery/campus/album_campus_08.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (12, 4, '/assets/gallery/campus/album_campus_09.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (13, 4, '/assets/gallery/campus/album_campus_10.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (14, 4, '/assets/gallery/campus/album_campus_11.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (15, 4, '/assets/gallery/campus/album_campus_12.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (16, 4, '/assets/gallery/campus/album_campus_13.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (17, 5, '/assets/gallery/iftar-2026/album_iftar-2026_01.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (18, 5, '/assets/gallery/iftar-2026/album_iftar-2026_02.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (19, 5, '/assets/gallery/iftar-2026/album_iftar-2026_03.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (20, 5, '/assets/gallery/iftar-2026/album_iftar-2026_04.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (21, 5, '/assets/gallery/iftar-2026/album_iftar-2026_05.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (22, 6, '/assets/gallery/new-principal/album_new-principal_01.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (23, 6, '/assets/gallery/new-principal/album_new-principal_02.jpeg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (24, 6, '/assets/gallery/new-principal/album_new-principal_03.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (25, 7, '/assets/gallery/principal-abul-kasem-kulkharni/album_principal-abul-kasem-kulkharni_01.jpg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (26, 7, '/assets/gallery/principal-abul-kasem-kulkharni/album_principal-abul-kasem-kulkharni_02.jpeg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL),
    (27, 7, '/assets/gallery/principal-abul-kasem-kulkharni/album_principal-abul-kasem-kulkharni_03.jpeg', NULL, '2026-08-30T00:00:00Z', NULL, 2, NULL)
ON CONFLICT (""Id"") DO NOTHING;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM ""EventPhotos"" WHERE ""Id"" BETWEEN 2 AND 27;
DELETE FROM ""EventGalleries"" WHERE ""Id"" BETWEEN 2 AND 7;
");
        }
    }
}
