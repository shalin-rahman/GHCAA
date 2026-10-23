using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Application.Security;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QRCoder;
using System.Security.Cryptography;
using System.Text;

namespace GHCAA.Infrastructure.Services;

// 95.6. Authenticator app step-up. The secret is encrypted with the data protection key ring,
// which lives in the database, so every instance can read it.
public sealed class AuthenticatorService(
    ApplicationDbContext db,
    IDataProtectionProvider dataProtection,
    IOrgConfigService orgConfigService,
    IActivityService activityService,
    ILogger<AuthenticatorService> logger) : IAuthenticatorService
{
    private readonly IDataProtector _protector = dataProtection.CreateProtector(Constants.Authenticator.ProtectorPurpose);

    public async Task<AuthenticatorStatusDto> GetStatusAsync(int userId, CancellationToken cancellationToken = default)
    {
        var row = await db.UserAuthenticators.AsNoTracking().FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);
        if (row == null)
            return new AuthenticatorStatusDto(false, false, 0);
        return new AuthenticatorStatusDto(row.ConfirmedAt != null, row.ConfirmedAt == null, SplitHashes(row.RecoveryCodeHashes).Count);
    }

    public async Task<AuthenticatorSetupDto?> BeginSetupAsync(int userId, string accountName, CancellationToken cancellationToken = default)
    {
        var row = await db.UserAuthenticators.FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);
        if (row?.ConfirmedAt != null)
            return null;

        if (row == null)
        {
            row = new UserAuthenticator { UserId = userId };
            db.UserAuthenticators.Add(row);
        }

        var secret = RandomNumberGenerator.GetBytes(Constants.Authenticator.SecretBytes);
        row.ProtectedSecret = Convert.ToBase64String(_protector.Protect(secret));
        row.CreatedAt = DateTime.UtcNow;
        row.LastUsedStep = 0;
        row.RecoveryCodeHashes = string.Empty;
        row.FailedAttempts = 0;
        row.LockedUntil = null;
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Two setups at once both tried to add the row. The first one wins.
            return null;
        }

        var branding = (await orgConfigService.GetConfigAsync()).Branding;
        var issuer = string.IsNullOrWhiteSpace(branding.ShortName) ? branding.AppName : branding.ShortName;
        var uri = Totp.OtpAuthUri(issuer, accountName, secret);

        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrData);
        var qr = "data:image/png;base64," + Convert.ToBase64String(qrCode.GetGraphic(8));

        return new AuthenticatorSetupDto(Totp.ToBase32(secret), uri, qr);
    }

    public async Task<RecoveryCodesDto?> ConfirmAsync(int userId, string code, CancellationToken cancellationToken = default)
    {
        var row = await db.UserAuthenticators.AsNoTracking().FirstOrDefaultAsync(a => a.UserId == userId && a.ConfirmedAt == null, cancellationToken);
        if (row == null || IsLocked(row))
            return null;

        var secret = SecretOf(row);
        if (secret == null)
            return null;

        var step = Totp.Match(secret, code.Trim(), DateTimeOffset.UtcNow, row.LastUsedStep);
        if (step == null)
        {
            await RecordFailureAsync(row.Id, cancellationToken);
            return null;
        }

        // Guarded on ConfirmedAt, so when two confirms race only one hands out recovery codes.
        var codes = NewRecoveryCodes();
        var hashes = string.Join('\n', codes.Select(HashRecoveryCode));
        var now = DateTime.UtcNow;
        var confirmed = await db.UserAuthenticators.IgnoreQueryFilters()
            .Where(a => a.Id == row.Id && a.ConfirmedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(a => a.ConfirmedAt, now)
                .SetProperty(a => a.LastUsedStep, step.Value)
                .SetProperty(a => a.RecoveryCodeHashes, hashes)
                .SetProperty(a => a.FailedAttempts, 0), cancellationToken);
        return confirmed == 1 ? new RecoveryCodesDto(codes) : null;
    }

    public async Task<bool> VerifyAsync(int userId, string code, CancellationToken cancellationToken = default)
    {
        var row = await db.UserAuthenticators.AsNoTracking().FirstOrDefaultAsync(a => a.UserId == userId && a.ConfirmedAt != null, cancellationToken);
        if (row == null || IsLocked(row))
            return false;

        var entered = code.Trim();
        if (entered.Length == Constants.Authenticator.Digits && entered.All(char.IsAsciiDigit))
        {
            var secret = SecretOf(row);
            if (secret == null)
                return false;

            var step = Totp.Match(secret, entered, DateTimeOffset.UtcNow, row.LastUsedStep);
            if (step == null)
                return await RecordFailureAsync(row.Id, cancellationToken);

            // Written only if no other request took this step first, so a code works once.
            var taken = await db.UserAuthenticators.IgnoreQueryFilters()
                .Where(a => a.Id == row.Id && a.LastUsedStep < step.Value)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(a => a.LastUsedStep, step.Value)
                    .SetProperty(a => a.FailedAttempts, 0), cancellationToken);
            return taken == 1;
        }

        var hashes = SplitHashes(row.RecoveryCodeHashes);
        var hash = HashRecoveryCode(entered);
        if (!hashes.Any(h => FixedTimeEquals(h, hash)))
            return await RecordFailureAsync(row.Id, cancellationToken);

        // Same guard for recovery codes: the update only lands on the list this request read.
        var remaining = string.Join('\n', hashes.Where(h => h != hash));
        var used = await db.UserAuthenticators.IgnoreQueryFilters()
            .Where(a => a.Id == row.Id && a.RecoveryCodeHashes == row.RecoveryCodeHashes)
            .ExecuteUpdateAsync(s => s
                .SetProperty(a => a.RecoveryCodeHashes, remaining)
                .SetProperty(a => a.FailedAttempts, 0), cancellationToken);
        return used == 1;
    }

    public async Task<RecoveryCodesDto?> RegenerateRecoveryCodesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var row = await db.UserAuthenticators.AsNoTracking().FirstOrDefaultAsync(a => a.UserId == userId && a.ConfirmedAt != null, cancellationToken);
        if (row == null)
            return null;

        // Lands only on the list this request read, so of two refreshes at once just one wins
        // and the other shows no codes rather than codes that no longer work.
        var codes = NewRecoveryCodes();
        var hashes = string.Join('\n', codes.Select(HashRecoveryCode));
        var replaced = await db.UserAuthenticators.IgnoreQueryFilters()
            .Where(a => a.Id == row.Id && a.RecoveryCodeHashes == row.RecoveryCodeHashes)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.RecoveryCodeHashes, hashes), cancellationToken);
        return replaced == 1 ? new RecoveryCodesDto(codes) : null;
    }

    public async Task<bool> RemoveAsync(int userId, CancellationToken cancellationToken = default)
    {
        var removed = await db.UserAuthenticators.IgnoreQueryFilters()
            .Where(a => a.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
        return removed > 0;
    }

    public async Task<bool> ResetByAdminAsync(int userId, int adminId, CancellationToken cancellationToken = default)
    {
        if (!await RemoveAsync(userId, cancellationToken))
            return false;

        // A new stamp ends every session the user holds, so whoever has the lost phone, or a
        // session left open on it, has to sign in again before setting up an app of their own.
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user != null)
        {
            user.RotateSecurityStamp();
            await db.SaveChangesAsync(cancellationToken);
        }

        await activityService.LogActivityAsync(user?.MemberId, Constants.Authenticator.AdminResetAuditType,
            $"Authenticator app removed for user {userId} by SuperAdmin {adminId}. The user was signed out everywhere.",
            adminId, cancellationToken: cancellationToken);
        logger.LogWarning("Authenticator app removed for user {UserId} by SuperAdmin {AdminId}", userId, adminId);
        return true;
    }

    public async Task<bool> PasswordMatchesAsync(int userId, string password, CancellationToken cancellationToken = default)
    {
        var hash = await db.Users.Where(u => u.Id == userId).Select(u => u.PasswordHash).FirstOrDefaultAsync(cancellationToken);
        return !string.IsNullOrEmpty(hash) && BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(a), Encoding.ASCII.GetBytes(b));

    private static bool IsLocked(UserAuthenticator row) => row.LockedUntil > DateTime.UtcNow;

    // Both SET expressions read the old FailedAttempts, so the count and the lock move together.
    private async Task<bool> RecordFailureAsync(int id, CancellationToken cancellationToken)
    {
        const int max = Constants.Authenticator.MaxFailedAttempts;
        DateTime? lockUntil = DateTime.UtcNow.AddMinutes(Constants.Authenticator.LockoutMinutes);
        await db.UserAuthenticators.IgnoreQueryFilters()
            .Where(a => a.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(a => a.LockedUntil, a => a.FailedAttempts + 1 >= max ? lockUntil : a.LockedUntil)
                .SetProperty(a => a.FailedAttempts, a => a.FailedAttempts + 1 >= max ? 0 : a.FailedAttempts + 1), cancellationToken);
        return false;
    }

    // A lost or rotated key ring makes the secret unreadable. The user then needs a SuperAdmin reset.
    private byte[]? SecretOf(UserAuthenticator row)
    {
        try
        {
            return _protector.Unprotect(Convert.FromBase64String(row.ProtectedSecret));
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException)
        {
            logger.LogError(ex, "Authenticator secret for user {UserId} could not be decrypted", row.UserId);
            return null;
        }
    }

    private static List<string> NewRecoveryCodes()
    {
        const int half = Constants.Authenticator.RecoveryCodeHalfLength;
        return Enumerable.Range(0, Constants.Authenticator.RecoveryCodeCount)
            .Select(_ => RandomNumberGenerator.GetString(Constants.Authenticator.RecoveryCodeAlphabet, half * 2))
            .Select(c => c[..half] + "-" + c[half..])
            .ToList();
    }

    // Dashes, spaces and case are ignored, so a code typed from paper still matches.
    internal static string HashRecoveryCode(string code)
    {
        var normalized = new string(code.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
    }

    private static List<string> SplitHashes(string hashes) =>
        hashes.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
}
