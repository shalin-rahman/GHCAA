using System.IO;
using GHCAA.Domain;

namespace GHCAA.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(Stream fileStream, string fileName, int memberId, Enums.FileUploadType uploadType, CancellationToken cancellationToken = default);
        Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default);
        string GetRelativeFilePath(int memberId, Enums.FileUploadType uploadType, string fileName);
    }
}