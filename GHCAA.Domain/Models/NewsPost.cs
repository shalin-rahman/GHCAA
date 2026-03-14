using System;
using System.ComponentModel.DataAnnotations;
using static GHCAA.Domain.Enums;

namespace GHCAA.Domain.Models
{
    public class NewsPost
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!; // HTML support

        [Required]
        public ArticleCategory Category { get; set; }

        public SubmissionStatus Status { get; set; } = SubmissionStatus.Approved; // Default for existing/admin news

        public DateTime PublishDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
        public string? ImageUrl { get; set; }
        public int AuthorId { get; set; }
        
        public DateTime? LastModified { get; set; }

        // Navigation
        public User? Author { get; set; }
    }
}
