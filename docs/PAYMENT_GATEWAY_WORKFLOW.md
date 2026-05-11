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

## 2. Implementation Pattern (`IPaymentGatewayService`)
Gateways must implement `IPaymentGatewayService` and be registered with an `HttpClient`.

### Dependencies:
- `ApplicationDbContext`: Required to fetch credentials from the database.
- `IConfiguration`: Used ONLY for environment-specific URLs (Sandbox vs Production) or non-sensitive global settings.
- `HttpClient`: Always used for external API calls.

### Code Structure:
```csharp
public async Task<PaymentGatewayResponseDto> InitiatePaymentAsync(...) {
    var dbConfig = await _db.PaymentConfigurations
        .FirstOrDefaultAsync(p => p.Gateway == GatewayType && p.IsEnabled);
    // Use dbConfig.GatewayPublicKey, dbConfig.GatewaySecretKey, etc.
}
```

## 3. Registration Workflow
1. **Enum**: Add the new gateway to `GHCAA.Domain.Enums.PaymentGateway`.
2. **Infrastructure**:
    - Add the gateway implementation in `GHCAA.Infrastructure/Gateways/`.
    - Register in `GHCAA.Infrastructure/DependencyInjection.cs` using `AddHttpClient<TGateway>()` and `AddScoped<IPaymentGatewayService, TGateway>()`.
3. **Controller**:
    - Add a callback endpoint in `GatewaysController.cs`.
    - Ensure successful payments trigger `HandleSuccessfulPayment`.
4. **Mobile**:
    - Update `PaymentService` in Flutter.
    - Add the gateway icon/option to the payment selection screen.

## 4. Security Requirements
- **HMAC Verification**: Always verify signatures/checksums on callbacks.
- **Server-to-Server Check**: Before finalizing any payment, perform a server-to-server status check if the gateway supports it.
- **AES/Encryption**: Sensitive data in callbacks must be handled according to the gateway's encryption standards (e.g., DGePay's AES-128-ECB).
- **No Secrets in Logs**: Never log full request/response bodies containing plain-text credentials or sensitive customer data.
