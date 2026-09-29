using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

// Spec 023 (37.12b).
public record ElectionPersonaDto(int Id, string Name, string GroupName, string Description, ElectionPermission Permissions, int MinCount, int? MaxCount, bool ShowOnPublicBoard, bool TakesOverFromAdmin, string DeclarationText, int SortOrder, bool IsActive);

public class SaveElectionPersonaDto : IValidatableObject
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required, MaxLength(100)]
    public string GroupName { get; set; } = null!;

    [Required, MaxLength(500)]
    public string Description { get; set; } = null!;

    public ElectionPermission Permissions { get; set; }

    [Range(0, int.MaxValue)]
    public int MinCount { get; set; }

    public int? MaxCount { get; set; }

    public bool ShowOnPublicBoard { get; set; }

    public bool TakesOverFromAdmin { get; set; }

    [Required, MaxLength(4000)]
    public string DeclarationText { get; set; } = null!;

    public int SortOrder { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MaxCount is int max && max < MinCount)
            yield return new ValidationResult("MaxCount cannot be less than MinCount.", new[] { nameof(MaxCount) });
    }
}

// Spec 023 (37.12d). Name either a member, or a name and email for someone outside the membership.
public class AppointDto : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int PersonaId { get; set; }

    [Range(1, int.MaxValue)]
    public int? MemberId { get; set; }

    [MaxLength(150)]
    public string? DisplayName { get; set; }

    [EmailAddress, MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var hasContact = !string.IsNullOrWhiteSpace(DisplayName) || !string.IsNullOrWhiteSpace(Email);
        if (MemberId is not null && hasContact)
            yield return new ValidationResult("Give a member or a name and email, not both.", new[] { nameof(MemberId) });
        else if (MemberId is null && (string.IsNullOrWhiteSpace(DisplayName) || string.IsNullOrWhiteSpace(Email)))
            yield return new ValidationResult("A name and email are required when no member is chosen.", new[] { nameof(Email) });
    }
}

public class AcceptAppointmentDto
{
    public bool AgreeToDeclaration { get; set; }
}

public class AppointmentReasonDto
{
    [MaxLength(400)]
    public string? Reason { get; set; }
}

public record ElectionAppointmentDto(int Id, int ElectionId, string ElectionTitle, int PersonaId, string PersonaName, string DeclarationText, int UserId, int? MemberId, string DisplayName, string Email, string? Phone, DateTime AppointedAt, DateTime? AcceptedAt, DateTime? DeclarationSignedAt, DateTime? RevokedAt, string? RevokedReason, DateTime? ExpiresAt, bool IsLive);

// What /api/auth/me returns for each live appointment.
public record ElectionAppointmentSummaryDto(int ElectionId, string ElectionTitle, string PersonaName, ElectionPermission Permissions);
