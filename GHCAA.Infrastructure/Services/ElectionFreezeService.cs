using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services;

public sealed class ElectionFreezeService(ApplicationDbContext db, IHttpContextAccessor http, IActivityService activity) : IElectionFreezeService
{
    public async Task EnsureNotFrozenAsync(string rule, object? attempted, int? electionId = null, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var frozen = await FindFrozenAsync(electionId, ct);
        if (frozen is null) return;

        int? actorId = int.TryParse(http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
        var unlock = Constants.Elections.FrozenRules.NeverUnlocked.Contains(rule)
            ? null
            : await OpenUnlocks(now).OrderBy(u => u.Id).FirstOrDefaultAsync(ct);
        if (unlock is not null)
        {
            // Logged before the change is saved, so a change that then fails its own checks
            // still leaves this row. It records the attempt, not that the change landed.
            await activity.LogActivityAsync(null, Constants.Elections.FrozenChangeUnlockedAuditType,
                $"Unlock {unlock.Id} allowed an attempt to change {rule} while \"{frozen.Title}\" is frozen. Reason: {unlock.Reason}", actorId,
                source: Constants.ActivitySources.System,
                metadata: JsonSerializer.Serialize(new { rule, electionId = frozen.Id, attempted, unlockId = unlock.Id, unlockReason = unlock.Reason }),
                cancellationToken: ct, timestamp: now);
            return;
        }

        await activity.LogActivityAsync(null, Constants.Elections.FrozenChangeRefusedAuditType,
            $"Refused a change to {rule} while \"{frozen.Title}\" is frozen.", actorId,
            source: Constants.ActivitySources.System,
            metadata: JsonSerializer.Serialize(new { rule, electionId = frozen.Id, attempted }),
            cancellationToken: ct, timestamp: now);
        throw new ElectionRulesFrozenException(frozen.Id, frozen.Title);
    }

    public async Task<FrozenElection?> FindFrozenAsync(int? electionId = null, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        // Frozen from the moment someone asks to open polling, not only once it opens, so the
        // rules cannot be changed while the second person is still deciding.
        return await db.Elections.AsNoTracking()
            .Where(e => electionId == null || e.Id == electionId)
            .Where(e => e.Phase == ElectionPhase.Polling || e.Phase == ElectionPhase.Counting
                || db.ElectionApprovals.Any(a => a.ElectionId == e.Id
                    && a.Action == ElectionApprovalAction.OpenPolling
                    && a.ExecutedAt == null && a.RejectedAt == null && a.ExpiresAt > now))
            .OrderBy(e => e.Id)
            .Select(e => new FrozenElection(e.Id, e.Title))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ElectionRulesUnlockDto?> GetOpenUnlockAsync(CancellationToken ct = default) =>
        await OpenUnlocks(DateTime.UtcNow).OrderBy(u => u.Id)
            .Select(u => new ElectionRulesUnlockDto(u.Id, u.OpenedByUserId,
                db.Users.Where(x => x.Id == u.OpenedByUserId).Select(x => x.Username).FirstOrDefault() ?? string.Empty,
                u.Reason, u.OpenedAt, u.ExpiresAt))
            .FirstOrDefaultAsync(ct);

    public async Task<(string? Error, ElectionRulesUnlockDto? Unlock)> OpenUnlockAsync(int userId, string? reason, int? minutes, CancellationToken ct = default)
    {
        reason = reason?.Trim() ?? string.Empty;
        if (reason.Length < Constants.Elections.RulesUnlockReasonMinLength) return (Constants.Elections.RulesUnlockErrors.ReasonTooShort, null);
        if (reason.Length > Constants.Elections.RulesUnlockReasonMaxLength) return (Constants.Elections.RulesUnlockErrors.ReasonTooLong, null);
        var length = minutes ?? Constants.Elections.RulesUnlockDefaultMinutes;
        if (length < 1 || length > Constants.Elections.RulesUnlockMaxMinutes) return (Constants.Elections.RulesUnlockErrors.InvalidMinutes, null);

        // Nothing to unlock when no election is frozen, and one unlock at a time keeps the log
        // readable. Two opened at the same instant could both land. Both are logged, and close
        // shuts every open one, so neither can outlive a close.
        var frozen = await FindFrozenAsync(ct: ct);
        if (frozen is null) return (Constants.Elections.RulesUnlockErrors.NotFrozen, null);
        var now = DateTime.UtcNow;
        if (await OpenUnlocks(now).AnyAsync(ct)) return (Constants.Elections.RulesUnlockErrors.AlreadyOpen, null);

        var unlock = new ElectionRulesUnlock { OpenedByUserId = userId, Reason = reason, OpenedAt = now, ExpiresAt = now.AddMinutes(length) };
        db.ElectionRulesUnlocks.Add(unlock);
        await db.SaveChangesAsync(ct);
        await activity.LogActivityAsync(null, Constants.Elections.RulesUnlockOpenedAuditType,
            $"Unlock {unlock.Id} opened for {length} minutes while \"{frozen.Title}\" is frozen. Reason: {reason}", userId,
            source: Constants.ActivitySources.System,
            metadata: JsonSerializer.Serialize(new { unlockId = unlock.Id, reason, unlock.ExpiresAt, electionId = frozen.Id }),
            cancellationToken: ct, timestamp: now);
        return (null, await GetOpenUnlockAsync(ct));
    }

    public async Task<string?> CloseUnlockAsync(int userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var open = await OpenUnlocks(now).OrderBy(u => u.Id).Select(u => new { u.Id, u.Reason }).ToListAsync(ct);
        if (open.Count == 0) return Constants.Elections.RulesUnlockErrors.NotOpen;
        var ids = open.Select(u => u.Id).ToList();
        var closed = await db.ElectionRulesUnlocks.Where(u => ids.Contains(u.Id) && u.ClosedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.ClosedAt, now).SetProperty(u => u.ClosedByUserId, userId), ct);
        if (closed == 0) return Constants.Elections.RulesUnlockErrors.NotOpen;
        foreach (var unlock in open)
            await activity.LogActivityAsync(null, Constants.Elections.RulesUnlockClosedAuditType,
                $"Unlock {unlock.Id} closed early.", userId,
                source: Constants.ActivitySources.System,
                metadata: JsonSerializer.Serialize(new { unlockId = unlock.Id, unlock.Reason }),
                cancellationToken: ct, timestamp: now);
        return null;
    }

    private IQueryable<ElectionRulesUnlock> OpenUnlocks(DateTime now) =>
        db.ElectionRulesUnlocks.AsNoTracking().Where(u => u.ClosedAt == null && u.ExpiresAt > now);
}
