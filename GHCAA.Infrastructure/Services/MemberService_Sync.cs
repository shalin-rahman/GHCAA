using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    public partial class MemberService
    {
        public async Task<int> SyncAlumniAsync(CancellationToken cancellationToken = default)
        {
            // Direct SQL Bulk Update to promote all Applied (0) to Active (1)
            var count = await _db.Database.ExecuteSqlRawAsync(
                "UPDATE \"Members\" SET \"Status\" = 1 WHERE \"Status\" = 0 AND \"IsArchived\" = false",
                cancellationToken);

            // Log as system activity (SuperAdmin ID: 1)
            await _activityService.LogActivityAsync(1, "SyncAlumni", $"Bulk promoted {count} alumni to Active status.", 1, cancellationToken: cancellationToken);

            _logger.LogInformation("SyncAlumni: {Count} members promoted to Active status.", count);
            return count;
        }
    }
}
