using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/gallery")]
    [Authorize]
    public class GalleryController : ControllerBase
    {
        private readonly IGalleryService _galleryService;
        private readonly IFileStorageService _fileStorage;

        public GalleryController(IGalleryService galleryService, IFileStorageService fileStorage)
        {
            _galleryService = galleryService;
            _fileStorage = fileStorage;
        }

        [HttpPost("upload-photo")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UploadPhoto(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded");
            
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            if (!int.TryParse(memberIdClaim, out var memberId))
            {
                // Fallback to simpler user ID claim if needed, but Admin should have MemberId
                return Unauthorized();
            }

            using var stream = file.OpenReadStream();
            var path = await _fileStorage.SaveFileAsync(stream, file.FileName, memberId, Domain.Enums.FileUploadType.GalleryPhoto, cancellationToken);
            
            // Return single path string or object
            return Ok(new { Path = path });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetGalleries(CancellationToken cancellationToken)
        {
            // Public viewing shows only active galleries
            var galleries = await _galleryService.GetAllGalleriesAsync(onlyActive: true, cancellationToken);
            return Ok(galleries);
        }

        [HttpGet("all")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllGalleries(CancellationToken cancellationToken)
        {
            // Admin sees everything
            var galleries = await _galleryService.GetAllGalleriesAsync(onlyActive: false, cancellationToken);
            return Ok(galleries);
        }

        [HttpPatch("admin/{id}/toggle-active")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ToggleActive(int id, CancellationToken cancellationToken)
        {
            var gallery = await _galleryService.GetGalleryByIdAsync(id, cancellationToken);
            if (gallery == null) return NotFound();

            gallery.IsActive = !gallery.IsActive;
            await _galleryService.UpdateEventGalleryAsync(gallery, cancellationToken);
            return Ok(new { gallery.IsActive });
        }

        [HttpPatch("admin/{id}/toggle-featured")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ToggleFeatured(int id, CancellationToken cancellationToken)
        {
            var gallery = await _galleryService.GetGalleryByIdAsync(id, cancellationToken);
            if (gallery == null) return NotFound();

            gallery.IsFeatured = !gallery.IsFeatured;
            await _galleryService.UpdateEventGalleryAsync(gallery, cancellationToken);
            return Ok(new { gallery.IsFeatured });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGallery(int id, CancellationToken cancellationToken)
        {
            var gallery = await _galleryService.GetGalleryByIdAsync(id, cancellationToken);
            if (gallery == null) return NotFound();
            return Ok(gallery);
        }

        [HttpPost("admin")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateGallery([FromBody] EventGallery gallery, CancellationToken cancellationToken)
        {
            var adminIdClaim = User.FindFirst("MemberId")?.Value;
            if (int.TryParse(adminIdClaim, out var adminId))
            {
                gallery.CreatedByAdminId = adminId;
            }

            var result = await _galleryService.CreateEventGalleryAsync(gallery, cancellationToken);
            return CreatedAtAction(nameof(GetGallery), new { id = result.Id }, result);
        }

        [HttpPost("admin/{id}/photos")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AddPhotos(int id, [FromBody] List<string> photoPaths, CancellationToken cancellationToken)
        {
            var result = await _galleryService.AddPhotosToGalleryAsync(id, photoPaths, cancellationToken);
            return result ? Ok() : NotFound();
        }

        [HttpDelete("admin/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteGallery(int id, CancellationToken cancellationToken)
        {
            var result = await _galleryService.DeleteGalleryAsync(id, cancellationToken);
            return result ? Ok() : NotFound();
        }
    }
}
