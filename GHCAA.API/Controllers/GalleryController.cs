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

        public GalleryController(IGalleryService galleryService)
        {
            _galleryService = galleryService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetGalleries(CancellationToken cancellationToken)
        {
            var galleries = await _galleryService.GetAllGalleriesAsync(cancellationToken);
            return Ok(galleries);
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
