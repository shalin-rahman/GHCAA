using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GHCAA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveAndFeaturedToGallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "EventGalleries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "EventGalleries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "EventGalleries",
                columns: new[] { "Id", "CreatedAt", "CreatedByAdminId", "Description", "EventDate", "IsActive", "IsFeatured", "Title" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Highlights from the 100th-anniversary gala of Haraganga College.", new DateTime(2025, 12, 10, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "Centennial Celebration" },
                    { 2, new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Scenic views of the historic GHC campus buildings and grounds.", new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "Campus Landscapes" }
                });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1,
                column: "DisplayOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DisplayOrder", "Label", "Value" },
                values: new object[] { 3, "Bachelor (Pass)", "Bachelor (Pass)" });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DisplayOrder", "Label", "Value" },
                values: new object[] { 4, "Bachelor (Honours)", "Bachelor (Honours)" });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DisplayOrder", "Label", "Value" },
                values: new object[] { 5, "Masters", "Masters" });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "DisplayOrder", "Label", "Value" },
                values: new object[] { 6, "PGD", "PGD" });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Category", "DisplayOrder", "Label", "Value" },
                values: new object[] { "Degree", 7, "PhD", "PhD" });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Category", "DisplayOrder", "Label", "Value" },
                values: new object[] { "Degree", 8, "Medicine", "Medicine" });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Category", "DisplayOrder", "Label", "Value" },
                values: new object[] { "Degree", 9, "Engineering", "Engineering" });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Category", "DisplayOrder", "Label", "Value" },
                values: new object[] { "Degree", 10, "Law", "Law" });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Category", "DisplayOrder", "Label", "Value" },
                values: new object[] { "AcademicGroup", 11, "Science", "Science" });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Category", "DisplayOrder", "Label", "Value" },
                values: new object[] { "AcademicGroup", 12, "Arts & Humanities", "Arts & Humanities" });

            migrationBuilder.InsertData(
                table: "Lookups",
                columns: new[] { "Id", "Category", "DisplayOrder", "IsActive", "Label", "Value" },
                values: new object[,]
                {
                    { 12, "AcademicGroup", 13, true, "Business Studies", "Business Studies" },
                    { 13, "AcademicSubject", 14, true, "None", "None" },
                    { 14, "AcademicSubject", 15, true, "Bengali", "Bengali" },
                    { 15, "AcademicSubject", 16, true, "English", "English" },
                    { 16, "AcademicSubject", 17, true, "History", "History" },
                    { 17, "AcademicSubject", 18, true, "Islamic History & Culture", "Islamic History & Culture" },
                    { 18, "AcademicSubject", 19, true, "Philosophy", "Philosophy" },
                    { 19, "AcademicSubject", 20, true, "Islamic Studies", "Islamic Studies" },
                    { 20, "AcademicSubject", 21, true, "Library Science", "Library Science" },
                    { 21, "AcademicSubject", 22, true, "Economics", "Economics" },
                    { 22, "AcademicSubject", 23, true, "Political Science", "Political Science" },
                    { 23, "AcademicSubject", 24, true, "Sociology", "Sociology" },
                    { 24, "AcademicSubject", 25, true, "Social Work", "Social Work" },
                    { 25, "AcademicSubject", 26, true, "Anthropology", "Anthropology" },
                    { 26, "AcademicSubject", 27, true, "Public Administration", "Public Administration" },
                    { 27, "AcademicSubject", 28, true, "Physics", "Physics" },
                    { 28, "AcademicSubject", 29, true, "Chemistry", "Chemistry" },
                    { 29, "AcademicSubject", 30, true, "Mathematics", "Mathematics" },
                    { 30, "AcademicSubject", 31, true, "Statistics", "Statistics" },
                    { 31, "AcademicSubject", 32, true, "Botany", "Botany" },
                    { 32, "AcademicSubject", 33, true, "Zoology", "Zoology" },
                    { 33, "AcademicSubject", 34, true, "Geography & Environment", "Geography & Environment" },
                    { 34, "AcademicSubject", 35, true, "Psychology", "Psychology" },
                    { 35, "AcademicSubject", 36, true, "Soil Science", "Soil Science" },
                    { 36, "AcademicSubject", 37, true, "Accounting", "Accounting" },
                    { 37, "AcademicSubject", 38, true, "Management", "Management" },
                    { 38, "AcademicSubject", 39, true, "Marketing", "Marketing" },
                    { 39, "AcademicSubject", 40, true, "Finance & Banking", "Finance & Banking" },
                    { 40, "AcademicSubject", 41, true, "Fine Arts", "Fine Arts" },
                    { 41, "AcademicSubject", 42, true, "Physical Education", "Physical Education" },
                    { 42, "AcademicSubject", 43, true, "Business Administration", "Business Administration" },
                    { 43, "AcademicSubject", 44, true, "Computer", "Computer" },
                    { 44, "AcademicSubject", 45, true, "Civil", "Civil" },
                    { 45, "AcademicSubject", 46, true, "Mechanical", "Mechanical" },
                    { 46, "AcademicSubject", 47, true, "Electrical", "Electrical" },
                    { 47, "AcademicSubject", 48, true, "Medical", "Medical" },
                    { 48, "AcademicSubject", 49, true, "Dentestry", "Dentestry" },
                    { 49, "AcademicSubject", 50, true, "Engineering", "Engineering" },
                    { 50, "AcademicSubject", 51, true, "Law", "Law" },
                    { 51, "AcademicSubject", 52, true, "Pharma", "Pharma" },
                    { 52, "AcademicSubject", 53, true, "Agriculture", "Agriculture" },
                    { 53, "AcademicSubject", 54, true, "Textile", "Textile" },
                    { 54, "AcademicSubject", 55, true, "Lather", "Lather" },
                    { 55, "AcademicSubject", 56, true, "Education", "Education" },
                    { 56, "ProfessionalSector", 57, true, "Ready-made Garments (RMG)", "Ready-made Garments (RMG)" },
                    { 57, "ProfessionalSector", 58, true, "Textiles & Spinning", "Textiles & Spinning" },
                    { 58, "ProfessionalSector", 59, true, "Pharmaceuticals", "Pharmaceuticals" },
                    { 59, "ProfessionalSector", 60, true, "Banking & Financial Services", "Banking & Financial Services" },
                    { 60, "ProfessionalSector", 61, true, "Information Technology (IT) & Software", "Information Technology (IT) & Software" },
                    { 61, "ProfessionalSector", 62, true, "Telecommunications", "Telecommunications" },
                    { 62, "ProfessionalSector", 63, true, "Agriculture & Crop Production", "Agriculture & Crop Production" },
                    { 63, "ProfessionalSector", 64, true, "Fisheries & Aquaculture", "Fisheries & Aquaculture" },
                    { 64, "ProfessionalSector", 65, true, "Livestock & Poultry", "Livestock & Poultry" },
                    { 65, "ProfessionalSector", 66, true, "Agro-processing & Food Production", "Agro-processing & Food Production" },
                    { 66, "ProfessionalSector", 67, true, "Leather & Footwear", "Leather & Footwear" },
                    { 67, "ProfessionalSector", 68, true, "Jute & Jute Goods", "Jute & Jute Goods" },
                    { 68, "ProfessionalSector", 69, true, "Light Engineering", "Light Engineering" },
                    { 69, "ProfessionalSector", 70, true, "Electronics & Electrical Appliances", "Electronics & Electrical Appliances" },
                    { 70, "ProfessionalSector", 71, true, "Real Estate & Housing", "Real Estate & Housing" },
                    { 71, "ProfessionalSector", 72, true, "Construction & Infrastructure", "Construction & Infrastructure" },
                    { 72, "ProfessionalSector", 73, true, "Healthcare & Medical Services", "Healthcare & Medical Services" },
                    { 73, "ProfessionalSector", 74, true, "Education & Research", "Education & Research" },
                    { 74, "ProfessionalSector", 75, true, "Tourism & Hospitality", "Tourism & Hospitality" },
                    { 75, "ProfessionalSector", 76, true, "Power, Energy & Mineral Resources", "Power, Energy & Mineral Resources" },
                    { 76, "ProfessionalSector", 77, true, "Steel & Re-rolling", "Steel & Re-rolling" },
                    { 77, "ProfessionalSector", 78, true, "Cement", "Cement" },
                    { 78, "ProfessionalSector", 79, true, "Ceramics", "Ceramics" },
                    { 79, "ProfessionalSector", 80, true, "Chemicals & Fertilizers", "Chemicals & Fertilizers" },
                    { 80, "ProfessionalSector", 81, true, "Shipbuilding", "Shipbuilding" },
                    { 81, "ProfessionalSector", 82, true, "Transportation & Logistics", "Transportation & Logistics" },
                    { 82, "ProfessionalSector", 83, true, "Fast-Moving Consumer Goods (FMCG)", "Fast-Moving Consumer Goods (FMCG)" },
                    { 83, "ProfessionalSector", 84, true, "Paper & Printing", "Paper & Printing" },
                    { 84, "ProfessionalSector", 85, true, "Plastic & Rubber Products", "Plastic & Rubber Products" },
                    { 85, "ProfessionalSector", 86, true, "Insurance", "Insurance" },
                    { 86, "ProfessionalSector", 87, true, "Advertising & Media", "Advertising & Media" },
                    { 87, "ProfessionalSector", 88, true, "Legal & Consultancy Services", "Legal & Consultancy Services" },
                    { 88, "ProfessionalSector", 89, true, "Public Administration & Defense", "Public Administration & Defense" }
                });

            migrationBuilder.InsertData(
                table: "EventPhotos",
                columns: new[] { "Id", "Caption", "EventGalleryId", "PhotoPath", "UploadedAt" },
                values: new object[,]
                {
                    { 1, "Gala Evening", 1, "https://images.unsplash.com/photo-1540575467063-178a50c2df87?q=80&w=2070", new DateTime(2026, 3, 6, 6, 56, 42, 778, DateTimeKind.Utc).AddTicks(3853) },
                    { 2, "Alumni Networking", 1, "https://images.unsplash.com/photo-1511795409834-ef04bbd61622?q=80&w=2069", new DateTime(2026, 3, 6, 6, 56, 42, 778, DateTimeKind.Utc).AddTicks(5249) },
                    { 3, "Main Administrative Building", 2, "https://images.unsplash.com/photo-1562774053-701939374585?q=80&w=1986", new DateTime(2026, 3, 6, 6, 56, 42, 778, DateTimeKind.Utc).AddTicks(5250) },
                    { 4, "College Playground", 2, "https://images.unsplash.com/photo-1492538350424-aaee9f201774?q=80&w=2070", new DateTime(2026, 3, 6, 6, 56, 42, 778, DateTimeKind.Utc).AddTicks(5252) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "EventPhotos",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 77);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 83);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 84);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 85);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 86);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 87);

            migrationBuilder.DeleteData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 88);

            migrationBuilder.DeleteData(
                table: "EventGalleries",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EventGalleries",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "EventGalleries");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "EventGalleries");

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1,
                column: "DisplayOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DisplayOrder", "Label", "Value" },
                values: new object[] { 2, "Bachelor / Honours", "Bachelor" });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DisplayOrder", "Label", "Value" },
                values: new object[] { 3, "Masters", "Masters" });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DisplayOrder", "Label", "Value" },
                values: new object[] { 4, "PhD", "PhD" });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "DisplayOrder", "Label", "Value" },
                values: new object[] { 5, "Other", "Other" });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Category", "DisplayOrder", "Label", "Value" },
                values: new object[] { "ProfessionalSector", 1, "Teaching / Education", "Teaching" });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Category", "DisplayOrder", "Label", "Value" },
                values: new object[] { "ProfessionalSector", 2, "Business", "Business" });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Category", "DisplayOrder", "Label", "Value" },
                values: new object[] { "ProfessionalSector", 3, "Information Technology", "IT" });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Category", "DisplayOrder", "Label", "Value" },
                values: new object[] { "ProfessionalSector", 4, "Medical / Healthcare", "Medical" });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Category", "DisplayOrder", "Label", "Value" },
                values: new object[] { "ProfessionalSector", 5, "Government Service", "Government" });

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Category", "DisplayOrder", "Label", "Value" },
                values: new object[] { "ProfessionalSector", 6, "Other", "Other" });
        }
    }
}
