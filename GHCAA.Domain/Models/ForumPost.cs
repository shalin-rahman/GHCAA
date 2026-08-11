using System;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Domain.Models
{
    public class ForumPost
    {
        public int Id { get; set; }

        public int TopicId { get; set; }

        [Required]
        public string Content { get; set; } = null!; // HTML Support

        public int AuthorId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Optional: Support for nested replies (parent post)
        public int? ParentPostId { get; set; }

        // Navigation
        public ForumTopic? Topic { get; set; }
        public Member? Author { get; set; }
        public ForumPost? ParentPost { get; set; }
    }
}
