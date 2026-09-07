using System.Security.Cryptography;
using System.Text;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GHCAA.Infrastructure.Services
{
    public class OtpService : IOtpService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICommunicationService _communication;
        private readonly ILogger<OtpService> _logger;
        private readonly int _expiryMinutes;
        private readonly string _hashKey;
        private const int MaxOtpAttempts = 5;

        public OtpService(ApplicationDbContext db, ICommunicationService communication, IOptions<OtpSettingsOptions> otpSettings, IConfiguration config, ILogger<OtpService> logger)
        {
            _db = db;
            _communication = communication;
            _logger = logger;
            _expiryMinutes = otpSettings.Value.ExpiryMinutes;
            // SECURITY AUDIT (2026-08-29): previously keyed on the email address, which is not a
            // secret — that made this an effectively unkeyed hash of a 6-digit code, brute-forceable
            // in microseconds from a DB dump. Reuses the JWT signing key as the HMAC secret (a real
            // server-side secret already required to be configured) rather than introduce a new
            // required config value; email still goes into the message for per-user domain separation.
            // Jwt:Key stays a raw IConfiguration read here (not JwtOptions) because JwtOptions
            // deliberately doesn't carry the signing secret — see its comment.
            _hashKey = otpSettings.Value.HashKey ?? config["Jwt:Key"]
                ?? throw new InvalidOperationException("OtpSettings:HashKey or Jwt:Key must be configured.");
        }

        // HMAC-SHA256(message="{email}:{code}", key=server secret) — keyed on a real secret so the
        // hash can't be brute-forced from a DB dump alone; email stays in the message so the same
        // code for different users still produces different stored values.
        private string ComputeOtpHash(string code, string email)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_hashKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{email}:{code}"));
            return Convert.ToHexString(hash).ToLowerInvariant(); // 64 lowercase hex chars
        }

        public async Task<string> GenerateAndSendOtpAsync(string email, Domain.Enums.OtpPurpose purpose = Domain.Enums.OtpPurpose.Registration, CancellationToken cancellationToken = default)
        {
            var plainCode = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();

            // Invalidate previous unverified OTPs for this email and purpose so only the latest is
            // valid. Scoped by purpose so issuing a step-up code can't silently void a pending
            // registration code (and vice versa).
            var previous = await _db.Otps
                .Where(o => o.Email == email && !o.IsVerified && o.Purpose == purpose)
                .ToListAsync(cancellationToken);
            foreach (var old in previous)
                old.IsVerified = true;

            var otp = new Otp
            {
                Email = email,
                Code = ComputeOtpHash(plainCode, email), // store hash, not plaintext
                ExpiryAt = DateTime.UtcNow.AddMinutes(_expiryMinutes),
                Purpose = purpose
            };

            await _db.Otps.AddAsync(otp, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // Send the plaintext code in the email — the hash is never exposed
            var customVars = new Dictionary<string, string> { { "OtpCode", plainCode } };
            await _communication.SendEmailByCodeAsync(email, "OTP_EMAIL", customVars, null, cancellationToken);

            _logger.LogInformation("OTP generated for {Email}", email);
            return plainCode;
        }

        public async Task<bool> VerifyOtpAsync(string email, string code, Domain.Enums.OtpPurpose purpose = Domain.Enums.OtpPurpose.Registration, CancellationToken cancellationToken = default)
        {
            // Find the latest active OTP for this email and purpose regardless of the submitted
            // code, so we can increment Attempts even on a miss. Purpose is part of the lookup so
            // a code issued for one flow can never satisfy a challenge from another.
            var otp = await _db.Otps
                .Where(o => o.Email == email && !o.IsVerified && o.Purpose == purpose && o.ExpiryAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (otp == null) return false;

            if (otp.Attempts >= MaxOtpAttempts)
            {
                _logger.LogWarning("OTP for {Email} is locked after {Max} failed attempts", email, MaxOtpAttempts);
                return false;
            }

            // 24.20: Compare the HMAC hash of the submitted code to the stored hash.
            var submittedHash = ComputeOtpHash(code, email);
            if (otp.Code != submittedHash)
            {
                otp.Attempts++;
                await _db.SaveChangesAsync(cancellationToken);
                return false;
            }

            otp.IsVerified = true;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
