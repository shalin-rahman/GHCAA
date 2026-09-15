using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    // Approval workflow and the archive/restore lifecycle that follows it: approve/reject a
    // pending application, and the later archive/restore/reactivate/bulk-archive transitions
    // that act on an already-decided member.
    public partial class MemberService
    {
        public async Task<ApproveMemberResultDto> ApproveMemberAsync(int memberId, int approvedByAdminId, CancellationToken cancellationToken = default)
        {
            Member? member;
            string membershipNumber;
            string cleanNid = string.Empty;

            // Use a transaction to prevent race conditions during membership Serial generation
            using (var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken))
            {
                try
                {
                    // Find member with AcademicHistory
                    member = await _db.Members
                        .Include(m => m.AcademicHistory)
                        .Include(m => m.ProfessionalHistory)
                        .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
                    if (member == null)
                    {
                        _logger.LogWarning("Approval failed: Member {MemberId} not found", memberId);
                        throw new KeyNotFoundException($"Member with ID {memberId} not found");
                    }

                    // Validate status
                    if (member.Status != Enums.MembershipStatus.Applied)
                    {
                        _logger.LogWarning("Approval failed: Member {MemberId} has status {Status}, expected Applied", memberId, member.Status);
                        throw new InvalidOperationException($"Member must have 'Applied' status to be approved. Current status: {member.Status}");
                    }

                    if (CalculateProfileCompletion(member) < 100)
                    {
                        throw new InvalidOperationException("Member profile must be 100% complete before approval.");
                    }

                    var isPaid = await _db.PaymentHistories.AnyAsync(p =>
                        p.MemberId == memberId &&
                        (p.FinancialCategory == Enums.FinancialCategory.RegistrationFee || p.FinancialCategory == Enums.FinancialCategory.MembershipFee) &&
                        p.Status == Enums.PaymentStatus.Completed,
                        cancellationToken);

                    if (!isPaid)
                    {
                        throw new InvalidOperationException("Member must complete the initial payment before approval.");
                    }

                    // Generate membership number: GHCYYMMXXX
                    var now = DateTime.UtcNow;
                    var prefix = $"GHC{now:yyMM}";

                    // 24.29: Order by Id to avoid lexicographic rollover bug.
                    var lastBound = await _db.Members
                        .Where(m => m.MembershipNumber != null && m.MembershipNumber.StartsWith(prefix))
                        .OrderByDescending(m => m.Id)
                        .FirstOrDefaultAsync(cancellationToken);

                    int nextId = 1;
                    if (lastBound?.MembershipNumber != null && lastBound.MembershipNumber.Length > prefix.Length)
                    {
                        var lastPart = lastBound.MembershipNumber[prefix.Length..];
                        if (int.TryParse(lastPart, out int lastId))
                            nextId = lastId + 1;
                    }

                    membershipNumber = member.MembershipNumber ?? $"{prefix}{nextId:D3}";

                    // Update member
                    member.Status = Enums.MembershipStatus.Active;
                    member.IsVerified = true; // Mark as verified upon admin approval
                    member.MembershipNumber = membershipNumber;
                    member.ApprovedDate = DateTime.UtcNow;
                    member.ApprovedBy = approvedByAdminId;

                    await _db.SaveChangesAsync(cancellationToken);

                    // Award points for verification
                    await _gamification.AwardPointsAsync(memberId, "PROFILE_VERIFIED", metadata: "Initial approval", cancellationToken: cancellationToken);

                    cleanNid = member.NID.Replace(" ", "");
                    var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.MemberId == memberId, cancellationToken);
                    if (existingUser == null)
                    {
                        // Create the first account inside the transaction so Member(Active) and
                        // User are always atomic.
                        await _userService.CreateUserAccountAsync(memberId, cleanNid, cleanNid, cancellationToken);
                    }
                    else
                    {
                        existingUser.IsActive = true;
                        existingUser.SecurityStamp = Guid.NewGuid().ToString("N");
                        await _tokenService.RevokeAllRefreshTokensAsync(existingUser.Id, cancellationToken);
                        await _db.SaveChangesAsync(cancellationToken);
                    }

                    await transaction.CommitAsync(cancellationToken);

                    await _activityService.LogActivityAsync(memberId, "Approved", $"Member approved by Admin {approvedByAdminId}. Membership Number: {membershipNumber}", approvedByAdminId, cancellationToken: cancellationToken);

                    _logger.LogInformation("Member {MemberId} approved by Admin {AdminId}. Membership Number: {MembershipNumber}",
                        memberId, approvedByAdminId, membershipNumber);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }

            var defaultPassword = cleanNid;

            // Send Welcome Email
            try
            {
                var customVars = new Dictionary<string, string> { { "DefaultPassword", defaultPassword } };
                await _activityService.LogActivityAsync(memberId, "EmailSent", "Welcome email with credentials sent to member.", approvedByAdminId, cancellationToken: cancellationToken);
                await _communicationService.SendIndividualEmailAsync(memberId, "WELCOME_EMAIL", customVars, cancellationToken);

                // Add System Notification
                await _notificationService.CreateNotificationAsync(memberId, "Welcome to GHCAA!", "Your membership has been approved. You can now access the full portal.", Enums.NotificationType.RegistrationUpdate, "/portal/dashboard", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to member {MemberId}", memberId);
                // We don't throw here as the approval and account creation were successful
            }

            return new ApproveMemberResultDto
            {
                MembershipNumber = membershipNumber,
                DefaultPassword = defaultPassword
            };
        }

        public async Task<bool> RevertMemberApprovalAsync(int memberId, int adminId, CancellationToken cancellationToken = default)
        {
            // Same isolation as ApproveMemberAsync: the member/user rows and the refresh-token
            // revocation must land together, or a failed SaveChangesAsync leaves tokens revoked
            // with the member still showing Active.
            using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            try
            {
                var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
                if (member == null) return false;
                if (member.Status != Enums.MembershipStatus.Active)
                    throw new InvalidOperationException("Only active members can have approval reverted.");

                member.Status = Enums.MembershipStatus.Applied;
                member.IsVerified = false;
                member.ApprovedDate = null;
                member.ApprovedBy = null;

                var user = await _db.Users.FirstOrDefaultAsync(u => u.MemberId == memberId, cancellationToken);
                if (user != null)
                {
                    user.IsActive = false;
                    user.SecurityStamp = Guid.NewGuid().ToString("N");
                    await _tokenService.RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
                }

                await _db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

            // Outside the transaction and its catch block: a failure here must not trigger
            // RollbackAsync on a transaction that's already committed.
            await NotifyMemberOfApprovalRevertAsync(memberId, adminId, cancellationToken);
            return true;
        }

        private async Task NotifyMemberOfApprovalRevertAsync(int memberId, int adminId, CancellationToken cancellationToken)
        {
            try
            {
                await _notificationService.CreateNotificationAsync(memberId,
                    "Membership approval reverted",
                    "An administrator has reverted your membership approval. Your application has been moved back to pending review.",
                    Enums.NotificationType.RegistrationUpdate, "/portal/dashboard", cancellationToken);

                await _activityService.LogActivityAsync(memberId, "ApprovalReverted",
                    $"Member approval reverted by Admin {adminId}.", adminId, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify member {MemberId} of approval revert", memberId);
                // Notification failure doesn't undo an already-committed revert.
            }
        }

        public async Task<bool> RejectMemberAsync(int id, int adminId, string reason, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { id }, cancellationToken);
            if (member == null) return false;

            if (member.Status != Enums.MembershipStatus.Applied)
                throw new InvalidOperationException("Only pending registrations can be rejected.");

            // Send rejection email before deleting the record
            try
            {
                var customVars = new Dictionary<string, string> { { "Reason", reason } };
                // Using SendEmailByCodeAsync with the template ensures professional styling from DB settings
                await _communicationService.SendEmailByCodeAsync(member.Email, "APPLICATION_REJECTED", customVars, member, cancellationToken);

                await _activityService.LogActivityAsync(id, "EmailSent", "Application rejection email sent.", adminId, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send rejection email to {Email}", member.Email);
            }

            // 24.30: Soft-delete preserves audit trail and prevents FK cascade failures.
            member.Status = Enums.MembershipStatus.Rejected;
            member.IsArchived = true;
            await _db.SaveChangesAsync(cancellationToken);
            await _activityService.LogActivityAsync(id, "Rejected", $"Application rejected by Admin {adminId}. Reason: {reason}", adminId, cancellationToken: cancellationToken);
            _logger.LogWarning("Admin {AdminId} rejected application {MemberId} for: {Reason}", adminId, id, reason);
            return true;
        }

        public async Task<bool> ArchiveMemberAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            if (member == null) return false;

            member.IsArchived = true;

            // Also archive the associated user if exists — rotate stamp to invalidate all sessions
            var user = await _db.Users.FirstOrDefaultAsync(u => u.MemberId == memberId, cancellationToken);
            if (user != null)
            {
                user.IsArchived = true;
                user.SecurityStamp = Guid.NewGuid().ToString("N"); // Invalidate all active JWTs
                // Rotating the stamp alone doesn't stop /api/auth/refresh from minting a fresh
                // access token carrying the new stamp — the refresh token itself must be revoked.
                await _tokenService.RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
            }

            // End active EC roles
            var activeRoles = await _db.ECMembers
                .Where(em => em.MemberId == memberId && em.EndDate == null)
                .ToListAsync(cancellationToken);

            foreach (var role in activeRoles)
            {
                role.EndDate = DateTime.UtcNow;
                role.ChangeReason = "Member archived";
            }

            // Cascade: Archive Job Opportunities
            await _db.JobOpportunities
                .Where(j => j.PostedByMemberId == memberId && j.IsActive)
                .ExecuteUpdateAsync(s => s.SetProperty(j => j.IsActive, false), cancellationToken);

            // Cascade: Cancel Family Link Requests (both sent and received)
            await _db.FamilyLinkRequests
                .Where(r => (r.RequesterId == memberId || r.TargetMemberId == memberId) && r.Status == Enums.FamilyLinkStatus.Requested)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, Enums.FamilyLinkStatus.Cancelled), cancellationToken);

            // Cascade: Unpublish news (AuthorId in NewsPost maps to User, who maps to Member)
            if (user != null)
            {
                await _db.NewsPosts
                    .Where(n => n.AuthorId == user.Id && n.IsActive)
                    .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsActive, false), cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);
            await _activityService.LogActivityAsync(memberId, "Archived", "Member archived (soft deleted).", cancellationToken: cancellationToken);
            _logger.LogInformation("Member {MemberId} archived (soft delete)", memberId);
            return true;
        }

        public async Task<bool> RestoreMemberAsync(int memberId, CancellationToken cancellationToken = default)
        {
            // Must use IgnoreQueryFilters to see archived records
            var member = await _db.Members.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            if (member == null) return false;

            member.IsArchived = false;

            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.MemberId == memberId, cancellationToken);
            if (user != null)
            {
                user.IsArchived = false;
            }

            await _db.SaveChangesAsync(cancellationToken);
            await _activityService.LogActivityAsync(memberId, "Restored", "Member record restored from archives.", cancellationToken: cancellationToken);
            _logger.LogInformation("Member {MemberId} restored", memberId);
            return true;
        }

        public async Task<bool> ReactivateMemberAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            if (member == null) return false;

            member.Status = Enums.MembershipStatus.Active;
            await _db.SaveChangesAsync(cancellationToken);
            await _activityService.LogActivityAsync(memberId, "Reactivated", "Member reactivated to Active status.", cancellationToken: cancellationToken);
            _logger.LogInformation("Member {MemberId} reactivated to Active status", memberId);
            return true;
        }

        public async Task<int> BulkArchiveInactiveMembersAsync(CancellationToken cancellationToken = default)
        {
            var inactiveStatuses = new[] {
                Enums.MembershipStatus.InactivePayment,
                Enums.MembershipStatus.InactiveResigned,
                Enums.MembershipStatus.Terminated
            };

            // 24.32: Replaced per-row ArchiveMemberAsync loop (N+1) with bulk ExecuteUpdateAsync calls.
            var threshold = DateTime.UtcNow.AddMonths(-6);

            var targetMemberIds = await _db.Members
                .Where(m => !m.IsArchived && inactiveStatuses.Contains(m.Status) && m.LastUpdateDate < threshold)
                .Select(m => m.Id)
                .ToListAsync(cancellationToken);

            if (targetMemberIds.Count == 0)
                return 0;

            // Bulk archive member rows
            await _db.Members
                .Where(m => targetMemberIds.Contains(m.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsArchived, true), cancellationToken);

            // Bulk archive user rows and rotate SecurityStamp to invalidate all active JWTs.
            // SecurityStamp must be unique per user so we load and update in a single SaveChanges.
            var usersToArchive = await _db.Users
                .Where(u => u.MemberId != null && targetMemberIds.Contains(u.MemberId.Value))
                .ToListAsync(cancellationToken);

            foreach (var user in usersToArchive)
            {
                user.IsArchived = true;
                user.SecurityStamp = Guid.NewGuid().ToString("N");
            }

            if (usersToArchive.Count > 0)
            {
                await _db.SaveChangesAsync(cancellationToken);
                foreach (var user in usersToArchive)
                    await _tokenService.RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
            }

            // Bulk end active EC roles
            await _db.ECMembers
                .Where(em => targetMemberIds.Contains(em.MemberId) && em.EndDate == null)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(em => em.EndDate, DateTime.UtcNow)
                    .SetProperty(em => em.ChangeReason, "Bulk archived"), cancellationToken);

            // Bulk cancel pending family link requests
            await _db.FamilyLinkRequests
                .Where(r => (targetMemberIds.Contains(r.RequesterId) || targetMemberIds.Contains(r.TargetMemberId))
                         && r.Status == Enums.FamilyLinkStatus.Requested)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, Enums.FamilyLinkStatus.Cancelled), cancellationToken);

            // Bulk deactivate job opportunities
            await _db.JobOpportunities
                .Where(j => targetMemberIds.Contains(j.PostedByMemberId) && j.IsActive)
                .ExecuteUpdateAsync(s => s.SetProperty(j => j.IsActive, false), cancellationToken);

            // Bulk unpublish news posts authored by archived users
            var archivedUserIds = usersToArchive.Select(u => u.Id).ToList();
            if (archivedUserIds.Count > 0)
            {
                await _db.NewsPosts
                    .Where(n => archivedUserIds.Contains(n.AuthorId) && n.IsActive)
                    .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsActive, false), cancellationToken);
            }

            _logger.LogInformation("Bulk-archived {Count} inactive members.", targetMemberIds.Count);
            return targetMemberIds.Count;
        }
    }
}
