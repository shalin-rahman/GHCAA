using GHCAA.Domain.Models;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.Interfaces
{
    public interface ISocialAuthConfigService
    {
        Task<List<SocialAuthConfig>> GetAllAsync();
        Task<List<SocialAuthConfig>> GetEnabledAsync();
        Task<SocialAuthConfig> UpsertAsync(SocialProvider provider, SocialAuthConfig update);
        Task<SocialAuthConfig?> ToggleAsync(SocialProvider provider);
    }
}
