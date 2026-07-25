using System;
using static GHCAA.Domain.Enums;

namespace GHCAA.Domain.Models
{
    public class ECMember
    {
        public int Id { get; set; }

        public int MemberId { get; set; }
        public int ECPeriodId { get; set; }

        public ECPosition Position { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ChangeReason { get; set; }

        // Navigation 
        public Member? Member { get; set; }
        public ECPeriod? ECPeriod { get; set; }
    }
}
