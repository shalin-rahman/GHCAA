using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GHCAA.Infrastructure.Services
{
    public class PollService : IPollService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PollService> _logger;

        public PollService(ApplicationDbContext db, ILogger<PollService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<PollDto>> GetActivePollsAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var polls = await _db.Polls
                .Include(p => p.Options)
                .Include(p => p.Votes)
                .Where(p => p.IsActive && !p.IsArchived && (p.ExpiryDate == null || p.ExpiryDate > DateTime.UtcNow))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);

            return polls.Select(p => MapToDto(p, memberId)).ToList();
        }

        public async Task<List<PollDto>> GetAllPollsAsync(CancellationToken cancellationToken = default)
        {
            var polls = await _db.Polls
                .Include(p => p.Options)
                .Include(p => p.Votes)
                .Where(p => !p.IsArchived)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);

            return polls.Select(p => MapToDto(p, 0)).ToList();
        }

        public async Task<PollDto?> GetPollByIdAsync(int id, int memberId, CancellationToken cancellationToken = default)
        {
            var poll = await _db.Polls
                .Include(p => p.Options)
                .Include(p => p.Votes)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsArchived, cancellationToken);

            return poll != null ? MapToDto(poll, memberId) : null;
        }

        public async Task<int> CreatePollAsync(CreatePollDto dto, int adminMemberId, CancellationToken cancellationToken = default)
        {
            var poll = new Poll
            {
                Title = dto.Title,
                Description = dto.Description,
                AllowMultipleChoice = dto.AllowMultipleChoice,
                ExpiryDate = dto.ExpiryDate,
                CreatedBy = adminMemberId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            foreach (var opt in dto.Options)
            {
                poll.Options.Add(new PollOption { OptionText = opt });
            }

            _db.Polls.Add(poll);
            await _db.SaveChangesAsync(cancellationToken);
            return poll.Id;
        }

        public async Task<bool> VoteAsync(int pollId, int memberId, List<int> optionIds, CancellationToken cancellationToken = default)
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
            return true;
        }

        public async Task<bool> TogglePollStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default)
        {
            var poll = await _db.Polls.FindAsync(new object[] { id }, cancellationToken);
            if (poll == null) return false;

            poll.IsActive = isActive;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeletePollAsync(int id, CancellationToken cancellationToken = default)
        {
            var poll = await _db.Polls.FindAsync(new object[] { id }, cancellationToken);
            if (poll == null) return false;

            poll.IsArchived = true;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
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
