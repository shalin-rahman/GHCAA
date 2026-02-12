using System;

namespace GHCAA.Domain.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public int? MemberId { get; set; }
        public int? ActorId { get; set; } // The person doing the action
        public string ActivityType { get; set; } = null!; // e.g., Login, ProfileUpdate, PasswordChange, Approved, Archived
        public string Description { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? IPAddress { get; set; }
        
        // Navigation
        public Member? Member { get; set; }
    }
}
