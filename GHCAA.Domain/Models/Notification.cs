using System;

namespace GHCAA.Domain.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? TargetUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public string Type { get; set; } = "General"; // Approval, Job, Payment, etc.

        [System.Text.Json.Serialization.JsonIgnore]
        public Member? Member { get; set; }
    }
}
