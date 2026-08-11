using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Route("api/notification")]
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
            try
            {
                if (!TryGetMemberId(out var memberId))
                    return Unauthorized(new { message = "Member profile is required for notifications." });

                var notifications = await _notificationService.GetUserNotificationsAsync(memberId, cancellationToken);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching notifications", details = ex.Message });
            }
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken)
        {
            if (!TryGetMemberId(out var memberId))
                return Unauthorized();

            var updated = await _notificationService.MarkAsReadAsync(id, memberId, cancellationToken);
            if (!updated)
                return NotFound();
            return Ok();
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
        {
            if (!TryGetMemberId(out var memberId))
                return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(memberId, cancellationToken);
            return Ok();
        }

        private bool TryGetMemberId(out int memberId)
        {
            memberId = 0;
            var memberIdStr = User.FindFirst("MemberId")?.Value;
            return !string.IsNullOrEmpty(memberIdStr) && int.TryParse(memberIdStr, out memberId);
        }
    }
}
