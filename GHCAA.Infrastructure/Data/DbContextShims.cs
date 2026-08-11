using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Data
{
    // Shims for legacy migrations that reference specific DbContext types
    public class PgSqlApplicationDbContext : ApplicationDbContext
    {
        public PgSqlApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    }

    public class MySqlApplicationDbContext : ApplicationDbContext
    {
        public MySqlApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    }

    public class SqliteApplicationDbContext : ApplicationDbContext
    {
        public SqliteApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    }
}
