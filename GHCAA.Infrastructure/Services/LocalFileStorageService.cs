using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Infrastructure.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace GHCAA.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _publicRoot;
        private readonly string _secureRoot;
        private readonly long _maxFileSize;
        private readonly FileStorageOptions _options;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(IOptions<FileStorageOptions> options, ILogger<LocalFileStorageService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _options = options.Value;
            _logger = logger;

            var publicRelative = _options.UploadsRelativePath;
            var secureRelative = _options.SecureRelativePath;

            // Normalize relative paths to use forward slashes for cross-platform consistency
            publicRelative = publicRelative.Replace("\\", "/").TrimEnd('/');
            secureRelative = secureRelative.Replace("\\", "/").TrimEnd('/');

            _publicRoot = Path.Combine(_options.BasePhysicalPath ?? "wwwroot", publicRelative);
            _secureRoot = Path.Combine(_options.BasePhysicalPath ?? AppDomain.CurrentDomain.BaseDirectory, secureRelative);

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

            _maxFileSize = _options.MaxFileSizeBytes;
        }

        private bool IsCompressionEnabled => _options.ImageCompression.Enabled;
        private int DefaultQuality => _options.ImageCompression.Quality;
        private int FallbackQuality => _options.ImageCompression.FallbackQuality;
        private int TargetSizeKB => _options.ImageCompression.TargetSizeKB;
        private int MaxDimensionPx => _options.ImageCompression.MaxDimensionPx;

        private bool IsSecureType(Enums.FileUploadType type)
        {
            // 29B.6: Signatures are sensitive (forgery risk) and must live under the auth-gated
            // secure_uploads tree, not the publicly served uploads tree.
            return type == Enums.FileUploadType.Certificate
                || type == Enums.FileUploadType.PaymentProof
                || type == Enums.FileUploadType.Signature;
        }

        // Only types that are always a plain display image get recompressed. Certificate/
        // NoticeDocument are frequently PDFs, and PaymentProof/Signature must keep pixel-for-pixel
        // fidelity (evidentiary receipt, legal signature) even when the upload happens to be a JPEG
        // — so those are never touched here regardless of file content.
        private bool IsCompressibleImageType(Enums.FileUploadType type)
        {
            return type == Enums.FileUploadType.Photo
                || type == Enums.FileUploadType.GalleryPhoto
                || type == Enums.FileUploadType.NewsImage;
        }

        public string GetRelativeFilePath(int memberId, Enums.FileUploadType uploadType, string fileName)
        {
            var safeFileName = Path.GetFileName(fileName);
            var prefix = uploadType.ToString().ToLower();
            var relativeRoot = IsSecureType(uploadType)
                    ? _options.SecureRelativePath
                    : _options.UploadsRelativePath;

            // World-class nested structure: members/{id}/{type}/{fileName}
            return Path.Combine(relativeRoot, memberId.ToString(), prefix, safeFileName).Replace("\\", "/");
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, int memberId, Enums.FileUploadType uploadType, CancellationToken cancellationToken = default)
        {
            if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
            if (fileStream.Length > _maxFileSize) throw new InvalidOperationException($"File exceeds maximum size {_maxFileSize} bytes.");

            var isSecure = IsSecureType(uploadType);
            var rootPath = isSecure ? _secureRoot : _publicRoot;
            var safeFileName = Path.GetFileName(fileName);
            var prefix = uploadType.ToString().ToLower();

            // Create nested directory for member and upload type
            var relativePrefix = isSecure ? _options.SecureRelativePath : _options.UploadsRelativePath;

            var targetDir = Path.Combine(rootPath, memberId.ToString(), prefix);
            Directory.CreateDirectory(targetDir);

            var uniqueName = $"{Guid.NewGuid():N}_{safeFileName}";
            var willCompress = IsCompressibleImageType(uploadType) && IsCompressionEnabled;
            // Ensure .jpg extension for images we compress, since they're always re-encoded as JPEG
            if (willCompress)
            {
                uniqueName = Path.ChangeExtension(uniqueName, ".jpg");
            }

            var diskPath = Path.Combine(targetDir, uniqueName);

            if (willCompress)
            {
                try
                {
                    // Reset position just in case
                    if (fileStream.CanSeek) fileStream.Position = 0;

                    using var image = await Image.LoadAsync(fileStream, cancellationToken);

                    if (image.Width > MaxDimensionPx || image.Height > MaxDimensionPx)
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Max,
                            Size = new Size(MaxDimensionPx, MaxDimensionPx)
                        }));
                    }

                    var encoder = new JpegEncoder { Quality = DefaultQuality };

                    using var ms = new MemoryStream();
                    await image.SaveAsJpegAsync(ms, encoder, cancellationToken);

                    // Check against target size
                    if (ms.Length > (TargetSizeKB * 1024))
                    {
                        encoder = new JpegEncoder { Quality = FallbackQuality };
                    }

                    await image.SaveAsJpegAsync(diskPath, encoder, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to compress/save photo. Falling back to direct copy.");
                    if (fileStream.CanSeek) fileStream.Position = 0;
                    using var fs = new FileStream(diskPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    await fileStream.CopyToAsync(fs, cancellationToken);
                }
            }
            else
            {
                using var fs = new FileStream(diskPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await fileStream.CopyToAsync(fs, cancellationToken);
            }

            var webRelative = Path.Combine(relativePrefix, memberId.ToString(), prefix, uniqueName).Replace("\\", "/");
            _logger.LogInformation("Saved {Type} file to {Path}", isSecure ? "secure" : "public", webRelative);
            return webRelative;
        }

        public Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            var basePath = _options.BasePhysicalPath;

            // Try public first
            var publicPath = Path.Combine(basePath ?? "wwwroot", relativePath.TrimStart('/', '\\'));
            if (File.Exists(publicPath))
            {
                File.Delete(publicPath);
                return Task.CompletedTask;
            }

            // Try secure
            var securePath = Path.Combine(basePath ?? AppDomain.CurrentDomain.BaseDirectory, relativePath.TrimStart('/', '\\'));
            if (File.Exists(securePath))
            {
                File.Delete(securePath);
            }

            return Task.CompletedTask;
        }
    }
}
