using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/secure-files")]
    public class SecureFilesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<SecureFilesController> _logger;

        public SecureFilesController(ApplicationDbContext db, ILogger<SecureFilesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet("{*filePath}")]
        public async Task<IActionResult> GetSecureFile(string filePath, CancellationToken cancellationToken)
        {
            // The filePath coming from the route might need normalization
            var normalizedPath = filePath.TrimStart('/');
            
            // Check if user is SuperAdmin or Admin
            bool isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("Admin");
            
            // Find the file in the database to get the owner
            var fileUpload = await _db.FileUploads
                .FirstOrDefaultAsync(f => f.FilePath == normalizedPath, cancellationToken);

            if (fileUpload == null)
            {
                _logger.LogWarning("Secure file request for non-existent path: {Path}", normalizedPath);
                return NotFound();
            }

            // Authorization logic
            if (!isAdmin)
            {
                // If not admin, check if the current user is the owner
                var currentMemberIdClaim = User.FindFirst("MemberId")?.Value;
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

            var secureRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads"));
            var fullPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, normalizedPath));

            // Ensure the resolved path is inside the intended uploads root.
            if (!fullPath.StartsWith(secureRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                && !fullPath.Equals(secureRoot, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Path traversal blocked — resolved path {Full} is outside root {Root}", fullPath, secureRoot);
                return NotFound();
            }

            if (!System.IO.File.Exists(fullPath))
            {
                _logger.LogError("Secure file record exists in DB but file is missing on disk: {Path}", fullPath);
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
