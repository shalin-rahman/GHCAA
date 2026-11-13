using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddElectionFormPrints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 37.14t. One row per filled form print. The QR code holds only the row id, and the row
            // keeps the SHA-256 of what was printed. Guarded because EnsureCreated may have built it.
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "ElectionFormPrints" (
                    "Id" uuid NOT NULL,
                    "ElectionId" integer NOT NULL,
                    "FormCode" character varying(16) NOT NULL,
                    "ContentHash" character varying(64) NOT NULL,
                    "PrintedAt" timestamp with time zone NOT NULL,
                    "PrintedByUserId" integer NULL,
                    "PrintedByRole" character varying(100) NOT NULL,
                    "PhaseAtPrint" integer NOT NULL,
                    "RevokedAt" timestamp with time zone NULL,
                    "RevokedByUserId" integer NULL,
                    "RevokeReason" character varying(500) NULL,
                    CONSTRAINT "PK_ElectionFormPrints" PRIMARY KEY ("Id"),
                    CONSTRAINT "FK_ElectionFormPrints_Elections_ElectionId" FOREIGN KEY ("ElectionId") REFERENCES "Elections" ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_ElectionFormPrints_Users_PrintedByUserId" FOREIGN KEY ("PrintedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
                    CONSTRAINT "FK_ElectionFormPrints_Users_RevokedByUserId" FOREIGN KEY ("RevokedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
                );
                CREATE INDEX IF NOT EXISTS "IX_ElectionFormPrints_ElectionId_FormCode" ON "ElectionFormPrints" ("ElectionId", "FormCode");
                CREATE INDEX IF NOT EXISTS "IX_ElectionFormPrints_PrintedByUserId" ON "ElectionFormPrints" ("PrintedByUserId");
                CREATE INDEX IF NOT EXISTS "IX_ElectionFormPrints_RevokedByUserId" ON "ElectionFormPrints" ("RevokedByUserId");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Dropping loses the print records, so QR codes on paper already handed out stop verifying.
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS "ElectionFormPrints";
                """);
        }
    }
}
