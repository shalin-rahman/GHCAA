using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponseDto?> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
    }
}
