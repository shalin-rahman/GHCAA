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
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services
{
    public class EventService : IEventService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICommunicationService _communicationService;
        private readonly IFileStorageService _fileStorageService;

        public EventService(ApplicationDbContext context, ICommunicationService communicationService, IFileStorageService fileStorageService)
        {
            _context = context;
            _communicationService = communicationService;
            _fileStorageService = fileStorageService;
        }

        public async Task<IEnumerable<EventDto>> GetActiveEventsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.AlumniEvents
                .Where(e => e.IsActive)
                .OrderBy(e => e.Date)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Date = e.Date,
                    Location = e.Location,
                    RegistrationFee = e.RegistrationFee,
                    RequiresPayment = e.RequiresPayment,
                    IsActive = e.IsActive,
                    ImageUrl = e.ImageUrl,
                    RegistrationDeadline = e.RegistrationDeadline,
                    AdminNote = e.AdminNote,
                    ParticipantCount = _context.EventRegistrations.Count(r => r.EventId == e.Id && r.Status != EventRegistrationStatus.Rejected)
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<EventDto>> GetAllEventsForAdminAsync(CancellationToken cancellationToken = default)
        {
            return await _context.AlumniEvents
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Date = e.Date,
                    Location = e.Location,
                    RegistrationFee = e.RegistrationFee,
                    RequiresPayment = e.RequiresPayment,
                    IsActive = e.IsActive,
                    ImageUrl = e.ImageUrl,
                    RegistrationDeadline = e.RegistrationDeadline,
                    AdminNote = e.AdminNote,
                    ParticipantCount = _context.EventRegistrations.Count(r => r.EventId == e.Id && r.Status != EventRegistrationStatus.Rejected)
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
                Date = e.Date,
                Location = e.Location,
                RegistrationFee = e.RegistrationFee,
                RequiresPayment = e.RequiresPayment,
                IsActive = e.IsActive,
                ImageUrl = e.ImageUrl,
                RegistrationDeadline = e.RegistrationDeadline,
                AdminNote = e.AdminNote,
                ParticipantCount = _context.EventRegistrations.Count(r => r.EventId == e.Id && r.Status != EventRegistrationStatus.Rejected)
            };
        }

        public async Task<AlumniEvent> CreateEventAsync(CreateEventDto dto, CancellationToken cancellationToken = default)
        {
            var alumniEvent = new AlumniEvent
            {
                Title = dto.Title,
                Description = dto.Description,
                Date = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc),
                Location = dto.Location,
                RegistrationFee = dto.RegistrationFee,
                RequiresPayment = dto.RequiresPayment,
                IsActive = dto.IsActive,
                ImageUrl = dto.ImageUrl,
                RegistrationDeadline = dto.RegistrationDeadline.HasValue 
                    ? DateTime.SpecifyKind(dto.RegistrationDeadline.Value, DateTimeKind.Utc) 
                    : null,
                AllowNonMembers = dto.AllowNonMembers,
                AdminNote = dto.AdminNote,
                CreatedAt = DateTime.UtcNow
            };

            _context.AlumniEvents.Add(alumniEvent);
            await _context.SaveChangesAsync(cancellationToken);
            return alumniEvent;
        }

        public async Task<AlumniEvent?> UpdateEventAsync(UpdateEventDto dto, CancellationToken cancellationToken = default)
        {
            var alumniEvent = await _context.AlumniEvents.FirstOrDefaultAsync(e => e.Id == dto.Id, cancellationToken);
            if (alumniEvent == null) return null;

            alumniEvent.Title = dto.Title;
            alumniEvent.Description = dto.Description;
            alumniEvent.Date = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc);
            alumniEvent.Location = dto.Location;
            alumniEvent.RegistrationFee = dto.RegistrationFee;
            alumniEvent.RequiresPayment = dto.RequiresPayment;
            alumniEvent.IsActive = dto.IsActive;
            alumniEvent.ImageUrl = dto.ImageUrl;
            alumniEvent.RegistrationDeadline = dto.RegistrationDeadline.HasValue 
                ? DateTime.SpecifyKind(dto.RegistrationDeadline.Value, DateTimeKind.Utc) 
                : null;
            alumniEvent.AllowNonMembers = dto.AllowNonMembers;
            alumniEvent.AdminNote = dto.AdminNote;

            await _context.SaveChangesAsync(cancellationToken);
            return alumniEvent;
        }

        public async Task<bool> DeleteEventAsync(int id, CancellationToken cancellationToken = default)
        {
            var alumniEvent = await _context.AlumniEvents.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
            if (alumniEvent == null) return false;

            _context.AlumniEvents.Remove(alumniEvent);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<EventRegistration> RegisterForEventAsync(RegisterForEventDto dto, int? memberId, UploadedFileDto? receipt = null, CancellationToken cancellationToken = default)
        {
            var alumniEvent = await _context.AlumniEvents.FindAsync(new object[] { dto.EventId }, cancellationToken);
            if (alumniEvent == null) throw new ArgumentException("Event not found");

            if (!alumniEvent.IsActive || (alumniEvent.RegistrationDeadline.HasValue && alumniEvent.RegistrationDeadline.Value < DateTime.UtcNow))
            {
                throw new InvalidOperationException("Registration is closed for this event.");
            }

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
                Status = EventRegistrationStatus.Pending,
                RegisteredAt = DateTime.UtcNow
            };

            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync(cancellationToken);

            // Send "Participation Received" email
            try {
                await SendEventEmailAsync(registration, "EVENT_PARTICIPATION_RECEIVED", cancellationToken);
            } catch { /* Suppress email errors to ensure registration succeeds */ }

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
                Items = items.Select(r => new {
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
                { "EventDate", registration.Event.Date.ToString("f") },
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
            var alumniEvent = await _context.AlumniEvents.FindAsync(eventId);
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
    }
}
