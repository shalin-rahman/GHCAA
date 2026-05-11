using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly GHCAA.Infrastructure.Data.ApplicationDbContext _db;

        public AuthController(IAuthService authService, ITokenService tokenService, GHCAA.Infrastructure.Data.ApplicationDbContext db)
        {
            _authService = authService;
            _tokenService = tokenService;
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
                return Unauthorized(new { Message = "Invalid username or password" });

            await SetAuthCookiesAsync(result, cancellationToken);
            return Ok(result);
        }

        [HttpPost("google")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] SocialLoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.GoogleLoginAsync(request.Token, cancellationToken);
            if (result == null) return Unauthorized(new { Message = "Google authentication failed" });
            await SetAuthCookiesAsync(result, cancellationToken);
            return Ok(result);
        }

        [HttpPost("facebook")]
        [AllowAnonymous]
        public async Task<IActionResult> FacebookLogin([FromBody] SocialLoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.FacebookLoginAsync(request.Token, cancellationToken);
            if (result == null) return Unauthorized(new { Message = "Facebook authentication failed" });
            await SetAuthCookiesAsync(result, cancellationToken);
            return Ok(result);
        }

        // 24.27+24.44: Issue a new access token from a valid refresh token cookie.
        [HttpPost("refresh")]
        [AllowAnonymous]
        [DisableRateLimiting]
        public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
        {
            if (!Request.Cookies.TryGetValue("refresh_token", out var oldRefreshToken) || string.IsNullOrEmpty(oldRefreshToken))
                return Unauthorized(new { Message = "No refresh token." });

            var rotation = await _tokenService.RotateRefreshTokenAsync(oldRefreshToken, cancellationToken);
            if (rotation == null)
            {
                ClearAuthCookies();
                return Unauthorized(new { Message = "Invalid or expired refresh token." });
            }

            var (newRefreshToken, userId) = rotation.Value;

            var user = await _db.Users.Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null) { ClearAuthCookies(); return Unauthorized(); }

            var newAccessToken = _tokenService.CreateToken(user);
            SetCookie("access_token", newAccessToken, TimeSpan.FromMinutes(65));
            SetCookie("refresh_token", newRefreshToken, TimeSpan.FromDays(7));

            return Ok(new { Token = newAccessToken });
        }

        // 24.39: Angular calls this on app init to restore auth state from the httpOnly cookie.
        [HttpGet("me")]
        [Authorize]
        [DisableRateLimiting]
        public IActionResult Me()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";
            var memberId = User.FindFirst("MemberId")?.Value;

            return Ok(new
            {
                Username = username,
                MemberId = memberId == null ? (int?)null : int.Parse(memberId),
                Role = role
            });
        }

        // 24.44: Revoke all refresh tokens and clear auth cookies.
        [HttpPost("logout")]
        [Authorize]
        [DisableRateLimiting]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
                await _tokenService.RevokeAllRefreshTokensAsync(userId, cancellationToken);

            ClearAuthCookies();
            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken cancellationToken)
        {
            var success = await _authService.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword, cancellationToken);
            if (!success)
                return BadRequest(new { Message = "Invalid or expired reset token." });

            return Ok(new { Message = "Password has been reset successfully. You can now login." });
        }

        // --- Helpers ---

        private async Task SetAuthCookiesAsync(TokenResponseDto result, CancellationToken cancellationToken)
        {
            SetCookie("access_token", result.Token, TimeSpan.FromMinutes(65));

            var refreshToken = _tokenService.GenerateRefreshToken();
            // Resolve User.Id from the JWT claim we just created.
            if (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                await _tokenService.StoreRefreshTokenAsync(userId, refreshToken, cancellationToken);
            }
            else
            {
                // Fallback: look up by username.
                var user = await _db.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == result.Username, cancellationToken);
                if (user != null)
                    await _tokenService.StoreRefreshTokenAsync(user.Id, refreshToken, cancellationToken);
            }

            SetCookie("refresh_token", refreshToken, TimeSpan.FromDays(7));
        }

        private void SetCookie(string name, string value, TimeSpan maxAge)
        {
            Response.Cookies.Append(name, value, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                MaxAge = maxAge,
                Path = "/"
            });
        }

        private void ClearAuthCookies()
        {
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");
        }

        public class SocialLoginRequest
        {
            public string Token { get; set; } = null!;
        }
    }
}
