using System;
using System.Collections.Generic;

namespace GHCAA.Domain.Models
{
    public class Constitution
    {
        public int Id { get; set; }
        public string Version { get; set; } = null!;
        public string Content { get; set; } = null!; // Full text
        public string? PdfUrl { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? SupersededDate { get; set; }
        public bool IsActive { get; set; }
        public string ChangeSummary { get; set; } = null!;
        
        public ICollection<AmendmentVote> Votes { get; set; } = new List<AmendmentVote>();
    }

    public class AmendmentVote
    {
        public int Id { get; set; }
        public int ConstitutionId { get; set; }
        public int MemberId { get; set; }
        public bool IsFor { get; set; }
        public string? Comments { get; set; }
        public DateTime VotedAt { get; set; } = DateTime.UtcNow;

        public Constitution? Constitution { get; set; }
        public Member? Member { get; set; }
    }
}
