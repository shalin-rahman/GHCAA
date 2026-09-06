using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);

            // 82.22: GetUserNotificationsAsync filters on MemberId then sorts by CreatedAt on every
            // portal page load, and this table grows one row per member per broadcast — the fastest
            // grower of the 46 entities, so it's the one that needs an index soonest.
            builder.HasIndex(n => new { n.MemberId, n.CreatedAt });

            builder.HasQueryFilter(n => n.Member != null && !n.Member.IsArchived);
        }
    }
}
