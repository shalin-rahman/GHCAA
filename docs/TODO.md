# GHCAA PLATFORM TASK TRACKER

## STANDING RULES — apply to everything, not to a phase

These are not tasks and never close. They bind every change from the date given onwards, and they are
deliberately **outside the work breakdown and outside the requirement set**: they are not WBS
activities, they carry no component, they are not FR or NFR entries, and `wbs.py` does not count them.
A rule that applies to all work cannot also be a piece of work without double-counting it.

**SR-1 — Nothing may read as machine-written.** Set 2026-09-03 by the user, applying from now on and
retroactively to any file a change touches. It covers everything the project produces: code comments
and doc-comments in C#, TypeScript, Dart and SQL; markdown docs; the dissertation in `docs/book/`;
commit messages; TODO entries; and user-facing strings in the applications. Plain, short sentences.
No filler openers, no restating what the code already says, no praise of the work, no em-dash-and-tricolon
cadence, no heading-banner comments. A comment earns its place by explaining a gotcha, a reason, or an
assumption a caller has to know. Where a file already has a comment style, match it rather than
imposing this one on lines nobody touched.

Restated and widened 2026-09-03: write everything in plain, simple words. This is not only about
avoiding the words a model reaches for. It rules out the elegant sentence as well: the clever
construction, the abstract noun where a verb would do, the phrase that sounds considered but takes
two readings. If a shorter, more ordinary sentence says the same thing, it is the correct sentence.
A reader should never have to work out what a line means before they can act on it.

This holds for the dissertation too, where a plain sentence is not a lesser academic register but a
clearer one. It holds for the instruments in `docs/book/instruments/`, which somebody has to follow
while running a session, and for anything else the project produces.

The rule is enforced where it can be and by attention where it cannot. `docs/book/build/lint.py`
carries the banned-vocabulary list and fails `build.py --strict` for the dissertation; the root
`CLAUDE.md` states the same rule for code and docs, and it is checked by review. If a check ever has
to be silenced to get a green run, the text is what changes, not the check.

Two carve-outs, because the rule would make them worse rather than better. Requirement statements
keep their "The system shall …" form, which is ISO/IEC/IEEE 29148 and is meant to be uniform and dull.
Generated tables — the WBS, the activity list, the traceability matrix — keep whatever shape the
script produces, since their value is that a machine wrote them from evidence.

**SR-3 — This file is the only task list.** Set 2026-09-03. Every actionable item lives here, in a
numbered area, with a status, a priority and its dependencies. Nowhere else: not a parallel list in
memory, not a checklist inside a design document, not a "next steps" section at the end of a report,
not a TODO comment in code standing in for a tracked item. A second list is worse than no list,
because the two disagree and nobody knows which is current.

Two consequences follow. Anything an item quotes from elsewhere is a pointer, not a copy. And an item
records what is true, including where the work was wrong: an entry that reads better than what
happened is a defect in the record, and the record is the only project-management evidence this
project has.

**SR-4 — Entries are written by an analyst for a developer.** Set 2026-09-03. Every item, open or
closed, reads as an analyst, architect or QA engineer specifying work for a developer to carry out.
It never reads as somebody's private note to self, and it never narrates whoever wrote it. No first
person, no reasoning aloud, no self-assessment.

An open item states the work required and carries an **Acceptance:** line saying how completion will
be judged, which is what makes it an assignment rather than an intention. A closed item records what
was delivered and what verified it, being a clean strict build, a test, or a command that can be
re-run. Findings are raised rather than confessed: a defect is raised against a section, its impact is
stated, and work ruled out is recorded as out of scope with the reason.

The unit of work is a **work package**, not an "area". The word was changed on 2026-09-03 because a
work package is the standard breakdown unit, which is what `docs/book/build/wbs.py` computes from
these headings and what Chapter 11 calls them. Both heading forms parse while the rename runs.
Work Package 78 is the pattern to follow.

**SR-5 — A cross-reference carries its subject, not only its number.** Set 2026-09-03. Applies to the
dissertation, the outline and every document in `docs/`. Write "§12.8, where the research questions
are answered", not "§12.8". The number stays, so the reader can still turn to it; the phrase is there
so that they do not have to. One clause is enough, and it names what is at the destination rather than
repeating what the sentence already said.

The reason is that a reference the reader has to resolve is a reference most readers skip, and a
dissertation that only makes sense to somebody willing to page back and forth does not survive an
examiner reading it once, straight through. The same rule covers a reference to a figure, a table, a
requirement identifier or a domain constraint.

**SR-2 — Every area carries its schedule facts inline.** Set 2026-09-03. Each area heading is
followed by a metadata line so that the activity list, the durations and the dependency network can be
generated instead of reconstructed:

```
<!-- wbs: component=C17 start=2026-09-02 end=2026-09-03 after=64,65 -->
```

`component` is the code component from the `CODE` table in `docs/book/build/wbs.py`; `start` and `end`
are ISO dates; `after` is a comma-separated list of area numbers this one depended on. Omit `start`
and `end` where commits carry the dates already — `wbs.py` reads git for those and only needs the
stated dates for work that left no commit, which is the fourth evidence class of Chapter 11. Omit
`after` where nothing blocked the area. `wbs.py --check` fails on an area with no component, from
either this line or the `CODE` table, so the mapping cannot silently rot. Adding the marker by
hand is optional: `python docs/book/build/wbs.py --sync` writes one for every area that has none,
taking the dates from the area's own `[DONE]` stamps and the component from the source paths it
names, and `--sync --dry-run` shows what it would write first.

**SR-6 — A test change may not reduce coverage, and a new test is checked against existing coverage
before it is written.** Set 2026-09-04 by the user, applying to every test suite this project has:
`GHCAA.Tests` (NUnit), `GHCAA.Web`'s spec files (vitest), and `GHCAA.Mobile/test` (Flutter). Before
adding a test, check whether an existing test already exercises the same setup and assertion shape.
Where one does, extend or parameterize it (`[TestCase]` in NUnit, `it.each` in vitest, a table-driven
loop or shared `group()`/fixture in Dart) rather than pasting a near-copy beside it. Where the existing
suite already has a shared helper for the setup in question, use it; where several tests independently
reimplement the same setup and none of them is the natural shared helper yet, that duplication is
itself a defect worth fixing in the same change, not a pattern to add a fourth copy of. Write a new,
separate test only when no existing test covers the case.

The other half of the rule is the one that bounds the first: consolidating tests must never cost a
real assertion. Two tests that look alike but exercise a different branch, status value, or failure
path are not duplicates and stay separate. A change that merges tests runs the full suite before and
after and reports both counts; a drop in count with no corresponding branch actually removed is a
defect in the change, not a cleanup. A pair that looks redundant but the change's author is not certain
about is flagged in place, left untouched, and named to the user — never silently deleted. The
2026-09-04 audits of all three suites (`GHCAA.Tests` 532→532, `GHCAA.Web` 381→383, `GHCAA.Mobile`
93→93, all held at zero net coverage loss with several genuine duplicates removed) are the worked
example of what this rule asks for.

**SR-7 — A new item is checked against the existing tracker before it is opened.** Set 2026-09-04 by
the user, generalising the reconciliation process Work Package 82 ran once against `docs/materials/
REVIEW.md` into a standing rule for every future addition to this file, not only a review's findings.
Before adding a new numbered item, search this file for one that already covers the same problem —
compared by root cause, affected component and intended outcome, not by title wording, since
"centralise X" and "remove duplicate X handling" can be the same underlying work said two ways. Where
a match exists: reuse it as-is if it fully covers the finding, expand it if the finding adds a genuinely
new sub-scope, or mark it superseded with a reason if the finding shows it is no longer the right
approach. Open a new item only when no existing one covers the ground, and say in the new item what was
checked and why it did not already exist there — the "Reconciliation done before writing these items"
paragraph at the top of Work Package 82 is the pattern to follow. This does not relax SR-3: there is
still exactly one list, and this rule is about not padding it with near-duplicate entries.

**SR-8 — A refactoring carries its justification, not just its description.** Set 2026-09-04 by the
user, applying to every refactoring this project has already done and every one it has yet to do.
A tracker entry, commit message or report row that says only *what* was moved, split, merged or
renamed is incomplete. It must also say **why**: the concrete duplication, coupling, defect or cost
that made the change worth doing, and what is different afterwards. "Split `MemberService`" is a
description; "split `MemberService` because one 1,577-line file holds the registry, the approval
workflow, profile updates and search, so any change to one of them re-reads and re-tests all four"
is a justification.

The reason is that a refactoring with no recorded cause cannot be reviewed, cannot be argued against,
and cannot be undone safely by anyone who was not in the room — it reads as taste. It also guards
against the failure REVIEW.md §4 and §25 both warn about: a change made because a pattern is
textbook-correct rather than because a real problem demanded it. If the justification cannot be
written down, that is evidence the refactoring should not be done.

This is retroactive in the same limited sense SR-1 is: when a past refactoring is touched or cited,
its justification gets written down then, rather than triggering a sweep of the whole history. It
applies equally to the refactorings named in `docs/ARCHITECTURE_AUDIT_2026-09.md`, which is why every
row in that report's refactoring backlog carries a problem statement and an evidence line rather than
a recommendation on its own.

**SR-9 — `[ONHOLD]` marks an item blocked on something outside this session, not just unstarted work.**
Set 2026-09-06 by the user. `[TODO]` means the next available session can pick the item up and finish
it; `[ONHOLD]` means it cannot move without an action only the user (or the Association) can take —
rotating a live credential, deciding a data-protection policy, granting dashboard access — so leaving
it `[TODO]` would misrepresent it as ready work sitting in a queue. An `[ONHOLD]` item still carries its
priority and depends-on tags and still counts as open, but `wbs.py`'s remaining-effort estimate excludes
it (a P0 blocked on the user rotating a key is not 4 hours of anyone's engineering time), and
`tracker.html` renders it like any other open state. Move an item back to `[TODO]` the moment the
blocker clears; do not leave it `[ONHOLD]` out of habit once it is actionable again.

---

## PRIORITY INDEX (triaged 2026-08-30 — re-triage when this drifts, don't trust it blind per `gotcha_todo_status_drift`)

Every open `[TODO]` item as of this date, grouped by severity/urgency. This index is a pointer, not a
duplicate — the full item text with context stays at its Area location; update both when an item's
status changes.

### DONE 2026-09-15 — Chapter 11 duration assumptions (64.7), kept for the arithmetic record

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

**Confirmed 2026-09-15, by the author:**
- **U1** — no extra prep or write-up time beyond the interview contact time itself; the 1x write-up
  ratio stands as assumed.
- **U3** — one formal review session happened, 3 July 2026; there was no second formal session. The
  three files in `docs/materials/IMPLEMENTATION_REVIEW_1.md`–`_3.md` are additional evidence of the
  same relevance cycle at finer grain (see §4.2), not a second formal review — U3 stays at one session.
- **U4** — the 18 stakeholder exchanges were a genuine mix, roughly half held as meetings/calls and
  half as written messages; the 30-min-per-area rate is kept as a blended average across both forms.

U2's reading rate is still open to challenge. U5 is deliberately not added to the total: those four
incident dates carry 9, 7, 10 and 2 commits, so the fix work is already inside the measured days and
counting it twice would inflate the figure.

64.8 unblocks now that U1/U3/U4 are confirmed and U2 is the only open rate, since risk exposure
RE = P × C in §4.8 needs an impact cost per risk on the same basis.

### P0 — CRITICAL (blocked on the user; cannot be closed from a coding session)
- **48.2** [WONTDO 2026-09-18, user decision] (merged 2026-09-15 with 47.10's credentials half and
  48.13) — Live production secrets committed to git (JWT signing key, DB passwords, Gmail app
  password, Render deploy-hook URL) in `docs/deploy_connection.txt`/`docs/deploy_conn_Info.txt`,
  `.env.remote`, `build_output/appsettings*.json`, `docs/RENDER_DEPLOYMENT.md`. Separately, the live
  SuperAdmin password sat in git history (`docs/BUSINESS_REVIEW_PLAN.md`) since before it was even
  set live — doc text is redacted, but the password itself was never independently rotated. Would
  have needed the user to rotate every credential via the relevant dashboards, then `git rm --cached`
  + `.gitignore` (done) + a history purge (`git filter-repo`). User decided against the rotation/purge;
  closed without remediation. No coding-session action can reopen this without a fresh user decision.
- **82.31 / 62.31** — 631 real alumni records, including all 631 password hashes, are literal
  `InsertData` values in eight committed migrations, so a clean clone of this repo builds a database
  full of real personal data. Editing `Seed/members.json` does not reach it, and no environment
  variable turns it off. Needs a decision on the history purge and a forced password reset before any
  code moves. `docs/SEED_CLASSIFICATION.md` has the per-file breakdown.

### P1 — HIGH (security surface / explicitly time-sensitive / blocking other work)
- **88.1–88.5** — The May 2026 alumni batch's delivery mechanism (a dedicated EF migration) no longer
  exists in the tree after the WP62 seed refactor; `InstitutionDataSeeder`'s empty-table-only model
  can't deliver it to a live DB either. Extending the existing bulk-import feature into an idempotent,
  atomic, PaymentHistory-covering path instead, plus a preview grid so bad rows get caught before
  commit, per `docs/specs/008-idempotent-member-batch-import/`.
  Not started; 88.4 needs a reachable dev/preprod database that isn't available from a coding session.
- **47.13.3–47.13.7** — Mutation-coverage remediation, remaining after 47.13.1/47.13.2 closed
  2026-09-04 (`AuthController` non-Login actions incl. the step-up endpoints, 20 tests; `LookupsController`
  full CRUD, 12 tests; backend suite 532→564, zero regressions).
- **48.18a** — 48.18/48.19 (dependency/CI supply-chain hardening) done 2026-09-04: packages bumped
  to latest `9.0.x`, CI Actions pinned to commit SHA, Docker base images pinned by digest, NuGet
  lockfiles added with `--locked-mode` restore in CI. What remains: a live-Postgres migration-apply
  check the bump couldn't get in this session (no reachable Docker daemon).
- **49.1(a) done 2026-09-05 / 49.2** — Custom-role UI no longer implies real access: relabeled, hinted,
  and the "Create System Administrator" role select restricted to Admin/SuperAdmin only. 49.1(b) (a
  real permission system) stays a fully-specified plan, not started — needs a confirmed concrete need
  first. 49.2 (system-admin disable/enable) has its own "DECISION NEEDED" gate, untouched. **49.3 done
  2026-09-04**: admin-initiated reset shipped for both members (a real token-revocation gap closed) and
  system admins (new), including the auth-flow fix the original plan missed (system admins have no
  email to look the reset up by).
- **46.5** — Org-wide Financial Ledger has zero rows post-import; aggregate income/expense view doesn't
  reflect the ~৳47,000 in per-member fees that ARE recorded correctly.
- **82.16 done 2026-09-04** — Financial ledger and payment rows now record who changed them and when,
  and are soft-deleted rather than removed. The rule for which entity classes need audit fields is
  `docs/ARCHITECTURE.md` §4, which splits entities into Class A (evidence) and Class B (recreatable
  content). Left two named gaps open as 82.29 (`ECMember` has two removal semantics) and 82.30
  (`Member`/`User` use `IsArchived` where the rule says `IsDeleted`), both below P1.
- **82.32 done 2026-09-05** — Full bug sweep of the financial/payment/event stack plus a client-side
  pass: 22 confirmed defects fixed (guest-payment crash, a casing bug letting event fees auto-induct
  members, five silently-emptied admin event screens, CSV formula injection, a poll double-vote race,
  a UTC-shift bug corrupting every admin event-date edit, silently-discarded participant limits, two
  broken receipt downloads, wrong dues on mobile, dead news links, wrong admin page titles). One item
  deliberately deferred as 82.33 (mobile receipt download needs a package decision, not a quick fix).
  All three test suites green throughout.
- **82.14** — The same audit did not cover the Angular or Flutter clients (two research streams
  returned nothing). `docs/ARCHITECTURE_AUDIT_2026-09.md` is a backend review until this closes.
  82.32's client bug sweep is not a substitute — see its note there.
- **84.1–84.3** — State-transition tables, a validation matrix, and endpoint contract tables, scoped
  from an external review of the 001 specification baseline. Drafted in
  `docs/specs/002-workflow-contracts-and-validation/`.

### P2 — MEDIUM (real, no urgency signal)
- **42.1–42.5** — Admin-manageable elections forms/docs, plan only.
- **51.4–51.5** — Remaining file-storage hardening: broader regression coverage and admin-configurable
  settings.
  tests, compression settings not admin-configurable yet.
- **47.10** — Missing profile photos for most of the 631 bulk-imported alumni (data gap, not a bug).
- **27.8** — No enforced ≥80%/file coverage threshold (would fail today if enforced).
- **28.32 / 28.33 / 8.8** — i18n (English+Bengali): dependencies present, extraction not started.
- **34.D7** — 21 stale mobile golden baselines need regenerating (unrelated housekeeping).
- **44.16** — Three unrelated Flutter classes all named `FamilyService` (deferred rename risk).
- **7.16, 8.3–8.7** — Mobile hardening/perf backlog (SSL pinning, pagination, background threading, etc.).
- **12.1–12.6** — Process items (API-change checklist, contract registry, mobile log capture).
- **52.5** — Mobile's separate "quick create gallery" dialog — left as-is, a future cleanup decision.
- **82.3 / 82.4 / 82.5 / 82.7 / 82.8 / 82.10a** — Cross-cutting engineering gaps measured 2026-09-04: no
  API versioning against an independently shipped mobile client; three error-response shapes; 55 copies
  of claim parsing in the controllers; seven Angular components bypassing the service layer; paging
  applied to only a fraction of the list endpoints; and no published API contract to diff, the OpenAPI
  document being served in Development only.

### P3 — LOW / PLAN-ONLY (large unbuilt features, no current pressure)
- **Work Package 37** (37.2–37.10) — scholarships, fundraising, cohorts/reunions, oral-history archive,
  bilingual UI, credential verification, geographic chapters, annual impact report.
- **6.2** — Alumni referral system for jobs/internships.
- **82.6 / 82.9 / 82.10b / 82.11–82.13** — Audit follow-through: split `MemberService` (1,577 lines),
  request correlation and structured logging, typed Dart models for the auth, profile and payment
  payloads, the configuration and constants classifications, and the missing decision records and
  recovery runbook. (82.10, whether to generate clients from OpenAPI, is decided and closed: rejected.)
- **61.3** — Drop the `Summary:`-style comment banner in `GHCAA.Tools/db_diag.cs` next time that file is touched.
- **84.4 / 84.5** — Web/Mobile client parity table and the authorization-policy/error-code catalogs;
  both depend on 84.3 existing first.
- **84.6 / 84.7** — Election ballot workflow scope and white-label second-institution deployability —
  product decisions for the project owner, not documentation tasks.


<!-- Closed Work Packages (1, 2, 3, 4, 5, 9, 10, 11, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 29, 30, 31, 32, 33, 35, 36, 38, 39, 41, 44, 45, 46, 50, 51, 53, 54, 55, 56, 57, 58, 59, 61, 66, 68, 69, 70, 71, 80) moved to docs/TODO_ARCHIVE.md on 2026-09-15 to keep this file lean. wbs.py reads both files for tracker counts. -->

## WORK PACKAGE 6: CAREER & OPPORTUNITIES

6.1  [DONE] Professional Hub: Alumni directory LinkedIn-style filters
6.2 [TODO] **Priority: P3.** Alumni referral system for jobs and internships

## WORK PACKAGE 7: SECURITY, INFRASTRUCTURE & HARDENING

7.1  [DONE] API: Fix Member Login with NID (401 resolution)
7.2  [DONE] API: Fix Password Reset timeout logic
7.3  [DONE] API: Real-time Session Termination upon status change
7.4  [DONE] API: Secure Admin routes with SuperAdmin functional guards
7.5  [DONE] API: Event Note data leakage fix (null out sensitive notes in public lists)
7.6  [DONE] Web: Profile rendering pascal/camel case mapping stability
7.7  [DONE] Web: Image path prefixing sync across all components
7.8  [DONE] Web: Event Date validation (StartDate < EndDate enforcement)
7.9  [DONE] Web: Readonly field protection in Profile update payloads
7.16 [IN-PROGRESS] **Priority: P3.** Hardening: SSL Pinning and Binary Obfuscation. Obfuscation is done: `mobile_deployment.yml`'s Android and iOS release build steps now pass `--obfuscate --split-debug-info=build/debug-info`, and that directory is uploaded as a separate `android-debug-info`/`ios-debug-info` artifact (90-day retention) so a crash report from a given release build can still be symbolicated later. SSL pinning is scaffolded, not enforced: `GHCAA.Mobile/lib/core/api/ssl_pinning.dart` gates pinning on `AppConfig.environment == 'production'` (dev/preprod are never affected) and only enforces it once `kProductionCertificateSha1Pins` holds a real value — `ssl_pinning_io.dart` wires the check into Dio via a `SecurityContext(withTrustedRoots: false)` + `badCertificateCallback`, `ssl_pinning_stub.dart` is the web no-op, `api_client.dart`'s `dioProvider` calls it. `kProductionCertificateSha1Pins` is still empty: there is no `.env.production` in this repo (only `.env` for dev and `.env.preprod`) and no way from this environment to fetch or confirm the real production certificate, so pinning stays a documented no-op rather than risk pinning a guessed value and locking out every user until an app update. Remaining step for someone with access to the live production host: run the `openssl s_client`/`openssl x509 -fingerprint` command documented at the top of `ssl_pinning.dart`, add the fingerprint to `kProductionCertificateSha1Pins`, and update `ssl_pinning_test.dart`'s placeholder-state test to assert `isPinningEnforceable('production')` is true. VERIFIED 2026-09-17: `flutter test test/ssl_pinning_test.dart test/mobile_deployment_obfuscation_test.dart` (9 tests: environment gating, enforceability-while-placeholder, fingerprint hex encoding, and a grep-assertion that both CI release build lines carry `--obfuscate`/`--split-debug-info`) and `flutter analyze` on all touched files, both clean.

## WORK PACKAGE 8: MOBILE ENGINEERING (TIER-1 STANDARDS)

8.1  [DONE] UI: Enforce 8pt grid and standard design tokens globally
8.2  [DONE] Nav: Adaptive layout for Tablets/Pads (Sidebar architecture)
8.3 [DONE 2026-09-17] **Priority: P2.** Perf: Cursor-based pagination for Alumni Registry. The Alumni Registry is `directory_screen.dart` (Member Directory), backed by `NetworkingController.Search()` → `NetworkingService.SearchMembersAsync()`. Replaced the offset Skip/Take page with keyset pagination on `(FullName, Id)`: `MemberSearchFilterDto.Cursor` and `PagedResult<T>.NextCursor` already existed as unused stub fields, so no new DTO was needed. A `cursor` query param (base64 of `{FullName, Id}`) replaces `page` for callers that send one; the service fetches `PageSize + 1` rows past the cursor to derive `hasMore`/`NextCursor` without trusting a possibly-stale `TotalItems`. An invalid or stale cursor falls back to page 1 instead of throwing. `professional_hub_screen.dart` shares the same endpoint and was left on legacy page-number pagination on purpose — the `else if (!cursorRequested)` branch in `NetworkingService.cs` keeps Skip/Take behavior for it, so it needed no change. `directory_screen.dart` and `networking_service.dart` (mobile) were updated to track `_nextCursor` instead of `_pageNumber` for infinite scroll, with no UI/scroll behavior change. VERIFIED 2026-09-17: `dotnet build GHCAA.sln` (0 warnings, 0 errors), `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~NetworkingServiceTests|FullyQualifiedName~NetworkingControllerTests"` (13 tests passed, including 3 new: happy-path multi-page walk, empty result, invalid-cursor fallback), and `flutter analyze lib/screens/member/directory_screen.dart lib/features/networking/networking_service.dart` (no issues found).
8.4 [DONE 2026-09-17] **Priority: P3.** Perf: Isolated background threading for JSON/Encryption processing. Audited the mobile app: there's no client-side encryption work (flutter_secure_storage does its encryption natively, outside Dart CPU) and the app's own explicit jsonDecode/jsonEncode calls (storage_service.dart profile cache, org_config_service.dart config cache) are single small objects, not worth isolating. The real hot spot was structural: Dio's default SyncTransformer decodes every response body with a synchronous jsonDecode() on the main isolate, including large list responses (alumni directory, events, gallery, financial ledger). Wired dio.transformer in api_client.dart to a SyncTransformer using a new decodeJsonInBackground callback (lib/core/utils/background_json.dart) that hands payloads at or above 50KB to a background isolate via compute(), leaving small responses decoded inline to avoid isolate hand-off overhead on them. VERIFIED 2026-09-17: `flutter test test/background_json_test.dart` (3 tests: small payload, 2000-row large-list payload, empty object, all matching jsonDecode's output) and `flutter analyze lib/core/utils/background_json.dart lib/core/api/api_client.dart test/background_json_test.dart`, both clean.
8.5 [TODO] **Priority: P4.** Persistence: Switch to High-Performance Local DB (Isar/Drift)
8.6 [DONE 2026-09-17] **Priority: P2.** Networking: Exponential backoff and connectivity banners. Added `RetryInterceptor` (lib/core/api/retry_interceptor.dart), wired into `dioProvider` after the existing auth/friendly-message interceptor so its onError runs first on the raw exception. Retries timeouts, connection errors, and 5xx responses only — never 4xx or auth failures — capped at 3 attempts with exponential backoff (500ms base, doubling) plus up to 150ms jitter, refetching through the same Dio instance so the auth token header is reattached on each retry. The connectivity banner (`NoInternetBanner`/`ConnectivityAwareWrapper` in lib/core/widgets/no_internet_banner.dart, backed by `connectivity_service.dart`'s `isOnlineProvider`) was already implemented and wired into `main.dart`'s `MaterialApp.router` builder — it just had no test coverage, added here. No new dependency: `connectivity_plus` was already in pubspec.yaml. VERIFIED 2026-09-17: `flutter test test/retry_interceptor_test.dart test/no_internet_banner_test.dart` (8 tests: retry on 5xx/timeout/connection-error, no retry on 4xx, gives up after max attempts, banner hidden/shown/reacts-to-stream-changes) and `flutter analyze lib/core/api/retry_interceptor.dart lib/core/api/api_client.dart test/retry_interceptor_test.dart test/no_internet_banner_test.dart`, both clean. Also re-ran `flutter test test/auth_service_test.dart` to confirm the new interceptor doesn't disturb the existing 401 refresh flow — unaffected.
8.7 [TODO] **Priority: P3.** State: Riverpod State Hydration (Local local persistence)
8.8 [TODO] **Priority: P2.** i18n: Unified Localization (English + Bengali)
8.9  [DONE] CI/CD: Fastlane + GitHub Actions Deployment Pipeline
## WORK PACKAGE 12: PROCESS & ENGINEERING STANDARDS

12.1 [DONE 2026-09-21] **Priority: P2.** PROCESS: On every API endpoint change, add verification checklist task for Web + Mobile parity. Found already in place while auditing this item for staleness, not newly built: `.github/copilot-instructions.md`'s opening paragraph makes "changes to a shared API contract... must be checked against both clients" a standing rule read before any cross-layer change; `docs/API_CONTRACT_REGISTRY.md` (12.2) requires a `Web:`/`Mobile:` line on every logged endpoint change, which is the per-change parity checklist this item asked for; and `.github/PULL_REQUEST_TEMPLATE.md`'s Verification Checklist already carries "Visual regression checked on both Mobile and Web." Together these three cover the process end to end — no new file was added. Process/documentation item, not testable code — no test run to cite.
12.2 [DONE 2026-09-17] **Priority: P3.** PROCESS: Implement API Contract Registry (changelog of all endpoint changes + which clients updated). Created `docs/API_CONTRACT_REGISTRY.md` with the current endpoint surface (pointing at `API_ENDPOINTS` in `app.constants.ts` rather than duplicating it), an entry format (date, change, reason, Web/Mobile update status), and a first log entry dated today for the registry's own creation — no fabricated historical entries. Cross-referenced from the "Add a new API endpoint" row in `docs/PROJECT_MAP.md`'s Impact Guide and from `.github/copilot-instructions.md`'s opening paragraph, so the next endpoint change finds it. Process/documentation item, not testable code — no test run to cite.
12.3 [DONE 2026-09-21] **Priority: P2.** Mobile: Implement in-app log capture (rotating file log) for all API errors and app events. Built `LogCaptureService` (`lib/core/logging/log_capture_service.dart`): a file-backed log in the app documents directory, appending API errors (wired into `api_client.dart`'s `onError`) and general app events, capped at 256KB with oldest-half-drop rotation once the cap is hit. Exposes `tail()` for inlining recent context into a report and `logFileForSharing()` for the raw file (12.4). VERIFIED 2026-09-21: `flutter test test/log_capture_service_test.dart` (6 tests: tail with no entries, API-error/event logging, tail truncation, rotation dropping oldest entries, no-file-yet case) and `flutter analyze`, both clean.
12.4 [DONE 2026-09-21] **Priority: P3.** Mobile: Add "Report a Problem" / "Share Logs" feature so users can email/share captured logs to admin. Wired 12.3's log tail into `AdminReportButton._send()` (`error_handlers.dart`) so an unhandled-error report now carries recent log context, not just the immediate exception. Added a "SHARE DIAGNOSTIC LOGS" button on the Support screen (`support_screen.dart`) that hands the raw log file to the OS share sheet via `share_plus`, for cases where the truncated tail isn't enough. VERIFIED 2026-09-21: `flutter test test/admin_report_button_test.dart` (2 tests, send + retry-on-failure, both now exercising the real log-tail read) and the full `flutter test` suite (157 tests, all passing, no regressions), plus `flutter analyze` clean.
12.5 [DONE 2026-09-17] **Priority: P3.** Mobile: On any unhandled error, show option to "Send Report to Administrator" with log attachment. Implemented as a second button on the ErrorWidget.builder fallback screen, next to the existing Sentry "DIAGNOSE & REPORT" button. It posts the exception message, a 5-line stack summary, and platform info through the same /contact pathway support_screen.dart already uses, so it lands in the same admin inbox as a member support message. Now also carries a recent log tail via 12.3/12.4. VERIFIED 2026-09-17 (send + retry states), re-verified 2026-09-21 with the log-tail integration: `flutter test test/admin_report_button_test.dart` and `flutter analyze` on the touched files, both clean.
12.6 [DONE 2026-09-17] **Priority: P2.** PROCESS: A task can only be marked as [DONE] after its tests have been successfully executed and passed. VERIFIED 2026-09-17: this has been the de facto rule for every closure since the archive split — every entry in docs/TODO_ARCHIVE.md cites the test run that backs it (e.g. NUnit/vitest counts, a named test file, or a live-verification trigger), and 37.11 exists specifically to hold Work Package 37 to it. Adopted formally here; no closure in this file or the archive may skip a test-run citation going forward.

## WORK PACKAGE 27: TEST COVERAGE IMPROVEMENT

27.1  [IN-PROGRESS] Generate low‑coverage report (parse coverage.cobertura.xml). Reporting began 2026-09-16: Coverlet baseline is 59.99% lines and 39.61% branches (7,272/12,122 lines; 1,768/4,463 branches); latest Vitest V8 report is 42.25% lines and 28.44% branches across `src/app` (2,453/5,805 lines; 866/3,045 branches); Flutter LCOV baseline is 14.23% lines (1,392/9,781). All reports are retained by CI. `docs/TEST_COVERAGE_PLAN.md` defines the shared collection, test-selection and threshold method before any threshold is enforced.
27.2  [DONE 2026-08-22] Add test project references for API, Application, Domain, Infrastructure VERIFIED 2026-08-22: `GHCAA.Tests.csproj` references all four projects (Application, Infrastructure, Domain, API).
27.3  [DONE 2026-08-22] Write unit tests for Controllers (WebApplicationFactory) VERIFIED 2026-08-22: 18 controller test classes under `GHCAA.Tests/Controllers/` plus a shared `ControllerTestBase.cs`.
27.4  [DONE 2026-08-22] Write unit tests for Handlers/Services (Moq) VERIFIED 2026-08-22: ~20 service test classes under `GHCAA.Tests/Services/`, with `Moq 4.20.72` + `FluentAssertions 6.12.2` referenced.
27.5  [DONE 2026-08-22] Write unit tests for Domain Validators (FluentValidation) VERIFIED 2026-08-22: `GHCAA.Tests/Validators/MemberRegistrationValidatorTests.cs` and `VerifyEmailValidatorTests.cs`.
27.6  [DONE 2026-08-22] Write repository integration tests with in‑memory SQLite VERIFIED 2026-08-22: `Microsoft.EntityFrameworkCore.Sqlite 9.0.1` + `.InMemory 9.0.1` referenced, with `GHCAA.Tests/Repositories/FileUploadRepositoryTests.cs` and SQLite-backed service tests.
27.7  [DONE 2026-08-22] Write utility class tests (DateFormatConverter, etc.) **CLOSED 2026-08-22:** `GHCAA.Tests/Utils/DateFormatConverterTests.cs` added — 20 tests, all passing; full backend suite now **350 passed / 0 failed** (was 330). Pins the 29F.3 contract in both directions: ISO-8601 on write (incl. time preserved and the `.fff` shape), dd-MM-yyyy accepted on read with ISO as fallback, day-first precedence for ambiguous input like `02-03-2026`, empty/whitespace → `default` on the non-nullable converter but → `null` on the nullable one, and malformed/impossible dates throwing `FormatException` rather than silently yielding `01-01-0001`. Prior note, now historical:  **PARTIAL, confirmed 2026-08-22:** the named example is still untested — `DateFormatConverter` / `NullableDateFormatConverter` live in `GHCAA.API/Utils/DateFormatConverter.cs` and are registered in `Program.cs` (lines ~137-138), but no test file references them. Given these two converters govern **every** DateTime on the wire (see the ISO-8601 switch), they are the highest-value gap in Work Package 27.
27.8 [TODO] **Priority: P2.** Run coverage and enforce ≥ 80 % per file (Still genuinely open, confirmed 2026-08-22: `coverlet.collector 6.0.2` is referenced so coverage *can* be collected locally, but no threshold is enforced anywhere and README explicitly declines to claim a figure. Enforcing >=80%/file would fail today.)
27.9  [DONE 2026-08-22] Update README with test & coverage instructions VERIFIED 2026-08-22: README line ~299 documents `dotnet test` / `npm test` / `flutter test`, and line ~301 explains the coverage position and the local `coverlet.collector` command.
27.10 [IN-PROGRESS — tracked in 82.86] API: Add focused negative tests for the four business-rule coverage gaps (COV-001 through COV-004), including intended 4xx mapping for invalid business input. Use 82.86 as the canonical execution record.
27.12 [DONE 2026-09-17] **Priority: P2.** The `GHCAA.Tests/coverlet.runsettings` collector drops `GHCAA.Infrastructure` from the Cobertura report entirely when run against the Release configuration (`dotnet test -c Release --collect:"XPlat Code Coverage"` on 2026-09-16 produced only `GHCAA.API`, `GHCAA.Application` and `GHCAA.Domain` packages — 515/4,112 lines, nowhere near the 12,122-line denominator the 27.1 baseline cites). Since Infrastructure holds most of the service layer, any coverage percentage collected this way understates real coverage. Needs investigation into why Infrastructure isn't instrumented in Release (module resolution, missing PDBs, or a coverlet/MSBuild config gap) before the 27.1 baseline is refreshed again.
**Resolved 2026-09-17:** does not reproduce. Ran `dotnet test GHCAA.Tests -c Release --collect:"XPlat Code
Coverage" --settings GHCAA.Tests/coverlet.runsettings` twice — once against the existing `bin`/`obj`,
once after `dotnet clean -c Release` and a full rebuild — and both runs produced all four packages
(`GHCAA.API`, `GHCAA.Application`, `GHCAA.Domain`, `GHCAA.Infrastructure`), 12,253 valid lines total,
matching the 27.1 baseline. `GHCAA.Infrastructure` alone reported 62.4% line coverage both times. The
2026-09-16 run that dropped it was a one-off — most likely a stale or partial `bin/obj` from an
interrupted or mixed-configuration build on that machine, not a standing config gap — since the same
`coverlet.runsettings` and project files reproduce cleanly today. No config change made; closing as
not reproducible, with the two commands above recorded so a future recurrence can be compared against
a known-good run.
27.13 [DONE 2026-09-17] Three pre-existing test failure clusters, found 2026-09-17 by a full-suite regression run after the 8.3/8.4/8.6/7.16/12.2 batch, none caused by that batch (confirmed by running each cluster in isolation on an unmodified tree). All three fixed:
  - Backend: `dotnet test --filter "FullyQualifiedName~FinancialServiceTests"` — 3 of 21 failed with a SQLite FK constraint error on `SaveChangesAsync`. Three tests hardcoded `PaymentHistory.MemberId = 1` without creating the matching `Member` row first. Fixed by creating a minimal `Member` for each. VERIFIED 2026-09-17: 21/21 pass.
  - Backend: `dotnet test --filter "FullyQualifiedName~MeProfile_AtTheSameUrl"` — logged as 1 test failing, but running the whole `OutputCacheSecurityTests` fixture showed all 3 tests in the file failed identically (the original filter substring only matched one test name, which undercounted the real scope). Root cause: `ApplicationDbContext.OnModelCreating` seeded two Visual-profile `FamilyLinkRequest` rows via `HasData`, referencing Member Ids 200/1/2 — but Members are Tier 3 seed data, deliberately excluded from `HasData` and loaded only at boot by `InstitutionDataSeeder`. For the Visual test profile, `DatabaseBootstrapperExtensions.BootstrapDatabaseAsync` ran the institution-data seed step before `EnsureCreated()` built the schema, so on every fresh test database that step silently failed with "no such table," Members never loaded, and `EnsureCreated()` then tried to insert the `FamilyLinkRequest` rows against Members that didn't exist yet, throwing `SQLite Error 19: FOREIGN KEY constraint failed`. Fixed by moving `EnsureCreated()`/`EnsureDeleted()` to run first in the Visual profile's boot sequence, and replacing the `HasData` rows with a small idempotent seed step that inserts them by ordinary `SaveChangesAsync()` after Members exist. VERIFIED 2026-09-17: all 3 tests in `OutputCacheSecurityTests` pass; full `GHCAA.Tests` suite re-run afterward with no new failures.
  - Mobile: `flutter test` — 17 of 149 failed across `comprehensive_visual_freeze_test.dart`, `dashboard_visual_test.dart`, `full_app_visual_freeze_test.dart`, all with `NetworkImageLoadException: HTTP request failed, statusCode: 400, http://10.0.2.2:5087/api/assets/logo.png` — a golden-test harness precaching the logo over HTTP against a backend that isn't running in the test environment. Fixing this also surfaced an unrelated overflow bug in `app_dropdown_field.dart` that the golden images were masking. VERIFIED 2026-09-17: 149/149 pass, goldens regenerated.
  Needs someone to pick each cluster up separately; they don't share a root cause.

> Reference doc: docs/CONFIG_DRIVEN_FRAMEWORK.md
> DRY/SOLID review completed by Claude Opus on 2026-05-30.
>
> **STATUS AUDIT 2026-08-22.** This area's checkboxes had drifted badly: 21 of its 34 items were
> still marked `[TODO]` while the code had in fact shipped, which made Work Package 28 look like the
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
              DEPENDS ON: PhaseB_S5S8 + AddRefreshTokens migrations from Work Package 24 applied first

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
> 28.16 [DONE 2026-08-22] Angular: Create feature guard (GHCAA.Web/src/app/core/guards/feature.guard.ts) VERIFIED 2026-08-22: `core/guards/feature.guard.ts` exports a `featureGuard(featureKey, fallbackUrl='/')` factory returning `true` or `router.parseUrl(fallback)`. Applied at **8 route sites** in `app.routes.ts` covering `enableGallery` (x2), `enableEvents` (x2), `enableJobHub` (x2), `enableForum` (x2). Note the plan named `/portal/polls` as a target and **polls is not guarded** — decide whether that is an intentional omission or a gap before closing Work Package 28.
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

## WORK PACKAGE 34: ADMIN-MANAGED SITE CONTENT + MERGED NEWS & NOTICE BOARD (raised by user 2026-08-02)

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
34.D7 [TODO] Regenerate the stale mobile goldens (`flutter test --update-goldens`) as a standalone housekeeping task, so a genuinely new mobile regression is not masked by the existing baseline drift. Deliberately kept out of this area — it is unrelated binary churn from a prior session's theme fix. **Count corrected 2026-09-06:** the original "21" is stale — 80.18 found 58 golden/pixel-diff failures against the current tree (confirmed pre-existing, not a regression, via a clean-tree rerun), out of 80 golden PNGs total. Per the note already in `test/flutter_test_config.dart`, these fail locally on any non-Linux font stack and are deliberately skipped in CI (`skipGoldenAssertion`), so this item is about refreshing the baselines for local dev convenience, not fixing a CI-blocking problem.
34.D8 [TODO] Live-browser verification of this area: public `/about` (seeded CMS blocks + hardcoded fallback), `/contact` (campus address, phones, map guard), `/news?type=Notice`; admin CMS block edit round-trip; admin notice creation with a PDF and a working public download link; and a **member account confirming it cannot create a Notice** (34.C3). **Blocked by the same local seed/role mismatch as 33.13** — `shalin` and `superadmin` resolve to ordinary alumni members in the local DB, so `adminGuard` bounces `/admin/*`; the local role assignment must be fixed before the admin-side steps can run.
34.D9 [DONE 2026-08-03] Preprod schema catch-up script `docs/sql/preprod_area34_sitecontent.sql` — idempotent SQL mirroring migration `20260802163432_AddSiteContentAndNoticeFields` (three `NewsPosts` columns + `SiteContents` table, unique `Key` index, the five seed blocks, and an identity-sequence `setval` so the first admin-created block does not collide on `Id = 1`). Needed because Work Package 34 shipped to preprod but the content never appeared: the runtime builds schema with `EnsureCreated()` (`GHCAA.API/Program.cs:290`), which is a **no-op on a database that already has tables**, so the new table and columns were never created and `/about` fell back to its static markup. **Applied to preprod/Neon 2026-08-03 and verified**: the three `NewsPosts` columns exist, the five blocks are present, the identity sequence sits at 5, `GET /api/news` returns 200 (was a 500 — `42703: column n.AttachmentFileName does not exist`) and `GET /api/site-content?group=about` returns the four About blocks.
34.D10 [DONE 2026-09-04] **Superseded, verified against `Program.cs`.** The non-Visual startup path
no longer calls `Database.EnsureCreated()` directly. `Program.cs:388-406` calls
`MigrationBootstrapper.EnsureMigratedAsync(schemaCtx, app.Logger)` for every profile except `Visual`,
which does exactly what this item asked for: baselines a legacy `EnsureCreated`-built database (writes
`__EFMigrationsHistory` for migrations already present) before running `Migrate()`, so new columns and
tables ship on boot without a manual step. `EnsureCreated()` survives only inside the `catch` as a
degrade-to-today's-behaviour fallback if the bootstrap itself throws, logged as an error rather than
silently. Per `gotcha_migrationbootstrapper_fixed_offset`, this shipped across two rounds of fixes
already covered elsewhere in this tracker; this entry closes because the item asked whether the switch
had happened, and it has. Visual profile keeps its own recreate/seed path, unaffected.
34.D11 [DONE 2026-08-03] Seeded SiteContent titles contained HTML entities (`Logo &amp; Flag`, `Purpose &amp; Objectives`), but `about.html` renders the title with `{{ }}` interpolation (which escapes) while only the body uses `[innerHTML]` — so the headings showed a literal `&amp;`. Corrected in all four places that carry the value: `Seed/site_content.json`, the migration's `InsertData`, `docs/sql/preprod_area34_sitecontent.sql`, and the live Neon rows (`UPDATE ... replace("Title",'&amp;','&')`). Body HTML entities (`&ndash;`, `&ldquo;`) are correct as-is — that field *is* parsed as HTML.
34.D12 [DONE 2026-08-03] Public nav fixes raised by user: (a) the association full name never rendered under the logo — `public-layout.html` carried `class="hidden xl:block"`, but this project has **no Tailwind**, so `hidden` matched the real global `.hidden { display: none }` utility in `styles.scss` while `xl:block` matched nothing; both classes removed, leaving `.hide-mobile` and the existing `≤600px` rule to do the responsive hiding. (b) The public site could get stuck on a white background: the theme choice is persisted globally by `ThemeService`, but the switch lived only in `<app-user-menu>` (portal/admin), so a visitor who once chose light mode had no way back. Extracted the button into a shared `common/theme-toggle/` component (per the "try common changes as reusable" constraint) and placed it in both the user menu and the public nav. `npx vitest run` **60 files / 244 tests** green; `ng build` clean.

34.D13 [DONE 2026-08-03] Logo asset: the crest artwork the user pasted is `assets/logo.jpg` — RGB 1024×1024 on an **opaque black square**. The existing `logo.png` was a transparent but lower-resolution 676×814 crop, so it was not the same artwork. Per "make another version of transparent background of these without another changes, and use it everywhere", a new transparent `logo.png` was derived **from the jpg** at RGBA 1024×1024 by BFS flood-fill seeded from every near-black (≤40) pixel on the four image edges — edge-seeding preserves interior black artwork and clears only background-contiguous black, so the drawing itself is unchanged. Installed to `GHCAA.Web/public/assets/logo.png` and `GHCAA.Mobile/assets/logo.png` (527,642 B, smaller than the 908,708 B it replaced), verified by compositing over white and over `#0c0c0c` — no halo, legible on both. The remaining 6 `appImgFallback` placeholders still on the jpg were repointed (`common/gallery/gallery.html`, `gallery.ts`, `common/news/news.html`, `public/magazine/magazine.html` ×3), so all 23 `assets/logo` references app-wide now use the png. This **reverses** the earlier "keep the gallery/news jpg fallbacks" rule, which predated the "use it every where" instruction. `Seed/photos.json` still names logo.jpg deliberately — sample gallery content, not branding. `flutter analyze` 0, `ng build` 0, `vitest` 60 files / 244 tests, `dotnet test` 330 passed.

34.D14 [DONE 2026-08-03] "Cant see public theme switcher" investigated and found to be **not a code defect**. Verified in a real headless Chromium at 1440×900 against the dev server: `app-theme-toggle` count 1, `visible: true`, box `{x:1376, y:18, w:20, h:25}`, computed `color: rgba(255,255,255,0.85)`, `title="Switch to light mode"`, and the `theme-light` sun glyph present as a correctly-namespaced `<circle>`+`<path>` inside a 20×20 `<svg>`. Two hypotheses were disproved along the way and are recorded so they are not re-chased: (a) `theme-dark`/`theme-light` *are* valid icon names — they live in `common/icon/icon.html` (lines ~159 and ~165), not in `icon.ts`, which is why grepping the `.ts` found nothing; (b) the pile of `<!--container-->` comments inside the toggle's `svg.innerHTML` is **normal** — Angular `@switch` emits one comment anchor per non-matching `@case`, so read `children.length` and `namespaceURI` instead of eyeballing innerHTML. Remaining explanations are non-code: low discoverability (a bare 20px 85%-white stroke icon pinned at the extreme right edge after eight bold uppercase links) or the user viewing the stale preprod deploy. Left unstyled pending user preference, per "keep all other thing as it is".

34.D15 [DONE 2026-08-03] About page content rewritten per "add about college, see official college website, and dont direct copy from constitution, write in meaningful way for others" — this **supersedes** the original plan decision to seed constitution text only with no web research. `Seed/site_content.json` went 5 → 6 blocks: `about-origin` retitled "The College and Its Origin" (18 Dec 1938 founding, Ashutosh Ganguly's one-lakh-rupee donation, advocate Satish Chandra Bhattacharya, A. H. M. Wajir Ali); **new** `about-college-today` with facts sourced from the official site (16 honours departments, 80+ teachers, 100,000+ graduates, campus address, EIIN 111160, college code 5701, principal, outbound link, plus an `<em>` disclaimer that the Association is an independent alumni body); `about-association` reworded out of verbatim constitution phrasing; `about-logo` "logo"→"crest"; and `about-objectives` retitled **"What We Do"** with the 11 constitution objectives restructured into 6 thematic bullets (Connection / Students / Welfare / Community / Heritage / Governance). The college's nationalisation date was **deliberately omitted** — it could not be confirmed on the official site, and no fact was invented. Note the seed JSON alone does **not** reach preprod (`EnsureCreated()` no-ops on a non-empty DB, see 34.D9/34.D10) — the new and changed blocks must go in via the admin SiteContent CMS or a catch-up SQL script.

34.D16 [DONE 2026-08-21] Fixed: `portal-layout.ts` now injects `OrgConfigService` (exposed as `orgConfig`) and both portal logos bind `[src]="orgConfig.config()?.branding?.logoUrl || '/assets/logo.png'"` + `[alt]="...shortName || 'Logo'"` with `appImgFallback="/assets/logo.png"`, mirroring `admin-layout.html` exactly. Two adjacent hardcodes in the same branding-opt-out class were fixed with it: the top-bar text `GHCAA Member Portal` and the `document.title` suffix now use `branding.shortName` (fallback `'GHCAA'`). The sidebar wordmark `HARAGANGIAN` was **left hardcoded on purpose** — it is the member nickname styled as a wordmark, and swapping it to `shortName` would visibly change the portal's sidebar text, which is beyond this item. Verified: `ng build` clean, vitest 60 files / 244 tests pass. Original finding follows. `portal-layout.html` hardcoded `src="/assets/logo.png"` in two places (the sidebar mini-logo and the collapsed-rail logo) instead of binding `orgConfigService.config()?.branding?.logoUrl` the way admin-layout and the public footer do — `portal-layout.ts` does not inject `OrgConfigService` at all. Harmless today because the configured default *is* `/assets/logo.png`, but it silently opts the portal out of org-config branding, so a tenant that sets a custom logo gets it everywhere except the member portal.

## WORK PACKAGE 37: ALUMNI-ASSOCIATION DEPTH — ELECTION ENGINE, SCHOLARSHIPS, PHILANTHROPY, MEMORY (raised by "what outstanding/extra ordinary matters can be incorporated on this project", 2026-08-23)

> Origin: a deliberate look at what a mature college alumni association does that this codebase
> does not yet model. Work Package 36 published the *documents* of governance and the *categories* of
> money; Work Package 37 builds the *machinery* behind them. Every item below is scoped against what the
> repository actually contains today, and — per `feedback_keep_lightweight` — **none of the ten
> requires a new npm or NuGet dependency.**
>
> **Evidence, money.** `GHCAA.Domain/Enums.cs` declares
> `FinancialCategory { MembershipFee, RegistrationFee, Donation, Event, Maintenance, Salary,
> Utilities, ReunionFee, Sponsorship, Grant, Refund, Other }`. Four of those values —
> `Donation`, `ReunionFee`, `Sponsorship`, `Grant` — **exist only as ledger labels an admin can
> pick when hand-entering a `FinancialRecord`.** There is no scholarship, no reunion and no grant
> application anywhere in the 45 files under `GHCAA.Domain/Models/`. The ledger can *record*
> philanthropy; the product cannot *conduct* most of it — the one exception is `Campaign`/
> `CampaignPledge` (37.3), which already exists and is a rollout gap, not a modelling gap.
>
> **Evidence, elections.** Work Package 36 shipped seven election documents as read-only markdown and 18
> blank ER-forms split out of the handbook. The only election-adjacent tables are `ECPeriod`
> (`Title`/`StartDate`/`EndDate`/`IsActive`) and `ECMember` (`MemberId`/`ECPeriodId`/`Position`/
> `StartDate`/`EndDate`/`ChangeReason`) — i.e. **the result of an election, recorded by hand after
> the fact.** There is no `Nomination`, `Candidate`, `Ballot`, `VoterRoll` or `ScrutinyDecision`
> model. `Poll`/`PollOption`/`PollVote` are a general opinion-poll feature with no eligibility
> roll, no secrecy separation, no nomination phase and no returning-officer role; they are not a
> ballot and must not be overloaded into one.
>
> **Evidence, people.** `Member` has no batch/session/graduation field at all — cohort identity
> lives on `AcademicRecord` (`AdmissionYear`, `PassingYear`, `IsOrgProfile`, `Degree`, `Subject`), which
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
> just because the model compiles, and `HasData` seed edits never reach it. Work Package 36 solved this
> once with `GHCAA.Infrastructure/Data/ConstitutionSeeder.SyncAsync(context, logger, ct)` — an
> idempotent boot-time syncer called from `Program.cs` behind a `CanConnectAsync()`/`LogWarning`
> guard. **Copying that pattern per feature is the fallback, not the plan.** 37.0 makes the real
> migration path a prerequisite, because four more bespoke syncers is a smell.
>
> **Recommended sequence: 37.0 → 37.3 → 37.2 → 37.1**, with 37.5 taken opportunistically (it is
> the smallest item in the Area and needs no new UI shell). 37.7–37.10 are independent and may be
> scheduled at any point after 37.0.

37.1 [DONE 2026-09-21] **Priority: P3.** **Election engine** — persisted backend, Web, Mobile, migration, security controls, reusable PDF forms, FR-tagged API, and cross-layer tests are complete. Verified 2026-09-21: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --configuration Release` (844 passed), `npm run test:unit` (87 files, 499 passed), `npm run build`, `flutter test --exclude-tags golden --reporter expanded` (92 passed), `flutter analyze`, focused election tests, and `graphify update .`. New enums in `GHCAA.Domain/Enums.cs`: `ElectionPhase { Announced, Nomination, Scrutiny, Withdrawal, CandidateList, Campaign, Polling, Counting, Declared, Archived }`, `NominationStatus { Submitted, UnderScrutiny, Accepted, Rejected, Withdrawn }`, `ElectionRole { ReturningOfficer, AssistantReturningOfficer, PollingOfficer, Scrutineer }`.
  - **37.1a Election + roll.** `Election` (`Id`, `Title`, `ECPeriodId`, `Phase`, `AnnouncedOn`, `NominationOpensOn`, `NominationClosesOn`, `ScrutinyOn`, `WithdrawalClosesOn`, `PollingOpensOn`, `PollingClosesOn`, `DeclaredOn?`, `IsActive`, `CreatedBy`), `ElectionSeat` (`Id`, `ElectionId`, `ECPosition Position`, `SeatCount`), `ElectionOfficer` (`Id`, `ElectionId`, `MemberId`, `ElectionRole Role`), and `VoterRoll` (`Id`, `ElectionId`, `MemberId`, `IsEligible`, `IneligibilityReason?`, `FrozenAt`, `VotedAt?`). The roll is **frozen by snapshot**, not computed at poll time: eligibility is the same Article III Section K rule already enforced in `GovernanceService.VoteOnConstitutionAsync` (`MembershipType` of `Founding`, `Executive` or `General`), plus dues-current per `MembershipDue`. Freezing is what makes a disputed result auditable.
  - **37.1b Nomination.** `Nomination` (`Id`, `ElectionId`, `ElectionSeatId`, `CandidateMemberId`, `ProposerMemberId`, `SeconderMemberId`, `Statement`, `PhotoPath?`, `Status`, `SubmittedAt`, `WithdrawnAt?`), `ScrutinyDecision` (`Id`, `NominationId`, `OfficerMemberId`, `Accepted`, `Reason`, `DecidedAt`). Proposer and seconder must both be on the frozen roll and must not be the candidate; enforce in the service, not only the UI.
  - **37.1c Ballot and poll.** `Ballot` (`Id`, `ElectionId`, `ElectionSeatId`, `SerialNumber`, `IssuedAt`, `IsSpoiled`) and `BallotVote` (`Id`, `BallotId`, `NominationId`, `CastAt`) kept in **separate tables with no member foreign key on the vote side** — the roll records *that* a member voted (`VoterRoll.VotedAt`), the ballot records *what* was voted, and nothing joins the two. That separation is the secret ballot, and it is the one design decision here that cannot be retrofitted. A unique index on `(ElectionId, MemberId)` in the roll prevents double voting.
  - **37.1d Counting and declaration.** `ElectionResult` (`Id`, `ElectionId`, `ElectionSeatId`, `NominationId`, `VoteCount`, `IsElected`, `IsTie`). On declaration, write the winners straight into `ECMember` rows against the election's `ECPeriodId` — this is the payoff: the committee roster stops being hand-typed.
  - **37.1e ER-forms as generated PDFs.** The existing handbook remains the legal template catalog. Filled documents must be generated from persisted records with **QuestPDF**, following `IDCardService.GenerateIDCardPdfAsync` for structure and its `GetQrDataUri` helper for verification QR data. The minimum generated set is ER-01 Election Notice, ER-02 Calendar, ER-07 Voter Roll Certification, ER-09 Nomination Paper, ER-10 Candidate Consent, ER-11 Scrutiny Checklist, ER-13 Withdrawal, ER-14 Final Candidate List, ER-19 Poll Integrity Certificate, ER-23 Count Sheet, ER-26 Recount Request, ER-28 Recount Report, ER-29 Final Result Sheet, ER-30 Result Certification, ER-31 Result Declaration, and ER-33 Handover Certificate. Every generated document must carry the election reference, form code, institution profile, generation timestamp, page number, signatory/evidence state, and verification QR where applicable. Raw Markdown is only the public handbook source, not an official completed form. See `docs/specs/003-alumni-programs-and-verification/evidence/election-forms-review.md`.
  - **Service/API.** `IElectionService` in `GHCAA.Application/Interfaces` + `GHCAA.Infrastructure/Services/ElectionService.cs` (no DI registration needed). `ElectionsController` at `api/elections`, `[Authorize]` at class level; `GET api/elections/public` and `GET api/elections/{id}/candidates` are `[AllowAnonymous]` (the candidate list is a published document); nomination, scrutiny and casting are member- or officer-scoped. Casting must reject any phase other than `Polling` server-side.
  - **UI.** Flag `enableElections`. The existing public `/elections` page gains a live banner when an election is not `Archived`. Member `portal/elections` — nominate, withdraw, view candidates, cast. Admin `admin/elections` — create, appoint officers, freeze roll, scrutinise, advance phase, count, declare, download ER PDFs. New `API_ENDPOINTS.ELECTIONS` block.
  - **Tests.** NUnit: roll freeze excludes Associate/Honorary/Advisory; proposer ≠ candidate; double vote rejected; cast outside `Polling` rejected; declaration writes `ECMember`; **a ballot row cannot be joined back to a member**. Vitest: phase-driven UI state, closed-nomination guard.
  - **37.1f Navigation [DONE 2026-09-22].** The engine had no drawer or sidebar entry, so a member or admin who knew the routes could reach `/portal/election` and `/admin/elections` but nobody else could find them. Added `Association Election` (member, Community section) and `Elections` (admin, Content section) to `nav.service.ts`, plus the matching mobile drawer entries. Verified: `npx vitest run src/app/core/services/nav.service.spec.ts` (5 passed).
  - **37.1g Post-ship defect fixes [DONE 2026-09-24].** A code-review pass on the shipped engine found three critical and one major defect, all now fixed:
    - `AdminElectionsController` was querying `ApplicationDbContext` directly instead of going through `IElectionService`, breaking the layering rule that controllers never touch the DbContext. Refactored `Publish`/`Close`/candidate add-remove to call service methods (`GetAdminElectionAsync`, `AddCandidateAsync`, `RemoveCandidateAsync`); the duplicate `BuildAdminElectionAsync`/`ToAdminElection`/`SeatTitle` helpers were deleted from the controller since that logic already lives in `ElectionService`.
    - `Scrutinise` in `ElectionsController` trusted the officer's member id from the request body instead of the auth claim, letting any caller record a scrutiny decision under someone else's identity. Now reads `officerMemberId` from `CurrentMemberIdRaw()`.
    - The double-vote guard was a single `VoterRoll.VotedAt` column, which blocked a legitimate second vote for a different seat in the same election, not just a replay for the same seat. Replaced with a new `SeatVote` table (`ElectionId`, `ElectionSeatId`, `MemberId`, `VotedAt`, unique index on the triple) as the actual compare-and-set target; `VoterRoll.VotedAt` stays as a first-vote-only informational flag. Migration `20260923184652_AddSeatVotes`.
    - `SetPhaseAsync` allowed re-entering the current phase, which could re-trigger phase-transition side effects. Added a same-phase no-op guard.
    - Also folded in while touching these files: migrated the remaining raw `[Authorize(Roles = "Admin,SuperAdmin")]` attributes on `ElectionsController` to `[Authorize(Policy = Policies.AdminOnly)]` (exact same role set, matches the policy already used on `AdminElectionsController`).
    - Verified: `dotnet build` clean; `dotnet test --filter "FullyQualifiedName~ElectionServiceTests"` (4/4, including a new `CastVoteAsync_AllowsVotingForDifferentSeatsInTheSameElection` regression test).
  - **37.1h Election module client redesign (web + mobile) [PLANNED, depends on 37.1g].** A follow-up review of the Angular and Flutter election clients (not covered by 37.1g, which was backend-only) found the Angular member voting page calls three routes that don't exist on the backend at all (`GET /elections/current`, `GET /elections/{id}/results`, `POST /elections/{id}/ballot`), so voting through the web UI is broken in production today. Also found: two parallel, half-wired Angular model/service families (an unused-but-correct per-seat DTO set alongside a used-but-wrong whole-election set); no in-flight guard on Angular admin `publish`/`close`; a Flutter provider that reaches into a service's private field and swallows all errors; no per-seat voted tracking on either client; no Flutter admin election screen despite most of the service methods existing unused; dead `createdBy`-in-body code on both clients mirroring the identity-spoofing pattern already fixed server-side in 37.1g. Full spec at `docs/specs/011-election-module-redesign/spec.md`. Staged: (1) spec — this item; (2) backend `GET /elections/current` addition; (3) Angular rebuild against the real per-seat API, reusing the campaigns module's `saving`-signal pattern; (4) Flutter fixes + new admin election screen modeled on `governance_registry_screen.dart`; (5) doc sync + full three-client test run. Sequenced to stay inside the user's stated weekly usage cap — stage 2 (Angular, the currently-broken client) is prioritized over stage 4 (Flutter admin screen), and this item's status will be updated per stage actually completed rather than left ambiguous if the session stops early.

37.2 [DONE] **Priority: P4.** Specified in
`docs/specs/003-alumni-programs-and-verification/spec.md` Story 4.
**Scholarship & student-aid programme** — fund → open call → application → blind review → award → disbursement. New enums: `ScholarshipApplicationStatus { Draft, Submitted, UnderReview, Shortlisted, Awarded, Rejected, Withdrawn }`, `DisbursementStatus { Pending, Approved, Paid, Cancelled }`.
  - **Models.** `ScholarshipFund` (`Id`, `Name`, `Description`, `NamedAfter?` — the endowment-in-memory case that links to 37.5, `TargetAmount`, `IsActive`, `CreatedAt`); `ScholarshipCall` (`Id`, `ScholarshipFundId`, `AcademicYear`, `OpensOn`, `ClosesOn`, `SlotCount`, `AwardAmount`, `EligibilityCriteria`, `IsActive`); `ScholarshipApplication` (`Id`, `ScholarshipCallId`, `ApplicantName`, `ApplicantEmail`, `ApplicantPhone`, `InstitutionName`, `Class`, `GuardianName`, `HouseholdIncome`, `NeedStatement`, `MeritStatement`, `Status`, `SubmittedAt`, `ReferenceCode`); `ScholarshipDocument` (`Id`, `ScholarshipApplicationId`, `FileUploadId`, `DocumentType`) reusing the existing `FileUpload` + `IFileValidationService` path — **no new upload plumbing**; `ScholarshipReview` (`Id`, `ScholarshipApplicationId`, `ReviewerMemberId`, `NeedScore`, `MeritScore`, `Comments`, `ReviewedAt`); `ScholarshipAward` (`Id`, `ScholarshipApplicationId`, `Amount`, `AwardedOn`, `DisbursementStatus`, `FinancialRecordId?`).
  - **Blind review is a query rule, not a UI rule.** `GetApplicationForReviewAsync` must project a DTO that omits `ApplicantName`, `ApplicantEmail`, `ApplicantPhone` and `GuardianName`, exposing only `ReferenceCode`. Reviewers must not be able to obtain identity from the API at all; hiding it in the template is not acceptance.
  - **Applicants are not members.** The public application form is `[AllowAnonymous]` and identified by `ReferenceCode` + email; **do not create `Member`/`User` rows for schoolchildren.** Status lookup is `GET api/scholarships/status/{referenceCode}`, also anonymous, rate-limited by the existing `LoginRateLimitMiddleware` pattern.
  - **Ledger tie-in.** Marking an award `Paid` writes a `FinancialRecord` with `RecordType = Expense`, `FinancialCategory = Grant`, `Reference = ReferenceCode`, and stores the new record's `Id` back on `ScholarshipAward.FinancialRecordId`. This is what makes the impact report in 37.10 derivable rather than typed.
  - **Service/API/UI.** `IScholarshipService` + `ScholarshipService`; `ScholarshipsController` at `api/scholarships`. Flag `enableScholarships`. Public `/scholarships` (call listing + apply + status check), member `portal/scholarships` (reviewer queue for panel members), admin `admin/scholarships` (funds, calls, shortlist, award, disburse). New `API_ENDPOINTS.SCHOLARSHIPS` block.
  - **Tests.** NUnit: review DTO carries no identifying field; application rejected outside the `OpensOn`–`ClosesOn` window; award → paid writes exactly one `Grant` `FinancialRecord` and is idempotent on repeat. Vitest: apply-form validation, status lookup with an unknown code.
**Acceptance:** done; `ScholarshipServiceTests` covers the blind-review projection, the call-window rejection and the idempotent `Grant` disbursement; `scholarship.service.spec.ts` covers apply-form validation and the unknown-code status lookup. `enableScholarships` is not wired to any feature-flag mechanism, since none exists project-wide yet; the flag column exists in `OrgConfigDto`/profile packs for when one lands. Verified 2026-09-23 against the WP37.11 gate (`dotnet test`: 859/859; `npx vitest run`: 511/511; `npm run type-check` and `npx ng build`: clean).

37.3 [DONE] **Priority: P2.** **Fundraising campaigns — complete the rollout.** Unlike the rest of this Area, this item is not a ground-up build: `Campaign`/`CampaignPledge` models, `ICampaignService`/`CampaignService`, a 12-route `CampaignsController` (public browse/pledge, member `my-pledges`, admin CRUD + donor tiers + pledge-receipt confirmation), an admin Angular page (`admin-campaigns.ts`), a public Angular page (`campaigns.ts`), and a member giving-history page (`member/giving/giving.ts`) are all already committed (`f960d216`) and covered by `CampaignServiceTests.cs`. What is missing is Flutter mobile coverage, not the web feature.
  - **Reuse — do not rebuild.** `ICampaignService`, `CampaignsController`, and all three existing Angular pages (public, admin, member giving-history) are done. `ConfirmPledgeReceiptAsync` is already idempotent (confirming an already-confirmed pledge does not write a second `FinancialRecord`) — that is the pattern 37.2's disbursement and 37.4's reunion fees follow, not the other way round.
  - **Migration: already done.** `Campaign`/`CampaignPledge` are already in the `InitialBaseline` migration, so they exist on a migrated database, not only under `EnsureCreated()`. No new migration is needed for this item.
  - **Web: already done.** `member/giving/giving.ts` (route `member/giving`, `git log` shows it landed in `f960d216` alongside the rest of the feature) already calls `GET api/campaigns/my-pledges` and renders pledge history with a running total. No Angular work remains.
  - **Mobile.** [DONE 2026-09-23] `lib/features/campaigns/` (`campaign_service.dart` + typed models) and three screens — `campaigns_screen.dart` (public browse), `campaign_detail_screen.dart` (detail, honour roll, pledge form), `my_pledges_screen.dart` — follow the Scholarship mobile pattern (typed `fromJson`/`toJson`, `Provider<CampaignService>`, no try/catch in the service). Routed at `/campaigns`, `/campaigns/my-pledges`, `/campaigns/:slug`; drawer entry added under My Account. This also added the drawer entry for Mentorship Hub (`/mentorship`), a pre-existing navigation gap noticed while adding Campaigns.
  - **Missing: docs.** `docs/FEATURES.md` has zero mentions of Campaigns despite being shipped — add a section. [DONE 2026-09-23] The stale `// TODO 37.8` comment (real 37.8 is Credential Verification) was removed from all five files it appeared in (`Campaign.cs`, `CampaignsController.cs`, `ICampaignService.cs`, `CampaignService.cs`, `CampaignDtos.cs`) rather than renumbered, since the feature it described is already built.
  - **Tests.** [DONE 2026-09-23] `test/campaign_service_test.dart` — 4 cases (public list, get-by-slug, honour roll tiers/untiered, submit pledge + my-pledges), all passing. Backend and admin/public/member Angular tests already exist and are not duplicated here.
**Acceptance:** done; the web feature was already shipped in `f960d216`, and the remaining gap — Flutter mobile coverage — is now built and routed, closing the item. Verified 2026-09-23: `flutter test test/campaign_service_test.dart` (4/4 passing) and `flutter analyze` on all new/changed files (no issues found).

37.4 [TODO] **Priority: P3.** **Batch cohorts and reunions as first-class objects.** Today a batch exists only as `AcademicRecord.PassingYear` — there is no cohort page, no cohort representative and no reunion.
  - **Models.** `BatchCohort` (`Id`, `PassingYear`, `Title`, `Story?`, `CoverImagePath?`, `RepresentativeMemberId?`, `IsActive`); `Reunion` (`Id`, `BatchCohortId?` — null means an all-alumni reunion, `AlumniEventId`, `Theme`, `SouvenirUrl?`) built **on top of** the existing `AlumniEvent` + `EventRegistration` + `EventBudget` stack rather than beside it — a reunion is an event with cohort identity, and duplicating registration logic would be the mistake here.
  - **Membership is derived, not stored.** Cohort membership = `AcademicRecord` rows with `IsOrgProfile == true` and the matching `PassingYear`. Do not add a `BatchYear` column to `Member`; it would immediately disagree with `AcademicRecord` for anyone holding two GHC records.
  - **Ledger tie-in.** Reunion fees collected through `EventRegistration` post as `FinancialCategory.ReunionFee`, finally giving that enum value a producer.
  - **Service/API/UI.** `IBatchService` + `BatchService`; `BatchesController` at `api/batches` with `GET api/batches/public` and `GET api/batches/{year}` `[AllowAnonymous]`. Flag `enableReunions`. Public `/batches` (year grid) + `/batches/:year`, member `portal/my-batch`, admin `admin/batches`. New `API_ENDPOINTS.BATCHES` block.
  - **Tests.** NUnit: a member with two GHC `AcademicRecord` rows appears in both cohorts; non-GHC records excluded. Vitest: year-grid grouping, empty-cohort `EmptyStateWidget` path.

37.5 [TODO] **Priority: P3.** Specified in
`docs/specs/003-alumni-programs-and-verification/spec.md` Story 3.
**In Memoriam register.** A new `MembershipType` value is **not** the mechanism — membership tier is admin-assigned and orthogonal (`feedback_membership_type_admin_only`). Instead: `MemorialEntry` (`Id`, `MemberId?` — nullable so a pre-digital alumnus can be honoured, `FullName`, `PassingYear?`, `DateOfBirth?`, `DateOfDeath`, `PhotoPath?`, `Tribute`, `IsPublished`, `SubmittedByMemberId?`, `SubmissionStatus Status`, `CreatedAt`) reusing the existing `SubmissionStatus { Draft, Pending, Approved, Rejected }` enum, and `Condolence` (`Id`, `MemorialEntryId`, `MemberId`, `Message`, `PostedAt`, `IsApproved`).
  - **Moderation is mandatory.** Nothing publishes without admin approval, and every condolence goes through the existing `HtmlSanitizer` path before storage. This is the single highest-sensitivity surface in the Area; an unmoderated tribute wall on a memorial page is a reputational incident.
  - **Setting `MemorialEntry.MemberId` must deactivate the linked `Member`** — and suppress them from the public directory and from any 37.1 voter roll — in the same transaction. A deceased member appearing on an election roll is the failure mode this clause exists to prevent.
  - **Service/API/UI.** Extend `IMemberService` **only if** the memorial logic stays under ~5 methods; otherwise `IMemorialService` + `MemorialService`. `MemorialController` at `api/memorial`, `GET api/memorial/public` `[AllowAnonymous]`. Flag `enableMemorial`. Public `/in-memoriam`, member submission form under `portal/`, admin moderation queue. New `API_ENDPOINTS.MEMORIAL` block.
  - **Tests.** NUnit: an unapproved entry is absent from the public projection; linking a `Member` deactivates them and removes them from directory results; condolence HTML is sanitised. Vitest: moderation-queue actions, published/unpublished rendering.

37.6 [DONE] **Priority: P4.** **Oral-history / legacy archive** — recorded memories from senior alumni, which is the one asset an alumni association can create that nobody else can. `ArchiveCollection` (`Id`, `Title`, `Description`, `IsPublished`, `SortOrder`) and `ArchiveItem` (`Id`, `ArchiveCollectionId`, `Title`, `NarratorName`, `NarratorMemberId?`, `RecordedOn?`, `Summary`, `Transcript?`, `MediaFileUploadId?`, `ExternalMediaUrl?`, `PhotoPath?`, `DecadeTag`, `IsPublished`, `SubmissionStatus Status`).
  - **Storage decision, settled before building:** `MediaFileUploadId` reuses `FileUpload` + `IFileStorageService`; `ExternalMediaUrl` covers a link to already-hosted audio/video. **Both fields exist deliberately** — audio is heavy and the Render deployment has no object store configured, so the external-link path is the default and the upload path is opt-in behind the existing size limits in `IFileValidationService`.
  - **The transcript is the product**, not the audio: it is searchable, printable, quotable in 37.10, and readable on a bad connection. Treat a missing transcript as an incomplete item in the admin queue, not merely an empty field.
  - **Service/API/UI.** `IArchiveService` + `ArchiveService`; `ArchiveController` at `api/archive`, `GET api/archive/public` and `GET api/archive/items/{id}` `[AllowAnonymous]`. Flag `enableLegacyArchive`. Public `/legacy` (collections → item with transcript) reusing the Work Package 36 `.doc-hero` / `.doc-prose` shell rather than new page chrome; member submission; admin curation. New `API_ENDPOINTS.ARCHIVE` block.
  - **Tests.** NUnit: unpublished items excluded from public reads; an item with none of `MediaFileUploadId`, `ExternalMediaUrl` or `Transcript` is rejected. Vitest: decade filter, transcript rendering and print styles.
**Acceptance:** done; `ArchiveServiceTests` covers the public-visibility filter and the `CreateItemAsync` rejection when a submission carries none of `FileUploadId`, `MediaUrl` or `Transcript`. Model naming diverges slightly from the spec (`ArchivePublicationState`/`ArchiveModerationState` in place of a generic `SubmissionStatus`), functionally equivalent. Verified 2026-09-23 against the WP37.11 gate (see 37.2's Acceptance line for the run figures).

37.7 [TODO] **Priority: P2.** **Bengali/English bilingual UI.** The association's constituency is Bengali-speaking and every string in the app is currently a hardcoded English literal in a template. **Do not install `@angular/localize` or `ngx-translate`** — `feedback_keep_lightweight` applies, and the requirement here is a single flat key → string lookup with a live runtime toggle, which `@angular/localize` (build-time, one bundle per locale) does not even satisfy.
  - **Mechanism.** `core/services/i18n.service.ts` holding a `signal<'en' | 'bn'>` persisted to `localStorage`; two dictionaries under `core/i18n/en.ts` and `core/i18n/bn.ts` typed as `Record<string, string>` with `en` as the key source of truth; and a pure `TranslatePipe` (`{{ 'nav.constitution' | t }}`) falling back to the English string, then to the key itself, when a Bengali value is missing. Update `<html lang>` on toggle. Ship the toggle next to the existing `<app-theme-toggle>` so it inherits placement and styling.
  - **Scope explicitly, and state it in the item when it lands:** public site + member portal nav, buttons, labels and validation messages. **Admin stays English-only** — it is staff-facing, and translating it doubles the surface for no constituency benefit. Server-stored content (constitution text, election documents, news) is not translated by this mechanism; it is authored content and belongs to whichever language it was written in.
  - **Font.** Bengali glyphs need a webfont with Bengali coverage. Production `ng build` inlines Google Fonts over the network and this environment cannot reach it — self-host the face under `public/assets/fonts/` and reference it from `styles.scss`, so the build stays offline-safe.
  - **Tests.** Vitest: the pipe returns the Bengali value, falls back to English on a missing key, and falls back to the key when both are missing; the toggle persists across a service re-instantiation; **every key present in `en.ts` resolves through the pipe** (guards against key drift).

37.8 [DONE] **Priority: P3.** Specified in
`docs/specs/003-alumni-programs-and-verification/spec.md` Story 1.
**Public credential verification.** `IIDCardService` already issues ID cards and certificates as PDFs, but nothing on the outside can confirm one is genuine — an employer holding a printed membership certificate has no check available.
  - **Models.** `IssuedCredential` (`Id`, `MemberId`, `CredentialType` — new enum `CredentialType { MembershipCertificate, IdCard, ElectionDocument }`, `ShortCode` — a 10-character unambiguous-alphabet code with a unique index, `IssuedOn`, `ExpiresOn?`, `IsRevoked`, `RevokedReason?`, `RevokedOn?`).
  - **Mechanism.** Extend `IIDCardService` (do not create a parallel service) so every generated document records an `IssuedCredential` and embeds a QR — via the existing `QRCoder` `GetQrDataUri` helper — pointing at `/verify/{shortCode}`. `GET api/verify/{shortCode}` is `[AllowAnonymous]` and returns **only** `{ valid, memberName, membershipType, issuedOn, status }`. It must never return an email, phone, address or member id: this endpoint is publicly enumerable by design, so the short code must be high-entropy and the response minimal. Rate-limit it with the existing `LoginRateLimitMiddleware` pattern.
  - **UI.** Flag `enableCredentialVerification`. Public `/verify/:code` plus a code-entry form at `/verify`, rendering a single valid / revoked / unknown verdict card; admin revocation action on the member detail page. New `API_ENDPOINTS.VERIFY` block.
  - **Tests.** NUnit: a revoked code returns `valid: false`; the response DTO exposes no contact field; short codes are unique across 10k generations. Vitest: the three verdict states, unknown-code path.
**Acceptance:** done; `CredentialCodeGeneratorTests` covers the 10k-issuance uniqueness/alphabet check and a reflection-based guard that `CredentialVerificationDto` carries only `Valid`/`MemberName`/`MembershipType`/`IssuedOn`/`Status`; `CredentialVerificationControllerTests` covers a revoked code still returning 200 with `Valid: false`, and a malformed code short-circuiting before the service is called. Verified 2026-09-23 against the WP37.11 gate (see 37.2's Acceptance line for the run figures).

37.9 [TODO] **Priority: P4.** Specified in
`docs/specs/003-alumni-programs-and-verification/spec.md` Story 5.
**Geographic chapters.** `Chapter` (`Id`, `Name`, `Region`, `Country`, `City`, `Description`, `CoordinatorMemberId?`, `ContactEmail?`, `IsActive`, `CreatedAt`) and `ChapterMembership` (`Id`, `ChapterId`, `MemberId`, `JoinedAt`, `IsCoordinator`) with a unique index on `(ChapterId, MemberId)`.
  - **Reuse, do not rebuild.** A chapter event is an `AlumniEvent` with a `ChapterId` — add the nullable column to `AlumniEvent` rather than creating a `ChapterEvent` table. Chapter announcements reuse `INotificationService.CreateNotificationAsync` fanned over the chapter's members; there is no new messaging surface in this item.
  - **Service/API/UI.** `IChapterService` + `ChapterService`; `ChaptersController` at `api/chapters`, `GET api/chapters/public` `[AllowAnonymous]`. Flag `enableChapters`. Public `/chapters` (list + detail with coordinator contact), member `portal/chapters` (join/leave, my chapter feed), admin `admin/chapters`. New `API_ENDPOINTS.CHAPTERS` block.
  - **Tests.** NUnit: joining twice does not duplicate; a chapter event appears only in that chapter's feed; coordinator contact is hidden from the anonymous projection unless `ContactEmail` is set. Vitest: join/leave state, empty-chapter state.

37.10 [TODO] **Priority: P3.** Specified in
`docs/specs/003-alumni-programs-and-verification/spec.md` Story 2.
**Annual impact report generated from the ledger** — the accountability artifact that closes the loop on 37.2, 37.3 and 37.4, and the reason those three back-link `FinancialRecordId`.
  - **Nothing in this item is hand-typed.** For a given year it aggregates: total income and expense by `FinancialCategory` from `FinancialRecord`; scholarships awarded and disbursed from `ScholarshipAward`; campaign totals and donor counts from `CampaignPledge`; events and attendance from `AlumniEvent` + `EventRegistration`; new members from `Member.CreatedAt` / `MembershipHistory`; reunions from 37.4. The only authored fields are a president's foreword and a cover image, stored in the existing `SiteContent` CMS from Work Package 34 — **do not add a table for two strings.**
  - **Output.** Server-side PDF via **QuestPDF**, following the `IDCardService.GenerateIDCardPdfAsync` pattern, plus an on-site HTML view reusing the Work Package 36 `.doc-hero` / `.doc-prose` shell. Extend `IFinancialLedgerService` with `Task<ImpactReportDto?> GetImpactReportAsync(int year, CancellationToken cancellationToken = default)`, and put the PDF method on the existing document-generation surface rather than inventing a third document service.
  - **Guard.** The report must degrade rather than throw when a source feature is not yet built or its flag is off — a year with no campaigns renders without that section. This item is therefore safe to build *before* 37.2/37.3/37.4 land, and must read every source through a null-tolerant projection.
  - **API/UI.** `GET api/financials/impact/{year}` and `GET api/financials/impact/{year}/pdf`, both `[AllowAnonymous]` (publishing it is the point). Flag `enableImpactReport`. Public `/impact/:year` with a year selector; admin action to set the foreword and publish. Additions to the existing `API_ENDPOINTS.FINANCIALS` block.
  - **Tests.** NUnit: a year with zero records returns a report with zeroed sections rather than null; category totals match a hand-summed fixture; the disbursed-scholarship total equals the sum of the linked `Grant` `FinancialRecord` rows. Vitest: year selector, empty-section rendering.

37.11 [TODO] **Priority: P2.** Per 12.6, nothing in Work Package 37 is `[DONE]` until `dotnet test`, `npx vitest run`, `npm run type-check` and `npx ng build` all pass. Baselines to beat at the start of this Area: **351 NUnit tests** and **64 vitest files / 306 tests**. Additionally, every item that adds a table must be verified against the 37.0 migration path on a **non-empty** database — a passing suite against a fresh SQLite file proves nothing about preprod (`gotcha_ensurecreated_no_op_existing_db`). Update `docs/FEATURES.md`, `docs/PROJECT_MAP.md`, `docs/SRS.md` and `docs/ARCHITECTURE.md` as each item lands, per `feedback_docs_update_scope`. Reviewed 2026-09-17: stays open. This is a standing gate on the rest of the Area, not a one-time task — it can't be marked done while 37.2-37.10 are still unbuilt.

---

# Work Package 40 — Member albums with admin approval, job-posting approval, events without registration (raised by user 2026-08-26)

Plan: `C:\Users\HabiburRahmanShalin\.claude\plans\piped-sniffing-lollipop.md`. `EventGallery`/`EventPhoto`
extended in place for member ownership (no new `Album` table); admin = `Admin`/`SuperAdmin` role;
legacy dead `POST api/gallery` "submit a memory" endpoint fixed separately from the new album flow.

> **STATUS: Work Package 40 is COMPLETE (backend + web + mobile).** 40.1-40.10 below are the original
> plan items; each was superseded by the 40.11-40.14 delivery entries at the end of this Area and
> re-marked accordingly on 2026-08-28. They were left as stale `[TODO]` for two days, which made
> shipped work read as outstanding — verify against the tree before trusting a marker
> (`gotcha_todo_status_drift`).

42.1 [TODO] **Priority: P3.** Explore/plan (Plan Mode required — multi-file, touches Domain/Application/Infra/API/Web
admin+public/Mobile): inventory exactly what's static today under the elections feature (entities,
seeders, controllers, public/portal components) before designing the admin-editable model — do not
assume it mirrors SiteContent without checking field/document shape differences (forms likely need
file/PDF attachments, not just rich text).
42.2 [TODO] **Priority: P3.** Design admin CRUD screens + API for whatever the inventory in 42.1 finds (forms list,
per-form fields/attachment, publish state) following `ghcaa-design` conventions.
42.3 [TODO] **Priority: P3.** Web public/portal elections pages read from the new admin-managed source instead of the
seeder/static content.
42.4 [TODO] **Priority: P3.** Mobile: sync if elections content is surfaced there.
42.5 [TODO] **Priority: P3.** Tests + docs update per usual closing convention.

---

# Work Package 43 — Application-wide exception handling & logging audit (raised by user 2026-08-27: "make sure entire application have propers exception handling with logging. best error management")

47.13 [TODO] **Priority: P2.** **Mutation (POST/PUT/DELETE) coverage remediation — task breakdown.** 47.9 closed the top
2 items (`RolesController.DeleteUser`, `AdminGovernanceController.DeleteECMember`). Remaining ~35%,
broken into independently-completable tasks below. Common approach for all of them: one new
`GHCAA.Tests/Controllers/*Tests.cs` file per controller, mocking the underlying service interface
(same pattern as `DestructiveStepUpActionsTests.cs`/`GalleryControllerTests.cs`) — one success-path
test + one failure-path test per action is the target depth; this is breadth-over-depth work, not
deep edge-case testing. Run `dotnet test` after each task, not just at the end.

47.13.1 [DONE 2026-09-04] **`AuthController` non-Login actions** (highest priority — the security surface, and
this session's own new step-up endpoints are among the untested ones): `GoogleLogin`, `FacebookLogin`,
`Refresh`, `RefreshMobile`, `Logout`, `RequestStepUp`, `VerifyStepUp`, `ResetPassword`. New file
`AuthControllerMutationTests.cs`.

`GHCAA.Tests/Controllers/AuthControllerMutationTests.cs` added, mocking `IAuthService`/`ITokenService`/
`IOtpService` directly with Moq and reusing `ControllerTestBase.SetUserContext` for the claims-principal
setup `Refresh`/`Logout`/`RequestStepUp`/`VerifyStepUp` need, per this project's existing controller-test
pattern (`GalleryControllerTests.cs`, `DestructiveStepUpActionsTests.cs`). 20 tests: one success-path
and one failure-path test per action, plus extra failure cases for `Refresh`/`RefreshMobile` (no cookie,
rotated-but-invalid token, inactive/archived user are genuinely distinct failure modes) and for
`RequestStepUp`/`VerifyStepUp` (missing email-on-file vs. an invalid/expired OTP code). All 20 pass, and
the full suite (`dotnet test GHCAA.Tests/GHCAA.Tests.csproj`) is green at 564/564.

47.13.2 [DONE 2026-09-04] **`LookupsController` full CRUD** (`CreateLookup`/`UpdateLookup`/`DeleteLookup`) — zero
coverage today, controls dropdown/lookup master data; same "silent bad data" risk class as this
Area's seed-integrity bugs (47.1). New file `LookupsControllerTests.cs`.

`GHCAA.Tests/Controllers/LookupsControllerTests.cs` added, covering every action on the controller
rather than just the three named above (`GetPublicStats`, `GetAllLookups`, `GetByGroup` included for
completeness): one success-path and one failure-path test per action, 12 tests total, mocking
`ILookupService`/`IMemberService` directly with Moq per this project's existing controller-test
pattern (`GalleryControllerTests.cs`, `DestructiveStepUpActionsTests.cs`). All 12 pass, and the full
suite (`dotnet test GHCAA.Tests/GHCAA.Tests.csproj`) is green at 564/564.

47.13.3 [DONE 2026-09-06] `GHCAA.Tests/Controllers/RolesControllerTests.cs` added, covering
`CreateAdmin`, `CreateRole`, `AssignRole`, `RemoveRole`, `GetUsers`, `GetRoles` (success + failure
path each), plus `DeleteUser`/`DisableUser`/`EnableUser`/`ResetPasswordAdmin` landed here too as part
of 49.5. Folded into the same file/PR as instructed there.

47.13.4 [DONE 2026-09-16] **`AdminPollController`** (`DeletePoll`, `ToggleStatus` — `CreatePoll` already covered). Added one controller-contract test class instead of duplicating `PollServiceTests`: it covers the list response, propagation of the acting member claim on create, both active states, a missing toggle target, and both delete outcomes. Focused run: 7/7 passed.

47.13.5 [DONE 2026-09-06] **`PaymentConfigController`** (`Create`, `Toggle`, `Delete` —
`Update`/`SeedDefaults` already covered per the mutation-coverage audit). Added as part of 80.13's
sweep, since that item rewrote `PaymentConfigController` to depend on the new `IPaymentConfigService`
anyway: `CreateConfig_PersistsAndReturnsMaskedSecrets`, `ToggleConfig_FlipsIsEnabled`,
`ToggleConfig_ReturnsNotFound_WhenConfigDoesNotExist`, `DeleteConfig_RemovesConfig`,
`DeleteConfig_ReturnsNotFound_WhenConfigDoesNotExist` in `PaymentConfigControllerTests.cs`.

47.13.6 [DONE 2026-09-18] Added one success + one failure test per action for all four
controllers. `AdminControllerTests.cs`: `SyncMembers`, `BulkArchiveInactive`, `RestoreMember`,
`UpdateMemberPhoto`, `UpdateMemberSignature`, `UpdateMemberDocuments` (12 tests). `GalleryControllerTests.cs`:
`UploadPhoto`, `ToggleActive`, `ToggleFeatured`, `SubmitMemberPhoto` (8 tests). New file
`CommunicationControllerTests.cs`: `CreateTemplate`, `UpdateTemplate`, `DeleteTemplate` (6 tests,
all via the throws-propagates pattern since none of the three have a NotFound/validation branch
of their own). New file `FamilyLinkControllerTests.cs`: `Remove`, `Cancel` (4 tests). **Naming
correction:** the `CancelRequest`/`UnlinkMember` names in the original text don't exist on
`FamilyLinkController` — the controller only has `Remove` and `Cancel`; those are the two that got
tests. **Correction 2026-09-06:** the original text also named `FamilyController` here; that
controller was deleted in 80.2 (2026-09-04) and no longer exists — dropped from this item rather
than left as a dead reference.

47.13.7 [DONE 2026-09-18] The two gaps flagged by the interrupted re-audit are closed:
`CommunicationControllerTests.cs` gained `SendBatch`, `SendType`, `SendCustom` (6 tests,
success + 400-validation-branch pairs), and `FamilyLinkControllerTests.cs` gained `Send`,
`Respond` (4 tests, success + not-found pairs). Full suite confirmed via `dotnet test`:
841 passed, 0 failed, 0 skipped-that-matter. No other `[HttpPost]/[HttpPut]/[HttpPatch]/
[HttpDelete]` actions were found uncovered in the same pass; this closes the 47.13 series.

# Work Package 48 — Full security audit (raised by user 2026-08-29: "plan for vulnurability check, check for
web security best paractices")

A `security-reviewer` subagent audit of the whole app (verifying prior S1-S9/Work Package 24 hardening is
still genuinely wired, and hunting for anything new) found 2 Critical, 4 High, 4 Medium, and several
Low findings. All code-fixable items below are done (511/511 backend tests green, `ng build` clean);
the two Critical items include work the user must do outside this codebase (external secret rotation).

## Round 2 — OWASP Top 10 gap-fill audit (2026-08-29, raised by user: "make sure OWASPs are covered")

Round 1 covered A01 (Access Control), A07 (Auth Failures), and SQL injection in depth. This round
targeted the categories round 1 didn't verify: A02 (crypto/headers), A03 (frontend XSS), A05
(misconfiguration), A06 (component versions), A08 (integrity), A10 (SSRF). 511/511 backend tests
and 352/352 frontend tests green after all fixes; `ng build`/`type-check` clean.

---

# Work Package 60 — Mobile parity plan for this session's portal changes (raised by user 2026-08-31: "plan
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
60.4 [TODO — tracked in 82.84] **Priority: P1.** Mobile: Complete authenticated integration verification on a supported Flutter device/toolchain for the recorded environment blockers. Use 82.84 as the canonical execution record.
60.5 [TODO — tracked in 82.85] **Priority: P2 | Depends on: 60.4.** Mobile: Correct integration credentials and deterministic financial/forum coverage gaps. Use 82.85 as the canonical execution record.

## WORK PACKAGE 62: INSTITUTION-AGNOSTIC / WHITE-LABEL PLATFORM (raised by "make this application generic rather than GHC ... will work with GHC or any other institution with minimal configuration changes", 2026-09-01)

> Reference doc: docs/WHITE_LABEL_PLAN.md (architecture decisions, full hardcode audit,
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
> Work Package 28 already shipped the runtime plumbing (OrganizationConfig entity, /api/config, Angular
> APP_INITIALIZER, Flutter Riverpod, feature guards, SiteContent CMS). Work Package 62 is NOT a rebuild of
> that. It replaces the three hardcoded GHC default blocks feeding it, and closes the long tail of
> literals that never went through the config path.
>
> HARD CONSTRAINT on every item below: the existing GHC deployment must behave identically after the
> change. See plan section 8 for the seven specific traps (data-protection app name, issued
> membership numbers, mobile bundle id, enum int values, live URLs, localization self-heal, seed path
> vs migration bootstrapper).
>
> **WORK PACKAGE 61 IS FOLDED INTO THIS AREA, NOT RUN BESIDE IT.** Work Package 62 touches most of the files Work Package 61
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

62.2 [PARTIAL 2026-09-16] `org-config.json` and `seo.json` and `site-content.json` (hyphen-named)
now exist for both `profiles/default/` and `profiles/ghc/` — `seo.json` drives
`scripts/apply-brand.mjs`'s SEO meta tags, `site-content.json` drives
`scripts/generate-site-content.mjs` → `site-content.generated.ts`. `documents.json`'s governing-
document registry is now covered too, folded into `org-config.json` as `Documents` rather than a
separate file (62.22). Remaining unbuilt: `email_templates.json`, `site_content.json` (the
underscore-named backend seed pair — `ApplicationDbContext.ProfilePackFiles` already gates for
them, but no per-profile files exist), `themes.json`, and `membership-tiers.json`/
`governance.json`. `lookups.json` stays out of scope by design — dropdown categories are shared
taxonomy across every institution, not a per-profile override. `assets/`/`demo-data/` folders:
still deferred to the phase that needs each one. Building schema for a consumer that doesn't
exist yet is the premature-abstraction risk the project's own rule warns against.

### PHASE B: API DE-BRANDING

### PHASE C: WEB DE-BRANDING

(62.22 done 2026-09-16 — see docs/TODO_ARCHIVE.md)

### PHASE D: MOBILE DE-BRANDING

### PHASE E: DATA, TIERS, GOVERNANCE

### PHASE F: ONBOARDING, OPS, PROOF

62.41 [PARTIAL 2026-09-06] **Priority: P1 | Depends on: 62.6, 62.15, 62.27 (all done, so this was
unblocked).** Acceptance test written: `GHCAA.Web/tests/e2e/generic-profile-acceptance.spec.ts` +
its own `playwright.generic.config.ts` (kept separate from the normal suite on purpose — it needs a
server booted with `ORG_PROFILE=default` against a database that has never run a migration, which
the everyday dev server doesn't provide, and it self-skips unless
`PLAYWRIGHT_GENERIC_BASE_URL` is set so it can never fail the normal CI run by accident). Walks
landing page, `/api/config`, register, login as the `default` profile's demo member, portal, digital
ID card, and the admin shell, asserting none of them render "GHCAA", "Govt. Haraganga College",
"Haraganga", or the Bengali name.
**Not run against a real empty database — that would mean provisioning one, which wasn't asked for.**
Written and confirmed to compile and load (`npx playwright test --list`); not executed end to end.
**Two reasons it would fail today, one already tracked and one new:**
1. The known one (62.31/82.31): 8 committed migrations still bake 631 real GHC alumni via
   `InsertData` regardless of `ORG_PROFILE`, so "an empty database" isn't actually empty.
2. ~~New, found while writing this test: no bootstrap created an initial SuperAdmin account on a
   fresh database.~~ **Resolved 2026-09-16 as 62.50**: `ProtectedSuperAdminSeeder.BootstrapFirstSuperAdminAsync`
   now creates the first protected username with a generated password on a database that holds no
   SuperAdmin yet, wired into `DatabaseBootstrapperExtensions.cs`, covered by
   `ProtectedSuperAdminSeederTests.cs`.
**Still blocked on reason 1 alone.** 62.31 stays ONHOLD by decision (see line 212) — the git-history
rewrite and real-data externalization it needs haven't happened, so a fresh clone still applies the
eight already-committed migrations that `InsertData` the 631 real rows regardless of `ORG_PROFILE`.
This item can't go green until that rewrite runs.
This is the item that proves the area is done, so it stays open until it's actually green, not just
written.

62.42 [TODO] **Priority: P2 | Depends on: 62.41.** Flip `brand-lint` from warn-only to blocking in
CI. Inherits 62.41's block: it can't run until 62.41 is green, and 62.41 can't go green until 62.31's
git-history rewrite happens.

### PHASE G: WORK PACKAGE 61 CARRY-OVER (runs inside phases A-F, not after them)

> These four items are the mechanism that folds Work Package 61 into Work Package 62. They are not a separate pass at
> the end. Each one is checked off per phase, and the phase is not done until its slice is done.

63.8 [TODO] **Priority: P3 | Depends on: nothing.** Re-take the repository figures listed in
`docs/book/README.md` under "Keeping the numbers true" immediately before any submission, and correct
the sentences that carry them. They were taken on 2026-09-01 and go stale with every feature.

63.10 [TODO] **Priority: P3 | Depends on: 63.6.** When Chapters 7-13 are written, every new figure
goes through the same gate: draw it, run `--audit`, fix the shape rather than marking `{landscape}`,
then `renumber.py --apply`. The fix order is in `docs/book/README.md` under "Fitting A4". Do not add a
figure to a chapter without a sentence in the body that names it, or the build will fail.

63.18 [TODO] **Priority: P3 | Depends on: 63.14.** Front matter is numbered in Arabic with the body,
not lower-case Roman as `DOCUMENTATION_BOOK_OUTLINE.md` specifies, and the title page carries a folio.
Chrome's footer template is one template for every page, so neither can be varied. Closing this needs
two prints (front matter and body, each with its own template) merged into one file, which needs a PDF
library beyond the standard library. Worth doing only if a supervisor asks for it.

64.8 [TODO] **Priority: P2 | Depends on: 64.7.** Revise §4.8 to carry risk exposure **RE = P × C** and
impact on the 1–5 scale, which is the convention the course material uses. The probabilities are
already there; the impact costs are author-stated and blocked on 64.7.

64.9 [TODO] **Priority: P2.** Chapter 11 figure set is seventeen per-component activity diagrams plus
six chapter-level charts, not one module network. A seventeen-node network with nine edges converging
on the web client prints at about 4pt, well under the enforced 7pt floor. Per-component diagrams also
sit beside the prose for their own component, which is where a reader wants them.

65.5 [TODO] **Priority: P3.** Within Chapter 6, §6.11 Design Principles (twelve subsections) and §6.12
Patterns (seven) come after §6.8 to §6.10 on user interface, mobile and configuration. Principles are
more fundamental than the specific designs that apply them, so the conventional order would put them
first. Not done: reordering sections inside a written chapter renumbers a large share of the 84 §6
references for a modest gain. Worth doing only if the chapter is revised for another reason anyway.

# Work Package 67 — The remaining chapters: nothing of Work Packages 63 to 66 reaches a reader yet

Raised by user 2026-09-02, on being told Work Package 64 was complete: "doesn't those going in book? where?"
A fair question, and the answer is that it does not, yet.

67.2 [PARTIAL 2026-09-16] **Priority: P1.** Chapter 11 is done: every section is written, every
number in it traces to a command run against the tree or `docs/book/build/wbs.py` (`git rev-list
--count HEAD`, `grep -rhoE '\[(HttpGet|HttpPost|HttpPut|HttpDelete|HttpPatch)' GHCAA.Api/Controllers
--include=*.cs`, `grep -c "public DbSet<" GHCAA.Infrastructure/Data/ApplicationDbContext.cs`, `wc -l
docs/TODO.md`, all run 16 September 2026), and `build.py --pdf --strict` ends `status : clean, ready
to deliver`. Two places a number could not be sourced honestly are left as `*[` placeholders rather
than guessed: the hours-per-function-point rate needed to turn §11.4's 1,902 adjusted function points
into an effort or cost figure, and the per-component completed-task-count breakdown §11.4.1 first
set out to compare against commit-days. Chapter 8 is done too, as of 16 September 2026: all 15
sections written, all 6 figures and 5 tables from the outline drawn, and `build.py --pdf --strict`
ends `status : clean, ready to deliver`. The remaining four chapters are not: as of 16 September
2026, `build.py --strict` reports 105 open placeholders across them (`07-implementation.md` 17,
`09-verification.md` 44, `10-deployment.md` 16, `12-results.md` 16, `13-conclusion.md` 12). Chapter
11 went first because `wbs.py` prints its activity table, critical path and arrival profile from git
and this file, so writing it was prose around generated numbers rather than gathering anything;
Chapter 8 went next as the WP82.6 refactor left it well scoped; the remaining four still need that
gathering done chapter by chapter.

67.3 [TODO] **Priority: P2 | Depends on: 67.2.** Seventeen per-component activity diagrams (64.9)
plus the six chapter-level charts, for chapters 7 to 13. Confirmed still at zero: none of the seven
new chapter files contains a single ```mermaid``` block (all 57 existing diagrams belong to chapters 1
to 6). They cannot be drawn before the surrounding prose exists to hold them, and under Work Package 66
every one of them has to survive the overprint and clipping checks, which the two quadrant charts and
the DFD did not on first attempt.

67.4 [TODO] **Priority: P2.** The reference list still carries seventeen entries no written chapter
cites (confirmed unchanged 2026-09-05: `build.py --strict` reports the same seventeen numbers). They
are not stale: each was collected for a chapter in Part III or IV. The build reports them as expected
while those parts are unwritten, and `--final` is the gate that stops accepting that excuse. Do not
delete a reference to quieten the report; write the chapter that uses it, or remove it deliberately
with the reason recorded here.

72.4 [TODO] **Priority: P3.** Watch the first Dependabot run. Five ecosystems opening at once
produces a burst even with the caps, and the grouping rules are a guess until they have been seen
against a real week's updates. Adjust the caps or the groups if the burst is unmanageable rather than
turning Dependabot off.

72.5 [TODO] **Priority: P3 | Depends on: user.** If the repository is ever made public, CodeQL becomes
free and 72.2 is worth revisiting; §10.4.2 would then need rewriting, since it currently states cost
as the reason static analysis is absent.

# Work Package 73 — Traceability and constitutional alignment: what is true, and what was claimed

<!-- wbs: component=C17 start=2026-09-03 end=2026-09-03 after=71 -->

Raised by user 2026-09-03 with six statements about requirements traceability, domain constraints,
verification of constraints, dependency mapping, an audit trail and non-functional constraints, and
an instruction to sync the docs. Each was checked against the tree before anything was written.
Three are already true and documented; three describe a system stronger than the one that exists, and
writing them up as fact would have put an unverifiable claim into a document whose whole argument is
that its claims are checkable.

73.5 [TODO] **Priority: P2 | Depends on: 73.4.** Generate the traceability matrix as a repository
artefact from those tags, the way `wbs.py` generates the Chapter 11 tables. Then §3.9's original
claim becomes true and can be restored, and §12.2 can close the matrix from evidence rather than by
reading.

73.6 [TODO] **Priority: P2.** "Constitutional rules fail the build if they drift from the governing
text" is the user's description and is not what happens today. `ConstitutionSeeder` synchronises the
stored constitution with the source document at boot, and the business rules have tests, but nothing
compares the two: the seeded text could diverge from the ratified PDF, or a rule could be changed
away from its clause, and the suite would stay green. A narrow, honest version is cheap — a test that
pins the seeded constitution's version and a hash of its clause text, so that changing the governing
data without changing the test is a failure. That is worth building, and it would let Chapter 8 and
Chapter 9 make a claim no commercial CRM comparison in Chapter 2 can match. Not built unasked,
because it is application code rather than documentation.

73.7 [TODO] **Priority: P3.** Two of the user's statements have no counterpart in the system and are
recorded here so they are not written up by mistake. There is no mapping of requirements to
dependency trees; the dependency evidence in the book is the assembly-level structure matrix of
Figure 7.2, which is a different thing. And there is no immutable per-requirement version history
recording who authorised a change: `docs/TODO.md` and the git history together carry when and why,
but not an authorisation record, and the Association has no change-control board to authorise
anything (§11.7 says so).

74.3 [TODO] **Priority: P3.** Backfill markers for Work Packages 1 to 67. Not done in bulk: a marker asserting
a start date is a claim about when work happened, and for most of those areas the honest source is the
commit record `wbs.py` already reads. Worth doing only where an area's real dates differ from its
commit dates, which is the case for the research and review work of U1 to U5.

75.5 [TODO] **Priority: P2 | Depends on: user.** The five PRE durations are the author's, not the
script's. Ten days for the governing-document study and eight for requirements analysis are the two
worth challenging: they set the pre-development total and therefore the person-month figure the
dissertation prints. Confirm or correct them the way 64.7 asks for the assumption rates.

# Work Package 76 — Sizing the delivered system, and the reuse that paid for it

<!-- wbs: component=C17 start=2026-09-03 end=2026-09-03 after=75 -->

Raised by user 2026-09-03: the whole implementation should come out at around 350 working days for the
book, made realistic by reusable components, AI and rapid development tooling, and code carried over
from earlier projects.

76.4 [TODO] **Priority: P2 | Depends on: user.** The four reduction factors and the four production
rates are the author's judgement. The two worth challenging first are the web rate of 25 lines an hour,
which carries the largest single block of source, and the ×0.80 for AI-assisted tooling, which is the
factor an examiner is most likely to ask about in a viva. Confirm or correct, as with 64.7 and 75.5.

77.4 [PARTIAL 2026-09-03] **Priority: P2 | Depends on: user.** The provenance is answered; the
proportion is not.

  **Answered.** The carried-over material comes from the author's own projects of **2019 to 2024**,
  and the parts are named: authentication, the front-end data grid, backend structure and base
  services, file handling, payment-gateway adapters and deployment configuration. The date window is
  what makes the factor credible rather than decorative — 2019 to 2024 is the .NET Core, Angular and
  Flutter generation, the same as this platform, so the material carried over as working code rather
  than as a design to be reimplemented. Twenty-year experience explains the judgement; a five-year
  window explains the code. Recorded in `wbs.py`, §11.0, §11.5.2, §11.5.4 and §12.11.

  **Still open.** Roughly what share of the delivered code has an ancestor in that library. Even a
  banded answer — a quarter, a third, a half — would let §11.5.2 replace the ×0.78 judgement with a
  measured proportion, which is the difference between a defensible section and an evidenced one.

78.9 [TODO] **Priority: P1. Depends on 78.4.** Administer the evaluation sessions: ordinary members
plus the three office holders, at least half of the member sessions on a handset, against the
pre-production deployment, one response log per participant.
**Acceptance:** a completed response log per participant in `docs/book/instruments/responses/`, no
participant name anywhere in the folder, and no session written up into §12.6 or §12.7 until every
session is complete. Partial write-up produces a chapter that argues for whoever was interviewed
first.

78.10 [TODO] **Priority: P1.** Produce the evidence that requires no participant: summarise the
coverage data already in `GHCAA.Tests/TestResults/`, run static analysis for complexity, coupling and
maintainability index, take the latency measurements defined in §4.5.3, and complete the ASVS 4.0.3
walkthrough control by control for §8.13.
**Acceptance:** each figure traceable to a command that can be re-run against the tree, with the date
of the run recorded, per the repository-numbers rule in `CLAUDE.md`.

78.12 [TODO] **Priority: P2.** Chapter completion order, recorded so it is not re-argued: Chapter 11
first, being the only chapter whose figures `wbs.py` already computes and which is blocked on nothing;
then 9; then the evidence of 78.9 and 78.10; then 12, 7, 8, 10, 13, the abstract, and a final
consistency pass. This departs from the review brief, which scheduled 11 near the end.

---

79.3 [DONE 2026-09-17] Swept `docs/*.md` (53 files under `docs/`, including `docs/adr/`,
`docs/Elections/` and `docs/materials/`) and the three root-level markdown files. The four
priority documents were read in full:
- `README.md` — already plain; no changes needed.
- `SRS.md` — reworded the Clean Architecture line (§2) from "to ensure maintainability and
  scalability" to "so business logic stays independent of frameworks and databases", matching the
  phrasing `README.md` already uses for the same claim. Left the Vision statement (§1.3) as
  written — it describes an actual product decision (the "Midnight Gold" brand), not filler.
- `FEATURES.md`, `RENDER_DEPLOYMENT.md` — already plain; no changes needed.
- `CLAUDE.md`, `GEMINI.md` — already plain; no changes needed. (`GEMINI.md` describes an older
  stack — .NET 8, Angular 19, SQLite/SQL Server — than the current one, but that's a content-drift
  issue, not a tone one, and out of scope here.)

Ran `lint.py`'s 47-entry banned-word list as a grep filter across the rest of `docs/*.md` to find
likely violations without reading all 10,885 lines by hand. Three real hits, fixed:
- `docs/PROJECT_MAP.md` — "Comprehensive Form Controls Map" (§23 heading and ToC entry) renamed to
  "Form Controls Map — Auth, Profile & Community", naming the three subsections it actually has
  instead of the empty "comprehensive".
- `docs/UI_FIX_PLAN.md` — "the Work Package 33 holistic pass" reworded to "the Work Package 33
  full-app review", naming what that pass covered.
- `docs/DOCUMENTATION_BOOK_OUTLINE.md` — Figure 11.24's caption still said "leverage" where
  `docs/TODO_ARCHIVE.md` (76.9) records that word was already replaced with "factor" everywhere
  else, including the outline table headers; this one caption was missed. Fixed to match. This was
  a genuine drift bug, not just a style flag.

The remaining grep hits were left alone: literal filenames (`comprehensive_visual_freeze_test.dart`),
a correct technical term ("robustness bug" in `BUSINESS_FINDINGS.md`), and closed historical
entries in `docs/TODO_ARCHIVE.md`/`docs/TODO_ACTIVITY_TITLES.md` — those are records of what was
done, not live prose, and rewriting them would falsify the record.

Deliberately left un-swept, and why:
- `docs/TODO.md`, `docs/TODO_ARCHIVE.md`, `docs/TODO_ACTIVITY_TITLES.md`,
  `docs/TODO_IMPLEMENTED_MISSING.md` — the tracker already has its own voice rule (SR-4, analyst
  voice) and closed entries are a record, not prose to rewrite after the fact.
- `docs/materials/*.md`, `docs/BACKEND_REVIEW_2026-07-03.md`, `docs/COVERAGE_SNAPSHOT_2026-05-26.md`,
  `docs/ARCHITECTURE_AUDIT_2026-09.md` — dated snapshots/review prompts captured at a point in time;
  `docs/materials/REVIEW.md` in particular is a review prompt someone else wrote, not project prose.
- `docs/Elections/*.md` — the seven election documents are formal/legal instruments with fixed
  wording (regulations, ballots, certificates); rewording them for tone risks changing their legal
  meaning, and they're outside what task 79.3 means by "docs a person outside the project reads".
- The remaining internal planning/reference docs (`ARCHITECTURE.md`, `API_CONTRACT_REGISTRY.md`,
  `BUSINESS_FINDINGS.md`, `BUSINESS_REVIEW_PLAN.md`, `BUSINESS_TEST_CHECKLIST.md`,
  `CONFIG_DRIVEN_FRAMEWORK.md`, `CONSTANTS_CLASSIFICATION.md`, `CONSTITUTION_PUBLISHING.md`,
  `ENV_REVIEW.md`, `FORUM_PLAN_2026-05.md`, `implementation_plan.md`, `INSTITUTION_ONBOARDING.md`,
  `PAYMENT_GATEWAY_WORKFLOW.md`, `RECOVERY_RUNBOOK.md`, `SEED_CLASSIFICATION.md`,
  `SHARED_PROFILE_COMPONENTS.md`, `TEST_COVERAGE_PLAN.md`, `WHITE_LABEL_PLAN.md`, `docs/adr/*.md`)
  cleared the banned-word scan with no hits; left as is rather than rewritten line by line with
  nothing concrete driving a change.

79.4 [DONE 2026-09-17] **Priority: P2. Depends on 62.49.** Code comments, roughly 3,684 lines carrying
`//` across the backend, web and mobile sources. The retroactive tone rule already applies to any
file a change touches, and the standing sweep is folded into Work Package 62 rather than run beside
it, so this item is a widening of scope rather than new work: 62's close-out audit checks for the old
AI-tells and must now also check construction.
**Resolved 2026-09-17:** widened 62.49's close-out entry in `docs/TODO_ARCHIVE.md` with a full
`//`-comment count run against the tree (Backend 2,252, Web 847, Mobile 648, total 3,747) and a
construction check layered on top of 62.49's original AI-tell grep. Two genuine hits, both fixed:
`GHCAA.Infrastructure/Services/MemberImportService.cs:168` and
`GHCAA.Web/src/app/admin/gallery/admin-gallery.ts:80` — both were "Robust ..." headings restated to
say what the code actually does. `dotnet build` (0 errors), `npx tsc --noEmit` (clean) and
`flutter analyze` ("No issues found!") all confirmed clean afterward. 62.49's recorded counts now
carry the widened rule, per the acceptance criterion below.
**Acceptance:** 62.49's recorded file counts include the widened rule, so the sweep stays provable
rather than asserted.

81.1 [DONE 2026-09-21] **Priority: P2 | Depends on: none.** Added member-scoped and admin per-member
communication history with channel, delivery status, message detail, pagination, and targeted/broadcast
classification. Email, configured SMS-template, and workflow SMS sends retain the recipient member id.
SMS templates use the SMS provider and record unavailable delivery when no mobile number is available.
Verified with the CommunicationService test set, API build, Angular production build, and Flutter analysis.
**Acceptance:** `GET /api/communications/me` and `GET /api/admin/comm/member/{memberId}` enforce their
respective authorization boundaries and return paginated redacted error details.

81.3 [DONE 2026-09-21] **Priority: P3 | Depends on: 81.1, 81.2.** Added member portal and mobile
communication history routes. The existing admin communication surface can switch to a per-member view
with its `memberId` query parameter, while the global log remains available.
**Acceptance:** Web and Mobile both consume the member history endpoint and their targeted builds/checks pass.

82.59 [TODO] **Priority: P2 | Depends on: 82.58.** Election handbook profile-awareness audit.
Investigate all markdown files under `GHCAA.Web/public/assets/elections/` (01–08) for hardcoded
institutional references (institution name, contact details, specific role titles, election dates).
Determine whether each reference should be (a) replaced with a template placeholder resolved at
build time by `apply-brand.mjs`, (b) moved to `profiles/<name>/` as an overridable asset, or (c)
left as GHC-specific policy text that is correct to be hardcoded. Update `brand-lint.mjs` patterns
to catch any newly templated tokens. Mirror the same audit to `docs/Elections/` (01–08) which serve
as the source-of-truth drafts. Add the `default` profile equivalents for any file moved to the
profile pack.

(82.115 done 2026-09-18 — see docs/TODO_ARCHIVE.md)

---

# Work Package 83 — Registration transaction bug found via the error-logs table

<!-- wbs: component=C3 start=2026-09-16 end=2026-09-16 after=43 -->

Found while closing 43.4's live-verification pass: the same `ErrorLogs` query used to confirm the
diagnostic trigger also returned a real, pre-existing row. `ErrorLogs.Id=1`, dated 2026-09-13, logs a
genuine production failure at `/api/auth/register`: "This NpgsqlTransaction has completed; it is no
longer usable." Not a diagnostic artifact — a real registration attempt hit this. Recorded here rather
than folded into 43.4, since 43.4 was about the logging pipeline working, not about this specific bug.

---

# Work Package 84 — Workflow contracts, validation, and endpoint detail from the specification review

<!-- wbs: component=C17 start=2026-09-21 end=2026-09-21 after=73 -->

Raised by an external review of `docs/specs/001-platform-baseline`'s evidence files. The review's
specific claims trace to gaps the baseline already records: no centralized state-transition table for
events, news/article approval, jobs, mentorship, payments, or polls; no single validation matrix
across write DTOs; and no exhaustive endpoint contract table for the API surface. The scope is drafted
in `docs/specs/002-workflow-contracts-and-validation/`, per baseline task T018, which keeps new
specification work in its own numbered directory rather than editing 001.

84.1 [DONE — 7-domain scope] **Priority: P1 | Depends on: none.** Build the state-transition table set
(events registration, news/article approval, job moderation, mentorship, payments, polls, governance
periods) from the actual enums and services, per `docs/specs/002-workflow-contracts-and-validation/spec.md`
Story 1. **Acceptance:** one table per domain, each enum value and transition traced to its triggering
endpoint/service method and required role, with unreachable values marked rather than omitted. All 7
domains named in spec.md Story 1 are done: `evidence/events-domain.md`, `news-domain.md`,
`jobs-domain.md`, `mentorship-domain.md`, `payments-domain.md`, `polls-domain.md`,
`governance-domain.md`. Scope is capped to the controllers/services backing these 7 domains, not all
35 `GHCAA.API` controllers — see `docs/specs/002-workflow-contracts-and-validation/tasks.md` T007's
scope note.

84.2 [DONE — 7-domain scope] **Priority: P1 | Depends on: none.** Build a validation matrix covering
every write DTO accepted by a POST/PUT/PATCH action across the 7 in-scope domains, per spec.md Story 2.
**Acceptance:** one row per DTO field, recording whether the rule lives in a FluentValidation validator,
a controller check, or a database constraint, with fields carrying no enforced rule flagged rather than
assumed safe. Done for all 7 domains, one evidence file each (see 84.1). Controllers outside the 7
named domains are out of scope for this pass.

84.3 [DONE — 7-domain scope] **Priority: P1 | Depends on: none.** Build an endpoint contract table per
in-scope controller (route, method, request/response DTO with key-field nullability, success/failure
status codes, authorization policy, pagination shape), per spec.md Story 3. **Acceptance:** every
in-scope controller action has a row, and the pagination column states offset or cursor per
`docs/specs/001-platform-baseline/contracts/api-cross-layer.md`. Done for all 7 domains; several
sub-routes are flagged "not read in this pass" where response-body internals or backing service
methods weren't opened (documented per-file, not blanket gaps). Controllers outside the 7 named
domains are out of scope for this pass.

84.4 [TODO] **Priority: P2 | Depends on: 84.3.** Build the Web/Mobile client parity table by
cross-referencing each endpoint from 84.3 against actual client call sites, per spec.md Story 4.
**Acceptance:** every endpoint row states whether Web calls it, whether Mobile calls it, and any
observed difference in how each client reads the response.

84.5 [TODO] **Priority: P3 | Depends on: 84.3.** Build the authorization-policy and problem-details
error-code catalogs from source and existing tests, per spec.md Story 5. **Acceptance:** every
`[Authorize]` policy/role combination in use is listed with its endpoints, and every problem-details
error code actually returned is cataloged.

84.6 [DONE] **Priority: P3 | Depends on: none — product decision, not a documentation task.**
Decision accepted: develop a persisted online election engine scoped by Work Package 37.1, retain static
election documents as supporting governance artifacts, and keep election ballots distinct from
constitution amendment voting. The implementation is tracked under 37.1; this decision item is complete.
Recorded in `docs/specs/002-workflow-contracts-and-validation/tasks.md` T018 and `spec.md`.

84.7 [TODO] **Priority: P3 | Depends on: none — product decision, not a documentation task.** Decide
whether removing the committed-migration-data blocker for white-label second-institution deployment is
in scope, or whether white-label stays a single-institution profile-pack mechanism
(`implementation-inventory.md` lines 135–136). No item in this Work Package should assume an answer
until the project owner decides.

---

# Work Package 85 — NFR catalogue currency for vote integrity and mobile diagnostics

<!-- wbs: component=C17 start=2026-09-21 end=2026-09-21 after=12,37 -->

Raised while grounding a book-content request in the actual codebase. Two features were built and
verified but never carried into the Chapter 3 NFR catalogue: the election engine's atomic vote-cast
update (Work Package 37) and the mobile client's rotating on-device diagnostic log (Work Package 12.3).
The requirements model had drifted behind delivered behaviour.

85.1 [DONE] **Priority: P2 | Depends on: none.** Add NFR-R5 (vote-cast integrity) and NFR-R6 (mobile
diagnostic log capture) to `docs/book/03-requirements.md`'s Reliability subsection, sourced from
`ElectionService.CastVoteAsync`'s single conditional `ExecuteUpdateAsync` and
`LogCaptureService`'s 256 KB oldest-half-drop rotation. NFR-R5's wording states the verified scope
precisely: the atomicity of the conditional update is the actual guarantee; the existing test
(`CastVoteAsync_RejectsReplayAfterTheFirstVote`) exercises sequential resubmission, not two literally
simultaneous requests, and the entry does not claim otherwise. **Acceptance:** both rows added; done.

85.2 [DONE] **Priority: P3 | Depends on: 85.1.** Update Figure 3.11's classification-tree Reliability
range label, add QAS-11 for NFR-R5 to the quality-attribute scenario table, and update §3.13's stated
NFR and QAS totals so the summary counts match the catalogue. **Acceptance:** figure label, QAS-11 row,
and totals updated in the same edit as 85.1; done.

85.3 [DONE] **Priority: P3 | Depends on: 85.1.** Append `"85"` to Work Package C17's tracked area list
in `docs/book/build/wbs.py`. **Acceptance:** done; `python docs/book/build/build.py --pdf --strict` was
run afterward and reported no new drift beyond the pre-existing stale commit/work-package counts in
04-methodology.md, 07-implementation.md and 11-project-management.md, which predate this change.

---

# Work Package 86 — Book sync after the election-navigation, communication-visibility and
organisation-profile commits

<!-- wbs: component=C17 start=2026-09-22 end=2026-09-22 after=85 -->

A routine review of recent implementations against the book turned up the drift 85.3 had flagged and
left for later: 04-methodology.md, 07-implementation.md and 11-project-management.md were still
quoting a 279-commit, 83-work-package snapshot from 16 September, and README.md's Contents table
stopped at Chapter 6 and claimed Chapters 7-13 were unwritten, though all of them existed on disk with
substantial content. Two shipped features also had no requirements-catalogue entry: the
organisation-profile rename (Work Package 62-line) and member-facing communication visibility.

86.1 [DONE] **Priority: P3 | Depends on: 85.** Re-source the repository counts against the tree
(`git rev-list --count HEAD` = 292 commits; `wbs.py --check` = 85 work packages, 901 tracker tasks, as
of 22 September 2026) and correct every quote of the stale 279/83/886 figures in 04-methodology.md,
07-implementation.md and 11-project-management.md, including the reactive/planned percentage
breakdown in 11-project-management.md, rebuilt from `wbs.py`'s actual current category split rather
than rescaled by hand. **Acceptance:** done; no `279`, `83 work packages`, or `eighty-three` instance
remains in the three files.

86.2 [DONE] **Priority: P3 | Depends on: none.** Correct README.md's Contents table and its "Chapters
7-13 not yet written" claim, which had gone stale as soon as those chapters were drafted. Row titles
were read from each chapter's own first heading, not invented, and the status line describes them
honestly as drafted with open placeholders, not as finished or missing. **Acceptance:** done.

86.3 [DONE] **Priority: P3 | Depends on: none.** Add FR-55 (organisation-profile identification,
sourced from `docs/specs/005-academic-organisation-profile-flag/spec.md` and `AcademicRecord.cs`) and
NFR-S9 (communication-history access scoped to the caller's own token, verified against
`MemberCommunicationsController.GetMine` and `CommunicationService.GetLogsForMemberAsync`) to
03-requirements.md, and a matching access-control paragraph to 08-security.md §8.4 naming the same
evidence. Election navigation (commit 975a65a1) was checked and needs no new FR: it only wires
existing pages into the drawer/sidebar, and FR-38/FR-39 already cover the election engine underneath.
**Acceptance:** done; §3.8 and §3.13's Should-count and FR/NFR totals were updated in the same change
so they reconcile against FR-01 to FR-55; `build.py --strict` introduced no new finding from either
file.

86.4 [DONE] **Priority: P4 | Depends on: none.** Confirmed the 5 "captions with no artefact beneath
them" (Table 3.1, 3.2, 3.3, 3.5, 3.6) that `build.py --strict` lists are deliberate index-style
captions in `lint.py`'s `ALLOWED_ORPHANS`, not a defect — the underlying data lives split across
several per-subsystem tables rather than one physical table, so no edit was made. **Acceptance:** done;
recorded here so the next review doesn't re-raise it as a gap.

86.5 [DONE] **Priority: P4 | Depends on: 86.1.** Writing this work package added four tracker tasks of
its own, which moved the live count from the 85/901 figure 86.1 had just sourced to 86 work packages /
905 tasks before this file was even closed. Re-ran `wbs.py --check` after adding 86.1-86.4 and corrected
04-methodology.md (eighty-five to eighty-six, two instances) and 11-project-management.md (85 to 86
work packages, 901 to 905 tasks, and the reactive/planned split recomputed against the new totals: 57 of
86 (66%) reactive, 32 feedback (37%), 16 defect (19%), 9 review-finding (10%); 680 of 905 tasks (75%)
reactive, 299 feedback (33%), 252 review-finding (28%), 129 defect (14%)) to match. Appended `"86"` to
Work Package C17's tracked area list in `docs/book/build/wbs.py`, matching 85.3's precedent. **Acceptance:**
done; `python docs/book/build/build.py --strict` reports `status: clean, ready to deliver` with no
repository-count finding.

**Not carried into this work package:** the 96 open `*[` placeholders across 07-implementation.md,
09-verification.md, 10-deployment.md, 11-project-management.md, 12-results.md and 13-conclusion.md.
Closing those needs real dissertation evidence (verification runs, deployment records, results data)
that cannot be produced by a documentation-sync pass, and is a separate, larger piece of work.

---

# Work Package 87 — Login authentication progress status

<!-- wbs: component=C13 start=2026-09-22 end=2026-09-22 after=86 -->

Two commits (894567f3, 6eec298a) shipped a new public login flow — `docs/specs/007-login-authentication-status/spec.md`
and `plan.md`, plus `login.html`/`login.scss`/`login.spec.ts`/`login.ts` — with no matching tracker
entry, found during a routine review of recent implementations against `docs/TODO.md`.

87.1 [DONE] **Priority: P3 | Depends on: none.** Replace the static `Authenticating...` label with a
timed sequence of technical-sounding status verbs (`Connecting`, `Validating`, `Hashing`, etc., a
6-to-7-item subset chosen per attempt) shown in strict forward order while the request is pending, each
transition a 1.2s crossfade using the login page's existing theme tokens and button styling. Submit and
social-login buttons stay disabled for the duration to block duplicate requests.
**Acceptance:** done; `docs/specs/007-login-authentication-status/spec.md` FR-1/FR-2/FR-3.

87.2 [DONE] **Priority: P3 | Depends on: 87.1.** Lifecycle safety: an 8-second timeout stops the
sequence, restores the idle form, and shows `Login timed out. Please try again.` if the request hangs;
`ngOnDestroy` clears the interval and timeout timers so no callback fires after the component is gone;
the progress indicator and label both return to their idle state immediately on success or failure.
**Acceptance:** done; `npx vitest run src/app/public/login/login.spec.ts` (6 passed, run 2026-09-22).

# Work Package 88 — Idempotent delivery path for the May 2026 alumni batch

Raised by the user 2026-09-22, who suspected Work Package 46.1 "reverted while migration re-factored."
Confirmed: 46.1-46.5 stay correctly `[DONE]` in `docs/TODO_ARCHIVE.md` for the work they describe, but
their delivery mechanism no longer exists in the tree. The dedicated EF migration
(`AddMay2026AlumniRegistrationBatch`) was never committed to tracked history, and the WP62 white-label
work replaced the migration-based seed with `InstitutionDataSeeder`, which seeds a table only when it
is completely empty — so on any real deployment, where `Members` already has rows, the 47-member batch
never reaches the live database even though `profiles/ghc/demo-data/members.json` still holds all 631
rows. Scope and design in `docs/specs/008-idempotent-member-batch-import/`.

The user separately flagged that 88.4's real-DB verification happens only after a commit — an admin
should be able to catch and fix a bad row before anything is written. 88.5 adds that preview step.

88.1 [TODO] **Priority: P1 | Depends on: none.** Extend the existing admin bulk-import feature
(`MemberImportController` / `MemberImportService`) — which already upserts Member rows by NID/email —
to accept `.csv` directly, enforce a unique `MembershipNumber` index, and cascade its idempotency check
to AcademicRecord, ProfessionalRecord, PaymentHistory (not currently touched by the importer), User,
and UserRoles: each related row inserted only when a matching one is not already present for that
member. Per `docs/specs/008-idempotent-member-batch-import/spec.md` FR-1 through FR-3.
**Acceptance:** not started.

88.2 [TODO] **Priority: P1 | Depends on: 88.1.** Wrap the whole import run in a single database
transaction so a write failure partway through rolls back every change from that run, replacing the
current two-phase save that can leave a Member row committed with its related rows missing. Extend the
import response with per-table insert/update/skip counts so a real run is its own DB-state
verification. Per spec FR-4/FR-5.
**Acceptance:** not started.

88.3 [TODO] **Priority: P1 | Depends on: 88.1, 88.2.** Add an integration test (`GHCAA.Tests/Services/
MemberImportServiceTests.cs`, existing `TestBase` SQLite fixture) that seeds a member with a full set
of related rows, runs the import twice, and asserts the second run reports zero inserts everywhere with
identical row counts before and after; add a companion test proving a forced mid-batch write failure
leaves nothing committed.
**Acceptance:** not started.

88.4 [TODO] **Priority: P1 | Depends on: 88.1, 88.2, 88.3.** Run the extended import against a real
database (not the seed JSON) seeded from `profiles/ghc/demo-data/`, using the row counts from the
database itself, before and after two consecutive runs, to confirm no duplication. This step needs a
reachable dev or preprod database and cannot be completed from a sandboxed session — record the actual
counts here once run.
**Acceptance:** not started.

88.5 [TODO] **Priority: P1 | Depends on: 88.1.** Add a preview (dry-run) endpoint and an editable
preview grid to the import flow, so an admin sees and fixes bad cells before anything is written
instead of only after a failed commit. The endpoint runs the same mapping/validation/idempotency
checks as a real import but writes nothing, returning per-row, per-cell errors (row index + column
key, not a flat message list). The grid marks each invalid cell at its exact location, lets the admin
edit it in place, and blocks commit while any row is still invalid. No reusable grid/table component
exists in `GHCAA.Web` today, so this also adds one as a generic component (rows, columns, per-cell
errors in; cell-edit and commit events out) for other bulk-data features to reuse later, rather than a
one-off built only for member import. Per spec FR-6.
**Acceptance:** not started.

---

# Work Package 89 — Mentorship web UI (raised by user 2026-09-23: "campaigns, ad-hoc reporting, and mentoring are the three genuine feature gaps")

Raised alongside Work Package 90 while checking whether campaigns, ad-hoc reporting and mentoring
were genuinely missing. Mentoring is not: `MentorshipRequest`, `IMentorshipService`, a 6-route
`MentorshipController` (send/sent/received/respond/complete/admin-all, class-level `[Authorize]`)
and a full Flutter screen (`lib/screens/member/mentorship_hub_screen.dart`) are already committed
and the tables are in `InitialBaseline` (properly migrated). Only Angular web has no consuming page —
`core/services/mentorship.service.ts` exists and is unused.

89.1 [DONE] **Priority: P2 | Depends on: none.** Admin oversight page at `admin/mentorship` against
the existing `GET api/mentorship/admin/all` route — list all requests with status, requester, mentor.
No new backend.
**Acceptance:** `admin-mentorship.ts`/`.html`/`.scss` built and routed at `/admin/mentorship`, nav
entry added under Content section, vitest spec covers load + search filter.

89.2 [DONE] **Priority: P2 | Depends on: none.** Member mentorship UI: the send-request form and
sent/received lists already existed on the combined `member/requests` page (built for family links,
tabbed to include mentorship) — the only missing action was mark-complete, added as `completeMentorship()`
wired to the existing `mentorship.service.ts` and the existing `MentorshipController` routes. No
separate `portal/mentorship` page was needed; extending the existing combined page matches how family
links and mentorship were already sharing it.
**Acceptance:** "Mark Complete" button added to both received and sent mentorship rows when
`status === 'Accepted'`; `completeMentorship()` calls `MentorshipService.markComplete` and reloads.

89.3 [DONE] **Priority: P2 | Depends on: 89.1, 89.2.** `MENTORSHIP` already existed in
`API_ENDPOINTS`; no separate `ADMIN_MENTORSHIP` block was needed since the admin list reuses the same
base path (`/admin/all` suffix). `mentorship.service.ts` was typed properly (`MentorshipRequestDto`,
`MentorshipAdminRow`) and gained `getAllForAdmin()`/`markComplete()`; `MENTORSHIP_STATUS_MAP` +
`getMentorshipStatusLabel`/`getMentorshipStatusClass` added to `app.constants.ts`, mirroring the
`PLEDGE_STATUS_MAP` convention.
**Acceptance:** `npm run type-check` passes clean.

89.4 [DONE] **Priority: P3 | Depends on: 89.1, 89.2.** Correct `docs/FEATURES.md` (~line 167), which
currently describes Mentorship only as a `JobCategory` enum value — add a section for the real
request/respond/complete workflow that ships in this item.
**Acceptance:** see below, same change.

89.5 [DONE] **Priority: P2 | Depends on: 89.1, 89.2.** Vitest specs for both new pages (send/respond/
complete flow, admin list rendering). Backend and mobile tests already exist and are not duplicated.
**Acceptance:** `admin-mentorship.spec.ts` (3 tests: create, load, filter) and 2 new tests added to
`requests.spec.ts` (`completeMentorship` success/error) — 14/14 passing, `npm run test:unit`.

---

# Work Package 90 — Ad-hoc reporting for admins (raised by user 2026-09-23: "campaigns, ad-hoc reporting, and mentoring are the three genuine feature gaps")

The one of the three that is a genuine gap: no `IReportService`, no `ReportsController`, no
report-builder entity anywhere in the codebase. Admin-only internal tool — no public, member or
mobile surface, matching every other admin-only backend tool in this repo. Spec:
`docs/specs/010-ad-hoc-reporting/spec.md`.

90.1 [TODO] **Priority: P3 | Depends on: none.** `SavedReport` model
(`Id`, `Name`, `EntityType`, `FiltersJson`, `ColumnsJson`, `CreatedBy`, `CreatedAt`) in
`GHCAA.Domain/Models/`, `SavedReportConfiguration`, migration `AddSavedReports`.
**Acceptance:** not started.

90.2 [TODO] **Priority: P3 | Depends on: 90.1.** `IReportService` (`GetEntityFields`, `RunReport`,
`SaveReport`, `GetSavedReports`, `ExportToExcel`) + `ReportService`, composing EF `IQueryable` per
entity — no raw SQL — with a validator that rejects any filter field not on that entity's whitelist.
Export reuses whatever Excel package `MemberImportService.cs`/`FileValidationService.cs` already
reference; no new Excel dependency.
**Acceptance:** not started.

90.3 [TODO] **Priority: P3 | Depends on: 90.2.** `AdminReportsController` at `api/admin/reports`,
`[Authorize(Roles = "Admin,SuperAdmin")]` matching the `AdminOnly` pattern in
`CampaignsController`/`MentorshipController`.
**Acceptance:** not started.

90.4 [TODO] **Priority: P3 | Depends on: 90.3.** Admin-only Angular page `admin/reports` — entity
picker, filter form, `.data-table` result grid (reuse the Directory table/card pattern), export
button — plus `core/services/report.service.ts` and an `ADMIN_REPORTS` block in `API_ENDPOINTS`.
**Acceptance:** not started.

90.5 [TODO] **Priority: P3 | Depends on: 90.2, 90.3.** NUnit tests (`GHCAA.Tests/Services/
ReportServiceTests.cs`): filter whitelist rejection, saved-report round trip, export row count. Vitest
spec for the report builder component. FR-tag both per `feedback_fr_nfr_tag_new_tests`.
**Acceptance:** not started.
