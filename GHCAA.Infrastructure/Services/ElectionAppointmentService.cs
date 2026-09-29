using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services;

// Spec 023 (37.12d). Appointing, accepting and ending election appointments.
public sealed class ElectionAppointmentService(
    ApplicationDbContext db,
    IOrgConfigService orgConfig,
    ICommunicationService communication,
    INotificationService notifications,
    ITokenService tokens,
    IOptions<AppSettingsOptions> appSettings,
    ILogger<ElectionAppointmentService> logger) : IElectionAppointmentService
{
    public async Task<(bool Success, string? Error, ElectionAppointmentDto? Appointment)> AppointAsync(int electionId, AppointDto dto, int actorUserId, CancellationToken ct)
    {
        if (!await db.Elections.AnyAsync(x => x.Id == electionId, ct))
            return (false, "not-found", null);
        var persona = await db.ElectionPersonas.FirstOrDefaultAsync(x => x.Id == dto.PersonaId && x.IsActive, ct);
        if (persona is null)
            return (false, "persona-not-found", null);
        if (!await MayAppointAsync(electionId, actorUserId, ct))
            return (false, "forbidden", null);

        User? user;
        string displayName, email;
        string? phone;
        var createdUser = false;

        if (dto.MemberId is int memberId)
        {
            var member = await db.Members.FirstOrDefaultAsync(x => x.Id == memberId && x.Status == MembershipStatus.Active, ct);
            if (member is null)
                return (false, "member-not-found", null);
            user = await db.Users.FirstOrDefaultAsync(x => x.MemberId == member.Id, ct);
            if (user is null)
                return (false, "member-no-login", null);
            (displayName, email, phone) = (member.FullName, member.Email, member.MobileNo);
        }
        else
        {
            email = dto.Email!.Trim();
            var lower = email.ToLower();
            if (await db.Members.IgnoreQueryFilters().AnyAsync(x => x.Email.ToLower() == lower, ct))
                return (false, "email-is-member", null);

            user = await db.Users.FirstOrDefaultAsync(x => x.MemberId == null && x.Username.ToLower() == lower, ct);
            if (user is null)
            {
                user = new User
                {
                    Username = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))),
                    MustChangePassword = true,
                    IsActive = true
                };
                db.Users.Add(user);
                createdUser = true;
            }
            else
            {
                // A past revoke may have switched the login off.
                user.IsActive = true;
            }
            (displayName, phone) = (dto.DisplayName!.Trim(), dto.Phone?.Trim());
        }

        if (!createdUser && await db.ElectionAppointments.AnyAsync(x => x.ElectionId == electionId && x.PersonaId == persona.Id && x.UserId == user.Id && x.RevokedAt == null, ct))
            return (false, "duplicate", null);

        var appointment = new ElectionAppointment
        {
            ElectionId = electionId,
            PersonaId = persona.Id,
            User = user,
            MemberId = dto.MemberId,
            DisplayName = displayName,
            Email = email,
            Phone = phone,
            AppointedByUserId = actorUserId,
            AppointedAt = DateTime.UtcNow
        };
        db.ElectionAppointments.Add(appointment);
        await db.SaveChangesAsync(ct);

        await SendInviteAsync(appointment, persona, user, ct);

        return (true, null, (await LoadAsync(db.ElectionAppointments.Where(x => x.Id == appointment.Id), ct)).Single());
    }

    public async Task<(bool Success, string? Error)> AcceptAsync(int appointmentId, int userId, AcceptAppointmentDto dto, string? ip, CancellationToken ct)
    {
        var appointment = await db.ElectionAppointments.Include(x => x.Persona).Include(x => x.User!).ThenInclude(u => u.Roles)
            .FirstOrDefaultAsync(x => x.Id == appointmentId && x.UserId == userId && x.RevokedAt == null, ct);
        if (appointment is null)
            return (false, "not-found");
        if (appointment.AcceptedAt is not null)
            return (false, "already-accepted");
        if (appointment.ExpiresAt is DateTime expires && expires <= DateTime.UtcNow)
            return (false, "expired");
        if (!dto.AgreeToDeclaration)
            return (false, "declaration-required");

        var now = DateTime.UtcNow;
        appointment.AcceptedAt = now;
        appointment.DeclarationSignedAt = now;
        appointment.DeclarationTextSnapshot = appointment.Persona!.DeclarationText;
        appointment.SignedFromIp = ip;

        var user = appointment.User!;
        if (!user.Roles.Any(r => r.Name == Constants.Roles.ElectionOfficial))
        {
            // The role is seeded at boot, so a fresh database without the seeder run may lack it.
            var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == Constants.Roles.ElectionOfficial, ct)
                ?? db.Roles.Add(new Role { Name = Constants.Roles.ElectionOfficial }).Entity;
            user.Roles.Add(role);
        }
        user.SecurityStamp = Guid.NewGuid().ToString("N");

        await db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeclineAsync(int appointmentId, int userId, string? reason, CancellationToken ct)
    {
        var appointment = await db.ElectionAppointments.FirstOrDefaultAsync(x => x.Id == appointmentId && x.UserId == userId && x.RevokedAt == null, ct);
        if (appointment is null)
            return (false, "not-found");

        appointment.RevokedAt = DateTime.UtcNow;
        appointment.RevokedByUserId = userId;
        appointment.RevokedReason = string.IsNullOrWhiteSpace(reason) ? "Declined" : $"Declined: {reason.Trim()}";
        await db.SaveChangesAsync(ct);

        await EndAccessIfNoneLeftAsync(userId, ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RevokeAsync(int appointmentId, int actorUserId, string? reason, CancellationToken ct)
    {
        var appointment = await db.ElectionAppointments.FirstOrDefaultAsync(x => x.Id == appointmentId && x.RevokedAt == null, ct);
        if (appointment is null)
            return (false, "not-found");
        if (!await MayAppointAsync(appointment.ElectionId, actorUserId, ct))
            return (false, "forbidden");

        appointment.RevokedAt = DateTime.UtcNow;
        appointment.RevokedByUserId = actorUserId;
        appointment.RevokedReason = reason?.Trim();
        await db.SaveChangesAsync(ct);

        await EndAccessIfNoneLeftAsync(appointment.UserId, ct);
        return (true, null);
    }

    public async Task EndAccessIfNoneLeftAsync(int userId, CancellationToken ct)
    {
        if (await db.ElectionAppointments.Where(ElectionAppointment.LiveAt(DateTime.UtcNow)).AnyAsync(x => x.UserId == userId, ct))
            return;

        var user = await db.Users.Include(x => x.Roles).FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user is null)
            return;

        var role = user.Roles.FirstOrDefault(r => r.Name == Constants.Roles.ElectionOfficial);
        if (role is not null)
        {
            user.Roles.Remove(role);
            user.SecurityStamp = Guid.NewGuid().ToString("N");
        }
        // Someone let in only to run an election has nothing left to sign in for.
        if (user.MemberId is null && user.Roles.Count == 0)
            user.IsActive = false;

        await db.SaveChangesAsync(ct);
        if (role is not null)
            await tokens.RevokeAllRefreshTokensAsync(userId, ct);
    }

    public async Task<IReadOnlyList<ElectionAppointmentDto>?> ListAsync(int electionId, int actorUserId, CancellationToken ct)
    {
        if (!await IsAdminAsync(actorUserId, ct) && !await HasPermissionAsync(electionId, actorUserId, ElectionPermission.ViewDashboard, ct))
            return null;
        return await LoadAsync(db.ElectionAppointments.Where(x => x.ElectionId == electionId), ct);
    }

    public Task<IReadOnlyList<ElectionAppointmentDto>> ListMineAsync(int userId, CancellationToken ct) =>
        LoadAsync(db.ElectionAppointments.Where(x => x.UserId == userId && x.RevokedAt == null), ct);

    public async Task<IReadOnlyList<ElectionAppointmentSummaryDto>> ListLiveSummariesAsync(int userId, CancellationToken ct) =>
        await db.ElectionAppointments.Where(ElectionAppointment.LiveAt(DateTime.UtcNow)).Where(x => x.UserId == userId)
            .OrderBy(x => x.ElectionId)
            .Select(x => new ElectionAppointmentSummaryDto(x.ElectionId, x.Election!.Title, x.Persona!.Name, x.Persona.Permissions))
            .ToListAsync(ct);

    // Admin appoints until a persona that takes over from admin is live on the election.
    // After that only someone holding AppointOfficials there, or a SuperAdmin, may.
    private async Task<bool> MayAppointAsync(int electionId, int actorUserId, CancellationToken ct)
    {
        var roles = await RoleNamesAsync(actorUserId, ct);
        if (roles.Contains(Constants.Roles.SuperAdmin))
            return true;

        var takenOver = await db.ElectionAppointments.Where(ElectionAppointment.LiveAt(DateTime.UtcNow))
            .AnyAsync(x => x.ElectionId == electionId && x.Persona!.TakesOverFromAdmin, ct);
        if (!takenOver)
            return roles.Contains(Constants.Roles.Admin);

        return await HasPermissionAsync(electionId, actorUserId, ElectionPermission.AppointOfficials, ct);
    }

    private async Task<bool> IsAdminAsync(int userId, CancellationToken ct)
    {
        var roles = await RoleNamesAsync(userId, ct);
        return roles.Contains(Constants.Roles.SuperAdmin) || roles.Contains(Constants.Roles.Admin);
    }

    private async Task<List<string>> RoleNamesAsync(int userId, CancellationToken ct) =>
        await db.Users.Where(x => x.Id == userId).SelectMany(x => x.Roles.Select(r => r.Name)).ToListAsync(ct);

    private Task<bool> HasPermissionAsync(int electionId, int userId, ElectionPermission permission, CancellationToken ct) =>
        db.ElectionAppointments.Where(ElectionAppointment.LiveAt(DateTime.UtcNow))
            .AnyAsync(x => x.ElectionId == electionId && x.UserId == userId && (x.Persona!.Permissions & permission) != 0, ct);

    // A member hears through the portal. Anyone else gets the password-reset link so they can set a
    // password for the new login. The send is best effort; the appointment stands either way.
    private async Task SendInviteAsync(ElectionAppointment appointment, ElectionPersona persona, User user, CancellationToken ct)
    {
        try
        {
            if (appointment.MemberId is int memberId)
            {
                await notifications.CreateNotificationAsync(memberId, "Election appointment",
                    $"You have been appointed {persona.Name}. Open My appointments to accept or decline.",
                    NotificationType.GeneralSystem, AppointmentsUrl, ct);
                return;
            }

            var hours = (await orgConfig.GetConfigAsync()).Elections.InviteLinkHours;
            var token = Guid.NewGuid().ToString("N");
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(hours);
            await db.SaveChangesAsync(ct);

            var url = $"{appSettings.Value.ClientUrl}/reset-password?email={Uri.EscapeDataString(appointment.Email)}&token={token}&invite=1";
            await communication.SendEmailByCodeAsync(appointment.Email, Constants.TemplateCodes.PasswordReset,
                new Dictionary<string, string> { ["ResetUrl"] = url, ["FullName"] = appointment.DisplayName }, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Appointment {AppointmentId} saved but the invite could not be sent.", appointment.Id);
        }
    }

    private const string AppointmentsUrl = "/officials/my-appointments";

    private static async Task<IReadOnlyList<ElectionAppointmentDto>> LoadAsync(IQueryable<ElectionAppointment> query, CancellationToken ct)
    {
        var rows = await query.Include(x => x.Election).Include(x => x.Persona).OrderBy(x => x.AppointedAt).ToListAsync(ct);
        var now = DateTime.UtcNow;
        return rows.Select(x => new ElectionAppointmentDto(
            x.Id, x.ElectionId, x.Election!.Title, x.PersonaId, x.Persona!.Name, x.Persona.DeclarationText,
            x.UserId, x.MemberId, x.DisplayName, x.Email, x.Phone, x.AppointedAt, x.AcceptedAt,
            x.DeclarationSignedAt, x.RevokedAt, x.RevokedReason, x.ExpiresAt, x.IsLive(now))).ToList();
    }
}
