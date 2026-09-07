using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class EventExpenseConfiguration : IEntityTypeConfiguration<EventExpense>
    {
        public void Configure(EntityTypeBuilder<EventExpense> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.Budget)
                .WithMany(b => b.Expenses)
                .HasForeignKey(e => e.EventBudgetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Mirrors EventBudgetConfiguration's own filter one level up: EventBudget hides rows
            // whose Event is archived, so a required child that doesn't repeat the same filter
            // makes EF warn that its required parent can be filtered out from under it.
            builder.HasQueryFilter(e => e.Budget != null && e.Budget.Event != null && e.Budget.Event.IsActive);
        }
    }
}
