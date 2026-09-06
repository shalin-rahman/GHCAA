using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateUserAccountAsync(int memberId, string username, string password, CancellationToken cancellationToken = default);
        Task<User> CreateSystemAdminAsync(string username, string password, string role, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword, CancellationToken cancellationToken = default);
        Task<bool> DeleteSystemAdminAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> SetUserActiveAsync(int userId, bool isActive, CancellationToken cancellationToken = default);
        Task<(bool Success, string? ResetUrl)> SendAdminPasswordResetLinkAsync(int userId, CancellationToken cancellationToken = default);
        string GenerateDefaultPassword();
    }
}
