using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class ArchiveItemConfiguration : IEntityTypeConfiguration<ArchiveItem>
    {
        public void Configure(EntityTypeBuilder<ArchiveItem> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Narrator).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Transcript).HasColumnType("text");
            builder.Property(x => x.Summary).HasMaxLength(4000);
            builder.Property(x => x.MediaUrl).HasMaxLength(1000);
            builder.HasIndex(x => new { x.PublicationState, x.ModerationState });
            builder.HasIndex(x => x.Transcript);
            builder.HasOne(x => x.LinkedMember).WithMany().HasForeignKey(x => x.LinkedMemberId).OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(x => x.FileUpload).WithMany().HasForeignKey(x => x.FileUploadId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
