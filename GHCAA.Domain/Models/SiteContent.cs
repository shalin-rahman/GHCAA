using System;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Domain.Models
{
    public class SiteContent
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Group { get; set; } = "about";

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string BodyHtml { get; set; } = null!;

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastModified { get; set; }

        public int? UpdatedByAdminId { get; set; }
    }
}
