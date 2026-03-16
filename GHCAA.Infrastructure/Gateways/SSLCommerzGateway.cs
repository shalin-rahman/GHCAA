using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace GHCAA.Infrastructure.Gateways
{
    public class SSLCommerzGateway : IPaymentGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<SSLCommerzGateway> _logger;

        public SSLCommerzGateway(HttpClient httpClient, ApplicationDbContext db, ILogger<SSLCommerzGateway> logger)
        {
            _httpClient = httpClient;
            _db = db;
            _logger = logger;
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
            var url = isSandbox ? "https://sandbox.sslcommerz.com/gwprocess/v4/api.php" : "https://securepay.sslcommerz.com/gwprocess/v4/api.php";

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
                { "cus_name", "Member " + dto.MemberId },
                { "cus_email", "member_" + dto.MemberId + "@ghcaa.org" }, // Fallback
                { "cus_add1", "Not Provided" },
                { "cus_city", "Dhaka" },
                { "cus_postcode", "1000" },
                { "cus_country", "Bangladesh" },
                { "cus_phone", "01700000000" },
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

        public Task<bool> VerifyCallbackAsync(IDictionary<string, string> callbackData, CancellationToken cancellationToken = default)
        {
            if (!callbackData.ContainsKey("status") || callbackData["status"] != "VALID") return Task.FromResult(false);
            
            // Logic to verify with SSLCommerz server if needed (IPN or Validation API)
            // For now, checking the hash or status is a basic start.
            // Ideally call: https://sandbox.sslcommerz.com/validator/api/validationserverAPI.php?val_id={val_id}&store_id={store_id}&store_passwd={store_passwd}
            
            return Task.FromResult(true);
        }

        private class SSLCommerzInitResponse
        {
            public string? status { get; set; }
            public string? failedreason { get; set; }
            public string? GatewayPageURL { get; set; }
        }
    }
}
