using System.IO;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly ILogger<LocalFileStorageService> _logger;
        private readonly string _uploadsRoot;
        private readonly long _maxFileSize;

        public LocalFileStorageService(IWebHostEnvironment env, IConfiguration config, ILogger<LocalFileStorageService> logger)
        {
            _env = env;
            _config = config;
            _logger = logger;
            var relativeRoot = _config["FileStorage:UploadsRelativePath"] ?? "uploads/members";
            _uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", relativeRoot);
            _maxFileSize = long.TryParse(_config["FileStorage:MaxFileSizeBytes"], out var v) ? v : 1048576;
        }

        public string GetRelativeFilePath(int memberId, Enums.FileUploadType uploadType, string fileName)
        {
            var safeFileName = Path.GetFileName(fileName);
            var folder = uploadType.ToString().ToLower();
            var memberFolder = Path.Combine("uploads", "members", memberId.ToString(), folder);
            return Path.Combine(memberFolder, safeFileName).Replace("\\", "/");
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, int memberId, Enums.FileUploadType uploadType, CancellationToken cancellationToken = default)
        {
            if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
            if (fileStream.Length > _maxFileSize) throw new InvalidOperationException($"File exceeds maximum size {_maxFileSize} bytes.");

            var safeFileName = Path.GetFileName(fileName);
            var subFolder = uploadType.ToString().ToLower();
            var targetFolder = Path.Combine(_uploadsRoot, memberId.ToString(), subFolder);
            Directory.CreateDirectory(targetFolder);

            var uniqueName = $"{Guid.NewGuid():N}_{safeFileName}";
            var diskPath = Path.Combine(targetFolder, uniqueName);

            using var fs = new FileStream(diskPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await fileStream.CopyToAsync(fs, cancellationToken);

            var webRelative = Path.Combine("uploads", "members", memberId.ToString(), subFolder, uniqueName).Replace("\\", "/");
            _logger.LogInformation("Saved file to {Path}", webRelative);
            return webRelative;
        }

        public Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            var full = Path.Combine(_env.WebRootPath ?? "wwwroot", relativePath.TrimStart('/', '\\'));
            if (File.Exists(full)) File.Delete(full);
            return Task.CompletedTask;
        }
    }
}