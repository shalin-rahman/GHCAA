using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface IEventService
    {
        // Event Management
        Task<IEnumerable<AlumniEvent>> GetActiveEventsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<AlumniEvent>> GetAllEventsForAdminAsync(CancellationToken cancellationToken = default);
        Task<AlumniEvent?> GetEventByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<AlumniEvent> CreateEventAsync(CreateEventDto dto, CancellationToken cancellationToken = default);
        Task<AlumniEvent?> UpdateEventAsync(UpdateEventDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteEventAsync(int id, CancellationToken cancellationToken = default);

        // Event Registration
        Task<EventRegistration> RegisterForEventAsync(RegisterForEventDto dto, int memberId, UploadedFileDto? receipt = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<EventRegistration>> GetRegistrationsByMemberAsync(int memberId, CancellationToken cancellationToken = default);
        Task<IEnumerable<EventRegistration>> GetAllRegistrationsForAdminAsync(CancellationToken cancellationToken = default);
        Task<bool> ApproveRegistrationAsync(int registrationId, int adminId, bool approve, CancellationToken cancellationToken = default);
    }
}
