using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    public interface IPollService
    {
        Task<List<PollDto>> GetActivePollsAsync(int memberId, CancellationToken cancellationToken = default);
        Task<List<PollDto>> GetAllPollsAsync(CancellationToken cancellationToken = default);
        Task<PollDto?> GetPollByIdAsync(int id, int memberId, CancellationToken cancellationToken = default);
        Task<int> CreatePollAsync(CreatePollDto dto, int adminMemberId, CancellationToken cancellationToken = default);
        Task<bool> VoteAsync(int pollId, int memberId, List<int> optionIds, CancellationToken cancellationToken = default);
        Task<bool> TogglePollStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default);
        Task<bool> DeletePollAsync(int id, CancellationToken cancellationToken = default);
    }
}
