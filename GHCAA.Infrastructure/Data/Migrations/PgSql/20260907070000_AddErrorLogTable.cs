using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <summary>
    /// docs/TODO.md 45.2. New ErrorLogs table for unhandled exceptions captured by
    /// ExceptionMiddleware.
    ///
    /// Hand-written rather than scaffolded: with an unrelated rename in flight elsewhere in the
    /// tree at the same time (see 20260907063000_RenameIsDeletedToIsArchived), `dotnet ef
    /// migrations add` diffed against that half-finished model and proposed deleting rows out of
    /// AcademicRecords/etc. and truncating PgSqlApplicationDbContextModelSnapshot.cs from ~74k
    /// lines to ~3.6k (see gotcha_ef_migrations_add_remove_corrupts_snapshot) — none of which
    /// belongs in this migration, so the snapshot was restored via `git checkout` and this
    /// migration was written by hand instead, same as AddFundraisingCampaigns.
    /// </summary>
    public partial class AddErrorLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Level = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    ExceptionType = table.Column<string>(type: "text", nullable: true),
                    StackTrace = table.Column<string>(type: "text", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: true),
                    RequestPath = table.Column<string>(type: "text", nullable: true),
                    RequestMethod = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    Username = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogs", x => x.Id);
                });

            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_ErrorLogs_OccurredAt"" ON ""ErrorLogs"" (""OccurredAt"");");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_ErrorLogs_Level"" ON ""ErrorLogs"" (""Level"");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErrorLogs");
        }
    }
}
