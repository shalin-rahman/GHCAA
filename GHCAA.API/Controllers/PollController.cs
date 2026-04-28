using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

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

        private int GetMemberId()
        {
            var claim = User.FindFirst("MemberId");
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActivePolls(CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            var polls = await _pollService.GetActivePollsAsync(memberId, cancellationToken);
            return Ok(polls);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPoll(int id, CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            var poll = await _pollService.GetPollByIdAsync(id, memberId, cancellationToken);
            if (poll == null) return NotFound();
            return Ok(poll);
        }

        [HttpPost("{id}/vote")]
        public async Task<IActionResult> Vote(int id, [FromBody] PollVoteDto dto, CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();

            var success = await _pollService.VoteAsync(id, memberId, dto.OptionIds, cancellationToken);
            if (!success) return BadRequest(new { Message = "Voting failed. You may have already voted or the poll is closed." });

            return Ok(new { Message = "Vote recorded successfully." });
        }
    }
}
