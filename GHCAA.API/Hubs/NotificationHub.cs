using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GHCAA.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // Join specific user group for targeted alerts
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }

            // Join specialized groups based on role
            if (Context.User?.IsInRole("Admin") == true || Context.User?.IsInRole("SuperAdmin") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }

            await base.OnConnectedAsync();
        }

        public async Task JoinBatch(string batchName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Batch_{batchName}");
        }

        public async Task JoinDepartment(string deptName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Dept_{deptName}");
        }

        public async Task SendGeneralNotice(string title, string content)
        {
            if (Context.User?.IsInRole("Admin") == true || Context.User?.IsInRole("SuperAdmin") == true)
            {
                await Clients.All.SendAsync("ReceiveNotice", new { Title = title, Content = content, Timestamp = System.DateTime.UtcNow });
            }
        }
    }
}
