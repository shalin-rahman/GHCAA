using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services
{
    public class SocialAuthConfigService : ISocialAuthConfigService
    {
        private readonly ApplicationDbContext _db;
        private readonly IOrgConfigService _orgConfig;

        public SocialAuthConfigService(ApplicationDbContext db, IOrgConfigService orgConfig)
        {
            _db = db;
            _orgConfig = orgConfig;
        }

        public Task<List<SocialAuthConfig>> GetAllAsync()
            => _db.SocialAuthConfigs.ToListAsync();

        // The one check the providers list and both sign-in endpoints share, so the login page
        // never shows a button the server would refuse.
        public async Task<List<SocialAuthConfig>> GetUsableAsync(CancellationToken ct = default)
        {
            if (!(await _orgConfig.GetConfigAsync()).Features.EnableSocialAuth) return [];
            var enabled = await _db.SocialAuthConfigs.AsNoTracking().Where(c => c.IsEnabled).ToListAsync(ct);
            return enabled.Where(HasKeys).ToList();
        }

        public async Task<SocialAuthConfig?> FindUsableAsync(SocialProvider provider, CancellationToken ct = default)
            => (await GetUsableAsync(ct)).FirstOrDefault(c => c.Provider == provider);

        // Facebook also needs the app secret to check which app a token was issued for (29B.1).
        private static bool HasKeys(SocialAuthConfig c) =>
            !string.IsNullOrWhiteSpace(c.ClientId)
            && (c.Provider != SocialProvider.Facebook || !string.IsNullOrWhiteSpace(c.ClientSecret));

        public async Task<SocialAuthConfig> UpsertAsync(SocialProvider provider, SocialAuthConfig update)
        {
            var config = await _db.SocialAuthConfigs.FirstOrDefaultAsync(c => c.Provider == provider);
            if (config == null)
            {
                config = new SocialAuthConfig { Provider = provider };
                _db.SocialAuthConfigs.Add(config);
            }

            config.ClientId = update.ClientId;
            config.ClientSecret = update.ClientSecret;
            config.IsEnabled = update.IsEnabled;
            config.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return config;
        }

        public async Task<SocialAuthConfig?> ToggleAsync(SocialProvider provider)
        {
            var config = await _db.SocialAuthConfigs.FirstOrDefaultAsync(c => c.Provider == provider);
            if (config == null) return null;

            config.IsEnabled = !config.IsEnabled;
            config.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return config;
        }
    }
}
