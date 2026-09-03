# Backend regression review, 3 July 2026

**Date:** 2026-07-03  
**Scope:** Full backend regression after Phases 2–3 parallel workstreams (API, Web, Mobile). All changes uncommitted — review before commit.

---

## Executive summary

Phase 4 re-ran the dependency loop (restore → build → test → HTTP smoke) and confirmed **no backend test regressions**. The API test suite grew from **305 → 308** passing tests (3 new tests from Phase 3: `ForumServiceTests` ×2, auth lockout ×1). Live smoke checks on `:5087` confirm `/healthz`, `/api/config`, forum CRUD (authenticated), and SignalR hubs (Chat + Notification) all work after BUG-001 and BUG-002 fixes.

**Regression status:** **PASS** — 308 passed, 0 failed, 1 intentionally skipped (`SyncMembersForReal`).

**Build note:** `dotnet build GHCAA.sln` fails while `GHCAA.API` is running (DLL file lock, MSB3027). Stop the API process before rebuilding, or build with the API already stopped.

---

## Fixes applied (uncommitted)

| ID | Area | Problem | Fix | Files |
|---|---|---|---|---|
| **BUG-001** | Org config | `GET /api/config` → 500 (`column o.RowVersion does not exist`) | EF migration adds `RowVersion` to `OrganizationConfig` | `20260703123040_AddOrganizationConfigRowVersion.cs`, model snapshot |
| **BUG-002** | SignalR | JWT query-token rejected on `/api/hubs/*` (401) | Accept token on both `/hubs` and `/api/hubs` prefixes | `GHCAA.API/Extensions/ServiceExtensions.cs` L44 |
| **ENV-001/004** | Local DB | Root `.env` pointed at remote Render DB | Renamed `.env` → `.env.remote` for local Development profile | `.env` deleted, `.env.remote` added |
| **ENV-003** | Migrations | 12 pending + RowVersion migration | Applied via `dotnet ef database update --context PgSqlApplicationDbContext` | Migration files |
| **ENV-005** | Test creds | `demo_user`, `shalin` login 401 | `HashGen --apply` + SQL password script | `HashGen/Program.cs`, `scripts/fix-local-test-passwords.*` |
| **nuget.config** | Restore | Corporate/offline feed issues | Repo-level config clearing sources to nuget.org only | `nuget.config` (new) |
| **HashGen** | Dev tooling | Manual password/forum seed | `--apply` (passwords + forum category), `--signalr-test` smoke | `HashGen/Program.cs`, `HashGen.csproj` |
| **Forum tests** | Coverage gap D4 | No forum unit tests | 2 service tests (create topic/post, list categories) | `GHCAA.Tests/Services/ForumServiceTests.cs` |
| **Lockout test** | Coverage gap F5 | No brute-force lockout unit test | `LoginAsync_AfterFiveFailedAttempts_ShouldLockOutForFifteenMinutes` | `GHCAA.Tests/Services/AuthServiceTests.cs` |
| **WEB-002** | Web proxy | E2E proxy → `:5000` (ECONNREFUSED) | Target → `:5087` | `GHCAA.Web/proxy.conf.json` |
| **WEB-003** | Playwright | API not started before E2E | Dual `webServer` (API + ng serve) | `GHCAA.Web/playwright.config.ts` |
| **WEB-004/005** | E2E auth | Wrong creds; missing storage file | Default login → `superadmin`; config spec anonymous | `auth-helper.ts`, multiple `tests/e2e/*.spec.ts` |
| **WEB-001** | Angular | Node &lt;20.19 blocked Angular 21 | Upgraded to Node v24.18.0 LTS | Environment (not in repo) |

---

## Test counts (Phase 4 run)

| Layer | Command | Pass | Fail | Skip | Notes |
|---|---|---:|---:|---:|---|
| **API** | `dotnet test GHCAA.sln --no-build` | **308** | 0 | 1 | 3m 46s; `SyncMembersForReal` skipped |
| **API build** | `dotnet build GHCAA.sln --configfile nuget.config` | — | 0 errors | — | 14 warnings (pre-existing nullable/duplicate usings) |
| **Mobile** | `flutter test` (Phase 3) | 28 | 0 | 0 | Visual freeze excluded |
| **Web unit** | `npm run test:unit` (Phase 3) | 230 | 0 | 0 | 58 Vitest files |
| **Web E2E** | Playwright `tests/e2e` (Phase 3) | 43 | 10 | 0 | Workflow specs still failing |
| **Web full E2E** | `npm run test:e2e` incl. visual (Phase 3) | 48 | 44 | 0 | Visual baselines stale |

**Phase 3 → Phase 4 delta:** +3 API tests (305 → 308); no existing tests broken.

---

## HTTP smoke (Phase 4, API on `:5087`)

| Endpoint | Result |
|---|---|
| `GET /healthz` | **200 Healthy** (Database, FileStorage, Email) |
| `GET /api/config` | **200** — `orgId=ghcaa` |
| `POST /api/auth/login` (`demo_user`) | **200** + JWT |
| `GET /api/forum/categories` (Bearer) | **200** — 1 category |
| SignalR (`HashGen --signalr-test`) | **Pass** — ChatHub + NotificationHub connected |

---

## Remaining environment blockers

| ID | Blocker | Impact | Mitigation |
|---|---|---|---|
| ENV-002 | `appsettings.json` placeholder `Password=CHANGE_ME` | None locally (Development overrides) | Expected; do not commit real secrets |
| MOB-BLOCK-001 | VS C++ workload missing | Flutter Windows integration E2E blocked | Install Desktop development with C++ |
| MOB-BLOCK-002/003 | No Android SDK / web integration unsupported | Mobile integration E2E blocked | Android Studio or fix Windows toolchain |
| WEB-008–010 | Admin workflow / article / full membership E2E | 10 functional Playwright failures | Empty queues or selector drift — separate fix pass |
| WEB-012 | Visual snapshot baselines | ~39 visual tests fail | `--update-snapshots` when UI stable |
| COV-001–004 | Negative-path unit tests | Coverage gaps only | Optional follow-up |

---

## Recommended commit grouping (for user review)

Split uncommitted work into **6 logical commits** to keep review focused:

### Commit 1 — `fix(api): org config RowVersion + SignalR JWT path`
- `GHCAA.Infrastructure/Data/Migrations/PgSql/20260703123040_*`
- `GHCAA.Infrastructure/Data/Migrations/PgSqlApplicationDbContextModelSnapshot.cs`
- `GHCAA.API/Extensions/ServiceExtensions.cs`

### Commit 2 — `chore(tooling): local dev passwords, forum seed, nuget restore`
- `nuget.config`
- `HashGen/Program.cs`, `HashGen/HashGen.csproj`
- `scripts/fix-local-test-passwords.sql`, `scripts/fix-local-test-passwords.ps1`
- `.env.remote` (document rename; **do not** commit `.env` with secrets)

### Commit 3 — `test(api): forum service + auth lockout coverage`
- `GHCAA.Tests/Services/ForumServiceTests.cs`
- `GHCAA.Tests/Services/AuthServiceTests.cs`

### Commit 4 — `fix(web): E2E infra — proxy, webServer, auth helpers`
- `GHCAA.Web/proxy.conf.json`
- `GHCAA.Web/playwright.config.ts`
- `GHCAA.Web/tests/e2e/**` (spec + auth-helper changes)

### Commit 5 — `fix(web): org-config and route alignment` *(if app changes are intentional)*
- `GHCAA.Web/src/app/admin/org-config/*`
- `GHCAA.Web/src/app/app.routes.ts`
- `GHCAA.Web/src/app/core/constants/app.constants.ts`

### Commit 6 — `chore(mobile): integration creds + config` *(optional, separate from backend)*
- `GHCAA.Mobile/integration_test/*`
- `GHCAA.Mobile/lib/core/config/app_config.dart`
- Exclude `test/failures/*.png` and generated plugin registrant noise unless needed

### Commit 7 — `docs: business review findings and Phase 4 summary`
- `docs/BUSINESS_FINDINGS.md`
- `docs/BUSINESS_TEST_CHECKLIST.md`
- `docs/BACKEND_REVIEW_2026-07-03.md`
- `docs/BUSINESS_REVIEW_PLAN.md`

**Exclude from all commits:** `build_current.txt`, upload stubs under `wwwroot/uploads/members/`, visual diff PNGs under `GHCAA.Mobile/test/failures/`.

---

## Verification commands (repeatable)

```powershell
# Restore + build (stop API first if running)
dotnet restore GHCAA.sln --configfile nuget.config
dotnet build GHCAA.sln --configfile nuget.config

# Full API regression
dotnet test GHCAA.sln --no-build

# HTTP smoke (API running on :5087)
Invoke-RestMethod http://localhost:5087/healthz
Invoke-RestMethod http://localhost:5087/api/config
dotnet run --project HashGen -- --signalr-test
```

---

## Conclusion

Backend consolidation is **complete for review**: all automated API tests pass, critical runtime endpoints verified, and fixes are documented with a suggested commit plan. Web E2E workflow failures and Mobile integration tooling remain **out of scope** for this backend workstream and should be addressed in follow-up passes before release.
