using GHCAA.Application.Interfaces;
using GHCAA.Domain;
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

        public CredentialVerificationController(IIDCardService idCardService) => _idCardService = idCardService;

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
        public async Task<IActionResult> Revoke(string shortCode, [FromBody] RevokeCredentialRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Reason)) return BadRequest("A revocation reason is required.");
            return await _idCardService.RevokeCredentialAsync(shortCode, request.Reason.Trim(), cancellationToken)
                ? NoContent()
                : NotFound();
        }
    }

    public sealed class RevokeCredentialRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
