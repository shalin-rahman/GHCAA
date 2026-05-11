using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/family-links")]
    [Authorize]
    public class FamilyLinkController : ControllerBase
    {
        private readonly IFamilyLinkService _familyLinkService;
        private readonly IFamilyService _familyService;

        public FamilyLinkController(IFamilyLinkService familyLinkService, IFamilyService familyService)
        {
            _familyLinkService = familyLinkService;
            _familyService = familyService;
        }

        // 24.50: Returns null when claim is absent or malformed, avoiding int.Parse crash.
        private int? GetMemberId()
        {
            var value = User.FindFirstValue("MemberId");
            return int.TryParse(value, out var id) ? id : null;
        }

        /// <summary>Send a family link request to another member by membership number.</summary>
        [HttpPost("send")]
        [HttpPost("/api/members/family")] // Legacy alias for mobile
        public async Task<IActionResult> Send([FromBody] SendFamilyLinkDto dto, CancellationToken ct)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();
            try
            {
                var result = await _familyLinkService.SendRequestAsync(memberId.Value, dto, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Approve or reject a received family link request.</summary>
        [HttpPost("respond")]
        public async Task<IActionResult> Respond([FromBody] RespondFamilyLinkDto dto, CancellationToken ct)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();
            var success = await _familyLinkService.RespondAsync(memberId.Value, dto, ct);
            return success ? Ok(new { message = "Response recorded." }) : NotFound();
        }

        [HttpDelete("remove/{requestId}")]
        public async Task<IActionResult> Remove(int requestId)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();
            var result = await _familyLinkService.RemoveLinkAsync(memberId.Value, requestId);
            return result ? Ok() : NotFound();
        }

        /// <summary>Cancel a pending request that you sent.</summary>
        [HttpPost("{requestId}/cancel")]
        public async Task<IActionResult> Cancel(int requestId, CancellationToken ct)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();
            var success = await _familyLinkService.CancelAsync(memberId.Value, requestId, ct);
            return success ? Ok(new { message = "Request cancelled." }) : NotFound();
        }

        /// <summary>List all requests you sent.</summary>
        [HttpGet("sent")]
        public async Task<IActionResult> GetSent(CancellationToken ct)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();
            return Ok(await _familyLinkService.GetSentRequestsAsync(memberId.Value, ct));
        }

        /// <summary>List all pending requests received (awaiting your response).</summary>
        [HttpGet("received")]
        public async Task<IActionResult> GetReceived(CancellationToken ct)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();
            return Ok(await _familyLinkService.GetReceivedRequestsAsync(memberId.Value, ct));
        }

        /// <summary>Get your approved family network.</summary>
        [HttpGet("my-family")]
        [HttpGet("/api/members/family")] // Legacy alias for mobile
        [HttpGet("/api/Family/links")]   // Web parity alias
        public async Task<IActionResult> GetFamily(CancellationToken ct)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();
            return Ok(await _familyLinkService.GetFamilyAsync(memberId.Value, memberId.Value, ct));
        }

        /// <summary>Get the public family network of a specific member.</summary>
        [HttpGet("{memberId}/family")]
        public async Task<IActionResult> GetPublicFamily(int memberId, CancellationToken ct)
        {
            var requesterId = GetMemberId();
            if (requesterId == null) return Unauthorized();
            return Ok(await _familyLinkService.GetFamilyAsync(memberId, requesterId.Value, ct));
        }

        /// <summary>Search for members by name to link as family.</summary>
        [HttpGet("search")]
        [HttpGet("/api/Family/search")] // Web parity alias
        public async Task<IActionResult> Search([FromQuery] string name, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name required");
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();
            var results = await _familyService.SearchByNameAsync(name, memberId.Value, ct);
            return Ok(results);
        }
    }
}
