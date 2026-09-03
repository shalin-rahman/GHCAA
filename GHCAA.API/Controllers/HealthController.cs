using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("healthz")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _email;
        private readonly IConfiguration _config;
        private readonly ILogger<HealthController> _logger;

        public HealthController(ApplicationDbContext db, IEmailService email, IConfiguration config, ILogger<HealthController> logger)
        {
            _db = db;
            _email = email;
            _config = config;
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
                var canConnect = await _db.Database.CanConnectAsync(ct);
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
                var uploadsBasePath = _config["FileStorage:BasePhysicalPath"]
                    ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
                var storageOk = Directory.Exists(uploadsBasePath);
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
            var emailConfig = _config.GetSection("GmailSettings");
            bool emailOk = !string.IsNullOrEmpty(emailConfig["Email"]) && !string.IsNullOrEmpty(emailConfig["AppPassword"]);
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
