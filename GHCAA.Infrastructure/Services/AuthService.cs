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

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                _logger.LogError("Login failed: Invalid password for user {Username}", user.Username);
                return null;
            }

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
                var response = await client.GetAsync($"https://graph.facebook.com/me?fields=id,name,email&access_token={accessToken}", cancellationToken);
                
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
                // Try linking by email if user exists but hasn't linked social yet
                user = await _db.Users
                    .Include(u => u.Roles)
                    .Include(u => u.Member)
                    .FirstOrDefaultAsync(u => u.Member!.Email == email, cancellationToken);

                if (user != null)
                {
                    if (provider == "Google") user.GoogleId = socialId;
                    else user.FacebookId = socialId;
                    await _db.SaveChangesAsync(cancellationToken);
                }
            }

            if (user == null)
            {
                // Create new user & member (applied status, profile incomplete)
                var member = new Member
                {
                    FullName = name ?? "Social User",
                    Email = email ?? $"{socialId}@{provider.ToLower()}.com",
                    Status = Enums.MembershipStatus.Applied,
                    AppliedDate = DateTime.UtcNow,
                    IsProfileComplete = false,
                    // Fill required but unknown fields with placeholders or nulls if allowed
                    FatherName = "TBD",
                    MotherName = "TBD",
                    NID = "TBD",
                    MobileNo = "TBD",
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

            // Update password and clear token
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Password reset successful for user {Username}", user.Username);
            
            await _activityService.LogActivityAsync(member.Id, "Password Reset", "User reset their password via email link.", cancellationToken: cancellationToken);
            
            return true;
        }
    }
}
