using System.ComponentModel.DataAnnotations;

namespace GHCAA.Domain.Models
{
    public class EmailTemplate
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = null!; // e.g., "WELCOME_EMAIL", "OTP_EMAIL"

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = null!;

        [Required]
        public string Body { get; set; } = null!; // Supports HTML

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = null!; // Internal name/usage info

        public string? Variables { get; set; } // JSON string of supported variables, e.g., "['FullName', 'MembershipNumber']"

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
