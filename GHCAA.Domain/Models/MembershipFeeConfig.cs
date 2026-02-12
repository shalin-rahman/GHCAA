using System;
using System.ComponentModel.DataAnnotations;
using static GHCAA.Domain.Enums;

namespace GHCAA.Domain.Models
{
    public class MembershipFeeConfig
    {
        public int Id { get; set; }

        [Required]
        public MembershipType MembershipType { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string Description { get; set; } = "Annual Membership Fee";

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedByAdminId { get; set; }
    }
}
