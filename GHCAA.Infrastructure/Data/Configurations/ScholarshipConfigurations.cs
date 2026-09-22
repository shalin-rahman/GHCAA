using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations;

public sealed class ScholarshipFundConfiguration : IEntityTypeConfiguration<ScholarshipFund>
{
    public void Configure(EntityTypeBuilder<ScholarshipFund> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        b.Property(x => x.NamedAfter).HasMaxLength(200);
    }
}

public sealed class ScholarshipCallConfiguration : IEntityTypeConfiguration<ScholarshipCall>
{
    public void Configure(EntityTypeBuilder<ScholarshipCall> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.AcademicYear).HasMaxLength(30).IsRequired();
        b.Property(x => x.EligibilityCriteria).HasMaxLength(4000).IsRequired();
        b.HasOne(x => x.ScholarshipFund).WithMany(x => x.Calls).HasForeignKey(x => x.ScholarshipFundId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ScholarshipApplicationConfiguration : IEntityTypeConfiguration<ScholarshipApplication>
{
    public void Configure(EntityTypeBuilder<ScholarshipApplication> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.ReferenceCode).HasMaxLength(40).IsRequired();
        b.HasIndex(x => x.ReferenceCode).IsUnique();
        b.Property(x => x.ApplicantEmail).HasMaxLength(320).IsRequired();
        b.Property(x => x.ApplicantName).HasMaxLength(200).IsRequired();
        b.Property(x => x.NeedStatement).HasMaxLength(4000).IsRequired();
        b.Property(x => x.MeritStatement).HasMaxLength(4000).IsRequired();
        b.HasOne(x => x.ScholarshipCall).WithMany(x => x.Applications).HasForeignKey(x => x.ScholarshipCallId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ScholarshipDocumentConfiguration : IEntityTypeConfiguration<ScholarshipDocument>
{
    public void Configure(EntityTypeBuilder<ScholarshipDocument> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.DocumentType).HasMaxLength(100).IsRequired();
        b.HasIndex(x => new { x.ScholarshipApplicationId, x.FileUploadId }).IsUnique();
        b.HasOne(x => x.ScholarshipApplication).WithMany(x => x.Documents).HasForeignKey(x => x.ScholarshipApplicationId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.FileUpload).WithMany().HasForeignKey(x => x.FileUploadId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ScholarshipReviewConfiguration : IEntityTypeConfiguration<ScholarshipReview>
{
    public void Configure(EntityTypeBuilder<ScholarshipReview> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Comments).HasMaxLength(2000);
        b.HasIndex(x => new { x.ScholarshipApplicationId, x.ReviewerMemberId }).IsUnique();
        b.HasOne(x => x.ScholarshipApplication).WithMany(x => x.Reviews).HasForeignKey(x => x.ScholarshipApplicationId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ReviewerMember).WithMany().HasForeignKey(x => x.ReviewerMemberId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ScholarshipAwardConfiguration : IEntityTypeConfiguration<ScholarshipAward>
{
    public void Configure(EntityTypeBuilder<ScholarshipAward> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.ScholarshipApplicationId).IsUnique();
        b.HasIndex(x => x.FinancialRecordId).IsUnique().HasFilter("\"FinancialRecordId\" IS NOT NULL");
        b.HasOne(x => x.ScholarshipApplication).WithOne(x => x.Award).HasForeignKey<ScholarshipAward>(x => x.ScholarshipApplicationId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.FinancialRecord).WithMany().HasForeignKey(x => x.FinancialRecordId).OnDelete(DeleteBehavior.Restrict);
    }
}
