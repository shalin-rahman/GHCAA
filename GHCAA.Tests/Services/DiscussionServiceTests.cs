using FluentAssertions;
using GHCAA.Application.DTOs.Discussion;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class DiscussionServiceTests : TestBase
    {
        private const string Poll = Constants.Discussion.ContextPoll;
        private DiscussionService _service = null!;

        [SetUp]
        public void Setup() => _service = new DiscussionService(_context, NewActivity());

        private async Task<int> CreatePollAsync(int ownerId)
        {
            var poll = new Poll { Title = "Venue vote", IsActive = true, AllowComments = true, CreatedBy = ownerId, CreatedAt = DateTime.UtcNow };
            _context.Polls.Add(poll);
            await _context.SaveChangesAsync();
            return poll.Id;
        }

        private Task<Member> MemberAsync(int n) =>
            CreateAndSaveTestMemberAsync($"Discussion member {n}", $"discussion{n}@example.com", $"017{600 + n:D8}", $"1{600 + n:D9}");

        private Task<DiscussionResult<CommentDto>> PostAsync(int pollId, int memberId, string text, int? parentId = null) =>
            _service.PostCommentAsync(Poll, pollId, memberId, false, new CreateCommentDto { Content = text, ParentId = parentId });

        [Category("FR-40")]
        [Test]
        public async Task PostComment_KeepsPlainTextAsTyped()
        {
            var member = await MemberAsync(1);
            var pollId = await CreatePollAsync(member.Id);

            var result = await PostAsync(pollId, member.Id, "a < b & c <script>x()</script>");

            result.Status.Should().Be(DiscussionStatus.Ok);
            result.Value!.Content.Should().Be("a < b & c");
        }

        [Category("FR-40")]
        [Test]
        public async Task PostComment_CallerWithoutMemberRow_IsRejectedEvenWhenThreadExists()
        {
            var owner = await MemberAsync(2);
            var pollId = await CreatePollAsync(owner.Id);
            (await PostAsync(pollId, owner.Id, "first")).Status.Should().Be(DiscussionStatus.Ok);

            var result = await PostAsync(pollId, 0, "from a system admin");

            result.Status.Should().Be(DiscussionStatus.Invalid);
            (await _context.ForumPosts.IgnoreQueryFilters().CountAsync()).Should().Be(1);
        }

        [Category("FR-40")]
        [Test]
        public async Task PostComment_ReplyToRemovedComment_IsRejected()
        {
            var member = await MemberAsync(3);
            var pollId = await CreatePollAsync(member.Id);
            var parent = (await PostAsync(pollId, member.Id, "parent")).Value!;
            (await _service.RemoveCommentAsync(Poll, pollId, parent.Id, member.Id, false, null)).Status.Should().Be(DiscussionStatus.Ok);

            var result = await PostAsync(pollId, member.Id, "reply", parent.Id);

            result.Status.Should().Be(DiscussionStatus.Invalid);
        }

        [Category("FR-40")]
        [Test]
        public async Task GetThread_LockedTopic_DisallowsComments()
        {
            var member = await MemberAsync(4);
            var pollId = await CreatePollAsync(member.Id);
            (await _service.SetLockedAsync(Poll, pollId, true, member.Id)).Status.Should().Be(DiscussionStatus.Ok);

            var thread = (await _service.GetThreadAsync(Poll, pollId, member.Id, false)).Value!;

            thread.IsLocked.Should().BeTrue();
            thread.AllowComments.Should().BeFalse();
        }

        [Category("FR-40")]
        [Test]
        public async Task PostComment_OverLimitAfterCleaning_IsRejected()
        {
            var member = await MemberAsync(5);
            var pollId = await CreatePollAsync(member.Id);

            var result = await PostAsync(pollId, member.Id, new string('&', Constants.Discussion.MaxCommentLength + 1));

            result.Status.Should().Be(DiscussionStatus.Invalid);
        }

        [Category("FR-40")]
        [Test]
        public async Task PostComment_BlankText_IsRejected()
        {
            var member = await MemberAsync(6);
            var pollId = await CreatePollAsync(member.Id);

            var result = await PostAsync(pollId, member.Id, "   ");

            result.Status.Should().Be(DiscussionStatus.Invalid);
            result.Error.Should().Be("Write a comment first.");
        }

        [Category("FR-40")]
        [Test]
        public async Task PostComment_ReplyToReply_HangsOffTopLevelComment()
        {
            var member = await MemberAsync(7);
            var pollId = await CreatePollAsync(member.Id);
            var top = (await PostAsync(pollId, member.Id, "top")).Value!;
            var reply = (await PostAsync(pollId, member.Id, "reply", top.Id)).Value!;

            var nested = await PostAsync(pollId, member.Id, "reply to reply", reply.Id);

            nested.Value!.ParentId.Should().Be(top.Id);
            var thread = (await _service.GetThreadAsync(Poll, pollId, member.Id, false)).Value!;
            thread.TotalCount.Should().Be(1);
            thread.Comments[0].Replies.Select(r => r.Content).Should().Equal("reply", "reply to reply");
        }

        [Category("FR-40")]
        [Test]
        public async Task PostComment_ParentFromAnotherPoll_IsRejected()
        {
            var member = await MemberAsync(8);
            var pollA = await CreatePollAsync(member.Id);
            var pollB = await CreatePollAsync(member.Id);
            var other = (await PostAsync(pollA, member.Id, "on poll A")).Value!;

            var result = await PostAsync(pollB, member.Id, "reply", other.Id);

            result.Status.Should().Be(DiscussionStatus.Invalid);
            result.Error.Should().Be("That comment is not part of this thread.");
        }

        [Category("FR-40")]
        [Test]
        public async Task PostComment_CommentsTurnedOff_IsRejectedWithReason()
        {
            var member = await MemberAsync(9);
            var pollId = await CreatePollAsync(member.Id);
            var poll = await _context.Polls.FindAsync(pollId);
            poll!.AllowComments = false;
            await _context.SaveChangesAsync();

            var result = await PostAsync(pollId, member.Id, "hello");

            result.Status.Should().Be(DiscussionStatus.Invalid);
            result.Error.Should().Be("Comments are turned off for this poll.");
        }

        [Category("FR-40")]
        [Test]
        public async Task ArchivedPoll_IsHiddenFromMembers_AndClosedForAdmins()
        {
            var member = await MemberAsync(10);
            var pollId = await CreatePollAsync(member.Id);
            var poll = await _context.Polls.FindAsync(pollId);
            poll!.IsArchived = true;
            await _context.SaveChangesAsync();

            (await _service.GetThreadAsync(Poll, pollId, member.Id, false)).Status.Should().Be(DiscussionStatus.NotFound);
            var adminPost = await _service.PostCommentAsync(Poll, pollId, member.Id, true, new CreateCommentDto { Content = "hi" });
            adminPost.Status.Should().Be(DiscussionStatus.Invalid);
            adminPost.Error.Should().Be("This poll is archived, so comments are closed.");
        }

        [Category("FR-40")]
        [Test]
        public async Task RemoveComment_ByAnotherMember_IsDenied()
        {
            var author = await MemberAsync(11);
            var other = await MemberAsync(12);
            var pollId = await CreatePollAsync(author.Id);
            var comment = (await PostAsync(pollId, author.Id, "mine")).Value!;

            var result = await _service.RemoveCommentAsync(Poll, pollId, comment.Id, other.Id, false, null);

            result.Status.Should().Be(DiscussionStatus.Forbidden);
            (await _context.ForumPosts.IgnoreQueryFilters().SingleAsync(p => p.Id == comment.Id)).IsActive.Should().BeTrue();
        }

        [Category("FR-40")]
        [Test]
        public async Task RemoveComment_ByAdmin_WritesAuditRow()
        {
            var author = await MemberAsync(13);
            var admin = await MemberAsync(14);
            var pollId = await CreatePollAsync(author.Id);
            var comment = (await PostAsync(pollId, author.Id, "rude")).Value!;

            var result = await _service.RemoveCommentAsync(Poll, pollId, comment.Id, admin.Id, true, "Off topic");

            result.Status.Should().Be(DiscussionStatus.Ok);
            var log = await _context.ActivityLogs.SingleAsync(a => a.ActivityType == Constants.Discussion.AuditTypes.CommentRemoved);
            log.MemberId.Should().Be(author.Id);
            log.ActorId.Should().Be(admin.Id);
            log.Description.Should().Contain("Off topic");
        }

        [Category("FR-40")]
        [Test]
        public async Task RemoveComment_ByAuthor_WritesNoAuditRow()
        {
            var author = await MemberAsync(15);
            var pollId = await CreatePollAsync(author.Id);
            var comment = (await PostAsync(pollId, author.Id, "oops")).Value!;

            (await _service.RemoveCommentAsync(Poll, pollId, comment.Id, author.Id, false, null)).Status.Should().Be(DiscussionStatus.Ok);

            (await _context.ActivityLogs.AnyAsync(a => a.ActivityType == Constants.Discussion.AuditTypes.CommentRemoved)).Should().BeFalse();
        }

        [Category("FR-40")]
        [Test]
        public async Task GetThread_RemovedComment_StaysAsPlaceholderOnlyWhenItHasReplies()
        {
            var member = await MemberAsync(16);
            var pollId = await CreatePollAsync(member.Id);
            var withReply = (await PostAsync(pollId, member.Id, "has a reply")).Value!;
            await PostAsync(pollId, member.Id, "the reply", withReply.Id);
            var alone = (await PostAsync(pollId, member.Id, "no replies")).Value!;
            await _service.RemoveCommentAsync(Poll, pollId, withReply.Id, member.Id, false, null);
            await _service.RemoveCommentAsync(Poll, pollId, alone.Id, member.Id, false, null);

            var thread = (await _service.GetThreadAsync(Poll, pollId, member.Id, false)).Value!;

            thread.Comments.Should().ContainSingle();
            var placeholder = thread.Comments[0];
            placeholder.Id.Should().Be(withReply.Id);
            placeholder.Removed.Should().BeTrue();
            placeholder.Content.Should().BeEmpty();
            placeholder.AuthorId.Should().Be(0);
            placeholder.Replies.Should().ContainSingle(r => r.Content == "the reply");
        }

        [Category("FR-40")]
        [Test]
        public async Task SetLocked_LockThenUnlock_WritesOneAuditRowEach()
        {
            var admin = await MemberAsync(17);
            var pollId = await CreatePollAsync(admin.Id);

            (await _service.SetLockedAsync(Poll, pollId, true, admin.Id)).Value.Should().BeTrue();
            (await _service.SetLockedAsync(Poll, pollId, true, admin.Id)).Value.Should().BeFalse();
            (await _service.SetLockedAsync(Poll, pollId, false, admin.Id)).Value.Should().BeTrue();

            var types = await _context.ActivityLogs.Select(a => a.ActivityType).ToListAsync();
            types.Count(t => t == Constants.Discussion.AuditTypes.ThreadLocked).Should().Be(1);
            types.Count(t => t == Constants.Discussion.AuditTypes.ThreadUnlocked).Should().Be(1);
            (await PostAsync(pollId, admin.Id, "open again")).Status.Should().Be(DiscussionStatus.Ok);
        }

        [Category("FR-40")]
        [Test]
        public async Task ForumRoutes_DoNotShowPollThreads()
        {
            var member = await MemberAsync(18);
            var pollId = await CreatePollAsync(member.Id);
            await PostAsync(pollId, member.Id, "poll talk");
            var topic = await _context.ForumTopics.IgnoreQueryFilters().SingleAsync(t => t.ContextId == pollId);
            var forum = new ForumService(_context);

            topic.CategoryId.Should().BeNull();
            (await forum.GetTopicByIdAsync(topic.Id)).Should().BeNull();
            (await forum.GetPostsByTopicAsync(topic.Id, 1, 20)).Should().BeEmpty();
            (await forum.GetCategoriesAsync()).Sum(c => c.TopicCount).Should().Be(0);
        }
    }
}
