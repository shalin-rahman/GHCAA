using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class AmendmentVoteConfiguration : IEntityTypeConfiguration<AmendmentVote>
    {
        public void Configure(EntityTypeBuilder<AmendmentVote> builder)
        {
            builder.HasKey(v => v.Id);

            builder.HasOne(v => v.Constitution)
                .WithMany(c => c.Votes)
                .HasForeignKey(v => v.ConstitutionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(v => v.Member)
                .WithMany()
                .HasForeignKey(v => v.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(v => new { v.ConstitutionId, v.MemberId })
                .IsUnique();

            // Matches Member's own HasQueryFilter(m => !m.IsArchived) — without this, EF warns
            // (10622) that an archived member's required Member navigation is unreachable, since
            // Member's filter excludes it while this entity has none.
            builder.HasQueryFilter(v => v.Member != null && !v.Member.IsArchived);
        }
    }
}
