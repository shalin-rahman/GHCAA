using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    // 82.53a: persists the FCM token the mobile client registers (push_notification_service.dart)
    // so a future targeted-push feature has somewhere to read it from. Doesn't send anything itself.
    public class DeviceTokenService : IDeviceTokenService
    {
        private readonly ApplicationDbContext _db;

        public DeviceTokenService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> RegisterTokenAsync(int memberId, DeviceTokenDto dto, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            if (member == null) return false;

            member.FcmToken = dto.Token;
            member.FcmTokenPlatform = dto.Platform;
            member.FcmTokenUpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<string?> GetTokenAsync(int memberId, CancellationToken cancellationToken = default)
        {
            return await _db.Members
                .Where(m => m.Id == memberId)
                .Select(m => m.FcmToken)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
