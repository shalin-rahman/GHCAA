using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using FluentAssertions;

using GHCAA.Application.DTOs;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class PollServiceTests : TestBase
    {
        private ApplicationDbContext _db = null!;
        private PollService _pollService = null!;
        private Mock<ILogger<PollService>> _mockLogger = null!;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<PollService>>();
            _pollService = new PollService(_context, _mockLogger.Object);
            _db = _context;
        }

        [Test]
        public async Task CreatePollAsync_ShouldSavePollWithOptions()
        {
            // Arrange
            var dto = new CreatePollDto
            {
                Title = "Test Poll",
                Description = "Description",
                AllowMultipleChoice = false,
                Options = new List<string> { "Option A", "Option B" }
            };

            // Act
            var pollId = await _pollService.CreatePollAsync(dto, 1); // adminMemberId = 1

            // Assert
            var poll = await _db.Polls.Include(p => p.Options).FirstOrDefaultAsync(p => p.Id == pollId);
            poll.Should().NotBeNull();
            poll!.Title.Should().Be("Test Poll");
            poll.Options.Count.Should().Be(2);
        }

        [Test]
        public async Task VoteAsync_ShouldRecordVoteAndPreventDuplicate()
        {
            // Arrange
            var member = await CreateAndSaveTestMemberAsync("VoteMember", "vote@e.com", "9", "9");
            var poll = new Poll
            {
                Title = "Vote Poll",
                AllowMultipleChoice = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Options = new List<PollOption>
                {
                    new PollOption { OptionText = "A" },
                    new PollOption { OptionText = "B" }
                }
            };
            _context.Polls.Add(poll);
            await _context.SaveChangesAsync();

            int optionId = poll.Options.First().Id;

            // Act - First Vote (Order: pollId, memberId, optionIds)
            var result = await _pollService.VoteAsync(poll.Id, member.Id, new List<int> { optionId });

            // Assert
            result.Should().BeTrue();
            var votes = await _context.PollVotes.Where(v => v.PollId == poll.Id && v.MemberId == member.Id).ToListAsync();
            votes.Count.Should().Be(1);

            // Act - Second Vote (Should Fail)
            var secondResult = await _pollService.VoteAsync(poll.Id, member.Id, new List<int> { optionId });
            secondResult.Should().BeFalse();
        }

        [Test]
        public async Task GetActivePollsAsync_ShouldReturnPollsWithCorrectPercentages()
        {
            // Arrange
            // Create test members to avoid FK constraint failures
            var m1 = await CreateAndSaveTestMemberAsync("M1", "m1@e.com", "1", "1");
            var m2 = await CreateAndSaveTestMemberAsync("M2", "m2@e.com", "2", "2");
            var m3 = await CreateAndSaveTestMemberAsync("M3", "m3@e.com", "3", "3");
            var m4 = await CreateAndSaveTestMemberAsync("M4", "m4@e.com", "4", "4");

            var poll = new Poll
            {
                Title = "Stats Poll",
                AllowMultipleChoice = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Options = new List<PollOption>
                {
                    new PollOption { OptionText = "A" },
                    new PollOption { OptionText = "B" }
                }
            };
            _context.Polls.Add(poll);
            await _context.SaveChangesAsync();

            var optionAId = poll.Options.First().Id;
            var optionBId = poll.Options.Skip(1).First().Id;

            // Add 3 votes for A, 1 vote for B
            _context.PollVotes.AddRange(
                new PollVote { PollId = poll.Id, PollOptionId = optionAId, MemberId = m1.Id, VotedAt = DateTime.UtcNow },
                new PollVote { PollId = poll.Id, PollOptionId = optionAId, MemberId = m2.Id, VotedAt = DateTime.UtcNow },
                new PollVote { PollId = poll.Id, PollOptionId = optionAId, MemberId = m3.Id, VotedAt = DateTime.UtcNow },
                new PollVote { PollId = poll.Id, PollOptionId = optionBId, MemberId = m4.Id, VotedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var polls = await _pollService.GetActivePollsAsync(m1.Id); // Call as Member 1

            // Assert
            polls.Count.Should().Be(1);
            var resultPoll = polls.First();
            resultPoll.HasVoted.Should().BeTrue();
            resultPoll.TotalVotes.Should().Be(4);
            
            var resOptA = resultPoll.Options.First(o => o.Id == optionAId);
            var resOptB = resultPoll.Options.First(o => o.Id == optionBId);
            
            resOptA.VoteCount.Should().Be(3);
            resOptA.Percentage.Should().Be(75.0);
            
            resOptB.VoteCount.Should().Be(1);
            resOptB.Percentage.Should().Be(25.0);
        }
    }
}
