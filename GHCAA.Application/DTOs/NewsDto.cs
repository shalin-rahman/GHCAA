using System;
using System.ComponentModel.DataAnnotations;
using GHCAA.Domain;

namespace GHCAA.Application.DTOs
{
    /// <summary>
    /// Accepts either an absolute http(s) URL or an app-relative path (e.g. "/uploads/news/x.jpg"),
    /// since the news image field is populated both by pasting an external URL and by the
    /// upload-image endpoint, which returns a relative path.
    /// </summary>
    public class RelativeOrAbsoluteUrlAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is not string s || string.IsNullOrWhiteSpace(s)) return true;
            if (s.StartsWith("/")) return true;
            return Uri.TryCreate(s, UriKind.Absolute, out var uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }

    public class NewsPostDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public Enums.ArticleCategory ArticleCategory { get; set; }
        public Enums.SubmissionStatus Status { get; set; }
        public Enums.PostType PostType { get; set; }
        public string? ImageUrl { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentFileName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AuthorName { get; set; }
        public List<string> Collaborators { get; set; } = new List<string>();
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

        public Enums.PostType PostType { get; set; } = Enums.PostType.News;

        [RelativeOrAbsoluteUrl(ErrorMessage = "Image URL must be a valid URL.")]
        public string? ImageUrl { get; set; }

        public string? AttachmentUrl { get; set; }

        [MaxLength(260)]
        public string? AttachmentFileName { get; set; }

        public List<string> Collaborators { get; set; } = new List<string>();

        public bool IsActive { get; set; } = true;
    }

    public class UpdateNewsDto : CreateNewsDto
    {
        public int Id { get; set; }
    }
}
