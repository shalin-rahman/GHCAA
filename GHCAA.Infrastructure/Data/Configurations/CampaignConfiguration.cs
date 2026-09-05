using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
    {
        public void Configure(EntityTypeBuilder<Campaign> builder)
        {
            builder.HasKey(c => c.Id);

            // The public URL is /campaigns/:slug — a slug collision would make one campaign's page
            // resolve to another's.
            builder.HasIndex(c => c.Slug).IsUnique();

            builder.HasMany(c => c.Pledges)
                .WithOne(p => p.Campaign)
                .HasForeignKey(p => p.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
