using System;
using System.ComponentModel.DataAnnotations;
using static GHCAA.Domain.Enums;

namespace GHCAA.Domain.Models
{
    public class EventRegistration
    {
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        public int? MemberId { get; set; } // Nullable for non-members

        public bool IsNonMember { get; set; } = false;

        [MaxLength(200)]
        public string? GuestName { get; set; }

        [MaxLength(100)]
        public string? GuestEmail { get; set; }

        [MaxLength(20)]
        public string? GuestMobile { get; set; }

        [MaxLength(100)]
        public string? PaymentReference { get; set; }

        [MaxLength(500)]
        public string? ReceiptPath { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.ManualReceipt;

        [Required]
        public EventRegistrationStatus Status { get; set; } = EventRegistrationStatus.Pending;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public DateTime? ApprovedAt { get; set; }

        public int? ApprovedByAdminId { get; set; }

        // Navigation
        public AlumniEvent? Event { get; set; }
        public Member? Member { get; set; }
    }
}
