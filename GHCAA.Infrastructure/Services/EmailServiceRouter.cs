using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GHCAA.Infrastructure.Services
{
    // Picks Gmail or Brevo on each send from the admin setting Workflow.EmailProvider, so an admin
    // can switch without a restart. The sender is resolved only when used, because each sender's
    // constructor throws when its own env settings are missing.
    public class EmailServiceRouter : IEmailService
    {
        private readonly IOrgConfigService _orgConfig;
        private readonly IServiceProvider _services;
        private readonly string _serverDefault;
        private readonly ILogger<EmailServiceRouter> _logger;

        public EmailServiceRouter(
            IOrgConfigService orgConfig,
            IServiceProvider services,
            IOptions<EmailSettingsOptions> options,
            ILogger<EmailServiceRouter> logger)
        {
            _orgConfig = orgConfig;
            _services = services;
            _serverDefault = options.Value.Provider;
            _logger = logger;
        }

        public async Task<string> GetActiveProviderAsync()
        {
            try
            {
                var config = await _orgConfig.GetConfigAsync();
                return EmailSettingsOptions.Resolve(config.Workflow?.EmailProvider, _serverDefault);
            }
            catch (Exception ex)
            {
                // A config read failure should not also stop OTP mail, so fall back to the server setting.
                _logger.LogWarning(ex, "Could not read the email provider setting; using {Provider}", _serverDefault);
                return EmailSettingsOptions.Resolve(null, _serverDefault);
            }
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            var provider = await GetActiveProviderAsync();
            IEmailService sender = provider == EmailSettingsOptions.Brevo
                ? _services.GetRequiredService<BrevoEmailService>()
                : _services.GetRequiredService<GmailEmailService>();
            await sender.SendEmailAsync(to, subject, htmlBody, cancellationToken);
        }
    }
}
