using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    public interface INewsService
    {
        Task<IEnumerable<NewsPostDto>> GetActiveNewsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<NewsPostDto>> GetAllNewsForAdminAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<NewsPostDto>> GetMySubmissionsAsync(int userId, CancellationToken cancellationToken = default);
        Task<NewsPostDto?> GetNewsByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<NewsPostDto> CreateNewsAsync(CreateNewsDto dto, int authorId, CancellationToken cancellationToken = default);
        Task<NewsPostDto> UpdateNewsAsync(UpdateNewsDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteNewsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ApproveArticleAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> RejectArticleAsync(int id, CancellationToken cancellationToken = default);
    }
}
