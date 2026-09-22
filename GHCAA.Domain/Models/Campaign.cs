using System;
using System.Collections.Generic;

namespace GHCAA.Domain.Models
{
    // TODO 37.8: a fundraising campaign with a public page and a live progress bar.
    public class Campaign
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Story { get; set; } = null!;
        public string? CoverImagePath { get; set; }
        public decimal TargetAmount { get; set; }
        public DateTime StartsOn { get; set; }
        public DateTime? EndsOn { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsArchived { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<CampaignPledge> Pledges { get; set; } = new();
    }
}
