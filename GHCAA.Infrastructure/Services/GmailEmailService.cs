using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Options;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GHCAA.Infrastructure.Services
{
    public class GmailEmailService : IEmailService
    {
        private readonly ILogger<GmailEmailService> _logger;
        private readonly string _email;
        private readonly string _appPassword;
        private readonly string _host;
        private readonly int _port;

        public GmailEmailService(IOptions<GmailSettingsOptions> options, ILogger<GmailEmailService> logger)
        {
            _logger = logger;
            var settings = options.Value;
            _email = settings.Email ?? throw new ArgumentNullException("GmailSettings:Email");
            _appPassword = settings.AppPassword ?? throw new ArgumentNullException("GmailSettings:AppPassword");
            _host = settings.Host;
            _port = settings.Port;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(_email));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;
                var body = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = body.ToMessageBody();

                using var client = new SmtpClient();
                // Increased timeout for potentially slow cloud networks (Render/Docker)
                client.Timeout = 30000;

                // Compatibility for environments with strict certificate validation
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                // Use 'Auto' to negotiate the best security option for the given port
                await client.ConnectAsync(_host, _port, MailKit.Security.SecureSocketOptions.Auto, cancellationToken);
                await client.AuthenticateAsync(_email, _appPassword, cancellationToken);
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("Email sent successfully to {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System failed to send email to {To}. Error: {Message}", to, ex.Message);
                throw new InvalidOperationException($"Email service error: {ex.Message}");
            }
        }
    }
}
