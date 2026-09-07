using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    public interface IErrorLogService
    {
        // Never throws — a logging failure must not replace or mask the original exception it's
        // trying to record. Callers (ExceptionMiddleware) fire this without awaiting its errors.
        Task LogAsync(string level, string message, string? exceptionType, string? stackTrace, string? source,
            string? requestPath, string? requestMethod, int? userId, string? username,
            CancellationToken cancellationToken = default);

        Task<PagedResult<ErrorLogDto>> GetPagedAsync(ErrorLogFilterDto filter, CancellationToken cancellationToken = default);
    }
}
