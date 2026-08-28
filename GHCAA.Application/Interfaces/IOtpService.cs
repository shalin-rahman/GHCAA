using static GHCAA.Domain.Enums;

namespace GHCAA.Application.Interfaces
{
    public interface IOtpService
    {
        Task<string> GenerateAndSendOtpAsync(string email, OtpPurpose purpose = OtpPurpose.Registration, CancellationToken cancellationToken = default);
        Task<bool> VerifyOtpAsync(string email, string code, OtpPurpose purpose = OtpPurpose.Registration, CancellationToken cancellationToken = default);
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    }
}