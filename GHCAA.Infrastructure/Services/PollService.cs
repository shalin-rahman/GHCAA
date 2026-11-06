using GHCAA.Application.DTOs;
using GHCAA.Application.DTOs.Discussion;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    public class PollService : IPollService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PollService> _logger;
        private readonly IDiscussionService _discussion;
        private readonly INewsService _news;
        private readonly IActivityService _activity;

        public PollService(ApplicationDbContext db, ILogger<PollService> logger, IDiscussionService discussion, INewsService news, IActivityService activity)
        {
            _db = db;
            _logger = logger;
            _discussion = discussion;
            _news = news;
            _activity = activity;
        }

        public async Task<List<PollDto>> GetActivePollsAsync(int memberId, string? category, CancellationToken cancellationToken = default)
        {
            var query = _db.Polls
                .Include(p => p.Options)
                .Include(p => p.Votes)
                .Where(p => p.IsActive && !p.IsArchived && (p.ExpiryDate == null || p.ExpiryDate > DateTime.UtcNow));
            query = FilterByCategory(query, category);

            var polls = await query.OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);
            return await MapAllAsync(polls, memberId, cancellationToken);
        }

        public async Task<List<PollDto>> GetAllPollsAsync(string? category, bool includeArchived, CancellationToken cancellationToken = default)
        {
            // PollOption hides the options of an archived poll, so the archived view has to lift that filter.
            var query = includeArchived ? _db.Polls.IgnoreQueryFilters() : _db.Polls.Where(p => !p.IsArchived);
            query = FilterByCategory(query, category);

            var polls = await query
                .Include(p => p.Options)
                .Include(p => p.Votes)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
            return await MapAllAsync(polls, 0, cancellationToken);
        }

        public async Task<PollDto?> GetPollByIdAsync(int id, int memberId, CancellationToken cancellationToken = default)
        {
            var poll = await _db.Polls
                .Include(p => p.Options)
                .Include(p => p.Votes)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsArchived, cancellationToken);

            return poll != null ? (await MapAllAsync(new List<Poll> { poll }, memberId, cancellationToken)).Single() : null;
        }

        public async Task<int> CreatePollAsync(CreatePollDto dto, int adminMemberId, int? authorUserId, CancellationToken cancellationToken = default)
        {
            var category = string.IsNullOrWhiteSpace(dto.Category) ? Constants.Discussion.DefaultCategory : dto.Category.Trim();
            // Other is always allowed, so a database without the lookup rows can still take a poll.
            // Any other value is matched ignoring case and stored as the lookup spells it, because
            // the category filter compares exactly and PostgreSQL is case-sensitive.
            if (string.Equals(category, Constants.Discussion.DefaultCategory, StringComparison.OrdinalIgnoreCase))
                category = Constants.Discussion.DefaultCategory;
            else
            {
                var lower = category.ToLower();
                category = await _db.Lookups
                    .Where(l => l.LookupGroup == Constants.Discussion.CategoryLookupGroup && l.IsActive && l.Value.ToLower() == lower)
                    .Select(l => l.Value)
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new ArgumentException($"'{dto.Category!.Trim()}' is not a poll category.");
            }
            if (dto.Announcement != null && authorUserId is not > 0)
                throw new ArgumentException("An announcement needs a signed-in admin user as its author.");

            var poll = new Poll
            {
                Title = dto.Title,
                Description = dto.Description,
                AllowMultipleChoice = dto.AllowMultipleChoice,
                ExpiryDate = dto.ExpiryDate,
                CreatedBy = adminMemberId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                AllowComments = dto.AllowComments,
                Category = category,
            };

            foreach (var opt in dto.Options)
            {
                poll.Options.Add(new PollOption { OptionText = opt });
            }

            if (dto.Announcement == null)
            {
                _db.Polls.Add(poll);
                await _db.SaveChangesAsync(cancellationToken);
                return poll.Id;
            }

            // The poll and its announcement are saved together or not at all.
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _db.Polls.Add(poll);
                await _db.SaveChangesAsync(cancellationToken);

                var a = dto.Announcement;
                // The post links to the poll page. The options stay on the poll so results never go stale in a post.
                var news = await _news.CreateNewsAsync(new CreateNewsDto
                {
                    Title = a.Title,
                    Content = $"{a.Content}<p><a href=\"{Constants.Discussion.PollPagePath}\">Open the poll</a></p>",
                    PostType = a.PostType,
                    PublishDate = a.PublishDate,
                    MembersOnly = a.MembersOnly,
                    IsActive = true,
                }, authorUserId!.Value, cancellationToken);

                poll.AnnouncementId = news.Id;
                await _db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return poll.Id;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                _db.ChangeTracker.Clear();
                throw;
            }
        }

        public async Task<bool> VoteAsync(int pollId, int memberId, List<int> optionIds, CancellationToken cancellationToken = default)
        {
            // 82.32: an empty list passed every check below (no duplicate vote, no choice-count
            // violation, no invalid option) and returned true having recorded nothing — a caller
            // could not tell an actual vote from a no-op.
            if (optionIds == null || optionIds.Count == 0) return false;

            // 82.32: the unique index backing this is (PollOptionId, MemberId), which is correct —
            // it has to allow several rows per member on a multiple-choice poll. That leaves the
            // "already voted" rule for single-choice polls enforced only by the in-memory check
            // below, so two concurrent single-choice votes for two different options both passed it
            // and both got inserted. Serializable isolation, the same pattern MemberService already
            // uses for its own check-then-act writes, makes the second transaction fail instead.
            using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            try
            {
                var poll = await _db.Polls
                    .Include(p => p.Options)
                    .Include(p => p.Votes)
                    .FirstOrDefaultAsync(p => p.Id == pollId && p.IsActive && !p.IsArchived, cancellationToken);

                if (poll == null) return false;
                if (poll.ExpiryDate < DateTime.UtcNow) return false;

                // Check if already voted
                if (poll.Votes.Any(v => v.MemberId == memberId)) return false;

                // Check choice constraints
                if (!poll.AllowMultipleChoice && optionIds.Count > 1) return false;

                // Check if all optionIds belong to this poll
                var validOptions = poll.Options.Select(o => o.Id).ToList();
                if (optionIds.Any(id => !validOptions.Contains(id))) return false;

                foreach (var optId in optionIds)
                {
                    _db.PollVotes.Add(new PollVote
                    {
                        PollId = pollId,
                        PollOptionId = optId,
                        MemberId = memberId,
                        VotedAt = DateTime.UtcNow
                    });
                }

                await _db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return true;
            }
            catch (Exception)
            {
                // Matches MemberService's other Serializable-transaction writes: roll back and
                // rethrow rather than swallow, so a genuine conflict surfaces instead of reading as
                // an ordinary "vote rejected" false.
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<bool> TogglePollStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default)
        {
            var poll = await _db.Polls.FindAsync(new object[] { id }, cancellationToken);
            if (poll == null) return false;

            poll.IsActive = isActive;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeletePollAsync(int id, int adminMemberId, bool hideAnnouncement, CancellationToken cancellationToken = default)
        {
            var poll = await _db.Polls.FindAsync(new object[] { id }, cancellationToken);
            if (poll == null) return false;

            poll.IsArchived = true;
            if (hideAnnouncement && poll.AnnouncementId is int newsId)
            {
                var news = await _db.NewsPosts.IgnoreQueryFilters().FirstOrDefaultAsync(n => n.Id == newsId, cancellationToken);
                if (news != null) news.IsActive = false;
            }
            await _db.SaveChangesAsync(cancellationToken);

            // An archived poll keeps its comments for the record but takes no new ones.
            var locked = await _discussion.SetLockedAsync(Constants.Discussion.ContextPoll, id, true, adminMemberId, cancellationToken);
            if (locked.Status != DiscussionStatus.Ok)
                _logger.LogWarning("Poll {PollId} archived but its thread was not locked: {Error}", id, locked.Error);
            return true;
        }

        public async Task<bool> SetAllowCommentsAsync(int id, bool allowComments, int adminMemberId, CancellationToken cancellationToken = default)
        {
            var poll = await _db.Polls.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (poll == null) return false;
            if (poll.AllowComments == allowComments) return true;

            poll.AllowComments = allowComments;
            await _db.SaveChangesAsync(cancellationToken);
            await _activity.LogActivityAsync(null, Constants.Discussion.AuditTypes.CommentsSwitched,
                $"Admin {adminMemberId} turned comments {(allowComments ? "on" : "off")} for poll {id}.",
                actorId: adminMemberId > 0 ? adminMemberId : null, cancellationToken: cancellationToken);
            return true;
        }

        // Polls saved before categories existed have no category, and they count as Other.
        private static IQueryable<Poll> FilterByCategory(IQueryable<Poll> query, string? category)
        {
            if (string.IsNullOrWhiteSpace(category)) return query;
            category = category.Trim();
            return category == Constants.Discussion.DefaultCategory
                ? query.Where(p => p.Category == category || p.Category == null || p.Category == "")
                : query.Where(p => p.Category == category);
        }

        private async Task<List<PollDto>> MapAllAsync(List<Poll> polls, int memberId, CancellationToken ct)
        {
            var ids = polls.Select(p => p.Id).ToList();
            var counts = await _discussion.CountCommentsAsync(Constants.Discussion.ContextPoll, ids, ct);
            var lockedIds = await _db.ForumTopics.IgnoreQueryFilters()
                .Where(t => t.ContextType == Constants.Discussion.ContextPoll && t.IsLocked && ids.Contains(t.ContextId!.Value))
                .Select(t => t.ContextId!.Value)
                .ToListAsync(ct);

            return polls.Select(p =>
            {
                var dto = MapToDto(p, memberId);
                dto.CommentCount = counts.GetValueOrDefault(p.Id);
                dto.CommentsLocked = lockedIds.Contains(p.Id);
                return dto;
            }).ToList();
        }

        private PollDto MapToDto(Poll poll, int memberId)
        {
            var totalVotes = poll.Votes.Select(v => v.MemberId).Distinct().Count();
            var userVotes = poll.Votes.Where(v => v.MemberId == memberId).Select(v => v.PollOptionId).ToList();

            return new PollDto
            {
                Id = poll.Id,
                Title = poll.Title,
                Description = poll.Description,
                AllowMultipleChoice = poll.AllowMultipleChoice,
                IsActive = poll.IsActive,
                CreatedAt = poll.CreatedAt,
                ExpiryDate = poll.ExpiryDate,
                TotalVotes = totalVotes,
                HasVoted = userVotes.Any(),
                SelectedOptionIds = userVotes,
                IsArchived = poll.IsArchived,
                AllowComments = poll.AllowComments,
                Category = poll.Category,
                AnnouncementId = poll.AnnouncementId,
                Options = poll.Options.Select(o => new PollOptionDto
                {
                    Id = o.Id,
                    Text = o.OptionText,
                    VoteCount = poll.Votes.Count(v => v.PollOptionId == o.Id),
                    Percentage = totalVotes > 0 ? Math.Round((double)poll.Votes.Count(v => v.PollOptionId == o.Id) / totalVotes * 100, 2) : 0
                }).ToList()
            };
        }
    }
}
