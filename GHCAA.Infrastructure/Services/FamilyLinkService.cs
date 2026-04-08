using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services
{
    public class FamilyLinkService : IFamilyLinkService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<FamilyLinkService> _logger;
        private readonly INotificationService _notifications;
        private readonly ICommunicationService _communication;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;

        public FamilyLinkService(ApplicationDbContext db, ILogger<FamilyLinkService> logger, INotificationService notifications, ICommunicationService communication, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _db = db;
            _logger = logger;
            _notifications = notifications;
            _communication = communication;
            _config = config;
        }

        public async Task<FamilyLinkRequestDto> SendRequestAsync(int requesterId, SendFamilyLinkDto dto, CancellationToken ct = default)
        {
            var target = await _db.Members.FirstOrDefaultAsync(m => m.MembershipNumber == dto.TargetMembershipNumber, ct)
                ?? throw new KeyNotFoundException($"No active member found with membership number '{dto.TargetMembershipNumber}'.");

            if (target.Id == requesterId)
                throw new InvalidOperationException("You cannot send a family link request to yourself.");

            // Prevent duplicates
            bool alreadyExists = await _db.FamilyLinkRequests.AnyAsync(r =>
                r.RequesterId == requesterId && r.TargetMemberId == target.Id &&
                (r.Status == FamilyLinkStatus.Requested || r.Status == FamilyLinkStatus.Accepted), ct);

            if (alreadyExists)
                throw new InvalidOperationException("A link or request already exists for this member.");

            var request = new FamilyLinkRequest
            {
                RequesterId = requesterId,
                TargetMemberId = target.Id,
                Relationship = dto.Relationship,
                Note = dto.Note,
                Status = FamilyLinkStatus.Requested,
                RequestedAt = DateTime.UtcNow
            };

            _db.FamilyLinkRequests.Add(request);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Member {RequesterId} sent a family link request to member {TargetId}", requesterId, target.Id);

            // Fetch requester name
            var requester = await _db.Members.FindAsync(new object[] { requesterId }, ct);

            // Notify the target member via In-App
            await _notifications.CreateNotificationAsync(
                target.Id,
                "Family Link Request",
                $"A member has sent you a family link request ({dto.Relationship}). Please review it in your profile.",
                Enums.NotificationType.GeneralSystem,
                cancellationToken: ct);

            // Notify via Email
            try
            {
                await _communication.SendIndividualEmailAsync(target.Id, "FAMILY_LINK_REQUEST", new Dictionary<string, string>
                {
                    { "RequesterName", requester?.FullName ?? "A member" },
                    { "Relationship", dto.Relationship.ToString() },
                    { "ProfileUrl", $"{_config["GeneralSettings:PortalBaseUrl"]}/profile" }
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send family link email to {MemberId}", target.Id);
            }

            return MapToDto(request, requester, target);
        }

        public async Task<bool> RespondAsync(int respondingMemberId, RespondFamilyLinkDto dto, CancellationToken ct = default)
        {
            var request = await _db.FamilyLinkRequests
                .Include(r => r.Requester)
                .Include(r => r.TargetMember)
                .FirstOrDefaultAsync(r => r.Id == dto.RequestId && r.TargetMemberId == respondingMemberId, ct);

            if (request == null) return false;
            if (request.Status != FamilyLinkStatus.Requested) return false;

            request.Status = dto.Approve ? FamilyLinkStatus.Accepted : FamilyLinkStatus.Rejected;
            request.RespondedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            // Notify requester of result
            var message = dto.Approve
                ? "Your family link request has been approved."
                : "Your family link request was declined.";
            await _notifications.CreateNotificationAsync(request.RequesterId, "Family Link Update", message, Enums.NotificationType.GeneralSystem, cancellationToken: ct);

            if (dto.Approve)
            {
                try
                {
                    await _communication.SendIndividualEmailAsync(request.RequesterId, "FAMILY_LINK_ACCEPTED", new Dictionary<string, string>
                    {
                        { "TargetName", request.TargetMember?.FullName ?? "The member" }
                    }, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send family link acceptance email to {MemberId}", request.RequesterId);
                }
            }

            return true;
        }

        public async Task<bool> CancelAsync(int requesterId, int requestId, CancellationToken ct = default)
        {
            var request = await _db.FamilyLinkRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.RequesterId == requesterId, ct);

            if (request == null || request.Status != FamilyLinkStatus.Requested) return false;

            request.Status = FamilyLinkStatus.Cancelled;
            request.RespondedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<List<FamilyLinkRequestDto>> GetSentRequestsAsync(int memberId, CancellationToken ct = default)
        {
            var requests = await _db.FamilyLinkRequests
                .Include(r => r.Requester)
                .Include(r => r.TargetMember)
                .Where(r => r.RequesterId == memberId)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync(ct);

            return requests.Select(r => MapToDto(r, r.Requester, r.TargetMember)).ToList();
        }

        public async Task<List<FamilyLinkRequestDto>> GetReceivedRequestsAsync(int memberId, CancellationToken ct = default)
        {
            var requests = await _db.FamilyLinkRequests
                .Include(r => r.Requester)
                .Include(r => r.TargetMember)
                .Where(r => r.TargetMemberId == memberId && r.Status == FamilyLinkStatus.Requested)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync(ct);

            return requests.Select(r => MapToDto(r, r.Requester, r.TargetMember)).ToList();
        }

        public async Task<List<FamilyLinkRequestDto>> GetFamilyAsync(int memberId, CancellationToken ct = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, ct);
            if (member == null) return new List<FamilyLinkRequestDto>();

            var requests = await _db.FamilyLinkRequests
                .Include(r => r.Requester)
                .Include(r => r.TargetMember)
                .Where(r => (r.RequesterId == memberId || r.TargetMemberId == memberId)
                            && r.Status == FamilyLinkStatus.Accepted)
                .ToListAsync(ct);

            // Filter based on privacy settings: only show if the other person is public or we are the admin?
            // Actually, if we are calling this for the CURRENT member's own network, we show all.
            // If calling for ANOTHER member (public view), we need to check IsFamilyPublic.
            // I'll add a 'publicView' flag to IFamilyLinkService if needed, but for now assuming this is for own view.
            
            return requests.Select(r => MapToDto(r, r.Requester, r.TargetMember)).ToList();
        }

        public async Task<bool> RemoveLinkAsync(int currentMemberId, int requestId, CancellationToken ct = default)
        {
            var request = await _db.FamilyLinkRequests
                .Include(r => r.Requester)
                .Include(r => r.TargetMember)
                .FirstOrDefaultAsync(r => r.Id == requestId, ct);

            if (request == null) return false;
            if (request.RequesterId != currentMemberId && request.TargetMemberId != currentMemberId) return false;

            // Mark as cancelled or removed
            request.Status = FamilyLinkStatus.Cancelled;
            request.RespondedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Family link {RequestId} removed by member {MemberId}", requestId, currentMemberId);
            return true;
        }

        private static FamilyLinkRequestDto MapToDto(FamilyLinkRequest r, Member? requester, Member? target) => new()
        {
            Id = r.Id,
            RequesterId = r.RequesterId,
            RequesterName = requester?.FullName ?? "Unknown",
            RequesterMembershipNumber = requester?.MembershipNumber,
            RequesterPhotoPath = requester?.PhotoPath,
            TargetMemberId = r.TargetMemberId,
            TargetMemberName = target?.FullName ?? "Unknown",
            TargetMembershipNumber = target?.MembershipNumber,
            TargetMemberPhotoPath = target?.PhotoPath,
            Relationship = r.Relationship,
            Status = r.Status,
            Note = r.Note,
            RequestedAt = r.RequestedAt,
            RespondedAt = r.RespondedAt
        };
    }
}
