using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces;

// Spec 023 (37.12d). Errors come back as short codes the controller maps to a status.
public interface IElectionAppointmentService
{
    Task<(bool Success, string? Error, ElectionAppointmentDto? Appointment)> AppointAsync(int electionId, AppointDto dto, int actorUserId, CancellationToken ct);
    Task<(bool Success, string? Error)> AcceptAsync(int appointmentId, int userId, AcceptAppointmentDto dto, string? ip, CancellationToken ct);
    Task<(bool Success, string? Error)> DeclineAsync(int appointmentId, int userId, string? reason, CancellationToken ct);
    Task<(bool Success, string? Error)> RevokeAsync(int appointmentId, int actorUserId, string? reason, CancellationToken ct);
    Task EndAccessIfNoneLeftAsync(int userId, CancellationToken ct);
    // 37.12g. Revokes every open appointment on the election, then tidies each user's access.
    Task EndAllAsync(int electionId, string reason, CancellationToken ct);
    // Null when the actor may not see this election's appointments.
    Task<IReadOnlyList<ElectionAppointmentDto>?> ListAsync(int electionId, int actorUserId, CancellationToken ct);
    Task<IReadOnlyList<ElectionAppointmentDto>> ListMineAsync(int userId, CancellationToken ct);
    Task<IReadOnlyList<ElectionAppointmentSummaryDto>> ListLiveSummariesAsync(int userId, CancellationToken ct);
}
