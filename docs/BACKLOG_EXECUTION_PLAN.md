# Backlog execution plan (all open, non-ON HOLD items, 2026-09-27)

This is the dependency-ordered plan for every `[TODO]` item in `docs/TODO.md` that is not
`[ONHOLD]`, built from a code-state audit against the current tree (not from the TODO text
alone). It does not repeat what a TODO.md entry already says in full — it adds sequencing,
conflict resolutions, and the reuse/file targets the audit confirmed or corrected. Work each
item by opening its TODO.md entry for the acceptance line, then this plan for how it fits with
the rest of the backlog.

Two sync fixes already applied to TODO.md as part of this plan: 47.13 flipped to `[DONE
2026-09-18]` (all seven sub-items were done, suite green at 841, the parent line was just never
flipped); 28.32/28.33's mobile-vs-Angular i18n cross-links corrected (see Workstream H).

## Sequencing rationale

Three items gate everything downstream of them and should go first, in this order:

1. **84.36 (FluentValidation dead code)** — decides whether new validation anywhere in this
   backlog targets DataAnnotations (the only path that actually runs today) or gets
   FluentValidation properly wired via `AddFluentValidationAutoValidation()`. Every item below
   that adds a DTO or validator (84.11, 84.12, 84.25, 88.1, 90.1) inherits this decision. Resolve
   it before touching any of them.
2. **88.5 vs 90.4 (generic data grid)** — `GHCAA.Web` has no reusable grid/table component today;
   every list screen (`directory.ts`, `news.ts`, `jobs.ts`, `gallery.ts`, `events`) hand-rolls its
   own `.data-table` markup. 88.5 needs an editable grid for import preview; 90.4 needs a
   read-oriented grid for report output and is currently written to reuse `directory.ts` instead.
   Build 88.5's grid first as a genuinely shared `common/data-grid/` component (rows/columns/
   per-cell-errors in, cell-edit/commit events out, a read-only mode for 90.4), then have 90.4
   consume it. Do this before starting 90.4; **do not build 90.4 against `directory.ts` and then
   duplicate the work in 88.5** — that's exactly the duplicate-implementation trap `no_magic_strings_centralize`/`keep_lightweight` exist to catch.
3. **37.7 (Angular i18n) is authoritative over 28.32.** Do not install `ngx-translate`. Build
   37.7's flat-dict + signal + pipe first; closing 28.32 is then a one-line status flip, not
   separate work.

Everything else below is grouped by workstream and ordered within it; workstreams themselves are
mostly independent of each other except where noted.

---

## Workstream A — Validation foundation (do first)

### 84.36 — FluentValidation wiring decision
**What/why:** `GHCAA.Application/DependencyInjection.cs:12` registers validators
(`AddValidatorsFromAssemblyContaining<MemberRegistrationValidator>`) but nothing calls
`AddFluentValidationAutoValidation()` or resolves `IValidator<T>` anywhere in `GHCAA.API` — they
run only inside their own unit tests. DataAnnotations via `[ApiController]` is the only
enforcement path that's actually live.
**Decision, not a rebuild:** don't silently keep both. Either (a) wire FluentValidation in
properly and migrate the DTOs that need it, or (b) delete the dead
`AddValidatorsFromAssemblyContaining` registration and the orphaned validator classes, keeping
DataAnnotations as the one enforcement path. Given every other validation gap in this backlog
(88.1's import rows, 90.1's report definitions, 84.25's `CreateJobDto`) is small enough for
DataAnnotations and the project already leans that way (`fluentvalidation_never_runs` memory,
84.24's completed sibling fix), **(b) is the lower-risk default** unless a specific upcoming DTO
genuinely needs FluentValidation's cross-field/conditional rules that DataAnnotations can't
express — check before choosing (b) blindly.
**Tests:** the two validator test files in `GHCAA.Tests/Validators/` build their validators with
`new` rather than through DI, so dropping the registration alone does not break them. Under
option (b) they go when their validator classes go. Under option (a) they stay, and a
controller-level test proving a bad request 400s is added.
**Depends on:** none. **Blocks:** 84.11, 84.12, 84.25, 88.1, 90.1 (only in the sense that new DTOs
in those items should target whichever mechanism this leaves standing).

---

## Workstream B — Mobile/web API contract bugs (small, independent, do early)

### 84.11 — Assistant query key mismatch
`GHCAA.Mobile/lib/features/assistant/assistant_service.dart:14` posts `{'question': question}`;
`GHCAA.API/Controllers/AssistantController.cs:32-34` binds `AssistantQueryDto.Query`. Every
mobile call 400s today. **Fix on the mobile side** (rename the key to `'query'`) rather than
adding a server-side alias — the server DTO is correct and other consumers may exist.
**Test:** new Flutter test for `assistant_service.dart`, following the existing
`job_update_body_test.dart` pattern (none exists today for this service).
**Depends on:** none.

### 84.12 — Assistant filters silently disabled
`AssistantService.cs` (~lines 40-52) has year/sector `Where` clauses commented out; the query
ignores filters and returns the first 10 members via `Take(10)`. Decide: restore the filters or
remove the dead parameters from `AssistantQueryDto` — don't leave commented-out code in place.
**Test:** `AssistantServiceTests.cs` does not exist — add it regardless of which way this goes.
**Depends on:** 84.36's outcome only if the fix adds new DTO-level validation.

### 84.43 — Error-message field precedence inconsistency
Three-way mismatch confirmed: `api_client.dart:128` reads `detail ?? title ?? message` (correct
ProblemDetails precedence); `api_exception.dart:21` reads the reverse (`message ?? title ??
detail`); `auth_service.dart:153,200` reads only `message ?? error`, ignoring `detail`/`title`
entirely. **Fix:** make `api_exception.dart` and `auth_service.dart` match `api_client.dart`'s
order — `api_client.dart` is correct, don't touch it.
**Test:** extend whatever Flutter test currently covers `api_exception.dart`/`auth_service.dart`
error parsing (audit didn't find one — add if missing, small and mechanical).
**Depends on:** none. Independent of 84.11/84.12.

### 84.10 — Missing step-up on role mutation
`RolesController.CreateRole` (line 82) and `RemoveRole` (line 98) are the only two role-mutating
actions in the controller without `[GHCAA.API.Filters.RequireStepUp]` — `AssignRole` and four
others have it. **Fix:** add the attribute to both actions, same pattern.
**Side effect you must not skip:** `AuthorizationPolicyReflectionTests.cs` (from 84.44) reflects
over every `[RequireStepUp]` action against `evidence/authorization-catalog.md`'s expected table
(321 entries). Adding step-up here requires updating that catalog in the same change or the
reflection test fails.
**Test:** extend `GHCAA.Tests/Controllers/RolesControllerTests.cs` (exists from 47.13.3) — don't
create a new file.
**Depends on:** none. **Related:** 84.42 (mobile has no step-up handling at all — see Workstream C).

---

## Workstream C — Admin-surface gaps and dead code (mostly independent, small)

Do these in priority order (P1 first): 84.42, then P2 items (84.14, 84.29, 84.27, 84.45,
62.42-blocked — skip, see note), then P3/P4.

### 84.42 — Mobile has no step-up flow at all (P1)
Confirmed: zero matches for `STEP_UP_REQUIRED`/`step-up`/`StepUp` anywhere in
`GHCAA.Mobile/lib`. `roles_service.dart:27,62` and `election_service.dart:185` already call
routes the server marks `[RequireStepUp]` (see 84.10) — they currently just fail with no
recovery path. **Reuse:** port the pattern from `GHCAA.Web`'s
`global-http.interceptor.ts:70` into mobile's Dio interceptor stack, alongside the existing
`RetryInterceptor`/auth interceptor (per 8.6). Needs a device-confirmation UI step per the
item's own acceptance criteria — don't skip that to just handle the 403.
**Depends on:** 84.10 landing first makes sense (so the full step-up-protected action set is
final before you build the client handling for it), but not a hard blocker.

### 84.14 — Nagad gateway is fake (P2)
`NagadGateway.InitiatePaymentAsync` (~lines 23-29) always returns
`Success = false, Message = "...coming soon..."`, but `PaymentConfigService.cs:104` still lists
Nagad as selectable (`PaymentMethod.Nagad`, SortOrder 2) — presented to users as working. Two
valid fixes, pick one: build it out against `BasePaymentGateway`/other working gateways as the
template, or hide the method from selection until it's built. **Do not leave it selectable and
broken** — that's the actual bug, independent of which fix you choose.
**Depends on:** none. Loosely related to 84.13 (same payments spec 015) but no hard dependency.

### 84.13 — Dead payment method with no caller (P3)
`FinancialService.cs` (~line 267) has an explicit comment: `ProcessGatewayPaymentAsync` has no
production caller, only exercised by tests, and duplicates `PaymentCallbackOrchestrator`.
**Action:** confirm no caller exists (grep already didn't find one), then remove the method —
but `FinancialServiceTests.cs` currently covers it, so the test removal must not silently drop
coverage of the orchestrator path it duplicates. Verify the orchestrator has equivalent test
coverage before deleting.

### 84.21 / 84.22 — Duplicate family-link workflow, admin bypass gap (P3)
`FamilyService.cs` has a full second request/respond/cancel/link/unlink/search workflow
alongside `FamilyLinkService`; `FamilyLinkController` injects both. Confirm (a grep pass didn't
fully rule out other callers of `FamilyService` beyond `SearchByNameAsync`) whether `FamilyService`
can be deleted or whether it's load-bearing for something `FamilyLinkService` doesn't cover, then
consolidate onto one service — don't maintain two parallel implementations of the same feature.
Separately, `FamilyLinkService.GetFamilyAsync` (~168-188) has the admin-bypass gap confirmed by
its own comment ("assuming requesterMemberId is only passed for public views") — a private
family record currently blocks even admin viewers. Fix: add the admin-role check the comment
flags as missing.
**Depends on:** resolve 84.21 (which service survives) before or alongside 84.22, since the fix
lands in whichever service remains.

### 84.27 — ArchiveController has no tests, no admin UI (P2)
Confirmed: no `ArchiveControllerTests.cs`, no archive-named admin screen (`GHCAA.Web/src/app/admin/`
has audit/campaigns/events/members/polls/themes, no archive). 11 actions need coverage. Build the
admin screen against whichever existing admin CRUD screen is closest in shape (audit or polls) —
don't invent new admin layout conventions.

### 84.28 — Unlimited/orphan-prone news image upload (P3)
The size cap is already there: `NewsController.cs:172` calls
`_fileValidationService.ValidateFormFile(file, FileCategory.Image, 10 * 1024 * 1024)`. What is
left is orphan-file cleanup for images uploaded but never attached to a saved article. Narrow the
TODO.md item to that. Reuse whatever cleanup pattern exists in the gallery photo upload path
rather than inventing a new one.
**Depends on:** none.

### 84.29 — Duplicate stats endpoints, ActivityController untested (P3)
`AdminController.cs:31-44` confirmed: `GET stats` and `GET analytics` both call
`_memberService.GetDashboardStatsAsync` with identical arguments — collapse to one endpoint (or
make the second a thin alias, but don't maintain two independent implementations). Separately,
`ActivityController` (3 routes) has zero tests — add `ActivityControllerTests.cs`.

### 84.45 — Empty-body decision for 401/403/429 (P3)
Role-literal replacement is already done: `GHCAA.API` has 26 `IsInRole(` calls, all passing
`Constants.Roles` values, and zero `IsInRole("` string literals. Remaining piece is a decision,
not code: whether framework-native 401/403/429 responses get a `ProblemDetails` body with a
`code` field, built the same way `ProblemExtensions.cs:26` builds one for
`RequireStepUpAttribute`'s 403. Record the decision in TODO.md's acceptance
line; only write code once decided.

### 62.42 — brand-lint blocking (blocked, do not start)
Confirmed: no `brand-lint` step wired into any `.github/workflows/*.yml` yet, consistent with its
stated dependency chain (62.41 → 62.31's git-history rewrite). Leave alone until that chain
clears — it's outside this backlog's reach on its own.

### 60.2 / 60.3 / 6.2 (low priority, independent)
60.2 is a verification-only task (confirm mobile doesn't duplicate server-enforced EndDate/expiry
checks) — no code change implied unless the audit finds a stale client-side check. 60.3 is
investigative (Messages vs Chats+Forum model parity) — scope it before estimating. 6.2 (alumni
referral system) is a net-new feature with no existing code to reuse; treat as its own
mini-project if picked up, not a quick add.

---

## Workstream D — Bulk import extension (88.1 → 88.5, strict order)

**88.1** extends `MemberImportController`/`MemberImportService.cs` (class at line 19; existing
upsert-by-NID/email and MembershipNumber logic at lines 341/525/566) to also touch
AcademicRecord/ProfessionalRecord/PaymentHistory/User/UserRoles, per spec
`docs/specs/008-idempotent-member-batch-import/spec.md` FR-1–3. **88.2** wraps the extended
import in a single DB transaction (no transaction-scope code exists in the service today).
**88.3** adds the idempotency test (double-run-zero-inserts) and mid-batch-rollback test to the
existing `GHCAA.Tests/Services/MemberImportServiceTests.cs` (13 tests already there covering
insert/dedupe/auto-fill/update — extend, don't duplicate). **88.4** is a real-DB dry run against a
reachable dev/preprod instance seeded from `profiles/ghc/demo-data/` — cannot run in a sandboxed
session; flag for the user to execute or provide DB access. **88.5** builds the generic
`common/data-grid/` component (see Sequencing rationale #2) and a preview/dry-run endpoint,
extending `member-import-modal.component.ts` rather than replacing it.
**Depends on:** 84.36's outcome if 88.1's new DTOs need validation. Strict internal order
88.1 → 88.2 → 88.3 → 88.4; 88.5 depends only on 88.1 (the preview endpoint needs the extended
import logic, not the transaction/test work).

---

## Workstream E — Ad-hoc reporting (90.1 → 90.5, after 88.5's grid)

Read `docs/specs/010-ad-hoc-reporting/spec.md` and `tasks.md` first — they already define more of
this than TODO.md's summary. **90.1** adds `SavedReport` domain model + EF configuration +
migration (nothing exists today — confirmed via grep, only doc references). **90.2**'s
`IReportService` should reuse whatever Excel library `MemberImportService.cs`/
`FileValidationService.cs` already reference — check their usings before adding a new package.
**90.3**'s `AdminReportsController` follows the existing `[Authorize(Roles = "Admin,SuperAdmin")]`
convention from `CampaignsController`/`MentorshipController`. **90.4**'s Angular page: **consume
88.5's `common/data-grid/` component in read-only mode**, not `directory.ts`'s markup as TODO.md's
current text suggests — that text predates the grid decision; update it when you start 90.4.
**90.5** adds `GHCAA.Tests/Services/ReportServiceTests.cs` and a Vitest spec for the report
builder, both FR-tagged per `feedback_fr_nfr_tag_new_tests`.
**Depends on:** 90.1→90.2→90.3→90.4→90.5 strictly; 90.4 additionally depends on 88.5.

---

## Workstream F — Elections content admin (42.1 → 42.5)

**Scope correction:** the election *voting mechanics* admin CRUD already exists in full
(`Election.cs`/`ElectionService.cs`/`AdminElectionsController.cs`/`admin-elections.ts`) — 42.1-42.5
is not about that. It targets the **static rules/procedure/form documents** currently living in
`docs/Elections/*.md`, synced to `public/assets/elections/` by
`GHCAA.Web/scripts/sync-election-docs.mjs`, and manifested by hand in
`GHCAA.Web/src/app/public/elections/election-docs.ts` (id/file/title/blurb/group/isForm) — adding
a document today means editing two files plus running a script. **42.1** (explore/plan, Plan Mode
required) should produce the actual inventory of what needs admin CRUD; that inventory doesn't
exist yet. **42.2** designs the CRUD following the `SiteContent`-style admin pattern (WP34), with
file/PDF attachment handling since forms aren't plain text. **42.3** is the public/portal read
path switching off the static manifest. **42.4** is mobile parity if elections content is
surfaced there (check first whether it currently is). **42.5** is tests + docs, last.
**Depends on:** strict order 42.1→42.2→42.3→42.4→42.5.

---

## Workstream G — Entity feature expansion (spec-gated, sequence before code)

**84.18** is a documentation/audit task (gap table in the spec 001 index, depends on 84.8) —
produces the acceptance criteria 84.19/84.20 need, no code. **84.19** (Meetings module) has
nothing built yet (confirmed: no Meeting/Minutes/AgendaItem/ActionItem model anywhere), reuses
the Events invite/RSVP pattern (spec 014), but **requires an approved spec under `docs/specs/`
before any code** — this is a planning gate, treat it as such. **84.20** (Tasks & notes) may not
become its own module at all — TODO.md defers that decision entirely to 84.19's spec (task/note
as a meeting action item, not a general task manager); don't design a separate task manager until
84.19 rules on this.

Independent feature items in the same spec family (do not block on 84.18-84.20):
- **37.4** (batch cohorts/reunions): confirmed gap — `AcademicRecord.cs:24` has `PassingYear`,
  nothing else; new `BatchCohort`/`Reunion` models, `IBatchService`, `BatchesController`, building
  on `AlumniEvent`/`EventRegistration`/`EventBudget`. No BatchYear column on Member per spec's own
  instruction — don't add one.
- **37.5** (In Memoriam): spec 003 Story 3, reuses `SubmissionStatus` enum + `HtmlSanitizer`,
  depends on `IMemberService` deactivation logic; cross-links to 37.1 (voter roll) — coordinate if
  both are in flight.
- **37.9** (Geographic chapters): spec Story 5, reuses `AlumniEvent` (add nullable `ChapterId`) +
  `INotificationService.CreateNotificationAsync`. Independent of everything else in this
  workstream.
- **37.10** (Annual impact report): spec Story 2, depends on 37.2/37.3/37.4's data existing but
  must degrade gracefully if those features are off; reuses `IDCardService.GenerateIDCardPdfAsync`
  (QuestPDF) and the WP34 `SiteContent`/`.doc-hero`/`.doc-prose` shell.

Small, unrelated client gaps in the same spec cluster, no dependency on the above:
- **84.23**: mobile has no signature-capture UI though `POST api/profile/signature` already
  exists server-side (confirmed: zero signature matches in `GHCAA.Mobile/lib`). Add a mobile
  screen/widget calling the existing endpoint — no server work needed.
- **84.25**: no `MentorshipControllerTests.cs` exists (only a reflection-based policy test hits
  that controller); no working validator for `CreateJobDto` — use DataAnnotations (matches 84.36's
  likely direction and 84.24's completed sibling fix), not FluentValidation.

---

## Workstream H — i18n (do 37.7 first; it resolves 28.32/28.33/8.8)

**37.7** (Angular, P2) is unstarted (confirmed: zero `TranslatePipe`/`i18n.service`/`ngx-translate`
matches in `GHCAA.Web/src`) and is the authoritative spec — build `core/services/i18n.service.ts`
+ `core/pipes/translate.pipe.ts` + flat `en.ts`/`bn.ts` dicts + a language toggle placed beside
`common/theme-toggle/` (same file layout: `.ts`/`.html`/`.scss`/`.spec.ts`). Needs a self-hosted
Bengali webfont under `public/assets/fonts/` (none present today). Once built, **28.32 closes as
superseded** (already annotated in TODO.md).

**8.8** (mobile) already has a working implementation:
`GHCAA.Mobile/lib/core/services/app_localizations.dart` (103 lines) — a flat `en`/`bn` dict +
Riverpod `LanguageNotifier` persisting to `shared_preferences`, exactly the mechanism 37.7 is
building for Angular, just with only ~25 keys covered. **28.33** wants a different mechanism
(`intl`/`.arb`, deps already present but `lib/l10n/` absent). Decision already recorded in
TODO.md: extend `app_localizations.dart` to cover the full string surface rather than migrating to
`.arb` — this mirrors 37.7's approach on the other client and avoids running two i18n mechanisms
side by side. Add a `test/` file for `app_localizations.dart` in the same change (none exists).

**Depends on:** none blocking; do 37.7 before touching 28.32/28.33 so the closing note is
accurate rather than speculative.

---

## Workstream I — Mobile persistence/state (independent of i18n)

**8.7** (Riverpod hydration, P3): `app_localizations.dart:70-90`'s `_loadLanguage()` pattern
(hydrate a provider from `shared_preferences` on init) is the template — generalize it to other
providers (auth, config) rather than inventing a new hydration mechanism. No existing hydration
test file; add one.

**8.5** (Isar/Drift persistence, P4): current persistence is `shared_preferences` +
`flutter_secure_storage` + `lib/core/storage/storage_service.dart` (used by `auth_service.dart`) — no Isar/Drift
dependency exists yet. This is a bigger architectural swap than the others in this workstream;
scope it as its own task (pick one of Isar/Drift, migrate `storage_service.dart` or add a new
`core/storage/` layer alongside it) rather than bundling into 8.7's work.

---

## Workstream J — Test coverage gate (do last, or continuously)

**27.8** (enforce ≥80%/file coverage): confirmed still failing today by a wide margin — backend
59.99%/39.61% (lines/branches), Angular 42.25%/28.44%, Flutter 14.23% lines (baselines from 27.1).
Every workstream above adds tests in the same change per `full_scope_services_tests_docs` — that
narrows the gap incrementally, but 27.8 itself (flipping on a CI threshold) should be the last
step once the rest of this plan's test additions have landed, not a blocking gate on any single
item above.

---

## Workstream K — Dissertation book (docs/book/)

Two items in the originally-scoped book cluster are **not actually book items** — flag and
exclude from book-writing sessions: **72.4** (watching the first Dependabot CI run) and **82.59**
(WP62 white-label brand-lint audit of election markdown). Handle them in Workstream C/whichever
infra pass covers WP62, not here.

**Master sequencing item:** **78.12** is itself the chapter-completion order (11 → 9 →
[78.9/78.10 evidence] → 12, 7, 8, 10, 13 → abstract → consistency pass) — confirm/apply this
ordering before scheduling any other book item below it.

**Claude-executable now, no blockers:**
- **63.8** — re-take the repository figures per `docs/book/README.md` § "Keeping the numbers
  true" (line 286) and update wherever chapters quote them.
- **84.35** — directly caused by the same drift: strict build fails on 7 stale repository counts
  (Ch.4/7/11 quote 299/88 commits/WPs; on 2026-09-27 `git rev-list --count HEAD` gave 308 and `wbs.py --check` gave 91 work packages; the commit count moves daily, so take it again on the day of the fix). Re-source every
  `wbs.py`-quoted figure with today's date, run `build.py --pdf --strict` until clean. Do 63.8 and
  84.35 together — same root cause, same fix pass.
- **64.9** — Chapter 11 needs 17 per-component activity diagrams + 6 chapter charts instead of one
  network diagram (7pt-label-floor reasoning). Diagram-drawing work, no dependency blocking it.
- **74.3** — backfill `wbs.py` date markers for WP1-67 only where real dates diverge from commit
  dates (named case: U1-U5). Narrow, mechanical.
- **78.10** — non-participant evidence: coverage summary from `GHCAA.Tests/TestResults/`, static
  analysis (complexity/coupling/maintainability), latency figures per §4.5.3, ASVS 4.0.3
  walkthrough for §8.13. All command-driven, no human subjects needed — but note 78.9 (below) must
  still complete before the combined evidence write-up into §12.6/12.7 per 78.12's ordering.
- **84.31** — spec-folder numbering clash (two folders both named `010-...`); rename + relink,
  low-risk, touches `docs/specs/` and the 001 index, not book chapters directly.

**Re-verify before starting (dependency looked satisfied but audit found it's partial):**
- **64.8** — depends on 64.7, which the Priority Index marks done, but confirm 64.7 actually
  supplied the impact-cost figures §4.8's RE = P×C revision needs before treating this as
  unblocked.
- **73.5** — depends on 73.4, which is only *partially* done (FR/DC tagging done 2026-09-06, NFR
  tagging explicitly not started per the archive note). The traceability mechanism itself
  (`docs/book/build/traceability.py`, with a `--check` mode) already exists and already
  regenerates Table 3.4 — the real remaining gap is 73.4's NFR tagging plus whatever §3.9/§12.2
  prose needs updating to point at it. Close 73.4's NFR gap first.

**Blocked on chapter-writing progress (not user, not code — sequencing):**
- **67.3 / 67.4** — depend on 67.2, which is only `[PARTIAL]` (Ch.8/11 done; Ch.7,9,10,12,13 still
  had 105 open placeholders as of 2026-09-16, confirmed still zero mermaid blocks in those files).
  Cannot draw the remaining diagrams or resolve the unused reference-list entries until those
  chapters have prose. This is the same blocker as 78.12's ordering — don't schedule 67.3/67.4
  before the chapters ahead of them in that order are written.

**Discretionary / low-priority, do only if the chapter is touched for another reason:**
- **63.10** — a standing per-figure process rule (draw → `--audit` → fix shape →
  `renumber.py --apply`), not a one-shot task; apply it as new Ch.7-13 figures are added.
- **65.5** — Chapter 6 §6.11/§6.12 reordering; the item's own text says only worth doing if the
  chapter is revised for another reason anyway.
- **73.7** — records that two traceability claims have no system counterpart; reads as an
  already-settled "won't build" note more than open work — consider marking it `[DONE]` as a
  documented decision rather than leaving it open.

**User-blocked — do not start without the user:**
- **63.18** (front-matter numbering, discretionary, needs supervisor ask), **72.5** (CodeQL,
  depends on a repo-visibility decision), **75.5** (confirm the 5 PRE-phase duration estimates),
  **76.4** (confirm the 4 reduction factors + 4 production rates), **78.9** (administer the
  evaluation sessions — human fieldwork, Claude can prep instruments/logging but not conduct it),
  **84.39** (proposal doc text is already drafted, explicitly waiting on the author's review —
  do not edit the file until they approve).

Note 75.5/76.4/64.7 form one linked "author must confirm assumption" cluster feeding the same
person-month/sizing figures — ask the user for all three at once rather than three separate asks.

---

Book page budget (added 2026-09-27): build the two tools first, **67.6** `pages.py` (budget
check in `--strict`) and **67.7** `merge_sections.py` (heading merge, renumber, cross-reference
rewrite), then run the **67.5** prose pass. The tools replace work that would otherwise be done by
hand about 24 times.

---

## Workstream L — Configurable approval workflows (92.1 → 92.5, strict order)

Added 2026-09-27. Every approve and reject endpoint is hardcoded to `AdminOnly` and none can be
turned off, so a custom role made in `RolesController` can approve nothing. The flow list and
file references are in TODO.md Work Package 92.

1. **92.1** spec 022. Four decisions need the user before code: where the settings live, what "off"
   means per flow, whether SuperAdmin always approves, and whether member import stays admin-only.
2. **92.2** backend requirement that reads each flow's roles, defaults matching today, audited
   settings endpoint. Touches the same controllers as Workstream A's validation work, so run it
   after 84.36 is settled to avoid two passes over the same endpoints.
3. **92.3** web settings screen and filtered pending-approvals queue; **92.4** mobile gating;
   **92.5** docs and book §8.4, the authorisation model.

Any new approval flow built in Workstream G (scholarships, manual payment verification) should
register with this mechanism from the start rather than add another hardcoded `AdminOnly`.

---

## Workstream M — Election ballot secrecy and standards (37.1i → 37.1q)

Added 2026-09-27 from a review of the election engine. The ballot can be joined back to the voter
four ways, count can run during polling, and the vote screen has no names, review step or receipt.
Spec 023, election ballot secrecy and standards, holds the findings, and its `plan.md` holds the
phases.

1. **37.1i, 37.1j, 37.1k** (P0, backend). Unlinkable ballot, whole-ballot submit with step-up,
   phase guards on count, declare and candidates. Must ship before the next live election. Needs
   one migration, a throwaway-DB dry run, and a `security-reviewer` pass.
2. **37.1l, 37.1m, 37.1n** (P1, web and mobile). Ballot with names, abstain and review; receipt;
   public results. 37.1m waits on decision D1, receipt content. Build the shared ballot component
   from spec 018 ENH-006 here.
3. **37.1o, 37.1p, 37.1q** (P2, rules). Multi-place seats, tie rule, recount, consent, officer
   conflict, filled ER forms. 37.1o waits on decision D2, the tie rule from the constitution.

Independent of the other workstreams. Workstream F (election documents admin, 42.1 to 42.5)
touches different files and can run in parallel.

---

## Not in TODO.md: spec 006 (Agentra AI)

`docs/specs/006-project-agentra-ai/` has a spec and a plan, but nothing is built and TODO.md has
no item for it. It stays out of this plan until the owner decides whether to track it.

---

## Verification checklist per item (applies across every workstream)

- Backend change: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj` green, new controller/service test
  file follows the Moq + `ControllerTestBase.SetUserContext` convention already in
  `GalleryControllerTests.cs`/`DestructiveStepUpActionsTests.cs`.
- Angular change: Vitest spec added/extended alongside the component (`.spec.ts` sibling), no
  Tailwind, no new component library — extend `common/` first.
- Mobile change: Flutter test added following existing service-test conventions
  (`job_update_body_test.dart` as the closest precedent).
- Any new DTO/validator: matches whichever mechanism 84.36 leaves standing — do not add a new
  FluentValidation validator if 84.36 removes the registration.
- Book chapter change: `python docs/book/build/build.py --pdf --strict` ends
  `status : clean, ready to deliver`; run `renumber.py --apply` after any figure/table add-remove-move.
- Any TODO.md item closed: acceptance line records what was delivered and what verified it (a
  clean strict build, a test run, a re-runnable command) — no first-person narration (SR-4).
