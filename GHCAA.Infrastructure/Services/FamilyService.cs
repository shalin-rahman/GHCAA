using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services
{
    public class FamilyService : IFamilyService
    {
        private readonly ApplicationDbContext _db;
        private readonly INotificationService _notification;
        private readonly IRealTimeService _realTime;

        public FamilyService(ApplicationDbContext db, INotificationService notification, IRealTimeService realTime)
        {
            _db = db;
            _notification = notification;
            _realTime = realTime;
        }

        public async Task<bool> SendRequestAsync(int requesterId, CreateFamilyRequestDto dto, CancellationToken cancellationToken = default)
        {
            if (requesterId == dto.TargetMemberId) return false;

            // Check if request already exists (either way) that is NOT rejected/cancelled
            var existing = await _db.FamilyLinkRequests.AnyAsync(r =>
                ((r.RequesterId == requesterId && r.TargetMemberId == dto.TargetMemberId) ||
                 (r.RequesterId == dto.TargetMemberId && r.TargetMemberId == requesterId)) &&
                r.Status != FamilyLinkStatus.Rejected && r.Status != FamilyLinkStatus.Cancelled,
                cancellationToken);

            if (existing) return false;

            var request = new FamilyLinkRequest
            {
                RequesterId = requesterId,
                TargetMemberId = dto.TargetMemberId,
                Relationship = dto.Relationship,
                Status = FamilyLinkStatus.Requested,
                Note = dto.Note,
                RequestedAt = DateTime.UtcNow
            };

            await _db.FamilyLinkRequests.AddAsync(request, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // Notify Target Member
            var requester = await _db.Members.FindAsync(new object[] { requesterId }, cancellationToken);
            await _notification.CreateNotificationAsync(
                dto.TargetMemberId,
                "Family Link Request",
                $"{requester?.FullName ?? "An alumnus"} has requested to link with you as {dto.Relationship}.",
                NotificationType.GeneralSystem,
                "/portal/family",
                cancellationToken);

            await _realTime.SendNotificationToUserAsync(dto.TargetMemberId, new { Type = "NEW_FAMILY_REQUEST", RequestId = request.Id, Requester = requester?.FullName });

            return true;
        }

        public async Task<bool> RespondAsync(int targetMemberId, int requestId, FamilyLinkStatus status, CancellationToken cancellationToken = default)
        {
            var request = await _db.FamilyLinkRequests
                .Include(r => r.Requester)
                .Include(r => r.TargetMember)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.TargetMemberId == targetMemberId, cancellationToken);

            if (request == null || request.Status != FamilyLinkStatus.Requested) return false;

            request.Status = status;
            request.RespondedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            // Notify Requester
            var responder = request.TargetMember?.FullName ?? "An alumnus";
            var outcome = status == FamilyLinkStatus.Accepted ? "accepted" : "declined";

            await _notification.CreateNotificationAsync(
                request.RequesterId,
                "Family Link Update",
                $"{responder} has {outcome} your family link request.",
                NotificationType.GeneralSystem,
                "/portal/family",
                cancellationToken);

            await _realTime.SendNotificationToUserAsync(request.RequesterId, new { Type = "FAMILY_REQUEST_RESPONDED", RequestId = requestId, Status = status.ToString() });

            return true;
        }

        public async Task<bool> CancelRequestAsync(int requesterId, int requestId, CancellationToken cancellationToken = default)
        {
            var request = await _db.FamilyLinkRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.RequesterId == requesterId, cancellationToken);
            if (request == null || request.Status != FamilyLinkStatus.Requested) return false;

            request.Status = FamilyLinkStatus.Cancelled;
            request.RespondedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IEnumerable<FamilyRequestDto>> GetRequestsAsync(int memberId, bool receivedOnly = false, CancellationToken cancellationToken = default)
        {
            IQueryable<FamilyLinkRequest> query = _db.FamilyLinkRequests
                .Include(r => r.Requester)
                .Include(r => r.TargetMember);

            if (receivedOnly)
            {
                query = query.Where(r => r.TargetMemberId == memberId);
            }
            else
            {
                query = query.Where(r => (r.RequesterId == memberId || r.TargetMemberId == memberId));
            }

            var requests = await query.OrderByDescending(r => r.RequestedAt).ToListAsync(cancellationToken);

            return requests.Select(r => new FamilyRequestDto
            {
                Id = r.Id,
                RequesterId = r.RequesterId,
                RequesterName = r.Requester?.FullName ?? "Unknown Alumnus",
                TargetMemberId = r.TargetMemberId,
                TargetMemberName = r.TargetMember?.FullName ?? "Unknown Alumnus",
                Relationship = r.Relationship,
                Status = r.Status,
                RequestedAt = r.RequestedAt,
                Note = r.Note,
                IsReceived = r.TargetMemberId == memberId
            });
        }

        public async Task<IEnumerable<MemberSummaryDto>> GetLinkedMembersAsync(int memberId, CancellationToken cancellationToken = default)
        {
            // A member is linked if they are either Requester OR Target and Status is Accepted
            var links = await _db.FamilyLinkRequests
                .Include(r => r.Requester)
                .Include(r => r.TargetMember)
                .Where(r => (r.RequesterId == memberId || r.TargetMemberId == memberId) && r.Status == FamilyLinkStatus.Accepted)
                .ToListAsync(cancellationToken);

            var linkedMembers = new List<MemberSummaryDto>();

            foreach (var link in links)
            {
                var otherMember = link.RequesterId == memberId ? link.TargetMember : link.Requester;
                if (otherMember != null)
                {
                    linkedMembers.Add(new MemberSummaryDto
                    {
                        Id = otherMember.Id,
                        FullName = otherMember.FullName,
                        MembershipNumber = otherMember.MembershipNumber,
                        PhotoPath = otherMember.PhotoPath,
                        Category = otherMember.Category,
                        MembershipType = otherMember.MembershipType,
                        IsVerified = otherMember.IsVerified,
                        Designation = $"Link: {link.Relationship}",
                        FatherName = otherMember.FatherName ?? "",
                        MotherName = otherMember.MotherName ?? "",
                        DateOfBirth = otherMember.DateOfBirth,
                        NID = otherMember.NID ?? "",
                        MobileNo = otherMember.MobileNo ?? "",
                        Email = otherMember.Email ?? "",
                        PresentAddress = otherMember.PresentAddress ?? "",
                        PermanentAddress = otherMember.PermanentAddress ?? "",
                        EmergencyContactName = otherMember.EmergencyContactName ?? "",
                        EmergencyContactRelation = otherMember.EmergencyContactRelation ?? "",
                        EmergencyContactPhone = otherMember.EmergencyContactPhone ?? ""
                    });
                }
            }

            return linkedMembers;
        }

        public async Task<bool> UnlinkAsync(int memberId, int linkedMemberId, CancellationToken cancellationToken = default)
        {
            var link = await _db.FamilyLinkRequests.FirstOrDefaultAsync(r =>
               ((r.RequesterId == memberId && r.TargetMemberId == linkedMemberId) ||
                (r.RequesterId == linkedMemberId && r.TargetMemberId == memberId)) &&
               r.Status == FamilyLinkStatus.Accepted,
               cancellationToken);

            if (link == null) return false;

            // Soft "unlink" by setting status to Cancelled or just deleting?
            // Existing logic for status-based audit suggests moving to "Cancelled".
            link.Status = FamilyLinkStatus.Cancelled;
            link.RespondedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IEnumerable<MemberSummaryDto>> SearchByNameAsync(string name, int excludeMemberId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name)) return Array.Empty<MemberSummaryDto>();

            var members = await _db.Members
                .Where(m => m.Id != excludeMemberId &&
                            m.IsFamilyPublic &&
                            m.Status == MembershipStatus.Active &&
                            m.FullName.Contains(name))
                .Take(20)
                .ToListAsync(cancellationToken);

            return members.Select(m => new MemberSummaryDto
            {
                Id = m.Id,
                FullName = m.FullName,
                MembershipNumber = m.MembershipNumber,
                PhotoPath = m.PhotoPath,
                Category = m.Category,
                MembershipType = m.MembershipType,
                IsVerified = m.IsVerified
            });
        }
    }
}
