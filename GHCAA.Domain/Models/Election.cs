using System;
using System.Collections.Generic;
using static GHCAA.Domain.Enums;

namespace GHCAA.Domain.Models;

public class Election
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int ECPeriodId { get; set; }
    public ElectionPhase Phase { get; set; } = ElectionPhase.Announced;
    public DateTime AnnouncedOn { get; set; }
    public DateTime NominationOpensOn { get; set; }
    public DateTime NominationClosesOn { get; set; }
    public DateTime? ScrutinyOn { get; set; }
    public DateTime? WithdrawalClosesOn { get; set; }
    public DateTime PollingOpensOn { get; set; }
    public DateTime PollingClosesOn { get; set; }
    public DateTime? DeclaredOn { get; set; }
    public bool IsActive { get; set; } = true;
    public int CreatedBy { get; set; }
    public ECPeriod? ECPeriod { get; set; }
    public ICollection<ElectionSeat> Seats { get; set; } = new List<ElectionSeat>();
    public ICollection<ElectionOfficer> Officers { get; set; } = new List<ElectionOfficer>();
    public ICollection<VoterRoll> VoterRoll { get; set; } = new List<VoterRoll>();
}

public class ElectionSeat
{
    public int Id { get; set; }
    public int ElectionId { get; set; }
    public ECPosition Position { get; set; }
    public int SeatCount { get; set; } = 1;
    public Election? Election { get; set; }
    public ICollection<Nomination> Nominations { get; set; } = new List<Nomination>();
}

public class ElectionOfficer
{
    public int Id { get; set; }
    public int ElectionId { get; set; }
    public int MemberId { get; set; }
    public ElectionRole Role { get; set; }
    public Election? Election { get; set; }
    public Member? Member { get; set; }
}

public class VoterRoll
{
    public int Id { get; set; }
    public int ElectionId { get; set; }
    public int MemberId { get; set; }
    public bool IsEligible { get; set; }
    public string? IneligibilityReason { get; set; }
    public DateTime FrozenAt { get; set; }
    public DateTime? VotedAt { get; set; }
    public Election? Election { get; set; }
    public Member? Member { get; set; }
}

public class Nomination
{
    public int Id { get; set; }
    public int ElectionId { get; set; }
    public int ElectionSeatId { get; set; }
    public int CandidateMemberId { get; set; }
    public int ProposerMemberId { get; set; }
    public int SeconderMemberId { get; set; }
    public string Statement { get; set; } = null!;
    public string? PhotoPath { get; set; }
    public NominationStatus Status { get; set; } = NominationStatus.Submitted;
    public DateTime SubmittedAt { get; set; }
    public DateTime? WithdrawnAt { get; set; }
    public Election? Election { get; set; }
    public ElectionSeat? ElectionSeat { get; set; }
}

public class ScrutinyDecision
{
    public int Id { get; set; }
    public int NominationId { get; set; }
    public int OfficerMemberId { get; set; }
    public bool Accepted { get; set; }
    public string? Reason { get; set; }
    public DateTime DecidedAt { get; set; }
}

public class Ballot
{
    public int Id { get; set; }
    public int ElectionId { get; set; }
    public int ElectionSeatId { get; set; }
    public string SerialNumber { get; set; } = null!;
    public DateTime IssuedAt { get; set; }
    public bool IsSpoiled { get; set; }
    public Election? Election { get; set; }
    public ICollection<BallotVote> Votes { get; set; } = new List<BallotVote>();
}

public class BallotVote
{
    public int Id { get; set; }
    public int BallotId { get; set; }
    public int NominationId { get; set; }
    public DateTime CastAt { get; set; }
    public Ballot? Ballot { get; set; }
}

public class SeatVote
{
    public int Id { get; set; }
    public int ElectionId { get; set; }
    public int ElectionSeatId { get; set; }
    public int MemberId { get; set; }
    public DateTime VotedAt { get; set; }
    public Election? Election { get; set; }
}

public class ElectionResult
{
    public int Id { get; set; }
    public int ElectionId { get; set; }
    public int ElectionSeatId { get; set; }
    public int NominationId { get; set; }
    public int VoteCount { get; set; }
    public bool IsElected { get; set; }
    public bool IsTie { get; set; }
}
