using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    public interface IAssistantService
    {
        Task<AssistantResponseDto> AskAsync(string query, CancellationToken cancellationToken = default);
    }

    public class AssistantResponseDto
    {
        public string Answer { get; set; } = null!;
        public IEnumerable<MemberProfileDto>? FoundMembers { get; set; }
    }
}
