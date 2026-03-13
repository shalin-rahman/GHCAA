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
    }
}
