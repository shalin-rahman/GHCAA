using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Infrastructure.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkiaSharp;

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
            var storedFileName = $"{prefix}_{safeFileName}";
            var relativeRoot = IsSecureType(uploadType)
                    ? _options.SecureRelativePath
                    : _options.UploadsRelativePath;

            // Keep stored paths grouped by member and upload type.
            return Path.Combine(relativeRoot, memberId.ToString(), prefix, storedFileName).Replace("\\", "/");
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

            var uniqueName = $"{prefix}_{Guid.NewGuid():N}_{safeFileName}";
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

                    // SKBitmap.Decode(Stream) closes the stream, and the fallback below needs it open.
                    using var buffer = new MemoryStream();
                    await fileStream.CopyToAsync(buffer, cancellationToken);
                    var image = DecodeUpright(buffer.ToArray())
                        ?? throw new InvalidDataException("The uploaded file is not a readable image.");

                    try
                    {
                        if (image.Width > MaxDimensionPx || image.Height > MaxDimensionPx)
                            image = FitWithin(image, MaxDimensionPx, MaxDimensionPx);

                        var targetBytes = TargetSizeKB * 1024L;
                        var quality = Math.Clamp(DefaultQuality, 1, 100);
                        var minimumQuality = Math.Clamp(FallbackQuality, 1, quality);
                        byte[] encoded;

                        while (true)
                        {
                            encoded = EncodeJpeg(image, quality);

                            if (encoded.Length <= targetBytes)
                                break;

                            if (quality > minimumQuality)
                            {
                                quality = Math.Max(minimumQuality, quality - 10);
                                continue;
                            }

                            if (Math.Max(image.Width, image.Height) <= 320)
                            {
                                throw new InvalidOperationException(
                                    $"Unable to encode {uploadType} within the configured image size cap of {TargetSizeKB} KB.");
                            }

                            image = FitWithin(image, Math.Max(320, (int)(image.Width * 0.85)), Math.Max(320, (int)(image.Height * 0.85)));
                            quality = Math.Clamp(DefaultQuality, 1, 100);
                        }

                        await File.WriteAllBytesAsync(diskPath, encoded, cancellationToken);

                        if (uploadType == Enums.FileUploadType.GalleryPhoto)
                            await WriteThumbnailAsync(image, targetDir, uniqueName, cancellationToken);
                    }
                    finally
                    {
                        image.Dispose();
                    }
                }
                catch (Exception ex) when (ex is not InvalidOperationException)
                {
                    _logger.LogError(ex, "Failed to compress/save photo. Falling back to direct copy.");
                    if (fileStream.CanSeek) fileStream.Position = 0;
                    await using (var fs = new FileStream(diskPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        await fileStream.CopyToAsync(fs, cancellationToken);

                    if (uploadType == Enums.FileUploadType.GalleryPhoto)
                        await WriteThumbnailFromFileAsync(diskPath, targetDir, uniqueName, cancellationToken);
                }
            }
            else
            {
                await using (var fs = new FileStream(diskPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    await fileStream.CopyToAsync(fs, cancellationToken);

                if (uploadType == Enums.FileUploadType.GalleryPhoto)
                    await WriteThumbnailFromFileAsync(diskPath, targetDir, uniqueName, cancellationToken);
            }

            var webRelative = Path.Combine(relativePrefix, memberId.ToString(), prefix, uniqueName).Replace("\\", "/");
            _logger.LogInformation("Saved {Type} file to {Path}", isSecure ? "secure" : "public", webRelative);
            return webRelative;
        }

        // 98.3: gallery grids load this instead of the full photo. A failure here only costs the
        // grid its small copy, so it is logged and the upload still succeeds.
        private async Task WriteThumbnailAsync(SKBitmap image, string targetDir, string fileName, CancellationToken cancellationToken)
        {
            try
            {
                var thumbDir = Path.Combine(targetDir, Constants.Defaults.ThumbnailFolder);
                Directory.CreateDirectory(thumbDir);

                var maxPx = Constants.Defaults.GalleryThumbnailMaxPx;
                var format = FormatFor(fileName);
                if (format == null)
                {
                    _logger.LogWarning("No thumbnail format matches {File}, so it has no gallery thumbnail", fileName);
                    return;
                }

                var copy = image.Copy() ?? throw new InvalidDataException("Could not copy the image for its thumbnail.");
                using var thumb = FitWithin(copy, maxPx, maxPx);
                await File.WriteAllBytesAsync(Path.Combine(thumbDir, fileName), Encode(thumb, format.Value, DefaultQuality), cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Could not write gallery thumbnail for {File}", fileName);
            }
        }

        // A photo stored as uploaded (compression off, or the compressor fell back) still gets its
        // grid copy. A file that will not decode as an image just goes without one, and the upload
        // still succeeds.
        private async Task WriteThumbnailFromFileAsync(string diskPath, string targetDir, string fileName, CancellationToken cancellationToken)
        {
            try
            {
                using var image = DecodeUpright(await File.ReadAllBytesAsync(diskPath, cancellationToken));
                if (image == null)
                {
                    _logger.LogWarning("Could not read {File} as an image, so it has no gallery thumbnail", fileName);
                    return;
                }

                await WriteThumbnailAsync(image, targetDir, fileName, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Could not write gallery thumbnail for {File}", fileName);
            }
        }

        public string? GetThumbnailPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return null;

            var normalized = relativePath.Replace("\\", "/").TrimStart('/');
            var uploadsPrefix = _options.UploadsRelativePath.Replace("\\", "/").Trim('/') + "/";
            if (!normalized.StartsWith(uploadsPrefix, StringComparison.OrdinalIgnoreCase)
                || normalized.Split('/').Any(part => part == ".."))
                return null;

            var lastSlash = normalized.LastIndexOf('/');
            var thumbRelative = $"{normalized[..lastSlash]}/{Constants.Defaults.ThumbnailFolder}/{normalized[(lastSlash + 1)..]}";
            var physical = Path.Combine(_options.BasePhysicalPath ?? "wwwroot", thumbRelative);
            if (!File.Exists(physical)) return null;

            // Same leading-slash shape the caller passed in.
            return relativePath.StartsWith('/') ? "/" + thumbRelative : thumbRelative;
        }

        // Scales down to fit inside maxW x maxH, keeping the aspect ratio. Disposes the source bitmap.
        // Phone cameras store the pixels sideways and record the turn in EXIF. SKBitmap.Decode ignores
        // the tag and a re-encoded copy drops it, so the pixels are turned upright here.
        private static SKBitmap? DecodeUpright(byte[] bytes)
        {
            using var data = SKData.CreateCopy(bytes);
            using var codec = SKCodec.Create(data);
            if (codec == null || (long)codec.Info.Width * codec.Info.Height > Constants.Defaults.MaxDecodePixels) return null;
            var decoded = SKBitmap.Decode(codec);
            var origin = codec.EncodedOrigin;
            if (decoded == null || origin == SKEncodedOrigin.TopLeft) return decoded;
            using var source = decoded;

            float w = decoded.Width, h = decoded.Height;
            // Each matrix moves a stored pixel to where it belongs on screen.
            var matrix = origin switch
            {
                SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, w, 0, 1, 0, 0, 0, 1),
                SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, w, 0, -1, h, 0, 0, 1),
                SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, h, 0, 0, 1),
                SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),
                SKEncodedOrigin.RightTop => new SKMatrix(0, -1, h, 1, 0, 0, 0, 0, 1),
                SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, h, -1, 0, w, 0, 0, 1),
                _ => new SKMatrix(0, 1, 0, -1, 0, w, 0, 0, 1),
            };
            var turned = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
            // A canvas cannot draw into every decoded color type (grayscale JPEGs among them), so the
            // upright copy uses the platform's own.
            var info = new SKImageInfo(turned ? decoded.Height : decoded.Width, turned ? decoded.Width : decoded.Height,
                SKImageInfo.PlatformColorType, decoded.AlphaType == SKAlphaType.Unknown ? SKAlphaType.Premul : decoded.AlphaType);
            var upright = new SKBitmap(info);
            try
            {
                using var canvas = new SKCanvas(upright);
                canvas.SetMatrix(matrix);
                canvas.DrawBitmap(decoded, 0, 0);
            }
            catch
            {
                upright.Dispose();
                throw;
            }
            return upright;
        }

        private static SKBitmap FitWithin(SKBitmap source, int maxW, int maxH)
        {
            var scale = Math.Min((double)maxW / source.Width, (double)maxH / source.Height);
            if (scale >= 1) return source;
            var info = new SKImageInfo(Math.Max(1, (int)Math.Round(source.Width * scale)), Math.Max(1, (int)Math.Round(source.Height * scale)));
            var resized = source.Resize(info, new SKSamplingOptions(SKCubicResampler.Mitchell));
            if (resized == null) return source;
            source.Dispose();
            return resized;
        }

        // The thumbnail keeps the photo's file name, so it is encoded to match that extension. Skia
        // cannot write GIF, BMP or HEIC, so those get no thumbnail rather than JPEG bytes under the
        // wrong name. Uploads only allow the four below today.
        private static SKEncodedImageFormat? FormatFor(string fileName) =>
            Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => SKEncodedImageFormat.Jpeg,
                ".png" => SKEncodedImageFormat.Png,
                ".webp" => SKEncodedImageFormat.Webp,
                _ => null,
            };

        private static byte[] Encode(SKBitmap image, SKEncodedImageFormat format, int quality)
        {
            using var skImage = SKImage.FromBitmap(image);
            using var data = skImage.Encode(format, quality);
            return data.ToArray();
        }

        private static byte[] EncodeJpeg(SKBitmap image, int quality)
        {
            using var skImage = SKImage.FromBitmap(image);
            using var data = skImage.Encode(SKEncodedImageFormat.Jpeg, quality);
            return data.ToArray();
        }

        public Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            var basePath = _options.BasePhysicalPath;

            // Try public first
            var publicPath = Path.GetFullPath(Path.Combine(basePath ?? "wwwroot", relativePath.TrimStart('/', '\\')));
            if (IsInside(publicPath, _publicRoot) && File.Exists(publicPath))
            {
                File.Delete(publicPath);

                // A gallery photo's thumbnail sits beside it under thumbs/ with the same name.
                var thumbPath = Path.Combine(Path.GetDirectoryName(publicPath)!, Constants.Defaults.ThumbnailFolder, Path.GetFileName(publicPath));
                if (File.Exists(thumbPath)) File.Delete(thumbPath);
                return Task.CompletedTask;
            }

            // Try secure
            var securePath = Path.GetFullPath(Path.Combine(basePath ?? AppDomain.CurrentDomain.BaseDirectory, relativePath.TrimStart('/', '\\')));
            if (IsInside(securePath, _secureRoot) && File.Exists(securePath))
            {
                File.Delete(securePath);
            }

            return Task.CompletedTask;
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
