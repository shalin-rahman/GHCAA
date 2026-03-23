using System;
using System.ComponentModel.DataAnnotations;
using GHCAA.Domain;

namespace GHCAA.Application.DTOs
{
    public class NewsPostDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public Enums.ArticleCategory ArticleCategory { get; set; }
        public Enums.SubmissionStatus Status { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AuthorName { get; set; }
    }

    public class CreateNewsDto
    {
        [Required(ErrorMessage = "Article title is required.")]
        [MinLength(5, ErrorMessage = "Title must be at least 5 characters.")]
        [MaxLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Article content is required.")]
        [MinLength(20, ErrorMessage = "Content must be at least 20 characters.")]
        public string Content { get; set; } = null!;

        public Enums.ArticleCategory ArticleCategory { get; set; }

        public Enums.SubmissionStatus Status { get; set; } = Enums.SubmissionStatus.Approved;

        [Url(ErrorMessage = "Image URL must be a valid URL.")]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateNewsDto : CreateNewsDto
    {
        public int Id { get; set; }
    }
}
