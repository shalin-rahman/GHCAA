using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using static GHCAA.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GHCAA.Application.Security;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FamilyController : ControllerBase
    {
        private readonly IFamilyService _familyService;

        public FamilyController(IFamilyService familyService)
        {
            _familyService = familyService;
        }

        private int GetMemberId()
        {
            var claim = User.FindFirst(AppClaimTypes.MemberId)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var memberId))
                throw new UnauthorizedAccessException();
            return memberId;
        }

        [HttpPost("request")]
        public async Task<IActionResult> SendRequest([FromBody] CreateFamilyRequestDto dto, CancellationToken cancellationToken)
        {
            var success = await _familyService.SendRequestAsync(GetMemberId(), dto, cancellationToken);
            return success ? Ok(new { Message = "Link request sent for approval." }) : BadRequest("Could not send request. Check member ID and status.");
        }

        [HttpPost("respond/{requestId}")]
        public async Task<IActionResult> RespondToRequest(int requestId, [FromQuery] FamilyLinkStatus status, CancellationToken cancellationToken)
        {
            if (status != FamilyLinkStatus.Accepted && status != FamilyLinkStatus.Rejected)
                return BadRequest("Invalid response status.");

            var success = await _familyService.RespondAsync(GetMemberId(), requestId, status, cancellationToken);
            return success ? Ok(new { Message = $"Family link {status.ToString().ToLower()}." }) : BadRequest("Request not found or already processed.");
        }

        [HttpDelete("request/{requestId}")]
        public async Task<IActionResult> CancelRequest(int requestId, CancellationToken cancellationToken)
        {
            var success = await _familyService.CancelRequestAsync(GetMemberId(), requestId, cancellationToken);
            return success ? Ok(new { Message = "Request cancelled." }) : BadRequest("Request not found or cannot be cancelled.");
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetRequests([FromQuery] bool receivedOnly = false, CancellationToken cancellationToken = default)
        {
            var requests = await _familyService.GetRequestsAsync(GetMemberId(), receivedOnly, cancellationToken);
            return Ok(requests);
        }

        [HttpGet("links")]
        public async Task<IActionResult> GetLinkedMembers(CancellationToken cancellationToken)
        {
            var links = await _familyService.GetLinkedMembersAsync(GetMemberId(), cancellationToken);
            return Ok(links);
        }

        [HttpDelete("unlink/{linkedMemberId}")]
        public async Task<IActionResult> UnlinkMember(int linkedMemberId, CancellationToken cancellationToken)
        {
            var success = await _familyService.UnlinkAsync(GetMemberId(), linkedMemberId, cancellationToken);
            return success ? Ok(new { Message = "Institutional link dissolved." }) : BadRequest("No active link found to dissolve.");
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchFamilyMembers([FromQuery] string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name parameter is required.");

            var results = await _familyService.SearchByNameAsync(name, GetMemberId(), cancellationToken);
            return Ok(results);
        }
    }
}
