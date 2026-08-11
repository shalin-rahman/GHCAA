using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponseDto?> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
        Task<TokenResponseDto?> GoogleLoginAsync(string idToken, CancellationToken cancellationToken = default);
        Task<TokenResponseDto?> FacebookLoginAsync(string accessToken, CancellationToken cancellationToken = default);
        Task<TokenResponseDto?> SocialLoginAsync(string socialId, string? email, string? name, string provider, CancellationToken cancellationToken = default);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);
    }
}
