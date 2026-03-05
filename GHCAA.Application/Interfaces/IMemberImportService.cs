using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    public interface IMemberImportService
    {
        Task<MemberImportResultDto> ImportMembersAsync(MemberImportRequestDto request, CancellationToken cancellationToken = default);
    }
}
