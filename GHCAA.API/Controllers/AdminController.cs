using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IMemberService _memberService;

        public AdminController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        [HttpPost("members/{id}/approve")]
        public async Task<IActionResult> ApproveMember(int id, [FromBody] ApproveMemberDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _memberService.ApproveMemberAsync(id, dto.ApprovedByAdminId, cancellationToken);
                return Ok(new 
                { 
                    Message = "Member approved successfully", 
                    MembershipNumber = result.MembershipNumber,
                    DefaultPassword = result.DefaultPassword
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
