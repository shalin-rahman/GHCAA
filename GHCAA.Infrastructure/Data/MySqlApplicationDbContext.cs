using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Data
{
    public class MySqlApplicationDbContext : ApplicationDbContext
    {
        public MySqlApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }
    }
}
