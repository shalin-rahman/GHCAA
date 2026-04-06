using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/ledger")]
    [Authorize(Policy = "SuperAdminOnly")] // Strict role parity: Sync with frontend superAdminGuard
    public class FinancialLedgerController : ControllerBase
    {
        private readonly IFinancialLedgerService _ledgerService;

        public FinancialLedgerController(IFinancialLedgerService ledgerService)
        {
            _ledgerService = ledgerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRecords(int page = 1, int pageSize = 10, [FromQuery] int? year = null, [FromQuery] string? search = null, [FromQuery] Domain.Enums.FinancialRecordType? type = null, CancellationToken cancellationToken = default)
        {
            var records = await _ledgerService.GetRecordsAsync(page, pageSize, year, search, type, cancellationToken);
            return Ok(records);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int year, CancellationToken cancellationToken)
        {
            var summary = await _ledgerService.GetSummaryAsync(year, cancellationToken);
            return Ok(summary);
        }

        [HttpPost]
        public async Task<IActionResult> AddRecord([FromBody] FinancialRecord record, CancellationToken cancellationToken)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(adminIdClaim, out var adminId))
            {
                record.CreatedByAdminId = adminId;
            }

            var result = await _ledgerService.AddRecordAsync(record, cancellationToken);
            return CreatedAtAction(nameof(GetRecords), new { year = result.Year }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRecord(int id, [FromBody] FinancialRecord record, CancellationToken cancellationToken)
        {
            record.Id = id;
            var result = await _ledgerService.UpdateRecordAsync(record, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecord(int id, CancellationToken cancellationToken)
        {
            var success = await _ledgerService.DeleteRecordAsync(id, cancellationToken);
            return success ? Ok() : NotFound();
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportCsv([FromQuery] int? year, CancellationToken cancellationToken)
        {
            var bytes = await _ledgerService.ExportRecordsAsync(year, cancellationToken);
            return File(bytes, "text/csv", $"Ledger_{year ?? DateTime.Now.Year}.csv");
        }
    }
}
