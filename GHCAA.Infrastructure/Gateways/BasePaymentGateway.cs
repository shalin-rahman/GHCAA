using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Gateways
{
    public abstract class BasePaymentGateway : IPaymentGatewayService
    {
        protected readonly HttpClient _httpClient;
        protected readonly ApplicationDbContext _db;
        protected readonly ILogger _logger;

        protected HttpClient HttpClient => _httpClient;
        protected ApplicationDbContext Db => _db;
        protected ILogger Logger => _logger;

        protected BasePaymentGateway(HttpClient httpClient, ApplicationDbContext db, ILogger logger)
        {
            _httpClient = httpClient;
            _db = db;
            _logger = logger;
        }

        public abstract Enums.PaymentGateway GatewayType { get; }

        public abstract Task<PaymentGatewayResponseDto> InitiatePaymentAsync(PaymentGatewayInitiationDto dto, CancellationToken cancellationToken = default);

        public abstract Task<bool> VerifyCallbackAsync(IDictionary<string, string> callbackData, CancellationToken cancellationToken = default);

        public abstract Task<PaymentWebhookResultDto> ProcessWebhookAsync(Stream stream, IDictionary<string, string> headers, CancellationToken cancellationToken = default);

        protected async Task<PaymentConfiguration?> GetActiveConfigurationAsync(CancellationToken cancellationToken = default)
        {
            return await Db.PaymentConfigurations
                .FirstOrDefaultAsync(p => p.Gateway == GatewayType && p.IsEnabled, cancellationToken);
        }

        protected PaymentGatewayResponseDto DisabledResponse(string gatewayName)
        {
            Logger.LogWarning("{GatewayName} payment initiation attempted but configuration is disabled or missing.", gatewayName);
            return new PaymentGatewayResponseDto
            {
                Success = false,
                Message = $"{gatewayName} configuration is missing or disabled."
            };
        }

        protected PaymentGatewayResponseDto ErrorResponse(string gatewayName, Exception ex)
        {
            Logger.LogError(ex, "Error processing payment with {GatewayName}.", gatewayName);
            return new PaymentGatewayResponseDto
            {
                Success = false,
                Message = $"An error occurred with {gatewayName}: {ex.Message}"
            };
        }
    }
}
