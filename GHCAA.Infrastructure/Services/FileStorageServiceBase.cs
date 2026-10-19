using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace GHCAA.Infrastructure.Services
{
    // Naming, compression and thumbnails are the same whether a file lands on disk or in a bucket,
    // so a stored path means the same thing under either provider. Subclasses only move bytes.
    public abstract class FileStorageServiceBase : IFileStorageService
    {
        protected readonly FileStorageOptions _options;
        protected readonly ILogger _logger;
        private readonly long _maxFileSize;

        protected FileStorageServiceBase(FileStorageOptions options, ILogger logger)
        {
            _options = options;
            _logger = logger;
            _maxFileSize = _options.MaxFileSizeBytes;
        }

        private bool IsCompressionEnabled => _options.ImageCompression.Enabled;
        private int DefaultQuality => _options.ImageCompression.Quality;
        private int FallbackQuality => _options.ImageCompression.FallbackQuality;
        private int TargetSizeKB => _options.ImageCompression.TargetSizeKB;
        private int MaxDimensionPx => _options.ImageCompression.MaxDimensionPx;

        protected string UploadsPrefix => Normalize(_options.UploadsRelativePath).Trim('/') + "/";
        protected string SecurePrefix => Normalize(_options.SecureRelativePath).Trim('/') + "/";

        // Writes one file at its stored path, for example uploads/members/12/photo/photo_<guid>_a.jpg.
        protected abstract Task WriteAsync(string relativePath, Stream content, bool isSecure, CancellationToken cancellationToken);

        // Says whether a gallery thumbnail can be served from this path.
        protected abstract bool ThumbnailExists(string thumbnailRelativePath);

        public abstract Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default);
        public abstract Task<StoredFile?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);
        public abstract Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

        protected static bool IsSecureType(Enums.FileUploadType type)
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
        private static bool IsCompressibleImageType(Enums.FileUploadType type)
        {
            return type == Enums.FileUploadType.Photo
                || type == Enums.FileUploadType.GalleryPhoto
                || type == Enums.FileUploadType.NewsImage;
        }

        protected static string Normalize(string path) => path.Replace("\\", "/");

        // A stored path with the leading slash removed, or null when it holds a "..". Paths come
        // from database rows and request URLs, so none may climb out of the storage root.
        protected static string? CleanRelativePath(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return null;
            var normalized = Normalize(relativePath).TrimStart('/');
            return normalized.Split('/').Any(part => part == "..") ? null : normalized;
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
            return Normalize(Path.Combine(relativeRoot, memberId.ToString(), prefix, storedFileName));
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, int memberId, Enums.FileUploadType uploadType, CancellationToken cancellationToken = default)
        {
            if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
            if (fileStream.Length > _maxFileSize) throw new InvalidOperationException($"File exceeds maximum size {_maxFileSize} bytes.");

            var isSecure = IsSecureType(uploadType);
            var safeFileName = Path.GetFileName(fileName);
            var prefix = uploadType.ToString().ToLower();
            var relativePrefix = isSecure ? _options.SecureRelativePath : _options.UploadsRelativePath;
            var relativeDir = Normalize(Path.Combine(relativePrefix, memberId.ToString(), prefix));

            var uniqueName = $"{prefix}_{Guid.NewGuid():N}_{safeFileName}";
            var willCompress = IsCompressibleImageType(uploadType) && IsCompressionEnabled;
            // Ensure .jpg extension for images we compress, since they're always re-encoded as JPEG
            if (willCompress)
            {
                uniqueName = Path.ChangeExtension(uniqueName, ".jpg");
            }

            var relativePath = $"{relativeDir}/{uniqueName}";
            var isGallery = uploadType == Enums.FileUploadType.GalleryPhoto;

            if (willCompress || isGallery)
            {
                if (fileStream.CanSeek) fileStream.Position = 0;

                // SKBitmap.Decode(Stream) closes the stream, and both the fallback and the
                // thumbnail need the bytes again, so the upload is read once into memory.
                byte[] original;
                using (var buffer = new MemoryStream())
                {
                    await fileStream.CopyToAsync(buffer, cancellationToken);
                    original = buffer.ToArray();
                }

                if (willCompress)
                {
                    SKBitmap? image = null;
                    try
                    {
                        // Only decoding and encoding may fall back to the original bytes. A failed
                        // write is a storage fault and goes to the caller, so it is never sent twice.
                        var toStore = original;
                        try
                        {
                            image = DecodeUpright(original)
                                ?? throw new InvalidDataException("The uploaded file is not a readable image.");
                            toStore = CompressToTarget(ref image, uploadType);
                        }
                        catch (Exception ex) when (ex is not InvalidOperationException and not OperationCanceledException)
                        {
                            _logger.LogError(ex, "Failed to compress photo. Storing it as uploaded.");
                            image?.Dispose();
                            image = null;
                        }

                        await WriteBytesAsync(relativePath, toStore, isSecure, cancellationToken);

                        if (isGallery)
                        {
                            if (image != null)
                                await WriteThumbnailAsync(image, relativeDir, uniqueName, cancellationToken);
                            else
                                await WriteThumbnailFromBytesAsync(original, relativeDir, uniqueName, cancellationToken);
                        }
                    }
                    finally
                    {
                        image?.Dispose();
                    }
                }
                else
                {
                    await WriteBytesAsync(relativePath, original, isSecure, cancellationToken);
                    await WriteThumbnailFromBytesAsync(original, relativeDir, uniqueName, cancellationToken);
                }
            }
            else
            {
                if (fileStream.CanSeek) fileStream.Position = 0;
                await WriteAsync(relativePath, fileStream, isSecure, cancellationToken);
            }

            _logger.LogInformation("Saved {Type} file to {Path}", isSecure ? "secure" : "public", relativePath);
            return relativePath;
        }

        // Lowers the quality first, then the size, until the JPEG fits under TargetSizeKB. May
        // swap the bitmap for a smaller one, so it takes the image by ref.
        private byte[] CompressToTarget(ref SKBitmap image, Enums.FileUploadType uploadType)
        {
            if (image.Width > MaxDimensionPx || image.Height > MaxDimensionPx)
                image = FitWithin(image, MaxDimensionPx, MaxDimensionPx);

            var targetBytes = TargetSizeKB * 1024L;
            var quality = Math.Clamp(DefaultQuality, 1, 100);
            var minimumQuality = Math.Clamp(FallbackQuality, 1, quality);

            while (true)
            {
                var encoded = EncodeJpeg(image, quality);

                if (encoded.Length <= targetBytes)
                    return encoded;

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
        }

        private async Task WriteBytesAsync(string relativePath, byte[] bytes, bool isSecure, CancellationToken cancellationToken)
        {
            using var content = new MemoryStream(bytes, writable: false);
            await WriteAsync(relativePath, content, isSecure, cancellationToken);
        }

        // 98.3: gallery grids load this instead of the full photo. A failure here only costs the
        // grid its small copy, so it is logged and the upload still succeeds.
        private async Task WriteThumbnailAsync(SKBitmap image, string relativeDir, string fileName, CancellationToken cancellationToken)
        {
            try
            {
                var format = FormatFor(fileName);
                if (format == null)
                {
                    _logger.LogWarning("No thumbnail format matches {File}, so it has no gallery thumbnail", fileName);
                    return;
                }

                var maxPx = Constants.Defaults.GalleryThumbnailMaxPx;
                var copy = image.Copy() ?? throw new InvalidDataException("Could not copy the image for its thumbnail.");
                using var thumb = FitWithin(copy, maxPx, maxPx);
                var thumbPath = $"{relativeDir}/{Constants.Defaults.ThumbnailFolder}/{fileName}";
                await WriteBytesAsync(thumbPath, Encode(thumb, format.Value, DefaultQuality), isSecure: false, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Could not write gallery thumbnail for {File}", fileName);
            }
        }

        // A photo stored as uploaded (compression off, or the compressor fell back) still gets its
        // grid copy. A file that will not decode as an image just goes without one, and the upload
        // still succeeds.
        private async Task WriteThumbnailFromBytesAsync(byte[] bytes, string relativeDir, string fileName, CancellationToken cancellationToken)
        {
            try
            {
                using var image = DecodeUpright(bytes);
                if (image == null)
                {
                    _logger.LogWarning("Could not read {File} as an image, so it has no gallery thumbnail", fileName);
                    return;
                }

                await WriteThumbnailAsync(image, relativeDir, fileName, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Could not write gallery thumbnail for {File}", fileName);
            }
        }

        public string? GetThumbnailPath(string relativePath)
        {
            var normalized = CleanRelativePath(relativePath);
            if (normalized == null || !normalized.StartsWith(UploadsPrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var thumbRelative = ThumbnailPathFor(normalized);
            if (!ThumbnailExists(thumbRelative)) return null;

            // Same leading-slash shape the caller passed in.
            return relativePath.StartsWith('/') ? "/" + thumbRelative : thumbRelative;
        }

        // True when the file already sits in a thumbs/ folder, so it has no thumbnail of its own.
        protected static bool IsThumbnailPath(string normalizedPath)
        {
            var parts = Normalize(normalizedPath).Split('/');
            return parts.Length >= 2 && string.Equals(parts[^2], Constants.Defaults.ThumbnailFolder, StringComparison.OrdinalIgnoreCase);
        }

        protected static string ThumbnailPathFor(string normalizedPath)
        {
            var lastSlash = normalizedPath.LastIndexOf('/');
            return $"{normalizedPath[..lastSlash]}/{Constants.Defaults.ThumbnailFolder}/{normalizedPath[(lastSlash + 1)..]}";
        }

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

        // Scales down to fit inside maxW x maxH, keeping the aspect ratio. Disposes the source bitmap.
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

        private static byte[] EncodeJpeg(SKBitmap image, int quality) => Encode(image, SKEncodedImageFormat.Jpeg, quality);
    }
}
