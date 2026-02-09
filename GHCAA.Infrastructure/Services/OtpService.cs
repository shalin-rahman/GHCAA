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
        private readonly IEmailService _email;
        private readonly ILogger<OtpService> _logger;
        private readonly int _expiryMinutes;

        public OtpService(ApplicationDbContext db, IEmailService email, IConfiguration config, ILogger<OtpService> logger)
        {
            _db = db;
            _email = email;
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

            var subject = "Your GHC Alumni OTP";
            var body = $"<p>Your verification code: <strong>{code}</strong>. It expires in {_expiryMinutes} minutes.</p>";
            await _email.SendEmailAsync(email, subject, body, cancellationToken);

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
