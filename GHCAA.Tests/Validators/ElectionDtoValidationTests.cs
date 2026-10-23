using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using GHCAA.Application.DTOs;

namespace GHCAA.Tests.Validators;

// TODO 84.36. The election request rules moved from unused FluentValidation validators onto the DTOs,
// where [ApiController] runs them. Validator.TryValidateObject runs the same Validate method.
[TestFixture]
[Category("FR-39")]
public class ElectionDtoValidationTests
{
    private static readonly DateTime Opens = new(2027, 1, 1);

    private static List<ValidationResult> Validate(object dto)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), results, true);
        return results;
    }

    private static CreateElectionDto Election(string title = "EC 2027", int period = 1, DateTime? nominationOpens = null,
        int closesAfter = 10, int pollOpensAfter = 20, int pollClosesAfter = 21)
    {
        var opens = nominationOpens ?? Opens;
        return new CreateElectionDto(title, period, opens, Opens.AddDays(closesAfter), Opens.AddDays(pollOpensAfter), Opens.AddDays(pollClosesAfter), 1);
    }

    [Test]
    public void CreateElection_ValidRequest_HasNoErrors()
    {
        Validate(Election()).Should().BeEmpty();
    }

    [TestCase("")]
    [TestCase("   ")]
    public void CreateElection_BlankTitle_IsRejected(string title)
    {
        Validate(Election(title: title)).Should().ContainSingle(r => r.MemberNames.Contains(nameof(CreateElectionDto.Title)));
    }

    [Test]
    public void CreateElection_TitleOver200_IsRejected()
    {
        Validate(Election(title: new string('x', 201))).Should().ContainSingle(r => r.MemberNames.Contains(nameof(CreateElectionDto.Title)));
        Validate(Election(title: new string('x', 200))).Should().BeEmpty();
    }

    [Test]
    public void CreateElection_NoPeriod_IsRejected()
    {
        Validate(Election(period: 0)).Should().ContainSingle(r => r.MemberNames.Contains(nameof(CreateElectionDto.ECPeriodId)));
    }

    [Test]
    public void CreateElection_MissingNominationOpening_IsRejected()
    {
        Validate(Election(nominationOpens: default(DateTime))).Should().Contain(r => r.MemberNames.Contains(nameof(CreateElectionDto.NominationOpensOn)));
    }

    [TestCase(0, 20, 21, nameof(CreateElectionDto.NominationClosesOn))]
    [TestCase(10, 10, 21, nameof(CreateElectionDto.PollingOpensOn))]
    [TestCase(10, 20, 20, nameof(CreateElectionDto.PollingClosesOn))]
    public void CreateElection_DatesOutOfOrder_NameTheLateField(int closes, int pollOpens, int pollCloses, string field)
    {
        Validate(Election(closesAfter: closes, pollOpensAfter: pollOpens, pollClosesAfter: pollCloses))
            .Should().ContainSingle(r => r.MemberNames.Contains(field));
    }

    [TestCase(1, 1)]
    [TestCase(1, 20)]
    public void ElectionSeat_InRange_HasNoErrors(int position, int seats)
    {
        Validate(new ElectionSeatRequestDto(position, seats)).Should().BeEmpty();
    }

    [TestCase(0, 1, nameof(ElectionSeatRequestDto.PositionId))]
    [TestCase(1, 0, nameof(ElectionSeatRequestDto.SeatCount))]
    [TestCase(1, 21, nameof(ElectionSeatRequestDto.SeatCount))]
    public void ElectionSeat_OutOfRange_IsRejected(int position, int seats, string field)
    {
        Validate(new ElectionSeatRequestDto(position, seats)).Should().ContainSingle(r => r.MemberNames.Contains(field));
    }

    [Test]
    public void Nomination_ProposerLeftAtZero_IsAccepted()
    {
        // The controller sets the proposer from the caller after binding.
        Validate(new NominationDto(1, 2, 0, 3, "My statement", null)).Should().BeEmpty();
    }

    [TestCase(0, 2, 3, "ok", nameof(NominationDto.ElectionSeatId))]
    [TestCase(1, 0, 3, "ok", nameof(NominationDto.CandidateMemberId))]
    [TestCase(1, 2, 0, "ok", nameof(NominationDto.SeconderMemberId))]
    [TestCase(1, 2, 2, "ok", nameof(NominationDto.SeconderMemberId))]
    [TestCase(1, 2, 3, " ", nameof(NominationDto.Statement))]
    public void Nomination_BadField_IsRejected(int seat, int candidate, int seconder, string statement, string field)
    {
        Validate(new NominationDto(seat, candidate, 0, seconder, statement, null)).Should().ContainSingle(r => r.MemberNames.Contains(field));
    }

    [Test]
    public void Nomination_StatementOver4000_IsRejected()
    {
        Validate(new NominationDto(1, 2, 0, 3, new string('s', 4001), null))
            .Should().ContainSingle(r => r.MemberNames.Contains(nameof(NominationDto.Statement)));
    }
}
