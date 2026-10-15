using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/networking")]
    [Authorize]
    public class NetworkingController : ControllerBase
    {
        private readonly INetworkingService _networkingService;

        public NetworkingController(INetworkingService networkingService)
        {
            _networkingService = networkingService;
        }


        [HttpGet("search")]
        [HttpGet("directory")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] MemberSearchFilterDto filter, CancellationToken cancellationToken)
        {
            var result = User.Identity?.IsAuthenticated == true
                ? await _networkingService.SearchMembersAsync(filter, cancellationToken)
                : await _networkingService.SearchPublicDirectoryAsync(filter, cancellationToken);
            return Ok(result);
        }

        // Members only. It was anonymous, which let anyone read any member's full profile.
        [HttpGet("member/{id}")]
        public async Task<IActionResult> GetPublicProfile(int id, CancellationToken cancellationToken)
        {
            var profile = await _networkingService.GetMemberProfileAsync(id, cancellationToken);
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        [HttpGet("committee")]
        [AllowAnonymous]
        public async Task<IActionResult> GetExecutiveCommittee([FromQuery] int? periodId, CancellationToken cancellationToken)
        {
            var committee = await _networkingService.GetExecutiveCommitteeAsync(periodId, cancellationToken);
            return Ok(committee);
        }

        [HttpGet("periods")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPeriods(CancellationToken cancellationToken)
        {
            var periods = await _networkingService.GetECPeriodsAsync(cancellationToken);
            return Ok(periods);
        }

        [HttpGet("updates")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLatestUpdates([FromQuery] int count = 10, CancellationToken cancellationToken = default)
        {
            var updates = await _networkingService.GetLatestAlumniUpdatesAsync(count, cancellationToken);
            return Ok(updates);
        }
    }
}
