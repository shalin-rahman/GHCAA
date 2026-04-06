using GHCAA.Application.DTOs;
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

        [HttpGet("fees/applicable")]
        [AllowAnonymous]
        public async Task<IActionResult> GetApplicableFee([FromQuery] Domain.Enums.FinancialCategory category, [FromQuery] Domain.Enums.MembershipType type, [FromQuery] DateTime? date, CancellationToken cancellationToken)
        {
            var fee = await _financialService.GetApplicableFeeAsync(category, type, date ?? DateTime.UtcNow, cancellationToken);
            return Ok(new { Amount = fee });
        }

        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyPaymentHistory(CancellationToken cancellationToken)
        {
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId))
            {
                if (User.IsInRole("SuperAdmin"))
                    return Ok(new List<GHCAA.Application.DTOs.PaymentHistoryDto>());

                return BadRequest("Invalid user session");
            }

            var history = await _financialService.GetMemberPaymentHistoryAsync(memberId, cancellationToken);
            return Ok(history);
        }

        [HttpPost("record-payment")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RecordPayment([FromForm] CreatePaymentHistoryDto dto, CancellationToken cancellationToken)
        {
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            if (!string.IsNullOrEmpty(memberIdClaim) && int.TryParse(memberIdClaim, out var memberId))
            {
                dto.MemberId = memberId;
            }

            var result = await _financialService.RecordPaymentAsync(dto, cancellationToken);
            return Ok(result);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPatch("update-status/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] Domain.Enums.PaymentStatus status, [FromQuery] string? notes, CancellationToken cancellationToken)
        {
            var success = await _financialService.UpdatePaymentStatusAsync(id, status, notes, cancellationToken);
            return success ? Ok() : NotFound();
        }

        [HttpGet("receipt/{paymentId}")]
        public async Task<IActionResult> DownloadReceipt(int paymentId, CancellationToken cancellationToken)
        {
            // Security check: If not admin, verify ownership
            if (!User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                var memberIdClaim = User.FindFirst("MemberId")?.Value;
                if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId))
                {
                    return Unauthorized("Invalid session.");
                }

                var payment = await _db.PaymentHistories.FindAsync(new object[] { paymentId }, cancellationToken);
                if (payment == null) return NotFound();
                if (payment.MemberId != memberId) return Forbid("You can only download your own receipts.");
            }

            var pdfBytes = await _financialService.GenerateTaxReceiptAsync(paymentId, cancellationToken);
            return File(pdfBytes, "application/pdf", $"Receipt_{paymentId}.pdf");
        }

        [HttpGet("my-dues")]
        public async Task<IActionResult> GetMyDues(CancellationToken cancellationToken)
        {
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            int memberId;
            if (memberIdClaim == null || !int.TryParse(memberIdClaim, out memberId))
            {
                if (User.IsInRole("SuperAdmin"))
                    return Ok(new List<object>());

                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
                var user = await _db.Users.FindAsync(userId);
                memberId = user?.MemberId ?? 0;
            }

            var dues = await _financialService.GetMemberDuesAsync(memberId, cancellationToken);
            return Ok(dues);
        }

        [HttpPost("dues/generate")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GenerateAnnualDues([FromQuery] int year, CancellationToken cancellationToken)
        {
            await _financialService.GenerateAnnualDuesAsync(year, cancellationToken);
            return Ok(new { Message = $"Annual dues for {year} generated successfully." });
        }

        [HttpGet("fees/config")]
        [Authorize(Policy = "SuperAdminOnly")] // Strict role parity: Sync with frontend superAdminGuard
        public async Task<IActionResult> GetFeeConfigs(CancellationToken cancellationToken)
        {
            var configs = await _financialService.GetMembershipFeeConfigsAsync(cancellationToken);
            return Ok(configs);
        }

        [HttpPost("fees/config")]
        [Authorize(Policy = "SuperAdminOnly")] // Strict role parity: Sync with frontend superAdminGuard
        public async Task<IActionResult> AddFeeConfig([FromBody] CreateMembershipFeeConfigDto dto, CancellationToken cancellationToken)
        {
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var adminId))
            {
                return Unauthorized();
            }

            var result = await _financialService.AddMembershipFeeConfigAsync(dto, adminId, cancellationToken);
            return Ok(result);
        }

        [HttpPut("fees/config")]
        [Authorize(Policy = "SuperAdminOnly")] // Strict role parity: Sync with frontend superAdminGuard
        public async Task<IActionResult> UpdateFeeConfig([FromBody] UpdateMembershipFeeConfigDto dto, CancellationToken cancellationToken)
        {
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var adminId))
            {
                return Unauthorized();
            }

            var result = await _financialService.UpdateMembershipFeeConfigAsync(dto, adminId, cancellationToken);
            return Ok(result);
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

        [HttpDelete("payment/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeletePayment(int id, CancellationToken cancellationToken)
        {
            var success = await _financialService.DeletePaymentAsync(id, cancellationToken);
            return success ? Ok() : NotFound();
        }

        [HttpGet("member/{memberId}/history")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetMemberPaymentHistory(int memberId, CancellationToken cancellationToken)
        {
            var history = await _financialService.GetMemberPaymentHistoryAsync(memberId, cancellationToken);
            return Ok(history);
        }

        [HttpGet("saved-methods")]
        public async Task<IActionResult> GetSavedPaymentMethods(CancellationToken cancellationToken)
        {
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId)) return Unauthorized();

            var methods = await _financialService.GetSavedPaymentMethodsAsync(memberId, cancellationToken);
            return Ok(methods);
        }

        [HttpPost("saved-methods")]
        public async Task<IActionResult> AddSavedPaymentMethod([FromBody] CreateSavedPaymentMethodDto dto, CancellationToken cancellationToken)
        {
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId)) return Unauthorized();

            var result = await _financialService.AddSavedPaymentMethodAsync(memberId, dto, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("saved-methods/{id}")]
        public async Task<IActionResult> DeleteSavedPaymentMethod(int id, CancellationToken cancellationToken)
        {
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId)) return Unauthorized();

            var result = await _financialService.DeleteSavedPaymentMethodAsync(memberId, id, cancellationToken);
            return result ? Ok() : NotFound();
        }
    }
}
