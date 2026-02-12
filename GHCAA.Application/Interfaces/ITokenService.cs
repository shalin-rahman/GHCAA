using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
