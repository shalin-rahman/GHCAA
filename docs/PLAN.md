# PLAN.md: 3.7 Discussion Forums and Community Groups (Mobile UI)

> **Status here is historical sequencing, not truth (audited 2026-08-22).** `docs/TODO.md` is the
> single source of truth for what is done. The unchecked boxes in this file's later phases mostly
> reflect work that has since shipped — Area 29 is 100% complete — and were never re-ticked. Two
> checkboxes were corrected in this pass (3.1, 6.2); the rest of Phases 2-5 should be read against
> TODO Area 29 rather than trusted as open. See also `MEMORY.md` → `gotcha_todo_status_drift`.

## Objective
Implement a high-fidelity Discussion Forums and Community Groups module in the Flutter application (`GHCAA.Mobile`) that communicates with the recently finished C# backend endpoints.

## Execution Steps

- [ ] **Step 1: Create Forum Service & Riverpod Providers**
  - Create file `lib/features/forum/forum_service.dart`.
  - Define Dart models: `ForumCategory`, `ForumTopic`, `ForumPost`.
  - Implement `ForumService` using `Dio` (`dioProvider`) mapping:
    - Get Categories: `GET /api/forum/categories`
    - Get Topics by Category: `GET /api/forum/categories/{id}/topics?page={page}&pageSize={pageSize}`
    - Get Topic by ID: `GET /api/forum/topics/{id}`
    - Get Posts by Topic: `GET /api/forum/topics/{id}/posts?page={page}&pageSize={pageSize}`
    - Create Topic: `POST /api/forum/topics`
    - Create Post: `POST /api/forum/topics/{topicId}/posts`
    - Delete Topic: `DELETE /api/forum/topics/{topicId}`
    - Delete Post: `DELETE /api/forum/posts/{postId}`
  - Expose Riverpod providers:
    - `forumServiceProvider` (Provider)
    - `forumCategoriesProvider` (FutureProvider)
    - `forumTopicsProvider(categoryId)` (FutureProvider.family)
    - `topicDetailProvider(topicId)` (FutureProvider.family)
    - `topicPostsProvider(topicId)` (FutureProvider.family)

- [ ] **Step 2: Implement Forum Category List Screen**
  - Create file `lib/screens/member/forum/forum_categories_screen.dart`.
  - Present categories as cards using standard design patterns, displaying name, description, topic count, and reply/post count.
  - Add search/filtering by category name.

- [ ] **Step 3: Implement Forum Topic List Screen**
  - Create file `lib/screens/member/forum/forum_topics_screen.dart`.
  - Show topics within a selected category in a clean, modern list.
  - Display author details, creation date (using `AppUtils.formatDate`), view count, and reply count.
  - Provide a "New Topic" button opening a bottom sheet/dialog to enter Title and Content.

- [ ] **Step 4: Implement Forum Topic Detail Screen**
  - Create file `lib/screens/member/forum/forum_topic_detail_screen.dart`.
  - Display the main topic description/content.
  - List replies in flat-thread style. If `parentPostId != null`, prefix with `↳ replying to [Author]`.
  - Implement a quick reply input at the bottom of the screen.
  - Support "reply directly to post" action which sets the `parentPostId`.
  - Render a delete button for posts/topics if the current user is the author or an Admin/SuperAdmin.

- [ ] **Step 5: Register Mobile Routes and Update App Drawer**
  - Update `lib/core/router/app_router.dart` with routes `/forum`, `/forum/topics/:id`, and `/forum/topic/:id`.
  - Update `lib/core/widgets/app_drawer.dart` to add "Discussions" under the `COMMUNITY` section.

- [ ] **Step 6: Verify and Document**
  - Verify that the app builds and runs without errors.
  - Update `project_map.md` and check off items in `task.md`.
  - Generate the `walkthrough.md` artifact.

---

# PLAN: Full-Stack Review Remediation (2026-07-24)

## Objective
Fix the ship-blockers and high-severity findings from the whole-project review (tracked in `docs/TODO.md` AREA 29). Sequenced so the highest-risk, lowest-effort integrity fixes land first. Each phase ends with the relevant test/type-check gate; a task is only `[DONE]` once its check passes (per Area 12.6).

## Phase 1 — Critical Ship-Blockers (TODO 29-A)
- [ ] **1.1 Web change-password route (29A.1)**
  - Add the `/portal/change-password` route + component (or point the guard at the existing screen if one exists — grep `change-password` first).
  - Verify: a `mustChangePassword` account can log in, land on the form, set a password, and reach the portal.
- [ ] **1.2 Mobile session expiry (29A.2)**
  - In `session_manager.dart`, make `clearSession()` delete the token from `FlutterSecureStorage`, clear Riverpod auth state, and route to login.
  - Verify: an expired/invalidated session actually logs the user out.
- [ ] **1.3 Mobile Digital ID field mapping (29A.3)**
  - Map `data['membershipNumber']` (and confirm batch key) in `digital_id_screen.dart`; QR/barcode must encode the real number, not `PENDING`.
  - Verify against an approved member (see memory: member Id 201 for a matched profile).
- [ ] **1.4 Event capacity enforcement (29A.4)**
  - In `EventService.cs`, count all occupying statuses (not just `Approved`) toward capacity; enforce the cap even when `HasWaitlist=false`; guard the count→insert against concurrent overfill (transaction / row lock / conditional insert).
  - Verify: registrations stop at capacity; concurrent requests can't exceed it.
- [ ] **1.5 Check-in status gate (29A.5)**
  - In `EventService.cs` check-in, reject any ticket not in an approved/checked-in-eligible status; no points for Pending/Rejected/Waitlisted.
- [ ] **Gate:** `dotnet test` (API) + `flutter analyze` + web `ng build` all green.

## Phase 2 — Money & Audit Integrity (TODO 29-F.1, 29-B.2, 29-B.3)
- [ ] **2.1 Admin-ID attribution (29F.1)** — read the acting admin from the JWT `MemberId` claim in every approval/reject path (web sends no adminId; server derives it). Remove all hardcoded/`=1` fallbacks. Backfill note for any already-corrupted audit rows.
- [ ] **2.2 Amount-verification bypass (29B.2)** — drop the `amount > 0` short-circuit; always compare callback amount to the originating `PaymentHistory` amount; reject on mismatch or missing amount.
- [ ] **2.3 Auto-approval scope (29B.3)** — gate membership auto-approval to membership-fee payments only.
- [ ] **Gate:** payment + approval unit/integration tests green.

## Phase 3 — Security Hardening (TODO 29-B)
- [x] **3.1 Facebook token app_id verification (29B.1)** — done; tokens are verified against `graph.facebook.com/debug_token` and the returned `app_id` is checked, mirroring the Google path.
- [ ] **3.2** Remove JWT-in-query-string acceptance or restrict to short-lived signed file URLs (29B.4).
- [ ] **3.3** Re-key login rate limiter to also throttle per-IP regardless of username (29B.5).
- [ ] **3.4** Move signatures under `secure_uploads/` behind auth (29B.6).
- [ ] **3.5** Content-validate `record-payment` receipt uploads (29B.7).
- [ ] **3.6** Confirm/fix SecureFilesController root path (`uploads` vs `secure_uploads`) (29B.8).
- [ ] **Gate:** `security-reviewer` subagent re-audit of touched files.

## Phase 4 — Web & Mobile High-Severity (TODO 29-D, 29-E)
- [ ] **4.1** Article rejection ngModel/signal crash (29D.1).
- [ ] **4.2** Member-approval detail fetch — show full applicant record (29D.2).
- [ ] **4.3** Chat token persistence + reconnect after reload (29D.3).
- [ ] **4.4** Payments page error handling — kill infinite spinner (29D.4).
- [ ] **4.5** Wire real ID-card download (29D.5); fix empty gallery `<img>` (29D.6); unsubscribe layout router events (29D.7); login expiry/returnUrl + profile field retention + governance error handler (29D.8).
- [ ] **4.6** Mobile: wire-or-hide social buttons (29E.1); surface service errors (29E.2); autoDispose notification providers (29E.3); debounce dropdown fetch (29E.4); `kIsWeb` fix (29E.5).
- [ ] **Gate:** web type-check + `flutter analyze` + affected E2E/golden specs.

## Phase 5 — Payment Gaps (TODO 29-G)
- [ ] **5.1** Mobile dues sheet: surface all config-driven methods (bKash/Nagad/Rocket/BankTransfer/ManualReceipt) (29G.1).
- [ ] **5.2** Mobile record-payment: include walletNumber/bankName/accountNumber (29G.2).
- [ ] **5.3** Remove dead options — Stripe tile, NagadGateway dropdown; make CashOnHand creatable if intended (29G.3).
- [ ] **5.4** Guard the gateway factory so webhook returns 4xx (not 500) for unknown gateways (29G.4).

## Phase 6 — Cross-Cutting Cleanup (TODO 29-F.2/3/4)
- [ ] **6.1** Silent-failure sweep — add error handlers to next-only subscribes (web) and error-swallowing try/catch (mobile) (29F.2).
- [x] **6.2** Settle the date contract (29F.3) — **SETTLED, as recommended.** ISO-8601 is the canonical wire format; `dd-MM-yyyy` is display/input only. `DateFormatConverter` and the client parsers were reconciled to that; TODO 23's `(dd-MM-yyyy)` heading is annotated as superseded. The contract is now also **test-pinned** in `GHCAA.Tests/Utils/DateFormatConverterTests.cs` (20 tests), so a regression back to `dd-MM-yyyy` on the write side fails the build — TODO 27.7, done 2026-08-22.
- [ ] **6.3** Finish white-labeling wiring (29F.4) — folds into Area 28 OrgConfigService consumer work.

## Verification standard (all phases)
Per TODO Area 12.6: no task marked `[DONE]` until its test passes. Run the full suite (`dotnet test`, `flutter test`, Playwright) before closing each phase; update `project_map.md` and check off the matching `AREA 29` items.
