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

        public async Task<IEnumerable<MemberProfileDto>> SearchMembersAsync(MemberSearchFilterDto filter, CancellationToken cancellationToken = default)
        {
            var query = _db.Members.AsQueryable();

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

        public async Task<IEnumerable<MemberProfileDto>> GetExecutiveCommitteeAsync(int? year = null, CancellationToken cancellationToken = default)
        {
            // For now, filtering by ECPosition being not None
            var query = _db.Members.Where(m => m.ECPosition != Enums.ECPosition.None);

            // If year is provided, we might filter by GHCAdmissionYear or a specific term (not yet implemented)
            // Assuming current EC for now.
            
            var members = await query.OrderBy(m => m.FullName).ToListAsync(cancellationToken);
            return members.Select(MapToDto);
        }

        public async Task<IEnumerable<MemberProfileDto>> GetLatestAlumniUpdatesAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            var members = await _db.Members
                .OrderByDescending(m => m.LastUpdateDate)
                .Take(count)
                .ToListAsync(cancellationToken);

            return members.Select(MapToDto);
        }

        private MemberProfileDto MapToDto(Member m)
        {
            // Privacy Filtering
            return new MemberProfileDto
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
                IsMobilePublic = m.IsMobilePublic,
                IsEmailPublic = m.IsEmailPublic,
                IsAddressPublic = m.IsAddressPublic
            };
        }
    }
}
