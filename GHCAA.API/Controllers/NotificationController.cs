using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications(CancellationToken cancellationToken)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, cancellationToken);
            return Ok(notifications);
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken)
        {
            await _notificationService.MarkAsReadAsync(id, cancellationToken);
            return Ok();
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
            return Ok();
        }
    }
}
