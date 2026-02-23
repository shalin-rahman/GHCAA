using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageDto dto, CancellationToken cancellationToken)
        {
            var senderIdClaim = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(senderIdClaim) || !int.TryParse(senderIdClaim, out var senderId))
                return BadRequest("Invalid user session");

            var message = await _chatService.SendMessageAsync(senderId, dto.ReceiverId, dto.Content, cancellationToken);
            return Ok(message);
        }

        [HttpGet("history/{otherMemberId}")]
        public async Task<IActionResult> GetHistory(int otherMemberId, CancellationToken cancellationToken)
        {
            var myIdClaim = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(myIdClaim) || !int.TryParse(myIdClaim, out var myId))
                return BadRequest("Invalid user session");

            var history = await _chatService.GetChatHistoryAsync(myId, otherMemberId, 50, cancellationToken);
            return Ok(history);
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread(CancellationToken cancellationToken)
        {
            var myIdClaim = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(myIdClaim) || !int.TryParse(myIdClaim, out var myId))
                return BadRequest("Invalid user session");

            var unread = await _chatService.GetUnreadMessagesAsync(myId, cancellationToken);
            return Ok(unread);
        }

        [HttpPatch("read/{messageId}")]
        public async Task<IActionResult> MarkRead(int messageId, CancellationToken cancellationToken)
        {
            await _chatService.MarkAsReadAsync(messageId, cancellationToken);
            return Ok();
        }
    }

    public class ChatMessageDto
    {
        public int ReceiverId { get; set; }
        public string Content { get; set; } = null!;
    }
}
