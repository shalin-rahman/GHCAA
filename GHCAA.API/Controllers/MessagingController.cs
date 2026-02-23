using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/messaging")]
    [Authorize]
    public class MessagingController : ControllerBase
    {
        private readonly IChatService _chatService;

        public MessagingController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentChats(CancellationToken cancellationToken)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var recent = await _chatService.GetRecentChatsAsync(userId, cancellationToken);
            return Ok(recent);
        }

        [HttpGet("history/{otherUserId}")]
        public async Task<IActionResult> GetChatHistory(int otherUserId, CancellationToken cancellationToken)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var history = await _chatService.GetChatHistoryAsync(userId, otherUserId, 50, cancellationToken);
            return Ok(history);
        }

        [HttpPost("mark-read/{messageId}")]
        public async Task<IActionResult> MarkAsRead(int messageId, CancellationToken cancellationToken)
        {
            await _chatService.MarkAsReadAsync(messageId, cancellationToken);
            return Ok();
        }
    }
}
