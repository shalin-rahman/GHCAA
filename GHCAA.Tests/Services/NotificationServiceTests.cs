using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using GHCAA.Application.Interfaces;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Services;

/// <summary>
/// Tests for NotificationService — member-configurable notification preferences,
/// broadcast filtering, mark-as-read, and preference-gate enforcement.
/// </summary>
[TestFixture]
public class NotificationServiceTests : TestBase
{
    private Mock<IRealTimeService> _mockRealTime = null!;
    private NotificationService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockRealTime = new Mock<IRealTimeService>();
        _mockRealTime
            .Setup(r => r.SendNotificationToUserAsync(It.IsAny<int>(), It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        _service = new NotificationService(_context, _mockRealTime.Object);
    }

    // ── CreateNotificationAsync — preference gate ────────────────────────────

    [Test]
    public async Task CreateNotification_WhenEventOptedIn_ShouldPersistNotification()
    {
        var member = await CreateAndSaveTestMemberAsync();
        member.NotifyEventCreation = true;
        await _context.SaveChangesAsync();

        await _service.CreateNotificationAsync(
            member.Id, "New Event", "A new event has been created",
            Enums.NotificationType.EventCreation);

        var saved = _context.Notifications.Where(n => n.MemberId == member.Id).ToList();
        saved.Should().HaveCount(1);
        saved[0].Title.Should().Be("New Event");
        saved[0].IsRead.Should().BeFalse();
    }

    [Test]
    public async Task CreateNotification_WhenEventOptedOut_ShouldSkipNotification()
    {
        var member = await CreateAndSaveTestMemberAsync();
        member.NotifyEventCreation = false;
        await _context.SaveChangesAsync();

        await _service.CreateNotificationAsync(
            member.Id, "New Event", "A new event",
            Enums.NotificationType.EventCreation);

        _context.Notifications.Where(n => n.MemberId == member.Id).Should().BeEmpty();
        _mockRealTime.Verify(
            r => r.SendNotificationToUserAsync(It.IsAny<int>(), It.IsAny<Notification>()),
            Times.Never);
    }

    [Test]
    public async Task CreateNotification_ParticipationApproval_RespectsOptOut()
    {
        var member = await CreateAndSaveTestMemberAsync();
        member.NotifyParticipationApproval = false;
        await _context.SaveChangesAsync();

        await _service.CreateNotificationAsync(
            member.Id, "Approved", "Event registration approved",
            Enums.NotificationType.ParticipationApproval);

        _context.Notifications.Where(n => n.MemberId == member.Id).Should().BeEmpty();
    }

    [Test]
    public async Task CreateNotification_RegistrationUpdate_RespectsOptOut()
    {
        var member = await CreateAndSaveTestMemberAsync();
        member.NotifyRegistrationUpdate = false;
        await _context.SaveChangesAsync();

        await _service.CreateNotificationAsync(
            member.Id, "Status Update", "Membership application changed",
            Enums.NotificationType.RegistrationUpdate);

        _context.Notifications.Where(n => n.MemberId == member.Id).Should().BeEmpty();
    }

    [Test]
    public async Task CreateNotification_DirectMessage_AlwaysDelivered_RegardlessOfPreferences()
    {
        var member = await CreateAndSaveTestMemberAsync();
        member.NotifyEventCreation = false;
        member.NotifyParticipationApproval = false;
        member.NotifyRegistrationUpdate = false;
        member.NotifyRelevantUpdates = false;
        await _context.SaveChangesAsync();

        await _service.CreateNotificationAsync(
            member.Id, "Direct Message", "Admin has a message for you",
            Enums.NotificationType.DirectMessage);

        // DirectMessage bypasses all preference flags
        _context.Notifications.Where(n => n.MemberId == member.Id).Should().HaveCount(1);
    }

    [Test]
    public async Task CreateNotification_ShouldBroadcastRealTime_AfterPersist()
    {
        var member = await CreateAndSaveTestMemberAsync();
        member.NotifyEventCreation = true;
        await _context.SaveChangesAsync();

        await _service.CreateNotificationAsync(
            member.Id, "Event Alert", "New event added",
            Enums.NotificationType.EventCreation);

        _mockRealTime.Verify(
            r => r.SendNotificationToUserAsync(
                member.Id, It.Is<Notification>(n => n.Title == "Event Alert")),
            Times.Once);
    }

    // ── GetUserNotificationsAsync ─────────────────────────────────────────────

    [Test]
    public async Task GetUserNotifications_ShouldReturnOnlyMembersOwnNotifications()
    {
        var m1 = await CreateAndSaveTestMemberAsync("Member One", "m1@test.com", "01711111111", "1111111111");
        var m2 = await CreateAndSaveTestMemberAsync("Member Two", "m2@test.com", "01722222222", "2222222222");
        m1.NotifyEventCreation = true;
        m2.NotifyEventCreation = true;
        await _context.SaveChangesAsync();

        await _service.CreateNotificationAsync(m1.Id, "M1 Notif", "For member 1", Enums.NotificationType.EventCreation);
        await _service.CreateNotificationAsync(m2.Id, "M2 Notif", "For member 2", Enums.NotificationType.EventCreation);

        var results = (await _service.GetUserNotificationsAsync(m1.Id)).ToList();
        results.Should().HaveCount(1);
        results[0].Title.Should().Be("M1 Notif");
    }

    [Test]
    public async Task GetUserNotifications_ShouldReturnMostRecentFirst()
    {
        var member = await CreateAndSaveTestMemberAsync();
        member.NotifyEventCreation = true;
        member.NotifyRelevantUpdates = true;
        await _context.SaveChangesAsync();

        await _service.CreateNotificationAsync(member.Id, "First", "Oldest", Enums.NotificationType.EventCreation);
        await Task.Delay(20);
        await _service.CreateNotificationAsync(member.Id, "Second", "Newest", Enums.NotificationType.GeneralSystem);

        var results = (await _service.GetUserNotificationsAsync(member.Id)).ToList();
        results[0].Title.Should().Be("Second");
    }

    // ── MarkAsReadAsync ───────────────────────────────────────────────────────

    [Test]
    public async Task MarkAsReadAsync_ShouldMarkSingleNotificationAsRead()
    {
        var member = await CreateAndSaveTestMemberAsync();
        member.NotifyEventCreation = true;
        await _context.SaveChangesAsync();

        await _service.CreateNotificationAsync(member.Id, "Unread", "Msg", Enums.NotificationType.EventCreation);
        var notification = _context.Notifications.First(n => n.MemberId == member.Id);
        notification.IsRead.Should().BeFalse();

        var ok = await _service.MarkAsReadAsync(notification.Id, member.Id);
        ok.Should().BeTrue();

        var updated = await _context.Notifications.FindAsync(notification.Id);
        updated!.IsRead.Should().BeTrue();
    }

    [Test]
    public async Task MarkAsReadAsync_WhenWrongMember_DoesNotUpdateRow()
    {
        var owner = await CreateAndSaveTestMemberAsync("Owner", "o@test.com", "01711111111", "1111111111");
        var other = await CreateAndSaveTestMemberAsync("Other", "x@test.com", "01722222222", "2222222222");
        owner.NotifyEventCreation = true;
        await _context.SaveChangesAsync();

        await _service.CreateNotificationAsync(owner.Id, "Sec", "Msg", Enums.NotificationType.EventCreation);
        var notification = _context.Notifications.First(n => n.MemberId == owner.Id);

        var ok = await _service.MarkAsReadAsync(notification.Id, other.Id);
        ok.Should().BeFalse();
        (await _context.Notifications.FindAsync(notification.Id))!.IsRead.Should().BeFalse();
    }

    // ── MarkAllAsReadAsync ────────────────────────────────────────────────────

    [Test]
    public async Task MarkAllAsReadAsync_ShouldMarkAllUnreadForMember()
    {
        var member = await CreateAndSaveTestMemberAsync();
        member.NotifyEventCreation = true;
        member.NotifyRelevantUpdates = true;
        await _context.SaveChangesAsync();

        await _service.CreateNotificationAsync(member.Id, "N1", "M1", Enums.NotificationType.EventCreation);
        await _service.CreateNotificationAsync(member.Id, "N2", "M2", Enums.NotificationType.GeneralSystem);

        _context.Notifications.Where(n => n.MemberId == member.Id && !n.IsRead).Should().HaveCount(2);

        await _service.MarkAllAsReadAsync(member.Id);

        _context.Notifications.Where(n => n.MemberId == member.Id && !n.IsRead).Should().BeEmpty();
    }

    // ── BroadcastNotificationAsync — preference filtering ─────────────────────

    [Test]
    public async Task BroadcastNotification_ShouldOnlyReachOptedInMembers()
    {
        var optedIn = await CreateAndSaveTestMemberAsync("Opted In", "opted@test.com", "01711111111", "1111111111");
        var optedOut = await CreateAndSaveTestMemberAsync("Opted Out", "out@test.com", "01722222222", "2222222222");

        optedIn.NotifyEventCreation = true;
        optedIn.IsArchived = false;
        optedOut.NotifyEventCreation = false;
        optedOut.IsArchived = false;
        await _context.SaveChangesAsync();

        await _service.BroadcastNotificationAsync(
            "Community Event", "New event launched",
            Enums.NotificationType.EventCreation);

        _context.Notifications.Where(n => n.MemberId == optedIn.Id).Should().HaveCount(1);
        _context.Notifications.Where(n => n.MemberId == optedOut.Id).Should().BeEmpty();
    }

    [Test]
    public async Task BroadcastNotification_ShouldExcludeArchivedMembers()
    {
        var active = await CreateAndSaveTestMemberAsync("Active", "active@test.com", "01711111111", "1111111111");
        var archived = await CreateAndSaveTestMemberAsync("Archived", "archived@test.com", "01722222222", "2222222222");

        active.NotifyEventCreation = true;
        active.IsArchived = false;
        archived.NotifyEventCreation = true;
        archived.IsArchived = true;
        await _context.SaveChangesAsync();

        await _service.BroadcastNotificationAsync(
            "Platform Update", "New features available",
            Enums.NotificationType.EventCreation);

        _context.Notifications.Where(n => n.MemberId == active.Id).Should().HaveCount(1);
        _context.Notifications.Where(n => n.MemberId == archived.Id).Should().BeEmpty();
    }
}
