using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class FamilyLinkRequestConfiguration : IEntityTypeConfiguration<FamilyLinkRequest>
    {
        public void Configure(EntityTypeBuilder<FamilyLinkRequest> builder)
        {
            builder.HasKey(f => f.Id);

            builder.HasOne(f => f.Requester)
                .WithMany(m => m.SentFamilyLinkRequests)
                .HasForeignKey(f => f.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.TargetMember)
                .WithMany(m => m.ReceivedFamilyLinkRequests)
                .HasForeignKey(f => f.TargetMemberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(f => f.Requester != null && !f.Requester.IsArchived && f.TargetMember != null && !f.TargetMember.IsArchived);
        }
    }
}
