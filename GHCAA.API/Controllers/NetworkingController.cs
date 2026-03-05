using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
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
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] MemberSearchFilterDto filter, CancellationToken cancellationToken)
        {
            var results = await _networkingService.SearchMembersAsync(filter, cancellationToken);
            return Ok(results);
        }

        [HttpGet("member/{id}")]
        [AllowAnonymous]
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
