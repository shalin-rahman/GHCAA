using System;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Application.DTOs
{
    public class MemberRegistrationDto : BaseMemberDto
    {
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the Terms & Conditions.")]
        public bool HasAcceptedTerms { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the Data Privacy (GDPR) policy.")]
        public bool HasAcceptedGdpr { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A valid payment method must be selected.")]
        public int PaymentMethodId { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(200, ErrorMessage = "Transaction ID must not exceed 200 characters.")]
        public string? TransactionId { get; set; }
    }
}