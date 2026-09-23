using System;
using System.Collections.Generic;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs;

public record CreateElectionDto(string Title, int ECPeriodId, DateTime NominationOpensOn, DateTime NominationClosesOn, DateTime PollingOpensOn, DateTime PollingClosesOn, int CreatedBy);
public record CreateAdminElectionRequest(string Title, int ECPeriodId, DateTime NominationOpensOn, DateTime NominationClosesOn, DateTime PollingOpensOn, DateTime PollingClosesOn, int CreatedBy, string? Description, IReadOnlyList<AdminElectionPositionRequest>? Positions);
public record AdminElectionPositionRequest(string Title, int Seats = 1, string? Description = null);
public record SaveCandidateRequest(int MemberId, int PositionId, string? Statement);
public record ElectionSeatDto(ECPosition Position, int SeatCount);
public record ElectionOfficerDto(int MemberId, ElectionRole Role);
public record ElectionSeatRequestDto(ECPosition Position, int SeatCount = 1);
public record NominationDto(int ElectionSeatId, int CandidateMemberId, int ProposerMemberId, int SeconderMemberId, string Statement, string? PhotoPath);
public record ScrutinyDto(bool Accepted, string? Reason);
public record CastVoteDto(int ElectionSeatId, int NominationId, string? SerialNumber);
public record ElectionSummaryDto(int Id, string Title, ElectionPhase Phase, int ECPeriodId, int VoterCount, int EligibleVoterCount);
public record AdminElectionPositionDto(int Id, string Title, string? Description, int Seats);
public record AdminElectionCandidateDto(int Id, int MemberId, string Name, string? PhotoUrl, string? Statement, int PositionId, string PositionTitle);
public record AdminElectionDto(int Id, string Title, string? Description, ElectionPhase Phase, DateTime? AnnouncedOn, DateTime? NominationOpensOn, DateTime? NominationClosesOn, DateTime? ScrutinyOn, DateTime? WithdrawalClosesOn, DateTime? PollingOpensOn, DateTime? PollingClosesOn, DateTime? DeclaredOn, bool IsActive, IReadOnlyList<AdminElectionPositionDto> Positions, IReadOnlyList<AdminElectionCandidateDto> Candidates, int EligibleVoterCount, bool HasVoted);
public record NominationViewDto(int Id, int ElectionSeatId, int CandidateMemberId, NominationStatus Status, string Statement);
public record ElectionResultDto(int ElectionSeatId, int NominationId, int VoteCount, bool IsElected, bool IsTie);
