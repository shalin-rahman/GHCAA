using System;
using GHCAA.Domain;

namespace GHCAA.Application.DTOs
{
    public class PaymentHistoryDto
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public string TransactionId { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public Enums.PaymentStatus Status { get; set; }
        public Enums.FinancialCategory FinancialCategory { get; set; }
        public string? Notes { get; set; }
    }

    public class CreatePaymentHistoryDto
    {
        public string TransactionId { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public Enums.FinancialCategory FinancialCategory { get; set; }
        public string? Notes { get; set; }
        // Admin might set this, otherwise ignored
        public int? MemberId { get; set; }
    }

    public class MembershipHistoryDto
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public string OldType { get; set; } = null!;
        public string NewType { get; set; } = null!;
        public DateTime ChangeDate { get; set; }
        public string? Reason { get; set; }
        public int? ChangedByAdminId { get; set; }
    }

    public class MembershipDueDto
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class MembershipFeeConfigDto
    {
        public int Id { get; set; }
        public string MembershipType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class CreateMembershipFeeConfigDto
    {
        public string MembershipType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateMembershipFeeConfigDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
