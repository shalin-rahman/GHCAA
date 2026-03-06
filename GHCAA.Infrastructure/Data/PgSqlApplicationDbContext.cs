using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Data
{
    public class PgSqlApplicationDbContext : ApplicationDbContext
    {
        public PgSqlApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }
    }
}
