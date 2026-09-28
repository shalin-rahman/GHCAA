using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using System.Security.Cryptography;
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
        BallotSeal.Open(OfficerKey, stored[0]).Should().Contain($"{poll.Nominations[0].Id}");
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
        _context.Elections.Single().Phase.Should().Be(ElectionPhase.Declared);
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
