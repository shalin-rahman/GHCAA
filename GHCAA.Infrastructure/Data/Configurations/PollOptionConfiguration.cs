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
        }
    }
}
