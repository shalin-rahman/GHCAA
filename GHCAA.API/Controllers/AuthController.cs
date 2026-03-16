using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(loginDto, cancellationToken);

            if (result == null)
            {
                return Unauthorized(new { Message = "Invalid username or password" });
            }

            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken cancellationToken)
        {
            var success = await _authService.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword, cancellationToken);
            if (!success)
            {
                return BadRequest(new { Message = "Invalid or expired reset token." });
            }

            return Ok(new { Message = "Password has been reset successfully. You can now login." });
        }
    }
}
