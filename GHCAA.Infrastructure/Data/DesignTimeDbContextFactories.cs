using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GHCAA.Infrastructure.Data
{
    public abstract class BaseDesignTimeDbContextFactory<T> : IDesignTimeDbContextFactory<T> where T : DbContext
    {
        protected IConfiguration Configuration { get; }

        protected BaseDesignTimeDbContextFactory()
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "GHCAA.API");
            Configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public abstract T CreateDbContext(string[] args);
    }

    public class PgSqlDesignTimeDbContextFactory : BaseDesignTimeDbContextFactory<PgSqlApplicationDbContext>
    {
        public override PgSqlApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var conn = Configuration.GetConnectionString("PgSqlConnection");
            optionsBuilder.UseNpgsql(conn, o => o.MigrationsAssembly("GHCAA.Infrastructure"));
            
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            return new PgSqlApplicationDbContext(optionsBuilder.Options);
        }
    }

    public class MySqlDesignTimeDbContextFactory : BaseDesignTimeDbContextFactory<MySqlApplicationDbContext>
    {
        public override MySqlApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var conn = Configuration.GetConnectionString("MySqlConnection");
            var mysqlVersion = new MySqlServerVersion(new Version(8, 0, 36));
            optionsBuilder.UseMySql(conn, mysqlVersion, o => o.MigrationsAssembly("GHCAA.Infrastructure"));
            
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            return new MySqlApplicationDbContext(optionsBuilder.Options);
        }
    }

    public class SqliteDesignTimeDbContextFactory : BaseDesignTimeDbContextFactory<SqliteApplicationDbContext>
    {
        public override SqliteApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var conn = Configuration.GetConnectionString("SqliteConnection");
            optionsBuilder.UseSqlite(conn, o => o.MigrationsAssembly("GHCAA.Infrastructure"));
            
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            return new SqliteApplicationDbContext(optionsBuilder.Options);
        }
    }
}
