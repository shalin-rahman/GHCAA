using static GHCAA.Domain.Enums;

namespace GHCAA.Domain.Models
{
    /// <summary>
    /// Represents a request sent by one member to link another member as a family/spouse.
    /// The target member must approve before the link is established.
    /// </summary>
    public class FamilyLinkRequest
    {
        public int Id { get; set; }

        // Who sent the request
        public int RequesterId { get; set; }
        public Member? Requester { get; set; }

        // Who needs to approve
        public int TargetMemberId { get; set; }
        public Member? TargetMember { get; set; }

        public RelationshipType Relationship { get; set; }

        public FamilyLinkStatus Status { get; set; } = FamilyLinkStatus.Requested;

        public string? Note { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
    }
}
