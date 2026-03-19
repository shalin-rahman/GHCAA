using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs
{
    public class FamilyLinkRequestDto
    {
        public int Id { get; set; }
        public int RequesterId { get; set; }
        public string RequesterName { get; set; } = null!;
        public string? RequesterMembershipNumber { get; set; }
        public string? RequesterPhotoPath { get; set; }

        public int TargetMemberId { get; set; }
        public string TargetMemberName { get; set; } = null!;
        public string? TargetMembershipNumber { get; set; }

        public RelationshipType Relationship { get; set; }
        public FamilyLinkStatus Status { get; set; }
        public string? Note { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }

    public class SendFamilyLinkDto
    {
        /// <summary>Membership number of the member to link with</summary>
        public string TargetMembershipNumber { get; set; } = null!;
        public RelationshipType Relationship { get; set; }
        public string? Note { get; set; }
    }

    public class RespondFamilyLinkDto
    {
        public int RequestId { get; set; }
        public bool Approve { get; set; }
    }
}
