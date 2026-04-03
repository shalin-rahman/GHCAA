using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace GHCAA.Infrastructure.Gateways
{
    public class SSLCommerzGateway : IPaymentGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<SSLCommerzGateway> _logger;
        private readonly IConfiguration _config;

        public SSLCommerzGateway(HttpClient httpClient, ApplicationDbContext db, ILogger<SSLCommerzGateway> logger, IConfiguration config)
        {
            _httpClient = httpClient;
            _db = db;
            _logger = logger;
            _config = config;
        }

        public Enums.PaymentGateway GatewayType => Enums.PaymentGateway.SSLCommerz;

        public async Task<PaymentGatewayResponseDto> InitiatePaymentAsync(PaymentGatewayInitiationDto dto, CancellationToken cancellationToken = default)
        {
            var config = await _db.PaymentConfigurations
                .FirstOrDefaultAsync(p => p.Gateway == GatewayType && p.IsEnabled, cancellationToken);

            if (config == null) 
                return new PaymentGatewayResponseDto { Success = false, Message = "SSLCommerz configuration not found or disabled." };

            var storeId = config.GatewayPublicKey;
            var storePass = config.GatewaySecretKey;
            
            var isSandbox = config.IsSandbox;
            var sandboxUrl = _config["PaymentGateways:SSLCommerz:SandboxUrl"] ?? "https://sandbox.sslcommerz.com";
            var prodUrl = _config["PaymentGateways:SSLCommerz:ProductionUrl"] ?? "https://securepay.sslcommerz.com";
            var url = isSandbox 
                ? $"{sandboxUrl.TrimEnd('/')}/gwprocess/v4/api.php" 
                : $"{prodUrl.TrimEnd('/')}/gwprocess/v4/api.php";

            var formData = new Dictionary<string, string>
            {
                { "store_id", storeId ?? "" },
                { "store_passwd", storePass ?? "" },
                { "total_amount", dto.Amount.ToString("0.00") },
                { "currency", dto.Currency },
                { "tran_id", dto.TransactionId ?? Guid.NewGuid().ToString("N") },
                { "success_url", dto.CallbackUrl },
                { "fail_url", dto.CallbackUrl },
                { "cancel_url", dto.CallbackUrl },
                { "cus_name", dto.CustomerName ?? ("Member " + dto.MemberId) },
                { "cus_email", dto.CustomerEmail ?? ("member_" + dto.MemberId + "@" + (_config["GeneralSettings:EmailDomain"] ?? "ghcaa.org")) },
                { "cus_add1", "Not Provided" },
                { "cus_city", "Dhaka" },
                { "cus_postcode", "1000" },
                { "cus_country", "Bangladesh" },
                { "cus_phone", dto.CustomerPhone ?? "01700000000" },
                { "product_name", "GHCAA " + dto.Reference },
                { "product_category", "Membership" },
                { "product_profile", "general" }
            };

            try
            {
                var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(formData), cancellationToken);
                var result = await response.Content.ReadFromJsonAsync<SSLCommerzInitResponse>(cancellationToken: cancellationToken);

                if (result?.status == "SUCCESS")
                {
                    return new PaymentGatewayResponseDto 
                    { 
                        Success = true, 
                        GatewayUrl = result.GatewayPageURL,
                        TransactionId = formData["tran_id"]
                    };
                }

                return new PaymentGatewayResponseDto { Success = false, Message = result?.failedreason ?? "Initialization failed." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SSLCommerz Initiation Failed");
                return new PaymentGatewayResponseDto { Success = false, Message = "Connection to payment gateway failed." };
            }
        }

        public async Task<bool> VerifyCallbackAsync(IDictionary<string, string> callbackData, CancellationToken cancellationToken = default)
        {
            if (!callbackData.ContainsKey("status") || callbackData["status"] != "VALID") return false;
            
            var valId = callbackData.TryGetValue("val_id", out var v) ? v : "";
            if (string.IsNullOrEmpty(valId)) return false;

            var config = await _db.PaymentConfigurations
                .FirstOrDefaultAsync(p => p.Gateway == GatewayType && p.IsEnabled, cancellationToken);
            
            if (config == null) return false;

            var isSandbox = config.IsSandbox;
            var sandboxUrl = _config["PaymentGateways:SSLCommerz:SandboxUrl"] ?? "https://sandbox.sslcommerz.com";
            var prodUrl = _config["PaymentGateways:SSLCommerz:ProductionUrl"] ?? "https://securepay.sslcommerz.com";
            
            var baseValidationUrl = isSandbox ? sandboxUrl : prodUrl;
            var validationUrl = $"{baseValidationUrl.TrimEnd('/')}/validator/api/validationserverAPI.php?val_id={valId}&store_id={config.GatewayPublicKey}&store_passwd={config.GatewaySecretKey}&format=json";

            try
            {
                var response = await _httpClient.GetAsync(validationUrl, cancellationToken);
                var result = await response.Content.ReadFromJsonAsync<SSLCommerzValidationResponse>(cancellationToken: cancellationToken);

                return result?.status == "VALID" || result?.status == "AUTHENTICATED";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SSLCommerz Verification Failed");
                return false;
            }
        }

        private class SSLCommerzValidationResponse
        {
            public string? status { get; set; }
        }

        private class SSLCommerzInitResponse
        {
            public string? status { get; set; }
            public string? failedreason { get; set; }
            public string? GatewayPageURL { get; set; }
        }
    }
}
