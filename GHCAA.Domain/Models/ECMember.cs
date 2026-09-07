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

        // 82.29: Class A soft-delete fields (ARCHITECTURE.md §4). EndDate already covers the
        // ordinary "this person's term ended" case; these cover the separate case of a row that
        // should never have existed (wrong member added) and is permanently removed by an admin.
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedByAdminId { get; set; }
        public bool IsArchived { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByAdminId { get; set; }

        // Navigation
        public Member? Member { get; set; }
        public ECPeriod? ECPeriod { get; set; }
    }
}
