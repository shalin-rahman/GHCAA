using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Security;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/polls")]
    [Authorize]
    public class PollController : ControllerBase
    {
        private readonly IPollService _pollService;

        public PollController(IPollService pollService)
        {
            _pollService = pollService;
        }

        // 24.50: Returns null when the claim is absent or not a valid integer, avoiding int.Parse crash.
        private int? GetMemberId()
        {
            var value = User.FindFirst(AppClaimTypes.MemberId)?.Value;
            return int.TryParse(value, out var id) ? id : null;
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActivePolls(CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();
            var polls = await _pollService.GetActivePollsAsync(memberId.Value, cancellationToken);
            return Ok(polls);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPoll(int id, CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();
            var poll = await _pollService.GetPollByIdAsync(id, memberId.Value, cancellationToken);
            if (poll == null) return NotFound();
            return Ok(poll);
        }

        [HttpPost("{id}/vote")]
        public async Task<IActionResult> Vote(int id, [FromBody] PollVoteDto dto, CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var success = await _pollService.VoteAsync(id, memberId.Value, dto.OptionIds, cancellationToken);
            if (!success) return BadRequest(new { Message = "Voting failed. You may have already voted or the poll is closed." });

            return Ok(new { Message = "Vote recorded successfully." });
        }
    }
}
