using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services
{
    public class PaymentConfigService : IPaymentConfigService
    {
        private readonly ApplicationDbContext _db;

        public PaymentConfigService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<PaymentConfiguration>> GetActiveMethodsAsync(CancellationToken cancellationToken = default)
            => await _db.PaymentConfigurations
                .Where(p => p.IsEnabled)
                .OrderBy(p => p.SortOrder)
                .ToListAsync(cancellationToken);

        public async Task<List<PaymentConfiguration>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _db.PaymentConfigurations
                .OrderBy(p => p.SortOrder)
                .ToListAsync(cancellationToken);

        public async Task<PaymentConfiguration> CreateAsync(PaymentConfiguration config, CancellationToken cancellationToken = default)
        {
            config.CreatedAt = DateTime.UtcNow;
            await _db.PaymentConfigurations.AddAsync(config, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return config;
        }

        public async Task<PaymentConfiguration?> UpdateAsync(int id, PaymentConfiguration config, bool allowSecretUpdate, CancellationToken cancellationToken = default)
        {
            var existing = await _db.PaymentConfigurations.FindAsync(new object[] { id }, cancellationToken);
            if (existing == null) return null;

            existing.DisplayName = config.DisplayName;
            existing.Description = config.Description;
            existing.Icon = config.Icon;
            existing.IsEnabled = config.IsEnabled;
            existing.WalletNumber = config.WalletNumber;
            existing.AccountHolderName = config.AccountHolderName;
            existing.BankName = config.BankName;
            existing.BranchName = config.BranchName;
            existing.AccountNumber = config.AccountNumber;
            existing.RoutingNumber = config.RoutingNumber;
            existing.Gateway = config.Gateway;

            if (allowSecretUpdate)
            {
                if (config.GatewayPublicKey != "********")
                    existing.GatewayPublicKey = config.GatewayPublicKey;
                if (config.GatewaySecretKey != "********")
                    existing.GatewaySecretKey = config.GatewaySecretKey;

                existing.GatewayCallbackUrl = config.GatewayCallbackUrl;
                existing.IsSandbox = config.IsSandbox;
            }

            existing.SortOrder = config.SortOrder;
            existing.Instructions = config.Instructions;
            existing.RequiresReceipt = config.RequiresReceipt;
            existing.RequiresReference = config.RequiresReference;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return existing;
        }

        public async Task<PaymentConfiguration?> ToggleAsync(int id, CancellationToken cancellationToken = default)
        {
            var config = await _db.PaymentConfigurations.FindAsync(new object[] { id }, cancellationToken);
            if (config == null) return null;

            config.IsEnabled = !config.IsEnabled;
            config.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return config;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var config = await _db.PaymentConfigurations.FindAsync(new object[] { id }, cancellationToken);
            if (config == null) return false;

            _db.PaymentConfigurations.Remove(config);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public Task<bool> HasAnyAsync(CancellationToken cancellationToken = default)
            => _db.PaymentConfigurations.AnyAsync(cancellationToken);

        public async Task<List<PaymentConfiguration>> SeedDefaultsAsync(CancellationToken cancellationToken = default)
        {
            var defaults = new List<PaymentConfiguration>
            {
                new() { Method = PaymentMethod.BKash, DisplayName = "bKash", Description = "Pay via bKash mobile wallet", Icon = "💖", WalletNumber = "01XXXXXXXXX", AccountHolderName = "GHCAA", Instructions = "Send money to the bKash number shown. Use your Registration ID as reference.", RequiresReceipt = true, RequiresReference = true, SortOrder = 1 },
                new() { Method = PaymentMethod.Nagad, DisplayName = "Nagad", Description = "Pay via Nagad mobile wallet", Icon = "🟧", WalletNumber = "01XXXXXXXXX", AccountHolderName = "GHCAA", Instructions = "Send money (Cash Out/Send Money) to the Nagad number shown. Screenshot your confirmation.", RequiresReceipt = true, RequiresReference = true, SortOrder = 2 },
                new() { Method = PaymentMethod.Rocket, DisplayName = "Rocket", Description = "Pay via Rocket mobile wallet", Icon = "🟪", WalletNumber = "01XXXXXXXXX", AccountHolderName = "GHCAA", Instructions = "Send money to the Rocket number shown. Keep your transaction reference.", RequiresReceipt = true, RequiresReference = true, SortOrder = 3 },
                new() { Method = PaymentMethod.BankTransfer, DisplayName = "Bank Transfer", Description = "Direct bank deposit or online transfer", Icon = "🏦", BankName = "Your Bank", BranchName = "Main Branch", AccountNumber = "XXXXXXXXX", AccountHolderName = "GHCAA", Instructions = "Transfer to the bank account shown. Attach deposit slip as proof.", RequiresReceipt = true, RequiresReference = true, SortOrder = 4 },
                new() { Method = PaymentMethod.ManualReceipt, DisplayName = "Cash / manual Receipt", Description = "Direct in-person payment acknowledgment", Icon = "🧾", Instructions = "Pay in person and upload the scan of your receipt/acknowledgment slip.", RequiresReceipt = true, RequiresReference = false, SortOrder = 5 },
                new() { Method = PaymentMethod.CreditCard, DisplayName = "Electronic Gateway", Description = "Pay securely via Card/Net banking (SSLCommerz)", Icon = "💳", Gateway = PaymentGateway.SSLCommerz, Instructions = "You will be redirected to a secure payment hub to complete your transaction.", RequiresReceipt = false, RequiresReference = false, IsEnabled = false, SortOrder = 6 }
            };

            await _db.PaymentConfigurations.AddRangeAsync(defaults, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return defaults;
        }

        public Task<PaymentConfiguration?> GetEnabledByGatewayAsync(PaymentGateway gateway, CancellationToken cancellationToken = default)
            => _db.PaymentConfigurations.FirstOrDefaultAsync(p => p.Gateway == gateway && p.IsEnabled, cancellationToken);
    }
}
