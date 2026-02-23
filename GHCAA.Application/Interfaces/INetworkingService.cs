using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    public interface INetworkingService
    {
        Task<IEnumerable<MemberProfileDto>> SearchMembersAsync(MemberSearchFilterDto filter, CancellationToken cancellationToken = default);
        Task<MemberProfileDto?> GetMemberProfileAsync(int memberId, CancellationToken cancellationToken = default);
        Task<IEnumerable<MemberProfileDto>> GetExecutiveCommitteeAsync(int? year = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<MemberProfileDto>> GetLatestAlumniUpdatesAsync(int count = 10, CancellationToken cancellationToken = default);
    }

    public class MemberSearchFilterDto
    {
        public string? FullName { get; set; }
        public int? PassingYear { get; set; }
        public string? BloodGroup { get; set; }
        public string? ProfessionalSector { get; set; }
        public string? Designation { get; set; }
        public string? ECPosition { get; set; }
    }
}
