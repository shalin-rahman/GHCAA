using System;
using GHCAA.Domain;

namespace GHCAA.Application.DTOs
{
    public class NewsPostDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public Enums.ArticleCategory Category { get; set; }
        public Enums.SubmissionStatus Status { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AuthorName { get; set; }
    }

    public class CreateNewsDto
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public Enums.ArticleCategory Category { get; set; }
        public Enums.SubmissionStatus Status { get; set; } = Enums.SubmissionStatus.Approved;
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateNewsDto : CreateNewsDto
    {
        public int Id { get; set; }
    }
}
