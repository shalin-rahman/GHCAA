using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class FileUploadConfiguration : IEntityTypeConfiguration<FileUpload>
    {
        public void Configure(EntityTypeBuilder<FileUpload> builder)
        {
            builder.HasKey(f => f.Id);

            builder.Property(f => f.FileName)
                .HasMaxLength(260);

            builder.HasIndex(f => new { f.MemberId, f.UploadType });

            builder.HasQueryFilter(f => f.Member != null && !f.Member.IsArchived);
        }
    }
}
