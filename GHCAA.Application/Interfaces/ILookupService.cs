using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface ILookupService
    {
        Task<IDictionary<string, IEnumerable<LookupDto>>> GetAllLookupsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<LookupDto>> GetByGroupAsync(string group, CancellationToken cancellationToken = default);

        // Admin Management
        Task<LookupItem> AddLookupItemAsync(LookupItem item, CancellationToken cancellationToken = default);
        Task<bool> UpdateLookupItemAsync(int id, LookupItem item, CancellationToken cancellationToken = default);
        Task<bool> DeleteLookupItemAsync(int id, CancellationToken cancellationToken = default);
    }
}
