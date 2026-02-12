using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/networking")]
    public class NetworkingController : ControllerBase
    {
        private readonly INetworkingService _networkingService;

        public NetworkingController(INetworkingService networkingService)
        {
            _networkingService = networkingService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] MemberSearchFilterDto filter, CancellationToken cancellationToken)
        {
            var results = await _networkingService.SearchMembersAsync(filter, cancellationToken);
            return Ok(results);
        }

        [HttpGet("committee")]
        public async Task<IActionResult> GetExecutiveCommittee([FromQuery] int? year, CancellationToken cancellationToken)
        {
            var committee = await _networkingService.GetExecutiveCommitteeAsync(year, cancellationToken);
            return Ok(committee);
        }

        [HttpGet("updates")]
        public async Task<IActionResult> GetLatestUpdates([FromQuery] int count = 10, CancellationToken cancellationToken = default)
        {
            var updates = await _networkingService.GetLatestAlumniUpdatesAsync(count, cancellationToken);
            return Ok(updates);
        }
    }
}
