using System.ComponentModel.DataAnnotations;

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
        public bool AllowComments { get; set; } = true;

        // A DiscussionCategory lookup value. A retired value stays on old polls.
        [MaxLength(50)]
        public string Category { get; set; } = Constants.Discussion.DefaultCategory;

        public int? AnnouncementId { get; set; }

        // Navigation
        public ICollection<PollOption> Options { get; set; } = new List<PollOption>();
        public ICollection<PollVote> Votes { get; set; } = new List<PollVote>();
    }
}
