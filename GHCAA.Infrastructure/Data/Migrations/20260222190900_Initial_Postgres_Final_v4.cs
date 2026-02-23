using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GHCAA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Postgres_Final_v4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Variables = table.Column<string>(type: "text", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventGalleries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    EventDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByAdminId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventGalleries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    RecordType = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByAdminId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FatherName = table.Column<string>(type: "text", nullable: false),
                    MotherName = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    BloodGroup = table.Column<int>(type: "integer", nullable: false),
                    NID = table.Column<string>(type: "text", nullable: false),
                    MobileNo = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    EmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    PresentAddress = table.Column<string>(type: "text", nullable: false),
                    PermanentAddress = table.Column<string>(type: "text", nullable: false),
                    EmergencyContactName = table.Column<string>(type: "text", nullable: false),
                    EmergencyContactRelation = table.Column<string>(type: "text", nullable: false),
                    EmergencyContactPhone = table.Column<string>(type: "text", nullable: false),
                    HSCAdmissionYear = table.Column<int>(type: "integer", nullable: false),
                    GHCAdmissionYear = table.Column<int>(type: "integer", nullable: false),
                    LastCertificateFromGHC = table.Column<string>(type: "text", nullable: false),
                    SubjectGroup = table.Column<string>(type: "text", nullable: false),
                    GHCLastCertificatePassingYear = table.Column<int>(type: "integer", nullable: false),
                    ProfessionalSector = table.Column<string>(type: "text", nullable: false),
                    Designation = table.Column<string>(type: "text", nullable: false),
                    PhotoPath = table.Column<string>(type: "text", nullable: true),
                    CertificatePath = table.Column<string>(type: "text", nullable: true),
                    PaymentProofPath = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AppliedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<int>(type: "integer", nullable: true),
                    MembershipNumber = table.Column<string>(type: "text", nullable: true),
                    IsMobilePublic = table.Column<bool>(type: "boolean", nullable: false),
                    IsEmailPublic = table.Column<bool>(type: "boolean", nullable: false),
                    IsAddressPublic = table.Column<bool>(type: "boolean", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MembershipType = table.Column<int>(type: "integer", nullable: false),
                    ECPosition = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MembershipFeeConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MembershipType = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByAdminId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipFeeConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Otps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    ExpiryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    Purpose = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Otps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventGalleryId = table.Column<int>(type: "integer", nullable: false),
                    PhotoPath = table.Column<string>(type: "text", nullable: false),
                    Caption = table.Column<string>(type: "text", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventPhotos_EventGalleries_EventGalleryId",
                        column: x => x.EventGalleryId,
                        principalTable: "EventGalleries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MemberId = table.Column<int>(type: "integer", nullable: true),
                    ActorId = table.Column<int>(type: "integer", nullable: true),
                    ActivityType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IPAddress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityLogs_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SenderId = table.Column<int>(type: "integer", nullable: false),
                    ReceiverId = table.Column<int>(type: "integer", nullable: false),
                    MessageContent = table.Column<string>(type: "text", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Members_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Members_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FileUploads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MemberId = table.Column<int>(type: "integer", nullable: false),
                    UploadType = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileUploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileUploads_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobOpportunities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Company = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Requirements = table.Column<string>(type: "text", nullable: false),
                    ContactEmail = table.Column<string>(type: "text", nullable: false),
                    ApplicationLink = table.Column<string>(type: "text", nullable: true),
                    PostedByMemberId = table.Column<int>(type: "integer", nullable: false),
                    PostedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    PostedById = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOpportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobOpportunities_Members_PostedById",
                        column: x => x.PostedById,
                        principalTable: "Members",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MembershipHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MemberId = table.Column<int>(type: "integer", nullable: false),
                    ChangedFrom = table.Column<string>(type: "text", nullable: false),
                    ChangedTo = table.Column<string>(type: "text", nullable: false),
                    ChangedByAdminId = table.Column<int>(type: "integer", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipHistories_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MemberId = table.Column<int>(type: "integer", nullable: false),
                    TransactionId = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentHistories_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    MemberId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MembershipDues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MemberId = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentHistoryId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipDues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipDues_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MembershipDues_PaymentHistories_PaymentHistoryId",
                        column: x => x.PaymentHistoryId,
                        principalTable: "PaymentHistories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NewsPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    PublishDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    AuthorId = table.Column<int>(type: "integer", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsPosts_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    RolesId = table.Column<int>(type: "integer", nullable: false),
                    UsersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.RolesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "Id", "Body", "Code", "LastUpdated", "Subject", "Variables" },
                values: new object[,]
                {
                    { 1, "Hello {{FullName}}, your OTP is: <strong>{{OtpCode}}</strong>. Valid for 10 minutes.", "OTP_EMAIL", new DateTime(2026, 2, 22, 19, 8, 59, 273, DateTimeKind.Utc).AddTicks(6386), "Your GHC Alumni Association Verification Code", "['FullName', 'OtpCode']" },
                    { 2, "Dear {{FullName}}, welcome! Your membership number is {{MembershipNumber}} and your default password is {{DefaultPassword}}.", "WELCOME_EMAIL", new DateTime(2026, 2, 22, 19, 8, 59, 273, DateTimeKind.Utc).AddTicks(7688), "Welcome to GHC Alumni Association!", "['FullName', 'MembershipNumber', 'DefaultPassword']" }
                });

            migrationBuilder.InsertData(
                table: "Lookups",
                columns: new[] { "Id", "Category", "DisplayOrder", "IsActive", "Label", "Value" },
                values: new object[,]
                {
                    { 1, "Degree", 1, true, "HSC", "HSC" },
                    { 2, "Degree", 2, true, "Bachelor / Honours", "Bachelor" },
                    { 3, "Degree", 3, true, "Masters", "Masters" },
                    { 4, "Degree", 4, true, "PhD", "PhD" },
                    { 5, "Degree", 5, true, "Other", "Other" },
                    { 6, "ProfessionalSector", 1, true, "Teaching / Education", "Teaching" },
                    { 7, "ProfessionalSector", 2, true, "Business", "Business" },
                    { 8, "ProfessionalSector", 3, true, "Information Technology", "IT" },
                    { 9, "ProfessionalSector", 4, true, "Medical / Healthcare", "Medical" },
                    { 10, "ProfessionalSector", 5, true, "Government Service", "Government" },
                    { 11, "ProfessionalSector", 6, true, "Other", "Other" }
                });

            migrationBuilder.InsertData(
                table: "Members",
                columns: new[] { "Id", "AppliedDate", "ApprovedBy", "ApprovedDate", "BloodGroup", "CertificatePath", "DateOfBirth", "Designation", "ECPosition", "Email", "EmailVerified", "EmergencyContactName", "EmergencyContactPhone", "EmergencyContactRelation", "FatherName", "FullName", "GHCAdmissionYear", "GHCLastCertificatePassingYear", "Gender", "HSCAdmissionYear", "IsAddressPublic", "IsArchived", "IsEmailPublic", "IsMobilePublic", "LastCertificateFromGHC", "LastUpdateDate", "MembershipNumber", "MembershipType", "MobileNo", "MotherName", "NID", "PaymentProofPath", "PermanentAddress", "PhotoPath", "PresentAddress", "ProfessionalSector", "Status", "SubjectGroup" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 0, null, new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Admin", 3, "shalin.rahman@gmail.com", false, "Emergency", "01700000000", "Family", "Father", "Habibur Rahman Shalin", 1950, 1952, 0, 1950, false, false, false, false, "Other", new DateTime(2026, 2, 22, 19, 8, 59, 275, DateTimeKind.Utc).AddTicks(4815), "ADM-SHALIN-1", 4, "01700000001", "Mother", "0000000001", null, "Munshiganj", null, "Munshiganj", "Other", 1, "Other" });

            migrationBuilder.InsertData(
                table: "MembershipFeeConfigs",
                columns: new[] { "Id", "Amount", "CreatedAt", "CreatedByAdminId", "Description", "EffectiveDate", "MembershipType" },
                values: new object[,]
                {
                    { 1, 5000m, new DateTime(2026, 2, 22, 19, 8, 59, 847, DateTimeKind.Utc).AddTicks(2420), null, "Founding Member Fee", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 2, 2000m, new DateTime(2026, 2, 22, 19, 8, 59, 847, DateTimeKind.Utc).AddTicks(3689), null, "Executive Member Fee", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 3, 1000m, new DateTime(2026, 2, 22, 19, 8, 59, 847, DateTimeKind.Utc).AddTicks(3692), null, "General Member Fee", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 4, 1000m, new DateTime(2026, 2, 22, 19, 8, 59, 847, DateTimeKind.Utc).AddTicks(3694), null, "Associate Member Fee", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3 },
                    { 5, 0m, new DateTime(2026, 2, 22, 19, 8, 59, 847, DateTimeKind.Utc).AddTicks(3696), null, "Life Member Fee", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6 }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "SuperAdmin" },
                    { 2, "Admin" },
                    { 3, "Member" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "IsActive", "IsArchived", "MemberId", "PasswordHash", "Username" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, null, "$2a$11$yUryc8gFlef8/.jJugVivORnhn76z3IW1HsiAiRjrIvZfPltqSlaC", "superadmin" });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RolesId", "UsersId" },
                values: new object[] { 1, 1 });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "IsActive", "IsArchived", "MemberId", "PasswordHash", "Username" },
                values: new object[] { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, 1, "$2a$11$Uoul61VXNQMEPT/uTJSutuHZj.I4quWIR43OTe5FjkKccDodHEj0K", "shalin" });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RolesId", "UsersId" },
                values: new object[] { 2, 2 });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_MemberId_Timestamp",
                table: "ActivityLogs",
                columns: new[] { "MemberId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ReceiverId",
                table: "ChatMessages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId",
                table: "ChatMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Code",
                table: "EmailTemplates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventPhotos_EventGalleryId",
                table: "EventPhotos",
                column: "EventGalleryId");

            migrationBuilder.CreateIndex(
                name: "IX_FileUploads_MemberId_UploadType",
                table: "FileUploads",
                columns: new[] { "MemberId", "UploadType" });

            migrationBuilder.CreateIndex(
                name: "IX_JobOpportunities_PostedById",
                table: "JobOpportunities",
                column: "PostedById");

            migrationBuilder.CreateIndex(
                name: "IX_Lookups_Category_Value",
                table: "Lookups",
                columns: new[] { "Category", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_Email",
                table: "Members",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_MobileNo",
                table: "Members",
                column: "MobileNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_NID",
                table: "Members",
                column: "NID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipDues_MemberId",
                table: "MembershipDues",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipDues_PaymentHistoryId",
                table: "MembershipDues",
                column: "PaymentHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipHistories_MemberId",
                table: "MembershipHistories",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsPosts_AuthorId",
                table: "NewsPosts",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistories_MemberId_TransactionId",
                table: "PaymentHistories",
                columns: new[] { "MemberId", "TransactionId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UsersId",
                table: "UserRoles",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_MemberId",
                table: "Users",
                column: "MemberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "ContactMessages");

            migrationBuilder.DropTable(
                name: "EmailTemplates");

            migrationBuilder.DropTable(
                name: "EventPhotos");

            migrationBuilder.DropTable(
                name: "FileUploads");

            migrationBuilder.DropTable(
                name: "FinancialRecords");

            migrationBuilder.DropTable(
                name: "JobOpportunities");

            migrationBuilder.DropTable(
                name: "Lookups");

            migrationBuilder.DropTable(
                name: "MembershipDues");

            migrationBuilder.DropTable(
                name: "MembershipFeeConfigs");

            migrationBuilder.DropTable(
                name: "MembershipHistories");

            migrationBuilder.DropTable(
                name: "NewsPosts");

            migrationBuilder.DropTable(
                name: "Otps");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "EventGalleries");

            migrationBuilder.DropTable(
                name: "PaymentHistories");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Members");
        }
    }
}
