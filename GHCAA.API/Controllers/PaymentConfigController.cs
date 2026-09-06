using GHCAA.Domain.Models;
using GHCAA.Domain;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/payment-config")]
    public class PaymentConfigController : ControllerBase
    {
        private readonly IPaymentConfigService _paymentConfigService;

        public PaymentConfigController(IPaymentConfigService paymentConfigService)
        {
            _paymentConfigService = paymentConfigService;
        }

        // PUBLIC: Get enabled payment methods (for registration forms)
        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActivePaymentMethods(CancellationToken cancellationToken)
        {
            var configs = await _paymentConfigService.GetActiveMethodsAsync(cancellationToken);
            var methods = configs.Select(p => new
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
            });

            return Ok(methods);
        }

        // ADMIN: Get all payment configs
        [HttpGet("admin/all")]
        [Authorize(Policy = Constants.Policies.SuperAdminOnly)] // Strict role parity: Sync with frontend superAdminGuard
        public async Task<IActionResult> GetAllConfigs(CancellationToken cancellationToken)
        {
            var configs = await _paymentConfigService.GetAllAsync(cancellationToken);

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
        [Authorize(Policy = Constants.Policies.SuperAdminOnly)] // Strict role parity: Sync with frontend superAdminGuard
        public async Task<IActionResult> CreateConfig([FromBody] PaymentConfiguration config, CancellationToken cancellationToken)
        {
            var created = await _paymentConfigService.CreateAsync(config, cancellationToken);
            return Ok(MaskSecrets(created));
        }

        // ADMIN: Update payment config
        [HttpPut("admin/{id}")]
        [Authorize(Policy = Constants.Policies.SuperAdminOnly)] // Strict role parity: Sync with frontend superAdminGuard
        public async Task<IActionResult> UpdateConfig(int id, [FromBody] PaymentConfiguration config, CancellationToken cancellationToken)
        {
            // Only SuperAdmin can update gateway secrets
            var updated = await _paymentConfigService.UpdateAsync(id, config, allowSecretUpdate: User.IsInRole("SuperAdmin"), cancellationToken);
            if (updated == null) return NotFound();
            return Ok(MaskSecrets(updated));
        }

        // ADMIN: Toggle enable/disable
        [HttpPost("admin/{id}/toggle")]
        [Authorize(Policy = Constants.Policies.SuperAdminOnly)] // Strict role parity: Sync with frontend superAdminGuard
        public async Task<IActionResult> ToggleConfig(int id, CancellationToken cancellationToken)
        {
            var config = await _paymentConfigService.ToggleAsync(id, cancellationToken);
            if (config == null) return NotFound();
            return Ok(new { config.Id, config.IsEnabled });
        }

        // ADMIN: Delete payment config
        [HttpDelete("admin/{id}")]
        [Authorize(Policy = Constants.Policies.SuperAdminOnly)] // Strict role parity: Sync with frontend superAdminGuard
        [GHCAA.API.Filters.RequireStepUp]
        public async Task<IActionResult> DeleteConfig(int id, CancellationToken cancellationToken)
        {
            var deleted = await _paymentConfigService.DeleteAsync(id, cancellationToken);
            if (!deleted) return NotFound();
            return Ok();
        }

        private static object MaskSecrets(PaymentConfiguration c) => new
        {
            c.Id,
            c.Method,
            c.DisplayName,
            c.Description,
            c.Icon,
            c.IsEnabled,
            c.IsSandbox,
            c.SortOrder,
            c.Gateway,
            c.WalletNumber,
            c.AccountHolderName,
            c.BankName,
            c.BranchName,
            c.AccountNumber,
            c.RoutingNumber,
            c.Instructions,
            c.RequiresReceipt,
            c.RequiresReference,
            c.GatewayCallbackUrl,
            c.CreatedAt,
            c.UpdatedAt,
            GatewayPublicKey = string.IsNullOrEmpty(c.GatewayPublicKey) ? null : "••••••••",
            GatewaySecretKey = string.IsNullOrEmpty(c.GatewaySecretKey) ? null : "••••••••"
        };

        // ADMIN: Seed default payment methods if none exist
        [HttpPost("admin/seed-defaults")]
        [Authorize(Policy = Constants.Policies.SuperAdminOnly)] // Strict role parity: Sync with frontend superAdminGuard
        public async Task<IActionResult> SeedDefaults(CancellationToken cancellationToken)
        {
            if (await _paymentConfigService.HasAnyAsync(cancellationToken))
                return BadRequest("Payment configurations already exist.");

            var defaults = await _paymentConfigService.SeedDefaultsAsync(cancellationToken);
            return Ok(defaults);
        }
    }
}
