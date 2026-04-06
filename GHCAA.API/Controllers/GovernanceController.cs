using System;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/governance")]
    [Authorize]
    public class GovernanceController : ControllerBase
    {
        private readonly IGovernanceService _governanceService;
        private readonly GHCAA.Infrastructure.Data.ApplicationDbContext _db;

        public GovernanceController(IGovernanceService governanceService, GHCAA.Infrastructure.Data.ApplicationDbContext db)
        {
            _governanceService = governanceService;
            _db = db;
        }

        [HttpGet("ec/current")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCurrentEC(CancellationToken cancellationToken)
        {
            var period = await _governanceService.GetActivePeriodAsync(cancellationToken);
            if (period == null) return NotFound("No active EC period found.");
            
            var members = await _governanceService.GetCommitteeMembersAsync(period.Id, cancellationToken);
            return Ok(new { Period = period, Members = members });
        }

        [HttpGet("ec/history")]
        [AllowAnonymous]
        public async Task<IActionResult> GetECHistory(CancellationToken cancellationToken)
        {
            var periods = await _governanceService.GetAllPeriodsAsync(cancellationToken);
            return Ok(periods);
        }

        [HttpGet("constitution")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCurrentConstitution(CancellationToken cancellationToken)
        {
            var constitution = await _governanceService.GetActiveConstitutionAsync(cancellationToken);
            return constitution == null ? NotFound() : Ok(constitution);
        }

        [HttpGet("constitution/history")]
        [AllowAnonymous]
        public async Task<IActionResult> GetConstitutionHistory(CancellationToken cancellationToken)
        {
            var history = await _governanceService.GetConstitutionHistoryAsync(cancellationToken);
            return Ok(history);
        }

        [HttpPost("constitution/{id:int}/vote")]
        public async Task<IActionResult> VoteOnAmendment(int id, [FromBody] bool isFor, [FromQuery] string? comments, CancellationToken cancellationToken)
        {
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId))
                return Unauthorized();

            var success = await _governanceService.VoteOnConstitutionAsync(id, memberId, isFor, comments, cancellationToken);
            return success ? Ok(new { Message = "Vote recorded." }) : BadRequest("Could not record vote. Ensure the version is active and you haven't voted yet.");
        }
    }
}
