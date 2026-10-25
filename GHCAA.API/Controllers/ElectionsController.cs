using GHCAA.API.Extensions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using static GHCAA.Domain.Constants;
using static GHCAA.Domain.Enums;

namespace GHCAA.API.Controllers;

[ApiController]
[Route("api/elections")]
[Authorize]
public sealed class ElectionsController(
    IElectionService service,
    IElectionDocumentService documentService,
    IElectionApprovalService approvals,
    IElectionAuditWriter audit) : ControllerBase
{
    /// <summary>FR-37.1a: creates an election and its persisted timetable.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.AdminOnly)]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> Create(CreateElectionDto request, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentMemberIdRaw(), out var memberId))
            return Unauthorized();
        var (created, error) = await service.CreateAsync(request with { CreatedBy = memberId }, ct);
        return created is null ? Problem(detail: CreateErrorDetail(error), statusCode: StatusCodes.Status400BadRequest) : Ok(created);
    }

    // Shared with AdminElectionsController.Create.
    internal static string CreateErrorDetail(string? error) => error switch
    {
        "dates-missing" => "Set the dates nominations open and close and polling opens and closes.",
        "dates-out-of-order" => "The dates must follow the phase order: nominations, scrutiny, withdrawals, then polling.",
        "no-period" => "Choose an EC period for this election.",
        "period-starts-early" => "The EC period must start on or after the day polling closes.",
        "period-has-members" => "That EC period already has committee members. Choose a new period.",
        "period-in-use" => "Another election already fills that EC period. Choose a new period.",
        _ => "The election could not be created.",
    };

    /// <summary>FR-37.1a: returns the public election summary.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        // FR-047. Reading it by id would show a test election exists.
        if (await service.IsTestAsync(id, ct)) return NotFound();
        return (await service.GetAsync(id, ct)) is { } result ? Ok(result) : NotFound();
    }

    /// <summary>Spec 023 FR-033 (37.12i): who runs the election, the timeline, turnout when it may be shown, and the key and audit fingerprints. No contact details.</summary>
    [HttpGet("{id:int}/board")]
    [AllowAnonymous]
    [ProducesResponseType<ElectionBoardDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Board(int id, CancellationToken ct)
        => (await service.GetBoardAsync(id, ct)) is { } result ? Ok(result) : NotFound();

    /// <summary>Spec 023 FR-011 to FR-014 (37.1l): the caller's ballot paper, with names and whether they have voted.</summary>
    [HttpGet("{id:int}/ballot")]
    [ProducesResponseType<BallotViewDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Ballot(int id, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentMemberIdRaw(), out var memberId))
            return Unauthorized();
        return (await service.GetBallotAsync(id, memberId, ct)) is { } result ? Ok(result) : NotFound();
    }

    /// <summary>Spec 023 FR-017 (37.1n): the declared result. 404 until the election is Declared.</summary>
    [HttpGet("{id:int}/results")]
    [AllowAnonymous]
    [ProducesResponseType<PublicElectionResultsDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Results(int id, CancellationToken ct)
    {
        // FR-047. A test election is never published.
        if (await service.IsTestAsync(id, ct)) return NotFound();
        return (await service.GetPublicResultsAsync(id, ct)) is { } result ? Ok(result) : NotFound();
    }

    /// <summary>Spec 023 FR-049 (37.13c): the opened ballots per seat. 404 until Declared or when the setting is off.</summary>
    [HttpGet("{id:int}/opened-ballots")]
    [AllowAnonymous]
    [ProducesResponseType<OpenedBallotsDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> OpenedBallots(int id, CancellationToken ct)
    {
        if (await service.IsTestAsync(id, ct)) return NotFound();
        return (await service.GetOpenedBallotsAsync(id, ct)) is { } result ? Ok(result) : NotFound();
    }

    /// <summary>Spec 023 FR-048 (37.13a): whether the election's audit chain is whole. Names the first broken entry.</summary>
    [HttpGet("{id:int}/audit/verify")]
    [AllowAnonymous]
    [ProducesResponseType<ElectionAuditVerifyDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyAudit(int id, CancellationToken ct)
    {
        // FR-047. Officials read a test election's chain through the audit list.
        if (await service.IsTestAsync(id, ct)) return NotFound();
        return Ok(await audit.VerifyAsync(id, ct));
    }

    /// <summary>Spec 023 FR-048 (37.13a): the audit entries, for officials who hold ViewAudit.</summary>
    [HttpGet("{id:int}/audit")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ViewAudit)]
    [ProducesResponseType<IReadOnlyList<ElectionAuditEntryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Audit(int id, CancellationToken ct) => Ok(await audit.ListAsync(id, ct));

    /// <summary>Spec 023 FR-016 (37.1m): every tracking code, sorted, with no choices. 404 until the count has run.</summary>
    [HttpGet("{id:int}/tracking-codes")]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> TrackingCodes(int id, CancellationToken ct)
    {
        // FR-047. The codes would show a test election exists and how many voted in it.
        if (await service.IsTestAsync(id, ct)) return NotFound();
        return (await service.GetTrackingCodesAsync(id, ct)) is { } result ? Ok(result) : NotFound();
    }

    /// <summary>FR-37.1h: returns the single election currently in an active phase, if any.</summary>
    [HttpGet("current")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCurrent(CancellationToken ct)
        => (await service.GetCurrentAsync(ct)) is { } result ? Ok(result) : NoContent();

    /// <summary>The newest Declared or Archived election that is not a test, in the same shape as current. 404 when there is none.</summary>
    [HttpGet("latest-declared")]
    [AllowAnonymous]
    [ProducesResponseType<ElectionSummaryDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLatestDeclared(CancellationToken ct)
        => (await service.GetLatestDeclaredAsync(ct)) is { } result ? Ok(result) : NotFound();

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
        return this.ApprovalReply(run) ?? run.Error switch
        {
            null => Ok(),
            Elections.Replies.PollingNotReady => await PollingNotReadyAsync(id, ct),
            _ => Problem(detail: "Invalid phase transition.", statusCode: StatusCodes.Status400BadRequest),
        };
    }

    /// <summary>Spec 023 (37.12w): what must be true before polling opens, and which items are still unmet.</summary>
    [HttpGet("{id:int}/readiness")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ViewDashboard)]
    [ProducesResponseType<ElectionReadinessDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Readiness(int id, CancellationToken ct)
        => (await service.GetReadinessAsync(id, ct)) is { } result ? Ok(result) : NotFound();

    /// <summary>Spec 023 (37.12w): an official records that one of the checks made outside the system is done.</summary>
    [HttpPost("{id:int}/readiness/attest")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireStepUp]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ChangePhase)]
    public async Task<IActionResult> AttestReadiness(int id, [FromBody] AttestReadinessRequest request, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        return await service.AttestReadinessAsync(id, userId, request.Condition, ct) switch
        {
            null => NoContent(),
            Elections.Replies.NotFound => NotFound(),
            Elections.Replies.AlreadyRecorded => Problem(detail: "This item is already recorded.", statusCode: StatusCodes.Status409Conflict),
            Elections.Replies.WrongPhase => Problem(detail: "Readiness can only be recorded before polling opens.", statusCode: StatusCodes.Status400BadRequest),
            _ => Problem(detail: "This item is not one an official records by hand.", statusCode: StatusCodes.Status400BadRequest),
        };
    }

    /// <summary>FR-37.1a: adds a seat to an election.</summary>
    [HttpPost("{id:int}/seats")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireStepUp]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ManageSetup)]
    public async Task<IActionResult> AddSeat(int id, ElectionSeatRequestDto request, CancellationToken ct)
    {
        try
        {
            return Ok(new { Id = await service.AddSeatAsync(id, request, ct) });
        }
        catch (InvalidOperationException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
        }
    }

    /// <summary>FR-37.1a: freezes the auditable voter-roll snapshot.</summary>
    [HttpPost("{id:int}/voter-roll/freeze")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireStepUp]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ManageVoterRoll)]
    public async Task<IActionResult> FreezeRoll(int id, CancellationToken ct)
        => Ok(new { Count = await service.FreezeVoterRollAsync(id, ct) });

    /// <summary>37.12q: throws the frozen voter roll away so it can be frozen again. Needs a reason.</summary>
    [HttpPost("{id:int}/voter-roll/unfreeze")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireStepUp]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ManageVoterRoll)]
    public async Task<IActionResult> UnfreezeRoll(int id, [FromBody] UnfreezeVoterRollDto request, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var (ok, error, removed) = await service.UnfreezeVoterRollAsync(id, userId, request.Reason, ct);
        if (ok) return Ok(new { Removed = removed });
        return error == "not-found"
            ? NotFound()
            : Problem(detail: error switch
            {
                "reason-required" => "A reason is required.",
                "phase-closed" => "The voter roll cannot be unfrozen after the Nomination phase ends.",
                "window-closed" => "The unfreeze window has closed.",
                _ => "The voter roll cannot be unfrozen once a nomination has been filed."
            }, statusCode: StatusCodes.Status409Conflict);
    }

    /// <summary>84.52: the seats a member can be nominated for, which the ballot leaves out before CandidateList.</summary>
    [HttpGet("{id:int}/seats")]
    [ProducesResponseType<IReadOnlyList<ElectionSeatViewDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Seats(int id, CancellationToken ct)
    {
        if (await service.IsTestAsync(id, ct)) return NotFound();
        return (await service.GetSeatsAsync(id, ct)) is { } result ? Ok(result) : NotFound();
    }

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
        try
        {
            return Ok(await service.SubmitNominationAsync(id, request with { ProposerMemberId = memberId }, ct));
        }
        catch (InvalidOperationException ex)
        {
            // The service names the broken nomination rule, so the member sees why.
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
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
        try
        {
            return await service.DecideNominationAsync(nominationId, userId, request, ct) ? Ok() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>FR-37.1b: withdraws an accepted nomination.</summary>
    [HttpPost("nominations/{nominationId:int}/withdraw")]
    public async Task<IActionResult> Withdraw(int nominationId, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentMemberIdRaw(), out var memberId))
            return Unauthorized();
        return await service.WithdrawNominationAsync(nominationId, memberId, ct)
            ? Ok()
            : Problem(detail: "Only the candidate can withdraw, only an accepted nomination, and only in the Withdrawal phase.", statusCode: 400);
    }

    /// <summary>FR-37.1c, spec 023 FR-005: records the whole ballot in one request and returns its
    /// tracking code. The ballot row has no member or time column. The link that remains is the
    /// batch it was moved in, described in spec 023. 37.14o: the reply also carries the receipt PDF.</summary>
    [HttpPost("{id:int}/vote")]
    [GHCAA.API.Filters.RequireStepUp]
    public async Task<IActionResult> Vote(int id, CastBallotDto request, [FromServices] ILogger<ElectionsController> logger, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentMemberIdRaw(), out var memberId))
            return Unauthorized();
        var (success, error, trackingCode) = await service.CastBallotAsync(id, memberId, request, ct);
        if (success)
            return Ok(new CastBallotResultDto(trackingCode!, await ReceiptPdfAsync(id, trackingCode!, logger)));
        return error == "already-voted"
            ? this.ProblemWithCode(ErrorCodes.AlreadyVoted, "You have already voted in this election.", StatusCodes.Status409Conflict)
            : Problem(detail: "Vote could not be recorded.", statusCode: StatusCodes.Status400BadRequest);
    }

    // The vote is already saved, so a failed PDF must not turn the reply into an error and lose
    // the code. The log names the election only, never the member or the code.
    private async Task<string?> ReceiptPdfAsync(int electionId, string trackingCode, ILogger logger)
    {
        try
        {
            var pdf = await documentService.GenerateBallotReceiptPdfAsync(electionId, trackingCode, DateTime.UtcNow, CancellationToken.None);
            return pdf is null ? null : Convert.ToBase64String(pdf);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ballot receipt PDF failed for election {ElectionId}", electionId);
            return null;
        }
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
        if (error is Elections.Replies.SamePerson or "not-requester" or "closed") return ApprovalError(error);
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
    [ProducesResponseType<IReadOnlyList<ElectionApprovalDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Approvals(int id, CancellationToken ct)
    {
        var open = await approvals.ListOpenAsync(id, ct);
        // The clients hide Approve on the caller's own request. The service still refuses it.
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Ok(open);
        return Ok(open.Select(a => a with { RequestedByMe = a.RequestedByUserId == userId }).ToList());
    }

    /// <summary>FR-050: an official with the key records that the key file was destroyed after the declare.</summary>
    [HttpPost("{id:int}/key-destroyed")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireStepUp]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.SetBallotKey)]
    public async Task<IActionResult> KeyDestroyed(int id, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        return await service.RecordKeyDestroyedAsync(id, userId, ct) switch
        {
            null => NoContent(),
            Elections.Replies.NotFound => NotFound(),
            Elections.Replies.AlreadyRecorded => Problem(detail: "The key is already recorded as destroyed.", statusCode: StatusCodes.Status409Conflict),
            Elections.Replies.RecountWindowOpen => Problem(detail: "Keep the key until the recount window has ended. Set the window's end date first if it is missing.", statusCode: StatusCodes.Status400BadRequest),
            _ => Problem(detail: "The key can be recorded as destroyed once the result is declared.", statusCode: StatusCodes.Status400BadRequest),
        };
    }

    /// <summary>Spec 023 (37.12f): a second person agrees, and the stored step runs.</summary>
    [HttpPost("approvals/{approvalId:int}/approve")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireStepUp]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.Approve, ElectionIdLookup.Approval)]
    public async Task<IActionResult> Approve(int approvalId, [FromServices] IElectionAccessService access, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var (success, error) = await approvals.ApproveAsync(approvalId, userId, CallerRoles(), ct);
        if (success) return Ok();
        if (error == Elections.Replies.PollingNotReady
            && await access.ElectionIdForAsync(ElectionIdLookup.Approval, approvalId, ct) is { } electionId)
            return await PollingNotReadyAsync(electionId, ct);
        return ApprovalError(error);
    }

    /// <summary>Spec 023 FR-040 (37.13r): asks to remove one official, the only change allowed while rules are frozen.</summary>
    [HttpPost("appointments/{appointmentId:int}/emergency-revoke")]
    [Authorize(Policy = Policies.SuperAdminOnly)]
    [GHCAA.API.Filters.RequireStepUp]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.AppointOfficials, ElectionIdLookup.Appointment)]
    public async Task<IActionResult> EmergencyRevoke(int appointmentId, [FromBody] AppointmentReasonDto dto, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var (success, error, approval) = await approvals.RequestEmergencyRevokeAsync(appointmentId, userId, CallerRoles(), dto.Reason, ct);
        return success ? Accepted(approval) : ApprovalError(error);
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

    /// <summary>FR-37.1e, 37.14s: prints an election's form, filled from stored records or blank. No mode uses the org's DefaultPrintMode.</summary>
    [HttpGet("{id:int}/documents/{formCode}")]
    [Authorize(Policy = Policies.ElectionStaff)]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ViewDashboard)]
    public async Task<IActionResult> Document(int id, string formCode, [FromQuery] ElectionPrintMode? mode, CancellationToken ct)
    {
        // Query binding turns "mode=7" into an unnamed enum value, which would print as filled.
        if (mode is { } m && !Enum.IsDefined(m))
            return Problem(detail: "Mode must be Blank or Filled.", statusCode: StatusCodes.Status400BadRequest);
        // Officials with ViewDashboard may print a rehearsal's forms, so there is no test check here.
        var pdf = await documentService.GeneratePdfAsync(id, formCode, mode, ct);
        return pdf is null
            ? NotFound()
            : File(pdf, "application/pdf", $"E-{id:D6}_{formCode.ToUpperInvariant()}.pdf");
    }

    /// <summary>37.14s: a published form with ruled lines and no election data. Admins only.</summary>
    [Authorize(Policy = Policies.AdminOnly)]
    [HttpGet("forms/{code}/blank")]
    public async Task<IActionResult> BlankForm(string code, CancellationToken ct)
    {
        var pdf = await documentService.GenerateBlankPdfAsync(code, ct);
        return pdf is null
            ? NotFound()
            : File(pdf, "application/pdf", $"{code.Trim().ToUpperInvariant()}_blank.pdf");
    }

    /// <summary>37.14n: confirms that the election and form named by a printed form's QR code exist. Shares the credential-check rate limit.</summary>
    [HttpGet("{id:int}/documents/{formCode}/verification")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.CredentialVerification)]
    [ProducesResponseType<ElectionDocumentVerifyDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyDocument(int id, string formCode, CancellationToken ct)
    {
        // VerifyAsync answers null for a test election too (FR-047).
        return (await documentService.VerifyAsync(id, formCode, ct)) is { } result ? Ok(result) : NotFound();
    }

    // 37.12w. The detail names the unmet items so the toast and snackbar can show it as it is,
    // and "unmet" carries their keys for a client that wants to mark them on its own list.
    private async Task<IActionResult> PollingNotReadyAsync(int electionId, CancellationToken ct)
    {
        var unmet = (await service.GetReadinessAsync(electionId, ct))?.Items
            .Where(i => !i.Met && !i.Advisory).Select(i => i.Key).ToList() ?? [];
        var detail = unmet.Count == 0
            ? "Polling cannot open until every readiness item is met."
            : "Polling cannot open until these are met: "
              + string.Join("; ", unmet.Select(k => Elections.Readiness.Labels.GetValueOrDefault(k, k))) + ".";
        var reply = this.ProblemWithCode(ErrorCodes.PollingNotReady, detail, StatusCodes.Status400BadRequest);
        ((ProblemDetails)reply.Value!).Extensions["unmet"] = unmet;
        return reply;
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
        Elections.Replies.NotFound => NotFound(),
        "forbidden" => this.ProblemWithCode(ErrorCodes.ElectionPermission, "You need the Approve permission on this election.", StatusCodes.Status403Forbidden),
        Elections.Replies.SamePerson => Problem(detail: "The person who asked for this step cannot also approve it, and the person who approved a count cannot run it.", statusCode: StatusCodes.Status403Forbidden),
        "not-requester" => Problem(detail: "Only the official who asked for this count can run it.", statusCode: StatusCodes.Status403Forbidden),
        "expired" => Problem(detail: "This request has expired. Ask again.", statusCode: StatusCodes.Status400BadRequest),
        "closed" => Problem(detail: "This request has already been decided.", statusCode: StatusCodes.Status409Conflict),
        Elections.Replies.Busy => this.ElectionBusy(),
        "already-pending" => this.ProblemWithCode(ErrorCodes.ApprovalPending, "This step is already waiting for a second person.", StatusCodes.Status409Conflict),
        "reason-required" => Problem(detail: "Give a reason for the revocation.", statusCode: StatusCodes.Status400BadRequest),
        "no-eligible-approver" => Problem(detail: "Another authorized election official with Approve permission is required.", statusCode: StatusCodes.Status409Conflict),
        "not-eligible-approver" => this.ProblemWithCode(ErrorCodes.ElectionPermission, "Only another active official on this election with Approve can approve this, not the person asking or the one being removed.", StatusCodes.Status403Forbidden),
        "target-not-live" => Problem(detail: "This official is no longer active, so there is nothing to revoke.", statusCode: StatusCodes.Status409Conflict),
        "phase-closed" => Problem(detail: "The key cannot change once polling has opened.", statusCode: StatusCodes.Status400BadRequest),
        "invalid-key" => Problem(detail: "The stored key is not valid.", statusCode: StatusCodes.Status400BadRequest),
        "period-has-members" => Problem(detail: "The EC period this election fills already has committee members. Remove them before declaring.", statusCode: StatusCodes.Status409Conflict),
        Elections.Replies.PollingNotReady => this.ProblemWithCode(ErrorCodes.PollingNotReady, "Polling cannot open until every readiness item is met.", StatusCodes.Status400BadRequest),
        _ => Problem(detail: "The election is not ready for this step.", statusCode: StatusCodes.Status400BadRequest),
    };
}
