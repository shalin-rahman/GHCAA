using GHCAA.Domain.Models;
using GHCAA.Domain;
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
                    p.Gateway,
                    IsOnline = p.Gateway != Enums.PaymentGateway.None
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

            // Obfuscate secrets for non-superadmins
            if (!User.IsInRole("SuperAdmin"))
            {
                foreach (var config in configs)
                {
                    if (!string.IsNullOrEmpty(config.GatewaySecretKey))
                        config.GatewaySecretKey = "********";
                    if (!string.IsNullOrEmpty(config.GatewayPublicKey))
                        config.GatewayPublicKey = "********";
                }
            }
            
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
            
            // Only SuperAdmin can update gateway secrets
            if (User.IsInRole("SuperAdmin"))
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
                new() { Method = Enums.PaymentMethod.BKash, DisplayName = "bKash", Description = "Pay via bKash mobile wallet", Icon = "💖", WalletNumber = "01XXXXXXXXX", AccountHolderName = "GHCAA", Instructions = "Send money to the bKash number shown. Use your Registration ID as reference.", RequiresReceipt = true, RequiresReference = true, SortOrder = 1 },
                new() { Method = Enums.PaymentMethod.Nagad, DisplayName = "Nagad", Description = "Pay via Nagad mobile wallet", Icon = "🟧", WalletNumber = "01XXXXXXXXX", AccountHolderName = "GHCAA", Instructions = "Send money (Cash Out/Send Money) to the Nagad number shown. Screenshot your confirmation.", RequiresReceipt = true, RequiresReference = true, SortOrder = 2 },
                new() { Method = Enums.PaymentMethod.Rocket, DisplayName = "Rocket", Description = "Pay via Rocket mobile wallet", Icon = "🟪", WalletNumber = "01XXXXXXXXX", AccountHolderName = "GHCAA", Instructions = "Send money to the Rocket number shown. Keep your transaction reference.", RequiresReceipt = true, RequiresReference = true, SortOrder = 3 },
                new() { Method = Enums.PaymentMethod.BankTransfer, DisplayName = "Bank Transfer", Description = "Direct bank deposit or online transfer", Icon = "🏦", BankName = "Your Bank", BranchName = "Main Branch", AccountNumber = "XXXXXXXXX", AccountHolderName = "GHCAA", Instructions = "Transfer to the bank account shown. Attach deposit slip as proof.", RequiresReceipt = true, RequiresReference = true, SortOrder = 4 },
                new() { Method = Enums.PaymentMethod.ManualReceipt, DisplayName = "Cash / manual Receipt", Description = "Direct in-person payment acknowledgment", Icon = "🧾", Instructions = "Pay in person and upload the scan of your receipt/acknowledgment slip.", RequiresReceipt = true, RequiresReference = false, SortOrder = 5 },
                new() { Method = Enums.PaymentMethod.CreditCard, DisplayName = "Electronic Gateway", Description = "Pay securely via Card/Net banking (SSLCommerz)", Icon = "💳", Gateway = Enums.PaymentGateway.SSLCommerz, Instructions = "You will be redirected to a secure payment hub to complete your transaction.", RequiresReceipt = false, RequiresReference = false, IsEnabled = false, SortOrder = 6 }
            };

            await _db.PaymentConfigurations.AddRangeAsync(defaults, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return Ok(defaults);
        }
    }
}
