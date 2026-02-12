using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    public class JobHubService : IJobHubService
    {
        private readonly ApplicationDbContext _db;

        public JobHubService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<JobOpportunity>> GetActiveJobsAsync(Enums.JobCategory? category = null, CancellationToken cancellationToken = default)
        {
            var query = _db.JobOpportunities
                .Where(j => j.IsActive && (j.ExpiryDate == null || j.ExpiryDate > DateTime.UtcNow));

            if (category.HasValue)
                query = query.Where(j => j.Category == category.Value);

            return await query.OrderByDescending(j => j.PostedDate).ToListAsync(cancellationToken);
        }

        public async Task<JobOpportunity> PostJobAsync(JobOpportunity job, CancellationToken cancellationToken = default)
        {
            job.PostedDate = DateTime.UtcNow;
            await _db.JobOpportunities.AddAsync(job, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return job;
        }

        public async Task<JobOpportunity?> GetJobByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.JobOpportunities.Include(j => j.PostedBy).FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
        }

        public async Task<bool> UpdateJobAsync(JobOpportunity job, CancellationToken cancellationToken = default)
        {
            var existing = await _db.JobOpportunities.FindAsync(new object[] { job.Id }, cancellationToken);
            if (existing == null) return false;

            existing.Title = job.Title;
            existing.Company = job.Company;
            existing.Location = job.Location;
            existing.Description = job.Description;
            existing.Requirements = job.Requirements;
            existing.ContactEmail = job.ContactEmail;
            existing.ExpiryDate = job.ExpiryDate;
            existing.IsActive = job.IsActive;
            existing.Category = job.Category;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeactivateJobAsync(int id, CancellationToken cancellationToken = default)
        {
            var job = await _db.JobOpportunities.FindAsync(new object[] { id }, cancellationToken);
            if (job == null) return false;

            job.IsActive = false;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IEnumerable<JobOpportunity>> GetMemberJobsAsync(int memberId, CancellationToken cancellationToken = default)
        {
            return await _db.JobOpportunities
                .Where(j => j.PostedByMemberId == memberId)
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync(cancellationToken);
        }
    }
}
