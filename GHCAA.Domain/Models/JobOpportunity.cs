using System;

namespace GHCAA.Domain.Models
{
    public class JobOpportunity
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Company { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Requirements { get; set; } = null!;
        public string ContactEmail { get; set; } = null!;
        public string? ApplicationLink { get; set; }
        public int PostedByMemberId { get; set; }
        public DateTime PostedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        public Enums.JobCategory Category { get; set; }

        // Navigation
        public Member? PostedBy { get; set; }
    }
}
