using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces;

// 37.14m. Data the printed election forms need that the main election flow does not collect:
// calendar and notice details, candidate consent, scrutiny ticks and recount requests.
// Errors are Constants.Elections.Replies values. Detail, where returned, says what to fix.
public interface IElectionFormService
{
    Task<(string? Error, string? Detail)> UpdateFormDetailsAsync(int electionId, UpdateElectionFormDetailsRequest request, int actorUserId, CancellationToken ct = default);
    Task<string?> RecordConsentAsync(int electionId, int nominationId, DateTime consentAt, int actorUserId, CancellationToken ct = default);
    // Null when the nomination is not on this election.
    Task<IReadOnlyList<ScrutinyCheckDto>?> GetScrutinyChecksAsync(int electionId, int nominationId, CancellationToken ct = default);
    Task<string?> SetScrutinyCheckAsync(int electionId, int nominationId, SetScrutinyCheckRequest request, int actorUserId, CancellationToken ct = default);
    Task<IReadOnlyList<ElectionRecountDto>> ListRecountsAsync(int electionId, CancellationToken ct = default);
    Task<(ElectionRecountDto? Recount, string? Error, string? Detail)> RecordRecountAsync(int electionId, RecordRecountRequest request, int actorUserId, CancellationToken ct = default);
    Task<string?> DecideRecountAsync(int electionId, int recountId, DecideRecountRequest request, int actorUserId, CancellationToken ct = default);
    Task<(string? Error, string? Detail)> RecordRecountFiguresAsync(int electionId, int recountId, IReadOnlyList<RecountFigureInput> figures, int actorUserId, CancellationToken ct = default);
}
