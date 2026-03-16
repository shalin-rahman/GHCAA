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

        public async Task<IEnumerable<AlumniEvent>> GetActiveEventsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.AlumniEvents
                .Where(e => e.IsActive)
                .OrderBy(e => e.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<AlumniEvent>> GetAllEventsForAdminAsync(CancellationToken cancellationToken = default)
        {
            return await _context.AlumniEvents
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<AlumniEvent?> GetEventByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.AlumniEvents.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
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
            var alumniEvent = await _context.AlumniEvents.FirstOrDefaultAsync(e => e.Id == dto.EventId, cancellationToken);
            if (alumniEvent == null) throw new ArgumentException("Event not found");

            if (!alumniEvent.IsActive || (alumniEvent.RegistrationDeadline.HasValue && alumniEvent.RegistrationDeadline.Value < DateTime.UtcNow))
            {
                throw new InvalidOperationException("Registration is closed for this event.");
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
                PaymentReference = alumniEvent.RequiresPayment ? dto.PaymentReference : "FREE-ENTRY",
                PaymentMethod = alumniEvent.RequiresPayment ? dto.PaymentMethod : GHCAA.Domain.Enums.PaymentMethod.ManualReceipt,
                ReceiptPath = receiptPath,
                Status = EventRegistrationStatus.Pending,
                RegisteredAt = DateTime.UtcNow
            };

            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync(cancellationToken);
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
                    r.ReceiptPath
                })
            };
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

            if (approve && registration.Event != null)
            {
                var email = registration.Member?.Email ?? registration.GuestEmail;
                var name = registration.Member?.FullName ?? registration.GuestName ?? "Guest";

                if (!string.IsNullOrEmpty(email))
                {
                    // Send confirmation email
                    var customVars = new Dictionary<string, string>
                    {
                        { "EventTitle", registration.Event.Title },
                        { "EventDate", registration.Event.Date.ToString("f") },
                        { "EventLocation", registration.Event.Location },
                        { "FullName", name }
                    };

                    // Note: communicationService.SendIndividualEmailAsync usually expects memberId.
                    // If it's a non-member, we might need a direct email send method.
                    if (registration.MemberId.HasValue)
                    {
                        await _communicationService.SendIndividualEmailAsync(registration.MemberId.Value, "EVENT_REGISTRATION_CONFIRMATION", customVars, cancellationToken);
                    }
                    else
                    {
                        // TODO: Implement direct email for non-members if template supports it, 
                        // or just use basic EmailService for now.
                        // For now I'll assume we can use the member-less overload if I add it.
                    }
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
    }
}
