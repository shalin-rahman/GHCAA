namespace GHCAA.Application.Interfaces
{
    public interface IUserService
    {
        Task<Domain.Models.User> CreateUserAccountAsync(int memberId, string username, string password, CancellationToken cancellationToken = default);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword, CancellationToken cancellationToken = default);
        string GenerateDefaultPassword();
    }
}
