using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    public interface INetworkingService
    {
        Task<MemberProfileDto?> GetMemberProfileAsync(int memberId, CancellationToken cancellationToken = default);
        Task<PagedResult<MemberSummaryDto>> SearchMembersAsync(MemberSearchFilterDto filter, CancellationToken cancellationToken = default);
        Task<IEnumerable<MemberSummaryDto>> GetExecutiveCommitteeAsync(int? periodId = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<object>> GetECPeriodsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<MemberSummaryDto>> GetLatestAlumniUpdatesAsync(int count = 10, CancellationToken cancellationToken = default);
    }

    public class MemberSearchFilterDto
    {
        public string? Query { get; set; }
        public int? PassingYear { get; set; }
        public string? BloodGroup { get; set; }
        public string? ProfessionalSector { get; set; }
        public string? Designation { get; set; }
        public string? ECPosition { get; set; }
        public string? Category { get; set; }
        public string? MembershipType { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage => Page < TotalPages;
    }
}
