using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace GHCAA.API.Extensions
{
    public static class FileValidationExtensions
    {
        public static FileValidationResult ValidateFormFile(this IFileValidationService service, IFormFile? file, FileCategory category, long maxSizeBytes)
        {
            if (file == null)
                return FileValidationResult.Fail("No file provided.");

            using var stream = file.OpenReadStream();
            return service.Validate(stream, file.FileName, file.ContentType, file.Length, category, maxSizeBytes);
        }
    }
}
