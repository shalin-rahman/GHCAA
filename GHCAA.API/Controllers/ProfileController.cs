using System.Security.Claims;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
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

        public ProfileController(IMemberService memberService, IUserService userService)
        {
            _memberService = memberService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
        {
            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();

            var profile = await _memberService.GetProfileAsync(memberId, cancellationToken);
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
