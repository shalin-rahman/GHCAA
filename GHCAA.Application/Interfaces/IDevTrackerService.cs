using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    public interface IDevTrackerService
    {
        // Reads docs/TODO.md fresh on every call. This is an internal SuperAdmin screen, not a
        // hot path, so there's no cache worth invalidating when the file changes.
        Task<List<DevTrackerItemDto>> GetOpenItemsAsync(DevTrackerFilterDto filter, CancellationToken cancellationToken = default);
    }
}
