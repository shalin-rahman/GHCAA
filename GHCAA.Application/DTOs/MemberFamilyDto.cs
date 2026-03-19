using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs
{
    public class MemberFamilyDto
    {
        public int RequestId { get; set; }
        public int MemberId { get; set; }
        public string FullName { get; set; } = null!;
        public string? MembershipNumber { get; set; }
        public string? PhotoPath { get; set; }
        public RelationshipType Relationship { get; set; }
        public FamilyLinkStatus Status { get; set; }
        public bool IsVerified { get; set; }
        public bool IsRequester { get; set; }
    }
}
