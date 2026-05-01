using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class SavedPaymentMethodConfiguration : IEntityTypeConfiguration<SavedPaymentMethod>
    {
        public void Configure(EntityTypeBuilder<SavedPaymentMethod> builder)
        {
            builder.HasKey(s => s.Id);

            builder.HasOne(s => s.Member)
                .WithMany()
                .HasForeignKey(s => s.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(s => s.Member != null && !s.Member.IsArchived);
        }
    }
}
