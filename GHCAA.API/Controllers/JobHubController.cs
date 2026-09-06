using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using GHCAA.Application.Security;
using GHCAA.API.Extensions;

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
        [AllowAnonymous]
        [OutputCache(PolicyName = Constants.OutputCachePolicies.PublicContent)]
        public async Task<IActionResult> GetActiveJobs([FromQuery] Enums.JobCategory? jobCategory, [FromQuery] string? query, CancellationToken cancellationToken)
        {
            var jobs = await _jobService.GetActiveJobsAsync(jobCategory, query, cancellationToken);
            return Ok(jobs);
        }

        [HttpPost]
        public async Task<IActionResult> PostJob([FromBody] CreateJobDto job, CancellationToken cancellationToken)
        {
            var memberIdClaim = this.CurrentMemberIdRaw();
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId))
            {
                return Problem(detail: "Invalid user session", statusCode: StatusCodes.Status400BadRequest);
            }

            var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            var result = await _jobService.PostJobAsync(job, memberId, isAdmin, cancellationToken);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJob(int id, [FromBody] CreateJobDto job, CancellationToken cancellationToken)
        {
            var memberIdClaim = this.CurrentMemberIdRaw();
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId))
            {
                return Problem(detail: "Invalid user session", statusCode: StatusCodes.Status400BadRequest);
            }

            var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            var success = await _jobService.UpdateJobAsync(id, job, memberId, isAdmin, cancellationToken);

            if (!success) return Forbid();
            return Ok(new { Message = "Job updated successfully" });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [OutputCache(PolicyName = Constants.OutputCachePolicies.PublicContent)]
        public async Task<IActionResult> GetJob(int id, CancellationToken cancellationToken)
        {
            var job = await _jobService.GetJobByIdAsync(id, cancellationToken);
            if (job == null) return NotFound();
            return Ok(job);
        }

        [HttpDelete("{id}")]
        [HttpPatch("deactivate/{id}")]
        public async Task<IActionResult> DeactivateJob(int id, CancellationToken cancellationToken)
        {
            var memberIdClaim = this.CurrentMemberIdRaw();
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

        [HttpGet("admin/pending")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> GetPendingJobs(CancellationToken cancellationToken)
        {
            var jobs = await _jobService.GetPendingJobsAsync(cancellationToken);
            return Ok(jobs);
        }

        [HttpPost("admin/{id}/approve")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> ApproveJob(int id, [FromQuery] bool notifyMember = true, CancellationToken cancellationToken = default)
        {
            var success = await _jobService.ApproveJobAsync(id, notifyMember, cancellationToken);
            return success ? Ok(new { Message = "Job approved." }) : NotFound();
        }

        [HttpPost("admin/{id}/reject")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> RejectJob(int id, [FromBody] RejectJobRequest request, CancellationToken cancellationToken)
        {
            var success = await _jobService.RejectJobAsync(id, request.Reason, request.NotifyMember, cancellationToken);
            return success ? Ok(new { Message = "Job rejected." }) : NotFound();
        }

        public class RejectJobRequest
        {
            public string Reason { get; set; } = null!;
            public bool NotifyMember { get; set; } = true;
        }
    }
}
