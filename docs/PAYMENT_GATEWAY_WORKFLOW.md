# Payment Gateway Integration Workflow

This document outlines the mandatory workflow for integrating new payment gateways into the GHCAA platform.

## 1. Data-First Configuration
All gateway credentials MUST be stored in the `PaymentConfigurations` table, NOT in `appsettings.json`.

### Table Mapping:
| Gateway Field | `PaymentConfiguration` Column |
|---|---|
| Merchant ID / Client ID | `GatewayPublicKey` |
| Secret Key / Client Secret | `GatewaySecretKey` |
| API Key / Integration Key | `WalletNumber` |
| Callback URL | `GatewayCallbackUrl` |

### Seeding:
Add the UAT/Production credentials to `GHCAA.Infrastructure/Data/Seed/payment_configurations.json`.

## 2. Implementation Pattern (`BasePaymentGateway` & `IPaymentGatewayService`)
Gateways must inherit from `BasePaymentGateway` (which implements `IPaymentGatewayService`) and be registered with an `HttpClient`.

### Base Class Capabilities:
- `GetActiveConfigAsync()`: Automatically retrieves the active gateway configuration from `ApplicationDbContext`.
- Standardizes sandbox / live URL resolution and credential retrieval.

### Code Structure:
```csharp
public class CustomGateway : BasePaymentGateway
{
    public CustomGateway(HttpClient http, ApplicationDbContext db, IConfiguration config, ILogger<CustomGateway> logger)
        : base(http, db, config, logger, PaymentGateway.Custom)
    {
    }

    public override async Task<PaymentGatewayResponseDto> InitiatePaymentAsync(PaymentInitiationDto dto, CancellationToken ct = default)
    {
        var dbConfig = await GetActiveConfigAsync(ct);
        // Use dbConfig.GatewayPublicKey, dbConfig.GatewaySecretKey, etc.
    }
}
```

## 3. Registration & Callback Workflow
1. **Enum**: Add the new gateway to `GHCAA.Domain.Enums.PaymentGateway`.
2. **Infrastructure**:
    - Add the gateway implementation in `GHCAA.Infrastructure/Gateways/` inheriting `BasePaymentGateway`.
    - Register in `GHCAA.Infrastructure/DependencyInjection.cs` using `AddHttpClient<TGateway>()` and `AddScoped<IPaymentGatewayService, TGateway>()`.
3. **Callback Handling (`IPaymentCallbackOrchestrator`)**:
    - Centralized in `PaymentCallbackOrchestrator`.
    - In `GatewaysController.cs`, delegate callback routing to `IPaymentCallbackOrchestrator.ProcessCallbackAsync(...)`.
    - Handles verification, database ledger updates, member/event status transition, and redirect URL generation uniformly.
4. **Mobile**:
    - Update `PaymentService` in Flutter.
    - Add the gateway icon/option to the payment selection screen.

## 4. Security Requirements
- **HMAC Verification**: Always verify signatures/checksums on callbacks.
- **Server-to-Server Check**: Before finalizing any payment, perform a server-to-server status check if the gateway supports it.
- **AES/Encryption**: Sensitive data in callbacks must be handled according to the gateway's encryption standards (e.g., DGePay's AES-128-ECB).
- **No Secrets in Logs**: Never log full request/response bodies containing plain-text credentials or sensitive customer data.
