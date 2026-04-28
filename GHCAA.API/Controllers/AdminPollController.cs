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
    [Route("api/admin/polls")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminPollController : ControllerBase
    {
        private readonly IPollService _pollService;

        public AdminPollController(IPollService pollService)
        {
            _pollService = pollService;
        }

        private int GetAdminMemberId()
        {
            var claim = User.FindFirst("MemberId");
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPolls(CancellationToken cancellationToken)
        {
            var polls = await _pollService.GetAllPollsAsync(cancellationToken);
            return Ok(polls);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePoll([FromBody] CreatePollDto dto, CancellationToken cancellationToken)
        {
            var adminId = GetAdminMemberId();
            var pollId = await _pollService.CreatePollAsync(dto, adminId, cancellationToken);
            return CreatedAtAction(nameof(GetAllPolls), new { id = pollId }, new { Id = pollId, Message = "Poll created successfully." });
        }

        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> ToggleStatus(int id, [FromBody] bool isActive, CancellationToken cancellationToken)
        {
            var success = await _pollService.TogglePollStatusAsync(id, isActive, cancellationToken);
            if (!success) return NotFound();
            return Ok(new { Message = $"Poll status updated to {(isActive ? "Active" : "Inactive")}." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePoll(int id, CancellationToken cancellationToken)
        {
            var success = await _pollService.DeletePollAsync(id, cancellationToken);
            if (!success) return NotFound();
            return Ok(new { Message = "Poll deleted successfully." });
        }
    }
}
