using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface IGalleryService
    {
        Task<EventGallery> CreateEventGalleryAsync(EventGallery gallery, CancellationToken cancellationToken = default);
        Task<bool> AddPhotosToGalleryAsync(int galleryId, IEnumerable<string> photoPaths, CancellationToken cancellationToken = default);
        Task<IEnumerable<EventGallery>> GetAllGalleriesAsync(bool onlyActive = false, CancellationToken cancellationToken = default);
        Task<EventGallery?> GetGalleryByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<EventGallery> UpdateEventGalleryAsync(EventGallery gallery, CancellationToken cancellationToken = default);
        Task<bool> DeleteGalleryAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> RemovePhotoAsync(int photoId, CancellationToken cancellationToken = default);

        // Member-owned album workflow
        Task<EventGallery> CreateMemberAlbumAsync(int memberId, string title, string? description, CancellationToken cancellationToken = default);
        Task<EventPhoto?> AddMemberPhotoToAlbumAsync(int memberId, int galleryId, string photoPath, string? caption, CancellationToken cancellationToken = default);
        Task<IEnumerable<EventGallery>> GetMemberAlbumsAsync(int memberId, CancellationToken cancellationToken = default);

        // Admin approval workflow
        Task<IEnumerable<EventGallery>> GetPendingGalleryApprovalsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<EventPhoto>> GetPendingPhotoApprovalsAsync(CancellationToken cancellationToken = default);
        Task<bool> ApproveGalleryAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> RejectGalleryAsync(int id, string reason, CancellationToken cancellationToken = default);
        Task<bool> ApprovePhotoAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> RejectPhotoAsync(int id, string reason, CancellationToken cancellationToken = default);
    }
}
