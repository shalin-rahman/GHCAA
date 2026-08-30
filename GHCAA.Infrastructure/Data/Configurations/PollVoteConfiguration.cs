using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class PollVoteConfiguration : IEntityTypeConfiguration<PollVote>
    {
        public void Configure(EntityTypeBuilder<PollVote> builder)
        {
            builder.HasKey(v => v.Id);

            builder.HasOne(v => v.Poll)
                .WithMany(p => p.Votes)
                .HasForeignKey(v => v.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(v => v.PollOption)
                .WithMany()
                .HasForeignKey(v => v.PollOptionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(v => v.Member)
                .WithMany()
                .HasForeignKey(v => v.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(v => new { v.PollOptionId, v.MemberId })
                .IsUnique();

            // Matches Member's own HasQueryFilter(m => !m.IsArchived) — without this, EF warns
            // (10622) that an archived member's required Member navigation is unreachable.
            builder.HasQueryFilter(v => v.Member != null && !v.Member.IsArchived);
        }
    }
}
