using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GHCAA.Domain;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/admin/members/import")]
    [Authorize(Policy = Constants.Policies.AdminOnly)]
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

        [HttpGet("export")]
        public async Task<IActionResult> Export(CancellationToken cancellationToken)
        {
            var bytes = await _importService.ExportMembersToExcelAsync(cancellationToken);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Members_Registry_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
