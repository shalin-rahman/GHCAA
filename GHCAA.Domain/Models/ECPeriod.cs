namespace GHCAA.Domain.Models
{
    public class ECPeriod
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!; // e.g., "Executive Committee 2024-2027"
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = false;

        // Navigation
        public ICollection<ECMember> ECMembers { get; set; } = new List<ECMember>();
        public ICollection<ECPeriodHandoverCheck> HandoverChecks { get; set; } = new List<ECPeriodHandoverCheck>();
    }

    // 37.14m. The handover checklist (ER-34) for the outgoing term. One row per item, kept when
    // unticked so the form still shows who last changed it.
    public class ECPeriodHandoverCheck
    {
        public int Id { get; set; }
        public int ECPeriodId { get; set; }
        public GHCAA.Domain.Enums.HandoverCheckItem Item { get; set; }
        public bool IsChecked { get; set; }
        public int SetByUserId { get; set; }
        public DateTime SetAt { get; set; }
    }
}
