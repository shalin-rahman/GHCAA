---
name: GHCAA platform project context
description: Core stack, layer structure, and key conventions for the GHCAA alumni association platform
type: project
---

**Stack:** ASP.NET Core 9 Clean Architecture (Domain / Application / Infrastructure / API) + Angular 21 (Signals, standalone) + Flutter mobile + PostgreSQL via EF Core 9 + SignalR + FluentValidation + NUnit+Moq tests (291 total)

**Why:** Alumni association management system (GHCAA) — membership registration, payment processing, events, ID cards, certificates.

**Layer conventions:**
- Domain: models + enums only (GHCAA.Domain)
- Application: interfaces + DTOs + validators (GHCAA.Application)
- Infrastructure: EF DbContext + service implementations + gateways (GHCAA.Infrastructure)
- API: controllers + middleware + hubs (GHCAA.API)
- Tests: NUnit + Moq, SQLite in-memory DB (GHCAA.Tests)

**Key policy names:** AdminOnly, SuperAdminOnly (must sync with frontend adminGuard/superAdminGuard)

**Payment gateways:** bKash, Nagad, Rocket (manual), BankTransfer (manual), ManualReceipt, SSLCommerz (online)

**Rate limiting:** auth=5/min, registration=10/5min, api=100/min (relaxed in dev/Visual profile)

**Test runner:** `dotnet test GHCAA.Tests/GHCAA.Tests.csproj`; build: `dotnet build GHCAA.sln --no-restore`

**How to apply:** Use project_map.md before reading files. When adding a new entity, add a Configuration class in GHCAA.Infrastructure/Data/Configurations/ and register it in ApplicationDbContext.
