using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    [Authorize]
    public class JobHubController : ControllerBase
    {
        private readonly IJobHubService _jobService;

        public JobHubController(IJobHubService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveJobs([FromQuery] Enums.JobCategory? category, CancellationToken cancellationToken)
        {
            var jobs = await _jobService.GetActiveJobsAsync(category, cancellationToken);
            return Ok(jobs);
        }

        [HttpPost]
        public async Task<IActionResult> PostJob([FromBody] CreateJobDto job, CancellationToken cancellationToken)
        {
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId))
            {
                return BadRequest("Invalid user session");
            }

            var result = await _jobService.PostJobAsync(job, memberId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetJob(int id, CancellationToken cancellationToken)
        {
            var job = await _jobService.GetJobByIdAsync(id, cancellationToken);
            if (job == null) return NotFound();
            return Ok(job);
        }

        [HttpPatch("deactivate/{id}")]
        public async Task<IActionResult> DeactivateJob(int id, CancellationToken cancellationToken)
        {
            // Verify ownership or Admin status
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            
            var job = await _jobService.GetJobByIdAsync(id, cancellationToken);
            if (job == null) return NotFound();

            if (!isAdmin && job.PostedByMemberId.ToString() != memberIdClaim)
            {
                return Forbid();
            }

            var success = await _jobService.DeactivateJobAsync(id, cancellationToken);
            return success ? Ok() : StatusCode(500);
        }
    }
}
