using GHCAA.Application.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace GHCAA.Infrastructure.Services
{
    public class GmailEmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<GmailEmailService> _logger;
        private readonly string _email;
        private readonly string _appPassword;
        private readonly string _host;
        private readonly int _port;

        public GmailEmailService(IConfiguration config, ILogger<GmailEmailService> logger)
        {
            _config = config;
            _logger = logger;
            _email = _config["GmailSettings:Email"] ?? throw new ArgumentNullException("GmailSettings:Email");
            _appPassword = _config["GmailSettings:AppPassword"] ?? throw new ArgumentNullException("GmailSettings:AppPassword");
            _host = _config["GmailSettings:Host"] ?? "smtp.gmail.com";
            _port = int.TryParse(_config["GmailSettings:Port"], out var p) ? p : 587;
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
                // Set a reasonable timeout (10 seconds)
                client.Timeout = 10000;
                
                await client.ConnectAsync(_host, _port, MailKit.Security.SecureSocketOptions.StartTls, cancellationToken);
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
