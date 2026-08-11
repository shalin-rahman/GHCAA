using GHCAA.Application.Interfaces;
using GHCAA.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace GHCAA.API.Services
{
    public class RealTimeService : IRealTimeService
    {
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly IHubContext<ChatHub> _chatHub;

        public RealTimeService(IHubContext<NotificationHub> notificationHub, IHubContext<ChatHub> chatHub)
        {
            _notificationHub = notificationHub;
            _chatHub = chatHub;
        }

        public async Task SendNotificationToUserAsync(int userId, object notification)
        {
            // Push to both user-specific groups in respective hubs
            await _notificationHub.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notification);
            await _chatHub.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notification);
        }

        public async Task SendAdminAlertAsync(string type, object data)
        {
            await _notificationHub.Clients.Group("Admins").SendAsync("ReceiveAdminAlert", new { Type = type, Data = data, Timestamp = System.DateTime.UtcNow });
        }

        public async Task BroadcastToBatchAsync(string batchName, object message)
        {
            await _notificationHub.Clients.Group($"Batch_{batchName}").SendAsync("ReceiveBatchMessage", message);
        }

        public async Task BroadcastToGroupAsync(string groupName, object message)
        {
            await _notificationHub.Clients.Group(groupName).SendAsync("ReceiveBroadcast", message);
        }
    }
}
