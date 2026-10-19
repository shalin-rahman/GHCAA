using GHCAA.Domain;

namespace GHCAA.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(Stream fileStream, string fileName, int memberId, Enums.FileUploadType uploadType, CancellationToken cancellationToken = default);
        Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default);
        string GetRelativeFilePath(int memberId, Enums.FileUploadType uploadType, string fileName);
        // Gallery thumbnail written beside an uploaded photo, or null when there isn't one.
        string? GetThumbnailPath(string relativePath);
        // Opens a stored file for reading, or returns null when it is missing or the path leaves
        // the uploads and secure_uploads trees. The caller disposes the content stream.
        Task<StoredFile?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);
        // Health check: can the store be reached right now.
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    }

    // Content can seek and knows its length, so callers can answer range requests. LastModified
    // and ETag let them answer If-None-Match and If-Modified-Since with a 304.
    public sealed record StoredFile(Stream Content, DateTimeOffset? LastModified, string? ETag);
}
