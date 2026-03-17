using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface IGovernanceService
    {
        Task<IEnumerable<ECPeriod>> GetAllPeriodsAsync(CancellationToken cancellationToken = default);
        Task<ECPeriod?> GetPeriodByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ECPeriod> CreatePeriodAsync(string title, DateTime startDate, DateTime? endDate, CancellationToken cancellationToken = default);
        Task<bool> UpdatePeriodAsync(int id, string title, DateTime startDate, DateTime? endDate, bool isActive, CancellationToken cancellationToken = default);
        Task<bool> ActivatePeriodAsync(int id, CancellationToken cancellationToken = default);
        
        Task<IEnumerable<ECMember>> GetCommitteeMembersAsync(int periodId, CancellationToken cancellationToken = default);
        Task<bool> AssignMemberToRoleAsync(int periodId, int memberId, int position, string? reason, CancellationToken cancellationToken = default);
        Task<bool> RemoveMemberFromCommitteeAsync(int ecMemberId, CancellationToken cancellationToken = default);
        Task<bool> DeleteECMemberAsync(int id, CancellationToken cancellationToken = default);
    }
}
