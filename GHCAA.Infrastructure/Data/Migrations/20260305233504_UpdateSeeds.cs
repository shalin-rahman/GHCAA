using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Body", "Description", "Subject" },
                values: new object[] { "<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'><h2 style='color: #2c3e50;'>Verification Code</h2><p>Hello <strong>{{FullName}}</strong>,</p><p>Your security code is:</p><div style='font-size: 24px; font-weight: bold; background: #f8f9fa; padding: 15px; text-align: center; border-radius: 5px; color: #3498db;'>{{OtpCode}}</div><p>Valid for 10 minutes. Do not share this code.</p></div>", "Security code for login/registration", "GHCAA Verification Code: {{OtpCode}}" });

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Body", "Description" },
                values: new object[] { "<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'><h2 style='color: #2c3e50;'>Welcome to GHCAA</h2><p>Dear <strong>{{FullName}}</strong>,</p><p>Your membership has been approved! We are excited to have you as part of our community.</p><div style='background: #e8f4fd; padding: 15px; border-radius: 5px;'><p><strong>Membership No:</strong> {{MembershipNumber}}</p><p><strong>Default Password:</strong> <code style='background:#fff; padding:2px 5px;'>{{DefaultPassword}}</code></p></div><p>Please log in and change your password immediately.</p></div>", "Official induction message" });

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Body", "Code", "Description", "Subject", "Variables" },
                values: new object[] { "<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'><h2 style='color: #2c3e50;'>Subscription Reminder</h2><p>Dear <strong>{{FullName}}</strong>,</p><p>This is a reminder that your annual membership subscription is now due.</p><p>Maintaining an active status ensures you continue to receive all alumni benefits and voting rights.</p><p>Thank you for your continued support!</p></div>", "FEE_REMINDER", "Friendly reminder for yearly dues", "Annual Membership Subscription Due", "['FullName']" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Designation", "ECPosition", "EmailVerified", "GHCAdmissionYear", "GHCLastCertificatePassingYear", "HSCAdmissionYear", "LastCertificateFromGHC", "MembershipNumber", "MembershipType", "ProfessionalSector", "SubjectGroup" },
                values: new object[] { "Software Engineer", 0, true, 2013, 2015, 2013, "HSC", "GHC-2015-0001", 0, "Engineering", "Science" });

            migrationBuilder.InsertData(
                table: "Members",
                columns: new[] { "Id", "AppliedDate", "ApprovedBy", "ApprovedDate", "BloodGroup", "Category", "CertificatePath", "DateOfBirth", "Designation", "ECPosition", "Email", "EmailVerified", "EmergencyContactName", "EmergencyContactPhone", "EmergencyContactRelation", "FatherName", "FullName", "GHCAdmissionYear", "GHCLastCertificatePassingYear", "Gender", "HSCAdmissionYear", "IsAddressPublic", "IsArchived", "IsEmailPublic", "IsMobilePublic", "LastCertificateFromGHC", "LastUpdateDate", "MembershipNumber", "MembershipType", "MobileNo", "MotherName", "NID", "PaymentProofPath", "PermanentAddress", "PhotoPath", "PresentAddress", "ProfessionalSector", "Status", "SubjectGroup" },
                values: new object[] { 101, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 4, 0, null, new DateTime(1992, 5, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Communications Manager", 2, "jane@example.com", true, "Friend", "01700000003", "None", "James Doe", "Jane Doe", 2014, 2016, 1, 2014, false, false, false, false, "HSC", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "GHC-2016-0001", 2, "01700000002", "Mary Doe", "0000000002", null, "Dhaka", null, "Dhaka", "Corporate", 1, "Humanities" });

            migrationBuilder.InsertData(
                table: "ECMembers",
                columns: new[] { "Id", "ChangeReason", "ECPeriodId", "EndDate", "MemberId", "Position", "StartDate" },
                values: new object[] { 2, null, 1, null, 101, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ECMembers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Body", "Description", "Subject" },
                values: new object[] { "Hello {{FullName}}, your OTP is: <strong>{{OtpCode}}</strong>. Valid for 10 minutes.", "Sent during registration and password reset", "Your GHC Alumni Association Verification Code" });

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Body", "Description" },
                values: new object[] { "Dear {{FullName}}, welcome! Your membership number is {{MembershipNumber}} and your default password is {{DefaultPassword}}.", "Sent after admin approves registration" });

            migrationBuilder.UpdateData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Body", "Code", "Description", "Subject", "Variables" },
                values: new object[] { "Dear {{FullName}}, your registration for the event <strong>{{EventTitle}}</strong> has been approved. <br/><br/><strong>Event Details:</strong><br/>Date: {{EventDate}}<br/>Location: {{EventLocation}}<br/><br/>Looking forward to seeing you there!", "EVENT_REGISTRATION_CONFIRMATION", "Sent after event registration approval", "Registration Confirmed: {{EventTitle}}", "['FullName', 'EventTitle', 'EventDate', 'EventLocation']" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Designation", "ECPosition", "EmailVerified", "GHCAdmissionYear", "GHCLastCertificatePassingYear", "HSCAdmissionYear", "LastCertificateFromGHC", "MembershipNumber", "MembershipType", "ProfessionalSector", "SubjectGroup" },
                values: new object[] { "Admin", 9, false, 1950, 1952, 1950, "Other", "ADM-SHALIN-1", 4, "Other", "Other" });
        }
    }
}
