using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class AdminNotificationServiceTests : TestBase
    {
        private AdminNotificationService _service = null!;
        private Mock<INotificationService> _notificationMock = null!;
        private Mock<IEmailService> _emailMock = null!;

        [SetUp]
        public void Setup()
        {
            _notificationMock = new Mock<INotificationService>();
            _emailMock = new Mock<IEmailService>();
            _service = new AdminNotificationService(_context, _notificationMock.Object, _emailMock.Object);
        }

        private async Task<Member> CreateMemberWithRoleAsync(string fullName, string email, string username, int roleId)
        {
            var member = new Member
            {
                FullName = fullName,
                Email = email,
                NID = username,
                MobileNo = username,
                FatherName = "F",
                MotherName = "M",
                PresentAddress = "A",
                PermanentAddress = "A",
                EmergencyContactName = "E",
                EmergencyContactRelation = "R",
                EmergencyContactPhone = "0"
            };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            var role = await _context.Roles.FirstAsync(r => r.Id == roleId);
            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123"),
                MemberId = member.Id,
                CreatedAt = System.DateTime.UtcNow,
                IsActive = true,
                Roles = new System.Collections.Generic.List<Role> { role }
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return member;
        }

        [Test]
        public async Task NotifyPendingApprovalAsync_FansOutToAdminAndSuperAdmin_ButNotRegularMember()
        {
            var superAdmin = await CreateMemberWithRoleAsync("Super Admin", "sa@e.com", "sa_user", 1);
            var admin = await CreateMemberWithRoleAsync("Admin User", "admin@e.com", "admin_user", 2);
            var regular = await CreateMemberWithRoleAsync("Regular Member", "member@e.com", "member_user", 3);

            await _service.NotifyPendingApprovalAsync("Job", "Test Job", "Jane Submitter", "/admin/jobs/1", CancellationToken.None);

            _notificationMock.Verify(x => x.CreateNotificationAsync(
                superAdmin.Id, It.IsAny<string>(), It.IsAny<string>(), Enums.NotificationType.ApprovalRequest, "/admin/jobs/1", It.IsAny<CancellationToken>()), Times.Once);

            _notificationMock.Verify(x => x.CreateNotificationAsync(
                admin.Id, It.IsAny<string>(), It.IsAny<string>(), Enums.NotificationType.ApprovalRequest, "/admin/jobs/1", It.IsAny<CancellationToken>()), Times.Once);

            _notificationMock.Verify(x => x.CreateNotificationAsync(
                regular.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Enums.NotificationType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

            _emailMock.Verify(x => x.SendEmailAsync("sa@e.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _emailMock.Verify(x => x.SendEmailAsync("admin@e.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _emailMock.Verify(x => x.SendEmailAsync("member@e.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task NotifyPendingApprovalAsync_DoesNotThrow_WhenEmailServiceFails()
        {
            var admin = await CreateMemberWithRoleAsync("Admin User", "admin2@e.com", "admin_user2", 2);
            _emailMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new System.Exception("SMTP down"));

            Func<Task> act = async () => await _service.NotifyPendingApprovalAsync("Album", "Test Album", "Jane", "/admin/gallery/1", CancellationToken.None);

            await act.Should().NotThrowAsync();
            _notificationMock.Verify(x => x.CreateNotificationAsync(
                admin.Id, It.IsAny<string>(), It.IsAny<string>(), Enums.NotificationType.ApprovalRequest, "/admin/gallery/1", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task NotifyPendingApprovalAsync_DoesNotNotifyRegularMembers()
        {
            var regular = await CreateMemberWithRoleAsync("Regular Member", "member2@e.com", "member_user2", 3);

            await _service.NotifyPendingApprovalAsync("Photo", "Test Photo", "Jane", "/admin/gallery/photos/1", CancellationToken.None);

            _notificationMock.Verify(x => x.CreateNotificationAsync(
                regular.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Enums.NotificationType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _emailMock.Verify(x => x.SendEmailAsync("member2@e.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
