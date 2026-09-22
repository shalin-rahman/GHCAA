using System;
using System.Collections.Generic;

namespace GHCAA.Domain.Models
{
    public class ArchiveCollection
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? Decade { get; set; }
        public Enums.ArchivePublicationState PublicationState { get; set; } = Enums.ArchivePublicationState.Draft;
        public Enums.ArchiveModerationState ModerationState { get; set; } = Enums.ArchiveModerationState.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedByMemberId { get; set; }
        public ICollection<ArchiveItem> Items { get; set; } = new List<ArchiveItem>();
    }
}
