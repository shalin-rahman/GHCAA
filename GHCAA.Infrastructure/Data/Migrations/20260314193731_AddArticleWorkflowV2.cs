using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleWorkflowV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "NewsPosts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasAcceptedTerms",
                table: "Members",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                column: "HasAcceptedTerms",
                value: true);

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 101,
                column: "HasAcceptedTerms",
                value: true);

            migrationBuilder.UpdateData(
                table: "NewsPosts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "Status" },
                values: new object[] { 2, 2 });

            migrationBuilder.UpdateData(
                table: "NewsPosts",
                keyColumn: "Id",
                keyValue: 2,
                column: "Status",
                value: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "NewsPosts");

            migrationBuilder.DropColumn(
                name: "HasAcceptedTerms",
                table: "Members");

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 1,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 13, 17, 52, 6, 814, DateTimeKind.Utc).AddTicks(3724));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 2,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 13, 17, 52, 6, 814, DateTimeKind.Utc).AddTicks(5063));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 3,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 13, 17, 52, 6, 814, DateTimeKind.Utc).AddTicks(5066));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 4,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 13, 17, 52, 6, 814, DateTimeKind.Utc).AddTicks(5067));

            migrationBuilder.UpdateData(
                table: "NewsPosts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Category",
                value: 1);
        }
    }
}
