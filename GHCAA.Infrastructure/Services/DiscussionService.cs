using Ganss.Xss;
using GHCAA.Application.DTOs.Discussion;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace GHCAA.Infrastructure.Services
{
    // Spec 028. A poll or meeting thread is a ForumTopic with ContextType and ContextId set and no
    // category, so the forum lists never show it. Its comments are ForumPosts.
    public class DiscussionService : IDiscussionService
    {
        private readonly ApplicationDbContext _db;
        private readonly IActivityService _activity;
        private static readonly HtmlSanitizer _sanitizer = new();

        public DiscussionService(ApplicationDbContext db, IActivityService activity)
        {
            _db = db;
            _activity = activity;
        }

        private sealed record ContextInfo(string Title, int? OwnerMemberId, string? ClosedReason);

        // Null means the caller may not see the context, which the API answers with 404.
        private async Task<ContextInfo?> ResolveContextAsync(string contextType, int contextId, bool isAdmin, CancellationToken ct)
        {
            if (contextType == Constants.Discussion.ContextPoll)
            {
                var poll = await _db.Polls.IgnoreQueryFilters().AsNoTracking()
                    .Where(p => p.Id == contextId)
                    .Select(p => new { p.Title, p.CreatedBy, p.IsArchived, p.AllowComments })
                    .FirstOrDefaultAsync(ct);
                if (poll == null || (poll.IsArchived && !isAdmin)) return null;

                var closed = poll.IsArchived ? "This poll is archived, so comments are closed."
                    : !poll.AllowComments ? "Comments are turned off for this poll."
                    : null;
                return new ContextInfo(poll.Title, poll.CreatedBy, closed);
            }
            return null;
        }

        private Task<ForumTopic?> FindTopicAsync(string contextType, int contextId, CancellationToken ct) =>
            _db.ForumTopics.IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.ContextType == contextType && t.ContextId == contextId, ct);

        // The topic author must be a real member row. A system admin has none, so fall back to the
        // context owner.
        private async Task<ForumTopic?> FindOrCreateTopicAsync(string contextType, int contextId, ContextInfo info, int actingMemberId, CancellationToken ct)
        {
            var topic = await FindTopicAsync(contextType, contextId, ct);
            if (topic != null) return topic;

            int? authorId = null;
            if (actingMemberId > 0 && await _db.Members.IgnoreQueryFilters().AnyAsync(m => m.Id == actingMemberId, ct))
                authorId = actingMemberId;
            else if (info.OwnerMemberId is int owner && owner > 0 && await _db.Members.IgnoreQueryFilters().AnyAsync(m => m.Id == owner, ct))
                authorId = owner;
            if (authorId == null) return null;

            topic = new ForumTopic
            {
                ContextType = contextType,
                ContextId = contextId,
                CategoryId = null,
                Title = info.Title.Length > 200 ? info.Title[..200] : info.Title,
                Content = "",
                AuthorId = authorId.Value,
            };
            _db.ForumTopics.Add(topic);
            try
            {
                await _db.SaveChangesAsync(ct);
                return topic;
            }
            catch (DbUpdateException)
            {
                // Another request may have created the thread first and hit the unique index. If no
                // thread exists after all, the save failed for another reason.
                _db.Entry(topic).State = EntityState.Detached;
                var existing = await FindTopicAsync(contextType, contextId, ct);
                if (existing == null) throw;
                return existing;
            }
        }

        public async Task<DiscussionResult<CommentThreadDto>> GetThreadAsync(string contextType, int contextId, int memberId, bool isAdmin, int page = 1, int pageSize = Constants.Discussion.DefaultPageSize, CancellationToken cancellationToken = default)
        {
            var info = await ResolveContextAsync(contextType, contextId, isAdmin, cancellationToken);
            if (info == null) return DiscussionResult<CommentThreadDto>.Missing("Not found.");

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var topic = await _db.ForumTopics.IgnoreQueryFilters().AsNoTracking()
                .FirstOrDefaultAsync(t => t.ContextType == contextType && t.ContextId == contextId, cancellationToken);

            var result = new CommentThreadDto
            {
                Page = page,
                PageSize = pageSize,
                IsLocked = topic?.IsLocked ?? false,
                AllowComments = info.ClosedReason == null && topic?.IsLocked != true,
                ClosedReason = info.ClosedReason ?? (topic?.IsLocked == true ? LockedReason : null),
            };
            if (topic == null) return DiscussionResult<CommentThreadDto>.Success(result);

            // Threads are small, so the whole thread loads and paging runs over top-level comments.
            var posts = await _db.ForumPosts.IgnoreQueryFilters().AsNoTracking()
                .Where(p => p.TopicId == topic.Id)
                .Include(p => p.Author)
                .OrderBy(p => p.CreatedAt).ThenBy(p => p.Id)
                .ToListAsync(cancellationToken);

            var repliesByParent = posts.Where(p => p.ParentPostId != null)
                .Where(IsVisible)
                .GroupBy(p => p.ParentPostId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var topLevel = posts.Where(p => p.ParentPostId == null)
                .Where(p => IsVisible(p) || repliesByParent.ContainsKey(p.Id))
                .ToList();

            result.TotalCount = topLevel.Count;
            result.Comments = topLevel
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p =>
                {
                    var dto = ToDto(p);
                    if (repliesByParent.TryGetValue(p.Id, out var replies))
                        dto.Replies = replies.Select(ToDto).ToList();
                    return dto;
                })
                .ToList();

            return DiscussionResult<CommentThreadDto>.Success(result);
        }

        private const string LockedReason = "An admin has locked this thread.";
        private const string NoProfileReason = "Your account has no member profile, so you cannot comment.";

        // Matches the forum's own filter: inactive posts and posts by archived members are hidden.
        private static bool IsVisible(ForumPost p) => p.IsActive && (p.Author == null || !p.Author.IsArchived);

        private static CommentDto ToDto(ForumPost p)
        {
            var visible = IsVisible(p);
            return new CommentDto
            {
                Id = p.Id,
                ParentId = p.ParentPostId,
                AuthorId = visible ? p.AuthorId : 0,
                AuthorName = visible ? p.Author?.FullName ?? "Unknown" : "",
                AuthorPhotoUrl = visible ? p.Author?.PhotoPath : null,
                Content = visible ? p.Content : "",
                PostedAt = p.CreatedAt,
                Removed = !visible,
            };
        }

        public async Task<DiscussionResult<CommentDto>> PostCommentAsync(string contextType, int contextId, int memberId, bool isAdmin, CreateCommentDto dto, CancellationToken cancellationToken = default)
        {
            var info = await ResolveContextAsync(contextType, contextId, isAdmin, cancellationToken);
            if (info == null) return DiscussionResult<CommentDto>.Missing("Not found.");
            if (info.ClosedReason != null) return DiscussionResult<CommentDto>.Bad(info.ClosedReason);

            // Comments are plain text and the clients escape them on display. Tags are stripped and the
            // sanitizer's entity encoding is undone, so "a < b & c" is stored and shown as typed.
            var clean = WebUtility.HtmlDecode(_sanitizer.Sanitize((dto.Content ?? "").Trim())).Trim();
            if (clean.Length == 0) return DiscussionResult<CommentDto>.Bad("Write a comment first.");
            if (clean.Length > Constants.Discussion.MaxCommentLength)
                return DiscussionResult<CommentDto>.Bad($"A comment can be at most {Constants.Discussion.MaxCommentLength} characters.");

            if (memberId <= 0 || !await _db.Members.IgnoreQueryFilters().AnyAsync(m => m.Id == memberId, cancellationToken))
                return DiscussionResult<CommentDto>.Bad(NoProfileReason);

            var topic = await FindOrCreateTopicAsync(contextType, contextId, info, memberId, cancellationToken);
            if (topic == null) return DiscussionResult<CommentDto>.Bad(NoProfileReason);
            if (topic.IsLocked) return DiscussionResult<CommentDto>.Bad(LockedReason);

            int? parentId = null;
            if (dto.ParentId is int requested)
            {
                var parent = await _db.ForumPosts.IgnoreQueryFilters().AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == requested, cancellationToken);
                if (parent == null || parent.TopicId != topic.Id)
                    return DiscussionResult<CommentDto>.Bad("That comment is not part of this thread.");
                if (!parent.IsActive)
                    return DiscussionResult<CommentDto>.Bad("That comment was removed, so it cannot be replied to.");
                // One level deep: a reply to a reply hangs off the top-level comment.
                parentId = parent.ParentPostId ?? parent.Id;
            }

            var post = new ForumPost
            {
                TopicId = topic.Id,
                ParentPostId = parentId,
                AuthorId = memberId,
                Content = clean,
                CreatedAt = DateTime.UtcNow,
            };
            _db.ForumPosts.Add(post);
            topic.LastUpdatedAt = post.CreatedAt;
            await _db.SaveChangesAsync(cancellationToken);

            var author = await _db.Members.IgnoreQueryFilters().AsNoTracking()
                .Where(m => m.Id == memberId)
                .Select(m => new { m.FullName, m.PhotoPath })
                .FirstOrDefaultAsync(cancellationToken);

            return DiscussionResult<CommentDto>.Success(new CommentDto
            {
                Id = post.Id,
                ParentId = post.ParentPostId,
                AuthorId = memberId,
                AuthorName = author?.FullName ?? "Unknown",
                AuthorPhotoUrl = author?.PhotoPath,
                Content = post.Content,
                PostedAt = post.CreatedAt,
            });
        }

        public async Task<DiscussionResult<bool>> RemoveCommentAsync(string contextType, int contextId, int commentId, int memberId, bool isAdmin, string? reason, CancellationToken cancellationToken = default)
        {
            var info = await ResolveContextAsync(contextType, contextId, isAdmin, cancellationToken);
            if (info == null) return DiscussionResult<bool>.Missing("Not found.");

            var post = await _db.ForumPosts.IgnoreQueryFilters()
                .Include(p => p.Topic)
                .FirstOrDefaultAsync(p => p.Id == commentId, cancellationToken);
            if (post == null || post.Topic == null || post.Topic.ContextType != contextType || post.Topic.ContextId != contextId || !post.IsActive)
                return DiscussionResult<bool>.Missing("Comment not found.");

            var isAuthor = post.AuthorId == memberId;
            if (!isAuthor && !isAdmin) return DiscussionResult<bool>.Denied("You can only remove your own comments.");

            post.IsActive = false;
            post.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);

            if (!isAuthor)
            {
                var why = string.IsNullOrWhiteSpace(reason) ? "No reason given." : reason.Trim();
                await _activity.LogActivityAsync(post.AuthorId, Constants.Discussion.AuditTypes.CommentRemoved,
                    $"Admin {memberId} removed comment {post.Id} on {contextType} {contextId}. Reason: {why}",
                    actorId: memberId, cancellationToken: cancellationToken);
            }
            return DiscussionResult<bool>.Success(true);
        }

        public async Task<DiscussionResult<bool>> SetLockedAsync(string contextType, int contextId, bool locked, int adminMemberId, CancellationToken cancellationToken = default)
        {
            var info = await ResolveContextAsync(contextType, contextId, true, cancellationToken);
            if (info == null) return DiscussionResult<bool>.Missing("Not found.");

            var topic = locked
                ? await FindOrCreateTopicAsync(contextType, contextId, info, adminMemberId, cancellationToken)
                : await FindTopicAsync(contextType, contextId, cancellationToken);
            if (topic == null)
            {
                // Unlocking a thread nobody has started is already done.
                if (!locked) return DiscussionResult<bool>.Success(false);
                return DiscussionResult<bool>.Bad("The thread could not be started because no member profile is linked to it.");
            }
            if (topic.IsLocked == locked) return DiscussionResult<bool>.Success(false);

            topic.IsLocked = locked;
            await _db.SaveChangesAsync(cancellationToken);

            await _activity.LogActivityAsync(null,
                locked ? Constants.Discussion.AuditTypes.ThreadLocked : Constants.Discussion.AuditTypes.ThreadUnlocked,
                $"Admin {adminMemberId} {(locked ? "locked" : "unlocked")} the comments on {contextType} {contextId}.",
                actorId: adminMemberId > 0 ? adminMemberId : null, cancellationToken: cancellationToken);
            return DiscussionResult<bool>.Success(true);
        }

        public async Task<Dictionary<int, int>> CountCommentsAsync(string contextType, IReadOnlyCollection<int> contextIds, CancellationToken cancellationToken = default)
        {
            if (contextIds.Count == 0) return new Dictionary<int, int>();
            // The post query filter already drops removed posts and archived authors.
            return await _db.ForumPosts
                .Where(p => p.Topic!.ContextType == contextType && contextIds.Contains(p.Topic.ContextId!.Value))
                .GroupBy(p => p.Topic!.ContextId!.Value)
                .Select(g => new { Id = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.Count, cancellationToken);
        }
    }
}
