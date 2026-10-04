using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services;

public sealed class ElectionPersonaService(ApplicationDbContext db) : IElectionPersonaService
{
    public async Task<IReadOnlyList<ElectionPersonaDto>> ListAsync(bool includeInactive, CancellationToken ct)
    {
        var query = db.ElectionPersonas.AsQueryable();
        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        var rows = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync(ct);
        return rows.Select(ToDto).ToList();
    }

    public async Task<(bool Success, string? Error, ElectionPersonaDto? Persona)> CreateAsync(SaveElectionPersonaDto dto, CancellationToken ct)
    {
        if (!IsKnownGroup(dto.GroupName))
            return (false, "invalid-group", null);
        if (await NameTakenAsync(dto.Name, null, ct))
            return (false, "duplicate-name", null);

        var now = DateTime.UtcNow;
        var entity = new ElectionPersona
        {
            Name = dto.Name,
            GroupName = dto.GroupName,
            Description = dto.Description,
            Permissions = dto.Permissions,
            MinCount = dto.MinCount,
            MaxCount = dto.MaxCount,
            ShowOnPublicBoard = dto.ShowOnPublicBoard,
            TakesOverFromAdmin = dto.TakesOverFromAdmin,
            DeclarationText = dto.DeclarationText,
            SortOrder = dto.SortOrder,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.ElectionPersonas.Add(entity);
        await db.SaveChangesAsync(ct);
        return (true, null, ToDto(entity));
    }

    public async Task<(bool Success, string? Error, ElectionPersonaDto? Persona)> UpdateAsync(int id, SaveElectionPersonaDto dto, CancellationToken ct)
    {
        var entity = await db.ElectionPersonas.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
            return (false, "not-found", null);
        // A row made before the group list existed may hold another group. It can keep it, but
        // cannot be moved to a new one outside the list.
        if (dto.GroupName != entity.GroupName && !IsKnownGroup(dto.GroupName))
            return (false, "invalid-group", null);
        if (await NameTakenAsync(dto.Name, id, ct))
            return (false, "duplicate-name", null);

        entity.Name = dto.Name;
        entity.GroupName = dto.GroupName;
        entity.Description = dto.Description;
        entity.Permissions = dto.Permissions;
        entity.MinCount = dto.MinCount;
        entity.MaxCount = dto.MaxCount;
        entity.ShowOnPublicBoard = dto.ShowOnPublicBoard;
        entity.TakesOverFromAdmin = dto.TakesOverFromAdmin;
        entity.DeclarationText = dto.DeclarationText;
        entity.SortOrder = dto.SortOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return (true, null, ToDto(entity));
    }

    public async Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct)
    {
        var entity = await db.ElectionPersonas.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
            return false;

        entity.IsActive = isActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await db.ElectionPersonas.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
            return (false, "not-found");

        // Appointments keep their persona for the record, even revoked ones. Deactivate instead.
        if (await db.ElectionAppointments.AnyAsync(x => x.PersonaId == id, ct))
            return (false, "in-use");
        db.ElectionPersonas.Remove(entity);
        await db.SaveChangesAsync(ct);
        return (true, null);
    }

    private static bool IsKnownGroup(string groupName) =>
        Constants.Elections.PersonaGroups.All.Contains(groupName);

    // Name has a unique index. Checking first turns a clash into a 409 instead of a DbUpdateException.
    private Task<bool> NameTakenAsync(string name, int? exceptId, CancellationToken ct) =>
        db.ElectionPersonas.AnyAsync(x => x.Name == name && x.Id != exceptId, ct);

    private static ElectionPersonaDto ToDto(ElectionPersona p) => new(
        p.Id, p.Name, p.GroupName, p.Description, p.Permissions, p.MinCount, p.MaxCount,
        p.ShowOnPublicBoard, p.TakesOverFromAdmin, p.DeclarationText, p.SortOrder, p.IsActive);
}
