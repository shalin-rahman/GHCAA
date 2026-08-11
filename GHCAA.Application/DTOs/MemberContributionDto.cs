namespace GHCAA.Application.DTOs
{
    public class MemberContributionDto
    {
        public int MemberId { get; set; }
        public string FullName { get; set; } = null!;
        public string? MembershipNumber { get; set; }
        public string? PhotoPath { get; set; }
        public int ContributionPoints { get; set; }
        public int Rank { get; set; }
    }
}
