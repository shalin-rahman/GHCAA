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

        [Required]
        [MaxLength(100)]
        public string AnnouncementText { get; set; } = null!;

        public bool IsEnabled { get; set; } = true;
    }
}
