using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMemberAcademicFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubjectGroup",
                table: "Members",
                newName: "HighestCertificateSubject");

            migrationBuilder.RenameColumn(
                name: "LastCertificateFromGHC",
                table: "Members",
                newName: "HighestCertificateGroup");

            migrationBuilder.AlterColumn<int>(
                name: "HSCAdmissionYear",
                table: "Members",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "GHCAdmissionYear",
                table: "Members",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "GHCLastCertificate",
                table: "Members",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GHCLastCertificateGroup",
                table: "Members",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GHCLastCertificateSubject",
                table: "Members",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HighestCertificate",
                table: "Members",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "HighestCertificatePassingYear",
                table: "Members",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "ECMembers",
                keyColumn: "Id",
                keyValue: 1,
                column: "Position",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ECMembers",
                keyColumn: "Id",
                keyValue: 2,
                column: "Position",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ECPosition", "GHCLastCertificate", "GHCLastCertificateGroup", "GHCLastCertificateSubject", "HighestCertificate", "HighestCertificateGroup", "HighestCertificatePassingYear", "HighestCertificateSubject" },
                values: new object[] { 1, "HSC", "Science", "None", "HSC", "Science", 2015, "None" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ECPosition", "GHCLastCertificate", "GHCLastCertificateGroup", "GHCLastCertificateSubject", "HighestCertificate", "HighestCertificateGroup", "HighestCertificatePassingYear", "HighestCertificateSubject" },
                values: new object[] { 3, "HSC", "Humanities", "None", "HSC", "Humanities", 2016, "None" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GHCLastCertificate",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "GHCLastCertificateGroup",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "GHCLastCertificateSubject",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "HighestCertificate",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "HighestCertificatePassingYear",
                table: "Members");

            migrationBuilder.RenameColumn(
                name: "HighestCertificateSubject",
                table: "Members",
                newName: "SubjectGroup");

            migrationBuilder.RenameColumn(
                name: "HighestCertificateGroup",
                table: "Members",
                newName: "LastCertificateFromGHC");

            migrationBuilder.AlterColumn<int>(
                name: "HSCAdmissionYear",
                table: "Members",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GHCAdmissionYear",
                table: "Members",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "ECMembers",
                keyColumn: "Id",
                keyValue: 1,
                column: "Position",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ECMembers",
                keyColumn: "Id",
                keyValue: 2,
                column: "Position",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ECPosition", "LastCertificateFromGHC", "SubjectGroup" },
                values: new object[] { 0, "HSC", "Science" });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ECPosition", "LastCertificateFromGHC", "SubjectGroup" },
                values: new object[] { 2, "HSC", "Humanities" });
        }
    }
}
