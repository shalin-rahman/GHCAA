using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Http;

namespace GHCAA.Infrastructure.Services
{
    public class ActivityService : IActivityService
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ActivityService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActivityAsync(int? memberId, string type, string description, int? actorId = null, string? ipAddress = null, string? userAgent = null, string? source = null, string? metadata = null, CancellationToken cancellationToken = default)
        {
            var ctx = _httpContextAccessor.HttpContext;
            var ip = ipAddress ?? ctx?.Connection?.RemoteIpAddress?.ToString();
            var ua = userAgent ?? ctx?.Request?.Headers["User-Agent"].ToString();

            // Auto-detect source if not provided
            var src = source ?? (ua?.ToLower().Contains("mobile") == true ? "Mobile" : "Web");

            var log = new ActivityLog
            {
                MemberId = memberId,
                ActorId = actorId,
                ActivityType = type,
                Description = description,
                IPAddress = ip,
                UserAgent = ua,
                Source = src,
                Metadata = metadata,
                Timestamp = DateTime.UtcNow
            };

            await _db.ActivityLogs.AddAsync(log, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<ActivityLog>> GetMemberActivityAsync(int memberId, int count = 20, CancellationToken cancellationToken = default)
        {
            return await _db.ActivityLogs
                .Where(a => a.MemberId == memberId)
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ActivityLog>> GetRecentGlobalActivityAsync(int count = 50, CancellationToken cancellationToken = default)
        {
            return await _db.ActivityLogs
                .Include(a => a.Member)
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
    }
}
