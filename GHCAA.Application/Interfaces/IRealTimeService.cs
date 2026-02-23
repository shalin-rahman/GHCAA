using System.Threading.Tasks;

namespace GHCAA.Application.Interfaces
{
    public interface IRealTimeService
    {
        Task SendNotificationToUserAsync(int userId, object notification);
    }
}
