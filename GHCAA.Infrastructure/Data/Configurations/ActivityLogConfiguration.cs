using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
    {
        public void Configure(EntityTypeBuilder<ActivityLog> builder)
        {
            builder.HasKey(a => a.Id);

            builder.HasIndex(a => new { a.MemberId, a.Timestamp });

            // 98.14: the retention sweep deletes by Timestamp across all members, which the
            // composite index above cannot serve.
            builder.HasIndex(a => a.Timestamp);
        }
    }
}
