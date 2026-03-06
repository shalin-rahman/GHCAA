using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Data
{
    public class SqliteApplicationDbContext : ApplicationDbContext
    {
        public SqliteApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }
    }
}
