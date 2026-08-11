using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GHCAA.Domain.Enums;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/admin/social-auth")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminSocialAuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public AdminSocialAuthController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetConfigs()
        {
            var configs = await _db.SocialAuthConfigs
                .Select(c => new
                {
                    c.Id,
                    c.Provider,
                    c.ClientId,
                    ClientSecret = string.IsNullOrEmpty(c.ClientSecret) ? null : "••••••••",
                    c.IsEnabled,
                    c.UpdatedAt
                })
                .ToListAsync();
            return Ok(configs);
        }

        [HttpPut("{provider}")]
        public async Task<IActionResult> UpdateConfig(SocialProvider provider, [FromBody] SocialAuthConfig updateDto)
        {
            var config = await _db.SocialAuthConfigs.FirstOrDefaultAsync(c => c.Provider == provider);

            if (config == null)
            {
                config = new SocialAuthConfig { Provider = provider };
                _db.SocialAuthConfigs.Add(config);
            }

            config.ClientId = updateDto.ClientId;
            config.ClientSecret = updateDto.ClientSecret;
            config.IsEnabled = updateDto.IsEnabled;
            config.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(new
            {
                config.Id,
                config.Provider,
                config.ClientId,
                ClientSecret = string.IsNullOrEmpty(config.ClientSecret) ? null : "••••••••",
                config.IsEnabled,
                config.UpdatedAt
            });
        }

        [HttpPost("{provider}/toggle")]
        public async Task<IActionResult> Toggle(SocialProvider provider)
        {
            var config = await _db.SocialAuthConfigs.FirstOrDefaultAsync(c => c.Provider == provider);
            if (config == null) return NotFound();

            config.IsEnabled = !config.IsEnabled;
            config.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(new { IsEnabled = config.IsEnabled });
        }
    }
}
