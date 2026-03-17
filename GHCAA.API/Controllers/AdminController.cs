using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IMemberService _memberService;
        private readonly IIDCardService _idCardService;

        public AdminController(IMemberService memberService, IIDCardService idCardService)
        {
            _memberService = memberService;
            _idCardService = idCardService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
        {
            var stats = await _memberService.GetDashboardStatsAsync(cancellationToken);
            return Ok(stats);
        }

        [HttpGet("members")]
        public async Task<IActionResult> GetAllMembers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchQuery = "",
            [FromQuery] string statusFilter = "all",
            [FromQuery] bool includeArchived = false, 
            CancellationToken cancellationToken = default)
        {
            // Standard Admins cannot see archived records
            if (includeArchived && !User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            var result = await _memberService.GetAllMembersAsync(page, pageSize, searchQuery, statusFilter, includeArchived, cancellationToken);
            return Ok(result);
        }

        [HttpGet("members/{id}")]
        public async Task<IActionResult> GetMemberById(int id, CancellationToken cancellationToken)
        {
            var profile = await _memberService.GetProfileAsync(id, cancellationToken);
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
            try
            {
                var result = await _memberService.ApproveMemberAsync(id, dto.ApprovedByAdminId, cancellationToken);
                return Ok(new 
                { 
                    Message = "Member approved successfully", 
                    MembershipNumber = result.MembershipNumber,
                    DefaultPassword = result.DefaultPassword
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
            try
            {
                var success = await _memberService.RejectMemberAsync(id, dto.RejectedByAdminId, dto.Reason, cancellationToken);
                if (!success) return NotFound();
                return Ok(new { Message = "Application rejected and user notified." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("members/{id}")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> ArchiveMember(int id, CancellationToken cancellationToken)
        {
            var success = await _memberService.ArchiveMemberAsync(id, cancellationToken);
            if (!success) return NotFound();
            return Ok(new { Message = "Member archived successfully" });
        }

        [HttpPost("members/{id}/restore")]
        [Authorize(Policy = "SuperAdminOnly")]
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
            // Admins can update all membership information
            var success = await _memberService.AdminUpdateMemberAsync(id, dto, cancellationToken);
            if (!success) return NotFound();
            return Ok(new { Message = "Member updated by admin successfully" });
        }

        [HttpPost("members/{id}/photo")]
        public async Task<IActionResult> UpdateMemberPhoto(int id, IFormFile photo, CancellationToken cancellationToken)
        {
            if (photo == null || photo.Length == 0) return BadRequest(new { Message = "No file provided." });
            var dto = new UploadedFileDto { FileName = photo.FileName, Length = photo.Length, Content = photo.OpenReadStream() };
            var path = await _memberService.UpdateMemberPhotoAsync(id, dto, cancellationToken);
            return Ok(new { Message = "Photo updated.", PhotoPath = path });
        }

        [HttpPatch("members/{id}/documents")]
        public async Task<IActionResult> UpdateMemberDocuments(int id, IFormFile? certificate, IFormFile? paymentProof, CancellationToken cancellationToken)
        {
            UploadedFileDto? certFile = null;
            if (certificate != null)
            {
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

        [HttpPost("members/{id}/reset-password-admin")]
        public async Task<IActionResult> ResetPasswordAdmin(int id, CancellationToken cancellationToken)
        {
            try
            {
                var success = await _memberService.SendAdminPasswordResetLinkAsync(id, cancellationToken);
                if (!success) return NotFound(new { Message = "Member or user account not found. Please ensure the member is approved and active." });
                return Ok(new { Message = "Password reset link sent to the member's registered email." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("members/{id}/certificate")]
        public async Task<IActionResult> GetMemberCertificate(int id, CancellationToken cancellationToken)
        {
            var dataUri = await _idCardService.GenerateCertificateDataUriAsync(id, cancellationToken);
            return Ok(new { DataUri = dataUri });
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
    }
}
