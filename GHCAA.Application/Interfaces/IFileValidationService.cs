using System.IO;

namespace GHCAA.Application.Interfaces
{
    public enum FileCategory
    {
        Image,
        Document,
        Spreadsheet
    }

    public class FileValidationResult
    {
        public bool IsValid { get; init; }
        public string? ErrorMessage { get; init; }

        public static FileValidationResult Ok() => new() { IsValid = true };
        public static FileValidationResult Fail(string message) => new() { IsValid = false, ErrorMessage = message };
    }

    public interface IFileValidationService
    {
        FileValidationResult Validate(Stream content, string fileName, string? contentType, long length, FileCategory category, long maxSizeBytes);
    }
}
