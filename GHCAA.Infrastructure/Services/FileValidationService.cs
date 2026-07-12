using System.Linq;
using GHCAA.Application.Interfaces;

namespace GHCAA.Infrastructure.Services
{
    public class FileValidationService : IFileValidationService
    {
        private static readonly string[] ImageContentTypes = { "image/jpeg", "image/png", "image/webp" };
        private static readonly string[] DocumentContentTypes = { "image/jpeg", "image/png", "image/webp", "application/pdf" };

        public FileValidationResult Validate(Stream content, string fileName, string? contentType, long length, FileCategory category, long maxSizeBytes)
        {
            if (length <= 0)
                return FileValidationResult.Fail("No file provided.");

            if (length > maxSizeBytes)
                return FileValidationResult.Fail($"File must be under {maxSizeBytes / (1024 * 1024)}MB.");

            var allowed = category == FileCategory.Image ? ImageContentTypes : DocumentContentTypes;
            var normalizedContentType = contentType?.ToLowerInvariant();
            if (string.IsNullOrEmpty(normalizedContentType) || !allowed.Contains(normalizedContentType))
                return FileValidationResult.Fail("Unsupported file type.");

            if (!content.CanSeek || !content.CanRead)
                return FileValidationResult.Fail("Invalid file stream.");

            var originalPosition = content.Position;
            var header = new byte[12];
            content.Position = 0;
            var bytesRead = content.Read(header, 0, header.Length);
            content.Position = originalPosition;

            if (!MatchesSignature(header, bytesRead, normalizedContentType))
                return FileValidationResult.Fail("File content does not match its declared type.");

            return FileValidationResult.Ok();
        }

        private static bool MatchesSignature(byte[] header, int bytesRead, string contentType)
        {
            switch (contentType)
            {
                case "image/jpeg":
                    return bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
                case "image/png":
                    return bytesRead >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;
                case "image/webp":
                    return bytesRead >= 12
                        && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
                        && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;
                case "application/pdf":
                    return bytesRead >= 4 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46;
                default:
                    return false;
            }
        }
    }
}
