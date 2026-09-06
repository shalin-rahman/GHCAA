using GHCAA.Application.DTOs;
using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponseDto?> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
        Task<TokenResponseDto?> GoogleLoginAsync(string idToken, CancellationToken cancellationToken = default);
        Task<TokenResponseDto?> FacebookLoginAsync(string accessToken, CancellationToken cancellationToken = default);
        Task<TokenResponseDto?> SocialLoginAsync(string socialId, string? email, string? name, string provider, CancellationToken cancellationToken = default);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);
        Task RequestPasswordResetAsync(string identifier, CancellationToken cancellationToken = default);

        // Backs AuthController's refresh/step-up flows: loading the current user by id (with
        // Roles, and optionally Member) and the username fallback used when SetAuthCookiesAsync
        // can't resolve the id from the JWT claim yet.
        Task<User?> GetUserWithRolesAsync(int userId, CancellationToken cancellationToken = default);
        Task<User?> GetUserWithRolesAndMemberAsync(int userId, CancellationToken cancellationToken = default);
        Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
    }
}
