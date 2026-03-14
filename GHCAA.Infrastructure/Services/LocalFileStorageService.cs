using System.IO;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _publicRoot;
        private readonly string _secureRoot;
        private readonly long _maxFileSize;
        private readonly IConfiguration _config;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(IConfiguration config, ILogger<LocalFileStorageService> logger)
        {
            _config = config;
            _logger = logger;
            
            var publicRelative = _config["FileStorage:UploadsRelativePath"] ?? "uploads/members";
            var secureRelative = _config["FileStorage:SecureRelativePath"] ?? "secure_uploads/members";
            
            _publicRoot = Path.Combine("wwwroot", publicRelative);
            _secureRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, secureRelative);
            
            _maxFileSize = long.TryParse(_config["FileStorage:MaxFileSizeBytes"], out var v) ? v : 52428800;
        }

        private bool IsSecureType(Enums.FileUploadType type)
        {
            return type == Enums.FileUploadType.Certificate || type == Enums.FileUploadType.PaymentProof;
        }

        public string GetRelativeFilePath(int memberId, Enums.FileUploadType uploadType, string fileName)
        {
            var safeFileName = Path.GetFileName(fileName);
            var prefix = uploadType.ToString().ToLower();
            var relativeRoot = IsSecureType(uploadType) 
                    ? (_config["FileStorage:SecureRelativePath"] ?? "secure_uploads/members")
                    : (_config["FileStorage:UploadsRelativePath"] ?? "uploads/members");
            
            return Path.Combine(relativeRoot, $"{prefix}_m{memberId}_{safeFileName}").Replace("\\", "/");
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, int memberId, Enums.FileUploadType uploadType, CancellationToken cancellationToken = default)
        {
            if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
            if (fileStream.Length > _maxFileSize) throw new InvalidOperationException($"File exceeds maximum size {_maxFileSize} bytes.");

            var isSecure = IsSecureType(uploadType);
            var rootPath = isSecure ? _secureRoot : _publicRoot;
            var relativePrefix = isSecure 
                ? (_config["FileStorage:SecureRelativePath"] ?? "secure_uploads/members")
                : (_config["FileStorage:UploadsRelativePath"] ?? "uploads/members");

            var safeFileName = Path.GetFileName(fileName);
            var prefix = uploadType.ToString().ToLower();
            
            Directory.CreateDirectory(rootPath);

            var uniqueName = $"{prefix}_m{memberId}_{Guid.NewGuid():N}_{safeFileName}";
            var diskPath = Path.Combine(rootPath, uniqueName);

            using var fs = new FileStream(diskPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await fileStream.CopyToAsync(fs, cancellationToken);

            var webRelative = Path.Combine(relativePrefix, uniqueName).Replace("\\", "/");
            _logger.LogInformation("Saved {Type} file to {Path}", isSecure ? "secure" : "public", webRelative);
            return webRelative;
        }

        public Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            // Try public first
            var publicPath = Path.Combine("wwwroot", relativePath.TrimStart('/', '\\'));
            if (File.Exists(publicPath))
            {
                File.Delete(publicPath);
                return Task.CompletedTask;
            }

            // Try secure
            var securePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath.TrimStart('/', '\\'));
            if (File.Exists(securePath))
            {
                File.Delete(securePath);
            }
            
            return Task.CompletedTask;
        }
    }
}
