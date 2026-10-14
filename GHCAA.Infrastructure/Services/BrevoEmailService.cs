using System.Net.Http.Json;
using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GHCAA.Infrastructure.Services
{
    // Sends over HTTPS, for hosts that block outbound SMTP (Render's free tier does).
    // Brevo accepts a single verified sender address, so no domain or DNS records are needed.
    public class BrevoEmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BrevoEmailService> _logger;
        private readonly string _apiKey;
        private readonly string _senderEmail;
        private readonly string? _senderName;
        private readonly string _baseUrl;

        public BrevoEmailService(HttpClient httpClient, IOptions<BrevoSettingsOptions> options, ILogger<BrevoEmailService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            var settings = options.Value;
            _apiKey = string.IsNullOrWhiteSpace(settings.ApiKey)
                ? throw new ArgumentNullException("BrevoSettings:ApiKey")
                : settings.ApiKey;
            _senderEmail = string.IsNullOrWhiteSpace(settings.SenderEmail)
                ? throw new ArgumentNullException("BrevoSettings:SenderEmail")
                : settings.SenderEmail;
            _senderName = string.IsNullOrWhiteSpace(settings.SenderName) ? null : settings.SenderName;
            _baseUrl = settings.BaseUrl;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            // Brevo rejects a sender with an empty name, so the key is left out when none is set.
            object sender = _senderName is null
                ? new { email = _senderEmail }
                : new { name = _senderName, email = _senderEmail };
            var payload = new
            {
                sender,
                to = new[] { new { email = to } },
                subject,
                htmlContent = htmlBody
            };

            HttpResponseMessage response;
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl)
                {
                    Content = JsonContent.Create(payload)
                };
                request.Headers.Add("api-key", _apiKey);
                request.Headers.Add("accept", "application/json");
                response = await _httpClient.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System failed to send email to {To}. Error: {Message}", to, ex.Message);
                throw new InvalidOperationException($"Email service error: {ex.Message}");
            }

            using (response)
            {
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully to {To}", to);
                    return;
                }

                // Brevo's error body names the cause (unverified sender, bad key, quota) and never echoes the key.
                var detail = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Brevo refused email to {To}. Status {Status}: {Detail}", to, (int)response.StatusCode, detail);
                throw new InvalidOperationException($"Email service error: Brevo returned {(int)response.StatusCode}. {detail}");
            }
        }
    }
}
