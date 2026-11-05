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
        // Null when the member does not exist.
        Task<IReadOnlyList<IssuedCredentialDto>?> GetMemberCredentialsAsync(int memberId, CancellationToken cancellationToken = default);
        // Both return null when the short code is unknown. Reissue returns the new credential.
        Task<IssuedCredentialDto?> RevokeCredentialAsync(string shortCode, string reason, CancellationToken cancellationToken = default);
        Task<IssuedCredentialDto?> ReissueCredentialAsync(string shortCode, string reason, CancellationToken cancellationToken = default);
    }
}
