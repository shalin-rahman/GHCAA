using GHCAA.API.Extensions;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers;

[ApiController]
[Route("api/communications")]
[Authorize]
public sealed class MemberCommunicationsController(ICommunicationService communicationService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var memberIdRaw = this.CurrentMemberIdRaw();
        if (!int.TryParse(memberIdRaw, out var memberId))
            return Problem(detail: "Member profile is required.", statusCode: StatusCodes.Status401Unauthorized);

        return Ok(await communicationService.GetMemberLogsAsync(memberId, page, pageSize, cancellationToken));
    }
}
