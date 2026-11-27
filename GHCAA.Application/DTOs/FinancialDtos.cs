using GHCAA.Domain;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Application.DTOs
{
    public class PaymentHistoryDto
    {
        public int Id { get; set; }

        // Nullable: a guest event payment (AllowNonMembers) has no Member row.
        public int? MemberId { get; set; }
        public string TransactionId { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public Enums.PaymentStatus Status { get; set; }
        public Enums.FinancialCategory FinancialCategory { get; set; }
        public Enums.PaymentMethod PaymentMethod { get; set; }
        public string? Notes { get; set; }
    }

    // 84.92. One row of the admin queue of declared payments waiting for review.
    public class PendingPaymentDto
    {
        public int Id { get; set; }
        public int? MemberId { get; set; }
        public string? MemberName { get; set; }

        // A payment row has no due link, so this is the one unpaid membership due of the same
        // member and amount, or null when none or several fit.
        public int? DueId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionReference { get; set; } = null!;
        public string? ProofPath { get; set; }

        // The date the member declared they paid. The row keeps no separate created time.
        public DateTime SubmittedAt { get; set; }
    }

    public class RejectPaymentDto
    {
        [Required(ErrorMessage = "A reason is required.")]
        [MinLength(Constants.PaymentReview.MinReasonLength, ErrorMessage = "The reason must be at least 3 characters.")]
        [MaxLength(Constants.PaymentReview.MaxReasonLength, ErrorMessage = "The reason must not exceed 500 characters.")]
        public string Reason { get; set; } = null!;
    }

    public enum PaymentReviewOutcome { Done, NotFound, Settled, InvalidReason }

    public class PaymentReviewResult
    {
        public PaymentReviewOutcome Outcome { get; set; }

        // Verify only: the due marked paid, or null when no single due matched.
        public int? DueId { get; set; }
    }

    public class CreatePaymentHistoryDto
    {
        [Required(ErrorMessage = "Transaction ID is required.")]
        [MaxLength(200, ErrorMessage = "Transaction ID must not exceed 200 characters.")]
        public string TransactionId { get; set; } = null!;

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment date is required.")]
        public DateTime PaidAt { get; set; }

        public Enums.FinancialCategory FinancialCategory { get; set; }
        public Enums.PaymentMethod PaymentMethod { get; set; }

        [MaxLength(500, ErrorMessage = "Notes must not exceed 500 characters.")]
        public string? Notes { get; set; }

        public int? MemberId { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile? Receipt { get; set; }
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
        public Enums.FinancialCategory Category { get; set; }
        public string MembershipType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class CreateMembershipFeeConfigDto : IValidatableObject
    {
        public Enums.FinancialCategory Category { get; set; } = Enums.FinancialCategory.MembershipFee;

        [Required(ErrorMessage = "Membership type is required.")]
        [MaxLength(100)]
        public string MembershipType { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Fee amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Effective date is required.")]
        public DateTime EffectiveDate { get; set; }

        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; } = true;

        [MaxLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EffectiveTo.HasValue && EffectiveTo.Value <= EffectiveDate)
                yield return new ValidationResult(
                    "'Effective To' date must be after the 'Effective From' date.",
                    new[] { nameof(EffectiveTo) });
        }
    }

    public class UpdateMembershipFeeConfigDto : IValidatableObject
    {
        public int Id { get; set; }
        public Enums.FinancialCategory? Category { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Fee amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Effective date is required.")]
        public DateTime EffectiveDate { get; set; }

        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; }

        [MaxLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EffectiveTo.HasValue && EffectiveTo.Value <= EffectiveDate)
                yield return new ValidationResult(
                    "'Effective To' date must be after the 'Effective From' date.",
                    new[] { nameof(EffectiveTo) });
        }
    }
}
