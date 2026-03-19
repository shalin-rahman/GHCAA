using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    public class OtpService : IOtpService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICommunicationService _communication;
        private readonly ILogger<OtpService> _logger;
        private readonly int _expiryMinutes;
 
        public OtpService(ApplicationDbContext db, ICommunicationService communication, IConfiguration config, ILogger<OtpService> logger)
        {
            _db = db;
            _communication = communication;
            _logger = logger;
            _expiryMinutes = int.TryParse(config["OtpSettings:ExpiryMinutes"], out var v) ? v : 10;
        }
 
        public async Task<string> GenerateAndSendOtpAsync(string email, CancellationToken cancellationToken = default)
        {
            var code = new Random().Next(0, 999999).ToString("D6");
            var otp = new Otp
            {
                Email = email,
                Code = code,
                ExpiryAt = DateTime.UtcNow.AddMinutes(_expiryMinutes)
            };
 
            await _db.Otps.AddAsync(otp, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
 
            var customVars = new Dictionary<string, string> { { "OtpCode", code } };
            // Since we might not have a MemberId yet (pre-registration verification), we use SendEmailByCodeAsync
            await _communication.SendEmailByCodeAsync(email, "OTP_EMAIL", customVars, null, cancellationToken);
 
            _logger.LogInformation("OTP generated for {Email}", email);
            return code;
        }

        public async Task<bool> VerifyOtpAsync(string email, string code, CancellationToken cancellationToken = default)
        {
            var otp = await _db.Otps
                .Where(o => o.Email == email && o.Code == code && !o.IsVerified && o.ExpiryAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (otp == null) return false;
            otp.IsVerified = true;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
