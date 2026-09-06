using GHCAA.Domain.Models;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.Interfaces
{
    public interface IPaymentConfigService
    {
        Task<List<PaymentConfiguration>> GetActiveMethodsAsync(CancellationToken cancellationToken = default);
        Task<List<PaymentConfiguration>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PaymentConfiguration> CreateAsync(PaymentConfiguration config, CancellationToken cancellationToken = default);
        Task<PaymentConfiguration?> UpdateAsync(int id, PaymentConfiguration config, bool allowSecretUpdate, CancellationToken cancellationToken = default);
        Task<PaymentConfiguration?> ToggleAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> HasAnyAsync(CancellationToken cancellationToken = default);
        Task<List<PaymentConfiguration>> SeedDefaultsAsync(CancellationToken cancellationToken = default);

        // Used by GatewaysController to check whether a given gateway's manual/online channel
        // is currently enabled, instead of re-querying PaymentConfigurations directly.
        Task<PaymentConfiguration?> GetEnabledByGatewayAsync(PaymentGateway gateway, CancellationToken cancellationToken = default);
    }
}
