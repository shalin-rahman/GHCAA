using System;

namespace GHCAA.Domain.Models
{
    public class MembershipHistory
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public string ChangedFrom { get; set; } = null!;
        public string ChangedTo { get; set; } = null!;
        public int? ChangedByAdminId { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string? Reason { get; set; }

        // Navigation
        public Member? Member { get; set; }
    }
}
