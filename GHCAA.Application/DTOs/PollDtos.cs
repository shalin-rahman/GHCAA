namespace GHCAA.Application.DTOs
{
    public class PollDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool AllowMultipleChoice { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public List<PollOptionDto> Options { get; set; } = new();
        public int TotalVotes { get; set; }
        public bool HasVoted { get; set; }
        public List<int> SelectedOptionIds { get; set; } = new();
        public bool IsArchived { get; set; }
        public bool AllowComments { get; set; }
        public bool CommentsLocked { get; set; }
        public int CommentCount { get; set; }
        public string Category { get; set; } = GHCAA.Domain.Constants.Discussion.DefaultCategory;
        public int? AnnouncementId { get; set; }
    }

    public class PollOptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public int VoteCount { get; set; }
        public double Percentage { get; set; }
    }

    public class CreatePollDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool AllowMultipleChoice { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public List<string> Options { get; set; } = new();
        public bool AllowComments { get; set; } = true;

        // A DiscussionCategory lookup value. Null or blank means Other.
        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string? Category { get; set; }

        // Set to publish a news post or notice with the poll, in the same save.
        public PollAnnouncementDto? Announcement { get; set; }
    }

    public class PollAnnouncementDto
    {
        public GHCAA.Domain.Enums.PostType PostType { get; set; } = GHCAA.Domain.Enums.PostType.News;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Announcement title is required.")]
        [System.ComponentModel.DataAnnotations.MinLength(5, ErrorMessage = "Announcement title must be at least 5 characters.")]
        [System.ComponentModel.DataAnnotations.MaxLength(300)]
        public string Title { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Announcement text is required.")]
        [System.ComponentModel.DataAnnotations.MinLength(20, ErrorMessage = "Announcement text must be at least 20 characters.")]
        public string Content { get; set; } = "";

        public DateTime? PublishDate { get; set; }

        public bool MembersOnly { get; set; } = true;
    }

    public class PollVoteDto
    {
        public List<int> OptionIds { get; set; } = new();
    }
}
