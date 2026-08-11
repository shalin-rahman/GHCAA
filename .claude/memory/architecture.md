---
name: GHCAA architecture and layer conventions
description: Clean Architecture layer rules, DI wiring, EF patterns, Angular structure, and cross-cutting conventions used throughout the codebase
type: project
---

## Clean Architecture Layers

```
GHCAA.Domain         — Models + Enums only. No dependencies on other layers.
GHCAA.Application    — Interfaces (I*Service) + DTOs + Validators. References Domain only.
GHCAA.Infrastructure — Implements Application interfaces. Has EF Core, external services, gateways.
GHCAA.API            — Controllers, Middleware, Hubs. References Application + Infrastructure (DI).
GHCAA.Tests          — NUnit + Moq. SQLite in-memory via ControllerTestBase.
GHCAA.Web            — Angular 21 standalone components, Signals-based state.
GHCAA.Mobile         — Flutter (Dart). Feature-based folder structure.
```

**Golden rule:** Infrastructure never exposes EF entities to the API layer directly — always project to DTOs or anonymous objects. Controllers should not know about `ApplicationDbContext` in general; the PaymentConfigController is a legacy exception.

---

## DI Registration

- `builder.Services.AddApplication()` → [GHCAA.Application/DependencyInjection.cs] — registers validators, FluentValidation
- `builder.Services.AddInfrastructure(configuration)` → [GHCAA.Infrastructure/DependencyInjection.cs] — registers all I*Service → *Service mappings, DbContext, HttpClients, gateways
- Payment gateways registered as `IPaymentGatewayService` named instances, resolved via `IPaymentGatewayFactory`

---

## EF Core Conventions

- Every entity needs an `IEntityTypeConfiguration<T>` in `GHCAA.Infrastructure/Data/Configurations/`
- All configurations are auto-discovered by `ApplicationDbContext.OnModelCreating` via `modelBuilder.ApplyConfigurationsFromAssembly`
- Soft-delete pattern: `HasQueryFilter(x => !x.IsArchived)` on Member and User — always use `.IgnoreQueryFilters()` when intentionally reading archived records
- Migrations: `dotnet ef migrations add <Name> --project GHCAA.Infrastructure --startup-project GHCAA.API`
- **Pending migration needed:** `User.FailedLoginAttempts`, `User.LockoutUntil` columns and new indexes on User (MemberId, ResetToken, GoogleId, FacebookId) + Otp (Email, ExpiryAt)

---

## API Layer Conventions

- All controllers inherit `ControllerBase`, use `[ApiController]` + `[Route("api/...")]`
- JSON: enum serialized as string (`JsonStringEnumConverter`), dates as `dd-MM-yyyy` (`DateFormatConverter`)
- Error handling: `ExceptionMiddleware` wraps all unhandled exceptions into consistent JSON error shape
- Middleware pipeline order (important):
  1. `UseCors`
  2. `UseResponseCompression` / `UseOutputCache`
  3. `UseHttpsRedirection` (prod only)
  4. `UseMiddleware<ExceptionMiddleware>`
  5. `UseMiddleware<SecurityHeadersMiddleware>` ← must be before UseStaticFiles
  6. `UseMiddleware<AuditLogMiddleware>`
  7. `UseRateLimiter`
  8. `UseWebSockets`
  9. `UseStaticFiles` (wwwroot + uploads alias)
  10. `UseMiddleware<QueryStringTokenMiddleware>`
  11. `UseAuthentication`
  12. `UseMiddleware<VisualTestAuthMiddleware>` (dev + Visual profile only)
  13. `UseMiddleware<SecurityStampMiddleware>`
  14. `UseAuthorization`
  15. `MapControllers`, `MapHub<>`, `MapHealthChecks`

---

## Rate Limiting Policies

| Policy name | Window | Limit | Used on |
|---|---|---|---|
| `auth` | 1 min | 5 (500 in dev) | Login, OTP, password reset |
| `registration` | 5 min | 10 (1000 in dev) | Registration submit |
| `api` | 1 min | 100 (10000 in dev) | General API |

Applied via `[EnableRateLimiting("auth")]` on controller/action.

---

## Auth / JWT

- HS256 signing via `JwtSigningKeyResolver.Resolve(config, env)` — throws in non-dev if key missing
- Token carries claims: `nameid` (UserId), `unique_name` (Username), `MemberId`, `role` (single string — highest privilege)
- `SecurityStampMiddleware` validates stamp on every authenticated request → instant session revocation
- `[Authorize(Policy = "AdminOnly")]` = Admin or SuperAdmin; `[Authorize(Policy = "SuperAdminOnly")]` = SuperAdmin only

---

## SignalR Hubs

- `ChatHub` (`/api/hubs/chat`) — [Authorize] required; ConcurrentDictionary<userId, connectionId> for online tracking
- `NotificationHub` (`/api/hubs/notifications`) — [Authorize] required; group-based broadcasts (batch year, department)
- `IRealTimeService` → `RealTimeService` (API layer) wraps hub context for service-layer use

---

## Angular Architecture

- **State:** Angular Signals (`signal()`, `computed()`) — no NgRx/BehaviorSubject in new code
- **HTTP:** Functional interceptors (`globalHttpInterceptor`) — attaches JWT only for `/api/` requests
- **Guards:** `authGuard` (checks auth + mustChangePassword), `adminGuard`, `superAdminGuard` — all functional
- **API calls:** Always use `API_ENDPOINTS` constants from `app.constants.ts`, never hardcode URLs
- **Images:** Always prefix with `environment.apiUrl` via the shared image-path utility
- **Date format:** `dd-MM-yyyy` everywhere — use `AppUtils.formatDate()` / `AppUtils.parseDate()`
- **Auth storage:** JWT + user object in `localStorage` under key `user_session` (TODO 24.39: move to httpOnly cookie)

---

## Testing Conventions

- Test class inherits `ControllerTestBase` which sets up SQLite in-memory `ApplicationDbContext`
- Helper methods: `CreateAndSaveTestMemberAsync()`, `SetMemberContext()`, `SetAdminContext()`, `SetSuperAdminContext()`
- Always use `CancellationToken.None` in tests
- `ApplicationDbContext.IsSeedDisabled = true` in [SetUp] to prevent test interference from data seeders
- Test build command: `dotnet build GHCAA.sln --no-restore`
- Test run command: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --no-build`
- Current count: **291 tests, all passing**

---

## Flutter Mobile Architecture

- Feature-based folders under `lib/features/`
- Riverpod for state (providers, AsyncValue)
- `ApiService` base class handles auth headers and 401 redirect
- `AppUtils.parseDate()` / `AppUtils.formatDate()` for dd-MM-yyyy handling
- `kIsWeb` guard required before any `local_auth` (biometric) calls
- Fastlane + GitHub Actions CI/CD for preprod and production builds
