using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    IElectionDocumentService documentService,
    IElectionApprovalService approvals) : ControllerBase
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

    /// <summary>FR-37.1a: advances the election through its controlled phases. Spec 023 (37.12f):
    /// publishing, opening and closing polling and archiving may wait for a second person (202).</summary>
    [HttpPost("{id:int}/phase")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireStepUp]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ChangePhase)]
    public async Task<IActionResult> SetPhase(int id, [FromBody] ElectionPhase phase, CancellationToken ct)
    {
        if (ApprovalActionFor(phase) is not { } action)
            return await service.SetPhaseAsync(id, phase, ct) ? Ok() : Problem(detail: "Invalid phase transition.", statusCode: StatusCodes.Status400BadRequest);
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var run = await approvals.RunOrRequestAsync(id, action, userId, CallerRoles(), ct: ct);
        return this.ApprovalReply(run) ?? (run.Error is null ? Ok() : Problem(detail: "Invalid phase transition.", statusCode: StatusCodes.Status400BadRequest));
    }

    /// <summary>FR-37.1a: adds a seat to an election.</summary>
    [HttpPost("{id:int}/seats")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireStepUp]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ManageSetup)]
    public async Task<IActionResult> AddSeat(int id, ElectionSeatRequestDto request, CancellationToken ct)
        => Ok(new { Id = await service.AddSeatAsync(id, request, ct) });

    /// <summary>FR-37.1a: freezes the auditable voter-roll snapshot.</summary>
    [HttpPost("{id:int}/voter-roll/freeze")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireStepUp]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ManageVoterRoll)]
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
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireStepUp]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.DecideNominations, ElectionIdLookup.Nomination)]
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

    /// <summary>FR-37.1d, spec 023 FR-001 and FR-003: opens the sealed ballots with the returning officer's key and counts them. 37.13h: may wait for a second person (202).</summary>
    [HttpPost("{id:int}/count")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireStepUp]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.Count)]
    public async Task<IActionResult> Count(int id, [FromBody] CountElectionRequest? request, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var (results, error, pending) = await approvals.CountAsync(id, userId, CallerRoles(), request?.PrivateKey, ct);
        if (results is not null) return Ok(results);
        var waiting = this.ApprovalReply(new ElectionApprovalRunResult(false, error, pending));
        if (waiting is not null) return waiting;
        if (error is "same-person" or "closed") return ApprovalError(error);
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

    /// <summary>FR-37.1d: declares counted results. Spec 023 (37.12f): may wait for a second person (202).</summary>
    [HttpPost("{id:int}/declare")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireStepUp]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.Declare)]
    public async Task<IActionResult> Declare(int id, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var run = await approvals.RunOrRequestAsync(id, ElectionApprovalAction.Declare, userId, CallerRoles(), ct: ct);
        return this.ApprovalReply(run) ?? (run.Error is null ? Ok() : Problem(detail: "Election is not ready for declaration.", statusCode: StatusCodes.Status400BadRequest));
    }

    /// <summary>Spec 023 (37.12f): the two-person steps on this election still waiting for a second person.</summary>
    [HttpGet("{id:int}/approvals")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireStepUp]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ViewDashboard)]
    public async Task<IActionResult> Approvals(int id, CancellationToken ct) => Ok(await approvals.ListOpenAsync(id, ct));

    /// <summary>Spec 023 (37.12f): a second person agrees, and the stored step runs.</summary>
    [HttpPost("approvals/{approvalId:int}/approve")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireStepUp]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.Approve, ElectionIdLookup.Approval)]
    public async Task<IActionResult> Approve(int approvalId, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var (success, error) = await approvals.ApproveAsync(approvalId, userId, CallerRoles(), ct);
        return success ? Ok() : ApprovalError(error);
    }

    /// <summary>Spec 023 (37.12f): turns a stored step down, so it never runs.</summary>
    [HttpPost("approvals/{approvalId:int}/reject")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireStepUp]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.Approve, ElectionIdLookup.Approval)]
    public async Task<IActionResult> Reject(int approvalId, [FromBody] RejectApprovalRequest? request, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var (success, error) = await approvals.RejectAsync(approvalId, userId, CallerRoles(), request?.Reason, ct);
        return success ? Ok() : ApprovalError(error);
    }

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

    private List<string> CallerRoles() => User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

    // Phases missing here are not two-person steps. Declared is reached only through Declare.
    private static ElectionApprovalAction? ApprovalActionFor(ElectionPhase phase) => phase switch
    {
        ElectionPhase.Nomination => ElectionApprovalAction.Publish,
        ElectionPhase.Polling => ElectionApprovalAction.OpenPolling,
        ElectionPhase.Counting => ElectionApprovalAction.ClosePolling,
        ElectionPhase.Archived => ElectionApprovalAction.Archive,
        _ => null,
    };

    private IActionResult ApprovalError(string? error) => error switch
    {
        "not-found" => NotFound(),
        "forbidden" => this.ProblemWithCode(ErrorCodes.ElectionPermission, "You need the Approve permission on this election.", StatusCodes.Status403Forbidden),
        "same-person" => Problem(detail: "The person who asked for this step cannot also approve it, and the person who approved a count cannot run it.", statusCode: StatusCodes.Status403Forbidden),
        "expired" => Problem(detail: "This request has expired. Ask again.", statusCode: StatusCodes.Status400BadRequest),
        "closed" => Problem(detail: "This request has already been decided.", statusCode: StatusCodes.Status409Conflict),
        "phase-closed" => Problem(detail: "The key cannot change once polling has opened.", statusCode: StatusCodes.Status400BadRequest),
        "invalid-key" => Problem(detail: "The stored key is not valid.", statusCode: StatusCodes.Status400BadRequest),
        _ => Problem(detail: "The election is not ready for this step.", statusCode: StatusCodes.Status400BadRequest),
    };
}
