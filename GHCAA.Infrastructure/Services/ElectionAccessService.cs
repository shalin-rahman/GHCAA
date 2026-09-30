using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services;

// Spec 023 (37.12e). No cache: each check is two small queries, and a revoke must take effect at once.
public sealed class ElectionAccessService(ApplicationDbContext db, IOrgConfigService orgConfig) : IElectionAccessService
{
    private static readonly ElectionPermission All =
        Enum.GetValues<ElectionPermission>().Aggregate(ElectionPermission.None, (acc, p) => acc | p);

    // Appeals go to a body independent of the administration, so Admin never decides them.
    private static readonly ElectionPermission AdminDefault = All & ~ElectionPermission.DecideAppeals;

    public async Task<ElectionPermission> GetPermissionsAsync(int electionId, int userId, IReadOnlyCollection<string> roles, CancellationToken ct = default)
    {
        if (roles.Contains(Constants.Roles.SuperAdmin))
            return All;

        var live = await db.ElectionAppointments.Where(ElectionAppointment.LiveAt(DateTime.UtcNow))
            .Where(x => x.ElectionId == electionId)
            .Select(x => new { x.UserId, x.Persona!.Permissions, x.Persona.TakesOverFromAdmin })
            .ToListAsync(ct);

        var granted = live.Where(x => x.UserId == userId)
            .Aggregate(ElectionPermission.None, (acc, x) => acc | x.Permissions);

        if (!roles.Contains(Constants.Roles.Admin))
            return granted;

        if (live.Any(x => x.TakesOverFromAdmin) && !(await orgConfig.GetConfigAsync()).Elections.AdminKeepsControlAfterHandover)
            return granted;

        return granted | AdminDefault;
    }

    public async Task<bool> HasAsync(int electionId, int userId, IReadOnlyCollection<string> roles, ElectionPermission needed, CancellationToken ct = default) =>
        (await GetPermissionsAsync(electionId, userId, roles, ct) & needed) == needed;

    public Task<bool> IsHandedOverAsync(int electionId, CancellationToken ct = default) =>
        db.ElectionAppointments.Where(ElectionAppointment.LiveAt(DateTime.UtcNow))
            .AnyAsync(x => x.ElectionId == electionId && x.Persona!.TakesOverFromAdmin, ct);

    public async Task<int?> ElectionIdForAsync(ElectionIdLookup kind, int id, CancellationToken ct = default) => kind switch
    {
        ElectionIdLookup.Nomination => await db.Nominations.Where(x => x.Id == id).Select(x => (int?)x.ElectionId).FirstOrDefaultAsync(ct),
        ElectionIdLookup.Appointment => await db.ElectionAppointments.Where(x => x.Id == id).Select(x => (int?)x.ElectionId).FirstOrDefaultAsync(ct),
        ElectionIdLookup.Approval => await db.ElectionApprovals.Where(x => x.Id == id).Select(x => (int?)x.ElectionId).FirstOrDefaultAsync(ct),
        _ => id,
    };

    public async Task<IReadOnlyCollection<int>> ElectionIdsWithLiveAppointmentAsync(int userId, CancellationToken ct = default) =>
        await db.ElectionAppointments.Where(ElectionAppointment.LiveAt(DateTime.UtcNow))
            .Where(x => x.UserId == userId)
            .Select(x => x.ElectionId).Distinct().ToListAsync(ct);
}
