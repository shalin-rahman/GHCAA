using GHCAA.API.Extensions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;

using Microsoft.AspNetCore.RateLimiting;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("registration")]
    public class RegistrationController : ControllerBase
    {
        private readonly IMemberService _memberService;
        private readonly IFileValidationService _fileValidationService;
        public RegistrationController(IMemberService memberService, IFileValidationService fileValidationService)
        {
            _memberService = memberService;
            _fileValidationService = fileValidationService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [RequestSizeLimit(10_485_760)]
        public async Task<IActionResult> Register([FromForm] MemberRegistrationDto dto,
            IFormFile? photo, IFormFile? certificate, IFormFile? paymentProof, CancellationToken cancellationToken = default)
        {
            if (photo != null)
            {
                var photoValidation = _fileValidationService.ValidateFormFile(photo, FileCategory.Image, 5 * 1024 * 1024);
                if (!photoValidation.IsValid) return BadRequest(new { Message = photoValidation.ErrorMessage });
            }
            if (certificate != null)
            {
                var certValidation = _fileValidationService.ValidateFormFile(certificate, FileCategory.Document, 10 * 1024 * 1024);
                if (!certValidation.IsValid) return BadRequest(new { Message = certValidation.ErrorMessage });
            }
            if (paymentProof != null)
            {
                var paymentValidation = _fileValidationService.ValidateFormFile(paymentProof, FileCategory.Document, 10 * 1024 * 1024);
                if (!paymentValidation.IsValid) return BadRequest(new { Message = paymentValidation.ErrorMessage });
            }

            // map IFormFile -> UploadedFileDto (Application DTO)
            UploadedFileDto? ToUploaded(IFormFile f)
            {
                if (f == null) return null;
                // copy to MemoryStream (caller keeps stream alive until request ends)
                var ms = new MemoryStream((int)f.Length);
                f.CopyTo(ms);
                ms.Position = 0;
                return new UploadedFileDto
                {
                    Content = ms,
                    FileName = f.FileName,
                    ContentType = f.ContentType ?? "application/octet-stream",
                    Length = f.Length
                };
            }

            var photoDto = photo != null ? ToUploaded(photo) : null;
            var certDto = certificate != null ? ToUploaded(certificate) : null;
            var paymentDto = paymentProof != null ? ToUploaded(paymentProof) : null;

            var memberId = await _memberService.RegisterAsync(dto, photoDto, certDto, paymentDto, cancellationToken);
            return CreatedAtAction(nameof(GetStatus), new { id = memberId, email = dto.Email }, new { MemberId = memberId, Message = "Application submitted. OTP sent to email." });
        }

        [HttpGet("status/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStatus(int id, [FromQuery] string email, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest(new { Message = "Email is required." });

            try
            {
                var res = await _memberService.GetStatusAsync(id, email, cancellationToken);
                return Ok(res);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto, CancellationToken cancellationToken)
        {
            var isVerified = await _memberService.VerifyEmailAsync(dto.Email, dto.OtpCode, cancellationToken);
            
            if (!isVerified)
            {
                return BadRequest(new { Message = "Invalid or expired OTP code" });
            }

            return Ok(new { Message = "Email verified successfully" });
        }

        [HttpPost("resend-otp")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto dto, CancellationToken cancellationToken)
        {
            var success = await _memberService.ResendOtpAsync(dto.Email, cancellationToken);
            if (!success)
            {
                return BadRequest(new { Message = "Could not resend OTP. Ensure the email is correct and not already verified." });
            }

            return Ok(new { Message = "A new OTP has been sent to your email." });
        }
    }
}