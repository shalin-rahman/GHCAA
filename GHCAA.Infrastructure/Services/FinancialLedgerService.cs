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

        public async Task<object> GetRecordsAsync(int page = 1, int pageSize = 10, int? year = null, string? search = null, Enums.FinancialRecordType? type = null, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            // 82.32: page/pageSize were never validated. page=0 or negative produced a negative
            // SQL OFFSET (provider exception); pageSize=0 divided by zero below, and casting
            // double.PositiveInfinity to int is unspecified (yields int.MinValue), so the response
            // advertised a TotalPages of roughly -2.1 billion instead of failing cleanly.
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 200) pageSize = 200;

            // 82.32: soft-deleted rows were unreachable from every read path — the audit trail
            // 82.16 built had nothing that could read it back. includeDeleted is SuperAdmin-only
            // (this whole controller is), off by default so no existing caller's result changes.
            var query = includeDeleted
                ? _db.FinancialRecords.IgnoreQueryFilters().AsQueryable()
                : _db.FinancialRecords.AsQueryable();

            if (year.HasValue)
                query = query.Where(r => r.Year == year.Value);

            if (type.HasValue)
                query = query.Where(r => r.RecordType == type.Value);

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLower();
                query = query.Where(r =>
                    (r.Description != null && r.Description.ToLower().Contains(s)) ||
                    (r.Reference != null && r.Reference.ToLower().Contains(s)));
            }

            var totalItems = await query.CountAsync(cancellationToken);
            var items = await query.OrderByDescending(r => r.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new
            {
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Items = items
            };
        }

        public async Task<IEnumerable<FinancialRecord>> GetAllRecordsForExportAsync(int? year = null, CancellationToken cancellationToken = default)
        {
            var query = _db.FinancialRecords.AsQueryable();
            if (year.HasValue) query = query.Where(r => r.Year == year.Value);
            return await query.OrderByDescending(r => r.Date).ToListAsync(cancellationToken);
        }

        public async Task<FinancialRecord> AddRecordAsync(FinancialRecord record, CancellationToken cancellationToken = default)
        {
            // 82.32: the client posts a FinancialRecord body directly. Without this, an admin
            // could set isDeleted/deletedByAdminId/updatedAt/updatedByAdminId on creation — forging
            // an attribution the whole point of 82.16 was to make trustworthy. CreatedByAdminId is
            // not reset here: the controller sets it from the caller's claim after binding, and
            // that assignment must win over whatever the client posted, not this one.
            record.UpdatedAt = null;
            record.UpdatedByAdminId = null;
            record.IsDeleted = false;
            record.DeletedAt = null;
            record.DeletedByAdminId = null;

            record.Date = DateTime.SpecifyKind(record.Date, DateTimeKind.Utc);
            record.CreatedAt = DateTime.UtcNow;
            await _db.FinancialRecords.AddAsync(record, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return record;
        }

        public async Task<FinancialRecord?> UpdateRecordAsync(FinancialRecord record, int adminId, CancellationToken cancellationToken = default)
        {
            // 82.32: FindAsync now returns null for a soft-deleted row (the query filter hides it),
            // where it used to return null only for a row that never existed. Both cases return the
            // same "not found" result to the controller instead of throwing — a deleted ledger row
            // is not an edit target, but editing it must not 500 either.
            var existing = await _db.FinancialRecords.FindAsync(new object[] { record.Id }, cancellationToken);
            if (existing == null) return null;

            existing.Year = record.Year;
            existing.RecordType = record.RecordType;
            existing.FinancialCategory = record.FinancialCategory;
            existing.Date = DateTime.SpecifyKind(record.Date, DateTimeKind.Utc);
            existing.Amount = record.Amount;
            existing.Description = record.Description;
            existing.Reference = record.Reference;

            // 82.16: who changed this ledger row, and when. Set on the same save as the values, so
            // an amount can never be edited without the stamp landing with it.
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByAdminId = adminId;

            await _db.SaveChangesAsync(cancellationToken);
            return existing;
        }

        public async Task<bool> DeleteRecordAsync(int id, int adminId, CancellationToken cancellationToken = default)
        {
            var record = await _db.FinancialRecords.FindAsync(new object[] { id }, cancellationToken);
            if (record == null) return false;

            // 82.16: soft delete. This used to be _db.FinancialRecords.Remove(record), which
            // destroyed a record of money with no trace of its value or who removed it. The row
            // stays; FinancialRecordConfiguration's query filter hides it from ordinary reads, so
            // every existing caller behaves as before while the evidence survives.
            if (record.IsDeleted) return false;

            record.IsDeleted = true;
            record.DeletedAt = DateTime.UtcNow;
            record.DeletedByAdminId = adminId;

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
                .GroupBy(r => new { r.RecordType, r.FinancialCategory })
                .Select(g => new LedgerCategorySummaryDto
                {
                    Type = g.Key.RecordType.ToString(),
                    FinancialCategory = g.Key.FinancialCategory.ToString(),
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

        public async Task<byte[]> ExportRecordsAsync(int? year = null, CancellationToken cancellationToken = default)
        {
            var records = await GetAllRecordsForExportAsync(year, cancellationToken);

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Date,Type,Category,Amount,Description,Reference");

            foreach (var r in records)
            {
                csv.AppendLine($"{r.Date:yyyy-MM-dd},{r.RecordType},{r.FinancialCategory},{r.Amount},\"{EscapeCsvFormula(r.Description)}\",\"{EscapeCsvFormula(r.Reference)}\"");
            }

            return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        }

        // 82.32: Description/Reference are admin-entered free text, quoted for embedded quotes but
        // not neutralised for a leading =, +, - or @ — a value like =HYPERLINK("http://evil/",...)
        // executes as a formula the moment this CSV is opened in Excel/Sheets. Prefixing with a
        // single quote makes the cell content literal without changing what a human reading the
        // CSV sees.
        private static string EscapeCsvFormula(string? value)
        {
            var escaped = value?.Replace("\"", "\"\"") ?? string.Empty;
            return escaped.Length > 0 && (escaped[0] == '=' || escaped[0] == '+' || escaped[0] == '-' || escaped[0] == '@')
                ? "'" + escaped
                : escaped;
        }
    }
}
