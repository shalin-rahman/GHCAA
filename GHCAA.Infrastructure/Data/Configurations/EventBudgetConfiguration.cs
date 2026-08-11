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
        }
    }
}
