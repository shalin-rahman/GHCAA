using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <summary>
    /// docs/TODO.md 82.53a: three nullable columns on Members holding the mobile client's
    /// current FCM token, so /notifications/device-token has somewhere to persist it.
    ///
    /// Hand-written rather than scaffolded: `dotnet ef migrations add` truncated the ~74k-line
    /// PgSqlApplicationDbContextModelSnapshot.cs to a few thousand lines (see
    /// gotcha_ef_migrations_add_remove_corrupts_snapshot) — discarded, snapshot restored from
    /// the index, and the three properties added to the snapshot by hand instead.
    /// </summary>
    public partial class AddMemberDeviceToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FcmToken",
                table: "Members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FcmTokenPlatform",
                table: "Members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FcmTokenUpdatedAt",
                table: "Members",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FcmToken",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "FcmTokenPlatform",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "FcmTokenUpdatedAt",
                table: "Members");
        }
    }
}
