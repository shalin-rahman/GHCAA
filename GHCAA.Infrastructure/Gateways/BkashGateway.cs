using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace GHCAA.Infrastructure.Gateways
{
    public class BkashGateway : IPaymentGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<BkashGateway> _logger;
        private readonly IConfiguration _config;

        public BkashGateway(HttpClient httpClient, ApplicationDbContext db, ILogger<BkashGateway> logger, IConfiguration config)
        {
            _httpClient = httpClient;
            _db = db;
            _logger = logger;
            _config = config;
        }

        public Enums.PaymentGateway GatewayType => Enums.PaymentGateway.BkashGateway;

        public async Task<PaymentGatewayResponseDto> InitiatePaymentAsync(PaymentGatewayInitiationDto dto, CancellationToken cancellationToken = default)
        {
            var config = await _db.PaymentConfigurations
                .FirstOrDefaultAsync(p => p.Gateway == GatewayType && p.IsEnabled, cancellationToken);

            if (config == null)
                return new PaymentGatewayResponseDto { Success = false, Message = "bKash configuration not found." };

            try
            {
                // 1. Grant Token
                var token = await GetTokenAsync(config, cancellationToken);
                if (token == null) return new PaymentGatewayResponseDto { Success = false, Message = "bKash Authentication failed." };

                // 2. Create Payment
                var baseUrl = config.IsSandbox 
                    ? _config["PaymentGateways:Bkash:SandboxUrl"] ?? "https://checkout.sandbox.bka.sh/v1.2.0-beta/checkout" 
                    : _config["PaymentGateways:Bkash:ProductionUrl"] ?? "https://checkout.pay.bka.sh/v1.2.0-beta/checkout";
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", token);
                _httpClient.DefaultRequestHeaders.Add("X-APP-Key", config.GatewayPublicKey);

                var createPayload = new
                {
                    amount = dto.Amount.ToString("0"),
                    currency = "BDT",
                    intent = "sale",
                    merchantInvoiceNumber = dto.Reference,
                    callbackURL = dto.CallbackUrl
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/payment/create", createPayload, cancellationToken);
                var result = await response.Content.ReadFromJsonAsync<BkashCreateResponse>(cancellationToken: cancellationToken);

                if (result?.StatusCode == "0000")
                {
                    return new PaymentGatewayResponseDto 
                    { 
                        Success = true, 
                        GatewayUrl = result.BkashURL,
                        TransactionId = result.PaymentID
                    };
                }

                return new PaymentGatewayResponseDto { Success = false, Message = result?.StatusMessage ?? "bKash payment creation failed." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "bKash Initiation Failed");
                return new PaymentGatewayResponseDto { Success = false, Message = "bKash gateway unrechable." };
            }
        }

        public async Task<bool> VerifyCallbackAsync(IDictionary<string, string> callbackData, CancellationToken cancellationToken = default)
        {
            if (!callbackData.TryGetValue("paymentID", out var paymentId) || string.IsNullOrEmpty(paymentId)) return false;
            if (!callbackData.TryGetValue("status", out var status) || status != "success") return false;

            var config = await _db.PaymentConfigurations
                .FirstOrDefaultAsync(p => p.Gateway == GatewayType && p.IsEnabled, cancellationToken);

            if (config == null) return false;

            try
            {
                var token = await GetTokenAsync(config, cancellationToken);
                if (token == null) return false;

                var baseUrl = config.IsSandbox 
                    ? _config["PaymentGateways:Bkash:SandboxUrl"] ?? "https://checkout.sandbox.bka.sh/v1.2.0-beta/checkout" 
                    : _config["PaymentGateways:Bkash:ProductionUrl"] ?? "https://checkout.pay.bka.sh/v1.2.0-beta/checkout";
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", token);
                _httpClient.DefaultRequestHeaders.Add("X-APP-Key", config.GatewayPublicKey);

                var executePayload = new { paymentID = paymentId };
                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/payment/execute", executePayload, cancellationToken);
                var result = await response.Content.ReadFromJsonAsync<BkashExecuteResponse>(cancellationToken: cancellationToken);

                // StatusCode 0000 and TransactionStatus Completed indicate success
                return result?.StatusCode == "0000" && result?.TransactionStatus == "Completed";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "bKash Execute Failed for PaymentID {PaymentID}", paymentId);
                return false;
            }
        }

        private async Task<string?> GetTokenAsync(Domain.Models.PaymentConfiguration config, CancellationToken cancellationToken)
        {
            var baseUrl = config.IsSandbox 
                ? _config["PaymentGateways:Bkash:SandboxUrl"] ?? "https://checkout.sandbox.bka.sh/v1.2.0-beta/checkout" 
                : _config["PaymentGateways:Bkash:ProductionUrl"] ?? "https://checkout.pay.bka.sh/v1.2.0-beta/checkout";
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("username", config.WalletNumber ?? _config["PaymentGateways:Bkash:Username"] ?? "sandbox_user");
            
            var password = config.IsSandbox 
                ? _config["PaymentGateways:Bkash:SandboxPassword"] ?? "sandbox_pass" 
                : _config["PaymentGateways:Bkash:ProductionPassword"] ?? "";
            
            _httpClient.DefaultRequestHeaders.Add("password", password);

            var payload = new { app_key = config.GatewayPublicKey, app_secret = config.GatewaySecretKey };
            var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/token/grant", payload, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<BkashTokenResponse>(cancellationToken: cancellationToken);
            
            return result?.IdToken;
        }

        private class BkashTokenResponse { [JsonPropertyName("id_token")] public string? IdToken { get; set; } }
        private class BkashCreateResponse { 
            public string? StatusCode { get; set; } 
            public string? StatusMessage { get; set; } 
            public string? BkashURL { get; set; } 
            public string? PaymentID { get; set; } 
        }
        private class BkashExecuteResponse {
            public string? StatusCode { get; set; }
            public string? StatusMessage { get; set; }
            public string? PaymentID { get; set; }
            public string? TrxID { get; set; }
            public string? TransactionStatus { get; set; }
            public string? Amount { get; set; }
        }
    }
}
