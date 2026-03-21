using System;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Domain.Models
{
    public class SpecialDayTheme
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = null!;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [MaxLength(7)]
        public string BackgroundColor { get; set; } = "#000000"; // Hex code

        [Required]
        [MaxLength(7)]
        public string TextColor { get; set; } = "#ffffff"; // Hex code

        [MaxLength(500)]
        public string AnnouncementText { get; set; } = string.Empty;


        [MaxLength(50)]
        public string AnimationStyle { get; set; } = "Fade"; // Fade, 3D, Typewriter, None

        [MaxLength(255)]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(7)]
        public string SidebarColor { get; set; } = string.Empty;

        public bool EnableGradientFading { get; set; } = true;

        public bool IsEnabled { get; set; } = true;
    }
}
