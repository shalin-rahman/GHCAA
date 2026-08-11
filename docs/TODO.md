# GHCAA PLATFORM TASK TRACKER

**Open: 73  ·  Completed: 392  ·  Total: 465**  
Last reconciled against the codebase: 2026-08-11.

This file is the authoritative status of the platform. [PLAN.md](PLAN.md) holds the
phased execution order; where the two disagree, this file wins.

[Part 1 — Open backlog](#part-1--open-backlog) · [Part 2 — Completed work](#part-2--completed-work)

---

## How to read this file

**Numbering.** IDs are stable and never reused, so `28.19` means the same thing in a
commit message today as it did when it was raised. The letter forms (`29E.1`, `34.D16`,
`35.B1`) group a sub-stream inside an area.

**Status.** Carried over verbatim from the old flat list, so `grep DONE` and `grep TODO`
still work. `TODO` not started · `IN-PROGRESS` started, not finished · `DONE` delivered
and its test passed · `NOTE` context rather than a unit of work. Every row in Part 1 is
`TODO` or `IN-PROGRESS`; every row in Part 2 is `DONE` or `NOTE`.

**Component.** Which part of the stack the work sits in: API · Web · Mobile ·
Cross-platform · DB · Tests · Security · Performance · Process · DevOps. It is a filter,
not a new label — the item's own wording is kept intact in the description alongside it.

**Estimate.** Relative sizing, not a commitment: **S** under a day · **M** one to three
days · **L** about a week · **XL** multi-week, expect to split it.

**Raised.** The date the item was filed, shown only where the source records one. Items
inherited from the original backlog have no recorded date and are left blank rather than
given an invented one.

**Depends on.** A hard ordering constraint: the listed item has to land first. Softer
relationships stay in the description as `RELATES TO`.

### Standing policy

Formerly tracked as item 12.6, promoted here because it governs every other row:

> PROCESS: A task can only be marked as [DONE] after its tests have been successfully executed and passed.

In practice that also means a change ships across the stack. A feature is not done until
the API, the Angular web app and the Flutter mobile app all reflect it, with tests on each
side, and regenerating the mobile goldens or the Playwright snapshots is part of the
change rather than a follow-up.

---

## Part 1 — Open backlog

Ordered by how much other work each epic unblocks. Areas 1-26 and 29-33 are fully
delivered and appear only in Part 2.

### 34. Data Integrity, Site Content & Cross-Cutting  (4 open)

> User instruction set: (1) update About Us with college + association information, (2) add an On Campus Address to Contact Us, (3) make both manageable from admin, (4) merge News and Notice management, notices may carry PDF documents. Standing constraints: **notices are admin-post-only**; About content is sourced from the GHCAA Constitution text and facts already in the repo **only** (no web research, no invented college facts); web + mobile parity; OrgConfig additions must be industry-standard and configurable; nothing may break.

#### 34.D — Cross-cutting (raised mid-implementation by user)

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **34.D8** | `TODO` | Cross-platform | Live-browser verification of the Area 34 surface | Live-browser verification of this area: public `/about` (seeded CMS blocks + hardcoded fallback), `/contact` (campus address, phones, map guard), `/news?type=Notice`; admin CMS block edit round-trip; admin notice creation with a PDF and a working public download link; and a **member account confirming it cannot create a Notice** (34.C3). **Blocked by the same local seed/role mismatch as 33.13** — `shalin` and `superadmin` resolve to ordinary alumni members in the local DB, so `adminGuard` bounces `/admin/*`; the local role assignment must be fixed before the admin-side steps can run. | M | 34.D17 | 2026-08-02 |
| **34.D10** | `TODO` | DB | Run migrations at startup instead of EnsureCreated() | **Durable fix for the above**: the app has a real migration tree but nothing runs it at startup, so every future schema change will hit the same silent no-op on any non-empty environment. Switch the non-Visual startup path from `Database.EnsureCreated()` to `Database.Migrate()`. Non-trivial because the preprod/Neon database was originally built by `EnsureCreated` and therefore has **no `__EFMigrationsHistory` table** — it must first be baselined (create the history table and insert every existing migration id as already-applied) or `Migrate()` will try to re-create tables that exist and fail. Plan: baseline preprod → switch the call → confirm a no-op `Migrate()` on an already-current DB → confirm a fresh empty DB still builds end-to-end. Also decide what happens to the `HasData` seed rows, which `EnsureCreated` and `Migrate` apply by different routes. Keep SQLite/local dev working throughout (it relies on `EnsureCreated` and has no SQLite migration tree — see 34.D4). | L | — | 2026-08-02 |
| **34.D16** | `TODO` | Web | Bind the portal layout logo to org config | `portal-layout.html` hardcodes `src="/assets/logo.png"` in two places (the sidebar mini-logo and the collapsed-rail logo) instead of binding `orgConfigService.config()?.branding?.logoUrl` the way admin-layout and the public footer do — `portal-layout.ts` does not inject `OrgConfigService` at all. Harmless today because the configured default *is* `/assets/logo.png`, but it silently opts the portal out of org-config branding, so a tenant that sets a custom logo gets it everywhere except the member portal. | S | — | 2026-08-02 |
| **34.D17** | `TODO` | DevOps | Re-seed local DB so admin accounts resolve as admins | The local database is stale relative to the seed files, so `shalin` and `superadmin` log in as ordinary alumni members and `adminGuard` bounces every `/admin/*` route. The seed itself is correct — `Seed/user_roles.json` maps user 1 to SuperAdmin and user 2 to Admin, and `Seed/roles.json` defines all three roles — so this is drift in the local DB, not a data bug (see the seed-vs-live-DB gotcha). Re-seed or repair the role rows locally. Filed by the 2026-08-11 audit because it silently blocks two verification tasks and was tracked nowhere. | S | — | 2026-08-11 |

### 35. Mobile Gap Review & Test Hardening  (17 open)

> Scope: a documentation-vs-source sweep of `GHCAA.Mobile` only — every item below was confirmed against the actual code, not inherited from an earlier area. Items an earlier area already settled deliberately are **not** repeated here (e.g. 29G.2 records walletNumber/bankName/accountNumber as display-only *by design*, so that is not a gap). Each task names the file, the line, and how to verify it, so it can be picked up cold. Standing constraints carry over: web + mobile parity, `flutter analyze` stays clean, and per the standing policy nothing is `[DONE]` until its test passes.
>
> **Measured on this machine (Flutter 3.44.2 / Dart 3.12.2, 2026-08-11):** `flutter analyze` — *No issues found*. `flutter test` — **121 passed / 0 failed**. The sweep started at 66 passed / 26 failed and reached 95 / 26 after 35.B7 and 35.B8. 35.B1 then found that part of that failing count was not a golden problem at all: the package config had drifted, so two visual-freeze files failed to compile rather than mismatch. After `flutter pub get` the real set was 21 stale goldens, and regenerating them took the suite green. **The bar for anything in this area is now zero failures** — any red is yours.

#### 35.A — Source gaps found in the sweep

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **35.A1** | `TODO` | Mobile | Resolve two orphaned, unreachable screens | Mobile: two screens are fully built but unreachable — nothing routes to them and nothing navigates to them: `lib/screens/member/committee_screen.dart` (227 lines) and `lib/screens/member/activity_log_screen.dart` (129 lines). Each duplicates a screen that *is* routed: `/committee` renders `GovernanceScreen` and `/activity` renders `MemberActivityHistoryScreen`. Decide per file — route it, fold anything it does better into its routed twin, or delete it — but do not leave both. `flutter analyze` does not flag an unreferenced screen, so this rots silently and the next person editing "the committee screen" has a 50 % chance of editing the dead one. | S | — | 2026-08-11 |
| **35.A2** | `TODO` | Mobile | Admin dashboard logout does not clear the session | Mobile: the admin AppBar logout button does not log out. `lib/screens/admin/admin_dashboard_screen.dart:34` is `onPressed: () => context.go('/login')` with no `AuthService.logout()` call, so the JWT survives in secure storage and `authStateProvider`'s poll can redirect straight back into `/admin_dashboard`. Route it through the same logout path `app_drawer.dart` already uses. Verify: log out from the admin dashboard, confirm the token is cleared and `/admin_dashboard` is unreachable without a fresh login. | S | — | 2026-08-11 |
| **35.A3** | `TODO` | Mobile | Invalidate cached role and profile on logout | Mobile: logout clears storage but not cached Riverpod state. Neither `SessionManager._handleSessionExpiry()` (`lib/core/session/session_manager.dart:32`) nor `AuthService.logout()` invalidates `roleProvider` or `userProfileProvider` — 29A.2 fixed the token half and 29E.3 added the `notificationHubServiceProvider` invalidation, but the role and profile caches were never included. Add them alongside that existing invalidation. Verify: log in as an admin, log out, log in as an ordinary member — no admin nav entry and no stale name/photo anywhere. | S | 35.A2 | 2026-08-11 |
| **35.A4** | `TODO` | Mobile | Digital ID encodes a literal PENDING barcode | Mobile: Digital ID emits a scannable code that resolves to nothing when a member has no number yet — `lib/screens/member/digital_id_screen.dart` falls back to the literal string `'PENDING'` at lines 115 and 129 (QR + barcode) and again at 228/232 in the PDF export. 29A.3 corrected the field name but kept the fallback. Show an explanatory empty state instead of a code when `membershipNumber` is null, in the card and the export alike. | S | — | 2026-08-11 |
| **35.A5** | `TODO` | Mobile | Debounce the directory search | Mobile: directory search fires one API request per keystroke — `lib/screens/member/directory_screen.dart:123-125` wires `_onSearchChanged` directly to `_fetchAlumni(refresh: true)`. 29E.4 solved exactly this for the shared `AppSearchField` (350 ms internal debounce), but the directory keeps its own `TextField` (line 154) and never adopted it. Reuse `AppSearchField` or apply the same debounce. | S | — | 2026-08-11 |
| **35.A6** | `TODO` | Mobile | Surface the remaining swallowed service errors | Mobile: silent `catch (_) {}` sites the 29E.2/29F.2 sweep did not reach, where the caller cannot distinguish "no data" from "the request failed" — `lib/features/forum/forum_service.dart:163` (`getTopic`), `:196` (`createTopic`), `:213` (`createPost`), and `lib/features/financials/financial_service.dart:112` (`recordPayment`, where a network failure and a server rejection both surface as the same bare `false` on a payment submission). Apply the rule 29E.2 established: `debugPrint` + `rethrow` for reads that feed an `AsyncValue`, a real message for imperative writes. Note 29E.2 deliberately exempted the lookup/dropdown static fallbacks — but `lib/features/lookups/lookup_service.dart:44` has *no* static fallback, it returns a bare `[]`, so `batchListProvider`/`sectorListProvider` render an empty dropdown with no explanation; that one belongs in this fix, not in the exemption. | M | — | 2026-08-11 |
| **35.A7** | `TODO` | Mobile | Hardcoded production baseUrl in the gateway payload | Mobile: `lib/features/financials/gateway_service.dart:37` posts a hardcoded `'baseUrl': 'https://api.ghcaa.org'` in the gateway-initiate payload — its own trailing comment admits it is a placeholder. A preprod or local build therefore hands the payment gateway a production return URL. Source it from `AppConfig` like the rest of the app does. | S | — | 2026-08-11 |
| **35.A8** | `TODO` | Mobile | Drop or wire the unused social-auth SDKs | Mobile: `google_sign_in` and `flutter_facebook_auth` are still declared in `pubspec.yaml` but have zero imports anywhere in `lib/` — 29E.1 removed the dead social-login UI and kept the SDKs, so both still ship in the binary and keep their native/Gradle/Info.plist config alive and unmaintained. Either finish the wiring under 7.15 or drop both packages together with the uncalled `AuthService.googleLogin`/`facebookLogin` (`lib/features/auth/auth_service.dart:82,86`). | S | 7.15 | 2026-08-11 |
| **35.A9** | `TODO` | Mobile | formatDate silently rolls over invalid dates | Mobile: `AppUtils.formatDate` turns an out-of-range date into a plausible wrong one instead of rejecting it — `lib/core/utils/app_utils.dart:15` calls `DateFormat('dd-MM-yyyy').parse()`, whose default lenient mode rolls overflow forward, so `'2026-13-45'` renders as `14-02-2027` rather than falling through to the `catch` that returns the raw value. Found while writing the 35.B7 tests; a bad date from the API therefore displays as a confident, wrong date on every screen that formats one. Use `parse(input, true)` (strict) so the existing fallback actually runs, and update the expectation in `test/unit_test.dart` that currently pins the rollover. | S | — | 2026-08-11 |
| **35.A10** | `TODO` | Mobile | Verify screenshot protection actually works on a device | The committed `GeneratedPluginRegistrant.java` and `GeneratedPluginRegistrant.m` did **not** register `screen_protector` — the plugin was in `pubspec.yaml` and called from three screens (`digital_id_screen.dart`, `financial_portal_screen.dart`, `payment_web_page.dart`), but the native registrants were stale, so `ScreenProtector.preventScreenshotOn()` would have had no plugin behind it. Found by 35.B1, when a `flutter pub get` regenerated both files and added the missing registration. A local build regenerates these, so this may never have reached a real release — but nobody appears to have checked. **Verify on a physical Android device and a physical iOS device that a screenshot is actually blocked on the Digital ID screen**, since that is the whole point of the feature and it cannot be proven by a unit test. While there, decide whether these generated registrants should be tracked in git at all; they are build output and they drift silently. | S | — | 2026-08-11 |

#### 35.B — Test suite

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **35.B2** | `TODO` | Tests | Extract a shared mobile test harness | Mobile: there is no shared test harness. Five golden files (`comprehensive_visual_freeze_test.dart`, `full_app_visual_freeze_test.dart`, `visual_freeze_test.dart`, `dashboard_visual_test.dart`, `registration_visual_test.dart`) each define their own `wrapInApp`/`_wrapInApp` plus their own copy of 15–20 fake services. 34.D6 already paid the price for this: one `NewsService` signature change forced the same edit in two files. Extract one `test/helpers/` harness (`pumpApp` + the fake set + the `dotenv.testLoad`/`local_auth` channel boilerplate) and have every visual test call it. | M | — | 2026-08-11 |
| **35.B3** | `TODO` | Tests | Cover api_client, session_manager and the widgets | Mobile: significant surface with no test of any kind. Whole features: the forum screens (`forum_categories_screen.dart`, `forum_topics_screen.dart`, `forum_topic_detail_screen.dart`). Core infrastructure: `core/api/api_client.dart` (including the 31.5 JWT-refresh interceptor, which is untested), `core/session/session_manager.dart`, `core/storage/storage_service.dart`, `core/services/org_config_service.dart`, `core/real_time/notification_hub_service.dart`, `core/services/connectivity_service.dart`, `core/services/device_info_service.dart`. All 15 widgets under `core/widgets/` — including `app_search_field.dart`, whose 350 ms debounce (29E.4) has no regression test. Prioritise `api_client` and `session_manager`: both govern auth state, and 35.A3 is about to change one of them. | L | 35.A3 | 2026-08-11 |
| **35.B4** | `TODO` | Tests | Run or formally shelve the integration suite | Mobile: `integration_test/` never runs anywhere. CI (`.github/workflows/ghcaa-ci-standard.yml`, `ghcaa-ci-preprod.yml`) runs `flutter analyze` and `flutter test`, which only picks up `test/`; the five integration files need a device and a live API and are blocked locally by MOB-BLOCK-001/003. Either add a CI job with an emulator + seeded API, or mark the suite explicitly manual in the mobile README so its green-looking presence stops implying coverage it does not provide. | M | — | 2026-08-11 |
| **35.B5** | `TODO` | Tests | DGePay test passes when the feature is absent | Mobile: `integration_test/dgepay_payment_test.dart` passes when the feature is missing — its taps are wrapped in `if (payButton.evaluate().isNotEmpty)`, so a build where the Pay button never renders reports success. Make the element presence an assertion. | S | — | 2026-08-11 |
| **35.B6** | `TODO` | Tests | Apply or drop the unused golden tag | Mobile: `dart_test.yaml` declares a `golden` tag that no test applies, so `flutter test --tags golden` (and its inverse, the obvious way to skip goldens locally) matches nothing. Either tag the five golden files with `@Tags(['golden'])` or drop the declaration. | S | — | 2026-08-11 |

#### 35.C — Documentation drift found during the sweep

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **35.C2** | `TODO` | Process | Delete the stale committed test logs | `GHCAA.Mobile/` has nine committed `test_output_2.txt` … `test_output_10.txt` files (~750 KB total) plus `rows.txt`. All nine are logs of a single isolated run of `full_app_visual_freeze_test.dart`, most of them failures, and several capture bugs that are already fixed (the `dotenv` `NotInitializedError` and the `await dotenv.testLoad` compile error). They are stale, they are not referenced by anything, and a newcomer grepping for test results will find these before the real ones. Delete them and add the pattern to `.gitignore`. | S | — | 2026-08-11 |
| **35.C3** | `TODO` | Process | Strip stale counts from the memory files | `.claude/memory/outstanding_todos.md` and `MEMORY.md` still carry statements this tracker has since overtaken — "only 30.1 done" for Area 30 (which is now essentially complete) and "only 2 pending DB migrations remain" (31.3 records that there are none). Both files already point at `docs/TODO.md` as canonical; strip the stale counts so they cannot be quoted back as current. | S | — | 2026-08-11 |

### 7. Security, Infrastructure & Hardening  (3 open)

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **7.13** | `TODO` | API | Two-factor authentication for admin actions | Security: Two-Factor Authentication (2FA) for admin actions | L | — | — |
| **7.15** | `TODO` | Cross-platform | Social auth (OAuth2) — LinkedIn + finish Google | Security: Social Auth (OAuth2) - LinkedIn/Google **[2026-08-11 audit] Reopened by the 2026-08-11 audit. LinkedIn OAuth exists nowhere in the repo, and the mobile half was removed by 29E.1 (no SDK wiring, no keys). Only the web Google/Facebook buttons (15.4) and the API side (15.3) are real.** | L | 15.5 | — |
| **7.16** | `TODO` | Mobile | SSL pinning and binary obfuscation | Hardening: SSL Pinning and Binary Obfuscation | M | — | — |

### 15. Social Auth & Onboarding  (1 open)

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **15.5** | `TODO` | Mobile | Implement mobile social login for real | Mobile: Implement Social Login (google_sign_in, flutter_facebook_auth) **[2026-08-11 audit] Reopened by the 2026-08-11 audit. 29E.1 removed the mobile social-login UI because it was a stub, and `google_sign_in` / `flutter_facebook_auth` still have zero imports anywhere in `lib/`. This was never delivered.** | L | 35.A8 | — |

### 28. Configuration-Driven Framework  (12 open)

> Reference doc: docs/CONFIG_DRIVEN_FRAMEWORK.md
> DRY/SOLID review completed by Claude Opus on 2026-05-30.
> All blocking bugs from review are fixed. Phase 1 (backend) is merge-ready pending 28.0.

#### PRIORITY 2 - PHASE 2: ANGULAR CONSUMER

> DEPENDS ON: 28.0 (migration applied, GET /api/config returning 200)

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **28.14** | `TODO` | Web | Rename API_ENDPOINTS.CONFIG to ORG_CONFIG, or close | Angular: Add API_ENDPOINTS.ORG_CONFIG = '/api/config' to app.constants.ts **[2026-08-11 audit] Near-complete: the constant shipped as `API_ENDPOINTS.CONFIG = '/api/config'` in `app.constants.ts`, with the value the task specified. Only the name differs from the `ORG_CONFIG` the task asked for. Rename it or close this — no functional work remains.** | S | — | 2026-05-30 |

#### PRIORITY 3 - PHASE 3: FLUTTER CONSUMER

> DEPENDS ON: 28.11-28.16 (Angular consumer pattern validated first)

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **28.19** | `TODO` | Mobile | Decide: lazy vs pre-runApp org-config load | Mobile: Wire OrgConfigService.instance.load() in main.dart before runApp **[2026-08-11 audit] Implemented differently, needs a decision rather than code. `OrgConfigService.load()` is not called before `runApp`; config resolves lazily through the Riverpod `orgConfigProvider` when the first widget watches `orgBrandingProvider` / `localePackProvider`. That avoids a blocking startup fetch and already falls back to cache then build defaults. Confirm the lazy pattern is acceptable and close, or implement the blocking pre-`runApp` load.** | S | 28.18 | 2026-05-30 |
| **28.20** | `TODO` | Mobile | Drawer menu item titles from the locale pack | Mobile: Update app_drawer.dart - replace 7 hardcoded strings with pack.nav.* 'ADMINISTRATION' -> pack.nav.administration 'MY ACCOUNT'     -> pack.nav.myAccount 'COMMUNITY'      -> pack.nav.community 'MEDIA & TOOLS'  -> pack.nav.mediaAndTools 'ADMINISTRATOR'  -> pack.nav.adminRoleLabel 'ALUMNI MEMBER'  -> pack.nav.memberRoleLabel 'Batch: '        -> pack.nav.batchPrefix **[2026-08-11 audit] Half done. The drawer's **section headers** already come from the locale pack (`localePackProvider` in `app_drawer.dart`), as do the role labels and batch prefix. The remaining hardcoding is the **individual menu item titles** ('Dashboard', 'Approvals', 'My Profile' and the rest), which are still string literals in the `_MenuItem` list.** | S | 28.18 | 2026-05-30 |
| **28.21** | `TODO` | Mobile | Add Guest to the two remaining membership lists | Mobile: Add Guest to any hardcoded MembershipType list in Flutter Grep: grep -r 'Advisory' lib --include="*.dart" DEPENDS ON: none (standalone fix) **[2026-08-11 audit] Partly done. `dropdown_service.dart`'s `defaultMembershipTypes` fallback already includes Guest. Two hardcoded lists still omit it: the TYPE filter in `directory_screen.dart:199` and `typeOptions` in `core/constants/registration_constants.dart`.** | S | — | 2026-05-30 |

#### PRIORITY 4 - TESTS

> DEPENDS ON: 28.0 (migration), 28.4 (service implemented)

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **28.22** | `TODO` | Tests | OrgConfigSeedTests — locale key completeness | Tests: OrgConfigSeedTests - assert GHCAA defaults have all required locale keys File: GHCAA.Tests/OrgConfig/OrgConfigSeedTests.cs Cases: en+bn present; MembershipTypeLabels has 7 keys (incl. Guest); all features default ON except Gamification+SocialAuth | S | — | 2026-05-30 |
| **28.23** | `TODO` | Tests | OrgConfigServiceTests on SQLite in-memory | Tests: OrgConfigServiceTests - unit tests with SQLite in-memory File: GHCAA.Tests/OrgConfig/OrgConfigServiceTests.cs Cases: returns defaults when DB empty; UpdateConfigAsync persists; cache invalidates; Bengali locale lookup | M | 28.0 | 2026-05-30 |
| **28.24** | `TODO` | Tests | OrgConfigControllerTests — GET/PUT authorisation | Tests: OrgConfigControllerTests - integration tests File: GHCAA.Tests/Integration/OrgConfigControllerTests.cs Cases: GET returns 200; PUT returns 403 for Admin role; PUT returns 204 for SuperAdmin | M | 28.0 | 2026-05-30 |
| **28.25** | `TODO` | Tests | Refresh Playwright snapshots after the config rollout | Tests: Re-run visual regression snapshots after Phase 2 Angular consumer is done Command: npx playwright test --update-snapshots WHY: APP_CONFIG references replaced by config-driven values may shift text in layout | S | 28.15 | 2026-05-30 |
| **28.26** | `TODO` | Tests | Playwright config-regression spec | Tests: Add Playwright config-regression spec File: GHCAA.Web/tests/e2e/config-regression.spec.ts Cases: org name from intercepted config (not hardcoded); feature=false route redirects | M | 28.24, 28.12, 28.16 | 2026-05-30 |

#### PRIORITY 5 - ADMIN UI (PHASE 4, OPTIONAL)

> DEPENDS ON: 28.12 (Angular OrgConfigService)

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **28.28** | `TODO` | Web | Relabel the admin nav link, or close | Angular: Add Organization Config link to admin sidebar in nav.service.ts **[2026-08-11 audit] Cosmetic only: the link already exists in `nav.service.ts` at `/admin/org-config`, SuperAdmin-gated, in the 'Finance & Tools' section — labelled 'Org Config' rather than 'Organization Config'. Adjust the wording or close.** | S | — | 2026-05-30 |

#### PRIORITY 6 - TECH DEBT CLEANUP (after all phases pass regression)

> DEPENDS ON: All of 28.1-28.28 green; do NOT delete Constants.cs fields before this

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **28.32** | `TODO` | Web | Install ngx-translate for UI-layer strings | Angular: Install ngx-translate for UI-layer strings (form labels, buttons, page titles) Separate from OrgConfigService locale packs which cover org terminology RELATES TO: 8.8 (Mobile i18n) | L | — | 2026-05-30 |
| **28.33** | `TODO` | Mobile | Flutter intl + .arb files for UI-layer strings | Mobile: Add Flutter intl + .arb files for UI-layer strings RELATES TO: 8.8 [TODO] i18n: Unified Localization (English + Bengali) **[2026-08-11 audit] No `.arb` files and no `lib/l10n/` exist. `intl` is present but used only for date formatting in `AppUtils`. `core/services/app_localizations.dart` is a hand-rolled `Map<String, Map<String, String>>`, and nav section labels come from the org-config locale pack — so there are already two parallel string mechanisms to reconcile, not a greenfield start. RELATES TO 8.8.** | L | — | 2026-05-30 |

### 27. Test Coverage Improvement  (5 open)

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **27.1** | `IN-PROGRESS` | Tests | Regenerate the coverage report from a real run | Generate low‑coverage report (parse coverage.cobertura.xml) **[2026-08-11 audit] `docs/low_coverage_report.md` exists but is dated 2026-05-26, there is no `coverage.cobertura.xml` in the repo, and no script regenerates it. Its headline figure (0.43 % over 1,002,454 "lines valid") counts generated and vendor code and is misleading — do not set a gate against it. Rebuild the report from a real `--collect:"XPlat Code Coverage"` run scoped to first-party assemblies.** | M | — | 2026-05-26 |
| **27.3** | `TODO` | Tests | Decide on WebApplicationFactory HTTP integration tests | Write unit tests for Controllers (WebApplicationFactory) **[2026-08-11 audit] 17 controller test files already exist under `GHCAA.Tests/Controllers/`, so the coverage goal is largely met — but they instantiate controllers directly with Moq, and `WebApplicationFactory` appears nowhere in the repo. Decide whether real in-process HTTP integration tests are still wanted, or close this and keep the Moq approach.** | L | — | 2026-05-26 |
| **27.6** | `TODO` | Tests | Repository integration tests on SQLite in-memory | Write repository integration tests with in‑memory SQLite **[2026-08-11 audit] SQLite in-memory is already wired in `GHCAA.Tests/TestBase.cs` and used by roughly 25 service and workflow tests. The gap is narrower than stated: only one dedicated repository test exists (`FileUploadRepositoryTests`) and it uses EF InMemory rather than SQLite.** | M | — | 2026-05-26 |
| **27.7** | `TODO` | Tests | DateFormatConverter and utility class tests | Write utility class tests (DateFormatConverter, etc.) | S | — | 2026-05-26 |
| **27.8** | `TODO` | Tests | Coverage gate at 80 % per first-party file | Run coverage and enforce ≥ 80 % per file | M | 27.1, 27.3, 27.6, 27.7 | 2026-05-26 |

### 8. Mobile Engineering (Tier-1 Standards)  (6 open)

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **8.3** | `TODO` | Mobile | Cursor-based pagination for the alumni registry | Perf: Cursor-based pagination for Alumni Registry | M | — | — |
| **8.4** | `TODO` | Mobile | Move JSON/encryption work to a background isolate | Perf: Isolated background threading for JSON/Encryption processing | M | — | — |
| **8.5** | `TODO` | Mobile | Swap local persistence to Isar or Drift | Persistence: Switch to High-Performance Local DB (Isar/Drift) | L | 8.7 | — |
| **8.6** | `TODO` | Mobile | Exponential backoff on the API client | Networking: Exponential backoff and connectivity banners **[2026-08-11 audit] Split verdict from the 2026-08-11 audit. The **connectivity banner half is already done** — `core/widgets/no_internet_banner.dart` plus `core/services/connectivity_service.dart` (connectivity_plus), wired through `ConnectivityAwareWrapper` in `main.dart`. What remains is **exponential backoff**: `core/api/api_client.dart` has only a single 401 JWT-refresh retry, and `async_value_widget.dart` offers a manual retry button. No automatic backoff exists.** | M | — | — |
| **8.7** | `TODO` | Mobile | General Riverpod state hydration | State: Riverpod State Hydration (Local local persistence) **[2026-08-11 audit] Narrower than it reads. Ad-hoc persistence already exists for the dashboard layout (`dashboardLayoutProvider` via `StorageService`), the org config cache, the selected locale, and tokens/credentials. What is missing is a general hydration mechanism — there is no Hive or riverpod-persistence layer, so each case is hand-rolled.** | L | — | — |
| **8.8** | `TODO` | Cross-platform | Unified English + Bengali localisation | i18n: Unified Localization (English + Bengali) | XL | 28.32, 28.33 | — |

### 12. Process & Engineering Standards  (5 open)

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **12.1** | `TODO` | Process | Web + Mobile parity checklist on every API change | PROCESS: On every API endpoint change, add verification checklist task for Web + Mobile parity | S | — | — |
| **12.2** | `TODO` | Process | API contract registry / endpoint changelog | PROCESS: Implement API Contract Registry (changelog of all endpoint changes + which clients updated) | M | 12.1 | — |
| **12.3** | `TODO` | Mobile | In-app rotating log capture | Mobile: Implement in-app log capture (rotating file log) for all API errors and app events | M | — | — |
| **12.4** | `TODO` | Mobile | "Report a Problem" / share logs | Mobile: Add "Report a Problem" / "Share Logs" feature so users can email/share captured logs to admin | M | 12.3 | — |
| **12.5** | `TODO` | Mobile | Send report to administrator on unhandled error | Mobile: On any unhandled error, show option to "Send Report to Administrator" with log attachment **[2026-08-11 audit] Partly covered by Sentry already. `main.dart` sets `PlatformDispatcher.instance.onError` to `Sentry.captureException` and the custom `ErrorWidget.builder` renders a "DIAGNOSE & REPORT" action. What is missing is the user-facing half: a report that carries the 12.3 logs and reaches an administrator rather than Sentry. DEPENDS ON 12.3.** | S | 12.3, 12.4 | — |

### 6. Career & Opportunities  (1 open)

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **6.2** | `TODO` | Cross-platform | Alumni referral system for jobs and internships | Alumni referral system for jobs and internships | XL | — | — |

### 36. Facebook Page → News/Notice Ingest  (19 open)

> Raised by user 2026-08-11: "would it be possible a pre-configured FB page links posts to be
> collected, admin able to choose which posts to display as notice/news, then post contents
> (image and texts, dates etc) are formatted as current news/notice structures?"
>
> **Feasibility: yes, and without Facebook App Review** — but only because GHCAA owns the Page.
> Reading a Page's own posts needs `pages_read_engagement` on a Page Access Token. Facebook
> waives App Review when the token belongs to a user who holds a role on that Page, which an
> association admin does. If that assumption ever breaks (agency-managed Page, role removed),
> the feature needs App Review and the estimates below are wrong. **36.A1 exists to prove this
> before any code is written.**
>
> **Design in one paragraph.** Fetch is *admin-triggered*, not scheduled — the repo has no
> background-job infrastructure (no Hangfire, no `IHostedService`, no cron), and the
> requirement is a human choosing posts anyway, so a "Fetch from Facebook" button avoids that
> entire surface. Fetched posts land in a **staging table**, not straight into `NewsPosts`;
> the admin then picks, edits and imports. Import reuses what already exists: media through
> `FileValidationService` + `LocalFileStorageService` (`FileUploadType.NewsImage`), the row
> through `NewsService.CreateNewsAsync`, which already sanitizes HTML. So the ingest is a new
> front door onto the existing News/Notice pipeline rather than a parallel one.
>
> **The three things most likely to make this fail in production**, each with a task below:
> Page tokens expire and the failure is silent (36.B2); Facebook's image CDN URLs rotate, so
> anything not downloaded at import time turns into a broken image later (36.B5); and a
> re-fetch must not create duplicates, which needs the Facebook post id as a unique key
> (36.B3).

#### 36.A — Decisions to close before writing code

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **36.A1** | `TODO` | Process | Prove the Page-token route works without App Review | Before anything is built, confirm by hand: an association admin who holds a role on the GHCAA Page can mint a long-lived Page Access Token with `pages_read_engagement` and read `/{page-id}/published_posts`. Do it once in Graph Explorer and record the exact permission set, the token lifetime observed, and the Graph API version used. **This is the single assumption the whole area rests on** — if the Page turns out to be agency-managed, or the token needs App Review, stop and re-plan rather than proceeding. Record the answer in this item. | S | — | 2026-08-11 |
| **36.A2** | `TODO` | Process | Settle the editorial policy | Four questions, all of which change the code: (1) does an imported post need visible attribution to the Facebook source, and does the association have the right to re-publish comment/reaction data (recommend: no comment data at all)? (2) if the post is edited or deleted on Facebook after import, does the site follow — recommend **no**, an imported item becomes an independent editorial artefact, and say so in the admin UI so nobody is surprised; (3) which post types are in scope — plain text and photo posts certainly, but decide now about videos, shared links and reels; (4) is an imported item allowed to become a `Notice`, given notices are admin-post-only and carry legal weight, or only ever `News`? | S | — | 2026-08-11 |
| **36.A3** | `TODO` | Security | Decide where the Page token lives | A long-lived Page Access Token grants read access to the Page and is a real credential. There is a precedent for DB storage — `SocialAuthConfigs` holds `ClientId`/`ClientSecret` — but that table is plaintext. Choose between an environment variable (simplest, but re-authorising means a redeploy, which will not survive contact with a 60-day expiry) and a DB row with encryption at rest and SuperAdmin-only write (recommended, because the token *will* need rotating by a non-developer). Whichever is chosen, the token must never be returned by any GET endpoint, not even masked to admins. | S | 36.A1 | 2026-08-11 |
| **36.A4** | `TODO` | Process | Record the mobile parity exception explicitly | The standing policy says a feature is not done until API, web and mobile all reflect it. This feature only partly can: mobile has **no admin news/notice management screen at all** (`admin_modules.dart`'s CMS entry is gallery-only), so the review-and-import UI is web-only by necessity, not by oversight. The member-facing half needs no work — mobile's `news_screen.dart` already renders whatever `GET /news` returns, so imported posts appear automatically. Write that exception down here so a later audit does not reopen this area as "mobile never delivered". | S | 36.A2 | 2026-08-11 |

#### 36.B — Backend: connection, fetch and import

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **36.B1** | `TODO` | API | Page connection config + SuperAdmin endpoints | Store the Page id, the token (per 36.A3), the pinned Graph API version, and the timestamp of the last successful fetch. Expose `GET`/`PUT /api/facebook-ingest/config` under SuperAdmin only. The GET must return connection *status* — connected / token expiring / token expired / never configured — and must not return the token itself. | M | 36.A3 | 2026-08-11 |
| **36.B2** | `TODO` | API | Graph client with a pinned version and a loud token health check | A thin typed client over `HttpClient` (register it with `AddHttpClient` the way the payment gateways already do). **Pin the Graph API version explicitly** — Facebook retires versions on roughly a two-year cycle, and an unpinned client breaks without warning. Add an explicit `CheckConnectionAsync` that distinguishes "token expired", "token lacks the permission", "page not found" and "Facebook is down", because those need four different admin messages. Per the rule 29E.2 established, none of these may be swallowed: a failed fetch must surface a real message, never an empty list that looks like "no new posts". | M | 36.B1 | 2026-08-11 |
| **36.B3** | `TODO` | DB | `FacebookPostImport` staging entity + migration | New entity: `FacebookPostId` (string, **unique index** — this is what makes a re-fetch idempotent), `Message`, `CreatedTime`, `PermalinkUrl`, `RawPayloadJson`, `MediaSourceUrl`, `Status` (`Fetched`/`Ignored`/`Imported`), `ImportedNewsPostId` (nullable FK), `FetchedAt`, `ImportedByUserId`. Nothing is written to `NewsPosts` at fetch time. Add the migration under `Data/Migrations/PgSql` and remember local dev is SQLite via `EnsureCreated` with no SQLite migration tree (see 34.D4/34.D10). | M | 36.A2 | 2026-08-11 |
| **36.B4** | `TODO` | API | Admin-triggered fetch with paging and idempotency | `POST /api/facebook-ingest/fetch` (AdminOnly) reads `/{page-id}/published_posts` newest-first, follows paging cursors up to a bounded page count, and upserts on `FacebookPostId` so re-running it is safe and cheap. Respect Graph rate limits — fail with a clear "try again later" rather than hammering. Return a summary the UI can show: fetched, new, already-seen. Deliberately **not** a scheduled job; see the area note. | M | 36.B2, 36.B3 | 2026-08-11 |
| **36.B5** | `TODO` | Security | Download and re-host media, with an SSRF allow-list | Facebook's CDN URLs expire and rotate, so an imported post that merely links to `scontent.*` will show a broken image within weeks — the media must be downloaded and re-hosted at import time. That means the server fetches a URL supplied by a third party, so: restrict the download to Facebook CDN hostnames, cap the response size, enforce a timeout, and run the bytes through the existing `FileValidationService` magic-byte check before storing via `LocalFileStorageService` as `FileUploadType.NewsImage`. Do not trust the content-type header. | M | 36.B3 | 2026-08-11 |
| **36.B6** | `TODO` | API | Import a staged post into a NewsPost | `POST /api/facebook-ingest/{id}/import` (AdminOnly) maps the staged row onto `CreateNewsDto` and calls the existing `NewsService.CreateNewsAsync`, so HTML sanitization and every existing rule apply unchanged. Four mapping details that will bite otherwise: Facebook posts **have no title** while `CreateNewsDto.Title` requires 5–300 characters, so derive a candidate from the first line and let the admin edit it (36.C2); `Content` has a 20-character minimum, so a two-word Facebook post cannot be imported as-is and needs a clear rejection message; `PublishDate` comes from the post's `created_time`, not the import time; and `AuthorId` must be the **importing admin's user id**, because it is a non-null FK and `NewsPostConfiguration` has a global query filter on `Author != null && !Author.IsArchived` — a synthetic "Facebook" user would make every imported post vanish from the public feed the moment it was archived. Also add `POST /{id}/ignore` so the queue can be cleared without importing. | L | 36.B5 | 2026-08-11 |
| **36.B7** | `TODO` | API | Feature toggle `enableFacebookNewsIngest` | Add to `FeatureToggleDto` (`OrgConfigDto.cs`), mirror in `org-config.model.ts` and `org_config.dart`, and **default it off**. Gate the API endpoints on it as well as the UI — a toggle that only hides the button is not a toggle. The admin org-config screen picks new keys up automatically from `Object.keys(config.features)`. | S | 36.B1 | 2026-08-11 |

#### 36.C — Web admin review queue

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **36.C1** | `TODO` | Web | Review queue screen | New admin screen listing staged posts newest-first with the Facebook text, the image thumbnail, the original post date and a permalink to the source, plus filters for Fetched / Ignored / Imported. Follows the existing `admin/news` component conventions and sits behind the same `AdminOnly` route guard plus the 36.B7 feature guard. | L | 36.B4, 36.B7 | 2026-08-11 |
| **36.C2** | `TODO` | Web | Edit before import | The admin must be able to set the title (mandatory, since Facebook supplies none), choose News or Notice per 36.A2, adjust the body, and drop the image, all before committing the import. Reuse the existing `admin-news` form controls rather than building a second editor — one `NewsPost` form is enough. | M | 36.C1, 36.B6 | 2026-08-11 |
| **36.C3** | `TODO` | Web | Connection status and re-authorise flow | Surface the 36.B2 health check where an admin will actually see it, and make the expiry path recoverable without a developer: a clear "the Facebook connection has expired, paste a new token" state on the org-config or ingest screen. A 60-day token that silently stops working is the most likely way this feature dies quietly. | M | 36.B2, 36.C1 | 2026-08-11 |

#### 36.D — Tests and rollout

| # | Status | Component | Title | Description | Est. | Depends on | Raised |
|---|---|---|---|---|---|---|---|
| **36.D1** | `TODO` | Tests | API unit tests | Cover the mapping and the guards: staged row → `CreateNewsDto`; the title-derivation rule; rejection when the body is under 20 characters; `PublishDate` taken from `created_time`; `AuthorId` set to the importing admin; re-fetch of a known `FacebookPostId` updates rather than duplicates; import of an already-imported row is refused; and every endpoint refuses a non-admin. Follow the existing `NewsServiceTests` / `NewsControllerTests` shape. | M | 36.B6 | 2026-08-11 |
| **36.D2** | `TODO` | Tests | Contract test against recorded Graph payloads | Save real (redacted) Graph responses as fixtures and test the client against them — a text-only post, a photo post, a multi-photo post, a shared link, a Bengali-language post, and an error envelope for an expired token. This is what catches a Graph version bump breaking the parse, and it runs without network access or a live token. | M | 36.B2 | 2026-08-11 |
| **36.D3** | `TODO` | Tests | Playwright E2E for the admin journey | Fetch (with the Graph call intercepted) → queue shows posts → edit title → import as Notice → the item appears on `/news?type=Notice`. Also assert the feature-off path: with `enableFacebookNewsIngest` false, the route is not reachable. | M | 36.C2, 36.B7 | 2026-08-11 |
| **36.D4** | `TODO` | Tests | Confirm imported posts render on mobile | No mobile code should be needed — `news_screen.dart` and `news_details_screen.dart` already render whatever `GET /news` returns. Prove it rather than assume it: an imported post with a re-hosted image and a Bengali body must render correctly in the list and the detail view. Regenerate the affected goldens if the fixtures change. | S | 36.B6 | 2026-08-11 |
| **36.D5** | `TODO` | Process | Staged rollout | Ship with the toggle off. Enable for one admin, import a handful of real posts, and let them sit for a fortnight — long enough for the CDN URLs of any non-re-hosted media to rotate, which is the cheapest way to prove 36.B5 actually works. Only then enable generally. | S | 36.D1, 36.D3 | 2026-08-11 |

---

## Part 2 — Completed work

One row per delivered item. The original entry — including the long implementation and
verification notes — is preserved verbatim in the collapsible block under each area.

### AREA 1: MOBILE PLATFORM STABILITY & PARITY

24 items

| # | Status | Component | Summary |
|---|---|---|---|
| 1.1 | `DONE` | Mobile | Fix AdminLedgerScreen class name mismatch in app_router.dart |
| 1.2 | `DONE` | Mobile | Audit/Match Registration Wizard labels to Web equivalents |
| 1.3 | `DONE` | Mobile | Sync RegisterModel with backend Member.cs (FatherName, MotherName, Emergency fields) |
| 1.4 | `DONE` | Mobile | Audit and match all mobile screen titles/labels to core domain terminology |
| 1.5 | `DONE` | Tests | Verify CRUD operations on all screens (Profile, Approvals, Events, Jobs, News, Gallery, Fee Config, Governance Assignment, Contact Messages) |
| 1.6 | `DONE` | Mobile | Article Submission & Approval logic on mobile (Editorial) |
| 1.7 | `DONE` | Mobile | Implement Mobile Messaging/Chat module (SignalR integration) |
| 1.8 | `DONE` | Mobile | Synchronize UI validations with API/DB nullability constraints |
| 1.9 | `DONE` | Mobile | Implement Automated 401 Session Redirection |
| 1.10 | `DONE` | Mobile | Implement 10-minute Inactivity Logout (Idle detection) |
| 1.11 | `DONE` | Mobile | Synchronize Administrative Member Management (Status/Category parity) |
| 1.12 | `DONE` | Mobile | Implement Optimized Media Delivery (CachedNetworkImage + Progressive Loading) |
| 1.13 | `DONE` | Mobile | Implement High-Reliability Error Boundaries (Global Catch-all) |
| 1.14 | `DONE` | Mobile | Global Haptic Feedback ecosystem for primary interactions |
| 1.15 | `DONE` | Mobile | Integrated Home Login Experience (no redirect jump) |
| 1.16 | `DONE` | Mobile | Unified registration portal shortcut on dashboard |
| 1.17 | `DONE` | Mobile | High-fidelity UI dividers and spatial consistency on edit screens |
| 1.18 | `DONE` | Mobile | Global Top Navbar Visibility Control (Drawer toggle) |
| 1.19 | `DONE` | Mobile | Route-aware back buttons on all AppScaffold instances |
| 1.20 | `DONE` | Mobile | Fix: Persistent roleProvider to prevent admin UI leakage in member sessions |
| 1.21 | `DONE` | Mobile | Fix: Dashboard profile completeness shared utility |
| 1.22 | `DONE` | Mobile | Fix: Dashboard banner typography and membership badge logic |
| 1.23 | `DONE` | Mobile | Fix: Gatekeeper QR scanner overlay rendering |
| 1.24 | `DONE` | Mobile | Integrated skeleton/shimmer screens for all async loading states |

### AREA 2: CORE ALUMNI MANAGEMENT & REGISTRY

8 items

| # | Status | Component | Summary |
|---|---|---|---|
| 2.1 | `DONE` | API | Member Academic/Professional record migration and mapping |
| 2.2 | `DONE` | API | Public Directory Enhancements (Batch/Type/Category visibility) |
| 2.3 | `DONE` | API | Membership Lifecycle: Spouse/Family linking (wired + routed) |
| 2.4 | `DONE` | API | Membership Lifecycle: "Blue Tick" Verified status control |
| 2.5 | `DONE` | API | Membership Lifecycle: Soft-delete cascading (IsArchived architecture) |
| 2.6 | `DONE` | API | Automated ID & Certificate Generation (PDF + QR) |
| 2.7 | `DONE` | API | Profile UI refinement (Education/Professional record edit buttons) |
| 2.8 | `DONE` | API | Create offline data collection templates (Google Forms) for manual member & event migration matching DB validations |

### AREA 3: COMMUNICATION & SOCIAL

7 items

| # | Status | Component | Summary |
|---|---|---|---|
| 3.1 | `DONE` | API | Real-time Communication Bridge (SignalR Admin Alerts) |
| 3.2 | `DONE` | API | Member Chat/Noticeboard Framework & Services |
| 3.3 | `DONE` | API | HTML Templating system for system notifications |
| 3.4 | `DONE` | API | SMS Gateway Integration (Greenweb/SSL Wireless) |
| 3.5 | `DONE` | Mobile | Networking: Privacy controls (visibility toggles respect DTO masking) |
| 3.6 | `DONE` | Mobile | Networking: Mentorship request flow in Job Hub |
| 3.7 | `DONE` | API | Discussion forums and community groups |

### AREA 4: EVENTS & GATHERINGS

6 items

| # | Status | Component | Summary |
|---|---|---|---|
| 4.1 | `DONE` | API | Automated registration closing for past/due events |
| 4.2 | `DONE` | Web | Landing Page: Featured event display with last closed history |
| 4.3 | `DONE` | API | Flexible Pricing: Free/Paid toggles and registration windows |
| 4.4 | `DONE` | API | Event Media: Logo upload and detail view rendering |
| 4.5 | `DONE` | API | My Participations: Payment gateway integration |
| 4.6 | `DONE` | API | Advanced: Waitlist management and QR Attendance scanning |

### AREA 5: FINANCIAL & ADMIN GOVERNANCE

6 items

| # | Status | Component | Summary |
|---|---|---|---|
| 5.1 | `DONE` | API | Smart Payment Gateway Automation (Webhooks for bKash/Nagad/SSL) |
| 5.2 | `DONE` | API | EC Management: Term configuration and role propagation |
| 5.3 | `DONE` | API | Constraint: Single EC role per member per period |
| 5.4 | `DONE` | API | Automated PDF Tax/Donation Receipts generation |
| 5.5 | `DONE` | API | Claims-based Auth: PermissionsMatrixScreen for role management |
| 5.6 | `DONE` | API | Governance Registry: Admin assignment UI |

### AREA 6: CAREER & OPPORTUNITIES

1 items

| # | Status | Component | Summary |
|---|---|---|---|
| 6.1 | `DONE` | API | Professional Hub: Alumni directory LinkedIn-style filters |

### AREA 7: SECURITY, INFRASTRUCTURE & HARDENING

13 items · completed 2026-08-11

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 7.1 | `DONE` | API | API: Fix Member Login with NID (401 resolution) | — |
| 7.2 | `DONE` | API | API: Fix Password Reset timeout logic | — |
| 7.3 | `DONE` | API | API: Real-time Session Termination upon status change | — |
| 7.4 | `DONE` | API | API: Secure Admin routes with SuperAdmin functional guards | — |
| 7.5 | `DONE` | API | API: Event Note data leakage fix (null out sensitive notes in public lists) | — |
| 7.6 | `DONE` | Web | Web: Profile rendering pascal/camel case mapping stability | — |
| 7.7 | `DONE` | Web | Web: Image path prefixing sync across all components | — |
| 7.8 | `DONE` | Web | Web: Event Date validation (StartDate < EndDate enforcement) | — |
| 7.9 | `DONE` | Web | Web: Readonly field protection in Profile update payloads | — |
| 7.10 | `DONE` | Mobile | Mobile: Credentials stored only upon explicit biometric opt-in | — |
| 7.11 | `DONE` | Mobile | Mobile: SharedPreferences preservation on logout (preserving layout) | — |
| 7.12 | `DONE` | Mobile | Mobile: Unified userProfileProvider to prevent sync-re-fetch loops | — |
| 7.14 | `DONE` | Security | Security: Biometric Authentication (FaceID/Fingerprint) | 2026-08-11 |

<details>
<summary>AREA 7 — original entries and completion notes (13 items)</summary>

```text
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
7.14 [TODO] Security: Biometric Authentication (FaceID/Fingerprint)
     [2026-08-11 audit] Verified already shipped by the 2026-08-11 audit. Biometric login is wired end-to-end: `biometric_service.dart` (local_auth), the "Enable Biometric Access" opt-in on `login_screen.dart`, credential storage via `storage_service.saveCredentials`, and the "Unlock with Biometrics" entry point in `app_home_screen.dart`. Duplicated 7.10/10.9/11.9, which were already DONE.
```

</details>

### AREA 8: MOBILE ENGINEERING (TIER-1 STANDARDS)

5 items

| # | Status | Component | Summary |
|---|---|---|---|
| 8.1 | `DONE` | Web | UI: Enforce 8pt grid and standard design tokens globally |
| 8.2 | `DONE` | Mobile | Nav: Adaptive layout for Tablets/Pads (Sidebar architecture) |
| 8.9 | `DONE` | DevOps | CI/CD: Fastlane + GitHub Actions Deployment Pipeline |
| 8.10 | `DONE` | Tests | Quality: Global Error Boundary and Sentry/Firebase tracing |
| 8.11 | `DONE` | DevOps | CI/CD: Operationalize multi-environment pipelines (Preprod/Standard) |

### AREA 9: INSTITUTIONAL GOVERNANCE & QUALITY

8 items

| # | Status | Component | Summary |
|---|---|---|---|
| 9.1 | `DONE` | API | Digital Constitution: Versioned legal repository |
| 9.2 | `DONE` | API | Amendment Voting: Secure participation for verified alumni |
| 9.3 | `DONE` | API | Collaborative Editorial: Multi-user news workflows |
| 9.4 | `DONE` | Mobile | Mobile Governance Portal (/committee route) |
| 9.5 | `DONE` | API | Public Transparency: Categorical sitemap in footer |
| 9.6 | `DONE` | Process | Documentation: System Architecture & Data Flow blueprint |
| 9.7 | `DONE` | Tests | Quality: GitHub PR Template + Sequential CI (API -> UI -> MOBILE) |
| 9.8 | `DONE` | Tests | Quality: Comprehensive Testing suite integration |

### AREA 10: USER FEEDBACK & RECENT ISSUES (PHASE 2)

11 items

| # | Status | Component | Summary |
|---|---|---|---|
| 10.1 | `DONE` | Mobile | Unify Profile Completeness Logic: Sync Backend (10 fields) with Mobile (11 fields) |
| 10.2 | `DONE` | Mobile | Fix Web/Mobile 404s: Alias News/Pending, Networking/Directory, Financial/Ledger, Governance/Current, Governance/Constitution, Notification (case sensitivity) |
| 10.3 | `DONE` | Mobile | Fix Mobile API Prefixes: Resolve double /api/api prefix in feature-specific calls (Governance, etc.) |
| 10.4 | `DONE` | Mobile | Governance UI (Web): Show Name, Photo, Position, ID, Type in EC list |
| 10.5 | `DONE` | Mobile | Family Link: Enable "Search by Name" for linking family members |
| 10.6 | `DONE` | Web | UX: Add "Back/Close" buttons to all modal-style screens (e.g. Profile) |
| 10.7 | `DONE` | Mobile | Digital ID: Implement high-fidelity card layout design |
| 10.8 | `DONE` | Mobile | News Fix: Resolve 500 mapping error for ArticleCategory=Magazine |
| 10.9 | `DONE` | Mobile | Mobile Stability: Gracefully handle Biometric Auth errors (local_auth) on Web platform |
| 10.10 | `DONE` | Mobile | Role Management: Fix session leakage where a member sees the Admin Dashboard/Badge after login |
| 10.11 | `DONE` | Mobile | Data Display: Fix "Batch: N/A" for members (ensure PassingYear is correctly mapped and rendered) |

### AREA 11: API PARITY AUDIT & FIX LIST

9 items

| # | Status | Component | Summary |
|---|---|---|---|
| 11.1 | `DONE` | API | Fix: api/governance/ec/current -> 404 (Controller route verified correct; path was correct) |
| 11.2 | `DONE` | API | Fix: api/governance/constitution -> 404 (Controller route verified correct) |
| 11.3 | `DONE` | API | Fix: Mobile calling api/Notification (capital N, path wrong); correct endpoint is api/notifications |
| 11.4 | `DONE` | API | Fix: Mobile financial_service calls /financial/ledger, should be /ledger |
| 11.5 | `DONE` | API | Fix: Mobile assistant_service calls /api/assistant/ask (double-api); should be /assistant/ask |
| 11.6 | `DONE` | API | Fix: api/networking/directory -> 404 (alias added to NetworkingController.cs) |
| 11.7 | `DONE` | API | Fix: api/news/admin/pending -> 404 (alias added to NewsController.cs) |
| 11.8 | `DONE` | API | Fix: auth role leakage - clear session completely before saving new login role |
| 11.9 | `DONE` | API | Fix: Mobile biometric (local_auth) crash on Flutter Web; guard with kIsWeb check |

### AREA 13: VISUAL TESTING & QUALITY FREEZE

5 items

| # | Status | Component | Summary |
|---|---|---|---|
| 13.1 | `DONE` | API | API: Implement 'Seed Data' profile for visual tests (migration-safe, Visual env-gated) |
| 13.2 | `DONE` | API | Infra: Define Visual Test Storage (Baseline/Failure/Diff) — ARCH.md created |
| 13.3 | `DONE` | Process | Scripting: Unified 'visual-check.ps1' with 4-stage runner, suite filtering, update-baselines flag |
| 13.4 | `DONE` | Tests | UI Quality Freeze: COMPREHENSIVE module-by-module coverage: |
| 13.5 | `DONE` | DevOps | Runner upgraded: -VisualOnly, -E2EOnly, -Suite, -UpdateBaselines, colored summary |

<details>
<summary>AREA 13 — original entries and completion notes (5 items)</summary>

```text
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
```

</details>

### AREA 14: FUNCTIONAL E2E (END-USER TESTING)

5 items

| # | Status | Component | Summary |
|---|---|---|---|
| 14.1 | `DONE` | Web | Web: Playwright member journey (Login -> Dashboard -> ID Card -> Logout) |
| 14.2 | `DONE` | Mobile | Mobile: Integration test journey (Login -> Dashboard -> ID Card -> Logout) |
| 14.3 | `DONE` | Web | Web: Admin approval workflow E2E |
| 14.4 | `DONE` | Mobile | Mobile: Financial ledger verification E2E |
| 14.5 | `DONE` | Cross-platform | Cross-Platform: Article contribution & editorial approval E2E (Web + Mobile) |

### AREA 15: SOCIAL AUTH & ONBOARDING

6 items

| # | Status | Component | Summary |
|---|---|---|---|
| 15.1 | `DONE` | API | API: Add GoogleId and FacebookId to User entity |
| 15.2 | `DONE` | API | API: Add IsProfileComplete to Member entity |
| 15.3 | `DONE` | API | API: Implement Social Auth (Google/Facebook) logic & Admin Config |
| 15.4 | `DONE` | Web | Web: Implement "Login with Google/Facebook" buttons |
| 15.6 | `DONE` | Cross-platform | Cross-Platform: Implementation of Onboarding Flow (Profile Setup -> Payment -> Approval) |
| 15.7 | `DONE` | API | Backend: Unit tests for Social Auth and Onboarding logic |

### AREA 16: POLLS & VOTING

7 items

| # | Status | Component | Summary |
|---|---|---|---|
| 16.1 | `DONE` | API | Domain: Create Poll, PollOption, and PollVote models |
| 16.2 | `DONE` | API | API: IPollService and PollService implementation |
| 16.3 | `DONE` | API | API: PollController for admin and member actions |
| 16.4 | `DONE` | Web | Web: Admin UI for Poll Management |
| 16.5 | `DONE` | Web | Web: Member UI for Poll Voting & Results |
| 16.6 | `DONE` | Mobile | Mobile: Member UI for Poll Voting & Results |
| 16.7 | `DONE` | API | Backend: Unit tests for Polls and Voting logic |

### AREA 17: REGRESSION & STABILITY

5 items

| # | Status | Component | Summary |
|---|---|---|---|
| 17.1 | `DONE` | API | API: Verify existing Auth flows (NID/Password) remain functional |
| 17.2 | `DONE` | API | API: Verify Member Registration and Approval workflows remain functional |
| 17.3 | `DONE` | Web | Web: Verify full member lifecycle (Login -> Profile -> Dashboard) |
| 17.4 | `DONE` | Mobile | Mobile: Verify full member lifecycle (Login -> Profile -> Dashboard) |
| 17.5 | `DONE` | Cross-platform | Cross-Platform: Run all existing Playwright and Flutter integration tests |

### AREA 18: GENERAL MAINTENANCE & STABILITY

9 items

| # | Status | Component | Summary |
|---|---|---|---|
| 18.1 | `DONE` | Mobile | Mobile: Fix unused import in registration_visual_test.dart |
| 18.2 | `DONE` | Mobile | Mobile: Upgrade Governance UI (Fonts, GlassContainer, Image resolution) |
| 18.3 | `DONE` | Web | Web: Refine Roles Management UI (Input padding, Fancy dropdowns, Smart selection) |
| 18.4 | `DONE` | API | Backend: Seed Constitution data to resolve Governance 404s |
| 18.5 | `DONE` | Mobile | Mobile: If not connected with net, show error on mobile app |
| 18.6 | `DONE` | Mobile | Mobile: Run 'flutter pub get' in GHCAA.Mobile to resolve connectivity_plus dependency errors |
| 18.7 | `DONE` | API | Backend: Start GHCAA.API to resolve ECONNREFUSED (port 5087) errors |
| 18.8 | `DONE` | API | Backend: Update PostgreSQL password in appsettings.Development.json if SyncMembersForReal test fails locally |
| 18.9 | `DONE` | Tests | UI Audit: Review all SCSS files for hardcoded #fff or #000 that break theme accessibility |

### AREA 19: PAYMENT VERIFICATION & POLICY

3 items

| # | Status | Component | Summary |
|---|---|---|---|
| 19.1 | `DONE` | API | Payments: Verify Registration Fee configuration in Admin Portal |
| 19.2 | `DONE` | API | Payments: Verify Registration Fee status on Member Dashboard (Profile Completion Wizard) |
| 19.3 | `DONE` | API | Payments: Ensure Registration Fee is mandatory for all members as per latest policy |

### AREA 20: COMPREHENSIVE E2E COVERAGE (ALL FEATURES)

2 items

| # | Status | Component | Summary |
|---|---|---|---|
| 20.1 | `DONE` | Web | Web: Expand Playwright E2E suite to cover all core portal features (Messaging, Job Hub, Alumni Directory, Events, Gallery, Governance, My Articles, My Profile) |
| 20.2 | `DONE` | Mobile | Mobile: Expand Flutter integration/visual tests to cover all core mobile features (Messaging, Job Hub, Alumni Directory, Events, Gallery, Governance, My Articles, My Profile, Admin Modules) |

### AREA 21: TEST DATA MANAGEMENT & VISUAL AUTOMATION

5 items

| # | Status | Component | Summary |
|---|---|---|---|
| 21.1 | `DONE` | Tests | Quality: Create 'test-dataset.json' with comprehensive edge cases (Large names, missing photos, various membership tiers) |
| 21.2 | `DONE` | Tests | Quality: Implement 'scripts/setup-test-data.ps1' to inject test dataset into active environment (separate from seed) |
| 21.3 | `DONE` | Tests | Quality: Implement 'scripts/cleanup-test-data.ps1' to revert environment to clean/seed state |
| 21.4 | `DONE` | Tests | Quality: Implement 'scripts/run-visual-tests.ps1' to execute all visual regressions with the test dataset |
| 21.5 | `DONE` | Tests | Quality: Integrate visual test report generation (HTML) for local review |

### AREA 22: PORTAL FEATURE HARDENING (E2E)

5 items

| # | Status | Component | Summary |
|---|---|---|---|
| 22.1 | `DONE` | Tests | E2E: Verify Messaging flow (Member <-> Admin) with real-time checks |
| 22.2 | `DONE` | Tests | E2E: Verify Job Hub (Post -> Review -> View) workflow |
| 22.3 | `DONE` | Tests | E2E: Verify Alumni Directory filtering and search precision |
| 22.4 | `DONE` | Tests | E2E: Verify Event Registration and QR generation flow |
| 22.5 | `DONE` | Tests | E2E: Verify Gallery upload and album organization (Admin side) |

### AREA 23: ECOSYSTEM-WIDE DATE STANDARDIZATION (dd-MM-yyyy)

6 items

| # | Status | Component | Summary |
|---|---|---|---|
| 23.1 | `DONE` | API | API: Implement DateFormatConverter for unified dd-MM-yyyy/ISO parsing |
| 23.2 | `DONE` | Web | Web: Standardize all Angular date inputs to dd-mm-yyyy (Registration, Profile, Events, Admin) |
| 23.3 | `DONE` | Web | Web: Update date validation logic and labels to guide users on dd-mm-yyyy format |
| 23.4 | `DONE` | Mobile | Mobile: Refactor AppUtils with parseDate/formatDate supporting dd-MM-yyyy standard |
| 23.5 | `DONE` | Mobile | Mobile: Update all screens (Registration, Profile, Events, Jobs, Gallery) to use standardized dates |
| 23.6 | `DONE` | Tests | E2E: Update Playwright test suite to use dd-mm-yyyy for all automated date entries |

### AREA 24: SECURITY HARDENING (from full-stack code review — 2026-05-02)

60 items

#### 24-A: CRITICAL — Authentication & Token Backdoors

| # | Status | Component | Summary |
|---|---|---|---|
| 24.1 | `DONE` | Security | Security: Gate VisualTestAuthMiddleware behind Development env + ASP_SEED_PROFILE=Visual; reject "Bearer visual_*" in all other environments (VisualTestAuthMiddleware.cs) |
| 24.2 | `DONE` | Security | Security: Remove committed JWT fallback key "LOCAL_DEVELOPMENT_JWT_FALLBACK_32_CHARS_MIN"; generate ephemeral random 32-byte key on dev startup, require User Secrets… |
| 24.3 | `DONE` | Security | Security: Remove DB passwords from appsettings.Development.json and appsettings.json; enforce env-var / User Secrets only (appsettings.*.json) |
| 24.4 | `DONE` | Security | Security: Add [Authorize] attribute to ChatHub class (ChatHub.cs) |
| 24.5 | `DONE` | Security | Security: Add [Authorize] attribute to NotificationHub class; gate JoinBatch/JoinDepartment to authenticated callers (NotificationHub.cs) |
| 24.6 | `DONE` | Security | Security: Replace non-thread-safe Dictionary<string,string> in ChatHub with ConcurrentDictionary; support multi-device per user (ChatHub.cs) |

#### 24-B: CRITICAL — Sensitive Data in Responses

| # | Status | Component | Summary |
|---|---|---|---|
| 24.7 | `DONE` | API | API: Do not return DefaultPassword in ApproveMember HTTP response; send via email only, return PasswordEmailed: true flag (AdminController.cs) |
| 24.8 | `DONE` | API | API: Do not return ResetUrl (containing reset token) in ResetPasswordAdmin response; force email-only delivery (AdminController.cs) |
| 24.9 | `DONE` | API | API: Do not return ClientSecret/GatewaySecretKey in AdminSocialAuthController.GetConfigs or PaymentConfigController responses; mask or omit secrets (AdminSocialAuthController.cs… |

#### 24-C: CRITICAL — Payment Gateway Security

| # | Status | Component | Summary |
|---|---|---|---|
| 24.10 | `DONE` | API | Payment: Implement HMAC/signature verification on bKash webhook before trusting paymentID (BkashGateway.cs ProcessWebhookAsync) |
| 24.11 | `DONE` | API | Payment: Implement verify_sign HMAC check on SSLCommerz webhook; verify store_passwd hash against all callback fields (SSLCommerzGateway.cs VerifyCallbackAsync) |
| 24.12 | `DONE` | API | Payment: Verify executed payment amount against the originating PaymentHistory amount for bKash (BkashGateway.cs VerifyCallbackAsync) |
| 24.13 | `DONE` | API | Payment: Add idempotency — store paymentID/val_id in PaymentHistory with UNIQUE constraint; short-circuit on duplicate callback (both gateways) |
| 24.14 | `DONE` | API | Payment: Fix HttpClient.DefaultRequestHeaders mutation race in BkashGateway (3 call sites); use per-request HttpRequestMessage headers (BkashGateway.cs) |
| 24.15 | `DONE` | API | Payment: Derive CallbackUrl exclusively from server-side config (AppSettings:PublicApiBaseUrl), never from request.BaseUrl supplied by the client (GatewaysController.cs) |
| 24.16 | `DONE` | API | Payment: Replace hardcoded fee fallback of 500 BDT and adminId fallback of "1" with startup-time config validation that fails loudly (GatewaysController.cs) |
| 24.17 | `DONE` | API | Payment: Fix SSLCommerz form body parser — split on '=' truncates base64 values; use QueryHelpers.ParseQuery instead (SSLCommerzGateway.cs ProcessWebhookAsync) |

#### 24-D: CRITICAL — File & Path Security

| # | Status | Component | Summary |
|---|---|---|---|
| 24.18 | `DONE` | Security | Security: Fix path traversal in SecureFilesController — after Path.GetFullPath, assert path starts with the secure-uploads root; reject any path containing ".." (SecureFilesController.cs) |

#### 24-E: CRITICAL — OTP Service

| # | Status | Component | Summary |
|---|---|---|---|
| 24.19 | `DONE` | Security | Security: Replace new Random() with RandomNumberGenerator.GetInt32(100_000, 1_000_000) for cryptographically secure OTP generation (OtpService.cs) |
| 24.20 | `DONE` | Security | Security: Store OTP as HMAC-SHA256(code, email), not cleartext; a DB compromise reveals all active codes (OtpService.cs) |
| 24.21 | `DONE` | Security | Security: Add per-email attempt counter — lock OTP after 5 wrong attempts; add Purpose field (EmailVerify, PasswordReset) to prevent cross-use (OtpService.cs) |
| 24.22 | `DONE` | Security | Security: Invalidate all previous unverified OTPs for the same email before inserting a new one (OtpService.cs) |

#### 24-F: HIGH — Authentication Service

| # | Status | Component | Summary |
|---|---|---|---|
| 24.23 | `DONE` | Security | Security: Add brute-force lockout to LoginAsync — track FailedLoginAttempts + LockoutUntil on User, lock 15 min after 5 failures (AuthService.cs) |
| 24.24 | `DONE` | Security | Security: Fix username enumeration timing attack — always run a dummy BCrypt.Verify when user is not found to normalize response time (AuthService.cs) |
| 24.25 | `DONE` | Security | Security: Fix social auto-link — only link IdP email to local account when member.EmailVerified == true (AuthService.cs SocialLoginAsync) |
| 24.26 | `DONE` | Security | Security: Social signup uses unique SOCIAL-{PROVIDER}-{socialId} sentinel instead of "TBD" for NID/MobileNo — prevents UNIQUE index crash on second social signup (AuthService.cs… |
| 24.27 | `DONE` | Security | Security: JWT lifetime reduced to 60 min; opaque refresh token (SHA-256 hash stored in RefreshTokens table, 7-day cookie rotation) implemented in TokenService.cs + RefreshToken entity +… |
| 24.28 | `DONE` | Security | Security: Rotate SecurityStamp on password reset and password change to invalidate all previous JWTs (AuthService.cs, UserService.cs) |

#### 24-G: HIGH — Data Integrity (MemberService)

| # | Status | Component | Summary |
|---|---|---|---|
| 24.29 | `DONE` | Security | Bug: Fix membership number lexicographic race — OrderByDescending(m.Id) instead of string sort; prevents regression at 999→1000 rollover (MemberService.cs RegisterAsync + ApproveMemberAsync) |
| 24.30 | `DONE` | Security | Bug: Replace hard-delete of rejected applicants with soft-delete (Status=Rejected, IsArchived=true) preserving audit trail; added Rejected to MembershipStatus enum (MemberService.cs… |
| 24.31 | `DONE` | Security | Bug: CreateUserAccountAsync moved inside the serializable transaction in ApproveMemberAsync — member+user are now atomic (MemberService.cs) |
| 24.32 | `DONE` | Performance | Perf: BulkArchiveInactiveMembersAsync replaced N+1 ArchiveMemberAsync loop with bulk ExecuteUpdateAsync calls for all cascades (MemberService.cs) |

#### 24-H: HIGH — Missing DB Indexes

| # | Status | Component | Summary |
|---|---|---|---|
| 24.33 | `DONE` | Performance | Perf: Add index on User.MemberId (hot lookup path in Auth, Member services) |
| 24.34 | `DONE` | Performance | Perf: Add partial index on User.ResetToken WHERE ResetToken IS NOT NULL |
| 24.35 | `DONE` | Performance | Perf: Add index on User.GoogleId, User.FacebookId (social login hot path) |
| 24.36 | `DONE` | Performance | Perf: Add composite index on Otp(Email, ExpiryAt) — OTP verification scans table without it |
| 24.37 | `DONE` | Performance | Perf: Add composite index on Member(Status, IsArchived) — heavily filtered in admin listing queries (MemberConfiguration.cs) |
| 24.38 | `DONE` | Performance | Perf: Add entity configuration file for Otp table (currently using EF defaults — no indexes, no column length caps on a security-critical table) |

#### 24-I: HIGH — Frontend (Angular)

| # | Status | Component | Summary |
|---|---|---|---|
| 24.39 | `DONE` | Security | Security: Move JWT from localStorage to httpOnly + Secure + SameSite=Strict cookie set by backend; token no longer stored in localStorage; Angular uses /auth/me on init; sessionStorage… |
| 24.40 | `DONE` | Security | Security: Wrap getUserFromStorage() JSON.parse in try/catch; fall back to null and clear corrupt entry (auth.service.ts) |
| 24.41 | `DONE` | Security | Security: Check JWT exp claim on storage restore — drop session if token is already expired (auth.service.ts) |
| 24.42 | `DONE` | Security | Security: Sanitize article HTML server-side with Ganss.HtmlSanitizer before storage in NewsService.CreateNewsAsync + UpdateNewsAsync; [innerHTML] now safe (NewsService.cs, magazine.html) |
| 24.43 | `DONE` | Security | Security: Restrict Authorization header injection in globalHttpInterceptor to /api/* URLs only, not all outgoing HttpClient requests (global-http.interceptor.ts) |
| 24.44 | `DONE` | Security | Security: Token refresh implemented — 401 queuing with BehaviorSubject, POST /auth/refresh retried, all concurrent 401s share single refresh call, logout only on refresh failure… |
| 24.45 | `DONE` | Security | Security: Client-side file validation added — images ≤ 5 MB (JPEG/PNG/WebP), PDFs ≤ 10 MB; shared validateUploadFile() utility applied to register.ts, gallery.ts, admin-members.ts… |
| 24.46 | `DONE` | Security | Security: Enforce mustChangePassword redirect — after login, route to change-password screen before allowing any navigation (auth.service.ts, auth.guard.ts) |

#### 24-J: HIGH — API Controller Hardening

| # | Status | Component | Summary |
|---|---|---|---|
| 24.47 | `DONE` | Security | Security: Restrict QueryStringTokenMiddleware to GET-only on /api/secure-files/ and /api/uploads/ paths (QueryStringTokenMiddleware.cs) |
| 24.48 | `DONE` | Security | Security: PartitionedRateLimiter keyed on (IP, username) for login endpoint; LoginRateLimitMiddleware peeks body username before rate limiter runs (Program.cs, LoginRateLimitMiddleware.cs) |
| 24.49 | `DONE` | Security | Security: Add startup validator that throws if AllowedOrigins is empty in Production (Program.cs) |
| 24.50 | `DONE` | Security | Security: Use int.TryParse for MemberId claim parsing; return Unauthorized on missing/invalid claim (PollController.cs, FamilyLinkController.cs) |
| 24.51 | `DONE` | Security | Security: Read admin identity from User.FindFirst("MemberId") JWT claim in ApproveMember and RejectMember (AdminController.cs) |

#### 24-K: MEDIUM — Validator & Input Hardening

| # | Status | Component | Summary |
|---|---|---|---|
| 24.52 | `DONE` | Tests | Validation: Add MaximumLength and safe-character regex to FatherName, MotherName, PresentAddress, PermanentAddress, EmergencyContactName, EmergencyContactRelation, EmergencyContactPhone in… |
| 24.53 | `DONE` | Tests | Validation: Add EmergencyContactPhone format validation (same regex as MobileNo) (MemberRegistrationValidator.cs) |
| 24.54 | `DONE` | Tests | Validation: Add minimum age gate (LessThan(UtcNow.AddYears(-13))) and sanity lower bound (GreaterThan(UtcNow.AddYears(-120))) to DateOfBirth (MemberRegistrationValidator.cs) |
| 24.55 | `DONE` | Tests | Validation: Add MaximumLength(254) to Email in VerifyEmailValidator; add caps to AcademicHistory.Subject and Result fields (MemberRegistrationValidator.cs, VerifyEmailValidator.cs) |
| 24.56 | `DONE` | Tests | Validation: Enforce AdmissionYear < PassingYear in academic record child rules (MemberRegistrationValidator.cs) |

#### 24-L: MEDIUM — CSP & Security Headers

> **CREDENTIALS**
> SuperAdmin: superadmin / SuperAdminPassword123!
> Developer Admin: shalin / Shalin@2024!
> TEST_MEMBER: demo_user / DemoPass123!
> Mobile: If not connected with net, show error on mobile app

| # | Status | Component | Summary |
|---|---|---|---|
| 24.57 | `DONE` | Security | Security: Remove 'unsafe-inline' from script-src in CSP header; use nonces or hashes (SecurityHeadersMiddleware.cs) |
| 24.58 | `DONE` | Security | Security: Register SecurityHeadersMiddleware before UseStaticFiles so static file responses also receive security headers (Program.cs) |
| 24.59 | `DONE` | Security | Security: Remove deprecated X-XSS-Protection header (SecurityHeadersMiddleware.cs) |
| 24.60 | `DONE` | Security | Security: Add window.open(..., '_blank', 'noopener,noreferrer') to all gallery/event external link openings to prevent tab-napping (gallery.ts, events.ts) |

<details>
<summary>AREA 24 — original entries and completion notes (60 items)</summary>

```text
24.1  [DONE] Security: Gate VisualTestAuthMiddleware behind Development env + ASP_SEED_PROFILE=Visual; reject "Bearer visual_*" in all other environments (VisualTestAuthMiddleware.cs)
24.2  [DONE] Security: Remove committed JWT fallback key "LOCAL_DEVELOPMENT_JWT_FALLBACK_32_CHARS_MIN"; generate ephemeral random 32-byte key on dev startup, require User Secrets (JwtSigningKeyResolver.cs)
24.3  [DONE] Security: Remove DB passwords from appsettings.Development.json and appsettings.json; enforce env-var / User Secrets only (appsettings.*.json)
24.4  [DONE] Security: Add [Authorize] attribute to ChatHub class (ChatHub.cs)
24.5  [DONE] Security: Add [Authorize] attribute to NotificationHub class; gate JoinBatch/JoinDepartment to authenticated callers (NotificationHub.cs)
24.6  [DONE] Security: Replace non-thread-safe Dictionary<string,string> in ChatHub with ConcurrentDictionary; support multi-device per user (ChatHub.cs)
24.7  [DONE] API: Do not return DefaultPassword in ApproveMember HTTP response; send via email only, return PasswordEmailed: true flag (AdminController.cs)
24.8  [DONE] API: Do not return ResetUrl (containing reset token) in ResetPasswordAdmin response; force email-only delivery (AdminController.cs)
24.9  [DONE] API: Do not return ClientSecret/GatewaySecretKey in AdminSocialAuthController.GetConfigs or PaymentConfigController responses; mask or omit secrets (AdminSocialAuthController.cs, PaymentConfigController.cs)
24.10 [DONE] Payment: Implement HMAC/signature verification on bKash webhook before trusting paymentID (BkashGateway.cs ProcessWebhookAsync)
24.11 [DONE] Payment: Implement verify_sign HMAC check on SSLCommerz webhook; verify store_passwd hash against all callback fields (SSLCommerzGateway.cs VerifyCallbackAsync)
24.12 [DONE] Payment: Verify executed payment amount against the originating PaymentHistory amount for bKash (BkashGateway.cs VerifyCallbackAsync)
24.13 [DONE] Payment: Add idempotency — store paymentID/val_id in PaymentHistory with UNIQUE constraint; short-circuit on duplicate callback (both gateways)
24.14 [DONE] Payment: Fix HttpClient.DefaultRequestHeaders mutation race in BkashGateway (3 call sites); use per-request HttpRequestMessage headers (BkashGateway.cs)
24.15 [DONE] Payment: Derive CallbackUrl exclusively from server-side config (AppSettings:PublicApiBaseUrl), never from request.BaseUrl supplied by the client (GatewaysController.cs)
24.16 [DONE] Payment: Replace hardcoded fee fallback of 500 BDT and adminId fallback of "1" with startup-time config validation that fails loudly (GatewaysController.cs)
24.17 [DONE] Payment: Fix SSLCommerz form body parser — split on '=' truncates base64 values; use QueryHelpers.ParseQuery instead (SSLCommerzGateway.cs ProcessWebhookAsync)
24.18 [DONE] Security: Fix path traversal in SecureFilesController — after Path.GetFullPath, assert path starts with the secure-uploads root; reject any path containing ".." (SecureFilesController.cs)
24.19 [DONE] Security: Replace new Random() with RandomNumberGenerator.GetInt32(100_000, 1_000_000) for cryptographically secure OTP generation (OtpService.cs)
24.20 [DONE] Security: Store OTP as HMAC-SHA256(code, email), not cleartext; a DB compromise reveals all active codes (OtpService.cs)
24.21 [DONE] Security: Add per-email attempt counter — lock OTP after 5 wrong attempts; add Purpose field (EmailVerify, PasswordReset) to prevent cross-use (OtpService.cs)
24.22 [DONE] Security: Invalidate all previous unverified OTPs for the same email before inserting a new one (OtpService.cs)
24.23 [DONE] Security: Add brute-force lockout to LoginAsync — track FailedLoginAttempts + LockoutUntil on User, lock 15 min after 5 failures (AuthService.cs)
24.24 [DONE] Security: Fix username enumeration timing attack — always run a dummy BCrypt.Verify when user is not found to normalize response time (AuthService.cs)
24.25 [DONE] Security: Fix social auto-link — only link IdP email to local account when member.EmailVerified == true (AuthService.cs SocialLoginAsync)
24.26 [DONE] Security: Social signup uses unique SOCIAL-{PROVIDER}-{socialId} sentinel instead of "TBD" for NID/MobileNo — prevents UNIQUE index crash on second social signup (AuthService.cs SocialLoginAsync)
24.27 [DONE] Security: JWT lifetime reduced to 60 min; opaque refresh token (SHA-256 hash stored in RefreshTokens table, 7-day cookie rotation) implemented in TokenService.cs + RefreshToken entity + /auth/refresh endpoint
24.28 [DONE] Security: Rotate SecurityStamp on password reset and password change to invalidate all previous JWTs (AuthService.cs, UserService.cs)
24.29 [DONE] Bug: Fix membership number lexicographic race — OrderByDescending(m.Id) instead of string sort; prevents regression at 999→1000 rollover (MemberService.cs RegisterAsync + ApproveMemberAsync)
24.30 [DONE] Bug: Replace hard-delete of rejected applicants with soft-delete (Status=Rejected, IsArchived=true) preserving audit trail; added Rejected to MembershipStatus enum (MemberService.cs, Enums.cs)
24.31 [DONE] Bug: CreateUserAccountAsync moved inside the serializable transaction in ApproveMemberAsync — member+user are now atomic (MemberService.cs)
24.32 [DONE] Perf: BulkArchiveInactiveMembersAsync replaced N+1 ArchiveMemberAsync loop with bulk ExecuteUpdateAsync calls for all cascades (MemberService.cs)
24.33 [DONE] Perf: Add index on User.MemberId (hot lookup path in Auth, Member services)
24.34 [DONE] Perf: Add partial index on User.ResetToken WHERE ResetToken IS NOT NULL
24.35 [DONE] Perf: Add index on User.GoogleId, User.FacebookId (social login hot path)
24.36 [DONE] Perf: Add composite index on Otp(Email, ExpiryAt) — OTP verification scans table without it
24.37 [DONE] Perf: Add composite index on Member(Status, IsArchived) — heavily filtered in admin listing queries (MemberConfiguration.cs)
24.38 [DONE] Perf: Add entity configuration file for Otp table (currently using EF defaults — no indexes, no column length caps on a security-critical table)
24.39 [DONE] Security: Move JWT from localStorage to httpOnly + Secure + SameSite=Strict cookie set by backend; token no longer stored in localStorage; Angular uses /auth/me on init; sessionStorage holds display fields only (auth.service.ts, ServiceExtensions.cs, AuthController.cs)
24.40 [DONE] Security: Wrap getUserFromStorage() JSON.parse in try/catch; fall back to null and clear corrupt entry (auth.service.ts)
24.41 [DONE] Security: Check JWT exp claim on storage restore — drop session if token is already expired (auth.service.ts)
24.42 [DONE] Security: Sanitize article HTML server-side with Ganss.HtmlSanitizer before storage in NewsService.CreateNewsAsync + UpdateNewsAsync; [innerHTML] now safe (NewsService.cs, magazine.html)
24.43 [DONE] Security: Restrict Authorization header injection in globalHttpInterceptor to /api/* URLs only, not all outgoing HttpClient requests (global-http.interceptor.ts)
24.44 [DONE] Security: Token refresh implemented — 401 queuing with BehaviorSubject, POST /auth/refresh retried, all concurrent 401s share single refresh call, logout only on refresh failure (global-http.interceptor.ts, AuthController.cs)
24.45 [DONE] Security: Client-side file validation added — images ≤ 5 MB (JPEG/PNG/WebP), PDFs ≤ 10 MB; shared validateUploadFile() utility applied to register.ts, gallery.ts, admin-members.ts, admin-events.ts, articles.ts (file-validation.util.ts)
24.46 [DONE] Security: Enforce mustChangePassword redirect — after login, route to change-password screen before allowing any navigation (auth.service.ts, auth.guard.ts)
24.47 [DONE] Security: Restrict QueryStringTokenMiddleware to GET-only on /api/secure-files/ and /api/uploads/ paths (QueryStringTokenMiddleware.cs)
24.48 [DONE] Security: PartitionedRateLimiter keyed on (IP, username) for login endpoint; LoginRateLimitMiddleware peeks body username before rate limiter runs (Program.cs, LoginRateLimitMiddleware.cs)
24.49 [DONE] Security: Add startup validator that throws if AllowedOrigins is empty in Production (Program.cs)
24.50 [DONE] Security: Use int.TryParse for MemberId claim parsing; return Unauthorized on missing/invalid claim (PollController.cs, FamilyLinkController.cs)
24.51 [DONE] Security: Read admin identity from User.FindFirst("MemberId") JWT claim in ApproveMember and RejectMember (AdminController.cs)
24.52 [DONE] Validation: Add MaximumLength and safe-character regex to FatherName, MotherName, PresentAddress, PermanentAddress, EmergencyContactName, EmergencyContactRelation, EmergencyContactPhone in MemberRegistrationValidator (MemberRegistrationValidator.cs)
24.53 [DONE] Validation: Add EmergencyContactPhone format validation (same regex as MobileNo) (MemberRegistrationValidator.cs)
24.54 [DONE] Validation: Add minimum age gate (LessThan(UtcNow.AddYears(-13))) and sanity lower bound (GreaterThan(UtcNow.AddYears(-120))) to DateOfBirth (MemberRegistrationValidator.cs)
24.55 [DONE] Validation: Add MaximumLength(254) to Email in VerifyEmailValidator; add caps to AcademicHistory.Subject and Result fields (MemberRegistrationValidator.cs, VerifyEmailValidator.cs)
24.56 [DONE] Validation: Enforce AdmissionYear < PassingYear in academic record child rules (MemberRegistrationValidator.cs)
24.57 [DONE] Security: Remove 'unsafe-inline' from script-src in CSP header; use nonces or hashes (SecurityHeadersMiddleware.cs)
24.58 [DONE] Security: Register SecurityHeadersMiddleware before UseStaticFiles so static file responses also receive security headers (Program.cs)
24.59 [DONE] Security: Remove deprecated X-XSS-Protection header (SecurityHeadersMiddleware.cs)
24.60 [DONE] Security: Add window.open(..., '_blank', 'noopener,noreferrer') to all gallery/event external link openings to prevent tab-napping (gallery.ts, events.ts)
```

</details>

### AREA 25: DGePAY PAYMENT GATEWAY INTEGRATION

7 items

| # | Status | Component | Summary |
|---|---|---|---|
| 25.1 | `DONE` | API | API: Implement DGePayGateway service (AES-128-ECB + HMAC-SHA256, Database-driven) |
| 25.2 | `DONE` | API | API: Register DGePayGateway in DependencyInjection.cs and PaymentGatewayFactory |
| 25.3 | `DONE` | API | Define callback endpoint in GatewaysController |
| 25.4 | `DONE` | API | Seed DGePay UAT credentials in payment_configurations.json |
| 25.5 | `DONE` | API | Formalize Gateway Workflow documentation (docs/PAYMENT_GATEWAY_WORKFLOW.md) |
| 25.6 | `DONE` | API | Update Mobile UI (Flutter) — gateway enum synced, selection bottom-sheet, PaymentWebPage integration |
| 25.7 | `DONE` | Tests | E2E Payment Flow Verification |

### AREA 26: VISUAL REGRESSION LAYOUT HARDENING

13 items

| # | Status | Component | Summary |
|---|---|---|---|
| 26.1 | `DONE` | Mobile | Mobile: Fix RenderFlex overflows — AppScaffold title/breadcrumb (maxLines + ellipsis) |
| 26.2 | `DONE` | Mobile | Mobile: Fix RenderFlex overflows — DirectoryScreen member designation badges (Flexible + ellipsis) |
| 26.3 | `DONE` | Mobile | Mobile: Fix RenderFlex overflow — DigitalIDScreen header title (Expanded + FittedBox) |
| 26.4 | `DONE` | Mobile | Mobile: Fix RenderFlex overflow — NewsScreen category chip row (Flexible + ellipsis) |
| 26.5 | `DONE` | Mobile | Mobile: Fix RenderFlex overflow — ArticlesScreen article status badge row (Flexible + ellipsis) |
| 26.6 | `DONE` | Mobile | Mobile: Fix RenderFlex overflow — JobDetailsScreen stat rows (Expanded + end-aligned ellipsis) |
| 26.7 | `DONE` | Mobile | Mobile: Fix RenderFlex overflow — EventsScreen date/fee row (Expanded + Flexible) |
| 26.8 | `DONE` | Mobile | Mobile: Fix overflow — DashboardScreen admin analytics stat card (FittedBox + label ellipsis) |
| 26.9 | `DONE` | Tests | Test: Fix MockHttpClient missing @override annotations in full_app_visual_freeze_test.dart |
| 26.10 | `DONE` | Mobile | Mobile: Generate final golden baselines after Riverpod teardown fix — 15/15 goldens synced |
| 26.11 | `DONE` | Tests | Test: Fix Riverpod NotInitializedError in tearDownAll — Refactored to isolated test lifecycles |
| 26.12 | `DONE` | Tests | Test: Fix registration_visual_test.dart — Added setStep() helper to RegisterWizardNotifier for direct rendering |
| 26.13 | `DONE` | Mobile | Mobile: Fix RenderFlex overflow in login_screen.dart (Flexible + FittedBox) |

### AREA 27: TEST COVERAGE IMPROVEMENT

4 items · completed 2026-08-11

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 27.2 | `DONE` | Tests | Add test project references for API, Application, Domain, Infrastructure | 2026-08-11 |
| 27.4 | `DONE` | Tests | Write unit tests for Handlers/Services (Moq) | 2026-08-11 |
| 27.5 | `DONE` | Tests | Write unit tests for Domain Validators (FluentValidation) | 2026-08-11 |
| 27.9 | `DONE` | Tests | Update README with test & coverage instructions | 2026-08-11 |

<details>
<summary>AREA 27 — original entries and completion notes (4 items)</summary>

```text
27.2  [TODO] Add test project references for API, Application, Domain, Infrastructure
     [2026-08-11 audit] Verified already shipped by the 2026-08-11 audit. `GHCAA.Tests/GHCAA.Tests.csproj` already carries ProjectReferences to Application, Infrastructure, Domain and API.
27.4  [TODO] Write unit tests for Handlers/Services (Moq)
     [2026-08-11 audit] Verified already shipped by the 2026-08-11 audit. 26 service test files under `GHCAA.Tests/Services/` using Moq 4.20.72.
27.5  [TODO] Write unit tests for Domain Validators (FluentValidation)
     [2026-08-11 audit] Verified already shipped by the 2026-08-11 audit. Application has exactly two FluentValidation validators and both are tested (`MemberRegistrationValidatorTests`, `VerifyEmailValidatorTests`).
27.9  [TODO] Update README with test & coverage instructions
     [2026-08-11 audit] Verified already shipped by the 2026-08-11 audit. Root `README.md` "Testing & Quality" documents all three suites and the local `--collect:"XPlat Code Coverage"` route.
```

</details>

### AREA 28: CONFIGURATION-DRIVEN FRAMEWORK

22 items · completed 2026-08-11

> Reference doc: docs/CONFIG_DRIVEN_FRAMEWORK.md
> DRY/SOLID review completed by Claude Opus on 2026-05-30.
> All blocking bugs from review are fixed. Phase 1 (backend) is merge-ready pending 28.0.

#### PRIORITY 0 - BLOCKING (must run before anything else in this area)

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 28.0 | `DONE` | DB | DB: Apply pending Area-24 migrations, then generate & apply AddOrganizationConfig | 2026-08-11 |

#### PRIORITY 1 - PHASE 1 BACKEND (DONE - verify before merge)

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 28.1 | `DONE` | API | Domain:  OrganizationConfig entity (GHCAA.Domain/Models/OrganizationConfig.cs) | — |
| 28.2 | `DONE` | API | App:     OrgConfigDto + nested records (GHCAA.Application/DTOs/OrgConfigDto.cs) | — |
| 28.3 | `DONE` | API | App:     IOrgConfigService interface (GetConfigAsync + UpdateConfigAsync only) | — |
| 28.4 | `DONE` | API | Infra:   OrgConfigService - GetOrCreateAsync cache, OrgId-keyed upsert, enum-driven MembershipTypes list | — |
| 28.5 | `DONE` | API | Infra:   OrganizationConfigConfiguration (text column, unique OrgId index, cross-provider safe) | — |
| 28.6 | `DONE` | API | Infra:   DbSet<OrganizationConfig></organizationconfig> added to ApplicationDbContext | — |
| 28.7 | `DONE` | API | API:     OrgConfigController - GET public / PUT SuperAdminOnly (parity comment) | — |
| 28.8 | `DONE` | API | API:     Program.cs startup seed (fault-tolerant try/catch, idempotent) | — |
| 28.9 | `DONE` | API | Domain:  Guest added to MembershipType enum (value=6, additive - no migration needed for enum) | — |
| 28.10 | `DONE` | Web | Angular: Guest added to MEMBERSHIP_TYPES + MEMBERSHIP_TYPE_OPTIONS (app.constants.ts) | — |

#### PRIORITY 2 - PHASE 2: ANGULAR CONSUMER

> DEPENDS ON: 28.0 (migration applied, GET /api/config returning 200)

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 28.11 | `DONE` | Web | Angular: Create OrgConfig TypeScript model (GHCAA.Web/src/app/core/models/org-config.model.ts) | 2026-08-11 |
| 28.12 | `DONE` | Web | Angular: Create OrgConfigService (GHCAA.Web/src/app/core/services/org-config.service.ts) | 2026-08-11 |
| 28.13 | `DONE` | Web | Angular: Wire APP_INITIALIZER in app.config.ts to call OrgConfigService.load() before render | 2026-08-11 |
| 28.15 | `DONE` | Web | Angular: Replace APP_CONFIG.* references in components with orgConfigService.config()?.branding.* | 2026-08-11 |
| 28.16 | `DONE` | Web | Angular: Create feature guard (GHCAA.Web/src/app/core/guards/feature.guard.ts) | 2026-08-11 |

#### PRIORITY 3 - PHASE 3: FLUTTER CONSUMER

> DEPENDS ON: 28.11-28.16 (Angular consumer pattern validated first)

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 28.17 | `DONE` | Mobile | Mobile: Create OrgConfig Dart model (GHCAA.Mobile/lib/core/models/org_config.dart) | 2026-08-11 |
| 28.18 | `DONE` | Mobile | Mobile: Create OrgConfigService Dart singleton (GHCAA.Mobile/lib/core/services/org_config_service.dart) | 2026-08-11 |

#### PRIORITY 5 - ADMIN UI (PHASE 4, OPTIONAL)

> DEPENDS ON: 28.12 (Angular OrgConfigService)

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 28.27 | `DONE` | Web | Angular: Create OrgConfig admin editor component | 2026-08-11 |

#### PRIORITY 6 - TECH DEBT CLEANUP (after all phases pass regression)

> DEPENDS ON: All of 28.1-28.28 green; do NOT delete Constants.cs fields before this

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 28.29 | `DONE` | Process | Cleanup: Add DEPRECATED comment to Constants.Branding.* and Constants.EmailSubjects.* | 2026-08-11 |
| 28.30 | `DONE` | Process | Cleanup: Migrate call sites - CommunicationService email subjects + any service using Constants.Branding.* | 2026-08-11 |
| 28.31 | `DONE` | Process | Cleanup: Add RowVersion/xmin concurrency token to OrganizationConfig entity | 2026-08-11 |

<details>
<summary>AREA 28 — original entries and completion notes (22 items)</summary>

```text
28.0  [TODO] DB: Apply pending Area-24 migrations, then generate & apply AddOrganizationConfig
              Step 1: dotnet ef database update --project GHCAA.Infrastructure --startup-project GHCAA.API
              Step 2: dotnet ef migrations add AddOrganizationConfig --project GHCAA.Infrastructure --startup-project GHCAA.API --output-dir Data/Migrations/PgSql
              Step 3: dotnet ef database update --project GHCAA.Infrastructure --startup-project GHCAA.API
              DEPENDS ON: PhaseB_S5S8 + AddRefreshTokens migrations from Area 24 applied first
     [2026-08-11 audit] Verified already shipped by the 2026-08-11 audit. `20260530092800_AddOrganizationConfig` exists in the PgSql migration tree and 31.3 already recorded that no migrations are pending. This was the PRIORITY 0 gate for the whole area, so Priorities 2-6 have been unblocked since then.
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
> 28.11 [TODO] Angular: Create OrgConfig TypeScript model (GHCAA.Web/src/app/core/models/org-config.model.ts)
> Shape: OrgBranding, FeatureToggles, LocalePack, NavLabels, EmailSubjects interfaces
     [2026-08-11 audit] Verified already shipped by the 2026-08-11 audit. `GHCAA.Web/src/app/core/models/org-config.model.ts` defines OrgBranding, FeatureToggles, LocalePack, NavLabels and the rest.
> 28.12 [TODO] Angular: Create OrgConfigService (GHCAA.Web/src/app/core/services/org-config.service.ts)
> - Signal<OrgConfig|null> config; load(): Promise<void></void> via GET /api/config
> - t(path, locale?) for locale string lookup
> - isEnabled(feature: keyof FeatureToggles) boolean
> - Falls back to GHCAA_DEFAULT_CONFIG on API failure
> DEPENDS ON: 28.11
     [2026-08-11 audit] Verified already shipped by the 2026-08-11 audit. `core/services/org-config.service.ts` fetches `/api/config` and falls back to an inline GHCAA default. Method is `loadConfig()`, not `load()`.
> 28.13 [TODO] Angular: Wire APP_INITIALIZER in app.config.ts to call OrgConfigService.load() before render
> DEPENDS ON: 28.12
     [2026-08-11 audit] Verified already shipped by the 2026-08-11 audit. `app.config.ts` registers an `APP_INITIALIZER` calling `orgConfigService.loadConfig()`.
> 28.15 [TODO] Angular: Replace APP_CONFIG.* references in components with orgConfigService.config()?.branding.*
> Grep target: grep -r "APP_CONFIG\." src/app --include="*.ts" --include="*.html" -l
> DEPENDS ON: 28.12, 28.13
     [2026-08-11 audit] Verified already shipped by the 2026-08-11 audit. Zero `APP_CONFIG` references remain under `GHCAA.Web/src`; nine components plus `nav.service.ts`, `feature.guard.ts` and `app.config.ts` consume OrgConfigService.
> 28.16 [TODO] Angular: Create feature guard (GHCAA.Web/src/app/core/guards/feature.guard.ts)
> Apply to /portal/forum, /portal/jobs, /portal/polls routes in app.routes.ts
> DEPENDS ON: 28.12
     [2026-08-11 audit] Verified already shipped by the 2026-08-11 audit. `core/guards/feature.guard.ts` exists and gates the gallery, events, job-hub and forum routes in `app.routes.ts`.
> 28.17 [TODO] Mobile: Create OrgConfig Dart model (GHCAA.Mobile/lib/core/models/org_config.dart)
> fromJson factory + ghcaaDefaults static getter for offline fallback
     [2026-08-11 audit] Verified already shipped by the 2026-08-11 audit. Shipped at `lib/core/config/org_config.dart` (not `core/models/`) with both the `fromJson` factory and the `ghcaaDefaults` offline fallback the task asked for.
> 28.18 [TODO] Mobile: Create OrgConfigService Dart singleton (GHCAA.Mobile/lib/core/services/org_config_service.dart)
> - load(ApiClient): fetches /api/config, caches to SharedPreferences for offline resilience
> - pack getter returns locale-appropriate LocalePack
> DEPENDS ON: 28.17
     [2026-08-11 audit] Verified already shipped by the 2026-08-11 audit. `lib/core/services/org_config_service.dart` has `load()` hitting `/config` on the `/api` base, SharedPreferences caching under `org_config_cache`, and a `localePackProvider` for the locale-appropriate pack.
> 28.27 [TODO] Angular: Create OrgConfig admin editor component
> File: GHCAA.Web/src/app/pages/admin/org-config/org-config.component.ts
> Tabs: Branding | Contact | Features | Localization-EN | Localization-BN | Workflow
> Route: /admin/org-config guarded by superAdminGuard
> DEPENDS ON: 28.12
     [2026-08-11 audit] Verified already shipped by the 2026-08-11 audit. `GHCAA.Web/src/app/admin/org-config/` ships the editor with Branding / Contact / Currency / Features / Workflow / Advanced tabs, routed at `/admin/org-config`.
> 28.29 [TODO] Cleanup: Add DEPRECATED comment to Constants.Branding.* and Constants.EmailSubjects.*
> Mark: // DEPRECATED: use IOrgConfigService; pending deletion after 28.30 complete
     [2026-08-11 audit] Verified already shipped by the 2026-08-11 audit. Superseded by a better outcome: `Constants.Branding` and `Constants.EmailSubjects` were deleted outright rather than deprecated. Zero code references remain.
> 28.30 [TODO] Cleanup: Migrate call sites - CommunicationService email subjects + any service using Constants.Branding.*
> Grep: Constants.Branding | Constants.EmailSubjects | Constants.Defaults.SupportEmail
> DEPENDS ON: 28.29
     [2026-08-11 audit] Verified already shipped by the 2026-08-11 audit. Call sites migrated — `CommunicationService.GetEmailFooterAsync()` and `MemberService` read `config.Branding.*` / `locale.EmailSubjects.*`.
> 28.31 [TODO] Cleanup: Add RowVersion/xmin concurrency token to OrganizationConfig entity
> Prevents last-write-wins on concurrent SuperAdmin edits
     [2026-08-11 audit] Verified already shipped by the 2026-08-11 audit. `OrganizationConfig.RowVersion` exists with migration `20260703123040_AddOrganizationConfigRowVersion`; `OrganizationConfigConfiguration` marks it `IsConcurrencyToken()` (app-managed, not xmin — deliberate, for cross-provider support).
```

</details>

### AREA 29: FULL-STACK REVIEW FINDINGS (2026-07-24)

36 items · completed 2026-07-25

> Source: whole-project review (backend correctness + security, Angular web, Flutter mobile, payment audit).
> Cross-referenced against docs/BUSINESS_FINDINGS.md and .antigravity/skills standards.
> Type-check + flutter analyze both pass clean — all items below are runtime/logic/UX, not compile errors.
> Fix order: 29-A blockers first, then 29-F.1 (audit-trail) + 29-B.2 (amount bypass), then 29-F sweep, then 29-G, then 29-C.

#### 29-A: CRITICAL — SHIP-BLOCKERS

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 29A.1 | `DONE` | Web | Web: Added /portal/change-password route + ChangePassword component; authGuard now URL-bypasses that page to avoid a redirect loop, and auth.service.clearMustChangePassword() clears the… | 2026-07-25 |
| 29A.2 | `DONE` | Mobile | Mobile: session_manager._handleSessionExpiry now clears the REAL secure-storage token via storageService.clearAll() (was only removing a stale SharedPreferences 'jwt_token')… | 2026-07-25 |
| 29A.3 | `DONE` | Mobile | Mobile: digital_id_screen now reads data['membershipNumber'] (was data['membershipId']) and data['passingYear'] (was data['batch']) across QR/barcode/share/PDF/filename — no more literal… | 2026-07-25 |
| 29A.4 | `DONE` | API | API: EventService capacity now counts slot-consuming registrations (Pending+Approved, not just Approved), rejects when full and HasWaitlist=false (was skipping the cap entirely), and closes… | 2026-07-25 |
| 29A.5 | `DONE` | API | API: CheckInParticipantAsync + CheckInByTicketCodeAsync now reject any non-Approved registration (Pending/Rejected/Waitlisted can no longer check in or earn points). (+regression test) | 2026-07-25 |

#### 29-B: HIGH — SECURITY

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 29B.1 | `DONE` | API | API: FacebookLoginAsync now verifies the token via graph.facebook.com/debug_token using an app access token (`{ClientId}\|{ClientSecret}`) and rejects unless data.is_valid && data.app_id ==… | 2026-07-25 |
| 29B.2 | `DONE` | API | API: Amount-verification bypass fixed. HandleSuccessfulPayment now takes `decimal? confirmedAmount`; check is `if (confirmedAmount.HasValue && Math.Abs(payment.Amount… | 2026-07-25 |
| 29B.3 | `DONE` | API | API: Membership auto-approval now scoped to membership fees only — gate is `if (payment.MemberId > 0 && payment.FinancialCategory == Enums.FinancialCategory.MembershipFee)`.… | 2026-07-25 |
| 29B.4 | `DONE` | API | API: Removed QueryStringTokenMiddleware (deleted file + registration in Program.cs). JWT is only accepted via the Authorization header now; no web/mobile client appended… | 2026-07-25 |
| 29B.5 | `DONE` | API | API: Auth rate-limiter now keyed on source IP alone (was {ip}:{username}, which gave each username its own bucket so one IP could spray N×limit accounts). PermitLimit 10/min per IP bounds… | 2026-07-25 |
| 29B.6 | `DONE` | API | API: Signatures moved to the auth-gated secure_uploads tree — IsSecureType now includes FileUploadType.Signature (forgery risk; was served publicly with no auth).… | 2026-07-25 |
| 29B.7 | `DONE` | API | API: record-payment now content-validates dto.Receipt via IFileValidationService.ValidateFormFile(FileCategory.Document, 10MB) before RecordPaymentAsync (magic-byte check; blocks a renamed… | 2026-07-25 |
| 29B.8 | `DONE` | API | API: SecureFilesController now resolves against BOTH roots LocalFileStorageService writes to (publicRoot=wwwroot, secureRoot=BaseDirectory, driven by FileStorage:BasePhysicalPath) and… | 2026-07-25 |

#### 29-C: HIGH — BACKEND CORRECTNESS

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 29C.1 | `DONE` | API | API: Deleted the divergent ApproveMemberInternalAsync copy (lexicographic serial sort colliding at #1000, no transaction, skipped profile/status/payment checks). Auto-approval after payment… | 2026-07-25 |
| 29C.2 | `DONE` | API | API: GetMemberDocumentsAsync now returns null for an unknown id instead of dereferencing a null member (NRE → 500); AdminController already maps null → 404. (MemberService.cs) | 2026-07-25 |

#### 29-D: HIGH — WEB (Angular)

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 29D.1 | `DONE` | Web | Web: Article rejection fixed — bound rejectReason to a plain field/model instead of a signal via [(ngModel)]; reject flow no longer throws "is not a function". (article-approval) | 2026-07-25 |
| 29D.2 | `DONE` | Web | Web: Member-approval panel now fetches full member detail (academic/professional/NID/addresses) before decision instead of the 8-field summary. (member-approval.ts) | 2026-07-25 |
| 29D.3 | `DONE` | Web | Web: Chat token persisted (no longer in-memory only); sends survive reload and error handlers added on loadRecentChats/loadHistory. (chat.service.ts) | 2026-07-25 |
| 29D.4 | `DONE` | Web | Web: Payments page no longer spins forever on error — flattened nested subscribes and added error callbacks (incl. deleteSavedMethod). (payments.ts) | 2026-07-25 |
| 29D.5 | `DONE` | Web | Web: Digital-ID download wired to the real getIDCard() endpoint, replacing the fake setTimeout stub. (digital-id.ts) | 2026-07-25 |
| 29D.6 | `DONE` | Web | Web: Gallery no longer emits an empty <img [src]=""> request — src guarded until a real URL exists. (gallery.html) | 2026-07-25 |
| 29D.7 | `DONE` | Web | Web: Router-event subscriptions in all 3 layouts now torn down via takeUntilDestroyed()/unsubscribe on destroy — no leak. | 2026-07-25 |
| 29D.8 | `DONE` | Web | Web: Login honors token expiry + returnUrl (createUrlTree(['/login'],{queryParams})); profile update no longer drops fields; governance page has an error handler. | 2026-07-25 |

#### 29-E: HIGH — MOBILE (Flutter)

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 29E.1 | `DONE` | Mobile | Mobile: Removed the dead social-login stubs (Google/Facebook buttons only showed a "Connecting…" snackbar; no SDK, no keys). Section hidden until real SDK wiring; auth_service… | 2026-07-25 |
| 29E.2 | `DONE` | Mobile | Mobile: List-fetch services now debugPrint+rethrow instead of catch→return [], so network failures surface as AsyncValue.error (shared AsyncValueWidget renders an error+retry state) instead… | 2026-07-25 |
| 29E.3 | `DONE` | Mobile | Mobile: NotificationHub leak fixed — provider now registers ref.onDispose(() => service.dispose()) (previously dead code; SignalR socket + 4 broadcast StreamControllers leaked and survived… | 2026-07-25 |
| 29E.4 | `DONE` | Mobile | Mobile: AppSearchField converted to a StatefulWidget with an internal 350ms debounce Timer (cancelled on dispose; clear bypasses debounce), so search consumers no longer re-fetch per… | 2026-07-25 |
| 29E.5 | `DONE` | Mobile | Mobile: Replaced bool.fromEnvironment('dart.library.js_util') (always false → Firebase init ran on web and crashed) with kIsWeb (imported from foundation). (main.dart) | 2026-07-25 |

#### 29-F: CROSS-CUTTING THEMES (repeat offenders)

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 29F.1 | `DONE` | Cross-platform | Web/Mobile: Admin-ID attribution — server already resolves the acting admin from the JWT MemberId claim (item 24.51, AdminController), ignoring any client-supplied id, so this was… | 2026-07-25 |
| 29F.2 | `DONE` | Cross-platform | All: Silent-failure sweep — web: converted next-only .subscribe() to object form with error: handlers across admin-roles, payments, login (quiet-degrade optional social), chat.service… | 2026-07-25 |
| 29F.3 | `DONE` | Cross-platform | All: Date contract settled — ISO-8601 is canonical wire format; dd-MM-yyyy is display/input only. | — |
| 29F.4 | `DONE` | Web | Web: White-labeling wired — admin-layout now pulls logo/brand strings from OrgConfigService (branding.shortName/logoUrl) instead of hardcoded values, adds a Sign Out control (footer +… | 2026-07-25 |

#### 29-G: PAYMENT GAPS (all 5 methods work manually/config-driven; no keys required — these are gaps only)

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 29G.1 | `DONE` | Mobile | Mobile: financial_portal_screen now fetches admin-configured methods from GET /api/payment-config/active (new activePaymentConfigsProvider + FinancialService.getActivePaymentConfigs) and… | 2026-07-25 |
| 29G.2 | `DONE` | Mobile | Mobile: new FinancialService.recordPayment (multipart POST /api/financials/record-payment) + manual-payment form surfaces the config's… | 2026-07-25 |
| 29G.3 | `DONE` | Cross-platform | Web+Mobile: dead Stripe tile removed (mobile sheet is now config-driven, no hardcoded gateways). Admin payment-config create form gained a Payment Method Type dropdown (methodOptions incl.… | 2026-07-25 |
| 29G.4 | `DONE` | API | API: GatewaysController.GatewayWebhook now wraps _gatewayFactory.GetGateway in try/catch(NotSupportedException) → returns 404 {status:"unsupported_gateway"} instead of an unhandled 500 for… | 2026-07-25 |

<details>
<summary>AREA 29 — original entries and completion notes (36 items)</summary>

```text
29A.1 [DONE 2026-07-25] Web: Added /portal/change-password route + ChangePassword component; authGuard now URL-bypasses that page to avoid a redirect loop, and auth.service.clearMustChangePassword() clears the flag on success. (guard spec 7/7)
29A.2 [DONE 2026-07-25] Mobile: session_manager._handleSessionExpiry now clears the REAL secure-storage token via storageService.clearAll() (was only removing a stale SharedPreferences 'jwt_token'); authStateProvider's 2s poll then sees null and the router redirects to /login. (flutter analyze clean)
29A.3 [DONE 2026-07-25] Mobile: digital_id_screen now reads data['membershipNumber'] (was data['membershipId']) and data['passingYear'] (was data['batch']) across QR/barcode/share/PDF/filename — no more literal "PENDING".
29A.4 [DONE 2026-07-25] API: EventService capacity now counts slot-consuming registrations (Pending+Approved, not just Approved), rejects when full and HasWaitlist=false (was skipping the cap entirely), and closes the overfill race with a deterministic post-insert Id-ordinal re-check that demotes/rejects the overflow. (+regression test)
29A.5 [DONE 2026-07-25] API: CheckInParticipantAsync + CheckInByTicketCodeAsync now reject any non-Approved registration (Pending/Rejected/Waitlisted can no longer check in or earn points). (+regression test)
29B.1 [DONE 2026-07-25] API: FacebookLoginAsync now verifies the token via graph.facebook.com/debug_token using an app access token (`{ClientId}|{ClientSecret}`) and rejects unless data.is_valid && data.app_id == config.ClientId (also rejects when ClientSecret unconfigured). Blocks tokens minted for a different app → account takeover. (AuthService.cs)
29B.2 [DONE 2026-07-25] API: Amount-verification bypass fixed. HandleSuccessfulPayment now takes `decimal? confirmedAmount`; check is `if (confirmedAmount.HasValue && Math.Abs(payment.Amount - confirmedAmount.Value) > 0.01m)` → mark payment Failed + return. SSLCommerz/DGePay callbacks now pass `null` when the amount key is ABSENT (skip verification, VerifyCallbackAsync remains primary gate) vs the parsed value — incl. 0 — when PRESENT (verify). Regression test SSLCommerzCallback_MarksFailedAndDoesNotApprove_WhenReportedAmountMismatches. (GatewaysController.cs)
29B.3 [DONE 2026-07-25] API: Membership auto-approval now scoped to membership fees only — gate is `if (payment.MemberId > 0 && payment.FinancialCategory == Enums.FinancialCategory.MembershipFee)`. FinancialCategory is set at initiation (RegistrationFee for EVT-REG refs, else MembershipFee), so event payments no longer trigger member approval. (GatewaysController.cs)
29B.4 [DONE 2026-07-25] API: Removed QueryStringTokenMiddleware (deleted file + registration in Program.cs). JWT is only accepted via the Authorization header now; no web/mobile client appended ?token=/?access_token= to secure-file URLs, so the query-token path was pure attack surface (leaked into proxy/access logs and the Referer header).
29B.5 [DONE 2026-07-25] API: Auth rate-limiter now keyed on source IP alone (was {ip}:{username}, which gave each username its own bucket so one IP could spray N×limit accounts). PermitLimit 10/min per IP bounds total auth attempts regardless of account count. (Program.cs)
29B.6 [DONE 2026-07-25] API: Signatures moved to the auth-gated secure_uploads tree — IsSecureType now includes FileUploadType.Signature (forgery risk; was served publicly with no auth). (LocalFileStorageService.cs)
29B.7 [DONE 2026-07-25] API: record-payment now content-validates dto.Receipt via IFileValidationService.ValidateFormFile(FileCategory.Document, 10MB) before RecordPaymentAsync (magic-byte check; blocks a renamed executable/script stored under a .jpg/.pdf name in the secure tree). (FinancialsController.cs)
29B.8 [DONE 2026-07-25] API: SecureFilesController now resolves against BOTH roots LocalFileStorageService writes to (publicRoot=wwwroot, secureRoot=BaseDirectory, driven by FileStorage:BasePhysicalPath) and serves the file only if it lands inside one of them — secure files under secure_uploads/ are now readable (were rejected) while path-traversal is still blocked. (SecureFilesController.cs)
29C.1 [DONE 2026-07-25] API: Deleted the divergent ApproveMemberInternalAsync copy (lexicographic serial sort colliding at #1000, no transaction, skipped profile/status/payment checks). Auto-approval after payment now delegates to the canonical MemberService.ApproveMemberAsync (Serializable txn, Id-ordered serial, full validation, user-account creation in-txn), resolved lazily via IServiceProvider to avoid the MemberService→IFinancialService DI cycle; on failure it logs and leaves the member Applied for manual review. (FinancialService.cs)
29C.2 [DONE 2026-07-25] API: GetMemberDocumentsAsync now returns null for an unknown id instead of dereferencing a null member (NRE → 500); AdminController already maps null → 404. (MemberService.cs)
29D.1 [DONE 2026-07-25] Web: Article rejection fixed — bound rejectReason to a plain field/model instead of a signal via [(ngModel)]; reject flow no longer throws "is not a function". (article-approval)
29D.2 [DONE 2026-07-25] Web: Member-approval panel now fetches full member detail (academic/professional/NID/addresses) before decision instead of the 8-field summary. (member-approval.ts)
29D.3 [DONE 2026-07-25] Web: Chat token persisted (no longer in-memory only); sends survive reload and error handlers added on loadRecentChats/loadHistory. (chat.service.ts)
29D.4 [DONE 2026-07-25] Web: Payments page no longer spins forever on error — flattened nested subscribes and added error callbacks (incl. deleteSavedMethod). (payments.ts)
29D.5 [DONE 2026-07-25] Web: Digital-ID download wired to the real getIDCard() endpoint, replacing the fake setTimeout stub. (digital-id.ts)
29D.6 [DONE 2026-07-25] Web: Gallery no longer emits an empty <img [src]=""> request — src guarded until a real URL exists. (gallery.html)
29D.7 [DONE 2026-07-25] Web: Router-event subscriptions in all 3 layouts now torn down via takeUntilDestroyed()/unsubscribe on destroy — no leak.
29D.8 [DONE 2026-07-25] Web: Login honors token expiry + returnUrl (createUrlTree(['/login'],{queryParams})); profile update no longer drops fields; governance page has an error handler.
29E.1 [DONE 2026-07-25] Mobile: Removed the dead social-login stubs (Google/Facebook buttons only showed a "Connecting…" snackbar; no SDK, no keys). Section hidden until real SDK wiring; auth_service googleLogin/facebookLogin plumbing retained for future. (login_screen.dart)
29E.2 [DONE 2026-07-25] Mobile: List-fetch services now debugPrint+rethrow instead of catch→return [], so network failures surface as AsyncValue.error (shared AsyncValueWidget renders an error+retry state) instead of a misleading empty list. ~40 methods across 18 services; 2 imperative call-sites (governance_registry, family_link) wrapped in try/catch+SnackBar. Intentional swallowers kept: lookup/dropdown static-fallback + auth getSocialProviders. (flutter analyze clean)
29E.3 [DONE 2026-07-25] Mobile: NotificationHub leak fixed — provider now registers ref.onDispose(() => service.dispose()) (previously dead code; SignalR socket + 4 broadcast StreamControllers leaked and survived logout). AuthService.logout() invalidates notificationHubServiceProvider so teardown fires on logout. (notification_hub_service.dart, auth_service.dart)
29E.4 [DONE 2026-07-25] Mobile: AppSearchField converted to a StatefulWidget with an internal 350ms debounce Timer (cancelled on dispose; clear bypasses debounce), so search consumers no longer re-fetch per keystroke — one call after typing settles. (app_search_field.dart)
29E.5 [DONE 2026-07-25] Mobile: Replaced bool.fromEnvironment('dart.library.js_util') (always false → Firebase init ran on web and crashed) with kIsWeb (imported from foundation). (main.dart)
29F.1 [DONE 2026-07-25] Web/Mobile: Admin-ID attribution — server already resolves the acting admin from the JWT MemberId claim (item 24.51, AdminController), ignoring any client-supplied id, so this was client-side dead/misleading code, not live audit corruption. Removed the hardcoded adminId=1 fallbacks and the now-unused approvedByAdminId/rejectedByAdminId fields across web (admin.service, member-approval, admin-members + specs), mobile (admin_service.dart), e2e helper, and DTOs (ApproveMemberDto/RejectMemberDto).
29F.2 [DONE 2026-07-25] All: Silent-failure sweep — web: converted next-only .subscribe() to object form with error: handlers across admin-roles, payments, login (quiet-degrade optional social), chat.service (loadRecentChats/loadHistory) + prior admin components. Mobile: try/catch-return-empty list fetches now debugPrint+rethrow so failures surface (see 29E.2). Errors no longer become blank screens / stuck spinners.
29F.3 [DONE] All: Date contract settled — ISO-8601 is canonical wire format; dd-MM-yyyy is display/input only.
              API unchanged (Write=ISO, Read accepts both). Web: added core/utils/date.util.ts (toDisplayDate/toWireDate/parseDisplayDate); routed 9 write-path components + register.ts through toWire; type-check clean.
              Mobile: AppUtils hardened ISO-first + added toWire(); fixed 6 send sites incl. RegisterModel.toJson DOB; flutter analyze clean.
              Skill doc ghcaa-date-standard/SKILL.md rewritten to the two-format contract. (2026-07-25)
29F.4 [DONE 2026-07-25] Web: White-labeling wired — admin-layout now pulls logo/brand strings from OrgConfigService (branding.shortName/logoUrl) instead of hardcoded values, adds a Sign Out control (footer + header) and a mobile off-canvas sidebar toggle (hamburger + backdrop, translateX). Feature-gated nav links honor OrgConfigService.isFeatureEnabled. (admin-layout .ts/.html/.scss)
29G.1 [DONE 2026-07-25] Mobile: financial_portal_screen now fetches admin-configured methods from GET /api/payment-config/active (new activePaymentConfigsProvider + FinancialService.getActivePaymentConfigs) and renders them in the payment sheet, replacing the hardcoded DGePay+Stripe tiles. Manual channels open a submission form; online channels (isOnline) route to the gateway via _gatewayFromString. (flutter analyze clean)
29G.2 [DONE 2026-07-25] Mobile: new FinancialService.recordPayment (multipart POST /api/financials/record-payment) + manual-payment form surfaces the config's walletNumber/accountNumber/accountHolder/bank/branch/routing as display-only "where to pay" details and captures the member's transaction reference + receipt. (walletNumber/bankName/accountNumber are display-only by design — the record-payment DTO carries transactionId/method/receipt, mirroring web member/payments.)
29G.3 [DONE 2026-07-25] Web+Mobile: dead Stripe tile removed (mobile sheet is now config-driven, no hardcoded gateways). Admin payment-config create form gained a Payment Method Type dropdown (methodOptions incl. CashOnHand) so any method is creatable — was hardcoding method:'ManualReceipt'. (Note: the web gateway dropdown's SSLCommerz/BkashGateway/NagadGateway all have registered implementations — not dead — so left intact.) (web tests 9/9)
29G.4 [DONE 2026-07-25] API: GatewaysController.GatewayWebhook now wraps _gatewayFactory.GetGateway in try/catch(NotSupportedException) → returns 404 {status:"unsupported_gateway"} instead of an unhandled 500 for unregistered gateways.
```

</details>

### AREA 30: UI/UX REMEDIATION (2026-07-30)

36 items · completed 2026-07-30 → 2026-08-02

> Full plan with root-cause analysis and file:line targets: **docs/UI_UX_REMEDIATION_PLAN.md**
> Source: ~30-symptom UI/UX defect list (admin panel + member portal + public landing), traced to 8 shared-layer root causes.
> RULE: anything specified for one panel applies to BOTH admin and member portal.
> RULE: fix centrally (styles.scss tokens / shared classes / shared components) — never per-component.
> Baselines to protect: web `npm run type-check` clean, `npm run build` clean, vitest 58 files / 233 tests, `dotnet test` exit 0.
> Approved decisions: compute ProfileCompletionPercentage server-side and DROP the Global Rank tile (no fake `#---`);
> portal nav sections = Overview / Community / Directory / Career / My Account;
> replace emoji nav/action icons with a monochrome inline-SVG `app-icon` set using `currentColor`.

#### 30-0: PHASE 0 — CENTRAL/SHARED LAYER (do first; these unblock the rest)

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 30.1 | `DONE` | Web | Web: De-duplicate styles.scss — merged the two competing `.filter-bar` blocks and the two `.status-badge` blocks into one authoritative definition each, preserving the effective cascade… | 2026-07-30 |
| 30.2 | `DONE` | Web | Web: Central action-control taxonomy — added `.btn-danger`, canonical `.icon-btn` (+ `.delete`/`.archive`/`.approve`/`.contact` modifiers), and `.action-group` to styles.scss; added… | 2026-08-01 |
| 30.3 | `DONE` | Web | Web: Added `src/app/common/icon/icon.ts`+`.html` — standalone `Icon` component (selector `app-icon`, inputs `name`/`size`/`strokeWidth`), `@switch(name)` inline-SVG set… | 2026-08-01 |
| 30.4 | `DONE` | Web | Web: Density pass in styles.scss — `.page-header` margin-bottom 3.5rem→2rem + h2 font-size 2.25rem→1.875rem; `.filter-bar`/`.action-bar` padding 1.25rem 1.75rem→1rem 1.5rem + margin-bottom… | 2026-08-01 |
| 30.5 | `DONE` | Web | Web: Added `.empty-state.compact` (tighter padding/gap + smaller icon/heading/copy) to styles.scss. Application to specific profile/dashboard/admin empty sections tracked as follow-up when… | 2026-08-01 |
| 30.6 | `DONE` | Web | Web: `body.dark-theme --border-color` changed from near-black `#1a1a1a` to `rgba(255,255,255,0.18)` (brighter than `--hairline`'s 0.1) so form-control borders are visible strokes against… | 2026-08-01 |
| 30.7 | `DONE` | Web | Web: Modal `.close-btn` enlarged 36px→40px + bumped glyph font-size, in styles.scss (applies to every modal at once). | 2026-08-01 |
| 30.8 | `DONE` | Web | Web: Added `src/app/common/user-menu/user-menu.ts`+`.html`+`.scss` — standalone `UserMenu` component (selector `app-user-menu`, `OnPush`, required `roleLabel` input) rendering the… | 2026-08-01 |
| 30.9 | `DONE` | Web | Web: `appImgFallback` directive (`common/directives/img-fallback.directive.ts`) + `/assets/placeholders/*` fallback assets were already wired up by a prior pass; this pass closed the… | 2026-08-01 |
| 30.10 | `DONE` | Web | Web: LogoSpinner (`app-logo-spinner`) rollout — most of the app (22 templates) already used the shared component from a prior pass. This pass found and fixed the remaining ad-hoc/missing… | 2026-08-01 |
| 30.11 | `DONE` | Web | Web: Added `NavService.labelFor(url, scope)` (`core/services/nav.service.ts`) as the single source of truth for "what should the header/breadcrumb/browser-tab title say for this URL"… | 2026-08-01 |
| 30.12 | `DONE` | Web | Web: Added `src/app/common/rich-text-editor/rich-text-editor.ts`+`.html`+`.scss` — standalone `RichTextEditor` component (selector `app-rich-text-editor`, plain `[(value)]`/`height`… | 2026-08-01 |

#### 30-1: PHASE 1 — NAVIGATION & SHELL

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 30.13 | `DONE` | Web | Web: `NavService.ALL_NAV_ITEMS` now each carry a `section` (`Overview`/`Community`/`Directory`/`Career`/`My Account`); added `portalNavSections` computed mirroring `adminNavSections`'s… | 2026-08-01 |
| 30.14 | `DONE` | Web | Web: `.nav-spacer { flex: 1 1 auto; }` added to portal-layout.scss — it previously had zero CSS rule anywhere in the app despite `.nav-list` already being a flex column with `flex: 1`, so… | 2026-08-01 |
| 30.15 | `DONE` | Web | Web: Admin header (`admin-layout.html`'s `.header-right`) now renders `<app-user-menu>` alongside "Exit Admin", giving admin its first theme toggle and avatar/photo display (previously… | 2026-08-01 |

#### 30-2: PHASE 2 — PER-PAGE FIXES

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 30.16 | `DONE` | Web | Web: jobs-preview "Login to View →" wraps to two lines — replaced `.btn-view` with `.btn.btn-sm` + nowrap. | 2026-08-02 |
| 30.17 | `DONE` | Web | Web: Admin Special Themes — `admin-themes.html`/`.ts`/`.scss` now derive SCHEDULED / LIVE / EXPIRED / IDLE from the theme's start/end dates instead of a static LIVE badge; confirmed via… | 2026-08-02 |
| 30.18 | `DONE` | Web | Web: Events — "Reg. Ends: Closed" → "Registration Closed" in `common/events/events.html`, matching `events-preview.html`'s existing wording. | 2026-08-02 |
| 30.19 | `DONE` | Web | Web: Submission-review + gallery-album cover placeholders added via `appImgFallback`; gallery album delete-button visibility and Edit/Delete gap fixed in `admin-gallery.html`/`.scss` using… | 2026-08-02 |
| 30.20 | `DONE` | Web | Web: Communications (`admin-comm.html`/`.ts`/`.scss`) — action-button gap/delete styling fixed with `.action-group`/`.icon-btn`/`.btn-danger`; bespoke logs filter replaced with the shared… | 2026-08-02 |
| 30.21 | `DONE` | Web | Web: Executive Committee cards (`common/governance/governance.html`) now render the member photo via `<img appImgFallback>` (avatar-placeholder fallback), matching the pattern already used… | 2026-08-02 |
| 30.22 | `DONE` | Web | Web: Member profile identity line — root cause was not a font-family mismatch (the global `Outfit` font already applies everywhere) but a perceptual clash from `.m-type` carrying… | 2026-08-02 |
| 30.23 | `DONE` | Web | Web: Member dashboard profile-completion steps converted to `<a routerLink>` elements linking to `/portal/profile#section-*` anchors (Identity & Photo / GHC History / Professional Info) and… | 2026-08-02 |
| 30.24 | `DONE` | Web | Web: Member Directory + Alumni Directory (shared `common/directory` component) — `.filters`/`.filter-grid`/`.filter-group` padding/margins compacted; `.filters` made `position: sticky; top… | 2026-08-02 |
| 30.25 | `DONE` | Web | Web: New Message flow (`member/messages/messages.ts`/`.html`/`.scss`) — added a "New Message" button + member-picker modal backed by `NetworkingService.searchMembers()`, and… | 2026-08-02 |
| 30.26 | `DONE` | Web | Web: Events first-load error — ROOT CAUSE found: a genuine race between `AuthService`'s async `/auth/me` session restore and `Events.loadEvents()`'s bare `setTimeout(..., 150)` deep-link… | 2026-08-02 |

#### 30-3: PHASE 3 — BACKEND

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 30.27 | `DONE` | API | API: `NetworkingService.MapToSummary` (`GHCAA.Infrastructure/Services/NetworkingService.cs`) now populates `PassingYear`/`Degree`/`Subject` on the EC summary DTO from the member's GHC… | 2026-08-02 |
| 30.28 | `DONE` | API | API: **Correction to this item's original premise** — `Rank`/`ProfileCompletionPercentage` were NOT "never computed anywhere"; both were already computed via a stricter 13-field… | 2026-08-02 |
| 30.29 | `DONE` | API | API: Confirmed already implemented — `POST /api/messaging/send` (aliased `/api/chat/send`) → `ChatService.SendMessageAsync` already supports starting a new conversation with no prior… | 2026-08-02 |
| 30.30 | `DONE` | API | API: Confirmed already correct — `ThemeService.GetActiveThemeAsync` already honours the Start/EndDate window server-side (Bangladesh local time). The LIVE-badge-ignoring-dates bug (30.17)… | 2026-08-02 |

#### 30-4: PHASE 4 — MOBILE PARITY & VERIFICATION

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 30.31 | `DONE` | Mobile | Mobile (`lib/core/theme/app_theme.dart`): added `dangerColor` (mirrors `--danger-color`/`.btn-danger`, wired into `ColorScheme.dark(error:)`), `borderColorBright` (distinct neutral token… | 2026-08-02 |
| 30.32 | `DONE` | Tests | Verify: `npm run type-check` clean; `npm run build` succeeds, emitted `dist/GHCAA.Web/browser/styles-*.css` confirmed to contain recently-added classes/tokens (`text-main`, `empty-state`)… | 2026-08-02 |
| 30.33 | `DONE` | Mobile | Mobile login screen (`lib/screens/auth/login_screen.dart`): identifier field label `'Member ID (Email / NID)'` → `'Email or Username'`; password field label `'Portal Password'` →… | 2026-08-01 |
| 30.34 | `DONE` | Mobile | Mobile: fixed floating-label/text overlap on ALL text fields app-wide, centrally in `lib/core/theme/app_theme.dart`'s `inputDecorationTheme` — added `floatingLabelBehavior… | 2026-08-01 |
| 30.35 | `DONE` | Mobile | Mobile: ran `flutter test` full suite after 30.33/30.34 — `widget_test.dart` (branding/button assertions) passes; `flutter analyze` clean on both edited files. Pre-existing golden… | 2026-08-01 |
| 30.36 | `DONE` | Process | REVIEW (raised by user 2026-08-01): redirect logic confirmed already correct (web `login.ts navigateAfterLogin()` routes Admin/SuperAdmin to `/admin/approvals`; mobile routes staff-admin… | 2026-08-02 |

<details>
<summary>AREA 30 — original entries and completion notes (36 items)</summary>

```text
30.1  [DONE 2026-07-30] Web: De-duplicate styles.scss — merged the two competing `.filter-bar` blocks and the two `.status-badge` blocks into one authoritative definition each, preserving the effective cascade exactly (`.search-icon` centring, full `.clear-search` rule, `.status-select` height/uppercase/`option` styling, `backdrop-filter` on `.filter-bar` only — never `.action-bar`). "Do NOT re-add a block here" comments left at both old sites. WHY IT GATED EVERYTHING: the later duplicate silently won every conflicting property, so density fixes applied to the earlier block did nothing. Verified type-check + build clean.
30.2  [DONE 2026-08-01] Web: Central action-control taxonomy — added `.btn-danger`, canonical `.icon-btn` (+ `.delete`/`.archive`/`.approve`/`.contact` modifiers), and `.action-group` to styles.scss; added `core/constants/actions.constants.ts` (`ACTION_LABELS`: Save/Edit/Delete/Close/Cancel/New) for new call sites to adopt incrementally. Removed the 5 per-component `.icon-btn` definitions (polls/news/members/governance/gallery scss) and fixed all 7 usage sites — `admin-comm.html` + `messages.html` (previously undefined, rendered browser-native) now styled; gallery delete + governance edit buttons given correct modifier/class instead of always-red or inline-style overrides. `.icon-btn` kept as the class name/alias so templates needed no rename. Verified type-check + build clean (emitted CSS contains `.action-group`/`.btn-danger`) + vitest 58/233 green.
30.3  [DONE 2026-08-01] Web: Added `src/app/common/icon/icon.ts`+`.html` — standalone `Icon` component (selector `app-icon`, inputs `name`/`size`/`strokeWidth`), `@switch(name)` inline-SVG set (`stroke="currentColor"`, theme-adaptive) covering ~55 icon names + `@default` fallback circle. Replaced every nav.service.ts emoji (portal + admin nav items) with semantic name keys; replaced hardcoded emoji in both layouts (back/menu/admin-panel/theme-toggle/sign-out) and toast.ts (`getIcon()` now returns `toast-success|error|warning|info`); replaced every Font Awesome `fa-`/`fas`/`far`/`fab` usage app-wide (register/login/reset-password/change-password eye-toggles, admin/roles, member/polls+dashboard, admin/events, admin/polls) with `<app-icon>`; removed the Font Awesome CDN `<link>` from index.html. Verified type-check + build clean (dist grep: zero `font-awesome` references) + vitest 58/233 green.
30.4  [DONE 2026-08-01] Web: Density pass in styles.scss — `.page-header` margin-bottom 3.5rem→2rem + h2 font-size 2.25rem→1.875rem; `.filter-bar`/`.action-bar` padding 1.25rem 1.75rem→1rem 1.5rem + margin-bottom 2rem→1.5rem + `.search-wrap input`/`.status-select` height 3.25rem→2.75rem; `.tab-nav`/`.tabs` margin-bottom 3.5rem→2rem + button padding 1rem 0→0.75rem 0. Added `--header-height: 4.5rem` token on `:root` (also set as `.top-bar`'s `min-height`) and made `.filter-bar`/`.action-bar` `position: sticky; top: var(--header-height); z-index: 90` inside `@media (min-width: 1024px)` — sticks just below the already-sticky `.top-bar` (z-index 100). Verified type-check + build clean (emitted CSS contains `--header-height`) + vitest 58/233 green.
30.5  [DONE 2026-08-01] Web: Added `.empty-state.compact` (tighter padding/gap + smaller icon/heading/copy) to styles.scss. Application to specific profile/dashboard/admin empty sections tracked as follow-up when touching those templates (30.19/30.21/30.22).
30.6  [DONE 2026-08-01] Web: `body.dark-theme --border-color` changed from near-black `#1a1a1a` to `rgba(255,255,255,0.18)` (brighter than `--hairline`'s 0.1) so form-control borders are visible strokes against black surfaces; added `& + &.checkbox-group { margin-top }` for spacing between stacked checkboxes.
30.7  [DONE 2026-08-01] Web: Modal `.close-btn` enlarged 36px→40px + bumped glyph font-size, in styles.scss (applies to every modal at once).
30.8  [DONE 2026-08-01] Web: Added `src/app/common/user-menu/user-menu.ts`+`.html`+`.scss` — standalone `UserMenu` component (selector `app-user-menu`, `OnPush`, required `roleLabel` input) rendering the theme-toggle button + avatar/photo/name/role block + sign-out button (same markup/classes as the old portal header: `.theme-toggle`, `.user-profile`, `.avatar`/`.avatar-img`, `.user-details`, `.username`, `.role`, `.logout-toggle`); fetches the photo itself via `ProfileService.getProfile()`. Since these rules previously lived scoped under portal-layout's `.right-section` in portal-layout.scss (component-scoped, not styles.scss) and Angular's emulated encapsulation does not let a parent's scoped CSS reach a child component's template, the CSS moved into the new component's own `user-menu.scss` rather than being duplicated. Wired into `portal-layout.html`'s `.right-section` (replacing the inline markup; removed now-dead `profilePhotoUrl`/`getImageUrl`/`ProfileService` from `portal-layout.ts`) and into `admin-layout.html`'s `.header-right` alongside the existing "Exit Admin" link (removed the old `.header-user` username/role-badge div, the standalone Sign Out button, and `AdminLayout.logout()` — all now redundant/dead; also dropped the matching now-unused `.header-user`/`.header-username`/`.header-role-badge`/`.logout-btn:hover` CSS from admin-layout.scss). Verified type-check + build clean + vitest 58/233 green.
30.9  [DONE 2026-08-01] Web: `appImgFallback` directive (`common/directives/img-fallback.directive.ts`) + `/assets/placeholders/*` fallback assets were already wired up by a prior pass; this pass closed the remaining gaps and verified full coverage. Fixed the one genuine ad-hoc handler left — `gallery-preview.html`'s `(error)="onImgError($event)"` + unstyled `.photo-placeholder` div (dead CSS, confirmed via `gallery-preview.scss`) — by replacing it with the directive and deleting the now-dead `onImgError()` from `gallery-preview.ts`. Added the directive (bare, using its built-in default) to every remaining static bundled-asset `<img>` that lacked it: both portal-layout logos, the login/register/reset-password auth-page logos, the about-page founder portrait, logo-spinner's own seal image, and events.html's invitation/print-preview logo. Final verification was a full-app multiline grep (`<img(\s[^>]*)?>` across every `src/app/**/*.html`, since Angular template attributes wrapping onto a continuation line produce false negatives under a naive single-line grep) confirming literally every `<img>` in the app now carries `appImgFallback`, either bare or with a contextual override (`avatar-placeholder.svg`, `event-placeholder.jpg`, `/assets/logo.png`, `image-placeholder.svg`). ROOT CAUSE of all "cover image missing / images not showing" reports is now closed. Verified type-check + build clean + vitest 59/236 green.
30.10 [DONE 2026-08-01] Web: LogoSpinner (`app-logo-spinner`) rollout — most of the app (22 templates) already used the shared component from a prior pass. This pass found and fixed the remaining ad-hoc/missing cases: `admin-audit.html`'s "Fetching activity stream..." block had only a static ⏳ glyph (no spinner) — given `<app-logo-spinner>`; `admin-gallery.html`, `admin-payment-config.html`, `admin-roles.html`, `common/governance.html` and `common/news.html` each showed a bare loading message/animate-pulse text with no spinner at all — all five given `<app-logo-spinner>` (added `LogoSpinnerComponent` to each `.ts` `imports` array). `admin-comm.html` had a `loading` signal driving both its Templates and Logs tabs but the template never rendered any loading feedback for either — added `@if (loading()) { <app-logo-spinner> } @else { <table> }` guards to both tabs (single shared `loading` signal already correctly gates whichever tab is active, confirmed via `setTab()`/`loadTemplates()`/`loadLogs()`). A follow-up sweep against the plan doc's own 0.10 candidate list found two more genuine gaps with no loading feedback at all: `admin/events/admin-events.ts` had no `loading` signal for its Management Hub grid nor its Participation Approvals table — added separate `loading`/`loadingRegistrations` signals (set in `loadAllEvents()`/`loadAllRegistrations()`) each gating their own `@if (...) { <app-logo-spinner> } @else { ... }` block; `admin/org-config/org-config.ts`'s async `loadConfig()` left the JSON textarea blank with zero feedback while it awaited the initial fetch — added an `isLoading` flag gating a spinner over the whole form. Two remaining plan-doc candidates were checked and confirmed *not* gaps: `public/directory` is a thin wrapper around `common/directory` which already has its own loading state (delegates, doesn't duplicate); `member/assistant`'s three-dot `typing()` chat bubble is a legitimate per-message typing indicator (same class of exception as inline submit-button text), not a missing page-level spinner. `member/dashboard/dashboard.html` and `admin/dashboard/admin-dashboard.html`'s skeleton-card/`sk-card` grid loaders were left as-is (both portals use the identical skeleton-placeholder style consistently, so this is a deliberate, symmetric design choice for dashboards specifically, not an ad-hoc one-off — not converted to the spinner). Login/register/reset-password/change-password/contact's `loading()` usages are inline submit-button text swaps ("Authenticating...", "Saving...", etc.), a different and correct pattern for in-place button feedback, left untouched. Verified type-check + build clean + vitest 59/236 green.
30.11 [DONE 2026-08-01] Web: Added `NavService.labelFor(url, scope)` (`core/services/nav.service.ts`) as the single source of truth for "what should the header/breadcrumb/browser-tab title say for this URL", replacing the near-identical inline `find()` logic previously duplicated in `admin-layout.ts` and `portal-layout.ts`. Both layouts' `currentPageTitle` (used for both the `Title` service and the breadcrumb) were already 100% derived from the nav item label by construction, so the real mismatch surface was exclusively each page's own on-page `<h1>/<h2>/app-page-header title=` text disagreeing with its nav label — reconciled by renaming each page's header text to match the nav label (rather than renaming nav labels, to minimize blast radius): admin `System Audit Logs`→`Audit Logs`, `Organization Configuration ✨`→`Org Config`; portal `Haraganga News Hub`→`News`, `Events & Gatherings`→`Events`, `GHC AI Assistant`→`Assistance`, `Executive Committee`→`Governance` (portal governance page, not to be confused with the admin EC roster page which correctly keeps "Executive Committee"), `Member Directory`→`Alumni Directory`, `Legacy Archive & Moments`→`Event Gallery`, `Opportunities Hub`→`Job Hub`, `Member Credentials`→`Digital ID`, `Fees & Dues`→`Payments`, `Articles & Submissions`→`My Articles`, `Member Dashboard`→`My Profile` (an outright copy-paste bug — profile.html was showing the dashboard's old title), `Community Forum`→`Discussions`. Also found and fixed a genuine gap, not just a mismatch: `member/dashboard/dashboard.html` had no on-page title at all (unlike `admin/dashboard`, which already showed "Dashboard") — added a matching `.page-header`/`.h2` block using the pre-existing global `.page-header` style from `styles.scss` (no new CSS needed). Exhaustively cross-checked every remaining `ALL_NAV_ITEMS`/`ADMIN_NAV_ITEMS` entry against its page's header text in one final pass; `member/messages` (chat-UI, no page-level title by design) and the two `/polls` pages (reachable but not present in either nav list, so no label to reconcile against) were confirmed as legitimately out of scope, not missed mismatches. Verified type-check + build clean + vitest 59/236 green.
30.12 [DONE 2026-08-01] Web: Added `src/app/common/rich-text-editor/rich-text-editor.ts`+`.html`+`.scss` — standalone `RichTextEditor` component (selector `app-rich-text-editor`, plain `[(value)]`/`height` two-way-bindable inputs, matching admin-comm's existing non-reactive-forms pattern instead of adding `ControlValueAccessor`) that owns its own container div via `ViewChild` + `ngAfterViewInit`, so Quill only ever initialises once Angular has actually rendered this component's own template — fixing the ROOT CAUSE of "Message Body (Rich Text) — no control found" (the old `document.getElementById` + `setTimeout` calls raced the `@if` that rendered the container). Renders a plain `<textarea>` fallback bound to the same value when `typeof window.Quill === 'undefined'`. Replaced both call sites in `admin-comm.html`/`admin-comm.ts` (`broadcast-editor` → `[(value)]="sendOptions.customBody"`, `template-editor` → `[(value)]="editingTemplate()!.body"`), removing the dead `initEditor`/`initBroadcastEditor` methods, the `setTimeout`-delayed init calls, and the raw `<div id="...">` containers. No existing spec covered the old Quill init (none existed for admin-comm). Verified type-check + build clean + vitest 59/236 green (no regressions vs. 58/233 baseline).
30.13 [DONE 2026-08-01] Web: `NavService.ALL_NAV_ITEMS` now each carry a `section` (`Overview`/`Community`/`Directory`/`Career`/`My Account`); added `portalNavSections` computed mirroring `adminNavSections`'s exact grouping/filter pattern on top of the existing feature-flag filtering in `portalNavItems()`. `portal-layout.html`'s `<nav class="nav-list">` now iterates `nav.portalNavSections()` → `.nav-section`/`.nav-section-label` (new rules added to portal-layout.scss, adapted to this sidebar's uppercase nav-item look — the admin `.nav-section`/`.nav-section-label` CSS lives component-scoped in admin-layout.scss, not styles.scss, so it could not be reused via selector broadening and was mirrored instead) before the `.nav-spacer` + Admin Panel link block, unchanged in placement. Verified type-check + build clean + vitest 58/233 green.
30.14 [DONE 2026-08-01] Web: `.nav-spacer { flex: 1 1 auto; }` added to portal-layout.scss — it previously had zero CSS rule anywhere in the app despite `.nav-list` already being a flex column with `flex: 1`, so the spacer itself never grew to push the "Admin Panel" link to the bottom of the sidebar. Verified type-check + build clean (rule present in the emitted portal-layout JS chunk, since component styles inline there rather than into the global CSS file) + vitest 58/233 green.
30.15 [DONE 2026-08-01] Web: Admin header (`admin-layout.html`'s `.header-right`) now renders `<app-user-menu>` alongside "Exit Admin", giving admin its first theme toggle and avatar/photo display (previously portal-only). See 30.8 for the shared-component details. Verified type-check + build clean + vitest 58/233 green.
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
30.27 [DONE 2026-08-02] API: `NetworkingService.MapToSummary` (`GHCAA.Infrastructure/Services/NetworkingService.cs`) now populates `PassingYear`/`Degree`/`Subject` on the EC summary DTO from the member's GHC academic record — root cause of "Executive Committee batch information is missing" confirmed and closed. Test added: `NetworkingServiceTests.GetExecutiveCommitteeAsync_ShouldPopulateBatchInformation`.
30.28 [DONE 2026-08-02] API: **Correction to this item's original premise** — `Rank`/`ProfileCompletionPercentage` were NOT "never computed anywhere"; both were already computed via a stricter 13-field `MemberService.CalculateProfileCompletion` that also gates `ApproveMemberAsync`/`member.IsProfileComplete` (deliberately left untouched to avoid changing admin-approval semantics). Added a NEW method `CalculateChecklistProfileCompletion` matching the dashboard's 4-item checklist (Identity&Photo/GHC History/Professional Info/Registration Payment, 25% each), now used for the profile page's `ProfileCompletionPercentage`. Global Rank tile removed from `member/profile/profile.html` (grid `md:grid-cols-5`→`md:grid-cols-4`). Test added: `MemberServiceTests.GetProfileAsync_ProfileCompletionPercentage_ShouldMatchDashboardChecklistCriteria`. See `session_area30_phase3_backend.md` memory for which method to use where in future work.
30.29 [DONE 2026-08-02] API: Confirmed already implemented — `POST /api/messaging/send` (aliased `/api/chat/send`) → `ChatService.SendMessageAsync` already supports starting a new conversation with no prior history. Added missing test coverage (`ChatServiceTests.cs`, file didn't previously exist).
30.30 [DONE 2026-08-02] API: Confirmed already correct — `ThemeService.GetActiveThemeAsync` already honours the Start/EndDate window server-side (Bangladesh local time). The LIVE-badge-ignoring-dates bug (30.17) was Angular-admin-page-only. Added `ThemeServiceTests.cs` (3 tests: expired/future/in-window themes).
30.31 [DONE 2026-08-02] Mobile (`lib/core/theme/app_theme.dart`): added `dangerColor` (mirrors `--danger-color`/`.btn-danger`, wired into `ColorScheme.dark(error:)`), `borderColorBright` (distinct neutral token mirroring dark-theme `--border-color`, kept separate from the gold-tinted `glassBorder`), and sizing tokens `iconButtonSize`/`closeButtonSize`/`headerHeight`/`actionGroupGap` mirroring `.icon-btn`/modal close/`--header-height`/`.action-group`. Added `emptyStateCompact*` tokens and an additive `compact` flag on `core/widgets/empty_state_widget.dart` (default false, backward-compatible) mirroring `.empty-state.compact`. New `lib/core/constants/action_labels.dart` mirrors web's `ACTION_LABELS`.
30.32 [DONE 2026-08-02] Verify: `npm run type-check` clean; `npm run build` succeeds, emitted `dist/GHCAA.Web/browser/styles-*.css` confirmed to contain recently-added classes/tokens (`text-main`, `empty-state`); `npx vitest run` 59 files / 236 tests passed (matches baseline); `dotnet test` 317/0 passed (matches baseline), no stray `GHCAA.API` process/file-lock encountered. `inlineCritical: false` confirmed still set in both angular.json configs. Playwright light+dark visual QA on the 10-page set was **skipped** (no browser available in this session) — still owed if/when a browser-capable session is available.
30.33 [DONE 2026-08-01] Mobile login screen (`lib/screens/auth/login_screen.dart`): identifier field label `'Member ID (Email / NID)'` → `'Email or Username'`; password field label `'Portal Password'` → `'Password'`; logo circle container background `Colors.white.withValues(alpha: 0.03)` → `Colors.black`.
30.34 [DONE 2026-08-01] Mobile: fixed floating-label/text overlap on ALL text fields app-wide, centrally in `lib/core/theme/app_theme.dart`'s `inputDecorationTheme` — added `floatingLabelBehavior: FloatingLabelBehavior.always` + a distinct `floatingLabelStyle` (royalGold, w700) and bumped vertical `contentPadding` (`spaceM` → `spaceM + 4`), since the label previously used `auto` behavior and sat centered on the border when a field was unfocused/empty.
30.35 [DONE 2026-08-01] Mobile: ran `flutter test` full suite after 30.33/30.34 — `widget_test.dart` (branding/button assertions) passes; `flutter analyze` clean on both edited files. Pre-existing golden pixel-diff failures in `comprehensive_visual_freeze_test.dart`/`full_app_visual_freeze_test.dart` remain (expected — goldens are known-stale per `session_mobile_ci_golden_fix.md`, CI skips the pixel-compare, `test/failures` is untracked); the `floatingLabelBehavior`/padding change in `app_theme.dart` will shift these goldens further on any screen with text fields — regenerate goldens in a follow-up pass if/when the golden baseline is refreshed, not part of this fix.
30.36 [DONE 2026-08-02] REVIEW (raised by user 2026-08-01): redirect logic confirmed already correct (web `login.ts navigateAfterLogin()` routes Admin/SuperAdmin to `/admin/approvals`; mobile routes staff-admin roles to `/admin_dashboard`) — no regression found. DISCOVERABILITY half implemented additively: `common/user-menu/user-menu.ts` gained an optional `@Input() isAdmin`, `user-menu.html` renders a conditional "Admin Panel" link (icon + label) when true, `user-menu.scss` adds a scoped `.admin-panel-link` rule; `layouts/portal-layout/portal-layout.html` wires `[isAdmin]="nav.isAdmin()"` into the existing `<app-user-menu>` (sidebar link untouched). `admin-layout.html`'s own `<app-user-menu>` usage doesn't pass `isAdmin`, so it correctly stays `false` there (no duplicate link inside the admin panel itself).
```

</details>

### AREA 31: DATABASE MIGRATIONS (merged from .claude/memory/outstanding_todos.md)

5 items

> This section supersedes `.claude/memory/outstanding_todos.md`, which was stale and is no longer maintained.

| # | Status | Component | Summary |
|---|---|---|---|
| 31.1 | `DONE` | DB | DB: `PhaseB_S5S8_OtpHmac_PaymentIdempotency_Indexes` applied (User.FailedLoginAttempts/LockoutUntil, PaymentHistory.GatewayPaymentId + unique partial index, Otp.Code widened to varchar(64)… |
| 31.2 | `DONE` | DB | DB: RefreshTokens table shipped — folded into the `AddDiscussionForums` migration rather than a standalone `AddRefreshTokens`. Table: RefreshTokens(Id, UserId FK→Users, TokenHash… |
| 31.3 | `NOTE` | DB | DB: There are NO pending migrations — the dev database is up to date. Area 28.0's "DEPENDS ON PhaseB_S5S8 + AddRefreshTokens applied first" precondition is therefore already satisfied. |
| 31.4 | `NOTE` | DB | DB: EF reporting "pending model changes" on PgSql is spurious, non-deterministic seed churn — NOT schema drift. Never scaffold or apply a migration for it. |
| 31.5 | `DONE` | Mobile | Mobile: JWT refresh flow complete — `/auth/refresh` + `/auth/refresh-mobile` with dedicated rate-limit policies, and the Flutter `api_client.dart` interceptor performs the refresh (no… |

<details>
<summary>AREA 31 — original entries and completion notes (5 items)</summary>

```text
31.1 [DONE] DB: `PhaseB_S5S8_OtpHmac_PaymentIdempotency_Indexes` applied (User.FailedLoginAttempts/LockoutUntil, PaymentHistory.GatewayPaymentId + unique partial index, Otp.Code widened to varchar(64) for HMAC-SHA256 hex, Member(Status,IsArchived), User(MemberId), User(ResetToken) partial, User(GoogleId), User(FacebookId), Otp(Email,ExpiryAt)).
31.2 [DONE] DB: RefreshTokens table shipped — folded into the `AddDiscussionForums` migration rather than a standalone `AddRefreshTokens`. Table: RefreshTokens(Id, UserId FK→Users, TokenHash varchar(64) UNIQUE, ExpiresAt, CreatedAt, IsRevoked); indexes TokenHash (unique) + (UserId, IsRevoked).
31.3 [NOTE] DB: There are NO pending migrations — the dev database is up to date. Area 28.0's "DEPENDS ON PhaseB_S5S8 + AddRefreshTokens applied first" precondition is therefore already satisfied.
31.4 [NOTE] DB: EF reporting "pending model changes" on PgSql is spurious, non-deterministic seed churn — NOT schema drift. Never scaffold or apply a migration for it.
31.5 [DONE] Mobile: JWT refresh flow complete — `/auth/refresh` + `/auth/refresh-mobile` with dedicated rate-limit policies, and the Flutter `api_client.dart` interceptor performs the refresh (no longer forces re-login every 60 min).
```

</details>

### AREA 32: PROFILE DATA INTEGRITY (raised by user 2026-08-01)

3 items · completed 2026-08-02

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 32.1 | `DONE` | API | INVESTIGATED and confirmed: (b) mapping/DTO bug — but in the **Angular frontend**, not the backend (opposite layer from 30.27's EC fix). `GHCAA.Infrastructure/Services/MemberService.cs`… | 2026-08-02 |
| 32.2 | `DONE` | Process | AUDITED: user's suspicion confirmed — the same hardcoded-whitelist mapping bug existed in 2 more places, both in the admin panel (grep for the whitelist pattern across `GHCAA.Web/src` found… | 2026-08-02 |
| 32.3 | `DONE` | API | FIXED centrally in all 3 files identified by 32.2, replacing each hardcoded field whitelist with a generic case-insensitive key-normalization pass over every own-enumerable key of the raw… | 2026-08-02 |

<details>
<summary>AREA 32 — original entries and completion notes (3 items)</summary>

```text
32.1 [DONE 2026-08-02] INVESTIGATED and confirmed: (b) mapping/DTO bug — but in the **Angular frontend**, not the backend (opposite layer from 30.27's EC fix). `GHCAA.Infrastructure/Services/MemberService.cs` `GetProfileAsync` already correctly `.Include()`s AcademicHistory/ProfessionalHistory and maps every field (PassingYear/Degree/Subject/Designation/OrganizationName/ProfessionalSector/Location) into the DTO — the backend/DB layer is not the problem. The actual bug: `GHCAA.Web/src/app/member/profile/profile.ts`'s `applyProfileResponse()` built the client-side `profile` object from a hardcoded ~30-field whitelist (added under a "29D.8" comment for case normalization); any backend DTO field NOT in that fixed list — `designation`, `organizationName`, `professionalSector`, `location`, `profileCompletionPercentage`, `categoryBadge`, etc. — was silently dropped and rendered blank. Education History itself displayed fine (its array was copied unconditionally); "Professional Info" fields were the ones actually missing, matching the user's report. ORIGINAL: Member profile page is missing/showing incorrect values for fields that were previously populated and displayed correctly (user specifically flagged Educational Info and Professional Info sections). Compare, per member: (a) what's in `Seed/*.json` (or whichever seed source is authoritative), (b) what's actually in the live DB tables (per [[gotcha_seed_json_vs_live_db.md]] — editing Seed JSON alone does NOT update an already-created SQLite DB, so seed and live DB can and do diverge; check the live DB directly, not just the seed files), and (c) what the profile API response / `profile.html` actually renders. Identify whether this is a data-loss bug (values never made it into the DB), a mapping/DTO bug (values exist in DB but aren't returned/rendered), or a stale-seed bug (DB was seeded before a field was added/renamed and never re-seeded).
32.2 [DONE 2026-08-02] AUDITED: user's suspicion confirmed — the same hardcoded-whitelist mapping bug existed in 2 more places, both in the admin panel (grep for the whitelist pattern across `GHCAA.Web/src` found exactly 3 hits total, all now fixed): `admin/members/admin-members.ts`'s `openDetail()` (the admin member-detail modal — same entity as the member's own profile page, was missing designation/organizationName/professionalSector/location/profileCompletionPercentage etc., i.e. the exact modal flagged separately in Area 33's 33.1/33.2 findings) and its `loadMembers()` list mapping (narrower field set, same fragile pattern). `admin/member-approval/member-approval.ts`'s pending-approval list mapping had the same pattern too (smaller field set, lower risk since it only surfaces id/name/email/mobile/membershipNumber/status/photo/category, none of which were affected, but fixed for consistency). Member Directory, EC/Governance cards (30.27), and Digital ID do NOT use this whitelist pattern — confirmed via grep, no further instances found; 30.27's EC fix was a genuinely separate backend DTO-population bug, unrelated to this one.
32.3 [DONE 2026-08-02] FIXED centrally in all 3 files identified by 32.2, replacing each hardcoded field whitelist with a generic case-insensitive key-normalization pass over every own-enumerable key of the raw response (so any current or future flat DTO field survives automatically instead of requiring manual whitelist maintenance): `member/profile/profile.ts` `applyProfileResponse()`, `admin/members/admin-members.ts` (`openDetail()` + `loadMembers()`), `admin/member-approval/member-approval.ts` `loadMembers()`. No backend/DTO/DB changes were needed (32.1 confirmed the backend was already correct). Added `member/profile/profile.spec.ts` regression tests (2 new cases: flat DTO fields outside the old whitelist now surface; PascalCase/NID key normalization still works) — vitest baseline now 59 files/238 tests (was 236, +2). `npm run type-check` clean, `npm run build` not required (no template/API changes). `dotnet test` unchanged at 317/0 (confirms no backend regression from this fix). No other `docs/` file required correction — `docs/architecture_data_flow.md`/`docs/BUSINESS_FINDINGS.md` already describe the backend DTO/mapping layer, which was already correct; the bug was purely in Angular-side response normalization, not in documented data flow.
```

</details>

### AREA 33: OVERALL DESIGN/LAYOUT & UX REVIEW (raised by user 2026-08-02; PROFILE PAGES ARE PRIORITY #1 within this area, per user 2026-08-02 clarification)

13 items · completed 2026-08-02

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 33.1 | `DONE` | Process | REVIEW: Design/layout consistency, profile pages first. Punch list: **(1)** `member/profile/profile.html` and `admin/members/admin-members.html`'s detail modal render the *same* Member… | 2026-08-02 |
| 33.2 | `DONE` | Process | REVIEW: Content/nav order. **(1)** `profile.html` section order is Membership card → EC History (read-only) → Education History (read-only) → Career History (read-only) → Photo → Signature… | 2026-08-02 |
| 33.3 | `DONE` | Process | REVIEW: Usability pass, profile page first. **(1)** Label clarity: several field/section labels use unnecessarily formal or jargon phrasing that will read as confusing to a first-time… | 2026-08-02 |
| 33.4 | `DONE` | Web | Triage of 33.1-33.3 findings below as 33.5-33.12 (bucket tagged). No fixes were implemented — investigate-and-triage only, per instruction. | 2026-08-02 |
| 33.5 | `DONE` | Web | Replaced all remaining raw emoji action icons (`🗑️` `📷` `🖊️` `💾` `✕` `🚀`) in `member/profile/profile.html` and the admin member-detail modal in `admin/members/admin-members.html` with… | 2026-08-02 |
| 33.6 | `DONE` | Web | CORRECTED SCOPE: the shared icon-button class already existed in `styles.scss` under the name `.icon-btn` (36px, canonical, comment-flagged "SINGLE SOURCE OF TRUTH"), with danger styling… | 2026-08-02 |
| 33.7 | `DONE` | Web | Simplified jargon-heavy labels/headings identified in 33.3(1) to plain language in `profile.html`: "Secure Identity Details" → "Personal Details", "Emergency Protocol Contact" → "Emergency… | 2026-08-02 |
| 33.8 | `DONE` | Web | Applied the `compact` modifier to the *top* read-only empty-placeholder blocks in `profile.html` (EC History L96, Academic L130, Professional L175) — 30.22 only reached the edit-form's… | 2026-08-02 |
| 33.9 | `DONE` | Web | Reconciled `profile.scss`'s dead `.status-grid { grid-template-columns: repeat(3, 1fr) }` with the template's actual `md:grid-cols-4` utility class (33.1 finding 5) by stripping… | 2026-08-02 |
| 33.10 | `DONE` | Web | Reordered `member/profile/profile.html`: Membership Status Card → Photo Upload → Signature Upload → main editable form… | 2026-08-02 |
| 33.11 | `DONE` | Web | Reordered `public/landing.html`: `landing-ec-preview` moved to directly after `landing-purpose` (leadership/trust signal surfaces early); `landing-jobs` moved to after `landing-membership`… | 2026-08-02 |
| 33.12 | `DONE` | Web | Added `docs/PROFILE_SHARED_COMPONENT_DESIGN.md` — design-only doc (no code) proposing 5 shared presentational components (`app-academic-history-editor`, `app-professional-history-editor`… | 2026-08-02 |
| 33.13 | `DONE` | Web | Live-browser Playwright walkthrough (dev servers started manually — `dotnet run` on the API + `ng serve` on Angular, since the `webapp-testing` skill's documented `with_server.py` helper… | 2026-08-02 |

<details>
<summary>AREA 33 — original entries and completion notes (13 items)</summary>

```text
33.1 [DONE 2026-08-02] REVIEW: Design/layout consistency, profile pages first. Punch list: **(1)** `member/profile/profile.html` and `admin/members/admin-members.html`'s detail modal render the *same* Member entity with divergent visual treatment for identical data (member profile: `.history-row` timeline rows with `.badge-info`; admin modal: plain label/value `.view-value` pairs) — no shared sub-component, so any future field addition must be hand-duplicated in two different markup styles. **(2)** Emoji icons (`🗑️` `📷` `🖊️` `💾` `✕` `🚀` `✅` `⏳`) are still used directly in `profile.html` and the admin member-detail modal, bypassing the `app-icon` SVG system rolled out app-wide in 30.3 — these two forms were missed because 30.3 targeted nav items and named action buttons, not ad-hoc buttons inside large forms. **(3)** `profile.html`'s `.remove-photo-btn` is a hand-rolled 28×28px round button, not the shared `.btn-icon`/`.icon-btn` (36px) taxonomy from 30.2 — inconsistent size and no reuse of the danger-color/hover rules already centralized there. **(4)** Empty-state inconsistency confirmed: the read-only "history" sections at the top of `profile.html` (EC History L96, Academic L130, Professional L175) still use bare `.empty-placeholder` (large icon, full height), while the edit-form's own duplicate empty states further down (L386, L477) use `.empty-placeholder dark compact` — i.e. 30.22's "empty states given `.empty-state.compact`" note only reached one of the two duplicate empty-state instances per section, not both, so the top of the page still reserves full height for members with no history. **(5)** `profile.scss`'s `.status-grid` is hardcoded `grid-template-columns: repeat(3, 1fr)` but the template (30.28) now renders 4 grid-items (Profile Health/Associated Batch/Highest Degree/Member Since) inside a `md:grid-cols-4` Tailwind utility class applied directly in the HTML — the component SCSS and the inline utility disagree on column count, so `.status-grid`'s own CSS is dead/overridden weight, a latent trap for the next person who edits `profile.scss` expecting it to control layout. Rest-of-app spacing/token usage (glass-card, form-group, btn*) was otherwise found consistent post-Area-30; no further divergent pages found in admin/portal/public sampling.
33.2 [DONE 2026-08-02] REVIEW: Content/nav order. **(1)** `profile.html` section order is Membership card → EC History (read-only) → Education History (read-only) → Career History (read-only) → Photo → Signature → edit form (Identity → Academic-edit → Professional-edit → Emergency → Address → Privacy → Notifications). For a brand-new member (the most common first-time case) this means three empty-state cards render *before* the member ever reaches an editable field — the actual "fill in your profile" task is buried below content that, for a new user, is empty. **(2)** The admin member-detail modal orders the *same fields* oppositely: Photo/Signature → Personal Details → Address → Emergency → Privacy → Notifications → Admin (verification/points) → Academic → Professional → EC History (Academic/Professional/EC pushed to the very bottom, opposite of the member's own page). Neither order is wrong in isolation, but the two views of one entity disagree, which will read as "different apps" to an admin who's also a member and costs orientation time when cross-referencing a support ticket against a member's own profile. **(3)** `public/landing.html` order is Banner → Purpose → **Jobs-preview** → Membership → Events → News → Gallery → EC-preview (leadership) → CTA-banner. Two placements read as awkward for a first-time visitor: Jobs-preview appears before the Membership/join section, i.e. a job board teaser is shown before the visitor is even told how to become a member; and EC-preview (the leadership/trust-building section) is second-to-last, after Gallery — for an alumni association, "who runs this" is normally a trust signal that belongs nearer the top, not buried after photo galleries. **(4)** Dashboard widget order (profile-completion wizard → stats row → news/events/notifications → recently-joined) and nav section grouping (Overview/Community/Directory/Career/My Account, and admin's Overview/Membership/Content/Finance & Tools) were already reordered/grouped correctly in 30.13/30.23 — no further issues found there.
33.3 [DONE 2026-08-02] REVIEW: Usability pass, profile page first. **(1)** Label clarity: several field/section labels use unnecessarily formal or jargon phrasing that will read as confusing to a first-time member — `profile.html`'s "Consanguinity / Relation" (plain meaning: relationship to the emergency contact), "Secure Identity Details" (Personal Details), "Detailed Academic Milestones" (Education History), "Professional Legacy & Snapshot" / "Detailed Career Timeline" (Work Experience), "Correspondence Identity" (Address). None of these are self-explanatory to a non-technical member and none match the plain-language dashboard checklist labels ("Identity & Photo", "GHC History", "Professional Info") that link into these very sections — so the same data is named three different ways across one page (nav/checklist label vs. section heading vs. status-card labels like "Associated Batch"). **(2)** Empty-state guidance: text is present and generally helpful (e.g. "Your educational journey is currently empty. Add your academic milestones below.") but, per 33.1 finding (4), only half the duplicate empty-state instances got the compact treatment, so new members still see three large ceremonial empty blocks before reaching the form. **(3)** Form validation feedback: present and consistent (red border + inline message pattern via `#xM="ngModel"` + `.touched && .invalid`) across every required field — no issues found here, this part of the page is in good shape. **(4)** Discoverability: re-checked for other "hidden feature" cases beyond 30.36's admin-panel link — none found; `app-user-menu` is the only such affordance and it's now handled. Read-only fields (NID/Email/Mobile) use `opacity-50` + a `title` tooltip to explain "contact admin to change" — acceptable but the reason is only visible on hover/long-press, which is a poor discovery pattern on touch devices (no visible affordance, tooltip requires a pointer). **(5)** Touch targets: `.remove-photo-btn` (28×28px, see 33.1 finding 3) and the inline emoji-only buttons are below the ~44px touch-target guideline; the standard `.icon-btn` (36px, per 30.2) is itself still under 44px but is the established app-wide baseline, so raising it is a larger token-level decision, not a one-page fix. **(6)** Live-browser walkthrough (Playwright/manual, per the `verify` skill) was **not** performed in this session — no browser available; findings above are from template/component code inspection only, and an actual golden-path walkthrough as a new member and as an admin is still owed.
33.4 [DONE 2026-08-02] Triage of 33.1-33.3 findings below as 33.5-33.12 (bucket tagged). No fixes were implemented — investigate-and-triage only, per instruction.
33.5 [DONE 2026-08-02] Replaced all remaining raw emoji action icons (`🗑️` `📷` `🖊️` `💾` `✕` `🚀`) in `member/profile/profile.html` and the admin member-detail modal in `admin/members/admin-members.html` with `<app-icon>`, completing the 30.3 icon-system rollout for these two forms. Added 4 new icon.html cases (`camera`, `pen`, `rocket`, `save`) since no existing case covered these concepts; imported `Icon` into both `profile.ts` and `admin-members.ts`. The Bulk Import Modal's `✕` close-btn (admin-members.html L~800) is a separate, out-of-scope modal — left untouched.
33.6 [DONE 2026-08-02] CORRECTED SCOPE: the shared icon-button class already existed in `styles.scss` under the name `.icon-btn` (36px, canonical, comment-flagged "SINGLE SOURCE OF TRUTH"), with danger styling under the `.delete`/`.archive` modifier — NOT `.btn-icon.danger` as originally phrased (that class/modifier combination never existed). Replaced `profile.html`'s bespoke `.remove-photo-btn` (28px) usages with `class="icon-btn delete"` and deleted the now-dead `.remove-photo-btn` rule from `profile.scss`, preserving its absolute-positioning (`bottom:-5px; right:-5px; z-index:10`) via a new local `.photo-preview-wrap .icon-btn` rule.
33.7 [DONE 2026-08-02] Simplified jargon-heavy labels/headings identified in 33.3(1) to plain language in `profile.html`: "Secure Identity Details" → "Personal Details", "Emergency Protocol Contact" → "Emergency Contact Details", "Consanguinity / Relation *" → "Relation *" — each now matches the admin member-detail modal's existing plain-language wording exactly (admin-members.html already used "Emergency Contact Details"/"Relation", so no admin-side edit was needed).
33.8 [DONE 2026-08-02] Applied the `compact` modifier to the *top* read-only empty-placeholder blocks in `profile.html` (EC History L96, Academic L130, Professional L175) — 30.22 only reached the edit-form's duplicate empty states (L386/L477), not these.
33.9 [DONE 2026-08-02] Reconciled `profile.scss`'s dead `.status-grid { grid-template-columns: repeat(3, 1fr) }` with the template's actual `md:grid-cols-4` utility class (33.1 finding 5) by stripping `display`/`grid-template-columns` from the SCSS rule, leaving only `gap`/`padding-top`/`border-top` — the template's Tailwind utility classes are now sole source of truth for the grid layout.
33.10 [DONE 2026-08-02] Reordered `member/profile/profile.html`: Membership Status Card → Photo Upload → Signature Upload → main editable form (Personal/Academic/Professional/Emergency/Address/Privacy/Notifications) → read-only EC/Academic/Professional history recaps (moved to the bottom). Pure block-move, no internal markup changes; confirmed no order-dependent selectors in `profile.scss` (`.section-block:first-of-type` is scoped inside the form) or assertions in `profile.spec.ts`. A new member now reaches an actionable field immediately after the status card instead of three empty-state cards.
33.11 [DONE 2026-08-02] Reordered `public/landing.html`: `landing-ec-preview` moved to directly after `landing-purpose` (leadership/trust signal surfaces early); `landing-jobs` moved to after `landing-membership` (join-CTA/credibility now precede the job-board teaser). Pure line reorder, no markup changes.
33.12 [DONE 2026-08-02] Added `docs/PROFILE_SHARED_COMPONENT_DESIGN.md` — design-only doc (no code) proposing 5 shared presentational components (`app-academic-history-editor`, `app-professional-history-editor`, `app-ec-history-view`, `app-emergency-contact-form`, `app-address-form`) driven by `@Input()`/`@Output()` off the same underlying `Member` fields both `profile.html` and the admin member-detail modal already bind to. Documents the section-order/label/duplication differences that must be reconciled before extraction and flags implementation as a separate future TODO once reviewed.
33.13 [DONE 2026-08-02] Live-browser Playwright walkthrough (dev servers started manually — `dotnet run` on the API + `ng serve` on Angular, since the `webapp-testing` skill's documented `with_server.py` helper does not exist in this installation). Verified: (1) landing page renders the new 33.11 section order exactly (`landing-banner, landing-purpose, landing-ec-preview, landing-membership, landing-jobs, landing-events, landing-news, landing-gallery-preview, landing-cta-banner`); (2) member login (`demo_user`/`DemoPass123!`) succeeds and lands on `/portal/dashboard`; (3) `/portal/profile` renders the new 33.10 order exactly (Membership Status → Photo → Signature → main form → Association Governance History → Educational Timeline → Professional Experience), with no visual breakage scrolling through the form (address/privacy/notification sections render normally). Not verified: the admin member-detail modal — both `shalin`/`Shalin@2024!` and `superadmin`/`SuperAdminPassword123!` logged in as ordinary alumni members (no admin role), so `/admin/members` redirected to `/portal/dashboard` via `adminGuard`; this is a local seed/role mismatch (per the known live-DB-vs-seed-JSON gotcha), not a regression from this session's changes, and 33.12 made no code changes to `admin-members.html` to verify.
```

</details>

### AREA 34: ADMIN-MANAGED SITE CONTENT + MERGED NEWS & NOTICE BOARD (raised by user 2026-08-02)

33 items · completed 2026-08-02 → 2026-08-11

> User instruction set: (1) update About Us with college + association information, (2) add an On Campus Address to Contact Us, (3) make both manageable from admin, (4) merge News and Notice management, notices may carry PDF documents. Standing constraints: **notices are admin-post-only**; About content is sourced from the GHCAA Constitution text and facts already in the repo **only** (no web research, no invented college facts); web + mobile parity; OrgConfig additions must be industry-standard and configurable; nothing may break.

#### 34.A — SiteContent CMS (About Us / Contact intro)

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 34.A1 | `DONE` | API | Domain: `GHCAA.Domain/Models/SiteContent.cs` (`Id, Key, Title, BodyHtml, DisplayOrder, IsActive, LastModified, UpdatedByAdminId`) +… | 2026-08-02 |
| 34.A2 | `DONE` | Cross-platform | Application/Infrastructure: `DTOs/SiteContentDto.cs` (`SiteContentDto`, `UpsertSiteContentDto`), `Interfaces/ISiteContentService.cs`, `Infrastructure/Services/SiteContentService.cs`… | 2026-08-02 |
| 34.A3 | `DONE` | API | API: `Controllers/SiteContentController.cs` — `GET /api/site-content?group=about` is `[AllowAnonymous]` (active blocks, `DisplayOrder` ordered); `GET /api/site-content/admin`, `POST`, `PUT… | 2026-08-02 |
| 34.A4 | `DONE` | API | Seed: `Infrastructure/Data/Seed/site-content.json` loaded through the existing `LoadSeed<T>` path. Blocks: `about-origin` (existing repo 1938 / Ashutosh Ganguly / Sher-e-Bangla text)… | 2026-08-02 |
| 34.A5 | `DONE` | Web | Web: `core/services/site-content.service.ts` + `SiteContent` interface in `core/models/business.models.ts`; `public/about/about.ts\|.html` renders CMS blocks via `[innerHTML]` inside the… | 2026-08-02 |
| 34.A6 | `DONE` | Web | Web admin: `admin/site-content/` screen (list, rich-text edit via the shared `<app-rich-text-editor>`, reorder, activate/deactivate); routed under the `admin` children in `app.routes.ts`… | 2026-08-02 |

#### 34.B — Contact Us on-campus address (OrgConfig)

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 34.B1 | `DONE` | API | Backend: `ContactDto` in `Application/DTOs/OrgConfigDto.cs` gains `CampusAddress`, `PhoneNumbers` (`List<string>`), `MapEmbedUrl`; `OrgConfigService.BuildGhcaaDefaults()` seeds the campus… | 2026-08-02 |
| 34.B2 | `DONE` | Web | Web: same three fields in `core/models/org-config.model.ts` + `core/services/org-config.service.ts` fallback; `public/contact/contact.html` binds address / `supportEmail` / phones /… | 2026-08-02 |
| 34.B3 | `DONE` | Web | Web admin: the SuperAdmin-gated `admin/org-config` form exposes campus address, map embed URL, and an add/remove repeater for phone numbers — so every new value is admin-configurable, not… | 2026-08-02 |
| 34.B4 | `DONE` | Mobile | Mobile: `GHCAA.Mobile/lib/core/config/org_config.dart` mirrors `campusAddress` / `phoneNumbers` / `mapEmbedUrl` in the model, `fromJson`, and the GHCAA defaults. | 2026-08-02 |
| 34.B5 | `DONE` | Security | SECURITY: the map embed URL is admin-supplied and therefore untrusted — `contact.ts` runs it through an allow-list check before `bypassSecurityTrustResourceUrl`, and the template renders… | 2026-08-02 |
| 34.B6 | `DONE` | Cross-platform | AUDIT (per user instruction "make sure OrgConfig is industry standard and all currently needed values are configurable"): verified full four-way parity — backend DTO + defaults, web model +… | 2026-08-02 |

#### 34.C — News + Notice unified

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 34.C1 | `DONE` | API | Domain: `PostType { News, Notice }` enum and `FileUploadType.NoticeDocument` added to `GHCAA.Domain/Enums.cs`; `NewsPost` gains `PostType` (defaults to `News`), `AttachmentUrl`… | 2026-08-02 |
| 34.C2 | `DONE` | API | API: `GetActiveNews` takes an optional `postType` filter threaded through `INewsService.GetActiveNewsAsync`; new `POST /api/news/upload-document` (`AdminOnly`, `FileCategory.Document`, 10… | 2026-08-02 |
| 34.C3 | `DONE` | Security | SECURITY — **notices are admin-post-only**: the member-facing `SubmitArticle` endpoint rejects `PostType.Notice` with `Forbid()` unless the caller is Admin/SuperAdmin… | 2026-08-02 |
| 34.C4 | `DONE` | API | DTOs: `PostType` + attachment fields on `CreateNewsDto`, `UpdateNewsDto`, `NewsDto`; `NewsService` filters by `PostType`. | 2026-08-02 |
| 34.C5 | `DONE` | Web | Web admin: `admin/news` gained a News/Notice tab filter over the same list, a `postType` selector in the create/edit form, and a PDF picker reusing `core/utils/file-validation.util.ts` for… | 2026-08-02 |
| 34.C6 | `DONE` | Web | Web public/portal: the shared `common/news` feed gained the same News/Notices tabs, a `.notice` accent treatment, and a PDF download affordance on posts that carry an attachment. | 2026-08-02 |
| 34.C7 | `DONE` | Mobile | Nav: a **Notices** entry in `layouts/public-layout/public-layout.html` deep-links to `/news?type=Notice`; `common/news/news.ts` syncs the tab with `ActivatedRoute.queryParamMap` and writes… | 2026-08-02 |
| 34.C8 | `DONE` | Mobile | Mobile: `NewsService.getLatestNews({postType})` filter plus notice/attachment handling in `news_screen.dart` / `news_details_screen.dart`. | 2026-08-02 |

#### 34.D — Cross-cutting (raised mid-implementation by user)

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 34.D1 | `DONE` | Cross-platform | "Fully responsive UI, try avoid scrolling": the news feed became a `repeat(auto-fill, minmax(320px, 1fr))` card grid with `line-clamp`-ed titles/bodies (equal-height cards, many more posts… | 2026-08-02 |
| 34.D2 | `DONE` | Cross-platform | "Try common changes as reusable": `POST_TYPE_TABS` + `matchesPostType()` added to `core/constants/app.constants.ts` and adopted by **both** the admin console and the public/portal feed, so… | 2026-08-02 |
| 34.D3 | `DONE` | Cross-platform | Style de-duplication found while doing 34.D2: `.pill` (×3), `.actions-cell` (×3), `.title-cell` (×2), `.key-cell`, and `.status-badge` copies that shadowed the already-central definition… | 2026-08-02 |
| 34.D4 | `DONE` | DB | Migrations: single migration covering the `SiteContents` table + the three new `NewsPosts` columns, added to the PostgreSQL tree only — SQLite's schema comes from `EnsureCreated()`, and per… | 2026-08-02 |
| 34.D5 | `DONE` | Tests | Tests: backend `dotnet test` **330 passed / 0 failed** (includes new SiteContent service + controller tests and the NewsController non-admin-Notice-rejection and `upload-document` cases)… | 2026-08-02 |
| 34.D6 | `DONE` | Tests | Mobile tests: `flutter analyze` clean after updating the two `FakeNewsService` overrides in `test/comprehensive_visual_freeze_test.dart` and `test/full_app_visual_freeze_test.dart` for the… | 2026-08-02 |
| 34.D7 | `DONE` | Cross-platform | Regenerate the 21 stale mobile goldens (`flutter test --update-goldens`) as a standalone housekeeping task, so a genuinely new mobile regression is not masked by the existing baseline… | 2026-08-11 |
| 34.D9 | `DONE` | Cross-platform | Preprod schema catch-up script `docs/sql/preprod_area34_sitecontent.sql` — idempotent SQL mirroring migration `20260802163432_AddSiteContentAndNoticeFields` (three `NewsPosts` columns +… | 2026-08-03 |
| 34.D11 | `DONE` | Cross-platform | Seeded SiteContent titles contained HTML entities (`Logo &amp; Flag`, `Purpose &amp; Objectives`), but `about.html` renders the title with `{{ }}` interpolation (which escapes) while only… | 2026-08-03 |
| 34.D12 | `DONE` | Cross-platform | Public nav fixes raised by user: (a) the association full name never rendered under the logo — `public-layout.html` carried `class="hidden xl:block"`, but this project has **no Tailwind**… | 2026-08-03 |
| 34.D13 | `DONE` | Cross-platform | Logo asset: the crest artwork the user pasted is `assets/logo.jpg` — RGB 1024×1024 on an **opaque black square**. The existing `logo.png` was a transparent but lower-resolution 676×814… | 2026-08-03 |
| 34.D14 | `DONE` | Cross-platform | "Cant see public theme switcher" investigated and found to be **not a code defect**. Verified in a real headless Chromium at 1440×900 against the dev server: `app-theme-toggle` count 1… | 2026-08-03 |
| 34.D15 | `DONE` | Cross-platform | About page content rewritten per "add about college, see official college website, and dont direct copy from constitution, write in meaningful way for others" — this **supersedes** the… | 2026-08-03 |

<details>
<summary>AREA 34 — original entries and completion notes (33 items)</summary>

```text
34.A1 [DONE 2026-08-02] Domain: `GHCAA.Domain/Models/SiteContent.cs` (`Id, Key, Title, BodyHtml, DisplayOrder, IsActive, LastModified, UpdatedByAdminId`) + `Infrastructure/Data/Configurations/SiteContentConfiguration.cs` (unique index on `Key`) + `DbSet<SiteContent>` on `ApplicationDbContext` (config auto-discovered by the existing `ApplyConfigurationsFromAssembly`).
34.A2 [DONE 2026-08-02] Application/Infrastructure: `DTOs/SiteContentDto.cs` (`SiteContentDto`, `UpsertSiteContentDto`), `Interfaces/ISiteContentService.cs`, `Infrastructure/Services/SiteContentService.cs` (registered by the existing `I*`→impl convention loop). `BodyHtml` is server-side sanitized via the existing `HtmlSanitizer` on every write, mirroring `NewsService`.
34.A3 [DONE 2026-08-02] API: `Controllers/SiteContentController.cs` — `GET /api/site-content?group=about` is `[AllowAnonymous]` (active blocks, `DisplayOrder` ordered); `GET /api/site-content/admin`, `POST`, `PUT /{id}`, `DELETE /{id}` are all `[Authorize(Policy = "AdminOnly")]`.
34.A4 [DONE 2026-08-02] Seed: `Infrastructure/Data/Seed/site-content.json` loaded through the existing `LoadSeed<T>` path. Blocks: `about-origin` (existing repo 1938 / Ashutosh Ganguly / Sher-e-Bangla text), `about-association` (Article I — name, "HARAGANGIAN", founding 29 Nov 2025, non-political/non-religious/non-profit/inclusive/philanthropic nature, motto EN+Bengali), `about-logo` (Article I §6 logo symbolism + §7 flag), `about-objectives` (Article II, 11 objectives), `contact-intro`. Constitution blanks (theme song, registration no., registered office, banner, digital platform, social handles) are deliberately **omitted** — they are unfilled placeholders in the source document, not content.
34.A5 [DONE 2026-08-02] Web: `core/services/site-content.service.ts` + `SiteContent` interface in `core/models/business.models.ts`; `public/about/about.ts|.html` renders CMS blocks via `[innerHTML]` inside the existing `.glass-card`/`.story-grid` shell, **retaining the previous hardcoded markup as a fallback** so the page never renders empty.
34.A6 [DONE 2026-08-02] Web admin: `admin/site-content/` screen (list, rich-text edit via the shared `<app-rich-text-editor>`, reorder, activate/deactivate); routed under the `admin` children in `app.routes.ts` and added to `adminNavItems` (Content section) in `core/services/nav.service.ts` with a matching admin-layout icon case.
34.B1 [DONE 2026-08-02] Backend: `ContactDto` in `Application/DTOs/OrgConfigDto.cs` gains `CampusAddress`, `PhoneNumbers` (`List<string>`), `MapEmbedUrl`; `OrgConfigService.BuildGhcaaDefaults()` seeds the campus address and the two phone numbers previously hardcoded in `contact.html`.
34.B2 [DONE 2026-08-02] Web: same three fields in `core/models/org-config.model.ts` + `core/services/org-config.service.ts` fallback; `public/contact/contact.html` binds address / `supportEmail` / phones / `socialLinks` from config instead of hardcoded markup (the three dead `href="#"` social buttons now resolve to configured links), and the header blurb comes from the `contact-intro` SiteContent block.
34.B3 [DONE 2026-08-02] Web admin: the SuperAdmin-gated `admin/org-config` form exposes campus address, map embed URL, and an add/remove repeater for phone numbers — so every new value is admin-configurable, not deploy-bound.
34.B4 [DONE 2026-08-02] Mobile: `GHCAA.Mobile/lib/core/config/org_config.dart` mirrors `campusAddress` / `phoneNumbers` / `mapEmbedUrl` in the model, `fromJson`, and the GHCAA defaults.
34.B5 [DONE 2026-08-02] SECURITY: the map embed URL is admin-supplied and therefore untrusted — `contact.ts` runs it through an allow-list check before `bypassSecurityTrustResourceUrl`, and the template renders the iframe only via `@if (mapUrl(); as src)` so a rejected URL yields no element at all.
34.B6 [DONE 2026-08-02] AUDIT (per user instruction "make sure OrgConfig is industry standard and all currently needed values are configurable"): verified full four-way parity — backend DTO + defaults, web model + service fallback, web admin form, mobile config — with no field configurable in one layer and missing in another.
34.C1 [DONE 2026-08-02] Domain: `PostType { News, Notice }` enum and `FileUploadType.NoticeDocument` added to `GHCAA.Domain/Enums.cs`; `NewsPost` gains `PostType` (defaults to `News`), `AttachmentUrl`, `AttachmentFileName`.
34.C2 [DONE 2026-08-02] API: `GetActiveNews` takes an optional `postType` filter threaded through `INewsService.GetActiveNewsAsync`; new `POST /api/news/upload-document` (`AdminOnly`, `FileCategory.Document`, 10 MB cap, `FileUploadType.NoticeDocument`) mirroring the existing `upload-image` action.
34.C3 [DONE 2026-08-02] SECURITY — **notices are admin-post-only**: the member-facing `SubmitArticle` endpoint rejects `PostType.Notice` with `Forbid()` unless the caller is Admin/SuperAdmin; `CreateNews`/`UpdateNews`/`upload-document` were already `AdminOnly`.
34.C4 [DONE 2026-08-02] DTOs: `PostType` + attachment fields on `CreateNewsDto`, `UpdateNewsDto`, `NewsDto`; `NewsService` filters by `PostType`.
34.C5 [DONE 2026-08-02] Web admin: `admin/news` gained a News/Notice tab filter over the same list, a `postType` selector in the create/edit form, and a PDF picker reusing `core/utils/file-validation.util.ts` for client-side type/size checks — one screen manages both post kinds, no second admin page.
34.C6 [DONE 2026-08-02] Web public/portal: the shared `common/news` feed gained the same News/Notices tabs, a `.notice` accent treatment, and a PDF download affordance on posts that carry an attachment.
34.C7 [DONE 2026-08-02] Nav: a **Notices** entry in `layouts/public-layout/public-layout.html` deep-links to `/news?type=Notice`; `common/news/news.ts` syncs the tab with `ActivatedRoute.queryParamMap` and writes it back via `replaceUrl` — the tab is shareable and no duplicate Notices page/component exists to maintain.
34.C8 [DONE 2026-08-02] Mobile: `NewsService.getLatestNews({postType})` filter plus notice/attachment handling in `news_screen.dart` / `news_details_screen.dart`.
34.D1 [DONE 2026-08-02] "Fully responsive UI, try avoid scrolling": the news feed became a `repeat(auto-fill, minmax(320px, 1fr))` card grid with `line-clamp`-ed titles/bodies (equal-height cards, many more posts above the fold, single column below 640px to avoid horizontal overflow); `about.scss`'s fixed 10rem/8rem/5rem rhythm and 70vh hero became `clamp()`-based fluid spacing with `auto-fit` story/pillar grids; `contact.scss`'s info panel became `minmax(280px, 380px)` and only stacks at 900px (was 1200px, which doubled page height prematurely).
34.D2 [DONE 2026-08-02] "Try common changes as reusable": `POST_TYPE_TABS` + `matchesPostType()` added to `core/constants/app.constants.ts` and adopted by **both** the admin console and the public/portal feed, so the two filters cannot drift (legacy posts with no `postType` count as News in one place only).
34.D3 [DONE 2026-08-02] Style de-duplication found while doing 34.D2: `.pill` (×3), `.actions-cell` (×3), `.title-cell` (×2), `.key-cell`, and `.status-badge` copies that shadowed the already-central definition were consolidated into `src/styles.scss` as a "data-table cell utilities" block and deleted from `admin/news`, `admin/site-content`, `admin/members`, and `admin/gallery` stylesheets, each left with a pointer comment. New central `.attachment-flag` / `.doc-link` cover the three attachment affordances instead of a new component (per the keep-it-lightweight constraint). **Trap this fixes:** component SCSS gets a view-encapsulation attribute suffix and therefore out-specifies an identical global rule, so a duplicated copy silently forks the look.
34.D4 [DONE 2026-08-02] Migrations: single migration covering the `SiteContents` table + the three new `NewsPosts` columns, added to the PostgreSQL tree only — SQLite's schema comes from `EnsureCreated()`, and per 31.4 the PgSql "pending model changes" seed churn was correctly ignored.
34.D5 [DONE 2026-08-02] Tests: backend `dotnet test` **330 passed / 0 failed** (includes new SiteContent service + controller tests and the NewsController non-admin-Notice-rejection and `upload-document` cases); web `npx vitest run` **59 files / 240 tests passed** (baseline 238, +2 new `common/news` specs for the postType filter and detail-panel auto-close); `npm run build` (AOT — the only thing that type-checks Angular templates; `tsc --noEmit` does not) succeeded with no new warnings.
34.D6 [DONE 2026-08-02] Mobile tests: `flutter analyze` clean after updating the two `FakeNewsService` overrides in `test/comprehensive_visual_freeze_test.dart` and `test/full_app_visual_freeze_test.dart` for the new `{String? postType}` parameter; `flutter test` — 70 non-golden tests pass. The 21 failing goldens (auth_login, auth_register, registration_*, directory/member_*, news_portal, events/financial) are the **pre-existing stale baseline** from the earlier app-wide `app_theme.dart` floating-label change, span screens this area never touched, and are skipped on CI per the existing `flutter_test_config` golden guard.
34.D7 [TODO] Regenerate the 21 stale mobile goldens (`flutter test --update-goldens`) as a standalone housekeeping task, so a genuinely new mobile regression is not masked by the existing baseline drift. Deliberately kept out of this area — it is unrelated binary churn from a prior session's theme fix.
     [2026-08-11 audit] Superseded by 35.B1, which owns the same golden regeneration with an accurate count (26 stale, not 21) and the per-image diff spread.
34.D9 [DONE 2026-08-03] Preprod schema catch-up script `docs/sql/preprod_area34_sitecontent.sql` — idempotent SQL mirroring migration `20260802163432_AddSiteContentAndNoticeFields` (three `NewsPosts` columns + `SiteContents` table, unique `Key` index, the five seed blocks, and an identity-sequence `setval` so the first admin-created block does not collide on `Id = 1`). Needed because Area 34 shipped to preprod but the content never appeared: the runtime builds schema with `EnsureCreated()` (`GHCAA.API/Program.cs:290`), which is a **no-op on a database that already has tables**, so the new table and columns were never created and `/about` fell back to its static markup. **Applied to preprod/Neon 2026-08-03 and verified**: the three `NewsPosts` columns exist, the five blocks are present, the identity sequence sits at 5, `GET /api/news` returns 200 (was a 500 — `42703: column n.AttachmentFileName does not exist`) and `GET /api/site-content?group=about` returns the four About blocks.
34.D11 [DONE 2026-08-03] Seeded SiteContent titles contained HTML entities (`Logo &amp; Flag`, `Purpose &amp; Objectives`), but `about.html` renders the title with `{{ }}` interpolation (which escapes) while only the body uses `[innerHTML]` — so the headings showed a literal `&amp;`. Corrected in all four places that carry the value: `Seed/site_content.json`, the migration's `InsertData`, `docs/sql/preprod_area34_sitecontent.sql`, and the live Neon rows (`UPDATE ... replace("Title",'&amp;','&')`). Body HTML entities (`&ndash;`, `&ldquo;`) are correct as-is — that field *is* parsed as HTML.
34.D12 [DONE 2026-08-03] Public nav fixes raised by user: (a) the association full name never rendered under the logo — `public-layout.html` carried `class="hidden xl:block"`, but this project has **no Tailwind**, so `hidden` matched the real global `.hidden { display: none }` utility in `styles.scss` while `xl:block` matched nothing; both classes removed, leaving `.hide-mobile` and the existing `≤600px` rule to do the responsive hiding. (b) The public site could get stuck on a white background: the theme choice is persisted globally by `ThemeService`, but the switch lived only in `<app-user-menu>` (portal/admin), so a visitor who once chose light mode had no way back. Extracted the button into a shared `common/theme-toggle/` component (per the "try common changes as reusable" constraint) and placed it in both the user menu and the public nav. `npx vitest run` **60 files / 244 tests** green; `ng build` clean.
34.D13 [DONE 2026-08-03] Logo asset: the crest artwork the user pasted is `assets/logo.jpg` — RGB 1024×1024 on an **opaque black square**. The existing `logo.png` was a transparent but lower-resolution 676×814 crop, so it was not the same artwork. Per "make another version of transparent background of these without another changes, and use it everywhere", a new transparent `logo.png` was derived **from the jpg** at RGBA 1024×1024 by BFS flood-fill seeded from every near-black (≤40) pixel on the four image edges — edge-seeding preserves interior black artwork and clears only background-contiguous black, so the drawing itself is unchanged. Installed to `GHCAA.Web/public/assets/logo.png` and `GHCAA.Mobile/assets/logo.png` (527,642 B, smaller than the 908,708 B it replaced), verified by compositing over white and over `#0c0c0c` — no halo, legible on both. The remaining 6 `appImgFallback` placeholders still on the jpg were repointed (`common/gallery/gallery.html`, `gallery.ts`, `common/news/news.html`, `public/magazine/magazine.html` ×3), so all 23 `assets/logo` references app-wide now use the png. This **reverses** the earlier "keep the gallery/news jpg fallbacks" rule, which predated the "use it every where" instruction. `Seed/photos.json` still names logo.jpg deliberately — sample gallery content, not branding. `flutter analyze` 0, `ng build` 0, `vitest` 60 files / 244 tests, `dotnet test` 330 passed.
34.D14 [DONE 2026-08-03] "Cant see public theme switcher" investigated and found to be **not a code defect**. Verified in a real headless Chromium at 1440×900 against the dev server: `app-theme-toggle` count 1, `visible: true`, box `{x:1376, y:18, w:20, h:25}`, computed `color: rgba(255,255,255,0.85)`, `title="Switch to light mode"`, and the `theme-light` sun glyph present as a correctly-namespaced `<circle>`+`<path>` inside a 20×20 `<svg>`. Two hypotheses were disproved along the way and are recorded so they are not re-chased: (a) `theme-dark`/`theme-light` *are* valid icon names — they live in `common/icon/icon.html` (lines ~159 and ~165), not in `icon.ts`, which is why grepping the `.ts` found nothing; (b) the pile of `<!--container-->` comments inside the toggle's `svg.innerHTML` is **normal** — Angular `@switch` emits one comment anchor per non-matching `@case`, so read `children.length` and `namespaceURI` instead of eyeballing innerHTML. Remaining explanations are non-code: low discoverability (a bare 20px 85%-white stroke icon pinned at the extreme right edge after eight bold uppercase links) or the user viewing the stale preprod deploy. Left unstyled pending user preference, per "keep all other thing as it is".
34.D15 [DONE 2026-08-03] About page content rewritten per "add about college, see official college website, and dont direct copy from constitution, write in meaningful way for others" — this **supersedes** the original plan decision to seed constitution text only with no web research. `Seed/site_content.json` went 5 → 6 blocks: `about-origin` retitled "The College and Its Origin" (18 Dec 1938 founding, Ashutosh Ganguly's one-lakh-rupee donation, advocate Satish Chandra Bhattacharya, A. H. M. Wajir Ali); **new** `about-college-today` with facts sourced from the official site (16 honours departments, 80+ teachers, 100,000+ graduates, campus address, EIIN 111160, college code 5701, principal, outbound link, plus an `<em>` disclaimer that the Association is an independent alumni body); `about-association` reworded out of verbatim constitution phrasing; `about-logo` "logo"→"crest"; and `about-objectives` retitled **"What We Do"** with the 11 constitution objectives restructured into 6 thematic bullets (Connection / Students / Welfare / Community / Heritage / Governance). The college's nationalisation date was **deliberately omitted** — it could not be confirmed on the official site, and no fact was invented. Note the seed JSON alone does **not** reach preprod (`EnsureCreated()` no-ops on a non-empty DB, see 34.D9/34.D10) — the new and changed blocks must go in via the admin SiteContent CMS or a catch-up SQL script.
```

</details>

### AREA 35: MOBILE GAP REVIEW & TEST HARDENING (raised by user 2026-08-11)

3 items · completed 2026-08-11

> Scope: a documentation-vs-source sweep of `GHCAA.Mobile` only — every item below was confirmed against the actual code, not inherited from an earlier area. Items an earlier area already settled deliberately are **not** repeated here (e.g. 29G.2 records walletNumber/bankName/accountNumber as display-only *by design*, so that is not a gap). Each task names the file, the line, and how to verify it, so it can be picked up cold. Standing constraints carry over: web + mobile parity, `flutter analyze` stays clean, and per the standing policy nothing is `[DONE]` until its test passes.
>
> **Measured on this machine (Flutter 3.44.2 / Dart 3.12.2, 2026-08-11):** `flutter analyze` — *No issues found*. `flutter test` — **121 passed / 0 failed**. The sweep started at 66 passed / 26 failed and reached 95 / 26 after 35.B7 and 35.B8. 35.B1 then found that part of that failing count was not a golden problem at all: the package config had drifted, so two visual-freeze files failed to compile rather than mismatch. After `flutter pub get` the real set was 21 stale goldens, and regenerating them took the suite green. **The bar for anything in this area is now zero failures** — any red is yours.

#### 35.B — Test suite

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 35.B1 | `DONE` | Tests | Mobile: regenerated the stale goldens — 21 baselines, not the 26 first counted. See the note below for why the count moved and what was reviewed. | 2026-08-11 |
| 35.B7 | `DONE` | Mobile | Mobile: replaced three placeholder tests that were reporting green while asserting nothing. `test/unit_test.dart` was an empty `AuthService` test (whole body commented out, awaiting a… | 2026-08-11 |
| 35.B8 | `DONE` | Mobile | Mobile: added `test/forum_service_test.dart` — the forum feature (3.7) shipped with no test at all. Covers `getCategories`/`getTopics`/`getPosts` mapping, query-parameter paging, the… | 2026-08-11 |

#### 35.C — Documentation drift found during the sweep

| # | Status | Component | Summary | Completed |
|---|---|---|---|---|
| 35.C1 | `DONE` | Mobile | `docs/PLAN.md` described work that had already shipped, which is a trap for anyone opening the plan file first: its "3.7 Discussion Forums" plan showed all six steps unchecked although 3.7… | 2026-08-11 |

<details>
<summary>AREA 35 — original entries and completion notes (3 items)</summary>

```text
35.B1 [DONE 2026-08-11] Mobile: regenerated the stale goldens. Two things were wrong with the
      original framing of this task. First, the failing count was inflated: `flutter analyze`
      and the two big visual-freeze files were failing to COMPILE, because the package config
      had drifted and `screen_protector` and `flutter_localizations` were unresolved. A
      `flutter pub get` restored them, analyze went back to *No issues found*, and the real
      failing set turned out to be 21 goldens, not 26 — the extra 5 were tests that never ran.
      Second, `member_dashboard` (0.26 %) and `member_dashboard_standard` (0.38 %) were NOT in
      the failing set once the suite compiled, so the "sub-1 % diff" worry did not apply.
      Regenerated with `flutter test --update-goldens`. Result: 121 passed / 0 failed, and
      exactly 21 baseline files changed — no collateral churn. Reviewed the two largest diffs
      against their predecessors before accepting: `registration_step_1` (32.61 %) and
      `login_portal` (21.45 %) both show the Area 30/33 form redesign, field labels moved from
      inside the input to above it. Legitimate, not a regression. One thing to keep an eye on:
      the login crest lost its dark circular backdrop in the same change — consistent with the
      Area 30 shared-layer icon work, but nothing explicitly recorded it.
35.B7 [DONE 2026-08-11] Mobile: replaced three placeholder tests that were reporting green while asserting nothing. `test/unit_test.dart` was an empty `AuthService` test (whole body commented out, awaiting a `build_runner` mockito generation that was never run — the `@GenerateMocks` annotation had no generated `.mocks.dart` anywhere in the repo) plus a literal `expect(true, true)`; it is now real `AppUtils` coverage — the ISO/`dd-MM-yyyy` dual-parse contract settled in 29F.3/23.4, `toWire`, currency, and initials. `test/model_test.dart` imported nothing from `lib/` and asserted values it had just written into a local map; it now covers the `fromJson` factories actually shipped in `forum_service.dart` and `org_config.dart`, including the `OrgConfig` contact fields added by 34.B4. `test/router_test.dart` asserted only `routes.isNotEmpty`, which cannot fail while a single route exists; it now pins the registered paths, so deleting or renaming one is caught.
35.B8 [DONE 2026-08-11] Mobile: added `test/forum_service_test.dart` — the forum feature (3.7) shipped with no test at all. Covers `getCategories`/`getTopics`/`getPosts` mapping, query-parameter paging, the `getTopic` 404-to-null path, `createTopic`/`createPost` including the `parentPostId` threading that drives the reply-to-post UI, and `deleteTopic`/`deletePost` status handling. Follows the `FakeDio` pattern already in `major_functionalities_test.dart` — a local stub, since that one implements only `get`/`post` and the forum service also deletes; folding both into one shared fake belongs with 35.B2. No network, no golden. This is a first slice of 35.B3, not a discharge of it.
35.C1 [DONE 2026-08-11] `docs/PLAN.md` described work that had already shipped, which is a trap for anyone opening the plan file first: its "3.7 Discussion Forums" plan showed all six steps unchecked although 3.7 is `[DONE]` and the service, three screens, three routes, and the drawer entry are all present; and its "Full-Stack Review Remediation" phases showed 1.2/1.3/4.6/5.1/5.2/6.1/6.2 unchecked although the matching 29A/29E/29G/29F items are all `[DONE 2026-07-25]`. Added a status banner to each of the two sections pointing at the authoritative area in this file, rather than back-ticking checkboxes whose completion notes live here.
```

</details>
