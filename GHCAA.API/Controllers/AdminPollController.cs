using GHCAA.API.Extensions;
using GHCAA.Application.DTOs;
using GHCAA.Application.DTOs.Discussion;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/admin/polls")]
    [Authorize(Policy = Constants.Policies.AdminOnly)]
    public class AdminPollController : ControllerBase
    {
        private readonly IPollService _pollService;
        private readonly IDiscussionService _discussion;

        public AdminPollController(IPollService pollService, IDiscussionService discussion)
        {
            _pollService = pollService;
            _discussion = discussion;
        }

        private int GetAdminMemberId()
        {
            var claim = this.CurrentMemberIdRaw();
            return claim != null ? int.Parse(claim) : 0;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPolls([FromQuery] string? category, [FromQuery] bool includeArchived, CancellationToken cancellationToken)
        {
            var polls = await _pollService.GetAllPollsAsync(category, includeArchived, cancellationToken);
            return Ok(polls);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePoll([FromBody] CreatePollDto dto, CancellationToken cancellationToken)
        {
            var adminId = GetAdminMemberId();
            int? userId = int.TryParse(this.CurrentUserIdRaw(), out var uid) ? uid : null;
            try
            {
                var pollId = await _pollService.CreatePollAsync(dto, adminId, userId, cancellationToken);
                return CreatedAtAction(nameof(GetAllPolls), new { id = pollId }, new { Id = pollId, Message = "Poll created successfully." });
            }
            catch (ArgumentException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
            }
        }

        [HttpPut("{id:int}/comments/lock")]
        public async Task<IActionResult> SetCommentsLocked(int id, [FromBody] SetThreadLockDto dto, CancellationToken cancellationToken)
        {
            var result = await _discussion.SetLockedAsync(Constants.Discussion.ContextPoll, id, dto.Locked, GetAdminMemberId(), cancellationToken);
            return this.ToActionResult(result, _ => new { Message = dto.Locked ? "Comments locked." : "Comments unlocked." });
        }

        [HttpPut("{id:int}/comments")]
        public async Task<IActionResult> SetAllowComments(int id, [FromBody] SetAllowCommentsDto dto, CancellationToken cancellationToken)
        {
            var success = await _pollService.SetAllowCommentsAsync(id, dto.AllowComments, GetAdminMemberId(), cancellationToken);
            if (!success) return NotFound();
            return Ok(new { Message = dto.AllowComments ? "Comments turned on." : "Comments turned off." });
        }

        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> ToggleStatus(int id, [FromBody] bool isActive, CancellationToken cancellationToken)
        {
            var success = await _pollService.TogglePollStatusAsync(id, isActive, cancellationToken);
            if (!success) return NotFound();
            return Ok(new { Message = $"Poll status updated to {(isActive ? "Active" : "Inactive")}." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePoll(int id, [FromQuery] bool hideAnnouncement, CancellationToken cancellationToken)
        {
            var success = await _pollService.DeletePollAsync(id, GetAdminMemberId(), hideAnnouncement, cancellationToken);
            if (!success) return NotFound();
            return Ok(new { Message = "Poll deleted successfully." });
        }
    }
}
