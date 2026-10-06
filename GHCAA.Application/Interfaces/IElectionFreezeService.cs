using System;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces;

// Spec 023 FR-039 (37.13q). Rules are locked while any election waits for polling to open, is
// polling, or is counting:
//   locked = EXISTS election WHERE status IN (WAITING_FOR_POLLING, POLLING, COUNTING)
// Locked: election settings, ballot and count rules, personas and their permissions, appointments,
// and the ballot key. Not locked: each election's own data (it follows its phase), the emergency
// revoke (two-person path), audit viewing and read-only monitoring.
// A SuperAdmin can open a short unlock with a reason (37.13v). It never opens a plain revoke.
public interface IElectionFreezeService
{
    // While an unlock is open, logs the change with the unlock id and reason and returns.
    // Otherwise saves an audit row for the refused attempt, then throws ElectionRulesFrozenException.
    // A null electionId checks every election, for site-wide rules like settings and personas.
    // Call it before changing anything, because it saves the shared DbContext.
    Task EnsureNotFrozenAsync(string rule, object? attempted, int? electionId = null, CancellationToken ct = default);

    // The first frozen election, or null. Writes nothing; for callers that only flag a change.
    Task<FrozenElection?> FindFrozenAsync(int? electionId = null, CancellationToken ct = default);

    Task<ElectionRulesUnlockDto?> GetOpenUnlockAsync(CancellationToken ct = default);
    // Errors: reason-too-short, reason-too-long, invalid-minutes, not-frozen, already-open.
    Task<(string? Error, ElectionRulesUnlockDto? Unlock)> OpenUnlockAsync(int userId, string? reason, int? minutes, CancellationToken ct = default);
    // Error: not-open.
    Task<string?> CloseUnlockAsync(int userId, CancellationToken ct = default);
}

public sealed record FrozenElection(int Id, string Title);

public sealed class ElectionRulesFrozenException(int electionId, string electionTitle)
    : InvalidOperationException($"Election rules are locked while \"{electionTitle}\" is polling or counting. They open again once the result is declared.")
{
    public int ElectionId { get; } = electionId;
}
