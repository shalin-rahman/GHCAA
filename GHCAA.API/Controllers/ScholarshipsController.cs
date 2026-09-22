using GHCAA.API.Extensions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using static GHCAA.Domain.Constants;

namespace GHCAA.API.Controllers;

[ApiController]
[Route("api/scholarships")]
public sealed class ScholarshipsController : ControllerBase
{
    private readonly IScholarshipService _scholarships;
    public ScholarshipsController(IScholarshipService scholarships) => _scholarships = scholarships;

    [HttpGet("public/funds"), AllowAnonymous]
    public async Task<IActionResult> PublicFunds(CancellationToken ct) => Ok(await _scholarships.GetPublicFundsAsync(ct));

    [HttpGet("public/calls"), AllowAnonymous]
    public async Task<IActionResult> PublicCalls(CancellationToken ct) => Ok(await _scholarships.GetPublicCallsAsync(ct));

    [HttpPost("calls/{callId:int}/applications"), AllowAnonymous]
    public async Task<IActionResult> Apply(int callId, CreateScholarshipApplicationDto dto, CancellationToken ct)
    {
        try { return Ok(await _scholarships.SubmitApplicationAsync(callId, dto, ct)); }
        catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: 404); }
        catch (InvalidOperationException ex) { return Problem(ex.Message, statusCode: 400); }
    }

    [HttpGet("status/{referenceCode}"), AllowAnonymous, EnableRateLimiting(RateLimitPolicies.ScholarshipStatus)]
    public async Task<IActionResult> Status(string referenceCode, [FromQuery] string email, CancellationToken ct)
    {
        var result = await _scholarships.GetPublicStatusAsync(referenceCode, email, ct);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("review-queue"), Authorize(Policy = Policies.MemberOnly)]
    public async Task<IActionResult> ReviewQueue(CancellationToken ct) => Ok(await _scholarships.GetReviewQueueAsync(ct));

    [HttpGet("applications/{applicationId:int}/review"), Authorize(Policy = Policies.MemberOnly)]
    public async Task<IActionResult> ReviewApplication(int applicationId, CancellationToken ct)
    {
        var result = await _scholarships.GetApplicationForReviewAsync(applicationId, ct);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("applications/{applicationId:int}/review"), Authorize(Policy = Policies.MemberOnly)]
    public async Task<IActionResult> SubmitReview(int applicationId, SubmitScholarshipReviewDto dto, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentMemberIdRaw(), out var memberId)) return Unauthorized();
        var result = await _scholarships.SubmitReviewAsync(applicationId, memberId, dto, ct);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("admin/applications"), Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> AdminApplications(CancellationToken ct) => Ok(await _scholarships.GetApplicationsForAdminAsync(ct));

    [HttpPost("admin/funds"), Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> CreateFund(CreateScholarshipFundDto dto, CancellationToken ct) => Ok(await _scholarships.CreateFundAsync(dto, ct));

    [HttpPost("admin/calls"), Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> CreateCall(CreateScholarshipCallDto dto, CancellationToken ct)
    {
        try { return Ok(await _scholarships.CreateCallAsync(dto, ct)); }
        catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: 404); }
        catch (ArgumentException ex) { return Problem(ex.Message, statusCode: 400); }
    }

    [HttpPost("admin/awards"), Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> CreateAward(CreateScholarshipAwardDto dto, CancellationToken ct)
    {
        try { return Ok(await _scholarships.CreateAwardAsync(dto, ct)); }
        catch (KeyNotFoundException ex) { return Problem(ex.Message, statusCode: 404); }
    }

    [HttpPost("admin/awards/{awardId:int}/disburse"), Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> Disburse(int awardId, CancellationToken ct)
    {
        if (!int.TryParse(this.CurrentUserIdRaw(), out var adminId)) return Unauthorized();
        var result = await _scholarships.DisburseAwardAsync(awardId, adminId, ct);
        return result ? Ok() : NotFound();
    }
}
