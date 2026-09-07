using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace GHCAA.Infrastructure.Gateways
{
    public class BkashGateway : IPaymentGatewayService
    {
        // Keys used both when reading a callback/webhook payload and when VerifyCallbackAsync
        // writes the Execute-response fields back into the passed dictionary for ProcessWebhookAsync.
        private const string PaymentIdKey = "paymentID";
        private const string MerchantInvoiceNumberKey = "merchantInvoiceNumber";
        private const string AmountKey = "amount";

        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<BkashGateway> _logger;
        private readonly BkashOptions _bkashOptions;

        public BkashGateway(HttpClient httpClient, ApplicationDbContext db, ILogger<BkashGateway> logger, IOptions<BkashOptions> bkashOptions)
        {
            _httpClient = httpClient;
            _db = db;
            _logger = logger;
            _bkashOptions = bkashOptions.Value;
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

                // 2. Create Payment — use per-request headers to avoid DefaultRequestHeaders race (S4.1).
                var baseUrl = config.IsSandbox
                    ? _bkashOptions.SandboxUrl
                    : _bkashOptions.ProductionUrl;

                var createPayload = new
                {
                    amount = dto.Amount.ToString("0"),
                    currency = "BDT",
                    intent = "sale",
                    // 24.12: Use TransactionId (our internal TrxId) as merchantInvoiceNumber so the
                    // execute response can be mapped back to the correct PaymentHistory row.
                    merchantInvoiceNumber = dto.TransactionId,
                    callbackURL = dto.CallbackUrl
                };

                var createMsg = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/payment/create")
                {
                    Content = System.Net.Http.Json.JsonContent.Create(createPayload)
                };
                createMsg.Headers.Add("Authorization", token);
                createMsg.Headers.Add("X-APP-Key", config.GatewayPublicKey);
                var response = await _httpClient.SendAsync(createMsg, cancellationToken);
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
            if (!callbackData.TryGetValue(PaymentIdKey, out var paymentId) || string.IsNullOrEmpty(paymentId)) return false;
            // Accept both "success" and "Success" — bKash docs inconsistently use both forms.
            if (!callbackData.TryGetValue("status", out var status)
                || !string.Equals(status, "success", StringComparison.OrdinalIgnoreCase)) return false;

            var config = await _db.PaymentConfigurations
                .FirstOrDefaultAsync(p => p.Gateway == GatewayType && p.IsEnabled, cancellationToken);

            if (config == null) return false;

            try
            {
                var token = await GetTokenAsync(config, cancellationToken);
                if (token == null) return false;

                var baseUrl = config.IsSandbox
                    ? _bkashOptions.SandboxUrl
                    : _bkashOptions.ProductionUrl;

                var executePayload = new { paymentID = paymentId };
                var executeMsg = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/payment/execute")
                {
                    Content = System.Net.Http.Json.JsonContent.Create(executePayload)
                };
                executeMsg.Headers.Add("Authorization", token);
                executeMsg.Headers.Add("X-APP-Key", config.GatewayPublicKey);
                var response = await _httpClient.SendAsync(executeMsg, cancellationToken);
                var result = await response.Content.ReadFromJsonAsync<BkashExecuteResponse>(cancellationToken: cancellationToken);

                if (result?.StatusCode != "0000" || result?.TransactionStatus != "Completed")
                    return false;

                // 24.12: Verify the executed amount matches the stored PaymentHistory amount.
                // merchantInvoiceNumber = our internal TransactionId (see InitiatePaymentAsync).
                if (!string.IsNullOrEmpty(result.MerchantInvoiceNumber)
                    && decimal.TryParse(result.Amount, out var executedAmount))
                {
                    var payment = await _db.PaymentHistories.AsNoTracking()
                        .FirstOrDefaultAsync(p => p.TransactionId == result.MerchantInvoiceNumber, cancellationToken);

                    if (payment != null && Math.Abs(payment.Amount - executedAmount) > 0.01m)
                    {
                        _logger.LogWarning("bKash amount mismatch for {TrxId}: expected {Expected}, got {Actual}",
                            result.MerchantInvoiceNumber, payment.Amount, executedAmount);
                        return false;
                    }
                }

                // Same pattern DGePayGateway uses: write the fields the caller actually needs to
                // key off back into the passed dictionary, since VerifyCallbackAsync's own signature
                // is fixed by IPaymentGatewayService and can't return them directly. MerchantInvoiceNumber
                // is our internal TransactionId, not bKash's own paymentID — the two are different fields.
                if (!string.IsNullOrEmpty(result.MerchantInvoiceNumber))
                    callbackData[MerchantInvoiceNumberKey] = result.MerchantInvoiceNumber;
                if (!string.IsNullOrEmpty(result.Amount))
                    callbackData[AmountKey] = result.Amount;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "bKash Execute Failed for PaymentID {PaymentID}", paymentId);
                return false;
            }
        }

        // 24.10: Verify X-APP-Key header before trusting any paymentID in the webhook body.
        public async Task<PaymentWebhookResultDto> ProcessWebhookAsync(Stream body, IDictionary<string, string> headers, CancellationToken cancellationToken = default)
        {
            try
            {
                // Reject immediately if the app key is missing or doesn't match config.
                if (!headers.TryGetValue("X-APP-Key", out var incomingKey) || string.IsNullOrEmpty(incomingKey))
                {
                    _logger.LogWarning("bKash webhook rejected: missing X-APP-Key header");
                    return PaymentWebhookResultDto.Invalid();
                }

                var config = await _db.PaymentConfigurations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Gateway == GatewayType && p.IsEnabled, cancellationToken);

                if (config == null || config.GatewayPublicKey != incomingKey)
                {
                    _logger.LogWarning("bKash webhook rejected: invalid X-APP-Key");
                    return PaymentWebhookResultDto.Invalid();
                }

                var payload = await System.Text.Json.JsonSerializer.DeserializeAsync<Dictionary<string, object>>(body, cancellationToken: cancellationToken);
                if (payload == null) return PaymentWebhookResultDto.Invalid();

                var data = payload.ToDictionary(x => x.Key, x => x.Value?.ToString() ?? "");
                var paymentId = data.TryGetValue(PaymentIdKey, out var pid) ? pid : null;

                if (!await VerifyCallbackAsync(data, cancellationToken))
                    return PaymentWebhookResultDto.Invalid();

                // VerifyCallbackAsync wrote merchantInvoiceNumber/amount back into `data` on success —
                // merchantInvoiceNumber is our internal TransactionId, not bKash's own paymentID.
                return new PaymentWebhookResultDto
                {
                    IsValid = true,
                    TransactionId = data.TryGetValue(MerchantInvoiceNumberKey, out var inv) ? inv : null,
                    ConfirmedAmount = data.TryGetValue(AmountKey, out var amt) && decimal.TryParse(amt, out var parsed) ? parsed : (decimal?)null,
                    GatewayPaymentId = paymentId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "bKash Webhook Processing Failed");
                return PaymentWebhookResultDto.Invalid();
            }
        }

        private async Task<string?> GetTokenAsync(Domain.Models.PaymentConfiguration config, CancellationToken cancellationToken)
        {
            var baseUrl = config.IsSandbox
                ? _bkashOptions.SandboxUrl
                : _bkashOptions.ProductionUrl;

            var password = config.IsSandbox
                ? _bkashOptions.SandboxPassword
                : _bkashOptions.ProductionPassword;

            // S4.1: Use per-request HttpRequestMessage instead of mutating DefaultRequestHeaders.
            var payload = new { app_key = config.GatewayPublicKey, app_secret = config.GatewaySecretKey };
            var msg = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/token/grant")
            {
                Content = System.Net.Http.Json.JsonContent.Create(payload)
            };
            msg.Headers.Add("username", config.WalletNumber ?? _bkashOptions.Username);
            msg.Headers.Add("password", password);

            var response = await _httpClient.SendAsync(msg, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<BkashTokenResponse>(cancellationToken: cancellationToken);

            return result?.IdToken;
        }

        private class BkashTokenResponse { [JsonPropertyName("id_token")] public string? IdToken { get; set; } }
        private class BkashCreateResponse
        {
            public string? StatusCode { get; set; }
            public string? StatusMessage { get; set; }
            public string? BkashURL { get; set; }
            public string? PaymentID { get; set; }
        }
        private class BkashExecuteResponse
        {
            public string? StatusCode { get; set; }
            public string? StatusMessage { get; set; }
            public string? PaymentID { get; set; }
            public string? TrxID { get; set; }
            public string? TransactionStatus { get; set; }
            public string? Amount { get; set; }
            [JsonPropertyName("merchantInvoiceNumber")]
            public string? MerchantInvoiceNumber { get; set; }
        }
    }
}
