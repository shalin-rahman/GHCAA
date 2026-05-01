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
        }
    }
}
