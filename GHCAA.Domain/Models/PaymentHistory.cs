using System;
using System.ComponentModel.DataAnnotations;
using static GHCAA.Domain.Enums;

namespace GHCAA.Domain.Models
{
    public class PaymentHistory
    {
        public int Id { get; set; }

        // Nullable: a guest paying an event fee for an event with AllowNonMembers has no Member
        // row (EventRegistration.MemberId is nullable for exactly this reason). Attributing a
        // guest payment to a fabricated member id would either crash on the foreign key or, worse,
        // silently attach real money to the wrong member.
        public int? MemberId { get; set; }
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

        // 82.16. Same reasoning as FinancialRecord: a payment row is the record that money was
        // received. Status moves Pending -> Completed/Failed/Refunded with no history table behind
        // it, so without these the previous state and the admin who changed it are simply gone.
        // Deletes are soft for the same reason — a deleted payment is evidence too.
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedByAdminId { get; set; }
        public bool IsArchived { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByAdminId { get; set; }

        // Navigation
        public Member? Member { get; set; }
    }
}
