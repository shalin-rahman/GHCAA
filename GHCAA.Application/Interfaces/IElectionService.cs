using GHCAA.Application.DTOs;
using GHCAA.Domain;

namespace GHCAA.Application.Interfaces;

public interface IElectionService
{
    // 37.1t: Error is set when the EC period is missing, starts before polling closes, or already has members.
    Task<(ElectionSummaryDto? Election, string? Error)> CreateAsync(CreateElectionDto request, CancellationToken ct = default);
    Task<ElectionSummaryDto?> GetAsync(int id, CancellationToken ct = default);
    Task<ElectionSummaryDto?> GetCurrentAsync(CancellationToken ct = default);
    // The newest Declared or Archived election that is not a test. Null when there is none.
    Task<ElectionSummaryDto?> GetLatestDeclaredAsync(CancellationToken ct = default);
    Task<int> AddSeatAsync(int id, ElectionSeatRequestDto request, CancellationToken ct = default);
    Task<string?> CheckSeatAsync(int? electionId, ElectionSeatRequestDto request, CancellationToken ct = default);
    // 95.2: an active position whose key or name matches the title, ignoring case, spaces and punctuation.
    Task<int?> FindPositionIdAsync(string? title, CancellationToken ct = default);
    Task<bool> SetPhaseAsync(int id, Enums.ElectionPhase phase, CancellationToken ct = default);
    Task<int> FreezeVoterRollAsync(int id, CancellationToken ct = default);
    // Deletes the frozen roll so it can be frozen again. Error codes: not-found, reason-required, phase-closed, window-closed, has-nominations.
    Task<(bool Success, string? Error, int Removed)> UnfreezeVoterRollAsync(int id, int userId, string reason, CancellationToken ct = default);
    Task<NominationViewDto> SubmitNominationAsync(int id, NominationDto request, CancellationToken ct = default);
    Task<bool> DecideNominationAsync(int nominationId, int decidedByUserId, ScrutinyDto request, CancellationToken ct = default);
    Task<bool> WithdrawNominationAsync(int nominationId, int memberId, CancellationToken ct = default);
    Task<(bool Success, string? Error, string? TrackingCode)> CastBallotAsync(int id, int memberId, CastBallotDto request, CancellationToken ct = default);
    // Null when the election is not in Counting or ballots are still waiting to be moved.
    Task<(bool Success, string? Error, string? Fingerprint)> SetBallotKeyAsync(int id, string publicKeySpkiBase64, CancellationToken ct = default);
    Task<(IReadOnlyList<ElectionResultDto>? Results, string? Error)> CountAsync(int id, string? privateKeyPkcs8Base64, CancellationToken ct = default);
    Task<bool> DeclareAsync(int id, CancellationToken ct = default);
    /// <summary>FR-050. Null when recorded, else one of Elections.Replies: NotFound, NotDeclared or AlreadyRecorded.</summary>
    Task<string?> RecordKeyDestroyedAsync(int id, int actorUserId, CancellationToken ct = default);
    /// <summary>37.12w. Null when the election is not found.</summary>
    Task<ElectionReadinessDto?> GetReadinessAsync(int id, CancellationToken ct = default);
    /// <summary>37.12w. Null when recorded, else NotFound, Invalid (not an attestation item), WrongPhase or AlreadyRecorded.</summary>
    Task<string?> AttestReadinessAsync(int id, int actorUserId, string condition, CancellationToken ct = default);
    Task<IReadOnlyList<NominationViewDto>> GetNominationsAsync(int id, CancellationToken ct = default);
    // 37.14m. The same list with the ER-10 consent time, for officials only.
    Task<IReadOnlyList<NominationViewDto>> GetNominationsWithConsentAsync(int id, CancellationToken ct = default);
    Task<BallotViewDto?> GetBallotAsync(int id, int memberId, CancellationToken ct = default);
    // Null when the election does not exist.
    Task<IReadOnlyList<ElectionSeatViewDto>?> GetSeatsAsync(int id, CancellationToken ct = default);
    // Null until CandidateList is finalized; returns only accepted candidates.
    Task<IReadOnlyList<PublicCandidateSeatDto>?> GetPublicCandidateListAsync(int id, CancellationToken ct = default);
    // Null until the count has stored results. Sorted, with no choices (spec 023 FR-016).
    Task<IReadOnlyList<string>?> GetTrackingCodesAsync(int id, CancellationToken ct = default);
    // Null unless the election is Declared or Archived (spec 023 FR-017).
    Task<PublicElectionResultsDto?> GetPublicResultsAsync(int id, CancellationToken ct = default);
    // Null until Declared, or when the PublishPerSeatBallots setting is off.
    Task<OpenedBallotsDto?> GetOpenedBallotsAsync(int id, CancellationToken ct = default);
    // Spec 023 FR-033. Null when the election does not exist.
    Task<ElectionBoardDto?> GetBoardAsync(int id, CancellationToken ct = default);
    // FR-047. The public reads answer 404 for a test election; admin reads do not ask.
    Task<bool> IsTestAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<AdminElectionDto>> ListAdminElectionsAsync(CancellationToken ct = default);
    Task<AdminElectionDto?> GetAdminElectionAsync(int id, CancellationToken ct = default);
    Task<(bool Success, string? Error, AdminElectionDto? Election)> AddCandidateAsync(int id, SaveCandidateRequest request, CancellationToken ct = default);
    Task<(bool Success, string? Error)> RemoveCandidateAsync(int id, int candidateId, CancellationToken ct = default);
}
