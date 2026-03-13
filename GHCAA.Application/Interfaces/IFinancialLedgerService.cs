using GHCAA.Application.DTOs;
using GHCAA.Domain;
using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface IFinancialLedgerService
    {
        Task<object> GetRecordsAsync(int page = 1, int pageSize = 10, int? year = null, string? search = null, Enums.FinancialRecordType? type = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<FinancialRecord>> GetAllRecordsForExportAsync(int? year = null, CancellationToken cancellationToken = default);
        Task<FinancialRecord> AddRecordAsync(FinancialRecord record, CancellationToken cancellationToken = default);
        Task<FinancialRecord> UpdateRecordAsync(FinancialRecord record, CancellationToken cancellationToken = default);
        Task<bool> DeleteRecordAsync(int id, CancellationToken cancellationToken = default);
        
        Task<LedgerSummaryDto> GetSummaryAsync(int year, CancellationToken cancellationToken = default);
        Task<byte[]> ExportRecordsAsync(int? year = null, CancellationToken cancellationToken = default);
    }
}
