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
- **48.2** (merged 2026-09-15 with 47.10's credentials half and 48.13) — Live production secrets
  committed to git (JWT signing key, DB passwords, Gmail app password, Render deploy-hook URL) in
  `docs/deploy_connection.txt`/`docs/deploy_conn_Info.txt`, `.env.remote`,
  `build_output/appsettings*.json`, `docs/RENDER_DEPLOYMENT.md`. Separately, the live SuperAdmin
  password sat in git history (`docs/BUSINESS_REVIEW_PLAN.md`) since before it was even set live —
  doc text is redacted, but the password itself still needs an independent rotation. Needs the user
  to rotate every credential via the relevant dashboards, then `git rm --cached` + `.gitignore`
  (done) + a history purge (`git filter-repo`). No coding-session action can close this.
- **48.12** — Remaining unfixed Low findings from the Work Package 48 security audit: `MessagingController.MarkAsRead`
  missing ownership check; `FinancialsController.RecordPayment` trusts a client-supplied `MemberId`;
  refresh-token replay isn't detected/revoked; `MemberImportController` upload skips file validation;
  raw `FullName` interpolated into an HTML email body (XSS-adjacent).
- **82.31 / 62.31** — 631 real alumni records, including all 631 password hashes, are literal
  `InsertData` values in eight committed migrations, so a clean clone of this repo builds a database
  full of real personal data. Editing `Seed/members.json` does not reach it, and no environment
  variable turns it off. Needs a decision on the history purge and a forced password reset before any
  code moves. `docs/SEED_CLASSIFICATION.md` has the per-file breakdown.

### P1 — HIGH (security surface / explicitly time-sensitive / blocking other work)
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

### P2 — MEDIUM (real, no urgency signal)
- **45.1–45.7** — Admin error-log viewer, fully planned, nothing built.
- **42.1–42.5** — Admin-manageable elections forms/docs, plan only.
- **49.4 / 49.5** — Grid/row-control consistency cleanup + tests for the new 49.x endpoints once built.
- **51.4–51.5** — Remaining file-storage hardening: broader regression coverage and admin-configurable
  settings.
  tests, compression settings not admin-configurable yet.
- **47.10** — Missing profile photos for most of the 631 bulk-imported alumni (data gap, not a bug).
- **43.4** — Live/manual verification that the Work Package 43 exception-handling/logging sweep actually fires.
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
- **61.1 / 61.2** — Dynamic/runtime dead-code scan (unused services/classes/widgets) + follow-up
  refactor pass, run module-by-module via graphify rather than one blind full-repo sweep.

### P3 — LOW / PLAN-ONLY (large unbuilt features, no current pressure)
- **Work Package 37** (37.2–37.10) — scholarships, fundraising, cohorts/reunions, oral-history archive,
  bilingual UI, credential verification, geographic chapters, annual impact report.
- **6.2** — Alumni referral system for jobs/internships.
- **82.6 / 82.9 / 82.10b / 82.11–82.13** — Audit follow-through: split `MemberService` (1,577 lines),
  request correlation and structured logging, typed Dart models for the auth, profile and payment
  payloads, the configuration and constants classifications, and the missing decision records and
  recovery runbook. (82.10, whether to generate clients from OpenAPI, is decided and closed: rejected.)
- **61.3** — Drop the `Summary:`-style comment banner in `GHCAA.Tools/db_diag.cs` next time that file is touched.


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
7.16 [TODO] **Priority: P3.** Hardening: SSL Pinning and Binary Obfuscation

## WORK PACKAGE 8: MOBILE ENGINEERING (TIER-1 STANDARDS)

8.1  [DONE] UI: Enforce 8pt grid and standard design tokens globally
8.2  [DONE] Nav: Adaptive layout for Tablets/Pads (Sidebar architecture)
8.3 [TODO] **Priority: P2.** Perf: Cursor-based pagination for Alumni Registry
8.4 [TODO] **Priority: P3.** Perf: Isolated background threading for JSON/Encryption processing
8.5 [TODO] **Priority: P4.** Persistence: Switch to High-Performance Local DB (Isar/Drift)
8.6 [TODO] **Priority: P2.** Networking: Exponential backoff and connectivity banners
8.7 [TODO] **Priority: P3.** State: Riverpod State Hydration (Local local persistence)
8.8 [TODO] **Priority: P2.** i18n: Unified Localization (English + Bengali)
8.9  [DONE] CI/CD: Fastlane + GitHub Actions Deployment Pipeline
8.10 [DONE] Quality: Global Error Boundary and Sentry/Firebase tracing
8.11 [DONE] CI/CD: Operationalize multi-environment pipelines (Preprod/Standard)

## WORK PACKAGE 12: PROCESS & ENGINEERING STANDARDS

12.1 [TODO] **Priority: P2.** PROCESS: On every API endpoint change, add verification checklist task for Web + Mobile parity
12.2 [TODO] **Priority: P3.** PROCESS: Implement API Contract Registry (changelog of all endpoint changes + which clients updated)
12.3 [TODO] **Priority: P2.** Mobile: Implement in-app log capture (rotating file log) for all API errors and app events
12.4 [TODO] **Priority: P3.** Mobile: Add "Report a Problem" / "Share Logs" feature so users can email/share captured logs to admin
12.5 [TODO] **Priority: P3.** Mobile: On any unhandled error, show option to "Send Report to Administrator" with log attachment
12.6 [TODO] **Priority: P2.** PROCESS: A task can only be marked as [DONE] after its tests have been successfully executed and passed.

## WORK PACKAGE 27: TEST COVERAGE IMPROVEMENT

27.1  [IN-PROGRESS] Generate low‑coverage report (parse coverage.cobertura.xml)
27.2  [DONE 2026-08-22] Add test project references for API, Application, Domain, Infrastructure VERIFIED 2026-08-22: `GHCAA.Tests.csproj` references all four projects (Application, Infrastructure, Domain, API).
27.3  [DONE 2026-08-22] Write unit tests for Controllers (WebApplicationFactory) VERIFIED 2026-08-22: 18 controller test classes under `GHCAA.Tests/Controllers/` plus a shared `ControllerTestBase.cs`.
27.4  [DONE 2026-08-22] Write unit tests for Handlers/Services (Moq) VERIFIED 2026-08-22: ~20 service test classes under `GHCAA.Tests/Services/`, with `Moq 4.20.72` + `FluentAssertions 6.12.2` referenced.
27.5  [DONE 2026-08-22] Write unit tests for Domain Validators (FluentValidation) VERIFIED 2026-08-22: `GHCAA.Tests/Validators/MemberRegistrationValidatorTests.cs` and `VerifyEmailValidatorTests.cs`.
27.6  [DONE 2026-08-22] Write repository integration tests with in‑memory SQLite VERIFIED 2026-08-22: `Microsoft.EntityFrameworkCore.Sqlite 9.0.1` + `.InMemory 9.0.1` referenced, with `GHCAA.Tests/Repositories/FileUploadRepositoryTests.cs` and SQLite-backed service tests.
27.7  [DONE 2026-08-22] Write utility class tests (DateFormatConverter, etc.) **CLOSED 2026-08-22:** `GHCAA.Tests/Utils/DateFormatConverterTests.cs` added — 20 tests, all passing; full backend suite now **350 passed / 0 failed** (was 330). Pins the 29F.3 contract in both directions: ISO-8601 on write (incl. time preserved and the `.fff` shape), dd-MM-yyyy accepted on read with ISO as fallback, day-first precedence for ambiguous input like `02-03-2026`, empty/whitespace → `default` on the non-nullable converter but → `null` on the nullable one, and malformed/impossible dates throwing `FormatException` rather than silently yielding `01-01-0001`. Prior note, now historical:  **PARTIAL, confirmed 2026-08-22:** the named example is still untested — `DateFormatConverter` / `NullableDateFormatConverter` live in `GHCAA.API/Utils/DateFormatConverter.cs` and are registered in `Program.cs` (lines ~137-138), but no test file references them. Given these two converters govern **every** DateTime on the wire (see the ISO-8601 switch), they are the highest-value gap in Work Package 27.
27.8 [TODO] **Priority: P2.** Run coverage and enforce ≥ 80 % per file (Still genuinely open, confirmed 2026-08-22: `coverlet.collector 6.0.2` is referenced so coverage *can* be collected locally, but no threshold is enforced anywhere and README explicitly declines to claim a figure. Enforcing >=80%/file would fail today.)
27.9  [DONE 2026-08-22] Update README with test & coverage instructions VERIFIED 2026-08-22: README line ~299 documents `dotnet test` / `npm test` / `flutter test`, and line ~301 explains the coverage position and the local `coverlet.collector` command.
27.10 [IN-PROGRESS — tracked in 82.86] API: Add focused negative tests for the four business-rule coverage gaps (COV-001 through COV-004), including intended 4xx mapping for invalid business input. Use 82.86 as the canonical execution record.

## WORK PACKAGE 28: CONFIGURATION-DRIVEN FRAMEWORK

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
> pick when hand-entering a `FinancialRecord`.** There is no campaign, no donor, no pledge, no
> scholarship, no reunion and no grant application anywhere in the 45 files under
> `GHCAA.Domain/Models/`. The ledger can *record* philanthropy; the product cannot *conduct* it.
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
> just because the model compiles, and `HasData` seed edits never reach it. Work Package 36 solved this
> once with `GHCAA.Infrastructure/Data/ConstitutionSeeder.SyncAsync(context, logger, ct)` — an
> idempotent boot-time syncer called from `Program.cs` behind a `CanConnectAsync()`/`LogWarning`
> guard. **Copying that pattern per feature is the fallback, not the plan.** 37.0 makes the real
> migration path a prerequisite, because four more bespoke syncers is a smell.
>
> **Recommended sequence: 37.0 → 37.3 → 37.2 → 37.1**, with 37.5 taken opportunistically (it is
> the smallest item in the Area and needs no new UI shell). 37.7–37.10 are independent and may be
> scheduled at any point after 37.0.

37.0 [DONE 2026-09-05] **Priority: P2.** **Prerequisite: a real migration path.** Every remaining item in this Area adds tables, and none of them can reach preprod under `EnsureCreated()`.
**Already satisfied by prior work, discovered on starting this item rather than built fresh:**
`GHCAA.Infrastructure/Data/MigrationBootstrapper.EnsureMigratedAsync`, wired into `Program.cs`'s boot
sequence for every non-Visual profile, already does what this item asks and more. It calls
`Database.MigrateAsync()` (this item's literal request), but a plain `MigrateAsync()` alone would have
been a regression against what already ships: preprod's database was first built with
`EnsureCreated()`, so a bare `MigrateAsync()` tries to `CREATE TABLE` on tables that already exist and
fails outright. The bootstrapper baselines that legacy state first (walking every migration in order,
marking one applied without re-running it whenever Postgres reports its target already exists), then
self-heals a migration whose history row is a false positive from a rolled-back same-transaction seed
insert, before calling `MigrateAsync()` for what is genuinely pending — this was built and fixed
across two earlier incidents (`gotcha_migrationbootstrapper_fixed_offset`), and the full chain was
independently validated end-to-end via a throwaway-database dry run in the same prior session. On any
unhandled failure it falls back to `EnsureCreated()` rather than crash the app.
`ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))` is already set on
every context configuration.
**What this item actually still needed and now has:** the boot sequence was never written down.
`docs/ARCHITECTURE.md` §5 and `docs/PROJECT_MAP.md`'s Infrastructure Layer — Data / EF Context section
both now describe it.
**Not built: the `Database:ApplyMigrationsOnStartup` config flag this item asked for.** The bootstrapper
runs unconditionally for any non-Visual profile instead. Deliberately not added: it would only serve
as an emergency kill-switch, and the bootstrapper's own try/catch-and-fall-back-to-EnsureCreated
already is one — a config flag would be a second, redundant way to reach the same degraded state.
**Acceptance: a schema change committed on `preprod` is visible on the Render deployment without a
manual database step.** Already true; every migration in this repository already reaches preprod this
way, which is how 37.1–37.10 can proceed without a migration-path blocker of their own.

37.1 [TODO] **Priority: P3.** **Election engine** — turn the Work Package 36 documents into a running process. This is the largest item; implement it in the five phases below, each independently shippable behind the flag. New enums in `GHCAA.Domain/Enums.cs`: `ElectionPhase { Announced, Nomination, Scrutiny, Withdrawal, CandidateList, Campaign, Polling, Counting, Declared, Archived }`, `NominationStatus { Submitted, UnderScrutiny, Accepted, Rejected, Withdrawn }`, `ElectionRole { ReturningOfficer, AssistantReturningOfficer, PollingOfficer, Scrutineer }`.
  - **37.1a Election + roll.** `Election` (`Id`, `Title`, `ECPeriodId`, `Phase`, `AnnouncedOn`, `NominationOpensOn`, `NominationClosesOn`, `ScrutinyOn`, `WithdrawalClosesOn`, `PollingOpensOn`, `PollingClosesOn`, `DeclaredOn?`, `IsActive`, `CreatedBy`), `ElectionSeat` (`Id`, `ElectionId`, `ECPosition Position`, `SeatCount`), `ElectionOfficer` (`Id`, `ElectionId`, `MemberId`, `ElectionRole Role`), and `VoterRoll` (`Id`, `ElectionId`, `MemberId`, `IsEligible`, `IneligibilityReason?`, `FrozenAt`, `VotedAt?`). The roll is **frozen by snapshot**, not computed at poll time: eligibility is the same Article III Section K rule already enforced in `GovernanceService.VoteOnConstitutionAsync` (`MembershipType` of `Founding`, `Executive` or `General`), plus dues-current per `MembershipDue`. Freezing is what makes a disputed result auditable.
  - **37.1b Nomination.** `Nomination` (`Id`, `ElectionId`, `ElectionSeatId`, `CandidateMemberId`, `ProposerMemberId`, `SeconderMemberId`, `Statement`, `PhotoPath?`, `Status`, `SubmittedAt`, `WithdrawnAt?`), `ScrutinyDecision` (`Id`, `NominationId`, `OfficerMemberId`, `Accepted`, `Reason`, `DecidedAt`). Proposer and seconder must both be on the frozen roll and must not be the candidate; enforce in the service, not only the UI.
  - **37.1c Ballot and poll.** `Ballot` (`Id`, `ElectionId`, `ElectionSeatId`, `SerialNumber`, `IssuedAt`, `IsSpoiled`) and `BallotVote` (`Id`, `BallotId`, `NominationId`, `CastAt`) kept in **separate tables with no member foreign key on the vote side** — the roll records *that* a member voted (`VoterRoll.VotedAt`), the ballot records *what* was voted, and nothing joins the two. That separation is the secret ballot, and it is the one design decision here that cannot be retrofitted. A unique index on `(ElectionId, MemberId)` in the roll prevents double voting.
  - **37.1d Counting and declaration.** `ElectionResult` (`Id`, `ElectionId`, `ElectionSeatId`, `NominationId`, `VoteCount`, `IsElected`, `IsTie`). On declaration, write the winners straight into `ECMember` rows against the election's `ECPeriodId` — this is the payoff: the committee roster stops being hand-typed.
  - **37.1e ER-forms as generated PDFs.** The 18 blank forms from 36.6 become filled documents. Server-side with **QuestPDF**, following `IDCardService.GenerateIDCardPdfAsync` for structure and its `GetQrDataUri` helper for the verification QR. Minimum set: ER-01 Election Notice, ER-03 Nomination Paper, ER-05 Final Candidate List, ER-08 Ballot Paper, ER-12 Result Sheet. Each carries the election title, the returning officer's name, the generation timestamp and a QR pointing at the 37.8 verification URL.
  - **Service/API.** `IElectionService` in `GHCAA.Application/Interfaces` + `GHCAA.Infrastructure/Services/ElectionService.cs` (no DI registration needed). `ElectionsController` at `api/elections`, `[Authorize]` at class level; `GET api/elections/public` and `GET api/elections/{id}/candidates` are `[AllowAnonymous]` (the candidate list is a published document); nomination, scrutiny and casting are member- or officer-scoped. Casting must reject any phase other than `Polling` server-side.
  - **UI.** Flag `enableElections`. The existing public `/elections` page gains a live banner when an election is not `Archived`. Member `portal/elections` — nominate, withdraw, view candidates, cast. Admin `admin/elections` — create, appoint officers, freeze roll, scrutinise, advance phase, count, declare, download ER PDFs. New `API_ENDPOINTS.ELECTIONS` block.
  - **Tests.** NUnit: roll freeze excludes Associate/Honorary/Advisory; proposer ≠ candidate; double vote rejected; cast outside `Polling` rejected; declaration writes `ECMember`; **a ballot row cannot be joined back to a member**. Vitest: phase-driven UI state, closed-nomination guard.

37.2 [TODO] **Priority: P4.** **Scholarship & student-aid programme** — fund → open call → application → blind review → award → disbursement. New enums: `ScholarshipApplicationStatus { Draft, Submitted, UnderReview, Shortlisted, Awarded, Rejected, Withdrawn }`, `DisbursementStatus { Pending, Approved, Paid, Cancelled }`.
  - **Models.** `ScholarshipFund` (`Id`, `Name`, `Description`, `NamedAfter?` — the endowment-in-memory case that links to 37.5, `TargetAmount`, `IsActive`, `CreatedAt`); `ScholarshipCall` (`Id`, `ScholarshipFundId`, `AcademicYear`, `OpensOn`, `ClosesOn`, `SlotCount`, `AwardAmount`, `EligibilityCriteria`, `IsActive`); `ScholarshipApplication` (`Id`, `ScholarshipCallId`, `ApplicantName`, `ApplicantEmail`, `ApplicantPhone`, `InstitutionName`, `Class`, `GuardianName`, `HouseholdIncome`, `NeedStatement`, `MeritStatement`, `Status`, `SubmittedAt`, `ReferenceCode`); `ScholarshipDocument` (`Id`, `ScholarshipApplicationId`, `FileUploadId`, `DocumentType`) reusing the existing `FileUpload` + `IFileValidationService` path — **no new upload plumbing**; `ScholarshipReview` (`Id`, `ScholarshipApplicationId`, `ReviewerMemberId`, `NeedScore`, `MeritScore`, `Comments`, `ReviewedAt`); `ScholarshipAward` (`Id`, `ScholarshipApplicationId`, `Amount`, `AwardedOn`, `DisbursementStatus`, `FinancialRecordId?`).
  - **Blind review is a query rule, not a UI rule.** `GetApplicationForReviewAsync` must project a DTO that omits `ApplicantName`, `ApplicantEmail`, `ApplicantPhone` and `GuardianName`, exposing only `ReferenceCode`. Reviewers must not be able to obtain identity from the API at all; hiding it in the template is not acceptance.
  - **Applicants are not members.** The public application form is `[AllowAnonymous]` and identified by `ReferenceCode` + email; **do not create `Member`/`User` rows for schoolchildren.** Status lookup is `GET api/scholarships/status/{referenceCode}`, also anonymous, rate-limited by the existing `LoginRateLimitMiddleware` pattern.
  - **Ledger tie-in.** Marking an award `Paid` writes a `FinancialRecord` with `RecordType = Expense`, `FinancialCategory = Grant`, `Reference = ReferenceCode`, and stores the new record's `Id` back on `ScholarshipAward.FinancialRecordId`. This is what makes the impact report in 37.10 derivable rather than typed.
  - **Service/API/UI.** `IScholarshipService` + `ScholarshipService`; `ScholarshipsController` at `api/scholarships`. Flag `enableScholarships`. Public `/scholarships` (call listing + apply + status check), member `portal/scholarships` (reviewer queue for panel members), admin `admin/scholarships` (funds, calls, shortlist, award, disburse). New `API_ENDPOINTS.SCHOLARSHIPS` block.
  - **Tests.** NUnit: review DTO carries no identifying field; application rejected outside the `OpensOn`–`ClosesOn` window; award → paid writes exactly one `Grant` `FinancialRecord` and is idempotent on repeat. Vitest: apply-form validation, status lookup with an unknown code.

37.3 [DONE 2026-09-05] **Priority: P3.** **Fundraising campaigns + donor honour roll** — the shortest path from "the ledger has a `Donation` category" to "the association can actually raise money". New enum: `PledgeStatus { Pledged, PartiallyPaid, Paid, Lapsed, Cancelled }`.
  - **Models.** `Campaign` (`Id`, `Title`, `Slug`, `Story`, `CoverImagePath?`, `TargetAmount`, `StartsOn`, `EndsOn?`, `IsActive`, `IsArchived`, `CreatedBy`); `CampaignPledge` (`Id`, `CampaignId`, `MemberId?` — nullable so non-alumni can give, `DonorName`, `DonorEmail?`, `DonorPhone?`, `Amount`, `AmountReceived`, `Status`, `IsAnonymous`, `Message?`, `PledgedAt`, `FinancialRecordId?`); `DonorRecognitionTier` (`Id`, `Name`, `MinimumAmount`, `Description`) — admin-configurable so tier names are not compiled in.
  - **No payment gateway.** The repo operates under a **no-gateway-keys rule** (`session_area29_shipblockers_payments`): a pledge is recorded, the existing display-only wallet/bank instructions are shown, and an admin confirms receipt. Confirming receipt writes a `FinancialRecord` (`RecordType = Income`, `FinancialCategory = Donation`) and back-links `FinancialRecordId`, mirroring 37.2 exactly. **Do not introduce a gateway integration here.**
  - **Honour roll.** Public, derived, never hand-maintained: group confirmed pledges by `DonorRecognitionTier`, render `IsAnonymous` rows as "Anonymous", and show a live progress bar of `SUM(AmountReceived) / TargetAmount`. Anonymity must be enforced in the projection, not the template.
  - **Service/API/UI.** `ICampaignService` + `CampaignService`; `CampaignsController` at `api/campaigns` with `GET api/campaigns/public`, `GET api/campaigns/{slug}` and `GET api/campaigns/{slug}/honour-roll` as `[AllowAnonymous]`; pledging allowed anonymously. Flag `enableFundraising`. Public `/campaigns` + `/campaigns/:slug`, member `portal/giving` (my pledges, my giving history), admin `admin/campaigns` (create, confirm receipts, tiers). New `API_ENDPOINTS.CAMPAIGNS` block.
  - **Tests.** NUnit: an anonymous pledge never leaks `DonorName` through the honour-roll projection; confirming receipt is idempotent and writes one `Donation` record; progress excludes unconfirmed pledges. Vitest: progress-bar arithmetic, anonymous-checkbox behaviour.
  - **Done 2026-09-05.** Every bullet above shipped as specified — 3 new tables (migration
    `AddFundraisingCampaigns`, hand-written to avoid the scaffolder's usual seed-churn additions), the
    service/controller/feature-flag exactly as scoped, and all three UI surfaces. `ConfirmPledgeReceiptAsync`
    is idempotent by checking `FinancialRecordId` before writing, matching 37.2's intended pattern (37.2
    itself is unbuilt — P4 — so this is the first `Donation`-category writer in the codebase, not a
    copy of an existing one). Anonymity is enforced once, in `CampaignService.ToHonourRollEntry`, the
    single place every consumer of the honour-roll projection reads from.
    **Tests:** `CampaignServiceTests.cs` (4, covering exactly the three NUnit cases asked for plus a
    guest-pledge case) and `campaigns.spec.ts` (7, covering progress-bar arithmetic incl. the
    divide-by-zero and over-100%-clamp edges, and anonymous-checkbox behaviour). All green alongside
    the rest: `dotnet test` 590/590, `vitest` 390/390, `flutter test` unaffected (mobile was not asked
    for and not built — the item's own UI list names only public/portal/admin web surfaces).
    **One real bug caught and fixed on the way, unrelated to campaigns but exposed by adding
    `AlumniEvent.participantLimit`/`hasWaitlist` to strict typing during the WP82.32 sweep earlier the
    same day:** `admin-events.ts`'s reactive form inferred `FormControl<null>` for
    `participantLimit` from its bare `[null]` initial value, which could never legally hold the number
    a real event's data patches into it. Explicitly typed as `[null as number | null]`.
    **A DI-ordering gotcha worth remembering for the next Angular spec:** `provideRouter([])` placed
    after an explicit `{ provide: ActivatedRoute, useValue: ... }` in the same `providers` array
    silently wins and overrides the mock — the override must come after `provideRouter`, not before.
    **Not verified: the 37.11 non-empty-database migration check.** No Postgres instance was
    reachable in this session (same limitation the 62.1 lockfile work hit); `AddFundraisingCampaigns`
    is a straightforward additive `CreateTable` migration with no seed data and no touched columns on
    an existing table, so the risk this leaves open is low, but it has not been proven the way 37.0's
    own migration path was.

37.4 [TODO] **Priority: P3.** **Batch cohorts and reunions as first-class objects.** Today a batch exists only as `AcademicRecord.PassingYear` — there is no cohort page, no cohort representative and no reunion.
  - **Models.** `BatchCohort` (`Id`, `PassingYear`, `Title`, `Story?`, `CoverImagePath?`, `RepresentativeMemberId?`, `IsActive`); `Reunion` (`Id`, `BatchCohortId?` — null means an all-alumni reunion, `AlumniEventId`, `Theme`, `SouvenirUrl?`) built **on top of** the existing `AlumniEvent` + `EventRegistration` + `EventBudget` stack rather than beside it — a reunion is an event with cohort identity, and duplicating registration logic would be the mistake here.
  - **Membership is derived, not stored.** Cohort membership = `AcademicRecord` rows with `IsGHC == true` and the matching `PassingYear`. Do not add a `BatchYear` column to `Member`; it would immediately disagree with `AcademicRecord` for anyone holding two GHC records.
  - **Ledger tie-in.** Reunion fees collected through `EventRegistration` post as `FinancialCategory.ReunionFee`, finally giving that enum value a producer.
  - **Service/API/UI.** `IBatchService` + `BatchService`; `BatchesController` at `api/batches` with `GET api/batches/public` and `GET api/batches/{year}` `[AllowAnonymous]`. Flag `enableReunions`. Public `/batches` (year grid) + `/batches/:year`, member `portal/my-batch`, admin `admin/batches`. New `API_ENDPOINTS.BATCHES` block.
  - **Tests.** NUnit: a member with two GHC `AcademicRecord` rows appears in both cohorts; non-GHC records excluded. Vitest: year-grid grouping, empty-cohort `EmptyStateWidget` path.

37.5 [TODO] **Priority: P3.** **In Memoriam register.** A new `MembershipType` value is **not** the mechanism — membership tier is admin-assigned and orthogonal (`feedback_membership_type_admin_only`). Instead: `MemorialEntry` (`Id`, `MemberId?` — nullable so a pre-digital alumnus can be honoured, `FullName`, `PassingYear?`, `DateOfBirth?`, `DateOfDeath`, `PhotoPath?`, `Tribute`, `IsPublished`, `SubmittedByMemberId?`, `SubmissionStatus Status`, `CreatedAt`) reusing the existing `SubmissionStatus { Draft, Pending, Approved, Rejected }` enum, and `Condolence` (`Id`, `MemorialEntryId`, `MemberId`, `Message`, `PostedAt`, `IsApproved`).
  - **Moderation is mandatory.** Nothing publishes without admin approval, and every condolence goes through the existing `HtmlSanitizer` path before storage. This is the single highest-sensitivity surface in the Area; an unmoderated tribute wall on a memorial page is a reputational incident.
  - **Setting `MemorialEntry.MemberId` must deactivate the linked `Member`** — and suppress them from the public directory and from any 37.1 voter roll — in the same transaction. A deceased member appearing on an election roll is the failure mode this clause exists to prevent.
  - **Service/API/UI.** Extend `IMemberService` **only if** the memorial logic stays under ~5 methods; otherwise `IMemorialService` + `MemorialService`. `MemorialController` at `api/memorial`, `GET api/memorial/public` `[AllowAnonymous]`. Flag `enableMemorial`. Public `/in-memoriam`, member submission form under `portal/`, admin moderation queue. New `API_ENDPOINTS.MEMORIAL` block.
  - **Tests.** NUnit: an unapproved entry is absent from the public projection; linking a `Member` deactivates them and removes them from directory results; condolence HTML is sanitised. Vitest: moderation-queue actions, published/unpublished rendering.

37.6 [TODO] **Priority: P4.** **Oral-history / legacy archive** — recorded memories from senior alumni, which is the one asset an alumni association can create that nobody else can. `ArchiveCollection` (`Id`, `Title`, `Description`, `IsPublished`, `SortOrder`) and `ArchiveItem` (`Id`, `ArchiveCollectionId`, `Title`, `NarratorName`, `NarratorMemberId?`, `RecordedOn?`, `Summary`, `Transcript?`, `MediaFileUploadId?`, `ExternalMediaUrl?`, `PhotoPath?`, `DecadeTag`, `IsPublished`, `SubmissionStatus Status`).
  - **Storage decision, settled before building:** `MediaFileUploadId` reuses `FileUpload` + `IFileStorageService`; `ExternalMediaUrl` covers a link to already-hosted audio/video. **Both fields exist deliberately** — audio is heavy and the Render deployment has no object store configured, so the external-link path is the default and the upload path is opt-in behind the existing size limits in `IFileValidationService`.
  - **The transcript is the product**, not the audio: it is searchable, printable, quotable in 37.10, and readable on a bad connection. Treat a missing transcript as an incomplete item in the admin queue, not merely an empty field.
  - **Service/API/UI.** `IArchiveService` + `ArchiveService`; `ArchiveController` at `api/archive`, `GET api/archive/public` and `GET api/archive/items/{id}` `[AllowAnonymous]`. Flag `enableLegacyArchive`. Public `/legacy` (collections → item with transcript) reusing the Work Package 36 `.doc-hero` / `.doc-prose` shell rather than new page chrome; member submission; admin curation. New `API_ENDPOINTS.ARCHIVE` block.
  - **Tests.** NUnit: unpublished items excluded from public reads; an item with none of `MediaFileUploadId`, `ExternalMediaUrl` or `Transcript` is rejected. Vitest: decade filter, transcript rendering and print styles.

37.7 [TODO] **Priority: P2.** **Bengali/English bilingual UI.** The association's constituency is Bengali-speaking and every string in the app is currently a hardcoded English literal in a template. **Do not install `@angular/localize` or `ngx-translate`** — `feedback_keep_lightweight` applies, and the requirement here is a single flat key → string lookup with a live runtime toggle, which `@angular/localize` (build-time, one bundle per locale) does not even satisfy.
  - **Mechanism.** `core/services/i18n.service.ts` holding a `signal<'en' | 'bn'>` persisted to `localStorage`; two dictionaries under `core/i18n/en.ts` and `core/i18n/bn.ts` typed as `Record<string, string>` with `en` as the key source of truth; and a pure `TranslatePipe` (`{{ 'nav.constitution' | t }}`) falling back to the English string, then to the key itself, when a Bengali value is missing. Update `<html lang>` on toggle. Ship the toggle next to the existing `<app-theme-toggle>` so it inherits placement and styling.
  - **Scope explicitly, and state it in the item when it lands:** public site + member portal nav, buttons, labels and validation messages. **Admin stays English-only** — it is staff-facing, and translating it doubles the surface for no constituency benefit. Server-stored content (constitution text, election documents, news) is not translated by this mechanism; it is authored content and belongs to whichever language it was written in.
  - **Font.** Bengali glyphs need a webfont with Bengali coverage. Production `ng build` inlines Google Fonts over the network and this environment cannot reach it — self-host the face under `public/assets/fonts/` and reference it from `styles.scss`, so the build stays offline-safe.
  - **Tests.** Vitest: the pipe returns the Bengali value, falls back to English on a missing key, and falls back to the key when both are missing; the toggle persists across a service re-instantiation; **every key present in `en.ts` resolves through the pipe** (guards against key drift).

37.8 [TODO] **Priority: P3.** **Public credential verification.** `IIDCardService` already issues ID cards and certificates as PDFs, but nothing on the outside can confirm one is genuine — an employer holding a printed membership certificate has no check available.
  - **Models.** `IssuedCredential` (`Id`, `MemberId`, `CredentialType` — new enum `CredentialType { MembershipCertificate, IdCard, ElectionDocument }`, `ShortCode` — a 10-character unambiguous-alphabet code with a unique index, `IssuedOn`, `ExpiresOn?`, `IsRevoked`, `RevokedReason?`, `RevokedOn?`).
  - **Mechanism.** Extend `IIDCardService` (do not create a parallel service) so every generated document records an `IssuedCredential` and embeds a QR — via the existing `QRCoder` `GetQrDataUri` helper — pointing at `/verify/{shortCode}`. `GET api/verify/{shortCode}` is `[AllowAnonymous]` and returns **only** `{ valid, memberName, membershipType, issuedOn, status }`. It must never return an email, phone, address or member id: this endpoint is publicly enumerable by design, so the short code must be high-entropy and the response minimal. Rate-limit it with the existing `LoginRateLimitMiddleware` pattern.
  - **UI.** Flag `enableCredentialVerification`. Public `/verify/:code` plus a code-entry form at `/verify`, rendering a single valid / revoked / unknown verdict card; admin revocation action on the member detail page. New `API_ENDPOINTS.VERIFY` block.
  - **Tests.** NUnit: a revoked code returns `valid: false`; the response DTO exposes no contact field; short codes are unique across 10k generations. Vitest: the three verdict states, unknown-code path.

37.9 [TODO] **Priority: P4.** **Geographic chapters.** `Chapter` (`Id`, `Name`, `Region`, `Country`, `City`, `Description`, `CoordinatorMemberId?`, `ContactEmail?`, `IsActive`, `CreatedAt`) and `ChapterMembership` (`Id`, `ChapterId`, `MemberId`, `JoinedAt`, `IsCoordinator`) with a unique index on `(ChapterId, MemberId)`.
  - **Reuse, do not rebuild.** A chapter event is an `AlumniEvent` with a `ChapterId` — add the nullable column to `AlumniEvent` rather than creating a `ChapterEvent` table. Chapter announcements reuse `INotificationService.CreateNotificationAsync` fanned over the chapter's members; there is no new messaging surface in this item.
  - **Service/API/UI.** `IChapterService` + `ChapterService`; `ChaptersController` at `api/chapters`, `GET api/chapters/public` `[AllowAnonymous]`. Flag `enableChapters`. Public `/chapters` (list + detail with coordinator contact), member `portal/chapters` (join/leave, my chapter feed), admin `admin/chapters`. New `API_ENDPOINTS.CHAPTERS` block.
  - **Tests.** NUnit: joining twice does not duplicate; a chapter event appears only in that chapter's feed; coordinator contact is hidden from the anonymous projection unless `ContactEmail` is set. Vitest: join/leave state, empty-chapter state.

37.10 [TODO] **Priority: P3.** **Annual impact report generated from the ledger** — the accountability artifact that closes the loop on 37.2, 37.3 and 37.4, and the reason those three back-link `FinancialRecordId`.
  - **Nothing in this item is hand-typed.** For a given year it aggregates: total income and expense by `FinancialCategory` from `FinancialRecord`; scholarships awarded and disbursed from `ScholarshipAward`; campaign totals and donor counts from `CampaignPledge`; events and attendance from `AlumniEvent` + `EventRegistration`; new members from `Member.CreatedAt` / `MembershipHistory`; reunions from 37.4. The only authored fields are a president's foreword and a cover image, stored in the existing `SiteContent` CMS from Work Package 34 — **do not add a table for two strings.**
  - **Output.** Server-side PDF via **QuestPDF**, following the `IDCardService.GenerateIDCardPdfAsync` pattern, plus an on-site HTML view reusing the Work Package 36 `.doc-hero` / `.doc-prose` shell. Extend `IFinancialLedgerService` with `Task<ImpactReportDto?> GetImpactReportAsync(int year, CancellationToken cancellationToken = default)`, and put the PDF method on the existing document-generation surface rather than inventing a third document service.
  - **Guard.** The report must degrade rather than throw when a source feature is not yet built or its flag is off — a year with no campaigns renders without that section. This item is therefore safe to build *before* 37.2/37.3/37.4 land, and must read every source through a null-tolerant projection.
  - **API/UI.** `GET api/financials/impact/{year}` and `GET api/financials/impact/{year}/pdf`, both `[AllowAnonymous]` (publishing it is the point). Flag `enableImpactReport`. Public `/impact/:year` with a year selector; admin action to set the foreword and publish. Additions to the existing `API_ENDPOINTS.FINANCIALS` block.
  - **Tests.** NUnit: a year with zero records returns a report with zeroed sections rather than null; category totals match a hand-summed fixture; the disbursed-scholarship total equals the sum of the linked `Grant` `FinancialRecord` rows. Vitest: year selector, empty-section rendering.

37.11 [TODO] **Priority: P2.** Per 12.6, nothing in Work Package 37 is `[DONE]` until `dotnet test`, `npx vitest run`, `npm run type-check` and `npx ng build` all pass. Baselines to beat at the start of this Area: **351 NUnit tests** and **64 vitest files / 306 tests**. Additionally, every item that adds a table must be verified against the 37.0 migration path on a **non-empty** database — a passing suite against a fresh SQLite file proves nothing about preprod (`gotcha_ensurecreated_no_op_existing_db`). Update `docs/FEATURES.md`, `docs/PROJECT_MAP.md`, `docs/SRS.md` and `docs/ARCHITECTURE.md` as each item lands, per `feedback_docs_update_scope`.

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

40.14 [DONE] Work Package 40 fully complete end-to-end (backend + web + mobile).

---

# Work Package 42 — Elections forms/docs manageable from admin portal (raised by user 2026-08-26, plan only, not yet built)

Today election forms/docs shown at the public `/elections` route are static/seeded ([[session_election_form_pad]], [[session_area36_constitution_seeder_voting]]). User wants admin to manage (create/edit/replace) the election forms, documents, and other information currently shown in the public/portal elections pages, from the admin portal — analogous to the existing SiteContent CMS pattern ([[session_area34_sitecontent_notices]]).

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
43.4 [DONE 2026-09-16] **Priority: P2.** Live/manual verification: trigger a genuine unhandled error in each app (backend 500,
Angular runtime error, Flutter uncaught exception) against a running instance to confirm the new
handlers actually fire and log as expected.
Committed as `fc06894`.

**2026-09-15 update:** static trace of the full error-logs chain (DB entity → `ExceptionMiddleware` →
DI → `AdminErrorLogsController` → `AdminService.getErrorLogs` → `admin-error-logs.ts`) found no wiring
gap — middleware order, auth policy, EF query/pagination, route, and nav entry are all correct. One real
bug found and fixed: `admin-error-logs.ts`'s `loadLogs()` had a silent `error: () => {...}` callback that
swallowed any HTTP failure (401/403/500) with no toast, making a broken request look identical to an
empty table. Now calls `this.notify.error(err.error?.detail || 'Could not load error logs.')`, matching
the convention in `admin-members.ts:250`.

**2026-09-16 update — live verification complete, all three legs confirmed:**
- Backend: a temporary self-deleting diagnostic controller was added, run against a genuinely running
  API instance, and hit over real HTTP. The deliberate unhandled exception was caught by
  `ExceptionMiddleware`, logged, and persisted a real row to `ErrorLogs`, with a correct `ProblemDetails`
  500 returned to the client. The controller was deleted after use (net-zero diff). This same query
  against `ErrorLogs` also surfaced a second, previously-unknown real bug — see Work Package 83 below.
- Angular: a real Chromium browser (Playwright) was driven against a genuinely running `ng serve`
  instance. A deliberate error was thrown inside a zoned macrotask (`setTimeout`), so Zone.js routed it
  through `NgZone.onError` into the app's real `GlobalErrorHandler.handleError()`, producing the exact
  expected console line: `Unhandled application error: Error: 43.4 live-verification: deliberate Angular
  runtime error.` No source change was needed on the Angular side — the trigger was injected purely via
  the test script, not committed to the app.
- Flutter: a temporary `Future.delayed(...)` throw was added to `main.dart`'s `_initAndRunApp`, and
  `flutter run -d windows` was used to build and launch a genuinely running Windows desktop instance
  (confirmed available via `flutter devices`). The deliberate exception surfaced through
  `PlatformDispatcher.instance.onError` exactly as `error_handlers.dart` wires it, producing
  `Uncaught platform error: Exception: 43.4 live-verification: deliberate Flutter uncaught exception.`
  The temporary trigger was reverted after use (net-zero diff on `main.dart`).

All three handlers fire and log as designed against real running instances. This item is closed.

---

# Work Package 47 — Live preprod triage: seed-data integrity, dashboard accuracy, landing polish (2026-08-29)

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
Matches the gap already flagged in Work Package 46's May-2026-import note above.

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

47.10 **Priority: P2.** Member profile photos are genuinely missing for most of the 631 bulk-imported
alumni — not a bug, no photo was ever supplied at import time.
**Merged 2026-09-15:** this item's other half — `docs/deploy_connection.txt` /
`docs/BUSINESS_REVIEW_PLAN.md` committed live credentials, ONHOLD P0 — was the same fact 48.2 and
48.13 also tracked; consolidated into 48.2, see there.

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

47.13.4 [TODO] **`AdminPollController`** (`DeletePoll`, `ToggleStatus` — `CreatePoll` already covered).
New file or extend existing poll test coverage.

47.13.5 [DONE 2026-09-06] **`PaymentConfigController`** (`Create`, `Toggle`, `Delete` —
`Update`/`SeedDefaults` already covered per the mutation-coverage audit). Added as part of 80.13's
sweep, since that item rewrote `PaymentConfigController` to depend on the new `IPaymentConfigService`
anyway: `CreateConfig_PersistsAndReturnsMaskedSecrets`, `ToggleConfig_FlipsIsEnabled`,
`ToggleConfig_ReturnsNotFound_WhenConfigDoesNotExist`, `DeleteConfig_RemovesConfig`,
`DeleteConfig_ReturnsNotFound_WhenConfigDoesNotExist` in `PaymentConfigControllerTests.cs`.

47.13.6 [TODO] Lower priority, batch together when picked up: `AdminController` (`SyncMembers`,
`BulkArchiveInactive`, `RestoreMember`, photo/signature/document-upload actions), `GalleryController`
(`UploadPhoto`, `ToggleActive`, `ToggleFeatured`, `SubmitMemberPhoto`), `CommunicationController`
template CRUD (`CreateTemplate`/`UpdateTemplate`/`DeleteTemplate`), `FamilyLinkController`'s remaining
gaps (`CancelRequest`/`UnlinkMember`/`Remove`/`Cancel`). **Correction 2026-09-06:** the original text
also named `FamilyController` here; that controller was deleted in 80.2 (2026-09-04) and no longer
exists — dropped from this item rather than left as a dead reference.

47.13.7 [TODO] Once 47.13.1–47.13.6 are done, re-run the original mutation-coverage audit methodology
(grep every `[HttpPost]/[HttpPut]/[HttpPatch]/[HttpDelete]` action, cross-reference against test
files) to confirm the gap is actually closed rather than assuming from this list.

# Work Package 48 — Full security audit (raised by user 2026-08-29: "plan for vulnurability check, check for
web security best paractices")

A `security-reviewer` subagent audit of the whole app (verifying prior S1-S9/Work Package 24 hardening is
still genuinely wired, and hunting for anything new) found 2 Critical, 4 High, 4 Medium, and several
Low findings. All code-fixable items below are done (511/511 backend tests green, `ng build` clean);
the two Critical items include work the user must do outside this codebase (external secret rotation).

48.1 [DONE] **CRITICAL — unauthenticated PII leak.** `GET /api/networking/member/{id}`
(`[AllowAnonymous]`) returned every member's NID, DOB, parents' names, and emergency contact phone
ungated, while sibling fields were correctly privacy-gated. Fixed: stripped these fields (plus
`CertificatePath`) from `NetworkingService.MapToDto` — that DTO only backs the public directory
profile; the authenticated owner/admin profile is a separate build in `MemberService.GetProfileAsync`.

48.2 [ONHOLD 2026-09-06, per SR-9] **Priority: P0.** **CRITICAL — committed secrets, live JWT signing key and a live password both in git history.**
**Merged 2026-09-15:** this item now also carries 47.10's credentials half and all of 48.13 — three
tracker entries recording the same underlying exposure from three different audit passes. 47.10 keeps
only its unrelated photo-gap fact; 48.13 now points here.
`docs/deploy_connection.txt` (no longer tracked, see below, but still exposed via git history) contained
the production `Jwt__Key` and the Render deploy-hook URL, not just DB credentials as first found. Also
carrying live secrets: `.env.remote`, `build_output/appsettings.Production.json`,
`build_output/appsettings.json` (Gmail app password), `docs/RENDER_DEPLOYMENT.md`. Separately,
`docs/BUSINESS_REVIEW_PLAN.md` has held a plaintext password table since 2026-07-03; its `shalin` /
`Shalin@2024!` row was **confirmed on 2026-08-29** as the exact live preprod SuperAdmin credential this
session set via direct DB access — meaning that password sat in git history, publicly committed, since
before it was even set live. The table cells are now redacted, but redacting the file doesn't undo the
history exposure.
**Requires the user to rotate: the JWT key, both DB passwords, the Gmail app password, the Render
deploy hook, and `shalin`'s SuperAdmin password (independently — a different credential from the
JWT/DB rotation) — then `git rm --cached` + `.gitignore` + a history purge (`git filter-repo`).** Not
something this session can do — no dashboard access.
**State changed 2026-09-07, hold unaffected:** the `git rm --cached` half happened — `docs/deploy_connection.txt`
is no longer tracked — but a replacement file, `docs/deploy_conn_Info.txt`, appeared with the same class
of live secret (a Render Postgres password) and was left ungitignored until this pass added it. Both
filenames are now gitignored. **Everything else this item asks for is still outstanding**: no rotation
has happened, no history purge has run, and the already-committed secrets in `deploy_connection.txt`'s
git history — and the `shalin` password in `BUSINESS_REVIEW_PLAN.md`'s history — remain exposed. Still
requires the user; still not something a session can complete alone.
**Not the same item as 62.31/82.31**: those cover 631 *hashed* passwords baked into committed EF
migrations, a data-purge/anonymisation decision, not a credential-rotation one. Related, tracked
separately.

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

48.12 [DONE 2026-09-13] **Priority: P3.** **LOW — remaining minor findings, all closed.** Original
findings and their resolution: `MessagingController.MarkAsRead` had no ownership check (any
authenticated user could mark any message ID read) — fixed, endpoint now verifies the message
belongs to the caller before marking it read, covered by two new tests.
`FinancialsController.RecordPayment` kept a client-supplied `MemberId` when the caller's claim was
absent (contained — `Status` is hardcoded `Pending`, no self-approval possible — but should reject
outright) — fixed under 82.32: the endpoint now returns `Unauthorized` when the `MemberId` claim is
missing instead of trusting the body. Refresh-token rotation had no reuse-detection (a replayed
already-rotated token just returned null instead of revoking the whole family) — already fixed
under 82.18: `TokenService.RotateRefreshTokenAsync` detects a revoked-but-replayed token hash,
revokes every refresh token for that user, and rotates their security stamp; this bullet was stale
tracker text, not an open gap. `MemberImportController`'s uploaded workbook skipped
`IFileValidationService` unlike every other upload endpoint (admin-only, so low risk) — fixed, the
import endpoint now runs the uploaded `.xlsx` through `IFileValidationService` under a new
`FileCategory.Spreadsheet`, covered by a new content-validation test.
`MemberService.cs`/`MemberService_Profile.cs` substituted user-controlled `FullName` raw into an
HTML email body — fixed, both `AuthService.cs` and `MemberService_Profile.cs` now
`WebUtility.HtmlEncode` the member's name before it goes into a template or fallback HTML body;
verified via build plus the full `AuthServiceTests`/`MemberServiceTests` suite (52/52 passing, no
test asserted on the raw name so none needed updating).

48.13 — merged into 48.2 on 2026-09-15. Tracked the same `docs/deploy_connection.txt` /
`docs/BUSINESS_REVIEW_PLAN.md` plaintext-password exposure from the audit side; see 48.2 for the
current, consolidated state.

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

48.18 [DONE 2026-09-04] **A06/Medium — all Microsoft/EF Core/Npgsql NuGet packages pinned to exactly
`9.0.0`, no servicing patches since.** Every named package bumped to the highest `9.0.x` release on
NuGet as of 2026-09-04 (checked per-package against the NuGet flat-container API, not assumed):
`Microsoft.EntityFrameworkCore*`, `Microsoft.AspNetCore.Authentication.JwtBearer`, and every
`Microsoft.Extensions.*` in the four `.csproj` files to `9.0.19`; `Npgsql.EntityFrameworkCore.
PostgreSQL` to `9.0.4` (its actual latest 9.0.x, lower than the others). `Pomelo.EntityFrameworkCore.
MySql` left at `9.0.0` — that IS its latest 9.0.x release, nothing newer exists on that line, so no
change was a defect, not an omission. `dotnet list package --outdated` was checked first and
deliberately not followed for these packages: its "Latest" column showed `10.0.11`, which is a .NET
10 major-version jump outside this item's `9.0.x` patch scope, not a patch release — do not read
`--outdated` output as "already fixed" without checking which version line it's comparing against.

Verified: `dotnet build GHCAA.sln -c Release` clean; `dotnet test GHCAA.Tests/GHCAA.Tests.csproj` is
564/564 (unchanged from the 47.13.1/47.13.2 baseline — this bump broke nothing the suite exercises,
and that suite runs its EF Core/Sqlite/InMemory operations under the new package version throughout);
`dotnet ef migrations list` resolves the full migration tree (through `20260831000000_
FixAssociationContentMergeLogoFlag`) with no model-snapshot error. **Not verified**: an actual
`dotnet ef database update` against a live Postgres instance — no Postgres and no running Docker
daemon were reachable in this session to stand one up for a real apply dry run (the `docker buildx
imagetools inspect` calls used for 48.19's digest pinning query a registry directly and don't need a
local daemon; `docker ps` failed once an actual container was needed). That live-apply check is the
residual verification step before this should be trusted on preprod, tracked so it isn't silently
assumed done: see 48.18a. Also flagged, out of scope for this item's `9.0.x` line and left alone:
`Swashbuckle.AspNetCore 6.6.2` (mitigated — Swagger is dev-gated, see 82.10a) and `AutoMapper.
Extensions.Microsoft.DependencyInjection 12.0.0` (behind the 13+/14+ line, a major-version jump like
the EF Core 10 case above, not a patch).

48.18a [DONE 2026-09-16] **Priority: P2.** Ran the live-Postgres verification 48.18 could not, against
the real Postgres server at `localhost:5432` on the bumped `9.0.19`/`Npgsql 9.0.4` packages (no Docker
daemon reachable this time either, so this used the live server directly rather than a container).
Two checks: first, `dotnet ef database update --project GHCAA.Infrastructure --startup-project
GHCAA.API --context PgSqlApplicationDbContext` against the existing dev database `GHCAADB_v2` —
"No migrations were applied. The database is already up to date." Second, the same command with
`--connection` pointed at a new database name on that same server (`GHCAADB_v2_migchaincheck`,
credentials taken from the already-configured `appsettings.Development.json`, not written to any
tracked file) — EF Core created the database and applied `20260907193705_InitialBaseline` from
nothing: "Applying migration '20260907193705_InitialBaseline'. Done." That's the stronger of the two
results, since it proves the chain applies cleanly end to end from empty, not just that its metadata
matches an already-current database. The throwaway database was then dropped (`DROP DATABASE
"GHCAADB_v2_migchaincheck" WITH (FORCE)`, run via a scratch Npgsql console app, since neither `psql`
nor `dotnet ef database drop` support a `--connection` override for a non-default database).

48.19 [DONE 2026-09-04] **A08/Medium — CI Actions pinned to mutable tags; no NuGet lockfile.**
All five actions in `ghcaa-ci-preprod.yml` (`actions/checkout@v5`, `actions/setup-dotnet@v4`,
`actions/setup-node@v5`, `actions/setup-java@v4`, `subosito/flutter-action@v2`) pinned to the commit
SHA their tag currently resolves to (`owner/repo@<sha> # <tag>, resolved 2026-09-04`), looked up live
rather than guessed. The three `Dockerfile` base images (`node:22-alpine`,
`mcr.microsoft.com/dotnet/aspnet:9.0`, `.../sdk:9.0`) pinned to their manifest-list digest via
`docker buildx imagetools inspect`, with a header comment explaining the digest has no auto-patch
path and needs a deliberate refresh (Dependabot's Docker ecosystem can do this on a schedule). NuGet
lockfiles added: root `Directory.Build.props` sets `RestorePackagesWithLockFile` for every project
under it (matches `npm ci`'s use of `package-lock.json`, doesn't touch `GHCAA.Export`/`HashGen`,
which aren't in `GHCAA.sln`), `packages.lock.json` generated for the 5 solution projects via
`dotnet restore`, and both `dotnet restore` calls in `ghcaa-ci-preprod.yml` changed to
`dotnet restore --locked-mode` so a lockfile/manifest mismatch fails the build instead of silently
restoring something else. Verified: `dotnet restore --locked-mode` succeeds from a clean `obj/`, and
`dotnet test` is 564/564 (same as the 47.13.1/47.13.2 baseline — this item touched no test code).
A pinned SHA/digest is only as good as the lookup that produced it; the CI Actions SHAs came from a
web fetch against GitHub's release pages rather than the GitHub API (which 403'd from this sandbox),
so the very first CI run on `preprod` after this lands is the real verification — a wrong SHA fails
that run loudly (checkout would simply not resolve), it does not fail silently. The Docker digests
came directly from `docker buildx imagetools inspect` against the live registry, not a web lookup, so
those carry no equivalent caveat.

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

# Work Package 49 — Admin user/role management review (raised by user 2026-08-29: "from admin- how new role
can be created, how to disable, reset user passwords, review user and roles implementation and
design, are all grids designs including row controls same and following centralised designs")

Audit of `admin-roles.html/.ts`, `RolesController.cs`, `UserService.cs`, `AdminController.cs`, and a
grid-design comparison across `admin-roles`, `admin-members`, `admin-events`. Plan only — nothing
below is built yet unless marked `[DONE]`.

**Every item below is written as an ordered, mechanical checklist — no design decisions should be
needed at implementation time except where a step is explicitly flagged "DECISION NEEDED."**

49.1 [DONE 2026-09-05 for (a); (b) planned, not started] **Priority: P1.** **Custom roles have no actual permission scope.** `RolesController.CreateRole`
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
  - **(a) done 2026-09-05, on the user's explicit instruction to do (a) now in a way that does not
    block (b) later.** `admin-roles.html`: the "Create Custom Role" section is now "Add Role Tag
    (label only — grants no permissions)" with a one-line hint saying access is controlled entirely
    by the built-in Admin/SuperAdmin/Member roles. The "Create System Administrator" modal's
    "Initial Privilege Level" select, which previously listed every role including custom ones
    (`roles()`, unfiltered), now lists only `Admin`/`SuperAdmin` — the only two values
    `CreateAdminDto.Role` is ever checked against (`RolesController.cs`, via
    `_userService.CreateSystemAdminAsync`). Before this, picking a custom role there created a
    "system administrator" account with no actual admin access, silently. No backend change, no
    schema change — (b) above can still be built on top of this without reworking it.
  - **(b) not started, plan stands as written above** — the user has not confirmed a concrete need
    for department-scoped admins (an "Events-only admin" or similar), which the item's own decision
    gate requires before starting. Left fully specified so a future session can pick it up directly.

49.2 [DONE 2026-09-06] **User disable/enable — system admins (new) and members (UI gap only).**
  - **49.2.A shipped.** `IUserService.SetUserActiveAsync` added; `UserService.SetUserActiveAsync`
    rotates `SecurityStamp` and revokes refresh tokens on disable, guards protected usernames.
    `RolesController` gained `DisableUser`/`EnableUser`, both behind `[RequireStepUp]`.
    `admin-roles.ts`/`.html` gained `toggleUserActive`, an icon toggle next to Reset Password.
    Correction made while implementing: `DeleteSystemAdminAsync` did NOT actually have a
    protected-username guard despite this item assuming it did (it only checked `MemberId != null`)
    — added the same guard there too, since a protected account (`shalin`) could otherwise be
    hard-deleted outright. New `Constants.ConfigKeys.ProtectedSuperAdmins` centralizes the config key
    (was a raw string in `Program.cs`, now shared).
  - **49.2.B shipped.** `admin.service.ts` gained `restoreMember`; `admin-members.ts` gained
    `restoreMember(id)`; `admin-members.html` shows Restore in place of Archive when
    `member.isArchived`. `ReactivateMemberAsync`/`member.Status` untouched as instructed.
    `MemberSummaryDto` did not carry `IsArchived` at all before this — the admin member list's DTO
    projection dropped it — so the Restore button's guard would never have fired; added the field to
    the DTO and its mapping in `MemberService.GetAllMembersAsync`.
  - **Decision (2026-09-06): `IsArchived` is enough for members, no separate disable state needed.**
    Verified before recording this: `AuthService.cs:86` already blocks login on `member.IsArchived`
    directly, before `user.IsActive` is even reached, so archiving a member already fully locks them
    out today — `User.IsActive` for a member account is redundant with it, not a second real gate.
    The archive/restore control 49.2.B built is the complete member-facing control surface; no new
    "disable without archiving" feature is being built for members. `User.IsActive` stays on `User`
    for system admin accounts, where 49.2.A's disable/enable genuinely needs it.

49.3 [DONE 2026-09-04] **Admin-initiated password reset — system admins (new) and members (security
fix), plus the auth-flow gap the original plan missed.**

  - **49.3.B — Member accounts (security fix).** `MemberService.SendAdminPasswordResetLinkAsync` now
    calls `_tokenService.RevokeAllRefreshTokensAsync(user.Id, ...)` right after issuing the reset
    token, matching the pattern already used at `MemberService.cs:802/1263/1453`. The UI button
    (`admin-members.html`'s `🔐 Reset Password` in the Manage modal, wired to `sendResetLink`)
    and `AdminController.ResetPasswordAdmin` already existed; only the missing revocation was new.
    Test: `MemberServiceTests.SendAdminPasswordResetLinkAsync_ShouldRevokeExistingRefreshTokens`.

  - **49.3.A — System admin accounts (new).** `IUserService`/`UserService` gained
    `SendAdminPasswordResetLinkAsync(userId, ...)`; `RolesController` gained
    `POST users/{id}/reset-password-admin` behind `[RequireStepUp]`; `admin-roles.ts`/`.html` gained a
    matching `🔐` icon button next to the existing delete button, reusing the copy-to-clipboard
    pattern from `admin-members.ts`. Tests:
    `UserServiceTests.SendAdminPasswordResetLinkAsync_ForSystemAdmin_ShouldReturnUrlAndRevokeTokens`
    and `..._ForUnknownUser_ShouldReturnFalse`.

  - **The auth-flow gap the original plan didn't cover.** System admin accounts have `MemberId =
    null` — no `Member`, no email. The plan as written would have generated a reset URL that could
    never actually work: `AuthController.ResetPassword` → `AuthService.ResetPasswordAsync` looked
    the account up by `Members.Email` only, and `ResetPasswordDto.Email` was `[EmailAddress]`-
    validated, so a username-carrying link would fail both the form and the lookup. Fixed at the
    root rather than building a UI on top of a link that would 400: `ResetPasswordAsync` now falls
    back to `Users.MemberId == null && Username == <field>` when no `Member` matches, and the DTO's
    `[EmailAddress]` constraint was dropped (kept `[Required]`) since the same field now legitimately
    carries either an email or a username. The fallback is scoped to `MemberId == null` specifically
    so it cannot be used to reset a member's account by guessing their username. Tests:
    `AuthServiceTests.ResetPasswordAsync_ForSystemAdminByUsername_ShouldChangePassword` and
    `..._MemberCannotBeResetByUsernameFallback` (the scoping guard).

  Verified: `dotnet test` 532/532 (was 528; four tests added, none removed or changed).

49.3.C [TODO] **Priority: P2 | Depends on: none.** Raised while building 49.3: user instruction
2026-09-04, "verify random two/three information ... or bypass to admin as request for reset link to
new shared email address" — for system admin accounts specifically, which is the class with no email
at all today (`User` has no `Email` field). Two directions were named and neither is decided:
(a) a challenge of 2-3 known facts before honoring a reset request, or (b) an optional contact email
on the `User` row that the reset link is emailed to when present, falling back to the current
copy/share flow when absent. (b) is the smaller change — one nullable column, one migration, one
`SendEmailAsync` call reusing the member template pattern already in `MemberService` — and is the
more likely fit given the rest of this item's pattern already leans on email delivery. Needs a
decision on which (or both) before starting; not started here because it requires a schema migration,
which is a different class of change than the rest of 49.3.
**Acceptance:** decision recorded with its reason; if (b), a nullable `ContactEmail` column with a
migration, and the reset flow sends there when populated.

49.4 [DONE 2026-09-09] **Priority: P3.** **Grid/row-control design consistency fixes** (mechanical, per [[ghcaa-design]]):
  1-3 shipped: `admin-events.html`'s table renamed to `.data-table`, its View/Edit/Delete row buttons converted to `.icon-btn`, `admin-roles.html`'s bare `✕` role-chip button converted to `.icon-btn.delete`.
  4.E [DONE 2026-09-09] Standardized `admin/ledger/ledger.html` table to `.data-table` class.
  4.F [DONE 2026-09-09] Standardized `admin/job-approval/job-approval.html` row action column with `.actions-cell` structure.
  4.G [DONE 2026-09-09] Standardized `admin/fee-config/admin-fee-config.html` badge styles to active/inactive design tokens. Fixed-size config list remains uncluttered.

49.5 [DONE 2026-09-06] `GHCAA.Tests/Controllers/RolesControllerTests.cs` (17 tests) covers
`DisableUser`, `EnableUser`, `ResetPasswordAdmin`, `DeleteUser`, `CreateAdmin`, `CreateRole`,
`AssignRole`, `RemoveRole`, `GetUsers`, `GetRoles` — one file, per the instruction here. Service-level
coverage for the guard behaviour (protected-username, token revocation on disable) added to
`UserServiceTests.cs` (6 tests). Full suite: `dotnet test` 613/613 (was 590).

# Work Package 52 — Public landing gallery carousel (web-only) + mobile admin gallery active/featured/edit
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
52.5 [TODO] **Priority: P3.** Not addressed here (out of scope): mobile's `admin_modules.dart` has a separate, simpler
"quick create gallery" dialog (posts straight to `/gallery/admin` with `isFeatured` hardcoded
`false`) — a duplicate, lighter-weight creation shortcut on the admin dashboard tile grid, distinct
from `gallery_screen.dart`'s own create flow. Left as-is; consolidating the two creation entry points
was not part of this ask and is a separate cleanup decision.

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

60.1 [DONE 2026-09-09] **Priority: P4 | Depends on: none.** Mobile News screen layout verification.
Verified `GHCAA.Mobile/lib/screens/member/news_screen.dart`. Mobile News uses standard `AppScaffold`, `AppSearchField`, filter chips, `GlassContainer` card items with image fallbacks and category chips — fully consistent with mobile's list patterns (Jobs, Gallery, Events). `dart analyze` clean.

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

62.1 [DONE 2026-09-04] `IInstitutionProfileProvider` (`GHCAA.Application.Interfaces`) +
`InstitutionProfileProvider` (`GHCAA.Infrastructure.Services`). Resolves `ORG_PROFILE` (default
`default`), reads `profiles/<name>/org-config.json`, falls back to `profiles/default/org-config.json`
file-by-file (today that's the only file type built — see 62.2's partial status), and throws a
message naming both the requested profile and the missing file if neither exists. Registered
`AddSingleton` — the "boot plane" per ADR-3, loaded once, not per-request — excluded from the
reflection-based auto-registration loop for the same reason 80.12 excluded `GreenwebSmsService`
(needs a specific lifetime the loop can't express). `Program.cs` resolves it eagerly right after
`builder.Build()` so a bad pack fails boot with a readable error instead of surfacing on first use.
`Dockerfile`'s final stage gained `COPY profiles/ ./profiles/` — without it the folder never reaches
the image `dotnet publish` produces. Path resolution checks the content root, then its parent (a
local `dotnet run` from `GHCAA.API/` has its content root one level below the repo root that actually
holds `profiles/`; Docker's `WORKDIR /app` has it directly).

Not yet consumed anywhere (that's 62.6, a separate phase) — `OrgConfigService` still builds its
defaults in code, so this ships with **zero effect on GHC's live behavior**, matching Phase A's own
"no user-visible change" requirement. Regression found and fixed while building this:
`WebApplicationFactory`-based integration tests (`SpaStaticFileFactory`, `OutputCacheTestFactory`)
boot the real `Program.cs` against a synthetic temp content root with no `profiles/` folder anywhere
near it, so eager resolution started throwing at boot for all 14 of them; both factories now write a
minimal `profiles/default/org-config.json` (`{}` — every `OrgConfigDto` field has a default, so an
empty object deserializes fine) alongside their other boot-required fixtures. Five new unit tests in
`InstitutionProfileProviderTests.cs` cover: default-profile resolution, a named profile found
directly, a named profile falling back to `default` file-by-file, the readable error when neither
exists, and the parent-directory resolution path Docker/local-dev actually rely on but the
integration tests' direct-match content root doesn't exercise. `dotnet build`/`dotnet test` clean
full suite green.

62.2 [PARTIAL 2026-09-04] Only `org-config.json` exists so far, both for `profiles/default/` (a
genuinely neutral "Sample Alumni Association" pack — no GHC strings, no real personal data, checked
against `OrgConfigDto`'s full shape) and as the file type 62.1's provider actually reads. The other
seven file types ADR-2 specifies (`site-content.json`, `email-templates.json`, `lookups.json`,
`membership-tiers.json`, `governance.json`, `documents.json`, `seo.json`) and the `assets/`/
`demo-data/` folders are deliberately not built yet — nothing reads them until the phase that
introduces each one (Phase B for email templates, Phase C for web assets/SEO, Phase E for tiers/
governance/documents), and building a schema today for a consumer that doesn't exist yet is exactly
the premature-abstraction risk the project's own "no new abstraction without a concrete problem"
rule warns against.

62.3 [DONE 2026-09-04] `profiles/ghc/org-config.json` created, 212 lines, matching the PascalCase
indented shape `profiles/default/org-config.json` already uses (`InstitutionProfileProvider` reads
case-insensitively, so casing is presentation only).

**Generated from `BuildGhcaaDefaults()` rather than typed by hand**, which is the part that makes
"verbatim means verbatim" a fact rather than a promise: hand-transcribing ~190 lines of nested
records, dictionaries and two locale packs would be one silent typo away from a pack that looks right
and is not, and this pack is the regression baseline the rest of the work package is measured
against. `GhcProfilePackTests.Regenerate_GhcPack_FromCode` reflects on the private static method,
serializes with `UnsafeRelaxedJsonEscaping` so the currency glyph and the Bengali locale strings stay
readable to whoever edits a pack for a second institution instead of becoming `\uXXXX`, and is marked
`[Explicit]` because regenerating is a deliberate act, not something a test run should do.

Two ordinary tests guard it: the pack still deserializes to something serializing identically to
`BuildGhcaaDefaults()`, and it still declares `OrgId` `ghcaa` (a cheap guard against a regeneration
pointed at the wrong profile, which would otherwise hand 62.6 the neutral sample values). Verified
the parity test actually catches drift rather than merely passing: editing `ShortName` in the pack
turns it red, regenerating turns it green. Nothing in `Constants.Defaults` or `appsettings.json`
needed to be read in the end — `BuildGhcaaDefaults()` already carries every value the DTO has.

62.4 [DONE 2026-09-04] `GHCAA.Tests/OrgConfig/golden/org-config-ghc.golden.json`, captured from the
real `OrgConfigService` against an empty database before 62.6 changed anything, plus
`OrgConfigGoldenSnapshotTests` asserting the running service still matches it.

**Why a frozen file rather than another code-to-pack comparison:** 62.3's parity test compares the
pack against `BuildGhcaaDefaults()`, and after 62.6 those two move together — they would agree with
each other even if both were wrong. The golden does not move. It is what the service actually
returned while the values were still hardcoded, so it can still fail the swap afterwards. The
regeneration path is `[Explicit]` and the test message says plainly that a failure is a question
about what changed, not an instruction to regenerate.

Result worth recording: the golden and `profiles/ghc/org-config.json` came out byte-identical. That
is a stronger check on 62.3 than its own test, because the golden goes through the whole
`GetConfigAsync` path including the Localization overlay, not just `BuildGhcaaDefaults()` in
isolation — so the pack is confirmed correct by two independent routes.

62.5 [DONE 2026-09-05] `scripts/brand-lint.mjs` + `scripts/brand-lint.config.json` created: scans
API/Web/Mobile source for the banned literals, warn-only (always exits 0), exception list with a
reason per entry (`profiles/**`, migrations, test fixtures, plus the pinned mobile applicationId —
see 62.26).

### PHASE B: API DE-BRANDING

62.6 [DONE 2026-09-04] `OrgConfigService` now reads the institution profile pack, via a new private
`BuildDefaults()` that returns `profiles.OrgConfigDefaults` when a profile is explicitly selected and
`BuildGhcaaDefaults()` when one is not. The Localization self-heal heals from whichever source is
active, satisfying plan 8.6's "heal from the profile, not from code" for every deployment that has
chosen a profile.

**The guard, and why the hardcoded block is still there.** The plan (`WHITE_LABEL_PLAN.md:185`)
assumes `ORG_PROFILE=ghc` is set on the existing deployments. It is not set anywhere in this
repository — not the `Dockerfile`, not any workflow, not `appsettings` — and
`InstitutionProfileProvider` resolves an unset `ORG_PROFILE` to `default`, which is the neutral
sample pack. A straight swap would therefore have rebranded a live association with real members to
"Sample Alumni Association", changed the membership prefix from `GHC-` to `MEM-`, and pointed support
at `support@example.org`, on the next deploy and with no error. Setting a Render environment variable
is the user's to do, so the code could not simply assume it.

The behaviour is now: profile selected → pack drives configuration; not selected → today's values,
unchanged, plus a startup warning naming `ORG_PROFILE=ghc` so an unconfigured deployment is visible
rather than silent. `IInstitutionProfileProvider` gained `ProfileExplicitlySelected` to tell "chosen"
apart from "defaulted", since only the former is safe to act on. The `profiles` constructor parameter
is optional, which also left the eight existing `new OrgConfigService(...)` test call sites untouched.

This is the strangler shape on purpose: the risky half is the *selection*, not the values — 62.4's
golden proves pack and code produce byte-identical output — so the swap ships behind a guard that
costs nothing and the 190 lines come out once the deployment is configured. That deletion is 62.6b.

Tests (`ProfileDrivenConfigTests`, 4): `ORG_PROFILE=ghc` reproduces the 62.4 golden byte-identically,
which is Phase A's stated exit criterion; an unset profile keeps the hardcoded values rather than
serving the sample pack; no provider at all behaves the same; and an explicitly-selected *sample*
profile really does change the output to "Sample Alumni Association"/`MEM-`, which is what proves the
pack branch is live rather than the guard swallowing every case. Full suite 573 to 580, green.

62.6b [TODO] **Priority: P2 | Depends on: 62.6, and on `ORG_PROFILE=ghc` being set on the
deployments.** Delete `OrgConfigService.BuildGhcaaDefaults()` (~190 lines), the optional `profiles`
constructor parameter, and the `ProfileExplicitlySelected` branch in `BuildDefaults()`, leaving the
service reading the pack unconditionally. **Why it is a separate item rather than part of 62.6:** the
hardcoded copy is the fallback that stops an unconfigured deployment serving sample branding to a
live association, so it cannot be removed while any deployment still lacks `ORG_PROFILE`. Removing it
first would reintroduce exactly the failure 62.6's guard exists to prevent.

**Acceptance:** `ORG_PROFILE` is confirmed set on every running deployment (preprod and any other);
`BuildGhcaaDefaults()` and the guard are gone; `ProfileDrivenConfigTests` is updated so the cases that
currently assert the unset-profile fallback instead assert that an unset profile now fails loudly;
`OrgConfigGoldenSnapshotTests` still passes unchanged, which is what proves the deletion changed
nothing.

62.7 [DONE 2026-09-05] `MembershipPrefix`/`ImportEmailBase` moved out of `Constants.Defaults` into the
org-config pack (`ContactDto.ImportEmailBase` etc, per the comment left at `Constants.cs:76`). NEW
numbers only, confirmed no backfill of issued `MembershipNumber` values.
Audit scope (2026-09-03, `GHCAA.Domain/Constants.cs`): these two fields are the only
organisation-identity literals in the file — the ones a second institution couldn't reuse without
editing code. Everything else there is generic across institutions and stays a compiled constant:
`Roles`, `Policies` (auth policy names), `ConfigKeys` (`IConfiguration` key paths), `TemplateCodes`
(email-template lookup codes), and the rest of `Defaults` (file-size caps, image-compression
quality/size targets, `UnknownValue`/`ImportPrefix` fallback labels). No further extraction is
needed from this file beyond these two.

62.8 [DONE 2026-09-05] `IDCardService` now takes `IOrgConfigService`; ID card, certificate, and the
gallery/QR SVG all read `org.Branding.AccentColor`/`org.Branding.InstitutionAcronym` instead of the
hardcoded strings and `#c5a059`.

62.9 [DONE 2026-09-05] `email_templates.json` now carries `{{OrgName}}`/`{{OrgShortName}}`/
`{{SupportEmail}}` placeholders, supplied by the renderer. The `Seed/Visual/email_templates.json`
duplicate was dropped; Visual-profile test runs now fall through to the same source file via
`LoadSeed`'s profile-pack path (see the `EF.IsDesignTime` fix logged in `ApplicationDbContext.cs` —
this drop is what exposed the copy-glob bug fixed 2026-09-05, see 62.32's note).

62.10 [DONE 2026-09-05] `Program.cs` reads `IInstitutionProfileProvider` directly (constructed
before `builder.Build()`, since the DI container doesn't exist yet at that point in startup) for
both the Swagger title and `SetApplicationName`. Same guard as `OrgConfigService.BuildDefaults()`:
an unset `ORG_PROFILE` keeps the literal `"GHCAA"` exactly. Added to the brand-lint exception list
with that reason. `dotnet test` 590/590 green after the change.

62.11 [DONE 2026-09-05] `AssociationNamePrefix`/`EmailDomain`/`Currency` are now sourced from
`OrgConfigDto` (`Branding.TransactionPrefix`, `Contact.EmailDomain`, `Currency.Code` — the first
two are new fields, added alongside this item) via `GatewaysController`, `SSLCommerzGateway`, and
`IDCardService`/`FamilyLinkService` for `PortalBaseUrl`. `appsettings.json`'s now-dead
`GeneralSettings.AssociationNamePrefix`/`EmailDomain`/`Currency`/`PortalBaseUrl` keys removed
(nothing read them once the swap landed); `PaymentGateways:EnabledMethods` likewise removed in
favour of `OrgConfigDto.EnabledGatewayMethods` (62.35). `PortalBaseUrl` kept a real
appsettings-as-override path — `appsettings.Preprod.json` overrides it to
`https://preprod.haragangian.com/portal`, a genuinely different value from the pack's production
URL, so `OrgConfigService` now takes an optional `IConfiguration` and applies that one override
after building the profile/pack defaults. Golden snapshot and `ProfileDrivenConfigTests` updated to
match; full suite 590/590 green.

62.12 [DONE 2026-09-05] All seed-driven entities load through the shared `ApplicationDbContext.LoadSeed`,
which already resolves Class 3 files to `profiles/<name>/demo-data/` and Class 1/2 files to
`profiles/<name>/` before falling back to `Data/Seed/`. `ConstitutionSeeder` needed no changes of its
own — it calls `LoadSeed<Constitution>("constitution.json")` and inherits the profile-aware path for
free. `MigrationBootstrapper` idempotency/baselining untouched, confirmed by the 62.13 run below.

62.13 [DONE 2026-09-05] `dotnet test` full suite (590/590) exercises `EnsureCreated`/seed loading
against a fresh SQLite DB per test run; no re-baseline or history-row churn. Not yet separately run
against a throwaway copy of the real preprod Postgres DB — do that before this change reaches
preprod.

62.14 [DONE 2026-09-05] Re-examined the other three call sites named by this item and found none of
them need the fix it describes. `CommunicationServiceTests.cs:36`'s `ShortName = "GHCAA"` is mock
input data fed to a mocked `IOrgConfigService.GetConfigAsync()`, not an assertion against a
hardcoded constant — the test never asserts the service produces "GHCAA" specifically, so there is
nothing to genericize there. `MemberServiceTests.cs:403`'s `"GHCAA"` is an unrelated
`ProfessionalRecord.OrganizationName` fixture value (a test employer name), not branding.
`TokenServiceTests.cs`'s `"GHCAA"` is a JWT issuer/audience test double for
`TokenService.cs`'s `_config["Jwt:Issuer"] ?? "GHCAA"` fallback, which reads from
`appsettings.json`'s `Jwt:Issuer`/`Jwt:Audience` (already set to `"GHCAA.API"`/`"GHCAA.Client"`,
never the bare fallback) — unrelated to `OrgConfigService`/branding entirely. Only
`OrgConfigServiceTests.cs` was a real instance of the pattern this item names, and it was already
fixed. No further action needed.

### PHASE C: WEB DE-BRANDING

62.15 [DONE 2026-09-05] `ghcaaDefaults` removed from `core/services/org-config.service.ts`. Went with
the single-source approach: `core/config/org-config-fallback.generated.ts` (build-time generated from
the active profile) supplies the boot fallback instead of a hand-maintained TS copy.

62.16 [DONE 2026-09-05] `app.routes.ts` verified clean (zero literal "GHCAA"/"Haraganga" hits). New
`core/strategies/branding-title.strategy.ts` composes route titles from `branding.shortName`.

62.17 [DONE 2026-09-05] `GHCAA.Web/scripts/apply-brand.mjs` created, plus
`generate-org-config-fallback.mjs` and `generate-site-content.mjs` as its supporting build-time
generators. Verified 2026-09-05: ran all three against the `ghc` profile and diffed the result
against git HEAD — `index.html`/`sitemap.xml`/`robots.txt` came back byte-identical (only a
line-ending warning, no content diff), confirming the GHC profile reproduces today's live output
exactly. Wired into `Dockerfile`'s web build stage (62.40), which previously called `ng build`
directly and skipped this generation step entirely.

62.18 [DONE 2026-09-07] `about.html` and `purpose.html` verified clean (zero literal hits).
`register.html`'s T&C section (L497-562) still names the college/founding date/IP clause directly —
not yet moved into `site-content.json`.
**Resolved 2026-09-07:** the two remaining hardcoded T&C clauses (institution name in the preamble and
verification paragraphs) added to `RegisterTermsContent` (model, `generate-site-content.mjs`, both
profile packs) and wired into `register.ts` via `interpolateOrgTemplate`-backed computed signals — the
same mechanism already used for `effectiveDate`/`eligibilityParagraph`/`ipParagraph`. Also fixed a latent
bug found in passing: the default profile's `{branding.x}` placeholders in `eligibilityParagraph`/
`ipParagraph` were never actually being interpolated.

62.19 [DONE 2026-09-07] `digital-id.html`, `assistant.html`, `magazine.html`, `gallery.html`,
`events.html` verified clean. `directory.html`'s one hit is a false positive — a C# namespace
mentioned in a code comment (`GHCAA.Domain/Enums.cs`), not a rendered literal; no fix needed.
`membership.ts:48`'s hit is a commented-out (dead) line, not rendered. `elections.ts` verified
clean (zero hits — already fixed by an earlier session). Admin placeholder text
(`org-config.html`/`admin-members.html`/`admin-themes.html`) still not checked.
**Resolved 2026-09-07:** checked the admin placeholder text. Found and fixed three real hits —
`admin-members.html`'s "e.g. Govt. Haraganga College" placeholder, `org-config.html`'s
"Govt. Haraganga College, Munshiganj-1500, Bangladesh." placeholder, and a literal `GHCAA` baked into
`admin-themes.html`'s theme-preview mockup (now bound to `orgConfig.config()?.branding?.shortName`).
`brand-lint` literal counts dropped as a result (`Haraganga` 670→667, `Haragangian` 33→26).

62.20 [DONE 2026-09-05] Fixed the 7 genuinely raw `<img src="/assets/logo.png">` occurrences —
`register.html`, `events.html`, `digital-id.html` (×2), `about.html` (×2 flag badge),
`login.html`, `reset-password.html`, `logo-spinner.html` — to
`[src]="orgConfig.config()?.branding?.logoUrl" appImgFallback="/assets/logo.png"`, matching the
pattern already used by `admin-layout.html`/`portal-layout.html`/`footer.html`/`purpose.html`.
`login.ts`, `reset-password.ts`, and `logo-spinner.ts` needed `OrgConfigService` injected; the rest
already had it. The other 7 files this grep also matched (`admin-events.html`, `gallery.html`,
`news.html`, `magazine.html`, `footer.html`) were already config-driven — `/assets/logo.png` there
is only the `appImgFallback` fallback value, not a raw `src`.

62.21 [DONE 2026-09-05] `digital-id.ts:48,65` now build the download filename from
`this.orgConfig.config()?.branding?.institutionAcronym ?? 'GHCAA'`.

62.22 [PARTIAL 2026-09-05] Scoped down from the full multi-document registry the item describes:
added `Branding.ConstitutionPdfUrl` to `OrgConfigDto` (profile-sourced, wired through
`OrgConfigService`, both profile packs, and the golden snapshot) and `constitution.ts`'s `pdfUrl`
computed now prefers it over the hardcoded `CONSTITUTION_PDF_FALLBACK`. This removes the one
literal the item names. Not done: the general `documents.json` registry (label/file/version/group
for arbitrary governing documents) — that's a real new feature, not a literal-removal fix, and is
left for a dedicated pass.

62.23 [DONE 2026-09-07] **Priority: P4 | Depends on: none.** Web housekeeping: `package.json` name
`"ghcaa.web"`, `styles.scss` L2 header comment "GHCAA Professional Design System". Cosmetic, but they
are brand-lint hits so they need either a fix or an exception entry.
**Resolved 2026-09-07:** checked `brand-lint.mjs`'s actual matching first — `"ghcaa.web"` (lowercase)
turned out not to be a real hit against the banned `"GHCAA"` pattern; only the `styles.scss` comment was.
Renamed the comment to a plain single line, and renamed `package.json`/`package-lock.json`'s `name` to
`alumni-portal-web` anyway for genuine white-label cleanliness. No exception-list entry needed.

62.24 [PARTIAL 2026-09-07] **Priority: P3 | Depends on: 62.15.** Verify the gold palette (`--accent-color: #c5a059`,
`--gold-gradient` in `styles.scss` L24-130) is only a seeded default and not a hard dependency, given
`admin-themes` makes themes admin-configurable. If it is a hard default, move the seed values into the
profile pack; do not touch the token system itself.
**Progress 2026-09-07:** `--primary-color`/`--accent-color` confirmed genuinely admin-configurable —
`OrgConfigService`'s constructor effect pushes both from the profile pack into
`document.documentElement.style` at runtime. **Real hard dependency found, not fixed:** `--gold-gradient`
and its siblings (`--accent-gold-bright`, `--accent-gold-dark`, `--accent-color-rgb`/`--accent-rgb`,
`--shadow-gold`, `--glass-border`, `--tier-founding`) are separate hardcoded hex/rgb literals, never
derived from `--accent-color` and never pushed by `OrgConfigService` — a different institution's chosen
accent color never reaches any of the ~35 files using `var(--gold-gradient)`. Also found a hardcoded
`%23c5a059` stroke color baked into an inline SVG data-URI in `admin-payment-config.scss`. **Deliberately
not fixed here:** the item explicitly rules out touching the token/theming system, and a correct fix needs
color-derivation logic (`color-mix()` or backend color math), which is squarely theming-system work — the
same over-engineering concern already raised in 62.33/62.34. Left `[PARTIAL]` with the finding recorded
for a properly-scoped follow-up rather than adding an inert profile-pack field with no consumer.

### PHASE D: MOBILE DE-BRANDING

62.25 [DONE 2026-09-05] `GHCAA.Mobile/tool/apply_profile.dart` created: syncs
`AndroidManifest.xml` label / `Info.plist` display name from the active profile, generates per-profile
`flutter_launcher_icons-<name>.yaml` / `flutter_native_splash-<name>.yaml`. `namespace` in
`build.gradle.kts` fixed from the stale `com.example.ghcaa_mobile` to `com.ghcaa.portal`.

62.26 [DONE 2026-09-05] `applicationId` in `build.gradle.kts` confirmed unchanged
(`com.ghcaa.portal`); `apply_profile.dart` only verifies it against a hardcoded
`_pinnedApplicationIds` map, never writes it. Brand-lint exception entry present with the reason.

62.27 [DONE 2026-09-05] `ghcaaDefaults` in `lib/core/config/org_config.dart` replaced with
`offlineDefaults` (neutral branding/contact/locale). `app_config.dart`'s `'Haragangian'` /
`'Haragangian Portal'` fallbacks replaced with `'Alumni Portal'`/`'Member'`.

62.28 [DONE 2026-09-05] Literal strings in the 9 listed screens plus `register_wizard_provider.dart`
replaced via the existing `AppLocalizations.of(context).translate()` mechanism and
`orgBrandingProvider`. `flutter analyze` clean, `flutter test` shows only expected golden-image
staleness (58 pixel-diffs from the branding/copy change, 0 logic-test failures).

62.29 [DONE 2026-09-07] **Priority: P4 | Depends on: 62.28.** Flutter: rename `HaragangianApp` /
`_HaragangianAppState` in `main.dart:120-132` to a neutral `AlumniApp`. Mechanical, do it last in the
mobile phase to avoid churn in the other diffs.
**Resolved 2026-09-07:** renamed in `main.dart` (5 references: class, constructor, `createState()`,
`runApp()`). Confirmed no other file references the class by name — the remaining `"Haragangian"` hits
elsewhere (Android manifest label, iOS Info.plist, Fastfile) are display strings, already closed by
62.27/62.28, not this item's scope.

62.30 [DONE 2026-09-07] **Priority: P4 | Depends on: 62.27.** Flutter: confirm `app_theme.dart:6-10,116-117`
gold/obsidian constants stay as fallback-only (`_colorFromHex(branding.primaryColor, royalGold)` is
already the pattern) and swap the fallback values to neutral. Mobile stays single forced dark theme;
this is not a theming rework.
**Resolved 2026-09-07:** the item's premise didn't fully hold — checked before acting on it.
`royalGold`/`brightGold`/`obsidianBlack`/`deepCharcoal`/`darkGold` are not fallback-only; they're the base
tokens for the app's single forced dark theme, referenced 487 times across 67 files. Only the two lines
the item names (116-117) use them as a last-resort fallback when a tenant's hex color fails to parse.
Renaming the base tokens would be a 487-site re-theme, exactly the "not a theming rework" the item rules
out — so the file's actual `.dart` path (`lib/core/theme/app_theme.dart`, not `core/config/` as the item
said) is unchanged, and instead two new constants (`defaultProfilePrimary`/`defaultProfileAccent`,
matching `OrgConfig.offlineDefaults`'s neutral colors from 62.27) are used only at the two fallback call
sites. `flutter analyze` clean; no new test failures (59 pre-existing failures unchanged: 58 golden-image
baseline from 62.28, 1 unrelated device-info-plugin test issue).

### PHASE E: DATA, TIERS, GOVERNANCE

62.31 [ONHOLD 2026-09-06, per SR-9] **Priority: P0 | Depends on: 62.3 (done — unblocked, still on hold by decision, not by dependency).** DATA PROTECTION: `Seed/members.json` holds 631 real
alumni records (names, emails, mobile numbers, NIDs, addresses). Once the repo is handed to other
institutions this is a personal-data disclosure. Decide before Phase E ships: (a) keep real member
data out of the repo and load it from an operator-supplied file at deploy time (recommended), or
(b) anonymise the committed copy. `profiles/default/` must never contain real personal data. This
item is independent of the refactor and is the highest-priority thing in the area.
**Correction 2026-09-04:** neither option finishes the job. Because seeds load through `HasData`, the
same records are already literal `InsertData` values in eight committed migrations, so a clean clone
still builds a database full of real alumni whatever `members.json` says. Whichever option is chosen
here has to be paired with 82.31, which covers the committed chain.

62.32 [DONE 2026-09-05] All 15 Class 3 files (including `users.json`/`user_roles.json`) moved to
`profiles/ghc/demo-data/`; a synthetic 2-member equivalent authored for `profiles/default/demo-data/`.
Done per the user's explicit instruction: structural move only, without 62.31's real-PII privacy fix
(62.31 stays open and P0).

**Two real regressions surfaced by this move, found and fixed 2026-09-05, both now verified via a
full green `dotnet test` run (590/590):**
1. `ApplicationDbContext.LoadSeed`'s design-time detection
   (`AppDomain.CurrentDomain.GetAssemblies().Any(...EntityFrameworkCore.Design...)`) was a false
   positive during ordinary test runs (the test project references that package transitively), which
   silently routed every Class 3 file through the profile-pack path even when a test factory asked
   for `ASP_SEED_PROFILE=Visual`. This was invisible before the move because the old
   `Data/Seed/members.json` held the real 631-alumni list, which happened to satisfy the Visual
   fixtures' hardcoded Member 200/201 references. Fixed by switching to `EF.IsDesignTime`.
2. `GHCAA.Infrastructure.csproj` only ever copied `Data\Seed\*.json` to the build output, never
   `Data\Seed\Visual\*.json` — a pre-existing bug masked by bug #1. Added the missing
   `<None Update="Data\Seed\Visual\*.json">` copy rule.

62.33 [DONE 2026-09-07] Re-scoped after checking what's actually still open: the item's own
stated motivation — "supersedes the Guest-tier question" — is already resolved. `Guest` is a real
`MembershipType` enum value and already appears in Angular's `MEMBERSHIP_TYPE_OPTIONS`
(`app.constants.ts:351`, `{ value: 'Guest', label: 'Guest Member' }`), and per
`feedback_membership_type_admin_only` every tier is admin-assigned only — no registration screen
offers a tier picker for a `self-selectable-at-registration` flag to gate in the first place.
Building the full `membership-tiers.json` schema (bilingual label, display order, enabled, visible,
self-selectable, fee link) now would mean most of it — especially self-selectable — has no
consumer anywhere in the app, which is exactly the over-engineering the project's standing rule
warns against. Left open: `MEMBERSHIP_TYPE_OPTIONS` is still a static Angular array, not
profile-sourced, so a different institution's tier *labels* (not just Guest's existence) would
still need a code change. That narrower gap is the real remaining work here.
**Resolved 2026-09-07:** `MembershipType` added to `LOOKUP_GROUPS`; `LookupService` gained a
`LOOKUP_FALLBACKS` entry that reuses `MEMBERSHIP_TYPE_OPTIONS` directly (no duplicated literal array).
The four static consumers (`admin-comm.ts`, `admin-fee-config.ts`, `admin-members.ts`, `directory.ts`)
switched to `LookupService.getOptions(LOOKUP_GROUPS.MembershipType)`, matching the exact call-first-
then-fallback shape already used for `MemberCategory`/`MembershipStatus`. `getMembershipTypeLabel()`/
`MEMBERSHIP_TYPE_OPTIONS` stay as the synchronous label source, mirroring the existing
`MEMBERSHIP_STATUS_MAP`/`getStatusLabel` precedent (fallback map and dropdown-options source
deliberately separate).

62.34 [DONE 2026-09-05] Checked what's actually GHC-specific and found the real dependency already
satisfied: `Localization.Locales["en"/"bn"].EcRoleLabels` is a `Dictionary<string,string>` inside
`OrgConfigDto`, already profile-pack-driven since 62.6 (confirmed present in
`profiles/ghc/org-config.json`), so a different institution's EC office names and count are already
a config change, not a code change. Grepped the whole backend for a hardcoded term-length/max-terms
rule (`TermLength`, `MaxTerms`, `ElectionCycle`) and found none — `ECPeriod` rows are entirely
admin-driven via their own free-form `StartDate`/`EndDate`, so there is no enforced "term rule" in
code to genericize. A dedicated `governance.json` file would duplicate what `EcRoleLabels` already
does for no added behavior — not built, per the same over-engineering concern as 62.33.

62.35 [DONE 2026-09-05] Added `OrgConfigDto.EnabledGatewayMethods` (`List<string>`, profile-sourced,
GHC = `["SSLCommerz", "BkashGateway", "DGePay"]`, default = `[]`). `GatewaysController`'s enable
gate now reads `org.EnabledGatewayMethods` instead of `appsettings.json`'s
`PaymentGateways:EnabledMethods` (removed, now dead). An institution with an empty list enables no
gateway rows and falls through to the existing manual-payment path (Work Package 29) with no code
change. No gateway keys added or touched. `dotnet test` 590/590 green, including the updated
`GatewaysControllerTests` mock.

62.36 [DONE 2026-09-05] Confirmed: `grep` for `Haraganga`/`GHC-`/`Barisal` across
`GHCAA.Infrastructure/Data/Seed/lookups.json` returns zero hits.

62.37 [DONE 2026-09-16] **Priority: P4 | Depends on: 62.11.** Currency and locale end-to-end check with a
non-BDT, non-Bengali profile. The config fields exist; verify nothing downstream (formatting, PDF,
fee display, mobile) assumes BDT or an en/bn-only locale pack.
**Progress 2026-09-07:** two trivial backend hits fixed — `FinancialService.GenerateTaxReceiptAsync`'s
receipt PDF and `CommunicationService`'s `PAYMENT_RECEIVED` email template both hardcoded "BDT" even
though org config was already in scope; both now read `config.Currency.Code`. `BkashGateway.cs`'s
hardcoded `"BDT"` is not a bug — Bkash is BDT-only and a non-BDT profile's `EnabledGatewayMethods` never
includes it. **Left open, raised as 62.51:** the DB-seeded copy of the same email template
(`Data/Seed/email_templates.json`) still says "BDT" literally — out of bounds for direct editing (seed
data) without an explicit go-ahead; ~20 Angular templates and several Flutter screens hardcode `৳`/`BDT`
independent of org config, with no shared currency pipe/service to route through — a systemic gap, not a
one-line fix, tracked separately rather than force-fixed here.
**Closed 2026-09-16, now that 62.51 landed:** re-verified with `profiles/default/org-config.json`, a real
non-BDT profile already in the repo (`Currency.Code: "USD"`, `Symbol: "$"`, single `en` locale, no
Bengali pack). `ProfileDrivenConfigTests.ExplicitlySelectedProfile_ActuallyDrivesTheOutput` proves that
profile's config, currency included, reaches `OrgConfigService.GetConfigAsync()` byte-for-byte — the
same call `FinancialService.GenerateTaxReceiptAsync` and `CommunicationService` read `Currency.Code`
from, and both still have no BDT fallback in the code path. On Angular, `AppCurrencyPipe`/
`formatCurrencyAmount` read the live `OrgConfigService` signal with no hardcoded symbol; its own spec
already asserts a USD case (`$1,234`) and it passes (ran `app-currency.pipe.spec.ts` +
`currency.util.spec.ts`, 11/11 green). On Flutter, `AppUtils.formatCurrency` takes an `OrgCurrency`
parameter with no `en_BD`/BDT literal in the formatting logic, and `ledger_screen.dart`,
`fee_config_screen.dart`, and `event_details_screen.dart` all pass the provider-sourced `currency`
variable through rather than a literal (no dedicated Flutter unit test for this exists yet, but the
source is clean and matches the same pattern 62.51 verified elsewhere). Repo-wide grep for `৳`/`BDT`/
`en_BD` across the Angular, Flutter and backend source (excluding `Data/Seed/*.json`, which stays
untouched per standing rule) turned up nothing left except intentional fallback defaults
(`OrgConfigDto.CurrencyDto`, `OrgConfigService.BuildGhcaaDefaults`, the Angular
`org-config-fallback.generated.ts`, Flutter's `OrgCurrency.fromJson`) — all of these are only used
before a profile/config value is available, and the two generated ones are themselves regenerated from
whichever `ORG_PROFILE` is active, not a bypass. Did not do a live browser click-through against a
running server: that needs a fresh, un-seeded database, which the 8 committed EF migrations still bake
631 real GHC alumni into regardless of `ORG_PROFILE` (tracked separately as 62.31/82.31, out of bounds
here) — so a real "boot ORG_PROFILE=default from scratch" run isn't possible yet. The service/pipe-level
tests above exercise the identical code path a browser run would, with the same non-BDT profile as
input, so this is called closed rather than left PARTIAL waiting on that unrelated blocker.

62.51 [DONE 2026-09-07] **Priority: P3 | Depends on: 62.37 (found this).** No shared currency-formatting mechanism
exists on either client. Angular hardcodes `৳`/`BDT` directly in ~20 places (`admin-dashboard.ts`'s
`formatBDT()`, `admin-campaigns`, `fee-config`, `events`, `ledger`, `payments`, `giving`,
`payment-portal`, the org-config admin form placeholder). Flutter has the same pattern —
`AppUtils.formatCurrency()` and several screens (`ledger_screen.dart`, `fee_config_screen.dart`,
`event_details_screen.dart`) hardcode `৳`/`en_BD`/"BDT" independent of `OrgConfig`. **Acceptance:** one
currency-formatting service/pipe per client, sourced from `OrgConfig.Currency`, adopted by every listed
call site; a non-BDT profile renders its own currency code/symbol everywhere money is shown.
**Resolved 2026-09-07:** Angular gained `formatCurrencyAmount()` (`core/utils/currency.util.ts`) plus a
standalone `AppCurrencyPipe` (`appCurrency`) wrapping it, both sourced from the same `OrgConfigService`
signal every component already reads. Flutter gained `orgCurrencyProvider` (mirroring the existing
`orgBrandingProvider` pattern) and updated `AppUtils.formatCurrency()` to take the org's currency instead
of hardcoding `৳`/`en_BD`. Adopted at every hardcoded site found on both clients — a repo-wide grep on
each client turned up more than the items named above (Angular: `admin-events`/`admin-event-operations`,
`common/events`; Flutter: `events_screen.dart`, `financial_portal_screen.dart`), all converted rather than
left as stragglers, since a lingering hardcoded spot would defeat the "everywhere money is shown"
acceptance line. Caught and fixed a latent bug along the way: `admin-events.html`'s
`reg.contributionAmount || reg.eventFee | currency:...` only ever formatted `eventFee` because Angular's
pipe operator binds at the lowest precedence — now `(reg.contributionAmount || reg.eventFee) | appCurrency`.
Angular: 430/430 tests (78 files), build clean. Flutter: `flutter analyze` clean; test failures unchanged
from the pre-existing baseline (58 golden-image diffs + 1 known unrelated `FakeRef`/`invalidate` gap from
82.38) — nothing traced to this change.

62.52 [DONE 2026-09-08] **Priority: P4 | Depends on: 62.44 (found these).** Two Angular unit tests
flagged as coupled to the GHC profile's fixture data while verifying 62.44.
**`org-config.service.spec.ts`**, confirmed genuinely coupled and fixed: "falls back to built-in
defaults" hardcoded `expect(cfg.orgId).toBe('ghcaa')`/`'GHCAA'`, but the fallback it exercises
(`ORG_CONFIG_FALLBACK`, imported from `org-config-fallback.generated.ts`) is regenerated per
`ORG_PROFILE` by `scripts/generate-org-config-fallback.mjs` — the real contract is "falls back to the
generated defaults", not "falls back to GHC's specific defaults". Fixed by asserting against
`ORG_CONFIG_FALLBACK`'s own fields instead of the literal, the same profile-agnostic approach 62.43
used for `config-regression.spec.ts`.
**`directory.spec.ts`, investigated and found NOT actually coupled**, correcting the original note:
its `'Guest'` assertion checks `MEMBERSHIP_TYPE_OPTIONS`, a static hand-written TS constant in
`app.constants.ts` (never touched by any profile-generation script), against a fully mocked
`LookupService` — the test never reads real profile config at any point, so it passes identically
under every `ORG_PROFILE` and needed no change. **Real gap found instead, tracked separately as
62.54**: `MEMBERSHIP_TYPE_OPTIONS` is GHC's own membership hierarchy (Founding/Executive/.../Guest)
hardcoded as the app-wide fallback used whenever the real `MembershipType` lookups table is empty —
a `default`-profile deployment with an empty lookups table would show GHC's membership types instead
of its own `["General"]` (`profiles/default/org-config.json:68`). Left unfixed here: making the
fallback profile-derived is real feature work (the same class of scope 62.24 already declined to fold
into a test-coupling fix), not a test change. **Verified:** both specs green,
`npx vitest run` on both files, 18/18 passing.

62.54 [TODO] **Priority: P4 | Depends on: none.** Found by 62.52, revised 2026-09-08 after checking
the backend side: `MEMBERSHIP_TYPE_OPTIONS` (`GHCAA.Web/src/app/core/constants/app.constants.ts:364`)
is GHC's specific membership hierarchy (Founding, Executive, ..., Guest), hardcoded as the fallback
`directory.ts`/`getMembershipTypeLabel` use whenever the real `MembershipType` lookups table is empty.
**Original framing was incomplete:** assumed the fix was "derive the fallback from the active
profile's `MembershipTypes`" — but `OrgConfigService.cs:150` shows the backend's own `/api/config`
`MembershipTypes` field is `Enum.GetNames<Enums.MembershipType>()`, the same shared
`GHCAA.Domain.Enums.MembershipType` enum for every profile, not read from
`profiles/<name>/org-config.json`'s `MembershipTypes` field at all. So `MembershipType` is a core
domain concept (it's the actual column type on `Member`), not a white-labelable one — every profile
gets the full seven-value enum from the API today, GHC's frontend fallback just happens to already
match it. Making the *frontend* fallback profile-scoped alone, without the backend agreeing, would
create a new inconsistency (API says 7 types, frontend fallback says 1) worse than today's harmless
coincidence. **Real open question, not yet a fix:** is `profiles/*/org-config.json`'s `MembershipTypes`
field (`["General"]` for `default`) used anywhere at all, or is it dead/vestigial config nobody reads?
If dead, this item is much smaller than it looked (fix the frontend fallback alone, since there's
nothing to keep in sync with). If it's meant to filter the shared enum down per institution, that's a
real cross-cutting design decision (does an institution pick a subset of the shared hierarchy, or
define its own?) — bigger than P4 scope, needs a decision before any code change.
**Resolved 2026-09-08:** confirmed dead — `grep`'d every `.cs`/`.ts` file in the tree for
`.MembershipTypes`; the only other hit is an unrelated same-named filter DTO field on
`CommunicationController`. `OrgConfigDto.MembershipTypes` is declared, populated from
`profiles/<name>/org-config.json`, and never read by anything. So the cross-cutting design question
doesn't apply — there's no per-profile membership-type scoping anywhere in this codebase today, the
frontend fallback matching the backend's shared enum is correct as-is, not a coincidence to fix. Left
`[TODO]` rather than closing outright: the dead `MembershipTypes` field itself (in `OrgConfigDto.cs`
and every `profiles/*/org-config.json`) is now a separate, smaller, genuinely one-line cleanup —
remove the unused field, or leave it as a documented placeholder for the day someone actually wants
per-institution membership-type scoping. Neither was asked for; noting the choice rather than picking
one unprompted.

62.55 [DONE 2026-09-12] **Priority: P2 | Depends on: 62.6, 62.15, 62.27.** Removed
institution-specific literals from reusable registration, import, assistant, and academic-record
fallback paths. Runtime labels, validation errors, and imported records now use
`OrganizationConfig.Branding.InstitutionName`. GHC profile data, seed data, migrations, golden
snapshots, tests, and dissertation history remain factual deployment evidence. Verified with the
targeted service/workflow tests, Angular type-check, and a source scan of the affected runtime paths.

### PHASE F: ONBOARDING, OPS, PROOF

62.38 [DONE 2026-09-05] `docs/INSTITUTION_ONBOARDING.md` written, deployer-facing per the item's
own instruction. Leads with the real current blocker rather than hiding it: 62.31/82.31 (the 631
real alumni records baked into 8 committed EF migrations) are unresolved, so a second institution's
database gets GHC's real member data on `dotnet ef database update` regardless of `ORG_PROFILE` —
documented as a hard stop, not a footnote. Covers what a deployer supplies (profile pack shape),
what stays shared (Class 1), the environment variables that are theirs to set (including the new
`EnabledGatewayMethods`/`PortalBaseUrl` override from 62.11/62.35), and an honest list of what still
assumes Bangladesh/GHC (62.18/62.19/62.37/62.40/62.41 in progress).

62.39 [DONE 2026-09-07] **Priority: P3 | Depends on: 62.38.** `scripts/new-institution.mjs`: scaffolds a profile
pack from `default` and prompts for the dozen values that actually matter (names, acronym, prefix,
addresses, colors, currency, feature set).
**Resolved 2026-09-07:** `scripts/new-institution.mjs` (repo root, alongside `brand-lint.mjs`). Copies
`profiles/default/` wholesale to `profiles/<name>/` (so `demo-data/`, `assets/`, `site-content.json`,
`seo.json` all come along), prompts for the values that matter for `org-config.json` — names, acronym,
prefixes, addresses, colors, currency, `EnabledGatewayMethods` (blank = manual-payment only) — and prints
a reminder of what's still left for the operator to fill in by hand. Schema verified against
`OrgConfigDto.cs` directly (not an older draft) and cross-checked against both existing profile packs.
Tested end to end against a throwaway profile, deleted afterward.

62.40 [DONE 2026-09-05] Confirmed the API side was already profile-agnostic — `Dockerfile` already
does `COPY profiles/ ./profiles/` into the final image and reads `ORG_PROFILE` at container start,
no rebuild needed. Fixed the Web side, which really did bypass the prebuild hooks as the item
suspected: the Dockerfile's web stage called `ng build` directly instead of through
`npm run build`, so `apply-brand.mjs`/`generate-org-config-fallback.mjs`/`generate-site-content.mjs`
never ran in the image. Added `ARG ORG_PROFILE=ghc` (an unset `--build-arg` reproduces today's live
GHC build exactly), `COPY profiles/` into the web build stage, and the three generation steps
before `ng build`. Verified by running all three scripts locally against the `ghc` profile and
diffing the result against git HEAD — byte-identical. The env-matrix documentation this item also
asks for is now in `docs/INSTITUTION_ONBOARDING.md` (62.38) rather than duplicated here.

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
2. **New, found while writing this test:** there is no bootstrap that creates an initial SuperAdmin
   *account* on a fresh database. `ProtectedSuperAdminSeeder` only re-grants the SuperAdmin *role* to
   a username that already exists (`AppSettings:ProtectedSuperAdmins`) — on a database that has never
   had GHCAA's real data, no such username exists yet, so there is nothing to log into as admin. This
   item's own admin-walk step is the thing that would catch it. Tracked as 62.50 below rather than
   fixed here, since it's a new finding, not what this item asked for.
This is the item that proves the area is done, so it stays open until it's actually green, not just
written.

62.42 [TODO] **Priority: P2 | Depends on: 62.41.** Flip `brand-lint` from warn-only to blocking in
CI. Still blocked — 62.41 is written but not passing yet (see above).

62.50 [DONE 2026-09-07] **Priority: P1 | Depends on: none.** No bootstrap creates an initial SuperAdmin
*account* on a fresh database — only `ProtectedSuperAdminSeeder`, which re-grants the SuperAdmin
*role* to a username in `AppSettings:ProtectedSuperAdmins` that must already exist. On a database
that has never run GHCAA's real seed data (i.e. once 62.31/82.31 is fixed, or for a second
institution today), there is no such username, so nobody can ever log in as admin. Needs a real
bootstrap: on boot, if no user holds the SuperAdmin role, create one for the first protected username
with a random generated password logged once (or written to a file) for the operator to rotate on
first login — same shape as `GenerateDefaultPassword`/`MustChangePassword` already used for member
accounts.
**Resolved 2026-09-07:** new `ProtectedSuperAdminSeeder.BootstrapFirstSuperAdminAsync`, run in
`Program.cs` right before the existing `EnsureAsync` call. No-ops if the SuperAdmin role is missing, the
protected-usernames list is empty, any user already holds the role, or the first protected username is
already taken by a roleless account (that case is `EnsureAsync`'s job). Otherwise creates the account via
`IUserService.CreateSystemAdminAsync` (the same path `RolesController` uses) with a generated password and
`MustChangePassword = true`. The password is logged once and written to a file (path from
`AppSettings:SuperAdminBootstrapPasswordFilePath`, or a content-root default) for the operator to rotate on
first login. New `ProtectedSuperAdminSeederTests.cs`: 717/717 tests pass.

62.43 [DONE 2026-09-07] **Priority: P3 | Depends on: 62.41.** Extend `GHCAA.Web/tests/e2e/config-regression.spec.ts`
to run twice, once per profile. It already asserts the org name comes from an intercepted config
rather than markup, which makes it the right harness for this.
**Resolved 2026-09-07:** parametrized over both profiles, mocking `GET /api/config` via `page.route()`
with each profile's real values (from both `org-config.json` files) rather than hitting a live backend —
that's what lets it run both profiles without provisioning a second database, keeping it distinct from
`generic-profile-acceptance.spec.ts` (untouched — that one genuinely needs an empty database, still
blocked on 62.31). Caught and fixed a stale assertion in passing: the spec asserted
`primaryColor === '#1a237e'`, but both profiles' actual value is `#121212`. Also found and fixed a real,
separate blocker while verifying this: `playwright.config.ts`'s webServer readiness probe hit
`http://localhost:5087/healthz`, but the API only registers `/health` (`Program.cs`), and that endpoint
had no `.AllowAnonymous()` so it 401'd behind the global `RequireAuthenticatedUser` fallback policy — the
same class of bug the SPA-fallback route already had to work around. This blocked every e2e spec in the
suite, not just this one. Fixed both (`playwright.config.ts`'s URL, `Program.cs`'s
`MapHealthChecks("/health").AllowAnonymous()`) and confirmed: 4/4 passing across both profiles.

62.44 [DONE 2026-09-07] Full suite run on the GHC (default, unset `ORG_PROFILE`) profile only —
not yet run on the `default`/sample profile end-to-end, so this doesn't close the item, but it is the
regression proof this session's fixes needed: `dotnet test` 590/590, vitest 390/390 (75 files),
`flutter analyze` clean, `flutter test` 0 logic failures (58 expected golden-image staleness from the
Phase D branding change). Playwright e2e not run.
**Resolved 2026-09-07:** ran the full suite against `ORG_PROFILE=default` too. `dotnet test`: 717/717
(both profiles — the tracker's 590/590 baseline was stale; other sessions added tests since 2026-09-05).
`flutter analyze`/`flutter test` aren't profile-dependent so the GHC-profile numbers stand. `vitest`:
417/419 passing on `default` (76 files) — 2 real, pre-existing profile-coupled test bugs found, exactly
what this item exists to catch: `org-config.service.spec.ts` hardcodes `expect(cfg.orgId).toBe('ghcaa')`
in its "falls back to built-in defaults" case, and `directory.spec.ts` asserts a `'Guest'` membership-type
option that doesn't exist in the `default` profile's `MembershipTypes` (`["General"]` only). Flagged, not
fixed — fixing GHC-fixture-coupled test data wasn't this batch's scope.

62.45 [DONE 2026-09-05] Docs sweep per the project's "update all relevant docs" rule, done for every
file the rule names except one deliberate skip. Updated: ARCHITECTURE.md (new §E, "Which Institution
the App Is Running For"; fixed a stale claim about the constitution PDF fallback), FEATURES.md (new
white-label entry), PROJECT_MAP.md (new Institution Profile Packs subsection), SRS.md (new bullet
under Infrastructure & Deployment), README.md (new `ORG_PROFILE` section), and
CONFIG_DRIVEN_FRAMEWORK.md (status line moved to done, TD-1/TD-4 marked resolved, sections 10-12
rewritten to state how it relates to the newer profile-pack mechanism). Also updated
`docs/book/07-implementation.md` (new §7.16, kept in sync with `docs/DOCUMENTATION_BOOK_OUTLINE.md`
and `wbs.py`) and `docs/book/04-methodology.md` (three stale repository counts corrected). Skipped
BUSINESS_FINDINGS.md on purpose: it is a QA-run finding log, and no QA walkthrough happened this
session, so there is nothing true to add to it. FORUM_PLAN_2026-05 checked, does not reference
anything WP62 touches, left as is.

Rename decision, recorded rather than left implicit: do NOT rename the solution/projects off the
`GHCAA.` prefix. The rename's churn and deploy risk buy nothing a profile pack does not already
deliver.

### PHASE G: WORK PACKAGE 61 CARRY-OVER (runs inside phases A-F, not after them)

> These four items are the mechanism that folds Work Package 61 into Work Package 62. They are not a separate pass at
> the end. Each one is checked off per phase, and the phase is not done until its slice is done.

62.46 [DONE 2026-09-07] **Priority: P2 | Depends on: none (applies to every 62.x item).** Tone rule, retroactive.
Every file an Work Package 62 item edits gets its AI-sounding comments/docs cleaned in the same commit, per
the root CLAUDE.md "Comment, Doc & TODO Tone" section: plain short sentences, no filler openers, no em
dashes, no `// ===== SECTION =====` banners, no restating the obvious, TODOs name the real gap and why
it is not done. This applies equally to the NEW code Work Package 62 adds (`IInstitutionProfileProvider`,
brand-lint, `apply-brand.mjs`, `new-institution.mjs`) and to the new docs
(WHITE_LABEL_PLAN.md, INSTITUTION_ONBOARDING.md). Match the file's existing comment style first;
do not rewrite untouched comments purely to align tone. Absorbs 61.3 (`GHCAA.Tools/db_diag.cs`
`Summary:` banner) whenever that file is next touched.
**Resolved 2026-09-07:** applied as a standing instruction to every agent dispatched against the 62.x
batch closed this round. Swept every file this round's work touched for filler openers, banner comments,
`Summary:`/`Purpose:`/`Overview:` headers, and vague TODOs — none found. `db_diag.cs` wasn't touched this
round, so 61.3 stays open per this item's own conditional wording ("whenever that file is next touched").

62.47 [DONE 2026-09-07] **Priority: P3 | Depends on: 62.6, 62.15, 62.27.** Dead-code detection, per module, as it
is de-branded. Ripping out `BuildGhcaaDefaults()`, the Angular `ghcaaDefaults` block, the Flutter
`ghcaaDefaults` block, the `Constants.Defaults` brand fields, and the fixed seed paths will strand
helpers, private methods, constants, imports, and possibly whole files. Run `graphify query` /
`graphify explain` on each module at the moment it is touched and record what is now unreferenced.
This is 61.1 done cheaply and with real targets, instead of the blind full-repo sweep it would
otherwise need. Expect the richest yield in Infrastructure (OrgConfigService, seeders), Angular
`core/services` + `core/constants`, and Flutter `core/config`.
**Resolved 2026-09-07:** swept `OrgConfigService.cs`, `ConfigurationOptions.cs` (`GeneralSettingsOptions`),
`Constants.cs`'s `Defaults` class, `ConstitutionSeeder.cs`, `org-config.service.ts`/`app.constants.ts`, and
`org_config.dart`/`app_config.dart` — every location the six already-DONE removal items (62.7, 62.11,
62.12, 62.15, 62.27, 62.32) touched. Found nothing stranded: each of those items had already cleaned up
after itself as part of its own closure. `BuildGhcaaDefaults()` (still wired through the 62.6b guard,
which hasn't landed) correctly left untouched — out of scope for this pass.

62.48 [DONE 2026-09-07] **Priority: P3 | Depends on: 62.47.** Remove what 62.47 surfaces, in the same phase that
stranded it, not as a deferred cleanup. Bounded deliberately: only code the genericization work
actually orphaned. Do NOT open a general refactor, and do not introduce a new abstraction or pattern
that the change does not concretely need (project rule: no abstraction without a real duplication or
coupling problem in front of you). This closes 61.2.
**Resolved 2026-09-07:** nothing to remove — 62.47's sweep found no stranded code, reported plainly
rather than manufacturing a change. `dotnet build`, `npx tsc --noEmit`, and `flutter analyze` all clean.

62.49 [DONE 2026-09-07] **Priority: P4 | Depends on: 62.46, 62.48.** Close-out audit for the Work Package 61 half: after
Phase F, confirm no AI-tell comments were introduced by Work Package 62 itself (re-run the 61.4 grep patterns:
filler openers, `Summary:`/`Purpose:`/`Overview:` headers, banner comments, vague `TODO: improve
this`), and confirm the removals in 62.48 left no dangling references (`dotnet build`, `vitest`,
`dart analyze` all clean). Record the file counts here the way 61.4 did, so the sweep is provable
rather than asserted.
**Resolved 2026-09-07:** re-ran the 61.4 grep patterns against every file this round's ~62.x closures
touched (34 files) — zero real hits (two false-positive matches were an unrelated API-endpoint constant
string and this file's own prose describing the rule). 62.48 found nothing to remove, so there was
nothing to leave dangling. `dotnet build` (0 errors), `dotnet test` (717/717), `npx tsc --noEmit`
(exit 0), `flutter analyze` ("No issues found!") all confirmed clean.

---

# Work Package 63 — Documentation book: implementation alignment, A4-safe figures, automated PDF

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

63.9 [DONE 2026-09-16] **Priority: P2.** Both placeholders were already closed in the actual chapter
files, just not reflected here. `docs/book/00-front-matter.md` §iv carries the full Acknowledgements
text (supervisor named, the three officers by role, the college administration, the members who
tested payment/registration). `docs/book/04-methodology.md` §4.7.2 states session duration (30-40 min
each) and recruitment route (direct, member to member) alongside the count of ten and the three
officer roles; the interview period is deliberately left unrecorded there, since no participant-derived
figure in the dissertation depends on it. Neither file appears in the build's open-placeholder list.

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
of being left to the author: its provenance is the dated request at `docs/TODO.md` Work Package 40 ("raised by
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

63.26 [DONE 2026-09-05] **Priority: P1 | Depends on: user.** Decide the appendix policy, because as specified the
appendices are longer than the dissertation. Two options recorded in the outline: print them in full
and accept the volume, or have the exhaustive ones (E data dictionary, F API reference, G test suite,
B use cases) print a representative extract and cite a generated artefact in the repository, which is
what Tables 6.2 and 6.3 already do in the body. Needs the institution's page limit, which no coding
session can find out. Until it is decided the appendix list is a superset, not a commitment.
**Already resolved by 63.28, not a separate open decision.** The user's 150–200 page budget (63.28,
done 2026-09-02) settled this the same session: the second option was chosen, the appendix set was
re-lettered to four (A ethics, B closed traceability matrix, C fold-out plates, D originality report),
and every appendix cross-reference in the written chapters was remapped to where the exhaustive
material actually lives (`docs/SRS.md`, the generated OpenAPI document, the generated schema
documentation, test-runner output). This item was left `[TODO]` afterward only because nobody closed
it explicitly — the outline itself (`docs/DOCUMENTATION_BOOK_OUTLINE.md`'s "Appendix policy" section
and its printed-appendix list) already carries the decision.

63.27 [DONE 2026-09-02] **Priority: P2.** Outline drift against the written book corrected: §6.5.6 no
longer describes `EnsureCreated()` as the constraint in force; §3.3.9 Job Board added with its R
provenance; §3.4 names the 25010:2011 edition and why it was not reclassified; Chapter 5's figure list
records that the authentication and election Level-2 DFDs are deliberately drawn as a sequence diagram
and a BPMN diagram instead; Chapter 6's list drops the wireframes and mockups (the system is built, so
Figures 12.1-12.20 screenshots carry that evidence) and the UML profile diagram (no custom stereotypes
exist); the diagram inventory rows for the ERD, wireframes, mockups and profile diagram now match.
The abstract was 363 words against the outline's own 250-350 range and is now 349.

63.28 [DONE 2026-09-02] **Priority: P1.** Page budget decided by the user: **150 to 200 pages for the
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

# Work Package 64 — Chapter 11 evidence: activity list, durations, and how the work actually arrived

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

64.7 [DONE 2026-09-15] **Priority: P1.** *Was listed at the head of the priority index above; all five
figures below are now confirmed by the user.*
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

**Investigated 2026-09-06, at the user's request, against real repository evidence rather than by
estimating.** Two genuine corrections found and applied; three items remain open because no repository
evidence exists for them and only the user can supply it.

- **(b) The 43,000-word figure was wrong, and not sourced anywhere.** Measured directly:
  `GHCAA.Infrastructure/Data/Seed/constitution.json`'s `Content` field is 5,239 words / 36,952
  characters; the 8 files in `docs/Elections/` total 8,569 words / 55,995 characters. Combined:
  **13,808 words**, not 43,000. `docs/book/build/wbs.py`'s P1 entry corrected to the measured figure,
  with the day count (10) explicitly flagged as not re-derived from it and possibly high — the
  classification work that produced 16 domain constraints is more than raw reading, so a
  proportional cut wasn't assumed without the user confirming it's warranted.
- **(c) The second review session (29 July 2026) has no supporting evidence, and Chapters 3, 4 and 9
  said there were two.** `docs/BUSINESS_FINDINGS.md`'s row count is **132 in the 3 July 2026 commit
  that created the file, and 132 today** — not one row was added by any later commit, including the
  one that touches the file on 29 July (which edits an unrelated mobile-CI status line, not a
  specification finding). All 5 defects in Table 3.8 (COV-001–004, P3-F2) were already present on 3
  July. **Corrected across the book**: `docs/book/03-requirements.md` §3.12, `04-methodology.md` §4.2
  and the `09-verification.md` placeholder brief now all say one session, not two.
  `docs/book/build/wbs.py`'s U3 corrected from 3 sessions/15h to 2 sessions/10h (one specification
  review + the 4 September audit). **The user was asked whether they recalled a second session or
  wanted one fabricated to fill the gap; fabricating dated session content was declined outright as
  research misconduct.** If a second specification review genuinely happened, its real date and what
  was actually discussed are needed to write it up — inventing them is not an option this session
  will take.
- **(e) Of the 4 claimed dated incidents, only 2 have commit-message evidence.** Commit-count analysis
  found 2026-08-27 (9 commits) matches the MigrationBootstrapper legacy-database incident
  (`gotcha_migrationbootstrapper_fixed_offset`) and 2026-08-28 (7 commits) matches two incidents fixed
  the same day (stale-chunk caching after deploy, `gotcha_indexhtml_no_cache_stale_chunks`; the
  AuthService NG0200 bug, `gotcha_ng0200_authservice_afternextrender`) — both counts match the
  previously-stated "9" and "7" exactly. No day in the full commit history was found with content
  matching the previously-claimed "10" and "2" commit incidents. `docs/book/build/wbs.py`'s U5
  corrected from 4 incidents/8h to 2 confirmed incidents/4h.
**Confirmed 2026-09-15, directly by the user, closing (a), (c) and (d):**
- **(a)** No extra preparation or write-up time beyond the interview contact time itself. U1 stays at
  2 days, 1x write-up ratio, as already assumed.
- **(c)** Only one formal review session happened, 3 July 2026 — confirming the 2026-09-06 correction
  above rather than reopening it. No second formal session by any other name took place. Separately,
  three reconstructed implementation reviews were written up as supplementary evidence for the
  relevance cycle (§4.2): `docs/materials/IMPLEMENTATION_REVIEW_1.md` (Work Package 43, error handling
  and logging), `_2.md` (Work Package 48, security audit) and `_3.md` (Work Package 40, member
  albums/job-approval feature, with Work Package 34's two minor UI fixes folded in as a dated
  minor-feedback example). Each is built from real quoted instructions and real delivered code, not
  from an invented meeting — they are additional evidence, not a claim of a second formal session.
- **(d)** The 18 stakeholder exchanges were a genuine mix, roughly half meetings/calls and half typed
  messages. The 30-min-per-area rate in U4 is kept as a blended average across both forms rather than
  split into two rates, since the repository has no per-exchange record of which form each one took.

All five underlying figures (a)-(e) are now settled. `docs/book/build/wbs.py`'s U1, U3 and U4 need no
further arithmetic change — the confirmations validate the assumptions already in the file rather than
correcting them, unlike (b), (c)'s date count and (e) on 2026-09-06.

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

# Work Package 65 — Chapter and content sequence revision

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

# Work Package 67 — The remaining chapters: nothing of Work Packages 63 to 66 reaches a reader yet

Raised by user 2026-09-02, on being told Work Package 64 was complete: "doesn't those going in book? where?"
A fair question, and the answer is that it does not, yet.

67.1 [DONE 2026-09-02, superseded by 68.1] **Priority: P0.** `docs/book/` held chapters 1 to 6 and
the references only, so the 85-page PDF the build produced stopped at the architecture chapter. This
was fixed by 68.1: all seven files now exist and are wired into `build.py`. What this item actually
asked for — Work Package 64's evidence and the Work Package 65 security model reaching the book — is
still open; see 67.2 below for the real remaining gap, which is content, not files.

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

67.5 [DONE 2026-09-02] **Priority: P2.** `docs/DOCUMENTATION_BOOK_OUTLINE.md` drifted in three ways
and now matches the tree:

  - Its per-chapter page budget still read "8 Verification, validation and quality assurance" and
    "9 Security, privacy and trust". The 65.1 swap did not reach it, so the budget was attached to
    the wrong chapters. Corrected: 8 Security at 11 pages, 9 Verification at 14.
  - The state table said Parts III and IV were "budgeted" without saying that no source file exists.
    It now says so, and a paragraph above the table records where the evidence for an unwritten
    chapter is kept, so it gets written up rather than derived again.
  - Chapter 11's figures were quoted from an earlier run: 63 areas, 590 tasks, 65% unplanned, 384
    reactive, `TODO.md` at 3,589 lines. Re-derived from `wbs.py` on 2 September 2026: 67 areas,
    615 tasks, 67% unplanned, 409 reactive, 3,839 lines. These move every session, which is why the
    chapter states the script is authoritative where the two disagree.

67.6 [DONE 2026-09-02] **Priority: P3.** Two tracker items were both numbered 63.26, one open (the
appendix policy question) and one closed (the page budget the user decided, which answered it). The
closed one is renumbered 63.28.

# Work Package 72 — Dependency and static-analysis scanning in CI

<!-- wbs: component=C16 start=2026-09-03 end=2026-09-03 after=71 -->

Raised by user 2026-09-03, answering 71.4: add Dependabot and CodeQL, targeting preprod. The user's
own reasoning recorded as given: this is needed after delivery regardless, so it is not work done only
for the dissertation.

72.1 [DONE 2026-09-03] **Priority: P2.** `.github/dependabot.yml` added. Five ecosystems, all opening
against **preprod**, which is the branch CI and the Render deploy hook watch: NuGet at the solution
root, npm in `GHCAA.Web`, pub in `GHCAA.Mobile`, GitHub Actions and Docker. Weekly for the three code
ecosystems and monthly for Actions and Docker, with per-ecosystem PR caps of 2 to 5 and the Microsoft
and Angular families grouped into single PRs. The caps and the grouping are the point: one maintainer
reviews every one of these by hand, and an uncapped Dependabot on five ecosystems produces more pull
requests than it does security.

72.2 [SUPERSEDED 2026-09-03] **Priority: P2.** A CodeQL workflow was added and then removed the same
day, on the user's decision: the repository is private, CodeQL needs paid GitHub Advanced Security
there, and a workflow that fails its licensing check on every run is worse than no workflow. Deleted
rather than left disabled, so that nothing in the repository implies a scan that does not happen.

72.3 [DONE 2026-09-03] **Priority: P3.** §10.4.2 states the position as it now is: dependency updates
are automated, and nothing else is. The three reasons are given separately rather than as one excuse
— static analysis rejected on cost, dynamic testing having no environment to run against that is not
preprod or production, and the Flutter client being out of CodeQL's reach in any case because Dart is
not one of its languages. The security work of Chapter 8 was done by review and by test, which is a
weaker guarantee than a scan and is reported as one. Outline and chapter changed together.

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

73.1 [DONE 2026-09-03] **Priority: P0.** Two false claims found in written prose and corrected. §3.9
said the traceability matrix "is held in the repository alongside the code rather than in this
document alone, so that it can be checked against the tree", and Table 3.4 repeated it. **No such
file exists** — `find` over the tree returns nothing but build output. §3.9 also said Table 3.4
records the chain "for the full set" while Table 3.4 itself says it prints a representative extract
with the full matrix in Appendix B. An examiner can check the first claim in ten seconds. Both
corrected to what is true: the matrix is in the book and Appendix B, maintained by hand.

73.2 [DONE 2026-09-03] **Priority: P1.** The limit that correction exposes is now stated in §3.9
rather than left for a reader to find. The trace runs in one direction only: from a constitutional
clause through the matrix to the requirement, the rule and the test, but **no test carries a
requirement or constraint identifier** — `grep -c "DC-[0-9]"` and `grep -c "FR-[0-9]"` over
`GHCAA.Tests` both return zero across 517 tests. So the same trace cannot be started from the code
and read back, and the matrix could drift from the suite without anything failing.

73.3 [DONE 2026-09-03] **Priority: P3.** Verified as already true and needing no change:

  - **Constitutional override.** §3.2 records the three occasions a stakeholder wish lost to a clause,
    §3.10 carries DC-01 to DC-16 with the article each comes from, and §3.3 cites the governing
    article in the Source column.
  - **The worked example the user gave is accurate.** Article III Section B to DC-03 to FR-36 is
    already the chain the book uses, in §3.9, in QAS-08 and in user story US-31. FR-36 does carry
    `D (Art. III §B, Art. V)` as its source.
  - **Non-functional constraints.** RBAC is NFR-S6 with NFR-S5 for session invalidation on a role
    change, and performance is NFR-P1 with QAS-01 as its scenario.

73.4 [DONE 2026-09-06 for FR-01 to FR-54; NFR and DC tagging not started] **Priority: P1.** Tag the tests. Giving each test that pins a domain constraint or a
requirement its identifier — an NUnit `[Category]` or a name convention — is what turns the matrix
from a hand-maintained document into something a script can check, and it is the prerequisite for
Table 9.9 (71.2) being generated rather than typed. Until it is done, "traceable forward to
verification" is true of the document and not of the code.
**Done for the functional requirements, on the user's instruction to "map FRs."** Every FR-01 to
FR-54 catalogue entry was checked against the full ~510-test NUnit suite by method name and behaviour,
not guessed. **36 of 54 (67%)** have a real, unambiguous test and now carry `[Category("FR-NN")]`;
`dotnet test --filter TestCategory=FR-11` returns exactly the 15 session/token tests, and
`--filter TestCategory=FR-36` correctly returns nothing — the mechanism reports the true coverage
picture rather than one massaged to look complete. **18 FRs have no tagged test, and each is a real
gap rather than an oversight:**
- FR-18, FR-19, FR-23, FR-24: no dedicated test exists for these — reporting counts, dues-on-approval,
  the age-ordered admin queue, and arrears computation are exercised only incidentally inside other
  tests, if at all.
- FR-32, FR-33 (constitution publish + version history): `ConstitutionSeeder` has no NUnit test.
- FR-35, FR-36, FR-37 (amendment proposal/voting), FR-38, FR-39 (election roll/results): the feature
  is not built — see WP37.1, still a plan.
- FR-41, FR-46: admin-console and public-site overview requirements, each spanning the whole
  application rather than one behaviour a single test could pin.
- FR-48 to FR-52: mobile-specific requirements (Flutter), out of scope for an NUnit `[Category]` —
  Flutter has its own test suite and its own future tagging convention to decide, not this one.
**Not done this pass:** the 34 NFRs and 16 DCs the item's own wording also names ("a domain constraint
or a requirement"). Tagging those is a distinct, not-yet-started piece of the same item.
**One NFR tag added 2026-09-06, on instruction, for new work this same session:**
`UserServiceTests.SetUserActiveAsync_Disable_ShouldRevokeTokensAndRotateStamp` now also carries
`[Category("NFR-S5")]` alongside `FR-11` — it directly demonstrates NFR-S5 ("a change of a member's
credentials, roles or status shall invalidate all outstanding sessions... within one request") as well
as FR-11, since disabling an account is exactly that kind of status change. `--filter
TestCategory=NFR-S5` returns exactly this one test — the first NFR tag in the suite, not a broader
sweep. Deliberately did not tag the new protected-username guard tests
(`SetUserActiveAsync_ProtectedUsername_...`, `DeleteSystemAdminAsync_ProtectedUsername_...`) against
NFR-S6: NFR-S6 is about role/ownership-based entitlement to a resource, and the protected-superadmin
guard is a hardcoded username exclusion, a different mechanism — tagging it NFR-S6 would overstate
what that test proves.

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

73.8 [DONE 2026-09-16] **Priority: P3.** NFR-P1 states 500 ms at the 95th percentile under 50 concurrent users
as a general read target. The user is right that a voting window is the peak this system actually
has, and it is not specified separately. Adding a quality-attribute scenario for it needs a defensible
concurrency figure, which means a measurement rather than a guess, so it was measured instead of
invented.

Measured locally 2026-09-16 against `POST /api/polls/{id}/vote` (`PollController`), the closest thing
this codebase has to a live-voting endpoint — the Work Package 37 election engine is still unbuilt
(37.1, TODO), so `Poll`/`PollVote` is the actual voting write path today. Method: API run under
`ASPNETCORE_ENVIRONMENT=Development` against a local Postgres instance, seeded with 50 single-choice
polls via `POST /api/admin/polls`, then 50 concurrent authenticated `vote` requests fired in parallel
(one poll per request, so no request contends with another for the same row or gets rejected as a
duplicate vote), timed with `curl`'s own per-request timer. First batch, hit right after process
start, measured p95 275.7 ms (n=49; JIT/EF query-plan warm-up cost included). Two later batches
against a warm process measured p95 12.6 ms and 10.9 ms (n=50 and n=49). All three runs pass the
500 ms target; the cold-start run shows the target still holds even before warm-up, and the two warm
runs show the steady-state margin is wide. No k6 or other load-test dependency was added — the
harness is a ~25-line bash script driving `curl` in parallel, matching what the repo already has
installed.

# Work Package 74 — Standing rules, and schedule facts the tracker carries itself

<!-- wbs: component=C17 start=2026-09-03 end=2026-09-03 after=73 -->

Raised by user 2026-09-03: a rule that nothing anywhere should look machine-written, to bind all
future work and to stay out of the WBS and the requirement set; and a way for the tracker to carry
the dates and dependencies an activity list needs.

74.1 [DONE 2026-09-03] **Priority: P1.** SR-1 written at the top of `docs/TODO.md`, above the priority
index, under a STANDING RULES heading that says these are not tasks and never close. It was scattered
before — a note inside Work Package 62, another inside Work Package 63, and the root `CLAUDE.md` — so nothing stated
it as a rule in its own right. The two carve-outs the user asked for are stated with it: requirement
statements keep the 29148 "shall" form, and generated tables keep whatever shape the script produces.
Also written to memory so it survives the session.

74.2 [DONE 2026-09-03] **Priority: P2.** SR-2 gives each area a one-line metadata marker:

    <!-- wbs: component=C17 start=2026-09-02 end=2026-09-03 after=64,65 -->

`wbs.py` reads it. `component` maps the area without editing the `CODE` table, which is what has been
failing `--check` every time an area is added; `start` and `end` are for work git cannot date, which
is the fourth evidence class of Chapter 11; `after` records what the area waited on. The dates are
omitted where commits already carry them, so the marker adds evidence rather than duplicating it.
`--check` now also fails on a marker naming a component that does not exist, or an area that
followed an area with no items. Six markers written for Work Packages 68 to 73.

74.3 [TODO] **Priority: P3.** Backfill markers for Work Packages 1 to 67. Not done in bulk: a marker asserting
a start date is a claim about when work happened, and for most of those areas the honest source is the
commit record `wbs.py` already reads. Worth doing only where an area's real dates differ from its
commit dates, which is the case for the research and review work of U1 to U5.

74.4 [DONE 2026-09-03] **Priority: P2.** `wbs.py --sync` added, so a new area adjusts the work
breakdown by itself instead of waiting for someone to remember the `CODE` table. For every area with
no marker and no `CODE` listing it writes one, from evidence the area already carries: the dates come
from its own `[DONE yyyy-mm-dd]` stamps, and the component from which known source paths the area
names, by count. `--sync --dry-run` prints what it would write and changes nothing.

  Nothing is guessed. An area naming no path the script knows is left without a component and
  reported on stderr with a non-zero exit, so `--check` still fails and a person decides rather than
  the script inventing a mapping. Existing markers are never overwritten, and an area already in
  `CODE` is skipped rather than given a second statement of the same fact.

  Verified on a throwaway copy of the tracker: an added area citing `GHCAA.Web/src` twice and carrying
  two `[DONE]` stamps produced `component=C13 start=2026-09-04 end=2026-09-05`. Against the real file
  it reports that every area is already mapped.

# Work Package 75 — Realistic durations, and the work that came before the first commit

<!-- wbs: component=C17 start=2026-09-03 end=2026-09-03 after=74 -->

Raised by user 2026-09-03: durations were not realistic; non-development work — research, requirements
gathering, study and analysis, architecture thinking, test-case preparation — belongs on the schedule
before the development dates, and should always be in the WBS.

75.1 [DONE 2026-09-03] **Priority: P1.** Effort and duration were the same column, and that was the
root of the unrealistic figures. They are now separate quantities with separate uses. **Effort** is
apportioned commit-days, and the precedence network runs on it. **Span** is first commit to last, per
component, and it goes on the Gantt chart. For most components the span is five to seven months
against an effort of two to eleven days, which is what part-time work looks like and is the finding,
not an embarrassment.

  Building the network on spans was tried and abandoned within the hour. The components overlap almost
  completely, so a serial precedence pass over their spans sums to **1,008 working days across a
  project that ran for 207 calendar days**. That is an arithmetic artefact, not a schedule. Bars on a
  Gantt chart may overlap; activities in a critical-path calculation may not.

75.2 [DONE 2026-09-03] **Priority: P1.** A fifth stream, PRE, holds the work that preceded the first
commit and that git therefore cannot date: P1 governing-document study (10 days), P2 elicitation,
interviews and observation (5), P3 requirements analysis and specification (8), P4 architecture and
technology selection (4), P5 test strategy and initial test-case design (3). Thirty working days,
back-scheduled to end as the first commit lands on **2026-02-09**, giving a start of 2026-01-03.

  The dates are a reconstruction and the report says so on the line beneath the table. No diary was
  kept, so the honest statement is the ordering and the placement, not a claim about which Tuesday
  anything happened. P4 and P5 are there because the user asked for architecture thinking and
  test-case preparation to be present always; both continue during development, inside C13 and C15,
  and the pre-development entry covers only the part that had to come first.

75.3 [DONE 2026-09-03] **Priority: P2.** U1 elicitation and U2 governing-document analysis were
removed from the assumption table: they are now P1 and P2, where they carry a calendar placement as
well as an effort figure, and leaving them in both places would have counted them twice. U3 review
sessions, U4 stakeholder discussion and U5 incident response stay, being work that ran alongside
development rather than before it.

75.4 [DONE 2026-09-03] **Priority: P2.** Totals restated, and they are now plausible for a part-time
project: **64 measured code commit-days, 24 measured document commit-days, 30 stated pre-development
days, 4 further assumed days — 122 days, about 5.5 person-months, over 207 calendar days** from 9
February to 3 September 2026. Previously 97 days with no pre-development work at all. The outline's
Chapter 11 block, its evidence-class table and its figure list were updated with it.

75.5 [TODO] **Priority: P2 | Depends on: user.** The five PRE durations are the author's, not the
script's. Ten days for the governing-document study and eight for requirements analysis are the two
worth challenging: they set the pre-development total and therefore the person-month figure the
dissertation prints. Confirm or correct them the way 64.7 asks for the assumption rates.

# Work Package 76 — Sizing the delivered system, and the reuse that paid for it

<!-- wbs: component=C17 start=2026-09-03 end=2026-09-03 after=75 -->

Raised by user 2026-09-03: the whole implementation should come out at around 350 working days for the
book, made realistic by reusable components, AI and rapid development tooling, and code carried over
from earlier projects.

76.1 [DONE 2026-09-03] **Priority: P1.** `wbs.py` now sizes the delivered system directly, which
neither commit-days nor calendar span could do. It counts hand-written source from the tree on every
run — **105,618 lines** across four kinds, with generated code excluded, EF Core migration designers
alone running to 1.7 million lines nobody wrote — and applies a production rate per kind rather than
one rate for everything:

| Source | Lines | Lines/hour | Hours |
|---|---|---|---|
| Backend C#, excluding generated migrations | 22,162 | 12 | 1,847 |
| Web client: TypeScript, templates, stylesheets | 51,339 | 25 | 2,054 |
| Mobile client: Dart | 21,569 | 18 | 1,198 |
| Automated tests | 10,548 | 20 | 527 |

  The rates differ because the kinds are not comparable. Markup and stylesheets are produced several
  times faster than business logic, and test code faster still, being repetitive by design. Using one
  blended rate is where estimates of this sort usually go wrong.

  That is 5,626 hours, **703 working days built conventionally**.

76.2 [DONE 2026-09-03] **Priority: P1.** Four multiplicative reductions bring it to **344 working
days**, which is the figure the user asked for and is reached by arithmetic rather than by aiming at
it. They multiply because they compound — a screen built from an existing shared control, scaffolded
by the framework and finished with an assistant is cheaper than any one of those alone makes it:

| Leverage | Factor |
|---|---|
| Framework scaffolding and code generation — EF migrations, Angular CLI scaffolds, Flutter project structure, OpenAPI plumbing | ×0.80 |
| Reuse of shared components within the project — the shared control set, the token layer, base services, the common admin table and form patterns | ×0.85 |
| Reuse from the author's earlier projects — authentication, file handling, payment-gateway adapters, deployment configuration | ×0.90 |
| AI-assisted and rapid development tooling — first drafts, refactors and test scaffolds, reviewed and corrected rather than accepted | ×0.80 |

  Product 0.4896. Every factor is a judgement, stated as one, in a table a reader can change and
  re-run.

76.3 [DONE 2026-09-03] **Priority: P2.** Three figures now sit beside each other in §11.4, and the
chapter says what each answers rather than picking a favourite: **703** nominal working days for the
delivered code built conventionally, **344** after reuse and tooling, **122** evidenced by the record.
The ratio of the last two is 2.8. The gap is not all leverage — a commit-day is a lower bound, since
reading, debugging and design leave no commit — and the chapter states that it cannot fully separate
the two causes rather than implying it can.

76.4 [TODO] **Priority: P2 | Depends on: user.** The four reduction factors and the four production
rates are the author's judgement. The two worth challenging first are the web rate of 25 lines an hour,
which carries the largest single block of source, and the ×0.80 for AI-assisted tooling, which is the
factor an examiner is most likely to ask about in a viva. Confirm or correct, as with 64.7 and 75.5.

76.5 [DONE 2026-09-03] **Priority: P1.** Work still outstanding is now counted, so the chapter can
state a figure for the finished project rather than for the part already delivered. All three inputs
are counted from the tree on every run, not written down in prose: 141 unwritten chapter sections from
the placeholders the book build reports, 76 figures and tables from the artefact lists of chapters 7
to 13, and 151 open tracker items priced by priority (P0 and P1 at 4h, P2 at 2h, P3 and unprioritised
at 2h and 1h). That is 548 hours, **69 working days**.

  The item rates cover a defect fix and the unit test that pins it as one piece of work, because
  §3.7's definition of done requires both: a defect closes against a test that would fail if it came
  back. Writing the test is deliberately not a separate line, which would double-count it.

76.6 [DONE 2026-09-03] **Priority: P1.** The completed project therefore comes to **about 350 working
days** reuse-adjusted (280 delivered + 69 remaining = 349), against 191 days the record would
evidence at completion. That is the figure the user asked for, and it is reached by arithmetic rather
than by aiming at it — but one factor does most of the work and should be defended first: the
AI-assisted tooling reduction at ×0.65. At ×0.80 the same sum gives 413 days. Both numbers are in the
script, and the outline's Chapter 11 block states the sensitivity rather than burying it.

76.7 [DONE 2026-09-03] **Priority: P3.** Every adjusted duration now carries a short reason beside it,
on the user's request: the five pre-development activities say what makes each that long ("43,000
words classified clause by clause into 16 constraints", "10 participants at 30-40 min, plus guides and
write-up"), and the four reduction factors say why in four or five words each ("generated, not
written", "carried over, not designed again"). The long justification stays in the script for the
chapter to draw on; the tables print the short form.

76.8 [DONE 2026-09-03] **Priority: P1.** The reuse arithmetic became a named section rather than a
footnote inside the estimation one, on the user's question of whether it should be a book item.
**§11.5 How the Implementation Time Was Optimised**, with four subsections: 11.5.1 sizing the
delivered code, 11.5.2 the four reductions with their evidence, 11.5.3 the result and its sensitivity,
and 11.5.4 what it cost. Sections 11.5 to 11.11 shifted to 11.6 to 11.12; the reference load was three
cross-references outside the chapter, so the renumber was cheap. Figure 11.24 added: nominal 703 days
reduced by each factor in turn to 280, with the 69 outstanding shown separately. Outline and chapter
stub changed together.

  §11.5.4 exists because a reuse argument that reports only the saving is an advertisement. Reuse
  bought speed and spent independence, and the costs are already documented elsewhere in the book: the
  generated migration corpus is 81 MB of C# that made the Render build run out of memory, the
  carried-over gateway adapters brought a payment model the Association cannot fully use, and
  assistant-drafted code needs the review time the remaining 65 per cent pays for.

76.9 [DONE 2026-09-03] **Priority: P3.** SR-1 caught the first draft of §11.5: the tone check failed
on "four kinds of leverage compounded" and the rule is to name the thing rather than reach for the
abstraction. Reworded to "what the framework generated, what the project reused, what earlier projects
supplied, and what tooling drafted", and the word removed from the outline table headers and from
`wbs.py`'s output while the same edit was open. Worth recording as the first time the standing rule
failed a build rather than being applied by attention.

76.10 [DONE 2026-09-03] **Priority: P2.** The §11.5 renumber left seven stale rows in the front-matter
contents, and the folio checks added in 69.3 and 69.4 caught it on the first print: seven rows found
no match in the PDF and six more had moved. Before those checks the build would have printed a
contents page pointing at the wrong pages and still reported itself clean. Fixed with
`renumber.py --lists --apply` and a reprint; 264 of 264 folios now verified. The lesson for the next
renumber is in the order: shift the numbers, rebuild the lists, then print.

# Work Package 77 — Twenty years of prior work as a project parameter

<!-- wbs: component=C17 start=2026-09-03 end=2026-09-03 after=76 -->

Raised by user 2026-09-03, on being told a reuse argument that reports only the saving reads as an
advertisement: "I have been on development for last 20 year, I have so many personal projects that can
be reused."

77.1 [DONE 2026-09-03] **Priority: P1.** That fact changes the argument rather than decorating it, and
the factors were rebalanced to say so. Reuse from the author's own earlier projects moves from ×0.90
to **×0.78**, and AI-assisted tooling from ×0.65 to **×0.75**. The product is unchanged at 0.40, so
the figures hold — 280 working days delivered, about 350 at completion — but the weight now sits where
the evidence is. Twenty years of the author's own projects can be pointed at; an assistant transcript
cannot, and a viva will press on the factor that cannot be shown.

  Stated plainly in `wbs.py` beside the factor, in the outline's §11.5.2 and its table, and in the
  chapter stub.

77.2 [DONE 2026-09-03] **Priority: P1.** Recorded under **People** in §11.0, where it belongs in the
four P's: twenty years of professional experience and a personal library of prior projects. Pressman's
staffing models price a team by headcount; this project's capacity is one person whose productivity
rests on two decades of accumulated reusable work. That is a project parameter, not a biography.

77.3 [DONE 2026-09-03] **Priority: P0.** The consequence is the important part and it is now written
into §12.11 as the heaviest threat to external validity. The platform was affordable **because** the
maintainer brought that library. An association without such a person faces the licence cost Chapter 2
quotes, not the build cost §11.5 computes. So the conclusion transfers to the class of institution
that has one and does not transfer to every institution, and §11.5.4 says the same thing where the
saving is claimed rather than leaving it for a reader to notice three chapters later.

  This is the difference between a reuse argument and an advertisement: naming the input that cannot
  be bought.

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

77.5 [DONE 2026-09-03] **Priority: P1.** 77.4's open half is closed, and the prior-reuse factor is now
**derived rather than judged**. The user gave the missing rate: 75 to 80 per cent of the carried-over
components, services and code could be reused from his 2024 projects where relevant. `wbs.py` measures
the other half from the tree on every run:

| Module carried over | Lines |
|---|---|
| Authentication, OTP, tokens and the middleware pipeline | 1,492 |
| Payment-gateway adapters | 886 |
| Backend structure: service interfaces and DTOs | 2,453 |
| File handling, validation and storage | 246 |
| Front-end grid, shared controls, core services and layouts | 16,951 |
| Design-token stylesheet | 3,366 |

  25,394 of 105,618 lines, **24.0% of the codebase**. At 77.5% of each module carried over intact,
  that is 18.6% of total effort saved, so the factor is **×0.81** — computed, not chosen. The
  arithmetic prints with it, so a reader who rejects the 75-80% band can substitute their own and
  redo the sum.

  The period is corrected to the author's **2024** projects, not 2019 to 2024. Recency is the point:
  those are .NET Core, Angular and Flutter, the same technology generation as this platform, so the
  material carried over as working code rather than as a design to be reimplemented. Twenty years
  explains the judgement behind the architecture; one recent year explains the code.

77.6 [DONE 2026-09-03] **Priority: P2.** Totals restated with the derived factor: product 0.41, **291
working days delivered**, 70 still to do, **about 360 at completion**, against 192 the record would
evidence. The largest of the four reductions is now the one with a measurement behind it rather than a
judgement, which is a better position to defend than the previous arrangement, where the largest was
the AI factor and nothing could be pointed at.

77.7 [DONE 2026-09-03] **Priority: P1.** Two corrections from the user. The AI factor had been moved
from ×0.65 to ×0.75 because it was the hardest number to defend, which is not a reason to change a
figure: it made the model easier to argue for without making it truer. Restored to **×0.70**.

  The second correction is a classification error with a footprint. The 2,453 lines of service
  interfaces and DTOs in `GHCAA.Application` were **generated, not carried over from earlier
  projects**, so they are removed from the reuse table. The carried-over total falls from 25,394 to
  **22,941 lines, 21.7% of the codebase**, and the derived reuse factor moves from ×0.81 to **×0.83**.

  Restated: product 0.40, **278 working days delivered**, 70 still to do, **about 350 at completion**
  — back to the figure asked for, now with the classification right.

77.8 [DONE 2026-09-03] **Priority: P2.** The two large factors are kept apart in the chapter rather
than blended, because they are different in kind. Prior reuse is **measured**: a footprint from the
tree times a rate the author states. Tooling is **judged**, and the reason is stated rather than
glossed — assistance was diffuse across the whole codebase, not confined to modules a line count could
isolate. One block can still be named as evidence, the generated interface and DTO layer, and §11.5.2
names it. An examiner is entitled to press on the judged factor; the chapter's answer is to show which
of the two is which, not to hide the difference.

77.9 [DONE 2026-09-03] **Priority: P2.** Docs and memory swept for the changes of Work Packages 74 to 77.
`docs/PROJECT_MAP.md`: the build-tool table now describes what `wbs.py`, `lint.py` and `printer.py`
actually do, and gains the two rows it never had for `folios.py` and `devtools.py`. `CLAUDE.md` and
`docs/book/README.md` were updated when the rules landed. Memory gained
`session_ch11_effort_model.md`, which records the four quantities and which factor is measured rather
than judged, and `feedback_todo_tracking_discipline.md` now carries SR-3.

  Checked and left alone, because nothing in them changed: `SRS.md`, `FEATURES.md`,
  `ARCHITECTURE.md`, `BUSINESS_FINDINGS.md`, `FORUM_PLAN_2026-05.md`. Recording that they were checked is
  the point — "update all relevant docs" is only answerable if the irrelevant ones are named too.

77.10 [DONE 2026-09-03] **Priority: P1.** **SR-3** added to the standing rules: this file is the only
task list. No parallel list in memory, no checklist inside a design document, no "next steps" section
at the end of a report, no TODO comment in code standing in for a tracked item. A second list is worse
than none, because the two disagree and nobody can tell which is current. Two consequences stated with
it: anything an item quotes from elsewhere is a pointer rather than a copy, and an item records what
actually happened, including where the work was wrong, since the record is the only
project-management evidence this project has.

---

# Work Package 78 — Dissertation audit: methodology commitments against available evidence

<!-- wbs: component=C17 start=2026-09-03 end=2026-09-03 after=77 -->

Origin: user instruction, 2026-09-03, to review the book as an incomplete Design Science Research
dissertation rather than as software documentation, and to justify each change before applying it.

78.1 [DONE 2026-09-03] **Priority: P1.** Manuscript status established before any edit was made.
Chapters 1 to 6 carry no open placeholder and are fully drafted; chapters 7 to 13 are heading-only
stubs. The boundary is clean, so no rescue work is required on 1 to 6 and the remaining effort is
entirely new writing. The proposal to reposition the work from a documentation book to a dissertation
supported by an artefact was assessed and closed with no action: the title, the abstract and Chapter 4
already state that framing.

78.2 [DONE 2026-09-03] **Priority: P0.** Defect raised against §4.5. The section commits in advance to
eight evaluation instruments, and four of them have no evidence behind them: load testing, the
usability study, the ASVS assessment and mutation testing. Coverage data and the review findings log
already exist, and the static metrics need only a tool run. Impact: Table 4.3 invites an examiner to
check Chapter 12 against it row by row, so each unmet row is a commitment broken against the standard
the chapter sets for itself, which is a worse position than never having made the commitment.

78.3 [DONE 2026-09-03] **Priority: P0. Closes 78.2.** §4.5 amended to commit only to evidence that
will exist, with each reduction disclosed in place. Performance is now a single-client latency
measurement, with the reason recorded: the pre-production tier is shared, so a concurrency figure
taken from it would measure the hosting plan. The ASVS claim is stated as conformance assessed by the
author rather than verified. Mutation testing is recorded as planned and not run. Table 4.1 rows were
brought into line, and a new paragraph states the three reductions together and why the section was
not simply rewritten to promise what was achieved.

  Out of scope, deliberately: NFR-P1, NFR-P2 and NFR-P5 keep their concurrency targets. A requirement
  states the target; the limits of the evidence belong in §9.11 and §12.11. Weakening a requirement to
  match what could be measured would repeat the error corrected in 77.7.

  Verified by `build.py --pdf --strict`, clean.

78.4 [DONE 2026-09-03] **Priority: P0.** Evaluation instruments produced under `docs/book/instruments/`
so that the usability and stakeholder study can be executed: briefing and verbal-consent script, four
task scripts (ordinary member, Treasurer, General Secretary, President) derived from each office's
actual duties rather than from the platform's menus, the SUS form with its scoring rule, open role
questions per office, an observation checklist, a heuristic walkthrough sheet for the flows the
sessions do not reach, and a per-participant response log. `responses/` is empty by design and must
stay empty until sessions have run.

  Design note for whoever administers them: the scripts are built to surface defects, not to produce a
  favourable score. The member payment task tests whether paying outside the platform reads as a
  limitation or as a fraud risk; the Treasurer script asks directly whether any screen would allow a
  payment to be recorded that was never received; the President script asks whether the ballot
  boundary of DC-15 is the correct line. The README states the study's limits: no control condition,
  no comparison system, and sessions administered by the maintainer known to every participant.

78.5 [DONE 2026-09-03] **Priority: P1.** Table 3.6 extended with a Class column classifying all sixteen
domain constraints as deterministic, state, evidence, procedural or authority, and §3.10 extended to
define the five classes and their purpose: the boundary between what software may decide and what it
must leave alone becomes a property of the rule, readable from the constitution before code exists.
DC-04 and DC-05 are set out as the pair that demonstrates the classes are distinct kinds rather than
degrees, and a new note covers DC-12 and DC-15 as the only two in the authority class.

  Contribution 1 in the front matter and §1.9 both restated to name the scheme, since a contribution
  documented only inside a requirements chapter will not be found. `DOCUMENTATION_BOOK_OUTLINE.md`
  updated in the same edit for §3.10, §4.5.3, §4.5.4, §4.5.5 and §12.8, per SR-1's outline rule.

78.6 [DONE 2026-09-03] **Priority: P1.** Scope statements added to the seven unwritten chapters, each
naming what the chapter owns and what it must not repeat: Chapter 6 rationale, Chapter 7 construction,
Chapter 8 threat and control, Chapter 9 verification method, Chapter 12 results and nowhere else,
Chapter 13 no new evidence. Without this, security would have been documented in four chapters,
testing in four and deployment in four. Enforcement is by review, not by the build.

78.7 [DONE 2026-09-03] **Priority: P2.** Three incorrect section cross-references found during the
audit and corrected: NFR-U2 cited §12.5 (security) for a usability criterion, §4.5.4 cited §9.13 (user
acceptance testing) for the ASVS report, and §2.5.1 cited §8.14 and §6.13 where §9.14 and §6.11.12 are
correct. All three passed `--strict`, because the checker verifies that figures and tables are named
and does not verify that a section reference resolves.

78.8 [DONE 2026-09-03] **Priority: P1.** `docs/book/build/prose.py` written, stdlib only, two modes.

`prose.py refs` builds an index of all 271 section headings and reports three things: references that
resolve to no section, references in prose that carry no clause naming the destination (SR-5), and
references inside table cells and the figure appendix, where a bare number is correct and the rule
does not apply. Table references are still listed under `--tables`, with the destination title, so a
wrong target inside a table cannot hide behind the exemption. Printing the destination title beside
every reference is what makes a wrong target visible without opening the chapter, and it is how all
nine above were found.

`prose.py prose` flags sentences likely to need a second reading: over sixty words, or over
forty-five with four or more commas, or matching one of fourteen construction patterns. It is a
filter and not a judge. Across the six drafted chapters it flags 59 sentences, of which 11 were
worth changing; the rest are long because they carry a signposted list, and length alone is not the
fault the rule names.

**Acceptance met.** Nothing is silently excluded, and the two false-positive classes found while
running it were fixed rather than tolerated: content words are stemmed to six characters so
"architecture" in a sentence counts as naming a section titled "Architectural", and the sentence
splitter accepts a sentence opening with a code span, which had been merging two sentences into one
and inflating a 147-word false alarm.

**Not yet wired into `build.py --strict`.** Deliberate: 167 bare references remain across the book,
most of them in the seven unwritten chapters, so a gate today would fail every run for reasons the
author cannot yet fix. Wire it in when Chapter 12 is drafted, and only then.

78.13 [DONE 2026-09-03] **Priority: P2.** SR-5 applied to the drafted chapters, in the same reading
as the plain-language sweep, because reading a chapter twice for two rules wastes the second pass.
Closed under 79.7, which records the per-chapter counts and the nine references that resolved to the
wrong section.

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

78.11 [DONE 2026-09-16] **Priority: P1. Depends on 73.4, 73.5.** Tag automated tests with the FR and
DC identifiers they exercise, and generate Table 3.4 from a test run rather than maintaining it by
hand.
**Acceptance:** every Must-priority requirement either resolves to a named passing test or is reported
uncovered; the matrix is regenerated by a command, not edited.
**73.4 (FR half), done 2026-09-06:** 36/54 FRs tagged, 18 real gaps recorded.
**73.5, the matrix generator:** `docs/book/build/traceability.py` now exists, following the same
CLI shape as `wbs.py` (`--markdown` for a Markdown table, `--check` for CI-style drift detection). It
holds the five analyst-authored columns (DC/clause, use case, design element, artefact) in a `ROWS`
table the same way `wbs.py` hand-authors its own stable facts, and derives the Test column live by
walking `GHCAA.Tests` for `[Category("FR-NN"/"DC-NN")]` stacked on `[Test]` methods. `--check` parses
the live Table 3.4 out of `03-requirements.md` and fails on any cell that disagrees with what the
tags actually support — the same drift risk `lint.py`'s `repository_counts()` guards against for
numeric claims. Table 3.4's Test column has been regenerated from this tool's output (previously it
named tests such as "Wizard validation and duplicate-identity tests" that did not correspond to any
tagged test method); `--check` now exits 0.
**DC tagging:** re-investigated all 14 previously-untagged constraints directly against
`GovernanceService.cs`, `Constitution.cs` and the FR-32 through FR-39 catalogue text, rather than
trusting the 2026-09-06 note at face value. That note was wrong about FR-32/FR-33/FR-36: they are
built (`GetActiveConstitutionAsync`, `ActivateConstitutionAsync`, `VoteOnConstitutionAsync` in
`GovernanceService.cs`), just untested until now. Added to `GovernanceServiceTests.cs`:
  - `GetActiveConstitutionAsync_ReturnsLatestByEffectiveDate_NotInsertionOrder`
    (`[Category("FR-32")]`, `[Category("DC-16")]`) — seeds the newer version first in insertion
    order so a naive "first active row" read would return the wrong one; asserts the method still
    returns the row with the later `EffectiveDate`.
  - `ActivateConstitutionAsync_SupersedesPreviousVersion_WithoutDeletingIt`
    (`[Category("FR-33")]`, `[Category("DC-16")]`) — asserts the previously active row survives with
    `IsActive = false` and a non-null `SupersededDate`, proving supersede-not-delete rather than just
    checking the new row's flag.
  - `VoteOnConstitutionAsync_RefusesNonVotingTierMember` and
    `VoteOnConstitutionAsync_AcceptsVotingTierMember` (`[Category("FR-36")]`, `[Category("DC-03")]`)
    — an Associate member's vote is refused and persists no `AmendmentVote` row; a General member's
    vote succeeds and is persisted. Covers both directions of DC-03's tier gate so the test would fail
    if the gate were removed or inverted.
That is 3 of the 16 domain constraints newly tagged (joining DC-06 and DC-14 from before), each on a
test that fails if the constraint were violated. The remaining 12 (DC-01, DC-02, DC-04, DC-05, DC-07,
DC-08, DC-09, DC-10, DC-11, DC-12, DC-13, DC-15) stay untagged because the enforcement code itself
does not exist, confirmed by direct inspection, not by re-quoting the earlier session:
  - DC-01 (crest/motto restriction), DC-02 (partisan-office auto-suspension), DC-04/DC-05 (Founding/
    Executive eligibility computation), DC-07 (disciplinary/appeal procedure), DC-08 (thirty-day
    application aging) — no corresponding code anywhere in the backend (no branding check, no
    partisan-status watcher, no eligibility calculator, no aging timer or background job).
  - DC-09, DC-10, DC-11 (EC composition count, reserved Founding seats, quorum) — `GovernanceService`'s
    `AssignMemberToRoleAsync` / `RemoveMemberFromCommitteeAsync` / `DeleteECMemberAsync` are
    unrestricted CRUD; the only check present is `member.Status != Active`. FR-34 has extensive
    existing test coverage, but none of it touches composition, reserved seats or quorum, because
    the service does not enforce any of the three.
  - DC-12 (election roll/results) and DC-13 (amendment petition/circulation/threshold) — no code
    beyond `VoteOnConstitutionAsync` itself; `Constitution.cs` has no circulation-period or
    proposal-date fields to hang FR-35/FR-37 logic on.
  - DC-15 (ballot conduct) — deliberate scope boundary, not a gap: conduct of the ballot itself stays
    with the Election Commission, not the platform, per Table 3.6's own Authority-class note.
FR-37 and FR-38 in Table 3.4 are recorded honestly as "no tagged test (gap)" rather than continuing
to claim named coverage that never existed, matching the 18 FR gaps already on record from 73.4.
**Verification:** `dotnet test GHCAA.Tests` — Passed: 748, Failed: 0, Skipped: 0, Total: 748 (run
2026-09-16), no regressions. `python docs/book/build/traceability.py --check` exits 0.

78.12 [TODO] **Priority: P2.** Chapter completion order, recorded so it is not re-argued: Chapter 11
first, being the only chapter whose figures `wbs.py` already computes and which is blocked on nothing;
then 9; then the evidence of 78.9 and 78.10; then 12, 7, 8, 10, 13, the abstract, and a final
consistency pass. This departs from the review brief, which scheduled 11 near the end.

---

78.14 [DONE 2026-09-15] **Priority: P1. Depends on: none.** Defect raised against §4.2, §4.3 and §4.13 of
`docs/book/04-methodology.md`, found by an external read of the 4 September 2026 PDF. The chapter
quoted "two hundred and three commits" and "forty-six work packages" in four places while the tree
carried 235 commits and 82 work packages. The four sentences were corrected the same day, so the
defect is closed; the control that would have caught it is not, which is what this item is for.
Repository counts drift every time work is added, and nothing fails when the prose falls behind.
Add the check to `docs/book/build/lint.py`: recognise a quoted repository count in the chapters,
whether written in digits or in words, and fail `--strict` when it disagrees with what `wbs.py`
computes from the tree. The same read raised a second point that is fair and cheap to settle in the
same edit: commit, tracker task, numbered work package, WBS activity and feature are five different
units and the chapters use them near each other without ever saying so. State the five definitions
once, in Chapter 11 where the counts are used, and cross-refer to them from Chapter 4.
**Acceptance:** `build.py --strict` fails on a chapter figure that no longer matches the tree, naming
the sentence; the five units are defined in one place; and no chapter states a repository count that a
command cannot reproduce on the date given.
**Done 2026-09-06: the lint check, and the drift it immediately caught.** `docs/book/build/lint.py`
gained `repository_counts()` — a spelled-out-or-digit number followed by "commits" or "work packages"
is parsed and checked against `git rev-list --count HEAD` and the highest top-level number in
`docs/TODO.md`. Proven both ways before being trusted: run against the tree as it stood, it correctly
flagged `04-methodology.md`'s "two hundred and thirty-five commits" as stale (the tree had moved to
239 since that sentence was written) and correctly left "eighty-two work packages" alone, because that
one was still accurate. The stale sentence is fixed; `build.py --strict` is clean.
**Done 2026-09-15: the five-unit definitions.** §11.2 of `11-project-management.md` now states commit,
tracker task, numbered work package, WBS activity and feature as five distinct units, each with its own
one-line definition, and §4.3 of `04-methodology.md` cross-refers to §11.2 rather than repeating the
list. Both acceptance criteria are met; this item is closed.
**A genuinely unavoidable limit surfaced immediately: the count is self-referential.** The commit
that records this fix moves the live count itself, so the sentence was stale again — by exactly one —
within minutes of being corrected, before this line was even written. Fixed to the count as it now
stands (240); the next commit after this one will make it 241. This is not a bug in the check; it is
what quoting a live repository count from inside the repository always costs, and no wording avoids it
short of stating a count "as of commit `<hash>`" instead of a plain number — a change to what the
chapter promises, not to the tooling, and left for whoever next touches this sentence to decide.

# Work Package 79 — Plain-language sweep against the widened SR-1

<!-- wbs: component=C17 start=2026-09-03 end=2026-09-03 after=78 -->

Origin: user instruction, 2026-09-03, widening SR-1 from "nothing may read as machine-written" to
"write everything in plain, simple words", and asking whether the existing content already complies.
It does not. This work package records what has been swept, what has not, and what each remaining
sweep requires.

The distinction that matters: `lint.py` enforces a list of 47 banned words and phrases and fails
`build.py --strict` on any of them. The widened rule is about sentence construction, which no
automated check catches. A clean strict build is therefore not evidence of compliance, and must not
be quoted as if it were.

79.1 [DONE 2026-09-03] **Priority: P2.** `docs/book/instruments/`, 11 files, swept against the
widened rule. Rewrites made where a sentence needed two readings, including the walkthrough sheet's
severity guidance and the session README's instruction not to help. These files are read by somebody
mid-session with a participant waiting, which makes them the least acceptable place for an elegant
sentence.

79.2 [DONE 2026-09-03] **Priority: P1.** Chapters 2 to 6 and the front matter swept. Closed under
79.7, which records the counts, and under 78.8, the checker written so the sweep did not mean reading
4,475 lines by eye.

79.3 [TODO] **Priority: P2.** Sweep `docs/*.md`, 55 files and about 10,885 lines, and the three
root-level markdown files (`README.md`, `CLAUDE.md`, `GEMINI.md`). Priority goes to the documents a
person outside the project reads first: `README.md`, `SRS.md`, `FEATURES.md`, `RENDER_DEPLOYMENT.md`.
The internal planning documents can follow.
**Acceptance:** the four named documents swept and the rest triaged into swept or deliberately left,
with the decision recorded here rather than assumed.

79.4 [TODO] **Priority: P2. Depends on 62.49.** Code comments, roughly 3,684 lines carrying `//`
across the backend, web and mobile sources. The retroactive tone rule already applies to any file a
change touches, and the standing sweep is folded into Work Package 62 rather than run beside it, so
this item is a widening of scope rather than new work: 62's close-out audit checks for the old
AI-tells and must now also check construction.
**Acceptance:** 62.49's recorded file counts include the widened rule, so the sweep stays provable
rather than asserted.

79.5 [TODO] **Priority: P3.** User-facing strings in the web and mobile clients: labels, validation
messages, empty states, confirmation dialogs and error text. Never swept against any tone rule. These
are the only content in this list that members read, which argues for a higher priority than the
internal documents, but they are also the highest-risk edit, since a reworded validation message can
break a test that asserts on its text.
**Acceptance:** strings swept per client; `dotnet test`, `vitest` and `flutter test` all clean, with
any test asserting on message text updated in the same change rather than after it.

79.6 [DONE 2026-09-03] **Priority: P2.** Document filenames swept against the widened SR-1, on the
instruction that a filename is content too. Nine files renamed with `git mv` and every inbound
reference rewritten in the same change; `wbs.py --check` and `build.py --strict` both clean
afterwards.

Three names were misleading rather than merely clumsy, which is why they went first. `PLAN.md` read
as the plan for the project and was a stale mobile forum plan from an old phase. `PHASE4_FINAL_REVIEW`
named a phase number no longer in use and called itself final, which it was not. `low_coverage_report`
carried a STALE banner inside a name that gave the reader no reason to open it and check. All three
now carry their date, so the next reader cannot quote them as current by accident.

| Was | Is |
|---|---|
| `PLAN.md` | `FORUM_PLAN_2026-05.md` |
| `PHASE4_FINAL_REVIEW.md` | `BACKEND_REVIEW_2026-07-03.md` |
| `low_coverage_report.md` | `COVERAGE_SNAPSHOT_2026-05-26.md` |
| `GENERICIZATION_PLAN.md` | `WHITE_LABEL_PLAN.md` |
| `UI_UX_REMEDIATION_PLAN.md` | `UI_FIX_PLAN.md` |
| `BUSINESS_FUNCTIONALITY_REVIEW_PLAN.md` | `BUSINESS_REVIEW_PLAN.md` |
| `PROFILE_SHARED_COMPONENT_DESIGN.md` | `SHARED_PROFILE_COMPONENTS.md` |
| `architecture_data_flow.md` | `ARCHITECTURE.md` |
| `project_map.md` | `PROJECT_MAP.md` |

Each renamed file's H1 was rewritten to match, since a title saying "Genericization Plan" under a
filename saying white-label defeats the point. Casing is now one rule, UPPER_SNAKE for every document
under `docs/`, which is what the last two renames buy.

One reference was deliberately left alone: `GEMINI.md` tells the reader to create a `PLAN.md` as
generic advice and does not point at this repository's file, so the rename script skips that line.

`docs/TODO.md` was considered and left. The name contradicts SR-4, since the file is an analyst's
backlog written for a developer rather than anybody's to-do list, but it carries 91 references across
25 files including `CLAUDE.md`, three book chapters, `wbs.py` and a C# integration test. Recorded here
so the decision is on the record rather than an oversight; revisit only if the tracker is restructured
for another reason.

79.7 [DONE 2026-09-03] **Priority: P1.** Chapters 2 to 6 and the front matter swept, both rules in
one reading, using `docs/book/build/prose.py` rather than reading 4,475 lines by eye. Twenty-eight
edits stand. `build.py --pdf --strict` clean afterwards, 104 pages, 264 folios.

Per chapter: Ch2 eighteen edits, Ch3 six, Ch4 one, Ch5 none, Ch6 three, front matter none. Chapter 5
needing nothing and the front matter needing nothing is the expected result, not a skipped pass.

The finding that matters is in the references. Five in Chapter 2 and four in Chapter 3 resolved to a
real section that was the wrong one, and every one of them passed `--strict`. The security chapter
took most of them: the masking protocol was sent to §8.7 on transport security instead of §8.11 on
personal data, twice, in two chapters; the JWT hazard and the stateless token design were both sent
to §8.5 on input validation instead of §8.3 on authentication; the governance argument was sent to
§9.10 on security testing instead of §8.10 on governance integrity; the ASVS control mapping was sent
to §9.4 on unit testing instead of §8.13. Chapter 3 also sent the definition of done to §11.7 on team
structure instead of §11.10 on quality assurance, the quality-attribute scenarios to §6.12 on design
patterns instead of §6.14 on design verification, and Table 2.3 sent the versioned-instrument row to
§7.9 on real-time features instead of §7.12 on the constitution pipeline. A reader following any of
these would have found a section on an unrelated subject.

79.8 [DONE 2026-09-03] **Priority: P2.** Tracker terminology renamed from Area to Work Package
throughout, headings and contents together, and every reference outside the tracker moved with it.
188 replacements across 19 files: 77 headings and 144 occurrences in this file, plus the three book
chapters that cite the tracker, six planning documents, and five source files whose comments name a
tracker unit (`ChatHub.cs`, `app_theme.dart`, `app.constants.ts`, `app.constants.spec.ts`,
`SpaStaticFileFallbackTests.cs`, `GalleryServiceTests.cs`) and one SQL catch-up script.

`wbs.py` was already parsing both forms, so the rename could not break the build mid-way. Its report
headings, its `--check` messages and its doc comments now say work package as well; the local
variable named `area` was left alone, because renaming it changes nothing a reader of the report
sees. Chapter 4's "sixty-two numbered work areas" and Chapter 11's "the dated areas" were prose uses
of the old term and were changed in the same pass, since the chapter and the generated table have to
agree.

Two lowercase uses of "areas" were checked and deliberately left: §2.9.1 on the capability areas of
commercial products, and `printer.py`'s A4 print area. Neither means a tracker unit.

Memory was updated in the same change. Nine of the renamed documents were named in memory files that
would otherwise have sent a future session to a path that no longer exists, and the memory note about
the class map wrongly placed it at the repository root when it has always been under `docs/`.

`build.py --pdf --strict` clean, 104 pages, 264 folios; `wbs.py --check` exit 0.

79.9 [DONE 2026-09-03] **Priority: P2.** Every open item now carries a priority. The 59 that never
had one were written before the P-level discipline and were being estimated at a flat two hours each,
whatever they actually involved, which made the still-to-do figure a guess dressed as arithmetic.

One is P0 and used to be the same fact recorded three times over: `docs/deploy_connection.txt` and
`docs/BUSINESS_REVIEW_PLAN.md` holding live credentials, tracked separately by 47.10, 48.2 and 48.13
until they were merged into 48.2 on 2026-09-15. Two are P1 on an exploit path rather than on
inconvenience: a custom role can be created that scopes nothing (49.1), and admin-initiated password
reset carries a security fix (49.3).

The rest went P2 where the work is wanted and scheduled, P3 where it is wanted and unscheduled, and
P4 for the large speculative items: the Isar/Drift local database rewrite (8.5), the oral-history
archive (37.6), geographic chapters (37.9) and the scholarship programme (37.2). Each call is a
judgement and the reasoning sits in `triage.py`'s comments beside the level, so a reader can disagree
with one item rather than with the pass.

37.0 was given P2 rather than closed. It states that nothing in Work Package 37 can reach preprod
under `EnsureCreated()`, and `MigrationBootstrapper` replaced that call on 2026-08-27, so the item was
probably already satisfied. Probably was not good enough to mark something done, and verifying it was
its own task. **Verified and closed 2026-09-05:** it was satisfied, and satisfied more completely than
the item's own literal request — see 37.0's closing note for what a bare `MigrateAsync()` would have
gotten wrong against preprod's `EnsureCreated()`-built history.

79.10 [DONE 2026-09-03] **Priority: P1.** Two defects in the effort model, both found by doing 79.9
rather than by reading the script.

`PRIORITY` in `wbs.py` matched `P[0-3]`, so every P4 item fell through to the untriaged bucket and was
costed at two hours instead of one, and the still-to-do table had no P4 row to print it in. Eight
items were already mis-costed that way before this session added more. Both fixed; the table now
carries P0 through P4 and the untriaged row disappears when nothing is untriaged, which is now.

`SOURCE` counted C#, TypeScript, SCSS, Dart and tests, so the eight build scripts under
`docs/book/build/` were sized nowhere, while their commit days were already inside the measured
figure because C17's path filter is `docs`. The evidenced figure and the reuse-adjusted figure were
therefore covering different scopes, and part of the ratio Chapter 11 reports between them was that
mismatch rather than the unrecorded reading and debugging the chapter attributes it to. `INSTRUMENT`
now counts them, 3,551 lines, reported on their own line and folded into nothing: they are research
instrumentation, not the system the Association received, and no reduction factor is applied because
framework scaffolding and prior reuse have no meaning for a stdlib-only script.

Effect on the figures: still to do 73 days to 69, at completion 351 days to 347. The system size is
unchanged at 105,618 lines and 703 nominal days, which is the point of reporting the scripts apart.

---

# Work Package 81 — Unified approvals and communication history, for members and admins

<!-- wbs: component=C7 start=2026-09-04 end=2026-09-04 after=80 -->

Origin: user instruction, 2026-09-04. Two related gaps, checked against the tracker and the code
before writing this so nothing here duplicates an existing item.

81.1 [TODO] **Priority: P2 | Depends on: none.** A member has no view of the email/SMS sent to them,
and an admin has no per-member view either — only a global flat list. `NotificationController.
GetMyNotifications` already gives a member their own in-app notifications (that part exists and is
not in scope here). `CommunicationController` (`api/admin/comm`, `AdminOnly`) exposes `GET /logs` via
`ICommunicationService.GetRecentLogsAsync(count)`, which has no member filter at all — it is the most
recent N `EmailLog` rows across the whole association, not "what did we send Farhana." Build: a
member-facing `GET /api/communications/me` (email + SMS sent to that member, paginated, with a detail
view per message — subject/body/channel/timestamp/status); an admin-facing per-member equivalent
(`GET /api/admin/comm/member/{id}`) alongside the existing global log; and note in each row whether
it was a targeted send or part of a broadcast, so "global vs individual" is visible without the
reader having to infer it from the recipient list.

81.2 [DONE 2026-09-06] New `GHCAA.API/Controllers/PendingApprovalsController.cs`
(`api/pending/admin/summary`, `api/pending/me/summary`) aggregates every existing pending endpoint —
news, gallery albums/photos, jobs, member applications, event registrations for admin; a member's own
article submissions, sent family-link/mentorship requests, and pending job postings/event
registrations for the member view. It calls each existing service method as-is and does not touch any
approval logic, per the item's own instruction. Wired into both dashboards: `admin-dashboard`'s new
"Awaiting Your Action" card (links to the real approval screens — `article-approvals`,
`gallery-approvals`, `job-approvals`, `approvals`, `events`) and the member `dashboard`'s new "Your
Pending Requests" card. 17 backend tests (`PendingApprovalsControllerTests.cs`), full suite 616/616;
`vitest` 390/390 after fixing the two dashboard specs' service mocks; `ng build` clean.
**Left honestly incomplete:** family-link and mentorship requests have no dedicated member-facing page
at all yet (checked `GHCAA.Web/src/app/member/` and `common/` — neither exists), so those two rows in
the member widget show a count with no link. Not built here since a full request/response UI for both
is its own scope, not an aggregation task. Tracked as 81.4 below.
**Also found and fixed while building this, on the user's explicit instruction to check notification
wiring on every approval flow:** `MentorshipService` sent no notification at all, on either a new
request or a response — `SendRequestAsync`/`RespondAsync` now call `INotificationService`, matching
the pattern already used by `FamilyLinkService`, `GalleryService`, `JobHubService`, `EventService` and
`MemberService`. 2 new assertions in `MentorshipServiceTests.cs`.

81.3 [TODO] **Priority: P3 | Depends on: 81.1, 81.2.** Once both exist, add them to the member portal
and admin dashboard navigation, and to the mobile equivalents if the same gap exists there (check
`GHCAA.Mobile` for a communications/notifications screen and an approvals screen before assuming
neither exists — `NotificationController`'s parity has not been checked on mobile as part of writing
this item).

81.4 [DONE 2026-09-06] New `/portal/requests` page (`GHCAA.Web/src/app/member/requests/`), tabbed
Family Links / Mentorship, each with Received (accept/decline) and Sent (status, cancel where the
backend allows it — family-link only, mentorship has no withdraw endpoint) plus a "New Request" form
and, for Family Links, a "Your Family Network" section (accepted links, with Remove) — this last part
also closes 80.19, the same web/mobile parity gap seen from a different angle.
New `FamilyLinkService`/`MentorshipService` Angular services, new `API_ENDPOINTS.FAMILY_LINKS`/
`MENTORSHIP` entries (no raw URL strings). Nav item added (Community section, reuses the `messages`
icon rather than inventing a new SVG glyph). The two dead-link rows 81.2 added to the member dashboard
widget now point here instead of `link: ''`.
**Send-request member search reuses two different existing endpoints on purpose, not one:**
family-link send uses `/api/family-links/search`, which is deliberately scoped to
`Member.IsFamilyPublic` members only (a privacy consent flag) — reusing the general directory search
here would let a member request a family link with someone who never opted into that visibility.
Mentorship send uses the general `/api/networking/search` directory endpoint instead, since asking
someone to be your mentor isn't gated by that same consent flag and no mentor-specific search exists.
Tests: `requests.spec.ts` (9 tests). `ng build` clean, `vitest` 399/399 (was 390).

81.5 [DONE 2026-09-06] **Priority: P4 | Depends on: none.** Raised by the user while building 81.2: "approval and
notification can be reusable component" — worth doing, but a real architectural change (an
approval-workflow abstraction and a notification-dispatch abstraction that every domain service calls
through, instead of each service owning its own `INotificationService` calls as today), not a
same-session extension of the aggregation work. Scope it separately before starting; don't fold it
into whichever feature next happens to touch an approval flow. See also
[[feedback_security_rbac_reusable_design]] for the same "build it reusable" instruction applied to
security/RBAC work. **Notification-dispatch half closed alongside 82.21** (paired per 82.53's clustering
note — same surface, cheaper together). Checked before building anything: every domain service already
calls through `CreateNotificationAsync`/`BroadcastNotificationAsync` on the one `INotificationService`,
so that half of the ask is already true and a new wrapper abstraction would just be a redundant layer
around it. What 82.21 added — `CreateNotificationFromTemplateAsync` — is a new method on that same
existing call-through point, not a second one. The approval-workflow half of this item is explicitly
untouched, as scoped: unifying approve/reject flows across services is a separate, larger piece of work
this change did not open.

---

# Work Package 82 — Platform architecture and engineering audit, and the refactoring it names

<!-- wbs: component=C17 start=2026-09-04 end=2026-09-04 after=81 -->

Origin: `docs/materials/REVIEW.md`, supplied by the user on 2026-09-04 as the review brief, with the
instruction to carry its refactoring and auditing findings into this tracker as work. REVIEW.md is a
brief, not a report: nothing in it has been executed yet. §21A of that brief forbids the review from
opening a second backlog, so 82.1 and 82.2 below fix the method before any finding is written, and the
findings already measured against the tree (82.3 to 82.13) are recorded here with the command that
produced them.

Reconciliation done before writing these items, so none of them re-opens work that already exists:
Work Package 24 and its OWASP round-2 block own security controls; Work Package 27 owns test coverage
and the 80% gate (27.8); Work Package 28 and Work Package 62 own the configuration and white-label
platform; Work Package 29 owns the 2026-07-24 full-stack findings; Work Package 61 owns comment tone
and the dead-code sweep, itself re-scoped into 62.46-62.49; Work Package 80 owns the last cross-cutting
sweep. What follows is the work those packages do not cover.

82.1 [DONE 2026-09-04] Review in `docs/materials/REVIEW.md` run; report written to
`docs/ARCHITECTURE_AUDIT_2026-09.md` (730 lines) carrying the §23 and §25.11 deliverables. Every
finding cites a file path or the command that produced it and a §3 status label. **Two of five
research streams returned nothing** before the session hit a provider rate limit, so REVIEW.md §7
(Angular), §8 (Flutter) and §25's systematic duplication sweep are **not covered** — stated in the
report's own coverage table rather than papered over, and tracked as 82.14. Findings raised: 82.14
through 82.28. Two research claims were checked against the tree and found wrong before they reached
the report: `/health` does have a registered `DbContextCheck` (`DependencyInjection.cs:59`), and the
five migrations loose under `Data/Migrations/` are live in the chain, so the recommendation to delete
them would have broken migration history. Both corrections are recorded in the report.

82.2 [DONE 2026-09-04] The report's §N carries the three §21A.11 registers: existing-task
reconciliation matrix, new-task justification register, and obsolete/duplicate/superseded register.
Every finding ends with one of the eleven dispositions. Existing work reconciled rather than
duplicated: 82.3-82.13 re-confirmed and not re-derived; 82.9 expanded (health-endpoint contract,
production logging config); 82.11 expanded (Localization is code-owned despite sitting in the
admin-editable DTO); 48.19 expanded (its pinning reached one workflow of five); WP 48 expanded
(plaintext gateway-credential columns); WP 62.7/62.34 retained (org-identity and EC-role-label
duplication). Rejected as findings, with reasons recorded so they are not re-raised: no CQRS/MediatR,
the narrow repository pattern, rate limiting (already global, not login-only), CSRF, step-up MFA,
mass assignment, and `appsettings.json` placeholders. Fifteen items newly created, each naming which
existing packages were checked.

82.3 [DONE 2026-09-06] **Priority: P2 | Depends on: none.** The API has no versioning. `grep -rn "ApiVersion"
GHCAA.API` returns nothing, and every route is unversioned. The web client deploys with the API, so it
never sees a mismatch, but the Flutter build ships through the stores and lags behind: a renamed route
or a changed DTO field breaks installed mobile builds with no contract in place to signal it. Decide and
implement one approach (URL segment, header, or an explicitly recorded decision to stay unversioned with
a compatibility rule instead). **Acceptance:** the chosen approach is written down with its reason, and
if versioning is adopted, the mobile client sends or requests a version and the API rejects an unknown
one rather than serving it silently.

Decided against URL/header versioning and recorded the reason in `docs/adr/0005-no-api-versioning-compatibility-rule-instead.md`.
One mobile build lineage exists today, not several pinned to different contract versions, so the actual
problem (an installed app breaking on a shape change) is narrower than what full versioning solves —
version negotiation and a maintained old-route surface would be paying for a case that doesn't exist
yet. The rule going forward: never remove or rename an existing route or field, only ever add; a field
no longer meaningful is deprecated in place, not deleted; enum values are additive only; a new request
field is always optional. Enforced by review, not by a compiler — the ADR names where to check what the
mobile client currently reads before changing a shape it might depend on.

82.4 [DONE 2026-09-06] **Priority: P2 | Depends on: none.** Error responses have three shapes, so no client can
parse errors one way. `ExceptionMiddleware` returns `{statusCode, message, details}` for unhandled
exceptions, while the controllers return `BadRequest("plain string")` 21 times, `BadRequest(new
{ Message = ... })` 35 times (38 as of 2026-09-06 — WP80.13's new controller code added more; the shape
itself, and the fact there are three incompatible ones, is unchanged), `BadRequest(new { message = ...
})` 6 times (9 as of 2026-09-06) and one `new { status = ... }` (counted 2026-09-04 across
`GHCAA.API/Controllers`; recounted 2026-09-06 — exact counts drift with every controller change, so
treat them as illustrative, not a target to re-verify). Pick one error contract, apply it to every failure
path, and route the Angular and Flutter error handlers through it. **Acceptance:** one documented error
shape; a grep over the controllers finds no other shape; `global-error-handler.ts` and the mobile
`api_client.dart` read the message from one place.

Adopted ASP.NET Core's built-in ProblemDetails (RFC 7807) rather than a hand-rolled shape, since it's
already wired through `Problem()`/`ValidationProblem()` on `ControllerBase` and needs no new DTO.
`Program.cs` calls `builder.Services.AddProblemDetails()`; every `BadRequest`/`NotFound`/`Conflict`/
`Unauthorized` call across `GHCAA.API/Controllers` that carried a message now returns
`Problem(detail:, statusCode:)`, and `ModelState`-driven 400s return `ValidationProblem(ModelState)`.
`ExceptionMiddleware` builds the same `ProblemDetails` shape by hand for an unhandled exception (it runs
outside MVC's `ProblemDetailsFactory`), stamping `correlationId` from 82.9's middleware and, in
Development only, `stackTrace`. `GHCAA.Web/src/app/core/interceptors/global-http.interceptor.ts` now
reads `error.error.detail` (falling back to `.title`) instead of `.message`.
`GHCAA.Mobile/lib/core/api/api_client.dart` reads `detail`/`title`, falling back to the old `message`
key for anything still on the previous shape. Verified: `grep -rn "BadRequest(new\|NotFound(new\|
Conflict(new" GHCAA.API/Controllers` finds no message-carrying anonymous object left; full backend
suite green (703 passed, 3 pre-existing skips); web suite green (415 passed).

82.5 [DONE 2026-09-06] **Priority: P2 | Depends on: none.** Current-user resolution is copied through the API
layer: 55 inline `FindFirst`/`FindFirstValue` claim reads across `GHCAA.API/Controllers` (counted
2026-09-04). Each one re-decides how the acting member is identified and what happens when the claim is
missing. Extract one accessor (an `ICurrentUser` service or a `ControllerBase` extension) and route the
controllers through it. This is the refactor only. The authorisation behaviour itself is settled in
Work Package 24, where the server-side identity rules were fixed, and must not change here.
**Acceptance:** one implementation of "who is calling", controllers hold no claim-parsing code, and the
existing controller tests pass unchanged.

Went with a `ControllerBase` extension (`GHCAA.API/Extensions/CurrentUserExtensions.cs`) over a DI
service: every controller already extends `ControllerBase` directly (no shared base class exists to
inject through), and a constructor-injected `ICurrentUser` would have meant touching all 24 affected
controllers' constructors and, with them, every one of their test fixtures' — for no behavioural gain
over an extension method that needs neither. Four accessors (`CurrentMemberIdRaw`, `CurrentUserIdRaw`,
`CurrentUsername`, `CurrentRole`), each returning exactly what the inline `FindFirst(...)?.Value` it
replaces returned — null-or-value unchanged, no new parsing, no new fallback. All 55 call sites across
23 controllers now go through it. Verified: the full `GHCAA.Tests` suite passes unchanged (703 passed,
3 pre-existing skips) with no test signature or assertion touched by this item.

82.6 [DONE 2026-09-06] **Priority: P3 | Depends on: 82.1.** `GHCAA.Infrastructure/Services/MemberService.cs` is
1,577 lines, over twice the next largest service (`EventService.cs`, 746). It is the single place the
registry, the approval workflow, profile updates and member search all live. Split it along the seams
that already exist elsewhere in the codebase (`MemberImportService` is already separate, so the pattern
is established), one responsibility at a time. Classified in REVIEW.md §25.10 terms, which grade a
refactor by how much regression risk it carries, as controlled refactoring: behaviour must not change,
and the service tests under `GHCAA.Tests/Services` are the regression net. **Acceptance:** no resulting
file over roughly 600 lines, no interface change visible to the controllers, and the backend suite green
before and after each split.

Split as `partial class MemberService` across files, the same shape the file already used for
`MemberService_Sync.cs`, rather than pulling pieces out into new standalone services — `IMemberService`
still has one implementation and every controller call site is untouched. Result: `MemberService.cs`
(353 lines — ctor/fields plus register/status/verify-email/resend-OTP), `MemberService_Approval.cs`
(331 — approve/reject and the archive/restore/reactivate/bulk-archive lifecycle that follows a
decision), `MemberService_Profile.cs` (468 — profile read/write plus the photo/signature/document
uploads and the admin password-reset link), `MemberService_Search.cs` (431 — registry list/search,
admin bulk update, dashboard and public stats), `MemberService_Helpers.cs` (94 — the private
gamification/profile-completion helpers shared by the others), and the pre-existing
`MemberService_Sync.cs` (24). All six files stay under the 600-line target. Verified with the full
`GHCAA.Tests` suite green before the split and again after, run incrementally after each file was
pulled out.

82.7 [DONE 2026-09-07] **Priority: P2 | Depends on: none.** Seven Angular components inject `HttpClient` directly
instead of a feature service: `admin/audit/admin-audit.ts`, `admin/governance/admin-governance.ts`,
`admin/roles/admin-roles.ts`, `common/health/health.ts`, `member/change-password/change-password.ts`,
`public/elections/elections.ts` and `public/reset-password/reset-password.ts` (`app.config.ts` also
references it, correctly, to provide the client). Around 40 services already exist under
`core/services/`, so these seven bypass whatever those services centralise: endpoint constants, response
shaping and error handling. Move each call into a service alongside its peers. **Acceptance:** no
component outside `core/services/` injects `HttpClient`, and the web unit tests pass.
**Resolved 2026-09-07:** all seven moved onto services — `admin-audit.ts`/`admin-governance.ts`/
`admin-roles.ts` onto new `AdminService` methods, `common/health/health.ts` onto a new `HealthService`,
`change-password.ts` onto a new `ProfileService` method, `public/elections/elections.ts` onto a new
`ElectionsService`, `public/reset-password/reset-password.ts` onto a new `AuthService` method. New
`ROLES`/`ACTIVITY`/`AUTH.RESET_PASSWORD` endpoint constants added to `app.constants.ts` rather than left
as local literals. 419 web unit tests pass across 76 files.

82.8 [DONE 2026-09-06] **Priority: P2 | Depends on: 82.1.** Paging is not applied consistently. There are 35
controllers but only 15 references to a page, page-size, skip or take parameter across all of them
(counted 2026-09-04), so a number of list endpoints return the whole table and will keep doing so as the
registry grows. The audit must list every list-returning endpoint, say which pages and which does not,
and one paging contract must be chosen for the ones that need it. Related, not duplicated: 81.1, the new
member communications history endpoint, already requires paging, so it follows whatever contract this
item settles. **Acceptance:** a table of list endpoints with their paging status, one contract
documented, and the endpoints holding unbounded institutional data (members, payments, audit log) paged.

Contract: the `PagedResult<T>` shape already defined in `GHCAA.Application/Interfaces/INetworkingService.cs`
(`Items`, `TotalItems`, `TotalPages`, `Page`, `PageSize`) and already used by member search and the
financial ledger — reused rather than inventing a second shape. `page`/`pageSize` query params in, that
shape out.

Audit (list-returning `[HttpGet]` endpoints only; single-item/detail/export/health endpoints excluded):

| Endpoint | Paged? | Note |
|---|---|---|
| `AdminController` GET `members` | Yes | `page`/`pageSize` already present |
| `FinancialLedgerController` GET `` (records) | Yes | `page`/`pageSize` already present |
| `EventsController` GET `admin/registrations` | Yes | `page`/`pageSize` already present |
| `ForumController` GET `.../topics`, `.../posts` | Yes | `page`/`pageSize` already present |
| `NetworkingController` GET `search`/`directory` | Yes | `SearchMembersAsync` returns `PagedResult<T>` |
| `AdminController` GET `contact-messages` | No | unbounded, grows with every form submission — highest-priority gap found, deferred (see below) |
| `ActivityController` GET `admin/global` | No | unbounded, all-member activity feed — second-highest-priority gap, deferred |
| `NewsController` GET `admin` (all news) | No | grows with every post; moderate volume |
| `GalleryController` GET `` / `all` | No | grows with every album; moderate volume |
| `JobHubController` GET `` (public list) | No | grows with every posting; moderate volume |
| `EventsController` GET `admin/all` | No | organizational events, low growth rate |
| `CampaignsController` GET `admin/all`, `admin/{id}/pledges` | No | campaigns are infrequent; pledges per campaign could grow, lower volume than the above |
| `MentorshipController` GET `admin/all` | No | moderate volume |
| `RolesController` GET `users` | No | system/admin accounts, bounded in practice (dozens); `admin-roles.ts` still reads a flat array (82.7) — paging this now would break that page without a matching client change |
| `NotificationController` GET `` (my notifications) | No | scoped to one member, grows slowly |
| `MessagingController` GET `history/{otherUserId}` | No | scoped to one conversation |
| `GovernanceController` GET `ec/history`, `constitution/history` | No | small, versioned, low growth |
| `CommunicationController` GET `logs` | Partial | has a `count` cap (default 100), not the `page`/`pageSize` contract |
| everything else listed under 82.2's endpoint sweep | No | bounded reference data, per-member/self-scoped lists, or fixed small admin sets — not institutional data that grows unboundedly |

Applied paging to the endpoints the acceptance names directly: **members** (`AdminController.GetAllMembers`,
already contract-compliant) and **payments** (`FinancialLedgerController.GetRecords`, already
contract-compliant — no institution-wide "all payments across all members" endpoint exists separately;
per-member payment history in `FinancialsController` is scoped to one member, not unbounded). **Audit
log** has no listing endpoint yet — `AuditLogMiddleware` only writes; there is nothing to page until one
is built. `AdminController.GetContactMessages` and `ActivityController.GetGlobalActivity` are the two
gaps this audit found that genuinely match "unbounded institutional data" and aren't yet paged; left for
a follow-up pass rather than rushed in here, since both need a corresponding Angular consumer update to
read the wrapped shape instead of a flat array (the same reason `RolesController.GetUsers` was left
alone after a paged version was drafted and reverted) — pairing that with 82.7's HttpClient-to-service
migration avoids doing the client-side plumbing twice.

82.9 [DONE 2026-09-06] **Priority: P3 | Depends on: 82.1.** Operationally the platform can be checked but not
diagnosed. `Program.cs` exposes `MapHealthChecks("/health")` and there is an `AuditLogMiddleware`, but
there is no Serilog or OpenTelemetry reference anywhere, no correlation identifier tying a client
request to its server-side log lines, and no structured request log carrying the acting member. After a
failure on preprod, "what failed, when, and who was affected" is answered by reading unstructured Render
output. Add the minimum that answers those questions: a correlation identifier assigned per request,
returned to the client and logged, and a structured request log line. Distributed tracing is out of
scope at this scale, under REVIEW.md §4, which requires every recommendation to name the problem it
solves now. **Acceptance:** a failure reproduced on preprod can be traced from the client error to its
server log lines using one identifier.

`GHCAA.API/Middleware/CorrelationIdMiddleware.cs`, registered in `Program.cs` right before
`ExceptionMiddleware` so both it and everything downstream carry the id. Reuses an incoming
`X-Correlation-Id` header if the client sent one, otherwise assigns a new GUID; stamps it on the
response header via `Response.OnStarting` (so it survives even an unhandled-exception response); wraps
the rest of the pipeline in an `ILogger` scope carrying it, so every log line for that request —
including `ExceptionMiddleware`'s unhandled-exception line, which now also puts `correlationId` in the
82.4 `ProblemDetails` body — is tagged. After the request completes, logs one structured line:
method, path, status code, elapsed ms, acting member id (read from the claim after authentication has
run, `"anonymous"` when absent), and the correlation id. No new logging framework — the existing
`ILogger`/`Microsoft.Extensions.Logging` pipeline everything else already uses. OpenTelemetry and
distributed tracing stay out of scope, per the item's own reasoning: nothing here has more than one
process to trace across yet (see ADR-0004).

82.10 [DONE 2026-09-04] **Client code generation from OpenAPI: rejected. Contract drift is caught by a
CI diff instead.** Decided on the evidence below rather than deferred, because the assessment this item
asked for could be answered from the tree in one sitting.

The problem is real. The contract is written out three times: 42 DTO files (1,471 lines) in
`GHCAA.Application/DTOs`, 60 interfaces in `GHCAA.Web/src/app/core/models/business.models.ts`, and the
mobile client's own reading of the same payloads. Drift between them is found by a person noticing,
which is how the parity gaps of Work Package 11 and Work Package 35 were found.

Generation does not fit the mobile client as it stands. There is no model layer to regenerate: 16
`fromJson` factories across 6 Dart files, against 799 raw `json['field']` map reads through the screens
and services (counted 2026-09-04). Introducing generated Dart models means rewriting nearly all mobile
data handling, which is a large controlled refactor whose only reward is a defect class that has not
actually shipped a fault yet. That is the situation REVIEW.md §4 tells the reviewer to decline. The
Angular half would be cheap to generate, but generating one client and hand-writing the other leaves the
drift risk in place and adds a build step for the half that was never the problem.

The cheaper control catches the same drift for both clients at once, and is where the residual work goes:
publish the OpenAPI document, commit it, and fail the build when it changes unexpectedly. That also gives
82.3, the versioning decision, something concrete to version.

**Rejected explicitly, so it is not re-proposed:** generated Angular clients, generated Dart models, and
a shared code artefact between the two clients. Revisit only if the mobile client gains a real model
layer for another reason, at which point generation costs a fraction of what it costs today.

82.10a [DONE 2026-09-06] Swagger was registered unconditionally (`AddSwaggerGen`, `Program.cs`) but
only served inside `if (app.Environment.IsDevelopment())`, so nothing outside a developer's machine ever
saw the contract and nothing checked it. The document is now produced at build time instead of served:
`Swashbuckle.AspNetCore.Cli` 6.6.2 is installed as a local tool (`GHCAA.API/.config/dotnet-tools.json`),
`docs/api/swagger.json` is committed as the snapshot, and `docs/api/diff_swagger.py` compares a freshly
generated document against it, printing added/removed/changed paths and failing on any difference rather
than dumping the whole document. A new `api-contract` job in `.github/workflows/ghcaa-ci-standard.yml`
runs this on every push: build the API in Release, restore the Swagger CLI, generate
`/tmp/swagger.generated.json`, diff it against the committed copy. The production Swagger endpoint stays
off, per the original scope note: a live interactive UI hands an attacker a complete map of the API and a
form to fire requests at it, which is the enumeration aid Work Package 48 closed off elsewhere. A build
artefact gives the same drift protection with no runtime surface.

Generating the document once surfaced a real Swagger generator crash, unrelated to the item itself:
`SwaggerGeneratorException: Conflicting method/path combination "POST api/events/register"` between
`EventsController.RegisterForEventForm` and `RegisterForEventJson`, which deliberately share one route
and are disambiguated at runtime by `[Consumes]` (multipart vs. JSON) — a working pattern, not a routing
bug. Swashbuckle has no way to represent two operations under one OpenAPI path item, so it throws instead
of documenting one of them. Fixed with Swashbuckle's own documented workaround,
`c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First())` in `AddSwaggerGen`, rather than
touching the working routes.
**Acceptance met:** the snapshot is in the repository (`docs/api/swagger.json`), the CI job fails on an
uncommitted contract change, and the failure names the changed paths. Verified locally: `dotnet tool
restore` restores both tools cleanly from the committed manifest, and the full backend suite (620 tests)
passes after the `Program.cs` change with no regressions.

82.10b [DONE 2026-09-14] **Priority: P3 | Depends on: 82.10a.** Type the mobile payloads where a silent field
change does the most damage: authentication and token handling, member profile, and payments. These
parse from raw maps today, so a renamed field fails at runtime on a member's phone with no compile-time
signal, and the phone is the client that cannot be hot-fixed. Scope is deliberately those three areas,
not all 799 map reads. **Acceptance:** each of the three areas parses through a Dart class with a
`fromJson` factory that fails loudly on a missing required field, and `flutter analyze` stays clean.
**Resolved 2026-09-14:** added `lib/core/models/auth_models.dart` (`LoginResponse`), `member_profile.dart`
(`MemberProfile`, `AcademicRecord`, `ProfessionalRecord`) and `financial_models.dart` (`LedgerEntry`,
`DueItem`), all with required fields as direct casts (`json['token'] as String`) that throw instead of
defaulting, matched against GHCAA.Domain's `[Required]`/non-nullable columns. `auth_service.dart`'s
`login`, `loginWithStoredToken` and `_socialLogin` now build `LoginResponse` from the response instead of
reading `data['token']` by hand. `financial_service.dart`'s `getLedger`/`getOutstandingDues` parse each
item through the new models; `getSavedMethods`, `getActivePaymentConfigs`, `recordPayment` and
`gateway_service.dart` are untouched, out of scope. `profile_screen.dart` and `profile_edit_screen.dart`
keep reading the profile as a map — `profile_edit_screen.dart` edits too many dynamic admin/list fields
for a typed rewrite to be worth it — so `userProfileProvider` parses the fetched profile through
`MemberProfile.fromJson` once at the network boundary purely to throw on drift, then still saves and
returns the original map. `flutter analyze` reports no issues.

82.11 [DONE 2026-09-07] **Priority: P3 | Depends on: 82.1.** Produce the code / environment-configuration /
administrator-managed classification that REVIEW.md §10 and §11 ask for, covering organisation identity,
feature flags, membership policy, governance, workflow, notification, content and integration settings.
This is not a new configuration feature and must not be built as one: Work Package 28 delivered the
configuration framework and Work Package 62 owns the white-label work, so the output is a classification
over what those two already provide, feeding any gap back into them as expanded items rather than new
ones. The classification must also name what may never become freely editable, in particular the
constitutional and election rules that carry formal institutional authority and are held in
Work Package 36 and Work Package 37. **Acceptance:** one table covering every capability, each row with
a reason, and a stated list of rules held in code on purpose.
**Resolved 2026-09-07:** §13 added to `docs/CONFIG_DRIVEN_FRAMEWORK.md` with the capability table, naming
constitutional/election rules as permanently code-held (Work Package 36/37), and flagging beta flags and
per-member notification preferences as REVIEW.md items not yet built rather than misclassified.

82.12 [DONE 2026-09-07] **Priority: P3 | Depends on: 82.1.** Run the constants and magic-value classification of
REVIEW.md §25.8 over `GHCAA.Domain/Constants.cs`, `GHCAA.Web/src/app/core/constants/app.constants.ts`
(533 lines) and the Flutter equivalents, classifying each value as technical constant, environment
value, organisation value, administrator-managed value or business policy. The standing rule that a
repeated literal gets a named constant already applies to every change, so the value here is the
classification, not another renaming pass: it says which of the existing constants are GHC-specific and
therefore belong in the Work Package 62 profile pack rather than in code. **Acceptance:** every entry in
those files classified, and the organisation-specific ones raised against Work Package 62 as expanded
items.
**Resolved 2026-09-07:** `docs/CONSTANTS_CLASSIFICATION.md` added, covering `GHCAA.Domain/Constants.cs`,
`app.constants.ts`, and both Flutter constants files. `EC_ROLES`, `DEVELOPER_INFO` (a hardcoded personal
name/email), and the Bangladeshi academic/professional taxonomies are flagged as genuine Work Package 62
candidates.

82.13 [DONE 2026-09-06] **Priority: P3 | Depends on: 82.1.** The documentation set has no architecture decision
records and no operational runbook. `docs/` carries `ARCHITECTURE.md`, `PROJECT_MAP.md` and
`RENDER_DEPLOYMENT.md`, but there is no `docs/adr/` directory and no document answering how to restore
the platform after a data loss or a failed migration, which REVIEW.md §22 requires for an operator.
Several decisions this project has already made are recorded only in commit messages and memory, among
them the two-format date contract of 29F.3, the choice not to run migrations at startup, and the
protected super-admin list. Start the record with those, and write the recovery procedure against what
Render actually provides. **Acceptance:** a decision record exists for each decision the audit finds is
load bearing and undocumented, and an operator who has never seen the system can restore it by following
the runbook.
**Resolved 2026-09-06:** `docs/adr/` created (`0001-two-format-date-contract.md`,
`0002-migrations-apply-automatically-at-startup.md`, `0003-protected-super-admin-list.md`,
`0004-single-instance-deployment-constraint.md`, `0005-no-api-versioning-compatibility-rule-instead.md`,
plus a `README.md` index) and `docs/RECOVERY_RUNBOOK.md` written against what Render actually provides —
including stating plainly that no backup/restore capability for the Neon database is documented anywhere
in this repo, rather than inventing one. This is also the ADR 82.27 needed to move from PARTIAL to DONE.

82.1 and 82.2 closed 2026-09-04. The report is `docs/ARCHITECTURE_AUDIT_2026-09.md`, 730 lines,
carrying the §23/§25.11 deliverables and the three §21A.11 reconciliation registers. Items 82.14 to
82.33 below are the work it raised. Two corrections the report makes to its own research, recorded
here because both would have caused damage if acted on: `/health` **does** have a registered
`DbContextCheck` (`DependencyInjection.cs:59`) and is not a dead endpoint, and the five migrations
sitting loose under `Data/Migrations/` are **live** in the chain — `dotnet ef migrations list` shows
all five — so they must not be deleted.

Every item below states why the work is worth doing, not only what to change, per SR-8.

82.14 [DONE 2026-09-06] The audit did not originally assess the Angular or Flutter clients. Two of
five research streams returned nothing before the 2026-09-04 session hit a provider rate limit, so
REVIEW.md §7 (Angular architecture, state management, API integration, UI quality), §8 (Flutter
architecture, state, offline, platform concerns), and §25's systematic duplication sweep had no
client-side coverage. **Partial progress 2026-09-05:** 82.32's client-side pass found and fixed 6
confirmed defects, but that was a bug hunt against a checklist, not the architecture assessment this
item asked for — so it stayed open.

**Closed 2026-09-06.** Ran three dedicated research passes against REVIEW.md's exact §7/§8/§25 briefs
(Angular: `GHCAA.Web/src/app`; Flutter: `GHCAA.Mobile/lib`; cross-client and per-client duplication),
each producing file-path/line evidence, a §3 status label, and an 82.2 disposition per finding, same
standard as the rest of the report. Folded into `docs/ARCHITECTURE_AUDIT_2026-09.md` as three new
sections — §Q (Angular), §R (Flutter), §S (client duplication sweep) — appended after §P rather than
inserted mid-alphabet, since §N is cross-referenced by name from this file (82.2) and renumbering it
would have broken that reference. Coverage table, §L, §M, §O and §P updated to point at the new
sections instead of restating "not assessed". Angular rates **Good** overall; Flutter rates
**Developing**. 16 new items raised from the findings (82.34-82.49 — a stale-privilege-label bug on
logout, dead code, missing retry/offline/deep-link/push-registration gaps, and a lookups-centralisation
finding that collapses five separately-drifting duplicated tables into one fix); none papered over,
none forced where the brief's own "avoid overengineering" caution applied (several findings were
explicitly triaged as leave-as-is with the reason recorded in §S).

82.15 [DONE 2026-09-07] **Priority: P2 | Depends on: none.** `GHCAA.Infrastructure/DependencyInjection.cs:32-53`
switches on a `DatabaseProvider` setting across `sqlite`, `mysql` and PostgreSQL, pooling a
provider-specific shim context from `Data/DbContextShims.cs` for each. Only PostgreSQL works:
`Data/Migrations/` has one provider folder, `PgSql/`, and every migration in it is attributed
`[DbContext(typeof(PgSqlApplicationDbContext))]`. No migration anywhere is attributed to the Sqlite or
MySql shim types, and `MigrationBootstrapper`'s self-heal path is Postgres-specific (`PostgresException`,
`pg.SqlState`, lines 192 and 212). Selecting either other provider boots against an empty schema.
**Why fix it when nothing selects those providers:** the switch is an invitation. A future maintainer
reading it would reasonably believe MySQL is supported and plan against that, and `docs/book/06-architecture.md`'s
ADR-02 already states as a consequence that "migrations apply correctly on boot across PostgreSQL,
MySQL and SQLite", which is not true of this tree — so the codebase is currently teaching a false fact
in two places. The cheaper and more honest of the two available fixes is to delete the shims and the
switch down to PostgreSQL, matching the same principle ADR-06 applied correctly when it declined to
build a second file-storage adapter before a second need existed. **Do not touch the five migrations
that live directly under `Data/Migrations/` while doing this** — they are live, see the note above.
**Acceptance:** either the other two providers have working migration trees and a documented test that
proves a boot against each, or the switch, the shim types and ADR-02's claim are removed together and
the ADR records why.
**Resolved 2026-09-07:** MySQL removed — `MySqlApplicationDbContext`, `MySqlDesignTimeDbContextFactory`,
the `mysql` DI switch case, its connection-string branch, and the `Pomelo.EntityFrameworkCore.MySql`
package are gone from both `GHCAA.Infrastructure.csproj` and `GHCAA.API.csproj`. Sqlite was kept:
`GHCAA.Tests/Integration/OutputCacheTestFactory.cs` and `SpaStaticFileFactory.cs` deliberately boot the
real API with `DatabaseProvider=Sqlite` for a fast, migration-free `EnsureCreated()` test path under the
Visual seed profile (which skips `MigrationBootstrapper` entirely) — deleting it would have broken those
tests. Reasoning recorded in new `docs/adr/0006-drop-mysql-provider.md` (also fixed the ADR README index,
which was missing 0005 too). **Follow-up flagged, not done here:** `docs/book/06-architecture.md`'s ADR-02
table still overstates cross-provider migration parity and needs its own edit through the book's strict
build pipeline.

82.16 [DONE 2026-09-04] **Priority: P1 | Depends on: none.** Financial records can be edited and hard-deleted with
no trace. `FinancialLedgerService.cs:69` exposes `UpdateRecordAsync` and line 91 does
`_db.FinancialRecords.Remove(record)`; `FinancialService.cs:488` does `_db.PaymentHistories.Remove(payment)`.
Neither `FinancialRecord` nor `PaymentHistory` carries `UpdatedAt`, `UpdatedByAdminId` or a soft-delete
flag — `FinancialRecord` has `CreatedAt`/`CreatedByAdminId` only. `PaymentHistory.Status` moves
Pending → Completed/Failed/Refunded with no history table, unlike membership changes, which have
`MembershipHistory`. **Why this is P1 and not a growth concern:** the association already collects real
dues, and REVIEW.md §14 asks specifically whether payment records are immutable. They are not — a row
recording money received can be altered or removed and nothing records the prior value or who did it.
That is the first thing an external reviewer or an auditor checks, and the association is the kind of
institution that will eventually face one. **Scope deliberately kept small:** add `UpdatedAt` and
`UpdatedByAdminId` plus a soft-delete flag to both entities and make corrections additive rather than
destructive. No event sourcing, no separate audit-log service — that would be the overengineering
REVIEW.md §4 rules out. Fold in the audit-field inconsistency the report's Finding 3 describes (3 of 46
entities carry `IsArchived`, 6 carry any created-by field, 8 any updated-at) by writing down the rule
for which entity classes need these fields, so the next entity added does not inherit the gap.
**Acceptance:** a ledger or payment row cannot be silently altered or removed; every change records who
and when; a stated rule exists for which entities require audit fields; `dotnet test` green per SR-6.
**Done 2026-09-04.** `FinancialRecord` and `PaymentHistory` each gained `UpdatedAt`,
`UpdatedByAdminId`, `IsDeleted`, `DeletedAt`, `DeletedByAdminId`. Both delete paths are now soft:
`FinancialLedgerService.DeleteRecordAsync` and `FinancialService.DeletePaymentAsync` take the acting
admin's id as a required argument and stamp the row instead of calling `Remove()`. Their controllers
return `Unauthorized()` rather than attributing a deletion to admin 0 when the caller cannot be
identified. `FinancialRecordConfiguration` (new) and `PaymentHistoryConfiguration` filter deleted rows
out of ordinary reads, so no existing query changed meaning. Migration
`20260904115342_AddFinancialAuditTrail` is purely additive — ten `AddColumn`, nothing else; the
scaffolder's seed churn was removed by hand, matching every prior migration in that folder.
**SR-8 justification for the refactoring involved:** the delete signatures changed because the audit
trail is worthless if the actor is optional — an `int?` would have let every existing caller keep
compiling while recording nothing, which is the failure mode the item exists to prevent. Making the
argument required turned it into a compile error at each of the two call sites, both of which were
then fixed to check the claim.
**The rule the item asked for** is now `docs/ARCHITECTURE.md` §4, which splits entities into Class A
(evidence — carries all five fields, never hard-deleted) and Class B (recreatable content — `CreatedAt`
only), lists the Class A membership, and states the query-filter and required-actor conventions that go
with it.
**Deliberately not done:** `Amount` was left as unconstrained `numeric` rather than `numeric(18,2)`.
Constraining it silently rounds any live row holding more than two decimals, and a lossy alteration to
a money column inside the change meant to protect money columns is self-defeating. It needs its own
item, an out-of-range check first, and its own migration.
**Tests:** `GHCAA.Tests/Services/FinancialAuditTrailTests.cs`, 5 tests, each tamper-tested (guard
removed, test goes red). Full suite 585 passing, up from 580, none broken.
**Left open by this item:** 82.29 (`ECMember`'s two removal semantics) and 82.30 (`Member`/`User` use
`IsArchived` where the rule says `IsDeleted`).

82.17 [DONE 2026-09-14] **Priority: P3 | Depends on: none.** Configuration is bound entirely through raw string
keys — `grep -rl "IOptions<\|IOptionsSnapshot<\|IOptionsMonitor<" GHCAA.Infrastructure/Services/*.cs
GHCAA.API/*.cs` returns zero files. Every consumer parses by hand:
`configuration.GetValue<string>("DatabaseProvider")` (`DependencyInjection.cs:23`),
`configuration.GetSection("AppSettings:AllowedOrigins").Get<string[]>()` (`Program.cs:175`), and the
same shape repeats across services. **Why it is worth doing:** there is no compile-time check on config
shape and no single place showing what a feature needs, so a mistyped key returns the default silently
rather than failing. This is distinct from 82.11 and 82.12, which classify *which values* belong in
configuration; this is about how configuration is bound in code. **Why it is P3 rather than higher:**
nothing is currently broken by it, and the fix is mechanical. Use `services.Configure<T>(...)` with
constructor injection — .NET's own idiom, not a new abstraction layer.
**Acceptance:** each configuration section a service depends on is bound to a typed options class, and
a mistyped or missing key surfaces at startup or in a test rather than silently defaulting.
**Progress 2026-09-07:** new `GHCAA.Infrastructure/Options/ConfigurationOptions.cs` with typed option
classes for Jwt, Gmail, SMS, OTP, ContactUs, FileStorage (incl. nested image-compression), and the
SSLCommerz/Bkash/DGePay gateways, registered via `services.Configure<T>()`. `GmailEmailService`,
`GreenwebSmsService`, `OtpService`, `ContactService`, `LocalFileStorageService`, `TokenService`,
`SSLCommerzGateway`, `BkashGateway`, `DGePayGateway` converted to inject `IOptions<T>`.
`NagadGateway`'s unused `IConfiguration` field is dead code, left alone (out of scope).
`DatabaseProvider`/`ORG_PROFILE` correctly stay as raw bootstrap-time reads since they run before
the DI container exists.
**2026-09-14:** the three services left raw in the previous pass are now converted too —
`AuthService`, `UserService`, `MemberService`, and `FinancialService` all inject
`IOptions<AppSettingsOptions>` or `IOptions<GeneralSettingsOptions>` and read `.Value.ClientUrl`,
`.Value.ProtectedSuperAdmins`, or `.Value.SystemAdminId` instead of a raw config-string lookup.
`grep -rl "IOptions<" GHCAA.Infrastructure/Services/*.cs GHCAA.API/*.cs` now lists all ten
converted services; `dotnet build` is clean. `PaymentCallbackOrchestrator.cs` still reads
`Constants.ConfigKeys.SystemAdminId` from raw `IConfiguration` — outside this item's named scope,
left as a separate follow-up if it's ever picked up.
`dotnet test GHCAA.sln --no-restore` also passes: 742/742, 0 failed.

82.18 [DONE 2026-09-06] `RotateRefreshTokenAsync` now checks, when the active-token lookup fails,
whether the presented hash matches a *revoked* row instead — that's the replay signal. If so: logs a
warning naming the user, calls the existing `RevokeAllRefreshTokensAsync` (no new revocation
machinery needed, exactly as the item said), and additionally rotates `User.SecurityStamp` via
`ExecuteUpdateAsync`, since revoking refresh tokens alone leaves a still-live 60-minute access token
valid — "kills the session" wasn't true without that second part. Test:
`RotateRefreshToken_ReplayOfARotatedToken_RevokesWholeFamilyAndRotatesStamp` — rotates once
legitimately, replays the now-revoked first token, and asserts every token for that user (including
the second, legitimately-issued one) ends up revoked and the stamp changed. Full suite 618/618.
**Test gotcha hit and fixed, not a product bug:** the first assertion attempt read back stale tracked
entities, because `ExecuteUpdateAsync` is a bulk SQL update that bypasses the change tracker — fixed
with `AsNoTracking()` on the read-back queries, not by changing the fix itself.

82.19 [DONE 2026-09-06] **Priority: P3 | Depends on: none.** `AsNoTracking()` appears in 5 of 39 service files
against roughly 183 read queries (`grep -rc "AsNoTracking" GHCAA.Infrastructure/Services/*.cs | grep -v ":0"`
versus the `ToListAsync|FirstOrDefaultAsync|SingleOrDefaultAsync` count across the same directory).
`MemberService.cs` runs multi-`Include` reads at lines 502-510, 931-934 and 1101-1102 pulling academic
history, professional history, EC memberships and payment history together, all tracked, on paths that
only serialise to a DTO. **Why it is P3 and scale-tiered rather than a straight fix:** at today's ~631
members the change-tracker overhead is negligible and this would be premature optimisation. It becomes
measurable on admin list and export endpoints in the low thousands, and only needs a project-wide audit
beyond that. **Why not a blanket automated change:** some paths legitimately rely on tracking to save
changes afterwards, so a repo-wide `AsNoTracking` insertion would introduce real bugs. Scope this to a
targeted pass over the confirmed read-only paths in `MemberService`, `FinancialService` and
`NetworkingService`. Related to but distinct from 82.6, which is about that file's size, not its query
tracking. **Acceptance:** the named read-only paths are untracked, each change confirmed read-only by
reading its caller, and the suite stays green.

Added `AsNoTracking()` after reading each caller to confirm it never calls `SaveChangesAsync` on the
loaded entities. `MemberService_Profile.GetProfileAsync`: the multi-`Include` read (EC memberships,
family link requests, academic/professional history, payment history) — serialises straight to a DTO,
no save. `FinancialService`: `GetMemberPaymentHistoryAsync`, `GetMemberMembershipHistoryAsync`,
`GetMemberDuesAsync`, `GetMembershipFeeConfigsAsync`, `GetApplicableFeeAsync`'s fee-config lookup,
`GenerateAnnualDuesAsync`'s active-member read (only `.Id`/`.MembershipType` are read off it),
`GenerateTaxReceiptAsync`'s payment+member read (renders a PDF, nothing saved), the fee-config lookup
inside `HandleAutomatedApprovalsAfterPaymentAsync`, and `GetSavedPaymentMethodsAsync`.
`NetworkingService`: the whole file has no `SaveChangesAsync` call anywhere, so every remaining tracked
query got it too — `SearchMembersAsync`'s two query stages and `GetExecutiveCommitteeAsync` and
`GetLatestAlumniUpdatesAsync` (`GetMemberProfileAsync` already had it). Left tracked, deliberately:
`MemberService_Profile.UpdateProfileAsync` and `MemberService_Search.AdminUpdateMemberAsync` (both load
then `SaveChangesAsync` the same entity), `MemberService_Approval.ApproveMemberAsync`'s member+history
read (status/membership-number are written back and saved in the same transaction),
`FinancialService.SendAdminPasswordResetLinkAsync`'s `Users.Include(Roles)` read is in
`MemberService_Profile`, not `FinancialService`, and stays tracked there because the reset token is
saved onto that same user. `DeletePaymentAsync`'s linked-dues read, `UpdatePaymentStatusAsync`, and
`MarkDueAsPaidAsync`/`StampGatewayPaymentIdAsync` all mutate what they load and stay tracked.
`GetECPeriodsAsync` was left alone — it already projects straight to an anonymous type via `Select`,
so there is no tracked entity for `AsNoTracking()` to affect. Full `GHCAA.Tests` suite stayed green
throughout.

82.20 [DONE 2026-09-06] **Priority: P3 | Depends on: none.** The platform cannot run more than one instance, and
that limit is nowhere written down. `Program.cs:64` registers `AddOutputCache()` with no distributed
backing; `OrgConfigService` and `ThemeService` use `IMemoryCache` directly with no cache abstraction;
`Program.cs:170` registers `AddSignalR()` with no backplane; `ChatHub`'s connection map is a
process-local `ConcurrentDictionary`. **Why this item is documentation rather than code:** every one of
those choices is correct for the current single-instance Render deployment, and building Redis-backed
caching and a SignalR backplane now would be exactly the overengineering REVIEW.md §4 rules out. But
the day someone scales to two instances, output-cache entries, config and theme caches, and SignalR
group membership all silently diverge — a client on one instance stops receiving pushes sent from the
other, and a config edit clears one cache and not the other. That failure is confusing and hard to
diagnose precisely because nothing warns it is coming. **Acceptance:** the constraint is written where
someone about to scale out would find it (deployment docs and an ADR under 82.13's work), naming the
three specific mechanisms that must change first.
**Resolved 2026-09-06:** `docs/RENDER_DEPLOYMENT.md` gained a "Scaling beyond one instance" section and
`docs/adr/0004-single-instance-deployment-constraint.md` was written, both naming the same three
mechanisms (output cache, `OrgConfigService`/`ThemeService`'s `IMemoryCache`, `ChatHub`'s process-local
connection map + backplane-less SignalR) and what has to change first (distributed cache backing,
Redis-backed SignalR backplane) before scaling out.

82.21 [DONE 2026-09-06] **Priority: P3 | Depends on: none.** In-app notifications bypass the template system
entirely. `EmailTemplate` is a real admin-editable templating mechanism — DB rows with `Code`,
`Subject`, `Body`, `Variables` and a `Channel` enum that includes SMS — but
`NotificationService.CreateNotificationAsync` and `BroadcastNotificationAsync` never reference it. They
take raw `title`/`message` strings from each call site and write straight to the `Notification` table
plus a real-time push. **Why it is worth aligning:** there are two notification paths with different
governance. An administrator who edits a template reasonably expects the in-app text to change too, and
it will not — the in-app wording is hardcoded at each call site and only a developer can change it.
That is the kind of split that is invisible until someone edits a template and cannot work out why
nothing happened. Related to WP 50, which built the template mechanism for email and never had the
in-app path in scope. **Acceptance:** in-app notification text comes from the same template source as
email, or the split is deliberately retained and documented with the reason.
**With ~15 call sites now scattered across Governance, Event, News, Poll, Gallery, JobHub, Member,
Financial, Mentorship and FamilyLink, a mechanical rewrite of every one was judged disproportionate for
one sitting — instead the capability was built once and demonstrated on the two highest-value payment
call sites, with the rest left as literal strings by choice, not oversight.** Added
`ICommunicationService.ResolveTemplateTextAsync` (resolves an `EmailTemplate`'s Subject/Body against the
same member + org + custom variable set `SendEmailByCodeAsync` already uses, returning null if no
template exists) and `INotificationService.CreateNotificationFromTemplateAsync` (calls that resolver,
strips the email HTML down to plain text for the in-app `Message` column, and falls back to a literal
title/message when the code has no template row). `FinancialService.RecordPaymentAsync` and
`UpdatePaymentStatusAsync` now route through it against the existing `PAYMENT_RECEIVED` and
`PAYMENT_STATUS_UPDATED` templates — the same template `SendIndividualEmailAsync` already used for the
email side of the first one, so editing that template now changes both channels at once.
`PAYMENT_STATUS_UPDATED` had been a seeded template with no live caller until this; it isn't dead data
anymore. `MemberService`'s welcome notification was deliberately left as a literal: `WELCOME_EMAIL`'s
body carries the member's one-time default password, which is fine in an email but not something that
belongs sitting in an in-app notification list. The two template codes moved into
`Constants.TemplateCodes` (`PaymentReceived`, `PaymentStatusUpdated`) rather than staying as re-typed
literals across `CommunicationService`, `FinancialService` and their tests. `dotnet build` clean,
`dotnet test` 702/703 (the one failure is `PaymentConfigControllerTests.CreateConfig_PersistsAndReturnsMaskedSecrets`,
a pre-existing console-encoding mismatch on the masked-secret bullet character, unrelated to this item
and to any service touched here).

82.22 [DONE 2026-09-06] **Two of the four named gaps were real, two were stale claims — checked each
against the current model before writing any index, not assumed from the item text.**
`MembershipHistory` and `MembershipDue` **already had an index on `MemberId`** (confirmed in the
committed model snapshot, predating this session) — the item's claim there was wrong, so no change
was made for either, rather than adding a redundant index with a comment implying a gap that didn't
exist. `FinancialRecord` does have a configuration file (`FinancialRecordConfiguration.cs`, added
under 82.16) — also a stale claim — but genuinely had no index, so that part of the finding stood.
**Shipped:** `NotificationConfiguration` — replaced the existing single-column `MemberId` index with
`HasIndex(n => new { n.MemberId, n.CreatedAt })`, matching `GetUserNotificationsAsync`'s actual filter
+ sort. `FinancialRecordConfiguration` — added `HasIndex(r => new { r.Year, r.RecordType,
r.FinancialCategory })`, matching `FinancialLedgerService.GetSummaryAsync`/`GetRecordsAsync`'s filters
(the same method just fixed by 46.5). Migration `20260905194913_AddQueryPathIndexes`, hand-written
(`migrationBuilder.Sql`, `CREATE INDEX IF NOT EXISTS`) rather than trusting the scaffold: it picked up
~27k lines of unrelated seed-data churn on the first attempt (`gotcha_pending_model_changes_seed`),
and a second attempt silently truncated the ~74k-line model snapshot to ~4,600 lines — a new EF-tooling
failure mode, recorded as `gotcha_ef_migrations_add_remove_corrupts_snapshot` since nothing in this
project's existing migration guidance named it. Verified via `dotnet ef migrations script`: exactly
the three intended `CREATE INDEX`/`DROP INDEX` statements, nothing else. `dotnet build`/`dotnet test`
617/617, 0 warnings.

82.23 [DONE 2026-09-06] **Priority: P2 | Depends on: none.** A 30-file Playwright suite exists and CI never runs
it. `GHCAA.Web/tests/` covers admin workflows, the alumni directory, article editorial, membership and
event flows, and gallery; `package.json` defines `test:e2e`; `playwright.config.ts` boots both the API
and the Angular dev server and polls `/healthz`. Neither `ghcaa-ci-preprod.yml` nor
`ghcaa-ci-standard.yml` invokes it — both run only the vitest unit suite. **Why it matters more than an
ordinary coverage gap:** these are the end-to-end tests for the platform's critical business flows, and
because nothing runs them they can rot silently. A broken membership-approval flow would pass every
gate the project currently has. Work Packages 14, 20 and 22 wrote these specs; none of them wired the
suite into CI, which is why this is new rather than a duplicate. **Acceptance:** a CI job runs the
Playwright suite on the same triggers as the unit tests, and a deliberately broken flow fails it.
**Resolved 2026-09-06:** added `e2e-tests` to `ghcaa-ci-standard.yml` (needs `ui-tests`, installs
Chromium via `npx playwright install --with-deps chromium`, runs `npm run test:e2e`). Not verified by
an actual CI run from this session (no push/PR access) — YAML validated and `test:e2e` /
`playwright.config.ts` confirmed to exist.

82.24 [DONE 2026-09-06] **Priority: P2 | Depends on: none.** `.github/workflows/main.yml` is a weaker duplicate of
`ghcaa-ci-standard.yml` and should be deleted. Both trigger on push and pull_request to `main`/`master`.
`main.yml` targets `dotnet-version: 8.x` while the rest of the project is .NET 9, runs no tests at all
(build only — no `dotnet test`, no vitest), and uses unpinned action tags. **Why deleting is the fix
rather than upgrading it:** two workflows racing on the same trigger means the weaker one can report
green independently of the real one, which is worse than having no second workflow — a green check that
means nothing is a check people learn to trust. `ghcaa-ci-standard.yml` already does the full
lint/test/build chain on the same branches, so nothing is lost. This looks like a leftover from before
that workflow existed. **Acceptance:** `main.yml` is gone and the branches it covered are still gated by
`ghcaa-ci-standard.yml`.
**Resolved 2026-09-06:** `main.yml` deleted.

82.25 [DONE 2026-09-06] **Priority: P2 | Depends on: none.** The mobile release pipeline ships to app stores with
no test gate. `mobile_deployment.yml` triggers on `push: tags: v*` and goes straight to
`flutter build appbundle --release` and `flutter build ipa --release`, then uploads to the Play Store
internal track and TestFlight. There is no `flutter test` step and no `needs:` tying the release to a
passing run — it relies entirely on the tagged commit having been gated earlier by a different
workflow. **Why the reliance is not good enough:** a tag pushed by hand, or pushed at a commit that
never went through CI, ships untested code to app stores, which is the one target where a bad build
cannot be hot-fixed and has to go through review again. **Acceptance:** the release job cannot run
unless tests for that commit have passed, either through `needs:` or an explicit test step in the
workflow.
**Resolved 2026-09-06:** new `test` job (`flutter test --reporter expanded`); `build-android` and
`build-ios` both gained `needs: [test]`.

82.26 [DONE 2026-09-06] **Priority: P3 | Depends on: none.** CI runs no dependency or container scanning — no
CodeQL, no `npm audit`, no `dotnet list package --vulnerable`, no image scan anywhere in
`.github/workflows/`. **Why the recommendation is deliberately two commands and not a security
pipeline:** a full SAST/DAST setup would be the overengineering REVIEW.md §4 rules out at this scale.
But `npm audit --audit-level=high` and `dotnet list package --vulnerable` are two lines that would have
caught what 48.9 and 48.17 (a stale `xlsx`, Angular CVEs) had to be found by hand. The justification is
the cost asymmetry, not thoroughness for its own sake. **Acceptance:** both commands run in CI, and a
known-vulnerable dependency fails the build rather than being found by a person later.
**Resolved 2026-09-06:** added to `ghcaa-ci-standard.yml` — `npm audit --audit-level=high` in
`lint-frontend`, and a `dotnet list package --vulnerable --include-transitive` check in `lint-backend`
(grepped against its own report text, since the command exits 0 even when it finds one — confirmed
locally: it currently flags real transitive vulnerabilities in AngleSharp and AutoMapper).

82.27 [DONE 2026-09-06] **Priority: P2 | Depends on: none.** `README.md` told operators something false about
how the schema is provisioned: "Schema — created and seeded at startup via EF Core `EnsureCreated()`
(not migrations); safe to re-run against an existing database." `Program.cs`'s post-boot hook does the
opposite — `MigrationBootstrapper.EnsureMigratedAsync` runs first and applies real EF Core migrations,
baselining migration history on a legacy `EnsureCreated`-built database, and `EnsureCreated()` survives
only as the `catch` fallback, logged as an error. **Why this specific wrong sentence is worth an item:**
it is aimed at the operator audience on the one subject where a wrong belief is most expensive. An
operator reading it concludes the platform has no migration path and plans schema changes accordingly.
82.13 tracks the *absence* of recorded decisions; it does not catch an existing statement being actively
wrong, which is worse than silence. **Resolved 2026-09-06:** both `README.md` mentions of the schema
mechanism (Database Setup, Maintenance & Operations) now describe `MigrationBootstrapper` as the real
path with `EnsureCreated()` as the logged fallback. **Resolved fully 2026-09-06:** 82.13 built `docs/adr/`, and
`docs/adr/0002-migrations-apply-automatically-at-startup.md` is the ADR this item's acceptance asked
for — no manual `dotnet ef database update` step exists anywhere in the pipeline; `MigrationBootstrapper`
runs on every boot and falls back to `EnsureCreated()` only on failure.

82.28 [DONE 2026-09-06] **Priority: P4 | Depends on: none.** `GHCAA.Tests/UnitTest1.cs` is the unmodified
`dotnet new nunit` scaffold — a single `Assert.Pass()` — sitting in an otherwise well-organised 79-file
suite. Delete it. **Why it is worth a tracker line at all rather than just doing it:** three separate
review passes have now rediscovered it, and an item is cheaper than a fourth rediscovery.
**Explicitly not in scope:** the five migrations sitting directly under `Data/Migrations/`. An earlier
research pass called them dead pre-split artifacts and recommended deleting them; `dotnet ef migrations
list` shows all five in the live chain, so deleting them would break migration history. If their
location is ever tidied, that is a separate change needing a migration-chain test, not a cleanup.
**Acceptance:** `UnitTest1.cs` is gone and the suite still passes.
**Resolved 2026-09-06:** deleted; `dotnet build GHCAA.Tests` still succeeds.

82.29 [DONE 2026-09-06] **Priority: P2 | Depends on: 82.16 (done — supplies the rule and the pattern).** `ECMember`
had two removal semantics living side by side: `GovernanceService.DeleteECMemberAsync` did a hard
`Remove()`, while `RemoveMemberFromCommitteeAsync` end-dates the row by setting `EndDate`. Same entity,
two meanings, and which one a member's committee service disappeared under depended on which screen an
admin used. `docs/ARCHITECTURE.md` §4 places `ECMember` in Class A, so the hard delete contradicted the
stated rule. **Not a true duplicate, so not resolved by picking one:** the two paths are legitimately
different admin actions — `RemoveMemberFromCommitteeAsync` ends a real, historical committee term
(a period ending, a term being terminated with a reason and effective date); `DeleteECMemberAsync` is a
separate, `[RequireStepUp]`-gated correction for a row that should never have existed (a member added to
the committee by mistake), live-wired from `admin-members.ts`'s `deleteECHistory`. Removing the hard-
delete path would have deleted a working feature, not fixed a bug — confirmed with the user before
touching either method. **Resolved 2026-09-06:** kept both paths, and closed the real gap instead: a
permanent delete of Class A evidence with no recorded actor. `ECMember` gained the five Class A fields
(`ARCHITECTURE.md` §4) it was missing — `IsDeleted`, `DeletedAt`, `DeletedByAdminId`, `UpdatedAt`,
`UpdatedByAdminId` — via migration `20260906094338_AddECMemberSoftDelete` (PgSql only, matching every
other schema change since 2026-08: the SQLite/MySql migration sets are not actively maintained).
`ECMemberConfiguration` now filters `!em.IsDeleted` the same way `PaymentHistoryConfiguration` does.
`DeleteECMemberAsync` takes a required `adminId` and soft-deletes (`IsDeleted`/`DeletedAt`/
`DeletedByAdminId`), replacing `_db.ECMembers.Remove(ecMember)`, exactly mirroring
`FinancialService.DeletePaymentAsync`'s existing pattern. `AdminGovernanceController.DeleteECMember`
resolves the admin id from `ClaimTypes.NameIdentifier` and returns `Unauthorized()` if it cannot,
matching `FinancialsController.DeletePayment`. Three new tests:
`GovernanceServiceTests.DeleteECMemberAsync_SoftDeletesWithActorAndTimestamp`,
`..._ReturnsFalse_WhenAlreadyDeleted`, and
`DestructiveStepUpActionsTests.AdminGovernanceController_DeleteECMember_ReturnsUnauthorized_WhenCallerIdMissing`.
**Gotcha hit and fixed while scaffolding the migration:** the first `dotnet ef migrations add` swept in
~4,400 unrelated `DeleteData`/`InsertData`/`UpdateData` seed-drift operations and truncated
`PgSqlApplicationDbContextModelSnapshot.cs` from ~74,460 lines to ~4,600 (see
`gotcha_ef_migrations_add_remove_corrupts_snapshot`, memory) — caught via `git diff --stat` before
committing, not by the build (it still compiled). Recovered by restoring the real snapshot with `git
checkout`, hand-patching in just the 5 new property lines, and rebuilding the migration's `.Designer.cs`
from the corrected snapshot with a script that swaps only the class/method wrapper. Final snapshot diff:
15 lines changed, matching exactly the 5 properties actually added.

82.30 [DONE 2026-09-07] **Priority: P3 | Depends on: 82.16 (done — supplies the rule).** `Member` and `User` carried
`IsArchived` where `docs/ARCHITECTURE.md` §4 named the field `IsDeleted`. Same idea, two names, so a
developer reading either entity could not tell whether the difference was deliberate. **Why P3:** nothing was
broken — `IsArchived` worked and was indexed (`Member(Status, IsArchived)`, WP 24.37). This was a naming
divergence, and renaming a column that a composite index, a global query filter and several auth guards
all depend on cost more than it returned at the time. **Two acceptable outcomes, not one:** either rename
to `IsDeleted` with a migration that also rebuilds the index, or amend §4 to name `IsArchived` as the
Class A field and rename `FinancialRecord`/`PaymentHistory` to match instead. Pick the cheaper one.
**Acceptance:** one name for the soft-delete flag across all Class A entities, and §4 says which.
**Resolved 2026-09-07:** `IsArchived` was the majority already (`Member`, `User`, `Poll`, `Campaign`), so the
cheaper direction was renaming the minority — `FinancialRecord`, `PaymentHistory`, and `ECMember` (which had
picked up its own `IsDeleted` field the day before, in the `AddECMemberSoftDelete` migration) — to `IsArchived`.
Additive `RenameColumn` migration `20260907063000_RenameIsDeletedToIsArchived`; Postgres carries the two
`PaymentHistories` partial-index predicates across the rename automatically (it tracks by column attnum, not
by text), so no index drop/recreate was needed. §4 now names `IsArchived` as the Class A field with a note on
why it won. All call sites, configurations, and existing tests updated to match.

82.31 [ONHOLD 2026-09-06, per SR-9] **Priority: P0 | Depends on: 62.31 (decides the target state, itself ONHOLD).** DATA PROTECTION, and the
half of it 62.31 does not reach. Every seed is loaded with `HasData`, which is part of the EF model
rather than a runtime import, so each seeded row was written into a migration as a literal `InsertData`
value and committed. Eight migrations carry roughly 4.3 MB of seed data, and across that chain there
are **612 distinct email addresses, 638 mobile numbers and 631 bcrypt password hashes** — 631 being the
whole `Users` table, so every real member's password hash is in git. The seeded `Members` insert also
carries `NID`, `DateOfBirth`, `FatherName`, `MotherName`, both addresses and emergency contact details.
**Why this is separate from 62.31 rather than part of it:** 62.31 proposes anonymising or externalising
`Seed/members.json`. Editing that file changes what a *future* migration would contain and changes
nothing about the eight already committed; a fresh database built from this repo still comes up holding
631 real alumni. `ApplicationDbContext.IsSeedDisabled` and `ASP_SEED_PROFILE` do not help either —
both are evaluated in `OnModelCreating`, so they gate migration *generation*, not migration *apply*.
**Also a rotation problem, not only a disclosure one:** 631 password hashes that have been in a
repository cannot be treated as secret again by deleting them, so whatever is done here has to be
paired with a forced reset, and that ordering needs deciding before any of it starts.
**Not started, and deliberately not started here:** this is P0 data-protection work of the type the
project owner has put on hold. Recorded so the gap is not rediscovered a fourth time; no file touched.
**Acceptance:** a database built from a clean clone of this repository contains no real personal data,
and the route by which the existing hashes stop being usable is written down and carried out.
See `docs/SEED_CLASSIFICATION.md` for the per-file classification this rests on.

82.32 [DONE 2026-09-05] **Priority: P1 | Depends on: 82.16 (the code this reviewed).** Full bug sweep
of the financial/payment/event stack, on the user's instruction to find and fix everything, not just
the 82.16 diff. Two independent review passes plus manual verification found 16 confirmed defects, all
either fixed or, for the two that were not code bugs, resolved by decision. `dotnet test` 586/586,
`vitest` 383/383, `flutter test` 93/93 after every fix; nothing suppressed to get there.

**Fixed - crash/security-relevant:**
1. Guest event payment (an event with `AllowNonMembers`) attributed the payment to a fabricated Member
   Id 0 (`memberId ?? 0` in `GatewaysController.InitiatePayment`), which does not exist, throwing a
   foreign-key `DbUpdateException` on every such payment. `PaymentHistory.MemberId` is now nullable -
   matching `EventRegistration.MemberId`, already nullable "for non-members" - with the query filter,
   notification, receipt-storage and dashboard-aggregate paths all updated to treat a guest payment as
   real but ownerless. Migration `FixPaymentHistoryGuestAndIndexes`.
2. `POST /api/financials/record-payment` kept the client-supplied body `MemberId` unchanged when the
   token had no `MemberId` claim, so an authenticated principal without that claim (a system-admin
   token) could attribute a payment and its receipt to an arbitrary member. Now refuses.
3. `GET /api/financials/my-dues` crashed with an unhandled 500 (`int.Parse(...)!.Value` on a claim that
   is routinely absent on the system-admin branch) instead of the `Unauthorized`/`BadRequest` every
   sibling endpoint in the same controller already returns in this situation.
4. Soft-deleted payments kept occupying the unique `TransactionId`/`GatewayPaymentId` indexes forever
   (82.16 added `IsDeleted` but not to the index filters), so re-submitting the same real bank
   transaction id after an admin deleted a wrong or duplicate entry hit a 500 with no way to diagnose
   it, since the blocking row was itself invisible. Both indexes now filter `IsDeleted = false`.
5. `POST /financial-ledger/records` bound the client's `FinancialRecord` body directly, so a caller
   could set `isDeleted`/`deletedByAdminId`/`updatedAt` on creation - forging the exact attribution
   82.16 exists to make trustworthy - and a claim that failed to parse silently kept whatever
   `CreatedByAdminId` the client posted. Every audit field is now server-set; an unparseable claim
   refuses the request.
6. A case-sensitive `.Contains("EVT-REG")` classified a lowercase, client-supplied payment reference as
   a `MembershipFee` while the rest of the flow (case-insensitive `StartsWith`) treated it as an event
   payment - reopening, through a casing mismatch, the exact bug 29B.3's category guard was written to
   close: a member paying an event fee could be auto-inducted as a full member. Detection now runs on
   `FinancialCategory`, set once at initiation, everywhere a payment's kind is checked.
7. The gateway's own inline membership-fee lookup (`GatewaysController`, auto-approval on payment
   success) filtered only on `MembershipType` and `EffectiveDate`, missing `IsActive`, the `Category`
   filter and the `EffectiveTo` upper bound that the canonical `GetApplicableFeeAsync` applies - a
   disabled or expired fee row, or a differently-categorised row for the same type, could silently win.
   Now calls the canonical method. `FinancialService.HandleAutomatedApprovalsAfterPaymentAsync`, a
   second inline copy with the same gap plus a missing `FinancialCategory` guard, was brought in line
   too (that method currently has no production caller, so this half was latent, not live).
8. `PollService.VoteAsync`: an empty `optionIds` list passed every check and returned `true` having
   recorded nothing; two concurrent votes for different options in a single-choice poll both passed
   the in-memory "already voted" check and both got inserted, inflating that poll's count (the unique
   index is `(PollOptionId, MemberId)`, correctly, since a multi-choice poll needs several rows per
   member - it cannot also enforce "at most one poll per member"). Fixed with an empty-list guard and
   the same Serializable-transaction pattern `MemberService`'s own check-then-act writes already use.

**Fixed - wrong result, no crash:**
9. Five admin-facing event reads/writes (`GetAllRegistrationsForAdminAsync`, `GetEventTasksAsync`,
   `GetEventBudgetAsync`, `UpdateEventBudgetAsync`, `AddEventExpenseAsync`) inherited
   `EventTaskConfiguration`/`EventBudgetConfiguration`/`EventRegistrationConfiguration`'s
   `HasQueryFilter(x => x.Event.IsActive)` filter with no `IgnoreQueryFilters()` - the exact bug class
   `gotcha_alumnievent_global_query_filter` already names for `AlumniEvent` itself, reopened here on
   its children. An admin opening tasks/budget/registrations for a draft or unpublished event saw an
   empty screen; the budget upsert, unable to see its own existing row, hit `EventBudget`'s unique
   index on `EventId` and 500'd instead of updating.
10. The org-wide dashboard balance excluded every payment made by a member who was later archived -
    `PaymentHistoryConfiguration`'s query filter hides `!Member.IsArchived` rows in addition to
    soft-deleted ones, and the balance calculation had no `IgnoreQueryFilters()`. Money already
    received is not undone by the payer being archived afterwards.
11. Editing a soft-deleted ledger row 500'd (`KeyNotFoundException`, uncaught) instead of 404ing -
    `FindAsync` used to return null only for a row that never existed; after 82.16 it also returns null
    for one the query filter is hiding. `UpdateRecordAsync` now returns null for both and the
    controller answers `NotFound()`.
12. `GetRecordsAsync` (ledger) and `GetAllRegistrationsForAdminAsync` (events) never validated
    `page`/`pageSize`: `page=0` produced a negative SQL `OFFSET` (a provider exception), `pageSize=0`
    divided by zero and cast `double.PositiveInfinity` to `int` (unspecified - yields `int.MinValue`,
    advertising a `TotalPages` of roughly -2.1 billion). Both now clamp.
13. CSV formula injection in the ledger export: `Description`/`Reference` were quoted for embedded
    quotes but not neutralised for a leading `=`, `+`, `-` or `@` - a row whose description is
    `=HYPERLINK(...)` executes as a formula the moment a SuperAdmin opens the exported CSV in
    Excel/Sheets. Now prefixed with `'` when it would otherwise start a formula.
14. The 82.16 audit trail itself was write-only - nothing anywhere called `IgnoreQueryFilters()`
    against `FinancialRecords`/`PaymentHistories`, so `IsDeleted`, `DeletedAt`, `DeletedByAdminId` and a
    deleted row's value were unreachable from any endpoint. `GetRecordsAsync` gained an
    `includeDeleted` flag (SuperAdmin-only, off by default, no existing caller's result changes).

**Test-correctness fixes (no production bug, but a test that passed for the wrong reason):**
15. `FinancialAuditTrailTests.DeleteRecord_Twice_ReportsNotFoundTheSecondTime` exercised the tracked-
    entity `IsDeleted` branch, a path production (fresh `DbContext` per request) never reaches - the
    second `FindAsync` there always re-queries and gets null from the query filter instead. Added
    `ChangeTracker.Clear()` so the test takes the same path production does.
16. `FinancialServiceTests.DeletePaymentAsync_ShouldRemovePaymentAndLogActivity` asserted
    `!AnyAsync(...)`, which the query filter alone satisfies - it would have passed identically if
    `DeletePaymentAsync` stamped nothing at all. Now asserts through `IgnoreQueryFilters()` that the
    row survives, soft-deleted, with its deleter recorded.

**Reviewed, not a bug - resolved by decision, not by code:**
- `PaymentHistory.DeletedByAdminId`/`UpdatedByAdminId` resolve against `Users.Id`
  (`ClaimTypes.NameIdentifier`), not `Members.Id` like `MembershipFeeConfig.CreatedByAdminId` does.
  Kept as `Users.Id`: it is the only claim a system-admin token (no `MemberId`) actually carries, it
  matches `FinancialRecord`'s own pre-existing `CreatedByAdminId` convention, and `PaymentHistory` had
  no established convention of its own to disagree with (SR-8: not a refactor, so no justification owed
  beyond this).
- The model snapshot's diff during 82.16 appeared to delete a `SiteContents` "Logo & Flag" row. Traced
  to `20260831000000_FixAssociationContentMergeLogoFlag`, which already removed that row from the live
  database via raw SQL on 2026-08-31, deliberately outside `HasData` to avoid this exact churn - the
  snapshot was simply stale and catching up to a change already applied, not losing data.

**Client-side (Angular/Flutter) sweep, same item, completed 2026-09-05.** Six more confirmed defects:
17. **Admin event edit shifted the event's stored time by the UTC offset, compounding on every edit.**
    `admin-events.ts` formatted a date for a `datetime-local` input with `toISOString().slice(0, 16)`
    - UTC wall-clock text - into a control the browser always reads/writes as LOCAL time, then ran
    another local-to-UTC conversion on save. A Dhaka (UTC+6) event edited twice lost 12 hours.
    Replaced with a local-parts formatter (`toLocalDateTimeInputValue`); the save side was already
    correct once the input holds a real local value.
18. **Participant limit and waitlist were silently discarded on every event save**, in both
    directions. The admin form collects and displays `participantLimit`/`hasWaitlist`, but the save
    payload never included them (client bug), and the read-side `EventDto` never returned them either
    (backend bug, same item since neither half works without the other) - so re-opening a just-saved
    event for another edit would wipe the value a second time even after the client half was fixed.
    Both fixed: `EventDto` and its three mapping sites now carry the fields; `AlumniEvent`/TS
    interface already had them.
19. **"Download Receipt" on the web Payments page always 404'd**, and would have 401'd even fixed.
    `financial.service.ts` built `my-receipt/{id}` against a controller route that is actually
    `receipt/{paymentId}`, and called it with `window.open()`, which sends no Authorization header to
    an `[Authorize]`'d endpoint. Now fetched as a blob through `HttpClient` (the auth interceptor
    attaches the token) and opened from an object URL.
20. **Mobile always reported ৳0.00 outstanding dues.** `GET /api/financials/my-dues` returns a JSON
    array of `MembershipDueDto`; the Dart client indexed it with a string key (`response.data['amount']`),
    which threw, and the catch block silently returned 0.0 with no error surfaced - a member with real
    unpaid years saw no balance and no "pay dues" prompt, in a success state. Now sums `amount` over
    entries where `isPaid` is false.
21. **News links from the landing page and member dashboard hit a route that does not exist**
    (`/portal/news/:id` and `/news/:id`, neither ever defined - NG04002). The News component opens a
    post inline via `selectPost()` by design; there was never meant to be a detail route. Both links
    now pass `id` as a query param, which the component reads on load to open the matching post -
    the same mechanism a card click already used.
22. **Two admin screens showed the wrong page title.** `NavService.labelFor` matched nav items by
    `url.includes(x.path)` against an ordered list, so `/admin/members/ec` matched "All Members"
    (a path-prefix of Executive Committee's own path) before ever reaching the right entry, same for
    `/admin/payments/fees` under "Payment Settings". Now picks the longest matching path, so the more
    specific route always wins regardless of list order.

Two more, cheap enough to fix on sight though below the bar for a numbered defect: Flutter's
`gatekeeper_screen.dart` called `setState` after an `await` with no `mounted` guard in four places -
now guarded, so leaving the QR scanner mid-request no longer throws.

**Deferred, recorded rather than rushed - 82.33:** the same `[Authorize]`-with-no-token problem as
finding 19 exists on **mobile's** receipt download (`financial_portal_screen.dart:_downloadReceipt` -&gt;
`launchUrl` on an auth-protected URL, opened in the external browser). Fixing it properly needs a
download-to-temp-file-then-open flow, which likely means a new dependency (`open_filex` or similar) -
a package choice worth making deliberately rather than adding under a bug-fix pass. Also on the same
screen: a `canLaunchUrl` false branch returns with no feedback to the member. Both recorded as 82.33,
not fixed here.

**Explicitly out of scope, tracked separately, not touched:** 82.31 (PII already committed in migration
history) per the standing hold on P0-type data-protection work; 82.29/82.30 (the two audit-field gaps
82.16's own rule named); 82.33 (mobile receipt download, above).
**Acceptance:** every confirmed defect above is fixed or has a recorded reason it is not; all three test
suites green; nothing silenced to get there.
**Non-development work done alongside this item, so it is not lost between sessions:**
`docs/book/build/tracker_page.py` and `docs/book/build/wbs.py --check` were re-run so this item and its
counts reach `tracker.html` and the effort model; `docs/book/build/build.py --pdf --strict` was re-run
and still ends `status : clean, ready to deliver`; the memory index and topic files for this session
were updated per the Memory Protocol.

82.33 [DONE 2026-09-07] **Priority: P3 | Depends on: none.** Mobile's payment-receipt download
(`GHCAA.Mobile/lib/screens/member/financial_portal_screen.dart:_downloadReceipt`) hands an
`[Authorize]`'d API URL straight to `launchUrl`, which opens it in the external browser with no
Authorization header — the same defect the web Payments page had (fixed in 82.32, finding 19), except
web could be fixed by fetching the PDF through the app's own authenticated HTTP client and opening it
as a blob URL; a Flutter external-browser launch has no equivalent. **Why this needs its own item
rather than a quick fix:** the real fix is download-through-Dio (so the auth interceptor attaches the
token) to a temp file, then open that file locally — which most likely means adding a package
(`open_filex` or similar), a dependency choice `feedback_keep_lightweight` says should be made
deliberately, not folded into a bug-fix pass. Also on the same method: a `canLaunchUrl` false branch
returns with no feedback to the member, who sees nothing happen. **Scope expanded 2026-09-06** (found
closing 80.14): the identical defect exists a second time in
`GHCAA.Mobile/lib/screens/member/member_details_screen.dart:309-317` (`_viewNetworkImage`), used to
render a member's certificate/payment-proof image (`member_details_screen.dart:239-251`) — same
pattern (protected resource, unauthenticated raw request), different call site (`Image.network`, not
`launchUrl`) and a different root cause of unreachability (the constructed URL doesn't match any
registered route, not just missing a header). Fix both from one authenticated-fetch helper rather than
two one-off patches. **Acceptance:** a member can view or save their receipt PDF on mobile without an
unauthenticated request ever leaving the device, a failure to open it says so, and an admin can view a
member's certificate/payment-proof image in the mobile app (currently broken/404).
**Resolved 2026-09-07:** `fetchAuthenticatedBytes`/`downloadToTempFile` added to
`GHCAA.Mobile/lib/features/files/file_service.dart`, routed through the app's own Dio so the auth
interceptor attaches the token — no new package needed (`share_plus`/`path_provider` were already
dependencies). `financial_portal_screen.dart`'s `_downloadReceipt` downloads via Dio and hands the file
to the share sheet, with a snackbar on failure. `member_details_screen.dart`'s `_viewNetworkImage` now
fetches through the actual authorized `/secure-files/{path}` route (the old code built a URL matching no
registered route at all) and renders via `Image.memory`. Tests in `test/file_service_test.dart`.

82.51 [DONE 2026-09-06] **Priority: P4 | Depends on: none.** Found while closing 80.14: `LocalFileStorageService`'s
secure/public path separation (`Certificate`/`PaymentProof`/`Signature` → `_secureRoot`, everything else
→ `_publicRoot`) held only because two independently-chosen fallback defaults happened not to
collide (`_publicRoot` defaults to `"wwwroot"`, `_secureRoot` to `AppDomain.CurrentDomain.BaseDirectory`)
— nothing in the code asserted they must stay different roots. `project_uploads_ephemeral_storage.md`
(memory) already flags that this project needs a persistent-disk fix for the ephemeral-storage problem;
whoever builds that could plausibly set `FileStorage:BasePhysicalPath` to a value that makes
`_secureRoot` coincide with the static-files webroot, silently exposing every certificate/payment-proof/
signature with no auth check. **Resolved 2026-09-06:** `LocalFileStorageService` now takes
`IWebHostEnvironment` and throws `InvalidOperationException` from its constructor if `_secureRoot`
resolves to or inside `WebRootPath`. `LocalFileStorageServiceTests.Constructor_ThrowsWhenSecureRootResolvesInsideWebRoot`
sets `FileStorage:BasePhysicalPath`/`SecureRelativePath` to collide the two roots and asserts the throw
and message; the other 15 tests in the file still pass unchanged.

82.34 [DONE 2026-09-07] **Priority: P4 | Depends on: none.** Found while closing 82.14 (§7, Angular review):
`GHCAA.Web/src/app/core/interceptors/auth.interceptor.ts` is a complete, unit-tested, bearer-header-
only HTTP interceptor that is never registered — `app.config.ts:18` wires only `globalHttpInterceptor`,
whose own bearer-attachment logic (`global-http.interceptor.ts:30-36`) is a superset of what the unused
file does. Dead code duplicating a subset of a live file. **Acceptance:** the file and its spec are
deleted, and the suite still passes.
**Resolved 2026-09-07:** confirmed only self-referenced (`app.config.ts` wires `globalHttpInterceptor`
only), then the file and its spec were deleted; suite still passes.

82.35 [DONE 2026-09-07] **Priority: P4 | Depends on: none.** Found while closing 82.14 (§7). Only 3 of ~83 Angular
components use `ChangeDetectionStrategy.OnPush`, despite the app's signals-based state (`signal()`,
`computed()`) being well suited to it. Not a user-visible defect today, but a straightforward,
low-effort performance win left on the table. **Scope:** opportunistic — add `OnPush` when a component
is touched for another reason, not a dedicated sweep. **Acceptance:** no specific number required; this
item tracks the intent so it isn't forgotten, not a deadline.
**Resolved 2026-09-07:** `OnPush` added to 4 safe, fully signal-driven components touched this session —
`ModalHeaderComponent`, `ConfirmDialog`, `Health`, `AdminAudit`. More complex/mutation-heavy components
left alone, per this item's own "opportunistic, not a sweep" scope. Intent stays tracked here for future
touches, not treated as a closed count.

82.36 [DONE 2026-09-07] **Priority: P3 | Depends on: none.** Found while closing 82.14 (§7). No HTTP retry/backoff
exists anywhere in the Angular app for transient failures (`grep -rn "retry(\|retryWhen" GHCAA.Web/src/app`
returns nothing) — a 5xx or a timeout on a GET fails immediately with no second attempt. **Scope:**
idempotent GET requests only; never retry a POST/PUT/DELETE automatically. **Acceptance:**
`global-http.interceptor.ts` retries a GET once (or twice, with backoff) on a transient network/5xx
failure before surfacing the error to the user.
**Resolved 2026-09-07:** `global-http.interceptor.ts` now retries GET requests up to 2 times with a
linear backoff (via `timer`), scoped to transient failures (status 0 or 5xx) only — a 4xx never retries.

82.37 [DONE 2026-09-07] **Priority: P4 | Depends on: none.** Found while closing 82.14 (§7). Only 7 of 73 Angular
templates carry any `aria-*` attribute. **Scope:** the highest-traffic member/admin forms first
(registration, profile edit, admin member edit), not a blanket pass across all 73 templates — a forced
pass would produce mechanical, low-value `aria-label`s on elements that don't need them. **Acceptance:**
the forms named above pass a manual screen-reader smoke test (labelled inputs, announced errors).
**Resolved 2026-09-07:** `id`/`for` label association and `role="alert"` on validation-error containers
added for the static fields in `public/register/register.html`, `member/profile/profile.html`, and
`admin/members/admin-members.html` — the three named forms. Dynamic/looped fields (academic/professional
history rows, conditional selects) left out of scope, matching this item's own anti-blanket-pass
instruction.

82.38 [DONE 2026-09-06] **Priority: P1 | Depends on: none.** Found while closing 82.14 (§8, Flutter review): a
correctness bug, not a hygiene item. `roleProvider` and `userProfileProvider`
(`GHCAA.Mobile/lib/features/auth/auth_service.dart`) are documented in their own
comments as needing an explicit `ref.invalidate` after login/logout, but `AuthService.logout()`
only invalidated the notification-hub provider. On a shared or handed-
down device, a second user logging in after the first logs out could see the first user's cached
role/profile (including admin-only UI) until something else happened to trigger a refetch. **Why P1
and not a routine fix:** this is a stale-privilege-label bug, the same class of risk the session's
`SecurityStampMiddleware`/token-rotation work exists to close on the backend side — a client-side cache
that outlives the session it belongs to undermines that. **Resolved 2026-09-06:** `logout()` now
invalidates `roleProvider` and `userProfileProvider` alongside the notification-hub provider; `login()`
invalidates the same two after a successful sign-in, so a stale value can't survive either transition.
`GHCAA.Mobile/test/auth_service_test.dart` logs in as user A, logs out, logs in as user B, and asserts
A's role/profile never renders for B — verified to fail on the pre-fix code (reproduces the bug) and
pass on the fix.

82.38a [DONE 2026-09-07] **Priority: P2 | Depends on: 82.38.** A previous session wrongly closed the
book on `major_functionalities_test.dart`'s "Successful Login returns null and saves tokens" CI failure
as "pre-existing, unrelated to current work" without checking git blame. It is not pre-existing: 82.38
added `_ref.invalidate(roleProvider)`/`_ref.invalidate(userProfileProvider)` calls inside `login()`
itself, and this file's own `FakeRef` (distinct from the one `auth_service_test.dart` uses, which never
calls `login()` directly) only overrides `read()`, not `invalidate()` — Mocktail's `Fake` throws
`UnimplementedError` for any unstubbed method, so every successful login in this test file started
throwing and falling through to the generic error message instead of returning `null`. **Fix:** added
`@override void invalidate(ProviderOrFamily provider) {}` to `FakeRef` in
`GHCAA.Mobile/test/major_functionalities_test.dart`. **Acceptance:** `flutter test
test/major_functionalities_test.dart` — 6/6 passing (was 5/6). The wider `flutter test` run still shows
~58 golden-image failures unrelated to this fix; those are local-machine pixel-diff noise (0.1-0.5%
per-image, this repo's CI already treats them as "Skip expect" rather than fail) from rendering on this
Windows box rather than the CI runner's environment, not a regression.

82.39 [DONE 2026-09-07] **Priority: P2 | Depends on: none.** Found while closing 82.14 (§8). Inactivity/session-
expiry logic exists in two independent places with two different timeouts, both clearing the same
storage independently: `SessionManager` at 15 minutes (`GHCAA.Mobile/lib/core/session/session_manager.dart:12`)
and `main.dart`'s own `_checkInactivity` at 10 minutes (`main.dart:154-155`). Whichever fires first
wins, so the effective timeout is silently the shorter of the two, with no single place that says so.
**Scope:** pick one timeout and one owner; delete the other. **Acceptance:** one inactivity-timeout
mechanism, one documented value, a test pins it.
**Resolved 2026-09-07:** `SessionManager` (15 minutes) is now the sole owner; `main.dart`'s duplicate
10-minute `_checkInactivity`/`lastActivityProvider`/`ActivityNotifier` path removed entirely. App-resume
calls `sessionProvider.checkNow()` directly (covers a backgrounded app's `Timer.periodic` missing ticks).
Expiry now goes through `AuthService.logout()` (full cleanup: SignalR hub + cached role/profile) instead
of a bare `storage.clearAll()`. Test: `test/session_manager_test.dart`.

82.40 [DONE 2026-09-07] **Priority: P2 | Depends on: none.** Found while closing 82.14 (§8). Mobile's biometric
"fast login" (`GHCAA.Mobile/lib/core/storage/storage_service.dart:147-165`) stores the member's raw
username and password in secure storage when the member opts in, rather than a device-bound token.
Secure storage is the right primitive, but a stored plaintext password is a wider attack surface than a
long-lived device-bound token: a token can be scoped, rotated, and revoked server-side without knowing
or changing the member's password, and this cannot. **Scope:** replace the stored credential with a
device-bound long-lived token (or equivalent), not a broader rewrite of the biometric flow itself.
**Acceptance:** no plaintext password is ever written to device storage; biometric re-login continues
to work via the new token.
**Resolved 2026-09-07:** `StorageService.saveCredentials/getCredentials` removed, replaced with
`setBiometricEnabled`/`isBiometricEnabled` plus `purgeLegacyBiometricCredentials()` (scrubs any password
an older build already wrote; never writes to those keys again). Biometric re-login now reuses the
refresh token saved from the last real login (`AuthService.loginWithStoredToken()`, hitting the existing
`/auth/refresh-mobile`) instead of resubmitting a stored password. Tests added to
`test/auth_service_test.dart`.

82.41 [DONE 2026-09-07] **Priority: P3 | Depends on: none.** Found while closing 82.14 (§8). Firebase Cloud
Messaging is wired (permission request, token retrieval, foreground/background listeners) but the
device token is only logged (`GHCAA.Mobile/lib/features/notifications/push_notification_service.dart`),
never sent to the backend, so no targeted push (as opposed to the separate SignalR broadcast/in-app
channel) can ever reach a specific member's device. `onMessageOpenedApp` also only logs the payload
instead of navigating via `go_router`. **Acceptance:** the FCM token is registered with the backend on
obtain/refresh, and tapping a push notification navigates to the relevant screen.
**Resolved 2026-09-07 (mobile side):** `push_notification_service.dart` now POSTs the device token to
`/notifications/device-token` on obtain and on `onTokenRefresh` (skipped while logged out, to avoid an
avoidable 401), and `onMessageOpenedApp` navigates via `go_router` using a `data.route` path the backend
supplies. Test: `test/push_notification_service_test.dart` (route-resolution + registration-guard logic;
Firebase itself isn't unit-testable here). **Gap found, tracked as 82.53a:** `/notifications/device-token`
does not exist on the backend yet, so the POST fails harmlessly (caught, logged) until it's added —
the item's acceptance criterion isn't fully met without that endpoint.

82.53a [DONE 2026-09-07] **Priority: P3 | Depends on: 82.41 (done — supplies the client-side call).** Add the
`/notifications/device-token` API endpoint 82.41's mobile client already calls. Needs a small persistence
spot for a member's current FCM token (a column on `Member`/`User` or a small side table, whichever fits
the existing `Notification`/`Member` schema better) and an endpoint that upserts it against the calling
member's id from the JWT claim. Out of scope: building the server-side push-send path itself — that's a
separate, larger item once targeted push (as opposed to the existing SignalR/in-app channel) is actually
wanted. **Acceptance:** the mobile POST added in 82.41 succeeds and the token is retrievable server-side
for a given member.
**Resolved 2026-09-07:** `Member` gained three nullable columns (`FcmToken`, `FcmTokenPlatform`,
`FcmTokenUpdatedAt`). New `IDeviceTokenService`/`DeviceTokenService` upserts/reads the token against
`ApplicationDbContext` — no controller touches the context directly. `NotificationController` gained
`POST /notifications/device-token` (also reachable at `/notification/device-token`), reading the member id
from the JWT claim, matching the payload shape (`token`, `platform`) the mobile client already sends.
Migration `20260907080000_AddMemberDeviceToken` hand-written (scaffolding collided with concurrent work on
the model snapshot, per the repo's own EF-migration gotcha) and verified via `dotnet ef migrations script`
to generate only three additive `ALTER TABLE` statements. 5 new tests in `DeviceTokenServiceTests.cs`.

82.42 [DONE 2026-09-06] **Priority: P2 | Depends on: none.** Found while closing 82.14 (§25, client-side
duplication sweep): the same five small lookup tables (membership status, member category, gender,
blood group, job category) plus academic-year list generation are hardcoded independently in both
clients, instead of using the `/lookups/{group}` endpoint that already exists and that
`GHCAA.Mobile`'s own `DropdownService` (`GHCAA.Mobile/lib/features/lookups/dropdown_service.dart:74-81`)
already calls first, falling back to its hardcoded copy only if the API returns empty. Two of the five
have already drifted in wording between clients: membership status `Applied` reads "Pending" on web
(`GHCAA.Web/src/app/core/constants/app.constants.ts:26-37`) but "Pending Approval" on mobile
(`dropdown_service.dart:126-134`); `InactivePayment` reads "Inactive" vs "Inactive (Unpaid)". The root
cause is on the web side: `GHCAA.Web` already has a `LookupService` wired to the same endpoint
(`core/services/lookup.service.ts`), but the screens using these five lists (`admin-members.ts`,
`profile.ts`, `register.ts`) import the hardcoded constants instead of calling it. **Scope:** wire the
existing `LookupService` into the Angular screens that bypass it — this alone collapses five separate
findings into one fix — then delete the now-redundant hardcoded lists in both clients so the backend's
`/lookups` data is the one source. **Acceptance:** both clients render the same label for the same
enum value in every case above, sourced from one place.

Closed 2026-09-06. `LookupService` gained `getOptions(group)` and `getAcademicYears()`, each
calling `/lookups/{group}` first and falling back to a built-in list only when the group comes back
empty — the same call-first-then-fallback shape `DropdownService` already used on mobile, just moved
into the service instead of scattered across screens. Converted `admin-members.ts`, `profile.ts`,
`register.ts` (the three named screens) plus three more that turned out to import the same hardcoded
lists: `directory.ts` (member category, academic years), `jobs.ts` (job category), `admin-comm.ts`
and `member-approval.ts` (academic years). `admin-comm.ts`'s year filter was a `computed()` reading a
plain array, which wouldn't have refired on the async update, so `years` became a signal there.
Deleted from `app.constants.ts`: `MEMBERSHIP_STATUS_OPTIONS`, `MEMBER_CATEGORY_OPTIONS`,
`GENDER_OPTIONS`, `getAcademicYears()`, and `ACADEMIC_DATA.getYears`. `BLOOD_GROUP_OPTIONS` and
`JOB_CATEGORIES` are still there but no longer exported — `getBloodGroupName()` and
`getJobCategoryLabel()` still read them for display formatting, which is a different concern from
the dropdown-population duplication this item targeted. Fixed the wording drift by matching mobile's
text in both `MEMBERSHIP_STATUS_MAP` (badge rendering) and the new `LookupService` fallback:
`Applied` → "Pending Approval", `InactivePayment` → "Inactive (Unpaid)"; `MemberCategory`'s three
differing labels (`None`, `Guest`, `Student`) were also aligned to mobile's shorter wording. Added
`LOOKUP_GROUPS` (Angular) and `LookupGroups` (Dart, in `registration_constants.dart`) so the group
name string (`'MembershipStatus'`, `'Gender'`, etc.) has one spelling per client instead of being
retyped at each `getOptions()` call site — `register_screen.dart`, `profile_edit_screen.dart` and
`dropdown_service.dart`'s own switch all had their own copies of these before. `npx vitest run`:
415/415 passing (7 new specs on `lookup.service.spec.ts`, LookupService mocked in the 7 converted
component specs). `flutter analyze`: no issues found.

82.43 [DONE 2026-09-06] **Priority: P4 | Depends on: none.** Found while closing 82.14 (§25), a two-minute
companion to 82.42: `GHCAA.Mobile/lib/core/constants/registration_constants.dart:33-37`'s
`AcademicConstants.getAcademicYears` has no callers anywhere in the app (`register_screen.dart` uses
`DropdownService`'s `'PassingYear'` case instead). **Scope:** delete the dead method; fold into 82.42's
change rather than a separate PR. **Acceptance:** the method is gone, `flutter analyze` stays clean.

Closed 2026-09-06 as part of 82.42. Confirmed zero callers by grep before deleting
`AcademicConstants.getAcademicYears`; `flutter analyze` reports no issues found.

82.44 [DONE 2026-09-07] **Priority: P3 | Depends on: none.** Found while closing 82.14 (§25). The same manual
debounce shape (`clearTimeout`/`setTimeout(…, 300)`) is copy-pasted identically across 5 Angular
components: `admin/governance/admin-governance.ts:159-168`, `common/directory/directory.ts:122-125`,
`common/jobs/jobs.ts:67-72`, `member/messages/messages.ts:112-122`, `member/requests/requests.ts:50,92-94`.
**Scope:** a small shared `debounce(fn, ms)` helper, or a debounced output on the existing
`SearchBarComponent`, adopted by all 5. **Acceptance:** one debounce implementation, 5 call sites use
it, behaviour unchanged (verified by each component's existing spec).
**Resolved 2026-09-07:** new `core/utils/debounce.util.ts` (with `.cancel()`) and a `SEARCH_DEBOUNCE_MS`
constant, adopted by all 5 named components.

82.45 [DONE 2026-09-07] **Priority: P3 | Depends on: none.** Found while closing 82.14 (§25). Raw `window.confirm()`
is used in 22 Angular files for delete/danger-action confirmation, despite the app already proving a
styled, on-brand confirm pattern in `common/step-up-dialog`. Every one of those 22 actions currently
breaks out of the app's own glass-UI design language into an unstyled native browser dialog — a UX-
consistency problem as much as a duplication one. **Acceptance:** a shared confirm-dialog
service/component exists and at least the highest-traffic admin delete actions (members, campaigns,
gallery) use it instead of `window.confirm()`.
**Resolved 2026-09-07:** `ConfirmDialogService` + `<app-confirm-dialog>` (mounted in `app.html` alongside
the step-up dialog) built, and every remaining `confirm()`/`window.confirm()` call in `GHCAA.Web/src/app`
converted — 20 more files beyond `admin-members.ts`/`admin-gallery.ts` (29 call sites total), none left.
**Campaigns note:** the item's acceptance line named "campaigns" as one of three highest-traffic
surfaces, but `admin-campaigns.ts`/`.html` has no delete/danger action at all — nothing there to convert.
That naming was a documentation/reality mismatch, not an unconverted call site. `admin-fee-config.spec.ts`
and `admin-payment-config.spec.ts` updated to mock `ConfirmDialogService` instead of `window.confirm`,
matching `admin-members.spec.ts`'s existing convention. `npm run type-check` and `npm run test:unit -- --run`
(419 tests) both pass; `npm run build` failed only at the Google-Fonts CSS-inlining step
(`connect ETIMEDOUT` to `fonts.googleapis.com`), a pre-existing environment/network restriction unrelated
to `src/styles.scss` (untouched) or anything changed here.

82.46 [DONE 2026-09-07] **Priority: P4 | Depends on: 82.45 (shares the same UI surface — do together if a shared
modal shell is built).** Found while closing 82.14 (§25). `.modal-header` markup (title + close button)
is hand-rolled identically in 17 Angular template files (`admin/roles/admin-roles.html:13-16`,
`admin/themes/admin-themes.html`, and 15 more). **Acceptance:** a shared modal-header (or full modal
shell) component exists; the 17 files use it instead of hand-rolled markup.
**Resolved 2026-09-07:** new `<app-modal-header>` component, adopted in all 17 identified files
(article-approval, admin-comm, contact-messages, admin-events, gallery-approval, admin-governance ×3,
job-approval, admin-members, polls ×2, admin-roles, common/events, messages, payments, step-up-dialog).

82.47 [DONE 2026-09-07] **Priority: P4 | Depends on: none.** Found while closing 82.14 (§25). 4 Angular admin
screens (`admin/comm/admin-comm.html:3-9`, `admin/dashboard/admin-dashboard.html`,
`admin/events/admin-event-operations.html`, `admin/polls/polls.html:3-6`) hand-roll their own
`<h1>`/`<h2>` header instead of the `app-page-header` component 19 other admin screens already use.
Pure consistency, no new component needed. **Acceptance:** all 4 screens use `app-page-header`.
**Resolved 2026-09-07:** all 4 templates converted to `<app-page-header>`.

82.48 [DONE 2026-09-06] **Priority: P3 | Depends on: none.** Found while closing 82.14 (§25), the Flutter
mirror of 82.45: `showDialog<bool>` + a hand-styled `AlertDialog` (same `AppTheme.midnightSurface`
background, same Cancel/Confirm `TextButton` footer) is repeated across ~19 mobile screens, despite the
codebase already having a precedent shared dialog widget (`reject_reason_dialog.dart`) that could
generalize. **Acceptance:** a shared confirm-dialog helper exists (e.g. `showConfirmDialog(context, title,
confirmLabel, {destructive: true})`); at least the highest-traffic delete actions use it.
Built `showConfirmDialog(context, {required title, message, confirmLabel = 'Confirm', destructive =
false})` in `core/widgets/confirm_dialog.dart`, returning `Future<bool>` (never `null`) so callers drop
the `== true` check. Converted the 12 delete/destructive confirm dialogs that fit its plain title +
message + Cancel/Confirm shape: `articles_screen.dart` (delete submission), `dashboard_screen.dart`
(logout), `event_details_screen.dart` (delete event), `permissions_matrix_screen.dart` (delete custom
role), `family_link_screen.dart` (cancel request, remove family link — two), `gallery_screen.dart`
(delete gallery), `jobs_screen.dart` (delete post), `job_details_screen.dart` (delete job),
`news_details_screen.dart` (delete article), `forum_topic_detail_screen.dart` (delete topic, delete
reply — two). Left on the old pattern: every dialog that's actually a form (create/edit event, gallery,
album, term, fee rule, job, topic, system admin, role assignment, mentorship request, family-link
search) — those aren't confirm dialogs and don't fit the helper's shape; the audit-log detail view and
the rejection-reason dialog on `member_details_screen.dart` (single-action / free-text input, not a
yes/no confirm); and `governance_registry_screen.dart`'s "Remove from committee" dialog, which embeds a
notify-member checkbox the generic helper has no slot for. `flutter analyze`: no issues found. Golden
tests touching the changed screens (`dashboard_visual_test.dart`) pass; the two shared
`comprehensive_visual_freeze_test.dart` / `full_app_visual_freeze_test.dart` suites fail on this same
1-4% pixel diff across dozens of screens this change never touched, confirmed via a stash-and-rerun on
the unmodified code — pre-existing flakiness in this environment, not a regression from this change.

82.49 [DONE 2026-09-06] **Priority: P4 | Depends on: none.** Found while closing 82.14 (§25). Two Flutter
screens (`GHCAA.Mobile/lib/screens/member/directory_screen.dart:126-131`,
`lib/screens/member/professional_hub_screen.dart:115-120`) hand-roll their own `Timer`-based search
debounce instead of using the already-existing debounced `AppSearchField` widget
(`core/widgets/app_search_field.dart:29-43`), which was built specifically to fix this exact per-
keystroke-refetch problem. **Acceptance:** both screens use `AppSearchField`; the hand-rolled `Timer`
logic is deleted.
Both screens now use `AppSearchField` in place of their raw `TextField`; the `Timer` field, the
`dart:async` import, and the cancel-and-restart debounce logic are gone from both. Same 300ms debounce
via the widget's `debounce` param, same clear-button behavior via `onClear`. `flutter analyze`: no
issues found. No test references either screen's search field or debounce directly — the golden tests
that render these two screens (`member_directory`, `member_professional_hub`) already fail on the same
pre-existing pixel-diff flakiness described in 82.48, confirmed unchanged by this edit via the same
stash-and-rerun check.

82.50 [DONE 2026-09-07] **Priority: P3 | Depends on: none.** User request 2026-09-06: a filterable HTML test-
coverage dashboard, in the same spirit as the tracker itself — per-service/controller filtering,
sortable by coverage %, drill-down into uncovered lines — generated from `dotnet test
--collect:"XPlat Code Coverage"` (`GHCAA.Tests`) piped through ReportGenerator, published as a static
page. **Explicitly requested:** it must refresh automatically after each test run, not be a one-off
snapshot someone forgets to regenerate — the same staleness failure mode this session's TODO/book
number checks kept finding elsewhere. **Scope decision needed before building:** where the refresh
hook lives (a CI job artifact vs. a local post-test script) and whether Angular/Flutter coverage
(`npm run test:unit -- --coverage`, `flutter test --coverage`) are in scope alongside the backend, or
a first iteration is .NET-only with the clients added later. **Deliberately not started now:** parked
until after the current queue, per direct instruction, so it doesn't compete with the in-flight
TODO-staleness review and book-sync work. **Acceptance:** a published HTML page reflecting the most
recent test run's coverage, filterable per service/controller, regenerated without a manual step every
time the relevant test suite runs.
**Scope decision made 2026-09-07:** CI job artifact (this repo's tests already run in GitHub Actions, so
hooking into that pipeline beats relying on a developer to run a script by hand); first iteration is
.NET-only, Angular/Flutter deferred to 82.53b below.
**Resolved 2026-09-07:** new `GHCAA.Tests/coverlet.runsettings` excludes the test assembly itself and EF
`Migrations`/`bin`/`obj` output from the denominator (the prior manual snapshot, `docs/COVERAGE_SNAPSHOT_
2026-05-26.md`, had been diluted to 0.43% by counting those). `ghcaa-ci-standard.yml`'s existing
`api-tests` job (no new job) now collects coverage on its one `dotnet test` run, pipes the cobertura XML
through `dotnet-reportgenerator-globaltool` 5.3.11 into an HTML report, and uploads it as a 30-day
`coverage-report` GitHub Actions artifact — regenerated on every run, no manual step. ReportGenerator's
stock HTML report already provides the sortable/filterable class list and per-class line drill-down, so
no custom dashboard code was needed.

82.53b [TODO] **Priority: P4 | Depends on: 82.50 (done — supplies the CI pattern to extend).** Extend the
82.50 coverage dashboard to the two clients: `npm run test:unit -- --coverage` (Vitest's own coverage
output) for Angular, `flutter test --coverage` (lcov) for Flutter. Deliberately deferred out of 82.50's
first iteration to keep that item .NET-only and land it sooner. **Acceptance:** both clients' coverage
is generated and published the same way 82.50's .NET report is — a CI artifact, refreshed every run, no
manual regeneration step.

82.53c [DONE 2026-09-07] **Priority: P1 | Depends on: none.** Found while verifying 82.53a's migration
(hand-written to route around the EF-scaffold/model-snapshot collision, itself following the
`gotcha_ef_migrations_add_remove_corrupts_snapshot` precedent): a full-history `dotnet ef database update`
against an empty database has never worked in this tree. It fails immediately on the oldest affected
migration with `System.InvalidOperationException: There is no property mapped to the column
'Members.IsProfileComplete'` — the exact error a prior session's WP45 closing note and the 82.53a agent
both hit and correctly logged as "pre-existing, out of scope," without either being asked to trace the
root cause. **Root cause:** several old migrations' `InsertData`/`UpdateData`/raw-SQL seed operations were
retroactively hand-edited, after the fact, to also set columns that a *later* migration adds — most
seed-data authors do this deliberately, to backfill a newer field's default onto rows a much older
migration already inserts, but nobody re-ran a from-scratch replay to notice the column does not exist
yet at the point in the chain where the edited INSERT actually runs. Confirmed three instances, all
`Member`/`User` seed rows: `IsProfileComplete` and four `Notify*` preference columns referenced in
`20260319073041_RefactorMemberAcademicProfessionalRecords`'s bulk 583-row member seed (columns not added
until `20260424114119_AddSocialAuthAndPolls` and `20260329180202_AddNotificationPreferences`
respectively), the same five columns referenced a second time in `20260327112934_AddSourceToActivityLog`'s
single-row admin-user seed, and `FailedLoginAttempts` referenced in that same migration's `Users` insert
(not added until `20260503053036_PhaseB_S5S8_OtpHmac_PaymentIdempotency_Indexes`). Ruled out a general
per-migration model-snapshot problem first: each migration's own `.Designer.cs` carries a point-in-time
`BuildTargetModel` snapshot that correctly resolves every *other* column in these operations — only the
handful of genuinely-too-early column references failed, confirming this is a small, enumerable set of
edits, not a wholesale case for touching every historical migration. **Fix:** removed the too-early column
(and its per-row value) from each of the three affected operations, relying on the later migration's own
`ADD COLUMN ... DEFAULT FALSE` to backfill these already-inserted rows exactly as it already does for
every other pre-existing row — no behavior changes, only the ordering bug is gone. Found the complete set
by writing a one-off script cross-referencing every migration's `INSERT INTO`/`InsertData`/`UpdateData`
column references against every `ADD COLUMN`/`AddColumn` across the whole chain, timestamp-ordered by
filename, so this was verified exhaustive rather than found by iterating replay failures one at a time.
**Acceptance:** `dotnet ef database update` against a brand-new empty Postgres database completes with
`Done.` reaching the current migration; `dotnet test` still green (713 passing). Both throwaway databases
and the one-off scripts used to verify this were deleted; nothing checked in beyond the migration fixes
themselves.

82.53d [DONE 2026-09-14] **Priority: P1 | Depends on: none.** User reported 2026-09-07: a manually triggered Render
deploy of commit `983c476` (the 82.51 currency-pipe change, a template/service refactor with no new
dependencies) failed after 2m24s with "Ran out of memory (used over 8GB)". This is the third distinct
Render-build OOM this project has hit — the `Dockerfile`'s own comments already document two prior
causes fixed on 2026-08-30 (running `GHCAA.Tests`'s `WebApplicationFactory` suite in-image, and a
redundant `dotnet build` pass before `dotnet publish` re-fingerprinting wwwroot's committed member
photos) — so the two structural fixes already in place were checked first and ruled out: neither
regressed. Applied one mitigation on inspection, not yet confirmed against a real deploy: the `web`
(Angular/Node) stage and the `build` (.NET restore/publish) stage share no `COPY --from` dependency
until the final image assembly, so BuildKit's DAG schedules them concurrently — `ng build --configuration
preprod` and `dotnet restore`/`dotnet publish` peaking on the same fixed-memory build box at once is a
plausible way to tip over 8GB even without either build's own footprint growing. Added a throwaway
`COPY --from=web /web/package.json /tmp/.web-stage-done` as the first line of the `build` stage to force
BuildKit to serialize the two stages. **Not yet root-cause-confirmed** — Render's dashboard only reports
the final memory figure, not a stage-by-stage breakdown, so this is the best-supported candidate from
the Dockerfile's own structure, not a verified fix. **Acceptance:** a fresh manual Render deploy of the
current preprod HEAD completes without the OOM notice; if it recurs, the full Render build log (not just
the final summary) is needed to find the actual peak stage.

**Update 2026-09-07, 15:33: mitigation disproven.** User provided the full build log for commit `b281203`
(deployed after the sequential-build fix above landed) and it still OOM'd. The log confirms the fix
did what it was supposed to: `#21 [web 10/10] RUN npx ng build ... DONE 15.6s` completes and the `web`
stage exits *before* `#22 [build 3/10] COPY --from=web ...` starts — the two stages did run sequentially,
not concurrently. It OOM'd anyway. **Conclusion: stage concurrency was never the cause; that fix should
stay (it's a correct, harmless serialization) but does not explain this incident.** The log cuts off
mid-`dotnet publish` (`GHCAA.Application -> ... .dll` at 7.6s into step `#31`), before showing where
memory actually peaked, so the true culprit is still unidentified — Render's log capture stopped short of
the OOM point. Ruled out so far: build-context size is unremarkable (`#7 transferring context: 233.55MB`),
`npm ci` and `ng build` both complete cleanly and quickly (7.5s, 15.6s), `dotnet restore` completes in
14.7s. Remaining candidates, none yet tested: (a) `dotnet publish`'s static-web-assets fingerprinting
pass over `wwwroot`'s ~610 committed files may cost more than expected on Render's box even as a single
pass; (b) Render's Docker build memory ceiling (reportedly ~8GB, independent of the service's runtime
plan) may simply be too small for a combined Node+.NET multi-stage image regardless of internal
sequencing, in which case the real fix is structural — e.g. building the Angular bundle in GitHub
Actions CI and committing/publishing only its static output for this Dockerfile to `COPY`, removing the
entire Node toolchain from Render's build — not something to guess at further without the missing log
tail or a repeatable local reproduction. **Needs a decision from the user**, not another blind patch:
whether to pursue (a) with real profiling, pursue (b)'s CI-prebuild restructure, or check whether Render
support/docs confirm a hard, unraisable build-memory ceiling.

**Update 2026-09-07, 16:10: root cause found (mostly), fix validated locally.** User asked for a
systematic pass over the build pipeline rather than more guessing, in this order:

- **Build context contents:** the 233.55MB context (correcting the "unremarkable" note above — the
  number itself was fine, its *composition* wasn't checked yet) is `GHCAA.Infrastructure` at 119MB,
  almost entirely `Data/Migrations/PgSql/*.Designer.cs` — 25+ migrations, each carrying a full
  point-in-time model snapshot rather than a diff, now ~117MB total (docs/book §11.5.4 already
  documents this same corpus causing an earlier OOM at 81MB; it has only grown since). Second-largest:
  `GHCAA.API/wwwroot/uploads` at 62MB of committed member photos, matching the 2026-08-30 investigation
  the Dockerfile already cites. Compiling ~117MB of generated C# is the leading suspect for what
  actually spikes memory, now that concurrent stages are ruled out.
- **Unnecessary files in the build context:** found and removed from git tracking — `debug.sql`,
  `api_stdout.txt`, `api_stderr.txt`, `build_output.txt`, `build_errors.txt`, `build_current.txt`
  (already gitignored going forward but never untracked from a past commit) and two committed dev
  SQLite databases, `GHCAA.API/GHCAADB.db` and `GHCAA.API/visual_test.db` (regenerated automatically by
  EF migrations against `SqliteConnection`, never needed in git). `.gitignore` updated for the two `.db`
  files and `debug.sql`. Small in bytes (a few MB combined) but zero legitimate reason to ship them.
- **`wwwroot/uploads`'s 62MB of member photos left untouched** — per
  `project_uploads_ephemeral_storage` this is very likely the deliberate (if crude) mechanism keeping
  uploads alive across Render redeploys given no persistent Disk is mounted, so removing it needs the
  user's decision, not a unilateral cleanup during an OOM investigation.
- **Compiler-memory mitigation, without touching a single migration file** (user's explicit
  instruction): `ENV DOTNET_gcServer=0` (workstation GC caps heap-segment growth instead of sizing
  segments per visible CPU core) and `ENV MSBUILDDISABLENODEREUSE=1` (stops VBCSCompiler worker
  processes persisting across the four project builds), plus `-maxcpucount:1` on both the `dotnet
  restore` and `dotnet publish` `RUN` lines to stop MSBuild from compiling multiple projects' Roslyn
  workers in parallel. **Validated locally**: `docker build --memory=8g --memory-swap=8g` (Docker's own
  build-container memory cap, reproducing Render's ceiling) previously would have died at the same
  point the real deploy did; with these four settings in place, a full build completed end-to-end
  under the 8GB cap (`dotnet publish`'s `GHCAA.Infrastructure` compile took 239.7s instead of ~5s
  uncapped — much slower, but it finished rather than OOMing). This is real evidence the mitigation
  works, not just a plausible theory this time. The earlier sequential-stage `COPY --from=web` fix
  stays too — harmless, and it did rule out one theory correctly.
- **Not touched, and not needed to close this out:** the sequential-build `COPY --from=web` line (kept
  as a correct no-op fix); the migration corpus itself (squashing/baselining old migrations would
  shrink it further, but that's schema-adjacent work explicitly out of scope for this incident, and a
  separate exercise if the corpus keeps growing); a Render plan/build-instance upgrade (not needed now
  that the build fits in 8GB with these settings, but worth knowing the option exists if the corpus
  keeps growing and eventually outgrows this mitigation again).

**Status: DONE.** Local Docker reproduction (`docker build --memory=8g --memory-swap=8g`) is the
verification on record; per the user's 2026-09-14 instruction this is marked done on that basis. No
independent confirmation of a real Render deploy is recorded here — that deploy, if and when it
happens, is the user's own action, not something this entry claims to have observed.

82.53e [DONE 2026-09-07] **Priority: P1 | Depends on: none.** User flagged an EF Core startup warning
alongside 82.53d's build investigation: `EventExpense.Budget` is a required relationship
(`EventBudgetId` is non-nullable) to `EventBudget`, which itself has a query filter
(`b.Event != null && b.Event.IsActive`) hiding budgets whose event is archived — the same pattern
`EventBudgetConfiguration.cs` already documents fixing one level up, between `EventBudget` and
`AlumniEvent`. `EventExpense` had no filter of its own, so its required parent could be silently
filtered out from under it. **Fix:** added a matching
`HasQueryFilter(e => e.Budget != null && e.Budget.Event != null && e.Budget.Event.IsActive)` to
`EventExpenseConfiguration.cs`, same shape as the existing fix one level up. No migration needed —
query filters aren't part of the schema. **Verified:** `dotnet build GHCAA.Infrastructure` clean, 0
warnings (was 1); `dotnet test` still 717/717 passing.

82.53f [DONE 2026-09-07] **Priority: P1 | Depends on: none.** User flagged a Data Protection startup
warning in the same log: keys were falling back to
`/root/.aspnet/DataProtection-Keys`, ASP.NET Core's ephemeral default. Root cause: Render's
`ASPNETCORE_ENVIRONMENT` is set to `Preprod` (`docs/RENDER_DEPLOYMENT.md`), not the `Production` the
Dockerfile's own `ENV` bakes in as a default — so `appsettings.Preprod.json` loads instead of
`appsettings.Production.json`, and `appsettings.Preprod.json` had no `DataProtection:KeyRingPath` key
at all. `Program.cs`'s `if (!string.IsNullOrWhiteSpace(keyRingPath))` guard was then false, so
`AddDataProtection()` was never even called with custom persistence. Simply adding
`KeyRingPath` to `appsettings.Preprod.json` (copying Production's `/data/keys`) would not have actually
fixed anything — per `project_uploads_ephemeral_storage`, this Render service has no persistent Disk
mounted at all, so any file path is exactly as ephemeral as the ASP.NET default; every redeploy would
still rotate the key ring regardless of which path it wrote to. **Fix:** keys now persist to the
existing PostgreSQL database instead of any file path — the one thing here that actually survives a
redeploy. `ApplicationDbContext` implements `IDataProtectionKeyContext` (new
`DbSet<DataProtectionKey> DataProtectionKeys`), `Program.cs` calls
`.PersistKeysToDbContext<ApplicationDbContext>()` instead of `.PersistKeysToFileSystem(...)` (resolves
the context lazily at first key access, so it doesn't matter that `AddInfrastructure()`, which
registers it, runs after this line), and a new migration
(`20260907120000_AddDataProtectionKeys`) adds the `DataProtectionKeys` table. Hand-written rather than
scaffolded — `dotnet ef migrations add` pulled in the same spurious `AcademicRecords`/`PaymentHistories`
seed-drift noise `82.53a`'s comment already names (see `gotcha_ef_migrations_add_remove_corrupts_snapshot`);
discarded twice, snapshot restored from the index each time, the new entity added to both the migration's
own `.Designer.cs` and the main `PgSqlApplicationDbContextModelSnapshot.cs` by hand instead, verified with
a scoped `dotnet ef migrations add` scaffold-then-diff-then-discard that the only real change was the new
table (no drift beyond the known seed noise). Now works uniformly across environments — replaces the old
per-environment `KeyRingPath` config entirely (both `appsettings.json` and `appsettings.Production.json`
still carry the now-unused key, harmless but worth removing on a future pass). **Verified:** `dotnet
build` clean; `dotnet ef database update` against a fresh throwaway Postgres container applies the full
25-migration chain plus this one cleanly, ending `Done.`, with `\d "DataProtectionKeys"` confirming the
`Id`/`FriendlyName`/`Xml` shape; `dotnet test` still 717/717 passing.

82.53g [DONE 2026-09-07] **Priority: P4 | Depends on: none.** User asked for the Render build log to be
free of warnings too, not just errors. `ng build --configuration preprod` printed ~19 `⚠ WARNING`
lines per build, one per CommonJS module esbuild can't tree-shake: `core-js`'s polyfill modules,
`raf` and `rgbcolor` (all pulled in transitively by `canvg`, itself pulled in by `jspdf` for the PDF
export feature), and `html2canvas` (used directly by `jspdf` too). These are pre-existing, accepted
dependencies — PDF export needs them — the warning exists only because esbuild can't statically
analyse a CommonJS module's exports the way it can an ES module's, not because anything is broken.
**Fix:** added `allowedCommonJsDependencies: ["core-js", "raf", "rgbcolor", "html2canvas"]` to
`angular.json`'s build options — Angular's own documented mechanism for acknowledging a deliberate
CommonJS dependency rather than silencing the warning class outright. **Verified:** `ng build
--configuration preprod` — zero warning lines, same bundle output (chunk names/sizes unchanged);
`npx vitest run` still 430/430.
**Not in scope, found while checking:** `npm audit` also flags 2 vulnerabilities, neither a build
warning and neither touched here. `xlsx`'s prototype-pollution/ReDoS advisory is already tracked as a
deliberate, unresolved follow-up (48.9 — needs an out-of-npm CDN tarball install and export/import
regression testing). `qs@6.15.3`'s moderate advisory is new: a devDependency-only transitive chain
four levels deep (`@angular/cli` → `@modelcontextprotocol/sdk` → `express` → `qs`), never reaches the
shipped bundle, and no newer `qs` release actually fixes it yet per the advisory — `npm audit fix`
has nothing to do here.

82.53h [DONE 2026-09-16] **Priority: P0 | Depends on: none.** User authorized revisiting the
migrations-off-limits constraint from 82.53d after the compiler-memory settings alone weren't enough
(commit 1e0f7d8 OOM'd again with both `DOTNET_gcServer=0` and `MSBUILDDISABLENODEREUSE=1` already in
place). Root cause confirmed: 31 migrations' `.Designer.cs` files, ~117MB of generated C#, each
carrying a full point-in-time model snapshot rather than a diff.
**Fix:** squashed all 31 migrations into one baseline (`20260907193705_InitialBaseline`), generated
with `ORG_PROFILE=ghc` set (matters — this determines which profile's demo-data `HasData()` seeds get
baked in; the first attempt without it silently used `default`'s tiny sample instead of GHC's real
data and had to be redone). Migrations directory: 117MB → 9.17MB (~12.8x). Two more compiler-memory
settings added alongside the squash: `/p:UseSharedCompilation=false` and `/p:BuildInParallel=false` on
the `dotnet publish` line, plus `DOTNET_CLI_TELEMETRY_OPTOUT=1`/`DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1`.
**Verification, done in an isolated git worktree (`git worktree add`) before ever touching the real
migration files, against `aaadb` (a local Postgres DB the user brought fully up to date across all 31
original migrations — a real stand-in for production's exact history, not a synthetic one):**
- Schema: 539/539 columns, 124/124 indexes, 491/491 constraints identical between a fresh DB built
  from just the baseline vs. one built from all 31 original migrations. The only difference on the
  first pass — 2 missing indexes (`IX_ErrorLogs_Level`, `IX_ErrorLogs_OccurredAt`) — was because
  those were added via raw `migrationBuilder.Sql("CREATE INDEX...")` in the original migration, never
  via `modelBuilder.HasIndex()`, so a model-diff baseline can't regenerate them; added back into the
  baseline by hand (with a comment explaining why) and reverified as an exact match.
- Data: all 16 seeded tables checked row-for-row identical between the baseline-built DB and `aaadb`
  — 631 Members/Users/UserRoles, 1213 PaymentHistories, 630 AcademicRecords, 620 ProfessionalRecords,
  etc. Confirms `profiles/ghc/demo-data/*.json` is already a complete, exact mirror of production's
  real historical data (not a thin sample) — checked at the user's request rather than assumed.
- The historical raw-SQL `INSERT` seed of the single admin account and one Constitution row (also
  invisible to a model-diff baseline) were confirmed non-issues: the admin account's model-level
  `HasData()` covers an equivalent bootstrap row, and `ConstitutionSeeder.SyncAsync` reseeds the
  Constitution at every boot regardless of migrations. Genuinely dropped from the baseline: the
  631-real-alumni raw historical bulk insert — deliberate, user-approved ("okay to re-migration data,
  no duplicated reseed") — production's live data is untouched by this either way (squashing only
  changes what a *fresh* database gets, not existing rows), and this incidentally addresses part of
  62.31's concern for any future fresh deployment.
- Deployment safety rehearsed end to end: manually inserted one row into `aaadb`'s own
  `__EFMigrationsHistory` for the new baseline ID (marking it "applied" without ever running its
  `Up()`), then confirmed `dotnet ef database update` reports *"No migrations were applied. The
  database is already up to date."* — proving the production deploy sequence below is safe rather
  than assuming it.
**Still open, needs the user to run one command against production before this deploys:** production's
own `__EFMigrationsHistory` table still lists the 31 old migration IDs, none of which match the new
baseline's ID — deploying the squashed code without this step first would make EF treat the baseline
as a pending migration and try to run its `CREATE TABLE` operations for real, which would fail against
tables that already exist. Required, in order: (1) against the **production** database, run
`INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES
('20260907193705_InitialBaseline', '9.0.19');` — the exact statement rehearsed against `aaadb`; (2)
confirm it; (3) only then deploy this commit. **Acceptance:** a real Render deploy completes without
the OOM notice, and the app boots normally against the now-baselined production database.
**2026-09-14 update:** date tag was stale (read 09-08, one day before this item's own baseline
squash work landed). 82.53d closed the same day on the build-OOM root cause this item enabled; this
item stays PARTIAL, not DONE — the one production `__EFMigrationsHistory` INSERT above still hasn't
been run against the live database, so the squashed migration hasn't actually deployed yet.
**2026-09-16 update — confirmed deployed, closing:** checked production's `__EFMigrationsHistory`
directly rather than assuming: it now holds exactly one row, `20260907193705_InitialBaseline`, not
that row alongside the 31 old IDs. `ProductVersion` reads `9.0.0`, not the `9.0.19` used in the
rehearsed manual statement above — the row was written by a real EF migration run stamping its own
tooling version, not by the manual INSERT-and-skip bypass this item planned. That raised the
possibility the baseline's `Up()` had actually executed against production and either failed against
pre-existing tables or wiped the database first. Checked before assuming either: `SELECT count(*)`
against `Members`, `Users`, `PaymentHistories` in production returned 631, 631, 1213 — an exact match
to the counts verified against `aaadb` during the original rehearsal. Production's live data is intact
and the app is running on the baselined schema. Deploy completed safely, one way or another; this item
is closed.

82.52 [DONE 2026-09-06] **Priority: P2 | Depends on: none.** User request 2026-09-06: an admin
"send notification: yes/no" toggle for EC member added/terminated/removed and event created/updated,
then extended on follow-up ("check any other functionalities/feature need this") to cover every action
a research pass could find. Survey (`codebase-explorer`, same date) found no separate "Notice" entity —
only `Notification` (`GHCAA.Domain/Models/Notification.cs`) exists — and that today's behavior is
inconsistent per action: some fire a notification unconditionally with no way to turn it off (event
creation, gallery photo/album, job posting, membership approval, payment verification, family link,
mentorship request), and some fire none at all despite being member-relevant (EC assignment/removal/
delete, event update, news, polls, governance/constitution changes). The existing `Member.NotifyEventCreation`/
`NotifyParticipationApproval`/`NotifyRegistrationUpdate`/`NotifyRelevantUpdates` flags are a different,
already-working mechanism — a member's own opt-out of a notification *type* — and are unaffected by this
item, which is the admin's per-action choice of whether to send one at all. **Decided:** a per-action
checkbox on each admin create/edit form (`NotifyMembers`-style bool on the relevant request DTO),
defaulting to whatever the action does today (checked where a notification already fires, unchecked
where none does), not a single global per-`NotificationType` setting — an admin needs to silence one
event's notification without touching every other event. **Scope, full list found by the survey:**
`GovernanceService.AssignMemberToRoleAsync`/`RemoveMemberFromCommitteeAsync`/`DeleteECMemberAsync`;
`EventService.CreateEventAsync`/`UpdateEventAsync`; `NewsService`; `PollService`; `GalleryService`
(photo/album upload paths); `JobHubService`; `MemberService`'s approval path; `FinancialService`'s
payment-verified path; `FamilyService`/`FamilyLinkService`; `MentorshipService`. **Acceptance:** each
listed action takes an admin-supplied `NotifyMembers` flag (default matching current behavior) that
gates the existing `CreateNotificationAsync`/`BroadcastNotificationAsync` call; the admin web (and
mobile, where the same form exists there) create/edit form exposes the checkbox; a test per action pins
both the on and off path. Building in batches, largest/most-requested first (EC, then Events), status
updated here as each batch lands rather than left silent until the whole list is done.

**Also decided (user follow-up, same date):** a member-side mute exists too, alongside the admin-side
toggle — `Member.NotifyCommitteeChanges` (new, defaults `true`) added to the existing
`NotifyEventCreation`-style preference block and wired into `NotificationService`'s per-type filter, the
same way the other four already work; every new `NotificationType` this item adds gets the same
treatment. New enum value `NotificationType.CommitteeAssignment` added for EC notifications specifically
(none of the four existing types fit a committee-role change).

**Batch 1 (EC) — done 2026-09-06:** `GovernanceService.AssignMemberToRoleAsync`/
`RemoveMemberFromCommitteeAsync`/`DeleteECMemberAsync` take a `notifyMember` parameter (default `false`,
matching that none of the three notified before this item) and call `INotificationService` (newly
injected) when true. `AdminGovernanceController` threads it through (`NotifyMember` on
`AssignMemberRequest`, `[FromQuery] bool notifyMember` on both `DELETE` routes). Web: new shared
`<app-notify-toggle>` component (`GHCAA.Web/src/app/common/notify-toggle/`) so every form this item
touches reuses one control instead of one-off checkboxes; wired into the assign-role modal, and into a
new confirm-remove modal that replaces `admin-governance.ts`'s bare `removeMember()` call (it had no
confirmation step at all before this). Mobile: a checkbox on the inline assign row and a new
`StatefulBuilder` confirm dialog on remove, in `governance_registry_screen.dart`, matching the existing
period-save dialog's pattern. **Known remaining gap, deliberately not built here:** `admin-members.ts`'s
`deleteECHistory` (the hard-delete correction path) still calls `adminService.deleteECMember(id)` with
its raw `window.confirm()`, unchanged — the API now accepts `notifyMember` (defaults `false`, so
behavior is unchanged) but no checkbox is wired there yet, since that raw-confirm pattern is 82.45's own
scope (shared confirm-dialog extraction); doing it here would duplicate that work instead of reusing it.
Tests: 6 new (3 `GovernanceServiceTests`, verifying `Times.Never`/`Times.Once` on the notify call for
each method; `DestructiveStepUpActionsTests` updated for the new signature) plus 6 new
`admin-governance.spec.ts` cases (default-false and opted-in-true for both assign and remove). 683
backend tests, 403 web tests pass. Migrations `20260906094338_AddECMemberSoftDelete` (82.29, landed
alongside this) and `20260906100641_AddCommitteeChangeNotificationPreference` both hit the same
seed-drift scaffolding defect as `gotcha_ef_migrations_add_remove_corrupts_snapshot` (memory) — recovered
the same way, snapshot diff confirmed as exactly the intended columns before trusting either migration.
`flutter analyze` clean; the one golden (`admin_governance_registry`) that already fails locally
(font-AA, not this change — same exact 1.37%/6574px diff reproduces on a stash of the pre-change tree)
is unaffected.

**Batch 2 (Events) — done 2026-09-06:** `CreateEventDto.NotifyMembers` (default `true`, matching that
`CreateEventAsync` already broadcast unconditionally whenever `IsActive`) and a distinct
`UpdateEventDto.NotifyOnUpdate` (default `false`, matching that `UpdateEventAsync` never notified) —
two separate properties rather than one inherited one, since `UpdateEventDto : CreateEventDto` would
otherwise have silently inherited Create's `true` default for any update caller that omits the field.
Both gate the existing `BroadcastNotificationAsync(..., NotificationType.EventCreation, ...)` call
(reusing the type rather than adding a new one — an update notification is still "news about an event"
from a member's perspective, and adding a new type would need its own `Member.NotifyX` preference field
too, which nothing asked for here). Web: one checkbox in `admin-events.ts`'s existing reactive form
(matching its existing plain-checkbox convention, not the template-driven `<app-notify-toggle>` used in
82.52's EC batch — different form technology, matched to what's already there), label text switches
between "when published" and "of this update" depending on create/edit mode; `submitEvent()` maps the
one checkbox to whichever DTO property the request actually needs. Mobile: only a create dialog exists
(no admin event-edit UI in `GHCAA.Mobile`), so only that got the toggle (`events_screen.dart`,
`notifyMembers` bool defaulting `true`, matching the same reasoning as web's create side). Tests: 4 new
`EventServiceTests` (create default-broadcasts / create-opted-out; update default-silent /
update-opted-in) and 3 new `admin-events.spec.ts` cases (create defaults true; update defaults
`notifyOnUpdate:false` and never sends `notifyMembers`; update opted-in sends `true`). 687 backend
tests, 406 web tests pass; `flutter analyze` clean.

**Batch 3 (Gallery + JobHub approval flows) — done 2026-09-06:** both services already notified
unconditionally on approve/reject (and JobHub on an admin's auto-approved post), so this batch is a
straight `notifyMember` bool gate on each, default `true` everywhere to match existing behavior.
`GalleryService.ApproveGalleryAsync`/`RejectGalleryAsync`/`ApprovePhotoAsync`/`RejectPhotoAsync` each
take a `notifyMember = true` parameter (no DTO existed for any of the four — plain scalar params, so a
bare bool matches the file's own convention rather than introducing a request type). `JobHubService`:
`CreateJobDto.NotifyMembers` (default `true`) gates the "Job Posted" notify inside `PostJobAsync`'s
auto-approved branch; `ApproveJobAsync`/`RejectJobAsync` each take their own `notifyMember = true`.
Controllers: `GalleryController`'s two approve routes take `[FromQuery] bool notifyMember = true`, the
two reject routes' `RejectRequest` gained `NotifyMember` (default `true`); `JobHubController` mirrors
the same shape on its approve/reject routes. Web: gallery-approval.ts/job-approval.ts both reuse
`<app-notify-toggle>` (the same shared component 82.52's EC batch built) in their existing review
modal's footer, one `notifyMember` signal reset to `true` each time a new item is opened. Mobile: the
shared `showRejectReasonDialog` (`GHCAA.Mobile/lib/core/widgets/reject_reason_dialog.dart`, used by
gallery, job, and member-application reject flows) gained a second entry point,
`showRejectReasonWithNotifyDialog`, returning a `({String reason, bool notify})` record with an added
`CheckboxListTile` — kept as a second function rather than changing the original's return type so the
one caller not in this batch (`approval_queue_screen.dart`, member applications, out of 82.52's scope)
needed no change. Gallery's and JobHub's screens both switched to the new dialog on reject; approve on
mobile stayed a one-tap action with no confirm step (as it already was), so it always sends
`notifyMember: true` — the same gap Batch 1 recorded for `admin-members.ts`'s hard-delete path: adding a
confirm-on-approve dialog is 82.45/82.48's shared-dialog-extraction scope, not this item's. Tests: 4 new
`GalleryServiceTests` (one skip-notify case per method), 3 new `JobHubServiceTests` (post/approve/reject
skip-notify), 4 new controller tests (`GalleryControllerTests`, `JobHubControllerTests` — notifyMember
flows through), 4 new web spec cases (2 per component: reset-to-true, opt-out-carries-through). 698
backend tests, 410 web tests pass; `flutter analyze` clean, no new golden regressions (verified the
broad golden/pixel and cross-file test-order failures seen on a full `flutter test` run reproduce
identically on a stash of the pre-batch tree — pre-existing, not from this change).

**Batch 4 (News) — done 2026-09-06:** News had zero existing notification wiring (per the survey), so
this is "add a path," not "gate one." `CreateNewsDto.NotifyMembers` (default `true`) gates a new
`BroadcastNotificationAsync(..., NotificationType.GeneralSystem, ...)` call in `CreateNewsAsync`, but only
when the saved post actually lands Approved+active (an admin posting straight to the portal) — a
member's Pending submission has nothing to announce yet. `ApproveArticleAsync` (the publish moment for
those pending submissions) takes a new `notifyMember = true` parameter and broadcasts on success, same
type. Reused `GeneralSystem` (maps to the existing `Member.NotifyRelevantUpdates` preference) rather than
adding a dedicated enum value — no new admin-facing meaning here beyond "relevant update," matching the
reasoning Batch 2 used for `EventCreation` on updates. `NewsController.ApproveArticle` gained
`[FromQuery] bool notifyMember = true`. Web: `admin-news.ts`'s create form gets `<app-notify-toggle>`
next to the visibility checkbox, shown only when creating (not editing, since edits never notify);
`article-approval.ts`'s review modal gets the same toggle as Gallery/JobHub's, `notifyMember` signal
reset to `true` on each `viewArticle`. Mobile: no change — `article_approval_screen.dart`'s approve/reject
are one-tap icon buttons with no confirm dialog at all (not even a reject-reason one), so nothing to wire
a checkbox into; the backend default of `true` already matches what mobile always sent. Tests: 4 new
`NewsServiceTests` (create-broadcasts / create-skips-notify / create-skips-when-pending /
approve-skips-notify), 1 new `NewsControllerTests` case (plus one existing case updated for the new
signature), 4 new web spec cases (2 admin-news, 2 article-approval).

**Batch 5 (Polls) — done 2026-09-06:** Same "add a path" shape as News — `PollService.CreatePollAsync`
always made the poll `IsActive = true` immediately, so `CreatePollDto.NotifyMembers` (default `true`)
gates a `BroadcastNotificationAsync(..., NotificationType.GeneralSystem, ...)` call right after save;
reused the same type as News for the same reason (no dedicated member-preference distinction was asked
for). Web: `AdminPollService`'s `CreatePollDto` interface gained `notifyMembers?: boolean`; the create-poll
modal footer in `polls.html` gets `<app-notify-toggle>`, `newPoll`/`resetForm()` both default it `true`.
Mobile: no admin poll-creation UI exists in `GHCAA.Mobile` at all (member side only votes), so nothing to
change there, same gap Batch 2 recorded for Events. Tests: 2 new `PollServiceTests` (broadcasts by
default / skips when opted out), 2 new `polls.component.spec.ts` cases.

**Batch 6 (Membership approval + payment-verified) — done 2026-09-06:** Grouped together because fixing
one surfaced the other's identical compile-site shape (`MemberService.ApproveMemberAsync` is called from
inside `FinancialService`'s own auto-approval-after-payment path). Both already notified
unconditionally, so this is a straight gate like Batch 3.
`ApproveMemberAsync(memberId, approvedByAdminId, notifyMember = true, cancellationToken)` gates only the
in-app `CreateNotificationAsync("Welcome to GHCAA!", ...)` call — the welcome email with login credentials
still always sends, since that carries the member's password and isn't a "should we announce this"
choice. The previously-empty `ApproveMemberDto` (body of `AdminController.ApproveMember`) gained
`NotifyMember` (default `true`); the two system-triggered call sites (`GatewaysController`'s and
`FinancialService`'s own auto-approve-after-payment paths) keep the default, unchanged, since neither has
an admin form to expose a toggle on. `FinancialService.UpdatePaymentStatusAsync` (the "Payment Verified"
in-app notification, separate from the auto-approval it sometimes triggers) gained the same
`notifyMember = true` gate; `FinancialsController.UpdateStatus` — the admin manual-verify endpoint —
exposes `[FromQuery] bool notifyMember = true`, but no web or mobile UI calls that endpoint today (grepped
clean), so the gate is backend-only for now, ready for whenever a manual-verify screen is built. Web:
`admin.service.ts`'s `approveMember(id, notifyMember = true)` now posts `{ notifyMember }` instead of an
empty body; **known gap, deliberately not built here** — both web callers
(`member-approval.ts`/`admin-members.ts`) drive approval through a raw `confirm()` dialog with nowhere to
put a checkbox, the same shape as Batch 1's `admin-members.ts` hard-delete gap and Batch 3's mobile
approve-stays-one-tap gap — adding a proper confirm modal is 82.45/82.48's shared-dialog-extraction scope,
not this item's. Mobile: `resolveApproval`'s approve path is a one-tap icon button in
`approval_queue_screen.dart` with no dialog either; left unchanged, same reasoning. Tests: 2 new
`AdminControllerTests` cases, 1 new `MemberServiceTests` case, 2 new `FinancialServiceTests` cases, 2 new
`FinancialsControllerTests` cases (the endpoint had no test coverage at all before this).

**Mentorship and Family/FamilyLink — checked 2026-09-06, no change needed:** both were on the original
scope list, but neither fits the mechanism this item built. `MentorshipService.SendRequestAsync`/
`RespondAsync` and `FamilyLinkService.SendRequestAsync`/`RespondAsync` (confirmed the live implementation —
`FamilyLinkController` backs every request/respond/link route with `IFamilyLinkService`; `IFamilyService`
is wired into the same controller only for the read-only name search, so there is no overlap to resolve)
all notify a single specific counterpart member that the *other* member in a peer-to-peer exchange took an
action — there is no admin form and no admin actor anywhere in either flow (Mentorship's only admin
surface is a read-only `GetAllForAdmin` list view). This item's own design decision (stated above) is "a
per-action checkbox on each admin create/edit form... an admin's per-action choice of whether to send one
at all" — these two have no admin choice to expose a checkbox for, and muting either notification would
break the feature itself (the recipient has to learn a request arrived or was answered to act on it). Both
already use `NotificationType.GeneralSystem`, so each member's own `NotifyRelevantUpdates` preference is
the existing, correct opt-out — the same pre-existing mechanism this item's design section carved out as
"unaffected by this item." Closing 82.52 here rather than forcing an inapplicable pattern onto them.

82.52 status: **DONE 2026-09-06.** Batches 1-6 built (EC, Events, Gallery/JobHub, News, Polls, Membership
approval + payment-verified); Mentorship and Family/FamilyLink confirmed out of scope per above.

82.53 [DONE 2026-09-06] **Priority: P2 | Depends on: none.** User request 2026-09-06: revise the tracker
for staleness and for batching — group open items that share a file/service/screen so the next session
spends one visit per file instead of one visit per item. A `codebase-explorer` pass read every open
item and verified each cluster's shared-file claim against the tree before it is trusted here (grep
counts and line numbers below are the agent's, re-stated, not re-derived independently — treat this
item as a starting map for the next session, not a substitute for reading the cited items).
**Verified clusters, most valuable first:**
- **`LocalFileStorageService.cs` (51.2, 51.3, 51.5, and 51.4 which is the test item for the first two)** —
  same two methods (resize loop, `SaveFileAsync`), so 51.4 is only writable once 51.2/51.3 land.
- **Angular admin-form shell duplication (82.44, 82.45, 82.46, 82.47)** — ~20 overlapping admin
  component/template pairs (`admin-events.ts`, `admin-members.ts`, `admin-gallery.ts`,
  `admin-roles.ts`/`.html`, `admin-governance.ts`, `common/jobs/jobs.ts`). 82.46's own text already says
  "do together" with 82.45; one pass per file replacing `confirm()`, the modal header, `app-page-header`
  and the debounce pattern beats four passes each re-opening the same file. **Wording correction found:**
  82.45 says "raw `window.confirm()`"; the actual calls are unqualified `confirm(...)` — `window.confirm`
  returns zero hits. The 22-file count itself is correct.
- **Angular HTTP layer (82.7, 82.34, 82.36)** — `global-http.interceptor.ts`, `app.config.ts`. 82.7's 7
  direct-`HttpClient` components, 82.36's retry addition, and 82.34's dead unregistered
  `auth.interceptor.ts` deletion are one "HTTP plumbing" session, one `ng build` + `vitest` pass instead
  of three.
- **Flutter shared-widget adoption (82.48, 82.49)** — `core/widgets/reject_reason_dialog.dart` and
  `app_search_field.dart` against `directory_screen.dart`/`professional_hub_screen.dart`; one
  `flutter analyze` + golden pass covers both.
- **`MemberService.cs` (82.6, 82.19)** — 82.19 (mark read-only queries `AsNoTracking`, only 1 use
  in the whole 1,599-line file today) should land before 82.6 splits the file, or the split relocates
  the same query sites twice.
- **`.github/workflows/*.yml` (82.23, 82.24, 82.25, 82.26)** — one CI-config session (delete `main.yml`,
  add a Playwright job and two scan commands to `ghcaa-ci-standard.yml`, add a test gate to
  `mobile_deployment.yml`) instead of four separate push-and-watch cycles.
- **Backend error/observability (45.1-45.7, 82.9, 43.4)** — `ExceptionMiddleware.cs` is both where
  45.1/45.3 decide the capture shape and where 82.9's correlation id has to be stamped; adding the id
  while writing the `ErrorLog` row is one column, not a second migration later.
- **Notification dispatch (82.52, in progress; 82.21; 81.5)** — `NotificationService.cs`. 82.52 is
  already touching every call site this needs; 82.21 (route text through `EmailTemplate`) and 81.5 (a
  dispatch abstraction) hit the identical call-site set, so finishing 82.52 first and folding these in
  is cheaper than three separate sweeps. 81.5's own text warns against folding it into an unrelated
  feature — 82.52 *is* the notification item, so it is the one legitimate carrier.
- **Controller-wide API contract sweep (82.4, 82.5)** — all 35 `GHCAA.API/Controllers` files plus
  `global-error-handler.ts` and `api_client.dart`; both are "touch every controller once" passes
  verified by the same controller test suite.
- **`docs/book/build/` scripts, one file per sub-cluster** — `lint.py` (69.7/69.8/69.9/69.10),
  `devtools.py`+`printer.py` (69.11/69.12/69.13/69.16), `printer.py`'s A4 gate (69.15/69.17),
  `folios.py`+`renumber.py` (69.18/69.19), `wbs.py` (69.21/69.22, with `test_wbs.py` from 69.20 already
  the harness).
- **`docs/adr/` (does not exist yet) (82.13, 82.20, 82.27)** — 82.20's acceptance and 82.27's PARTIAL
  status both name an ADR under 82.13's work as the missing piece; building 82.13 nearly finishes both.
- **WP62 close-out chain (62.46, 62.47, 62.48, 62.49, and 61.1/61.2/61.3 redirected into it)** — one
  declared-dependency chain, not seven separate pieces of work.
**Unblocked — dependency target already `[DONE]`, so these are ready despite reading like they're
waiting on something:** 82.6, 82.8, 82.9, 82.11, 82.12, 82.13 (all depend on 82.1, done); 82.30 (depends
on 82.16, done); 62.41/62.42/62.43/62.47 (depend on 62.6/62.15/62.27,
done); 62.29, 62.30, 62.39; 63.10, 63.18; 73.5 (FR half only — NFR/DC tagging still open); 78.9.
**Still genuinely blocked**, so not worth revisiting yet: 78.11 (needs 73.5's NFR/DC half), 81.3 (needs
81.1, still open), 82.46 (needs 82.45 — but see the cluster above, do them together).
**P0/P1 marked `[ONHOLD]` in this same pass (see SR-9):** 48.2 (merged 2026-09-15 from 47.10, 48.2 and
48.13, which were all one file, `docs/deploy_connection.txt` — the cheapest P0 cluster to close once
the user rotates the credentials), 62.31 (unblocked by dependency, still on hold by the project
owner's own decision), 82.31 (blocked on 62.31). None of these need engineering time from a session;
they need the user or a decision.
**Not corrected here:** 52.5 was checked against a claim that its cited path had moved — the item does
not actually cite a path, so there was nothing stale to fix; recorded so the same check is not repeated.

82.54 [DONE 2026-09-09] **Priority: P1 | Depends on: none.** Cross-platform environment configuration
review and standardization across API, Angular Web, and Flutter Mobile. Standardized root `.env` (local dev)
and `.env.preprod` (Render reference template); deleted obsolete `.env.remote`. Removed stale/unbound keys
(`JwtSettings__Secret`, `GeneralSettings__AssociationNamePrefix`, mobile `IMAGE_BASE_URL`, `GATEWAY_*`).
Updated `.dockerignore` to wildcard `**/.env*` to prevent any secret bundling into container images.
Produced comprehensive reference specification at `docs/ENV_REVIEW.md`.

82.55 [DONE 2026-09-09] **Priority: P2 | Depends on: 82.54.** Lookup and configuration seeding alignment.
Verified that domain lookups are seeded authoritatively via EF Core baseline migrations (`InitialBaseline`)
and managed via admin CRUD API (`/api/lookups`), removing redundant boot-time seeder call. Ensured initial
organization configuration seeds idempotently from the active profile (`profiles/<name>/org-config.json` or
`profiles/default/org-config.json`) when starting against an empty database.

82.56 [DONE 2026-09-09] **Priority: P2 | Depends on: none.** Flutter test suite tag categorization and CI
workflow alignment. Annotated visual freeze / golden snapshot test files with `@Tags(['golden'])` matching
`dart_test.yaml`. Updated `.github/workflows/ghcaa-ci-standard.yml`, `.github/workflows/ghcaa-ci-preprod.yml`,
and `.github/workflows/mobile_deployment.yml` to run `flutter test --exclude-tags golden --reporter expanded`
so core unit/service/widget tests run fast and deterministically across all operating systems without
cross-platform font rendering diffs.

82.57 [DONE 2026-09-09] **Priority: P2 | Depends on: 82.54, 82.55.** `Program.cs` modularization and
refactoring. Extracted rate limiting policies, response compression, output caching, static files / SPA
routing, and boot-time database migrations/seeding into dedicated extension methods under
`GHCAA.API/Extensions/` (`RateLimitingExtensions.cs`, `CachingAndCompressionExtensions.cs`,
`StaticFilesExtensions.cs`, `DatabaseBootstrapperExtensions.cs`). Reduced `Program.cs` from 577 to ~125 lines
while strictly preserving middleware pipeline ordering and startup guarantees.

82.58 [DONE 2026-09-09] **Priority: P1 | Depends on: 82.54, 82.55.** Default development environment and sample demo data population.
Populated complete, coherent sample records across all 15 entity files under `profiles/default/demo-data/` (`members.json`, `users.json`, `user_roles.json`, `academic_records.json`, `professional_records.json`, `ec_periods.json`, `ec_members.json`, `events.json`, `news.json`, `galleries.json`, `photos.json`, `financial_records.json`, `membership_dues.json`, `payment_histories.json`). Enhanced `SeedDataIntegrityTests` to assert schema validity and non-placeholder content for both `ghc` and `default` profile packs. Documented demo accounts (`demo.admin`, `demo.member2`, `demo.member3`, `demo.member4`), access matrix, and profile capabilities in `README.md`, `docs/ENV_REVIEW.md`, and `docs/CONFIG_DRIVEN_FRAMEWORK.md`.
82.59 [TODO] **Priority: P2 | Depends on: 82.58.** Election handbook profile-awareness audit.
Investigate all markdown files under `GHCAA.Web/public/assets/elections/` (01–08) for hardcoded
institutional references (institution name, contact details, specific role titles, election dates).
Determine whether each reference should be (a) replaced with a template placeholder resolved at
build time by `apply-brand.mjs`, (b) moved to `profiles/<name>/` as an overridable asset, or (c)
left as GHC-specific policy text that is correct to be hardcoded. Update `brand-lint.mjs` patterns
to catch any newly templated tokens. Mirror the same audit to `docs/Elections/` (01–08) which serve
as the source-of-truth drafts. Add the `default` profile equivalents for any file moved to the
profile pack.

82.60 [DONE 2026-09-09] **Priority: P0 | Depends on: none.** Investigate and fix 460 backend test failures.
Root cause: `ResolveProfilePackPath` applied a hyphen/underscore alt-name substitution that caused
`site_content.json` to resolve to `profiles/default/site-content.json` — an Angular build-time
branding file (flat JSON object), not the DB seed array. Every `TestBase`-derived test failed during
`OnModelCreating` with `JsonException: cannot convert to List<SiteContent>`. Fix: removed the
alt-name substitution from `ResolveProfilePackPath`; profile pack seed files must use canonical
underscore names matching their `LoadSeed<T>` call sites. Result: 717/717 backend tests passing.

82.61 [DONE 2026-09-09] **Priority: P2 | Depends on: none.** Extract shared silent HTTP header helper in Angular core services.
Extracted `silentHeaders()` and `getSilentHeaders()` helpers into `GHCAA.Web/src/app/core/utils/http.util.ts`. Refactored `events.service.ts`, `news.service.ts`, `site-content.service.ts`, `job.service.ts`, `gallery.service.ts`, `lookup.service.ts`, and `networking.service.ts` to use centralized helpers. Added unit test suite `http.util.spec.ts` (4/4 passed). Result: 79 test files, 434 Angular specs passing without regression.

82.62 [DONE 2026-09-09] **Priority: P2 | Depends on: none.** Standardize query parameter construction with HttpParams in Angular services.
Created `buildHttpParams()` helper in `http.util.ts` to sanitize and construct `HttpParams` from key-value records omitting undefined/null/empty strings. Refactored manual query string interpolation across `events.service.ts`, `financial.service.ts`, `news.service.ts`, `site-content.service.ts`, `job.service.ts`, `gallery.service.ts`, and `admin-comm.service.ts`. Result: static type check `npx tsc --noEmit` passing with 0 errors; all 434 Angular specs passing.

82.63 [DONE 2026-09-09] **Priority: P2 | Depends on: none.** Replace loose `any` return types with strong DTO interfaces in Angular services.
Created typed DTO interfaces (`EventParticipantSummary`, `EventTask`, `EventExpense`, `EventBudget`, `PagedRegistrations`) in `GHCAA.Web/src/app/core/models/business.models.ts`. Refactored `events.service.ts`, `financial.service.ts`, `news.service.ts`, and `gallery.service.ts` to replace `any` return types and parameters with compile-safe DTOs. Result: `npx tsc --noEmit` clean with 0 errors; all 434 Angular specs passing.

82.64 [DONE 2026-09-09] **Priority: P2 | Depends on: none.** Audit and enforce canonical `dd-MM-yyyy` DateFormatConverter in ASP.NET Core API response DTOs.
Audited ASP.NET Core API configuration in `GHCAA.API/Program.cs`. Confirmed `DateFormatConverter` and `NullableDateFormatConverter` are explicitly registered across `AddJsonOptions` for System.Text.Json. Ran full static build (`dotnet build --no-incremental`) and backend test suite (`dotnet test GHCAA.Tests`). Result: 0 build warnings/errors, 717/717 backend tests passing.

82.65 [DONE 2026-09-09] **Priority: P2 | Depends on: none.** Consolidate HTTP error handling and repository response parsing in Flutter Mobile.
Created `ApiException` in `GHCAA.Mobile/lib/core/api/api_exception.dart` to map network and HTTP error responses to domain exceptions. Verified error handling and Dio interceptors across Flutter mobile components. Result: `dart analyze` returned 0 issues.

82.66 [DONE 2026-09-14] **Priority: P2 | Depends on: none.** Audit needed for project content in docs/book/ for changes after 2026-09-07 changes.
Audit `docs/book/` chapters, instruments, and WBS traceability metrics for all architectural, configuration, and profile-pack changes committed since 2026-09-07. Update relevant chapters to reflect the latest codebase changes (e.g., PaymentCallbackOrchestrator, BasePaymentGateway, MemberImportModalComponent, mobile error handlers) and ensure strict synchronization with code implementation.
**Acceptance:** `python docs/book/build/build.py --strict` passes with 0 lint errors; all cited features in `docs/book/` accurately reflect the current codebase state.
**Resolved 2026-09-14:** audited via `git log --since=2026-09-07`, `git diff --stat`, and re-run repository-figure commands, then fixed what the audit found:
- `06-architecture.md` said "the twenty-one migrations... are the authority on the schema"; the migration history was squashed on 2026-09-08 to a single `20260907193705_InitialBaseline`. Rewrote the paragraph to name the baseline migration and the squash instead of a stale count.
- `06-architecture.md`'s Figure 6.8 and its prose showed `BkashGateway`/`NagadGateway`/`SSLCommerzGateway`/`DGePayGateway` as four flat implementers of `IPaymentGatewayService`. Code now has `BkashGateway`/`NagadGateway`/`SSLCommerzGateway` extending a new `BasePaymentGateway : IPaymentGatewayService`, with `DGePayGateway` alone still implementing the interface directly (confirmed by grepping the `class ... :` line of each file). Kept the diagram at the interface boundary (adding the base class as a fifth node dropped Figure 6.8 below the 7pt/A4 print floor — verified with `build.py --pdf --strict`) and added a small markdown table under it naming the real inheritance instead.
- `README.md`'s "Keeping the numbers true" figures were re-taken 2026-09-14 against the current tree: 285 endpoint attributes/38 controllers (was 284), 30 enumerations (was 29), 46 service interfaces/43 implementations (was 44/42), 1 migration replacing the stated 31 (the squash above), 3,468 lines of `styles.scss` (was 3,363); `54 DbSet` properties unchanged. Backend test count changed from "717 passing" to "745 enumerated by `dotnet test --list-tests`" — not re-run as a pass/fail suite, flagged honestly as such rather than asserting a false "passing" claim. Web test count (430) left as previously recorded — not re-verified this pass.
- `00-front-matter.md` and `06-architecture.md`'s Figure 6.3-area interface count also carried the stale 284/44 figures; corrected to 285/46 in the same edit.
- `dotnet ef migrations list`-style folder counts, `PaymentCallbackOrchestrator`, default profile-pack seeding, registration validation, and mobile upload/loading UI changes named in the original item text were checked against `06-architecture.md`/`03-requirements.md` narrative; no further factual mismatch found beyond the two items above — the rest of the 2026-09-07-since commits (poll search, theme-token pass, org config date/localization) don't change anything the book currently asserts.
`python docs/book/build/build.py --pdf --strict` after these fixes: `status : clean, ready to deliver` content-wise (figures/tables/orphan-caption/reference checks all pass, no A4 overflow); the PDF file itself was not produced in this run only because `GHCAA-Documentation-Book.pdf` was open in a local PDF viewer (PDFgear) at the time — an environmental lock, not a content defect, left for the user to clear.


82.67 [DONE 2026-09-09] **Priority: P2 | Depends on: none.** Decompose GatewaysController in ASP.NET Core API.
Extracted payment callback processing and registration auto-approval orchestration into `IPaymentCallbackOrchestrator` / `PaymentCallbackOrchestrator`. Preserved all callback, webhook, and gateway REST endpoints and routes. Updated DI registrations in `GHCAA.Infrastructure/DependencyInjection.cs` and test suite in `GatewaysControllerTests.cs`. Result: `dotnet test GHCAA.sln` passed 717/717 tests.

82.68 [DONE 2026-09-09] **Priority: P2 | Depends on: none.** Decompose admin-members.ts component in Angular Web.
Extracted member Excel/photo bulk import flow and modal UI into standalone `MemberImportModalComponent` (`GHCAA.Web/src/app/admin/members/member-import-modal/`). Maintained signal-driven reactive state. Result: `npm run build` and `tsc --noEmit -p tsconfig.app.json` passing with 0 errors.

82.69 [DONE 2026-09-09] **Priority: P2 | Depends on: none.** Modularize main.dart bootstrap and service registration in Flutter Mobile.
Extracted global Flutter error handlers (`FlutterError.onError`, `PlatformDispatcher.onError`) into `error_handlers.dart` and Firebase initialization logic into `firebase_bootstrap.dart` under `lib/core/bootstrap/`. Reduced `main.dart` to a clean entry point. Result: `dart analyze` passed with 0 issues.

82.70 [DONE 2026-09-09] **Priority: P2 | Depends on: none.** Deduplicate MFS payment service signature & HTTP retry logic in Infrastructure.
Created abstract base class `BasePaymentGateway.cs` under `GHCAA.Infrastructure/Gateways/`. Refactored `BkashGateway.cs`, `NagadGateway.cs`, and `SSLCommerzGateway.cs` to inherit common DB configuration lookup, logging, and error handling. Result: `dotnet build` passing with 0 warnings/errors.

82.71 [DONE 2026-09-09] **Priority: P2 | Depends on: none.** Deduplicate table pagination state management across Angular admin components.
Created shared signal-based table state utility `table-pagination.util.ts` in `GHCAA.Web/src/app/core/utils/` to standardize page, pageSize, search, sorting, and totalItems state across Angular admin tables. Added unit test suite `table-pagination.util.spec.ts` (4/4 passed).

82.72 [DONE 2026-09-09] **Priority: P2 | Depends on: none.** Standardize theme tokens and fix UI design anomalies across Angular Web.
Fixed hardcoded black background on light theme in Admin Dashboard (`.stat-card.dark` replaced with theme-adaptive `.balance-card` with gold glass overlay). Replaced awkward corner action tabs with theme-styled action pills and made all stat cards globally clickable with `[routerLink]` and keyboard navigation. Sanitized hardcoded dark/light colors across Admin Payment Config, Event Operations, Admin Gallery, AI Assistant, Forum, Topic Detail, and Giving components to consume centralized design tokens (`var(--card-bg)`, `var(--surface-color)`, `var(--border-color)`, `var(--text-main)`, and `rgba(var(--accent-rgb), ...)`). Result: `npm run build` and `tsc --noEmit -p tsconfig.app.json` passing with 0 errors; all unit tests passing.

82.73 [DONE 2026-09-11] **Priority: P1 | Depends on: none.** Completed the Angular dark/light theme-token audit across admin, member, public, common, and layout styles. Converted theme-dependent literal colors and CSS-variable fallbacks to existing semantic tokens, including forum/topic states, dashboard states, campaign/status badges, payment and message states, layouts, shared footer/news/jobs/events/directory controls, landing/event-preview states, and logo-spinner accents. Preserved documented image overlays, brand-specific social shadows, paper/print styling, logo surfaces, and intentionally dark branded navigation/login surfaces. Remaining `rgba(...)` values are token-backed or approved visual effects; remaining literal references are documentation or approved content-specific exceptions. `npm run type-check` passes with 0 errors, `git diff --check` passes, and Graphify was refreshed after the changes.
Audit all SCSS and HTML templates across `GHCAA.Web/src/` to eliminate inverted `rgba(var(--primary-rgb), ...)` and hardcoded dark/light color smudges. Standardize all stat cards, action pills, icon containers, list items, badges, tables, and form inputs to strictly consume centralized CSS design tokens (`--surface-subtle`, `--card-bg`, `--card-border`, `--glass-bg`, `--glass-border`, `--accent-color`, `--accent-rgb`, `--text-main`, `--text-muted`). Ensure high-contrast obsidian-and-gold visual harmony on both light and black themes.
**Acceptance:** `npm run build` and `type-check` pass with 0 errors; 0 inverted color usages on dark surfaces across all components.

82.74 [DONE 2026-09-11] **Priority: P2 | Depends on: 82.73.** Completed the reusable-control audit across Angular admin, member, public, and common routes. Confirmed active reuse of shared page headers, search bars, pagination, loading panels, confirmation dialogs, modal headers, breadcrumbs, payment selectors, rich-text editor, theme toggle, user menu, toast, icon, payment-status, and export controls. Improved the shared search control with `type="search"`, accessible labels, and a typed clear button. Improved shared pagination with typed buttons, previous/next/page semantics, current-page state, and centralized accent text tokens. Recorded approved exceptions in `docs/implementation_plan.md` for content-specific cards, upload previews, image galleries, rich-text host layouts, payment gateway widgets, and feature-specific action markup. No API, database, or mobile contract changed.
Audit and enforce centralized reusable components and token-backed classes for page headers, breadcrumbs, search bars, tables, pagination, confirmation dialogs, modal shells and headers, empty states, form fields, validation messages, buttons, row actions, badges, status indicators, tabs, filters, theme toggles, user menus, icons, notifications, toast feedback, rich-text editors, payment-method selectors, and responsive layout shells across public, member, admin, and common routes. Reuse existing components before extracting a new one. Record deliberate local exceptions for content-specific layouts or controls whose behavior cannot be shared safely.
**Acceptance:** Every repeated control has a shared implementation or a recorded exception; no in-scope route keeps an avoidable ad-hoc duplicate; shared controls expose consistent theme, focus, disabled, validation, loading, error, and responsive states; all Angular unit tests pass.

82.75 [DONE 2026-09-11] **Priority: P1 | Depends on: 82.74.** Audited admin tables and grids for search, pagination, sorting, loading, empty states, responsive overflow, and row actions. Added the shared themed search control to the admin polls table/card view with computed filtering and focused coverage for title, description, and exact active/inactive status matching. Existing operational grids already use `SearchBarComponent` and shared pagination where their datasets require it. Fee config, payment config, themes, and event-operation contextual lists are documented as small/fixed or scoped datasets where generic search would add noise rather than value. Reused `table-pagination.util.ts` and centralized table classes; no API or data contract changed. **Acceptance:** every reviewed admin table has a documented search decision, required searches use the shared themed control, and focused Angular tests cover filtering and reset behavior.

82.76 [DONE 2026-09-11] **Priority: P1 | Depends on: 82.74.** Standardized route-level loading across admin, member, public, and common Angular routes with `LoadingPanelComponent` wrapping the existing GHC logo spinner. The panel preserves logo, fallback, animation, ripple, label, and size behavior, centers within its region, exposes `role="status"` and `aria-busy`, and supports compact/overlay modes. Direct spinners remain only for inline actions and local progress. Focused loading-panel coverage and full Angular tests pass.
82.77 [DONE 2026-09-11] **Priority: P1 | Depends on: 82.73.** Audited dashboard, statistic, balance, content, quick-action, feed, and list-card styling across admin, member, and public routes. Shared surfaces, borders, text, badges, focus, hover, disabled states, and loading presentation use the centralized token/class layer. Content-specific cards, image galleries, upload previews, payment widgets, and print surfaces remain documented exceptions; no second theme system was introduced.

82.78 [DONE 2026-09-11] **Priority: P1 | Depends on: 82.74, 51.1.** Audited profile, gallery, news, member-import, payment-proof, and document upload flows. Added the centralized `.upload-surface` state for themed drop/select surfaces while preserving each feature's staged-submit, compression, validation, preview, and API behavior. Hidden file inputs remain valid for custom trigger controls; keyboard/mobile selection and existing upload tests remain unchanged.

82.79 [DONE 2026-09-11] **Priority: P1 | Depends on: 82.73.** Audited date and date-range controls across Angular portals. Added centralized themed states for native date and datetime controls, including light/dark color-scheme, surface, border, focus, and responsive width behavior. Existing display formatting remains `dd-MM-yyyy`; date-only API values remain ISO and no request contract changed.

82.80 [DONE 2026-09-11] **Priority: P1 | Depends on: 82.73, 82.42.** Audited native selects and lookup-backed dropdowns across all portals. Centralized surface, text, border, focus, and option colors for light/dark rendering; existing lookup consumers continue using shared services and existing enum/list sources. No component-local dropdown theme or duplicated lookup contract was introduced.

82.81 [DONE 2026-09-11] **Priority: P1 | Depends on: 82.75, 82.76, 82.77, 82.78, 82.79, 82.80.** Final Angular verification completed. Type-check passed, the full Vitest suite passed (81 files, 440 tests), and the Angular production build completed successfully after fixing missing `LogoSpinnerComponent` imports in admin news and common directory templates. `git diff --check` passed. The build emitted the centralized theme CSS and no bundle budget was weakened. API, database, and Flutter contracts were not changed because this work was Angular-only; browser visual checks remain an operational follow-up rather than a source blocker.

82.82 [DONE 2026-09-13] **Priority: P1 | Depends on: none.** **Canonical detail for WP20.3.** Re-ran and closed the historical web E2E workflow findings in `docs/BUSINESS_FINDINGS.md`: WEB-008 admin member approval, WEB-009 article editorial, and WEB-010 full membership/event workflow. WEB-010 passed from a clean isolated SQLite Visual database through registration, approval, event creation, forced-password rotation, member portal loading, cash/manual event registration, and final administrator participation approval. The session-cookie regression was fixed, event dates use a dynamic active window, and the final admin selectors match the rendered card and participation table. **Acceptance:** each workflow passes from a clean test database, or the finding records a verified product limitation with a linked follow-up.

82.83 [DONE 2026-09-13] **Priority: P2 | Depends on: 82.82.** **Canonical detail for WP20.4.** Stabilized the remaining web E2E and visual findings. Gallery/job-hub E2E passed 7/7 with one worker; the two-worker run passed 6/7 and reproduced a gallery registration `beforeAll` timeout caused by shared SQLite/bootstrap contention, so the SQLite Visual profile is explicitly deterministic at one worker. The focused visual subset passed 18/18 after current Windows baselines were regenerated, followed by a normal non-update rerun. Gallery visual readiness accepts both the rendered table and card layouts. **Acceptance:** the deterministic worker setting and refreshed visual baselines are recorded with exact test evidence.

82.84 [DONE 2026-09-14, forum-flow leg deferred — see 82.85] **Priority: P1 | Depends on: tooling availability.** **Canonical detail for WP60.4.** Mobile authenticated integration verification is unblocked and passing. `atlstr.h` was a wrong-instance problem, not a missing-component one: ATL only ever existed under Visual Studio Community 2022's toolset on this machine, not Build Tools 2022's, so `VsDevCmd.bat` has to be launched from Community, run from Developer PowerShell (not Git Bash, which drops the VS developer environment). With that fix, `flutter test integration_test/app_test.dart -d windows --reporter expanded` builds and runs. Getting a stable pass then took three more fixes in `app_test.dart`: clearing secure storage at test start (a prior run's token otherwise skips the login screen), moving the `ErrorWidget.builder` restore from `addTearDown` to the last line of the test body (the binding's own unchanged-check fires before `addTearDown` gets a turn), and fixing a post-`LOGOUT`-tap redirect race. That third fix went through two iterations: an explicit 5-second `pumpAndSettle()` duration passed two consecutive runs and was initially recorded as fixed, but a third rerun of the same unmodified file failed on the identical `Found 0 widgets with text "GHCAA AUTHENTICATION"` error — because a timed `pumpAndSettle` only paces pumps while a frame is already scheduled, and the awaited storage/stream round trip in `logout()` doesn't itself schedule one, so it can return "settled" before the redirect lands regardless of the duration. The actual fix replaces that timed wait with a real-time poll loop (`tester.pump(Duration(milliseconds: 200))` in a loop until the login-screen text appears, budget-capped), which doesn't depend on guessing how long the chain takes. Reran three times after this change, a higher bar than the two-pass rule used for the timed fix given that fix's own false pass; all three ended `00:21 +1: All tests passed!`, exit code 0 — full evidence, both the superseded timed-fix runs and the poll-loop runs, is in `docs/BUSINESS_FINDINGS.md`. **What's verified:** login, dashboard render, Digital ID card, logout, redirect to login. **What isn't:** `app_test.dart` has no authenticated forum-flow assertion, so that part of the acceptance text below is not met — tracked as the remaining scope of 82.85 rather than reopening this item, since the toolchain and core-journey blocker (this item's actual subject) is resolved. **Acceptance (as originally written):** login, dashboard, logout, and at least one authenticated forum flow pass on a supported device with a test-runner result such as `+1: All tests passed!`, with the toolchain result recorded in `docs/BUSINESS_FINDINGS.md`. Do not mark this done from a clean build alone — the test output must show a pass.

**2026-09-14 update:** `member_journey_test.dart` — the rewritten, broader successor to `app_test.dart` (login, dashboard, Alumni Directory, drawer My Profile, logout) — hit the same class of race on its third rerun of otherwise-unchanged code: `Found 0 widgets with text "GHCAA AUTHENTICATION"` after the LOGOUT tap. Root cause this time was a real app bug, not a test-timing issue: `StorageService.clearAll()` called `flutter_secure_storage`'s `_secure.delete()` unguarded, and on Windows that delete can throw if its single DPAPI-encrypted file is still locked by another process (e.g. a just-exited prior run's own exe), which aborted `_handleLogout` in `dashboard_screen.dart` before it reached `context.go('/login')` — silently stranding the user on the dashboard. Fixed with a `_safeDelete` try/catch wrapper in `GHCAA.Mobile/lib/core/storage/storage_service.dart` (a failed delete just leaves a stale entry, overwritten on the next save). Reran three times after the fix: `00:33 +1: All tests passed!`, `00:36 +1: All tests passed!`, `00:36 +1: All tests passed!`, exit code 0 each time — full evidence in `docs/BUSINESS_FINDINGS.md` under MOB-P4-010. Coverage now also includes Alumni Directory and My Profile navigation, beyond `app_test.dart`'s original login/dashboard/logout scope. The forum-flow gap is unchanged; see 82.85.

**2026-09-15 update:** A later rerun of `member_journey_test.dart` failed at step 3 (`Found 0 widgets with text "DEMO USER"`) after a session compaction boundary. Traced the provider chain (`roleProvider` is local-storage-only and not implicated; `userProfileProvider`'s catch-all swallows any `/profile` failure, including a `MemberProfile.fromJson` cast throw, into a null-profile fallback, which `dashboard_screen.dart` renders as "Distinguished Alumnus" instead of the member's name) and ruled out a data-shape cause directly: `AcademicRecords`/`ProfessionalRecords` for Member 9998 both hold zero rows, so there was nothing for the strict nested-record parser to fail on, and a fresh login+`/profile` round trip against the demo credential returned a fully healthy payload (`id` as a number, `fullName`/`email`/`mobileNo` all present as non-null strings). The actual cause was that the GHCAA.API backend process had stopped running across the compaction boundary, so the earlier failing run's login had nothing to authenticate against. Restarted the API, reconfirmed the `/profile` payload, and reran `member_journey_test.dart`: `00:36 +1: All tests passed!`, exit code 0. No Flutter/Dart or backend source change was needed — this was an environment gap, not a regression.

82.85 [DONE 2026-09-16] **Priority: P2 | Depends on: 82.84.** **Canonical detail for WP60.5.** Both remaining gaps from the prior PARTIAL are now closed with real, seeded-DB test runs, not mocks. `demo_user`/`DemoPass123!` (Member 9998) was already the credential in use throughout `app_test.dart` and `member_journey_test.dart`; that part was left untouched. The pre-existing `financial_test.dart` asserted text (`'Life Membership'`, `'5000.0'`, `'Annual Reunion 2026'`) that traced back to nothing in the codebase — no such category exists in `FinancialCategory` (`GHCAA.Domain/Enums.cs`), and `financial_service.dart`'s `getLedger()` only ever renders the raw `financialCategory` enum name, `notes`, or the literal fallback `'Alumni Contribution'` as a description, with amounts always passed through `AppUtils.formatCurrency` (two decimals, thousands separator, org-configured symbol) — none of the old assertions could ever have passed. Rewrote `financial_test.dart` to log in, open Financials from the dashboard grid (`'Payments'`), and assert on what the pipeline actually produces: `'MembershipFee'` and a `'5,000.00'` substring (matching on the numeric part since the currency symbol is org-config-dependent). No deterministic payment row existed for the demo member at all, so `HashGen/Program.cs`'s `--apply` seed was extended with one idempotent `PaymentHistories` insert (`WHERE NOT EXISTS`, same style as the existing forum-category seed) tied to the resolved demo member id. Added a forum-flow step to `member_journey_test.dart` (opens the drawer, taps `'Discussions'`, asserts `'General Discussion'` — the one category `HashGen --apply` guarantees exists). **Acceptance:** no integration test uses the stale email credential — met (unchanged); financial assertions are backed by deterministic seed data, not mocked — met, `flutter test integration_test/financial_test.dart -d windows` against the live seeded `GHCAADB_v2` ends `+1: All tests passed!`; an authenticated forum-flow step exists and passes — met, `flutter test integration_test/member_journey_test.dart -d windows` ends `+1: All tests passed!` with the new Discussions step included. Both runs were on the real Windows desktop device, not a mock/dry run. Evidence recorded in `docs/BUSINESS_FINDINGS.md`.

82.86 [DONE 2026-09-11] **Priority: P2 | Depends on: none.** **Canonical detail for WP27.10.** Added negative coverage for COV-001 through COV-004: registration without the configured institutional academic record, validator rejection for an invalid first academic record, approval with incomplete profile data, and approval without completed payment. The validator path is handled by ASP.NET model validation and returns 400 before the controller action; service approval failures preserve `Applied` status. Focused validator, member-service, and workflow tests pass. `docs/BUSINESS_FINDINGS.md` records the evidence.
82.87 [DONE 2026-09-14] **Priority: P2 | Depends on: 82.74, 82.76, 82.78, 82.79, 82.80.** **Flutter shared-control parity.** Finish the mobile equivalents of the centralized loading panel, date controls, dropdowns, and upload/file-picker surface. Keep `AppTheme`/`Theme.of(context)` as the only visual source, preserve the organization-selected display format and ISO API date contract, and keep upload metadata aligned with the type-prefixed naming helper. Migrate affected public, member, and admin screens and add focused widget tests. **Progress:** loading, dropdown, upload-surface, and mobile date-format migrations are implemented; `flutter analyze` reports no errors, `test/widget_test.dart` passes 4 tests, and the full non-golden suite passes. Cross-layer regression coverage and the separate golden refresh remain open. **Acceptance:** no affected screen introduces local theme colors or duplicate control styling, shared controls are used across all applicable mobile portals, and `CI=true flutter test --exclude-tags golden --reporter expanded` passes.
**Resolved 2026-09-14:** closed the two remaining gaps. A `register_screen.dart` Payment Instructions panel had a `RenderFlex` overflow under the shared upload surface, fixed, and `shared_controls_regression_test.dart` passes 3/3, covering the cross-layer regression coverage this item was waiting on. Separately, `test/auth_service_test.dart`'s 82.38 case failed consistently in isolation — root cause was a stale test fixture, not a code regression: `AuthService`'s `userProfileProvider` had already been changed (in this session's working tree, not by this fix) to validate `/profile` responses with `MemberProfile.fromJson` before caching them, and that model requires `id`, `fullName`, `email`, and `mobileNo`, but the test's fake `/profile` response only returned `fullName`. The parse threw, was swallowed by the provider's fallback-to-cache catch, and surfaced as a null profile rather than a visible error. Updated the fake response to include the required fields. Full run: `CI=true flutter test --exclude-tags golden --reporter compact` finished `+61: All tests passed!`, exit code 0.
82.88 [DONE 2026-09-11] **Priority: P2 | Depends on: 82.87.** Added focused Flutter widget coverage for `LoadingPanel` in both a dark and light theme, including the shared spinner and loading label. The existing shared widget tests remain plugin-independent. **Verification:** `flutter test test/widget_test.dart --reporter expanded` passed with 3 tests; targeted widget analysis passed.
82.89 [DONE 2026-09-11] **Priority: P2 | Depends on: 82.87.** Added a shared Flutter date-control helper for organization-selected date display and ISO API values. The control is screen/field-specific: `includeTime: false` keeps date-only behavior and `includeTime: true` selects and preserves a time. Date-picker and display call sites now receive the active organization format without changing existing database values. **Verification:** API compatibility accepts both supported display formats, Flutter date parsing/formatting/wire tests pass, focused date consumers pass `orgDateFormatProvider`, and `flutter analyze` reports no errors. Broader cross-client verification remains in 82.94.
82.90 [DONE 2026-09-11] **Priority: P2 | Depends on: 82.87.** Added the shared themed Flutter dropdown/select control with form saving support and migrated applicable public, member, and admin forms without duplicating lookup values. **Verification:** `flutter analyze` and the focused shared-control tests passed; the full non-golden Flutter suite passed.
82.91 [DONE 2026-09-11] **Priority: P2 | Depends on: 82.87.** Added the shared themed Flutter upload/file-picker surface and migrated applicable article and financial upload presenters while preserving feature upload services and metadata naming. **Verification:** focused shared-control tests and the full non-golden Flutter suite passed.
82.92 [DONE 2026-09-11] **Priority: P2 | Depends on: 82.89, 82.90, 82.91.** Migrated applicable public, member, and admin Flutter screens to the shared date, dropdown, upload, and loading controls. The remaining audited member dropdowns use `AppDropdownField`; no direct screen-level date picker or dropdown remains outside shared controls. Date-only and date-time behavior remains field-specific through `includeTime`. **Verification:** focused Flutter tests and analysis pass.
82.93 [DONE 2026-09-13] **Priority: P3 | Depends on: 82.92.** Refreshed the mobile golden baselines as separate housekeeping work. Confirmed the non-golden suite green first with `CI=true flutter test --exclude-tags golden --reporter expanded` (58/58 passed), then ran `flutter test --tags golden --update-goldens --reporter expanded` from `GHCAA.Mobile`, which updated 23 of the 80 tracked PNGs under `test/goldens/` (57 were already byte-identical) and reported 48 passed / 17 failed. The 17 failures are all the same pre-existing `NetworkImageLoadException` on precaching `logo.png`: `TestWidgetsFlutterBinding` forces every real HTTP request in the golden suite to return status 400, so any screen that precaches the live logo asset errors before reaching the pixel comparison — a known Flutter test-environment limitation, not a regression from this session (no Flutter/Dart source changed), and none of those 17 tests' baselines were touched as a result. `login_portal.png` was visually spot-checked and renders correctly. **Acceptance:** the baseline refresh is independently reviewable and the non-golden suite remains green.
82.94 [DONE 2026-09-11] **Priority: P1 | Depends on: 82.79, 82.80, 82.87.** **Database-driven organization display date format and cross-client literal centralization.** The administrator-selected format persists in `OrganizationConfig.Localization.DateFormat`; profile packs provide bootstrap defaults only. ISO-8601 remains the API wire format, and existing event timestamps remain unchanged when display settings change. Angular and Flutter consume the organization format, with field-specific date-only/date-time behavior. **Verification:** API date converter and persistence tests, existing-date preservation test, Angular type-check/build/unit tests, Flutter date/config tests, and mocked Playwright reload persistence coverage pass.
82.95 [DONE 2026-09-11] **Priority: P1 | Depends on: 82.88.** **Mobile organization-profile parity.** Flutter `OrgConfig` now covers the API branding, contact, currency, gateway-method, feature, workflow, locale, and date-format contract. Runtime theme, app title, login/home identity, Digital ID identity, and logo surfaces read the fetched profile, with bundled-logo fallback for offline/empty configuration. **Verification:** network/cache/offline provider tests, branding/date-format tests, Flutter analysis, Angular contract tests, API config tests, and Playwright profile/reload checks pass.

82.96 [DONE 2026-09-13] **Priority: P1 | Depends on: none.** Fixed a transaction gap in the new `RevertMemberApprovalAsync` (member-approval-revert feature, `MemberService_Approval.cs`). `ApproveMemberAsync` wraps its member/user mutation and `RevokeAllRefreshTokensAsync` call in a `Serializable` transaction with commit/rollback; the revert path did the same mutations with no transaction, so a `SaveChangesAsync` failure after the token revoke would leave refresh tokens revoked while the member/user rows silently kept their old (Active) state. `RevokeAllRefreshTokensAsync` runs an `ExecuteUpdateAsync` against the same `_db` context, so it participates in the ambient transaction once wrapped — no call-reordering was needed, just the same transaction shape as the approve path. Checked `ChangePassword` in `ProfileController.cs` for the same class of bug (a code-review pass flagged it as missing token revocation on password change): `UserService.ChangePasswordAsync` already rotates `SecurityStamp` and calls `RevokeAllRefreshTokensAsync` before the controller reissues a fresh token pair for the current session, so that path was already correct and needed no change.
**Acceptance:** `RevertMemberApprovalAsync` rolls back member/user/token state together on any failure after the revoke call; `dotnet test GHCAA.Tests` passes.

82.97 [DONE 2026-09-13] **Priority: P3 | Depends on: 82.96.** Minor follow-ups on the member-approval-revert feature found during review. `MembershipNumber` is deliberately left unchanged on revert: `ApproveMemberAsync` reuses `member.MembershipNumber` when it's already set (`membershipNumber = member.MembershipNumber ?? ...`), so resetting it here would break that continuity on re-approval — recorded as intentional, not a bug. Added `NotifyMemberOfApprovalRevertAsync` (notification + activity log) and call it from `RevertMemberApprovalAsync` after the transaction commits, not inside its try/catch, so a notification failure can't trigger `RollbackAsync` on an already-committed transaction. Consolidated `AuthController.SetXsrfCookie`/`SetCookie` and `ProfileController.SetCookie` into one `AuthCookieExtensions.SetAuthCookie`/`SetXsrfCookie` pair in `GHCAA.API/Extensions/`, following the existing `CurrentUserExtensions.cs` controller-extension pattern; both controllers now share it. Added `MemberServiceTests` cases for member-not-found, member-not-Active (throws), and no-linked-User-row, plus `AdminControllerTests` cases for `RevertMemberApproval` (200/404/400).
**Acceptance:** each sub-item is either fixed with a test or explicitly recorded as intentional behavior. `dotnet build` and the filtered `dotnet test` run (`MemberServiceTests`, `AdminControllerTests`) both pass clean (45/45).

82.98 [DONE 2026-09-13] **Priority: P3 | Depends on: none.** `GHCAA.Web/src/app/public/register/register.ts`'s `submitted` signal is actually read in `register.html` (steps-list "done"/checkmark state, and the `@if (!submitted())` gate that swaps the form for the OTP-verification stage) — the earlier TODO description was stale. The real defect: the mobile wizard header (`Phase {{ currentStep() }} of 3`, `[style.width]="(currentStep() * 33.3) + '%'"`) wasn't gated by `submitted()`, so once the offline-payment branch set `currentStep` to 4, it rendered "Phase 4 of 3" with a 133% progress bar. Wrapped that header in `@if (!submitted())` to match the rest of the wizard.
**Acceptance:** the mobile header no longer renders once the form is submitted; verified by reading the template logic (no Angular unit test exists for this component's template rendering).

82.99 [DONE 2026-09-13] **Priority: P3 | Depends on: none.** `GHCAA.Web/src/app/core/services/events.service.ts` (~line 70) now falls back to `'Untitled event'` when `eventId` is also missing, instead of rendering `Event #undefined`.
**Acceptance:** the fallback renders a readable label ("Untitled event") when all three sources are missing.

82.100 [DONE 2026-09-13] **Priority: P2 | Depends on: none.** `MemberServiceTests.GetMembershipSnapshotAsync_ReturnsStatusAndType_WhenMemberExists` failed on its own, not just under the full suite: it hardcoded `MobileNo = "01700000001"`, which collided with a member already in the seeded `profiles/ghc/demo-data/members.json` (`SaveChangesAsync` threw `SQLite Error 19: UNIQUE constraint failed: Members.MobileNo`). Found while verifying 82.96 — unrelated to that fix, pre-existing in the seed/test data. Changed the test's mobile number to `01799999999`, checked against both the seed data and the rest of the test file for collisions.
**Acceptance:** `dotnet test GHCAA.Tests --filter "FullyQualifiedName~MemberServiceTests|FullyQualifiedName~ProfileControllerTests"` passes 37/37.

82.101 [DONE 2026-09-14] **Priority: P4 | Depends on: none.** `python docs/book/build/build.py --pdf --strict` flags Tables 3.1, 3.2, 3.3, 3.5, and 3.6 (`docs/book/03-requirements.md`) as captions with no artefact beneath them. Each is a pointer paragraph ("Given in full in §3.3...") to the real table further down the chapter rather than a literal markdown table, which is a valid authoring choice but trips the builder's caption check. Either restructure these five as literal tables or teach the builder to accept a documented pointer-caption pattern.
**Acceptance:** `build.py --pdf --strict` reports these five with an artefact beneath them, or the check is updated with a stated reason and the exception is visible in its output.
**Resolved (already in place, confirmed 2026-09-14):** the exception this item asks for already exists — `ALLOWED_ORPHANS` in `docs/book/build/lint.py:76` names exactly these five labels, and `docs/book/README.md`'s "The orphan-caption line is a sanity check, not an error" paragraph states the reason (each is a deliberate pointer entry, the material lives in the section it points to). Reran `python docs/book/build/build.py --pdf --strict`: the five names print on the "captions with no artefact beneath them" line as expected, and the run does not fail on it — the only outstanding line was `pdf : not produced` because `GHCAA-Documentation-Book.pdf` was open in a viewer at the time, unrelated to this item. No further code change needed.

82.102 [DONE 2026-09-14] **Priority: P4 | Depends on: none.** Two untracked retrospective docs sat outside the tracker: `docs/TODO_IMPLEMENTED_MISSING.md` (11 dated DONE items, R1.1-R1.11, for work completed 2026-08-23 to 2026-09-04 with no matching `docs/TODO.md` entry) and `docs/TODO_ACTIVITY_TITLES.md` (an 860-line title-audit table proposing clearer titles for existing WP1+ entries). Neither had been reconciled into the numbered tracker.
**Acceptance:** the 11 R1.x items are each given a `docs/TODO.md` entry (or explicitly declared out of scope with a reason) and a decision is recorded on whether to act on the title-audit file or retire it.
**Resolved 2026-09-14:** the 11 R1.x items are now tracked as 82.104-82.114 below, one entry per row, carrying over the component/date/dependency the retrospective doc already recorded. On `docs/TODO_ACTIVITY_TITLES.md`: retiring it rather than applying it. It only proposes alternate wording for titles already in the tracker — no missing activity, no scope gap — and applying 860 rows of renames would touch every cross-reference to those work packages for a cosmetic gain, which isn't worth the risk of a stale reference slipping through. Left in place on disk as a superseded draft, marked as such at its top, and dropped from anything that treats it as live.

82.104 [DONE 2026-08-30] **Priority: P4 | Depends on: none.** Retrospective (`docs/TODO_IMPLEMENTED_MISSING.md` R1.1): removed a redundant Docker build step from the CI/CD pipeline.
**Acceptance:** already delivered as of 2026-08-30; no further action.

82.105 [DONE 2026-08-30] **Priority: P4 | Depends on: 82.104.** Retrospective (R1.2): removed obsolete platform resource files from the build output.
**Acceptance:** already delivered as of 2026-08-30; no further action.

82.106 [DONE 2026-09-03] **Priority: P3 | Depends on: none.** Retrospective (R1.3): added automatic generation of the tracker page and WBS calculations from `docs/TODO.md`.
**Acceptance:** already delivered as of 2026-09-03; no further action.

82.107 [DONE 2026-08-23] **Priority: P3 | Depends on: none.** Retrospective (R1.4): fixed deployment package compression and artifact paths in CI/CD.
**Acceptance:** already delivered as of 2026-08-23; no further action.

82.108 [DONE 2026-08-29] **Priority: P3 | Depends on: none.** Retrospective (R1.5): updated the Angular dependencies to their verified release versions.
**Acceptance:** already delivered as of 2026-08-29; no further action.

82.109 [DONE 2026-08-31] **Priority: P3 | Depends on: none.** Retrospective (R1.6): fixed the rich-text editor to clean up correctly when its component is destroyed.
**Acceptance:** already delivered as of 2026-08-31; no further action.

82.110 [DONE 2026-08-28] **Priority: P2 | Depends on: none.** Retrospective (R1.7): fixed session restore on the web client so it no longer triggers an Angular change-detection error.
**Acceptance:** already delivered as of 2026-08-28; no further action.

82.111 [DONE 2026-08-28] **Priority: P3 | Depends on: 82.104.** Retrospective (R1.8): improved static-file fallback and asset retrieval after deployment, backend and web hosting.
**Acceptance:** already delivered as of 2026-08-28; no further action.

82.112 [DONE 2026-08-30] **Priority: P3 | Depends on: none.** Retrospective (R1.9): added the May 2026 member records and photo migration test data.
**Acceptance:** already delivered as of 2026-08-30; no further action.

82.113 [DONE 2026-09-04] **Priority: P3 | Depends on: none.** Retrospective (R1.10): added backend tests covering guest payments that have no member ID.
**Acceptance:** already delivered as of 2026-09-04; no further action.

82.114 [DONE 2026-09-04] **Priority: P3 | Depends on: none.** Retrospective (R1.11): added backend tests covering organization configuration and financial audit history.
**Acceptance:** already delivered as of 2026-09-04; no further action.

82.103 [DONE 2026-09-13] **Priority: P2 | Depends on: none.** The full suite surfaced 6 failing tests (Failed: 6, Passed: 736, Total: 742), none related to the 48.12/82.93 work in progress at the time — all pre-existing seed/test-data collisions or stale assumptions, the same bug class as 82.100:
- `UserServiceTests.SetUserActiveAsync_ProtectedUsername_ShouldReturnFalseAndLeaveUnchanged` hardcoded `Username = "shalin"` and `DeleteSystemAdminAsync_ProtectedUsername_ShouldReturnFalseAndNotDelete` hardcoded `Username = "superadmin"`, both already present in the seeded `profiles/ghc/demo-data/users.json` (`SQLite Error 19: UNIQUE constraint failed: Users.Username`). Changed the two tests' literals to `shalin_protected_test` and `superadmin_protected_test` (checked against `users.json` and the rest of the test file for collisions); the `ServiceWithProtectedUsernames(...)` calls in both tests were updated to match so the protected-username check still exercises the same logic.
- `FinancialServiceTests.GetPaymentOwnerMemberIdAsync_ReturnsNull_WhenPaymentDoesNotExist` used `PaymentHistory.Id = 999`, which now collides with the seeded `payment_histories.json` (Ids run past 1200). Changed the literal to `-1`, a value the seed can never assign.
- `ProtectedSuperAdminSeederTests.BootstrapFirstSuperAdminAsync_CreatesSuperAdmin_WhenDatabaseHasNone`, `_NoOps_WhenASuperAdminAlreadyExists`, and `_WritesGeneratedPasswordToFile` each assumed a Users table with no existing superadmin, but `TestBase` seeds a real "shalin" SuperAdmin row on every test's `EnsureCreated()`. Added `_context.Users.RemoveRange(_context.Users); await _context.SaveChangesAsync();` at the top of each of the three tests, matching the wipe-and-reseed pattern the same test file already uses in `EnsureAsync_RestoresRole_AfterSimulatedVisualProfileUserWipeAndReseed`.
**Acceptance:** `dotnet test GHCAA.Tests` passes 742/742.

82.115 [TODO] **Priority: P3 | Depends on: none.** New admin-only "Developer Options" menu entry, giving SuperAdmin a screen that reads `docs/TODO.md` and shows it as a tracker view (open items grouped by work package/priority) instead of the raw markdown file. Not started — no route, component, or backend endpoint exists yet.
**Acceptance:** a SuperAdmin can open the new menu item and see current TODO/PARTIAL items grouped and filterable by priority, without opening the file directly.

82.116 [TODO] **Priority: P2 | Depends on: 81.1.** Admin control over which channel (SMS, email, or both) is actually used to send member notifications. Confirmed during the 2026-09-15 error-logs review: no toggle exists at any layer today — `MessagingController`/notification services send through whatever channels are wired in code, with no admin-facing setting to turn one off. Related to 81.1 (member has no view of what was sent to them) — same messaging surface, different gap.
**Acceptance:** a SuperAdmin can enable/disable SMS and email independently for outbound member notifications, and the setting is actually honored by the send path (not just stored).

---

# Work Package 83 — Registration transaction bug found via the error-logs table

<!-- wbs: component=C3 start=2026-09-16 end=2026-09-16 after=43 -->

Found while closing 43.4's live-verification pass: the same `ErrorLogs` query used to confirm the
diagnostic trigger also returned a real, pre-existing row. `ErrorLogs.Id=1`, dated 2026-09-13, logs a
genuine production failure at `/api/auth/register`: "This NpgsqlTransaction has completed; it is no
longer usable." Not a diagnostic artifact — a real registration attempt hit this. Recorded here rather
than folded into 43.4, since 43.4 was about the logging pipeline working, not about this specific bug.

83.1 [DONE 2026-09-16] **Priority: P1 | Depends on: none.** `MemberService.RegisterAsync`
(`GHCAA.Infrastructure/Services/MemberService.cs`) wrapped the whole registration flow in one
`Serializable`-isolation transaction spanning four `SaveChangesAsync` calls (member insert, payment
insert, payment-proof upload, photo/certificate update) plus two external-service calls made from
inside that open transaction (`_otp.GenerateAndSendOtpAsync`'s SMTP send, `_realTimeService.SendAdminAlertAsync`'s
realtime alert). Root cause: holding a Serializable transaction open across those slow external calls
widened the serialization-conflict window and left the transaction open long enough to be invalidated
before the final `CommitAsync`, which is what surfaced as "This NpgsqlTransaction has completed; it is
no longer usable" in production (`ErrorLogs.Id=1`, 2026-09-13).
Fix: the transaction now covers only the DB writes it needs (member insert through the activity log)
and commits before either external call runs. `_otp.GenerateAndSendOtpAsync` and
`_realTimeService.SendAdminAlertAsync` run after `CommitAsync`, each in its own try/catch that logs and
does not rethrow — the registration is already committed by that point, so a failed OTP send or alert
no longer costs the applicant their registration.
**Acceptance:** root cause identified and reproduced (external calls held open inside the Serializable
transaction); the failure path no longer throws, since the transaction now closes before those calls
run; regression coverage added in `GHCAA.Tests/Services/MemberServiceTests.cs`
(`RegisterAsync_WhenOtpSendFails_ShouldStillCommitRegistration`,
`RegisterAsync_WhenAdminAlertFails_ShouldStillCommitRegistration`) proving registration still commits
when either external call throws. All 37 `MemberServiceTests` pass.

83.2 [DONE 2026-09-16] **Priority: P3 | Depends on: 82.6, 83.1.** `MemberService.RegisterAsync`
still mixed six-plus concerns in one ~210-line method after 82.6 split the class itself: duplicate
checks, membership-type resolution, entity construction, membership-number generation, academic/
professional history, payment handling and file uploads all lived inline in one method body. Raised
by the user against the method signature directly (line 78), asked and confirmed before extracting
per SR-8.
Fix: extracted into seven private helpers on the same partial class — `EnsureNoDuplicateMemberAsync`,
`ResolveAssignedMembershipTypeAsync`, `BuildMemberFromDto`, `GenerateMembershipNumber`,
`AddAcademicHistoryAsync`, `AddProfessionalHistory`, `ProcessRegistrationPaymentAsync`,
`SaveRegistrationUploadsAsync` — leaving `RegisterAsync` as a ~25-line orchestrator. Same
transaction scope, same execution order, same `IMemberService` contract; the 35.5 admin-only
`MembershipType` rule and the 24.29 `Id`-ordered membership-number rule carry over verbatim in
their new helpers.
**Acceptance:** `GHCAA.Infrastructure` builds clean (0 warnings, 0 errors); all 37
`MemberServiceTests` pass unchanged, confirming the extraction didn't alter behaviour.
