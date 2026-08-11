# PLAN.md — phased execution order

This file sequences work; [TODO.md](TODO.md) holds the authoritative status of every item.
Where the two disagree, TODO.md wins. Older plans are kept below as history — each carries a
status banner saying what actually happened.

## Current critical path (reconciled 2026-08-11)

73 items are open across 10 areas. 54 of those are remediation and mostly independent of each
other, so the ordering below is about unblocking verification rather than untangling
dependencies; the other 19 are Area 36, a new feature that is planned separately at the bottom
of this section.

**Start here, because it is blocking two other items and takes an hour.** `34.D17` — the local
database has drifted from the seed files, so `shalin` and `superadmin` log in as ordinary
members and `adminGuard` bounces every `/admin/*` route. Until that is repaired nobody can
finish the live-browser verification in `34.D8` or the admin half of `33.13`.

**Then the three chains that actually have an order to them:**

| Chain | Order | Note |
|---|---|---|
| Auth correctness → its tests | `35.A2` → `35.A3` → `35.B3` | `35.B3` is meant to cover the code `35.A3` changes |
| Social auth | `7.15` → `15.5` → `35.A8` | Decide the feature first; `35.A8` is "drop the SDKs" only if the answer is no |

**The mobile suite is green as of 2026-08-11** — 121 passed, 0 failed, analyze clean. `35.B1`
regenerated the 21 stale goldens and is closed, so the "fix the UI before touching the goldens"
chain no longer applies. The flip side is that the Area 35 UI items (`35.A1`–`35.A9`) now each
carry the cost of regenerating whatever goldens they move, and anyone landing one of them
should expect to re-run `--update-goldens` as part of the change rather than after it.

**Everything else is independent** and can be picked up by anyone in any order: the Area 28
test suite (`28.22`–`28.26`), the Area 27 coverage work (`27.1` gates `27.8`), the Area 12
logging chain (`12.3` → `12.4` → `12.5`), and the long-lived Tier-1 mobile items in Area 8.

**Two corrections from the same audit, worth knowing before you plan around them.** Area 28 is
not the large backlog it appears to be — 12 of its items were already delivered and have been
closed, and what is left is mostly the test suite plus small naming decisions. Conversely,
`15.5` and `7.15` were marked `[DONE]` but never delivered: `29E.1` removed the mobile social
login because it was a stub, and LinkedIn OAuth exists nowhere in the repo.

---

# PLAN: Facebook Page → News/Notice ingest (TODO Area 36)

> **Status: planned, not started.** Raised 2026-08-11. Nineteen items, `36.A1`–`36.D5`.

## The question that was asked

Can posts from a pre-configured Facebook Page be collected, offered to an admin who picks
which ones become News or Notice items, and then rendered through the existing News/Notice
structures — text, image, date and all?

## The answer

Yes, and the reason it is cheap is that almost none of it is new. GHCAA already has one table
for both News and Notices (`NewsPost`, discriminated by a `PostType` enum), an admin editor for
them, file upload with magic-byte validation, HTML sanitization on save, and mobile and web
readers that render whatever `GET /api/news` returns. A Facebook import is a **new front door
onto that existing pipeline**, not a parallel one. What is genuinely new is a Graph API client,
a staging table, and one admin screen.

There is one external dependency the whole thing rests on, and it is not a technical one.
Reading a Page's posts requires `pages_read_engagement` on a Page Access Token, which normally
means Facebook App Review — except when the token belongs to someone who holds a role on that
Page, which an association admin does. **`36.A1` proves that by hand before any code is
written.** If the Page turns out to be agency-managed or the permission needs review, the whole
plan changes shape, and finding that out in an afternoon is much better than finding it out in
sprint three.

## Two design choices worth stating up front

**Fetch is admin-triggered, not scheduled.** The repo has no background-job infrastructure at
all — no Hangfire, no `IHostedService`, no cron — so a periodic poll would mean building that
first, plus retries, failure alerting and a schedule nobody owns. Since the requirement already
puts a human in the loop choosing posts, a "Fetch from Facebook" button gets the same outcome
and skips all of it. A scheduled poll can be added later without reworking anything.

**Fetched posts land in a staging table, never straight into `NewsPosts`.** This is what makes
re-fetching safe (unique index on the Facebook post id), gives an audit trail of what was seen
and ignored, and keeps the public feed clean of anything an admin has not explicitly approved.

## Phases

| Phase | Items | Gate before moving on |
|---|---|---|
| 0 — Decide | `36.A1`–`36.A4` | The token route is proven and the editorial policy is settled. **Do not start Phase 1 until `36.A1` is answered.** |
| 1 — Connect and fetch | `36.B1`–`36.B4`, `36.B7` | A real fetch populates the staging table, re-running it creates no duplicates, and an expired token produces a clear error rather than an empty list |
| 2 — Import | `36.B5`, `36.B6` | An imported post appears on `/news` with its image re-hosted on our own storage and its original publish date intact |
| 3 — Admin UI | `36.C1`–`36.C3` | An admin can do the whole journey without a developer, including recovering from an expired token |
| 4 — Prove and roll out | `36.D1`–`36.D5` | Tests green on API, web and mobile; feature enabled for one admin before anyone else |

## The failure modes this plan is built around

Three things will break this feature in production if they are not handled deliberately, and
each has an item of its own rather than being left as a note.

**The token expires and nobody notices.** Long-lived Page tokens last about 60 days. If expiry
surfaces as an empty result set, the feature simply appears to stop finding new posts, and that
can go unnoticed for a long time. `36.B2` makes the client distinguish expired-token from
no-new-posts, and `36.C3` gives an admin a way to fix it without a deployment. This is also why
`36.A3` leans towards storing the token somewhere a non-developer can rotate it.

**Facebook's image CDN URLs rotate.** Storing a `scontent.*` URL on a `NewsPost` produces an
item that looks perfect on the day it is imported and shows a broken image a few weeks later.
`36.B5` downloads and re-hosts the media at import time, through the existing validation and
storage services. `36.D5` deliberately leaves the pilot posts sitting for a fortnight, because
that is the only cheap way to prove it worked.

**A re-fetch duplicates everything.** Handled by a unique index on `FacebookPostId` in `36.B3`
and an upsert in `36.B4`, and asserted in `36.D1`.

## Things that will surprise whoever picks this up

- Facebook posts **have no title**; `CreateNewsDto.Title` requires 5–300 characters. A title
  has to be derived and then made editable — an import cannot be fully unattended.
- `CreateNewsDto.Content` has a 20-character minimum, so a very short Facebook post cannot be
  imported as-is and needs an explicit, comprehensible refusal.
- `NewsPost.AuthorId` is a non-null FK **and** `NewsPostConfiguration` carries a global query
  filter on `Author != null && !Author.IsArchived`. A synthetic "Facebook" author is therefore
  a trap: archive that user and every imported post silently disappears from the public feed.
  Use the importing admin's user id.
- The admin review UI is **web-only**, and that is deliberate — mobile has no admin news
  management screen at all today. `36.A4` records it so a later parity audit does not reopen
  the area. The member-facing side needs no mobile work whatsoever.

---

# PLAN.md: 3.7 Discussion Forums and Community Groups (Mobile UI)

> **Status: shipped — this plan is history, not work in progress.** TODO 3.7 is `[DONE]`; `lib/features/forum/forum_service.dart`, the three screens under `lib/screens/member/forum/`, the `/forum` routes, and the drawer's "Discussions" entry are all in the tree. The unchecked boxes below were never ticked. Remaining forum work is tracked in TODO Area 35 (35.A6 error handling, 35.B8 service tests).

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

> **Status: executed — see TODO Area 29 for what each phase actually did.** The checkboxes below were never ticked, but 29A/29B/29D/29E/29F/29G are `[DONE 2026-07-25]` with their completion notes. Do not re-open a phase from this page without checking the matching `29*` entry first.

## Objective
Fix the ship-blockers and high-severity findings from the whole-project review (tracked in `docs/TODO.md` AREA 29). Sequenced so the highest-risk, lowest-effort integrity fixes land first. Each phase ends with the relevant test/type-check gate; a task is only `[DONE]` once its check passes (per the standing policy in TODO.md, formerly item 12.6).

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
- [ ] **3.1 Facebook token app_id verification (29B.1)** — mirror the Google debug-token check.
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
- [ ] **6.2** Settle the date contract (29F.3) — decide canonical wire format (recommend: API keeps writing ISO-8601, clients parse ISO on read and only *display*/​*input* dd-MM-yyyy), then reconcile `DateFormatConverter`, client parsers, the `ghcaa-date-standard` skill doc, and Area 23 tasks.
- [ ] **6.3** Finish white-labeling wiring (29F.4) — folds into Area 28 OrgConfigService consumer work.

## Verification standard (all phases)
Per the standing policy at the top of TODO.md (formerly item 12.6): no task marked `[DONE]` until its test passes. Run the full suite (`dotnet test`, `flutter test`, Playwright) before closing each phase; update `project_map.md` and check off the matching `AREA 29` items.

---

# PLAN: Mobile Gap Review & Test Hardening (2026-08-11)

## Objective
Close the mobile gaps found in the 2026-08-11 documentation-vs-source sweep, tracked as `AREA 35` in `docs/TODO.md`. This is `GHCAA.Mobile` only — no API, web, or database change is required by any item here, so the phases can run independently of whatever else is in flight. Every task in Area 35 names its file and line; this page only sequences them and states the gate.

## Known state before starting
Measured on Flutter 3.44.2 / Dart 3.12.2, with 35.B7 and 35.B8 already landed:

| Gate | Current |
|---|---|
| `flutter analyze` | No issues found |
| `flutter test` | 95 passed / 26 failed — all 26 are golden pixel mismatches, nothing else |

Any phase that ends with more than 26 failures, or with a failure that is not a golden mismatch, has broken something.

## Phase 1 — Auth and session correctness (35.A2, 35.A3)
Do these two together; they touch the same logout path and the same providers, and 35.B3 wants a test around it afterwards.
- [ ] **1.1 Admin logout actually logs out (35.A2)** — replace the bare `context.go('/login')` in `admin_dashboard_screen.dart:34` with the drawer's logout path.
- [ ] **1.2 Invalidate cached identity on logout and on expiry (35.A3)** — add `roleProvider` and `userProfileProvider` invalidation next to the existing `notificationHubServiceProvider` one, in both `AuthService.logout()` and `SessionManager._handleSessionExpiry()`.
- [ ] **Gate:** manual account-switch check (admin → logout → member shows no admin nav, no stale profile), plus `flutter analyze`.

## Phase 2 — Correctness bugs visible to members (35.A4, 35.A5, 35.A6, 35.A9)
- [ ] **2.1 Digital ID `PENDING` codes (35.A4)** — card and PDF export.
- [ ] **2.2 Debounce directory search (35.A5)** — prefer adopting `AppSearchField` over hand-rolling a second debounce.
- [ ] **2.3 Surface the swallowed errors (35.A6)** — forum reads/writes, `recordPayment`, `LookupService.getByGroup`.
- [ ] **2.4 Strict date parsing (35.A9)** — `AppUtils.formatDate` currently renders an invalid date as a plausible wrong one. Fixing it also means updating the expectation in `test/unit_test.dart` that pins the rollover.
- [ ] **Gate:** `flutter analyze` + `flutter test` (35.B8's forum service tests cover 2.3's forum half).

## Phase 3 — Configuration and dead weight (35.A1, 35.A7, 35.A8)
Independent of Phases 1–2; can run in parallel by a second person.
- [ ] **3.1 Hardcoded gateway `baseUrl` (35.A7)** — source from `AppConfig`.
- [ ] **3.2 Resolve the two orphan screens (35.A1)** — route, merge, or delete; no duplicates left behind.
- [ ] **3.3 Decide the social-auth SDKs (35.A8)** — wire under 7.15 or remove the packages and the uncalled service methods. Removing them changes the Android/iOS build config, so rebuild both platforms, not just `flutter test`.
- [ ] **Gate:** `flutter analyze` + a real `flutter build apk --debug` if 3.3 removed packages.

## Phase 4 — Test suite (35.B1–35.B6)
Run **35.B1 last within this phase**, after Phases 1–3 have landed, so the regenerated baselines capture the fixed UI instead of needing a second regeneration.
- [ ] **4.1 Shared test harness (35.B2)** — extract `test/helpers/`; the five visual files stop carrying private copies of the same fakes.
- [ ] **4.2 Fill the highest-risk coverage gaps (35.B3)** — `api_client` (JWT refresh, 31.5) and `session_manager` first, since Phase 1 changes them.
- [ ] **4.3 Integration suite honesty (35.B4, 35.B5)** — CI job or documented-manual, and make the DGePay test assert instead of skip.
- [ ] **4.4 Golden tag (35.B6)**.
- [ ] **4.5 Regenerate the 26 stale goldens (35.B1)** — review each image before committing; a 0.26 % dashboard diff and a 32.61 % register diff need different scrutiny.
- [ ] **Gate:** `flutter test` fully green (0 failures) — this is the phase that finally makes that possible.

## Phase 5 — Housekeeping (35.C2, 35.C3)
- [ ] **5.1** Delete the nine `test_output_*.txt` logs and `rows.txt`; add the pattern to `.gitignore` (35.C2).
- [ ] **5.2** Strip the stale counts from `.claude/memory/outstanding_todos.md` and `MEMORY.md` (35.C3).

## Verification standard (all phases)
Per the standing policy at the top of TODO.md (formerly item 12.6), nothing is `[DONE]` until its test passes. `flutter analyze` must stay at *No issues found* throughout — it is clean today, so any new warning is yours. Update `docs/project_map.md` when a screen, route, or test file is added or removed, per its Section 24 protocol.
