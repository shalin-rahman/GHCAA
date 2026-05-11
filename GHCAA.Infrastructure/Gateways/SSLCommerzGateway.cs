using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

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

        // 24.11: Verify SSLCommerz verify_sign. Algorithm: sort all fields except verify_sign and
        // verify_sign_sha2 alphabetically, concatenate values with '&', append store_passwd, MD5 the result.
        private static bool VerifySign(IDictionary<string, string> data, string storePass)
        {
            if (!data.TryGetValue("verify_sign", out var expectedSign) || string.IsNullOrEmpty(expectedSign))
                return false;

            var sortedValues = data.Keys
                .Where(k => k != "verify_sign" && k != "verify_sign_sha2")
                .OrderBy(k => k)
                .Select(k => data[k]);

            var sb = new StringBuilder();
            foreach (var v in sortedValues)
                sb.Append(v).Append('&');
            sb.Append(storePass);

            var hash = MD5.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
            var computed = Convert.ToHexString(hash).ToLowerInvariant();
            return computed == expectedSign.ToLowerInvariant();
        }

        public async Task<bool> VerifyCallbackAsync(IDictionary<string, string> callbackData, CancellationToken cancellationToken = default)
        {
            if (!callbackData.ContainsKey("status") || callbackData["status"] != "VALID") return false;

            var valId = callbackData.TryGetValue("val_id", out var v) ? v : "";
            if (string.IsNullOrEmpty(valId)) return false;

            var config = await _db.PaymentConfigurations
                .FirstOrDefaultAsync(p => p.Gateway == GatewayType && p.IsEnabled, cancellationToken);

            if (config == null) return false;

            // 24.11: Verify the HMAC signature before trusting any val_id or amount in the callback.
            if (!VerifySign(callbackData, config.GatewaySecretKey ?? string.Empty))
            {
                _logger.LogWarning("SSLCommerz callback rejected: verify_sign mismatch");
                return false;
            }

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

        public async Task<bool> ProcessWebhookAsync(Stream body, IDictionary<string, string> headers, CancellationToken cancellationToken = default)
        {
            try
            {
                using var reader = new StreamReader(body);
                var content = await reader.ReadToEndAsync(cancellationToken);
                // S4.2: Use QueryHelpers.ParseQuery so base64 values containing '=' are not truncated.
                var parsed = QueryHelpers.ParseQuery(content);
                var data = parsed.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());

                return await VerifyCallbackAsync(data, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SSLCommerz Webhook Processing Failed");
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
