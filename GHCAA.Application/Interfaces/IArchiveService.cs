using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    public interface IArchiveService
    {
        Task<IReadOnlyList<ArchiveCollectionDto>> GetPublicCollectionsAsync(string? search, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ArchiveCollectionDto>> GetAdminCollectionsAsync(CancellationToken cancellationToken = default);
        Task<ArchiveItemDto?> GetPublicItemAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ArchiveItemDto>> GetAdminItemsAsync(string? search, CancellationToken cancellationToken = default);
        Task<ArchiveCollectionDto> CreateCollectionAsync(ArchiveCollectionDto dto, int memberId, CancellationToken cancellationToken = default);
        Task<ArchiveCollectionDto?> UpdateCollectionAsync(int id, ArchiveCollectionDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteCollectionAsync(int id, CancellationToken cancellationToken = default);
        Task<ArchiveItemDto> CreateItemAsync(ArchiveItemDto dto, int memberId, CancellationToken cancellationToken = default);
        Task<ArchiveItemDto?> UpdateItemAsync(int id, ArchiveItemDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteItemAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ModerateItemAsync(int id, GHCAA.Domain.Enums.ArchiveModerationState state, GHCAA.Domain.Enums.ArchivePublicationState publication, CancellationToken cancellationToken = default);
    }
}
