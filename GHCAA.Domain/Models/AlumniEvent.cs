using System;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Domain.Models
{
    public class AlumniEvent
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = null!;

        public decimal? RegistrationFee { get; set; }

        public bool IsActive { get; set; } = true;
        public bool AllowNonMembers { get; set; } = false;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public DateTime? RegistrationDeadline { get; set; }
        
        public string? AdminNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
