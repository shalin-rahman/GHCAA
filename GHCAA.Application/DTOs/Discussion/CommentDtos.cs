using System.ComponentModel.DataAnnotations;
using GHCAA.Domain;

namespace GHCAA.Application.DTOs.Discussion
{
    public class CommentDto
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = "";
        public string? AuthorPhotoUrl { get; set; }
        // Empty when Removed is true.
        public string Content { get; set; } = "";
        public DateTime PostedAt { get; set; }
        public bool Removed { get; set; }
        public List<CommentDto> Replies { get; set; } = new();
    }

    public class CommentThreadDto
    {
        public List<CommentDto> Comments { get; set; } = new();
        // Top-level comments in the whole thread, for paging.
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool IsLocked { get; set; }
        public bool AllowComments { get; set; }
        // Why posting is closed, or null when the caller can post.
        public string? ClosedReason { get; set; }
    }

    public class CreateCommentDto
    {
        [Required(ErrorMessage = "Write a comment first.")]
        [MaxLength(Constants.Discussion.MaxCommentLength, ErrorMessage = "A comment can be at most {1} characters.")]
        public string Content { get; set; } = "";

        public int? ParentId { get; set; }
    }

    public class RemoveCommentDto
    {
        // Admins give a reason for the audit log. The author does not need one.
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    public class SetThreadLockDto
    {
        public bool Locked { get; set; }
    }

    public class SetAllowCommentsDto
    {
        public bool AllowComments { get; set; }
    }

    // Outcome of a discussion call. The controller maps Status to the HTTP reply.
    public enum DiscussionStatus
    {
        Ok,
        NotFound,
        Invalid,
        Forbidden,
    }

    public record DiscussionResult<T>(DiscussionStatus Status, T? Value = default, string? Error = null)
    {
        public static DiscussionResult<T> Success(T value) => new(DiscussionStatus.Ok, value);
        public static DiscussionResult<T> Missing(string error) => new(DiscussionStatus.NotFound, default, error);
        public static DiscussionResult<T> Bad(string error) => new(DiscussionStatus.Invalid, default, error);
        public static DiscussionResult<T> Denied(string error) => new(DiscussionStatus.Forbidden, default, error);
    }
}
