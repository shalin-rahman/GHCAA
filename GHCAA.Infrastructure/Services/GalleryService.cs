using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    public class GalleryService : IGalleryService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAdminNotificationService _adminNotification;
        private readonly INotificationService _notification;

        public GalleryService(ApplicationDbContext db, IAdminNotificationService adminNotification, INotificationService notification)
        {
            _db = db;
            _adminNotification = adminNotification;
            _notification = notification;
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
                    UploadedAt = DateTime.UtcNow
                };
                await _db.EventPhotos.AddAsync(photo, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IEnumerable<EventGallery>> GetAllGalleriesAsync(bool onlyActive = false, CancellationToken cancellationToken = default)
        {
            var query = _db.EventGalleries
                .Include(g => g.Photos)
                .AsQueryable();

            if (onlyActive)
            {
                // Public listings only ever show admin-approved galleries.
                query = query.Where(g => g.IsActive && g.Status == Enums.SubmissionStatus.Approved);
            }

            var galleries = await query
                .OrderByDescending(g => g.EventDate)
                .ToListAsync(cancellationToken);

            if (onlyActive)
            {
                foreach (var gallery in galleries)
                {
                    gallery.Photos = gallery.Photos.Where(p => p.Status == Enums.SubmissionStatus.Approved).ToList();
                }
            }

            return galleries;
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

            _db.EventGalleries.Remove(gallery);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> RemovePhotoAsync(int photoId, CancellationToken cancellationToken = default)
        {
            var photo = await _db.EventPhotos.FindAsync(new object[] { photoId }, cancellationToken);
            if (photo == null) return false;

            _db.EventPhotos.Remove(photo);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
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
