using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using NUnit.Framework;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services;

[TestFixture]
public sealed class ElectionServiceTests : TestBase
{
    [Test]
    public async Task FreezeVoterRollAsync_UsesMembershipAndDuesSnapshot()
    {
        var member = await CreateAndSaveTestMemberAsync();
        var period = new ECPeriod { Title = "2026", StartDate = DateTime.UtcNow.AddDays(-1) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        var election = new Election { Title = "Election", ECPeriodId = period.Id, NominationOpensOn = DateTime.UtcNow, NominationClosesOn = DateTime.UtcNow.AddDays(1), PollingOpensOn = DateTime.UtcNow.AddDays(2), PollingClosesOn = DateTime.UtcNow.AddDays(3) };
        _context.Elections.Add(election);
        await _context.SaveChangesAsync();

        var count = await new ElectionService(_context).FreezeVoterRollAsync(election.Id);

        count.Should().Be(1);
        _context.VoterRolls.Single().IsEligible.Should().BeTrue();
        _context.VoterRolls.Single().FrozenAt.Should().NotBe(default);
    }

    [Test]
    public async Task CastVoteAsync_RecordsVoteWithoutMemberOnBallotVote()
    {
        var voter = await CreateAndSaveTestMemberAsync();
        var candidate = await CreateAndSaveTestMemberAsync("Candidate", "candidate@example.com", "01712345679", "1234567891");
        var seconder = await CreateAndSaveTestMemberAsync("Seconder", "seconder@example.com", "01712345680", "1234567892");
        var period = new ECPeriod { Title = "2026", StartDate = DateTime.UtcNow.AddDays(-1) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        var election = new Election { Title = "Election", ECPeriodId = period.Id, Phase = ElectionPhase.Polling, NominationOpensOn = DateTime.UtcNow.AddDays(-3), NominationClosesOn = DateTime.UtcNow.AddDays(-2), PollingOpensOn = DateTime.UtcNow.AddHours(-1), PollingClosesOn = DateTime.UtcNow.AddHours(1) };
        _context.Elections.Add(election);
        await _context.SaveChangesAsync();
        var seat = new ElectionSeat { ElectionId = election.Id, Position = ECPosition.President };
        _context.ElectionSeats.Add(seat);
        _context.VoterRolls.Add(new VoterRoll { ElectionId = election.Id, MemberId = voter.Id, IsEligible = true, FrozenAt = DateTime.UtcNow });
        _context.VoterRolls.Add(new VoterRoll { ElectionId = election.Id, MemberId = candidate.Id, IsEligible = true, FrozenAt = DateTime.UtcNow });
        _context.VoterRolls.Add(new VoterRoll { ElectionId = election.Id, MemberId = seconder.Id, IsEligible = true, FrozenAt = DateTime.UtcNow });
        var nomination = new Nomination { ElectionId = election.Id, ElectionSeat = seat, CandidateMemberId = candidate.Id, ProposerMemberId = voter.Id, SeconderMemberId = seconder.Id, Statement = "Run", Status = NominationStatus.Accepted, SubmittedAt = DateTime.UtcNow };
        _context.Nominations.Add(nomination);
        await _context.SaveChangesAsync();

        var recorded = await new ElectionService(_context).CastVoteAsync(election.Id, voter.Id, new CastVoteDto(seat.Id, nomination.Id, null));

        recorded.Should().BeTrue();
        _context.BallotVotes.Single().Ballot.Should().NotBeNull();
        _context.BallotVotes.Single().Ballot!.ElectionId.Should().Be(election.Id);
        _context.BallotVotes.Single().Ballot!.Votes.Single().Ballot!.ElectionId.Should().Be(election.Id);
        _context.VoterRolls.Single(x => x.MemberId == voter.Id).VotedAt.Should().NotBeNull();
    }

    [Test]
    public async Task CastVoteAsync_RejectsReplayAfterTheFirstVote()
    {
        var voter = await CreateAndSaveTestMemberAsync();
        var candidate = await CreateAndSaveTestMemberAsync("Candidate", "candidate2@example.com", "01712345679", "1234567891");
        var period = new ECPeriod { Title = "2026", StartDate = DateTime.UtcNow.AddDays(-1) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        var election = new Election
        {
            Title = "Election",
            ECPeriodId = period.Id,
            Phase = ElectionPhase.Polling,
            NominationOpensOn = DateTime.UtcNow.AddDays(-3),
            NominationClosesOn = DateTime.UtcNow.AddDays(-2),
            PollingOpensOn = DateTime.UtcNow.AddHours(-1),
            PollingClosesOn = DateTime.UtcNow.AddHours(1)
        };
        var seat = new ElectionSeat { Election = election, Position = ECPosition.President };
        _context.Elections.Add(election);
        _context.ElectionSeats.Add(seat);
        _context.VoterRolls.Add(new VoterRoll { Election = election, MemberId = voter.Id, IsEligible = true, FrozenAt = DateTime.UtcNow });
        var nomination = new Nomination
        {
            Election = election,
            ElectionSeat = seat,
            CandidateMemberId = candidate.Id,
            ProposerMemberId = voter.Id,
            SeconderMemberId = voter.Id + 1,
            Statement = "Run",
            Status = NominationStatus.Accepted,
            SubmittedAt = DateTime.UtcNow
        };
        _context.Nominations.Add(nomination);
        await _context.SaveChangesAsync();

        var service = new ElectionService(_context);
        var request = new CastVoteDto(seat.Id, nomination.Id, null);

        (await service.CastVoteAsync(election.Id, voter.Id, request)).Should().BeTrue();
        (await service.CastVoteAsync(election.Id, voter.Id, request)).Should().BeFalse();
        _context.BallotVotes.Should().HaveCount(1);
    }
}
