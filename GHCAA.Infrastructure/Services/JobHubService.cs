using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
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

        public async Task<IEnumerable<JobDto>> GetActiveJobsAsync(Enums.JobCategory? category = null, CancellationToken cancellationToken = default)
        {
            var query = _db.JobOpportunities
                .Include(j => j.PostedBy)
                .Where(j => j.IsActive && (j.ExpiryDate == null || j.ExpiryDate > DateTime.UtcNow));

            if (category.HasValue)
                query = query.Where(j => j.Category == category.Value);

            var jobs = await query
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync(cancellationToken);

            return jobs.Select(MapToDto);
        }

        public async Task<JobDto> PostJobAsync(CreateJobDto dto, int memberId, CancellationToken cancellationToken = default)
        {
            var job = new JobOpportunity
            {
                Title = dto.Title,
                Company = dto.CompanyName,
                Location = dto.Location,
                Description = dto.Description,
                Requirements = dto.Requirements,
                ContactEmail = dto.ApplicationEmail ?? "",
                ApplicationLink = dto.ApplicationLink,
                Category = dto.Category,
                PostedByMemberId = memberId,
                PostedDate = DateTime.UtcNow,
                ExpiryDate = dto.ApplicationDeadline,
                IsActive = true
            };

            await _db.JobOpportunities.AddAsync(job, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // Reload to get member info if needed, or just map locally
            // Ideally we want the member name, which we might not have yet unless we include it
            var postedBy = await _db.Members.FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            job.PostedBy = postedBy;

            return MapToDto(job);
        }

        public async Task<JobDto?> GetJobByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var job = await _db.JobOpportunities
                .Include(j => j.PostedBy)
                .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
            
            return job == null ? null : MapToDto(job);
        }

        public async Task<bool> DeactivateJobAsync(int id, CancellationToken cancellationToken = default)
        {
            var job = await _db.JobOpportunities.FindAsync(new object[] { id }, cancellationToken);
            if (job == null) return false;

            job.IsActive = false;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IEnumerable<JobDto>> GetMemberJobsAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var jobs = await _db.JobOpportunities
                .Include(j => j.PostedBy)
                .Where(j => j.PostedByMemberId == memberId)
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync(cancellationToken);

            return jobs.Select(MapToDto);
        }

        private static JobDto MapToDto(JobOpportunity job)
        {
            return new JobDto
            {
                Id = job.Id,
                Title = job.Title,
                CompanyName = job.Company,
                Location = job.Location,
                Description = job.Description,
                Requirements = job.Requirements,
                ApplicationEmail = job.ContactEmail,
                ApplicationLink = job.ApplicationLink,
                PostedDate = job.PostedDate,
                ApplicationDeadline = job.ExpiryDate ?? DateTime.MaxValue,
                Category = job.Category,
                IsActive = job.IsActive,
                PostedByMemberId = job.PostedByMemberId,
                PostedByMemberName = job.PostedBy?.FullName
            };
        }
    }
}
