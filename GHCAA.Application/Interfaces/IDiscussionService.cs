using GHCAA.Application.DTOs.Discussion;

namespace GHCAA.Application.Interfaces
{
    // Comment threads on a poll or a meeting (spec 028). contextType is a Constants.Discussion
    // context name. The context's own access rule decides who may read and post.
    public interface IDiscussionService
    {
        Task<DiscussionResult<CommentThreadDto>> GetThreadAsync(string contextType, int contextId, int memberId, bool isAdmin, int page = 1, int pageSize = GHCAA.Domain.Constants.Discussion.DefaultPageSize, CancellationToken cancellationToken = default);

        Task<DiscussionResult<CommentDto>> PostCommentAsync(string contextType, int contextId, int memberId, bool isAdmin, CreateCommentDto dto, CancellationToken cancellationToken = default);

        Task<DiscussionResult<bool>> RemoveCommentAsync(string contextType, int contextId, int commentId, int memberId, bool isAdmin, string? reason, CancellationToken cancellationToken = default);

        Task<DiscussionResult<bool>> SetLockedAsync(string contextType, int contextId, bool locked, int adminMemberId, CancellationToken cancellationToken = default);

        // Active comments per context id. Ids with no thread are left out.
        Task<Dictionary<int, int>> CountCommentsAsync(string contextType, IReadOnlyCollection<int> contextIds, CancellationToken cancellationToken = default);
    }
}
