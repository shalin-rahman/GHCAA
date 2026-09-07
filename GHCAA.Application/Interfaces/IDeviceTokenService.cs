using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    public interface IDeviceTokenService
    {
        /// <summary>
        /// Upserts the calling member's current FCM token. Returns false if no member with
        /// <paramref name="memberId"/> exists.
        /// </summary>
        Task<bool> RegisterTokenAsync(int memberId, DeviceTokenDto dto, CancellationToken cancellationToken = default);

        Task<string?> GetTokenAsync(int memberId, CancellationToken cancellationToken = default);
    }
}
