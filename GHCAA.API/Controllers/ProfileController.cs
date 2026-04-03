using System.Security.Claims;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        public ProfileController(IMemberService memberService, IUserService userService, IIDCardService idCardService)
        {
            _memberService = memberService;
            _userService = userService;
            _idCardService = idCardService;
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
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var success = await _userService.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword, cancellationToken);
            if (!success)
            {
                return BadRequest(new { Message = "Password change failed. Verify your old password." });
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

        [HttpGet("certificate")]
        public async Task<IActionResult> GetCertificate(CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();

            var dataUri = await _idCardService.GenerateCertificateDataUriAsync(memberId, cancellationToken);
            return Ok(new { DataUri = dataUri });
        }

        [HttpPost("photo")]
        public async Task<IActionResult> UploadPhoto(IFormFile photo, CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();
            if (photo == null || photo.Length == 0) return BadRequest(new { Message = "No file provided." });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(photo.ContentType.ToLower()))
                return BadRequest(new { Message = "Only JPG, PNG, or WebP images are allowed." });

            if (photo.Length > 5 * 1024 * 1024)
                return BadRequest(new { Message = "Photo must be under 5MB." });

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
            if (signature == null || signature.Length == 0) return BadRequest(new { Message = "No file provided." });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(signature.ContentType.ToLower()))
                return BadRequest(new { Message = "Only JPG, PNG, or WebP images are allowed." });

            if (signature.Length > 2 * 1024 * 1024)
                return BadRequest(new { Message = "Signature must be under 2MB." });

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
            var memberIdStr = User.FindFirst("MemberId")?.Value;
            if (int.TryParse(memberIdStr, out var memberId))
            {
                return memberId;
            }
            return 0;
        }
    }
}
