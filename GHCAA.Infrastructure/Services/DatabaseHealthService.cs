using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    public class DatabaseHealthService : IDatabaseHealthService
    {
        private readonly ApplicationDbContext _db;

        public DatabaseHealthService(ApplicationDbContext db)
        {
            _db = db;
        }

        public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        {
            return _db.Database.CanConnectAsync(cancellationToken);
        }
    }
}
