using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces;

public interface IElectionPersonaService
{
    Task<IReadOnlyList<ElectionPersonaDto>> ListAsync(bool includeInactive, CancellationToken ct);
    Task<(bool Success, string? Error, ElectionPersonaDto? Persona)> CreateAsync(SaveElectionPersonaDto dto, CancellationToken ct);
    Task<(bool Success, string? Error, ElectionPersonaDto? Persona)> UpdateAsync(int id, SaveElectionPersonaDto dto, CancellationToken ct);
    Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct);
    Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken ct);
}
