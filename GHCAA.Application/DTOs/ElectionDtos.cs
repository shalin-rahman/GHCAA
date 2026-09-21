using System;
using System.Collections.Generic;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs;

public record CreateElectionDto(string Title, int ECPeriodId, DateTime NominationOpensOn, DateTime NominationClosesOn, DateTime PollingOpensOn, DateTime PollingClosesOn, int CreatedBy);
public record ElectionSeatDto(ECPosition Position, int SeatCount);
public record ElectionOfficerDto(int MemberId, ElectionRole Role);
public record ElectionSeatRequestDto(ECPosition Position, int SeatCount = 1);
public record NominationDto(int ElectionSeatId, int CandidateMemberId, int ProposerMemberId, int SeconderMemberId, string Statement, string? PhotoPath);
public record ScrutinyDto(bool Accepted, string? Reason, int OfficerMemberId);
public record CastVoteDto(int ElectionSeatId, int NominationId, string? SerialNumber);
public record ElectionSummaryDto(int Id, string Title, ElectionPhase Phase, int ECPeriodId, int VoterCount, int EligibleVoterCount);
public record NominationViewDto(int Id, int ElectionSeatId, int CandidateMemberId, NominationStatus Status, string Statement);
public record ElectionResultDto(int ElectionSeatId, int NominationId, int VoteCount, bool IsElected, bool IsTie);
