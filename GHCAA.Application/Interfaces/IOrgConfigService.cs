using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    // ISP: read + write are the only public concerns; cache invalidation is an implementation detail
    public interface IOrgConfigService
    {
        Task<OrgConfigDto> GetConfigAsync();
        Task UpdateConfigAsync(OrgConfigDto dto, string updatedByAdminId);
    }
}
