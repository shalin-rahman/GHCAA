using GHCAA.Application.DTOs;
using GHCAA.Domain.Models;
using static GHCAA.Domain.Enums;

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
        Task<IEnumerable<CommitteeSeatDto>> GetCommitteeSeatsAsync(int periodId, CancellationToken cancellationToken = default);
        // A seat with a current holder is refused unless endCurrentHolderReason is given, which
        // ends that holder's term in the same save.
        Task<bool> AssignMemberToRoleAsync(int periodId, int memberId, int position, string? reason, bool notifyMember = false,
            VacancyReason? endCurrentHolderReason = null, string? endCurrentHolderNote = null, CancellationToken cancellationToken = default);
        Task<bool> RemoveMemberFromCommitteeAsync(int ecMemberId, VacancyReason reason, string? note, bool notifyMember = false, CancellationToken cancellationToken = default);
        Task<bool> DeleteECMemberAsync(int id, int adminId, bool notifyMember = false, CancellationToken cancellationToken = default);

        Task<ECPeriodDto?> GetActivePeriodAsync(CancellationToken cancellationToken = default);

        // 37.14m: the ER-34 handover checklist. Get returns null when the term does not exist.
        // Set returns false for a missing term and throws InvalidOperationException when another
        // admin saved the same item at the same moment.
        Task<IReadOnlyList<HandoverCheckDto>?> GetHandoverChecksAsync(int periodId, CancellationToken cancellationToken = default);
        Task<bool> SetHandoverCheckAsync(int periodId, SetHandoverCheckRequest request, int actorUserId, CancellationToken cancellationToken = default);

        // 95.2: committee positions. Retire a position that has past terms or seats; delete only
        // one nobody has used.
        Task<IReadOnlyList<EcPositionDto>> GetPositionsAsync(bool includeRetired, CancellationToken cancellationToken = default);
        Task<EcPositionDto> CreatePositionAsync(SaveEcPositionRequest request, CancellationToken cancellationToken = default);
        Task<EcPositionDto?> UpdatePositionAsync(int id, SaveEcPositionRequest request, CancellationToken cancellationToken = default);
        Task<bool> SetPositionActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default);
        Task<bool> DeletePositionAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<InvalidPositionRowDto>> GetInvalidPositionRowsAsync(CancellationToken cancellationToken = default);
        Task<bool> RepairPositionAsync(RepairPositionRequest request, CancellationToken cancellationToken = default);

        // Constitution logic
        Task<Constitution?> GetActiveConstitutionAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Constitution>> GetConstitutionHistoryAsync(CancellationToken cancellationToken = default);
        Task<bool> CreateConstitutionVersionAsync(string version, string content, string changeSummary, CancellationToken cancellationToken = default);
        Task<bool> ActivateConstitutionAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> VoteOnConstitutionAsync(int constitutionId, int memberId, bool isFor, string? comments, CancellationToken cancellationToken = default);
    }
}
