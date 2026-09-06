using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Extensions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using GHCAA.Domain;
using GHCAA.Application.Security;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/gallery")]
    [Authorize]
    public class GalleryController : ControllerBase
    {
        private readonly IGalleryService _galleryService;
        private readonly IFileStorageService _fileStorage;
        private readonly IFileValidationService _fileValidationService;

        public GalleryController(IGalleryService galleryService, IFileStorageService fileStorage, IFileValidationService fileValidationService)
        {
            _galleryService = galleryService;
            _fileStorage = fileStorage;
            _fileValidationService = fileValidationService;
        }

        [HttpPost("upload-photo")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> UploadPhoto(IFormFile file, CancellationToken cancellationToken)
        {
            var validation = _fileValidationService.ValidateFormFile(file, FileCategory.Image, 10 * 1024 * 1024);
            if (!validation.IsValid) return Problem(detail: validation.ErrorMessage, statusCode: StatusCodes.Status400BadRequest);

            var memberIdClaim = this.CurrentMemberIdRaw();
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
        [OutputCache(PolicyName = Constants.OutputCachePolicies.PublicContent)]
        public async Task<IActionResult> GetGalleries(CancellationToken cancellationToken)
        {
            // Public viewing shows only active galleries
            var galleries = await _galleryService.GetAllGalleriesAsync(onlyActive: true, cancellationToken);
            return Ok(galleries);
        }

        [HttpGet("all")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> GetAllGalleries(CancellationToken cancellationToken)
        {
            // Admin sees everything
            var galleries = await _galleryService.GetAllGalleriesAsync(onlyActive: false, cancellationToken);
            return Ok(galleries);
        }

        [HttpPatch("admin/{id}/toggle-active")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> ToggleActive(int id, CancellationToken cancellationToken)
        {
            var gallery = await _galleryService.GetGalleryByIdAsync(id, cancellationToken);
            if (gallery == null) return NotFound();

            gallery.IsActive = !gallery.IsActive;
            await _galleryService.UpdateEventGalleryAsync(gallery, cancellationToken);
            return Ok(new { gallery.IsActive });
        }

        [HttpPatch("admin/{id}/toggle-featured")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
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
        [OutputCache(PolicyName = Constants.OutputCachePolicies.PublicContent)]
        public async Task<IActionResult> GetGallery(int id, CancellationToken cancellationToken)
        {
            var gallery = await _galleryService.GetGalleryByIdAsync(id, cancellationToken);
            if (gallery == null) return NotFound();
            return Ok(gallery);
        }

        [HttpPost("admin")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> CreateGallery([FromBody] EventGallery gallery, CancellationToken cancellationToken)
        {
            var adminIdClaim = this.CurrentMemberIdRaw();
            if (int.TryParse(adminIdClaim, out var adminId))
            {
                gallery.CreatedByAdminId = adminId;
            }

            var result = await _galleryService.CreateEventGalleryAsync(gallery, cancellationToken);
            return CreatedAtAction(nameof(GetGallery), new { id = result.Id }, result);
        }

        [HttpPut("admin/{id}")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> UpdateGallery(int id, [FromBody] EventGallery updatedGallery, CancellationToken cancellationToken)
        {
            var existingGallery = await _galleryService.GetGalleryByIdAsync(id, cancellationToken);
            if (existingGallery == null) return NotFound();

            existingGallery.Title = updatedGallery.Title;
            existingGallery.Description = updatedGallery.Description;
            existingGallery.EventDate = updatedGallery.EventDate;
            existingGallery.Location = updatedGallery.Location;
            existingGallery.IsActive = updatedGallery.IsActive;
            existingGallery.IsFeatured = updatedGallery.IsFeatured;

            var result = await _galleryService.UpdateEventGalleryAsync(existingGallery, cancellationToken);
            return Ok(result);
        }

        [HttpPost("admin/{id}/photos")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> AddPhotos(int id, [FromBody] List<string> photoPaths, CancellationToken cancellationToken)
        {
            var result = await _galleryService.AddPhotosToGalleryAsync(id, photoPaths, cancellationToken);
            return result ? Ok() : NotFound();
        }

        [HttpDelete("admin/{id}")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> DeleteGallery(int id, CancellationToken cancellationToken)
        {
            var result = await _galleryService.DeleteGalleryAsync(id, cancellationToken);
            return result ? Ok() : NotFound();
        }

        [HttpDelete("admin/photos/{photoId}")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> RemovePhoto(int photoId, CancellationToken cancellationToken)
        {
            var result = await _galleryService.RemovePhotoAsync(photoId, cancellationToken);
            return result ? Ok() : NotFound();
        }

        // ---- Member quick single-photo submission (previously dead POST api/gallery) ----

        [HttpPost]
        [Authorize(Policy = Constants.Policies.MemberOnly)]
        public async Task<IActionResult> SubmitMemberPhoto([FromForm] string title, [FromForm] string? description, IFormFile file, CancellationToken cancellationToken)
        {
            if (!TryGetMemberId(out var memberId)) return Unauthorized();

            var validation = _fileValidationService.ValidateFormFile(file, FileCategory.Image, 10 * 1024 * 1024);
            if (!validation.IsValid) return Problem(detail: validation.ErrorMessage, statusCode: StatusCodes.Status400BadRequest);

            using var stream = file.OpenReadStream();
            var path = await _fileStorage.SaveFileAsync(stream, file.FileName, memberId, Domain.Enums.FileUploadType.GalleryPhoto, cancellationToken);

            var gallery = await _galleryService.CreateMemberAlbumAsync(memberId, title, description, cancellationToken);
            await _galleryService.AddMemberPhotoToAlbumAsync(memberId, gallery.Id, path, description, cancellationToken);

            return CreatedAtAction(nameof(GetGallery), new { id = gallery.Id }, gallery);
        }

        // ---- Member album management ----

        [HttpPost("albums")]
        [Authorize(Policy = Constants.Policies.MemberOnly)]
        public async Task<IActionResult> CreateAlbum([FromBody] CreateAlbumRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetMemberId(out var memberId)) return Unauthorized();

            var gallery = await _galleryService.CreateMemberAlbumAsync(memberId, request.Title, request.Description, cancellationToken);
            return CreatedAtAction(nameof(GetGallery), new { id = gallery.Id }, gallery);
        }

        [HttpGet("albums/mine")]
        [Authorize(Policy = Constants.Policies.MemberOnly)]
        public async Task<IActionResult> GetMyAlbums(CancellationToken cancellationToken)
        {
            if (!TryGetMemberId(out var memberId)) return Unauthorized();

            var albums = await _galleryService.GetMemberAlbumsAsync(memberId, cancellationToken);
            return Ok(albums);
        }

        [HttpPost("albums/{id}/photos")]
        [Authorize(Policy = Constants.Policies.MemberOnly)]
        public async Task<IActionResult> AddPhotoToAlbum(int id, IFormFile file, [FromForm] string? caption, CancellationToken cancellationToken)
        {
            if (!TryGetMemberId(out var memberId)) return Unauthorized();

            var gallery = await _galleryService.GetGalleryByIdAsync(id, cancellationToken)
                          ?? (await _galleryService.GetMemberAlbumsAsync(memberId, cancellationToken)).FirstOrDefault(g => g.Id == id);
            if (gallery == null) return NotFound();

            var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            if (gallery.OwnerMemberId != memberId && !isAdmin) return Forbid();

            var validation = _fileValidationService.ValidateFormFile(file, FileCategory.Image, 10 * 1024 * 1024);
            if (!validation.IsValid) return Problem(detail: validation.ErrorMessage, statusCode: StatusCodes.Status400BadRequest);

            using var stream = file.OpenReadStream();
            var path = await _fileStorage.SaveFileAsync(stream, file.FileName, memberId, Domain.Enums.FileUploadType.GalleryPhoto, cancellationToken);

            var photo = await _galleryService.AddMemberPhotoToAlbumAsync(memberId, id, path, caption, cancellationToken);
            return photo == null ? NotFound() : Ok(photo);
        }

        // ---- Admin approval workflow ----

        [HttpGet("admin/pending")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> GetPendingApprovals(CancellationToken cancellationToken)
        {
            var galleries = await _galleryService.GetPendingGalleryApprovalsAsync(cancellationToken);
            var photos = await _galleryService.GetPendingPhotoApprovalsAsync(cancellationToken);
            return Ok(new { Galleries = galleries, Photos = photos });
        }

        [HttpPost("admin/{id}/approve")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> ApproveGallery(int id, [FromQuery] bool notifyMember = true, CancellationToken cancellationToken = default)
        {
            var result = await _galleryService.ApproveGalleryAsync(id, notifyMember, cancellationToken);
            return result ? Ok(new { Message = "Album approved." }) : NotFound();
        }

        [HttpPost("admin/{id}/reject")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> RejectGallery(int id, [FromBody] RejectRequest request, CancellationToken cancellationToken)
        {
            var result = await _galleryService.RejectGalleryAsync(id, request.Reason, request.NotifyMember, cancellationToken);
            return result ? Ok(new { Message = "Album rejected." }) : NotFound();
        }

        [HttpPost("photos/{photoId}/approve")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> ApprovePhoto(int photoId, [FromQuery] bool notifyMember = true, CancellationToken cancellationToken = default)
        {
            var result = await _galleryService.ApprovePhotoAsync(photoId, notifyMember, cancellationToken);
            return result ? Ok(new { Message = "Photo approved." }) : NotFound();
        }

        [HttpPost("photos/{photoId}/reject")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> RejectPhoto(int photoId, [FromBody] RejectRequest request, CancellationToken cancellationToken)
        {
            var result = await _galleryService.RejectPhotoAsync(photoId, request.Reason, request.NotifyMember, cancellationToken);
            return result ? Ok(new { Message = "Photo rejected." }) : NotFound();
        }

        private bool TryGetMemberId(out int memberId)
        {
            var memberIdClaim = this.CurrentMemberIdRaw();
            return int.TryParse(memberIdClaim, out memberId);
        }

        public class CreateAlbumRequest
        {
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
        }

        public class RejectRequest
        {
            public string Reason { get; set; } = null!;
            public bool NotifyMember { get; set; } = true;
        }
    }
}
