using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Extensions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GHCAA.Domain.Enums;
using static GHCAA.Domain.Constants;

namespace GHCAA.API.Controllers;

[ApiController]
[Route("api/admin/elections")]
[Authorize(Policy = Policies.AdminOnly)]
[GHCAA.API.Filters.RequireStepUp]
public sealed class AdminElectionsController(IElectionService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(await service.ListAdminElectionsAsync(ct));

    [HttpPost]
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
            memberId), ct);

        foreach (var position in request.Positions ?? Array.Empty<AdminElectionPositionRequest>())
        {
            if (string.IsNullOrWhiteSpace(position.Title))
                continue;

            var parsed = ParsePosition(position.Title);
            if (parsed == ECPosition.None)
                return BadRequest($"Position '{position.Title}' is not supported.");

            await service.AddSeatAsync(created.Id, new ElectionSeatRequestDto(parsed, Math.Max(1, position.Seats)), ct);
        }

        return Ok(await service.GetAdminElectionAsync(created.Id, ct));
    }

    [HttpPost("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id, CancellationToken ct)
    {
        var updated = await service.SetPhaseAsync(id, ElectionPhase.Nomination, ct)
            ? await service.GetAdminElectionAsync(id, ct)
            : null;

        return updated is null ? BadRequest("Election is not ready to publish.") : Ok(updated);
    }

    [HttpPost("{id:int}/close")]
    public async Task<IActionResult> Close(int id, CancellationToken ct)
    {
        var updated = await service.SetPhaseAsync(id, ElectionPhase.Counting, ct)
            ? await service.GetAdminElectionAsync(id, ct)
            : null;

        return updated is null ? BadRequest("Election is not ready to close.") : Ok(updated);
    }

    [HttpPost("{id:int}/candidates")]
    public async Task<IActionResult> AddCandidate(int id, [FromBody] SaveCandidateRequest request, CancellationToken ct)
    {
        var (success, error, election) = await service.AddCandidateAsync(id, request, ct);
        if (!success)
            return error == "duplicate-candidate" ? Conflict("This member is already a candidate for this seat.") : NotFound();

        return Ok(election);
    }

    [HttpDelete("{id:int}/candidates/{candidateId:int}")]
    public async Task<IActionResult> RemoveCandidate(int id, int candidateId, CancellationToken ct)
    {
        var (success, error) = await service.RemoveCandidateAsync(id, candidateId, ct);
        if (!success)
            return error == "has-votes" ? Conflict("This candidate already has votes recorded and cannot be removed.") : NotFound();

        return Ok();
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
