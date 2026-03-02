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

        [Required]
        public int MemberId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PaymentReference { get; set; } = null!;

        [MaxLength(500)]
        public string? ReceiptPath { get; set; }

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
