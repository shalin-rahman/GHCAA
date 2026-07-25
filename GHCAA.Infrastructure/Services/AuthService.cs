using System.Linq;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using BCrypt.Net;

namespace GHCAA.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;
        private readonly IActivityService _activityService;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(ApplicationDbContext db,
            ITokenService tokenService,
            ILogger<AuthService> logger,
            IActivityService activityService,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _tokenService = tokenService;
            _logger = logger;
            _activityService = activityService;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        // TODO [CRITICAL]: No brute-force / lockout protection. An attacker can make unlimited login
        // attempts against any discoverable username. Add FailedLoginAttempts + LockoutUntil to User,
        // increment on each failure, lock for 15 min after 5 failures, and return uniform 401 always.
        // TODO [HIGH]: Username enumeration via timing. BCrypt.Verify only runs when the user EXISTS —
        // a measurably shorter response for "user not found" reveals valid usernames. Always run a
        // dummy BCrypt.Verify against a static hash when the user is not found to normalize timing.
        public async Task<TokenResponseDto?> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
        {
            // Trim whitespace and remove internal spaces for identifiers like NID/Username
            var input = loginDto.Username?.Trim() ?? string.Empty;
            var normalizedInput = input.Replace(" ", "");

            // 1. Try finding user directly by Username (exact match with normalized string)
            var user = await _db.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Username == normalizedInput || u.Username == input, cancellationToken);

            // 2. Fallback: If not found, try finding user via Member properties (Email, NID, MembershipNumber)
            if (user == null)
            {
                _logger.LogInformation("Direct username lookup failed for '{Username}', trying member fallbacks...", input);

                var member = await _db.Members
                    .IgnoreQueryFilters() // Just for lookup, we'll check status/archived later
                    .FirstOrDefaultAsync(m => m.Email == input || m.NID == normalizedInput || m.MembershipNumber == normalizedInput || m.MembershipNumber == input, cancellationToken);

                if (member != null)
                {
                    user = await _db.Users
                        .Include(u => u.Roles)
                        .FirstOrDefaultAsync(u => u.MemberId == member.Id, cancellationToken);

                    if (user == null)
                    {
                        _logger.LogWarning("Member found for '{Username}' but has no associated user account", input);
                    }
                }
            }

            if (user == null)
            {
                _logger.LogWarning("Login failed: User {Username} not found after checking all identifiers", input);
                // S5.2: Run a dummy BCrypt verify to equalize response timing and prevent username enumeration.
                BCrypt.Net.BCrypt.Verify(loginDto.Password, "$2a$11$dummyhashfortimingequalizationXXXXXXXXXXXXXXXXXXXXXX");
                return null;
            }

            // Check if member status allows login (if associated with a member profile)
            if (user.MemberId.HasValue)
            {
                var member = await _db.Members.FindAsync(user.MemberId.Value);
                if (member == null || member.IsArchived || (member.Status != Enums.MembershipStatus.Active && member.Status != Enums.MembershipStatus.Applied))
                {
                    _logger.LogWarning("Login failed: Member {Username} is archived or has restricted status: {Status}", loginDto.Username, member?.Status);
                    return null;
                }
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User {Username} is inactive", loginDto.Username);
                return null;
            }

            // S5.1: Brute-force lockout check.
            if (user.LockoutUntil.HasValue && user.LockoutUntil.Value > DateTime.UtcNow)
            {
                _logger.LogWarning("Login blocked: User {Username} is locked until {Until}", user.Username, user.LockoutUntil.Value);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutUntil = DateTime.UtcNow.AddMinutes(15);
                    _logger.LogWarning("User {Username} locked out for 15 minutes after {N} failed attempts", user.Username, user.FailedLoginAttempts);
                }
                await _db.SaveChangesAsync(cancellationToken);
                _logger.LogError("Login failed: Invalid password for user {Username}", user.Username);
                return null;
            }

            // Reset lockout on successful login.
            user.FailedLoginAttempts = 0;
            user.LockoutUntil = null;

            var token = _tokenService.CreateToken(user);

            _logger.LogInformation("User {Username} logged in successfully", user.Username);

            if (user.MemberId.HasValue)
            {
                await _activityService.LogActivityAsync(user.MemberId.Value, "Login", $"User {user.Username} logged in.", source: "System", cancellationToken: default);
            }

            string? fullName = null;
            string? email = null;
            string? mobileNo = null;

            if (user.MemberId.HasValue)
            {
                var member = await _db.Members.FindAsync(user.MemberId.Value);
                if (member != null)
                {
                    fullName = member.FullName;
                    email = member.Email;
                    mobileNo = member.MobileNo;
                }
            }
            return new TokenResponseDto
            {
                Token = token,
                Username = user.Username,
                MemberId = user.MemberId,
                Role = PickPrimaryRoleNameForClient(user.Roles),
                FullName = fullName,
                Email = email,
                MobileNo = mobileNo,
                MustChangePassword = user.MustChangePassword
            };
        }

        public async Task<TokenResponseDto?> GoogleLoginAsync(string idToken, CancellationToken cancellationToken = default)
        {
            try
            {
                var config = await _db.SocialAuthConfigs.FirstOrDefaultAsync(c => c.Provider == Enums.SocialProvider.Google, cancellationToken);
                if (config == null || !config.IsEnabled)
                {
                    _logger.LogWarning("Google login is disabled or not configured.");
                    return null;
                }

                var settings = new Google.Apis.Auth.GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { config.ClientId }
                };

                var payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return await SocialLoginAsync(payload.Subject, payload.Email, payload.Name, "Google", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google token validation failed");
                return null;
            }
        }

        public async Task<TokenResponseDto?> FacebookLoginAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                var config = await _db.SocialAuthConfigs.FirstOrDefaultAsync(c => c.Provider == Enums.SocialProvider.Facebook, cancellationToken);
                if (config == null || !config.IsEnabled)
                {
                    _logger.LogWarning("Facebook login is disabled or not configured.");
                    return null;
                }

                var client = _httpClientFactory.CreateClient();

                // 29B.1: Verify the access token was actually issued for OUR app before trusting it.
                // A token minted for any other Facebook app the user authorized would otherwise
                // resolve to a valid /me response, letting an attacker take over the matching account.
                if (string.IsNullOrEmpty(config.ClientSecret))
                {
                    _logger.LogWarning("Facebook login rejected: app secret not configured, cannot verify token audience.");
                    return null;
                }

                var appAccessToken = $"{config.ClientId}|{config.ClientSecret}";
                var debugResponse = await client.GetAsync(
                    $"https://graph.facebook.com/debug_token?input_token={Uri.EscapeDataString(accessToken)}&access_token={Uri.EscapeDataString(appAccessToken)}",
                    cancellationToken);

                if (!debugResponse.IsSuccessStatusCode) return null;

                var debugContent = await debugResponse.Content.ReadAsStringAsync(cancellationToken);
                var debugResult = System.Text.Json.JsonSerializer.Deserialize<FacebookDebugTokenResponse>(debugContent, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (debugResult?.Data == null || !debugResult.Data.IsValid || debugResult.Data.AppId != config.ClientId)
                {
                    _logger.LogWarning("Facebook login rejected: token failed app_id/validity verification.");
                    return null;
                }

                var response = await client.GetAsync($"https://graph.facebook.com/me?fields=id,name,email&access_token={Uri.EscapeDataString(accessToken)}", cancellationToken);

                if (!response.IsSuccessStatusCode) return null;

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var fbUser = System.Text.Json.JsonSerializer.Deserialize<FacebookUserDto>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (fbUser == null || string.IsNullOrEmpty(fbUser.Id)) return null;

                return await SocialLoginAsync(fbUser.Id, fbUser.Email, fbUser.Name, "Facebook", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Facebook token validation failed");
                return null;
            }
        }

        public async Task<TokenResponseDto?> SocialLoginAsync(string socialId, string? email, string? name, string provider, CancellationToken cancellationToken = default)
        {
            var user = await _db.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => (provider == "Google" && u.GoogleId == socialId) || (provider == "Facebook" && u.FacebookId == socialId), cancellationToken);

            if (user == null && !string.IsNullOrEmpty(email))
            {
                // 24.25: Only auto-link when the local member's email is verified.
                // An unverified local email could be attacker-controlled, enabling account takeover.
                user = await _db.Users
                    .Include(u => u.Roles)
                    .Include(u => u.Member)
                    .FirstOrDefaultAsync(u => u.Member!.Email == email && u.Member.EmailVerified == true, cancellationToken);

                if (user != null)
                {
                    if (provider == "Google") user.GoogleId = socialId;
                    else user.FacebookId = socialId;
                    await _db.SaveChangesAsync(cancellationToken);
                }
            }

            if (user == null)
            {
                // 24.26: Use a provider-scoped unique sentinel for NID and MobileNo so a second social
                // signup does not crash the UNIQUE index. Profile completion forces the member to supply
                // real values before admin approval.
                var uniqueSentinel = $"SOCIAL-{provider.ToUpper()}-{socialId}";
                var member = new Member
                {
                    FullName = name ?? "Social User",
                    Email = email ?? $"{socialId}@{provider.ToLower()}.com",
                    Status = Enums.MembershipStatus.Applied,
                    AppliedDate = DateTime.UtcNow,
                    IsProfileComplete = false,
                    FatherName = "TBD",
                    MotherName = "TBD",
                    NID = uniqueSentinel,
                    MobileNo = uniqueSentinel,
                    PresentAddress = "TBD",
                    PermanentAddress = "TBD",
                    EmergencyContactName = "TBD",
                    EmergencyContactRelation = "TBD",
                    EmergencyContactPhone = "TBD"
                };

                _db.Members.Add(member);
                await _db.SaveChangesAsync(cancellationToken);

                user = new User
                {
                    Username = email ?? socialId,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Random password
                    MemberId = member.Id,
                    GoogleId = provider == "Google" ? socialId : null,
                    FacebookId = provider == "Facebook" ? socialId : null,
                    IsActive = true
                };

                var memberRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Member", cancellationToken);
                if (memberRole != null) user.Roles.Add(memberRole);

                _db.Users.Add(user);
                await _db.SaveChangesAsync(cancellationToken);

                user = await _db.Users.Include(u => u.Roles).FirstAsync(u => u.Id == user.Id, cancellationToken);
            }

            // Standard login logic from here
            if (!user.IsActive) return null;

            var token = _tokenService.CreateToken(user);

            var memberProfile = await _db.Members.FindAsync(user.MemberId);

            return new TokenResponseDto
            {
                Token = token,
                Username = user.Username,
                MemberId = user.MemberId,
                Role = PickPrimaryRoleNameForClient(user.Roles),
                FullName = memberProfile?.FullName,
                Email = memberProfile?.Email,
                MobileNo = memberProfile?.MobileNo,
                MustChangePassword = false
            };
        }

        private class FacebookUserDto
        {
            public string Id { get; set; } = null!;
            public string? Name { get; set; }
            public string? Email { get; set; }
        }

        private class FacebookDebugTokenResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("data")]
            public FacebookDebugTokenData? Data { get; set; }
        }

        private class FacebookDebugTokenData
        {
            [System.Text.Json.Serialization.JsonPropertyName("app_id")]
            public string? AppId { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("is_valid")]
            public bool IsValid { get; set; }
        }

        /// <summary>
        /// Mobile UI checks for "SuperAdmin" / "Admin" strings. When a user has multiple roles,
        /// EF does not guarantee order; pick the highest-privilege role for the login payload.
        /// </summary>
        private static string PickPrimaryRoleNameForClient(ICollection<Role>? roles)
        {
            if (roles == null || roles.Count == 0) return "Member";
            var names = roles.Where(r => !string.IsNullOrWhiteSpace(r.Name)).Select(r => r.Name!).ToList();
            if (names.Count == 0) return "Member";
            if (names.Contains("SuperAdmin")) return "SuperAdmin";
            if (names.Contains("Admin")) return "Admin";
            return names[0];
        }
        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Email.ToLower() == email.Trim().ToLower(), cancellationToken);

            if (member == null) return false;

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.MemberId == member.Id && u.ResetToken == token, cancellationToken);

            if (user == null) return false;

            if (!user.ResetTokenExpiry.HasValue || user.ResetTokenExpiry.Value < DateTime.UtcNow)
            {
                _logger.LogWarning("Password reset failed: Token expired for user {Username}", user.Username);
                return false;
            }

            // Update password and clear token. Rotate SecurityStamp to invalidate existing JWTs (S5.4).
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            user.SecurityStamp = Guid.NewGuid().ToString("N");

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Password reset successful for user {Username}", user.Username);

            await _activityService.LogActivityAsync(member.Id, "Password Reset", "User reset their password via email link.", cancellationToken: cancellationToken);

            return true;
        }
    }
}
