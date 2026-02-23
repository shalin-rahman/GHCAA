using System.Threading;
using System.Threading.Tasks;

namespace GHCAA.Application.Interfaces
{
    public interface IIDCardService
    {
        Task<string> GenerateIDCardDataUriAsync(int memberId, CancellationToken cancellationToken = default);
        Task<string> GenerateCertificateDataUriAsync(int memberId, CancellationToken cancellationToken = default);
    }
}
