using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface IActivityService
    {
        Task LogActivityAsync(int? memberId, string type, string description, int? actorId = null, string? ipAddress = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<ActivityLog>> GetMemberActivityAsync(int memberId, int count = 20, CancellationToken cancellationToken = default);
        Task<IEnumerable<ActivityLog>> GetRecentGlobalActivityAsync(int count = 50, CancellationToken cancellationToken = default);
    }
}
