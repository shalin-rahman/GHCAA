using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services;

// Spec 023 (37.12f). The approval row records who asked, who decided and when. The hash-chained
// audit entry for each of these is added by 37.13a.
public sealed class ElectionApprovalService(
    ApplicationDbContext db,
    IElectionService elections,
    IElectionAccessService access,
    IOrgConfigService orgConfig,
    ILogger<ElectionApprovalService> logger) : IElectionApprovalService
{
    private sealed record BallotKeyPayload(string PublicKey, string Fingerprint);

    public async Task<ElectionApprovalRunResult> RunOrRequestAsync(int electionId, ElectionApprovalAction action, int userId, IReadOnlyCollection<string> roles, string? newPublicKey = null, CancellationToken ct = default)
    {
        var needsSecond = await NeedsSecondAsync(action, roles);
        // Setting the first key replaces nothing, so it runs at once.
        if (needsSecond && action == ElectionApprovalAction.ReplaceBallotKey
            && await db.Elections.AnyAsync(x => x.Id == electionId && x.BallotKeyFingerprint == null, ct))
            needsSecond = false;

        if (needsSecond)
        {
            var (_, requestError, approval) = await RequestAsync(electionId, action, userId, newPublicKey, ct);
            return new ElectionApprovalRunResult(false, requestError, approval);
        }

        var (payloadError, payload) = BuildPayload(action, newPublicKey);
        if (payloadError != null)
            return new ElectionApprovalRunResult(false, payloadError, null);
        return new ElectionApprovalRunResult(true, await ExecuteAsync(electionId, action, payload, ct), null);
    }

    public async Task<(bool Success, string? Error, ElectionApprovalDto? Approval)> RequestAsync(int electionId, ElectionApprovalAction action, int userId, string? newPublicKey = null, CancellationToken ct = default)
    {
        var election = await db.Elections.AsNoTracking().Where(x => x.Id == electionId).Select(x => new { x.Phase }).FirstOrDefaultAsync(ct);
        if (election is null)
            return (false, "not-found", null);
        // Checked again when the key is set, but there is no point asking for a key that can no longer change.
        if (action == ElectionApprovalAction.ReplaceBallotKey && election.Phase >= ElectionPhase.Polling)
            return (false, "phase-closed", null);

        var (payloadError, payload) = BuildPayload(action, newPublicKey);
        if (payloadError != null)
            return (false, payloadError, null);

        var now = DateTime.UtcNow;
        // An approved count waiting to run also blocks a second request for one.
        if (await OpenAt(now).Union(ReadyToCountAt(now)).AnyAsync(x => x.ElectionId == electionId && x.Action == action, ct))
            return (false, "already-pending", null);

        var hours = Math.Max(1, (await orgConfig.GetConfigAsync()).Elections.ApprovalExpiryHours);
        var approval = new ElectionApproval
        {
            ElectionId = electionId,
            Action = action,
            PayloadJson = payload,
            RequestedByUserId = userId,
            RequestedAt = now,
            ExpiresAt = now.AddHours(hours)
        };
        db.ElectionApprovals.Add(approval);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Election {ElectionId}: user {UserId} asked for {Action} (approval {ApprovalId}).", electionId, userId, action, approval.Id);

        return (true, null, (await ToDtos(db.ElectionApprovals.Where(x => x.Id == approval.Id), ct)).Single());
    }

    public async Task<(bool Success, string? Error)> ApproveAsync(int approvalId, int userId, IReadOnlyCollection<string> roles, CancellationToken ct = default)
    {
        var approval = await db.ElectionApprovals.AsNoTracking().FirstOrDefaultAsync(x => x.Id == approvalId, ct);
        if (approval is null)
            return (false, "not-found");
        var now = DateTime.UtcNow;
        if (approval.ExecutedAt != null || approval.RejectedAt != null)
            return (false, "closed");
        if (approval.ExpiresAt <= now)
            return (false, "expired");
        if (approval.RequestedByUserId == userId)
            return (false, "same-person");
        if (!await access.HasAsync(approval.ElectionId, userId, roles, ElectionPermission.Approve, ct))
            return (false, "forbidden");

        // The row is claimed with a conditional update, so two approvers at once cannot both run it.
        // The step runs in the same transaction, and a failed step rolls the claim back.
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var claimed = await OpenAt(now).Where(x => x.Id == approvalId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.ApprovedByUserId, userId)
                .SetProperty(x => x.ApprovedAt, now)
                .SetProperty(x => x.ExecutedAt, now), ct);
        if (claimed != 1)
            return (false, "closed");

        var error = await ExecuteAsync(approval.ElectionId, approval.Action, approval.PayloadJson, ct);
        if (error != null)
            return (false, error);

        await transaction.CommitAsync(ct);
        logger.LogInformation("Election {ElectionId}: user {UserId} approved {Action} (approval {ApprovalId}).", approval.ElectionId, userId, approval.Action, approvalId);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RejectAsync(int approvalId, int userId, IReadOnlyCollection<string> roles, string? reason, CancellationToken ct = default)
    {
        var approval = await db.ElectionApprovals.AsNoTracking().FirstOrDefaultAsync(x => x.Id == approvalId, ct);
        if (approval is null)
            return (false, "not-found");
        var now = DateTime.UtcNow;
        if (!approval.IsOpenAt(now))
            return (false, "closed");
        if (!await access.HasAsync(approval.ElectionId, userId, roles, ElectionPermission.Approve, ct))
            return (false, "forbidden");

        // Conditional like the approve claim, so a reject cannot land on a step that has just run.
        var trimmed = reason?.Trim();
        var rejectReason = string.IsNullOrEmpty(trimmed) ? null : trimmed[..Math.Min(trimmed.Length, 500)];
        var rejected = await OpenAt(now).Where(x => x.Id == approvalId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.RejectedByUserId, userId)
                .SetProperty(x => x.RejectedAt, now)
                .SetProperty(x => x.RejectReason, rejectReason), ct);
        if (rejected != 1)
            return (false, "closed");
        logger.LogInformation("Election {ElectionId}: user {UserId} rejected {Action} (approval {ApprovalId}).", approval.ElectionId, userId, approval.Action, approvalId);
        return (true, null);
    }

    public async Task<ElectionCountRunResult> CountAsync(int electionId, int userId, IReadOnlyCollection<string> roles, string? privateKeyPkcs8Base64, CancellationToken ct = default)
    {
        // Results already stored, or an election not in counting, need no approval.
        var (stored, probeError) = await elections.CountAsync(electionId, null, ct);
        if (stored is not null || probeError == "not-counting")
            return new ElectionCountRunResult(stored, probeError, null);
        // A caller with no key only wanted the stored results. It must not store a count request.
        if (string.IsNullOrWhiteSpace(privateKeyPkcs8Base64))
            return new ElectionCountRunResult(null, "no-key", null);

        if (!await NeedsSecondAsync(ElectionApprovalAction.Count, roles))
        {
            var (results, error) = await elections.CountAsync(electionId, privateKeyPkcs8Base64, ct);
            return new ElectionCountRunResult(results, error, null);
        }

        var now = DateTime.UtcNow;
        var approved = await ReadyToCountAt(now).Where(x => x.ElectionId == electionId)
            .OrderBy(x => x.ApprovedAt).AsNoTracking().FirstOrDefaultAsync(ct);
        if (approved is null)
        {
            var (_, requestError, pending) = await RequestAsync(electionId, ElectionApprovalAction.Count, userId, null, ct);
            return new ElectionCountRunResult(null, requestError, pending);
        }
        if (approved.ApprovedByUserId == userId)
            return new ElectionCountRunResult(null, "same-person", null);

        // The count opens its own transaction, so the claim cannot share it. A count that fails
        // gives the approval back, so a wrong key file does not use it up.
        var claimed = await ReadyToCountAt(now).Where(x => x.Id == approved.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ConsumedAt, now), ct);
        if (claimed != 1)
            return new ElectionCountRunResult(null, "closed", null);

        var (counted, countError) = await elections.CountAsync(electionId, privateKeyPkcs8Base64, ct);
        if (counted is null)
        {
            await db.ElectionApprovals.Where(x => x.Id == approved.Id && x.ConsumedAt == now)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.ConsumedAt, (DateTime?)null), CancellationToken.None);
            return new ElectionCountRunResult(null, countError, null);
        }
        logger.LogInformation("Election {ElectionId}: user {UserId} counted under approval {ApprovalId}.", electionId, userId, approved.Id);
        return new ElectionCountRunResult(counted, null, null);
    }

    public async Task<IReadOnlyList<ElectionApprovalDto>> ListOpenAsync(int electionId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var waiting = OpenAt(now).Union(ReadyToCountAt(now));
        return await ToDtos(waiting.Where(x => x.ElectionId == electionId).OrderBy(x => x.RequestedAt), ct);
    }

    private async Task<bool> NeedsSecondAsync(ElectionApprovalAction action, IReadOnlyCollection<string> roles)
    {
        var settings = (await orgConfig.GetConfigAsync()).Elections;
        return settings.TwoPersonActions.Contains(action.ToString(), StringComparer.OrdinalIgnoreCase)
            && !(settings.SuperAdminActsAlone && roles.Contains(Constants.Roles.SuperAdmin));
    }

    private IQueryable<ElectionApproval> OpenAt(DateTime now) =>
        db.ElectionApprovals.Where(x => x.ExecutedAt == null && x.RejectedAt == null && x.ExpiresAt > now);

    // An approved count that has not run yet. It still has to run before the request expires.
    private IQueryable<ElectionApproval> ReadyToCountAt(DateTime now) =>
        db.ElectionApprovals.Where(x => x.Action == ElectionApprovalAction.Count && x.ApprovedAt != null
            && x.ConsumedAt == null && x.RejectedAt == null && x.ExpiresAt > now);

    private async Task<List<ElectionApprovalDto>> ToDtos(IQueryable<ElectionApproval> query, CancellationToken ct)
    {
        var rows = await query
            .Select(x => new { Approval = x, RequestedBy = db.Users.Where(u => u.Id == x.RequestedByUserId).Select(u => u.Username).FirstOrDefault() })
            .AsNoTracking()
            .ToListAsync(ct);
        return rows.Select(r => new ElectionApprovalDto(
            r.Approval.Id, r.Approval.ElectionId, r.Approval.Action, r.Approval.RequestedByUserId, r.RequestedBy ?? "",
            r.Approval.RequestedAt, r.Approval.ExpiresAt, ReadKey(r.Approval.PayloadJson)?.Fingerprint,
            r.Approval.Action == ElectionApprovalAction.Count ? r.Approval.ApprovedAt : null)).ToList();
    }

    private static (string? Error, string? PayloadJson) BuildPayload(ElectionApprovalAction action, string? newPublicKey)
    {
        if (action != ElectionApprovalAction.ReplaceBallotKey)
            return (null, null);
        var key = newPublicKey?.Trim();
        var fingerprint = string.IsNullOrEmpty(key) ? null : BallotSeal.Fingerprint(key);
        return fingerprint == null ? ("invalid-key", null) : (null, JsonSerializer.Serialize(new BallotKeyPayload(key!, fingerprint)));
    }

    private static BallotKeyPayload? ReadKey(string? payloadJson) =>
        string.IsNullOrEmpty(payloadJson) ? null : JsonSerializer.Deserialize<BallotKeyPayload>(payloadJson);

    // Returns null when the step ran.
    private async Task<string?> ExecuteAsync(int electionId, ElectionApprovalAction action, string? payloadJson, CancellationToken ct)
    {
        switch (action)
        {
            case ElectionApprovalAction.ReplaceBallotKey:
                var payload = ReadKey(payloadJson);
                if (payload is null)
                    return "invalid-key";
                var (success, error, fingerprint) = await elections.SetBallotKeyAsync(electionId, payload.PublicKey, ct);
                if (!success)
                    return error;
                return fingerprint == payload.Fingerprint ? null : "invalid-key";
            case ElectionApprovalAction.Declare:
                return await elections.DeclareAsync(electionId, ct) ? null : "not-ready";
            case ElectionApprovalAction.Count:
                // Approving only allows the count. It runs when the requester brings the key.
                return await db.Elections.AnyAsync(x => x.Id == electionId && x.Phase == ElectionPhase.Counting, ct) ? null : "not-ready";
            default:
                return await elections.SetPhaseAsync(electionId, PhaseFor(action), ct) ? null : "not-ready";
        }
    }

    private static ElectionPhase PhaseFor(ElectionApprovalAction action) => action switch
    {
        ElectionApprovalAction.Publish => ElectionPhase.Nomination,
        ElectionApprovalAction.OpenPolling => ElectionPhase.Polling,
        ElectionApprovalAction.ClosePolling => ElectionPhase.Counting,
        ElectionApprovalAction.Archive => ElectionPhase.Archived,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
    };
}
