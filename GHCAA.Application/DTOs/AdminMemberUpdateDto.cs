using System;

namespace GHCAA.Application.DTOs
{
    public class AdminMemberUpdateDto : BaseMemberDto
    {
        public string Gender { get; set; } = null!;
        public string BloodGroup { get; set; } = null!;
        
        // Membership Status & Type
        public string MembershipType { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string? MembershipNumber { get; set; }
        
        public string? ECChangeReason { get; set; }
    }
}
