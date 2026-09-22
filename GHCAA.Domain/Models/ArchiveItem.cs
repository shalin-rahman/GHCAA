using System;

namespace GHCAA.Domain.Models
{
    public class ArchiveItem
    {
        public int Id { get; set; }
        public int ArchiveCollectionId { get; set; }
        public string Narrator { get; set; } = null!;
        public string? Transcript { get; set; }
        public string? Summary { get; set; }
        public int? Decade { get; set; }
        public int? LinkedMemberId { get; set; }
        public int? FileUploadId { get; set; }
        public string? MediaUrl { get; set; }
        public Enums.ArchivePublicationState PublicationState { get; set; } = Enums.ArchivePublicationState.Draft;
        public Enums.ArchiveModerationState ModerationState { get; set; } = Enums.ArchiveModerationState.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedByMemberId { get; set; }
        public ArchiveCollection? Collection { get; set; }
        public Member? LinkedMember { get; set; }
        public FileUpload? FileUpload { get; set; }
    }
}
