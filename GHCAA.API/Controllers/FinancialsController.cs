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
        private readonly GHCAA.Infrastructure.Data.ApplicationDbContext _db;

        public FinancialsController(IFinancialService financialService, GHCAA.Infrastructure.Data.ApplicationDbContext db)
        {
            _financialService = financialService;
            _db = db;
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

        [HttpGet("my-dues")]
        public async Task<IActionResult> GetMyDues(CancellationToken cancellationToken)
        {
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            int memberId;
            if (memberIdClaim == null || !int.TryParse(memberIdClaim, out memberId)) // Corrected condition
            {
                // Fallback or handle null - usually if logged in, MemberId should be there
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
                var user = await _db.Users.FindAsync(userId);
                memberId = user?.MemberId ?? 0;
            }
            // else block is not needed as memberId is already assigned by out var or will be assigned in the if block
            // if (memberIdClaim != null && int.TryParse(memberIdClaim, out memberId)) { } // This is redundant

            var dues = await _financialService.GetMemberDuesAsync(memberId, cancellationToken);
            return Ok(dues);
        }

        [HttpPost("generate-dues")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GenerateDues([FromQuery] int year, CancellationToken cancellationToken)
        {
            await _financialService.GenerateAnnualDuesAsync(year, cancellationToken);
            return Ok(new { Message = $"Dues generation for year {year} initiated/completed." });
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
