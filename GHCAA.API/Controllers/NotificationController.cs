using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GHCAA.Application.Security;
using GHCAA.API.Extensions;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Route("api/notification")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IDeviceTokenService _deviceTokenService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService notificationService, IDeviceTokenService deviceTokenService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _deviceTokenService = deviceTokenService;
            _logger = logger;
        }

        // 82.53a: registers/refreshes the FCM token push_notification_service.dart sends on
        // every app start and token rotation (82.41). Just persists it — sending a push is a
        // separate, not-yet-built feature.
        [HttpPost("device-token")]
        public async Task<IActionResult> RegisterDeviceToken([FromBody] DeviceTokenDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetMemberId(out var memberId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.Token))
                return BadRequest("Token is required.");

            var updated = await _deviceTokenService.RegisterTokenAsync(memberId, dto, cancellationToken);
            if (!updated)
                return NotFound();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications(CancellationToken cancellationToken)
        {
            try
            {
                if (!TryGetMemberId(out var memberId))
                    return Problem(detail: "Member profile is required for notifications.", statusCode: StatusCodes.Status401Unauthorized);

                var notifications = await _notificationService.GetUserNotificationsAsync(memberId, cancellationToken);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notifications");
                return Problem(detail: ex.Message, title: "Error fetching notifications", statusCode: StatusCodes.Status500InternalServerError);
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
            var memberIdStr = this.CurrentMemberIdRaw();
            return !string.IsNullOrEmpty(memberIdStr) && int.TryParse(memberIdStr, out memberId);
        }
    }
}
