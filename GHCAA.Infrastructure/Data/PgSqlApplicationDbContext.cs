using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Data
{
    public class PgSqlApplicationDbContext : ApplicationDbContext
    {
        public PgSqlApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
