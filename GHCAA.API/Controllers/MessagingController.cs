using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GHCAA.API.Extensions;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/messaging")]
    [Route("api/chat")]
    [Authorize]
    public class MessagingController : ControllerBase
    {
        private readonly IChatService _chatService;

        public MessagingController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("recent")]
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations(CancellationToken cancellationToken)
        {
            var userIdStr = this.CurrentUserIdRaw();
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var recent = await _chatService.GetRecentChatsAsync(userId, cancellationToken);
            return Ok(recent);
        }

        [HttpGet("history/{otherUserId}")]
        public async Task<IActionResult> GetChatHistory(int otherUserId, CancellationToken cancellationToken)
        {
            var userIdStr = this.CurrentUserIdRaw();
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            // We default to 50 messages for mobile history view
            var history = await _chatService.GetChatHistoryAsync(userId, otherUserId, 50, cancellationToken);
            return Ok(history);
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
        {
            var userIdStr = this.CurrentUserIdRaw();
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var unread = await _chatService.GetUnreadMessagesAsync(userId, cancellationToken);
            return Ok(unread);
        }

        [HttpPost("mark-read/{messageId}")]
        [HttpPatch("read/{messageId}")]
        public async Task<IActionResult> MarkAsRead(int messageId, CancellationToken cancellationToken)
        {
            await _chatService.MarkAsReadAsync(messageId, cancellationToken);
            return Ok(new { Message = "Marked as read" });
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageDto dto, CancellationToken cancellationToken)
        {
            var userIdStr = this.CurrentUserIdRaw();
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var message = await _chatService.SendMessageAsync(userId, dto.ReceiverId, dto.Content, cancellationToken);
            return Ok(message);
        }
    }

    public class ChatMessageDto
    {
        public int ReceiverId { get; set; }
        public string Content { get; set; } = null!;
    }
}
