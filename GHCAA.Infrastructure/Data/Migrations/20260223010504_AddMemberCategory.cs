using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Members",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    TargetUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Members_UserId",
                        column: x => x.UserId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2026, 2, 23, 1, 5, 3, 24, DateTimeKind.Utc).AddTicks(6312));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2026, 2, 23, 1, 5, 3, 24, DateTimeKind.Utc).AddTicks(7235));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "ECPosition", "LastUpdateDate" },
                values: new object[] { 0, 9, new DateTime(2026, 2, 23, 1, 5, 3, 25, DateTimeKind.Utc).AddTicks(7725) });

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 23, 1, 5, 3, 568, DateTimeKind.Utc).AddTicks(8961));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 23, 1, 5, 3, 569, DateTimeKind.Utc).AddTicks(155));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 23, 1, 5, 3, 569, DateTimeKind.Utc).AddTicks(159));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 23, 1, 5, 3, 569, DateTimeKind.Utc).AddTicks(161));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "Description", "MembershipType" },
                values: new object[] { new DateTime(2026, 2, 23, 1, 5, 3, 569, DateTimeKind.Utc).AddTicks(225), "Honorary Member Fee", 4 });

            migrationBuilder.InsertData(
                table: "MembershipFeeConfigs",
                columns: new[] { "Id", "Amount", "CreatedAt", "CreatedByAdminId", "Description", "EffectiveDate", "MembershipType" },
                values: new object[] { 6, 0m, new DateTime(2026, 2, 23, 1, 5, 3, 569, DateTimeKind.Utc).AddTicks(228), null, "Advisory Member Fee", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$TJZYgpcHqVct.TadUtyKPevHWV5.FKUIS0G.uWytUCJqAWfZLEP3O");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$WgiZ9fPVPELxMlIqspGJVO8gO76TZ1bERdcmU8arLu7bVdr8/XxMy");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DeleteData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Members");

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2026, 2, 22, 19, 8, 59, 273, DateTimeKind.Utc).AddTicks(6386));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2026, 2, 22, 19, 8, 59, 273, DateTimeKind.Utc).AddTicks(7688));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ECPosition", "LastUpdateDate" },
                values: new object[] { 3, new DateTime(2026, 2, 22, 19, 8, 59, 275, DateTimeKind.Utc).AddTicks(4815) });

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 19, 8, 59, 847, DateTimeKind.Utc).AddTicks(2420));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 19, 8, 59, 847, DateTimeKind.Utc).AddTicks(3689));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 19, 8, 59, 847, DateTimeKind.Utc).AddTicks(3692));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 22, 19, 8, 59, 847, DateTimeKind.Utc).AddTicks(3694));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "Description", "MembershipType" },
                values: new object[] { new DateTime(2026, 2, 22, 19, 8, 59, 847, DateTimeKind.Utc).AddTicks(3696), "Life Member Fee", 6 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$yUryc8gFlef8/.jJugVivORnhn76z3IW1HsiAiRjrIvZfPltqSlaC");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$Uoul61VXNQMEPT/uTJSutuHZj.I4quWIR43OTe5FjkKccDodHEj0K");
        }
    }
}
