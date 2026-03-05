using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/admin/members/import")]
    [Authorize(Policy = "AdminOnly")]
    public class MemberImportController : ControllerBase
    {
        private readonly IMemberImportService _importService;

        public MemberImportController(IMemberImportService importService)
        {
            _importService = importService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Import([FromForm] MemberImportRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _importService.ImportMembersAsync(request, cancellationToken);
            return Ok(result);
        }
    }
}
