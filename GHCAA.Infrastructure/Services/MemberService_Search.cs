using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    // Registry list/search, the admin dashboard counters, and the admin-facing bulk record
    // update — the read/write surface admins use to browse and edit the whole membership.
    public partial class MemberService
    {
        public async Task<object> GetDashboardStatsAsync(bool isPrivileged, CancellationToken cancellationToken = default)
        {
            var totalMembers = await _db.Members.CountAsync(m => !m.IsArchived, cancellationToken);
            var applied = await _db.Members.CountAsync(m => m.Status == Enums.MembershipStatus.Applied && !m.IsArchived, cancellationToken);
            var active = await _db.Members.CountAsync(m => m.Status == Enums.MembershipStatus.Active && !m.IsArchived, cancellationToken);
            var inactive = await _db.Members.CountAsync(m => (m.Status == Enums.MembershipStatus.InactivePayment || m.Status == Enums.MembershipStatus.InactiveResigned) && !m.IsArchived, cancellationToken);

            decimal? balance = null;
            if (isPrivileged)
            {
                // Two separate income sources feed the org's actual funds: the general ledger
                // (FinancialRecords — donations/grants/manually-recorded income and expenses) and
                // member payment collection (PaymentHistories — registration/membership/event
                // fees taken through the payment flow). Summing only FinancialRecords understates
                // the real balance whenever it's sparse/empty and PaymentHistories carries the
                // actual transaction volume, which is the normal case for this org.
                var ledgerIncome = await _db.FinancialRecords
                    .Where(r => r.RecordType == Enums.FinancialRecordType.Income)
                    .SumAsync(r => r.Amount, cancellationToken);

                var ledgerExpense = await _db.FinancialRecords
                    .Where(r => r.RecordType == Enums.FinancialRecordType.Expense)
                    .SumAsync(r => r.Amount, cancellationToken);

                // 82.32: PaymentHistoryConfiguration's query filter also hides a payment whose
                // Member is archived, on top of hiding soft-deleted rows. FinancialRecords carries
                // no such filter, so archiving a member silently shrank the org-wide balance by
                // everything they had already paid — money already received is not undone by the
                // payer being archived later. IgnoreQueryFilters() restores that half; !IsDeleted is
                // reapplied by hand since that half of the filter is still correct here.
                var memberPayments = await _db.PaymentHistories
                    .IgnoreQueryFilters()
                    .Where(p => p.Status == Enums.PaymentStatus.Completed && !p.IsDeleted)
                    .SumAsync(p => p.Amount, cancellationToken);

                balance = ledgerIncome + memberPayments - ledgerExpense;
            }

            return new
            {
                TotalMembers = totalMembers,
                Applied = applied,
                Active = active,
                Inactive = inactive,
                Balance = balance,
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task<object> GetAllMembersAsync(int page = 1, int pageSize = 10, string searchQuery = "", string statusFilter = "all", string categoryFilter = "all", string membershipTypeFilter = "all", bool includeArchived = false, bool isPrivileged = false, CancellationToken cancellationToken = default)
        {
            IQueryable<Member> query = _db.Members
                .AsNoTracking()
                .AsSplitQuery() // Prevent duplicates from collection includes
                .Include(m => m.ECMembers)
                .ThenInclude(em => em.ECPeriod)
                .Include(m => m.AcademicHistory)
                .Include(m => m.ProfessionalHistory);

            if (includeArchived)
            {
                query = query.IgnoreQueryFilters();
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var q = searchQuery.ToLower();
                // Special case for M-ID search used in refresh
                if (q.StartsWith("m-") && int.TryParse(q.Substring(2), out var mid))
                {
                    query = query.Where(m => m.Id == mid);
                }
                else
                {
                    var terms = q.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var term in terms)
                    {
                        query = query.Where(m =>
                            (m.FullName != null && m.FullName.ToLower().Contains(term)) ||
                            (m.Email != null && m.Email.ToLower().Contains(term)) ||
                            (m.MembershipNumber != null && m.MembershipNumber.ToLower().Contains(term)) ||
                            (m.MobileNo != null && m.MobileNo.ToLower().Contains(term)) ||
                            m.AcademicHistory.Any(a => a.InstitutionName.ToLower().Contains(term) || a.Degree.ToLower().Contains(term) || a.Subject.ToLower().Contains(term)) ||
                            m.ProfessionalHistory.Any(p => p.OrganizationName.ToLower().Contains(term) || p.Designation.ToLower().Contains(term)));
                    }
                }
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "all")
            {
                if (statusFilter == "Applied")
                {
                    query = query.Where(m => m.Status == Enums.MembershipStatus.Applied);
                }
                else if (Enum.TryParse<Enums.MembershipStatus>(statusFilter, true, out var statusEnum))
                {
                    query = query.Where(m => m.Status == statusEnum);
                }
                else if (int.TryParse(statusFilter, out var statusInt))
                {
                    var casted = (Enums.MembershipStatus)statusInt;
                    query = query.Where(m => m.Status == casted);
                }
            }

            // Apply category filter
            if (!string.IsNullOrWhiteSpace(categoryFilter) && categoryFilter != "all")
            {
                if (Enum.TryParse<Enums.MemberCategory>(categoryFilter, true, out var catEnum))
                {
                    query = query.Where(m => m.Category == catEnum);
                }
                else if (int.TryParse(categoryFilter, out var catInt))
                {
                    var casted = (Enums.MemberCategory)catInt;
                    query = query.Where(m => m.Category == casted);
                }
            }

            // Apply membership type filter
            if (!string.IsNullOrWhiteSpace(membershipTypeFilter) && membershipTypeFilter != "all")
            {
                if (Enum.TryParse<Enums.MembershipType>(membershipTypeFilter, true, out var typeEnum))
                {
                    query = query.Where(m => m.MembershipType == typeEnum);
                }
                else if (int.TryParse(membershipTypeFilter, out var typeInt))
                {
                    var casted = (Enums.MembershipType)typeInt;
                    query = query.Where(m => m.MembershipType == casted);
                }
            }

            // Get total count
            var totalItems = await query.CountAsync(cancellationToken);

            // Apply paging
            var membersQuery = query.OrderByDescending(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var members = await membersQuery.ToListAsync(cancellationToken);
            var memberDtos = new List<MemberSummaryDto>();

            foreach (var member in members)
            {
                var gains = await GetMemberGainsAsync(member.Id, member.ContributionPoints, cancellationToken);
                var dto = new MemberSummaryDto
                {
                    Id = member.Id,
                    FullName = member.FullName,
                    Status = member.Status,
                    AppliedDate = member.AppliedDate.ToLocalTime(),
                    MembershipNumber = member.MembershipNumber,
                    PhotoPath = member.PhotoPath,
                    BloodGroup = member.BloodGroup,
                    MembershipType = member.MembershipType,
                    Category = member.Category,
                    IsArchived = member.IsArchived,
                    IsEmailPublic = member.IsEmailPublic,
                    IsMobilePublic = member.IsMobilePublic,
                    IsAddressPublic = member.IsAddressPublic,
                    IsNIDPublic = member.IsNIDPublic,

                    // Gamification
                    ContributionPoints = member.ContributionPoints,
                    Rank = gains.rank,
                    CategoryBadge = gains.badge,

                    // Mask PII if not privileged (SuperAdmin or self) AND not public
                    Email = (isPrivileged || member.IsEmailPublic) ? (member.Email ?? "") : (MaskPii(member.Email, 3, 3) ?? ""),
                    MobileNo = (isPrivileged || member.IsMobilePublic) ? (member.MobileNo ?? "") : (MaskPii(member.MobileNo, 4, 3) ?? ""),
                    NID = (isPrivileged || member.IsNIDPublic) ? (member.NID ?? "") : (MaskPii(member.NID, 3, 2) ?? "")
                };

                if (member.ECMembers != null && member.ECMembers.Any())
                {
                    dto.ECHistory = member.ECMembers.Select(em => new ECHistoryDto
                    {
                        Id = em.Id,
                        PeriodId = em.ECPeriodId,
                        PeriodTitle = em.ECPeriod?.Title ?? "Unknown",
                        Position = em.Position,
                        StartDate = em.StartDate.ToLocalTime(),
                        EndDate = em.EndDate.HasValue ? em.EndDate.Value.ToLocalTime() : null,
                        ChangeReason = em.ChangeReason,
                        IsCurrent = em.ECPeriod?.IsActive ?? false
                    }).OrderByDescending(h => h.StartDate).ToList();
                }

                // Enhanced Summary Data from normalized tables
                var ghcRecord = member.AcademicHistory?.FirstOrDefault(a => a.IsGHC);
                if (ghcRecord != null)
                {
                    dto.GHCLastCertificatePassingYear = ghcRecord.PassingYear;
                    dto.GHCLastCertificate = ghcRecord.Degree;
                    dto.GHCLastCertificateSubject = ghcRecord.Subject;
                }

                var currentJob = member.ProfessionalHistory?.FirstOrDefault(p => p.IsCurrent);
                if (currentJob != null)
                {
                    dto.Designation = currentJob.Designation;
                    dto.OrganizationName = currentJob.OrganizationName;
                    dto.ProfessionalSector = currentJob.Sector;
                    dto.Location = currentJob.Location;
                }

                memberDtos.Add(dto);
            }

            return new
            {
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Items = memberDtos
            };
        }

        public async Task<bool> AdminUpdateMemberAsync(int id, AdminMemberUpdateDto dto, int adminId, bool isPrivilegedCaller, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members
                .Include(m => m.AcademicHistory)
                .Include(m => m.ProfessionalHistory)
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
            if (member == null) return false;

            // A plain Admin must never be able to rewrite a SuperAdmin's linked email — that email
            // is where SendAdminPasswordResetLinkAsync's reset link goes, so together the two would
            // let a lower-privileged Admin take over a SuperAdmin account.
            if (!isPrivilegedCaller)
            {
                var targetIsSuperAdmin = await _db.Users
                    .Where(u => u.MemberId == id)
                    .SelectMany(u => u.Roles)
                    .AnyAsync(r => r.Name == "SuperAdmin", cancellationToken);
                if (targetIsSuperAdmin)
                    throw new UnauthorizedAccessException("Only a SuperAdmin may edit a SuperAdmin's own member record.");
            }

            // Track membership type change
            if (member.MembershipType != dto.MembershipType)
            {
                await _financialService.RecordMembershipChangeAsync(
                    id,
                    member.MembershipType.ToString(),
                    dto.MembershipType.ToString(),
                    adminId,
                    dto.MembershipChangeReason ?? "Administrative Update",
                    cancellationToken);
            }

            member.FullName = dto.FullName;
            member.FatherName = dto.FatherName;
            member.MotherName = dto.MotherName;
            member.DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth, DateTimeKind.Utc);
            member.NID = dto.NID;
            member.MobileNo = dto.MobileNo;
            member.Email = dto.Email;
            member.PresentAddress = dto.PresentAddress;
            member.PermanentAddress = dto.PermanentAddress;

            member.EmergencyContactName = dto.EmergencyContactName;
            member.EmergencyContactRelation = dto.EmergencyContactRelation;
            member.EmergencyContactPhone = dto.EmergencyContactPhone;
            if (!string.IsNullOrWhiteSpace(dto.TShirtSize)) member.TShirtSize = dto.TShirtSize;

            member.NotifyEventCreation = dto.NotifyEventCreation;
            member.NotifyParticipationApproval = dto.NotifyParticipationApproval;
            member.NotifyRegistrationUpdate = dto.NotifyRegistrationUpdate;
            member.NotifyRelevantUpdates = dto.NotifyRelevantUpdates;
            member.NotifyCommitteeChanges = dto.NotifyCommitteeChanges;

            member.Gender = dto.Gender;
            member.BloodGroup = dto.BloodGroup;
            member.MembershipNumber = dto.MembershipNumber;
            if (!string.IsNullOrWhiteSpace(dto.PhotoPath)) member.PhotoPath = dto.PhotoPath;
            if (!string.IsNullOrWhiteSpace(dto.SignaturePath)) member.SignaturePath = dto.SignaturePath;

            member.MembershipType = dto.MembershipType;
            member.Status = dto.Status;
            // Domain Validation: Only Founding members can be Lifelong Patrons
            if (dto.Category == Enums.MemberCategory.LifelongPatron && member.MembershipType != Enums.MembershipType.Founding)
            {
                _logger.LogWarning("Admin update attempted to assign Lifelong Patron to a non-founding member {Id}.", id);
                throw new InvalidOperationException("Only Founding members can be assigned as Lifelong Patrons.");
            }

            member.Category = dto.Category;
            member.MembershipChangeReason = dto.MembershipChangeReason;
            member.ECChangeReason = dto.ECChangeReason;
            member.IsVerified = dto.IsVerified;
            member.ContributionPoints = dto.ContributionPoints;
            member.LastUpdateDate = DateTime.UtcNow;

            // Summary fields sync (for directory/search performance)
            if (!string.IsNullOrWhiteSpace(dto.CertificatePath)) member.CertificatePath = dto.CertificatePath;
            if (!string.IsNullOrWhiteSpace(dto.PaymentProofPath)) member.PaymentProofPath = dto.PaymentProofPath;

            // Sync Academic History
            if (dto.AcademicHistory != null)
            {
                // Validation matching RegisterAsync
                if (dto.AcademicHistory.Any() && !dto.AcademicHistory.Any(a => a.IsGHC || a.InstitutionName.Contains("Haraganga", StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException("At least one academic record must be from Govt. Haraganga College.");
                }

                member.AcademicHistory.Clear();
                foreach (var a in dto.AcademicHistory)
                {
                    member.AcademicHistory.Add(new AcademicRecord
                    {
                        InstitutionName = a.InstitutionName,
                        Degree = a.Degree,
                        Subject = a.Subject,
                        AdmissionYear = a.AdmissionYear,
                        PassingYear = a.PassingYear ?? 0,
                        IsGHC = a.IsGHC || a.InstitutionName.Contains("Haraganga", StringComparison.OrdinalIgnoreCase),
                        Result = a.Result
                    });
                }
            }

            // Sync Professional History
            if (dto.ProfessionalHistory != null)
            {
                member.ProfessionalHistory.Clear();
                foreach (var p in dto.ProfessionalHistory)
                {
                    member.ProfessionalHistory.Add(new ProfessionalRecord
                    {
                        OrganizationName = p.OrganizationName,
                        Designation = p.Designation,
                        Sector = p.Sector,
                        Location = p.Location,
                        StartDate = DateTime.SpecifyKind(p.StartDate, DateTimeKind.Utc),
                        EndDate = p.EndDate.HasValue ? DateTime.SpecifyKind(p.EndDate.Value, DateTimeKind.Utc) : null,
                        IsCurrent = p.IsCurrent
                    });
                }
            }

            // Sync EC History
            if (dto.ECHistory != null)
            {
                var existingEC = await _db.ECMembers.Where(ec => ec.MemberId == id).ToListAsync(cancellationToken);
                _db.ECMembers.RemoveRange(existingEC);

                foreach (var ec in dto.ECHistory)
                {
                    var period = await _db.ECPeriods.FirstOrDefaultAsync(p => p.Title == ec.PeriodTitle, cancellationToken);
                    if (period == null) continue;

                    _db.ECMembers.Add(new ECMember
                    {
                        MemberId = id,
                        ECPeriodId = period.Id,
                        Position = ec.Position,
                        StartDate = DateTime.SpecifyKind(ec.StartDate, DateTimeKind.Utc),
                        EndDate = ec.EndDate.HasValue ? DateTime.SpecifyKind(ec.EndDate.Value, DateTimeKind.Utc) : null,
                        ChangeReason = ec.ChangeReason
                    });
                }
            }

            member.IsMobilePublic = dto.IsMobilePublic;
            member.IsNIDPublic = dto.IsNIDPublic;

            member.IsEmailPublic = dto.IsEmailPublic;
            member.IsAddressPublic = dto.IsAddressPublic;
            member.IsVerified = dto.IsVerified;
            member.ContributionPoints = dto.ContributionPoints;
            member.LastUpdateDate = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            // Rotate SecurityStamp if status transitions to Terminated or Resigned — invalidates all JWT sessions
            if (dto.Status == Enums.MembershipStatus.Terminated || dto.Status == Enums.MembershipStatus.InactiveResigned)
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.MemberId == id, cancellationToken);
                if (user != null)
                {
                    user.SecurityStamp = Guid.NewGuid().ToString("N");
                    await _db.SaveChangesAsync(cancellationToken);
                    await _tokenService.RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
                    _logger.LogWarning("SecurityStamp rotated for member {MemberId} — all existing sessions terminated.", id);
                }
            }

            _logger.LogInformation("Member {MemberId} information updated by Admin", id);
            return true;
        }

        public async Task<object> GetPublicStatsAsync(CancellationToken cancellationToken = default)
        {
            var activeMembers = await _db.Members.CountAsync(m => m.Status == Enums.MembershipStatus.Active && !m.IsArchived, cancellationToken);
            var ecMembers = await _db.ECMembers.Where(em => em.ECPeriod != null && em.ECPeriod.IsActive && em.EndDate == null).CountAsync(cancellationToken);
            var totalEvents = await _db.AlumniEvents.CountAsync(e => e.Status == Enums.EventStatus.Published, cancellationToken);

            return new
            {
                TotalActiveMembers = activeMembers,
                CurrentECMembers = ecMembers,
                PublishedEvents = totalEvents,
                LastUpdated = DateTime.UtcNow
            };
        }
    }
}
