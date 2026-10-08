using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Moq;
using NUnit.Framework;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services;

[TestFixture]
[Category("FR-39")]
public sealed class ElectionAccessServiceTests : TestBase
{
    private static readonly string[] Admin = [Constants.Roles.Admin];
    private static readonly string[] Official = [Constants.Roles.ElectionOfficial];

    private Mock<IOrgConfigService> _orgConfig = null!;
    private ElectionAccessService _service = null!;
    private Election _election = null!;
    private ElectionPersona _scrutineer = null!;
    private ElectionPersona _returningOfficer = null!;

    [SetUp]
    public async Task SetUpFixture()
    {
        _users.Clear();
        _orgConfig = new Mock<IOrgConfigService>();
        _orgConfig.Setup(s => s.GetConfigAsync()).ReturnsAsync(new OrgConfigDto());
        _service = new ElectionAccessService(_context, _orgConfig.Object);

        var period = new ECPeriod { Title = "2027", StartDate = DateTime.UtcNow.AddDays(-1) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        _election = new Election { Title = "Election", ECPeriodId = period.Id, NominationOpensOn = DateTime.UtcNow, NominationClosesOn = DateTime.UtcNow.AddDays(1), PollingOpensOn = DateTime.UtcNow.AddDays(2), PollingClosesOn = DateTime.UtcNow.AddDays(3) };
        _scrutineer = new ElectionPersona { Name = "Scrutineer", GroupName = "Officials", Description = "d", DeclarationText = "t", Permissions = ElectionPermission.DecideNominations | ElectionPermission.ViewDashboard, IsActive = true };
        _returningOfficer = new ElectionPersona { Name = "Returning Officer", GroupName = "Commission", Description = "d", DeclarationText = "t", Permissions = ElectionPermission.Count | ElectionPermission.Declare, TakesOverFromAdmin = true, IsActive = true };
        _context.Elections.Add(_election);
        _context.ElectionPersonas.AddRange(_scrutineer, _returningOfficer);
        await _context.SaveChangesAsync();
    }

    private readonly Dictionary<int, int> _users = new();

    // Tests name users by a small key. The appointment FK needs a real row behind each one.
    // NUnit reuses one fixture instance, so SetUp clears this for each fresh database.
    private async Task<int> UserAsync(int key)
    {
        if (_users.TryGetValue(key, out var id))
            return id;
        var user = new User { Username = $"access-user-{key}", PasswordHash = "x" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return _users[key] = user.Id;
    }

    private async Task<ElectionAppointment> AppointAsync(int key, ElectionPersona persona, bool live = true)
    {
        var now = DateTime.UtcNow;
        var userId = await UserAsync(key);
        var a = new ElectionAppointment
        {
            ElectionId = _election.Id,
            PersonaId = persona.Id,
            UserId = userId,
            AppointedByUserId = userId,
            DisplayName = "Official",
            Email = $"u{key}-{persona.Id}@example.com",
            AppointedAt = now,
            AcceptedAt = live ? now : null,
            DeclarationSignedAt = live ? now : null,
        };
        _context.ElectionAppointments.Add(a);
        await _context.SaveChangesAsync();
        return a;
    }

    [Test]
    public async Task SuperAdmin_HasEveryPermission_EvenAfterTakeover()
    {
        await AppointAsync(50, _returningOfficer);

        var perms = await _service.GetPermissionsAsync(_election.Id, await UserAsync(1), [Constants.Roles.SuperAdmin]);

        perms.Should().HaveFlag(ElectionPermission.DecideAppeals).And.HaveFlag(ElectionPermission.Approve).And.HaveFlag(ElectionPermission.ChangePhase);
    }

    [Test]
    public async Task Admin_BeforeTakeover_HasEverythingButAppeals()
    {
        var perms = await _service.GetPermissionsAsync(_election.Id, await UserAsync(1), Admin);

        perms.Should().HaveFlag(ElectionPermission.ChangePhase).And.HaveFlag(ElectionPermission.AppointOfficials);
        perms.HasFlag(ElectionPermission.DecideAppeals).Should().BeFalse();
        (await _service.IsHandedOverAsync(_election.Id)).Should().BeFalse();
    }

    [Test]
    public async Task Admin_AfterTakeover_KeepsOnlyOwnAppointments()
    {
        await AppointAsync(50, _returningOfficer);
        await AppointAsync(1, _scrutineer);

        var perms = await _service.GetPermissionsAsync(_election.Id, await UserAsync(1), Admin);

        perms.Should().Be(_scrutineer.Permissions);
        (await _service.IsHandedOverAsync(_election.Id)).Should().BeTrue();
    }

    [Test]
    public async Task Admin_AfterTakeover_KeepsControl_WhenConfigSaysSo()
    {
        _orgConfig.Setup(s => s.GetConfigAsync()).ReturnsAsync(new OrgConfigDto { Elections = new ElectionSettingsDto { AdminKeepsControlAfterHandover = true } });
        await AppointAsync(50, _returningOfficer);

        (await _service.HasAsync(_election.Id, await UserAsync(1), Admin, ElectionPermission.ChangePhase)).Should().BeTrue();
        (await _service.IsHandedOverAsync(_election.Id)).Should().BeTrue();
    }

    [Test]
    public async Task LiveApproverCount_CountsLiveOfficialsWithActiveApprove_OncePerUser()
    {
        var approver = new ElectionPersona { Name = "Approver", GroupName = "Officials", Description = "d", DeclarationText = "t", Permissions = ElectionPermission.Approve, IsActive = true };
        var deputy = new ElectionPersona { Name = "Deputy", GroupName = "Officials", Description = "d", DeclarationText = "t", Permissions = ElectionPermission.Approve | ElectionPermission.Count, IsActive = true };
        var offPersona = new ElectionPersona { Name = "Old approver", GroupName = "Officials", Description = "d", DeclarationText = "t", Permissions = ElectionPermission.Approve, IsActive = false };
        _context.ElectionPersonas.AddRange(approver, deputy, offPersona);
        await _context.SaveChangesAsync();
        await AppointAsync(1, approver);
        await AppointAsync(1, deputy); // two personas, one person
        await AppointAsync(2, approver, live: false);
        await AppointAsync(3, offPersona);
        await AppointAsync(4, _scrutineer);

        (await _service.LiveApproverCountAsync(_election.Id)).Should().Be(1);

        await AppointAsync(5, approver);
        (await _service.LiveApproverCountAsync(_election.Id)).Should().Be(2);
    }

    [Test]
    public async Task RevokedExpiredOrUnsignedAppointments_GiveNothing()
    {
        var revoked = await AppointAsync(7, _scrutineer);
        revoked.RevokedAt = DateTime.UtcNow;
        var expired = await AppointAsync(7, _returningOfficer);
        expired.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await _context.SaveChangesAsync();
        await AppointAsync(8, _scrutineer, live: false);

        (await _service.GetPermissionsAsync(_election.Id, await UserAsync(7), Official)).Should().Be(ElectionPermission.None);
        (await _service.GetPermissionsAsync(_election.Id, await UserAsync(8), Official)).Should().Be(ElectionPermission.None);
        (await _service.IsHandedOverAsync(_election.Id)).Should().BeFalse();
        (await _service.ElectionIdsWithLiveAppointmentAsync(await UserAsync(7))).Should().BeEmpty();
    }

    // 94.5
    [Test]
    public async Task ArchivedUser_KeepsTheAppointmentOnRecord_ButGetsNoPermissions()
    {
        var appointment = await AppointAsync(9, _returningOfficer);
        var user = await _context.Users.FindAsync(await UserAsync(9));
        user!.IsArchived = true;
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        (await _context.ElectionAppointments.AnyAsync(x => x.Id == appointment.Id)).Should().BeTrue();
        (await _service.ElectionIdForAsync(ElectionIdLookup.Appointment, appointment.Id)).Should().Be(_election.Id);

        (await _service.GetPermissionsAsync(_election.Id, user.Id, Official)).Should().Be(ElectionPermission.None);
        (await _service.IsHandedOverAsync(_election.Id)).Should().BeFalse();
        (await _service.ElectionIdsWithLiveAppointmentAsync(user.Id)).Should().BeEmpty();
    }

    [Test]
    public async Task TwoPersonas_GiveTheUnion()
    {
        await AppointAsync(7, _scrutineer);
        await AppointAsync(7, _returningOfficer);

        var perms = await _service.GetPermissionsAsync(_election.Id, await UserAsync(7), Official);

        perms.Should().Be(_scrutineer.Permissions | _returningOfficer.Permissions);
        (await _service.HasAsync(_election.Id, await UserAsync(7), Official, ElectionPermission.Count | ElectionPermission.DecideNominations)).Should().BeTrue();
        (await _service.HasAsync(_election.Id, await UserAsync(7), Official, ElectionPermission.ChangePhase)).Should().BeFalse();
        (await _service.ElectionIdsWithLiveAppointmentAsync(await UserAsync(7))).Should().Equal(_election.Id);
    }

    [Test]
    public async Task ElectionIdFor_ResolvesAppointments_AndIsNullForUnknownIds()
    {
        var a = await AppointAsync(7, _scrutineer);

        (await _service.ElectionIdForAsync(ElectionIdLookup.Appointment, a.Id)).Should().Be(_election.Id);
        (await _service.ElectionIdForAsync(ElectionIdLookup.Nomination, 999_999)).Should().BeNull();
        (await _service.ElectionIdForAsync(ElectionIdLookup.Election, 42)).Should().Be(42);

        var approval = new ElectionApproval { ElectionId = _election.Id, Action = ElectionApprovalAction.Publish, RequestedByUserId = await UserAsync(7), RequestedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddHours(1) };
        _context.ElectionApprovals.Add(approval);
        await _context.SaveChangesAsync();
        (await _service.ElectionIdForAsync(ElectionIdLookup.Approval, approval.Id)).Should().Be(_election.Id);
        (await _service.ElectionIdForAsync(ElectionIdLookup.Approval, 999_999)).Should().BeNull();
    }
}
