using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace GHCAA.Infrastructure.Gateways
{
    public class NagadGateway : BasePaymentGateway
    {
        private readonly IConfiguration _config;

        public NagadGateway(HttpClient httpClient, ApplicationDbContext db, ILogger<NagadGateway> logger, IConfiguration config)
            : base(httpClient, db, logger)
        {
            _config = config;
        }

        public override Enums.PaymentGateway GatewayType => Enums.PaymentGateway.NagadGateway;

        public override Task<PaymentGatewayResponseDto> InitiatePaymentAsync(PaymentGatewayInitiationDto dto, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PaymentGatewayResponseDto
            {
                Success = false,
                Message = "Nagad automatic gateway integration is coming soon. Please use manual method for now."
            });
        }

        public override Task<bool> VerifyCallbackAsync(IDictionary<string, string> callbackData, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public override Task<PaymentWebhookResultDto> ProcessWebhookAsync(Stream stream, IDictionary<string, string> headers, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(PaymentWebhookResultDto.Invalid());
        }
    }
}
