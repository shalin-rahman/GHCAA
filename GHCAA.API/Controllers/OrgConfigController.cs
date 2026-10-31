using GHCAA.API.Extensions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
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
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetConfig()
            => Ok(await configService.GetConfigAsync());

        // Strict role parity: Sync with frontend superAdminGuard
        [HttpPut]
        [Authorize(Policy = Constants.Policies.SuperAdminOnly)]
        public async Task<IActionResult> UpdateConfig([FromBody] OrgConfigDto dto)
        {
            if (dto is null)
                return Problem(detail: "Config payload is required.", statusCode: StatusCodes.Status400BadRequest);

            var adminId = this.CurrentMemberIdRaw() ?? string.Empty;
            try
            {
                await configService.UpdateConfigAsync(dto, adminId);
            }
            catch (ArgumentException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
            }
            return NoContent();
        }
    }
}
