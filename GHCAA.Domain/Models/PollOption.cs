using System;

namespace GHCAA.Domain.Models
{
    public class PollOption
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public string OptionText { get; set; } = null!;

        // Navigation
        public Poll Poll { get; set; } = null!;
    }
}
