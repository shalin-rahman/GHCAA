using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    public interface ISiteContentService
    {
        Task<IEnumerable<SiteContentDto>> GetActiveByGroupAsync(string group, CancellationToken cancellationToken = default);
        Task<IEnumerable<SiteContentDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<SiteContentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<SiteContentDto> CreateAsync(UpsertSiteContentDto dto, int adminId, CancellationToken cancellationToken = default);
        Task<SiteContentDto> UpdateAsync(int id, UpsertSiteContentDto dto, int adminId, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
