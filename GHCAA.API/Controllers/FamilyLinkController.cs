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

        public FamilyLinkController(IFamilyLinkService familyLinkService)
        {
            _familyLinkService = familyLinkService;
        }

        private int GetMemberId() =>
            int.Parse(User.FindFirstValue("MemberId") ?? "0");

        /// <summary>Send a family link request to another member by membership number.</summary>
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendFamilyLinkDto dto, CancellationToken ct)
        {
            try
            {
                var result = await _familyLinkService.SendRequestAsync(GetMemberId(), dto, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Approve or reject a received family link request.</summary>
        [HttpPost("respond")]
        public async Task<IActionResult> Respond([FromBody] RespondFamilyLinkDto dto, CancellationToken ct)
        {
            var success = await _familyLinkService.RespondAsync(GetMemberId(), dto, ct);
            return success ? Ok(new { message = "Response recorded." }) : NotFound();
        }

        [HttpDelete("remove/{requestId}")]
        public async Task<IActionResult> Remove(int requestId)
        {
            var result = await _familyLinkService.RemoveLinkAsync(GetMemberId(), requestId);
            return result ? Ok() : NotFound();
        }

        /// <summary>Cancel a pending request that you sent.</summary>
        [HttpPost("{requestId}/cancel")]
        public async Task<IActionResult> Cancel(int requestId, CancellationToken ct)
        {
            var success = await _familyLinkService.CancelAsync(GetMemberId(), requestId, ct);
            return success ? Ok(new { message = "Request cancelled." }) : NotFound();
        }

        /// <summary>List all requests you sent.</summary>
        [HttpGet("sent")]
        public async Task<IActionResult> GetSent(CancellationToken ct) =>
            Ok(await _familyLinkService.GetSentRequestsAsync(GetMemberId(), ct));

        /// <summary>List all pending requests received (awaiting your response).</summary>
        [HttpGet("received")]
        public async Task<IActionResult> GetReceived(CancellationToken ct) =>
            Ok(await _familyLinkService.GetReceivedRequestsAsync(GetMemberId(), ct));

        /// <summary>Get your approved family network.</summary>
        [HttpGet("my-family")]
        public async Task<IActionResult> GetFamily(CancellationToken ct) =>
            Ok(await _familyLinkService.GetFamilyAsync(GetMemberId(), ct));
    }
}
