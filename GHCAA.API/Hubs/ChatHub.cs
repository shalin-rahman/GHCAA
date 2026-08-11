using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;

namespace GHCAA.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        // Tracks userId → connectionId for presence; ConcurrentDictionary is safe under concurrent
        // OnConnectedAsync / OnDisconnectedAsync calls. Note: last-write-wins for multi-device sessions —
        // see TODO 24.6 in Area 24 for full multi-device tracking if needed.
        private static readonly ConcurrentDictionary<string, string> _connections = new();

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                _connections[userId] = Context.ConnectionId;
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                _connections.TryRemove(userId, out _);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendDirectMessage(int receiverUserId, string message)
        {
            var senderUserIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(senderUserIdStr, out var senderUserId)) return;

            // Persist to DB
            var result = await _chatService.SendMessageAsync(senderUserId, receiverUserId, message);

            // Send real-time if online
            await Clients.Group($"User_{receiverUserId}").SendAsync("ReceiveMessage", result);
            // Also send back to sender for sync across devices
            await Clients.Caller.SendAsync("ReceiveMessage", result);
        }
    }
}
