using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    public interface IIDCardService
    {
        Task<string> GenerateIDCardDataUriAsync(int memberId, CancellationToken cancellationToken = default);
        Task<string> GenerateCertificateDataUriAsync(int memberId, CancellationToken cancellationToken = default);

        Task<byte[]> GenerateIDCardPdfAsync(int memberId, CancellationToken cancellationToken = default);
        Task<byte[]> GenerateCertificatePdfAsync(int memberId, CancellationToken cancellationToken = default);
        Task<CredentialVerificationDto?> VerifyCredentialAsync(string shortCode, CancellationToken cancellationToken = default);
        Task<bool> RevokeCredentialAsync(string shortCode, string reason, CancellationToken cancellationToken = default);
    }
}
