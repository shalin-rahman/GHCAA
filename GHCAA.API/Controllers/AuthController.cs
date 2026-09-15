using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Application.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GHCAA.Domain;
using GHCAA.API.Extensions;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting(Constants.RateLimitPolicies.Auth)]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly ISocialAuthConfigService _socialAuthConfigService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public AuthController(IAuthService authService, ITokenService tokenService, ISocialAuthConfigService socialAuthConfigService, IWebHostEnvironment env, IConfiguration config)
        {
            _authService = authService;
            _tokenService = tokenService;
            _socialAuthConfigService = socialAuthConfigService;
            _env = env;
            _config = config;
        }

        [HttpGet("providers")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProviders()
        {
            var enabled = await _socialAuthConfigService.GetEnabledAsync();
            return Ok(enabled.Select(c => new { c.Provider, c.ClientId }));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(loginDto, cancellationToken);
            if (result == null)
                return Problem(detail: "Invalid username or password", statusCode: StatusCodes.Status401Unauthorized);

            await SetAuthCookiesAsync(result, cancellationToken);
            return Ok(result);
        }

        [HttpPost("google")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] SocialLoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.GoogleLoginAsync(request.Token, cancellationToken);
            if (result == null) return Problem(detail: "Google authentication failed", statusCode: StatusCodes.Status401Unauthorized);
            await SetAuthCookiesAsync(result, cancellationToken);
            return Ok(result);
        }

        [HttpPost("facebook")]
        [AllowAnonymous]
        public async Task<IActionResult> FacebookLogin([FromBody] SocialLoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.FacebookLoginAsync(request.Token, cancellationToken);
            if (result == null) return Problem(detail: "Facebook authentication failed", statusCode: StatusCodes.Status401Unauthorized);
            await SetAuthCookiesAsync(result, cancellationToken);
            return Ok(result);
        }

        // 24.27+24.44: Issue a new access token from a valid refresh token cookie.
        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting(Constants.RateLimitPolicies.Refresh)]
        public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
        {
            if (!Request.Cookies.TryGetValue("refresh_token", out var oldRefreshToken) || string.IsNullOrEmpty(oldRefreshToken))
                return Problem(detail: "No refresh token.", statusCode: StatusCodes.Status401Unauthorized);

            var rotation = await _tokenService.RotateRefreshTokenAsync(oldRefreshToken, cancellationToken);
            if (rotation == null)
            {
                ClearAuthCookies();
                return Problem(detail: "Invalid or expired refresh token.", statusCode: StatusCodes.Status401Unauthorized);
            }

            var (newRefreshToken, userId) = rotation.Value;

            var user = await _authService.GetUserWithRolesAsync(userId, cancellationToken);
            // A terminated/archived member's SecurityStamp rotation and refresh-token revocation
            // race the client's already-issued refresh token; !IsActive is the backstop that closes
            // that window even if revocation is somehow missed at the point of deactivation.
            if (user == null || !user.IsActive || user.IsArchived) { ClearAuthCookies(); return Unauthorized(); }

            var newAccessToken = CreateRefreshedAccessToken(user);
            this.SetAuthCookie(_env, "access_token", newAccessToken, TimeSpan.FromMinutes(65));
            this.SetAuthCookie(_env, "refresh_token", newRefreshToken, TimeSpan.FromDays(7));
            this.SetXsrfCookie(_env, TimeSpan.FromDays(7));

            return Ok(new { Token = newAccessToken });
        }

        // Mobile equivalent of /refresh: no cookie jar, so the refresh token
        // travels in the request/response body instead.
        [HttpPost("refresh-mobile")]
        [AllowAnonymous]
        [EnableRateLimiting(Constants.RateLimitPolicies.Refresh)]
        public async Task<IActionResult> RefreshMobile([FromBody] RefreshRequestDto dto, CancellationToken cancellationToken)
        {
            var rotation = await _tokenService.RotateRefreshTokenAsync(dto.RefreshToken, cancellationToken);
            if (rotation == null)
                return Problem(detail: "Invalid or expired refresh token.", statusCode: StatusCodes.Status401Unauthorized);

            var (newRefreshToken, userId) = rotation.Value;

            var user = await _authService.GetUserWithRolesAsync(userId, cancellationToken);
            if (user == null || !user.IsActive || user.IsArchived) return Unauthorized();

            var newAccessToken = _tokenService.CreateToken(user);

            return Ok(new { Token = newAccessToken, RefreshToken = newRefreshToken });
        }

        // 24.39: Angular calls this on app init to restore auth state from the httpOnly cookie.
        [HttpGet("me")]
        [Authorize]
        [DisableRateLimiting]
        public IActionResult Me()
        {
            var userId = this.CurrentUserIdRaw();
            var username = this.CurrentUsername();
            var role = this.CurrentRole() ?? "Member";
            var memberId = this.CurrentMemberIdRaw();

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
            var userIdClaim = this.CurrentUserIdRaw();
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
                return Problem(detail: "No email address is on file for this account.", statusCode: StatusCodes.Status400BadRequest);

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
                return Problem(detail: "No email address is on file for this account.", statusCode: StatusCodes.Status400BadRequest);

            var verified = await otpService.VerifyOtpAsync(email, dto.Code, Domain.Enums.OtpPurpose.AdminStepUp, cancellationToken);
            if (!verified)
                return Problem(detail: "That verification code is invalid or has expired.", statusCode: StatusCodes.Status400BadRequest);

            // Re-issue the access token carrying the step-up claim. The refresh token is left
            // alone: this raises the current session's assurance level, it is not a new login.
            var stepUpToken = _tokenService.CreateStepUpToken(user);
            this.SetAuthCookie(_env, "access_token", stepUpToken, TimeSpan.FromMinutes(65));

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
            if (!int.TryParse(this.CurrentUserIdRaw(), out var userId))
                return null;

            return await _authService.GetUserWithRolesAndMemberAsync(userId, cancellationToken);
        }

        // 80.16: the mobile client already posts this shape to this exact route
        // (AuthService.forgotPassword() in GHCAA.Mobile) — it only needed the route to exist.
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [EnableRateLimiting(Constants.RateLimitPolicies.PasswordReset)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken cancellationToken)
        {
            await _authService.RequestPasswordResetAsync(dto.Identifier, cancellationToken);
            // Always the same response, matched or not — the request-a-reset endpoint must not be
            // usable to check which emails/usernames exist.
            return Ok(new { Message = "If that account exists, a password reset link has been sent." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken cancellationToken)
        {
            var success = await _authService.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword, cancellationToken);
            if (!success)
                return Problem(detail: "Invalid or expired reset token.", statusCode: StatusCodes.Status400BadRequest);

            return Ok(new { Message = "Password has been reset successfully. You can now login." });
        }

        // --- Helpers ---

        private async Task SetAuthCookiesAsync(TokenResponseDto result, CancellationToken cancellationToken)
        {
            this.SetAuthCookie(_env, "access_token", result.Token, TimeSpan.FromMinutes(65));

            var refreshToken = _tokenService.GenerateRefreshToken();
            // Resolve User.Id from the JWT claim we just created.
            if (int.TryParse(this.CurrentUserIdRaw(), out var userId))
            {
                await _tokenService.StoreRefreshTokenAsync(userId, refreshToken, cancellationToken);
            }
            else
            {
                // Fallback: look up by username.
                var user = await _authService.GetUserByUsernameAsync(result.Username, cancellationToken);
                if (user != null)
                    await _tokenService.StoreRefreshTokenAsync(user.Id, refreshToken, cancellationToken);
            }

            this.SetAuthCookie(_env, "refresh_token", refreshToken, TimeSpan.FromDays(7));
            this.SetXsrfCookie(_env, TimeSpan.FromDays(7));

            // Cookie-based (web) clients ignore this; mobile clients (no cookie
            // jar) persist it and send it back to /auth/refresh-mobile.
            result.RefreshToken = refreshToken;
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
