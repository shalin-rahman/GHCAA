using System;

namespace GHCAA.Domain.Models
{
    public class IssuedCredential
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public Enums.CredentialType CredentialType { get; set; }
        public string ShortCode { get; set; } = null!;
        public DateTime IssuedOn { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresOn { get; set; }
        public bool IsRevoked { get; set; }
        public string? RevokedReason { get; set; }
        public DateTime? RevokedOn { get; set; }
        public Member? Member { get; set; }
    }
}
