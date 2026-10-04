using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GHCAA.Domain;
using GHCAA.API.Extensions;
using static GHCAA.Domain.Enums;

namespace GHCAA.API.Controllers
{
    [Authorize(Policy = Constants.Policies.AdminOnly)]
    [ApiController]
    [Route("api/admin/governance")]
    public class AdminGovernanceController : ControllerBase
    {
        private readonly IGovernanceService _governanceService;

        public AdminGovernanceController(IGovernanceService governanceService)
        {
            _governanceService = governanceService;
        }

        [HttpGet("periods")]
        public async Task<IActionResult> GetPeriods(CancellationToken cancellationToken)
        {
            var periods = await _governanceService.GetAllPeriodsAsync(cancellationToken);
            return Ok(periods);
        }

        [HttpPost("periods")]
        public async Task<IActionResult> CreatePeriod([FromBody] CreatePeriodRequest request, CancellationToken cancellationToken)
        {
            var period = await _governanceService.CreatePeriodAsync(request.Title, request.StartDate, request.EndDate, cancellationToken);
            return Ok(period);
        }

        [HttpPut("periods/{id}")]
        public async Task<IActionResult> UpdatePeriod(int id, [FromBody] UpdatePeriodRequest request, CancellationToken cancellationToken)
        {
            var success = await _governanceService.UpdatePeriodAsync(id, request.Title, request.StartDate, request.EndDate, request.IsActive, cancellationToken);
            if (!success) return NotFound();
            return Ok(new { Message = "Period updated successfully" });
        }

        [HttpPost("periods/{id}/activate")]
        public async Task<IActionResult> ActivatePeriod(int id, CancellationToken cancellationToken)
        {
            var success = await _governanceService.ActivatePeriodAsync(id, cancellationToken);
            if (!success) return NotFound();
            return Ok(new { Message = "Period activated successfully" });
        }

        [HttpGet("periods/{id}/members")]
        public async Task<IActionResult> GetCommitteeMembers(int id, CancellationToken cancellationToken)
        {
            var members = await _governanceService.GetCommitteeMembersAsync(id, cancellationToken);
            return Ok(members);
        }

        [HttpGet("periods/{id}/seats")]
        public async Task<IActionResult> GetCommitteeSeats(int id, CancellationToken cancellationToken)
        {
            var seats = await _governanceService.GetCommitteeSeatsAsync(id, cancellationToken);
            return Ok(seats);
        }

        [HttpPost("periods/{id}/members")]
        public async Task<IActionResult> AssignMember(int id, [FromBody] AssignMemberRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var success = await _governanceService.AssignMemberToRoleAsync(id, request.MemberId, request.Position, request.Reason, request.NotifyMember,
                    request.EndCurrentHolderReason, request.EndCurrentHolderNote, cancellationToken);
                if (!success) return Problem(detail: "Assignment failed", statusCode: StatusCodes.Status400BadRequest);
                return Ok(new { Message = "Member assigned to role successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
            }
        }

        // 95.3: replaces DELETE members/{ecMemberId}, which ended a term without saying why.
        [HttpPost("members/{ecMemberId}/end-term")]
        public async Task<IActionResult> EndTerm(int ecMemberId, [FromBody] EndTermRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var success = await _governanceService.RemoveMemberFromCommitteeAsync(ecMemberId, request.Reason!.Value, request.Note, request.NotifyMember, cancellationToken);
                if (!success) return NotFound();
                return Ok(new { Message = "Committee term ended" });
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
            }
        }

        [HttpDelete("members/{ecMemberId}/hard-delete")]
        [GHCAA.API.Filters.RequireStepUp]
        public async Task<IActionResult> DeleteECMember(int ecMemberId, [FromQuery] bool notifyMember, CancellationToken cancellationToken)
        {
            // 82.29: a deleted ECMember records who deleted it, so refuse rather than attribute it
            // to admin 0 when the caller cannot be identified.
            if (!int.TryParse(this.CurrentUserIdRaw(), out var adminId))
                return Unauthorized();

            var success = await _governanceService.DeleteECMemberAsync(ecMemberId, adminId, notifyMember, cancellationToken);
            if (!success) return NotFound();
            return Ok(new { Message = "Member role history permanently deleted" });
        }
    }

    public class CreatePeriodRequest
    {
        public string Title { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class UpdatePeriodRequest
    {
        public string Title { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class AssignMemberRequest
    {
        public int MemberId { get; set; }
        public int Position { get; set; }
        public string? Reason { get; set; }
        public bool NotifyMember { get; set; } = false;
        // Set only to replace whoever holds the seat now.
        public VacancyReason? EndCurrentHolderReason { get; set; }
        [MaxLength(Constants.Governance.VacancyNoteMaxLength)]
        public string? EndCurrentHolderNote { get; set; }
    }

    public class EndTermRequest
    {
        [Required]
        public VacancyReason? Reason { get; set; }
        [MaxLength(Constants.Governance.VacancyNoteMaxLength)]
        public string? Note { get; set; }
        public bool NotifyMember { get; set; }
    }
}
