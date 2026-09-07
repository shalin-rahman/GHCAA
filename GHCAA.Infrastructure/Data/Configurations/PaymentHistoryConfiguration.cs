using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class PaymentHistoryConfiguration : IEntityTypeConfiguration<PaymentHistory>
    {
        public void Configure(EntityTypeBuilder<PaymentHistory> builder)
        {
            builder.HasKey(p => p.Id);

            builder.HasOne(p => p.Member)
                .WithMany(m => m.PaymentHistories)
                .HasForeignKey(p => p.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // 82.32: TransactionId is a real bank/gateway reference, so it must stay unique among
            // live rows, but a soft-deleted payment must not permanently occupy it — reusing the
            // same TransactionId after an admin deletes a wrong or duplicate entry is the ordinary
            // case this exists to fix, not an edge case.
            builder.HasIndex(p => p.TransactionId)
                .IsUnique()
                .HasFilter("\"IsArchived\" = false");

            builder.HasIndex(p => new { p.MemberId, p.TransactionId });

            // 24.13: Partial unique index on GatewayPaymentId prevents duplicate callback processing.
            // 82.32: excludes soft-deleted rows for the same reason as TransactionId above.
            builder.HasIndex(p => p.GatewayPaymentId)
                .IsUnique()
                .HasFilter("\"GatewayPaymentId\" IS NOT NULL AND \"IsArchived\" = false");

            // 82.16 added `!ph.IsArchived`: a soft-deleted payment must not appear in any ordinary
            // read, or "delete" would stop meaning delete to every caller that already exists.
            // 82.32: MemberId is nullable (guest event payments), so `Member == null` must pass this
            // filter rather than hide every guest row from the ledger — the prior version silently
            // dropped them because the null-check was written the wrong way round.
            // Admin views that need to see deleted rows use IgnoreQueryFilters() deliberately.
            builder.HasQueryFilter(ph => (ph.Member == null || !ph.Member.IsArchived) && !ph.IsArchived);
        }
    }
}
