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
        public bool IsVerified { get; set; }
        public DateTime AppliedDate { get; set; }
        public MemberCategory Category { get; set; }
        public bool IsFamilyPublic { get; set; }
        public List<MemberFamilyDto> FamilyMembers { get; set; } = new();

        // Directory Fields (Flattened current status)
        public int? PassingYear { get; set; }
        public string? Degree { get; set; }
        public string? Subject { get; set; }
        public string? Designation { get; set; }
        public string? OrganizationName { get; set; }
        public string? ProfessionalSector { get; set; }

        // Gamification
        public int ContributionPoints { get; set; }
        public int Rank { get; set; }
        public string? CategoryBadge { get; set; }
    }
}
