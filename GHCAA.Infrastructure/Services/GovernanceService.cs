using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services
{
    public class GovernanceService : IGovernanceService
    {
        private readonly ApplicationDbContext _db;
        private readonly INotificationService _notificationService;

        public GovernanceService(ApplicationDbContext db, INotificationService notificationService)
        {
            _db = db;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<ECPeriodDto>> GetAllPeriodsAsync(CancellationToken cancellationToken = default)
        {
            var periods = await _db.ECPeriods
                .OrderByDescending(p => p.StartDate)
                .ToListAsync(cancellationToken);

            return periods.Select(MapToPeriodDto);
        }

        public async Task<ECPeriodDto?> GetPeriodByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var period = await _db.ECPeriods
                .Include(p => p.ECMembers)
                .ThenInclude(m => m.Member)
                .Include(p => p.ECMembers)
                .ThenInclude(m => m.EcPosition)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            return period == null ? null : MapToPeriodDto(period);
        }

        public async Task<ECPeriodDto?> GetActivePeriodAsync(CancellationToken cancellationToken = default)
        {
            var period = await _db.ECPeriods
                .FirstOrDefaultAsync(p => p.IsActive, cancellationToken);

            return period == null ? null : MapToPeriodDto(period);
        }

        public async Task<ECPeriodDto> CreatePeriodAsync(string title, DateTime startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc) : (DateTime?)null;

            await EnsureNoOverlapAsync(null, utcStart, utcEnd, cancellationToken);

            var period = new ECPeriod
            {
                Title = title,
                StartDate = utcStart,
                EndDate = utcEnd,
                IsActive = false
            };

            _db.ECPeriods.Add(period);
            await _db.SaveChangesAsync(cancellationToken);
            return MapToPeriodDto(period);
        }

        public async Task<bool> UpdatePeriodAsync(int id, string title, DateTime startDate, DateTime? endDate, bool isActive, CancellationToken cancellationToken = default)
        {
            var period = await _db.ECPeriods.FindAsync(new object[] { id }, cancellationToken);
            if (period == null) return false;

            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc) : (DateTime?)null;

            await EnsureNoOverlapAsync(id, utcStart, utcEnd, cancellationToken);

            period.Title = title;
            period.StartDate = utcStart;
            period.EndDate = utcEnd;

            if (isActive && !period.IsActive)
            {
                var today = DateTime.UtcNow.Date;
                if (utcStart.Date > today || (utcEnd.HasValue && utcEnd.Value.Date < today))
                {
                    throw new InvalidOperationException("Only a period covering the current date can be activated.");
                }
                await ActivatePeriodInternalAsync(id, cancellationToken);
            }
            else if (!isActive && period.IsActive)
            {
                period.IsActive = false;
            }
            else
            {
                period.IsActive = isActive;
            }

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        private async Task EnsureNoOverlapAsync(int? excludeId, DateTime start, DateTime? end, CancellationToken ct)
        {
            var query = _db.ECPeriods.Where(p => (!excludeId.HasValue || p.Id != excludeId.Value));
            var overlapping = await query.AnyAsync(p =>
                (p.EndDate == null || p.EndDate >= start) && (end == null || p.StartDate <= end), ct);

            if (overlapping)
            {
                throw new InvalidOperationException("The specified dates overlap with an existing EC Period.");
            }
        }

        public async Task<bool> ActivatePeriodAsync(int id, CancellationToken cancellationToken = default)
        {
            var period = await _db.ECPeriods.FindAsync(new object[] { id }, cancellationToken);
            if (period == null) return false;

            var today = DateTime.UtcNow.Date;
            if (period.StartDate.Date > today || (period.EndDate.HasValue && period.EndDate.Value.Date < today))
            {
                throw new InvalidOperationException("Only a period covering the current date can be activated.");
            }

            var success = await ActivatePeriodInternalAsync(id, cancellationToken);
            if (success)
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            return success;
        }

        private async Task<bool> ActivatePeriodInternalAsync(int id, CancellationToken cancellationToken)
        {
            var activePeriods = await _db.ECPeriods.Where(p => p.IsActive).ToListAsync(cancellationToken);
            foreach (var p in activePeriods) p.IsActive = false;

            var period = await _db.ECPeriods.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (period != null) period.IsActive = true;

            return true;
        }

        public async Task<IEnumerable<ECMemberDto>> GetCommitteeMembersAsync(int periodId, CancellationToken cancellationToken = default)
        {
            var members = await _db.ECMembers
                .Include(em => em.Member)
                .Include(em => em.EcPosition)
                .Where(em => em.ECPeriodId == periodId && em.EndDate == null)
                .OrderBy(em => em.EcPosition!.DisplayOrder).ThenBy(em => em.Position)
                .ToListAsync(cancellationToken);

            return members.Select(MapToMemberDto);
        }

        public async Task<IEnumerable<CommitteeSeatDto>> GetCommitteeSeatsAsync(int periodId, CancellationToken cancellationToken = default)
        {
            var rows = await _db.ECMembers
                .Include(em => em.Member)
                .Include(em => em.EcPosition)
                .Where(em => em.ECPeriodId == periodId)
                .ToListAsync(cancellationToken);

            // 95.2: a retired position still shows in a period where someone held it.
            var heldIds = rows.Select(r => r.Position).Distinct().ToList();
            var positions = await _db.EcPositions
                .Where(p => p.IsActive || heldIds.Contains(p.Id))
                .OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id)
                .ToListAsync(cancellationToken);

            var seats = new List<CommitteeSeatDto>();
            foreach (var position in positions)
            {
                var held = rows.Where(r => r.Position == position.Id).ToList();
                // Older data can hold more open rows than seats. The newest ones fill the seats.
                var holders = held.Where(r => r.EndDate == null).OrderByDescending(r => r.StartDate)
                    .Take(position.SeatCount).ToList();
                foreach (var holder in holders)
                {
                    seats.Add(new CommitteeSeatDto
                    {
                        Position = position.Key,
                        PositionId = position.Id,
                        PositionName = position.Name,
                        ShowOnBoard = position.ShowOnBoard,
                        Holder = MapToMemberDto(holder)
                    });
                }

                // Each empty seat shows the most recent term that ended on this position, newest first.
                var ended = held.Where(r => r.EndDate != null).OrderByDescending(r => r.EndDate).ToList();
                for (var i = 0; i < position.SeatCount - holders.Count; i++)
                {
                    var last = i < ended.Count ? ended[i] : null;
                    seats.Add(new CommitteeSeatDto
                    {
                        Position = position.Key,
                        PositionId = position.Id,
                        PositionName = position.Name,
                        ShowOnBoard = position.ShowOnBoard,
                        VacancyReason = last?.EndReason,
                        VacancyNote = last?.EndNote,
                        VacatedOn = last?.EndDate,
                        LastHolderName = last?.Member?.FullName
                    });
                }
            }
            return seats;
        }

        public async Task<bool> AssignMemberToRoleAsync(int periodId, int memberId, int position, string? reason, bool notifyMember = false,
            VacancyReason? endCurrentHolderReason = null, string? endCurrentHolderNote = null, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            if (member == null || member.Status != MembershipStatus.Active)
            {
                throw new InvalidOperationException("Only active members can be assigned to the Executive Committee.");
            }

            // 95.3: one current holder per seat. Replacing someone has to say why their term ended.
            // Serializable so two admins filling the same seat at once cannot both get in, and a
            // position retired at the same moment is not filled.
            await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            var ecPosition = await _db.EcPositions.FirstOrDefaultAsync(p => p.Id == position && p.IsActive, cancellationToken);
            if (ecPosition == null)
            {
                throw new InvalidOperationException("Pick a committee position.");
            }

            var currentHolders = await _db.ECMembers
                .Include(em => em.Member)
                .Where(em => em.ECPeriodId == periodId && em.Position == position && em.EndDate == null)
                .ToListAsync(cancellationToken);
            if (currentHolders.Any(h => h.MemberId == memberId))
            {
                throw new InvalidOperationException("This member already holds that seat.");
            }
            // A position with spare seats takes another holder. When a multi-seat position is full
            // there is no safe guess at whom to replace, so the admin ends one term first.
            if (currentHolders.Count >= ecPosition.SeatCount && ecPosition.SeatCount > 1)
            {
                throw new InvalidOperationException($"All {ecPosition.SeatCount} {ecPosition.Name} seats are held. End one term first.");
            }
            if (currentHolders.Count > 0 && ecPosition.SeatCount == 1)
            {
                if (endCurrentHolderReason == null)
                {
                    var name = currentHolders[0].Member?.FullName ?? "another member";
                    throw new InvalidOperationException($"{ecPosition.Name} is held by {name}. End their term first, or replace them and give a reason.");
                }
                foreach (var holder in currentHolders)
                {
                    EndTerm(holder, endCurrentHolderReason.Value, endCurrentHolderNote);
                }
            }

            var newAssignment = new ECMember
            {
                ECPeriodId = periodId,
                MemberId = memberId,
                Position = position,
                StartDate = DateTime.UtcNow,
                ChangeReason = reason
            };

            _db.ECMembers.Add(newAssignment);
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is DbUpdateException or DbException)
            {
                _db.ChangeTracker.Clear();
                throw new InvalidOperationException("Someone else changed this seat just now. Reload and try again.", ex);
            }

            // 82.52: admin-chosen, off by default (nothing notified here before this item).
            if (notifyMember)
            {
                await _notificationService.CreateNotificationAsync(
                    memberId,
                    "Committee Assignment",
                    $"You have been assigned to the Executive Committee as {ecPosition.Name}.",
                    NotificationType.CommitteeAssignment,
                    cancellationToken: cancellationToken);
            }

            return true;
        }

        public async Task<bool> RemoveMemberFromCommitteeAsync(int ecMemberId, VacancyReason reason, string? note, bool notifyMember = false, CancellationToken cancellationToken = default)
        {
            var ecMember = await _db.ECMembers.FindAsync(new object[] { ecMemberId }, cancellationToken);
            if (ecMember == null) return false;
            if (ecMember.EndDate != null)
            {
                throw new InvalidOperationException("This term has already ended.");
            }

            EndTerm(ecMember, reason, note);
            await _db.SaveChangesAsync(cancellationToken);

            // 82.52: admin-chosen, off by default (nothing notified here before this item).
            if (notifyMember)
            {
                await _notificationService.CreateNotificationAsync(
                    ecMember.MemberId,
                    "Committee Term Ended",
                    "Your term on the Executive Committee has ended.",
                    NotificationType.CommitteeAssignment,
                    cancellationToken: cancellationToken);
            }

            return true;
        }

        public async Task<bool> DeleteECMemberAsync(int id, int adminId, bool notifyMember = false, CancellationToken cancellationToken = default)
        {
            var ecMember = await _db.ECMembers.FindAsync(new object[] { id }, cancellationToken);
            if (ecMember == null || ecMember.IsArchived) return false;

            // 82.29: soft delete, replacing _db.ECMembers.Remove(ecMember). ECMember is Class A
            // (ARCHITECTURE.md §4) — this path is for a row that should never have existed (wrong
            // member added), which is still worth keeping as evidence of who removed it and when,
            // the same reasoning FinancialService.DeletePaymentAsync already applies to PaymentHistory.
            ecMember.IsArchived = true;
            ecMember.DeletedAt = DateTime.UtcNow;
            ecMember.DeletedByAdminId = adminId;

            await _db.SaveChangesAsync(cancellationToken);

            // 82.52: admin-chosen, off by default. This is the "wrong member added" correction
            // path, so notifying is rarely wanted, but an admin who does want to explain the
            // removal to the affected member can opt in per action.
            if (notifyMember)
            {
                await _notificationService.CreateNotificationAsync(
                    ecMember.MemberId,
                    "Committee Record Removed",
                    "A committee role record for you has been removed by an administrator.",
                    NotificationType.CommitteeAssignment,
                    cancellationToken: cancellationToken);
            }

            return true;
        }

        public async Task<Constitution?> GetActiveConstitutionAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Constitutions
                .OrderByDescending(c => c.EffectiveDate)
                .FirstOrDefaultAsync(c => c.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<Constitution>> GetConstitutionHistoryAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Constitutions
                .OrderByDescending(c => c.EffectiveDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> CreateConstitutionVersionAsync(string version, string content, string changeSummary, CancellationToken cancellationToken = default)
        {
            var constitution = new Constitution
            {
                Version = version,
                Content = content,
                ChangeSummary = changeSummary,
                EffectiveDate = DateTime.UtcNow,
                IsActive = false
            };

            _db.Constitutions.Add(constitution);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> ActivateConstitutionAsync(int id, CancellationToken cancellationToken = default)
        {
            var target = await _db.Constitutions.FindAsync(new object[] { id }, cancellationToken);
            if (target == null) return false;

            var activeList = await _db.Constitutions.Where(c => c.IsActive).ToListAsync(cancellationToken);
            foreach (var c in activeList)
            {
                c.IsActive = false;
                c.SupersededDate = DateTime.UtcNow;
            }

            target.IsActive = true;
            target.SupersededDate = null;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> VoteOnConstitutionAsync(int constitutionId, int memberId, bool isFor, string? comments, CancellationToken cancellationToken = default)
        {
            var constitution = await _db.Constitutions.FindAsync(new object[] { constitutionId }, cancellationToken);
            if (constitution == null || !constitution.IsActive) return false;

            // Article III Section K: only Founding, Executive and General members are Voting Members.
            // Associate, Honorary and Advisory members may read and comment but not ratify amendments.
            var isVotingMember = await _db.Members.AnyAsync(m => m.Id == memberId &&
                (m.MembershipType == MembershipType.Founding ||
                 m.MembershipType == MembershipType.Executive ||
                 m.MembershipType == MembershipType.General), cancellationToken);
            if (!isVotingMember) return false;

            var alreadyVoted = await _db.AmendmentVotes.AnyAsync(v => v.ConstitutionId == constitutionId && v.MemberId == memberId, cancellationToken);
            if (alreadyVoted) return false;

            var vote = new AmendmentVote
            {
                ConstitutionId = constitutionId,
                MemberId = memberId,
                IsFor = isFor,
                Comments = comments,
                VotedAt = DateTime.UtcNow
            };

            _db.AmendmentVotes.Add(vote);
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // A concurrent request for the same member won the unique index on
                // (ConstitutionId, MemberId). Report it the same way as the alreadyVoted check.
                return false;
            }
            return true;
        }

        // Mappings
        private static ECPeriodDto MapToPeriodDto(ECPeriod p) => new()
        {
            Id = p.Id,
            Title = p.Title,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            IsActive = p.IsActive,
            ECMembers = p.ECMembers?.Select(MapToMemberDto).ToList() ?? new List<ECMemberDto>()
        };

        public async Task<IReadOnlyList<EcPositionDto>> GetPositionsAsync(bool includeRetired, CancellationToken cancellationToken = default)
        {
            var rows = await _db.EcPositions
                .Where(p => includeRetired || p.IsActive)
                .OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id)
                .ToListAsync(cancellationToken);
            return rows.Select(MapToPositionDto).ToList();
        }

        public async Task<EcPositionDto> CreatePositionAsync(SaveEcPositionRequest request, CancellationToken cancellationToken = default)
        {
            var name = CleanPositionName(request.Name);
            var key = PositionKeyFrom(name);
            if (key.Length == 0)
            {
                throw new InvalidOperationException("The name needs at least one letter or digit.");
            }
            var lowerKey = key.ToLower();
            var lowerName = name.ToLower();
            if (await _db.EcPositions.AnyAsync(p => p.Key.ToLower() == lowerKey || p.Name.ToLower() == lowerName, cancellationToken))
            {
                throw new InvalidOperationException($"A position called {name} already exists.");
            }

            var position = new EcPosition { Key = key, IsActive = true };
            ApplyPosition(position, name, request);
            await ClearOtherSignatoriesAsync(position, cancellationToken);
            _db.EcPositions.Add(position);
            await _db.SaveChangesAsync(cancellationToken);
            return MapToPositionDto(position);
        }

        public async Task<EcPositionDto?> UpdatePositionAsync(int id, SaveEcPositionRequest request, CancellationToken cancellationToken = default)
        {
            var position = await _db.EcPositions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (position == null) return null;

            var name = CleanPositionName(request.Name);
            var lowerName = name.ToLower();
            if (await _db.EcPositions.AnyAsync(p => p.Id != id && p.Name.ToLower() == lowerName, cancellationToken))
            {
                throw new InvalidOperationException($"A position called {name} already exists.");
            }

            // Fewer seats than the people holding them, or than an open election is filling,
            // would hide holders on the governance page.
            var seats = Math.Clamp(request.SeatCount, 1, Constants.Governance.PositionMaxSeats);
            if (seats < position.SeatCount)
            {
                var periodHolders = await _db.ECMembers
                    .Where(em => em.Position == id && em.EndDate == null)
                    .GroupBy(em => em.ECPeriodId).Select(g => g.Count())
                    .ToListAsync(cancellationToken);
                var held = periodHolders.Count == 0 ? 0 : periodHolders.Max();
                if (held > seats)
                {
                    throw new InvalidOperationException($"{held} people hold {position.Name} now. End terms before cutting it to {seats} seat(s).");
                }
                if (await _db.ElectionSeats.AnyAsync(s => s.Position == id && s.SeatCount > seats
                        && s.Election!.Phase != ElectionPhase.Declared && s.Election.Phase != ElectionPhase.Archived, cancellationToken))
                {
                    throw new InvalidOperationException($"An election still running fills more than {seats} {position.Name} seat(s).");
                }
            }
            if (!request.SignsAsHead && position.SignsAsHead || !request.SignsAsSecretary && position.SignsAsSecretary)
            {
                throw new InvalidOperationException("Certificates need a signatory. Mark another position as signatory instead of clearing it here.");
            }

            ApplyPosition(position, name, request);
            await ClearOtherSignatoriesAsync(position, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return MapToPositionDto(position);
        }

        public async Task<IReadOnlyList<HandoverCheckDto>?> GetHandoverChecksAsync(int periodId, CancellationToken cancellationToken = default)
        {
            if (!await _db.ECPeriods.AnyAsync(p => p.Id == periodId, cancellationToken)) return null;
            return await _db.ECPeriodHandoverChecks.AsNoTracking().Where(c => c.ECPeriodId == periodId)
                .OrderBy(c => c.Item)
                .Select(c => new HandoverCheckDto(c.Item, c.IsChecked, c.SetByUserId, c.SetAt)).ToListAsync(cancellationToken);
        }

        public async Task<bool> SetHandoverCheckAsync(int periodId, SetHandoverCheckRequest request, int actorUserId, CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(request.Item)) throw new InvalidRequestException("Unknown handover item.");
            if (!await _db.ECPeriods.AnyAsync(p => p.Id == periodId, cancellationToken)) return false;

            var check = await _db.ECPeriodHandoverChecks.FirstOrDefaultAsync(c => c.ECPeriodId == periodId && c.Item == request.Item, cancellationToken);
            if (check == null)
            {
                check = new ECPeriodHandoverCheck { ECPeriodId = periodId, Item = request.Item };
                _db.ECPeriodHandoverChecks.Add(check);
            }
            check.IsChecked = request.IsChecked;
            check.SetByUserId = actorUserId;
            check.SetAt = DateTime.UtcNow;
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            // The unique index on (term, item) refused a second first-time tick saved at the same moment.
            catch (DbUpdateException)
            {
                _db.ChangeTracker.Clear();
                throw new InvalidOperationException("Another admin changed this item at the same time. Reload and try again.");
            }
            return true;
        }

        public async Task<bool> SetPositionActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default)
        {
            var position = await _db.EcPositions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (position == null) return false;
            if (!isActive && await _db.ECMembers.AnyAsync(em => em.Position == id && em.EndDate == null, cancellationToken))
            {
                throw new InvalidOperationException($"{position.Name} has a current holder. End their term before retiring it.");
            }
            if (!isActive && (position.SignsAsHead || position.SignsAsSecretary))
            {
                throw new InvalidOperationException($"{position.Name} signs certificates. Mark another position as signatory first.");
            }

            position.IsActive = isActive;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeletePositionAsync(int id, CancellationToken cancellationToken = default)
        {
            var position = await _db.EcPositions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (position == null) return false;
            // IgnoreQueryFilters so an archived term still counts as use. The FK would refuse it anyway.
            var used = await _db.ECMembers.IgnoreQueryFilters().AnyAsync(em => em.Position == id, cancellationToken)
                || await _db.ElectionSeats.IgnoreQueryFilters().AnyAsync(s => s.Position == id, cancellationToken);
            if (used)
            {
                throw new InvalidOperationException($"{position.Name} has past terms or election seats. Retire it instead.");
            }

            _db.EcPositions.Remove(position);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        // 95.2: rows left holding a position that matches no row stop the migration adding its key.
        // IgnoreQueryFilters so archived rows are listed too, since the key covers them as well.
        public async Task<IReadOnlyList<InvalidPositionRowDto>> GetInvalidPositionRowsAsync(CancellationToken cancellationToken = default)
        {
            var terms = await _db.ECMembers.IgnoreQueryFilters().AsNoTracking()
                .Where(em => !_db.EcPositions.Any(p => p.Id == em.Position))
                .Select(em => new InvalidPositionRowDto
                {
                    Kind = Constants.Governance.RepairKinds.CommitteeTerm,
                    Id = em.Id,
                    Position = em.Position,
                    Description = (em.Member != null ? em.Member.FullName : "Member " + em.MemberId)
                        + " in " + (em.ECPeriod != null ? em.ECPeriod.Title : "period " + em.ECPeriodId)
                })
                .ToListAsync(cancellationToken);
            var seats = await _db.ElectionSeats.IgnoreQueryFilters().AsNoTracking()
                .Where(s => !_db.EcPositions.Any(p => p.Id == s.Position))
                .Select(s => new InvalidPositionRowDto
                {
                    Kind = Constants.Governance.RepairKinds.ElectionSeat,
                    Id = s.Id,
                    Position = s.Position,
                    Description = s.Election != null ? s.Election.Title : "Election " + s.ElectionId
                })
                .ToListAsync(cancellationToken);
            return terms.Concat(seats).ToList();
        }

        public async Task<bool> RepairPositionAsync(RepairPositionRequest request, CancellationToken cancellationToken = default)
        {
            var position = await _db.EcPositions.FirstOrDefaultAsync(p => p.Id == request.PositionId, cancellationToken)
                ?? throw new InvalidOperationException("Pick a committee position.");
            if (request.Kind == Constants.Governance.RepairKinds.CommitteeTerm)
            {
                var term = await _db.ECMembers.IgnoreQueryFilters().FirstOrDefaultAsync(em => em.Id == request.Id, cancellationToken);
                if (term == null) return false;
                if (term.EndDate == null && await _db.ECMembers.CountAsync(em => em.Id != term.Id && em.ECPeriodId == term.ECPeriodId
                        && em.Position == position.Id && em.EndDate == null, cancellationToken) >= position.SeatCount)
                {
                    throw new InvalidOperationException($"Every {position.Name} seat in that period is held. End one term first.");
                }
                term.Position = position.Id;
            }
            else if (request.Kind == Constants.Governance.RepairKinds.ElectionSeat)
            {
                var seat = await _db.ElectionSeats.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
                if (seat == null) return false;
                if (await _db.ElectionSeats.AnyAsync(s => s.Id != seat.Id && s.ElectionId == seat.ElectionId && s.Position == position.Id, cancellationToken))
                {
                    throw new InvalidOperationException($"{position.Name} is already on that election.");
                }
                seat.Position = position.Id;
            }
            else
            {
                return false;
            }
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static string CleanPositionName(string? name)
        {
            var clean = (name ?? string.Empty).Trim();
            if (clean.Length == 0)
            {
                throw new InvalidOperationException("Give the position a name.");
            }
            if (clean.Length > Constants.Governance.PositionNameMaxLength)
            {
                throw new InvalidOperationException($"The name can be at most {Constants.Governance.PositionNameMaxLength} characters.");
            }
            return clean;
        }

        // "Joint Secretary 2" becomes "JointSecretary2", the same shape as the seeded keys.
        internal static string PositionKeyFrom(string name)
        {
            var key = new string(name.Where(char.IsAsciiLetterOrDigit).ToArray());
            return key.Length > Constants.Governance.PositionKeyMaxLength
                ? key[..Constants.Governance.PositionKeyMaxLength]
                : key;
        }

        private static void ApplyPosition(EcPosition position, string name, SaveEcPositionRequest request)
        {
            var nameBn = request.NameBn?.Trim();
            if (nameBn != null && nameBn.Length > Constants.Governance.PositionNameMaxLength)
            {
                throw new InvalidOperationException($"The Bengali name can be at most {Constants.Governance.PositionNameMaxLength} characters.");
            }
            position.Name = name;
            position.NameBn = string.IsNullOrEmpty(nameBn) ? null : nameBn;
            if (request.DisplayOrder < 0 || request.DisplayOrder > Constants.Governance.PositionMaxDisplayOrder)
            {
                throw new InvalidOperationException($"Display order must be 0 to {Constants.Governance.PositionMaxDisplayOrder}.");
            }
            position.DisplayOrder = request.DisplayOrder;
            position.SeatCount = Math.Clamp(request.SeatCount, 1, Constants.Governance.PositionMaxSeats);
            position.ShowOnBoard = request.ShowOnBoard;
            position.SignsAsHead = request.SignsAsHead;
            position.SignsAsSecretary = request.SignsAsSecretary;
        }

        // Forms print one head and one secretary, so setting a flag here takes it off the others.
        private async Task ClearOtherSignatoriesAsync(EcPosition position, CancellationToken cancellationToken)
        {
            if (!position.SignsAsHead && !position.SignsAsSecretary) return;
            var others = await _db.EcPositions
                .Where(p => p.Id != position.Id && ((position.SignsAsHead && p.SignsAsHead) || (position.SignsAsSecretary && p.SignsAsSecretary)))
                .ToListAsync(cancellationToken);
            foreach (var other in others)
            {
                if (position.SignsAsHead) other.SignsAsHead = false;
                if (position.SignsAsSecretary) other.SignsAsSecretary = false;
            }
        }

        private static EcPositionDto MapToPositionDto(EcPosition p) => new()
        {
            Id = p.Id,
            Key = p.Key,
            Name = p.Name,
            NameBn = p.NameBn,
            DisplayOrder = p.DisplayOrder,
            SeatCount = p.SeatCount,
            IsActive = p.IsActive,
            ShowOnBoard = p.ShowOnBoard,
            SignsAsHead = p.SignsAsHead,
            SignsAsSecretary = p.SignsAsSecretary
        };

        private static void EndTerm(ECMember ecMember, VacancyReason reason, string? note)
        {
            // System.Text.Json lets an unknown number such as 99 through to here.
            if (!Enum.IsDefined(reason))
            {
                throw new InvalidOperationException("Pick a reason the term ended.");
            }
            note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            if (reason == VacancyReason.Other && note == null)
            {
                throw new InvalidOperationException("Say what happened when the reason is Other.");
            }
            if (note != null && note.Length > Constants.Governance.VacancyNoteMaxLength)
            {
                throw new InvalidOperationException($"The note can be at most {Constants.Governance.VacancyNoteMaxLength} characters.");
            }

            ecMember.EndDate = DateTime.UtcNow;
            ecMember.EndReason = reason;
            ecMember.EndNote = note;
        }

        private static ECMemberDto MapToMemberDto(ECMember m) => new()
        {
            Id = m.Id,
            MemberId = m.MemberId,
            Position = m.EcPosition?.Key ?? string.Empty,
            PositionId = m.Position,
            PositionName = m.EcPosition?.Name ?? string.Empty,
            StartDate = m.StartDate,
            EndDate = m.EndDate,
            Member = m.Member == null ? null : new MemberSummaryDto
            {
                Id = m.Member.Id,
                FullName = m.Member.FullName,
                MembershipNumber = m.Member.MembershipNumber,
                PhotoPath = m.Member.PhotoPath,
                Status = m.Member.Status,
                IsVerified = m.Member.IsVerified
            }
        };
    }
}
