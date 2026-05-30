using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Domain.Models
{
    public class ForumCategory
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<ForumTopic> Topics { get; set; } = new List<ForumTopic>();
    }
}
