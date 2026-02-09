using System;
using static GHCAA.Domain.Enums;

namespace GHCAA.Domain.Models
{
    public class PaymentHistory
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public string TransactionId { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? Notes { get; set; }

        // Navigation
        public Member? Member { get; set; }
    }
}
