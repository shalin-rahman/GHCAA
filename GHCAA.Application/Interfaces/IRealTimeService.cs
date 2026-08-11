using System.Threading.Tasks;

namespace GHCAA.Application.Interfaces
{
    public interface IRealTimeService
    {
        Task SendNotificationToUserAsync(int userId, object notification);
        Task SendAdminAlertAsync(string type, object data);
        Task BroadcastToBatchAsync(string batchName, object message);
        Task BroadcastToGroupAsync(string groupName, object message);
    }
}
