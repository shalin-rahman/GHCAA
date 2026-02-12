using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface INewsService
    {
        Task<IEnumerable<NewsPost>> GetActiveNewsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<NewsPost>> GetAllNewsForAdminAsync(CancellationToken cancellationToken = default);
        Task<NewsPost?> GetNewsByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<NewsPost> CreateNewsAsync(NewsPost post, CancellationToken cancellationToken = default);
        Task<NewsPost> UpdateNewsAsync(NewsPost post, CancellationToken cancellationToken = default);
        Task<bool> DeleteNewsAsync(int id, CancellationToken cancellationToken = default);
    }
}
