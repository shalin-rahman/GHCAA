using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/financials")]
    [Authorize]
    public class FinancialsController : ControllerBase
    {
        private readonly IFinancialService _financialService;

        public FinancialsController(IFinancialService financialService)
        {
            _financialService = financialService;
        }

        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyPaymentHistory(CancellationToken cancellationToken)
        {
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId))
            {
                return BadRequest("Invalid user session");
            }

            var history = await _financialService.GetMemberPaymentHistoryAsync(memberId, cancellationToken);
            return Ok(history);
        }

        [HttpPost("record-payment")]
        public async Task<IActionResult> RecordPayment([FromBody] PaymentHistory payment, CancellationToken cancellationToken)
        {
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            if (!string.IsNullOrEmpty(memberIdClaim) && int.TryParse(memberIdClaim, out var memberId))
            {
                payment.MemberId = memberId;
            }

            var result = await _financialService.RecordPaymentAsync(payment, cancellationToken);
            return Ok(result);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPatch("update-status/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] Domain.Enums.PaymentStatus status, [FromQuery] string? notes, CancellationToken cancellationToken)
        {
            var success = await _financialService.UpdatePaymentStatusAsync(id, status, notes, cancellationToken);
            return success ? Ok() : NotFound();
        }

        [HttpGet("membership-history/{memberId}")]
        public async Task<IActionResult> GetMembershipHistory(int memberId, CancellationToken cancellationToken)
        {
            // If not admin, can only see own history
            if (!User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                var myMemberId = User.FindFirst("MemberId")?.Value;
                if (myMemberId != memberId.ToString()) return Forbid();
            }

            var history = await _financialService.GetMemberMembershipHistoryAsync(memberId, cancellationToken);
            return Ok(history);
        }
    }
}
