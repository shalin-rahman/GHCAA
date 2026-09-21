using System;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Extensions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GHCAA.Domain.Enums;

namespace GHCAA.API.Controllers;

[ApiController]
[Route("api/elections")]
[Authorize]
public sealed class ElectionsController(
    IElectionService service,
    IElectionDocumentService documentService) : ControllerBase
{
    /// <summary>FR-37.1a: creates an election and its persisted timetable.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> Create(CreateElectionDto request, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentMemberIdRaw(), out var memberId))
            return Unauthorized();
        return Ok(await service.CreateAsync(request with { CreatedBy = memberId }, ct));
    }

    /// <summary>FR-37.1a: returns the public election summary.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
        => (await service.GetAsync(id, ct)) is { } result ? Ok(result) : NotFound();

    /// <summary>FR-37.1a: advances the election through its controlled phases.</summary>
    [HttpPost("{id:int}/phase")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> SetPhase(int id, [FromBody] ElectionPhase phase, CancellationToken ct)
        => await service.SetPhaseAsync(id, phase, ct) ? Ok() : BadRequest("Invalid phase transition.");

    /// <summary>FR-37.1a: adds a seat to an election.</summary>
    [HttpPost("{id:int}/seats")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> AddSeat(int id, ElectionSeatRequestDto request, CancellationToken ct)
        => Ok(new { Id = await service.AddSeatAsync(id, request, ct) });

    /// <summary>FR-37.1a: assigns an election officer.</summary>
    [HttpPost("{id:int}/officers")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> AssignOfficer(int id, ElectionOfficerDto request, CancellationToken ct)
        => await service.AssignOfficerAsync(id, request, ct) ? Ok() : Conflict("Officer assignment already exists or member is inactive.");

    /// <summary>FR-37.1a: freezes the auditable voter-roll snapshot.</summary>
    [HttpPost("{id:int}/voter-roll/freeze")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> FreezeRoll(int id, CancellationToken ct)
        => Ok(new { Count = await service.FreezeVoterRollAsync(id, ct) });

    /// <summary>FR-37.1b: returns nominations for an election.</summary>
    [HttpGet("{id:int}/nominations")]
    [AllowAnonymous]
    public async Task<IActionResult> Nominations(int id, CancellationToken ct)
        => Ok(await service.GetNominationsAsync(id, ct));

    /// <summary>FR-37.1b: submits a nomination during the nomination phase.</summary>
    [HttpPost("{id:int}/nominations")]
    public async Task<IActionResult> Nominate(int id, NominationDto request, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentMemberIdRaw(), out var memberId))
            return Unauthorized();
        return Ok(await service.SubmitNominationAsync(id, request with { ProposerMemberId = memberId }, ct));
    }

    /// <summary>FR-37.1b: records the authorised scrutiny decision.</summary>
    [HttpPost("nominations/{nominationId:int}/scrutiny")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> Scrutinise(int nominationId, ScrutinyDto request, CancellationToken ct)
        => await service.DecideNominationAsync(nominationId, request, ct) ? Ok() : NotFound();

    /// <summary>FR-37.1b: withdraws an accepted nomination.</summary>
    [HttpPost("nominations/{nominationId:int}/withdraw")]
    public async Task<IActionResult> Withdraw(int nominationId, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentMemberIdRaw(), out var memberId))
            return Unauthorized();
        return await service.WithdrawNominationAsync(nominationId, memberId, ct) ? Ok() : BadRequest();
    }

    /// <summary>FR-37.1c: records a secret ballot without linking the vote to the voter.</summary>
    [HttpPost("{id:int}/vote")]
    public async Task<IActionResult> Vote(int id, CastVoteDto request, CancellationToken ct)
    {
        var claim = this.CurrentMemberIdRaw();
        return int.TryParse(claim, out var memberId) && await service.CastVoteAsync(id, memberId, request, ct) ? Ok() : BadRequest("Vote could not be recorded.");
    }

    /// <summary>FR-37.1d: counts accepted nominations after polling closes.</summary>
    [HttpPost("{id:int}/count")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> Count(int id, CancellationToken ct) => Ok(await service.CountAsync(id, ct));

    /// <summary>FR-37.1d: declares counted results.</summary>
    [HttpPost("{id:int}/declare")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> Declare(int id, CancellationToken ct) => await service.DeclareAsync(id, ct) ? Ok() : BadRequest("Election is not ready for declaration.");

    /// <summary>FR-37.1e: generates a completed official election form from persisted records.</summary>
    [HttpGet("{id:int}/documents/{formCode}")]
    [AllowAnonymous]
    public async Task<IActionResult> Document(int id, string formCode, CancellationToken ct)
    {
        var pdf = await documentService.GeneratePdfAsync(id, formCode, ct);
        return pdf is null
            ? NotFound()
            : File(pdf, "application/pdf", $"E-{id:D6}_{formCode.ToUpperInvariant()}.pdf");
    }
}
