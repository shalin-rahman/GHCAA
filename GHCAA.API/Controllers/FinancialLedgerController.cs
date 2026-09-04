using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GHCAA.Domain;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/ledger")]
    [Authorize(Policy = Constants.Policies.SuperAdminOnly)] // Strict role parity: Sync with frontend superAdminGuard
    public class FinancialLedgerController : ControllerBase
    {
        private readonly IFinancialLedgerService _ledgerService;

        public FinancialLedgerController(IFinancialLedgerService ledgerService)
        {
            _ledgerService = ledgerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRecords(int page = 1, int pageSize = 10, [FromQuery] int? year = null, [FromQuery] string? search = null, [FromQuery] Domain.Enums.FinancialRecordType? type = null, [FromQuery] bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var records = await _ledgerService.GetRecordsAsync(page, pageSize, year, search, type, includeDeleted, cancellationToken);
            return Ok(records);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int year, CancellationToken cancellationToken)
        {
            var summary = await _ledgerService.GetSummaryAsync(year, cancellationToken);
            return Ok(summary);
        }

        [HttpPost]
        [GHCAA.API.Filters.RequireStepUp]
        public async Task<IActionResult> AddRecord([FromBody] FinancialRecord record, CancellationToken cancellationToken)
        {
            // 82.32: was silently falling through with whatever CreatedByAdminId the client
            // posted in the body when the claim failed to parse, so a forged attribution on
            // creation went uncaught. Every other write path in this controller already refuses
            // rather than proceeds in that situation.
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var adminId))
                return Unauthorized();
            record.CreatedByAdminId = adminId;

            var result = await _ledgerService.AddRecordAsync(record, cancellationToken);
            return CreatedAtAction(nameof(GetRecords), new { year = result.Year }, result);
        }

        [HttpPut("{id}")]
        [GHCAA.API.Filters.RequireStepUp]
        public async Task<IActionResult> UpdateRecord(int id, [FromBody] FinancialRecord record, CancellationToken cancellationToken)
        {
            // 82.16: the ledger records who edited a row, so an unidentifiable caller is refused
            // rather than written as admin 0. Same claim AddRecord already reads.
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var adminId))
                return Unauthorized();

            record.Id = id;
            var result = await _ledgerService.UpdateRecordAsync(record, adminId, cancellationToken);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [GHCAA.API.Filters.RequireStepUp]
        public async Task<IActionResult> DeleteRecord(int id, CancellationToken cancellationToken)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var adminId))
                return Unauthorized();

            var success = await _ledgerService.DeleteRecordAsync(id, adminId, cancellationToken);
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
