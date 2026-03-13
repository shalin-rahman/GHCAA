using System;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs
{
    public class EventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime Date { get; set; }
        public string Location { get; set; } = null!;
        public decimal? RegistrationFee { get; set; }
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? RegistrationDeadline { get; set; }
        public string? AdminNote { get; set; }
    }

    public class CreateEventDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime Date { get; set; }
        public string Location { get; set; } = null!;
        public decimal? RegistrationFee { get; set; }
        public bool IsActive { get; set; } = true;
        public bool AllowNonMembers { get; set; } = false;
        public string? ImageUrl { get; set; }
        public DateTime? RegistrationDeadline { get; set; }
        public string? AdminNote { get; set; }
    }

    public class UpdateEventDto : CreateEventDto
    {
        public int Id { get; set; }
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
        public string PaymentReference { get; set; } = null!;
        public string? ReceiptPath { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
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
        public string PaymentReference { get; set; } = null!;
        public string? ReceiptPath { get; set; } // Path received after actual upload
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.ManualReceipt;
    }

    public class ApproveRegistrationDto
    {
        public int RegistrationId { get; set; }
        public bool Approve { get; set; } // True for approve, false for reject
    }
}
