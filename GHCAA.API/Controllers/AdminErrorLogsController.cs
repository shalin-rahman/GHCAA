using System;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/admin/error-logs")]
    [Authorize(Policy = Constants.Policies.SuperAdminOnly)]
    public class AdminErrorLogsController : ControllerBase
    {
        private readonly IErrorLogService _errorLogService;

        public AdminErrorLogsController(IErrorLogService errorLogService)
        {
            _errorLogService = errorLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetErrorLogs(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? level,
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var filter = new ErrorLogFilterDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                Level = level,
                Query = query,
                Page = page,
                PageSize = pageSize
            };

            var result = await _errorLogService.GetPagedAsync(filter, cancellationToken);
            return Ok(result);
        }
    }
}
