using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GHCAA.Infrastructure.Data
{
    /// <summary>
    /// Design-time factory used by EF Core CLI tools (dotnet ef migrations add/update).
    /// Reads EF_PROVIDER environment variable to choose the database provider.
    ///
    /// Supported values: PgSql (default), MySql, Sqlite
    ///
    /// Usage examples:
    ///   -- PostgreSQL (default):
    ///      dotnet ef migrations add <name> --output-dir Data/Migrations/Postgres
    ///
    ///   -- MySQL:
    ///      $env:EF_PROVIDER="MySql"   (PowerShell)
    ///      dotnet ef migrations add <name> --output-dir Data/Migrations/MySql
    ///
    ///   -- SQLite:
    ///      $env:EF_PROVIDER="Sqlite"
    ///      dotnet ef migrations add <name> --output-dir Data/Migrations/Sqlite
    ///
    ///   Apply migrations at runtime:
    ///      dotnet ef database update --project GHCAA.Infrastructure --startup-project GHCAA.API
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var provider = Environment.GetEnvironmentVariable("EF_PROVIDER") ?? "PgSql";
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            if (provider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
            {
                const string conn = "Server=localhost;Port=3306;Database=GHCAADB;User=root;Password=root;";
                // Use hardcoded version so migration scaffolding works without a live DB.
                // MySQL 8.0 is the target. Change to match your actual MySQL version if needed.
                var mysqlVersion = new MySqlServerVersion(new Version(8, 0, 36));
                optionsBuilder.UseMySql(conn, mysqlVersion,
                    o => o.MigrationsAssembly("GHCAA.Infrastructure")
                           .MigrationsHistoryTable("__EFMigrationsHistory_MySql"));
            }
            else if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                const string conn = "Data Source=GHCAADB.db";
                optionsBuilder.UseSqlite(conn,
                    o => o.MigrationsAssembly("GHCAA.Infrastructure")
                           .MigrationsHistoryTable("__EFMigrationsHistory_Sqlite"));
            }
            else // PgSql (default)
            {
                const string conn = "Host=localhost;Port=5432;Database=GHCAADB;Username=postgres;Password=postgres;SslMode=Prefer;Timeout=30";
                optionsBuilder.UseNpgsql(conn,
                    o => o.MigrationsAssembly("GHCAA.Infrastructure")
                           .MigrationsHistoryTable("__EFMigrationsHistory_Postgres"));
            }

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
