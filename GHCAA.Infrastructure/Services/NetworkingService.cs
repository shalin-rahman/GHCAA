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

        public async Task<IEnumerable<MemberProfileDto>> SearchMembersAsync(MemberSearchFilterDto filter, CancellationToken cancellationToken = default)
        {
            var query = _db.Members
                .Where(m => m.Status == Enums.MembershipStatus.Active && !m.IsArchived)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.FullName))
                query = query.Where(m => m.FullName.Contains(filter.FullName));

            if (filter.PassingYear.HasValue)
                query = query.Where(m => m.GHCLastCertificatePassingYear == filter.PassingYear.Value);

            if (!string.IsNullOrEmpty(filter.BloodGroup))
                query = query.Where(m => m.BloodGroup.ToString() == filter.BloodGroup);

            if (!string.IsNullOrEmpty(filter.ProfessionalSector))
                query = query.Where(m => m.ProfessionalSector == filter.ProfessionalSector);

            if (!string.IsNullOrEmpty(filter.Designation))
                query = query.Where(m => m.Designation.Contains(filter.Designation));

            if (!string.IsNullOrEmpty(filter.ECPosition) && System.Enum.TryParse<Enums.ECPosition>(filter.ECPosition, true, out var pos))
                query = query.Where(m => m.ECPosition == pos);

            var members = await query.ToListAsync(cancellationToken);

            return members.Select(MapToDto);
        }

        public async Task<IEnumerable<MemberProfileDto>> GetExecutiveCommitteeAsync(int? periodId = null, CancellationToken cancellationToken = default)
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

            var results = await query.ToListAsync(cancellationToken);
            return results.Select(em => {
                var dto = MapToDto(em.Member!);
                dto.ECPosition = em.Position; // Use position from history for that period
                return dto;
            });
        }

        public async Task<IEnumerable<object>> GetECPeriodsAsync(CancellationToken cancellationToken = default)
        {
            return await _db.ECPeriods
                .OrderByDescending(p => p.StartDate)
                .Select(p => new { p.Id, p.Title, p.IsActive })
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MemberProfileDto>> GetLatestAlumniUpdatesAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            var members = await _db.Members
                .Where(m => m.Status == Enums.MembershipStatus.Active && !m.IsArchived)
                .OrderByDescending(m => m.LastUpdateDate)
                .Take(count)
                .ToListAsync(cancellationToken);

            return members.Select(MapToDto);
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
                GHCLastCertificatePassingYear = m.GHCLastCertificatePassingYear,
                LastCertificateFromGHC = m.LastCertificateFromGHC,
                SubjectGroup = m.SubjectGroup,
                ProfessionalSector = m.ProfessionalSector,
                Designation = m.Designation,
                PhotoPath = m.PhotoPath,
                PresentAddress = m.IsAddressPublic ? m.PresentAddress : "Confidential",
                PermanentAddress = m.IsAddressPublic ? m.PermanentAddress : "Confidential",
                BloodGroup = m.BloodGroup,
                MembershipType = m.MembershipType,
                Category = m.Category,
                ECPosition = m.ECPosition,
                IsMobilePublic = m.IsMobilePublic,
                IsEmailPublic = m.IsEmailPublic,
                IsAddressPublic = m.IsAddressPublic
            };

            if (m.ECMembers != null && m.ECMembers.Any())
            {
                dto.ECHistory = m.ECMembers.Select(em => new ECHistoryDto
                {
                    PeriodTitle = em.ECPeriod?.Title ?? "Unknown Period",
                    Position = em.Position,
                    StartDate = em.ECPeriod?.StartDate ?? DateTime.MinValue,
                    EndDate = em.ECPeriod?.EndDate
                }).OrderByDescending(h => h.StartDate).ToList();
            }

            return dto;
        }
    }
}
