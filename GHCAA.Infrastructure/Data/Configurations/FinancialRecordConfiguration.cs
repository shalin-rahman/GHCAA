using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    // FinancialRecord had no configuration of its own until 82.16 — it was mapped by convention
    // alone, which is also why it carried no indexes despite Year/RecordType being the columns
    // every ledger screen filters on (that gap is 82.22, deliberately left to its own item).
    public class FinancialRecordConfiguration : IEntityTypeConfiguration<FinancialRecord>
    {
        public void Configure(EntityTypeBuilder<FinancialRecord> builder)
        {
            builder.HasKey(r => r.Id);

            // 82.22: FinancialLedgerService.GetSummaryAsync/GetRecordsAsync filter on Year and,
            // optionally, RecordType/FinancialCategory on every ledger page load and CSV export.
            builder.HasIndex(r => new { r.Year, r.RecordType, r.FinancialCategory });

            // Deliberately NOT setting Amount's precision here. It reads as an obvious improvement
            // — money should be numeric(18,2) — but the column is currently unconstrained `numeric`
            // with live rows in it, so constraining it silently rounds anything holding more than
            // two decimal places. Introducing a lossy alteration to a money column inside the very
            // change meant to protect financial records would be self-defeating. If it is wanted,
            // it needs its own item, a check for out-of-range values first, and its own migration.

            // 82.16: a soft-deleted ledger row stays in the table as evidence but must not reach a
            // caller that did not deliberately ask for it. Every existing read of FinancialRecords
            // keeps working unchanged; only the delete path's meaning changed.
            builder.HasQueryFilter(r => !r.IsDeleted);
        }
    }
}
