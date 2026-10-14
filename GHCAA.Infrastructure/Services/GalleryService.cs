using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    public class GalleryService : IGalleryService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAdminNotificationService _adminNotification;
        private readonly INotificationService _notification;
        private readonly IFileStorageService _fileStorage;
        private readonly ILogger<GalleryService> _logger;

        public GalleryService(ApplicationDbContext db, IAdminNotificationService adminNotification, INotificationService notification, IFileStorageService fileStorage, ILogger<GalleryService> logger)
        {
            _logger = logger;
            _db = db;
            _adminNotification = adminNotification;
            _notification = notification;
            _fileStorage = fileStorage;
        }

        public async Task<EventGallery> CreateEventGalleryAsync(EventGallery gallery, CancellationToken cancellationToken = default)
        {
            gallery.EventDate = DateTime.SpecifyKind(gallery.EventDate, DateTimeKind.Utc);
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
                    ThumbnailPath = _fileStorage.GetThumbnailPath(path),
                    UploadedAt = DateTime.UtcNow
                };
                await _db.EventPhotos.AddAsync(photo, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IEnumerable<EventGallery>> GetAllGalleriesAsync(bool onlyActive = false, CancellationToken cancellationToken = default)
        {
            // Public listings only ever show admin-approved galleries and photos. The photo filter
            // runs in SQL so pending and rejected photos are never loaded. No tracking, or photos the
            // context already holds would be fixed up into the list past the filter.
            var query = onlyActive
                ? _db.EventGalleries
                    .AsNoTracking()
                    .Include(g => g.Photos.Where(p => p.Status == Enums.SubmissionStatus.Approved))
                    .Where(g => g.IsActive && g.Status == Enums.SubmissionStatus.Approved)
                : _db.EventGalleries.Include(g => g.Photos);

            return await query
                .OrderByDescending(g => g.EventDate)
                .ThenByDescending(g => g.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResult<EventGallery>> GetActiveGalleriesPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _db.EventGalleries
                .AsNoTracking()
                .Include(g => g.Photos.Where(p => p.Status == Enums.SubmissionStatus.Approved))
                .Where(g => g.IsActive && g.Status == Enums.SubmissionStatus.Approved)
                .OrderByDescending(g => g.EventDate)
                .ThenByDescending(g => g.Id)
                .ToPagedResultAsync(page, pageSize, cancellationToken);
        }

        public async Task<EventGallery?> GetGalleryByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var gallery = await _db.EventGalleries
                .Include(g => g.Photos)
                .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

            if (gallery != null && gallery.Status == Enums.SubmissionStatus.Approved)
            {
                gallery.Photos = gallery.Photos.Where(p => p.Status == Enums.SubmissionStatus.Approved).ToList();
            }

            return gallery;
        }

        public async Task<EventGallery> UpdateEventGalleryAsync(EventGallery gallery, CancellationToken cancellationToken = default)
        {
            gallery.EventDate = DateTime.SpecifyKind(gallery.EventDate, DateTimeKind.Utc);
            _db.EventGalleries.Update(gallery);
            await _db.SaveChangesAsync(cancellationToken);
            return gallery;
        }

        public async Task<bool> DeleteGalleryAsync(int id, CancellationToken cancellationToken = default)
        {
            var gallery = await _db.EventGalleries.FindAsync(new object[] { id }, cancellationToken);
            if (gallery == null) return false;

            // The photo rows go with the gallery, so their paths are read first.
            var paths = await _db.EventPhotos
                .Where(p => p.EventGalleryId == id)
                .Select(p => p.PhotoPath)
                .ToListAsync(cancellationToken);
            _db.EventGalleries.Remove(gallery);
            await _db.SaveChangesAsync(cancellationToken);
            foreach (var path in paths) await DeletePhotoFileAsync(path);
            return true;
        }

        public async Task<bool> RemovePhotoAsync(int photoId, CancellationToken cancellationToken = default)
        {
            var photo = await _db.EventPhotos.FindAsync(new object[] { photoId }, cancellationToken);
            if (photo == null) return false;

            _db.EventPhotos.Remove(photo);
            await _db.SaveChangesAsync(cancellationToken);
            await DeletePhotoFileAsync(photo.PhotoPath);
            return true;
        }

        // The row is already gone, so a file that will not delete is logged and left behind.
        // It ignores the caller's token, so a client abort cannot stop the cleanup halfway. Storage
        // itself refuses seeded asset paths, since those are not under the uploads folder.
        private async Task DeletePhotoFileAsync(string path)
        {
            try { await _fileStorage.DeleteFileAsync(path, CancellationToken.None); }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Could not delete gallery photo file {Path}", path);
            }
        }

        public async Task<EventGallery> CreateMemberAlbumAsync(int memberId, string title, string? description, CancellationToken cancellationToken = default)
        {
            var gallery = new EventGallery
            {
                Title = title,
                Description = description,
                EventDate = DateTime.UtcNow,
                OwnerMemberId = memberId,
                CreatedByAdminId = memberId,
                Status = Enums.SubmissionStatus.Pending,
                IsActive = false
            };

            await _db.EventGalleries.AddAsync(gallery, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            await _adminNotification.NotifyPendingApprovalAsync("Album", gallery.Title, member?.FullName ?? "A member", $"/admin/gallery/{gallery.Id}", cancellationToken);

            return gallery;
        }

        public async Task<EventPhoto?> AddMemberPhotoToAlbumAsync(int memberId, int galleryId, string photoPath, string? caption, CancellationToken cancellationToken = default)
        {
            var gallery = await _db.EventGalleries.FindAsync(new object[] { galleryId }, cancellationToken);
            if (gallery == null) return null;

            var photo = new EventPhoto
            {
                EventGalleryId = galleryId,
                PhotoPath = photoPath,
                ThumbnailPath = _fileStorage.GetThumbnailPath(photoPath),
                Caption = caption,
                UploadedAt = DateTime.UtcNow,
                UploadedByMemberId = memberId,
                Status = Enums.SubmissionStatus.Pending
            };

            await _db.EventPhotos.AddAsync(photo, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            await _adminNotification.NotifyPendingApprovalAsync("Photo", gallery.Title, member?.FullName ?? "A member", $"/admin/gallery/{gallery.Id}", cancellationToken);

            return photo;
        }

        public async Task<IEnumerable<EventGallery>> GetMemberAlbumsAsync(int memberId, CancellationToken cancellationToken = default)
        {
            return await _db.EventGalleries
                .Include(g => g.Photos)
                .Where(g => g.OwnerMemberId == memberId)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<EventGallery>> GetPendingGalleryApprovalsAsync(CancellationToken cancellationToken = default)
        {
            return await _db.EventGalleries
                .Include(g => g.Photos)
                .Include(g => g.OwnerMember)
                .Where(g => g.Status == Enums.SubmissionStatus.Pending)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<EventPhoto>> GetPendingPhotoApprovalsAsync(CancellationToken cancellationToken = default)
        {
            return await _db.EventPhotos
                .Include(p => p.EventGallery)
                .Where(p => p.Status == Enums.SubmissionStatus.Pending)
                .OrderByDescending(p => p.UploadedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ApproveGalleryAsync(int id, bool notifyMember = true, CancellationToken cancellationToken = default)
        {
            var gallery = await _db.EventGalleries.FindAsync(new object[] { id }, cancellationToken);
            if (gallery == null) return false;

            gallery.Status = Enums.SubmissionStatus.Approved;
            gallery.IsActive = true;
            gallery.RejectionReason = null;
            await _db.SaveChangesAsync(cancellationToken);

            if (notifyMember && gallery.OwnerMemberId.HasValue)
            {
                await _notification.CreateNotificationAsync(
                    gallery.OwnerMemberId.Value,
                    "Album Approved",
                    $"Your album '{gallery.Title}' has been approved and is now visible on the public gallery.",
                    Enums.NotificationType.GeneralSystem,
                    $"/portal/gallery/{gallery.Id}",
                    cancellationToken);
            }

            return true;
        }

        public async Task<bool> RejectGalleryAsync(int id, string reason, bool notifyMember = true, CancellationToken cancellationToken = default)
        {
            var gallery = await _db.EventGalleries.FindAsync(new object[] { id }, cancellationToken);
            if (gallery == null) return false;

            gallery.Status = Enums.SubmissionStatus.Rejected;
            gallery.IsActive = false;
            gallery.RejectionReason = reason;
            await _db.SaveChangesAsync(cancellationToken);

            if (notifyMember && gallery.OwnerMemberId.HasValue)
            {
                await _notification.CreateNotificationAsync(
                    gallery.OwnerMemberId.Value,
                    "Album Rejected",
                    $"Your album '{gallery.Title}' was rejected. Reason: {reason}",
                    Enums.NotificationType.GeneralSystem,
                    $"/portal/gallery/{gallery.Id}",
                    cancellationToken);
            }

            return true;
        }

        public async Task<bool> ApprovePhotoAsync(int id, bool notifyMember = true, CancellationToken cancellationToken = default)
        {
            var photo = await _db.EventPhotos.Include(p => p.EventGallery).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (photo == null) return false;

            photo.Status = Enums.SubmissionStatus.Approved;
            photo.RejectionReason = null;
            await _db.SaveChangesAsync(cancellationToken);

            if (notifyMember && photo.UploadedByMemberId.HasValue)
            {
                await _notification.CreateNotificationAsync(
                    photo.UploadedByMemberId.Value,
                    "Photo Approved",
                    $"Your photo in album '{photo.EventGallery?.Title}' has been approved.",
                    Enums.NotificationType.GeneralSystem,
                    $"/portal/gallery/{photo.EventGalleryId}",
                    cancellationToken);
            }

            return true;
        }

        public async Task<bool> RejectPhotoAsync(int id, string reason, bool notifyMember = true, CancellationToken cancellationToken = default)
        {
            var photo = await _db.EventPhotos.Include(p => p.EventGallery).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (photo == null) return false;

            photo.Status = Enums.SubmissionStatus.Rejected;
            photo.RejectionReason = reason;
            await _db.SaveChangesAsync(cancellationToken);

            if (notifyMember && photo.UploadedByMemberId.HasValue)
            {
                await _notification.CreateNotificationAsync(
                    photo.UploadedByMemberId.Value,
                    "Photo Rejected",
                    $"Your photo in album '{photo.EventGallery?.Title}' was rejected. Reason: {reason}",
                    Enums.NotificationType.GeneralSystem,
                    $"/portal/gallery/{photo.EventGalleryId}",
                    cancellationToken);
            }

            return true;
        }
    }
}
