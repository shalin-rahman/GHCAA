using GHCAA.Application.Interfaces;
using GHCAA.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GHCAA.API.Services
{
    public class RealTimeService : IRealTimeService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public RealTimeService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationToUserAsync(int userId, object notification)
        {
            // We use the same group naming convention as in ChatHub
            await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notification);
        }
    }
}
