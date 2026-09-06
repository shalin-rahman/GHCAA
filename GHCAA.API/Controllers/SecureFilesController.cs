using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GHCAA.Application.Interfaces;
using GHCAA.Application.Security;

namespace GHCAA.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/secure-files")]
    public class SecureFilesController : ControllerBase
    {
        private readonly IFileUploadRepository _fileUploads;
        private readonly ILogger<SecureFilesController> _logger;
        private readonly IConfiguration _config;

        public SecureFilesController(IFileUploadRepository fileUploads, ILogger<SecureFilesController> logger, IConfiguration config)
        {
            _fileUploads = fileUploads;
            _logger = logger;
            _config = config;
        }

        [HttpGet("{*filePath}")]
        public async Task<IActionResult> GetSecureFile(string filePath, CancellationToken cancellationToken)
        {
            // The filePath coming from the route might need normalization
            var normalizedPath = filePath.TrimStart('/');

            // Check if user is SuperAdmin or Admin
            bool isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("Admin");

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
                var currentMemberIdClaim = User.FindFirst(AppClaimTypes.MemberId)?.Value;
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
            // signatures) while public assets live under wwwroot/uploads/. Resolve against the
            // same bases LocalFileStorageService writes to, and accept the file only if it lands
            // inside one of those two roots.
            var basePath = _config["FileStorage:BasePhysicalPath"];
            var publicRoot = Path.GetFullPath(basePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot"));
            var secureRoot = Path.GetFullPath(basePath ?? AppDomain.CurrentDomain.BaseDirectory);

            var publicCandidate = Path.GetFullPath(Path.Combine(publicRoot, normalizedPath));
            var secureCandidate = Path.GetFullPath(Path.Combine(secureRoot, normalizedPath));

            static bool IsInside(string candidate, string root) =>
                candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || candidate.Equals(root, StringComparison.OrdinalIgnoreCase);

            string? fullPath = null;
            if (IsInside(secureCandidate, secureRoot) && System.IO.File.Exists(secureCandidate))
                fullPath = secureCandidate;
            else if (IsInside(publicCandidate, publicRoot) && System.IO.File.Exists(publicCandidate))
                fullPath = publicCandidate;

            if (fullPath == null)
            {
                _logger.LogError("Secure file record exists in DB but file is missing/out-of-root: {Path}", normalizedPath);
                return NotFound();
            }

            // Determine content type
            var contentType = "application/octet-stream";
            if (normalizedPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) contentType = "application/pdf";
            else if (normalizedPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || normalizedPath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)) contentType = "image/jpeg";
            else if (normalizedPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) contentType = "image/png";

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath, cancellationToken);
            return File(fileBytes, contentType, fileUpload.FileName);
        }
    }
}
