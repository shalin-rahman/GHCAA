using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    // 82.115: backs the "Developer Options" admin screen — a tracker view of docs/TODO.md so a
    // SuperAdmin doesn't have to open the raw file.
    [ApiController]
    [Route("api/admin/dev-tracker")]
    [Authorize(Policy = Constants.Policies.SuperAdminOnly)]
    public class AdminDevTrackerController : ControllerBase
    {
        private readonly IDevTrackerService _devTrackerService;

        public AdminDevTrackerController(IDevTrackerService devTrackerService)
        {
            _devTrackerService = devTrackerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOpenItems([FromQuery] string? priority, CancellationToken cancellationToken)
        {
            var filter = new DevTrackerFilterDto { Priority = priority };
            var items = await _devTrackerService.GetOpenItemsAsync(filter, cancellationToken);
            return Ok(items);
        }
    }
}
