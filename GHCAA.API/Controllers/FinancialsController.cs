using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.API.Extensions;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GHCAA.Domain;
using GHCAA.Application.Security;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/financials")]
    [Authorize]
    public class FinancialsController : ControllerBase
    {
        private readonly IFinancialService _financialService;
        private readonly IFileValidationService _fileValidationService;

        public FinancialsController(IFinancialService financialService, IFileValidationService fileValidationService)
        {
            _financialService = financialService;
            _fileValidationService = fileValidationService;
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
            var memberIdClaim = this.CurrentMemberIdRaw();
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId))
            {
                if (User.IsInRole("SuperAdmin"))
                    return Ok(new List<GHCAA.Application.DTOs.PaymentHistoryDto>());

                return Problem(detail: "Invalid user session", statusCode: StatusCodes.Status400BadRequest);
            }

            var history = await _financialService.GetMemberPaymentHistoryAsync(memberId, cancellationToken);
            return Ok(history);
        }

        [HttpPost("record-payment")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RecordPayment([FromForm] CreatePaymentHistoryDto dto, CancellationToken cancellationToken)
        {
            // 82.32: was silently keeping the client-supplied dto.MemberId when the MemberId claim
            // was absent, so any authenticated principal without that claim (a system-admin token)
            // could attribute a payment, and its receipt, to an arbitrary member id of its choosing.
            // This endpoint is member self-service only (mobile derives MemberId server-side for the
            // same reason) — every sibling endpoint in this controller already refuses rather than
            // trusts the body in this situation.
            var memberIdClaim = this.CurrentMemberIdRaw();
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId))
            {
                return Unauthorized();
            }
            dto.MemberId = memberId;

            // 29B.7: Content-validate the uploaded receipt (magic-byte check) so a renamed
            // executable/script can't be stored under a .jpg/.pdf name in the secure tree.
            if (dto.Receipt != null)
            {
                var receiptValidation = _fileValidationService.ValidateFormFile(dto.Receipt, FileCategory.Document, 10 * 1024 * 1024);
                if (!receiptValidation.IsValid) return Problem(detail: receiptValidation.ErrorMessage, statusCode: StatusCodes.Status400BadRequest);
            }

            var result = await _financialService.RecordPaymentAsync(dto, cancellationToken);
            return Ok(result);
        }

        [Authorize(Policy = Constants.Policies.AdminOnly)]
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
                var memberIdClaim = this.CurrentMemberIdRaw();
                if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId))
                {
                    return Unauthorized("Invalid session.");
                }

                var ownerMemberId = await _financialService.GetPaymentOwnerMemberIdAsync(paymentId, cancellationToken);
                if (ownerMemberId == null) return NotFound();
                if (ownerMemberId != memberId) return Forbid("You can only download your own receipts.");
            }

            var pdfBytes = await _financialService.GenerateTaxReceiptAsync(paymentId, cancellationToken);
            return File(pdfBytes, "application/pdf", $"Receipt_{paymentId}.pdf");
        }

        [HttpGet("my-dues")]
        public async Task<IActionResult> GetMyDues(CancellationToken cancellationToken)
        {
            var memberIdClaim = this.CurrentMemberIdRaw();
            int memberId;
            if (memberIdClaim == null || !int.TryParse(memberIdClaim, out memberId))
            {
                if (User.IsInRole("SuperAdmin"))
                    return Ok(new List<object>());

                // 82.32: was `int.Parse(...NameIdentifier)!.Value`, which crashed with a 500 for
                // any caller reaching this branch without a parseable nameid claim — the exact
                // shape every sibling endpoint in this controller instead answers with
                // Unauthorized/BadRequest. This is the system-admin branch (no MemberId claim), so
                // it is reached routinely, not only on a malformed token.
                var userIdClaim = this.CurrentUserIdRaw();
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                    return Unauthorized("Invalid session.");

                memberId = await _financialService.GetMemberIdForUserAsync(userId, cancellationToken) ?? 0;
            }

            var dues = await _financialService.GetMemberDuesAsync(memberId, cancellationToken);
            return Ok(dues);
        }

        [HttpPost("dues/generate")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> GenerateAnnualDues([FromQuery] int year, CancellationToken cancellationToken)
        {
            await _financialService.GenerateAnnualDuesAsync(year, cancellationToken);
            return Ok(new { Message = $"Annual dues for {year} generated successfully." });
        }

        [HttpGet("fees/config")]
        [Authorize(Policy = Constants.Policies.SuperAdminOnly)] // Strict role parity: Sync with frontend superAdminGuard
        public async Task<IActionResult> GetFeeConfigs(CancellationToken cancellationToken)
        {
            var configs = await _financialService.GetMembershipFeeConfigsAsync(cancellationToken);
            return Ok(configs);
        }

        [HttpPost("fees/config")]
        [Authorize(Policy = Constants.Policies.SuperAdminOnly)] // Strict role parity: Sync with frontend superAdminGuard
        public async Task<IActionResult> AddFeeConfig([FromBody] CreateMembershipFeeConfigDto dto, CancellationToken cancellationToken)
        {
            var memberIdClaim = this.CurrentMemberIdRaw();
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var adminId))
            {
                return Unauthorized();
            }

            var result = await _financialService.AddMembershipFeeConfigAsync(dto, adminId, cancellationToken);
            return Ok(result);
        }

        [HttpPut("fees/config")]
        [Authorize(Policy = Constants.Policies.SuperAdminOnly)] // Strict role parity: Sync with frontend superAdminGuard
        public async Task<IActionResult> UpdateFeeConfig([FromBody] UpdateMembershipFeeConfigDto dto, CancellationToken cancellationToken)
        {
            var memberIdClaim = this.CurrentMemberIdRaw();
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
                var myMemberId = this.CurrentMemberIdRaw();
                if (myMemberId != memberId.ToString()) return Forbid();
            }

            var history = await _financialService.GetMemberMembershipHistoryAsync(memberId, cancellationToken);
            return Ok(history);
        }

        [HttpDelete("payment/{id}")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> DeletePayment(int id, CancellationToken cancellationToken)
        {
            // 82.16: a deleted payment records who deleted it, so refuse rather than attribute it
            // to admin 0 when the caller cannot be identified.
            if (!int.TryParse(this.CurrentUserIdRaw(), out var adminId))
                return Unauthorized();

            var success = await _financialService.DeletePaymentAsync(id, adminId, cancellationToken);
            return success ? Ok() : NotFound();
        }

        [HttpGet("member/{memberId}/history")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> GetMemberPaymentHistory(int memberId, CancellationToken cancellationToken)
        {
            var history = await _financialService.GetMemberPaymentHistoryAsync(memberId, cancellationToken);
            return Ok(history);
        }

        [HttpGet("saved-methods")]
        public async Task<IActionResult> GetSavedPaymentMethods(CancellationToken cancellationToken)
        {
            var memberIdClaim = this.CurrentMemberIdRaw();
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId)) return Unauthorized();

            var methods = await _financialService.GetSavedPaymentMethodsAsync(memberId, cancellationToken);
            return Ok(methods);
        }

        [HttpPost("saved-methods")]
        public async Task<IActionResult> AddSavedPaymentMethod([FromBody] CreateSavedPaymentMethodDto dto, CancellationToken cancellationToken)
        {
            var memberIdClaim = this.CurrentMemberIdRaw();
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId)) return Unauthorized();

            var result = await _financialService.AddSavedPaymentMethodAsync(memberId, dto, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("saved-methods/{id}")]
        public async Task<IActionResult> DeleteSavedPaymentMethod(int id, CancellationToken cancellationToken)
        {
            var memberIdClaim = this.CurrentMemberIdRaw();
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId)) return Unauthorized();

            var result = await _financialService.DeleteSavedPaymentMethodAsync(memberId, id, cancellationToken);
            return result ? Ok() : NotFound();
        }
    }
}
