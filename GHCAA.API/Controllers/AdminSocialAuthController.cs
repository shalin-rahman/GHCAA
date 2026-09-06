using GHCAA.Domain.Models;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GHCAA.Domain.Enums;
using GHCAA.Domain;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/admin/social-auth")]
    [Authorize(Policy = Constants.Policies.AdminOnly)]
    public class AdminSocialAuthController : ControllerBase
    {
        private readonly ISocialAuthConfigService _socialAuthConfigService;

        public AdminSocialAuthController(ISocialAuthConfigService socialAuthConfigService)
        {
            _socialAuthConfigService = socialAuthConfigService;
        }

        [HttpGet]
        public async Task<IActionResult> GetConfigs()
        {
            var configs = await _socialAuthConfigService.GetAllAsync();
            return Ok(configs.Select(c => new
            {
                c.Id,
                c.Provider,
                c.ClientId,
                ClientSecret = string.IsNullOrEmpty(c.ClientSecret) ? null : "••••••••",
                c.IsEnabled,
                c.UpdatedAt
            }));
        }

        [HttpPut("{provider}")]
        public async Task<IActionResult> UpdateConfig(SocialProvider provider, [FromBody] SocialAuthConfig updateDto)
        {
            var config = await _socialAuthConfigService.UpsertAsync(provider, updateDto);
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
            var config = await _socialAuthConfigService.ToggleAsync(provider);
            if (config == null) return NotFound();
            return Ok(new { IsEnabled = config.IsEnabled });
        }
    }
}
