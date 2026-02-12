using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    public class FinancialLedgerService : IFinancialLedgerService
    {
        private readonly ApplicationDbContext _db;

        public FinancialLedgerService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<FinancialRecord>> GetRecordsAsync(int? year = null, Enums.FinancialRecordType? type = null, CancellationToken cancellationToken = default)
        {
            var query = _db.FinancialRecords.AsQueryable();

            if (year.HasValue)
                query = query.Where(r => r.Year == year.Value);

            if (type.HasValue)
                query = query.Where(r => r.RecordType == type.Value);

            return await query.OrderByDescending(r => r.Date).ToListAsync(cancellationToken);
        }

        public async Task<FinancialRecord> AddRecordAsync(FinancialRecord record, CancellationToken cancellationToken = default)
        {
            record.CreatedAt = DateTime.UtcNow;
            await _db.FinancialRecords.AddAsync(record, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return record;
        }

        public async Task<FinancialRecord> UpdateRecordAsync(FinancialRecord record, CancellationToken cancellationToken = default)
        {
            var existing = await _db.FinancialRecords.FindAsync(new object[] { record.Id }, cancellationToken);
            if (existing == null) throw new KeyNotFoundException("Record not found");

            existing.Year = record.Year;
            existing.RecordType = record.RecordType;
            existing.Category = record.Category;
            existing.Date = record.Date;
            existing.Amount = record.Amount;
            existing.Description = record.Description;
            existing.Reference = record.Reference;

            await _db.SaveChangesAsync(cancellationToken);
            return existing;
        }

        public async Task<bool> DeleteRecordAsync(int id, CancellationToken cancellationToken = default)
        {
            var record = await _db.FinancialRecords.FindAsync(new object[] { id }, cancellationToken);
            if (record == null) return false;

            _db.FinancialRecords.Remove(record);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<LedgerSummaryDto> GetSummaryAsync(int year, CancellationToken cancellationToken = default)
        {
            var records = await _db.FinancialRecords
                .Where(r => r.Year == year)
                .ToListAsync(cancellationToken);

            var totalIncome = records.Where(r => r.RecordType == Enums.FinancialRecordType.Income).Sum(r => r.Amount);
            var totalExpense = records.Where(r => r.RecordType == Enums.FinancialRecordType.Expense).Sum(r => r.Amount);

            var byCategory = records
                .GroupBy(r => new { r.RecordType, r.Category })
                .Select(g => new LedgerCategorySummaryDto
                {
                    Type = g.Key.RecordType.ToString(),
                    Category = g.Key.Category.ToString(),
                    Total = g.Sum(r => r.Amount)
                });

            return new LedgerSummaryDto
            {
                Year = year,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                NetBalance = totalIncome - totalExpense,
                Details = byCategory.ToList()
            };
        }
    }
}
