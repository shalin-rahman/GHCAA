using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Extensions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers;

// Spec 023 FR-039 (37.13v). A SuperAdmin can open a short unlock of the frozen election rules
// when something has gone wrong mid-election. Opening and closing are logged, and so is every
// change made while it is open. A plain appointment revoke stays locked either way.
[ApiController]
[Route("api/admin/elections/rules-unlock")]
[Authorize(Policy = Constants.Policies.SuperAdminOnly)]
[GHCAA.API.Filters.RequireStepUp]
public sealed class ElectionRulesUnlockController(IElectionFreezeService freeze) : ControllerBase
{
    /// <summary>The open unlock, or 204 when there is none.</summary>
    [HttpGet]
    [ProducesResponseType<ElectionRulesUnlockDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        await freeze.GetOpenUnlockAsync(ct) is { } unlock ? Ok(unlock) : NoContent();

    [HttpPost]
    [ProducesResponseType<ElectionRulesUnlockDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Open([FromBody] OpenElectionRulesUnlockRequest request, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var (error, unlock) = await freeze.OpenUnlockAsync(userId, request?.Reason, request?.Minutes, ct);
        return error switch
        {
            null => Ok(unlock),
            Constants.Elections.RulesUnlockErrors.ReasonTooShort => Problem(detail: $"Give a reason of at least {Constants.Elections.RulesUnlockReasonMinLength} characters.", statusCode: StatusCodes.Status400BadRequest),
            Constants.Elections.RulesUnlockErrors.ReasonTooLong => Problem(detail: $"The reason can be at most {Constants.Elections.RulesUnlockReasonMaxLength} characters.", statusCode: StatusCodes.Status400BadRequest),
            Constants.Elections.RulesUnlockErrors.InvalidMinutes => Problem(detail: $"An unlock lasts 1 to {Constants.Elections.RulesUnlockMaxMinutes} minutes.", statusCode: StatusCodes.Status400BadRequest),
            Constants.Elections.RulesUnlockErrors.NotFrozen => Problem(detail: "No election is frozen, so there is nothing to unlock.", statusCode: StatusCodes.Status409Conflict),
            Constants.Elections.RulesUnlockErrors.AlreadyOpen => Problem(detail: "An unlock is already open.", statusCode: StatusCodes.Status409Conflict),
            _ => throw new InvalidOperationException($"Unknown unlock error '{error}'."),
        };
    }

    [HttpPost("close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Close(CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        return await freeze.CloseUnlockAsync(userId, ct) is null
            ? NoContent()
            : Problem(detail: "No unlock is open.", statusCode: StatusCodes.Status409Conflict);
    }
}
