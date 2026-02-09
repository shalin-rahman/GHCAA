namespace GHCAA.Application.Interfaces
{
    public interface IOtpService
    {
        Task<string> GenerateAndSendOtpAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> VerifyOtpAsync(string email, string code, CancellationToken cancellationToken = default);
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    }
}