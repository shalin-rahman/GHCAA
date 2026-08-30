using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GHCAA.Domain;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services
{
    public class EventService : IEventService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICommunicationService _communicationService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IGamificationService _gamificationService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<EventService> _logger;

        public EventService(ApplicationDbContext context, ICommunicationService communicationService, IFileStorageService fileStorageService, IGamificationService gamificationService, INotificationService notificationService, ILogger<EventService> logger)
        {
            _context = context;
            _communicationService = communicationService;
            _fileStorageService = fileStorageService;
            _gamificationService = gamificationService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<IEnumerable<EventDto>> GetActiveEventsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.AlumniEvents
                .Where(e => e.IsActive)
                .OrderBy(e => e.StartDate)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Location = e.Location,
                    RegistrationFee = e.RegistrationFee,
                    RequiresPayment = e.RequiresPayment,
                    IsActive = e.IsActive,
                    ImageUrl = e.ImageUrl,
                    RegistrationStartDate = e.RegistrationStartDate,
                    RegistrationEndDate = e.RegistrationEndDate,
                    AdminNote = null, // Secure: Do not leak AdminNote in public listing
                    ParticipantCount = _context.EventRegistrations.Count(r => r.EventId == e.Id && r.Status != EventRegistrationStatus.Rejected),
                    RequiresRegistration = e.RequiresRegistration
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<EventDto>> GetAllEventsForAdminAsync(CancellationToken cancellationToken = default)
        {
            // Admin must see every event regardless of publish state (IsActive) so it can be
            // re-published later. AlumniEventConfiguration applies a global HasQueryFilter(e =>
            // e.IsActive) for all public/portal reads (see GetActiveEventsAsync/GetEventByIdAsync,
            // which rely on it); this admin-only path explicitly bypasses that filter.
            return await _context.AlumniEvents
                .IgnoreQueryFilters()
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Location = e.Location,
                    RegistrationFee = e.RegistrationFee,
                    RequiresPayment = e.RequiresPayment,
                    IsActive = e.IsActive,
                    ImageUrl = e.ImageUrl,
                    RegistrationStartDate = e.RegistrationStartDate,
                    RegistrationEndDate = e.RegistrationEndDate,
                    AdminNote = e.AdminNote,
                    ParticipantCount = _context.EventRegistrations.Count(r => r.EventId == e.Id && r.Status != EventRegistrationStatus.Rejected),
                    RequiresRegistration = e.RequiresRegistration
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<EventDto?> GetEventByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var e = await _context.AlumniEvents.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
            if (e == null) return null;

            return new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Location = e.Location,
                RegistrationFee = e.RegistrationFee,
                RequiresPayment = e.RequiresPayment,
                IsActive = e.IsActive,
                ImageUrl = e.ImageUrl,
                RegistrationStartDate = e.RegistrationStartDate,
                RegistrationEndDate = e.RegistrationEndDate,
                AdminNote = null, // Secure: Do not leak AdminNote in public detail view
                ParticipantCount = _context.EventRegistrations.Count(r => r.EventId == e.Id && r.Status != EventRegistrationStatus.Rejected),
                RequiresRegistration = e.RequiresRegistration
            };
        }

        public async Task<AlumniEvent> CreateEventAsync(CreateEventDto dto, CancellationToken cancellationToken = default)
        {
            var alumniEvent = new AlumniEvent
            {
                Title = dto.Title,
                Description = dto.Description,
                StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc),
                Location = dto.Location,
                RegistrationFee = dto.RegistrationFee,
                RequiresPayment = dto.RequiresPayment,
                IsActive = dto.IsActive,
                ImageUrl = dto.ImageUrl,
                RegistrationStartDate = dto.RegistrationStartDate.HasValue
                    ? DateTime.SpecifyKind(dto.RegistrationStartDate.Value, DateTimeKind.Utc)
                    : null,
                RegistrationEndDate = dto.RegistrationEndDate.HasValue
                    ? DateTime.SpecifyKind(dto.RegistrationEndDate.Value, DateTimeKind.Utc)
                    : null,
                AllowNonMembers = dto.AllowNonMembers,
                AdminNote = dto.AdminNote,
                RequiresRegistration = dto.RequiresRegistration,
                CreatedAt = DateTime.UtcNow
            };

            alumniEvent.ParticipantLimit = dto.ParticipantLimit;
            alumniEvent.HasWaitlist = dto.HasWaitlist;

            _context.AlumniEvents.Add(alumniEvent);
            await _context.SaveChangesAsync(cancellationToken);

            if (alumniEvent.IsActive)
            {
                await _notificationService.BroadcastNotificationAsync(
                    "New Event Created!",
                    $"Registration is now open for: {alumniEvent.Title}. Join us at {alumniEvent.Location} on {alumniEvent.StartDate:dd MMM}.",
                    Enums.NotificationType.EventCreation,
                    $"/portal/events/{alumniEvent.Id}",
                    cancellationToken
                );
            }

            return alumniEvent;
        }

        public async Task<AlumniEvent?> UpdateEventAsync(UpdateEventDto dto, CancellationToken cancellationToken = default)
        {
            // IgnoreQueryFilters: without this, an already-unpublished (IsActive == false) event
            // would be invisible to the global query filter and could never be found/re-published.
            var alumniEvent = await _context.AlumniEvents.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == dto.Id, cancellationToken);
            if (alumniEvent == null) return null;

            alumniEvent.Title = dto.Title;
            alumniEvent.Description = dto.Description;
            alumniEvent.StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);
            alumniEvent.EndDate = DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc);
            alumniEvent.Location = dto.Location;
            alumniEvent.RegistrationFee = dto.RegistrationFee;
            alumniEvent.RequiresPayment = dto.RequiresPayment;
            alumniEvent.IsActive = dto.IsActive;
            alumniEvent.ImageUrl = dto.ImageUrl;
            alumniEvent.RegistrationStartDate = dto.RegistrationStartDate.HasValue
                ? DateTime.SpecifyKind(dto.RegistrationStartDate.Value, DateTimeKind.Utc)
                : null;
            alumniEvent.RegistrationEndDate = dto.RegistrationEndDate.HasValue
                ? DateTime.SpecifyKind(dto.RegistrationEndDate.Value, DateTimeKind.Utc)
                : null;
            alumniEvent.AllowNonMembers = dto.AllowNonMembers;
            alumniEvent.AdminNote = dto.AdminNote;
            alumniEvent.ParticipantLimit = dto.ParticipantLimit;
            alumniEvent.HasWaitlist = dto.HasWaitlist;
            alumniEvent.RequiresRegistration = dto.RequiresRegistration;

            await _context.SaveChangesAsync(cancellationToken);
            return alumniEvent;
        }

        public async Task<bool> DeleteEventAsync(int id, CancellationToken cancellationToken = default)
        {
            // Admin must be able to delete an event it has unpublished; bypass the IsActive filter.
            var alumniEvent = await _context.AlumniEvents.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
            if (alumniEvent == null) return false;

            _context.AlumniEvents.Remove(alumniEvent);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<EventRegistration> RegisterForEventAsync(RegisterForEventDto dto, int? memberId, UploadedFileDto? receipt = null, CancellationToken cancellationToken = default)
        {
            var alumniEvent = await _context.AlumniEvents.FindAsync(new object[] { dto.EventId }, cancellationToken);
            if (alumniEvent == null) throw new ArgumentException("Event not found");

            if (!alumniEvent.IsActive)
                throw new InvalidOperationException("This event is not currently active.");

            if (!alumniEvent.RequiresRegistration)
                throw new InvalidOperationException("This event does not require registration.");

            var now = DateTime.UtcNow;
            if (alumniEvent.RegistrationStartDate.HasValue && now < alumniEvent.RegistrationStartDate.Value)
                throw new InvalidOperationException("Registration for this event has not opened yet.");

            if (alumniEvent.RegistrationEndDate.HasValue && now > alumniEvent.RegistrationEndDate.Value)
                throw new InvalidOperationException("Registration for this event is closed.");

            // 54.6: RegistrationEndDate is optional — an admin who never set one previously had no
            // gate at all once the event itself had already ended. The event's own EndDate is the
            // hard backstop regardless of whether a registration deadline was configured.
            if (now > alumniEvent.EndDate)
                throw new InvalidOperationException("This event has already ended.");

            // Check for existing registration for this event
            bool alreadyRegistered = false;
            if (memberId.HasValue)
            {
                alreadyRegistered = await _context.EventRegistrations.AnyAsync(r => r.EventId == dto.EventId && r.MemberId == memberId.Value && r.Status != EventRegistrationStatus.Rejected, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(dto.GuestEmail))
            {
                alreadyRegistered = await _context.EventRegistrations.AnyAsync(r => r.EventId == dto.EventId && r.GuestEmail == dto.GuestEmail && r.Status != EventRegistrationStatus.Rejected, cancellationToken);
            }

            if (alreadyRegistered)
            {
                throw new InvalidOperationException("You are already registered for this event.");
            }

            if (!memberId.HasValue && !alumniEvent.AllowNonMembers)
            {
                throw new InvalidOperationException("This event is for members only.");
            }

            string? receiptPath = null;
            if (receipt != null)
            {
                // Use a dummy ID for non-members in path or separate folder
                int targetId = memberId ?? 0;
                receiptPath = await _fileStorageService.SaveFileAsync(receipt.Content, receipt.FileName, targetId, FileUploadType.PaymentProof, cancellationToken);
            }

            var initialStatus = EventRegistrationStatus.Pending;
            if (alumniEvent.ParticipantLimit.HasValue)
            {
                // 29A.4: Count every registration that occupies a slot — both Pending (awaiting
                // payment/approval) and Approved. Counting only Approved (the old behavior) let an
                // unlimited number of Pending registrations pile up and overfill the event once they
                // were approved. Waitlisted/Rejected registrations do not consume a slot.
                var occupiedCount = await _context.EventRegistrations
                    .CountAsync(r => r.EventId == dto.EventId &&
                        (r.Status == EventRegistrationStatus.Pending || r.Status == EventRegistrationStatus.Approved),
                        cancellationToken);
                if (occupiedCount >= alumniEvent.ParticipantLimit.Value)
                {
                    // The old code only handled HasWaitlist; when HasWaitlist was false the cap was
                    // skipped entirely, allowing unlimited registrations. Reject instead.
                    if (!alumniEvent.HasWaitlist)
                        throw new InvalidOperationException("This event has reached its participant limit.");
                    initialStatus = EventRegistrationStatus.Waitlisted;
                }
            }

            var registration = new EventRegistration
            {
                EventId = dto.EventId,
                MemberId = memberId,
                IsNonMember = !memberId.HasValue,
                GuestName = dto.GuestName,
                GuestEmail = dto.GuestEmail,
                GuestMobile = dto.GuestMobile,
                PaymentReference = alumniEvent.RequiresPayment ? (dto.PaymentReference ?? "PENDING") : "FREE-ENTRY",
                PaymentMethod = alumniEvent.RequiresPayment
                    ? (dto.PaymentMethod ?? GHCAA.Domain.Enums.PaymentMethod.ManualReceipt)
                    : GHCAA.Domain.Enums.PaymentMethod.ManualReceipt,
                ReceiptPath = receiptPath,
                ContributionAmount = dto.ContributionAmount,
                TicketCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                Status = initialStatus,
                RegisteredAt = DateTime.UtcNow
            };

            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync(cancellationToken);

            // 29A.4: Close the overfill race. Two concurrent registrations can both pass the
            // pre-insert capacity check above. After persisting, re-count the slot-consuming
            // registrations at or before this one's Id; if this registration pushed the event past
            // its limit, demote it to the waitlist (or reject and roll it back when no waitlist is
            // configured). Ordering by Id makes the outcome deterministic — only the latest inserts
            // that crossed the line are demoted, so the cap is never exceeded under concurrency.
            if (alumniEvent.ParticipantLimit.HasValue && registration.Status != EventRegistrationStatus.Waitlisted)
            {
                var slotOrdinal = await _context.EventRegistrations
                    .CountAsync(r => r.EventId == dto.EventId && r.Id <= registration.Id &&
                        (r.Status == EventRegistrationStatus.Pending || r.Status == EventRegistrationStatus.Approved),
                        cancellationToken);
                if (slotOrdinal > alumniEvent.ParticipantLimit.Value)
                {
                    if (alumniEvent.HasWaitlist)
                    {
                        registration.Status = EventRegistrationStatus.Waitlisted;
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        _context.EventRegistrations.Remove(registration);
                        await _context.SaveChangesAsync(cancellationToken);
                        throw new InvalidOperationException("This event has reached its participant limit.");
                    }
                }
            }

            // Send "Participation Received" email
            try
            {
                await SendEventEmailAsync(registration, "EVENT_PARTICIPATION_RECEIVED", cancellationToken);
            }
            catch (Exception ex)
            {
                // Don't fail the registration over a notification failure, but a permanently
                // broken mail path (bad SMTP config, template error) is otherwise undetectable.
                _logger.LogError(ex, "Failed to send EVENT_PARTICIPATION_RECEIVED email for registration {RegistrationId}", registration.Id);
            }

            return registration;
        }

        public async Task<IEnumerable<EventRegistration>> GetRegistrationsByMemberAsync(int memberId, CancellationToken cancellationToken = default)
        {
            return await _context.EventRegistrations
                .Include(r => r.Event)
                .Where(r => r.MemberId == memberId)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<object> GetAllRegistrationsForAdminAsync(int page = 1, int pageSize = 10, int? eventId = null, string? status = null, string? search = null, CancellationToken cancellationToken = default)
        {
            var query = _context.EventRegistrations
                .Include(r => r.Event)
                .Include(r => r.Member)
                .AsQueryable();

            if (eventId.HasValue)
                query = query.Where(r => r.EventId == eventId.Value);

            if (!string.IsNullOrEmpty(status) && status != "all")
                if (Enum.TryParse<EventRegistrationStatus>(status, true, out var statusEnum))
                    query = query.Where(r => r.Status == statusEnum);

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLower();
                query = query.Where(r =>
                    (r.GuestName != null && r.GuestName.ToLower().Contains(s)) ||
                    (r.GuestEmail != null && r.GuestEmail.ToLower().Contains(s)) ||
                    (r.Member != null && r.Member.FullName.ToLower().Contains(s)) ||
                    (r.PaymentReference != null && r.PaymentReference.ToLower().Contains(s)));
            }

            var totalItems = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(r => r.RegisteredAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new
            {
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Items = items.Select(r => new
                {
                    r.Id,
                    r.EventId,
                    EventTitle = r.Event?.Title,
                    r.MemberId,
                    MemberName = r.Member?.FullName,
                    r.IsNonMember,
                    r.GuestName,
                    r.GuestEmail,
                    r.GuestMobile,
                    r.PaymentReference,
                    r.PaymentMethod,
                    r.Status,
                    r.RegisteredAt,
                    r.ApprovedAt,
                    r.ReceiptPath,
                    r.ContributionAmount,
                    EventFee = r.Event != null ? r.Event.RegistrationFee : 0
                })
            };
        }

        public async Task<bool> SendInvitationEmailAsync(int registrationId, CancellationToken cancellationToken = default)
        {
            var registration = await _context.EventRegistrations
                .Include(r => r.Event)
                .Include(r => r.Member)
                .FirstOrDefaultAsync(r => r.Id == registrationId, cancellationToken);

            if (registration == null || registration.Status != EventRegistrationStatus.Approved) return false;

            return await SendEventEmailAsync(registration, "EVENT_PARTICIPATION_APPROVED", cancellationToken);
        }

        private async Task<bool> SendEventEmailAsync(EventRegistration registration, string templateCode, CancellationToken cancellationToken)
        {
            if (registration.Event == null) return false;

            var email = registration.Member?.Email ?? registration.GuestEmail;
            var name = registration.Member?.FullName ?? registration.GuestName ?? "Guest";

            if (string.IsNullOrEmpty(email)) return false;

            var customVars = new Dictionary<string, string>
            {
                { "EventTitle", registration.Event.Title },
                { "EventDate", registration.Event.StartDate.ToString("f") },
                { "EventLocation", registration.Event.Location },
                { "FullName", name },
                { "Status", registration.Status.ToString() },
                { "PassId", $"REG-{registration.Id.ToString().PadLeft(6, '0')}" }
            };

            string subject = templateCode.Contains("RECEIVED")
                ? $"Participation Received: {registration.Event.Title}"
                : $"Participation Approved: {registration.Event.Title}";

            if (registration.MemberId.HasValue)
            {
                await _communicationService.SendIndividualEmailAsync(registration.MemberId.Value, templateCode, customVars, cancellationToken);
            }
            else
            {
                await _communicationService.SendCustomEmailAsync(
                    new List<string> { email },
                    templateCode,
                    subject,
                    null,
                    customVars,
                    cancellationToken);
            }

            return true;
        }

        public async Task<bool> ApproveRegistrationAsync(int registrationId, int adminId, bool approve, CancellationToken cancellationToken = default)
        {
            var registration = await _context.EventRegistrations
                .Include(r => r.Event)
                .Include(r => r.Member)
                .FirstOrDefaultAsync(r => r.Id == registrationId, cancellationToken);

            if (registration == null) return false;

            registration.Status = approve ? EventRegistrationStatus.Approved : EventRegistrationStatus.Rejected;
            registration.ApprovedAt = DateTime.UtcNow;
            registration.ApprovedByAdminId = adminId;

            await _context.SaveChangesAsync(cancellationToken);

            if (approve)
            {
                await SendEventEmailAsync(registration, "EVENT_PARTICIPATION_APPROVED", cancellationToken);

                if (registration.MemberId.HasValue)
                {
                    await _notificationService.CreateNotificationAsync(
                        registration.MemberId.Value,
                        "Participation Approved!",
                        $"Your request to join '{registration.Event?.Title}' has been approved. See you there!",
                        Enums.NotificationType.ParticipationApproval,
                        $"/portal/events/{registration.EventId}",
                        cancellationToken);
                }
            }
            return true;
        }

        public async Task<EventRegistration?> GetRegistrationByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.EventRegistrations
                .Include(r => r.Event)
                .Include(r => r.Member)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }
        public async Task<string> UpdateEventLogoAsync(int eventId, UploadedFileDto logo, CancellationToken cancellationToken = default)
        {
            // FindAsync honors the global IsActive query filter, so an unpublished event would
            // otherwise be reported as "not found" here. Use an explicit lookup that bypasses it.
            var alumniEvent = await _context.AlumniEvents.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);
            if (alumniEvent == null) throw new ArgumentException("Event not found");

            // Save the file. Use eventId for organization.
            string logoPath = await _fileStorageService.SaveFileAsync(logo.Content, logo.FileName, eventId, FileUploadType.NewsImage, cancellationToken); // Reusing NewsImage type as it's generic public image

            alumniEvent.ImageUrl = logoPath;
            await _context.SaveChangesAsync(cancellationToken);

            return logoPath;
        }

        public async Task<IEnumerable<PublicParticipantDto>> GetPublicParticipantsAsync(int eventId, CancellationToken cancellationToken = default)
        {
            return await _context.EventRegistrations
                .Include(r => r.Member)
                .Where(r => r.EventId == eventId)
                .OrderBy(r => r.RegisteredAt)
                .Select(r => new PublicParticipantDto
                {
                    Name = r.IsNonMember ? r.GuestName! : r.Member!.FullName,
                    Status = r.Status.ToString(),
                    RegisteredAt = r.RegisteredAt
                })
                .ToListAsync(cancellationToken);
        }

        // --- Event Operations (Admin) ---

        public async Task<IEnumerable<EventTaskDto>> GetEventTasksAsync(int eventId, CancellationToken cancellationToken = default)
        {
            return await _context.EventTasks
                .Include(t => t.AssignedMember)
                .Where(t => t.EventId == eventId)
                .OrderBy(t => t.DueDate)
                .Select(t => new EventTaskDto
                {
                    Id = t.Id,
                    EventId = t.EventId,
                    Title = t.Title,
                    Description = t.Description,
                    AssignedMemberId = t.AssignedMemberId,
                    AssignedMemberName = t.AssignedMember != null ? t.AssignedMember.FullName : null,
                    DueDate = t.DueDate,
                    IsCompleted = t.IsCompleted
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<EventTask> CreateEventTaskAsync(CreateEventTaskDto dto, CancellationToken cancellationToken = default)
        {
            var task = new EventTask
            {
                EventId = dto.EventId,
                Title = dto.Title,
                Description = dto.Description,
                AssignedMemberId = dto.AssignedMemberId,
                DueDate = dto.DueDate.HasValue ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc) : null,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.EventTasks.Add(task);
            await _context.SaveChangesAsync(cancellationToken);
            return task;
        }

        public async Task<bool> ToggleTaskStatusAsync(int taskId, CancellationToken cancellationToken = default)
        {
            var task = await _context.EventTasks.FindAsync(new object[] { taskId }, cancellationToken);
            if (task == null) return false;

            task.IsCompleted = !task.IsCompleted;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteTaskAsync(int taskId, CancellationToken cancellationToken = default)
        {
            var task = await _context.EventTasks.FindAsync(new object[] { taskId }, cancellationToken);
            if (task == null) return false;

            _context.EventTasks.Remove(task);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<EventBudgetDto?> GetEventBudgetAsync(int eventId, CancellationToken cancellationToken = default)
        {
            var budget = await _context.EventBudgets
                .Include(b => b.Expenses)
                .FirstOrDefaultAsync(b => b.EventId == eventId, cancellationToken);

            if (budget == null) return null;

            return new EventBudgetDto
            {
                Id = budget.Id,
                EventId = budget.EventId,
                EstimatedTotal = budget.EstimatedTotal,
                ActualTotal = budget.ActualTotal,
                Expenses = budget.Expenses.Select(e => new EventExpenseDto
                {
                    Id = e.Id,
                    Category = e.Category,
                    Amount = e.Amount,
                    Note = e.Note,
                    SpentAt = e.SpentAt
                }).OrderByDescending(e => e.SpentAt).ToList()
            };
        }

        public async Task<bool> UpdateEventBudgetAsync(UpdateEventBudgetDto dto, CancellationToken cancellationToken = default)
        {
            var budget = await _context.EventBudgets.FirstOrDefaultAsync(b => b.EventId == dto.EventId, cancellationToken);
            if (budget == null)
            {
                budget = new EventBudget
                {
                    EventId = dto.EventId,
                    EstimatedTotal = dto.EstimatedTotal,
                    CreatedAt = DateTime.UtcNow
                };
                _context.EventBudgets.Add(budget);
            }
            else
            {
                budget.EstimatedTotal = dto.EstimatedTotal;
                budget.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<EventExpense> AddEventExpenseAsync(AddEventExpenseDto dto, CancellationToken cancellationToken = default)
        {
            var budget = await _context.EventBudgets.FirstOrDefaultAsync(b => b.EventId == dto.EventId, cancellationToken);
            if (budget == null)
            {
                budget = new EventBudget
                {
                    EventId = dto.EventId,
                    EstimatedTotal = 0,
                    CreatedAt = DateTime.UtcNow
                };
                _context.EventBudgets.Add(budget);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var expense = new EventExpense
            {
                EventBudgetId = budget.Id,
                Category = dto.Category,
                Amount = dto.Amount,
                Note = dto.Note,
                SpentAt = DateTime.SpecifyKind(dto.SpentAt, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow
            };

            _context.EventExpenses.Add(expense);

            // Re-calculate actual total
            budget.ActualTotal += dto.Amount;
            budget.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return expense;
        }

        public async Task<bool> DeleteExpenseAsync(int expenseId, CancellationToken cancellationToken = default)
        {
            var expense = await _context.EventExpenses.FindAsync(new object[] { expenseId }, cancellationToken);
            if (expense == null) return false;

            var budget = await _context.EventBudgets.FindAsync(expense.EventBudgetId);
            if (budget != null)
            {
                budget.ActualTotal -= expense.Amount;
                budget.UpdatedAt = DateTime.UtcNow;
            }

            _context.EventExpenses.Remove(expense);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> CheckInParticipantAsync(int registrationId, CancellationToken cancellationToken = default)
        {
            var reg = await _context.EventRegistrations
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.Id == registrationId, cancellationToken);

            if (reg == null || reg.IsCheckedIn) return false;
            // 29A.5: Only approved participants may check in. Previously any status (Pending,
            // Rejected, Waitlisted) could check in and earn attendance points.
            if (reg.Status != EventRegistrationStatus.Approved) return false;

            reg.IsCheckedIn = true;
            reg.CheckedInAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Award Points if member
            if (reg.MemberId.HasValue)
            {
                await _gamificationService.AwardPointsAsync(reg.MemberId.Value, "EVENT_ATTENDANCE", reg.EventId, $"Attended: {reg.Event?.Title}", cancellationToken);
            }

            return true;
        }

        public async Task<bool> CheckInByTicketCodeAsync(string ticketCode, CancellationToken cancellationToken = default)
        {
            var reg = await _context.EventRegistrations
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.TicketCode == ticketCode, cancellationToken);

            if (reg == null || reg.IsCheckedIn) return false;
            // 29A.5: Only approved participants may check in (see CheckInParticipantAsync).
            if (reg.Status != EventRegistrationStatus.Approved) return false;

            reg.IsCheckedIn = true;
            reg.CheckedInAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Award Points if member
            if (reg.MemberId.HasValue)
            {
                await _gamificationService.AwardPointsAsync(reg.MemberId.Value, "EVENT_ATTENDANCE", reg.EventId, $"Attended: {reg.Event?.Title}", cancellationToken);
            }

            return true;
        }
    }
}
