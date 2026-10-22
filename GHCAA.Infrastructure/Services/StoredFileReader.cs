using GHCAA.Application.Interfaces;

namespace GHCAA.Infrastructure.Services
{
    // Reads an image a PDF needs (a member photo, the org logo) from the upload store, which may
    // be a bucket rather than wwwroot. Seeded rows point at assets/... files that ship in the web
    // root instead. A missing file gives null and the PDF is drawn without it.
    public static class StoredFileReader
    {
        public static async Task<byte[]?> ReadAsync(IFileStorageService? storage, string? webRootPath, string? path, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var relativePath = path.TrimStart('/', '\\');
            var file = storage == null ? null : await storage.OpenReadAsync(relativePath, cancellationToken);
            if (file == null)
            {
                var webRootFile = WebRootFile(webRootPath, relativePath);
                return webRootFile == null ? null : await File.ReadAllBytesAsync(webRootFile, cancellationToken);
            }

            await using (file.Content)
            {
                using var buffer = new MemoryStream();
                await file.Content.CopyToAsync(buffer, cancellationToken);
                return buffer.ToArray();
            }
        }

        // Only a file that sits inside the web root. The path comes from a database row.
        public static string? WebRootFile(string? webRootPath, string relativePath)
        {
            if (string.IsNullOrEmpty(webRootPath)) return null;

            var rootFull = Path.TrimEndingDirectorySeparator(Path.GetFullPath(webRootPath)) + Path.DirectorySeparatorChar;
            var fullPath = Path.GetFullPath(Path.Combine(rootFull, relativePath));
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return fullPath.StartsWith(rootFull, comparison) && File.Exists(fullPath) ? fullPath : null;
        }
    }
}
