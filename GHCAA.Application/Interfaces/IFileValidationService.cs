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

    // The upload rules and the secure-file download share these lists, so a type accepted on
    // upload is served back with its real content type.
    public static class FileTypes
    {
        public static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        public static readonly string[] DocumentExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
        public static readonly string[] SpreadsheetExtensions = { ".xlsx" };

        public static readonly IReadOnlyDictionary<string, string> ContentTypeByExtension =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [".jpg"] = "image/jpeg",
                [".jpeg"] = "image/jpeg",
                [".png"] = "image/png",
                [".webp"] = "image/webp",
                [".pdf"] = "application/pdf",
                [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            };

        public static string ContentTypeFor(string path) =>
            ContentTypeByExtension.TryGetValue(Path.GetExtension(path), out var type) ? type : "application/octet-stream";
    }

    public interface IFileValidationService
    {
        FileValidationResult Validate(Stream content, string fileName, string? contentType, long length, FileCategory category, long maxSizeBytes);
    }
}
