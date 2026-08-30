using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class EventBudgetConfiguration : IEntityTypeConfiguration<EventBudget>
    {
        public void Configure(EntityTypeBuilder<EventBudget> builder)
        {
            builder.HasKey(b => b.Id);

            builder.HasOne(b => b.Event)
                .WithOne()
                .HasForeignKey<EventBudget>(b => b.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Matches AlumniEvent's own HasQueryFilter(e => e.IsActive) — without this, EF warns
            // (10622) that an inactive event's required Event navigation is unreachable.
            builder.HasQueryFilter(b => b.Event != null && b.Event.IsActive);
        }
    }
}
