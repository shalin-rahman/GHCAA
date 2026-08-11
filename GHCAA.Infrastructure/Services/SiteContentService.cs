using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Ganss.Xss;

namespace GHCAA.Infrastructure.Services
{
    public class SiteContentService : ISiteContentService
    {
        private readonly ApplicationDbContext _db;
        private static readonly HtmlSanitizer _sanitizer = new();

        public SiteContentService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<SiteContentDto>> GetActiveByGroupAsync(string group, CancellationToken cancellationToken = default)
        {
            var blocks = await _db.SiteContents
                .Where(s => s.IsActive && s.Group == group)
                .OrderBy(s => s.DisplayOrder)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return blocks.Select(MapToDto);
        }

        public async Task<IEnumerable<SiteContentDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var blocks = await _db.SiteContents
                .OrderBy(s => s.Group)
                .ThenBy(s => s.DisplayOrder)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return blocks.Select(MapToDto);
        }

        public async Task<SiteContentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var block = await _db.SiteContents.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            return block == null ? null : MapToDto(block);
        }

        public async Task<SiteContentDto> CreateAsync(UpsertSiteContentDto dto, int adminId, CancellationToken cancellationToken = default)
        {
            var block = new SiteContent
            {
                Key = dto.Key,
                Group = dto.Group,
                Title = dto.Title,
                BodyHtml = _sanitizer.Sanitize(dto.BodyHtml ?? ""),
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive,
                LastModified = DateTime.UtcNow,
                UpdatedByAdminId = adminId
            };

            await _db.SiteContents.AddAsync(block, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return MapToDto(block);
        }

        public async Task<SiteContentDto> UpdateAsync(int id, UpsertSiteContentDto dto, int adminId, CancellationToken cancellationToken = default)
        {
            var existing = await _db.SiteContents.FindAsync(new object[] { id }, cancellationToken);
            if (existing == null) throw new KeyNotFoundException("Content block not found");

            existing.Key = dto.Key;
            existing.Group = dto.Group;
            existing.Title = dto.Title;
            existing.BodyHtml = _sanitizer.Sanitize(dto.BodyHtml ?? "");
            existing.DisplayOrder = dto.DisplayOrder;
            existing.IsActive = dto.IsActive;
            existing.LastModified = DateTime.UtcNow;
            existing.UpdatedByAdminId = adminId;

            await _db.SaveChangesAsync(cancellationToken);
            return MapToDto(existing);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var block = await _db.SiteContents.FindAsync(new object[] { id }, cancellationToken);
            if (block == null) return false;

            _db.SiteContents.Remove(block);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static SiteContentDto MapToDto(SiteContent s) => new()
        {
            Id = s.Id,
            Key = s.Key,
            Group = s.Group,
            Title = s.Title,
            BodyHtml = s.BodyHtml,
            DisplayOrder = s.DisplayOrder,
            IsActive = s.IsActive,
            LastModified = s.LastModified
        };
    }
}
