using System.Security.Claims;
using GHCAA.API.Extensions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GHCAA.Application.Security;

namespace GHCAA.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IMemberService _memberService;
        private readonly IUserService _userService;
        private readonly IIDCardService _idCardService;
        private readonly IFileValidationService _fileValidationService;

        public ProfileController(IMemberService memberService, IUserService userService, IIDCardService idCardService, IFileValidationService fileValidationService)
        {
            _memberService = memberService;
            _userService = userService;
            _idCardService = idCardService;
            _fileValidationService = fileValidationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();

            var profile = await _memberService.GetProfileAsync(memberId, isPrivileged: true, cancellationToken);
            if (profile == null) return NotFound();

            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto, CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();

            var success = await _memberService.UpdateProfileAsync(memberId, dto, cancellationToken);
            if (!success) return NotFound();

            return Ok(new { Message = "Profile updated successfully" });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken cancellationToken)
        {
            var userIdStr = this.CurrentUserIdRaw();
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var success = await _userService.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword, cancellationToken);
            if (!success)
            {
                return Problem(detail: "Password change failed. Verify your old password.", statusCode: StatusCodes.Status400BadRequest);
            }

            return Ok(new { Message = "Password changed successfully" });
        }

        [HttpGet("id-card")]
        public async Task<IActionResult> GetIDCard(CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();

            var dataUri = await _idCardService.GenerateIDCardDataUriAsync(memberId, cancellationToken);
            return Ok(new { DataUri = dataUri });
        }

        [HttpGet("id-card/pdf")]
        public async Task<IActionResult> GetIDCardPdf(CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();

            var pdfBytes = await _idCardService.GenerateIDCardPdfAsync(memberId, cancellationToken);
            return File(pdfBytes, "application/pdf", $"ID_Card_{memberId}.pdf");
        }

        [HttpGet("certificate")]
        public async Task<IActionResult> GetCertificate(CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();

            var dataUri = await _idCardService.GenerateCertificateDataUriAsync(memberId, cancellationToken);
            return Ok(new { DataUri = dataUri });
        }

        [HttpGet("certificate/pdf")]
        public async Task<IActionResult> GetCertificatePdf(CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();

            var pdfBytes = await _idCardService.GenerateCertificatePdfAsync(memberId, cancellationToken);
            return File(pdfBytes, "application/pdf", $"Certificate_{memberId}.pdf");
        }

        [HttpPost("photo")]
        public async Task<IActionResult> UploadPhoto(IFormFile photo, CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();
            var validation = _fileValidationService.ValidateFormFile(photo, FileCategory.Image, 5 * 1024 * 1024);
            if (!validation.IsValid) return Problem(detail: validation.ErrorMessage, statusCode: StatusCodes.Status400BadRequest);

            var dto = new UploadedFileDto
            {
                FileName = photo.FileName,
                Length = photo.Length,
                Content = photo.OpenReadStream()
            };

            var photoPath = await _memberService.UpdateMemberPhotoAsync(memberId, dto, cancellationToken);
            return Ok(new { Message = "Photo updated successfully.", PhotoPath = photoPath });
        }

        [HttpPost("signature")]
        public async Task<IActionResult> UploadSignature(IFormFile signature, CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();
            var validation = _fileValidationService.ValidateFormFile(signature, FileCategory.Image, 2 * 1024 * 1024);
            if (!validation.IsValid) return Problem(detail: validation.ErrorMessage, statusCode: StatusCodes.Status400BadRequest);

            var dto = new UploadedFileDto
            {
                FileName = signature.FileName,
                Length = signature.Length,
                Content = signature.OpenReadStream()
            };

            var signaturePath = await _memberService.UpdateMemberSignatureAsync(memberId, dto, cancellationToken);
            return Ok(new { Message = "Signature updated successfully.", SignaturePath = signaturePath });
        }

        private int GetMemberId()
        {
            var memberIdStr = this.CurrentMemberIdRaw();
            if (int.TryParse(memberIdStr, out var memberId))
            {
                return memberId;
            }
            return 0;
        }
    }
}
