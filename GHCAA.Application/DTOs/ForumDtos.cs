using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Application.DTOs
{
    public class ForumCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public int TopicCount { get; set; }
        public int PostCount { get; set; }
    }

    public class ForumTopicDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorPhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public int ViewCount { get; set; }
        public bool IsPinned { get; set; }
        public bool IsLocked { get; set; }
        public int ReplyCount { get; set; }
    }

    public class ForumPostDto
    {
        public int Id { get; set; }
        public int TopicId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorPhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? ParentPostId { get; set; }
    }

    public class CreateForumTopicDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "A category is required.")]
        public int CategoryId { get; set; }
        // Matches ForumTopic.Title [MaxLength(200)].
        [Required(AllowEmptyStrings = false), MaxLength(200, ErrorMessage = "Title must be 200 characters or fewer.")]
        public string Title { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = false), MaxLength(10000, ErrorMessage = "Content must be 10000 characters or fewer.")]
        public string Content { get; set; } = string.Empty;
    }

    public class CreateForumPostDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "A topic is required.")]
        public int TopicId { get; set; }
        [Required(AllowEmptyStrings = false), MaxLength(10000, ErrorMessage = "Content must be 10000 characters or fewer.")]
        public string Content { get; set; } = string.Empty;
        [Range(1, int.MaxValue)]
        public int? ParentPostId { get; set; }
    }
}
