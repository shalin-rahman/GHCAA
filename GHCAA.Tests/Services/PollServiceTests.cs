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
using Microsoft.Extensions.Logging;
using Moq;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class PollServiceTests : TestBase
    {
        private const int AdminMemberId = 1;
        private const int NonexistentPollId = 999;
        private const string PollTitle = "Test Poll";
        private const string PollDescription = "Test description";
        private const string FirstOption = "Option A";
        private const string SecondOption = "Option B";
        private const string PollMemberNamePrefix = "Poll member";
        private const string PollMemberEmailPrefix = "poll.member";

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

        private static Poll CreatePoll(bool allowMultipleChoice = false, DateTime? expiryDate = null, bool isActive = true)
        {
            return new Poll
            {
                Title = PollTitle,
                AllowMultipleChoice = allowMultipleChoice,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow,
                ExpiryDate = expiryDate,
                Options = new List<PollOption>
                {
                    new() { OptionText = FirstOption },
                    new() { OptionText = SecondOption }
                }
            };
        }

        // Offset well past the Visual/members.json seed range (01700000001-01700000004 etc.)
        // so these fixtures never collide with the seeded MobileNo UNIQUE constraint.
        private const int PollMemberMobileOffset = 500;

        private Task<Member> CreatePollMemberAsync(int sequence) =>
            CreateAndSaveTestMemberAsync(
                $"{PollMemberNamePrefix} {sequence}",
                $"{PollMemberEmailPrefix}{sequence}@example.com",
                $"017{PollMemberMobileOffset + sequence:D8}",
                $"1{PollMemberMobileOffset + sequence:D9}");

        [Category("FR-40")]
        [Test]
        public async Task CreatePollAsync_ShouldSavePollWithOptions()
        {
            // Arrange
            var dto = new CreatePollDto
            {
                Title = PollTitle,
                Description = PollDescription,
                AllowMultipleChoice = false,
                Options = new List<string> { FirstOption, SecondOption }
            };

            // Act
            var pollId = await _pollService.CreatePollAsync(dto, AdminMemberId);

            // Assert
            var poll = await _db.Polls.Include(p => p.Options).FirstOrDefaultAsync(p => p.Id == pollId);
            poll.Should().NotBeNull();
            poll!.Title.Should().Be(PollTitle);
            poll.Description.Should().Be(PollDescription);
            poll.CreatedBy.Should().Be(AdminMemberId);
            poll.Options.Count.Should().Be(2);
        }

        [Category("FR-40")]
        [Test]
        public async Task VoteAsync_ShouldRecordVoteAndPreventDuplicate()
        {
            // Arrange
            var member = await CreatePollMemberAsync(1);
            var poll = CreatePoll();
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

        [Category("FR-40")]
        [Test]
        public async Task GetActivePollsAsync_ShouldReturnPollsWithCorrectPercentages()
        {
            // Arrange
            var m1 = await CreatePollMemberAsync(1);
            var m2 = await CreatePollMemberAsync(2);
            var m3 = await CreatePollMemberAsync(3);
            var m4 = await CreatePollMemberAsync(4);

            var poll = CreatePoll();
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

        [Test]
        public async Task VoteAsync_RejectsNullEmptyMultipleAndForeignChoices()
        {
            var member = await CreatePollMemberAsync(1);
            var poll = CreatePoll();
            _db.Polls.Add(poll);
            await _db.SaveChangesAsync();
            var optionIds = poll.Options.Select(option => option.Id).ToList();

            var missing = await _pollService.VoteAsync(poll.Id, member.Id, null!);
            var empty = await _pollService.VoteAsync(poll.Id, member.Id, new List<int>());
            var multiple = await _pollService.VoteAsync(poll.Id, member.Id, optionIds);
            var foreign = await _pollService.VoteAsync(poll.Id, member.Id, new List<int> { NonexistentPollId });

            missing.Should().BeFalse();
            empty.Should().BeFalse();
            multiple.Should().BeFalse();
            foreign.Should().BeFalse();
            (await _db.PollVotes.CountAsync()).Should().Be(0);
        }

        [Test]
        public async Task GetActivePollsAsync_ExcludesInactiveExpiredAndArchivedPolls()
        {
            var active = CreatePoll();
            var inactive = CreatePoll(isActive: false);
            var expired = CreatePoll(expiryDate: DateTime.UtcNow.AddMinutes(-1));
            var archived = CreatePoll();
            archived.IsArchived = true;
            _db.Polls.AddRange(active, inactive, expired, archived);
            await _db.SaveChangesAsync();

            var polls = await _pollService.GetActivePollsAsync(AdminMemberId);

            polls.Select(poll => poll.Id).Should().Equal(active.Id);
        }

        [Test]
        public async Task GetAllPollsAsync_ReturnsInactiveAndExpiredButNotArchivedPolls()
        {
            var active = CreatePoll();
            var inactive = CreatePoll(isActive: false);
            var expired = CreatePoll(expiryDate: DateTime.UtcNow.AddMinutes(-1));
            var archived = CreatePoll();
            archived.IsArchived = true;
            _db.Polls.AddRange(active, inactive, expired, archived);
            await _db.SaveChangesAsync();

            var polls = await _pollService.GetAllPollsAsync();

            polls.Select(poll => poll.Id).Should().BeEquivalentTo(new[] { active.Id, inactive.Id, expired.Id });
            polls.Should().OnlyContain(poll => !poll.HasVoted && !poll.SelectedOptionIds.Any());
        }

        [Test]
        public async Task GetPollByIdAsync_ReturnsPollAndSelectedOptionsForVotingMember()
        {
            var member = await CreatePollMemberAsync(1);
            var poll = CreatePoll();
            _db.Polls.Add(poll);
            await _db.SaveChangesAsync();
            var selectedOptionId = poll.Options.First().Id;
            _db.PollVotes.Add(new PollVote
            {
                PollId = poll.Id,
                PollOptionId = selectedOptionId,
                MemberId = member.Id,
                VotedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            var result = await _pollService.GetPollByIdAsync(poll.Id, member.Id);

            result.Should().NotBeNull();
            result!.HasVoted.Should().BeTrue();
            result.SelectedOptionIds.Should().Equal(selectedOptionId);
            result.TotalVotes.Should().Be(1);
            result.Options.Single(option => option.Id == selectedOptionId).Percentage.Should().Be(100);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task GetPollByIdAsync_ReturnsNullForMissingOrArchivedPoll(bool archived)
        {
            var poll = CreatePoll();
            if (archived)
            {
                poll.IsArchived = true;
                _db.Polls.Add(poll);
                await _db.SaveChangesAsync();
            }

            var result = await _pollService.GetPollByIdAsync(archived ? poll.Id : NonexistentPollId, AdminMemberId);

            result.Should().BeNull();
        }

        [Test]
        public async Task GetActivePollsAsync_ReturnsZeroPercentagesWithoutVotes()
        {
            var poll = CreatePoll();
            _db.Polls.Add(poll);
            await _db.SaveChangesAsync();

            var result = (await _pollService.GetActivePollsAsync(AdminMemberId)).Single();

            result.TotalVotes.Should().Be(0);
            result.Options.Should().OnlyContain(option => option.VoteCount == 0 && option.Percentage == 0);
        }

        [Test]
        public async Task VoteAsync_AllowsMultipleOwnedChoicesAndCountsOneParticipant()
        {
            var member = await CreatePollMemberAsync(1);
            var poll = CreatePoll(allowMultipleChoice: true);
            _db.Polls.Add(poll);
            await _db.SaveChangesAsync();
            var optionIds = poll.Options.Select(option => option.Id).ToList();

            var accepted = await _pollService.VoteAsync(poll.Id, member.Id, optionIds);
            var result = (await _pollService.GetActivePollsAsync(member.Id)).Single();

            accepted.Should().BeTrue();
            (await _db.PollVotes.CountAsync()).Should().Be(2);
            result.TotalVotes.Should().Be(1);
            result.SelectedOptionIds.Should().BeEquivalentTo(optionIds);
            result.Options.Should().OnlyContain(option => option.Percentage == 100);
        }

        [TestCase(false, true, false, false)]
        [TestCase(true, false, false, false)]
        [TestCase(true, true, true, false)]
        [TestCase(true, true, false, true)]
        public async Task VoteAsync_RejectsMissingInactiveArchivedAndExpiredPolls(
            bool pollExists,
            bool isActive,
            bool isArchived,
            bool isExpired)
        {
            var member = await CreatePollMemberAsync(1);
            var poll = CreatePoll(isActive: isActive, expiryDate: isExpired ? DateTime.UtcNow.AddMinutes(-1) : null);
            poll.IsArchived = isArchived;
            if (pollExists)
            {
                _db.Polls.Add(poll);
                await _db.SaveChangesAsync();
            }

            var accepted = await _pollService.VoteAsync(
                pollExists ? poll.Id : NonexistentPollId,
                member.Id,
                new List<int> { pollExists ? poll.Options.First().Id : NonexistentPollId });

            accepted.Should().BeFalse();
            (await _db.PollVotes.CountAsync()).Should().Be(0);
        }

        [Test]
        public async Task DeletePollAsync_HidesDeletedPollFromReadMethods()
        {
            var poll = CreatePoll();
            _db.Polls.Add(poll);
            await _db.SaveChangesAsync();

            var deleted = await _pollService.DeletePollAsync(poll.Id);
            var allPolls = await _pollService.GetAllPollsAsync();
            var pollById = await _pollService.GetPollByIdAsync(poll.Id, AdminMemberId);

            deleted.Should().BeTrue();
            allPolls.Should().BeEmpty();
            pollById.Should().BeNull();
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ToggleAndDeletePollAsync_MapExistingAndMissingRecords(bool recordExists)
        {
            var poll = CreatePoll();
            if (recordExists)
            {
                _db.Polls.Add(poll);
                await _db.SaveChangesAsync();
            }

            var pollId = recordExists ? poll.Id : NonexistentPollId;
            var toggled = await _pollService.TogglePollStatusAsync(pollId, false);
            var deleted = await _pollService.DeletePollAsync(pollId);

            toggled.Should().Be(recordExists);
            deleted.Should().Be(recordExists);
            if (recordExists)
            {
                var storedPoll = await _db.Polls.IgnoreQueryFilters().SingleAsync(item => item.Id == poll.Id);
                storedPoll.IsActive.Should().BeFalse();
                storedPoll.IsArchived.Should().BeTrue();
            }
        }
    }
}
