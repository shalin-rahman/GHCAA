using System;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Domain.Models
{
    public class AcademicRecord
    {
        public int Id { get; set; }
        public int MemberId { get; set; }

        [Required]
        [MaxLength(200)]
        public string InstitutionName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Degree { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Subject { get; set; } = null!;

        public int? AdmissionYear { get; set; }
        public int PassingYear { get; set; }

        public bool IsOrgProfile { get; set; } // Flag for Govt. Haraganga College
        public string? Result { get; set; }
        public string? CertificatePath { get; set; }

        // Navigation
        public Member? Member { get; set; }
    }
}
