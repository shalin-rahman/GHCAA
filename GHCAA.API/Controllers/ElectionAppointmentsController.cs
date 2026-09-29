using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Extensions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static GHCAA.Domain.Constants;

namespace GHCAA.API.Controllers;

// Spec 023 (37.12d). The service checks the per-election permission until the 37.12e filter does it here.
[ApiController]
[Route("api")]
public sealed class ElectionAppointmentsController(IElectionAppointmentService service) : ControllerBase
{
    [HttpGet("elections/{id:int}/appointments")]
    [Authorize(Policy = Policies.ElectionStaff)]
    public async Task<IActionResult> List(int id, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var rows = await service.ListAsync(id, userId, ct);
        return rows is null ? Forbid() : Ok(rows);
    }

    [HttpPost("elections/{id:int}/appointments")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> Appoint(int id, [FromBody] AppointDto dto, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var (success, error, appointment) = await service.AppointAsync(id, dto, userId, ct);
        return success ? Ok(appointment) : Failure(error);
    }

    [HttpPost("elections/appointments/{id:int}/revoke")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> Revoke(int id, [FromBody] AppointmentReasonDto dto, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var (success, error) = await service.RevokeAsync(id, userId, dto.Reason, ct);
        return success ? Ok() : Failure(error);
    }

    [HttpGet("me/election-appointments")]
    [Authorize]
    public async Task<IActionResult> Mine(CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        return Ok(await service.ListMineAsync(userId, ct));
    }

    [HttpPost("me/election-appointments/{id:int}/accept")]
    [Authorize]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> Accept(int id, [FromBody] AcceptAppointmentDto dto, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (success, error) = await service.AcceptAsync(id, userId, dto, ip, ct);
        return success ? Ok() : Failure(error);
    }

    [HttpPost("me/election-appointments/{id:int}/decline")]
    [Authorize]
    public async Task<IActionResult> Decline(int id, [FromBody] AppointmentReasonDto dto, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var (success, error) = await service.DeclineAsync(id, userId, dto.Reason, ct);
        return success ? Ok() : Failure(error);
    }

    private IActionResult Failure(string? error) => error switch
    {
        "forbidden" => Forbid(),
        "not-found" or "persona-not-found" or "member-not-found" => NotFound(),
        "duplicate" => Problem(detail: "This person already holds this role in the election.", statusCode: StatusCodes.Status409Conflict),
        "email-is-member" => Problem(detail: "This email belongs to a member. Appoint them as a member instead.", statusCode: StatusCodes.Status409Conflict),
        "member-no-login" => Problem(detail: "This member has no login yet.", statusCode: StatusCodes.Status409Conflict),
        "already-accepted" => Problem(detail: "This appointment is already accepted.", statusCode: StatusCodes.Status409Conflict),
        "expired" => Problem(detail: "This appointment has expired.", statusCode: StatusCodes.Status409Conflict),
        "declaration-required" => Problem(detail: "You must agree to the declaration to accept.", statusCode: StatusCodes.Status400BadRequest),
        _ => BadRequest(),
    };
}
