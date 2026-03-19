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
                .Include(m => m.ECMembers)
                .ThenInclude(em => em.ECPeriod)
                .Where(m => m.Id == memberId && m.Status == Enums.MembershipStatus.Active && !m.IsArchived)
                .FirstOrDefaultAsync(cancellationToken);

            return member == null ? null : MapToDto(member);
        }

        public async Task<PagedResult<MemberSummaryDto>> SearchMembersAsync(MemberSearchFilterDto filter, CancellationToken cancellationToken = default)
        {
            var query = _db.Members
                .Where(m => m.Status == Enums.MembershipStatus.Active && !m.IsArchived)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Query))
            {
                var q = filter.Query.ToLower();
                query = query.Where(m => 
                    m.FullName.ToLower().Contains(q) || 
                    (m.MembershipNumber != null && m.MembershipNumber.ToLower().Contains(q)) ||
                    (m.Email != null && m.Email.ToLower().Contains(q)));
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

            var totalItems = await query.CountAsync(cancellationToken);
            var pageSize = Math.Clamp(filter.PageSize, 1, 100);
            var page = Math.Max(filter.Page, 1);
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var members = await query
                .Include(m => m.ECMembers)
                .ThenInclude(em => em.ECPeriod)
                .Include(m => m.AcademicHistory)
                .Include(m => m.ProfessionalHistory)
                .OrderBy(m => m.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<MemberSummaryDto>
            {
                Items = members.Select(MapToSummary),
                TotalItems = totalItems,
                TotalPages = totalPages,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<MemberSummaryDto>> GetExecutiveCommitteeAsync(int? periodId = null, CancellationToken cancellationToken = default)
        {
            // If no period specified, get current active one
            var query = _db.ECMembers
                .Include(em => em.Member)
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
                .Select(p => new { p.Id, p.Title, p.IsActive })
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MemberSummaryDto>> GetLatestAlumniUpdatesAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            var members = await _db.Members
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
                
                // Personal
                FatherName = m.FatherName,
                MotherName = m.MotherName,
                DateOfBirth = m.DateOfBirth,
                AppliedDate = DateTime.SpecifyKind(m.AppliedDate, DateTimeKind.Utc),
                ApprovedDate = m.ApprovedDate.HasValue ? DateTime.SpecifyKind(m.ApprovedDate.Value, DateTimeKind.Utc) : null,
                Gender = m.Gender,
                BloodGroup = m.BloodGroup,
                NID = m.NID,
                EmergencyContactName = m.EmergencyContactName,
                EmergencyContactRelation = m.EmergencyContactRelation,
                EmergencyContactPhone = m.EmergencyContactPhone,

                // History
                AcademicHistory = m.AcademicHistory.Select(a => new AcademicRecordDto {
                    Id = a.Id,
                    InstitutionName = a.InstitutionName,
                    Degree = a.Degree,
                    Subject = a.Subject,
                    AdmissionYear = a.AdmissionYear,
                    PassingYear = a.PassingYear,
                    IsGHC = a.IsGHC,
                    Result = a.Result,
                    CertificatePath = a.CertificatePath
                }).ToList(),
                ProfessionalHistory = m.ProfessionalHistory.Select(p => new ProfessionalRecordDto {
                    Id = p.Id,
                    OrganizationName = p.OrganizationName,
                    Designation = p.Designation,
                    Sector = p.Sector,
                    Location = p.Location,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    IsCurrent = p.IsCurrent
                }).ToList(),

                // Info & Privacy
                PhotoPath = m.PhotoPath,
                PresentAddress = m.IsAddressPublic ? m.PresentAddress : "Confidential",
                PermanentAddress = m.IsAddressPublic ? m.PermanentAddress : "Confidential",
                IsMobilePublic = m.IsMobilePublic,
                IsEmailPublic = m.IsEmailPublic,
                IsAddressPublic = m.IsAddressPublic,
                
                MembershipType = m.MembershipType,
                Category = m.Category,
                ECHistory = new List<ECHistoryDto>()
            };

            if (m.ECMembers != null && m.ECMembers.Any())
            {
                dto.ECHistory = m.ECMembers.Select(em => new ECHistoryDto
                {
                    Id = em.Id,
                    PeriodId = em.ECPeriodId,
                    PeriodTitle = em.ECPeriod?.Title ?? "Unknown Period",
                    Position = em.Position,
                    StartDate = em.ECPeriod?.StartDate ?? DateTime.MinValue,
                    EndDate = em.ECPeriod?.EndDate,
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
            }

            return dto;
        }
    }
}
