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
    public ElectionTieRule TieRule { get; set; } = ElectionTieRule.DrawingLots;
    // The returning officer's public key (SPKI, base64). Choices are sealed under it at cast and
    // only the officer's private key, brought to the count, opens them.
    public string? BallotPublicKey { get; set; }
    public string? BallotKeyFingerprint { get; set; }
    // Which BallotSeal format this election's ballots use. Elections that existed before the
    // version 2 format were set to 1 by the migration, so their ballots still open.
    public int BallotSealVersion { get; set; } = Constants.Elections.BallotSealVersion;
    public bool IsActive { get; set; } = true;
    public int CreatedBy { get; set; }
    public ECPeriod? ECPeriod { get; set; }
    public ICollection<ElectionSeat> Seats { get; set; } = new List<ElectionSeat>();
    public ICollection<ElectionAppointment> Appointments { get; set; } = new List<ElectionAppointment>();
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

// Spec 023 (37.12d). One person holding one persona on one election. The person always has a
// User row; MemberId is set only when they are also a member. DisplayName and Email are copied
// at appointment time so the record still reads correctly if the member changes their details.
public class ElectionAppointment
{
    public int Id { get; set; }
    public int ElectionId { get; set; }
    public int PersonaId { get; set; }
    public int UserId { get; set; }
    public int? MemberId { get; set; }
    public string DisplayName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public int AppointedByUserId { get; set; }
    public DateTime AppointedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? DeclarationSignedAt { get; set; }
    public string? DeclarationTextSnapshot { get; set; }
    public string? SignedFromIp { get; set; }
    public DateTime? RevokedAt { get; set; }
    public int? RevokedByUserId { get; set; }
    public string? RevokedReason { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Election? Election { get; set; }
    public ElectionPersona? Persona { get; set; }
    public User? User { get; set; }

    // Only a live appointment grants anything. Keep this in step with LiveAt below, which is the
    // same rule written so EF can translate it to SQL.
    public bool IsLive(DateTime now) =>
        AcceptedAt != null && DeclarationSignedAt != null && RevokedAt == null && (ExpiresAt == null || ExpiresAt > now);

    public static System.Linq.Expressions.Expression<Func<ElectionAppointment, bool>> LiveAt(DateTime now) =>
        a => a.AcceptedAt != null && a.DeclarationSignedAt != null && a.RevokedAt == null && (a.ExpiresAt == null || a.ExpiresAt > now);
}

// Spec 023 (37.12f). A sensitive step held until a second person with Approve agrees. The row
// itself records who asked, who decided and when. PayloadJson carries what the step needs, such as
// the new ballot key, so the approver sees exactly what they approve.
public class ElectionApproval
{
    public int Id { get; set; }
    public int ElectionId { get; set; }
    public ElectionApprovalAction Action { get; set; }
    public string? PayloadJson { get; set; }
    public int RequestedByUserId { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? RejectedByUserId { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectReason { get; set; }
    public DateTime? ExecutedAt { get; set; }
    // Count only (37.13h). Approving a count does not run it, because the key is never stored.
    // The count claims the approved row by setting this, so one approval allows one count.
    public DateTime? ConsumedAt { get; set; }
    public Election? Election { get; set; }

    public bool IsOpenAt(DateTime now) => ExecutedAt == null && RejectedAt == null && ExpiresAt > now;
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
    public int DecidedByUserId { get; set; }
    public bool Accepted { get; set; }
    public string? Reason { get; set; }
    public DateTime DecidedAt { get; set; }
}

// Ballot and BallotVote hold what was voted, never who voted. Keys are random GUIDs set in
// code and there is no time column, so neither key order nor a timestamp can be matched to a
// SeatVote row. Rows arrive here only at the count, all at once and shuffled.
public class Ballot
{
    public Guid Id { get; set; }
    public int ElectionId { get; set; }
    public bool IsSpoiled { get; set; }
    public Election? Election { get; set; }
    public ICollection<BallotVote> Votes { get; set; } = new List<BallotVote>();
}

// One row per chosen candidate. A seat with no row on a ballot is an abstention.
public class BallotVote
{
    public Guid Id { get; set; }
    public Guid BallotId { get; set; }
    public int ElectionSeatId { get; set; }
    public int NominationId { get; set; }
    public Ballot? Ballot { get; set; }
}

// Holding row written in the same transaction as the voter's SeatVote rows. It has no member
// and no time column. SealedChoices can only be opened with the returning officer's private
// key, and the row is deleted at the count.
public class PendingBallot
{
    public Guid Id { get; set; }
    public int ElectionId { get; set; }
    public string SealedChoices { get; set; } = null!;
}

// The tracking codes handed to voters. They sit apart from the ballots, so a receipt shows a
// ballot was received but cannot be matched to the choices on it.
public class BallotReceipt
{
    public Guid Id { get; set; }
    public int ElectionId { get; set; }
    public string TrackingCode { get; set; } = null!;
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
