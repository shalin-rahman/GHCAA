using System;

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
    }
}
