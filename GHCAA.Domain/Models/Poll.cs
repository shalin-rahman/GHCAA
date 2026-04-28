using System;
using System.Collections.Generic;

namespace GHCAA.Domain.Models
{
    public class Poll
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool AllowMultipleChoice { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public bool IsArchived { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; }
        public int CreatedBy { get; set; } // Admin Member ID

        // Navigation
        public ICollection<PollOption> Options { get; set; } = new List<PollOption>();
        public ICollection<PollVote> Votes { get; set; } = new List<PollVote>();
    }
}
