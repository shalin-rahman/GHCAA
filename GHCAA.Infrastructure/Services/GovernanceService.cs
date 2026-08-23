using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services
{
    public class GovernanceService : IGovernanceService
    {
        private readonly ApplicationDbContext _db;

        public GovernanceService(ApplicationDbContext db)
        {
            _db = db;
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

        public async Task<bool> AssignMemberToRoleAsync(int periodId, int memberId, int position, string? reason, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            if (member == null || member.Status != MembershipStatus.Active)
            {
                throw new InvalidOperationException("Only active members can be assigned to the Executive Committee.");
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
            return true;
        }

        public async Task<bool> RemoveMemberFromCommitteeAsync(int ecMemberId, CancellationToken cancellationToken = default)
        {
            var ecMember = await _db.ECMembers.FindAsync(new object[] { ecMemberId }, cancellationToken);
            if (ecMember == null) return false;

            ecMember.EndDate = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteECMemberAsync(int id, CancellationToken cancellationToken = default)
        {
            var ecMember = await _db.ECMembers.FindAsync(new object[] { id }, cancellationToken);
            if (ecMember == null) return false;

            _db.ECMembers.Remove(ecMember);
            await _db.SaveChangesAsync(cancellationToken);
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
            await _db.SaveChangesAsync(cancellationToken);
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
