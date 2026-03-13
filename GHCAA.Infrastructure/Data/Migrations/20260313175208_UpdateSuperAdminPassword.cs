using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSuperAdminPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "PaymentHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$CQ4KnTDZ7qUQMNre86iruOpgOx8fEoMe2G3RF/1U4cCLa5ltYtE1O");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$J0UJbz.FdyElDw2mV22g1OikjTExwKvZ.c4eP3Wenc1MkmYDrgUme");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "PaymentHistories");

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 1,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 12, 23, 9, 44, 682, DateTimeKind.Utc).AddTicks(8723));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 2,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 12, 23, 9, 44, 683, DateTimeKind.Utc).AddTicks(998));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 3,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 12, 23, 9, 44, 683, DateTimeKind.Utc).AddTicks(1001));

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 4,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 12, 23, 9, 44, 683, DateTimeKind.Utc).AddTicks(1004));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$PsQ96./aGE/qwhaRF8f/quI32akOnAxuaAXh2Inrvz0CenOsG/Py.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$1tNxw.gy4OW16EqT0GpN9eFDSYwhoooPkovDBi1KLYP1SQWEaqQaW");
        }
    }
}
