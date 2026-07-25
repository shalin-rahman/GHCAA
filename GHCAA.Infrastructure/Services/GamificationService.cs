using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    public class GamificationService : IGamificationService
    {
        private readonly ApplicationDbContext _db;
        private readonly IActivityService _activity;
        private readonly ILogger<GamificationService> _logger;

        public GamificationService(ApplicationDbContext db, IActivityService activity, ILogger<GamificationService> logger)
        {
            _db = db;
            _activity = activity;
            _logger = logger;
        }

        public async Task AwardPointsAsync(int memberId, string activityCode, int? relatedId = null, string? metadata = null, CancellationToken cancellationToken = default)
        {
            var config = await _db.GamificationConfigs
                .FirstOrDefaultAsync(c => c.ActivityCode == activityCode && c.IsActive, cancellationToken);

            if (config == null || config.Points == 0) return;

            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            if (member == null) return;

            member.ContributionPoints += config.Points;

            await _db.SaveChangesAsync(cancellationToken);

            await _activity.LogActivityAsync(
                memberId,
                "PointsAwarded",
                $"Earned {config.Points} pts for: {config.Name}",
                metadata: metadata,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Awarded {Points} points to Member {MemberId} for {Activity}", config.Points, memberId, activityCode);
        }

        public async Task<IEnumerable<GamificationConfig>> GetConfigsAsync(CancellationToken cancellationToken = default)
        {
            return await _db.GamificationConfigs.OrderBy(c => c.Name).ToListAsync(cancellationToken);
        }

        public async Task UpdateConfigAsync(int configId, int points, bool isActive, CancellationToken cancellationToken = default)
        {
            var config = await _db.GamificationConfigs.FindAsync(new object[] { configId }, cancellationToken);
            if (config != null)
            {
                config.Points = points;
                config.IsActive = isActive;
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<MemberContributionDto>> GetLeaderboardAsync(int top = 20, CancellationToken cancellationToken = default)
        {
            var topMembers = await _db.Members
                .Where(m => m.ContributionPoints > 0 && !m.IsArchived)
                .OrderByDescending(m => m.ContributionPoints)
                .Take(top)
                .ToListAsync(cancellationToken);

            return topMembers.Select((m, index) => new MemberContributionDto
            {
                MemberId = m.Id,
                FullName = m.FullName,
                MembershipNumber = m.MembershipNumber,
                PhotoPath = m.PhotoPath,
                ContributionPoints = m.ContributionPoints,
                Rank = index + 1
            });
        }
    }
}
