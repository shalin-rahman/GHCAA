using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class MembershipFeeConfigConfiguration : IEntityTypeConfiguration<MembershipFeeConfig>
    {
        public void Configure(EntityTypeBuilder<MembershipFeeConfig> builder)
        {
            // 62.56g: one fee per category, membership type and effective date. The config pack
            // import matches rows on this key.
            builder.HasIndex(f => new { f.Category, f.MembershipType, f.EffectiveDate }).IsUnique();
        }
    }
}
