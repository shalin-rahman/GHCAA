using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Domain;
using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface IJobHubService
    {
        Task<IEnumerable<JobOpportunity>> GetActiveJobsAsync(Enums.JobCategory? category = null, CancellationToken cancellationToken = default);
        Task<JobOpportunity> PostJobAsync(JobOpportunity job, CancellationToken cancellationToken = default);
        Task<JobOpportunity?> GetJobByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> UpdateJobAsync(JobOpportunity job, CancellationToken cancellationToken = default);
        Task<bool> DeactivateJobAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<JobOpportunity>> GetMemberJobsAsync(int memberId, CancellationToken cancellationToken = default);
    }
}
