using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class EventTaskConfiguration : IEntityTypeConfiguration<EventTask>
    {
        public void Configure(EntityTypeBuilder<EventTask> builder)
        {
            builder.HasKey(t => t.Id);

            builder.HasOne(t => t.AssignedMember)
                .WithMany()
                .HasForeignKey(t => t.AssignedMemberId)
                .OnDelete(DeleteBehavior.SetNull);

            // Matches AlumniEvent's own HasQueryFilter(e => e.IsActive) — without this, EF warns
            // (10622) that an inactive event's required Event navigation is unreachable.
            builder.HasQueryFilter(t => t.Event != null && t.Event.IsActive);
        }
    }
}
