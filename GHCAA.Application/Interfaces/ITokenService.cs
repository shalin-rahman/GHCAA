using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);

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
