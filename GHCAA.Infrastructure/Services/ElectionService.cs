using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text.Json;
using static GHCAA.Domain.Constants;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services;

// orgConfig is optional so tests can build the service from a context alone. Without it the
// ballot order falls back to the CandidateOrder default, Random.
public sealed class ElectionService(ApplicationDbContext db, ILogger<ElectionService>? logger = null, IOrgConfigService? orgConfig = null, IElectionAuditWriter? audit = null) : IElectionService
{
    private readonly ApplicationDbContext _db = db;
    private readonly IElectionAuditWriter _audit = audit ?? new ElectionAuditWriter(db);
    private readonly ILogger<ElectionService>? _logger = logger;
    private readonly IOrgConfigService? _orgConfig = orgConfig;

    public async Task<(ElectionSummaryDto? Election, string? Error)> CreateAsync(CreateElectionDto request, CancellationToken ct = default)
    {
        // A date the client left empty arrives as DateTime.MinValue, which would pass the order check.
        if (request.NominationOpensOn == default || request.NominationClosesOn == default
            || request.PollingOpensOn == default || request.PollingClosesOn == default)
            return (null, "dates-missing");

        // Spec 023 D3. Scrutiny and the withdrawal deadline are optional, but when given they sit
        // between the close of nominations and the opening of polling.
        DateTime?[] steps = [request.NominationOpensOn, request.NominationClosesOn, request.ScrutinyOn, request.WithdrawalClosesOn, request.PollingOpensOn, request.PollingClosesOn];
        var given = steps.Where(x => x.HasValue).Select(x => AsUtc(x!.Value)).ToList();
        if (given.Zip(given.Skip(1)).Any(pair => pair.Second < pair.First)) return (null, "dates-out-of-order");

        // 37.1t. The winners are written into this period on Declare, so it has to be a real
        // period that starts once polling is over and has no committee in it yet. Dates are
        // compared by day because a period's start is stored as a date.
        // FR-047: a rehearsal skips the two checks that guard the real committee.
        var period = await _db.ECPeriods.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ECPeriodId, ct);
        if (period == null) return (null, "no-period");
        if (!request.IsTest && period.StartDate.Date < AsUtc(request.PollingClosesOn).Date) return (null, "period-starts-early");
        if (!request.IsTest && await _db.ECMembers.AnyAsync(x => x.ECPeriodId == period.Id, ct)) return (null, "period-has-members");
        if (await _db.Elections.AnyAsync(x => x.ECPeriodId == period.Id && !x.IsTest && x.Phase != ElectionPhase.Archived, ct)) return (null, "period-in-use");

        var election = new Election
        {
            Title = request.Title,
            ECPeriodId = request.ECPeriodId,
            CreatedBy = request.CreatedBy,
            AnnouncedOn = DateTime.UtcNow,
            NominationOpensOn = AsUtc(request.NominationOpensOn),
            NominationClosesOn = AsUtc(request.NominationClosesOn),
            ScrutinyOn = request.ScrutinyOn is { } scrutinyOn ? AsUtc(scrutinyOn) : null,
            WithdrawalClosesOn = request.WithdrawalClosesOn is { } withdrawalClosesOn ? AsUtc(withdrawalClosesOn) : null,
            PollingOpensOn = AsUtc(request.PollingOpensOn),
            PollingClosesOn = AsUtc(request.PollingClosesOn),
            TieRule = request.TieRule,
            IsTest = request.IsTest
        };
        _db.Elections.Add(election);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Two creates raced past the check above and IX_Elections_ECPeriodId_Live stopped the second.
            _db.Entry(election).State = EntityState.Detached;
            if (!await _db.Elections.AnyAsync(x => x.ECPeriodId == period.Id && !x.IsTest && x.Phase != ElectionPhase.Archived, ct))
                throw;
            return (null, "period-in-use");
        }
        return (ToSummary(election, 0, 0), null);
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
            .Where(x => !x.IsTest)
            .OrderByDescending(x => x.AnnouncedOn)
            .FirstOrDefaultAsync(ct);
        return e == null ? null : ToSummary(e, e.VoterRoll.Count, e.VoterRoll.Count(x => x.IsEligible));
    }

    public async Task<ElectionSummaryDto?> GetLatestDeclaredAsync(CancellationToken ct = default)
    {
        var e = await _db.Elections
            .Include(x => x.VoterRoll)
            .Where(x => x.Phase == ElectionPhase.Declared || x.Phase == ElectionPhase.Archived)
            .Where(x => !x.IsTest)
            .OrderByDescending(x => x.DeclaredOn ?? x.AnnouncedOn)
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
        if (phase == ElectionPhase.CandidateList) await SetBallotOrderAsync(id, ct);
        var from = e.Phase;
        e.Phase = phase;
        await _audit.AppendAsync(id, Elections.AuditActions.PhaseChanged, null, new { from = from.ToString(), to = phase.ToString() }, ct);
        await _audit.SaveAsync(ct);
        return true;
    }

    // Spec 023 FR-033 (37.12i). Only accepted, live appointments on public personas are listed.
    // An archived election keeps its roster: archiving revokes every appointment, so those ended
    // with the election still count. Anyone revoked for another reason stays off the board.
    // Turnout is shown once polling has closed, or during polling when the setting is on, and never
    // below the minimum ballot count, so a small turnout cannot point at known voters.
    public async Task<ElectionBoardDto?> GetBoardAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Elections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e == null) return null;

        var now = DateTime.UtcNow;
        var archived = e.Phase == ElectionPhase.Archived;
        var rows = await _db.ElectionAppointments.AsNoTracking()
            .Where(a => a.AcceptedAt != null && a.DeclarationSignedAt != null)
            .Where(a => a.RevokedAt == null
                ? (a.ExpiresAt == null || a.ExpiresAt > now)
                : archived && a.RevokedReason == Elections.ElectionArchivedReason)
            .Where(a => a.ElectionId == id && a.Persona!.IsActive && a.Persona.ShowOnPublicBoard)
            .OrderBy(a => a.Persona!.SortOrder).ThenBy(a => a.Persona!.Id).ThenBy(a => a.AcceptedAt)
            .Select(a => new { Group = a.Persona!.GroupName, Persona = a.Persona.Name, a.DisplayName, AcceptedAt = a.AcceptedAt!.Value })
            .ToListAsync(ct);
        var groups = rows.GroupBy(r => r.Group)
            .Select(g => new ElectionBoardGroupDto(g.Key, g.Select(r => new ElectionBoardMemberDto(r.Persona, r.DisplayName, r.AcceptedAt)).ToArray()))
            .ToArray();

        ElectionBoardTurnoutDto? turnout = null;
        var showDuringPolling = _orgConfig != null && (await _orgConfig.GetConfigAsync()).Elections.ShowTurnoutDuringPolling;
        var turnoutOpen = e.Phase >= ElectionPhase.Counting || (e.Phase == ElectionPhase.Polling && showDuringPolling);
        if (turnoutOpen)
        {
            var size = await _db.VoterRolls.CountAsync(x => x.ElectionId == id && x.IsEligible, ct);
            var voted = await _db.VoterRolls.CountAsync(x => x.ElectionId == id && x.IsEligible && x.VotedAt != null, ct);
            if (voted >= Elections.MinimumBallotsToCount)
                turnout = new ElectionBoardTurnoutDto(voted, size);
        }

        // 37.12p. The head shown is the Declared row, not the newest row. Archiving adds a revoke row
        // per appointment, so a head noted on the day of the result would no longer match. The
        // check link still covers the rows after it. An election declared before the chain existed
        // has no Declared row and shows the newest one.
        string? head = null;
        if (e.Phase >= ElectionPhase.Declared)
        {
            var chain = _db.ElectionAuditEntries.AsNoTracking().Where(x => x.ElectionId == id);
            head = await chain.Where(x => x.Action == Elections.AuditActions.Declared)
                .OrderByDescending(x => x.Seq).Select(x => x.Hash).FirstOrDefaultAsync(ct)
                ?? await chain.OrderByDescending(x => x.Seq).Select(x => x.Hash).FirstOrDefaultAsync(ct);
        }

        return new ElectionBoardDto(e.Id, e.Title, e.Phase, e.IsTest, e.AnnouncedOn, e.NominationOpensOn, e.NominationClosesOn,
            e.ScrutinyOn, e.WithdrawalClosesOn, e.PollingOpensOn, e.PollingClosesOn, e.DeclaredOn, groups, turnout, e.BallotKeyFingerprint, head);
    }

    public Task<bool> IsTestAsync(int id, CancellationToken ct = default) =>
        _db.Elections.AsNoTracking().AnyAsync(x => x.Id == id && x.IsTest, ct);

    // Spec 023 FR-046 (37.13b). Saved in the same SaveChanges as the phase, so the order and the
    // phase change land together. Phases only move forward, so this runs once per election.
    // The order is recorded in the election's audit chain.
    private async Task SetBallotOrderAsync(int id, CancellationToken ct)
    {
        var order = _orgConfig == null ? ElectionCandidateOrder.Random : (await _orgConfig.GetConfigAsync()).Elections.CandidateOrder;
        var accepted = await (from n in _db.Nominations
                              join m in _db.Members on n.CandidateMemberId equals m.Id
                              where n.ElectionId == id && n.Status == NominationStatus.Accepted
                              select new { Nomination = n, m.FullName }).ToListAsync(ct);
        if (accepted.Count == 0 || accepted.Any(x => x.Nomination.BallotOrder != null)) return;

        var seats = new List<object>();
        foreach (var seat in accepted.GroupBy(x => x.Nomination.ElectionSeatId).OrderBy(g => g.Key))
        {
            var list = order == ElectionCandidateOrder.Alphabetical
                ? seat.OrderBy(x => x.FullName, StringComparer.OrdinalIgnoreCase).ThenBy(x => x.Nomination.Id).ToArray()
                : seat.ToArray();
            if (order != ElectionCandidateOrder.Alphabetical) RandomNumberGenerator.Shuffle(list.AsSpan());
            for (var i = 0; i < list.Length; i++) list[i].Nomination.BallotOrder = i + 1;
            seats.Add(new { seatId = seat.Key, nominationIds = list.Select(x => x.Nomination.Id).ToArray() });
        }
        await _audit.AppendAsync(id, Elections.AuditActions.CandidateOrderSet, null, new { order = order.ToString(), seats }, ct);
    }

    public async Task<(bool Success, string? Error, string? Fingerprint)> SetBallotKeyAsync(int id, string publicKeySpkiBase64, CancellationToken ct = default)
    {
        var e = await _db.Elections.FindAsync([id], ct);
        if (e == null) return (false, Elections.Replies.NotFound, null);
        // Once polling opens, ballots are sealed under the stored key, so it cannot change.
        if (e.Phase >= ElectionPhase.Polling) return (false, "phase-closed", null);
        var key = publicKeySpkiBase64?.Trim();
        var fingerprint = string.IsNullOrEmpty(key) ? null : BallotSeal.Fingerprint(key);
        if (fingerprint == null) return (false, "invalid-key", null);
        // The key and its audit row are saved together. The approval service already holds a
        // transaction when it calls this one, so that one is joined. Called alone, this opens its own.
        var ownsTransaction = _db.Database.CurrentTransaction == null;
        await using var transaction = ownsTransaction ? await _db.Database.BeginTransactionAsync(ct) : null;
        // The phase is checked again in the update itself, so a key cannot land after polling
        // opened between the read above and this write.
        var updated = await _db.Elections.Where(x => x.Id == id && x.Phase < ElectionPhase.Polling)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.BallotPublicKey, key).SetProperty(x => x.BallotKeyFingerprint, fingerprint), ct);
        _db.Entry(e).State = EntityState.Detached;
        if (updated != 1) return (false, "phase-closed", null);
        await _audit.AppendAsync(id, Elections.AuditActions.BallotKeySet, null, new { fingerprint }, ct);
        await _audit.SaveAsync(ct);
        if (transaction != null) await transaction.CommitAsync(ct);
        return (true, null, fingerprint);
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

    public async Task<(bool Success, string? Error, int Removed)> UnfreezeVoterRollAsync(int id, int userId, string reason, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(reason)) return (false, "reason-required", 0);
        var e = await _db.Elections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e == null) return (false, "not-found", 0);
        if (e.Phase > ElectionPhase.Nomination) return (false, "phase-closed", 0);
        var cutoffDays = _orgConfig == null ? new ElectionSettingsDto().UnfreezeCutoffDaysBeforeNominationClose
            : (await _orgConfig.GetConfigAsync()).Elections.UnfreezeCutoffDaysBeforeNominationClose;
        if (DateTime.UtcNow > e.NominationClosesOn.AddDays(-cutoffDays)) return (false, "window-closed", 0);
        if (await _db.Nominations.AnyAsync(n => n.ElectionId == id, ct)) return (false, "has-nominations", 0);
        var ownsTransaction = _db.Database.CurrentTransaction == null;
        await using var transaction = ownsTransaction ? await _db.Database.BeginTransactionAsync(ct) : null;
        var removed = await _db.VoterRolls.Where(v => v.ElectionId == id).ExecuteDeleteAsync(ct);
        await _audit.AppendAsync(id, Elections.AuditActions.VoterRollUnfrozen, userId, new { reason = reason.Trim(), removed }, ct);
        await _audit.SaveAsync(ct);
        if (transaction != null) await transaction.CommitAsync(ct);
        return (true, null, removed);
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

    public async Task<bool> DecideNominationAsync(int nominationId, int decidedByUserId, ScrutinyDto request, CancellationToken ct = default)
    {
        var n = await _db.Nominations.Include(x => x.Election).FirstOrDefaultAsync(x => x.Id == nominationId, ct);
        if (n == null || n.Status is NominationStatus.Withdrawn) return false;
        if (n.Election?.Phase != ElectionPhase.Scrutiny) return false;
        n.Status = request.Accepted ? NominationStatus.Accepted : NominationStatus.Rejected;
        _db.ScrutinyDecisions.Add(new ScrutinyDecision { NominationId = nominationId, DecidedByUserId = decidedByUserId, Accepted = request.Accepted, Reason = request.Reason, DecidedAt = DateTime.UtcNow });
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
        var sealedChoices = BallotSeal.Seal(e.BallotPublicKey, JsonSerializer.Serialize(choices), e.Id, e.BallotSealVersion);

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
                opened.Add(JsonSerializer.Deserialize<PendingChoice[]>(BallotSeal.Open(rsa, p.SealedChoices, id, e.BallotSealVersion)) ?? []);
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
            // Inside the count transaction. A clash with a second count rolls this one back.
            await _audit.AppendAsync(id, Elections.AuditActions.Counted, null, new { ballots = codes.Length }, ct);
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
        var e = await _db.Elections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e == null || e.Phase != ElectionPhase.Counting) return false;
        var results = await StoredResultsAsync(id, ct);
        if (results.Count == 0) return false;

        // The phase is claimed with a conditional update, so two declares at once cannot both
        // write the committee. The loser waits on the row, then finds it no longer in Counting.
        // Leaving without a commit rolls the claim back. The approval service already holds a
        // transaction when it calls this, and EF refuses a nested one, so that one is joined
        // and its owner commits or rolls back.
        var ownsTransaction = _db.Database.CurrentTransaction == null;
        await using var transaction = ownsTransaction ? await _db.Database.BeginTransactionAsync(ct) : null;
        var declaredOn = DateTime.UtcNow;
        var claimed = await _db.Elections.Where(x => x.Id == id && x.Phase == ElectionPhase.Counting)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Phase, ElectionPhase.Declared)
                .SetProperty(x => x.DeclaredOn, declaredOn), ct);
        if (claimed != 1) return false;

        // 37.1u. The winners take office when their period starts, not on the day of the result.
        // The outgoing committee stays current until an admin makes the new period active.
        // Members can be added to the period by hand while the election runs. The winners would
        // then sit beside them, so Declare refuses.
        // FR-047: a test election declares its result but puts nobody on the committee.
        if (!e.IsTest)
        {
            var period = await _db.ECPeriods.AsNoTracking().FirstOrDefaultAsync(x => x.Id == e.ECPeriodId, ct);
            if (period == null || await _db.ECMembers.AnyAsync(x => x.ECPeriodId == period.Id, ct)) return false;
            var periodStart = period.StartDate;
            foreach (var r in results.Where(x => x.IsElected))
            {
                var n = await _db.Nominations.FindAsync([r.NominationId], ct);
                if (n != null) _db.ECMembers.Add(new ECMember { ECPeriodId = e.ECPeriodId, MemberId = n.CandidateMemberId, Position = (await _db.ElectionSeats.FindAsync([r.ElectionSeatId], ct))!.Position, StartDate = periodStart });
            }
        }
        await SetAppointmentExpiryAsync(id, declaredOn, ct);
        await _audit.AppendAsync(id, Elections.AuditActions.Declared, null, new { elected = results.Count(x => x.IsElected) }, ct);
        try
        {
            // Inside the transaction the audit writer cannot retry a Seq clash, so a clash lands here.
            await _audit.SaveAsync(ct);
            if (transaction != null) await transaction.CommitAsync(ct);
        }
        catch (Exception ex) when (ownsTransaction && ex is (DbUpdateException or DbException))
        {
            await RollbackQuietlyAsync(transaction!);
            _db.ChangeTracker.Clear();
            return false;
        }
        return true;
    }

    // One caller at a time per election, so a double click cannot write the record twice.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, SemaphoreSlim> KeyDestroyedLocks = new();

    public async Task<string?> RecordKeyDestroyedAsync(int id, int actorUserId, CancellationToken ct = default)
    {
        var gate = KeyDestroyedLocks.GetOrAdd(id, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            return await RecordKeyDestroyedLockedAsync(id, actorUserId, ct);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<string?> RecordKeyDestroyedLockedAsync(int id, int actorUserId, CancellationToken ct)
    {
        var e = await _db.Elections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e == null) return Elections.Replies.NotFound;
        if (e.Phase != ElectionPhase.Declared) return Elections.Replies.NotDeclared;
        if (await _db.ElectionAuditEntries.AnyAsync(x => x.ElectionId == id && x.Action == Elections.AuditActions.KeyDestroyed, ct))
            return Elections.Replies.AlreadyRecorded;
        await _audit.AppendAsync(id, Elections.AuditActions.KeyDestroyed, actorUserId, new { fingerprint = e.BallotKeyFingerprint }, ct);
        await _audit.SaveAsync(ct);
        return null;
    }

    // FR-032 (37.12g). Open appointments with no end date get one from the declare date. The access
    // service already refuses an expired row, so nothing has to run on that day. Saved with the phase.
    private async Task SetAppointmentExpiryAsync(int electionId, DateTime declaredOn, CancellationToken ct)
    {
        var days = _orgConfig == null ? new ElectionSettingsDto().AccessEndsDaysAfterDeclare : (await _orgConfig.GetConfigAsync()).Elections.AccessEndsDaysAfterDeclare;
        var ends = declaredOn.AddDays(days);
        var open = await _db.ElectionAppointments.Where(x => x.ElectionId == electionId && x.RevokedAt == null && x.ExpiresAt == null).ToListAsync(ct);
        foreach (var appointment in open)
            appointment.ExpiresAt = ends;
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

    // The web form sends a bare yyyy-MM-dd and mobile some dates with no offset. Those bind as
    // Unspecified, and ToUniversalTime would read them as server local time, so the stored date
    // would move by the server's offset. Same rule as the UTC converter in ApplicationDbContext.
    private static DateTime AsUtc(DateTime value) =>
        value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value.ToUniversalTime();

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

    public async Task<BallotViewDto?> GetBallotAsync(int id, int memberId, CancellationToken ct = default)
    {
        var e = await _db.Elections.AsNoTracking().Include(x => x.Seats).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e == null) return null;
        var hasVoted = await _db.SeatVotes.AnyAsync(x => x.ElectionId == id && x.MemberId == memberId, ct);
        var roll = await _db.VoterRolls.AsNoTracking().Where(x => x.ElectionId == id && x.MemberId == memberId)
            .Select(x => new { x.IsEligible, x.IneligibilityReason }).FirstOrDefaultAsync(ct);
        var rollFrozen = roll != null || await _db.VoterRolls.AnyAsync(x => x.ElectionId == id, ct);
        bool? isEligible = rollFrozen ? roll?.IsEligible == true : null;
        var reason = isEligible != false ? null
            : roll == null ? "You are not on the voter roll for this election."
            : roll.IneligibilityReason ?? "You are not eligible to vote in this election.";
        var candidates = await (from n in _db.Nominations.AsNoTracking()
                                join m in _db.Members.AsNoTracking() on n.CandidateMemberId equals m.Id
                                where n.ElectionId == id && n.Status == NominationStatus.Accepted
                                select new { n.Id, n.ElectionSeatId, n.BallotOrder, m.FullName, Photo = n.PhotoPath ?? m.PhotoPath, n.Statement })
                               .ToListAsync(ct);
        // FR-046. Rows with no BallotOrder come from elections that reached CandidateList before
        // the order existed. They sort after any ordered row, by name.
        var bySeat = candidates.OrderBy(x => x.BallotOrder ?? int.MaxValue)
            .ThenBy(x => x.FullName, StringComparer.OrdinalIgnoreCase).ToLookup(x => x.ElectionSeatId);
        // A seat with no accepted candidate is not on the paper (see IsWholeBallotValid).
        var seats = e.Seats.Where(s => bySeat[s.Id].Any()).OrderBy(s => s.Position).ThenBy(s => s.Id)
            .Select(s => new BallotSeatViewDto(s.Id, SeatTitle(s.Position), Math.Max(1, s.SeatCount),
                bySeat[s.Id].Select(c => new BallotCandidateDto(c.Id, c.FullName, c.Photo, c.Statement)).ToArray()))
            .ToArray();
        return new BallotViewDto(e.Id, e.Title, e.Phase, hasVoted, e.BallotKeyFingerprint, seats, isEligible, reason);
    }

    public async Task<IReadOnlyList<string>?> GetTrackingCodesAsync(int id, CancellationToken ct = default)
    {
        // During polling the receipt rows sit in the order voters cast them. The count rewrites
        // them in a random order, so the list is safe to show only after it has run.
        var e = await _db.Elections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e == null || e.Phase < ElectionPhase.Counting) return null;
        if (!await _db.ElectionResults.AnyAsync(x => x.ElectionId == id, ct)) return null;
        return await _db.BallotReceipts.AsNoTracking().Where(x => x.ElectionId == id)
            .Select(x => x.TrackingCode).OrderBy(x => x).ToListAsync(ct);
    }

    // Spec 023 FR-049 (37.13c). Each seat's ballots are grouped by ballot and ordered by the smallest
    // BallotVote.Id in the group. The ids are random Guids, so the order is the same on every read
    // and unrelated between seats. Nothing is stored for it.
    public async Task<OpenedBallotsDto?> GetOpenedBallotsAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Elections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e == null || (e.Phase != ElectionPhase.Declared && e.Phase != ElectionPhase.Archived)) return null;
        if (_orgConfig != null && !(await _orgConfig.GetConfigAsync()).Elections.PublishPerSeatBallots) return null;

        var votes = await _db.BallotVotes.AsNoTracking().Where(v => v.Ballot!.ElectionId == id && !v.Ballot.IsSpoiled)
            .Select(v => new { v.Id, v.BallotId, v.ElectionSeatId, v.NominationId }).ToListAsync(ct);
        var seats = votes.GroupBy(v => v.ElectionSeatId).OrderBy(g => g.Key).Select(seat => new OpenedSeatBallotsDto(
            seat.Key,
            seat.GroupBy(v => v.BallotId)
                .OrderBy(b => b.Min(v => v.Id))
                .Select(b => (IReadOnlyList<int>)b.Select(v => v.NominationId).OrderBy(n => n).ToArray())
                .ToArray())).ToArray();
        return new OpenedBallotsDto(id, seats);
    }

    public async Task<PublicElectionResultsDto?> GetPublicResultsAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Elections.AsNoTracking().Include(x => x.Seats).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e == null || (e.Phase != ElectionPhase.Declared && e.Phase != ElectionPhase.Archived)) return null;

        var results = await StoredResultsAsync(id, ct);
        var names = await (from n in _db.Nominations.AsNoTracking()
                           join m in _db.Members.AsNoTracking() on n.CandidateMemberId equals m.Id
                           where n.ElectionId == id
                           select new { n.Id, m.FullName }).ToDictionaryAsync(x => x.Id, x => x.FullName, ct);
        var eligible = await _db.VoterRolls.CountAsync(x => x.ElectionId == id && x.IsEligible, ct);
        var cast = await _db.Ballots.CountAsync(x => x.ElectionId == id, ct);
        var spoiled = await _db.Ballots.CountAsync(x => x.ElectionId == id && x.IsSpoiled, ct);
        // A ballot that named nobody for a seat has no vote row for it.
        var ballotsPerSeat = (await _db.BallotVotes.Where(v => v.Ballot!.ElectionId == id && !v.Ballot.IsSpoiled)
                .Select(v => new { v.ElectionSeatId, v.BallotId }).Distinct().ToListAsync(ct))
            .GroupBy(x => x.ElectionSeatId).ToDictionary(g => g.Key, g => g.Count());
        var valid = cast - spoiled;

        var bySeat = results.ToLookup(x => x.ElectionSeatId);
        var seats = e.Seats.Where(s => bySeat[s.Id].Any()).OrderBy(s => s.Position).ThenBy(s => s.Id)
            .Select(s => new PublicSeatResultDto(s.Id, SeatTitle(s.Position), Math.Max(1, s.SeatCount),
                valid - ballotsPerSeat.GetValueOrDefault(s.Id),
                bySeat[s.Id].OrderByDescending(r => r.VoteCount).ThenBy(r => names.GetValueOrDefault(r.NominationId))
                    .Select(r => new PublicCandidateResultDto(r.NominationId, names.GetValueOrDefault(r.NominationId) ?? string.Empty, r.VoteCount, r.IsElected, r.IsTie))
                    .ToArray()))
            .ToArray();
        return new PublicElectionResultsDto(e.Id, e.Title, e.Phase, e.DeclaredOn, eligible, cast, spoiled, seats);
    }

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
        if (e == null) return (false, Elections.Replies.NotFound, null);
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
        if (nomination is null) return (false, Elections.Replies.NotFound);
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
            election.BallotKeyFingerprint,
            IsTest: election.IsTest);
    }

    internal static string SeatTitle(ECPosition position) => position switch
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

    private static ElectionSummaryDto ToSummary(Election e, int count, int eligible) => new(e.Id, e.Title, e.Phase, e.ECPeriodId, count, eligible, e.BallotKeyFingerprint);
    private static NominationViewDto ToNomination(Nomination n) => new(n.Id, n.ElectionSeatId, n.CandidateMemberId, n.Status, n.Statement);
}
