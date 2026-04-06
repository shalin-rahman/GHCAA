using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    public class GreenwebSmsService : ISmsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<GreenwebSmsService> _logger;
        private readonly ApplicationDbContext _db;
        private readonly string _token;
        private readonly string _baseUrl;

        public GreenwebSmsService(
            HttpClient httpClient, 
            IConfiguration config, 
            ILogger<GreenwebSmsService> logger, 
            ApplicationDbContext db)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
            _db = db;
            _token = _config["SmsSettings:Token"] ?? string.Empty;
            _baseUrl = _config["SmsSettings:BaseUrl"] ?? "https://api.greenweb.com.bd/api.php";
        }

        public async Task<bool> SendSmsAsync(string mobileNo, string message, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(_token))
            {
                _logger.LogWarning("SMS attempt ignored: No token configured.");
                return false;
            }

            try
            {
                // Format: token=YOUR_TOKEN&to=RECIPIENT_NUMBER&message=YOUR_MESSAGE
                var url = $"{_baseUrl}?token={_token}&to={mobileNo.TrimStart('+')}&message={Uri.EscapeDataString(message)}";
                
                var response = await _httpClient.GetAsync(url, cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("SMS sent successfully to {MobileNo}", mobileNo);
                    return true;
                }

                _logger.LogWarning("Failed to send SMS to {MobileNo}. Response: {Content}", mobileNo, content);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS service error for {MobileNo}.", mobileNo);
                return false;
            }
        }

        public async Task<bool> SendOtpSmsAsync(string mobileNo, string otpCode, CancellationToken cancellationToken = default)
        {
            var message = $"GHC Alumni Association: Your verification code is {otpCode}. Valid for 10 minutes.";
            return await SendSmsAsync(mobileNo, message, cancellationToken);
        }

        public async Task<bool> SendAlertSmsAsync(int memberId, string message, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            if (member == null || string.IsNullOrEmpty(member.MobileNo)) return false;

            return await SendSmsAsync(member.MobileNo, message, cancellationToken);
        }
    }
}
