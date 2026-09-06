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

        public SocialAuthConfigService(ApplicationDbContext db)
        {
            _db = db;
        }

        public Task<List<SocialAuthConfig>> GetAllAsync()
            => _db.SocialAuthConfigs.ToListAsync();

        public Task<List<SocialAuthConfig>> GetEnabledAsync()
            => _db.SocialAuthConfigs.Where(c => c.IsEnabled).ToListAsync();

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
