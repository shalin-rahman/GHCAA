using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services;

// Spec 023 FR-033 (37.12i): the public board shows who runs the election and nothing private.
[TestFixture]
[Category("FR-33")]
public sealed class ElectionBoardTests : TestBase
{
    private Election _election = null!;
    private int _next;

    [SetUp]
    public async Task SetUpFixture()
    {
        var period = new ECPeriod { Title = "2027", StartDate = DateTime.UtcNow.AddDays(-1) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        _election = new Election { Title = "Board Election", ECPeriodId = period.Id, NominationOpensOn = DateTime.UtcNow, NominationClosesOn = DateTime.UtcNow.AddDays(1), PollingOpensOn = DateTime.UtcNow.AddDays(2), PollingClosesOn = DateTime.UtcNow.AddDays(3) };
        _context.Elections.Add(_election);
        await _context.SaveChangesAsync();
    }

    private ElectionService NewService(bool showTurnoutDuringPolling = false)
    {
        var config = new Mock<IOrgConfigService>();
        config.Setup(x => x.GetConfigAsync()).ReturnsAsync(new OrgConfigDto { Elections = new ElectionSettingsDto { ShowTurnoutDuringPolling = showTurnoutDuringPolling } });
        return new ElectionService(_context, null, config.Object);
    }

    private async Task<ElectionPersona> AddPersonaAsync(string name, bool isPublic, int sortOrder = 0, string group = "Officials")
    {
        var persona = new ElectionPersona { Name = name, GroupName = group, Description = "d", DeclarationText = "t", ShowOnPublicBoard = isPublic, SortOrder = sortOrder, IsActive = true };
        _context.ElectionPersonas.Add(persona);
        await _context.SaveChangesAsync();
        return persona;
    }

    private async Task AddAppointmentAsync(ElectionPersona persona, string name, bool accepted = true, bool revoked = false, string? revokedReason = null)
    {
        var user = new User { Username = "u" + ++_next, PasswordHash = "x" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        var now = DateTime.UtcNow;
        _context.ElectionAppointments.Add(new ElectionAppointment
        {
            ElectionId = _election.Id,
            PersonaId = persona.Id,
            UserId = user.Id,
            DisplayName = name,
            Email = name.Replace(' ', '.') + "@example.com",
            Phone = "01700000000",
            AppointedAt = now.AddDays(-2),
            AcceptedAt = accepted ? now.AddDays(-1) : null,
            DeclarationSignedAt = accepted ? now.AddDays(-1) : null,
            RevokedAt = revoked ? now : null,
            RevokedReason = revokedReason
        });
        await _context.SaveChangesAsync();
    }

    private async Task SetPhaseAsync(ElectionPhase phase)
    {
        _election.Phase = phase;
        await _context.SaveChangesAsync();
    }

    private async Task AddRollAsync(int voted, int notVoted = 0)
    {
        for (var i = 0; i < voted + notVoted; i++)
        {
            var member = new Member
            {
                FullName = "M" + ++_next,
                FatherName = "F",
                MotherName = "Mo",
                Email = $"m{_next}@example.com",
                NID = "N" + _next,
                MobileNo = "0170" + _next.ToString("D7"),
                PresentAddress = "A",
                PermanentAddress = "A",
                EmergencyContactName = "E",
                EmergencyContactRelation = "R",
                EmergencyContactPhone = "0"
            };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();
            _context.VoterRolls.Add(new VoterRoll { ElectionId = _election.Id, MemberId = member.Id, IsEligible = true, FrozenAt = DateTime.UtcNow, VotedAt = i < voted ? DateTime.UtcNow : null });
        }
        await _context.SaveChangesAsync();
    }

    [Test]
    public async Task Board_UnknownElection_ReturnsNull()
    {
        (await NewService().GetBoardAsync(9999)).Should().BeNull();
    }

    [Test]
    public async Task Board_OnlyAcceptedPublicPersonas()
    {
        var publicPersona = await AddPersonaAsync("Board Public", true);
        var hiddenPersona = await AddPersonaAsync("Board Hidden", false);
        await AddAppointmentAsync(publicPersona, "Shown Person");
        await AddAppointmentAsync(publicPersona, "Pending Person", accepted: false);
        await AddAppointmentAsync(publicPersona, "Revoked Person", revoked: true);
        await AddAppointmentAsync(hiddenPersona, "Hidden Person");

        var board = await NewService().GetBoardAsync(_election.Id);

        board!.Groups.SelectMany(g => g.Members).Select(m => m.DisplayName).Should().Equal("Shown Person");
    }

    [Test]
    public async Task Board_ArchivedElection_KeepsOfficialsEndedByTheArchive()
    {
        var persona = await AddPersonaAsync("Board Archived", true);
        await AddAppointmentAsync(persona, "Ended With Election", revoked: true, revokedReason: Constants.Elections.ElectionArchivedReason);
        await AddAppointmentAsync(persona, "Removed Earlier", revoked: true, revokedReason: "Conflict of interest");
        await AddAppointmentAsync(persona, "Never Accepted", accepted: false, revoked: true, revokedReason: Constants.Elections.ElectionArchivedReason);
        await SetPhaseAsync(ElectionPhase.Archived);

        var board = await NewService().GetBoardAsync(_election.Id);

        board!.Groups.SelectMany(g => g.Members).Select(m => m.DisplayName).Should().Equal("Ended With Election");
    }

    [Test]
    public async Task Board_ArchiveReasonBeforeArchive_StaysOff()
    {
        var persona = await AddPersonaAsync("Board Early", true);
        await AddAppointmentAsync(persona, "Revoked Early", revoked: true, revokedReason: Constants.Elections.ElectionArchivedReason);

        (await NewService().GetBoardAsync(_election.Id))!.Groups.Should().BeEmpty();
    }

    [Test]
    public async Task Board_TurnoutIgnoresVotersMarkedIneligible()
    {
        await SetPhaseAsync(ElectionPhase.Counting);
        await AddRollAsync(Constants.Elections.MinimumBallotsToCount + 1);
        var extra = await _context.VoterRolls.OrderBy(x => x.Id).FirstAsync();
        extra.IsEligible = false;
        await _context.SaveChangesAsync();

        (await NewService().GetBoardAsync(_election.Id))!.Turnout.Should()
            .Be(new ElectionBoardTurnoutDto(Constants.Elections.MinimumBallotsToCount, Constants.Elections.MinimumBallotsToCount));
    }

    [Test]
    public async Task Board_SortedBySortOrder()
    {
        var late = await AddPersonaAsync("Board Late", true, sortOrder: 20, group: "Later");
        var early = await AddPersonaAsync("Board Early", true, sortOrder: 10, group: "Earlier");
        await AddAppointmentAsync(late, "Late Person");
        await AddAppointmentAsync(early, "Early Person");

        var board = await NewService().GetBoardAsync(_election.Id);

        board!.Groups.Select(g => g.GroupName).Should().Equal("Earlier", "Later");
        board.Groups[0].Members[0].PersonaName.Should().Be("Board Early");
    }

    [Test]
    public async Task Board_TurnoutHiddenDuringPolling()
    {
        await SetPhaseAsync(ElectionPhase.Polling);
        await AddRollAsync(Constants.Elections.MinimumBallotsToCount, 2);

        (await NewService(showTurnoutDuringPolling: false).GetBoardAsync(_election.Id))!.Turnout.Should().BeNull();
    }

    [Test]
    public async Task Board_TurnoutShownWhenSettingOn()
    {
        await SetPhaseAsync(ElectionPhase.Polling);
        await AddRollAsync(Constants.Elections.MinimumBallotsToCount, 2);

        var turnout = (await NewService(showTurnoutDuringPolling: true).GetBoardAsync(_election.Id))!.Turnout;

        turnout.Should().Be(new ElectionBoardTurnoutDto(Constants.Elections.MinimumBallotsToCount, Constants.Elections.MinimumBallotsToCount + 2));
    }

    [Test]
    public async Task Board_TurnoutShownAfterPolling()
    {
        await SetPhaseAsync(ElectionPhase.Counting);
        await AddRollAsync(Constants.Elections.MinimumBallotsToCount);

        (await NewService().GetBoardAsync(_election.Id))!.Turnout.Should().NotBeNull();
    }

    [Test]
    public async Task Board_TurnoutHiddenBelowMinimum()
    {
        await SetPhaseAsync(ElectionPhase.Declared);
        await AddRollAsync(Constants.Elections.MinimumBallotsToCount - 1, 5);

        (await NewService().GetBoardAsync(_election.Id))!.Turnout.Should().BeNull();
    }

    [Test]
    public async Task Board_AuditHashOnlyAfterDeclared()
    {
        var writer = new ElectionAuditWriter(_context);
        await writer.AppendAsync(_election.Id, Constants.Elections.AuditActions.Counted, null, null);
        await writer.SaveAsync();
        await SetPhaseAsync(ElectionPhase.Counting);

        (await NewService().GetBoardAsync(_election.Id))!.AuditHeadHash.Should().BeNull();

        await SetPhaseAsync(ElectionPhase.Declared);
        var head = _context.ElectionAuditEntries.Single().Hash;
        (await NewService().GetBoardAsync(_election.Id))!.AuditHeadHash.Should().Be(head);
    }

    // 37.12p: archiving adds a revoke row per appointment after the declaration. The board
    // head must stay on the Declared row so a hash copied on the day still matches.
    [Test]
    public async Task Board_AuditHeadStaysOnDeclaredRowAfterLaterRows()
    {
        var writer = new ElectionAuditWriter(_context);
        await writer.AppendAsync(_election.Id, Constants.Elections.AuditActions.Declared, null, null);
        await writer.SaveAsync();
        var declared = _context.ElectionAuditEntries.Single().Hash;
        await writer.AppendAsync(_election.Id, Constants.Elections.AuditActions.AppointmentRevoked, null, null);
        await writer.SaveAsync();
        await SetPhaseAsync(ElectionPhase.Declared);

        (await NewService().GetBoardAsync(_election.Id))!.AuditHeadHash.Should().Be(declared);
    }

    [Test]
    public async Task Board_IsReadableForATestElection()
    {
        _election.IsTest = true;
        await _context.SaveChangesAsync();

        (await NewService().GetBoardAsync(_election.Id))!.IsTest.Should().BeTrue();
    }

    [Test]
    public void BoardDto_ContainsNoContactFields()
    {
        var banned = new[] { "email", "phone", "mobile", "ip", "userid", "memberid" };
        var types = new[] { typeof(ElectionBoardDto), typeof(ElectionBoardGroupDto), typeof(ElectionBoardMemberDto), typeof(ElectionBoardTurnoutDto) };

        var names = types.SelectMany(t => t.GetProperties()).Select(p => p.Name.ToLowerInvariant());

        names.Where(n => banned.Any(b => n == b || n.Contains(b))).Should().BeEmpty();
    }
}
