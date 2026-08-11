using System;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs
{
    public class AdminMemberUpdateDto : BaseMemberDto
    {
        // Membership Status & Other Metadata 
        public MembershipStatus Status { get; set; }
        public MembershipType MembershipType { get; set; }
        public MemberCategory Category { get; set; }
        public bool IsVerified { get; set; }
        public int ContributionPoints { get; set; }
        public string? MembershipNumber { get; set; }
        public string? ECChangeReason { get; set; }
        public string? MembershipChangeReason { get; set; }
    }
}
