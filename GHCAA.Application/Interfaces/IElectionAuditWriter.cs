using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces;

// Spec 023 FR-048 (37.13a). The hash chain kept for each election.
public interface IElectionAuditWriter
{
    // Adds the next entry to the caller's context without saving, so it lands in the caller's SaveChanges.
    Task AppendAsync(int electionId, string action, int? actorUserId, object? detail, CancellationToken ct = default);
    // Saves the caller's context. If another writer took the same Seq, the pending entries are
    // numbered again and saved once more.
    Task SaveAsync(CancellationToken ct = default);
    Task<ElectionAuditVerifyDto> VerifyAsync(int electionId, CancellationToken ct = default);
    Task<IReadOnlyList<ElectionAuditEntryDto>> ListAsync(int electionId, CancellationToken ct = default);
}
