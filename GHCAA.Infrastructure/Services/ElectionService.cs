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
            Title = request.Title,
            ECPeriodId = request.ECPeriodId,
            CreatedBy = request.CreatedBy,
            AnnouncedOn = DateTime.UtcNow,
            NominationOpensOn = request.NominationOpensOn.ToUniversalTime(),
            NominationClosesOn = request.NominationClosesOn.ToUniversalTime(),
            PollingOpensOn = request.PollingOpensOn.ToUniversalTime(),
            PollingClosesOn = request.PollingClosesOn.ToUniversalTime()
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
        _db.SeatVotes.Add(new SeatVote { ElectionId = id, ElectionSeatId = request.ElectionSeatId, MemberId = memberId, VotedAt = votedAt });
        if (voter.VotedAt == null) voter.VotedAt = votedAt;
        var ballot = new Ballot { ElectionId = id, ElectionSeatId = request.ElectionSeatId, SerialNumber = request.SerialNumber ?? Guid.NewGuid().ToString("N"), IssuedAt = DateTime.UtcNow };
        _db.Ballots.Add(ballot);
        _db.BallotVotes.Add(new BallotVote { Ballot = ballot, NominationId = nomination.Id, CastAt = DateTime.UtcNow });
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(ct);
            return false;
        }
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
        if (!await _db.ElectionSeats.AnyAsync(x => x.ElectionId == id && x.Id == request.PositionId, ct))
            return (false, "seat-not-found", null);

        _db.Nominations.Add(new Nomination
        {
            ElectionId = id,
            ElectionSeatId = request.PositionId,
            CandidateMemberId = request.MemberId,
            ProposerMemberId = request.MemberId,
            SeconderMemberId = request.MemberId,
            Statement = request.Statement ?? string.Empty,
            Status = NominationStatus.Accepted,
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
        var nomination = await _db.Nominations.FirstOrDefaultAsync(x => x.ElectionId == id && x.Id == candidateId, ct);
        if (nomination is null) return (false, "not-found");

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
            election.VoterRoll.Any(x => x.VotedAt.HasValue));
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
