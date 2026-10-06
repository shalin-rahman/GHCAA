using GHCAA.Domain.Models;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.Interfaces
{
    public interface ISocialAuthConfigService
    {
        Task<List<SocialAuthConfig>> GetAllAsync();
        // 7.17. Providers that can sign someone in right now. Empty when EnableSocialAuth is off.
        Task<List<SocialAuthConfig>> GetUsableAsync(CancellationToken ct = default);
        Task<SocialAuthConfig?> FindUsableAsync(SocialProvider provider, CancellationToken ct = default);
        Task<SocialAuthConfig> UpsertAsync(SocialProvider provider, SocialAuthConfig update);
        Task<SocialAuthConfig?> ToggleAsync(SocialProvider provider);
    }
}
