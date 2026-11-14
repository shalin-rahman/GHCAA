# Master execution plan

Started 2026-11-01 under Work Package 99. Rewritten on 2026-11-13 (owner) as small session cards.
This is the only open plan (owner, 2026-11-03). `docs/TODO.md` and `docs/TODO_ARCHIVE.md` stay the
record of status. Every other plan file in `docs/` and `docs/specs/` is a design reference (how, never
when) or a closed record; section 9 lists them.

Each open item sits on one session card in section 4, or on a gate card in section 5 when it waits on
the owner. A card names the files to open, the steps, the test commands and a one-line "Done when".
The tracker entry keeps the full acceptance text. When either one changes, edit both in the same
change. Check the plan against the tracker after any status change:

```
python scripts/backlog_inventory.py --check docs/MASTER_EXECUTION_PLAN.md </dev/null
```

It exits 1 when an open item is missing here or a closed one is still named. Text between the
`backlog-check: off` and `on` markers is history and is not checked, so a card that closes moves its
IDs out of the active text (rule 9 in section 2).

## 1. Handover

- **Date:** 2026-11-13. **Open:** 125 items (`python scripts/backlog_inventory.py`), held 12
  (`--state ONHOLD`).
- **Next session:** S01 (84.100). Then run the sequence in section 3 top to bottom. Do not skip ahead
  unless a card's "After" column is met and the owner asks.
- **In the working tree, not committed (owner owns Git):** the 37.14t print-record work, its
  migration `20261112120000_AddElectionFormPrints`, the 37.16j concurrency token, the ER-17 phase
  bands and the walkthrough "Print records" section. The walkthrough has not run yet; S06 runs it.
- **Owner answers 2026-11-13:** plan as session cards in this file; ER-17 stays gated at Scrutiny
  (closed the same day, see the log); the book is one batch at the end (track H), never inside a feature session; owner-gated
  items get gate cards (section 5) and no slot until the gate clears.
- **Owner gates open:** see section 5. The ones that unblock the most: G-REL (preprod migration
  release batch), G-MAIL and G-UPL (7.26, 7.28, which also unblock 78.9).
- **Commands:**
  - Open items: `python scripts/backlog_inventory.py [--wp N] [--state PARTIAL]`
  - Plan check: the command above.
  - Election checks: `python scripts/verify-election.py` (seven steps).
  - Layer gates: `python scripts/ci-local.py --only backend|web|mobile|contract`; full run at a
    phase end; `--e2e` only when a card says so.
  - After code or doc changes: `graphify update .`

## 2. How to run a session

These rules hold for every card. A card adds only what differs.

1. **Start.** Run the plan check (0 problems) and `git status --short`. Leave changes you did not make
   alone. Read section 1, then only your card, then each item's tracker entry
   (`grep -n "<id> \[\|\*\*<id>\*\*" docs/TODO.md`). Do not read other cards or whole design files.
2. **Pre-check.** `graphify query "<first symbol on the card>"`. If the code already meets "Done
   when", rerun the card's test command, record the result and close the item. Do not rebuild it.
3. **Build order.** Domain, migration, service, controller, backend tests, web, web tests, mobile,
   mobile tests, then registry and docs. Controllers never touch `ApplicationDbContext`.
4. **Tests.** Write the failing test first and see it fail. While working, run only the card's focused
   command. Before handback, run `ci-local.py --only <layer>` for each layer touched. Tag new backend
   tests with their FR or NFR (`[Category("FR-39")]` for elections, `NFR-R5` for a retry or failure
   path). A new web spec or Flutter test for an election change goes into `WEB_SPECS` or
   `MOBILE_TESTS` in `scripts/verify-election.py`. A card that changes a workflow with a local
   walkthrough adds its step and a refusal probe for each new rule there, updates the "What the run
   covers" table and the "Not covered yet" list in that folder's README in the same session, and
   type-checks the file. Elections: `GHCAA.Web/tests/e2e/election-walkthrough/`; registration,
   payment verification and approval: `membership-walkthrough/`; events, registrations, passes,
   check-in and the waitlist: `event-walkthrough/`. The run itself needs the local stack; if it cannot run in the session, say
   so in the tracker entry.
5. **Migrations.** Hand-written `yyyyMMddHHmmss_Name.cs` plus `.Designer.cs` in
   `GHCAA.Infrastructure/Data/Migrations/PgSql/`, snapshot edited to match. Never `ef migrations add`
   then `remove`. There are no SQLite migrations: SQLite (tests, local) builds from the model, so a
   card that says "both providers" means a PgSql migration plus a SQLite test. Take a timestamp later
   than the last row of the ledger in section 7, and land migrations in ledger order. Dry run on local
   PostgreSQL: a fresh database and a copy of `GHCAADB_v2`; apply, delete the history row, apply
   again, roll back to the previous migration, apply again. A sequence reset is written as
   `DO $sv$ BEGIN PERFORM setval(...); END $sv$;`, never a bare `SELECT setval`.
6. **Literals and UI.** New literals go in `GHCAA.Domain/Constants.cs` and
   `GHCAA.Web/src/app/core/constants/app.constants.ts` (mobile: `app_constants.dart`). Labels and
   lists come from OrgConfig and lookups. Web screens use `src/app/common/` components and theme
   tokens; mobile gets its own layout. Dates show as `dd-MM-yyyy` through the shared helpers.
7. **Review.** A card sized M or L gets one `code-reviewer` pass on its diff. Cards marked "security"
   also get `security-reviewer`. Fix each confirmed finding with a test before handback, and route it
   (guard rule in `scripts/guard.py`, a path rule in `.claude/rules/`, or a question added to a later
   card).
8. **Contract.** Any route or DTO change: entry in `docs/API_CONTRACT_REGISTRY.md`, both clients
   checked, `python scripts/ci-local.py --only contract`. A new route also goes through
   `authz_catalog.py --write` and the reflection test list.
9. **Close.** In one edit: the tracker entry gets its status and evidence (command and count); the
   session row in section 3 becomes "done YYYY-MM-DD, see log" with its IDs removed, and its card is
   deleted (the checker flags a closed ID left in active text); the progress log (section 12) gets
   one row with the IDs and evidence; section 1 names the next session; spec task boxes follow. An
   item that stays open after its card keeps its ID on the next card. Then `graphify update .` and
   the plan check.
10. **Park, do not stretch.** If a card grows past its size, set the item PARTIAL with the step left,
    write that step as the first step of the next card, and stop.
11. **Owner work.** Questions go through AskUserQuestion with options and a recommended first option.
    A card that lists a question already gives its default; ask, then build. Owner steps (device,
    preprod, inbox) go to the gate cards in section 5, never into a session.
12. **Hands off.** Never commit, push or deploy. Never edit `.env*`, secret or connection keys in
    `appsettings*.json`, or `Seed/*.json`, unless the card names the owner's approval.
13. **Book.** No `docs/book/` edit or build outside track H. A card that changes a number or claim the
    book quotes adds one line to the H00 list instead.
14. **Windows.** With the API running, build tests to `-o GHCAA.Tests/bin/alt-out`. Check free space
    on C: before a ci-local run. Python edit scripts are files run with `</dev/null`; keep each
    file's BOM and line endings.
15. **Tone.** The CLAUDE.md tone rules apply to every comment, doc and tracker line.

## 3. Session sequence

Size: S is one file or one test file, M is one layer, L crosses layers or carries a migration. Mig is
the ledger row in section 7. Run top to bottom.

| # | Items | Size | Layers | Mig | After |
|---|---|---|---|---|---|
| **A** | **CI health and the rest of session 2b** | | | | |
| S01 | 84.100 | M | web tests, ci-local | | |
| S02 | 84.101, 84.99 | S | ci-local, migrations SQL | | |
| S03 | 37.16j | S | backend | | |
| S04 | 37.16k | M | backend | | S03 |
| S05 | 37.16i | M | API, web | | S04 |
| S06 | 37.14n, 37.14t | S | local stack run | | S05 |
| S07 | 99.3 | S | full ci-local | | S01, S02 |
| **B** | **P1 items with no owner gate** | | | | |
| S08 | 84.25, 28.25 | M | backend, e2e | | |
| S09 | 84.51, 84.49 (API part) | S | local API | | |
| S10 | 84.92 (backend) | M | API | | |
| S11 | 84.92 (clients) | M | web, mobile | | S10 |
| S12 | 84.80 (backend) | M | API | | |
| S13 | 84.80 (clients) | M | web, mobile | | S12 |
| S14 | 84.81 | L | API, OrgConfig | 3 | |
| S15 | 88.1 | L | API | 4 | |
| S16 | 88.2, 88.3 | M | API | | S15 |
| S17 | 88.5 (backend) | M | API | | S16 |
| S18 | 88.5 (grid) | M | web | | S17 |
| S19 | 88.4 | S | local PostgreSQL | | S16 |
| **C** | **Election sessions 3 to 6** | | | | |
| S20 | 37.16f | S | backend | | |
| S21 | 37.16g | M | backend | | S20 |
| S22 | 37.16h (backend) | L | API | 5 | S21 |
| S23 | 37.16h (clients) | M | web, mobile | | S22 |
| S24 | 37.14d, F2 schema | L | migration | 6 | S23 |
| S25 | 37.16e | M | backend | | S24 |
| S26 | 37.1p (recount, consent, reopen) | M | backend | | S25 |
| S27 | 37.14a | M | backend, security | | S26 |
| S28 | 37.12y | M | backend, security | | S27 |
| S29 | 37.1p (unopposed, paper counts) | M | backend, security | | S28 |
| S30 | 37.14f | M | backend, security | | S29 |
| S31 | F5 web part 1 | M | web | | S30 |
| S32 | F5 web part 2 | M | web | | S31 |
| S33 | F6 mobile, 84.49 (B4-B5) | M | mobile | | S32 |
| **D** | **Meetings, then the model work after it** | | | | |
| S34 | 84.19 (T002, T002a, T003) | L | domain, migration | 7 | |
| S35 | 84.19 (T004 to T006) | M | API | | S34 |
| S36 | 84.19 (T007 to T009) | M | API | | S35 |
| S37 | 84.20 (T010 to T012) | M | API | | S36 |
| S38 | 84.19 (T013 to T015) | M | API | | S37 |
| S39 | 84.19 (T016 to T018) | M | web | | S38 |
| S40 | 84.19 (T019 to T020) | M | mobile | | S39 |
| S41 | 84.19, 84.20 (T021, close) | S | docs | | S40 |
| S42 | 84.59 | M | API, both clients | | S41 |
| S43 | 84.84 (backend) | L | API | 8 | S41 |
| S44 | 84.84 (clients) | M | web, mobile | | S43 |
| **E** | **Election sessions 7 to 9** | | | | |
| S45 | 95.2 (N1 migration) | L | migration | 9 | S33 |
| S46 | 95.4 | M | API, web, mobile | | S45 |
| S47 | 95.12 (backend) | M | API, security | | S46 |
| S48 | 95.12 (clients) | M | web, mobile | | S47 |
| S49 | 95.5 (backend) | M | API | | S48 |
| S50 | 95.5 (clients) | M | web, mobile | | S49 |
| S51 | 37.13f | S | docs, backend log | | S50 |
| **F** | **P2 items** | | | | |
| S52 | 84.4 | S | docs | | |
| S53 | 93.3 | S | e2e | | |
| S54 | 84.27 (API tests) | S | backend | | |
| S55 | 84.27 (web page) | M | web | | S54 |
| S56 | 84.55 | M | API, clients check | | |
| S57 | 84.93 | M | API, web | | |
| S58 | 84.97 | M | API, web | | |
| S59 | 84.67 | M | mobile | | |
| S60 | 34.D8 | M | local stack | | |
| S61 | 84.65 | M | web | | |
| S62 | 49.3.C | L | API, web | 10 | |
| S63 | 97.1 (backend) | M | API | | |
| S64 | 97.1 (clients) | M | web, mobile | | S63 |
| S65 | 84.83 (backend) | L | API | 11 | |
| S66 | 84.83 (clients) | M | web, mobile | | S65 |
| S67 | 84.82 (backend) | L | API | 12 | |
| S68 | 84.82 (clients) | M | web, mobile | | S67 |
| S69 | 84.72 (backend) | L | API | 13 | |
| S70 | 84.72 (web) | M | web | | S69 |
| S71 | 84.72 (mobile) | M | mobile | | S70 |
| S72 | 92.1 | S | spec | | |
| S73 | 92.2 | L | API | 14 if table | S72 |
| S74 | 92.3 | M | web | | S73 |
| S75 | 92.4, 92.5 | S | mobile, docs | | S74 |
| S76 | 37.7 (web mechanism) | M | web | | |
| S77 | 37.7 (web strings) | M | web | | S76 |
| S78 | 8.8 | M | mobile | | S77 |
| S79 | 62.2 | M | profiles | | |
| S80 | 51.4 | S | backend tests | | |
| S81 | 27.1, 27.8 | M | ci | | |
| S82 | 73.5 | M | scripts | | |
| S83 | 73.6 | S | backend test | | |
| **G** | **P3 and P4 items** | | | | |
| S84 | 84.57, 84.62 | S | API | | |
| S85 | 84.64, 93.4 | S | mobile | | |
| S86 | 84.69 | M | mobile | | |
| S87 | 84.23 | M | mobile, API | | |
| S88 | 96.3 | M | web | | |
| S89 | 84.90 | M | API, both clients | | |
| S90 | 84.95 | S | web, mobile | | |
| S91 | 84.94 | M | API, web | | |
| S92 | 84.66 | M | API, both clients | | |
| S93 | 51.5 | M | API, web | | S80 |
| S94 | 84.98 | M | web, mobile | | |
| S95 | 90.1, 90.2 | L | API | 15 | S18 |
| S96 | 90.3, 90.5 | M | API | | S95 |
| S97 | 90.4 | M | web | | S96 |
| S98 | done 2026-11-13, see log | | | | |
| S99 | 37.4 (backend) | L | API | 16 | |
| S100 | 37.4 (clients) | M | web, mobile | | S99 |
| S101 | 37.5 (backend) | L | API | 17 | |
| S102 | 37.5 (clients) | M | web, mobile | | S101 |
| S103 | 37.10 | L | API, web, mobile | | S100 |
| S104 | 62.56 (a, g) | M | API | | S79 |
| S105 | 62.56 (b) | M | API, web | | S104 |
| S106 | 62.56 (c, d) | L | API, web | | S105 |
| S107 | 62.56 (e, f) | M | API, docs | | S106 |
| S108 | 60.3, 6.2, 8.5, 8.7, 37.9 | S | decisions | | |
| S109 | 37.11 | S | WP37 gate | | S103 |
| **H** | **Book track, on the owner's word** | | | | |
| H00 | book facts list | S | docs | | |
| H01 | 78.10 | M | measurements | | |
| H02 | 63.8, 84.35 | S | book counts | | H01 |
| H03 | 84.96, 64.8 | S | book | | |
| H04 | 64.9, 67.3 | M | book figures | | |
| H05 | 67.2 (chapters 7, 10) | M | book | | H01 |
| H06 | 67.2 (chapter 9) | M | book | | H05 |
| H07 | 67.2 (chapters 11 to 13), 67.4 | M | book | | H06 |
| H08 | 67.7, 67.5 | M | book | | H07 |
| H09 | 63.34, 63.18, 74.3 | S | book | | H08 |

Owner-gated items and where they re-enter: section 5.

## 4. Session cards

Each card: what to read, the steps, the test command, and "Done when". Rules in section 2 apply.

### Phase A. CI health and the rest of session 2b

**S01 · 84.100 · M**
- Read: `GHCAA.Web/vitest.config.ts`, its `setupFiles`, the web step in `scripts/ci-local.py`.
- Steps: (1) `npm run test:unit 2>&1 > %TEMP%\vt.log`; list the 36 files with "reading 'config'".
  (2) Run them alone in one call; they pass in groups of 20, so bisect the full file list by halves
  until one spec makes the next files fail. Look for `vi.stubGlobal`, `vi.mock` of a module that
  holds `config`, or a spec that replaces `window`/`environment` without restoring it. (3) Fix the
  cause in that spec, or set `unstubGlobals: true` / `restoreMocks: true` in `vitest.config.ts` if
  several specs leak. (4) In `ci-local.py`, fail the web step when vitest's "Test Files" line reports
  any failed file, not only failed tests.
- Tests: `npm run test:unit` twice; `python scripts/ci-local.py --only web`. Prove step 4 with a
  temporary spec that throws in `describe`, then delete it.
- Done when: 129 of 129 files pass twice in a row and ci-local fails on a file-level error.

**S02 · 84.101, 84.99 · S**
- Read: the contract step in `scripts/ci-local.py`; the five migrations named in 84.99.
- Steps 84.101: before `dotnet tool restore`, walk from the repo root up to the drive root and list
  every `dotnet-tools.json` and `.config/dotnet-tools.json`; when restore succeeds but the swagger
  tool is missing, print which manifest was used. Owner step: rename `C:\Users\dotnet-tools.json`.
- Steps 84.99: in each of `20260927121346_ElectionBallotSecrecy`, `20260930020138_ElectionAppointments`,
  `20261021100000_AddEcPositions`, `20261105120000_SeedJobCategoryLookups`,
  `20261105200808_AddPollDiscussion`, change each bare `SELECT setval(...)` to
  `DO $sv$ BEGIN PERFORM setval(...); END $sv$;`. `PERFORM` alone is wrong: it is PL/pgSQL only and
  fails in the plain `Migrate()` run. Applied databases never rerun these, so live data is unaffected.
- Tests: put the manifest walk in a function `find_tool_manifests(start)` in `ci-local.py` and test it
  in a new `scripts/test_ci_local.py` with a temp folder holding a stray manifest
  (`python -m unittest scripts/test_ci_local.py`); on an empty local PostgreSQL: `dotnet ef migrations script --idempotent -o x.sql`, apply
  `x.sql` twice with `psql`, then start the API once against another empty database (`Migrate()`).
- Done when: contract passes on this machine; a stray manifest gives a message naming it; the
  idempotent script applies twice and `Migrate()` still builds an empty database.

**S03 · 37.16j · S**
- Read: `RevokePrintAsync` in `ElectionDocumentService.cs`; the test
  `RevokePrint_RevokedBetweenReadAndSave_ReturnsAlreadyRecorded`.
- Steps: run the same race on local PostgreSQL. Add a test fixture switch or a one-off console
  check that opens two contexts on a PG test database, revokes from both, and counts `RevokedAt`
  rows and `FormPrintRevoked` audit entries. Record what happens when the audit write fails after the
  claim: the claim and the audit entry are in one `SaveChanges`, so both roll back; write that as the
  decision in the tracker.
- Tests: `dotnet test --filter "FullyQualifiedName~ElectionDocumentServiceTests"`.
- Done when: one `RevokedAt` and one audit entry on PostgreSQL; 37.16j DONE.

**S04 · 37.16k · M**
- Read: `RateLimitingExtensions.cs`, the `CredentialVerification` policy, the election settings in
  `OrgConfigDto.cs`, the filled-print path in `ElectionDocumentService.cs`, memory
  `gotcha_ratelimit_api_policy_overrides_named_policies`.
- Steps: (1) a named policy `PrintVerification` in `Constants`; its permit count and window read from
  the OrgConfig election settings (add `PrintVerifyLimit` default 60 and `PrintVerifyWindowMinutes`
  default 15, golden snapshot and `CONFIG_DRIVEN_FRAMEWORK.md` updated). (2) Put it on
  `GET api/elections/print-records/{printId}/verification`; check the global limiter does not replace
  it. (3) On a filled print, look up a row with the same election, form, printer user and content
  hash that is not revoked; return its id instead of inserting.
- Tests: controller test for the policy attribute; service test for reuse and for a revoked row not
  reused; a rate-limit test showing card verification still answers after the print limit is hit.
- Done when: own policy; second identical download returns the same id; registry and authz catalog
  updated.

**S05 · 37.16i · M**
- Read: `ElectionsController.cs` revoke action; the officials area under
  `GHCAA.Web/src/app/admin/elections/`; `common/confirm-dialog`, `step-up-dialog`.
- Steps: (1) `GET api/elections/{id}/print-records` (officials with Count and ManageSetup) returning
  form code, printed at, role, phase, revoked at, reason. (2) Web: a "Print records" tab in the
  election's officials view with the list and a Revoke action (reason, at least 3 visible
  characters, step-up). Hide the action without both permissions. (3) Mobile: none. Mobile has no
  official form screens, since admin steps live on the web; write that into the tracker entry.
- Tests: controller tests (200, 403); Vitest for the dialog validation and the hidden action.
  Browser check light and dark with the API mocked (memory `reference_browser_check_mocked_api`).
- Done when: revoke from the web shows "Print revoked" on `/verify/form/:id`; 403 without permission;
  registry updated.

**S06 · 37.14n, 37.14t · S**
- Read: `scripts/run-election-walkthrough.bat`, memory `gotcha_election_walkthrough_local_run`.
- The run now also checks: roll unfreeze and refreeze, filled prints refused before their phase,
  the receipt PDF on API ballots, a handover tick, and an unknown form code (added 2026-11-13,
  type-checked, not yet run).
- Steps: (1) restart the local API (applies the print migration; `.env` already has
  `GeneralSettings__PortalBaseUrl`). (2) `npx ng serve --host 0.0.0.0`, port 4200 open in the
  firewall. (3) Run the walkthrough; the Print records section must pass. (4) Owner scans one printed
  QR code with a phone; record phone, browser and result.
- Done when: walkthrough Print records green; phone scan opens the verify page; 37.14n and 37.14t
  DONE (the preprod release is G-REL, not part of these items' acceptance).

**S07 · 99.3 · S**
- Steps: `python scripts/ci-local.py` (full), after S01 and S02. Name each failure with its cause
  (98.34's `npm audit` stays red, on hold). Ask the owner to accept wave 1 with the result.
- Done when: owner accepts; 99.3 DONE.

### Phase B. P1 items with no owner gate

**S08 · 84.25, 28.25 · M**
- 84.25: both parts exist. `MentorshipControllerTests.cs` covers the controller; `CreateJobDto` has
  DataAnnotations (FluentValidation never runs, so the text asking for a validator is wrong). Run
  `dotnet test --filter "FullyQualifiedName~MentorshipControllerTests|FullyQualifiedName~JobDtoValidationTests"`
  and check the 401, 400, 409 and AdminOnly cases exist; add any missing one.
- 28.25: run `npx playwright test tests/e2e/config-regression.spec.ts` against the local stack; record
  the result and close 28.25 as covered by it. Do not run `--update-snapshots` on the whole suite.
- Done when: both DONE with the counts.

**S09 · 84.51, 84.49 (API part) · S**
- Steps: local stack up. 84.51: five wrong passwords on `setup-with-password` give 423 on the sixth
  and on login; `POST api/roles/users/{id}/unlock` clears it; record the HTTP log. 84.49: import one
  member through `MemberImportController` (placeholder NID), then `PUT api/profile` with a valid
  10-digit NID succeeds and an invalid one gives 400 with the reason.
- Done when: 84.51 DONE; 84.49 notes the API check passed and stays PARTIAL for B4-B5 (S33) and the
  device check (gate G-DEV).

**S10 · 84.92 (backend) · M**
- Read: `FinancialsController.cs` (`update-status`), `FinancialService.UpdatePaymentStatusAsync`,
  `PaymentHistory.cs`, `INotificationService`.
- Steps: (1) `GET api/financials/pending` (AdminOnly): Pending declarations with member name, due,
  amount, reference, proof path, submitted at. (2) Replace the query-string `update-status` with
  `POST api/financials/{id}/verify` and `POST api/financials/{id}/reject` (body `{ reason }`, at least
  3 visible characters); keep `update-status` working for one release and mark it deprecated in the
  registry. (3) Only Pending can change; a settled payment gives 409. (4) Verify marks the due paid
  (`MembershipDue.IsPaid`, `PaymentDate`, `PaymentHistoryId`); reject stores the reason in `Notes`.
  (5) Notify the member both ways; activity row with admin, old and new status.
- Tests: `FinancialServiceTests` and `FinancialsControllerTests` for verify, reject without a reason,
  second action on a settled payment, the due marked paid.
- Done when: backend acceptance of 84.92 holds; registry updated.

**S11 · 84.92 (clients) · M**
- Steps: web `admin/ledger` (or a new `admin/payments-pending` under the finance nav section): list,
  open proof through `SecureFilesController`, verify, reject with reason. Member payments page on web
  and mobile shows status and the rejection reason.
- Tests: Vitest for the page and service; Flutter widget test for the reason on the member screen.
- Walkthrough: membership walkthrough verifies the fee on the new screen and rejects one declaration
  with a reason; drop its 84.92 gap and README line.
- Done when: 84.92 DONE; 78.9 notes Treasurer Tasks 1 and 2 unblocked.

**S12 · 84.80 (backend) · M**
- Read: `EventService.cs` registration path, `EventRegistrationStatus` in `Enums.cs`, the
  registration configuration (check whether the status is stored as int or string).
- Steps: (1) add `Cancelled` at the end of `EventRegistrationStatus` (no schema change either way).
  (2) `POST api/events/registrations/{id}/cancel` for the owner of the registration. (3) In one
  transaction: cancel, then promote the oldest Waitlisted registration of that event to Approved (or
  Pending when the event needs payment) and notify it. (4) A paid registration: cancel is allowed,
  refund is out of scope; write `Notes` "refund handled by the treasurer" and notify the treasurer
  role. Record that decision in the tracker.
- Tests: full event promotes one; empty waitlist; paid registration; cancel by someone else 403.
- Done when: backend acceptance holds; registry updated.

**S13 · 84.80 (clients) · M**
- Steps: cancel action on "My registrations" on web and mobile with a confirm; refresh counts.
- Tests: Vitest and a Flutter widget test for the action and the confirm.
- Walkthrough: event walkthrough cancels from My Participations and checks the first waitlisted
  registration moves up; drop its waitlist gap and README line.
- Done when: 84.80 DONE.

**S14 · 84.81 · L · migration 3**
- Read: `ExpiredRevokeSweep.cs` (pattern), `MembershipDue.cs`, `EventRegistration`, email templates
  (`FEE_REMINDER`), OrgConfig workflow settings.
- Steps: (1) migration `AddReminderSentMarks`: `MembershipDue.ReminderSentAt`,
  `EventRegistration.ReminderSentAt` (nullable). (2) OrgConfig `Workflow.DueReminderLeadDays`
  (default 14) and `Workflow.EventReminderLeadDays` (default 2); 0 turns a reminder off; golden and
  `CONFIG_DRIVEN_FRAMEWORK.md`. (3) `ReminderSweep` hosted service, daily, like `ExpiredRevokeSweep`:
  unpaid dues with `DueDate` inside the window and no mark, approved registrations for events
  starting inside the window and no mark; send one email (and push when 84.61 lands), set the mark in
  the same save.
- Tests: window edges (day before, first day, last day), a paid due, a second run sends nothing,
  lead 0 sends nothing.
- Done when: 84.81 DONE; dry run of the migration per rule 5.

**S15 · 88.1 · L · migration 4**
- Read: spec 008 `spec.md` FR-1 to FR-3 and `plan.md`; `MemberImportService.cs`,
  `MemberImportController.cs`.
- Steps: (1) before the index, a query that lists duplicate `MembershipNumber` values; the migration
  `AddMembershipNumberUniqueIndex` aborts with that list when any exist (raise in a DO block), and the
  tracker records the local result. (2) Accept `.csv` beside `.xlsx` (same column map). (3) For each
  member: insert AcademicRecord, ProfessionalRecord, PaymentHistory, User and UserRoles only when a
  matching row (natural key per table, listed in spec 008) is missing.
- Tests: `MemberImportServiceTests`: csv path; each related table not duplicated on rerun.
- Done when: FR-1 to FR-3 tests pass; migration dry run per rule 5.

**S16 · 88.2, 88.3 · M**
- Steps: one transaction through the execution strategy around the whole run; response gains
  per-table insert, update and skip counts. Integration tests: a full member imported twice gives zero
  inserts the second time with equal row counts; a forced failure mid-batch leaves nothing.
- Done when: both DONE; registry entry for the response shape.

**S17 · 88.5 (backend) · M**
- Steps: `POST api/admin/member-import/preview` runs mapping, validation and idempotency and writes
  nothing; returns rows with `{ rowIndex, columnKey, message }` errors. Commit refuses a file whose
  preview has errors.
- Tests: preview writes nothing; cell errors carry row and column; commit refused while invalid.

**S18 · 88.5 (grid) · M**
- Steps: `src/app/common/data-grid/` generic component: inputs rows, columns, cell errors, editable
  flag; outputs cellEdit and commit. Theme tokens only. Import page uses it; commit disabled while any
  error remains. 90.4 reuses the same grid read-only.
- Tests: Vitest for the grid (error marking, edit event, commit blocked) and the import page.
- Done when: 88.5 DONE.

**S19 · 88.4 · S**
- Steps: local PostgreSQL copy seeded from `profiles/ghc/demo-data/`; count rows of Members,
  AcademicRecords, ProfessionalRecords, PaymentHistory, Users, UserRoles before, after run 1, after
  run 2 (SQL in the tracker). This runs locally; no preprod.
- Done when: counts after run 2 equal counts after run 1; 88.4 DONE.

### Phase C. Election sessions 3 to 6

Design reference for every card here: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`
sections F2 to F6. Where it disagrees with a card, the card wins (section 8 lists why).

**S20 · 37.16f · S**
- Read: `ElectionService.FreezeVoterRollAsync`, `GovernanceService.cs` near the voting-member check.
- Steps: `Constants.Membership.VotingTypes` (Founding, Executive, General) and one static
  `MembershipRules.IsVotingMember(MembershipType)` in `GHCAA.Domain`; both services call it.
- Tests: one `[TestCase]` per membership type in each service's tests giving the same answer.

**S21 · 37.16g · M**
- Read: `ElectionService.DeclareAsync` (ECMember insert), the governance assignment method in
  `GovernanceService` used by `AdminGovernanceController` assign.
- Steps: declare calls the governance assignment path for each winner (seat count, term dates from
  the period, audit row). A test election still writes no ECMember rows.
- Tests: declared winner gets the same ECMember fields and audit row as a manual assignment; existing
  declare tests pass.

**S22 · 37.16h (backend) · L · migration 5**
- Read: `SubmitNominationAsync`, `IFileValidationService`, `IFileStorageService`, ballot read path.
- Steps: (1) migration `AddNominationPhotoApproval`: `Nomination.PhotoStatus` (int, default 0 None),
  `PhotoApprovedByUserId`, `PhotoApprovedAt`. (2) `POST api/elections/{id}/nominations/{nid}/photo`
  (the nominee) through file validation and storage under `uploads/members`; the free `PhotoPath` in
  the request is ignored. (3) `POST .../photo/approve` and `/reject` for officials with Scrutiny
  rights, audited. (4) Ballot and public lists show the photo only when Approved.
- Tests: unknown path refused, upload validated, ballot hides pending and rejected photos, audit row.

**S23 · 37.16h (clients) · M**
- Steps: web nominate form uploads the photo; officials scrutiny view approves or rejects; mobile
  nominate uploads; both ballots show only approved photos.
- Tests: Vitest and Flutter tests for upload and the hidden photo. Done when: 37.16h DONE.

**S24 · 37.14d and the F2 schema · L · migration 6**
- Already in the model from earlier work, do not add again: `Election.RecountWindowEndsOn`,
  `Nomination.ConsentAt`, `ElectionRecountRequest` (one row per filing with decision fields),
  `ElectionRecountFigure`.
- Migration `ElectionF2` adds: `ElectionConflictDeclaration` (appointment, nomination, relation enum
  Spouse, Parent, Child, Sibling, Proposer, Seconder; declared by, declared at); `ElectionKeyCopy`
  (election, holder appointment, recorded at, confirmed at, destroyed at); `ElectionPaperCount`
  (election, issued, unused, spoiled, entered by, entered at, approved by, approved at);
  `ElectionSeat.NominationsReopenedUntil`; `Election.PollingExtensionCount` (default 0),
  `Election.MaxPollingExtensions`, `Election.MaxExtensionHours`, `Election.NominationReopenDays`
  (nullable, null means the OrgConfig default); `ElectionPurgeRecord` (election id, title, declared
  on, requested by, approved by, purged at, summary json; FK to Elections Restrict); audit FK
  Cascade to Restrict (`ElectionConfigurations.cs`). OrgConfig election defaults: key copies 2,
  extension hours 24, extensions 2, reopen days 3. The election create and edit DTOs take the three
  per-election limits (null means the default) and refuse a value above the default; registry entry.
- Tests: deleting an election with audit entries fails on SQLite and PostgreSQL; a chain with its last
  entry removed no longer matches the printed head; `ER29_PrintsTheAuditChainHead` passes; creation
  refuses per-election limits above the OrgConfig defaults (37.12y acceptance part).
- Done when: 37.14d DONE; full chain dry run per rule 5 on a non-empty copy; code-reviewer on the
  migration (idempotency, rollback, provider).

**S25 · 37.16e · M**
- Steps: `ElectionAccessService.CheckConflictAsync` called from `SubmitNominationAsync`,
  `AddCandidateAsync`, `DecideNominationAsync`, `ElectionAppointmentService.AppointAsync` and
  `AcceptAsync`; named refusal codes in `Constants.Elections`; declarations on accept and later by
  the official or the Chief Commissioner, audited. Drop "not built yet" in
  `docs/Elections/00-Election-Master-Overview.md` (section 2 item 6, step 13, section 6, VVSG table).
- Tests: each path refused with its code; an untied official still decides; a later declaration
  takes effect at once. Done when: backend acceptance holds (client screens in S32, S33).

**S26 · 37.1p (recount, consent, reopen) · M**
- Steps: (1) recount decision on `ElectionRecountRequest` becomes a two-person step through
  `ElectionApprovalService`; at most one undecided request per seat (one row per filing stays, as
  37.14m built it). (2) A nomination without `ConsentAt` cannot move to the candidate list. (3) Chief
  Commissioner reopens nominations for a seat with no valid candidate: sets
  `NominationsReopenedUntil` from the election's reopen days, refused if it would pass the roll
  freeze; a seat still empty at polling stays vacant.
- Tests: second approver needed and audited; no consent blocks the list; reopen one seat; reopen past
  the freeze refused; empty seat at polling vacant.

**S27 · 37.14a · M · security**
- Steps: holders named at key set (appointed, not candidate, distinct), copies recorded up to the
  configured count, each holder confirms only their own copy; readiness (`GetReadinessAsync`) lists
  missing copies and confirmations and blocks Polling; key destroy records copies destroyed.
- Tests: each refusal; readiness lists what is missing; Polling opens only when complete.

**S28 · 37.12y · M · security**
- Steps: extension request (hours, reason) by the Chief Commissioner, second approval with step-up,
  within the election's limits, only in Polling; on approval move `PollingClosesOn`, increment the
  count, notify voters once through `INotificationService`, audit both steps. A request past the
  count names the L3 void-and-rerun procedure.
- Tests: each refusal case on the tracker; one notice per approved extension; audit rows.

**S29 · 37.1p (unopposed, paper counts) · M · security**
- Steps: a seat with one valid candidate is left off the ballot and declared at declare marked
  unopposed (result and certificate); all seats unopposed or vacant: CandidateList to Declared as a
  two-person step with step-up. Paper counts entered by a polling officer, approved by the Chief
  Commissioner before the count; refused unless issued equals used plus unused plus spoiled; printed
  on the poll-closing certificate.
- Tests: unopposed seat absent from the ballot and declared; all-uncontested path only through the
  two-person step; counts in the count only after approval. Done when: 37.1p backend complete.

**S30 · 37.14f · M · security**
- Steps: pin test first (no job deletes election rows). Dashboard query: elections declared more than
  ten years ago. Purge request plus second approver (SuperAdmin when no Commission sits); in one
  transaction delete Ballot, BallotVote, SeatVote, PendingBallot, BallotReceipt, ElectionApproval and
  ElectionAuditEntry rows of that election; keep Election, seats, nominations and ElectionResult;
  write `ElectionPurgeRecord`.
- Tests: pin; under ten years refused; second approver needed; rows gone and result plus record kept.

**S31 · F5 web part 1 · M**
- Screens in the web officials area: readiness list with attestation buttons, key holders and copy
  confirmation, polling extension request and approval, paper counts entry and approval, and the
  three per-election limits on the create and edit form.
- Tests: Vitest per component (refusal codes shown through the interceptor message, hidden actions
  by permission); add specs to `WEB_SPECS`. Browser check light and dark.

**S32 · F5 web part 2 · M**
- Screens: recount decision, reopen nominations, conflict declaration at acceptance and later,
  consent entry, ten-year list and purge request, the all-uncontested declare, refusal messages on
  nominate, add candidate, appoint and accept.
- Done when: 37.16e, 37.14a, 37.12y and 37.14f DONE once S33 covers their mobile parts.

**S33 · F6 mobile, 84.49 (B4-B5) · M**
- Steps: candidate recount request, consent shown, readiness and phase read-only, conflict
  declaration at acceptance, confirm own key copy, refusal messages. 84.49 B4: the key progress
  dialog closes through its own dialog context; B5: a 60 s timeout on the server save only, retry
  sends the same key so no second key is stored (`docs/AUTH_AND_DEVICE_FIX_PLAN.md` B4, B5).
- Tests: widget tests (dispose mid-key leaves no dialog; slow save shows the timeout and retry);
  add to `MOBILE_TESTS`; `python scripts/verify-election.py` seven of seven.
- Done when: 37.1p, 37.16e, 37.14a, 37.12y, 37.14f DONE; 84.49 PARTIAL only for the device check.

### Phase D. Meetings, then the model work after it

Design reference: `docs/specs/026-meetings-and-action-items/` (spec, plan, tasks) and spec 028 for the
discussion service. Tick the spec 026 task boxes as each card closes.

**S34 · 84.19 T002, T002a, T003 · L · migration 7** Entities, enums, `Poll.MeetingId`, constants,
`MeetingType` lookup group (migration data step, not `Seed/*.json`), list filters, migration
`AddMeetings` with dry run. Tests: model builds on SQLite; migration rule 5.

**S35 · 84.19 T004 to T006 · M** Services, DTOs, admin and member controllers, announcements
(FR-25 to FR-27) reusing the poll announcement path. Tests T006.

**S36 · 84.19 T007 to T009 · M** Agenda ordering and status, participants, RSVP until start,
attendance, decisions, outcome, minutes publish to Closed. Tests T008 and the minutes part of T012.

**S37 · 84.20 T010 to T012 · M** To-do lists per meeting (FR-18 to FR-19b), "My to-dos", create
next meeting with carry-over. Tests T010a, T012.

**S38 · 84.19 T013 to T015 · M** Attach polls, audience rule for poll reads and votes, `Meeting`
context in the spec 028 discussion service. Tests T015.

**S39 · 84.19 T016 to T018 · M** Web admin list and editor, lookup values editable, member list and
detail, event page and dashboard links. Vitest T018.

**S40 · 84.19 T019 to T020 · M** Mobile member and admin screens (admins only, D1). Widget tests at
phone width.

**S41 · 84.19, 84.20 T021 · S** Registry, `PROJECT_MAP.md`, feature catalogue, swagger snapshot,
`ci-local.py --only backend|web|mobile|contract`. Done when: 84.19 and 84.20 DONE.

**S42 · 84.59 · M**
- Steps: typed DTOs for `IMentorshipService` (three methods), `INetworkingService.GetECPeriodsAsync`,
  `IContactService.GetMessagesAsync`, `IChatService.GetRecentChatsAsync`, keeping the current JSON
  property names so clients do not change; regenerate `docs/api/swagger.json`; compare web and mobile
  models with it.
- Tests: a serialization test per DTO pinning the property names; existing controller tests.

**S43 · 84.84 (backend) · L · migration 8**
- Steps: `Member.City`, `Member.Country` (nullable, max 100), migration `AddMemberLocation`;
  `MemberSearchFilterDto.City`, `.Country` (case-insensitive, trimmed); a member with
  `IsAddressPublic` off is never matched by location and the fields are blanked in directory output.
- Tests: match, case, hidden member, blanked fields.

**S44 · 84.84 (clients) · M** Profile fields and directory filters on web and mobile. Tests on both.
Done when: 84.84 DONE.

### Phase E. Election sessions 7 to 9

**S45 · 95.2 (N1) · L · migration 9**
- Steps: migration `AddCommitteePositionsAndTerms`: `EcEndReason` table (seeded with today's five
  `VacancyReason` values under the same ids; TermEnded and Died marked system), `ECMember` end reason
  FK, `Election.Kind` (General 0, ByElection 1), `ECPeriod.DefaultTermMonths` (nullable), rebuild
  `IX_Elections_ECPeriodId_Live` on period and kind. Rollback refuses while admin-made end reasons
  exist (raise in a DO block).
- Tests: model on SQLite; migration rule 5; rollback refusal on PostgreSQL.
- Done when: N1 built; 95.2 stays PARTIAL until G-REL confirms preprod and the invalid-rows report.

**S46 · 95.4 · M** One vacant-seat query in `GovernanceService` (holders in the active period below
the position's seat count); filter on the web governance page and the mobile governance screen.
Tests: ended term appears, filled seat leaves. Done when: 95.4 DONE.

**S47 · 95.12 (backend) · M · security** `TermEndSweep` like `DataRetentionSweep`, off by default
(`OrgConfig` governance setting); default term length fills a new period's end date; end-reason
admin routes; system reasons cannot be removed; `end-term` takes a reason id and a note.
Tests: on and off, early end with each reason, system reason delete refused.

**S48 · 95.12 (clients) · M** Web end-reason admin next to positions, reason picker on web and
mobile end-term. Done when: 95.12 DONE.

**S49 · 95.5 (backend) · M** Kind on create; a by-election offers vacant seats only and refuses a held
seat at create and at declare; declare never ends a holder (owner 2026-10-14, the older tracker text
about ending a holder is superseded); `GetCurrentAsync` returns the general election and any live
by-election. Tests: declare fills only the vacant seats, guards, two live elections.

**S50 · 95.5 (clients) · M** Kind shown on ballot, results and ER forms; both live elections on web
and mobile; admin create on web only. Done when: 95.5 DONE.

**S51 · 37.13f · S** Close the 11 *[to confirm]* points in doc 09 against free Neon and free Render
public docs (link and date for each); mark availability as a known limit with no SLA; backup schedule
with RTO and RPO; branch warning in `docs/DEPLOYMENT_CHECKLIST.md`; a log line for a failed count with
a test. Points that need the owner's dashboard values name the owner as the source still owed.

### Phase F. P2 items

**S52 · 84.4 · S** Parity table in spec 004 (or `docs/API_CONTRACT_REGISTRY.md` appendix): for each
endpoint in the spec 004 contract table, grep `GHCAA.Web/src` and `GHCAA.Mobile/lib` for the route; columns web,
mobile, response difference. Script it in `scripts/` if it takes more than one pass.

**S53 · 93.3 · S** Find why `registerNewMemberViaUi` gets "Registration failed" on the local API (read
`api_error.log` tail and the registration response body; the membership walkthrough records the
API's answer). Fix the cause (seed, OrgConfig or test data), run `portal-mobile-nav.spec.ts` and
`gallery.spec.ts`.

**S54 · 84.27 (API tests) · S** `ArchiveControllerTests` for each route: 200, 401, 403, 404.

**S55 · 84.27 (web page) · M** `admin/archive` moderation page with the shared admin components and
an `ADMIN_ARCHIVE` endpoint block. Vitest. Done when: 84.27 DONE.

**S56 · 84.55 · M** `CreateGalleryDto` with caller fields only, mapped in the service; test that
extra fields are ignored; web and mobile callers checked; registry.

**S57 · 84.93 · M** `POST api/admin/governance/constitution` (AdminOnly policy) with
version, text, PDF, effective date, change summary; on the effective date the new one becomes active
and the old gets `SupersededDate` (computed on read, so no job); public page shows the version in
force. Tests: future date, two on the same date (refused). Web admin screen. Unblocks Secretary
Task 5 (78.9).

**S58 · 84.97 · M** Status change to Terminated or InactiveResigned requires a reason; write
`MembershipHistory` (from, to, admin, reason); admin member page shows the history. Tests: missing
reason 400, history row. Walkthrough: membership walkthrough terminates the new member at the
end (missing reason refused, history shown).

**S59 · 84.67 · M** Mobile Change Password screen under settings calling the web route, step-up
dialog, `serverMessage`. Widget tests: success, wrong current password, step-up cancel.

**S60 · 34.D8 · M** Fix the local admin role through the protected SuperAdmin seeder path (memory
`feedback_protected_superadmin_pattern`), not SQL. Then the browser walk on the tracker list,
including a member refused when creating a Notice. Record each step.

**S61 · 84.65 · M** Web `admin/scholarships` page on the existing routes; list, create, edit, close,
review applications. Vitest; spec 015.

**S62 · 49.3.C · L · migration 10** `User.ContactEmail` (nullable, unique when set), migration
`AddUserContactEmail`; SuperAdmin sets it on the roles page; reset for a no-email account sends there
when set, else the current copy and share flow. Tests on both paths.

**S63 · 97.1 (backend) · M** `OtpPurpose.EmailVerify` appended; profile routes to send and verify;
changing the email clears `EmailVerified`; reuse the OTP service and its lock. Tests: OTP path, lock,
clear on change, social link refused while unverified.

**S64 · 97.1 (clients) · M** Badge and Verify action on web and mobile profile. Walkthrough: membership
walkthrough shows the badge after the first login and verifies by code. Done when: 97.1 DONE.

**S65 · 84.83 (backend) · L · migration 11** `Member.IsMentor` and `Member.MentorTopics` (one row per
person, so not on `ProfessionalRecord`); migration `AddMentorOptIn`; requests only to
opted-in members; directory filter. Tests incl. opting out with a request pending (pending stays,
new ones refused).

**S66 · 84.83 (clients) · M** Opt-in setting and directory filter on web and mobile.

**S67 · 84.82 (backend) · L · migration 12** `EventFeedback` (event, member, rating 1-5, comment,
created at; unique event and member), migration `AddEventFeedback`; only approved attendees after the
event starts; admin summary. Tests: non-attendee, repeat.

**S68 · 84.82 (clients) · M** Rating form on web and mobile; admin summary on web.

**S69 · 84.72 (backend) · L · migration 13** Admin forum routes (categories CRUD, order, retire;
pin, lock, remove with reason to the audit log, reusing spec 028 moderation); migration
`SeedForumCategories` with a few defaults (data step). Tests.

**S70 · 84.72 (web) · M** Admin forum section. **S71 · 84.72 (mobile) · M** Same tools behind a role
guard (memory `gotcha_mobile_admin_routes_no_role_guard`). Done when: 84.72 DONE; spec 017 updated.

**S72 · 92.1 · S** Write spec 022 decisions with the owner: settings in an OrgConfig `Approvals`
section (recommended) or a table; "off" means created approved with an audit row (recommended);
SuperAdmin always approves (yes, protected rule); member import stays SuperAdmin and Admin.

**S73 · 92.2 · L** Per spec 022. If the owner chose a table, it takes ledger row 14. Policy handler
reading the flow's roles; unknown flow refused and logged; `MemberApprovalMode` mapped. Tests per
the tracker; reflection tests updated.

**S74 · 92.3 · M** Web settings screen and queue filtering. **S75 · 92.4, 92.5 · S** Mobile gating
with widget tests; FR and NFR entries, spec 002 tables, `business_flow` notes, `PROJECT_MAP.md`.
The book part of 92.5 (§8.4, the role matrix, and its figure) goes to H00.

**S76 · 37.7 (web mechanism) · M** `I18nService`, `en.ts`, `bn.ts`, `TranslatePipe`, toggle beside
the theme toggle, self-hosted Bengali font, `<html lang>`; key-drift test. Admin stays English.

**S77 · 37.7 (web strings) · M** Public site and portal nav, buttons, labels, validation messages.
Bengali strings are drafts; 37.7 stays PARTIAL until gate G-BN.

**S78 · 8.8 · M** Mobile: same keys in `app_localizations.dart` (flat dictionary, no ARB). 8.8 stays
PARTIAL until G-BN.

**S79 · 62.2 · M** Write the missing profile files for `profiles/default/` and `profiles/ghc/`:
`email_templates.json`, `site_content.json`, `themes.json`, `membership-tiers.json`,
`governance.json`, and the GHC `constitution.json`; match the shapes the seeders read. Tests: the
profile pack loader test per file.

**S80 · 51.4 · S** Add the excluded-type matrix (`PaymentProof`, `Certificate`, `Signature`,
`NoticeDocument` not compressed) and the non-image fallback to `LocalFileStorageServiceTests`.

**S81 · 27.1, 27.8 · M** Script the low-coverage report from `coverage.cobertura.xml` (backend), the
Vitest V8 summary and Flutter LCOV. Question to the owner first: enforce 80 % per file now (fails
today) or a ratchet (no file drops below its current figure, new files at least 80 %)
(recommended). Wire the chosen gate into `ci-local.py`.

**S82 · 73.5 · M** Extend `traceability.py` to generate the whole matrix from the FR and NFR tags.
The book claim it enables goes to H00.

**S83 · 73.6 · S** Test pinning the seeded constitution version and a SHA-256 of its clause text.

### Phase G. P3 and P4 items

**S84 · 84.57, 84.62 · S** Named upload limits in `Constants.Uploads` used at every call site listed in
84.57; a per-user `Assistant` rate-limit policy (default 20 per hour from OrgConfig) with a 429 test,
checked against the global limiter.

**S85 · 84.64, 93.4 · S** Mobile "mark all read" on the notifications screen (widget test). 93.4:
ask the owner which of `/communications`, `/legacy`, `/professionals` stay (recommended: add the
ones with a working screen to the drawer, remove the rest), and gate drawer entries on the OrgConfig
flags.

**S86 · 84.69 · M** Mobile site content list and detail from the public route; widget tests and a
phone-viewport check.

**S87 · 84.23 · M** Question to the owner: mobile signature upload (recommended yes, `POST
api/profile/signature` exists) and the unused PDF routes (recommended: mobile downloads
`id-card/pdf` and `certificate/pdf` and drops its own PDF builder, which also fixes Bengali names).

**S88 · 96.3 · M** Messages page header and composer, directory search input, campaign form inputs
onto the shared components.

**S89 · 84.90 · M** Owner question: reuse the newest live code per type (recommended) or keep minting.
Build the chosen rule; spec 013 FR-010.

**S90 · 84.95 · S** Applied date and days waiting on the approval list, past-limit mark, both
clients; tests for day 30 and 31; membership walkthrough checks the applied date and days waiting
in the queue. Runs after the 78.9 sessions unless the owner says first.

**S91 · 84.94 · M** Arrears report with CSV export; tests for no dues, part payment, waived due.
Same timing rule as S90.

**S92 · 84.66 · M** OrgConfig cap on pending archive submissions (default 5); API refusal message;
both clients show it; golden snapshot.

**S93 · 51.5 · M** OrgConfig `fileStorage` section read by the storage service with `Constants`
fallback; admin settings tab.

**S94 · 84.98 · M** The seven theme gaps on the tracker; color logic tests; browser check both modes
with a light custom primary.

**S95 · 90.1, 90.2 · L · migration 15** `SavedReport` with `CreatedByUserId` (spec 010 wins over the
tracker's `CreatedBy`), migration `AddSavedReports`; `ReportService` with field whitelist and row cap.

**S96 · 90.3, 90.5 · M** Controller and tests. **S97 · 90.4 · M** Web page on the shared grid from
S18.

**S99 to S103 · 37.4, 37.5, 37.10** Build as the tracker items describe; migrations 16 and 17 for
cohorts and memorial; 37.10 after 37.4.

**S104 to S107 · 62.56** Spec 025 tasks T001 to T036 in four parts: section map and key uniqueness
(a, g); export (b); upload, mapping and preview (c, d); modes, transaction, tests and docs (e, f).

**S108 · 60.3, 6.2, 8.5, 8.7, 37.9 · S** One owner question round, each with a recommended default:
60.3 read both clients and close as a naming difference if they hit the same routes; 6.2 fold into
84.83 (mentor and referral) or drop; 8.5 drop unless a measured slowness exists; 8.7 drop with 8.5;
37.9 keep at P4 for after 37.4. Record each answer; build cards are added only for a "build" answer.

**S109 · 37.11 · S** Refresh the WP37 baselines from a full run and update `FEATURES.md`,
`PROJECT_MAP.md`, `SRS.md` and `ARCHITECTURE.md`. Close 37.11 when every other WP37 item is DONE or
WONTDO; if gate items remain (G-DEV, G-BN), set it PARTIAL naming them.

### Track H. Book, on the owner's word

H00 keeps the list of book facts changed by feature sessions. It starts with: 92.5 role matrix
(§8.4, the role matrix, and its figure), 73.5 matrix claim (§3.9 and §12.2), every repository count.

H01 78.10 (coverage summary, static analysis, latency, ASVS walkthrough, each with its command and
date). H02 63.8 and 84.35 (re-take every count `wbs.py` quotes). H03 84.96 (owner restates the DC-15
claim) and 64.8. H04 64.9 and 67.3 (diagrams). H05 to H07 67.2 placeholders chapter by chapter, and
67.4. H08 67.7 prose joins and 67.5 page tightening. H09 63.34, 63.18 (only if asked), 74.3. Each H
card ends with `python docs/book/build/build.py --pdf --strict` clean.

## 5. Owner gate cards

Engineering does not start these. When the gate clears, the "Then" step becomes a card at the top of
the next phase.

| Gate | Items | Owner step | Then |
|---|---|---|---|
| G-REL | 37.14m, 95.2 | Deploy the built migrations to preprod in ledger order (rows 1 and 2, later 5, 6, 9) and read `__EFMigrationsHistory`; run `GET api/admin/governance/positions/invalid-rows`. | Close 37.14m; 95.2 closes after N1 is released too. |
| G-MAIL | 7.26 | A sign-in code from preprod reaches an inbox; `EmailLogs` row Sent. | Close 7.26. |
| G-UPL | 7.28 | Upload a photo on preprod, redeploy, photo still loads. | Close 7.28. |
| G-DEV | 37.1m, 37.14o, 95.6, 84.49 (device part); 84.50 and 84.42 held | Phone and Windows desktop checks in `docs/AUTH_AND_DEVICE_FIX_PLAN.md` Part A. | Close each with the device, OS and date. |
| G-SEED | 84.7, then 62.41, then 62.42 | Answer gates G1 to G4 in `docs/SEED_PROFILE_PLAN.md` section 8. | Cards for the seed plan, then 62.41 on a throwaway database, then 62.42. |
| G-EVAL | 78.9 | Run the sessions after G-MAIL, G-UPL, S11 and S57. | Track H uses the responses. |
| G-A11Y | 37.13g | A person with NVDA and TalkBack runs doc 10, after S50. | Fix cards for each failed row. |
| G-BN | 37.7, 8.8 | A Bengali speaker signs off the strings. | Close both. |
| G-PROP | 84.39 | Approve the drafted proposal text. | Paste it; close. |
| G-CFG | 98.43 | Decide `ConnectionStrings:MySqlConnection` and the DGePay `ClientId`, `ClientSecret`, `ApiKey`. | Close with the owner's edit. |
| G-TOOLS | 99.8 | Decide each point on the item (dead tools, credential files, the CSV, spec 006). | Card for the deletions; book part to H00. |
| G-PIN | 7.16 | Production certificate fingerprint. | Add the pin and flip the test. |
| G-PERF | 98.7 | A measurement on the deployed site. | Build or close with the number. |

## 6. On hold

From `python scripts/backlog_inventory.py --state ONHOLD`. Never placed in a session.

| Items | Waiting for |
|---|---|
| 91.3, 91.4, 91.5, 91.6 | A second host or a second consuming app. |
| 98.34 | A fixed undici that works with the Angular toolchain. |
| 82.31 | The data-protection decision (also G2 of the seed plan). |
| 7.21 | The domain. |
| 72.4 | The first Dependabot pull requests. |
| 84.42, 84.50 | The owner's device run (G-DEV). |
| 84.48 | The preprod checks (G-MAIL, G-UPL). |
| 84.61 | The Firebase service key. S14 sends email only until then. |

## 7. Migration ledger

One at a time, in this order. Each takes a timestamp later than the row above.

| Row | Migration | Card | State on 2026-11-13 |
|---|---|---|---|
| 1 | `20261021100000_AddEcPositions`, `20261021110000_RetireEcPositionLookups`, `20261021120000_AddElectionFormFields` | built | Local PostgreSQL checked; preprod at G-REL |
| 2 | `20261112120000_AddElectionFormPrints` | built | Local PostgreSQL checked; preprod at G-REL |
| 3 | `AddReminderSentMarks` | S14 | |
| 4 | `AddMembershipNumberUniqueIndex` | S15 | |
| 5 | `AddNominationPhotoApproval` | S22 | |
| 6 | `ElectionF2` | S24 | |
| 7 | `AddMeetings` | S34 | |
| 8 | `AddMemberLocation` | S43 | |
| 9 | `AddCommitteePositionsAndTerms` | S45 | |
| 10 | `AddUserContactEmail` | S62 | |
| 11 | `AddMentorOptIn` | S65 | |
| 12 | `AddEventFeedback` | S67 | |
| 13 | `SeedForumCategories` | S69 | |
| 14 | approvals table, only if chosen in S72 | S73 | |
| 15 | `AddSavedReports` | S95 | |
| 16, 17 | cohorts and reunions; memorial | S99, S101 | |

## 8. Plan review, 2026-11-13

<!-- backlog-check: off -->

The open items, the old stream E order, the spec 023 delivery plan and the code were read against
each other before the cards were written. Each line is a fault found and what the cards do instead.

| # | Fault | Evidence | Fixed in |
|---|---|---|---|
| R1 | The 84.99 fix (`PERFORM setval`) would break the plain `Migrate()` run, since `PERFORM` is PL/pgSQL only. | PostgreSQL grammar; `Migrate()` runs each statement outside a DO block. | S02 uses `DO $sv$ BEGIN PERFORM setval(...); END $sv$;`; tracker 84.99 corrected. |
| R2 | The delivery plan's F2 list adds `RecountWindowEndsOn`, `ConsentAt` and a recount request table, all built under 37.14m on 2026-10-21. A second copy would fail the migration or duplicate data. | `GHCAA.Domain/Models/Election.cs`: `Election.RecountWindowEndsOn`, `Nomination.ConsentAt`, `ElectionRecountRequest`, `ElectionRecountFigure`. | S24 lists what exists and adds only the rest; S26 reuses the request table. Delivery plan F2 carries a pointer. |
| R3 | 37.1p asks for a recount request "unique on election and seat"; 37.14m built one row per filing. | Same file. | S26 keeps one row per filing and allows one undecided request per seat. |
| R4 | Session 5b had no migration but 37.14f needs a purge record that outlives the purged rows. | 37.14f acceptance. | `ElectionPurgeRecord` joins the F2 migration (S24). |
| R5 | Several items say "SQLite and PostgreSQL migrations"; the repo has only PgSql migrations and SQLite builds from the model. | `GHCAA.Infrastructure/Data/Migrations/` holds only `PgSql/`; `MigrationBootstrapper.cs`. | Rule 5; tracker 84.84, 92.2 and 95.2 corrected. |
| R6 | 84.25 asks for a FluentValidation validator, which never runs; both parts are already covered. | `MentorshipControllerTests.cs` exists; `CreateJobDto` has DataAnnotations. | S08 verifies and closes; tracker text corrected. |
| R7 | 84.59 was IN PROGRESS with no code; it waits on 84.19. | The four interfaces still return `IEnumerable<object>`. | Status set to TODO; S42 after the meetings work. |
| R8 | 84.19 shows IN PROGRESS but only the spec task (T001) is done. | Spec 026 `tasks.md`: T002 to T021 open. | Kept IN PROGRESS (spec approved); S34 to S41 carry the rest. |
| R9 | 28.33 duplicates 8.8; the flat dictionary decision was taken with 37.7. | Tracker 28.33 conflict note; project memory. | 28.33 set WONTDO; S78 covers 8.8. |
| R10 | 95.5 text still says declare ends the current holder; the 2026-10-14 decision says it never does. | 95.5 plan note. | S49 follows the decision. |
| R11 | 90.1 names `CreatedBy`; spec 010 names `CreatedByUserId`. | `docs/specs/010-ad-hoc-reporting/spec.md:105`. | S95 uses `CreatedByUserId`. |
| R12 | 12.9 lists "mobile posts `question`" as a bug; mobile posts `query`. | `assistant_service.dart:14`. | S98 fixes the spec text only. |
| R13 | 37.16i left mobile revoke open. Mobile has no official form screens; admin steps live on the web. | 37.14n text; the election client split recorded in spec 011. | S05 builds web only and records it. |
| R14 | Section 1 of the old plan was dated 2026-11-02 and its wave 2 list predates session 2b. | Old sections 1 and 4. | Replaced by sections 1 and 3. |
| R15 | 84.81 needs a mark so a reminder goes once per member; no column held one. | `MembershipDue.cs`, `EventRegistration`. | Migration row 3 (S14). |
| R16 | Preprod applies every pending migration on deploy (`Migrate()` at start-up), so "release one migration" means "deploy the code that holds it". | `MigrationBootstrapper.cs`. | G-REL asks for one deploy and a read of `__EFMigrationsHistory`. |
| R17 | 37.14u, 37.14v, 37.14w and 37.14x were opened on 2026-11-12 under IDs that closed items in `TODO_ARCHIVE.md` already hold. The parser reads `TODO.md` first, so the closed ones were hidden, and archiving the new ones would have put two of each in the archive. | `TODO_ARCHIVE.md` lines near 3129 to 3145. | Renumbered to 37.16i, 37.16j, 37.16k and 37.16l in the tracker, the delivery plan, spec 023 tasks and this file. 37.16j set PARTIAL to match its own update. History rows keep the old IDs. |
| R18 | The Priority Index showed 37.16j as PARTIAL while its item said TODO. | `TODO.md` index line and item line. | Item set PARTIAL 2026-11-13. |

<!-- backlog-check: on -->

## 9. Design references

These files hold the how for open items. The tracker holds status, and this plan holds the order
and the session cards. Where a design file and a card disagree, the card wins (section 8).
Each opens with a line that says so.

| File | Holds | Open items |
|---|---|---|
| `docs/AUTH_AND_DEVICE_FIX_PLAN.md` | Part A setup and check steps, B4-B5 code steps, owner decisions D1-D5 | 84.50, 84.49, 84.42, 95.6 |
| `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md` | Session rules, F and N session build notes, what stays with a person | Stream E |
| `docs/specs/023-election-ballot-secrecy-and-standards/plan-officials.md` and `plan.md` | Officials design, Phase 3 rules | 37.1p, 37.16e, 37.13f, 37.13g |
| `docs/WHITE_LABEL_PLAN.md` | ADR-1 to ADR-5, phases A to F, verification | 62.2, 62.41, 62.42 |
| `docs/specs/025-org-config-pack-export-import/plan.md` | Export and import shape | 62.56 |
| `docs/specs/022-configurable-approval-workflows/plan.md` | Approval workflow steps | 92.1 to 92.5 |
| `docs/specs/008-idempotent-member-batch-import/plan.md` | Import gaps and checks | 88.1 to 88.5 |
| `docs/specs/004-pending-work-packages/plan.md` | Per-item slices and dependency reasons | Streams N2 to N8 |
| `docs/specs/009-campaigns-scholarships-archive/plan.md` | WP37 history | 84.27 |
| `docs/specs/024-performance/plan.md` | Performance phases | 98.7, 98.43 |
| `docs/TEST_COVERAGE_PLAN.md` | Coverage targets and method | 27.1, 27.8 |

Closed records, not scheduled: `archive/BUSINESS_REVIEW_PLAN.md` (audit snapshot of 2026-07-03),
`FORUM_PLAN_2026-05.md`, `archive/UI_FIX_PLAN.md`, `implementation_plan.md`, and the plans of specs 001,
005, 007 and 024's built phases. Spec 006 (Agentra) is a draft with no tracker item; 99.8 holds it.

## 10. Test strategy and automation

### Test strategy

| Layer | Command | Notes |
|---|---|---|
| API | `python scripts/ci-local.py --only backend` (dotnet build and NUnit) | Moq and FluentAssertions. Tag new tests with their FR or NFR. |
| Web | `python scripts/ci-local.py --only web` (Vitest, lint, build) | Use `componentRef.setInput` for signal inputs in specs. |
| Mobile | `python scripts/ci-local.py --only mobile` (flutter analyze and test) | Wrap widget tests in `ProviderScope`. Goldens follow shared assets. |
| Contract | `python scripts/ci-local.py --only contract` | Any route or DTO change also goes to `docs/API_CONTRACT_REGISTRY.md`. |
| E2E | `python scripts/ci-local.py --e2e` | Playwright, against a local API. |
| Schema | A dry run against a non-empty local PostgreSQL | Needed for every migration. |
| Book | `python docs/book/build/build.py --pdf --strict` | Must end with `status : clean, ready to deliver`. |
| Scripts | `python -m unittest discover -s scripts -p "test_*.py"` | Covers `backlog_inventory.py`. |

What counts as evidence:

- A passing test shows the mechanism works. It does not show the effect in real use. Device,
  preprod and participant checks stay owner gates.
- An item is DONE only with a command and its result in the tracker entry.
- A shared API or business rule change covers backend, web and mobile in the same change.

### Automation

| Script | When to run it |
|---|---|
| `scripts/backlog_inventory.py` | At the start of a session, and `--check` after any tracker status change. |
| `scripts/ci-local.py` | `--only <layer>` once per layer a wave touches; the full run once at the end of a wave, before handing it to the owner. `--quick` for a fast format and type check. |
| `scripts/archive_done_todos.py` | When DONE items pile up in `TODO.md`. |
| `scripts/verify-election.py` | After election changes, before a preprod release. |
| `scripts/visual-check.ps1` | After a web UI change, for screenshots. |
| `docs/book/build/tracker_page.py` | Run by the book build. Its parser is also used by `backlog_inventory.py`. |
| `docs/book/build/wbs.py --check` | After a work package is added or renumbered. |

## 11. Cost rules, quality floor and item brief

These rules were written for waves; read "wave" as "phase" of section 3. Rule 7 is narrowed by the
owner's instruction of 2026-11-06: work inline, no subagents, except the review pass of rule 7 in
section 2.

### Cost and time rules

Every wave follows these rules. They cut test runs, rework and owner round trips, which is where
most of the time goes.

1. **Check before building.** Run `graphify query` on the item first. If the code already does what
   the item asks, close it with the evidence command instead of writing it again.
2. **Cheapest first.** Order a wave docs-only, then test-only, then one-layer code, then cross-layer.
   Small items close early and a wave that runs short still ships something.
3. **Size each item.** S is one file or one test file, M is one layer, L crosses layers or needs a
   migration. An L item gets its own wave, or is split, so it does not hold up the small ones.
4. **Smallest test while working, layer suite before handback.** While working an item, run only its
   test file (`dotnet test --filter`, one Vitest file, one flutter test file). Before handing the
   item back, run the unit suite of each layer it touched, because an older test can still expect
   the old behaviour. Run the full `ci-local.py` (and `--e2e` only when a wave has e2e work) once at
   the end of the wave.
5. **Reuse first.** Use the shared component, service, helper or script that already exists. No new
   abstraction or dependency unless two places need it now.
6. **Review before handback.** An M or L item gets one code-reviewer pass on its own diff before it
   is reported done, and its findings are fixed then. The wave still gets one review at the end.
   Fixing a finding before handback costs one pass; fixing it after costs a second round with the owner.
7. **Cheaper model for mechanical work.** Doc tables, renames, golden refreshes and test boilerplate
   run on Sonnet or Haiku subagents. Opus is kept for cross-layer design and security review.
8. **Batch the owner's work.** Collect every owner step a wave raises (visual checks, preprod checks,
   decisions) into one list in the wave report, so the owner does them in one sitting.
9. **Book figures once per wave, on request.** Repository counts quoted in the book drift with every
   tracker edit. Re-sync them once, when the owner asks, not after each item.
10. **Park, do not stretch.** If an item grows past its size, mark it PARTIAL with the step that is
    left and move on. The next wave picks it up.

### Quality floor

The rules above save time on how work is run. They never lower what an item must meet. These hold
for every item, whatever it costs:

- A failing test is written and seen to fail before the fix, and passes after it.
- A shared API or business rule change covers backend, web and mobile in the same item, with its
  tests and its `docs/API_CONTRACT_REGISTRY.md` entry.
- A migration gets a dry run against a non-empty local PostgreSQL.
- The full `ci-local.py` run is green at the end of the wave, or each failure is named with its
  cause. No check is silenced or skipped to get a green run.
- The wave's code review runs on the most capable model, and every confirmed Critical or Important
  finding is fixed with a test before handover.
- An item is DONE only with its evidence command and result in the tracker. Anything left is
  PARTIAL with the step named.
- A cheaper model's work is checked by the same tests and the same review as any other.

<!-- backlog-check: off -->

Added after the wave 1 review on 2026-11-01. Each line is a mistake that review found:

- Code moved from one class to another gets a test for every filter it applies (case, trimming,
  privacy, active state, the caller, the result cap), and is compared with the same method in a
  sibling class. The 84.21 merge lost the case-insensitive search that way.
- A checker reports any input it cannot read. A count that drops a line it cannot parse is wrong
  without saying so; that is how seven items fell out of every count.
- Tracker statuses use only the parser's words: TODO, PARTIAL, IN PROGRESS, ONHOLD, BLOCKED,
  DONE, WONTDO. The archive holds only DONE and WONTDO; `archive_done_todos.py` moves the rest back.
- A tracker count counts each ID once and skips the Priority Index. Headings come in two cases
  ("WORK PACKAGE 7:" and "Work Package 40"), so a heading test covers both.
- Every gate the plan names has a tracker item, so `--check` can see it.
- The plan states its order once, and the handover, wave table and size column agree with it.
- When an item closes, every place that shows its status changes in the same edit: the tracker,
  spec task boxes and plan lists. Then only those places are checked, and the code is read again
  where they disagree. Standing rules carry no checkbox, since they never close.

This list is closed. A finding from a later review does not get a line here. It goes to the
cheapest place that stops the same kind of issue without anyone remembering it:

1. **A check that runs by itself**, when a pattern can be detected in the code. Add one line to
   `RULES` in `scripts/guard.py` and a case to `scripts/test_guard.py`. The guard reads only lines
   added since HEAD, so old code never blocks it, and it runs as the first `ci-local.py --quick`
   step. Examples: a literal date format in a template, a migration conflict with no target.
2. **A path-scoped rule** in `.claude/rules/`, when it needs judgement but applies only to some
   files. It loads when such a file is opened, for example zoneless signal rules for web components
   or the rerun and rollback rules for migrations.
3. **A question in the item brief**, when it is about scope or decisions and not about one kind of
   file.

The fix for a review finding is done only when the finding is fixed and it has been routed to one
of these three places.

<!-- backlog-check: on -->

If a cost rule and the floor disagree, the floor wins.

### Item brief (before code)

An M or L item gets a short brief before any code. It goes in the tracker entry, a few lines long.
It makes the build cheaper and leaves the review little to find, because questions are asked once
and every place the change reaches is known up front.

1. **Impact map.** One `graphify query` per changed value, type or route. List every reader: API,
   web, mobile, seeds, migrations, specs, golden files and docs. Anything the item changes that is
   missing from the list is the cause of later scope.
2. **Decisions first.** Every choice that changes the result, such as wording, source of truth,
   fallback and migration shape, goes to the owner in one question round before code. Nothing is
   decided halfway through.
3. **Break cases.** For each changed value, write down what breaks it: a late or async value (who
   reads it, and when), partial input to a merge or fallback, null, empty, negative or out of range
   input, a rerun of a migration and its rollback. Each one becomes a test.
4. **Test list.** Name the test files to add and the existing tests that expect the old behaviour.
5. **Rules check.** Name the rules that apply, such as config over constants, no seed JSON edits,
   theme tokens and comment tone, so they are followed while writing and not found later.

The brief is the exploration. Coding then reads only the files it names, which keeps token use low.
Anything the brief did not foresee is a gap in the brief: note it in the list below, so the next
brief asks for it.

<!-- backlog-check: off -->

## 12. Progress log

| Date | ID | Evidence | Files |
|---|---|---|---|
| 2026-11-01 | 99.1 | `python -m unittest discover -s scripts -p test_backlog_inventory.py`: 7 of 7 OK | `scripts/backlog_inventory.py`, `scripts/test_backlog_inventory.py` |
| 2026-11-01 | 7.22 | grep shows `https://ghcaa-ryl6.onrender.com` in all four preprod settings | `GHCAA.Mobile/.env.preprod`, `docs/TODO.md` |
| 2026-11-01 | 84.21 | `dotnet test --filter "FamilyLinkControllerTests|MemberSearchOrderTests"`: 23 of 23, after the review fix that restored the case-insensitive name search | `FamilyLinkController.cs`, `IFamilyLinkService.cs`, `FamilyLinkService.cs`, two deleted files, two test files, `PROJECT_MAP.md`, book chapter 06, spec 013 |
| 2026-11-01 | 84.31 | grep finds `010-election-engine-fixes` only in dated delivery-plan rows | `docs/specs/027-election-engine-fixes/`, specs 001, 011, 018, 023, `docs/TODO.md` |
| 2026-11-01 | 90.4 | Text only; spec 010 already matched | `docs/TODO.md` |
| 2026-11-01 | 99.2 | `backlog_inventory.py --check docs/MASTER_EXECUTION_PLAN.md`: 0 problems | This file, old plan pointers, `docs/TODO.md` |
| 2026-11-01 | 99.4 | `test_tracker_page.py` 12 of 12; tracker page and `backlog_inventory.py` give the same open count | `docs/TODO.md`, `docs/TODO_ARCHIVE.md`, `tracker_page.py`, `test_tracker_page.py`, `archive_done_todos.py`, `test_archive_done_todos.py` |
| 2026-11-01 | 99.6 | `ci-local.py --only backend`: 6 of 6 steps; spec box scan finds no ticked box for an open item and no open box whose items are all closed | Specs 001, 003, 004, 005, 012, 013, 020, 022, 026; `BACKLOG_EXECUTION_PLAN.md`, `AUTH_AND_DEVICE_FIX_PLAN.md`, this file |
| 2026-11-01 | 99.5 | `git ls-files` lists neither `.env.dev` file and `git check-ignore --no-index` matches both; a rerun of the key fill finds no key missing within a tier. Owner chose no rotation | `.gitignore`, `.env.dev.example`, `docs/TODO.md`, this file; ignored env files changed locally only |
| 2026-11-02 | 99.2 | Owner accepted the plan | `docs/TODO.md` |
| 2026-11-02 | 72.5, 75.5, 76.4, 77.4, 77.11 | Owner answers: the repository stays private; the P1 duration, the factors and the 18.5% rate stand as written | `docs/TODO.md`, `docs/book/build/wbs.py` |
| 2026-11-02 | 7.21, 72.4 | Put on hold: no domain yet; no Dependabot pull requests yet | `docs/TODO.md`, this file |
| 2026-11-02 | 84.46 | Handbook text read against `BALLOT_KEY_BITS` in `ballot-key.util.ts` | `docs/Elections/12-Election-Officials-Handbook.md` section 5 |
| 2026-11-02 | 98.43 (part) | Each new key name grepped in the code it configures; values not checked | `.env.dev.example`: commented keys added. Removing unused appsettings keys waits for the owner |
| 2026-11-02 | 62.6b | `dotnet test GHCAA.Tests`: 1873 of 1873; golden snapshot unchanged | `OrgConfigService.cs`, `InstitutionProfileProvider.cs`, `IInstitutionProfileProvider.cs`, `Program.cs`, `ci` swagger step, `TestProfiles.cs`, OrgConfig and integration tests, specs 020 and 025, `CONFIG_DRIVEN_FRAMEWORK.md`, `WHITE_LABEL_PLAN.md` |
| 2026-11-02 | 12.8 (part) | `git ls-files` no longer lists `docs/appsettings.txt` or `docs/dgepay_info.txt`; both kept on disk and ignored | `.gitignore` |
| 2026-11-03 | 37.16f-h, 84.54-84.64 (added) | Spec review against the tracker; each gap checked in the code at the file and line the item names. 90.2 and 92.2 acceptance extended | `docs/TODO.md`, this file, specs listed in the review |
| 2026-11-03 | 84.65-84.69 (added) | Owner answers to the six review questions; election screen split and accepted refactors recorded in the specs | `docs/TODO.md`, this file, specs 013, 014, 015, 018, 023 |
| 2026-11-03 | 99.7, 99.8 (added) | Scripts and plans audited against the code; `--check` 0 problems; script tests 18 and authz tests 12 pass | Fixed scripts and plans listed in 99.7; two backlog plans and nine dead scripts deleted |
| 2026-11-03 | 99.9 (added, closed) | Owner: this file is the only open plan. All 102 open items were already named here; the open order of the auth plan, the spec 023 delivery plan and the white-label plan moved into section 3 | This file, the other plan files' opening lines, `CLAUDE.md`, `AGENTS.md`, `GEMINI.md`, `.github/copilot-instructions.md` |
| 2026-11-05 | 84.74-84.76 (added) | Owner set the three gaps left open by 84.73 as P1; each checked in the code at the file and line the item names | `docs/TODO.md`, this file |
| 2026-11-05 | 84.54, 84.74, 84.75, 84.76 (closed) | `event_gateway_sheet_test.dart` 5 of 5, `flutter analyze` clean; `nav.service.spec.ts` and `admin-layout.spec.ts` 33 of 33, type-check clean. FR-41 widened on the owner's choice | `events_screen.dart`, `financial_service.dart`, `gateway_service.dart`, `financial_portal_screen.dart`, `nav.service.ts`, `app.constants.ts`, both layouts, `admin-layout.scss`, `styles.scss`, `03-requirements.md`, specs 001 and 015 |
| 2026-11-05 | 84.77 (added) | Owner asked whether client option lists should come from the server; checked in the code the same day | `docs/TODO.md`, this file |
| 2026-11-05 | 84.77 (ordered first), 63.8, 63.34 (book last) | Owner set 84.77 next and the book strict build last, and chose the 84.77 approach | `docs/TODO.md`, this file, section 1 |
| 2026-11-05 | 84.78, 84.79 (added) | Owner asked for the constitution note and for date inputs to follow the configured format; the date gaps checked in the code at the files the item names | `docs/TODO.md`, this file |
| 2026-11-05 | 84.77 (part: web and backend) | Web type-check clean, `npm run test:unit` 986 of 986; backend migration and seed tests 27 of 28, the one failure waiting on the owner's `lookups.json` rows. Code review findings fixed the same day | `app.constants.ts`, `lookup.service.ts`, `org-config.service.ts`, register, profile, directory, admin-members, member-approval, org-config and their specs, `20261105120000_SeedJobCategoryLookups.cs`, `JobCategorySeedSyncTests.cs` |
| 2026-11-06 | 84.77 (part: mobile) | `member_labels_test.dart` 13 of 13 and `directory_filters_test.dart` 3 of 3; full suite, analyze and goldens not run because drive C: is full | `member_labels.dart`, `dropdown_service.dart`, `lookup_service.dart`, `registration_constants.dart`, `register_wizard_provider.dart`, dashboard, directory, member details, profile edit, gatekeeper and fee config screens, four visual-test fakes |
| 2026-11-06 | 84.77 (part: mobile, full run) | After disk space was freed: `flutter analyze` clean, `flutter test --exclude-tags golden` 397 of 397. Still PARTIAL for the owner's `lookups.json` rows; goldens not run | `docs/TODO.md` |
| 2026-11-06 | 84.9, 84.18 (closed); 84.80-84.84 (added) | Spec 001 is the domain-spec index with a roll-up of 55 Enhancements items counted by grep; the gap table found eight new gaps, five raised as items (priorities proposed), three left in the table | `docs/specs/001-platform-baseline/spec.md`, `docs/TODO.md` |
| 2026-11-06 | 84.73 (closed) | `nav.service.spec.ts` and `admin-layout.spec.ts` 33 of 33; browser check at 1366x900 and 390x844 showed eight sections, no page errors, and the search narrowing to one link | `docs/TODO.md` |
| 2026-11-06 | 84.7, 84.42, 84.48, 84.50, 84.61 (on hold); 84.78 (won't do) | Owner put the items that wait on device or inbox checks, the seeding plan or the Firebase key on hold, and dropped the constitution note | `docs/TODO.md` |
| 2026-11-06 | Owner order | Owner asked for 84.79 with the calendar picker bug, 84.73, 84.9, 84.18, 84.19, 84.71, 84.59, 84.63 and the partial items 84.49, 84.51, 84.68, 84.77; 84.60 gets an OrgConfig validity setting in months, default 12, empty or 0 for no expiry | `docs/TODO.md`, section 1 |
| 2026-11-06 | 84.19, 84.59, 84.60, 84.63, 84.68, 84.71, 84.79 (in progress); 84.84 (planned) | Work started on the seven items; no results yet. Owner ordered 84.84; its plan adds `City` and `Country` to `Member`, hidden when the address is private, and it starts after the 84.71 migration lands | `docs/TODO.md`, section 1 |
| 2026-11-06 | 84.79 (done) | Web date inputs use the shared `app-date-input`, which follows the configured format; the event calendar opens and keeps a picked day; specs run under both formats and a browser check passed | `docs/TODO.md`, section 1 |
| 2026-11-06 | 84.63 (done) | Controller tests added for Mentorship, Assistant, Campaigns, Scholarships, AdminErrorLogs and AdminDevTracker, plus `AuthController.GetProviders`; 59 tests pass in the filtered run | `docs/TODO.md`, section 1 |
| 2026-11-06 | 84.58 (done); 84.56 (part: server) | `dotnet test` filtered to job DTO, JobHub, SecureFiles and file validation tests: 67 of 67. 84.56 stays PARTIAL until web and mobile show the 400 message | `JobDto.cs`, `IFileValidationService.cs`, `FileValidationService.cs`, `SecureFilesController.cs`, `JobDtoValidationTests.cs`, `SecureFilesControllerTests.cs`, `docs/TODO.md`, spec 013 |
| 2026-11-06 | 84.71, 84.68 (review fixes) | Code review found four faults, now fixed: news update dropping `membersOnly`, members' news served from the output cache, poll category case, and `Other` missing uncategorised polls; reissue now runs in a serializable transaction. Filtered `dotnet test` on Poll, News, IDCard, CredentialVerification and OutputCache: 108 of 108; `ci-local.py --only contract` 3 of 3. Both items stay in progress | `NewsController.cs`, `NewsDto.cs`, `NewsService.cs`, `PollService.cs`, `IDCardService.cs`, `NewsServiceTests.cs`, `PollServiceTests.cs`, `docs/API_CONTRACT_REGISTRY.md`, `docs/TODO.md` |
| 2026-11-07 | 84.56, 84.71, 84.77, 34.D7 (closed); 84.85 (added and closed); 84.86 (added and closed, P1: unpublished news by Id now 404 except for admin, author and collaborators) | Owner asked why small items stayed open and had them closed. 84.56: web and mobile show the server's 400 message in the same order; mobile job link sent as `applicationLink`. 84.71: `DiscussionCategory` rows in `lookups.json`, poll-link test, T013. 84.77: JobCategory rows, golden fixtures use `Founding`, four dashboard goldens regenerated. 84.85: dates swept to the shared helpers on both clients, forum pages included, with guard tests. Review fixes: mobile reads list-shaped `errors`, `postJob` catches every error, guards also catch `substring(0, 10)` and `Intl.DateTimeFormat`. `ci-local.py --only` backend 7 of 7, contract 3 of 3, web 4 of 4, mobile 2 of 2; `flutter test --tags golden` 65 of 65 | `http.util.ts`, `global-http.interceptor.ts`, `api_exception.dart`, `job_service.dart`, `jobs_screen.dart`, `lookups.json`, date screens on both clients, `date-display-guard.spec.ts`, `date_display_guard_test.dart`, four visual tests and goldens, `NewsServiceTests.cs`, `PollServiceTests.cs`, `JobCategorySeedSyncTests.cs`, `docs/PROJECT_MAP.md`, `docs/TODO.md`, spec 028 tasks |
| 2026-11-07 | 84.87, 84.88 (added); 84.88 (closed); 84.89 (added) | Theme review: security review found no vulnerabilities. Code review found a gradient contrast false pass in the web admin preview and low contrast for mobile buttons using the offline fallback accent. 84.88's branding inheritance fix passed focused theme tests and the web checks; 84.87 remains open for the gradient fix and visual review. | `docs/TODO.md`, `docs/THEME_TOKEN_PLAN.md`, web theme files, this file |
| 2026-11-07 | 84.87 review scope clarified | Effective runtime styles can diverge from declared tokens when theme selectors override organization branding. Expanded 84.87 verification to cover default and custom branding in Light and Dark modes, actual component foreground/background/hover/focus/shadow values, and remapped utility classes in their rendered contexts. No code changed. | `docs/TODO.md`, `docs/THEME_TOKEN_PLAN.md`, this file |
| 2026-11-07 | 84.89 (closed); 84.87 (part: gradient contrast) | 84.87: the special-day check samples every rendered gradient color, ignores an inactive `sidebarColor`, and applies a suggestion only if it passes everywhere, else warns and leaves the field alone. `admin-themes` and color tests 80 passed; web type-check and guard passed. Computed-style checks, the rest of the audit and the visual review stay open; the visual review did not run because the Playwright browser tool failed to connect. 84.89: button labels pick a readable color on the accent, which stays unchanged; four focused tests, `flutter analyze` clean, four goldens regenerated for the label color only, `flutter test --tags golden` 65 of 65 | `admin-themes.ts`, `admin-themes.html`, `admin-themes.spec.ts`, `color-math.ts`, `org-config.service.ts`, `app_theme.dart`, `app_theme_contrast_test.dart`, four goldens, `docs/THEME_TOKEN_PLAN.md`, `docs/TODO.md`, this file |
| 2026-11-07 | 7.26, 7.28, 12.8, 98.43, 84.7, 78.9 (owner answers) | 7.26 and 7.28 not checked yet. 12.8 and 98.43: owner wants each list before anything moves or is removed. 84.7 off hold for the plan only. 78.9: session pack to be checked now; sessions still wait on preprod. No code changed | `docs/TODO.md`, this file |
| 2026-11-07 | 84.60, 84.68 (closed); 84.90 (added) | Credential validity months and web revoke and reissue with step-up and audit were already built; rerun: filtered `dotnet test` 72 of 72, Vitest 19 of 19 over three spec files. 84.90 tracks the minting gap spec 013 noted after 84.68 | `docs/TODO.md`, this file |
| 2026-11-07 | 98.43 (part) | Owner chose to comment out, not delete. FileStorage `WebRootPath` and both MIME lists, the Nagad and Rocket blocks and DGePay `BaseUrl` are now `//` lines with the reason. `run-app.ps1` strips comment lines before `ConvertFrom-Json`, which PowerShell 5.1 needs. `GeneralSettingsOptions` comment now names `PaymentCallbackOrchestrator` as the real reader of `SystemAdminId`. API build clean; filtered `dotnet test` (Config, Options, Startup, Payment, FileValidation) 166 passed | `GHCAA.API/appsettings.json`, `GHCAA.Tools/run-app.ps1`, `ConfigurationOptions.cs`, `docs/TODO.md`, this file |
| 2026-11-07 | 12.8 (closed) | Seven docs, the old deck and the old proposal files moved to `docs/archive/` with a README. Book chapters 03, 07, 08 and 09, the outline, ADR 0004, `BUSINESS_FINDINGS.md`, `coverlet.runsettings`, the presentation README, spec 001 evidence and this file point at the new paths; `wbs.py` keeps old and new paths so commit dates survive the move. `prose.py` 0 broken references, `wbs.py --check` and the plan check pass. The strict book build still reports three issues that predate the move: commit count 384 against 396, five table captions with no table, 93 open placeholders | `docs/archive/`, `docs/book/`, `docs/book/build/wbs.py`, `docs/TODO.md`, this file |
| 2026-11-07 | 78.9 (session pack checked); 84.92, 84.93, 84.94, 84.95, 84.96, 84.97 (added) | README and the four task scripts checked against the code, no session run. README: mobile `BASE_API_URL` step, a check that every task has a screen, a no-live-gateway check, and office-holder sessions on the web. Blocked tasks marked: Treasurer 1-2 (no client calls `update-status`), Secretary 5 (no constitution publish route). President Task 3 now matches §8.8, where the election engine is described. Gaps raised: payment verify UI, constitution publishing, arrears report, application waiting time, chapter 3 DC-15 drift, termination reason and audit. | `docs/book/instruments/`, `docs/TODO.md`, this file |
| 2026-11-07 | 84.7 (plan written) | Seeding plan in `docs/SEED_PROFILE_PLAN.md`, no code. A fresh PostgreSQL database still gets GHC's 631 members from `InitialBaseline`, because the baseline runs before the runtime seeder. Recommends stripping the data out of that applied migration (schema unchanged, live databases unaffected), moving Tier 2 content to the profile packs, content-keyed rollback, and a sequence reset after explicit-Id inserts. Also found: the Tier 3 seeder's `setval` breaks on SQLite, and the baseline never resets its sequences. `GHCAA.Tools/TestData/` is unused; delete with owner approval. Waits on owner gates G1 to G4 | `docs/SEED_PROFILE_PLAN.md`, `docs/SEED_CLASSIFICATION.md`, `docs/TODO.md`, this file |
| 2026-11-07 | 84.87 (review fixes); 84.91 (added) | Code review of commit 2b3646ff. Web: `--text-on-accent` picks the dark or white label on the configured accent; accent text, hover, focus ring and hover shadow follow the accent and sit on `<body>` so the dark theme rule cannot hide them; dark accent text is lightened to 4.5:1; `--primary-rgb` set from branding; contrast message rounds down; advice cached. Three web spec files, 87 of 87; type-check and guard pass. 84.91 tracks the mobile label colors and hex parsing. 84.87 stays in progress | `docs/TODO.md`, web theme files, this file |
| 2026-11-07 | 84.87, 84.91 (closed) | 84.87: the configured primary sits on `<body>` and is lifted for dark mode; `.on-dark` islands use their own accent token; the accent gradient narrows its spread until the label reaches 4.5:1 on every stop (owner choice). Browser check on the login page: label ratios 7.62 and 7.08 with default branding, 4.80 and 4.83 with a custom accent; one 3px focus ring in both modes. Admin table and modal were not tabbed through. Batch 5 result recorded in `docs/THEME_TOKEN_PLAN.md`. 57 color tests; web unit 127 files, 1124 tests; type-check, build and guard pass. 84.91: mobile onPrimary and onSecondary pick a readable label; hex parser takes 3, 6 or 8 digits; `flutter analyze` clean, 489 of 489; three goldens regenerated with owner approval | `color-math.ts`, `org-config.service.ts`, `styles.scss`, `public-layout.scss`, their specs, `app_theme.dart`, `dynamic_theme_service.dart`, `app_theme_contrast_test.dart`, three goldens, `docs/THEME_TOKEN_PLAN.md`, `docs/TODO.md`, this file |
| 2026-11-07 | 84.87 (second review fixes); 84.98 (added) | Code review of the close. Fixed: branding tokens clear when a color is removed, a non-hex primary or accent counts as unset, a light primary darkens the on-dark accent, the footer uses the on-dark accent, `--tier-founding` follows dark-mode accent text, ratio rounding has a float guard. Theme specs 114 of 114, `ci-local --only web` 4 of 4, `guard.py` clean, mobile contrast test 10 of 10. Left in 84.98 (stream N6, Phase 6 performance and quality): light surface check, 8-digit hex order between web and Dart, `--accent-gold-bright` with a narrowed gradient, disabled button in dark mode, fixed white text on the primary, mouse focus rings, admin table and modal keyboard check. |
| 2026-11-12 | 37.14t (built, partial); 37.14m (local check done); 84.99 (added) | Stream E session 2. 37.14t: print record table and migration `20261112120000_AddElectionFormPrints`, a record per filled print, phase check, anonymous verify route, revoke by officials holding Count and ManageSetup (owner choice) with audit, web page `/verify/form/:id`, registry entry. Code review fixes: result forms wait for Declared, the footer prints the hash, a revoke reason needs 3 visible characters. Backend filtered run 186 of 186, web specs pass. Both migrations applied twice on local PostgreSQL 18 (fresh and a copy of `GHCAADB_v2`) with a clean rollback. Left: 37.14n browser check, owner preprod release, the follow-ups listed on 37.14t. 84.99 (stream N7): the `--idempotent` script fails on bare `setval` in older migrations | `docs/TODO.md`, election API, service, DTOs, migration, web verify page, tests, `docs/API_CONTRACT_REGISTRY.md`, this file |
| 2026-11-12 | 37.14u, 37.14v, 37.14w, 37.14x, 84.100, 84.101 (added); session 2b | Session 2 leftovers and the two CI faults found by ci-local became tracker items with scope and acceptance, ordered as session 2b before session 3. Owner question open on 37.14x (ER-17 at Nomination or Scrutiny). The revoke DTO binding fault found by the backend guard test was fixed in session 2. |
| 2026-11-13 | Plan rewrite (owner) | All open items read against the code, the old stream E order and the spec 023 delivery plan. Plan rewritten as session cards S01 to S109 and track H, gate cards in section 5, migration ledger in section 7, faults R1 to R18 in section 8. Owner answers: cards in this file, 37.14x keeps ER-17 at Scrutiny, book batched in track H, gated items get gate cards. Tracker: 28.33 WONTDO, 84.59 back to TODO, corrections on 84.25, 84.84, 84.99, 92.2, 95.2 and 37.1p; 37.14u to 37.14x renumbered to 37.16i to 37.16l (R17); 37.16l records the owner answer. `backlog_inventory.py --check`: 0 problems | This file, `docs/TODO.md`, delivery plan F2 pointer, `CLAUDE.md`, `AGENTS.md`, `GEMINI.md`, `.github/copilot-instructions.md`, orchestrator skills, `.claude/memory/` |
| 2026-11-13 | 37.16l (closed) | Owner kept ER-17 at Scrutiny. Handbook Annex B now states the phase each filled form prints from, matching `EarliestPhase`. Band tests passed in the 2026-11-13 backend filtered run (186 of 186), not rerun for the doc edit | `docs/Elections/12-Election-Officials-Handbook.md`, `docs/TODO.md`, this file |
| 2026-11-13 | Context sync | Stale "stream" pointers in nine design files fixed; README no longer claims FluentValidation; book rule in `.claude/rules/book.md` and the copilot file; project memory files for WP37, WP62, meetings and owner decisions point at the cards | Design reference files, `README.md`, agent context files, `.claude/memory/` |
| 2026-11-13 | 12.9 (closed) | The spec fixes it left: status lines on specs 011 and 027, a note on spec 004's 37.9 and 37.10 headings, spec 012 FR-035, spec 013 FR-022, spec 014 method references, spec 017 FR-032, and `CreatedByUserId` on 90.1. Spec 019 needed nothing. Checked by grep; no code changed | Specs 004, 011, 012, 013, 014, 017, 027, `docs/TODO.md`, this file |
| 2026-11-13 | Election walkthrough (owner) | New checks: roll unfreeze and refreeze, filled ER-17 and ER-29 refused before their phase, ER-17 filled at Scrutiny, receipt PDF on API ballots, a handover tick, an unknown form code. README gained "What the run covers". Rule 4 now makes every election card add its walkthrough step. `tsc --noEmit` on the file: clean. The run was not made | `election.walkthrough.ts`, its README, agent context files, `docs/TODO.md`, this file |
| 2026-11-13 | Membership and event walkthroughs (owner) | Built in the election walkthrough's shape, sharing its helpers: `tests/e2e/membership-walkthrough/` (two applications, email codes, fee marked received, approve and reject, first login) and `tests/e2e/event-walkthrough/` (paid event with a seat limit and waitlist, member and guest registrations, approve and reject, pass, check-in). Runners `scripts/run-membership-walkthrough.bat`, `run-event-walkthrough.bat`, `run-workflow-walkthrough.bat`. The old single-browser spec kept, with two selector fixes. Found while writing them: duplicates answer 500 (84.102, added to S08). Type-checked with `tsc --noEmit`; not run, since the local stack is needed | Walkthrough folders, `playwright.*.config.ts`, `package.json`, `.gitignore`, `scripts/local-election.py` (`sender`, `code-email`, `mail-error-email`), runners, `docs/TODO.md`, this file |
| 2026-11-13 | 84.102 (built, partial) | Duplicate application and refused event registrations answer 409 with the reason, unknown event 404; web shows one message, mobile shows the server reason. Tests written in all three layers; web app type-check clean; backend, Vitest and Flutter runs left for the Windows machine. Walkthroughs now make sure a cash payment method is enabled, creating or switching it on as the SuperAdmin | `RequestConflictException.cs`, `MemberService.cs`, `EventService.cs`, `RegistrationController.cs`, `EventsController.cs`, their tests, `register.ts`, `events.ts`, their specs, `events_service.dart`, two event screens, `events_register_error_test.dart`, `fake_services.dart`, specs 012 and 014, registry, walkthroughs |
| 2026-11-13 | Docs sync after the walkthroughs and 84.102 | `PROJECT_MAP.md` test table lists the three walkthroughs, their helpers, runners and `local-election.py`; README testing section names them; `BUSINESS_TEST_CHECKLIST.md` points sections A and B at them; tracker items 84.80, 84.92, 84.95, 84.97 and 97.1 and cards S11, S13, S58, S64 and S90 now say which walkthrough step each adds | `docs/PROJECT_MAP.md`, `README.md`, `docs/BUSINESS_TEST_CHECKLIST.md`, `docs/TODO.md`, this file |
| 2026-11-14 | 84.102 (closed) | Ran the three test layers and both walkthroughs on Windows: backend 105/105, web `register.spec.ts` + `events.spec.ts` 21/21, `events_register_error_test.dart` 3/3, `run-membership-walkthrough.bat` and `run-event-walkthrough.bat` both passed. `events.spec.ts`'s "shows the server reason when a registration is refused" had a `paymentReference` of 'REF' (3 chars), short of the form's own `minLength(4)` validator, so the form's own validation message pre-empted the assertion; fixed to 'REF123'. 93.3's note no longer needs to rule out the old duplicate-500 toast, since 84.102 replaced it with a 409 and the server's reason | `events.spec.ts`, `docs/TODO.md`, this file |

<!-- backlog-check: on -->

## 13. History

<!-- backlog-check: off -->

### Gap report (2026-11-01)

| ID | Finding | Evidence | Fix |
|---|---|---|---|
| G1 | The non-election plan names 5 closed items and leaves 33 open items out. | `backlog_inventory.py --check docs/NON_ELECTION_BACKLOG_EXECUTION_PLAN.md`: 38 problems. Closed: 34.D17, 84.5, 84.8, 96.15, 98.6. That file has since been deleted; to rerun the check, restore it from git history first (`git show HEAD:docs/NON_ELECTION_BACKLOG_EXECUTION_PLAN.md`). | That plan is now history. This file names every open item and passes `--check`. |
| G2 | Seven tracker lines use a status the parser does not read, so they are not counted as open or closed. | `[IN-PROGRESS]` with a hyphen: 7.16 and 27.10 in `TODO.md`; 20.3, 20.4, 51.4 and 51.5 in `TODO_ARCHIVE.md`. `[OPEN]`: 98.43. The parser in `docs/book/build/tracker_page.py` reads `IN PROGRESS` with a space. | Fixed 2026-11-01 (99.4). 27.10, 20.3 and 20.4 closed, since 82.86, 82.82 and 82.83 are DONE. 7.16, 51.4 and 51.5 are `[IN PROGRESS]` and back in `TODO.md`. 98.43 is `[ONHOLD]` behind the config hold. The archive now holds only DONE and WONTDO items. |
| G3 | Five wave 1 candidates from the plan were stale. | 46.5, 82.4, 44.16 and 61.3 are DONE. 51.4 sits in the archive with an unreadable status (G2). 84.46 is used twice. | Dropped from wave 1. 84.46 stays in stream E with an owner gate. |
| G4 | The spec 001 index has no row for spec 026, the meetings spec. | `docs/specs/001-platform-baseline/spec.md`, feature spec table. | Add the row with 84.19. |
| G5 | Docs written before 84.21 still name `FamilyService`. | `BACKLOG_EXECUTION_PLAN.md`, the 57.1 audit, the 001 feature catalogue, `TEST_COVERAGE_PLAN.md`. | Left alone: they are dated records. |
| G6 | The ci-local web unit failure ("reading 'config'") did not come back. | `python scripts/ci-local.py --only web` on 2026-11-01: 938 of 938 passed, exit 0. | Closed. Watch for it in later runs. |
| G7 | The Priority Index at the top of `TODO.md` matches its items. | Each single-ID index line compared with its item state on 2026-11-01: no disagreement. | None. Recheck with each wave. |
| G8 | Some PARTIAL items have only an owner step left. | 7.26, 7.28, 37.1m, 37.14o, 84.42, 84.48. | Listed as owner gates in section 5, not as engineering work. |

### Wave 1 (built 2026-11-01)

Wave 1 was meant to hold defects and PARTIAL items that code alone could close. After the stale
candidates were dropped (G3), four items were left. It was not padded with items that need owner input.

| ID | Stream | Why ready | Layers | Test evidence | Gate |
|---|---|---|---|---|---|
| 7.22 | N1 | One config value, no code path. | Mobile config | grep of the four preprod files | Mobile sign-in on preprod stays an owner check. |
| 84.21 | N2 | `FamilyService` duplicated `FamilyLinkService` search. | API, Application, Infrastructure | `FamilyLinkControllerTests`, `MemberSearchOrderTests`: 20 of 20 | None |
| 84.31 | N2 | Two spec folders numbered 010. | Docs | grep for `010-election-engine-fixes` outside history rows | None |
| 90.4 | N4 | Text named a component that does not exist. | Tracker text | Spec 010 and the non-election plan already name the 88.5 grid | None |

<!-- backlog-check: on -->
