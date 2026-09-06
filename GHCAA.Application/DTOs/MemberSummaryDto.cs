using System;
using System.Collections.Generic;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs
{
    public class MemberSummaryDto : BaseMemberDto
    {
        public int Id { get; set; }
        public string? MembershipNumber { get; set; }
        public MembershipStatus Status { get; set; }
        public MembershipType MembershipType { get; set; }
        public MemberCategory Category { get; set; }
        public bool IsVerified { get; set; }
        public bool IsArchived { get; set; }
        public int ContributionPoints { get; set; }
        public DateTime AppliedDate { get; set; }
        public bool IsFamilyPublic { get; set; }
        public List<MemberFamilyDto> FamilyMembers { get; set; } = new();

        // Directory Fields (Flattened current status)
        public int? PassingYear { get; set; }
        public string? Degree { get; set; }
        public string? Subject { get; set; }

        // Gamification
        public int Rank { get; set; }
        public string? CategoryBadge { get; set; }
        public decimal ProfileCompletionPercentage { get; set; }
    }
}
