# GHCAA PLATFORM MASTER TASK TRACKER

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

## AREA 3: COMMUNICATION & SOCIAL
3.1  [DONE] Real-time Communication Bridge (SignalR Admin Alerts)
3.2  [DONE] Member Chat/Noticeboard Framework & Services
3.3  [DONE] HTML Templating system for system notifications
3.4  [DONE] SMS Gateway Integration (Greenweb/SSL Wireless)
3.5  [DONE] Networking: Privacy controls (visibility toggles respect DTO masking)
3.6  [DONE] Networking: Mentorship request flow in Job Hub
3.7  [TODO] Discussion forums and community groups

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
7.13 [TODO] Security: Two-Factor Authentication (2FA) for admin actions
7.14 [TODO] Security: Biometric Authentication (FaceID/Fingerprint)
7.15 [DONE] Security: Social Auth (OAuth2) - LinkedIn/Google
7.16 [TODO] Hardening: SSL Pinning and Binary Obfuscation

## AREA 8: MOBILE ENGINEERING (TIER-1 STANDARDS)
8.1  [TODO] UI: Enforce 8pt grid and standard design tokens globally
8.2  [TODO] Nav: Adaptive layout for Tablets/Pads (Sidebar architecture)
8.3  [TODO] Perf: Cursor-based pagination for Alumni Registry
8.4  [TODO] Perf: Isolated background threading for JSON/Encryption processing
8.5  [TODO] Persistence: Switch to High-Performance Local DB (Isar/Drift)
8.6  [TODO] Networking: Exponential backoff and connectivity banners
8.7  [TODO] State: Riverpod State Hydration (Local local persistence)
8.8  [TODO] i18n: Unified Localization (English + Bengali)
8.9  [DONE] CI/CD: Fastlane + GitHub Actions Deployment Pipeline
8.10 [DONE] Quality: Global Error Boundary and Sentry/Firebase tracing

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

## CREDENTIALS:
- SuperAdmin: superadmin / SuperAdminPassword123!
- Developer Admin: shalin / Shalin@2024!
- TEST_MEMBER: demo_user / DemoPass123!
- Mobile: If not connected with net, show error on mobile app
