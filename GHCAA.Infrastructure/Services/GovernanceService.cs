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
    public class GovernanceService : IGovernanceService
    {
        private readonly ApplicationDbContext _db;
        private readonly INotificationService _notificationService;

        public GovernanceService(ApplicationDbContext db, INotificationService notificationService)
        {
            _db = db;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<ECPeriodDto>> GetAllPeriodsAsync(CancellationToken cancellationToken = default)
        {
            var periods = await _db.ECPeriods
                .OrderByDescending(p => p.StartDate)
                .ToListAsync(cancellationToken);

            return periods.Select(MapToPeriodDto);
        }

        public async Task<ECPeriodDto?> GetPeriodByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var period = await _db.ECPeriods
                .Include(p => p.ECMembers)
                .ThenInclude(m => m.Member)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            return period == null ? null : MapToPeriodDto(period);
        }

        public async Task<ECPeriodDto?> GetActivePeriodAsync(CancellationToken cancellationToken = default)
        {
            var period = await _db.ECPeriods
                .FirstOrDefaultAsync(p => p.IsActive, cancellationToken);

            return period == null ? null : MapToPeriodDto(period);
        }

        public async Task<ECPeriodDto> CreatePeriodAsync(string title, DateTime startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc) : (DateTime?)null;

            await EnsureNoOverlapAsync(null, utcStart, utcEnd, cancellationToken);

            var period = new ECPeriod
            {
                Title = title,
                StartDate = utcStart,
                EndDate = utcEnd,
                IsActive = false
            };

            _db.ECPeriods.Add(period);
            await _db.SaveChangesAsync(cancellationToken);
            return MapToPeriodDto(period);
        }

        public async Task<bool> UpdatePeriodAsync(int id, string title, DateTime startDate, DateTime? endDate, bool isActive, CancellationToken cancellationToken = default)
        {
            var period = await _db.ECPeriods.FindAsync(new object[] { id }, cancellationToken);
            if (period == null) return false;

            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc) : (DateTime?)null;

            await EnsureNoOverlapAsync(id, utcStart, utcEnd, cancellationToken);

            period.Title = title;
            period.StartDate = utcStart;
            period.EndDate = utcEnd;

            if (isActive && !period.IsActive)
            {
                var today = DateTime.UtcNow.Date;
                if (utcStart.Date > today || (utcEnd.HasValue && utcEnd.Value.Date < today))
                {
                    throw new InvalidOperationException("Only a period covering the current date can be activated.");
                }
                await ActivatePeriodInternalAsync(id, cancellationToken);
            }
            else if (!isActive && period.IsActive)
            {
                period.IsActive = false;
            }
            else
            {
                period.IsActive = isActive;
            }

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        private async Task EnsureNoOverlapAsync(int? excludeId, DateTime start, DateTime? end, CancellationToken ct)
        {
            var query = _db.ECPeriods.Where(p => (!excludeId.HasValue || p.Id != excludeId.Value));
            var overlapping = await query.AnyAsync(p =>
                (p.EndDate == null || p.EndDate >= start) && (end == null || p.StartDate <= end), ct);

            if (overlapping)
            {
                throw new InvalidOperationException("The specified dates overlap with an existing EC Period.");
            }
        }

        public async Task<bool> ActivatePeriodAsync(int id, CancellationToken cancellationToken = default)
        {
            var period = await _db.ECPeriods.FindAsync(new object[] { id }, cancellationToken);
            if (period == null) return false;

            var today = DateTime.UtcNow.Date;
            if (period.StartDate.Date > today || (period.EndDate.HasValue && period.EndDate.Value.Date < today))
            {
                throw new InvalidOperationException("Only a period covering the current date can be activated.");
            }

            var success = await ActivatePeriodInternalAsync(id, cancellationToken);
            if (success)
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            return success;
        }

        private async Task<bool> ActivatePeriodInternalAsync(int id, CancellationToken cancellationToken)
        {
            var activePeriods = await _db.ECPeriods.Where(p => p.IsActive).ToListAsync(cancellationToken);
            foreach (var p in activePeriods) p.IsActive = false;

            var period = await _db.ECPeriods.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (period != null) period.IsActive = true;

            return true;
        }

        public async Task<IEnumerable<ECMemberDto>> GetCommitteeMembersAsync(int periodId, CancellationToken cancellationToken = default)
        {
            var members = await _db.ECMembers
                .Include(em => em.Member)
                .Where(em => em.ECPeriodId == periodId && em.EndDate == null)
                .OrderBy(em => em.Position)
                .ToListAsync(cancellationToken);

            return members.Select(MapToMemberDto);
        }

        public async Task<IEnumerable<CommitteeSeatDto>> GetCommitteeSeatsAsync(int periodId, CancellationToken cancellationToken = default)
        {
            var rows = await _db.ECMembers
                .Include(em => em.Member)
                .Where(em => em.ECPeriodId == periodId)
                .ToListAsync(cancellationToken);

            var seats = new List<CommitteeSeatDto>();
            foreach (var position in Enum.GetValues<ECPosition>().Where(p => p != ECPosition.None))
            {
                var held = rows.Where(r => r.Position == position).ToList();
                var holder = held.FirstOrDefault(r => r.EndDate == null);
                if (holder != null)
                {
                    seats.Add(new CommitteeSeatDto { Position = position, Holder = MapToMemberDto(holder) });
                    continue;
                }

                var last = held.OrderByDescending(r => r.EndDate).FirstOrDefault();
                seats.Add(new CommitteeSeatDto
                {
                    Position = position,
                    VacancyReason = last?.EndReason,
                    VacancyNote = last?.EndNote,
                    VacatedOn = last?.EndDate,
                    LastHolderName = last?.Member?.FullName
                });
            }
            return seats;
        }

        public async Task<bool> AssignMemberToRoleAsync(int periodId, int memberId, int position, string? reason, bool notifyMember = false,
            VacancyReason? endCurrentHolderReason = null, string? endCurrentHolderNote = null, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            if (member == null || member.Status != MembershipStatus.Active)
            {
                throw new InvalidOperationException("Only active members can be assigned to the Executive Committee.");
            }

            // 95.3: one current holder per seat. Replacing someone has to say why their term ended.
            var currentHolders = await _db.ECMembers
                .Include(em => em.Member)
                .Where(em => em.ECPeriodId == periodId && em.Position == (ECPosition)position && em.EndDate == null)
                .ToListAsync(cancellationToken);
            if (currentHolders.Any(h => h.MemberId == memberId))
            {
                throw new InvalidOperationException("This member already holds that seat.");
            }
            if (currentHolders.Count > 0)
            {
                if (endCurrentHolderReason == null)
                {
                    var name = currentHolders[0].Member?.FullName ?? "another member";
                    throw new InvalidOperationException($"{(ECPosition)position} is held by {name}. End their term first, or replace them and give a reason.");
                }
                foreach (var holder in currentHolders)
                {
                    EndTerm(holder, endCurrentHolderReason.Value, endCurrentHolderNote);
                }
            }

            var newAssignment = new ECMember
            {
                ECPeriodId = periodId,
                MemberId = memberId,
                Position = (ECPosition)position,
                StartDate = DateTime.UtcNow,
                ChangeReason = reason
            };

            _db.ECMembers.Add(newAssignment);
            await _db.SaveChangesAsync(cancellationToken);

            // 82.52: admin-chosen, off by default (nothing notified here before this item).
            if (notifyMember)
            {
                await _notificationService.CreateNotificationAsync(
                    memberId,
                    "Committee Assignment",
                    $"You have been assigned to the Executive Committee as {(ECPosition)position}.",
                    NotificationType.CommitteeAssignment,
                    cancellationToken: cancellationToken);
            }

            return true;
        }

        public async Task<bool> RemoveMemberFromCommitteeAsync(int ecMemberId, VacancyReason reason, string? note, bool notifyMember = false, CancellationToken cancellationToken = default)
        {
            var ecMember = await _db.ECMembers.FindAsync(new object[] { ecMemberId }, cancellationToken);
            if (ecMember == null) return false;
            if (ecMember.EndDate != null)
            {
                throw new InvalidOperationException("This term has already ended.");
            }

            EndTerm(ecMember, reason, note);
            await _db.SaveChangesAsync(cancellationToken);

            // 82.52: admin-chosen, off by default (nothing notified here before this item).
            if (notifyMember)
            {
                await _notificationService.CreateNotificationAsync(
                    ecMember.MemberId,
                    "Committee Term Ended",
                    "Your term on the Executive Committee has ended.",
                    NotificationType.CommitteeAssignment,
                    cancellationToken: cancellationToken);
            }

            return true;
        }

        public async Task<bool> DeleteECMemberAsync(int id, int adminId, bool notifyMember = false, CancellationToken cancellationToken = default)
        {
            var ecMember = await _db.ECMembers.FindAsync(new object[] { id }, cancellationToken);
            if (ecMember == null || ecMember.IsArchived) return false;

            // 82.29: soft delete, replacing _db.ECMembers.Remove(ecMember). ECMember is Class A
            // (ARCHITECTURE.md §4) — this path is for a row that should never have existed (wrong
            // member added), which is still worth keeping as evidence of who removed it and when,
            // the same reasoning FinancialService.DeletePaymentAsync already applies to PaymentHistory.
            ecMember.IsArchived = true;
            ecMember.DeletedAt = DateTime.UtcNow;
            ecMember.DeletedByAdminId = adminId;

            await _db.SaveChangesAsync(cancellationToken);

            // 82.52: admin-chosen, off by default. This is the "wrong member added" correction
            // path, so notifying is rarely wanted, but an admin who does want to explain the
            // removal to the affected member can opt in per action.
            if (notifyMember)
            {
                await _notificationService.CreateNotificationAsync(
                    ecMember.MemberId,
                    "Committee Record Removed",
                    "A committee role record for you has been removed by an administrator.",
                    NotificationType.CommitteeAssignment,
                    cancellationToken: cancellationToken);
            }

            return true;
        }

        public async Task<Constitution?> GetActiveConstitutionAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Constitutions
                .OrderByDescending(c => c.EffectiveDate)
                .FirstOrDefaultAsync(c => c.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<Constitution>> GetConstitutionHistoryAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Constitutions
                .OrderByDescending(c => c.EffectiveDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> CreateConstitutionVersionAsync(string version, string content, string changeSummary, CancellationToken cancellationToken = default)
        {
            var constitution = new Constitution
            {
                Version = version,
                Content = content,
                ChangeSummary = changeSummary,
                EffectiveDate = DateTime.UtcNow,
                IsActive = false
            };

            _db.Constitutions.Add(constitution);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> ActivateConstitutionAsync(int id, CancellationToken cancellationToken = default)
        {
            var target = await _db.Constitutions.FindAsync(new object[] { id }, cancellationToken);
            if (target == null) return false;

            var activeList = await _db.Constitutions.Where(c => c.IsActive).ToListAsync(cancellationToken);
            foreach (var c in activeList)
            {
                c.IsActive = false;
                c.SupersededDate = DateTime.UtcNow;
            }

            target.IsActive = true;
            target.SupersededDate = null;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> VoteOnConstitutionAsync(int constitutionId, int memberId, bool isFor, string? comments, CancellationToken cancellationToken = default)
        {
            var constitution = await _db.Constitutions.FindAsync(new object[] { constitutionId }, cancellationToken);
            if (constitution == null || !constitution.IsActive) return false;

            // Article III Section K: only Founding, Executive and General members are Voting Members.
            // Associate, Honorary and Advisory members may read and comment but not ratify amendments.
            var isVotingMember = await _db.Members.AnyAsync(m => m.Id == memberId &&
                (m.MembershipType == MembershipType.Founding ||
                 m.MembershipType == MembershipType.Executive ||
                 m.MembershipType == MembershipType.General), cancellationToken);
            if (!isVotingMember) return false;

            var alreadyVoted = await _db.AmendmentVotes.AnyAsync(v => v.ConstitutionId == constitutionId && v.MemberId == memberId, cancellationToken);
            if (alreadyVoted) return false;

            var vote = new AmendmentVote
            {
                ConstitutionId = constitutionId,
                MemberId = memberId,
                IsFor = isFor,
                Comments = comments,
                VotedAt = DateTime.UtcNow
            };

            _db.AmendmentVotes.Add(vote);
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // A concurrent request for the same member won the unique index on
                // (ConstitutionId, MemberId). Report it the same way as the alreadyVoted check.
                return false;
            }
            return true;
        }

        // Mappings
        private static ECPeriodDto MapToPeriodDto(ECPeriod p) => new()
        {
            Id = p.Id,
            Title = p.Title,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            IsActive = p.IsActive,
            ECMembers = p.ECMembers?.Select(MapToMemberDto).ToList() ?? new List<ECMemberDto>()
        };

        private static void EndTerm(ECMember ecMember, VacancyReason reason, string? note)
        {
            note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            if (reason == VacancyReason.Other && note == null)
            {
                throw new InvalidOperationException("Say what happened when the reason is Other.");
            }
            if (note != null && note.Length > Constants.Governance.VacancyNoteMaxLength)
            {
                throw new InvalidOperationException($"The note can be at most {Constants.Governance.VacancyNoteMaxLength} characters.");
            }

            ecMember.EndDate = DateTime.UtcNow;
            ecMember.EndReason = reason;
            ecMember.EndNote = note;
        }

        private static ECMemberDto MapToMemberDto(ECMember m) => new()
        {
            Id = m.Id,
            MemberId = m.MemberId,
            Position = m.Position,
            StartDate = m.StartDate,
            EndDate = m.EndDate,
            Member = m.Member == null ? null : new MemberSummaryDto
            {
                Id = m.Member.Id,
                FullName = m.Member.FullName,
                MembershipNumber = m.Member.MembershipNumber,
                PhotoPath = m.Member.PhotoPath,
                Status = m.Member.Status,
                IsVerified = m.Member.IsVerified
            }
        };
    }
}
