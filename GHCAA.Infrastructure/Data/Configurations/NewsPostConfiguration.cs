using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class NewsPostConfiguration : IEntityTypeConfiguration<NewsPost>
    {
        public void Configure(EntityTypeBuilder<NewsPost> builder)
        {
            builder.HasKey(n => n.Id);

            builder.HasQueryFilter(np => np.Author != null && !np.Author.IsArchived);
        }
    }
}
