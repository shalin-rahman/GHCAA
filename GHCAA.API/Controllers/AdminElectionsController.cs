using System;
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
[Route("api/admin/elections")]
[Authorize(Policy = Policies.ElectionStaff)]
[GHCAA.API.Filters.RequireStepUp]
public sealed class AdminElectionsController(IElectionService service, IElectionAccessService access) : ControllerBase
{
    // Admin sees every election. An official sees only the ones they hold a live appointment on.
    [HttpGet]
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

        var created = await service.CreateAsync(new CreateElectionDto(
            request.Title,
            request.ECPeriodId,
            request.NominationOpensOn,
            request.NominationClosesOn,
            request.PollingOpensOn,
            request.PollingClosesOn,
            memberId,
            request.TieRule), ct);

        foreach (var position in request.Positions ?? Array.Empty<AdminElectionPositionRequest>())
        {
            if (string.IsNullOrWhiteSpace(position.Title))
                continue;

            var parsed = ParsePosition(position.Title);
            if (parsed == ECPosition.None)
                return Problem(detail: $"Position '{position.Title}' is not supported.", statusCode: StatusCodes.Status400BadRequest);

            await service.AddSeatAsync(created.Id, new ElectionSeatRequestDto(parsed, Math.Max(1, position.Seats)), ct);
        }

        return Ok(await DetailAsync(created.Id, ct));
    }

    [HttpPost("{id:int}/publish")]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ChangePhase)]
    public async Task<IActionResult> Publish(int id, CancellationToken ct)
    {
        var updated = await service.SetPhaseAsync(id, ElectionPhase.Nomination, ct)
            ? await DetailAsync(id, ct)
            : null;

        return updated is null ? Problem(detail: "Election is not ready to publish.", statusCode: StatusCodes.Status400BadRequest) : Ok(updated);
    }

    /// <summary>Spec 023 FR-001: stores the returning officer's public key. Allowed only before polling opens.</summary>
    [HttpPost("{id:int}/ballot-key")]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.SetBallotKey)]
    public async Task<IActionResult> SetBallotKey(int id, [FromBody] SetBallotKeyRequest request, CancellationToken ct)
    {
        var (success, error, _) = await service.SetBallotKeyAsync(id, request.PublicKey, ct);
        if (success) return Ok(await DetailAsync(id, ct));
        return error switch
        {
            "not-found" => NotFound(),
            "phase-closed" => Problem(detail: "The key cannot change once polling has opened.", statusCode: StatusCodes.Status400BadRequest),
            _ => Problem(detail: $"The key must be an RSA public key of at least {Elections.BallotKeyMinBits} bits.", statusCode: StatusCodes.Status400BadRequest),
        };
    }

    [HttpPost("{id:int}/close")]
    [GHCAA.API.Filters.RequireElectionPermission(ElectionPermission.ChangePhase)]
    public async Task<IActionResult> Close(int id, CancellationToken ct)
    {
        var updated = await service.SetPhaseAsync(id, ElectionPhase.Counting, ct)
            ? await DetailAsync(id, ct)
            : null;

        return updated is null ? Problem(detail: "Election is not ready to close.", statusCode: StatusCodes.Status400BadRequest) : Ok(updated);
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
        return election with { MyPermissions = names, AdminHandedOver = await access.IsHandedOverAsync(election.Id, ct) };
    }

    private static ECPosition ParsePosition(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ECPosition.None;

        var normalised = value.Trim();
        if (normalised.Contains("Joint Secretary", StringComparison.OrdinalIgnoreCase))
            return normalised.Contains("2", StringComparison.OrdinalIgnoreCase)
                ? ECPosition.JointSecretary2
                : ECPosition.JointSecretary1;

        if (normalised.Contains("Media", StringComparison.OrdinalIgnoreCase) && normalised.Contains("Sports", StringComparison.OrdinalIgnoreCase))
            return ECPosition.MediaCulturalAndSportsSecretary;

        if (normalised.Contains("Information", StringComparison.OrdinalIgnoreCase) && normalised.Contains("Technology", StringComparison.OrdinalIgnoreCase))
            return ECPosition.InformationAndTechnologySecretary;

        if (normalised.Contains("Member", StringComparison.OrdinalIgnoreCase) && normalised.Contains("1", StringComparison.OrdinalIgnoreCase))
            return ECPosition.Member1;

        if (normalised.Contains("Member", StringComparison.OrdinalIgnoreCase) && normalised.Contains("2", StringComparison.OrdinalIgnoreCase))
            return ECPosition.Member2;

        var compact = string.Concat(normalised.Where(char.IsLetterOrDigit)).ToLowerInvariant();

        return compact switch
        {
            "president" => ECPosition.President,
            "vicepresident" => ECPosition.VicePresident,
            "generalsecretary" => ECPosition.GeneralSecretary,
            "officesecretary" => ECPosition.OfficeSecretary,
            "treasurer" => ECPosition.Treasurer,
            "organizationalsecretary" => ECPosition.OrganizationalSecretary,
            "lawsecretary" => ECPosition.LawSecretary,
            "immediatepastpresident" => ECPosition.ImmediatePastPresident,
            "institutionalrepresentative" => ECPosition.InstitutionalRepresentative,
            _ => Enum.TryParse<ECPosition>(normalised.Replace("-", string.Empty).Replace("&", string.Empty).Replace(" ", string.Empty), true, out var position)
                ? position
                : ECPosition.None,
        };
    }

    private static string SeatTitle(ECPosition position) => position switch
    {
        ECPosition.President => "President",
        ECPosition.VicePresident => "Vice President",
        ECPosition.GeneralSecretary => "General Secretary",
        ECPosition.OfficeSecretary => "Office Secretary",
        ECPosition.JointSecretary1 => "Joint Secretary 1",
        ECPosition.JointSecretary2 => "Joint Secretary 2",
        ECPosition.Treasurer => "Treasurer",
        ECPosition.MediaCulturalAndSportsSecretary => "Media Cultural & Sports Secretary",
        ECPosition.OrganizationalSecretary => "Organizational Secretary",
        ECPosition.InformationAndTechnologySecretary => "Information and Technology Secretary",
        ECPosition.Member1 => "Member 1",
        ECPosition.Member2 => "Member 2",
        ECPosition.LawSecretary => "Law Secretary",
        ECPosition.ImmediatePastPresident => "Immediate Past President",
        ECPosition.InstitutionalRepresentative => "Institutional Representative",
        _ => position.ToString(),
    };
}
