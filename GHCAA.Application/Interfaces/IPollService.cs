using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    public interface IPollService
    {
        // category filters on a DiscussionCategory value. Null or blank returns every category.
        Task<List<PollDto>> GetActivePollsAsync(int memberId, string? category, CancellationToken cancellationToken = default);
        Task<List<PollDto>> GetAllPollsAsync(string? category, bool includeArchived, CancellationToken cancellationToken = default);
        Task<PollDto?> GetPollByIdAsync(int id, int memberId, CancellationToken cancellationToken = default);

        // authorUserId is the admin's user id, the author of the announcement post when one is asked for.
        // Throws ArgumentException for an unknown category or an announcement with no author.
        Task<int> CreatePollAsync(CreatePollDto dto, int adminMemberId, int? authorUserId, CancellationToken cancellationToken = default);
        Task<bool> VoteAsync(int pollId, int memberId, List<int> optionIds, CancellationToken cancellationToken = default);
        Task<bool> TogglePollStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default);

        // Archives the poll and locks its comment thread. hideAnnouncement also hides the linked news post.
        Task<bool> DeletePollAsync(int id, int adminMemberId, bool hideAnnouncement, CancellationToken cancellationToken = default);
        Task<bool> SetAllowCommentsAsync(int id, bool allowComments, int adminMemberId, CancellationToken cancellationToken = default);
    }
}
