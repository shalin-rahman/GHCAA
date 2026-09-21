using System.Threading;
using System.Threading.Tasks;

namespace GHCAA.Application.Interfaces;

public interface IElectionDocumentService
{
    Task<byte[]?> GeneratePdfAsync(int electionId, string formCode, CancellationToken cancellationToken = default);
}
