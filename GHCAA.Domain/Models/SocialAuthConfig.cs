using System;
using System.ComponentModel.DataAnnotations;
using static GHCAA.Domain.Enums;

namespace GHCAA.Domain.Models
{
    public class SocialAuthConfig
    {
        public int Id { get; set; }

        [Required]
        public SocialProvider Provider { get; set; }

        [Required]
        [MaxLength(500)]
        public string ClientId { get; set; } = null!;

        [MaxLength(500)]
        public string? ClientSecret { get; set; }

        public bool IsEnabled { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
