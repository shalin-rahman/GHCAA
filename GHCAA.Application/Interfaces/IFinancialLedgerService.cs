using GHCAA.Application.DTOs;
using GHCAA.Domain;
using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface IFinancialLedgerService
    {
        Task<IEnumerable<FinancialRecord>> GetRecordsAsync(int? year = null, Enums.FinancialRecordType? type = null, CancellationToken cancellationToken = default);
        Task<FinancialRecord> AddRecordAsync(FinancialRecord record, CancellationToken cancellationToken = default);
        Task<FinancialRecord> UpdateRecordAsync(FinancialRecord record, CancellationToken cancellationToken = default);
        Task<bool> DeleteRecordAsync(int id, CancellationToken cancellationToken = default);
        
        Task<LedgerSummaryDto> GetSummaryAsync(int year, CancellationToken cancellationToken = default);
        Task<byte[]> ExportRecordsAsync(int? year = null, CancellationToken cancellationToken = default);
    }
}
