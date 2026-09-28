using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GHCAA.Domain.Constants;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services;

public sealed class ElectionService(ApplicationDbContext db, ILogger<ElectionService>? logger = null) : IElectionService
{
    private readonly ApplicationDbContext _db = db;
    private readonly ILogger<ElectionService>? _logger = logger;

    public async Task<ElectionSummaryDto> CreateAsync(CreateElectionDto request, CancellationToken ct = default)
    {
        var election = new Election
        {
            Title = request.Title,
            ECPeriodId = request.ECPeriodId,
            CreatedBy = request.CreatedBy,
            AnnouncedOn = DateTime.UtcNow,
            NominationOpensOn = request.NominationOpensOn.ToUniversalTime(),
            NominationClosesOn = request.NominationClosesOn.ToUniversalTime(),
            PollingOpensOn = request.PollingOpensOn.ToUniversalTime(),
            PollingClosesOn = request.PollingClosesOn.ToUniversalTime(),
            TieRule = request.TieRule
        };
        _db.Elections.Add(election);
        await _db.SaveChangesAsync(ct);
        return ToSummary(election, 0, 0);
    }

    public async Task<ElectionSummaryDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Elections.Include(x => x.VoterRoll).FirstOrDefaultAsync(x => x.Id == id, ct);
        return e == null ? null : ToSummary(e, e.VoterRoll.Count, e.VoterRoll.Count(x => x.IsEligible));
    }

    public async Task<ElectionSummaryDto?> GetCurrentAsync(CancellationToken ct = default)
    {
        var e = await _db.Elections
            .Include(x => x.VoterRoll)
            .Where(x => x.Phase != ElectionPhase.Announced && x.Phase != ElectionPhase.Declared && x.Phase != ElectionPhase.Archived)
            .OrderByDescending(x => x.AnnouncedOn)
            .FirstOrDefaultAsync(ct);
        return e == null ? null : ToSummary(e, e.VoterRoll.Count, e.VoterRoll.Count(x => x.IsEligible));
    }

    public async Task<int> AddSeatAsync(int id, ElectionSeatRequestDto request, CancellationToken ct = default)
    {
        var election = await _db.Elections.FindAsync([id], ct);
        if (election == null) throw new InvalidOperationException("Election was not found.");
        if (election.Phase != ElectionPhase.Announced) throw new InvalidOperationException("Seats cannot be changed after nominations open.");
        var seat = new ElectionSeat { ElectionId = id, Position = request.Position, SeatCount = request.SeatCount };
        _db.ElectionSeats.Add(seat);
        await _db.SaveChangesAsync(ct);
        return seat.Id;
    }

    public async Task<bool> AssignOfficerAsync(int id, ElectionOfficerDto request, CancellationToken ct = default)
    {
        if (!await _db.Elections.AnyAsync(x => x.Id == id, ct) || !await _db.Members.AnyAsync(x => x.Id == request.MemberId && x.Status == MembershipStatus.Active, ct)) return false;
        if (await _db.ElectionOfficers.AnyAsync(x => x.ElectionId == id && x.MemberId == request.MemberId && x.Role == request.Role, ct)) return false;
        _db.ElectionOfficers.Add(new ElectionOfficer { ElectionId = id, MemberId = request.MemberId, Role = request.Role });
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> SetPhaseAsync(int id, ElectionPhase phase, CancellationToken ct = default)
    {
        var e = await _db.Elections.FindAsync([id], ct);
        if (e == null || phase == e.Phase || phase < e.Phase || phase > ElectionPhase.Archived || (int)phase > (int)e.Phase + 1) return false;
        if (phase == ElectionPhase.Polling && !await _db.VoterRolls.AnyAsync(x => x.ElectionId == id, ct)) return false;
        // Spec 023 FR-001: no ballot may be cast before there is a key to seal it under.
        if (phase == ElectionPhase.Polling && string.IsNullOrEmpty(e.BallotPublicKey)) return false;
        if (phase == ElectionPhase.Counting && DateTime.UtcNow < e.PollingClosesOn) return false;
        // Spec 023 FR-008: only DeclareAsync may declare, so the ECMember rows are always written.
        if (phase == ElectionPhase.Declared) return false;
        e.Phase = phase;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<(bool Success, string? Error, string? Fingerprint)> SetBallotKeyAsync(int id, string publicKeySpkiBase64, CancellationToken ct = default)
    {
        var e = await _db.Elections.FindAsync([id], ct);
        if (e == null) return (false, "not-found", null);
        // Once polling opens, ballots are sealed under the stored key, so it cannot change.
        if (e.Phase >= ElectionPhase.Polling) return (false, "phase-closed", null);
        var key = publicKeySpkiBase64?.Trim();
        var fingerprint = string.IsNullOrEmpty(key) ? null : BallotSeal.Fingerprint(key);
        if (fingerprint == null) return (false, "invalid-key", null);
        // The phase is checked again in the update itself, so a key cannot land after polling
        // opened between the read above and this write.
        var updated = await _db.Elections.Where(x => x.Id == id && x.Phase < ElectionPhase.Polling)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.BallotPublicKey, key).SetProperty(x => x.BallotKeyFingerprint, fingerprint), ct);
        _db.Entry(e).State = EntityState.Detached;
        return updated == 1 ? (true, null, fingerprint) : (false, "phase-closed", null);
    }

    public async Task<int> FreezeVoterRollAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Elections.Include(x => x.VoterRoll).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new InvalidOperationException("Election was not found.");
        if (e.Phase > ElectionPhase.Nomination) throw new InvalidOperationException("The voter roll cannot be frozen after nominations open.");
        if (e.VoterRoll.Count != 0) return e.VoterRoll.Count;
        var now = DateTime.UtcNow;
        var members = await _db.Members.Where(m => !m.IsArchived && m.Status == MembershipStatus.Active &&
            (m.MembershipType == MembershipType.Founding || m.MembershipType == MembershipType.Executive || m.MembershipType == MembershipType.General)).ToListAsync(ct);
        var ids = members.Select(m => m.Id).ToHashSet();
        var unpaid = await _db.MembershipDues.Where(d => ids.Contains(d.MemberId) && !d.IsPaid && d.DueDate <= now)
            .Select(d => d.MemberId).Distinct().ToListAsync(ct);
        foreach (var m in members)
            e.VoterRoll.Add(new VoterRoll { MemberId = m.Id, IsEligible = !unpaid.Contains(m.Id), IneligibilityReason = unpaid.Contains(m.Id) ? "Membership dues are overdue." : null, FrozenAt = now });
        await _db.SaveChangesAsync(ct);
        return e.VoterRoll.Count;
    }

    public async Task<NominationViewDto> SubmitNominationAsync(int id, NominationDto request, CancellationToken ct = default)
    {
        var e = await _db.Elections.Include(x => x.VoterRoll).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new InvalidOperationException("Election was not found.");
        if (e.Phase != ElectionPhase.Nomination || DateTime.UtcNow > e.NominationClosesOn) throw new InvalidOperationException("Nominations are closed.");
        var seatExists = await _db.ElectionSeats.AnyAsync(x => x.Id == request.ElectionSeatId && x.ElectionId == id, ct);
        if (!seatExists) throw new InvalidOperationException("The election seat was not found.");
        var roll = e.VoterRoll.Where(x => x.IsEligible).Select(x => x.MemberId).ToHashSet();
        if (!roll.Contains(request.CandidateMemberId)) throw new InvalidOperationException("The candidate must be an eligible voter.");
        if (!roll.Contains(request.ProposerMemberId) || !roll.Contains(request.SeconderMemberId)) throw new InvalidOperationException("Proposer and seconder must be eligible voters.");
        if (request.CandidateMemberId == request.ProposerMemberId || request.CandidateMemberId == request.SeconderMemberId) throw new InvalidOperationException("Candidate cannot propose or second their own nomination.");
        var n = new Nomination { ElectionId = id, ElectionSeatId = request.ElectionSeatId, CandidateMemberId = request.CandidateMemberId, ProposerMemberId = request.ProposerMemberId, SeconderMemberId = request.SeconderMemberId, Statement = request.Statement, PhotoPath = request.PhotoPath, SubmittedAt = DateTime.UtcNow };
        _db.Nominations.Add(n);
        await _db.SaveChangesAsync(ct);
        return ToNomination(n);
    }

    public async Task<bool> DecideNominationAsync(int nominationId, int officerMemberId, ScrutinyDto request, CancellationToken ct = default)
    {
        var n = await _db.Nominations.Include(x => x.Election).FirstOrDefaultAsync(x => x.Id == nominationId, ct);
        if (n == null || n.Status is NominationStatus.Withdrawn) return false;
        if (n.Election?.Phase != ElectionPhase.Scrutiny) return false;
        if (!await _db.ElectionOfficers.AnyAsync(x => x.ElectionId == n.ElectionId && x.MemberId == officerMemberId &&
            (x.Role == ElectionRole.ReturningOfficer || x.Role == ElectionRole.AssistantReturningOfficer || x.Role == ElectionRole.Scrutineer), ct))
            return false;
        n.Status = request.Accepted ? NominationStatus.Accepted : NominationStatus.Rejected;
        _db.ScrutinyDecisions.Add(new ScrutinyDecision { NominationId = nominationId, OfficerMemberId = officerMemberId, Accepted = request.Accepted, Reason = request.Reason, DecidedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> WithdrawNominationAsync(int nominationId, int memberId, CancellationToken ct = default)
    {
        var n = await _db.Nominations.Include(x => x.Election).FirstOrDefaultAsync(x => x.Id == nominationId, ct);
        if (n == null || n.Status != NominationStatus.Accepted || n.Election?.Phase != ElectionPhase.Withdrawal ||
            n.CandidateMemberId != memberId) return false;
        n.Status = NominationStatus.Withdrawn; n.WithdrawnAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<(bool Success, string? Error, string? TrackingCode)> CastBallotAsync(int id, int memberId, CastBallotDto request, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        // Spec 023 FR-004. The vote rows keep the date only, so they carry no order to match.
        var votedOn = now.Date;
        var e = await _db.Elections.Include(x => x.Seats).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e == null || e.Phase != ElectionPhase.Polling || now < e.PollingOpensOn || now > e.PollingClosesOn || string.IsNullOrEmpty(e.BallotPublicKey)) return (false, "not-open", null);
        var voter = await _db.VoterRolls.SingleOrDefaultAsync(x => x.ElectionId == id && x.MemberId == memberId && x.IsEligible, ct);
        if (voter == null) return (false, "not-eligible", null);

        var accepted = await _db.Nominations.Where(x => x.ElectionId == id && x.Status == NominationStatus.Accepted)
            .Select(x => new { x.Id, x.ElectionSeatId }).ToListAsync(ct);
        if (!IsWholeBallotValid(e.Seats, accepted.Select(x => (x.Id, x.ElectionSeatId)), request)) return (false, "invalid-ballot", null);

        var trackingCode = NewTrackingCode();
        var choices = request.Seats.Select(x => new PendingChoice(x.ElectionSeatId, x.NominationIds.ToArray())).ToArray();
        var sealedChoices = BallotSeal.Seal(e.BallotPublicKey, JsonSerializer.Serialize(choices));

        await using (var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct))
        {
            if (voter.VotedAt != null || await _db.SeatVotes.AnyAsync(x => x.ElectionId == id && x.MemberId == memberId, ct))
                return (false, "already-voted", null);
            // Read again inside the transaction, so a close that commits first makes this vote
            // fail instead of landing after the count, and a ballot is never kept under a key
            // the election no longer holds.
            if (!await _db.Elections.AnyAsync(x => x.Id == id && x.Phase == ElectionPhase.Polling && x.BallotKeyFingerprint == e.BallotKeyFingerprint, ct))
                return (false, "not-open", null);

            foreach (var seat in e.Seats)
                _db.SeatVotes.Add(new SeatVote { ElectionId = id, ElectionSeatId = seat.Id, MemberId = memberId, VotedAt = votedOn });
            voter.VotedAt = votedOn;
            _db.PendingBallots.Add(new PendingBallot { Id = Guid.NewGuid(), ElectionId = id, SealedChoices = sealedChoices });
            _db.BallotReceipts.Add(new BallotReceipt { Id = Guid.NewGuid(), ElectionId = id, TrackingCode = trackingCode });
            try
            {
                await _db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch (Exception ex) when (ex is DbUpdateException or DbException)
            {
                await RollbackQuietlyAsync(transaction);
                _db.ChangeTracker.Clear();
                var voted = await _db.SeatVotes.AnyAsync(x => x.ElectionId == id && x.MemberId == memberId, ct);
                return (false, voted ? "already-voted" : "not-recorded", null);
            }
        }

        return (true, null, trackingCode);
    }

    public async Task<(IReadOnlyList<ElectionResultDto>? Results, string? Error)> CountAsync(int id, string? privateKeyPkcs8Base64, CancellationToken ct = default)
    {
        var e = await _db.Elections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e == null || e.Phase != ElectionPhase.Counting) return (null, "not-counting");

        var stored = await StoredResultsAsync(id, ct);
        if (stored.Count != 0) return (stored, null);
        if (string.IsNullOrWhiteSpace(privateKeyPkcs8Base64) || e.BallotKeyFingerprint == null) return (null, "no-key");

        using var rsa = RSA.Create();
        byte[]? keyBytes = null;
        try
        {
            keyBytes = Convert.FromBase64String(privateKeyPkcs8Base64.Trim());
            rsa.ImportPkcs8PrivateKey(keyBytes, out _);
        }
        catch (Exception ex) when (ex is FormatException or CryptographicException)
        {
            return (null, "wrong-key");
        }
        finally
        {
            if (keyBytes != null) CryptographicOperations.ZeroMemory(keyBytes);
        }
        if (BallotSeal.PrivateKeyFingerprint(rsa) != e.BallotKeyFingerprint) return (null, "wrong-key");

        await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        var pending = await _db.PendingBallots.Where(x => x.ElectionId == id).ToArrayAsync(ct);
        var alreadyMoved = await _db.Ballots.CountAsync(x => x.ElectionId == id, ct);
        var receipts = await _db.BallotReceipts.Where(x => x.ElectionId == id).ToArrayAsync(ct);
        var voted = await _db.VoterRolls.CountAsync(x => x.ElectionId == id && x.VotedAt != null, ct);
        var cast = pending.Length + alreadyMoved;
        // Spec 023 FR-003. Every voter marked as voted has one receipt and one ballot, or the
        // count stops before anything is written.
        if (cast != receipts.Length || cast != voted) return (null, "integrity");
        if (cast < Elections.MinimumBallotsToCount) return (null, "below-threshold");

        var opened = new List<PendingChoice[]>(pending.Length);
        try
        {
            foreach (var p in pending)
                opened.Add(JsonSerializer.Deserialize<PendingChoice[]>(BallotSeal.Open(rsa, p.SealedChoices)) ?? []);
        }
        catch (Exception ex) when (ex is CryptographicException or JsonException or FormatException)
        {
            return (null, "unreadable");
        }

        // Spec 023 FR-001. One shuffle over the whole election, written in one transaction, so
        // every ballot row shares one transaction id and the stored order is random.
        var ballots = opened.ToArray();
        RandomNumberGenerator.Shuffle<PendingChoice[]>(ballots);
        foreach (var choices in ballots)
        {
            var ballot = new Ballot { Id = Guid.NewGuid(), ElectionId = id };
            foreach (var choice in choices)
                foreach (var nominationId in choice.N)
                    ballot.Votes.Add(new BallotVote { Id = Guid.NewGuid(), ElectionSeatId = choice.S, NominationId = nominationId });
            _db.Ballots.Add(ballot);
        }
        _db.PendingBallots.RemoveRange(pending);

        // The receipts were written in the voter's own transaction. Writing them again here
        // drops that link to the SeatVote rows.
        var codes = receipts.Select(x => x.TrackingCode).ToArray();
        RandomNumberGenerator.Shuffle<string>(codes);
        _db.BallotReceipts.RemoveRange(receipts);

        try
        {
            await _db.SaveChangesAsync(ct);
            _db.BallotReceipts.AddRange(codes.Select(c => new BallotReceipt { Id = Guid.NewGuid(), ElectionId = id, TrackingCode = c }));
            var results = await TallyAsync(id, ct);
            _db.ElectionResults.AddRange(results.Select(x => new ElectionResult { ElectionId = id, ElectionSeatId = x.ElectionSeatId, NominationId = x.NominationId, VoteCount = x.VoteCount, IsElected = x.IsElected, IsTie = x.IsTie }));
            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception ex) when (ex is DbUpdateException or DbException)
        {
            // Two counts at once: the loser rolls back and returns what the winner stored.
            await RollbackQuietlyAsync(transaction);
            _db.ChangeTracker.Clear();
            var winner = await StoredResultsAsync(id, CancellationToken.None);
            return winner.Count != 0 ? (winner, null) : (null, "not-recorded");
        }

        await VacuumPendingBallotsAsync(id);
        return (await StoredResultsAsync(id, CancellationToken.None), null);
    }

    public async Task<bool> DeclareAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Elections.FindAsync([id], ct);
        if (e == null || e.Phase != ElectionPhase.Counting) return false;
        var results = await StoredResultsAsync(id, ct);
        if (results.Count == 0) return false;
        foreach (var r in results.Where(x => x.IsElected))
        {
            var n = await _db.Nominations.FindAsync([r.NominationId], ct);
            if (n != null) _db.ECMembers.Add(new ECMember { ECPeriodId = e.ECPeriodId, MemberId = n.CandidateMemberId, Position = (await _db.ElectionSeats.FindAsync([r.ElectionSeatId], ct))!.Position, StartDate = DateTime.UtcNow });
        }
        e.Phase = ElectionPhase.Declared; e.DeclaredOn = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private Task<List<ElectionResultDto>> StoredResultsAsync(int id, CancellationToken ct) =>
        _db.ElectionResults.AsNoTracking().Where(x => x.ElectionId == id)
            .OrderBy(x => x.ElectionSeatId).ThenBy(x => x.NominationId)
            .Select(x => new ElectionResultDto(x.ElectionSeatId, x.NominationId, x.VoteCount, x.IsElected, x.IsTie)).ToListAsync(ct);

    // Every accepted candidate gets a row, with zero when nobody chose them, so a stored result
    // always shows the count has run.
    private async Task<List<ElectionResultDto>> TallyAsync(int id, CancellationToken ct)
    {
        var counts = await _db.BallotVotes.Where(v => v.Ballot!.ElectionId == id && !v.Ballot.IsSpoiled)
            .GroupBy(v => v.NominationId).Select(g => new { NominationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.NominationId, x => x.Count, ct);
        var accepted = await _db.Nominations.Where(x => x.ElectionId == id && x.Status == NominationStatus.Accepted)
            .Select(x => new { x.Id, x.ElectionSeatId }).ToListAsync(ct);
        var places = await _db.ElectionSeats.Where(x => x.ElectionId == id).ToDictionaryAsync(x => x.Id, x => Math.Max(1, x.SeatCount), ct);
        return accepted.GroupBy(x => x.ElectionSeatId)
            .SelectMany(g => SeatResults(g.Key, places.GetValueOrDefault(g.Key, 1), g.Select(x => (x.Id, counts.GetValueOrDefault(x.Id)))))
            .OrderBy(x => x.ElectionSeatId).ThenBy(x => x.NominationId).ToList();
    }

    // The deleted pending and receipt rows stay in the table pages, with the voters' transaction
    // ids, until Postgres cleans them up. This asks for that now. It is best effort: a failure
    // here does not undo the count. Backups taken during polling are not touched (spec 023,
    // Known limits).
    private async Task VacuumPendingBallotsAsync(int id)
    {
        if (!_db.Database.IsNpgsql()) return;
        foreach (var sql in new[] { "VACUUM \"PendingBallots\"", "VACUUM \"BallotReceipts\"" })
        {
            try
            {
                await _db.Database.ExecuteSqlRawAsync(sql, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "{Sql} after the count of election {ElectionId} failed.", sql, id);
            }
        }
    }

    // A seat with N places elects its top N. Candidates level on votes at the last place are a
    // tie, and none of them is elected until the election's tie rule settles it.
    private static IEnumerable<ElectionResultDto> SeatResults(int seatId, int places, IEnumerable<(int NominationId, int Count)> counts)
    {
        var ordered = counts.OrderByDescending(x => x.Count).ToList();
        if (ordered.Count <= places)
            return ordered.Select(x => new ElectionResultDto(seatId, x.NominationId, x.Count, true, false));

        var cutOff = ordered[places - 1].Count;
        var tie = ordered[places].Count == cutOff;
        return ordered.Select(x => new ElectionResultDto(seatId, x.NominationId, x.Count,
            tie ? x.Count > cutOff : x.Count >= cutOff, tie && x.Count == cutOff));
    }

    private static bool IsWholeBallotValid(IEnumerable<ElectionSeat> seats, IEnumerable<(int Id, int SeatId)> accepted, CastBallotDto request)
    {
        if (request?.Seats == null) return false;
        var places = seats.ToDictionary(x => x.Id, x => Math.Max(1, x.SeatCount));
        var acceptedBySeat = accepted.ToLookup(x => x.SeatId, x => x.Id);
        // Every seat with a candidate must be on the ballot, even if left blank. A seat with no
        // accepted candidate never reaches the voter's screen, so it may be left out.
        var contested = places.Keys.Where(x => acceptedBySeat[x].Any()).ToList();
        if (contested.Count == 0 || contested.Any(x => !request.Seats.Any(s => s?.ElectionSeatId == x))) return false;
        var seen = new HashSet<int>();
        foreach (var entry in request.Seats)
        {
            if (entry?.NominationIds == null || !places.TryGetValue(entry.ElectionSeatId, out var seatCount) || !seen.Add(entry.ElectionSeatId)) return false;
            if (entry.NominationIds.Count > seatCount || entry.NominationIds.Distinct().Count() != entry.NominationIds.Count) return false;
            if (entry.NominationIds.Any(n => !acceptedBySeat[entry.ElectionSeatId].Contains(n))) return false;
        }
        return true;
    }

    // A serializable transaction that failed at COMMIT is already finished, and rolling it back
    // again throws. Nothing was written either way.
    private static async Task RollbackQuietlyAsync(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
    {
        try { await transaction.RollbackAsync(CancellationToken.None); }
        catch (InvalidOperationException) { }
    }

    private static string NewTrackingCode()
    {
        var length = Elections.TrackingCodeGroupLength;
        var raw = RandomNumberGenerator.GetString(Elections.TrackingCodeAlphabet, Elections.TrackingCodeGroups * length);
        return string.Join('-', Enumerable.Range(0, Elections.TrackingCodeGroups).Select(i => raw.Substring(i * length, length)));
    }

    // Short names keep the sealed choices small. S is the seat id and N the chosen nomination ids.
    private sealed record PendingChoice(int S, int[] N);

    public async Task<IReadOnlyList<NominationViewDto>> GetNominationsAsync(int id, CancellationToken ct = default) =>
        (await _db.Nominations.Where(x => x.ElectionId == id).OrderBy(x => x.ElectionSeatId).ToListAsync(ct)).Select(ToNomination).ToList();

    public async Task<IReadOnlyList<AdminElectionDto>> ListAdminElectionsAsync(CancellationToken ct = default)
    {
        var elections = await _db.Elections.AsNoTracking()
            .Include(x => x.Seats)
            .Include(x => x.VoterRoll)
            .OrderByDescending(x => x.AnnouncedOn)
            .ToListAsync(ct);
        return elections.Select(ToAdminElection).ToArray();
    }

    public async Task<AdminElectionDto?> GetAdminElectionAsync(int id, CancellationToken ct = default)
    {
        var election = await _db.Elections.AsNoTracking()
            .Include(x => x.Seats)
            .Include(x => x.VoterRoll)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return election is null ? null : ToAdminElection(election);
    }

    public async Task<(bool Success, string? Error, AdminElectionDto? Election)> AddCandidateAsync(int id, SaveCandidateRequest request, CancellationToken ct = default)
    {
        var e = await _db.Elections.Include(x => x.VoterRoll).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e == null) return (false, "not-found", null);
        // Spec 023 FR-009: an admin-added candidate still goes through scrutiny, so it has to
        // arrive while scrutiny can still happen.
        if (e.Phase > ElectionPhase.Scrutiny) return (false, "phase-closed", null);
        if (!await _db.ElectionSeats.AnyAsync(x => x.ElectionId == id && x.Id == request.PositionId, ct))
            return (false, "seat-not-found", null);

        var roll = e.VoterRoll.Where(x => x.IsEligible).Select(x => x.MemberId).ToHashSet();
        if (!roll.Contains(request.MemberId)) return (false, "not-eligible", null);
        if (!roll.Contains(request.ProposerMemberId) || !roll.Contains(request.SeconderMemberId) ||
            new[] { request.MemberId, request.ProposerMemberId, request.SeconderMemberId }.Distinct().Count() != 3)
            return (false, "invalid-proposer", null);

        _db.Nominations.Add(new Nomination
        {
            ElectionId = id,
            ElectionSeatId = request.PositionId,
            CandidateMemberId = request.MemberId,
            ProposerMemberId = request.ProposerMemberId,
            SeconderMemberId = request.SeconderMemberId,
            Statement = request.Statement ?? string.Empty,
            Status = NominationStatus.Submitted,
            SubmittedAt = DateTime.UtcNow,
        });

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return (false, "duplicate-candidate", null);
        }

        return (true, null, await GetAdminElectionAsync(id, ct));
    }

    public async Task<(bool Success, string? Error)> RemoveCandidateAsync(int id, int candidateId, CancellationToken ct = default)
    {
        var nomination = await _db.Nominations.Include(x => x.Election).FirstOrDefaultAsync(x => x.ElectionId == id && x.Id == candidateId, ct);
        if (nomination is null) return (false, "not-found");
        // Once the candidate list is out, a candidate leaves only by withdrawing.
        if (nomination.Election!.Phase >= ElectionPhase.CandidateList) return (false, "phase-closed");

        _db.Nominations.Remove(nomination);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return (false, "has-votes");
        }

        return (true, null);
    }

    private static AdminElectionDto ToAdminElection(Election election)
    {
        var positions = election.Seats
            .OrderBy(x => x.Id)
            .Select(x => new AdminElectionPositionDto(x.Id, SeatTitle(x.Position), null, x.SeatCount))
            .ToArray();

        return new AdminElectionDto(
            election.Id,
            election.Title,
            null,
            election.Phase,
            election.AnnouncedOn,
            election.NominationOpensOn,
            election.NominationClosesOn,
            election.ScrutinyOn,
            election.WithdrawalClosesOn,
            election.PollingOpensOn,
            election.PollingClosesOn,
            election.DeclaredOn,
            election.IsActive,
            positions,
            Array.Empty<AdminElectionCandidateDto>(),
            election.VoterRoll.Count(x => x.IsEligible),
            election.VoterRoll.Any(x => x.VotedAt.HasValue),
            election.TieRule,
            election.BallotKeyFingerprint);
    }

    private static string SeatTitle(ECPosition position) => position switch
    {
        ECPosition.President => "President",
        ECPosition.VicePresident => "Vice President",
        ECPosition.GeneralSecretary => "General Secretary",
        ECPosition.OfficeSecretary => "Office Secretary",
        ECPosition.JointSecretary1 => "Joint Secretary 1",
        ECPosition.JointSecretary2 => "Joint Secretary 2",
        ECPosition.Treasurer => "Treasurer",
        ECPosition.MediaCulturalAndSportsSecretary => "Media Cultural & Sports Secretary",
        ECPosition.OrganizationalSecretary => "Organizational Secretary",
        ECPosition.InformationAndTechnologySecretary => "Information and Technology Secretary",
        ECPosition.Member1 => "Member 1",
        ECPosition.Member2 => "Member 2",
        ECPosition.LawSecretary => "Law Secretary",
        ECPosition.ImmediatePastPresident => "Immediate Past President",
        ECPosition.InstitutionalRepresentative => "Institutional Representative",
        _ => position.ToString(),
    };

    private static ElectionSummaryDto ToSummary(Election e, int count, int eligible) => new(e.Id, e.Title, e.Phase, e.ECPeriodId, count, eligible);
    private static NominationViewDto ToNomination(Nomination n) => new(n.Id, n.ElectionSeatId, n.CandidateMemberId, n.Status, n.Statement);
}
