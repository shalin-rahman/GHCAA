using System;
using System.Collections.Generic;

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
        public int CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class CreateForumPostDto
    {
        public int TopicId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int? ParentPostId { get; set; }
    }
}
