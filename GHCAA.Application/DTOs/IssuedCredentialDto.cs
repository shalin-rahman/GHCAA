using System.ComponentModel.DataAnnotations;
using GHCAA.Domain;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs
{
    // 84.68: one row of a member's credential list on the admin member page.
    public sealed class IssuedCredentialDto
    {
        public int MemberId { get; init; }
        public string ShortCode { get; init; } = null!;
        public CredentialType CredentialType { get; init; }
        public DateTime IssuedOn { get; init; }
        public DateTime? ExpiresOn { get; init; }
        public bool IsRevoked { get; init; }
        public string? RevokedReason { get; init; }
        public DateTime? RevokedOn { get; init; }
    }

    public sealed class CredentialActionRequest
    {
        [Required, MaxLength(Constants.Credentials.ReasonMaxLength)]
        public string Reason { get; set; } = string.Empty;
    }
}
