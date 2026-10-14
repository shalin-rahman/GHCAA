using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
    {
        public void Configure(EntityTypeBuilder<EmailLog> builder)
        {
            builder.HasKey(e => e.Id);

            // 98.14: the daily retention sweep deletes by SentDate. Without this index each pass
            // scans the whole table, which holds the full body of every email sent.
            builder.HasIndex(e => e.SentDate);
        }
    }
}
