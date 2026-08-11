using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class MembershipHistoryConfiguration : IEntityTypeConfiguration<MembershipHistory>
    {
        public void Configure(EntityTypeBuilder<MembershipHistory> builder)
        {
            builder.HasKey(h => h.Id);

            builder.HasQueryFilter(h => h.Member != null && !h.Member.IsArchived);
        }
    }
}
