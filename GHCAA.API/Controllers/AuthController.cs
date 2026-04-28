using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly GHCAA.Infrastructure.Data.ApplicationDbContext _db;

        public AuthController(IAuthService authService, GHCAA.Infrastructure.Data.ApplicationDbContext db)
        {
            _authService = authService;
            _db = db;
        }

        [HttpGet("providers")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProviders()
        {
            var providers = await _db.SocialAuthConfigs
                .Where(c => c.IsEnabled)
                .Select(c => new { c.Provider, c.ClientId })
                .ToListAsync();

            return Ok(providers);
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

        [HttpPost("google")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] SocialLoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.GoogleLoginAsync(request.Token, cancellationToken);
            if (result == null) return Unauthorized(new { Message = "Google authentication failed" });
            return Ok(result);
        }

        [HttpPost("facebook")]
        [AllowAnonymous]
        public async Task<IActionResult> FacebookLogin([FromBody] SocialLoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.FacebookLoginAsync(request.Token, cancellationToken);
            if (result == null) return Unauthorized(new { Message = "Facebook authentication failed" });
            return Ok(result);
        }

        public class SocialLoginRequest
        {
            public string Token { get; set; } = null!;
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
