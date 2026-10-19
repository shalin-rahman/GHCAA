using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Infrastructure.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GHCAA.Infrastructure.Services
{
    public class LocalFileStorageService : FileStorageServiceBase
    {
        private readonly string _publicBase;
        private readonly string _secureBase;
        private readonly string _publicRoot;
        private readonly string _secureRoot;

        public LocalFileStorageService(IOptions<FileStorageOptions> options, ILogger<LocalFileStorageService> logger, IWebHostEnvironment webHostEnvironment)
            : base(options.Value, logger)
        {
            // Normalize relative paths to use forward slashes for cross-platform consistency
            var publicRelative = Normalize(_options.UploadsRelativePath).TrimEnd('/');
            var secureRelative = Normalize(_options.SecureRelativePath).TrimEnd('/');

            _publicBase = _options.BasePhysicalPath ?? "wwwroot";
            _secureBase = _options.BasePhysicalPath ?? AppDomain.CurrentDomain.BaseDirectory;
            _publicRoot = Path.Combine(_publicBase, publicRelative);
            _secureRoot = Path.Combine(_secureBase, secureRelative);

            // 82.51: today the two roots stay apart only because their fallback defaults ("wwwroot"
            // vs. AppDomain.CurrentDomain.BaseDirectory) happen not to collide — nothing enforces it.
            // A future FileStorage:BasePhysicalPath change could make _secureRoot land inside the
            // static-files web root, which would serve Certificate/PaymentProof/Signature files with
            // no auth check. Fail loudly at startup instead of silently exposing them.
            if (!string.IsNullOrEmpty(webHostEnvironment.WebRootPath))
            {
                var webRoot = Path.GetFullPath(webHostEnvironment.WebRootPath);
                var secureFull = Path.GetFullPath(_secureRoot);
                var isSameOrNested = secureFull.Equals(webRoot, StringComparison.OrdinalIgnoreCase)
                    || secureFull.StartsWith(webRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
                if (isSameOrNested)
                {
                    throw new InvalidOperationException(
                        $"FileStorage misconfiguration: the secure uploads root ('{secureFull}') resolves inside the " +
                        $"static-files web root ('{webRoot}'). This would serve Certificate/PaymentProof/Signature " +
                        "files through the unauthenticated static-files route. Set FileStorage:SecureRelativePath or " +
                        "FileStorage:BasePhysicalPath so the secure root stays outside the web root.");
                }
            }
        }

        protected override async Task WriteAsync(string relativePath, Stream content, bool isSecure, CancellationToken cancellationToken)
        {
            var fullPath = Path.GetFullPath(Path.Combine(isSecure ? _secureBase : _publicBase, relativePath));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            await using var target = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            await content.CopyToAsync(target, cancellationToken);
        }

        protected override bool ThumbnailExists(string thumbnailRelativePath) =>
            File.Exists(Path.Combine(_publicBase, thumbnailRelativePath));

        public override Task<StoredFile?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            var fullPath = Resolve(relativePath);
            if (fullPath == null) return Task.FromResult<StoredFile?>(null);

            FileStream stream;
            try
            {
                stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            }
            // Deleted between the exists check and the open. Same answer as never there.
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                return Task.FromResult<StoredFile?>(null);
            }
            var lastModified = new DateTimeOffset(File.GetLastWriteTimeUtc(fullPath));
            // Same tag shape the static-files middleware gives a disk file.
            var etag = "\"" + Convert.ToString(lastModified.ToFileTime() ^ stream.Length, 16) + "\"";
            return Task.FromResult<StoredFile?>(new StoredFile(stream, lastModified, etag));
        }

        public override Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Directory.Exists(_publicBase));

        public override Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            // Try public first
            var publicPath = Path.GetFullPath(Path.Combine(_publicBase, relativePath.TrimStart('/', '\\')));
            if (IsInside(publicPath, _publicRoot) && File.Exists(publicPath))
            {
                File.Delete(publicPath);

                // A gallery photo's thumbnail sits beside it under thumbs/ with the same name. The
                // photo is already gone, so a thumbnail that will not delete is only logged.
                if (!IsThumbnailPath(publicPath))
                {
                    var thumbPath = Path.Combine(Path.GetDirectoryName(publicPath)!, Constants.Defaults.ThumbnailFolder, Path.GetFileName(publicPath));
                    try
                    {
                        if (File.Exists(thumbPath)) File.Delete(thumbPath);
                    }
                    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                    {
                        _logger.LogWarning(ex, "Could not delete thumbnail {Path}", thumbPath);
                    }
                }
                return Task.CompletedTask;
            }

            // Try secure
            var securePath = Path.GetFullPath(Path.Combine(_secureBase, relativePath.TrimStart('/', '\\')));
            if (IsInside(securePath, _secureRoot) && File.Exists(securePath))
            {
                File.Delete(securePath);
            }

            return Task.CompletedTask;
        }

        // Secure tree first, then public, and only a file that sits inside one of the two roots.
        private string? Resolve(string relativePath)
        {
            var cleaned = CleanRelativePath(relativePath);
            if (cleaned == null) return null;

            var securePath = Path.GetFullPath(Path.Combine(_secureBase, cleaned));
            if (IsInside(securePath, _secureRoot) && File.Exists(securePath)) return securePath;

            var publicPath = Path.GetFullPath(Path.Combine(_publicBase, cleaned));
            if (IsInside(publicPath, _publicRoot) && File.Exists(publicPath)) return publicPath;

            return null;
        }

        // The path comes from a database row. A "..", or a seeded asset path outside the uploads
        // folders, must not reach File.Delete.
        private static bool IsInside(string fullPath, string root)
        {
            var rootFull = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)) + Path.DirectorySeparatorChar;
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return fullPath.StartsWith(rootFull, comparison);
        }
    }
}
