using System;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Application.DTOs
{
    public class SiteContentDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = null!;
        public string Group { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string BodyHtml { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class UpsertSiteContentDto
    {
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
    }
}
