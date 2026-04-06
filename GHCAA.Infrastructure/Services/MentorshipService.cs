using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    public class MentorshipService : IMentorshipService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<MentorshipService> _logger;

        public MentorshipService(ApplicationDbContext db, ILogger<MentorshipService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<MentorshipRequest> SendRequestAsync(int requesterId, int mentorId, string? message, string? domain, CancellationToken ct = default)
        {
            // Prevent duplicate pending requests
            var existing = await _db.MentorshipRequests
                .FirstOrDefaultAsync(r => r.RequesterId == requesterId && r.MentorId == mentorId
                    && r.Status == MentorshipStatus.Pending, ct);

            if (existing != null)
                throw new InvalidOperationException("A pending mentorship request already exists for this mentor.");

            var request = new MentorshipRequest
            {
                RequesterId = requesterId,
                MentorId = mentorId,
                Message = message,
                Domain = domain,
                Status = MentorshipStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            _db.MentorshipRequests.Add(request);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Mentorship request {Id} created: Member {R} → Member {M}", request.Id, requesterId, mentorId);
            return request;
        }

        public async Task<IEnumerable<object>> GetSentRequestsAsync(int requesterId, CancellationToken ct = default)
        {
            return await _db.MentorshipRequests
                .Where(r => r.RequesterId == requesterId)
                .Include(r => r.Mentor)
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new
                {
                    r.Id, r.Domain, r.Message, r.Status, r.RequestedAt, r.RespondedAt, r.ResponseNote,
                    Mentor = new { r.Mentor!.Id, r.Mentor.FullName, r.Mentor.PhotoPath, r.Mentor.MembershipNumber }
                })
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<object>> GetReceivedRequestsAsync(int mentorId, CancellationToken ct = default)
        {
            return await _db.MentorshipRequests
                .Where(r => r.MentorId == mentorId)
                .Include(r => r.Requester)
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new
                {
                    r.Id, r.Domain, r.Message, r.Status, r.RequestedAt, r.RespondedAt, r.ResponseNote,
                    Requester = new { r.Requester!.Id, r.Requester.FullName, r.Requester.PhotoPath, r.Requester.MembershipNumber }
                })
                .ToListAsync(ct);
        }

        public async Task<bool> RespondAsync(int requestId, int mentorId, bool accept, string? note, CancellationToken ct = default)
        {
            var request = await _db.MentorshipRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.MentorId == mentorId, ct);

            if (request == null) return false;

            request.Status = accept ? MentorshipStatus.Accepted : MentorshipStatus.Declined;
            request.ResponseNote = note;
            request.RespondedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> MarkCompleteAsync(int requestId, int memberId, CancellationToken ct = default)
        {
            var request = await _db.MentorshipRequests
                .FirstOrDefaultAsync(r => r.Id == requestId
                    && (r.RequesterId == memberId || r.MentorId == memberId)
                    && r.Status == MentorshipStatus.Accepted, ct);

            if (request == null) return false;

            request.Status = MentorshipStatus.Completed;
            request.RespondedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<IEnumerable<object>> GetAllForAdminAsync(CancellationToken ct = default)
        {
            return await _db.MentorshipRequests
                .Include(r => r.Requester)
                .Include(r => r.Mentor)
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new
                {
                    r.Id, r.Domain, r.Message, r.Status, r.RequestedAt,
                    Requester = r.Requester == null ? null : new { r.Requester.Id, r.Requester.FullName },
                    Mentor = r.Mentor == null ? null : new { r.Mentor.Id, r.Mentor.FullName }
                })
                .ToListAsync(ct);
        }
    }
}
