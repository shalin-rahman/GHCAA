using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class PollOptionConfiguration : IEntityTypeConfiguration<PollOption>
    {
        public void Configure(EntityTypeBuilder<PollOption> builder)
        {
            builder.HasKey(o => o.Id);

            builder.HasOne(o => o.Poll)
                .WithMany(p => p.Options)
                .HasForeignKey(o => o.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            // Matches Poll's own HasQueryFilter(p => !p.IsArchived) — without this, EF warns
            // (10622) that an archived poll's required Poll navigation is unreachable.
            builder.HasQueryFilter(o => o.Poll != null && !o.Poll.IsArchived);
        }
    }
}
