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

        // Member-owned album support
        public int? OwnerMemberId { get; set; }
        public Enums.SubmissionStatus Status { get; set; } = Enums.SubmissionStatus.Approved; // Default for existing/admin galleries
        public string? RejectionReason { get; set; }

        // Navigation
        public Member? OwnerMember { get; set; }
        public ICollection<EventPhoto> Photos { get; set; } = new List<EventPhoto>();
    }

    public class EventPhoto
    {
        public int Id { get; set; }
        public int EventGalleryId { get; set; }
        public string PhotoPath { get; set; } = null!;
        public string? Caption { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Member-owned photo support
        public int? UploadedByMemberId { get; set; }
        public Enums.SubmissionStatus Status { get; set; } = Enums.SubmissionStatus.Approved; // Default for existing/admin photos
        public string? RejectionReason { get; set; }

        // Navigation
        public EventGallery? EventGallery { get; set; }
    }
}
