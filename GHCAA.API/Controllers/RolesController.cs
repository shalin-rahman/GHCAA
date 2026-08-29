using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GHCAA.Domain;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.API.Controllers
{
    [Authorize(Policy = Constants.Policies.SuperAdminOnly)]
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IUserService _userService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IRoleService roleService, IUserService userService, ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        {
            var users = await _userService.GetAllUsersAsync(cancellationToken);
            return Ok(users.Select(u => new
            {
                u.Id,
                u.Username,
                u.MemberId,
                FullName = u.Member?.FullName ?? "System Account",
                u.IsActive,
                u.CreatedAt,
                Roles = u.Roles.Select(r => r.Name)
            }));
        }

        [HttpPost("users")]
        [GHCAA.API.Filters.RequireStepUp]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userService.CreateSystemAdminAsync(dto.Username, dto.Password, dto.Role, cancellationToken);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create admin user {Username}", dto.Username);
                return BadRequest(new { Message = ex.Message });
            }
        }

        public class CreateAdminDto
        {
            public string Username { get; set; } = null!;

            [Required(ErrorMessage = "Password is required.")]
            [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
                ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one digit.")]
            public string Password { get; set; } = null!;
            public string Role { get; set; } = Constants.Roles.Admin;
        }


        [HttpGet]
        public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
        {
            var roles = await _roleService.GetAllRolesAsync(cancellationToken);
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] string roleName, CancellationToken cancellationToken)
        {
            var role = await _roleService.CreateRoleAsync(roleName, cancellationToken);
            return Ok(role);
        }

        [HttpPost("assign")]
        [GHCAA.API.Filters.RequireStepUp]
        public async Task<IActionResult> AssignRole(int userId, string roleName, CancellationToken cancellationToken)
        {
            var success = await _roleService.AssignRoleToUserAsync(userId, roleName, cancellationToken);
            if (!success) return BadRequest(new { Message = "User or Role not found" });
            return Ok(new { Message = "Role assigned successfully" });
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveRole(int userId, string roleName, CancellationToken cancellationToken)
        {
            var success = await _roleService.RemoveRoleFromUserAsync(userId, roleName, cancellationToken);
            if (!success) return BadRequest(new { Message = "User not found" });
            return Ok(new { Message = "Role removed successfully" });
        }

        [HttpDelete("users/{id}")]
        [GHCAA.API.Filters.RequireStepUp]
        public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
        {
            var success = await _userService.DeleteSystemAdminAsync(id, cancellationToken);
            if (!success) return BadRequest(new { Message = "Only non-member system administrator accounts can be deleted here." });
            return Ok(new { Message = "System administrator account deleted." });
        }
    }
}
