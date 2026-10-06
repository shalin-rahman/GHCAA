using static GHCAA.Domain.Enums;

namespace GHCAA.Application.Interfaces
{
    public interface IOtpService
    {
        Task<string> GenerateAndSendOtpAsync(string email, OtpPurpose purpose = OtpPurpose.Registration, CancellationToken cancellationToken = default);
        Task<bool> VerifyOtpAsync(string email, string code, OtpPurpose purpose = OtpPurpose.Registration, CancellationToken cancellationToken = default);
    }

    // The code was saved but the email did not go out, so the user will never see it.
    public sealed class OtpDeliveryException(string email)
        : InvalidOperationException($"The verification email to {email} could not be sent.")
    {
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    }
}