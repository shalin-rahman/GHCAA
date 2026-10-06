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

### A1 — Registration wizard validation (configured institution record required)

- [x] **A1.1–A1.4, A1.7** API unit tests pass (Phase 1)
- [x] **A1.5** Web registration and profile flows use the configured institution record (Web)
- [x] **A1.6** Mobile registration and profile flows enforce the same rule (Mobile)
- [x] **A1.8** Empty history and later institutional records are rejected by focused service tests

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
- [ ] **A11.3** With `Features.EnableSocialAuth` off, or a provider row disabled or missing its client id, `GET /api/auth/providers` returns an empty list and the web login page shows no social buttons or "or" separator (API, Web). Tests: `SocialAuthConfigServiceTests`.
- [ ] **A11.4** With the provider usable, its button shows on web and mobile (Web/Mobile).

### A12 — Step-up code message and delivery failure

- [ ] **A12.1** Request a step-up code as an admin. The message names the masked email address (for example `s***@example.com`), not only "your registered email" (API, Web, Mobile).
- [ ] **A12.2** With the email provider failing, the request answers 503 and the screen shows the "try again in a few minutes" text (API, Web, Mobile).

### A13 — Hard delete of a user with election records

- [ ] **A13.1** As SuperAdmin, delete a system-admin user who holds an appointment, an approval or a rules unlock. The API answers 409 "has election records, deactivate instead" and the user stays (API, Web). Tests: `UserServiceTests`, `RolesControllerTests`.
- [ ] **A13.2** Delete a system-admin user with no election records. It is removed (API, Web).

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

### E6 — Election officials, rules freeze and rules unlock

Manual checks for the screens added with spec 023 FR-034 to FR-041. Run them on a test election, not a live one.

- [ ] **E6.1** Officials panel, web: open `/admin/elections`, expand a row, and appoint a member found by search. The new appointment shows with its persona and live status (Web).
- [ ] **E6.2** Officials panel, web: appoint a person by name and email, with no member search. The appointment is stored (Web).
- [ ] **E6.3** Returning Officer: appoint a second person with the Returning Officer box ticked. The first Returning Officer row expires and only one badge shows (Web, Mobile).
- [ ] **E6.4** Officials sheet, mobile: open it from the election card, appoint, and revoke an appointment while no election is frozen (Mobile).
- [ ] **E6.5** Mobile, signed in as an election official who is not an admin: the member search is not offered and a note asks for a name and email (Mobile).
- [ ] **E6.6** Ask to open polling and leave the request waiting for a second official (or put an election in Polling). On `/admin/org-config`, change an election setting. The save is refused with the 409 message naming the election, and an `ElectionFrozenChangeRefused` row appears in the activity record (Web, API).
- [ ] **E6.7** While frozen, edit a persona and add an appointment. Both are refused. A plain revoke is refused too (Web, Mobile, API).
- [ ] **E6.8** While frozen, a SuperAdmin asks for an emergency revoke with a reason. The request is stored and waits for approval (Web, Mobile).
- [ ] **E6.9** Another active official with Approve approves it. The target is revoked and the activity record shows requester, approver, target and reason. The requester and the target cannot approve it (Web, API).
- [ ] **E6.10** With no other active official holding Approve, the emergency revoke request is refused. SuperAdmin cannot approve it alone (API).
- [ ] **E6.11** Leave an emergency revoke unapproved past its expiry. Within 15 minutes an `expired` row appears once in the activity record (API).
- [ ] **E6.12** Rules unlock, web: as SuperAdmin, open the unlock box on `/admin/elections` with a reason shorter than 20 characters (refused), then a valid reason with the default 30 minutes. The box shows who opened it, the reason and the time left (Web).
- [ ] **E6.13** With the unlock open, an election setting saves. An `ElectionFrozenChangeUnlocked` row carries the unlock id and reason. A plain appointment revoke is still refused (Web, API).
- [ ] **E6.14** Close the unlock early. The next change to a frozen rule is refused again (Web, Mobile).
- [ ] **E6.15** Rules unlock, mobile: the sheet from the election management app bar loads nothing until asked, and opens and closes an unlock after step-up (Mobile).
- [ ] **E6.16** Count: after a second official approves a count, the approver cannot run it. The requester can, with the key file, and an `ElectionCountRun` row names the executor (API, Web).
- [ ] **E6.17** Count with `CountRequesterOnly` on (on `/admin/org-config`): a Count holder who is not the requester gets `not-requester`. With it off, that person can run the count (API, Web).
- [ ] **E6.18** Ask for polling with fewer than 2 live officials holding Approve. The warning shows on the admin elections row, and on the mobile advance-phase dialog (Web, Mobile).
- [ ] **E6.19** Send two identical approval requests together. One is stored and the other answers `already-pending` (API).

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
| Election officials and rules unlock (E6) | [ ] | [ ] | [ ] | Manual checks E6.1 to E6.19 not yet run |

---

## Automated test runs (log date + result)

| Date | Filter / scope | Pass | Fail | Skip | Phase 4 status | Command |
|---|---|---:|---:|---:|---|---|
| 2026-07-03 | A1–A6 membership tests | 92 | 0 | 0 | Unchanged | Phase 1 filter |
| 2026-07-03 | A7–F Phase 2 suites | 150 | 0 | 0 | Unchanged | Profile+Family+Networking+Events+Financial+Governance+Auth |
| 2026-07-03 | Full regression (Phase 2) | 305 | 0 | 1 | Superseded | `dotnet test GHCAA.Tests` |
| 2026-07-03 | **Full regression (Phase 4)** | **308** | **0** | **1** | **No regressions** | `dotnet test GHCAA.sln --no-build` |
| 2026-07-03 | Flutter core (Phase 3 Mobile) | 28 | 0 | 0 | Not re-run P4 | `flutter test` (6 files, visual freeze excluded) |
| 2026-07-03 | ForumServiceTests | 2 | 0 | 0 | Included in 308 | Phase 3 API gaps |
| 2026-07-03 | AuthServiceTests lockout (F5) | 1 | 0 | 0 | Included in 308 | Phase 3 API gaps |
| 2026-07-03 | Vitest unit (Phase 3 Web) | 230 | 0 | 0 | Not re-run P4 | `npm run test:unit` GHCAA.Web |
| 2026-07-03 | Playwright full `npm run test:e2e` (Phase 3 Web) | 48 | 44 | 0 | Not re-run P4 | `npm run test:e2e` 92 tests, 15.1m |
| 2026-07-03 | HTTP smoke (Phase 4) | 5 | 0 | 0 | **Pass** | `/healthz`, `/api/config`, forum, SignalR |
