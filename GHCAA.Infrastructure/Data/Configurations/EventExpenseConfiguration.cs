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
        }
    }
}
