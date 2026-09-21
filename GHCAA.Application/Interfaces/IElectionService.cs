using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Domain;

namespace GHCAA.Application.Interfaces;

public interface IElectionService
{
    Task<ElectionSummaryDto> CreateAsync(CreateElectionDto request, CancellationToken ct = default);
    Task<ElectionSummaryDto?> GetAsync(int id, CancellationToken ct = default);
    Task<int> AddSeatAsync(int id, ElectionSeatRequestDto request, CancellationToken ct = default);
    Task<bool> AssignOfficerAsync(int id, ElectionOfficerDto request, CancellationToken ct = default);
    Task<bool> SetPhaseAsync(int id, Enums.ElectionPhase phase, CancellationToken ct = default);
    Task<int> FreezeVoterRollAsync(int id, CancellationToken ct = default);
    Task<NominationViewDto> SubmitNominationAsync(int id, NominationDto request, CancellationToken ct = default);
    Task<bool> DecideNominationAsync(int nominationId, ScrutinyDto request, CancellationToken ct = default);
    Task<bool> WithdrawNominationAsync(int nominationId, int memberId, CancellationToken ct = default);
    Task<bool> CastVoteAsync(int id, int memberId, CastVoteDto request, CancellationToken ct = default);
    Task<IReadOnlyList<ElectionResultDto>> CountAsync(int id, CancellationToken ct = default);
    Task<bool> DeclareAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<NominationViewDto>> GetNominationsAsync(int id, CancellationToken ct = default);
}
