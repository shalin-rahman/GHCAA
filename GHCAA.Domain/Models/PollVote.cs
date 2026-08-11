using System;

namespace GHCAA.Domain.Models
{
    public class PollVote
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public int PollOptionId { get; set; }
        public int MemberId { get; set; }
        public DateTime VotedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Poll Poll { get; set; } = null!;
        public PollOption PollOption { get; set; } = null!;
        public Member Member { get; set; } = null!;
    }
}
