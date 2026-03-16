using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GHCAA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventRequiresPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RolesId", "UsersId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "IsSandbox",
                table: "PaymentConfigurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresPayment",
                table: "AlumniEvents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AlumniEvents",
                keyColumn: "Id",
                keyValue: 1,
                column: "RequiresPayment",
                value: true);

            migrationBuilder.DeleteData(table: "AlumniEvents", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "AlumniEvents", keyColumn: "Id", keyValue: 3);
            migrationBuilder.DeleteData(table: "AlumniEvents", keyColumn: "Id", keyValue: 4);

            migrationBuilder.InsertData(
                table: "AlumniEvents",
                columns: new[] { "Id", "AdminNote", "AllowNonMembers", "CreatedAt", "Date", "Description", "ImageUrl", "IsActive", "Location", "RegistrationDeadline", "RegistrationFee", "RequiresPayment", "Title" },
                values: new object[,]
                {
                    { 2, null, true, new DateTime(2026, 3, 13, 7, 22, 3, 166, DateTimeKind.Utc).AddTicks(5720), new DateTime(2026, 3, 14, 7, 21, 0, 0, DateTimeKind.Utc), "Iftar 2026", null, true, "Darbar Party Center", new DateTime(2026, 3, 8, 7, 21, 0, 0, DateTimeKind.Utc), 0m, true, "Iftar 2026" },
                    { 3, null, false, new DateTime(2026, 3, 13, 8, 38, 59, 794, DateTimeKind.Utc).AddTicks(3100), new DateTime(2026, 6, 30, 1, 38, 0, 0, DateTimeKind.Utc), "Recreational ", null, true, "Savar", new DateTime(2026, 5, 31, 8, 38, 0, 0, DateTimeKind.Utc), 2000m, true, "Annual Tour" },
                    { 4, null, true, new DateTime(2026, 3, 13, 9, 41, 12, 13, DateTimeKind.Utc).AddTicks(1080), new DateTime(2026, 4, 28, 9, 40, 0, 0, DateTimeKind.Utc), "FF", null, true, "BD", new DateTime(2026, 3, 31, 9, 40, 0, 0, DateTimeKind.Utc), 0m, true, "Flood fund collection" }
                });

            migrationBuilder.UpdateData(
                table: "ECPeriods",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EndDate", "StartDate", "Title" },
                values: new object[] { new DateTime(2026, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Interim Executive Committee" });

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 1,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 15, 10, 17, 45, 660, DateTimeKind.Utc).AddTicks(9570));

            migrationBuilder.DeleteData(table: "JobOpportunities", keyColumn: "Id", keyValue: 2);

            migrationBuilder.InsertData(
                table: "JobOpportunities",
                columns: new[] { "Id", "ApplicationLink", "Category", "Company", "ContactEmail", "Description", "ExpiryDate", "IsActive", "Location", "PostedById", "PostedByMemberId", "PostedDate", "Requirements", "Title" },
                values: new object[] { 2, null, 3, "Square Pharma", "", "Sales Promotion", null, true, "Dhaka, Bangladesh", null, 1, new DateTime(2026, 3, 13, 8, 5, 57, 627, DateTimeKind.Utc).AddTicks(6790), "", "Sales Representitive" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 42, 419, DateTimeKind.Utc).AddTicks(30), new DateTime(2026, 3, 15, 16, 15, 42, 419, DateTimeKind.Utc).AddTicks(400), "uploads/members/seed/2512003.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 42, 897, DateTimeKind.Utc).AddTicks(8100), new DateTime(2026, 3, 15, 16, 15, 42, 897, DateTimeKind.Utc).AddTicks(8120), "uploads/members/seed/2512005.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 42, 954, DateTimeKind.Utc).AddTicks(6640), new DateTime(2026, 3, 15, 16, 15, 42, 954, DateTimeKind.Utc).AddTicks(6670), "uploads/members/seed/2512006.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 42, 978, DateTimeKind.Utc).AddTicks(3150), new DateTime(2026, 3, 15, 16, 15, 42, 978, DateTimeKind.Utc).AddTicks(3190), "uploads/members/seed/2512012.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 204,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 329, DateTimeKind.Utc).AddTicks(6000), "uploads/members/seed/2512017.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 205,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 353, DateTimeKind.Utc).AddTicks(4570), "uploads/members/seed/2512019.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 206,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 476, DateTimeKind.Utc).AddTicks(7400), new DateTime(2026, 3, 15, 16, 15, 43, 476, DateTimeKind.Utc).AddTicks(7420), "uploads/members/seed/2512020.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 207,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 528, DateTimeKind.Utc).AddTicks(5650), new DateTime(2026, 3, 15, 16, 15, 43, 528, DateTimeKind.Utc).AddTicks(5670), "uploads/members/seed/2512022.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 208,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 556, DateTimeKind.Utc).AddTicks(2180), new DateTime(2026, 3, 15, 16, 15, 43, 556, DateTimeKind.Utc).AddTicks(2210), "uploads/members/seed/2512023.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 209,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 570, DateTimeKind.Utc).AddTicks(230), new DateTime(2026, 3, 15, 16, 15, 43, 570, DateTimeKind.Utc).AddTicks(250), "uploads/members/seed/2512027.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 210,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 581, DateTimeKind.Utc).AddTicks(4310), new DateTime(2026, 3, 15, 16, 15, 43, 581, DateTimeKind.Utc).AddTicks(4330), "uploads/members/seed/2512028.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 211,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 593, DateTimeKind.Utc).AddTicks(2310), new DateTime(2026, 3, 15, 16, 15, 43, 593, DateTimeKind.Utc).AddTicks(2340), "uploads/members/seed/2512029.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 212,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 615, DateTimeKind.Utc).AddTicks(9780), "uploads/members/seed/2512030.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 213,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 718, DateTimeKind.Utc).AddTicks(3960), new DateTime(2026, 3, 15, 16, 15, 43, 718, DateTimeKind.Utc).AddTicks(3980), "uploads/members/seed/2512031.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 214,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 813, DateTimeKind.Utc).AddTicks(9430), new DateTime(2026, 3, 15, 16, 15, 43, 813, DateTimeKind.Utc).AddTicks(9450), "uploads/members/seed/2512032.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 215,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 855, DateTimeKind.Utc).AddTicks(8510), new DateTime(2026, 3, 15, 16, 15, 43, 855, DateTimeKind.Utc).AddTicks(8520), "uploads/members/seed/2512033.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 216,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 876, DateTimeKind.Utc).AddTicks(9560), new DateTime(2026, 3, 15, 16, 15, 43, 876, DateTimeKind.Utc).AddTicks(9570), "uploads/members/seed/2512034.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 217,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 892, DateTimeKind.Utc).AddTicks(8230), new DateTime(2026, 3, 15, 16, 15, 43, 892, DateTimeKind.Utc).AddTicks(8260), "uploads/members/seed/2512035.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 218,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 98, DateTimeKind.Utc).AddTicks(5210), new DateTime(2026, 3, 15, 16, 15, 44, 98, DateTimeKind.Utc).AddTicks(5240), "uploads/members/seed/2512036.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 219,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 184, DateTimeKind.Utc).AddTicks(4450), new DateTime(2026, 3, 15, 16, 15, 44, 184, DateTimeKind.Utc).AddTicks(4460), "uploads/members/seed/2512038.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 220,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 201, DateTimeKind.Utc).AddTicks(4400), new DateTime(2026, 3, 15, 16, 15, 44, 201, DateTimeKind.Utc).AddTicks(4420), "uploads/members/seed/2512040.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 221,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 218, DateTimeKind.Utc).AddTicks(2210), new DateTime(2026, 3, 15, 16, 15, 44, 218, DateTimeKind.Utc).AddTicks(2230), "uploads/members/seed/2512043.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 222,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 265, DateTimeKind.Utc).AddTicks(3380), new DateTime(2026, 3, 15, 16, 15, 44, 265, DateTimeKind.Utc).AddTicks(3400), "uploads/members/seed/2512044.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 223,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 296, DateTimeKind.Utc).AddTicks(4030), new DateTime(2026, 3, 15, 16, 15, 44, 296, DateTimeKind.Utc).AddTicks(4050), "uploads/members/seed/2512046.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 224,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 325, DateTimeKind.Utc).AddTicks(5140), new DateTime(2026, 3, 15, 16, 15, 44, 325, DateTimeKind.Utc).AddTicks(5150), "uploads/members/seed/2512047.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 225,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 378, DateTimeKind.Utc).AddTicks(5610), new DateTime(2026, 3, 15, 16, 15, 44, 378, DateTimeKind.Utc).AddTicks(5630), "uploads/members/seed/2512049.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 226,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 430, DateTimeKind.Utc).AddTicks(3300), new DateTime(2026, 3, 15, 16, 15, 44, 430, DateTimeKind.Utc).AddTicks(3320), "uploads/members/seed/2512050.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 227,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 446, DateTimeKind.Utc).AddTicks(8330), new DateTime(2026, 3, 15, 16, 15, 44, 446, DateTimeKind.Utc).AddTicks(8350), "uploads/members/seed/2512051.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 228,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 455, DateTimeKind.Utc).AddTicks(3140), new DateTime(2026, 3, 15, 16, 15, 44, 455, DateTimeKind.Utc).AddTicks(3160), "uploads/members/seed/2512052.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 229,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 532, DateTimeKind.Utc).AddTicks(8860), "uploads/members/seed/2512053.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 230,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 586, DateTimeKind.Utc).AddTicks(6840), "uploads/members/seed/2512054.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 231,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 613, DateTimeKind.Utc).AddTicks(4810), new DateTime(2026, 3, 15, 16, 15, 44, 613, DateTimeKind.Utc).AddTicks(4830), "uploads/members/seed/2512055.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 232,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 625, DateTimeKind.Utc).AddTicks(4130), new DateTime(2026, 3, 15, 16, 15, 44, 625, DateTimeKind.Utc).AddTicks(4150), "uploads/members/seed/2512056.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 233,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 642, DateTimeKind.Utc).AddTicks(5090), new DateTime(2026, 3, 15, 16, 15, 44, 642, DateTimeKind.Utc).AddTicks(5110), "uploads/members/seed/2512057.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 234,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 713, DateTimeKind.Utc).AddTicks(5330), new DateTime(2026, 3, 15, 16, 15, 44, 713, DateTimeKind.Utc).AddTicks(5350), "uploads/members/seed/2512058.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 235,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 721, DateTimeKind.Utc).AddTicks(9750), new DateTime(2026, 3, 15, 16, 15, 44, 721, DateTimeKind.Utc).AddTicks(9770), "uploads/members/seed/2512059.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 236,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 732, DateTimeKind.Utc).AddTicks(9680), new DateTime(2026, 3, 15, 16, 15, 44, 732, DateTimeKind.Utc).AddTicks(9700), "uploads/members/seed/2512060.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 237,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 747, DateTimeKind.Utc).AddTicks(4700), "uploads/members/seed/2512061.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 238,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 777, DateTimeKind.Utc).AddTicks(6330), new DateTime(2026, 3, 15, 16, 15, 44, 777, DateTimeKind.Utc).AddTicks(6350), "uploads/members/seed/2512064.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 239,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 839, DateTimeKind.Utc).AddTicks(4480), new DateTime(2026, 3, 15, 16, 15, 44, 839, DateTimeKind.Utc).AddTicks(4500), "uploads/members/seed/2512065.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 240,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 858, DateTimeKind.Utc).AddTicks(8310), new DateTime(2026, 3, 15, 16, 15, 44, 858, DateTimeKind.Utc).AddTicks(8320), "uploads/members/seed/2512066.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 241,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 897, DateTimeKind.Utc).AddTicks(1300), new DateTime(2026, 3, 15, 16, 15, 44, 897, DateTimeKind.Utc).AddTicks(1330), "uploads/members/seed/2512067.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 242,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 911, DateTimeKind.Utc).AddTicks(8730), new DateTime(2026, 3, 15, 16, 15, 44, 911, DateTimeKind.Utc).AddTicks(8750), "uploads/members/seed/2512069.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 243,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 919, DateTimeKind.Utc).AddTicks(9730), new DateTime(2026, 3, 15, 16, 15, 44, 919, DateTimeKind.Utc).AddTicks(9750), "uploads/members/seed/2512070.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 244,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 928, DateTimeKind.Utc).AddTicks(4830), new DateTime(2026, 3, 15, 16, 15, 44, 928, DateTimeKind.Utc).AddTicks(4840), "uploads/members/seed/2512071.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 245,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 940, DateTimeKind.Utc).AddTicks(710), new DateTime(2026, 3, 15, 16, 15, 44, 940, DateTimeKind.Utc).AddTicks(730), "uploads/members/seed/2512072.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 246,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 948, DateTimeKind.Utc).AddTicks(3560), new DateTime(2026, 3, 15, 16, 15, 44, 948, DateTimeKind.Utc).AddTicks(3580), "uploads/members/seed/2512073.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 247,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 960, DateTimeKind.Utc).AddTicks(8880), new DateTime(2026, 3, 15, 16, 15, 44, 960, DateTimeKind.Utc).AddTicks(8900), "uploads/members/seed/2512074.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 248,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 965, DateTimeKind.Utc).AddTicks(9020), new DateTime(2026, 3, 15, 16, 15, 44, 965, DateTimeKind.Utc).AddTicks(9040), "uploads/members/seed/2512075.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 249,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 977, DateTimeKind.Utc).AddTicks(4180), new DateTime(2026, 3, 15, 16, 15, 44, 977, DateTimeKind.Utc).AddTicks(4200), "uploads/members/seed/2512078.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 250,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 986, DateTimeKind.Utc).AddTicks(8150), new DateTime(2026, 3, 15, 16, 15, 44, 986, DateTimeKind.Utc).AddTicks(8170), "uploads/members/seed/2512079.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 251,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 999, DateTimeKind.Utc).AddTicks(840), "uploads/members/seed/2512080.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 252,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 10, DateTimeKind.Utc).AddTicks(2440), new DateTime(2026, 3, 15, 16, 15, 45, 10, DateTimeKind.Utc).AddTicks(2470), "uploads/members/seed/2512082.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 253,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 54, DateTimeKind.Utc).AddTicks(6780), "uploads/members/seed/2512083.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 254,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 122, DateTimeKind.Utc).AddTicks(620), new DateTime(2026, 3, 15, 16, 15, 45, 122, DateTimeKind.Utc).AddTicks(640), "uploads/members/seed/2512084.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 255,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 139, DateTimeKind.Utc).AddTicks(5850), new DateTime(2026, 3, 15, 16, 15, 45, 139, DateTimeKind.Utc).AddTicks(5860), "uploads/members/seed/2512087.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 256,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 149, DateTimeKind.Utc).AddTicks(4140), new DateTime(2026, 3, 15, 16, 15, 45, 149, DateTimeKind.Utc).AddTicks(4150), "uploads/members/seed/2512088.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 257,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 179, DateTimeKind.Utc).AddTicks(8870), "uploads/members/seed/2512089.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 258,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 216, DateTimeKind.Utc).AddTicks(9900), new DateTime(2026, 3, 15, 16, 15, 45, 216, DateTimeKind.Utc).AddTicks(9910), "uploads/members/seed/2512091.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 259,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 262, DateTimeKind.Utc).AddTicks(1700), new DateTime(2026, 3, 15, 16, 15, 45, 262, DateTimeKind.Utc).AddTicks(1710), "uploads/members/seed/2512092.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 260,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 273, DateTimeKind.Utc).AddTicks(9130), "uploads/members/seed/2512093.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 261,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 309, DateTimeKind.Utc).AddTicks(7160), "uploads/members/seed/2512094.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 262,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 360, DateTimeKind.Utc).AddTicks(8860), "uploads/members/seed/2512095.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 263,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 385, DateTimeKind.Utc).AddTicks(7510), "uploads/members/seed/2512096.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 264,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 394, DateTimeKind.Utc).AddTicks(6020), new DateTime(2026, 3, 15, 16, 15, 45, 394, DateTimeKind.Utc).AddTicks(6030), "uploads/members/seed/2512097.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 265,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 400, DateTimeKind.Utc).AddTicks(5590), new DateTime(2026, 3, 15, 16, 15, 45, 400, DateTimeKind.Utc).AddTicks(5600), "uploads/members/seed/2512098.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 266,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 558, DateTimeKind.Utc).AddTicks(5460), new DateTime(2026, 3, 15, 16, 15, 45, 558, DateTimeKind.Utc).AddTicks(5470), "uploads/members/seed/2512099.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 267,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 591, DateTimeKind.Utc).AddTicks(3040), "uploads/members/seed/2512101.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 268,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 609, DateTimeKind.Utc).AddTicks(7160), new DateTime(2026, 3, 15, 16, 15, 45, 609, DateTimeKind.Utc).AddTicks(7170), "uploads/members/seed/2512102.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 269,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 619, DateTimeKind.Utc).AddTicks(6550), new DateTime(2026, 3, 15, 16, 15, 45, 619, DateTimeKind.Utc).AddTicks(6560), "uploads/members/seed/2512103.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 270,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 630, DateTimeKind.Utc).AddTicks(5530), new DateTime(2026, 3, 15, 16, 15, 45, 630, DateTimeKind.Utc).AddTicks(5540), "uploads/members/seed/2512105.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 271,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 696, DateTimeKind.Utc).AddTicks(110), new DateTime(2026, 3, 15, 16, 15, 45, 696, DateTimeKind.Utc).AddTicks(120), "uploads/members/seed/2512106.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 272,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 713, DateTimeKind.Utc).AddTicks(8070), new DateTime(2026, 3, 15, 16, 15, 45, 713, DateTimeKind.Utc).AddTicks(8090), "uploads/members/seed/2512107.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 273,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 729, DateTimeKind.Utc).AddTicks(2440), new DateTime(2026, 3, 15, 16, 15, 45, 729, DateTimeKind.Utc).AddTicks(2450), "uploads/members/seed/2512108.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 274,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 739, DateTimeKind.Utc).AddTicks(9920), new DateTime(2026, 3, 15, 16, 15, 45, 739, DateTimeKind.Utc).AddTicks(9930), "uploads/members/seed/2512110.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 275,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 758, DateTimeKind.Utc).AddTicks(1090), new DateTime(2026, 3, 15, 16, 15, 45, 758, DateTimeKind.Utc).AddTicks(1100), "uploads/members/seed/2512111.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 276,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 796, DateTimeKind.Utc).AddTicks(750), "uploads/members/seed/2512112.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 277,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 804, DateTimeKind.Utc).AddTicks(650), new DateTime(2026, 3, 15, 16, 15, 45, 804, DateTimeKind.Utc).AddTicks(650), "uploads/members/seed/2512113.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 278,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 814, DateTimeKind.Utc).AddTicks(760), "uploads/members/seed/2512114.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 279,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 826, DateTimeKind.Utc).AddTicks(5780), new DateTime(2026, 3, 15, 16, 15, 45, 826, DateTimeKind.Utc).AddTicks(5790), "uploads/members/seed/2512115.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 280,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 840, DateTimeKind.Utc).AddTicks(7120), new DateTime(2026, 3, 15, 16, 15, 45, 840, DateTimeKind.Utc).AddTicks(7130), "uploads/members/seed/2512116.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 281,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 847, DateTimeKind.Utc).AddTicks(3120), new DateTime(2026, 3, 15, 16, 15, 45, 847, DateTimeKind.Utc).AddTicks(3130), "uploads/members/seed/2512117.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 282,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 860, DateTimeKind.Utc).AddTicks(8580), new DateTime(2026, 3, 15, 16, 15, 45, 860, DateTimeKind.Utc).AddTicks(8580), "uploads/members/seed/2512118.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 283,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 893, DateTimeKind.Utc).AddTicks(740), new DateTime(2026, 3, 15, 16, 15, 45, 893, DateTimeKind.Utc).AddTicks(760), "uploads/members/seed/2512120.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 284,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 899, DateTimeKind.Utc).AddTicks(3610), new DateTime(2026, 3, 15, 16, 15, 45, 899, DateTimeKind.Utc).AddTicks(3620), "uploads/members/seed/2512123.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 285,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 911, DateTimeKind.Utc).AddTicks(8100), new DateTime(2026, 3, 15, 16, 15, 45, 911, DateTimeKind.Utc).AddTicks(8100), "uploads/members/seed/2512125.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 286,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 919, DateTimeKind.Utc).AddTicks(9040), "uploads/members/seed/2512126.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 287,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 935, DateTimeKind.Utc).AddTicks(9640), new DateTime(2026, 3, 15, 16, 15, 45, 935, DateTimeKind.Utc).AddTicks(9640), "uploads/members/seed/2512127.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 288,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 980, DateTimeKind.Utc).AddTicks(4360), new DateTime(2026, 3, 15, 16, 15, 45, 980, DateTimeKind.Utc).AddTicks(4370), "uploads/members/seed/2512128.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 289,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 989, DateTimeKind.Utc).AddTicks(7480), "uploads/members/seed/2512129.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 290,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 3, DateTimeKind.Utc).AddTicks(3570), new DateTime(2026, 3, 15, 16, 15, 46, 3, DateTimeKind.Utc).AddTicks(3580), "uploads/members/seed/2512131.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 291,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 123, DateTimeKind.Utc).AddTicks(9050), new DateTime(2026, 3, 15, 16, 15, 46, 123, DateTimeKind.Utc).AddTicks(9060), "uploads/members/seed/2512135.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 292,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 172, DateTimeKind.Utc).AddTicks(1680), new DateTime(2026, 3, 15, 16, 15, 46, 172, DateTimeKind.Utc).AddTicks(1690), "uploads/members/seed/2512136.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 293,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 205, DateTimeKind.Utc).AddTicks(6020), "uploads/members/seed/2512137.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 294,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 211, DateTimeKind.Utc).AddTicks(4340), "uploads/members/seed/2512138.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 295,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 256, DateTimeKind.Utc).AddTicks(3690), new DateTime(2026, 3, 15, 16, 15, 46, 256, DateTimeKind.Utc).AddTicks(3700), "uploads/members/seed/2512139.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 296,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 265, DateTimeKind.Utc).AddTicks(8370), new DateTime(2026, 3, 15, 16, 15, 46, 265, DateTimeKind.Utc).AddTicks(8380), "uploads/members/seed/2512140.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 297,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 278, DateTimeKind.Utc).AddTicks(9250), "uploads/members/seed/2512141.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 298,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 292, DateTimeKind.Utc).AddTicks(1850), new DateTime(2026, 3, 15, 16, 15, 46, 292, DateTimeKind.Utc).AddTicks(1850), "uploads/members/seed/2512142.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 299,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 319, DateTimeKind.Utc).AddTicks(7770), "uploads/members/seed/2512143.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 355, DateTimeKind.Utc).AddTicks(6050), new DateTime(2026, 3, 15, 16, 15, 46, 355, DateTimeKind.Utc).AddTicks(6070), "uploads/members/seed/2512144.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 379, DateTimeKind.Utc).AddTicks(420), new DateTime(2026, 3, 15, 16, 15, 46, 379, DateTimeKind.Utc).AddTicks(430), "uploads/members/seed/2512145.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 302,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 386, DateTimeKind.Utc).AddTicks(3610), new DateTime(2026, 3, 15, 16, 15, 46, 386, DateTimeKind.Utc).AddTicks(3620), "uploads/members/seed/2512146.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 303,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 435, DateTimeKind.Utc).AddTicks(1340), new DateTime(2026, 3, 15, 16, 15, 46, 435, DateTimeKind.Utc).AddTicks(1350), "uploads/members/seed/2512147.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 304,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 453, DateTimeKind.Utc).AddTicks(6440), "uploads/members/seed/2512148.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 305,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 470, DateTimeKind.Utc).AddTicks(4660), new DateTime(2026, 3, 15, 16, 15, 46, 470, DateTimeKind.Utc).AddTicks(4670), "uploads/members/seed/2512149.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 306,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 502, DateTimeKind.Utc).AddTicks(5680), new DateTime(2026, 3, 15, 16, 15, 46, 502, DateTimeKind.Utc).AddTicks(5700), "uploads/members/seed/2512150.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 307,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 509, DateTimeKind.Utc).AddTicks(4880), "uploads/members/seed/2512151.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 308,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 525, DateTimeKind.Utc).AddTicks(8200), new DateTime(2026, 3, 15, 16, 15, 46, 525, DateTimeKind.Utc).AddTicks(8220), "uploads/members/seed/2512152.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 309,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 569, DateTimeKind.Utc).AddTicks(7480), "uploads/members/seed/2512153.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 310,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 579, DateTimeKind.Utc).AddTicks(10), new DateTime(2026, 3, 15, 16, 15, 46, 579, DateTimeKind.Utc).AddTicks(20), "uploads/members/seed/2512154.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 311,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 661, DateTimeKind.Utc).AddTicks(5650), new DateTime(2026, 3, 15, 16, 15, 46, 661, DateTimeKind.Utc).AddTicks(5660), "uploads/members/seed/2512155.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 312,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 695, DateTimeKind.Utc).AddTicks(100), new DateTime(2026, 3, 15, 16, 15, 46, 695, DateTimeKind.Utc).AddTicks(110), "uploads/members/seed/2512156.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 313,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 710, DateTimeKind.Utc).AddTicks(4430), new DateTime(2026, 3, 15, 16, 15, 46, 710, DateTimeKind.Utc).AddTicks(4440), "uploads/members/seed/2512157.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 314,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 737, DateTimeKind.Utc).AddTicks(4040), new DateTime(2026, 3, 15, 16, 15, 46, 737, DateTimeKind.Utc).AddTicks(4050), "uploads/members/seed/2512158.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 315,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 786, DateTimeKind.Utc).AddTicks(7970), new DateTime(2026, 3, 15, 16, 15, 46, 786, DateTimeKind.Utc).AddTicks(7980), "uploads/members/seed/2512159.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 316,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 799, DateTimeKind.Utc).AddTicks(9510), new DateTime(2026, 3, 15, 16, 15, 46, 799, DateTimeKind.Utc).AddTicks(9520), "uploads/members/seed/2512160.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 317,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 845, DateTimeKind.Utc).AddTicks(3220), new DateTime(2026, 3, 15, 16, 15, 46, 845, DateTimeKind.Utc).AddTicks(3230), "uploads/members/seed/2512161.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 318,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 914, DateTimeKind.Utc).AddTicks(4160), new DateTime(2026, 3, 15, 16, 15, 46, 914, DateTimeKind.Utc).AddTicks(4180), "uploads/members/seed/2512162.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 319,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 956, DateTimeKind.Utc).AddTicks(5960), "uploads/members/seed/2512163.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 320,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 30, DateTimeKind.Utc).AddTicks(9460), new DateTime(2026, 3, 15, 16, 15, 47, 30, DateTimeKind.Utc).AddTicks(9470), "uploads/members/seed/2512164.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 321,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 82, DateTimeKind.Utc).AddTicks(4940), new DateTime(2026, 3, 15, 16, 15, 47, 82, DateTimeKind.Utc).AddTicks(4960), "uploads/members/seed/2512165.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 322,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 107, DateTimeKind.Utc).AddTicks(3750), new DateTime(2026, 3, 15, 16, 15, 47, 107, DateTimeKind.Utc).AddTicks(3750), "uploads/members/seed/2512171.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 323,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 115, DateTimeKind.Utc).AddTicks(3830), new DateTime(2026, 3, 15, 16, 15, 47, 115, DateTimeKind.Utc).AddTicks(3840), "uploads/members/seed/2512172.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 324,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 122, DateTimeKind.Utc).AddTicks(7520), new DateTime(2026, 3, 15, 16, 15, 47, 122, DateTimeKind.Utc).AddTicks(7540), "uploads/members/seed/2512173.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 325,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 138, DateTimeKind.Utc).AddTicks(490), "uploads/members/seed/2512174.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 326,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 171, DateTimeKind.Utc).AddTicks(2690), new DateTime(2026, 3, 15, 16, 15, 47, 171, DateTimeKind.Utc).AddTicks(2700), "uploads/members/seed/2512175.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 327,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 199, DateTimeKind.Utc).AddTicks(1740), "uploads/members/seed/2512176.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 328,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 231, DateTimeKind.Utc).AddTicks(2600), new DateTime(2026, 3, 15, 16, 15, 47, 231, DateTimeKind.Utc).AddTicks(2620), "uploads/members/seed/2512177.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 329,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 283, DateTimeKind.Utc).AddTicks(9410), new DateTime(2026, 3, 15, 16, 15, 47, 283, DateTimeKind.Utc).AddTicks(9420), "uploads/members/seed/2512179.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 330,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 309, DateTimeKind.Utc).AddTicks(3700), new DateTime(2026, 3, 15, 16, 15, 47, 309, DateTimeKind.Utc).AddTicks(3710), "uploads/members/seed/2512180.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 331,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 340, DateTimeKind.Utc).AddTicks(6330), new DateTime(2026, 3, 15, 16, 15, 47, 340, DateTimeKind.Utc).AddTicks(6350), "uploads/members/seed/2512181.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 332,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 348, DateTimeKind.Utc).AddTicks(3880), "uploads/members/seed/2512182.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 333,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 378, DateTimeKind.Utc).AddTicks(4250), new DateTime(2026, 3, 15, 16, 15, 47, 378, DateTimeKind.Utc).AddTicks(4260), "uploads/members/seed/2512183.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 334,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 382, DateTimeKind.Utc).AddTicks(6230), "uploads/members/seed/2512184.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 335,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 415, DateTimeKind.Utc).AddTicks(8020), new DateTime(2026, 3, 15, 16, 15, 47, 415, DateTimeKind.Utc).AddTicks(8030), "uploads/members/seed/2512185.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 336,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 450, DateTimeKind.Utc).AddTicks(4430), "uploads/members/seed/2512187.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 337,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 466, DateTimeKind.Utc).AddTicks(6690), new DateTime(2026, 3, 15, 16, 15, 47, 466, DateTimeKind.Utc).AddTicks(6700), "uploads/members/seed/2512188.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 338,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 476, DateTimeKind.Utc).AddTicks(740), new DateTime(2026, 3, 15, 16, 15, 47, 476, DateTimeKind.Utc).AddTicks(760), "uploads/members/seed/2512189.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 339,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 516, DateTimeKind.Utc).AddTicks(1290), new DateTime(2026, 3, 15, 16, 15, 47, 516, DateTimeKind.Utc).AddTicks(1310), "uploads/members/seed/2512190.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 340,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 553, DateTimeKind.Utc).AddTicks(1450), new DateTime(2026, 3, 15, 16, 15, 47, 553, DateTimeKind.Utc).AddTicks(1460), "uploads/members/seed/2512191.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 341,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 560, DateTimeKind.Utc).AddTicks(7610), new DateTime(2026, 3, 15, 16, 15, 47, 560, DateTimeKind.Utc).AddTicks(7620), "uploads/members/seed/2512192.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 342,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 566, DateTimeKind.Utc).AddTicks(8190), new DateTime(2026, 3, 15, 16, 15, 47, 566, DateTimeKind.Utc).AddTicks(8200), "uploads/members/seed/2512195.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 343,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 582, DateTimeKind.Utc).AddTicks(8090), new DateTime(2026, 3, 15, 16, 15, 47, 582, DateTimeKind.Utc).AddTicks(8100), "uploads/members/seed/2512196.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 344,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 591, DateTimeKind.Utc).AddTicks(9200), "uploads/members/seed/2512197.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 345,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 600, DateTimeKind.Utc).AddTicks(1680), new DateTime(2026, 3, 15, 16, 15, 47, 600, DateTimeKind.Utc).AddTicks(1690), "uploads/members/seed/2512198.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 346,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 606, DateTimeKind.Utc).AddTicks(9440), "uploads/members/seed/2512201.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 347,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 619, DateTimeKind.Utc).AddTicks(290), new DateTime(2026, 3, 15, 16, 15, 47, 619, DateTimeKind.Utc).AddTicks(300), "uploads/members/seed/2512204.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 348,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 654, DateTimeKind.Utc).AddTicks(9220), new DateTime(2026, 3, 15, 16, 15, 47, 654, DateTimeKind.Utc).AddTicks(9230), "uploads/members/seed/2512205.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 349,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 658, DateTimeKind.Utc).AddTicks(7830), "uploads/members/seed/2512208.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 350,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 696, DateTimeKind.Utc).AddTicks(7830), new DateTime(2026, 3, 15, 16, 15, 47, 696, DateTimeKind.Utc).AddTicks(7850), "uploads/members/seed/2512210.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 351,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 823, DateTimeKind.Utc).AddTicks(1010), new DateTime(2026, 3, 15, 16, 15, 47, 823, DateTimeKind.Utc).AddTicks(1020), "uploads/members/seed/2512213.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 352,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 834, DateTimeKind.Utc).AddTicks(9470), new DateTime(2026, 3, 15, 16, 15, 47, 834, DateTimeKind.Utc).AddTicks(9480), "uploads/members/seed/2512214.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 353,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 870, DateTimeKind.Utc).AddTicks(810), new DateTime(2026, 3, 15, 16, 15, 47, 870, DateTimeKind.Utc).AddTicks(820), "uploads/members/seed/2512215.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 354,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 880, DateTimeKind.Utc).AddTicks(6550), new DateTime(2026, 3, 15, 16, 15, 47, 880, DateTimeKind.Utc).AddTicks(6570), "uploads/members/seed/2512216.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 355,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 893, DateTimeKind.Utc).AddTicks(4800), "uploads/members/seed/2512217.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 356,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 906, DateTimeKind.Utc).AddTicks(8650), new DateTime(2026, 3, 15, 16, 15, 47, 906, DateTimeKind.Utc).AddTicks(8670), "uploads/members/seed/2512218.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 357,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 927, DateTimeKind.Utc).AddTicks(2360), new DateTime(2026, 3, 15, 16, 15, 47, 927, DateTimeKind.Utc).AddTicks(2370), "uploads/members/seed/2512219.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 358,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 941, DateTimeKind.Utc).AddTicks(220), new DateTime(2026, 3, 15, 16, 15, 47, 941, DateTimeKind.Utc).AddTicks(230), "uploads/members/seed/2512220.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 359,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 950, DateTimeKind.Utc).AddTicks(4820), new DateTime(2026, 3, 15, 16, 15, 47, 950, DateTimeKind.Utc).AddTicks(4830), "uploads/members/seed/2512221.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 360,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 963, DateTimeKind.Utc).AddTicks(4570), new DateTime(2026, 3, 15, 16, 15, 47, 963, DateTimeKind.Utc).AddTicks(4580), "uploads/members/seed/2512222.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 361,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 968, DateTimeKind.Utc).AddTicks(3940), new DateTime(2026, 3, 15, 16, 15, 47, 968, DateTimeKind.Utc).AddTicks(3950), "uploads/members/seed/2512223.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 362,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 983, DateTimeKind.Utc).AddTicks(4180), new DateTime(2026, 3, 15, 16, 15, 47, 983, DateTimeKind.Utc).AddTicks(4190), "uploads/members/seed/2512224.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 363,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 40, DateTimeKind.Utc).AddTicks(9760), new DateTime(2026, 3, 15, 16, 15, 48, 40, DateTimeKind.Utc).AddTicks(9770), "uploads/members/seed/2512228.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 364,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 48, DateTimeKind.Utc).AddTicks(1780), new DateTime(2026, 3, 15, 16, 15, 48, 48, DateTimeKind.Utc).AddTicks(1790), "uploads/members/seed/2512229.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 365,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 53, DateTimeKind.Utc).AddTicks(6110), new DateTime(2026, 3, 15, 16, 15, 48, 53, DateTimeKind.Utc).AddTicks(6120), "uploads/members/seed/2512230.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 366,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 81, DateTimeKind.Utc).AddTicks(5650), new DateTime(2026, 3, 15, 16, 15, 48, 81, DateTimeKind.Utc).AddTicks(5660), "uploads/members/seed/2512231.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 367,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 93, DateTimeKind.Utc).AddTicks(7540), new DateTime(2026, 3, 15, 16, 15, 48, 93, DateTimeKind.Utc).AddTicks(7550), "uploads/members/seed/2512235.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 368,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 102, DateTimeKind.Utc).AddTicks(5580), "uploads/members/seed/2512236.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 369,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 219, DateTimeKind.Utc).AddTicks(620), "uploads/members/seed/2512237.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 370,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 238, DateTimeKind.Utc).AddTicks(4580), new DateTime(2026, 3, 15, 16, 15, 48, 238, DateTimeKind.Utc).AddTicks(4600), "uploads/members/seed/2512238.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 371,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 254, DateTimeKind.Utc).AddTicks(5630), new DateTime(2026, 3, 15, 16, 15, 48, 254, DateTimeKind.Utc).AddTicks(5640), "uploads/members/seed/2512239.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 372,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 315, DateTimeKind.Utc).AddTicks(2030), new DateTime(2026, 3, 15, 16, 15, 48, 315, DateTimeKind.Utc).AddTicks(2040), "uploads/members/seed/2512240.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 373,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 320, DateTimeKind.Utc).AddTicks(6850), new DateTime(2026, 3, 15, 16, 15, 48, 320, DateTimeKind.Utc).AddTicks(6860), "uploads/members/seed/2512243.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 374,
                columns: new[] { "AppliedDate", "LastUpdateDate" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 321, DateTimeKind.Utc).AddTicks(970), new DateTime(2026, 3, 15, 16, 15, 48, 321, DateTimeKind.Utc).AddTicks(980) });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 375,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 353, DateTimeKind.Utc).AddTicks(1240), new DateTime(2026, 3, 15, 16, 15, 48, 353, DateTimeKind.Utc).AddTicks(1250), "uploads/members/seed/2512246.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 376,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 388, DateTimeKind.Utc).AddTicks(5020), new DateTime(2026, 3, 15, 16, 15, 48, 388, DateTimeKind.Utc).AddTicks(5030), "uploads/members/seed/2512247.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 377,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 396, DateTimeKind.Utc).AddTicks(7930), new DateTime(2026, 3, 15, 16, 15, 48, 396, DateTimeKind.Utc).AddTicks(7940), "uploads/members/seed/2512248.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 378,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 404, DateTimeKind.Utc).AddTicks(2670), new DateTime(2026, 3, 15, 16, 15, 48, 404, DateTimeKind.Utc).AddTicks(2680), "uploads/members/seed/2512249.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 379,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 426, DateTimeKind.Utc).AddTicks(1210), new DateTime(2026, 3, 15, 16, 15, 48, 426, DateTimeKind.Utc).AddTicks(1220), "uploads/members/seed/2512250.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 380,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 430, DateTimeKind.Utc).AddTicks(4860), "uploads/members/seed/2512251.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 381,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 440, DateTimeKind.Utc).AddTicks(3470), new DateTime(2026, 3, 15, 16, 15, 48, 440, DateTimeKind.Utc).AddTicks(3480), "uploads/members/seed/2512252.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 382,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 450, DateTimeKind.Utc).AddTicks(4280), new DateTime(2026, 3, 15, 16, 15, 48, 450, DateTimeKind.Utc).AddTicks(4290), "uploads/members/seed/2512253.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 383,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 460, DateTimeKind.Utc).AddTicks(5410), new DateTime(2026, 3, 15, 16, 15, 48, 460, DateTimeKind.Utc).AddTicks(5420), "uploads/members/seed/2512254.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 384,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 471, DateTimeKind.Utc).AddTicks(2850), new DateTime(2026, 3, 15, 16, 15, 48, 471, DateTimeKind.Utc).AddTicks(2860), "uploads/members/seed/2512255.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 385,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 502, DateTimeKind.Utc).AddTicks(260), new DateTime(2026, 3, 15, 16, 15, 48, 502, DateTimeKind.Utc).AddTicks(270), "uploads/members/seed/2512256.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 386,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 506, DateTimeKind.Utc).AddTicks(8450), new DateTime(2026, 3, 15, 16, 15, 48, 506, DateTimeKind.Utc).AddTicks(8470), "uploads/members/seed/2512257.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 387,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 514, DateTimeKind.Utc).AddTicks(2450), new DateTime(2026, 3, 15, 16, 15, 48, 514, DateTimeKind.Utc).AddTicks(2460), "uploads/members/seed/2512258.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 388,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 526, DateTimeKind.Utc).AddTicks(2780), new DateTime(2026, 3, 15, 16, 15, 48, 526, DateTimeKind.Utc).AddTicks(2790), "uploads/members/seed/2512259.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 389,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 544, DateTimeKind.Utc).AddTicks(2740), "uploads/members/seed/2512260.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 390,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 584, DateTimeKind.Utc).AddTicks(9670), new DateTime(2026, 3, 15, 16, 15, 48, 584, DateTimeKind.Utc).AddTicks(9670), "uploads/members/seed/2512261.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 391,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 592, DateTimeKind.Utc).AddTicks(1200), new DateTime(2026, 3, 15, 16, 15, 48, 592, DateTimeKind.Utc).AddTicks(1210), "uploads/members/seed/2512262.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 392,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 618, DateTimeKind.Utc).AddTicks(9620), "uploads/members/seed/2512263.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 393,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 638, DateTimeKind.Utc).AddTicks(8840), new DateTime(2026, 3, 15, 16, 15, 48, 638, DateTimeKind.Utc).AddTicks(8850), "uploads/members/seed/2512264.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 394,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 645, DateTimeKind.Utc).AddTicks(2600), new DateTime(2026, 3, 15, 16, 15, 48, 645, DateTimeKind.Utc).AddTicks(2610), "uploads/members/seed/2512265.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 395,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 653, DateTimeKind.Utc).AddTicks(7650), new DateTime(2026, 3, 15, 16, 15, 48, 653, DateTimeKind.Utc).AddTicks(7660), "uploads/members/seed/2512266.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 396,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 663, DateTimeKind.Utc).AddTicks(7510), new DateTime(2026, 3, 15, 16, 15, 48, 663, DateTimeKind.Utc).AddTicks(7520), "uploads/members/seed/2512267.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 397,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 677, DateTimeKind.Utc).AddTicks(1120), new DateTime(2026, 3, 15, 16, 15, 48, 677, DateTimeKind.Utc).AddTicks(1130), "uploads/members/seed/2512268.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 398,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 681, DateTimeKind.Utc).AddTicks(6760), new DateTime(2026, 3, 15, 16, 15, 48, 681, DateTimeKind.Utc).AddTicks(6770), "uploads/members/seed/2512269.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 399,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 689, DateTimeKind.Utc).AddTicks(2620), new DateTime(2026, 3, 15, 16, 15, 48, 689, DateTimeKind.Utc).AddTicks(2630), "uploads/members/seed/2512272.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 697, DateTimeKind.Utc).AddTicks(7510), new DateTime(2026, 3, 15, 16, 15, 48, 697, DateTimeKind.Utc).AddTicks(7520), "uploads/members/seed/2512273.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 705, DateTimeKind.Utc).AddTicks(6280), new DateTime(2026, 3, 15, 16, 15, 48, 705, DateTimeKind.Utc).AddTicks(6290), "uploads/members/seed/2512275.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 716, DateTimeKind.Utc).AddTicks(7410), new DateTime(2026, 3, 15, 16, 15, 48, 716, DateTimeKind.Utc).AddTicks(7410), "uploads/members/seed/2512278.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 729, DateTimeKind.Utc).AddTicks(2980), new DateTime(2026, 3, 15, 16, 15, 48, 729, DateTimeKind.Utc).AddTicks(3000), "uploads/members/seed/2512279.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 743, DateTimeKind.Utc).AddTicks(5690), "uploads/members/seed/2512280.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 783, DateTimeKind.Utc).AddTicks(7790), "uploads/members/seed/2512281.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 813, DateTimeKind.Utc).AddTicks(1330), new DateTime(2026, 3, 15, 16, 15, 48, 813, DateTimeKind.Utc).AddTicks(1330), "uploads/members/seed/2512283.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 407,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 853, DateTimeKind.Utc).AddTicks(2570), new DateTime(2026, 3, 15, 16, 15, 48, 853, DateTimeKind.Utc).AddTicks(2580), "uploads/members/seed/2512284.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 408,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 860, DateTimeKind.Utc).AddTicks(5380), new DateTime(2026, 3, 15, 16, 15, 48, 860, DateTimeKind.Utc).AddTicks(5390), "uploads/members/seed/2512285.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 409,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 872, DateTimeKind.Utc).AddTicks(4930), new DateTime(2026, 3, 15, 16, 15, 48, 872, DateTimeKind.Utc).AddTicks(4940), "uploads/members/seed/2512286.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 410,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 883, DateTimeKind.Utc).AddTicks(8740), "uploads/members/seed/2512287.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 411,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 916, DateTimeKind.Utc).AddTicks(6030), new DateTime(2026, 3, 15, 16, 15, 48, 916, DateTimeKind.Utc).AddTicks(6050), "uploads/members/seed/2512288.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 412,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 6, DateTimeKind.Utc).AddTicks(7580), new DateTime(2026, 3, 15, 16, 15, 49, 6, DateTimeKind.Utc).AddTicks(7580), "uploads/members/seed/2512290.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 413,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 14, DateTimeKind.Utc).AddTicks(300), "uploads/members/seed/2512291.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 414,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 37, DateTimeKind.Utc).AddTicks(9840), new DateTime(2026, 3, 15, 16, 15, 49, 37, DateTimeKind.Utc).AddTicks(9850), "uploads/members/seed/2512292.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 415,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 61, DateTimeKind.Utc).AddTicks(7040), new DateTime(2026, 3, 15, 16, 15, 49, 61, DateTimeKind.Utc).AddTicks(7060), "uploads/members/seed/2512293.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 416,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 83, DateTimeKind.Utc).AddTicks(2790), new DateTime(2026, 3, 15, 16, 15, 49, 83, DateTimeKind.Utc).AddTicks(2800), "uploads/members/seed/2512294.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 417,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 94, DateTimeKind.Utc).AddTicks(4180), new DateTime(2026, 3, 15, 16, 15, 49, 94, DateTimeKind.Utc).AddTicks(4190), "uploads/members/seed/2512295.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 418,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 121, DateTimeKind.Utc).AddTicks(5610), "uploads/members/seed/2512296.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 419,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 188, DateTimeKind.Utc).AddTicks(7280), "uploads/members/seed/2512298.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 420,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 214, DateTimeKind.Utc).AddTicks(2200), new DateTime(2026, 3, 15, 16, 15, 49, 214, DateTimeKind.Utc).AddTicks(2210), "uploads/members/seed/2512299.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 421,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 224, DateTimeKind.Utc).AddTicks(4000), new DateTime(2026, 3, 15, 16, 15, 49, 224, DateTimeKind.Utc).AddTicks(4010), "uploads/members/seed/2512300.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 422,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 233, DateTimeKind.Utc).AddTicks(5020), new DateTime(2026, 3, 15, 16, 15, 49, 233, DateTimeKind.Utc).AddTicks(5030), "uploads/members/seed/2512301.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 423,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 255, DateTimeKind.Utc).AddTicks(5070), new DateTime(2026, 3, 15, 16, 15, 49, 255, DateTimeKind.Utc).AddTicks(5080), "uploads/members/seed/2512302.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 424,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 277, DateTimeKind.Utc).AddTicks(9630), new DateTime(2026, 3, 15, 16, 15, 49, 277, DateTimeKind.Utc).AddTicks(9640), "uploads/members/seed/2512303.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 425,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 295, DateTimeKind.Utc).AddTicks(7000), new DateTime(2026, 3, 15, 16, 15, 49, 295, DateTimeKind.Utc).AddTicks(7000), "uploads/members/seed/2512305.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 426,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 306, DateTimeKind.Utc).AddTicks(9510), new DateTime(2026, 3, 15, 16, 15, 49, 306, DateTimeKind.Utc).AddTicks(9520), "uploads/members/seed/2512306.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 427,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 320, DateTimeKind.Utc).AddTicks(8010), new DateTime(2026, 3, 15, 16, 15, 49, 320, DateTimeKind.Utc).AddTicks(8020), "uploads/members/seed/2512308.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 428,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 360, DateTimeKind.Utc).AddTicks(7270), new DateTime(2026, 3, 15, 16, 15, 49, 360, DateTimeKind.Utc).AddTicks(7280), "uploads/members/seed/2512309.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 429,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 436, DateTimeKind.Utc).AddTicks(8190), new DateTime(2026, 3, 15, 16, 15, 49, 436, DateTimeKind.Utc).AddTicks(8200), "uploads/members/seed/2512311.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 430,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 486, DateTimeKind.Utc).AddTicks(4300), new DateTime(2026, 3, 15, 16, 15, 49, 486, DateTimeKind.Utc).AddTicks(4310), "uploads/members/seed/2512312.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 431,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 501, DateTimeKind.Utc).AddTicks(8220), new DateTime(2026, 3, 15, 16, 15, 49, 501, DateTimeKind.Utc).AddTicks(8230), "uploads/members/seed/2512313.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 432,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 527, DateTimeKind.Utc).AddTicks(7200), "uploads/members/seed/2512314.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 433,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 540, DateTimeKind.Utc).AddTicks(220), new DateTime(2026, 3, 15, 16, 15, 49, 540, DateTimeKind.Utc).AddTicks(230), "uploads/members/seed/2512315.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 434,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 579, DateTimeKind.Utc).AddTicks(8950), new DateTime(2026, 3, 15, 16, 15, 49, 579, DateTimeKind.Utc).AddTicks(8960), "uploads/members/seed/2512316.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 435,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 625, DateTimeKind.Utc).AddTicks(7680), new DateTime(2026, 3, 15, 16, 15, 49, 625, DateTimeKind.Utc).AddTicks(7690), "uploads/members/seed/2512319.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 436,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 657, DateTimeKind.Utc).AddTicks(6370), new DateTime(2026, 3, 15, 16, 15, 49, 657, DateTimeKind.Utc).AddTicks(6380), "uploads/members/seed/2512320.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 437,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 674, DateTimeKind.Utc).AddTicks(6120), new DateTime(2026, 3, 15, 16, 15, 49, 674, DateTimeKind.Utc).AddTicks(6130), "uploads/members/seed/2512323.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 438,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 687, DateTimeKind.Utc).AddTicks(4770), new DateTime(2026, 3, 15, 16, 15, 49, 687, DateTimeKind.Utc).AddTicks(4780), "uploads/members/seed/2512324.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 439,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 697, DateTimeKind.Utc).AddTicks(9170), new DateTime(2026, 3, 15, 16, 15, 49, 697, DateTimeKind.Utc).AddTicks(9180), "uploads/members/seed/2512325.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 440,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 748, DateTimeKind.Utc).AddTicks(4200), new DateTime(2026, 3, 15, 16, 15, 49, 748, DateTimeKind.Utc).AddTicks(4210), "uploads/members/seed/2512326.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 441,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 767, DateTimeKind.Utc).AddTicks(8840), new DateTime(2026, 3, 15, 16, 15, 49, 767, DateTimeKind.Utc).AddTicks(8850), "uploads/members/seed/2512333.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 442,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 784, DateTimeKind.Utc).AddTicks(1470), "uploads/members/seed/2512334.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 443,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 821, DateTimeKind.Utc).AddTicks(4600), new DateTime(2026, 3, 15, 16, 15, 49, 821, DateTimeKind.Utc).AddTicks(4610), "uploads/members/seed/2512336.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 444,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 826, DateTimeKind.Utc).AddTicks(6760), new DateTime(2026, 3, 15, 16, 15, 49, 826, DateTimeKind.Utc).AddTicks(6770), "uploads/members/seed/2512337.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 445,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 831, DateTimeKind.Utc).AddTicks(2600), "uploads/members/seed/2512338.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 446,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 844, DateTimeKind.Utc).AddTicks(6090), new DateTime(2026, 3, 15, 16, 15, 49, 844, DateTimeKind.Utc).AddTicks(6100), "uploads/members/seed/2512339.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 447,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 901, DateTimeKind.Utc).AddTicks(2890), new DateTime(2026, 3, 15, 16, 15, 49, 901, DateTimeKind.Utc).AddTicks(2900), "uploads/members/seed/2512340.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 448,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 905, DateTimeKind.Utc).AddTicks(6690), new DateTime(2026, 3, 15, 16, 15, 49, 905, DateTimeKind.Utc).AddTicks(6700), "uploads/members/seed/2512341.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 449,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 951, DateTimeKind.Utc).AddTicks(3410), new DateTime(2026, 3, 15, 16, 15, 49, 951, DateTimeKind.Utc).AddTicks(3410), "uploads/members/seed/2512342.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 450,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 956, DateTimeKind.Utc).AddTicks(1120), "uploads/members/seed/2512344.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 451,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 979, DateTimeKind.Utc).AddTicks(5060), new DateTime(2026, 3, 15, 16, 15, 49, 979, DateTimeKind.Utc).AddTicks(5080), "uploads/members/seed/2512345.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 452,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 15, DateTimeKind.Utc).AddTicks(1980), new DateTime(2026, 3, 15, 16, 15, 50, 15, DateTimeKind.Utc).AddTicks(1990), "uploads/members/seed/2512346.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 453,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 26, DateTimeKind.Utc).AddTicks(590), new DateTime(2026, 3, 15, 16, 15, 50, 26, DateTimeKind.Utc).AddTicks(600), "uploads/members/seed/2512347.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 454,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 67, DateTimeKind.Utc).AddTicks(7900), new DateTime(2026, 3, 15, 16, 15, 50, 67, DateTimeKind.Utc).AddTicks(7920), "uploads/members/seed/2512348.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 455,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 86, DateTimeKind.Utc).AddTicks(5800), "uploads/members/seed/2512350.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 456,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 97, DateTimeKind.Utc).AddTicks(480), new DateTime(2026, 3, 15, 16, 15, 50, 97, DateTimeKind.Utc).AddTicks(490), "uploads/members/seed/2512351.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 457,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 175, DateTimeKind.Utc).AddTicks(2100), new DateTime(2026, 3, 15, 16, 15, 50, 175, DateTimeKind.Utc).AddTicks(2100), "uploads/members/seed/2512352.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 458,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 232, DateTimeKind.Utc).AddTicks(4020), "uploads/members/seed/2512353.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 459,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 305, DateTimeKind.Utc).AddTicks(7030), new DateTime(2026, 3, 15, 16, 15, 50, 305, DateTimeKind.Utc).AddTicks(7040), "uploads/members/seed/2512354.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 460,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 428, DateTimeKind.Utc).AddTicks(780), new DateTime(2026, 3, 15, 16, 15, 50, 428, DateTimeKind.Utc).AddTicks(790), "uploads/members/seed/2512355.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 461,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 438, DateTimeKind.Utc).AddTicks(5790), new DateTime(2026, 3, 15, 16, 15, 50, 438, DateTimeKind.Utc).AddTicks(5800), "uploads/members/seed/2512357.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 462,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 442, DateTimeKind.Utc).AddTicks(1870), new DateTime(2026, 3, 15, 16, 15, 50, 442, DateTimeKind.Utc).AddTicks(1880), "uploads/members/seed/2512362.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 463,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 448, DateTimeKind.Utc).AddTicks(6820), "uploads/members/seed/2512363.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 464,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 480, DateTimeKind.Utc).AddTicks(6900), new DateTime(2026, 3, 15, 16, 15, 50, 480, DateTimeKind.Utc).AddTicks(6910), "uploads/members/seed/2512364.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 465,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 495, DateTimeKind.Utc).AddTicks(5470), new DateTime(2026, 3, 15, 16, 15, 50, 495, DateTimeKind.Utc).AddTicks(5480), "uploads/members/seed/2512365.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 466,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 552, DateTimeKind.Utc).AddTicks(8810), new DateTime(2026, 3, 15, 16, 15, 50, 552, DateTimeKind.Utc).AddTicks(8820), "uploads/members/seed/2512366.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 467,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 566, DateTimeKind.Utc).AddTicks(4010), new DateTime(2026, 3, 15, 16, 15, 50, 566, DateTimeKind.Utc).AddTicks(4020), "uploads/members/seed/2512367.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 468,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 579, DateTimeKind.Utc).AddTicks(5850), new DateTime(2026, 3, 15, 16, 15, 50, 579, DateTimeKind.Utc).AddTicks(5860), "uploads/members/seed/2512368.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 469,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 594, DateTimeKind.Utc).AddTicks(8490), new DateTime(2026, 3, 15, 16, 15, 50, 594, DateTimeKind.Utc).AddTicks(8500), "uploads/members/seed/2512369.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 470,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 611, DateTimeKind.Utc).AddTicks(7750), new DateTime(2026, 3, 15, 16, 15, 50, 611, DateTimeKind.Utc).AddTicks(7760), "uploads/members/seed/2512370.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 471,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 688, DateTimeKind.Utc).AddTicks(4250), new DateTime(2026, 3, 15, 16, 15, 50, 688, DateTimeKind.Utc).AddTicks(4260), "uploads/members/seed/2512371.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 472,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 784, DateTimeKind.Utc).AddTicks(9260), "uploads/members/seed/2512372.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 473,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 799, DateTimeKind.Utc).AddTicks(6710), new DateTime(2026, 3, 15, 16, 15, 50, 799, DateTimeKind.Utc).AddTicks(6720), "uploads/members/seed/2512373.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 474,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 824, DateTimeKind.Utc).AddTicks(6650), "uploads/members/seed/2512374.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 475,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 836, DateTimeKind.Utc).AddTicks(2980), new DateTime(2026, 3, 15, 16, 15, 50, 836, DateTimeKind.Utc).AddTicks(2990), "uploads/members/seed/2512376.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 476,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 913, DateTimeKind.Utc).AddTicks(5280), new DateTime(2026, 3, 15, 16, 15, 50, 913, DateTimeKind.Utc).AddTicks(5290), "uploads/members/seed/2512377.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 477,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 929, DateTimeKind.Utc).AddTicks(3290), new DateTime(2026, 3, 15, 16, 15, 50, 929, DateTimeKind.Utc).AddTicks(3300), "uploads/members/seed/2512378.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 478,
                column: "PhotoPath",
                value: "uploads/members/seed/2512379.jpg");

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 479,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 966, DateTimeKind.Utc).AddTicks(6390), new DateTime(2026, 3, 15, 16, 15, 50, 966, DateTimeKind.Utc).AddTicks(6400), "uploads/members/seed/2512380.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 480,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 39, DateTimeKind.Utc).AddTicks(110), new DateTime(2026, 3, 15, 16, 15, 51, 39, DateTimeKind.Utc).AddTicks(120), "uploads/members/seed/2512381.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 481,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 60, DateTimeKind.Utc).AddTicks(6260), new DateTime(2026, 3, 15, 16, 15, 51, 60, DateTimeKind.Utc).AddTicks(6270), "uploads/members/seed/2512383.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 482,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 122, DateTimeKind.Utc).AddTicks(1340), "uploads/members/seed/2512384.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 483,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 152, DateTimeKind.Utc).AddTicks(1060), new DateTime(2026, 3, 15, 16, 15, 51, 152, DateTimeKind.Utc).AddTicks(1070), "uploads/members/seed/2512385.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 484,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 165, DateTimeKind.Utc).AddTicks(1100), new DateTime(2026, 3, 15, 16, 15, 51, 165, DateTimeKind.Utc).AddTicks(1110), "uploads/members/seed/2512386.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 485,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 172, DateTimeKind.Utc).AddTicks(8510), new DateTime(2026, 3, 15, 16, 15, 51, 172, DateTimeKind.Utc).AddTicks(8520), "uploads/members/seed/2512387.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 486,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 181, DateTimeKind.Utc).AddTicks(3330), new DateTime(2026, 3, 15, 16, 15, 51, 181, DateTimeKind.Utc).AddTicks(3340), "uploads/members/seed/2512388.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 487,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 189, DateTimeKind.Utc).AddTicks(5370), new DateTime(2026, 3, 15, 16, 15, 51, 189, DateTimeKind.Utc).AddTicks(5380), "uploads/members/seed/2512389.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 488,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 210, DateTimeKind.Utc).AddTicks(8050), "uploads/members/seed/2512390.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 489,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 231, DateTimeKind.Utc).AddTicks(90), new DateTime(2026, 3, 15, 16, 15, 51, 231, DateTimeKind.Utc).AddTicks(100), "uploads/members/seed/2512391.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 490,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 236, DateTimeKind.Utc).AddTicks(5000), new DateTime(2026, 3, 15, 16, 15, 51, 236, DateTimeKind.Utc).AddTicks(5010), "uploads/members/seed/2512392.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 491,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 245, DateTimeKind.Utc).AddTicks(300), new DateTime(2026, 3, 15, 16, 15, 51, 245, DateTimeKind.Utc).AddTicks(310), "uploads/members/seed/2512393.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 492,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 291, DateTimeKind.Utc).AddTicks(3680), "uploads/members/seed/2512394.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 493,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 301, DateTimeKind.Utc).AddTicks(600), new DateTime(2026, 3, 15, 16, 15, 51, 301, DateTimeKind.Utc).AddTicks(610), "uploads/members/seed/2512395.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 494,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 304, DateTimeKind.Utc).AddTicks(8810), new DateTime(2026, 3, 15, 16, 15, 51, 304, DateTimeKind.Utc).AddTicks(8820), "uploads/members/seed/2512396.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 495,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 333, DateTimeKind.Utc).AddTicks(8780), new DateTime(2026, 3, 15, 16, 15, 51, 333, DateTimeKind.Utc).AddTicks(8790), "uploads/members/seed/2512397.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 496,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 343, DateTimeKind.Utc).AddTicks(8290), new DateTime(2026, 3, 15, 16, 15, 51, 343, DateTimeKind.Utc).AddTicks(8300), "uploads/members/seed/2512398.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 497,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 350, DateTimeKind.Utc).AddTicks(7140), new DateTime(2026, 3, 15, 16, 15, 51, 350, DateTimeKind.Utc).AddTicks(7150), "uploads/members/seed/2512400.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 498,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 378, DateTimeKind.Utc).AddTicks(4740), new DateTime(2026, 3, 15, 16, 15, 51, 378, DateTimeKind.Utc).AddTicks(4750), "uploads/members/seed/2512401.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 499,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 409, DateTimeKind.Utc).AddTicks(7320), new DateTime(2026, 3, 15, 16, 15, 51, 409, DateTimeKind.Utc).AddTicks(7330), "uploads/members/seed/2512402.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 416, DateTimeKind.Utc).AddTicks(1300), new DateTime(2026, 3, 15, 16, 15, 51, 416, DateTimeKind.Utc).AddTicks(1320), "uploads/members/seed/2512403.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 461, DateTimeKind.Utc).AddTicks(2320), "uploads/members/seed/2512407.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 479, DateTimeKind.Utc).AddTicks(6940), new DateTime(2026, 3, 15, 16, 15, 51, 479, DateTimeKind.Utc).AddTicks(6950), "uploads/members/seed/2512408.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 494, DateTimeKind.Utc).AddTicks(2260), new DateTime(2026, 3, 15, 16, 15, 51, 494, DateTimeKind.Utc).AddTicks(2270), "uploads/members/seed/2512409.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 505, DateTimeKind.Utc).AddTicks(2770), new DateTime(2026, 3, 15, 16, 15, 51, 505, DateTimeKind.Utc).AddTicks(2780), "uploads/members/seed/2512413.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 516, DateTimeKind.Utc).AddTicks(2530), new DateTime(2026, 3, 15, 16, 15, 51, 516, DateTimeKind.Utc).AddTicks(2540), "uploads/members/seed/2512414.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 542, DateTimeKind.Utc).AddTicks(5970), new DateTime(2026, 3, 15, 16, 15, 51, 542, DateTimeKind.Utc).AddTicks(5980), "uploads/members/seed/2512415.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 651, DateTimeKind.Utc).AddTicks(2350), new DateTime(2026, 3, 15, 16, 15, 51, 651, DateTimeKind.Utc).AddTicks(2360), "uploads/members/seed/2512416.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 508,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 696, DateTimeKind.Utc).AddTicks(4590), new DateTime(2026, 3, 15, 16, 15, 51, 696, DateTimeKind.Utc).AddTicks(4590), "uploads/members/seed/2512418.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 509,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 720, DateTimeKind.Utc).AddTicks(900), new DateTime(2026, 3, 15, 16, 15, 51, 720, DateTimeKind.Utc).AddTicks(910), "uploads/members/seed/2512419.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 510,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 727, DateTimeKind.Utc).AddTicks(9570), new DateTime(2026, 3, 15, 16, 15, 51, 727, DateTimeKind.Utc).AddTicks(9580), "uploads/members/seed/2512420.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 511,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 741, DateTimeKind.Utc).AddTicks(5920), "uploads/members/seed/2512421.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 512,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 758, DateTimeKind.Utc).AddTicks(8900), "uploads/members/seed/2512422.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 513,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 764, DateTimeKind.Utc).AddTicks(9770), new DateTime(2026, 3, 15, 16, 15, 51, 764, DateTimeKind.Utc).AddTicks(9780), "uploads/members/seed/2512423.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 514,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 770, DateTimeKind.Utc).AddTicks(2430), new DateTime(2026, 3, 15, 16, 15, 51, 770, DateTimeKind.Utc).AddTicks(2440), "uploads/members/seed/2512424.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 515,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 774, DateTimeKind.Utc).AddTicks(900), new DateTime(2026, 3, 15, 16, 15, 51, 774, DateTimeKind.Utc).AddTicks(910), "uploads/members/seed/2512425.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 516,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 802, DateTimeKind.Utc).AddTicks(8150), "uploads/members/seed/2512426.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 517,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 832, DateTimeKind.Utc).AddTicks(8580), new DateTime(2026, 3, 15, 16, 15, 51, 832, DateTimeKind.Utc).AddTicks(8590), "uploads/members/seed/2512427.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 518,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 837, DateTimeKind.Utc).AddTicks(6510), new DateTime(2026, 3, 15, 16, 15, 51, 837, DateTimeKind.Utc).AddTicks(6520), "uploads/members/seed/2512428.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 519,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 843, DateTimeKind.Utc).AddTicks(6060), new DateTime(2026, 3, 15, 16, 15, 51, 843, DateTimeKind.Utc).AddTicks(6070), "uploads/members/seed/2512429.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 520,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 869, DateTimeKind.Utc).AddTicks(3420), new DateTime(2026, 3, 15, 16, 15, 51, 869, DateTimeKind.Utc).AddTicks(3430), "uploads/members/seed/2512430.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 521,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 876, DateTimeKind.Utc).AddTicks(3170), new DateTime(2026, 3, 15, 16, 15, 51, 876, DateTimeKind.Utc).AddTicks(3180), "uploads/members/seed/2512431.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 522,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 895, DateTimeKind.Utc).AddTicks(600), new DateTime(2026, 3, 15, 16, 15, 51, 895, DateTimeKind.Utc).AddTicks(610), "uploads/members/seed/2512432.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 523,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 922, DateTimeKind.Utc).AddTicks(6650), new DateTime(2026, 3, 15, 16, 15, 51, 922, DateTimeKind.Utc).AddTicks(6660), "uploads/members/seed/2512433.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 524,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 949, DateTimeKind.Utc).AddTicks(8160), new DateTime(2026, 3, 15, 16, 15, 51, 949, DateTimeKind.Utc).AddTicks(8230), "uploads/members/seed/2512434.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 525,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 22, DateTimeKind.Utc).AddTicks(340), new DateTime(2026, 3, 15, 16, 15, 52, 22, DateTimeKind.Utc).AddTicks(350), "uploads/members/seed/2512435.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 526,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 58, DateTimeKind.Utc).AddTicks(8790), new DateTime(2026, 3, 15, 16, 15, 52, 58, DateTimeKind.Utc).AddTicks(8800), "uploads/members/seed/2512436.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 527,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 66, DateTimeKind.Utc).AddTicks(6120), new DateTime(2026, 3, 15, 16, 15, 52, 66, DateTimeKind.Utc).AddTicks(6130), "uploads/members/seed/2512438.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 528,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 89, DateTimeKind.Utc).AddTicks(5870), new DateTime(2026, 3, 15, 16, 15, 52, 89, DateTimeKind.Utc).AddTicks(5880), "uploads/members/seed/2512439.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 529,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 95, DateTimeKind.Utc).AddTicks(9580), new DateTime(2026, 3, 15, 16, 15, 52, 95, DateTimeKind.Utc).AddTicks(9590), "uploads/members/seed/2512440.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 530,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 122, DateTimeKind.Utc).AddTicks(7540), new DateTime(2026, 3, 15, 16, 15, 52, 122, DateTimeKind.Utc).AddTicks(7550), "uploads/members/seed/2512441.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 531,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 139, DateTimeKind.Utc).AddTicks(1250), new DateTime(2026, 3, 15, 16, 15, 52, 139, DateTimeKind.Utc).AddTicks(1260), "uploads/members/seed/2512442.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 532,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 142, DateTimeKind.Utc).AddTicks(920), new DateTime(2026, 3, 15, 16, 15, 52, 142, DateTimeKind.Utc).AddTicks(930), "uploads/members/seed/2512443.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 533,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 175, DateTimeKind.Utc).AddTicks(4560), new DateTime(2026, 3, 15, 16, 15, 52, 175, DateTimeKind.Utc).AddTicks(4570), "uploads/members/seed/2512444.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 534,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 183, DateTimeKind.Utc).AddTicks(8190), new DateTime(2026, 3, 15, 16, 15, 52, 183, DateTimeKind.Utc).AddTicks(8200), "uploads/members/seed/2512446.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 535,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 191, DateTimeKind.Utc).AddTicks(7540), "uploads/members/seed/2512447.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 536,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 199, DateTimeKind.Utc).AddTicks(3340), new DateTime(2026, 3, 15, 16, 15, 52, 199, DateTimeKind.Utc).AddTicks(3350), "uploads/members/seed/2512448.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 537,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 231, DateTimeKind.Utc).AddTicks(8240), new DateTime(2026, 3, 15, 16, 15, 52, 231, DateTimeKind.Utc).AddTicks(8250), "uploads/members/seed/2512449.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 538,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 284, DateTimeKind.Utc).AddTicks(5990), new DateTime(2026, 3, 15, 16, 15, 52, 284, DateTimeKind.Utc).AddTicks(6000), "uploads/members/seed/2512450.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 539,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 294, DateTimeKind.Utc).AddTicks(4880), new DateTime(2026, 3, 15, 16, 15, 52, 294, DateTimeKind.Utc).AddTicks(4890), "uploads/members/seed/2512452.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 540,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 298, DateTimeKind.Utc).AddTicks(4910), new DateTime(2026, 3, 15, 16, 15, 52, 298, DateTimeKind.Utc).AddTicks(4920), "uploads/members/seed/2512454.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 541,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 339, DateTimeKind.Utc).AddTicks(4510), new DateTime(2026, 3, 15, 16, 15, 52, 339, DateTimeKind.Utc).AddTicks(4520), "uploads/members/seed/2512455.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 542,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 348, DateTimeKind.Utc).AddTicks(7710), new DateTime(2026, 3, 15, 16, 15, 52, 348, DateTimeKind.Utc).AddTicks(7710), "uploads/members/seed/2512456.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 543,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 353, DateTimeKind.Utc).AddTicks(3250), new DateTime(2026, 3, 15, 16, 15, 52, 353, DateTimeKind.Utc).AddTicks(3250), "uploads/members/seed/2512457.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 544,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 364, DateTimeKind.Utc).AddTicks(5650), new DateTime(2026, 3, 15, 16, 15, 52, 364, DateTimeKind.Utc).AddTicks(5660), "uploads/members/seed/2512458.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 545,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 375, DateTimeKind.Utc).AddTicks(80), new DateTime(2026, 3, 15, 16, 15, 52, 375, DateTimeKind.Utc).AddTicks(90), "uploads/members/seed/2512459.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 546,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 391, DateTimeKind.Utc).AddTicks(3790), new DateTime(2026, 3, 15, 16, 15, 52, 391, DateTimeKind.Utc).AddTicks(3800), "uploads/members/seed/2512461.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 547,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 399, DateTimeKind.Utc).AddTicks(1740), new DateTime(2026, 3, 15, 16, 15, 52, 399, DateTimeKind.Utc).AddTicks(1750), "uploads/members/seed/2512462.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 548,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 405, DateTimeKind.Utc).AddTicks(1510), new DateTime(2026, 3, 15, 16, 15, 52, 405, DateTimeKind.Utc).AddTicks(1520), "uploads/members/seed/2512463.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 549,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 431, DateTimeKind.Utc).AddTicks(8390), new DateTime(2026, 3, 15, 16, 15, 52, 431, DateTimeKind.Utc).AddTicks(8410), "uploads/members/seed/2512465.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 550,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 450, DateTimeKind.Utc).AddTicks(3570), new DateTime(2026, 3, 15, 16, 15, 52, 450, DateTimeKind.Utc).AddTicks(3580), "uploads/members/seed/2512466.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 551,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 462, DateTimeKind.Utc).AddTicks(890), new DateTime(2026, 3, 15, 16, 15, 52, 462, DateTimeKind.Utc).AddTicks(900), "uploads/members/seed/2512467.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 552,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 502, DateTimeKind.Utc).AddTicks(1940), new DateTime(2026, 3, 15, 16, 15, 52, 502, DateTimeKind.Utc).AddTicks(1960), "uploads/members/seed/2512468.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 553,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 549, DateTimeKind.Utc).AddTicks(8250), new DateTime(2026, 3, 15, 16, 15, 52, 549, DateTimeKind.Utc).AddTicks(8260), "uploads/members/seed/2512469.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 554,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 560, DateTimeKind.Utc).AddTicks(7190), "uploads/members/seed/2512471.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 555,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 586, DateTimeKind.Utc).AddTicks(950), new DateTime(2026, 3, 15, 16, 15, 52, 586, DateTimeKind.Utc).AddTicks(950), "uploads/members/seed/2512472.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 556,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 619, DateTimeKind.Utc).AddTicks(6440), new DateTime(2026, 3, 15, 16, 15, 52, 619, DateTimeKind.Utc).AddTicks(6450), "uploads/members/seed/2512473.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 557,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 640, DateTimeKind.Utc).AddTicks(3790), new DateTime(2026, 3, 15, 16, 15, 52, 640, DateTimeKind.Utc).AddTicks(3800), "uploads/members/seed/2512474.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 558,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 649, DateTimeKind.Utc).AddTicks(8070), new DateTime(2026, 3, 15, 16, 15, 52, 649, DateTimeKind.Utc).AddTicks(8080), "uploads/members/seed/2512475.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 559,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 674, DateTimeKind.Utc).AddTicks(8850), new DateTime(2026, 3, 15, 16, 15, 52, 674, DateTimeKind.Utc).AddTicks(8860), "uploads/members/seed/2512476.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 560,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 728, DateTimeKind.Utc).AddTicks(7780), new DateTime(2026, 3, 15, 16, 15, 52, 728, DateTimeKind.Utc).AddTicks(7790), "uploads/members/seed/2512477.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 561,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 736, DateTimeKind.Utc).AddTicks(1650), new DateTime(2026, 3, 15, 16, 15, 52, 736, DateTimeKind.Utc).AddTicks(1650), "uploads/members/seed/2512478.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 562,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 745, DateTimeKind.Utc).AddTicks(7330), new DateTime(2026, 3, 15, 16, 15, 52, 745, DateTimeKind.Utc).AddTicks(7350), "uploads/members/seed/2512479.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 563,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 753, DateTimeKind.Utc).AddTicks(3510), new DateTime(2026, 3, 15, 16, 15, 52, 753, DateTimeKind.Utc).AddTicks(3520), "uploads/members/seed/2512480.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 564,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 767, DateTimeKind.Utc).AddTicks(8340), new DateTime(2026, 3, 15, 16, 15, 52, 767, DateTimeKind.Utc).AddTicks(8350), "uploads/members/seed/2512481.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 565,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 802, DateTimeKind.Utc).AddTicks(1990), new DateTime(2026, 3, 15, 16, 15, 52, 802, DateTimeKind.Utc).AddTicks(2000), "uploads/members/seed/2512482.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 566,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 819, DateTimeKind.Utc).AddTicks(6390), new DateTime(2026, 3, 15, 16, 15, 52, 819, DateTimeKind.Utc).AddTicks(6400), "uploads/members/seed/2512484.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 567,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 832, DateTimeKind.Utc).AddTicks(2220), new DateTime(2026, 3, 15, 16, 15, 52, 832, DateTimeKind.Utc).AddTicks(2230), "uploads/members/seed/2512485.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 568,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 841, DateTimeKind.Utc).AddTicks(1820), new DateTime(2026, 3, 15, 16, 15, 52, 841, DateTimeKind.Utc).AddTicks(1830), "uploads/members/seed/2512486.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 569,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 863, DateTimeKind.Utc).AddTicks(1350), new DateTime(2026, 3, 15, 16, 15, 52, 863, DateTimeKind.Utc).AddTicks(1360), "uploads/members/seed/2512487.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 570,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 868, DateTimeKind.Utc).AddTicks(1260), new DateTime(2026, 3, 15, 16, 15, 52, 868, DateTimeKind.Utc).AddTicks(1270), "uploads/members/seed/2512488.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 571,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 878, DateTimeKind.Utc).AddTicks(4270), new DateTime(2026, 3, 15, 16, 15, 52, 878, DateTimeKind.Utc).AddTicks(4280), "uploads/members/seed/2512489.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 572,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 898, DateTimeKind.Utc).AddTicks(6490), new DateTime(2026, 3, 15, 16, 15, 52, 898, DateTimeKind.Utc).AddTicks(6510), "uploads/members/seed/2512490.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 573,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 911, DateTimeKind.Utc).AddTicks(6430), "uploads/members/seed/2512491.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 574,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 939, DateTimeKind.Utc).AddTicks(2030), new DateTime(2026, 3, 15, 16, 15, 52, 939, DateTimeKind.Utc).AddTicks(2040), "uploads/members/seed/2512492.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 575,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 947, DateTimeKind.Utc).AddTicks(2600), new DateTime(2026, 3, 15, 16, 15, 52, 947, DateTimeKind.Utc).AddTicks(2610), "uploads/members/seed/2512493.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 576,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 958, DateTimeKind.Utc).AddTicks(1470), new DateTime(2026, 3, 15, 16, 15, 52, 958, DateTimeKind.Utc).AddTicks(1480), "uploads/members/seed/2512494.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 577,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 969, DateTimeKind.Utc).AddTicks(90), new DateTime(2026, 3, 15, 16, 15, 52, 969, DateTimeKind.Utc).AddTicks(100), "uploads/members/seed/2512495.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 578,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 983, DateTimeKind.Utc).AddTicks(4680), new DateTime(2026, 3, 15, 16, 15, 52, 983, DateTimeKind.Utc).AddTicks(4690), "uploads/members/seed/2512496.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 579,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 989, DateTimeKind.Utc).AddTicks(5940), new DateTime(2026, 3, 15, 16, 15, 52, 989, DateTimeKind.Utc).AddTicks(5950), "uploads/members/seed/2512497.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 580,
                column: "PhotoPath",
                value: "uploads/members/seed/2512498.jpg");

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 581,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 97, DateTimeKind.Utc).AddTicks(7220), new DateTime(2026, 3, 15, 16, 15, 53, 97, DateTimeKind.Utc).AddTicks(7230), "uploads/members/seed/2512500.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 582,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 106, DateTimeKind.Utc).AddTicks(4070), new DateTime(2026, 3, 15, 16, 15, 53, 106, DateTimeKind.Utc).AddTicks(4080), "uploads/members/seed/2512501.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 583,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 110, DateTimeKind.Utc).AddTicks(1740), new DateTime(2026, 3, 15, 16, 15, 53, 110, DateTimeKind.Utc).AddTicks(1750), "uploads/members/seed/2512502.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 584,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 153, DateTimeKind.Utc).AddTicks(7420), new DateTime(2026, 3, 15, 16, 15, 53, 153, DateTimeKind.Utc).AddTicks(7430), "uploads/members/seed/2512503.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 585,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 193, DateTimeKind.Utc).AddTicks(2810), "uploads/members/seed/2512504.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 586,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 202, DateTimeKind.Utc).AddTicks(7080), new DateTime(2026, 3, 15, 16, 15, 53, 202, DateTimeKind.Utc).AddTicks(7090), "uploads/members/seed/2512505.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 587,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 276, DateTimeKind.Utc).AddTicks(4760), new DateTime(2026, 3, 15, 16, 15, 53, 276, DateTimeKind.Utc).AddTicks(4770), "uploads/members/seed/2512506.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 588,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 284, DateTimeKind.Utc).AddTicks(3640), new DateTime(2026, 3, 15, 16, 15, 53, 284, DateTimeKind.Utc).AddTicks(3650), "uploads/members/seed/2512507.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 589,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 294, DateTimeKind.Utc).AddTicks(5300), new DateTime(2026, 3, 15, 16, 15, 53, 294, DateTimeKind.Utc).AddTicks(5310), "uploads/members/seed/2512508.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 590,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 317, DateTimeKind.Utc).AddTicks(7570), "uploads/members/seed/2512509.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 591,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 326, DateTimeKind.Utc).AddTicks(8910), new DateTime(2026, 3, 15, 16, 15, 53, 326, DateTimeKind.Utc).AddTicks(8910), "uploads/members/seed/2512510.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 592,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 361, DateTimeKind.Utc).AddTicks(6550), "uploads/members/seed/2512512.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 593,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 447, DateTimeKind.Utc).AddTicks(4900), new DateTime(2026, 3, 15, 16, 15, 53, 447, DateTimeKind.Utc).AddTicks(4910), "uploads/members/seed/2512513.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 594,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 481, DateTimeKind.Utc).AddTicks(2650), new DateTime(2026, 3, 15, 16, 15, 53, 481, DateTimeKind.Utc).AddTicks(2660), "uploads/members/seed/2512514.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 595,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 565, DateTimeKind.Utc).AddTicks(7360), new DateTime(2026, 3, 15, 16, 15, 53, 565, DateTimeKind.Utc).AddTicks(7370), "uploads/members/seed/2512515.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 596,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 575, DateTimeKind.Utc).AddTicks(8740), new DateTime(2026, 3, 15, 16, 15, 53, 575, DateTimeKind.Utc).AddTicks(8750), "uploads/members/seed/2512516.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 597,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 581, DateTimeKind.Utc).AddTicks(4920), "uploads/members/seed/2512517.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 598,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 591, DateTimeKind.Utc).AddTicks(4840), new DateTime(2026, 3, 15, 16, 15, 53, 591, DateTimeKind.Utc).AddTicks(4850), "uploads/members/seed/2512518.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 599,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 600, DateTimeKind.Utc).AddTicks(4920), new DateTime(2026, 3, 15, 16, 15, 53, 600, DateTimeKind.Utc).AddTicks(4930), "uploads/members/seed/2512519.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 600,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 609, DateTimeKind.Utc).AddTicks(6400), new DateTime(2026, 3, 15, 16, 15, 53, 609, DateTimeKind.Utc).AddTicks(6410), "uploads/members/seed/2512520.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 601,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 620, DateTimeKind.Utc).AddTicks(880), new DateTime(2026, 3, 15, 16, 15, 53, 620, DateTimeKind.Utc).AddTicks(890), "uploads/members/seed/2512521.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 602,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 659, DateTimeKind.Utc).AddTicks(4770), new DateTime(2026, 3, 15, 16, 15, 53, 659, DateTimeKind.Utc).AddTicks(4780), "uploads/members/seed/2512522.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 603,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 670, DateTimeKind.Utc).AddTicks(2130), new DateTime(2026, 3, 15, 16, 15, 53, 670, DateTimeKind.Utc).AddTicks(2140), "uploads/members/seed/2512524.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 604,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 681, DateTimeKind.Utc).AddTicks(8880), new DateTime(2026, 3, 15, 16, 15, 53, 681, DateTimeKind.Utc).AddTicks(8890), "uploads/members/seed/2512525.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 605,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 720, DateTimeKind.Utc).AddTicks(6220), new DateTime(2026, 3, 15, 16, 15, 53, 720, DateTimeKind.Utc).AddTicks(6230), "uploads/members/seed/2512526.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 606,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 750, DateTimeKind.Utc).AddTicks(7080), "uploads/members/seed/2512527.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 607,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 765, DateTimeKind.Utc).AddTicks(1340), new DateTime(2026, 3, 15, 16, 15, 53, 765, DateTimeKind.Utc).AddTicks(1350), "uploads/members/seed/2512531.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 608,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 770, DateTimeKind.Utc).AddTicks(9270), new DateTime(2026, 3, 15, 16, 15, 53, 770, DateTimeKind.Utc).AddTicks(9280), "uploads/members/seed/2512532.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 609,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 792, DateTimeKind.Utc).AddTicks(3900), new DateTime(2026, 3, 15, 16, 15, 53, 792, DateTimeKind.Utc).AddTicks(3910), "uploads/members/seed/2512533.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 610,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 800, DateTimeKind.Utc).AddTicks(7190), new DateTime(2026, 3, 15, 16, 15, 53, 800, DateTimeKind.Utc).AddTicks(7200), "uploads/members/seed/2512534.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 611,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 806, DateTimeKind.Utc).AddTicks(9530), new DateTime(2026, 3, 15, 16, 15, 53, 806, DateTimeKind.Utc).AddTicks(9540), "uploads/members/seed/2512537.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 612,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 816, DateTimeKind.Utc).AddTicks(4190), new DateTime(2026, 3, 15, 16, 15, 53, 816, DateTimeKind.Utc).AddTicks(4200), "uploads/members/seed/2512538.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 613,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 833, DateTimeKind.Utc).AddTicks(6930), new DateTime(2026, 3, 15, 16, 15, 53, 833, DateTimeKind.Utc).AddTicks(6940), "uploads/members/seed/2512539.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 614,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 851, DateTimeKind.Utc).AddTicks(5500), new DateTime(2026, 3, 15, 16, 15, 53, 851, DateTimeKind.Utc).AddTicks(5510), "uploads/members/seed/2512541.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 615,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 872, DateTimeKind.Utc).AddTicks(5340), new DateTime(2026, 3, 15, 16, 15, 53, 872, DateTimeKind.Utc).AddTicks(5350), "uploads/members/seed/2512542.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 616,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 918, DateTimeKind.Utc).AddTicks(5080), new DateTime(2026, 3, 15, 16, 15, 53, 918, DateTimeKind.Utc).AddTicks(5090), "uploads/members/seed/2512545.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 617,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 925, DateTimeKind.Utc).AddTicks(9070), "uploads/members/seed/2512547.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 618,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 938, DateTimeKind.Utc).AddTicks(2030), new DateTime(2026, 3, 15, 16, 15, 53, 938, DateTimeKind.Utc).AddTicks(2040), "uploads/members/seed/2512549.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 619,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 943, DateTimeKind.Utc).AddTicks(6000), new DateTime(2026, 3, 15, 16, 15, 53, 943, DateTimeKind.Utc).AddTicks(6010), "uploads/members/seed/2512550.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 620,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 957, DateTimeKind.Utc).AddTicks(9600), new DateTime(2026, 3, 15, 16, 15, 53, 957, DateTimeKind.Utc).AddTicks(9620), "uploads/members/seed/2512551.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 621,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 965, DateTimeKind.Utc).AddTicks(9090), new DateTime(2026, 3, 15, 16, 15, 53, 965, DateTimeKind.Utc).AddTicks(9100), "uploads/members/seed/2512552.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 622,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 970, DateTimeKind.Utc).AddTicks(9770), new DateTime(2026, 3, 15, 16, 15, 53, 970, DateTimeKind.Utc).AddTicks(9770), "uploads/members/seed/2512553.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 623,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 0, DateTimeKind.Utc).AddTicks(2760), "uploads/members/seed/2512554.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 624,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 6, DateTimeKind.Utc).AddTicks(6730), new DateTime(2026, 3, 15, 16, 15, 54, 6, DateTimeKind.Utc).AddTicks(6740), "uploads/members/seed/2512555.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 625,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 14, DateTimeKind.Utc).AddTicks(6520), new DateTime(2026, 3, 15, 16, 15, 54, 14, DateTimeKind.Utc).AddTicks(6520), "uploads/members/seed/2512556.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 626,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 19, DateTimeKind.Utc).AddTicks(2520), new DateTime(2026, 3, 15, 16, 15, 54, 19, DateTimeKind.Utc).AddTicks(2530), "uploads/members/seed/2512557.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 627,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 28, DateTimeKind.Utc).AddTicks(9100), new DateTime(2026, 3, 15, 16, 15, 54, 28, DateTimeKind.Utc).AddTicks(9110), "uploads/members/seed/2512558.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 628,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 45, DateTimeKind.Utc).AddTicks(1460), new DateTime(2026, 3, 15, 16, 15, 54, 45, DateTimeKind.Utc).AddTicks(1470), "uploads/members/seed/2512559.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 629,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 80, DateTimeKind.Utc).AddTicks(5480), new DateTime(2026, 3, 15, 16, 15, 54, 80, DateTimeKind.Utc).AddTicks(5490), "uploads/members/seed/2512560.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 630,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 102, DateTimeKind.Utc).AddTicks(5570), new DateTime(2026, 3, 15, 16, 15, 54, 102, DateTimeKind.Utc).AddTicks(5580), "uploads/members/seed/2512561.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 631,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 111, DateTimeKind.Utc).AddTicks(2890), new DateTime(2026, 3, 15, 16, 15, 54, 111, DateTimeKind.Utc).AddTicks(2900), "uploads/members/seed/2512562.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 632,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 132, DateTimeKind.Utc).AddTicks(6410), new DateTime(2026, 3, 15, 16, 15, 54, 132, DateTimeKind.Utc).AddTicks(6420), "uploads/members/seed/2512563.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 633,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 136, DateTimeKind.Utc).AddTicks(5390), new DateTime(2026, 3, 15, 16, 15, 54, 136, DateTimeKind.Utc).AddTicks(5400), "uploads/members/seed/2512564.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 634,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 172, DateTimeKind.Utc).AddTicks(2620), new DateTime(2026, 3, 15, 16, 15, 54, 172, DateTimeKind.Utc).AddTicks(2640), "uploads/members/seed/2512565.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 635,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 204, DateTimeKind.Utc).AddTicks(2530), "uploads/members/seed/2512566.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 636,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 240, DateTimeKind.Utc).AddTicks(2540), new DateTime(2026, 3, 15, 16, 15, 54, 240, DateTimeKind.Utc).AddTicks(2550), "uploads/members/seed/2512567.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 637,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 263, DateTimeKind.Utc).AddTicks(2350), "uploads/members/seed/2512568.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 638,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 478, DateTimeKind.Utc).AddTicks(5450), "uploads/members/seed/2512570.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 639,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 507, DateTimeKind.Utc).AddTicks(7770), new DateTime(2026, 3, 15, 16, 15, 54, 507, DateTimeKind.Utc).AddTicks(7780), "uploads/members/seed/2512571.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 640,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 624, DateTimeKind.Utc).AddTicks(5210), new DateTime(2026, 3, 15, 16, 15, 54, 624, DateTimeKind.Utc).AddTicks(5220), "uploads/members/seed/2512572.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 641,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 672, DateTimeKind.Utc).AddTicks(8860), new DateTime(2026, 3, 15, 16, 15, 54, 672, DateTimeKind.Utc).AddTicks(8870), "uploads/members/seed/2512573.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 642,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 723, DateTimeKind.Utc).AddTicks(830), "uploads/members/seed/2512574.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 643,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 729, DateTimeKind.Utc).AddTicks(9550), new DateTime(2026, 3, 15, 16, 15, 54, 729, DateTimeKind.Utc).AddTicks(9560), "uploads/members/seed/2512575.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 644,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 814, DateTimeKind.Utc).AddTicks(9390), new DateTime(2026, 3, 15, 16, 15, 54, 814, DateTimeKind.Utc).AddTicks(9400), "uploads/members/seed/2512578.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 645,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 826, DateTimeKind.Utc).AddTicks(7190), "uploads/members/seed/2512579.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 646,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 835, DateTimeKind.Utc).AddTicks(4350), new DateTime(2026, 3, 15, 16, 15, 54, 835, DateTimeKind.Utc).AddTicks(4360), "uploads/members/seed/2512581.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 647,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 845, DateTimeKind.Utc).AddTicks(9900), new DateTime(2026, 3, 15, 16, 15, 54, 845, DateTimeKind.Utc).AddTicks(9910), "uploads/members/seed/2512582.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 648,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 853, DateTimeKind.Utc).AddTicks(4550), new DateTime(2026, 3, 15, 16, 15, 54, 853, DateTimeKind.Utc).AddTicks(4560), "uploads/members/seed/2512583.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 649,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 861, DateTimeKind.Utc).AddTicks(4160), new DateTime(2026, 3, 15, 16, 15, 54, 861, DateTimeKind.Utc).AddTicks(4170), "uploads/members/seed/2512584.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 650,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 876, DateTimeKind.Utc).AddTicks(8950), new DateTime(2026, 3, 15, 16, 15, 54, 876, DateTimeKind.Utc).AddTicks(8960), "uploads/members/seed/2512585.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 651,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 901, DateTimeKind.Utc).AddTicks(6960), new DateTime(2026, 3, 15, 16, 15, 54, 901, DateTimeKind.Utc).AddTicks(6970), "uploads/members/seed/2512586.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 652,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 910, DateTimeKind.Utc).AddTicks(4890), "uploads/members/seed/2512587.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 653,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 921, DateTimeKind.Utc).AddTicks(740), new DateTime(2026, 3, 15, 16, 15, 54, 921, DateTimeKind.Utc).AddTicks(750), "uploads/members/seed/2512588.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 654,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 944, DateTimeKind.Utc).AddTicks(170), new DateTime(2026, 3, 15, 16, 15, 54, 944, DateTimeKind.Utc).AddTicks(180), "uploads/members/seed/2512589.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 655,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 962, DateTimeKind.Utc).AddTicks(7420), new DateTime(2026, 3, 15, 16, 15, 54, 962, DateTimeKind.Utc).AddTicks(7430), "uploads/members/seed/2512591.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 656,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 5, DateTimeKind.Utc).AddTicks(3420), new DateTime(2026, 3, 15, 16, 15, 55, 5, DateTimeKind.Utc).AddTicks(3430), "uploads/members/seed/2512592.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 657,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 31, DateTimeKind.Utc).AddTicks(9040), new DateTime(2026, 3, 15, 16, 15, 55, 31, DateTimeKind.Utc).AddTicks(9060), "uploads/members/seed/2512593.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 658,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 50, DateTimeKind.Utc).AddTicks(2170), new DateTime(2026, 3, 15, 16, 15, 55, 50, DateTimeKind.Utc).AddTicks(2180), "uploads/members/seed/2512594.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 659,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 68, DateTimeKind.Utc).AddTicks(8490), "uploads/members/seed/2512595.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 660,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 79, DateTimeKind.Utc).AddTicks(1700), new DateTime(2026, 3, 15, 16, 15, 55, 79, DateTimeKind.Utc).AddTicks(1720), "uploads/members/seed/2512596.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 661,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 84, DateTimeKind.Utc).AddTicks(4110), new DateTime(2026, 3, 15, 16, 15, 55, 84, DateTimeKind.Utc).AddTicks(4130), "uploads/members/seed/2512597.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 662,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 111, DateTimeKind.Utc).AddTicks(9410), new DateTime(2026, 3, 15, 16, 15, 55, 111, DateTimeKind.Utc).AddTicks(9420), "uploads/members/seed/2512598.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 663,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 149, DateTimeKind.Utc).AddTicks(9010), new DateTime(2026, 3, 15, 16, 15, 55, 149, DateTimeKind.Utc).AddTicks(9020), "uploads/members/seed/2512599.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 664,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 161, DateTimeKind.Utc).AddTicks(6850), new DateTime(2026, 3, 15, 16, 15, 55, 161, DateTimeKind.Utc).AddTicks(6870), "uploads/members/seed/2512600.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 665,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 179, DateTimeKind.Utc).AddTicks(3250), new DateTime(2026, 3, 15, 16, 15, 55, 179, DateTimeKind.Utc).AddTicks(3260), "uploads/members/seed/2512601.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 666,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 198, DateTimeKind.Utc).AddTicks(370), new DateTime(2026, 3, 15, 16, 15, 55, 198, DateTimeKind.Utc).AddTicks(380), "uploads/members/seed/2512602.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 667,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 278, DateTimeKind.Utc).AddTicks(2380), new DateTime(2026, 3, 15, 16, 15, 55, 278, DateTimeKind.Utc).AddTicks(2390), "uploads/members/seed/2512604.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 668,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 344, DateTimeKind.Utc).AddTicks(3940), new DateTime(2026, 3, 15, 16, 15, 55, 344, DateTimeKind.Utc).AddTicks(3950), "uploads/members/seed/2512605.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 669,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 588, DateTimeKind.Utc).AddTicks(1270), new DateTime(2026, 3, 15, 16, 15, 55, 588, DateTimeKind.Utc).AddTicks(1280), "uploads/members/seed/2512607.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 670,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 591, DateTimeKind.Utc).AddTicks(6960), new DateTime(2026, 3, 15, 16, 15, 55, 591, DateTimeKind.Utc).AddTicks(6970), "uploads/members/seed/2512609.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 671,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 636, DateTimeKind.Utc).AddTicks(5410), new DateTime(2026, 3, 15, 16, 15, 55, 636, DateTimeKind.Utc).AddTicks(5420), "uploads/members/seed/2512610.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 672,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 645, DateTimeKind.Utc).AddTicks(9230), new DateTime(2026, 3, 15, 16, 15, 55, 645, DateTimeKind.Utc).AddTicks(9230), "uploads/members/seed/2512611.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 673,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 792, DateTimeKind.Utc).AddTicks(170), new DateTime(2026, 3, 15, 16, 15, 55, 792, DateTimeKind.Utc).AddTicks(180), "uploads/members/seed/2512612.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 674,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 814, DateTimeKind.Utc).AddTicks(2170), "uploads/members/seed/2512614.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 675,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 839, DateTimeKind.Utc).AddTicks(880), new DateTime(2026, 3, 15, 16, 15, 55, 839, DateTimeKind.Utc).AddTicks(890), "uploads/members/seed/2512615.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 676,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 849, DateTimeKind.Utc).AddTicks(930), new DateTime(2026, 3, 15, 16, 15, 55, 849, DateTimeKind.Utc).AddTicks(940), "uploads/members/seed/2512616.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 677,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 922, DateTimeKind.Utc).AddTicks(1920), new DateTime(2026, 3, 15, 16, 15, 55, 922, DateTimeKind.Utc).AddTicks(1930), "uploads/members/seed/2512617.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 678,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 941, DateTimeKind.Utc).AddTicks(7580), new DateTime(2026, 3, 15, 16, 15, 55, 941, DateTimeKind.Utc).AddTicks(7590), "uploads/members/seed/2512618.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 679,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 986, DateTimeKind.Utc).AddTicks(9940), new DateTime(2026, 3, 15, 16, 15, 55, 986, DateTimeKind.Utc).AddTicks(9950), "uploads/members/seed/2512619.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 680,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 14, DateTimeKind.Utc).AddTicks(5580), new DateTime(2026, 3, 15, 16, 15, 56, 14, DateTimeKind.Utc).AddTicks(5590), "uploads/members/seed/2512620.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 681,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 81, DateTimeKind.Utc).AddTicks(2790), new DateTime(2026, 3, 15, 16, 15, 56, 81, DateTimeKind.Utc).AddTicks(2800), "uploads/members/seed/2512621.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 682,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 112, DateTimeKind.Utc).AddTicks(6610), "uploads/members/seed/2512623.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 683,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 138, DateTimeKind.Utc).AddTicks(1180), "uploads/members/seed/2512625.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 684,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 146, DateTimeKind.Utc).AddTicks(7050), new DateTime(2026, 3, 15, 16, 15, 56, 146, DateTimeKind.Utc).AddTicks(7060), "uploads/members/seed/2512626.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 685,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 155, DateTimeKind.Utc).AddTicks(7240), "uploads/members/seed/2512627.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 686,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 169, DateTimeKind.Utc).AddTicks(430), new DateTime(2026, 3, 15, 16, 15, 56, 169, DateTimeKind.Utc).AddTicks(440), "uploads/members/seed/2512628.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 687,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 175, DateTimeKind.Utc).AddTicks(9070), new DateTime(2026, 3, 15, 16, 15, 56, 175, DateTimeKind.Utc).AddTicks(9080), "uploads/members/seed/2512629.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 688,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 185, DateTimeKind.Utc).AddTicks(1620), new DateTime(2026, 3, 15, 16, 15, 56, 185, DateTimeKind.Utc).AddTicks(1630), "uploads/members/seed/2512630.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 689,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 190, DateTimeKind.Utc).AddTicks(4040), new DateTime(2026, 3, 15, 16, 15, 56, 190, DateTimeKind.Utc).AddTicks(4050), "uploads/members/seed/2512631.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 690,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 220, DateTimeKind.Utc).AddTicks(8920), new DateTime(2026, 3, 15, 16, 15, 56, 220, DateTimeKind.Utc).AddTicks(8940), "uploads/members/seed/2512632.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 691,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 242, DateTimeKind.Utc).AddTicks(8450), new DateTime(2026, 3, 15, 16, 15, 56, 242, DateTimeKind.Utc).AddTicks(8460), "uploads/members/seed/2512633.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 692,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 269, DateTimeKind.Utc).AddTicks(3250), new DateTime(2026, 3, 15, 16, 15, 56, 269, DateTimeKind.Utc).AddTicks(3270), "uploads/members/seed/2512634.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 693,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 278, DateTimeKind.Utc).AddTicks(420), new DateTime(2026, 3, 15, 16, 15, 56, 278, DateTimeKind.Utc).AddTicks(430), "uploads/members/seed/2512635.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 694,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 285, DateTimeKind.Utc).AddTicks(350), new DateTime(2026, 3, 15, 16, 15, 56, 285, DateTimeKind.Utc).AddTicks(360), "uploads/members/seed/2512636.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 695,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 331, DateTimeKind.Utc).AddTicks(1250), new DateTime(2026, 3, 15, 16, 15, 56, 331, DateTimeKind.Utc).AddTicks(1260), "uploads/members/seed/2512637.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 696,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 343, DateTimeKind.Utc).AddTicks(9680), new DateTime(2026, 3, 15, 16, 15, 56, 343, DateTimeKind.Utc).AddTicks(9690), "uploads/members/seed/2512638.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 697,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 366, DateTimeKind.Utc).AddTicks(3360), new DateTime(2026, 3, 15, 16, 15, 56, 366, DateTimeKind.Utc).AddTicks(3380), "uploads/members/seed/2512639.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 698,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 372, DateTimeKind.Utc).AddTicks(6270), new DateTime(2026, 3, 15, 16, 15, 56, 372, DateTimeKind.Utc).AddTicks(6270), "uploads/members/seed/2512640.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 699,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 424, DateTimeKind.Utc).AddTicks(420), new DateTime(2026, 3, 15, 16, 15, 56, 424, DateTimeKind.Utc).AddTicks(430), "uploads/members/seed/2512641.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 431, DateTimeKind.Utc).AddTicks(5100), new DateTime(2026, 3, 15, 16, 15, 56, 431, DateTimeKind.Utc).AddTicks(5110), "uploads/members/seed/2512642.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 457, DateTimeKind.Utc).AddTicks(5640), new DateTime(2026, 3, 15, 16, 15, 56, 457, DateTimeKind.Utc).AddTicks(5660), "uploads/members/seed/2512643.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 466, DateTimeKind.Utc).AddTicks(9960), new DateTime(2026, 3, 15, 16, 15, 56, 466, DateTimeKind.Utc).AddTicks(9980), "uploads/members/seed/2512644.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 539, DateTimeKind.Utc).AddTicks(9430), new DateTime(2026, 3, 15, 16, 15, 56, 539, DateTimeKind.Utc).AddTicks(9440), "uploads/members/seed/2512645.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 548, DateTimeKind.Utc).AddTicks(6640), new DateTime(2026, 3, 15, 16, 15, 56, 548, DateTimeKind.Utc).AddTicks(6650), "uploads/members/seed/2512647.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 705,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 597, DateTimeKind.Utc).AddTicks(4510), new DateTime(2026, 3, 15, 16, 15, 56, 597, DateTimeKind.Utc).AddTicks(4520), "uploads/members/seed/2512648.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 706,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 610, DateTimeKind.Utc).AddTicks(3180), "uploads/members/seed/2512651.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 707,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 623, DateTimeKind.Utc).AddTicks(6500), new DateTime(2026, 3, 15, 16, 15, 56, 623, DateTimeKind.Utc).AddTicks(6500), "uploads/members/seed/2512653.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 708,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 628, DateTimeKind.Utc).AddTicks(9570), new DateTime(2026, 3, 15, 16, 15, 56, 628, DateTimeKind.Utc).AddTicks(9580), "uploads/members/seed/2512654.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 709,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 665, DateTimeKind.Utc).AddTicks(9630), "uploads/members/seed/2512655.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 710,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 706, DateTimeKind.Utc).AddTicks(1540), new DateTime(2026, 3, 15, 16, 15, 56, 706, DateTimeKind.Utc).AddTicks(1550), "uploads/members/seed/2512656.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 711,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 714, DateTimeKind.Utc).AddTicks(3200), new DateTime(2026, 3, 15, 16, 15, 56, 714, DateTimeKind.Utc).AddTicks(3220), "uploads/members/seed/2512657.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 712,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 724, DateTimeKind.Utc).AddTicks(3550), new DateTime(2026, 3, 15, 16, 15, 56, 724, DateTimeKind.Utc).AddTicks(3560), "uploads/members/seed/2512658.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 713,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 744, DateTimeKind.Utc).AddTicks(4920), new DateTime(2026, 3, 15, 16, 15, 56, 744, DateTimeKind.Utc).AddTicks(4930), "uploads/members/seed/2512659.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 714,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 756, DateTimeKind.Utc).AddTicks(4800), new DateTime(2026, 3, 15, 16, 15, 56, 756, DateTimeKind.Utc).AddTicks(4810), "uploads/members/seed/2512660.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 715,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 766, DateTimeKind.Utc).AddTicks(8710), new DateTime(2026, 3, 15, 16, 15, 56, 766, DateTimeKind.Utc).AddTicks(8720), "uploads/members/seed/2512661.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 716,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 769, DateTimeKind.Utc).AddTicks(4790), new DateTime(2026, 3, 15, 16, 15, 56, 769, DateTimeKind.Utc).AddTicks(4800), "uploads/members/seed/2512662.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 717,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 780, DateTimeKind.Utc).AddTicks(6350), new DateTime(2026, 3, 15, 16, 15, 56, 780, DateTimeKind.Utc).AddTicks(6360), "uploads/members/seed/2512663.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 718,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 819, DateTimeKind.Utc).AddTicks(1770), "uploads/members/seed/2512664.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 719,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 886, DateTimeKind.Utc).AddTicks(4550), new DateTime(2026, 3, 15, 16, 15, 56, 886, DateTimeKind.Utc).AddTicks(4560), "uploads/members/seed/2512665.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 720,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 909, DateTimeKind.Utc).AddTicks(6520), "uploads/members/seed/2512666.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 721,
                column: "PhotoPath",
                value: "uploads/members/seed/2512667.jpg");

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 722,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 938, DateTimeKind.Utc).AddTicks(7830), new DateTime(2026, 3, 15, 16, 15, 56, 938, DateTimeKind.Utc).AddTicks(7840), "uploads/members/seed/2512669.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 723,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 967, DateTimeKind.Utc).AddTicks(1780), new DateTime(2026, 3, 15, 16, 15, 56, 967, DateTimeKind.Utc).AddTicks(1790), "uploads/members/seed/2512670.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 724,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 975, DateTimeKind.Utc).AddTicks(5120), new DateTime(2026, 3, 15, 16, 15, 56, 975, DateTimeKind.Utc).AddTicks(5130), "uploads/members/seed/2512671.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 725,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 5, DateTimeKind.Utc).AddTicks(7120), new DateTime(2026, 3, 15, 16, 15, 57, 5, DateTimeKind.Utc).AddTicks(7130), "uploads/members/seed/2512672.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 726,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 25, DateTimeKind.Utc).AddTicks(1840), new DateTime(2026, 3, 15, 16, 15, 57, 25, DateTimeKind.Utc).AddTicks(1850), "uploads/members/seed/2512673.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 727,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 40, DateTimeKind.Utc).AddTicks(7590), new DateTime(2026, 3, 15, 16, 15, 57, 40, DateTimeKind.Utc).AddTicks(7600), "uploads/members/seed/2512674.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 728,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 44, DateTimeKind.Utc).AddTicks(3900), "uploads/members/seed/2512675.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 729,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 58, DateTimeKind.Utc).AddTicks(2030), "uploads/members/seed/2512676.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 730,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 71, DateTimeKind.Utc).AddTicks(7920), new DateTime(2026, 3, 15, 16, 15, 57, 71, DateTimeKind.Utc).AddTicks(7930), "uploads/members/seed/2512677.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 731,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 90, DateTimeKind.Utc).AddTicks(3390), new DateTime(2026, 3, 15, 16, 15, 57, 90, DateTimeKind.Utc).AddTicks(3400), "uploads/members/seed/2512678.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 732,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 101, DateTimeKind.Utc).AddTicks(2040), new DateTime(2026, 3, 15, 16, 15, 57, 101, DateTimeKind.Utc).AddTicks(2050), "uploads/members/seed/2512679.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 733,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 138, DateTimeKind.Utc).AddTicks(9460), new DateTime(2026, 3, 15, 16, 15, 57, 138, DateTimeKind.Utc).AddTicks(9470), "uploads/members/seed/2512681.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 734,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 192, DateTimeKind.Utc).AddTicks(2040), "uploads/members/seed/2512682.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 735,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 197, DateTimeKind.Utc).AddTicks(640), new DateTime(2026, 3, 15, 16, 15, 57, 197, DateTimeKind.Utc).AddTicks(650), "uploads/members/seed/2512683.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 736,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 259, DateTimeKind.Utc).AddTicks(3750), "uploads/members/seed/2512684.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 737,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 283, DateTimeKind.Utc).AddTicks(5880), new DateTime(2026, 3, 15, 16, 15, 57, 283, DateTimeKind.Utc).AddTicks(5890), "uploads/members/seed/2512685.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 738,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 293, DateTimeKind.Utc).AddTicks(6790), new DateTime(2026, 3, 15, 16, 15, 57, 293, DateTimeKind.Utc).AddTicks(6800), "uploads/members/seed/2512686.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 739,
                column: "PhotoPath",
                value: "uploads/members/seed/2512687.jpg");

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 740,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 326, DateTimeKind.Utc).AddTicks(790), new DateTime(2026, 3, 15, 16, 15, 57, 326, DateTimeKind.Utc).AddTicks(800), "uploads/members/seed/2512688.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 741,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 357, DateTimeKind.Utc).AddTicks(1750), "uploads/members/seed/2512689.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 742,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 390, DateTimeKind.Utc).AddTicks(9520), new DateTime(2026, 3, 15, 16, 15, 57, 390, DateTimeKind.Utc).AddTicks(9530), "uploads/members/seed/2512690.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 743,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 403, DateTimeKind.Utc).AddTicks(5920), new DateTime(2026, 3, 15, 16, 15, 57, 403, DateTimeKind.Utc).AddTicks(5940), "uploads/members/seed/2512691.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 744,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 465, DateTimeKind.Utc).AddTicks(3020), new DateTime(2026, 3, 15, 16, 15, 57, 465, DateTimeKind.Utc).AddTicks(3030), "uploads/members/seed/2512692.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 745,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 489, DateTimeKind.Utc).AddTicks(8310), new DateTime(2026, 3, 15, 16, 15, 57, 489, DateTimeKind.Utc).AddTicks(8320), "uploads/members/seed/2512693.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 746,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 529, DateTimeKind.Utc).AddTicks(5380), new DateTime(2026, 3, 15, 16, 15, 57, 529, DateTimeKind.Utc).AddTicks(5390), "uploads/members/seed/2512694.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 747,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 559, DateTimeKind.Utc).AddTicks(4300), new DateTime(2026, 3, 15, 16, 15, 57, 559, DateTimeKind.Utc).AddTicks(4310), "uploads/members/seed/2512695.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 748,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 573, DateTimeKind.Utc).AddTicks(8630), "uploads/members/seed/2512696.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 749,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 601, DateTimeKind.Utc).AddTicks(5750), new DateTime(2026, 3, 15, 16, 15, 57, 601, DateTimeKind.Utc).AddTicks(5760), "uploads/members/seed/2512698.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 750,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 616, DateTimeKind.Utc).AddTicks(5540), new DateTime(2026, 3, 15, 16, 15, 57, 616, DateTimeKind.Utc).AddTicks(5550), "uploads/members/seed/2512701.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 751,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 624, DateTimeKind.Utc).AddTicks(5300), new DateTime(2026, 3, 15, 16, 15, 57, 624, DateTimeKind.Utc).AddTicks(5320), "uploads/members/seed/2512702.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 752,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 692, DateTimeKind.Utc).AddTicks(3380), "uploads/members/seed/2512703.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 753,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 719, DateTimeKind.Utc).AddTicks(5610), new DateTime(2026, 3, 15, 16, 15, 57, 719, DateTimeKind.Utc).AddTicks(5620), "uploads/members/seed/2512704.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 754,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 917, DateTimeKind.Utc).AddTicks(2290), new DateTime(2026, 3, 15, 16, 15, 57, 917, DateTimeKind.Utc).AddTicks(2310), "uploads/members/seed/2512705.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 755,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 934, DateTimeKind.Utc).AddTicks(4270), new DateTime(2026, 3, 15, 16, 15, 57, 934, DateTimeKind.Utc).AddTicks(4280), "uploads/members/seed/2512706.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 756,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 991, DateTimeKind.Utc).AddTicks(6740), "uploads/members/seed/2512707.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 757,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 2, DateTimeKind.Utc).AddTicks(3860), new DateTime(2026, 3, 15, 16, 15, 58, 2, DateTimeKind.Utc).AddTicks(3870), "uploads/members/seed/2512708.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 758,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 14, DateTimeKind.Utc).AddTicks(2930), new DateTime(2026, 3, 15, 16, 15, 58, 14, DateTimeKind.Utc).AddTicks(2940), "uploads/members/seed/2512709.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 759,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 26, DateTimeKind.Utc).AddTicks(8600), new DateTime(2026, 3, 15, 16, 15, 58, 26, DateTimeKind.Utc).AddTicks(8610), "uploads/members/seed/2512710.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 760,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 37, DateTimeKind.Utc).AddTicks(7910), new DateTime(2026, 3, 15, 16, 15, 58, 37, DateTimeKind.Utc).AddTicks(7920), "uploads/members/seed/2512711.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 761,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 59, DateTimeKind.Utc).AddTicks(7090), "uploads/members/seed/2512712.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 762,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 68, DateTimeKind.Utc).AddTicks(900), new DateTime(2026, 3, 15, 16, 15, 58, 68, DateTimeKind.Utc).AddTicks(910), "uploads/members/seed/2512713.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 763,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 103, DateTimeKind.Utc).AddTicks(560), new DateTime(2026, 3, 15, 16, 15, 58, 103, DateTimeKind.Utc).AddTicks(570), "uploads/members/seed/2512714.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 764,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 111, DateTimeKind.Utc).AddTicks(1600), new DateTime(2026, 3, 15, 16, 15, 58, 111, DateTimeKind.Utc).AddTicks(1610), "uploads/members/seed/2512715.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 765,
                column: "PhotoPath",
                value: "uploads/members/seed/2512716.jpg");

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 766,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 125, DateTimeKind.Utc).AddTicks(3790), new DateTime(2026, 3, 15, 16, 15, 58, 125, DateTimeKind.Utc).AddTicks(3800), "uploads/members/seed/2512718.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 767,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 150, DateTimeKind.Utc).AddTicks(1000), new DateTime(2026, 3, 15, 16, 15, 58, 150, DateTimeKind.Utc).AddTicks(1010), "uploads/members/seed/2512719.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 768,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 159, DateTimeKind.Utc).AddTicks(9870), new DateTime(2026, 3, 15, 16, 15, 58, 159, DateTimeKind.Utc).AddTicks(9880), "uploads/members/seed/2512722.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 769,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 209, DateTimeKind.Utc).AddTicks(4830), new DateTime(2026, 3, 15, 16, 15, 58, 209, DateTimeKind.Utc).AddTicks(4840), "uploads/members/seed/2512723.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 770,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 233, DateTimeKind.Utc).AddTicks(5400), new DateTime(2026, 3, 15, 16, 15, 58, 233, DateTimeKind.Utc).AddTicks(5410), "uploads/members/seed/2512724.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 771,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 275, DateTimeKind.Utc).AddTicks(820), new DateTime(2026, 3, 15, 16, 15, 58, 275, DateTimeKind.Utc).AddTicks(830), "uploads/members/seed/2512725.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 772,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 323, DateTimeKind.Utc).AddTicks(6570), new DateTime(2026, 3, 15, 16, 15, 58, 323, DateTimeKind.Utc).AddTicks(6580), "uploads/members/seed/2512726.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 773,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 335, DateTimeKind.Utc).AddTicks(30), "uploads/members/seed/2512728.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 774,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 376, DateTimeKind.Utc).AddTicks(3240), new DateTime(2026, 3, 15, 16, 15, 58, 376, DateTimeKind.Utc).AddTicks(3250), "uploads/members/seed/2512729.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 775,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 463, DateTimeKind.Utc).AddTicks(4510), "uploads/members/seed/2512730.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 776,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 474, DateTimeKind.Utc).AddTicks(370), new DateTime(2026, 3, 15, 16, 15, 58, 474, DateTimeKind.Utc).AddTicks(380), "uploads/members/seed/2512731.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 777,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 528, DateTimeKind.Utc).AddTicks(1390), new DateTime(2026, 3, 15, 16, 15, 58, 528, DateTimeKind.Utc).AddTicks(1400), "uploads/members/seed/2512732.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 778,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 534, DateTimeKind.Utc).AddTicks(2280), "uploads/members/seed/2512734.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 779,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 575, DateTimeKind.Utc).AddTicks(4790), "uploads/members/seed/2512735.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 780,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 630, DateTimeKind.Utc).AddTicks(3450), new DateTime(2026, 3, 15, 16, 15, 58, 630, DateTimeKind.Utc).AddTicks(3460), "uploads/members/seed/2512737.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 781,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 633, DateTimeKind.Utc).AddTicks(7340), new DateTime(2026, 3, 15, 16, 15, 58, 633, DateTimeKind.Utc).AddTicks(7350), "uploads/members/seed/2512740.jpg" });

            for (int i = 1; i <= 6; i++) migrationBuilder.DeleteData(table: "PaymentConfigurations", keyColumn: "Id", keyValue: i);

            migrationBuilder.InsertData(
                table: "PaymentConfigurations",
                columns: new[] { "Id", "AccountHolderName", "AccountNumber", "BankName", "BranchName", "CreatedAt", "Description", "DisplayName", "Gateway", "GatewayCallbackUrl", "GatewayPublicKey", "GatewaySecretKey", "Icon", "Instructions", "IsEnabled", "IsSandbox", "Method", "RequiresReceipt", "RequiresReference", "RoutingNumber", "SortOrder", "UpdatedAt", "WalletNumber" },
                values: new object[,]
                {
                    { 1, "GHCAA", null, null, null, new DateTime(2026, 3, 13, 18, 34, 13, 224, DateTimeKind.Utc).AddTicks(8170), "Pay via bKash mobile wallet", "bKash", 0, null, null, null, "🟥", "Send money to the bKash number shown. Use your Registration ID as reference.", true, true, 1, true, true, null, 1, null, "01XXXXXXXXX" },
                    { 2, "GHCAA", null, null, null, new DateTime(2026, 3, 13, 18, 34, 13, 225, DateTimeKind.Utc).AddTicks(330), "Pay via Nagad mobile wallet", "Nagad", 0, null, null, null, "🟧", "Send money to the Nagad number shown. Screenshot your confirmation.", true, true, 2, true, true, null, 2, null, "01XXXXXXXXX" },
                    { 3, "GHCAA", null, null, null, new DateTime(2026, 3, 13, 18, 34, 13, 225, DateTimeKind.Utc).AddTicks(330), "Pay via Rocket mobile wallet", "Rocket", 0, null, null, null, "🟪", "Send money to the Rocket number shown.", true, true, 3, true, true, null, 3, null, "01XXXXXXXXX" },
                    { 4, "GHCAA", "XXXXXXXXX", "Your Bank", "Main Branch", new DateTime(2026, 3, 13, 18, 34, 13, 225, DateTimeKind.Utc).AddTicks(340), "Direct bank deposit or online transfer", "Bank Transfer", 0, null, null, null, "🏦", "Transfer to the bank account shown. Attach deposit slip.", true, true, 5, true, true, null, 4, null, null },
                    { 5, null, null, null, null, new DateTime(2026, 3, 13, 18, 34, 13, 225, DateTimeKind.Utc).AddTicks(810), "Pay in cash and upload receipt", "Cash / Manual Receipt", 0, null, null, null, "🧾", "Pay in person and upload your receipt/acknowledgment slip.", true, true, 0, true, false, null, 5, new DateTime(2026, 3, 13, 18, 35, 27, 695, DateTimeKind.Utc).AddTicks(880), null },
                    { 6, null, null, null, null, new DateTime(2026, 3, 13, 18, 34, 13, 225, DateTimeKind.Utc).AddTicks(820), "Pay securely with Visa/Mastercard via SSLCommerz", "Credit/Debit Card", 3, null, null, null, "💳", "You will be redirected to a secure payment page.", false, true, 4, false, false, null, 6, null, null }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 201,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 42, 897, DateTimeKind.Utc).AddTicks(8150));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 202,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 42, 954, DateTimeKind.Utc).AddTicks(6700));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 203,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 42, 978, DateTimeKind.Utc).AddTicks(3290));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 204,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 329, DateTimeKind.Utc).AddTicks(6020));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 205,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 353, DateTimeKind.Utc).AddTicks(4600));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 206,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 476, DateTimeKind.Utc).AddTicks(7440));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 207,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 528, DateTimeKind.Utc).AddTicks(5730));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 208,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 556, DateTimeKind.Utc).AddTicks(2230));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 209,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 570, DateTimeKind.Utc).AddTicks(280));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 210,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 581, DateTimeKind.Utc).AddTicks(4370));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 211,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 593, DateTimeKind.Utc).AddTicks(2360));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 212,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 615, DateTimeKind.Utc).AddTicks(9830));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 213,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 718, DateTimeKind.Utc).AddTicks(4010));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 214,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 813, DateTimeKind.Utc).AddTicks(9470));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 215,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 855, DateTimeKind.Utc).AddTicks(8580));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 216,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 876, DateTimeKind.Utc).AddTicks(9590));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 218,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 98, DateTimeKind.Utc).AddTicks(5270));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 219,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 184, DateTimeKind.Utc).AddTicks(4480));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 220,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 201, DateTimeKind.Utc).AddTicks(4440));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 221,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 218, DateTimeKind.Utc).AddTicks(2250));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 222,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 265, DateTimeKind.Utc).AddTicks(3420));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 223,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 296, DateTimeKind.Utc).AddTicks(4070));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 224,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 325, DateTimeKind.Utc).AddTicks(5170));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 225,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 378, DateTimeKind.Utc).AddTicks(5640));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 226,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 430, DateTimeKind.Utc).AddTicks(3340));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 227,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 446, DateTimeKind.Utc).AddTicks(8370));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 228,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 455, DateTimeKind.Utc).AddTicks(3180));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 229,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 532, DateTimeKind.Utc).AddTicks(8880));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 230,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 586, DateTimeKind.Utc).AddTicks(6860));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 231,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 613, DateTimeKind.Utc).AddTicks(4870));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 232,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 625, DateTimeKind.Utc).AddTicks(4160));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 233,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 642, DateTimeKind.Utc).AddTicks(5130));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 234,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 713, DateTimeKind.Utc).AddTicks(5370));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 235,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 721, DateTimeKind.Utc).AddTicks(9780));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 236,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 732, DateTimeKind.Utc).AddTicks(9710));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 237,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 747, DateTimeKind.Utc).AddTicks(4720));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 238,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 777, DateTimeKind.Utc).AddTicks(7300));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 239,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 839, DateTimeKind.Utc).AddTicks(4520));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 240,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 858, DateTimeKind.Utc).AddTicks(8340));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 242,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 911, DateTimeKind.Utc).AddTicks(8770));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 244,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 928, DateTimeKind.Utc).AddTicks(4860));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 245,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 940, DateTimeKind.Utc).AddTicks(740));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 246,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 948, DateTimeKind.Utc).AddTicks(3590));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 247,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 960, DateTimeKind.Utc).AddTicks(8910));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 248,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 965, DateTimeKind.Utc).AddTicks(9060));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 249,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 977, DateTimeKind.Utc).AddTicks(4220));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 250,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 986, DateTimeKind.Utc).AddTicks(8190));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 251,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 999, DateTimeKind.Utc).AddTicks(870));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 252,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 10, DateTimeKind.Utc).AddTicks(2510));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 253,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 54, DateTimeKind.Utc).AddTicks(6830));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 254,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 122, DateTimeKind.Utc).AddTicks(660));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 255,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 139, DateTimeKind.Utc).AddTicks(5880));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 256,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 149, DateTimeKind.Utc).AddTicks(4160));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 257,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 179, DateTimeKind.Utc).AddTicks(8900));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 258,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 216, DateTimeKind.Utc).AddTicks(9930));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 259,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 262, DateTimeKind.Utc).AddTicks(1730));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 260,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 273, DateTimeKind.Utc).AddTicks(9150));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 261,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 309, DateTimeKind.Utc).AddTicks(7180));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 262,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 360, DateTimeKind.Utc).AddTicks(8890));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 263,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 385, DateTimeKind.Utc).AddTicks(7550));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 264,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 394, DateTimeKind.Utc).AddTicks(6060));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 265,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 400, DateTimeKind.Utc).AddTicks(5620));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 266,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 558, DateTimeKind.Utc).AddTicks(5490));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 267,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 591, DateTimeKind.Utc).AddTicks(3070));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 268,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 609, DateTimeKind.Utc).AddTicks(7190));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 270,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 630, DateTimeKind.Utc).AddTicks(5560));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 271,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 696, DateTimeKind.Utc).AddTicks(140));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 272,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 713, DateTimeKind.Utc).AddTicks(8160));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 273,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 729, DateTimeKind.Utc).AddTicks(2490));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 274,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 739, DateTimeKind.Utc).AddTicks(9960));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 275,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 758, DateTimeKind.Utc).AddTicks(1160));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 276,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 796, DateTimeKind.Utc).AddTicks(780));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 277,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 804, DateTimeKind.Utc).AddTicks(670));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 279,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 826, DateTimeKind.Utc).AddTicks(5810));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 280,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 840, DateTimeKind.Utc).AddTicks(7160));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 281,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 847, DateTimeKind.Utc).AddTicks(3150));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 282,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 860, DateTimeKind.Utc).AddTicks(8600));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 283,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 893, DateTimeKind.Utc).AddTicks(790));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 284,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 899, DateTimeKind.Utc).AddTicks(3650));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 285,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 911, DateTimeKind.Utc).AddTicks(8130));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 286,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 919, DateTimeKind.Utc).AddTicks(9060));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 287,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 935, DateTimeKind.Utc).AddTicks(9660));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 288,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 980, DateTimeKind.Utc).AddTicks(4390));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 289,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 989, DateTimeKind.Utc).AddTicks(7500));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 290,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 3, DateTimeKind.Utc).AddTicks(3600));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 293,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 205, DateTimeKind.Utc).AddTicks(6070));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 294,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 211, DateTimeKind.Utc).AddTicks(4370));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 295,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 256, DateTimeKind.Utc).AddTicks(3740));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 296,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 265, DateTimeKind.Utc).AddTicks(8410));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 297,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 278, DateTimeKind.Utc).AddTicks(9280));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 298,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 292, DateTimeKind.Utc).AddTicks(1880));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 299,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 319, DateTimeKind.Utc).AddTicks(7810));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 300,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 355, DateTimeKind.Utc).AddTicks(6100));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 302,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 386, DateTimeKind.Utc).AddTicks(3650));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 303,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 435, DateTimeKind.Utc).AddTicks(1380));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 304,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 453, DateTimeKind.Utc).AddTicks(6460));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 305,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 470, DateTimeKind.Utc).AddTicks(4700));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 306,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 502, DateTimeKind.Utc).AddTicks(5740));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 307,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 509, DateTimeKind.Utc).AddTicks(4910));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 308,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 525, DateTimeKind.Utc).AddTicks(8260));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 309,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 569, DateTimeKind.Utc).AddTicks(7520));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 310,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 579, DateTimeKind.Utc).AddTicks(40));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 311,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 661, DateTimeKind.Utc).AddTicks(5700));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 313,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 710, DateTimeKind.Utc).AddTicks(4460));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 314,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 737, DateTimeKind.Utc).AddTicks(4080));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 316,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 799, DateTimeKind.Utc).AddTicks(9540));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 317,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 845, DateTimeKind.Utc).AddTicks(3270));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 318,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 914, DateTimeKind.Utc).AddTicks(4220));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 319,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 956, DateTimeKind.Utc).AddTicks(6010));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 321,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 82, DateTimeKind.Utc).AddTicks(5000));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 322,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 107, DateTimeKind.Utc).AddTicks(3780));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 323,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 115, DateTimeKind.Utc).AddTicks(3870));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 325,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 138, DateTimeKind.Utc).AddTicks(510));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 326,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 171, DateTimeKind.Utc).AddTicks(2740));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 327,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 199, DateTimeKind.Utc).AddTicks(1810));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 328,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 231, DateTimeKind.Utc).AddTicks(2660));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 329,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 283, DateTimeKind.Utc).AddTicks(9440));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 330,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 309, DateTimeKind.Utc).AddTicks(3750));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 331,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 340, DateTimeKind.Utc).AddTicks(6380));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 332,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 348, DateTimeKind.Utc).AddTicks(3960));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 333,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 378, DateTimeKind.Utc).AddTicks(4280));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 334,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 382, DateTimeKind.Utc).AddTicks(6250));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 335,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 415, DateTimeKind.Utc).AddTicks(8070));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 336,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 450, DateTimeKind.Utc).AddTicks(4470));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 337,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 466, DateTimeKind.Utc).AddTicks(6720));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 338,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 476, DateTimeKind.Utc).AddTicks(790));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 339,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 516, DateTimeKind.Utc).AddTicks(1350));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 340,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 553, DateTimeKind.Utc).AddTicks(1500));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 341,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 560, DateTimeKind.Utc).AddTicks(7650));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 342,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 566, DateTimeKind.Utc).AddTicks(8210));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 343,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 582, DateTimeKind.Utc).AddTicks(8120));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 344,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 591, DateTimeKind.Utc).AddTicks(9260));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 345,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 600, DateTimeKind.Utc).AddTicks(1710));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 346,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 606, DateTimeKind.Utc).AddTicks(9470));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 348,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 654, DateTimeKind.Utc).AddTicks(9250));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 349,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 658, DateTimeKind.Utc).AddTicks(7850));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 350,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 696, DateTimeKind.Utc).AddTicks(7880));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 351,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 823, DateTimeKind.Utc).AddTicks(1050));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 352,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 834, DateTimeKind.Utc).AddTicks(9510));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 353,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 870, DateTimeKind.Utc).AddTicks(850));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 354,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 880, DateTimeKind.Utc).AddTicks(6600));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 355,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 893, DateTimeKind.Utc).AddTicks(4830));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 356,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 906, DateTimeKind.Utc).AddTicks(8700));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 357,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 927, DateTimeKind.Utc).AddTicks(2390));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 359,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 950, DateTimeKind.Utc).AddTicks(4860));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 360,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 963, DateTimeKind.Utc).AddTicks(4620));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 362,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 983, DateTimeKind.Utc).AddTicks(4210));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 363,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 40, DateTimeKind.Utc).AddTicks(9800));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 364,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 48, DateTimeKind.Utc).AddTicks(1810));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 365,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 53, DateTimeKind.Utc).AddTicks(6140));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 366,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 81, DateTimeKind.Utc).AddTicks(5690));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 367,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 93, DateTimeKind.Utc).AddTicks(7570));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 368,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 102, DateTimeKind.Utc).AddTicks(5630));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 370,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 238, DateTimeKind.Utc).AddTicks(4640));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 371,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 254, DateTimeKind.Utc).AddTicks(5680));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 372,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 315, DateTimeKind.Utc).AddTicks(2060));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 373,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 320, DateTimeKind.Utc).AddTicks(6890));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 374,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 321, DateTimeKind.Utc).AddTicks(980));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 375,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 353, DateTimeKind.Utc).AddTicks(1270));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 376,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 388, DateTimeKind.Utc).AddTicks(5060));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 377,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 396, DateTimeKind.Utc).AddTicks(7960));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 378,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 404, DateTimeKind.Utc).AddTicks(2700));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 379,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 426, DateTimeKind.Utc).AddTicks(1250));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 380,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 430, DateTimeKind.Utc).AddTicks(4910));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 381,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 440, DateTimeKind.Utc).AddTicks(3500));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 382,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 450, DateTimeKind.Utc).AddTicks(4310));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 383,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 460, DateTimeKind.Utc).AddTicks(5440));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 385,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 502, DateTimeKind.Utc).AddTicks(310));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 386,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 506, DateTimeKind.Utc).AddTicks(8500));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 387,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 514, DateTimeKind.Utc).AddTicks(2500));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 389,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 544, DateTimeKind.Utc).AddTicks(2770));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 390,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 584, DateTimeKind.Utc).AddTicks(9710));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 391,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 592, DateTimeKind.Utc).AddTicks(1230));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 392,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 618, DateTimeKind.Utc).AddTicks(9650));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 393,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 638, DateTimeKind.Utc).AddTicks(8890));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 394,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 645, DateTimeKind.Utc).AddTicks(2640));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 395,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 653, DateTimeKind.Utc).AddTicks(7690));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 396,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 663, DateTimeKind.Utc).AddTicks(7540));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 397,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 677, DateTimeKind.Utc).AddTicks(1180));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 398,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 681, DateTimeKind.Utc).AddTicks(6790));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 399,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 689, DateTimeKind.Utc).AddTicks(2650));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 400,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 697, DateTimeKind.Utc).AddTicks(7540));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 401,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 705, DateTimeKind.Utc).AddTicks(6310));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 403,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 729, DateTimeKind.Utc).AddTicks(3030));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 404,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 743, DateTimeKind.Utc).AddTicks(5720));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 405,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 783, DateTimeKind.Utc).AddTicks(7850));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 406,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 813, DateTimeKind.Utc).AddTicks(1360));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 407,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 853, DateTimeKind.Utc).AddTicks(2650));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 408,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 860, DateTimeKind.Utc).AddTicks(5410));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 409,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 872, DateTimeKind.Utc).AddTicks(4970));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 410,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 883, DateTimeKind.Utc).AddTicks(8770));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 411,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 916, DateTimeKind.Utc).AddTicks(6080));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 412,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 6, DateTimeKind.Utc).AddTicks(7610));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 414,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 37, DateTimeKind.Utc).AddTicks(9870));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 415,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 61, DateTimeKind.Utc).AddTicks(7090));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 416,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 83, DateTimeKind.Utc).AddTicks(2820));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 417,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 94, DateTimeKind.Utc).AddTicks(4220));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 418,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 121, DateTimeKind.Utc).AddTicks(5640));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 419,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 188, DateTimeKind.Utc).AddTicks(7310));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 420,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 214, DateTimeKind.Utc).AddTicks(2250));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 422,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 233, DateTimeKind.Utc).AddTicks(5060));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 423,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 255, DateTimeKind.Utc).AddTicks(5110));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 424,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 277, DateTimeKind.Utc).AddTicks(9680));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 425,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 295, DateTimeKind.Utc).AddTicks(7030));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 426,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 306, DateTimeKind.Utc).AddTicks(9540));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 427,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 320, DateTimeKind.Utc).AddTicks(8040));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 428,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 360, DateTimeKind.Utc).AddTicks(7300));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 429,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 436, DateTimeKind.Utc).AddTicks(8230));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 430,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 486, DateTimeKind.Utc).AddTicks(4350));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 431,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 501, DateTimeKind.Utc).AddTicks(8250));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 435,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 625, DateTimeKind.Utc).AddTicks(7720));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 436,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 657, DateTimeKind.Utc).AddTicks(6410));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 437,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 674, DateTimeKind.Utc).AddTicks(6160));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 438,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 687, DateTimeKind.Utc).AddTicks(4810));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 439,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 697, DateTimeKind.Utc).AddTicks(9210));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 440,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 748, DateTimeKind.Utc).AddTicks(4230));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 441,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 767, DateTimeKind.Utc).AddTicks(8870));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 443,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 821, DateTimeKind.Utc).AddTicks(4630));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 444,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 826, DateTimeKind.Utc).AddTicks(6790));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 446,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 844, DateTimeKind.Utc).AddTicks(6120));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 447,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 901, DateTimeKind.Utc).AddTicks(2930));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 448,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 905, DateTimeKind.Utc).AddTicks(6720));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 449,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 951, DateTimeKind.Utc).AddTicks(3440));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 450,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 956, DateTimeKind.Utc).AddTicks(1130));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 451,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 979, DateTimeKind.Utc).AddTicks(5120));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 452,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 15, DateTimeKind.Utc).AddTicks(2020));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 453,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 26, DateTimeKind.Utc).AddTicks(630));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 454,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 67, DateTimeKind.Utc).AddTicks(7950));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 455,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 86, DateTimeKind.Utc).AddTicks(5910));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 456,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 97, DateTimeKind.Utc).AddTicks(530));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 457,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 175, DateTimeKind.Utc).AddTicks(2130));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 458,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 232, DateTimeKind.Utc).AddTicks(4050));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 459,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 305, DateTimeKind.Utc).AddTicks(7090));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 460,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 428, DateTimeKind.Utc).AddTicks(820));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 461,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 438, DateTimeKind.Utc).AddTicks(5820));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 462,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 442, DateTimeKind.Utc).AddTicks(1900));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 463,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 448, DateTimeKind.Utc).AddTicks(6840));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 464,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 480, DateTimeKind.Utc).AddTicks(6950));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 465,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 495, DateTimeKind.Utc).AddTicks(5520));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 467,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 566, DateTimeKind.Utc).AddTicks(4040));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 468,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 579, DateTimeKind.Utc).AddTicks(5900));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 469,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 594, DateTimeKind.Utc).AddTicks(8540));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 470,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 611, DateTimeKind.Utc).AddTicks(7780));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 471,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 688, DateTimeKind.Utc).AddTicks(4280));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 472,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 784, DateTimeKind.Utc).AddTicks(9290));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 473,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 799, DateTimeKind.Utc).AddTicks(6750));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 474,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 824, DateTimeKind.Utc).AddTicks(6680));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 476,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 913, DateTimeKind.Utc).AddTicks(5310));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 477,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 929, DateTimeKind.Utc).AddTicks(3320));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 478,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 950, DateTimeKind.Utc).AddTicks(6350));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 479,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 966, DateTimeKind.Utc).AddTicks(6420));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 480,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 39, DateTimeKind.Utc).AddTicks(150));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 481,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 60, DateTimeKind.Utc).AddTicks(6300));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 483,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 152, DateTimeKind.Utc).AddTicks(1100));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 485,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 172, DateTimeKind.Utc).AddTicks(8540));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 486,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 181, DateTimeKind.Utc).AddTicks(3360));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 487,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 189, DateTimeKind.Utc).AddTicks(5410));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 488,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 210, DateTimeKind.Utc).AddTicks(8090));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 489,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 231, DateTimeKind.Utc).AddTicks(130));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 490,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 236, DateTimeKind.Utc).AddTicks(5040));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 491,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 245, DateTimeKind.Utc).AddTicks(330));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 492,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 291, DateTimeKind.Utc).AddTicks(3710));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 493,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 301, DateTimeKind.Utc).AddTicks(630));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 494,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 304, DateTimeKind.Utc).AddTicks(8840));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 495,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 333, DateTimeKind.Utc).AddTicks(8810));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 496,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 343, DateTimeKind.Utc).AddTicks(8320));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 497,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 350, DateTimeKind.Utc).AddTicks(7170));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 498,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 378, DateTimeKind.Utc).AddTicks(4770));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 499,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 409, DateTimeKind.Utc).AddTicks(7360));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 500,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 416, DateTimeKind.Utc).AddTicks(1360));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 501,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 461, DateTimeKind.Utc).AddTicks(2350));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 502,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 479, DateTimeKind.Utc).AddTicks(7030));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 503,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 494, DateTimeKind.Utc).AddTicks(2290));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 504,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 505, DateTimeKind.Utc).AddTicks(2810));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 505,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 516, DateTimeKind.Utc).AddTicks(2570));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 506,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 542, DateTimeKind.Utc).AddTicks(6010));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 507,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 651, DateTimeKind.Utc).AddTicks(2380));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 508,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 696, DateTimeKind.Utc).AddTicks(4840));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 510,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 727, DateTimeKind.Utc).AddTicks(9600));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 511,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 741, DateTimeKind.Utc).AddTicks(5950));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 512,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 758, DateTimeKind.Utc).AddTicks(8920));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 513,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 764, DateTimeKind.Utc).AddTicks(9800));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 514,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 770, DateTimeKind.Utc).AddTicks(2460));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 515,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 774, DateTimeKind.Utc).AddTicks(930));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 516,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 802, DateTimeKind.Utc).AddTicks(8170));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 517,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 832, DateTimeKind.Utc).AddTicks(8630));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 519,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 843, DateTimeKind.Utc).AddTicks(6220));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 520,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 869, DateTimeKind.Utc).AddTicks(3480));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 522,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 895, DateTimeKind.Utc).AddTicks(660));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 523,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 922, DateTimeKind.Utc).AddTicks(6700));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 526,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 58, DateTimeKind.Utc).AddTicks(8820));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 527,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 66, DateTimeKind.Utc).AddTicks(6150));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 528,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 89, DateTimeKind.Utc).AddTicks(5910));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 529,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 95, DateTimeKind.Utc).AddTicks(9610));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 530,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 122, DateTimeKind.Utc).AddTicks(7580));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 531,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 139, DateTimeKind.Utc).AddTicks(1290));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 532,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 142, DateTimeKind.Utc).AddTicks(950));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 533,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 175, DateTimeKind.Utc).AddTicks(4600));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 534,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 183, DateTimeKind.Utc).AddTicks(8240));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 535,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 191, DateTimeKind.Utc).AddTicks(7570));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 536,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 199, DateTimeKind.Utc).AddTicks(3370));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 537,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 231, DateTimeKind.Utc).AddTicks(8300));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 538,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 284, DateTimeKind.Utc).AddTicks(6020));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 539,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 294, DateTimeKind.Utc).AddTicks(4900));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 540,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 298, DateTimeKind.Utc).AddTicks(4940));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 541,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 339, DateTimeKind.Utc).AddTicks(4540));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 542,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 348, DateTimeKind.Utc).AddTicks(7740));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 543,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 353, DateTimeKind.Utc).AddTicks(3280));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 544,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 364, DateTimeKind.Utc).AddTicks(5690));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 545,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 375, DateTimeKind.Utc).AddTicks(110));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 546,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 391, DateTimeKind.Utc).AddTicks(3840));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 547,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 399, DateTimeKind.Utc).AddTicks(1770));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 548,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 405, DateTimeKind.Utc).AddTicks(1560));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 551,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 462, DateTimeKind.Utc).AddTicks(920));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 552,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 502, DateTimeKind.Utc).AddTicks(2000));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 553,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 549, DateTimeKind.Utc).AddTicks(8280));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 554,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 560, DateTimeKind.Utc).AddTicks(7220));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 555,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 586, DateTimeKind.Utc).AddTicks(970));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 556,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 619, DateTimeKind.Utc).AddTicks(6480));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 558,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 649, DateTimeKind.Utc).AddTicks(8110));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 559,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 674, DateTimeKind.Utc).AddTicks(8880));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 560,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 728, DateTimeKind.Utc).AddTicks(7820));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 562,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 745, DateTimeKind.Utc).AddTicks(7390));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 563,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 753, DateTimeKind.Utc).AddTicks(3540));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 564,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 767, DateTimeKind.Utc).AddTicks(8390));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 567,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 832, DateTimeKind.Utc).AddTicks(2250));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 568,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 841, DateTimeKind.Utc).AddTicks(1850));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 569,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 863, DateTimeKind.Utc).AddTicks(1380));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 570,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 868, DateTimeKind.Utc).AddTicks(1280));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 571,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 878, DateTimeKind.Utc).AddTicks(4300));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 572,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 898, DateTimeKind.Utc).AddTicks(6550));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 573,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 911, DateTimeKind.Utc).AddTicks(6460));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 574,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 939, DateTimeKind.Utc).AddTicks(2080));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 575,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 947, DateTimeKind.Utc).AddTicks(2630));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 576,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 958, DateTimeKind.Utc).AddTicks(1500));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 577,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 969, DateTimeKind.Utc).AddTicks(120));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 578,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 983, DateTimeKind.Utc).AddTicks(4720));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 580,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 12, DateTimeKind.Utc).AddTicks(8230));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 581,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 97, DateTimeKind.Utc).AddTicks(7260));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 582,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 106, DateTimeKind.Utc).AddTicks(4110));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 583,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 110, DateTimeKind.Utc).AddTicks(1770));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 584,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 153, DateTimeKind.Utc).AddTicks(7470));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 585,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 193, DateTimeKind.Utc).AddTicks(2840));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 586,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 202, DateTimeKind.Utc).AddTicks(7110));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 587,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 276, DateTimeKind.Utc).AddTicks(4800));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 588,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 284, DateTimeKind.Utc).AddTicks(3670));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 589,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 294, DateTimeKind.Utc).AddTicks(5330));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 590,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 317, DateTimeKind.Utc).AddTicks(7600));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 591,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 326, DateTimeKind.Utc).AddTicks(8940));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 592,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 361, DateTimeKind.Utc).AddTicks(6610));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 593,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 447, DateTimeKind.Utc).AddTicks(4960));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 594,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 481, DateTimeKind.Utc).AddTicks(2680));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 595,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 565, DateTimeKind.Utc).AddTicks(7410));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 596,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 575, DateTimeKind.Utc).AddTicks(8800));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 597,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 581, DateTimeKind.Utc).AddTicks(4950));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 598,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 591, DateTimeKind.Utc).AddTicks(4870));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 599,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 600, DateTimeKind.Utc).AddTicks(4950));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 600,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 609, DateTimeKind.Utc).AddTicks(6430));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 601,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 620, DateTimeKind.Utc).AddTicks(920));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 602,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 659, DateTimeKind.Utc).AddTicks(4810));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 603,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 670, DateTimeKind.Utc).AddTicks(2170));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 604,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 681, DateTimeKind.Utc).AddTicks(8920));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 605,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 720, DateTimeKind.Utc).AddTicks(6260));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 607,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 765, DateTimeKind.Utc).AddTicks(1370));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 608,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 770, DateTimeKind.Utc).AddTicks(9300));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 609,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 792, DateTimeKind.Utc).AddTicks(3930));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 610,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 800, DateTimeKind.Utc).AddTicks(7240));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 611,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 806, DateTimeKind.Utc).AddTicks(9580));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 612,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 816, DateTimeKind.Utc).AddTicks(4230));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 613,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 833, DateTimeKind.Utc).AddTicks(6970));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 614,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 851, DateTimeKind.Utc).AddTicks(5540));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 616,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 918, DateTimeKind.Utc).AddTicks(5120));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 617,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 925, DateTimeKind.Utc).AddTicks(9120));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 618,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 938, DateTimeKind.Utc).AddTicks(2080));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 619,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 943, DateTimeKind.Utc).AddTicks(6040));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 620,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 957, DateTimeKind.Utc).AddTicks(9660));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 621,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 965, DateTimeKind.Utc).AddTicks(9120));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 622,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 970, DateTimeKind.Utc).AddTicks(9800));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 623,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 0, DateTimeKind.Utc).AddTicks(2790));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 624,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 6, DateTimeKind.Utc).AddTicks(6770));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 625,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 14, DateTimeKind.Utc).AddTicks(6550));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 627,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 28, DateTimeKind.Utc).AddTicks(9160));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 628,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 45, DateTimeKind.Utc).AddTicks(1490));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 629,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 80, DateTimeKind.Utc).AddTicks(5530));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 630,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 102, DateTimeKind.Utc).AddTicks(5620));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 631,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 111, DateTimeKind.Utc).AddTicks(2930));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 632,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 132, DateTimeKind.Utc).AddTicks(6440));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 633,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 136, DateTimeKind.Utc).AddTicks(5420));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 634,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 172, DateTimeKind.Utc).AddTicks(2680));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 635,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 204, DateTimeKind.Utc).AddTicks(2740));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 636,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 240, DateTimeKind.Utc).AddTicks(2580));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 637,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 263, DateTimeKind.Utc).AddTicks(2380));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 638,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 478, DateTimeKind.Utc).AddTicks(5480));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 639,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 507, DateTimeKind.Utc).AddTicks(7810));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 640,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 624, DateTimeKind.Utc).AddTicks(5240));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 641,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 672, DateTimeKind.Utc).AddTicks(8920));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 643,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 729, DateTimeKind.Utc).AddTicks(9580));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 644,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 814, DateTimeKind.Utc).AddTicks(9440));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 645,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 826, DateTimeKind.Utc).AddTicks(7220));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 646,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 835, DateTimeKind.Utc).AddTicks(4400));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 647,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 845, DateTimeKind.Utc).AddTicks(9930));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 648,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 853, DateTimeKind.Utc).AddTicks(4580));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 649,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 861, DateTimeKind.Utc).AddTicks(4200));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 650,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 876, DateTimeKind.Utc).AddTicks(8990));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 652,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 910, DateTimeKind.Utc).AddTicks(4920));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 654,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 944, DateTimeKind.Utc).AddTicks(200));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 655,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 962, DateTimeKind.Utc).AddTicks(7450));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 656,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 5, DateTimeKind.Utc).AddTicks(3470));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 657,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 31, DateTimeKind.Utc).AddTicks(9170));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 658,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 50, DateTimeKind.Utc).AddTicks(2220));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 660,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 79, DateTimeKind.Utc).AddTicks(1760));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 661,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 84, DateTimeKind.Utc).AddTicks(4160));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 662,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 111, DateTimeKind.Utc).AddTicks(9440));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 663,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 149, DateTimeKind.Utc).AddTicks(9060));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 664,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 161, DateTimeKind.Utc).AddTicks(6900));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 665,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 179, DateTimeKind.Utc).AddTicks(3300));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 666,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 198, DateTimeKind.Utc).AddTicks(400));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 667,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 278, DateTimeKind.Utc).AddTicks(2420));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 668,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 344, DateTimeKind.Utc).AddTicks(3980));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 669,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 588, DateTimeKind.Utc).AddTicks(1320));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 670,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 591, DateTimeKind.Utc).AddTicks(6980));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 671,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 636, DateTimeKind.Utc).AddTicks(5450));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 672,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 645, DateTimeKind.Utc).AddTicks(9250));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 673,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 792, DateTimeKind.Utc).AddTicks(220));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 674,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 814, DateTimeKind.Utc).AddTicks(2210));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 675,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 839, DateTimeKind.Utc).AddTicks(930));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 676,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 849, DateTimeKind.Utc).AddTicks(980));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 677,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 922, DateTimeKind.Utc).AddTicks(1980));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 678,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 941, DateTimeKind.Utc).AddTicks(7620));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 680,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 14, DateTimeKind.Utc).AddTicks(5620));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 681,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 81, DateTimeKind.Utc).AddTicks(2830));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 682,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 112, DateTimeKind.Utc).AddTicks(6640));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 683,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 138, DateTimeKind.Utc).AddTicks(1220));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 684,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 146, DateTimeKind.Utc).AddTicks(7090));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 685,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 155, DateTimeKind.Utc).AddTicks(7270));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 686,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 169, DateTimeKind.Utc).AddTicks(470));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 687,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 175, DateTimeKind.Utc).AddTicks(9100));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 688,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 185, DateTimeKind.Utc).AddTicks(1650));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 689,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 190, DateTimeKind.Utc).AddTicks(4080));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 690,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 220, DateTimeKind.Utc).AddTicks(8980));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 691,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 242, DateTimeKind.Utc).AddTicks(8500));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 692,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 269, DateTimeKind.Utc).AddTicks(3310));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 693,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 278, DateTimeKind.Utc).AddTicks(450));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 694,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 285, DateTimeKind.Utc).AddTicks(380));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 695,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 331, DateTimeKind.Utc).AddTicks(1290));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 696,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 343, DateTimeKind.Utc).AddTicks(9720));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 697,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 366, DateTimeKind.Utc).AddTicks(3440));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 699,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 424, DateTimeKind.Utc).AddTicks(470));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 702,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 467, DateTimeKind.Utc).AddTicks(10));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 703,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 539, DateTimeKind.Utc).AddTicks(9470));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 704,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 548, DateTimeKind.Utc).AddTicks(6670));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 706,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 610, DateTimeKind.Utc).AddTicks(3210));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 707,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 623, DateTimeKind.Utc).AddTicks(6530));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 709,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 665, DateTimeKind.Utc).AddTicks(9660));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 711,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 714, DateTimeKind.Utc).AddTicks(3330));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 712,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 724, DateTimeKind.Utc).AddTicks(3580));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 713,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 744, DateTimeKind.Utc).AddTicks(4950));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 714,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 756, DateTimeKind.Utc).AddTicks(4830));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 715,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 766, DateTimeKind.Utc).AddTicks(8750));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 716,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 769, DateTimeKind.Utc).AddTicks(4830));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 717,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 780, DateTimeKind.Utc).AddTicks(6390));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 718,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 819, DateTimeKind.Utc).AddTicks(1800));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 719,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 886, DateTimeKind.Utc).AddTicks(4580));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 722,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 938, DateTimeKind.Utc).AddTicks(7880));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 723,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 967, DateTimeKind.Utc).AddTicks(1820));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 724,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 975, DateTimeKind.Utc).AddTicks(5160));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 725,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 5, DateTimeKind.Utc).AddTicks(7170));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 726,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 25, DateTimeKind.Utc).AddTicks(1870));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 727,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 40, DateTimeKind.Utc).AddTicks(7630));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 728,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 44, DateTimeKind.Utc).AddTicks(3920));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 729,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 58, DateTimeKind.Utc).AddTicks(2070));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 730,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 71, DateTimeKind.Utc).AddTicks(7950));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 731,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 90, DateTimeKind.Utc).AddTicks(3440));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 732,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 101, DateTimeKind.Utc).AddTicks(2090));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 733,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 138, DateTimeKind.Utc).AddTicks(9500));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 734,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 192, DateTimeKind.Utc).AddTicks(2090));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 735,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 197, DateTimeKind.Utc).AddTicks(670));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 736,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 259, DateTimeKind.Utc).AddTicks(3780));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 738,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 293, DateTimeKind.Utc).AddTicks(6820));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 739,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 321, DateTimeKind.Utc).AddTicks(8720));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 740,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 326, DateTimeKind.Utc).AddTicks(820));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 741,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 357, DateTimeKind.Utc).AddTicks(1790));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 742,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 390, DateTimeKind.Utc).AddTicks(9580));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 743,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 403, DateTimeKind.Utc).AddTicks(6000));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 744,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 465, DateTimeKind.Utc).AddTicks(3060));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 745,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 489, DateTimeKind.Utc).AddTicks(8340));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 746,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 529, DateTimeKind.Utc).AddTicks(5410));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 747,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 559, DateTimeKind.Utc).AddTicks(4330));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 748,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 573, DateTimeKind.Utc).AddTicks(8650));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 749,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 601, DateTimeKind.Utc).AddTicks(5800));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 750,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 616, DateTimeKind.Utc).AddTicks(5590));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 751,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 624, DateTimeKind.Utc).AddTicks(5350));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 752,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 692, DateTimeKind.Utc).AddTicks(3440));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 753,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 719, DateTimeKind.Utc).AddTicks(5650));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 754,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 917, DateTimeKind.Utc).AddTicks(2350));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 755,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 934, DateTimeKind.Utc).AddTicks(4300));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 756,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 991, DateTimeKind.Utc).AddTicks(6760));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 757,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 2, DateTimeKind.Utc).AddTicks(3890));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 758,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 14, DateTimeKind.Utc).AddTicks(2980));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 759,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 26, DateTimeKind.Utc).AddTicks(8650));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 760,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 37, DateTimeKind.Utc).AddTicks(7950));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 761,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 59, DateTimeKind.Utc).AddTicks(7130));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 763,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 103, DateTimeKind.Utc).AddTicks(600));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 764,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 111, DateTimeKind.Utc).AddTicks(1650));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 765,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 116, DateTimeKind.Utc).AddTicks(7130));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 766,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 125, DateTimeKind.Utc).AddTicks(3820));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 768,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 159, DateTimeKind.Utc).AddTicks(9900));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 770,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 233, DateTimeKind.Utc).AddTicks(5440));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 771,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 275, DateTimeKind.Utc).AddTicks(850));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 772,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 323, DateTimeKind.Utc).AddTicks(6610));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 773,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 335, DateTimeKind.Utc).AddTicks(60));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 774,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 376, DateTimeKind.Utc).AddTicks(3280));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 775,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 463, DateTimeKind.Utc).AddTicks(4530));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 776,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 474, DateTimeKind.Utc).AddTicks(400));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 777,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 528, DateTimeKind.Utc).AddTicks(1440));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 778,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 534, DateTimeKind.Utc).AddTicks(2320));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 779,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 575, DateTimeKind.Utc).AddTicks(4850));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 780,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 630, DateTimeKind.Utc).AddTicks(3480));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 781,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 633, DateTimeKind.Utc).AddTicks(7370));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AlumniEvents",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AlumniEvents",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AlumniEvents",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "JobOpportunities",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PaymentConfigurations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PaymentConfigurations",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PaymentConfigurations",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PaymentConfigurations",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PaymentConfigurations",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "PaymentConfigurations",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "IsSandbox",
                table: "PaymentConfigurations");

            migrationBuilder.DropColumn(
                name: "RequiresPayment",
                table: "AlumniEvents");

            migrationBuilder.UpdateData(
                table: "ECPeriods",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EndDate", "StartDate", "Title" },
                values: new object[] { null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Current EC" });

            migrationBuilder.UpdateData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 1,
                column: "UploadedAt",
                value: new DateTime(2026, 3, 15, 10, 17, 45, 660, DateTimeKind.Utc).AddTicks(9579));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 42, 419, DateTimeKind.Utc).AddTicks(32), new DateTime(2026, 3, 15, 16, 15, 42, 419, DateTimeKind.Utc).AddTicks(403), "uploads/members/photo_m200_7bab51a062714f77881fcd343e4ba8af_2512003.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 42, 897, DateTimeKind.Utc).AddTicks(8107), new DateTime(2026, 3, 15, 16, 15, 42, 897, DateTimeKind.Utc).AddTicks(8128), "uploads/members/photo_m201_292e84dd71d144e690e5ee15ca8b8bfc_2512005.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 42, 954, DateTimeKind.Utc).AddTicks(6649), new DateTime(2026, 3, 15, 16, 15, 42, 954, DateTimeKind.Utc).AddTicks(6672), "uploads/members/photo_m202_c5b7f9d7a1714b15b6c6cf568ad561cc_2512006.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 42, 978, DateTimeKind.Utc).AddTicks(3157), new DateTime(2026, 3, 15, 16, 15, 42, 978, DateTimeKind.Utc).AddTicks(3191), "uploads/members/photo_m203_2f76036bb7ab4da784e20b459a0d7628_2512012.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 204,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 329, DateTimeKind.Utc).AddTicks(6001), "uploads/members/photo_m204_9256556c09234e1199bbc6cf3b0cf922_2512017.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 205,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 353, DateTimeKind.Utc).AddTicks(4572), "uploads/members/photo_m205_765d4c1813454d42b57df59cf3287fdc_2512019.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 206,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 476, DateTimeKind.Utc).AddTicks(7404), new DateTime(2026, 3, 15, 16, 15, 43, 476, DateTimeKind.Utc).AddTicks(7426), "uploads/members/photo_m206_5d691f14160048c3a284f0887a29824f_2512020.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 207,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 528, DateTimeKind.Utc).AddTicks(5655), new DateTime(2026, 3, 15, 16, 15, 43, 528, DateTimeKind.Utc).AddTicks(5673), "uploads/members/photo_m207_37ef7a120d794ccba78823a742d494b1_2512022.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 208,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 556, DateTimeKind.Utc).AddTicks(2189), new DateTime(2026, 3, 15, 16, 15, 43, 556, DateTimeKind.Utc).AddTicks(2219), "uploads/members/photo_m208_e18f149f3fb444d0abd14edc6af2626e_2512023.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 209,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 570, DateTimeKind.Utc).AddTicks(234), new DateTime(2026, 3, 15, 16, 15, 43, 570, DateTimeKind.Utc).AddTicks(256), "uploads/members/photo_m209_7cb861bd6d90476891786947de0c7eeb_2512027.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 210,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 581, DateTimeKind.Utc).AddTicks(4319), new DateTime(2026, 3, 15, 16, 15, 43, 581, DateTimeKind.Utc).AddTicks(4335), "uploads/members/photo_m210_c2d649226e6441df84d5a391cf2a44a8_2512028.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 211,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 593, DateTimeKind.Utc).AddTicks(2319), new DateTime(2026, 3, 15, 16, 15, 43, 593, DateTimeKind.Utc).AddTicks(2341), "uploads/members/photo_m211_660846c0d5254673bfa8f1ff9544a51c_2512029.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 212,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 615, DateTimeKind.Utc).AddTicks(9785), "uploads/members/photo_m212_5b74129586844426b219610cae1b08e9_2512030.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 213,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 718, DateTimeKind.Utc).AddTicks(3965), new DateTime(2026, 3, 15, 16, 15, 43, 718, DateTimeKind.Utc).AddTicks(3988), "uploads/members/photo_m213_07a9d1a39b82494b81f2d9394f4f1879_2512031.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 214,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 813, DateTimeKind.Utc).AddTicks(9433), new DateTime(2026, 3, 15, 16, 15, 43, 813, DateTimeKind.Utc).AddTicks(9451), "uploads/members/photo_m214_d1fdae2074874561ae14730f49762d0d_2512032.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 215,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 855, DateTimeKind.Utc).AddTicks(8514), new DateTime(2026, 3, 15, 16, 15, 43, 855, DateTimeKind.Utc).AddTicks(8529), "uploads/members/photo_m215_5b6c407233ec4f2bbb11749d3be6f6f2_2512033.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 216,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 876, DateTimeKind.Utc).AddTicks(9565), new DateTime(2026, 3, 15, 16, 15, 43, 876, DateTimeKind.Utc).AddTicks(9578), "uploads/members/photo_m216_4ed64ebfb0bd4e3cb0717280022a86c7_2512034.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 217,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 43, 892, DateTimeKind.Utc).AddTicks(8235), new DateTime(2026, 3, 15, 16, 15, 43, 892, DateTimeKind.Utc).AddTicks(8262), "uploads/members/photo_m217_7fd5a27bfbc44fb4aa608ad3e9804381_2512035.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 218,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 98, DateTimeKind.Utc).AddTicks(5213), new DateTime(2026, 3, 15, 16, 15, 44, 98, DateTimeKind.Utc).AddTicks(5243), "uploads/members/photo_m218_2ac0121a0e794c43b1a1154f5affba53_2512036.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 219,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 184, DateTimeKind.Utc).AddTicks(4453), new DateTime(2026, 3, 15, 16, 15, 44, 184, DateTimeKind.Utc).AddTicks(4468), "uploads/members/photo_m219_68adf55e686143e78a85b5cc04036807_2512038.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 220,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 201, DateTimeKind.Utc).AddTicks(4408), new DateTime(2026, 3, 15, 16, 15, 44, 201, DateTimeKind.Utc).AddTicks(4425), "uploads/members/photo_m220_458b50ef620843209f5521fbfb2b33a7_2512040.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 221,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 218, DateTimeKind.Utc).AddTicks(2216), new DateTime(2026, 3, 15, 16, 15, 44, 218, DateTimeKind.Utc).AddTicks(2232), "uploads/members/photo_m221_c445ad199ec044408a11de9f6739e1a1_2512043.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 222,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 265, DateTimeKind.Utc).AddTicks(3386), new DateTime(2026, 3, 15, 16, 15, 44, 265, DateTimeKind.Utc).AddTicks(3406), "uploads/members/photo_m222_7239da8ec67a4b15b16ac338a3770b80_2512044.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 223,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 296, DateTimeKind.Utc).AddTicks(4034), new DateTime(2026, 3, 15, 16, 15, 44, 296, DateTimeKind.Utc).AddTicks(4052), "uploads/members/photo_m223_20cb1677c60742b79229cb227c83b676_2512046.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 224,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 325, DateTimeKind.Utc).AddTicks(5141), new DateTime(2026, 3, 15, 16, 15, 44, 325, DateTimeKind.Utc).AddTicks(5155), "uploads/members/photo_m224_c6ab408528404fb897a52c754c0f984e_2512047.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 225,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 378, DateTimeKind.Utc).AddTicks(5618), new DateTime(2026, 3, 15, 16, 15, 44, 378, DateTimeKind.Utc).AddTicks(5632), "uploads/members/photo_m225_712742a4fb724a65a9cf92c06a24129b_2512049.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 226,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 430, DateTimeKind.Utc).AddTicks(3308), new DateTime(2026, 3, 15, 16, 15, 44, 430, DateTimeKind.Utc).AddTicks(3327), "uploads/members/photo_m226_4b148ff3e11041098457712c8396f329_2512050.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 227,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 446, DateTimeKind.Utc).AddTicks(8336), new DateTime(2026, 3, 15, 16, 15, 44, 446, DateTimeKind.Utc).AddTicks(8355), "uploads/members/photo_m227_f707551f5f004ecc87e82d3d5b8003f0_2512051.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 228,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 455, DateTimeKind.Utc).AddTicks(3146), new DateTime(2026, 3, 15, 16, 15, 44, 455, DateTimeKind.Utc).AddTicks(3163), "uploads/members/photo_m228_e3aa88d294de4a7e95676ff91011e5f5_2512052.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 229,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 532, DateTimeKind.Utc).AddTicks(8869), "uploads/members/photo_m229_ac115d6389ab401699ff98257b2857b5_2512053.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 230,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 586, DateTimeKind.Utc).AddTicks(6848), "uploads/members/photo_m230_0ea0c0a0f68e4dbab752ea2e14fde1d6_2512054.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 231,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 613, DateTimeKind.Utc).AddTicks(4818), new DateTime(2026, 3, 15, 16, 15, 44, 613, DateTimeKind.Utc).AddTicks(4835), "uploads/members/photo_m231_cbb6bda8acdc4d41929265176c44cdb6_2512055.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 232,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 625, DateTimeKind.Utc).AddTicks(4139), new DateTime(2026, 3, 15, 16, 15, 44, 625, DateTimeKind.Utc).AddTicks(4153), "uploads/members/photo_m232_c0ba4bc0e59e4c17b3f65c360a01052c_2512056.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 233,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 642, DateTimeKind.Utc).AddTicks(5097), new DateTime(2026, 3, 15, 16, 15, 44, 642, DateTimeKind.Utc).AddTicks(5113), "uploads/members/photo_m233_933238cb8f1c42ca9195f357ca7fa9ce_2512057.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 234,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 713, DateTimeKind.Utc).AddTicks(5336), new DateTime(2026, 3, 15, 16, 15, 44, 713, DateTimeKind.Utc).AddTicks(5356), "uploads/members/photo_m234_faebaa6b29b545eba4a197f9da43899a_2512058.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 235,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 721, DateTimeKind.Utc).AddTicks(9753), new DateTime(2026, 3, 15, 16, 15, 44, 721, DateTimeKind.Utc).AddTicks(9771), "uploads/members/photo_m235_767086e03be6471489f98d6a79a7c213_2512059.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 236,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 732, DateTimeKind.Utc).AddTicks(9682), new DateTime(2026, 3, 15, 16, 15, 44, 732, DateTimeKind.Utc).AddTicks(9701), "uploads/members/photo_m236_200e7243001742eb8d3d5c74600fdc6b_2512060.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 237,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 747, DateTimeKind.Utc).AddTicks(4709), "uploads/members/photo_m237_ac53266a28a440dc988ef7b3932b13fb_2512061.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 238,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 777, DateTimeKind.Utc).AddTicks(6334), new DateTime(2026, 3, 15, 16, 15, 44, 777, DateTimeKind.Utc).AddTicks(6356), "uploads/members/photo_m238_d3906fa435df4251bafba6d9680d28c4_2512064.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 239,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 839, DateTimeKind.Utc).AddTicks(4489), new DateTime(2026, 3, 15, 16, 15, 44, 839, DateTimeKind.Utc).AddTicks(4508), "uploads/members/photo_m239_bf5c93e973ab4eb3936a32302fe379c2_2512065.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 240,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 858, DateTimeKind.Utc).AddTicks(8311), new DateTime(2026, 3, 15, 16, 15, 44, 858, DateTimeKind.Utc).AddTicks(8326), "uploads/members/photo_m240_26cb904692814acea32d70e2c0e43aaa_2512066.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 241,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 897, DateTimeKind.Utc).AddTicks(1306), new DateTime(2026, 3, 15, 16, 15, 44, 897, DateTimeKind.Utc).AddTicks(1332), "uploads/members/photo_m241_373f3fcdf4214811a1db8bd935a1f074_2512067.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 242,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 911, DateTimeKind.Utc).AddTicks(8736), new DateTime(2026, 3, 15, 16, 15, 44, 911, DateTimeKind.Utc).AddTicks(8754), "uploads/members/photo_m242_41be4d516ac94ed8acbc43c31cfedc05_2512069.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 243,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 919, DateTimeKind.Utc).AddTicks(9739), new DateTime(2026, 3, 15, 16, 15, 44, 919, DateTimeKind.Utc).AddTicks(9752), "uploads/members/photo_m243_b71044b3aab04845bd7b01b4dcd6914c_2512070.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 244,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 928, DateTimeKind.Utc).AddTicks(4831), new DateTime(2026, 3, 15, 16, 15, 44, 928, DateTimeKind.Utc).AddTicks(4846), "uploads/members/photo_m244_e1a908c99c0d47d58d557be70022f561_2512071.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 245,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 940, DateTimeKind.Utc).AddTicks(719), new DateTime(2026, 3, 15, 16, 15, 44, 940, DateTimeKind.Utc).AddTicks(735), "uploads/members/photo_m245_7426c18ff04741b58bcb69efce34cf80_2512072.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 246,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 948, DateTimeKind.Utc).AddTicks(3568), new DateTime(2026, 3, 15, 16, 15, 44, 948, DateTimeKind.Utc).AddTicks(3582), "uploads/members/photo_m246_025913f7e597481b812d081057109f19_2512073.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 247,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 960, DateTimeKind.Utc).AddTicks(8885), new DateTime(2026, 3, 15, 16, 15, 44, 960, DateTimeKind.Utc).AddTicks(8901), "uploads/members/photo_m247_3430182f9f134363a154aac62dc5a610_2512074.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 248,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 965, DateTimeKind.Utc).AddTicks(9028), new DateTime(2026, 3, 15, 16, 15, 44, 965, DateTimeKind.Utc).AddTicks(9044), "uploads/members/photo_m248_cf5faa0b182547a1883b61c9ceb8c529_2512075.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 249,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 977, DateTimeKind.Utc).AddTicks(4189), new DateTime(2026, 3, 15, 16, 15, 44, 977, DateTimeKind.Utc).AddTicks(4207), "uploads/members/photo_m249_c2b63f0d803248328e9d33fc019e859f_2512078.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 250,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 986, DateTimeKind.Utc).AddTicks(8157), new DateTime(2026, 3, 15, 16, 15, 44, 986, DateTimeKind.Utc).AddTicks(8176), "uploads/members/photo_m250_7a9150c50bc84957b1e1f47d856174a7_2512079.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 251,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 44, 999, DateTimeKind.Utc).AddTicks(843), "uploads/members/photo_m251_8fef6ff5c779451db9fbc23c4c1029fa_2512080.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 252,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 10, DateTimeKind.Utc).AddTicks(2448), new DateTime(2026, 3, 15, 16, 15, 45, 10, DateTimeKind.Utc).AddTicks(2479), "uploads/members/photo_m252_340cb3e1b0ee48f38cb2777ed8721901_2512082.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 253,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 54, DateTimeKind.Utc).AddTicks(6787), "uploads/members/photo_m253_587f9dd2cb6a4817ad728506fa005466_2512083.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 254,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 122, DateTimeKind.Utc).AddTicks(621), new DateTime(2026, 3, 15, 16, 15, 45, 122, DateTimeKind.Utc).AddTicks(646), "uploads/members/photo_m254_3dc9251f9288436fa6743154ab13a30d_2512084.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 255,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 139, DateTimeKind.Utc).AddTicks(5854), new DateTime(2026, 3, 15, 16, 15, 45, 139, DateTimeKind.Utc).AddTicks(5863), "uploads/members/photo_m255_b0a407f8198844b8b895264c48b91413_2512087.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 256,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 149, DateTimeKind.Utc).AddTicks(4144), new DateTime(2026, 3, 15, 16, 15, 45, 149, DateTimeKind.Utc).AddTicks(4151), "uploads/members/photo_m256_f55cc1db9223464bb16287e7ed54acf7_2512088.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 257,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 179, DateTimeKind.Utc).AddTicks(8872), "uploads/members/photo_m257_105347d04ca24be8aa577f6eaa51cee1_2512089.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 258,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 216, DateTimeKind.Utc).AddTicks(9902), new DateTime(2026, 3, 15, 16, 15, 45, 216, DateTimeKind.Utc).AddTicks(9912), "uploads/members/photo_m258_5047a9e813074ec592c6f5e3d4f537e0_2512091.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 259,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 262, DateTimeKind.Utc).AddTicks(1707), new DateTime(2026, 3, 15, 16, 15, 45, 262, DateTimeKind.Utc).AddTicks(1716), "uploads/members/photo_m259_fafda99c58d84187a4815578ee929659_2512092.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 260,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 273, DateTimeKind.Utc).AddTicks(9137), "uploads/members/photo_m260_98084b64789b4176b18646348445c163_2512093.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 261,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 309, DateTimeKind.Utc).AddTicks(7163), "uploads/members/photo_m261_089334c285614a4390415c4079e3f297_2512094.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 262,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 360, DateTimeKind.Utc).AddTicks(8862), "uploads/members/photo_m262_b4685b42378d480bbf411cefdb629eb9_2512095.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 263,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 385, DateTimeKind.Utc).AddTicks(7514), "uploads/members/photo_m263_8dea6cd0e4034c36a353fa480322718e_2512096.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 264,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 394, DateTimeKind.Utc).AddTicks(6026), new DateTime(2026, 3, 15, 16, 15, 45, 394, DateTimeKind.Utc).AddTicks(6034), "uploads/members/photo_m264_dadb72f6c0174ab78c6d8fd780232e72_2512097.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 265,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 400, DateTimeKind.Utc).AddTicks(5598), new DateTime(2026, 3, 15, 16, 15, 45, 400, DateTimeKind.Utc).AddTicks(5604), "uploads/members/photo_m265_52fe88dcc24d4c8dae4626a59a911f8f_2512098.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 266,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 558, DateTimeKind.Utc).AddTicks(5465), new DateTime(2026, 3, 15, 16, 15, 45, 558, DateTimeKind.Utc).AddTicks(5473), "uploads/members/photo_m266_522bd817486b4ef0b184d7130e425fd4_2512099.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 267,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 591, DateTimeKind.Utc).AddTicks(3048), "uploads/members/photo_m267_63a2517ec39846b7b574c4318731f46a_2512101.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 268,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 609, DateTimeKind.Utc).AddTicks(7163), new DateTime(2026, 3, 15, 16, 15, 45, 609, DateTimeKind.Utc).AddTicks(7172), "uploads/members/photo_m268_9a1078d625ee403b9f3a7e5b2c26060c_2512102.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 269,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 619, DateTimeKind.Utc).AddTicks(6555), new DateTime(2026, 3, 15, 16, 15, 45, 619, DateTimeKind.Utc).AddTicks(6562), "uploads/members/photo_m269_7d5400da42a040648d4567c78bc61ecc_2512103.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 270,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 630, DateTimeKind.Utc).AddTicks(5534), new DateTime(2026, 3, 15, 16, 15, 45, 630, DateTimeKind.Utc).AddTicks(5542), "uploads/members/photo_m270_69eb0d946489478893435d13abfab8ac_2512105.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 271,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 696, DateTimeKind.Utc).AddTicks(114), new DateTime(2026, 3, 15, 16, 15, 45, 696, DateTimeKind.Utc).AddTicks(122), "uploads/members/photo_m271_a13ff78ae87c4e769529ebb8ef71eff4_2512106.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 272,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 713, DateTimeKind.Utc).AddTicks(8074), new DateTime(2026, 3, 15, 16, 15, 45, 713, DateTimeKind.Utc).AddTicks(8098), "uploads/members/photo_m272_8adf1cb0f2ce480598c44d067c6f16d9_2512107.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 273,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 729, DateTimeKind.Utc).AddTicks(2442), new DateTime(2026, 3, 15, 16, 15, 45, 729, DateTimeKind.Utc).AddTicks(2456), "uploads/members/photo_m273_4eb33de31dd64261a11c9d84fe37afad_2512108.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 274,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 739, DateTimeKind.Utc).AddTicks(9928), new DateTime(2026, 3, 15, 16, 15, 45, 739, DateTimeKind.Utc).AddTicks(9939), "uploads/members/photo_m274_ae21cf5643784377a2c33acb5f10c745_2512110.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 275,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 758, DateTimeKind.Utc).AddTicks(1093), new DateTime(2026, 3, 15, 16, 15, 45, 758, DateTimeKind.Utc).AddTicks(1102), "uploads/members/photo_m275_6ad647b5cdc64ea887f094c4b93f470b_2512111.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 276,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 796, DateTimeKind.Utc).AddTicks(752), "uploads/members/photo_m276_568d21c351684354b1847fd88b4d37f1_2512112.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 277,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 804, DateTimeKind.Utc).AddTicks(652), new DateTime(2026, 3, 15, 16, 15, 45, 804, DateTimeKind.Utc).AddTicks(659), "uploads/members/photo_m277_579f995ef8e04518ba52390fe7f014cd_2512113.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 278,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 814, DateTimeKind.Utc).AddTicks(762), "uploads/members/photo_m278_3902080cd5d9420482985077846e0131_2512114.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 279,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 826, DateTimeKind.Utc).AddTicks(5787), new DateTime(2026, 3, 15, 16, 15, 45, 826, DateTimeKind.Utc).AddTicks(5795), "uploads/members/photo_m279_faa8a83582ad48fc8d237394faf7cd1b_2512115.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 280,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 840, DateTimeKind.Utc).AddTicks(7124), new DateTime(2026, 3, 15, 16, 15, 45, 840, DateTimeKind.Utc).AddTicks(7137), "uploads/members/photo_m280_999f3cf98e9d41c1ba7e132905ecaa46_2512116.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 281,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 847, DateTimeKind.Utc).AddTicks(3127), new DateTime(2026, 3, 15, 16, 15, 45, 847, DateTimeKind.Utc).AddTicks(3135), "uploads/members/photo_m281_bbff8eac44574ef5bb8aa59d9616443b_2512117.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 282,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 860, DateTimeKind.Utc).AddTicks(8581), new DateTime(2026, 3, 15, 16, 15, 45, 860, DateTimeKind.Utc).AddTicks(8588), "uploads/members/photo_m282_43d7c4ae1ce84836bec554de4c5987a5_2512118.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 283,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 893, DateTimeKind.Utc).AddTicks(749), new DateTime(2026, 3, 15, 16, 15, 45, 893, DateTimeKind.Utc).AddTicks(762), "uploads/members/photo_m283_d3e6306f904c40359062f7f24cb12fa8_2512120.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 284,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 899, DateTimeKind.Utc).AddTicks(3617), new DateTime(2026, 3, 15, 16, 15, 45, 899, DateTimeKind.Utc).AddTicks(3629), "uploads/members/photo_m284_bbb6455212a943439791cea3eb2e8b18_2512123.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 285,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 911, DateTimeKind.Utc).AddTicks(8101), new DateTime(2026, 3, 15, 16, 15, 45, 911, DateTimeKind.Utc).AddTicks(8109), "uploads/members/photo_m285_ccb98a53459c4809a6459de779647301_2512125.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 286,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 919, DateTimeKind.Utc).AddTicks(9048), "uploads/members/photo_m286_4e7c9ff5750845d3b0733ff88fbf4a8f_2512126.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 287,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 935, DateTimeKind.Utc).AddTicks(9642), new DateTime(2026, 3, 15, 16, 15, 45, 935, DateTimeKind.Utc).AddTicks(9649), "uploads/members/photo_m287_d0b87bac2f62471585fded60a2f61fe0_2512127.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 288,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 980, DateTimeKind.Utc).AddTicks(4368), new DateTime(2026, 3, 15, 16, 15, 45, 980, DateTimeKind.Utc).AddTicks(4376), "uploads/members/photo_m288_2ec1b0ff6dda4178b0945f28bd972c38_2512128.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 289,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 45, 989, DateTimeKind.Utc).AddTicks(7481), "uploads/members/photo_m289_285480c7f20b4494912f6013db42f274_2512129.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 290,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 3, DateTimeKind.Utc).AddTicks(3576), new DateTime(2026, 3, 15, 16, 15, 46, 3, DateTimeKind.Utc).AddTicks(3583), "uploads/members/photo_m290_bc1f97cfee4147c28e83092fd69953f6_2512131.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 291,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 123, DateTimeKind.Utc).AddTicks(9059), new DateTime(2026, 3, 15, 16, 15, 46, 123, DateTimeKind.Utc).AddTicks(9066), "uploads/members/photo_m291_3e5f053b137c4ece80040c6a00526718_2512135.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 292,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 172, DateTimeKind.Utc).AddTicks(1682), new DateTime(2026, 3, 15, 16, 15, 46, 172, DateTimeKind.Utc).AddTicks(1695), "uploads/members/photo_m292_9125d60cca224564bb7ca77a72ee7a9c_2512136.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 293,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 205, DateTimeKind.Utc).AddTicks(6028), "uploads/members/photo_m293_6a79324840b04aff97acaf659a23e9c5_2512137.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 294,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 211, DateTimeKind.Utc).AddTicks(4344), "uploads/members/photo_m294_4f74982cc0554d9bbd6ee22e418055ab_2512138.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 295,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 256, DateTimeKind.Utc).AddTicks(3694), new DateTime(2026, 3, 15, 16, 15, 46, 256, DateTimeKind.Utc).AddTicks(3706), "uploads/members/photo_m295_53a3dda5aacb453d82fe4be06b8e16e2_2512139.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 296,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 265, DateTimeKind.Utc).AddTicks(8376), new DateTime(2026, 3, 15, 16, 15, 46, 265, DateTimeKind.Utc).AddTicks(8388), "uploads/members/photo_m296_fefb0e21c90c4736bd23e831fef90556_2512140.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 297,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 278, DateTimeKind.Utc).AddTicks(9252), "uploads/members/photo_m297_c89d3d89b18b4cf8bcb1d9b8e3b7ee66_2512141.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 298,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 292, DateTimeKind.Utc).AddTicks(1852), new DateTime(2026, 3, 15, 16, 15, 46, 292, DateTimeKind.Utc).AddTicks(1859), "uploads/members/photo_m298_21758c82af43406cacf5b8f95c84373e_2512142.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 299,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 319, DateTimeKind.Utc).AddTicks(7773), "uploads/members/photo_m299_d7138c77213244a68304d4fb69446368_2512143.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 355, DateTimeKind.Utc).AddTicks(6059), new DateTime(2026, 3, 15, 16, 15, 46, 355, DateTimeKind.Utc).AddTicks(6072), "uploads/members/photo_m300_510bff86e6d84d9ba4182354340871df_2512144.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 379, DateTimeKind.Utc).AddTicks(422), new DateTime(2026, 3, 15, 16, 15, 46, 379, DateTimeKind.Utc).AddTicks(434), "uploads/members/photo_m301_10cec5c7e64d4f6fb36c1e7ef7cad6fc_2512145.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 302,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 386, DateTimeKind.Utc).AddTicks(3614), new DateTime(2026, 3, 15, 16, 15, 46, 386, DateTimeKind.Utc).AddTicks(3627), "uploads/members/photo_m302_3ea229d2c3b84a85890c8558eb399acc_2512146.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 303,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 435, DateTimeKind.Utc).AddTicks(1341), new DateTime(2026, 3, 15, 16, 15, 46, 435, DateTimeKind.Utc).AddTicks(1354), "uploads/members/photo_m303_38b2dcc82afd420c833aa953b80c86f2_2512147.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 304,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 453, DateTimeKind.Utc).AddTicks(6445), "uploads/members/photo_m304_dc3fe4c5e639404ba21e1be687aef184_2512148.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 305,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 470, DateTimeKind.Utc).AddTicks(4667), new DateTime(2026, 3, 15, 16, 15, 46, 470, DateTimeKind.Utc).AddTicks(4676), "uploads/members/photo_m305_977053c0af9d49ed972958cad4568946_2512149.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 306,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 502, DateTimeKind.Utc).AddTicks(5688), new DateTime(2026, 3, 15, 16, 15, 46, 502, DateTimeKind.Utc).AddTicks(5704), "uploads/members/photo_m306_1a886f56f7a34ec78a8159bcb59eeed5_2512150.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 307,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 509, DateTimeKind.Utc).AddTicks(4882), "uploads/members/photo_m307_2716acef61e34273bad2142d396ba015_2512151.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 308,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 525, DateTimeKind.Utc).AddTicks(8209), new DateTime(2026, 3, 15, 16, 15, 46, 525, DateTimeKind.Utc).AddTicks(8222), "uploads/members/photo_m308_a903047f9ed648bfb26c80323bf16186_2512152.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 309,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 569, DateTimeKind.Utc).AddTicks(7483), "uploads/members/photo_m309_37881a082e4e45c8ba652dac859a0fb3_2512153.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 310,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 579, DateTimeKind.Utc).AddTicks(16), new DateTime(2026, 3, 15, 16, 15, 46, 579, DateTimeKind.Utc).AddTicks(24), "uploads/members/photo_m310_306c625c351142eba5f52eebe0190bbe_2512154.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 311,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 661, DateTimeKind.Utc).AddTicks(5651), new DateTime(2026, 3, 15, 16, 15, 46, 661, DateTimeKind.Utc).AddTicks(5666), "uploads/members/photo_m311_24998b0abb064958ae465a8490028649_2512155.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 312,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 695, DateTimeKind.Utc).AddTicks(107), new DateTime(2026, 3, 15, 16, 15, 46, 695, DateTimeKind.Utc).AddTicks(115), "uploads/members/photo_m312_45304cd915a94a1aa8198daba794c511_2512156.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 313,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 710, DateTimeKind.Utc).AddTicks(4437), new DateTime(2026, 3, 15, 16, 15, 46, 710, DateTimeKind.Utc).AddTicks(4445), "uploads/members/photo_m313_cea7234a3c974e1a895fe7f918b65b13_2512157.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 314,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 737, DateTimeKind.Utc).AddTicks(4045), new DateTime(2026, 3, 15, 16, 15, 46, 737, DateTimeKind.Utc).AddTicks(4055), "uploads/members/photo_m314_4fe3384d9c7d454f89f7bd772b245472_2512158.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 315,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 786, DateTimeKind.Utc).AddTicks(7974), new DateTime(2026, 3, 15, 16, 15, 46, 786, DateTimeKind.Utc).AddTicks(7987), "uploads/members/photo_m315_b6ee585c975346b9976e29eb178236b1_2512159.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 316,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 799, DateTimeKind.Utc).AddTicks(9517), new DateTime(2026, 3, 15, 16, 15, 46, 799, DateTimeKind.Utc).AddTicks(9523), "uploads/members/photo_m316_496fd69bf7d6489fbcda2867079fe79f_2512160.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 317,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 845, DateTimeKind.Utc).AddTicks(3224), new DateTime(2026, 3, 15, 16, 15, 46, 845, DateTimeKind.Utc).AddTicks(3238), "uploads/members/photo_m317_7470c4941e2743d2b4e6e40b2d40c17b_2512161.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 318,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 914, DateTimeKind.Utc).AddTicks(4168), new DateTime(2026, 3, 15, 16, 15, 46, 914, DateTimeKind.Utc).AddTicks(4182), "uploads/members/photo_m318_9433c935066f471c97ced1b513aba980_2512162.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 319,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 46, 956, DateTimeKind.Utc).AddTicks(5968), "uploads/members/photo_m319_827ac2b693604bf4836b6dfa69f24033_2512163.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 320,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 30, DateTimeKind.Utc).AddTicks(9465), new DateTime(2026, 3, 15, 16, 15, 47, 30, DateTimeKind.Utc).AddTicks(9477), "uploads/members/photo_m320_d260f49ce0754a858827b6e8b0616097_2512164.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 321,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 82, DateTimeKind.Utc).AddTicks(4949), new DateTime(2026, 3, 15, 16, 15, 47, 82, DateTimeKind.Utc).AddTicks(4964), "uploads/members/photo_m321_be0a4134487a49a0ac1cd6f0fd295df8_2512165.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 322,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 107, DateTimeKind.Utc).AddTicks(3751), new DateTime(2026, 3, 15, 16, 15, 47, 107, DateTimeKind.Utc).AddTicks(3759), "uploads/members/photo_m322_e8bea3998493486b83c816e3da3d8c70_2512171.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 323,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 115, DateTimeKind.Utc).AddTicks(3833), new DateTime(2026, 3, 15, 16, 15, 47, 115, DateTimeKind.Utc).AddTicks(3845), "uploads/members/photo_m323_cabac1e93a424e8181aec05298da1e39_2512172.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 324,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 122, DateTimeKind.Utc).AddTicks(7529), new DateTime(2026, 3, 15, 16, 15, 47, 122, DateTimeKind.Utc).AddTicks(7545), "uploads/members/photo_m324_282a931aaf3f43a0badead86a631dd69_2512173.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 325,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 138, DateTimeKind.Utc).AddTicks(496), "uploads/members/photo_m325_1a4e271d941d49d599fcad63fd686293_2512174.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 326,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 171, DateTimeKind.Utc).AddTicks(2694), new DateTime(2026, 3, 15, 16, 15, 47, 171, DateTimeKind.Utc).AddTicks(2707), "uploads/members/photo_m326_a53fff355ee64759ae0f0b7d86e4665d_2512175.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 327,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 199, DateTimeKind.Utc).AddTicks(1748), "uploads/members/photo_m327_24a12ab839a245a4aac6935d01459df7_2512176.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 328,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 231, DateTimeKind.Utc).AddTicks(2605), new DateTime(2026, 3, 15, 16, 15, 47, 231, DateTimeKind.Utc).AddTicks(2622), "uploads/members/photo_m328_3ef9c130020449be9c159cebbb8860be_2512177.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 329,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 283, DateTimeKind.Utc).AddTicks(9418), new DateTime(2026, 3, 15, 16, 15, 47, 283, DateTimeKind.Utc).AddTicks(9425), "uploads/members/photo_m329_175924b3797f4f3a98de992e09f36696_2512179.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 330,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 309, DateTimeKind.Utc).AddTicks(3703), new DateTime(2026, 3, 15, 16, 15, 47, 309, DateTimeKind.Utc).AddTicks(3714), "uploads/members/photo_m330_2dd6bfa1698a4b3aa6dd76d81d15ef73_2512180.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 331,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 340, DateTimeKind.Utc).AddTicks(6336), new DateTime(2026, 3, 15, 16, 15, 47, 340, DateTimeKind.Utc).AddTicks(6352), "uploads/members/photo_m331_b85d03de7c8341069552246fb59bdaaa_2512181.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 332,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 348, DateTimeKind.Utc).AddTicks(3886), "uploads/members/photo_m332_19fe8632f49947e79d2421ec4f17bc4b_2512182.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 333,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 378, DateTimeKind.Utc).AddTicks(4254), new DateTime(2026, 3, 15, 16, 15, 47, 378, DateTimeKind.Utc).AddTicks(4262), "uploads/members/photo_m333_2911187e43e7408ba5fc4f6e06134798_2512183.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 334,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 382, DateTimeKind.Utc).AddTicks(6231), "uploads/members/photo_m334_a8221061ae7a4bb2a4567de54be3fcac_2512184.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 335,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 415, DateTimeKind.Utc).AddTicks(8022), new DateTime(2026, 3, 15, 16, 15, 47, 415, DateTimeKind.Utc).AddTicks(8035), "uploads/members/photo_m335_e5cb7ea0162f4d9191f075fc9fec80b4_2512185.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 336,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 450, DateTimeKind.Utc).AddTicks(4432), "uploads/members/photo_m336_80f0bb570ad348a094208deceb433d41_2512187.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 337,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 466, DateTimeKind.Utc).AddTicks(6696), new DateTime(2026, 3, 15, 16, 15, 47, 466, DateTimeKind.Utc).AddTicks(6702), "uploads/members/photo_m337_bb4df6d8a7704243b5042bd9824dbaba_2512188.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 338,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 476, DateTimeKind.Utc).AddTicks(748), new DateTime(2026, 3, 15, 16, 15, 47, 476, DateTimeKind.Utc).AddTicks(761), "uploads/members/photo_m338_ff16c10d056c4b30aee2ee721e388ffa_2512189.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 339,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 516, DateTimeKind.Utc).AddTicks(1299), new DateTime(2026, 3, 15, 16, 15, 47, 516, DateTimeKind.Utc).AddTicks(1311), "uploads/members/photo_m339_a00fa879783a4734a16eb035fb81d408_2512190.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 340,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 553, DateTimeKind.Utc).AddTicks(1453), new DateTime(2026, 3, 15, 16, 15, 47, 553, DateTimeKind.Utc).AddTicks(1465), "uploads/members/photo_m340_d9732c3832684f2f9b4967ce724dcb7b_2512191.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 341,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 560, DateTimeKind.Utc).AddTicks(7614), new DateTime(2026, 3, 15, 16, 15, 47, 560, DateTimeKind.Utc).AddTicks(7626), "uploads/members/photo_m341_e8f2a1efbc2c472b9d7cd393d4316420_2512192.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 342,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 566, DateTimeKind.Utc).AddTicks(8193), new DateTime(2026, 3, 15, 16, 15, 47, 566, DateTimeKind.Utc).AddTicks(8201), "uploads/members/photo_m342_eea46290223d4b6c8f5be676e0f12771_2512195.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 343,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 582, DateTimeKind.Utc).AddTicks(8096), new DateTime(2026, 3, 15, 16, 15, 47, 582, DateTimeKind.Utc).AddTicks(8105), "uploads/members/photo_m343_cd5ac6a2ad25422ba9b02d0af1a866b6_2512196.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 344,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 591, DateTimeKind.Utc).AddTicks(9208), "uploads/members/photo_m344_b6e0072efdde48b9ab4b2cad5f4b3dee_2512197.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 345,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 600, DateTimeKind.Utc).AddTicks(1687), new DateTime(2026, 3, 15, 16, 15, 47, 600, DateTimeKind.Utc).AddTicks(1695), "uploads/members/photo_m345_e45e2b08c0424631a7d9cafe75deede7_2512198.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 346,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 606, DateTimeKind.Utc).AddTicks(9441), "uploads/members/photo_m346_af403113ed9b47f3b47436ef9c948485_2512201.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 347,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 619, DateTimeKind.Utc).AddTicks(297), new DateTime(2026, 3, 15, 16, 15, 47, 619, DateTimeKind.Utc).AddTicks(306), "uploads/members/photo_m347_52d8d94690e84f36baa6374e0aa90048_2512204.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 348,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 654, DateTimeKind.Utc).AddTicks(9227), new DateTime(2026, 3, 15, 16, 15, 47, 654, DateTimeKind.Utc).AddTicks(9235), "uploads/members/photo_m348_25277c7684b34af980aea8ce342e046a_2512205.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 349,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 658, DateTimeKind.Utc).AddTicks(7837), "uploads/members/photo_m349_00c8cb91c93442ad95ef55292e2f5d6d_2512208.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 350,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 696, DateTimeKind.Utc).AddTicks(7839), new DateTime(2026, 3, 15, 16, 15, 47, 696, DateTimeKind.Utc).AddTicks(7852), "uploads/members/photo_m350_549419797acb4b1cb0638ca386c87996_2512210.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 351,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 823, DateTimeKind.Utc).AddTicks(1019), new DateTime(2026, 3, 15, 16, 15, 47, 823, DateTimeKind.Utc).AddTicks(1028), "uploads/members/photo_m351_6f6fb4de7619452198f78ac8cb2691b6_2512213.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 352,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 834, DateTimeKind.Utc).AddTicks(9478), new DateTime(2026, 3, 15, 16, 15, 47, 834, DateTimeKind.Utc).AddTicks(9487), "uploads/members/photo_m352_3ac783e35b304cc98729446db1167f19_2512214.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 353,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 870, DateTimeKind.Utc).AddTicks(813), new DateTime(2026, 3, 15, 16, 15, 47, 870, DateTimeKind.Utc).AddTicks(825), "uploads/members/photo_m353_a32f36fbf9b248539fa3671b0c8d7193_2512215.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 354,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 880, DateTimeKind.Utc).AddTicks(6559), new DateTime(2026, 3, 15, 16, 15, 47, 880, DateTimeKind.Utc).AddTicks(6573), "uploads/members/photo_m354_04ea1d720ed14c00b118fe9e62f35b08_2512216.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 355,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 893, DateTimeKind.Utc).AddTicks(4802), "uploads/members/photo_m355_530eec6e5c874293801396514e1900d1_2512217.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 356,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 906, DateTimeKind.Utc).AddTicks(8658), new DateTime(2026, 3, 15, 16, 15, 47, 906, DateTimeKind.Utc).AddTicks(8672), "uploads/members/photo_m356_4d946ad0998c400da72c5ff71339b8eb_2512218.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 357,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 927, DateTimeKind.Utc).AddTicks(2363), new DateTime(2026, 3, 15, 16, 15, 47, 927, DateTimeKind.Utc).AddTicks(2371), "uploads/members/photo_m357_020ac5daa9ee4f6387f5af20c86c108c_2512219.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 358,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 941, DateTimeKind.Utc).AddTicks(226), new DateTime(2026, 3, 15, 16, 15, 47, 941, DateTimeKind.Utc).AddTicks(234), "uploads/members/photo_m358_5d63fea9d5a748c0822df7446c178343_2512220.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 359,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 950, DateTimeKind.Utc).AddTicks(4821), new DateTime(2026, 3, 15, 16, 15, 47, 950, DateTimeKind.Utc).AddTicks(4833), "uploads/members/photo_m359_f99e8ab1bdcc45c2991cf065ffa64457_2512221.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 360,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 963, DateTimeKind.Utc).AddTicks(4573), new DateTime(2026, 3, 15, 16, 15, 47, 963, DateTimeKind.Utc).AddTicks(4586), "uploads/members/photo_m360_b2455302f48540dda4f5aad6f8517ab2_2512222.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 361,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 968, DateTimeKind.Utc).AddTicks(3944), new DateTime(2026, 3, 15, 16, 15, 47, 968, DateTimeKind.Utc).AddTicks(3954), "uploads/members/photo_m361_ca75b3c8a1444fb8aef7071560d9fe90_2512223.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 362,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 47, 983, DateTimeKind.Utc).AddTicks(4186), new DateTime(2026, 3, 15, 16, 15, 47, 983, DateTimeKind.Utc).AddTicks(4194), "uploads/members/photo_m362_78584f510b3a4bf7a4598c90bb8aeda0_2512224.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 363,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 40, DateTimeKind.Utc).AddTicks(9762), new DateTime(2026, 3, 15, 16, 15, 48, 40, DateTimeKind.Utc).AddTicks(9771), "uploads/members/photo_m363_9e37db05dcf94bcc9e204235e6173833_2512228.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 364,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 48, DateTimeKind.Utc).AddTicks(1787), new DateTime(2026, 3, 15, 16, 15, 48, 48, DateTimeKind.Utc).AddTicks(1795), "uploads/members/photo_m364_8b7c49aa6aaa438294db19b017b15b77_2512229.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 365,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 53, DateTimeKind.Utc).AddTicks(6118), new DateTime(2026, 3, 15, 16, 15, 48, 53, DateTimeKind.Utc).AddTicks(6126), "uploads/members/photo_m365_cc031253af754a1bb8f6ae3de8e56ebc_2512230.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 366,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 81, DateTimeKind.Utc).AddTicks(5657), new DateTime(2026, 3, 15, 16, 15, 48, 81, DateTimeKind.Utc).AddTicks(5666), "uploads/members/photo_m366_b6bae0f1734549329a40610cd9785497_2512231.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 367,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 93, DateTimeKind.Utc).AddTicks(7545), new DateTime(2026, 3, 15, 16, 15, 48, 93, DateTimeKind.Utc).AddTicks(7552), "uploads/members/photo_m367_d89648edc0314d8382a78ba314b8d0cd_2512235.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 368,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 102, DateTimeKind.Utc).AddTicks(5587), "uploads/members/photo_m368_c5fa6c2d9eec41ea839b48e5918a7745_2512236.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 369,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 219, DateTimeKind.Utc).AddTicks(623), "uploads/members/photo_m369_ed50f3e519cb420aba3919ccfe44b9f7_2512237.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 370,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 238, DateTimeKind.Utc).AddTicks(4587), new DateTime(2026, 3, 15, 16, 15, 48, 238, DateTimeKind.Utc).AddTicks(4601), "uploads/members/photo_m370_f68d5cc6b6214ba584521e63d8d70e76_2512238.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 371,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 254, DateTimeKind.Utc).AddTicks(5637), new DateTime(2026, 3, 15, 16, 15, 48, 254, DateTimeKind.Utc).AddTicks(5649), "uploads/members/photo_m371_68c828d242784bd0b888c6775928c0a8_2512239.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 372,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 315, DateTimeKind.Utc).AddTicks(2032), new DateTime(2026, 3, 15, 16, 15, 48, 315, DateTimeKind.Utc).AddTicks(2041), "uploads/members/photo_m372_7e2023c8a307422895ea14b273f57ce6_2512240.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 373,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 320, DateTimeKind.Utc).AddTicks(6856), new DateTime(2026, 3, 15, 16, 15, 48, 320, DateTimeKind.Utc).AddTicks(6865), "uploads/members/photo_m373_e1c71ab7045a42e6a7cff362ea2f1679_2512243.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 374,
                columns: new[] { "AppliedDate", "LastUpdateDate" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 321, DateTimeKind.Utc).AddTicks(979), new DateTime(2026, 3, 15, 16, 15, 48, 321, DateTimeKind.Utc).AddTicks(983) });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 375,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 353, DateTimeKind.Utc).AddTicks(1244), new DateTime(2026, 3, 15, 16, 15, 48, 353, DateTimeKind.Utc).AddTicks(1252), "uploads/members/photo_m375_53aa3ee8ae8a4ba697e39bf379961d8a_2512246.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 376,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 388, DateTimeKind.Utc).AddTicks(5028), new DateTime(2026, 3, 15, 16, 15, 48, 388, DateTimeKind.Utc).AddTicks(5036), "uploads/members/photo_m376_3159f49e36dc4bd58c8fbeeb3ccdb74a_2512247.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 377,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 396, DateTimeKind.Utc).AddTicks(7936), new DateTime(2026, 3, 15, 16, 15, 48, 396, DateTimeKind.Utc).AddTicks(7944), "uploads/members/photo_m377_2eb679ab95d64a0a8732fbb8b4729c9f_2512248.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 378,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 404, DateTimeKind.Utc).AddTicks(2675), new DateTime(2026, 3, 15, 16, 15, 48, 404, DateTimeKind.Utc).AddTicks(2682), "uploads/members/photo_m378_9833249866c14121872dd95e1638f0c4_2512249.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 379,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 426, DateTimeKind.Utc).AddTicks(1213), new DateTime(2026, 3, 15, 16, 15, 48, 426, DateTimeKind.Utc).AddTicks(1224), "uploads/members/photo_m379_f9356477ad324f2aa908dee66b243ad0_2512250.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 380,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 430, DateTimeKind.Utc).AddTicks(4867), "uploads/members/photo_m380_d333ce80082849f1bfde74a7c9ea5ffa_2512251.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 381,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 440, DateTimeKind.Utc).AddTicks(3472), new DateTime(2026, 3, 15, 16, 15, 48, 440, DateTimeKind.Utc).AddTicks(3482), "uploads/members/photo_m381_dd694bad5fb54d119d722aeec3cb4b14_2512252.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 382,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 450, DateTimeKind.Utc).AddTicks(4287), new DateTime(2026, 3, 15, 16, 15, 48, 450, DateTimeKind.Utc).AddTicks(4295), "uploads/members/photo_m382_2788582c35e2493ab586ce64140ca040_2512253.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 383,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 460, DateTimeKind.Utc).AddTicks(5417), new DateTime(2026, 3, 15, 16, 15, 48, 460, DateTimeKind.Utc).AddTicks(5425), "uploads/members/photo_m383_d81d214d401140d8b807373f10372a63_2512254.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 384,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 471, DateTimeKind.Utc).AddTicks(2857), new DateTime(2026, 3, 15, 16, 15, 48, 471, DateTimeKind.Utc).AddTicks(2863), "uploads/members/photo_m384_5384c4e5eda2497bbc7da86cbc0a8f1e_2512255.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 385,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 502, DateTimeKind.Utc).AddTicks(265), new DateTime(2026, 3, 15, 16, 15, 48, 502, DateTimeKind.Utc).AddTicks(277), "uploads/members/photo_m385_7a4fa9cc57fb483ba7b779208fe21e92_2512256.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 386,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 506, DateTimeKind.Utc).AddTicks(8457), new DateTime(2026, 3, 15, 16, 15, 48, 506, DateTimeKind.Utc).AddTicks(8471), "uploads/members/photo_m386_906f054f90c4407fa98bb90e09eb6905_2512257.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 387,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 514, DateTimeKind.Utc).AddTicks(2457), new DateTime(2026, 3, 15, 16, 15, 48, 514, DateTimeKind.Utc).AddTicks(2469), "uploads/members/photo_m387_6724130200f34100b18aa49b17ddf97d_2512258.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 388,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 526, DateTimeKind.Utc).AddTicks(2786), new DateTime(2026, 3, 15, 16, 15, 48, 526, DateTimeKind.Utc).AddTicks(2794), "uploads/members/photo_m388_2d35e1cf2cda4fe7a29ce73764814acd_2512259.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 389,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 544, DateTimeKind.Utc).AddTicks(2747), "uploads/members/photo_m389_7fb0a164050045a0b3e17fe67f08d228_2512260.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 390,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 584, DateTimeKind.Utc).AddTicks(9671), new DateTime(2026, 3, 15, 16, 15, 48, 584, DateTimeKind.Utc).AddTicks(9679), "uploads/members/photo_m390_8334d63892a3414998ac273822048a04_2512261.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 391,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 592, DateTimeKind.Utc).AddTicks(1209), new DateTime(2026, 3, 15, 16, 15, 48, 592, DateTimeKind.Utc).AddTicks(1218), "uploads/members/photo_m391_0843bab83b194fd98926088100b3a515_2512262.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 392,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 618, DateTimeKind.Utc).AddTicks(9622), "uploads/members/photo_m392_c756af8eff10436a95917a94e27380a7_2512263.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 393,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 638, DateTimeKind.Utc).AddTicks(8842), new DateTime(2026, 3, 15, 16, 15, 48, 638, DateTimeKind.Utc).AddTicks(8853), "uploads/members/photo_m393_ce3414b21b2d4d95a2dd0c6c3f0ca3fa_2512264.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 394,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 645, DateTimeKind.Utc).AddTicks(2605), new DateTime(2026, 3, 15, 16, 15, 48, 645, DateTimeKind.Utc).AddTicks(2614), "uploads/members/photo_m394_28a9510c4a554cbb9db8d7ace71c92ef_2512265.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 395,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 653, DateTimeKind.Utc).AddTicks(7657), new DateTime(2026, 3, 15, 16, 15, 48, 653, DateTimeKind.Utc).AddTicks(7665), "uploads/members/photo_m395_aa358f076461418ba23c0b13fb4cf9a7_2512266.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 396,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 663, DateTimeKind.Utc).AddTicks(7515), new DateTime(2026, 3, 15, 16, 15, 48, 663, DateTimeKind.Utc).AddTicks(7523), "uploads/members/photo_m396_0f44a359140a41ed939151903f616a3f_2512267.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 397,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 677, DateTimeKind.Utc).AddTicks(1124), new DateTime(2026, 3, 15, 16, 15, 48, 677, DateTimeKind.Utc).AddTicks(1138), "uploads/members/photo_m397_2108ccd54f23438584d5c490fb42e827_2512268.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 398,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 681, DateTimeKind.Utc).AddTicks(6767), new DateTime(2026, 3, 15, 16, 15, 48, 681, DateTimeKind.Utc).AddTicks(6776), "uploads/members/photo_m398_5906c8c76eac4630996eb7f563060199_2512269.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 399,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 689, DateTimeKind.Utc).AddTicks(2624), new DateTime(2026, 3, 15, 16, 15, 48, 689, DateTimeKind.Utc).AddTicks(2632), "uploads/members/photo_m399_f5d8183ec49a4c35a1c8320c9eea5331_2512272.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 697, DateTimeKind.Utc).AddTicks(7515), new DateTime(2026, 3, 15, 16, 15, 48, 697, DateTimeKind.Utc).AddTicks(7523), "uploads/members/photo_m400_397055f2ffe24494890c6f31ba7c788d_2512273.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 705, DateTimeKind.Utc).AddTicks(6286), new DateTime(2026, 3, 15, 16, 15, 48, 705, DateTimeKind.Utc).AddTicks(6293), "uploads/members/photo_m401_8dc8c04bd0ec44e8936e9ced2e9c86ee_2512275.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 716, DateTimeKind.Utc).AddTicks(7411), new DateTime(2026, 3, 15, 16, 15, 48, 716, DateTimeKind.Utc).AddTicks(7418), "uploads/members/photo_m402_0cd4ae15c20d4877b0791c9c9585b577_2512278.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 729, DateTimeKind.Utc).AddTicks(2986), new DateTime(2026, 3, 15, 16, 15, 48, 729, DateTimeKind.Utc).AddTicks(3001), "uploads/members/photo_m403_442e48d80d674991b69b35b1b5c8e5de_2512279.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 743, DateTimeKind.Utc).AddTicks(5698), "uploads/members/photo_m404_14f99ff19f9943c3b0da807a00dacc49_2512280.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 783, DateTimeKind.Utc).AddTicks(7795), "uploads/members/photo_m405_553a22833f4e4da9bd22585d7910d138_2512281.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 813, DateTimeKind.Utc).AddTicks(1331), new DateTime(2026, 3, 15, 16, 15, 48, 813, DateTimeKind.Utc).AddTicks(1338), "uploads/members/photo_m406_be7d786cd32f4957bb1fade677cb45b8_2512283.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 407,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 853, DateTimeKind.Utc).AddTicks(2576), new DateTime(2026, 3, 15, 16, 15, 48, 853, DateTimeKind.Utc).AddTicks(2584), "uploads/members/photo_m407_2f8f2cb414d24f3a9f0ec29f6749c4f4_2512284.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 408,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 860, DateTimeKind.Utc).AddTicks(5383), new DateTime(2026, 3, 15, 16, 15, 48, 860, DateTimeKind.Utc).AddTicks(5391), "uploads/members/photo_m408_73b6b37964df4a7f96c5fe58f2397dac_2512285.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 409,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 872, DateTimeKind.Utc).AddTicks(4936), new DateTime(2026, 3, 15, 16, 15, 48, 872, DateTimeKind.Utc).AddTicks(4945), "uploads/members/photo_m409_15ea535393124f3fbf8b93f54877103a_2512286.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 410,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 883, DateTimeKind.Utc).AddTicks(8742), "uploads/members/photo_m410_fb88d1f97f784951acba6efb25bffe99_2512287.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 411,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 48, 916, DateTimeKind.Utc).AddTicks(6035), new DateTime(2026, 3, 15, 16, 15, 48, 916, DateTimeKind.Utc).AddTicks(6051), "uploads/members/photo_m411_64a395338a7d4183a6a3e57ef7c4c841_2512288.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 412,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 6, DateTimeKind.Utc).AddTicks(7581), new DateTime(2026, 3, 15, 16, 15, 49, 6, DateTimeKind.Utc).AddTicks(7589), "uploads/members/photo_m412_63f5f1f5285044829553af621a7f69c2_2512290.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 413,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 14, DateTimeKind.Utc).AddTicks(309), "uploads/members/photo_m413_2afbb86e78ff45c485d629901ba6f73b_2512291.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 414,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 37, DateTimeKind.Utc).AddTicks(9844), new DateTime(2026, 3, 15, 16, 15, 49, 37, DateTimeKind.Utc).AddTicks(9853), "uploads/members/photo_m414_985c6699523e4aa1b7962262600f26b9_2512292.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 415,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 61, DateTimeKind.Utc).AddTicks(7047), new DateTime(2026, 3, 15, 16, 15, 49, 61, DateTimeKind.Utc).AddTicks(7062), "uploads/members/photo_m415_2cefca6995dd4a31a426daf89fb1f1d3_2512293.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 416,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 83, DateTimeKind.Utc).AddTicks(2792), new DateTime(2026, 3, 15, 16, 15, 49, 83, DateTimeKind.Utc).AddTicks(2802), "uploads/members/photo_m416_eb577ab0466f4907ad2d28b45f01918c_2512294.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 417,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 94, DateTimeKind.Utc).AddTicks(4189), new DateTime(2026, 3, 15, 16, 15, 49, 94, DateTimeKind.Utc).AddTicks(4197), "uploads/members/photo_m417_e7f5bae786f045de83cfc3ff20fff34d_2512295.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 418,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 121, DateTimeKind.Utc).AddTicks(5612), "uploads/members/photo_m418_7509ad1f18344addb73247ebc3987721_2512296.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 419,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 188, DateTimeKind.Utc).AddTicks(7288), "uploads/members/photo_m419_b479fcb192584366afe559322273385f_2512298.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 420,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 214, DateTimeKind.Utc).AddTicks(2204), new DateTime(2026, 3, 15, 16, 15, 49, 214, DateTimeKind.Utc).AddTicks(2218), "uploads/members/photo_m420_508ae08d7f1b4b8cbf3062bc0162a390_2512299.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 421,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 224, DateTimeKind.Utc).AddTicks(4005), new DateTime(2026, 3, 15, 16, 15, 49, 224, DateTimeKind.Utc).AddTicks(4014), "uploads/members/photo_m421_05e1ff6c02814a558d61b86b5def5b5d_2512300.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 422,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 233, DateTimeKind.Utc).AddTicks(5023), new DateTime(2026, 3, 15, 16, 15, 49, 233, DateTimeKind.Utc).AddTicks(5032), "uploads/members/photo_m422_7c6b4c4b36da49fea167b330f8410760_2512301.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 423,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 255, DateTimeKind.Utc).AddTicks(5075), new DateTime(2026, 3, 15, 16, 15, 49, 255, DateTimeKind.Utc).AddTicks(5086), "uploads/members/photo_m423_184b108bebfc4fa9a0f6d37015416d93_2512302.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 424,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 277, DateTimeKind.Utc).AddTicks(9634), new DateTime(2026, 3, 15, 16, 15, 49, 277, DateTimeKind.Utc).AddTicks(9647), "uploads/members/photo_m424_2847ba534a4b4a93b7a81b46989431c6_2512303.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 425,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 295, DateTimeKind.Utc).AddTicks(7001), new DateTime(2026, 3, 15, 16, 15, 49, 295, DateTimeKind.Utc).AddTicks(7008), "uploads/members/photo_m425_63cd45ecc36f4e72bb1de5089e96b251_2512305.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 426,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 306, DateTimeKind.Utc).AddTicks(9512), new DateTime(2026, 3, 15, 16, 15, 49, 306, DateTimeKind.Utc).AddTicks(9521), "uploads/members/photo_m426_a29faab687014c808f85eda9a868bfab_2512306.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 427,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 320, DateTimeKind.Utc).AddTicks(8016), new DateTime(2026, 3, 15, 16, 15, 49, 320, DateTimeKind.Utc).AddTicks(8024), "uploads/members/photo_m427_1cf8c2e2d26e45edaafbad6ad062a9a6_2512308.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 428,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 360, DateTimeKind.Utc).AddTicks(7274), new DateTime(2026, 3, 15, 16, 15, 49, 360, DateTimeKind.Utc).AddTicks(7282), "uploads/members/photo_m428_9e0ce65405e34018964cac722dfa57fd_2512309.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 429,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 436, DateTimeKind.Utc).AddTicks(8196), new DateTime(2026, 3, 15, 16, 15, 49, 436, DateTimeKind.Utc).AddTicks(8204), "uploads/members/photo_m429_22cc31b29c7944388d22267a75964418_2512311.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 430,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 486, DateTimeKind.Utc).AddTicks(4304), new DateTime(2026, 3, 15, 16, 15, 49, 486, DateTimeKind.Utc).AddTicks(4316), "uploads/members/photo_m430_5277527ec8ed414497c7a6186da969b4_2512312.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 431,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 501, DateTimeKind.Utc).AddTicks(8225), new DateTime(2026, 3, 15, 16, 15, 49, 501, DateTimeKind.Utc).AddTicks(8233), "uploads/members/photo_m431_a481a3b8b9864867993025b35b3c50a3_2512313.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 432,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 527, DateTimeKind.Utc).AddTicks(7206), "uploads/members/photo_m432_bff1c26e51ff41e18d83a9959725fe09_2512314.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 433,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 540, DateTimeKind.Utc).AddTicks(222), new DateTime(2026, 3, 15, 16, 15, 49, 540, DateTimeKind.Utc).AddTicks(237), "uploads/members/photo_m433_6572a63c00bf4ac6bdf0900d5587ae94_2512315.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 434,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 579, DateTimeKind.Utc).AddTicks(8955), new DateTime(2026, 3, 15, 16, 15, 49, 579, DateTimeKind.Utc).AddTicks(8964), "uploads/members/photo_m434_99aae2a2fde64d09b87b0b7f040bbd9a_2512316.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 435,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 625, DateTimeKind.Utc).AddTicks(7688), new DateTime(2026, 3, 15, 16, 15, 49, 625, DateTimeKind.Utc).AddTicks(7696), "uploads/members/photo_m435_610d90afb14b46c9bd7e7bac3f7d9973_2512319.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 436,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 657, DateTimeKind.Utc).AddTicks(6376), new DateTime(2026, 3, 15, 16, 15, 49, 657, DateTimeKind.Utc).AddTicks(6386), "uploads/members/photo_m436_8cef6739889546d59c0746935e39555e_2512320.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 437,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 674, DateTimeKind.Utc).AddTicks(6126), new DateTime(2026, 3, 15, 16, 15, 49, 674, DateTimeKind.Utc).AddTicks(6136), "uploads/members/photo_m437_a3ea0419ba3c48a18cf36e02b901ff8b_2512323.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 438,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 687, DateTimeKind.Utc).AddTicks(4779), new DateTime(2026, 3, 15, 16, 15, 49, 687, DateTimeKind.Utc).AddTicks(4788), "uploads/members/photo_m438_b19b30f8a62a4ca6a3d165dd4bc2e388_2512324.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 439,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 697, DateTimeKind.Utc).AddTicks(9179), new DateTime(2026, 3, 15, 16, 15, 49, 697, DateTimeKind.Utc).AddTicks(9187), "uploads/members/photo_m439_c08e98cf47a846dd8e9ab53bfeb5b11f_2512325.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 440,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 748, DateTimeKind.Utc).AddTicks(4203), new DateTime(2026, 3, 15, 16, 15, 49, 748, DateTimeKind.Utc).AddTicks(4212), "uploads/members/photo_m440_32c8ed23f4a64c4ebc9dd47df0f315fc_2512326.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 441,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 767, DateTimeKind.Utc).AddTicks(8844), new DateTime(2026, 3, 15, 16, 15, 49, 767, DateTimeKind.Utc).AddTicks(8852), "uploads/members/photo_m441_bcd4e2b4a3f14746b38fb6da9aa908c7_2512333.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 442,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 784, DateTimeKind.Utc).AddTicks(1478), "uploads/members/photo_m442_953ea013b9d1431fb42a6a7ab7886998_2512334.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 443,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 821, DateTimeKind.Utc).AddTicks(4606), new DateTime(2026, 3, 15, 16, 15, 49, 821, DateTimeKind.Utc).AddTicks(4613), "uploads/members/photo_m443_c17d841f58914fafaef504a1d480e49b_2512336.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 444,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 826, DateTimeKind.Utc).AddTicks(6767), new DateTime(2026, 3, 15, 16, 15, 49, 826, DateTimeKind.Utc).AddTicks(6775), "uploads/members/photo_m444_08af1a207b5b44128faa83f4bd247174_2512337.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 445,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 831, DateTimeKind.Utc).AddTicks(2608), "uploads/members/photo_m445_a3b742cc139f40a4a9c9b20ba63209ee_2512338.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 446,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 844, DateTimeKind.Utc).AddTicks(6094), new DateTime(2026, 3, 15, 16, 15, 49, 844, DateTimeKind.Utc).AddTicks(6102), "uploads/members/photo_m446_1eefbd17284c492583bc983ac57c3384_2512339.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 447,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 901, DateTimeKind.Utc).AddTicks(2892), new DateTime(2026, 3, 15, 16, 15, 49, 901, DateTimeKind.Utc).AddTicks(2902), "uploads/members/photo_m447_224cae68abbc43c6998aba503e31e55c_2512340.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 448,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 905, DateTimeKind.Utc).AddTicks(6699), new DateTime(2026, 3, 15, 16, 15, 49, 905, DateTimeKind.Utc).AddTicks(6706), "uploads/members/photo_m448_2776c658d4de4ca788886a990eb5cd5f_2512341.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 449,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 951, DateTimeKind.Utc).AddTicks(3412), new DateTime(2026, 3, 15, 16, 15, 49, 951, DateTimeKind.Utc).AddTicks(3419), "uploads/members/photo_m449_1cc98ccec1754ff6ad29ba36cbf4d116_2512342.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 450,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 956, DateTimeKind.Utc).AddTicks(1128), "uploads/members/photo_m450_1723340e879849cea032bd6762bc0d7d_2512344.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 451,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 49, 979, DateTimeKind.Utc).AddTicks(5067), new DateTime(2026, 3, 15, 16, 15, 49, 979, DateTimeKind.Utc).AddTicks(5082), "uploads/members/photo_m451_a5bf1fea3b8f46d587f0eb5792c4b375_2512345.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 452,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 15, DateTimeKind.Utc).AddTicks(1988), new DateTime(2026, 3, 15, 16, 15, 50, 15, DateTimeKind.Utc).AddTicks(1996), "uploads/members/photo_m452_d1c541260ef04bd99d5c4567fae407f5_2512346.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 453,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 26, DateTimeKind.Utc).AddTicks(597), new DateTime(2026, 3, 15, 16, 15, 50, 26, DateTimeKind.Utc).AddTicks(606), "uploads/members/photo_m453_14285372f62e45c88bac039ba8f83c82_2512347.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 454,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 67, DateTimeKind.Utc).AddTicks(7908), new DateTime(2026, 3, 15, 16, 15, 50, 67, DateTimeKind.Utc).AddTicks(7922), "uploads/members/photo_m454_0db7b1dfb0d8484a9aceae0a3d05d091_2512348.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 455,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 86, DateTimeKind.Utc).AddTicks(5805), "uploads/members/photo_m455_ae341dfae19848e7a0cbde1a26dcf423_2512350.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 456,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 97, DateTimeKind.Utc).AddTicks(484), new DateTime(2026, 3, 15, 16, 15, 50, 97, DateTimeKind.Utc).AddTicks(497), "uploads/members/photo_m456_1bd3b66a9b144e89aacd62b8eb5160e0_2512351.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 457,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 175, DateTimeKind.Utc).AddTicks(2101), new DateTime(2026, 3, 15, 16, 15, 50, 175, DateTimeKind.Utc).AddTicks(2109), "uploads/members/photo_m457_53fe8d8051ea4dcebdce6604a59c324f_2512352.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 458,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 232, DateTimeKind.Utc).AddTicks(4022), "uploads/members/photo_m458_5908969388934ee3be8b472ffbc651f7_2512353.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 459,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 305, DateTimeKind.Utc).AddTicks(7036), new DateTime(2026, 3, 15, 16, 15, 50, 305, DateTimeKind.Utc).AddTicks(7047), "uploads/members/photo_m459_8651e3cb3be247e8a1b749c03a4279c9_2512354.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 460,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 428, DateTimeKind.Utc).AddTicks(786), new DateTime(2026, 3, 15, 16, 15, 50, 428, DateTimeKind.Utc).AddTicks(793), "uploads/members/photo_m460_26800c1c73424fd395a48458b233a0fe_2512355.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 461,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 438, DateTimeKind.Utc).AddTicks(5796), new DateTime(2026, 3, 15, 16, 15, 50, 438, DateTimeKind.Utc).AddTicks(5805), "uploads/members/photo_m461_7821cbf741154e36a0abc3c1bf4035a9_2512357.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 462,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 442, DateTimeKind.Utc).AddTicks(1876), new DateTime(2026, 3, 15, 16, 15, 50, 442, DateTimeKind.Utc).AddTicks(1886), "uploads/members/photo_m462_502c7ca27e0d463aa89ac3a9634ab9a7_2512362.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 463,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 448, DateTimeKind.Utc).AddTicks(6822), "uploads/members/photo_m463_b51c58b484784c928e77da43a14f91d1_2512363.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 464,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 480, DateTimeKind.Utc).AddTicks(6904), new DateTime(2026, 3, 15, 16, 15, 50, 480, DateTimeKind.Utc).AddTicks(6918), "uploads/members/photo_m464_ca5c7633698c455a894c263d03fe22b4_2512364.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 465,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 495, DateTimeKind.Utc).AddTicks(5477), new DateTime(2026, 3, 15, 16, 15, 50, 495, DateTimeKind.Utc).AddTicks(5488), "uploads/members/photo_m465_0a9aa3d38d124a16b71e57b07a4bcac5_2512365.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 466,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 552, DateTimeKind.Utc).AddTicks(8817), new DateTime(2026, 3, 15, 16, 15, 50, 552, DateTimeKind.Utc).AddTicks(8824), "uploads/members/photo_m466_0960060006d541b4b0e64260a939d656_2512366.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 467,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 566, DateTimeKind.Utc).AddTicks(4017), new DateTime(2026, 3, 15, 16, 15, 50, 566, DateTimeKind.Utc).AddTicks(4026), "uploads/members/photo_m467_6fb8941107664c189cf7cd36bccbd403_2512367.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 468,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 579, DateTimeKind.Utc).AddTicks(5852), new DateTime(2026, 3, 15, 16, 15, 50, 579, DateTimeKind.Utc).AddTicks(5863), "uploads/members/photo_m468_6729adfe691c4819b2720b3565220190_2512368.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 469,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 594, DateTimeKind.Utc).AddTicks(8496), new DateTime(2026, 3, 15, 16, 15, 50, 594, DateTimeKind.Utc).AddTicks(8509), "uploads/members/photo_m469_284fa80eefe34e369fff783c8226956a_2512369.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 470,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 611, DateTimeKind.Utc).AddTicks(7755), new DateTime(2026, 3, 15, 16, 15, 50, 611, DateTimeKind.Utc).AddTicks(7763), "uploads/members/photo_m470_a8a9392773714116bea2e6b6d2066021_2512370.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 471,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 688, DateTimeKind.Utc).AddTicks(4254), new DateTime(2026, 3, 15, 16, 15, 50, 688, DateTimeKind.Utc).AddTicks(4262), "uploads/members/photo_m471_f862e84682a4484fb8964fff4ae4fc1d_2512371.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 472,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 784, DateTimeKind.Utc).AddTicks(9263), "uploads/members/photo_m472_b89d258222a442318d1f479ada8e80a7_2512372.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 473,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 799, DateTimeKind.Utc).AddTicks(6717), new DateTime(2026, 3, 15, 16, 15, 50, 799, DateTimeKind.Utc).AddTicks(6725), "uploads/members/photo_m473_020a0e9e1cff42939841335ef00d3593_2512373.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 474,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 824, DateTimeKind.Utc).AddTicks(6658), "uploads/members/photo_m474_dccbbc46f1d64e9197479d6eeaa1fd48_2512374.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 475,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 836, DateTimeKind.Utc).AddTicks(2987), new DateTime(2026, 3, 15, 16, 15, 50, 836, DateTimeKind.Utc).AddTicks(2995), "uploads/members/photo_m475_32e6b097605843a7a03d399e15d669cb_2512376.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 476,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 913, DateTimeKind.Utc).AddTicks(5282), new DateTime(2026, 3, 15, 16, 15, 50, 913, DateTimeKind.Utc).AddTicks(5291), "uploads/members/photo_m476_bdc60bbc26c84c71b57cffdccc387bca_2512377.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 477,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 929, DateTimeKind.Utc).AddTicks(3296), new DateTime(2026, 3, 15, 16, 15, 50, 929, DateTimeKind.Utc).AddTicks(3304), "uploads/members/photo_m477_419b49f51ab94d068543a90e80e01fe0_2512378.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 478,
                column: "PhotoPath",
                value: "uploads/members/photo_m478_7453a959b0e54ae4840c80386c51b1d2_2512379.jpg");

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 479,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 50, 966, DateTimeKind.Utc).AddTicks(6397), new DateTime(2026, 3, 15, 16, 15, 50, 966, DateTimeKind.Utc).AddTicks(6405), "uploads/members/photo_m479_fb36179be6264a68bba638c8f699a1f2_2512380.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 480,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 39, DateTimeKind.Utc).AddTicks(115), new DateTime(2026, 3, 15, 16, 15, 51, 39, DateTimeKind.Utc).AddTicks(123), "uploads/members/photo_m480_4ff23cf8132e4103aa364e17711c4b59_2512381.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 481,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 60, DateTimeKind.Utc).AddTicks(6261), new DateTime(2026, 3, 15, 16, 15, 51, 60, DateTimeKind.Utc).AddTicks(6272), "uploads/members/photo_m481_f689cf31cf79439ab84b1f420153c638_2512383.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 482,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 122, DateTimeKind.Utc).AddTicks(1341), "uploads/members/photo_m482_b20d5152210d412daf8995c391be4ed3_2512384.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 483,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 152, DateTimeKind.Utc).AddTicks(1064), new DateTime(2026, 3, 15, 16, 15, 51, 152, DateTimeKind.Utc).AddTicks(1074), "uploads/members/photo_m483_14923f02b44b4ca4b9bcfb2dd23ec697_2512385.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 484,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 165, DateTimeKind.Utc).AddTicks(1104), new DateTime(2026, 3, 15, 16, 15, 51, 165, DateTimeKind.Utc).AddTicks(1114), "uploads/members/photo_m484_c854aa005ffb4bb7a0118d148eb60257_2512386.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 485,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 172, DateTimeKind.Utc).AddTicks(8515), new DateTime(2026, 3, 15, 16, 15, 51, 172, DateTimeKind.Utc).AddTicks(8524), "uploads/members/photo_m485_f31595ea85ca495d95f2f1d839d64506_2512387.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 486,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 181, DateTimeKind.Utc).AddTicks(3334), new DateTime(2026, 3, 15, 16, 15, 51, 181, DateTimeKind.Utc).AddTicks(3343), "uploads/members/photo_m486_ae00901aedf2407587a47b6fc4f593b6_2512388.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 487,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 189, DateTimeKind.Utc).AddTicks(5379), new DateTime(2026, 3, 15, 16, 15, 51, 189, DateTimeKind.Utc).AddTicks(5387), "uploads/members/photo_m487_0ecd0edcda634df6b37410bd40bbeb0b_2512389.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 488,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 210, DateTimeKind.Utc).AddTicks(8053), "uploads/members/photo_m488_e5a20efb322a4dc7b8c61501f0ee526d_2512390.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 489,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 231, DateTimeKind.Utc).AddTicks(96), new DateTime(2026, 3, 15, 16, 15, 51, 231, DateTimeKind.Utc).AddTicks(106), "uploads/members/photo_m489_feb9ececd5784ad68c33538a6576e757_2512391.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 490,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 236, DateTimeKind.Utc).AddTicks(5009), new DateTime(2026, 3, 15, 16, 15, 51, 236, DateTimeKind.Utc).AddTicks(5017), "uploads/members/photo_m490_d0a466cba9eb43a09249df1b85813af7_2512392.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 491,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 245, DateTimeKind.Utc).AddTicks(304), new DateTime(2026, 3, 15, 16, 15, 51, 245, DateTimeKind.Utc).AddTicks(311), "uploads/members/photo_m491_670b72bf9ba94bc798fb89ee7bf4a9a7_2512393.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 492,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 291, DateTimeKind.Utc).AddTicks(3688), "uploads/members/photo_m492_13eb0fab86124b67a40638784c507240_2512394.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 493,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 301, DateTimeKind.Utc).AddTicks(605), new DateTime(2026, 3, 15, 16, 15, 51, 301, DateTimeKind.Utc).AddTicks(613), "uploads/members/photo_m493_5aa4df5049714a3da6766853d1449505_2512395.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 494,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 304, DateTimeKind.Utc).AddTicks(8815), new DateTime(2026, 3, 15, 16, 15, 51, 304, DateTimeKind.Utc).AddTicks(8823), "uploads/members/photo_m494_bb874016b77d4620a07edee3bdc52b04_2512396.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 495,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 333, DateTimeKind.Utc).AddTicks(8783), new DateTime(2026, 3, 15, 16, 15, 51, 333, DateTimeKind.Utc).AddTicks(8792), "uploads/members/photo_m495_8ab7aa4db44346edba493e9f2ebeb214_2512397.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 496,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 343, DateTimeKind.Utc).AddTicks(8292), new DateTime(2026, 3, 15, 16, 15, 51, 343, DateTimeKind.Utc).AddTicks(8301), "uploads/members/photo_m496_6ac29900f2ab49dfaabd9a1fca7d316d_2512398.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 497,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 350, DateTimeKind.Utc).AddTicks(7145), new DateTime(2026, 3, 15, 16, 15, 51, 350, DateTimeKind.Utc).AddTicks(7153), "uploads/members/photo_m497_2dbceffa1c884b149d376cddab16dfab_2512400.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 498,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 378, DateTimeKind.Utc).AddTicks(4745), new DateTime(2026, 3, 15, 16, 15, 51, 378, DateTimeKind.Utc).AddTicks(4753), "uploads/members/photo_m498_664cb21ba49a47d6ad400f5c6ed48d5b_2512401.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 499,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 409, DateTimeKind.Utc).AddTicks(7329), new DateTime(2026, 3, 15, 16, 15, 51, 409, DateTimeKind.Utc).AddTicks(7337), "uploads/members/photo_m499_fb2db7c8154b41dfb69def4899477e9a_2512402.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 416, DateTimeKind.Utc).AddTicks(1309), new DateTime(2026, 3, 15, 16, 15, 51, 416, DateTimeKind.Utc).AddTicks(1323), "uploads/members/photo_m500_4d7d22abe1f147ac8b7cf5b8338c771a_2512403.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 461, DateTimeKind.Utc).AddTicks(2322), "uploads/members/photo_m501_77ff494ece5b46b39b6ed6feaa5e450d_2512407.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 479, DateTimeKind.Utc).AddTicks(6946), new DateTime(2026, 3, 15, 16, 15, 51, 479, DateTimeKind.Utc).AddTicks(6956), "uploads/members/photo_m502_75137440aeb94404bbfd0cff945cb30d_2512408.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 494, DateTimeKind.Utc).AddTicks(2265), new DateTime(2026, 3, 15, 16, 15, 51, 494, DateTimeKind.Utc).AddTicks(2271), "uploads/members/photo_m503_766df9c1f3c64a3c85137b8e86a69e77_2512409.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 505, DateTimeKind.Utc).AddTicks(2778), new DateTime(2026, 3, 15, 16, 15, 51, 505, DateTimeKind.Utc).AddTicks(2789), "uploads/members/photo_m504_7a236c14b1224b58a36ff714c42b5978_2512413.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 516, DateTimeKind.Utc).AddTicks(2537), new DateTime(2026, 3, 15, 16, 15, 51, 516, DateTimeKind.Utc).AddTicks(2546), "uploads/members/photo_m505_9134cdbf4b2a40b5bebd2669a1e917d1_2512414.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 542, DateTimeKind.Utc).AddTicks(5977), new DateTime(2026, 3, 15, 16, 15, 51, 542, DateTimeKind.Utc).AddTicks(5986), "uploads/members/photo_m506_5843d67b9bcc481aa8d49fa05ed6fc21_2512415.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 651, DateTimeKind.Utc).AddTicks(2356), new DateTime(2026, 3, 15, 16, 15, 51, 651, DateTimeKind.Utc).AddTicks(2363), "uploads/members/photo_m507_9ddad71b1e0241ec8fab7f97a6783d43_2512416.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 508,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 696, DateTimeKind.Utc).AddTicks(4592), new DateTime(2026, 3, 15, 16, 15, 51, 696, DateTimeKind.Utc).AddTicks(4599), "uploads/members/photo_m508_1f0edf363f93406f803afca9f91b7bda_2512418.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 509,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 720, DateTimeKind.Utc).AddTicks(903), new DateTime(2026, 3, 15, 16, 15, 51, 720, DateTimeKind.Utc).AddTicks(911), "uploads/members/photo_m509_bceb105789d3401388eda1b6a6847763_2512419.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 510,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 727, DateTimeKind.Utc).AddTicks(9575), new DateTime(2026, 3, 15, 16, 15, 51, 727, DateTimeKind.Utc).AddTicks(9582), "uploads/members/photo_m510_5a0cdd8f34774864bff60dcbe45a9c02_2512420.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 511,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 741, DateTimeKind.Utc).AddTicks(5923), "uploads/members/photo_m511_dbaea9e1b88e4b19a0808f04c533bff8_2512421.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 512,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 758, DateTimeKind.Utc).AddTicks(8907), "uploads/members/photo_m512_6dd60890180c49f4a467adf674d3d634_2512422.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 513,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 764, DateTimeKind.Utc).AddTicks(9774), new DateTime(2026, 3, 15, 16, 15, 51, 764, DateTimeKind.Utc).AddTicks(9786), "uploads/members/photo_m513_f296e030505141f1bc3bee44730cae8e_2512423.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 514,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 770, DateTimeKind.Utc).AddTicks(2439), new DateTime(2026, 3, 15, 16, 15, 51, 770, DateTimeKind.Utc).AddTicks(2447), "uploads/members/photo_m514_950e6997841e44d084e3ce459f01647b_2512424.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 515,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 774, DateTimeKind.Utc).AddTicks(908), new DateTime(2026, 3, 15, 16, 15, 51, 774, DateTimeKind.Utc).AddTicks(916), "uploads/members/photo_m515_b080bf46e97a415c905a148a7d0afb65_2512425.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 516,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 802, DateTimeKind.Utc).AddTicks(8159), "uploads/members/photo_m516_acaa65ce82fd43208b32b3ed0558b008_2512426.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 517,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 832, DateTimeKind.Utc).AddTicks(8582), new DateTime(2026, 3, 15, 16, 15, 51, 832, DateTimeKind.Utc).AddTicks(8595), "uploads/members/photo_m517_4fe6970957614a7d919fdb7b5cc42861_2512427.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 518,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 837, DateTimeKind.Utc).AddTicks(6516), new DateTime(2026, 3, 15, 16, 15, 51, 837, DateTimeKind.Utc).AddTicks(6524), "uploads/members/photo_m518_465c979654394ac78b903a2c6f3694ac_2512428.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 519,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 843, DateTimeKind.Utc).AddTicks(6066), new DateTime(2026, 3, 15, 16, 15, 51, 843, DateTimeKind.Utc).AddTicks(6075), "uploads/members/photo_m519_31c2e4fb84c64d71985fec500abd2ebb_2512429.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 520,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 869, DateTimeKind.Utc).AddTicks(3424), new DateTime(2026, 3, 15, 16, 15, 51, 869, DateTimeKind.Utc).AddTicks(3438), "uploads/members/photo_m520_cc56cb9605a44ca08e7a6f5582150512_2512430.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 521,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 876, DateTimeKind.Utc).AddTicks(3178), new DateTime(2026, 3, 15, 16, 15, 51, 876, DateTimeKind.Utc).AddTicks(3188), "uploads/members/photo_m521_91af4751278a4d81aa3e15090a86c92b_2512431.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 522,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 895, DateTimeKind.Utc).AddTicks(609), new DateTime(2026, 3, 15, 16, 15, 51, 895, DateTimeKind.Utc).AddTicks(617), "uploads/members/photo_m522_01bf5d7d0250410c91716795cada640a_2512432.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 523,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 922, DateTimeKind.Utc).AddTicks(6655), new DateTime(2026, 3, 15, 16, 15, 51, 922, DateTimeKind.Utc).AddTicks(6666), "uploads/members/photo_m523_5cda6877f1244d75a4de089cca59b273_2512433.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 524,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 51, 949, DateTimeKind.Utc).AddTicks(8161), new DateTime(2026, 3, 15, 16, 15, 51, 949, DateTimeKind.Utc).AddTicks(8237), "uploads/members/photo_m524_24661bd272004daab4e205b8f4ea3a93_2512434.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 525,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 22, DateTimeKind.Utc).AddTicks(342), new DateTime(2026, 3, 15, 16, 15, 52, 22, DateTimeKind.Utc).AddTicks(351), "uploads/members/photo_m525_ded2978dbacf488c9db4737f927bf015_2512435.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 526,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 58, DateTimeKind.Utc).AddTicks(8795), new DateTime(2026, 3, 15, 16, 15, 52, 58, DateTimeKind.Utc).AddTicks(8804), "uploads/members/photo_m526_006edc6d4152481ab5dcf619d864cac1_2512436.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 527,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 66, DateTimeKind.Utc).AddTicks(6125), new DateTime(2026, 3, 15, 16, 15, 52, 66, DateTimeKind.Utc).AddTicks(6133), "uploads/members/photo_m527_9c73db2ee4f94822afc53872147c6c0b_2512438.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 528,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 89, DateTimeKind.Utc).AddTicks(5879), new DateTime(2026, 3, 15, 16, 15, 52, 89, DateTimeKind.Utc).AddTicks(5889), "uploads/members/photo_m528_9eafeee07e564848abd4fdcb48469970_2512439.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 529,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 95, DateTimeKind.Utc).AddTicks(9585), new DateTime(2026, 3, 15, 16, 15, 52, 95, DateTimeKind.Utc).AddTicks(9593), "uploads/members/photo_m529_67961a545f0a4cad8efd39530867636d_2512440.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 530,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 122, DateTimeKind.Utc).AddTicks(7549), new DateTime(2026, 3, 15, 16, 15, 52, 122, DateTimeKind.Utc).AddTicks(7557), "uploads/members/photo_m530_a0587546ae14414180b0e576b76f7126_2512441.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 531,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 139, DateTimeKind.Utc).AddTicks(1251), new DateTime(2026, 3, 15, 16, 15, 52, 139, DateTimeKind.Utc).AddTicks(1264), "uploads/members/photo_m531_189f0e6771e64c938d5290076409fbaf_2512442.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 532,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 142, DateTimeKind.Utc).AddTicks(921), new DateTime(2026, 3, 15, 16, 15, 52, 142, DateTimeKind.Utc).AddTicks(931), "uploads/members/photo_m532_b4250653857642cda3dc8a56de66f831_2512443.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 533,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 175, DateTimeKind.Utc).AddTicks(4568), new DateTime(2026, 3, 15, 16, 15, 52, 175, DateTimeKind.Utc).AddTicks(4576), "uploads/members/photo_m533_2d4e40862c0e4651afce28b030a08b60_2512444.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 534,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 183, DateTimeKind.Utc).AddTicks(8198), new DateTime(2026, 3, 15, 16, 15, 52, 183, DateTimeKind.Utc).AddTicks(8208), "uploads/members/photo_m534_be4b2ab4326545e7913b82ad74e34462_2512446.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 535,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 191, DateTimeKind.Utc).AddTicks(7542), "uploads/members/photo_m535_802cf9e06fac45a58f2fc4495af2ac38_2512447.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 536,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 199, DateTimeKind.Utc).AddTicks(3349), new DateTime(2026, 3, 15, 16, 15, 52, 199, DateTimeKind.Utc).AddTicks(3357), "uploads/members/photo_m536_1b11c8e5c8ac42589e2807b9ed4ff314_2512448.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 537,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 231, DateTimeKind.Utc).AddTicks(8241), new DateTime(2026, 3, 15, 16, 15, 52, 231, DateTimeKind.Utc).AddTicks(8255), "uploads/members/photo_m537_01610da9acfb40f6afa640aecb3cc93b_2512449.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 538,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 284, DateTimeKind.Utc).AddTicks(5994), new DateTime(2026, 3, 15, 16, 15, 52, 284, DateTimeKind.Utc).AddTicks(6001), "uploads/members/photo_m538_5fc1483a95c849f0954690a8dfab15fb_2512450.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 539,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 294, DateTimeKind.Utc).AddTicks(4883), new DateTime(2026, 3, 15, 16, 15, 52, 294, DateTimeKind.Utc).AddTicks(4891), "uploads/members/photo_m539_a0bfe581c15c4ca0862b2f97cd5b26e2_2512452.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 540,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 298, DateTimeKind.Utc).AddTicks(4917), new DateTime(2026, 3, 15, 16, 15, 52, 298, DateTimeKind.Utc).AddTicks(4924), "uploads/members/photo_m540_a925506b1e284bda8e4870a3311c58a2_2512454.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 541,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 339, DateTimeKind.Utc).AddTicks(4515), new DateTime(2026, 3, 15, 16, 15, 52, 339, DateTimeKind.Utc).AddTicks(4524), "uploads/members/photo_m541_149905b72e3e4e58b72f9ea73b67eb40_2512455.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 542,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 348, DateTimeKind.Utc).AddTicks(7711), new DateTime(2026, 3, 15, 16, 15, 52, 348, DateTimeKind.Utc).AddTicks(7719), "uploads/members/photo_m542_2df8be3f75994c61ab756f1e9d16d7b1_2512456.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 543,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 353, DateTimeKind.Utc).AddTicks(3251), new DateTime(2026, 3, 15, 16, 15, 52, 353, DateTimeKind.Utc).AddTicks(3259), "uploads/members/photo_m543_4da9ed3a12964eff8293400bdd3c35d0_2512457.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 544,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 364, DateTimeKind.Utc).AddTicks(5658), new DateTime(2026, 3, 15, 16, 15, 52, 364, DateTimeKind.Utc).AddTicks(5666), "uploads/members/photo_m544_c3e03e68d8984474a3806a84b1a727df_2512458.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 545,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 375, DateTimeKind.Utc).AddTicks(87), new DateTime(2026, 3, 15, 16, 15, 52, 375, DateTimeKind.Utc).AddTicks(96), "uploads/members/photo_m545_3190f6b94283469bb390ba437902e6ee_2512459.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 546,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 391, DateTimeKind.Utc).AddTicks(3792), new DateTime(2026, 3, 15, 16, 15, 52, 391, DateTimeKind.Utc).AddTicks(3804), "uploads/members/photo_m546_892f75489a104d1abdbb3d998c10bea8_2512461.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 547,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 399, DateTimeKind.Utc).AddTicks(1744), new DateTime(2026, 3, 15, 16, 15, 52, 399, DateTimeKind.Utc).AddTicks(1753), "uploads/members/photo_m547_add8418f577948f1854ffdabaa4f7cf8_2512462.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 548,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 405, DateTimeKind.Utc).AddTicks(1516), new DateTime(2026, 3, 15, 16, 15, 52, 405, DateTimeKind.Utc).AddTicks(1528), "uploads/members/photo_m548_c879c183769c4fecbcca33381f549453_2512463.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 549,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 431, DateTimeKind.Utc).AddTicks(8398), new DateTime(2026, 3, 15, 16, 15, 52, 431, DateTimeKind.Utc).AddTicks(8411), "uploads/members/photo_m549_06ac1ae22ebd47049cacb43c5baba56b_2512465.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 550,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 450, DateTimeKind.Utc).AddTicks(3577), new DateTime(2026, 3, 15, 16, 15, 52, 450, DateTimeKind.Utc).AddTicks(3585), "uploads/members/photo_m550_cf1e1b30bcf44afb8f939cbfc0e83ee9_2512466.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 551,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 462, DateTimeKind.Utc).AddTicks(896), new DateTime(2026, 3, 15, 16, 15, 52, 462, DateTimeKind.Utc).AddTicks(903), "uploads/members/photo_m551_f4139609c2d94541b6e4976be84c880d_2512467.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 552,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 502, DateTimeKind.Utc).AddTicks(1948), new DateTime(2026, 3, 15, 16, 15, 52, 502, DateTimeKind.Utc).AddTicks(1963), "uploads/members/photo_m552_6c463a652cfc439b901877553f58f560_2512468.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 553,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 549, DateTimeKind.Utc).AddTicks(8257), new DateTime(2026, 3, 15, 16, 15, 52, 549, DateTimeKind.Utc).AddTicks(8265), "uploads/members/photo_m553_f21be1dd97a3476285bf4b130058c9f7_2512469.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 554,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 560, DateTimeKind.Utc).AddTicks(7199), "uploads/members/photo_m554_10e603dd6af8419599ed06b394a158fb_2512471.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 555,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 586, DateTimeKind.Utc).AddTicks(951), new DateTime(2026, 3, 15, 16, 15, 52, 586, DateTimeKind.Utc).AddTicks(959), "uploads/members/photo_m555_5698dc9d7e8e445caf553b3e22f10d45_2512472.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 556,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 619, DateTimeKind.Utc).AddTicks(6448), new DateTime(2026, 3, 15, 16, 15, 52, 619, DateTimeKind.Utc).AddTicks(6456), "uploads/members/photo_m556_aa3fd2e31f63405daa59222b340379e9_2512473.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 557,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 640, DateTimeKind.Utc).AddTicks(3793), new DateTime(2026, 3, 15, 16, 15, 52, 640, DateTimeKind.Utc).AddTicks(3804), "uploads/members/photo_m557_92b88a4542344e6ebcee0ae064e4b0f1_2512474.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 558,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 649, DateTimeKind.Utc).AddTicks(8071), new DateTime(2026, 3, 15, 16, 15, 52, 649, DateTimeKind.Utc).AddTicks(8082), "uploads/members/photo_m558_b2a8a80bb7874468857854ee125b9938_2512475.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 559,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 674, DateTimeKind.Utc).AddTicks(8854), new DateTime(2026, 3, 15, 16, 15, 52, 674, DateTimeKind.Utc).AddTicks(8861), "uploads/members/photo_m559_18b9b22eb3b64ca8b02650267b485332_2512476.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 560,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 728, DateTimeKind.Utc).AddTicks(7787), new DateTime(2026, 3, 15, 16, 15, 52, 728, DateTimeKind.Utc).AddTicks(7796), "uploads/members/photo_m560_b0f77a9c639d4970adb13603c9848186_2512477.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 561,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 736, DateTimeKind.Utc).AddTicks(1651), new DateTime(2026, 3, 15, 16, 15, 52, 736, DateTimeKind.Utc).AddTicks(1659), "uploads/members/photo_m561_78de26ed763a460087da2ff64a8a5ffd_2512478.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 562,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 745, DateTimeKind.Utc).AddTicks(7338), new DateTime(2026, 3, 15, 16, 15, 52, 745, DateTimeKind.Utc).AddTicks(7353), "uploads/members/photo_m562_9d04e9e2b9bd4ae18f4b2a9095a45792_2512479.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 563,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 753, DateTimeKind.Utc).AddTicks(3514), new DateTime(2026, 3, 15, 16, 15, 52, 753, DateTimeKind.Utc).AddTicks(3523), "uploads/members/photo_m563_1aa691c38af642f0acb5d4e0dcf05542_2512480.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 564,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 767, DateTimeKind.Utc).AddTicks(8341), new DateTime(2026, 3, 15, 16, 15, 52, 767, DateTimeKind.Utc).AddTicks(8354), "uploads/members/photo_m564_319e3696242c46bba97808b91c70ccda_2512481.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 565,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 802, DateTimeKind.Utc).AddTicks(1994), new DateTime(2026, 3, 15, 16, 15, 52, 802, DateTimeKind.Utc).AddTicks(2002), "uploads/members/photo_m565_3630770a42074ce9b44102f32a8c69eb_2512482.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 566,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 819, DateTimeKind.Utc).AddTicks(6394), new DateTime(2026, 3, 15, 16, 15, 52, 819, DateTimeKind.Utc).AddTicks(6408), "uploads/members/photo_m566_04b0ad2b4ad649cba068e764c630b4c5_2512484.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 567,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 832, DateTimeKind.Utc).AddTicks(2225), new DateTime(2026, 3, 15, 16, 15, 52, 832, DateTimeKind.Utc).AddTicks(2232), "uploads/members/photo_m567_7229c6f5f3b8457fb794afcf53f6a0f0_2512485.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 568,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 841, DateTimeKind.Utc).AddTicks(1824), new DateTime(2026, 3, 15, 16, 15, 52, 841, DateTimeKind.Utc).AddTicks(1832), "uploads/members/photo_m568_6e33d63d69b3408ea3d87ba1bcd891b1_2512486.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 569,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 863, DateTimeKind.Utc).AddTicks(1359), new DateTime(2026, 3, 15, 16, 15, 52, 863, DateTimeKind.Utc).AddTicks(1367), "uploads/members/photo_m569_22e36a5157b24aa0b694bbc255bd38e3_2512487.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 570,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 868, DateTimeKind.Utc).AddTicks(1261), new DateTime(2026, 3, 15, 16, 15, 52, 868, DateTimeKind.Utc).AddTicks(1271), "uploads/members/photo_m570_9a60822d3de64d549f7c8f222d402c16_2512488.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 571,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 878, DateTimeKind.Utc).AddTicks(4273), new DateTime(2026, 3, 15, 16, 15, 52, 878, DateTimeKind.Utc).AddTicks(4282), "uploads/members/photo_m571_664373fe75a44cefb3d1bf79e4cd8a15_2512489.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 572,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 898, DateTimeKind.Utc).AddTicks(6499), new DateTime(2026, 3, 15, 16, 15, 52, 898, DateTimeKind.Utc).AddTicks(6512), "uploads/members/photo_m572_f8f55eaf4d004eefa5588a0ac26fe024_2512490.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 573,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 911, DateTimeKind.Utc).AddTicks(6432), "uploads/members/photo_m573_32445b27f2e44cddb420dd18efa8acf7_2512491.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 574,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 939, DateTimeKind.Utc).AddTicks(2033), new DateTime(2026, 3, 15, 16, 15, 52, 939, DateTimeKind.Utc).AddTicks(2048), "uploads/members/photo_m574_95d5c70f204e41cb98c69ea403c2152c_2512492.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 575,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 947, DateTimeKind.Utc).AddTicks(2602), new DateTime(2026, 3, 15, 16, 15, 52, 947, DateTimeKind.Utc).AddTicks(2611), "uploads/members/photo_m575_b17afefa43564e47a575e666e6773df9_2512493.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 576,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 958, DateTimeKind.Utc).AddTicks(1472), new DateTime(2026, 3, 15, 16, 15, 52, 958, DateTimeKind.Utc).AddTicks(1481), "uploads/members/photo_m576_2196a214bc8242a597e0d599a08b4ed0_2512494.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 577,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 969, DateTimeKind.Utc).AddTicks(94), new DateTime(2026, 3, 15, 16, 15, 52, 969, DateTimeKind.Utc).AddTicks(102), "uploads/members/photo_m577_76604ad7ec914ee4a4076d2a24be490d_2512495.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 578,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 983, DateTimeKind.Utc).AddTicks(4685), new DateTime(2026, 3, 15, 16, 15, 52, 983, DateTimeKind.Utc).AddTicks(4694), "uploads/members/photo_m578_d1bba1989f664580ad80425df4c7dfc3_2512496.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 579,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 52, 989, DateTimeKind.Utc).AddTicks(5942), new DateTime(2026, 3, 15, 16, 15, 52, 989, DateTimeKind.Utc).AddTicks(5959), "uploads/members/photo_m579_427bc8941cf24f90a85f5e801cb8ca33_2512497.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 580,
                column: "PhotoPath",
                value: "uploads/members/photo_m580_d2fb039c56eb4c7b84e9436c3e3f9ca5_2512498.jpeg");

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 581,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 97, DateTimeKind.Utc).AddTicks(7223), new DateTime(2026, 3, 15, 16, 15, 53, 97, DateTimeKind.Utc).AddTicks(7236), "uploads/members/photo_m581_e9a4ae1890884d6da3005d432d756fab_2512500.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 582,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 106, DateTimeKind.Utc).AddTicks(4078), new DateTime(2026, 3, 15, 16, 15, 53, 106, DateTimeKind.Utc).AddTicks(4087), "uploads/members/photo_m582_432ae413260940b6bfc00a9b2dd1c9aa_2512501.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 583,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 110, DateTimeKind.Utc).AddTicks(1748), new DateTime(2026, 3, 15, 16, 15, 53, 110, DateTimeKind.Utc).AddTicks(1757), "uploads/members/photo_m583_83451dc902794812896701a9ec5879df_2512502.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 584,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 153, DateTimeKind.Utc).AddTicks(7425), new DateTime(2026, 3, 15, 16, 15, 53, 153, DateTimeKind.Utc).AddTicks(7437), "uploads/members/photo_m584_0e135cd85d884ca391d005d0b2080a4c_2512503.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 585,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 193, DateTimeKind.Utc).AddTicks(2817), "uploads/members/photo_m585_1c8ea880dcff4f668768ba0fadccbfcc_2512504.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 586,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 202, DateTimeKind.Utc).AddTicks(7087), new DateTime(2026, 3, 15, 16, 15, 53, 202, DateTimeKind.Utc).AddTicks(7095), "uploads/members/photo_m586_125747a2b6a04847a169a2289c086d1e_2512505.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 587,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 276, DateTimeKind.Utc).AddTicks(4769), new DateTime(2026, 3, 15, 16, 15, 53, 276, DateTimeKind.Utc).AddTicks(4778), "uploads/members/photo_m587_9bbf35b0740d467fa1b7e6ebc051c326_2512506.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 588,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 284, DateTimeKind.Utc).AddTicks(3645), new DateTime(2026, 3, 15, 16, 15, 53, 284, DateTimeKind.Utc).AddTicks(3653), "uploads/members/photo_m588_38c0a6ce5a004a54977ddc729ebe2d9f_2512507.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 589,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 294, DateTimeKind.Utc).AddTicks(5302), new DateTime(2026, 3, 15, 16, 15, 53, 294, DateTimeKind.Utc).AddTicks(5311), "uploads/members/photo_m589_0034857ad732413089bcd74eba940030_2512508.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 590,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 317, DateTimeKind.Utc).AddTicks(7572), "uploads/members/photo_m590_7f244604afcd47f1aec2260205528d44_2512509.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 591,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 326, DateTimeKind.Utc).AddTicks(8911), new DateTime(2026, 3, 15, 16, 15, 53, 326, DateTimeKind.Utc).AddTicks(8917), "uploads/members/photo_m591_1dea21b45bde45e5a62e90aad3e7fc3a_2512510.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 592,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 361, DateTimeKind.Utc).AddTicks(6557), "uploads/members/photo_m592_1ca653551e38497cb30b4b387357af28_2512512.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 593,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 447, DateTimeKind.Utc).AddTicks(4902), new DateTime(2026, 3, 15, 16, 15, 53, 447, DateTimeKind.Utc).AddTicks(4919), "uploads/members/photo_m593_9fbbcade42284d1aa31058f372636748_2512513.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 594,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 481, DateTimeKind.Utc).AddTicks(2652), new DateTime(2026, 3, 15, 16, 15, 53, 481, DateTimeKind.Utc).AddTicks(2661), "uploads/members/photo_m594_25bfe29fbd194db796b0695b65744d2f_2512514.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 595,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 565, DateTimeKind.Utc).AddTicks(7367), new DateTime(2026, 3, 15, 16, 15, 53, 565, DateTimeKind.Utc).AddTicks(7378), "uploads/members/photo_m595_f117d5dffa2c4b6eb785d59c93ed5b3f_2512515.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 596,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 575, DateTimeKind.Utc).AddTicks(8745), new DateTime(2026, 3, 15, 16, 15, 53, 575, DateTimeKind.Utc).AddTicks(8759), "uploads/members/photo_m596_f984f4a0b5f744eb8e31a4775f41fc56_2512516.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 597,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 581, DateTimeKind.Utc).AddTicks(4923), "uploads/members/photo_m597_dfaaa9e66b704cf4aed780b915708e0a_2512517.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 598,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 591, DateTimeKind.Utc).AddTicks(4846), new DateTime(2026, 3, 15, 16, 15, 53, 591, DateTimeKind.Utc).AddTicks(4854), "uploads/members/photo_m598_c10edf1b569f4f2180a1ed723d0c70d8_2512518.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 599,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 600, DateTimeKind.Utc).AddTicks(4927), new DateTime(2026, 3, 15, 16, 15, 53, 600, DateTimeKind.Utc).AddTicks(4935), "uploads/members/photo_m599_6ecb21ec43c44d9fbac834b86c73106b_2512519.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 600,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 609, DateTimeKind.Utc).AddTicks(6405), new DateTime(2026, 3, 15, 16, 15, 53, 609, DateTimeKind.Utc).AddTicks(6414), "uploads/members/photo_m600_ee21de23059a43239e867178dc99b76a_2512520.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 601,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 620, DateTimeKind.Utc).AddTicks(889), new DateTime(2026, 3, 15, 16, 15, 53, 620, DateTimeKind.Utc).AddTicks(897), "uploads/members/photo_m601_6df99521d6994c2685642bb06fd87a91_2512521.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 602,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 659, DateTimeKind.Utc).AddTicks(4771), new DateTime(2026, 3, 15, 16, 15, 53, 659, DateTimeKind.Utc).AddTicks(4781), "uploads/members/photo_m602_097fd1c969d64fc78ecdda38f2a2703b_2512522.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 603,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 670, DateTimeKind.Utc).AddTicks(2139), new DateTime(2026, 3, 15, 16, 15, 53, 670, DateTimeKind.Utc).AddTicks(2149), "uploads/members/photo_m603_6e50862ef8b941dfacc5dcc8fcad31e7_2512524.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 604,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 681, DateTimeKind.Utc).AddTicks(8884), new DateTime(2026, 3, 15, 16, 15, 53, 681, DateTimeKind.Utc).AddTicks(8894), "uploads/members/photo_m604_889562c139da4bc09112efc80f692c1d_2512525.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 605,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 720, DateTimeKind.Utc).AddTicks(6227), new DateTime(2026, 3, 15, 16, 15, 53, 720, DateTimeKind.Utc).AddTicks(6236), "uploads/members/photo_m605_facf9c921dcc47448946e02442976b72_2512526.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 606,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 750, DateTimeKind.Utc).AddTicks(7088), "uploads/members/photo_m606_ac2559d7d773469d9e13d8e56aeb4b83_2512527.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 607,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 765, DateTimeKind.Utc).AddTicks(1346), new DateTime(2026, 3, 15, 16, 15, 53, 765, DateTimeKind.Utc).AddTicks(1355), "uploads/members/photo_m607_f5f55ac299844f869a75f6cf1b53e587_2512531.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 608,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 770, DateTimeKind.Utc).AddTicks(9275), new DateTime(2026, 3, 15, 16, 15, 53, 770, DateTimeKind.Utc).AddTicks(9283), "uploads/members/photo_m608_4f97310c65624319a2ef9a87934cdb22_2512532.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 609,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 792, DateTimeKind.Utc).AddTicks(3902), new DateTime(2026, 3, 15, 16, 15, 53, 792, DateTimeKind.Utc).AddTicks(3911), "uploads/members/photo_m609_845382da93594fa69630afc9e1f82ad8_2512533.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 610,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 800, DateTimeKind.Utc).AddTicks(7196), new DateTime(2026, 3, 15, 16, 15, 53, 800, DateTimeKind.Utc).AddTicks(7209), "uploads/members/photo_m610_2d83bf1779d348339c2f34af75ff0580_2512534.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 611,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 806, DateTimeKind.Utc).AddTicks(9536), new DateTime(2026, 3, 15, 16, 15, 53, 806, DateTimeKind.Utc).AddTicks(9548), "uploads/members/photo_m611_23ff0633f8254a079ca5bc650a061c47_2512537.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 612,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 816, DateTimeKind.Utc).AddTicks(4197), new DateTime(2026, 3, 15, 16, 15, 53, 816, DateTimeKind.Utc).AddTicks(4208), "uploads/members/photo_m612_c2c0efb7229a485e9940835bff83ce60_2512538.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 613,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 833, DateTimeKind.Utc).AddTicks(6937), new DateTime(2026, 3, 15, 16, 15, 53, 833, DateTimeKind.Utc).AddTicks(6948), "uploads/members/photo_m613_e55da7b6ae244d69bf5175818fcc28f5_2512539.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 614,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 851, DateTimeKind.Utc).AddTicks(5508), new DateTime(2026, 3, 15, 16, 15, 53, 851, DateTimeKind.Utc).AddTicks(5516), "uploads/members/photo_m614_6a439e1f6d5344408d10ac8985a5878a_2512541.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 615,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 872, DateTimeKind.Utc).AddTicks(5344), new DateTime(2026, 3, 15, 16, 15, 53, 872, DateTimeKind.Utc).AddTicks(5351), "uploads/members/photo_m615_b82cdae0469f4fe78a1f4980af857eb0_2512542.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 616,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 918, DateTimeKind.Utc).AddTicks(5087), new DateTime(2026, 3, 15, 16, 15, 53, 918, DateTimeKind.Utc).AddTicks(5095), "uploads/members/photo_m616_fd925dcf143a4bf0a2746606e8f388cf_2512545.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 617,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 925, DateTimeKind.Utc).AddTicks(9077), "uploads/members/photo_m617_92e3d5e7f1944fc1b2c310d6e3c1c11a_2512547.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 618,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 938, DateTimeKind.Utc).AddTicks(2035), new DateTime(2026, 3, 15, 16, 15, 53, 938, DateTimeKind.Utc).AddTicks(2048), "uploads/members/photo_m618_8fab223c4a274f96986a88e7dc7572aa_2512549.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 619,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 943, DateTimeKind.Utc).AddTicks(6003), new DateTime(2026, 3, 15, 16, 15, 53, 943, DateTimeKind.Utc).AddTicks(6015), "uploads/members/photo_m619_af3128af900944bca8da88490dac5f5c_2512550.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 620,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 957, DateTimeKind.Utc).AddTicks(9609), new DateTime(2026, 3, 15, 16, 15, 53, 957, DateTimeKind.Utc).AddTicks(9624), "uploads/members/photo_m620_223f522fd42748dc88667ebe076cc9c6_2512551.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 621,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 965, DateTimeKind.Utc).AddTicks(9096), new DateTime(2026, 3, 15, 16, 15, 53, 965, DateTimeKind.Utc).AddTicks(9102), "uploads/members/photo_m621_bd9c811f82bc43398fbafd448c4c8c44_2512552.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 622,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 53, 970, DateTimeKind.Utc).AddTicks(9771), new DateTime(2026, 3, 15, 16, 15, 53, 970, DateTimeKind.Utc).AddTicks(9779), "uploads/members/photo_m622_24eabd8c5464422d8bbf0a129f48314a_2512553.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 623,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 0, DateTimeKind.Utc).AddTicks(2763), "uploads/members/photo_m623_0f3e9a1fb7e94c5f9f097bd97357a35b_2512554.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 624,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 6, DateTimeKind.Utc).AddTicks(6735), new DateTime(2026, 3, 15, 16, 15, 54, 6, DateTimeKind.Utc).AddTicks(6747), "uploads/members/photo_m624_dcc830bd99c94cc9aa3e53662105ba3d_2512555.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 625,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 14, DateTimeKind.Utc).AddTicks(6521), new DateTime(2026, 3, 15, 16, 15, 54, 14, DateTimeKind.Utc).AddTicks(6529), "uploads/members/photo_m625_9d1e8bc0e7fe4249a0183f6738f8f08a_2512556.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 626,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 19, DateTimeKind.Utc).AddTicks(2529), new DateTime(2026, 3, 15, 16, 15, 54, 19, DateTimeKind.Utc).AddTicks(2537), "uploads/members/photo_m626_39f1cff8bd0b410f9da347b9198865ce_2512557.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 627,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 28, DateTimeKind.Utc).AddTicks(9102), new DateTime(2026, 3, 15, 16, 15, 54, 28, DateTimeKind.Utc).AddTicks(9118), "uploads/members/photo_m627_5443f962a0d44650ba7fe3abd8a1a333_2512558.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 628,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 45, DateTimeKind.Utc).AddTicks(1464), new DateTime(2026, 3, 15, 16, 15, 54, 45, DateTimeKind.Utc).AddTicks(1472), "uploads/members/photo_m628_c0fa40dcd5cf446497a742a9bd31d87b_2512559.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 629,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 80, DateTimeKind.Utc).AddTicks(5484), new DateTime(2026, 3, 15, 16, 15, 54, 80, DateTimeKind.Utc).AddTicks(5496), "uploads/members/photo_m629_6d544a60113647fdb4964571bca61989_2512560.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 630,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 102, DateTimeKind.Utc).AddTicks(5572), new DateTime(2026, 3, 15, 16, 15, 54, 102, DateTimeKind.Utc).AddTicks(5584), "uploads/members/photo_m630_a4bebf71e5924bfcb416f3d61e5966c6_2512561.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 631,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 111, DateTimeKind.Utc).AddTicks(2898), new DateTime(2026, 3, 15, 16, 15, 54, 111, DateTimeKind.Utc).AddTicks(2906), "uploads/members/photo_m631_5ff8a5b32d0c4645af70d5beffa88c27_2512562.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 632,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 132, DateTimeKind.Utc).AddTicks(6419), new DateTime(2026, 3, 15, 16, 15, 54, 132, DateTimeKind.Utc).AddTicks(6426), "uploads/members/photo_m632_56fffb0554ef4b53a6b66044b6998a4e_2512563.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 633,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 136, DateTimeKind.Utc).AddTicks(5399), new DateTime(2026, 3, 15, 16, 15, 54, 136, DateTimeKind.Utc).AddTicks(5406), "uploads/members/photo_m633_5a6b1352a7434f14a0d8aecc60bbec92_2512564.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 634,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 172, DateTimeKind.Utc).AddTicks(2629), new DateTime(2026, 3, 15, 16, 15, 54, 172, DateTimeKind.Utc).AddTicks(2645), "uploads/members/photo_m634_53a70777a6bf4cbba9a2042a1acbc8a9_2512565.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 635,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 204, DateTimeKind.Utc).AddTicks(2531), "uploads/members/photo_m635_54bada7678f24aae9d4dbc7baf9677dd_2512566.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 636,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 240, DateTimeKind.Utc).AddTicks(2549), new DateTime(2026, 3, 15, 16, 15, 54, 240, DateTimeKind.Utc).AddTicks(2559), "uploads/members/photo_m636_97626a22e2794f34a0fde2bc0aa6419c_2512567.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 637,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 263, DateTimeKind.Utc).AddTicks(2359), "uploads/members/photo_m637_01ea7e63c5b24e29aa6652f67d978ab5_2512568.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 638,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 478, DateTimeKind.Utc).AddTicks(5451), "uploads/members/photo_m638_532da464eb27491dbc94066de62d7857_2512570.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 639,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 507, DateTimeKind.Utc).AddTicks(7776), new DateTime(2026, 3, 15, 16, 15, 54, 507, DateTimeKind.Utc).AddTicks(7784), "uploads/members/photo_m639_c4652a6b88f645339fa2d99d47ac266d_2512571.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 640,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 624, DateTimeKind.Utc).AddTicks(5212), new DateTime(2026, 3, 15, 16, 15, 54, 624, DateTimeKind.Utc).AddTicks(5221), "uploads/members/photo_m640_1524998529f545f9bd0b47a0dabb5011_2512572.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 641,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 672, DateTimeKind.Utc).AddTicks(8865), new DateTime(2026, 3, 15, 16, 15, 54, 672, DateTimeKind.Utc).AddTicks(8875), "uploads/members/photo_m641_a535841d9e814e4f9ae6b86a7832f280_2512573.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 642,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 723, DateTimeKind.Utc).AddTicks(834), "uploads/members/photo_m642_92cf7c3f007a43548d9d6dd8a22b1b28_2512574.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 643,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 729, DateTimeKind.Utc).AddTicks(9555), new DateTime(2026, 3, 15, 16, 15, 54, 729, DateTimeKind.Utc).AddTicks(9562), "uploads/members/photo_m643_84f68b86f0da47079aff2a22bad65ea4_2512575.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 644,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 814, DateTimeKind.Utc).AddTicks(9393), new DateTime(2026, 3, 15, 16, 15, 54, 814, DateTimeKind.Utc).AddTicks(9405), "uploads/members/photo_m644_17976556400a4e4292fe4ee409dcac5a_2512578.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 645,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 826, DateTimeKind.Utc).AddTicks(7199), "uploads/members/photo_m645_4d72c6724e9e4e87a25e80e2b6281583_2512579.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 646,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 835, DateTimeKind.Utc).AddTicks(4353), new DateTime(2026, 3, 15, 16, 15, 54, 835, DateTimeKind.Utc).AddTicks(4368), "uploads/members/photo_m646_fea6cdcd46c64a03b1df5f6f30bec44c_2512581.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 647,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 845, DateTimeKind.Utc).AddTicks(9905), new DateTime(2026, 3, 15, 16, 15, 54, 845, DateTimeKind.Utc).AddTicks(9913), "uploads/members/photo_m647_226f8f97f6a54bab98e686df625e7546_2512582.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 648,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 853, DateTimeKind.Utc).AddTicks(4559), new DateTime(2026, 3, 15, 16, 15, 54, 853, DateTimeKind.Utc).AddTicks(4567), "uploads/members/photo_m648_315d4110aa494f8c90cd56989813b26b_2512583.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 649,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 861, DateTimeKind.Utc).AddTicks(4168), new DateTime(2026, 3, 15, 16, 15, 54, 861, DateTimeKind.Utc).AddTicks(4176), "uploads/members/photo_m649_d818c31aaa2a4009bc18938146886051_2512584.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 650,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 876, DateTimeKind.Utc).AddTicks(8958), new DateTime(2026, 3, 15, 16, 15, 54, 876, DateTimeKind.Utc).AddTicks(8966), "uploads/members/photo_m650_52769838539f4d2b803c8110e99beed5_2512585.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 651,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 901, DateTimeKind.Utc).AddTicks(6965), new DateTime(2026, 3, 15, 16, 15, 54, 901, DateTimeKind.Utc).AddTicks(6973), "uploads/members/photo_m651_e8d15860156445e9bf4b567aaba3f55e_2512586.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 652,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 910, DateTimeKind.Utc).AddTicks(4898), "uploads/members/photo_m652_80f9f757333e4d05a478321ae2c7f580_2512587.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 653,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 921, DateTimeKind.Utc).AddTicks(741), new DateTime(2026, 3, 15, 16, 15, 54, 921, DateTimeKind.Utc).AddTicks(754), "uploads/members/photo_m653_21e0176d135f48eab95378ff22bde603_2512588.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 654,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 944, DateTimeKind.Utc).AddTicks(174), new DateTime(2026, 3, 15, 16, 15, 54, 944, DateTimeKind.Utc).AddTicks(181), "uploads/members/photo_m654_c805137c916e436895234e29db72116d_2512589.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 655,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 54, 962, DateTimeKind.Utc).AddTicks(7425), new DateTime(2026, 3, 15, 16, 15, 54, 962, DateTimeKind.Utc).AddTicks(7434), "uploads/members/photo_m655_7941849fdd094ef1b96c03bfa6e17aa1_2512591.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 656,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 5, DateTimeKind.Utc).AddTicks(3423), new DateTime(2026, 3, 15, 16, 15, 55, 5, DateTimeKind.Utc).AddTicks(3436), "uploads/members/photo_m656_7823c88c0fce43b1bbefe942fdc6384c_2512592.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 657,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 31, DateTimeKind.Utc).AddTicks(9049), new DateTime(2026, 3, 15, 16, 15, 55, 31, DateTimeKind.Utc).AddTicks(9064), "uploads/members/photo_m657_e3a4085f15c6431fa53881919f87d8a8_2512593.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 658,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 50, DateTimeKind.Utc).AddTicks(2173), new DateTime(2026, 3, 15, 16, 15, 55, 50, DateTimeKind.Utc).AddTicks(2187), "uploads/members/photo_m658_62ebb3fe7ec54d59a697de0af320821a_2512594.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 659,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 68, DateTimeKind.Utc).AddTicks(8494), "uploads/members/photo_m659_affb73f50f724eb9a157f3f5339401ae_2512595.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 660,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 79, DateTimeKind.Utc).AddTicks(1705), new DateTime(2026, 3, 15, 16, 15, 55, 79, DateTimeKind.Utc).AddTicks(1721), "uploads/members/photo_m660_4994e85944734f3e9fe8608e2add43d3_2512596.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 661,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 84, DateTimeKind.Utc).AddTicks(4118), new DateTime(2026, 3, 15, 16, 15, 55, 84, DateTimeKind.Utc).AddTicks(4131), "uploads/members/photo_m661_57bc8c345d044ffebc9854b7852c8e44_2512597.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 662,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 111, DateTimeKind.Utc).AddTicks(9417), new DateTime(2026, 3, 15, 16, 15, 55, 111, DateTimeKind.Utc).AddTicks(9425), "uploads/members/photo_m662_33683e25886e4a97af3cdfc0d616505f_2512598.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 663,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 149, DateTimeKind.Utc).AddTicks(9014), new DateTime(2026, 3, 15, 16, 15, 55, 149, DateTimeKind.Utc).AddTicks(9028), "uploads/members/photo_m663_c78f59c1e5714e049884641d0b794032_2512599.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 664,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 161, DateTimeKind.Utc).AddTicks(6859), new DateTime(2026, 3, 15, 16, 15, 55, 161, DateTimeKind.Utc).AddTicks(6873), "uploads/members/photo_m664_5b38db4114e54f51949a58b7fae59189_2512600.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 665,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 179, DateTimeKind.Utc).AddTicks(3251), new DateTime(2026, 3, 15, 16, 15, 55, 179, DateTimeKind.Utc).AddTicks(3263), "uploads/members/photo_m665_609178e764dc4a52ac23b24ef74272e7_2512601.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 666,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 198, DateTimeKind.Utc).AddTicks(375), new DateTime(2026, 3, 15, 16, 15, 55, 198, DateTimeKind.Utc).AddTicks(383), "uploads/members/photo_m666_0f117e6cf3c049c4996e6540c54e6819_2512602.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 667,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 278, DateTimeKind.Utc).AddTicks(2387), new DateTime(2026, 3, 15, 16, 15, 55, 278, DateTimeKind.Utc).AddTicks(2395), "uploads/members/photo_m667_ea33ca58c2f847448deeb8326d4456e1_2512604.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 668,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 344, DateTimeKind.Utc).AddTicks(3941), new DateTime(2026, 3, 15, 16, 15, 55, 344, DateTimeKind.Utc).AddTicks(3952), "uploads/members/photo_m668_cf135a926d6c4f0cb3c164749807f12e_2512605.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 669,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 588, DateTimeKind.Utc).AddTicks(1275), new DateTime(2026, 3, 15, 16, 15, 55, 588, DateTimeKind.Utc).AddTicks(1287), "uploads/members/photo_m669_8ec8cf9bc46c42909fd924e4a6c68987_2512607.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 670,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 591, DateTimeKind.Utc).AddTicks(6962), new DateTime(2026, 3, 15, 16, 15, 55, 591, DateTimeKind.Utc).AddTicks(6971), "uploads/members/photo_m670_dcbec9af0caf493e8498af7404018813_2512609.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 671,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 636, DateTimeKind.Utc).AddTicks(5415), new DateTime(2026, 3, 15, 16, 15, 55, 636, DateTimeKind.Utc).AddTicks(5425), "uploads/members/photo_m671_e712ac9e515b4187bedc5987613c0ead_2512610.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 672,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 645, DateTimeKind.Utc).AddTicks(9231), new DateTime(2026, 3, 15, 16, 15, 55, 645, DateTimeKind.Utc).AddTicks(9237), "uploads/members/photo_m672_d9f2dc3437a5407ea26e335d32d7f0ca_2512611.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 673,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 792, DateTimeKind.Utc).AddTicks(177), new DateTime(2026, 3, 15, 16, 15, 55, 792, DateTimeKind.Utc).AddTicks(189), "uploads/members/photo_m673_589d7c6ace4147c99b9d37fc809c4e6b_2512612.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 674,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 814, DateTimeKind.Utc).AddTicks(2173), "uploads/members/photo_m674_a40c82558f9e46a8a3033de57f7c3b2c_2512614.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 675,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 839, DateTimeKind.Utc).AddTicks(884), new DateTime(2026, 3, 15, 16, 15, 55, 839, DateTimeKind.Utc).AddTicks(898), "uploads/members/photo_m675_77f6542629f545ddb5b2780a317333ed_2512615.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 676,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 849, DateTimeKind.Utc).AddTicks(932), new DateTime(2026, 3, 15, 16, 15, 55, 849, DateTimeKind.Utc).AddTicks(945), "uploads/members/photo_m676_610ca4799d914ca08532de6e574d5525_2512616.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 677,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 922, DateTimeKind.Utc).AddTicks(1926), new DateTime(2026, 3, 15, 16, 15, 55, 922, DateTimeKind.Utc).AddTicks(1939), "uploads/members/photo_m677_f73cd1b8df674c70938fae4474f1d158_2512617.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 678,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 941, DateTimeKind.Utc).AddTicks(7587), new DateTime(2026, 3, 15, 16, 15, 55, 941, DateTimeKind.Utc).AddTicks(7596), "uploads/members/photo_m678_90150ceee58549599da8f3f2792a8103_2512618.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 679,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 55, 986, DateTimeKind.Utc).AddTicks(9949), new DateTime(2026, 3, 15, 16, 15, 55, 986, DateTimeKind.Utc).AddTicks(9957), "uploads/members/photo_m679_1abadc0959f34c2e96bbafd0477629c6_2512619.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 680,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 14, DateTimeKind.Utc).AddTicks(5584), new DateTime(2026, 3, 15, 16, 15, 56, 14, DateTimeKind.Utc).AddTicks(5593), "uploads/members/photo_m680_79721e587b114eca9c83847e6adc8f8c_2512620.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 681,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 81, DateTimeKind.Utc).AddTicks(2798), new DateTime(2026, 3, 15, 16, 15, 56, 81, DateTimeKind.Utc).AddTicks(2805), "uploads/members/photo_m681_63847191942a4671a03d4aca2395ef56_2512621.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 682,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 112, DateTimeKind.Utc).AddTicks(6618), "uploads/members/photo_m682_9ccef76e9d8d4373b71408959b6cbdf8_2512623.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 683,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 138, DateTimeKind.Utc).AddTicks(1182), "uploads/members/photo_m683_7a769aee819146ccbf4823c6551ea3b7_2512625.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 684,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 146, DateTimeKind.Utc).AddTicks(7059), new DateTime(2026, 3, 15, 16, 15, 56, 146, DateTimeKind.Utc).AddTicks(7067), "uploads/members/photo_m684_6ae720825566450da62f5a92b2b5f72f_2512626.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 685,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 155, DateTimeKind.Utc).AddTicks(7241), "uploads/members/photo_m685_847babf753f54ea2918340e769fbafe1_2512627.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 686,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 169, DateTimeKind.Utc).AddTicks(439), new DateTime(2026, 3, 15, 16, 15, 56, 169, DateTimeKind.Utc).AddTicks(447), "uploads/members/photo_m686_e1b24ce8ef584f7d99108a005f71730d_2512628.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 687,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 175, DateTimeKind.Utc).AddTicks(9079), new DateTime(2026, 3, 15, 16, 15, 56, 175, DateTimeKind.Utc).AddTicks(9086), "uploads/members/photo_m687_c23cb4c423414dee8dc7941002c29a2a_2512629.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 688,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 185, DateTimeKind.Utc).AddTicks(1625), new DateTime(2026, 3, 15, 16, 15, 56, 185, DateTimeKind.Utc).AddTicks(1632), "uploads/members/photo_m688_dc20c1d84a8f41fa98741c75aadd9edb_2512630.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 689,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 190, DateTimeKind.Utc).AddTicks(4048), new DateTime(2026, 3, 15, 16, 15, 56, 190, DateTimeKind.Utc).AddTicks(4056), "uploads/members/photo_m689_c74d992e7a424026b9cea7a295ec1501_2512631.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 690,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 220, DateTimeKind.Utc).AddTicks(8926), new DateTime(2026, 3, 15, 16, 15, 56, 220, DateTimeKind.Utc).AddTicks(8941), "uploads/members/photo_m690_82a48f147fa9429fb87758a4b2644526_2512632.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 691,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 242, DateTimeKind.Utc).AddTicks(8452), new DateTime(2026, 3, 15, 16, 15, 56, 242, DateTimeKind.Utc).AddTicks(8466), "uploads/members/photo_m691_9a2119e624474155b4bc534e466c5933_2512633.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 692,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 269, DateTimeKind.Utc).AddTicks(3257), new DateTime(2026, 3, 15, 16, 15, 56, 269, DateTimeKind.Utc).AddTicks(3271), "uploads/members/photo_m692_e7decd3413b14cef80eacde98a41d9cc_2512634.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 693,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 278, DateTimeKind.Utc).AddTicks(421), new DateTime(2026, 3, 15, 16, 15, 56, 278, DateTimeKind.Utc).AddTicks(432), "uploads/members/photo_m693_2c149579935249ebbfdaa6b676e05968_2512635.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 694,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 285, DateTimeKind.Utc).AddTicks(351), new DateTime(2026, 3, 15, 16, 15, 56, 285, DateTimeKind.Utc).AddTicks(361), "uploads/members/photo_m694_b0ed005346fa4f94b867cfc56720a88f_2512636.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 695,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 331, DateTimeKind.Utc).AddTicks(1257), new DateTime(2026, 3, 15, 16, 15, 56, 331, DateTimeKind.Utc).AddTicks(1268), "uploads/members/photo_m695_15bb5027f7964f2f9b775a04b4622c7d_2512637.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 696,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 343, DateTimeKind.Utc).AddTicks(9688), new DateTime(2026, 3, 15, 16, 15, 56, 343, DateTimeKind.Utc).AddTicks(9696), "uploads/members/photo_m696_a7bd60293fa0452b80d6f4cc01742a39_2512638.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 697,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 366, DateTimeKind.Utc).AddTicks(3369), new DateTime(2026, 3, 15, 16, 15, 56, 366, DateTimeKind.Utc).AddTicks(3384), "uploads/members/photo_m697_6d24a28410d446c5b64559a011e159c8_2512639.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 698,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 372, DateTimeKind.Utc).AddTicks(6272), new DateTime(2026, 3, 15, 16, 15, 56, 372, DateTimeKind.Utc).AddTicks(6279), "uploads/members/photo_m698_69d3c44304fd4352a7258b09abce931b_2512640.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 699,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 424, DateTimeKind.Utc).AddTicks(423), new DateTime(2026, 3, 15, 16, 15, 56, 424, DateTimeKind.Utc).AddTicks(434), "uploads/members/photo_m699_20246f4fbd6c443d88fe56852801d90b_2512641.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 431, DateTimeKind.Utc).AddTicks(5101), new DateTime(2026, 3, 15, 16, 15, 56, 431, DateTimeKind.Utc).AddTicks(5114), "uploads/members/photo_m700_70ea853e8694421e84db9841419c7394_2512642.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 457, DateTimeKind.Utc).AddTicks(5648), new DateTime(2026, 3, 15, 16, 15, 56, 457, DateTimeKind.Utc).AddTicks(5661), "uploads/members/photo_m701_16e997c8073a4f9c9640960ba36fcc99_2512643.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 466, DateTimeKind.Utc).AddTicks(9966), new DateTime(2026, 3, 15, 16, 15, 56, 466, DateTimeKind.Utc).AddTicks(9981), "uploads/members/photo_m702_a5e4fb88dcc24b679ae087a47171b27b_2512644.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 539, DateTimeKind.Utc).AddTicks(9437), new DateTime(2026, 3, 15, 16, 15, 56, 539, DateTimeKind.Utc).AddTicks(9445), "uploads/members/photo_m703_917a3db47cbf4ed1ad0f1119bd25dd69_2512645.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 548, DateTimeKind.Utc).AddTicks(6649), new DateTime(2026, 3, 15, 16, 15, 56, 548, DateTimeKind.Utc).AddTicks(6657), "uploads/members/photo_m704_419b836b26a043b9a76c2b0f26d364a6_2512647.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 705,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 597, DateTimeKind.Utc).AddTicks(4517), new DateTime(2026, 3, 15, 16, 15, 56, 597, DateTimeKind.Utc).AddTicks(4526), "uploads/members/photo_m705_e476a3d85a1d470e8b503542a93849a9_2512648.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 706,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 610, DateTimeKind.Utc).AddTicks(3188), "uploads/members/photo_m706_605f6b8e6b9a468d834aba8ec6a001bf_2512651.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 707,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 623, DateTimeKind.Utc).AddTicks(6502), new DateTime(2026, 3, 15, 16, 15, 56, 623, DateTimeKind.Utc).AddTicks(6509), "uploads/members/photo_m707_b0f6412ea92548108af900eb8584d3c9_2512653.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 708,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 628, DateTimeKind.Utc).AddTicks(9573), new DateTime(2026, 3, 15, 16, 15, 56, 628, DateTimeKind.Utc).AddTicks(9581), "uploads/members/photo_m708_bed724d4a92343fdbb3fba9e34c4c50a_2512654.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 709,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 665, DateTimeKind.Utc).AddTicks(9632), "uploads/members/photo_m709_d2701989acd04faf956bc9acd4b997e8_2512655.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 710,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 706, DateTimeKind.Utc).AddTicks(1544), new DateTime(2026, 3, 15, 16, 15, 56, 706, DateTimeKind.Utc).AddTicks(1554), "uploads/members/photo_m710_8042732b15664203a7bc0d120ff51506_2512656.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 711,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 714, DateTimeKind.Utc).AddTicks(3208), new DateTime(2026, 3, 15, 16, 15, 56, 714, DateTimeKind.Utc).AddTicks(3221), "uploads/members/photo_m711_7720ca1f0e254a258d0ef5104a92e334_2512657.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 712,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 724, DateTimeKind.Utc).AddTicks(3558), new DateTime(2026, 3, 15, 16, 15, 56, 724, DateTimeKind.Utc).AddTicks(3567), "uploads/members/photo_m712_95cec73717d44490a516a4a18d4621a4_2512658.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 713,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 744, DateTimeKind.Utc).AddTicks(4925), new DateTime(2026, 3, 15, 16, 15, 56, 744, DateTimeKind.Utc).AddTicks(4934), "uploads/members/photo_m713_d0f3df8606934ef1980ad8b06d017b71_2512659.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 714,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 756, DateTimeKind.Utc).AddTicks(4806), new DateTime(2026, 3, 15, 16, 15, 56, 756, DateTimeKind.Utc).AddTicks(4814), "uploads/members/photo_m714_366deb8fb3a141ce96e6cae956fd58ae_2512660.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 715,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 766, DateTimeKind.Utc).AddTicks(8711), new DateTime(2026, 3, 15, 16, 15, 56, 766, DateTimeKind.Utc).AddTicks(8723), "uploads/members/photo_m715_c86caf869c084fdd877cc9a3527875aa_2512661.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 716,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 769, DateTimeKind.Utc).AddTicks(4791), new DateTime(2026, 3, 15, 16, 15, 56, 769, DateTimeKind.Utc).AddTicks(4802), "uploads/members/photo_m716_a574b84cce494b50b970e580b3706f3e_2512662.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 717,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 780, DateTimeKind.Utc).AddTicks(6353), new DateTime(2026, 3, 15, 16, 15, 56, 780, DateTimeKind.Utc).AddTicks(6367), "uploads/members/photo_m717_fbbc8bf613204a1fb84f920f89917e03_2512663.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 718,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 819, DateTimeKind.Utc).AddTicks(1773), "uploads/members/photo_m718_52189a42ea164fe5a54f8188ce2238a8_2512664.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 719,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 886, DateTimeKind.Utc).AddTicks(4557), new DateTime(2026, 3, 15, 16, 15, 56, 886, DateTimeKind.Utc).AddTicks(4565), "uploads/members/photo_m719_ca01f4e3cf3f41d9a1a0dae5b6fdc8de_2512665.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 720,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 909, DateTimeKind.Utc).AddTicks(6521), "uploads/members/photo_m720_12b254caf4ea4a1e9646be14a5be9768_2512666.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 721,
                column: "PhotoPath",
                value: "uploads/members/photo_m721_ffb16f11d8e647f69d82796a9fec68aa_2512667.jpeg");

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 722,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 938, DateTimeKind.Utc).AddTicks(7832), new DateTime(2026, 3, 15, 16, 15, 56, 938, DateTimeKind.Utc).AddTicks(7846), "uploads/members/photo_m722_1ff1e0d4ead44584a7750ea7496871e6_2512669.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 723,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 967, DateTimeKind.Utc).AddTicks(1788), new DateTime(2026, 3, 15, 16, 15, 56, 967, DateTimeKind.Utc).AddTicks(1797), "uploads/members/photo_m723_ff53ca8dc194420abdb974753afb87b6_2512670.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 724,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 56, 975, DateTimeKind.Utc).AddTicks(5122), new DateTime(2026, 3, 15, 16, 15, 56, 975, DateTimeKind.Utc).AddTicks(5135), "uploads/members/photo_m724_fcbd3bc50bb04b07962e0a3dab58bd13_2512671.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 725,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 5, DateTimeKind.Utc).AddTicks(7121), new DateTime(2026, 3, 15, 16, 15, 57, 5, DateTimeKind.Utc).AddTicks(7132), "uploads/members/photo_m725_161b880290b2413190b3aa98c4bcefd7_2512672.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 726,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 25, DateTimeKind.Utc).AddTicks(1848), new DateTime(2026, 3, 15, 16, 15, 57, 25, DateTimeKind.Utc).AddTicks(1857), "uploads/members/photo_m726_1cdac2f0b3b54695b8284be64386f375_2512673.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 727,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 40, DateTimeKind.Utc).AddTicks(7597), new DateTime(2026, 3, 15, 16, 15, 57, 40, DateTimeKind.Utc).AddTicks(7608), "uploads/members/photo_m727_fd6e175bbec54ba091fdd916a879b912_2512674.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 728,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 44, DateTimeKind.Utc).AddTicks(3903), "uploads/members/photo_m728_805afd2c929442f2b53b33dce602b07d_2512675.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 729,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 58, DateTimeKind.Utc).AddTicks(2039), "uploads/members/photo_m729_9b5c65bff71f4f33b68ad23a1875e524_2512676.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 730,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 71, DateTimeKind.Utc).AddTicks(7927), new DateTime(2026, 3, 15, 16, 15, 57, 71, DateTimeKind.Utc).AddTicks(7937), "uploads/members/photo_m730_45a7f27e9b6944f3a221f3be4bbe1b92_2512677.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 731,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 90, DateTimeKind.Utc).AddTicks(3392), new DateTime(2026, 3, 15, 16, 15, 57, 90, DateTimeKind.Utc).AddTicks(3406), "uploads/members/photo_m731_fd62c0988d674233b710d4a7e475cf7e_2512678.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 732,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 101, DateTimeKind.Utc).AddTicks(2045), new DateTime(2026, 3, 15, 16, 15, 57, 101, DateTimeKind.Utc).AddTicks(2058), "uploads/members/photo_m732_6abb3f4d67a54712b49766f12cecb998_2512679.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 733,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 138, DateTimeKind.Utc).AddTicks(9469), new DateTime(2026, 3, 15, 16, 15, 57, 138, DateTimeKind.Utc).AddTicks(9476), "uploads/members/photo_m733_138be1f5eb88463fb2ac1943325ea92a_2512681.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 734,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 192, DateTimeKind.Utc).AddTicks(2047), "uploads/members/photo_m734_06aa6ebcf4444bf5a1b3acdd0077d2c6_2512682.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 735,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 197, DateTimeKind.Utc).AddTicks(643), new DateTime(2026, 3, 15, 16, 15, 57, 197, DateTimeKind.Utc).AddTicks(651), "uploads/members/photo_m735_9835e03bf67f45f2bb2b2e0e854e2b59_2512683.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 736,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 259, DateTimeKind.Utc).AddTicks(3758), "uploads/members/photo_m736_e1fa00eaffbb4a728223e8e9ca408d08_2512684.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 737,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 283, DateTimeKind.Utc).AddTicks(5886), new DateTime(2026, 3, 15, 16, 15, 57, 283, DateTimeKind.Utc).AddTicks(5895), "uploads/members/photo_m737_4ef9dbe3a3474938964cabd7e45a1056_2512685.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 738,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 293, DateTimeKind.Utc).AddTicks(6797), new DateTime(2026, 3, 15, 16, 15, 57, 293, DateTimeKind.Utc).AddTicks(6805), "uploads/members/photo_m738_e65b90e3c1bc40b6994ff8d1502ea382_2512686.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 739,
                column: "PhotoPath",
                value: "uploads/members/photo_m739_20fda229f9ff48c7bc345b2c29c62ab3_2512687.jpeg");

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 740,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 326, DateTimeKind.Utc).AddTicks(795), new DateTime(2026, 3, 15, 16, 15, 57, 326, DateTimeKind.Utc).AddTicks(804), "uploads/members/photo_m740_7ac02e11ab764d848b05755af6dc92ee_2512688.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 741,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 357, DateTimeKind.Utc).AddTicks(1753), "uploads/members/photo_m741_ab47470cd45b402291b4540f26af08c2_2512689.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 742,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 390, DateTimeKind.Utc).AddTicks(9526), new DateTime(2026, 3, 15, 16, 15, 57, 390, DateTimeKind.Utc).AddTicks(9539), "uploads/members/photo_m742_a540b98db88a40bda3d1c4262807b95e_2512690.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 743,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 403, DateTimeKind.Utc).AddTicks(5926), new DateTime(2026, 3, 15, 16, 15, 57, 403, DateTimeKind.Utc).AddTicks(5943), "uploads/members/photo_m743_41bfa248dd7e471e85d72c7e92800a86_2512691.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 744,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 465, DateTimeKind.Utc).AddTicks(3024), new DateTime(2026, 3, 15, 16, 15, 57, 465, DateTimeKind.Utc).AddTicks(3031), "uploads/members/photo_m744_d00b3d7940024f789dc26ef1feaca2df_2512692.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 745,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 489, DateTimeKind.Utc).AddTicks(8314), new DateTime(2026, 3, 15, 16, 15, 57, 489, DateTimeKind.Utc).AddTicks(8324), "uploads/members/photo_m745_1d6e39ce9e3d445599c7d4e6aaaba54a_2512693.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 746,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 529, DateTimeKind.Utc).AddTicks(5384), new DateTime(2026, 3, 15, 16, 15, 57, 529, DateTimeKind.Utc).AddTicks(5393), "uploads/members/photo_m746_b3ca33d67124407cbca8231a6dd0a517_2512694.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 747,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 559, DateTimeKind.Utc).AddTicks(4302), new DateTime(2026, 3, 15, 16, 15, 57, 559, DateTimeKind.Utc).AddTicks(4311), "uploads/members/photo_m747_a5f6ad9ba61b4cf7a03dae87e12fb202_2512695.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 748,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 573, DateTimeKind.Utc).AddTicks(8635), "uploads/members/photo_m748_c80b809f3cd344f2849197ad561ab25a_2512696.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 749,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 601, DateTimeKind.Utc).AddTicks(5753), new DateTime(2026, 3, 15, 16, 15, 57, 601, DateTimeKind.Utc).AddTicks(5769), "uploads/members/photo_m749_5b7831f497794e1f9fd07a4542fa3dc9_2512698.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 750,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 616, DateTimeKind.Utc).AddTicks(5542), new DateTime(2026, 3, 15, 16, 15, 57, 616, DateTimeKind.Utc).AddTicks(5555), "uploads/members/photo_m750_0dda46c2fadd4919af1328d903c170d7_2512701.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 751,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 624, DateTimeKind.Utc).AddTicks(5309), new DateTime(2026, 3, 15, 16, 15, 57, 624, DateTimeKind.Utc).AddTicks(5322), "uploads/members/photo_m751_8ff233231a084b689d8a1f175a7fd07e_2512702.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 752,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 692, DateTimeKind.Utc).AddTicks(3388), "uploads/members/photo_m752_bd5103248610481d9f70e144d0332a2b_2512703.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 753,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 719, DateTimeKind.Utc).AddTicks(5619), new DateTime(2026, 3, 15, 16, 15, 57, 719, DateTimeKind.Utc).AddTicks(5628), "uploads/members/photo_m753_9d56991be5b74a64995670d1aab4cff2_2512704.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 754,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 917, DateTimeKind.Utc).AddTicks(2298), new DateTime(2026, 3, 15, 16, 15, 57, 917, DateTimeKind.Utc).AddTicks(2311), "uploads/members/photo_m754_0c53c8b60fcd41f888e47948b5333e41_2512705.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 755,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 934, DateTimeKind.Utc).AddTicks(4274), new DateTime(2026, 3, 15, 16, 15, 57, 934, DateTimeKind.Utc).AddTicks(4284), "uploads/members/photo_m755_08c4ad9eaaea4344b27dd3162ced86aa_2512706.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 756,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 57, 991, DateTimeKind.Utc).AddTicks(6742), "uploads/members/photo_m756_86d317e6490a449e97013244a0a8d848_2512707.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 757,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 2, DateTimeKind.Utc).AddTicks(3867), new DateTime(2026, 3, 15, 16, 15, 58, 2, DateTimeKind.Utc).AddTicks(3876), "uploads/members/photo_m757_efb6d4b9097d48138f2a80d67a76f025_2512708.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 758,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 14, DateTimeKind.Utc).AddTicks(2933), new DateTime(2026, 3, 15, 16, 15, 58, 14, DateTimeKind.Utc).AddTicks(2947), "uploads/members/photo_m758_ae0a6608e2494526803da22c05be28d0_2512709.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 759,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 26, DateTimeKind.Utc).AddTicks(8602), new DateTime(2026, 3, 15, 16, 15, 58, 26, DateTimeKind.Utc).AddTicks(8615), "uploads/members/photo_m759_1ebd11f16edd49d0beaad9e5ea7cf84a_2512710.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 760,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 37, DateTimeKind.Utc).AddTicks(7916), new DateTime(2026, 3, 15, 16, 15, 58, 37, DateTimeKind.Utc).AddTicks(7927), "uploads/members/photo_m760_cd336fcc60044505a3424fbfd5de9f4f_2512711.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 761,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 59, DateTimeKind.Utc).AddTicks(7098), "uploads/members/photo_m761_afbc4d5767344ae4b699d76251bcacc0_2512712.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 762,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 68, DateTimeKind.Utc).AddTicks(904), new DateTime(2026, 3, 15, 16, 15, 58, 68, DateTimeKind.Utc).AddTicks(917), "uploads/members/photo_m762_55b32f9bace84160bec65eb04d73ea1a_2512713.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 763,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 103, DateTimeKind.Utc).AddTicks(564), new DateTime(2026, 3, 15, 16, 15, 58, 103, DateTimeKind.Utc).AddTicks(573), "uploads/members/photo_m763_0131b6cd0c5b4569adb1b139ab4c179c_2512714.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 764,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 111, DateTimeKind.Utc).AddTicks(1603), new DateTime(2026, 3, 15, 16, 15, 58, 111, DateTimeKind.Utc).AddTicks(1615), "uploads/members/photo_m764_f7c85347ea5a426a99d2ea7853db90f8_2512715.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 765,
                column: "PhotoPath",
                value: "uploads/members/photo_m765_c8eed1d938a74486a0a6fd363e043e71_2512716.jpg");

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 766,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 125, DateTimeKind.Utc).AddTicks(3798), new DateTime(2026, 3, 15, 16, 15, 58, 125, DateTimeKind.Utc).AddTicks(3805), "uploads/members/photo_m766_b3fbeae362c34aa0a9d4814f3d56a1bd_2512718.png" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 767,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 150, DateTimeKind.Utc).AddTicks(1009), new DateTime(2026, 3, 15, 16, 15, 58, 150, DateTimeKind.Utc).AddTicks(1017), "uploads/members/photo_m767_979386621c3940bfb2d3b93b03cbd209_2512719.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 768,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 159, DateTimeKind.Utc).AddTicks(9876), new DateTime(2026, 3, 15, 16, 15, 58, 159, DateTimeKind.Utc).AddTicks(9884), "uploads/members/photo_m768_bc12a8adb84448e8aa82704dec7eb2f3_2512722.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 769,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 209, DateTimeKind.Utc).AddTicks(4836), new DateTime(2026, 3, 15, 16, 15, 58, 209, DateTimeKind.Utc).AddTicks(4845), "uploads/members/photo_m769_964346b7e9124d719e45c76f8395f1fe_2512723.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 770,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 233, DateTimeKind.Utc).AddTicks(5403), new DateTime(2026, 3, 15, 16, 15, 58, 233, DateTimeKind.Utc).AddTicks(5413), "uploads/members/photo_m770_d7dfb7091e5a4cf7914ab210e1c348a2_2512724.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 771,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 275, DateTimeKind.Utc).AddTicks(825), new DateTime(2026, 3, 15, 16, 15, 58, 275, DateTimeKind.Utc).AddTicks(832), "uploads/members/photo_m771_1314b6624f8a4caaa5aeee0da8410f2e_2512725.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 772,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 323, DateTimeKind.Utc).AddTicks(6576), new DateTime(2026, 3, 15, 16, 15, 58, 323, DateTimeKind.Utc).AddTicks(6589), "uploads/members/photo_m772_f7803af378c74dfab6805190039c4857_2512726.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 773,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 335, DateTimeKind.Utc).AddTicks(32), "uploads/members/photo_m773_3b2ae8ce581b4266bb663b107247cbfe_2512728.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 774,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 376, DateTimeKind.Utc).AddTicks(3245), new DateTime(2026, 3, 15, 16, 15, 58, 376, DateTimeKind.Utc).AddTicks(3256), "uploads/members/photo_m774_d653472b2d8649d5af08f2870acc0b95_2512729.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 775,
                columns: new[] { "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 463, DateTimeKind.Utc).AddTicks(4511), "uploads/members/photo_m775_5b3555c3aef041388402c9350f766b49_2512730.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 776,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 474, DateTimeKind.Utc).AddTicks(373), new DateTime(2026, 3, 15, 16, 15, 58, 474, DateTimeKind.Utc).AddTicks(381), "uploads/members/photo_m776_a76406771e73421295b780bac7683d5b_2512731.jpeg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 777,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 528, DateTimeKind.Utc).AddTicks(1392), new DateTime(2026, 3, 15, 16, 15, 58, 528, DateTimeKind.Utc).AddTicks(1404), "uploads/members/photo_m777_0e7a1b2ea845402d8f0ab1c6df9132ed_2512732.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 778,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 534, DateTimeKind.Utc).AddTicks(2289), "uploads/members/photo_m778_7f1cea2a7c044473843684e07049524e_2512734.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 779,
                columns: new[] { "AppliedDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 575, DateTimeKind.Utc).AddTicks(4797), "uploads/members/photo_m779_64e1c0225ee3470cb1bd060b2ee4e316_2512735.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 780,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 630, DateTimeKind.Utc).AddTicks(3455), new DateTime(2026, 3, 15, 16, 15, 58, 630, DateTimeKind.Utc).AddTicks(3464), "uploads/members/photo_m780_007fc400bb9249198812184b0e0921fa_2512737.jpg" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 781,
                columns: new[] { "AppliedDate", "LastUpdateDate", "PhotoPath" },
                values: new object[] { new DateTime(2026, 3, 15, 16, 15, 58, 633, DateTimeKind.Utc).AddTicks(7343), new DateTime(2026, 3, 15, 16, 15, 58, 633, DateTimeKind.Utc).AddTicks(7354), "uploads/members/photo_m781_d5e135bbab314fe6bfa253089f3d2ee7_2512740.jpg" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 201,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 42, 897, DateTimeKind.Utc).AddTicks(8153));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 202,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 42, 954, DateTimeKind.Utc).AddTicks(6701));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 203,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 42, 978, DateTimeKind.Utc).AddTicks(3295));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 204,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 329, DateTimeKind.Utc).AddTicks(6023));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 205,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 353, DateTimeKind.Utc).AddTicks(4605));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 206,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 476, DateTimeKind.Utc).AddTicks(7448));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 207,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 528, DateTimeKind.Utc).AddTicks(5734));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 208,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 556, DateTimeKind.Utc).AddTicks(2238));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 209,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 570, DateTimeKind.Utc).AddTicks(281));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 210,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 581, DateTimeKind.Utc).AddTicks(4375));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 211,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 593, DateTimeKind.Utc).AddTicks(2362));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 212,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 615, DateTimeKind.Utc).AddTicks(9835));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 213,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 718, DateTimeKind.Utc).AddTicks(4014));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 214,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 813, DateTimeKind.Utc).AddTicks(9471));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 215,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 855, DateTimeKind.Utc).AddTicks(8581));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 216,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 43, 876, DateTimeKind.Utc).AddTicks(9593));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 218,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 98, DateTimeKind.Utc).AddTicks(5279));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 219,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 184, DateTimeKind.Utc).AddTicks(4486));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 220,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 201, DateTimeKind.Utc).AddTicks(4445));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 221,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 218, DateTimeKind.Utc).AddTicks(2251));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 222,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 265, DateTimeKind.Utc).AddTicks(3424));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 223,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 296, DateTimeKind.Utc).AddTicks(4073));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 224,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 325, DateTimeKind.Utc).AddTicks(5172));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 225,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 378, DateTimeKind.Utc).AddTicks(5649));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 226,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 430, DateTimeKind.Utc).AddTicks(3343));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 227,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 446, DateTimeKind.Utc).AddTicks(8378));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 228,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 455, DateTimeKind.Utc).AddTicks(3182));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 229,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 532, DateTimeKind.Utc).AddTicks(8882));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 230,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 586, DateTimeKind.Utc).AddTicks(6868));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 231,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 613, DateTimeKind.Utc).AddTicks(4877));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 232,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 625, DateTimeKind.Utc).AddTicks(4169));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 233,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 642, DateTimeKind.Utc).AddTicks(5131));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 234,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 713, DateTimeKind.Utc).AddTicks(5376));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 235,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 721, DateTimeKind.Utc).AddTicks(9789));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 236,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 732, DateTimeKind.Utc).AddTicks(9716));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 237,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 747, DateTimeKind.Utc).AddTicks(4728));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 238,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 777, DateTimeKind.Utc).AddTicks(7303));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 239,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 839, DateTimeKind.Utc).AddTicks(4527));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 240,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 858, DateTimeKind.Utc).AddTicks(8345));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 242,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 911, DateTimeKind.Utc).AddTicks(8774));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 244,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 928, DateTimeKind.Utc).AddTicks(4864));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 245,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 940, DateTimeKind.Utc).AddTicks(749));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 246,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 948, DateTimeKind.Utc).AddTicks(3598));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 247,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 960, DateTimeKind.Utc).AddTicks(8919));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 248,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 965, DateTimeKind.Utc).AddTicks(9062));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 249,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 977, DateTimeKind.Utc).AddTicks(4229));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 250,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 986, DateTimeKind.Utc).AddTicks(8192));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 251,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 44, 999, DateTimeKind.Utc).AddTicks(878));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 252,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 10, DateTimeKind.Utc).AddTicks(2515));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 253,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 54, DateTimeKind.Utc).AddTicks(6839));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 254,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 122, DateTimeKind.Utc).AddTicks(669));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 255,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 139, DateTimeKind.Utc).AddTicks(5882));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 256,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 149, DateTimeKind.Utc).AddTicks(4167));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 257,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 179, DateTimeKind.Utc).AddTicks(8905));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 258,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 216, DateTimeKind.Utc).AddTicks(9934));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 259,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 262, DateTimeKind.Utc).AddTicks(1738));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 260,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 273, DateTimeKind.Utc).AddTicks(9158));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 261,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 309, DateTimeKind.Utc).AddTicks(7183));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 262,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 360, DateTimeKind.Utc).AddTicks(8891));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 263,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 385, DateTimeKind.Utc).AddTicks(7552));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 264,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 394, DateTimeKind.Utc).AddTicks(6064));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 265,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 400, DateTimeKind.Utc).AddTicks(5621));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 266,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 558, DateTimeKind.Utc).AddTicks(5496));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 267,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 591, DateTimeKind.Utc).AddTicks(3072));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 268,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 609, DateTimeKind.Utc).AddTicks(7194));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 270,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 630, DateTimeKind.Utc).AddTicks(5562));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 271,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 696, DateTimeKind.Utc).AddTicks(145));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 272,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 713, DateTimeKind.Utc).AddTicks(8167));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 273,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 729, DateTimeKind.Utc).AddTicks(2498));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 274,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 739, DateTimeKind.Utc).AddTicks(9965));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 275,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 758, DateTimeKind.Utc).AddTicks(1167));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 276,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 796, DateTimeKind.Utc).AddTicks(784));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 277,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 804, DateTimeKind.Utc).AddTicks(678));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 279,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 826, DateTimeKind.Utc).AddTicks(5817));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 280,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 840, DateTimeKind.Utc).AddTicks(7168));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 281,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 847, DateTimeKind.Utc).AddTicks(3153));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 282,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 860, DateTimeKind.Utc).AddTicks(8609));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 283,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 893, DateTimeKind.Utc).AddTicks(798));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 284,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 899, DateTimeKind.Utc).AddTicks(3658));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 285,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 911, DateTimeKind.Utc).AddTicks(8131));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 286,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 919, DateTimeKind.Utc).AddTicks(9065));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 287,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 935, DateTimeKind.Utc).AddTicks(9668));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 288,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 980, DateTimeKind.Utc).AddTicks(4399));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 289,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 45, 989, DateTimeKind.Utc).AddTicks(7506));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 290,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 3, DateTimeKind.Utc).AddTicks(3609));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 293,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 205, DateTimeKind.Utc).AddTicks(6075));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 294,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 211, DateTimeKind.Utc).AddTicks(4372));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 295,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 256, DateTimeKind.Utc).AddTicks(3743));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 296,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 265, DateTimeKind.Utc).AddTicks(8417));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 297,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 278, DateTimeKind.Utc).AddTicks(9283));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 298,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 292, DateTimeKind.Utc).AddTicks(1881));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 299,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 319, DateTimeKind.Utc).AddTicks(7812));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 300,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 355, DateTimeKind.Utc).AddTicks(6106));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 302,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 386, DateTimeKind.Utc).AddTicks(3655));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 303,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 435, DateTimeKind.Utc).AddTicks(1386));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 304,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 453, DateTimeKind.Utc).AddTicks(6466));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 305,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 470, DateTimeKind.Utc).AddTicks(4706));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 306,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 502, DateTimeKind.Utc).AddTicks(5745));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 307,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 509, DateTimeKind.Utc).AddTicks(4914));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 308,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 525, DateTimeKind.Utc).AddTicks(8261));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 309,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 569, DateTimeKind.Utc).AddTicks(7523));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 310,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 579, DateTimeKind.Utc).AddTicks(47));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 311,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 661, DateTimeKind.Utc).AddTicks(5706));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 313,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 710, DateTimeKind.Utc).AddTicks(4467));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 314,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 737, DateTimeKind.Utc).AddTicks(4082));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 316,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 799, DateTimeKind.Utc).AddTicks(9546));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 317,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 845, DateTimeKind.Utc).AddTicks(3271));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 318,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 914, DateTimeKind.Utc).AddTicks(4221));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 319,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 46, 956, DateTimeKind.Utc).AddTicks(6011));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 321,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 82, DateTimeKind.Utc).AddTicks(5002));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 322,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 107, DateTimeKind.Utc).AddTicks(3782));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 323,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 115, DateTimeKind.Utc).AddTicks(3877));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 325,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 138, DateTimeKind.Utc).AddTicks(516));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 326,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 171, DateTimeKind.Utc).AddTicks(2741));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 327,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 199, DateTimeKind.Utc).AddTicks(1819));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 328,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 231, DateTimeKind.Utc).AddTicks(2661));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 329,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 283, DateTimeKind.Utc).AddTicks(9447));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 330,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 309, DateTimeKind.Utc).AddTicks(3756));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 331,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 340, DateTimeKind.Utc).AddTicks(6384));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 332,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 348, DateTimeKind.Utc).AddTicks(3966));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 333,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 378, DateTimeKind.Utc).AddTicks(4285));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 334,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 382, DateTimeKind.Utc).AddTicks(6257));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 335,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 415, DateTimeKind.Utc).AddTicks(8074));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 336,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 450, DateTimeKind.Utc).AddTicks(4471));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 337,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 466, DateTimeKind.Utc).AddTicks(6724));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 338,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 476, DateTimeKind.Utc).AddTicks(795));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 339,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 516, DateTimeKind.Utc).AddTicks(1355));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 340,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 553, DateTimeKind.Utc).AddTicks(1504));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 341,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 560, DateTimeKind.Utc).AddTicks(7657));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 342,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 566, DateTimeKind.Utc).AddTicks(8219));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 343,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 582, DateTimeKind.Utc).AddTicks(8129));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 344,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 591, DateTimeKind.Utc).AddTicks(9263));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 345,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 600, DateTimeKind.Utc).AddTicks(1715));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 346,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 606, DateTimeKind.Utc).AddTicks(9475));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 348,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 654, DateTimeKind.Utc).AddTicks(9257));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 349,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 658, DateTimeKind.Utc).AddTicks(7855));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 350,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 696, DateTimeKind.Utc).AddTicks(7887));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 351,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 823, DateTimeKind.Utc).AddTicks(1059));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 352,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 834, DateTimeKind.Utc).AddTicks(9513));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 353,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 870, DateTimeKind.Utc).AddTicks(858));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 354,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 880, DateTimeKind.Utc).AddTicks(6607));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 355,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 893, DateTimeKind.Utc).AddTicks(4839));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 356,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 906, DateTimeKind.Utc).AddTicks(8709));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 357,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 927, DateTimeKind.Utc).AddTicks(2393));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 359,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 950, DateTimeKind.Utc).AddTicks(4869));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 360,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 963, DateTimeKind.Utc).AddTicks(4625));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 362,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 47, 983, DateTimeKind.Utc).AddTicks(4216));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 363,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 40, DateTimeKind.Utc).AddTicks(9804));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 364,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 48, DateTimeKind.Utc).AddTicks(1817));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 365,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 53, DateTimeKind.Utc).AddTicks(6147));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 366,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 81, DateTimeKind.Utc).AddTicks(5692));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 367,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 93, DateTimeKind.Utc).AddTicks(7576));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 368,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 102, DateTimeKind.Utc).AddTicks(5636));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 370,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 238, DateTimeKind.Utc).AddTicks(4641));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 371,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 254, DateTimeKind.Utc).AddTicks(5687));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 372,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 315, DateTimeKind.Utc).AddTicks(2067));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 373,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 320, DateTimeKind.Utc).AddTicks(6891));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 374,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 321, DateTimeKind.Utc).AddTicks(988));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 375,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 353, DateTimeKind.Utc).AddTicks(1276));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 376,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 388, DateTimeKind.Utc).AddTicks(5064));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 377,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 396, DateTimeKind.Utc).AddTicks(7965));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 378,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 404, DateTimeKind.Utc).AddTicks(2702));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 379,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 426, DateTimeKind.Utc).AddTicks(1256));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 380,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 430, DateTimeKind.Utc).AddTicks(4912));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 381,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 440, DateTimeKind.Utc).AddTicks(3506));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 382,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 450, DateTimeKind.Utc).AddTicks(4317));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 383,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 460, DateTimeKind.Utc).AddTicks(5448));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 385,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 502, DateTimeKind.Utc).AddTicks(312));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 386,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 506, DateTimeKind.Utc).AddTicks(8502));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 387,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 514, DateTimeKind.Utc).AddTicks(2504));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 389,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 544, DateTimeKind.Utc).AddTicks(2771));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 390,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 584, DateTimeKind.Utc).AddTicks(9715));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 391,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 592, DateTimeKind.Utc).AddTicks(1238));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 392,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 618, DateTimeKind.Utc).AddTicks(9656));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 393,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 638, DateTimeKind.Utc).AddTicks(8894));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 394,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 645, DateTimeKind.Utc).AddTicks(2641));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 395,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 653, DateTimeKind.Utc).AddTicks(7691));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 396,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 663, DateTimeKind.Utc).AddTicks(7549));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 397,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 677, DateTimeKind.Utc).AddTicks(1181));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 398,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 681, DateTimeKind.Utc).AddTicks(6794));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 399,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 689, DateTimeKind.Utc).AddTicks(2654));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 400,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 697, DateTimeKind.Utc).AddTicks(7544));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 401,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 705, DateTimeKind.Utc).AddTicks(6319));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 403,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 729, DateTimeKind.Utc).AddTicks(3031));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 404,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 743, DateTimeKind.Utc).AddTicks(5727));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 405,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 783, DateTimeKind.Utc).AddTicks(7859));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 406,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 813, DateTimeKind.Utc).AddTicks(1364));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 407,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 853, DateTimeKind.Utc).AddTicks(2651));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 408,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 860, DateTimeKind.Utc).AddTicks(5414));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 409,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 872, DateTimeKind.Utc).AddTicks(4972));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 410,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 883, DateTimeKind.Utc).AddTicks(8774));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 411,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 48, 916, DateTimeKind.Utc).AddTicks(6086));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 412,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 6, DateTimeKind.Utc).AddTicks(7617));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 414,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 37, DateTimeKind.Utc).AddTicks(9875));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 415,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 61, DateTimeKind.Utc).AddTicks(7099));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 416,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 83, DateTimeKind.Utc).AddTicks(2826));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 417,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 94, DateTimeKind.Utc).AddTicks(4222));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 418,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 121, DateTimeKind.Utc).AddTicks(5643));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 419,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 188, DateTimeKind.Utc).AddTicks(7313));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 420,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 214, DateTimeKind.Utc).AddTicks(2256));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 422,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 233, DateTimeKind.Utc).AddTicks(5061));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 423,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 255, DateTimeKind.Utc).AddTicks(5113));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 424,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 277, DateTimeKind.Utc).AddTicks(9686));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 425,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 295, DateTimeKind.Utc).AddTicks(7035));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 426,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 306, DateTimeKind.Utc).AddTicks(9546));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 427,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 320, DateTimeKind.Utc).AddTicks(8045));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 428,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 360, DateTimeKind.Utc).AddTicks(7308));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 429,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 436, DateTimeKind.Utc).AddTicks(8234));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 430,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 486, DateTimeKind.Utc).AddTicks(4356));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 431,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 501, DateTimeKind.Utc).AddTicks(8259));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 435,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 625, DateTimeKind.Utc).AddTicks(7723));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 436,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 657, DateTimeKind.Utc).AddTicks(6417));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 437,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 674, DateTimeKind.Utc).AddTicks(6163));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 438,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 687, DateTimeKind.Utc).AddTicks(4819));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 439,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 697, DateTimeKind.Utc).AddTicks(9211));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 440,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 748, DateTimeKind.Utc).AddTicks(4238));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 441,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 767, DateTimeKind.Utc).AddTicks(8878));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 443,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 821, DateTimeKind.Utc).AddTicks(4637));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 444,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 826, DateTimeKind.Utc).AddTicks(6799));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 446,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 844, DateTimeKind.Utc).AddTicks(6127));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 447,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 901, DateTimeKind.Utc).AddTicks(2933));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 448,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 905, DateTimeKind.Utc).AddTicks(6724));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 449,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 951, DateTimeKind.Utc).AddTicks(3444));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 450,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 956, DateTimeKind.Utc).AddTicks(1139));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 451,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 49, 979, DateTimeKind.Utc).AddTicks(5128));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 452,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 15, DateTimeKind.Utc).AddTicks(2022));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 453,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 26, DateTimeKind.Utc).AddTicks(631));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 454,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 67, DateTimeKind.Utc).AddTicks(7952));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 455,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 86, DateTimeKind.Utc).AddTicks(5917));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 456,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 97, DateTimeKind.Utc).AddTicks(531));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 457,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 175, DateTimeKind.Utc).AddTicks(2132));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 458,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 232, DateTimeKind.Utc).AddTicks(4053));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 459,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 305, DateTimeKind.Utc).AddTicks(7094));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 460,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 428, DateTimeKind.Utc).AddTicks(821));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 461,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 438, DateTimeKind.Utc).AddTicks(5827));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 462,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 442, DateTimeKind.Utc).AddTicks(1904));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 463,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 448, DateTimeKind.Utc).AddTicks(6847));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 464,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 480, DateTimeKind.Utc).AddTicks(6956));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 465,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 495, DateTimeKind.Utc).AddTicks(5525));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 467,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 566, DateTimeKind.Utc).AddTicks(4049));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 468,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 579, DateTimeKind.Utc).AddTicks(5904));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 469,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 594, DateTimeKind.Utc).AddTicks(8543));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 470,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 611, DateTimeKind.Utc).AddTicks(7786));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 471,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 688, DateTimeKind.Utc).AddTicks(4287));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 472,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 784, DateTimeKind.Utc).AddTicks(9297));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 473,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 799, DateTimeKind.Utc).AddTicks(6751));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 474,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 824, DateTimeKind.Utc).AddTicks(6681));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 476,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 913, DateTimeKind.Utc).AddTicks(5315));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 477,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 929, DateTimeKind.Utc).AddTicks(3327));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 478,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 950, DateTimeKind.Utc).AddTicks(6354));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 479,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 50, 966, DateTimeKind.Utc).AddTicks(6428));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 480,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 39, DateTimeKind.Utc).AddTicks(159));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 481,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 60, DateTimeKind.Utc).AddTicks(6306));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 483,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 152, DateTimeKind.Utc).AddTicks(1107));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 485,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 172, DateTimeKind.Utc).AddTicks(8547));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 486,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 181, DateTimeKind.Utc).AddTicks(3362));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 487,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 189, DateTimeKind.Utc).AddTicks(5411));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 488,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 210, DateTimeKind.Utc).AddTicks(8097));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 489,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 231, DateTimeKind.Utc).AddTicks(132));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 490,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 236, DateTimeKind.Utc).AddTicks(5043));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 491,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 245, DateTimeKind.Utc).AddTicks(333));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 492,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 291, DateTimeKind.Utc).AddTicks(3717));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 493,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 301, DateTimeKind.Utc).AddTicks(637));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 494,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 304, DateTimeKind.Utc).AddTicks(8845));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 495,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 333, DateTimeKind.Utc).AddTicks(8815));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 496,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 343, DateTimeKind.Utc).AddTicks(8324));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 497,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 350, DateTimeKind.Utc).AddTicks(7171));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 498,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 378, DateTimeKind.Utc).AddTicks(4777));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 499,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 409, DateTimeKind.Utc).AddTicks(7364));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 500,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 416, DateTimeKind.Utc).AddTicks(1364));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 501,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 461, DateTimeKind.Utc).AddTicks(2356));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 502,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 479, DateTimeKind.Utc).AddTicks(7032));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 503,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 494, DateTimeKind.Utc).AddTicks(2297));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 504,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 505, DateTimeKind.Utc).AddTicks(2816));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 505,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 516, DateTimeKind.Utc).AddTicks(2571));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 506,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 542, DateTimeKind.Utc).AddTicks(6013));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 507,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 651, DateTimeKind.Utc).AddTicks(2384));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 508,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 696, DateTimeKind.Utc).AddTicks(4849));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 510,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 727, DateTimeKind.Utc).AddTicks(9605));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 511,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 741, DateTimeKind.Utc).AddTicks(5959));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 512,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 758, DateTimeKind.Utc).AddTicks(8927));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 513,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 764, DateTimeKind.Utc).AddTicks(9809));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 514,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 770, DateTimeKind.Utc).AddTicks(2461));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 515,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 774, DateTimeKind.Utc).AddTicks(931));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 516,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 802, DateTimeKind.Utc).AddTicks(8176));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 517,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 832, DateTimeKind.Utc).AddTicks(8637));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 519,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 843, DateTimeKind.Utc).AddTicks(6221));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 520,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 869, DateTimeKind.Utc).AddTicks(3481));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 522,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 895, DateTimeKind.Utc).AddTicks(668));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 523,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 51, 922, DateTimeKind.Utc).AddTicks(6702));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 526,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 58, DateTimeKind.Utc).AddTicks(8825));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 527,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 66, DateTimeKind.Utc).AddTicks(6151));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 528,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 89, DateTimeKind.Utc).AddTicks(5913));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 529,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 95, DateTimeKind.Utc).AddTicks(9612));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 530,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 122, DateTimeKind.Utc).AddTicks(7581));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 531,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 139, DateTimeKind.Utc).AddTicks(1299));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 532,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 142, DateTimeKind.Utc).AddTicks(954));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 533,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 175, DateTimeKind.Utc).AddTicks(4602));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 534,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 183, DateTimeKind.Utc).AddTicks(8243));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 535,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 191, DateTimeKind.Utc).AddTicks(7574));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 536,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 199, DateTimeKind.Utc).AddTicks(3379));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 537,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 231, DateTimeKind.Utc).AddTicks(8301));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 538,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 284, DateTimeKind.Utc).AddTicks(6026));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 539,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 294, DateTimeKind.Utc).AddTicks(4909));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 540,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 298, DateTimeKind.Utc).AddTicks(4941));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 541,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 339, DateTimeKind.Utc).AddTicks(4549));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 542,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 348, DateTimeKind.Utc).AddTicks(7741));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 543,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 353, DateTimeKind.Utc).AddTicks(3282));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 544,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 364, DateTimeKind.Utc).AddTicks(5691));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 545,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 375, DateTimeKind.Utc).AddTicks(115));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 546,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 391, DateTimeKind.Utc).AddTicks(3849));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 547,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 399, DateTimeKind.Utc).AddTicks(1773));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 548,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 405, DateTimeKind.Utc).AddTicks(1563));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 551,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 462, DateTimeKind.Utc).AddTicks(926));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 552,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 502, DateTimeKind.Utc).AddTicks(2004));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 553,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 549, DateTimeKind.Utc).AddTicks(8289));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 554,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 560, DateTimeKind.Utc).AddTicks(7224));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 555,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 586, DateTimeKind.Utc).AddTicks(979));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 556,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 619, DateTimeKind.Utc).AddTicks(6482));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 558,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 649, DateTimeKind.Utc).AddTicks(8117));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 559,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 674, DateTimeKind.Utc).AddTicks(8882));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 560,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 728, DateTimeKind.Utc).AddTicks(7821));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 562,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 745, DateTimeKind.Utc).AddTicks(7393));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 563,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 753, DateTimeKind.Utc).AddTicks(3546));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 564,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 767, DateTimeKind.Utc).AddTicks(8393));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 567,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 832, DateTimeKind.Utc).AddTicks(2251));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 568,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 841, DateTimeKind.Utc).AddTicks(1854));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 569,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 863, DateTimeKind.Utc).AddTicks(1388));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 570,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 868, DateTimeKind.Utc).AddTicks(1289));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 571,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 878, DateTimeKind.Utc).AddTicks(4305));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 572,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 898, DateTimeKind.Utc).AddTicks(6553));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 573,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 911, DateTimeKind.Utc).AddTicks(6464));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 574,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 939, DateTimeKind.Utc).AddTicks(2089));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 575,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 947, DateTimeKind.Utc).AddTicks(2634));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 576,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 958, DateTimeKind.Utc).AddTicks(1504));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 577,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 969, DateTimeKind.Utc).AddTicks(128));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 578,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 52, 983, DateTimeKind.Utc).AddTicks(4721));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 580,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 12, DateTimeKind.Utc).AddTicks(8234));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 581,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 97, DateTimeKind.Utc).AddTicks(7269));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 582,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 106, DateTimeKind.Utc).AddTicks(4112));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 583,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 110, DateTimeKind.Utc).AddTicks(1776));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 584,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 153, DateTimeKind.Utc).AddTicks(7478));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 585,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 193, DateTimeKind.Utc).AddTicks(2843));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 586,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 202, DateTimeKind.Utc).AddTicks(7117));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 587,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 276, DateTimeKind.Utc).AddTicks(4802));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 588,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 284, DateTimeKind.Utc).AddTicks(3678));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 589,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 294, DateTimeKind.Utc).AddTicks(5339));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 590,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 317, DateTimeKind.Utc).AddTicks(7605));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 591,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 326, DateTimeKind.Utc).AddTicks(8944));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 592,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 361, DateTimeKind.Utc).AddTicks(6613));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 593,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 447, DateTimeKind.Utc).AddTicks(4961));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 594,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 481, DateTimeKind.Utc).AddTicks(2686));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 595,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 565, DateTimeKind.Utc).AddTicks(7419));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 596,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 575, DateTimeKind.Utc).AddTicks(8802));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 597,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 581, DateTimeKind.Utc).AddTicks(4955));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 598,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 591, DateTimeKind.Utc).AddTicks(4876));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 599,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 600, DateTimeKind.Utc).AddTicks(4957));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 600,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 609, DateTimeKind.Utc).AddTicks(6439));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 601,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 620, DateTimeKind.Utc).AddTicks(924));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 602,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 659, DateTimeKind.Utc).AddTicks(4812));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 603,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 670, DateTimeKind.Utc).AddTicks(2174));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 604,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 681, DateTimeKind.Utc).AddTicks(8922));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 605,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 720, DateTimeKind.Utc).AddTicks(6264));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 607,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 765, DateTimeKind.Utc).AddTicks(1377));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 608,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 770, DateTimeKind.Utc).AddTicks(9302));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 609,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 792, DateTimeKind.Utc).AddTicks(3934));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 610,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 800, DateTimeKind.Utc).AddTicks(7248));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 611,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 806, DateTimeKind.Utc).AddTicks(9584));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 612,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 816, DateTimeKind.Utc).AddTicks(4237));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 613,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 833, DateTimeKind.Utc).AddTicks(6972));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 614,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 851, DateTimeKind.Utc).AddTicks(5543));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 616,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 918, DateTimeKind.Utc).AddTicks(5122));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 617,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 925, DateTimeKind.Utc).AddTicks(9121));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 618,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 938, DateTimeKind.Utc).AddTicks(2083));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 619,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 943, DateTimeKind.Utc).AddTicks(6047));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 620,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 957, DateTimeKind.Utc).AddTicks(9663));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 621,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 965, DateTimeKind.Utc).AddTicks(9121));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 622,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 53, 970, DateTimeKind.Utc).AddTicks(9807));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 623,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 0, DateTimeKind.Utc).AddTicks(2791));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 624,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 6, DateTimeKind.Utc).AddTicks(6778));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 625,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 14, DateTimeKind.Utc).AddTicks(6551));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 627,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 28, DateTimeKind.Utc).AddTicks(9161));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 628,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 45, DateTimeKind.Utc).AddTicks(1494));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 629,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 80, DateTimeKind.Utc).AddTicks(5534));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 630,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 102, DateTimeKind.Utc).AddTicks(5623));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 631,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 111, DateTimeKind.Utc).AddTicks(2932));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 632,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 132, DateTimeKind.Utc).AddTicks(6449));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 633,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 136, DateTimeKind.Utc).AddTicks(5423));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 634,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 172, DateTimeKind.Utc).AddTicks(2689));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 635,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 204, DateTimeKind.Utc).AddTicks(2742));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 636,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 240, DateTimeKind.Utc).AddTicks(2584));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 637,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 263, DateTimeKind.Utc).AddTicks(2384));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 638,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 478, DateTimeKind.Utc).AddTicks(5484));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 639,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 507, DateTimeKind.Utc).AddTicks(7812));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 640,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 624, DateTimeKind.Utc).AddTicks(5245));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 641,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 672, DateTimeKind.Utc).AddTicks(8922));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 643,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 729, DateTimeKind.Utc).AddTicks(9583));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 644,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 814, DateTimeKind.Utc).AddTicks(9445));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 645,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 826, DateTimeKind.Utc).AddTicks(7225));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 646,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 835, DateTimeKind.Utc).AddTicks(4408));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 647,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 845, DateTimeKind.Utc).AddTicks(9936));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 648,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 853, DateTimeKind.Utc).AddTicks(4589));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 649,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 861, DateTimeKind.Utc).AddTicks(4203));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 650,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 876, DateTimeKind.Utc).AddTicks(8992));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 652,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 910, DateTimeKind.Utc).AddTicks(4921));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 654,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 944, DateTimeKind.Utc).AddTicks(207));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 655,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 54, 962, DateTimeKind.Utc).AddTicks(7458));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 656,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 5, DateTimeKind.Utc).AddTicks(3476));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 657,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 31, DateTimeKind.Utc).AddTicks(9174));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 658,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 50, DateTimeKind.Utc).AddTicks(2223));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 660,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 79, DateTimeKind.Utc).AddTicks(1761));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 661,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 84, DateTimeKind.Utc).AddTicks(4162));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 662,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 111, DateTimeKind.Utc).AddTicks(9447));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 663,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 149, DateTimeKind.Utc).AddTicks(9068));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 664,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 161, DateTimeKind.Utc).AddTicks(6907));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 665,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 179, DateTimeKind.Utc).AddTicks(3304));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 666,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 198, DateTimeKind.Utc).AddTicks(404));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 667,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 278, DateTimeKind.Utc).AddTicks(2421));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 668,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 344, DateTimeKind.Utc).AddTicks(3983));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 669,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 588, DateTimeKind.Utc).AddTicks(1327));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 670,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 591, DateTimeKind.Utc).AddTicks(6983));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 671,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 636, DateTimeKind.Utc).AddTicks(5453));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 672,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 645, DateTimeKind.Utc).AddTicks(9259));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 673,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 792, DateTimeKind.Utc).AddTicks(226));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 674,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 814, DateTimeKind.Utc).AddTicks(2213));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 675,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 839, DateTimeKind.Utc).AddTicks(937));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 676,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 849, DateTimeKind.Utc).AddTicks(981));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 677,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 922, DateTimeKind.Utc).AddTicks(1986));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 678,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 55, 941, DateTimeKind.Utc).AddTicks(7621));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 680,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 14, DateTimeKind.Utc).AddTicks(5621));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 681,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 81, DateTimeKind.Utc).AddTicks(2831));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 682,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 112, DateTimeKind.Utc).AddTicks(6644));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 683,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 138, DateTimeKind.Utc).AddTicks(1224));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 684,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 146, DateTimeKind.Utc).AddTicks(7094));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 685,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 155, DateTimeKind.Utc).AddTicks(7273));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 686,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 169, DateTimeKind.Utc).AddTicks(474));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 687,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 175, DateTimeKind.Utc).AddTicks(9108));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 688,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 185, DateTimeKind.Utc).AddTicks(1656));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 689,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 190, DateTimeKind.Utc).AddTicks(4081));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 690,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 220, DateTimeKind.Utc).AddTicks(8984));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 691,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 242, DateTimeKind.Utc).AddTicks(8506));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 692,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 269, DateTimeKind.Utc).AddTicks(3312));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 693,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 278, DateTimeKind.Utc).AddTicks(456));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 694,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 285, DateTimeKind.Utc).AddTicks(388));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 695,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 331, DateTimeKind.Utc).AddTicks(1297));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 696,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 343, DateTimeKind.Utc).AddTicks(9721));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 697,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 366, DateTimeKind.Utc).AddTicks(3448));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 699,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 424, DateTimeKind.Utc).AddTicks(474));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 702,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 467, DateTimeKind.Utc).AddTicks(19));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 703,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 539, DateTimeKind.Utc).AddTicks(9471));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 704,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 548, DateTimeKind.Utc).AddTicks(6677));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 706,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 610, DateTimeKind.Utc).AddTicks(3213));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 707,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 623, DateTimeKind.Utc).AddTicks(6532));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 709,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 665, DateTimeKind.Utc).AddTicks(9665));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 711,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 714, DateTimeKind.Utc).AddTicks(3332));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 712,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 724, DateTimeKind.Utc).AddTicks(3589));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 713,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 744, DateTimeKind.Utc).AddTicks(4958));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 714,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 756, DateTimeKind.Utc).AddTicks(4833));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 715,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 766, DateTimeKind.Utc).AddTicks(8754));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 716,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 769, DateTimeKind.Utc).AddTicks(4832));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 717,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 780, DateTimeKind.Utc).AddTicks(6399));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 718,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 819, DateTimeKind.Utc).AddTicks(1802));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 719,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 886, DateTimeKind.Utc).AddTicks(4589));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 722,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 938, DateTimeKind.Utc).AddTicks(7881));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 723,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 967, DateTimeKind.Utc).AddTicks(1823));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 724,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 56, 975, DateTimeKind.Utc).AddTicks(5167));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 725,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 5, DateTimeKind.Utc).AddTicks(7171));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 726,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 25, DateTimeKind.Utc).AddTicks(1879));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 727,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 40, DateTimeKind.Utc).AddTicks(7631));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 728,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 44, DateTimeKind.Utc).AddTicks(3927));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 729,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 58, DateTimeKind.Utc).AddTicks(2076));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 730,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 71, DateTimeKind.Utc).AddTicks(7959));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 731,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 90, DateTimeKind.Utc).AddTicks(3441));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 732,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 101, DateTimeKind.Utc).AddTicks(2098));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 733,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 138, DateTimeKind.Utc).AddTicks(9504));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 734,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 192, DateTimeKind.Utc).AddTicks(2091));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 735,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 197, DateTimeKind.Utc).AddTicks(675));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 736,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 259, DateTimeKind.Utc).AddTicks(3788));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 738,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 293, DateTimeKind.Utc).AddTicks(6827));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 739,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 321, DateTimeKind.Utc).AddTicks(8724));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 740,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 326, DateTimeKind.Utc).AddTicks(826));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 741,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 357, DateTimeKind.Utc).AddTicks(1796));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 742,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 390, DateTimeKind.Utc).AddTicks(9588));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 743,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 403, DateTimeKind.Utc).AddTicks(6001));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 744,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 465, DateTimeKind.Utc).AddTicks(3061));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 745,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 489, DateTimeKind.Utc).AddTicks(8349));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 746,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 529, DateTimeKind.Utc).AddTicks(5419));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 747,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 559, DateTimeKind.Utc).AddTicks(4337));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 748,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 573, DateTimeKind.Utc).AddTicks(8658));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 749,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 601, DateTimeKind.Utc).AddTicks(5809));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 750,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 616, DateTimeKind.Utc).AddTicks(5596));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 751,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 624, DateTimeKind.Utc).AddTicks(5351));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 752,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 692, DateTimeKind.Utc).AddTicks(3448));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 753,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 719, DateTimeKind.Utc).AddTicks(5654));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 754,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 917, DateTimeKind.Utc).AddTicks(2358));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 755,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 934, DateTimeKind.Utc).AddTicks(4302));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 756,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 57, 991, DateTimeKind.Utc).AddTicks(6766));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 757,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 2, DateTimeKind.Utc).AddTicks(3897));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 758,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 14, DateTimeKind.Utc).AddTicks(2983));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 759,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 26, DateTimeKind.Utc).AddTicks(8653));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 760,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 37, DateTimeKind.Utc).AddTicks(7959));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 761,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 59, DateTimeKind.Utc).AddTicks(7136));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 763,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 103, DateTimeKind.Utc).AddTicks(601));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 764,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 111, DateTimeKind.Utc).AddTicks(1652));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 765,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 116, DateTimeKind.Utc).AddTicks(7131));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 766,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 125, DateTimeKind.Utc).AddTicks(3824));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 768,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 159, DateTimeKind.Utc).AddTicks(9909));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 770,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 233, DateTimeKind.Utc).AddTicks(5441));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 771,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 275, DateTimeKind.Utc).AddTicks(856));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 772,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 323, DateTimeKind.Utc).AddTicks(6614));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 773,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 335, DateTimeKind.Utc).AddTicks(65));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 774,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 376, DateTimeKind.Utc).AddTicks(3288));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 775,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 463, DateTimeKind.Utc).AddTicks(4534));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 776,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 474, DateTimeKind.Utc).AddTicks(405));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 777,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 528, DateTimeKind.Utc).AddTicks(1444));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 778,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 534, DateTimeKind.Utc).AddTicks(2328));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 779,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 575, DateTimeKind.Utc).AddTicks(4856));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 780,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 630, DateTimeKind.Utc).AddTicks(3489));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 781,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 16, 15, 58, 633, DateTimeKind.Utc).AddTicks(7379));

            migrationBuilder.DeleteData(table: "UserRoles", keyColumns: new[] { "RolesId", "UsersId" }, keyValues: new object[] { 1, 1 });
            migrationBuilder.DeleteData(table: "Users", keyColumn: "Id", keyValue: 1);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "IsActive", "IsArchived", "MemberId", "PasswordHash", "ResetToken", "ResetTokenExpiry", "Username" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, null, "$2a$11$CQ4KnTDZ7qUQMNre86iruOpgOx8fEoMe2G3RF/1U4cCLa5ltYtE1O", null, null, "superadmin" });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RolesId", "UsersId" },
                values: new object[] { 1, 1 });
        }
    }
}
