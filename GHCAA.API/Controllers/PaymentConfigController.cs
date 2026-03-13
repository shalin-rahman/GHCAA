using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/payment-config")]
    public class PaymentConfigController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public PaymentConfigController(ApplicationDbContext db)
        {
            _db = db;
        }

        // PUBLIC: Get enabled payment methods (for registration forms)
        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActivePaymentMethods(CancellationToken cancellationToken)
        {
            var methods = await _db.PaymentConfigurations
                .Where(p => p.IsEnabled)
                .OrderBy(p => p.SortOrder)
                .Select(p => new
                {
                    p.Id,
                    p.Method,
                    p.DisplayName,
                    p.Description,
                    p.Icon,
                    p.WalletNumber,
                    p.AccountHolderName,
                    p.BankName,
                    p.BranchName,
                    p.AccountNumber,
                    p.RoutingNumber,
                    p.Instructions,
                    p.RequiresReceipt,
                    p.RequiresReference,
                    p.SortOrder,
                    p.Gateway
                })
                .ToListAsync(cancellationToken);

            return Ok(methods);
        }

        // ADMIN: Get all payment configs
        [HttpGet("admin/all")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllConfigs(CancellationToken cancellationToken)
        {
            var configs = await _db.PaymentConfigurations
                .OrderBy(p => p.SortOrder)
                .ToListAsync(cancellationToken);
            return Ok(configs);
        }

        // ADMIN: Create new payment config
        [HttpPost("admin")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateConfig([FromBody] PaymentConfiguration config, CancellationToken cancellationToken)
        {
            config.CreatedAt = DateTime.UtcNow;
            await _db.PaymentConfigurations.AddAsync(config, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return Ok(config);
        }

        // ADMIN: Update payment config
        [HttpPut("admin/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateConfig(int id, [FromBody] PaymentConfiguration config, CancellationToken cancellationToken)
        {
            var existing = await _db.PaymentConfigurations.FindAsync(new object[] { id }, cancellationToken);
            if (existing == null) return NotFound();

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
            existing.GatewayPublicKey = config.GatewayPublicKey;
            existing.GatewaySecretKey = config.GatewaySecretKey;
            existing.GatewayCallbackUrl = config.GatewayCallbackUrl;
            existing.SortOrder = config.SortOrder;
            existing.Instructions = config.Instructions;
            existing.RequiresReceipt = config.RequiresReceipt;
            existing.RequiresReference = config.RequiresReference;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return Ok(existing);
        }

        // ADMIN: Toggle enable/disable
        [HttpPost("admin/{id}/toggle")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ToggleConfig(int id, CancellationToken cancellationToken)
        {
            var config = await _db.PaymentConfigurations.FindAsync(new object[] { id }, cancellationToken);
            if (config == null) return NotFound();

            config.IsEnabled = !config.IsEnabled;
            config.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return Ok(new { config.Id, config.IsEnabled });
        }

        // ADMIN: Delete payment config
        [HttpDelete("admin/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteConfig(int id, CancellationToken cancellationToken)
        {
            var config = await _db.PaymentConfigurations.FindAsync(new object[] { id }, cancellationToken);
            if (config == null) return NotFound();

            _db.PaymentConfigurations.Remove(config);
            await _db.SaveChangesAsync(cancellationToken);
            return Ok();
        }

        // ADMIN: Seed default payment methods if none exist
        [HttpPost("admin/seed-defaults")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> SeedDefaults(CancellationToken cancellationToken)
        {
            if (await _db.PaymentConfigurations.AnyAsync(cancellationToken))
                return BadRequest("Payment configurations already exist.");

            var defaults = new List<PaymentConfiguration>
            {
                new() { Method = Domain.Enums.PaymentMethod.BKash, DisplayName = "bKash", Description = "Pay via bKash mobile wallet", Icon = "🟥", WalletNumber = "01XXXXXXXXX", AccountHolderName = "GHCAA", Instructions = "Send money to the bKash number shown. Use your Registration ID as reference.", RequiresReceipt = true, RequiresReference = true, SortOrder = 1 },
                new() { Method = Domain.Enums.PaymentMethod.Nagad, DisplayName = "Nagad", Description = "Pay via Nagad mobile wallet", Icon = "🟧", WalletNumber = "01XXXXXXXXX", AccountHolderName = "GHCAA", Instructions = "Send money to the Nagad number shown. Screenshot your confirmation.", RequiresReceipt = true, RequiresReference = true, SortOrder = 2 },
                new() { Method = Domain.Enums.PaymentMethod.Rocket, DisplayName = "Rocket", Description = "Pay via Rocket mobile wallet", Icon = "🟪", WalletNumber = "01XXXXXXXXX", AccountHolderName = "GHCAA", Instructions = "Send money to the Rocket number shown.", RequiresReceipt = true, RequiresReference = true, SortOrder = 3 },
                new() { Method = Domain.Enums.PaymentMethod.BankTransfer, DisplayName = "Bank Transfer", Description = "Direct bank deposit or online transfer", Icon = "🏦", BankName = "Your Bank", BranchName = "Main Branch", AccountNumber = "XXXXXXXXX", AccountHolderName = "GHCAA", Instructions = "Transfer to the bank account shown. Attach deposit slip.", RequiresReceipt = true, RequiresReference = true, SortOrder = 4 },
                new() { Method = Domain.Enums.PaymentMethod.ManualReceipt, DisplayName = "Cash / Manual Receipt", Description = "Pay in cash and upload receipt", Icon = "🧾", Instructions = "Pay in person and upload your receipt/acknowledgment slip.", RequiresReceipt = true, RequiresReference = false, SortOrder = 5 },
                new() { Method = Domain.Enums.PaymentMethod.CreditCard, DisplayName = "Credit/Debit Card", Description = "Pay securely with Visa/Mastercard via SSLCommerz", Icon = "💳", Gateway = Domain.Enums.PaymentGateway.SSLCommerz, Instructions = "You will be redirected to a secure payment page.", RequiresReceipt = false, RequiresReference = false, IsEnabled = false, SortOrder = 6 }
            };

            await _db.PaymentConfigurations.AddRangeAsync(defaults, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return Ok(defaults);
        }
    }
}
