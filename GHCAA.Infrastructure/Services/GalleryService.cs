using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    public class GalleryService : IGalleryService
    {
        private readonly ApplicationDbContext _db;

        public GalleryService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<EventGallery> CreateEventGalleryAsync(EventGallery gallery, CancellationToken cancellationToken = default)
        {
            await _db.EventGalleries.AddAsync(gallery, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return gallery;
        }

        public async Task<bool> AddPhotosToGalleryAsync(int galleryId, IEnumerable<string> photoPaths, CancellationToken cancellationToken = default)
        {
            var gallery = await _db.EventGalleries.FindAsync(new object[] { galleryId }, cancellationToken);
            if (gallery == null) return false;

            foreach (var path in photoPaths)
            {
                var photo = new EventPhoto
                {
                    EventGalleryId = galleryId,
                    PhotoPath = path,
                    UploadedAt = DateTime.UtcNow
                };
                await _db.EventPhotos.AddAsync(photo, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IEnumerable<EventGallery>> GetAllGalleriesAsync(CancellationToken cancellationToken = default)
        {
            return await _db.EventGalleries
                .Include(g => g.Photos)
                .OrderByDescending(g => g.EventDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<EventGallery?> GetGalleryByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.EventGalleries
                .Include(g => g.Photos)
                .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        }

        public async Task<bool> DeleteGalleryAsync(int id, CancellationToken cancellationToken = default)
        {
            var gallery = await _db.EventGalleries.FindAsync(new object[] { id }, cancellationToken);
            if (gallery == null) return false;

            _db.EventGalleries.Remove(gallery);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
