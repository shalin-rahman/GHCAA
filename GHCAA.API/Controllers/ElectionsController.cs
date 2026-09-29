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
using static GHCAA.Domain.Constants;

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
    [Authorize(Policy = Policies.AdminOnly)]
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

    /// <summary>FR-37.1h: returns the single election currently in an active phase, if any.</summary>
    [HttpGet("current")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCurrent(CancellationToken ct)
        => (await service.GetCurrentAsync(ct)) is { } result ? Ok(result) : NoContent();

    /// <summary>FR-37.1a: advances the election through its controlled phases.</summary>
    [HttpPost("{id:int}/phase")]
    [Authorize(Policy = Policies.AdminOnly)]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> SetPhase(int id, [FromBody] ElectionPhase phase, CancellationToken ct)
        => await service.SetPhaseAsync(id, phase, ct) ? Ok() : Problem(detail: "Invalid phase transition.", statusCode: StatusCodes.Status400BadRequest);

    /// <summary>FR-37.1a: adds a seat to an election.</summary>
    [HttpPost("{id:int}/seats")]
    [Authorize(Policy = Policies.AdminOnly)]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> AddSeat(int id, ElectionSeatRequestDto request, CancellationToken ct)
        => Ok(new { Id = await service.AddSeatAsync(id, request, ct) });

    /// <summary>FR-37.1a: freezes the auditable voter-roll snapshot.</summary>
    [HttpPost("{id:int}/voter-roll/freeze")]
    [Authorize(Policy = Policies.AdminOnly)]
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
    [Authorize(Policy = Policies.AdminOnly)]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> Scrutinise(int nominationId, ScrutinyDto request, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        return await service.DecideNominationAsync(nominationId, userId, request, ct) ? Ok() : NotFound();
    }

    /// <summary>FR-37.1b: withdraws an accepted nomination.</summary>
    [HttpPost("nominations/{nominationId:int}/withdraw")]
    public async Task<IActionResult> Withdraw(int nominationId, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentMemberIdRaw(), out var memberId))
            return Unauthorized();
        return await service.WithdrawNominationAsync(nominationId, memberId, ct) ? Ok() : BadRequest();
    }

    /// <summary>FR-37.1c, spec 023 FR-005: records the whole ballot in one request and returns its
    /// tracking code. The ballot row has no member or time column. The link that remains is the
    /// batch it was moved in, described in spec 023.</summary>
    [HttpPost("{id:int}/vote")]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> Vote(int id, CastBallotDto request, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentMemberIdRaw(), out var memberId))
            return Unauthorized();
        var (success, error, trackingCode) = await service.CastBallotAsync(id, memberId, request, ct);
        if (success)
            return Ok(new CastBallotResultDto(trackingCode!));
        return error == "already-voted"
            ? this.ProblemWithCode(ErrorCodes.AlreadyVoted, "You have already voted in this election.", StatusCodes.Status409Conflict)
            : Problem(detail: "Vote could not be recorded.", statusCode: StatusCodes.Status400BadRequest);
    }

    /// <summary>FR-37.1d, spec 023 FR-001 and FR-003: opens the sealed ballots with the returning officer's key and counts them.</summary>
    [HttpPost("{id:int}/count")]
    [Authorize(Policy = Policies.AdminOnly)]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> Count(int id, [FromBody] CountElectionRequest? request, CancellationToken ct)
    {
        var (results, error) = await service.CountAsync(id, request?.PrivateKey, ct);
        if (results is not null) return Ok(results);
        var detail = error switch
        {
            "no-key" => "Upload the returning officer's private key file to count.",
            "wrong-key" => "This is not the key the election was sealed under.",
            "below-threshold" => $"Fewer than {Elections.MinimumBallotsToCount} ballots were cast, so the count would not keep the vote secret.",
            "integrity" => "The number of ballots, receipts and voters marked as voted do not match. Nothing was counted.",
            "unreadable" => "A sealed ballot could not be opened. Nothing was counted.",
            "not-recorded" => "The count could not be saved. Try again.",
            _ => "The election is not in counting.",
        };
        return Problem(detail: detail, statusCode: StatusCodes.Status400BadRequest);
    }

    /// <summary>FR-37.1d: declares counted results.</summary>
    [HttpPost("{id:int}/declare")]
    [Authorize(Policy = Policies.AdminOnly)]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> Declare(int id, CancellationToken ct) => await service.DeclareAsync(id, ct) ? Ok() : Problem(detail: "Election is not ready for declaration.", statusCode: StatusCodes.Status400BadRequest);

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
