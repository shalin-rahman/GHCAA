using System;
using System.Collections.Generic;

namespace GHCAA.Domain.Models
{
    public class EventGallery
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string? Location { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedByAdminId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;

        // Navigation
        public ICollection<EventPhoto> Photos { get; set; } = new List<EventPhoto>();
    }

    public class EventPhoto
    {
        public int Id { get; set; }
        public int EventGalleryId { get; set; }
        public string PhotoPath { get; set; } = null!;
        public string? Caption { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public EventGallery? EventGallery { get; set; }
    }
}
