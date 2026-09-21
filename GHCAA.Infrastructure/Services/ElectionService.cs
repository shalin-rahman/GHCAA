using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services;

public sealed class ElectionService(ApplicationDbContext db) : IElectionService
{
    private readonly ApplicationDbContext _db = db;

    public async Task<ElectionSummaryDto> CreateAsync(CreateElectionDto request, CancellationToken ct = default)
    {
        var election = new Election
        {
            Title = request.Title, ECPeriodId = request.ECPeriodId, CreatedBy = request.CreatedBy,
            AnnouncedOn = DateTime.UtcNow, NominationOpensOn = request.NominationOpensOn.ToUniversalTime(),
            NominationClosesOn = request.NominationClosesOn.ToUniversalTime(),
            PollingOpensOn = request.PollingOpensOn.ToUniversalTime(), PollingClosesOn = request.PollingClosesOn.ToUniversalTime()
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
        if (e == null || phase < e.Phase || phase > ElectionPhase.Archived || (int)phase > (int)e.Phase + 1) return false;
        if (phase == ElectionPhase.Polling && !await _db.VoterRolls.AnyAsync(x => x.ElectionId == id, ct)) return false;
        if (phase == ElectionPhase.Counting && DateTime.UtcNow < e.PollingClosesOn) return false;
        if (phase == ElectionPhase.Declared && !await _db.ElectionResults.AnyAsync(x => x.ElectionId == id, ct)) return false;
        e.Phase = phase;
        if (phase == ElectionPhase.Declared) e.DeclaredOn = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
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

    public async Task<bool> DecideNominationAsync(int nominationId, ScrutinyDto request, CancellationToken ct = default)
    {
        var n = await _db.Nominations.Include(x => x.Election).FirstOrDefaultAsync(x => x.Id == nominationId, ct);
        if (n == null || n.Status is NominationStatus.Withdrawn) return false;
        if (n.Election?.Phase != ElectionPhase.Scrutiny) return false;
        if (!await _db.ElectionOfficers.AnyAsync(x => x.ElectionId == n.ElectionId && x.MemberId == request.OfficerMemberId &&
            (x.Role == ElectionRole.ReturningOfficer || x.Role == ElectionRole.AssistantReturningOfficer || x.Role == ElectionRole.Scrutineer), ct))
            return false;
        n.Status = request.Accepted ? NominationStatus.Accepted : NominationStatus.Rejected;
        _db.ScrutinyDecisions.Add(new ScrutinyDecision { NominationId = nominationId, OfficerMemberId = request.OfficerMemberId, Accepted = request.Accepted, Reason = request.Reason, DecidedAt = DateTime.UtcNow });
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

    public async Task<bool> CastVoteAsync(int id, int memberId, CastVoteDto request, CancellationToken ct = default)
    {
        var e = await _db.Elections.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e == null || e.Phase != ElectionPhase.Polling || DateTime.UtcNow < e.PollingOpensOn || DateTime.UtcNow > e.PollingClosesOn) return false;
        var voter = await _db.VoterRolls.SingleOrDefaultAsync(x => x.ElectionId == id && x.MemberId == memberId && x.IsEligible, ct);
        if (voter == null) return false;
        var nomination = await _db.Nominations.SingleOrDefaultAsync(x => x.Id == request.NominationId && x.ElectionId == id && x.ElectionSeatId == request.ElectionSeatId && x.Status == NominationStatus.Accepted, ct);
        if (nomination == null) return false;
        await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        var votedAt = DateTime.UtcNow;
        var marked = await _db.VoterRolls.Where(x => x.Id == voter.Id && x.VotedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.VotedAt, votedAt), ct);
        if (marked != 1) return false;
        voter.VotedAt = votedAt;
        var ballot = new Ballot { ElectionId = id, ElectionSeatId = request.ElectionSeatId, SerialNumber = request.SerialNumber ?? Guid.NewGuid().ToString("N"), IssuedAt = DateTime.UtcNow };
        _db.Ballots.Add(ballot);
        _db.BallotVotes.Add(new BallotVote { Ballot = ballot, NominationId = nomination.Id, CastAt = DateTime.UtcNow });
        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<ElectionResultDto>> CountAsync(int id, CancellationToken ct = default)
    {
        var rows = await _db.BallotVotes.Where(v => v.Ballot!.ElectionId == id && !v.Ballot.IsSpoiled)
            .GroupBy(v => new { v.NominationId, v.Ballot!.ElectionSeatId }).Select(g => new { g.Key.NominationId, g.Key.ElectionSeatId, Count = g.Count() }).ToListAsync(ct);
        var results = rows.GroupBy(x => x.ElectionSeatId).SelectMany(g => { var max = g.Max(x => x.Count); var tie = g.Count(x => x.Count == max) > 1; return g.Select(x => new ElectionResultDto(x.ElectionSeatId, x.NominationId, x.Count, x.Count == max && !tie, tie)); }).ToList();
        _db.ElectionResults.RemoveRange(_db.ElectionResults.Where(x => x.ElectionId == id));
        _db.ElectionResults.AddRange(results.Select(x => new ElectionResult { ElectionId = id, ElectionSeatId = x.ElectionSeatId, NominationId = x.NominationId, VoteCount = x.VoteCount, IsElected = x.IsElected, IsTie = x.IsTie }));
        await _db.SaveChangesAsync(ct);
        return results;
    }

    public async Task<bool> DeclareAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Elections.FindAsync([id], ct);
        if (e == null || e.Phase != ElectionPhase.Counting) return false;
        var results = await CountAsync(id, ct);
        foreach (var r in results.Where(x => x.IsElected))
        {
            var n = await _db.Nominations.FindAsync([r.NominationId], ct);
            if (n != null) _db.ECMembers.Add(new ECMember { ECPeriodId = e.ECPeriodId, MemberId = n.CandidateMemberId, Position = (await _db.ElectionSeats.FindAsync([r.ElectionSeatId], ct))!.Position, StartDate = DateTime.UtcNow });
        }
        e.Phase = ElectionPhase.Declared; e.DeclaredOn = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<NominationViewDto>> GetNominationsAsync(int id, CancellationToken ct = default) =>
        (await _db.Nominations.Where(x => x.ElectionId == id).OrderBy(x => x.ElectionSeatId).ToListAsync(ct)).Select(ToNomination).ToList();

    private static ElectionSummaryDto ToSummary(Election e, int count, int eligible) => new(e.Id, e.Title, e.Phase, e.ECPeriodId, count, eligible);
    private static NominationViewDto ToNomination(Nomination n) => new(n.Id, n.ElectionSeatId, n.CandidateMemberId, n.Status, n.Statement);
}
