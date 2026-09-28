using System;
using System.Collections.Generic;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs;

public record CreateElectionDto(string Title, int ECPeriodId, DateTime NominationOpensOn, DateTime NominationClosesOn, DateTime PollingOpensOn, DateTime PollingClosesOn, int CreatedBy, ElectionTieRule TieRule = ElectionTieRule.DrawingLots);
public record CreateAdminElectionRequest(string Title, int ECPeriodId, DateTime NominationOpensOn, DateTime NominationClosesOn, DateTime PollingOpensOn, DateTime PollingClosesOn, int CreatedBy, string? Description, IReadOnlyList<AdminElectionPositionRequest>? Positions, ElectionTieRule TieRule = ElectionTieRule.DrawingLots);
// Spec 023 FR-001. The returning officer's public key, base64 SPKI. The private half never reaches the server until the count.
public record SetBallotKeyRequest(string PublicKey);
// The private key, base64 PKCS#8. It is used for the count and not stored. Left out once the count has run, to read the stored results.
public record CountElectionRequest(string? PrivateKey);
public record AdminElectionPositionRequest(string Title, int Seats = 1, string? Description = null);
public record SaveCandidateRequest(int MemberId, int PositionId, string? Statement, int ProposerMemberId, int SeconderMemberId);
public record ElectionSeatDto(ECPosition Position, int SeatCount);
public record ElectionOfficerDto(int MemberId, ElectionRole Role);
public record ElectionSeatRequestDto(ECPosition Position, int SeatCount = 1);
public record NominationDto(int ElectionSeatId, int CandidateMemberId, int ProposerMemberId, int SeconderMemberId, string Statement, string? PhotoPath);
public record ScrutinyDto(bool Accepted, string? Reason);
// Spec 023 FR-005: the whole paper in one request. Every seat appears once. An empty
// NominationIds list is an abstention for that seat.
public record CastBallotDto(IReadOnlyList<BallotSeatChoiceDto> Seats);
public record BallotSeatChoiceDto(int ElectionSeatId, IReadOnlyList<int> NominationIds);
public record CastBallotResultDto(string TrackingCode);
public record ElectionSummaryDto(int Id, string Title, ElectionPhase Phase, int ECPeriodId, int VoterCount, int EligibleVoterCount);
public record AdminElectionPositionDto(int Id, string Title, string? Description, int Seats);
public record AdminElectionCandidateDto(int Id, int MemberId, string Name, string? PhotoUrl, string? Statement, int PositionId, string PositionTitle);
public record AdminElectionDto(int Id, string Title, string? Description, ElectionPhase Phase, DateTime? AnnouncedOn, DateTime? NominationOpensOn, DateTime? NominationClosesOn, DateTime? ScrutinyOn, DateTime? WithdrawalClosesOn, DateTime? PollingOpensOn, DateTime? PollingClosesOn, DateTime? DeclaredOn, bool IsActive, IReadOnlyList<AdminElectionPositionDto> Positions, IReadOnlyList<AdminElectionCandidateDto> Candidates, int EligibleVoterCount, bool HasVoted, ElectionTieRule TieRule, string? BallotKeyFingerprint = null);
public record NominationViewDto(int Id, int ElectionSeatId, int CandidateMemberId, NominationStatus Status, string Statement);
public record ElectionResultDto(int ElectionSeatId, int NominationId, int VoteCount, bool IsElected, bool IsTie);
