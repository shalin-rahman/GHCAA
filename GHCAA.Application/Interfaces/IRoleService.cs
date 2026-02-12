using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface IRoleService
    {
        Task<Role> CreateRoleAsync(string roleName, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> GetAllRolesAsync(CancellationToken cancellationToken = default);
        Task<bool> AssignRoleToUserAsync(int userId, string roleName, CancellationToken cancellationToken = default);
        Task<bool> RemoveRoleFromUserAsync(int userId, string roleName, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default);
    }
}
