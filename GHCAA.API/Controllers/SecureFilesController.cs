using GHCAA.API.Extensions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace GHCAA.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/secure-files")]
    public class SecureFilesController : ControllerBase
    {
        private readonly IFileUploadRepository _fileUploads;
        private readonly ILogger<SecureFilesController> _logger;
        private readonly IFileStorageService _storage;

        public SecureFilesController(IFileUploadRepository fileUploads, ILogger<SecureFilesController> logger, IFileStorageService storage)
        {
            _fileUploads = fileUploads;
            _logger = logger;
            _storage = storage;
        }

        [HttpGet("{*filePath}")]
        public async Task<IActionResult> GetSecureFile(string filePath, CancellationToken cancellationToken)
        {
            // The filePath coming from the route might need normalization
            var normalizedPath = filePath.TrimStart('/');

            // Check if user is SuperAdmin or Admin
            bool isAdmin = User.IsInRole(Constants.Roles.SuperAdmin) || User.IsInRole(Constants.Roles.Admin);

            // Find the file in the database to get the owner
            var fileUpload = await _fileUploads.GetByFilePathAsync(normalizedPath, cancellationToken);

            if (fileUpload == null)
            {
                _logger.LogWarning("Secure file request for non-existent path: {Path}", normalizedPath);
                return NotFound();
            }

            // Authorization logic
            if (!isAdmin)
            {
                // If not admin, check if the current user is the owner
                var currentMemberIdClaim = this.CurrentMemberIdRaw();
                if (string.IsNullOrEmpty(currentMemberIdClaim) || fileUpload.MemberId.ToString() != currentMemberIdClaim)
                {
                    _logger.LogWarning("Unauthorized access attempt to secure file {Path} by user {User}", normalizedPath, User.Identity?.Name);
                    return Forbid();
                }
            }

            // S6.1: Reject paths containing ".." before disk access.
            if (normalizedPath.Contains(".."))
            {
                _logger.LogWarning("Path traversal attempt blocked: {Path}", normalizedPath);
                return NotFound();
            }

            // 29B.8: Secure files live under secure_uploads/ (certificates, payment proofs,
            // signatures) while public assets live under uploads/. The storage service opens a
            // file only from one of those two trees, on disk or in the bucket.
            var file = await _storage.OpenReadAsync(normalizedPath, cancellationToken);

            if (file == null)
            {
                _logger.LogError("Secure file record exists in DB but file is missing/out-of-root: {Path}", normalizedPath);
                return NotFound();
            }

            // Determine content type
            var contentType = "application/octet-stream";
            if (normalizedPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) contentType = "application/pdf";
            else if (normalizedPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || normalizedPath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)) contentType = "image/jpeg";
            else if (normalizedPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) contentType = "image/png";

            // 98.9. Stream the file instead of reading it into memory first. Both stores hand back a
            // seekable stream, so Range and conditional requests work for disk and bucket files.
            return new FileStreamResult(file.Content, contentType)
            {
                FileDownloadName = fileUpload.FileName,
                LastModified = file.LastModified,
                EntityTag = EntityTagHeaderValue.TryParse(file.ETag, out var etag) ? etag : null,
                EnableRangeProcessing = true,
            };
        }
    }
}
