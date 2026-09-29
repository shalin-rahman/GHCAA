using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GHCAA.Domain.Constants;

namespace GHCAA.API.Controllers;

// Spec 023 (37.12b). Admin CRUD for election personas.
[ApiController]
[Route("api/admin/election-personas")]
[Authorize(Policy = Policies.SuperAdminOnly)]
public sealed class ElectionPersonasController(IElectionPersonaService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(await service.ListAsync(true, ct));

    [HttpPost]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> Create([FromBody] SaveElectionPersonaDto dto, CancellationToken ct)
    {
        var (success, error, persona) = await service.CreateAsync(dto, ct);
        return success ? Ok(persona) : DuplicateName();
    }

    [HttpPut("{id:int}")]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> Update(int id, [FromBody] SaveElectionPersonaDto dto, CancellationToken ct)
    {
        var (success, error, persona) = await service.UpdateAsync(id, dto, ct);
        if (success) return Ok(persona);
        return error == "duplicate-name" ? DuplicateName() : NotFound();
    }

    [HttpPost("{id:int}/active")]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> SetActive(int id, [FromBody] bool isActive, CancellationToken ct)
        => await service.SetActiveAsync(id, isActive, ct) ? Ok() : NotFound();

    [HttpDelete("{id:int}")]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var (success, error) = await service.DeleteAsync(id, ct);
        if (success) return Ok();
        return error switch
        {
            "in-use" => Problem(detail: "This persona is assigned to an appointment. Deactivate it instead.", statusCode: StatusCodes.Status409Conflict),
            _ => NotFound(),
        };
    }

    private ObjectResult DuplicateName() =>
        Problem(detail: "Another persona already uses this name.", statusCode: StatusCodes.Status409Conflict);
}

// Spec 023 (37.12b). Read-only list for election staff. The appoint dialog needs the active
// personas. 37.12e will add the per-election permission check; for now any signed-in user with
// an account can read this.
[ApiController]
[Route("api/election-personas")]
[Authorize]
public sealed class ElectionPersonasReadController(IElectionPersonaService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(await service.ListAsync(false, ct));
}
