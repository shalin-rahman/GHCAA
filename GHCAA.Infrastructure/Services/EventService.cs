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
                Date = dto.Date,
                Location = dto.Location,
                RegistrationFee = dto.RegistrationFee,
                IsActive = dto.IsActive,
                ImageUrl = dto.ImageUrl,
                RegistrationDeadline = dto.RegistrationDeadline,
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
            alumniEvent.Date = dto.Date;
            alumniEvent.Location = dto.Location;
            alumniEvent.RegistrationFee = dto.RegistrationFee;
            alumniEvent.IsActive = dto.IsActive;
            alumniEvent.ImageUrl = dto.ImageUrl;
            alumniEvent.RegistrationDeadline = dto.RegistrationDeadline;
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

        public async Task<EventRegistration> RegisterForEventAsync(RegisterForEventDto dto, int memberId, UploadedFileDto? receipt = null, CancellationToken cancellationToken = default)
        {
            var alumniEvent = await _context.AlumniEvents.FirstOrDefaultAsync(e => e.Id == dto.EventId, cancellationToken);
            if (alumniEvent == null) throw new ArgumentException("Event not found");

            if (!alumniEvent.IsActive || (alumniEvent.RegistrationDeadline.HasValue && alumniEvent.RegistrationDeadline.Value < DateTime.UtcNow))
            {
                throw new InvalidOperationException("Registration is closed for this event.");
            }

            string? receiptPath = null;
            if (receipt != null)
            {
                receiptPath = await _fileStorageService.SaveFileAsync(receipt.Content, receipt.FileName, memberId, FileUploadType.PaymentProof, cancellationToken);
            }

            var registration = new EventRegistration
            {
                EventId = dto.EventId,
                MemberId = memberId,
                PaymentReference = dto.PaymentReference,
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

        public async Task<IEnumerable<EventRegistration>> GetAllRegistrationsForAdminAsync(CancellationToken cancellationToken = default)
        {
            return await _context.EventRegistrations
                .Include(r => r.Event)
                .Include(r => r.Member)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync(cancellationToken);
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

            if (approve && registration.Member != null && registration.Event != null)
            {
                // Send confirmation email
                var customVars = new Dictionary<string, string>
                {
                    { "EventTitle", registration.Event.Title },
                    { "EventDate", registration.Event.Date.ToString("f") },
                    { "EventLocation", registration.Event.Location },
                    { "FullName", registration.Member.FullName }
                };

                await _communicationService.SendIndividualEmailAsync(registration.MemberId, "EVENT_REGISTRATION_CONFIRMATION", customVars, cancellationToken);
            }

            return true;
        }
    }
}
