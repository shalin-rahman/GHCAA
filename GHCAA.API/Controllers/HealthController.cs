using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("healthz")]
    public class HealthController : ControllerBase
    {
        private readonly IDatabaseHealthService _dbHealth;
        private readonly IEmailService _email;
        private readonly IConfiguration _config;
        private readonly IOrgConfigService _orgConfig;
        private readonly IFileStorageService _storage;
        private readonly ILogger<HealthController> _logger;

        public HealthController(IDatabaseHealthService dbHealth, IEmailService email, IConfiguration config, IOrgConfigService orgConfig, IFileStorageService storage, ILogger<HealthController> logger)
        {
            _dbHealth = dbHealth;
            _email = email;
            _config = config;
            _orgConfig = orgConfig;
            _storage = storage;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetHealth(CancellationToken ct)
        {
            var health = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Checks = new List<object>()
            };

            bool allHealthy = true;

            // 1. DB check
            try
            {
                var canConnect = await _dbHealth.CanConnectAsync(ct);
                health.Checks.Add(new { Name = "Database", Status = canConnect ? "Healthy" : "Unhealthy" });
                if (!canConnect) allHealthy = false;
            }
            catch (Exception ex)
            {
                // Anonymous endpoint — never leak raw exception text in the response (host/port/
                // credentials can appear in Npgsql connection-failure messages). Server-side log is
                // fine; only the client-facing body stays generic.
                _logger.LogError(ex, "Health check: Database probe failed");
                health.Checks.Add(new { Name = "Database", Status = "Error" });
                allHealthy = false;
            }

            // 2. Storage Check
            try
            {
                var storageOk = await _storage.IsAvailableAsync(ct);
                health.Checks.Add(new { Name = "FileStorage", Status = storageOk ? "Healthy" : "Unhealthy" });
                if (!storageOk) allHealthy = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check: FileStorage probe failed");
                health.Checks.Add(new { Name = "FileStorage", Status = "Error" });
                allHealthy = false;
            }

            // 3. Email Config check
            // Checks the sender EmailServiceRouter would use now: the admin's choice, else the server default.
            string? adminChoice = null;
            try
            {
                adminChoice = (await _orgConfig.GetConfigAsync()).Workflow?.EmailProvider;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check: could not read the email provider setting");
            }
            bool emailOk = EmailSettingsOptions.Resolve(adminChoice, _config["EmailSettings:Provider"]) == EmailSettingsOptions.Brevo
                ? !string.IsNullOrEmpty(_config["BrevoSettings:ApiKey"]) && !string.IsNullOrEmpty(_config["BrevoSettings:SenderEmail"])
                : !string.IsNullOrEmpty(_config["GmailSettings:Email"]) && !string.IsNullOrEmpty(_config["GmailSettings:AppPassword"]);
            health.Checks.Add(new { Name = "EmailService", Status = emailOk ? "Healthy" : "Misconfigured" });
            if (!emailOk) allHealthy = false;

            // 4. SMS Gateway (Placeholder or Check setting)
            var smsOk = !string.IsNullOrEmpty(_config["SmsSettings:Token"]);
            health.Checks.Add(new { Name = "SmsService", Status = smsOk ? "Healthy" : "Unconfigured" });
            // SMS is currently optional in early phases, so we won't mark allHealthy false for it yet

            if (!allHealthy)
                return StatusCode(503, new { Status = "Degraded", health.Checks });

            return Ok(health);
        }
    }
}
