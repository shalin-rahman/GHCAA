using GHCAA.API.Extensions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static GHCAA.Domain.Constants;
using static GHCAA.Domain.Enums;

namespace GHCAA.API.Controllers;

[ApiController]
[Route("api/admin/elections")]
[Authorize(Policy = Policies.ElectionStaff)]
[GHCAA.API.Filters.RequireStepUp]
public sealed class AdminElectionsController(IElectionService service, IElectionAccessService access, IElectionApprovalService approvals, IElectionFormService forms) : ControllerBase
{
    // Admin sees every election. An official sees only the ones they hold a live appointment on.
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AdminElectionDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();

        var roles = CallerRoles();
        var elections = await service.ListAdminElectionsAsync(ct);
        if (!roles.Contains(Constants.Roles.Admin) && !roles.Contains(Constants.Roles.SuperAdmin))
        {
            var mine = await access.ElectionIdsWithLiveAppointmentAsync(userId, ct);
            elections = elections.Where(e => mine.Contains(e.Id)).ToList();
        }

        var result = new List<AdminElectionDto>(elections.Count);
        foreach (var election in elections)
            result.Add(await WithAccessAsync(election, userId, roles, ct));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> Create([FromBody] CreateAdminElectionRequest request, CancellationToken ct)
    {
        if (request is null)
            return BadRequest();

        if (!int.TryParse(this.CurrentMemberIdRaw(), out var memberId))
            return Unauthorized();

        // Every position is checked before the election is created, so a bad one cannot leave half an election.
        var seats = new List<ElectionSeatRequestDto>();
        foreach (var position in request.Positions ?? Array.Empty<AdminElectionPositionRequest>())
        {
            if (string.IsNullOrWhiteSpace(position.Title) && position.PositionId is not > 0)
                continue;

            // 95.2: the id when the client sends one, else a title that matches a position key or name.
            var positionId = position.PositionId is > 0 ? position.PositionId : await service.FindPositionIdAsync(position.Title, ct);
            if (positionId is null)
                return Problem(detail: $"Position '{position.Title}' is not supported.", statusCode: StatusCodes.Status400BadRequest);
            if (seats.Any(s => s.PositionId == positionId.Value))
                return Problem(detail: $"Position '{position.Title}' is listed twice.", statusCode: StatusCodes.Status400BadRequest);

            var seat = new ElectionSeatRequestDto(positionId.Value, Math.Max(1, position.Seats));
            var problem = await service.CheckSeatAsync(null, seat, ct);
            if (problem != null)
                return Problem(detail: problem, statusCode: StatusCodes.Status400BadRequest);
            seats.Add(seat);
        }

        var (created, error) = await service.CreateAsync(new CreateElectionDto(
            request.Title,
            request.ECPeriodId,
            request.NominationOpensOn,
            request.NominationClosesOn,
            request.PollingOpensOn,
            request.PollingClosesOn,
            memberId,
            request.TieRule,
            request.IsTest,
            request.ScrutinyOn,
            request.WithdrawalClosesOn), ct);
        if (created is null)
            return Problem(detail: ElectionsController.CreateErrorDetail(error), statusCode: StatusCodes.Status400BadRequest);

        foreach (var seat in seats)
            await service.AddSeatAsync(created.Id, seat, ct);

        return Ok(await DetailAsync(created.Id, ct));
    }

    [HttpPost("{id:int}/publish")]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ChangePhase)]
    public async Task<IActionResult> Publish(int id, CancellationToken ct)
        => await RunAsync(id, ElectionApprovalAction.Publish, "Election is not ready to publish.", ct);

    /// <summary>Spec 023 FR-001: stores the returning officer's public key. Allowed only before polling opens.
    /// Replacing a key already set may wait for a second person (37.12f, 202).</summary>
    [HttpPost("{id:int}/ballot-key")]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.SetBallotKey)]
    public async Task<IActionResult> SetBallotKey(int id, [FromBody] SetBallotKeyRequest request, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var run = await approvals.RunOrRequestAsync(id, ElectionApprovalAction.ReplaceBallotKey, userId, CallerRoles(), request.PublicKey, ct);
        if (this.ApprovalReply(run) is { } reply) return reply;
        if (run.Error is null) return Ok(await DetailAsync(id, ct));
        return run.Error switch
        {
            Elections.Replies.NotFound => NotFound(),
            "phase-closed" => Problem(detail: "The key cannot change once polling has opened.", statusCode: StatusCodes.Status400BadRequest),
            _ => Problem(detail: $"The key must be an RSA public key of at least {Elections.BallotKeyMinBits} bits.", statusCode: StatusCodes.Status400BadRequest),
        };
    }

    [HttpPost("{id:int}/close")]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ChangePhase)]
    public async Task<IActionResult> Close(int id, CancellationToken ct)
        => await RunAsync(id, ElectionApprovalAction.ClosePolling, "Election is not ready to close.", ct);

    // Spec 023 (37.12f): a two-person step replies 202 with the stored request instead of running.
    private async Task<IActionResult> RunAsync(int id, ElectionApprovalAction action, string notReady, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var run = await approvals.RunOrRequestAsync(id, action, userId, CallerRoles(), ct: ct);
        if (this.ApprovalReply(run) is { } reply) return reply;
        return run.Error is null && await DetailAsync(id, ct) is { } updated
            ? Ok(updated)
            : Problem(detail: notReady, statusCode: StatusCodes.Status400BadRequest);
    }

    [HttpPost("{id:int}/candidates")]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ManageSetup)]
    public async Task<IActionResult> AddCandidate(int id, [FromBody] SaveCandidateRequest request, CancellationToken ct)
    {
        var (success, error, election) = await service.AddCandidateAsync(id, request, ct);
        if (!success)
            return error switch
            {
                "duplicate-candidate" => Problem(detail: "This member is already a candidate for this seat.", statusCode: StatusCodes.Status409Conflict),
                "phase-closed" => Problem(detail: "Candidates can only be added up to the scrutiny phase.", statusCode: StatusCodes.Status400BadRequest),
                "not-eligible" => Problem(detail: "The candidate must be an eligible voter.", statusCode: StatusCodes.Status400BadRequest),
                "invalid-proposer" => Problem(detail: "Proposer and seconder must be two other eligible voters.", statusCode: StatusCodes.Status400BadRequest),
                _ => NotFound(),
            };

        return Ok(election);
    }

    [HttpDelete("{id:int}/candidates/{candidateId:int}")]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ManageSetup)]
    public async Task<IActionResult> RemoveCandidate(int id, int candidateId, CancellationToken ct)
    {
        var (success, error) = await service.RemoveCandidateAsync(id, candidateId, ct);
        if (!success)
            return error switch
            {
                "has-votes" => Problem(detail: "This candidate already has votes recorded and cannot be removed.", statusCode: StatusCodes.Status409Conflict),
                "phase-closed" => Problem(detail: "The candidate list is published. A candidate can now only withdraw.", statusCode: StatusCodes.Status400BadRequest),
                _ => NotFound(),
            };

        return Ok();
    }

    // 37.14m. The calendar and notice fields the printed forms need (ER-01, ER-02).
    [HttpPut("{id:int}/form-details")]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ManageSetup)]
    public async Task<IActionResult> UpdateFormDetails(int id, [FromBody] UpdateElectionFormDetailsRequest request, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var (error, detail) = await forms.UpdateFormDetailsAsync(id, request, userId, ct);
        if (error is not null) return FormError(error, detail);
        return await DetailAsync(id, ct) is { } updated ? Ok(updated) : NotFound();
    }

    // ER-10 and ER-17. The nominations with their consent times, which the public list leaves out.
    [HttpGet("{id:int}/nominations")]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ViewDashboard)]
    public async Task<IActionResult> Nominations(int id, CancellationToken ct)
        => Ok(await service.GetNominationsWithConsentAsync(id, ct));

    // ER-10. The time on the candidate's signed consent.
    [HttpPut("{id:int}/nominations/{nominationId:int}/consent")]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ManageSetup)]
    public async Task<IActionResult> RecordConsent(int id, int nominationId, [FromBody] RecordConsentRequest request, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var error = await forms.RecordConsentAsync(id, nominationId, request.ConsentAt!.Value, userId, ct);
        return error is null
            ? NoContent()
            : FormError(error, error == Elections.Replies.WrongPhase ? "Consent cannot be recorded once polling has opened." : "The consent time cannot be in the future.");
    }

    // ER-17. The scrutiny sheet for one nomination.
    [HttpGet("{id:int}/nominations/{nominationId:int}/scrutiny-checks")]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.DecideNominations)]
    public async Task<IActionResult> GetScrutinyChecks(int id, int nominationId, CancellationToken ct)
        => await forms.GetScrutinyChecksAsync(id, nominationId, ct) is { } checks ? Ok(checks) : NotFound();

    [HttpPut("{id:int}/nominations/{nominationId:int}/scrutiny-checks")]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.DecideNominations)]
    public async Task<IActionResult> SetScrutinyCheck(int id, int nominationId, [FromBody] SetScrutinyCheckRequest request, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var error = await forms.SetScrutinyCheckAsync(id, nominationId, request, userId, ct);
        if (error is not null)
            return FormError(error, error == Elections.Replies.WrongPhase ? "Scrutiny checks can only be ticked during scrutiny." : null);
        return await forms.GetScrutinyChecksAsync(id, nominationId, ct) is { } checks ? Ok(checks) : NotFound();
    }

    // ER-28, 28A and 28B.
    [HttpGet("{id:int}/recounts")]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ViewDashboard)]
    public async Task<IActionResult> ListRecounts(int id, CancellationToken ct)
        => Ok(await forms.ListRecountsAsync(id, ct));

    [HttpPost("{id:int}/recounts")]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.DecideAppeals)]
    public async Task<IActionResult> RecordRecount(int id, [FromBody] RecordRecountRequest request, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var (recount, error, detail) = await forms.RecordRecountAsync(id, request, userId, ct);
        return recount is not null ? Ok(recount) : FormError(error!, detail);
    }

    [HttpPut("{id:int}/recounts/{recountId:int}/decision")]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.DecideAppeals)]
    public async Task<IActionResult> DecideRecount(int id, int recountId, [FromBody] DecideRecountRequest request, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var error = await forms.DecideRecountAsync(id, recountId, request, userId, ct);
        return error is null ? NoContent() : FormError(error, error == Elections.Replies.AlreadyRecorded ? "This request has already been decided." : null);
    }

    [HttpPut("{id:int}/recounts/{recountId:int}/figures")]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.Count)]
    public async Task<IActionResult> RecordRecountFigures(int id, int recountId, [FromBody] RecordRecountFiguresRequest request, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return Unauthorized();
        var (error, detail) = await forms.RecordRecountFiguresAsync(id, recountId, request.Figures, userId, ct);
        return error is null ? NoContent() : FormError(error, detail ?? (error == Elections.Replies.AlreadyRecorded ? "The recount figures are already recorded." : null));
    }

    private IActionResult FormError(string error, string? detail) => error switch
    {
        Elections.Replies.NotFound => NotFound(),
        Elections.Replies.AlreadyRecorded or Elections.Replies.Busy => Problem(detail: detail ?? "Someone else changed this at the same time. Try again.", statusCode: StatusCodes.Status409Conflict),
        _ => Problem(detail: detail ?? "This change is not allowed now.", statusCode: StatusCodes.Status400BadRequest),
    };

    // Every reply carries the caller's permissions, so the page can swap one election in place.
    private async Task<AdminElectionDto?> DetailAsync(int id, CancellationToken ct)
    {
        var election = await service.GetAdminElectionAsync(id, ct);
        if (election is null || !int.TryParse(this.CurrentUserIdRaw(), out var userId))
            return election;
        return await WithAccessAsync(election, userId, CallerRoles(), ct);
    }

    private List<string> CallerRoles() => User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

    private async Task<AdminElectionDto> WithAccessAsync(AdminElectionDto election, int userId, IReadOnlyCollection<string> roles, CancellationToken ct)
    {
        var perms = await access.GetPermissionsAsync(election.Id, userId, roles, ct);
        var names = Enum.GetValues<ElectionPermission>()
            .Where(p => p != ElectionPermission.None && perms.HasFlag(p))
            .Select(p => p.ToString())
            .ToList();
        return election with
        {
            MyPermissions = names,
            AdminHandedOver = await access.IsHandedOverAsync(election.Id, ct),
            TooFewApprovers = await access.LiveApproverCountAsync(election.Id, ct) < Constants.Elections.MinApproversBeforePolling,
        };
    }
}
