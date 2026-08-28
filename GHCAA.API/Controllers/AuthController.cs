using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Application.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using GHCAA.Domain;

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
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public AuthController(IAuthService authService, ITokenService tokenService, GHCAA.Infrastructure.Data.ApplicationDbContext db, IWebHostEnvironment env, IConfiguration config)
        {
            _authService = authService;
            _tokenService = tokenService;
            _db = db;
            _env = env;
            _config = config;
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
        [AllowAnonymous]
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
        [EnableRateLimiting("refresh")]
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

            var newAccessToken = CreateRefreshedAccessToken(user);
            SetCookie("access_token", newAccessToken, TimeSpan.FromMinutes(65));
            SetCookie("refresh_token", newRefreshToken, TimeSpan.FromDays(7));
            SetXsrfCookie(TimeSpan.FromDays(7));

            return Ok(new { Token = newAccessToken });
        }

        // Mobile equivalent of /refresh: no cookie jar, so the refresh token
        // travels in the request/response body instead.
        [HttpPost("refresh-mobile")]
        [AllowAnonymous]
        [EnableRateLimiting("refresh")]
        public async Task<IActionResult> RefreshMobile([FromBody] RefreshRequestDto dto, CancellationToken cancellationToken)
        {
            var rotation = await _tokenService.RotateRefreshTokenAsync(dto.RefreshToken, cancellationToken);
            if (rotation == null)
                return Unauthorized(new { Message = "Invalid or expired refresh token." });

            var (newRefreshToken, userId) = rotation.Value;

            var user = await _db.Users.Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null) return Unauthorized();

            var newAccessToken = _tokenService.CreateToken(user);

            return Ok(new { Token = newAccessToken, RefreshToken = newRefreshToken });
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
            var memberId = User.FindFirst(AppClaimTypes.MemberId)?.Value;

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

        // 7.13: Step-up verification. An admin already holds a valid session; these two endpoints
        // prove they still control the account's email inbox before a destructive/financial action
        // is allowed through [RequireStepUp].
        [HttpPost("admin/step-up/request")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> RequestStepUp([FromServices] IOtpService otpService, CancellationToken cancellationToken)
        {
            var user = await LoadCurrentUserAsync(cancellationToken);
            var email = user?.Member?.Email;

            if (user == null || string.IsNullOrWhiteSpace(email))
                return BadRequest(new { Message = "No email address is on file for this account." });

            await otpService.GenerateAndSendOtpAsync(email, Domain.Enums.OtpPurpose.AdminStepUp, cancellationToken);
            return Ok(new { Message = "A verification code has been sent to your registered email address." });
        }

        [HttpPost("admin/step-up/verify")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> VerifyStepUp([FromBody] StepUpVerifyDto dto, [FromServices] IOtpService otpService, CancellationToken cancellationToken)
        {
            var user = await LoadCurrentUserAsync(cancellationToken);
            var email = user?.Member?.Email;

            if (user == null || string.IsNullOrWhiteSpace(email))
                return BadRequest(new { Message = "No email address is on file for this account." });

            var verified = await otpService.VerifyOtpAsync(email, dto.Code, Domain.Enums.OtpPurpose.AdminStepUp, cancellationToken);
            if (!verified)
                return BadRequest(new { Message = "That verification code is invalid or has expired." });

            // Re-issue the access token carrying the step-up claim. The refresh token is left
            // alone: this raises the current session's assurance level, it is not a new login.
            var stepUpToken = _tokenService.CreateStepUpToken(user);
            SetCookie("access_token", stepUpToken, TimeSpan.FromMinutes(65));

            return Ok(new { Token = stepUpToken });
        }

        // 7.13: A refresh must not silently reset the ~30-day step-up grace period — the access
        // token is refreshed roughly hourly, far more often than the OTP challenge should ever
        // need to reappear. The (now-expired) outgoing access_token cookie is the only place that
        // grace period is recorded, so it's read here, signature-checked, and carried forward
        // onto the new token if still within TTL. A fresh login never does this (Login() calls
        // plain CreateToken), so signing back in after signing out always starts unverified.
        private string CreateRefreshedAccessToken(GHCAA.Domain.Models.User user)
        {
            var ttlMinutes = int.TryParse(_config["AppSettings:StepUpTtlMinutes"], out var v) && v > 0
                ? v
                : StepUpClaim.DefaultTtlMinutes;

            Request.Cookies.TryGetValue("access_token", out var previousAccessToken);
            var carriedEpoch = _tokenService.TryGetValidStepUpEpoch(previousAccessToken, ttlMinutes);

            return carriedEpoch.HasValue
                ? _tokenService.CreateTokenWithCarriedStepUp(user, carriedEpoch.Value)
                : _tokenService.CreateToken(user);
        }

        private async Task<GHCAA.Domain.Models.User?> LoadCurrentUserAsync(CancellationToken cancellationToken)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                return null;

            return await _db.Users
                .Include(u => u.Roles)
                .Include(u => u.Member)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
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
            SetXsrfCookie(TimeSpan.FromDays(7));

            // Cookie-based (web) clients ignore this; mobile clients (no cookie
            // jar) persist it and send it back to /auth/refresh-mobile.
            result.RefreshToken = refreshToken;
        }

        private void SetCookie(string name, string value, TimeSpan maxAge)
        {
            Response.Cookies.Append(name, value, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                MaxAge = maxAge,
                Path = "/"
            });
        }

        // 1a: Readable (non-httpOnly) double-submit-cookie token. Angular's HttpClient
        // reads this and echoes it back as the X-XSRF-TOKEN header; XsrfMiddleware
        // validates the two match on state-changing requests.
        private void SetXsrfCookie(TimeSpan maxAge)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            Response.Cookies.Append("XSRF-TOKEN", token, new CookieOptions
            {
                HttpOnly = false,
                Secure = !_env.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                MaxAge = maxAge,
                Path = "/"
            });
        }

        private void ClearAuthCookies()
        {
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");
            Response.Cookies.Delete("XSRF-TOKEN");
        }

        public class SocialLoginRequest
        {
            public string Token { get; set; } = null!;
        }
    }
}
