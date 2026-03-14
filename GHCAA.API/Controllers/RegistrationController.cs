using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
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
        public RegistrationController(IMemberService memberService) => _memberService = memberService;

        [HttpPost("register")]
        [RequestSizeLimit(10_485_760)]
        public async Task<IActionResult> Register([FromForm] MemberRegistrationDto dto,
            IFormFile? photo, IFormFile? certificate, IFormFile? paymentProof, CancellationToken cancellationToken = default)
        {
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
            return CreatedAtAction(nameof(GetStatus), new { id = memberId }, new { MemberId = memberId, Message = "Application submitted. OTP sent to email." });
        }

        [HttpGet("status/{id:int}")]
        public async Task<IActionResult> GetStatus(int id, CancellationToken cancellationToken)
        {
            var res = await _memberService.GetStatusAsync(id, cancellationToken);
            return Ok(res);
        }

        [HttpPost("verify-email")]
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
    }
}