using GHCAA.Application.DTOs;
using GHCAA.Domain;

namespace GHCAA.Application.Interfaces
{
    public interface IPaymentGatewayService
    {
        Enums.PaymentGateway GatewayType { get; }
        Task<PaymentGatewayResponseDto> InitiatePaymentAsync(PaymentGatewayInitiationDto dto, CancellationToken cancellationToken = default);
        Task<bool> VerifyCallbackAsync(IDictionary<string, string> callbackData, CancellationToken cancellationToken = default);
        Task<PaymentWebhookResultDto> ProcessWebhookAsync(Stream body, IDictionary<string, string> headers, CancellationToken cancellationToken = default);
    }

    public interface IPaymentGatewayFactory
    {
        IPaymentGatewayService GetGateway(Enums.PaymentGateway gateway);
    }
}
