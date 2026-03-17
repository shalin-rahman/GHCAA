using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task<IEnumerable<ECPeriod>> GetAllPeriodsAsync(CancellationToken cancellationToken = default)
        {
            return await _db.ECPeriods
                .OrderByDescending(p => p.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<ECPeriod?> GetPeriodByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.ECPeriods
                .Include(p => p.ECMembers)
                .ThenInclude(m => m.Member)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<ECPeriod> CreatePeriodAsync(string title, DateTime startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc) : (DateTime?)null;

            // Gap Check: No Overlaps
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
            return period;
        }

        public async Task<bool> UpdatePeriodAsync(int id, string title, DateTime startDate, DateTime? endDate, bool isActive, CancellationToken cancellationToken = default)
        {
            var period = await _db.ECPeriods.FindAsync(new object[] { id }, cancellationToken);
            if (period == null) return false;

            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc) : (DateTime?)null;

            // Gap Check: No Overlaps (excluding self)
            await EnsureNoOverlapAsync(id, utcStart, utcEnd, cancellationToken);

            period.Title = title;
            period.StartDate = utcStart;
            period.EndDate = utcEnd;

            if (isActive && !period.IsActive)
            {
                // Strict Rule: Active period must cover "Current Dates"
                var today = DateTime.UtcNow.Date;
                if (utcStart.Date > today || (utcEnd.HasValue && utcEnd.Value.Date < today))
                {
                    throw new InvalidOperationException("Only a period covering the current date can be activated.");
                }
                await ActivatePeriodInternalAsync(id, cancellationToken);
            }
            else if (isActive && period.IsActive)
            {
                // Already active? Re-sync to ensure everything is matched (Immediate Effect)
                await ActivatePeriodInternalAsync(id, cancellationToken);
            }
            else if (!isActive && period.IsActive)
            {
                period.IsActive = false;
                var membersWithRole = await _db.Members.Where(m => m.ECPosition != ECPosition.None).ToListAsync(cancellationToken);
                foreach (var m in membersWithRole) m.ECPosition = ECPosition.None;
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
            
            // Logic: Two periods overlap if (Start1 <= End2) AND (End1 >= Start2)
            // If end is null, we treat it as infinite future
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
            var period = await _db.ECPeriods
                .Include(p => p.ECMembers)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
                
            if (period == null) return false;

            // 1. Deactivate other periods
            var activePeriods = await _db.ECPeriods.Where(p => p.IsActive).ToListAsync(cancellationToken);
            foreach (var p in activePeriods) p.IsActive = false;

            // 2. Reset ALL members current EC positions on the main Member table
            // using ExecuteUpdate for performance if possible, or simple loop
            var membersWithRole = await _db.Members.Where(m => m.ECPosition != ECPosition.None).ToListAsync(cancellationToken);
            foreach (var m in membersWithRole) m.ECPosition = ECPosition.None;

            // 3. Set the new period as active
            period.IsActive = true;

            // 4. Sync positions from the new period into the Member table
            foreach (var em in period.ECMembers.Where(em => em.EndDate == null))
            {
                var member = await _db.Members.FindAsync(new object[] { em.MemberId }, cancellationToken);
                if (member != null)
                {
                    member.ECPosition = em.Position;
                }
            }

            return true;
        }

        public async Task<IEnumerable<ECMember>> GetCommitteeMembersAsync(int periodId, CancellationToken cancellationToken = default)
        {
            return await _db.ECMembers
                .Include(em => em.Member)
                .Where(em => em.ECPeriodId == periodId && em.EndDate == null)
                .OrderBy(em => em.Position)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> AssignMemberToRoleAsync(int periodId, int memberId, int position, string? reason, CancellationToken cancellationToken = default)
        {
            var period = await _db.ECPeriods.FindAsync(new object[] { periodId }, cancellationToken);
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);

            if (period == null || member == null) return false;

            // Business Gap: Only Active members can hold EC positions
            if (member.Status != MembershipStatus.Active)
            {
                throw new InvalidOperationException("Only active members can be assigned to the Executive Committee.");
            }

            // Business Gap: Unique positions check (for key roles)
            var keyRoles = new[] { ECPosition.President, ECPosition.GeneralSecretary, ECPosition.Treasurer };
            var targetPos = (ECPosition)position;

            if (keyRoles.Contains(targetPos))
            {
                var alreadyHas = await _db.ECMembers
                    .AnyAsync(em => em.ECPeriodId == periodId && em.Position == targetPos && em.EndDate == null && em.MemberId != memberId, cancellationToken);
                
                if (alreadyHas)
                {
                    throw new InvalidOperationException($"A {targetPos} is already assigned to this period.");
                }
            }

            // Remove existing active role for this member in THIS period (if any)
            var existing = await _db.ECMembers
                .Where(em => em.ECPeriodId == periodId && em.MemberId == memberId && em.EndDate == null)
                .FirstOrDefaultAsync(cancellationToken);
            
            if (existing != null)
            {
                throw new InvalidOperationException("This member already holds a role in this EC Period. Please remove the existing role first.");
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

            // If this is the active period, sync the member's current position
            if (period.IsActive)
            {
                member.ECPosition = (ECPosition)position;
            }

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> RemoveMemberFromCommitteeAsync(int ecMemberId, CancellationToken cancellationToken = default)
        {
            var ecMember = await _db.ECMembers
                .Include(em => em.Member)
                .Include(em => em.ECPeriod)
                .FirstOrDefaultAsync(em => em.Id == ecMemberId, cancellationToken);

            if (ecMember == null) return false;

            ecMember.EndDate = DateTime.UtcNow;

            // If active period, reset member's position
            if (ecMember.ECPeriod != null && ecMember.ECPeriod.IsActive && ecMember.Member != null)
            {
                ecMember.Member.ECPosition = ECPosition.None;
            }

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteECMemberAsync(int id, CancellationToken cancellationToken = default)
        {
            var ecMember = await _db.ECMembers
                .Include(em => em.Member)
                .Include(em => em.ECPeriod)
                .FirstOrDefaultAsync(em => em.Id == id, cancellationToken);

            if (ecMember == null) return false;

            // If it was an active position in the active period, reset member's position
            if (ecMember.ECPeriod != null && ecMember.ECPeriod.IsActive && ecMember.EndDate == null && ecMember.Member != null)
            {
                ecMember.Member.ECPosition = ECPosition.None;
            }

            _db.ECMembers.Remove(ecMember);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
