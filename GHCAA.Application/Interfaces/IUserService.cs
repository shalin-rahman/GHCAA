namespace GHCAA.Application.Interfaces
{
    public interface IUserService
    {
        Task<Domain.Models.User> CreateUserAccountAsync(int memberId, string username, string password, CancellationToken cancellationToken = default);
        string GenerateDefaultPassword();
    }
}
