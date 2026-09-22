using System;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs
{
    public sealed class CredentialVerificationDto
    {
        public bool Valid { get; init; }
        public string? MemberName { get; init; }
        public MembershipType? MembershipType { get; init; }
        public DateTime? IssuedOn { get; init; }
        public string Status { get; init; } = null!;
    }
}
