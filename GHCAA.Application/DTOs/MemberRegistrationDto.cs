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

        // 35.5: no MembershipType here on purpose. The tier is assigned by an admin only, so the
        // registration payload must not be able to carry one — RegisterAsync applies the org
        // config's DefaultMembershipType and an admin changes it later (audited).
        public GHCAA.Domain.Enums.MemberCategory Category { get; set; } = GHCAA.Domain.Enums.MemberCategory.None;

        [System.ComponentModel.DataAnnotations.MaxLength(200, ErrorMessage = "Transaction ID must not exceed 200 characters.")]
        public string? TransactionId { get; set; }
    }
}