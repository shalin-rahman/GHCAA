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
    IElectionAppointmentService appointments,
    IActivityService activity,
    ILogger<ElectionApprovalService> logger,
    IElectionFreezeService freeze) : IElectionApprovalService
{
    private sealed record BallotKeyPayload(string PublicKey, string Fingerprint);
    private sealed record RevokePayload(int AppointmentId, string Reason);

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
        await EnsureKeyNotFrozenAsync(electionId, action, payload, ct);
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
        await EnsureKeyNotFrozenAsync(electionId, action, payload, ct);

        var now = DateTime.UtcNow;
        var key = ElectionApproval.KeyFor(electionId, action);
        await ReleaseKeyAsync(key, now, ct);
        // An approved count waiting to run also blocks a second request for one.
        if (await Blocking(now).AnyAsync(x => x.OpenKey == key, ct))
            return (false, "already-pending", null);

        var hours = Math.Max(1, (await orgConfig.GetConfigAsync()).Elections.ApprovalExpiryHours);
        var approval = new ElectionApproval
        {
            ElectionId = electionId,
            Action = action,
            PayloadJson = payload,
            RequestedByUserId = userId,
            RequestedAt = now,
            ExpiresAt = now.AddHours(hours),
            OpenKey = key
        };
        if (!await TryInsertAsync(approval, ct))
            return (false, "already-pending", null);
        logger.LogInformation("Election {ElectionId}: user {UserId} asked for {Action} (approval {ApprovalId}).", electionId, userId, action, approval.Id);

        return (true, null, (await ToDtos(db.ElectionApprovals.Where(x => x.Id == approval.Id), ct)).Single());
    }

    public async Task<(bool Success, string? Error, ElectionApprovalDto? Approval)> RequestEmergencyRevokeAsync(int appointmentId, int userId, IReadOnlyCollection<string> roles, string? reason, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var trimmed = reason?.Trim();
        var target = await db.ElectionAppointments.AsNoTracking().Where(ElectionAppointment.LiveAt(now)).FirstOrDefaultAsync(x => x.Id == appointmentId, ct);

        string? error = null;
        if (!roles.Contains(Constants.Roles.SuperAdmin))
            error = "forbidden";
        else if (string.IsNullOrEmpty(trimmed))
            error = "reason-required";
        else if (target is null)
            error = "not-found";
        else if (await RevokePendingAsync(target.ElectionId, appointmentId, now, ct))
            error = "already-pending";
        else if (!await EligibleApprovers(target.ElectionId, userId, target.UserId, now).AnyAsync(ct))
            error = "no-eligible-approver";

        if (error != null)
        {
            await AuditRevokeAsync(Constants.Elections.EmergencyRevokeOutcomes.RefusedPrefix + error, target?.ElectionId, null, userId, null, appointmentId, trimmed, Snapshot(target), null, ct);
            return (false, error, null);
        }

        var hours = Math.Max(1, (await orgConfig.GetConfigAsync()).Elections.ApprovalExpiryHours);
        var approval = new ElectionApproval
        {
            ElectionId = target!.ElectionId,
            Action = ElectionApprovalAction.EmergencyRevoke,
            PayloadJson = JsonSerializer.Serialize(new RevokePayload(appointmentId, trimmed![..Math.Min(trimmed.Length, 500)])),
            RequestedByUserId = userId,
            RequestedAt = now,
            ExpiresAt = now.AddHours(hours),
            OpenKey = ElectionApproval.KeyFor(target.ElectionId, ElectionApprovalAction.EmergencyRevoke, appointmentId)
        };
        if (!await TryInsertAsync(approval, ct))
        {
            await AuditRevokeAsync(Constants.Elections.EmergencyRevokeOutcomes.RefusedPrefix + "already-pending", target.ElectionId, null, userId, null, appointmentId, trimmed, Snapshot(target), null, ct);
            return (false, "already-pending", null);
        }
        await AuditRevokeAsync(Constants.Elections.EmergencyRevokeOutcomes.Requested, target.ElectionId, approval.Id, userId, null, appointmentId, trimmed, Snapshot(target), null, ct);

        return (true, null, (await ToDtos(db.ElectionApprovals.Where(x => x.Id == approval.Id), ct)).Single());
    }

    public async Task<(bool Success, string? Error)> ApproveAsync(int approvalId, int userId, IReadOnlyCollection<string> roles, CancellationToken ct = default)
    {
        var approval = await db.ElectionApprovals.AsNoTracking().FirstOrDefaultAsync(x => x.Id == approvalId, ct);
        if (approval is null)
            return (false, "not-found");
        if (approval.Action == ElectionApprovalAction.EmergencyRevoke)
            return await ApproveEmergencyRevokeAsync(approval, userId, ct);
        var now = DateTime.UtcNow;
        if (approval.ExecutedAt != null || approval.RejectedAt != null)
            return (false, "closed");
        if (approval.ExpiresAt <= now)
            return (false, "expired");
        if (approval.RequestedByUserId == userId)
            return (false, "same-person");
        if (!await access.HasAsync(approval.ElectionId, userId, roles, ElectionPermission.Approve, ct))
            return (false, "forbidden");

        // Before the transaction, so the refused-change audit row is not rolled back with it.
        await EnsureKeyNotFrozenAsync(approval.ElectionId, approval.Action, approval.PayloadJson, ct);

        // The row is claimed with a conditional update, so two approvers at once cannot both run it.
        // The step runs in the same transaction, and a failed step rolls the claim back.
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        // An approved count still blocks a new request until it runs, so it keeps its key.
        var keyAfter = approval.Action == ElectionApprovalAction.Count ? approval.OpenKey : null;
        var claimed = await OpenAt(now).Where(x => x.Id == approvalId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.ApprovedByUserId, userId)
                .SetProperty(x => x.ApprovedAt, now)
                .SetProperty(x => x.ExecutedAt, now)
                .SetProperty(x => x.OpenKey, keyAfter), ct);
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
        if (approval.Action == ElectionApprovalAction.EmergencyRevoke)
        {
            // The requester may withdraw it. Otherwise only someone who could approve it may say no,
            // so the official being removed cannot close their own removal.
            if (approval.RequestedByUserId != userId && !await MayDecideRevokeAsync(approval, userId, now, ct))
                return (false, "not-eligible-approver");
        }
        else if (!await access.HasAsync(approval.ElectionId, userId, roles, ElectionPermission.Approve, ct))
            return (false, "forbidden");

        // Conditional like the approve claim, so a reject cannot land on a step that has just run.
        var trimmed = reason?.Trim();
        var rejectReason = string.IsNullOrEmpty(trimmed) ? null : trimmed[..Math.Min(trimmed.Length, 500)];
        var rejected = await OpenAt(now).Where(x => x.Id == approvalId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.RejectedByUserId, userId)
                .SetProperty(x => x.RejectedAt, now)
                .SetProperty(x => x.RejectReason, rejectReason)
                .SetProperty(x => x.OpenKey, (string?)null), ct);
        if (rejected != 1)
            return (false, "closed");
        if (approval.Action == ElectionApprovalAction.EmergencyRevoke)
        {
            var payload = ReadRevoke(approval.PayloadJson);
            await AuditRevokeAsync(Constants.Elections.EmergencyRevokeOutcomes.Rejected, approval.ElectionId, approvalId, approval.RequestedByUserId, userId, payload?.AppointmentId, payload?.Reason, null, null, ct);
        }
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
        if (approved.RequestedByUserId != userId && (await orgConfig.GetConfigAsync()).Elections.CountRequesterOnly)
            return new ElectionCountRunResult(null, "not-requester", null);

        // The count opens its own transaction, so the claim cannot share it. A count that fails
        // gives the approval back, so a wrong key file does not use it up.
        var claimed = await ReadyToCountAt(now).Where(x => x.Id == approved.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ConsumedAt, now).SetProperty(x => x.ExecutedByUserId, userId), ct);
        if (claimed != 1)
            return new ElectionCountRunResult(null, "closed", null);

        var (counted, countError) = await elections.CountAsync(electionId, privateKeyPkcs8Base64, ct);
        if (counted is null)
        {
            await db.ElectionApprovals.Where(x => x.Id == approved.Id && x.ConsumedAt == now)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.ConsumedAt, (DateTime?)null).SetProperty(x => x.ExecutedByUserId, (int?)null), CancellationToken.None);
            return new ElectionCountRunResult(null, countError, null);
        }
        // A key left behind is freed by the next request for it, so a failure here is only logged.
        try
        {
            await db.ElectionApprovals.Where(x => x.Id == approved.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.OpenKey, (string?)null), CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Election {ElectionId}: count ran under approval {ApprovalId} but its key was not cleared.", electionId, approved.Id);
        }
        // The results are already stored. A failed audit write must not turn that into an error the client retries.
        try
        {
            await activity.LogActivityAsync(null, Constants.Elections.CountRunAuditType,
                $"Election {electionId} counted under approval {approved.Id}.", userId,
                source: Constants.ActivitySources.System,
                metadata: JsonSerializer.Serialize(new { electionId, approvalId = approved.Id, requesterId = approved.RequestedByUserId, approverId = approved.ApprovedByUserId, executorId = userId }),
                cancellationToken: CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Election {ElectionId}: count ran under approval {ApprovalId} but its audit row was not written.", electionId, approved.Id);
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

    // Checked again here because the request may be hours old. Only a live official on the
    // election may approve, never a SuperAdmin acting on the role alone.
    private async Task<(bool Success, string? Error)> ApproveEmergencyRevokeAsync(ElectionApproval approval, int userId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var payload = ReadRevoke(approval.PayloadJson);
        ElectionAppointment? target = null;
        string? error = null;
        if (payload is null)
            error = "not-ready";
        else if (approval.ExecutedAt != null || approval.RejectedAt != null)
            error = "closed";
        else if (approval.ExpiresAt <= now)
            error = "expired";
        else if (approval.RequestedByUserId == userId)
            error = "same-person";
        else
        {
            target = await db.ElectionAppointments.AsNoTracking().Where(ElectionAppointment.LiveAt(now)).FirstOrDefaultAsync(x => x.Id == payload.AppointmentId, ct);
            if (target is null)
                error = "target-not-live";
            else if (!await EligibleApprovers(approval.ElectionId, approval.RequestedByUserId, target.UserId, now).AnyAsync(x => x.UserId == userId, ct))
                error = "not-eligible-approver";
        }

        if (error == "expired")
        {
            // The sweep may have written the expired row already. Only the claim that wins writes it.
            await AuditExpiredAsync(approval, ct);
            return (false, error);
        }
        if (error != null)
        {
            await AuditRevokeAsync(Constants.Elections.EmergencyRevokeOutcomes.RefusedPrefix + error, approval.ElectionId, approval.Id, approval.RequestedByUserId, userId, payload?.AppointmentId, payload?.Reason, Snapshot(target), null, ct);
            return (false, error);
        }

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var claimed = await OpenAt(now).Where(x => x.Id == approval.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.ApprovedByUserId, userId)
                .SetProperty(x => x.ApprovedAt, now)
                .SetProperty(x => x.ExecutedAt, now)
                .SetProperty(x => x.OpenKey, (string?)null), ct);
        if (claimed != 1)
            return (false, "closed");

        // Conditional, so an ordinary revoke that landed after the checks above is not written over.
        var revoked = await db.ElectionAppointments.Where(x => x.Id == target!.Id && x.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.RevokedAt, now)
                .SetProperty(x => x.RevokedByUserId, approval.RequestedByUserId)
                .SetProperty(x => x.RevokedReason, payload!.Reason), ct);
        if (revoked != 1)
            return (false, "target-not-live");
        var before = Snapshot(target);
        target!.RevokedAt = now;
        target.RevokedByUserId = approval.RequestedByUserId;
        target.RevokedReason = payload!.Reason;
        await appointments.EndAccessIfNoneLeftAsync(target.UserId, ct);
        await AuditRevokeAsync(Constants.Elections.EmergencyRevokeOutcomes.Revoked, approval.ElectionId, approval.Id, approval.RequestedByUserId, userId, target.Id, payload.Reason, before, Snapshot(target), ct);
        await transaction.CommitAsync(ct);
        logger.LogWarning("Election {ElectionId}: appointment {AppointmentId} revoked in an emergency, asked by {RequestedBy}, approved by {ApprovedBy}.", approval.ElectionId, target.Id, approval.RequestedByUserId, userId);
        return (true, null);
    }

    private async Task<bool> MayDecideRevokeAsync(ElectionApproval approval, int userId, DateTime now, CancellationToken ct)
    {
        var payload = ReadRevoke(approval.PayloadJson);
        if (payload is null)
            return false;
        var targetUserId = await db.ElectionAppointments.Where(x => x.Id == payload.AppointmentId).Select(x => (int?)x.UserId).FirstOrDefaultAsync(ct);
        return targetUserId is not null
            && await EligibleApprovers(approval.ElectionId, approval.RequestedByUserId, targetUserId.Value, now).AnyAsync(x => x.UserId == userId, ct);
    }

    // Live officials on the election whose active persona grants Approve, leaving out the requester and the target.
    private IQueryable<ElectionAppointment> EligibleApprovers(int electionId, int requesterId, int targetUserId, DateTime now) =>
        db.ElectionAppointments.Where(ElectionAppointment.LiveAt(now)).Where(ElectionAppointment.GrantsApprove)
            .Where(x => x.ElectionId == electionId && x.UserId != requesterId && x.UserId != targetUserId);

    private async Task<bool> RevokePendingAsync(int electionId, int appointmentId, DateTime now, CancellationToken ct)
    {
        var key = ElectionApproval.KeyFor(electionId, ElectionApprovalAction.EmergencyRevoke, appointmentId);
        await ReleaseKeyAsync(key, now, ct);
        return await Blocking(now).AnyAsync(x => x.OpenKey == key, ct);
    }

    public async Task<int> AuditExpiredRevokesAsync(CancellationToken ct = default)
    {
        var expired = await ExpiredRevokesAt(DateTime.UtcNow).AsNoTracking().ToListAsync(ct);
        var written = 0;
        foreach (var approval in expired)
        {
            // One bad row must not stop the rest. Its claim was given back, so the next sweep retries it.
            try
            {
                if (await AuditExpiredAsync(approval, ct))
                    written++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Approval {ApprovalId}: expired emergency revoke was not audited.", approval.Id);
            }
        }
        return written;
    }

    // Frees a key held by a row that no longer blocks, so a new request can take it. An expired
    // emergency revoke still owes its expired row, so it is audited here rather than just cleared.
    private async Task ReleaseKeyAsync(string key, DateTime now, CancellationToken ct)
    {
        foreach (var approval in await ExpiredRevokesAt(now).Where(x => x.OpenKey == key).AsNoTracking().ToListAsync(ct))
            await AuditExpiredAsync(approval, ct);
        await db.ElectionApprovals.Where(x => x.OpenKey == key)
            .Where(x => !(x.ExecutedAt == null && x.RejectedAt == null && x.ExpiresAt > now))
            .Where(x => !(x.Action == ElectionApprovalAction.Count && x.ApprovedAt != null && x.ConsumedAt == null && x.RejectedAt == null && x.ExpiresAt > now))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.OpenKey, (string?)null), ct);
    }

    // Clearing the key is the claim, so the sweep and an approver trying the same row write one row between them.
    private async Task<bool> AuditExpiredAsync(ElectionApproval approval, CancellationToken ct)
    {
        var claimed = await db.ElectionApprovals.Where(x => x.Id == approval.Id && x.OpenKey != null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.OpenKey, (string?)null), ct);
        if (claimed != 1)
            return false;
        var payload = ReadRevoke(approval.PayloadJson);
        try
        {
            await AuditRevokeAsync(Constants.Elections.EmergencyRevokeOutcomes.Expired, approval.ElectionId, approval.Id, approval.RequestedByUserId, null, payload?.AppointmentId, payload?.Reason, null, null, ct, approval.ExpiresAt);
            return true;
        }
        catch
        {
            // Give the claim back so the next sweep tries again.
            await db.ElectionApprovals.Where(x => x.Id == approval.Id && x.OpenKey == null)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.OpenKey, approval.OpenKey), CancellationToken.None);
            throw;
        }
    }

    // The unique index on OpenKey refuses the second of two requests sent at once.
    private async Task<bool> TryInsertAsync(ElectionApproval approval, CancellationToken ct)
    {
        db.ElectionApprovals.Add(approval);
        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException) when (approval.OpenKey != null)
        {
            db.Entry(approval).State = EntityState.Detached;
            if (await db.ElectionApprovals.AnyAsync(x => x.OpenKey == approval.OpenKey, ct))
                return false;
            throw;
        }
    }

    // A payload that will not parse still gets its audit row, just without the appointment and reason.
    private static RevokePayload? ReadRevoke(string? payloadJson)
    {
        if (string.IsNullOrEmpty(payloadJson))
            return null;
        try
        {
            return JsonSerializer.Deserialize<RevokePayload>(payloadJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static object? Snapshot(ElectionAppointment? a) =>
        a is null ? null : new { a.Id, a.UserId, a.PersonaId, a.IsReturningOfficer, a.AcceptedAt, a.RevokedAt, a.RevokedByUserId, a.RevokedReason, a.ExpiresAt };

    private async Task AuditRevokeAsync(string outcome, int? electionId, int? approvalId, int requesterId, int? approverId, int? appointmentId, string? reason, object? before, object? after, CancellationToken ct, DateTime? expiredAt = null)
    {
        await activity.LogActivityAsync(null, Constants.Elections.EmergencyRevokeAuditType,
            $"Emergency revocation of appointment {appointmentId}: {outcome}.", approverId ?? requesterId,
            source: Constants.ActivitySources.System,
            metadata: JsonSerializer.Serialize(new { outcome, electionId, approvalId, requesterId, approverId, appointmentId, reason, before, after, expiredAt }),
            cancellationToken: ct);
    }

    private async Task<bool> NeedsSecondAsync(ElectionApprovalAction action, IReadOnlyCollection<string> roles)
    {
        var settings = (await orgConfig.GetConfigAsync()).Elections;
        return settings.TwoPersonActions.Contains(action.ToString(), StringComparer.OrdinalIgnoreCase)
            && !(settings.SuperAdminActsAlone && roles.Contains(Constants.Roles.SuperAdmin));
    }

    private IQueryable<ElectionApproval> OpenAt(DateTime now) =>
        db.ElectionApprovals.Where(x => x.ExecutedAt == null && x.RejectedAt == null && x.ExpiresAt > now);

    // Rows that stop a new request for the same step.
    private IQueryable<ElectionApproval> Blocking(DateTime now) => OpenAt(now).Union(ReadyToCountAt(now));

    private IQueryable<ElectionApproval> ExpiredRevokesAt(DateTime now) =>
        db.ElectionApprovals.Where(x => x.Action == ElectionApprovalAction.EmergencyRevoke && x.OpenKey != null
            && x.ExecutedAt == null && x.RejectedAt == null && x.ExpiresAt <= now);

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
            r.Approval.Action == ElectionApprovalAction.Count ? r.Approval.ApprovedAt : null,
            r.Approval.ExecutedByUserId)).ToList();
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

    // 37.13v. The ballot key is security configuration, so it is locked with the other rules while
    // the election waits for polling to open. The phase check in SetBallotKeyAsync covers later on.
    private Task EnsureKeyNotFrozenAsync(int electionId, ElectionApprovalAction action, string? payloadJson, CancellationToken ct) =>
        action == ElectionApprovalAction.ReplaceBallotKey
            ? freeze.EnsureNotFrozenAsync(Constants.Elections.FrozenRules.BallotKey, new { action = "ballot-key", ReadKey(payloadJson)?.Fingerprint }, electionId, ct)
            : Task.CompletedTask;

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
            case ElectionApprovalAction.EmergencyRevoke:
                // Runs only through ApproveEmergencyRevokeAsync.
                return "not-ready";
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
