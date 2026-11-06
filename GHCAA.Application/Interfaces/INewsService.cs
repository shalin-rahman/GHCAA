using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    public interface INewsService
    {
        // Members-only posts are left out unless the caller says the reader is signed in.
        Task<IEnumerable<NewsPostDto>> GetActiveNewsAsync(GHCAA.Domain.Enums.ArticleCategory? articleCategory = null, GHCAA.Domain.Enums.PostType? postType = null, bool includeMembersOnly = false, CancellationToken cancellationToken = default);
        Task<IEnumerable<NewsPostDto>> GetAllNewsForAdminAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<NewsPostDto>> GetPendingSubmissionsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<NewsPostDto>> GetMySubmissionsAsync(int userId, CancellationToken cancellationToken = default);
        // Returns null for a post that is not approved and active, unless the viewer is an admin,
        // its author or a collaborator.
        Task<NewsPostDto?> GetNewsByIdAsync(int id, int? viewerUserId, bool viewerIsAdmin, CancellationToken cancellationToken = default);
        Task<NewsPostDto> CreateNewsAsync(CreateNewsDto dto, int authorId, CancellationToken cancellationToken = default);
        Task<NewsPostDto> UpdateNewsAsync(UpdateNewsDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteNewsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ApproveArticleAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> RejectArticleAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddCollaboratorAsync(int newsPostId, int userId, CancellationToken cancellationToken = default);
        Task<bool> RemoveCollaboratorAsync(int newsPostId, int userId, CancellationToken cancellationToken = default);
        /// <summary>Counts one news image upload for the account. False once the daily limit is reached.</summary>
        Task<bool> TryCountImageUploadAsync(int userId, CancellationToken cancellationToken = default);
        /// <summary>Gives back a counted upload when the save failed. Never drops below zero.</summary>
        Task ReleaseImageUploadAsync(int userId, CancellationToken cancellationToken = default);
    }
}
