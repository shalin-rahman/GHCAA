using System;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Domain.Models
{
    public class MembershipDue
    {
        public int Id { get; set; }

        [Required]
        public int MemberId { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public bool IsPaid { get; set; } = false;

        public DateTime? PaymentDate { get; set; }

        public int? PaymentHistoryId { get; set; }

        // Navigation
        public Member? Member { get; set; }
        public PaymentHistory? PaymentHistory { get; set; }
    }
}
