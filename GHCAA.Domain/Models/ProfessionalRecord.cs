using System;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Domain.Models
{
    public class ProfessionalRecord
    {
        public int Id { get; set; }
        public int MemberId { get; set; }

        [Required]
        [MaxLength(200)]
        public string OrganizationName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Designation { get; set; } = null!;

        [MaxLength(100)]
        public string? Sector { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsCurrent { get; set; }

        // Navigation
        public Member? Member { get; set; }
    }
}
