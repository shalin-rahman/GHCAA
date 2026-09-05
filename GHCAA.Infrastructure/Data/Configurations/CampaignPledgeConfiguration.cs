using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class CampaignPledgeConfiguration : IEntityTypeConfiguration<CampaignPledge>
    {
        public void Configure(EntityTypeBuilder<CampaignPledge> builder)
        {
            builder.HasKey(p => p.Id);

            // MemberId is nullable — a guest donor has no Member row, same reasoning as
            // EventRegistration.MemberId and (post-82.32) PaymentHistory.MemberId.
            builder.HasOne(p => p.Member)
                .WithMany()
                .HasForeignKey(p => p.MemberId)
                .OnDelete(DeleteBehavior.SetNull);

            // Optional, set only once an admin confirms receipt. Matching MembershipDue's own
            // nullable FK to PaymentHistory: an optional relationship to a soft-deleted-capable
            // entity does not trip EF's required-navigation-to-filtered-entity warning.
            builder.HasOne(p => p.FinancialRecord)
                .WithMany()
                .HasForeignKey(p => p.FinancialRecordId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(p => p.CampaignId);
        }
    }
}
