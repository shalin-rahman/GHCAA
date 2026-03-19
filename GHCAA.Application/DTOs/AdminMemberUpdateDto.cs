using System;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs
{
    public class AdminMemberUpdateDto : BaseMemberDto
    {
        // Membership Status & Type
        public MembershipStatus Status { get; set; }
        public MemberCategory Category { get; set; }
        public string? MembershipNumber { get; set; }
        
        public string? ECChangeReason { get; set; }
    }
}
