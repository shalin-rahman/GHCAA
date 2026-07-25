using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GHCAA.Infrastructure.Gateways
{
    public class DGePayGateway : IPaymentGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<DGePayGateway> _logger;
        private readonly IConfiguration _config;
        private string? _cachedToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        public DGePayGateway(HttpClient httpClient, ApplicationDbContext db, ILogger<DGePayGateway> logger, IConfiguration config)
        {
            _httpClient = httpClient;
            _db = db;
            _logger = logger;
            _config = config;
        }

        public Enums.PaymentGateway GatewayType => Enums.PaymentGateway.DGePay;

        public async Task<PaymentGatewayResponseDto> InitiatePaymentAsync(PaymentGatewayInitiationDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var dbConfig = await _db.PaymentConfigurations
                    .FirstOrDefaultAsync(p => p.Gateway == GatewayType && p.IsEnabled, cancellationToken);

                if (dbConfig == null)
                {
                    return new PaymentGatewayResponseDto { Success = false, Message = "DGePay configuration not found in database." };
                }

                var token = await GetAccessTokenAsync(cancellationToken);
                if (string.IsNullOrEmpty(token))
                {
                    return new PaymentGatewayResponseDto { Success = false, Message = "Failed to authenticate with DGePay." };
                }

                var baseUrl = dbConfig.IsSandbox
                    ? _config["PaymentGateways:DGePay:SandboxUrl"] ?? "https://api-uat.dgepay.net/dipon/v3"
                    : _config["PaymentGateways:DGePay:ProductionUrl"] ?? "https://api.dgepay.net/dipon/v3";

                var clientId = dbConfig.GatewayPublicKey;
                var clientSecret = dbConfig.GatewaySecretKey;
                var apiKey = dbConfig.WalletNumber;

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(apiKey))
                {
                    return new PaymentGatewayResponseDto { Success = false, Message = "DGePay credentials missing in database." };
                }

                var payload = new Dictionary<string, object?>
                {
                    { "amount", (double)dto.Amount },
                    { "customer_token", null },
                    { "note", $"GHCAA {dto.Reference}" },
                    { "payee_information", null },
                    { "payment_method", null },
                    { "redirect_url", dto.CallbackUrl },
                    { "unique_txn_id", dto.TransactionId },
                    { "meta_data", null },
                    { "unique_user_reference", dto.MemberId?.ToString() }
                };

                var signature = GenerateSignature(payload, apiKey);
                var encryptedBody = EncryptPayload(payload, clientSecret);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/payment_gateway/initiate_payment");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Add("Signature", signature);
                request.Content = new StringContent(encryptedBody, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("DGePay Initiation Failed. Status: {Status}, Body: {Body}", response.StatusCode, responseContent);
                    return new PaymentGatewayResponseDto { Success = false, Message = "DGePay initialization failed." };
                }

                var result = JsonSerializer.Deserialize<DGePayResponse<DGePayInitData>>(responseContent);

                if (result?.Data?.WebviewUrl != null)
                {
                    return new PaymentGatewayResponseDto
                    {
                        Success = true,
                        GatewayUrl = result.Data.WebviewUrl,
                        TransactionId = dto.TransactionId
                    };
                }

                return new PaymentGatewayResponseDto { Success = false, Message = result?.Message ?? "Failed to parse DGePay response." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DGePay Initiation Exception");
                return new PaymentGatewayResponseDto { Success = false, Message = "An error occurred while connecting to DGePay." };
            }
        }

        public async Task<bool> VerifyCallbackAsync(IDictionary<string, string> callbackData, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!callbackData.TryGetValue("data", out var encryptedData))
                {
                    _logger.LogWarning("DGePay Callback missing 'data' parameter.");
                    return false;
                }

                // Handle the '+' character bug (frameworks often convert + to space)
                encryptedData = encryptedData.Replace(" ", "+");

                var dbConfig = await _db.PaymentConfigurations
                    .FirstOrDefaultAsync(p => p.Gateway == GatewayType && p.IsEnabled, cancellationToken);

                if (dbConfig == null) return false;

                var clientSecret = dbConfig.GatewaySecretKey;
                if (string.IsNullOrEmpty(clientSecret)) return false;

                var decryptedJson = DecryptPayload(encryptedData, clientSecret);
                if (string.IsNullOrEmpty(decryptedJson)) return false;

                var data = JsonSerializer.Deserialize<DGePayCallbackData>(decryptedJson);
                if (data == null) return false;

                // Store decrypted info for the controller
                callbackData["unique_txn_id"] = data.UniqueTxnId ?? "";
                callbackData["status_code"] = data.StatusCode?.ToString() ?? "";
                callbackData["amount"] = data.Amount?.ToString() ?? "0";

                // Status code 3 = success
                if (data.StatusCode != 3)
                {
                    _logger.LogInformation("DGePay Callback status not successful: {Status}", data.StatusCode);
                    return false;
                }

                // Verify with server-to-server check
                return await VerifyStatusWithServerAsync(data.UniqueTxnId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DGePay Callback Verification Exception");
                return false;
            }
        }

        public async Task<bool> ProcessWebhookAsync(Stream body, IDictionary<string, string> headers, CancellationToken cancellationToken = default)
        {
            // Webhooks for DGePay often send the same structure as callbacks or similar.
            // For now, we'll focus on the callback redirect flow.
            return false;
        }

        private async Task<bool> VerifyStatusWithServerAsync(string uniqueTxnId, CancellationToken cancellationToken)
        {
            try
            {
                var dbConfig = await _db.PaymentConfigurations
                    .FirstOrDefaultAsync(p => p.Gateway == GatewayType && p.IsEnabled, cancellationToken);

                if (dbConfig == null) return false;

                var token = await GetAccessTokenAsync(cancellationToken);
                if (string.IsNullOrEmpty(token)) return false;

                var baseUrl = dbConfig.IsSandbox
                    ? _config["PaymentGateways:DGePay:SandboxUrl"] ?? "https://api-uat.dgepay.net/dipon/v3"
                    : _config["PaymentGateways:DGePay:ProductionUrl"] ?? "https://api.dgepay.net/dipon/v3";

                var clientSecret = dbConfig.GatewaySecretKey;
                var apiKey = dbConfig.WalletNumber;

                var payload = new Dictionary<string, object?> { { "unique_txn_id", uniqueTxnId } };
                var signature = GenerateSignature(payload, apiKey!);
                var encryptedBody = EncryptPayload(payload, clientSecret!);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/payment_gateway/check_transaction_status");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Add("Signature", signature);
                request.Content = new StringContent(encryptedBody, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode) return false;

                var result = JsonSerializer.Deserialize<DGePayResponse<DGePayCallbackData>>(responseContent);
                return result?.Data?.StatusCode == 3;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DGePay Status Verification Exception");
                return false;
            }
        }

        private async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            if (_cachedToken != null && _tokenExpiry > DateTime.UtcNow.AddMinutes(1))
            {
                return _cachedToken;
            }

            var dbConfig = await _db.PaymentConfigurations
                .FirstOrDefaultAsync(p => p.Gateway == GatewayType && p.IsEnabled, cancellationToken);

            if (dbConfig == null) return null;

            var baseUrl = dbConfig.IsSandbox
                ? _config["PaymentGateways:DGePay:SandboxUrl"] ?? "https://api-uat.dgepay.net/dipon/v3"
                : _config["PaymentGateways:DGePay:ProductionUrl"] ?? "https://api.dgepay.net/dipon/v3";

            var clientId = dbConfig.GatewayPublicKey;
            var clientSecret = dbConfig.GatewaySecretKey;

            var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            var authPayload = new { client_id = clientId, client_secret = clientSecret };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/payment_gateway/authenticate");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
            request.Content = JsonContent.Create(authPayload);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<DGePayResponse<DGePayAuthData>>(cancellationToken: cancellationToken);
            if (result?.Data?.AccessToken != null)
            {
                _cachedToken = result.Data.AccessToken;
                _tokenExpiry = DateTime.UtcNow.AddHours(1); // Default expiry if not provided
                return _cachedToken;
            }

            return null;
        }

        private string GenerateSignature(IDictionary<string, object?> data, string apiKey)
        {
            var flatString = FlattenAndConcatenate(data);
            // Strip characters as per DGePay requirement
            var cleanString = flatString.Replace("{", "").Replace("}", "").Replace("\"", "").Replace(":", "").Replace(" ", "").Replace(",", "");

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(cleanString));
            return Convert.ToBase64String(hash);
        }

        private string FlattenAndConcatenate(IDictionary<string, object?> data)
        {
            var sortedKeys = data.Keys.OrderBy(k => k).ToList();
            var sb = new StringBuilder();

            foreach (var key in sortedKeys)
            {
                var value = data[key];
                sb.Append(key);

                if (value == null)
                {
                    sb.Append("null");
                }
                else if (value is bool b)
                {
                    sb.Append(b ? "true" : "false");
                }
                else if (value is int || value is long || value is double || value is decimal || value is float)
                {
                    // Numbers must be formatted as X.0
                    sb.Append(Convert.ToDouble(value).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture));
                }
                else if (value is IDictionary<string, object?> nested)
                {
                    sb.Append(FlattenAndConcatenate(nested));
                }
                else
                {
                    sb.Append(value.ToString());
                }
            }

            return sb.ToString();
        }

        private string EncryptPayload(object data, string secretKey)
        {
            var json = JsonSerializer.Serialize(data);
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);

            // AES-128-ECB
            // .NET's Aes defaults to CBC, we must change it.
            // Secret key must be 16 bytes for AES-128.
            var key = new byte[16];
            Array.Copy(keyBytes, key, Math.Min(keyBytes.Length, 16));

            using var aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(json);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }

        private string DecryptPayload(string encryptedBase64, string secretKey)
        {
            try
            {
                var encryptedBytes = Convert.FromBase64String(encryptedBase64);
                var keyBytes = Encoding.UTF8.GetBytes(secretKey);

                var key = new byte[16];
                Array.Copy(keyBytes, key, Math.Min(keyBytes.Length, 16));

                using var aes = Aes.Create();
                aes.Key = key;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to decrypt DGePay payload.");
                return string.Empty;
            }
        }

        #region Internal Models
        private class DGePayResponse<T>
        {
            [JsonPropertyName("status")]
            public string? Status { get; set; }
            [JsonPropertyName("message")]
            public string? Message { get; set; }
            [JsonPropertyName("data")]
            public T? Data { get; set; }
        }

        private class DGePayAuthData
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }
        }

        private class DGePayInitData
        {
            [JsonPropertyName("webview_url")]
            public string? WebviewUrl { get; set; }
            [JsonPropertyName("unique_txn_id")]
            public string? UniqueTxnId { get; set; }
        }

        private class DGePayCallbackData
        {
            [JsonPropertyName("status_code")]
            public int? StatusCode { get; set; }
            [JsonPropertyName("message")]
            public string? Message { get; set; }
            [JsonPropertyName("unique_txn_id")]
            public string? UniqueTxnId { get; set; }
            [JsonPropertyName("txn_number")]
            public string? TxnNumber { get; set; }
            [JsonPropertyName("amount")]
            public object? Amount { get; set; }
        }
        #endregion
    }
}
