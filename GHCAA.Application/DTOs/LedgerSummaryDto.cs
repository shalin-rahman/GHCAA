using System.Collections.Generic;

namespace GHCAA.Application.DTOs
{
    public class LedgerSummaryDto
    {
        public int Year { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetBalance { get; set; }
        public IEnumerable<LedgerCategorySummaryDto> Details { get; set; } = new List<LedgerCategorySummaryDto>();
    }

    public class LedgerCategorySummaryDto
    {
        public string Type { get; set; } = null!;
        public string FinancialCategory { get; set; } = null!;
        public decimal Total { get; set; }
    }
}
