namespace GHCAA.Domain.Models
{
    public class GamificationConfig
    {
        public int Id { get; set; }
        public string ActivityCode { get; set; } = null!; // e.g., PROFILE_UPDATE, EVENT_ATTENDANCE, DONATION
        public string Name { get; set; } = null!;
        public int Points { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
