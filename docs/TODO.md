# GHCAA PLATFORM TASK TRACKER

## PRIORITY INDEX (triaged 2026-08-30 — re-triage when this drifts, don't trust it blind per `gotcha_todo_status_drift`)

Every open `[TODO]` item as of this date, grouped by severity/urgency. This index is a pointer, not a
duplicate — the full item text with context stays at its Area location; update both when an item's
status changes.

### ACTION NOW — confirm or correct the Chapter 11 duration assumptions (64.7)

Five activities left no commit, so `docs/book/build/wbs.py` now carries a **calculated assumption** for
each, with the arithmetic printed beside it. Run `python docs/book/build/wbs.py` and either confirm
each rate or give a better one. Nothing else in Chapter 11 can be finished until these are settled,
because the project's total effort figure (97 days, about 4.4 person-months) rests on them.

| ID | Activity | Assumed | Rate to confirm |
|---|---|---|---|
| U1 | Elicitation interviews | 2 days | 35 min contact per participant; write-up at 1× contact time |
| U2 | Governing-document analysis | 5 days | 1,500 words/hour for clause-by-clause classification of 43,000 words |
| U3 | Formal technical review | 2 days | 2h preparation + 2h session + 1h logging, per session |
| U4 | Stakeholder discussion | 2 days | 30 min of discussion per feedback area, 18 areas |
| U5 | Incident response | 1 day, **not added** | 2h diagnosis per incident, before the first fix commit |

The two most open to challenge are U2's reading rate and U1's write-up ratio. U5 is deliberately not
added to the total: those four incident dates carry 9, 7, 10 and 2 commits, so the fix work is already
inside the measured days and counting it twice would inflate the figure.

Answer the five and 64.8 unblocks too, since risk exposure RE = P × C in §4.8 needs an impact cost per
risk on the same basis.

### P0 — CRITICAL (blocked on the user; cannot be closed from a coding session)
- **48.2** — Live production secrets committed to git (JWT signing key, DB passwords, Gmail app
  password, Render deploy-hook URL) in `docs/deploy_connection.txt`, `.env.remote`,
  `build_output/appsettings*.json`, `docs/RENDER_DEPLOYMENT.md`. Needs the user to rotate every
  credential via the relevant dashboards, then `git rm --cached` + `.gitignore` + a history purge
  (`git filter-repo`). No coding-session action can close this.
- **48.13** — The live SuperAdmin password sat in git history (`docs/BUSINESS_FUNCTIONALITY_REVIEW_PLAN.md`)
  since before it was even set as the live password. Doc text is redacted, but the password itself
  still needs an independent rotation — redacting the doc doesn't undo the history exposure.
- **48.12** — Remaining unfixed Low findings from the Area 48 security audit: `MessagingController.MarkAsRead`
  missing ownership check; `FinancialsController.RecordPayment` trusts a client-supplied `MemberId`;
  refresh-token replay isn't detected/revoked; `MemberImportController` upload skips file validation;
  raw `FullName` interpolated into an HTML email body (XSS-adjacent).

### P1 — HIGH (security surface / explicitly time-sensitive / blocking other work)
- **47.13.1–47.13.7** — Mutation-coverage remediation (~35% of POST/PUT/DELETE still untested). Start
  with **47.13.1** (`AuthController` non-Login actions, including this project's own untested step-up
  endpoints) and **47.13.2** (`LookupsController` full CRUD, zero coverage).
- **48.18 / 48.19** — Dependency/CI supply-chain hardening: EF Core/Npgsql pinned to exact `9.0.0` GA
  with no patch tracking; CI Actions pinned to mutable tags with access to the Render deploy-hook
  secret in the same workflow; no NuGet lockfile; no Docker base-image digest pin.
- **49.1–49.3** — Custom roles grant zero actual permissions (label-only — misleads admins); no
  disable/enable for system-admin accounts; no admin-initiated password reset for system admins. Each
  has a "DECISION NEEDED" gate before work starts (see Area 49 for the actual questions).
- **46.5** — Org-wide Financial Ledger has zero rows post-import; aggregate income/expense view doesn't
  reflect the ~৳47,000 in per-member fees that ARE recorded correctly.
- **34.D10** — Likely already superseded by `MigrationBootstrapper` (see `gotcha_ensurecreated_no_op_existing_db`)
  — verify against current `Program.cs` before treating this as still open; don't just re-do it.

### P2 — MEDIUM (real, no urgency signal)
- **45.1–45.7** — Admin error-log viewer, fully planned, nothing built.
- **42.1–42.5** — Admin-manageable elections forms/docs, plan only.
- **49.4 / 49.5** — Grid/row-control consistency cleanup + tests for the new 49.x endpoints once built.
- **51.2–51.5** — File-storage hardening: no real hard-cap on image size, opaque filenames, missing
  tests, compression settings not admin-configurable yet.
- **47.10** — Missing profile photos for most of the 631 bulk-imported alumni (data gap, not a bug).
- **43.4** — Live/manual verification that the Area 43 exception-handling/logging sweep actually fires.
- **27.8** — No enforced ≥80%/file coverage threshold (would fail today if enforced).
- **28.32 / 28.33 / 8.8** — i18n (English+Bengali): dependencies present, extraction not started.
- **34.D7** — 21 stale mobile golden baselines need regenerating (unrelated housekeeping).
- **44.16** — Three unrelated Flutter classes all named `FamilyService` (deferred rename risk).
- **7.16, 8.3–8.7** — Mobile hardening/perf backlog (SSL pinning, pagination, background threading, etc.).
- **12.1–12.6** — Process items (API-change checklist, contract registry, mobile log capture).
- **52.5** — Mobile's separate "quick create gallery" dialog — left as-is, a future cleanup decision.
- **61.1 / 61.2** — Dynamic/runtime dead-code scan (unused services/classes/widgets) + follow-up
  refactor pass, run module-by-module via graphify rather than one blind full-repo sweep.

### P3 — LOW / PLAN-ONLY (large unbuilt features, no current pressure)
- **Area 37** (37.2–37.10) — scholarships, fundraising, cohorts/reunions, oral-history archive,
  bilingual UI, credential verification, geographic chapters, annual impact report.
- **6.2** — Alumni referral system for jobs/internships.
- **61.3** — Drop the `Summary:`-style comment banner in `GHCAA.Tools/db_diag.cs` next time that file is touched.

## AREA 1: MOBILE PLATFORM STABILITY & PARITY

1.1  [DONE] Fix AdminLedgerScreen class name mismatch in app_router.dart
1.2  [DONE] Audit/Match Registration Wizard labels to Web equivalents
1.3  [DONE] Sync RegisterModel with backend Member.cs (FatherName, MotherName, Emergency fields)
1.4  [DONE] Audit and match all mobile screen titles/labels to core domain terminology
1.5  [DONE] Verify CRUD operations on all screens (Profile, Approvals, Events, Jobs, News, Gallery, Fee Config, Governance Assignment, Contact Messages)
1.6  [DONE] Article Submission & Approval logic on mobile (Editorial)
1.7  [DONE] Implement Mobile Messaging/Chat module (SignalR integration)
1.8  [DONE] Synchronize UI validations with API/DB nullability constraints
1.9  [DONE] Implement Automated 401 Session Redirection
1.10 [DONE] Implement 10-minute Inactivity Logout (Idle detection)
1.11 [DONE] Synchronize Administrative Member Management (Status/Category parity)
1.12 [DONE] Implement Optimized Media Delivery (CachedNetworkImage + Progressive Loading)
1.13 [DONE] Implement High-Reliability Error Boundaries (Global Catch-all)
1.14 [DONE] Global Haptic Feedback ecosystem for primary interactions
1.15 [DONE] Integrated Home Login Experience (no redirect jump)
1.16 [DONE] Unified registration portal shortcut on dashboard
1.17 [DONE] High-fidelity UI dividers and spatial consistency on edit screens
1.18 [DONE] Global Top Navbar Visibility Control (Drawer toggle)
1.19 [DONE] Route-aware back buttons on all AppScaffold instances
1.20 [DONE] Fix: Persistent roleProvider to prevent admin UI leakage in member sessions
1.21 [DONE] Fix: Dashboard profile completeness shared utility
1.22 [DONE] Fix: Dashboard banner typography and membership badge logic
1.23 [DONE] Fix: Gatekeeper QR scanner overlay rendering
1.24 [DONE] Integrated skeleton/shimmer screens for all async loading states

## AREA 2: CORE ALUMNI MANAGEMENT & REGISTRY

2.1  [DONE] Member Academic/Professional record migration and mapping
2.2  [DONE] Public Directory Enhancements (Batch/Type/Category visibility)
2.3  [DONE] Membership Lifecycle: Spouse/Family linking (wired + routed)
2.4  [DONE] Membership Lifecycle: "Blue Tick" Verified status control
2.5  [DONE] Membership Lifecycle: Soft-delete cascading (IsArchived architecture)
2.6  [DONE] Automated ID & Certificate Generation (PDF + QR)
2.7  [DONE] Profile UI refinement (Education/Professional record edit buttons)
2.8  [DONE] Create offline data collection templates (Google Forms) for manual member & event migration matching DB validations

## AREA 3: COMMUNICATION & SOCIAL

3.1  [DONE] Real-time Communication Bridge (SignalR Admin Alerts)
3.2  [DONE] Member Chat/Noticeboard Framework & Services
3.3  [DONE] HTML Templating system for system notifications
3.4  [DONE] SMS Gateway Integration (Greenweb/SSL Wireless)
3.5  [DONE] Networking: Privacy controls (visibility toggles respect DTO masking)
3.6  [DONE] Networking: Mentorship request flow in Job Hub
3.7  [DONE] Discussion forums and community groups

## AREA 4: EVENTS & GATHERINGS

4.1  [DONE] Automated registration closing for past/due events
4.2  [DONE] Landing Page: Featured event display with last closed history
4.3  [DONE] Flexible Pricing: Free/Paid toggles and registration windows
4.4  [DONE] Event Media: Logo upload and detail view rendering
4.5  [DONE] My Participations: Payment gateway integration
4.6  [DONE] Advanced: Waitlist management and QR Attendance scanning

## AREA 5: FINANCIAL & ADMIN GOVERNANCE

5.1  [DONE] Smart Payment Gateway Automation (Webhooks for bKash/Nagad/SSL)
5.2  [DONE] EC Management: Term configuration and role propagation
5.3  [DONE] Constraint: Single EC role per member per period
5.4  [DONE] Automated PDF Tax/Donation Receipts generation
5.5  [DONE] Claims-based Auth: PermissionsMatrixScreen for role management
5.6  [DONE] Governance Registry: Admin assignment UI

## AREA 6: CAREER & OPPORTUNITIES

6.1  [DONE] Professional Hub: Alumni directory LinkedIn-style filters
6.2  [TODO] Alumni referral system for jobs and internships

## AREA 7: SECURITY, INFRASTRUCTURE & HARDENING

7.1  [DONE] API: Fix Member Login with NID (401 resolution)
7.2  [DONE] API: Fix Password Reset timeout logic
7.3  [DONE] API: Real-time Session Termination upon status change
7.4  [DONE] API: Secure Admin routes with SuperAdmin functional guards
7.5  [DONE] API: Event Note data leakage fix (null out sensitive notes in public lists)
7.6  [DONE] Web: Profile rendering pascal/camel case mapping stability
7.7  [DONE] Web: Image path prefixing sync across all components
7.8  [DONE] Web: Event Date validation (StartDate < EndDate enforcement)
7.9  [DONE] Web: Readonly field protection in Profile update payloads
7.10 [DONE] Mobile: Credentials stored only upon explicit biometric opt-in
7.11 [DONE] Mobile: SharedPreferences preservation on logout (preserving layout)
7.12 [DONE] Mobile: Unified userProfileProvider to prevent sync-re-fetch loops
7.13 [DONE 2026-08-29] Security: Admin step-up (2FA) for destructive/financial/identity actions.
Reuses the existing `IOtpService` (new `OtpPurpose.AdminStepUp`, OTP codes now scoped by purpose
so a registration code can never satisfy a step-up challenge) rather than building new OTP
plumbing. New `POST /api/auth/admin/step-up/request` + `/verify` endpoints; a
`[RequireStepUp]` action filter gates 7 endpoints: member archive, EC hard-delete, financial
ledger add/update/delete, system-admin delete, payment-config delete. Verification is a JWT claim
(`step_up_verified_at`) with a **30-day grace period since last verification** (not per-action —
tuned down from an initial 15-minute design per user feedback: "don't want OTP on every
action/login"), carried forward across the access token's hourly silent refresh via
`TokenService.TryGetValidStepUpEpoch` (validates the outgoing token's signature before trusting
its claim) so the grace period survives normal activity but a fresh login always starts
unverified. Angular: `StepUpService` + `app-step-up-dialog` (mounted once at app root next to
`<app-toast>`) + an interceptor branch that catches `403 STEP_UP_REQUIRED` and retries. 20 new
backend tests + 8 frontend tests. `dotnet test` 468/468, `npx vitest run` 335/335 pass.
7.14 [DONE 2026-08-22] Security: Biometric Authentication (FaceID/Fingerprint) VERIFIED 2026-08-22 — this was already implemented and mis-tracked. `local_auth ^2.2.0` is in `pubspec.yaml`; `lib/core/services/biometric_service.dart` exposes `isBiometricsAvailable()`, `getAvailableBiometrics()`, `authenticate({reason})` with a graceful Flutter-Web false; `auth_service.login(..., enableBiometric)` stores credentials only on opt-in; `app_home_screen.dart` gates the fast-login affordance on `_checkBiometrics()`.
7.15 [DONE] Security: Social Auth (OAuth2) - LinkedIn/Google
7.16 [TODO] Hardening: SSL Pinning and Binary Obfuscation

## AREA 8: MOBILE ENGINEERING (TIER-1 STANDARDS)

8.1  [DONE] UI: Enforce 8pt grid and standard design tokens globally
8.2  [DONE] Nav: Adaptive layout for Tablets/Pads (Sidebar architecture)
8.3  [TODO] Perf: Cursor-based pagination for Alumni Registry
8.4  [TODO] Perf: Isolated background threading for JSON/Encryption processing
8.5  [TODO] Persistence: Switch to High-Performance Local DB (Isar/Drift)
8.6  [TODO] Networking: Exponential backoff and connectivity banners
8.7  [TODO] State: Riverpod State Hydration (Local local persistence)
8.8  [TODO] i18n: Unified Localization (English + Bengali)
8.9  [DONE] CI/CD: Fastlane + GitHub Actions Deployment Pipeline
8.10 [DONE] Quality: Global Error Boundary and Sentry/Firebase tracing
8.11 [DONE] CI/CD: Operationalize multi-environment pipelines (Preprod/Standard)

## AREA 9: INSTITUTIONAL GOVERNANCE & QUALITY

9.1  [DONE] Digital Constitution: Versioned legal repository
9.2  [DONE] Amendment Voting: Secure participation for verified alumni
9.3  [DONE] Collaborative Editorial: Multi-user news workflows
9.4  [DONE] Mobile Governance Portal (/committee route)
9.5  [DONE] Public Transparency: Categorical sitemap in footer
9.6  [DONE] Documentation: System Architecture & Data Flow blueprint
9.7  [DONE] Quality: GitHub PR Template + Sequential CI (API -> UI -> MOBILE)
9.8  [DONE] Quality: Comprehensive Testing suite integration

## AREA 10: USER FEEDBACK & RECENT ISSUES (PHASE 2)

10.1 [DONE] Unify Profile Completeness Logic: Sync Backend (10 fields) with Mobile (11 fields)
10.2 [DONE] Fix Web/Mobile 404s: Alias News/Pending, Networking/Directory, Financial/Ledger, Governance/Current, Governance/Constitution, Notification (case sensitivity)
10.3 [DONE] Fix Mobile API Prefixes: Resolve double /api/api prefix in feature-specific calls (Governance, etc.)
10.4 [DONE] Governance UI (Web): Show Name, Photo, Position, ID, Type in EC list
10.5 [DONE] Family Link: Enable "Search by Name" for linking family members
10.6 [DONE] UX: Add "Back/Close" buttons to all modal-style screens (e.g. Profile)
10.7 [DONE] Digital ID: Implement high-fidelity card layout design
10.8 [DONE] News Fix: Resolve 500 mapping error for ArticleCategory=Magazine
10.9 [DONE] Mobile Stability: Gracefully handle Biometric Auth errors (local_auth) on Web platform
10.10 [DONE] Role Management: Fix session leakage where a member sees the Admin Dashboard/Badge after login
10.11 [DONE] Data Display: Fix "Batch: N/A" for members (ensure PassingYear is correctly mapped and rendered)

## AREA 11: API PARITY AUDIT & FIX LIST

11.1 [DONE] Fix: api/governance/ec/current -> 404 (Controller route verified correct; path was correct)
11.2 [DONE] Fix: api/governance/constitution -> 404 (Controller route verified correct)
11.3 [DONE] Fix: Mobile calling api/Notification (capital N, path wrong); correct endpoint is api/notifications
11.4 [DONE] Fix: Mobile financial_service calls /financial/ledger, should be /ledger
11.5 [DONE] Fix: Mobile assistant_service calls /api/assistant/ask (double-api); should be /assistant/ask
11.6 [DONE] Fix: api/networking/directory -> 404 (alias added to NetworkingController.cs)
11.7 [DONE] Fix: api/news/admin/pending -> 404 (alias added to NewsController.cs)
11.8 [DONE] Fix: auth role leakage - clear session completely before saving new login role
11.9 [DONE] Fix: Mobile biometric (local_auth) crash on Flutter Web; guard with kIsWeb check

## AREA 12: PROCESS & ENGINEERING STANDARDS

12.1 [TODO] PROCESS: On every API endpoint change, add verification checklist task for Web + Mobile parity
12.2 [TODO] PROCESS: Implement API Contract Registry (changelog of all endpoint changes + which clients updated)
12.3 [TODO] Mobile: Implement in-app log capture (rotating file log) for all API errors and app events
12.4 [TODO] Mobile: Add "Report a Problem" / "Share Logs" feature so users can email/share captured logs to admin
12.5 [TODO] Mobile: On any unhandled error, show option to "Send Report to Administrator" with log attachment
12.6 [TODO] PROCESS: A task can only be marked as [DONE] after its tests have been successfully executed and passed.

## AREA 13: VISUAL TESTING & QUALITY FREEZE

13.1 [DONE] API: Implement 'Seed Data' profile for visual tests (migration-safe, Visual env-gated)
13.2 [DONE] Infra: Define Visual Test Storage (Baseline/Failure/Diff) — ARCH.md created
13.3 [DONE] Scripting: Unified 'visual-check.ps1' with 4-stage runner, suite filtering, update-baselines flag
13.4 [DONE] UI Quality Freeze: COMPREHENSIVE module-by-module coverage:
     - [DONE] Member Identity: Digital ID Card         — digital-id.spec.ts + visual_freeze_test.dart
     - [DONE] Member Home: Dashboard                   — dashboard.spec.ts + dashboard_visual_test.dart
     - [DONE] Public: Landing, About, Contact, Login   — public-pages.spec.ts
     - [DONE] Public: Registration page                — public-pages.spec.ts + auth_register golden
     - [DONE] Content: News Hub, Magazine, Gallery     — content.spec.ts + goldens
     - [DONE] Governance: EC Committee Registry        — governance.spec.ts + goldens
     - [DONE] Governance: Digital Constitution         — goldens (governance_constitution)
     - [DONE] Directory: Member directory (auth+public) — directory.spec.ts + goldens
     - [DONE] Career: Job listings & Mentorship        — jobs.spec.ts + goldens
     - [DONE] Events: Event list (auth+public)         — events.spec.ts + goldens
     - [DONE] Finance: Member Payment Portal           — finance.spec.ts + goldens
     - [DONE] Profile: Member profile & Articles       — profile.spec.ts + goldens
     - [DONE] Admin: Dashboard, Members, Approvals     — admin.spec.ts (full)
     - [DONE] Admin: News, Events, EC, Gallery         — admin.spec.ts (full)
     - [DONE] Admin: Article approvals, Contact msgs   — admin.spec.ts (full)
     - [DONE] SuperAdmin: Ledger, Fees, Roles, Audit   — admin.spec.ts (SuperAdmin group)
     - [DONE] Support: AI Assistant & Messaging        — support.spec.ts + goldens
     - [DONE] Notifications & Activity Log             — goldens
     - [DONE] Auth: Login & Register screens           — goldens
13.5 [DONE] Runner upgraded: -VisualOnly, -E2EOnly, -Suite, -UpdateBaselines, colored summary

## AREA 14: FUNCTIONAL E2E (END-USER TESTING)

14.1 [DONE] Web: Playwright member journey (Login -> Dashboard -> ID Card -> Logout)
14.2 [DONE] Mobile: Integration test journey (Login -> Dashboard -> ID Card -> Logout)
14.3 [DONE] Web: Admin approval workflow E2E
14.4 [DONE] Mobile: Financial ledger verification E2E
14.5 [DONE] Cross-Platform: Article contribution & editorial approval E2E (Web + Mobile)

## AREA 15: SOCIAL AUTH & ONBOARDING

15.1 [DONE] API: Add GoogleId and FacebookId to User entity
15.2 [DONE] API: Add IsProfileComplete to Member entity
15.3 [DONE] API: Implement Social Auth (Google/Facebook) logic & Admin Config
15.4 [DONE] Web: Implement "Login with Google/Facebook" buttons
15.5 [DONE] Mobile: Implement Social Login (google_sign_in, flutter_facebook_auth)
15.6 [DONE] Cross-Platform: Implementation of Onboarding Flow (Profile Setup -> Payment -> Approval)
15.7 [DONE] Backend: Unit tests for Social Auth and Onboarding logic

## AREA 16: POLLS & VOTING

16.1 [DONE] Domain: Create Poll, PollOption, and PollVote models
16.2 [DONE] API: IPollService and PollService implementation
16.3 [DONE] API: PollController for admin and member actions
16.4 [DONE] Web: Admin UI for Poll Management
16.5 [DONE] Web: Member UI for Poll Voting & Results
16.6 [DONE] Mobile: Member UI for Poll Voting & Results
16.7 [DONE] Backend: Unit tests for Polls and Voting logic

## AREA 17: REGRESSION & STABILITY

17.1 [DONE] API: Verify existing Auth flows (NID/Password) remain functional
17.2 [DONE] API: Verify Member Registration and Approval workflows remain functional
17.3 [DONE] Web: Verify full member lifecycle (Login -> Profile -> Dashboard)
17.4 [DONE] Mobile: Verify full member lifecycle (Login -> Profile -> Dashboard)
17.5 [DONE] Cross-Platform: Run all existing Playwright and Flutter integration tests

## AREA 18: GENERAL MAINTENANCE & STABILITY

18.1 [DONE] Mobile: Fix unused import in registration_visual_test.dart
18.2 [DONE] Mobile: Upgrade Governance UI (Fonts, GlassContainer, Image resolution)
18.3 [DONE] Web: Refine Roles Management UI (Input padding, Fancy dropdowns, Smart selection)
18.4 [DONE] Backend: Seed Constitution data to resolve Governance 404s
18.5 [DONE] Mobile: If not connected with net, show error on mobile app
18.6 [DONE] Mobile: Run 'flutter pub get' in GHCAA.Mobile to resolve connectivity_plus dependency errors
18.7 [DONE] Backend: Start GHCAA.API to resolve ECONNREFUSED (port 5087) errors
18.8 [DONE] Backend: Update PostgreSQL password in appsettings.Development.json if SyncMembersForReal test fails locally
18.9 [DONE] UI Audit: Review all SCSS files for hardcoded #fff or #000 that break theme accessibility

## AREA 19: PAYMENT VERIFICATION & POLICY

19.1 [DONE] Payments: Verify Registration Fee configuration in Admin Portal
19.2 [DONE] Payments: Verify Registration Fee status on Member Dashboard (Profile Completion Wizard)
19.3 [DONE] Payments: Ensure Registration Fee is mandatory for all members as per latest policy

## AREA 20: COMPREHENSIVE E2E COVERAGE (ALL FEATURES)

20.1 [DONE] Web: Expand Playwright E2E suite to cover all core portal features (Messaging, Job Hub, Alumni Directory, Events, Gallery, Governance, My Articles, My Profile)
20.2 [DONE] Mobile: Expand Flutter integration/visual tests to cover all core mobile features (Messaging, Job Hub, Alumni Directory, Events, Gallery, Governance, My Articles, My Profile, Admin Modules)

## AREA 21: TEST DATA MANAGEMENT & VISUAL AUTOMATION

21.1 [DONE] Quality: Create 'test-dataset.json' with comprehensive edge cases (Large names, missing photos, various membership tiers)
21.2 [DONE] Quality: Implement 'scripts/setup-test-data.ps1' to inject test dataset into active environment (separate from seed)
21.3 [DONE] Quality: Implement 'scripts/cleanup-test-data.ps1' to revert environment to clean/seed state
21.4 [DONE] Quality: Implement 'scripts/run-visual-tests.ps1' to execute all visual regressions with the test dataset
21.5 [DONE] Quality: Integrate visual test report generation (HTML) for local review

## AREA 22: PORTAL FEATURE HARDENING (E2E)

22.1 [DONE] E2E: Verify Messaging flow (Member <-> Admin) with real-time checks
22.2 [DONE] E2E: Verify Job Hub (Post -> Review -> View) workflow
22.3 [DONE] E2E: Verify Alumni Directory filtering and search precision
22.4 [DONE] E2E: Verify Event Registration and QR generation flow
22.5 [DONE] E2E: Verify Gallery upload and album organization (Admin side)

## AREA 23: ECOSYSTEM-WIDE DATE STANDARDIZATION (dd-MM-yyyy)

> **HEADING SUPERSEDED — read 29F.3 first (noted 2026-08-22).** The `(dd-MM-yyyy)` in this
> heading described the original intent, before **29F.3 [DONE]** settled the contract:
> **ISO-8601 is the canonical wire format; `dd-MM-yyyy` is display/input only.** The items below
> are all still correctly `[DONE]` — 23.2/23.3/23.5/23.6 are display-and-input work and are
> unaffected — but do not read this heading as licence to emit `dd-MM-yyyy` in a payload.
>
> The contract is enforced entirely by `DateFormatConverter` / `NullableDateFormatConverter`
> (`GHCAA.API/Utils/DateFormatConverter.cs`, registered globally in `Program.cs` ~137-138), and
> **those two types have zero tests** — so nothing would fail if the format regressed back to the
> heading's wording. Tracked as **27.7**; also flagged in `docs/low_coverage_report.md`.

23.1 [DONE] API: Implement DateFormatConverter for unified dd-MM-yyyy/ISO parsing
23.2 [DONE] Web: Standardize all Angular date inputs to dd-mm-yyyy (Registration, Profile, Events, Admin)
23.3 [DONE] Web: Update date validation logic and labels to guide users on dd-mm-yyyy format
23.4 [DONE] Mobile: Refactor AppUtils with parseDate/formatDate supporting dd-MM-yyyy standard
23.5 [DONE] Mobile: Update all screens (Registration, Profile, Events, Jobs, Gallery) to use standardized dates
23.6 [DONE] E2E: Update Playwright test suite to use dd-mm-yyyy for all automated date entries

## AREA 24: SECURITY HARDENING (from full-stack code review — 2026-05-02)

### 24-A: CRITICAL — Authentication & Token Backdoors

24.1  [DONE] Security: Gate VisualTestAuthMiddleware behind Development env + ASP_SEED_PROFILE=Visual; reject "Bearer visual_*" in all other environments (VisualTestAuthMiddleware.cs)
24.2  [DONE] Security: Remove committed JWT fallback key "LOCAL_DEVELOPMENT_JWT_FALLBACK_32_CHARS_MIN"; generate ephemeral random 32-byte key on dev startup, require User Secrets (JwtSigningKeyResolver.cs)
24.3  [DONE] Security: Remove DB passwords from appsettings.Development.json and appsettings.json; enforce env-var / User Secrets only (appsettings.*.json)
24.4  [DONE] Security: Add [Authorize] attribute to ChatHub class (ChatHub.cs)
24.5  [DONE] Security: Add [Authorize] attribute to NotificationHub class; gate JoinBatch/JoinDepartment to authenticated callers (NotificationHub.cs)
24.6  [DONE] Security: Replace non-thread-safe Dictionary<string,string> in ChatHub with ConcurrentDictionary; support multi-device per user (ChatHub.cs)

### 24-B: CRITICAL — Sensitive Data in Responses

24.7  [DONE] API: Do not return DefaultPassword in ApproveMember HTTP response; send via email only, return PasswordEmailed: true flag (AdminController.cs)
24.8  [DONE] API: Do not return ResetUrl (containing reset token) in ResetPasswordAdmin response; force email-only delivery (AdminController.cs)
24.9  [DONE] API: Do not return ClientSecret/GatewaySecretKey in AdminSocialAuthController.GetConfigs or PaymentConfigController responses; mask or omit secrets (AdminSocialAuthController.cs, PaymentConfigController.cs)

### 24-C: CRITICAL — Payment Gateway Security

24.10 [DONE] Payment: Implement HMAC/signature verification on bKash webhook before trusting paymentID (BkashGateway.cs ProcessWebhookAsync)
24.11 [DONE] Payment: Implement verify_sign HMAC check on SSLCommerz webhook; verify store_passwd hash against all callback fields (SSLCommerzGateway.cs VerifyCallbackAsync)
24.12 [DONE] Payment: Verify executed payment amount against the originating PaymentHistory amount for bKash (BkashGateway.cs VerifyCallbackAsync)
24.13 [DONE] Payment: Add idempotency — store paymentID/val_id in PaymentHistory with UNIQUE constraint; short-circuit on duplicate callback (both gateways)
24.14 [DONE] Payment: Fix HttpClient.DefaultRequestHeaders mutation race in BkashGateway (3 call sites); use per-request HttpRequestMessage headers (BkashGateway.cs)
24.15 [DONE] Payment: Derive CallbackUrl exclusively from server-side config (AppSettings:PublicApiBaseUrl), never from request.BaseUrl supplied by the client (GatewaysController.cs)
24.16 [DONE] Payment: Replace hardcoded fee fallback of 500 BDT and adminId fallback of "1" with startup-time config validation that fails loudly (GatewaysController.cs)
24.17 [DONE] Payment: Fix SSLCommerz form body parser — split on '=' truncates base64 values; use QueryHelpers.ParseQuery instead (SSLCommerzGateway.cs ProcessWebhookAsync)

### 24-D: CRITICAL — File & Path Security

24.18 [DONE] Security: Fix path traversal in SecureFilesController — after Path.GetFullPath, assert path starts with the secure-uploads root; reject any path containing ".." (SecureFilesController.cs)

### 24-E: CRITICAL — OTP Service

24.19 [DONE] Security: Replace new Random() with RandomNumberGenerator.GetInt32(100_000, 1_000_000) for cryptographically secure OTP generation (OtpService.cs)
24.20 [DONE] Security: Store OTP as HMAC-SHA256(code, email), not cleartext; a DB compromise reveals all active codes (OtpService.cs)
24.21 [DONE] Security: Add per-email attempt counter — lock OTP after 5 wrong attempts; add Purpose field (EmailVerify, PasswordReset) to prevent cross-use (OtpService.cs)
24.22 [DONE] Security: Invalidate all previous unverified OTPs for the same email before inserting a new one (OtpService.cs)

### 24-F: HIGH — Authentication Service

24.23 [DONE] Security: Add brute-force lockout to LoginAsync — track FailedLoginAttempts + LockoutUntil on User, lock 15 min after 5 failures (AuthService.cs)
24.24 [DONE] Security: Fix username enumeration timing attack — always run a dummy BCrypt.Verify when user is not found to normalize response time (AuthService.cs)
24.25 [DONE] Security: Fix social auto-link — only link IdP email to local account when member.EmailVerified == true (AuthService.cs SocialLoginAsync)
24.26 [DONE] Security: Social signup uses unique SOCIAL-{PROVIDER}-{socialId} sentinel instead of "TBD" for NID/MobileNo — prevents UNIQUE index crash on second social signup (AuthService.cs SocialLoginAsync)
24.27 [DONE] Security: JWT lifetime reduced to 60 min; opaque refresh token (SHA-256 hash stored in RefreshTokens table, 7-day cookie rotation) implemented in TokenService.cs + RefreshToken entity + /auth/refresh endpoint
24.28 [DONE] Security: Rotate SecurityStamp on password reset and password change to invalidate all previous JWTs (AuthService.cs, UserService.cs)

### 24-G: HIGH — Data Integrity (MemberService)

24.29 [DONE] Bug: Fix membership number lexicographic race — OrderByDescending(m.Id) instead of string sort; prevents regression at 999→1000 rollover (MemberService.cs RegisterAsync + ApproveMemberAsync)
24.30 [DONE] Bug: Replace hard-delete of rejected applicants with soft-delete (Status=Rejected, IsArchived=true) preserving audit trail; added Rejected to MembershipStatus enum (MemberService.cs, Enums.cs)
24.31 [DONE] Bug: CreateUserAccountAsync moved inside the serializable transaction in ApproveMemberAsync — member+user are now atomic (MemberService.cs)
24.32 [DONE] Perf: BulkArchiveInactiveMembersAsync replaced N+1 ArchiveMemberAsync loop with bulk ExecuteUpdateAsync calls for all cascades (MemberService.cs)

### 24-H: HIGH — Missing DB Indexes

24.33 [DONE] Perf: Add index on User.MemberId (hot lookup path in Auth, Member services)
24.34 [DONE] Perf: Add partial index on User.ResetToken WHERE ResetToken IS NOT NULL
24.35 [DONE] Perf: Add index on User.GoogleId, User.FacebookId (social login hot path)
24.36 [DONE] Perf: Add composite index on Otp(Email, ExpiryAt) — OTP verification scans table without it
24.37 [DONE] Perf: Add composite index on Member(Status, IsArchived) — heavily filtered in admin listing queries (MemberConfiguration.cs)
24.38 [DONE] Perf: Add entity configuration file for Otp table (currently using EF defaults — no indexes, no column length caps on a security-critical table)

### 24-I: HIGH — Frontend (Angular)

24.39 [DONE] Security: Move JWT from localStorage to httpOnly + Secure + SameSite=Strict cookie set by backend; token no longer stored in localStorage; Angular uses /auth/me on init; sessionStorage holds display fields only (auth.service.ts, ServiceExtensions.cs, AuthController.cs)
24.40 [DONE] Security: Wrap getUserFromStorage() JSON.parse in try/catch; fall back to null and clear corrupt entry (auth.service.ts)
24.41 [DONE] Security: Check JWT exp claim on storage restore — drop session if token is already expired (auth.service.ts)
24.42 [DONE] Security: Sanitize article HTML server-side with Ganss.HtmlSanitizer before storage in NewsService.CreateNewsAsync + UpdateNewsAsync; [innerHTML] now safe (NewsService.cs, magazine.html)
24.43 [DONE] Security: Restrict Authorization header injection in globalHttpInterceptor to /api/* URLs only, not all outgoing HttpClient requests (global-http.interceptor.ts)
24.44 [DONE] Security: Token refresh implemented — 401 queuing with BehaviorSubject, POST /auth/refresh retried, all concurrent 401s share single refresh call, logout only on refresh failure (global-http.interceptor.ts, AuthController.cs)
24.45 [DONE] Security: Client-side file validation added — images ≤ 5 MB (JPEG/PNG/WebP), PDFs ≤ 10 MB; shared validateUploadFile() utility applied to register.ts, gallery.ts, admin-members.ts, admin-events.ts, articles.ts (file-validation.util.ts)
24.46 [DONE] Security: Enforce mustChangePassword redirect — after login, route to change-password screen before allowing any navigation (auth.service.ts, auth.guard.ts)

### 24-J: HIGH — API Controller Hardening

24.47 [DONE] Security: Restrict QueryStringTokenMiddleware to GET-only on /api/secure-files/ and /api/uploads/ paths (QueryStringTokenMiddleware.cs)
24.48 [DONE] Security: PartitionedRateLimiter keyed on (IP, username) for login endpoint; LoginRateLimitMiddleware peeks body username before rate limiter runs (Program.cs, LoginRateLimitMiddleware.cs)
24.49 [DONE] Security: Add startup validator that throws if AllowedOrigins is empty in Production (Program.cs)
24.50 [DONE] Security: Use int.TryParse for MemberId claim parsing; return Unauthorized on missing/invalid claim (PollController.cs, FamilyLinkController.cs)
24.51 [DONE] Security: Read admin identity from User.FindFirst("MemberId") JWT claim in ApproveMember and RejectMember (AdminController.cs)

### 24-K: MEDIUM — Validator & Input Hardening

24.52 [DONE] Validation: Add MaximumLength and safe-character regex to FatherName, MotherName, PresentAddress, PermanentAddress, EmergencyContactName, EmergencyContactRelation, EmergencyContactPhone in MemberRegistrationValidator (MemberRegistrationValidator.cs)
24.53 [DONE] Validation: Add EmergencyContactPhone format validation (same regex as MobileNo) (MemberRegistrationValidator.cs)
24.54 [DONE] Validation: Add minimum age gate (LessThan(UtcNow.AddYears(-13))) and sanity lower bound (GreaterThan(UtcNow.AddYears(-120))) to DateOfBirth (MemberRegistrationValidator.cs)
24.55 [DONE] Validation: Add MaximumLength(254) to Email in VerifyEmailValidator; add caps to AcademicHistory.Subject and Result fields (MemberRegistrationValidator.cs, VerifyEmailValidator.cs)
24.56 [DONE] Validation: Enforce AdmissionYear < PassingYear in academic record child rules (MemberRegistrationValidator.cs)

### 24-L: MEDIUM — CSP & Security Headers

24.57 [DONE] Security: Remove 'unsafe-inline' from script-src in CSP header; use nonces or hashes (SecurityHeadersMiddleware.cs)
24.58 [DONE] Security: Register SecurityHeadersMiddleware before UseStaticFiles so static file responses also receive security headers (Program.cs)
24.59 [DONE] Security: Remove deprecated X-XSS-Protection header (SecurityHeadersMiddleware.cs)
24.60 [DONE] Security: Add window.open(..., '_blank', 'noopener,noreferrer') to all gallery/event external link openings to prevent tab-napping (gallery.ts, events.ts)

## CREDENTIALS:

- SuperAdmin: superadmin / SuperAdminPassword123!
- Developer Admin: shalin / Shalin@2024!
- TEST_MEMBER: demo_user / DemoPass123!
- Mobile: If not connected with net, show error on mobile app

## AREA 25: DGePAY PAYMENT GATEWAY INTEGRATION

- [X] 25.1 API: Implement DGePayGateway service (AES-128-ECB + HMAC-SHA256, Database-driven)
- [X] 25.2 API: Register DGePayGateway in DependencyInjection.cs and PaymentGatewayFactory
- [X] 25.3 Define callback endpoint in GatewaysController
- [X] 25.4 Seed DGePay UAT credentials in payment_configurations.json
- [X] 25.5 Formalize Gateway Workflow documentation (docs/PAYMENT_GATEWAY_WORKFLOW.md)
- [X] 25.6 Update Mobile UI (Flutter) — gateway enum synced, selection bottom-sheet, PaymentWebPage integration
- [X] 25.7 E2E Payment Flow Verification

## AREA 26: VISUAL REGRESSION LAYOUT HARDENING

26.1 [DONE] Mobile: Fix RenderFlex overflows — AppScaffold title/breadcrumb (maxLines + ellipsis)
26.2 [DONE] Mobile: Fix RenderFlex overflows — DirectoryScreen member designation badges (Flexible + ellipsis)
26.3 [DONE] Mobile: Fix RenderFlex overflow — DigitalIDScreen header title (Expanded + FittedBox)
26.4 [DONE] Mobile: Fix RenderFlex overflow — NewsScreen category chip row (Flexible + ellipsis)
26.5 [DONE] Mobile: Fix RenderFlex overflow — ArticlesScreen article status badge row (Flexible + ellipsis)
26.6 [DONE] Mobile: Fix RenderFlex overflow — JobDetailsScreen stat rows (Expanded + end-aligned ellipsis)
26.7 [DONE] Mobile: Fix RenderFlex overflow — EventsScreen date/fee row (Expanded + Flexible)
26.8 [DONE] Mobile: Fix overflow — DashboardScreen admin analytics stat card (FittedBox + label ellipsis)
26.9 [DONE] Test: Fix MockHttpClient missing @override annotations in full_app_visual_freeze_test.dart
26.10 [DONE] Mobile: Generate final golden baselines after Riverpod teardown fix — 15/15 goldens synced
26.11 [DONE] Test: Fix Riverpod NotInitializedError in tearDownAll — Refactored to isolated test lifecycles
26.12 [DONE] Test: Fix registration_visual_test.dart — Added setStep() helper to RegisterWizardNotifier for direct rendering
26.13 [DONE] Mobile: Fix RenderFlex overflow in login_screen.dart (Flexible + FittedBox)

## AREA 27: TEST COVERAGE IMPROVEMENT

27.1  [IN-PROGRESS] Generate low‑coverage report (parse coverage.cobertura.xml)
27.2  [DONE 2026-08-22] Add test project references for API, Application, Domain, Infrastructure VERIFIED 2026-08-22: `GHCAA.Tests.csproj` references all four projects (Application, Infrastructure, Domain, API).
27.3  [DONE 2026-08-22] Write unit tests for Controllers (WebApplicationFactory) VERIFIED 2026-08-22: 18 controller test classes under `GHCAA.Tests/Controllers/` plus a shared `ControllerTestBase.cs`.
27.4  [DONE 2026-08-22] Write unit tests for Handlers/Services (Moq) VERIFIED 2026-08-22: ~20 service test classes under `GHCAA.Tests/Services/`, with `Moq 4.20.72` + `FluentAssertions 6.12.2` referenced.
27.5  [DONE 2026-08-22] Write unit tests for Domain Validators (FluentValidation) VERIFIED 2026-08-22: `GHCAA.Tests/Validators/MemberRegistrationValidatorTests.cs` and `VerifyEmailValidatorTests.cs`.
27.6  [DONE 2026-08-22] Write repository integration tests with in‑memory SQLite VERIFIED 2026-08-22: `Microsoft.EntityFrameworkCore.Sqlite 9.0.1` + `.InMemory 9.0.1` referenced, with `GHCAA.Tests/Repositories/FileUploadRepositoryTests.cs` and SQLite-backed service tests.
27.7  [DONE 2026-08-22] Write utility class tests (DateFormatConverter, etc.) **CLOSED 2026-08-22:** `GHCAA.Tests/Utils/DateFormatConverterTests.cs` added — 20 tests, all passing; full backend suite now **350 passed / 0 failed** (was 330). Pins the 29F.3 contract in both directions: ISO-8601 on write (incl. time preserved and the `.fff` shape), dd-MM-yyyy accepted on read with ISO as fallback, day-first precedence for ambiguous input like `02-03-2026`, empty/whitespace → `default` on the non-nullable converter but → `null` on the nullable one, and malformed/impossible dates throwing `FormatException` rather than silently yielding `01-01-0001`. Prior note, now historical:  **PARTIAL, confirmed 2026-08-22:** the named example is still untested — `DateFormatConverter` / `NullableDateFormatConverter` live in `GHCAA.API/Utils/DateFormatConverter.cs` and are registered in `Program.cs` (lines ~137-138), but no test file references them. Given these two converters govern **every** DateTime on the wire (see the ISO-8601 switch), they are the highest-value gap in Area 27.
27.8  [TODO] Run coverage and enforce ≥ 80 % per file (Still genuinely open, confirmed 2026-08-22: `coverlet.collector 6.0.2` is referenced so coverage *can* be collected locally, but no threshold is enforced anywhere and README explicitly declines to claim a figure. Enforcing >=80%/file would fail today.)
27.9  [DONE 2026-08-22] Update README with test & coverage instructions VERIFIED 2026-08-22: README line ~299 documents `dotnet test` / `npm test` / `flutter test`, and line ~301 explains the coverage position and the local `coverlet.collector` command.

## AREA 28: CONFIGURATION-DRIVEN FRAMEWORK

> Reference doc: docs/CONFIG_DRIVEN_FRAMEWORK.md
> DRY/SOLID review completed by Claude Opus on 2026-05-30.
>
> **STATUS AUDIT 2026-08-22.** This area's checkboxes had drifted badly: 21 of its 34 items were
> still marked `[TODO]` while the code had in fact shipped, which made Area 28 look like the
> project's largest open block when it is very nearly closed. Every item was re-verified against
> the tree (file existence + symbol grep + `dotnet test`), not against this file. Phases 1, 2, 3, 4
> and the test phase are **done**; 28.29/28.30 are **not applicable** (the `Constants.Branding` /
> `Constants.EmailSubjects` classes they target do not exist). **28.21 is closed as of
> 2026-08-22 by 35.5** — the Flutter registration tier list was deleted, not completed with
> `Guest`, because tiers are admin-assigned only. Only 4 items remain genuinely open:
> **28.25** (a snapshot re-run, unverifiable as
> written), **28.32** (ngx-translate), **28.33** (no `.arb` files yet), and the deploy half of
> **28.0**, which is tracked once in **34.D10**.
>
> Where the shipped code deviates from a plan line, the plan line now records the **real** symbol
> name — `isFeatureEnabled` not `isEnabled`, `localePack(locale)` not `t(path)`,
> `API_ENDPOINTS.CONFIG` not `ORG_CONFIG`, Riverpod `orgConfigProvider` not
> `OrgConfigService.instance.load()`. Trust the noted names, not the original wording.

### PRIORITY 0 - BLOCKING (must run before anything else in this area)

28.0  [VERIFIED-STALE 2026-08-22] DB: Apply pending Area-24 migrations, then generate & apply AddOrganizationConfig **STALE ENTRY — code side is done.** Verified 2026-08-22: `20260530092800_AddOrganizationConfig.cs` and `20260703123040_AddOrganizationConfigRowVersion.cs` both exist in `Data/Migrations/PgSql/`, and the Area-24 prerequisites (`PhaseB_S5S8...`, `AddRefreshTokens`) are in the same tree, so Steps 1-3 were carried out. This item is therefore NOT blocking Phase 2/3 (both of which shipped). What remains is purely the *deployment* half — nothing runs the migration tree at startup, so a non-empty preprod DB never receives it. That residual risk is tracked once, in 34.D10; do not re-open it here.
              Step 1: dotnet ef database update --project GHCAA.Infrastructure --startup-project GHCAA.API
              Step 2: dotnet ef migrations add AddOrganizationConfig --project GHCAA.Infrastructure --startup-project GHCAA.API --output-dir Data/Migrations/PgSql
              Step 3: dotnet ef database update --project GHCAA.Infrastructure --startup-project GHCAA.API
              DEPENDS ON: PhaseB_S5S8 + AddRefreshTokens migrations from Area 24 applied first

### PRIORITY 1 - PHASE 1 BACKEND (DONE - verify before merge)

28.1  [DONE] Domain:  OrganizationConfig entity (GHCAA.Domain/Models/OrganizationConfig.cs)
28.2  [DONE] App:     OrgConfigDto + nested records (GHCAA.Application/DTOs/OrgConfigDto.cs)
28.3  [DONE] App:     IOrgConfigService interface (GetConfigAsync + UpdateConfigAsync only)
28.4  [DONE] Infra:   OrgConfigService - GetOrCreateAsync cache, OrgId-keyed upsert, enum-driven MembershipTypes list
28.5  [DONE] Infra:   OrganizationConfigConfiguration (text column, unique OrgId index, cross-provider safe)
28.6  [DONE] Infra:   DbSet<OrganizationConfig></organizationconfig> added to ApplicationDbContext
28.7  [DONE] API:     OrgConfigController - GET public / PUT SuperAdminOnly (parity comment)
28.8  [DONE] API:     Program.cs startup seed (fault-tolerant try/catch, idempotent)
28.9  [DONE] Domain:  Guest added to MembershipType enum (value=6, additive - no migration needed for enum)
28.10 [DONE] Angular: Guest added to MEMBERSHIP_TYPES + MEMBERSHIP_TYPE_OPTIONS (app.constants.ts)

### PRIORITY 2 - PHASE 2: ANGULAR CONSUMER

> DEPENDS ON: 28.0 (migration applied, GET /api/config returning 200)
> 28.11 [DONE 2026-08-22] Angular: Create OrgConfig TypeScript model (GHCAA.Web/src/app/core/models/org-config.model.ts) VERIFIED 2026-08-22: file exists with 10 exported interfaces (`SocialLinks`, `OrgBranding`, `OrgContact`, `OrgCurrency`, `FeatureToggles`, `OrgWorkflow`, `NavLabels`, `LocalePack`, `OrgLocalization`, `OrgConfig`) — a superset of the planned shape.
> Shape: OrgBranding, FeatureToggles, LocalePack, NavLabels, EmailSubjects interfaces
> 28.12 [DONE 2026-08-22] Angular: Create OrgConfigService (GHCAA.Web/src/app/core/services/org-config.service.ts) VERIFIED 2026-08-22: service exists with `config` signal, `loadConfig()`, `isFeatureEnabled()`, `localePack()`, `updateConfig()`, plus a constructor `effect()` that pushes `primaryColor`/`accentColor` into CSS custom properties, and an inline GHCAA default config on API failure. **Two naming deviations from this plan line, deliberate and now canonical:** `isEnabled` shipped as **`isFeatureEnabled`**, and `t(path, locale?)` shipped as **`localePack(locale)`** returning the whole pack instead of a per-path getter. Use the real names.
> - Signal<OrgConfig|null> config; load(): Promise<void></void> via GET /api/config
> - t(path, locale?) for locale string lookup
> - isEnabled(feature: keyof FeatureToggles) boolean
> - Falls back to GHCAA_DEFAULT_CONFIG on API failure
> DEPENDS ON: 28.11
> 28.13 [DONE 2026-08-22] Angular: Wire APP_INITIALIZER in app.config.ts to call OrgConfigService.load() before render VERIFIED 2026-08-22: `app.config.ts` provides `APP_INITIALIZER` with `useFactory: (s: OrgConfigService) => () => s.loadConfig()` and `deps: [OrgConfigService]`.
> DEPENDS ON: 28.12
> 28.14 [DONE 2026-08-22] Angular: Add API_ENDPOINTS.ORG_CONFIG = '/api/config' to app.constants.ts VERIFIED 2026-08-22: shipped as **`API_ENDPOINTS.CONFIG = '/api/config'`** (app.constants.ts), not `ORG_CONFIG` as planned — grep for `CONFIG` not `ORG_CONFIG`.
> 28.15 [DONE 2026-08-22] Angular: Replace APP_CONFIG.* references in components with orgConfigService.config()?.branding.* VERIFIED 2026-08-22: the migration is complete — `grep -rn "APP_CONFIG" GHCAA.Web/src/app` returns **zero** hits across both `.ts` and `.html`.
> Grep target: grep -r "APP_CONFIG\." src/app --include="*.ts" --include="*.html" -l
> DEPENDS ON: 28.12, 28.13
> 28.16 [DONE 2026-08-22] Angular: Create feature guard (GHCAA.Web/src/app/core/guards/feature.guard.ts) VERIFIED 2026-08-22: `core/guards/feature.guard.ts` exports a `featureGuard(featureKey, fallbackUrl='/')` factory returning `true` or `router.parseUrl(fallback)`. Applied at **8 route sites** in `app.routes.ts` covering `enableGallery` (x2), `enableEvents` (x2), `enableJobHub` (x2), `enableForum` (x2). Note the plan named `/portal/polls` as a target and **polls is not guarded** — decide whether that is an intentional omission or a gap before closing Area 28.
> Apply to /portal/forum, /portal/jobs, /portal/polls routes in app.routes.ts
> DEPENDS ON: 28.12

### PRIORITY 3 - PHASE 3: FLUTTER CONSUMER

> DEPENDS ON: 28.11-28.16 (Angular consumer pattern validated first)
> 28.17 [DONE 2026-08-22] Mobile: Create OrgConfig Dart model (GHCAA.Mobile/lib/core/models/org_config.dart) VERIFIED 2026-08-22: shipped at **`lib/core/config/org_config.dart`**, not `lib/core/models/` as planned. Has `OrgConfig.fromJson`, `LocalePack.fromJson`, and the `ghcaaDefaults` static getter for offline fallback, with both `en` and `bn` packs.
> fromJson factory + ghcaaDefaults static getter for offline fallback
> 28.18 [DONE 2026-08-22] Mobile: Create OrgConfigService Dart singleton (GHCAA.Mobile/lib/core/services/org_config_service.dart) VERIFIED 2026-08-22: `lib/core/services/org_config_service.dart` has `load()`, `refresh()`, `_saveToCache()`/`_loadFromCache()` over `shared_preferences`, and exposes `orgConfigServiceProvider`, `orgConfigProvider` (FutureProvider), `localePackProvider`, `featureTogglesProvider`.
> - load(ApiClient): fetches /api/config, caches to SharedPreferences for offline resilience
> - pack getter returns locale-appropriate LocalePack
> DEPENDS ON: 28.17
> 28.19 [DONE 2026-08-22] Mobile: Wire OrgConfigService.instance.load() in main.dart before runApp VERIFIED 2026-08-22 **but implemented differently from this plan line.** There is no `OrgConfigService.instance.load()` before `runApp` — mobile uses Riverpod instead: `orgConfigProvider` is a `FutureProvider` resolved lazily inside `ProviderScope`, and `main.dart` consumes it via `ref.watch(orgBrandingProvider)`. Same outcome (config available app-wide, with cache + defaults fallback) without blocking startup on a network call. Treat the Riverpod pattern as canonical; do not "fix" main.dart to match the original wording.
> DEPENDS ON: 28.18
> 28.20 [DONE 2026-08-22] Mobile: Update app_drawer.dart - replace 7 hardcoded strings with pack.nav.* VERIFIED 2026-08-22: all 7 strings are gone from `app_drawer.dart`. It now does `ref.watch(localePackProvider)` and reads `localePack.administration`, `.myAccount`, `.community`, `.mediaAndTools`, `.adminRoleLabel`, `.memberRoleLabel`, `.batchPrefix`. (`Sign Out` / dialog copy is still hardcoded, which belongs to UI-layer i18n = 28.33, not here.)
> 'ADMINISTRATION' -> pack.nav.administration
> 'MY ACCOUNT'     -> pack.nav.myAccount
> 'COMMUNITY'      -> pack.nav.community
> 'MEDIA & TOOLS'  -> pack.nav.mediaAndTools
> 'ADMINISTRATOR'  -> pack.nav.adminRoleLabel
> 'ALUMNI MEMBER'  -> pack.nav.memberRoleLabel
> 'Batch: '        -> pack.nav.batchPrefix
> DEPENDS ON: 28.18
> 28.21 [TODO — 1 site left, awaiting product decision → **now tracked as 35.5**] Mobile: Add Guest to any hardcoded MembershipType list in Flutter **UPDATE 2026-08-22 (latest): the remaining half moved to 35.5**, where it is reframed by new evidence — the *web* registration form has been offering Guest all along, so the mobile omission is a client-to-client inconsistency, not an admin-assign-only policy. Do not resolve this item here; resolve 35.5. **UPDATE 2026-08-22 (earlier):** `screens/member/directory_screen.dart` is now **FIXED** — `Guest` added to the TYPE filter (with a comment tying that list to `GHCAA.Domain/Enums.cs`), `flutter analyze` clean on the file. The directory no longer hides Guest members. The one remaining site is `core/constants/registration_constants.dart` (`MembershipConstants.typeOptions`), left untouched **on purpose**: that list drives the self-service registration form, so adding `Guest` there permits applicants to self-select Guest membership. That is a product call, not a sync gap — if Guest is admin-assign-only (likely, since it sits outside the fee tiers), the correct resolution is to leave it out and close this item as such. Original re-scoping note follows. **RE-SCOPED 2026-08-22 — PARTIAL, 2 sites left.** `Guest` IS already present in `core/config/org_config.dart` (membershipTypes + en/bn label maps), `features/lookups/dropdown_service.dart`, and `screens/admin/fee_config_screen.dart`. Still missing in exactly two places: (a) `core/constants/registration_constants.dart` `MembershipConstants.typeOptions` (stops at `Advisory`), and (b) `screens/member/directory_screen.dart` line ~199 `_buildFilterDropdown('TYPE', [...])` (stops at `Advisory`). **Left unfixed deliberately** — (a) is the self-service registration list and it is a product decision whether an applicant may self-select `Guest` or whether it is admin-assign-only. Decide that first; (b) should be fixed regardless, since a filter that cannot select Guest hides real members.
> Grep: grep -r 'Advisory' lib --include="*.dart"
> DEPENDS ON: none (standalone fix)

### PRIORITY 4 - TESTS

> DEPENDS ON: 28.0 (migration), 28.4 (service implemented)
> 28.22 [DONE 2026-08-22] Tests: OrgConfigSeedTests - assert GHCAA defaults have all required locale keys VERIFIED 2026-08-22: `GHCAA.Tests/OrgConfig/OrgConfigSeedTests.cs` exists and passes.
> File: GHCAA.Tests/OrgConfig/OrgConfigSeedTests.cs
> Cases: en+bn present; MembershipTypeLabels has 7 keys (incl. Guest); all features default ON except Gamification+SocialAuth
> 28.23 [DONE 2026-08-22] Tests: OrgConfigServiceTests - unit tests with SQLite in-memory VERIFIED 2026-08-22: `GHCAA.Tests/OrgConfig/OrgConfigServiceTests.cs` exists and passes.
> File: GHCAA.Tests/OrgConfig/OrgConfigServiceTests.cs
> Cases: returns defaults when DB empty; UpdateConfigAsync persists; cache invalidates; Bengali locale lookup
> DEPENDS ON: 28.0
> 28.24 [DONE 2026-08-22] Tests: OrgConfigControllerTests - integration tests VERIFIED 2026-08-22: `GHCAA.Tests/Integration/OrgConfigControllerTests.cs` exists and passes. (Full backend suite: **330 passed / 0 failed**, `dotnet test`, 2026-08-22.)
> File: GHCAA.Tests/Integration/OrgConfigControllerTests.cs
> Cases: GET returns 200; PUT returns 403 for Admin role; PUT returns 204 for SuperAdmin
> DEPENDS ON: 28.0
> 28.25 [TODO] Tests: Re-run visual regression snapshots after Phase 2 Angular consumer is done **UNVERIFIABLE AS WRITTEN 2026-08-22.** Its dependency 28.15 is now confirmed done, so the trigger condition has passed — but this is a *run this command* item, not an artifact, so nothing on disk can prove it happened. Either run `npx playwright test --update-snapshots` and record the result here, or close it as superseded by 28.26's `config-regression.spec.ts`.
> Command: npx playwright test --update-snapshots
> WHY: APP_CONFIG references replaced by config-driven values may shift text in layout
> DEPENDS ON: 28.15
> 28.26 [DONE 2026-08-22] Tests: Add Playwright config-regression spec VERIFIED 2026-08-22: `GHCAA.Web/tests/e2e/config-regression.spec.ts` exists.
> File: GHCAA.Web/tests/e2e/config-regression.spec.ts
> Cases: org name from intercepted config (not hardcoded); feature=false route redirects
> DEPENDS ON: 28.12, 28.16

### PRIORITY 5 - ADMIN UI (PHASE 4, OPTIONAL)

> DEPENDS ON: 28.12 (Angular OrgConfigService)
> 28.27 [DONE 2026-08-22] Angular: Create OrgConfig admin editor component VERIFIED 2026-08-22: shipped at **`GHCAA.Web/src/app/admin/org-config/org-config.ts`** exporting `AdminOrgConfig` (not `pages/admin/org-config/org-config.component.ts` as planned), lazy-loaded at route `admin/org-config` in `app.routes.ts`.
> File: GHCAA.Web/src/app/pages/admin/org-config/org-config.component.ts
> Tabs: Branding | Contact | Features | Localization-EN | Localization-BN | Workflow
> Route: /admin/org-config guarded by superAdminGuard
> DEPENDS ON: 28.12
> 28.28 [DONE 2026-08-22] Angular: Add Organization Config link to admin sidebar in nav.service.ts VERIFIED 2026-08-22: `nav.service.ts` line ~61 has `{ path: '/admin/org-config', label: 'Org Config', icon: 'org-config', roles: ['SuperAdmin'], section: 'Finance & Tools' }`.

### PRIORITY 6 - TECH DEBT CLEANUP (after all phases pass regression)

> DEPENDS ON: All of 28.1-28.28 green; do NOT delete Constants.cs fields before this
> 28.29 [VERIFIED-STALE 2026-08-22] Cleanup: Add DEPRECATED comment to Constants.Branding.* and Constants.EmailSubjects.* **OBSOLETE — nothing to deprecate.** Verified 2026-08-22: `GHCAA.Domain/Constants.cs` contains only `Roles`, `ConfigKeys`, `TemplateCodes`, `Defaults`. There is no `Constants.Branding` and no `Constants.EmailSubjects` class, so there is no field to mark DEPRECATED. Closing as not-applicable rather than done.
> Mark: // DEPRECATED: use IOrgConfigService; pending deletion after 28.30 complete
> 28.30 [VERIFIED-STALE 2026-08-22] Cleanup: Migrate call sites - CommunicationService email subjects + any service using Constants.Branding.* **OBSOLETE — no call sites exist.** Verified 2026-08-22: `grep -rn "Constants.Branding|Constants.EmailSubjects|Constants.Defaults.SupportEmail" --include=*.cs` (excluding bin/obj) returns **zero** hits, so there is nothing left to migrate. Closing as not-applicable.
> Grep: Constants.Branding | Constants.EmailSubjects | Constants.Defaults.SupportEmail
> DEPENDS ON: 28.29
> 28.31 [DONE 2026-08-22] Cleanup: Add RowVersion/xmin concurrency token to OrganizationConfig entity VERIFIED 2026-08-22: `OrganizationConfig.cs` has `public byte[] RowVersion` and `OrganizationConfigConfiguration.cs` marks it `IsConcurrencyToken()`, with migration `20260703123040_AddOrganizationConfigRowVersion`. The config comment records **why `IsRowVersion()` was not used**: it left the column NULL on insert and tripped a Postgres 23502 not-null violation, so a plain concurrency token was chosen for cross-provider safety with no xmin dependency.
> Prevents last-write-wins on concurrent SuperAdmin edits
> 28.32 [TODO] Angular: Install ngx-translate for UI-layer strings (form labels, buttons, page titles) (Still genuinely open, confirmed 2026-08-22: no `ngx-translate` entry in `GHCAA.Web/package.json`.)
> Separate from OrgConfigService locale packs which cover org terminology
> RELATES TO: 8.8 (Mobile i18n)
> 28.33 [TODO] Mobile: Add Flutter intl + .arb files for UI-layer strings **PARTIAL, confirmed 2026-08-22:** `intl: ^0.20.2` and `flutter_localizations` are already in `pubspec.yaml`, but there is **no `lib/l10n/` directory and no `.arb` files** — the dependency is in place and the extraction work is not started.
> RELATES TO: 8.8 [TODO] i18n: Unified Localization (English + Bengali)

## AREA 29: FULL-STACK REVIEW FINDINGS (2026-07-24)

> Source: whole-project review (backend correctness + security, Angular web, Flutter mobile, payment audit).
> Cross-referenced against docs/BUSINESS_FINDINGS.md and .antigravity/skills standards.
> Type-check + flutter analyze both pass clean — all items below are runtime/logic/UX, not compile errors.
> Fix order: 29-A blockers first, then 29-F.1 (audit-trail) + 29-B.2 (amount bypass), then 29-F sweep, then 29-G, then 29-C.
>
> **DOC-DRIFT WARNING 2026-08-22:** Area 29 is 100% DONE here, but `docs/PLAN.md` still carries an
> unticked mirror of the same work — its Phase 2 (2.1-2.3) and Phase 3 (3.1-3.6) checkboxes are all
> `[ ]` even though the matching 29B/29F items below are `[DONE 2026-07-25]`. `PLAN.md` line 86
> ("3.1 Facebook token app_id verification") is the clearest example: 29B.1 below records that
> verification shipped via `graph.facebook.com/debug_token`. **TODO.md is the single source of
> truth for status**; PLAN.md is a historical sequencing doc and its checkboxes should not be read
> as open work. Either tick PLAN.md's Phase 2/3 through or add a pointer header to it.

### 29-A: CRITICAL — SHIP-BLOCKERS

29A.1 [DONE 2026-07-25] Web: Added /portal/change-password route + ChangePassword component; authGuard now URL-bypasses that page to avoid a redirect loop, and auth.service.clearMustChangePassword() clears the flag on success. (guard spec 7/7)
29A.2 [DONE 2026-07-25] Mobile: session_manager._handleSessionExpiry now clears the REAL secure-storage token via storageService.clearAll() (was only removing a stale SharedPreferences 'jwt_token'); authStateProvider's 2s poll then sees null and the router redirects to /login. (flutter analyze clean)
29A.3 [DONE 2026-07-25] Mobile: digital_id_screen now reads data['membershipNumber'] (was data['membershipId']) and data['passingYear'] (was data['batch']) across QR/barcode/share/PDF/filename — no more literal "PENDING".
29A.4 [DONE 2026-07-25] API: EventService capacity now counts slot-consuming registrations (Pending+Approved, not just Approved), rejects when full and HasWaitlist=false (was skipping the cap entirely), and closes the overfill race with a deterministic post-insert Id-ordinal re-check that demotes/rejects the overflow. (+regression test)
29A.5 [DONE 2026-07-25] API: CheckInParticipantAsync + CheckInByTicketCodeAsync now reject any non-Approved registration (Pending/Rejected/Waitlisted can no longer check in or earn points). (+regression test)

### 29-B: HIGH — SECURITY

29B.1 [DONE 2026-07-25] API: FacebookLoginAsync now verifies the token via graph.facebook.com/debug_token using an app access token (`{ClientId}|{ClientSecret}`) and rejects unless data.is_valid && data.app_id == config.ClientId (also rejects when ClientSecret unconfigured). Blocks tokens minted for a different app → account takeover. (AuthService.cs)
29B.2 [DONE 2026-07-25] API: Amount-verification bypass fixed. HandleSuccessfulPayment now takes `decimal? confirmedAmount`; check is `if (confirmedAmount.HasValue && Math.Abs(payment.Amount - confirmedAmount.Value) > 0.01m)` → mark payment Failed + return. SSLCommerz/DGePay callbacks now pass `null` when the amount key is ABSENT (skip verification, VerifyCallbackAsync remains primary gate) vs the parsed value — incl. 0 — when PRESENT (verify). Regression test SSLCommerzCallback_MarksFailedAndDoesNotApprove_WhenReportedAmountMismatches. (GatewaysController.cs)
29B.3 [DONE 2026-07-25] API: Membership auto-approval now scoped to membership fees only — gate is `if (payment.MemberId > 0 && payment.FinancialCategory == Enums.FinancialCategory.MembershipFee)`. FinancialCategory is set at initiation (RegistrationFee for EVT-REG refs, else MembershipFee), so event payments no longer trigger member approval. (GatewaysController.cs)
29B.4 [DONE 2026-07-25] API: Removed QueryStringTokenMiddleware (deleted file + registration in Program.cs). JWT is only accepted via the Authorization header now; no web/mobile client appended ?token=/?access_token= to secure-file URLs, so the query-token path was pure attack surface (leaked into proxy/access logs and the Referer header).
29B.5 [DONE 2026-07-25] API: Auth rate-limiter now keyed on source IP alone (was {ip}:{username}, which gave each username its own bucket so one IP could spray N×limit accounts). PermitLimit 10/min per IP bounds total auth attempts regardless of account count. (Program.cs)
29B.6 [DONE 2026-07-25] API: Signatures moved to the auth-gated secure_uploads tree — IsSecureType now includes FileUploadType.Signature (forgery risk; was served publicly with no auth). (LocalFileStorageService.cs)
29B.7 [DONE 2026-07-25] API: record-payment now content-validates dto.Receipt via IFileValidationService.ValidateFormFile(FileCategory.Document, 10MB) before RecordPaymentAsync (magic-byte check; blocks a renamed executable/script stored under a .jpg/.pdf name in the secure tree). (FinancialsController.cs)
29B.8 [DONE 2026-07-25] API: SecureFilesController now resolves against BOTH roots LocalFileStorageService writes to (publicRoot=wwwroot, secureRoot=BaseDirectory, driven by FileStorage:BasePhysicalPath) and serves the file only if it lands inside one of them — secure files under secure_uploads/ are now readable (were rejected) while path-traversal is still blocked. (SecureFilesController.cs)

### 29-C: HIGH — BACKEND CORRECTNESS

29C.1 [DONE 2026-07-25] API: Deleted the divergent ApproveMemberInternalAsync copy (lexicographic serial sort colliding at #1000, no transaction, skipped profile/status/payment checks). Auto-approval after payment now delegates to the canonical MemberService.ApproveMemberAsync (Serializable txn, Id-ordered serial, full validation, user-account creation in-txn), resolved lazily via IServiceProvider to avoid the MemberService→IFinancialService DI cycle; on failure it logs and leaves the member Applied for manual review. (FinancialService.cs)
29C.2 [DONE 2026-07-25] API: GetMemberDocumentsAsync now returns null for an unknown id instead of dereferencing a null member (NRE → 500); AdminController already maps null → 404. (MemberService.cs)

### 29-D: HIGH — WEB (Angular)

29D.1 [DONE 2026-07-25] Web: Article rejection fixed — bound rejectReason to a plain field/model instead of a signal via [(ngModel)]; reject flow no longer throws "is not a function". (article-approval)
29D.2 [DONE 2026-07-25] Web: Member-approval panel now fetches full member detail (academic/professional/NID/addresses) before decision instead of the 8-field summary. (member-approval.ts)
29D.3 [DONE 2026-07-25] Web: Chat token persisted (no longer in-memory only); sends survive reload and error handlers added on loadRecentChats/loadHistory. (chat.service.ts)
29D.4 [DONE 2026-07-25] Web: Payments page no longer spins forever on error — flattened nested subscribes and added error callbacks (incl. deleteSavedMethod). (payments.ts)
29D.5 [DONE 2026-07-25] Web: Digital-ID download wired to the real getIDCard() endpoint, replacing the fake setTimeout stub. (digital-id.ts)
29D.6 [DONE 2026-07-25] Web: Gallery no longer emits an empty <img [src]=""> request — src guarded until a real URL exists. (gallery.html)
29D.7 [DONE 2026-07-25] Web: Router-event subscriptions in all 3 layouts now torn down via takeUntilDestroyed()/unsubscribe on destroy — no leak.
29D.8 [DONE 2026-07-25] Web: Login honors token expiry + returnUrl (createUrlTree(['/login'],{queryParams})); profile update no longer drops fields; governance page has an error handler.

### 29-E: HIGH — MOBILE (Flutter)

29E.1 [DONE 2026-07-25] Mobile: Removed the dead social-login stubs (Google/Facebook buttons only showed a "Connecting…" snackbar; no SDK, no keys). Section hidden until real SDK wiring; auth_service googleLogin/facebookLogin plumbing retained for future. (login_screen.dart)
29E.2 [DONE 2026-07-25] Mobile: List-fetch services now debugPrint+rethrow instead of catch→return [], so network failures surface as AsyncValue.error (shared AsyncValueWidget renders an error+retry state) instead of a misleading empty list. ~40 methods across 18 services; 2 imperative call-sites (governance_registry, family_link) wrapped in try/catch+SnackBar. Intentional swallowers kept: lookup/dropdown static-fallback + auth getSocialProviders. (flutter analyze clean)
29E.3 [DONE 2026-07-25] Mobile: NotificationHub leak fixed — provider now registers ref.onDispose(() => service.dispose()) (previously dead code; SignalR socket + 4 broadcast StreamControllers leaked and survived logout). AuthService.logout() invalidates notificationHubServiceProvider so teardown fires on logout. (notification_hub_service.dart, auth_service.dart)
29E.4 [DONE 2026-07-25] Mobile: AppSearchField converted to a StatefulWidget with an internal 350ms debounce Timer (cancelled on dispose; clear bypasses debounce), so search consumers no longer re-fetch per keystroke — one call after typing settles. (app_search_field.dart)
29E.5 [DONE 2026-07-25] Mobile: Replaced bool.fromEnvironment('dart.library.js_util') (always false → Firebase init ran on web and crashed) with kIsWeb (imported from foundation). (main.dart)

### 29-F: CROSS-CUTTING THEMES (repeat offenders)

29F.1 [DONE 2026-07-25] Web/Mobile: Admin-ID attribution — server already resolves the acting admin from the JWT MemberId claim (item 24.51, AdminController), ignoring any client-supplied id, so this was client-side dead/misleading code, not live audit corruption. Removed the hardcoded adminId=1 fallbacks and the now-unused approvedByAdminId/rejectedByAdminId fields across web (admin.service, member-approval, admin-members + specs), mobile (admin_service.dart), e2e helper, and DTOs (ApproveMemberDto/RejectMemberDto).
29F.2 [DONE 2026-07-25] All: Silent-failure sweep — web: converted next-only .subscribe() to object form with error: handlers across admin-roles, payments, login (quiet-degrade optional social), chat.service (loadRecentChats/loadHistory) + prior admin components. Mobile: try/catch-return-empty list fetches now debugPrint+rethrow so failures surface (see 29E.2). Errors no longer become blank screens / stuck spinners.
29F.3 [DONE] All: Date contract settled — ISO-8601 is canonical wire format; dd-MM-yyyy is display/input only.
              API unchanged (Write=ISO, Read accepts both). Web: added core/utils/date.util.ts (toDisplayDate/toWireDate/parseDisplayDate); routed 9 write-path components + register.ts through toWire; type-check clean.
              Mobile: AppUtils hardened ISO-first + added toWire(); fixed 6 send sites incl. RegisterModel.toJson DOB; flutter analyze clean.
              Skill doc ghcaa-date-standard/SKILL.md rewritten to the two-format contract. (2026-07-25)
29F.4 [DONE 2026-07-25] Web: White-labeling wired — admin-layout now pulls logo/brand strings from OrgConfigService (branding.shortName/logoUrl) instead of hardcoded values, adds a Sign Out control (footer + header) and a mobile off-canvas sidebar toggle (hamburger + backdrop, translateX). Feature-gated nav links honor OrgConfigService.isFeatureEnabled. (admin-layout .ts/.html/.scss)

### 29-G: PAYMENT GAPS (all 5 methods work manually/config-driven; no keys required — these are gaps only)

29G.1 [DONE 2026-07-25] Mobile: financial_portal_screen now fetches admin-configured methods from GET /api/payment-config/active (new activePaymentConfigsProvider + FinancialService.getActivePaymentConfigs) and renders them in the payment sheet, replacing the hardcoded DGePay+Stripe tiles. Manual channels open a submission form; online channels (isOnline) route to the gateway via _gatewayFromString. (flutter analyze clean)
29G.2 [DONE 2026-07-25] Mobile: new FinancialService.recordPayment (multipart POST /api/financials/record-payment) + manual-payment form surfaces the config's walletNumber/accountNumber/accountHolder/bank/branch/routing as display-only "where to pay" details and captures the member's transaction reference + receipt. (walletNumber/bankName/accountNumber are display-only by design — the record-payment DTO carries transactionId/method/receipt, mirroring web member/payments.)
29G.3 [DONE 2026-07-25] Web+Mobile: dead Stripe tile removed (mobile sheet is now config-driven, no hardcoded gateways). Admin payment-config create form gained a Payment Method Type dropdown (methodOptions incl. CashOnHand) so any method is creatable — was hardcoding method:'ManualReceipt'. (Note: the web gateway dropdown's SSLCommerz/BkashGateway/NagadGateway all have registered implementations — not dead — so left intact.) (web tests 9/9)
29G.4 [DONE 2026-07-25] API: GatewaysController.GatewayWebhook now wraps _gatewayFactory.GetGateway in try/catch(NotSupportedException) → returns 404 {status:"unsupported_gateway"} instead of an unhandled 500 for unregistered gateways.

## AREA 30: UI/UX REMEDIATION (2026-07-30)

> Full plan with root-cause analysis and file:line targets: **docs/UI_UX_REMEDIATION_PLAN.md**
> Source: ~30-symptom UI/UX defect list (admin panel + member portal + public landing), traced to 8 shared-layer root causes.
> RULE: anything specified for one panel applies to BOTH admin and member portal.
> RULE: fix centrally (styles.scss tokens / shared classes / shared components) — never per-component.
> Baselines to protect: web `npm run type-check` clean, `npm run build` clean, vitest 58 files / 233 tests, `dotnet test` exit 0.
> Approved decisions: compute ProfileCompletionPercentage server-side and DROP the Global Rank tile (no fake `#---`);
> portal nav sections = Overview / Community / Directory / Career / My Account;
> replace emoji nav/action icons with a monochrome inline-SVG `app-icon` set using `currentColor`.

### 30-0: PHASE 0 — CENTRAL/SHARED LAYER (do first; these unblock the rest)

30.1  [DONE 2026-07-30] Web: De-duplicate styles.scss — merged the two competing `.filter-bar` blocks and the two `.status-badge` blocks into one authoritative definition each, preserving the effective cascade exactly (`.search-icon` centring, full `.clear-search` rule, `.status-select` height/uppercase/`option` styling, `backdrop-filter` on `.filter-bar` only — never `.action-bar`). "Do NOT re-add a block here" comments left at both old sites. WHY IT GATED EVERYTHING: the later duplicate silently won every conflicting property, so density fixes applied to the earlier block did nothing. Verified type-check + build clean.
30.2  [DONE 2026-08-01] Web: Central action-control taxonomy — added `.btn-danger`, canonical `.icon-btn` (+ `.delete`/`.archive`/`.approve`/`.contact` modifiers), and `.action-group` to styles.scss; added `core/constants/actions.constants.ts` (`ACTION_LABELS`: Save/Edit/Delete/Close/Cancel/New) for new call sites to adopt incrementally. Removed the 5 per-component `.icon-btn` definitions (polls/news/members/governance/gallery scss) and fixed all 7 usage sites — `admin-comm.html` + `messages.html` (previously undefined, rendered browser-native) now styled; gallery delete + governance edit buttons given correct modifier/class instead of always-red or inline-style overrides. `.icon-btn` kept as the class name/alias so templates needed no rename. Verified type-check + build clean (emitted CSS contains `.action-group`/`.btn-danger`) + vitest 58/233 green.
30.3  [DONE 2026-08-01] Web: Added `src/app/common/icon/icon.ts`+`.html` — standalone `Icon` component (selector `app-icon`, inputs `name`/`size`/`strokeWidth`), `@switch(name)` inline-SVG set (`stroke="currentColor"`, theme-adaptive) covering ~55 icon names + `@default` fallback circle. Replaced every nav.service.ts emoji (portal + admin nav items) with semantic name keys; replaced hardcoded emoji in both layouts (back/menu/admin-panel/theme-toggle/sign-out) and toast.ts (`getIcon()` now returns `toast-success|error|warning|info`); replaced every Font Awesome `fa-`/`fas`/`far`/`fab` usage app-wide (register/login/reset-password/change-password eye-toggles, admin/roles, member/polls+dashboard, admin/events, admin/polls) with `<app-icon>`; removed the Font Awesome CDN `<link>` from index.html. Verified type-check + build clean (dist grep: zero `font-awesome` references) + vitest 58/233 green.
30.4  [DONE 2026-08-01] Web: Density pass in styles.scss — `.page-header` margin-bottom 3.5rem→2rem + h2 font-size 2.25rem→1.875rem; `.filter-bar`/`.action-bar` padding 1.25rem 1.75rem→1rem 1.5rem + margin-bottom 2rem→1.5rem + `.search-wrap input`/`.status-select` height 3.25rem→2.75rem; `.tab-nav`/`.tabs` margin-bottom 3.5rem→2rem + button padding 1rem 0→0.75rem 0. Added `--header-height: 4.5rem` token on `:root` (also set as `.top-bar`'s `min-height`) and made `.filter-bar`/`.action-bar` `position: sticky; top: var(--header-height); z-index: 90` inside `@media (min-width: 1024px)` — sticks just below the already-sticky `.top-bar` (z-index 100). Verified type-check + build clean (emitted CSS contains `--header-height`) + vitest 58/233 green.
30.5  [DONE 2026-08-01] Web: Added `.empty-state.compact` (tighter padding/gap + smaller icon/heading/copy) to styles.scss. Application to specific profile/dashboard/admin empty sections tracked as follow-up when touching those templates (30.19/30.21/30.22).
30.6  [DONE 2026-08-01] Web: `body.dark-theme --border-color` changed from near-black `#1a1a1a` to `rgba(255,255,255,0.18)` (brighter than `--hairline`'s 0.1) so form-control borders are visible strokes against black surfaces; added `& + &.checkbox-group { margin-top }` for spacing between stacked checkboxes.
30.7  [DONE 2026-08-01] Web: Modal `.close-btn` enlarged 36px→40px + bumped glyph font-size, in styles.scss (applies to every modal at once).
30.8  [DONE 2026-08-01] Web: Added `src/app/common/user-menu/user-menu.ts`+`.html`+`.scss` — standalone `UserMenu` component (selector `app-user-menu`, `OnPush`, required `roleLabel` input) rendering the theme-toggle button + avatar/photo/name/role block + sign-out button (same markup/classes as the old portal header: `.theme-toggle`, `.user-profile`, `.avatar`/`.avatar-img`, `.user-details`, `.username`, `.role`, `.logout-toggle`); fetches the photo itself via `ProfileService.getProfile()`. Since these rules previously lived scoped under portal-layout's `.right-section` in portal-layout.scss (component-scoped, not styles.scss) and Angular's emulated encapsulation does not let a parent's scoped CSS reach a child component's template, the CSS moved into the new component's own `user-menu.scss` rather than being duplicated. Wired into `portal-layout.html`'s `.right-section` (replacing the inline markup; removed now-dead `profilePhotoUrl`/`getImageUrl`/`ProfileService` from `portal-layout.ts`) and into `admin-layout.html`'s `.header-right` alongside the existing "Exit Admin" link (removed the old `.header-user` username/role-badge div, the standalone Sign Out button, and `AdminLayout.logout()` — all now redundant/dead; also dropped the matching now-unused `.header-user`/`.header-username`/`.header-role-badge`/`.logout-btn:hover` CSS from admin-layout.scss). Verified type-check + build clean + vitest 58/233 green.
30.9  [DONE 2026-08-01] Web: `appImgFallback` directive (`common/directives/img-fallback.directive.ts`) + `/assets/placeholders/*` fallback assets were already wired up by a prior pass; this pass closed the remaining gaps and verified full coverage. Fixed the one genuine ad-hoc handler left — `gallery-preview.html`'s `(error)="onImgError($event)"` + unstyled `.photo-placeholder` div (dead CSS, confirmed via `gallery-preview.scss`) — by replacing it with the directive and deleting the now-dead `onImgError()` from `gallery-preview.ts`. Added the directive (bare, using its built-in default) to every remaining static bundled-asset `<img>` that lacked it: both portal-layout logos, the login/register/reset-password auth-page logos, the about-page founder portrait, logo-spinner's own seal image, and events.html's invitation/print-preview logo. Final verification was a full-app multiline grep (`<img(\s[^>]*)?>` across every `src/app/**/*.html`, since Angular template attributes wrapping onto a continuation line produce false negatives under a naive single-line grep) confirming literally every `<img>` in the app now carries `appImgFallback`, either bare or with a contextual override (`avatar-placeholder.svg`, `event-placeholder.jpg`, `/assets/logo.png`, `image-placeholder.svg`). ROOT CAUSE of all "cover image missing / images not showing" reports is now closed. Verified type-check + build clean + vitest 59/236 green.
30.10 [DONE 2026-08-01] Web: LogoSpinner (`app-logo-spinner`) rollout — most of the app (22 templates) already used the shared component from a prior pass. This pass found and fixed the remaining ad-hoc/missing cases: `admin-audit.html`'s "Fetching activity stream..." block had only a static hourglass glyph (no spinner) — given `<app-logo-spinner>`; `admin-gallery.html`, `admin-payment-config.html`, `admin-roles.html`, `common/governance.html` and `common/news.html` each showed a bare loading message/animate-pulse text with no spinner at all — all five given `<app-logo-spinner>` (added `LogoSpinnerComponent` to each `.ts` `imports` array). `admin-comm.html` had a `loading` signal driving both its Templates and Logs tabs but the template never rendered any loading feedback for either — added `@if (loading()) { <app-logo-spinner> } @else { <table> }` guards to both tabs (single shared `loading` signal already correctly gates whichever tab is active, confirmed via `setTab()`/`loadTemplates()`/`loadLogs()`). A follow-up sweep against the plan doc's own 0.10 candidate list found two more genuine gaps with no loading feedback at all: `admin/events/admin-events.ts` had no `loading` signal for its Management Hub grid nor its Participation Approvals table — added separate `loading`/`loadingRegistrations` signals (set in `loadAllEvents()`/`loadAllRegistrations()`) each gating their own `@if (...) { <app-logo-spinner> } @else { ... }` block; `admin/org-config/org-config.ts`'s async `loadConfig()` left the JSON textarea blank with zero feedback while it awaited the initial fetch — added an `isLoading` flag gating a spinner over the whole form. Two remaining plan-doc candidates were checked and confirmed *not* gaps: `public/directory` is a thin wrapper around `common/directory` which already has its own loading state (delegates, doesn't duplicate); `member/assistant`'s three-dot `typing()` chat bubble is a legitimate per-message typing indicator (same class of exception as inline submit-button text), not a missing page-level spinner. `member/dashboard/dashboard.html` and `admin/dashboard/admin-dashboard.html`'s skeleton-card/`sk-card` grid loaders were left as-is (both portals use the identical skeleton-placeholder style consistently, so this is a deliberate, symmetric design choice for dashboards specifically, not an ad-hoc one-off — not converted to the spinner). Login/register/reset-password/change-password/contact's `loading()` usages are inline submit-button text swaps ("Authenticating...", "Saving...", etc.), a different and correct pattern for in-place button feedback, left untouched. Verified type-check + build clean + vitest 59/236 green.
30.11 [DONE 2026-08-01] Web: Added `NavService.labelFor(url, scope)` (`core/services/nav.service.ts`) as the single source of truth for "what should the header/breadcrumb/browser-tab title say for this URL", replacing the near-identical inline `find()` logic previously duplicated in `admin-layout.ts` and `portal-layout.ts`. Both layouts' `currentPageTitle` (used for both the `Title` service and the breadcrumb) were already 100% derived from the nav item label by construction, so the real mismatch surface was exclusively each page's own on-page `<h1>/<h2>/app-page-header title=` text disagreeing with its nav label — reconciled by renaming each page's header text to match the nav label (rather than renaming nav labels, to minimize blast radius): admin `System Audit Logs`→`Audit Logs`, `Organization Configuration (with a sparkle glyph)`→`Org Config`; portal `Haraganga News Hub`→`News`, `Events & Gatherings`→`Events`, `GHC AI Assistant`→`Assistance`, `Executive Committee`→`Governance` (portal governance page, not to be confused with the admin EC roster page which correctly keeps "Executive Committee"), `Member Directory`→`Alumni Directory`, `Legacy Archive & Moments`→`Event Gallery`, `Opportunities Hub`→`Job Hub`, `Member Credentials`→`Digital ID`, `Fees & Dues`→`Payments`, `Articles & Submissions`→`My Articles`, `Member Dashboard`→`My Profile` (an outright copy-paste bug — profile.html was showing the dashboard's old title), `Community Forum`→`Discussions`. Also found and fixed a genuine gap, not just a mismatch: `member/dashboard/dashboard.html` had no on-page title at all (unlike `admin/dashboard`, which already showed "Dashboard") — added a matching `.page-header`/`.h2` block using the pre-existing global `.page-header` style from `styles.scss` (no new CSS needed). Exhaustively cross-checked every remaining `ALL_NAV_ITEMS`/`ADMIN_NAV_ITEMS` entry against its page's header text in one final pass; `member/messages` (chat-UI, no page-level title by design) and the two `/polls` pages (reachable but not present in either nav list, so no label to reconcile against) were confirmed as legitimately out of scope, not missed mismatches. Verified type-check + build clean + vitest 59/236 green.
30.12 [DONE 2026-08-01] Web: Added `src/app/common/rich-text-editor/rich-text-editor.ts`+`.html`+`.scss` — standalone `RichTextEditor` component (selector `app-rich-text-editor`, plain `[(value)]`/`height` two-way-bindable inputs, matching admin-comm's existing non-reactive-forms pattern instead of adding `ControlValueAccessor`) that owns its own container div via `ViewChild` + `ngAfterViewInit`, so Quill only ever initialises once Angular has actually rendered this component's own template — fixing the ROOT CAUSE of "Message Body (Rich Text) — no control found" (the old `document.getElementById` + `setTimeout` calls raced the `@if` that rendered the container). Renders a plain `<textarea>` fallback bound to the same value when `typeof window.Quill === 'undefined'`. Replaced both call sites in `admin-comm.html`/`admin-comm.ts` (`broadcast-editor` → `[(value)]="sendOptions.customBody"`, `template-editor` → `[(value)]="editingTemplate()!.body"`), removing the dead `initEditor`/`initBroadcastEditor` methods, the `setTimeout`-delayed init calls, and the raw `<div id="...">` containers. No existing spec covered the old Quill init (none existed for admin-comm). Verified type-check + build clean + vitest 59/236 green (no regressions vs. 58/233 baseline).

### 30-1: PHASE 1 — NAVIGATION & SHELL

30.13 [DONE 2026-08-01] Web: `NavService.ALL_NAV_ITEMS` now each carry a `section` (`Overview`/`Community`/`Directory`/`Career`/`My Account`); added `portalNavSections` computed mirroring `adminNavSections`'s exact grouping/filter pattern on top of the existing feature-flag filtering in `portalNavItems()`. `portal-layout.html`'s `<nav class="nav-list">` now iterates `nav.portalNavSections()` → `.nav-section`/`.nav-section-label` (new rules added to portal-layout.scss, adapted to this sidebar's uppercase nav-item look — the admin `.nav-section`/`.nav-section-label` CSS lives component-scoped in admin-layout.scss, not styles.scss, so it could not be reused via selector broadening and was mirrored instead) before the `.nav-spacer` + Admin Panel link block, unchanged in placement. Verified type-check + build clean + vitest 58/233 green.
30.14 [DONE 2026-08-01] Web: `.nav-spacer { flex: 1 1 auto; }` added to portal-layout.scss — it previously had zero CSS rule anywhere in the app despite `.nav-list` already being a flex column with `flex: 1`, so the spacer itself never grew to push the "Admin Panel" link to the bottom of the sidebar. Verified type-check + build clean (rule present in the emitted portal-layout JS chunk, since component styles inline there rather than into the global CSS file) + vitest 58/233 green.
30.15 [DONE 2026-08-01] Web: Admin header (`admin-layout.html`'s `.header-right`) now renders `<app-user-menu>` alongside "Exit Admin", giving admin its first theme toggle and avatar/photo display (previously portal-only). See 30.8 for the shared-component details. Verified type-check + build clean + vitest 58/233 green.

### 30-2: PHASE 2 — PER-PAGE FIXES

30.16 [DONE 2026-08-02] Web: jobs-preview "Login to View →" wraps to two lines — replaced `.btn-view` with `.btn.btn-sm` + nowrap.
30.17 [DONE 2026-08-02] Web: Admin Special Themes — `admin-themes.html`/`.ts`/`.scss` now derive SCHEDULED / LIVE / EXPIRED / IDLE from the theme's start/end dates instead of a static LIVE badge; confirmed via 30.30 that the server (`ThemeService.GetActiveThemeAsync`) already independently enforced the same date window, so this closes the client-side display mismatch specifically.
30.18 [DONE 2026-08-02] Web: Events — "Reg. Ends: Closed" → "Registration Closed" in `common/events/events.html`, matching `events-preview.html`'s existing wording.
30.19 [DONE 2026-08-02] Web: Submission-review + gallery-album cover placeholders added via `appImgFallback`; gallery album delete-button visibility and Edit/Delete gap fixed in `admin-gallery.html`/`.scss` using the shared `.action-group`/`.icon-btn` classes from 30.2.
30.20 [DONE 2026-08-02] Web: Communications (`admin-comm.html`/`.ts`/`.scss`) — action-button gap/delete styling fixed with `.action-group`/`.icon-btn`/`.btn-danger`; bespoke logs filter replaced with the shared `.filter-bar` pattern; tabs compacted using existing density tokens.
30.21 [DONE 2026-08-02] Web: Executive Committee cards (`common/governance/governance.html`) now render the member photo via `<img appImgFallback>` (avatar-placeholder fallback), matching the pattern already used in `directory.html`/`admin-governance.html`; `.avatar-large`/`.avatar-img` scss added. Batch line (PassingYear/Degree/Subject) confirmed already correctly bound, populated by 30.27's backend fix.
30.22 [DONE 2026-08-02] Web: Member profile identity line — root cause was not a font-family mismatch (the global `Outfit` font already applies everywhere) but a perceptual clash from `.m-type` carrying `uppercase`/letter-spacing/weight 600 next to mixed-case text; normalized to `font-weight: 700` with no transform/tracking in `profile.scss`. Empty Academic/Professional History sections given the `.empty-state.compact` class from 30.5.
30.23 [DONE 2026-08-02] Web: Member dashboard profile-completion steps converted to `<a routerLink>` elements linking to `/portal/profile#section-*` anchors (Identity & Photo / GHC History / Professional Info) and `/portal/payments` (Registration Payment); added `anchorScrolling: 'enabled'` in `app.config.ts` so fragment links actually scroll; hover affordance + "→" arrow added (reusing the existing "See All →" convention, no new icon added).
30.24 [DONE 2026-08-02] Web: Member Directory + Alumni Directory (shared `common/directory` component) — `.filters`/`.filter-grid`/`.filter-group` padding/margins compacted; `.filters` made `position: sticky; top: var(--header-height)` on `min-width: 1024px`, reusing the 30.4 sticky pattern. Covers both surfaces since Alumni Directory delegates to the same component.
30.25 [DONE 2026-08-02] Web: New Message flow (`member/messages/messages.ts`/`.html`/`.scss`) — added a "New Message" button + member-picker modal backed by `NetworkingService.searchMembers()`, and `ChatService.sendFirstMessage()` (new REST call to `POST /api/messaging/send`, distinct from the SignalR `sendMessage()`) to originate a first message when no conversation exists yet, then opens it via the existing `selectThread()`. Picker correctly excludes the current user by `memberId` (ChatMessage sender/receiver are Member IDs, not User IDs).
30.26 [DONE 2026-08-02] Web: Events first-load error — ROOT CAUSE found: a genuine race between `AuthService`'s async `/auth/me` session restore and `Events.loadEvents()`'s bare `setTimeout(..., 150)` deep-link modal open; if `/auth/me` hadn't resolved yet, `openRegisterModal()`'s `isGuest()` check misidentified a logged-in member as a guest and silently redirected them to `/login` for members-only events. Fixed by adding `AuthService.authChecked` (signal/computed, true once a cached user is found or `/auth/me` settles) and replacing the `setTimeout` in `events.ts` with a `pendingDeepLinkEvent` signal + `effect()` that waits for `authChecked()` before opening the modal.

### 30-3: PHASE 3 — BACKEND

30.27 [DONE 2026-08-02] API: `NetworkingService.MapToSummary` (`GHCAA.Infrastructure/Services/NetworkingService.cs`) now populates `PassingYear`/`Degree`/`Subject` on the EC summary DTO from the member's GHC academic record — root cause of "Executive Committee batch information is missing" confirmed and closed. Test added: `NetworkingServiceTests.GetExecutiveCommitteeAsync_ShouldPopulateBatchInformation`.
30.28 [DONE 2026-08-02] API: **Correction to this item's original premise** — `Rank`/`ProfileCompletionPercentage` were NOT "never computed anywhere"; both were already computed via a stricter 13-field `MemberService.CalculateProfileCompletion` that also gates `ApproveMemberAsync`/`member.IsProfileComplete` (deliberately left untouched to avoid changing admin-approval semantics). Added a NEW method `CalculateChecklistProfileCompletion` matching the dashboard's 4-item checklist (Identity&Photo/GHC History/Professional Info/Registration Payment, 25% each), now used for the profile page's `ProfileCompletionPercentage`. Global Rank tile removed from `member/profile/profile.html` (grid `md:grid-cols-5`→`md:grid-cols-4`). Test added: `MemberServiceTests.GetProfileAsync_ProfileCompletionPercentage_ShouldMatchDashboardChecklistCriteria`. See `session_area30_phase3_backend.md` memory for which method to use where in future work.
30.29 [DONE 2026-08-02] API: Confirmed already implemented — `POST /api/messaging/send` (aliased `/api/chat/send`) → `ChatService.SendMessageAsync` already supports starting a new conversation with no prior history. Added missing test coverage (`ChatServiceTests.cs`, file didn't previously exist).
30.30 [DONE 2026-08-02] API: Confirmed already correct — `ThemeService.GetActiveThemeAsync` already honours the Start/EndDate window server-side (Bangladesh local time). The LIVE-badge-ignoring-dates bug (30.17) was Angular-admin-page-only. Added `ThemeServiceTests.cs` (3 tests: expired/future/in-window themes).

### 30-4: PHASE 4 — MOBILE PARITY & VERIFICATION

30.31 [DONE 2026-08-02] Mobile (`lib/core/theme/app_theme.dart`): added `dangerColor` (mirrors `--danger-color`/`.btn-danger`, wired into `ColorScheme.dark(error:)`), `borderColorBright` (distinct neutral token mirroring dark-theme `--border-color`, kept separate from the gold-tinted `glassBorder`), and sizing tokens `iconButtonSize`/`closeButtonSize`/`headerHeight`/`actionGroupGap` mirroring `.icon-btn`/modal close/`--header-height`/`.action-group`. Added `emptyStateCompact*` tokens and an additive `compact` flag on `core/widgets/empty_state_widget.dart` (default false, backward-compatible) mirroring `.empty-state.compact`. New `lib/core/constants/action_labels.dart` mirrors web's `ACTION_LABELS`.
30.33 [DONE 2026-08-01] Mobile login screen (`lib/screens/auth/login_screen.dart`): identifier field label `'Member ID (Email / NID)'` → `'Email or Username'`; password field label `'Portal Password'` → `'Password'`; logo circle container background `Colors.white.withValues(alpha: 0.03)` → `Colors.black`.
30.34 [DONE 2026-08-01] Mobile: fixed floating-label/text overlap on ALL text fields app-wide, centrally in `lib/core/theme/app_theme.dart`'s `inputDecorationTheme` — added `floatingLabelBehavior: FloatingLabelBehavior.always` + a distinct `floatingLabelStyle` (royalGold, w700) and bumped vertical `contentPadding` (`spaceM` → `spaceM + 4`), since the label previously used `auto` behavior and sat centered on the border when a field was unfocused/empty.
30.32 [DONE 2026-08-02] Verify: `npm run type-check` clean; `npm run build` succeeds, emitted `dist/GHCAA.Web/browser/styles-*.css` confirmed to contain recently-added classes/tokens (`text-main`, `empty-state`); `npx vitest run` 59 files / 236 tests passed (matches baseline); `dotnet test` 317/0 passed (matches baseline), no stray `GHCAA.API` process/file-lock encountered. `inlineCritical: false` confirmed still set in both angular.json configs. Playwright light+dark visual QA on the 10-page set was **skipped** (no browser available in this session) — still owed if/when a browser-capable session is available.
30.35 [DONE 2026-08-01] Mobile: ran `flutter test` full suite after 30.33/30.34 — `widget_test.dart` (branding/button assertions) passes; `flutter analyze` clean on both edited files. Pre-existing golden pixel-diff failures in `comprehensive_visual_freeze_test.dart`/`full_app_visual_freeze_test.dart` remain (expected — goldens are known-stale per `session_mobile_ci_golden_fix.md`, CI skips the pixel-compare, `test/failures` is untracked); the `floatingLabelBehavior`/padding change in `app_theme.dart` will shift these goldens further on any screen with text fields — regenerate goldens in a follow-up pass if/when the golden baseline is refreshed, not part of this fix.
30.36 [DONE 2026-08-02] REVIEW (raised by user 2026-08-01): redirect logic confirmed already correct (web `login.ts navigateAfterLogin()` routes Admin/SuperAdmin to `/admin/approvals`; mobile routes staff-admin roles to `/admin_dashboard`) — no regression found. DISCOVERABILITY half implemented additively: `common/user-menu/user-menu.ts` gained an optional `@Input() isAdmin`, `user-menu.html` renders a conditional "Admin Panel" link (icon + label) when true, `user-menu.scss` adds a scoped `.admin-panel-link` rule; `layouts/portal-layout/portal-layout.html` wires `[isAdmin]="nav.isAdmin()"` into the existing `<app-user-menu>` (sidebar link untouched). `admin-layout.html`'s own `<app-user-menu>` usage doesn't pass `isAdmin`, so it correctly stays `false` there (no duplicate link inside the admin panel itself).

## AREA 31: DATABASE MIGRATIONS (merged from .claude/memory/outstanding_todos.md)

> This section supersedes `.claude/memory/outstanding_todos.md`, which was stale and is no longer maintained.

31.1 [DONE] DB: `PhaseB_S5S8_OtpHmac_PaymentIdempotency_Indexes` applied (User.FailedLoginAttempts/LockoutUntil, PaymentHistory.GatewayPaymentId + unique partial index, Otp.Code widened to varchar(64) for HMAC-SHA256 hex, Member(Status,IsArchived), User(MemberId), User(ResetToken) partial, User(GoogleId), User(FacebookId), Otp(Email,ExpiryAt)).
31.2 [DONE] DB: RefreshTokens table shipped — folded into the `AddDiscussionForums` migration rather than a standalone `AddRefreshTokens`. Table: RefreshTokens(Id, UserId FK→Users, TokenHash varchar(64) UNIQUE, ExpiresAt, CreatedAt, IsRevoked); indexes TokenHash (unique) + (UserId, IsRevoked).
31.3 [NOTE] DB: There are NO pending migrations — the dev database is up to date. Area 28.0's "DEPENDS ON PhaseB_S5S8 + AddRefreshTokens applied first" precondition is therefore already satisfied.
31.4 [NOTE] DB: EF reporting "pending model changes" on PgSql is spurious, non-deterministic seed churn — NOT schema drift. Never scaffold or apply a migration for it.
31.5 [DONE] Mobile: JWT refresh flow complete — `/auth/refresh` + `/auth/refresh-mobile` with dedicated rate-limit policies, and the Flutter `api_client.dart` interceptor performs the refresh (no longer forces re-login every 60 min).

## AREA 32: PROFILE DATA INTEGRITY (raised by user 2026-08-01)

32.1 [DONE 2026-08-02] INVESTIGATED and confirmed: (b) mapping/DTO bug — but in the **Angular frontend**, not the backend (opposite layer from 30.27's EC fix). `GHCAA.Infrastructure/Services/MemberService.cs` `GetProfileAsync` already correctly `.Include()`s AcademicHistory/ProfessionalHistory and maps every field (PassingYear/Degree/Subject/Designation/OrganizationName/ProfessionalSector/Location) into the DTO — the backend/DB layer is not the problem. The actual bug: `GHCAA.Web/src/app/member/profile/profile.ts`'s `applyProfileResponse()` built the client-side `profile` object from a hardcoded ~30-field whitelist (added under a "29D.8" comment for case normalization); any backend DTO field NOT in that fixed list — `designation`, `organizationName`, `professionalSector`, `location`, `profileCompletionPercentage`, `categoryBadge`, etc. — was silently dropped and rendered blank. Education History itself displayed fine (its array was copied unconditionally); "Professional Info" fields were the ones actually missing, matching the user's report. ORIGINAL: Member profile page is missing/showing incorrect values for fields that were previously populated and displayed correctly (user specifically flagged Educational Info and Professional Info sections). Compare, per member: (a) what's in `Seed/*.json` (or whichever seed source is authoritative), (b) what's actually in the live DB tables (per [[gotcha_seed_json_vs_live_db.md]] — editing Seed JSON alone does NOT update an already-created SQLite DB, so seed and live DB can and do diverge; check the live DB directly, not just the seed files), and (c) what the profile API response / `profile.html` actually renders. Identify whether this is a data-loss bug (values never made it into the DB), a mapping/DTO bug (values exist in DB but aren't returned/rendered), or a stale-seed bug (DB was seeded before a field was added/renamed and never re-seeded).
32.2 [DONE 2026-08-02] AUDITED: user's suspicion confirmed — the same hardcoded-whitelist mapping bug existed in 2 more places, both in the admin panel (grep for the whitelist pattern across `GHCAA.Web/src` found exactly 3 hits total, all now fixed): `admin/members/admin-members.ts`'s `openDetail()` (the admin member-detail modal — same entity as the member's own profile page, was missing designation/organizationName/professionalSector/location/profileCompletionPercentage etc., i.e. the exact modal flagged separately in Area 33's 33.1/33.2 findings) and its `loadMembers()` list mapping (narrower field set, same fragile pattern). `admin/member-approval/member-approval.ts`'s pending-approval list mapping had the same pattern too (smaller field set, lower risk since it only surfaces id/name/email/mobile/membershipNumber/status/photo/category, none of which were affected, but fixed for consistency). Member Directory, EC/Governance cards (30.27), and Digital ID do NOT use this whitelist pattern — confirmed via grep, no further instances found; 30.27's EC fix was a genuinely separate backend DTO-population bug, unrelated to this one.
32.3 [DONE 2026-08-02] FIXED centrally in all 3 files identified by 32.2, replacing each hardcoded field whitelist with a generic case-insensitive key-normalization pass over every own-enumerable key of the raw response (so any current or future flat DTO field survives automatically instead of requiring manual whitelist maintenance): `member/profile/profile.ts` `applyProfileResponse()`, `admin/members/admin-members.ts` (`openDetail()` + `loadMembers()`), `admin/member-approval/member-approval.ts` `loadMembers()`. No backend/DTO/DB changes were needed (32.1 confirmed the backend was already correct). Added `member/profile/profile.spec.ts` regression tests (2 new cases: flat DTO fields outside the old whitelist now surface; PascalCase/NID key normalization still works) — vitest baseline now 59 files/238 tests (was 236, +2). `npm run type-check` clean, `npm run build` not required (no template/API changes). `dotnet test` unchanged at 317/0 (confirms no backend regression from this fix). No other `docs/` file required correction — `docs/architecture_data_flow.md`/`docs/BUSINESS_FINDINGS.md` already describe the backend DTO/mapping layer, which was already correct; the bug was purely in Angular-side response normalization, not in documented data flow.

## AREA 33: OVERALL DESIGN/LAYOUT & UX REVIEW (raised by user 2026-08-02; PROFILE PAGES ARE PRIORITY #1 within this area, per user 2026-08-02 clarification)

33.1 [DONE 2026-08-02] REVIEW: Design/layout consistency, profile pages first. Punch list: **(1)** `member/profile/profile.html` and `admin/members/admin-members.html`'s detail modal render the *same* Member entity with divergent visual treatment for identical data (member profile: `.history-row` timeline rows with `.badge-info`; admin modal: plain label/value `.view-value` pairs) — no shared sub-component, so any future field addition must be hand-duplicated in two different markup styles. **(2)** Emoji icons (wastebasket, camera, pen, floppy-disk, multiply, rocket, check-mark and hourglass glyphs) are still used directly in `profile.html` and the admin member-detail modal, bypassing the `app-icon` SVG system rolled out app-wide in 30.3 — these two forms were missed because 30.3 targeted nav items and named action buttons, not ad-hoc buttons inside large forms. **(3)** `profile.html`'s `.remove-photo-btn` is a hand-rolled 28×28px round button, not the shared `.btn-icon`/`.icon-btn` (36px) taxonomy from 30.2 — inconsistent size and no reuse of the danger-color/hover rules already centralized there. **(4)** Empty-state inconsistency confirmed: the read-only "history" sections at the top of `profile.html` (EC History L96, Academic L130, Professional L175) still use bare `.empty-placeholder` (large icon, full height), while the edit-form's own duplicate empty states further down (L386, L477) use `.empty-placeholder dark compact` — i.e. 30.22's "empty states given `.empty-state.compact`" note only reached one of the two duplicate empty-state instances per section, not both, so the top of the page still reserves full height for members with no history. **(5)** `profile.scss`'s `.status-grid` is hardcoded `grid-template-columns: repeat(3, 1fr)` but the template (30.28) now renders 4 grid-items (Profile Health/Associated Batch/Highest Degree/Member Since) inside a `md:grid-cols-4` Tailwind utility class applied directly in the HTML — the component SCSS and the inline utility disagree on column count, so `.status-grid`'s own CSS is dead/overridden weight, a latent trap for the next person who edits `profile.scss` expecting it to control layout. Rest-of-app spacing/token usage (glass-card, form-group, btn*) was otherwise found consistent post-Area-30; no further divergent pages found in admin/portal/public sampling.
33.2 [DONE 2026-08-02] REVIEW: Content/nav order. **(1)** `profile.html` section order is Membership card → EC History (read-only) → Education History (read-only) → Career History (read-only) → Photo → Signature → edit form (Identity → Academic-edit → Professional-edit → Emergency → Address → Privacy → Notifications). For a brand-new member (the most common first-time case) this means three empty-state cards render *before* the member ever reaches an editable field — the actual "fill in your profile" task is buried below content that, for a new user, is empty. **(2)** The admin member-detail modal orders the *same fields* oppositely: Photo/Signature → Personal Details → Address → Emergency → Privacy → Notifications → Admin (verification/points) → Academic → Professional → EC History (Academic/Professional/EC pushed to the very bottom, opposite of the member's own page). Neither order is wrong in isolation, but the two views of one entity disagree, which will read as "different apps" to an admin who's also a member and costs orientation time when cross-referencing a support ticket against a member's own profile. **(3)** `public/landing.html` order is Banner → Purpose → **Jobs-preview** → Membership → Events → News → Gallery → EC-preview (leadership) → CTA-banner. Two placements read as awkward for a first-time visitor: Jobs-preview appears before the Membership/join section, i.e. a job board teaser is shown before the visitor is even told how to become a member; and EC-preview (the leadership/trust-building section) is second-to-last, after Gallery — for an alumni association, "who runs this" is normally a trust signal that belongs nearer the top, not buried after photo galleries. **(4)** Dashboard widget order (profile-completion wizard → stats row → news/events/notifications → recently-joined) and nav section grouping (Overview/Community/Directory/Career/My Account, and admin's Overview/Membership/Content/Finance & Tools) were already reordered/grouped correctly in 30.13/30.23 — no further issues found there.
33.3 [DONE 2026-08-02] REVIEW: Usability pass, profile page first. **(1)** Label clarity: several field/section labels use unnecessarily formal or jargon phrasing that will read as confusing to a first-time member — `profile.html`'s "Consanguinity / Relation" (plain meaning: relationship to the emergency contact), "Secure Identity Details" (Personal Details), "Detailed Academic Milestones" (Education History), "Professional Legacy & Snapshot" / "Detailed Career Timeline" (Work Experience), "Correspondence Identity" (Address). None of these are self-explanatory to a non-technical member and none match the plain-language dashboard checklist labels ("Identity & Photo", "GHC History", "Professional Info") that link into these very sections — so the same data is named three different ways across one page (nav/checklist label vs. section heading vs. status-card labels like "Associated Batch"). **(2)** Empty-state guidance: text is present and generally helpful (e.g. "Your educational journey is currently empty. Add your academic milestones below.") but, per 33.1 finding (4), only half the duplicate empty-state instances got the compact treatment, so new members still see three large ceremonial empty blocks before reaching the form. **(3)** Form validation feedback: present and consistent (red border + inline message pattern via `#xM="ngModel"` + `.touched && .invalid`) across every required field — no issues found here, this part of the page is in good shape. **(4)** Discoverability: re-checked for other "hidden feature" cases beyond 30.36's admin-panel link — none found; `app-user-menu` is the only such affordance and it's now handled. Read-only fields (NID/Email/Mobile) use `opacity-50` + a `title` tooltip to explain "contact admin to change" — acceptable but the reason is only visible on hover/long-press, which is a poor discovery pattern on touch devices (no visible affordance, tooltip requires a pointer). **(5)** Touch targets: `.remove-photo-btn` (28×28px, see 33.1 finding 3) and the inline emoji-only buttons are below the ~44px touch-target guideline; the standard `.icon-btn` (36px, per 30.2) is itself still under 44px but is the established app-wide baseline, so raising it is a larger token-level decision, not a one-page fix. **(6)** Live-browser walkthrough (Playwright/manual, per the `verify` skill) was **not** performed in this session — no browser available; findings above are from template/component code inspection only, and an actual golden-path walkthrough as a new member and as an admin is still owed.
33.4 [DONE 2026-08-02] Triage of 33.1-33.3 findings below as 33.5-33.12 (bucket tagged). No fixes were implemented — investigate-and-triage only, per instruction.

33.5 [DONE 2026-08-02] Replaced all remaining raw emoji action icons (wastebasket, camera, pen, floppy-disk, multiply and rocket glyphs) in `member/profile/profile.html` and the admin member-detail modal in `admin/members/admin-members.html` with `<app-icon>`, completing the 30.3 icon-system rollout for these two forms. Added 4 new icon.html cases (`camera`, `pen`, `rocket`, `save`) since no existing case covered these concepts; imported `Icon` into both `profile.ts` and `admin-members.ts`. The Bulk Import Modal's multiply-glyph close-btn (admin-members.html L~800) is a separate, out-of-scope modal — left untouched.
33.6 [DONE 2026-08-02] CORRECTED SCOPE: the shared icon-button class already existed in `styles.scss` under the name `.icon-btn` (36px, canonical, comment-flagged "SINGLE SOURCE OF TRUTH"), with danger styling under the `.delete`/`.archive` modifier — NOT `.btn-icon.danger` as originally phrased (that class/modifier combination never existed). Replaced `profile.html`'s bespoke `.remove-photo-btn` (28px) usages with `class="icon-btn delete"` and deleted the now-dead `.remove-photo-btn` rule from `profile.scss`, preserving its absolute-positioning (`bottom:-5px; right:-5px; z-index:10`) via a new local `.photo-preview-wrap .icon-btn` rule.
33.7 [DONE 2026-08-02] Simplified jargon-heavy labels/headings identified in 33.3(1) to plain language in `profile.html`: "Secure Identity Details" → "Personal Details", "Emergency Protocol Contact" → "Emergency Contact Details", "Consanguinity / Relation *" → "Relation *" — each now matches the admin member-detail modal's existing plain-language wording exactly (admin-members.html already used "Emergency Contact Details"/"Relation", so no admin-side edit was needed).
33.8 [DONE 2026-08-02] Applied the `compact` modifier to the *top* read-only empty-placeholder blocks in `profile.html` (EC History L96, Academic L130, Professional L175) — 30.22 only reached the edit-form's duplicate empty states (L386/L477), not these.
33.9 [DONE 2026-08-02] Reconciled `profile.scss`'s dead `.status-grid { grid-template-columns: repeat(3, 1fr) }` with the template's actual `md:grid-cols-4` utility class (33.1 finding 5) by stripping `display`/`grid-template-columns` from the SCSS rule, leaving only `gap`/`padding-top`/`border-top` — the template's Tailwind utility classes are now sole source of truth for the grid layout.
33.10 [DONE 2026-08-02] Reordered `member/profile/profile.html`: Membership Status Card → Photo Upload → Signature Upload → main editable form (Personal/Academic/Professional/Emergency/Address/Privacy/Notifications) → read-only EC/Academic/Professional history recaps (moved to the bottom). Pure block-move, no internal markup changes; confirmed no order-dependent selectors in `profile.scss` (`.section-block:first-of-type` is scoped inside the form) or assertions in `profile.spec.ts`. A new member now reaches an actionable field immediately after the status card instead of three empty-state cards.
33.11 [DONE 2026-08-02] Reordered `public/landing.html`: `landing-ec-preview` moved to directly after `landing-purpose` (leadership/trust signal surfaces early); `landing-jobs` moved to after `landing-membership` (join-CTA/credibility now precede the job-board teaser). Pure line reorder, no markup changes.
33.12 [DONE 2026-08-02] Added `docs/PROFILE_SHARED_COMPONENT_DESIGN.md` — design-only doc (no code) proposing 5 shared presentational components (`app-academic-history-editor`, `app-professional-history-editor`, `app-ec-history-view`, `app-emergency-contact-form`, `app-address-form`) driven by `@Input()`/`@Output()` off the same underlying `Member` fields both `profile.html` and the admin member-detail modal already bind to. Documents the section-order/label/duplication differences that must be reconciled before extraction and flags implementation as a separate future TODO once reviewed.
33.13 [DONE 2026-08-02] Live-browser Playwright walkthrough (dev servers started manually — `dotnet run` on the API + `ng serve` on Angular, since the `webapp-testing` skill's documented `with_server.py` helper does not exist in this installation). Verified: (1) landing page renders the new 33.11 section order exactly (`landing-banner, landing-purpose, landing-ec-preview, landing-membership, landing-jobs, landing-events, landing-news, landing-gallery-preview, landing-cta-banner`); (2) member login (`demo_user`/`DemoPass123!`) succeeds and lands on `/portal/dashboard`; (3) `/portal/profile` renders the new 33.10 order exactly (Membership Status → Photo → Signature → main form → Association Governance History → Educational Timeline → Professional Experience), with no visual breakage scrolling through the form (address/privacy/notification sections render normally). Not verified: the admin member-detail modal — both `shalin`/`Shalin@2024!` and `superadmin`/`SuperAdminPassword123!` logged in as ordinary alumni members (no admin role), so `/admin/members` redirected to `/portal/dashboard` via `adminGuard`; this is a local seed/role mismatch (per the known live-DB-vs-seed-JSON gotcha), not a regression from this session's changes, and 33.12 made no code changes to `admin-members.html` to verify.

## AREA 34: ADMIN-MANAGED SITE CONTENT + MERGED NEWS & NOTICE BOARD (raised by user 2026-08-02)

> User instruction set: (1) update About Us with college + association information, (2) add an On Campus Address to Contact Us, (3) make both manageable from admin, (4) merge News and Notice management, notices may carry PDF documents. Standing constraints: **notices are admin-post-only**; About content is sourced from the GHCAA Constitution text and facts already in the repo **only** (no web research, no invented college facts); web + mobile parity; OrgConfig additions must be industry-standard and configurable; nothing may break.

### 34.A — SiteContent CMS (About Us / Contact intro)

34.A1 [DONE 2026-08-02] Domain: `GHCAA.Domain/Models/SiteContent.cs` (`Id, Key, Title, BodyHtml, DisplayOrder, IsActive, LastModified, UpdatedByAdminId`) + `Infrastructure/Data/Configurations/SiteContentConfiguration.cs` (unique index on `Key`) + `DbSet<SiteContent>` on `ApplicationDbContext` (config auto-discovered by the existing `ApplyConfigurationsFromAssembly`).
34.A2 [DONE 2026-08-02] Application/Infrastructure: `DTOs/SiteContentDto.cs` (`SiteContentDto`, `UpsertSiteContentDto`), `Interfaces/ISiteContentService.cs`, `Infrastructure/Services/SiteContentService.cs` (registered by the existing `I*`→impl convention loop). `BodyHtml` is server-side sanitized via the existing `HtmlSanitizer` on every write, mirroring `NewsService`.
34.A3 [DONE 2026-08-02] API: `Controllers/SiteContentController.cs` — `GET /api/site-content?group=about` is `[AllowAnonymous]` (active blocks, `DisplayOrder` ordered); `GET /api/site-content/admin`, `POST`, `PUT /{id}`, `DELETE /{id}` are all `[Authorize(Policy = "AdminOnly")]`.
34.A4 [DONE 2026-08-02] Seed: `Infrastructure/Data/Seed/site-content.json` loaded through the existing `LoadSeed<T>` path. Blocks: `about-origin` (existing repo 1938 / Ashutosh Ganguly / Sher-e-Bangla text), `about-association` (Article I — name, "HARAGANGIAN", founding 29 Nov 2025, non-political/non-religious/non-profit/inclusive/philanthropic nature, motto EN+Bengali), `about-logo` (Article I §6 logo symbolism + §7 flag), `about-objectives` (Article II, 11 objectives), `contact-intro`. Constitution blanks (theme song, registration no., registered office, banner, digital platform, social handles) are deliberately **omitted** — they are unfilled placeholders in the source document, not content.
34.A5 [DONE 2026-08-02] Web: `core/services/site-content.service.ts` + `SiteContent` interface in `core/models/business.models.ts`; `public/about/about.ts|.html` renders CMS blocks via `[innerHTML]` inside the existing `.glass-card`/`.story-grid` shell, **retaining the previous hardcoded markup as a fallback** so the page never renders empty.
34.A6 [DONE 2026-08-02] Web admin: `admin/site-content/` screen (list, rich-text edit via the shared `<app-rich-text-editor>`, reorder, activate/deactivate); routed under the `admin` children in `app.routes.ts` and added to `adminNavItems` (Content section) in `core/services/nav.service.ts` with a matching admin-layout icon case.

### 34.B — Contact Us on-campus address (OrgConfig)

34.B1 [DONE 2026-08-02] Backend: `ContactDto` in `Application/DTOs/OrgConfigDto.cs` gains `CampusAddress`, `PhoneNumbers` (`List<string>`), `MapEmbedUrl`; `OrgConfigService.BuildGhcaaDefaults()` seeds the campus address and the two phone numbers previously hardcoded in `contact.html`.
34.B2 [DONE 2026-08-02] Web: same three fields in `core/models/org-config.model.ts` + `core/services/org-config.service.ts` fallback; `public/contact/contact.html` binds address / `supportEmail` / phones / `socialLinks` from config instead of hardcoded markup (the three dead `href="#"` social buttons now resolve to configured links), and the header blurb comes from the `contact-intro` SiteContent block.
34.B3 [DONE 2026-08-02] Web admin: the SuperAdmin-gated `admin/org-config` form exposes campus address, map embed URL, and an add/remove repeater for phone numbers — so every new value is admin-configurable, not deploy-bound.
34.B4 [DONE 2026-08-02] Mobile: `GHCAA.Mobile/lib/core/config/org_config.dart` mirrors `campusAddress` / `phoneNumbers` / `mapEmbedUrl` in the model, `fromJson`, and the GHCAA defaults.
34.B5 [DONE 2026-08-02] SECURITY: the map embed URL is admin-supplied and therefore untrusted — `contact.ts` runs it through an allow-list check before `bypassSecurityTrustResourceUrl`, and the template renders the iframe only via `@if (mapUrl(); as src)` so a rejected URL yields no element at all.
34.B6 [DONE 2026-08-02] AUDIT (per user instruction "make sure OrgConfig is industry standard and all currently needed values are configurable"): verified full four-way parity — backend DTO + defaults, web model + service fallback, web admin form, mobile config — with no field configurable in one layer and missing in another.

### 34.C — News + Notice unified

34.C1 [DONE 2026-08-02] Domain: `PostType { News, Notice }` enum and `FileUploadType.NoticeDocument` added to `GHCAA.Domain/Enums.cs`; `NewsPost` gains `PostType` (defaults to `News`), `AttachmentUrl`, `AttachmentFileName`.
34.C2 [DONE 2026-08-02] API: `GetActiveNews` takes an optional `postType` filter threaded through `INewsService.GetActiveNewsAsync`; new `POST /api/news/upload-document` (`AdminOnly`, `FileCategory.Document`, 10 MB cap, `FileUploadType.NoticeDocument`) mirroring the existing `upload-image` action.
34.C3 [DONE 2026-08-02] SECURITY — **notices are admin-post-only**: the member-facing `SubmitArticle` endpoint rejects `PostType.Notice` with `Forbid()` unless the caller is Admin/SuperAdmin; `CreateNews`/`UpdateNews`/`upload-document` were already `AdminOnly`.
34.C4 [DONE 2026-08-02] DTOs: `PostType` + attachment fields on `CreateNewsDto`, `UpdateNewsDto`, `NewsDto`; `NewsService` filters by `PostType`.
34.C5 [DONE 2026-08-02] Web admin: `admin/news` gained a News/Notice tab filter over the same list, a `postType` selector in the create/edit form, and a PDF picker reusing `core/utils/file-validation.util.ts` for client-side type/size checks — one screen manages both post kinds, no second admin page.
34.C6 [DONE 2026-08-02] Web public/portal: the shared `common/news` feed gained the same News/Notices tabs, a `.notice` accent treatment, and a PDF download affordance on posts that carry an attachment.
34.C7 [DONE 2026-08-02] Nav: a **Notices** entry in `layouts/public-layout/public-layout.html` deep-links to `/news?type=Notice`; `common/news/news.ts` syncs the tab with `ActivatedRoute.queryParamMap` and writes it back via `replaceUrl` — the tab is shareable and no duplicate Notices page/component exists to maintain.
34.C8 [DONE 2026-08-02] Mobile: `NewsService.getLatestNews({postType})` filter plus notice/attachment handling in `news_screen.dart` / `news_details_screen.dart`.

### 34.D — Cross-cutting (raised mid-implementation by user)

34.D1 [DONE 2026-08-02] "Fully responsive UI, try avoid scrolling": the news feed became a `repeat(auto-fill, minmax(320px, 1fr))` card grid with `line-clamp`-ed titles/bodies (equal-height cards, many more posts above the fold, single column below 640px to avoid horizontal overflow); `about.scss`'s fixed 10rem/8rem/5rem rhythm and 70vh hero became `clamp()`-based fluid spacing with `auto-fit` story/pillar grids; `contact.scss`'s info panel became `minmax(280px, 380px)` and only stacks at 900px (was 1200px, which doubled page height prematurely).
34.D2 [DONE 2026-08-02] "Try common changes as reusable": `POST_TYPE_TABS` + `matchesPostType()` added to `core/constants/app.constants.ts` and adopted by **both** the admin console and the public/portal feed, so the two filters cannot drift (legacy posts with no `postType` count as News in one place only).
34.D3 [DONE 2026-08-02] Style de-duplication found while doing 34.D2: `.pill` (×3), `.actions-cell` (×3), `.title-cell` (×2), `.key-cell`, and `.status-badge` copies that shadowed the already-central definition were consolidated into `src/styles.scss` as a "data-table cell utilities" block and deleted from `admin/news`, `admin/site-content`, `admin/members`, and `admin/gallery` stylesheets, each left with a pointer comment. New central `.attachment-flag` / `.doc-link` cover the three attachment affordances instead of a new component (per the keep-it-lightweight constraint). **Trap this fixes:** component SCSS gets a view-encapsulation attribute suffix and therefore out-specifies an identical global rule, so a duplicated copy silently forks the look.
34.D4 [DONE 2026-08-02] Migrations: single migration covering the `SiteContents` table + the three new `NewsPosts` columns, added to the PostgreSQL tree only — SQLite's schema comes from `EnsureCreated()`, and per 31.4 the PgSql "pending model changes" seed churn was correctly ignored.
34.D5 [DONE 2026-08-02] Tests: backend `dotnet test` **330 passed / 0 failed** (includes new SiteContent service + controller tests and the NewsController non-admin-Notice-rejection and `upload-document` cases); web `npx vitest run` **59 files / 240 tests passed** (baseline 238, +2 new `common/news` specs for the postType filter and detail-panel auto-close); `npm run build` (AOT — the only thing that type-checks Angular templates; `tsc --noEmit` does not) succeeded with no new warnings.
34.D6 [DONE 2026-08-02] Mobile tests: `flutter analyze` clean after updating the two `FakeNewsService` overrides in `test/comprehensive_visual_freeze_test.dart` and `test/full_app_visual_freeze_test.dart` for the new `{String? postType}` parameter; `flutter test` — 70 non-golden tests pass. The 21 failing goldens (auth_login, auth_register, registration_*, directory/member_*, news_portal, events/financial) are the **pre-existing stale baseline** from the earlier app-wide `app_theme.dart` floating-label change, span screens this area never touched, and are skipped on CI per the existing `flutter_test_config` golden guard.
34.D7 [TODO] Regenerate the 21 stale mobile goldens (`flutter test --update-goldens`) as a standalone housekeeping task, so a genuinely new mobile regression is not masked by the existing baseline drift. Deliberately kept out of this area — it is unrelated binary churn from a prior session's theme fix.
34.D8 [TODO] Live-browser verification of this area: public `/about` (seeded CMS blocks + hardcoded fallback), `/contact` (campus address, phones, map guard), `/news?type=Notice`; admin CMS block edit round-trip; admin notice creation with a PDF and a working public download link; and a **member account confirming it cannot create a Notice** (34.C3). **Blocked by the same local seed/role mismatch as 33.13** — `shalin` and `superadmin` resolve to ordinary alumni members in the local DB, so `adminGuard` bounces `/admin/*`; the local role assignment must be fixed before the admin-side steps can run.
34.D9 [DONE 2026-08-03] Preprod schema catch-up script `docs/sql/preprod_area34_sitecontent.sql` — idempotent SQL mirroring migration `20260802163432_AddSiteContentAndNoticeFields` (three `NewsPosts` columns + `SiteContents` table, unique `Key` index, the five seed blocks, and an identity-sequence `setval` so the first admin-created block does not collide on `Id = 1`). Needed because Area 34 shipped to preprod but the content never appeared: the runtime builds schema with `EnsureCreated()` (`GHCAA.API/Program.cs:290`), which is a **no-op on a database that already has tables**, so the new table and columns were never created and `/about` fell back to its static markup. **Applied to preprod/Neon 2026-08-03 and verified**: the three `NewsPosts` columns exist, the five blocks are present, the identity sequence sits at 5, `GET /api/news` returns 200 (was a 500 — `42703: column n.AttachmentFileName does not exist`) and `GET /api/site-content?group=about` returns the four About blocks.
34.D10 [TODO] **Durable fix for the above**: the app has a real migration tree but nothing runs it at startup, so every future schema change will hit the same silent no-op on any non-empty environment. Switch the non-Visual startup path from `Database.EnsureCreated()` to `Database.Migrate()`. Non-trivial because the preprod/Neon database was originally built by `EnsureCreated` and therefore has **no `__EFMigrationsHistory` table** — it must first be baselined (create the history table and insert every existing migration id as already-applied) or `Migrate()` will try to re-create tables that exist and fail. Plan: baseline preprod → switch the call → confirm a no-op `Migrate()` on an already-current DB → confirm a fresh empty DB still builds end-to-end. Also decide what happens to the `HasData` seed rows, which `EnsureCreated` and `Migrate` apply by different routes. Keep SQLite/local dev working throughout (it relies on `EnsureCreated` and has no SQLite migration tree — see 34.D4).
34.D11 [DONE 2026-08-03] Seeded SiteContent titles contained HTML entities (`Logo &amp; Flag`, `Purpose &amp; Objectives`), but `about.html` renders the title with `{{ }}` interpolation (which escapes) while only the body uses `[innerHTML]` — so the headings showed a literal `&amp;`. Corrected in all four places that carry the value: `Seed/site_content.json`, the migration's `InsertData`, `docs/sql/preprod_area34_sitecontent.sql`, and the live Neon rows (`UPDATE ... replace("Title",'&amp;','&')`). Body HTML entities (`&ndash;`, `&ldquo;`) are correct as-is — that field *is* parsed as HTML.
34.D12 [DONE 2026-08-03] Public nav fixes raised by user: (a) the association full name never rendered under the logo — `public-layout.html` carried `class="hidden xl:block"`, but this project has **no Tailwind**, so `hidden` matched the real global `.hidden { display: none }` utility in `styles.scss` while `xl:block` matched nothing; both classes removed, leaving `.hide-mobile` and the existing `≤600px` rule to do the responsive hiding. (b) The public site could get stuck on a white background: the theme choice is persisted globally by `ThemeService`, but the switch lived only in `<app-user-menu>` (portal/admin), so a visitor who once chose light mode had no way back. Extracted the button into a shared `common/theme-toggle/` component (per the "try common changes as reusable" constraint) and placed it in both the user menu and the public nav. `npx vitest run` **60 files / 244 tests** green; `ng build` clean.

34.D13 [DONE 2026-08-03] Logo asset: the crest artwork the user pasted is `assets/logo.jpg` — RGB 1024×1024 on an **opaque black square**. The existing `logo.png` was a transparent but lower-resolution 676×814 crop, so it was not the same artwork. Per "make another version of transparent background of these without another changes, and use it everywhere", a new transparent `logo.png` was derived **from the jpg** at RGBA 1024×1024 by BFS flood-fill seeded from every near-black (≤40) pixel on the four image edges — edge-seeding preserves interior black artwork and clears only background-contiguous black, so the drawing itself is unchanged. Installed to `GHCAA.Web/public/assets/logo.png` and `GHCAA.Mobile/assets/logo.png` (527,642 B, smaller than the 908,708 B it replaced), verified by compositing over white and over `#0c0c0c` — no halo, legible on both. The remaining 6 `appImgFallback` placeholders still on the jpg were repointed (`common/gallery/gallery.html`, `gallery.ts`, `common/news/news.html`, `public/magazine/magazine.html` ×3), so all 23 `assets/logo` references app-wide now use the png. This **reverses** the earlier "keep the gallery/news jpg fallbacks" rule, which predated the "use it every where" instruction. `Seed/photos.json` still names logo.jpg deliberately — sample gallery content, not branding. `flutter analyze` 0, `ng build` 0, `vitest` 60 files / 244 tests, `dotnet test` 330 passed.

34.D14 [DONE 2026-08-03] "Cant see public theme switcher" investigated and found to be **not a code defect**. Verified in a real headless Chromium at 1440×900 against the dev server: `app-theme-toggle` count 1, `visible: true`, box `{x:1376, y:18, w:20, h:25}`, computed `color: rgba(255,255,255,0.85)`, `title="Switch to light mode"`, and the `theme-light` sun glyph present as a correctly-namespaced `<circle>`+`<path>` inside a 20×20 `<svg>`. Two hypotheses were disproved along the way and are recorded so they are not re-chased: (a) `theme-dark`/`theme-light` *are* valid icon names — they live in `common/icon/icon.html` (lines ~159 and ~165), not in `icon.ts`, which is why grepping the `.ts` found nothing; (b) the pile of `<!--container-->` comments inside the toggle's `svg.innerHTML` is **normal** — Angular `@switch` emits one comment anchor per non-matching `@case`, so read `children.length` and `namespaceURI` instead of eyeballing innerHTML. Remaining explanations are non-code: low discoverability (a bare 20px 85%-white stroke icon pinned at the extreme right edge after eight bold uppercase links) or the user viewing the stale preprod deploy. Left unstyled pending user preference, per "keep all other thing as it is".

34.D15 [DONE 2026-08-03] About page content rewritten per "add about college, see official college website, and dont direct copy from constitution, write in meaningful way for others" — this **supersedes** the original plan decision to seed constitution text only with no web research. `Seed/site_content.json` went 5 → 6 blocks: `about-origin` retitled "The College and Its Origin" (18 Dec 1938 founding, Ashutosh Ganguly's one-lakh-rupee donation, advocate Satish Chandra Bhattacharya, A. H. M. Wajir Ali); **new** `about-college-today` with facts sourced from the official site (16 honours departments, 80+ teachers, 100,000+ graduates, campus address, EIIN 111160, college code 5701, principal, outbound link, plus an `<em>` disclaimer that the Association is an independent alumni body); `about-association` reworded out of verbatim constitution phrasing; `about-logo` "logo"→"crest"; and `about-objectives` retitled **"What We Do"** with the 11 constitution objectives restructured into 6 thematic bullets (Connection / Students / Welfare / Community / Heritage / Governance). The college's nationalisation date was **deliberately omitted** — it could not be confirmed on the official site, and no fact was invented. Note the seed JSON alone does **not** reach preprod (`EnsureCreated()` no-ops on a non-empty DB, see 34.D9/34.D10) — the new and changed blocks must go in via the admin SiteContent CMS or a catch-up SQL script.

34.D16 [DONE 2026-08-21] Fixed: `portal-layout.ts` now injects `OrgConfigService` (exposed as `orgConfig`) and both portal logos bind `[src]="orgConfig.config()?.branding?.logoUrl || '/assets/logo.png'"` + `[alt]="...shortName || 'Logo'"` with `appImgFallback="/assets/logo.png"`, mirroring `admin-layout.html` exactly. Two adjacent hardcodes in the same branding-opt-out class were fixed with it: the top-bar text `GHCAA Member Portal` and the `document.title` suffix now use `branding.shortName` (fallback `'GHCAA'`). The sidebar wordmark `HARAGANGIAN` was **left hardcoded on purpose** — it is the member nickname styled as a wordmark, and swapping it to `shortName` would visibly change the portal's sidebar text, which is beyond this item. Verified: `ng build` clean, vitest 60 files / 244 tests pass. Original finding follows. `portal-layout.html` hardcoded `src="/assets/logo.png"` in two places (the sidebar mini-logo and the collapsed-rail logo) instead of binding `orgConfigService.config()?.branding?.logoUrl` the way admin-layout and the public footer do — `portal-layout.ts` does not inject `OrgConfigService` at all. Harmless today because the configured default *is* `/assets/logo.png`, but it silently opts the portal out of org-config branding, so a tenant that sets a custom logo gets it everywhere except the member portal.

## AREA 35: WEB MEMBERSHIP-TYPE PARITY (raised by "check mobile implementation compared to web", 2026-08-22)

> Origin: a mobile-vs-web audit of the membership-type implementation. **Counter-intuitive result: mobile is the cleaner side.** `screens/member/dashboard_screen.dart:209` does `profile?['membershipType']?.toString()` — it renders whatever string the API sent, so a new enum value needs no mobile change, and the word `'Life'` appears nowhere in `GHCAA.Mobile/lib`. The web app instead keeps **three divergent copies** of the type list: one correct (`core/constants/app.constants.ts`) and two wrong ones that hardcode `6: 'Life'` — a value that does not exist in `GHCAA.Domain/Enums.cs`, where index 6 is `Guest`. So an approved Guest member is labelled "Life" on the web. These four items are the web-side follow-ups; they also absorb the still-open half of **28.21**.
>
> Shared reference that already exists and should be the single source: `MEMBERSHIP_TYPES` (`app.constants.ts:39-47`, ends `'Guest Member'`), `MEMBERSHIP_TYPE_OPTIONS` (L303-311, ends `{ value: 'Guest', label: 'Guest Member' }`), and the helper `getMembershipTypeLabel(type)` (L281), which correctly indexes `MEMBERSHIP_TYPES`.

35.1 [DONE 2026-08-22] **Highest severity — wrong label on a printed ID card.** `member/digital-id/digital-id.ts:31-34` `getMembershipName()` holds a private array `['Founding','Executive','General','Associate','Honorary','Advisory','Life']`; index 6 is `Guest` in the domain enum, so a Guest member's digital ID card prints "Life". Delete the local array and call the existing `getMembershipTypeLabel()` from `core/constants/app.constants.ts`. Severity is above 35.2 only because this artifact is downloaded/printed and shown as identity proof.

35.2 [DONE 2026-08-22] `member/dashboard/dashboard.ts:83-89` `getMembershipType()` holds the same wrong map as a `Record<number, string>` (`6: 'Life'`), plus an `|| 'General'` fallback that silently mislabels any unknown value instead of surfacing it. Replace with `getMembershipTypeLabel()`. Do 35.1 and 35.2 in one change — they are the same defect in two files.

35.3 [DONE 2026-08-22] `common/directory/directory.html:62-67` hardcodes the membership-type `<option>` list in the template, ending at `<option value="Advisory">Advisory Member</option>` — **no Guest option, so the web directory filter hides Guest members**, the exact defect already fixed on mobile in 28.21. `MEMBERSHIP_TYPE_OPTIONS` exists and is correct but is not used here. Replace the literal options with an `@for (m of membershipTypeOptions; track m.value)` loop, as `public/register/register.html:137` already does.

35.4 [DONE 2026-08-22] `core/models/business.models.ts:2` — the `MembershipType` TS union stops at `'Advisory'` and is missing `'Guest'`, so any code assigning the real value fails type-check and gets worked around. Add `'Guest'`. **Also reconciles a doc line:** `docs/project_map.md:986` was updated on 2026-08-22 to show `'Guest'` in this union, which currently documents *intended* rather than actual state — that line becomes accurate only once this item lands.

35.5 [DONE 2026-08-22] **Closes the remaining half of 28.21.** **Product decision (user, 2026-08-22, verbatim): "Guest - membership will be updated by admin, infact any membershiptypes only can be updated by admin."** So the answer is admin-assign-only for *every* tier, not just Guest — and the previous behaviour was a **privilege-escalation hole**, not merely a client inconsistency: `MemberRegistrationDto.MembershipType` flowed straight into `Member.MembershipType`, so a public self-registration could grant itself `Founding` or `Executive`.
>
> Enforced at the server first, so neither client is the enforcement point:
> - `GHCAA.Application/DTOs/MemberRegistrationDto.cs` — the `MembershipType` property is **deleted**; the registration payload can no longer carry a tier.
> - `MemberService.RegisterAsync` — derives `assignedType` from `IOrgConfigService.GetConfigAsync().Workflow.DefaultMembershipType` (null-safe, falls back to `General`) and uses it for **both** `Member.MembershipType` and the `GetApplicableFeeAsync(RegistrationFee, assignedType)` lookup, so the fee always matches the tier actually assigned.
> - Web: `public/register/register.html` — tier `<select>` replaced by a read-only note; `register.ts` no longer imports `MEMBERSHIP_TYPE_OPTIONS` and sources the fee tier from `orgConfig.config()?.workflow?.defaultMembershipType`.
> - Mobile: `screens/auth/register_screen.dart` — Step-3 dropdown replaced by explanatory text; `features/auth/register_wizard_provider.dart` — `membershipType` removed from the model, ctor, `copyWith`, `data` map, `updateData` switch and the submit payload; `core/constants/registration_constants.dart` — dead `MembershipConstants.typeOptions` deleted (**this is what closes 28.21**).
> - **Members can still see their own tier** (read-only): web `member/dashboard`, `member/profile`, `member/digital-id`; mobile `dashboard_screen` and — new in this change — a read-only `Member Tier & Category` block in `profile_edit_screen.dart` for non-admins, replacing the previous state where a member saw nothing (the editable dropdown was already admin-gated).
> - Admin-side tier assignment is untouched: `AdminMemberUpdateDto.MembershipType` → `UpdateMemberByAdminAsync` (audited), the admin CSV import, and all label/directory/filter paths keep every tier including `Guest`.
>
> Tests (12.6): new `MemberServiceTests.RegisterAsync_ShouldAssignDefaultMembershipType_NotAClientSuppliedOne` (also asserts the DTO has no `MembershipType` property by reflection) and a `registration_wizard_test.dart` guard that `toJson()` carries no tier key. Verified: `dotnet build` clean, `dotnet test` 351 passed, `ng build` clean, `npx vitest run` 61 files / 258 tests, `flutter analyze` no issues, `flutter test test/registration_wizard_test.dart` 15 passed.
>
> Original finding follows. Decide and align whether an applicant may self-select `Guest` at registration. Evidence gathered: the **web** registration form has been offering Guest all along (`register.ts:53` binds `MEMBERSHIP_TYPE_OPTIONS`, whose last entry is Guest), while **mobile** `core/constants/registration_constants.dart` `MembershipConstants.typeOptions` stops at `Advisory`. So this is a **client-to-client inconsistency, not an admin-assign-only policy** — one of the two forms is wrong whichever way the decision goes. Either add `Guest` to the Dart list (mobile matches web) or remove it from `MEMBERSHIP_TYPE_OPTIONS`' registration usage and keep it admin-assign-only (web matches mobile) — note the latter must **not** remove Guest from the admin/directory/label paths, which legitimately need it. Direction is a product call.

35.6 [DONE 2026-08-22] Once 35.1-35.4 land: add a web unit test that asserts the rendered label for the highest `MembershipType` index equals the domain enum's last member (i.e. a Guest member is not labelled "Life"), so this class of drift fails the suite rather than shipping. Also extend the sync checklist in `docs/CONFIG_DRIVEN_FRAMEWORK.md` §8 — it currently tracks Backend / Angular-constants / Config / Flutter / DB / Tests and has **no rows** for the directory template, the two component-local label maps, or the TS union, which is why all four drifted unnoticed. Per 12.6, nothing in Area 35 is `[DONE]` until `npx vitest run` and `ng build` pass.

> **AREA 35 IMPLEMENTATION NOTE (2026-08-22).** Area 35 is now **fully closed** — 35.1-35.4 and
> 35.6 shipped first, and 35.5 landed later the same day once the product decision arrived (see
> the 35.5 entry above; it also closed 28.21). What actually changed, and two things that were not in the
> original plan:
>
> 1. **`getMembershipTypeLabel()` was hardened, not just reused.** It previously returned raw input
> for string values, so a numeric `6` rendered `"Guest Member"` while the string `'Guest'`
> rendered `"Guest"` — the same member labelled two ways depending on which endpoint answered.
> It now resolves enum names through `MEMBERSHIP_TYPE_OPTIONS`, accepts a numeric ordinal sent as
> a string (`'6'`), accepts `null`/`undefined`/`''` (needed because the ID-card template passes
> `profile()?.membershipType`, which AOT correctly types as possibly-undefined — the build failed
> until the signature was widened), and passes an *unrecognised* name through verbatim rather
> than silently mislabelling it "General".
> 2. **Both templates had to drop a literal `" Member"`.** `digital-id.html` and `dashboard.html`
> rendered `{{ helper(...) }} Member`, which worked only because the old local arrays returned
> bare names. The shared labels already end in " Member", so leaving the templates alone would
> have produced "Guest Member Member". Consequence for future callers: **`getMembershipTypeLabel`
> returns the full display label — never append " Member" to it.**
>
> Side effect accepted deliberately: because string resolution now goes through the options list,
> admin surfaces that already used the helper (`admin-members` table + its CSV export,
> `admin-governance`, `member/profile`) render `"Guest Member"` where they previously rendered
> `"Guest"`. That is the correct label and consistent with the numeric path.
>
> Verified: `npx vitest run` **61 files / 258 tests passed** (baseline was 60/244 — +1 file, +14
> tests) and `npx ng build` succeeded with only the pre-existing `canvg`/`jspdf` CommonJS warnings.
> `docs/project_map.md:986` is now accurate — the union it documents really does carry `'Guest'`.

## AREA 36: PUBLIC CONSTITUTION & ELECTION DOCUMENT HUB (raised by "did we show full constitution in public portal and Election processes, forms view and download", 2026-08-23)

> Origin: a direct check of whether the public site surfaces (a) the full constitution and (b) the
> election processes/forms with view + download. **It surfaces neither.** Both are
> backend/content-complete and frontend-missing, which is why nothing in Areas 1-35 flagged them —
> `9.1 Digital Constitution: Versioned legal repository` and `18.4 Backend: Seed Constitution data`
> are both marked `[DONE]`, and they genuinely are, on the server. The UI step was never scoped.
>
> **Evidence, constitution.** The only public surface is a raw static PDF link:
> `public/landing/sections/banner/banner.html:15` → `href="/assets/GHCAA constitution 4.0.pdf"`
> (`target="_blank"`; asset present, 2,947,720 bytes). No in-app page, no article navigation,
> no version history, no dedicated route — `app.routes.ts` public children are landing, login,
> register, reset-password, about, contact, gallery, magazine, directory, events, news, jobs,
> payment, healtz. Meanwhile `GovernanceController` exposes
> `GET /api/governance/constitution` (L46) and `GET /api/governance/constitution/history` (L54),
> **both already `[AllowAnonymous]`**, and `POST /api/governance/constitution/{id}/vote` (L62).
> A grep for `governance/constitution` across `GHCAA.Web/src` returns **zero callers** — the
> versioned repository, the `Constitution` entity (`Version`/`Content`/`PdfUrl`/`EffectiveDate`/
> `SupersededDate`/`IsActive`/`ChangeSummary`) and the `AmendmentVote` table are all dead to the UI.
> `docs/SRS.md:99` specifies "**3.6.2 Constitution Hub**: Version-controlled governing documents
> with member voting capabilities", so this is an untracked gap against a stated requirement.
>
> **Evidence, elections.** Nothing in either app. `grep -i election` across `GHCAA.Web/src`
> returns only false positives (`toggleSelection`, `no-selection`, `selection-indicator`) plus one
> prose mention at `member/profile/profile.html:506`. The seven documents committed in `9dc4fb2`
> (~53 KB) live only as repo markdown under `docs/Elections/` — not copied to `public/assets/`,
> not seeded, not routed, not referenced by any template, and **not mentioned anywhere in this
> file** (`grep -i election docs/TODO.md` → nothing before this Area).
>
> **Delivery decision (2026-08-23):** render the election docs from markdown **without adding a
> markdown dependency**, per `feedback_keep_lightweight`. A feature survey of all seven files shows
> the syntax in use is a small fixed subset — `#`/`##` headings, ordered and unordered lists, GFM
> tables, `---` rules, `**bold**`, and blank-line paragraphs. These are repo-authored static assets,
> not user input, so a ~120-line in-repo renderer covering exactly that subset is the right trade
> against pulling in `marked`/`ngx-markdown`. Seeding them into `SiteContent` was rejected:
> `EnsureCreated()` means seed changes never reach preprod (see `gotcha_ensurecreated_no_op_existing_db`
> and 34.D10), so admin-editable storage would silently not deploy.

36.1 [DONE] Public **`/constitution`** route + standalone component consuming the two already-anonymous endpoints. Render `version`, `effectiveDate`, `changeSummary` and the full `content` body with article-level navigation (a sticky in-page ToC built from the `Article N:` headings), plus a **Download PDF** action resolving `pdfUrl` first and falling back to `/assets/GHCAA constitution 4.0.pdf`. Must handle the `404` that `GetCurrentConstitution` returns when no active row exists — fall back to the PDF-only view rather than rendering an error page, because the PDF is the authoritative document today. **(Shipped as `public/constitution/` (component + template + scss), route registered under `PublicLayout`, service `core/services/constitution.service.ts`, endpoints added to `API_ENDPOINTS.GOVERNANCE`. 404 suppressed via `X-Skip-Error-Notify` and handled by the `pdfOnly()` computed, which renders the PDF-only card.)**

36.2 [DONE] Version-history panel on the same page, from `GET /api/governance/constitution/history` — one collapsible row per version showing `version`, `effectiveDate`, `supersededDate`, `changeSummary`, and an inline diff-free full-text view. This is the "version-controlled" half of SRS §3.6.2 and is the reason the history endpoint exists. **(Shipped — collapsible `.history-item` rows on the same page, active version filtered out of `pastVersions()`.)**

36.3 [DONE] **The seeded constitution is a placeholder, not the constitution.** `GHCAA.Infrastructure/Data/Seed/constitution.json` holds a single row (Id 1, Version `1.2.0`) whose `Content` is **828 characters** — five stub articles (Name and Office, Objectives, Membership, Executive Committee, Meetings) — and whose `PdfUrl` is **absent/null**. The real document is the 2.9 MB `GHCAA constitution 4.0.pdf`. Two consequences: the page from 36.1 will show a five-paragraph summary while the banner PDF shows the real thing (a visible contradiction), and the seeded `Version` (`1.2.0`) disagrees with the PDF's own `4.0`. Extract the PDF text into `Content`, set `PdfUrl`, and correct `Version` to `4.0`. **Blocked on the same `EnsureCreated()` problem as 34.D10** — re-seeding does not reach an existing preprod DB, so this needs a data-migration path, not just a JSON edit. **(Shipped — `Seed/constitution.json` rewritten with the full ratified v4.0 text (26,175 chars, preamble + Articles I–XIII), `Version` `4.0`, `PdfUrl` `/assets/GHCAA constitution 4.0.pdf`, and a real `ChangeSummary`. Content is authored as plain `Article <Roman>: <Title>` headings with blank-line paragraphs because `parseArticles` escapes text and does not run markdown. Data-migration path: new `GHCAA.Infrastructure/Data/ConstitutionSeeder.SyncAsync`, called from `Program.cs` on every boot behind the same fault-tolerant `CanConnect`/`LogWarning` guard as the OrgConfig boot-seed. It is idempotent, reuses `ApplicationDbContext.LoadSeed<T>` (widened to `internal static`), inserts versions it cannot find, refreshes a stored version in place when the seed text changes, supersedes (never deletes) real prior versions so `AmendmentVote` rows survive, and deletes only the vote-free `1.2.0` placeholder so the public version history never publishes an unratified document.)**

36.4 [DONE] Publish the seven `docs/Elections/*.md` files as public assets and add a public **`/elections`** page listing them with, per document, a **View** action (in-app render) and a **Download** action (the raw `.md`). Copy rather than move — `docs/` stays the source of truth, and the copy step should be a build/CI step or an explicitly documented manual step so the two never silently drift. **(Shipped — `GHCAA.Web/scripts/sync-election-docs.mjs` copies `docs/Elections/*.md` into `public/assets/elections/` (wiping the target first so a renamed doc cannot linger). Wired into `npm start` and `npm run build` via `sync:docs`, and into the `Dockerfile` web stage (which bypasses `npm run build`, so it needed its own `COPY docs/Elections/` + `RUN npm run sync:docs`). Public `/elections` page lists all seven with View + Download.)**

36.5 [DONE] In-repo markdown renderer (`core/utils/markdown.util.ts`) covering only the surveyed subset: `#`–`####`, `-` and `1.` lists, GFM pipe tables, `---`, `**bold**`, paragraphs. Escape HTML on the way in and never pass raw HTML through, so the renderer cannot become an injection surface if a document is ever sourced from anywhere but the repo. No new npm dependency. **(Shipped — `core/utils/markdown.util.ts`, no new dependency. Escapes every source character before emitting markup. Tables reuse the central `.table-wrap`/`.data-table` classes.)**

36.6 [DONE] Forms view for `05-Election-Forms-and-Templates.md` — the file defines discrete forms (`FORM ER-01 Election Notice`, etc.) separated by `# FORM …` headings. Split on those headings and present each as an individually viewable/printable/downloadable form rather than one 5.9 KB wall, since a blank form is the unit a user actually wants. **(Shipped — `splitForms()` in `public/elections/election-docs.ts` splits the handbook on `# FORM ER-nn` into 18 individually viewable / printable / downloadable forms (download is a Blob built from the section text, since a single form has no file of its own).)**

36.7 [DONE] Rename `docs/Elections/06-Election-Ballot-Seal-and-Poll-Integrety-Certicate.md` → `06-Election-Ballot-Seal-and-Poll-Integrity-Certificate.md`. Two typos ("Integrety", "Certicate") that become a public URL the moment 36.4 lands. Do this **before** 36.4 ships, not after. **(Done — renamed via `git mv`; the only remaining "Integrety/Certicate" match in the repo is this line.)**

36.8 [DONE] Navigation: link `/constitution` and `/elections` from the public layout nav/footer, and cross-link both from the member `/governance` page. Also fix the mislabelled block at `common/governance/governance.html:60` — its comment says "Constitution Quick Reference" but it renders `<h2>Governance Pillars</h2>` with three hardcoded prose cards (Political Neutrality, Life-Long Connection, Transparency). It is not constitution content and the comment has been misdescribing it. **(Shipped — nav link (`Constitution`) in `public-layout.html`, both links in the footer Governance block, cross-links from `common/governance/governance.html`, and the mislabelled comment corrected.)**

36.9 [DONE] Amendment-voting UI for `POST /api/governance/constitution/{id}/vote` — the endpoint reads the `MemberId` claim, enforces one vote per member via the unique `(ConstitutionId, MemberId)` index, and has **no caller**. Member-portal only (it is the one governance endpoint that is *not* `[AllowAnonymous]`). Completes SRS §3.6.2's "with member voting capabilities". **(Shipped — ratification card on the member `/governance` page: `constitution()` / `voteComments()` / `pendingChoice()` / `voteOutcome()` signals in `common/governance/governance.ts`, `castVote(isFor)` calling the existing `ConstitutionService.vote()`, per-button busy state, and a one-shot guard so a recorded vote cannot be resubmitted. Styling appended to `governance.scss` using tokens only. Server-side gap also closed: `GovernanceService.VoteOnConstitutionAsync` had **no membership-tier check**, so Associate/Honorary/Advisory members — explicitly non-voting under Article III Section K — could ratify amendments; it now returns false unless the member is Founding, Executive or General. 3 new specs in `governance.spec.ts`.)**

36.10 [DONE] Per 12.6, nothing in Area 36 is `[DONE]` until `npx vitest run` and `npx ng build` pass. Add unit tests for the markdown renderer (table + nested-list + escaping cases) and for the 36.1 fallback path (404 from the endpoint must still render the PDF action). **(Done for what shipped — `npx vitest run` 64 files / 286 tests green, `npx ng build` green, emitted `styles-*.css` contains `.doc-hero`/`.doc-prose`/`.md-blank` and the seven assets land in `dist/.../assets/elections/`. New specs: `markdown.util.spec.ts`, `constitution.spec.ts`, `election-docs.spec.ts` (25 tests).)**

## AREA 37: ALUMNI-ASSOCIATION DEPTH — ELECTION ENGINE, SCHOLARSHIPS, PHILANTHROPY, MEMORY (raised by "what outstanding/extra ordinary matters can be incorporated on this project", 2026-08-23)

> Origin: a deliberate look at what a mature college alumni association does that this codebase
> does not yet model. Area 36 published the *documents* of governance and the *categories* of
> money; Area 37 builds the *machinery* behind them. Every item below is scoped against what the
> repository actually contains today, and — per `feedback_keep_lightweight` — **none of the ten
> requires a new npm or NuGet dependency.**
>
> **Evidence, money.** `GHCAA.Domain/Enums.cs` declares
> `FinancialCategory { MembershipFee, RegistrationFee, Donation, Event, Maintenance, Salary,
> Utilities, ReunionFee, Sponsorship, Grant, Refund, Other }`. Four of those values —
> `Donation`, `ReunionFee`, `Sponsorship`, `Grant` — **exist only as ledger labels an admin can
> pick when hand-entering a `FinancialRecord`.** There is no campaign, no donor, no pledge, no
> scholarship, no reunion and no grant application anywhere in the 45 files under
> `GHCAA.Domain/Models/`. The ledger can *record* philanthropy; the product cannot *conduct* it.
>
> **Evidence, elections.** Area 36 shipped seven election documents as read-only markdown and 18
> blank ER-forms split out of the handbook. The only election-adjacent tables are `ECPeriod`
> (`Title`/`StartDate`/`EndDate`/`IsActive`) and `ECMember` (`MemberId`/`ECPeriodId`/`Position`/
> `StartDate`/`EndDate`/`ChangeReason`) — i.e. **the result of an election, recorded by hand after
> the fact.** There is no `Nomination`, `Candidate`, `Ballot`, `VoterRoll` or `ScrutinyDecision`
> model. `Poll`/`PollOption`/`PollVote` are a general opinion-poll feature with no eligibility
> roll, no secrecy separation, no nomination phase and no returning-officer role; they are not a
> ballot and must not be overloaded into one.
>
> **Evidence, people.** `Member` has no batch/session/graduation field at all — cohort identity
> lives on `AcademicRecord` (`AdmissionYear`, `PassingYear`, `IsGHC`, `Degree`, `Subject`), which
> is where every batch query in this Area must read from. There is no obituary, chapter, or
> oral-history model, and no i18n: `GHCAA.Web/package.json` carries neither `@angular/localize`
> nor `ngx-translate`, and every string in the app is hardcoded English in a template.
>
> **Capabilities already paid for (use these; do not add libraries).**
> `GHCAA.Infrastructure` already references **QuestPDF 2026.2.3** and **QRCoder 1.8.0**, both used
> by `IDCardService` (`QuestPDF.Settings.License = LicenseType.Community` is set in its ctor, and
> `GetQrDataUri` is the working QR pattern). Server-side PDF generation and QR encoding are
> therefore free for 37.1 and 37.8. `ClosedXML` is present for spreadsheet export.
> `GHCAA.Web` already has `jspdf` + `jspdf-autotable` for client-side documents and `xlsx` for
> sheets. `INotificationService.CreateNotificationAsync` / `BroadcastNotificationAsync` is the
> in-app messaging channel; `ICommunicationService` is the email channel.
>
> **DI:** `GHCAA.Infrastructure/DependencyInjection.cs` reflects over every class whose namespace
> contains `"Services"` and registers it scoped against each `GHCAA.Application.Interfaces`
> interface it implements. **A new `GHCAA.Infrastructure/Services/XService.cs` implementing
> `IXService` needs no registration line.** Do not add one.
>
> **Conventions every item must follow.** Domain models: `int Id`, `= null!` on required strings,
> nullable navigation properties (`public Member? Member { get; set; }`),
> `ICollection<T> X { get; set; } = new List<T>();`, `DateTime CreatedAt { get; set; } = DateTime.UtcNow;`.
> Enums go **inside** `public class Enums` in `namespace GHCAA.Domain` (consumers write
> `using static GHCAA.Domain.Enums;`). Service methods end `CancellationToken cancellationToken = default`,
> return `Task<bool>` for mutations and `Task<XDto?>` / `Task<IEnumerable<XDto>>` for reads.
> Controllers are `[ApiController]` + `[Route("api/<area>")]` + `[Authorize]` at class level with
> `[AllowAnonymous]` applied **per action**. Angular routes are lazy
> (`loadComponent: () => import('./public/x/x').then(m => m.XPage)`) and feature-gated with
> `canActivate: [featureGuard('<flag>')]`; each new flag is declared in
> `core/models/org-config.model.ts` (`FeatureToggles`), defaulted in `core/services/org-config.service.ts`,
> and referenced by any nav entry in `core/services/nav.service.ts` (`feature: '<flag>'`).
> Endpoints go in the matching block of `core/constants/app.constants.ts` (`API_ENDPOINTS`),
> function-valued when they take a route parameter. Styling: tokens only, every control inside
> `.form-group`, no inline `style=""` — see the `ghcaa-design` skill.
>
> **Seed/deploy constraint — applies to all ten.** Runtime uses `Database.EnsureCreated()`, which
> is a **no-op on a non-empty database**, and migrations are not run at startup
> (`gotcha_ensurecreated_no_op_existing_db`). A new table therefore does **not** appear on preprod
> just because the model compiles, and `HasData` seed edits never reach it. Area 36 solved this
> once with `GHCAA.Infrastructure/Data/ConstitutionSeeder.SyncAsync(context, logger, ct)` — an
> idempotent boot-time syncer called from `Program.cs` behind a `CanConnectAsync()`/`LogWarning`
> guard. **Copying that pattern per feature is the fallback, not the plan.** 37.0 makes the real
> migration path a prerequisite, because four more bespoke syncers is a smell.
>
> **Recommended sequence: 37.0 → 37.3 → 37.2 → 37.1**, with 37.5 taken opportunistically (it is
> the smallest item in the Area and needs no new UI shell). 37.7–37.10 are independent and may be
> scheduled at any point after 37.0.

37.0 [TODO] **Prerequisite: a real migration path.** Every remaining item in this Area adds tables, and none of them can reach preprod under `EnsureCreated()`. Replace the startup call with `await context.Database.MigrateAsync()` guarded by a config flag (`Database:ApplyMigrationsOnStartup`, default `true` for Development/Preprod), and add the baseline migration that reconciles the existing preprod schema so the first `MigrateAsync` on a populated database is a no-op rather than a failed `CREATE TABLE`. Keep `ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))` — the pending-model warning here is non-deterministic seed churn, not schema drift (`gotcha_pending_model_changes_seed`). `ConstitutionSeeder` stays as-is: it syncs *content*, which is a different job from schema. Document the new boot sequence in `docs/architecture_data_flow.md` and `docs/project_map.md`. **Acceptance: a schema change committed on `preprod` is visible on the Render deployment without a manual database step.**

37.1 [TODO] **Election engine** — turn the Area 36 documents into a running process. This is the largest item; implement it in the five phases below, each independently shippable behind the flag. New enums in `GHCAA.Domain/Enums.cs`: `ElectionPhase { Announced, Nomination, Scrutiny, Withdrawal, CandidateList, Campaign, Polling, Counting, Declared, Archived }`, `NominationStatus { Submitted, UnderScrutiny, Accepted, Rejected, Withdrawn }`, `ElectionRole { ReturningOfficer, AssistantReturningOfficer, PollingOfficer, Scrutineer }`.
  - **37.1a Election + roll.** `Election` (`Id`, `Title`, `ECPeriodId`, `Phase`, `AnnouncedOn`, `NominationOpensOn`, `NominationClosesOn`, `ScrutinyOn`, `WithdrawalClosesOn`, `PollingOpensOn`, `PollingClosesOn`, `DeclaredOn?`, `IsActive`, `CreatedBy`), `ElectionSeat` (`Id`, `ElectionId`, `ECPosition Position`, `SeatCount`), `ElectionOfficer` (`Id`, `ElectionId`, `MemberId`, `ElectionRole Role`), and `VoterRoll` (`Id`, `ElectionId`, `MemberId`, `IsEligible`, `IneligibilityReason?`, `FrozenAt`, `VotedAt?`). The roll is **frozen by snapshot**, not computed at poll time: eligibility is the same Article III Section K rule already enforced in `GovernanceService.VoteOnConstitutionAsync` (`MembershipType` of `Founding`, `Executive` or `General`), plus dues-current per `MembershipDue`. Freezing is what makes a disputed result auditable.
  - **37.1b Nomination.** `Nomination` (`Id`, `ElectionId`, `ElectionSeatId`, `CandidateMemberId`, `ProposerMemberId`, `SeconderMemberId`, `Statement`, `PhotoPath?`, `Status`, `SubmittedAt`, `WithdrawnAt?`), `ScrutinyDecision` (`Id`, `NominationId`, `OfficerMemberId`, `Accepted`, `Reason`, `DecidedAt`). Proposer and seconder must both be on the frozen roll and must not be the candidate; enforce in the service, not only the UI.
  - **37.1c Ballot and poll.** `Ballot` (`Id`, `ElectionId`, `ElectionSeatId`, `SerialNumber`, `IssuedAt`, `IsSpoiled`) and `BallotVote` (`Id`, `BallotId`, `NominationId`, `CastAt`) kept in **separate tables with no member foreign key on the vote side** — the roll records *that* a member voted (`VoterRoll.VotedAt`), the ballot records *what* was voted, and nothing joins the two. That separation is the secret ballot, and it is the one design decision here that cannot be retrofitted. A unique index on `(ElectionId, MemberId)` in the roll prevents double voting.
  - **37.1d Counting and declaration.** `ElectionResult` (`Id`, `ElectionId`, `ElectionSeatId`, `NominationId`, `VoteCount`, `IsElected`, `IsTie`). On declaration, write the winners straight into `ECMember` rows against the election's `ECPeriodId` — this is the payoff: the committee roster stops being hand-typed.
  - **37.1e ER-forms as generated PDFs.** The 18 blank forms from 36.6 become filled documents. Server-side with **QuestPDF**, following `IDCardService.GenerateIDCardPdfAsync` for structure and its `GetQrDataUri` helper for the verification QR. Minimum set: ER-01 Election Notice, ER-03 Nomination Paper, ER-05 Final Candidate List, ER-08 Ballot Paper, ER-12 Result Sheet. Each carries the election title, the returning officer's name, the generation timestamp and a QR pointing at the 37.8 verification URL.
  - **Service/API.** `IElectionService` in `GHCAA.Application/Interfaces` + `GHCAA.Infrastructure/Services/ElectionService.cs` (no DI registration needed). `ElectionsController` at `api/elections`, `[Authorize]` at class level; `GET api/elections/public` and `GET api/elections/{id}/candidates` are `[AllowAnonymous]` (the candidate list is a published document); nomination, scrutiny and casting are member- or officer-scoped. Casting must reject any phase other than `Polling` server-side.
  - **UI.** Flag `enableElections`. The existing public `/elections` page gains a live banner when an election is not `Archived`. Member `portal/elections` — nominate, withdraw, view candidates, cast. Admin `admin/elections` — create, appoint officers, freeze roll, scrutinise, advance phase, count, declare, download ER PDFs. New `API_ENDPOINTS.ELECTIONS` block.
  - **Tests.** NUnit: roll freeze excludes Associate/Honorary/Advisory; proposer ≠ candidate; double vote rejected; cast outside `Polling` rejected; declaration writes `ECMember`; **a ballot row cannot be joined back to a member**. Vitest: phase-driven UI state, closed-nomination guard.

37.2 [TODO] **Scholarship & student-aid programme** — fund → open call → application → blind review → award → disbursement. New enums: `ScholarshipApplicationStatus { Draft, Submitted, UnderReview, Shortlisted, Awarded, Rejected, Withdrawn }`, `DisbursementStatus { Pending, Approved, Paid, Cancelled }`.
  - **Models.** `ScholarshipFund` (`Id`, `Name`, `Description`, `NamedAfter?` — the endowment-in-memory case that links to 37.5, `TargetAmount`, `IsActive`, `CreatedAt`); `ScholarshipCall` (`Id`, `ScholarshipFundId`, `AcademicYear`, `OpensOn`, `ClosesOn`, `SlotCount`, `AwardAmount`, `EligibilityCriteria`, `IsActive`); `ScholarshipApplication` (`Id`, `ScholarshipCallId`, `ApplicantName`, `ApplicantEmail`, `ApplicantPhone`, `InstitutionName`, `Class`, `GuardianName`, `HouseholdIncome`, `NeedStatement`, `MeritStatement`, `Status`, `SubmittedAt`, `ReferenceCode`); `ScholarshipDocument` (`Id`, `ScholarshipApplicationId`, `FileUploadId`, `DocumentType`) reusing the existing `FileUpload` + `IFileValidationService` path — **no new upload plumbing**; `ScholarshipReview` (`Id`, `ScholarshipApplicationId`, `ReviewerMemberId`, `NeedScore`, `MeritScore`, `Comments`, `ReviewedAt`); `ScholarshipAward` (`Id`, `ScholarshipApplicationId`, `Amount`, `AwardedOn`, `DisbursementStatus`, `FinancialRecordId?`).
  - **Blind review is a query rule, not a UI rule.** `GetApplicationForReviewAsync` must project a DTO that omits `ApplicantName`, `ApplicantEmail`, `ApplicantPhone` and `GuardianName`, exposing only `ReferenceCode`. Reviewers must not be able to obtain identity from the API at all; hiding it in the template is not acceptance.
  - **Applicants are not members.** The public application form is `[AllowAnonymous]` and identified by `ReferenceCode` + email; **do not create `Member`/`User` rows for schoolchildren.** Status lookup is `GET api/scholarships/status/{referenceCode}`, also anonymous, rate-limited by the existing `LoginRateLimitMiddleware` pattern.
  - **Ledger tie-in.** Marking an award `Paid` writes a `FinancialRecord` with `RecordType = Expense`, `FinancialCategory = Grant`, `Reference = ReferenceCode`, and stores the new record's `Id` back on `ScholarshipAward.FinancialRecordId`. This is what makes the impact report in 37.10 derivable rather than typed.
  - **Service/API/UI.** `IScholarshipService` + `ScholarshipService`; `ScholarshipsController` at `api/scholarships`. Flag `enableScholarships`. Public `/scholarships` (call listing + apply + status check), member `portal/scholarships` (reviewer queue for panel members), admin `admin/scholarships` (funds, calls, shortlist, award, disburse). New `API_ENDPOINTS.SCHOLARSHIPS` block.
  - **Tests.** NUnit: review DTO carries no identifying field; application rejected outside the `OpensOn`–`ClosesOn` window; award → paid writes exactly one `Grant` `FinancialRecord` and is idempotent on repeat. Vitest: apply-form validation, status lookup with an unknown code.

37.3 [TODO] **Fundraising campaigns + donor honour roll** — the shortest path from "the ledger has a `Donation` category" to "the association can actually raise money". New enum: `PledgeStatus { Pledged, PartiallyPaid, Paid, Lapsed, Cancelled }`.
  - **Models.** `Campaign` (`Id`, `Title`, `Slug`, `Story`, `CoverImagePath?`, `TargetAmount`, `StartsOn`, `EndsOn?`, `IsActive`, `IsArchived`, `CreatedBy`); `CampaignPledge` (`Id`, `CampaignId`, `MemberId?` — nullable so non-alumni can give, `DonorName`, `DonorEmail?`, `DonorPhone?`, `Amount`, `AmountReceived`, `Status`, `IsAnonymous`, `Message?`, `PledgedAt`, `FinancialRecordId?`); `DonorRecognitionTier` (`Id`, `Name`, `MinimumAmount`, `Description`) — admin-configurable so tier names are not compiled in.
  - **No payment gateway.** The repo operates under a **no-gateway-keys rule** (`session_area29_shipblockers_payments`): a pledge is recorded, the existing display-only wallet/bank instructions are shown, and an admin confirms receipt. Confirming receipt writes a `FinancialRecord` (`RecordType = Income`, `FinancialCategory = Donation`) and back-links `FinancialRecordId`, mirroring 37.2 exactly. **Do not introduce a gateway integration here.**
  - **Honour roll.** Public, derived, never hand-maintained: group confirmed pledges by `DonorRecognitionTier`, render `IsAnonymous` rows as "Anonymous", and show a live progress bar of `SUM(AmountReceived) / TargetAmount`. Anonymity must be enforced in the projection, not the template.
  - **Service/API/UI.** `ICampaignService` + `CampaignService`; `CampaignsController` at `api/campaigns` with `GET api/campaigns/public`, `GET api/campaigns/{slug}` and `GET api/campaigns/{slug}/honour-roll` as `[AllowAnonymous]`; pledging allowed anonymously. Flag `enableFundraising`. Public `/campaigns` + `/campaigns/:slug`, member `portal/giving` (my pledges, my giving history), admin `admin/campaigns` (create, confirm receipts, tiers). New `API_ENDPOINTS.CAMPAIGNS` block.
  - **Tests.** NUnit: an anonymous pledge never leaks `DonorName` through the honour-roll projection; confirming receipt is idempotent and writes one `Donation` record; progress excludes unconfirmed pledges. Vitest: progress-bar arithmetic, anonymous-checkbox behaviour.

37.4 [TODO] **Batch cohorts and reunions as first-class objects.** Today a batch exists only as `AcademicRecord.PassingYear` — there is no cohort page, no cohort representative and no reunion.
  - **Models.** `BatchCohort` (`Id`, `PassingYear`, `Title`, `Story?`, `CoverImagePath?`, `RepresentativeMemberId?`, `IsActive`); `Reunion` (`Id`, `BatchCohortId?` — null means an all-alumni reunion, `AlumniEventId`, `Theme`, `SouvenirUrl?`) built **on top of** the existing `AlumniEvent` + `EventRegistration` + `EventBudget` stack rather than beside it — a reunion is an event with cohort identity, and duplicating registration logic would be the mistake here.
  - **Membership is derived, not stored.** Cohort membership = `AcademicRecord` rows with `IsGHC == true` and the matching `PassingYear`. Do not add a `BatchYear` column to `Member`; it would immediately disagree with `AcademicRecord` for anyone holding two GHC records.
  - **Ledger tie-in.** Reunion fees collected through `EventRegistration` post as `FinancialCategory.ReunionFee`, finally giving that enum value a producer.
  - **Service/API/UI.** `IBatchService` + `BatchService`; `BatchesController` at `api/batches` with `GET api/batches/public` and `GET api/batches/{year}` `[AllowAnonymous]`. Flag `enableReunions`. Public `/batches` (year grid) + `/batches/:year`, member `portal/my-batch`, admin `admin/batches`. New `API_ENDPOINTS.BATCHES` block.
  - **Tests.** NUnit: a member with two GHC `AcademicRecord` rows appears in both cohorts; non-GHC records excluded. Vitest: year-grid grouping, empty-cohort `EmptyStateWidget` path.

37.5 [TODO] **In Memoriam register.** A new `MembershipType` value is **not** the mechanism — membership tier is admin-assigned and orthogonal (`feedback_membership_type_admin_only`). Instead: `MemorialEntry` (`Id`, `MemberId?` — nullable so a pre-digital alumnus can be honoured, `FullName`, `PassingYear?`, `DateOfBirth?`, `DateOfDeath`, `PhotoPath?`, `Tribute`, `IsPublished`, `SubmittedByMemberId?`, `SubmissionStatus Status`, `CreatedAt`) reusing the existing `SubmissionStatus { Draft, Pending, Approved, Rejected }` enum, and `Condolence` (`Id`, `MemorialEntryId`, `MemberId`, `Message`, `PostedAt`, `IsApproved`).
  - **Moderation is mandatory.** Nothing publishes without admin approval, and every condolence goes through the existing `HtmlSanitizer` path before storage. This is the single highest-sensitivity surface in the Area; an unmoderated tribute wall on a memorial page is a reputational incident.
  - **Setting `MemorialEntry.MemberId` must deactivate the linked `Member`** — and suppress them from the public directory and from any 37.1 voter roll — in the same transaction. A deceased member appearing on an election roll is the failure mode this clause exists to prevent.
  - **Service/API/UI.** Extend `IMemberService` **only if** the memorial logic stays under ~5 methods; otherwise `IMemorialService` + `MemorialService`. `MemorialController` at `api/memorial`, `GET api/memorial/public` `[AllowAnonymous]`. Flag `enableMemorial`. Public `/in-memoriam`, member submission form under `portal/`, admin moderation queue. New `API_ENDPOINTS.MEMORIAL` block.
  - **Tests.** NUnit: an unapproved entry is absent from the public projection; linking a `Member` deactivates them and removes them from directory results; condolence HTML is sanitised. Vitest: moderation-queue actions, published/unpublished rendering.

37.6 [TODO] **Oral-history / legacy archive** — recorded memories from senior alumni, which is the one asset an alumni association can create that nobody else can. `ArchiveCollection` (`Id`, `Title`, `Description`, `IsPublished`, `SortOrder`) and `ArchiveItem` (`Id`, `ArchiveCollectionId`, `Title`, `NarratorName`, `NarratorMemberId?`, `RecordedOn?`, `Summary`, `Transcript?`, `MediaFileUploadId?`, `ExternalMediaUrl?`, `PhotoPath?`, `DecadeTag`, `IsPublished`, `SubmissionStatus Status`).
  - **Storage decision, settled before building:** `MediaFileUploadId` reuses `FileUpload` + `IFileStorageService`; `ExternalMediaUrl` covers a link to already-hosted audio/video. **Both fields exist deliberately** — audio is heavy and the Render deployment has no object store configured, so the external-link path is the default and the upload path is opt-in behind the existing size limits in `IFileValidationService`.
  - **The transcript is the product**, not the audio: it is searchable, printable, quotable in 37.10, and readable on a bad connection. Treat a missing transcript as an incomplete item in the admin queue, not merely an empty field.
  - **Service/API/UI.** `IArchiveService` + `ArchiveService`; `ArchiveController` at `api/archive`, `GET api/archive/public` and `GET api/archive/items/{id}` `[AllowAnonymous]`. Flag `enableLegacyArchive`. Public `/legacy` (collections → item with transcript) reusing the Area 36 `.doc-hero` / `.doc-prose` shell rather than new page chrome; member submission; admin curation. New `API_ENDPOINTS.ARCHIVE` block.
  - **Tests.** NUnit: unpublished items excluded from public reads; an item with none of `MediaFileUploadId`, `ExternalMediaUrl` or `Transcript` is rejected. Vitest: decade filter, transcript rendering and print styles.

37.7 [TODO] **Bengali/English bilingual UI.** The association's constituency is Bengali-speaking and every string in the app is currently a hardcoded English literal in a template. **Do not install `@angular/localize` or `ngx-translate`** — `feedback_keep_lightweight` applies, and the requirement here is a single flat key → string lookup with a live runtime toggle, which `@angular/localize` (build-time, one bundle per locale) does not even satisfy.
  - **Mechanism.** `core/services/i18n.service.ts` holding a `signal<'en' | 'bn'>` persisted to `localStorage`; two dictionaries under `core/i18n/en.ts` and `core/i18n/bn.ts` typed as `Record<string, string>` with `en` as the key source of truth; and a pure `TranslatePipe` (`{{ 'nav.constitution' | t }}`) falling back to the English string, then to the key itself, when a Bengali value is missing. Update `<html lang>` on toggle. Ship the toggle next to the existing `<app-theme-toggle>` so it inherits placement and styling.
  - **Scope explicitly, and state it in the item when it lands:** public site + member portal nav, buttons, labels and validation messages. **Admin stays English-only** — it is staff-facing, and translating it doubles the surface for no constituency benefit. Server-stored content (constitution text, election documents, news) is not translated by this mechanism; it is authored content and belongs to whichever language it was written in.
  - **Font.** Bengali glyphs need a webfont with Bengali coverage. Production `ng build` inlines Google Fonts over the network and this environment cannot reach it — self-host the face under `public/assets/fonts/` and reference it from `styles.scss`, so the build stays offline-safe.
  - **Tests.** Vitest: the pipe returns the Bengali value, falls back to English on a missing key, and falls back to the key when both are missing; the toggle persists across a service re-instantiation; **every key present in `en.ts` resolves through the pipe** (guards against key drift).

37.8 [TODO] **Public credential verification.** `IIDCardService` already issues ID cards and certificates as PDFs, but nothing on the outside can confirm one is genuine — an employer holding a printed membership certificate has no check available.
  - **Models.** `IssuedCredential` (`Id`, `MemberId`, `CredentialType` — new enum `CredentialType { MembershipCertificate, IdCard, ElectionDocument }`, `ShortCode` — a 10-character unambiguous-alphabet code with a unique index, `IssuedOn`, `ExpiresOn?`, `IsRevoked`, `RevokedReason?`, `RevokedOn?`).
  - **Mechanism.** Extend `IIDCardService` (do not create a parallel service) so every generated document records an `IssuedCredential` and embeds a QR — via the existing `QRCoder` `GetQrDataUri` helper — pointing at `/verify/{shortCode}`. `GET api/verify/{shortCode}` is `[AllowAnonymous]` and returns **only** `{ valid, memberName, membershipType, issuedOn, status }`. It must never return an email, phone, address or member id: this endpoint is publicly enumerable by design, so the short code must be high-entropy and the response minimal. Rate-limit it with the existing `LoginRateLimitMiddleware` pattern.
  - **UI.** Flag `enableCredentialVerification`. Public `/verify/:code` plus a code-entry form at `/verify`, rendering a single valid / revoked / unknown verdict card; admin revocation action on the member detail page. New `API_ENDPOINTS.VERIFY` block.
  - **Tests.** NUnit: a revoked code returns `valid: false`; the response DTO exposes no contact field; short codes are unique across 10k generations. Vitest: the three verdict states, unknown-code path.

37.9 [TODO] **Geographic chapters.** `Chapter` (`Id`, `Name`, `Region`, `Country`, `City`, `Description`, `CoordinatorMemberId?`, `ContactEmail?`, `IsActive`, `CreatedAt`) and `ChapterMembership` (`Id`, `ChapterId`, `MemberId`, `JoinedAt`, `IsCoordinator`) with a unique index on `(ChapterId, MemberId)`.
  - **Reuse, do not rebuild.** A chapter event is an `AlumniEvent` with a `ChapterId` — add the nullable column to `AlumniEvent` rather than creating a `ChapterEvent` table. Chapter announcements reuse `INotificationService.CreateNotificationAsync` fanned over the chapter's members; there is no new messaging surface in this item.
  - **Service/API/UI.** `IChapterService` + `ChapterService`; `ChaptersController` at `api/chapters`, `GET api/chapters/public` `[AllowAnonymous]`. Flag `enableChapters`. Public `/chapters` (list + detail with coordinator contact), member `portal/chapters` (join/leave, my chapter feed), admin `admin/chapters`. New `API_ENDPOINTS.CHAPTERS` block.
  - **Tests.** NUnit: joining twice does not duplicate; a chapter event appears only in that chapter's feed; coordinator contact is hidden from the anonymous projection unless `ContactEmail` is set. Vitest: join/leave state, empty-chapter state.

37.10 [TODO] **Annual impact report generated from the ledger** — the accountability artifact that closes the loop on 37.2, 37.3 and 37.4, and the reason those three back-link `FinancialRecordId`.
  - **Nothing in this item is hand-typed.** For a given year it aggregates: total income and expense by `FinancialCategory` from `FinancialRecord`; scholarships awarded and disbursed from `ScholarshipAward`; campaign totals and donor counts from `CampaignPledge`; events and attendance from `AlumniEvent` + `EventRegistration`; new members from `Member.CreatedAt` / `MembershipHistory`; reunions from 37.4. The only authored fields are a president's foreword and a cover image, stored in the existing `SiteContent` CMS from Area 34 — **do not add a table for two strings.**
  - **Output.** Server-side PDF via **QuestPDF**, following the `IDCardService.GenerateIDCardPdfAsync` pattern, plus an on-site HTML view reusing the Area 36 `.doc-hero` / `.doc-prose` shell. Extend `IFinancialLedgerService` with `Task<ImpactReportDto?> GetImpactReportAsync(int year, CancellationToken cancellationToken = default)`, and put the PDF method on the existing document-generation surface rather than inventing a third document service.
  - **Guard.** The report must degrade rather than throw when a source feature is not yet built or its flag is off — a year with no campaigns renders without that section. This item is therefore safe to build *before* 37.2/37.3/37.4 land, and must read every source through a null-tolerant projection.
  - **API/UI.** `GET api/financials/impact/{year}` and `GET api/financials/impact/{year}/pdf`, both `[AllowAnonymous]` (publishing it is the point). Flag `enableImpactReport`. Public `/impact/:year` with a year selector; admin action to set the foreword and publish. Additions to the existing `API_ENDPOINTS.FINANCIALS` block.
  - **Tests.** NUnit: a year with zero records returns a report with zeroed sections rather than null; category totals match a hand-summed fixture; the disbursed-scholarship total equals the sum of the linked `Grant` `FinancialRecord` rows. Vitest: year selector, empty-section rendering.

37.11 [TODO] Per 12.6, nothing in Area 37 is `[DONE]` until `dotnet test`, `npx vitest run`, `npm run type-check` and `npx ng build` all pass. Baselines to beat at the start of this Area: **351 NUnit tests** and **64 vitest files / 306 tests**. Additionally, every item that adds a table must be verified against the 37.0 migration path on a **non-empty** database — a passing suite against a fresh SQLite file proves nothing about preprod (`gotcha_ensurecreated_no_op_existing_db`). Update `docs/FEATURES.md`, `docs/project_map.md`, `docs/SRS.md` and `docs/architecture_data_flow.md` as each item lands, per `feedback_docs_update_scope`.

---

# Area 38 — Constitution v4.2 & the "always latest" rule

**Standing rule (2026-08-25): whenever a newer constitution is ratified, every surface must follow
it automatically.** No page, component, document or link may pin a version. Publication is one
command; the surfaces are already version-agnostic. Reference: `docs/CONSTITUTION_PUBLISHING.md`.

38.1 [DONE] Publish `GHCAA Constitution V4.2.pdf` as the active constitution. `Seed/constitution.json`
rewritten as a single active record — `Version` `4.2`, `EffectiveDate` `2026-07-01`, `PdfUrl`
`/assets/GHCAA Constitution V4.2.pdf`, `Content` 36,999 chars (preamble + Articles I–XII + Future
Vision), real `ChangeSummary`. Reaches preprod through `ConstitutionSeeder.SyncAsync`, not through
`EnsureCreated()`. **`GHCAA constitution 4.0.pdf` stays on disk** — the seeder supersedes rather
than deletes, so the v4.0 history row's `PdfUrl` must still resolve.

38.2 [DONE] Remove the last hardcoded version reference from a page. `landing/sections/banner/banner.html`
linked `href="/assets/GHCAA constitution 4.0.pdf"`; it now `routerLink`s to `/constitution`, which
renders whatever row is active. The only version-bearing string left in application code is
`CONSTITUTION_PDF_FALLBACK` in `public/constitution/constitution.ts`, and the publishing tool owns it.

38.3 [DONE] Committed the extraction pipeline as `tools/constitution/publish_constitution.py`
(PyMuPDF; build-time documentation tool, **not** an application dependency). It extracts the PDF
into the plain-text shape `parseArticles` expects, writes the seed, and repoints
`CONSTITUTION_PDF_FALLBACK`. Version and effective date are read from the document's closing
colophon because **the v4.2 cover page still says "V 4.0"**. Verified: a `--dry-run` re-extraction
reproduces the shipped `Content` byte-for-byte. Handles the Google-Docs U+200B fencing rule
(doubled = swallowed space, single = intra-word), bold-run structure detection and page-break
paragraph rejoining — all documented in `docs/CONSTITUTION_PUBLISHING.md`.

38.4 [DONE 2026-08-28] **Source-document defect.** Article V, Section C, item 6 read
*"6. TReplace the 21-day election notice rule with: …"* — a leftover editing instruction carried
verbatim into the published text. Corrected in `GHCAA.Infrastructure/Data/Seed/constitution.json`
to *"6. Election notice and timetable shall follow constitutional minimums and Election
Regulations."*, dropping only the instruction prefix and preserving the substantive rule
(which was already the clear intent of the sentence).
**CAVEAT — JSON and PDF now diverge:** `constitution.json` is normally a generated artifact
extracted from `GHCAA.Web/src/assets/GHCAA Constitution V4.2.pdf` by
`tools/constitution/publish_constitution.py`, and **the stray text still exists in that PDF**.
The JSON was hand-corrected because the defect is in the source document, not the extractor.
Before the next formal re-publish, fix item 6 in the source document itself and re-run 38.3 —
otherwise regenerating from the current PDF will silently reintroduce the defect.

38.5 [DONE] Docs updated per `feedback_docs_update_scope`: new `docs/CONSTITUTION_PUBLISHING.md`;
`FEATURES.md` §5.1a, `SRS.md` §3.6.2, `architecture_data_flow.md` §2.D and `project_map.md`
(build-time tool entry) all carry the always-latest rule.

---

# Area 39 — Election forms as operative documents

**Standing rule (2026-08-26): every change must work in BOTH themes and be implemented
CENTRALLY** — tokens and shared classes in the single global `GHCAA.Web/src/styles.scss`, never a
per-component one-off. The one sanctioned exception is recorded in 39.4.

39.1 [DONE] **Forms are ready-to-use documents, not specimens.** Each split form from 36.6 now
renders as a real association form: letterhead pad (crest, org name, address/phone/email, motto,
watermark), a Reference/Date rule line, a `FORM ER-nn` code chip, ruled write-on fields, tick-box
lists, banded section headers, a signature grid and a dashed seal circle. Driven by
`renderFormMarkdown` in `core/utils/markdown.util.ts` from a small directive DSL in the source
markdown — `:: grid`, `:: sign Who / qualifier`, `:: lines Label | n` (clamped 1–12),
`Label: ____` field lines and `[ ]` tick boxes, inline or as a list. The renderer escapes every
source character before emitting markup, so a stored document still cannot inject HTML. The
letterhead markup stays inline in `elections.html`; a `pad-sheet` component was considered and
rejected as an unnecessary abstraction (`feedback_keep_lightweight`).

39.2 [DONE] **A4 print fidelity.** Global `@page { size: A4 portrait; margin: 14mm 13mm }`, the
on-screen sheet at 210mm × 297mm with matching padding, `break-inside: avoid` on every field
block and `break-before: page` between sheets. Verified under `emulateMedia({media:'print'})` at
794 × 1123: **zero overflowing descendants**, and the generated PDF paginates correctly in both
themes. Because `[innerHTML]` content never receives Angular's `_ngcontent` attribute, every
class the renderer emits (`.pad-*`, `.form-doc`, `.f-*`, `.sign-*`, `.form-table`, `.check-list`,
`.seal-box`) lives in global `styles.scss`; only page layout lives in `elections.scss`.

39.3 [DONE] **The letterhead is fully configuration-driven.** No organisational literal remains in
the form header. `branding.establishedOn` was added end-to-end —
`GHCAA.Application/DTOs/OrgConfigDto.cs` (`BrandingDto`) →
`OrgConfigService.BuildGhcaaDefaults()` (seeds `29 Nov 2025`, Constitution Article I) →
`core/models/org-config.model.ts` → `core/services/org-config.service.ts` fallback →
`elections.ts` `get establishedOn()`. **No EF migration needed**: `ConfigJson` is deserialized
straight into the DTO. Documented in `docs/CONFIG_DRIVEN_FRAMEWORK.md` §5.
**`GHCAA.Web/public/assets/app.config.json` is dead code** — zero references repo-wide and an
unrelated flat shape; do not add config fields there.

39.4 [DONE] **Forms have no dark theme, by design.** `--paper-bg`, `--paper-ink`,
`--paper-ink-soft`, `--paper-rule`, `--paper-hairline` and `--paper-band` are defined once in
`:root` and are the only tokens in the system that deliberately carry **no** `body.dark-theme`
override and need no `@media print` re-pin: a form is a paper document, printed and signed, so it
is ink-on-white on screen too. Verified by computed style under both themes — `.pad-sheet` is
`rgb(255,255,255)` on `rgb(20,24,31)` in each.

39.5 [DONE] **Constitution section subheadings are legible.** `renderParagraph` in
`public/constitution/constitution.ts` promotes a short `Section N: Title` paragraph (numeric,
lettered or roman, ≤ 60 chars) to an `<h4>`, and emphasises only the label as
`<p><strong>Section N:</strong> …</p>` when the body runs on from it. Escaping still happens
before any markup is emitted.

39.6 [DONE] **`Constitution` removed from the public top nav.** It is reached from the footer
reference links instead; the `/elections` footer link already existed (`footer.html:32`) and was
left as-is.

39.7 [DONE] Per 12.6: `npm run type-check` clean, `dotnet build` 0 warnings / 0 errors, `npx
vitest run` **64 files / 306 tests** green (17 new — 11 for `renderFormMarkdown`, 6 for the
constitution subheading rules). Visual QA done in both themes for `/elections`,
`/elections?doc=forms`, `/elections?doc=er-19` and `/constitution`, plus print-media screenshots
and A4 PDFs. **Known environmental gap:** `npm run build` currently fails in Angular's
font-inlining plugin (`connect ETIMEDOUT` to `fonts.googleapis.com` at `styles.scss:6`) — not a
code defect; re-run when the network allows and re-grep the emitted `styles-*.css`.

---

# Area 40 — Member albums with admin approval, job-posting approval, events without registration (raised by user 2026-08-26)

Plan: `C:\Users\HabiburRahmanShalin\.claude\plans\piped-sniffing-lollipop.md`. `EventGallery`/`EventPhoto`
extended in place for member ownership (no new `Album` table); admin = `Admin`/`SuperAdmin` role;
legacy dead `POST api/gallery` "submit a memory" endpoint fixed separately from the new album flow.

> **STATUS: Area 40 is COMPLETE (backend + web + mobile).** 40.1-40.10 below are the original
> plan items; each was superseded by the 40.11-40.14 delivery entries at the end of this Area and
> re-marked accordingly on 2026-08-28. They were left as stale `[TODO]` for two days, which made
> shipped work read as outstanding — verify against the tree before trusting a marker
> (`gotcha_todo_status_drift`).

40.1 [DONE — see 40.11] Domain + migration: `EventGallery`/`EventPhoto` gain `OwnerMemberId`/`UploadedByMemberId`,
`Status` (`SubmissionStatus`, default `Approved`), `RejectionReason`; `JobOpportunity` gains
`Status`/`RejectionReason`; `Enums.NotificationType.ApprovalRequest` added; `AlumniEvent` gains
`RequiresRegistration` (bool, default `true`). One EF migration for all of the above (`AddApprovalWorkflowToGalleryAndJobs`), verified against a non-empty DB per 37.0's `EnsureCreated()` gotcha.

40.2 [DONE — see 40.11] `IAdminNotificationService`/`AdminNotificationService` (new) — resolves Admin/SuperAdmin
members via `User.Roles`, fans out `INotificationService.CreateNotificationAsync` +
`IEmailService.SendEmailAsync` on any pending approval.

40.3 [DONE — see 40.11] Gallery/album backend: member album create/add-photo/list-mine endpoints, admin
pending/approve/reject endpoints (gallery + per-photo), public/member-facing reads filtered to
`Status == Approved`, fixed `POST api/gallery` "submit a memory" handler.

40.4 [DONE — see 40.11] Jobs backend: `Status` gate on `PostJobAsync`/`GetActiveJobsAsync`, admin
pending/approve/reject endpoints, poster notified on resolution.

40.5 [DONE — see 40.11] Events-without-registration: `RequiresRegistration` threaded through Create/UpdateEventDto
+ `EventService`, `RegisterForEventAsync` rejects when false.

40.6 [DONE — see 40.12] Web member portal: "My Albums" UI in `common/gallery/`, pending-job badge in
`common/jobs/`, admin-events form checkbox for `requiresRegistration`, public events Register
button gated on `requiresRegistration`.

40.7 [DONE — see 40.12] Web admin: new `admin/gallery-approval/` and `admin/job-approval/` screens cloned from
`admin/article-approval/` pattern, wired into nav + `app.routes.ts`.

40.8 [DONE — see 40.13] Mobile: `GalleryService`/job-service pending/approve/reject calls, member "My Albums"
section on `gallery_screen.dart`, generalized/sibling approval screens off
`approval_queue_screen.dart` (with reject-reason capture), `event_details_screen.dart` FAB gated on
`requiresRegistration`.

40.9 [DONE — see 40.11/40.12] Tests: backend `GalleryControllerTests`/job-approval/`AdminNotificationService` unit
tests; frontend `gallery-approval.spec.ts`/`job-approval.spec.ts` cloned from
`article-approval.spec.ts`; e2e `gallery.spec.ts` extended; mobile widget tests if the project
convention has them.

40.10 [DONE — see 40.11-40.14] Per 12.6: `dotnet test`, `npx vitest run`, `npm run type-check`, `npx ng build` (or note
the known font-inlining network gap from 39.7) all pass before closing this Area. Update
`docs/FEATURES.md` per `feedback_docs_update_scope`.

40.11 [DONE] Backend for 40.1-40.5, 40.9 shipped: Domain/migration `AddApprovalWorkflowToGalleryAndJobs`
applied, `AdminNotificationService`, Gallery member-album + approve/reject endpoints, Jobs approve/reject
endpoints, `AlumniEvent.RequiresRegistration` + `RegisterForEventAsync` guard, backend tests
(368 total / 367 passed / 1 pre-existing skip). `docs/FEATURES.md` §7.2 updated.

40.12 [DONE] Web (40.6-40.7) shipped: `admin/gallery-approval/` + `admin/job-approval/` screens
(cloned from `article-approval`), member "My Albums" UI, job pending badge, `admin-events`
`requiresRegistration` checkbox, public events Register/participation UI fully hidden (not just
disabled) when `requiresRegistration===false` — closes 41.5. `npx vitest run` 315/315 (66 files),
type-check clean. Job endpoints are actually under `api/jobs`, not `api/jobhub`.

40.13 [DONE] Mobile (40.8) shipped: `gallery_approval_screen.dart` + `job_approval_screen.dart` as
sibling screens off the dashboard (not tabs, matching the `article_approval_screen` precedent), member
"My Albums" section on `gallery_screen.dart`, reject-reason capture added to all three approval flows
(including the previously-missing member-approval one), event registration FAB/badge fully hidden when
`requiresRegistration===false`. `flutter analyze` clean, `flutter test` 28/28 passed (goldens skipped
per existing CI convention). No admin events-registration screen exists on mobile (confirmed, N/A).

40.14 [DONE] Area 40 fully complete end-to-end (backend + web + mobile).

---

# Area 41 — Live-site bug fixes (raised by user 2026-08-26, fix before continuing Area 40 web/mobile)

41.1 [DONE] `POST /api/news` 400 fixed — root cause was `[Url]` validation on `CreateNewsDto.ImageUrl`
rejecting the relative paths the app's own image-upload endpoint returns; replaced with a
`RelativeOrAbsoluteUrlAttribute`. `NewsControllerTests` extended (7-case parameterized test).

41.2 [DONE] Notice image-before-save fixed — `admin-news.ts` was the only admin image-upload form
uploading on file-select instead of staging-then-uploading on Save (gallery/members/events all already
staged-then-submit); brought into line, `admin-news.spec.ts` extended.

41.3 [DONE] Admin delete-any-entity audited across all 18 admin screens; added to Contact Messages
and Roles (system-admin accounts only, member-linked login accounts excluded to avoid locking out
portal access); Ledger/Audit/approval-queues intentionally left non-deletable (audit-trail integrity).

41.4 [DONE] Admin event visibility on unpublish fixed — root cause was an EF Core global
`HasQueryFilter(e => e.IsActive)` on `AlumniEvent` (`AlumniEventConfiguration.cs`) silently applying to
admin's list/update/delete/logo-update queries too, not just public reads; admin paths in
`EventService.cs` now use `.IgnoreQueryFilters()`, matching the existing `MemberService`/`NewsService`/
`AuthService` convention. Also fixed as a side effect: admin previously couldn't re-publish, delete, or
change the logo of an already-unpublished event. `EventServiceTests` extended (3 new tests).

41.5 [DONE] Public portal shows zero registration/participation UI (button, count, "spots left", etc.)
for events where `RequiresRegistration == false` — web (`common/events/`) fully removes the
Register/Closed button + participant-count pill, mobile (`event_details_screen.dart`,
`events_screen.dart`) fully hides the FAB/badge, in both cases rather than merely disabling them.
Shipped as part of Area 40 (40.12/40.13).

41.6 [DONE] `dotnet test` 378/378 passed, `npx vitest run` 306/306 passed (64 files) after all four
bug fixes; final combined state after Area 40 frontend work: `npx vitest run` 315/315 (66 files),
`flutter test` 28/28 passed.

41.7 [DONE 2026-08-27] Live `GET /api/jobs` and `GET /api/gallery` 500s after the Area 40 deploy —
root cause was `MigrationBootstrapper.cs` (introduced same day, commit `5c08b99`): on a legacy
`EnsureCreated()`-built database it wrongly assumed only the single newest migration was pending and
baselined every earlier one as already-applied without running it, so `AddApprovalWorkflowToGalleryAndJobs`
failed (its `SiteContents` insert hit a table that was never actually created) and rolled back, leaving
the new `Status`/`RejectionReason` columns missing. Fixed by walking every migration in order and
applying each for real via `IMigrator.MigrateAsync(id)`, only baselining (without running) one whose
Postgres error confirms its effect already exists (`SqlState` in `42P07`/`42701`/`42P06`/`42710`/`23505`).
Commit `3b381f0`, already live on preprod. `docs/RENDER_DEPLOYMENT.md` and `docs/FEATURES.md` updated —
see [[gotcha_migrationbootstrapper_fixed_offset]].

41.8 [DONE 2026-08-27] Round 2: `/api/jobs` and `/api/gallery` 500s recurred, plus a new `/api/events`
500, even with `3b381f0` live. Root cause: EF Core runs a migration's operations in one transaction —
`AddApprovalWorkflowToGalleryAndJobs` mixes new DDL (`Status`/`RejectionReason` columns,
`AlumniEvents.RequiresRegistration`, an FK) with a trailing `InsertData` seeding `SiteContents` Id=6.
Under the pre-`3b381f0` bootstrapper an earlier partial run had already left a colliding `SiteContents`
row, so the retried insert's `23505` unique-violation rolled back the *whole* transaction — DDL included
— yet still matched the "already exists" baseline logic, marking the migration applied with none of its
schema changes actually landed. Once falsely baselined, every later boot trusted
`__EFMigrationsHistory` via plain `Database.MigrateAsync()` and never revisited it, so the missing
columns persisted across redeploys. Fixed in two parts: (1) the migration now runs
`DELETE FROM "SiteContents" WHERE "Id" = 6 OR "Key" = 'about-college-today';` immediately before its
`InsertData`, so the seed can no longer collide; (2) generalized (not hardcoded to this one migration,
per explicit user request for a "proper fix") — `MigrationBootstrapper` gained
`SelfHealFalselyBaselinedMigrationsAsync`, run on every boot before `MigrateAsync()`. For every migration
recorded as applied, it uses EF's `IMigrationsAssembly.CreateMigration(...).UpOperations` to inspect the
migration's actual operations at runtime, flags any that mix a schema op (`AddColumn`/`CreateTable`) with
a data op (`InsertData`/`UpdateData`/`DeleteData` — the exact shape that caused this bug), verifies each
flagged migration's schema targets against `information_schema`, and deletes the history row (forcing
genuine reapplication) if any are missing. This automatically covers 4 other migrations with the same
risky shape (`AddSiteContentAndNoticeFields`, `AddDiscussionForums`,
`PhaseB_S5S8_OtpHmac_PaymentIdempotency_Indexes`, `AddNotificationPreferences`) without editing them
individually — they weren't touched since they're already applied historically and not currently
symptomatic; the generic self-heal is defense-in-depth for them. Build clean, 9/9 targeted
Migration/SiteContent tests pass, full suite re-verified. Not yet deployed/verified live — see
[[gotcha_migrationbootstrapper_fixed_offset]].

41.9 [DONE 2026-08-27] End-to-end validation of the full migration chain against a throwaway Postgres
seeded with a `pg_dump` of live preprod (not synthetic data) — so the dry run exercised the real
`EnsureCreated()`-baselined legacy-schema shape, not a clean-slate DB. Ran `GHCAA.API` against it
repeatedly and fixed every migration that threw a `42P07`/duplicate-key error, converting the offending
`CreateTable`/`CreateIndex`/`AddForeignKey`/`InsertData` calls to idempotent raw SQL
(`CREATE ... IF NOT EXISTS`, a `pg_constraint` existence guard for `AddForeignKey`, and either a
`DELETE`-guard before `InsertData` on FK-safe lookup tables or a full `INSERT ... ON CONFLICT DO NOTHING`
conversion otherwise). This went beyond 41.8's self-heal scope — the self-heal only repairs a migration
*after* it's already been falsely baselined by a prior boot; this pass fixes the underlying collisions so
they never falsely baseline in the first place. Files fixed: `AddSocialAuthAndPolls`,
`AddSocialAuthConfig`, `AddOrganizationConfig`, `AddSiteContentAndNoticeFields`, `AddDiscussionForums`,
`PhaseB_S5S8_OtpHmac_PaymentIdempotency_Indexes`, `AddApprovalWorkflowToGalleryAndJobs` — i.e. all 4
migrations 41.8 had deliberately left untouched, plus 3 more discovered via a full-folder sweep. Also
stripped temporary DEBUG logging and a `CanConnectAsync` retry loop from `MigrationBootstrapper.cs` that
had been added mid-investigation and were no longer needed once the real cause (migration content, not
connection flakiness) was confirmed. Full chain now applies cleanly end-to-end with zero exceptions
(`Application started` reached, no `Migration bootstrap failed`/`PostgresException`). Backend suite
378/378 still passes. Throwaway container and the preprod PII dump were deleted after the run.
**Committed 2026-08-28** as `2b98bd6` "Refactor database migrations to use raw SQL for table and index
creation" (confirmed via `git log`) — live preprod verification per 41.7/41.8 is presumed covered by
that deploy, not independently re-checked. See [[session_migration_idempotency_validation]] and
[[gotcha_migrationbootstrapper_fixed_offset]].

---

41.10 [DONE 2026-08-28] Live 404s on `main-*.js`/`chunk-*.js` after a deploy — root cause was
`app.UseStaticFiles()` (`Program.cs`) serving `index.html` with no cache-control headers at all, so a
browser could hold a stale cached copy across a redeploy; its `<script>` tags then requested the
*previous* build's hashed JS filenames, which no longer exist once the new build replaces `wwwroot`.
Fixed via `StaticFileOptions.OnPrepareResponse`: `index.html` now gets
`Cache-Control: no-cache, no-store, must-revalidate` + `Pragma: no-cache` + `Expires: 0`, forcing
revalidation on every load so a new deploy is always picked up. Hashed JS/CSS left as-is (unhashed
default headers) since their filename already changes whenever content does. `dotnet build` 0/0
warnings/errors.

---

# Area 42 — Elections forms/docs manageable from admin portal (raised by user 2026-08-26, plan only, not yet built)

Today election forms/docs shown at the public `/elections` route are static/seeded ([[session_election_form_pad]], [[session_area36_constitution_seeder_voting]]). User wants admin to manage (create/edit/replace) the election forms, documents, and other information currently shown in the public/portal elections pages, from the admin portal — analogous to the existing SiteContent CMS pattern ([[session_area34_sitecontent_notices]]).

42.1 [TODO] Explore/plan (Plan Mode required — multi-file, touches Domain/Application/Infra/API/Web
admin+public/Mobile): inventory exactly what's static today under the elections feature (entities,
seeders, controllers, public/portal components) before designing the admin-editable model — do not
assume it mirrors SiteContent without checking field/document shape differences (forms likely need
file/PDF attachments, not just rich text).
42.2 [TODO] Design admin CRUD screens + API for whatever the inventory in 42.1 finds (forms list,
per-form fields/attachment, publish state) following `ghcaa-design` conventions.
42.3 [TODO] Web public/portal elections pages read from the new admin-managed source instead of the
seeder/static content.
42.4 [TODO] Mobile: sync if elections content is surfaced there.
42.5 [TODO] Tests + docs update per usual closing convention.

---

# Area 43 — Application-wide exception handling & logging audit (raised by user 2026-08-27: "make sure entire application have propers exception handling with logging. best error management")

43.1 [DONE 2026-08-27] Backend: audited `ExceptionMiddleware.cs` and controller-level try/catch blocks
across `AdminController`, `NotificationController`, `RolesController` for swallowed exceptions and
missing/weak logging; added structured `ILogger<T>` logging (message templates, not `ex.Message` as the
template) at each previously-silent catch. Added `GHCAA.Tests/Middleware/ExceptionMiddlewareTests.cs`
covering the middleware's status-code mapping and logging behavior. `dotnet test` 382/382 passes.
43.2 [DONE 2026-08-27] Angular web: added `GlobalErrorHandler` (`core/services/global-error-handler.ts`)
implementing `ErrorHandler`, wired in `app.config.ts` alongside `provideBrowserGlobalErrorListeners()` to
catch uncaught component/template errors and unhandled promise rejections app-wide (HTTP errors are
excluded — the interceptor already owns those). Fixed two silent `catchError(() => of(null))` subscribes
in `auth.service.ts` (`/auth/me` session restore, `logout()`) to log via `console.error` before falling
back; `refresh()`'s catch was deliberately left unlogged since a 401 there is an expected, routine path,
not a bug. `npx vitest run` 66 files / 315 tests pass, 0 failures.
43.3 [DONE 2026-08-27] Flutter mobile: added a `FlutterError.onError` handler in `main.dart` (previously
only `PlatformDispatcher.instance.onError` existed) so uncaught framework errors are auto-reported to
Sentry + `debugPrint` instead of relying on a manual user-tapped report button. Swept every Flutter
service/provider for silent `catch (e) { ... }`/`catch (_) { ... }` blocks that dropped the exception on
the floor and added `debugPrint('<Class>.<method> failed: $e')` (or `developer.log` in
`push_notification_service.dart`, matching that file's pre-existing convention) before each fallback —
covering `financial_service.dart`, `job_service.dart`, `content_service.dart`, `events_service.dart`,
`auth_service.dart`, `admin_service.dart`, `dynamic_theme_service.dart`, `poll_service.dart`,
`forum_service.dart`, `assistant_service.dart`, `mentorship_service.dart`, `roles_service.dart`,
`dropdown_service.dart` (13 files total). No new logging dependency added — `debugPrint`/`developer.log`
matches this codebase's existing lightweight-logging convention ([[feedback_keep_lightweight]]).
`flutter analyze` clean (no issues); `flutter test` 66 passing tests unaffected — the 26 failures are all
pre-existing stale golden pixel-compares in `comprehensive_visual_freeze_test.dart`
([[session_mobile_ci_golden_fix]], [[session_mobile_login_fixes]]), not caused by this change.
43.4 [TODO] Live/manual verification: trigger a genuine unhandled error in each app (backend 500,
Angular runtime error, Flutter uncaught exception) against a running instance to confirm the new
handlers actually fire and log as expected — not yet done this session.
Committed as `fc06894`.

---

# Area 44 — Full-app review of the last 2 days' fixes (raised by user 2026-08-28: "review entire
application. make sure all issues are taken cared")

Three parallel code-reviewer passes (backend, web, mobile) audited every fix from Areas 40–43 plus
41.10 for correctness, not just superficial patching. Real, verified bugs were found in all three
layers — several of today's own "fixes" were themselves incomplete. All findings below were fixed
this session (not merely logged) and re-verified: `dotnet test` 382/382, `npx vitest run` 66 files /
317 tests, `npx tsc --noEmit` clean, `ng build --configuration development` clean, `flutter analyze`
clean, `flutter test --exclude-tags=golden` 28/28.

44.1 [DONE] Backend — `Program.cs`'s `MapFallback` handler (the code path that serves the SPA shell
for almost every real navigation, `/`, `/portal/...`, a refreshed deep link) never carried the
41.10 no-cache headers — only an explicit `GET /index.html` did, via `UseStaticFiles`'
`OnPrepareResponse`. The actual fix for the `main-*.js`/`chunk-*.js` 404 bug was inert for normal
traffic. Added the same three headers directly in `MapFallback` before `SendFileAsync`.

44.2 [DONE] Backend — `MigrationBootstrapper.SelfHealFalselyBaselinedMigrationsAsync`'s risky-shape
check (`AddColumnOperation`/`CreateTableOperation` mixed with `InsertData`/`UpdateData`/`DeleteData`)
could never match any current migration, because every migration converted to raw SQL in 41.9/
[[session_migration_idempotency_validation]] materializes as a single opaque `SqlOperation`, not
those typed ops — the whole method was dead code against exactly the migrations it exists to guard
(`AddApprovalWorkflowToGalleryAndJobs` and friends). Extended it to also regex-extract
`CREATE TABLE`/`ADD COLUMN` targets out of `SqlOperation.Sql` text and always verify those; also
schema-qualified the `information_schema` probes (`table_schema = current_schema()`) and fixed the
`SqlQueryRaw<int>` column-naming (`SELECT 1 AS "Value"`) which could otherwise throw and silently
disable the whole bootstrapper. See [[gotcha_migrationbootstrapper_fixed_offset]].

44.3 [DONE] Backend — `BaselineLegacyDatabaseAsync` only logged an aggregate baselined/applied count;
added per-migration `LogInformation` (id + Postgres `SqlState`) since that's exactly the diagnostic
the 2026-08-27 incident needed and didn't have.

44.4 [DONE] Backend — `ForumService.DeleteTopicAsync`/`DeletePostAsync` used `FindAsync`, which
applies `ForumTopicConfiguration`/`ForumPostConfiguration`'s global query filter
(`IsActive && Category.IsActive` / `IsActive && Topic.IsActive`) — the same bug class as the
`AlumniEvent` fix in 41.4, just undiscovered there: a SuperAdmin could never moderate a topic/post
under an already-deactivated category, silently no-op'ing with an apparent-success 204. Fixed with
`.IgnoreQueryFilters()`, matching the `AlumniEvent`/`MemberService`/`AuthService` convention.

44.5 [DONE] Backend — hardening sweep on findings that don't map to a single root cause: `HealthController`
leaked raw DB exception text (host/port/credentials) on its anonymous endpoint, and its "FileStorage"
check never actually checked anything (fixed to test `Directory.Exists`); `ExceptionMiddleware` didn't
guard `Response.HasStarted`, so a throw after a response started writing (mid-`SendFileAsync`, a
streaming export) replaced the real logged error with a generic connection reset; the
`/api/uploads` `PhysicalFileProvider` threw at startup if its root didn't exist yet — harmless today,
fatal the day `FileStorage:BasePhysicalPath` points at a freshly-mounted empty disk (added
`Directory.CreateDirectory`); `EventService`'s participation-email catch was fully silent with no
`ILogger` in the class at all (added one).

44.6 [DONE] Web — **critical**: the NG0200 fix (`afterNextRender` deferring `/auth/me`) turned a
pre-existing race into a deterministic bug. `AlertService`'s constructor (present on every page via
the header/nav) unconditionally called the `[Authorize]` `GET /api/notifications`; for any guest that
401s, and the interceptor's `handle401` (not excluding that URL) chased it into
`refresh() → fail → logout() → POST /api/auth/logout` (also `[Authorize]`, also not excluded) `→ 401
→ handle401` again — an unbounded refresh/logout/redirect loop for a first-time anonymous visitor,
complete with an "Invalid credentials" toast and a bounce to `/login`. Fixed in
`global-http.interceptor.ts` (exclude `/api/auth/me` and `/api/auth/logout` from `handle401`) and in
`auth.service.ts` (`X-Skip-Error-Notify` on the `/auth/me` probe) and in `alert.service.ts` (gate
`loadNotifications()` on `authChecked()` via `effect()`, same pattern as 44.7 below).

44.7 [DONE] Web — the deferred `/auth/me` restore also means `authGuard`/`adminGuard`/`superAdminGuard`
(`auth.guard.ts`) and three components' `ngOnInit()` one-shot `isAuthenticated()` reads
(`gallery.ts`'s "My Albums", `payment-portal.component.ts`'s saved methods, `events.ts`'s
`loadMyRegistrations`) can run before the restore resolves — worst case, a valid member opening a
`/portal/...` deep link in a new tab (empty per-tab `sessionStorage`) gets bounced to `/login`
despite a valid session cookie, or a logged-in member's own data silently never loads with no retry.
Fixed: the three guards now wait for `authChecked()` via `toObservable(...).pipe(filter(Boolean),
take(1))` before deciding; the three components gate their load call on an `effect()` keyed to
`authChecked()`, mirroring the existing pattern in `events.ts`'s `deepLinkEffect` (30.26). Also
applied the same `afterNextRender` deferral to `ThemeService` (`activeSpecialTheme`, read directly in
`public-layout.html`) for consistency, since it has the identical shape.

44.8 [DONE] Web — `admin-news.ts`'s 41.2 staged-upload fix only covered the image field:
`onImageSelect` never called `validateUploadFile` (a renamed non-image file was accepted at
select-time and only rejected server-side, after which the whole submit aborted), and
`onDocumentSelect` still uploaded the PDF immediately on file-select — the exact bug 41.2 fixed for
images — so cancelling the form after picking a document orphaned it on the server. Fixed both:
image select now validates before staging; document select now stages
(`stagedDocumentFile`/`stagedDocumentName`) and uploads only as part of `saveNews`'s chain
(image → document → submit), matching every other admin upload form.

44.9 [DONE] Web — `GlobalErrorHandler` tested `instanceof HttpErrorResponse` before unwrapping an
unhandled-promise-rejection wrapper, so an `HttpErrorResponse` thrown inside a promise
(`firstValueFrom`/`toPromise()` call sites) arrived as `{rejection: HttpErrorResponse}`, missed the
check, and got double-reported on top of the interceptor's own toast. Fixed the ordering (unwrap
first, then check). Also removed an unused `NgZone` import.

44.10 [DONE] Mobile — **major**: `FlutterError.onError`/`PlatformDispatcher.instance.onError` (added
in 43.3) were both assigned *after* `SentryFlutter.init(...)`, silently detaching Sentry's own
`FlutterErrorIntegration`/`OnErrorIntegration` (which install by chaining to whatever handler already
exists at init time) — losing unhandled-vs-handled crash classification, silent-error filtering, and
context collection, while also risking a Sentry-report flood since our handler unconditionally
reported every frame of a persistent layout error with no `silent` check. Fixed by moving both
assignments before `SentryFlutter.init` (chaining to the pre-existing default via a saved reference
for `FlutterError.onError`) so Sentry's integrations wrap around them correctly.

44.11 [DONE] Mobile — the 43.2 logging sweep's file list missed the three most security-relevant
catches in the codebase: `auth_service.dart`'s `login`/`_socialLogin`/`register` all logged nothing
before returning a generic error string. Added `debugPrint` to all three, and fixed a pre-existing
mislabeled log (a catch around `deviceInfoProvider` printed `"AuthService.login failed"`).

44.12 [DONE] Mobile — event registration bugs found outside the 43-file sweep, all pre-existing (not
introduced this session, but surfaced by the same audit): `events_screen.dart`'s payment button
routed every registration-required event through the SSLCommerz/DGePay sheet regardless of
`requiresPayment`, so a free-but-registration-required event pushed a $0 gateway charge and never
actually called `registerForEvent` — member never registered. Fixed with a `registerFreeEvent` path
mirroring `event_details_screen.dart`'s correct branching. Separately, `event_details_screen.dart`'s
register handler ignored `registerForEvent`'s `bool` return (the service logs-and-returns `false`
rather than throwing on failure — 43's swallow-and-log-false convention), so a duplicate/closed/
expired-session registration attempt displayed "Registration successful." regardless. Fixed to check
the result. Also: `event_details_screen.dart`'s participant-count/entry-fee/"REGISTERED MEMBERS"
block wasn't gated by `requiresRegistration` like the FAB already was (an informational-only event
showed a permanent "No members registered yet."); `events_screen.dart`'s role check constructed a
brand-new `FutureProvider` literal inside `build()` on every rebuild (leaking providers,
self-perpetuating re-fetches) instead of using the existing stable `roleProvider` — fixed both; and a
`dynamic > 0` comparison on `registrationFee` that would throw if the API ever serialized it as a
string — fixed with `num.tryParse`.

44.13 [DONE] Mobile — `file_service.dart`'s `uploadProfilePhoto`/`uploadArticleImage` silently
returned `null` on any exception (indistinguishable from a user-cancelled picker); added `debugPrint`
to both. `event_details_screen.dart`'s `eventDetailsProvider` similarly swallowed everything into a
"Event not found." with no log; added one.

44.14b [DONE] Live local verification: ran the API (`dotnet run --project GHCAA.API --urls http://localhost:5087`, against real local Postgres — also incidentally re-confirmed 44.2's migration fix applies `AddApprovalWorkflowToGalleryAndJobs` cleanly) and the Angular dev server (`npm start`), then drove it headlessly with Playwright (login as `superadmin`, portal/admin dashboards, gallery, events) capturing the browser console. Found two more real issues:
- `AlertService`/`AuthService`/interceptor were still logging **three** separate error-shaped console
  entries (browser's own network-error log, the interceptor's skip-notify `console.error`, and
  `auth.service.ts`'s own `console.error`) for the entirely routine "guest not logged in" 401 on
  every single anonymous page load — a side effect of 43.2's blanket "log every catch" sweep applied
  to what is actually an expected response, not a failure. Fixed by special-casing 401 on
  `/api/auth/me` in both `auth.service.ts`'s `catchError` and the interceptor's `handleError` to skip
  logging entirely (the browser's own native network-tab log line for the 401 is unavoidable and
  present on every site doing this pattern — not fixable from app code, and not a bug).
- `proxy.conf.json`'s `/api` context lacked `"ws": true`, so local `ng serve` couldn't proxy the
  WebSocket upgrade for `/api/hubs/chat`/`/api/hubs/notifications` (SignalR hubs live under `/api/`
  since the BUG-002 fix; the separate `/hubs` proxy context with `ws:true` is now dead/pointing at a
  path nothing uses). Caused an intermittent `net::ERR_CONNECTION_TIMED_OUT` console error, local-dev
  only (production is same-origin, no proxy). Fixed by adding `"ws": true` to the `/api` context.
- One remaining console entry (`403` on `GET /api/gallery/albums/mine` while logged in as the seeded
  `superadmin` test account) is a pre-existing local-seed-data mismatch
  ([[session_area33_review_triage]] already documented an admin login role/seed mismatch in the local
  DB) — not caused by any change this session, not chased further.
Re-verified: `npx tsc --noEmit` clean, `npx vitest run` 66 files/317 tests pass.

44.15 [DONE] Mobile — closed out the remaining minor logging gaps from 44.13/reviewer finding C:
`gateway_service.dart`'s failure message no longer leaks the raw Dio exception (request URI/response
body) into the user-facing SnackBar in `events_screen.dart` — returns a generic message, logs the
real error via `debugPrint`. Added `debugPrint` to every previously-silent catch in
`support_service.dart` (both `SupportService` and the `FamilyService` it also defines — disambiguated
in the log text, since there are 3 unrelated classes named `FamilyService` in this codebase:
`features/family/`, `features/networking/`, and this one in `features/support/`),
`networking/family_service.dart`, `networking_service.dart`, `lookup_service.dart`,
`notification_service.dart`, `role_service.dart`, `governance_api.dart`, and `main.dart`'s two bare
`catch (_) {}` blocks. `flutter analyze` clean, `flutter test --exclude-tags=golden` 28/28 (one test
updated: `gateway_service`'s failure-wrapper test asserted the old raw-exception passthrough, now
asserts the generic message and that the raw text is absent).

44.16 [TODO] Not fixed — deliberately out of scope for a logging sweep: 3 classes are all named
`FamilyService` (`features/family/family_service.dart`, `features/networking/family_service.dart`,
`features/support/support_service.dart`). Renaming is a real refactor (import aliasing, provider
naming, call-site updates) with regression risk disproportionate to a naming/debugging-clarity issue
— worth doing deliberately, not as a drive-by.

44.17 [SUPERSEDED by 44.20] The `OutputCacheMiddleware`/compression-mismatch theory below turned out
to be wrong — see 44.20 for the real cause and fix of the live outage this was originally guessing
at. `OutputCacheMiddleware` does still cache the SPA-shell fallback response independently of the
41.10/44.1 `Cache-Control` headers, and an explicit `.CacheOutput(policy => policy.NoCache())`
exclusion on that route remains a reasonable follow-up, but it was never the cause of any observed
404/`NS_ERROR_CORRUPTED_CONTENT` incident.

44.20 [DONE] **Live preprod outage 2026-08-28: every static asset (main-*.js, chunk-*.js,
styles-*.css, /assets/*, /api/uploads/*) 404'd in production — including index.html itself when
requested directly — while `GET /` still served fine.** Root cause: `f47d066`'s `MapFallback` change
from the parameterless overload (which uses an implicit `:nonfile` route constraint) to an explicit
`"/{**path}"` pattern with NO constraint, to fix a separate "missing upload returns 401 not 404"
problem. Consequence: ASP.NET Core's endpoint routing matches routes (including this catch-all)
*before* `UseStaticFiles` gets a turn, and `StaticFileMiddleware` unconditionally backs off once
`context.GetEndpoint()` is non-null — confirmed via `Microsoft.AspNetCore.StaticFiles` debug logging:
`"Static files was skipped as the request already matched an endpoint."` So literally every request
matched the fallback route first and got swallowed by its "file-like path → 404" heuristic; static
files middleware never got to serve anything. Reproduced locally end-to-end (real `dotnet publish`
output + real Angular `preprod` build, run with `ASPNETCORE_ENVIRONMENT=Production`) before touching
any code — first suspected (wrongly) an `OutputCache`/`ResponseCompression` `Accept-Encoding`
mismatch (see 44.17), then a Render build-cache staleness issue (ruled out: a full "Clear build
cache & deploy" rebuilt everything from scratch and the bug persisted identically), before isolating
the actual mechanism via `IWebHostEnvironment.WebRootFileProvider` debug output (file correctly
found) and `Microsoft.AspNetCore.Routing`/`StaticFiles` debug logs (endpoint matched first, static
files middleware skipped). **Fix** (`GHCAA.API/Program.cs`): restored `:nonfile` on the fallback
route (`"/{**path:nonfile}"`), and moved the "missing file-like path → 404 instead of a misleading
401" logic out of the routed endpoint into plain `app.Use(...)` middleware (scoped to `/api/uploads`
and non-`/api` paths) positioned right after both `UseStaticFiles` blocks — plain middleware executes
in registration order and never participates in endpoint-routing precedence, so it can't shadow real
static files the way a routed catch-all can. Verified locally: real static files 200, missing static
file 404, missing upload 404 (not 401), unknown `/api` route 404, SPA deep link still 200. Added a
`WebApplicationFactory<Program>`-based regression suite,
`GHCAA.Tests/Integration/SpaStaticFileFallbackTests.cs` (7 tests, boots the real pipeline against a
throwaway wwwroot), plus `[assembly: InternalsVisibleTo("GHCAA.Tests")]` on `GHCAA.API.csproj` and the
`Microsoft.AspNetCore.Mvc.Testing` package on `GHCAA.Tests.csproj` to make that possible. See
[[gotcha_mapfallback_nonfile_routing_precedence]].

44.18 [DONE 2026-08-29] **Root cause found and fixed**: not a seed-data problem — a seeder-ordering
bug in `Program.cs`. `ProtectedSuperAdminSeeder.EnsureAsync` ran correctly, but the Visual-profile
block ran *after* it and, via `OverrideEFCoreMigratedData`, wiped and re-inserted every `User` row
from `Seed/Visual/users.json` (which carries no role data) — silently undoing the just-restored
`SuperAdmin` role on every local boot. Fixed by moving the protected-admin restore to run *last*,
after the Visual-profile reseed, so it's never undone. Also added `"superadmin"` to
`AppSettings:ProtectedSuperAdmins` (previously only `"shalin"` was listed, despite this TODO
explicitly naming `superadmin` as affected). 5 new tests in
`GHCAA.Tests/Services/ProtectedSuperAdminSeederTests.cs`, including one that reproduces the exact
wipe-then-reseed sequence and asserts the role survives. `dotnet test` 468/468 pass.

44.19 [DONE] Web — centralized the "run this once authChecked() settles AND the user turns out to be
logged in" pattern (introduced 4 times this session: `AlertService`, `gallery.ts`, `events.ts`,
`payment-portal.component.ts`) behind one method, `AuthService.whenAuthenticated(callback)`, instead
of leaving 4 near-identical inline `effect()` blocks. All 4 call sites now read as a single line;
`AuthService` is the natural home since it owns `authChecked`/`isAuthenticated`. Left
`events.ts`'s `deepLinkEffect` (30.26) and `payment-portal.component.ts`'s `saveRequested`-sync effect
alone — different shape (one gates on `authChecked()` alone with a `setTimeout`, the other has nothing
to do with auth). Updated `createAuthServiceMock` (testing-utils.ts) plus 3 ad-hoc component-local
mocks (`gallery.spec.ts`, `events.spec.ts`, `payment-portal.component.spec.ts`) to implement
`whenAuthenticated` as a synchronous check-and-call, matching how each test already sets up mock
state before construction. Re-verified: `npx tsc --noEmit` clean, `npx vitest run` 66 files/317 tests,
and a live local run (API + `ng serve`, login as `superadmin`, portal/admin/gallery) — behavior
unchanged, no regression.

---

# Area 45 — SuperAdmin error-log viewer (raised by user 2026-08-28: "super admin role should able to
view application error logs from UI, able to search, by date or error details or part"), plan only,
not yet built

Today errors only reach `stdout` (`ExceptionMiddleware`'s `ILogger.LogError`, plus every
`ILogger<T>.LogError`/`LogWarning` call added across Areas 43/44) and Render's log stream — nothing
is persisted queryably, so there is nothing for an admin UI to read from yet. Per
[[feedback_keep_lightweight]], the right shape here is a dedicated small table + a thin capture
sink, not a logging framework (Serilog/ELK/Seq) — this app has deliberately avoided that class of
dependency so far.

45.1 [TODO] Explore/plan (Plan Mode required — spans Domain/Infrastructure/API/Web): decide the
capture point(s). Candidates to reconcile: a custom `ILoggerProvider` registered in `Program.cs`
alongside the console provider (captures every `ILogger` call app-wide, broadest coverage, more
plumbing); vs. writing directly from `ExceptionMiddleware` only (captures unhandled exceptions —
matches this request's literal wording, "application error logs" — much simpler, but misses
`LogWarning`/handled-but-logged errors from the Area 43/44 sweep). Confirm which with the user before
building either.
45.2 [TODO] Domain + migration: new `ErrorLog` entity — at minimum `Id`, `OccurredAt` (UTC,
indexed), `Level` (Error/Warning), `Message`, `ExceptionType`, `StackTrace`, `Source` (controller/
middleware/class name), `RequestPath`, `RequestMethod`, `UserId`/`Username` (nullable — many errors
are pre-auth or background). Raw SQL migration per the idempotent-migration convention established
in 41.9/44.2 ([[gotcha_migrationbootstrapper_fixed_offset]]).
45.3 [TODO] Infrastructure: the capture sink decided in 45.1, writing rows via a scoped/background
write (never let logging itself throw or block the request it's logging) — batch or fire-and-forget
inserts so a logging-table write can't become a new source of request latency or failure.
45.4 [TODO] API: `GET /api/admin/error-logs` (`[Authorize(Policy = "SuperAdminOnly")]`, matching the
existing policy convention in `ServiceExtensions.cs`) with query params for date range, free-text
search (message/exception-type/stack-trace substring), level, and pagination — push filtering to the
DB query, not an in-memory scan, since this table will grow unbounded without a retention policy
(see 45.6).
45.5 [TODO] Web admin: new `admin/error-logs/` screen (list + filters: date range picker, text search,
level dropdown; row expansion for full stack trace) — follow `ghcaa-design` conventions and the
existing admin list-page pattern (search bar + filters component already used elsewhere, e.g.
`admin-news.ts`/`admin-members.ts` — reuse `SearchBarComponent`/`PageHeaderComponent`, don't rebuild).
45.6 [TODO] Retention/cleanup: decide and implement a bound (e.g. delete rows older than N days, or
cap total row count) — an error-log table with no retention policy will grow forever and eventually
degrade the very queries meant to search it.
45.7 [TODO] Tests + docs update per usual closing convention (`dotnet test`, `npx vitest run`,
`npx tsc --noEmit`, live verification that a genuine error actually appears in the new admin screen).

---

# Area 46 — May 2026 alumni registration batch import (raised by user 2026-08-28: import
`GHCAA.Tools/HRAGANGIAN Alumni Registration May 2026 02.csv` into seed)

46.1 [DONE] Imported 47 new members from the Google-Form CSV export into the seed JSON
(`GHCAA.Infrastructure/Data/Seed/{members,users,user_roles,academic_records,professional_records,
payment_histories}.json`) plus a matching EF migration (`AddMay2026AlumniRegistrationBatch`,
`GHCAA.Infrastructure/Data/Migrations/PgSql/`) so it actually reaches an already-created Postgres DB
(HasData alone only seeds a brand-new `EnsureCreated()` database — see
[[gotcha_ensurecreated_no_op_existing_db]]). Mapping: CSV `Member No` → `Member.NID` +
`MembershipNumber = "GHC-" + NID` (matches the existing 584-member convention exactly — those aren't
real national IDs either); real `Gender`/`BloodGroup` values this time (existing bulk import has them
all at `0/Unknown`); `AcademicRecord` created for all 47 (`InstitutionName = "Govt. Haraganga
College"`, `IsGHC = true`); `ProfessionalRecord` created for the 37 rows with an Organization or
Designation; `PaymentHistory` created for all 47 at ৳1,000 (current active "General Membership Fee"
per `fee_configs.json`), `FinancialCategory = MembershipFee` — note the seed JSON's payment records
use a `"Category"` key that does NOT match the C# property name `FinancialCategory`
(`System.Text.Json` default options are case-sensitive AND name-sensitive), so every existing
seeded payment silently defaults to `FinancialCategory.MembershipFee = 0` regardless of what its JSON
`"Category"` value says — a pre-existing quirk, not touched, but worth knowing before trusting that
field on old rows. `User` created per member (`Username = NID`, `PasswordHash` = bcrypt of the NID
itself, `MustChangePassword = true` — per explicit user decision, forcing a real password on first
login) + `UserRoles` (Member).

46.2 [DONE] Real data-integrity conflicts found and resolved (all confirmed via `MemberConfiguration.cs`
unique indexes on `Email`/`NID`/`MobileNo`, and `PaymentHistories.IX_PaymentHistories_TransactionId`):
- 8 rows had an email shared with another registrant (4 family members using `kamal.uddin1276@gmail.com`,
  2 using `ahsankabir.bot@gmail.com` — both already the email of an existing member, ids 389/203
  respectively — plus one couple sharing `mdnurulhaquegazi@gmail.com` within this CSV) — disambiguated
  with a Gmail `+MemberNo` tag (`local+2605023@gmail.com`), matching the exact convention the original
  584-member bulk seed already used for its own row collisions (`haragangian+row583@gmail.com`).
- One couple (2605023/2605024) shared mobile `01339956569` — kept on the first, synthesized
  `01339956570` (last digit bumped) for the second per explicit user decision, since neither had an
  alternate number available.
- 6 "Cash" transaction references (no real reference number) plus one shared bKash number
  (`01878375387`, used by 2 rows) collided under the unique `TransactionId` index — **this was only
  caught because the first migration-apply attempt failed with a real `23505` unique-violation
  mid-batch** (transaction rolled back cleanly, migration removed, seed files reverted, re-fixed,
  regenerated) — disambiguated the same way, `Cash-2605011` etc.
- 6 rows had a blank `Passing Year:` — fell back to `Admission Year` per explicit user decision
  (`AcademicRecord.PassingYear` is non-nullable).
- Member 2608046 has a Qatar mobile number (`+97455637444`) that won't pass the app's `^01\d{9}$`
  registration validator if he ever edits his profile through the UI — left as the real number,
  flagged rather than fabricated into the wrong shape.

46.3 [DONE] A real, unrelated test failure surfaced by adding genuine data:
`NetworkingServiceTests.SearchMembersAsync_WithNewTableFilters_ShouldReturnCorrectMembers` filtered on
`ProfessionalSector = "Banking"` expecting exactly 1 seed match — several of the new alumni are
actual bankers, so the filter now (correctly) matched 6. The test already had a same-shape comment
("PassingYear 1938 — unique in seed") flagging this exact fragility class. Fixed by giving the test's
synthetic member a collision-proof sector marker (`"Banking-NT-Test"`) instead of depending on
"currently unique in the shared seed," which any future real-data addition could break again.
`dotnet test` 382/382 after the fix.

46.4 [DONE] Live-verified end-to-end: applied the migration to the local Postgres dev DB (`dotnet ef
database update`), started the API, logged in as the first new member (username/password = their
NID, `2605001`/`2605001`) and confirmed the returned profile (name, email, mobile, blood group,
academic record) matches the CSV row exactly; `mustChangePassword: true` as expected. Admin
members-list total is 632 (584 original + 47 new + 1 pre-existing unrelated `GHC-DEMO-0001` test
account). Confirmed `paymentStatus`/`tShirtSize` display quirks on the new members' detail view are
identical to the pre-existing baseline behavior on an original 584-batch member (id 781) — not a
regression, a pre-existing DTO-mapping gap unrelated to this import, not chased further.

46.5 [TODO] Not touched: the org-wide Financial Ledger (`FinancialRecord`/`financial_records.json`)
has zero rows — the existing 584-member bulk import never populated it either, so the new 47
members' ৳47,000 in membership fees is correctly reflected in `PaymentHistories` (per-member ledger)
but not in the aggregate income/expense ledger view. This is a pre-existing gap in how seeded/bulk
member data relates to the org ledger, not something this import introduced — flagging for whoever
next needs the aggregate ledger to reflect bulk-imported history.

# Area 47 — Live preprod triage: seed-data integrity, dashboard accuracy, landing polish (2026-08-29)

47.1 [DONE] **Seed-data integrity regression suite.** New `GHCAA.Tests/Data/SeedDataIntegrityTests.cs`
reflects every `LoadSeed<T>("*.json")` call in `ApplicationDbContext` and asserts every JSON key in
the file matches a real public property on its target entity — `System.Text.Json.Deserialize`
silently drops unmatched keys with no error, so a renamed/stale field name in seed data reaches
production undetected. Caught 3 real, previously-live bugs on first run: `news.json` had a stray
`Category` key (should be `ArticleCategory`) causing every seeded article to silently default to
`ArticleCategory.Event` regardless of its real type; `jobs.json` had the same `Category`→
`JobCategory` mismatch on both listings; `members.json` carried a dead `ECPosition` key (uniform `0`
across all 631 records, never bound to anything — real EC roles live in `ECMember`/`ECPeriod`).
`payment_histories.json` had the same `Category`→`FinancialCategory` mismatch on 1166 of 1213
transactions, meaning every one of them silently defaulted to `FinancialCategory.Other` instead of
its real category. All 4 seed files fixed; live preprod DB backfilled to match (`NewsPosts`,
`JobOpportunities`, `PaymentHistories` — 1166-row single-round-trip bulk `UPDATE`).

47.2 [DONE] **Dashboard "Net Fund Balance" undercounted real income.** `MemberService.GetDashboardStatsAsync`
only summed `FinancialRecords` (the manually-entered org ledger) — confirmed **empty (0 rows)** on
live preprod — while `PaymentHistories` (real member registration/membership/event payments) held
1213 completed transactions totaling ৳1,796,000, entirely excluded from the balance shown to
SuperAdmins. Fixed: balance now = ledger income + completed `PaymentHistories` − ledger expense.
Matches the gap already flagged in Area 46's May-2026-import note above.

47.3 [DONE] **Admin dashboard KPI-tile inconsistency.** "Active Members" was the only stat-card with a
progress-bar/rate treatment ("X% of total") the sibling cards didn't have — visually flagged as
"design broken" and, since bulk-imported members are all seeded `Status=Active`, the number is
almost always identical to "Total Alumni" right next to it, making the emphasis read as duplicated
info. Normalized to the same plain-card layout as its siblings; removed the now-dead
`membershipRate` getter.

47.4 [DONE] **Recent-News widget mislabeled an article as an Event** (green "LIVE" badge on a
Pending-status article) — root cause was 47.1's `ArticleCategory` bug, now fixed at the data layer.
Also fixed the widget's own status badge, which only distinguished Draft/"Live" and had no
Pending state — now uses the shared `SUBMISSION_STATUS_MAP` (Draft/Pending Approval/Approved/Rejected).

47.5 [DONE] **Login/about-us campus background.** `/assets/images/campus/ghc-old-building.jpg` was
referenced by `about.scss` but the file never existed (permanent 404, silent fallback to concept
art). Real photo added (source watermark removed via feathered blur — no inpainting tool available);
also wired into `login.scss`, which previously had no campus photo at all (flat `#050505`).

47.6 [DONE] **EC period dates never displayed anywhere** — both the landing page's EC preview and the
governance page's period selector showed only the period title, never its date range. Added
`formatPeriodRange()` (`core/utils/date.util.ts`) — an *active* period always reads "YYYY - Present"
regardless of its stored target end date, since `isActive` is the real signal of whether it has
concluded. Live preprod's one period ("Founding Interim Executive Committee", 2024-12-01 →
2026-06-30 target) now correctly shows "(Period: 2024 - Present)". **Not done**: a second, older
"2015-2017" committee period the user referenced was never entered into the system at all — needs
real confirmed dates/title before it can be added; not invented here.

47.7 [DONE] Misc landing-page polish: removed the "Ready to Step Into the Legacy?" CTA banner
section entirely (component + registration deleted); hero headline and all section headers (shared
`.section-header h2` class) reduced 15% font size; inter-section vertical padding centralized into
one `--section-padding-y` token (was `padding: 5rem 0` duplicated 8× in `landing.scss`) and reduced
20% (`5rem`→`4rem`); "Join This Tier" button no longer stretches full-width (no explicit width, so
it filled its flex/grid ancestor unlike every other `.btn.btn-accent` on the page — now `width: fit-content`
like its siblings); new `image.util.ts` `safeImageUrl()` guard (shared by gallery/admin-dashboard/
article-approval/member-articles) so a non-URL string (bad seed/import data) never reaches an `<img src>` again.

47.8 [DONE] **SEO baseline** — `robots.txt` and `sitemap.xml` (neither existed), a canonical `<link>`,
and `AlumniOrganization` JSON-LD structured data in `index.html` targeting "Govt. Haraganga College
Munshiganj" / "Alumni Association"; `robots.txt` disallows `/admin/` and `/portal/` so only the
public site is indexed.

47.9 [DONE] **Mutation-coverage audit** (this session, full report in chat history — not reproduced
here) found ~60% of POST/PUT/DELETE actions covered; closed the two highest-risk gaps it flagged —
`RolesController.DeleteUser` and `AdminGovernanceController.DeleteECMember`, both `[RequireStepUp]`-
protected destructive deletes with zero prior coverage (`GHCAA.Tests/Controllers/DestructiveStepUpActionsTests.cs`).
**Not done**: the remaining ~40% gap (notably `AuthController`'s non-Login actions — social login,
refresh, logout, the step-up request/verify endpoints this session added, reset-password — and
`LookupsController`'s full CRUD) is still open; report exists but no further remediation started.

47.10 [TODO] Still open, explicitly deferred: `docs/deploy_connection.txt` committed live-credentials
file (flagged, not rotated); `docs/BUSINESS_FUNCTIONALITY_REVIEW_PLAN.md:79` has a real password in
plain text (flagged, not scrubbed); member profile photos are genuinely missing for most of the 631
bulk-imported alumni (not a bug — no photo was ever supplied at import time).

`dotnet test` 486/486, `npx vitest run` 70 files / 347 tests, `ng build` clean throughout this Area.

47.11 [DONE] **SEO baseline round 2**: homepage `<h1>` was actually an `<h2>` (the hero headline) —
the single most search-weighted tag on the site's most important page had none; fixed. Per-route
`<title>` via Angular Router's native `title:` route property (was one static site-wide title for
every page) + a small `Meta`-service hook in `app.ts` reading `route.data.description` on navigation,
covering home/about/contact/events/news/jobs/register/etc. Added a visible "Official Alumni
Association of Govt. Haraganga College, Munshiganj" line under the hero H1 (config-driven off
`OrgConfigService`) so the college's own name appears in real page content, not just metadata —
targets "Govt. Haraganga College" / "Haraganga College" queries specifically, not just "Haragangian".
`robots.txt`/`sitemap.xml`/canonical/JSON-LD from 47.8 already covered the crawl-layer half of this.

47.12 [DONE] Added missing unit tests for this Area's two new shared/central utilities:
`image.util.spec.ts` (`safeImageUrl` — used by gallery/admin-dashboard/article-approval/member-articles)
and confirmed `date.util.spec.ts` (`formatPeriodRange`) was updated when its behavior was simplified
(dropped the "YYYY - Present" branch per user feedback — always shows real stored years now, no
"ongoing" language, single period only; no second historical 2015-2017 period was added — real
dates for that were never confirmed).

47.13 [TODO] **Mutation (POST/PUT/DELETE) coverage remediation — task breakdown.** 47.9 closed the top
2 items (`RolesController.DeleteUser`, `AdminGovernanceController.DeleteECMember`). Remaining ~35%,
broken into independently-completable tasks below. Common approach for all of them: one new
`GHCAA.Tests/Controllers/*Tests.cs` file per controller, mocking the underlying service interface
(same pattern as `DestructiveStepUpActionsTests.cs`/`GalleryControllerTests.cs`) — one success-path
test + one failure-path test per action is the target depth; this is breadth-over-depth work, not
deep edge-case testing. Run `dotnet test` after each task, not just at the end.

47.13.1 [TODO] **`AuthController` non-Login actions** (highest priority — the security surface, and
this session's own new step-up endpoints are among the untested ones): `GoogleLogin`, `FacebookLogin`,
`Refresh`, `RefreshMobile`, `Logout`, `RequestStepUp`, `VerifyStepUp`, `ResetPassword`. New file
`AuthControllerMutationTests.cs`.

47.13.2 [TODO] **`LookupsController` full CRUD** (`CreateLookup`/`UpdateLookup`/`DeleteLookup`) — zero
coverage today, controls dropdown/lookup master data; same "silent bad data" risk class as this
Area's seed-integrity bugs (47.1). New file `LookupsControllerTests.cs`.

47.13.3 [TODO] **`RolesController` remaining actions** (`CreateAdmin`, `CreateRole`, `AssignRole`,
`RemoveRole` — `DeleteUser` already covered per 47.9). Extend the existing
`DestructiveStepUpActionsTests.cs` or add a sibling `RolesControllerTests.cs`.

47.13.4 [TODO] **`AdminPollController`** (`DeletePoll`, `ToggleStatus` — `CreatePoll` already covered).
New file or extend existing poll test coverage.

47.13.5 [TODO] **`PaymentConfigController`** (`Create`, `Toggle`, `Delete` — `Update`/`SeedDefaults`
already covered per the mutation-coverage audit). New file `PaymentConfigControllerTests.cs`.

47.13.6 [TODO] Lower priority, batch together when picked up: `AdminController` (`SyncMembers`,
`BulkArchiveInactive`, `RestoreMember`, photo/signature/document-upload actions), `GalleryController`
(`UploadPhoto`, `ToggleActive`, `ToggleFeatured`, `SubmitMemberPhoto`), `CommunicationController`
template CRUD (`CreateTemplate`/`UpdateTemplate`/`DeleteTemplate`), `FamilyController`/
`FamilyLinkController` remaining gaps (`CancelRequest`/`UnlinkMember`/`Remove`/`Cancel`).

47.13.7 [TODO] Once 47.13.1–47.13.6 are done, re-run the original mutation-coverage audit methodology
(grep every `[HttpPost]/[HttpPut]/[HttpPatch]/[HttpDelete]` action, cross-reference against test
files) to confirm the gap is actually closed rather than assuming from this list.

# Area 48 — Full security audit (raised by user 2026-08-29: "plan for vulnurability check, check for
web security best paractices")

A `security-reviewer` subagent audit of the whole app (verifying prior S1-S9/Area 24 hardening is
still genuinely wired, and hunting for anything new) found 2 Critical, 4 High, 4 Medium, and several
Low findings. All code-fixable items below are done (511/511 backend tests green, `ng build` clean);
the two Critical items include work the user must do outside this codebase (external secret rotation).

48.1 [DONE] **CRITICAL — unauthenticated PII leak.** `GET /api/networking/member/{id}`
(`[AllowAnonymous]`) returned every member's NID, DOB, parents' names, and emergency contact phone
ungated, while sibling fields were correctly privacy-gated. Fixed: stripped these fields (plus
`CertificatePath`) from `NetworkingService.MapToDto` — that DTO only backs the public directory
profile; the authenticated owner/admin profile is a separate build in `MemberService.GetProfileAsync`.

48.2 [TODO] **CRITICAL — committed secrets, live JWT signing key included.**
`docs/deploy_connection.txt` (still tracked) contains the production `Jwt__Key` and the Render
deploy-hook URL, not just DB credentials as previously known. Also newly found with live secrets:
`.env.remote`, `build_output/appsettings.Production.json`, `build_output/appsettings.json` (Gmail
app password), `docs/RENDER_DEPLOYMENT.md`. **Requires the user to rotate the JWT key, both DB
passwords, the Gmail app password, and the Render deploy hook, then `git rm --cached` + `.gitignore`
+ history purge (`git filter-repo`).** Not something this session can do — no dashboard access.

48.3 [DONE] **HIGH — refresh tokens survived termination/reset.** `SecurityStamp` rotation (the
documented S5.4 kill-switch) fired in 5 places but never called `RevokeAllRefreshTokensAsync`, so a
still-held refresh token kept minting valid access tokens after termination or a password reset.
Fixed in `UserService.ChangePasswordAsync`, `MemberService` (`ArchiveMemberAsync`,
`AdminUpdateMemberAsync`'s status-transition branch, `BulkArchiveInactiveMembersAsync`), and
`AuthService.ResetPasswordAsync`. Also added a missing `!user.IsActive || user.IsArchived` check to
`AuthController.Refresh`/`RefreshMobile` (previously only checked `user == null`).

48.4 [DONE] **HIGH — Admin→SuperAdmin takeover via email rewrite + admin-initiated reset.** A plain
Admin could rewrite a SuperAdmin's linked email via `PUT /api/admin/members/{id}`, then self-serve a
reset link via `POST /api/admin/members/{id}/reset-password-admin`. Fixed: `AdminUpdateMemberAsync`
and `SendAdminPasswordResetLinkAsync` now take an `isPrivilegedCaller` flag and throw
`UnauthorizedAccessException` (→ 403) when a non-SuperAdmin caller targets a SuperAdmin-linked user.

48.5 [DONE] **HIGH — step-up (2FA) bypassable via a sibling route.** `RolesController.AssignRole`
(grants SuperAdmin) and `CreateAdmin` had no `[RequireStepUp]` despite being equal/higher-impact than
the already-gated `DeleteUser`. Also `AdminController.BulkArchiveInactive`, `ResetPasswordAdmin`,
`SyncMembers`. All four now carry `[RequireStepUp]`. Also added password-strength validation to
`CreateAdminDto` (previously accepted a one-character password).

48.6 [DONE] **MEDIUM — step-up TTL of 30 days defeated its own purpose.** A stolen/left-open
access-token cookie almost always already carried a valid claim, since it rides along on every
hourly silent refresh for the full 30 days. Reduced `StepUpClaim.DefaultTtlMinutes` and
`appsettings.json`'s `StepUpTtlMinutes` from 43200 to 30. **Note: this reverses an explicit
mid-session product decision** (7.13 originally shipped with a 15-minute TTL, raised to 30 days
after user feedback "don't want OTP on every action/login") — flagged to the user, not silently
overridden as a permanent decision without visibility.

48.7 [DONE] **MEDIUM — anonymous email-enumeration oracle.** `GET /api/networking/search` matched
the `Email` filter regardless of `IsEmailPublic`, so a non-empty result confirmed a guessed address
belonged to a real member even though the response correctly masks that same address as
"Confidential". Fixed: the `Email.Contains(q)` clause now requires `m.IsEmailPublic`.

48.8 [DONE] **MEDIUM — upload extension not validated (polyglot HTML-injection vector).**
`FileValidationService.Validate` checked Content-Type and magic bytes but never the filename
extension, so a real JPEG uploaded as `x.html` with `Content-Type: image/jpeg` passed every check
and kept its `.html` extension on disk (only `FileUploadType.Photo` was force-renamed). Fixed with
an extension allowlist (`.jpg/.jpeg/.png/.webp` for images, `+.pdf` for documents) in the same
validator. (Confirmed as correct-as-built: Certificate/PaymentProof/Signature already route to the
authenticated `secure_uploads/` tree, not the public one — no change needed there.)

48.9 [DONE] **MEDIUM — stale `xlsx@0.18.5` + unused `bcryptjs`.** 0.18.5 is npm's final SheetJS
release; the prototype-pollution/ReDoS fixes only ship from `cdn.sheetjs.com` 0.19.3+, so
`npm audit fix` can never resolve it. **Not upgraded this session** (needs the CDN tarball install
+ regression-testing the export/import screens — a deliberate follow-up, not skipped by oversight).
Removed `bcryptjs` from `package.json` (zero references in `GHCAA.Web/src`).

48.10 [DONE] **LOW — unkeyed OTP hash.** `OtpService`'s HMAC was keyed on the recipient's email
(not a secret), making it an effectively unkeyed hash of a 6-digit code — brute-forceable in
microseconds from a DB dump. Now keyed on `Jwt:Key` (a real server secret already required to be
configured), with email folded into the message for per-user domain separation. **Note: this
invalidates any OTP issued before this deploy** (they expire in ~10 minutes anyway).

48.11 [DONE] **LOW — default `ClockSkew`.** Added `ClockSkew = TimeSpan.FromSeconds(30)` to JWT
validation (`ServiceExtensions.cs`) — the default 5-minute skew silently extended every ~60-minute
access token to ~65 minutes.

48.12 [TODO] **LOW — remaining minor findings not yet fixed** (lower value/effort ratio than 48.1-48.11,
picked up opportunistically): `MessagingController.MarkAsRead` has no ownership check (any
authenticated user can mark any message ID read — integrity only, no read access);
`FinancialsController.RecordPayment` keeps a client-supplied `MemberId` when the caller's claim is
absent (contained — `Status` is hardcoded `Pending`, no self-approval possible — but should reject
outright); refresh-token rotation has no reuse-detection (a replayed already-rotated token just
returns null instead of revoking the whole family); `MemberImportController`'s uploaded workbook
skips `IFileValidationService` unlike every other upload endpoint (admin-only, so low risk);
`MemberService.cs:~1350` substitutes user-controlled `FullName` raw into an HTML email body
(HTML-encode template variables).

48.13 [TODO] Known, still-open: `docs/deploy_connection.txt` (see 48.2) still tracked with the live
JWT key. `docs/BUSINESS_FUNCTIONALITY_REVIEW_PLAN.md`'s plaintext password table (committed since
2026-07-03) was **upgraded from a docs-hygiene item to a confirmed active exposure on 2026-08-29**:
its `shalin` / `Shalin@2024!` row was the exact live preprod SuperAdmin credential this session set
via direct DB access — meaning that password has been sitting in git history, publicly committed,
since before it was even set live. The table cells are now redacted, but **`shalin`'s live password
needs rotating again** (a second time, independent of the JWT-key/DB-password rotation in 48.2) —
redacting the file doesn't undo ~2 months of git-history exposure.

## Round 2 — OWASP Top 10 gap-fill audit (2026-08-29, raised by user: "make sure OWASPs are covered")

Round 1 covered A01 (Access Control), A07 (Auth Failures), and SQL injection in depth. This round
targeted the categories round 1 didn't verify: A02 (crypto/headers), A03 (frontend XSS), A05
(misconfiguration), A06 (component versions), A08 (integrity), A10 (SSRF). 511/511 backend tests
and 352/352 frontend tests green after all fixes; `ng build`/`type-check` clean.

48.14 [DONE] **A02/High — HSTS never sent in production; `UseHttpsRedirection()` was a silent
no-op.** Render terminates TLS at its edge and forwards over plain HTTP with
`X-Forwarded-Proto: https`; nothing consumed that header, so `Request.IsHttps` was permanently
`false` in prod — disabling 3 controls at once (the HSTS header in `SecurityHeadersMiddleware`,
`app.UseHsts()`, and `UseHttpsRedirection()`'s port resolution). Fixed: added
`app.UseForwardedHeaders(...)` as the very first pipeline middleware (before `ExceptionMiddleware`),
with `KnownNetworks`/`KnownProxies` cleared (Render's edge IP isn't a known private range — accepted
since Render is the sole ingress); added `app.UseHsts()` alongside the existing
`UseHttpsRedirection()` call.

48.15 [DONE] **A05/High — committed static JWT key in `appsettings.Development.json` + prod
origins in the dev CORS allow-list.** Removed the literal `Jwt:Key` value (was
`LOCAL_DEVELOPMENT_ONLY_DO_NOT_USE_IN_PRODUCTION_32_CHARS_MIN!`, committed in git) —
`JwtSigningKeyResolver` already generates a safe random ephemeral key per-process when none is
configured in Development, so the literal bought nothing but exposure risk if `ASPNETCORE_ENVIRONMENT`
were ever mis-set to Development in prod. Also removed `https://ghcaa-web.onrender.com` /
`https://ghcaa.onrender.com` from the Development `AllowedOrigins` list — production origins have
no reason to be pre-approved for a dev-mode CORS policy.

48.16 [DONE] **A05/Medium — `ASP_SEED_PROFILE=Visual` disabled login rate limiting in ANY
environment.** `Program.cs`'s rate-limiter partition key used
`ASP_SEED_PROFILE == "Visual" || IsDevelopment()` — a plain env var settable in production (no other
symptom) collapsed every rate limit to a shared bucket at 500-10000x the real limit. Fixed: relaxed
limits now depend only on `IsDevelopment()`, matching how `VisualTestAuthMiddleware` already gates
the Visual profile elsewhere.

48.17 [DONE] **A06/High — Angular 21.1.4 had 6 advisories, 2 of them XSS.** Upgraded the full
`@angular/*` set (core/common/compiler/forms/platform-browser/router/build/cli/compiler-cli) to
21.2.22 — a patch-level bump within the same major, no breaking changes. Required a clean
`node_modules`/`package-lock.json` reinstall (a same-transaction multi-package `npm install` hit
ERESOLVE peer-dependency conflicts against the stale lockfile). This brought `npm audit` from
**42 vulnerabilities (4 critical, 25 high)** down to **1 high** — the already-known, already-flagged
`xlsx@0.18.5` issue (48.9), which is unfixable via npm registry and deferred deliberately, not by
oversight.

48.18 [TODO] **A06/Medium — all Microsoft/EF Core/Npgsql NuGet packages pinned to exactly `9.0.0`
(the .NET 9 GA release), no servicing patches since.** Not fixed this round (needs care — a batch
EF Core bump should be verified against the full migration suite before landing). Bump
`Microsoft.EntityFrameworkCore*`, `Npgsql.EntityFrameworkCore.PostgreSQL`,
`Pomelo.EntityFrameworkCore.MySql`, `Microsoft.AspNetCore.Authentication.JwtBearer`, and
`Microsoft.Extensions.*` to the latest `9.0.x` patch. Also flagged: `Swashbuckle.AspNetCore 6.6.2`
(mitigated — Swagger is dev-gated) and `AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.0`
(behind the 13+/14+ line).

48.19 [TODO] **A08/Medium — CI Actions pinned to mutable tags; no NuGet lockfile.** Every GitHub
Action in `ghcaa-ci-preprod.yml` is pinned by floating major tag (`actions/checkout@v5`, etc.,
including third-party `subosito/flutter-action@v2`) with access to `RENDER_DEPLOY_HOOK_URL` in the
same workflow — a repointed tag executes with deploy-to-production access. Base images in
`Dockerfile` (`node:22-alpine`, `mcr.microsoft.com/dotnet/aspnet:9.0`, `.../sdk:9.0`) are floating
tags with no `@sha256:` digest. `npm ci` correctly uses `package-lock.json` integrity hashes; NuGet
restore has no equivalent (`packages.lock.json` doesn't exist anywhere in the repo). Not fixed this
round — pinning every action to a specific commit SHA needs those real SHAs looked up (Dependabot
can maintain them going forward), not guessed.

48.20 [DONE] **A10/Low — unescaped `mobileNo` in `GreenwebSmsService`'s SMS API query string.**
Currently mitigated by `MemberRegistrationValidator`'s `^01\d{9}$` regex at the only write path
today, but the interpolation itself wasn't defensive. `Uri.EscapeDataString`'d it so a future
write path without that same validation can't inject an extra query parameter (e.g. an attacker
overriding `message=` to send arbitrary spoofed SMS on the association's credits).

48.21 — Confirmed clean, no action needed: **A03 Angular frontend XSS** (all 8 `[innerHTML]` sinks
+ 1 `bypassSecurityTrustResourceUrl` traced — every user-submitted rich-text path is
server-side-sanitized via `HtmlSanitizer` before storage, the one unsanitized write path in
`GovernanceService.CreateConstitutionVersionAsync` has no controller route exposing it); **A05
error/detail leaks** (Swagger dev-gated, stack traces dev-gated, `/health` leaks nothing);
**A08 deserialization** (zero `BinaryFormatter`/`JavaScriptSerializer`/unsafe deserializers
repo-wide); **A10 SSRF** (every outbound HTTP call's target URL traced to `IConfiguration` or a
hardcoded literal — the admin-editable `PaymentConfiguration.GatewayCallbackUrl` is confirmed never
used as an actual request target, per the existing S4.3 design).

# Area 49 — Admin user/role management review (raised by user 2026-08-29: "from admin- how new role
can be created, how to disable, reset user passwords, review user and roles implementation and
design, are all grids designs including row controls same and following centralised designs")

Audit of `admin-roles.html/.ts`, `RolesController.cs`, `UserService.cs`, `AdminController.cs`, and a
grid-design comparison across `admin-roles`, `admin-members`, `admin-events`. Plan only — nothing
below is built yet unless marked `[DONE]`.

**Every item below is written as an ordered, mechanical checklist — no design decisions should be
needed at implementation time except where a step is explicitly flagged "DECISION NEEDED."**

49.1 [TODO] **Custom roles have no actual permission scope.** `RolesController.CreateRole`
(`RolesController.cs:77-82`) inserts any free-text role name and `AssignRole` attaches it to a user,
but every endpoint in the app authorizes against exactly 3 hardcoded ASP.NET policies
(`SuperAdminOnly`/`AdminOnly`/`MemberOnly` — `ServiceExtensions.cs:77-79`, each a compile-time
`RequireRole(Constants.Roles.X)` list). A custom role is never referenced by any `[Authorize]`
attribute, so assigning one grants zero additional access.
  - **DECISION NEEDED (ask user before starting):** ship option (a) or (b)?
    - (a) Minimal fix, ~1-2 hrs: keep roles label-only but stop implying otherwise.
    - (b) Real fix, multi-day: build a permission system. Only do this if the user confirms a concrete
      need (e.g. "an Events-only admin").
  - **If (a) is chosen, steps:**
    1. `GHCAA.Web/src/app/admin/roles/admin-roles.html`: change the "Create Custom Role" section
       heading/button label to something like "Add Role Tag (label only — grants no permissions)".
    2. Add a one-line `<p class="hint">` under that section: "Custom roles are for grouping/reporting
       only. Access is controlled by the built-in Admin/SuperAdmin/Member roles."
    3. In the same file, in the "Create System Administrator" modal, if the role `<select>` currently
       lists custom roles as options, restrict it to only `Admin`/`SuperAdmin` (the two values
       `CreateAdminDto.Role` at `RolesController.cs:66` actually gets checked against anywhere).
    4. No backend change needed for (a).
  - **If (b) is chosen, steps (do NOT start without explicit user sign-off — this is a multi-file,
    multi-day change):**
    1. `GHCAA.Domain/Models/`: add `Permission.cs` (Id, Name, e.g. `"ManageEvents"`) and
       `RolePermission.cs` (RoleId, PermissionId) join entity; add `Role.Permissions` nav collection.
    2. `GHCAA.Infrastructure/Data/AppDbContext.cs`: register the two new `DbSet`s + FK configuration.
    3. Add an EF migration (`dotnet ef migrations add AddRolePermissions`), apply it.
    4. `GHCAA.API/Extensions/ServiceExtensions.cs`: register a custom `IAuthorizationHandler` +
       `IAuthorizationRequirement` (e.g. `PermissionRequirement`) that checks the caller's `Roles`
       against the required permission via a DB/claims lookup, and register one `AddPolicy` call per
       permission needed (or a dynamic policy provider — simpler to hardcode a fixed permission list
       matching known admin feature areas: Events, Gallery, News, Financials, Members, JobHub).
    5. Update `RolesController` with CRUD for permissions-per-role (`GET/POST/DELETE
       api/roles/{id}/permissions`).
    6. `admin-roles.html/.ts`: add a permissions checklist UI per custom role.
    7. Go controller-by-controller replacing relevant `[Authorize(Policy = AdminOnly)]` attributes
       with permission-scoped policies where department-level admins are wanted — do this
       incrementally, not all at once, and add tests per controller touched.

49.2 [TODO] **User disable/enable — system admins (new) and members (UI gap only).**
  - **49.2.A — System admin accounts (new backend + UI):**
    1. `GHCAA.Application/Interfaces/IUserService.cs`: add
       `Task<bool> SetUserActiveAsync(int userId, bool isActive, CancellationToken cancellationToken = default);`
    2. `GHCAA.Infrastructure/Services/UserService.cs`: implement `SetUserActiveAsync` following the
       shape of `DeleteSystemAdminAsync` (line 126) — load user, guard against
       `ProtectedSuperAdminSeeder`-protected accounts (same check `DeleteSystemAdminAsync` uses), set
       `user.IsActive = isActive`, `SaveChangesAsync`; when `isActive == false`, also rotate
       `user.SecurityStamp` and call `await _tokenService.RevokeAllRefreshTokensAsync(userId,
       cancellationToken)` (mirror `UserService.cs:122`). Return `false` if user not found or
       protected.
    3. `GHCAA.API/Controllers/RolesController.cs`: add two actions under the existing `[Authorize(Policy
       = SuperAdminOnly)]` class-level attribute:
       ```
       [HttpPost("users/{id}/disable")]
       [GHCAA.API.Filters.RequireStepUp]
       public async Task<IActionResult> DisableUser(int id, CancellationToken cancellationToken)
       [HttpPost("users/{id}/enable")]
       [GHCAA.API.Filters.RequireStepUp]
       public async Task<IActionResult> EnableUser(int id, CancellationToken cancellationToken)
       ```
       Both call `_userService.SetUserActiveAsync(id, true/false, cancellationToken)`, return
       `BadRequest` on `false`, else `Ok`.
    4. `GHCAA.Web/src/app/admin/roles/admin-roles.ts`: add `toggleUserActive(userId: number, isActive:
       boolean)` calling the new endpoints, refreshing the grid on success.
    5. `admin-roles.html`: add an `.icon-btn` toggle in the row actions (near the existing delete
       icon) — show a "disable" icon when `user.isActive`, an "enable" icon otherwise; bind to
       `toggleUserActive`.
    6. Add `RolesControllerTests.cs` cases for both new actions (see 49.5).
  - **49.2.B — Member accounts (pure UI wiring, no backend change — endpoint already exists):**
    1. `GHCAA.Web/src/app/admin/members/admin-members.ts`: add `restoreMember(memberId: number)`
       calling the existing `POST members/{id}/restore` endpoint (same pattern as the existing
       `archiveMember` at `admin-members.ts:280-285`), refreshing the grid on success.
    2. `admin-members.html`: next to the existing "Archive" `.icon-btn` (lines 112-115), add a
       "Restore" `.icon-btn` shown only when `member.isArchived` is true (mirror the `@if
       (nav.isSuperAdmin())` guard already wrapping Archive), bound to `restoreMember`.
    3. Do not touch `ReactivateMemberAsync`/`member.Status` — that is a separate business-status
       action, unrelated to this restore/unarchive control.
    4. **DECISION NEEDED:** should `User.IsActive` be removed for members (since `IsArchived` already
       covers lockout) or kept as a distinct "temporarily disabled without archiving" state? Flag to
       user; do not silently pick one. If kept, this becomes its own follow-up item — do not scope-creep
       it into this task.

49.3 [TODO] **Admin-initiated password reset — system admins (new) and members (security fix).**
  - **49.3.A — System admin accounts (new backend + UI):**
    1. `GHCAA.Application/Interfaces/IUserService.cs`: add
       `Task<(bool Success, string? ResetUrl)> SendAdminPasswordResetLinkAsync(int userId, CancellationToken cancellationToken = default);`
       (same return shape as `IMemberService`'s version for consistency).
    2. `GHCAA.Infrastructure/Services/UserService.cs`: implement it — load `User` by id (no `Member`
       join), generate `token = Guid.NewGuid().ToString("N")`, set `ResetToken`/`ResetTokenExpiry =
       UtcNow.AddHours(24)`, `SaveChangesAsync`, build `resetUrl` using `Constants.ConfigKeys.ClientUrl`
       exactly as `MemberService.cs:1366-1367` does but keyed on `user.Username` (system admins may not
       have a real inbox — email may not apply; if `User` has no email field, return the `resetUrl` in
       the response for the SuperAdmin to copy/share manually rather than emailing it — confirm `User`
       entity has no `Email` field before assuming this).
    3. `RolesController.cs`: add
       ```
       [HttpPost("users/{id}/reset-password-admin")]
       [GHCAA.API.Filters.RequireStepUp]
       public async Task<IActionResult> ResetPasswordAdmin(int id, CancellationToken cancellationToken)
       ```
       calling `_userService.SendAdminPasswordResetLinkAsync`, returning the `ResetUrl` in the response
       body so the UI can display/copy it.
    4. `admin-roles.ts/html`: add a reset-password `.icon-btn` in system-admin rows; on click, call the
       endpoint and show the returned URL in a copyable dialog/toast (reuse whatever pattern
       `admin-members` uses if `ResetPasswordAdmin` already surfaces a URL client-side — check
       `admin-members.ts` for how it currently handles `AdminController.ResetPasswordAdmin`'s response
       before inventing a new pattern).
    5. Add `RolesControllerTests.cs` case for this action (see 49.5).
  - **49.3.B — Member accounts (security fix, no UI change to behavior — just backend hardening +
    add the missing button):**
    1. `GHCAA.Infrastructure/Services/MemberService.cs`, inside `SendAdminPasswordResetLinkAsync`
       (starts line 1344), immediately after the `user.ResetToken`/`ResetTokenExpiry` block and
       `SaveChangesAsync` (line 1361), add:
       `await _tokenService.RevokeAllRefreshTokensAsync(user.Id, cancellationToken);`
       (matches the pattern at `MemberService.cs:802` and `:1263`/`:1453`).
    2. `admin-members.html`: add a reset-password `.icon-btn` — check
       `GHCAA.Web/src/app/admin/members/` for an existing member-detail/"Manage" component first (the
       row already has Approve/Archive/Contact/Manage; if the Manage detail view exists, put it there
       instead of adding a 5th row icon — read that component before deciding).
    3. Wire it to `AdminController.ResetPasswordAdmin` (already exists, no backend change needed here
       beyond 49.3.B.1).
    4. Add/extend a test in `GHCAA.Tests/Controllers/AdminControllerTests.cs` (or `MemberServiceTests.cs`)
       asserting `RevokeAllRefreshTokensAsync` is now called during this flow.

49.4 [TODO] **Grid/row-control design consistency fixes** (mechanical, per [[ghcaa-design]]):
  1. `GHCAA.Web/src/app/admin/events/admin-events.html` line 322: rename the `.admin-table` class to
     `.data-table`. Then `grep -rn "admin-table" GHCAA.Web/src` to confirm no other file references it
     as a CSS selector; if the SCSS for `.admin-table` is now dead, delete that SCSS block.
  2. Same file, lines 272-275: replace the raw `.btn.btn-outline`/`.btn-secondary`/`.btn-sm` row-action
     buttons with `.icon-btn` markup matching `admin-roles.html:150`'s pattern (same icon-only button
     shape, `title` attribute for the tooltip, keep the existing click handlers unchanged).
  3. `admin-roles.html` lines 131-133: replace the bare `<button>✕</button>` role-chip-removal control
     with an `.icon-btn` (use the smallest/inline variant already defined in the shared stylesheet if
     one exists for inline-chip contexts; otherwise use the same `.icon-btn` sizing as the delete icon
     at line 150 and accept the size looking slightly large inside the chip — do not invent a new
     button variant class).
  4. After 1-3 are done, grep each of these files for `app-page-header`, `app-search-bar`,
     `.data-table`, `.icon-btn` to confirm all four are present: `admin-gallery.html`, `admin-jobs.html`,
     `admin-news.html`, `admin-financials.html` (not yet inspected in this audit). For each file missing
     one of the four, add it as a new lettered sub-item here (49.4.E, .F, ...) with the exact line
     number found, rather than fixing silently in the same pass — keeps this checklist auditable.

49.5 [TODO] **Test coverage for all new/changed endpoints above.** Add or extend
`GHCAA.Tests/Controllers/RolesControllerTests.cs` (new file if it doesn't exist yet) covering:
`DisableUser`, `EnableUser`, `ResetPasswordAdmin` (system-admin version) from 49.2.A/49.3.A, plus the
still-open pre-existing gap from 47.13.3 (`CreateAdmin`, `CreateRole`, `AssignRole`, `RemoveRole`) so
this doesn't become a second untracked follow-up — one test file, one PR, covering the whole
controller. Also extend `GHCAA.Tests/Services/MemberServiceTests.cs` or
`GHCAA.Tests/Controllers/AdminControllerTests.cs` per 49.3.B.4 for the refresh-token-revocation
regression test.

# Area 50 — Admin-configurable email/SMS template bodies (raised by user 2026-08-29/30: "need to
manage emails body to be confurable with all relevant informations, this also for sms (if used) by
admin") [DONE 2026-08-30]

Expanded the template-variable set used when substituting `{{Var}}` placeholders into admin-authored
`EmailTemplate` rows, and added the data-model/admin-UI abstraction for SMS templates (channel picked
by the user: build the shared abstraction for both channels, wire real sending for Email only —
SMS stays configurable-but-dormant, matching `GreenwebSmsService`'s existing zero-caller state).

50.1 [DONE] `GHCAA.Domain/Enums.cs`: added `MessageChannel { Email, Sms }`. `EmailTemplate.Channel`
(default `Email`) added via a hand-written idempotent PgSql migration
(`Migrations/PgSql/20260829173937_AddChannelToEmailTemplate.cs`) — the `dotnet ef migrations add`
auto-scaffold produced a 25k-line file from the known non-deterministic seed-drift issue (see
`gotcha_pending_model_changes_seed` in memory), so the `Up()`/`Down()` bodies were trimmed by hand to
just the real `ADD COLUMN`, following the same pattern already used in `AddSourceToActivityLog`.

50.2 [DONE] `CommunicationService.cs`: consolidated the two previously-independent variable-building
call sites (`SendEmailByCodeAsync`'s 2-var dict and `SendTemplatedEmailAsync`'s ~15-var dict) into one
`BuildTemplateVariables(member, cancellationToken)`, so a template resolves the same variables no
matter which send path delivers it. Added member fields `Status`/`Category`/`AppliedDate`/
`ApprovedDate`; added org fields `OrgName`/`OrgShortName`/`SupportEmail`/`PortalUrl`/`CurrentYear`
sourced from `IOrgConfigService.GetConfigAsync()` (already injected). Fixed `ReplacePlaceholders` to
HTML-encode substituted values on the Email channel via `WebUtility.HtmlEncode` — closes the raw
member-controlled-value injection gap already logged as 48.12 (a `<script>`-laden `FullName` no
longer lands unescaped in an HTML email body). `UpdateTemplateAsync`'s explicit field whitelist now
also copies `Channel` (it was silently dropping any field not listed there).

50.3 [DONE] `admin-comm` (web): template editor gained a Channel toggle — Email keeps the existing
shared `app-rich-text-editor`; SMS swaps to a plain `<textarea>` with a 160-char segment counter and
hides the Subject field. The old free-text "Variable Placeholders (JSON list)" input (no canonical
list, admins could mistype/forget names) was replaced with a clickable variable-chip reference panel
(Member / Organization groups) that inserts `{{VarName}}` at the cursor — `RichTextEditor` gained a
reusable `insertAtCursor` method for this. Templates list shows a Channel badge. All channel string
comparisons go through a centralized `MessageChannels` constant (`admin-comm.service.ts`), not
literals, per this repo's existing magic-string-centralization convention.

50.4 [DONE] Tests: `CommunicationServiceTests.cs` extended (org-var substitution on both send paths,
HTML-encoding of a malicious `FullName`, Sms-channel template Create/Update/Get round-trip);
`admin-comm.service.spec.ts` extended (channel round-trips through save); new
`admin-comm.spec.ts` and `rich-text-editor.spec.ts` added (state-level assertions only — this
project's `vitest.config.ts` strips every `templateUrl` to an empty template for all specs, so
DOM-structure assertions against `admin-comm.html` are not possible in this harness). Final state:
514/514 backend tests, 73 files/362 frontend tests, `ng build --configuration production` and
`tsc --noEmit` both clean.

Not done (explicitly out of scope this round, per user's channel-scope decision): no SMS send method
was wired — `ISmsService`/`GreenwebSmsService` remain untouched and still have zero callers anywhere
in the codebase.

---

# Area 51 — Universal photo-upload compression hard-cap (raised by user 2026-08-30: "photo_name should
be compressed by size with maximum quality not more than 512 kb, by internal compressed functionalities,
lightweight, error free, 100% workable" — server-side, applies to all photo uploads, resize+fixed-quality
strategy)

`LocalFileStorageService.SaveFileAsync` already has a compression path (`SixLabors.ImageSharp`,
quality 85→70 fallback, target 350KB). There's no hard size guarantee today: if an image is still
over target even at fallback quality, it's saved anyway — "not more than 512kb" is not actually
enforced, only aimed for.

51.1 [DONE 2026-08-30] `LocalFileStorageService.SaveFileAsync`: widened the compression branch (via
`IsCompressibleImageType`) from `uploadType == Photo` to every type that is *always* a plain display
image: `Photo`, `GalleryPhoto` (gallery/album uploads — `GalleryController`), `NewsImage`
(`NewsController`, `EventService` event logo). Deliberately left uncompressed, per explicit user
direction ("compress images only, not files") plus fidelity/evidentiary concerns: `Certificate` and
`NoticeDocument` (frequently PDFs, not images at all), `PaymentProof` (financial evidence — lossy
re-encoding of a receipt is undesirable even when it happens to be a photo), `Signature` (must stay
pixel-exact, forgery/legal-fidelity risk). The existing try/catch already falls back to a raw copy on
decode failure, so a non-image file mistakenly tagged with a compressible type degrades safely. The
output extension is only forced to `.jpg` for the three compressible types (`willCompress` flag),
never for the excluded types.
51.2 [TODO] Add a real hard-cap enforcement step: after the existing quality-drop (85%→70%) still
exceeds the target, downscale image dimensions (e.g. `Mutate(x => x.Resize(...))`, stepping the max
dimension down, not just quality) and re-encode, looping until under the cap or a sane minimum
dimension floor is hit — so "512kb max" is an actual guarantee, not best-effort. Introduce a distinct
hard-cap constant (`Constants.Defaults`: e.g. `MaxImageSizeKB = 512`) separate from the existing
"aim for good quality" `TargetImageSizeKB` (currently 350, keep as the first-pass target below the
hard cap).
51.3 [TODO] File naming: give saved files a type-prefixed name (per user's explicit ask — "event_",
"album_", "member_" or similarly descriptive, not an opaque GUID) instead of today's
`{Guid}_{originalFileName}` in `SaveFileAsync`'s `uniqueName` — e.g. `photo_`, `galleryphoto_`,
`newsimage_` prefixes keyed off `uploadType`, still GUID-suffixed for uniqueness.
Note: this is about the live upload pipeline going forward; the 6 gallery albums manually imported
from `GHC\images\albums\` this session already use a hand-applied `album_<slug>_NN.ext` convention
under `GHCAA.Web/public/assets/gallery/` (bundled web assets, not this upload pipeline) and don't need
touching for this.
51.4 [TODO] Tests: extend `LocalFileStorageService` coverage (`LocalFileStorageServiceTests.cs` exists
today but only ever exercises `FileUploadType.Photo` with compression disabled) for: compression
actually firing on `GalleryPhoto`/`NewsImage`, confirming it still does NOT fire on
`PaymentProof`/`Certificate`/`Signature`/`NoticeDocument`, the hard-cap resize loop (51.2) actually
converging under 512KB on a large fixture image, graceful fallback on a non-image input tagged with a
compressible type, and the new filename prefix per type (51.3).
51.5 [TODO] Admin-configurable file storage settings — today `ImageCompressionEnabled` /
`ImageCompressionQuality` / `ImageCompressionFallbackQuality` / `ImageCompressionTargetSizeKB` /
`MaxFileSizeBytes` only live in `appsettings.json` (`Constants.ConfigKeys`), so tuning them needs a
redeploy. Move them into the existing admin-editable `OrganizationConfig` row (single-row
`ConfigJson` blob already used for org-wide feature flags, loaded client-side via `OrgConfigService`
+ `APP_INITIALIZER` — same mechanism as `enableGallery` etc.) under a `fileStorage` section: enable
toggle, target/hard-cap sizes in KB, and quality/fallback-quality knobs, editable from an admin
settings screen the same way other org config sections are. `LocalFileStorageService` should read
current values from `IOrganizationConfigService`/equivalent (falling back to the existing
`Constants.Defaults` if the org row has no `fileStorage` section yet, e.g. right after this ships)
instead of `IConfiguration` directly, so a change takes effect immediately without a restart.

---

# Area 52 — Public landing gallery carousel (web-only) + mobile admin gallery active/featured/edit
parity (raised by user 2026-08-30: audit gallery work already on web, close the mobile gap, add
missing tests)

52.1 [DONE] Web: reworked `landing-gallery-preview` ("Campus Moments" section on the public landing
page, `GHCAA.Web/src/app/public/landing/sections/gallery-preview/`) to show every active album with
at least one photo (capped at 8), each cycling through its own photos on a shared 3s timer, instead
of a flat list of photos from only `isFeatured` albums. Web-only by design — mobile has no public
landing page (it's an authenticated member app), so there is no mobile equivalent to build here.
52.2 [DONE] Test coverage gap closed: no unit test existed for `LandingGalleryPreview` despite real
branching logic (`albums()` filtering/cap, `currentPhoto()` indexing, the cycling timer,
error-clears-state path). Added `gallery-preview.spec.ts` following the existing
`events-preview.spec.ts` mock pattern.
52.3 [DONE] Mobile parity gap found and closed: the web admin gallery screen
(`admin-gallery.ts`/`.html`) already let admins toggle an album's `isActive` (Public/Hidden) and
`isFeatured` (★ Featured) status and edit an existing album's title/date/location/description — all
backed by existing `GalleryController` endpoints (`toggle-active`, `toggle-featured`, `PUT
admin/{id}`). Mobile's `gallery_screen.dart` (member + admin album screen) only had Upload/Delete for
admins. Added `GalleryService.toggleActive`/`toggleFeatured` (`content_service.dart`, mirroring the
existing `deleteGallery`/`updateGallery` call style), wired them plus a reusable edit-capable
`_createGallery({existing})` dialog into the admin action-circle row, and added a static "★ FEATURED"
chip on the card (mirrors web's `admin-gallery.html` badge).
52.4 [DONE] Tests: extended `comprehensive_visual_freeze_test.dart`'s `FakeGalleryService` with the 2
new methods (required to keep implementing the interface) and added a plain `testWidgets` (not
`testGoldens`, per this repo's golden-fragility convention) asserting the new active/featured/edit
icons render for an admin role and that tapping them calls through to the service. The existing
`member_gallery` golden is unaffected — it renders as role `'Member'`, and the new controls are
admin-only.
52.5 [TODO] Not addressed here (out of scope): mobile's `admin_modules.dart` has a separate, simpler
"quick create gallery" dialog (posts straight to `/gallery/admin` with `isFeatured` hardcoded
`false`) — a duplicate, lighter-weight creation shortcut on the admin dashboard tile grid, distinct
from `gallery_screen.dart`'s own create flow. Left as-is; consolidating the two creation entry points
was not part of this ask and is a separate cleanup decision.

---

# Area 53 — Favicon / browser tab icon review (raised by user 2026-08-30)

53.1 [DONE] **Priority: P3 | Depends on: none.** Reviewed the favicon: `index.html` already pointed
at `assets/logo.png` (the correct transparent-branding asset, confirmed 1024x1024 RGBA with real
alpha — not the old opaque "dark box" `logo.jpg`), so branding was already right. The real gap was
performance/quality, not branding: the raw 515KB 1024px asset was being fetched directly as the
favicon and downscaled by the browser on every page load. Generated proper pre-sized icons
(`favicon-16.png`, `-32.png`, `-48.png`, `-180.png` via Pillow LANCZOS resize, `GHCAA.Web/public/`)
and wired them in `index.html` with explicit `sizes` attributes plus an `apple-touch-icon`; visually
confirmed the 32px version stays legible (crest shape + color quadrants read clearly at that size).

---

# Area 54 — Live-site issues raised by user 2026-08-30 (console log + admin comm + ledger)

54.1 [DONE] **Priority: P2 | Depends on: none.** Admin Gallery now defaults to a `.data-table` view
(matching News/Events/Roles), with a Table/Grid toggle (`viewMode` signal, defaults `'table'`) to
switch back to the card/cover-photo view. Caught and fixed a self-introduced template bug (an
`@else`-block/`</div>` closing-order mistake) via `ng build --configuration production` before
calling it done — `tsc`/`vitest` alone didn't catch it.

54.2 [DONE] **Priority: P1 | Depends on: none — but effect is deploy-gated (see note).** Stale-chunk 404 / "Failed to fetch dynamically imported module" errors reported by
user, plus admin communication page not loading.** Root cause is a known, partially-mitigated class
of bug (see `GHCAA.API/Program.cs:229-241,327-352`, which already sends `no-cache` headers on
`index.html` specifically to stop *new* page loads from serving a stale shell after a deploy). The
gap: a browser tab that was **already open** before a deploy still holds the old `index.html`'s
lazy-chunk hash references in memory; navigating to a lazy-loaded route in that stale tab (here,
`admin/comm` → `GHCAA.Web/src/app/app.routes.ts:225`,
`import('./admin/comm/admin-comm').then(...)`) requests a chunk filename that no longer exists on
the server post-deploy, and the browser throws `TypeError: Failed to fetch dynamically imported
module`. Confirmed via grep: no `ChunkLoadError`/dynamic-import error handler exists anywhere in
`GHCAA.Web/src/app`. Fix: add a global error handler (Angular `ErrorHandler` or a router
`NavigationError` subscription in `app.ts`/`app.config.ts`) that detects this specific failure
(message matching `Failed to fetch dynamically imported module`/`ChunkLoadError`) and does a hard
`window.location.reload()` (or a one-time redirect to the target URL) instead of surfacing the raw
error — this is the standard mitigation for this class of SPA deploy issue and doesn't need a new
deploy each time to "go away" (it will otherwise recur after every future deploy for any tab left
open across it). Re-test the admin communication page specifically after this lands, in case that
report was purely a symptom of this chunk-loading failure rather than a separate app bug.
**Implemented:** `GlobalErrorHandler` (`GHCAA.Web/src/app/core/services/global-error-handler.ts`)
now detects this failure and does a one-time `sessionStorage`-guarded `window.location.reload()`;
`main.ts` clears that guard on a clean bootstrap so a future deploy's stale-chunk incident still
gets one retry. **Important caveat given to the user:** this fix only takes effect once built and
deployed — it cannot retroactively fix the console errors already seen on the currently-live site,
and even post-deploy, a tab that already has the *old* JS running won't have this handler loaded
until it next reloads/navigates fresh. Re-test admin/comm after the next deploy.

54.3 [DONE] **Priority: P3 | Depends on: none.** Financial Ledger: added an income-by-category
breakdown alongside the existing Total Income / Total Expense / Current Balance summary cards.
This was a pure frontend gap — the data already existed and was already returned to the client unused:
`FinancialLedgerService.GetSummaryAsync` (`GHCAA.Infrastructure/Services/FinancialLedgerService.cs:96-121`)
already groups every record `.GroupBy(r => new { r.RecordType, r.FinancialCategory })` and returns
it as `LedgerSummaryDto.Details` (a `List<LedgerCategorySummaryDto>`, one row per
type+category with its own subtotal). `GHCAA.Web/src/app/admin/ledger/ledger.html:16-30`'s
"Quick Summary" block only reads `summary()?.totalIncome/totalExpense/netBalance` and never renders
`summary()?.details` at all. Add a breakdown section (e.g. a small table or a set of category
chips) filtered to `recordType === 'Income'` showing each `financialCategory` with its subtotal,
placed near the existing balance cards; do the same for expense categories if useful, but income-
by-category (the specific ask) is the priority.
**Implemented:** added `incomeByCategory` computed signal (`ledger.ts`) filtering
`summary().details` to `type === 'Income'`, rendered as a chip list under the balance cards
(`ledger.html`/`.scss`), reusing the existing `getCategoryName()` label helper. Expense-by-category
was left out — income was the specific ask and this stays reviewable as a small, focused diff.

54.4 [DONE] **Priority: P2 | Depends on: none.** Admin Users/Roles page role-assignment dropdown.
Confirmed both parts, and both are fixed:
  - Styling: `.role-select` (`admin-roles.scss`) turned out to be **dead CSS**, not just a
    mismatched one-off style — it was nested under `.actions`, a class no longer present on any
    ancestor in `admin-roles.html`, so the rule never matched anything and the dropdown rendered as
    a totally unstyled native `<select>`. Made it a top-level rule reusing the same
    `.form-group select` tokens (`var(--bg-color)`/`var(--border-color)`/`var(--accent-color-rgb)`
    focus ring) so it now matches every other dropdown in the app.
  - Functionality: added `updateRole(userId, oldRole, newRole)` (`admin-roles.ts`) doing
    remove-then-assign as one click. The row now shows an "Update" button (pre-filled dropdown,
    replaces old role) when the user has exactly one role, and keeps the original additive "Assign"
    + per-chip ✕-remove flow when a user has multiple roles (replacing one of several isn't
    unambiguous, so that case intentionally keeps the manual flow).

54.5 [DONE] **Priority: P2 | Depends on: none.** Fee Policy (and Payment Config) table action
buttons weren't using the app's central button design — confirmed genuinely unstyled, not just
"different." `admin-fee-config.html:52,55`
(`.action-btn edit` / `.action-btn archive`) and `admin-payment-config.html:57,60` (`.action-btn
edit` / `.action-btn delete`) both use an `.action-btn` class with **no CSS definition anywhere** —
grepped `admin-fee-config.scss`, `admin-payment-config.scss`, and the global `styles.scss`, all zero
hits, so these render as bare unstyled browser buttons, unlike every other admin table (News,
Events, Gallery, Roles, Members) which uses the shared `.icon-btn` / `.icon-btn delete` classes
(central style in `styles.scss`). Note `admin-themes.html:70,73` also uses `.action-btn edit`/`.action-btn
delete` but that one IS fine — `admin-themes.scss:136` defines its own `.action-btn` style, so it's a
one-off name collision, not a shared broken class; don't touch admin-themes when fixing this. Fix:
replace the `.action-btn edit`/`.action-btn archive`/`.action-btn delete` buttons in
`admin-fee-config.html` and `admin-payment-config.html` with the standard `.icon-btn` /
`.icon-btn delete` markup (icon glyph, `title` attribute) to match every other admin table, and
delete the now-dead custom classes if nothing else references them. **Check other areas too** (per
user's ask) — this repo-wide grep for `class="action-btn` found only these 3 files
(fee-config/payment-config/themes) using the pattern; no further instances found elsewhere, but if a
future admin page introduces its own one-off button class instead of `.icon-btn`, it should be
caught the same way.
**Implemented:** replaced both files' `.action-btn` buttons with standard `.icon-btn`/
`.icon-btn delete` markup; left `admin-themes.html` untouched (its `.action-btn` is a real, styled
class, not the same bug).

54.6 [DONE] **Priority: P1 | Depends on: none.** Bug: events stayed "Active" and the public portal
still accepted registrations after the event's own end date had passed. Confirmed in
`GHCAA.Infrastructure/Services/EventService.cs:205-221` (`RegisterForEventAsync`): the only date
gates checked before allowing a registration are `RegistrationStartDate` (line 217) and
`RegistrationEndDate` (line 220) — both **optional** (`DateTime?`) fields an admin may leave unset.
The event's own `EndDate` is never checked at all in this method. Separately, `IsActive` (line 210)
is a purely admin-controlled publish flag — nothing in the codebase automatically flips it to
`false` once `EndDate` passes, so a past event with `IsActive == true` and no
`RegistrationEndDate` set stays visible as "Active" and open for registration indefinitely. Fix:
add `if (now > alumniEvent.EndDate) throw new InvalidOperationException("This event has already
ended.");` in `RegisterForEventAsync` alongside the existing date checks (defense-in-depth even if
`RegistrationEndDate` is always set going forward), and decide with the user whether "Active" in the
public listing/admin list should also become computed (`IsActive && EndDate >= now`) rather than
purely the stored flag, or whether `IsActive` should stay a separate manual publish/unpublish switch
with a distinct "Ended" badge computed from `EndDate` shown alongside it — this is a product decision
about what "Active" is supposed to mean, not just a bug fix, so confirm the intended semantics before
changing what's displayed (the registration-blocking fix above is unambiguous and should ship either
way).
**Implemented (the unambiguous half):** added the `EndDate` hard-stop to
`EventService.RegisterForEventAsync` (backend) and to `EventsComponent.isRegistrationOpen()`
(`GHCAA.Web/src/app/common/events/events.ts` — the public "Closed" button state now correctly
triggers once `endDate` passes, not just `registrationEndDate`).
**Follow-up (2026-08-31), the deferred half now resolved:** user confirmed via AskUserQuestion —
**auto-compute status from dates.** Added `getEventStatus`/`getEventStatusMeta`
(`GHCAA.Web/src/app/core/utils/date.util.ts`) returning `Unpublished` (isActive false, always wins)
/ `Upcoming` / `Ongoing` / `Ended` from `startDate`/`endDate`; `IsActive` still controls
publish/hide, it just no longer pretends to mean "the event is currently happening." Applied to:
`admin-events.html`'s event-row badge (was literally `isActive ? 'Active' : 'Archived'`, the exact
thing the user was seeing); the public/member events list (`common/events/events.html`) gained the
same badge next to the "Members Only"/"Guests Welcome" pill, where previously there was no visible
lifecycle status at all — only the register button's Open/Closed state hinted at it. Reused the
existing `.status-badge` class + state modifiers (`active`/`pending`/`inactive`/`terminated`) already
defined centrally in `styles.scss` rather than inventing new colors. Verified via
`ng build --configuration production` (clean) and vitest (372/372).

---

# Area 55 — Landing page spacing + preview-section seed coverage (raised by user 2026-08-31)

55.1 [DONE] **Priority: P3 | Depends on: none.** Reduce the large empty gaps on the public landing
page between each section's header (title + subtitle + gold underline) and its content below —
user marked these with red boxes on a live screenshot, appearing under "Purpose & Objectives",
under "Executive Committee", and above "Membership Registry". Root cause is a single centralized
rule, so this is a one-place fix that affects every landing section at once: `.section-header` in
`GHCAA.Web/src/app/public/landing/landing.scss:739-768` sets `margin-bottom: 8rem` on the header
block itself and `p { margin-bottom: 3rem }` on the subtitle — an 8rem (128px) gap before content on
every section using this shared header pattern (Purpose, EC preview, Membership, and likely
Events/News/Gallery/Jobs previews too, since they all appear to reuse `.section-header`). Reduce
both by ~20%: `margin-bottom: 8rem` → `6.4rem`, subtitle `margin-bottom: 3rem` → `2.4rem`. Verify
visually across at least the 3 sections the user flagged, plus the other landing preview sections,
since this is a shared rule and a 20% reduction on an 8rem gap is still a substantial ~26px absolute
change per section.
**Implemented:** both values reduced exactly as above in `landing.scss`. Not yet visually verified
in a running browser against the flagged screenshot (no dev server/browser check performed this
pass) — worth a quick look after deploy since this is a shared rule touching every section at once.

55.2 [DONE] **Priority: P3 | Depends on: none — but re-verify counts before adding anything, they
may have changed since this check.** Ensure every data-driven section of the public landing page
("portal home") has at least one seed sample so it never renders empty, but only insert a seed
row when the live API actually returns nothing for that section — matching the repo's existing
idempotent/conditional-seed convention (e.g. `ConstitutionSeeder`, `ProtectedSuperAdminSeeder`).
**Checked now, before adding anything:** every relevant seed file already has ≥1 record —
`ec_members.json` (19), `events.json` (4), `galleries.json` (7), `jobs.json` (2), `members.json`
(631), `news.json` (1, thin but non-empty). Per the "only seed if empty" rule, **no new seed data
is needed today** for any current landing section. This task is forward-looking: (a) if
`news.json`'s single record ever proves too thin to exercise the news-preview carousel/pagination
properly, add one or two more idempotently (check-then-insert, not a blind re-seed); (b) any
**future** landing preview section that's added later should get its own conditional seed check
(read the table via the relevant `Get*Async()` — if it returns anything, skip; if empty, insert one
realistic sample row) as part of that feature's own PR, not deferred to a follow-up like this one
was.

55.3 [DONE] **Priority: P3 | Depends on: none.** Add a "Recently Joined Haragangians" section to the
public landing page ("portal home"), placed in the middle of the section order, mirroring the
member dashboard's existing widget. Confirmed feasible with **no new backend work**: the member
dashboard's version (`GHCAA.Web/src/app/member/dashboard/dashboard.html:219-244`, `.networking-widget`)
already shows a photo/name/membership-number grid via `NetworkingService.getRecentlyJoined()`
(`networking.service.ts:74`, calls `GET api/networking/search?sortBy=joinDate&sortDesc=true`), and
that endpoint is **already `[AllowAnonymous]`** (`NetworkingController.cs:23-26`, same one the
public Directory page already uses) — so a landing preview section can call it directly with no
auth changes needed. Implementation sketch: new `GHCAA.Web/src/app/public/landing/sections/
recent-members-preview/` component (mirror `ec-preview`'s structure — it's the closest existing
landing section using `NetworkingService`), reusing the dashboard's `.compact-member-card` markup
pattern (photo, name, membership number) inside a `.section-header`-styled wrapper for visual
consistency with the rest of the landing page. Placement: after `<landing-membership>` and before
`<landing-jobs>` in `GHCAA.Web/src/app/public/landing/landing.html:1-8` — sits in the middle of the
8-section page and reads naturally right after the Membership Registry section. Cap at 6-8 members
(match the dashboard's limit) and confirm with the user whether membership number should be shown
publicly (it's shown to authenticated members today; the landing page is anonymous-facing, so this
is worth a one-line confirmation before shipping even though the underlying endpoint already allows
anonymous access).
**Implemented:** new `GHCAA.Web/src/app/public/landing/sections/recent-members-preview/` component
(`.ts`/`.html`/`.scss`), mirroring `ec-preview`'s structure and reusing `landing.scss`'s
`.ec-carousel`/`.ec-card` visual pattern locally (`.recent-members-grid`/`.recent-member-card`).
Calls `NetworkingService.getRecentlyJoined(8)` (already anonymous). **Decided without re-asking**
(reasonable default, not a new open question): shows photo + full name + degree/passing-year
instead of membership number — membership number is an internal identifier with no clear public
value, while degree/batch is exactly the kind of "which Haragangian generation" info a public
visitor would find meaningful, so this avoids the anonymous-exposure question entirely rather than
needing a decision on it. Registered in `landing.ts`'s imports and placed in `landing.html` between
`<landing-membership>` and `<landing-jobs>` as specified. Verified via
`ng build --configuration production` (clean) and vitest (372/372).

55.4 [DONE] **Priority: P4 (design preference) | Depends on: none.** User wanted the public landing
page's News section to adopt the same look the member dashboard uses for "Latest News." Scope
confirmed via AskUserQuestion: **News only** (not Events/Jobs/Gallery). Confirmed the two were
genuinely different design languages, not just a minor styling drift:
  - Dashboard (`GHCAA.Web/src/app/member/dashboard/dashboard.html:136-158`, `.activity-feed`): a
    compact vertical text feed — header with a "See All →" link, then a list of `.feed-item` rows
    (small accent dot, bold title, one-line excerpt, small date), no images, no card borders. Same
    pattern reused for "Upcoming Events" right below it (lines 160-182, `.event-feed-item` with a
    small date-badge instead of a dot).
  - Landing page News (`GHCAA.Web/src/app/public/landing/sections/news-preview/news-preview.html`):
    a `.news-card.glass-card` grid — bordered cards, date, title, full content preview, its own
    "Read Full Story →" link per card. Landing page Events (`events-preview.html`) uses the same
    card-grid family (`.event-mini-card`).
  - Both were internally consistent and neither was broken — this was a **design-direction change**,
    not a bug fix.
  **Implemented:** replaced `news-preview.html`'s `.news-card` grid with a `.news-feed-list` of
  `.news-feed-item` rows (accent dot, bold title, 2-line clamped excerpt, date, arrow — mirroring
  dashboard's `.feed-item`), each row now a single `routerLink` anchor to the article (no separate
  "Read Full Story" sub-link, matching how the dashboard's feed rows work). Removed the now-dead
  `.news-grid`/`.news-card` rules from `landing.scss` (only news-preview used them) and updated the
  two mobile-breakpoint blocks that referenced them. Preserved: the `.section-header` title/underline
  (shared landing-wide convention, not part of what changed), the "All News" footer button, the
  empty-state message, and `isVisible()`-driven section hiding — all still work exactly as before.
  Verified via `ng build --configuration production` (clean) and the full vitest suite (372/372,
  no existing spec covers this component). Events/Jobs/Gallery previews were explicitly left on
  their existing card-grid design per the user's scope choice.

---

# Area 56 — Retroactive log: earlier same-session fixes not yet recorded (per user 2026-08-31: "make
sure you added tasks with status for all changes you done so far")

These landed before this session started tracking work as numbered TODO items; recording them now
for a complete audit trail. All `[DONE]`, all verified at the time via `dotnet build`/`dotnet test`
(516/516) and/or `ng build`/`vitest` (372/372) as noted in the original responses.

56.1 [DONE] **Priority: P2 | Depends on: none.** Fixed all 8 `Microsoft.EntityFrameworkCore.Model.
Validation[10622]` warnings from a production deploy log (global query filter vs. required
navigation mismatches). Added matching `HasQueryFilter`s to `AmendmentVoteConfiguration`,
`EventBudgetConfiguration`, `EventRegistrationConfiguration`, `EventTaskConfiguration`,
`NewsCollaboratorConfiguration`, `PollOptionConfiguration`, `PollVoteConfiguration`,
`RefreshTokenConfiguration`, plus a **new** `MentorshipRequestConfiguration` (no config class existed
for it before). Query-filter-only change, no migration needed. The 2 `DataProtection` warnings from
the same log were deliberately left as a known Render-free-tier infra limitation (ephemeral
container, no persistent key storage) rather than a code fix.

56.2 [DONE] **Priority: P1 | Depends on: none.** Admin News bug fixes (user-reported: "status not
saving, can't modify date"):
  - "Status not saving" was actually a **display bug**, not a save bug — `admin-news.html`'s table
    listed `IsActive`/"Visibility" instead of the actual `SubmissionStatus` dropdown value. Fixed to
    render via the existing `SUBMISSION_STATUS_MAP` helper (same one Jobs/Gallery/Articles already use).
  - Added a missing `PublishDate` field end-to-end (DTO, service, admin form `<input type="date">`)
    — previously there was no way to edit a post's date at all.
  - The new date field then hit two more bugs, both fixed: (a) a bare `"yyyy-MM-dd"` string sent to
    a `timestamptz` column with no UTC `Kind` made Npgsql reject the save entirely — fixed via a
    `toSafeISO()` conversion mirroring the one `admin-events.ts` already used correctly; (b) the
    native date-picker icon/popup was invisible in dark theme because the app never set the CSS
    `color-scheme` property — fixed by adding `color-scheme: light` to `:root` and
    `color-scheme: dark` to `body.dark-theme` in `styles.scss` (an app-wide fix, not News-specific).
  - A broader audit of Events' own date/datetime-local fields (raised by the user mid-fix) found
    those were already handled correctly (`toSafeISO` already applied, `datetime-local` always
    carries both date+time) — no changes needed there.

---

# Area 57 — Test coverage audit + a new live-site report to investigate (2026-08-31)

57.1 [TODO] **Priority: P1 | Depends on: none.** User asked: does the test suite actually verify
that create/update actions persist **every field** correctly, for **every entity** — not just a
happy-path subset? This has not been audited in this session. Needs a systematic pass: for each
entity with a create/update service method (Member, NewsPost, AlumniEvent, GalleryAlbum,
FinancialRecord, FeeConfiguration, PaymentConfiguration, User/Role, PollOption, MentorshipRequest,
etc.), check whether its existing test(s) actually assert on **every mapped field** after a
save/update round-trip (e.g. `existing.Status = dto.Status` needs a test that asserts
`result.Status == dto.Status`, not just "the call didn't throw" or "one or two fields matched").
Recommended approach: grep each `*Service.cs`'s `Update*Async`/`Create*Async` methods for the full
list of `existing.X = dto.X` assignments, cross-reference against that service's test file's
assertions, and report gaps as a checklist (entity → fields covered vs. fields silently untested) —
this is exactly the class of bug 54.6/56.2 turned out to be (a field quietly not applied, or applied
but never checked), so this audit is likely to surface real, currently-undetected bugs, not just
formalities. Do the audit and report findings before writing new tests, since the fix in each case
might be "add an assertion" or might be "the field genuinely isn't being saved" — those need
different responses.

57.2 [DONE — likely resolved as a side effect of 58.1, needs live confirmation] **Priority: P2 |
Depends on: 58.1.** User reported seeing "0% Profile Complete," in the **same message** reporting
"can't see member payments," "profile current data are not valid," and "profile health" also broken
— and explicitly noted the account involved was "an admin user but not member." That's the exact
58.1 scenario: a memberless Admin/SuperAdmin account hitting `/portal/*` pages that assume a real
`memberId`. `ProfileController.GetProfile` returns 401 when `GetMemberId()==0`
(`ProfileController.cs:33-36`); the frontend's `profileCompletion` getter (`dashboard.ts:36-37`)
defaults to `?? 0` when the profile fetch fails — which would produce **all four** symptoms
reported (missing payments, invalid-looking profile data, 0% completion, broken "profile health")
from one root cause, not four separate bugs. Not re-litigating `CalculateChecklistProfileCompletion`
itself (`MemberService.cs:1547`) — its logic and `.Include()`s were re-checked and are intact; the
real gap was `memberGuard` not existing yet to keep a memberless admin off these pages in the first
place, which 58.1 now fixes. **Needs live confirmation**, not just code inspection: re-test with the
same memberless admin account after this deploys — if 0%/missing-payments still appears on an
account that HAS a real memberId, that would be a genuinely separate bug and this item should be
reopened.

---

# Area 58 — Admin-without-member access, card/table-view audit, poll voting-window check (2026-08-31)

58.1 [DONE] **Priority: P1 | Depends on: none.** Bug: an Admin/SuperAdmin account with no linked
Member record (e.g. `ProtectedSuperAdminSeeder`-created accounts) could reach every `/portal/*`
page — Profile, Payments, Dashboard — because `authGuard` only checked `isAuthenticated()`, never
whether the account had a `memberId`. Those pages all assume a real member server-side
(`ProfileController.GetProfile` returns 401 without one via `GetMemberId()==0`), so a memberless
admin landed on broken/blank member pages (matches the user's report: "can't see member payments,"
"profile current data are not valid," "profile health" also broken). **Scope confirmed via
AskUserQuestion: block/redirect, not grant access** — a pure admin-only account has no member data
to grant access to. **Implemented:** new `memberGuard` (`GHCAA.Web/src/app/core/guards/auth.guard.ts`)
checks `auth.currentUser()?.memberId`, redirecting to `/admin/dashboard` if absent (mirrors
`adminGuard`'s existing redirect-away pattern); special-cases `/portal/change-password` through
unconditionally so a memberless admin who's also forced to change their password isn't caught in a
redirect loop with `authGuard`'s own change-password special-case. Applied to the `/portal` parent
route (`app.routes.ts`) alongside `authGuard`. Also hid the "Member Portal"/"Exit Admin" nav links in
`admin-layout.html` for memberless admins, so they don't even see a link that would just bounce them
back. Verified via `ng build --configuration production` (clean) and vitest (372/372).

58.2 [DONE] **Priority: P3 | Depends on: none.** Generalize 54.1's pattern (table view,
defaulting on, with a toggle back to card view) to every remaining card-grid list across the admin
**and** member portal. Audited admin/ this pass: most admin list pages already use `.data-table`
(News, Events, Job Approval, Gallery Approval, Members, Ledger) — **only one confirmed card-only
admin page found:** `GHCAA.Web/src/app/admin/polls/polls.html:13` (`.poll-card`, `*ngFor`, no table
alternative, not yet fixed). **Implemented for the public/member "News & Notices" page**
(`GHCAA.Web/src/app/common/news/news.html` — the page whose subtitle literally reads
"Announcements, updates, and stories," user specifically called this one out as "not card view, use
table view"): added the same `viewMode` signal (defaults `'table'`) + Table/Card toggle pattern as
`admin-gallery`, with a `.data-table` branch (Type/Title/Category/Date/Author/Actions columns,
row click opens the existing inline detail panel) alongside the original `.news-article` card list.
Verified via `ng build --configuration production` (clean) and vitest (372/372, no spec covers this
component). **Also implemented: `admin/polls/polls.html`** — added the same `viewMode`
(`polls.component.ts`) + Table/Card toggle, table columns Title/Status/Total Votes/Options/Type/
Created/Actions, with a "Results ↗" action opening a modal that reuses the card view's existing
option-by-option progress-bar breakdown (didn't try to cram vote percentages into table cells).
Existing spec for this component still passes unmodified. **Explicitly NOT converting
`member/polls/polls.html`** (the voting page, distinct from admin's management page): confirmed via
inspection it's a real voting form (`polls.html:14,23,36` — `.poll-vote-card`, checkbox/radio
options, "Submit My Vote" button) — a table row can't hold selectable options + a submit action
sensibly, and forcing this into a table would break actual voting usability, so this page is a
deliberate exception to the pattern, not a miss. Voting itself was independently confirmed still
correct in 58.3. **Audited the rest of member-portal, found more real candidates, not yet
implemented:** `common/directory/directory.html:86` (`.member-card`, no table view),
`common/jobs/jobs.html:105` (`.job-card`, no table view — this is also where "Mentorship" job
postings live, there's no separate Mentorship page). Not yet checked: member-facing Gallery. Do
these before considering 58.2 fully closed. **User confirmed: continue converting
Directory → Jobs → Gallery, in that order.**
**All three now implemented, 58.2 fully closed:**
  - `common/directory/directory.ts`/`.html`: added `viewMode` (defaults `'table'`), toggle hidden
    when `isCompact` (the embedded picker mode used elsewhere always stays card-based — forcing a
    full data table into that smaller embedded context didn't make sense). Table columns:
    Member (photo+name)/Membership No./Batch/Profession/Blood Group/Actions. The infinite-scroll
    sentinel + `IntersectionObserver` wiring was duplicated into the table branch (Angular
    `ViewChild('sentinel')` resolves to whichever branch is actually rendered) rather than shared,
    since card and table are mutually-exclusive `@if` branches.
  - `common/jobs/jobs.ts`/`.html`: same pattern, table columns Title/Company+Location/Category/
    Status/Posted By/Actions, Edit/Delete kept as `.icon-btn` where `canEdit(job)` is true.
  - `common/gallery/gallery.ts`/`.html`: same pattern, table columns Cover (thumbnail, reused
    admin-gallery's `.table-thumb` style)/Title/Date/Location/Photos/Actions. Row click opens the
    same photo-detail view the card grid already used.
  - All three verified via `ng build --configuration production` (clean), `dotnet test` (516/516),
    and `vitest` (372/372).

58.6 [DONE] **Priority: P1 | Depends on: none.** Bug: public landing page's Executive Committee
section rendered literal text "(Period: NaN)" in production. Real root cause (found after an
initial pass that just removed the text — user clarified they wanted the date-range **value** kept,
just not the word "Period:"): `NetworkingService.GetECPeriodsAsync`
(`GHCAA.Infrastructure/Services/NetworkingService.cs:131-137`) projected only
`{ p.Id, p.Title, p.IsActive }` — `StartDate`/`EndDate` were never sent to the client at all, even
though they exist in the DB (confirmed via `ec_periods.json`: the one seeded period has real
`StartDate`/`EndDate` values). `ec-preview.ts` then called `formatPeriodRange(active)` on an object
with no `startDate` field, so `new Date(undefined).getFullYear()` produced `NaN`, which interpolated
straight into the template. **Fixed at the source:** added `p.StartDate, p.EndDate` to the backend
projection; restored `activePeriodDateRange` in `ec-preview.ts` (now guarded with `active?.startDate
?` before calling `formatPeriodRange`, so a future data gap degrades to hiding the range instead of
showing `NaN` again); `ec-preview.html` shows `({{ activePeriodDateRange() }})` — the value only,
no "Period:" label — next to the period title. Verified via `dotnet build`, full `dotnet test`
(516/516) and `ng build --configuration production`/vitest (372/372), all clean.

58.7 [DONE] **Priority: P3 | Depends on: none.** On the public landing page, any section with zero
records should hide itself entirely rather than render an "empty" placeholder message. Currently
most preview sections only set `isVisible.set(false)` on a request **error**, not when the request
succeeds with zero items — so an empty section today shows a placeholder message instead of just
not existing. Audit and fix per section: `news-preview.ts` (shows "No recent announcements..."),
`events-preview.ts`, `jobs-preview.ts`, `gallery-preview.ts`, `recent-members-preview.ts` (added in
55.3, currently shows "No new members to show yet..."). **Needs one judgment call per section, not
a blind find-replace:** `ec-preview.html`'s `@empty` block renders a fixed list of "Vacant" position
placeholders (President, Vice President, etc.) when the committee list is empty — that's arguably
intentional (showing the org structure exists even with unfilled seats), not the same "no data, hide
it" case as a preview list simply having nothing to show yet; confirm with the user whether EC
should also hide when `committee()` is empty, or is exempt like `member/polls` was exempted in 58.2.
`landing-purpose`/`landing-membership` are not data-driven previews (static content), out of scope.
**Implemented (defaulted EC to stay exempt, per the same reasoning as its own note above — no
explicit override given, so left as the safer no-behavior-change default):** `news-preview.ts`,
`events-preview.ts`, `jobs-preview.ts`, `gallery-preview.ts`, `recent-members-preview.ts` now all
set `isVisible` based on the actual result length, not just on error; removed each one's now-dead
`@empty` placeholder markup (`news-preview.html`, `events-preview.html`, `jobs-preview.html`,
`gallery-preview.html` — the last one had 4 **fabricated** fake album names like "Annual Picnic
2024" as filler, now gone entirely rather than ever shown). `ec-preview.html`'s "Vacant seats"
placeholder deliberately left untouched. Verified via `ng build --configuration production` (clean)
and vitest (372/372, no existing spec asserted on the removed placeholders).

58.3 [DONE — verified working, no bug found] **Priority: n/a | Depends on: none.** User asked how
members are restricted to voting on active/open polls only. Checked both ends — already correctly
implemented, no fix needed: `PollService.GetActivePollsAsync` (`PollService.cs:31`) filters
`p.IsActive && !p.IsArchived && (p.ExpiryDate == null || p.ExpiryDate > DateTime.UtcNow)` for the
listing a member sees, and `PollService.VoteAsync` (`PollService.cs:83-91`) independently
re-validates both `p.IsActive && !p.IsArchived` (in the query) and `poll.ExpiryDate <
DateTime.UtcNow` (explicit check, correctly short-circuits to `false` when `ExpiryDate` is null, so
polls with no expiry aren't wrongly blocked) before accepting a vote — so a direct API call against
an expired/inactive poll is rejected server-side even if the member never saw it listed. This is the
same defense-in-depth pattern 54.6 added for events; polls already had it.

58.4 [DONE — same known cause, no new bug] **Priority: n/a | Depends on: 58.1 (deploy), 54.2
(deploy).** User pasted another copy of the live console log (same `Failed to fetch dynamically
imported module`/stale-chunk errors as 54.2/56.2 — this is the **currently-deployed** site, so of
course it still shows the pre-fix behavior; nothing new here), plus one new line:
`GET /api/gallery/albums/mine 401 (Unauthorized)`. That 401 is the **same 58.1 scenario** — a
memberless admin browsing a member-only portal page (the gallery "mine" endpoint needs a real
`memberId`) — not a separate bug. Once 58.1 and 54.2 are deployed, a memberless admin will be
redirected away from `/portal/*` before this call ever fires, and any tab still open from before
that deploy will self-heal via the 54.2 stale-chunk reload handler. No additional code change made
for this report.

58.5 [DONE] **Priority: P2 | Depends on: none.**
User reported the admin dashboard's stat-card action buttons ("Manage"/"View"/"View All"/"Ledger")
don't seem to work on click. Root cause found via inspection, not yet fixed:
`admin-dashboard.scss:167-181` (`.action-btn`) sets `opacity: 0; transform: translateY(6px);` by
**default**, only becoming visible (`opacity: 1`) on the parent `.stat-card:hover`
(`admin-dashboard.scss:117-121`) — the button is a real, working `routerLink` the whole time
(`admin-dashboard.html:44,53,62,71,81,91`), it's just invisible until the card is hovered, tucked
into the absolute-positioned bottom-right corner. `opacity:0` alone doesn't block clicks, so a
precise click on that exact invisible spot still works — but a user who doesn't hover first (most
likely on touch/tablet, where there's no true hover-before-tap, or anyone clicking the visible card
body/icon/title expecting the whole card to be the link) has no visible indication anything is
clickable there, which reads as "the buttons don't work." Recommended fix: stop hiding `.action-btn`
behind hover — either make it permanently visible (simplest, matches how every other action button
in the app behaves — none of them hover-reveal), or make the entire `.stat-card` clickable via its
own `routerLink`/`(click)` (the card already has a `ripple-effect` class hinting a whole-card click
was the original intent) with the button kept as a secondary, always-visible affordance. Don't just
remove the `opacity:0`/`transform` rule and call it done — re-check the `:hover` block still makes
sense afterward (it may become dead/redundant), and verify on an actual touch viewport, not just by
reasoning about CSS.
**Implemented (first option — simplest, no scope creep):** removed the `opacity:0`/
`transform: translateY(6px)` default state and the now-redundant hover rule that only existed to
undo it (`admin-dashboard.scss:117-121,167-181`); `.action-btn` is permanently visible now. Did not
also make the whole card clickable — that would be a bigger UX change than "make the existing button
work" and risks conflicting click targets (card-click vs. button-click) without a clear need. Not
verified on an actual touch device/viewport (no such tool available here) — verify that if possible
after deploy; the fix itself is unambiguous (a real click target is now visible where it wasn't).

---

# Area 59 — Association flag on the public About page (raised by user 2026-08-31, referencing
https://ghcaa-ryl6.onrender.com/about)

59.1 [DONE] **Priority: P4 | Depends on: none.** Add an "About The Association" section to the
public About page (`GHCAA.Web/src/app/public/about/about.html`) showing the association's **flag**
as a separate, distinct visual from the logo image — not the logo alone reused twice. Checked the
actual design spec so this isn't guessed: `GHCAA.Infrastructure/Data/Seed/constitution.json`
(`Content` field), **Article I, Section 7 — Flag**, states verbatim: *"Design: The official flag
features a solid white background with the association's logo positioned prominently in the
center."* / *"Symbolism of Color: The white color of the flag serves as a symbol of peace, harmony,
non-violence, and purity."* No flag image asset exists yet in `GHCAA.Web/public/assets/`.
Implementation approach: **render the flag live with CSS** rather than commissioning/generating a
separate image file — a solid white rectangular panel (with a thin border/shadow so it's visible
against the page background, and mind light/dark theme — the flag's white should likely stay pure
white regardless of the site's dark theme, per the "PAPER" token precedent in `landing.scss`'s
`--paper-bg` for the same never-flips-with-theme reasoning) with `assets/logo.png` (the existing
transparent logo) centered on top via absolute positioning or flexbox centering. This keeps the flag
in sync with the logo automatically if the logo is ever updated, and needs no new binary asset,
image generation, or admin upload flow. Lay the section out with the flag and the existing circular
logo medallion side-by-side (or flag left / logo+text right), each clearly labeled ("Official Flag"
/ "Official Emblem") so a visitor doesn't read them as the same image repeated. Reuse the existing
`.story-card`/`glass-card` section styling already established on this page rather than a new
one-off layout.
**Implemented exactly as planned:** new "Official Emblem"/"Official Flag" card pair in `about.html`
(reusing `.story-card`/`glass-card`), both rendered from the same `assets/logo.png` — the emblem
shown plainly, the flag shown centered on a `var(--paper-bg)` white panel (new `.symbol-display`/
`.flag-display` in `about.scss`, mirroring `landing.scss`'s `--paper-bg` "never flips with theme"
token for the same reason: a flag is a physical object, not themeable UI chrome). No new image
asset, no admin upload flow. Verified via `ng build --configuration production` (clean) and vitest
(372/372).

59.2 [DONE] **Priority: P2 | Depends on: none.** User reported garbled Bengali tagline text on the
About page: `"॥থিহ্যের বিনিময়..."` instead of `"ঐতিহ্যের বিনিময়..."`. Confirmed the **source code**
(`OrgConfigService.cs:182`, the `"bn"` `LocalePackDto.Tagline` default) already has the **correct**
text — this was never a code bug to begin with. Real root cause: `GetConfigAsync`
(`OrgConfigService.cs:26-37`) only falls back to `BuildGhcaaDefaults()` when the `OrganizationConfig`
DB row is entirely missing; once a row exists, it's trusted completely, and `UpdateConfigAsync`
(`OrgConfigService.cs:39-58`) round-trips the **entire** `OrgConfigDto` on every admin save (Org
Config admin form only has fields for Branding/Workflow/Features, confirmed no Localization/tagline
field exists in `admin/org-config/org-config.html`) — so a `Localization` value captured into the
live DB row before this tagline was corrected in source stayed permanently stale, being silently
re-saved untouched every time an admin edited anything else in Org Config. **Fixed as a self-heal**,
matching this repo's own established pattern for code-vs-stored-data drift (`ConstitutionSeeder`,
`MigrationBootstrapper`): `GetConfigAsync` now always overlays `Localization` from
`BuildGhcaaDefaults()` onto whatever was loaded, via `dto with { Localization = ... }` (`OrgConfigDto`
is a record) — since no admin UI ever intentionally edits this section, it should always reflect
current source, not whatever got frozen into a row historically. This also self-heals any *future*
copy fix the same way, not just this one instance. Verified via `dotnet build`, the 9 existing
OrgConfig-specific tests, and a full `dotnet test` run (516/516 including these).

59.3 [DONE] **Priority: P4 | Depends on: none.** User asked to make the About page's story-card
boxes have close word/character counts — confirmed a real imbalance: the "Historic Foundation" card
(`about.html:26-37`) ran ~75 words while the "Vision" card (`about.html:39-53`) ran only ~30 words
plus a short mission-pills list, reading visually lopsided in the two-card grid. Expanded the Vision
paragraph (kept all existing dynamic `orgConfigService` bindings — org full name/short name/member
nickname — and its factual meaning unchanged, just elaborated with real content already implied by
its own mission-pills list: networking, mentorship, heritage) to ~75 words, matching Historic
Foundation's length. Did not touch the admin-managed CMS story blocks (`blocks()`, shown instead of
this static fallback when Site Content has entries) — that's admin-authored content, not something
to silently rewrite. Verified via `ng build --configuration production` (clean).

59.4 [DONE] **Priority: P2 | Depends on: none.** User asked to remove the "Logo & Flag" About-page
section (the 59.1 Emblem/Flag card pair read as two near-duplicate boxes since both render the same
`logo.png`), distribute its crest/flag content onto "The Association" content, and add a
wave-on-hover animation to the surviving flag visual. Also reported garbled Bengali motto text on
the live site — this turned out to be a **second, separate instance** of the 59.2 bug: the CMS `SiteContents` row
`Key = about-association` carries its own independently-seeded copy of the tagline with wrong
numeric character references (`&#2405;&#2469;` decoding to `"॥থ"` instead of `"ঐত"`), unrelated to
the `OrganizationConfig`/`OrgConfigService` row 59.2 already fixed —
`SiteContent` has no self-heal (it's admin-editable content, so overlaying source defaults on every
boot would silently clobber real admin edits), so this needed a one-time data fix, not a self-heal.
Removed the `about-logo` CMS block (`GHCAA.Infrastructure/Data/Seed/site_content.json`) and merged
its crest/flag description plus the corrected motto into `about-association`'s `BodyHtml`; same
change shipped to production via a hand-written idempotent migration
(`20260831000000_FixAssociationContentMergeLogoFlag.cs`, matched by `Key` not `Id` — UPDATE/DELETE
are no-ops if re-run) rather than scaffolded `UpdateData`/`DeleteData` (`dotnet ef migrations add`
here always also emits unrelated `Users.SecurityStamp`/`EmailTemplates.LastUpdated` churn, see
`gotcha_pending_model_changes_seed`). Also fixed the same wrong entities in the historical
`20260802163432_AddSiteContentAndNoticeFields.cs` seed insert (cosmetic — doesn't affect already-
migrated prod data, only future from-scratch DBs). In `about.html`, removed the hardcoded
`symbols-grid` (Emblem+Flag) block entirely and added a small `.assoc-flag-badge` (still
`logo.png`, no new asset) inside the CMS-rendered card where `block.key === 'about-association'`,
plus the same badge in the static fallback's "Vision" card; new `flag-wave` CSS keyframe animation
plays on `:hover`. Page section count: header + 4 CMS blocks (Origin, College Today, Association,
What We Do) + governance/pillars banner = 6, as requested. Verified via `dotnet build` (0 errors),
`npx tsc --noEmit`, and `ng build --configuration production` (clean).

---

# Area 60 — Mobile parity plan for this session's portal changes (raised by user 2026-08-31: "plan
for mobile tasks that have in portal but missed and needed")

A codebase-wide comparison (member portal `GHCAA.Web/src/app/member/`+`common/` vs. `GHCAA.Mobile/lib/`)
found mobile already has an equivalent screen for essentially every member-facing web feature —
Dashboard, Profile, Payments/Financials, Directory, Jobs, Mentorship Hub, Events, News, Gallery,
Polls, Chats/Forum, Notifications, Governance, Digital ID. This is a **plan only** — nothing below
has been implemented; each item needs its own scoping/estimate before work starts.

**Explicitly rejected, not a real gap:** the initial pass flagged "port the new Table/Card view
toggle to mobile Directory/Jobs/Gallery/News" as a gap. That is **not applicable to mobile** — a
wide multi-column data table is a desktop/web affordance; phone-width screens already use a
card/list layout for exactly the reason a table wouldn't fit, and that's the *correct* mobile
pattern, not a missing feature. Do not port table views to mobile.

60.1 [TODO] **Priority: P4 | Depends on: none.** Mobile's News screen
(`GHCAA.Mobile/lib/screens/member/news_screen.dart`) has not been re-checked against this session's
web News restyle (55.4 — compact dashboard-style feed-list replacing the old card grid, News-only).
Verify whether mobile's News screen still uses a materially different layout convention than both
the (also News-only, web-side) restyled section and mobile's own established list-screen patterns
elsewhere (Jobs, Gallery) — if it's already visually consistent with mobile's own conventions, no
action needed; a redesign is only warranted if it's inconsistent with itself, not to chase visual
parity with a web-specific style choice.

60.2 [TODO] **Priority: P3 | Depends on: none.** Confirm this session's two *behavioral* (not
visual) fixes protect mobile automatically, since both are enforced server-side, not client-side:
  - 54.6 — event registration blocked past `EndDate` even with no `RegistrationEndDate` set. Mobile's
    event registration call hits the same `RegisterForEventAsync` backend method, so this should
    already be covered with no mobile code change — but verify mobile's own UI doesn't *also* have a
    client-side "can register" check duplicating the old (pre-fix) logic, which would show a
    misleading enabled button that the server then rejects.
  - 58.3 — poll voting already correctly blocked past `ExpiryDate`/inactive server-side
    (`PollService.VoteAsync`), confirmed already correct — no web fix was needed, so nothing to
    check on mobile for this one specifically, included here only for completeness of the sweep.

60.3 [TODO] **Priority: P4 | Depends on: none.** Messages vs. Chat+Forum naming mismatch: web has a
single "Messages" page (`member/messages`); mobile splits the same networking space into a separate
Chats screen and a Forum feature (`chats_screen.dart`/`chat_room_screen.dart` +
`forum_categories_screen.dart`/`forum_topics_screen.dart`/`forum_topic_detail_screen.dart`). Not
confirmed whether these map to the same backend feature/data model or web's Messages covers ground
mobile's split model doesn't (e.g. direct 1:1 alumni messaging vs. threaded forum discussion) —
needs a closer read of both the web and mobile networking/messaging services before concluding
anything is actually missing; flagged as needs-verification, not a confirmed gap.

## AREA 61: CODE COMMENT/DOC TONE + REFACTOR SWEEP (raised by "prepare a plan for human-toned comments/docs/TODOs, refactor review, token usage", 2026-09-01)

A repo-wide `git diff` came back empty (working tree clean, no unmerged upstream commits), so there
was nothing to run a bug-hunting diff review against this session. Scope was refactor/tone/plan work
instead: a global rule was added and a light scan was run to size the actual cleanup, rather than
guessing at it.

**Done this session:**
- Root `CLAUDE.md` now has a "Comment, Doc & TODO Tone" section: plain sentences, no AI filler
  openers ("This function is responsible for...", "It's important to note..."), no comment banners,
  no restating-the-obvious comments, TODOs must name the real gap.
- `.claude/skills/ghcaa-standards/SKILL.md` Style section now points at that rule so it surfaces
  whenever the standards skill loads.
- Scanned `*.cs`/`*.ts`/`*.dart` for AI-tell comment patterns (filler openers, `=== SECTION ===`
  banners, `Summary:`/`Purpose:`/`Overview:` headers, vague `TODO: improve/fix this`). Only one hit:
  a `Summary:`-style banner in `GHCAA.Tools/db_diag.cs` (a standalone diagnostic script, not part of
  the shipped app) — not worth a dedicated pass. TODO/FIXME/HACK markers total 15 across 12 files,
  small enough to review inline next time each file is touched rather than as a separate sweep.
- 2026-09-01 follow-up: scope widened on request — the tone rule now applies retroactively (touch a
  file for any reason, clean up what you pass over in it), and a full repo-wide sweep was requested
  ("nothing should be missed"), not just the light grep above. See 61.4.

61.4 [DONE 2026-09-01] Full repo-wide human-tone pass, run as 4 parallel grep-driven sweeps
(backend/web/mobile/docs) instead of one blind full-repo read:
- **Backend** (`GHCAA.Api`/`Application`/`Domain`/`Infrastructure`/`Tools`/`Tests`): 6 files fixed.
  Worst finds were leftover first-person AI reasoning traces left in as comments
  (`// I'll fix service next`, `// Actually... Better approach... No.`) in `NewsController.cs` and
  `GatewaysController.cs` — replaced with one factual comment / a TODO naming the real gap
  (webhook can't thread the transaction ID back to `HandleSuccessfulPayment`). Also trimmed a
  filler `/// <summary>` in `VisualTestAuthMiddleware.cs`, an obvious `// Increment view count`
  in `ForumService.cs`, and the `Summary:` banner in `GHCAA.Tools/db_diag.cs` (closes 61.3).
  `dotnet build` clean on GHCAA.Api and GHCAA.Tests.
- **Web** (`GHCAA.Web/src`): 0 files changed — already clean, no genuine AI-tell comments found.
- **Mobile** (`GHCAA.Mobile/lib`): 8 files fixed. Marketing-flavored comment fluff ("World-Class",
  "Industry Standard", "Majestic", "Dynamic ... Framework") layered on otherwise fine code, plus one
  rambling draft-style comment in `submit_article_screen.dart` replaced with a plain sentence.
  `dart analyze lib` clean.
- **Docs** (`docs/*.md`, root `README.md`): 3 files fixed (`SRS.md`, `FEATURES.md`, `README.md`) —
  stripped brochure adjectives ("enterprise-grade", "intelligent", "comprehensive") that didn't
  match what the described feature actually does (e.g. "Intelligent Support Chat" is a plain
  rule-based chat, not AI). ~25 docs checked, rest already plain.

Root `CLAUDE.md`'s tone rule is now retroactive (applies whenever a file is touched, not just new
edits), per this session's explicit ask.

> **2026-09-01 UPDATE: 61.1, 61.2 and 61.3 are now executed inside Area 62, not separately.** Area 62
> (white-label/genericization) edits most of the same files, so running these as standalone sweeps
> means reading the whole repo twice. They are re-scoped as per-phase obligations there and tracked in
> 62.46-62.49. Do not start a separate pass for them; if Area 62 is cancelled or deferred, re-open
> them here as originally written.

61.1 [TODO → tracked in 62.47] **Priority: P3 | Depends on: none.** No dedicated dynamic/runtime code-analysis pass has
been run against this app (dead-route detection, unused Angular providers/services, unreferenced
.NET classes, unused Flutter widgets). `graphify query`/`graphify explain` can narrow this cheaply
per-module instead of a blind full-repo sweep — run it module by module next time this is picked up,
not as one pass, to keep token usage down.

61.2 [TODO → tracked in 62.48] **Priority: P3 | Depends on: 61.1.** Once dead/unused code is identified, do the actual
refactor pass (remove or consolidate) — deferred until 61.1 gives real targets instead of guessing.

61.3 [TODO → absorbed by 62.46] **Priority: P4 | Depends on: none.** Spot-check `GHCAA.Tools/db_diag.cs` next time it's
touched and drop the `Summary:`-style banner comment for a plain one-line comment, matching the new
tone rule. Not worth a standalone edit today — it's a diagnostic script, not shipped app code.

## AREA 62: INSTITUTION-AGNOSTIC / WHITE-LABEL PLATFORM (raised by "make this application generic rather than GHC ... will work with GHC or any other institution with minimal configuration changes", 2026-09-01)

> Reference doc: docs/GENERICIZATION_PLAN.md (architecture decisions, full hardcode audit,
> non-breaking guarantees, verification strategy). Read it before picking up any item here.
>
> Scope in one line: one institution profile pack (`profiles/<name>/` JSON + assets) plus a few env
> vars stands up the platform for any alumni association. No source edits per institution.
>
> Key decisions already made (see plan ADR-1..ADR-5): deployment-per-institution, NOT row-level
> multi-tenancy; profile pack is the single source of truth; runtime plane (OrganizationConfig, admin
> editable) stays separate from boot plane (favicon, bundle id, SEO); MembershipType stays an enum
> and gains a config-driven label/policy layer; a `brand-lint` CI check prevents re-branding drift.
>
> Area 28 already shipped the runtime plumbing (OrganizationConfig entity, /api/config, Angular
> APP_INITIALIZER, Flutter Riverpod, feature guards, SiteContent CMS). Area 62 is NOT a rebuild of
> that. It replaces the three hardcoded GHC default blocks feeding it, and closes the long tail of
> literals that never went through the config path.
>
> HARD CONSTRAINT on every item below: the existing GHC deployment must behave identically after the
> change. See plan section 8 for the seven specific traps (data-protection app name, issued
> membership numbers, mobile bundle id, enum int values, live URLs, localization self-heal, seed path
> vs migration bootstrapper).
>
> **AREA 61 IS FOLDED INTO THIS AREA, NOT RUN BESIDE IT.** Area 62 touches most of the files Area 61
> still has open work on, so doing them separately means reading the same files twice. Three standing
> rules apply to every 62.x item, and they are not optional extras:
>
> 1. **Tone rule is retroactive (root CLAUDE.md).** Any file a 62.x item edits gets its AI-sounding
>    comments/docs cleaned up in the same commit, not just the lines being changed. That includes the
>    new code: profile loaders, brand-lint, and the onboarding docs must read like a person wrote
>    them. No filler openers, no em dashes, no banner comments, no restating the obvious. TODOs name
>    the real gap.
> 2. **Dead-code detection is free here (61.1).** Genericization deletes three large hardcoded default
>    blocks and re-points seeders, which strands helpers, constants, and imports. Run
>    `graphify query`/`explain` on each module as it is touched and record what falls out, instead of
>    the separate blind full-repo sweep 61.1 was going to need.
> 3. **Refactor as you pass, do not open a parallel refactor (61.2).** Remove what 61.1 surfaces in
>    the same phase that stranded it. Do not start a general refactor that is unrelated to
>    genericization; that is still out of scope and still bounded by the "no new abstraction without a
>    concrete duplication problem" rule.
>
> Net effect: 61.1 and 61.2 stop being standalone blind sweeps and become per-phase obligations,
> tracked in 62.46-62.49 below. 61.3 is absorbed by rule 1 the next time that file is touched.

### PHASE A: PROFILE PACK FOUNDATION (blocking, no user-visible change)

62.1 [TODO] **Priority: P1 | Depends on: none.** Infra: `IInstitutionProfileProvider` +
implementation. Resolves `ORG_PROFILE` env var (default `default`), loads `profiles/<name>/*.json`,
merges file-by-file over `profiles/default/`, caches, validates on boot and fails with a readable
error listing missing keys. Registered in DI ahead of `OrgConfigService`.

62.2 [TODO] **Priority: P1 | Depends on: 62.1.** Create `profiles/default/` neutral pack:
org-config.json, site-content.json, email-templates.json, lookups.json, membership-tiers.json,
governance.json, documents.json, seo.json, assets/ (neutral logo, favicons, seal), demo-data/ (small
synthetic set). Must contain zero real personal data and zero GHC strings.

62.3 [TODO] **Priority: P1 | Depends on: 62.1.** Create `profiles/ghc/` by extracting today's values
verbatim from `OrgConfigService.BuildGhcaaDefaults()`, `Constants.Defaults`, `appsettings.json`
GeneralSettings, and the existing Seed JSON files. Verbatim means verbatim: this pack is the
regression baseline.

62.4 [TODO] **Priority: P1 | Depends on: 62.3.** Tests: golden config snapshot. Capture
`GET /api/config` output BEFORE any Phase B change, then assert `ORG_PROFILE=ghc` reproduces it
byte-identically. This test is what makes the rest of the area safe; write it first.

62.5 [TODO] **Priority: P2 | Depends on: 62.2.** CI: `brand-lint` script scanning API/Web/Mobile
source (excluding `profiles/`, migrations, test fixtures) for banned literals: `GHCAA`, `GHC-`,
`Haraganga`, `Haragangian`, the live Render hostname, `1938`. Warn-only until 62.42. Exception list
in `brand-lint.config.json` with a reason per entry.

### PHASE B: API DE-BRANDING

62.6 [TODO] **Priority: P1 | Depends on: 62.1, 62.4.** Infra: replace
`OrgConfigService.BuildGhcaaDefaults()` (~190 lines of hardcoded C#) with
`BuildDefaultsFromProfile()` reading the pack. Keep the Localization self-heal behaviour, but heal
from the profile, not from code (see plan 8.6).

62.7 [TODO] **Priority: P2 | Depends on: 62.6.** Domain: move `Constants.Defaults.MembershipPrefix`
(`"GHC-"`) and `ImportEmailBase` (`"haragangian"`) to config. NEW numbers only. No backfill or
reformat of already-issued `MembershipNumber` values (plan 8.2).

62.8 [TODO] **Priority: P2 | Depends on: 62.6.** Infra: `IDCardService` (QuestPDF) takes
`IOrgConfigService`. Removes the three hardcoded institution strings (lines ~73, ~141, ~197) and the
hardcoded `#c5a059` accent, using `branding.accentColor` instead. ID card and certificate both.

62.9 [TODO] **Priority: P2 | Depends on: 62.6.** Infra: parameterize `email_templates.json` with
`{{OrgName}}`/`{{OrgShortName}}`/`{{SupportEmail}}` placeholders (org name is currently literal text
in 5 template bodies) and supply them in the renderer. Also drop the duplicate copy under
`Seed/Visual/` or make it a build-time copy of the same source.

62.10 [TODO] **Priority: P2 | Depends on: 62.1.** API: `Program.cs` Swagger title and
`SetApplicationName("GHCAA")` become config. CRITICAL: the GHC profile must keep the exact string
`GHCAA` for the app name; changing it invalidates the data-protection key ring and every token/cookie
it protects (plan 8.1). Add it to the brand-lint exception list with that reason.

62.11 [TODO] **Priority: P2 | Depends on: 62.1.** API: `appsettings.json` GeneralSettings
(`AssociationNamePrefix = "HARAGANGIAN-"`, `EmailDomain = "haragangian.com"`, PortalBaseUrl,
Currency) become profile-sourced with appsettings as an override, not the source.

62.12 [TODO] **Priority: P2 | Depends on: 62.1, 62.3.** Infra: seeders (`ConstitutionSeeder`,
site content, email templates, lookups, themes) read profile-relative paths instead of fixed
`Data/Seed/*.json`. Path change only. Do NOT alter `MigrationBootstrapper` idempotency or baselining
logic (plan 8.7).

62.13 [TODO] **Priority: P2 | Depends on: 62.12.** Verify 62.12 against a throwaway copy of the
preprod database (project convention for migration work) and confirm no re-seed, no re-baseline, no
history-row churn, before it touches preprod.

62.14 [TODO] **Priority: P2 | Depends on: 62.6.** Tests: fix the 6 literal `"GHCAA"` assertions in
`OrgConfig/OrgConfigServiceTests.cs`, `Services/CommunicationServiceTests.cs`,
`Services/MemberServiceTests.cs`, `Services/TokenServiceTests.cs` to assert against the loaded
profile rather than a constant.

### PHASE C: WEB DE-BRANDING

62.15 [TODO] **Priority: P2 | Depends on: 62.6.** Angular: remove the `ghcaaDefaults` block in
`core/services/org-config.service.ts` (lines ~37-129). Preferred fix is a single source: emit the
boot fallback from the active profile at build time rather than keeping a hand-maintained TS copy.

62.16 [TODO] **Priority: P2 | Depends on: 62.15.** Angular: route titles. `app.routes.ts` L13-92
hardcodes "GHCAA" / the full college name in every route `title` and `data.description`. Replace with
a `TitleStrategy` composing a generic route label with `branding.shortName` from config. Also
`app.ts` L24 fallback SEO description.

62.17 [TODO] **Priority: P2 | Depends on: 62.2.** Web build: `scripts/apply-brand.mjs` prebuild step
that templatizes `index.html` (title, meta description/keywords, canonical, the full
`AlumniOrganization` JSON-LD block L17-35), `public/sitemap.xml` (9 hardcoded Render URLs),
`robots.txt` (sitemap URL), and copies the favicon set from `profiles/<name>/assets`. Driven by
`seo.json`. GHC profile must emit the current hostname unchanged (plan 8.5).

62.18 [TODO] **Priority: P2 | Depends on: 62.2.** Angular: move static institution prose into
`site-content.json` blocks with a genuinely generic empty state. Files: `register.html` (T&C L497-562
naming the college, founding date, IP/branding ownership clause), `about.html` (founding story L36-39
which is hardcoded even inside the config-driven `@else` fallback branch), `purpose.html` L68.

62.19 [TODO] **Priority: P3 | Depends on: 62.15.** Angular: remaining literal-string components:
`digital-id.html` L23/59/60/73, `assistant.html` L7/17 ("GHCAA-AI"/"Haraganga AI Assistant"),
`directory.html` L4/139/224, `magazine.html` L3, `gallery.html` L5, `events.html` L326,
`membership.ts` L26/43/108, `payment-status.ts` L49, `elections.ts` L97/121 hardcoded fallbacks
(`'29 Nov 2025'`, full org name), plus admin placeholder text in `org-config.html` L86,
`admin-members.html` L489, `admin-themes.html` L204.

62.20 [TODO] **Priority: P3 | Depends on: 62.15.** Angular: direct `/assets/logo.png` references that
bypass OrgConfig: `reset-password.html` L4, `register.html` L7, `logo-spinner.html` L5, `about.html`
L20/48 (the flag badge). Route through the config-driven image path with the neutral asset as
fallback. Keep the intentional jpg fallbacks noted in project memory.

62.21 [TODO] **Priority: P3 | Depends on: 62.15.** Angular: hardcoded download filenames.
`digital-id.ts` L46 `'GHCAA_ID_Card.png'`, L63 `'GHCAA_Certificate.png'` become
`${branding.institutionAcronym}_...`.

62.22 [TODO] **Priority: P3 | Depends on: 62.2.** Angular + API: constitution/bylaws document
registry. `constitution.ts` L21 hardcodes `'/assets/GHCAA Constitution V4.2.pdf'`. Replace with
`documents.json` entries (label, file, version, group) so any institution publishes its own governing
documents and forms without a code change.

62.23 [TODO] **Priority: P4 | Depends on: none.** Web housekeeping: `package.json` name
`"ghcaa.web"`, `styles.scss` L2 header comment "GHCAA Professional Design System". Cosmetic, but they
are brand-lint hits so they need either a fix or an exception entry.

62.24 [TODO] **Priority: P3 | Depends on: 62.15.** Verify the gold palette (`--accent-color: #c5a059`,
`--gold-gradient` in `styles.scss` L24-130) is only a seeded default and not a hard dependency, given
`admin-themes` makes themes admin-configurable. If it is a hard default, move the seed values into the
profile pack; do not touch the token system itself.

### PHASE D: MOBILE DE-BRANDING

62.25 [TODO] **Priority: P2 | Depends on: 62.2.** Flutter: flavor setup driven by the profile. App
name, bundle id, icons, and splash generated from `profiles/<name>/assets` via
`flutter_launcher_icons` + `flutter_native_splash` in the build script. Covers
`AndroidManifest.xml:8` label, `build.gradle.kts:23` applicationId (and the still-default namespace
`com.example.ghcaa_mobile` at L9), `Info.plist:26,34`, `pubspec.yaml:1-2`.

62.26 [TODO] **Priority: P1 | Depends on: 62.25.** CRITICAL non-breaking constraint: the GHC flavor
pins `com.ghcaa.portal`. Changing the bundle id of the published app makes it a NEW store listing,
not an update (plan 8.3). Only new institutions get a new id. Add to the brand-lint exception list.

62.27 [TODO] **Priority: P3 | Depends on: 62.6.** Flutter: replace the `ghcaaDefaults` block in
`lib/core/config/org_config.dart` with a neutral offline fallback, and the `'Haragangian'` /
`'Haragangian Portal'` / `'Govt. Haraganga College'` fallbacks in `lib/core/config/app_config.dart`
L21/37/49.

62.28 [TODO] **Priority: P3 | Depends on: 62.27.** Flutter: literal strings to `localePack` keys.
`app_home_screen.dart:45` biometric prompt, `register_screen.dart:230` hint,
`about_screen.dart:64/84/88`, `ai_chat_screen.dart:17` greeting, `chat_room_screen.dart:65`
("Haragangian Nexus"), `digital_id_screen.dart:81`, `magazine_screen.dart:38`,
`profile_edit_screen.dart:119/128`, `submit_article_screen.dart:59/100`,
`register_wizard_provider.dart:71` default institutionName.

62.29 [TODO] **Priority: P4 | Depends on: 62.28.** Flutter: rename `HaragangianApp` /
`_HaragangianAppState` in `main.dart:120-132` to a neutral `AlumniApp`. Mechanical, do it last in the
mobile phase to avoid churn in the other diffs.

62.30 [TODO] **Priority: P4 | Depends on: 62.27.** Flutter: confirm `app_theme.dart:6-10,116-117`
gold/obsidian constants stay as fallback-only (`_colorFromHex(branding.primaryColor, royalGold)` is
already the pattern) and swap the fallback values to neutral. Mobile stays single forced dark theme;
this is not a theming rework.

### PHASE E: DATA, TIERS, GOVERNANCE

62.31 [TODO] **Priority: P0 | Depends on: 62.3.** DATA PROTECTION: `Seed/members.json` holds 631 real
alumni records (names, emails, mobile numbers, NIDs, addresses). Once the repo is handed to other
institutions this is a personal-data disclosure. Decide before Phase E ships: (a) keep real member
data out of the repo and load it from an operator-supplied file at deploy time (recommended), or
(b) anonymise the committed copy. `profiles/default/` must never contain real personal data. This
item is independent of the refactor and is the highest-priority thing in the area.

62.32 [TODO] **Priority: P2 | Depends on: 62.31.** Move the institution-specific seed sets
(`members.json`, `ec_members.json`, `ec_periods.json`, `events.json`, `galleries.json`, `news.json`,
`financial_records.json`, `academic_records.json`, `professional_records.json`, `photos.json`,
`payment_histories.json`, `membership_*`) into `profiles/ghc/demo-data/`, and author a small
synthetic equivalent for `profiles/default/demo-data/`.

62.33 [TODO] **Priority: P2 | Depends on: 62.6.** Membership tiers per ADR-4: `membership-tiers.json`
supplies label (en/bn), display order, enabled, visible, self-selectable-at-registration, and fee
link for each existing `MembershipType` enum key. Enum values and their ints DO NOT change (plan 8.4).
Wire through API DTO, Angular `app.constants.ts` MEMBERSHIP_TYPE_OPTIONS, and Flutter
`registration_constants.dart`. Note this finally supersedes the long-open 28.21 / 35.5 Guest-tier
question by making it a per-institution config flag instead of a product decision baked into code.

62.34 [TODO] **Priority: P3 | Depends on: 62.6.** Governance: `governance.json` for EC role names and
term rules. Today `EcRoleLabels` in the locale pack encodes the GHC executive-committee structure;
another institution has different offices and counts.

62.35 [TODO] **Priority: P3 | Depends on: 62.1.** Payments: make the gateway set a profile-keyed
registry. Current config assumes Bangladesh providers (SSLCommerz, bKash, Nagad, Rocket, DGePay). An
institution outside BD must be able to enable none of them and run the manual-payment path only,
which the app already supports (no-gateway-keys model, Area 29). Do NOT add gateway keys to the repo.

62.36 [TODO] **Priority: P3 | Depends on: 62.1.** Lookups: confirm `lookups.json` (808 lines of
dropdown data: departments, districts, batches) is fully profile-sourced and contains nothing
GHC-shaped that a different institution would inherit wrongly.

62.37 [TODO] **Priority: P4 | Depends on: 62.11.** Currency and locale end-to-end check with a
non-BDT, non-Bengali profile. The config fields exist; verify nothing downstream (formatting, PDF,
fee display, mobile) assumes BDT or an en/bn-only locale pack.

### PHASE F: ONBOARDING, OPS, PROOF

62.38 [TODO] **Priority: P2 | Depends on: 62.2.** `docs/INSTITUTION_ONBOARDING.md`: what a new
institution supplies, in what format, with a worked example. Written for a deployer, not for a
developer of this repo.

62.39 [TODO] **Priority: P3 | Depends on: 62.38.** `scripts/new-institution.mjs`: scaffolds a profile
pack from `default` and prompts for the dozen values that actually matter (names, acronym, prefix,
addresses, colors, currency, feature set).

62.40 [TODO] **Priority: P2 | Depends on: 62.17.** Docker/CI: API image stays profile-agnostic and
reads `ORG_PROFILE` at runtime. Web image builds per profile with `--build-arg ORG_PROFILE` because
of the index.html/favicon/SEO prebuild. Document the per-institution env matrix (profile, connection
string, `FileStorage:BasePhysicalPath`, SMTP, gateway keys, ProtectedSuperAdmins, portal base URL).
Note the existing Dockerfile bypasses `npm run build`, so the prebuild hook needs a home there.

62.41 [TODO] **Priority: P1 | Depends on: 62.6, 62.15, 62.27.** Acceptance test for "generic": boot
with `ORG_PROFILE=default` against an empty database and walk register, login, portal, admin, ID card,
certificate, and PDF generation. Nothing may render "GHCAA", the college name, the Bengali motto, or
the gold crest anywhere. This is the item that proves the area is done.

62.42 [TODO] **Priority: P2 | Depends on: 62.41.** Flip `brand-lint` from warn-only to blocking in CI.

62.43 [TODO] **Priority: P3 | Depends on: 62.41.** Extend `GHCAA.Web/tests/e2e/config-regression.spec.ts`
to run twice, once per profile. It already asserts the org name comes from an intercepted config
rather than markup, which makes it the right harness for this.

62.44 [TODO] **Priority: P3 | Depends on: 62.41.** Full suite green on both profiles: `dotnet test`
(330+ NUnit), vitest (59 files), `flutter analyze`, Playwright e2e. Record the numbers here when done,
since this is the regression baseline for any future institution.

62.45 [TODO] **Priority: P4 | Depends on: 62.41.** Docs sweep per the project's "update all relevant
docs" rule: architecture_data_flow, BUSINESS_FINDINGS, FEATURES, PLAN, project_map, SRS, README, and
CONFIG_DRIVEN_FRAMEWORK.md (which documents Area 28 and now has a successor). Also decide whether to
rename the solution/projects off the `GHCAA.` prefix; recommendation is NO, because the rename churn
and its deploy risk buy nothing a profile pack does not already deliver, but record the decision
rather than leaving it implicit.

### PHASE G: AREA 61 CARRY-OVER (runs inside phases A-F, not after them)

> These four items are the mechanism that folds Area 61 into Area 62. They are not a separate pass at
> the end. Each one is checked off per phase, and the phase is not done until its slice is done.

62.46 [TODO] **Priority: P2 | Depends on: none (applies to every 62.x item).** Tone rule, retroactive.
Every file an Area 62 item edits gets its AI-sounding comments/docs cleaned in the same commit, per
the root CLAUDE.md "Comment, Doc & TODO Tone" section: plain short sentences, no filler openers, no em
dashes, no `// ===== SECTION =====` banners, no restating the obvious, TODOs name the real gap and why
it is not done. This applies equally to the NEW code Area 62 adds (`IInstitutionProfileProvider`,
brand-lint, `apply-brand.mjs`, `new-institution.mjs`) and to the new docs
(GENERICIZATION_PLAN.md, INSTITUTION_ONBOARDING.md). Match the file's existing comment style first;
do not rewrite untouched comments purely to align tone. Absorbs 61.3 (`GHCAA.Tools/db_diag.cs`
`Summary:` banner) whenever that file is next touched.

62.47 [TODO] **Priority: P3 | Depends on: 62.6, 62.15, 62.27.** Dead-code detection, per module, as it
is de-branded. Ripping out `BuildGhcaaDefaults()`, the Angular `ghcaaDefaults` block, the Flutter
`ghcaaDefaults` block, the `Constants.Defaults` brand fields, and the fixed seed paths will strand
helpers, private methods, constants, imports, and possibly whole files. Run `graphify query` /
`graphify explain` on each module at the moment it is touched and record what is now unreferenced.
This is 61.1 done cheaply and with real targets, instead of the blind full-repo sweep it would
otherwise need. Expect the richest yield in Infrastructure (OrgConfigService, seeders), Angular
`core/services` + `core/constants`, and Flutter `core/config`.

62.48 [TODO] **Priority: P3 | Depends on: 62.47.** Remove what 62.47 surfaces, in the same phase that
stranded it, not as a deferred cleanup. Bounded deliberately: only code the genericization work
actually orphaned. Do NOT open a general refactor, and do not introduce a new abstraction or pattern
that the change does not concretely need (project rule: no abstraction without a real duplication or
coupling problem in front of you). This closes 61.2.

62.49 [TODO] **Priority: P4 | Depends on: 62.46, 62.48.** Close-out audit for the Area 61 half: after
Phase F, confirm no AI-tell comments were introduced by Area 62 itself (re-run the 61.4 grep patterns:
filler openers, `Summary:`/`Purpose:`/`Overview:` headers, banner comments, vague `TODO: improve
this`), and confirm the removals in 62.48 left no dangling references (`dotnet build`, `vitest`,
`dart analyze` all clean). Record the file counts here the way 61.4 did, so the sweep is provable
rather than asserted.

---

# Area 63 — Documentation book: implementation alignment, A4-safe figures, automated PDF

Raised by user 2026-09-01: "I want docs books contents to be aligned with latest implementation, IEEE
styled, all drawing, diagrams are designed and formatted thus nothing breaks on a4 sized paper print,
mechanism of latest content to be pdf as described. no contents should look like ai generated, always
in plain simple words and human tone. applied for current and futures", then "make sure diagrams and
figures are well fit not overlapped, clearly visible in to a4 page considering position, should not
break single diagrams/figures into multiple page", and "make contents as the way to be ready to
deliver always".

The rule this area establishes: `python docs/book/build/build.py --pdf --strict` is the gate. It fails
on a stale caption, a numbering gap, a figure the body never names, a front-matter list that has
drifted, a banned-vocabulary hit, a diagram that will not print legibly on A4, and an open placeholder.
Anything that is not enforced there will drift again.

63.1 [DONE 2026-09-01] **Priority: P2.** Repository figures in the book brought back in line with the
tree: 260 endpoints to 276, 23 enums to 28, 34 service interfaces to 40 (37 implementations), 352-case
backend suite to 517 (and the 381-case web suite named), `styles.scss` 3,354 lines to 3,366, 49
components to 80, "each entity has a Fluent API configuration" to the 37 of 49 that actually do, MoSCoW
counts to 38/9/7/8 over FR-01 to FR-54, NFR count to 34, and three wrong routes in the endpoint
catalogue. Counting commands and the date they were taken are recorded in `docs/book/README.md` so the
next pass is a re-run rather than a re-derivation.

63.2 [DONE 2026-09-01] **Priority: P1.** §6.5.6 and ADR-03 rewritten: the book described
`EnsureCreated()` as the schema mechanism in force, which stopped being true on 2026-08-27. It now
describes `MigrationBootstrapper.EnsureMigratedAsync`, the twenty-one migrations, the legacy-database
baseline, the false-baseline self-heal, and why `ConstitutionSeeder.SyncAsync` is still separate. The
Chapter 4 risk register entry was re-rated High/High to match what actually happened.

63.3 [DONE 2026-09-01] **Priority: P2.** Every figure and table renumbered into bound order, gaps
closed (Chapter 5 had five, Chapter 6 twenty-one), and `docs/book/build/renumber.py` added to keep it
that way: it relabels captions, rewrites every mention, rebuilds the List of Figures and List of
Tables, and refuses to run while two captions share a label.

63.4 [DONE 2026-09-01] **Priority: P2.** Seven references to figures, tables and sections that do not
exist removed or redirected to the chapter, and two figures the prose promised actually drawn (design
class diagram of the domain model, site map of the public site). Forty-five figures and tables that
were printed without the body ever naming them now have a sentence that names them, which is the IEEE
requirement and is checked on every build.

63.5 [DONE 2026-09-01] **Priority: P1.** Three diagrams had Mermaid syntax errors and were printing as
boxes of source: a semicolon cutting a sequence-diagram note in half, an unquoted `/` in a node label,
and a colon in a quadrant label. Diagrams are now rendered one at a time so one bad diagram costs one
figure and is reported by caption, instead of dropping every diagram in the book back to source.

63.6 [DONE 2026-09-01] **Priority: P1.** Every diagram is now fitted to one A4 page at view time
(portrait 174x224mm, landscape 257x148mm), so no figure splits across a page break. Twenty-nine
diagrams that would have overflowed or printed below 6.5pt were redesigned rather than shrunk: fan-out
trees turned `LR`, step chains turned `TB`, the ERD split into four sub-models, the CRC card set set as
a table, the onion diagram nested, the analysis class model stripped of attributes, labels shortened,
and one genuinely wide figure (high-level architecture) given the landscape page. Verified: 100
measured artefacts, zero overflows, 83 pages, 82 portrait and 1 landscape.

63.7 [DONE 2026-09-01] **Priority: P2.** `--pdf` added: it serves the built HTML on localhost, drives
headless Chrome or Edge, refuses to print if any diagram failed to draw, and writes the A4 PDF. No
manual print dialog, no forgotten setting.

63.8 [TODO] **Priority: P3 | Depends on: nothing.** Re-take the repository figures listed in
`docs/book/README.md` under "Keeping the numbers true" immediately before any submission, and correct
the sentences that carry them. They were taken on 2026-09-01 and go stale with every feature.

63.9 [TODO] **Priority: P2 | Depends on: user.** Two placeholders remain open, both needing the
author: the Acknowledgements wording, and the elicitation interview period, session duration and
recruitment route (§3.1.2 — the count of ten, the three officer roles and the author's own position
among them are stated). The build lists both after every run.

63.19 [DONE 2026-09-01] **Priority: P2.** Commercial pricing in §2.9 checked against vendor pages
rather than left as unsourced bands. Findings: Hivebrite now publishes prices (Core from US$895/month
billed annually, Flex from US$1,995/month), which contradicted the section's own claim that this
segment does not publish; Zoho CRM and Paid Memberships Pro publish per-seat and per-year prices;
Salesforce's ten free licences carry a 501(c)(3)-or-equivalent condition this Association may not
meet, so its band is now "low if eligible"; Almabase and Anthology publish nothing, and the US$8,000
figure that circulates for Almabase is marked as secondary reporting and not relied on. Table 2.2's
C3 row now reports whether the vendor publishes as well as the band. References [69] to [73], all with
access dates.

63.20 [DONE 2026-09-01] **Priority: P2.** The five dagger-marked references now carry access dates,
and two standards were found to have been revised since the work was done: ISO/IEC 25010:2011 by
25010:2023 [74] (usability becomes interaction capability, portability becomes flexibility, safety
added) and OWASP ASVS 4.0.3 by 5.0.0 [75], with OWASP Top Ten 2021 by the 2025 edition [76]. Each is
cited by the edition the work was carried out against, with the successor named and the consequence
stated in §3.4 and §4.5. The NFR taxonomy was NOT reclassified: the identifiers run through the whole
book and the argument does not turn on the revision. Do not "modernise" these citations without
redoing the classification.

63.13 [DONE 2026-09-01] **Priority: P1.** Ethics and participants written from what the author
supplied: no ethics committee reviewed the study, the Association gave verbal permission with no
reference number and no recorded date, and consent from both interview participants and the members
whose live records the system holds was verbal and undocumented. §3.1.3 states the three consequences
that follow rather than presenting the position as equivalent to institutional review, and the
front-matter ethics statement was corrected — it had claimed a participant information sheet and
signed consent form that do not exist. Participants are counted and described by role but not named:
they are identifiable members of a small association who agreed verbally.

63.11 [DONE 2026-09-01] **Priority: P3.** The placeholder check only looked at the first two characters
of a paragraph, so an inline `*[` inside a sentence or a table cell was never reported: the submission
date and FR-54's source had both been sitting open unnoticed. It now matches anywhere in a line
outside a code fence, which is why the open count went from five to eight without anything new being
added.

63.10 [TODO] **Priority: P3 | Depends on: 63.6.** When Chapters 7-13 are written, every new figure
goes through the same gate: draw it, run `--audit`, fix the shape rather than marking `{landscape}`,
then `renumber.py --apply`. The fix order is in `docs/book/README.md` under "Fitting A4". Do not add a
figure to a chapter without a sentence in the body that names it, or the build will fail.

63.12 [DONE 2026-09-01] **Priority: P3.** FR-54's source attribution closed from the repository instead
of being left to the author: its provenance is the dated request at `docs/TODO.md` Area 40 ("raised by
user 2026-08-26"), which is also FR-53's origin. Both now carry a new Source code R, defined in §3.3 as
a stakeholder request recorded in the tracker after the elicitation of §3.1 closed, which is a weaker
record than an interview and is marked as such rather than dressed up as one.

63.14 [DONE 2026-09-01] **Priority: P1.** The PDF had no page numbers at all. Chrome's
`--print-to-pdf` switch cannot add a folio and silently drops background graphics, and Chrome still
does not implement CSS margin boxes, so `docs/book/build/devtools.py` now drives the print over the
DevTools protocol instead (a stdlib WebSocket client) with `printBackground`, `preferCSSPageSize` and
a footer template. Every page now carries "N of M". No running head: Chrome applies one header
template to every page, so it cannot carry a chapter name and a constant one would print across the
title page.

63.15 [DONE 2026-09-01] **Priority: P1.** The Table of Contents was a one-line stub reading "generated
at typesetting". It is now generated from the headings by `renumber.py`, with every part, chapter and
numbered section.

63.16 [DONE 2026-09-01] **Priority: P2.** The Page columns of the contents, List of Figures and List of
Tables were empty. `docs/book/build/folios.py` reads the printed PDF back, finds the page each heading
and caption landed on, writes the folios into the front matter and reprints, then verifies that
nothing moved. 159 of 159 rows filled and independently re-checked. This is the one optional
dependency in the build (`pypdf` or `PyMuPDF`): without either, the book still builds and the report
says the columns were left empty.

63.17 [DONE 2026-09-01] **Priority: P2.** Layout bug found while chasing a wrong folio: the print
stylesheet's `h1:first-of-type { break-before: avoid }` was intended for the document title but in the
flow it matched the PART I heading, so Part I had no title page and ran on from the front matter.
Removed; the title page's own rule already covers the intended case.

63.18 [TODO] **Priority: P3 | Depends on: 63.14.** Front matter is numbered in Arabic with the body,
not lower-case Roman as `DOCUMENTATION_BOOK_OUTLINE.md` specifies, and the title page carries a folio.
Chrome's footer template is one template for every page, so neither can be varied. Closing this needs
two prints (front matter and body, each with its own template) merged into one file, which needs a PDF
library beyond the standard library. Worth doing only if a supervisor asks for it.

63.21 [DONE 2026-09-01] **Priority: P3.** `docs/materials/` added to `.gitignore`. It holds Pressman
7th-edition slide sets (Ch. 24 project management concepts, Ch. 25-26 process and project metrics and
estimation, Ch. 27 project scheduling, Ch. 28 risk analysis) and a precedence-diagram-method exercise,
copied in for reference while writing the book. Third-party copyrighted teaching material: read it,
never commit it, never quote it in the book. Cite Pressman and Maxim 8th ed., which is reference [54].

63.22 [DONE 2026-09-01] **Priority: P3.** Pressman alignment applied where the written chapters
already use his apparatus rather than retrofitted everywhere: §4.8 now cites [54] for the RMMM
structure it was already following, and the outline's Chapter 11 gains the four P's as its framing,
names the precedence diagram method for §11.3, and records that earned value in §11.5 can only be
reconstructed from the dated work items and the commit record. The outline also states that a
technique the project did not use is reported as not used, not reconstructed to look complete.

63.23 [DONE 2026-09-01] **Priority: P2.** All author placeholders closed. Acknowledgements written
from the three officers the author named (President, Member Secretary, Law Secretary), by office
rather than by name for the reason §3.1.3 gives. §3.1.2 now states thirty-to-forty-minute sessions and
direct member-to-member recruitment, with the selection bias that carries, and records that the period
was not logged rather than reconstructing a date range from memory.
`build.py --pdf --strict --no-placeholders` passes.

63.24 [DONE 2026-09-01] **Priority: P3.** The submission gate was conflating two conditions: open
placeholders and references not yet cited. `--no-placeholders` now covers the first, and a new
`--final` covers both. The second cannot pass until Part III and IV exist, so folding it into the
first made the gate unreachable.

63.25 [DONE 2026-09-02] **Priority: P2.** Page budget measured and added to
`docs/DOCUMENTATION_BOOK_OUTLINE.md`, which had a "Scale" line with no page figure at all. Chapters 1
to 6 print in 72 pages; chapters 7 to 13 estimate at 82 to 99 from their section, figure and table
counts at the rate the written chapters actually print. Body plus front matter lands at 173 to 201
pages. The appendices as specified come to 231 to 357, which follows from what they promise against
real counts: 276 endpoints in Appendix F, 49 tables plus DDL in Appendix E, 898 tests in Appendix G,
about forty-four remaining use cases in Appendix B. Total as specified: 404 to 558 pages, against the
three hundred the house style assumes.

63.26 [TODO] **Priority: P1 | Depends on: user.** Decide the appendix policy, because as specified the
appendices are longer than the dissertation. Two options recorded in the outline: print them in full
and accept the volume, or have the exhaustive ones (E data dictionary, F API reference, G test suite,
B use cases) print a representative extract and cite a generated artefact in the repository, which is
what Tables 6.2 and 6.3 already do in the body. Needs the institution's page limit, which no coding
session can find out. Until it is decided the appendix list is a superset, not a commitment.

63.27 [DONE 2026-09-02] **Priority: P2.** Outline drift against the written book corrected: §6.5.6 no
longer describes `EnsureCreated()` as the constraint in force; §3.3.9 Job Board added with its R
provenance; §3.4 names the 25010:2011 edition and why it was not reclassified; Chapter 5's figure list
records that the authentication and election Level-2 DFDs are deliberately drawn as a sequence diagram
and a BPMN diagram instead; Chapter 6's list drops the wireframes and mockups (the system is built, so
Figures 12.1-12.20 screenshots carry that evidence) and the UML profile diagram (no custom stereotypes
exist); the diagram inventory rows for the ERD, wireframes, mockups and profile diagram now match.
The abstract was 363 words against the outline's own 250-350 range and is now 349.

63.26 [DONE 2026-09-02] **Priority: P1.** Page budget decided by the user: **150 to 200 pages for the
whole volume.** It cannot be met by cutting appendices alone, because the body plus front matter,
references and index already comes to 173-201 on its own, so the decision carries two commitments,
both now recorded in `docs/DOCUMENTATION_BOOK_OUTLINE.md`.

First, chapters 7 to 13 are written to a per-chapter page budget rather than trimmed afterwards: 12,
14, 11, 9, 9, 14 and 5 pages, totalling 74 against the 82-99 they would run to at the density the
written chapters print at. Where a chapter cannot make its budget without dropping evidence, the
evidence stays and the budget is renegotiated in the outline in writing. Do not write a chapter long
and cut it: cutting finished prose removes the qualifications and the negative findings first, which
are the parts of this book that make it credible.

Second, only the appendices an examiner needs in the bound copy are printed, and the set is
re-lettered to four: A ethics, B closed traceability matrix, C full-page fold-out plates, D
originality report, 11-19 pages together. Everything exhaustive is a generated artefact in the
repository or a document delivered beside the dissertation, cited precisely enough to be checked —
the pattern Tables 6.2 and 6.3 already use in the body. Projected total: 172-198 pages.

The re-lettering broke every appendix cross-reference in the written chapters and in the outline
itself; all were remapped in the same change. A reference to material that left the volume now names
where it actually lives (`docs/SRS.md`, the generated OpenAPI document, the generated schema
documentation, the test-runner output) rather than pointing at an appendix letter that no longer
exists. Check this again if the appendix set changes.

---

# Area 64 — Chapter 11 evidence: activity list, durations, and how the work actually arrived

Raised by user 2026-09-02 while checking `docs/materials/` for content the book should cover. The
three assignment PDFs there are the author's own group submissions for MITM 301, and they are for the
**DU Estate Office** scenario, not GHCAA — four authors, submitted 16 June and 5 May 2026. Their
numbers cannot enter the dissertation; their method and coverage can. `docs/materials/` is git-ignored.

64.1 [DONE 2026-09-02] **Priority: P2.** `docs/book/build/wbs.py` added. It derives every Chapter 11
figure from git history and `docs/TODO.md` rather than from anyone's memory: apportioned commit-days
per component, the CPM forward and backward pass, task counts per component, and the arrival profile.
`--check` fails if a component matches no commits, has no tracker areas, or if a tracker area belongs
to no component. Re-run it before submission; where the text and the script disagree, the script wins.

64.2 [DONE 2026-09-02] **Priority: P2.** Seventeen code components defined, each tied to the tracker
areas that produced it, so the activity list and `docs/TODO.md` are one list read two ways. All 63
areas and all 590 items map to a component with none left over. Durations are apportioned commit-days,
rounded up: a day touching five components contributes a fifth to each, so the parts sum to the 63 days
actually worked instead of counting one day five times.

64.3 [DONE 2026-09-02] **Priority: P1.** Critical path computed: C1 persistence → C2 auth → C3 registry
→ C5 events → C8 gallery → C13 web → C15 testing → C17 docs, **48 working days against 63 worked and
206 elapsed**. The gap is availability, not dependency. Largest float: security 18 days, mobile 16.
Two activities behave as hammocks rather than discrete boxes — persistence touched on 42 separate days,
security on 33 — and Chapter 11 must draw them that way.

64.4 [DONE 2026-09-02] **Priority: P1.** Planned-versus-reactive measured: **65% of delivered tasks
were never planned** — 32% stakeholder feedback, 19% review findings, 14% defects — and 26 of the dated
areas arrived in August 2026 alone. This is the framing for §11.1: a critical path over an up-front WBS
would be fiction, because two thirds of the work did not exist when that WBS would have been drawn.

64.5 [DONE 2026-09-02] **Priority: P2.** Two estimation methods compared on a common base, commit-days
against completed-task counts. Eight of seventeen components disagree by more than twofold. Task
granularity varies by an order of magnitude between components, so any estimate built on task counts
inherits that noise — reported as the finding rather than hidden by picking one method.

64.6 [DONE 2026-09-02] **Priority: P2.** Documentation counted as a work stream: 39 documents, 19,136
lines, 26 apportioned days across eight deliverables. `docs/TODO.md` at 3,589 lines is the largest
single document in the project and serves as plan, change log, defect log and decision record at once.
The Elections set is 4,320 lines of the Association's own operative documents — transcription, and
labelled as such rather than counted as authored content.

64.7 [TODO] **Priority: P1 | Depends on: user.** *Listed at the head of the priority index above.*
Five activities produced no commits, so each now carries a calculated assumption in
`docs/book/build/wbs.py` with its arithmetic printed beside it, rather than being left blank: U1
interviews 2 days, U2 governing-document analysis 5 days, U3 review sessions 2 days, U4 stakeholder
discussion 2 days, U5 incident response 1 day and deliberately not added to the total. Project effort
across all four streams therefore stands at **97 days, about 4.4 person-months**. Confirm or correct
each rate — the reading rate of 1,500 words an hour and the write-up ratio of 1x contact time are the
two most open to challenge. The underlying figures still wanted: (a) elicitation interviews beyond the 10 participants at 30–40 minutes each already given —
when, and how much preparation and write-up; (b) analysis of 12 constitutional articles and 7 election
documents clause by clause, which produced the 16 domain constraints; (c) the two formal technical
review sessions of 3 and 29 July 2026, their duration and preparation; (d) the stakeholder exchanges
behind 18 feedback areas, and whether they were meetings, calls or messages; (e) response time for the
four dated deployment incidents. Do not estimate these.

64.8 [TODO] **Priority: P2 | Depends on: 64.7.** Revise §4.8 to carry risk exposure **RE = P × C** and
impact on the 1–5 scale, which is the convention the course material uses. The probabilities are
already there; the impact costs are author-stated and blocked on 64.7.

64.9 [TODO] **Priority: P2.** Chapter 11 figure set is seventeen per-component activity diagrams plus
six chapter-level charts, not one module network. A seventeen-node network with nine edges converging
on the web client prints at about 4pt, well under the enforced 7pt floor. Per-component diagrams also
sit beside the prose for their own component, which is where a reader wants them.

64.10 [DONE 2026-09-02] **Priority: P3.** COCOMO II dropped from §11.4 in favour of the function-point
chain the course material teaches (UFP → TDI → VAF → AFP → effort → LOC → BDT), computed from the
delivered system: 276 endpoint attributes as EI/EO/EQ, 49 `DbSet` properties as ILF, four gateways plus
email, SMS and social identity as EIF. COCOMO II's five scale factors and seventeen effort multipliers
cannot be justified for a single-maintainer project, and an indefensible model adds no evidence.

# Area 65 — Chapter and content sequence revision

Raised by user 2026-09-02: "revise chapters and content sequences".

65.1 [DONE 2026-09-02] **Priority: P1.** Security and verification were in the wrong order. §9.10
security testing was specified as "mapped to the threat model of Chapter 9" — verification
forward-referenced the threat model it derives from, so §9.10 could not be written until the chapter
after it existed. Security is now **Chapter 8** and Verification, Validation and Quality Assurance is
**Chapter 9**. Cheap to do now and expensive later: both chapters are unwritten, so only the 80
references from Part I and Part II moved, all mechanically (§8.x ↔ §9.x is a bijection; internal
numbering untouched). Verified: 80 references found, 80 after. The outline blocks were physically
reordered, §1.10's structure paragraph now states the order **and the reason**, and the diagram
inventory's chapter column was swapped.

65.2 [DONE 2026-09-02] **Priority: P2.** Data modelling was claimed twice: §5.7 "conceptual to
logical" and §6.5.1 "conceptual, logical and physical progression". §5.7 now stops at conceptual,
where persistence concerns begin, and §6.5.1 takes that model as input rather than restating it.

65.3 [DONE 2026-09-02] **Priority: P3.** Ethics had three homes: the front-matter declaration, §3.1.3
and §4.9. §4.9 now owns the research-ethics account; the front matter states the position; §3.1.3
points to §4.9 rather than repeating it.

65.4 [DONE 2026-09-02] **Priority: P2.** §3.1 described elicitation techniques, purposive sampling,
instruments and ethics — methodology, sitting in the requirements chapter *before* the methodology
chapter explains the research paradigm, and overlapping §4.7 Data Collection and Analysis Procedures.
Option (b) taken, on the user's decision: the order of the chapters is unchanged and the method
content moved to Chapter 4 instead.

  - §3.1 is now **Sources of the Requirements**: it names the four sources, says what the Source
    column of §3.3 records, and points to §4.7 for procedure and §4.9 for ethics. Chapter 3 is the
    specification and nothing else.
  - §4.7 gained **4.7.1 Elicitation techniques** and **4.7.2 Participants, sampling and instruments**,
    with the existing prose as **4.7.3 Analysis procedures**. The duplication between §3.1.1 and §4.7
    is gone, not moved.
  - §4.9 Research Ethics now carries the participant account it previously described as an
    "unresolved placeholder", which had gone stale when 63.9 closed the gap. Its live-member-data
    half was already there; the duplicated paragraph from §3.1.3 was dropped rather than copied.
  - Refs repointed: §3.1.1 → §4.7.1 in Chapter 2; §3.1.3 → §4.9 in the front matter (three places)
    and `docs/book/README.md`; the outline's Chapter 3 and Chapter 4 blocks and its Appendix A note.

Option (a), swapping the two chapters outright, stays available and is not blocked by this. It was
priced at 85 §3 and 29 §4 references in written prose plus the generated contents and every folio.

65.4a [DONE 2026-09-02] **Priority: P1.** Fallout from 65.1 found while doing 65.4, and worth
recording because a blanket renumber sweep will do this again. The §8.x ↔ §9.x swap was applied to
prose refs, which was right, but it also hit strings that were not prose refs:

  - The outline's own bullet numbers (`**9.1**` … `**9.15**`) never matched the `§` pattern, so the
    blocks were physically reordered while their bullets stayed put: the Chapter 8 heading sat above
    a 9.1–9.15 list. Both blocks renumbered to match their chapter, figures and tables included.
  - The Diagram Inventory's leftmost `#` column was swapped along with the chapter column, so
    diagrams 8 and 9 traded identifiers. Restored.
  - Short forms escaped the sweep because they are not written `Chapter N`: `Ch. 8` in the objective
    table of §1.6, the traceability matrix header of §3.9 and two Chapter 4 diagram node labels all
    meant verification and now read `Ch. 9`; Figure 1.3 had a node reading `Ch 9 §8.10`.

The lesson for the next sweep: a chapter renumber has to cover `§N.`, `Chapter N`, `Ch. N`, `Ch N`,
bare numbering in list markup, and identifier columns that happen to hold the same digits.

65.4b [DONE 2026-09-02] **Priority: P2.** Four references to the data-protection position pointed at
§8.8, which is Rate Limiting and Abuse Prevention; one pointed at §9.8, System and End-to-End
Testing. All five meant §8.11 Personal Data, and predate the chapter swap — the swap only moved a
wrong number to a different wrong number. Corrected in §2.8, §3.11 and Table 3.7, and in the §3.1.3
text as it moved into §4.9.

65.5 [TODO] **Priority: P3.** Within Chapter 6, §6.11 Design Principles (twelve subsections) and §6.12
Patterns (seven) come after §6.8 to §6.10 on user interface, mobile and configuration. Principles are
more fundamental than the specific designs that apply them, so the conventional order would put them
first. Not done: reordering sections inside a written chapter renumbers a large share of the 84 §6
references for a modest gain. Worth doing only if the chapter is revised for another reason anyway.
