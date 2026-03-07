using GHCAA.Application.DTOs;
using GHCAA.Domain;

namespace GHCAA.Application.Interfaces
{
    public interface IJobHubService
    {
        Task<IEnumerable<JobDto>> GetActiveJobsAsync(Enums.JobCategory? category = null, string? query = null, CancellationToken cancellationToken = default);
        Task<JobDto> PostJobAsync(CreateJobDto job, int memberId, CancellationToken cancellationToken = default);
        Task<JobDto?> GetJobByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> DeactivateJobAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> UpdateJobAsync(int id, CreateJobDto job, int memberId, bool isAdmin, CancellationToken cancellationToken = default);
        Task<IEnumerable<JobDto>> GetMemberJobsAsync(int memberId, CancellationToken cancellationToken = default);
    }
}
