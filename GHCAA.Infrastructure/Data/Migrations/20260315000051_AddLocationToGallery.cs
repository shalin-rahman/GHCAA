using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationToGallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "EventGalleries",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "EventGalleries",
                keyColumn: "Id",
                keyValue: 1,
                column: "Location",
                value: null);

            migrationBuilder.UpdateData(
                table: "EventGalleries",
                keyColumn: "Id",
                keyValue: 2,
                column: "Location",
                value: null);

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 1,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 15, 0, 0, 50, 516, DateTimeKind.Utc).AddTicks(3068));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 2,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 15, 0, 0, 50, 516, DateTimeKind.Utc).AddTicks(3640));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 3,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 15, 0, 0, 50, 516, DateTimeKind.Utc).AddTicks(3641));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 4,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 15, 0, 0, 50, 516, DateTimeKind.Utc).AddTicks(3643));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "EventGalleries");

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 1,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 14, 19, 37, 30, 308, DateTimeKind.Utc).AddTicks(3458));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 2,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 14, 19, 37, 30, 308, DateTimeKind.Utc).AddTicks(4020));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 3,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 14, 19, 37, 30, 308, DateTimeKind.Utc).AddTicks(4021));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 4,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 14, 19, 37, 30, 308, DateTimeKind.Utc).AddTicks(4022));
        }
    }
}
