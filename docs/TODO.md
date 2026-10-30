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
RE = P × C in §4.7 needs an impact cost per risk on the same basis.

### P0 — CRITICAL (blocked on the user; cannot be closed from a coding session)
- **34.D17** — [IN PROGRESS] Preprod missing columns from the old `EnsureCreated` build. Code done.
  Preprod starts again as of 2026-10-31 (34.D18 and 96.15 closed that day). Still to check: the
  member-summary queries that failed with `42703` now return data. Election release F2 waits on this
  and 34.D19.

### Closed or held by user decision (moved out of the P0 list on 2026-10-09 and 2026-10-31)
- **82.31 / 62.31** — 82.31 on hold and 62.31 dropped (WONTDO) since 2026-10-31, by owner decision. 631 real alumni records, including all 631
  password hashes, are literal `InsertData` values in eight committed migrations, so a clean clone of
  this repo builds a database full of real personal data. Editing `Seed/members.json` does not reach
  it, and no environment variable turns it off. Needs a decision on the history purge and a forced
  password reset before any code moves. `docs/SEED_CLASSIFICATION.md` has the per-file breakdown.
  Both items sit in `docs/TODO_ARCHIVE.md`.
- **48.2** [WONTDO 2026-09-18, user decision] (merged 2026-09-15 with 47.10's credentials half and
  48.13) — Live production secrets committed to git (JWT signing key, DB passwords, Gmail app
  password, Render deploy-hook URL) in `docs/deploy_connection.txt`/`docs/deploy_conn_Info.txt`,
  `.env.remote`, `build_output/appsettings*.json`, `docs/RENDER_DEPLOYMENT.md`. Separately, the live
  SuperAdmin password sat in git history (`docs/BUSINESS_REVIEW_PLAN.md`) since before it was even
  set live — doc text is redacted, but the password itself was never independently rotated. Would
  have needed the user to rotate every credential via the relevant dashboards, then `git rm --cached`
  + `.gitignore` (done) + a history purge (`git filter-repo`). User decided against the rotation/purge;
  closed without remediation. No coding-session action can reopen this without a fresh user decision.

### P1 — HIGH (security surface / explicitly time-sensitive / blocking other work)
- **37.1i–37.1l, 37.1n** — Election ballot secrecy (review 2026-09-27). All done as of 2026-10-09. Spec 023.
- **37.1m** — [PARTIAL] The vote receipt and the tracking-code list are built. Automated web, mobile
  and API checks pass. The server receipt PDF has also been inspected in headless Chromium at
  desktop size and a 390x844 viewport using a Bengali test election. Still needs a live look on a
  phone. The receipt itself comes from the server (37.14o).
- **37.14o** — [PARTIAL 2026-10-29] Depends on 37.1m. The server-made receipt uses QuestPDF and the
  bundled Noto Sans Bengali font, shows cast time in Bangladesh time (`dd-MM-yyyy HH:mm`), and the
  cast reply includes its PDF without ballot choices. Web
  and mobile keep the PDF on the device and offer it for download/share. If local storage fails,
  both clients tell the voter to download it before leaving. The API passes request cancellation
  through receipt generation and still returns the tracking code when PDF generation fails.
  Focused API, web and mobile tests pass. Headless Chromium confirms the Bengali title, code, cast
  time and no-choice text fit on the page at desktop and 390x844 sizes. Remaining: inspect it on a
  physical phone.
- **34.D19** — [IN PROGRESS] Member cards show "Batch: N/A" because the rename migration for the org
  academic record never ran. Code done; needs confirming on preprod with 34.D17 and 34.D18.
- **7.22** — [OWNER STEP] The preprod origin and ClientUrl on Render point at a dead domain. Set both
  to `https://ghcaa-ryl6.onrender.com`. No code change.
- **37.13e** — [DONE 2026-10-23] Ballot key procedure and protocol, with a test vector sealed outside .NET.
- **37.12w** — [DONE 2026-10-21] Open-polling readiness gate on the API, web and mobile. Polling
  opens only when the roll, candidate list, ballot key, appointments, polling window and the four
  Chief Commissioner attestations are met.
- **37.1s, 37.1v, 37.1t, 37.1u** — [DONE] Ballots are sealed under the returning officer's key
  (37.1s, 2026-09-27), a key replacement needs a second person (37.1v, 2026-10-01), an election
  must name an empty future EC period, and winners take office when that period is made active
  (37.1t, 37.1u, 2026-10-09).
- **37.1w** (P0) — Done 2026-09-29. Step-up is open to any signed-in user, so members can vote
  on web and mobile.
- **37.1y** (P0) - Done 2026-10-05. A failed code email now says so on web and mobile. Preprod
  email itself is still down; 7.26 tracks the fix.
- **7.26** - Code done 2026-10-14: a Brevo sender, since Render blocks SMTP. Since 2026-10-15 an
  admin picks the sender in Settings; `EmailSettings:Provider` is the fallback. Owner step: Brevo
  account, verified sender, API key on Render, then pick Brevo.
- **7.28** - Code done 2026-10-19: uploads can live in an S3-compatible bucket, and the local
  preprod uploads are copied into `ghcaa-storage`. Owner step: storage variables on Render, and
  copy any upload made on the live site since its last deploy before redeploying.
- **37.12a–37.12b** — [DONE 2026-09-29] Election settings section in OrgConfig, and a configurable
  `ElectionPersona` table with its own admin screen.
- **37.12c** — [DONE 2026-09-30] ElectionOfficial role, and role lists on web and mobile.
- **37.12d** — [DONE 2026-09-30] Appointments with accept and a signed declaration, on the backend,
  web and mobile, with the migration that copies the old officer rows.
- **37.12e** — [DONE 2026-10-01] Per-election permission check on every election write, and the
  admin handover.
- **37.12f** — [DONE 2026-10-01] Two-person rule for publish, polling, key replacement, close,
  declare and archive, with the key fingerprint on the vote screen.
- **37.12g** — [DONE 2026-10-10] Declare sets the end date on every appointment, login and refresh
  refuse a non-member official after it, and archive revokes every appointment.
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
- **84.1–84.3** — Done for seven domains. State-transition tables, a validation matrix, and
  endpoint contract tables in `docs/specs/002-workflow-contracts-and-validation/`. Moved to
  docs/TODO_ARCHIVE.md on 2026-10-14.
- **84.42** — [PARTIAL] Mobile step-up is built (37.1w); only a real-device check of the admin step-up routes is left. Found by 84.5.
  (84.41, the same-area `Forbid(string)` 500, is done.)

### P2 — MEDIUM (real, no urgency signal)
- **37.1p** — Election rules still open: recount, candidate consent, officer conflict, spoiled
  and unopposed seats. Spec 023 Phase 3. The reopen-nominations rule (37.12z) was merged in on
  2026-10-21. Multi-place seats and the tie rule (37.1o), filled ER
  forms (37.1q) and device details on the turnout record (37.1r) are done and archived.
- **37.14t** — The form verification page currently shows that the election and form exist, but does not prove that the paper is genuine or unchanged. Owner decision 2026-10-27: verification must establish authenticity and integrity, and reject forms that could not yet exist in the election phase. Bind the proof to the canonical printed content; settle the cryptographic construction during implementation design.
- **92.1–92.5** — No approval flow can be switched off or given to another role; all are
  hardcoded AdminOnly. Spec 022 first.
- **67.5** — Book under 100 pages, Chapter 1 to References. Layout done; prose pass waits on the
  author's merge and cut confirmations. (P1 in its own entry.)
- **42.1–42.5** — Done 2026-10-21 by merging into 37.14s (form wording from settings, print any form
  blank or filled).
- **51.4–51.5** — Remaining file-storage hardening: broader regression coverage and admin-configurable
  settings.
  tests, compression settings not admin-configurable yet.
- **47.10** — Missing profile photos for most of the 631 bulk-imported alumni (data gap, not a bug).
- **27.8** — No enforced ≥80%/file coverage threshold (would fail today if enforced).
- **28.32 / 28.33 / 8.8** — i18n (English+Bengali): dependencies present, extraction not started.
- **34.D7** — 21 stale mobile golden baselines need regenerating (unrelated housekeeping).
- **44.16** — Three unrelated Flutter classes all named `FamilyService` (deferred rename risk).
- **7.16, 8.5, 8.7** — Mobile hardening and local storage (SSL pinning, local database, state hydration). 8.3, 8.4 and 8.6 are done. Other performance work is under Work Package 98.
- **12.1–12.6** — [DONE] Process items (API-change checklist, contract registry, mobile log
  capture), moved to docs/TODO_ARCHIVE.md on 2026-10-14.
- **52.5** — Mobile's separate "quick create gallery" dialog — left as-is, a future cleanup decision.
- **82.3 / 82.4 / 82.5 / 82.7 / 82.8 / 82.10a** — Cross-cutting engineering gaps measured 2026-09-04: no
  API versioning against an independently shipped mobile client; three error-response shapes; 55 copies
  of claim parsing in the controllers; seven Angular components bypassing the service layer; paging
  applied to only a fraction of the list endpoints; and no published API contract to diff, the OpenAPI
  document being served in Development only.

### P3 — LOW / PLAN-ONLY (large unbuilt features, no current pressure)
- **Work Package 37** (37.4, 37.5, 37.7, 37.9, 37.10) — cohorts and reunions, the In Memoriam register,
  bilingual UI, geographic chapters and the annual impact report. 37.2, 37.3, 37.6 and 37.8 are done and archived.
- **6.2** — Alumni referral system for jobs/internships.
- **62.56a–62.56g** — Export and import of the organisation profile, settings and configurations as a zip pack, with a section map,
  a dry-run preview, and "add only" or "override" per section. Zip only (owner, 2026-10-15).
- **82.6 / 82.9 / 82.10b / 82.11–82.13** — Audit follow-through: split `MemberService` (1,577 lines),
  request correlation and structured logging, typed Dart models for the auth, profile and payment
  payloads, the configuration and constants classifications, and the missing decision records and
  recovery runbook. (82.10, whether to generate clients from OpenAPI, is decided and closed: rejected.)
- **61.3** — Drop the `Summary:`-style comment banner in `GHCAA.Tools/db_diag.cs` next time that file is touched.
- **84.4** — Web/Mobile client parity table; depends on 84.3 existing first. (84.5, the
  authorization and error catalogs, is done.)
- **84.6 / 84.7** — Election ballot workflow scope and white-label second-institution deployability —
  product decisions for the project owner, not documentation tasks.

<!-- Closed Work Packages (1, 2, 3, 4, 5, 9, 10, 11, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 29, 30, 31, 32, 33, 35, 36, 38, 39, 41, 44, 45, 46, 50, 51, 53, 54, 55, 56, 57, 58, 59, 61, 66, 68, 69, 70, 71, 80, 43, 48, 83, 85, 87, 89) moved to docs/TODO_ARCHIVE.md on 2026-09-15 (43, 48, 83, 85, 87 and 89 on 2026-10-11) to keep this file lean. wbs.py reads both files for tracker counts. -->

## WORK PACKAGE 6: CAREER & OPPORTUNITIES

6.2 [TODO] **Priority: P3.** Alumni referral system for jobs and internships

## WORK PACKAGE 7: SECURITY, INFRASTRUCTURE & HARDENING

7.16 [IN-PROGRESS] **Priority: P3.** Hardening: SSL Pinning and Binary Obfuscation. Obfuscation is done: `mobile_deployment.yml`'s Android and iOS release build steps now pass `--obfuscate --split-debug-info=build/debug-info`, and that directory is uploaded as a separate `android-debug-info`/`ios-debug-info` artifact (90-day retention) so a crash report from a given release build can still be symbolicated later. SSL pinning is scaffolded, not enforced: `GHCAA.Mobile/lib/core/api/ssl_pinning.dart` gates pinning on `AppConfig.environment == 'production'` (dev/preprod are never affected) and only enforces it once `kProductionCertificateSha1Pins` holds a real value — `ssl_pinning_io.dart` wires the check into Dio via a `SecurityContext(withTrustedRoots: false)` + `badCertificateCallback`, `ssl_pinning_stub.dart` is the web no-op, `api_client.dart`'s `dioProvider` calls it. `kProductionCertificateSha1Pins` is still empty: there is no `.env.production` in this repo (only `.env` for dev and `.env.preprod`) and no way from this environment to fetch or confirm the real production certificate, so pinning stays a documented no-op rather than risk pinning a guessed value and locking out every user until an app update. Remaining step for someone with access to the live production host: run the `openssl s_client`/`openssl x509 -fingerprint` command documented at the top of `ssl_pinning.dart`, add the fingerprint to `kProductionCertificateSha1Pins`, and update `ssl_pinning_test.dart`'s placeholder-state test to assert `isPinningEnforceable('production')` is true. VERIFIED 2026-09-17: `flutter test test/ssl_pinning_test.dart test/mobile_deployment_obfuscation_test.dart` (9 tests: environment gating, enforceability-while-placeholder, fingerprint hex encoding, and a grep-assertion that both CI release build lines carry `--obfuscate`/`--split-debug-info`) and `flutter analyze` on all touched files, both clean.
7.21 [TODO] **Priority: P2 | Depends on: 7.20, owner DNS.** Check the `www` redirect on the live site. haragangian.com, www.haragangian.com and preprod.haragangian.com did not resolve on 2026-10-05. Once DNS is live the owner adds the Render custom-domain redirect, then `curl -I https://www.haragangian.com/x?y=1` must answer 301 with `Location: https://haragangian.com/x?y=1`. Owner step. Raised 2026-10-05.
7.22 [TODO] **Priority: P1 | Depends on: none.** Preprod origin settings point at a domain that does not resolve. On 2026-10-05 `https://ghcaa-ryl6.onrender.com` answered CORS with `access-control-allow-origin: https://preprod.haragangian.com`, so Render's `AppSettings__AllowedOrigins__0` is the dead domain, and ClientUrl very likely is too, which would send payment returns there. Owner step: set both to `https://ghcaa-ryl6.onrender.com` on Render, and change the preprod lines in the root `.env.preprod` and `GHCAA.Mobile/.env.preprod`. The code does not change; `docs/ENV_REVIEW.md` already shows the right value. Raised 2026-10-05.
  - **Review, 2026-10-14.** `GHCAA.API/appsettings.Preprod.json:6-9`, added by 96.15, also names the dead domain `preprod.haragangian.com`. It belongs in the owner's fix list with the Render settings; it is not edited here because it is a settings file.
  - **Owner answer, 2026-10-14.** The preprod origin is the Render web address. The API serves the web client from the same host, so the origin is `https://ghcaa-ryl6.onrender.com`; the owner sets it in `GHCAA.API/appsettings.Preprod.json:6-9` and in Render's `AppSettings__AllowedOrigins__0` and ClientUrl. The owner confirms the address when making the change.
7.26 [PARTIAL 2026-10-14] **Priority: P1 | Depends on: 37.1y.** Preprod sends no email, so sign-in codes, step-up codes and every notice fail. Render's free tier blocks outbound SMTP and `GmailEmailService` connects to `smtp.gmail.com:587`. 37.1y made the failure show; this item makes sending work. Code done 2026-10-14: a second sender, `BrevoEmailService`, posts to Brevo's HTTPS API, and `EmailSettings:Provider` picks `Gmail` (the default, so local and dev setups do not change) or `Brevo` as the server default; any other value stops startup in `DependencyInjection.AddEmailService`. Changed 2026-10-15 at the owner's request: an admin picks the sender under Settings, Workflow tab, Email Sender, saved as `Workflow.EmailProvider` in OrganizationConfig. `EmailServiceRouter` reads it on each send, so a switch needs no restart, and a blank or unknown value falls back to `EmailSettings:Provider`. Keys stay in env. `GET healthz` checks the settings of the provider in use (spec 020 FR-016). Docs: `docs/DEPLOYMENT_CHECKLIST.md` §1, `docs/ENV_REVIEW.md`, `docs/CONFIG_DRIVEN_FRAMEWORK.md`, `GHCAA.API/.env.example`. Tests: `BrevoEmailServiceTests` (missing settings, the request sent, no sender name, a refused send, a network failure) `HealthControllerTests` (checks follow the admin pick, then the server default), `EmailServiceRouterTests` (admin pick, server default, a failed config read), `OrgConfigServiceTests` (the provider name is saved normalized) and the org-config web spec. Owner steps left: create a free Brevo account, add and verify the sending address under Senders, create an API key, then set `BrevoSettings__ApiKey`, `BrevoSettings__SenderEmail` and optionally `BrevoSettings__SenderName` on Render, redeploy, and pick Brevo under Settings (or set `EmailSettings__Provider=Brevo`). Close when a step-up code reaches an inbox from preprod and the `EmailLogs` row reads Sent. Local env files (`.env`, `.env.dev`, `.env.preprod`, and `GHCAA.API/.env`, `.env.dev`, `.env.preprod`, `.env.prod`) still need the commented Brevo block from `.env.example`; they hold real values, so the owner pastes it.
7.28 [PARTIAL 2026-10-19] **Priority: P1 | Depends on: none.** Uploads on preprod are lost on every redeploy, because Render wipes the container disk. Raised by the owner on 2026-10-19, who added an S3-compatible bucket (`ghcaa-storage`) to `GHCAA.API/.env.preprod` and asked for all preprod images to move there and be served from there. Code done 2026-10-19: `FileStorage:Provider` picks `Local` (the default) or `S3`; `S3FileStorageService` saves, reads and deletes in the bucket, sharing path rules and image compression with the disk store through `FileStorageServiceBase`; `/api/uploads` streams from the bucket when the provider is `S3`, with the same thumbnail fallback and placeholder; `SecureFilesController`, `IDCardService` and `healthz` read through `IFileStorageService`. Tests: `S3FileStorageServiceTests`, `UploadProxyTests`, `FileStorageRegistrationTests` and new `LocalFileStorageServiceTests` cases; full backend suite 1497 passed. Data: the local `GHCAA.API/wwwroot/uploads` tree (631 files, 62,335,395 bytes) was copied to `s3://ghcaa-storage/uploads` with `aws s3 sync`, and a second dry run found nothing left to copy. Docs: `DEPLOYMENT_CHECKLIST.md` Render variables, `RECOVERY_RUNBOOK.md` Scenario 3 (uploads lost), `ENV_REVIEW.md` registry, spec 020 FR-015.
  - **Open, owner steps.** Set `FileStorage__Provider=S3`, `FileStorage__S3__BucketName=ghcaa-storage` and the four `AWS_*` variables on Render before the next deploy. Files uploaded on the live site since its last deploy exist only on that container's disk and are not in the local copy; they need copying into the bucket before the redeploy, or they are lost with it. Listing them needs the upload paths from the Neon database.
  - **Review findings, 2026-10-20.** The owner asked for the findings left open on 2026-10-19 to follow standard behaviour. `OpenReadAsync` now returns a `StoredFile` with the stream, last-modified time and ETag. `/api/uploads` on S3 and `SecureFilesController` pass these on with range processing on, so a repeat request with `If-None-Match` gets 304, HEAD sends headers only, and `Range` gets 206. The S3 read stream can seek: a read at a new position asks the bucket for that byte range of the same version (`EtagToMatch`). A failed write after image compression now throws after one attempt, without a second upload. A thumbnail delete that fails is logged and does not fail the photo delete, and deleting a thumbnail key deletes only that key, on both stores. `IDCardService` reads the photo from the store first and falls back to the web root for seeded `assets/` photos, refusing a path that leaves the web root. Proxy tests confirm `..` and encoded `%2e%2e` reach the store as `..`, which the store refuses. Left as is, with reasons: S3 `ThumbnailExists` still answers true and the proxy falls back to the full photo, which saves one HEAD request per gallery tile; the S3 health probe is not cached, matching the database probe, and the rate limit on `healthz` bounds it. A code review of these fixes found three more, fixed the same day: the S3 health probe now gives up after `Constants.Deploy.StorageProbeTimeoutSeconds` (5 s) and reports a timeout as Unhealthy instead of an error; a disk file deleted between the exists check and the open now reads as missing instead of a 500; and a test pins that the stores read only under `uploads/members` and `secure_uploads/members`, where every upload is saved (the seeded `uploads/receipts/*.png` payment rows point at files that were never on disk). Tests tagged NFR-P4, NFR-R4 and NFR-S7 in `UploadProxyTests`, `SecureFilesControllerTests`, `S3FileStorageServiceTests`, `LocalFileStorageServiceTests` and the new `IDCardServicePhotoTests`; full backend suite 1519 passed. Docs: spec 020 FR-015 (upload store health check), `DEPLOYMENT_CHECKLIST.md` (the bucket credential needs list rights).

## WORK PACKAGE 8: MOBILE ENGINEERING (TIER-1 STANDARDS)

8.5 [TODO] **Priority: P4.** Persistence: Switch to High-Performance Local DB (Isar/Drift)
8.7 [TODO] **Priority: P3.** State: Riverpod State Hydration (Local local persistence)
8.8 [PARTIAL 2026-10-09] **Priority: P2.** i18n: Unified Localization (English + Bengali). Audit 2026-10-09: `GHCAA.Mobile/lib/core/services/app_localizations.dart` holds a small hand-written `en`/`bn` map. There is no ARB or `l10n.yaml` pipeline and most screen strings are not extracted yet.
## WORK PACKAGE 12: PROCESS & ENGINEERING STANDARDS

12.8 [TODO] **Priority: P3 | Depends on: user approval.** `docs/` holds dated reviews, old plans, duplicate proposal versions and two files that carry secrets, next to the live docs. An audit on 2026-10-06 listed what to archive, merge, fix or untrack. Nothing gets moved or deleted until the user approves the list, and any move updates the book build refs and README links in the same change. Requested 2026-10-06.
  - **Review, 2026-10-14.** A third file with secrets sits in `docs/`: `docs/deploy_conn_Info.txt`, untracked. The tracked two are `docs/appsettings.txt` and `docs/dgepay_info.txt`. The approval list should name all three.
  - **Owner answer, 2026-10-14.** Leave the three secret files as they are for now. Asked again when the docs clean-up runs.

12.9 [PARTIAL 2026-10-20] **Priority: P3 | Depends on: none.** Review of all 24 folders in `docs/specs` against the code and this tracker, requested 2026-10-20 together with a check of the open election items. Fixed the same day, as plain factual corrections:
  - Spec 001 (platform baseline) and spec 002's task list: 48 controllers, not 45 or 35. Spec 001's domain table now names ElectionAppointments, ElectionPersonas and ElectionRulesUnlock under spec 018.
  - Spec 003 (alumni programmes): status says 37.2, 37.8 and the 37.0 prerequisite are done and 37.5, 37.9 and 37.10 are open. Spec 009 (campaigns, scholarships, archive): 37.3a, 37.2 and 37.6 marked as shipped.
  - Spec 014 (events and content): the gallery upload route is `upload-photo`. Spec 016 (networking): the edge case that called every directory action public now matches FR-002, where `member/{id}` needs sign-in.
  - Spec 017 (communication) no longer calls spec 019 untracked. Spec 019 (assistant): the rate limit is the `GlobalLimiter` in `RateLimitingExtensions.cs`, not the removed `RequireRateLimiting` call in `Program.cs`.
  - Spec 018 (governance and elections): add and remove candidate, count, declare, tie and multi-seat text, FR-023, FR-024, FR-031, SC-002 and the test-coverage note now describe the code as built since spec 023's fixes. The scrutiny-bypass clarification is dropped because `AddCandidateAsync` now adds a Submitted nomination.
  - Spec 020 (platform operations): the health trace rows for FR-014 to FR-016 cite `HealthControllerTests`, and FR-016 is tagged code and test.
  - Status lines: spec 023 (ballot secrecy) remains in progress; see its entries in `docs/TODO.md` for current open work. Spec 024 (performance) has only 98.7 open plus 98.34 on hold; spec 025 (config pack) has FR-18 built and FR-1 to FR-17 not started.
  - Still open, larger than a fact fix: spec 004 (backlog roadmap) lists closed items as active (37.2, 37.6, 47.13, 60.2, 60.5, 65.5, 73.7, 78.12, 81.1 to 81.3, 84.5, 84.6), repeats the 81.1 to 81.3 heading, and quotes 37.8, 37.9 and 37.10 against the wrong subjects. Spec 010-election-engine-fixes describes a ballot model (`SerialNumber`, `IssuedAt`, `CastAt`) and a "not met" secrecy rule that spec 023 replaced. Spec 011 (election module redesign) has no status line, reads as new work although 37.1h is done, and keeps positions as a fixed enum, which the 2026-10-14 decision under 37.1h-1 reverses. Spec 012 (membership and auth) has no requirement for the configurable access-token lifetime and OTP attempt limit, and its ENH-004 still says the web fee is hardcoded. Spec 013 (profile) FR-022 still says `SecureFilesController` reads `FileStorage:BasePhysicalPath`; it goes through `IFileStorageService` once the S3 work under 7.28 lands. Spec 014 cites drifted line numbers for FR-004, FR-005 and the gallery upload. Spec 017 does not mention the Brevo sender or `Workflow.EmailProvider` from 7.26. Spec 019 says mobile posts `question` while the API binds `Query`, and no tracker item covers that bug. Item 90.1 (ad-hoc reporting) names the column `CreatedBy`; spec 010-ad-hoc-reporting says `CreatedByUserId`.
  - Election check, 2026-10-20: none of the 21 open Work Package 37 items has been built since its text was written. 37.12x (the web election test that fails in a full run) is still at `admin-elections.spec.ts:256` and not skipped; one clean full web run would close it.
  - Update 2026-10-21: 37.12x closed after a full web run passed 792 of 792. 37.1h-1 and six overlapping items (37.12n, 37.12o, 37.12r, 37.12s, 37.12t, 37.12z) were merged into the items that cover them. 16 Work Package 37 items are open now, counted by grepping the item tags.

## WORK PACKAGE 27: TEST COVERAGE IMPROVEMENT

27.1  [IN-PROGRESS] Generate low‑coverage report (parse coverage.cobertura.xml). Reporting began 2026-09-16: Coverlet baseline is 59.99% lines and 39.61% branches (7,272/12,122 lines; 1,768/4,463 branches); latest Vitest V8 report is 42.25% lines and 28.44% branches across `src/app` (2,453/5,805 lines; 866/3,045 branches); Flutter LCOV baseline is 14.23% lines (1,392/9,781). All reports are retained by CI. `docs/TEST_COVERAGE_PLAN.md` defines the shared collection, test-selection and threshold method before any threshold is enforced.
27.8 [TODO] **Priority: P2.** Run coverage and enforce ≥ 80 % per file (Still genuinely open, confirmed 2026-08-22: `coverlet.collector 6.0.2` is referenced so coverage *can* be collected locally, but no threshold is enforced anywhere and README explicitly declines to claim a figure. Enforcing >=80%/file would fail today.)
27.10 [IN-PROGRESS — tracked in 82.86] API: Add focused negative tests for the four business-rule coverage gaps (COV-001 through COV-004), including intended 4xx mapping for invalid business input. Use 82.86 as the canonical execution record.

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

(28.1 to 28.9, the OrgConfig backend, done; moved 2026-10-14 to docs/TODO_ARCHIVE.md under Work Package 27)

### PRIORITY 2 - PHASE 2: ANGULAR CONSUMER

> DEPENDS ON: 28.0 (migration applied, GET /api/config returning 200)

### PRIORITY 3 - PHASE 3: FLUTTER CONSUMER

> DEPENDS ON: 28.11-28.16 (Angular consumer pattern validated first)

### PRIORITY 4 - TESTS

> DEPENDS ON: 28.0 (migration), 28.4 (service implemented)
> 28.25 [TODO] Tests: Re-run visual regression snapshots after Phase 2 Angular consumer is done **UNVERIFIABLE AS WRITTEN 2026-08-22.** Its dependency 28.15 is now confirmed done, so the trigger condition has passed — but this is a *run this command* item, not an artifact, so nothing on disk can prove it happened. Either run `npx playwright test --update-snapshots` and record the result here, or close it as superseded by 28.26's `config-regression.spec.ts`.
> Command: npx playwright test --update-snapshots
> WHY: APP_CONFIG references replaced by config-driven values may shift text in layout
> DEPENDS ON: 28.15

### PRIORITY 5 - ADMIN UI (PHASE 4, OPTIONAL)

> DEPENDS ON: 28.12 (Angular OrgConfigService)

### PRIORITY 6 - TECH DEBT CLEANUP (after all phases pass regression)

> DEPENDS ON: All of 28.1-28.28 green; do NOT delete Constants.cs fields before this
> 28.29 [VERIFIED-STALE 2026-08-22] Cleanup: Add DEPRECATED comment to Constants.Branding.* and Constants.EmailSubjects.* **OBSOLETE — nothing to deprecate.** Verified 2026-08-22: `GHCAA.Domain/Constants.cs` contains only `Roles`, `ConfigKeys`, `TemplateCodes`, `Defaults`. There is no `Constants.Branding` and no `Constants.EmailSubjects` class, so there is no field to mark DEPRECATED. Closing as not-applicable rather than done.
> Mark: // DEPRECATED: use IOrgConfigService; pending deletion after 28.30 complete
> 28.30 [VERIFIED-STALE 2026-08-22] Cleanup: Migrate call sites - CommunicationService email subjects + any service using Constants.Branding.* **OBSOLETE — no call sites exist.** Verified 2026-08-22: `grep -rn "Constants.Branding|Constants.EmailSubjects|Constants.Defaults.SupportEmail" --include=*.cs` (excluding bin/obj) returns **zero** hits, so there is nothing left to migrate. Closing as not-applicable.
> Grep: Constants.Branding | Constants.EmailSubjects | Constants.Defaults.SupportEmail
> DEPENDS ON: 28.29
> 28.33 [TODO] Mobile: Add Flutter intl + .arb files for UI-layer strings **PARTIAL, confirmed 2026-08-22:** `intl: ^0.20.2` and `flutter_localizations` are already in `pubspec.yaml`, but there is **no `lib/l10n/` directory and no `.arb` files** — the dependency is in place and the extraction work is not started. **CONFLICT noted 2026-09-27:** `app_localizations.dart` already ships a working flat `en`/`bn` dict + Riverpod toggle (the mechanism 8.8 actually wants) with only ~25 keys covered. Whoever picks this up decides once: extend that dict (cheaper, mirrors 37.7's Angular approach) or migrate to `.arb`/`intl_utils` — do not do both. Recommendation: extend the existing dict and close 28.33 as superseded by that decision, consistent with 28.32's resolution.
> RELATES TO: 8.8 [TODO] i18n: Unified Localization (English + Bengali)

## WORK PACKAGE 34: ADMIN-MANAGED SITE CONTENT + MERGED NEWS & NOTICE BOARD (raised by user 2026-08-02)

> User instruction set: (1) update About Us with college + association information, (2) add an On Campus Address to Contact Us, (3) make both manageable from admin, (4) merge News and Notice management, notices may carry PDF documents. Standing constraints: **notices are admin-post-only**; About content is sourced from the GHCAA Constitution text and facts already in the repo **only** (no web research, no invented college facts); web + mobile parity; OrgConfig additions must be industry-standard and configurable; nothing may break.

### 34.A — SiteContent CMS (About Us / Contact intro)


### 34.B — Contact Us on-campus address (OrgConfig)


### 34.C — News + Notice unified


### 34.D — Cross-cutting (raised mid-implementation by user)

34.D7 [TODO] Regenerate the stale mobile goldens (`flutter test --update-goldens`) as a standalone housekeeping task, so a genuinely new mobile regression is not masked by the existing baseline drift. Deliberately kept out of this area — it is unrelated binary churn from a prior session's theme fix. **Count corrected 2026-09-06:** the original "21" is stale — 80.18 found 58 golden/pixel-diff failures against the current tree (confirmed pre-existing, not a regression, via a clean-tree rerun), out of 80 golden PNGs total. Per the note already in `test/flutter_test_config.dart`, these fail locally on any non-Linux font stack and are deliberately skipped in CI (`skipGoldenAssertion`), so this item is about refreshing the baselines for local dev convenience, not fixing a CI-blocking problem.
34.D8 [TODO] Live-browser verification of this area: public `/about` (seeded CMS blocks + hardcoded fallback), `/contact` (campus address, phones, map guard), `/news?type=Notice`; admin CMS block edit round-trip; admin notice creation with a PDF and a working public download link; and a **member account confirming it cannot create a Notice** (34.C3). **Blocked by the same local seed/role mismatch as 33.13** — `shalin` and `superadmin` resolve to ordinary alumni members in the local DB, so `adminGuard` bounces `/admin/*`; the local role assignment must be fixed before the admin-side steps can run.

34.D17 [IN PROGRESS 2026-10-01] [P0] Preprod log (2026-10-01) shows Postgres `42703 errorMissingColumn` on member-summary queries. The log never names the column. Preprod was first built by `EnsureCreated`, so a column the model has can be absent live even though every migration is marked applied. Fix: `MigrationBootstrapper.RepairMissingColumnsAsync` runs after `MigrateAsync` on Postgres. It compares the EF model with `information_schema.columns`, replays a migration rename when the old column is still there, and otherwise adds the column with `ADD COLUMN IF NOT EXISTS` and the model default (or the type's zero value for NOT NULL). Identity, computed and jsonb NOT NULL columns are logged for a manual fix, not guessed. A failed repair is logged and does not stop startup. 10 new unit tests pin the SQL (`MigrationBootstrapperAddColumnTests`). Code backup on local branch `backup/2026-10-01-column-repair`. Open: not yet run against a real Postgres (the local boot against the prod-data database was blocked by the tool permission check); confirm on the next preprod deploy by searching the startup log for `was missing from the live schema`. The same repair runs on production at startup. Depends on: user redeploying preprod.
  - **Plan review, 2026-10-14.** F2 waits for this to be confirmed on preprod, with 34.D18 and 34.D19, because F2 adds a migration on the same chain. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.

34.D19 [IN PROGRESS 2026-10-13] [P1] Every member directory card shows "Batch: N/A (N/A )" (raised by user 2026-10-13: "there was a logic for displaying like, batch of HSC 2001"). The card code is fine: it reads `PassingYear` and `Degree` from the academic record with `IsOrgProfile` set. The data is not. On the local database all 586 `AcademicRecords` rows have the old `IsGHC = true` and an empty `IsOrgProfile = false`, so no member has an org record. Cause: `20260921110115_RenameIsGhcToIsOrgProfile` was committed without its Designer file, so it had no `[Migration]` attribute and EF never ran it. `RepairMissingColumnsAsync` (34.D17, the startup column repair) then saw the model column missing and added it with the zero value. Fix: the migration now carries its attributes and runs guarded SQL. With `IsGHC` alone it renames the column; with both columns it sets `IsOrgProfile` where `IsGHC` is true and drops `IsGHC`. Dry run on a copy of the local database: 586 rows flagged, a second run changes nothing, and the rename-only shape also ends with 586 flagged. Guard test `MigrationBootstrapperSelfHealTests.EveryMigrationClass_IsDiscoveredByEf` fails for any migration class EF cannot find. Open: the fix applies at the next API start; confirm locally and on the next preprod deploy that directory cards show a year and degree. Preprod has not been queried, but it ran the same code, so it is very likely affected.
  - **Plan review, 2026-10-14.** F2 waits for this to be confirmed on preprod, with 34.D17 and 34.D18, because F2 adds a migration on the same chain. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.

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

  - **37.1m Printable receipt and published tracking codes [PARTIAL 2026-10-09] Priority: P1 | Depends on: 37.1i, 37.1l, decision D1.** No receipt exists. Web print stylesheet with `window.print()`, mobile PDF on the device, both built on the client only. Tracking-code list published after close with no choices. Spec 023 FR-015, FR-016 and its receipt and print standard. Built 2026-10-09: the receipt is made from the choices held in memory on the confirmation screen and printed once (web print rule on `.vote-receipt` in `styles.scss`, mobile PDF through `Printing.sharePdf`, nothing written to disk). `GET /api/elections/{id}/tracking-codes` is anonymous and waits for the count, not only for polling to close, because receipt rows are in cast order until the count rewrites them. Test: `GetTrackingCodesAsync_StaysHiddenUntilTheCountHasRun`. Left: look at the printed receipt in a browser and on a phone. The first plan also bundled a Unicode font for the mobile PDF, since the default Helvetica cannot draw Bengali names; 37.14o replaced that with a server-made receipt. Decided 2026-10-14: Noto Sans Bengali, bundled in the mobile assets. Session F1. Update 2026-10-20: a font alone does not fix the receipt. The Dart `pdf` package (3.12.0) shapes only Arabic text, so with Noto Sans Bengali bundled the vowel signs that sit before a consonant (ি, ে, ৈ) would still print after it and joined letters would break apart. Waiting on the owner to choose how Bengali names reach the PDF. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track. Decided 2026-10-20: the receipt is made on the server with QuestPDF and a bundled Noto Sans Bengali font, which shapes Bengali text correctly. It prints the receipt code, the election and the time, and no choices, so nothing about the vote is sent to or kept on the server. Built under 37.14o.
  - **37.1p Recount, candidate consent, officer conflict, spoiled and unopposed seats [TODO] Priority: P2 | Depends on: 37.1k.** None of these rules exist in code. Spec 023 FR-020 to FR-024. The unopposed rule and the recount window need the constitution checked first. Decided 2026-10-11 and 2026-10-14, no constitution read needed: recount request per seat, approved by the Chief Commissioner as a two-person step; officials cannot be nominees in the same election and declared conflicts (self-declared at acceptance, the Chief Commissioner can add) keep them out of that candidate's decisions; unopposed seats are left off the ballot and declared at declare, and an election with every seat uncontested goes from CandidateList to Declared as a two-person step with step-up; paper counts entered by a polling officer and approved before the count. Sessions F2 and F3. Merged 2026-10-21 from 37.12z: when a seat has no valid candidate, the Chief Commissioner reopens nominations for that seat only, for a period set per election (default 3 days) that ends before the roll freeze; a seat still empty at polling stays vacant until a 95.5 by-election. Update 2026-10-20: not built. The F2 migration waits until 34.D17 to 34.D19 (the preprod start-up and migration repairs) are confirmed on preprod, as the delivery plan says. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.
  - **37.12y Extend polling [TODO] Priority: P2 | Depends on: 37.12w.** Decided 2026-10-11: up to 24 hours at a time, at most twice, while Polling is open. Chief Commissioner, two-person, step-up, reason required, logged. Decided 2026-10-14: voters are told through the existing notification service and the board shows the new closing time. A third request is refused and points to the L3 void-and-rerun procedure. Session F3. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.
  - **37.13f Incident response, threat model and compliance table [PARTIAL 2026-10-10] Priority: P3 | Depends on: none.** Two short documents and a table mapping each standards requirement to the code or TODO item that meets it. Standards §17 and §19. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/plan-officials.md`. Raised by the online-voting review, `docs/Elections/00-Election-Online-Voting-Review.md`, 2026-10-04. The threat model names the design a trusted-server secret ballot and says what that does and does not protect: who can link a voter to a ballot (the server operator during polling, someone holding a polling-day backup and the key file), what the client is trusted with, and where the Render platform boundary sits. The incident procedure covers a lost or leaked key, a suspected link between voter and ballot, and a stopped poll. A logging and retention note lists what the vote and count routes log and for how long. Draft written 2026-10-09 in `docs/Elections/09-Election-Security-and-Compliance.md`, left open for the committee. It holds nine *[to confirm]* points (Neon backup and point-in-time settings, Render log retention, retention of vote dates and the activity log, severity levels). Gaps it found: `docs/DEPLOYMENT_CHECKLIST.md` asks for a Neon branch before a migration with no warning that a branch taken during polling is a copy of the sealed rows; a failed count writes no log line of its own; no backup schedule, RTO or RPO is written down. Update 2026-10-10: `docs/Elections/09-Election-Security-and-Compliance.md` now has a row for each area of the standards section 21 matrix. Software integrity and availability are marked as gaps to confirm. The nine *[to confirm]* points above are still open. Code check 2026-10-14: 11 *[to confirm]* points left. Session F7. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.
  - **37.13g Manual accessibility check of the vote screens [TODO] Priority: P3 | Depends on: none.** WCAG 2.2 AA checklist run once with keyboard only and a screen reader. Standards §13. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/plan-officials.md`. Checklist drafted 2026-10-09 in `docs/Elections/10-Election-Accessibility-Checklist.md`, one table per screen (vote, receipt, tracking-code check, results). Rows for the abstain label and help line, untouched seats, and the ineligible notice added the same day for 37.1z. Not yet run; it needs a person with a keyboard and NVDA or TalkBack. Run when F5 and F6 close. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.

  - **37.14a Key copy count and holder confirmation not held [TODO] Priority: P3 | Depends on: 37.13e.** Decided 2026-10-11: the Association sets the number of sealed key copies (default 2, at least 1) and the Chief Commissioner names the holders, each a different appointed official who is not a candidate and who confirms receipt. No setting, table or check holds any of this. Doc 11 and handbook section 5 give it as procedure and say so. Owner decision 2026-10-27: polling opening waits until the configured copy count matches recorded copies and every configured holder confirms receipt (see 37.12w). For implementation, use the existing election settings in `OrgConfig` unless review finds a concrete constraint; store holders and confirmations and expose unmet custody readiness. Session F3. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.
  - **37.14d Audit chain cascades from Elections and a cut tail is not detected [PARTIAL 2026-10-20] Priority: P2 | Depends on: 37.13a.** The audit table has a foreign key that cascades from `Elections`, so deleting an election removes its chain. Entries cut off the end of the chain cannot be detected without a chain head held outside the database. Half done: ER-29 and the public board print the chain head, and the move to Restrict is left. Decided 2026-10-14: foreign key to Restrict, and the chain head hash printed on ER-29 and the public board. Session F2. Update 2026-10-20: ER-29 now prints the chain head hash in its declaration table, the same value as the public board, and "Not declared" before that. Test `ER29_PrintsTheAuditChainHead`. The foreign key change to Restrict is in the F2 migration, which waits for 34.D17 to 34.D19 on preprod. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.
  - **37.14f Ten-year purge by request [TODO] Priority: P3 | Depends on: none.** Merged 2026-10-21 from 37.12s: first a test that pins the ten-year rule (Regulation Article 17), showing that no archive, cleanup or restore job removes ballots, audit entries or approvals early. The 2026-10-14 code check found nothing that deletes election rows, so only that test is missing. Decided 2026-10-11: no automatic delete. The dashboard lists elections more than ten years past declare; purge needs a request and a second approver (SuperAdmin when no Commission sits). The summary result and the purge record stay. Session F3. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.
  - **37.14m Stored fields for the election forms [PARTIAL 2026-10-21] Priority: P2 | Depends on: none.** Add the fields the owner approved on 2026-10-20: the schedule dates, campaign and silence period, venue, notice reference and recount window end on `Election` (ER-01, ER-02); the seven scrutiny ticks on each nomination with who set them and when (ER-17, any commissioner may tick); one recount request record per filing holding the request and the decision (ER-28, ER-28A, ER-28B); and the 15 handover ticks (ER-34). Merged 2026-10-21 from 37.12o and 37.12r: `ConsentAt` on `Nomination` for ER-10, with old rows printing "not recorded", and `RecountWindowEndsOn` on `Election`, with the destroy-key step refused until that date has passed. The migration is written and tested on local PostgreSQL only, then goes to preprod after the F2 gate (34.D17 to 34.D19). Run code-reviewer on the migration. See `docs/specs/023-election-ballot-secrecy-and-standards/form-inventory-draft.md`, section Owner decisions. Update 2026-10-21: built on the API, web and mobile. Owner choices: silence records a start time only, the handover ticks sit on the committee term (`ECPeriod`), and recount figures are stored on the request. Mobile adds a Form records sheet on each election card (form details, consent times, scrutiny ticks, recount requests and figures) and a Handover button on the governance registry. Tests pass on all three clients. Code review fixes the same day: the public nominations list no longer carries the consent time (officials read it from `GET /api/admin/elections/{id}/nominations`), the final list date is compared by day, a recount can only be recorded for a position with a declared result, and two officials saving a recount decision or figures at once get a 409 instead of a 500. Owner follow-ups the same day: consent entry stays open until polling (a paper signed earlier can still be entered during the campaign), the form-details audit entry now records the old values beside the new, the 409 path has a test with a mocked audit writer, and mobile reads election phase names from `ElectionPhases` in `app_constants.dart` instead of string literals. Still open: the migration goes to preprod only after the F2 gate (34.D17 to 34.D19, the preprod start-up and migration repairs) is confirmed.
  - **37.14n Election forms renumbered to the Regulations [PARTIAL 2026-10-22] Priority: P2 | Depends on: none.** Merged 2026-10-21 from 37.12t, which recorded the numbering conflict and the owner's 2026-10-20 decision: Regulations numbers ER-01 to ER-35 as they stand, template-only forms lettered after the form they follow, and today's 16 platform codes kept as old codes. Change `ElectionDocumentService.SupportedForms` to the Regulations numbers and the lettered codes for template-only forms. The plan to keep the old codes working was dropped on 2026-10-22 (see the update below). Update web and mobile form lists and tests on all three clients. The `# FORM` headings in `docs/Elections/05`, `06` and `07` were renumbered on 2026-10-21 and the interim crosswalk tables dropped. `08` and the Officials Handbook Annex B (the list of all 50 forms) now cite the new numbers only, the web forms register sorts and groups by them (`formNumber` and `formStage` in `election-docs.ts`), and the 05 nomination checklist now asks for ER-12, ER-13 and ER-11 in place of the wrong ER-04 and ER-10. See `docs/specs/023-election-ballot-secrecy-and-standards/form-inventory-draft.md`, section Owner decisions. Update 2026-10-22: owner decisions the same day were new codes only, with no old-code aliases, and a public verify page built here. `SupportedForms` now holds the 16 Regulations codes (ER-01, ER-02, ER-08, ER-10, ER-11, ER-17, ER-18, ER-19, ER-25, ER-27, ER-27C, ER-28, ER-28B, ER-29, ER-30, ER-34), and an old code answers 404 on `GET api/elections/{id}/documents/{formCode}`. The owner accepted that a QR code on a form printed before the change no longer resolves; the API contract registry records it as a breaking change. The QR code on each form points at `/verify/election/{id}/{formCode}`, built by `ElectionDocumentService.VerificationPath`. That page calls the new anonymous `GET api/elections/{id}/documents/{formCode}/verification` (rate limit `CredentialVerification`), which returns the election reference, title and phase and the form code and title, or 404 for an unknown election or code and for a test election (FR-047). It shows that the election and form exist, not that a paper copy is unaltered (see 37.14t, signed QR codes). Mobile has no form list and calls neither route, so it has no change. Tests: `Verify_KnownForm_NamesTheElectionAndForm`, `Verify_UnknownElection_ReturnsNull`, `Verify_TestElection_ReturnsNull` and `VerificationPath_MatchesTheWebRoute` in `ElectionDocumentServiceTests`; two `VerifyDocument` cases in `ElectionsControllerTests`; `election-document-verify.spec.ts` and the verify case in `elections.service.spec.ts`. The authorization catalog (`authz_catalog.py --write`) and the reflection test list carry the new route. Left: a look at the verify page in a browser and a scan of a printed QR code.
  - **37.14t A form's QR code does not prove the paper is genuine [TODO] Priority: P2 | Depends on: none.** Found by the security review of 37.14n on 2026-10-22. The verify URL holds only the election id and form code, so anyone can print a matching QR code on an altered or invented form and the page will confirm it. It also answers for a form the election's phase could not have produced yet, such as ER-29 before the count. Owner decision 2026-10-27: verification must prove the paper is genuine and unchanged and refuse forms that could not yet exist in the current election phase. Bind the proof to the canonical printed content; settle the cryptographic construction during implementation design. The page wording already says it confirms the record exists, not the paper.
  - **37.14o Server-made vote receipt with Bengali text [PARTIAL 2026-10-21] Priority: P1 | Depends on: 37.1m.** QuestPDF receipt with a bundled Noto Sans Bengali font, carrying the receipt code, election and time and no choices. Web and mobile download it instead of drawing their own. Raised by the owner 2026-10-20. Decided 2026-10-21: the cast reply carries the PDF beside the receipt code, so no later request holds the voter beside the code. It prints the code, election title, and the server's cast time to the minute in the Association's time zone. The web keeps its print view with choices as a second option; mobile drops its own PDF and sends the server one to the share sheet. Each client keeps the PDF on the device until the election closes and offers a Download receipt button on the election page. Noto Sans Bengali is registered as a QuestPDF fallback font for all election PDFs, so the ER forms print Bengali names too. Started before the F2 gate with the owner's agreement, since it needs no migration. Built 2026-10-21. Server: `ElectionDocumentService.GenerateBallotReceiptPdfAsync` makes the receipt; `POST api/elections/{id}/vote` returns it as `receiptPdfBase64` in `CastBallotResultDto`, and if the PDF fails the vote still stands and the reply carries the code alone (logged as an error). If the Bengali font cannot be registered, a warning is logged, the PDF prints with Lato alone, and the next PDF tries again. `AssociationTime` holds the Association's offset (UTC+6, Dhaka) and `ThemeService` now uses it too. `ElectionPdfFonts` registers the two Noto Sans Bengali weights, embedded from `GHCAA.Infrastructure/Fonts/` with their OFL licence, as the fallback for every election PDF. Web: the election page stores the PDF in `localStorage` under the member and election (`ghcaa_ballot_receipt_<memberId>_<electionId>`), shows Download receipt after the cast and on later visits, drops receipts for other elections, and drops this one once the election is Archived. Logout drops every stored receipt, and a damaged stored copy is dropped with an error message. The print view with choices stays. Mobile: the Dart receipt builder is removed, the PDF goes to `Printing.sharePdf`, and it is kept in shared preferences under `ballot_receipt_<electionId>` with the same archive rule; sign-out clears it. "Until the election closes" is read as until it is Archived, so a voter can still check the code against the published list after the count. Tests: `BallotReceipt_BengaliTitle_RendersWithTheNotoFallback`, `BallotReceipt_UnknownElection_ReturnsNull`, `GeneratePdf_BengaliName_PrintsWithTheNotoFallback` and `AssociationTime_IsSixHoursAheadOfUtc` in `ElectionDocumentServiceTests`; `Vote_ReceiptPdfFails_StillReturnsTheCode` in `ElectionsControllerTests`; receipt specs in `election.spec.ts`, including "drops a damaged stored PDF instead of throwing"; "drops stored ballot receipts on logout and keeps other settings" in `auth.service.spec.ts`; three receipt cases in `election_screen_test.dart` and the cast case in `election_service_test.dart`. Left: look at the receipt on a phone and in a browser. No OrgConfig setting holds the time zone yet, so `AssociationTime` is fixed at UTC+6; a white-label install in another zone needs one (area 62, genericization).

37.4 [TODO] **Priority: P3.** **Batch cohorts and reunions as first-class objects.** Today a batch exists only as `AcademicRecord.PassingYear` — there is no cohort page, no cohort representative and no reunion.
  - **Models.** `BatchCohort` (`Id`, `PassingYear`, `Title`, `Story?`, `CoverImagePath?`, `RepresentativeMemberId?`, `IsActive`); `Reunion` (`Id`, `BatchCohortId?` — null means an all-alumni reunion, `AlumniEventId`, `Theme`, `SouvenirUrl?`) built **on top of** the existing `AlumniEvent` + `EventRegistration` + `EventBudget` stack rather than beside it — a reunion is an event with cohort identity, and duplicating registration logic would be the mistake here.
  - **Membership is derived, not stored.** Cohort membership = `AcademicRecord` rows with `IsOrgProfile == true` and the matching `PassingYear`. Do not add a `BatchYear` column to `Member`; it would immediately disagree with `AcademicRecord` for anyone holding two GHC records.
  - **Ledger tie-in.** Reunion fees collected through `EventRegistration` post as `FinancialCategory.ReunionFee`, finally giving that enum value a producer.
  - **Service/API/UI.** `IBatchService` + `BatchService`; `BatchesController` at `api/batches` with `GET api/batches/public` and `GET api/batches/{year}` `[AllowAnonymous]`. Flag `enableReunions`. Public `/batches` (year grid) + `/batches/:year`, member `portal/my-batch`, admin `admin/batches`. New `API_ENDPOINTS.BATCHES` block.
  - **Tests.** NUnit: a member with two GHC `AcademicRecord` rows appears in both cohorts; non-GHC records excluded. Vitest: year-grid grouping, empty-cohort `EmptyStateWidget` path.
  - **Plan, 2026-10-14.** Session N5; mobile gets my batch and the cohort page. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.

37.5 [TODO] **Priority: P3.** Specified in
`docs/specs/003-alumni-programs-and-verification/spec.md` Story 3.
**In Memoriam register.** A new `MembershipType` value is **not** the mechanism — membership tier is admin-assigned and orthogonal (`feedback_membership_type_admin_only`). Instead: `MemorialEntry` (`Id`, `MemberId?` — nullable so a pre-digital alumnus can be honoured, `FullName`, `PassingYear?`, `DateOfBirth?`, `DateOfDeath`, `PhotoPath?`, `Tribute`, `IsPublished`, `SubmittedByMemberId?`, `SubmissionStatus Status`, `CreatedAt`) reusing the existing `SubmissionStatus { Draft, Pending, Approved, Rejected }` enum, and `Condolence` (`Id`, `MemorialEntryId`, `MemberId`, `Message`, `PostedAt`, `IsApproved`).
  - **Moderation is mandatory.** Nothing publishes without admin approval, and every condolence goes through the existing `HtmlSanitizer` path before storage. This is the single highest-sensitivity surface in the Area; an unmoderated tribute wall on a memorial page is a reputational incident.
  - **Setting `MemorialEntry.MemberId` must deactivate the linked `Member`** — and suppress them from the public directory and from any 37.1 voter roll — in the same transaction. A deceased member appearing on an election roll is the failure mode this clause exists to prevent.
  - **Service/API/UI.** Extend `IMemberService` **only if** the memorial logic stays under ~5 methods; otherwise `IMemorialService` + `MemorialService`. `MemorialController` at `api/memorial`, `GET api/memorial/public` `[AllowAnonymous]`. Flag `enableMemorial`. Public `/in-memoriam`, member submission form under `portal/`, admin moderation queue. New `API_ENDPOINTS.MEMORIAL` block.
  - **Tests.** NUnit: an unapproved entry is absent from the public projection; linking a `Member` deactivates them and removes them from directory results; condolence HTML is sanitised. Vitest: moderation-queue actions, published/unpublished rendering.
  - **Plan, 2026-10-14.** Session N4; mobile gets the list, entry page and condolence form. Decided 2026-10-14: a deceased member on a frozen roll raises a dashboard warning and the Chief Commissioner removes the entry through unfreeze and refreeze; nothing changes a frozen roll by itself. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.

37.7 [TODO] **Priority: P2.** **Bengali/English bilingual UI.** The association's constituency is Bengali-speaking and every string in the app is currently a hardcoded English literal in a template. **Do not install `@angular/localize` or `ngx-translate`** — `feedback_keep_lightweight` applies, and the requirement here is a single flat key → string lookup with a live runtime toggle, which `@angular/localize` (build-time, one bundle per locale) does not even satisfy.
  - **Mechanism.** `core/services/i18n.service.ts` holding a `signal<'en' | 'bn'>` persisted to `localStorage`; two dictionaries under `core/i18n/en.ts` and `core/i18n/bn.ts` typed as `Record<string, string>` with `en` as the key source of truth; and a pure `TranslatePipe` (`{{ 'nav.constitution' | t }}`) falling back to the English string, then to the key itself, when a Bengali value is missing. Update `<html lang>` on toggle. Ship the toggle next to the existing `<app-theme-toggle>` so it inherits placement and styling.
  - **Scope explicitly, and state it in the item when it lands:** public site + member portal nav, buttons, labels and validation messages. **Admin stays English-only** — it is staff-facing, and translating it doubles the surface for no constituency benefit. Server-stored content (constitution text, election documents, news) is not translated by this mechanism; it is authored content and belongs to whichever language it was written in.
  - **Font.** Bengali glyphs need a webfont with Bengali coverage. Production `ng build` inlines Google Fonts over the network and this environment cannot reach it — self-host the face under `public/assets/fonts/` and reference it from `styles.scss`, so the build stays offline-safe.
  - **Tests.** Vitest: the pipe returns the Bengali value, falls back to English on a missing key, and falls back to the key when both are missing; the toggle persists across a service re-instantiation; **every key present in `en.ts` resolves through the pipe** (guards against key drift).
  - **Plan, 2026-10-14.** Sessions N2 (web) and N3 (mobile, same keys in `app_localizations.dart`). Decided 2026-10-14: Bengali strings are drafted and listed here as unreviewed; the item stays PARTIAL until a Bengali speaker signs them off. Admin stays English on both clients. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.

37.9 [TODO] **Priority: P4.** Specified in
`docs/specs/003-alumni-programs-and-verification/spec.md` Story 5.
**Geographic chapters.** `Chapter` (`Id`, `Name`, `Region`, `Country`, `City`, `Description`, `CoordinatorMemberId?`, `ContactEmail?`, `IsActive`, `CreatedAt`) and `ChapterMembership` (`Id`, `ChapterId`, `MemberId`, `JoinedAt`, `IsCoordinator`) with a unique index on `(ChapterId, MemberId)`.
  - **Reuse, do not rebuild.** A chapter event is an `AlumniEvent` with a `ChapterId` — add the nullable column to `AlumniEvent` rather than creating a `ChapterEvent` table. Chapter announcements reuse `INotificationService.CreateNotificationAsync` fanned over the chapter's members; there is no new messaging surface in this item.
  - **Service/API/UI.** `IChapterService` + `ChapterService`; `ChaptersController` at `api/chapters`, `GET api/chapters/public` `[AllowAnonymous]`. Flag `enableChapters`. Public `/chapters` (list + detail with coordinator contact), member `portal/chapters` (join/leave, my chapter feed), admin `admin/chapters`. New `API_ENDPOINTS.CHAPTERS` block.
  - **Tests.** NUnit: joining twice does not duplicate; a chapter event appears only in that chapter's feed; coordinator contact is hidden from the anonymous projection unless `ContactEmail` is set. Vitest: join/leave state, empty-chapter state.
  - **Plan, 2026-10-14.** Session N7; mobile gets the list, join and leave, and the chapter feed. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.

37.10 [TODO] **Priority: P3.** Specified in
`docs/specs/003-alumni-programs-and-verification/spec.md` Story 2.
**Annual impact report generated from the ledger** — the accountability artifact that closes the loop on 37.2, 37.3 and 37.4, and the reason those three back-link `FinancialRecordId`.
  - **Nothing in this item is hand-typed.** For a given year it aggregates: total income and expense by `FinancialCategory` from `FinancialRecord`; scholarships awarded and disbursed from `ScholarshipAward`; campaign totals and donor counts from `CampaignPledge`; events and attendance from `AlumniEvent` + `EventRegistration`; new members from `Member.CreatedAt` / `MembershipHistory`; reunions from 37.4. The only authored fields are a president's foreword and a cover image, stored in the existing `SiteContent` CMS from Work Package 34 — **do not add a table for two strings.**
  - **Output.** Server-side PDF via **QuestPDF**, following the `IDCardService.GenerateIDCardPdfAsync` pattern, plus an on-site HTML view reusing the Work Package 36 `.doc-hero` / `.doc-prose` shell. Extend `IFinancialLedgerService` with `Task<ImpactReportDto?> GetImpactReportAsync(int year, CancellationToken cancellationToken = default)`, and put the PDF method on the existing document-generation surface rather than inventing a third document service.
  - **Guard.** The report must degrade rather than throw when a source feature is not yet built or its flag is off — a year with no campaigns renders without that section. This item is therefore safe to build *before* 37.2/37.3/37.4 land, and must read every source through a null-tolerant projection.
  - **API/UI.** `GET api/financials/impact/{year}` and `GET api/financials/impact/{year}/pdf`, both `[AllowAnonymous]` (publishing it is the point). Flag `enableImpactReport`. Public `/impact/:year` with a year selector; admin action to set the foreword and publish. Additions to the existing `API_ENDPOINTS.FINANCIALS` block.
  - **Tests.** NUnit: a year with zero records returns a report with zeroed sections rather than null; category totals match a hand-summed fixture; the disbursed-scholarship total equals the sum of the linked `Grant` `FinancialRecord` rows. Vitest: year selector, empty-section rendering.
  - **Plan, 2026-10-14.** Session N6, after 37.4; mobile gets the report view and the PDF link. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.

37.11 [TODO] **Priority: P2.** Per 12.6, nothing in Work Package 37 is `[DONE]` until `dotnet test`, `npx vitest run`, `npm run type-check` and `npx ng build` all pass. Baselines to beat at the start of this Area: **351 NUnit tests** and **64 vitest files / 306 tests**. Additionally, every item that adds a table must be verified against the 37.0 migration path on a **non-empty** database — a passing suite against a fresh SQLite file proves nothing about preprod (`gotcha_ensurecreated_no_op_existing_db`). Update `docs/FEATURES.md`, `docs/PROJECT_MAP.md`, `docs/SRS.md` and `docs/ARCHITECTURE.md` as each item lands, per `feedback_docs_update_scope`. Reviewed 2026-09-17: stays open. This is a standing gate on the rest of the Area, not a one-time task — it can't be marked done while 37.2-37.10 are still unbuilt.
  - **Plan, 2026-10-14.** Session N8 closes it; baselines are refreshed from a run on the day. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.

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

60.3 [TODO] **Priority: P4 | Depends on: none.** Messages vs. Chat+Forum naming mismatch: web has a
single "Messages" page (`member/messages`); mobile splits the same networking space into a separate
Chats screen and a Forum feature (`chats_screen.dart`/`chat_room_screen.dart` +
`forum_categories_screen.dart`/`forum_topics_screen.dart`/`forum_topic_detail_screen.dart`). Not
confirmed whether these map to the same backend feature/data model or web's Messages covers ground
mobile's split model doesn't (e.g. direct 1:1 alumni messaging vs. threaded forum discussion) —
needs a closer read of both the web and mobile networking/messaging services before concluding
anything is actually missing; flagged as needs-verification, not a confirmed gap.

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
> 3. **Refactor in passing, do not open a parallel refactor (61.2).** Remove what 61.1 surfaces in
>    the same phase that stranded it. Do not start a general refactor that is unrelated to
>    genericization; that is still out of scope and still bounded by the "no new abstraction without a
>    concrete duplication problem" rule.
>
> Net effect: 61.1 and 61.2 stop being standalone blind sweeps and become per-phase obligations,
> tracked in 62.46-62.49 below. 61.3 is absorbed by rule 1 the next time that file is touched.

### PHASE A: PROFILE PACK FOUNDATION (blocking, no user-visible change)

62.2 [PARTIAL 2026-09-16] **Priority: P2 | Depends on: none.** `org-config.json` and `seo.json` and `site-content.json` (hyphen-named)
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
  - **Plan, 2026-10-14 (round 2).** Session N9 writes the remaining profile files. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.
  - **Review, 2026-10-14.** Priority added (P2), session N9. `constitution.json` exists in `profiles/default/` only; N9 writes the GHC copy.

### PHASE B: API DE-BRANDING

### PHASE C: WEB DE-BRANDING

(62.22 done 2026-09-16 — see docs/TODO_ARCHIVE.md)

### PHASE D: MOBILE DE-BRANDING

### PHASE E: DATA, TIERS, GOVERNANCE

62.56 [TODO] **Priority: P3 | Depends on: 62.2.** Export and import of the organisation profile data, in the same shape as the seed files. Raised by the owner on 2026-10-15. Today the profile lives in `profiles/<name>/` (`org-config.json`, `seo.json`, `site-content.json`, `constitution.json`, `assets/`, `demo-data/`) and in `GHCAA.Infrastructure/Data/Seed/*.json`, and the only way to move it between installs is to copy files and reseed. Seed JSON does not touch an existing database, so a running site cannot take a new pack at all. This item adds a SuperAdmin page that exports the live data as a pack and imports a pack back. The requirements, section map, plan and task list are in spec 025 (`docs/specs/025-org-config-pack-export-import/`, the profile and settings export and import spec), tasks T001 to T036. Sub-items:
- **62.56a** Section map. One list in the backend that names each section, the pack file it reads and writes, the table behind it, and the natural key used to match rows (lookups by group and value, site content by key, and so on). Export and import both work from this list, so a new section is one entry. The owner added on 2026-10-15 that the pack must carry the organisational settings and the other configurations, not only the profile content. So the list covers every admin-editable settings store: Org Config (branding, workflow, feature flags, email provider choice), SEO, site content, constitution, email templates, themes, lookups, fee policy and fee configs, payment settings, social sign-in, election personas, gamification points, and branding assets. A read of the code on 2026-10-15 found these stores and their keys, listed in the spec's section map. SEO has no table: `seo.json` is read only at web build time, so it is export-only. Constitution versions come in inactive and never overwrite an existing version. The payment gateway secret and the social sign-in client secret go out blanked, and on import a blank secret keeps the value already stored. The importer never writes an `Id`, so the PostgreSQL identity sequences stay ahead of the data. Saved payment methods, users, roles and member records stay out. They hold personal data, and the never-edit rule for secrets and connection strings applies to the importer too, so nothing from `appsettings*.json` or environment variables is exported or imported.
- **62.56b** Export. The admin ticks the sections to include and downloads one `.zip` holding the JSON files plus a `manifest.json` (pack version, app version, export date, the sections and their row counts). Assets go in `assets/` inside the zip.
- **62.56c** Import upload. Accepts a single `.json` file for one section, a folder of JSON files picked in the browser, or a `.zip` laid out like `profiles/<name>/`. A pack without a manifest is matched by file name, so an existing profile folder or the seed folder imports as it is. The server rejects entries with `..` or absolute paths, caps the unpacked size and file count, and checks each JSON file against its section before anything is written.
- **62.56d** Mapping and preview. After upload the admin sees each file matched to a section, can untick a section, or point a file at a section by hand when its name does not match. Each section then shows a dry-run count: new rows, rows that match an existing key, and rows that fail validation, with the failures listed. A key found twice in the pack is reported as a failure. In the live table each key is unique once 62.56g lands, so it needs 62.56g first.
- **62.56e** Mode per section. "Add only" inserts new keys and leaves matching rows alone. "Override" also updates matching rows from the pack. Neither mode deletes rows that are missing from the pack. The whole import runs in one transaction, writes an audit log entry naming the sections and counts, and clears the OrgConfig cache so the new values show without a restart. The org config is merged onto the live one, so an older pack does not reset newer settings, and it passes the same checks as the org config editor, frozen election rules included. Election persona override is refused while an election is running.
- **62.56f** Tests and docs. Service tests for each mode, the zip path and size checks, the key matching, the rollback on a bad file and an export-then-import round trip that changes nothing. A web spec for the mapping step. FEATURES.md, PROJECT_MAP.md, a new `profiles/README.md` with the pack format, and a line in `docs/INSTITUTION_ONBOARDING.md`. Mobile needs no change: it has no admin settings screens and the org config shape stays the same.

RAR settled on 2026-10-15: zip only. The owner first asked for `.rar` on both sides, but no library can write RAR without the licensed WinRAR tool, and reading it needs a new package (SharpCompress). The owner chose zip on both sides, and the import page says so. Three smaller choices are proposed in spec 025's Decisions table and can be confirmed before the phase that needs them: upload the file twice (preview, then commit) with a hash check instead of storing it on Render's wiped disk, keep `seo.json` export-only, and default every section to "add only" except the org config.

### PHASE F: ONBOARDING, OPS, PROOF

62.6b [TODO] **Priority: P2 | Depends on: 62.6, and on `ORG_PROFILE=ghc` being set on the
deployments.** Delete `OrgConfigService.BuildGhcaaDefaults()` (~190 lines), the optional `profiles`
constructor parameter, and the `ProfileExplicitlySelected` branch in `BuildDefaults()`, leaving the
service reading the pack unconditionally. **Why it is a separate item rather than part of 62.6:** the
hardcoded copy is the fallback that stops an unconfigured deployment serving sample branding to a
live association, so it cannot be removed while any deployment still lacks `ORG_PROFILE`. Removing it
first would reintroduce exactly the failure 62.6's guard exists to prevent.
  - **Plan review, 2026-10-14.** Session N9, after a person confirms `ORG_PROFILE=ghc` on every deployment. Part of the 2026-10-14 decision that GHC is hardcoded nowhere. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.

**Acceptance:** `ORG_PROFILE` is confirmed set on every running deployment (preprod and any other);
`BuildGhcaaDefaults()` and the guard are gone; `ProfileDrivenConfigTests` is updated so the cases that
currently assert the unset-profile fallback instead assert that an unset profile now fails loudly;
`OrgConfigGoldenSnapshotTests` still passes unchanged, which is what proves the deletion changed
nothing.
Audit 2026-10-09: moved back from `docs/TODO_ARCHIVE.md`, where it was the only open copy.
`OrgConfigService.cs:22` and `:185-193` still hold `BuildGhcaaDefaults()` and the guard. Only
`.env.preprod` shows `ORG_PROFILE=ghc`; the Render settings cannot be checked from the repo.

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
**Still blocked on reason 1 alone.** The 62.31 hold was lifted on 2026-10-14 with no git-history
rewrite. The eight committed migrations stay as they are; session N11 adds a startup step that
removes the rows they bake in, only when the profile is not `ghc` and the database holds nothing
else. This item goes green when that step lets the test pass on an empty database.
This is the item that proves the area is done, so it stays open until it's actually green, not just
written.
  - **Plan, 2026-10-14 (round 2).** Session N9. Committed migrations are not edited; a startup step removes the rows they bake in, only when the profile is not `ghc` and the database holds nothing else. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.
  - **Plan review, 2026-10-14.** Moved from N9 to session N11, which depends on N9. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.

62.42 [TODO] **Priority: P2 | Depends on: 62.41.** Flip `brand-lint` from warn-only to blocking in
CI. It waits for 62.41 to go green. brand-lint runs in no CI workflow today (not in
`.github/workflows/` and not in `scripts/ci-local.py`), and `scripts/brand-lint.mjs` always exits 0,
so N11 adds the step to CI and then changes the exit code.
  - **Plan, 2026-10-14 (round 2).** Decided 2026-10-14: the code name `GHCAA` stays, and `brand-lint.config.json` stops counting it in namespaces and project names. Session N9 makes the check blocking after 62.41 is green. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.
  - **Plan review, 2026-10-14.** Moved from N9 to session N11, after 62.41. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.

### PHASE G: WORK PACKAGE 61 CARRY-OVER (runs inside phases A-F, not after them)

> These four items are the mechanism that folds Work Package 61 into Work Package 62. They are not a separate pass at
> the end. Each one is checked off per phase, and the phase is not done until its slice is done.

63.8 [TODO] **Priority: P3 | Depends on: nothing.** Re-take the repository figures listed in
`docs/book/README.md` under "Keeping the numbers true" immediately before any submission, and correct
the sentences that carry them. They were re-taken on 2026-10-14 (`docs/book/README.md`) and go stale with every feature.
  - **Review, 2026-10-14.** Stays open for the re-take just before submission.

63.34 [TODO] **Priority: P4 | Depends on: none.** Chapter page budgets in the outline were set before Chapters 7 to 13 were written. The strict build of 2026-10-14 printed 93 of 99 pages, with five chapters over budget: Chapter 1 by 1 page, Chapter 3 by 3, Chapter 6 by 3, Chapter 8 by 1 and Chapter 11 by 1. The author decided on 2026-10-14 to keep the book at its current length for now and review the budgets later. Found 2026-10-14.
**Acceptance:** each chapter's page count from `build.py --pdf` sits inside its stated budget, or the budget is changed with a reason.

63.18 [TODO] **Priority: P3 | Depends on: 63.14.** Front matter is numbered in Arabic with the body,
not lower-case Roman as `DOCUMENTATION_BOOK_OUTLINE.md` specifies, and the title page carries a folio.
Chrome's footer template is one template for every page, so neither can be varied. Closing this needs
two prints (front matter and body, each with its own template) merged into one file, which needs a PDF
library beyond the standard library. Worth doing only if a supervisor asks for it.

64.8 [PARTIAL 2026-10-09] **Priority: P2 | Depends on: 64.7.** Revise §4.7 to carry risk exposure **RE = P × C** and
impact on the 1–5 scale, which is the convention the course material uses. The probabilities are
already there; the impact costs are author-stated and blocked on 64.7.
Audit 2026-10-09: RE = P × C is now used in §11.9, the project risk register. §4.7 and Table 4.2 still lack the
1–5 impact scale.

64.9 [TODO] **Priority: P2.** Chapter 11 figure set is seventeen per-component activity diagrams plus
six chapter-level charts, not one module network. A seventeen-node network with nine edges converging
on the web client prints at about 4pt, well under the enforced 7pt floor. Per-component diagrams also
sit beside the prose for their own component, which is where a reader wants them.

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
2026, `build.py --strict` reported 105 open placeholders across them (`07-implementation.md` 17,
`09-verification.md` 44, `10-deployment.md` 16, `12-results.md` 16, `13-conclusion.md` 12).
Recounted 2026-10-09 by counting `*[` in each chapter file: `07-implementation.md` 6,
`09-verification.md` 43, `10-deployment.md` 16, `11-project-management.md` 1, `12-results.md` 16,
`13-conclusion.md` 11, 93 in all. Recounted the same way on 2026-10-30: still 93, the same per
chapter, as the book has been frozen since. Chapter
11 went first because `wbs.py` prints its activity table, critical path and arrival profile from git
and this file, so writing it was prose around generated numbers rather than gathering anything;
Chapter 8 went next as the WP82.6 refactor left it well scoped; the remaining four still need that
gathering done chapter by chapter.
  - **Review, 2026-10-14.** Chapter 11 still holds one placeholder: the hours-per-function-point figure at `docs/book/11-project-management.md:269`. Counts on 2026-10-14: chapter 7 has 6, 9 has 43, 10 has 16, 11 has 1, 12 has 16, 13 has 11, 93 in all.

67.3 [TODO] **Priority: P2 | Depends on: 67.2.** Seventeen per-component activity diagrams (64.9)
plus the six chapter-level charts, for chapters 7 to 13. Author decided on 2026-09-27 to keep all
seventeen, drawn left-to-right with single-line box labels so each one costs as little height as
possible (see 67.5). Confirmed still at zero: none of the seven
new chapter files contains a single ```mermaid``` block (all 63 existing diagrams belong to chapters 1
to 6 and 8, recounted 2026-10-14). They cannot be drawn before the surrounding prose exists to hold them, and under Work Package 66
every one of them has to survive the overprint and clipping checks, which the two quadrant charts and
the DFD did not on first attempt.
  - **Review, 2026-10-14.** Mermaid count on 2026-10-14: chapter 1 has 4, 2 has 4, 3 has 12, 4 has 5, 5 has 19, 6 has 13, 8 has 6; chapters 7 and 9 to 13 have none.

67.4 [TODO] **Priority: P2.** The reference list still carries seventeen entries no written chapter
cites (confirmed unchanged 2026-09-05: `build.py --strict` reports the same seventeen numbers). They
are not stale: each was collected for a chapter in Part III or IV. The build reports them as expected
while those parts are unwritten, and `--final` is the gate that stops accepting that excuse. Do not
delete a reference to quieten the report; write the chapter that uses it, or remove it deliberately
with the reason recorded here.
  - **Review, 2026-10-14.** Confirmed 2026-10-14: the book lint still lists seventeen uncited references, numbers 19, 20, 22, 31, 32, 38, 39, 40, 42, 43, 44, 59, 60, 61, 62, 63 and 68.

67.5 [IN PROGRESS] **Priority: P1 | Depends on: none; 67.2 and 67.3 must be written to it.** The book
has to print in under 100 pages from Chapter 1 to the References (decided 2026-09-27, replacing the
150 to 200 page volume). Per-chapter budget is in the outline's "Page budget" block. Measured 102
counted pages on 2026-09-27, with four chapters still stubs. Done so far: Part divider pages dropped,
diagram enlargement capped at natural size, table rows compacted, tables over six rows allowed to
split with the header repeated (`build.py`, `ieee-print.css`). Still open: about 14 pages of prose
tightening, section merges under one heading naming both, and any cuts, each cut agreed by name with
the author first. The whole pass also has to read as human-written and clear `lint.py` and `prose.py`.
Decisions taken with the author on 2026-09-27: all 24 merges approved (M1 to M24, listed in 67.7).
Cuts approved: 9.16 folds into 12.11, Threats to Validity; 13.3 becomes a one-line pointer to
§12.8, where the research questions are answered. Kept: the chapter Summary sections and 1.11,
Structure of the Dissertation. No figure is cut. Instead, tall figures are redrawn to run
left-to-right with each box label on one line. The first four named are Fig. 1.2 (stakeholder
onion), Fig. 1.4 (research question, objective and chapter map), Fig. 2.1 (study selection flow)
and Fig. 2.2 (concept map of the reviewed literature). Every other diagram of the same tall shape
gets the same treatment. Figs. 3.11, 3.12, 5.1 and 5.19 get the single-line redraw first, and the
author sees the result before deciding whether any of them goes. Each redraw still has to fit one A4
page with labels at 7pt or larger and no overlaps, and the build's diagram audit is the check.
  - **Review, 2026-10-14.** The archived 67.5 of 2026-09-02 was renumbered to 67.8 the same day, so this item is the only 67.5.

67.7 [PARTIAL 2026-09-27] **Priority: P1 | Depends on: none; merge list confirmed 2026-09-27.** The 24 confirmed
merges: 3.4+3.5, 3.6+3.7, 3.8+3.9, 3.11+3.12, 4.5+4.6, 5.3+5.4, 5.7+5.8, 6.4+6.6, 6.8+6.9,
6.11+6.12, 6.13+6.14, 7.7+7.8+7.9, 8.5+8.6, 8.7+8.8, 8.13+8.14, 9.4+9.5, 9.7+9.8, 9.14+9.15,
10.1+10.2+10.3, 10.5+10.6, 10.8+10.9, 10.10+10.11, 12.4+12.5+12.6, 13.1+13.2. Numbers are as of that
date; apply them from the end of each chapter backwards so earlier numbers stay valid. Each one changes the heading, renumbers the later sections in the chapter and the
outline, and rewrites every "§n.m" reference across the book. Add
`docs/book/build/merge_sections.py`: dry run by default, `--apply` to write, one merge per call
("3.11+3.12 'Feasibility Analysis and Requirements Validation'"). It moves no body text; joining the
prose stays a manual edit. **Acceptance:** tests for a merge in the middle and at the end of a
chapter, and for a cross-reference in another chapter; `build.py --strict` clean after a real merge.
**Progress 2026-09-27:** the script is in, with 9 tests (the ninth checks that a CRLF file stays
CRLF and an LF file stays LF). All 24 merges are applied to the chapters, the outline and wbs.py.
6.4+6.6 was done by swapping 6.5 and 6.6 first, so Interface Design now sits above Data Design and
the merged section is 6.4, Component and Interface Design. renumber.py rebuilt the contents, and
`build.py --strict` reports no outline drift and no lint finding. Still open: the prose join at each
dropped heading, which belongs to the 67.5 pass, and a `--pdf` build to refill the contents folios.

72.4 [TODO] **Priority: P3.** Watch the first Dependabot run. Five ecosystems opening at once
produces a burst even with the caps, and the grouping rules are a guess until they have been seen
against a real week's updates. Adjust the caps or the groups if the burst is unmanageable rather than
turning Dependabot off.

72.5 [TODO] **Priority: P3 | Depends on: user.** If the repository is ever made public, CodeQL becomes
free and 72.2 is worth revisiting; §10.4.2 would then need rewriting, since it currently states cost
as the reason static analysis is absent.
  - **Review, 2026-10-14.** The §10.4.2 section this item points at was not found on 2026-10-14. Check the reference when the item is picked up.

# Work Package 73 — Traceability and constitutional alignment: what is true, and what was claimed

<!-- wbs: component=C17 start=2026-09-03 end=2026-09-03 after=71 -->

Raised by user 2026-09-03 with six statements about requirements traceability, domain constraints,
verification of constraints, dependency mapping, an audit trail and non-functional constraints, and
an instruction to sync the docs. Each was checked against the tree before anything was written.
Three are already true and documented; three describe a system stronger than the one that exists, and
writing them up as fact would have put an unverifiable claim into a document whose whole argument is
that its claims are checkable.

73.5 [PARTIAL 2026-10-09] **Priority: P2 | Depends on: 73.4.** Generate the traceability matrix as a repository
artefact from those tags, the way `wbs.py` generates the Chapter 11 tables. Then §3.9's original
claim becomes true and can be restored, and §12.2 can close the matrix from evidence rather than by
reading.
Audit 2026-10-09: `traceability.py` exists but regenerates only the Test column. The rest of the matrix is still
written by hand.

73.6 [TODO] **Priority: P2.** "Constitutional rules fail the build if they drift from the governing
text" is the user's description and is not what happens today. `ConstitutionSeeder` synchronises the
stored constitution with the source document at boot, and the business rules have tests, but nothing
compares the two: the seeded text could diverge from the ratified PDF, or a rule could be changed
away from its clause, and the suite would stay green. A narrow, honest version is cheap — a test that
pins the seeded constitution's version and a hash of its clause text, so that changing the governing
data without changing the test is a failure. That is worth building, and it would let Chapter 8 and
Chapter 9 make a claim no commercial CRM comparison in Chapter 2 can match. Not built unasked,
because it is application code rather than documentation.

74.3 [TODO] **Priority: P3.** Backfill markers for Work Packages 1 to 67. Not done in bulk: a marker asserting
a start date is a claim about when work happened, and for most of those areas the honest source is the
commit record `wbs.py` already reads. Worth doing only where an area's real dates differ from its
commit dates, which is the case for the research and review work of U1 to U5.

75.5 [TODO] **Priority: P2 | Depends on: user.** The five PRE durations are the author's, not the
script's. Ten days for the governing-document study and eight for requirements analysis are the two
worth challenging: they set the pre-development total and therefore the person-month figure the
dissertation prints. Confirm or correct them the way 64.7 asks for the assumption rates.
  - **Owner answer, 2026-10-14.** The 27 pre-commit days stand as written, ending on 9 February 2026. The five durations are confirmed; the item stays open only for the P1 day count against the corrected word count.

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
  - **Owner answer, 2026-10-14.** Banded answer: about a quarter of the delivered code has an ancestor in the 2019 to 2024 library. The five measured modules give 18.5%; the owner names three more that are not yet measured, which 77.11 adds.

77.11 [TODO] **Priority: P2 | Depends on: 77.4.** The owner named three more parts carried over from the
2019 to 2024 library that `wbs.py` does not yet count as prior reuse: backend base services (generic
repository, base service and DTO patterns), deploy and CI configuration (Dockerfile, `render.yaml`, the
GitHub workflows and scripts), and test scaffolding (fixtures, factories and helpers). Add each to the
reuse list in `docs/book/build/wbs.py` with the exact paths, after the owner confirms which files descend
from the library, then refresh §11.0, §11.5.2, §11.5.3 and the outline's Chapter 11 block from the new
run. The owner puts the total at about a quarter, against the 18.5% measured now.
**Acceptance:** the reuse share comes from a `wbs.py` run against the tree, dated, and lands near a quarter;
`build.py --pdf --strict` ends clean.

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

---

(82.115 done 2026-09-18 — see docs/TODO_ARCHIVE.md)

---

# Work Package 84 — Workflow contracts, validation, and endpoint detail from the specification review

<!-- wbs: component=C17 start=2026-09-21 end=2026-09-21 after=73 -->

Raised by an external review of `docs/specs/001-platform-baseline`'s evidence files. The review's
specific claims trace to gaps the baseline already records: no centralized state-transition table for
events, news/article approval, jobs, mentorship, payments, or polls; no single validation matrix
across write DTOs; and no exhaustive endpoint contract table for the API surface. The scope is drafted
in `docs/specs/002-workflow-contracts-and-validation/`, per baseline task T018, which keeps new
specification work in its own numbered directory rather than editing 001.

84.4 [TODO] **Priority: P2 | Depends on: 84.3.** Build the Web/Mobile client parity table by
cross-referencing each endpoint from 84.3 against actual client call sites, per spec.md Story 4.
**Acceptance:** every endpoint row states whether Web calls it, whether Mobile calls it, and any
observed difference in how each client reads the response.

84.7 [TODO] **Priority: P3 | Depends on: none — product decision, not a documentation task.** Decide
whether removing the committed-migration-data blocker for white-label second-institution deployment is
in scope, or whether white-label stays a single-institution profile-pack mechanism
(`implementation-inventory.md` lines 135–136). No item in this Work Package should assume an answer
until the project owner decides.

84.9 [TODO] **Priority: P2 | Depends on: 84.8.** Turn `docs/specs/001-platform-baseline/spec.md` into
the index for the domain specs. Keep its cross-layer requirements, key entities and outcomes. Replace
its ten stories with a table naming the domain spec that now owns each story, list specs 002 to 011,
and add a roll-up of every domain spec's Enhancements items. **Acceptance:** each of the ten old
stories links to one domain spec, and the roll-up count matches the Enhancements items across 012
to 020.

84.18 [TODO] **Priority: P2 | Depends on: 84.8.** Review each entity feature against what a
standard association platform offers, using the domain specs 012 to 020 as the baseline. Each
missing feature is listed with its domain, the users it serves and whether it fits an alumni
association. **Acceptance:** a gap table in the 001 index, with each accepted gap raised as its own
item.

84.19 [TODO] **Priority: P2 | Depends on: 84.8, 84.18.** Meetings module, built as its own entity
module. It covers a meeting, its invitees with RSVP, an ordered agenda, minutes, and action items.
It is justified because EC governance runs on meetings and `governance.html` line 135 already tells
members that minutes are archived, but no meeting or minutes entity exists in any layer. Admin and
EC roles manage meetings from web and mobile. It reuses the invite and RSVP pattern from events
where spec 014 (events, news, gallery and content) shows one. **Acceptance:** an approved spec under
docs/specs before any code.

84.20 [TODO] **Priority: P3 | Depends on: 84.19.** Tasks and notes. A task with an owner, due date
and status is justified only as a meeting action item or an EC work item, so it lands inside or
next to the meetings module rather than as a general task manager. Standalone notes add little
beside meeting minutes. They stay out unless the 84.18 review finds a real user for them.
**Acceptance:** the decision on a separate module or part of 84.19 is recorded in the 84.19 spec.

84.21 [TODO] **Priority: P3 | Depends on: none.** `FamilyService.cs` holds a second family-link
workflow (send, respond, cancel, list, unlink) beside `FamilyLinkService`. `FamilyLinkController`
calls only `FamilyService.SearchByNameAsync`, and no other caller was found. **Acceptance:** search
moves to `FamilyLinkService` and the unused methods go, or a caller is named. Spec 013 (profile,
family and identity files) ENH-001 has the detail.

84.23 [TODO] **Priority: P3 | Depends on: none.** Two client gaps from spec 013. Mobile has no
signature upload, although `POST api/profile/signature` exists. The backend PDF routes
`id-card/pdf` and `certificate/pdf` have no caller in either client, since mobile builds its PDF
itself in `digital_id_screen.dart`. **Acceptance:** mobile gets signature upload or the gap is
accepted, and the PDF routes are either used or removed.

84.25 [TODO] **Priority: P3 | Depends on: none.** Networking and careers test and validation gaps.
`MentorshipController` has no controller tests, so its 401, 400 and 409 branches and the AdminOnly
list are untested. `CreateJobDto` has no FluentValidation validator. **Acceptance:** both exist.

84.27 [TODO] **Priority: P2 | Depends on: none.** `ArchiveController` has no test file and no web
admin screen, so archive moderation is reachable only from the API and mobile. **Acceptance:**
controller tests exist and web admins can moderate archive items, or spec 014 records that mobile
owns it.

84.31 [TODO] **Priority: P3 | Depends on: none.** Two spec folders share number 010:
`010-ad-hoc-reporting` and `010-election-engine-fixes`. **Acceptance:** one is renumbered and every
link to it is updated, or the clash is accepted and noted in the 001 index.
  - **Plan, 2026-10-14 (round 2).** Session F7: `010-election-engine-fixes` moves to the next free spec number (check for a reserved "spec NNN" first) and every reference follows. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.

84.35 [TODO] **Priority: P2 | Depends on: none.** The strict book build fails on 7 repository
counts (run 2026-09-25). Chapters 4, 7 and 11 quote 299 commits and 88 work packages, and the tree
gives 304 and 90, after Work Packages 89 and 90 were added. On 2026-09-26 Work Package 91
moved the tree to 306 commits and 91 work packages, and `wbs.py` now reports 61 of 91 work packages
and 733 of 963 tasks as reactive. On 2026-09-27 the tree gave 308 commits, and Work Package 92
makes it 92 work packages. **Acceptance:** every figure `wbs.py`
quotes is re-sourced with its date, not only the two that failed, and
`build.py --pdf --strict` reports no count drift.

84.39 [TODO] **Priority: P2 | Depends on: 84.37, 84.38.** The proposal
(`docs/proposal-docs/FINAL_PROPOSAL_AND_PRESENTATION.md`) needs the new title and a requirements
engineering and SDD paragraph. The text is drafted and waiting for the author to review it. The file
is left unedited until they approve. **Acceptance:** the approved text is in the proposal's title,
its Development approach section and Slide 7.

84.42 [PARTIAL 2026-10-09] **Priority: P1 | Depends on: none.** Found by 84.5: the mobile admin screens call
routes marked `[RequireStepUp]`: `POST /roles/users` and `POST /roles/assign`
(`features/admin/roles_service.dart:27` and `:62`) and `POST /admin/elections`
(`features/elections/election_service.dart:185`). The web client handles the 403 in
`global-http.interceptor.ts:70`. **Acceptance:** a device check that these three actions succeed through
the mobile prompt. A test covers the interceptor.
Built since: 37.1w added the mobile prompt, so the first version of this item (no mobile step-up flow) no
longer holds.
Audit 2026-10-09: the first version of this item predates 37.1w. `GHCAA.Mobile/lib/core/api/step_up_interceptor.dart` now reads
`STEP_UP_REQUIRED` and is wired in `api_client.dart`, so the actions should no longer always fail.
What is left is a device check that `POST /roles/users`, `POST /roles/assign` and
`POST /admin/elections` succeed through the prompt.
  - **Plan, 2026-10-14 (round 2).** Decided 2026-10-14: mobile keeps create election. Session F6 routes it through the step-up prompt with a 403 test; the device check stays with a person. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.
  - **Plan review, 2026-10-14.** Moved from F6 to session F1: the election call needs nothing from F2 to F4. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.

84.46 [TODO 2026-10-23] **Priority: P3 | Depends on: owner.** `GHCAA.Tests/Services/BallotSealVector.cs` holds the throwaway RSA key for the 37.13e test vector (ballot seal checked against an outside sealer). It already says it seals nothing real. The owner asked for a clearer test-only line and a secret-scanner allow-list entry (`.github/secret_scanning.yml`, path ignore). The repo has no scanner config yet. The session's tool policy refused both edits because the file holds key material, so the owner makes them by hand. Owner decision (2026-10-23): the owner makes both edits from text given in the session. CI runs no secret scanner, so the file only matters if GitHub secret scanning is on for the repo.

84.48 [PARTIAL 2026-10-23] **Priority: P1 | Depends on: owner keys.** Preprod env check against `docs/ENV_REVIEW.md` and `docs/DEPLOYMENT_CHECKLIST.md`. Render has the environment, JWT key, database URL, allowed origin, client URLs and org profile. It lacks `EmailSettings__Provider=Brevo` with `BrevoSettings__ApiKey` and `BrevoSettings__SenderEmail`, so mail fails because Render blocks SMTP. It lacks `FileStorage__Provider=S3`, the bucket name and the four `AWS_*` values, so uploads are lost on each redeploy. Optional: `SmsSettings__Token`, the live bKash password and `OtpSettings__HashKey`. Render sets `ASPNETCORE_ENVIRONMENT=Preprod` on what the owner calls production. The gitignored `GHCAA.API/.env.preprod` now carries every `appsettings.Preprod.json` key; Gmail comes from the last real account in `appsettings.json` history (commit 962f9000). `ConnectionStrings__PgSqlConnection` and the localhost-only second origin are commented out, since `DATABASE_URL` wins. Brevo, bucket, `AWS_*` and SMS stay `REPLACE_ME`, because no copy exists in the repo or its history. No appsettings file was changed. Later on 2026-10-23 the owner filled Brevo and the four `AWS_*` values locally; the endpoint is Neon's S3-compatible storage. Owner decisions (2026-10-23): the owner creates a bucket in Neon and supplies the name; SMS stays off for now, since email and the authenticator app cover OTP; the owner pastes the values into Render from a key-name checklist, then the live site is checked (OTP email arrives, an upload survives a redeploy); `ASPNETCORE_ENVIRONMENT` stays `Preprod` until a separate production service exists.

84.49 [TODO 2026-10-23] **Priority: P2 | Depends on: none.** Left by the 84.36 review (DataAnnotations rules on the member DTOs). The NID and age rules also run on update requests, so an imported member with the placeholder NID `IMPORT-<row>-<ticks>` (`MemberImportService.cs`) cannot save a profile edit until the NID is fixed. The PassingYear upper bound is the current year, which blocks current students. Dates use UTC, so near midnight in Bangladesh the age check is a day off. On mobile, the ballot key progress dialog can stay open if the screen closes while the key is made, and key making has no timeout. Fixed in the same review: a member who turns 13 today is accepted, an empty academic list gives its own message, mobile strips spaces from the NID, the phone picker's cached key file is deleted, and the unused FluentValidation package versions are gone.
  - **Owner decisions (2026-10-23; NID and lookup choices confirmed 2026-10-27).**
    - NID: members may edit their NID at any time. Validate the 10, 13, or 17 digit format and uniqueness on every submission, including first entry. An imported placeholder is not a separate permission rule.
    - PassingYear: use database rows when available and a fallback list in both Web and Mobile. Allow up to the current year + 6.
    - Dates: the age and year rules use the Dhaka date. Move `AssociationTime` from Infrastructure to Domain so the DTO rules can use it.
    - Mobile key spinner: close it through its own dialog context so it closes even after the screen is gone.
    - Key timeout: about 60 s on the server save call only, not on making the key on the device. A retry after a timeout must not leave two keys.
  - Plan (2026-10-27): `docs/AUTH_AND_DEVICE_FIX_PLAN.md`, Part B. Its B1-B3 work is in the non-election sequence; ballot-key B4-B5 stays with election work. The plan names the files, tests and checks for each fix.

84.50 [TODO 2026-10-23] **Priority: P1 | Depends on: a phone or the Windows desktop device.** Device checks the owner asked to have fixed, not just listed, in the next session. 95.6: scan the authenticator QR, pass a step-up with the app code, then switch to an emailed code. 84.42: the mobile admin step-up routes. 37.1m and 37.14o: the vote receipt and tracking-code list on a phone. 84.44: make the ballot key pair on mobile, save the private key, and run the first count with it. Where no phone is at hand, the Windows desktop build drives the same screens (see the 43.x live-check method). **Acceptance:** each check passes on a device, or its defect is fixed with a test that would fail if it came back.
  - Plan (2026-10-25): `docs/AUTH_AND_DEVICE_FIX_PLAN.md`, Part A, with the screens and `local-election.py` run order. The tracking-code list test gap was closed on 2026-10-26 by `FR-016: lists every published tracking code` in `election_results_screen_test.dart`; device checks remain open.

84.51 [TODO 2026-10-23] **Priority: P1 | Depends on: nothing.** Review of commit 3b436f6f (the 2026-10-23 work) found no critical defect but these gaps.
  - **Password setup route has no lockout.** `POST api/auth/authenticator/setup-with-password` checks only the password, under the shared Auth rate limit. Someone holding a no-email account's session can keep guessing the password, and a hit lets them enrol their own app. Wrong passwords count against the same custom account lockout as login, a success writes an activity row, and admins can unlock any account, including members and system administrators. Login and authenticator-app locks clear together.
  - **App-code lockout can be raced.** `AuthenticatorService` reads `LockedUntil` and writes the failure count in two steps, so parallel requests each get a guess before the lock lands. Make the check and the count one `ExecuteUpdate`. Owner decision (2026-10-23): fix now. The test needs a relational provider, since EF InMemory has no `ExecuteUpdate`.
  - **Smaller fixes.** `RequestStepUp` takes an `IAuthenticatorService` it never uses. A JSON `null` first entry in `AcademicHistory` gives a 500 in `MemberRegistrationDto.Validate`. `NewsController.UploadImage` charges the daily quota before the save, so a failed save still uses one. `BeginSetupAsync` turns every `DbUpdateException` into 409 "already set up". `ResetByAdminAsync` deletes the app and rotates the stamp in two saves, not one transaction. `PaymentConfigController` now catches every `ArgumentException`, not only the gateway-build one.
  - **Missing tests.** `RolesController.ResetAuthenticator`; `SetupAuthenticatorWithPassword` with a wrong password, an account that has an email, and success; the `FamilyLinkController.GetPublicFamily` admin branch; the 429 from `NewsController.UploadImage`; step-up verify rejecting a recovery-code shape with method email; the proposer-is-candidate rule, which now lives only in `ElectionService`.
  - **Acceptance:** each gap is fixed with a test tagged FR, or the owner decides to leave it and the reason is written here.
  - Owner decision (2026-10-27): `ResetByAdminAsync` must delete the authenticator and rotate the security stamp in one transaction.
  - Plan (2026-10-27): `docs/AUTH_AND_DEVICE_FIX_PLAN.md`, Part C for general account security and Part D for admin unlock. Implement this non-election work before Part A and election-specific Part B4-B5. Login lockout is custom code on `User`, not ASP.NET Identity; the test base is SQLite, which supports `ExecuteUpdate`, so the InMemory note above is out of date. Four of the "missing tests" already exist and the plan names them.


---

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

# Work Package 91 — Pluggable feature modules (raised by user 2026-09-26: "i wanted features modules to be as reusable package/modules with relevant UI, API, and App ... these may not be developed for now but may be later these will be needed")
<!-- wbs: component=C11 start=2026-09-26 end=2026-09-26 after=62 -->

Deferred on purpose. Each feature today is spread across Domain, Infrastructure, API, Web and
Mobile, so none of it can be installed in another app on its own. Spec
`docs/specs/021-pluggable-feature-modules/spec.md` records the module shape (a .NET library, an
Angular library and a Flutter package per feature), the host contracts a module may call, and the
candidate order. The book's §6.2, where the modular monolith was chosen, still holds: a module is a
package the monolith loads, not a separate service. Nothing past 91.2 starts until a real second app
asks for a module, because building for an imagined consumer is the speculative abstraction §6.11.12
trades away.

91.3 [ONHOLD 2026-09-26, deferred until the school or club portal project starts] **Priority: P3 | Depends on: 91.2, WP62.** Move the host contracts into a small shared
contracts package. Current user is an extension class in `GHCAA.API/Extensions/CurrentUserExtensions.cs`,
not an interface, so it needs one. The others (`IOrgConfigService`, `INotificationService`,
`IPaymentGatewayService`, `IFileStorageService`) already exist in `GHCAA.Application/Interfaces`.
**Acceptance:** not started.

91.4 [ONHOLD 2026-09-26, deferred until a second app needs a module] **Priority: P3 | Depends on: 91.3.** Extract the pilot module (polls, events or gallery,
whichever the portal needs first) into three packages, load it back into GHCAA, and delete the in-repo copy. Migrations must
not drop or recreate the existing tables, and must run on SQLite and PostgreSQL. The pilot's existing tests
move with it so coverage does not drop.
**Acceptance:** not started.

91.5 [ONHOLD 2026-09-26, deferred until a second app needs a module] **Priority: P3 | Depends on: 91.4.** Prove the pilot in an empty host app with stub
contracts (spec 021 SC-001), and add a module on/off flag to OrgConfig Features. A new Features flag
needs the DTO, `BuildGhcaaDefaults()` and the golden snapshot changed together.
**Acceptance:** not started.

91.6 [ONHOLD 2026-09-26, deferred until a second app needs a module] **Priority: P3 | Depends on: 91.5.** Extract the remaining candidates one at a time,
only when a consumer asks: gallery, events with RSVP, mentorship, campaigns and scholarships,
elections, and meetings once 84.19 is built.
**Acceptance:** not started.

# Work Package 92 — Configurable approval workflows (raised by user 2026-09-27: "if all available workflow approvals aren't configurable (enable/disable, with approval roles assignment) update relevant plan for that")
<!-- wbs: component=C12 start=2026-09-27 end=2026-09-27 after=84 -->

Audit on 2026-09-27 found no approval flow that can be switched off or given to a different role at
runtime. Every approve and reject endpoint carries `[Authorize(Policy = Constants.Policies.AdminOnly)]`,
which `ServiceExtensions.cs:119` resolves to SuperAdmin or Admin. `RolesController` can create a
custom role, but that role opens no approval endpoint, because no policy reads it. The OrgConfig
`Features` flags (`OrgConfigDto.cs:91-111`) turn a whole module on or off, not its approval step.
One setting already exists: `MemberApprovalMode` (`OrgConfigDto.cs:116`, Auto or ManualReview,
default ManualReview) shows in the web and mobile org-config screens, but no service reads it, so
changing it does nothing.

| Flow | Approve/reject | Today |
|---|---|---|
| Member registration | `AdminController.cs:95`, `:139` | always on, AdminOnly; `MemberApprovalMode` is saved but never read |
| Event registration | `EventsController.cs:226`; auto-approved after a gateway payment (`PaymentCallbackOrchestrator.cs:105`) | always on, AdminOnly |
| Gallery album and photo | `GalleryController.cs:240-264` | always on, AdminOnly |
| Job posting | `JobHubController.cs:102`, `:110` | always on, AdminOnly |
| News article | `NewsController.cs:137`, `:146` | always on, AdminOnly |
| Election nomination | `ElectionsController.cs:90` | always on, AdminOnly |
| Manual payment verification | `FinancialsController.cs:80-84` (`PATCH update-status/{id}`) | always on, AdminOnly |
| Member import batch | `MemberImportController.cs` | AdminOnly |

Out of scope: family-link requests (the recipient decides, not an approver role) and constitution
amendments (a vote, not an approval). Scholarship applications have no approval step in code yet;
when one is built it joins this mechanism rather than hardcoding
AdminOnly. The pending-approvals queue (`PendingApprovalsController.cs:49`) has to follow the same
settings. Related: WP40 added the gallery and job approvals; 84.1 holds the state-transition tables
these flows must keep matching; 91.5 is the module flag, a different switch.

92.1 [TODO] **Priority: P2 | Depends on: none.** Write spec 022 for configurable approvals. It has to
settle four things with the user before any code: where the settings live (an `Approvals` section in
OrgConfig, or its own table); what "off" means for each flow (create as approved, or skip the queue
but keep an audit row); whether SuperAdmin can always approve regardless of the list (the protected
SuperAdmin rule says yes); and whether member import stays SuperAdmin/Admin only. **Acceptance:** spec
exists with the flow table above and the four answers recorded under "Decisions taken".

92.2 [TODO] **Priority: P2 | Depends on: 92.1.** Backend. One settings record per flow: enabled, and
the roles that may approve, defaulting to today's behaviour (enabled, SuperAdmin and Admin) so
nothing changes on deploy. The member-registration entry takes over `MemberApprovalMode` rather than
adding a second switch: Auto maps to off, ManualReview to on, and the registration path reads it. Flow keys and defaults go in `Constants`, not as literals. Replace
`AdminOnly` on each approve and reject endpoint with a requirement that reads the flow's roles, kept
behind an interface so it can move with the modules in WP91. When a flow is off, the create path
sets the approved state and writes the audit entry. SuperAdmin-only endpoint to read and change the
settings, with each change audited. **Acceptance:** unit tests per flow for on, off, an allowed role, a
refused role and the default settings; the reflection-based authorization policy tests updated;
SQLite and PostgreSQL migrations both apply.

92.3 [TODO] **Priority: P2 | Depends on: 92.2.** Web admin. A SuperAdmin settings screen listing each
flow with an on/off switch and a role picker fed from the roles `RolesController` returns, built from
the existing admin form and toggle components. Approve and reject buttons, and the pending-approvals
queue, show only flows the current user may approve. **Acceptance:** component tests for the screen
and for the hidden buttons; a live check that a custom role given one flow sees only that flow's queue.

92.4 [TODO] **Priority: P3 | Depends on: 92.2.** Mobile. Check which approval actions the Flutter app
shows and gate them on the same settings, so a user never sees a button the API will refuse.
**Acceptance:** widget tests for a shown and a hidden action.

92.5 [TODO] **Priority: P3 | Depends on: 92.2.** Docs. Add the FR and NFR entries, update the spec 002
state-transition tables, `business_flow` notes, `PROJECT_MAP.md`, and the book's role–permission
matrix (§8.4, Authorisation Model) and Fig. 8.3. **Acceptance:** `build.py --pdf --strict` clean for the
book changes.

# Work Package 93 — Member portal menu on small screens (raised by user 2026-10-03: "review member menu navigation panel items for not showing all, from smaller device browser")

93.3 [TODO] **Priority: P2 | Depends on: none.** `GHCAA.Web/tests/e2e/portal-mobile-nav.spec.ts` covers 93.1 but
could not run locally: `registerNewMemberViaUi` gets "Registration failed" from the local API, and
`gallery.spec.ts` fails the same way, so the cause is in registration or the local DB, not the menu.
**Acceptance:** both specs pass against a fresh local stack.

93.4 [TODO] **Priority: P3 | Depends on: none.** Flutter drawer (`GHCAA.Mobile/lib/core/widgets/app_drawer.dart`) has no entry for
`/communications`, `/legacy` or `/professionals`, and nothing else in the app links to them. It also
shows Events, Job Hub, Discussions, Gallery and the rest without checking the OrgConfig feature flags
the web menu honours. Decide which of these screens should be reachable before adding them.
**Acceptance:** each routed member screen is either in the drawer or deliberately removed.

# Work Package 95 — Step-up for email-less admins and by-elections (raised by user 2026-10-03: "if superadmin logged in and he dont have email, how did he get 6 digit code for election menu" and "if any specefic position(s) got empty, there masy needed another elction")

95.2 [PARTIAL 2026-10-21] **Priority: P2 | Depends on: 34.D17-34.D19.** Built 2026-10-21; only the
preprod release is left, and it waits on the F2 gate (34.D17 to 34.D19). Committee positions must be
configurable (user decision 2026-10-03). Before this item only the labels were configurable
(`ecRoleLabels` in org config). The set itself was the `ECPosition` enum (`GHCAA.Domain/Enums.cs:8`), stored as an int on `ECMember.Position`
and `ElectionSeat.Position`, repeated as a fixed list in `lookup.service.ts`, and parsed by
`AdminElectionsController.ParsePosition`. Plan: a `CommitteePosition` table (key, label, sort order,
seats, active) seeded with the current enum values under the same ids, so existing rows map 1:1;
both columns become a foreign key; admin screen to add, rename, reorder and retire positions; web
and mobile read the list from the API instead of the enum.
**Acceptance:** adding a position makes it selectable for assignment and elections without a code
change; retiring one hides it from new use and leaves past terms and results intact.
  - **Plan, 2026-10-14 (round 2).** Decided 2026-10-14: positions are rows per organisation with no fixed set; GHC's 15 are seeded only by `profiles/ghc`, the default profile seeds a short generic set. Session N1, migration `AddCommitteePositionsAndTerms`, which also carries the end-reason table, `Election.Kind` and the live-election index rebuild for F10. 37.1h-1 was merged into this item on 2026-10-21. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.
  - **Decided 2026-10-20 (owner, raised as "EC positions dynamic and configurable to the whole system and forms").** The table is `EcPosition`, with name, Bangla name, order, active flag and three role flags: shown on the board, signs as head and signs as secretary. Forms and ID cards print whichever position holds a flag, with its configured label, instead of the fixed "President" and "General Secretary" in `IDCardService` and the election form signature blocks. Everything that names a position reads the table: `ElectionService.SeatTitle`, `AdminElectionsController.ParsePosition` and its copy of `SeatTitle`, the enum loop in `GovernanceService`, seat order in `ElectionService` and `ElectionDocumentService`, `EC_ROLES` and `getECPositionName` in `app.constants.ts`, the board lists in `governance.ts`, the fallback list in `lookup.service.ts`, `ec-preview`, and the Flutter registry default of 5. Built before the election forms (37.14m onward). Its migration joins the form-fields migration and is tested on local PostgreSQL only until the F2 gate (34.D17 to 34.D19) clears. See `docs/specs/023-election-ballot-secrecy-and-standards/form-inventory-draft.md`, section Owner decisions.
  - **Built 2026-10-21, waiting on the F2 gate.** The `EcPosition` table replaces the enum, with Ids 1 to 15 equal to the old enum values, so `ECMember.Position` and `ElectionSeat.Position` rows keep their meaning and become foreign keys. Migration `20261021100000_AddEcPositions` is guarded for databases built by EnsureCreated. It also keeps any rename an admin made through the old Lookups ECPosition labels, and skips a foreign key with a warning naming the rows when a table still holds the old `None = 0` value. A dry run on a throwaway local PostgreSQL 18 confirmed this, run twice. Admins manage positions from "EC Positions" on the web governance page (add, edit, retire, restore, and delete when never held). The mobile assign panel picks from the API list. Seat count is enforced on both the governance page and the member EC history form. It does not reach preprod until the F2 gate (34.D17 to 34.D19, the preprod migration checks) clears. Still open: rows already holding position 0 need an owner decision before the key can be added by hand; rolling back the migration drops positions an admin added; certificate signatory captions have no PDF test.
  - **Follow-up 2026-10-21 (owner decisions on the three open points).** Rows holding position 0 are reported, not guessed: `GET api/admin/governance/positions/invalid-rows` lists them and `POST api/admin/governance/positions/repair` sets the right position on one row, refusing a position whose seats are all held. Rolling back refuses while admin-made position data exists. The certificate captions moved into `IDCardService.GetSignatureCaptionsAsync` so they can be tested: a renamed signatory prints its new name, and the fixed captions return when no position signs. The Lookups ECPosition group and the `EcRoleLabels` setting are retired as label sources (migration `20261021110000_RetireEcPositionLookups`). The web and mobile admin screens report a failed positions load, mobile keeps Assign off until positions load, and a term on a retired position shows its stored name marked retired. Tests: FR-34 in `GovernanceServiceTests`, `admin-governance.spec.ts`, `admin-members.spec.ts` and `governance_registry_positions_test.dart`. The 15 Lookups rows of group ECPosition (ids 4001 to 4015) were deleted from `GHCAA.Infrastructure/Data/Seed/lookups.json` on 2026-10-21 at the owner's instruction: "Delete lookups - edit seed data". A live database keeps its rows until the retire migration runs.
  - **UI labels from the database [DONE 2026-10-21].** Owner instruction 2026-10-21: "In UI use Positions/roles labels as per configured in DB". Every position label on web and mobile now comes from the `EcPositions` table through the API, with no hardcoded list. The two unused helpers `getCurrentECPosition` and `getECPositionForPeriod` in `app.constants.ts` were deleted on 2026-10-21 at the owner's instruction: "getCurrentECPosition and getECPositionForPeriod in app.constants.ts are unused. DElete them".

95.4 [TODO] **Priority: P2 | Depends on: 95.3.** Vacant-seat view and filter. A seat is vacant when
no `ECMember` row for it in the active period has `EndDate == null`. Show vacant seats on the admin
governance page with a filter, and use the same query for the by-election form.
**Acceptance:** after a term is ended the seat appears in the vacant filter; once filled it leaves it.
  - **Plan, 2026-10-14 (round 2).** Session F10. Vacant means fewer current holders than the position's seat count in the active period. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.

95.5 [TODO] **Priority: P2 | Depends on: 95.2, 95.4.** By-election type. Add an election kind
(General or ByElection) shown on the ballot and results. For a by-election the form offers only
vacant seats. `ElectionService.DeclareAsync` (ElectionService.cs:308) adds a new `ECMember` for each
seat in the election and touches no other seat, which is right for a by-election, but it must first
end any current holder of that seat with reason TermEnded. `GetCurrentAsync` returns only the latest
open election, so two open elections in one period need handling.
**Acceptance:** a by-election for two vacant seats fills only those seats and leaves every other
committee member untouched; tests cover declare, the vacant-only guard and two open elections.
  - **Plan, 2026-10-14 (round 2).** Decided 2026-10-14: declare never ends a current holder. A by-election offers vacant seats only and refuses a held one at create and at declare, so the TermEnded step above is dropped. `IX_Elections_ECPeriodId_Live` is rebuilt on period and kind in N1 so a by-election can run next to a declared general election. Session F10. Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.

95.12 [TODO] **Priority: P2 | Depends on: 95.2, 95.3.** Term ending by date and configurable end
reasons (user decision 2026-10-14: "any person can be removed anytime due to a valid reason, let make
this configurable"). A daily job, built like `DataRetentionSweep` and `ExpiredRevokeSweep`, ends each
open `ECMember` row whose `ECPeriod.EndDate` has passed, with reason TermEnded and an audit entry. It
runs only when the organisation turns it on (off by default). A default term length fills a new
period's end date. An admin can still end any term early with a reason and a note through
`POST members/{ecMemberId}/end-term`. The `VacancyReason` enum becomes an end-reason table per
organisation, seeded with today's five under the same ids; TermEnded and Died are system rows that
cannot be removed. Session N1 (schema) and F10 (job, screens, tests). Plan: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`, Fast track.
**Acceptance:** with the setting on, a term past its period end date is ended by the job with
TermEnded; with it off nothing changes; an admin can add a reason and use it to end a term early;
a system reason cannot be removed.

95.6 [PARTIAL 2026-10-23] **Priority: P3 | Depends on: 95.1.** Authenticator app (TOTP) as a second way to pass
step-up, for admins who would rather not rely on email. Needs a secret per user, enrolment with a
QR code, recovery codes and a reset path for a lost phone.
**Acceptance:** an enrolled admin passes step-up with a 6-digit app code; email still works.
  - **Built 2026-10-23.** Any signed-in user can add an app, not only admins. The API has `GET`, `POST setup`, `POST confirm`, `POST recovery-codes` and `DELETE` under `api/auth/authenticator`; setup, new codes and removal need a fresh step-up first. Codes follow RFC 6238 (SHA-1, 30 seconds, one step either side), written in `GHCAA.Application/Security/Totp.cs` with no new package; the QR comes from QRCoder. The secret is kept encrypted, and the 10 recovery codes are kept hashed and work once each. Five wrong codes lock the app for 15 minutes. Step-up verify takes `method: app` or `email`. A SuperAdmin removes a lost app through `DELETE api/roles/users/{id}/authenticator` (the web roles page has the button). Web: the settings card on the change-password page, and the step-up dialog asks for the app code. Mobile: Profile, Authenticator App, and the step-up dialog asks for the app code. Migration `20261023120000_AddUserAuthenticators` adds the `UserAuthenticators` table. Tests (FR-39): `AuthenticatorServiceTests`, the web `authenticator-settings.spec.ts` and `admin-roles.spec.ts` cases, and mobile `authenticator_screen_test.dart` and `step_up_interceptor_test.dart`.
  - **Owner answer, 2026-10-23: app or email.** An enrolled user is asked for the app code first and no email goes out, but can ask for an emailed code instead when the phone is not at hand. Step-up verify takes either method for every user. The web dialog has an "Email me a code instead" button and a way back to the app; the mobile dialog has the same button. A user with no email on file is not offered the switch.
  - **Gaps closed, 2026-10-23 (owner: "the standard practise").** A user with no email on file sets up the app by entering the password, through `POST api/auth/authenticator/setup-with-password`; the route refuses anyone who has an email, so they still need a step-up. The status call returns `setupNeedsPassword` so web and mobile show the password box only then. The SuperAdmin reset now writes an activity-log row and changes the SecurityStamp, which signs the user out everywhere. Two recovery-code refreshes at the same moment no longer both succeed: the update only lands on the list it read, so the second gets no codes rather than codes that do not work. Recovery codes are compared in constant time. The 15-minute lockout after five wrong codes stays as it is. Tests (FR-39): new `AuthControllerMutationTests` cases for the password route and the email switch, web `step-up.service.spec.ts` and `authenticator-settings.spec.ts` cases, and mobile `step_up_interceptor_test.dart`, `authenticator_screen_test.dart` and the new `step_up_dialog_test.dart`. The mobile dialog now keeps its text controller in a small stateful widget, because disposing it right after the dialog closed broke the field during the closing animation.
  - **Still open:** verify QR enrollment, app-code step-up and switching to email on a real device under 84.50 A1. Keep the device evidence there; this item tracks the TOTP feature, not a second copy of the same check.

# Work Package 96 — Compact admin and portal layout (raised by user 2026-10-03: "titles, and top common are is taking too much space ... navbar height is increased for too many mentu items ... whatever you do do centrally managed way")

96.3 [TODO] **Priority: P3 | Depends on: 96.1.** Pages that still draw their own header or search
instead of the shared ones: member giving, forum and polls (own h1), messages, admin governance,
election personas and the directory (own search CSS), and the Fundraising donor-tier inputs (raw
inputs). Move them onto `app-page-header`, `app-search-bar` and the form tokens.
**Acceptance:** each listed page uses the shared header and search, with no page-level size overrides.
  - **Review, 2026-10-14.** Rechecked against the code. Giving, forum, polls, admin governance, election personas and the directory header already use `<app-page-header>`. Still open: messages (no page header, raw composer input, `<h3>` panes), the directory's raw search `<input>` (`common/directory/directory.html:14`), and the donor-tier and campaign form inputs (`admin/campaigns/admin-campaigns.html:49-51` and `:59-64`). 96.1 is done (archive).

# Work Package 97 — Email verification from the member profile (raised by user 2026-10-12: "add todo for email verification of member from their profile")

97.1 [TODO] **Priority: P2 | Depends on: none.** A member can verify an email only during registration (`MemberService.cs`, where `EmailVerified` is set after the OTP). The profile page has no way to verify or re-verify it. Imported members arrive with `EmailVerified = true` and no check, and a member who changes the email on the profile keeps the old flag. Add to the profile: a status badge (verified or not), a "Verify email" action that sends an OTP with the `EmailVerify` purpose, added to `OtpPurpose`, and a rule that editing the email clears `EmailVerified` until the new address is confirmed. Reuse the registration OTP service and its 5-attempt lock; do not write a second one. Web and mobile together. Decided 2026-10-12: an unverified email only shows the badge and never blocks sign-in. Social sign-in links by email only when `EmailVerified` is true (`AuthService.cs`), so a cleared flag stops auto-linking until the member verifies.
**Acceptance:** a member sees the verified state on the profile, can verify by OTP, and changing the email clears the flag; tests cover the OTP path, the lockout and the clear-on-change rule on the backend, web and mobile.
  - **Review, 2026-10-14.** `OtpPurpose` today is `Registration`, `PasswordReset` and `AdminStepUp` (`GHCAA.Domain/Enums.cs:18`), so `EmailVerify` is a new value. The verify path today is `RegistrationController.cs:89-94` to `MemberService.VerifyEmailAsync`.

# Work Package 98 — Performance pass across database, API, web and mobile (raised by user 2026-10-14: "i want all specs plans, tasks for performance improvement, that mat db, api, FE,design or else first, than start fix")

Spec 024 (`docs/specs/024-performance/`, the performance spec with its plan and task list) holds the findings and their evidence. Four read-only audits of 2026-10-14 covered the database, the API, the web client with its assets, and the mobile client. Nothing found fails at today's scale. The items below cost time or data on every visit, or grow without bound. Finished work is not repeated: 8.3, 8.4, 8.6, 96.13, 28.4, 28.18, 44.17, 45.6. Open 8.5 and 8.7 stay where they are.

Raised by user 2026-10-14: "make sure nothing left open of perprmance,and relevan, test cover all services, review and fix". Every item below is to be closed in this pass, each service it changes gets tests in the same change, and the whole package gets a code review before handoff. While running the CI gates, `GHCAA.Mobile/test/router_test.dart` failed with MissingPluginException because it built the router without a secure-storage mock. It now mocks the channel and disposes its container.

98.7 [TODO] **Priority: P4 | Depends on: 98.6.** Static assets are compressed on each request and nothing serves pre-compressed files. Worth doing only if 98.6 leaves a measurable cost.
Deferred 2026-10-14. It needs a measurement on the deployed site after 98.6, which has not shipped yet.
**Acceptance:** Brotli files served for the built chunks, or the item closed with the measurement.

98.34 [ONHOLD 2026-10-14, until a patched 9.0.x ships or the API moves to .NET 10] **Priority: P4 | Depends on: none.** The `vulnerable-packages` gate in `scripts/ci-local.py` still fails on two transitive packages: `System.Security.Cryptography.Pkcs` 10.0.0 (GHSA-846f-f764-76mx) and `System.Security.Cryptography.Xml` 9.0.20 (GHSA-r955-8q5j-3h83), both High, in the API, Infrastructure and Tests projects. The fixed versions are 10.x builds that fail restore on net9.0, and the other way out needs a paid licence. Decided by the user 2026-10-14: leave them, and treat the other gates as the release check.

98.43 [OPEN 2026-10-16] **Priority: P2 | Depends on: the hold on config and secret files.** Config keys checked against the code that reads them. Read only, key names only: the hold means the user edits `appsettings*.json` and `.env*`. Read by the code but in no file: `EmailSettings:Provider`, `BrevoSettings:ApiKey`, `BrevoSettings:SenderEmail` and `SenderName` (preprod sends mail through Brevo), `AppSettings:PublicApiBaseUrl` (the gateway callback throws without it), `AppSettings:StepUpTtlMinutes` outside the base file, `ContactUsSettings:Recipients`, `PaymentGateways:Bkash:ProductionPassword` and `Username`. Not read by any code: `Jwt:ExpiryMinutes` (`TokenService` fixes 60 minutes), `OtpSettings:MaxAttempts`, `FileStorage:WebRootPath` and both MIME lists, `GeneralSettings:PortalBaseUrl` (the portal URL comes from the org profile), `ConnectionStrings:MySqlConnection`, the Nagad and Rocket URLs, and the `DGePay` `BaseUrl`, `ClientId`, `ClientSecret` and `ApiKey` (DGePay takes its credentials from the database). `appsettings.Production.json` has no `AllowedOrigins` or `ClientUrl`, and startup refuses a non-Development run without origins, so the host has to set both. The `GHCAA.API/.env*` files lack `ORG_PROFILE`, `OtpSettings__HashKey` and `AppSettings__ClientUrl`, which the root files have. `docs/ENV_REVIEW.md`, the environment variable guide, says `DotNetEnv.Env.Load()` reads the root `.env`. It reads `.env` in the working directory, which is `GHCAA.API/` under `dotnet run` there. Web and mobile keys match their code. Acceptance: the user decides which missing keys to add and which unread keys to drop or wire up, and ENV_REVIEW.md names the file that is actually loaded. Progress 2026-10-17: `TokenService` now reads `Jwt:ExpiryMinutes` and `OtpService` reads `OtpSettings:MaxAttempts`. Both fall back to the old 60 and 5 (`Defaults.AccessTokenExpiryMinutes`, `Defaults.OtpMaxAttempts`) when the key is missing or not positive, with tests in `TokenServiceTests` and `OtpServiceTests`. ENV_REVIEW.md now says the API reads `GHCAA.API/.env`. Still open: the missing keys, the other unread keys and the `GHCAA.API/.env*` gaps, which wait on the user's config edits.
