using System.ComponentModel.DataAnnotations;

namespace GHCAA.Domain.Models
{
    public class LookupItem
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string LookupGroup { get; set; } = null!; // e.g., "ProfessionalSector", "Degree"

        [Required]
        [MaxLength(100)]
        public string Value { get; set; } = null!; // Key for logic

        [Required]
        [MaxLength(200)]
        public string Label { get; set; } = null!; // Display name for frontend
        
        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
    }
}
