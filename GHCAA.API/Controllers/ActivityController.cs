using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GHCAA.Domain;
using GHCAA.Application.Security;
using GHCAA.API.Extensions;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/activity")]
    [Authorize]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityService _activityService;

        public ActivityController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyActivity(CancellationToken cancellationToken)
        {
            var memberIdClaim = this.CurrentMemberIdRaw();
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId))
                return Problem(detail: "Invalid user session", statusCode: StatusCodes.Status400BadRequest);

            var logs = await _activityService.GetMemberActivityAsync(memberId, 20, cancellationToken);
            return Ok(logs);
        }

        [HttpGet("admin/{memberId}")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> GetMemberActivity(int memberId, CancellationToken cancellationToken)
        {
            var logs = await _activityService.GetMemberActivityAsync(memberId, 50, cancellationToken);
            return Ok(logs);
        }

        [HttpGet("admin/global")]
        [Authorize(Policy = Constants.Policies.SuperAdminOnly)] // Strict role parity: Sync with frontend superAdminGuard
        public async Task<IActionResult> GetGlobalActivity(CancellationToken cancellationToken)
        {
            var logs = await _activityService.GetRecentGlobalActivityAsync(50, cancellationToken);
            return Ok(logs);
        }
    }
}
