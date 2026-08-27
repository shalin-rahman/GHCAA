using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Data
{
    // Shims for legacy migrations that reference specific DbContext types
    public class PgSqlApplicationDbContext : ApplicationDbContext
    {
        public PgSqlApplicationDbContext(DbContextOptions<PgSqlApplicationDbContext> options) : base(options) { }
    }

    public class MySqlApplicationDbContext : ApplicationDbContext
    {
        public MySqlApplicationDbContext(DbContextOptions<MySqlApplicationDbContext> options) : base(options) { }
    }

    public class SqliteApplicationDbContext : ApplicationDbContext
    {
        public SqliteApplicationDbContext(DbContextOptions<SqliteApplicationDbContext> options) : base(options) { }
    }
}
