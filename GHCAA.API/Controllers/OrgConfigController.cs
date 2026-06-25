using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/config")]
    public class OrgConfigController(IOrgConfigService configService) : ControllerBase
    {
        // Server-side caching is handled by OrgConfigService (IMemoryCache, 10-min TTL).
        // No [ResponseCache] here — AddResponseCaching middleware is not registered in this project.
        [HttpGet]
        public async Task<IActionResult> GetConfig()
            => Ok(await configService.GetConfigAsync());

        // Strict role parity: Sync with frontend superAdminGuard
        [HttpPut]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> UpdateConfig([FromBody] OrgConfigDto dto)
        {
            if (dto is null)
                return BadRequest("Config payload is required.");

            var adminId = User.FindFirst("MemberId")?.Value ?? string.Empty;
            await configService.UpdateConfigAsync(dto, adminId);
            return NoContent();
        }
    }
}
