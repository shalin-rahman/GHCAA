using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [Authorize(Policy = "SuperAdminOnly")]
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
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
    }
}
