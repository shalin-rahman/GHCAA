using System.IO;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _uploadsRoot;
        private readonly long _maxFileSize;
        private readonly IConfiguration _config;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(IConfiguration config, ILogger<LocalFileStorageService> logger)
        {
            _config = config;
            _logger = logger;
            var relativeRoot = _config["FileStorage:UploadsRelativePath"] ?? "uploads/members";
            _uploadsRoot = Path.Combine("wwwroot", relativeRoot);
            _maxFileSize = long.TryParse(_config["FileStorage:MaxFileSizeBytes"], out var v) ? v : 1048576;
        }

        public string GetRelativeFilePath(int memberId, Enums.FileUploadType uploadType, string fileName)
        {
            var safeFileName = Path.GetFileName(fileName);
            var prefix = uploadType.ToString().ToLower();
            var relativeRoot = _config["FileStorage:UploadsRelativePath"] ?? "uploads/members";
            return Path.Combine(relativeRoot, $"{prefix}_m{memberId}_{safeFileName}").Replace("\\", "/");
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, int memberId, Enums.FileUploadType uploadType, CancellationToken cancellationToken = default)
        {
            if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
            if (fileStream.Length > _maxFileSize) throw new InvalidOperationException($"File exceeds maximum size {_maxFileSize} bytes.");

            var safeFileName = Path.GetFileName(fileName);
            var prefix = uploadType.ToString().ToLower();
            
            // Create the single root directory if it does not exist
            Directory.CreateDirectory(_uploadsRoot);

            var uniqueName = $"{prefix}_m{memberId}_{Guid.NewGuid():N}_{safeFileName}";
            var diskPath = Path.Combine(_uploadsRoot, uniqueName);

            using var fs = new FileStream(diskPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await fileStream.CopyToAsync(fs, cancellationToken);

            var relativeRoot = _config["FileStorage:UploadsRelativePath"] ?? "uploads/members";
            var webRelative = Path.Combine(relativeRoot, uniqueName).Replace("\\", "/");
            _logger.LogInformation("Saved file to {Path}", webRelative);
            return webRelative;
        }

        public Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            var full = Path.Combine("wwwroot", relativePath.TrimStart('/', '\\'));
            if (File.Exists(full)) File.Delete(full);
            return Task.CompletedTask;
        }
    }
}
