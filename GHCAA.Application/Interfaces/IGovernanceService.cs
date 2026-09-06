using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface IGovernanceService
    {
        Task<IEnumerable<ECPeriodDto>> GetAllPeriodsAsync(CancellationToken cancellationToken = default);
        Task<ECPeriodDto?> GetPeriodByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ECPeriodDto> CreatePeriodAsync(string title, DateTime startDate, DateTime? endDate, CancellationToken cancellationToken = default);
        Task<bool> UpdatePeriodAsync(int id, string title, DateTime startDate, DateTime? endDate, bool isActive, CancellationToken cancellationToken = default);
        Task<bool> ActivatePeriodAsync(int id, CancellationToken cancellationToken = default);

        Task<IEnumerable<ECMemberDto>> GetCommitteeMembersAsync(int periodId, CancellationToken cancellationToken = default);
        Task<bool> AssignMemberToRoleAsync(int periodId, int memberId, int position, string? reason, bool notifyMember = false, CancellationToken cancellationToken = default);
        Task<bool> RemoveMemberFromCommitteeAsync(int ecMemberId, bool notifyMember = false, CancellationToken cancellationToken = default);
        Task<bool> DeleteECMemberAsync(int id, int adminId, bool notifyMember = false, CancellationToken cancellationToken = default);

        Task<ECPeriodDto?> GetActivePeriodAsync(CancellationToken cancellationToken = default);

        // Constitution logic
        Task<Constitution?> GetActiveConstitutionAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Constitution>> GetConstitutionHistoryAsync(CancellationToken cancellationToken = default);
        Task<bool> CreateConstitutionVersionAsync(string version, string content, string changeSummary, CancellationToken cancellationToken = default);
        Task<bool> ActivateConstitutionAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> VoteOnConstitutionAsync(int constitutionId, int memberId, bool isFor, string? comments, CancellationToken cancellationToken = default);
    }
}
