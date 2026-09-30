using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.Interfaces;

// Spec 023 (37.12f). The two-person rule. Roles are passed in because Approve comes from the
// caller's role as well as their appointments.
public interface IElectionApprovalService
{
    // Runs the step at once when it is not a two-person step, or the caller is SuperAdmin and
    // SuperAdminActsAlone is on. Otherwise stores it. newPublicKey is used only by ReplaceBallotKey.
    Task<ElectionApprovalRunResult> RunOrRequestAsync(int electionId, ElectionApprovalAction action, int userId, IReadOnlyCollection<string> roles, string? newPublicKey = null, CancellationToken ct = default);
    Task<(bool Success, string? Error, ElectionApprovalDto? Approval)> RequestAsync(int electionId, ElectionApprovalAction action, int userId, string? newPublicKey = null, CancellationToken ct = default);
    Task<(bool Success, string? Error)> ApproveAsync(int approvalId, int userId, IReadOnlyCollection<string> roles, CancellationToken ct = default);
    Task<(bool Success, string? Error)> RejectAsync(int approvalId, int userId, IReadOnlyCollection<string> roles, string? reason, CancellationToken ct = default);
    Task<IReadOnlyList<ElectionApprovalDto>> ListOpenAsync(int electionId, CancellationToken ct = default);
}
