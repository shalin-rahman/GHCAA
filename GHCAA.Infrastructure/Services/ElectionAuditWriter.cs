using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GHCAA.Infrastructure.Services;

public sealed class ElectionAuditWriter(ApplicationDbContext db) : IElectionAuditWriter
{
    public async Task AppendAsync(int electionId, string action, int? actorUserId, object? detail, CancellationToken ct = default)
    {
        var (prevHash, seq) = await TailAsync(electionId, ct);
        db.ElectionAuditEntries.Add(Link(electionId, seq + 1, prevHash, action, actorUserId, JsonSerializer.Serialize(detail ?? new { })));
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        // Inside a caller's transaction Postgres aborts it on the clash, so a retry cannot work there.
        catch (DbUpdateException) when (db.Database.CurrentTransaction == null
            && db.ChangeTracker.Entries<ElectionAuditEntry>().Any(x => x.State == EntityState.Added))
        {
            await RenumberPendingAsync(ct);
            await db.SaveChangesAsync(ct);
        }
    }

    // A clash means someone else saved entries first. The pending ones are linked again after
    // the new tail. Their order is kept.
    private async Task RenumberPendingAsync(CancellationToken ct)
    {
        var pending = db.ChangeTracker.Entries<ElectionAuditEntry>().Where(x => x.State == EntityState.Added)
            .Select(x => x.Entity).OrderBy(x => x.ElectionId).ThenBy(x => x.Seq).ToList();
        foreach (var group in pending.GroupBy(x => x.ElectionId))
        {
            var last = await db.ElectionAuditEntries.AsNoTracking().Where(x => x.ElectionId == group.Key)
                .OrderByDescending(x => x.Seq).Select(x => new { x.Seq, x.Hash }).FirstOrDefaultAsync(ct);
            var seq = last?.Seq ?? 0;
            var prev = last?.Hash ?? GHCAA.Domain.Constants.Elections.AuditGenesisHash;
            foreach (var entry in group)
            {
                entry.Seq = ++seq;
                entry.PrevHash = prev;
                entry.Hash = ComputeHash(prev, entry.ElectionId, entry.Seq, entry.At, entry.ActorUserId, entry.Action, entry.DetailJson);
                prev = entry.Hash;
            }
        }
    }

    public async Task<ElectionAuditVerifyDto> VerifyAsync(int electionId, CancellationToken ct = default)
    {
        var rows = await db.ElectionAuditEntries.AsNoTracking().Where(x => x.ElectionId == electionId).OrderBy(x => x.Seq).ToListAsync(ct);
        var prev = GHCAA.Domain.Constants.Elections.AuditGenesisHash;
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var at = DateTime.SpecifyKind(row.At, DateTimeKind.Utc);
            // A gap in Seq means a row was removed, so the entry after the gap is the first that fails.
            if (row.Seq != i + 1 || row.PrevHash != prev || row.Hash != ComputeHash(prev, row.ElectionId, row.Seq, at, row.ActorUserId, row.Action, row.DetailJson))
                return new ElectionAuditVerifyDto(false, rows.Count, row.Seq);
            prev = row.Hash;
        }
        return new ElectionAuditVerifyDto(true, rows.Count, null);
    }

    public async Task<IReadOnlyList<ElectionAuditEntryDto>> ListAsync(int electionId, CancellationToken ct = default) =>
        await db.ElectionAuditEntries.AsNoTracking().Where(x => x.ElectionId == electionId).OrderBy(x => x.Seq)
            .Select(x => new ElectionAuditEntryDto(x.Seq, x.At, x.ActorUserId, x.Action, x.DetailJson, x.Hash)).ToListAsync(ct);

    // Entries added earlier in the same unit of work are not in the database yet, so the local
    // ones count first.
    private async Task<(string Hash, int Seq)> TailAsync(int electionId, CancellationToken ct)
    {
        var local = db.ChangeTracker.Entries<ElectionAuditEntry>()
            .Where(x => x.State == EntityState.Added && x.Entity.ElectionId == electionId)
            .Select(x => x.Entity).OrderByDescending(x => x.Seq).FirstOrDefault();
        if (local != null) return (local.Hash, local.Seq);
        var last = await db.ElectionAuditEntries.AsNoTracking().Where(x => x.ElectionId == electionId)
            .OrderByDescending(x => x.Seq).Select(x => new { x.Seq, x.Hash }).FirstOrDefaultAsync(ct);
        return last == null ? (GHCAA.Domain.Constants.Elections.AuditGenesisHash, 0) : (last.Hash, last.Seq);
    }

    private static ElectionAuditEntry Link(int electionId, int seq, string prevHash, string action, int? actorUserId, string detailJson)
    {
        // Postgres keeps microseconds, so the hash must be taken over the value it will give back.
        var now = DateTime.UtcNow;
        var at = new DateTime(now.Ticks - now.Ticks % 10, DateTimeKind.Utc);
        return new ElectionAuditEntry
        {
            ElectionId = electionId,
            Seq = seq,
            At = at,
            ActorUserId = actorUserId,
            Action = action,
            DetailJson = detailJson,
            PrevHash = prevHash,
            Hash = ComputeHash(prevHash, electionId, seq, at, actorUserId, action, detailJson),
        };
    }

    public static string ComputeHash(string prevHash, int electionId, int seq, DateTime at, int? actorUserId, string action, string detailJson) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes($"{prevHash}|{electionId}|{seq}|{at:O}|{actorUserId}|{action}|{detailJson}")));
}
