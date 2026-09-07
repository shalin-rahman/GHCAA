using GHCAA.Application.DTOs;
using GHCAA.Domain;
using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface IFinancialLedgerService
    {
        // 82.32: includeDeleted, off by default, is the only way anything can read the 82.16
        // audit trail back — every other query in the codebase now hides soft-deleted rows.
        Task<object> GetRecordsAsync(int page = 1, int pageSize = 10, int? year = null, string? search = null, Enums.FinancialRecordType? type = null, bool includeDeleted = false, CancellationToken cancellationToken = default);
        Task<IEnumerable<FinancialRecord>> GetAllRecordsForExportAsync(int? year = null, CancellationToken cancellationToken = default);
        Task<FinancialRecord> AddRecordAsync(FinancialRecord record, CancellationToken cancellationToken = default);
        // 82.16: adminId is required, not optional, because an audit trail that a caller can
        // silently omit is not an audit trail. Both operations record who acted and when.
        // 82.32: null return means the id does not exist or names a soft-deleted row — both read
        // as "not found" to a caller trying to edit it.
        Task<FinancialRecord?> UpdateRecordAsync(FinancialRecord record, int adminId, CancellationToken cancellationToken = default);

        // Soft delete. The row survives with IsArchived set and is hidden by the entity's query
        // filter; nothing removes a ledger row from the table.
        Task<bool> DeleteRecordAsync(int id, int adminId, CancellationToken cancellationToken = default);

        Task<LedgerSummaryDto> GetSummaryAsync(int year, CancellationToken cancellationToken = default);
        Task<byte[]> ExportRecordsAsync(int? year = null, CancellationToken cancellationToken = default);
    }
}
