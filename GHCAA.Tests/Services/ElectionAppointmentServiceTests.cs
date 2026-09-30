using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Options;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services;

[TestFixture]
[Category("FR-39")]
public sealed class ElectionAppointmentServiceTests : TestBase
{
    private Mock<ITokenService> _tokens = null!;
    private ElectionAppointmentService _service = null!;
    private Election _election = null!;
    private ElectionPersona _persona = null!;
    private User _admin = null!;

    [SetUp]
    public async Task SetUpFixture()
    {
        _tokens = new Mock<ITokenService>();
        var orgConfig = new Mock<IOrgConfigService>();
        orgConfig.Setup(s => s.GetConfigAsync()).ReturnsAsync(new OrgConfigDto());
        _service = new ElectionAppointmentService(_context, orgConfig.Object, new ElectionAccessService(_context, orgConfig.Object), new Mock<ICommunicationService>().Object,
            new Mock<INotificationService>().Object, _tokens.Object,
            Microsoft.Extensions.Options.Options.Create(new AppSettingsOptions()), NullLogger<ElectionAppointmentService>.Instance);

        var period = new ECPeriod { Title = "2027", StartDate = DateTime.UtcNow.AddDays(-1) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        _election = new Election { Title = "Election", ECPeriodId = period.Id, NominationOpensOn = DateTime.UtcNow, NominationClosesOn = DateTime.UtcNow.AddDays(1), PollingOpensOn = DateTime.UtcNow.AddDays(2), PollingClosesOn = DateTime.UtcNow.AddDays(3) };
        _persona = new ElectionPersona { Name = "Scrutineer Test", GroupName = "Officials", Description = "d", DeclarationText = "I will be fair.", Permissions = ElectionPermission.DecideNominations, IsActive = true };
        _context.Elections.Add(_election);
        _context.ElectionPersonas.Add(_persona);
        _admin = new User { Username = "admin-test", PasswordHash = "x" };
        _admin.Roles.Add(await RoleAsync(Constants.Roles.Admin));
        _context.Users.Add(_admin);
        await _context.SaveChangesAsync();
    }

    private async Task<Role> RoleAsync(string name) =>
        await _context.Roles.FirstOrDefaultAsync(r => r.Name == name) ?? _context.Roles.Add(new Role { Name = name }).Entity;

    private Task<(bool Success, string? Error, ElectionAppointmentDto? Appointment)> AppointOutsiderAsync(string email = "outsider@example.com") =>
        _service.AppointAsync(_election.Id, new AppointDto { PersonaId = _persona.Id, DisplayName = "Out Sider", Email = email }, _admin.Id, CancellationToken.None);

    private Task<(bool Success, string? Error)> AcceptAsync(ElectionAppointmentDto a) =>
        _service.AcceptAsync(a.Id, a.UserId, new AcceptAppointmentDto { AgreeToDeclaration = true }, "10.0.0.1", CancellationToken.None);

    [Test]
    public async Task Appoint_Member_UsesTheMembersUser()
    {
        var member = await CreateAndSaveTestMemberAsync();
        var user = new User { Username = "member-login", PasswordHash = "x", MemberId = member.Id };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var (success, _, appointment) = await _service.AppointAsync(_election.Id, new AppointDto { PersonaId = _persona.Id, MemberId = member.Id }, _admin.Id, CancellationToken.None);

        success.Should().BeTrue();
        appointment!.UserId.Should().Be(user.Id);
        appointment.Email.Should().Be(member.Email);
    }

    [Test]
    public async Task Appoint_NonMember_CreatesOneUserAndReusesItForTheSameEmail()
    {
        var first = (await AppointOutsiderAsync()).Appointment!;
        var second = new ElectionPersona { Name = "Polling Test", GroupName = "Officials", Description = "d", DeclarationText = "t", IsActive = true };
        _context.ElectionPersonas.Add(second);
        await _context.SaveChangesAsync();

        var again = (await _service.AppointAsync(_election.Id, new AppointDto { PersonaId = second.Id, DisplayName = "Out Sider", Email = "OUTSIDER@example.com" }, _admin.Id, CancellationToken.None)).Appointment!;

        again.UserId.Should().Be(first.UserId);
        var user = _context.Users.Single(u => u.Username == "outsider@example.com");
        user.MustChangePassword.Should().BeTrue();
        user.MemberId.Should().BeNull();
    }

    [Test]
    public async Task Appoint_NonMemberPathWithAMembersEmail_IsRefused()
    {
        var member = await CreateAndSaveTestMemberAsync(email: "taken@example.com");

        var (success, error, _) = await AppointOutsiderAsync(member.Email);

        success.Should().BeFalse();
        error.Should().Be("email-is-member");
    }

    [Test]
    public async Task Appointment_IsNotLiveUntilAccepted()
    {
        var appointment = (await AppointOutsiderAsync()).Appointment!;

        appointment.IsLive.Should().BeFalse();
        (await _service.ListLiveSummariesAsync(appointment.UserId, CancellationToken.None)).Should().BeEmpty();
        (await _service.AcceptAsync(appointment.Id, appointment.UserId, new AcceptAppointmentDto(), null, CancellationToken.None)).Error.Should().Be("declaration-required");
    }

    [Test]
    public async Task Accept_StoresTheDeclarationAndAddsTheRoleOnce()
    {
        var appointment = (await AppointOutsiderAsync()).Appointment!;
        var stampBefore = _context.Users.Single(u => u.Id == appointment.UserId).SecurityStamp;

        (await AcceptAsync(appointment)).Success.Should().BeTrue();
        _persona.DeclarationText = "Changed later.";
        await _context.SaveChangesAsync();

        var row = _context.ElectionAppointments.Single();
        row.DeclarationTextSnapshot.Should().Be("I will be fair.");
        row.SignedFromIp.Should().Be("10.0.0.1");
        var user = _context.Users.Include(u => u.Roles).Single(u => u.Id == appointment.UserId);
        user.Roles.Count(r => r.Name == Constants.Roles.ElectionOfficial).Should().Be(1);
        user.SecurityStamp.Should().NotBe(stampBefore);
        (await _service.ListLiveSummariesAsync(appointment.UserId, CancellationToken.None)).Should().ContainSingle();
        (await AcceptAsync(appointment)).Error.Should().Be("already-accepted");
    }

    [Test]
    public async Task Revoke_LastLiveAppointment_RemovesRoleRotatesStampAndDeactivatesNonMember()
    {
        var appointment = (await AppointOutsiderAsync()).Appointment!;
        await AcceptAsync(appointment);
        var stampBefore = _context.Users.Single(u => u.Id == appointment.UserId).SecurityStamp;

        (await _service.RevokeAsync(appointment.Id, _admin.Id, "Stood down", CancellationToken.None)).Success.Should().BeTrue();

        var user = _context.Users.Include(u => u.Roles).Single(u => u.Id == appointment.UserId);
        user.Roles.Should().BeEmpty();
        user.SecurityStamp.Should().NotBe(stampBefore);
        user.IsActive.Should().BeFalse();
        _tokens.Verify(t => t.RevokeAllRefreshTokensAsync(appointment.UserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Revoke_OneOfTwo_LeavesTheRole()
    {
        var first = (await AppointOutsiderAsync()).Appointment!;
        var second = new ElectionPersona { Name = "Polling Test", GroupName = "Officials", Description = "d", DeclarationText = "t", IsActive = true };
        _context.ElectionPersonas.Add(second);
        await _context.SaveChangesAsync();
        var other = (await _service.AppointAsync(_election.Id, new AppointDto { PersonaId = second.Id, DisplayName = "Out Sider", Email = "outsider@example.com" }, _admin.Id, CancellationToken.None)).Appointment!;
        await AcceptAsync(first);
        await AcceptAsync(other);

        await _service.RevokeAsync(first.Id, _admin.Id, null, CancellationToken.None);

        var user = _context.Users.Include(u => u.Roles).Single(u => u.Id == first.UserId);
        user.Roles.Should().Contain(r => r.Name == Constants.Roles.ElectionOfficial);
        user.IsActive.Should().BeTrue();
        _tokens.Verify(t => t.RevokeAllRefreshTokensAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Appoint_AfterTakeover_RefusesPlainAdmin()
    {
        var commission = new ElectionPersona { Name = "Commission Test", GroupName = "Commission", Description = "d", DeclarationText = "t", TakesOverFromAdmin = true, Permissions = ElectionPermission.AppointOfficials, IsActive = true };
        _context.ElectionPersonas.Add(commission);
        await _context.SaveChangesAsync();
        var chief = (await _service.AppointAsync(_election.Id, new AppointDto { PersonaId = commission.Id, DisplayName = "Chief", Email = "chief@example.com" }, _admin.Id, CancellationToken.None)).Appointment!;
        await AcceptAsync(chief);

        (await AppointOutsiderAsync()).Error.Should().Be("forbidden");
        (await _service.AppointAsync(_election.Id, new AppointDto { PersonaId = _persona.Id, DisplayName = "Out Sider", Email = "outsider@example.com" }, chief.UserId, CancellationToken.None)).Success.Should().BeTrue();
    }
}
