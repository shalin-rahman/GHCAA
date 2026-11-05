using System.ComponentModel.DataAnnotations;

namespace GHCAA.Domain.Models
{
    public class ForumTopic
    {
        public int Id { get; set; }

        // Null for a comment thread that belongs to a poll or meeting, so it stays out of the forum.
        public int? CategoryId { get; set; }

        [MaxLength(20)]
        public string? ContextType { get; set; }

        public int? ContextId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!; // HTML Support for initial post

        public int AuthorId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastUpdatedAt { get; set; }

        public int ViewCount { get; set; } = 0;

        public bool IsPinned { get; set; } = false;

        public bool IsLocked { get; set; } = false;

        public bool IsActive { get; set; } = true;

        // Navigation
        public ForumCategory? Category { get; set; }
        public Member? Author { get; set; }
        public ICollection<ForumPost> Posts { get; set; } = new List<ForumPost>();
    }
}
