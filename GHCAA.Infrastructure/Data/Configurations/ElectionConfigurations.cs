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
        b.Property(x => x.BallotPublicKey).HasMaxLength(2000);
        b.Property(x => x.BallotKeyFingerprint).HasMaxLength(64);
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

public sealed class ElectionAppointmentConfiguration : IEntityTypeConfiguration<ElectionAppointment>
{
    public void Configure(EntityTypeBuilder<ElectionAppointment> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.DisplayName).HasMaxLength(150).IsRequired();
        b.Property(x => x.Email).HasMaxLength(256).IsRequired();
        b.Property(x => x.Phone).HasMaxLength(30);
        b.Property(x => x.DeclarationTextSnapshot).HasMaxLength(4000);
        b.Property(x => x.SignedFromIp).HasMaxLength(64);
        b.Property(x => x.RevokedReason).HasMaxLength(500);
        // Quoted identifiers, so the same filter works on Postgres and on the Sqlite test database.
        b.HasIndex(x => new { x.ElectionId, x.PersonaId, x.UserId }).IsUnique().HasFilter("\"RevokedAt\" IS NULL");
        b.HasIndex(x => x.UserId);
        b.HasOne(x => x.Election).WithMany(x => x.Appointments).HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.PersonaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Member>().WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ElectionApprovalConfiguration : IEntityTypeConfiguration<ElectionApproval>
{
    public void Configure(EntityTypeBuilder<ElectionApproval> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.PayloadJson).HasMaxLength(8000);
        b.Property(x => x.RejectReason).HasMaxLength(500);
        b.HasIndex(x => new { x.ElectionId, x.Action, x.ExecutedAt, x.RejectedAt });
        b.HasOne(x => x.Election).WithMany().HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.RejectedByUserId).OnDelete(DeleteBehavior.Restrict);
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
        b.HasOne<User>().WithMany().HasForeignKey(x => x.DecidedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class BallotConfiguration : IEntityTypeConfiguration<Ballot>
{
    public void Configure(EntityTypeBuilder<Ballot> b)
    {
        b.HasKey(x => x.Id);
        // Npgsql makes time-ordered GUIDs by default, which would put the vote order back into
        // the key. The service sets a random Guid.NewGuid() instead.
        b.Property(x => x.Id).ValueGeneratedNever();
        b.HasIndex(x => x.ElectionId);
        b.HasOne(x => x.Election).WithMany().HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class BallotVoteConfiguration : IEntityTypeConfiguration<BallotVote>
{
    public void Configure(EntityTypeBuilder<BallotVote> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.HasIndex(x => new { x.BallotId, x.ElectionSeatId, x.NominationId }).IsUnique();
        b.HasOne(x => x.Ballot).WithMany(x => x.Votes).HasForeignKey(x => x.BallotId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<ElectionSeat>().WithMany().HasForeignKey(x => x.ElectionSeatId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Nomination>().WithMany().HasForeignKey(x => x.NominationId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PendingBallotConfiguration : IEntityTypeConfiguration<PendingBallot>
{
    public void Configure(EntityTypeBuilder<PendingBallot> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.SealedChoices).IsRequired();
        b.HasIndex(x => x.ElectionId);
        b.HasOne<Election>().WithMany().HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class BallotReceiptConfiguration : IEntityTypeConfiguration<BallotReceipt>
{
    public void Configure(EntityTypeBuilder<BallotReceipt> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.TrackingCode).HasMaxLength(80).IsRequired();
        b.HasIndex(x => new { x.ElectionId, x.TrackingCode }).IsUnique();
        b.HasOne<Election>().WithMany().HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class SeatVoteConfiguration : IEntityTypeConfiguration<SeatVote>
{
    public void Configure(EntityTypeBuilder<SeatVote> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ElectionId, x.ElectionSeatId, x.MemberId }).IsUnique();
        b.HasOne(x => x.Election).WithMany().HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<ElectionSeat>().WithMany().HasForeignKey(x => x.ElectionSeatId).OnDelete(DeleteBehavior.Restrict);
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

public sealed class ElectionPersonaConfiguration : IEntityTypeConfiguration<ElectionPersona>
{
    public void Configure(EntityTypeBuilder<ElectionPersona> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.GroupName).HasMaxLength(100).IsRequired();
        b.Property(x => x.Description).HasMaxLength(500).IsRequired();
        b.Property(x => x.DeclarationText).HasMaxLength(4000).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
    }
}
