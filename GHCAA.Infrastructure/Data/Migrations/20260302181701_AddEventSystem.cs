using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlumniEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RegistrationFee = table.Column<decimal>(type: "numeric", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlumniEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    MemberId = table.Column<int>(type: "integer", nullable: false),
                    PaymentReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReceiptPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByAdminId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventRegistrations_AlumniEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "AlumniEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventRegistrations_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 55, DateTimeKind.Utc).AddTicks(182));

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 55, DateTimeKind.Utc).AddTicks(1162));

            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "Id", "Body", "Code", "LastUpdated", "Subject", "Variables" },
                values: new object[] { 3, "Dear {{FullName}}, your registration for the event <strong>{{EventTitle}}</strong> has been approved. <br/><br/><strong>Event Details:</strong><br/>Date: {{EventDate}}<br/>Location: {{EventLocation}}<br/><br/>Looking forward to seeing you there!", "EVENT_REGISTRATION_CONFIRMATION", new DateTime(2026, 3, 2, 18, 17, 0, 55, DateTimeKind.Utc).AddTicks(1164), "Registration Confirmed: {{EventTitle}}", "['FullName', 'EventTitle', 'EventDate', 'EventLocation']" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdateDate",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 56, DateTimeKind.Utc).AddTicks(3607));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 598, DateTimeKind.Utc).AddTicks(6318));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 598, DateTimeKind.Utc).AddTicks(8058));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 598, DateTimeKind.Utc).AddTicks(8062));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 598, DateTimeKind.Utc).AddTicks(8064));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 598, DateTimeKind.Utc).AddTicks(8066));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 18, 17, 0, 598, DateTimeKind.Utc).AddTicks(8068));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$G7.EABGSSKHqD7Mpffh2Je3GzGJTp0mQEs0D6KigmPaA0f5J2hFAm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$QpeEP.gPueRwQWnby7/tvOR7KeCaRnadiGsHXgP/rOMdpx8.z0NZu");

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_EventId",
                table: "EventRegistrations",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_MemberId",
                table: "EventRegistrations",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRegistrations");

            migrationBuilder.DropTable(
                name: "AlumniEvents");

            migrationBuilder.DeleteData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 3);

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
                column: "LastUpdateDate",
                value: new DateTime(2026, 2, 23, 1, 5, 3, 25, DateTimeKind.Utc).AddTicks(7725));

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
                column: "CreatedAt",
                value: new DateTime(2026, 2, 23, 1, 5, 3, 569, DateTimeKind.Utc).AddTicks(225));

            migrationBuilder.UpdateData(
                table: "MembershipFeeConfigs",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 23, 1, 5, 3, 569, DateTimeKind.Utc).AddTicks(228));

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
        }
    }
}
