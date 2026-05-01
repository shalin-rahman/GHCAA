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

            builder.HasQueryFilter(em => em.Member != null && !em.Member.IsArchived);
        }
    }
}
