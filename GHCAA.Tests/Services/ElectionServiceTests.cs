using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services;

[TestFixture]
public sealed class ElectionServiceTests : TestBase
{
    // One 3072-bit key for the whole fixture. Making a new one per test is slow.
    private static readonly RSA OfficerKey = RSA.Create(Constants.Elections.BallotKeyMinBits);
    private static readonly string OfficerPublicKey = Convert.ToBase64String(OfficerKey.ExportSubjectPublicKeyInfo());
    private static readonly string OfficerPrivateKey = Convert.ToBase64String(OfficerKey.ExportPkcs8PrivateKey());

    [OneTimeTearDown]
    public void DisposeKey() => OfficerKey.Dispose();

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

    private async Task<Election> CreateFrozenElectionAsync(DateTime nominationClosesOn)
    {
        await CreateAndSaveTestMemberAsync();
        var period = new ECPeriod { Title = "2026", StartDate = DateTime.UtcNow.AddDays(-1) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        var election = new Election { Title = "Election", ECPeriodId = period.Id, Phase = ElectionPhase.Nomination, NominationOpensOn = DateTime.UtcNow, NominationClosesOn = nominationClosesOn, PollingOpensOn = nominationClosesOn.AddDays(2), PollingClosesOn = nominationClosesOn.AddDays(3) };
        _context.Elections.Add(election);
        await _context.SaveChangesAsync();
        await new ElectionService(_context).FreezeVoterRollAsync(election.Id);
        return election;
    }

    [Test]
    public async Task UnfreezeVoterRollAsync_DeletesTheRollAndWritesAnAuditRow()
    {
        var election = await CreateFrozenElectionAsync(DateTime.UtcNow.AddDays(10));

        var (ok, error, removed) = await new ElectionService(_context).UnfreezeVoterRollAsync(election.Id, 7, " Wrong roll ");

        ok.Should().BeTrue();
        error.Should().BeNull();
        removed.Should().Be(1);
        _context.VoterRolls.Should().BeEmpty();
        var row = _context.ElectionAuditEntries.Single(a => a.Action == Constants.Elections.AuditActions.VoterRollUnfrozen);
        row.ActorUserId.Should().Be(7);
        row.DetailJson.Should().Contain("Wrong roll");
    }

    [Test]
    public async Task UnfreezeVoterRollAsync_RefusesABlankReason()
    {
        var election = await CreateFrozenElectionAsync(DateTime.UtcNow.AddDays(10));

        var (ok, error, _) = await new ElectionService(_context).UnfreezeVoterRollAsync(election.Id, 7, "  ");

        ok.Should().BeFalse();
        error.Should().Be("reason-required");
        _context.VoterRolls.Should().HaveCount(1);
    }

    [Test]
    public async Task UnfreezeVoterRollAsync_RefusesAfterNominationsOpenPhase()
    {
        var election = await CreateFrozenElectionAsync(DateTime.UtcNow.AddDays(10));
        election.Phase = ElectionPhase.Scrutiny;
        await _context.SaveChangesAsync();

        var (ok, error, _) = await new ElectionService(_context).UnfreezeVoterRollAsync(election.Id, 7, "Mistake");

        ok.Should().BeFalse();
        error.Should().Be("phase-closed");
    }

    [Test]
    public async Task UnfreezeVoterRollAsync_RefusesInsideTheCutoffWindow()
    {
        // Default cutoff is 2 days, so a close date one day away is already too late.
        var election = await CreateFrozenElectionAsync(DateTime.UtcNow.AddDays(1));

        var (ok, error, _) = await new ElectionService(_context).UnfreezeVoterRollAsync(election.Id, 7, "Mistake");

        ok.Should().BeFalse();
        error.Should().Be("window-closed");
        _context.VoterRolls.Should().HaveCount(1);
    }

    [Test]
    public async Task UnfreezeVoterRollAsync_RefusesOnceANominationExists()
    {
        var election = await CreateFrozenElectionAsync(DateTime.UtcNow.AddDays(10));
        var seat = new ElectionSeat { ElectionId = election.Id, Position = ECPosition.President };
        _context.ElectionSeats.Add(seat);
        await _context.SaveChangesAsync();
        _context.Nominations.Add(new Nomination { ElectionId = election.Id, ElectionSeatId = seat.Id, CandidateMemberId = 1, ProposerMemberId = 1, SeconderMemberId = 1, Statement = "x", SubmittedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var (ok, error, _) = await new ElectionService(_context).UnfreezeVoterRollAsync(election.Id, 7, "Mistake");

        ok.Should().BeFalse();
        error.Should().Be("has-nominations");
        _context.VoterRolls.Should().HaveCount(1);
    }

    [Test]
    public async Task UnfreezeVoterRollAsync_ReturnsNotFoundForAnUnknownElection()
    {
        var (ok, error, _) = await new ElectionService(_context).UnfreezeVoterRollAsync(9999, 7, "Mistake");

        ok.Should().BeFalse();
        error.Should().Be("not-found");
    }

    [Test]
    public async Task GetCurrentAsync_ReturnsElectionInAnActivePhase()
    {
        var period = new ECPeriod { Title = "2026", StartDate = DateTime.UtcNow.AddDays(-1) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        var election = new Election { Title = "Election", ECPeriodId = period.Id, Phase = ElectionPhase.Nomination, NominationOpensOn = DateTime.UtcNow, NominationClosesOn = DateTime.UtcNow.AddDays(1), PollingOpensOn = DateTime.UtcNow.AddDays(2), PollingClosesOn = DateTime.UtcNow.AddDays(3) };
        _context.Elections.Add(election);
        await _context.SaveChangesAsync();

        var result = await new ElectionService(_context).GetCurrentAsync();

        result.Should().NotBeNull();
        result!.Id.Should().Be(election.Id);
    }

    [Test]
    public async Task GetCurrentAsync_ReturnsNullWhenNoElectionIsActive()
    {
        var period = new ECPeriod { Title = "2026", StartDate = DateTime.UtcNow.AddDays(-1) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        var election = new Election { Title = "Election", ECPeriodId = period.Id, Phase = ElectionPhase.Declared, NominationOpensOn = DateTime.UtcNow, NominationClosesOn = DateTime.UtcNow.AddDays(1), PollingOpensOn = DateTime.UtcNow.AddDays(2), PollingClosesOn = DateTime.UtcNow.AddDays(3) };
        _context.Elections.Add(election);
        await _context.SaveChangesAsync();

        var result = await new ElectionService(_context).GetCurrentAsync();

        result.Should().BeNull();
    }

    [Test]
    [Category("FR-39")]
    public async Task CastBallotAsync_RecordsTheWholeBallotAndMarksTheVoter()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);

        var (success, error, code) = await new ElectionService(_context).CastBallotAsync(poll.Election.Id, poll.Voters[0].Id, poll.BallotFor(0));

        success.Should().BeTrue(error);
        code.Should().MatchRegex("^[0-9A-Z]{4}-[0-9A-Z]{4}-[0-9A-Z]{4}$");
        _context.PendingBallots.Should().ContainSingle();
        _context.BallotReceipts.Should().ContainSingle(x => x.TrackingCode == code);
        _context.SeatVotes.Should().HaveCount(2);
        _context.VoterRolls.Single(x => x.MemberId == poll.Voters[0].Id).VotedAt.Should().NotBeNull();
    }

    [Test]
    [Category("FR-39")]
    public void BallotTables_HaveNoMemberOrTimeColumn()
    {
        foreach (var type in new[] { typeof(Ballot), typeof(BallotVote), typeof(PendingBallot), typeof(BallotReceipt) })
            type.GetProperties().Should().NotContain(p =>
                p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?) || p.Name.Contains("Member"),
                $"{type.Name} must not tie a ballot to a voter or a time");
    }

    [Test]
    [Category("NFR-R5")]
    public async Task CastBallotAsync_RejectsReplayAfterTheFirstVote()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        var service = new ElectionService(_context);

        (await service.CastBallotAsync(poll.Election.Id, poll.Voters[0].Id, poll.BallotFor(0))).Success.Should().BeTrue();
        var replay = await service.CastBallotAsync(poll.Election.Id, poll.Voters[0].Id, poll.BallotFor(0));

        replay.Success.Should().BeFalse();
        replay.Error.Should().Be("already-voted");
        _context.PendingBallots.Should().HaveCount(1);
        _context.SeatVotes.Should().HaveCount(2);
    }

    [Test]
    [Category("FR-39")]
    public async Task CastBallotAsync_StoresOnlySealedChoicesOfAFixedSize()
    {
        var poll = await CreatePollingElectionAsync(voters: 2);
        var service = new ElectionService(_context);
        await service.CastBallotAsync(poll.Election.Id, poll.Voters[0].Id, poll.BallotFor(0));
        await service.CastBallotAsync(poll.Election.Id, poll.Voters[1].Id, new CastBallotDto([
            new BallotSeatChoiceDto(poll.Seats[0].Id, []),
            new BallotSeatChoiceDto(poll.Seats[1].Id, []),
        ]));

        var stored = _context.PendingBallots.Select(x => x.SealedChoices).ToList();
        stored.Should().OnlyContain(x => !x.Contains("\"N\"") && !x.Contains("\"S\""));
        // Padding hides how many choices a ballot carries.
        stored.Select(x => x.Length).Distinct().Should().ContainSingle();
        stored.Should().OnlyContain(x => x.StartsWith(Constants.Elections.BallotSealPrefix));
        BallotSeal.Open(OfficerKey, stored[0], poll.Election.Id, poll.Election.BallotSealVersion).Should().Contain($"{poll.Nominations[0].Id}");
    }

    [Test]
    [Category("FR-39")]
    public async Task CastBallotAsync_RefusesWhenTheElectionHasNoKey()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        poll.Election.BallotPublicKey = null;
        await _context.SaveChangesAsync();

        (await new ElectionService(_context).CastBallotAsync(poll.Election.Id, poll.Voters[0].Id, poll.BallotFor(0))).Error.Should().Be("not-open");
        _context.PendingBallots.Should().BeEmpty();
    }

    [Test]
    [Category("FR-39")]
    public async Task CastBallotAsync_KeepsTheDateOnlyOnTheVoteRows()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);

        await new ElectionService(_context).CastBallotAsync(poll.Election.Id, poll.Voters[0].Id, poll.BallotFor(0));

        _context.VoterRolls.Single(x => x.MemberId == poll.Voters[0].Id).VotedAt!.Value.TimeOfDay.Should().Be(TimeSpan.Zero);
        _context.SeatVotes.Should().OnlyContain(x => x.VotedAt.TimeOfDay == TimeSpan.Zero);
    }

    [Test]
    [Category("FR-39")]
    public async Task CastBallotAsync_OneBadSeatWritesNothing()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        var ballot = new CastBallotDto([
            new BallotSeatChoiceDto(poll.Seats[0].Id, [poll.Nominations[0].Id]),
            new BallotSeatChoiceDto(poll.Seats[1].Id, [poll.Nominations[0].Id]),
        ]);

        var result = await new ElectionService(_context).CastBallotAsync(poll.Election.Id, poll.Voters[0].Id, ballot);

        result.Error.Should().Be("invalid-ballot");
        _context.PendingBallots.Should().BeEmpty();
        _context.SeatVotes.Should().BeEmpty();
        _context.VoterRolls.Single(x => x.MemberId == poll.Voters[0].Id).VotedAt.Should().BeNull();
    }

    [Test]
    [Category("FR-39")]
    public async Task CastBallotAsync_RejectsABallotThatLeavesOutASeat()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        var ballot = new CastBallotDto([new BallotSeatChoiceDto(poll.Seats[0].Id, [poll.Nominations[0].Id])]);

        var result = await new ElectionService(_context).CastBallotAsync(poll.Election.Id, poll.Voters[0].Id, ballot);

        result.Error.Should().Be("invalid-ballot");
    }

    [Test]
    [Category("FR-39")]
    public async Task CastBallotAsync_RejectsMoreChoicesThanPlaces()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        var ballot = new CastBallotDto([
            new BallotSeatChoiceDto(poll.Seats[0].Id, [poll.Nominations[0].Id]),
            new BallotSeatChoiceDto(poll.Seats[1].Id, [poll.Nominations[1].Id, poll.Nominations[2].Id, poll.Nominations[1].Id]),
        ]);

        (await new ElectionService(_context).CastBallotAsync(poll.Election.Id, poll.Voters[0].Id, ballot)).Error.Should().Be("invalid-ballot");
    }

    [Test]
    [Category("FR-39")]
    public async Task CastBallotAsync_AcceptsABlankSeat()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        var ballot = new CastBallotDto([
            new BallotSeatChoiceDto(poll.Seats[0].Id, [poll.Nominations[0].Id]),
            new BallotSeatChoiceDto(poll.Seats[1].Id, []),
        ]);

        (await new ElectionService(_context).CastBallotAsync(poll.Election.Id, poll.Voters[0].Id, ballot)).Success.Should().BeTrue();
    }

    [Test]
    [Category("FR-39")]
    public async Task CastBallotAsync_RejectsAVoterNotOnTheRoll()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        var outsider = await CreateAndSaveTestMemberAsync("Outsider", "outsider@example.com", "01799999999", "9999999999");

        (await new ElectionService(_context).CastBallotAsync(poll.Election.Id, outsider.Id, poll.BallotFor(0))).Error.Should().Be("not-eligible");
    }

    [Test]
    [Category("FR-39")]
    public async Task CastBallotAsync_RejectsAVoterOnTheRollWhoIsNotEligible()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        _context.VoterRolls.Single(x => x.MemberId == poll.Voters[0].Id).IsEligible = false;
        await _context.SaveChangesAsync();

        var cast = await new ElectionService(_context).CastBallotAsync(poll.Election.Id, poll.Voters[0].Id, poll.BallotFor(0));

        cast.Error.Should().Be("not-eligible");
        _context.SeatVotes.Any().Should().BeFalse();
    }

    [Test]
    [Category("FR-48")]
    public async Task AuditEntries_AfterCastAndCount_HoldNoVoterOrBallotIdentity()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount);
        var service = new ElectionService(_context);
        var codes = new List<string>();
        for (var i = 0; i < poll.Voters.Length; i++)
            codes.Add((await service.CastBallotAsync(poll.Election.Id, poll.Voters[i].Id, poll.BallotFor(i))).TrackingCode!);
        await CloseAndStartCountingAsync(service, poll.Election);
        (await service.CountAsync(poll.Election.Id, OfficerPrivateKey)).Error.Should().BeNull();

        var entries = _context.ElectionAuditEntries.AsNoTracking().Where(x => x.ElectionId == poll.Election.Id).ToList();
        entries.Should().NotBeEmpty();
        entries.Should().OnlyContain(x => x.ActorUserId == null);
        var ballotIds = _context.Ballots.AsNoTracking().Select(b => b.Id.ToString()).ToList();
        foreach (var entry in entries)
        {
            codes.Should().OnlyContain(c => !entry.DetailJson.Contains(c));
            ballotIds.Should().OnlyContain(b => !entry.DetailJson.Contains(b));
            poll.Voters.Should().OnlyContain(v => !entry.DetailJson.Contains("memberId"));
        }
    }

    [Test]
    [Category("FR-39")]
    public async Task CountAsync_MovesEveryBallotAtOnceAndRewritesTheReceipts()
    {
        var poll = await CreatePollingElectionAsync(voters: 2 * Constants.Elections.MinimumBallotsToCount);
        var service = new ElectionService(_context);
        var issued = new List<string>();
        for (var i = 0; i < poll.Voters.Length; i++)
            issued.Add((await service.CastBallotAsync(poll.Election.Id, poll.Voters[i].Id, poll.BallotFor(i))).TrackingCode!);
        var receiptIdsAtVote = _context.BallotReceipts.Select(x => x.Id).ToList();
        _context.Ballots.Should().BeEmpty();

        await CloseAndStartCountingAsync(service, poll.Election);
        (await service.CountAsync(poll.Election.Id, OfficerPrivateKey)).Error.Should().BeNull();

        _context.ChangeTracker.Clear();
        _context.PendingBallots.Should().BeEmpty();
        _context.Ballots.Should().HaveCount(issued.Count);
        _context.BallotReceipts.Select(x => x.TrackingCode).Should().BeEquivalentTo(issued);
        _context.BallotReceipts.Select(x => x.Id).Should().NotIntersectWith(receiptIdsAtVote);
    }

    [Test]
    [Category("FR-39")]
    public async Task CountAsync_RefusesWhenFewerBallotsThanTheMinimumWereCast()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount - 1);
        var service = new ElectionService(_context);
        await CastAllAsync(service, poll);
        await CloseAndStartCountingAsync(service, poll.Election);

        (await service.CountAsync(poll.Election.Id, OfficerPrivateKey)).Error.Should().Be("below-threshold");
        _context.Ballots.Should().BeEmpty();
        _context.ElectionResults.Should().BeEmpty();
    }

    [Test]
    [Category("FR-39")]
    public async Task CountAsync_RefusesAKeyTheElectionWasNotSealedUnder()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount);
        var service = new ElectionService(_context);
        await CastAllAsync(service, poll);
        await CloseAndStartCountingAsync(service, poll.Election);
        using var other = RSA.Create(Constants.Elections.BallotKeyMinBits);

        (await service.CountAsync(poll.Election.Id, Convert.ToBase64String(other.ExportPkcs8PrivateKey()))).Error.Should().Be("wrong-key");
        (await service.CountAsync(poll.Election.Id, "not base64")).Error.Should().Be("wrong-key");
        (await service.CountAsync(poll.Election.Id, null)).Error.Should().Be("no-key");
        _context.PendingBallots.Should().HaveCount(Constants.Elections.MinimumBallotsToCount);
    }

    [Test]
    [Category("FR-39")]
    public async Task CountAsync_StopsWhenReceiptsAndBallotsDoNotMatch()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount);
        var service = new ElectionService(_context);
        await CastAllAsync(service, poll);
        _context.BallotReceipts.Remove(_context.BallotReceipts.First());
        await _context.SaveChangesAsync();
        await CloseAndStartCountingAsync(service, poll.Election);

        (await service.CountAsync(poll.Election.Id, OfficerPrivateKey)).Error.Should().Be("integrity");
        _context.ElectionResults.Should().BeEmpty();
    }

    // TODO 37.13j. Both elections are sealed under the same key, so only the election id in the
    // seal stops a ballot from one being counted in the other.
    [Test]
    [Category("FR-39")]
    public async Task CountAsync_RefusesABallotSealedForAnotherElection()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount);
        var service = new ElectionService(_context);
        await CastAllAsync(service, poll);
        var moved = _context.PendingBallots.First();
        var choices = BallotSeal.Open(OfficerKey, moved.SealedChoices, poll.Election.Id, poll.Election.BallotSealVersion);
        moved.SealedChoices = BallotSeal.Seal(OfficerPublicKey, choices, poll.Election.Id + 1, Constants.Elections.BallotSealVersion);
        await _context.SaveChangesAsync();
        await CloseAndStartCountingAsync(service, poll.Election);

        (await service.CountAsync(poll.Election.Id, OfficerPrivateKey)).Error.Should().Be("unreadable");
        _context.ChangeTracker.Clear();
        _context.Ballots.Should().BeEmpty();
        _context.ElectionResults.Should().BeEmpty();
        _context.PendingBallots.Should().HaveCount(Constants.Elections.MinimumBallotsToCount);
    }

    [Test]
    [Category("FR-39")]
    public async Task CountAsync_RefusesALegacyBallotInAVersion2Election()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount);
        var service = new ElectionService(_context);
        await CastAllAsync(service, poll);
        var moved = _context.PendingBallots.First();
        var choices = BallotSeal.Open(OfficerKey, moved.SealedChoices, poll.Election.Id, poll.Election.BallotSealVersion);
        moved.SealedChoices = BallotSeal.Seal(OfficerPublicKey, choices, poll.Election.Id, Constants.Elections.BallotSealLegacyVersion);
        await _context.SaveChangesAsync();
        await CloseAndStartCountingAsync(service, poll.Election);

        (await service.CountAsync(poll.Election.Id, OfficerPrivateKey)).Error.Should().Be("unreadable");
        _context.ElectionResults.Should().BeEmpty();
    }

    [Test]
    [Category("FR-39")]
    public async Task CountAsync_StillCountsAnElectionSealedInTheLegacyFormat()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount);
        poll.Election.BallotSealVersion = Constants.Elections.BallotSealLegacyVersion;
        await _context.SaveChangesAsync();
        var service = new ElectionService(_context);
        await CastAllAsync(service, poll);
        _context.PendingBallots.Select(x => x.SealedChoices).ToList()
            .Should().OnlyContain(x => !x.StartsWith(Constants.Elections.BallotSealPrefix));
        await CloseAndStartCountingAsync(service, poll.Election);

        (await service.CountAsync(poll.Election.Id, OfficerPrivateKey)).Error.Should().BeNull();
    }

    [Test]
    [Category("FR-39")]
    public async Task CountAsync_RefusesDuringPolling()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);

        var (results, error) = await new ElectionService(_context).CountAsync(poll.Election.Id, OfficerPrivateKey);

        results.Should().BeNull();
        error.Should().Be("not-counting");
    }

    [Test]
    [Category("FR-39")]
    public async Task CountAsync_ElectsTheTopCandidatesUpToTheSeatCount()
    {
        // Two places on the second seat and two candidates, so both are elected.
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount);
        var service = new ElectionService(_context);
        await CastAllAsync(service, poll);
        await CloseAndStartCountingAsync(service, poll.Election);

        var first = (await service.CountAsync(poll.Election.Id, OfficerPrivateKey)).Results!;
        // Once stored, the results come back without the key.
        var second = (await service.CountAsync(poll.Election.Id, null)).Results;

        first.Should().NotBeNull();
        first.Single(x => x.NominationId == poll.Nominations[0].Id).VoteCount.Should().Be(Constants.Elections.MinimumBallotsToCount);
        first.Where(x => x.ElectionSeatId == poll.Seats[1].Id).Should().OnlyContain(x => x.IsElected && !x.IsTie);
        second.Should().BeEquivalentTo(first);
        _context.ElectionResults.Should().HaveCount(first.Count);
    }

    [Test]
    [Category("FR-39")]
    public async Task CountAsync_FlagsATieAtTheLastPlace()
    {
        // One place on the first seat and two candidates level on votes.
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount, secondPresidentCandidate: true);
        var service = new ElectionService(_context);
        for (var i = 0; i < poll.Voters.Length; i++)
            await service.CastBallotAsync(poll.Election.Id, poll.Voters[i].Id, new CastBallotDto([
                new BallotSeatChoiceDto(poll.Seats[0].Id, [poll.Nominations[i % 2 == 0 ? 0 : 3].Id]),
                new BallotSeatChoiceDto(poll.Seats[1].Id, []),
            ]));
        await CloseAndStartCountingAsync(service, poll.Election);

        var results = (await service.CountAsync(poll.Election.Id, OfficerPrivateKey)).Results;

        results!.Where(x => x.ElectionSeatId == poll.Seats[0].Id).Should().HaveCount(2).And.OnlyContain(x => x.IsTie && !x.IsElected);
    }

    [Test]
    [Category("FR-39")]
    public async Task SetPhaseAsync_CannotDeclare()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        await ClosePollingAsync(poll.Election);
        var service = new ElectionService(_context);
        await service.SetPhaseAsync(poll.Election.Id, ElectionPhase.Counting);

        (await service.SetPhaseAsync(poll.Election.Id, ElectionPhase.Declared)).Should().BeFalse();
    }

    [Test]
    [Category("FR-48")]
    public async Task SetPhase_WritesAnAuditEntryThatNamesNoVoter()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        var service = new ElectionService(_context);

        await CloseAndStartCountingAsync(service, poll.Election);

        var entry = _context.ElectionAuditEntries.Single(x => x.Action == Constants.Elections.AuditActions.PhaseChanged);
        entry.DetailJson.Should().Contain("Counting");
        entry.ActorUserId.Should().BeNull();
    }

    [Test]
    [Category("FR-48")]
    public async Task Count_AndDeclare_WriteAuditEntriesInAWholeChain()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount);
        var service = new ElectionService(_context);
        await CastAllAsync(service, poll);
        await CloseAndStartCountingAsync(service, poll.Election);
        await service.CountAsync(poll.Election.Id, OfficerPrivateKey);
        await service.DeclareAsync(poll.Election.Id);

        _context.ElectionAuditEntries.Select(x => x.Action).Should().Contain([Constants.Elections.AuditActions.Counted, Constants.Elections.AuditActions.Declared]);
        (await new GHCAA.Infrastructure.Services.ElectionAuditWriter(_context).VerifyAsync(poll.Election.Id)).Ok.Should().BeTrue();
    }

    [Test]
    public async Task Declare_InsideTheCallersTransaction_JoinsItAndLeavesTheCommitToTheCaller()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount);
        var service = new ElectionService(_context);
        await CastAllAsync(service, poll);
        await CloseAndStartCountingAsync(service, poll.Election);
        await service.CountAsync(poll.Election.Id, OfficerPrivateKey);

        await using var outer = await _context.Database.BeginTransactionAsync();
        var declared = await service.DeclareAsync(poll.Election.Id);

        declared.Should().BeTrue();
        _context.Database.CurrentTransaction.Should().BeSameAs(outer);
        await outer.RollbackAsync();
        (await _context.Elections.AsNoTracking().SingleAsync(x => x.Id == poll.Election.Id)).Phase.Should().Be(ElectionPhase.Counting);
    }

    [Test]
    [Category("FR-50")]
    public async Task KeyDestroyed_WritesAuditEntry()
    {
        var (service, id, _) = await DeclaredPollAsync();
        await service.DeclareAsync(id);

        var error = await service.RecordKeyDestroyedAsync(id, 7);

        error.Should().BeNull();
        var entry = _context.ElectionAuditEntries.Single(x => x.ElectionId == id && x.Action == Constants.Elections.AuditActions.KeyDestroyed);
        entry.ActorUserId.Should().Be(7);
        (await new GHCAA.Infrastructure.Services.ElectionAuditWriter(_context).VerifyAsync(id)).Ok.Should().BeTrue();
        (await service.RecordKeyDestroyedAsync(id, 7)).Should().Be(Constants.Elections.Replies.AlreadyRecorded);
    }

    [Test]
    [Category("FR-50")]
    public async Task KeyDestroyed_IndexRefusesASecondRowForTheSameElection()
    {
        var (service, id, _) = await DeclaredPollAsync();
        await service.DeclareAsync(id);
        (await service.RecordKeyDestroyedAsync(id, 7)).Should().BeNull();

        _context.ElectionAuditEntries.Add(new ElectionAuditEntry
        {
            ElectionId = id,
            Seq = 9999,
            Action = Constants.Elections.AuditActions.KeyDestroyed,
            DetailJson = "{}",
            PrevHash = new string('0', 64),
            Hash = new string('1', 64),
            At = DateTime.UtcNow
        });
        var save = async () => await _context.SaveChangesAsync();

        await save.Should().ThrowAsync<DbUpdateException>();
        _context.ChangeTracker.Clear();
    }

    [Test]
    [Category("FR-50")]
    public async Task KeyDestroyed_TwoAtOnce_WritesOneRecord()
    {
        var (service, id, _) = await DeclaredPollAsync();
        await service.DeclareAsync(id);

        var replies = await Task.WhenAll(service.RecordKeyDestroyedAsync(id, 7), service.RecordKeyDestroyedAsync(id, 8));

        replies.Count(r => r == null).Should().Be(1);
        replies.Count(r => r == "already-recorded").Should().Be(1);
        _context.ElectionAuditEntries.Count(x => x.ElectionId == id && x.Action == Constants.Elections.AuditActions.KeyDestroyed).Should().Be(1);
    }

    [Test]
    [Category("NFR-R5")]
    public async Task KeyDestroyed_BeforeDeclared_Refused()
    {
        var (service, id, _) = await DeclaredPollAsync();

        var error = await service.RecordKeyDestroyedAsync(id, 7);

        error.Should().Be(Constants.Elections.Replies.NotDeclared);
        _context.ElectionAuditEntries.Any(x => x.Action == Constants.Elections.AuditActions.KeyDestroyed).Should().BeFalse();
    }

    [Test]
    [Category("NFR-R5")]
    public async Task KeyDestroyed_MissingElection_NotFound()
    {
        var (service, id, _) = await DeclaredPollAsync();

        var error = await service.RecordKeyDestroyedAsync(id + 999, 7);

        error.Should().Be(Constants.Elections.Replies.NotFound);
    }

    private async Task<(ElectionService Service, int ElectionId, int Cast)> DeclaredPollAsync(bool publish = true)
    {
        var poll = await CreatePollingElectionAsync(voters: 12);
        var config = new Moq.Mock<GHCAA.Application.Interfaces.IOrgConfigService>();
        config.Setup(x => x.GetConfigAsync()).Returns(Task.FromResult(new OrgConfigDto { Elections = new ElectionSettingsDto { PublishPerSeatBallots = publish } }));
        var service = new ElectionService(_context, null, config.Object);
        await CastAllAsync(service, poll);
        await CloseAndStartCountingAsync(service, poll.Election);
        await service.CountAsync(poll.Election.Id, OfficerPrivateKey);
        return (service, poll.Election.Id, poll.Voters.Length);
    }

    [Test]
    [Category("FR-49")]
    public async Task OpenedBallots_BeforeDeclared_ReturnsNull()
    {
        var (service, id, _) = await DeclaredPollAsync();

        (await service.GetOpenedBallotsAsync(id)).Should().BeNull();
    }

    [Test]
    [Category("FR-49")]
    public async Task OpenedBallots_SettingOff_ReturnsNull()
    {
        var (service, id, _) = await DeclaredPollAsync(publish: false);
        await service.DeclareAsync(id);

        (await service.GetOpenedBallotsAsync(id)).Should().BeNull();
    }

    [Test]
    [Category("FR-49")]
    public async Task OpenedBallots_AfterDeclared_HoldOneRowPerBallotAndSeat()
    {
        var (service, id, cast) = await DeclaredPollAsync();
        await service.DeclareAsync(id);

        var opened = (await service.GetOpenedBallotsAsync(id))!;

        opened.Seats.Should().NotBeEmpty();
        opened.Seats.Should().OnlyContain(s => s.Ballots.Count <= cast);
        opened.Seats.Sum(s => s.Ballots.Sum(b => b.Count)).Should().Be(_context.BallotVotes.Count());
        (await service.GetOpenedBallotsAsync(id))!.Seats.Select(s => s.Ballots.Select(b => string.Join(",", b))).Should()
            .BeEquivalentTo(opened.Seats.Select(s => s.Ballots.Select(b => string.Join(",", b))), o => o.WithStrictOrdering());
    }

    [Test]
    [Category("FR-39")]
    public async Task DeclareAsync_WritesTheWinnersAsECMembers()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount);
        var service = new ElectionService(_context);
        await CastAllAsync(service, poll);
        await CloseAndStartCountingAsync(service, poll.Election);

        (await service.DeclareAsync(poll.Election.Id)).Should().BeFalse("nothing has been counted yet");
        await service.CountAsync(poll.Election.Id, OfficerPrivateKey);
        (await service.DeclareAsync(poll.Election.Id)).Should().BeTrue();

        _context.ECMembers.Should().HaveCount(3);
        _context.ECMembers.Should().ContainSingle(x => x.MemberId == poll.Nominations[0].CandidateMemberId && x.Position == ECPosition.President);
        _context.Elections.AsNoTracking().Single().Phase.Should().Be(ElectionPhase.Declared);
    }

    [Test]
    [Category("FR-39")]
    public async Task Declare_SetsAppointmentExpiryFromSetting()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount);
        var service = new ElectionService(_context);
        await CastAllAsync(service, poll);
        await CloseAndStartCountingAsync(service, poll.Election);
        await service.CountAsync(poll.Election.Id, OfficerPrivateKey);
        var appointmentId = await AddLiveAppointmentAsync(poll.Election.Id, null);

        (await service.DeclareAsync(poll.Election.Id)).Should().BeTrue();

        var row = _context.ElectionAppointments.AsNoTracking().Single(x => x.Id == appointmentId);
        var declaredOn = _context.Elections.AsNoTracking().Single(x => x.Id == poll.Election.Id).DeclaredOn!.Value;
        row.ExpiresAt.Should().Be(declaredOn.AddDays(new ElectionSettingsDto().AccessEndsDaysAfterDeclare));
    }

    [Test]
    [Category("FR-39")]
    public async Task Declare_KeepsExistingExpiry()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount);
        var service = new ElectionService(_context);
        await CastAllAsync(service, poll);
        await CloseAndStartCountingAsync(service, poll.Election);
        await service.CountAsync(poll.Election.Id, OfficerPrivateKey);
        var fixedExpiry = DateTime.UtcNow.AddDays(3);
        var appointmentId = await AddLiveAppointmentAsync(poll.Election.Id, fixedExpiry);

        (await service.DeclareAsync(poll.Election.Id)).Should().BeTrue();

        _context.ElectionAppointments.AsNoTracking().Single(x => x.Id == appointmentId).ExpiresAt.Should().Be(fixedExpiry);
    }

    private async Task<int> AddLiveAppointmentAsync(int electionId, DateTime? expiresAt)
    {
        var persona = new ElectionPersona { Name = "Scrutineer " + Guid.NewGuid(), GroupName = "Officials", Description = "d", DeclarationText = "d", IsActive = true };
        var user = new User { Username = "official-" + Guid.NewGuid(), PasswordHash = "x" };
        _context.ElectionPersonas.Add(persona);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        var row = new ElectionAppointment { ElectionId = electionId, PersonaId = persona.Id, UserId = user.Id, DisplayName = "o", Email = "o@example.com", AppointedByUserId = user.Id, AppointedAt = DateTime.UtcNow, AcceptedAt = DateTime.UtcNow, DeclarationSignedAt = DateTime.UtcNow, ExpiresAt = expiresAt };
        _context.ElectionAppointments.Add(row);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        return row.Id;
    }

    [Test]
    [Category("FR-39")]
    public async Task SetPhaseAsync_WillNotOpenPollingWithoutAKey()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        poll.Election.Phase = ElectionPhase.Campaign;
        poll.Election.BallotPublicKey = null;
        await _context.SaveChangesAsync();
        var service = new ElectionService(_context);

        (await service.SetPhaseAsync(poll.Election.Id, ElectionPhase.Polling)).Should().BeFalse();
        (await service.SetBallotKeyAsync(poll.Election.Id, OfficerPublicKey)).Success.Should().BeTrue();
        _context.ElectionAuditEntries.Select(x => x.Action).Should().Contain(Constants.Elections.AuditActions.BallotKeySet);
        (await service.SetPhaseAsync(poll.Election.Id, ElectionPhase.Polling)).Should().BeTrue();
    }

    [Test]
    [Category("FR-39")]
    public async Task SetBallotKeyAsync_RefusesAWeakKeyAndAnyChangeOncePollingOpens()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        var service = new ElectionService(_context);
        using var weak = RSA.Create(2048);

        (await service.SetBallotKeyAsync(poll.Election.Id, OfficerPublicKey)).Error.Should().Be("phase-closed");
        poll.Election.Phase = ElectionPhase.Scrutiny;
        await _context.SaveChangesAsync();
        (await service.SetBallotKeyAsync(poll.Election.Id, Convert.ToBase64String(weak.ExportSubjectPublicKeyInfo()))).Error.Should().Be("invalid-key");
        (await service.SetBallotKeyAsync(poll.Election.Id, "junk")).Error.Should().Be("invalid-key");
        var ok = await service.SetBallotKeyAsync(poll.Election.Id, OfficerPublicKey);
        ok.Fingerprint.Should().Be(BallotSeal.PrivateKeyFingerprint(OfficerKey));
    }

    [Test]
    public async Task SetBallotKeyAsync_SavesTheKeyAndItsAuditRowTogether()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        poll.Election.Phase = ElectionPhase.Scrutiny;
        await _context.SaveChangesAsync();

        using var other = RSA.Create(3072);
        var result = await new ElectionService(_context).SetBallotKeyAsync(poll.Election.Id, Convert.ToBase64String(other.ExportSubjectPublicKeyInfo()));

        result.Success.Should().BeTrue();
        _context.ChangeTracker.Clear();
        (await _context.Elections.AsNoTracking().SingleAsync(x => x.Id == poll.Election.Id)).BallotKeyFingerprint.Should().Be(result.Fingerprint);
        _context.ElectionAuditEntries.Count(x => x.ElectionId == poll.Election.Id && x.Action == Constants.Elections.AuditActions.BallotKeySet).Should().Be(1);
        _context.Database.CurrentTransaction.Should().BeNull();
    }

    [Test]
    public async Task SetBallotKeyAsync_JoinsAnOuterTransactionAndLeavesNothingWhenItRollsBack()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        poll.Election.Phase = ElectionPhase.Scrutiny;
        await _context.SaveChangesAsync();

        var before = poll.Election.BallotKeyFingerprint;
        using var other = RSA.Create(3072);
        await using (var outer = await _context.Database.BeginTransactionAsync())
        {
            (await new ElectionService(_context).SetBallotKeyAsync(poll.Election.Id, Convert.ToBase64String(other.ExportSubjectPublicKeyInfo()))).Success.Should().BeTrue();
            await outer.RollbackAsync();
        }

        _context.ChangeTracker.Clear();
        (await _context.Elections.AsNoTracking().SingleAsync(x => x.Id == poll.Election.Id)).BallotKeyFingerprint.Should().Be(before);
        _context.ElectionAuditEntries.Any(x => x.ElectionId == poll.Election.Id && x.Action == Constants.Elections.AuditActions.BallotKeySet).Should().BeFalse();
    }

    [Test]
    [Category("FR-39")]
    public async Task AddCandidateAsync_FailsAfterScrutiny()
    {
        var poll = await CreatePollingElectionAsync(voters: 3);

        var result = await new ElectionService(_context).AddCandidateAsync(poll.Election.Id,
            new SaveCandidateRequest(poll.Voters[0].Id, poll.Seats[0].Id, null, poll.Voters[1].Id, poll.Voters[2].Id));

        result.Error.Should().Be("phase-closed");
    }

    [Test]
    [Category("FR-39")]
    public async Task AddCandidateAsync_NeedsTwoOtherEligibleVotersAsProposerAndSeconder()
    {
        var poll = await CreatePollingElectionAsync(voters: 3);
        poll.Election.Phase = ElectionPhase.Scrutiny;
        await _context.SaveChangesAsync();
        var service = new ElectionService(_context);

        var self = await service.AddCandidateAsync(poll.Election.Id,
            new SaveCandidateRequest(poll.Voters[0].Id, poll.Seats[0].Id, null, poll.Voters[0].Id, poll.Voters[1].Id));
        var ok = await service.AddCandidateAsync(poll.Election.Id,
            new SaveCandidateRequest(poll.Voters[0].Id, poll.Seats[0].Id, null, poll.Voters[1].Id, poll.Voters[2].Id));

        self.Error.Should().Be("invalid-proposer");
        ok.Success.Should().BeTrue(ok.Error);
        _context.Nominations.Single(x => x.CandidateMemberId == poll.Voters[0].Id).Status.Should().Be(NominationStatus.Submitted);
    }

    [Test]
    [Category("FR-39")]
    public async Task RemoveCandidateAsync_FailsOnceTheCandidateListIsOut()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);

        var result = await new ElectionService(_context).RemoveCandidateAsync(poll.Election.Id, poll.Nominations[1].Id);

        result.Error.Should().Be("phase-closed");
        _context.Nominations.Should().HaveCount(3);
    }

    [Test]
    [Category("FR-39")]
    public async Task CreateAsync_NeedsAnEmptyPeriodThatStartsOnceVotingIsOver()
    {
        var closes = DateTime.UtcNow.AddDays(10);
        var early = new ECPeriod { Title = "Early", StartDate = closes.AddDays(-1) };
        var taken = new ECPeriod { Title = "Taken", StartDate = closes.AddDays(5) };
        var fresh = new ECPeriod { Title = "Fresh", StartDate = closes.Date };
        _context.ECPeriods.AddRange(early, taken, fresh);
        var sitting = await CreateAndSaveTestMemberAsync();
        _context.ECMembers.Add(new ECMember { ECPeriod = taken, MemberId = sitting.Id, Position = ECPosition.President, StartDate = taken.StartDate });
        await _context.SaveChangesAsync();
        var service = new ElectionService(_context);
        CreateElectionDto For(int periodId) => new("Election", periodId, closes.AddDays(-9), closes.AddDays(-5), closes.AddDays(-1), closes, sitting.Id);

        (await service.CreateAsync(For(fresh.Id + 100))).Error.Should().Be("no-period");
        (await service.CreateAsync(For(early.Id))).Error.Should().Be("period-starts-early");
        (await service.CreateAsync(For(taken.Id))).Error.Should().Be("period-has-members");
        _context.Elections.Should().BeEmpty();

        var (created, error) = await service.CreateAsync(For(fresh.Id));

        error.Should().BeNull();
        created!.ECPeriodId.Should().Be(fresh.Id);
        _context.Elections.Should().ContainSingle();
        (await service.CreateAsync(For(fresh.Id))).Error.Should().Be("period-in-use");
    }

    [Test]
    [Category("FR-39")]
    public async Task Database_HoldsOneLiveElectionPerPeriod_ButAnArchivedOneDoesNotCount()
    {
        var period = new ECPeriod { Title = "Next", StartDate = DateTime.UtcNow.AddDays(30) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        Election Make(ElectionPhase phase) => new()
        {
            Title = "Election",
            ECPeriodId = period.Id,
            Phase = phase,
            NominationOpensOn = DateTime.UtcNow,
            NominationClosesOn = DateTime.UtcNow.AddDays(1),
            PollingOpensOn = DateTime.UtcNow.AddDays(2),
            PollingClosesOn = DateTime.UtcNow.AddDays(3)
        };
        _context.Elections.AddRange(Make(ElectionPhase.Archived), Make(ElectionPhase.Polling));
        await _context.SaveChangesAsync();

        _context.Elections.Add(Make(ElectionPhase.Announced));

        await FluentActions.Awaiting(() => _context.SaveChangesAsync())
            .Should().ThrowAsync<Microsoft.EntityFrameworkCore.DbUpdateException>("two admins can race past the service check");
    }

    private async Task<ECPeriod> AddPeriodAsync(string title, int startsInDays)
    {
        var period = new ECPeriod { Title = title, StartDate = DateTime.UtcNow.AddDays(startsInDays) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        return period;
    }

    private static Election MakeElection(int periodId, ElectionPhase phase, bool isTest = false, DateTime? declaredOn = null) => new()
    {
        Title = "Election",
        ECPeriodId = periodId,
        Phase = phase,
        IsTest = isTest,
        DeclaredOn = declaredOn,
        NominationOpensOn = DateTime.UtcNow,
        NominationClosesOn = DateTime.UtcNow.AddDays(1),
        PollingOpensOn = DateTime.UtcNow.AddDays(2),
        PollingClosesOn = DateTime.UtcNow.AddDays(3)
    };

    [Test]
    [Category("FR-47")]
    public async Task Database_ARehearsalDoesNotBlockTheRealElection_InEitherOrder()
    {
        var first = await AddPeriodAsync("First", 30);
        var second = await AddPeriodAsync("Second", 60);
        _context.Elections.AddRange(
            MakeElection(first.Id, ElectionPhase.Polling, isTest: true), MakeElection(first.Id, ElectionPhase.Polling),
            MakeElection(second.Id, ElectionPhase.Polling), MakeElection(second.Id, ElectionPhase.Polling, isTest: true));
        await _context.SaveChangesAsync();

        _context.Elections.Add(MakeElection(first.Id, ElectionPhase.Announced));

        await FluentActions.Awaiting(() => _context.SaveChangesAsync())
            .Should().ThrowAsync<DbUpdateException>("two real elections still cannot share a period");
    }

    [Test]
    [Category("FR-47")]
    public async Task CreateAsync_ARehearsalAndTheRealElectionDoNotBlockEachOther()
    {
        var closes = DateTime.UtcNow.AddDays(10);
        var period = new ECPeriod { Title = "Next", StartDate = closes.Date };
        _context.ECPeriods.Add(period);
        var member = await CreateAndSaveTestMemberAsync();
        await _context.SaveChangesAsync();
        var service = new ElectionService(_context);
        CreateElectionDto For(bool isTest) => new("Election", period.Id, closes.AddDays(-9), closes.AddDays(-5), closes.AddDays(-1), closes, member.Id, IsTest: isTest);

        (await service.CreateAsync(For(true))).Error.Should().BeNull();
        (await service.CreateAsync(For(true))).Error.Should().BeNull("a second rehearsal is fine");
        (await service.CreateAsync(For(false))).Error.Should().BeNull("the rehearsals are not live elections");
        (await service.CreateAsync(For(false))).Error.Should().Be("period-in-use");
        _context.Elections.Count(x => !x.IsTest).Should().Be(1);
    }

    [Test]
    [Category("FR-47")]
    public async Task CreateAsync_ARehearsalSkipsThePeriodStartAndMembersChecks()
    {
        var closes = DateTime.UtcNow.AddDays(10);
        var early = new ECPeriod { Title = "Early", StartDate = closes.AddDays(-1) };
        var taken = new ECPeriod { Title = "Taken", StartDate = closes.AddDays(5) };
        _context.ECPeriods.AddRange(early, taken);
        var sitting = await CreateAndSaveTestMemberAsync();
        _context.ECMembers.Add(new ECMember { ECPeriod = taken, MemberId = sitting.Id, Position = ECPosition.President, StartDate = taken.StartDate });
        await _context.SaveChangesAsync();
        var service = new ElectionService(_context);
        CreateElectionDto For(int periodId, bool isTest) => new("Election", periodId, closes.AddDays(-9), closes.AddDays(-5), closes.AddDays(-1), closes, sitting.Id, IsTest: isTest);

        (await service.CreateAsync(For(early.Id, true))).Error.Should().BeNull();
        (await service.CreateAsync(For(taken.Id, true))).Error.Should().BeNull();
        (await service.CreateAsync(For(early.Id, false))).Error.Should().Be("period-starts-early");
        (await service.CreateAsync(For(taken.Id, false))).Error.Should().Be("period-has-members");
    }

    [Test]
    [Category("FR-39")]
    public async Task DeclareAsync_SecondCallIsRefused_AndNothingIsWrittenTwice()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount);
        var service = new ElectionService(_context);
        await CastAllAsync(service, poll);
        await CloseAndStartCountingAsync(service, poll.Election);
        await service.CountAsync(poll.Election.Id, OfficerPrivateKey);
        var options = new DbContextOptionsBuilder<GHCAA.Infrastructure.Data.ApplicationDbContext>()
            .UseSqlite(_context.Database.GetDbConnection()).Options;
        await using var other = new GHCAA.Infrastructure.Data.ApplicationDbContext(options);

        (await service.DeclareAsync(poll.Election.Id)).Should().BeTrue();
        (await new ElectionService(other).DeclareAsync(poll.Election.Id)).Should().BeFalse("the election is no longer in Counting");

        _context.ECMembers.AsNoTracking().Should().HaveCount(3);
        _context.ElectionAuditEntries.AsNoTracking().Count(x => x.Action == Constants.Elections.AuditActions.Declared).Should().Be(1);
    }

    [Test]
    [Category("FR-39")]
    public async Task GetLatestDeclaredAsync_IsNullWhenNothingIsDeclared()
    {
        var period = await AddPeriodAsync("P", 30);
        _context.Elections.Add(MakeElection(period.Id, ElectionPhase.Counting));
        await _context.SaveChangesAsync();

        (await new ElectionService(_context).GetLatestDeclaredAsync()).Should().BeNull();
    }

    [Test]
    [Category("FR-39")]
    public async Task GetLatestDeclaredAsync_PicksTheNewestDeclaredOrArchivedAndSkipsRehearsals()
    {
        var now = DateTime.UtcNow;
        var p1 = await AddPeriodAsync("P1", 30);
        var p2 = await AddPeriodAsync("P2", 60);
        var p3 = await AddPeriodAsync("P3", 90);
        var p4 = await AddPeriodAsync("P4", 120);
        var older = MakeElection(p1.Id, ElectionPhase.Archived, declaredOn: now.AddDays(-30));
        var newest = MakeElection(p2.Id, ElectionPhase.Declared, declaredOn: now.AddDays(-5));
        var rehearsal = MakeElection(p3.Id, ElectionPhase.Declared, isTest: true, declaredOn: now.AddDays(-1));
        var counting = MakeElection(p4.Id, ElectionPhase.Counting);
        _context.Elections.AddRange(older, newest, rehearsal, counting);
        await _context.SaveChangesAsync();

        var result = await new ElectionService(_context).GetLatestDeclaredAsync();

        result.Should().NotBeNull();
        result!.Id.Should().Be(newest.Id);
        result.Phase.Should().Be(ElectionPhase.Declared);
    }

    [Test]
    [Category("FR-39")]
    public async Task GetBallotAsync_ShowsNamesAndPlacesAndWhetherTheMemberHasVoted()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        var service = new ElectionService(_context);

        var before = await service.GetBallotAsync(poll.Election.Id, poll.Voters[0].Id);

        before!.HasVoted.Should().BeFalse();
        before.BallotKeyFingerprint.Should().Be(poll.Election.BallotKeyFingerprint);
        before.Seats.Select(s => s.PositionTitle).Should().Equal("President", "Member 1");
        before.Seats.Select(s => s.Places).Should().Equal(1, 2);
        before.Seats[1].Candidates.Select(c => c.Name).Should().Equal("Member 1", "Member 2");
        before.Seats[1].Candidates.Select(c => c.NominationId).Should().Equal(poll.Nominations[1].Id, poll.Nominations[2].Id);

        await service.CastBallotAsync(poll.Election.Id, poll.Voters[0].Id, poll.BallotFor(0));

        (await service.GetBallotAsync(poll.Election.Id, poll.Voters[0].Id))!.HasVoted.Should().BeTrue();
        (await service.GetBallotAsync(poll.Election.Id + 100, poll.Voters[0].Id)).Should().BeNull();
    }

    [Test]
    [Category("FR-39")]
    public async Task GetBallotAsync_SaysWhetherTheMemberCanVoteAndWhyNot()
    {
        var poll = await CreatePollingElectionAsync(voters: 2);
        var unpaid = _context.VoterRolls.Single(x => x.MemberId == poll.Voters[1].Id);
        unpaid.IsEligible = false;
        unpaid.IneligibilityReason = "Membership dues are overdue.";
        var outsider = await CreateAndSaveTestMemberAsync("Outsider", "outsider@example.com", "01799999999", "9999999999");
        await _context.SaveChangesAsync();
        var service = new ElectionService(_context);

        var eligible = await service.GetBallotAsync(poll.Election.Id, poll.Voters[0].Id);
        var onRollButNot = await service.GetBallotAsync(poll.Election.Id, poll.Voters[1].Id);
        var notOnRoll = await service.GetBallotAsync(poll.Election.Id, outsider.Id);

        eligible!.IsEligible.Should().BeTrue();
        eligible.IneligibilityReason.Should().BeNull();
        onRollButNot!.IsEligible.Should().BeFalse();
        onRollButNot.IneligibilityReason.Should().Be("Membership dues are overdue.");
        notOnRoll!.IsEligible.Should().BeFalse();
        notOnRoll.IneligibilityReason.Should().Be("You are not on the voter roll for this election.");
    }

    [Test]
    [Category("FR-39")]
    public async Task GetBallotAsync_LeavesEligibilityOpenUntilTheRollIsFrozen()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        _context.VoterRolls.RemoveRange(_context.VoterRolls);
        await _context.SaveChangesAsync();

        var ballot = await new ElectionService(_context).GetBallotAsync(poll.Election.Id, poll.Voters[0].Id);

        ballot!.IsEligible.Should().BeNull();
        ballot.IneligibilityReason.Should().BeNull();
    }

    [Test]
    [Category("FR-39")]
    public async Task GetBallotAsync_GivesAReasonWhenTheRollRowHasNone()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        _context.VoterRolls.Single(x => x.MemberId == poll.Voters[0].Id).IsEligible = false;
        await _context.SaveChangesAsync();

        var ballot = await new ElectionService(_context).GetBallotAsync(poll.Election.Id, poll.Voters[0].Id);

        ballot!.IsEligible.Should().BeFalse();
        ballot.IneligibilityReason.Should().Be("You are not eligible to vote in this election.");
    }

    [Test]
    [Category("FR-39")]
    public async Task GetTrackingCodesAsync_StaysHiddenUntilTheCountHasRun()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount);
        var service = new ElectionService(_context);
        var issued = new List<string>();
        for (var i = 0; i < poll.Voters.Length; i++)
            issued.Add((await service.CastBallotAsync(poll.Election.Id, poll.Voters[i].Id, poll.BallotFor(i))).TrackingCode!);

        (await service.GetTrackingCodesAsync(poll.Election.Id)).Should().BeNull("receipts are still in voting order");
        await CloseAndStartCountingAsync(service, poll.Election);
        (await service.GetTrackingCodesAsync(poll.Election.Id)).Should().BeNull("the count has not run");
        await service.CountAsync(poll.Election.Id, OfficerPrivateKey);

        var codes = await service.GetTrackingCodesAsync(poll.Election.Id);

        codes.Should().BeEquivalentTo(issued);
        codes.Should().BeInAscendingOrder();
    }

    [Test]
    [Category("FR-39")]
    public async Task GetPublicResultsAsync_ShowsTurnoutAbstentionsAndCandidatesWithNoVotes()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount, secondPresidentCandidate: true);
        var service = new ElectionService(_context);
        var blankSeat = new CastBallotDto([
            new BallotSeatChoiceDto(poll.Seats[0].Id, [poll.Nominations[0].Id]),
            new BallotSeatChoiceDto(poll.Seats[1].Id, []),
        ]);
        (await service.CastBallotAsync(poll.Election.Id, poll.Voters[0].Id, blankSeat)).Success.Should().BeTrue();
        for (var i = 1; i < poll.Voters.Length; i++)
            (await service.CastBallotAsync(poll.Election.Id, poll.Voters[i].Id, poll.BallotFor(i))).Success.Should().BeTrue();
        await CloseAndStartCountingAsync(service, poll.Election);
        await service.CountAsync(poll.Election.Id, OfficerPrivateKey);

        (await service.GetPublicResultsAsync(poll.Election.Id)).Should().BeNull("the result is not declared yet");
        (await service.DeclareAsync(poll.Election.Id)).Should().BeTrue();

        var result = (await service.GetPublicResultsAsync(poll.Election.Id))!;

        result.EligibleVoters.Should().Be(poll.Voters.Length + 4);
        result.BallotsCast.Should().Be(poll.Voters.Length);
        result.SpoiledBallots.Should().Be(0);
        result.Seats.Select(s => s.Abstentions).Should().Equal(0, 1);
        var president = result.Seats[0].Candidates;
        president.Select(c => c.Name).Should().Equal("Member 0", "Member 3");
        president.Select(c => c.VoteCount).Should().Equal(poll.Voters.Length, 0);
        president.Select(c => c.IsElected).Should().Equal(true, false);
    }

    [Test]
    [Category("FR-39")]
    public async Task DeclareAsync_StartsTheWinnersWhenTheirPeriodStarts()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount);
        var periodStart = DateTime.UtcNow.Date.AddDays(30);
        poll.Election.ECPeriod!.StartDate = periodStart;
        await _context.SaveChangesAsync();
        var service = new ElectionService(_context);
        await CastAllAsync(service, poll);
        await CloseAndStartCountingAsync(service, poll.Election);
        await service.CountAsync(poll.Election.Id, OfficerPrivateKey);

        (await service.DeclareAsync(poll.Election.Id)).Should().BeTrue();

        _context.ECMembers.Should().HaveCount(3).And.OnlyContain(x => x.StartDate == periodStart);
    }

    [Test]
    [Category("FR-39")]
    public async Task DeclareAsync_RefusesWhenThePeriodWasFilledMeanwhile()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount);
        var service = new ElectionService(_context);
        await CastAllAsync(service, poll);
        await CloseAndStartCountingAsync(service, poll.Election);
        await service.CountAsync(poll.Election.Id, OfficerPrivateKey);
        var outsider = await CreateAndSaveTestMemberAsync();
        var added = new ECMember { ECPeriodId = poll.Election.ECPeriodId, MemberId = outsider.Id, Position = ECPosition.President, StartDate = DateTime.UtcNow };
        _context.ECMembers.Add(added);
        await _context.SaveChangesAsync();

        (await service.DeclareAsync(poll.Election.Id)).Should().BeFalse("someone was put in the period by hand");
        _context.ECMembers.Should().ContainSingle();
    }

    [Test]
    [Category("FR-39")]
    public async Task SetPhaseAsync_ToCandidateList_NumbersEachSeatFromOne()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        poll.Election.Phase = ElectionPhase.Withdrawal;
        await _context.SaveChangesAsync();

        (await new ElectionService(_context).SetPhaseAsync(poll.Election.Id, ElectionPhase.CandidateList)).Should().BeTrue();

        poll.Nominations[0].BallotOrder.Should().Be(1);
        new[] { poll.Nominations[1].BallotOrder, poll.Nominations[2].BallotOrder }.Should().BeEquivalentTo(new int?[] { 1, 2 });
        _context.ElectionAuditEntries.Should().ContainSingle(x => x.Action == Constants.Elections.AuditActions.CandidateOrderSet);
    }

    [Test]
    [Category("FR-39")]
    public async Task SetPhaseAsync_AlphabeticalSetting_OrdersByName()
    {
        var (election, nominations) = await CreateWithdrawalElectionAsync("Zara", "adil", "Mina");
        var config = new Moq.Mock<GHCAA.Application.Interfaces.IOrgConfigService>();
        config.Setup(x => x.GetConfigAsync()).Returns(Task.FromResult(new OrgConfigDto
        {
            Elections = new ElectionSettingsDto { CandidateOrder = ElectionCandidateOrder.Alphabetical },
        }));

        (await new ElectionService(_context, null, config.Object).SetPhaseAsync(election.Id, ElectionPhase.CandidateList)).Should().BeTrue();

        nominations.Select(n => n.BallotOrder).Should().Equal(3, 1, 2);
    }

    [Test]
    [Category("FR-39")]
    public async Task SetPhaseAsync_PastCandidateList_KeepsTheOrder()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        poll.Election.Phase = ElectionPhase.Withdrawal;
        await _context.SaveChangesAsync();
        var service = new ElectionService(_context);
        (await service.SetPhaseAsync(poll.Election.Id, ElectionPhase.CandidateList)).Should().BeTrue();
        var order = poll.Nominations.Select(n => n.BallotOrder).ToArray();

        (await service.SetPhaseAsync(poll.Election.Id, ElectionPhase.Campaign)).Should().BeTrue();

        poll.Nominations.Select(n => n.BallotOrder).Should().Equal(order);
        _context.ElectionAuditEntries.Count(x => x.Action == Constants.Elections.AuditActions.CandidateOrderSet).Should().Be(1);
    }

    [Test]
    [Category("FR-39")]
    public async Task GetBallotAsync_SortsByBallotOrder()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        poll.Nominations[1].BallotOrder = 2;
        poll.Nominations[2].BallotOrder = 1;
        await _context.SaveChangesAsync();

        var ballot = await new ElectionService(_context).GetBallotAsync(poll.Election.Id, poll.Voters[0].Id);

        ballot!.Seats[1].Candidates.Select(c => c.NominationId).Should().Equal(poll.Nominations[2].Id, poll.Nominations[1].Id);
    }

    [Test]
    [Category("FR-39")]
    public async Task GetBallotAsync_NominationsWithoutAnOrder_FallBackToName()
    {
        var poll = await CreatePollingElectionAsync(voters: 1);
        (await _context.Members.FindAsync(poll.Nominations[1].CandidateMemberId))!.FullName = "Zara";
        (await _context.Members.FindAsync(poll.Nominations[2].CandidateMemberId))!.FullName = "Adil";
        await _context.SaveChangesAsync();

        var ballot = await new ElectionService(_context).GetBallotAsync(poll.Election.Id, poll.Voters[0].Id);

        ballot!.Seats[1].Candidates.Select(c => c.Name).Should().Equal("Adil", "Zara");
    }

    [Test]
    [Category("FR-39")]
    public async Task GetCurrentAsync_SkipsATestElection()
    {
        var period = new ECPeriod { Title = "2026", StartDate = DateTime.UtcNow.AddDays(-1) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        _context.Elections.Add(new Election { Title = "Rehearsal", ECPeriodId = period.Id, Phase = ElectionPhase.Polling, IsTest = true, NominationOpensOn = DateTime.UtcNow, NominationClosesOn = DateTime.UtcNow.AddDays(1), PollingOpensOn = DateTime.UtcNow.AddDays(2), PollingClosesOn = DateTime.UtcNow.AddDays(3) });
        await _context.SaveChangesAsync();
        var service = new ElectionService(_context);

        (await service.GetCurrentAsync()).Should().BeNull();
        (await service.IsTestAsync(_context.Elections.Single().Id)).Should().BeTrue();
    }

    [Test]
    [Category("FR-39")]
    public async Task DeclareAsync_TestElection_WritesNoCommitteeMembers()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount);
        poll.Election.IsTest = true;
        await _context.SaveChangesAsync();
        var service = new ElectionService(_context);
        await CastAllAsync(service, poll);
        await CloseAndStartCountingAsync(service, poll.Election);
        await service.CountAsync(poll.Election.Id, OfficerPrivateKey);

        (await service.DeclareAsync(poll.Election.Id)).Should().BeTrue();

        _context.ECMembers.Should().BeEmpty();
        _context.Elections.AsNoTracking().Single().Phase.Should().Be(ElectionPhase.Declared);
    }

    [Test]
    [Category("FR-39")]
    public async Task CreateAsync_KeepsTheTestFlag()
    {
        var period = new ECPeriod { Title = "Next", StartDate = DateTime.UtcNow.AddDays(30) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();

        var (created, _) = await new ElectionService(_context).CreateAsync(new CreateElectionDto(
            "Rehearsal", period.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(3), 1, IsTest: true));

        created.Should().NotBeNull();
        (await new ElectionService(_context).GetAdminElectionAsync(created!.Id))!.IsTest.Should().BeTrue();
    }

    [Test]
    [Category("FR-39")]
    public async Task CreateAsync_SetsScrutinyAndWithdrawalDates()
    {
        var period = new ECPeriod { Title = "Next", StartDate = DateTime.UtcNow.AddDays(30) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        var start = DateTime.UtcNow.Date.AddDays(1);

        var (created, error) = await new ElectionService(_context).CreateAsync(new CreateElectionDto(
            "Election", period.Id, start, start.AddDays(5), start.AddDays(10), start.AddDays(12), 1,
            ElectionTieRule.RunOff, ScrutinyOn: start.AddDays(6), WithdrawalClosesOn: start.AddDays(8)));

        error.Should().BeNull();
        var stored = _context.Elections.Single(x => x.Id == created!.Id);
        stored.ScrutinyOn.Should().Be(start.AddDays(6));
        stored.WithdrawalClosesOn.Should().Be(start.AddDays(8));
        stored.TieRule.Should().Be(ElectionTieRule.RunOff);
    }

    [Test]
    [Category("FR-39")]
    public async Task CreateAsync_DatesOutOfOrder_Refused()
    {
        var period = new ECPeriod { Title = "Next", StartDate = DateTime.UtcNow.AddDays(30) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        var start = DateTime.UtcNow.Date.AddDays(1);
        var service = new ElectionService(_context);

        // Scrutiny before nominations close, then a withdrawal deadline after polling opens.
        var early = await service.CreateAsync(new CreateElectionDto(
            "Election", period.Id, start, start.AddDays(5), start.AddDays(10), start.AddDays(12), 1, ScrutinyOn: start.AddDays(4)));
        var late = await service.CreateAsync(new CreateElectionDto(
            "Election", period.Id, start, start.AddDays(5), start.AddDays(10), start.AddDays(12), 1, WithdrawalClosesOn: start.AddDays(11)));

        early.Error.Should().Be("dates-out-of-order");
        late.Error.Should().Be("dates-out-of-order");
        _context.Elections.Should().BeEmpty();
    }

    [Test]
    [Category("NFR-R5")]
    public async Task CreateAsync_MainDateMissing_Refused()
    {
        var period = new ECPeriod { Title = "Next", StartDate = DateTime.UtcNow.AddDays(30) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        var start = DateTime.UtcNow.Date.AddDays(1);

        // An empty date input reaches the server as DateTime.MinValue.
        var (created, error) = await new ElectionService(_context).CreateAsync(new CreateElectionDto(
            "Election", period.Id, default, default, default, default, 1, ScrutinyOn: start, WithdrawalClosesOn: start.AddDays(2)));

        created.Should().BeNull();
        error.Should().Be("dates-missing");
    }

    [Test]
    [Category("FR-39")]
    public async Task CreateAsync_DateWithNoZone_StoredAsUtcWithoutShift()
    {
        var period = new ECPeriod { Title = "Next", StartDate = DateTime.UtcNow.AddDays(60) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        // Built the way a bare yyyy-MM-dd from the web form binds, with no zone.
        var start = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(1), DateTimeKind.Unspecified);

        var (created, error) = await new ElectionService(_context).CreateAsync(new CreateElectionDto(
            "Election", period.Id, start, start.AddDays(5), start.AddDays(10), start.AddDays(12), 1, ScrutinyOn: start.AddDays(6)));

        error.Should().BeNull();
        var stored = _context.Elections.Single(x => x.Id == created!.Id);
        stored.NominationOpensOn.Should().Be(DateTime.SpecifyKind(start, DateTimeKind.Utc));
        stored.NominationOpensOn.Kind.Should().Be(DateTimeKind.Utc);
        stored.ScrutinyOn.Should().Be(DateTime.SpecifyKind(start.AddDays(6), DateTimeKind.Utc));
        stored.PollingClosesOn.Should().Be(DateTime.SpecifyKind(start.AddDays(12), DateTimeKind.Utc));
    }

    [Test]
    [Category("FR-39")]
    public async Task CountAsync_TwoPicksOnATwoPlaceSeat_CountsBoth()
    {
        var poll = await CreatePollingElectionAsync(voters: Constants.Elections.MinimumBallotsToCount);
        var service = new ElectionService(_context);
        foreach (var voter in poll.Voters)
        {
            var ballot = new CastBallotDto([
                new BallotSeatChoiceDto(poll.Seats[0].Id, [poll.Nominations[0].Id]),
                new BallotSeatChoiceDto(poll.Seats[1].Id, [poll.Nominations[1].Id, poll.Nominations[2].Id]),
            ]);
            (await service.CastBallotAsync(poll.Election.Id, voter.Id, ballot)).Success.Should().BeTrue();
        }
        await CloseAndStartCountingAsync(service, poll.Election);

        var results = (await service.CountAsync(poll.Election.Id, OfficerPrivateKey)).Results!;

        results.Where(x => x.ElectionSeatId == poll.Seats[1].Id).Select(x => x.VoteCount)
            .Should().Equal(Constants.Elections.MinimumBallotsToCount, Constants.Elections.MinimumBallotsToCount);
    }

    // One President seat in Withdrawal, the phase before CandidateList, one accepted nomination per name, in the order given.
    private async Task<(Election Election, Nomination[] Nominations)> CreateWithdrawalElectionAsync(params string[] names)
    {
        var candidates = new List<Member>();
        for (var i = 0; i < names.Length; i++)
            candidates.Add(await CreateAndSaveTestMemberAsync(names[i], $"cand{i}@example.com", $"018{i:D8}", $"80{i:D8}"));
        var proposer = await CreateAndSaveTestMemberAsync("Proposer", "proposer@example.com", "01900000001", "7000000001");
        var seconder = await CreateAndSaveTestMemberAsync("Seconder", "seconder@example.com", "01900000002", "7000000002");
        var election = new Election
        {
            Title = "Election",
            ECPeriod = new ECPeriod { Title = "2026", StartDate = DateTime.UtcNow.AddDays(30) },
            Phase = ElectionPhase.Withdrawal,
            NominationOpensOn = DateTime.UtcNow.AddDays(-3),
            NominationClosesOn = DateTime.UtcNow.AddDays(-2),
            PollingOpensOn = DateTime.UtcNow.AddDays(1),
            PollingClosesOn = DateTime.UtcNow.AddDays(2),
        };
        var seat = new ElectionSeat { Election = election, Position = ECPosition.President };
        var nominations = candidates.Select(c => new Nomination
        {
            Election = election,
            ElectionSeat = seat,
            CandidateMemberId = c.Id,
            ProposerMemberId = proposer.Id,
            SeconderMemberId = seconder.Id,
            Statement = "Run",
            Status = NominationStatus.Accepted,
            SubmittedAt = DateTime.UtcNow,
        }).ToArray();
        _context.Elections.Add(election);
        _context.ElectionSeats.Add(seat);
        _context.Nominations.AddRange(nominations);
        await _context.SaveChangesAsync();
        return (election, nominations);
    }

    private sealed record PollingElection(Election Election, ElectionSeat[] Seats, Nomination[] Nominations, Member[] Voters)
    {
        // Every voter picks the President candidate. On the two-place seat, even voters pick
        // the second nomination and odd voters the third.
        public CastBallotDto BallotFor(int voter) => new([
            new BallotSeatChoiceDto(Seats[0].Id, [Nominations[0].Id]),
            new BallotSeatChoiceDto(Seats[1].Id, [Nominations[1 + voter % 2].Id]),
        ]);
    }

    // Seat 0 is President with one place and seat 1 has two places. Nominations 0 to 2 are the
    // President candidate and the two seat-1 candidates. A fourth, for President, is optional.
    private async Task<PollingElection> CreatePollingElectionAsync(int voters, bool secondPresidentCandidate = false)
    {
        var members = new List<Member>();
        for (var i = 0; i < voters + 4; i++)
            members.Add(await CreateAndSaveTestMemberAsync($"Member {i}", $"member{i}@example.com", $"017{i:D8}", $"90{i:D8}"));
        var candidates = members.Take(4).ToArray();
        var electors = members.Skip(4).ToArray();

        var election = new Election
        {
            Title = "Election",
            ECPeriod = new ECPeriod { Title = "2026", StartDate = DateTime.UtcNow.AddDays(-1) },
            Phase = ElectionPhase.Polling,
            NominationOpensOn = DateTime.UtcNow.AddDays(-3),
            NominationClosesOn = DateTime.UtcNow.AddDays(-2),
            PollingOpensOn = DateTime.UtcNow.AddHours(-1),
            PollingClosesOn = DateTime.UtcNow.AddHours(1),
            BallotPublicKey = OfficerPublicKey,
            BallotKeyFingerprint = BallotSeal.Fingerprint(OfficerPublicKey),
        };
        var seats = new[]
        {
            new ElectionSeat { Election = election, Position = ECPosition.President },
            new ElectionSeat { Election = election, Position = ECPosition.Member1, SeatCount = 2 },
        };
        _context.Elections.Add(election);
        _context.ElectionSeats.AddRange(seats);
        foreach (var m in members)
            _context.VoterRolls.Add(new VoterRoll { Election = election, MemberId = m.Id, IsEligible = true, FrozenAt = DateTime.UtcNow });

        var entries = new List<(ElectionSeat Seat, Member Candidate)> { (seats[0], candidates[0]), (seats[1], candidates[1]), (seats[1], candidates[2]) };
        if (secondPresidentCandidate)
            entries.Add((seats[0], candidates[3]));
        var nominations = entries.Select(x => new Nomination
        {
            Election = election,
            ElectionSeat = x.Seat,
            CandidateMemberId = x.Candidate.Id,
            ProposerMemberId = candidates.First(c => c != x.Candidate).Id,
            SeconderMemberId = candidates.Last(c => c != x.Candidate).Id,
            Statement = "Run",
            Status = NominationStatus.Accepted,
            SubmittedAt = DateTime.UtcNow,
        }).ToArray();
        _context.Nominations.AddRange(nominations);
        await _context.SaveChangesAsync();
        return new PollingElection(election, seats, nominations, electors);
    }

    private async Task ClosePollingAsync(Election election)
    {
        election.PollingClosesOn = DateTime.UtcNow.AddMinutes(-1);
        await _context.SaveChangesAsync();
    }

    private async Task CloseAndStartCountingAsync(ElectionService service, Election election)
    {
        await ClosePollingAsync(election);
        (await service.SetPhaseAsync(election.Id, ElectionPhase.Counting)).Should().BeTrue();
    }

    private static async Task CastAllAsync(ElectionService service, PollingElection poll)
    {
        for (var i = 0; i < poll.Voters.Length; i++)
            (await service.CastBallotAsync(poll.Election.Id, poll.Voters[i].Id, poll.BallotFor(i))).Success.Should().BeTrue();
    }
}
