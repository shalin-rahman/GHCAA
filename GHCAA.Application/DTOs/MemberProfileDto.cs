using System;
using System.Collections.Generic;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs
{
    public class MemberProfileDto : BaseMemberDto
    {
        public int Id { get; set; }
        public string? MembershipNumber { get; set; }
        public MembershipStatus Status { get; set; }
        public MemberCategory Category { get; set; }
        public DateTime AppliedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        
        public bool IsVerified { get; set; }
        public string? PrimaryMemberNumber { get; set; }
        public List<MemberSummaryDto> Dependents { get; set; } = new();
        public List<MemberFamilyDto> FamilyMembers { get; set; } = new();
        public bool IsFamilyPublic { get; set; }
    }
}
