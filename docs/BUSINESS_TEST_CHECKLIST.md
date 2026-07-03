# GHCAA Business Test Checklist

Use during Phases 2–4. Mark `[x]` when verified on the relevant layer(s). Note layer abbreviations: **API**, **Web**, **Mobile**.

**Findings:** Log failures in `docs/BUSINESS_FINDINGS.md`.

---

## Environment prerequisites

- [x] PostgreSQL port reachable on localhost:5432 (2026-07-03)
- [x] Migrations applied — 12 pending + `AddOrganizationConfigRowVersion` (2026-07-03)
- [x] API starts (`dotnet run --project GHCAA.API`) — 2026-07-03
- [x] `GET /healthz` returns **200 Healthy** (after `.env` → `.env.remote`)
- [x] `superadmin` login smoke test passes (2026-07-03)
- [x] `demo_user` / `shalin` login (fixed 2026-07-03 — `HashGen --apply`)
- [x] Angular dev server (optional for Web UI checks) — Playwright `webServer` starts ng serve (2026-07-03)
- [x] Flutter `pub get` + `analyze` clean (2026-07-03 Phase 3 Mobile)
- [x] Flutter core tests 28/28 pass (2026-07-03; visual freeze excluded)
- [ ] Flutter integration E2E (blocked: VS C++ workload + no Android SDK — MOB-BLOCK-001/003)
- [ ] Flutter app live E2E against local API (blocked by integration toolchain)

---

## A. Membership & Identity Lifecycle

### A1 — Registration wizard validation (Haraganga college record required)

- [x] **A1.1–A1.4, A1.7** API unit tests pass (Phase 1)
- [ ] **A1.5** Web 3-step wizard blocks step advance without GHC record (Web)
- [ ] **A1.6** Mobile registration enforces same rule (Mobile)

### A2 — OTP / uniqueness (NID, Mobile, Email)

- [x] **A2.1–A2.8** API unit tests pass (Phase 1)

### A3 — Admin approval → membership number

- [x] **A3.1–A3.7** API unit tests pass (Phase 1)
- [x] **A3.1** HTTP smoke: admin members list returns 584 (superadmin)

### A4 — Rejection soft-delete

- [x] **A4.1–A4.5** API unit tests pass (Phase 1)

### A5 — Profile completeness gate (13 fields)

- [x] **A5.1–A5.5** Logic verified + positive tests (Phase 1)
- [ ] **A5.6–A5.7** Web/Mobile UI (not tested)

### A6 — Registration fee payment requirement

- [x] **A6.1–A6.4** API unit tests pass (Phase 1)
- [ ] **A6.5** Admin payment status in portal (Web)

### A7 — Digital ID card

- [x] **A7.1–A7.2** `ProfileControllerTests.GetIDCard_ReturnsOk` (API controller layer)
- [ ] **A7.3** Non-active member gate (no dedicated test)

### A8 — Privacy toggles

- [x] **A8.1–A8.4** `NetworkingServiceTests.SearchMembersAsync_ShouldHonorPrivacyFlags` + masked profile tests (API)

### A9 — Family/spouse linking

- [x] **A9.1–A9.3** `FamilyLinkServiceTests` — request, approve, reject (API)

### A10 — Blue tick verification

- [x] **A10.1** `IsVerified=true` on approval (code + EC tests)
- [ ] **A10.2** Badge visible in directory (Web/Mobile)

### A11 — Social login

- [x] **A11.1** `AuthServiceTests.SocialLoginAsync_WithValidGoogleId` (API)
- [ ] **A11.2** Incomplete profile onboarding wizard (Web/Mobile)

---

## B. Events & Participation

- [x] **B1–B3, B5** API tests pass (`EventServiceTests`, `EventsControllerTests`, workflow)
- [ ] **B4** QR attendance scan (no isolated test; manual/Web)

---

## C. Financial Governance

- [x] **C1–C5** API tests pass (`FinancialServiceTests`, `FinancialLedger*`, `GatewaysControllerTests`, `PaymentConfigControllerTests`)

---

## D. Networking & Social

- [x] **D1, D3, D6** API tests pass (Networking, JobHub, Mentorship, Poll)
- [x] **D2** Real-time messaging (SignalR — `HashGen --signalr-test`; BUG-002 JWT path fixed)
- [x] **D4** Discussion forums (`ForumServiceTests` 2/2 + HTTP smoke 2026-07-03)
- [x] **D5** Haraganga AI assistant (HTTP smoke — local NLP; no Gemini key)

---

## E. Governance & CMS

- [x] **E1–E5** API tests pass (`GovernanceServiceTests`, `NewsControllerTests`, `CommunicationServiceTests`, `GalleryControllerTests`)

---

## F. Security & Session

- [x] **F1, F3, F4** Partial — login 401/200, inactive user blocked (`AuthServiceTests`, `AuthControllerTests`, `TokenServiceTests`)
- [x] **F2** 10-minute inactivity logout (client-only — Web/Mobile idle timers; no API)
- [x] **F5** Brute-force lockout (`AuthServiceTests.LoginAsync_AfterFiveFailedAttempts_*` + HTTP 2026-07-03)

---

## Cross-platform parity (Phase 3)

| Area | API contract OK | Web | Mobile | Notes |
|---|---|---|---|---|
| Notifications | [x] | [x] | [x] | E2E admin/member nav verified; SignalR not browser-tested |
| Ledger | [x] | [x] | [x] | Web admin ledger E2E pass (`admin-panels` superadmin); Mobile member path |
| Directory | [x] | [x] | [x] | Web `alumni-directory.spec.ts` 4/4 pass |
| Forum | [x] | [ ] | [x] | API + Mobile verified; Phase 4 HTTP smoke 1 category OK |
| Governance | [x] | [x] | [x] | Web `governance.spec.ts` 3/3 pass |
| Org config | [x] | [x] | [x] | Web `config-regression.spec.ts` 2/2 pass |

---

## Automated test runs (log date + result)

| Date | Filter / scope | Pass | Fail | Skip | Phase 4 status | Command |
|---|---|---:|---:|---:|---|---|
| 2026-07-03 | A1–A6 membership tests | 92 | 0 | 0 | Unchanged | Phase 1 filter |
| 2026-07-03 | A7–F Phase 2 suites | 150 | 0 | 0 | Unchanged | Profile+Family+Networking+Events+Financial+Governance+Auth |
| 2026-07-03 | Full regression (Phase 2) | 305 | 0 | 1 | Superseded | `dotnet test GHCAA.Tests` |
| 2026-07-03 | **Full regression (Phase 4)** | **308** | **0** | **1** | **✅ No regressions** | `dotnet test GHCAA.sln --no-build` |
| 2026-07-03 | Flutter core (Phase 3 Mobile) | 28 | 0 | 0 | Not re-run P4 | `flutter test` (6 files, visual freeze excluded) |
| 2026-07-03 | ForumServiceTests | 2 | 0 | 0 | Included in 308 | Phase 3 API gaps |
| 2026-07-03 | AuthServiceTests lockout (F5) | 1 | 0 | 0 | Included in 308 | Phase 3 API gaps |
| 2026-07-03 | Vitest unit (Phase 3 Web) | 230 | 0 | 0 | Not re-run P4 | `npm run test:unit` GHCAA.Web |
| 2026-07-03 | Playwright full `npm run test:e2e` (Phase 3 Web) | 48 | 44 | 0 | Not re-run P4 | `npm run test:e2e` 92 tests, 15.1m |
| 2026-07-03 | HTTP smoke (Phase 4) | 5 | 0 | 0 | **✅ Pass** | `/healthz`, `/api/config`, forum, SignalR |
