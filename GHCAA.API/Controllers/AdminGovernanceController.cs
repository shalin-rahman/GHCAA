using GHCAA.API.Extensions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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
            try
            {
                var period = await _governanceService.CreatePeriodAsync(request.Title, request.StartDate, request.EndDate, cancellationToken);
                return Ok(period);
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
            }
        }

        [HttpPut("periods/{id}")]
        public async Task<IActionResult> UpdatePeriod(int id, [FromBody] UpdatePeriodRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var success = await _governanceService.UpdatePeriodAsync(id, request.Title, request.StartDate, request.EndDate, request.IsActive, cancellationToken);
                if (!success) return NotFound();
                return Ok(new { Message = "Period updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
            }
        }

        [HttpPost("periods/{id}/activate")]
        public async Task<IActionResult> ActivatePeriod(int id, CancellationToken cancellationToken)
        {
            try
            {
                var success = await _governanceService.ActivatePeriodAsync(id, cancellationToken);
                if (!success) return NotFound();
                return Ok(new { Message = "Period activated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
            }
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

        // 95.2: committee positions.
        [HttpGet("positions")]
        public async Task<IActionResult> GetPositions([FromQuery] bool includeRetired = true, CancellationToken cancellationToken = default)
            => Ok(await _governanceService.GetPositionsAsync(includeRetired, cancellationToken));

        [HttpPost("positions")]
        public async Task<IActionResult> CreatePosition([FromBody] SaveEcPositionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await _governanceService.CreatePositionAsync(request, cancellationToken));
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
            }
        }

        [HttpPut("positions/{id:int}")]
        public async Task<IActionResult> UpdatePosition(int id, [FromBody] SaveEcPositionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var position = await _governanceService.UpdatePositionAsync(id, request, cancellationToken);
                return position == null ? NotFound() : Ok(position);
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
            }
        }

        [HttpPost("positions/{id:int}/retire")]
        public Task<IActionResult> RetirePosition(int id, CancellationToken cancellationToken) => SetPositionActive(id, false, cancellationToken);

        [HttpPost("positions/{id:int}/restore")]
        public Task<IActionResult> RestorePosition(int id, CancellationToken cancellationToken) => SetPositionActive(id, true, cancellationToken);

        [HttpDelete("positions/{id:int}")]
        public async Task<IActionResult> DeletePosition(int id, CancellationToken cancellationToken)
        {
            try
            {
                return await _governanceService.DeletePositionAsync(id, cancellationToken) ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
            }
        }

        [HttpGet("positions/invalid-rows")]
        public async Task<IActionResult> GetInvalidPositionRows(CancellationToken cancellationToken)
            => Ok(await _governanceService.GetInvalidPositionRowsAsync(cancellationToken));

        [HttpPost("positions/repair")]
        public async Task<IActionResult> RepairPosition([FromBody] RepairPositionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await _governanceService.RepairPositionAsync(request, cancellationToken) ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
            }
        }

        private async Task<IActionResult> SetPositionActive(int id, bool isActive, CancellationToken cancellationToken)
        {
            try
            {
                return await _governanceService.SetPositionActiveAsync(id, isActive, cancellationToken) ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
            }
        }

        // 37.14m: ER-34 handover checklist for a committee term.
        [HttpGet("periods/{id:int}/handover-checks")]
        public async Task<IActionResult> GetHandoverChecks(int id, CancellationToken cancellationToken)
            => await _governanceService.GetHandoverChecksAsync(id, cancellationToken) is { } checks ? Ok(checks) : NotFound();

        [HttpPut("periods/{id:int}/handover-checks")]
        public async Task<IActionResult> SetHandoverCheck(int id, [FromBody] SetHandoverCheckRequest request, CancellationToken cancellationToken)
        {
            if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
                return Unauthorized();
            try
            {
                if (!await _governanceService.SetHandoverCheckAsync(id, request, userId, cancellationToken)) return NotFound();
            }
            catch (InvalidRequestException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
            }
            return await _governanceService.GetHandoverChecksAsync(id, cancellationToken) is { } checks ? Ok(checks) : NotFound();
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
