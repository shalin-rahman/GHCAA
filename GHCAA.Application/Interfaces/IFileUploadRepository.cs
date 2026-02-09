using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface IFileUploadRepository
    {
        Task AddAsync(FileUpload entity, CancellationToken cancellationToken = default);
        Task<FileUpload?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<FileUpload>> GetByMemberAsync(int memberId, CancellationToken cancellationToken = default);
        Task UpdateAsync(FileUpload entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(FileUpload entity, CancellationToken cancellationToken = default);
    }
}