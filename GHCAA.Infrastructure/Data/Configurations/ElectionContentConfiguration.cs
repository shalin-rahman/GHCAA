using GHCAA.Domain;
using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations;

public sealed class ElectionContentConfiguration : IEntityTypeConfiguration<ElectionContent>
{
    public void Configure(EntityTypeBuilder<ElectionContent> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Key).HasMaxLength(Constants.Elections.ContentKeyMaxLength).IsRequired();
        b.Property(x => x.Title).HasMaxLength(Constants.Elections.ContentTitleMaxLength).IsRequired();
        b.Property(x => x.Summary).HasMaxLength(Constants.Elections.ContentSummaryMaxLength);
        b.Property(x => x.Group).HasMaxLength(Constants.Elections.ContentGroupMaxLength);
        b.Property(x => x.Body).IsRequired();
        // A page and a form may share a key: the page "er-25" is the guide to form ER-25.
        b.HasIndex(x => new { x.Kind, x.Key }).IsUnique();
    }
}
