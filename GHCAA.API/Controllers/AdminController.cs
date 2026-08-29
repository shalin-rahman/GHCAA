using GHCAA.API.Extensions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GHCAA.Domain;
using GHCAA.Application.Security;

namespace GHCAA.API.Controllers
{
    [Authorize(Policy = Constants.Policies.AdminOnly)]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IMemberService _memberService;
        private readonly IIDCardService _idCardService;
        private readonly IFileValidationService _fileValidationService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IMemberService memberService, IIDCardService idCardService, IFileValidationService fileValidationService, ILogger<AdminController> logger)
        {
            _memberService = memberService;
            _idCardService = idCardService;
            _fileValidationService = fileValidationService;
            _logger = logger;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
        {
            var isPrivileged = User.IsInRole("SuperAdmin");
            var stats = await _memberService.GetDashboardStatsAsync(isPrivileged, cancellationToken);
            return Ok(stats);
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics(CancellationToken cancellationToken)
        {
            var isPrivileged = User.IsInRole("SuperAdmin");
            var stats = await _memberService.GetDashboardStatsAsync(isPrivileged, cancellationToken);
            return Ok(stats);
        }

        [HttpPost("sync-members")]
        [GHCAA.API.Filters.RequireStepUp]
        public async Task<IActionResult> SyncMembers(CancellationToken cancellationToken)
        {
            var count = await _memberService.SyncAlumniAsync(cancellationToken);
            return Ok(new { Message = $"Successfully synchronized {count} alumni.", Count = count });
        }

        [HttpGet("members")]
        public async Task<IActionResult> GetAllMembers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchQuery = "",
            [FromQuery] string statusFilter = "all",
            [FromQuery] string categoryFilter = "all",
            [FromQuery] string membershipTypeFilter = "all",
            [FromQuery] bool includeArchived = false,
            CancellationToken cancellationToken = default)
        {
            // Standard Admins cannot see archived records
            if (includeArchived && !User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            var isPrivileged = User.IsInRole("SuperAdmin");
            var result = await _memberService.GetAllMembersAsync(page, pageSize, searchQuery, statusFilter, categoryFilter, membershipTypeFilter, includeArchived, isPrivileged, cancellationToken);
            return Ok(result);
        }

        [HttpGet("members/{id}")]
        public async Task<IActionResult> GetMemberById(int id, CancellationToken cancellationToken)
        {
            var isPrivileged = User.IsInRole("SuperAdmin");
            var profile = await _memberService.GetProfileAsync(id, isPrivileged: true, cancellationToken);
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        [HttpGet("members/{id}/documents")]
        public async Task<IActionResult> GetDocuments(int id, CancellationToken cancellationToken)
        {
            var docs = await _memberService.GetMemberDocumentsAsync(id, cancellationToken);
            if (docs == null) return NotFound();
            return Ok(docs);
        }

        [HttpPost("members/{id}/approve")]
        public async Task<IActionResult> ApproveMember(int id, [FromBody] ApproveMemberDto dto, CancellationToken cancellationToken)
        {
            // 24.51: Read admin identity from the JWT claim, not the request body.
            if (!int.TryParse(User.FindFirst(AppClaimTypes.MemberId)?.Value, out var adminMemberId))
                return Unauthorized();

            try
            {
                var result = await _memberService.ApproveMemberAsync(id, adminMemberId, cancellationToken);
                return Ok(new
                {
                    Message = "Member approved successfully. Login credentials have been emailed to the member.",
                    MembershipNumber = result.MembershipNumber,
                    PasswordEmailed = true
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("members/{id}/reject")]
        public async Task<IActionResult> RejectMember(int id, [FromBody] RejectMemberDto dto, CancellationToken cancellationToken)
        {
            // 24.51: Read admin identity from the JWT claim, not the request body.
            if (!int.TryParse(User.FindFirst(AppClaimTypes.MemberId)?.Value, out var adminMemberId))
                return Unauthorized();

            try
            {
                var success = await _memberService.RejectMemberAsync(id, adminMemberId, dto.Reason, cancellationToken);
                if (!success) return NotFound();
                return Ok(new { Message = "Application rejected and user notified." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("members/{id}")]
        [Authorize(Policy = Constants.Policies.SuperAdminOnly)]
        [GHCAA.API.Filters.RequireStepUp]
        public async Task<IActionResult> ArchiveMember(int id, CancellationToken cancellationToken)
        {
            var success = await _memberService.ArchiveMemberAsync(id, cancellationToken);
            if (!success) return NotFound();
            return Ok(new { Message = "Member archived successfully" });
        }

        [HttpPost("members/bulk-archive-inactive")]
        [Authorize(Policy = Constants.Policies.SuperAdminOnly)]
        [GHCAA.API.Filters.RequireStepUp]
        public async Task<IActionResult> BulkArchiveInactive(CancellationToken cancellationToken)
        {
            var count = await _memberService.BulkArchiveInactiveMembersAsync(cancellationToken);
            return Ok(new { Message = $"Successfully bulk-archived {count} inactive members.", ArchivedCount = count });
        }

        [HttpPost("members/{id}/restore")]
        [Authorize(Policy = Constants.Policies.SuperAdminOnly)]
        public async Task<IActionResult> RestoreMember(int id, CancellationToken cancellationToken)
        {
            var success = await _memberService.RestoreMemberAsync(id, cancellationToken);
            if (!success) return NotFound();
            return Ok(new { Message = "Member restored successfully" });
        }

        [HttpPost("members/{id}/reactivate")]
        public async Task<IActionResult> ReactivateMember(int id, CancellationToken cancellationToken)
        {
            var success = await _memberService.ReactivateMemberAsync(id, cancellationToken);
            if (!success) return NotFound();
            return Ok(new { Message = "Member reactivated to Active status successfully" });
        }

        [HttpPut("members/{id}")]
        public async Task<IActionResult> UpdateMemberAdmin(int id, [FromBody] AdminMemberUpdateDto dto, CancellationToken cancellationToken)
        {
            var adminIdClaim = User.FindFirst(AppClaimTypes.MemberId)?.Value;
            if (string.IsNullOrEmpty(adminIdClaim) || !int.TryParse(adminIdClaim, out var adminId))
            {
                return Unauthorized();
            }

            // Admins can update all membership information — except a fellow SuperAdmin's own
            // linked member record, otherwise a plain Admin could rewrite a SuperAdmin's email
            // to one they control and self-serve a password reset (see ResetPasswordAdmin below).
            try
            {
                var isPrivileged = User.IsInRole("SuperAdmin");
                var success = await _memberService.AdminUpdateMemberAsync(id, dto, adminId, isPrivileged, cancellationToken);
                if (!success) return NotFound();
                return Ok(new { Message = "Member updated by admin successfully" });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost("members/{id}/photo")]
        public async Task<IActionResult> UpdateMemberPhoto(int id, IFormFile photo, CancellationToken cancellationToken)
        {
            var validation = _fileValidationService.ValidateFormFile(photo, FileCategory.Image, 5 * 1024 * 1024);
            if (!validation.IsValid) return BadRequest(new { Message = validation.ErrorMessage });
            var dto = new UploadedFileDto { FileName = photo.FileName, Length = photo.Length, Content = photo.OpenReadStream() };
            var path = await _memberService.UpdateMemberPhotoAsync(id, dto, cancellationToken);
            return Ok(new { Message = "Photo updated.", PhotoPath = path });
        }

        [HttpPost("members/{id}/signature")]
        public async Task<IActionResult> UpdateMemberSignature(int id, IFormFile signature, CancellationToken cancellationToken)
        {
            var validation = _fileValidationService.ValidateFormFile(signature, FileCategory.Image, 2 * 1024 * 1024);
            if (!validation.IsValid) return BadRequest(new { Message = validation.ErrorMessage });
            var dto = new UploadedFileDto { FileName = signature.FileName, Length = signature.Length, Content = signature.OpenReadStream() };
            var path = await _memberService.UpdateMemberSignatureAsync(id, dto, cancellationToken);
            return Ok(new { Message = "Signature updated.", SignaturePath = path });
        }

        [HttpPatch("members/{id}/documents")]
        public async Task<IActionResult> UpdateMemberDocuments(int id, IFormFile? certificate, IFormFile? paymentProof, CancellationToken cancellationToken)
        {
            UploadedFileDto? certFile = null;
            if (certificate != null)
            {
                var certValidation = _fileValidationService.ValidateFormFile(certificate, FileCategory.Document, 10 * 1024 * 1024);
                if (!certValidation.IsValid) return BadRequest(new { Message = certValidation.ErrorMessage });

                certFile = new UploadedFileDto
                {
                    FileName = certificate.FileName,
                    Length = certificate.Length,
                    Content = certificate.OpenReadStream()
                };
            }

            UploadedFileDto? payFile = null;
            if (paymentProof != null)
            {
                var payValidation = _fileValidationService.ValidateFormFile(paymentProof, FileCategory.Document, 10 * 1024 * 1024);
                if (!payValidation.IsValid) return BadRequest(new { Message = payValidation.ErrorMessage });

                payFile = new UploadedFileDto
                {
                    FileName = paymentProof.FileName,
                    Length = paymentProof.Length,
                    Content = paymentProof.OpenReadStream()
                };
            }

            var success = await _memberService.UpdateMemberDocumentsAsync(id, certFile, payFile, cancellationToken);
            if (!success) return NotFound();
            return Ok(new { Message = "Documents updated successfully" });
        }

        [HttpGet("members/{id}/id-card")]
        public async Task<IActionResult> GetMemberIDCard(int id, CancellationToken cancellationToken)
        {
            var dataUri = await _idCardService.GenerateIDCardDataUriAsync(id, cancellationToken);
            return Ok(new { DataUri = dataUri });
        }

        [HttpGet("members/{id}/id-card/pdf")]
        public async Task<IActionResult> GetMemberIDCardPdf(int id, CancellationToken cancellationToken)
        {
            var pdfBytes = await _idCardService.GenerateIDCardPdfAsync(id, cancellationToken);
            return File(pdfBytes, "application/pdf", $"ID_Card_{id}.pdf");
        }

        [HttpPost("members/{id}/reset-password-admin")]
        [GHCAA.API.Filters.RequireStepUp]
        public async Task<IActionResult> ResetPasswordAdmin(int id, CancellationToken cancellationToken)
        {
            try
            {
                var isPrivileged = User.IsInRole("SuperAdmin");
                var result = await _memberService.SendAdminPasswordResetLinkAsync(id, isPrivileged, cancellationToken);
                if (!result.Success) return NotFound(new { Message = "Member or user account not found. Please ensure the member is approved and active." });
                return Ok(new
                {
                    Message = "Password reset link has been sent to the member's registered email address."
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin password reset failed for member {MemberId}", id);
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("members/{id}/certificate")]
        public async Task<IActionResult> GetMemberCertificate(int id, CancellationToken cancellationToken)
        {
            var dataUri = await _idCardService.GenerateCertificateDataUriAsync(id, cancellationToken);
            return Ok(new { DataUri = dataUri });
        }

        [HttpGet("members/{id}/certificate/pdf")]
        public async Task<IActionResult> GetMemberCertificatePdf(int id, CancellationToken cancellationToken)
        {
            var pdfBytes = await _idCardService.GenerateCertificatePdfAsync(id, cancellationToken);
            return File(pdfBytes, "application/pdf", $"Certificate_{id}.pdf");
        }

        [HttpGet("contact-messages")]
        public async Task<IActionResult> GetContactMessages([FromServices] IContactService contactService, CancellationToken cancellationToken)
        {
            var messages = await contactService.GetMessagesAsync(cancellationToken);
            return Ok(messages);
        }

        [HttpPost("contact-messages/{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int id, [FromServices] IContactService contactService, CancellationToken cancellationToken)
        {
            var success = await contactService.MarkAsReadAsync(id, cancellationToken);
            if (!success) return NotFound();
            return Ok(new { Message = "Message marked as read." });
        }

        [HttpDelete("contact-messages/{id}")]
        public async Task<IActionResult> DeleteContactMessage(int id, [FromServices] IContactService contactService, CancellationToken cancellationToken)
        {
            var success = await contactService.DeleteMessageAsync(id, cancellationToken);
            if (!success) return NotFound();
            return Ok(new { Message = "Message deleted." });
        }
    }
}
