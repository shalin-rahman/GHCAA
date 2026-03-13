using System;
using System.ComponentModel.DataAnnotations;
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
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.ManualReceipt;
        [MaxLength(500)]
        public string? ReceiptPath { get; set; }

        // Navigation
        public Member? Member { get; set; }
    }
}
