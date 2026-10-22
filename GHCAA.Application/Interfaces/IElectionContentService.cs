using GHCAA.Application.DTOs;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.Interfaces
{
    public interface IElectionContentService
    {
        // Published rows without their bodies, in display order.
        Task<IReadOnlyList<ElectionContentDto>> GetPublishedAsync(ElectionContentKind? kind, CancellationToken cancellationToken = default);

        // One published row with its tokens filled in, or null.
        Task<ElectionContentDto?> GetPublishedAsync(ElectionContentKind kind, string key, CancellationToken cancellationToken = default);

        // Admin reads return the raw text, tokens and all, so a save does not bake today's names in.
        Task<IReadOnlyList<ElectionContentDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ElectionContentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        // Error is a message for a 400 reply. Update throws KeyNotFoundException for an unknown id.
        Task<(ElectionContentDto? Content, string? Error)> CreateAsync(UpsertElectionContentDto dto, int adminId, CancellationToken cancellationToken = default);
        Task<(ElectionContentDto? Content, string? Error)> UpdateAsync(int id, UpsertElectionContentDto dto, int adminId, CancellationToken cancellationToken = default);
    }
}
