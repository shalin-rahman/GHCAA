using System;
using GHCAA.Domain;

namespace GHCAA.Application.DTOs
{
    public class ArchiveCollectionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? Decade { get; set; }
        public Enums.ArchivePublicationState PublicationState { get; set; }
        public Enums.ArchiveModerationState ModerationState { get; set; }
    }

    public class ArchiveItemDto
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
        public Enums.ArchivePublicationState PublicationState { get; set; }
        public Enums.ArchiveModerationState ModerationState { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
