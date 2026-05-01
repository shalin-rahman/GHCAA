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

            builder.HasIndex(p => p.TransactionId)
                .IsUnique();

            builder.HasIndex(p => new { p.MemberId, p.TransactionId });

            builder.HasQueryFilter(ph => ph.Member != null && !ph.Member.IsArchived);
        }
    }
}
