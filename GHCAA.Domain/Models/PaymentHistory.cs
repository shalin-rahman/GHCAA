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
        public FinancialCategory FinancialCategory { get; set; } = FinancialCategory.Other;
        public string? Notes { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.ManualReceipt;
        [MaxLength(500)]
        public string? ReceiptPath { get; set; }

        // 24.13: Stores the gateway's own transaction identifier (bKash paymentID / SSLCommerz val_id).
        // UNIQUE partial index (WHERE NOT NULL) prevents duplicate callback processing.
        [MaxLength(255)]
        public string? GatewayPaymentId { get; set; }

        // Navigation
        public Member? Member { get; set; }
    }
}
