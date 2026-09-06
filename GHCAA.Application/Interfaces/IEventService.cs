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
        Task<IEnumerable<EventDto>> GetActiveEventsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<EventDto>> GetAllEventsForAdminAsync(CancellationToken cancellationToken = default);
        Task<EventDto?> GetEventByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<AlumniEvent> CreateEventAsync(CreateEventDto dto, CancellationToken cancellationToken = default);
        Task<AlumniEvent?> UpdateEventAsync(UpdateEventDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteEventAsync(int id, CancellationToken cancellationToken = default);
        Task<string> UpdateEventLogoAsync(int eventId, UploadedFileDto logo, CancellationToken cancellationToken = default);

        // Event Registration
        Task<EventRegistration> RegisterForEventAsync(RegisterForEventDto dto, int? memberId, UploadedFileDto? receipt = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<EventRegistration>> GetRegistrationsByMemberAsync(int memberId, CancellationToken cancellationToken = default);
        Task<object> GetAllRegistrationsForAdminAsync(int page = 1, int pageSize = 10, int? eventId = null, string? status = null, string? search = null, CancellationToken cancellationToken = default);
        Task<bool> ApproveRegistrationAsync(int registrationId, int adminId, bool approve, CancellationToken cancellationToken = default);
        Task<EventRegistration?> GetRegistrationByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> SendInvitationEmailAsync(int registrationId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PublicParticipantDto>> GetPublicParticipantsAsync(int eventId, CancellationToken cancellationToken = default);

        // Event Operations (Admin)
        Task<IEnumerable<EventTaskDto>> GetEventTasksAsync(int eventId, CancellationToken cancellationToken = default);
        Task<EventTask> CreateEventTaskAsync(CreateEventTaskDto dto, CancellationToken cancellationToken = default);
        Task<bool> ToggleTaskStatusAsync(int taskId, CancellationToken cancellationToken = default);
        Task<bool> DeleteTaskAsync(int taskId, CancellationToken cancellationToken = default);

        Task<EventBudgetDto?> GetEventBudgetAsync(int eventId, CancellationToken cancellationToken = default);
        Task<bool> UpdateEventBudgetAsync(UpdateEventBudgetDto dto, CancellationToken cancellationToken = default);
        Task<EventExpense> AddEventExpenseAsync(AddEventExpenseDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteExpenseAsync(int expenseId, CancellationToken cancellationToken = default);

        // Check-in & QR
        Task<bool> CheckInParticipantAsync(int registrationId, CancellationToken cancellationToken = default);
        Task<bool> CheckInByTicketCodeAsync(string ticketCode, CancellationToken cancellationToken = default);

        // GatewaysController: reads used to validate/initiate a payment against a registration
        // (by its PaymentReference) and to auto-approve it once the gateway confirms payment. The
        // auto-approve path deliberately does not send the participation-approved notification
        // that ApproveRegistrationAsync sends for an admin-initiated approval — this is a payment
        // confirmation, not an admin review, so it keeps the narrower behavior the controller had.
        Task<EventRegistration?> GetRegistrationByPaymentReferenceAsync(string paymentReference, CancellationToken cancellationToken = default);
        Task AutoApproveRegistrationAfterPaymentAsync(int registrationId, int adminId, CancellationToken cancellationToken = default);
    }
}
