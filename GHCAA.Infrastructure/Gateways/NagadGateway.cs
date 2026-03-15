using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Gateways
{
    public class NagadGateway : IPaymentGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<NagadGateway> _logger;

        public NagadGateway(HttpClient httpClient, ApplicationDbContext db, ILogger<NagadGateway> logger)
        {
            _httpClient = httpClient;
            _db = db;
            _logger = logger;
        }

        public Enums.PaymentGateway GatewayType => Enums.PaymentGateway.NagadGateway;

        public async Task<PaymentGatewayResponseDto> InitiatePaymentAsync(PaymentGatewayInitiationDto dto, CancellationToken cancellationToken = default)
        {
            // Nagad integration is multi-step (sensitive data encryption, order creation).
            // Placeholder implementation.
            return new PaymentGatewayResponseDto 
            { 
                Success = false, 
                Message = "Nagad automatic gateway integration is coming soon. Please use manual method for now." 
            };
        }

        public async Task<bool> VerifyCallbackAsync(IDictionary<string, string> callbackData, CancellationToken cancellationToken = default)
        {
            return false;
        }
    }
}
