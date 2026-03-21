using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface IGamificationService
    {
        Task AwardPointsAsync(int memberId, string activityCode, int? relatedId = null, string? metadata = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<GamificationConfig>> GetConfigsAsync(CancellationToken cancellationToken = default);
        Task UpdateConfigAsync(int configId, int points, bool isActive, CancellationToken cancellationToken = default);
        Task<IEnumerable<MemberContributionDto>> GetLeaderboardAsync(int top = 20, CancellationToken cancellationToken = default);
    }
}
