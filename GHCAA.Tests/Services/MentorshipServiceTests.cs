using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class MentorshipServiceTests : TestBase
    {
        private MentorshipService _service = null!;
        private Mock<ILogger<MentorshipService>> _mockLogger = null!;
        private Mock<INotificationService> _mockNotifications = null!;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<MentorshipService>>();
            _mockNotifications = new Mock<INotificationService>();
            _service = new MentorshipService(_context, _mockLogger.Object, _mockNotifications.Object);
        }

        [Test]
        public async Task SendRequestAsync_ShouldCreatePendingRequest()
        {
            // Arrange
            var requester = await CreateAndSaveTestMemberAsync("Requester", "req@ex.com", "1", "1");
            var mentor = await CreateAndSaveTestMemberAsync("Mentor", "men@ex.com", "2", "2");

            // Act
            var request = await _service.SendRequestAsync(requester.Id, mentor.Id, "Help me", "IT");

            // Assert
            request.Should().NotBeNull();
            request.RequesterId.Should().Be(requester.Id);
            request.MentorId.Should().Be(mentor.Id);
            request.Status.Should().Be(MentorshipStatus.Pending);
            request.Message.Should().Be("Help me");
            _mockNotifications.Verify(x => x.CreateNotificationAsync(
                mentor.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GHCAA.Domain.Enums.NotificationType>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendRequestAsync_DuplicatePending_ShouldThrow()
        {
            // Arrange
            var requester = await CreateAndSaveTestMemberAsync("Requester", "req2@ex.com", "3", "3");
            var mentor = await CreateAndSaveTestMemberAsync("Mentor", "men2@ex.com", "4", "4");
            await _service.SendRequestAsync(requester.Id, mentor.Id, "First", "IT");

            // Act
            var act = async () => await _service.SendRequestAsync(requester.Id, mentor.Id, "Second", "IT");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("A pending mentorship request already exists for this mentor.");
        }

        [Test]
        public async Task RespondAsync_Accept_ShouldUpdateStatus()
        {
            // Arrange
            var requester = await CreateAndSaveTestMemberAsync("Requester", "req3@ex.com", "5", "5");
            var mentor = await CreateAndSaveTestMemberAsync("Mentor", "men3@ex.com", "6", "6");
            var request = await _service.SendRequestAsync(requester.Id, mentor.Id, "Msg", "IT");

            // Act
            var success = await _service.RespondAsync(request.Id, mentor.Id, true, "WIP Note");

            // Assert
            success.Should().BeTrue();
            var updated = await _context.MentorshipRequests.FindAsync(request.Id);
            updated!.Status.Should().Be(MentorshipStatus.Accepted);
            updated.ResponseNote.Should().Be("WIP Note");
            updated.RespondedAt.Should().NotBeNull();
            _mockNotifications.Verify(x => x.CreateNotificationAsync(
                requester.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GHCAA.Domain.Enums.NotificationType>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task RespondAsync_WrongMentor_ShouldReturnFalse()
        {
            // Arrange
            var requester = await CreateAndSaveTestMemberAsync("Requester", "req4@ex.com", "7", "7");
            var mentor = await CreateAndSaveTestMemberAsync("Mentor", "men4@ex.com", "8", "8");
            var request = await _service.SendRequestAsync(requester.Id, mentor.Id, "Msg", "IT");

            // Act
            var success = await _service.RespondAsync(request.Id, 999, true, "No");

            // Assert
            success.Should().BeFalse();
        }

        [Test]
        public async Task MarkCompleteAsync_ShouldUpdateToCompleted()
        {
            // Arrange
            var requester = await CreateAndSaveTestMemberAsync("Requester", "req5@ex.com", "9", "9");
            var mentor = await CreateAndSaveTestMemberAsync("Mentor", "men5@ex.com", "10", "10");
            var request = await _service.SendRequestAsync(requester.Id, mentor.Id, "Msg", "IT");
            await _service.RespondAsync(request.Id, mentor.Id, true, "OK");

            // Act
            var success = await _service.MarkCompleteAsync(request.Id, requester.Id);

            // Assert
            success.Should().BeTrue();
            var final = await _context.MentorshipRequests.FindAsync(request.Id);
            final!.Status.Should().Be(MentorshipStatus.Completed);
        }

        [Test]
        public async Task AdminCloseAsync_Pending_ShouldDeclineAndNotifyBothMembers()
        {
            var requester = await CreateAndSaveTestMemberAsync("Requester", "req6@ex.com", "11", "11");
            var mentor = await CreateAndSaveTestMemberAsync("Mentor", "men6@ex.com", "12", "12");
            var request = await _service.SendRequestAsync(requester.Id, mentor.Id, "Msg", "IT");
            _mockNotifications.Invocations.Clear();

            var success = await _service.AdminCloseAsync(request.Id, "No reply in 60 days");

            success.Should().BeTrue();
            var final = await _context.MentorshipRequests.FindAsync(request.Id);
            final!.Status.Should().Be(MentorshipStatus.Declined);
            final.ResponseNote.Should().Be("Closed by admin: No reply in 60 days");
            final.RespondedAt.Should().NotBeNull();
            _mockNotifications.Verify(n => n.CreateNotificationAsync(requester.Id, It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<GHCAA.Domain.Enums.NotificationType>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockNotifications.Verify(n => n.CreateNotificationAsync(mentor.Id, It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<GHCAA.Domain.Enums.NotificationType>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task AdminCloseAsync_Accepted_ShouldCompleteAndKeepMentorNote()
        {
            var requester = await CreateAndSaveTestMemberAsync("Requester", "req7@ex.com", "13", "13");
            var mentor = await CreateAndSaveTestMemberAsync("Mentor", "men7@ex.com", "14", "14");
            var request = await _service.SendRequestAsync(requester.Id, mentor.Id, "Msg", "IT");
            await _service.RespondAsync(request.Id, mentor.Id, true, "Happy to help");

            var success = await _service.AdminCloseAsync(request.Id, "Programme ended");

            success.Should().BeTrue();
            var final = await _context.MentorshipRequests.FindAsync(request.Id);
            final!.Status.Should().Be(MentorshipStatus.Completed);
            final.ResponseNote.Should().Be("Happy to help\nClosed by admin: Programme ended");
        }

        [Test]
        public async Task AdminCloseAsync_WithoutNote_ShouldLeaveNoteUntouched()
        {
            var requester = await CreateAndSaveTestMemberAsync("Requester", "req8@ex.com", "15", "15");
            var mentor = await CreateAndSaveTestMemberAsync("Mentor", "men8@ex.com", "16", "16");
            var request = await _service.SendRequestAsync(requester.Id, mentor.Id, "Msg", "IT");

            var success = await _service.AdminCloseAsync(request.Id, "   ");

            success.Should().BeTrue();
            var final = await _context.MentorshipRequests.FindAsync(request.Id);
            final!.ResponseNote.Should().BeNull();
        }

        [Test]
        public async Task AdminCloseAsync_LongNote_ShouldTruncateTo500()
        {
            var requester = await CreateAndSaveTestMemberAsync("Requester", "req9@ex.com", "17", "17");
            var mentor = await CreateAndSaveTestMemberAsync("Mentor", "men9@ex.com", "18", "18");
            var request = await _service.SendRequestAsync(requester.Id, mentor.Id, "Msg", "IT");
            await _service.RespondAsync(request.Id, mentor.Id, true, new string('m', 300));

            await _service.AdminCloseAsync(request.Id, new string('a', 400));

            var final = await _context.MentorshipRequests.FindAsync(request.Id);
            final!.ResponseNote!.Length.Should().Be(500);
        }

        [Test]
        public async Task AdminCloseAsync_AlreadyClosedOrMissing_ShouldReturnFalse()
        {
            var requester = await CreateAndSaveTestMemberAsync("Requester", "req10@ex.com", "19", "19");
            var mentor = await CreateAndSaveTestMemberAsync("Mentor", "men10@ex.com", "20", "20");
            var request = await _service.SendRequestAsync(requester.Id, mentor.Id, "Msg", "IT");
            await _service.RespondAsync(request.Id, mentor.Id, false, "Busy");

            (await _service.AdminCloseAsync(request.Id, null)).Should().BeFalse();
            (await _service.AdminCloseAsync(999999, null)).Should().BeFalse();
            var final = await _context.MentorshipRequests.FindAsync(request.Id);
            final!.Status.Should().Be(MentorshipStatus.Declined);
            final.ResponseNote.Should().Be("Busy");
        }
    }
}
