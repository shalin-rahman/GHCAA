using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <summary>
    /// Data Protection keys move from the container filesystem (KeyRingPath, or ASP.NET Core's
    /// own /root/.aspnet/DataProtection-Keys default) to the database, since Render mounts no
    /// persistent disk on this service and either file path silently resets on every redeploy.
    ///
    /// Hand-written rather than scaffolded: `dotnet ef migrations add` truncated the ~74k-line
    /// PgSqlApplicationDbContextModelSnapshot.cs to a few thousand lines and pulled in unrelated
    /// seed drift (see gotcha_ef_migrations_add_remove_corrupts_snapshot) — discarded, snapshot
    /// restored from the index, and the new entity added to both files by hand instead.
    /// </summary>
    public partial class AddDataProtectionKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataProtectionKeys");
        }
    }
}
