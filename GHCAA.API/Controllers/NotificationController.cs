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
            try
            {
                var memberIdStr = User.FindFirst("MemberId")?.Value;
                int userId = 0;

                if (!string.IsNullOrEmpty(memberIdStr) && int.TryParse(memberIdStr, out var mid))
                {
                    userId = mid;
                }
                else
                {
                    // Fallback: If no MemberId claim, this user might not have a member profile
                    // We check NameIdentifier which is the User.Id
                    var nameIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(nameIdStr) || !int.TryParse(nameIdStr, out var uid))
                    {
                        return Unauthorized();
                    }
                    userId = uid; 
                }

                var notifications = await _notificationService.GetUserNotificationsAsync(userId, cancellationToken);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationController] Error in GetMyNotifications: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching notifications", details = ex.Message });
            }
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
            var userIdStr = User.FindFirst("MemberId")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId)) return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
            return Ok();
        }
    }
}
