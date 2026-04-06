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
        public MembershipType MembershipType { get; set; }
        public MemberCategory Category { get; set; }
        public DateTime AppliedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        
        public bool IsVerified { get; set; }
        public string? PrimaryMemberNumber { get; set; }
        public List<MemberSummaryDto> Dependents { get; set; } = new();
        public List<MemberFamilyDto> FamilyMembers { get; set; } = new();
        public bool IsFamilyPublic { get; set; }

        // Gamification & Health
        public int ContributionPoints { get; set; }
        public int Rank { get; set; }
        public decimal ProfileCompletionPercentage { get; set; }
        
        public bool HasAcceptedTerms { get; set; }
        public bool HasAcceptedGdpr { get; set; }

        // Summary Data for easier display
        public int? PassingYear { get; set; }
        public string? Degree { get; set; }
        public string? Subject { get; set; }
        public string? CategoryBadge { get; set; }
    }
}
