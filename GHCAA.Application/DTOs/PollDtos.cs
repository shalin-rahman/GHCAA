using System;
using System.Collections.Generic;

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
    }

    public class PollVoteDto
    {
        public List<int> OptionIds { get; set; } = new();
    }
}
