using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.Interfaces;

// Spec 023 (37.12e). Which id a route carries when it is not the election id itself.
public enum ElectionIdLookup { Election, Nomination, Appointment, Approval }

// Spec 023 (37.12e). What a user may do on one election, from their role and live appointments.
public interface IElectionAccessService
{
    Task<ElectionPermission> GetPermissionsAsync(int electionId, int userId, IReadOnlyCollection<string> roles, CancellationToken ct = default);
    Task<bool> HasAsync(int electionId, int userId, IReadOnlyCollection<string> roles, ElectionPermission needed, CancellationToken ct = default);
    // True once a live appointment with a persona that takes over from admin exists on the election.
    Task<bool> IsHandedOverAsync(int electionId, CancellationToken ct = default);
    // Live officials on the election whose active persona grants Approve.
    Task<int> LiveApproverCountAsync(int electionId, CancellationToken ct = default);
    // Null when the nomination, appointment or approval does not exist.
    Task<int?> ElectionIdForAsync(ElectionIdLookup kind, int id, CancellationToken ct = default);
    // Elections where the user holds a live appointment.
    Task<IReadOnlyCollection<int>> ElectionIdsWithLiveAppointmentAsync(int userId, CancellationToken ct = default);
}
