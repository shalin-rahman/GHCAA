using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    // 95.6. Authenticator app (TOTP) step-up. Email step-up stays; this is a second way in.
    public interface IAuthenticatorService
    {
        Task<AuthenticatorStatusDto> GetStatusAsync(int userId, CancellationToken cancellationToken = default);

        // Replaces a pending setup. Returns null when the user already has a confirmed authenticator.
        Task<AuthenticatorSetupDto?> BeginSetupAsync(int userId, string accountName, CancellationToken cancellationToken = default);

        // Returns the recovery codes when the code matches the pending setup, otherwise null.
        Task<RecoveryCodesDto?> ConfirmAsync(int userId, string code, CancellationToken cancellationToken = default);

        // Takes an app code or an unused recovery code. A recovery code is used up when accepted.
        Task<bool> VerifyAsync(int userId, string code, CancellationToken cancellationToken = default);

        // Returns null when the user has no confirmed authenticator.
        Task<RecoveryCodesDto?> RegenerateRecoveryCodesAsync(int userId, CancellationToken cancellationToken = default);

        // Removes the user's authenticator, pending or confirmed. False when there was none.
        Task<bool> RemoveAsync(int userId, CancellationToken cancellationToken = default);

        // A SuperAdmin reset for a lost phone. Also signs the user out everywhere and writes an
        // audit row. False when the user had no authenticator.
        Task<bool> ResetByAdminAsync(int userId, int adminId, CancellationToken cancellationToken = default);

        // Used in place of an email step-up for a user with no email on file.
        Task<bool> PasswordMatchesAsync(int userId, string password, CancellationToken cancellationToken = default);
    }
}
