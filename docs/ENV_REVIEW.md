# GHCAA Environment Configuration Review

> All values cross-referenced against actual code. No guesses.

---

## 1. API (Root `.env` files — loaded by `DotNetEnv.Env.Load()` in `Program.cs`)

### Three environments

| File | Loaded by | Purpose |
|---|---|---|
| [`.env`](../.env) | `DotNetEnv.Env.Load()` (always) | Local Development |
| [`.env.dev`](../.env.dev) | Manual rename to `.env` when needed | Dev alias (not loaded automatically) |
| [`.env.preprod`](../.env.preprod) | Render/CI env vars (not DotNetEnv) | Pre-production Render deployment |

> [!IMPORTANT]
> `DotNetEnv.Env.Load()` in `Program.cs` loads only `.env` (not `.env.preprod`). The preprod/production
> env vars must be set as **host environment variables** on Render — not committed as `.env` files.
> `.env.preprod` serves as a reference template for what to configure on Render.

### Variable Registry

| Variable | Read by | `.env` (Local) | `.env.preprod` (Render) |
|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core | `Development` | `Preprod` |
| `ORG_PROFILE` | [`InstitutionProfileProvider.cs`](../GHCAA.Infrastructure/Services/InstitutionProfileProvider.cs) | `ghc` (or `default`) | `ghc` |
| `DATABASE_URL` | [`DependencyInjection.cs`](../GHCAA.Infrastructure/DependencyInjection.cs) | *(blank — uses PgSqlConnection)* | Render postgres URL |
| `ConnectionStrings__PgSqlConnection` | `DependencyInjection.cs` | `Host=localhost;...` | *(not needed — DATABASE_URL used)* |
| `PORT` | [`Program.cs`](../GHCAA.API/Program.cs) | *(set by Render automatically)* | *(set by Render automatically)* |
| `Jwt__Key` | [`JwtSigningKeyResolver.cs`](../GHCAA.Application/Security/JwtSigningKeyResolver.cs) | *(blank = ephemeral dev key)* | **Required, ≥32 chars** |
| `OtpSettings__HashKey` | [`OtpService.cs`](../GHCAA.Infrastructure/Services/OtpService.cs) | *(blank = falls back to Jwt:Key)* | *(blank = falls back to Jwt:Key)* |
| `GmailSettings__Email` | [`GmailEmailService.cs`](../GHCAA.Infrastructure/Services/GmailEmailService.cs) | `dev-placeholder@localhost` | **Required** (throws on null) |
| `GmailSettings__AppPassword` | `GmailEmailService.cs` | `dev-not-used` | **Required** (throws on null) |
| `SmsSettings__Token` | [`GreenwebSmsService.cs`](../GHCAA.Infrastructure/Services/GreenwebSmsService.cs) | *(blank = SMS silently disabled)* | Fill to enable SMS |
| `PaymentGateways__Bkash__SandboxPassword` | [`BkashGateway.cs`](../GHCAA.Infrastructure/Gateways/BkashGateway.cs) | *(default `sandbox_pass`)* | `sandbox_pass` |
| `PaymentGateways__Bkash__ProductionPassword` | `BkashGateway.cs` | *(not needed in dev)* | Fill for prod Bkash |
| `AppSettings__AllowedOrigins__0..N` | `Program.cs` | `http://localhost:4200/1/5087` | `https://preprod.haragangian.com` |
| `AppSettings__ClientUrl` | `AppSettingsOptions` | `http://localhost:4200` | `https://preprod.haragangian.com` |
| `ASP_SEED_PROFILE` | `ApplicationDbContext.cs` | *(omit for standard seed)* | *(omit; set `Visual` for test-only DB)* |

> [!NOTE]
> **Removed as obsolete:**
> - `JwtSettings__Secret` — never read; code uses `Jwt__Key` only
> - `GeneralSettings__AssociationNamePrefix` — migrated to ORG_PROFILE / org-config.json
> - `PaymentGateways__DGePay__ClientId/Secret/ApiKey` — DGePayGateway reads from DB, not config
> - `IMAGE_BASE_URL` (mobile) — derived from `BASE_API_URL.replaceFirst('/api', '')`
> - `GATEWAY_SSLCOMMERZ/BKASH/AAMARPAY` (mobile) — gateways fetched from API `GET /api/payment-config/active`
> - `AppSettings__AllowedOrigins__*` wildcard syntax — invalid; must be `__0`, `__1`, etc.

---

## 2. Web (Angular — environment files)

Angular has **no `.env` files**. Config is baked in at build time via `fileReplacements` in [`angular.json`](../GHCAA.Web/angular.json).

| Environment | File | Build config | `apiUrl` |
|---|---|---|---|
| Local dev | [`environment.ts`](../GHCAA.Web/src/environments/environment.ts) | `ng serve` / `development` | `/api` (proxy via `proxy.conf.json` → `localhost:5087`) |
| Pre-production | [`environment.preprod.ts`](../GHCAA.Web/src/environments/environment.preprod.ts) | `ng build --configuration preprod` | `/api` (relative — same-origin Docker) |
| Production | [`environment.prod.ts`](../GHCAA.Web/src/environments/environment.prod.ts) | `ng build --configuration production` | `https://haragangian.com/api` |

---

## 3. Mobile (Flutter — `.env` files loaded by `flutter_dotenv`)

Mobile has **two** `.env` files. Only `.env` is bundled as a Flutter asset (declared in [`pubspec.yaml`](../GHCAA.Mobile/pubspec.yaml)). To use `.env.preprod`, swap it in before building.

| File | Used for |
|---|---|
| [`GHCAA.Mobile/.env`](../GHCAA.Mobile/.env) | Local development |
| [`GHCAA.Mobile/.env.preprod`](../GHCAA.Mobile/.env.preprod) | Pre-production / Play Store internal track |

### Variable Registry

| Variable | Read by | `.env` | `.env.preprod` |
|---|---|---|---|
| `BASE_API_URL` | [`AppConfig.dart`](../GHCAA.Mobile/lib/core/config/app_config.dart) | `http://10.0.2.2:5087/api` | `https://preprod.haragangian.com/api` |
| `ENVIRONMENT` | `AppConfig.dart` | `development` | `preprod` |
| `APP_VERSION` | `AppConfig.dart` | `1.0.0+1 (Dev)` | `1.0.0-preprod` |
| `SENTRY_DSN` | [`main.dart`](../GHCAA.Mobile/lib/main.dart) | *(blank = Sentry offline)* | Fill for observability |

> [!IMPORTANT]
> **Branding fallback is unified.** `AppConfig` branding getters (`appName`, `organizationName`, etc.)
> fall back to `OrgConfig.offlineDefaults` (which reads from the profile pack), so `.env` files only need infrastructure keys.

---

## 4. Docker / Deployment

### `.dockerignore` — Updated
All `.env*` files are excluded from the build context via `**/.env*`.
Secrets flow through **Render environment variables only** — never baked into the image.

### Render Service Environment Variables (must be configured in Render dashboard)
Set these for the **API service** on Render (preprod):

```
ASPNETCORE_ENVIRONMENT=Preprod
ORG_PROFILE=ghc
DATABASE_URL=<Render Postgres Internal URL>
Jwt__Key=<production-secret-32-chars>
GmailSettings__Email=<smtp-email>
GmailSettings__AppPassword=<smtp-app-password>
AppSettings__AllowedOrigins__0=https://preprod.haragangian.com
AppSettings__ClientUrl=https://preprod.haragangian.com
DOTNET_hostBuilder__reloadConfigOnChange=false
```

---

## 5. Default Dev Environment (`ORG_PROFILE=default`) & Demo Data

When deploying or running locally with `ORG_PROFILE=default`:

### Profile Branding
- **Name**: Sample Alumni Association
- **Acronym**: SAA
- **Membership Prefix**: `MEM-`
- **Currency**: BDT / ৳

### Sample Accounts & Access Matrix

| Username | Role Access | Member Name | Membership No | Email | Profile State |
|---|---|---|---|---|---|
| `demo.admin` | `Admin`, `Member` | Ava Sample | `MEM-0000000001` | `demo.admin@example.org` | EC President, Active |
| `demo.member2` | `Admin`, `Member` | Ben Sample | `MEM-0000000002` | `demo.member2@example.org` | EC General Secretary, Active |
| `demo.member3` | `Member` | Clara Sample | `MEM-0000000003` | `demo.member3@example.org` | EC Treasurer, Associate Member |
| `demo.member4` | `Member` | David Sample | `MEM-0000000004` | `demo.member4@example.org` | Advisory Council, Life Patron |

### Available Seed Data Entities (`profiles/default/demo-data/*.json`)

1. **Members (`members.json`)**: Coherent member records spanning Founding, General, Associate, and Life Patron categories.
2. **Users & User Roles (`users.json`, `user_roles.json`)**: Ready-to-login accounts with Admin & Member roles.
3. **Academic & Professional Records (`academic_records.json`, `professional_records.json`)**: Degree histories, CSE/BBA majors, and tech/finance career records.
4. **Governance (`ec_periods.json`, `ec_members.json`)**: 2024-2026 Executive Committee term and position assignments.
5. **Events & News (`events.json`, `news.json`)**: Annual Reunion 2025, Mentorship Forum, and Portal Announcements.
6. **Galleries & Media (`galleries.json`, `photos.json`)**: Annual Reunion and Inauguration photo galleries.
7. **Financial Ledger & Dues (`financial_records.json`, `membership_dues.json`, `payment_histories.json`)**: Income/Expense records, membership fee dues (paid & pending), and payment transaction histories.

---

### Build arg: `ORG_PROFILE`
Dockerfile accepts `--build-arg ORG_PROFILE=ghc` (default `ghc`). This controls which profile pack
drives the Angular build. For a different institution: `docker build --build-arg ORG_PROFILE=<name>`.

---

## 5. Lookups & Seeding Model
- Lookups are seeded exclusively via EF Core baseline migrations (`InitialBaseline`) and managed at runtime via admin CRUD API (`/api/lookups`).
- Initial OrgConfig defaults are seeded dynamically from the active profile (`profiles/<name>/org-config.json` or `profiles/default/org-config.json`) on first boot when the DB is clean.
