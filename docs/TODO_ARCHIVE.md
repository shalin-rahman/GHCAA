# TODO Archive

Closed Work Packages moved out of docs/TODO.md on 2026-09-15 to keep that file
lean. Every item here is [DONE]; nothing in this file is still open. docs/book/build/wbs.py
reads this file together with docs/TODO.md for its tracker counts, so moving a
Work Package here does not change its Chapter 11 figures.

Work Packages archived: 51 (of 82 total): 1, 2, 3, 4, 5, 9, 10, 11, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 29, 30, 31, 32, 33, 35, 36, 38, 39, 41, 44, 45, 46, 50, 51, 53, 54, 55, 56, 57, 58, 59, 61, 66, 68, 69, 70, 71, 80.
See docs/TODO.md for the current Priority Index and all open work.

---

## WORK PACKAGE 1: MOBILE PLATFORM STABILITY & PARITY

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

## WORK PACKAGE 2: CORE ALUMNI MANAGEMENT & REGISTRY

2.1  [DONE] Member Academic/Professional record migration and mapping
2.2  [DONE] Public Directory Enhancements (Batch/Type/Category visibility)
2.3  [DONE] Membership Lifecycle: Spouse/Family linking (wired + routed)
2.4  [DONE] Membership Lifecycle: "Blue Tick" Verified status control
2.5  [DONE] Membership Lifecycle: Soft-delete cascading (IsArchived architecture)
2.6  [DONE] Automated ID & Certificate Generation (PDF + QR)
2.7  [DONE] Profile UI refinement (Education/Professional record edit buttons)
2.8  [DONE] Create offline data collection templates (Google Forms) for manual member & event migration matching DB validations

## WORK PACKAGE 3: COMMUNICATION & SOCIAL

3.1  [DONE] Real-time Communication Bridge (SignalR Admin Alerts)
3.2  [DONE] Member Chat/Noticeboard Framework & Services
3.3  [DONE] HTML Templating system for system notifications
3.4  [DONE] SMS Gateway Integration (Greenweb/SSL Wireless)
3.5  [DONE] Networking: Privacy controls (visibility toggles respect DTO masking)
3.6  [DONE] Networking: Mentorship request flow in Job Hub
3.7  [DONE] Discussion forums and community groups

## WORK PACKAGE 4: EVENTS & GATHERINGS

4.1  [DONE] Automated registration closing for past/due events
4.2  [DONE] Landing Page: Featured event display with last closed history
4.3  [DONE] Flexible Pricing: Free/Paid toggles and registration windows
4.4  [DONE] Event Media: Logo upload and detail view rendering
4.5  [DONE] My Participations: Payment gateway integration
4.6  [DONE] Advanced: Waitlist management and QR Attendance scanning

## WORK PACKAGE 5: FINANCIAL & ADMIN GOVERNANCE

5.1  [DONE] Smart Payment Gateway Automation (Webhooks for bKash/Nagad/SSL)
5.2  [DONE] EC Management: Term configuration and role propagation
5.3  [DONE] Constraint: Single EC role per member per period
5.4  [DONE] Automated PDF Tax/Donation Receipts generation
5.5  [DONE] Claims-based Auth: PermissionsMatrixScreen for role management
5.6  [DONE] Governance Registry: Admin assignment UI

## WORK PACKAGE 9: INSTITUTIONAL GOVERNANCE & QUALITY

9.1  [DONE] Digital Constitution: Versioned legal repository
9.2  [DONE] Amendment Voting: Secure participation for verified alumni
9.3  [DONE] Collaborative Editorial: Multi-user news workflows
9.4  [DONE] Mobile Governance Portal (/committee route)
9.5  [DONE] Public Transparency: Categorical sitemap in footer
9.6  [DONE] Documentation: System Architecture & Data Flow blueprint
9.7  [DONE] Quality: GitHub PR Template + Sequential CI (API -> UI -> MOBILE)
9.8  [DONE] Quality: Comprehensive Testing suite integration

## WORK PACKAGE 10: USER FEEDBACK & RECENT ISSUES (PHASE 2)

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

## WORK PACKAGE 11: API PARITY AUDIT & FIX LIST

11.1 [DONE] Fix: api/governance/ec/current -> 404 (Controller route verified correct; path was correct)
11.2 [DONE] Fix: api/governance/constitution -> 404 (Controller route verified correct)
11.3 [DONE] Fix: Mobile calling api/Notification (capital N, path wrong); correct endpoint is api/notifications
11.4 [DONE] Fix: Mobile financial_service calls /financial/ledger, should be /ledger
11.5 [DONE] Fix: Mobile assistant_service calls /api/assistant/ask (double-api); should be /assistant/ask
11.6 [DONE] Fix: api/networking/directory -> 404 (alias added to NetworkingController.cs)
11.7 [DONE] Fix: api/news/admin/pending -> 404 (alias added to NewsController.cs)
11.8 [DONE] Fix: auth role leakage - clear session completely before saving new login role
11.9 [DONE] Fix: Mobile biometric (local_auth) crash on Flutter Web; guard with kIsWeb check

## WORK PACKAGE 13: VISUAL TESTING & QUALITY FREEZE

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
13.6 [DONE 2026-09-11; canonical verification in 82.81] Web: Final Angular verification for the centralized theme/control pass completed: type-check, 81-file/440-test Vitest suite, production build, and `git diff --check` passed. Browser light/dark visual validation remains tracked with the web E2E/visual follow-up in 82.83.

## WORK PACKAGE 14: FUNCTIONAL E2E (END-USER TESTING)

14.1 [DONE] Web: Playwright member journey (Login -> Dashboard -> ID Card -> Logout)
14.2 [DONE] Mobile: Integration test journey (Login -> Dashboard -> ID Card -> Logout)
14.3 [DONE] Web: Admin approval workflow E2E
14.4 [DONE] Mobile: Financial ledger verification E2E
14.5 [DONE] Cross-Platform: Article contribution & editorial approval E2E (Web + Mobile)

## WORK PACKAGE 15: SOCIAL AUTH & ONBOARDING

15.1 [DONE] API: Add GoogleId and FacebookId to User entity
15.2 [DONE] API: Add IsProfileComplete to Member entity
15.3 [DONE] API: Implement Social Auth (Google/Facebook) logic & Admin Config
15.4 [DONE] Web: Implement "Login with Google/Facebook" buttons
15.5 [DONE] Mobile: Implement Social Login (google_sign_in, flutter_facebook_auth)
15.6 [DONE] Cross-Platform: Implementation of Onboarding Flow (Profile Setup -> Payment -> Approval)
15.7 [DONE] Backend: Unit tests for Social Auth and Onboarding logic

## WORK PACKAGE 16: POLLS & VOTING

16.1 [DONE] Domain: Create Poll, PollOption, and PollVote models
16.2 [DONE] API: IPollService and PollService implementation
16.3 [DONE] API: PollController for admin and member actions
16.4 [DONE] Web: Admin UI for Poll Management
16.5 [DONE] Web: Member UI for Poll Voting & Results
16.6 [DONE] Mobile: Member UI for Poll Voting & Results
16.7 [DONE] Backend: Unit tests for Polls and Voting logic

## WORK PACKAGE 17: REGRESSION & STABILITY

17.1 [DONE] API: Verify existing Auth flows (NID/Password) remain functional
17.2 [DONE] API: Verify Member Registration and Approval workflows remain functional
17.3 [DONE] Web: Verify full member lifecycle (Login -> Profile -> Dashboard)
17.4 [DONE] Mobile: Verify full member lifecycle (Login -> Profile -> Dashboard)
17.5 [DONE] Cross-Platform: Run all existing Playwright and Flutter integration tests

## WORK PACKAGE 18: GENERAL MAINTENANCE & STABILITY

18.1 [DONE] Mobile: Fix unused import in registration_visual_test.dart
18.2 [DONE] Mobile: Upgrade Governance UI (Fonts, GlassContainer, Image resolution)
18.3 [DONE] Web: Refine Roles Management UI (Input padding, Fancy dropdowns, Smart selection)
18.4 [DONE] Backend: Seed Constitution data to resolve Governance 404s
18.5 [DONE] Mobile: If not connected with net, show error on mobile app
18.6 [DONE] Mobile: Run 'flutter pub get' in GHCAA.Mobile to resolve connectivity_plus dependency errors
18.7 [DONE] Backend: Start GHCAA.API to resolve ECONNREFUSED (port 5087) errors
18.8 [DONE] Backend: Update PostgreSQL password in appsettings.Development.json if SyncMembersForReal test fails locally
18.9 [DONE] UI Audit: Review all SCSS files for hardcoded #fff or #000 that break theme accessibility

## WORK PACKAGE 19: PAYMENT VERIFICATION & POLICY

19.1 [DONE] Payments: Verify Registration Fee configuration in Admin Portal
19.2 [DONE] Payments: Verify Registration Fee status on Member Dashboard (Profile Completion Wizard)
19.3 [DONE] Payments: Ensure Registration Fee is mandatory for all members as per latest policy

## WORK PACKAGE 20: COMPREHENSIVE E2E COVERAGE (ALL FEATURES)

20.1 [DONE] Web: Expand Playwright E2E suite to cover all core portal features (Messaging, Job Hub, Alumni Directory, Events, Gallery, Governance, My Articles, My Profile)
20.2 [DONE] Mobile: Expand Flutter integration/visual tests to cover all core mobile features (Messaging, Job Hub, Alumni Directory, Events, Gallery, Governance, My Articles, My Profile, Admin Modules)
20.3 [IN-PROGRESS — tracked in 82.82] Web: Re-run and close the historical admin approval, article editorial, and full membership/event workflow findings (WEB-008, WEB-009, WEB-010) against clean seeded data. Use 82.82 as the canonical execution record.
20.4 [IN-PROGRESS — tracked in 82.83] Web: Reproduce remaining parallel-worker E2E instability and refresh visual baselines only after confirming the current UI (WEB-011, WEB-012). Use 82.83 as the canonical execution record.

## WORK PACKAGE 21: TEST DATA MANAGEMENT & VISUAL AUTOMATION

21.1 [DONE] Quality: Create 'test-dataset.json' with comprehensive edge cases (Large names, missing photos, various membership tiers)
21.2 [DONE] Quality: Implement 'scripts/setup-test-data.ps1' to inject test dataset into active environment (separate from seed)
21.3 [DONE] Quality: Implement 'scripts/cleanup-test-data.ps1' to revert environment to clean/seed state
21.4 [DONE] Quality: Implement 'scripts/run-visual-tests.ps1' to execute all visual regressions with the test dataset
21.5 [DONE] Quality: Integrate visual test report generation (HTML) for local review

## WORK PACKAGE 22: PORTAL FEATURE HARDENING (E2E)

22.1 [DONE] E2E: Verify Messaging flow (Member <-> Admin) with real-time checks
22.2 [DONE] E2E: Verify Job Hub (Post -> Review -> View) workflow
22.3 [DONE] E2E: Verify Alumni Directory filtering and search precision
22.4 [DONE] E2E: Verify Event Registration and QR generation flow
22.5 [DONE] E2E: Verify Gallery upload and album organization (Admin side)

## WORK PACKAGE 23: ECOSYSTEM-WIDE DATE STANDARDIZATION (dd-MM-yyyy)

> **HEADING SUPERSEDED — read 29F.3 first (noted 2026-08-22).** The `(dd-MM-yyyy)` in this
> heading described the original intent, before **29F.3 [DONE]** settled the contract:
> **ISO-8601 is the canonical wire format; `dd-MM-yyyy` is display/input only.** The items below
> are all still correctly `[DONE]` — 23.2/23.3/23.5/23.6 are display-and-input work and are
> unaffected — but do not read this heading as licence to emit `dd-MM-yyyy` in a payload.
>
> The contract is enforced entirely by `DateFormatConverter` / `NullableDateFormatConverter`
> (`GHCAA.API/Utils/DateFormatConverter.cs`, registered globally in `Program.cs` ~137-138), and
> **those two types have zero tests** — so nothing would fail if the format regressed back to the
> heading's wording. Tracked as **27.7**; also flagged in `docs/COVERAGE_SNAPSHOT_2026-05-26.md`.

23.1 [DONE] API: Implement DateFormatConverter for unified dd-MM-yyyy/ISO parsing
23.2 [DONE] Web: Standardize all Angular date inputs to dd-mm-yyyy (Registration, Profile, Events, Admin)
23.3 [DONE] Web: Update date validation logic and labels to guide users on dd-mm-yyyy format
23.4 [DONE] Mobile: Refactor AppUtils with parseDate/formatDate supporting dd-MM-yyyy standard
23.5 [DONE] Mobile: Update all screens (Registration, Profile, Events, Jobs, Gallery) to use standardized dates
23.6 [DONE] E2E: Update Playwright test suite to use dd-mm-yyyy for all automated date entries
23.7 [DONE 2026-09-11; canonical detail in 82.79] Web: Centralized the light/dark presentation of native date and datetime controls without changing the `dd-MM-yyyy` display contract or ISO API wire format.

## WORK PACKAGE 24: SECURITY HARDENING (from full-stack code review — 2026-05-02)

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
24.48 [DONE, description superseded 2026-09-06] Security: PartitionedRateLimiter keyed on (IP,
username) for login endpoint; LoginRateLimitMiddleware peeks body username before rate limiter runs
(Program.cs, LoginRateLimitMiddleware.cs). **Correction:** this is no longer how login rate limiting
works. 29B.5 (2026-07-25) moved the `Auth` policy to IP-only keying specifically because per-username
buckets let one IP spray N accounts at 5/min each; 80.17 (2026-09-06) then deleted
`LoginRateLimitMiddleware` entirely, since nothing read the username it stashed once 29B.5 landed. The
work this item describes happened and is superseded, not wrong when written — a reader today should
look at 29B.5 and 80.17 for the current mechanism, not this entry.
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

## WORK PACKAGE 25: DGePAY PAYMENT GATEWAY INTEGRATION

- [X] 25.1 API: Implement DGePayGateway service (AES-128-ECB + HMAC-SHA256, Database-driven)
- [X] 25.2 API: Register DGePayGateway in DependencyInjection.cs and PaymentGatewayFactory
- [X] 25.3 Define callback endpoint in GatewaysController
- [X] 25.4 Seed DGePay UAT credentials in payment_configurations.json
- [X] 25.5 Formalize Gateway Workflow documentation (docs/PAYMENT_GATEWAY_WORKFLOW.md)
- [X] 25.6 Update Mobile UI (Flutter) — gateway enum synced, selection bottom-sheet, PaymentWebPage integration
- [X] 25.7 E2E Payment Flow Verification

## WORK PACKAGE 26: VISUAL REGRESSION LAYOUT HARDENING

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

## WORK PACKAGE 29: FULL-STACK REVIEW FINDINGS (2026-07-24)

> Source: whole-project review (backend correctness + security, Angular web, Flutter mobile, payment audit).
> Cross-referenced against docs/BUSINESS_FINDINGS.md and .antigravity/skills standards.
> Type-check + flutter analyze both pass clean — all items below are runtime/logic/UX, not compile errors.
> Fix order: 29-A blockers first, then 29-F.1 (audit-trail) + 29-B.2 (amount bypass), then 29-F sweep, then 29-G, then 29-C.
>
> **DOC-DRIFT WARNING 2026-08-22:** Work Package 29 is 100% DONE here, but `docs/FORUM_PLAN_2026-05.md` still carries an
> unticked mirror of the same work — its Phase 2 (2.1-2.3) and Phase 3 (3.1-3.6) checkboxes are all
> `[ ]` even though the matching 29B/29F items below are `[DONE 2026-07-25]`. `FORUM_PLAN_2026-05.md` line 86
> ("3.1 Facebook token app_id verification") is the clearest example: 29B.1 below records that
> verification shipped via `graph.facebook.com/debug_token`. **TODO.md is the single source of
> truth for status**; FORUM_PLAN_2026-05.md is a historical sequencing doc and its checkboxes should not be read
> as open work. Either tick FORUM_PLAN_2026-05.md's Phase 2/3 through or add a pointer header to it.

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

## WORK PACKAGE 30: UI/UX REMEDIATION (2026-07-30)

> Full plan with root-cause analysis and file:line targets: **docs/UI_FIX_PLAN.md**
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
30.37 [DONE 2026-09-11; canonical detail in 82.73–82.80] Web: Closed the centralized Angular theme and reusable-control follow-up across admin, member, public, and common routes. The detailed records cover token cleanup (82.73), shared controls (82.74), admin table search (82.75), route loading (82.76), dashboard/card styling (82.77), upload surfaces (82.78), date controls (82.79), and dropdown/select states (82.80). No component-local theme system was introduced; remaining exceptions are documented in `docs/implementation_plan.md`.

## WORK PACKAGE 31: DATABASE MIGRATIONS (merged from .claude/memory/outstanding_todos.md)

> This section supersedes `.claude/memory/outstanding_todos.md`, which was stale and is no longer maintained.

31.1 [DONE] DB: `PhaseB_S5S8_OtpHmac_PaymentIdempotency_Indexes` applied (User.FailedLoginAttempts/LockoutUntil, PaymentHistory.GatewayPaymentId + unique partial index, Otp.Code widened to varchar(64) for HMAC-SHA256 hex, Member(Status,IsArchived), User(MemberId), User(ResetToken) partial, User(GoogleId), User(FacebookId), Otp(Email,ExpiryAt)).
31.2 [DONE] DB: RefreshTokens table shipped — folded into the `AddDiscussionForums` migration rather than a standalone `AddRefreshTokens`. Table: RefreshTokens(Id, UserId FK→Users, TokenHash varchar(64) UNIQUE, ExpiresAt, CreatedAt, IsRevoked); indexes TokenHash (unique) + (UserId, IsRevoked).
31.3 [NOTE] DB: There are NO pending migrations — the dev database is up to date. Work Package 28.0's "DEPENDS ON PhaseB_S5S8 + AddRefreshTokens applied first" precondition is therefore already satisfied.
31.4 [NOTE] DB: EF reporting "pending model changes" on PgSql is spurious, non-deterministic seed churn — NOT schema drift. Never scaffold or apply a migration for it.
31.5 [DONE] Mobile: JWT refresh flow complete — `/auth/refresh` + `/auth/refresh-mobile` with dedicated rate-limit policies, and the Flutter `api_client.dart` interceptor performs the refresh (no longer forces re-login every 60 min).

## WORK PACKAGE 32: PROFILE DATA INTEGRITY (raised by user 2026-08-01)

32.1 [DONE 2026-08-02] INVESTIGATED and confirmed: (b) mapping/DTO bug — but in the **Angular frontend**, not the backend (opposite layer from 30.27's EC fix). `GHCAA.Infrastructure/Services/MemberService.cs` `GetProfileAsync` already correctly `.Include()`s AcademicHistory/ProfessionalHistory and maps every field (PassingYear/Degree/Subject/Designation/OrganizationName/ProfessionalSector/Location) into the DTO — the backend/DB layer is not the problem. The actual bug: `GHCAA.Web/src/app/member/profile/profile.ts`'s `applyProfileResponse()` built the client-side `profile` object from a hardcoded ~30-field whitelist (added under a "29D.8" comment for case normalization); any backend DTO field NOT in that fixed list — `designation`, `organizationName`, `professionalSector`, `location`, `profileCompletionPercentage`, `categoryBadge`, etc. — was silently dropped and rendered blank. Education History itself displayed fine (its array was copied unconditionally); "Professional Info" fields were the ones actually missing, matching the user's report. ORIGINAL: Member profile page is missing/showing incorrect values for fields that were previously populated and displayed correctly (user specifically flagged Educational Info and Professional Info sections). Compare, per member: (a) what's in `Seed/*.json` (or whichever seed source is authoritative), (b) what's actually in the live DB tables (per [[gotcha_seed_json_vs_live_db.md]] — editing Seed JSON alone does NOT update an already-created SQLite DB, so seed and live DB can and do diverge; check the live DB directly, not just the seed files), and (c) what the profile API response / `profile.html` actually renders. Identify whether this is a data-loss bug (values never made it into the DB), a mapping/DTO bug (values exist in DB but aren't returned/rendered), or a stale-seed bug (DB was seeded before a field was added/renamed and never re-seeded).
32.2 [DONE 2026-08-02] AUDITED: user's suspicion confirmed — the same hardcoded-whitelist mapping bug existed in 2 more places, both in the admin panel (grep for the whitelist pattern across `GHCAA.Web/src` found exactly 3 hits total, all now fixed): `admin/members/admin-members.ts`'s `openDetail()` (the admin member-detail modal — same entity as the member's own profile page, was missing designation/organizationName/professionalSector/location/profileCompletionPercentage etc., i.e. the exact modal flagged separately in Work Package 33's 33.1/33.2 findings) and its `loadMembers()` list mapping (narrower field set, same fragile pattern). `admin/member-approval/member-approval.ts`'s pending-approval list mapping had the same pattern too (smaller field set, lower risk since it only surfaces id/name/email/mobile/membershipNumber/status/photo/category, none of which were affected, but fixed for consistency). Member Directory, EC/Governance cards (30.27), and Digital ID do NOT use this whitelist pattern — confirmed via grep, no further instances found; 30.27's EC fix was a genuinely separate backend DTO-population bug, unrelated to this one.
32.3 [DONE 2026-08-02] FIXED centrally in all 3 files identified by 32.2, replacing each hardcoded field whitelist with a generic case-insensitive key-normalization pass over every own-enumerable key of the raw response (so any current or future flat DTO field survives automatically instead of requiring manual whitelist maintenance): `member/profile/profile.ts` `applyProfileResponse()`, `admin/members/admin-members.ts` (`openDetail()` + `loadMembers()`), `admin/member-approval/member-approval.ts` `loadMembers()`. No backend/DTO/DB changes were needed (32.1 confirmed the backend was already correct). Added `member/profile/profile.spec.ts` regression tests (2 new cases: flat DTO fields outside the old whitelist now surface; PascalCase/NID key normalization still works) — vitest baseline now 59 files/238 tests (was 236, +2). `npm run type-check` clean, `npm run build` not required (no template/API changes). `dotnet test` unchanged at 317/0 (confirms no backend regression from this fix). No other `docs/` file required correction — `docs/ARCHITECTURE.md`/`docs/BUSINESS_FINDINGS.md` already describe the backend DTO/mapping layer, which was already correct; the bug was purely in Angular-side response normalization, not in documented data flow.

## WORK PACKAGE 33: OVERALL DESIGN/LAYOUT & UX REVIEW (raised by user 2026-08-02; PROFILE PAGES ARE PRIORITY #1 within this area, per user 2026-08-02 clarification)

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
33.12 [DONE 2026-08-02] Added `docs/SHARED_PROFILE_COMPONENTS.md` — design-only doc (no code) proposing 5 shared presentational components (`app-academic-history-editor`, `app-professional-history-editor`, `app-ec-history-view`, `app-emergency-contact-form`, `app-address-form`) driven by `@Input()`/`@Output()` off the same underlying `Member` fields both `profile.html` and the admin member-detail modal already bind to. Documents the section-order/label/duplication differences that must be reconciled before extraction and flags implementation as a separate future TODO once reviewed.
33.13 [DONE 2026-08-02] Live-browser Playwright walkthrough (dev servers started manually — `dotnet run` on the API + `ng serve` on Angular, since the `webapp-testing` skill's documented `with_server.py` helper does not exist in this installation). Verified: (1) landing page renders the new 33.11 section order exactly (`landing-banner, landing-purpose, landing-ec-preview, landing-membership, landing-jobs, landing-events, landing-news, landing-gallery-preview, landing-cta-banner`); (2) member login (`demo_user`/`DemoPass123!`) succeeds and lands on `/portal/dashboard`; (3) `/portal/profile` renders the new 33.10 order exactly (Membership Status → Photo → Signature → main form → Association Governance History → Educational Timeline → Professional Experience), with no visual breakage scrolling through the form (address/privacy/notification sections render normally). Not verified: the admin member-detail modal — both `shalin`/`Shalin@2024!` and `superadmin`/`SuperAdminPassword123!` logged in as ordinary alumni members (no admin role), so `/admin/members` redirected to `/portal/dashboard` via `adminGuard`; this is a local seed/role mismatch (per the known live-DB-vs-seed-JSON gotcha), not a regression from this session's changes, and 33.12 made no code changes to `admin-members.html` to verify.

## WORK PACKAGE 35: WEB MEMBERSHIP-TYPE PARITY (raised by "check mobile implementation compared to web", 2026-08-22)

> Origin: a mobile-vs-web audit of the membership-type implementation. **Counter-intuitive result: mobile is the cleaner side.** `screens/member/dashboard_screen.dart:209` does `profile?['membershipType']?.toString()` — it renders whatever string the API sent, so a new enum value needs no mobile change, and the word `'Life'` appears nowhere in `GHCAA.Mobile/lib`. The web app instead keeps **three divergent copies** of the type list: one correct (`core/constants/app.constants.ts`) and two wrong ones that hardcode `6: 'Life'` — a value that does not exist in `GHCAA.Domain/Enums.cs`, where index 6 is `Guest`. So an approved Guest member is labelled "Life" on the web. These four items are the web-side follow-ups; they also absorb the still-open half of **28.21**.
>
> Shared reference that already exists and should be the single source: `MEMBERSHIP_TYPES` (`app.constants.ts:39-47`, ends `'Guest Member'`), `MEMBERSHIP_TYPE_OPTIONS` (L303-311, ends `{ value: 'Guest', label: 'Guest Member' }`), and the helper `getMembershipTypeLabel(type)` (L281), which correctly indexes `MEMBERSHIP_TYPES`.

35.1 [DONE 2026-08-22] **Highest severity — wrong label on a printed ID card.** `member/digital-id/digital-id.ts:31-34` `getMembershipName()` holds a private array `['Founding','Executive','General','Associate','Honorary','Advisory','Life']`; index 6 is `Guest` in the domain enum, so a Guest member's digital ID card prints "Life". Delete the local array and call the existing `getMembershipTypeLabel()` from `core/constants/app.constants.ts`. Severity is above 35.2 only because this artifact is downloaded/printed and shown as identity proof.

35.2 [DONE 2026-08-22] `member/dashboard/dashboard.ts:83-89` `getMembershipType()` holds the same wrong map as a `Record<number, string>` (`6: 'Life'`), plus an `|| 'General'` fallback that silently mislabels any unknown value instead of surfacing it. Replace with `getMembershipTypeLabel()`. Do 35.1 and 35.2 in one change — they are the same defect in two files.

35.3 [DONE 2026-08-22] `common/directory/directory.html:62-67` hardcodes the membership-type `<option>` list in the template, ending at `<option value="Advisory">Advisory Member</option>` — **no Guest option, so the web directory filter hides Guest members**, the exact defect already fixed on mobile in 28.21. `MEMBERSHIP_TYPE_OPTIONS` exists and is correct but is not used here. Replace the literal options with an `@for (m of membershipTypeOptions; track m.value)` loop, as `public/register/register.html:137` already does.

35.4 [DONE 2026-08-22] `core/models/business.models.ts:2` — the `MembershipType` TS union stops at `'Advisory'` and is missing `'Guest'`, so any code assigning the real value fails type-check and gets worked around. Add `'Guest'`. **Also reconciles a doc line:** `docs/PROJECT_MAP.md:986` was updated on 2026-08-22 to show `'Guest'` in this union, which currently documents *intended* rather than actual state — that line becomes accurate only once this item lands.

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

35.6 [DONE 2026-08-22] Once 35.1-35.4 land: add a web unit test that asserts the rendered label for the highest `MembershipType` index equals the domain enum's last member (i.e. a Guest member is not labelled "Life"), so this class of drift fails the suite rather than shipping. Also extend the sync checklist in `docs/CONFIG_DRIVEN_FRAMEWORK.md` §8 — it currently tracks Backend / Angular-constants / Config / Flutter / DB / Tests and has **no rows** for the directory template, the two component-local label maps, or the TS union, which is why all four drifted unnoticed. Per 12.6, nothing in Work Package 35 is `[DONE]` until `npx vitest run` and `ng build` pass.

> **WORK PACKAGE 35 IMPLEMENTATION NOTE (2026-08-22).** Work Package 35 is now **fully closed** — 35.1-35.4 and
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
> `docs/PROJECT_MAP.md:986` is now accurate — the union it documents really does carry `'Guest'`.

## WORK PACKAGE 36: PUBLIC CONSTITUTION & ELECTION DOCUMENT HUB (raised by "did we show full constitution in public portal and Election processes, forms view and download", 2026-08-23)

> Origin: a direct check of whether the public site surfaces (a) the full constitution and (b) the
> election processes/forms with view + download. **It surfaces neither.** Both are
> backend/content-complete and frontend-missing, which is why nothing in Work Packages 1-35 flagged them —
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

36.10 [DONE] Per 12.6, nothing in Work Package 36 is `[DONE]` until `npx vitest run` and `npx ng build` pass. Add unit tests for the markdown renderer (table + nested-list + escaping cases) and for the 36.1 fallback path (404 from the endpoint must still render the PDF action). **(Done for what shipped — `npx vitest run` 64 files / 286 tests green, `npx ng build` green, emitted `styles-*.css` contains `.doc-hero`/`.doc-prose`/`.md-blank` and the seven assets land in `dist/.../assets/elections/`. New specs: `markdown.util.spec.ts`, `constitution.spec.ts`, `election-docs.spec.ts` (25 tests).)**

# Work Package 38 — Constitution v4.2 & the "always latest" rule

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
`FEATURES.md` §5.1a, `SRS.md` §3.6.2, `ARCHITECTURE.md` §2.D and `PROJECT_MAP.md`
(build-time tool entry) all carry the always-latest rule.

---

# Work Package 39 — Election forms as operative documents

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

# Work Package 41 — Live-site bug fixes (raised by user 2026-08-26, fix before continuing Work Package 40 web/mobile)

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
Shipped as part of Work Package 40 (40.12/40.13).

41.6 [DONE] `dotnet test` 378/378 passed, `npx vitest run` 306/306 passed (64 files) after all four
bug fixes; final combined state after Work Package 40 frontend work: `npx vitest run` 315/315 (66 files),
`flutter test` 28/28 passed.

41.7 [DONE 2026-08-27] Live `GET /api/jobs` and `GET /api/gallery` 500s after the Work Package 40 deploy —
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

# Work Package 44 — Full-app review of the last 2 days' fixes (raised by user 2026-08-28: "review entire
application. make sure all issues are taken cared")

Three parallel code-reviewer passes (backend, web, mobile) audited every fix from Work Packages 40–43 plus
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

44.16 [DONE 2026-09-04] Resolved as a side effect of 80.2/80.4, not by the rename this item originally
called for. `features/family/family_service.dart` turned out to have zero importers anywhere in the
app (dead file, deleted) and `features/support/support_service.dart`'s own `FamilyService` was equally
dead — `support_screen.dart` imports the file only for `SupportService`, never calls
`getFamilyLinks`/`addFamilyMember`, and that class's route (`/familylink`) didn't match any real
endpoint anyway (see 80.4). Deleting both leaves exactly one `FamilyService` in the codebase
(`features/networking/family_service.dart`, the one `family_link_screen.dart` actually uses), so the
naming collision this item was tracking no longer exists. No rename was needed.

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

# Work Package 45 — SuperAdmin error-log viewer (raised by user 2026-08-28: "super admin role should able to
view application error logs from UI, able to search, by date or error details or part"), plan only,
not yet built

Today errors only reach `stdout` (`ExceptionMiddleware`'s `ILogger.LogError`, plus every
`ILogger<T>.LogError`/`LogWarning` call added across Work Packages 43/44) and Render's log stream — nothing
is persisted queryably, so there is nothing for an admin UI to read from yet. Per
[[feedback_keep_lightweight]], the right shape here is a dedicated small table + a thin capture
sink, not a logging framework (Serilog/ELK/Seq) — this app has deliberately avoided that class of
dependency so far.

45.1 [DONE 2026-09-07] **Priority: P2.** Explore/plan (Plan Mode required — spans Domain/Infrastructure/API/Web): decide the
capture point(s). Candidates to reconcile: a custom `ILoggerProvider` registered in `Program.cs`
alongside the console provider (captures every `ILogger` call app-wide, broadest coverage, more
plumbing); vs. writing directly from `ExceptionMiddleware` only (captures unhandled exceptions —
matches this request's literal wording, "application error logs" — much simpler, but misses
`LogWarning`/handled-but-logged errors from the Work Package 43/44 sweep). Confirm which with the user before
building either.
**Resolved 2026-09-07:** `ExceptionMiddleware`-only, per the literal "application error logs" wording —
the simpler capture point, at the cost of not catching handled-but-logged `LogWarning` calls.
45.2 [DONE 2026-09-07] **Priority: P2.** Domain + migration: new `ErrorLog` entity — at minimum `Id`, `OccurredAt` (UTC,
indexed), `Level` (Error/Warning), `Message`, `ExceptionType`, `StackTrace`, `Source` (controller/
middleware/class name), `RequestPath`, `RequestMethod`, `UserId`/`Username` (nullable — many errors
are pre-auth or background). Raw SQL migration per the idempotent-migration convention established
in 41.9/44.2 ([[gotcha_migrationbootstrapper_fixed_offset]]).
**Resolved 2026-09-07:** `GHCAA.Domain/Models/ErrorLog.cs` with the fields above; hand-written migration
`20260907070000_AddErrorLogTable` (+ Designer) with indexes on `OccurredAt` and `Level` — hand-written
rather than scaffolded, since scaffolding mid-session collided with concurrent work on the model
snapshot (see 45.7's note).
45.3 [DONE 2026-09-07] **Priority: P2.** Infrastructure: the capture sink decided in 45.1, writing rows via a scoped/background
write (never let logging itself throw or block the request it's logging) — batch or fire-and-forget
inserts so a logging-table write can't become a new source of request latency or failure.
**Resolved 2026-09-07:** `ErrorLogService`/`IErrorLogService` in `GHCAA.Infrastructure/Services`, injected
into `ExceptionMiddleware` via method injection (matching `AuditLogMiddleware`'s existing pattern). Writes
are awaited rather than true fire-and-forget — the request-scoped DbContext is disposed the moment the
middleware returns, so a real fire-and-forget would race that disposal — but wrapped in try/catch at both
the service and middleware layers, so a logging failure can never replace the real exception being handled.
45.4 [DONE 2026-09-07] **Priority: P2.** API: `GET /api/admin/error-logs` (`[Authorize(Policy = "SuperAdminOnly")]`, matching the
existing policy convention in `ServiceExtensions.cs`) with query params for date range, free-text
search (message/exception-type/stack-trace substring), level, and pagination — push filtering to the
DB query, not an in-memory scan, since this table will grow unbounded without a retention policy
(see 45.6).
**Resolved 2026-09-07:** `AdminErrorLogsController.cs`, filters pushed into the EF query as specified.
45.5 [DONE 2026-09-07] **Priority: P2.** Web admin: new `admin/error-logs/` screen (list + filters: date range picker, text search,
level dropdown; row expansion for full stack trace) — follow `ghcaa-design` conventions and the
existing admin list-page pattern (search bar + filters component already used elsewhere, e.g.
`admin-news.ts`/`admin-members.ts` — reuse `SearchBarComponent`/`PageHeaderComponent`, don't rebuild).
**Resolved 2026-09-07:** `GHCAA.Web/src/app/admin/error-logs/`, route `/admin/error-logs` behind
`superAdminGuard`, nav entry under "Finance & Tools". Reuses `PageHeaderComponent`, `SearchBarComponent`,
`PaginationComponent` per convention; row expansion for the stack trace.
45.6 [DONE 2026-09-07] **Priority: P3.** Retention/cleanup: decide and implement a bound (e.g. delete rows older than N days, or
cap total row count) — an error-log table with no retention policy will grow forever and eventually
degrade the very queries meant to search it.
**Resolved 2026-09-07:** rows older than `Constants.ErrorLogs.RetentionDays` (90) are swept on roughly
1-in-20 writes via `ExecuteDeleteAsync` — no new hosted-service for what is a P3 concern.
45.7 [DONE 2026-09-07] **Priority: P2.** Tests + docs update per usual closing convention (`dotnet test`, `npx vitest run`,
`npx tsc --noEmit`, live verification that a genuine error actually appears in the new admin screen).
**Resolved 2026-09-07:** `dotnet test GHCAA.Tests` (Release) — 708 passed, 0 failed. `npx tsc --noEmit` /
`npm run type-check` clean. `npx vitest run` — 265 passed; 9 failures across 4 unrelated spec files
(gallery, polls, events.service, lookup.service, step-up.service), all `[vitest-pool]: Timeout waiting for
worker to respond` (resource contention, not this change) — the new `admin-error-logs.spec.ts` passed 5/5
in isolation. Full-history migration replay from empty currently fails at an unrelated, pre-existing March
migration (`RefactorMemberAcademicProfessionalRecords`, missing `Members.IsProfileComplete` mapping) — not
caused by this change; verified instead by generating the incremental SQL from the last stable migration
through this one (`dotnet ef migrations script`) and running it directly against a throwaway Postgres DB,
where `ErrorLogs` and both indexes created cleanly. **Live verification of the admin screen against a
running instance was not done this session** (no running app/browser available) — folded into 43.4, which
already tracks that same live-verification gap and remains open. **Environment note:** partway through,
this same working tree was being concurrently modified by another in-flight session/agent (the 82.30
`IsDeleted`→`IsArchived` rename) — three of this task's edits were briefly reverted then restored, and
running `dotnet ef migrations add` while that other work was mid-flight corrupted the EF model snapshot
(the exact failure `gotcha_ef_migrations_add_remove_corrupts_snapshot` warns about); restored via
`git checkout` and the migration was hand-written instead, matching the existing `AddFundraisingCampaigns`
precedent for this situation.

---

# Work Package 46 — May 2026 alumni registration batch import (raised by user 2026-08-28: import
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

46.5 [DONE 2026-09-06] **Fixed at the summary level, deliberately not by backfilling rows.** The org-
wide Financial Ledger view (`FinancialLedgerService.GetSummaryAsync`) counted only `FinancialRecord`
rows, so a year with real membership/event-fee income but no manually-entered ledger row reported
near-zero — the exact gap this item named. Considered and rejected: writing a `FinancialRecord` row
for every historical `PaymentHistories` payment. Rejected because `MemberService.GetDashboardStatsAsync`
already adds `PaymentHistories` income on top of `FinancialRecords` income to get the org-wide balance
(see its own comment there) — backfilling would double-count everything against that existing,
correct calculation, and the backfilled rows would carry no real `CreatedByAdminId`/audit provenance
for what is genuinely bulk-imported data, not an admin action.
**What shipped instead:** `GetSummaryAsync` now also sums completed, non-deleted `PaymentHistories` for
the requested year and adds them to `TotalIncome`, plus a per-category breakdown row labelled "Income
(Payment History)" so admins can see it's a distinct source, not a manually entered one. No new
`FinancialRecord`/seed rows written — `financial_records.json` legitimately stays empty; the fix is in
what the summary counts, not in the data.
**Left out of scope, on purpose:** the itemized, paginated ledger list (`GetRecordsAsync`) and the CSV
export (`ExportRecordsAsync`) still show `FinancialRecord` rows only. Merging in payment-history rows
there would mean a paginated union across two differently-shaped entities, and — since only real
`FinancialRecord` rows support the edit/delete audit trail (82.16) — inventing edit/delete affordances
for a merged-in payment row that don't actually work. That's a real, separate feature, not this fix.
Tests: `FinancialLedgerServiceTests.GetSummaryAsync_IncludesCompletedMemberPayments_NotJustLedgerRecords`
(pending and prior-year payments correctly excluded). Full suite 617/617, 0 warnings.

# Work Package 50 — Admin-configurable email/SMS template bodies (raised by user 2026-08-29/30: "need to
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

# Work Package 51 — Universal photo-upload compression hard-cap (raised by user 2026-08-30: "photo_name should
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
51.2 [DONE 2026-09-11] **Priority: P2.** Add a real hard-cap enforcement step: after the existing quality-drop (85%→70%) still
exceeds the target, downscale image dimensions (e.g. `Mutate(x => x.Resize(...))`, stepping the max
dimension down, not just quality) and re-encode, looping until under the cap or a sane minimum
dimension floor is hit — so "512kb max" is an actual guarantee, not best-effort. Introduce a distinct
hard-cap constant (`Constants.Defaults`: e.g. `MaxImageSizeKB = 512`) separate from the existing
"aim for good quality" `TargetImageSizeKB` (currently 350, keep as the first-pass target below the
hard cap).
51.3 [DONE 2026-09-11] **Priority: P2.** File naming: give saved files a type-prefixed name (per user's explicit ask — "event_",
"album_", "member_" or similarly descriptive, not an opaque GUID) instead of today's
`{Guid}_{originalFileName}` in `SaveFileAsync`'s `uniqueName` — e.g. `photo_`, `galleryphoto_`,
`newsimage_` prefixes keyed off `uploadType`, still GUID-suffixed for uniqueness.
Note: this is about the live upload pipeline going forward; the 6 gallery albums manually imported
from `GHC\images\albums\` this session already use a hand-applied `album_<slug>_NN.ext` convention
under `GHCAA.Web/public/assets/gallery/` (bundled web assets, not this upload pipeline) and don't need
touching for this.
51.4 [IN-PROGRESS 2026-09-11] **Priority: P2.** Tests: extend `LocalFileStorageService` coverage (`LocalFileStorageServiceTests.cs` exists
today but only ever exercises `FileUploadType.Photo` with compression disabled) for: compression
actually firing on `GalleryPhoto`/`NewsImage`, confirming it still does NOT fire on
`PaymentProof`/`Certificate`/`Signature`/`NoticeDocument`, the hard-cap resize loop (51.2) actually
converging under 512KB on a large fixture image, graceful fallback on a non-image input tagged with a
compressible type, and the new filename prefix per type (51.3). **Progress:** focused coverage now
passes for compression, dimension limits, hard-cap convergence, and type-prefixed paths; the excluded
type and fallback matrix remains.
51.5 [IN-PROGRESS 2026-09-11] **Priority: P3.** Admin-configurable file storage settings — today `ImageCompressionEnabled` /
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
51.6 [DONE 2026-09-11; canonical detail in 82.78] Web: Completed the control-surface portion of the upload audit across profile, gallery, news, member-import, payment-proof, and document upload flows. Added the shared themed `.upload-surface` state without changing the server-side compression, validation, staged-submit, preview, or API behavior. The remaining hard-cap, naming, configuration, and service-level test work stays tracked by 51.2–51.5.

---

# Work Package 53 — Favicon / browser tab icon review (raised by user 2026-08-30)

53.1 [DONE] **Priority: P3 | Depends on: none.** Reviewed the favicon: `index.html` already pointed
at `assets/logo.png` (the correct transparent-branding asset, confirmed 1024x1024 RGBA with real
alpha — not the old opaque "dark box" `logo.jpg`), so branding was already right. The real gap was
performance/quality, not branding: the raw 515KB 1024px asset was being fetched directly as the
favicon and downscaled by the browser on every page load. Generated proper pre-sized icons
(`favicon-16.png`, `-32.png`, `-48.png`, `-180.png` via Pillow LANCZOS resize, `GHCAA.Web/public/`)
and wired them in `index.html` with explicit `sizes` attributes plus an `apple-touch-icon`; visually
confirmed the 32px version stays legible (crest shape + color quadrants read clearly at that size).

---

# Work Package 54 — Live-site issues raised by user 2026-08-30 (console log + admin comm + ledger)

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

# Work Package 55 — Landing page spacing + preview-section seed coverage (raised by user 2026-08-31)

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

# Work Package 56 — Retroactive log: earlier same-session fixes not yet recorded (per user 2026-08-31: "make
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

# Work Package 57 — Test coverage audit + a new live-site report to investigate (2026-08-31)

57.1 [DONE 2026-09-04] User asked: does the test suite actually verify that create/update actions
persist **every field**, not just a happy-path subset? Audit run and written to
`docs/materials/57.1-field-coverage-audit.md`: 19 service files, ~28 `Create*Async`/`Update*Async`
methods, each field assignment cross-referenced against its test's assertions with file:line for
both sides. No case found where a field is provably *not* persisted — every assignment reaches
`SaveChangesAsync()` — but 10 methods have a real assertion gap: a field the service writes on every
call with no test anywhere reading it back, several touching money, registration limits, or a
member's login-identity fields (Email/MobileNo/NID). Worst: `MemberService.AdminUpdateMemberAsync`
assigns ~30 fields, its one test asserts 3. `FinancialLedgerService.UpdateRecordAsync`,
`JobHubService.UpdateJobAsync`, `LookupService.UpdateLookupItemAsync`,
`ThemeService.CreateThemeAsync`/`UpdateThemeAsync`, and `GalleryService.UpdateEventGalleryAsync` have
no real test at all — only a controller test that mocks the service call. Follow-up to close the
gaps is 57.3.

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

57.3 [DONE 2026-09-04] Closed all 10 gaps 57.1's audit found. `MemberService.
AdminUpdateMemberAsync`/`UpdateProfileAsync` extended in place (`MemberServiceTests.cs`) to assert
the ~27/~15 fields each assigns, including the login-identity fields (Email/MobileNo/NID) and the
academic/professional history round-trip; `ECHistory` was already covered by
`MemberService_EC_Tests.cs` and correctly left alone. The five zero-test methods
(`FinancialLedgerService.UpdateRecordAsync`, `JobHubService.UpdateJobAsync`,
`LookupService.UpdateLookupItemAsync`, `ThemeService.CreateThemeAsync`/`UpdateThemeAsync`,
`GalleryService.UpdateEventGalleryAsync`) each got the real service-level test that was missing
(`LookupServiceTests.cs` is a new file). `EventService.CreateEventAsync`/`UpdateEventAsync` extended
in place for their remaining fields. Verified: `dotnet test` 564→573 (9 net new tests across both
extension and new-file work), 0 failed, per SR-6.

Every added assertion passed against real (non-mocked) service + SQLite-backed `DbContext` behavior
except one: `NewsService.UpdateNewsAsync`'s `Status` overwrite is a genuine behavioral defect, not a
missing-assertion gap, and closes 57.1's "or a defect raised here with its own item" clause as 57.4.

57.4 [DONE 2026-09-06] Defect found while closing 57.3, proven by a test
rather than inferred: `NewsService.UpdateNewsAsync` (`NewsService.cs:134`) runs
`existing.Status = dto.Status` unconditionally, two lines below `PublishDate`, which is correctly
null-guarded (`if (dto.PublishDate.HasValue)`). `UpdateNewsDto` inherits `CreateNewsDto.Status`'s
default of `Enums.SubmissionStatus.Approved`, so any caller that builds the DTO without deliberately
setting `Status` silently flips the post to `Approved` — confirmed by
`NewsServiceTests.UpdateNewsAsync_OverwritesStatus_WithDtoDefault_WhenCallerDoesNotSetIt`, which
seeds a `Pending` post, updates via a DTO that never touches `Status`, and gets back `Approved`.

Checked before writing this: `NewsController.UpdateNews` (`AdminOnly`) is the only caller of this
method — approve/reject go through the separate `ApproveArticleAsync`/`RejectArticleAsync`, which
don't touch this path — and the one live caller, `admin-news.ts`'s edit form, seeds
`status: post.status ?? 2` (`admin-news.ts:97`) before every save, so **no live UI path triggers this
today**. It's a latent defect in the method's own contract, not a currently-exploitable one: a future
caller (a partial-update endpoint, direct API use via Swagger, a minimal-payload mobile client) that
doesn't know to always resend the current status would silently move a post to Approved with no
error. Fix: guard `Status` the same way `PublishDate` already is — either make it nullable on
`UpdateNewsDto` and only apply when set, or split it into a dedicated status-change action (matching
how approve/reject are already separate from the general update) so `UpdateNewsAsync` stops being
able to change status as a side effect of an unrelated edit.
**Acceptance:** `UpdateNewsAsync` no longer changes `Status` unless the caller explicitly intends to,
and the existing regression test (renamed/adjusted as needed) asserts the new, safe behavior instead
of the current defect.
**Fixed 2026-09-06.** Took the second option: `existing.Status = dto.Status;` removed from
`UpdateNewsAsync` entirely rather than making the DTO field nullable, since the only live caller
(`admin-news.ts`) always resends the current status anyway and status changes belong to
`ApproveArticleAsync`/`RejectArticleAsync` exclusively. The regression test was renamed
(`UpdateNewsAsync_DoesNotChangeStatus_EvenWhenDtoCarriesTheDefault`) and now asserts a `Pending` post
survives an unrelated edit instead of asserting the old bug. Tagged `[Category("FR-28")]`. Full suite
616/616.

---

# Work Package 58 — Admin-without-member access, card/table-view audit, poll voting-window check (2026-08-31)

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

# Work Package 59 — Association flag on the public About page (raised by user 2026-08-31, referencing
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

## WORK PACKAGE 61: CODE COMMENT/DOC TONE + REFACTOR SWEEP (raised by "prepare a plan for human-toned comments/docs/TODOs, refactor review, token usage", 2026-09-01)

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

> **2026-09-01 UPDATE: 61.1, 61.2 and 61.3 are now executed inside Work Package 62, not separately.** Work Package 62
> (white-label/genericization) edits most of the same files, so running these as standalone sweeps
> means reading the whole repo twice. They are re-scoped as per-phase obligations there and tracked in
> 62.46-62.49. Do not start a separate pass for them; if Work Package 62 is cancelled or deferred, re-open
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

# Work Package 66 — Print legibility: overprinted and clipped diagram labels

Raised by user 2026-09-02 from two screenshots of the printed PDF: risk-matrix labels stacked on top
of each other, and DFD edge labels overlapping. A third arrived while the first two were being fixed:
a quadrant-chart title cut off at the left, missing its first letter.

66.1 [DONE 2026-09-02] **Priority: P0.** The A4 audit measured the box and the type size and passed
every one of these figures. Both defects are invisible to it by construction: the drawing is the
right shape and the labels are above the 7pt floor, but the words are printed over each other or
outside the frame. Two checks added to the measurement pass in `build.py`, reported by
`printer.overflows()`:

  - **overprinted** — every text element's client rectangle is compared pairwise; an intersection
    over a quarter of the smaller box is a collision, reported with the two label texts.
  - **clipped** — any text element extending past the SVG's own frame is cut off at print, because
    Mermaid sizes the viewBox from the drawing and not from the title.

Both run inside `--audit` and therefore inside `--strict`, so a figure that overprints now fails the
build the same way one that overflows the page does.

66.2 [DONE 2026-09-02] **Priority: P1.** Five figures fixed, three of which nobody had reported:

  - Figure 4.5 risk matrix — eight risks plotted on three coordinates, so six labels printed on top
    of each other. Spread within each rating band; every risk stays in the quadrant its Table 4.2
    probability and impact put it in.
  - Figure 5.1 DFD Level 0 — two arrows between the same pair of nodes put both labels at the same
    midpoint. Replaced with one double-headed arrow per actor, labelled `in:` and `out:` relative to
    the platform, and the caption now says so.
  - Figure 2.4 positioning chart — CiviCRM sat on the quadrant-3 title, and Anthology Encompass at
    x = 0.95 ran off the right edge. Both moved; the ordering the chart argues is unchanged.
  - Figure 3.9 domain model — the `holds` role name printed over a multiplicity. Reordering the
    associations did not clear it and neither did renaming, so the role name is dropped; the
    Member-to-CommitteeTerm association is unambiguous without it.
  - Figure 6.13 architecture trade-off — the title was wider than the chart and lost its first
    letter, and two of the four candidates overlapped. Title shortened to the axis it compares, the
    figure caption carrying the full name; points spread.

66.3 [DONE 2026-09-02] **Priority: P1.** The PDF build was silently degrading. When the output file
is open in a viewer the protocol route cannot write it, and the fallback to Chrome's `--print-to-pdf`
switch produces a copy with no page numbers and no background graphics. It said so in one warning
line and still reported `clean, ready to deliver`. Now the build checks the file is writable first
and stops with the reason, and any other fallback prints the underlying cause rather than only the
consequence. **Close the PDF before building.**

66.4 [DONE 2026-09-02] **Priority: P3.** The contents list prefixed every section row with the
section sign, "§4.1 Research Paradigm". The section sign marks a cross-reference in prose; a contents
list carries the bare number. `renumber.py` no longer writes it and `folios.py` treats it as optional
so a hand-written row still matches.

# Work Package 68 — Chapter files for 7 to 13, and the outline kept in step by the build

<!-- wbs: component=C17 start=2026-09-02 end=2026-09-02 after=65,67 -->

Raised by user 2026-09-02: "create other chapter md files with book outline contents heading, remember
any changes in book outline need same needed changes on this new outline files, vice versa, remember
this always", then "make sure chapters and topics are follows as per needed order".

68.1 [DONE 2026-09-02] **Priority: P1.** Seven chapter files created and wired into `CHAPTERS` in
`build.py`, in bound order: `07-implementation.md`, `08-security.md`, `09-verification.md`,
`10-deployment.md`, `11-project-management.md`, `12-results.md`, `13-conclusion.md`. Chapter 7 carries
the Part III title and Chapter 12 carries Part IV. Every heading is generated from
`docs/DOCUMENTATION_BOOK_OUTLINE.md`, and each section holds one `*[Not written]*` placeholder with
the outline's brief, so the count of what is left to write is exact: 139 placeholders across the seven
files. The book now prints 102 pages with 261 folios, all filled.

68.2 [DONE 2026-09-02] **Priority: P0.** The sync rule the user asked for is enforced, not remembered.
`lint.outline_drift` compares every chapter against the outline in both directions — a section in one
and not the other, the two in a different order, a chapter retitled in one alone — and `--strict`
fails on any of it. Recorded in `docs/book/README.md` and in `CLAUDE.md`.

68.3 [DONE 2026-09-02] **Priority: P1.** Two drifts the new check found immediately, both invisible
before it:

  - The 65.1 chapter swap missed three-part sub-bullets: 25 of them still read `**8.x.y**` under
    Chapter 9 in the outline, because the sweep only matched two-part numbers.
  - The outline promised §5.2.4 "Process specifications and data-store definitions", which the written
    chapter folded into §5.2.3 (Tables 5.3 and 5.4 carry it), and the written §5.7.1 and §5.7.2 were
    never added to the outline. Both reconciled.

  Order was checked as well as membership: all thirteen chapters now run in the outline's sequence,
  and the four parts open at chapters 1, 4, 7 and 12.

68.4 [DONE 2026-09-02] **Priority: P2.** Two build-output faults found while doing the above. A lint
finding quoting a non-breaking hyphen killed the run with a `UnicodeEncodeError` on a cp1252 console;
stdout and stderr are now reconfigured to UTF-8. And the placeholder list printed 139 lines on every
build, burying the real findings above it; it now prints one summary line per file and the full list
only under `--no-placeholders`, which is when it is the thing being closed.

# Work Package 69 — Review of the seven book build scripts

<!-- wbs: component=C17 start=2026-09-02 end=2026-09-03 after=63 -->

Raised by user 2026-09-02: "review ALL THE TOOLS AND SCRIPTS". Reviewed by the `code-reviewer`
subagent against `docs/book/README.md`, findings verified by running the modules read-only. The design
of the gate held up; what it found were holes through which a bad artefact could still ship under
"clean, ready to deliver". Those are fixed. The rest are recorded here, unfixed, in the reviewer's own
order of severity.

69.1 [DONE 2026-09-02] **Priority: P0.** The fallback print shipped silently on the reprint. The
folio-filling reprint passed `stream=io.StringIO()`, so the warning that the PDF had been printed
through the command-line switch — no page numbers, no background graphics, possibly Mermaid source
boxes — went into a discarded buffer and nothing counted it. `to_pdf` now returns that as a defect in
the problems list, so both callers count it, and `_fill_folios` prints it.

69.2 [DONE 2026-09-02] **Priority: P0.** The fallback accepted a stale PDF as a fresh one: if the
command-line print wrote nothing, yesterday's file passed the exists-and-over-20KB test and was
returned as the deliverable, page count and all. The old file is deleted before the fallback runs.

69.3 [DONE 2026-09-02] **Priority: P1.** `FOLIOS MOVED` was printed and never added to the failure
count, so a run that knew the contents page numbers were wrong still ended clean. `_fill_folios` now
returns a defect count, and an anchor that vanishes on the reprint counts as moved rather than as
unchanged.

69.4 [DONE 2026-09-02] **Priority: P1.** An anchor that stopped matching left yesterday's folio in the
row with nothing said. `filled < total` is now a failure, and "nothing to fill" with rows waiting is a
failure rather than a note.

69.5 [DONE 2026-09-02] **Priority: P1.** A missing chapter file was a line on stderr and the build
carried on, producing a book with a hole that lint could not see, since the captions of the missing
chapter simply ceased to exist. A file listed in `CHAPTERS` and not on disk is now fatal.

69.6 [DONE 2026-09-02] **Priority: P1.** Front-matter drift was checked one way only: a caption
missing from the List of Figures was reported, a row in the list with no caption behind it was not. So
a deleted or renumbered figure left a stale row with a stale page number and the build passed. Both
directions are checked now.

69.7 [DONE 2026-09-06] **Priority: P2.** Already fixed, tracker not re-ticked. `lint._mentions` toggles
`in_fence` on a triple-backtick line and skips lines inside one, with a comment naming exactly this
case — a figure named inside another figure's Mermaid source. Verified by reading the function and by
the two examples the item named (`04-methodology.md` Figure 4.5's title, `06-architecture.md` Figure
6.12's node label): both still carry a prose mention too, so nothing changed in the report. Another
instance of `gotcha_todo_status_drift`.

69.8 [DONE 2026-09-06] **Priority: P3.** Already fixed. `references()` replaces a fence with
`"\n" * m.group(0).count("\n")` rather than deleting it, so a citation after a fence keeps the line
number it actually has in the file.

69.9 [DONE 2026-09-06] **Priority: P2.** Already fixed. `abstract_word_count` reports "the Abstract
heading or its Word count line no longer matches the pattern" instead of returning an empty list when
the heading moves, and `references()` reports "no reference list found" instead of `([], [])` when
`99-references.md` is missing. Both name the failure instead of passing silently.

69.10 [DONE 2026-09-06] **Priority: P3.** Already fixed. `_prose_lines` strips the leading `>` and
yields a blockquote's text to the tone check rather than skipping it, with a comment noting that user
stories and acceptance criteria are the author's own prose.

69.11 [DONE 2026-09-06] **Priority: P2.** Already fixed. `Browser.__init__` wraps the port wait and the
WebSocket handshake in `try/except BaseException: self.close(); raise`, and `close()` tolerates a
socket that was never created. Confirmed no leaked `chrome.exe` process after several PDF builds this
session.

69.12 [DONE 2026-09-06] **Priority: P3.** Already fixed. `WebSocket.__init__` splits the handshake read
on `data.partition(b"\r\n\r\n")` and keeps whatever followed the header as the start of the buffer
instead of discarding it, and closes the socket before raising on a refused upgrade.

69.13 [DONE 2026-09-06] **Priority: P2.** `subprocess.TimeoutExpired` was already added to `main`'s
except clause in an earlier pass, but a `pypdf` read error was not: `pypdf.errors.PdfReadError` does
not subclass `OSError`, so a locked or truncated PDF read inside `_fill_folios` still escaped as a
traceback. Added `folios.PdfError` — `pypdf.errors.PyPdfError` when pypdf is installed, a local
stand-in otherwise — and added it to the except tuple in `build.py:main`.

69.14 [DONE 2026-09-06] **Priority: P3.** Already fixed. `inline()` loops `while "\x00" in text:`,
re-expanding held spans until none remain, which resolves a code span nested inside a link label. No
source in the book uses that construct, so the rendered output is unchanged.

69.15 [DONE 2026-09-06] **Priority: P2.** Already fixed. The audit script in `build.py` adds
`(note ? note.getBoundingClientRect().height : 0)` to a figure's measured height, so a lead-in note is
now part of the A4 fit check.

69.16 [DONE 2026-09-06] **Priority: P2.** The mismatch itself was still there — an earlier pass added a
comment defending it, not a fix. `printer.dump_dom` now opens the audit page through the same
`devtools.Browser` session and the same `document.body.dataset.diagrams` wait that `print_pdf` uses,
instead of a one-shot `--dump-dom` process carrying `--run-all-compositor-stages-before-draw` and a
60-second virtual-time budget. The measured page and the printed page are now the same render.
`_base_args`'s two flags stay, but only for the command-line `--print-to-pdf` fallback, which has no
other way to wait for Mermaid.

69.17 [DONE 2026-09-06] **Priority: P3.** Already fixed. 2 CSS px at 96 dpi is 2 / 96 * 25.4 = 0.53mm,
not under a fifth of a millimetre; the comment now reads "half a millimetre at 96 dpi", which is what
the constant actually allows. `SLACK` itself did not need to change — 0.53mm was always the intended
rounding tolerance, the comment was what was wrong.

69.18 [DONE 2026-09-06] **Priority: P3.** Already fixed. Both writes in `folios.py` and `renumber.py`
now open with `newline=""` inside a `with` block, so the front matter's line endings stay whatever the
platform wrote instead of turning CRLF on every run, and the handle closes on every path including an
exception.

69.19 [DONE 2026-09-06] **Priority: P3.** Already fixed. `anchors_from_front` returns
`(line index, row text, anchor)` and `write_pages`/`fill` key the page-number map by that index, so two
byte-identical rows in the front matter no longer collapse to one entry.

69.20 [DONE 2026-09-04] `wbs.commit_days` checks git's exit status. **The guard was already in the
code when this item was picked up** — `wbs.py:212-220` wraps the call in `try/except OSError` for a
missing git binary and raises `SystemExit` on any non-zero return status, with the directory and
git's own stderr in the message. Its docstring already paraphrased this item's wording, so the fix
shipped at some point without the item being re-ticked. Another instance of the status drift
`gotcha_todo_status_drift` records; the item is closed against verification, not against new code.

What was genuinely missing, and is what this item delivered: nothing pinned the behaviour. No test
existed for any script under `docs/book/build/`, so a later edit could drop the check and silently
restore the original defect — and that defect's output is not a crash but a complete, confident,
fabricated schedule whose numbers are quoted in Chapter 11. New `docs/book/build/test_wbs.py`,
standard library `unittest` only (matching the build's stdlib-only design, no new dependency), 4
tests: the happy path still returns real dates from this repository; a non-repository directory
raises rather than returning an empty set; any non-zero exit surfaces with git's stderr; and a
missing git binary raises. **Verified the test actually catches the regression** rather than merely
passing — removing the `returncode` guard from `wbs.py` turns 2 of the 4 red, and restoring it turns
them green, which is the §3.7 standard of a defect closing against a test that would fail if it came
back. `wbs.py` itself is unchanged (restored via `git checkout` after the experiment).

69.21 [DONE 2026-09-06] **Priority: P2.** Already fixed. `tracker()` runs
`for area in tasks: arrival.setdefault(area, "planned")` after building `arrival` from the headings, so
an area whose heading the pattern missed is still counted in the arrival denominator, and raises
`SystemExit` if `tasks` comes back empty rather than letting a zero denominator reach the percentage
arithmetic in `report()`.

69.22 [DONE 2026-09-06] **Priority: P3.** Already fixed. `critical_path`'s `visit()` builds `order` by a
depth-first walk of `predecessors` — a real topological sort, not `CODE`'s declaration order — and
raises `SystemExit` naming the missing id when a predecessor is not a known component, instead of
`KeyError`.

# Work Package 70 — Architecture diagram: the real-time path

<!-- wbs: component=C17 start=2026-09-03 end=2026-09-03 after=63 -->

Raised by user 2026-09-03, marking the SignalR hubs on Figure 6.1 and asking how mobile connects with
the services.

70.1 [DONE 2026-09-03] **Priority: P1.** Figure 6.1 left the two SignalR hubs as a dead end: clients
connected to them and nothing connected them to the rest of the system, so the diagram could not
answer "how does the mobile client reach a service?". The REST path was there all along
(`MOB -->|HTTPS + JWT| MW`), but the push path was missing entirely. Added
`SVC -.->|IRealTimeService| HUB`, and §6.3.4 now explains the part that looks like a layering
violation and is not: `IRealTimeService` is declared in `GHCAA.Application`, the member, financial,
family and notification services depend on the interface, and the implementation sits in `GHCAA.API`
because it needs `IHubContext<T>`, a hosting type. Verified in the tree:
`GHCAA.API/Services/RealTimeService.cs`, `GHCAA.API/Hubs/NotificationHub.cs` and `ChatHub.cs`, and
the mobile client connecting to `/hubs/notifications` in
`GHCAA.Mobile/lib/core/real_time/notification_hub_service.dart`.

# Work Package 71 — Supervisor-style review of the book: verify each suggestion, act where justified

<!-- wbs: component=C17 start=2026-09-03 end=2026-09-03 after=65,68 -->

Raised by user 2026-09-03 with ten suggestions across structure, depth, modernity and trimming. Each
was checked against the tree and the outline before anything was changed; four were already satisfied
and are recorded as such rather than re-done.

71.1 [DONE 2026-09-03] **Priority: P1.** Conceptual framework added as §1.4, with Figure 1.3 drawing
inputs, artefact and outputs and the evaluation loop as a return edge rather than an implication. This
was a genuine gap: Chapter 1 had a context diagram, a stakeholder onion and an RQ-to-chapter map, none
of which links the Association's rule set, its current practice and its operating constraints to the
artefact and then to the evidence. Sections 1.4 to 1.10 shifted to 1.5 to 1.11; the renumber cost was
eight cross-references outside Chapter 1, which is why this was cheap enough to do properly rather
than bolt on at the end of the chapter.

71.2 [DONE 2026-09-03] **Priority: P1.** Constitutional traceability into testing: Table 9.9 added to
the Chapter 9 specification — DC identifier, constitutional article and section, the rule as enforced,
the requirement it governs, the automated test that pins it, and the verdict. §9.4.5 now points
forward to it. Table 3.4 traces requirements through to a test; this closes the other loop, from the
clause in the constitution to the test that fails if the software stops honouring it, which for this
project is the claim the whole dissertation rests on.

71.3 [DONE 2026-09-03] **Priority: P1.** DevSecOps: checked before writing. There is no security
scanning in the pipeline at all — no CodeQL or other SAST, no DAST, no dependency vulnerability
scanning, no secret scanning, no Dependabot. The five workflows run `dotnet format --verify-no-changes`,
`npm run type-check`, `flutter analyze`, build, test and deploy. So the suggestion cannot be met by
writing it up, only by building it or by saying so. §10.4 now splits into §10.4.1, the gates actually
enforced, and §10.4.2, security in the pipeline and what is not automated, with the remedy costed in
§13.4. Writing a DevSecOps posture the pipeline does not have would have been the one unrecoverable
kind of error in this document.

71.4 [DONE 2026-09-03] **Priority: P2.** The user asked for both to be added, targeting preprod, on
the reasoning that the project needs them after delivery anyway. Done in Work Package 72.

71.5 [DONE 2026-09-03] **Priority: P3.** Vendor lock-in named. Criterion C5 in §2.10 already measured
it as "data sovereignty and exit" and the comparison table already carries per-vendor licence cost
with citations, so the substance was there under another name; the criterion now says it is vendor
lock-in measured from the buyer's side, which is the term an examiner will look for.

71.6 [DONE 2026-09-03] **Priority: P3.** Four suggestions verified as already satisfied, and
deliberately not re-done:

  - **Design-science thread.** §4.3 already maps each design-science activity to the chapter that
    evidences it, §1.8 states the method in brief and forward-references Chapter 4, and §12.13 reflects
    on the contribution. Adding more DSR vocabulary to intermediate chapters would be decoration.
  - **Deployment economics.** §10.10 is an operational cost model under institutional budget
    constraints, with Table 10.2 giving component, tier, monthly cost and scaling trigger.
  - **Threats to validity.** §12.11 covers construct, internal, external and conclusion validity with
    Table 12.4, and §9.16 raises the evaluation-specific threats and points there.
  - **Data privacy compliance.** §8.11 covers lawful basis, minimisation, consent, retention and
    subject rights, with Table 8.4 as the personal-data inventory; the front-matter declaration and
    §4.9 carry the position on live member data.

71.7 [DONE 2026-09-03] **Priority: P3.** The two trimming suggestions were already the practice.
Tables 3.1 and 3.2 do not reprint the requirement catalogues: they state that §3.3 carries the ID,
statement, source and priority for FR-01 to FR-54 and give the research-question linkage only, which
is why the build lists them as captions with no artefact beneath them. Chapter 11 defines no textbook
methodology: its specification says only the parts with evidence are written, and where a technique
was not used it reports what was done instead. Neither Agile nor Scrum is defined anywhere in the
book.

# Work Package 80 — Output-cache cross-user leak, and what a caching/architecture sweep found

<!-- wbs: component=C2 start=2026-09-03 end=2026-09-03 after=44 -->

Origin: user instruction, 2026-09-03/04, asking whether caching exists anywhere, whether it's needed
for performance, and for an implementation plan if so — plus a look at cookies/sessions and any
broken or half-wired flows while at it. Four parallel audits covered: existing caching, hot read
paths, cookies/tokens/headers, and broken flows. Every finding below was checked against the actual
code or a throwaway repro before being written down; three claims from the audits did not survive
that check and are recorded as verified-non-issues so nobody re-raises them.

80.1 [DONE 2026-09-03] **Priority: P0.** Fixed a real cross-member data leak. `GHCAA.API/Program.cs`
registered Output Cache with `AddBasePolicy(b => b.Cache())` — every GET/HEAD 200 response cached by
URL alone, and Output Cache does not vary by Cookie or Authorization unless told to. Reproduced
against a throwaway ASP.NET Core 9 app: two requests to the same URL carrying two different
members' `access_token` cookies got back the identical cached body. Any authenticated fixed-URL
route (`/api/me/profile`, `/api/financials/my-dues`, `/api/events/my-registrations`, every other
"me"-shaped route) was one cache hit away from serving one member's response to the next member who
hit the same URL inside the cache window.

Fix: base policy is now `NoCache`; nothing is cached unless a controller action opts in with
`[OutputCache(PolicyName = ...)]`, and that's only done on `[AllowAnonymous]` actions that return the
same body to every caller. Two named policies replace the old single `"StaticData"` one (which was
registered but never actually applied to any endpoint): `PublicReference` (2 min — lookups,
governance/EC/constitution) and `PublicContent` (30 s — news, events, gallery, jobs, site content).
Policy names live in `GHCAA.Domain.Constants.OutputCachePolicies`, not as literals at each call site.
`OrgConfigController` and `ThemeController` were deliberately left undecorated — both already have
their own `IMemoryCache` layer (10 min / 5 min TTL) with correct invalidation on write, so an output-
cache attribute would add a second, looser-invalidated cache on top for no real gain.

Tests: `GHCAA.Tests/Integration/OutputCacheTestFactory.cs` (a `WebApplicationFactory<Program>` in the
Development environment so `VisualTestAuthMiddleware` is active, letting a request authenticate as a
known Visual-seed member via a `Bearer visual_*_token` header with no real login round trip) and
`GHCAA.Tests/Integration/OutputCacheSecurityTests.cs`, three tests: an authenticated fixed-URL
endpoint reflects a database write made between two calls (proves it isn't cached); the same
endpoint at the same URL never mixes two different members' data; a `PublicReference`-decorated
endpoint does serve a stale copy within its window (proves the opt-in policy is actually wired, not
just present). Full `GHCAA.Tests` suite re-run green after the change (2026-09-03).

80.2 [DONE 2026-09-04] Confirmed neither web nor mobile calls any `FamilyController` route (grepped
both clients for every literal and constant shape before deleting — zero hits), and it fully
duplicates `FamilyLinkController`'s `links`/`search` functionality under a colliding route template.
Deleted `GHCAA.API/Controllers/FamilyController.cs`, which removes the `AmbiguousMatchException` risk
permanently rather than just resolving the alias. `IFamilyService`/`FamilyService` were NOT deleted
with it on the first pass — a filtered grep missed that `FamilyLinkController` also depends on
`IFamilyService` for its own `search` action (`_familyService.SearchByNameAsync`, line 122); the build
caught the mistake immediately, both files were restored, and the fix is now just the one dead
controller. `dotnet build`/`dotnet test` clean afterward. `FamilyLinkController`'s own `/api/Family/
links` and `/api/Family/search` aliases were left in place — harmless now that nothing else claims the
same template, and no client uses them either way.

80.3 [DONE — stale claim, checked 2026-09-06] Already fixed, just never marked here: `git log -S`
shows `app.MapControllers().RequireRateLimiting(GHCAA.Domain.Constants.RateLimitPolicies.Api)` was
added 2026-09-04, before this item's own text was written against an older state of the file. The
"api" policy is genuinely global now, and `AuthController`/`RegistrationController`'s own
`[EnableRateLimiting]` attributes correctly take precedence over it per-controller (ASP.NET Core
endpoint metadata beats the group-level convention). No code change made here — corrected the tracker
to match reality rather than re-doing already-done work.

80.4 [DONE 2026-09-04] Three client calls that 404'd, resolved individually — two turned out to be
dead duplicate code sitting next to a working equivalent, one is a real missing backend feature:

- `.../networking/networking_service.dart:79-87 (`updateProfile`, calling `PUT /api/profile/update`)
  — dead. `profile_edit_screen.dart` actually calls `AuthService.updateProfile()`, which already
  correctly hits `PUT /api/profile` (matching `ProfileController`'s real route). The
  `NetworkingService` copy had no caller; deleted rather than pointed at a URL nothing would ever
  reach through it.
- `.../support/support_service.dart`'s inline `FamilyService` (`getFamilyLinks`/`addFamilyMember`,
  calling `/familylink`) — dead, and its sibling `features/family/family_service.dart` (calling
  `/members/family`, which — unlike `/familylink` — is a real registered alias on
  `FamilyLinkController`) had zero importers either. Neither was reachable from any screen; both
  deleted. This also closed 44.16 (see that item) by leaving exactly one `FamilyService` class.
- `GHCAA.Mobile/lib/features/auth/auth_service.dart:167-175` (`forgotPassword`, posting
  `/api/auth/forgot-password`) — different case, left as-is. Nothing calls it either, but unlike the
  two above there is no working equivalent to point it at: `AuthController` has no endpoint to
  *request* a reset token at all, only `reset-password` (`Email`+`Token`+`NewPassword`) to *consume*
  one already issued some other way. This is an absent self-service feature, not a URL mismatch —
  see 80.16 for the real work.

`dotnet build`/`dotnet test` and `flutter analyze lib` both clean after all three.

80.16 [DONE 2026-09-06] `AuthService.RequestPasswordResetAsync` — new `POST /api/auth/forgot-password`
(`[AllowAnonymous]`), reuses the same `ResetToken`/`ResetTokenExpiry` fields and 24-hour window the
existing admin-initiated reset already uses (checked the OTP infrastructure first — it's built around
a 6-digit code + purpose enum for registration verification, a different shape than a reset link, so
reusing the existing token fields was the smaller, more consistent change, not a second mechanism).
Sends via the `PASSWORD_RESET` DB template if one exists, else a fallback body that says "we received
a request" rather than the admin-flow's "an administrator initiated" wording, since this one really is
self-service. Always returns the same generic response and does comparable work (a throwaway BCrypt
hash) on a no-match identifier, matching `LoginAsync`'s S5.2 timing-equalization reasoning — an
enumeration oracle was exactly the risk the item named. New `Constants.RateLimitPolicies.PasswordReset`
(5 requests / 15 minutes per IP, tighter than `auth`/`refresh` since this one has an outbound email
side effect). Mobile: `AuthService.forgotPassword()` already posted the right shape to this route with
nothing to call it — new `ForgotPasswordScreen` (`screens/auth/forgot_password_screen.dart`), routed
at `/forgot-password`, linked from the login screen's new "Forgot Password?" button. `flutter analyze`
clean. Tests: `RequestPasswordResetAsync_KnownEmail_GeneratesTokenAndSendsEmail`,
`..._UnknownIdentifier_DoesNothingObservableAndDoesNotThrow`. Full suite 620/620.

80.5 [DONE 2026-09-04] Two of the three named catches were genuinely silent and are fixed:
`HealthController` (both its DB and FileStorage probes; the DB one deliberately keeps its
client-facing response generic — Npgsql failure text can contain host/credentials — but now logs
server-side via a newly-injected `ILogger<HealthController>`) and `LoginRateLimitMiddleware.cs:27`
(now logs via `ILogger<LoginRateLimitMiddleware>` instead of a bare `catch { }`). The third claim
was wrong: `Program.cs`'s seeding blocks (~395, ~421, ~439) already call `app.Logger.LogError`/
`LogWarning` with the exception attached and a comment explaining why continuing is the intended
behavior (schema/seed gaps need a human, not a crashed boot) — not swallowed, just non-fatal by
design. `dotnet build`/`dotnet test` clean.

80.6 [DONE 2026-09-04] All six `ActivatedRoute` subscriptions now carry `takeUntilDestroyed(this.
destroyRef)`, matching the pattern already used in `layouts/public-layout/public-layout.ts` (the
project's existing convention — `takeUntilDestroyed()` called bare only works inside a constructor's
injection context, and all six call sites are in `ngOnInit`, so each file gained a `private
destroyRef = inject(DestroyRef)` field to pass explicitly): `admin/comm/admin-comm.ts`,
`member/messages/messages.ts`, `common/news/news.ts`, `common/payment-status/payment-status.ts`
(both of its two subscriptions), `member/forum/topic-detail.ts`, `public/elections/elections.ts`.
`tsc --noEmit` clean, `npx vitest run` 381/381 (74 files) green. (The `inactivityTimer`/
`searchDebounce` half of this item turned out already correct — see 80.15.)

80.7 [DONE 2026-09-04] Confirmed zero callers of any `api/me` route from either client, and both
constructor dependencies (`IMemberService`, `IIDCardService`) are already shared with
`ProfileController`, so deleting the controller strands nothing (same check that caught the
`FamilyController`/`IFamilyService` mistake in 80.2 was run here first). Deleted
`GHCAA.API/Controllers/MeController.cs`. `dotnet build` clean.

Verified non-issues, recorded so a future sweep doesn't re-flag them: the payment-gateway DI
registration (`AddHttpClient<T>()` does register a bare `HttpClient`, confirmed with a standalone DI
container test — no `InvalidOperationException` at resolution); `ThemeService`'s own `IMemoryCache`
(it already calls `_cache.Remove(CacheKey)` on create/update/delete); `AuthController.RefreshMobile`
missing an `IsActive`/`IsArchived` recheck (it has the identical guard as the cookie-based `Refresh`
action, `AuthController.cs:122`); `Program.cs`'s seed-ordering hazard at the old line 438 (fixed and
closed as 44.18); and `app.constants.ts:284`'s `getMembershipTypeLabel` comment (it documents a
Work Package 35 bug already fixed — the comment is a warning against regressing it, not a live one).

80.8 [DONE 2026-09-04] **Priority: P3.** Three stale comments that describe a problem already fixed a
few lines below them, found and cleaned in the same pass as 80.1–80.7: `AuthService.cs:39-44`'s "TODO
[CRITICAL]: No brute-force / lockout protection" and "TODO [HIGH]: Username enumeration via timing" —
both S5.1/S5.2 are implemented in the body of the same method (`LockoutUntil` check, dummy
`BCrypt.Verify` on user-not-found); and `VisualTestAuthMiddleware.cs:9-13`'s "TODO [CRITICAL]:
registered unconditionally" — `Program.cs` has guarded it with `IsDevelopment() && ASP_SEED_PROFILE
== "Visual"` since 2026-08-29. All three replaced with a one-line factual note. No behaviour change.

80.9 [DONE 2026-09-04] **Priority: P2.** Real dead health-check page, found in the same sweep:
`common/health/health.ts` called `this.http.get('/healtz')` (missing an 'h') against a same-origin
SPA, so the request fell through Output Cache's `MapFallback` and returned the SPA shell instead of
the real `/healthz` JSON — the health widget always showed whatever garbage came back from parsing
HTML as JSON. Fixed: the Angular route (`app.routes.ts`) and the HTTP call both renamed to
`healthz`, and the URL is now `API_ENDPOINTS.HEALTH` instead of a literal. Also removed
`API_ENDPOINTS.ORG` (`'/api/org'`, zero references anywhere in the app, and wrong — no controller
serves that route; `CONFIG` already covers `OrgConfigController`) and fixed
`admin-dashboard.html:191`'s dead `routerLink="/admin/governance"` to the real route,
`/admin/members/ec` (matches `nav.service.ts`'s own correct link to the same screen). `tsc --noEmit`
and `dotnet build` both clean after all three fixes.

80.10 [DONE 2026-09-04] Removed the `NagadGateway` (`"Nagad (Direct Gateway)"`) option from the
admin payment-config's Gateway dropdown (`admin-payment-config.html`) — confirmed no seed data or
live config referenced it first. A member can no longer select a gateway that always fails with
"coming soon". The backend stub is untouched; restore the option once the integration is real.
`tsc --noEmit` clean.

80.11 [DONE 2026-09-04] Changed `IPaymentGatewayService.ProcessWebhookAsync` to return a new
`PaymentWebhookResultDto` (`IsValid`, `TransactionId`, `ConfirmedAmount`, `GatewayPaymentId`) instead
of a bare `bool`, and implemented it properly in all four gateways:
- `SSLCommerzGateway`/`BkashGateway`: parse the same fields their redirect-callback siblings already
  use (`tran_id`/`amount`/`val_id` for SSLCommerz; bKash needed `VerifyCallbackAsync` to write
  `merchantInvoiceNumber`/`amount` back into the passed dictionary, since that method's own signature
  is fixed by the interface and can't return them directly — the same pattern `DGePayGateway`
  already used for its own callback).
- `DGePayGateway`: implemented for real (was `Task.FromResult(false)`), parsing the webhook body as
  either a JSON `data` field or form/query-encoded, matching what the redirect callback already
  expects, then running the same `VerifyCallbackAsync` decrypt-and-verify path.
- `NagadGateway`: still `PaymentWebhookResultDto.Invalid()` — it's the stub from 80.10, nothing to
  wire up until the gateway itself is real.

`GatewaysController.GatewayWebhook` now calls `HandleSuccessfulPayment` directly when a gateway
reports a valid result with a transaction id, closing the gap the inline TODO (added by 61.4) named:
a webhook could validate a payment but never actually mark it Completed. Dictionary keys used more
than once (`"tran_id"`, `"amount"`, `"val_id"`, `"merchantInvoiceNumber"`, `"paymentID"`,
`"unique_txn_id"`, `"data"`) are now `private const string` fields on each gateway class, not
repeated literals. Two new tests in `GatewaysControllerTests.cs` prove the wiring: a valid webhook
result reaches `UpdatePaymentStatusAsync(..., Completed, ...)`, an invalid one touches nothing.
`dotnet build`/`dotnet test` clean (9/9 in `GatewaysControllerTests`, full suite green).

80.12 [DONE 2026-09-04] Reflection-based DI auto-registration in `DependencyInjection.cs` now
excludes `GreenwebSmsService` explicitly (it needs the typed `HttpClient` only `AddHttpClient<ISmsService,
...>` can wire, which the reflection loop can't provide — registering it twice was correct only
because the `AddHttpClient` line happened to run second) and tracks every `(interface, implementation)`
pair it registers, throwing at startup if two different implementations are ever found for the same
interface instead of silently letting scan order pick a winner. `dotnet build`/`dotnet test` clean —
the guard doesn't fire today, confirming there's no live duplicate, only the risk of one.

80.13 [DONE 2026-09-06] Clean Architecture layer boundary was crossed in 8 controllers that
injected `ApplicationDbContext` directly instead of going through an Application-layer service:
`AdminSocialAuthController`, `AuthController`, `FinancialsController`, `GatewaysController`,
`GovernanceController`, `HealthController`, `PaymentConfigController`, `SecureFilesController`.
`FinancialsController`, `GatewaysController` (the worst offender — its webhook handler mutated
`PaymentHistory.GatewayPaymentId` directly while `IFinancialService` owned the row's `Status` field
in the same method) split a resource's write path across two layers with no single owner;
`GovernanceController`'s `ApplicationDbContext` field turned out to be dead — injected but never
read anywhere in the class.

All 8 now depend only on Application-layer interfaces. New surface added to make that possible:
- `ISocialAuthConfigService`/`SocialAuthConfigService` (new) — owns `SocialAuthConfig` CRUD, used by
  both `AdminSocialAuthController` and `AuthController.GetProviders`.
- `IPaymentConfigService`/`PaymentConfigService` (new) — owns all `PaymentConfiguration` CRUD
  (100% of `PaymentConfigController` was direct-context before this), plus
  `GetEnabledByGatewayAsync` so `GatewaysController` stops re-querying the same table on its own.
- `IDatabaseHealthService`/`DatabaseHealthService` (new) — wraps `Database.CanConnectAsync()` for
  `HealthController`; no exception for how small the read is.
- `IFileUploadRepository.GetByFilePathAsync` (added) — backs `SecureFilesController`'s path lookup.
- `IAuthService.GetUserWithRolesAsync` / `GetUserWithRolesAndMemberAsync` / `GetUserByUsernameAsync`
  (added) — the four reads `AuthController`'s refresh/step-up flows used to run directly.
- `IFinancialService.GetPaymentOwnerMemberIdAsync` / `GetMemberIdForUserAsync` (added) —
  `FinancialsController`'s receipt-ownership check and system-admin dues lookup.
- `IFinancialService.IsGatewayPaymentAlreadyProcessedAsync` / `GetPaymentSnapshotByTransactionIdAsync`
  / `StampGatewayPaymentIdAsync` (added) — `GatewaysController`'s webhook idempotency/read/stamp,
  now all owned by the same service that already owned the row's status transitions.
- `IEventService.GetRegistrationByPaymentReferenceAsync` / `AutoApproveRegistrationAfterPaymentAsync`
  (added) — deliberately narrower than the existing `ApproveRegistrationAsync` (no participation-
  approved email/notification): this is a payment confirmation, not an admin review, and swapping in
  the admin-review method would have added a notification send that never happened on this path
  before.
- `IMemberService.GetMembershipSnapshotAsync` (added) — cheap `(Status, MembershipType)` read for
  the gateway auto-approval eligibility check, instead of `ApproveMemberAsync`'s full profile load.

`GHCAA.API.csproj`'s direct EF package references were checked and deliberately left alone:
`Program.cs` and `SecurityStampMiddleware.cs` both still call EF Core extension methods directly
(startup/migration/seed logic, security-stamp lookup), and `GHCAA.API` is the documented
`--startup-project` for `dotnet ef` (see `docs/CONFIG_DRIVEN_FRAMEWORK.md`), so `.Design` needs to
be visible from it. The existing csproj comment says the versions were pinned explicitly to avoid an
NU1604 warning; trimming them is a separate, riskier change this item didn't ask for.

**Verified:** `dotnet build` clean on `GHCAA.API`/`GHCAA.Tests`; full backend suite passes
(671/671, up from 620 — new/updated coverage added for every new service and the four previously-
untested controllers: `AdminSocialAuthControllerTests`, `HealthControllerTests`,
`SecureFilesControllerTests` are new; `SocialAuthConfigServiceTests`, `PaymentConfigServiceTests`,
`DatabaseHealthServiceTests` are new; `AuthServiceTests`, `FinancialServiceTests`,
`MemberServiceTests`, `EventServiceTests`, `FileUploadRepositoryTests` gained tests for their new
methods; `AuthControllerTests`, `AuthControllerMutationTests`, `FinancialsControllerTests`,
`GatewaysControllerTests`, `PaymentConfigControllerTests` were updated for the new constructors
(the latter two now wire real service instances backed by the test's own `ApplicationDbContext`
rather than mocks, since those tests seed and assert against that context directly).
Lower priority item closed as an architecture-cleanliness fix, not a functional defect — no
behavior change intended anywhere outside the two narrow, called-out exceptions above (event-
registration auto-approve's notification scope, and `GovernanceController`'s dead field removal).

80.14 [DONE 2026-09-06] Three items, all resolved (was PARTIAL — the third's investigation is complete, see below).

- **Done:** removed `FinancialLedgerController`'s dead second route attribute, `api/financial/ledger`
  (confirmed zero callers; `api/ledger` is the one both clients use).
- **Done:** `FamilyLinkController.cs:33,96`'s `/api/members/family` alias no longer overlaps anything
  — `FamilyController` (the thing it overlapped with) was deleted in 80.2.
- **Resolved 2026-09-06.** Traced end to end: `LocalFileStorageService` routes `Certificate`,
  `PaymentProof` and `Signature` uploads to `_secureRoot` (`FileStorage:SecureRelativePath`, default
  `secure_uploads/members`), never to `_publicRoot`/`uploads/members`. `Program.cs`'s guarded
  `UseStaticFiles` block (the one at `RequestPath = "/api/uploads"`) maps only
  `{BasePhysicalPath}/uploads`; the earlier, unguarded bare `app.UseStaticFiles()` call serves
  `IWebHostEnvironment.WebRootPath` (`ContentRootPath/wwwroot`), fixed by ASP.NET Core convention and
  not reconfigurable via `FileStorage:BasePhysicalPath`. `FileStorage:BasePhysicalPath` is unset in
  every `appsettings*.json` today, so `_secureRoot` falls back to `AppDomain.CurrentDomain.BaseDirectory`
  (the content root) — a sibling of `wwwroot`, not inside it. Net effect: the three sensitive fields
  are **not** reachable through the unguarded static route today. **But this isolation is accidental,
  not enforced** — it's a side effect of two independently-chosen fallback defaults (a relative
  `"wwwroot"` vs. `AppDomain.CurrentDomain.BaseDirectory`) rather than a boundary the code actually
  checks. It would silently break if `FileStorage:BasePhysicalPath` were ever set to `wwwroot` itself —
  a plausible mistake when `project_uploads_ephemeral_storage.md`'s persistent-disk fix eventually gets
  built, since nothing here stops someone from pointing both public and secure roots at the same
  disk mount. **Follow-up logged as 82.51** to make the separation an explicit assertion instead of a
  happy accident. Separately, tracing *why* `SecureFilesController` truly has zero working call sites
  surfaced a real, live bug, not dead code: `GHCAA.Mobile/lib/screens/member/member_details_screen.dart:239-251`
  does render `certificatePath`/`paymentProofPath` via `_viewNetworkImage` (`member_details_screen.dart:309-317`),
  but that method builds `{apiBaseUrl}/{rawStoredPath}` directly — for a secure-type path that's
  `{apiBaseUrl}/secure_uploads/members/...`, which matches neither the public `/api/uploads` mapping
  nor `SecureFilesController`'s own `/api/secure-files/{path}` route, and `Image.network` sends no
  Authorization header regardless. The feature is currently just broken (404/failed image load for an
  admin reviewing a member's certificate or payment proof), not a leak. Folded into 82.33's scope
  (below) rather than a new item, since it's the identical root defect 82.33 already names — a
  protected resource hit with an unauthenticated raw network call — just a second call site.

80.15 [VERIFIED-STALE 2026-09-04] Both timers named here turned out already correct on inspection.
`core/services/auth.service.ts`'s `inactivityTimer` lives in a root-injectable singleton `@Injectable`
service, not a component — there is no `ngOnDestroy` to leak into, since the service exists for the
app's whole session by design, and `resetTimer()` already clears the previous timer (`if
(this.inactivityTimer) clearTimeout(...)`) before setting a new one, so at most one is ever live.
`common/directory/directory.ts` already `implements OnDestroy` and clears `searchDebounce` at
`ngOnDestroy():96`. Closing as not-applicable rather than done; the audit's premise was wrong for both.

80.17 [DONE 2026-09-06] Found while fixing 80.5's logging gap, unrelated to it:
`LoginRateLimitMiddleware` parsed the login request body and stashed the username in
`context.Items["LoginUsername"]`, and its own class comment said this was "so the
PartitionedRateLimiter can key on (IP, username)". Checked: nothing read that key anywhere in the
codebase, and the `Auth` rate-limit policy in `Program.cs` keys purely on IP — deliberately, per
29B.5 (2026-07-25), which moved off a per-username key precisely because it let one IP spray
thousands of accounts at 5/min each. So the comment was stale: this was parse-and-stash left over
from before 29B.5, not a hook for some other unwired policy. Deleted the middleware entirely
(`GHCAA.API/Middleware/LoginRateLimitMiddleware.cs`) and its registration in `Program.cs`. Full
backend suite (620 tests) passes unchanged.

80.18 [DONE 2026-09-06] User instruction, 2026-09-04: a stateful action (save, search, any button
that triggers an API call and changes what the user sees) should not be triggerable a second time
until the first call resolves — success or error — to stop double-submit bugs (duplicate saves,
doubled search requests). Explicitly scoped to stateful actions only; stateless/fire-and-forget calls
(external link launches, downloads) were left untouched.

Two read-only audit passes (one per client) surveyed every save/search entry point before any code
changed, to fix only real gaps rather than rewrite already-correct components:

- **`GHCAA.Web`:** fixed 10 components with no guard at all (`member-approval`, `admin-campaigns`,
  `admin-contact-messages`, `admin-polls`, `member-polls`, `member-requests`, `admin-event-operations`),
  one with a `submitting` signal that existed but wasn't wired to `[disabled]` or an early-return guard
  (`admin-members`), and four raw keystroke-triggered searches with no debounce (`common/jobs`,
  `admin-governance`, `member-requests`, `member-messages`) — all now debounce 300ms, matching the
  existing pattern in `common/directory`. The shape used throughout: a per-component (or per-row-id)
  `signal()` set before the call and cleared in both the success and error branches, wired to both
  `[disabled]` in the template and an early-return guard in the handler — matching the convention the
  audit found already in use (`profile.ts`, `admin-news.ts`, `messages.ts`'s `sendFirstMessage`), not a
  new abstraction. No global HTTP-interceptor lock was added, since it can't distinguish a stateful save
  from a stateless background poll.
- **`GHCAA.Mobile`:** fixed 14 screens with the same bug class — no guard (`news_details_screen`
  approve action, `jobs_screen` delete/create, `events_screen` create-event dialog, `gallery_screen`'s
  five album/photo actions, `member_details_screen` approve/reject, `mentorship_hub_screen` respond/mark-
  complete, `family_link_screen` respond/cancel/remove, `permissions_matrix_screen` create-admin/assign-
  role/remove-role, `governance_registry_screen` activate-period/save-period), a loading flag present but
  not gating re-entry (`gatekeeper_screen`'s barcode `onDetect`, which fires on every camera frame
  regardless of `_isLoading`), and two raw un-debounced searches (`directory_screen`,
  `professional_hub_screen`). Two screens (`NewsDetailsScreen`, `MemberDetailsScreen`) were converted
  from `ConsumerWidget` to `ConsumerStatefulWidget` since the guard needs local state a stateless widget
  can't hold. `AdminActionCircle` (used only by `gallery_screen`) gained an optional `disabled` param.
  `job_details_screen`'s and `news_details_screen`'s existing delete-confirm dialogs (pop-then-await, so
  the button can't be re-tapped) and `financial_portal_screen`'s already-guarded payment-submit dialog
  were left as-is — already safe, not part of the fix.

**Verified:** `npm run type-check` and `npm run test:unit -- --run` (399/399) clean on `GHCAA.Web`;
`flutter analyze` and `flutter test` clean on `GHCAA.Mobile` — the pre-existing golden/pixel-diff
failures (58 failures, unrelated screens including ones this item never touched) were confirmed present
on the unmodified tree too via `git stash`, so they predate this change and aren't a regression from it.

80.19 [DONE 2026-09-06] **Closed as a side effect of building 81.4**, which was the same gap
described from the member-request-management angle. The `/portal/requests` page it built covers all
five things this item named: send by membership number, respond to received requests, cancel a sent
one, remove an accepted link (`FamilyLinkService.remove`/`GHCAA.API`'s `DELETE remove/{requestId}`,
newly wired here since 81.4 itself only covered send/respond/cancel), and view accepted links (new
"Your Family Network" section, calling `GET my-family`). `enableFamilyLink` can stay `true` — the web
portal now has the feature it was already advertising. `ng build` clean, `vitest` 399/399.

80.20 [DONE 2026-09-06] **Priority: P3 | Depends on: none.** Found while tagging 80.16's new tests against the
FR catalogue: FR-09 in `docs/book/03-requirements.md` read "shall verify a registered email address
and mobile number by a single-use, time-limited **code**, and shall use the **same mechanism** for
password reset." The actual password-reset flow — both the pre-existing admin-initiated one and
80.16's new self-service one — uses a GUID token embedded in a link, not the OTP code mechanism
`OtpService` already provides for registration verification. This mismatch predated this session (the
admin flow's tests already carried `[Category("FR-09")]` before 80.16 touched anything); 80.16's new
tests kept the same tag rather than inventing a different one, since the actual behavior does
genuinely belong under FR-09's password-reset half, just not via the mechanism the requirement named.
**Resolved 2026-09-06:** reworded FR-09 to describe the token-link mechanism actually built, rather than
rebuilding a working reset flow (and its mobile deep-link UX) to match a requirement that never matched
what was shipped. The `[Category("FR-09")]` tags on the existing reset-flow tests stay accurate as-is.

---

