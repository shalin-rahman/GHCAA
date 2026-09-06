using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class ECMemberConfiguration : IEntityTypeConfiguration<ECMember>
    {
        public void Configure(EntityTypeBuilder<ECMember> builder)
        {
            builder.HasKey(em => em.Id);

            // 82.29: !em.IsDeleted keeps a permanently-deleted (wrong-entry) row out of ordinary
            // reads, the same way PaymentHistoryConfiguration does for its own hard-delete path.
            // Admin views that need the deleted rows use IgnoreQueryFilters() deliberately.
            builder.HasQueryFilter(em => em.Member != null && !em.Member.IsArchived && !em.IsDeleted);
        }
    }
}
