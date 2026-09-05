namespace GHCAA.Domain.Models
{
    // TODO 37.3: admin-configurable so tier names/thresholds are not compiled in. The honour roll
    // groups confirmed pledges by the highest tier a donor's SUM(AmountReceived) across a campaign
    // qualifies for.
    public class DonorRecognitionTier
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal MinimumAmount { get; set; }
        public string? Description { get; set; }
    }
}
