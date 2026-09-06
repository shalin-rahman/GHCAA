using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs
{
    public class EventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; } = null!;
        public decimal? RegistrationFee { get; set; }
        public bool RequiresPayment { get; set; }
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationEndDate { get; set; }
        public string? AdminNote { get; set; }
        public int ParticipantCount { get; set; }
        public bool RequiresRegistration { get; set; }

        // 82.32: were write-only (CreateEventDto/UpdateEventDto only) — the admin edit form reads
        // these back from the event it just fetched, so their absence here meant every edit-then-
        // save round trip silently wiped whatever limit/waitlist setting had been set at creation.
        public int? ParticipantLimit { get; set; }
        public bool HasWaitlist { get; set; }
    }

    public class CreateEventDto : IValidatableObject
    {
        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string Location { get; set; } = null!;

        public decimal? RegistrationFee { get; set; }
        public bool RequiresPayment { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public bool AllowNonMembers { get; set; } = false;
        public string? ImageUrl { get; set; }
        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationEndDate { get; set; }
        public string? AdminNote { get; set; }

        public int? ParticipantLimit { get; set; }
        public bool HasWaitlist { get; set; } = false;

        public bool RequiresRegistration { get; set; } = true;

        // 82.52: admin per-action choice of whether creating this event broadcasts a notification.
        // Defaults true because that's what CreateEventAsync already did (gated on IsActive) before
        // this item existed — an admin who doesn't touch this field sees unchanged behavior.
        public bool NotifyMembers { get; set; } = true;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // 1. End date must be strictly after start date
            if (EndDate <= StartDate)
                yield return new ValidationResult(
                    "Event end date must be after the start date.",
                    new[] { nameof(EndDate) });

            // 2. Registration window: close must be after open
            if (RegistrationStartDate.HasValue && RegistrationEndDate.HasValue
                && RegistrationEndDate.Value <= RegistrationStartDate.Value)
                yield return new ValidationResult(
                    "Registration close date must be after the registration open date.",
                    new[] { nameof(RegistrationEndDate) });

            // 3. Registration should close on or before the event starts
            if (RegistrationEndDate.HasValue && RegistrationEndDate.Value > StartDate)
                yield return new ValidationResult(
                    "Registration must close on or before the event start date.",
                    new[] { nameof(RegistrationEndDate) });
        }
    }

    public class UpdateEventDto : CreateEventDto
    {
        public int Id { get; set; }

        // 82.52: UpdateEventAsync never sent a notification before this item, unlike create — a
        // distinct property (not the inherited NotifyMembers, which stays true by default for
        // Create's own semantics) so an update request that omits this field keeps that "never"
        // default rather than inheriting Create's "true".
        public bool NotifyOnUpdate { get; set; } = false;
    }

    public class EventRegistrationDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; } = null!;
        public int? MemberId { get; set; }
        public string? MemberName { get; set; }
        public bool IsNonMember { get; set; }
        public string? GuestName { get; set; }
        public string? GuestEmail { get; set; }
        public string? GuestMobile { get; set; }
        public string? PaymentReference { get; set; }
        public string? ReceiptPath { get; set; }
        public decimal? ContributionAmount { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public EventRegistrationStatus Status { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    public class RegisterForEventDto
    {
        public int EventId { get; set; }
        public bool IsNonMember { get; set; }
        public string? GuestName { get; set; }
        public string? GuestEmail { get; set; }
        public string? GuestMobile { get; set; }
        public string? PaymentReference { get; set; }
        public string? ReceiptPath { get; set; } // Path received after actual upload
        public decimal? ContributionAmount { get; set; }
        public PaymentMethod? PaymentMethod { get; set; } = GHCAA.Domain.Enums.PaymentMethod.ManualReceipt;
    }

    public class ApproveRegistrationDto
    {
        public int RegistrationId { get; set; }
        public bool Approve { get; set; } // True for approve, false for reject
    }

    public class PublicParticipantDto
    {
        public string Name { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime RegisteredAt { get; set; }
    }
}

