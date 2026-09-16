using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    public class NetworkingService : INetworkingService
    {
        private readonly ApplicationDbContext _db;

        public NetworkingService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<MemberProfileDto?> GetMemberProfileAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members
                .AsNoTracking()
                .Include(m => m.ECMembers)
                .ThenInclude(em => em.ECPeriod)
                .Include(m => m.SentFamilyLinkRequests)
                    .ThenInclude(r => r.TargetMember)
                .Include(m => m.ReceivedFamilyLinkRequests)
                    .ThenInclude(r => r.Requester)
                .Include(m => m.AcademicHistory)
                .Include(m => m.ProfessionalHistory)
                .Where(m => m.Id == memberId && m.Status == Enums.MembershipStatus.Active && !m.IsArchived)
                .FirstOrDefaultAsync(cancellationToken);

            return member == null ? null : MapToDto(member);
        }

        public async Task<PagedResult<MemberSummaryDto>> SearchMembersAsync(MemberSearchFilterDto filter, CancellationToken cancellationToken = default)
        {
            var query = _db.Members
                .AsNoTracking()
                .Where(m => m.Status == Enums.MembershipStatus.Active && !m.IsArchived)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Query))
            {
                var q = filter.Query.ToLower();
                // This endpoint is [AllowAnonymous] (public directory search). Matching on Email
                // regardless of IsEmailPublic turns a non-empty result into an oracle: an attacker
                // submits a guessed address and confirms it belongs to a real member even though
                // MapToSummary correctly masks that same address as "Confidential" in the response.
                query = query.Where(m =>
                    m.FullName.ToLower().Contains(q) ||
                    (m.MembershipNumber != null && m.MembershipNumber.ToLower().Contains(q)) ||
                    (m.IsEmailPublic && m.Email != null && m.Email.ToLower().Contains(q)));
            }

            if (filter.PassingYear.HasValue)
                query = query.Where(m => m.AcademicHistory.Any(a => a.IsGHC && a.PassingYear == filter.PassingYear.Value));

            if (!string.IsNullOrEmpty(filter.BloodGroup))
                query = query.Where(m => m.BloodGroup.ToString() == filter.BloodGroup);

            if (!string.IsNullOrEmpty(filter.ProfessionalSector))
                query = query.Where(m => m.ProfessionalHistory.Any(p => p.IsCurrent && p.Sector == filter.ProfessionalSector));

            if (!string.IsNullOrEmpty(filter.Designation))
                query = query.Where(m => m.ProfessionalHistory.Any(p => p.IsCurrent && p.Designation.Contains(filter.Designation)));

            if (!string.IsNullOrEmpty(filter.ECPosition) && System.Enum.TryParse<Enums.ECPosition>(filter.ECPosition, true, out var pos))
                query = query.Where(m => m.ECMembers.Any(em => em.Position == pos && em.EndDate == null));

            if (!string.IsNullOrEmpty(filter.Category) && System.Enum.TryParse<Enums.MemberCategory>(filter.Category, true, out var cat))
                query = query.Where(m => m.Category == cat);

            if (!string.IsNullOrEmpty(filter.MembershipType) && System.Enum.TryParse<Enums.MembershipType>(filter.MembershipType, true, out var type))
                query = query.Where(m => m.MembershipType == type);

            var totalItems = await query.CountAsync(cancellationToken);
            var pageSize = Math.Clamp(filter.PageSize, 1, 100);
            var page = Math.Max(filter.Page, 1);
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var sortedQuery = query
                .Include(m => m.ECMembers)
                .ThenInclude(em => em.ECPeriod)
                .Include(m => m.AcademicHistory)
                .Include(m => m.ProfessionalHistory)
                .OrderBy(m => m.FullName)
                .ThenBy(m => m.Id);

            // Cursor-based (keyset) pagination: the sort key is (FullName, Id), so a cursor is
            // "everything after this name+id pair". This replaces OFFSET/Skip, whose cost grows
            // with page depth because the DB still has to walk and discard every earlier row.
            // A missing or corrupt Cursor value (stale client, tampered query param) falls back
            // to the first page instead of erroring — a dead cursor shouldn't 400 the screen.
            var cursorRequested = !string.IsNullOrWhiteSpace(filter.Cursor);
            IQueryable<Member> pageQuery = sortedQuery;
            if (cursorRequested && TryDecodeCursor(filter.Cursor, out var cursorName, out var cursorId))
            {
                pageQuery = pageQuery.Where(m =>
                    m.FullName.CompareTo(cursorName) > 0 ||
                    (m.FullName == cursorName && m.Id > cursorId));
            }
            else if (!cursorRequested)
            {
                // No cursor supplied: honor legacy page-number pagination for callers that
                // haven't moved to cursors yet (e.g. the professional hub screen).
                pageQuery = pageQuery.Skip((page - 1) * pageSize);
            }

            // Fetch one extra row so HasNextPage/NextCursor reflect whether more data actually
            // exists, rather than being inferred from a possibly-stale TotalItems count.
            var fetched = await pageQuery.Take(pageSize + 1).ToListAsync(cancellationToken);
            var hasMore = fetched.Count > pageSize;
            var members = hasMore ? fetched.Take(pageSize).ToList() : fetched;
            var nextCursor = hasMore ? EncodeCursor(members[^1].FullName, members[^1].Id) : null;

            return new PagedResult<MemberSummaryDto>
            {
                Items = members.Select(MapToSummary),
                TotalItems = totalItems,
                TotalPages = totalPages,
                Page = page,
                PageSize = pageSize,
                NextCursor = nextCursor
            };
        }

        private static string EncodeCursor(string fullName, int id)
        {
            var json = JsonSerializer.Serialize(new CursorPayload(fullName, id));
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        private static bool TryDecodeCursor(string? cursor, out string fullName, out int id)
        {
            fullName = string.Empty;
            id = 0;
            if (string.IsNullOrWhiteSpace(cursor)) return false;

            try
            {
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
                var payload = JsonSerializer.Deserialize<CursorPayload>(json);
                if (payload == null || string.IsNullOrEmpty(payload.FullName)) return false;

                fullName = payload.FullName;
                id = payload.Id;
                return true;
            }
            catch (Exception ex) when (ex is FormatException or JsonException)
            {
                return false;
            }
        }

        private sealed record CursorPayload(string FullName, int Id);

        public async Task<IEnumerable<MemberSummaryDto>> GetExecutiveCommitteeAsync(int? periodId = null, CancellationToken cancellationToken = default)
        {
            // If no period specified, get current active one
            var query = _db.ECMembers
                .AsNoTracking()
                .Include(em => em.Member)
                    .ThenInclude(m => m!.AcademicHistory)
                .Include(em => em.ECPeriod)
                .AsQueryable();

            if (periodId.HasValue)
            {
                query = query.Where(em => em.ECPeriodId == periodId.Value);
            }
            else
            {
                query = query.Where(em => em.ECPeriod!.IsActive);
            }

            // Only current active roles in that period
            query = query.Where(em => em.EndDate == null);

            var results = await query.OrderBy(em => em.Position).ToListAsync(cancellationToken);
            return results.Select(em => MapToSummary(em.Member!));
        }

        public async Task<IEnumerable<object>> GetECPeriodsAsync(CancellationToken cancellationToken = default)
        {
            return await _db.ECPeriods
                .OrderByDescending(p => p.StartDate)
                .Select(p => new { p.Id, p.Title, p.IsActive, p.StartDate, p.EndDate })
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MemberSummaryDto>> GetLatestAlumniUpdatesAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            var members = await _db.Members
                .AsNoTracking()
                .Include(m => m.ECMembers)
                .ThenInclude(em => em.ECPeriod)
                .Where(m => m.Status == Enums.MembershipStatus.Active && !m.IsArchived)
                .OrderByDescending(m => m.LastUpdateDate)
                .Take(count)
                .ToListAsync(cancellationToken);

            return members.Select(MapToSummary);
        }

        private MemberProfileDto MapToDto(Member m)
        {
            var dto = new MemberProfileDto
            {
                Id = m.Id,
                FullName = m.FullName,
                Email = m.IsEmailPublic ? m.Email : "Confidential",
                MobileNo = m.IsMobilePublic ? m.MobileNo : "Confidential",
                MembershipNumber = m.MembershipNumber,
                Status = m.Status,

                // Personal — this DTO backs the [AllowAnonymous] public directory profile
                // (NetworkingController.GetPublicProfile), so identity-verification fields
                // (NID, DOB, parents' names, emergency contact) and certificate file paths
                // must never appear here regardless of any privacy flag. The authenticated
                // owner/admin view of a member's full profile is a separate DTO build in
                // MemberService.GetProfileAsync, not this one.
                AppliedDate = m.AppliedDate.ToLocalTime(),
                ApprovedDate = m.ApprovedDate.HasValue ? m.ApprovedDate.Value.ToLocalTime() : null,
                Gender = m.Gender,
                BloodGroup = m.BloodGroup,

                // History
                AcademicHistory = m.AcademicHistory.Select(a => new AcademicRecordDto
                {
                    Id = a.Id,
                    InstitutionName = a.InstitutionName,
                    Degree = a.Degree,
                    Subject = a.Subject,
                    AdmissionYear = a.AdmissionYear,
                    PassingYear = a.PassingYear,
                    IsGHC = a.IsGHC,
                    Result = a.Result
                }).ToList(),
                ProfessionalHistory = m.ProfessionalHistory.Select(p => new ProfessionalRecordDto
                {
                    Id = p.Id,
                    OrganizationName = p.OrganizationName,
                    Designation = p.Designation,
                    Sector = p.Sector,
                    Location = p.Location,
                    StartDate = p.StartDate.ToLocalTime(),
                    EndDate = p.EndDate.HasValue ? p.EndDate.Value.ToLocalTime() : null,
                    IsCurrent = p.IsCurrent
                }).ToList(),

                // Info & Privacy
                PhotoPath = m.PhotoPath,
                PresentAddress = m.IsAddressPublic ? m.PresentAddress : "Confidential",
                PermanentAddress = m.IsAddressPublic ? m.PermanentAddress : "Confidential",
                IsVerified = m.IsVerified,
                MembershipType = m.MembershipType,
                Category = m.Category,
                IsFamilyPublic = m.IsFamilyPublic,
                ECHistory = new List<ECHistoryDto>(),
                FamilyMembers = new List<MemberFamilyDto>(),

                // Summary Data for easier display
                CategoryBadge = m.Category.ToString(),
                GHCLastCertificatePassingYear = m.AcademicHistory?.FirstOrDefault(a => a.IsGHC)?.PassingYear,
                GHCLastCertificate = m.AcademicHistory?.FirstOrDefault(a => a.IsGHC)?.Degree,
                GHCLastCertificateSubject = m.AcademicHistory?.FirstOrDefault(a => a.IsGHC)?.Subject,
                Designation = m.ProfessionalHistory?.FirstOrDefault(p => p.IsCurrent)?.Designation,
                OrganizationName = m.ProfessionalHistory?.FirstOrDefault(p => p.IsCurrent)?.OrganizationName,
                ProfessionalSector = m.ProfessionalHistory?.FirstOrDefault(p => p.IsCurrent)?.Sector,
                Location = m.ProfessionalHistory?.FirstOrDefault(p => p.IsCurrent)?.Location
            };

            // Populate Family links from both sent and received requests
            if (m.IsFamilyPublic)
            {
                // From sent requests
                if (m.SentFamilyLinkRequests != null)
                {
                    foreach (var r in m.SentFamilyLinkRequests)
                    {
                        if (r.TargetMember != null && r.TargetMember.IsFamilyPublic)
                        {
                            dto.FamilyMembers.Add(new MemberFamilyDto
                            {
                                RequestId = r.Id,
                                MemberId = r.TargetMemberId,
                                FullName = r.TargetMember.FullName,
                                MembershipNumber = r.TargetMember.MembershipNumber,
                                PhotoPath = r.TargetMember.PhotoPath,
                                IsVerified = r.TargetMember.IsVerified,
                                Relationship = r.Relationship,
                                Status = r.Status,
                                IsRequester = true
                            });
                        }
                    }
                }

                // From received requests
                if (m.ReceivedFamilyLinkRequests != null)
                {
                    foreach (var r in m.ReceivedFamilyLinkRequests)
                    {
                        if (r.Requester != null && r.Requester.IsFamilyPublic)
                        {
                            dto.FamilyMembers.Add(new MemberFamilyDto
                            {
                                RequestId = r.Id,
                                MemberId = r.RequesterId,
                                FullName = r.Requester.FullName,
                                MembershipNumber = r.Requester.MembershipNumber,
                                PhotoPath = r.Requester.PhotoPath,
                                IsVerified = r.Requester.IsVerified,
                                Relationship = r.Relationship,
                                Status = r.Status,
                                IsRequester = false
                            });
                        }
                    }
                }
            }

            if (m.ECMembers != null && m.ECMembers.Any())
            {
                dto.ECHistory = m.ECMembers.Select(em => new ECHistoryDto
                {
                    Id = em.Id,
                    PeriodId = em.ECPeriodId,
                    PeriodTitle = em.ECPeriod?.Title ?? "Unknown Period",
                    Position = em.Position,
                    StartDate = (em.ECPeriod?.StartDate ?? em.StartDate).ToLocalTime(),
                    EndDate = (em.ECPeriod?.EndDate ?? em.EndDate)?.ToLocalTime(),
                    ChangeReason = em.ChangeReason,
                    IsCurrent = em.ECPeriod?.IsActive ?? false
                }).OrderByDescending(h => h.StartDate).ToList();
            }

            return dto;
        }

        private MemberSummaryDto MapToSummary(Member m)
        {
            var dto = new MemberSummaryDto
            {
                Id = m.Id,
                FullName = m.FullName,
                MembershipNumber = m.MembershipNumber,
                PhotoPath = m.PhotoPath,
                BloodGroup = m.BloodGroup,
                Email = m.IsEmailPublic ? m.Email : "Confidential",
                IsEmailPublic = m.IsEmailPublic,
                MobileNo = m.IsMobilePublic ? m.MobileNo : "Confidential",
                IsMobilePublic = m.IsMobilePublic,
                MembershipType = m.MembershipType,
                Category = m.Category,
                Status = m.Status,
                AppliedDate = m.AppliedDate.ToLocalTime(),
                IsVerified = m.IsVerified,
                IsFamilyPublic = m.IsFamilyPublic,
                FamilyMembers = new List<MemberFamilyDto>(),
                ECHistory = m.ECMembers?.Select(em => new ECHistoryDto
                {
                    Id = em.Id,
                    PeriodId = em.ECPeriodId,
                    PeriodTitle = em.ECPeriod?.Title ?? "Unknown Period",
                    Position = em.Position,
                    StartDate = em.ECPeriod?.StartDate ?? DateTime.MinValue,
                    EndDate = em.EndDate,
                    ChangeReason = em.ChangeReason,
                    IsCurrent = em.ECPeriod?.IsActive ?? false
                }).OrderByDescending(h => h.StartDate).ToList() ?? new()
            };

            // Enhanced Summary Data from normalized tables
            var ghcRecord = m.AcademicHistory?.FirstOrDefault(a => a.IsGHC);
            if (ghcRecord != null)
            {
                dto.GHCLastCertificatePassingYear = ghcRecord.PassingYear;
                dto.GHCLastCertificate = ghcRecord.Degree;
                dto.GHCLastCertificateSubject = ghcRecord.Subject;

                // 30.27: also populate the flattened Directory fields (PassingYear/Degree/Subject) -
                // these were previously left at their default values, which is the confirmed root
                // cause of "Executive Committee batch information is missing" on the web EC cards.
                dto.PassingYear = ghcRecord.PassingYear;
                dto.Degree = ghcRecord.Degree;
                dto.Subject = ghcRecord.Subject;
            }

            var currentJob = m.ProfessionalHistory?.FirstOrDefault(p => p.IsCurrent);
            if (currentJob != null)
            {
                dto.Designation = currentJob.Designation;
                dto.OrganizationName = currentJob.OrganizationName;
                dto.ProfessionalSector = currentJob.Sector;
                dto.Location = currentJob.Location;
            }

            return dto;
        }
    }
}
