using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations;

public sealed class ElectionConfiguration : IEntityTypeConfiguration<Election>
{
    public void Configure(EntityTypeBuilder<Election> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.HasIndex(x => new { x.ECPeriodId, x.IsActive });
        b.HasOne(x => x.ECPeriod).WithMany().HasForeignKey(x => x.ECPeriodId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ElectionSeatConfiguration : IEntityTypeConfiguration<ElectionSeat>
{
    public void Configure(EntityTypeBuilder<ElectionSeat> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ElectionId, x.Position }).IsUnique();
        b.HasOne(x => x.Election).WithMany(x => x.Seats).HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ElectionOfficerConfiguration : IEntityTypeConfiguration<ElectionOfficer>
{
    public void Configure(EntityTypeBuilder<ElectionOfficer> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ElectionId, x.MemberId, x.Role }).IsUnique();
        b.HasOne(x => x.Election).WithMany(x => x.Officers).HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class VoterRollConfiguration : IEntityTypeConfiguration<VoterRoll>
{
    public void Configure(EntityTypeBuilder<VoterRoll> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.IneligibilityReason).HasMaxLength(500);
        b.HasIndex(x => new { x.ElectionId, x.MemberId }).IsUnique();
        b.HasOne(x => x.Election).WithMany(x => x.VoterRoll).HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class NominationConfiguration : IEntityTypeConfiguration<Nomination>
{
    public void Configure(EntityTypeBuilder<Nomination> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Statement).HasMaxLength(4000).IsRequired();
        b.HasOne(x => x.Election).WithMany().HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ElectionSeat).WithMany(x => x.Nominations).HasForeignKey(x => x.ElectionSeatId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.ElectionSeatId, x.CandidateMemberId }).IsUnique();
    }
}

public sealed class ScrutinyDecisionConfiguration : IEntityTypeConfiguration<ScrutinyDecision>
{
    public void Configure(EntityTypeBuilder<ScrutinyDecision> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne<Nomination>().WithMany().HasForeignKey(x => x.NominationId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Member>().WithMany().HasForeignKey(x => x.OfficerMemberId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class BallotConfiguration : IEntityTypeConfiguration<Ballot>
{
    public void Configure(EntityTypeBuilder<Ballot> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.SerialNumber).HasMaxLength(80).IsRequired();
        b.HasIndex(x => new { x.ElectionId, x.SerialNumber }).IsUnique();
        b.HasOne(x => x.Election).WithMany().HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class BallotVoteConfiguration : IEntityTypeConfiguration<BallotVote>
{
    public void Configure(EntityTypeBuilder<BallotVote> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Ballot).WithMany(x => x.Votes).HasForeignKey(x => x.BallotId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Nomination>().WithMany().HasForeignKey(x => x.NominationId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ElectionResultConfiguration : IEntityTypeConfiguration<ElectionResult>
{
    public void Configure(EntityTypeBuilder<ElectionResult> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ElectionId, x.ElectionSeatId, x.NominationId }).IsUnique();
        b.HasOne<Election>().WithMany().HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Cascade);
    }
}
