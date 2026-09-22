using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    public class ArchiveService : IArchiveService
    {
        private readonly ApplicationDbContext _db;
        public ArchiveService(ApplicationDbContext db) => _db = db;

        private static ArchiveCollectionDto CollectionDto(ArchiveCollection x) => new()
        {
            Id = x.Id, Title = x.Title, Description = x.Description, Decade = x.Decade,
            PublicationState = x.PublicationState, ModerationState = x.ModerationState
        };
        private static ArchiveItemDto ItemDto(ArchiveItem x) => new()
        {
            Id = x.Id, ArchiveCollectionId = x.ArchiveCollectionId, Narrator = x.Narrator,
            Transcript = x.Transcript, Summary = x.Summary, Decade = x.Decade,
            LinkedMemberId = x.LinkedMemberId, FileUploadId = x.FileUploadId, MediaUrl = x.MediaUrl,
            PublicationState = x.PublicationState, ModerationState = x.ModerationState, CreatedAt = x.CreatedAt
        };
        private static IQueryable<ArchiveItem> Approved(IQueryable<ArchiveItem> query) =>
            query.Where(x => x.PublicationState == Enums.ArchivePublicationState.Published &&
                             x.ModerationState == Enums.ArchiveModerationState.Approved &&
                             x.Collection!.PublicationState == Enums.ArchivePublicationState.Published &&
                             x.Collection.ModerationState == Enums.ArchiveModerationState.Approved);

        public async Task<IReadOnlyList<ArchiveCollectionDto>> GetPublicCollectionsAsync(string? search, CancellationToken ct = default)
        {
            var q = _db.ArchiveCollections.Where(x => x.PublicationState == Enums.ArchivePublicationState.Published && x.ModerationState == Enums.ArchiveModerationState.Approved);
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => x.Title.Contains(search) || x.Description!.Contains(search));
            return await q.OrderBy(x => x.Title).Select(x => new ArchiveCollectionDto
            {
                Id = x.Id, Title = x.Title, Description = x.Description, Decade = x.Decade,
                PublicationState = x.PublicationState, ModerationState = x.ModerationState
            }).ToListAsync(ct);
        }
        public async Task<IReadOnlyList<ArchiveCollectionDto>> GetAdminCollectionsAsync(CancellationToken ct = default)
            => await _db.ArchiveCollections.OrderByDescending(x => x.CreatedAt).Select(x => new ArchiveCollectionDto
            {
                Id = x.Id, Title = x.Title, Description = x.Description, Decade = x.Decade,
                PublicationState = x.PublicationState, ModerationState = x.ModerationState
            }).ToListAsync(ct);
        public async Task<ArchiveItemDto?> GetPublicItemAsync(int id, CancellationToken ct = default)
            => await Approved(_db.ArchiveItems).Where(x => x.Id == id).Select(x => new ArchiveItemDto
            {
                Id = x.Id, ArchiveCollectionId = x.ArchiveCollectionId, Narrator = x.Narrator,
                Transcript = x.Transcript, Summary = x.Summary, Decade = x.Decade,
                LinkedMemberId = x.LinkedMemberId, FileUploadId = x.FileUploadId, MediaUrl = x.MediaUrl,
                PublicationState = x.PublicationState, ModerationState = x.ModerationState, CreatedAt = x.CreatedAt
            }).SingleOrDefaultAsync(ct);
        public async Task<IReadOnlyList<ArchiveItemDto>> GetAdminItemsAsync(string? search, CancellationToken ct = default)
        {
            var q = _db.ArchiveItems.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => (x.Transcript ?? "").Contains(search) || x.Narrator.Contains(search) || (x.Summary ?? "").Contains(search));
            return await q.OrderByDescending(x => x.CreatedAt).Select(x => new ArchiveItemDto
            {
                Id = x.Id, ArchiveCollectionId = x.ArchiveCollectionId, Narrator = x.Narrator,
                Transcript = x.Transcript, Summary = x.Summary, Decade = x.Decade,
                LinkedMemberId = x.LinkedMemberId, FileUploadId = x.FileUploadId, MediaUrl = x.MediaUrl,
                PublicationState = x.PublicationState, ModerationState = x.ModerationState, CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        }
        public async Task<ArchiveCollectionDto> CreateCollectionAsync(ArchiveCollectionDto dto, int memberId, CancellationToken ct = default)
        {
            var entity = new ArchiveCollection { Title = dto.Title, Description = dto.Description, Decade = dto.Decade, CreatedByMemberId = memberId, PublicationState = dto.PublicationState, ModerationState = dto.ModerationState };
            _db.ArchiveCollections.Add(entity); await _db.SaveChangesAsync(ct); return CollectionDto(entity);
        }
        public async Task<ArchiveCollectionDto?> UpdateCollectionAsync(int id, ArchiveCollectionDto dto, CancellationToken ct = default)
        {
            var entity = await _db.ArchiveCollections.FindAsync(new object[] { id }, ct); if (entity == null) return null;
            entity.Title = dto.Title; entity.Description = dto.Description; entity.Decade = dto.Decade;
            entity.PublicationState = dto.PublicationState; entity.ModerationState = dto.ModerationState;
            await _db.SaveChangesAsync(ct); return CollectionDto(entity);
        }
        public async Task<bool> DeleteCollectionAsync(int id, CancellationToken ct = default)
        {
            var entity = await _db.ArchiveCollections.FindAsync(new object[] { id }, ct); if (entity == null) return false;
            _db.ArchiveCollections.Remove(entity); await _db.SaveChangesAsync(ct); return true;
        }
        public async Task<ArchiveItemDto> CreateItemAsync(ArchiveItemDto dto, int memberId, CancellationToken ct = default)
        {
            ValidateMedia(dto);
            var entity = new ArchiveItem { ArchiveCollectionId = dto.ArchiveCollectionId, Narrator = dto.Narrator, Transcript = dto.Transcript, Summary = dto.Summary, Decade = dto.Decade, LinkedMemberId = dto.LinkedMemberId, FileUploadId = dto.FileUploadId, MediaUrl = dto.MediaUrl, CreatedByMemberId = memberId, PublicationState = dto.PublicationState, ModerationState = dto.ModerationState };
            _db.ArchiveItems.Add(entity); await _db.SaveChangesAsync(ct); return ItemDto(entity);
        }
        public async Task<ArchiveItemDto?> UpdateItemAsync(int id, ArchiveItemDto dto, CancellationToken ct = default)
        {
            ValidateMedia(dto);
            var entity = await _db.ArchiveItems.FindAsync(new object[] { id }, ct); if (entity == null) return null;
            entity.ArchiveCollectionId = dto.ArchiveCollectionId; entity.Narrator = dto.Narrator; entity.Transcript = dto.Transcript; entity.Summary = dto.Summary; entity.Decade = dto.Decade; entity.LinkedMemberId = dto.LinkedMemberId; entity.FileUploadId = dto.FileUploadId; entity.MediaUrl = dto.MediaUrl; entity.PublicationState = dto.PublicationState; entity.ModerationState = dto.ModerationState;
            await _db.SaveChangesAsync(ct); return ItemDto(entity);
        }
        public async Task<bool> DeleteItemAsync(int id, CancellationToken ct = default)
        {
            var entity = await _db.ArchiveItems.FindAsync(new object[] { id }, ct); if (entity == null) return false;
            _db.ArchiveItems.Remove(entity); await _db.SaveChangesAsync(ct); return true;
        }

        private static void ValidateMedia(ArchiveItemDto dto)
        {
            if (dto.FileUploadId is null &&
                string.IsNullOrWhiteSpace(dto.MediaUrl) &&
                string.IsNullOrWhiteSpace(dto.Transcript))
            {
                throw new InvalidOperationException(
                    "An archive item must include a file upload, external media URL, or transcript.");
            }
        }
        public async Task<bool> ModerateItemAsync(int id, Enums.ArchiveModerationState state, Enums.ArchivePublicationState publication, CancellationToken ct = default)
        {
            var entity = await _db.ArchiveItems.FindAsync(new object[] { id }, ct); if (entity == null) return false;
            entity.ModerationState = state; entity.PublicationState = publication; await _db.SaveChangesAsync(ct); return true;
        }
    }
}
