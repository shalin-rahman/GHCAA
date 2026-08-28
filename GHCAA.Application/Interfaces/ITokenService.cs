using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);

        // 7.13: Same token, plus a step_up_verified_at claim proving the user completed an OTP
        // challenge just now. Gates destructive/financial admin actions via [RequireStepUp].
        string CreateStepUpToken(User user);

        // Same token, carrying forward a step-up verification from an earlier token rather than
        // minting a fresh one — used by /auth/refresh so the ~30-day grace period survives the
        // access token's hourly silent refresh instead of being wiped by it.
        string CreateTokenWithCarriedStepUp(User user, long stepUpVerifiedAtEpochSeconds);

        // Validates a (possibly expired) previous access token's signature/issuer/audience and,
        // if it carries a step-up claim still within ttlMinutes, returns that claim's epoch.
        // Returns null for a missing, tampered, or lapsed claim — never trust an unverified token.
        long? TryGetValidStepUpEpoch(string? previousAccessToken, int ttlMinutes);

        // 24.27: Returns a cryptographically random plaintext token (not stored).
        string GenerateRefreshToken();

        // Hashes the plaintext token and persists it for userId.
        Task StoreRefreshTokenAsync(int userId, string plaintextToken, CancellationToken cancellationToken = default);

        // Validates the token by hash; on success revokes it and issues a replacement.
        // Returns (newToken, userId), or null if validation fails.
        Task<(string NewToken, int UserId)?> RotateRefreshTokenAsync(string plaintextToken, CancellationToken cancellationToken = default);

        // Revokes all active refresh tokens for a user (logout).
        Task RevokeAllRefreshTokensAsync(int userId, CancellationToken cancellationToken = default);
    }
}
