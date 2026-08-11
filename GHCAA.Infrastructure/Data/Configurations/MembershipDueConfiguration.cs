using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class MembershipDueConfiguration : IEntityTypeConfiguration<MembershipDue>
    {
        public void Configure(EntityTypeBuilder<MembershipDue> builder)
        {
            builder.HasKey(d => d.Id);

            builder.HasQueryFilter(d => d.Member != null && !d.Member.IsArchived);
        }
    }
}
