using GHCAA.API.Extensions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using static GHCAA.Domain.Constants;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/verify")]
    public sealed class CredentialVerificationController : ControllerBase
    {
        private readonly IIDCardService _idCardService;
        private readonly IActivityService _activityService;

        public CredentialVerificationController(IIDCardService idCardService, IActivityService activityService)
        {
            _idCardService = idCardService;
            _activityService = activityService;
        }

        [HttpGet("{shortCode}"), AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.CredentialVerification)]
        public async Task<IActionResult> Verify(string shortCode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(shortCode) || shortCode.Length != 10)
                return NotFound();

            var result = await _idCardService.VerifyCredentialAsync(shortCode, cancellationToken);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("{shortCode}/revoke"), Authorize(Policy = Policies.AdminOnly)]
        [GHCAA.API.Filters.RequireStepUp]
        public async Task<IActionResult> Revoke(string shortCode, [FromBody] CredentialActionRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Reason)) return Problem(detail: "A revocation reason is required.", statusCode: StatusCodes.Status400BadRequest);
            var reason = request.Reason.Trim();
            var revoked = await _idCardService.RevokeCredentialAsync(shortCode, reason, cancellationToken);
            if (revoked == null) return NotFound();

            await _activityService.LogActivityAsync(revoked.MemberId, Credentials.RevokedAuditType,
                $"Revoked {revoked.CredentialType} {shortCode}. Reason: {reason}", ActorId(),
                source: ActivitySources.Web, cancellationToken: cancellationToken);
            return NoContent();
        }

        // 84.68: revokes the old code and issues a new one of the same type, with expiry from the current setting.
        [HttpPost("{shortCode}/reissue"), Authorize(Policy = Policies.AdminOnly)]
        [GHCAA.API.Filters.RequireStepUp]
        public async Task<IActionResult> Reissue(string shortCode, [FromBody] CredentialActionRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Reason)) return Problem(detail: "A reason is required to reissue.", statusCode: StatusCodes.Status400BadRequest);
            var reason = request.Reason.Trim();
            var replacement = await _idCardService.ReissueCredentialAsync(shortCode, reason, cancellationToken);
            if (replacement == null) return NotFound();

            await _activityService.LogActivityAsync(replacement.MemberId, Credentials.ReissuedAuditType,
                $"Reissued {replacement.CredentialType} {shortCode} as {replacement.ShortCode}. Reason: {reason}", ActorId(),
                source: ActivitySources.Web, cancellationToken: cancellationToken);
            return Ok(replacement);
        }

        private int? ActorId() => int.TryParse(this.CurrentUserIdRaw(), out var id) ? id : null;
    }
}
