using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GHCAA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedSamplePublicData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResetToken",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpiry",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AlumniEvents",
                columns: new[] { "Id", "AdminNote", "CreatedAt", "Date", "Description", "ImageUrl", "IsActive", "Location", "RegistrationDeadline", "RegistrationFee", "Title" },
                values: new object[] { 1, null, new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 5, 15, 9, 0, 0, 0, DateTimeKind.Utc), "The biggest gathering of Haragangians across the globe. Join us for a day of nostalgia, networking, and cultural celebrations.", "https://images.unsplash.com/photo-1511578334221-d748ef50b502?q=80&w=2070", true, "College Ground, Munshiganj", new DateTime(2026, 4, 30, 23, 59, 59, 0, DateTimeKind.Utc), 1500m, "Grand Reunion 2026" });

            migrationBuilder.InsertData(
                table: "JobOpportunities",
                columns: new[] { "Id", "ApplicationLink", "Category", "Company", "ContactEmail", "Description", "ExpiryDate", "IsActive", "Location", "PostedById", "PostedByMemberId", "PostedDate", "Requirements", "Title" },
                values: new object[] { 1, null, 0, "GlobalTech Solutions", "careers@globaltech.com", "Looking for an experienced architect to lead our fintech transition. Great benefits and remote flexibility.", new DateTime(2026, 4, 30, 23, 59, 59, 0, DateTimeKind.Utc), true, "Dhaka, Bangladesh", null, 1, new DateTime(2026, 2, 24, 0, 0, 0, 0, DateTimeKind.Utc), "10+ years of experience, C# Experts only.", "Senior Software Architect" });

            migrationBuilder.InsertData(
                table: "NewsPosts",
                columns: new[] { "Id", "AuthorId", "Category", "Content", "ImageUrl", "IsActive", "LastModified", "PublishDate", "Title" },
                values: new object[,]
                {
                    { 1, 2, 1, "The historic library of Govt. Haraganga College has been fully renovated with modern amenities and digital archiving systems, funded by the 1985 batch alumni.", "https://images.unsplash.com/photo-1524995997946-a1c2e315a42f?q=80&w=2070", true, null, new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "College Library Renovation Project Completed" },
                    { 2, 2, 0, "GHCAA members in the UK gathered at the Royal Museum today to discuss international networking and scholarship opportunities for current students.", "https://images.unsplash.com/photo-1513635269975-59663e0ac1ad?q=80&w=2070", true, null, new DateTime(2026, 3, 4, 0, 0, 0, 0, DateTimeKind.Utc), "Haragangian Global Meet 2026: London Chapter" }
                });

            migrationBuilder.InsertData(
                table: "SpecialDayThemes",
                columns: new[] { "Id", "AnnouncementText", "BackgroundColor", "EndDate", "IsEnabled", "StartDate", "TextColor", "Title" },
                values: new object[] { 1, "Celebrating 55 Years of Victory! Happy Independence Day to all Haragangians.", "#213921", new DateTime(2026, 3, 27, 23, 59, 59, 0, DateTimeKind.Utc), true, new DateTime(2026, 3, 20, 0, 0, 0, 0, DateTimeKind.Utc), "#dc2626", "Independence Day 2026" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ResetToken", "ResetTokenExpiry" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ResetToken", "ResetTokenExpiry" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AlumniEvents",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "JobOpportunities",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "NewsPosts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "NewsPosts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SpecialDayThemes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "ResetToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ResetTokenExpiry",
                table: "Users");
        }
    }
}
