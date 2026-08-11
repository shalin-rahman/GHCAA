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

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<MentorshipService>>();
            _service = new MentorshipService(_context, _mockLogger.Object);
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
    }
}
