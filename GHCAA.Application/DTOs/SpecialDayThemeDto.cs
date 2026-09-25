using System;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Application.DTOs
{
    // Request body for theme create and update. The controller used to bind the
    // SpecialDayTheme entity itself, so a client could post any column. Id is kept only
    // so the update route can reject a body that names a different theme; create ignores it.
    public class SpecialDayThemeSaveDto
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false), MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required(AllowEmptyStrings = false), MaxLength(7)]
        public string BackgroundColor { get; set; } = "#000000";

        [Required(AllowEmptyStrings = false), MaxLength(7)]
        public string TextColor { get; set; } = "#ffffff";

        [MaxLength(500)]
        public string? AnnouncementText { get; set; }

        [MaxLength(50)]
        public string? AnimationStyle { get; set; }

        [MaxLength(255)]
        public string? ImageUrl { get; set; }

        [MaxLength(7)]
        public string? SidebarColor { get; set; }

        public bool EnableGradientFading { get; set; } = true;

        public bool IsEnabled { get; set; } = true;
    }
}
