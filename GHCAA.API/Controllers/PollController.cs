using GHCAA.API.Extensions;
using GHCAA.Application.DTOs;
using GHCAA.Application.DTOs.Discussion;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/polls")]
    [Authorize]
    public class PollController : ControllerBase
    {
        private readonly IPollService _pollService;
        private readonly IDiscussionService _discussion;

        public PollController(IPollService pollService, IDiscussionService discussion)
        {
            _pollService = pollService;
            _discussion = discussion;
        }

        // 24.50: Returns null when the claim is absent or not a valid integer, avoiding int.Parse crash.
        private int? GetMemberId()
        {
            var value = this.CurrentMemberIdRaw();
            return int.TryParse(value, out var id) ? id : null;
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActivePolls([FromQuery] string? category, CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();
            var polls = await _pollService.GetActivePollsAsync(memberId.Value, category, cancellationToken);
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
            if (!success) return Problem(detail: "Voting failed. You may have already voted or the poll is closed.", statusCode: StatusCodes.Status400BadRequest);

            return Ok(new { Message = "Vote recorded successfully." });
        }

        // A system admin has no member claim but may still read and moderate, so 0 stands in for them.
        [HttpGet("{id:int}/comments")]
        public async Task<IActionResult> GetComments(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = Domain.Constants.Discussion.DefaultPageSize, CancellationToken cancellationToken = default)
        {
            var isAdmin = this.IsDiscussionAdmin();
            var memberId = GetMemberId();
            if (memberId == null && !isAdmin) return Unauthorized();
            var result = await _discussion.GetThreadAsync(Domain.Constants.Discussion.ContextPoll, id, memberId ?? 0, isAdmin, page, pageSize, cancellationToken);
            return this.ToActionResult(result);
        }

        [HttpPost("{id:int}/comments")]
        public async Task<IActionResult> PostComment(int id, [FromBody] CreateCommentDto dto, CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Problem(detail: "Only members can comment.", statusCode: StatusCodes.Status403Forbidden);
            var result = await _discussion.PostCommentAsync(Domain.Constants.Discussion.ContextPoll, id, memberId.Value, this.IsDiscussionAdmin(), dto, cancellationToken);
            return this.ToActionResult(result);
        }

        [HttpDelete("{id:int}/comments/{commentId:int}")]
        public async Task<IActionResult> RemoveComment(int id, int commentId, [FromBody] RemoveCommentDto? dto, CancellationToken cancellationToken)
        {
            var isAdmin = this.IsDiscussionAdmin();
            var memberId = GetMemberId();
            if (memberId == null && !isAdmin) return Unauthorized();
            var result = await _discussion.RemoveCommentAsync(Domain.Constants.Discussion.ContextPoll, id, commentId, memberId ?? 0, isAdmin, dto?.Reason, cancellationToken);
            return this.ToActionResult(result, _ => new { Message = "Comment removed." });
        }
    }
}
