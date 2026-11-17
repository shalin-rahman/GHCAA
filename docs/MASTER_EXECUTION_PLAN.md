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

- **Date:** 2026-11-17. **Open:** 145 items (`python scripts/backlog_inventory.py`), held 12
  (`--state ONHOLD`).
- **Added 2026-11-17:** Work Package 100 (public site, admin and member experience gaps), 20 items,
  19 of them open. Owner, 2026-11-17: place its sessions by their dependencies, not last. The rows
  sit in section 3 by priority and after what they need: S110 to S113 (P1) in phase B before S14,
  S114 to S120, S127 and S128 (P2) at the end of phase F, the rest (P3) in phase G. Their cards
  stay together in the short form under "Phase I" in section 4; rewrite a card before running
  it. The owner answered decisions D1 to D8 of spec 029 the same day, so no session of the
  package waits on a gate. The decision item closed with the answers (S122, see the progress
  log); 100.16 to 100.20 are the build items.
- **Cards (99.10):** phases A, B, C and D (S01 to S44) are in the eleven-part form and were checked against
  the code. Phases E to G and track H are still in the short form; rewrite a phase before running
  it. `python scripts/backlog_inventory.py --card <session or item id>` prints a card skeleton and
  the tracker entry.
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

| # | Items | Summary | Size | Layers | Mig | After |
|---|---|---|---|---|---|---|
| **A** | **CI health and the rest of session 2b** | | | | | |
| S01 | 84.100 | [Fix web test isolation and CI failure detection](#s01) | M | web tests, ci-local | | |
| S02 | 84.101, 84.99 | [Find tool manifests and repair migration SQL](#s02) | S | ci-local, migrations SQL | | |
| S03 | 37.16j | [Verify print revocation and audit atomicity](#s03) | S | backend | | |
| S04 | 37.16k | [Rate-limit print verification and reuse print records](#s04) | M | backend | | S03 |
| S05 | 37.16i | [Manage election print records on the web](#s05) | M | API, web | | S04 |
| S06 | 37.14n, 37.14t | [Run the print-record walkthrough and QR check](#s06) | S | local stack run | | S05 |
| S07 | 99.3 | [Run full CI and get owner acceptance](#s07) | S | full ci-local | | S01, S02 |
| **B** | **P1 items with no owner gate** | | | | | |
| S08 | 84.25, 28.25 | [Check mentorship and job validation plus config E2E](#s08) | M | backend, e2e | | |
| S09 | 84.51, 84.49 (API part) | [Verify login lockout and member NID API paths](#s09) | S | local API | | |
| S10 | 84.92 (backend) | [Add payment verification and rejection APIs](#s10) | M | API | | |
| S11 | 84.92 (clients) | [Add payment review and rejection details to clients](#s11) | M | web, mobile | | S10 |
| S12 | 84.80 (backend) | [Cancel event registrations and promote the waitlist](#s12) | M | API | | |
| S13 | 84.80 (clients) | [Add registration cancellation to both clients](#s13) | M | web, mobile | | S12 |
| S110 | 100.1, 100.2 | [Fix public links, add Join, clean index.html](#s110) | S | web | | |
| S111 | 100.3 (API) | [Save all mail flags, add the mail check and unsubscribe](#s111) | M | API | | |
| S112 | 100.3 (clients) | [Email preferences on web and mobile, unsubscribe page](#s112) | M | web, mobile | | S111 |
| S113 | 100.4 | [Preview, confirm and queue batch email](#s113) | L | API, web | 2a | S111 |
| S14 | 84.81 | [Send due and event reminders on schedule](#s14) | L | API, OrgConfig | 3 | S111 |
| S15 | 88.1 | [Make member imports unique, CSV-capable and idempotent](#s15) | L | API | 4 | |
| S16 | 88.2, 88.3 | [Make batch imports transactional and report row counts](#s16) | M | API | | S15 |
| S17 | 88.5 (backend) | [Preview member imports and report cell errors](#s17) | M | API | | S16 |
| S18 | 88.5 (grid) | [Build the shared editable import data grid](#s18) | M | web | | S17 |
| S19 | 88.4 | [Verify member import counts on PostgreSQL](#s19) | S | local PostgreSQL | | S16 |
| **C** | **Election sessions 3 to 6** | | | | | |
| S20 | 37.16f | [Share the voting-member eligibility rule](#s20) | S | backend | | |
| S21 | 37.16g | [Assign election winners through governance](#s21) | M | backend | | S20 |
| S22 | 37.16h (backend) | [Add nomination photo upload and approval APIs](#s22) | L | API | 5 | S21 |
| S23 | 37.16h (clients) | [Add approved nomination photos to both clients](#s23) | M | web, mobile | | S22 |
| S24 | 37.14d, F2 schema | [Add election safeguards and audit schema](#s24) | L | migration | 6 | S23 |
| S25 | 37.16e | [Enforce election conflict declarations](#s25) | M | backend | | S24 |
| S26 | 37.1p (recount, consent, reopen) | [Add recount approval, consent and nomination reopening](#s26) | M | backend | | S25 |
| S27 | 37.14a | [Track election key holders and readiness](#s27) | M | backend, security | | S26 |
| S28 | 37.12y | [Add controlled polling extensions](#s28) | M | backend, security | | S27 |
| S29 | 37.1p (unopposed, paper counts) | [Handle unopposed seats and approved paper counts](#s29) | M | backend, security | | S28 |
| S30 | 37.14f | [Add two-person election data purge](#s30) | M | backend, security | | S29 |
| S31 | F5 web part 1 | [Build web readiness, key and polling controls](#s31) | M | web | | S30 |
| S32 | F5 web part 2 | [Build web recount, conflict and purge workflows](#s32) | M | web | | S31 |
| S33 | F6 mobile, 84.49 (B4-B5) | [Add election controls and safe key retries on mobile](#s33) | M | mobile | | S32 |
| **D** | **Meetings, then the model work after it** | | | | | |
| S34 | 84.19 (T002, T002a, T003) | [Add meeting entities and schema](#s34) | L | domain, migration | 7 | |
| S35 | 84.19 (T004 to T006) | [Add meeting services and API endpoints](#s35) | M | API | | S34 |
| S36 | 84.19 (T007 to T009) | [Add meeting agendas, participants and RSVPs](#s36) | M | API | | S35 |
| S37 | 84.20 (T010 to T012) | [Add meeting to-do lists](#s37) | M | API | | S36 |
| S38 | 84.19 (T013 to T015) | [Link meeting polls and audience rules](#s38) | M | API | | S37 |
| S39 | 84.19 (T016 to T018) | [Build meeting admin and member web screens](#s39) | M | web | | S38 |
| S40 | 84.19 (T019 to T020) | [Build meeting screens on mobile](#s40) | M | mobile | | S39 |
| S41 | 84.19, 84.20 (T021, close) | [Register and document meeting contracts](#s41) | S | docs | | S40 |
| S42 | 84.59 | [Type shared API DTOs and compare both clients](#s42) | M | API, both clients | | S41 |
| S43 | 84.84 (backend) | [Add private member locations and directory filters](#s43) | L | API | 8 | S41 |
| S44 | 84.84 (clients) | [Add location fields and filters to both clients](#s44) | M | web, mobile | | S43 |
| **E** | **Election sessions 7 to 9** | | | | | |
| S45 | 95.2 (N1 migration) | [Add election kinds, end reasons and term settings](#s45) | L | migration | 9 | S33 |
| S46 | 95.4 | [Show vacant governance seats](#s46) | M | API, web, mobile | | S45 |
| S47 | 95.12 (backend) | [Automate term endings and manage end reasons](#s47) | M | API, security | | S46 |
| S48 | 95.12 (clients) | [Add end-reason controls to both clients](#s48) | M | web, mobile | | S47 |
| S49 | 95.5 (backend) | [Support by-elections for vacant seats](#s49) | M | API | | S48 |
| S50 | 95.5 (clients) | [Show election kinds and live elections in clients](#s50) | M | web, mobile | | S49 |
| S51 | 37.13f | [Verify hosting limits and document backup evidence](#s51) | S | docs, backend log | | S50 |
| **F** | **P2 items** | | | | | |
| S52 | 84.4 | [Compare spec 004 endpoints with both clients](#s52) | S | docs | | |
| S53 | 93.3 | [Diagnose and fix the member-registration E2E failure](#s53) | S | e2e | | |
| S54 | 84.27 (API tests) | [Test archive API status and authorization paths](#s54) | S | backend | | |
| S55 | 84.27 (web page) | [Build the admin archive moderation page](#s55) | M | web | | S54 |
| S56 | 84.55 | [Limit gallery creation to supported fields](#s56) | M | API, clients check | | |
| S57 | 84.93 | [Manage effective-dated constitution versions](#s57) | M | API, web | | |
| S58 | 84.97 | [Require termination reasons and show history](#s58) | M | API, web | | |
| S59 | 84.67 | [Add mobile password change with step-up](#s59) | M | mobile | | |
| S60 | 34.D8 | [Repair the local admin role through the protected seeder](#s60) | M | local stack | | |
| S61 | 84.65 | [Build the scholarship admin page](#s61) | M | web | | |
| S62 | 49.3.C | [Add account contact email and reset delivery](#s62) | L | API, web | 10 | |
| S63 | 97.1 (backend) | [Add email-verification OTP endpoints](#s63) | M | API | | |
| S64 | 97.1 (clients) | [Show and complete email verification in both clients](#s64) | M | web, mobile | | S63 |
| S65 | 84.83 (backend) | [Add mentor opt-in and directory filtering](#s65) | L | API | 11 | |
| S66 | 84.83 (clients) | [Add mentor settings and filters to both clients](#s66) | M | web, mobile | | S65 |
| S67 | 84.82 (backend) | [Add event feedback storage and admin summary](#s67) | L | API | 12 | |
| S68 | 84.82 (clients) | [Add event ratings to both clients](#s68) | M | web, mobile | | S67 |
| S69 | 84.72 (backend) | [Add forum moderation APIs and default categories](#s69) | L | API | 13 | |
| S70 | 84.72 (web) | [Build the admin forum tools for web](#s70) | M | web | | S69 |
| S71 | 84.72 (mobile) | [Build role-guarded forum tools for mobile](#s71) | M | mobile | | S70 |
| S72 | 92.1 | [Set approval-flow rules with the owner](#s72) | S | spec | | |
| S73 | 92.2 | [Implement configurable approval policies](#s73) | L | API | 14 if table | S72 |
| S74 | 92.3 | [Add web approval settings and queue filters](#s74) | M | web | | S73 |
| S75 | 92.4, 92.5 | [Add mobile approval gating and update specs](#s75) | S | mobile, docs | | S74 |
| S76 | 37.7 (web mechanism) | [Add Bengali translation support to web](#s76) | M | web | | |
| S77 | 37.7 (web strings) | [Translate web interface strings to Bengali](#s77) | M | web | | S76 |
| S78 | 8.8 | [Add Bengali translations to mobile](#s78) | M | mobile | | S77 |
| S79 | 62.2 | [Complete default and GHC profile data packs](#s79) | M | profiles | | |
| S80 | 51.4 | [Test file-storage exclusions and image fallback](#s80) | S | backend tests | | |
| S81 | 27.1, 27.8 | [Add cross-client coverage reporting and a CI gate](#s81) | M | ci | | |
| S82 | 73.5 | [Generate the FR and NFR traceability matrix](#s82) | M | scripts | | |
| S83 | 73.6 | [Pin seeded constitution text in tests](#s83) | S | backend test | | |
| S114 | 100.6 (API, web), 100.7, 100.8 | [Policy pages, skip links and fees on the home page](#s114) | M | API, web | 14a | S110 |
| S115 | 100.5 | [Titles, prerendered link previews, sitemap and share](#s115) | L | API, web, mobile | | S110 |
| S116 | 100.9 | [Web notifications page and bell](#s116) | M | API, web, mobile | | |
| S117 | 100.10 | [Event pass QR, web check-in and attendance list](#s117) | L | API, web, mobile | | S13 |
| S118 | 100.11 | [Typed admin stats, trends and a real status badge](#s118) | M | API, web, mobile | | |
| S119 | 100.12 | [Build the shared data table and move the first lists](#s119) | L | web | | S18 |
| S120 | 100.13 | [Warn before leaving a changed form](#s120) | M | web, mobile | | |
| S127 | 100.19 (API) | [Record refunds with a second person's approval](#s127) | L | API | 14b | S11 |
| S128 | 100.19 (clients) | [Show and approve refunds in both clients](#s128) | M | web, mobile | | S127 |
| **G** | **P3 and P4 items** | | | | | |
| S84 | 84.57, 84.62 | [Centralize upload limits and add assistant rate limits](#s84) | S | API | | |
| S85 | 84.64, 93.4 | [Add mark-all-read and configure the mobile drawer](#s85) | S | mobile | | |
| S86 | 84.69 | [Show public site content in the mobile app](#s86) | M | mobile | | |
| S130 | 100.6 (mobile) | [Link the policy pages from the mobile app](#s130) | S | mobile | | S86, S114 |
| S87 | 84.23 | [Choose and add mobile signature and PDF support](#s87) | M | mobile, API | | |
| S88 | 96.3 | [Move shared web forms to common components](#s88) | M | web | | |
| S121 | 100.14 | [Find members, payments and events from the admin header](#s121) | S | web | | S88 |
| S89 | 84.90 | [Choose and implement certificate-code reuse](#s89) | M | API, both clients | | |
| S90 | 84.95 | [Show application wait times in both clients](#s90) | S | web, mobile | | |
| S91 | 84.94 | [Add an arrears report with CSV export](#s91) | M | API, web | | |
| S92 | 84.66 | [Limit pending archive requests and explain refusals](#s92) | M | API, both clients | | |
| S93 | 51.5 | [Configure file storage and add admin settings](#s93) | M | API, web | | S80 |
| S94 | 84.98 | [Close theme gaps and verify both color modes](#s94) | M | web, mobile | | |
| S95 | 90.1, 90.2 | [Add saved-report storage and query service](#s95) | L | API | 15 | S18 |
| S96 | 90.3, 90.5 | [Add saved-report API endpoints](#s96) | M | API | | S95 |
| S97 | 90.4 | [Build the saved-report web page](#s97) | M | web | | S96 |
| S98 | done 2026-11-13, see log | [Completed; see progress log](#12-progress-log) | | | | |
| S99 | 37.4 (backend) | [Add cohort support to the API](#s99) | L | API | 16 | |
| S100 | 37.4 (clients) | [Add cohort features to web and mobile](#s100) | M | web, mobile | | S99 |
| S101 | 37.5 (backend) | [Add the memorial register to the API](#s101) | L | API | 17 | |
| S102 | 37.5 (clients) | [Add the memorial register to both clients](#s102) | M | web, mobile | | S101 |
| S103 | 37.10 | [Build the annual impact report across layers](#s103) | L | API, web, mobile | | S100 |
| S104 | 62.56 (a, g) | [Map profile-pack sections and check key uniqueness](#s104) | M | API | | S79 |
| S105 | 62.56 (b) | [Export organization profile packs](#s105) | M | API, web | | S104 |
| S106 | 62.56 (c, d) | [Upload, map and preview profile-pack imports](#s106) | L | API, web | | S105 |
| S107 | 62.56 (e, f) | [Add import modes, transactions, tests and docs](#s107) | M | API, docs | | S106 |
| S123 | 100.16 | [Let an author or admin mark one article public](#s123) | L | API, web, mobile | 18 | S115 |
| S124 | 100.17 | [Add alumni spotlights as a news category](#s124) | M | API, web, mobile | | S110 |
| S125 | 100.18 (API) | [Add newsletter subscribers with double opt-in](#s125) | L | API | 19 | S112, S113 |
| S126 | 100.18 (web) | [Add the newsletter sign-up pages and the admin send](#s126) | M | web | | S125 |
| S129 | 100.20 | [Add Android app links on the preprod host](#s129) | M | mobile, web | | S115 |
| S122 | done 2026-11-17, see log | [Completed; see progress log](#12-progress-log) | | | | |
| S108 | 60.3, 6.2, 8.5, 8.7, 37.9 | [Resolve owner decisions on five deferred items](#s108) | S | decisions | | |
| S109 | 37.11 | [Refresh WP37 baselines and close the work package](#s109) | S | WP37 gate | | S103 |
| **H** | **Book track, on the owner's word** | | | | | |
| H00 | book facts list | [Track book facts changed by feature work](#h00) | S | docs | | |
| H01 | 78.10 | [Measure coverage, analysis and operational quality](#h01) | M | measurements | | |
| H02 | 63.8, 84.35 | [Refresh the counts used in the book](#h02) | S | book counts | | H01 |
| H03 | 84.96, 64.8 | [Update the owner-approved claims](#h03) | S | book | | |
| H04 | 64.9, 67.3 | [Update book diagrams](#h04) | M | book figures | | |
| H05 | 67.2 (chapters 7, 10) | [Complete book placeholders in chapters 7 and 10](#h05) | M | book | | H01 |
| H06 | 67.2 (chapter 9) | [Complete book placeholders in chapter 9](#h06) | M | book | | H05 |
| H07 | 67.2 (chapters 11 to 13), 67.4, 67.10 (placeholders left in chapters 9, 10, 12, 13) | [Complete book chapters 11 to 13 and 67.4](#h07) | M | book | | H06 |
| H08 | 67.7, 67.5 | [Join book prose and tighten page layout](#h08) | M | book | | H07 |
| H09 | 63.34, 63.18, 74.3 | [Complete the remaining book updates](#h09) | S | book | | H08 |

Owner-gated items and where they re-enter: section 5.

## 4. Session cards

Rules in section 2 apply to every card. A card in a rewritten phase has the eleven parts below, in
this order, and the plan check fails when one is missing or when a named file does not exist. A file
the card creates is marked "(new)". Phases still in the short form are listed by the check as owed.

<!-- card-template: A B C D -->

| Part | What it holds |
|---|---|
| Goal | One or two sentences: what is true after the session that is not true now. |
| Before you start | Cards that must be closed, and one command that shows the starting state. |
| Read (only these) | The files and symbols to open. Nothing else is read. |
| Change (only these files) | Every file the session may edit or create. Another file needs the owner's yes. |
| Do not touch | Nearby code that looks related and must stay as it is. |
| Steps | Numbered. Each step ends with "-> check:" and how to see it worked. |
| Contract | Route, policy, request and response fields, status codes. "None" when no route or DTO changes. |
| Tests | File, test name, and what the test proves. |
| Run | The focused command first, then the layer gate. |
| Done when | The facts that close the item, and the status each ID gets. |
| Stop and ask if | The cases where the session stops and asks the owner, with the default answer. |

Names on a card were checked against the code on 2026-11-14. A line number is a hint; find the symbol
by name. When a name on a card is not in the code, stop and ask; do not guess a replacement.

### Phase A. CI health and the rest of session 2b

<a id="s01"></a>
**S01 · 84.100 · M**
- Goal: `npm run test:unit` loads all 129 spec files, and `ci-local.py` fails the web layer when any
  spec file fails to load, even with every test passing.
- Before you start: no card needed. In `GHCAA.Web` run `npm run test:unit > vt.log 2>&1`; the log
  shows 36 failed files with "reading 'config'" and no failed test. Delete `vt.log` at the end.
- Read (only these): `GHCAA.Web/vitest.config.ts`, `GHCAA.Web/src/test-setup.ts`, and in
  `scripts/ci-local.py` the `STEPS` list, `run(step)` and `main()`.
- Change (only these files): the one spec that leaks, or `GHCAA.Web/vitest.config.ts`;
  `scripts/ci-local.py`; `scripts/test_ci_local.py` (new).
- Do not touch: application code under `GHCAA.Web/src/app` outside spec files; the `test:unit`
  script in `package.json`; the other web steps (`type-check`, `build`, `audit`).
- Steps:
  1. List the 36 failing files from the log. -> check: the count is 36.
  2. Run the full file list in halves (`npx vitest run <files>`), then halve the failing half, until
     one spec makes the files after it fail. Look in it for `vi.stubGlobal`, a `vi.mock` of a module
     that holds `config`, or `window` or `environment` replaced and not restored. -> check: the full
     run passes with that one spec left out.
  3. Fix the cause in that spec (restore in `afterEach`). Only if three or more specs leak, set
     `unstubGlobals: true` and `restoreMocks: true` in `vitest.config.ts` instead. -> check:
     `npm run test:unit` shows 129 passed files, twice in a row.
  4. In `ci-local.py` add `vitest_failed_files(output)`: it reads the "Test Files" line and returns
     the failed count, or -1 when the line is missing. The `unit-tests` web step captures its output
     and fails when the result is not 0, whatever the exit code. -> check: step 5.
  5. Add a temporary spec that throws inside `describe`, run `python scripts/ci-local.py --only web`,
     see it fail, delete the spec. -> check: `git status --short` shows no leftover spec.
- Contract: none.
- Tests: `scripts/test_ci_local.py` :: `test_failed_files_counted` (a "Test Files  2 failed | 127
  passed" line gives 2); `test_all_passed_gives_zero`; `test_missing_line_gives_minus_one`. Load
  `ci-local.py` with `importlib.util.spec_from_file_location`, since the name has a hyphen.
- Run: `python -m unittest scripts/test_ci_local.py </dev/null`; then
  `python scripts/ci-local.py --only web`.
- Done when: 129 of 129 files pass twice in a row; the temporary spec made the web layer fail;
  84.100 DONE with both counts.
- Stop and ask if: the leak is in `src/test-setup.ts` or in application code, not in a spec. Default:
  fix it in the setup file and say so in the tracker.

<a id="s02"></a>
**S02 · 84.101, 84.99 · S**
- Goal: the contract step names the tool manifest it used, and five old migrations no longer hold a
  bare `SELECT setval`, so the idempotent script applies twice.
- Before you start: S01 closed (it creates `scripts/test_ci_local.py`). Run
  `grep -c "SELECT setval" GHCAA.Infrastructure/Data/Migrations/PgSql/20261021100000_AddEcPositions.cs`;
  a count above 0 means 84.99 is still open.
- Read (only these): in `scripts/ci-local.py` the contract steps `api-build-release`,
  `swagger-generate`, `swagger-diff`; the five migration files in step 3.
- Change (only these files): `scripts/ci-local.py`; `scripts/test_ci_local.py` (new if S01 did not
  make it); `20260927121346_ElectionBallotSecrecy.cs`, `20260930020138_ElectionAppointments.cs`,
  `20261021100000_AddEcPositions.cs`, `20261105120000_SeedJobCategoryLookups.cs`,
  `20261105200808_AddPollDiscussion.cs`.
- Do not touch: the `.Designer.cs` files, `PgSqlApplicationDbContextModelSnapshot.cs`, any other
  migration, and the file `C:\Users\dotnet-tools.json` (the owner renames it).
- Steps:
  1. 84.101: add `find_tool_manifests(start)` to `ci-local.py`. It walks from `start` up to the
     drive root and returns every `dotnet-tools.json` and `.config/dotnet-tools.json` it finds,
     nearest first. -> check: the unit tests below.
  2. Before `dotnet tool restore`, print the list. When restore succeeds and the swagger tool is
     still missing, print which manifest was used and that a stray one higher up hides the repo's.
     -> check: `python scripts/ci-local.py --only contract` prints the list.
  3. 84.99: in each of the five migrations, change every bare `SELECT setval(...)` inside
     `migrationBuilder.Sql` to `DO $sv$ BEGIN PERFORM setval(...); END $sv$;`. Keep the arguments
     as they are. `PERFORM` alone is wrong: it is PL/pgSQL only and fails in the plain `Migrate()`
     run. -> check: `grep -n "SELECT setval" <file>` gives nothing for each file.
  4. On an empty local PostgreSQL database: `dotnet ef migrations script --idempotent -o <scratch file>`,
     with the scratch file outside the repo. Apply it twice with `psql`, then start the API once
     against a second empty database. Delete the scratch file. -> check: both applies end without error and the API starts.
- Contract: none.
- Tests: `scripts/test_ci_local.py` :: `test_finds_stray_manifest_above_repo` (a temp tree with a
  manifest two levels up is listed); `test_nearest_first`; `test_none_found_gives_empty_list`.
- Run: `python -m unittest scripts/test_ci_local.py </dev/null`; then
  `python scripts/ci-local.py --only contract`.
- Done when: the contract layer passes on this machine or names the stray manifest; the idempotent
  script applies twice; `Migrate()` builds an empty database; 84.101 and 84.99 DONE. The tracker
  notes the owner step (rename `C:\Users\dotnet-tools.json`).
- Stop and ask if: a `setval` call sits outside `migrationBuilder.Sql`, or a sixth migration holds
  one. Default: fix it the same way and name the file in the tracker.

<a id="s03"></a>
**S03 · 37.16j · S**
- Goal: proof on PostgreSQL that two revokes of one print at the same moment leave one `RevokedAt`
  and one audit entry, and a written decision on what happens when the audit write fails.
- Before you start: no card needed. Local PostgreSQL is up with a database that tests may create and
  drop. `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~RevokePrint"` passes.
- Read (only these): `RevokePrintAsync` in `GHCAA.Infrastructure/Services/ElectionDocumentService.cs`
  (line 228); `RevokePrint_RevokedBetweenReadAndSave_ReturnsAlreadyRecorded` in
  `GHCAA.Tests/Services/ElectionDocumentServiceTests.cs` (line 883); `GHCAA.Tests/LiveSyncTest.cs`
  for how a test reaches a real database.
- Change (only these files): `GHCAA.Tests/Services/ElectionDocumentServiceTests.cs`; `docs/TODO.md`.
- Do not touch: `RevokePrintAsync` itself, unless the PostgreSQL run shows two rows; the concurrency
  token on `ElectionFormPrint`.
- Steps:
  1. Add the test below. It reads its connection string from the environment variable
     `GHCAA_TEST_PG` and calls `Assert.Ignore` when the variable is not set, so CI skips it.
     -> check: without the variable the test shows as skipped.
  2. The test builds the schema on that database, adds one election and one print, opens two
     contexts, calls `RevokePrintAsync` from both at once, then counts. -> check: with the variable
     set, one call returns done and the other returns already recorded.
  3. Write the decision in the 37.16j tracker entry: the claim and the audit entry are in one
     `SaveChanges`, so a failed audit write rolls the claim back and the caller can try again.
     -> check: the entry names the test and the counts.
- Contract: none.
- Tests: `ElectionDocumentServiceTests.cs` :: `RevokePrint_TwoContextsOnPostgres_OneRevokeOneAuditEntry`
  (`[Category("NFR-R5")]`): one row with `RevokedAt`, one `FormPrintRevoked` audit entry.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ElectionDocumentServiceTests"`
  once without and once with `GHCAA_TEST_PG`; then `python scripts/ci-local.py --only backend`.
- Done when: the PostgreSQL run shows one `RevokedAt` and one audit entry; 37.16j DONE with the
  decision written.
- Stop and ask if: PostgreSQL shows two revokes or two audit entries. Default: stop, set 37.16j
  PARTIAL with the counts, and do not change the service without the owner's answer.

<a id="s04"></a>
**S04 · 37.16k · M**
- Goal: print verification has its own rate limit read from OrgConfig, and a second identical filled
  download returns the first print id and adds no row.
- Before you start: S03 closed. `grep -n "CredentialVerification" GHCAA.API/Controllers/ElectionsController.cs`
  shows the policy on the print verification route (line 486).
- Read (only these): `GHCAA.API/Extensions/RateLimitingExtensions.cs`; `RateLimitPolicies` in
  `GHCAA.Domain/Constants.cs` (line 41); `ElectionSettingsDto` in `GHCAA.Application/DTOs/OrgConfigDto.cs`
  (line 27); `GeneratePdfAsync` in `ElectionDocumentService.cs` (line 98, the `new ElectionFormPrint`
  at 128); `ElectionFormPrint` in `GHCAA.Domain/Models/Election.cs`; memory
  `gotcha_ratelimit_api_policy_overrides_named_policies`.
- Change (only these files): `GHCAA.Domain/Constants.cs`, `RateLimitingExtensions.cs`,
  `GHCAA.API/Controllers/ElectionsController.cs`, `OrgConfigDto.cs`, `ElectionDocumentService.cs`,
  `GHCAA.Tests/OrgConfig/golden/org-config-ghc.golden.json`, `docs/CONFIG_DRIVEN_FRAMEWORK.md`,
  `docs/API_CONTRACT_REGISTRY.md`, `GHCAA.Tests/Services/ElectionDocumentServiceTests.cs`,
  `GHCAA.Tests/Controllers/ElectionsControllerTests.cs`, the authz catalog output.
- Do not touch: the `CredentialVerification` policy and its numbers; the route
  `GET api/elections/{id:int}/documents/{formCode}/verification` (it keeps the old policy);
  `appsettings*.json`; the global limiter.
- Steps:
  1. Add `RateLimitPolicies.PrintVerification = "PrintVerification"`. -> check: build.
  2. Add `PrintVerifyLimit` (int, default 60, `[Range(1, 1000)]`) and `PrintVerifyWindowMinutes`
     (int, default 15, `[Range(1, 1440)]`) to `ElectionSettingsDto`. Both are non-nullable, since
     stored JSON drops nulls. Update the golden as the failure message of
     `OrgConfigGoldenSnapshotTests.cs` says, and add both keys to `CONFIG_DRIVEN_FRAMEWORK.md`.
     -> check: `--filter "FullyQualifiedName~OrgConfigGoldenSnapshotTests"` passes.
  3. Register the policy in `RateLimitingExtensions.cs`: fixed window, per IP, numbers read from the
     OrgConfig election settings. -> check: the rate-limit test below.
  4. Put `[EnableRateLimiting(RateLimitPolicies.PrintVerification)]` on the print verification
     action only. -> check: the controller test below.
  5. In `GeneratePdfAsync`, before the insert, look for a print with the same `ElectionId`,
     `FormCode`, `PrintedByUserId` and `ContentHash` and `RevokedAt == null`. When found, use its
     `Id` and insert nothing. -> check: the two service tests below.
- Contract: no new route. `GET api/elections/print-records/{printId:guid}/verification` stays
  anonymous with the same response; only its rate-limit policy changes (429 past the limit).
  `ElectionSettingsDto` gains the two fields. Record both in `docs/API_CONTRACT_REGISTRY.md`.
- Tests: `ElectionsControllerTests.cs` :: `PrintVerification_UsesItsOwnRateLimitPolicy` (attribute
  names `PrintVerification`); `ElectionDocumentServiceTests.cs` ::
  `GeneratePdf_SameOfficialSameContent_ReusesPrintId` and `GeneratePdf_RevokedPrint_IsNotReused`;
  `ElectionsControllerTests.cs` :: `CardVerification_StillAnswers_AfterPrintLimitIsHit`. Tag
  `[Category("FR-39")]`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ElectionDocumentServiceTests|FullyQualifiedName~ElectionsControllerTests"`;
  `python docs/api/authz_catalog.py --write </dev/null`; `python scripts/ci-local.py --only backend`;
  `python scripts/ci-local.py --only contract`.
- Done when: the four tests pass; the registry and the catalog are updated; 37.16k DONE.
- Stop and ask if: the limiter cannot read OrgConfig per request without blocking on async code.
  Default: read both values once at startup and note in the tracker that a change needs a restart.
  Also stop if the global limiter replaces the named policy; report what the test shows.

<a id="s05"></a>
**S05 · 37.16i · M**
- Goal: an official with Count and ManageSetup lists an election's print records on the web and
  revokes one with a reason; mobile shows the same list read only.
- Before you start: S04 closed. `grep -n "print-records" GHCAA.API/Controllers/ElectionsController.cs`
  shows two routes (verification, revoke) and no list route.
- Read (only these): the revoke action in `ElectionsController.cs` (line 494) and
  `ElectionFormPrintRevokeDto`; `GHCAA.Web/src/app/admin/elections/admin-elections.ts`;
  `GHCAA.Web/src/app/admin/elections/election-form-records/` (the pattern to copy);
  `GHCAA.Web/src/app/common/confirm-dialog/`, `GHCAA.Web/src/app/common/step-up-dialog/`;
  `GHCAA.Web/src/app/core/services/elections.service.ts`;
  `GHCAA.Mobile/lib/screens/admin/election_form_records_sheet.dart` (the pattern to copy).
- Change (only these files): `ElectionsController.cs`, `IElectionDocumentService` and
  `ElectionDocumentService.cs`, `GHCAA.Application/DTOs/ElectionDtos.cs`; web
  `admin/elections/election-print-records/` (new, component plus spec), `admin-elections.ts` and its
  template, `elections.service.ts`, `GHCAA.Web/src/app/core/models/election.models.ts`; mobile
  `GHCAA.Mobile/lib/screens/admin/election_print_records_sheet.dart` (new),
  `election_management_screen.dart`, `GHCAA.Mobile/lib/features/elections/election_service.dart`,
  `GHCAA.Mobile/test/election_print_records_sheet_test.dart` (new); `scripts/verify-election.py`;
  `docs/API_CONTRACT_REGISTRY.md`; backend tests.
- Do not touch: the revoke route and its rules; `election-form-records/` (form details, a different
  thing); the public verify page.
- Steps:
  1. Service method `ListPrintsAsync(int electionId, CancellationToken)` returning rows newest
     first. -> check: the service test.
  2. Controller action `GET {id:int}/print-records` with the same attributes as the revoke action,
     without step-up. -> check: the controller tests.
  3. Web service method `getPrintRecords(electionId)` and `revokePrint(electionId, printId, reason)`
     (the second only if it is missing). -> check: `npm run type-check`.
  4. Web component: a table (form code, printed at, role, phase, revoked at, reason) in
     `.table-wrap`, and a Revoke action that opens the confirm dialog with a reason field (at least
     3 visible characters after trim), then step-up. Hide the action without both permissions and
     on a revoked row. Add it as a "Print records" tab in the election's officials view.
     -> check: the Vitest specs.
  5. Mobile: a read-only sheet with the same columns, opened from `election_management_screen.dart`.
     No revoke on mobile. -> check: the widget test.
  6. Add the new spec to `WEB_SPECS` and the new test to `MOBILE_TESTS` in
     `scripts/verify-election.py`. -> check: `python scripts/verify-election.py </dev/null`.
  7. Browser check in light and dark with the API mocked (memory
     `reference_browser_check_mocked_api`). -> check: both themes show the table and the dialog.
- Contract: `GET api/elections/{id:int}/print-records`. Policy `Policies.ElectionStaff` plus
  `RequireElectionPermission(Count | ManageSetup)`. Response: array of `{ id, formCode, printedAt,
  printedByRole, phaseAtPrint, revokedAt, revokeReason }`. 200; 403 without the permissions; 404 for
  an unknown election. The revoke route is unchanged (204, 409, 404; 400 on a blank reason).
- Tests: `ElectionsControllerTests.cs` :: `ListPrints_WithPermissions_Returns200`,
  `ListPrints_WithoutPermissions_Returns403`; `ElectionDocumentServiceTests.cs` ::
  `ListPrints_ReturnsNewestFirst_WithRevokeFields`; web spec :: "reason under 3 characters keeps
  Revoke disabled", "hides Revoke without both permissions", "hides Revoke on a revoked row"; mobile
  test :: "shows the list and no revoke action".
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ElectionsControllerTests|FullyQualifiedName~ElectionDocumentServiceTests"`;
  in `GHCAA.Web`: `npm run type-check; npm run test:unit -- src/app/admin/elections/election-print-records`;
  in `GHCAA.Mobile`: `flutter analyze; flutter test test/election_print_records_sheet_test.dart`;
  then `python docs/api/authz_catalog.py --write </dev/null` and ci-local for backend, web, mobile
  and contract.
- Done when: a revoke from the web makes `/verify/form/:id` show "Print revoked" with the date; 403
  without permission; registry updated; 37.16i DONE.
- Stop and ask if: the officials view has no tab strip to extend. Default: add the list as a section
  under the form records section.

<a id="s06"></a>
**S06 · 37.14n, 37.14t · S**
- Goal: the election walkthrough passes with its "Print records" section, and one printed QR code
  opens the verify page on a phone.
- Before you start: S05 closed. Read memory `gotcha_election_walkthrough_local_run`. The local
  `.env` already has `GeneralSettings__PortalBaseUrl`; do not edit it.
- Read (only these): `scripts/run-election-walkthrough.bat`;
  `GHCAA.Web/tests/e2e/election-walkthrough/README.md`.
- Change (only these files): `docs/TODO.md`;
  `GHCAA.Web/tests/e2e/election-walkthrough/README.md` and `election.walkthrough.ts`, only when a
  step fails because the test is wrong.
- Do not touch: `.env`, `appsettings*.json`, application code. A failure caused by the application
  goes to the tracker as a new item; it is not fixed in this session.
- Steps:
  1. Restart the local API so the print migration applies. -> check: the API log shows
     `20261112120000_AddElectionFormPrints` applied or already present.
  2. In `GHCAA.Web`: `npx ng serve --host 0.0.0.0`; port 4200 open in the firewall. -> check: the
     site opens from the phone's browser on the same network.
  3. Run `scripts\run-election-walkthrough.bat`. The run also covers roll unfreeze and refreeze,
     filled prints refused before their phase, the receipt PDF on API ballots, a handover tick and
     an unknown form code. -> check: the report shows every section green, "Print records" included.
  4. Owner step: scan one printed QR code with a phone. -> check: the verify page opens; record
     phone, browser and result in the tracker.
- Contract: none.
- Tests: none added. The walkthrough is the evidence.
- Run: `scripts\run-election-walkthrough.bat`.
- Done when: the walkthrough is green; the phone scan opens the verify page; 37.14n and 37.14t DONE.
  The preprod release is gate G-REL and is not part of these items.
- Stop and ask if: the owner is not at hand for the scan. Default: close step 3, set both items
  PARTIAL with "phone scan left", and leave the card open for step 4 only.

<a id="s07"></a>
**S07 · 99.3 · S**
- Goal: one full `ci-local.py` run with every failure named, and the owner's acceptance of wave 1.
- Before you start: S01 and S02 closed. At least 10 GB free on C: (section 2, rule 14).
- Read (only these): the 99.3 and 98.34 tracker entries.
- Change (only these files): `docs/TODO.md`, this plan.
- Do not touch: any code. A failure found here becomes a tracker item, not a fix.
- Steps:
  1. `python scripts/ci-local.py`. If disk space is short, run `--only backend`, then `web`, then
     `mobile`, then `contract`, one at a time. -> check: a pass or fail line for each step.
  2. For each failure write one line: step, cause, tracker ID. The `audit` step stays red under
     98.34, which is on hold. -> check: no failure without an ID.
  3. Ask the owner (question dialog) to accept wave 1, with the result table in the question.
     -> check: the answer is recorded in the 99.3 entry with its date.
- Contract: none.
- Tests: none added.
- Run: `python scripts/ci-local.py`.
- Done when: the owner accepts; 99.3 DONE with the run date and counts.
- Stop and ask if: a step other than `audit` fails. Default: open an item for it, give it a card, and
  ask the owner whether acceptance waits on that card.

### Phase B. P1 items with no owner gate

<a id="s08"></a>
**S08 · 84.25, 28.25 · M**
- Goal: the mentorship controller and the job DTO have tests for each refusal case, and the config
  regression run is recorded as the cover for 28.25.
- Before you start: no card needed. Local stack up (`.\GHCAA.Tools\run-app.ps1`) for step 3.
- Read (only these): `GHCAA.Tests/Controllers/MentorshipControllerTests.cs`,
  `GHCAA.Tests/Validators/JobDtoValidationTests.cs`, `GHCAA.Web/tests/e2e/config-regression.spec.ts`.
- Change (only these files): the two test files above; `docs/TODO.md`.
- Do not touch: the controller and the DTO. No FluentValidation validator is added (they never run;
  `CreateJobDto` already uses DataAnnotations). No `--update-snapshots`.
- Steps:
  1. Run the focused command below. -> check: it passes; note the count.
  2. In the two test files look for a test of each case: 401 without a login, 400 on an invalid
     body, 409 on a duplicate request, 403 on an AdminOnly action as a member. Add a test for each
     missing case, in the file's style. -> check: one test per case, all passing.
  3. In `GHCAA.Web`: `npx playwright test tests/e2e/config-regression.spec.ts`. -> check: all pass;
     note the count.
- Contract: none.
- Tests: `MentorshipControllerTests.cs` and `JobDtoValidationTests.cs` :: one test per case in
  step 2, named `<Action>_<Case>_Returns<Code>`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~MentorshipControllerTests|FullyQualifiedName~JobDtoValidationTests"`;
  then `python scripts/ci-local.py --only backend`.
- Done when: 84.25 DONE with the test count; 28.25 DONE as covered by the config regression run,
  with its count and date.
- Stop and ask if: the config regression run fails. Default: 28.25 stays open with the failing test
  named; no snapshot is updated.

<a id="s09"></a>
**S09 · 84.51, 84.49 (API part) · S**
- Goal: a recorded local run shows the lockout and unlock work, and an imported member can replace a
  placeholder NID.
- Before you start: no card needed. Local stack up. A SuperAdmin login and one member invite at hand.
- Read (only these): the 84.51 and 84.49 tracker entries; `Constants.Login` and `Constants.Nid` in
  `GHCAA.Domain/Constants.cs`.
- Change (only these files): `docs/TODO.md`.
- Do not touch: any code. A wrong result becomes a tracker note, not a fix.
- Steps:
  1. 84.51: send five wrong passwords to `setup-with-password` for one user. -> check: the sixth
     try gives 423, and login for that user gives 423.
  2. `POST api/roles/users/{id}/unlock` as SuperAdmin. -> check: login then succeeds.
  3. 84.49: import one member through `POST api/admin/members/import` with a placeholder NID.
     -> check: the member exists.
  4. As that member, `PUT api/profile` with a valid 10-digit NID, then with an invalid one.
     -> check: 200, then 400 with the reason in the body.
  5. Paste the method, route and status of each call into the two tracker entries. No tokens or
     passwords in the tracker. -> check: each step has its status line.
- Contract: none.
- Tests: none added. The HTTP log is the evidence.
- Run: the calls above with `curl` or the Swagger page.
- Done when: 84.51 DONE; 84.49 says the API check passed and stays PARTIAL for B4 and B5 (S33) and
  the device check (gate G-DEV).
- Stop and ask if: a call gives another status. Default: record it, leave the item open, and do not
  change code in this session.

<a id="s10"></a>
**S10 · 84.92 (backend) · M**
- Goal: an admin lists pending payment declarations and verifies or rejects one with a reason; the
  due is marked paid on verify and the member is told either way.
- Before you start: no card needed. `grep -n "update-status" GHCAA.API/Controllers/FinancialsController.cs`
  shows one PATCH route.
- Read (only these): `GHCAA.API/Controllers/FinancialsController.cs` (`update-status`);
  `UpdatePaymentStatusAsync` in `GHCAA.Infrastructure/Services/FinancialService.cs`;
  `GHCAA.Domain/Models/PaymentHistory.cs`, `GHCAA.Domain/Models/MembershipDue.cs`;
  `GHCAA.Application/DTOs/FinancialDtos.cs`; `GHCAA.Application/Interfaces/INotificationService.cs`.
- Change (only these files): `FinancialsController.cs`, `GHCAA.Application/Interfaces/IFinancialService.cs`,
  `FinancialService.cs`, `FinancialDtos.cs`, `GHCAA.Tests/Services/FinancialServiceTests.cs`,
  `GHCAA.Tests/Controllers/FinancialsControllerTests.cs`, `docs/API_CONTRACT_REGISTRY.md`, the authz
  catalog output, `docs/TODO.md`.
- Do not touch: `PaymentStatus` in `GHCAA.Domain/Enums.cs` (no new value; reject uses `Failed`);
  `PaymentCallbackOrchestrator.cs` and the gateway path; the old `PATCH update-status` route keeps
  working for one release.
- Steps:
  1. DTOs: `PendingPaymentDto` and `RejectPaymentDto` with `[Required, MinLength(3)]` on `Reason`;
     the service also trims and refuses fewer than 3 visible characters. -> check: build.
  2. Service `GetPendingPaymentsAsync`: rows with `Status == Pending`, oldest first. -> check: test.
  3. Service `VerifyPaymentAsync(int paymentId, int adminUserId)` and
     `RejectPaymentAsync(int paymentId, int adminUserId, string reason)`. Only a Pending payment
     changes; anything else returns a "settled" result. Verify sets `Completed` and, on the linked
     due, `IsPaid`, `PaymentDate`, `PaymentHistoryId`. Reject sets `Failed` and writes the reason to
     `Notes`. One `SaveChanges` for each. -> check: tests.
  4. Both write an activity row (admin, old status, new status) and notify the member through
     `INotificationService`, in the way `UpdatePaymentStatusAsync` does today. -> check: tests.
  5. Controller actions per the contract; mark `update-status` deprecated in the registry.
     -> check: controller tests and the reflection test.
- Contract: all `AdminOnly`.
  `GET api/financials/pending` -> 200, array of `{ id, memberId, memberName, dueId, amount,
  transactionReference, proofPath, submittedAt }`.
  `POST api/financials/{id:int}/verify` -> 204; 404 unknown; 409 not Pending.
  `POST api/financials/{id:int}/reject`, body `{ reason }` -> 204; 400 reason under 3 visible
  characters; 404; 409.
- Tests: `FinancialServiceTests.cs` :: `Verify_PendingPayment_MarksDuePaid`,
  `Reject_StoresReasonAndSetsFailed`, `Verify_SettledPayment_ReturnsSettled`,
  `Reject_SettledPayment_ReturnsSettled`, `GetPending_ReturnsOnlyPending_OldestFirst`,
  `Verify_NotifiesMember`; `FinancialsControllerTests.cs` :: `Reject_BlankReason_Returns400`,
  `Verify_Settled_Returns409`, `Pending_AsMember_IsAdminOnly`. Tag with the FR of 84.92 in spec 015.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~FinancialServiceTests|FullyQualifiedName~FinancialsControllerTests"`;
  `python docs/api/authz_catalog.py --write </dev/null`; `python scripts/ci-local.py --only backend`;
  `python scripts/ci-local.py --only contract`.
- Done when: the nine tests pass; registry lists the three routes and the deprecation; 84.92 notes
  "backend done, clients in S11" and stays PARTIAL.
- Stop and ask if: `PaymentHistory` has no field that links it to a due, so verify cannot find the
  due. Default: match on member and amount only when exactly one unpaid due fits; otherwise leave
  the due alone and say so in the response. Also stop if a field in the pending response does not
  exist on the model; name the missing field.

<a id="s11"></a>
**S11 · 84.92 (clients) · M**
- Goal: the web admin has a pending payments page with verify and reject; the member payment screens
  on web and mobile show the status and the rejection reason.
- Before you start: S10 closed. The three routes answer on the local API.
- Read (only these): `GHCAA.Web/src/app/admin/ledger/ledger.ts` (page pattern);
  `GHCAA.Web/src/app/core/services/financial.service.ts`; `GHCAA.Web/src/app/member/payments/payments.ts`;
  `GHCAA.Web/src/app/app.routes.ts` (admin block); `ADMIN_NAV_SECTIONS` in
  `GHCAA.Web/src/app/core/services/nav.service.ts`; `GHCAA.Mobile/lib/screens/member/financial_portal_screen.dart`;
  `GHCAA.Mobile/lib/core/models/financial_models.dart`;
  `GHCAA.Web/tests/e2e/membership-walkthrough/README.md`.
- Change (only these files): web `src/app/admin/payments-pending/` (new: component, template, spec),
  `app.routes.ts`, `nav.service.ts`, `financial.service.ts` and its spec, `member/payments/payments.ts`
  with its template and spec; mobile `financial_portal_screen.dart`, `financial_models.dart`,
  `GHCAA.Mobile/test/financial_portal_rejection_test.dart` (new);
  `GHCAA.Web/tests/e2e/membership-walkthrough/membership.walkthrough.ts` and `README.md`;
  `docs/PROJECT_MAP.md`; `docs/TODO.md`.
- Do not touch: the admin routes `payments` and `ledger` and their pages; mobile gets no admin
  verify screen (not asked for); theme files (use the existing tokens and shared components).
- Steps:
  1. Web service: `getPendingPayments()`, `verifyPayment(id)`, `rejectPayment(id, reason)`.
     -> check: service spec.
  2. Page at route `admin/payments-pending` with the same guard as `admin/ledger`: a table in
     `.table-wrap` (member, amount, reference, submitted, proof link, actions). Proof opens through
     the secure files route the ledger page uses. Verify asks through the confirm dialog. Reject
     opens a dialog with a reason field, disabled under 3 visible characters. A 409 shows the server
     message and reloads the list. -> check: component spec.
  3. Add the page to the finance section of `ADMIN_NAV_SECTIONS`. -> check: the link shows for an
     admin.
  4. Member payments page (web) and `financial_portal_screen.dart` (mobile): show the status and,
     on a failed declaration, the reason from `notes`. -> check: spec and widget test.
  5. Membership walkthrough: verify one fee on the new page and reject one declaration with a
     reason; remove the 84.92 gap line and update "What the run covers" in the README. -> check:
     `scripts\run-membership-walkthrough.bat` is green.
  6. Browser check in light and dark. -> check: table, both dialogs and the member reason show.
- Contract: none new (uses S10's routes).
- Tests: `payments-pending` spec :: "lists pending payments", "reject stays disabled under 3
  characters", "verify calls the service and reloads", "409 shows the message and reloads";
  `payments.spec.ts` :: "shows the rejection reason"; `financial_portal_rejection_test.dart` ::
  "shows the rejection reason on a failed payment".
- Run: in `GHCAA.Web`: `npm run type-check; npm run test:unit -- src/app/admin/payments-pending src/app/member/payments`;
  in `GHCAA.Mobile`: `flutter analyze; flutter test test/financial_portal_rejection_test.dart`; then
  `python scripts/ci-local.py --only web` and `--only mobile`.
- Done when: the walkthrough is green with the two new steps; 84.92 DONE; the 78.9 entry notes
  Treasurer Tasks 1 and 2 are unblocked.
- Stop and ask if: the nav has no finance section, or the member payment DTO does not carry `notes`.
  Default for the first: put the link beside "Ledger". For the second: stop, since it needs a DTO
  change that belongs in S10.

<a id="s12"></a>
**S12 · 84.80 (backend) · M**
- Goal: a member cancels their own event registration, and the oldest waitlisted registration of
  that event moves up in the same transaction.
- Before you start: no card needed.
  `grep -n "Waitlisted" GHCAA.Domain/Enums.cs` shows `EventRegistrationStatus { Pending, Approved,
  Rejected, Waitlisted }` with no `Cancelled`.
- Read (only these): `RegisterForEventAsync` (line 229) and `ApproveRegistrationAsync` (line 499)
  in `GHCAA.Infrastructure/Services/EventService.cs`; `GHCAA.Domain/Models/EventRegistration.cs`;
  `GHCAA.Infrastructure/Data/Configurations/EventRegistrationConfiguration.cs`;
  `GHCAA.API/Controllers/EventsController.cs`.
- Change (only these files): `GHCAA.Domain/Enums.cs`, `GHCAA.Application/Interfaces/IEventService.cs`,
  `EventService.cs`, `EventsController.cs`, `GHCAA.Application/DTOs/EventDto.cs` (only if a result
  type is needed), `GHCAA.Tests/Services/EventServiceTests.cs`,
  `GHCAA.Tests/Controllers/EventsControllerTests.cs`, `docs/API_CONTRACT_REGISTRY.md`, the authz
  catalog output, `docs/TODO.md`.
- Do not touch: no migration and no new column (`EventRegistration` has no `Notes`; none is added);
  refunds; the capacity rule in `RegisterForEventAsync`.
- Steps:
  1. Add `Cancelled` as the last value of `EventRegistrationStatus`. The status is stored as an
     int, so the order of the old values must not change. -> check: build; existing tests pass.
  2. Every count of taken places must leave out `Cancelled`. Grep `EventRegistrationStatus` in
     `EventService.cs` and fix each count that would include it. -> check: test
     `Cancel_FreesThePlace`.
  3. `CancelRegistrationAsync(int registrationId, int memberId)` inside the execution strategy and
     one transaction: load the registration; not the owner returns Forbidden; already `Cancelled` or
     `Rejected` returns Conflict; set `Cancelled`; if it held a place (`Approved`, or `Pending` on a
     paid event), take the oldest `Waitlisted` registration of that event and set it to `Pending`
     when the event has a fee, else `Approved`. -> check: tests.
  4. After commit: notify the promoted member. For a registration that had a completed payment,
     notify users in the treasurer role "refund to be handled" and write an activity row; the
     refund itself is out of scope. Record that decision in the 84.80 entry. -> check: tests.
  5. Controller action per the contract. -> check: controller tests.
- Contract: `POST api/events/registrations/{id:int}/cancel`, any signed-in member, no body.
  204; 403 not the owner; 404 unknown; 409 already cancelled or rejected.
- Tests: `EventServiceTests.cs` :: `Cancel_FullEvent_PromotesOldestWaitlisted`,
  `Cancel_EmptyWaitlist_OnlyCancels`, `Cancel_PaidEvent_PromotesToPending`,
  `Cancel_PaidRegistration_NotifiesTreasurer`, `Cancel_ByAnotherMember_IsForbidden`,
  `Cancel_Twice_IsConflict`, `Cancel_FreesThePlace`; `EventsControllerTests.cs` ::
  `Cancel_NotOwner_Returns403`, `Cancel_Twice_Returns409`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~EventServiceTests|FullyQualifiedName~EventsControllerTests"`;
  `python docs/api/authz_catalog.py --write </dev/null`; `python scripts/ci-local.py --only backend`;
  `python scripts/ci-local.py --only contract`.
- Done when: the nine tests pass; registry updated; 84.80 notes "backend done, clients in S13".
- Stop and ask if: a client `switch` or map over the status breaks on the new value (grep
  `Waitlisted` in `GHCAA.Web/src` and `GHCAA.Mobile/lib`). Default: add the `Cancelled` label there
  in S13 and list the files in the tracker. Also stop if no treasurer role constant exists in
  `Constants.Roles`; default is to notify `Admin`.

<a id="s13"></a>
**S13 · 84.80 (clients) · M**
- Goal: a member cancels a registration from the web and from mobile, after a confirm, and the list
  and counts refresh.
- Before you start: S12 closed. The cancel route answers on the local API.
- Read (only these): `GHCAA.Web/src/app/common/events/events.ts` (it calls `getMyRegistrations`);
  `GHCAA.Web/src/app/core/services/events.service.ts`; `GHCAA.Web/src/app/common/confirm-dialog/`;
  `GHCAA.Mobile/lib/features/events/events_service.dart`;
  `GHCAA.Mobile/lib/screens/member/events_screen.dart`, `event_details_screen.dart`;
  `GHCAA.Web/tests/e2e/event-walkthrough/README.md`.
- Change (only these files): web `events.service.ts` and its spec, `common/events/events.ts` with
  its template and spec; mobile `events_service.dart`, `events_screen.dart`,
  `event_details_screen.dart`, `GHCAA.Mobile/test/events_cancel_registration_test.dart` (new);
  `GHCAA.Web/tests/e2e/event-walkthrough/event.walkthrough.ts` and `README.md`; `docs/TODO.md`.
- Do not touch: admin event pages; the register flow; golden images (run mobile tests with
  `--exclude-tags golden`).
- Steps:
  1. Web service `cancelRegistration(id)`; mobile service `cancelRegistration(id)`. -> check:
     service spec and analyzer.
  2. Web: a Cancel action on the member's own registrations with status Pending, Approved or
     Waitlisted. It opens the confirm dialog; on yes it calls the service, shows a toast and reloads
     the registrations and the event counts. Show a "Cancelled" label for the new status.
     -> check: component spec.
  3. Mobile: the same action on the event the member is registered for, with a confirm dialog;
     errors go through the shared `serverMessage` helper. -> check: widget test.
  4. Event walkthrough: fill an event, add one waitlisted member, cancel from the first member's
     registrations, check the waitlisted one moved up; remove the waitlist gap line and update "What
     the run covers". -> check: `scripts\run-event-walkthrough.bat` is green.
  5. Browser check in light and dark. -> check: action, dialog and label show.
- Contract: none new (uses S12's route).
- Tests: `events.spec.ts` :: "cancel asks for a confirm", "cancel reloads registrations", "no cancel
  action on a rejected or cancelled registration"; `events_cancel_registration_test.dart` :: "confirm
  then cancel calls the service", "dismiss does not call the service".
- Run: in `GHCAA.Web`: `npm run type-check; npm run test:unit -- src/app/common/events`; in
  `GHCAA.Mobile`: `flutter analyze; flutter test test/events_cancel_registration_test.dart --exclude-tags golden`;
  then `python scripts/ci-local.py --only web` and `--only mobile`.
- Done when: the walkthrough is green with the cancel step; 84.80 DONE.
- Stop and ask if: mobile has no place that shows the member's own registrations. Default: put the
  action on `event_details_screen.dart` when the event's own status says the member is registered.

<a id="s14"></a>
**S14 · 84.81 · L · migration 3**
- Goal: a daily job sends one reminder for each unpaid due and each approved event registration
  inside a lead window set in OrgConfig, and never sends the same reminder twice.
- Before you start: S13 closed (it adds `Cancelled`, which this job must skip). Migration 2 of the
  ledger (section 7) is applied locally.
- Read (only these): `GHCAA.API/Services/ExpiredRevokeSweep.cs` (the pattern to copy, and where it
  is registered in `Program.cs`); `GHCAA.Domain/Models/MembershipDue.cs`, `EventRegistration.cs`;
  `WorkflowDto` in `GHCAA.Application/DTOs/OrgConfigDto.cs` (line 144); `TemplateCodes` in
  `GHCAA.Domain/Constants.cs` (line 80); the newest migration `20261112120000_AddElectionFormPrints`
  with its `.Designer.cs` as the shape to copy; memory `gotcha_orgconfig_feature_flag_default_drift`.
- Change (only these files): `MembershipDue.cs`, `EventRegistration.cs`;
  `GHCAA.Infrastructure/Data/Migrations/PgSql/<timestamp>_AddReminderSentMarks.cs` (new) with its
  `.Designer.cs` (new) and `PgSqlApplicationDbContextModelSnapshot.cs`; `OrgConfigDto.cs`;
  `GHCAA.API/Services/ReminderSweep.cs` (new); `GHCAA.API/Program.cs` (one registration line);
  `GHCAA.Domain/Constants.cs` (template code, only if missing);
  `GHCAA.Tests/Services/ReminderSweepTests.cs` (new);
  `GHCAA.Tests/OrgConfig/golden/org-config-ghc.golden.json`; `docs/CONFIG_DRIVEN_FRAMEWORK.md`;
  `docs/TODO.md`; the ledger row in section 7.
- Do not touch: `Seed/*.json`; `appsettings*.json`; `ExpiredRevokeSweep.cs`; push notifications
  (84.61 adds them later).
- Steps:
  1. Add `DateTime? ReminderSentAt` to both models. -> check: build.
  2. Write the migration by hand: two nullable `timestamp with time zone` columns, `Down` drops
     them. Copy the `.Designer.cs` from the model and update the snapshot. Never run
     `ef migrations add` then `remove`. -> check: a fresh local PostgreSQL database migrates, and
     the migration reruns clean (rule 5).
  3. `WorkflowDto`: `DueReminderLeadDays` (int, default 14, `[Range(0, 365)]`) and
     `EventReminderLeadDays` (int, default 2, `[Range(0, 60)]`); 0 turns that reminder off. Update
     the golden as its test says, and `CONFIG_DRIVEN_FRAMEWORK.md`. -> check: the golden test passes.
  4. `ReminderSweep`: a hosted service that runs once a day like `ExpiredRevokeSweep`, with the
     work in a public method `RunOnceAsync(DateTime utcNow, CancellationToken)` so tests call it.
     Dues: `IsPaid == false`, `ReminderSentAt == null`, `DueDate` from today to today plus the lead
     days. Registrations: `Approved`, `ReminderSentAt == null`, event start in the same kind of
     window. Send the email, then set `ReminderSentAt`; save after each item so a crash does not
     resend the ones already sent. A failed send leaves the mark empty. -> check: tests.
  5. Register it in `Program.cs` beside `ExpiredRevokeSweep`. -> check: the API starts.
- Contract: no route. `WorkflowDto` gains two fields; record them in `docs/API_CONTRACT_REGISTRY.md`.
- Tests: `ReminderSweepTests.cs` :: `Due_DayBeforeWindow_NoMail`, `Due_FirstDayOfWindow_OneMail`,
  `Due_LastDayOfWindow_OneMail`, `Due_Paid_NoMail`, `SecondRun_SendsNothing`, `LeadZero_SendsNothing`,
  `Event_ApprovedInsideWindow_OneMail`, `Event_PendingOrCancelled_NoMail`,
  `FailedSend_LeavesMarkEmpty`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ReminderSweepTests|FullyQualifiedName~OrgConfigGoldenSnapshotTests"`;
  `python scripts/ci-local.py --only backend`; `python scripts/ci-local.py --only contract`; then
  the code-reviewer pass on the migration.
- Done when: the nine tests pass; the migration dry run is recorded; 84.81 DONE; ledger row 3 marked.
- Stop and ask if: no email template for a due or an event reminder exists in the database seed.
  Default: reuse `FEE_REMINDER` for dues, and for events send through the existing generic
  notification email; do not edit `Seed/*.json`. Also stop if the golden has no regeneration path.

<a id="s15"></a>
**S15 · 88.1 · L · migration 4**
- Goal: `MembershipNumber` is unique in the database, the import takes `.csv` beside `.xlsx`, and a
  rerun adds no second academic, professional, payment, user or role row.
- Before you start: S14 closed (migration order). Run on local PostgreSQL:
  `SELECT "MembershipNumber", count(*) FROM "Members" WHERE "MembershipNumber" IS NOT NULL GROUP BY 1 HAVING count(*) > 1;`
  and record the result in the 88.1 entry.
- Read (only these): `docs/specs/008-idempotent-member-batch-import/spec.md` FR-1 to FR-3 and
  `plan.md`; `GHCAA.Infrastructure/Services/MemberImportService.cs`;
  `GHCAA.API/Controllers/MemberImportController.cs`; `GHCAA.Application/DTOs/MemberImportDtos.cs`;
  `GHCAA.Infrastructure/Data/Configurations/MemberConfiguration.cs`.
- Change (only these files): `MemberConfiguration.cs`;
  `GHCAA.Infrastructure/Data/Migrations/PgSql/<timestamp>_AddMembershipNumberUniqueIndex.cs` (new)
  with its `.Designer.cs` (new) and `PgSqlApplicationDbContextModelSnapshot.cs`;
  `MemberImportService.cs`, `MemberImportController.cs`;
  `GHCAA.Tests/Services/MemberImportServiceTests.cs`;
  `docs/specs/008-idempotent-member-batch-import/tasks.md` (tick boxes); `docs/TODO.md`; ledger row 4.
- Do not touch: the response DTO (S16 changes it); the transaction scope (S16); the column map of
  the `.xlsx` path; `Seed/*.json`.
- Steps:
  1. `MemberConfiguration.cs`: unique index on `MembershipNumber`, filtered to non-null values.
     -> check: build.
  2. Migration by hand. `Up` starts with a `DO` block that raises an exception listing the
     duplicate numbers when any exist, then creates the filtered unique index. `Down` drops the
     index. -> check: on a database with a planted duplicate the migration stops with the list; on a
     clean one it applies and reruns clean (rule 5).
  3. Accept `.csv`: same headers and column map as `.xlsx`, UTF-8 with or without BOM, quoted
     fields. An unknown extension gives 400. -> check: tests.
  4. For each member, insert each related row only when no row with the natural key from spec 008
     exists. AcademicRecord, ProfessionalRecord, PaymentHistory, User and UserRoles are each
     checked. -> check: tests.
- Contract: `POST api/admin/members/import` now accepts `.csv` as well; same response. Record it in
  `docs/API_CONTRACT_REGISTRY.md`.
- Tests: `MemberImportServiceTests.cs` :: `Import_Csv_SameResultAsXlsx`,
  `Import_CsvWithBomAndQuotedCommas_Parses`, `Rerun_DoesNotDuplicateAcademicRecords`,
  `Rerun_DoesNotDuplicateProfessionalRecords`, `Rerun_DoesNotDuplicatePaymentHistory`,
  `Rerun_DoesNotDuplicateUsersOrRoles`; `MemberImportControllerTests.cs` ::
  `Import_UnknownExtension_Returns400`. Tag `[Category("FR-1")]` to `FR-3` of spec 008 as fits.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~MemberImport"`;
  `python scripts/ci-local.py --only backend`; then the code-reviewer pass on the migration.
- Done when: the seven tests pass; both migration runs are recorded; 88.1 DONE; ledger row 4 marked.
- Stop and ask if: the local or preprod data has duplicate membership numbers, or spec 008 names no
  natural key for one of the five tables. Default for the first: stop; the owner decides which row
  keeps the number. For the second: stop and ask; do not invent a key.

<a id="s16"></a>
**S16 · 88.2, 88.3 · M**
- Goal: one import run is one transaction, and the response reports insert, update and skip counts
  for each table.
- Before you start: S15 closed.
- Read (only these): `MemberImportService.cs`; `MemberImportResultDto` in `MemberImportDtos.cs`;
  the use of `CreateExecutionStrategy` in `GHCAA.Infrastructure/Services/EventService.cs` as the
  pattern; spec 008 `spec.md` FR-4 and FR-5.
- Change (only these files): `MemberImportService.cs`, `MemberImportDtos.cs`,
  `GHCAA.Tests/Services/MemberImportServiceTests.cs`;
  `GHCAA.Web/src/app/admin/members/member-import-modal/member-import-modal.component.ts` and its
  template (show the counts); `docs/API_CONTRACT_REGISTRY.md`;
  `docs/specs/008-idempotent-member-batch-import/tasks.md`; `docs/TODO.md`.
- Do not touch: the natural keys from S15; the controller route; mobile (it has no import screen).
- Steps:
  1. Wrap the whole run in `CreateExecutionStrategy().ExecuteAsync` with one transaction; any
     exception rolls everything back. No retry inside the transaction (memory
     `gotcha_election_audit_writer_transactions`). -> check: the failure test.
  2. Add `Tables` to `MemberImportResultDto`: a list of `{ table, inserted, updated, skipped }`,
     one entry for each of the six tables. Keep the existing fields. -> check: tests.
  3. The web modal shows the counts in a small table. -> check: `npm run type-check`; browser check.
- Contract: `POST api/admin/members/import` response gains `tables: [{ table, inserted, updated,
  skipped }]`. Existing fields stay.
- Tests: `MemberImportServiceTests.cs` :: `ImportTwice_SecondRunInsertsNothing_AndRowCountsMatch`
  (counts all six tables before and after), `FailureMidBatch_LeavesNoRows`,
  `Result_ReportsCountsPerTable`. Tag FR-4 and FR-5.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~MemberImport"`; in
  `GHCAA.Web`: `npm run type-check`; then `python scripts/ci-local.py --only backend`, `--only web`,
  `--only contract`.
- Done when: the three tests pass; registry updated; 88.2 and 88.3 DONE.
- Stop and ask if: the service calls another service that opens its own transaction. Default: move
  that work behind a method that takes no transaction, and name it in the tracker.

<a id="s17"></a>
**S17 · 88.5 (backend) · M**
- Goal: a preview call runs the whole import check and writes nothing, and commit refuses a file
  that still has errors.
- Before you start: S16 closed.
- Read (only these): `MemberImportController.cs`, `MemberImportService.cs`, `MemberImportDtos.cs`.
- Change (only these files): `MemberImportController.cs`,
  `GHCAA.Application/Interfaces/IMemberImportService.cs`, `MemberImportService.cs`,
  `MemberImportDtos.cs`, `GHCAA.Tests/Services/MemberImportServiceTests.cs`,
  `GHCAA.Tests/Controllers/MemberImportControllerTests.cs`, `docs/API_CONTRACT_REGISTRY.md`, the
  authz catalog output, `docs/TODO.md`.
- Do not touch: web (S18); the import transaction from S16.
- Steps:
  1. `MemberImportPreviewDto`: `{ columns: [{ key, header }], rows: [{ rowIndex, cells }],
     errors: [{ rowIndex, columnKey, message }], canCommit }`. `rowIndex` counts from 1 on the data
     rows; `columnKey` is the column map key. -> check: build.
  2. `PreviewAsync`: the same mapping, validation and natural-key checks as the import, sharing the
     code and not copying it, with no `SaveChanges`. -> check: the no-write test.
  3. Controller action per the contract. -> check: controller tests.
  4. The commit path runs the same checks first and returns 400 with the same `errors` list when
     any exist. -> check: test.
- Contract: `POST api/admin/members/import/preview`, same policy and form field as the import route.
  200 with `MemberImportPreviewDto`; 400 for an unknown extension or an empty file. The import route
  now gives 400 with `{ errors }` for an invalid file.
- Tests: `MemberImportServiceTests.cs` :: `Preview_WritesNothing` (row counts equal before and
  after), `Preview_CellError_CarriesRowAndColumn`, `Preview_ValidFile_CanCommit`,
  `Import_FileWithErrors_IsRefused`; `MemberImportControllerTests.cs` :: `Preview_UsesImportPolicy`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~MemberImport"`;
  `python docs/api/authz_catalog.py --write </dev/null`; `python scripts/ci-local.py --only backend`;
  `python scripts/ci-local.py --only contract`.
- Done when: the five tests pass; registry updated; 88.5 notes "backend done, grid in S18".
- Stop and ask if: validation today happens only while rows are saved, so it cannot run without a
  write. Default: run the import inside a transaction that is always rolled back, and say so in the
  tracker.

<a id="s18"></a>
**S18 · 88.5 (grid) · M**
- Goal: a shared grid component shows the preview with cell errors, lets the admin fix cells, and
  the import page blocks commit while an error remains.
- Before you start: S17 closed. `ls GHCAA.Web/src/app/common` shows no `data-grid` folder.
- Read (only these): `GHCAA.Web/src/app/admin/members/member-import-modal/member-import-modal.component.ts`
  and its template; one shared component such as `GHCAA.Web/src/app/common/pagination/` for the
  file layout and style; memories `gotcha_table_styles_wrapper_scoped`, `gotcha_vitest_jit_signal_inputs`.
- Change (only these files): `GHCAA.Web/src/app/common/data-grid/` (new: component, template, styles,
  spec); the import modal component, template and a spec for it (new if missing); the web service
  the modal calls for import; `docs/PROJECT_MAP.md`; `docs/TODO.md`.
- Do not touch: other tables in the app (no page is moved to the grid here); theme files; no new
  npm package.
- Steps:
  1. `DataGridComponent` (standalone). Inputs: `rows`, `columns` (`{ key, header }`), `cellErrors`
     (`{ rowIndex, columnKey, message }`), `editable` (default false). Outputs: `cellEdit`
     (`{ rowIndex, columnKey, value }`) and `commit`. A cell with an error gets an error class, the
     message as its title and `aria-invalid`. Theme tokens only. -> check: grid spec.
  2. Import modal: after file choice call preview, show the grid with `editable` true; a cell edit
     updates the local row and re-runs preview; Commit is disabled while `errors` is not empty or
     a preview is running. -> check: modal spec.
  3. Commit sends the edited rows as a `.csv` file built in the browser to the import route, so the
     server checks the same data again. -> check: modal spec.
  4. Browser check in light and dark, with a file that has two bad cells. -> check: both cells are
     marked, fixing them enables Commit.
- Contract: none new (uses S17's routes).
- Tests: `data-grid` spec :: "marks a cell that has an error", "emits cellEdit with row and column",
  "read-only grid has no inputs", "emits commit"; modal spec :: "commit disabled while errors
  remain", "commit enabled after the last error is fixed". Use `componentRef.setInput` in specs.
- Run: in `GHCAA.Web`: `npm run type-check; npm run test:unit -- src/app/common/data-grid src/app/admin/members/member-import-modal`;
  then `python scripts/ci-local.py --only web`.
- Done when: the six specs pass; the browser check is recorded; 88.5 DONE. The 90.4 entry notes it
  reuses this grid read only.
- Stop and ask if: the file is too large to edit in the browser (over 2,000 rows). Default: show the
  first 200 rows with errors first, and say how many rows are hidden.

<a id="s19"></a>
**S19 · 88.4 · S**
- Goal: row counts on a local PostgreSQL copy prove a second import of the same file adds nothing.
- Before you start: S16 closed. A local PostgreSQL database seeded from `profiles/ghc/demo-data/`.
  This runs locally; the tracker entry is changed to say so, and no preprod database is used.
- Read (only these): the 88.4 tracker entry; `GHCAA.Tools/delete_imported_members.sql` (cleanup).
- Change (only these files): `docs/TODO.md`.
- Do not touch: any code; the demo-data JSON files; preprod.
- Steps:
  1. Count rows in Members, AcademicRecords, ProfessionalRecords, PaymentHistories, Users and
     UserRoles (use the table names the database has). -> check: six numbers.
  2. Import one real-shaped file through the import route. Count again. -> check: counts grow by
     what the response reports.
  3. Import the same file again. Count again. -> check: every count equals the count after step 2,
     and the response shows zero inserts.
  4. Write the SQL and the three sets of counts in the 88.4 entry. -> check: the entry holds them.
- Contract: none.
- Tests: none added (S16 has the automated version).
- Run: the SQL above with `psql`.
- Done when: counts after run 2 equal counts after run 1; 88.4 DONE.
- Stop and ask if: a count grows on the second run. Default: 88.4 stays open, the table is named,
  and a fix goes back to S15's natural keys with the owner's yes.

### Phase C. Election sessions 3 to 6

Design reference for every card here: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`
sections F2 to F6. Where it disagrees with a card, the card wins (section 8 lists why).

Names in these cards were checked against the code on 2026-11-15. Names the older cards used that are
not in the code: `MembershipRules`, `CheckConflictAsync`, `ElectionApprovalKind`, `SetKey`,
`DestroyKey`, `ER29_PrintsTheAuditChainHead`, a "security" test category. Each card gives the real
name or marks the new one.

Every backend card in this phase ends its run with `python scripts/verify-election.py --only backend`.
Tests carry the FR or NFR category of the tracker item (for example `[Category("FR-39")]`).

<a id="s20"></a>
**S20 · 37.16f · S**
- Goal: one rule says which membership types may vote. The voter roll freeze and the constitution
  vote both call it, so they cannot drift apart.
- Before you start: no card needed. `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~FreezeVoterRoll"`
  passes.
- Read (only these): `MembershipType` in `GHCAA.Domain/Enums.cs`; `ElectionService.FreezeVoterRollAsync`
  (the type check a few lines into the method); `GovernanceService.VoteOnConstitutionAsync` (the type
  check after the member is loaded); the `Constants.Governance` group in `GHCAA.Domain/Constants.cs`.
- Change (only these files): `GHCAA.Domain/Constants.cs`, `GHCAA.Domain/MembershipRules.cs` (new),
  `ElectionService.cs`, `GovernanceService.cs`, `GHCAA.Tests/Services/MembershipRulesTests.cs` (new),
  `ElectionServiceTests.cs`, `GovernanceServiceTests.cs`, `docs/TODO.md`.
- Do not touch: who may stand as a candidate, the roll snapshot rows, any client.
- Steps:
  1. Write down the set of types each of the two checks accepts today. -> check: the two sets are
     the same (Founding, Executive, General). If not, stop (see the last part).
  2. Add `Constants.Membership.VotingTypes` (a new group, read-only list) and the static method
     `MembershipRules.IsVotingMember(MembershipType type)` that reads it. -> check: the new unit tests.
  3. Replace both inline checks with the method. Keep each refusal message as it is. -> check: the
     existing tests of both services pass unchanged.
- Contract: none.
- Tests: `MembershipRulesTests.cs` :: `IsVotingMember_EachType` (one `[TestCase]` per enum value);
  `ElectionServiceTests.cs` :: `FreezeVoterRoll_TakesOnlyVotingTypes` (one member of each type, the
  roll holds the voting ones); `GovernanceServiceTests.cs` :: `VoteOnConstitution_RefusesNonVotingType`
  (one `[TestCase]` per non-voting type).
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~MembershipRulesTests|FullyQualifiedName~ElectionServiceTests|FullyQualifiedName~GovernanceServiceTests"`;
  then `python scripts/ci-local.py --only backend`.
- Done when: no other place in `GHCAA.Infrastructure` compares `MembershipType` values to decide a
  vote (`grep -rn "MembershipType\." GHCAA.Infrastructure/Services` shows none for voting); 37.16f DONE.
- Stop and ask if: the two checks accept different types today. Default: Founding, Executive and
  General for both, and the tracker names the service whose answer changed.

<a id="s21"></a>
**S21 · 37.16g · M**
- Goal: a declared winner becomes an Executive Committee member through the same code a manual
  assignment uses, with the same checks, fields and audit row.
- Before you start: S20 closed. `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~Declare"`
  passes.
- Read (only these): `ElectionService.DeclareAsync` (the loop that adds `ECMember` rows with
  `StartDate = periodStart`, inside the declare transaction); `GovernanceService.AssignMemberToRoleAsync`
  (it opens its own Serializable transaction and uses the current time); `IGovernanceService.cs`;
  the declare tests in `ElectionServiceTests.cs`.
- Change (only these files): `GHCAA.Infrastructure/Services/EcMemberAssigner.cs` (new),
  `GovernanceService.cs`, `ElectionService.cs`, `ElectionServiceTests.cs`, `GovernanceServiceTests.cs`,
  `docs/TODO.md`.
- Do not touch: `IGovernanceService.cs` and `IElectionService.cs` (no signature changes), the
  constructor of `ElectionService` (tests build it with the context only), `AdminGovernanceController`,
  the notification a manual assignment sends.
- Steps:
  1. Move the checks and writes of `AssignMemberToRoleAsync` (seat count, duplicate seat, the
     `ECMember` row, the audit row) into `EcMemberAssigner.AssignAsync(db, periodId, memberId,
     position, startDate, reason, actorUserId, ct)`. It opens no transaction and sends no
     notification. `AssignMemberToRoleAsync` keeps its transaction and its notification and calls
     it with the current time. -> check: `GovernanceServiceTests` pass unchanged.
  2. In `DeclareAsync`, call `EcMemberAssigner.AssignAsync` for each winner with the period start,
     inside the declare transaction. Remove the direct `ECMember` insert. -> check: the new declare
     test.
  3. A refusal from the assigner (seat full, member already seated) rolls back the whole declare and
     returns the assigner's message. -> check: the refusal test.
  4. A test election still writes no `ECMember` row. -> check: the existing test for it passes.
- Contract: none. Declare keeps its route and replies; one new refusal text, added under
  `Constants.Elections.Replies`.
- Tests: `ElectionServiceTests.cs` :: `Declare_WinnerGetsSameRowAsManualAssignment` (compare every
  `ECMember` field and the audit row with one made through `AssignMemberToRoleAsync`),
  `Declare_SeatAlreadyFull_RollsBackAndSaysWhy`, `Declare_TestElection_WritesNoEcMember` (existing or
  new); `GovernanceServiceTests.cs` unchanged and green.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ElectionServiceTests|FullyQualifiedName~GovernanceServiceTests"`;
  then `python scripts/ci-local.py --only backend`.
- Done when: `DeclareAsync` holds no `new ECMember`; the tests above pass; 37.16g DONE.
- Stop and ask if: (a) the manual path does something a declared winner must not get (for example it
  ends the member's current seat in the same period). Default: the assigner takes a flag for that step
  and declare leaves it off. (b) `DeclareAsync` runs on PostgreSQL with a transaction level below
  Serializable and the seat count check can race. Default: keep the declare transaction as it is and
  record the gap in the tracker.

<a id="s22"></a>
**S22 · 37.16h (backend) · L · migration 5**
- Goal: a nominee uploads a candidate photo through the checked upload path, an official approves or
  rejects it, and voters see a nomination photo only after approval.
- Before you start: S21 closed. Migration 4 of the ledger (section 7, the migration ledger) is applied
  locally. `grep -n "PhotoPath" GHCAA.Infrastructure/Services/ElectionService.cs` shows the copy in
  `SubmitNominationAsync` and the fallback `n.PhotoPath ?? m.PhotoPath` in `GetBallotAsync` and
  `GetPublicCandidateListAsync`.
- Read (only these): `Nomination` in `GHCAA.Domain/Models/Election.cs`; `NominationDto` and
  `NominationViewDto` in `ElectionDtos.cs`; `SubmitNominationAsync`, `GetBallotAsync`,
  `GetPublicCandidateListAsync` in `ElectionService.cs`; `ProfileController.UploadPhoto` and the member
  service method it calls (the upload pattern to copy); `FileValidationExtensions.cs`;
  `ElectionsController.Scrutinise` (policy, step-up and permission to copy); `NominationConfiguration`
  in `ElectionConfigurations.cs`; `Constants.Elections.AuditActions`; `IElectionAuditWriter.cs`.
- Change (only these files): `GHCAA.Domain/Enums.cs`, `Election.cs`, `Constants.cs`, `ElectionDtos.cs`,
  `IElectionService.cs`, `ElectionService.cs`, `ElectionConfigurations.cs`, `ElectionsController.cs`;
  a new migration `AddNominationPhotoApproval` with its `.Designer.cs`, and
  `PgSqlApplicationDbContextModelSnapshot.cs`; `ElectionServiceTests.cs`, `ElectionsControllerTests.cs`;
  `docs/API_CONTRACT_REGISTRY.md`, `docs/TODO.md`, the ledger row in this plan.
- Do not touch: the member profile photo and `ProfileController`; the scrutiny decision itself
  (`DecideNominationAsync`); `Seed/*.json`; any client (S23).
- Steps:
  1. Model: enum `NominationPhotoStatus { None, Pending, Approved, Rejected }`; on `Nomination` add
     `PhotoStatus` (default `None`), `PhotoApprovedByUserId` (nullable), `PhotoApprovedAt` (nullable).
     -> check: the project builds.
  2. Migration per rule 5 of section 2 (the migration rule). It adds the three columns and sets
     `PhotoStatus` to Pending for every row that already has a `PhotoPath`. -> check: the dry run in
     rule 5 on a fresh database and on a copy with nominations.
  3. `SubmitNominationAsync` stops copying `PhotoPath` from the request. -> check: test "free path
     ignored".
  4. `SetNominationPhotoAsync(electionId, nominationId, memberId, file, ct)`: only the nominee, only
     in the Nomination phase, only while the nomination is not rejected or withdrawn. It stores the
     file the way the member photo is stored, sets `PhotoPath` and `PhotoStatus = Pending`, and clears
     the approver fields. -> check: service tests.
  5. `DecideNominationPhotoAsync(nominationId, approve, reason, actorUserId, ct)`: refuses when there
     is no pending photo; reject needs a reason; sets the status and approver fields; writes one audit
     entry (`NominationPhotoApproved` or `NominationPhotoRejected`, new in `AuditActions`) with the
     nomination id and the reason. -> check: service tests.
  6. `GetBallotAsync` and `GetPublicCandidateListAsync` show the nomination photo only when it is
     Approved, and the member's profile photo otherwise. `NominationViewDto` gains `PhotoPath` and
     `PhotoStatus` so the nominee and the officials see the state. -> check: read tests.
  7. Routes (next part) with the same file check `UploadPhoto` uses (image, 5 MB). -> check:
     controller tests.
- Contract: `POST api/elections/{id}/nominations/{nominationId}/photo` (signed-in member, multipart
  field `photo`): 200 `{ photoPath, photoStatus }`, 400 bad file or wrong phase, 403 not the nominee,
  404 unknown. `POST api/elections/nominations/{nominationId}/photo/approve` and `.../photo/reject`
  (body `{ reason }`, required on reject): `Policies.ElectionStaff`, `[RequireStepUp]`,
  `ElectionPermission.DecideNominations`; 204, 400 no reason, 404 unknown, 409 no pending photo.
  `NominationDto.PhotoPath` stays in the DTO and is ignored. Registry entry for all four changes.
- Tests: `ElectionServiceTests.cs` :: `SubmitNomination_IgnoresPhotoPathInRequest`,
  `SetNominationPhoto_NotTheNominee_Refused`, `SetNominationPhoto_OutsideNominationPhase_Refused`,
  `SetNominationPhoto_NewUploadAfterApproval_GoesBackToPending`, `DecidePhoto_RejectWithoutReason_Refused`,
  `DecidePhoto_WritesOneAuditEntry`, `Ballot_ShowsNominationPhotoOnlyWhenApproved` (pending and
  rejected fall back to the profile photo), `PublicList_ShowsNominationPhotoOnlyWhenApproved`;
  `ElectionsControllerTests.cs` :: `UploadNominationPhoto_BadFile_Returns400`,
  `RejectNominationPhoto_WithoutReason_Returns400WithTheRule`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ElectionServiceTests|FullyQualifiedName~ElectionsControllerTests"`;
  then `python scripts/ci-local.py --only backend` and `--only contract`.
- Done when: the tests pass; the migration dry run is recorded in the tracker; one `code-reviewer`
  pass on the migration (idempotent, rollback, provider); ledger row 5 filled; 37.16h PARTIAL
  (clients in S23).
- Stop and ask if: (a) the owner wants no photo at all until approval. Default: fall back to the
  member's profile photo, as the code does today. (b) `ElectionService` cannot store a file without a
  new constructor dependency. Default: add `IFileStorageService? fileStorage = null` as the last
  optional constructor parameter, so existing tests still build it with the context only.
  (c) Existing nominations with a photo belong to a declared election. Default: set those to Approved
  in the migration so published history does not change, and Pending for the rest.

<a id="s23"></a>
**S23 · 37.16h (clients) · M**
- Goal: the nominee uploads the photo on web and mobile, officials approve or reject it on web, and
  both clients show the state.
- Before you start: S22 closed. `grep -n "photo" GHCAA.Web/src/app/core/services/elections.service.ts`
  shows no upload method yet.
- Read (only these): web `election-nomination.ts` with its template and spec, `elections.service.ts`
  (`scrutinise` and the endpoint constants it uses), `election.models.ts` (`NominationViewDto`),
  `nomination-records.ts`, `officials-dashboard.ts` (where `scrutinise` is called), `profile.service.ts`
  (the upload call to copy); mobile `election_nomination_panel.dart`, `election_service.dart`,
  `officials_election_detail_screen.dart`, `election_nomination_panel_test.dart`;
  `GHCAA.Web/tests/e2e/election-walkthrough/README.md`.
- Change (only these files): web `elections.service.ts`, `election.models.ts`, `app.constants.ts`,
  `election-nomination.ts`, `officials-dashboard.ts`, each with its template and spec; mobile
  `election_service.dart`, `election_nomination_panel.dart`, `election_nomination_panel_test.dart`,
  `election_service_test.dart`; `GHCAA.Web/tests/e2e/election-walkthrough/election.walkthrough.ts` and
  `README.md`; `docs/TODO.md`.
- Do not touch: the ballot components (the server already sends the right photo), the profile photo
  screens, golden images, `scenario.json`.
- Steps:
  1. Web service: `uploadNominationPhoto(electionId, nominationId, file)`,
     `approveNominationPhoto(nominationId)`, `rejectNominationPhoto(nominationId, reason)`; models gain
     `photoPath` and `photoStatus`. -> check: service spec.
  2. Web nominate form: a file input (image, 5 MB, checked before the call), upload after the
     nomination is saved, a status label (Pending, Approved, Rejected). The free photo path field goes
     away if the form has one. -> check: component spec.
  3. Web officials dashboard: in the scrutiny list show the pending photo with Approve and Reject
     (reject asks for a reason; both go through the step-up dialog). Shown only with the
     DecideNominations permission. -> check: component spec.
  4. Mobile: the nomination panel picks an image and uploads it, and shows the status label. Errors
     go through `serverMessage`. Officials on mobile stay read-only. -> check: widget test.
  5. Walkthrough: the nominee uploads a photo, an official approves it, the ballot shows it; add the
     row to "What the run covers". -> check: `scripts\run-election-walkthrough.bat` is green.
  6. Browser check in light and dark. -> check: input, label and buttons show.
- Contract: none new (uses S22's routes).
- Tests: `election-nomination.spec.ts` :: "uploads the photo after save", "refuses a file over 5 MB
  before the call", "shows the photo status"; `officials-dashboard.spec.ts` :: "approve calls the
  service after step-up", "reject needs a reason", "no photo actions without the permission";
  `election_nomination_panel_test.dart` :: "upload calls the service", "status label shows".
- Run: in `GHCAA.Web`: `npm run type-check; npm run test:unit -- src/app/member/election src/app/officials`;
  in `GHCAA.Mobile`: `flutter analyze; flutter test test/election_nomination_panel_test.dart --exclude-tags golden`;
  then `python scripts/ci-local.py --only web` and `--only mobile`.
- Done when: the walkthrough is green with the photo steps; 37.16h DONE.
- Stop and ask if: the mobile app has no image picker package in `pubspec.yaml`. Default: reuse the
  picker the profile photo screen uses; add no new package.

<a id="s24"></a>
**S24 · 37.14d and the F2 schema · L · migration 6**
- Goal: the tables and columns that S25 to S30 need exist, an election with audit entries cannot be
  deleted, and each election can carry its own limits below the OrgConfig defaults.
- Before you start: S23 closed. Migration 5 is applied locally. `grep -n "OnDelete" GHCAA.Infrastructure/Data/Configurations/ElectionConfigurations.cs`
  shows Cascade on the `ElectionAuditEntry` foreign key.
- Read (only these): `Election.cs` (all of it); `ElectionConfigurations.cs` (`ElectionAuditEntryConfiguration`
  and one configuration to copy); the election DbSets in `ApplicationDbContext.cs`; `ElectionSettingsDto`
  in `OrgConfigDto.cs`; `CreateElectionDto`, `CreateAdminElectionRequest` and
  `UpdateElectionFormDetailsRequest` in `ElectionDtos.cs`; `ElectionService.CreateAsync`;
  `ElectionFormService.UpdateFormDetailsAsync`; `ER27C_PrintsTheAuditChainHead` in
  `ElectionDocumentServiceTests.cs`; `OrgConfigGoldenSnapshotTests.cs`.
- Change (only these files): `Election.cs`, `Enums.cs`, `ElectionConfigurations.cs`,
  `ApplicationDbContext.cs`, `OrgConfigDto.cs`, `ElectionDtos.cs`, `ElectionService.cs`,
  `ElectionFormService.cs`, `Constants.cs`; a new migration `ElectionF2` with its `.Designer.cs`, and
  `PgSqlApplicationDbContextModelSnapshot.cs`; `ElectionServiceTests.cs`, `ElectionFormServiceTests.cs`,
  `ElectionAuditWriterTests.cs`, `ElectionDtoValidationTests.cs`; `docs/API_CONTRACT_REGISTRY.md`,
  `docs/TODO.md`, the ledger row in this plan.
- Do not touch: already in the model, do not add again: `Election.RecountWindowEndsOn`,
  `Nomination.ConsentAt`, `ElectionRecountRequest`, `ElectionRecountFigure`. Also leave
  `org-config-ghc.golden.json` and `Seed/*.json` alone (see the last part), and add no service logic
  for the new tables here.
- Steps:
  1. Entities: `ElectionConflictDeclaration` (appointment id, nomination id, relation enum
     `ElectionConflictRelation { Spouse, Parent, Child, Sibling, Proposer, Seconder }`, declared by,
     declared at); `ElectionKeyCopy` (election id, holder appointment id, recorded at, confirmed at,
     destroyed at); `ElectionPaperCount` (election id, issued, unused, spoiled, entered by, entered at,
     approved by, approved at); `ElectionPurgeRecord` (election id, title, declared on, requested by,
     approved by, purged at, summary json; foreign key to `Elections` Restrict). -> check: build.
  2. Columns: `ElectionSeat.NominationsReopenedUntil` (nullable); `Election.PollingExtensionCount`
     (default 0); `Election.MaxPollingExtensions`, `Election.MaxExtensionHours`,
     `Election.NominationReopenDays` (nullable, null means the OrgConfig default). -> check: build.
  3. Configurations and DbSets for the four entities; indexes on the election id; one key copy per
     holder per election (unique). Change the `ElectionAuditEntry` foreign key from Cascade to
     Restrict. -> check: the delete test below.
  4. `ElectionSettingsDto` gains `KeyCopyCount = 2`, `MaxExtensionHours = 24`,
     `MaxPollingExtensions = 2`, `NominationReopenDays = 3`. -> check: an old `ConfigJson` row reads
     the defaults (existing pattern in the DTO).
  5. `CreateElectionDto`, `CreateAdminElectionRequest` and `UpdateElectionFormDetailsRequest` take the
     three per-election limits (nullable, DataAnnotations `[Range]` with a floor of 0 or 1).
     `CreateAsync` and `UpdateFormDetailsAsync` refuse a value above the OrgConfig default with a
     named reply in `Constants.Elections.Replies`, and store null as null. -> check: service tests.
  6. Migration per rule 5 of section 2 (the migration rule): four tables, five columns, the foreign
     key change. -> check: the dry run in rule 5, on a copy that has elections with audit entries.
- Contract: create and edit election requests gain `maxPollingExtensions`, `maxExtensionHours`,
  `nominationReopenDays` (all optional); 400 with the named reply when above the default. The
  OrgConfig election section gains four fields. Registry entries for both.
- Tests: `ElectionAuditWriterTests.cs` :: `DeleteElection_WithAuditEntries_Fails` (SQLite; the
  PostgreSQL proof is the dry run), `Chain_LastEntryRemoved_NoLongerMatchesPrintedHead`;
  `ElectionDocumentServiceTests.cs` :: `ER27C_PrintsTheAuditChainHead` still passes;
  `ElectionServiceTests.cs` :: `Create_LimitAboveOrgDefault_Refused` (one `[TestCase]` per limit),
  `Create_NullLimits_StoredAsNull`; `ElectionFormServiceTests.cs` :: `UpdateFormDetails_LimitAboveOrgDefault_Refused`;
  `ElectionDtoValidationTests.cs` :: `Limits_NegativeValue_Invalid`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~Election|FullyQualifiedName~OrgConfigGoldenSnapshotTests"`;
  then `python scripts/ci-local.py --only backend` and `--only contract`.
- Done when: the tests pass; the dry run is recorded; one `code-reviewer` pass on the migration; ledger
  row 6 filled; 37.14d DONE.
- Stop and ask if: (a) `OrgConfigGoldenSnapshotTests` fails because the golden file lacks the four new
  fields. The golden file is JSON data and is not edited without the owner's yes. Default: the owner
  approves a refresh through the test's own refresh path. (b) Existing data has an election that was
  deleted while its audit entries stayed, so the Restrict key cannot be added. Default: stop, list the
  rows, change nothing.

<a id="s25"></a>
**S25 · 37.16e · M**
- Goal: an official with a declared tie to a candidate cannot act on that candidate, a candidate
  cannot hold an appointment in the same election, and each refusal carries a named code.
- Before you start: S24 closed (it adds `ElectionConflictDeclaration`). `grep -n "ConflictDeclaration" GHCAA.Infrastructure/Services/*.cs`
  gives nothing.
- Read (only these): `ElectionAccessService.cs` and `IElectionAccessService.cs`;
  `SubmitNominationAsync`, `AddCandidateAsync`, `DecideNominationAsync` in `ElectionService.cs`;
  `AppointAsync` and `AcceptAsync` in `ElectionAppointmentService.cs`; `Constants.Elections.Replies`;
  tracker entry 37.16e; `docs/Elections/00-Election-Master-Overview.md` section 2 item 6, step 13,
  section 6 and the VVSG table.
- Change (only these files): `IElectionAccessService.cs`, `ElectionAccessService.cs`,
  `ElectionService.cs`, `ElectionAppointmentService.cs`, `IElectionAppointmentService.cs`,
  `ElectionDtos.cs`, `Constants.cs`, `ElectionsController.cs`; `ElectionAccessServiceTests.cs`,
  `ElectionServiceTests.cs`, `ElectionAppointmentServiceTests.cs`, `ElectionsControllerTests.cs`;
  `docs/Elections/00-Election-Master-Overview.md`, `docs/API_CONTRACT_REGISTRY.md`, `docs/TODO.md`.
- Do not touch: the permission flags in `ElectionPermission`, the persona seeder, the approval
  service, any client (S32 and S33).
- Steps:
  1. `ElectionAccessService.CheckConflictAsync(electionId, actorUserId, nominationId, ct)` (new)
     returns null or a refusal code: the actor is the candidate, the proposer or the seconder of that
     nomination, or has a declaration against it. -> check: service tests.
  2. Call it from `DecideNominationAsync` (the deciding official) and from `AddCandidateAsync`.
     `SubmitNominationAsync` refuses a nominee who holds a live appointment in the election.
     `AppointAsync` and `AcceptAsync` refuse a person who is a candidate, proposer or seconder in the
     election. Codes live in a new group `Constants.Elections.ConflictErrors`. -> check: one test per
     path.
  3. Declarations: `DeclareConflictAsync(appointmentId, nominationId, relation, actorUserId, ct)`.
     The official declares at acceptance and later; the Chief Commissioner (permission
     `AppointOfficials`) declares for any official. Each writes one audit entry
     (`ConflictDeclared`, new in `AuditActions`) and takes effect on the next call. -> check: tests.
  4. Remove "not built yet" for this rule in the Master Overview at the four places named above.
     -> check: `grep -n "not built" docs/Elections/00-Election-Master-Overview.md` shows none of them.
- Contract: `POST api/elections/{id}/conflicts` (body `{ appointmentId, nominationId, relation }`,
  `Policies.ElectionStaff`): 204, 400 unknown relation, 403 another official's appointment without
  `AppointOfficials`, 404 unknown. `GET api/elections/{id}/conflicts` lists them for staff. Existing
  routes gain 409 with the conflict code in the problem detail. Registry entry.
- Tests: `ElectionAccessServiceTests.cs` :: `CheckConflict_Candidate`, `_Proposer`, `_Seconder`,
  `_Declared`, `_Untied_ReturnsNull`; `ElectionServiceTests.cs` :: `DecideNomination_ConflictedOfficial_Refused`,
  `DecideNomination_UntiedOfficial_Decides`, `AddCandidate_ConflictedOfficial_Refused`,
  `SubmitNomination_AppointedOfficial_Refused`; `ElectionAppointmentServiceTests.cs` ::
  `Appoint_Candidate_Refused`, `Accept_Proposer_Refused`, `DeclareConflict_TakesEffectAtOnce`,
  `DeclareConflict_WritesAuditEntry`; `ElectionsControllerTests.cs` :: `Scrutinise_Conflict_Returns409WithTheCode`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ElectionAccessServiceTests|FullyQualifiedName~ElectionServiceTests|FullyQualifiedName~ElectionAppointmentServiceTests|FullyQualifiedName~ElectionsControllerTests"`;
  then `python scripts/ci-local.py --only backend` and `--only contract`.
- Done when: the tests pass; 37.16e PARTIAL (client screens in S32 and S33).
- Stop and ask if: an election already running has an official who is also a proposer or seconder.
  Default: the check applies from now on, existing appointments stay, and the dashboard query in S32
  lists them.

<a id="s26"></a>
**S26 · 37.1p (recount, consent, reopen) · M**
- Goal: a recount decision needs two people, a nomination without recorded consent cannot be accepted,
  and the Chief Commissioner can reopen nominations for a seat with no valid candidate.
- Before you start: S25 closed. `grep -n "ConsentAt" GHCAA.Infrastructure/Services/ElectionService.cs`
  shows no check in `DecideNominationAsync`.
- Read (only these): `RecordRecountAsync`, `DecideRecountAsync`, `RecordRecountFiguresAsync` and
  `RecordConsentAsync` in `ElectionFormService.cs`; `RunOrRequestAsync` and `NeedsSecondAsync` in
  `ElectionApprovalService.cs`; `ElectionApprovalAction` in `Enums.cs`; `DecideNominationAsync`,
  `SubmitNominationAsync` and `FreezeVoterRollAsync` in `ElectionService.cs`; the recount routes in
  `AdminElectionsController.cs`; tracker entry 37.1p.
- Change (only these files): `Enums.cs`, `Constants.cs`, `ElectionFormService.cs`,
  `ElectionApprovalService.cs`, `ElectionService.cs`, `IElectionService.cs`, `ElectionDtos.cs`,
  `AdminElectionsController.cs`; `ElectionFormServiceTests.cs`, `ElectionApprovalServiceTests.cs`,
  `ElectionServiceTests.cs`, `AdminElectionsControllerTests.cs`; `docs/API_CONTRACT_REGISTRY.md`,
  `docs/TODO.md`.
- Do not touch: one row per recount filing (37.14m built it that way); `RecordRecountFiguresAsync`;
  the phase order in `SetPhaseAsync`; `org-config-ghc.golden.json`.
- Steps:
  1. Add `DecideRecount` to `ElectionApprovalAction` (at the end, so stored numbers do not move). The
     decide route runs through `RunOrRequestAsync` like `ReplaceBallotKey` does. -> check: approval
     tests.
  2. `RecordRecountAsync` refuses a second undecided request for the same seat (409). -> check: test.
  3. `DecideNominationAsync` refuses to accept a nomination with no `ConsentAt`, with a named reply.
     Reject still works without consent. -> check: tests.
  4. `ReopenNominationsAsync(electionId, seatId, actorUserId, ct)`: permission `ManageSetup`, only
     when the seat has no accepted nomination and the phase is Scrutiny, Withdrawal or CandidateList.
     It sets `ElectionSeat.NominationsReopenedUntil` to now plus the election's reopen days (the
     OrgConfig default when null), refuses when that passes the roll freeze or the polling start, and
     writes an audit entry (`NominationsReopened`, new). `SubmitNominationAsync` accepts a nomination
     for that seat until then. -> check: tests.
  5. A seat still without a valid candidate at Polling stays vacant: it is left off the ballot and
     gets no result row. -> check: test.
- Contract: `POST api/admin/elections/{id}/seats/{seatId}/reopen-nominations` (step-up, `ManageSetup`):
  200 `{ nominationsReopenedUntil }`, 409 with the reason. The recount decide route returns 202 with
  the approval when a second person is needed, as the other two-person routes do. Seat DTOs gain
  `nominationsReopenedUntil`. Registry entry.
- Tests: `ElectionFormServiceTests.cs` :: `RecordRecount_SecondUndecidedForSeat_Refused`,
  `DecideRecount_NeedsSecondApprover`, `DecideRecount_AuditsBothSteps`; `ElectionServiceTests.cs` ::
  `DecideNomination_AcceptWithoutConsent_Refused`, `DecideNomination_RejectWithoutConsent_Allowed`,
  `ReopenNominations_SeatWithNoValidCandidate_Opens`, `ReopenNominations_SeatWithCandidate_Refused`,
  `ReopenNominations_PastRollFreeze_Refused`, `SubmitNomination_ReopenedSeat_Accepted`,
  `Ballot_EmptySeatAtPolling_IsVacant`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ElectionFormServiceTests|FullyQualifiedName~ElectionApprovalServiceTests|FullyQualifiedName~ElectionServiceTests|FullyQualifiedName~AdminElectionsControllerTests"`;
  then `python scripts/ci-local.py --only backend` and `--only contract`.
- Done when: the tests pass; 37.1p stays PARTIAL with this part ticked (the rest is in S29).
- Stop and ask if: (a) `TwoPersonActions` in a saved OrgConfig row does not list `DecideRecount`, so
  the step runs with one person on old data. Default: `NeedsSecondAsync` treats `DecideRecount` as
  always two-person, like `EmergencyRevoke`. (b) The election walkthrough accepts a nomination before
  consent. Default: add the consent step to the walkthrough in S32 and keep the rule.

<a id="s27"></a>
**S27 · 37.14a · M · security**
- Goal: the ballot key has named holders and recorded copies, each holder confirms their own copy,
  and polling cannot open until every copy is confirmed.
- Before you start: S26 closed. `grep -rn "ElectionKeyCopy" GHCAA.Infrastructure/Services` gives
  nothing.
- Read (only these): `SetBallotKeyAsync`, `RecordKeyDestroyedAsync`, `GetReadinessAsync`,
  `ReadinessOfAsync` and `SetPhaseAsync` in `ElectionService.cs`; `ElectionReadinessDto` and
  `ElectionReadinessItemDto` in `ElectionDtos.cs`; `Constants.Elections.Readiness`; the ballot key
  route in `AdminElectionsController.cs` and the key-destroyed route in `ElectionsController.cs`;
  `ElectionAppointment` in `Election.cs`; `docs/Elections/11-Election-Key-Procedure.md`; tracker
  entry 37.14a.
- Change (only these files): `ElectionService.cs`, `IElectionService.cs`, `ElectionDtos.cs`,
  `Constants.cs`, `AdminElectionsController.cs`, `ElectionsController.cs`; `ElectionServiceTests.cs`,
  `AdminElectionsControllerTests.cs`, `ElectionsControllerTests.cs`; `docs/API_CONTRACT_REGISTRY.md`,
  `docs/Elections/11-Election-Key-Procedure.md`, `docs/TODO.md`.
- Do not touch: how the key is made or encrypted; the `ReplaceBallotKey` approval; the unique index
  `IX_ElectionAuditEntries_KeyDestroyed_Once`; the private key never reaches the server and no step
  here changes that.
- Steps:
  1. `SetBallotKeyAsync` takes the holder appointment ids. It refuses unless the count equals the
     OrgConfig `KeyCopyCount`, each holder has a live accepted appointment in this election, none is
     a candidate, proposer or seconder, and all are distinct. It writes one `ElectionKeyCopy` row per
     holder with `RecordedAt`. Replacing the key removes the old rows. -> check: tests.
  2. `ConfirmKeyCopyAsync(electionId, actorUserId, ct)`: only the holder, only their own copy, once.
     Sets `ConfirmedAt` and writes an audit entry (`KeyCopyConfirmed`, new). -> check: tests.
  3. Readiness gains the item `KeyCopies`, met when every copy is confirmed. `ReadinessOfAsync`
     counts it, so `SetPhaseAsync` and `IsReadyForPollingAsync` refuse Polling without it.
     -> check: tests.
  4. `RecordKeyDestroyedAsync` sets `DestroyedAt` on every copy of the election in the same save.
     -> check: test.
  5. Update the key procedure document: holders, confirmation, what blocks polling. -> check: the
     steps there match the routes.
- Contract: `POST api/admin/elections/{id}/ballot-key` body gains `holderAppointmentIds`; 400 with a
  named reply for each refusal. `POST api/elections/{id}/key-copy/confirm` (`Policies.ElectionStaff`,
  step-up): 204, 403 not a holder, 409 already confirmed. `GET api/elections/{id}/key-copies` (staff)
  returns holder name, recorded, confirmed and destroyed times. The readiness reply gains one item.
  Registry entry.
- Tests: `ElectionServiceTests.cs` :: `SetBallotKey_WrongHolderCount_Refused`,
  `SetBallotKey_HolderIsCandidate_Refused`, `SetBallotKey_SameHolderTwice_Refused`,
  `SetBallotKey_HolderWithoutLiveAppointment_Refused`, `SetBallotKey_Replace_ClearsOldCopies`,
  `ConfirmKeyCopy_NotAHolder_Refused`, `ConfirmKeyCopy_Twice_Refused`,
  `Readiness_ListsKeyCopiesUntilAllConfirmed`, `SetPhase_Polling_RefusedUntilCopiesConfirmed`,
  `KeyDestroyed_MarksEveryCopyDestroyed`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ElectionServiceTests|FullyQualifiedName~AdminElectionsControllerTests|FullyQualifiedName~ElectionsControllerTests"`;
  then `python scripts/ci-local.py --only backend` and `--only contract`.
- Done when: the tests pass; one `security-reviewer` pass on the diff; 37.14a PARTIAL (screens in S31
  and S33).
- Stop and ask if: (a) the web and mobile key screens send no holders yet, so setting a key fails
  between this card and S31. Default: accept that on the working branch and run S31 and S33 before
  any release; the election walkthrough is updated in S31. (b) An election in Polling or later has a
  key and no copy rows. Default: readiness counts `KeyCopies` only for elections that have not
  reached Polling.

<a id="s28"></a>
**S28 · 37.12y · M · security**
- Goal: polling can be extended only by a request from the Chief Commissioner plus a second approval,
  within the election's limits, with one notice to voters and an audit entry for each step.
- Before you start: S27 closed. `grep -n "PollingExtension" GHCAA.Infrastructure/Services/*.cs` gives
  nothing.
- Read (only these): `ElectionApprovalService.cs` (`RunOrRequestAsync`, `RequestAsync`, `ApproveAsync`,
  `NeedsSecondAsync`); `ElectionApprovalAction`; the readers of `PollingClosesOn` in
  `ElectionService.cs`, `ElectionDocumentService.cs` and `ElectionFormService.cs`;
  `INotificationService.cs` and `NotificationType`; tracker entry 37.12y (its refusal cases);
  `docs/Elections/12-Election-Officials-Handbook.md` near "L1 to L3".
- Change (only these files): `Enums.cs`, `Constants.cs`, `ElectionService.cs`, `IElectionService.cs`,
  `ElectionApprovalService.cs`, `ElectionDtos.cs`, `AdminElectionsController.cs`;
  `ElectionServiceTests.cs`, `ElectionApprovalServiceTests.cs`, `AdminElectionsControllerTests.cs`;
  `docs/API_CONTRACT_REGISTRY.md`, `docs/Elections/12-Election-Officials-Handbook.md`, `docs/TODO.md`.
- Do not touch: `PollingOpensOn`; ballots already cast; the count; the `ClosePolling` approval.
- Steps:
  1. Add `ExtendPolling` to `ElectionApprovalAction` (at the end). It always needs a second person,
     like `EmergencyRevoke`. -> check: approval test.
  2. `RequestPollingExtensionAsync(electionId, hours, reason, actorUserId, ct)`: only in Polling, only
     before `PollingClosesOn`, hours from 1 to the election's `MaxExtensionHours` (OrgConfig default
     when null), `PollingExtensionCount` below `MaxPollingExtensions`, reason required, permission
     `ChangePhase`. Each refusal has a named reply. -> check: one test per refusal on the tracker.
  3. On approval, in one transaction: move `PollingClosesOn` by the hours, add one to
     `PollingExtensionCount`, write audit entries for the request and the approval
     (`PollingExtensionRequested`, `PollingExtensionApproved`, new). After the save, send one
     broadcast to the voters on the frozen roll. A failed notice does not undo the extension and is
     logged. -> check: tests.
  4. A request when the count is used up is refused and the reply names the handbook's void-and-rerun
     procedure. Add that procedure as a short named section in the handbook next to "L1 to L3" (the
     handbook has no such section today). -> check: the reply text and the heading match.
  5. Every reader of `PollingClosesOn` uses the stored value, so forms and the voting window follow
     the new time. -> check: test `Vote_AfterOldCloseWithinExtension_Accepted`.
- Contract: `POST api/admin/elections/{id}/polling-extension` (body `{ hours, reason }`, step-up,
  `ChangePhase`): 202 with the approval; 400 or 409 with the named reply. Approval goes through the
  existing approve route, with step-up. Election DTOs gain `pollingExtensionCount` and the three
  limits. Registry entry.
- Tests: `ElectionServiceTests.cs` :: `ExtendPolling_NotInPolling_Refused`, `_AfterClose_Refused`,
  `_HoursAboveLimit_Refused`, `_CountUsedUp_RefusedAndNamesTheProcedure`, `_NoReason_Refused`,
  `_Approved_MovesCloseAndCounts`, `_Approved_SendsOneNotice`, `_Approved_AuditsBothSteps`,
  `Vote_AfterOldCloseWithinExtension_Accepted`; `ElectionApprovalServiceTests.cs` ::
  `ExtendPolling_AlwaysNeedsSecond`, `ExtendPolling_SamePersonCannotApprove`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ElectionServiceTests|FullyQualifiedName~ElectionApprovalServiceTests|FullyQualifiedName~AdminElectionsControllerTests"`;
  then `python scripts/ci-local.py --only backend` and `--only contract`.
- Done when: the tests pass; one `security-reviewer` pass; 37.12y PARTIAL (screens in S31).
- Stop and ask if: (a) `NotificationType` has no value that fits and a new one would change both
  clients. Default: use `GeneralSystem`. (b) The approval arrives after the old close time has passed.
  Default: refuse it, since polling has closed, and say so in the reply.

<a id="s29"></a>
**S29 · 37.1p (unopposed, paper counts) · M · security**
- Goal: a seat with one valid candidate is declared unopposed without a vote, an election with no
  contested seat is declared through a two-person step, and paper ballot counts are entered, approved
  and printed.
- Before you start: S28 closed. `grep -rn "Unopposed\|ElectionPaperCount" GHCAA.Infrastructure/Services`
  gives nothing.
- Read (only these): `GetBallotAsync`, `GetSeatsAsync`, `CountAsync`, `StoredResultsAsync`,
  `DeclareAsync`, `GetPublicResultsAsync` and `SetPhaseAsync` in `ElectionService.cs`; `ElectionResult`
  and `ElectionSeat` in `Election.cs`; `PollOpenCloseTableAsync` and `PollRecordTableAsync` in
  `ElectionDocumentService.cs`; `ElectionApprovalService.RunOrRequestAsync`; tracker entry 37.1p.
- Change (only these files): `Enums.cs`, `Constants.cs`, `ElectionService.cs`, `IElectionService.cs`,
  `ElectionDocumentService.cs`, `ElectionApprovalService.cs`, `ElectionDtos.cs`,
  `AdminElectionsController.cs`, `ElectionsController.cs`; `ElectionServiceTests.cs`,
  `ElectionDocumentServiceTests.cs`, `ElectionApprovalServiceTests.cs`; `docs/API_CONTRACT_REGISTRY.md`,
  `docs/TODO.md`.
- Do not touch: the tally of online ballots; the tie rule; the `ElectionResult` table shape (unopposed
  is worked out, not stored: a seat is unopposed when it has one accepted nomination at CandidateList).
- Steps:
  1. `GetBallotAsync` leaves out a seat with exactly one accepted nomination. `CountAsync` writes a
     result row for that candidate with `VoteCount = 0` and `IsElected = true`. Result DTOs and
     `GetPublicResultsAsync` gain `IsUnopposed`. -> check: tests.
  2. All seats unopposed or vacant: add `DeclareUncontested` to `ElectionApprovalAction` (at the end,
     always two-person). On approval the election goes from CandidateList to Declared, writes the
     result rows and runs the same winner assignment as `DeclareAsync` (S21). The route refuses when
     any seat is contested. -> check: tests.
  3. Paper counts: `RecordPaperCountAsync(electionId, issued, unused, spoiled, actorUserId, ct)`
     (permission `Count`, phase Polling or Counting) refuses negative numbers and unused plus spoiled
     above issued. `ApprovePaperCountAsync` (permission `Declare`, a different person) sets the
     approver fields. A new entry replaces an unapproved one and is refused after approval. Each
     writes an audit entry. -> check: tests.
  4. `CountAsync` refuses to run when a paper count is entered and not approved. With none entered it
     runs as today. -> check: tests.
  5. `PollOpenCloseTableAsync` prints issued, used (issued minus unused minus spoiled), unused and
     spoiled on the closing form (ER-24) when an approved count exists. -> check: document test.
- Contract: `POST api/elections/{id}/paper-count` (body `{ issued, unused, spoiled }`, step-up) 204;
  `POST api/elections/{id}/paper-count/approve` 204, 409 same person or nothing to approve;
  `GET api/elections/{id}/paper-count`; `POST api/admin/elections/{id}/declare-uncontested` 202 with
  the approval, 409 when a seat is contested. Result DTOs gain `isUnopposed`. Registry entry.
- Tests: `ElectionServiceTests.cs` :: `Ballot_LeavesOutUnopposedSeat`, `Count_UnopposedSeat_ElectedWithZeroVotes`,
  `PublicResults_MarkUnopposed`, `DeclareUncontested_ContestedSeat_Refused`,
  `DeclareUncontested_NeedsSecondApprover`, `DeclareUncontested_AssignsWinnersLikeDeclare`,
  `PaperCount_PartsAboveIssued_Refused`, `PaperCount_ApprovedBySamePerson_Refused`,
  `PaperCount_ChangeAfterApproval_Refused`, `Count_UnapprovedPaperCount_Refused`;
  `ElectionDocumentServiceTests.cs` :: `ER24_PrintsApprovedPaperCounts`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ElectionServiceTests|FullyQualifiedName~ElectionDocumentServiceTests|FullyQualifiedName~ElectionApprovalServiceTests"`;
  then `python scripts/ci-local.py --only backend` and `--only contract`.
- Done when: the tests pass; one `security-reviewer` pass; 37.1p backend complete, PARTIAL until S32
  and S33.
- Stop and ask if: (a) the regulations ask for a confirming vote on an unopposed seat
  (`docs/Elections/01-Election-Regulations.md`). Default: no vote, declared unopposed, as the tracker
  says. (b) Paper votes must be added to the online tally. Default: no, the paper count is a
  reconciliation record only and the tally stays online ballots.

<a id="s30"></a>
**S30 · 37.14f · M · security**
- Goal: no job ever deletes election rows, and ballot data of an election declared more than ten
  years ago can be purged only by a request and a second approver, leaving the result and a record.
- Before you start: S29 closed (S24 added `ElectionPurgeRecord`). `grep -n "Election" GHCAA.Infrastructure/Services/DataRetentionService.cs`
  shows no election table in `PurgedTables`.
- Read (only these): `DataRetentionService.cs` (`PurgedTables`, `PurgeAsync`); `DataRetentionSweep.cs`
  and `ExpiredRevokeSweep.cs`; the election DbSets in `ApplicationDbContext.cs`;
  `ElectionApprovalService.RunOrRequestAsync` and `LiveApproverCountAsync` in `ElectionAccessService.cs`;
  `Constants.Retention`; tracker entry 37.14f.
- Change (only these files): `Enums.cs`, `Constants.cs`, `ElectionService.cs`, `IElectionService.cs`,
  `ElectionApprovalService.cs`, `ElectionDtos.cs`, `AdminElectionsController.cs`;
  `DataRetentionServiceTests.cs`, `ElectionServiceTests.cs`, `ElectionApprovalServiceTests.cs`,
  `AdminElectionsControllerTests.cs`; `docs/API_CONTRACT_REGISTRY.md`,
  `docs/Elections/09-Election-Security-and-Compliance.md`, `docs/TODO.md`.
- Do not touch: `DataRetentionService.PurgedTables` (no election table goes in); `Election`, seats,
  nominations and `ElectionResult` rows; the audit foreign key (Restrict since S24).
- Steps:
  1. Pin test first: `PurgedTables` names no election table, and a retention run leaves every
     election row in place. -> check: the test passes before any other change.
  2. `GetPurgeCandidatesAsync`: elections with `DeclaredOn` more than ten years ago and no
     `ElectionPurgeRecord`. The ten years is `Constants.Retention.ElectionBallotYears` (new).
     -> check: test.
  3. Add `PurgeBallotData` to `ElectionApprovalAction` (at the end, always two-person). The request
     is refused under ten years. The second approver is a live election approver, or a SuperAdmin when
     `LiveApproverCountAsync` is zero. -> check: tests.
  4. On approval, in one transaction: write the `ElectionPurgeRecord` (title, declared on, both
     people, row counts per table and the audit chain head as the summary), then delete the
     election's `BallotVotes`, `SeatVotes`, `Ballots`, `PendingBallots`, `BallotReceipts`,
     `ElectionApprovals` and `ElectionAuditEntries`, children first. -> check: tests.
  5. Describe the rule in the security and compliance document. -> check: the years and the tables
     match the code.
- Contract: `GET api/admin/elections/purge-candidates`; `POST api/admin/elections/{id}/purge`
  (step-up; 202 with the approval, 409 under ten years or already purged);
  `GET api/admin/elections/{id}/purge-record`. Registry entry.
- Tests: `DataRetentionServiceTests.cs` :: `Purge_NeverTouchesElectionTables`; `ElectionServiceTests.cs`
  :: `PurgeCandidates_OnlyOlderThanTenYears`, `Purge_UnderTenYears_Refused`,
  `Purge_RemovesBallotDataKeepsResultAndRecord`, `Purge_Twice_Refused`;
  `ElectionApprovalServiceTests.cs` :: `PurgeBallotData_AlwaysNeedsSecond`,
  `PurgeBallotData_NoCommission_SuperAdminApproves`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~DataRetentionServiceTests|FullyQualifiedName~ElectionServiceTests|FullyQualifiedName~ElectionApprovalServiceTests|FullyQualifiedName~AdminElectionsControllerTests"`;
  then `python scripts/ci-local.py --only backend` and `--only contract`.
- Done when: the tests pass; one `security-reviewer` pass; 37.14f PARTIAL (screen in S32).
- Stop and ask if: (a) the approval that authorises the purge is itself an `ElectionApprovals` row of
  that election and would be deleted with the rest. Default: delete it too; the purge record holds
  both names and times. (b) The delete must pass a table not listed here (a new election table since
  2026-11-15). Default: stop and ask whether it is ballot data or record.

<a id="s31"></a>
**S31 · F5 web part 1 · M**
- Goal: officials and admins can do on the web what S24, S27, S28 and S29 added: set per-election
  limits, name key holders and confirm copies, see what blocks polling, extend polling, and enter and
  approve paper counts.
- Before you start: S30 closed. `python scripts/verify-election.py --only web` is green.
- Read (only these): `admin-elections.ts` and `admin-elections.html` (the create form),
  `election.models.ts` (`CreateElectionRequest`, `ELECTION_PERMISSION_FLAGS`), `elections.service.ts`,
  `officials-dashboard.ts` and `officials-dashboard.html` (readiness list and key card),
  `confirm-dialog.ts`, `step-up-dialog.ts`, `WEB_SPECS` in `scripts/verify-election.py`;
  `GHCAA.Web/tests/e2e/election-walkthrough/README.md`; the Contract parts of S24, S27, S28 and S29.
- Change (only these files): `election.models.ts`, `elections.service.ts`, `app.constants.ts`,
  `admin-elections.ts`, `officials-dashboard.ts`, each with its template, styles and spec;
  `scripts/verify-election.py`; `GHCAA.Web/tests/e2e/election-walkthrough/election.walkthrough.ts`,
  `web.ts`, `api.ts` and `README.md`; `docs/TODO.md`.
- Do not touch: the ballot and voting pages; the key generation code in the browser; `scenario.json`;
  the global interceptor (refusals already show through it); theme tokens (use the ones there).
- Steps:
  1. Models and service methods for every route in the four contracts. -> check: service spec.
  2. Create and edit form: three optional number fields for the limits, with the OrgConfig default
     as the hint and the server's refusal shown. -> check: component spec.
  3. Officials dashboard, key card: choose holders from the accepted appointments when setting the
     key; list copies with recorded and confirmed times; a holder sees "Confirm my copy" (step-up).
     -> check: component spec.
  4. Readiness list: show the `KeyCopies` item with what is missing. -> check: spec.
  5. Polling extension: a request form (hours, reason) shown in Polling with the `ChangePhase`
     permission, the count used and left, and the pending approval. -> check: spec.
  6. Paper counts: entry form with the `Count` permission, approve button with `Declare`, the sum
     rule checked before the call. -> check: spec.
  7. Every action is hidden without its permission (`can(permission)`) and asks the confirm dialog
     before a step that cannot be undone. -> check: one "hidden without the permission" test each.
  8. Add each new or changed spec file to `WEB_SPECS`. Walkthrough: name holders at key set, confirm
     each copy before polling opens, enter and approve a paper count; update "What the run covers".
     -> check: `python scripts/verify-election.py --only web` and `scripts\run-election-walkthrough.bat`.
  9. Browser check in light and dark. -> check: every new control shows and reads well.
- Contract: none new (uses the routes of S24, S27, S28 and S29).
- Tests: `admin-elections.spec.ts` :: "sends null for an empty limit", "shows the refusal for a limit
  above the default"; `officials-dashboard.spec.ts` :: "key set sends the chosen holders", "only a
  holder sees confirm", "readiness shows missing copies", "extension form hidden outside polling",
  "extension request sends hours and reason", "paper count refuses parts above issued", "approve
  hidden without the permission"; `elections.service.spec.ts` :: one test per new method.
- Run: in `GHCAA.Web`: `npm run type-check; npm run test:unit -- src/app/admin/elections src/app/officials src/app/core/services/elections.service.spec.ts`;
  then `python scripts/verify-election.py --only web` and `python scripts/ci-local.py --only web`.
- Done when: the walkthrough is green with the new steps; 37.14d stays DONE; 37.14a, 37.12y and
  37.1p wait for S32 and S33.
- Stop and ask if: `officials-dashboard.ts` would pass about 400 lines of new code. Default: put the
  key copies, the extension and the paper count each in a small child component under
  `GHCAA.Web/src/app/officials/dashboard/`, each with its spec in `WEB_SPECS`.

<a id="s32"></a>
**S32 · F5 web part 2 · M**
- Goal: the web covers the rest of the new rules: recount decision by two people, reopening
  nominations, conflict declarations, the ten-year list and purge, the all-uncontested declare, and
  plain refusal messages on nominate, add candidate, appoint and accept.
- Before you start: S31 closed. `python scripts/verify-election.py --only web` is green.
- Read (only these): `election-form-records.ts` (recount and consent entry), `nomination-records.ts`,
  `election-officials.ts`, `my-appointments.ts`, `election-nomination.ts`, `admin-elections.ts`,
  `officials-dashboard.ts`, `election-results.ts`, `election.models.ts`, `elections.service.ts`,
  `global-http.interceptor.ts`; the Contract parts of S25, S26, S29 and S30;
  `GHCAA.Web/tests/e2e/election-walkthrough/README.md`.
- Change (only these files): `election.models.ts`, `elections.service.ts`, `app.constants.ts`,
  `election-form-records.ts`, `nomination-records.ts`, `election-officials.ts`, `my-appointments.ts`,
  `election-nomination.ts`, `admin-elections.ts`, `officials-dashboard.ts`, `election-results.ts`,
  each with its template and spec; `scripts/verify-election.py`;
  `GHCAA.Web/tests/e2e/election-walkthrough/election.walkthrough.ts`, `web.ts`, `api.ts` and
  `README.md`; `docs/TODO.md`.
- Do not touch: the screens S31 built, other than adding to them; the interceptor's behaviour; the
  public verify pages.
- Steps:
  1. Models and service methods for the routes of S25, S26, S29 and S30. -> check: service spec.
  2. Recount: the decide action shows "waiting for a second approver" on 202 and the open approval.
     -> check: spec.
  3. Reopen nominations: an action on a seat with no valid candidate (`ManageSetup`), showing the
     date it stays open until. -> check: spec.
  4. Conflict declaration: on accepting an appointment (`my-appointments.ts`) the official picks none
     or a nomination and a relation; later declarations from the same page; the Chief Commissioner
     declares for any official from `election-officials.ts`. Both pages list the declarations.
     -> check: specs.
  5. Consent: `nomination-records.ts` shows consent recorded or missing next to Accept, and the
     server's refusal when it is missing. -> check: spec.
  6. Purge: in `admin-elections.ts` a list of elections past ten years, a request action behind the
     confirm dialog and step-up, and the purge record once done. -> check: spec.
  7. All-uncontested declare: shown at CandidateList only when no seat is contested; results show
     "Unopposed". -> check: specs.
  8. Refusals: the conflict codes and the consent, reopen and extension replies show the server's
     message once, through the interceptor, with no second toast from the component. -> check: spec.
  9. Add new spec files to `WEB_SPECS`. Walkthrough: record consent before accept, declare one
     conflict and see the refusal, decide a recount with two people; update "What the run covers".
     -> check: `python scripts/verify-election.py --only web` and `scripts\run-election-walkthrough.bat`.
  10. Browser check in light and dark. -> check: every new control shows and reads well.
- Contract: none new.
- Tests: `election-form-records.spec.ts` :: "recount decision shows the pending approval";
  `nomination-records.spec.ts` :: "shows missing consent", "reopen action only on a seat with no
  valid candidate"; `my-appointments.spec.ts` :: "accept sends the declaration", "accept with none
  sends no declaration"; `election-officials.spec.ts` :: "declares a conflict for an official";
  `admin-elections.spec.ts` :: "lists purge candidates", "purge asks for confirm and step-up",
  "uncontested declare hidden when a seat is contested"; `election-results.spec.ts` :: "marks an
  unopposed seat".
- Run: in `GHCAA.Web`: `npm run type-check; npm run test:unit -- src/app/admin/elections src/app/officials src/app/member/election src/app/public/elections`;
  then `python scripts/verify-election.py --only web` and `python scripts/ci-local.py --only web`.
- Done when: the walkthrough is green with the new steps; 37.14f DONE; 37.16e, 37.14a, 37.12y and
  37.1p DONE once S33 covers their mobile parts.
- Stop and ask if: the card passes its size. Default: land steps 1 to 5 and 8, set the items PARTIAL
  with steps 6 and 7 named, and stop.

<a id="s33"></a>
**S33 · F6 mobile, 84.49 (B4-B5) · M**
- Goal: mobile shows the new election facts and lets a candidate and an official do their own small
  steps, and the ballot key screen can no longer leave a dialog behind or store a second key.
- Before you start: S32 closed. `python scripts/verify-election.py --only mobile` is green.
- Read (only these): `election_management_screen.dart` (`_withProgress` and the dialog with key
  `ballot-key-progress`), `election_management_screen_test.dart`, `election_service.dart`
  (`setBallotKey`), `retry_interceptor.dart`, `officials_dashboard_screen.dart`,
  `officials_election_detail_screen.dart`, `election_screen.dart`, `election_nomination_panel.dart`,
  `MOBILE_TESTS` in `scripts/verify-election.py`; `docs/AUTH_AND_DEVICE_FIX_PLAN.md` items B4 and B5;
  the Contract parts of S25, S26 and S27.
- Change (only these files): `election_service.dart`, `election_management_screen.dart`,
  `officials_dashboard_screen.dart`, `officials_election_detail_screen.dart`, `election_screen.dart`,
  `election_nomination_panel.dart`; `election_management_screen_test.dart`, `election_service_test.dart`,
  `election_screen_test.dart`, `officials_dashboard_screen_test.dart`,
  `GHCAA.Mobile/test/officials_election_detail_screen_test.dart` (new); `scripts/verify-election.py`;
  `docs/AUTH_AND_DEVICE_FIX_PLAN.md`, `docs/TODO.md`.
- Do not touch: `retry_interceptor.dart` (the key save gets its own timeout, not a global one); the
  key generation; golden images; admin-only tools that stay web only (limits, extension, paper
  counts, purge).
- Steps:
  1. 84.49 B4: the progress dialog is closed through the context its own builder gives, kept in a
     local variable, not through the screen's context. Leaving the screen while the key is being made
     leaves no dialog and throws nothing. -> check: widget test "dispose mid-key leaves no dialog".
  2. 84.49 B5: a 60 second timeout on the `setBallotKey` call only. On timeout show a message with
     Retry. Retry sends the same public key, so the server holds one key. -> check: widget test and
     service test.
  3. Service methods and models: readiness with `KeyCopies`, key copies, confirm own copy, conflict
     declarations, recount request, consent and photo status fields. -> check: service tests.
  4. Candidate: a recount request on a declared seat inside the recount window, and consent shown on
     the nomination panel. -> check: widget tests.
  5. Official: readiness and phase read-only, "Confirm my copy" for a holder, conflict declaration
     when accepting an appointment. -> check: widget tests.
  6. Refusals from the server show through `serverMessage`. -> check: one test with a conflict code.
  7. Add the new test file to `MOBILE_TESTS`. -> check: `python scripts/verify-election.py` passes
     seven of seven steps.
- Contract: none new.
- Tests: `election_management_screen_test.dart` :: "dispose mid-key leaves no dialog", "slow save
  shows the timeout and retry", "retry sends the same key"; `election_service_test.dart` :: one test
  per new method; `election_screen_test.dart` :: "recount request only inside the window", "consent
  state shows"; `officials_dashboard_screen_test.dart` :: "readiness lists missing copies";
  `officials_election_detail_screen_test.dart` :: "only a holder sees confirm", "accept sends the
  declaration".
- Run: in `GHCAA.Mobile`: `flutter analyze; flutter test test/election_management_screen_test.dart test/election_service_test.dart test/election_screen_test.dart test/officials_dashboard_screen_test.dart test/officials_election_detail_screen_test.dart --exclude-tags golden`;
  then `python scripts/verify-election.py` and `python scripts/ci-local.py --only mobile`.
- Done when: seven of seven; 37.1p, 37.16e, 37.14a and 37.12y DONE; 84.49 PARTIAL only for the check
  on a real device, which is the owner's.
- Stop and ask if: (a) the candidate recount request has no member route on the server (today's
  recount routes are under `api/admin/elections`). Default: leave the request out of mobile, keep it
  recorded by an official as now, and say so in the tracker. (b) Accepting an appointment happens
  only through the emailed link and not in the app. Default: put the declaration on the official's
  election detail screen.

### Phase D. Meetings, then the model work after it

Design reference for S34 to S41: `docs/specs/026-meetings-and-action-items/` (spec, plan, tasks), and
spec 028 (comments on polls and meetings) for the discussion service. Tick the spec 026 task boxes as
each card closes. Where the design reference disagrees with a card, the card wins.

Names in these cards were checked against the code on 2026-11-15. What the older cards and spec 026
assumed that the code does not have:

- No audit log service. Audit rows go through `IActivityService.LogActivityAsync`.
- No `ContextType` enum. The discussion service takes a string; `Constants.Discussion.ContextMeeting`
  is already there, and `DiscussionService.ResolveContextAsync` handles only `ContextPoll`.
- No SQLite migrations folder. Migrations live in `GHCAA.Infrastructure/Data/Migrations/PgSql/` and
  SQLite builds from the model.
- `INotificationService` has no method for a list of members; it has one member or everyone.
- The current EC period is the `ECPeriod` with `IsActive`, and a seat counts while `EndDate` is null
  (`NetworkingService.GetExecutiveCommitteeAsync`).
- The web has no admin page for lookups and no event detail page; events show in
  `GHCAA.Web/src/app/common/events/events.ts`. `LookupsController` already has admin-only create,
  update and delete routes.
- Mobile has no admin poll screen and no role guard on admin routes.
- `MemberSearchFilterDto` sits in `GHCAA.Application/Interfaces/INetworkingService.cs`.
- The feature catalogue is `docs/FEATURES.md`.

Meeting services are built by tests with the context and the services they call, as `PollService` is.
Tests carry the FR or NFR category of spec 026 (for example `[Category("FR-8")]`).

<a id="s34"></a>
**S34 · 84.19 (T002, T002a, T003) · L · migration 7**
- Goal: the meeting tables, enums, constants and the `MeetingType` lookup values are in the model and
  in one migration, and a poll can point at a meeting. No route yet.
- Before you start: the poll comments of spec 028 are in the code and migration 6 of the ledger (section 7,
  the migration ledger) is applied locally. `grep -rn "class Meeting" GHCAA.Domain` shows nothing.
- Read (only these): the data model part of `docs/specs/026-meetings-and-action-items/spec.md`;
  `GHCAA.Domain/Models/Poll.cs`; `Constants.Discussion` in `GHCAA.Domain/Constants.cs`;
  `GHCAA.Infrastructure/Data/Migrations/PgSql/20261105200808_AddPollDiscussion.cs` (the lookup rows
  with fixed Ids, `ON CONFLICT DO NOTHING` and the identity sequence step);
  `GHCAA.Infrastructure/Data/Configurations/LookupItemConfiguration.cs`; the `DbSet` block in
  `GHCAA.Infrastructure/Data/ApplicationDbContext.cs`; `GHCAA.Tests/Data/JobCategorySeedSyncTests.cs`.
- Change (only these files): `GHCAA.Domain/Enums.cs`, `GHCAA.Domain/Models/Meeting.cs` (new, all
  seven classes), `Poll.cs`, `Constants.cs`, `ApplicationDbContext.cs`,
  `GHCAA.Infrastructure/Data/Configurations/MeetingConfigurations.cs` (new); a new migration
  `AddMeetings` with its `.Designer.cs`, and `PgSqlApplicationDbContextModelSnapshot.cs`;
  `GHCAA.Tests/Data/MeetingModelTests.cs` (new); `docs/TODO.md`, the ledger row in this plan.
- Do not touch: `Seed/*.json` (see the last part), `PollService.cs`, `DiscussionService.cs`, any
  controller, any client. The list filters of T002a are built in S35 (routes), S39 and S40 (lists).
- Steps:
  1. Enums: `MeetingMode { InPerson, Online, Hybrid }`, `MeetingAudience { AllMembers, EcMembers,
     InviteesOnly }`, `MeetingStatus { Draft, Scheduled, Held, Closed, Cancelled }`, `MeetingRsvp
     { None, Yes, No, Maybe }`, `AgendaItemStatus { Planned, Discussed, Deferred, Dropped }`,
     `ActionItemStatus { Open, InProgress, Done, Dropped }`, `ActionItemPriority { Low, Normal, High }`.
     -> check: the project builds.
  2. Entities `MeetingSeries`, `Meeting`, `MeetingAgendaItem`, `MeetingParticipant`, `MeetingDecision`,
     `MeetingTodoList`, `MeetingActionItem` with the fields of the spec's data model. Type and
     category are strings holding the lookup value, as `Poll.Category` is. Add nullable
     `Poll.MeetingId`. -> check: the project builds.
  3. Configurations: lengths from new constants; `Meeting` indexed on `StartsAt` and on `SeriesId`;
     one row per (`MeetingId`, `MemberId`) in `MeetingParticipant`; deleting a meeting sets
     `Poll.MeetingId` to null and never deletes the poll. -> check: `MeetingModelTests`.
  4. Constants: a new group `Constants.Meetings` with `TypeLookupGroup = "MeetingType"`, field
     lengths, the audit type strings and the member page path. The category group stays
     `Constants.Discussion.CategoryLookupGroup`. -> check: no meeting literal outside the group.
  5. Migration per rule 5 of section 2 (the migration rule): seven tables, the `Polls.MeetingId`
     column, and the six `MeetingType` rows (AGM, EGM, EC meeting, General meeting, Event planning,
     Discussion session) with Ids 5201 to 5206, written the way `AddPollDiscussion` writes its rows,
     with the sequence step. -> check: the dry run in rule 5 on a fresh database and on a copy that
     has polls; a second run adds no row.
- Contract: none.
- Tests: `MeetingModelTests.cs` :: `Model_BuildsOnSqlite` (the context is created and one row of each
  entity saves); `Participant_OnePerMember` (a second row for the same member is refused);
  `DeletingMeeting_KeepsItsPolls`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~MeetingModelTests"`;
  then `python scripts/ci-local.py --only backend`.
- Done when: the migration applies and reruns clean; ledger row 7 has its timestamp; T002 and T003
  ticked in the spec 026 tasks; 84.19 gets an evidence note and stays IN PROGRESS.
- Stop and ask if: a new SQLite database needs the `MeetingType` rows. They come only from
  `GHCAA.Infrastructure/Data/Seed/lookups.json`, which agents do not edit. Default: the owner adds
  the six rows there (same values and Ids), and the card closes without them. Also stop if
  `JobCategorySeedSyncTests` fails because the JSON and the migration differ.

<a id="s35"></a>
**S35 · 84.19 (T004 to T006, T002a routes) · M**
- Goal: an admin creates, edits, schedules and cancels series and meetings through the API, and a
  member reads only the meetings of an audience they belong to.
- Before you start: S34 closed. `grep -rn "IMeetingService" GHCAA.Application` shows nothing.
- Read (only these): FR-1 to FR-9 and FR-24 to FR-27 of the spec 026 `spec.md`;
  `GHCAA.Infrastructure/Services/PollService.cs` (`CreatePollAsync`, where it calls
  `INewsService.CreateNewsAsync` for the announcement); `PollAnnouncementDto` in
  `GHCAA.Application/DTOs/PollDtos.cs`; `GHCAA.API/Controllers/AdminPollController.cs` and
  `PollController.cs` (policies and how the member id is read);
  `NetworkingService.GetExecutiveCommitteeAsync`; `INotificationService.cs`; `IActivityService.cs`.
- Change (only these files): `GHCAA.Application/DTOs/Meetings/MeetingDtos.cs` (new),
  `GHCAA.Application/Interfaces/IMeetingService.cs` (new), `IMeetingSeriesService.cs` (new),
  `GHCAA.Infrastructure/Services/MeetingService.cs` (new), `MeetingSeriesService.cs` (new),
  `GHCAA.API/Controllers/AdminMeetingController.cs` (new), `AdminMeetingSeriesController.cs` (new),
  `MeetingController.cs` (new), the file that registers `IPollService` (add the two services),
  `Constants.cs`, `GHCAA.Tests/Services/MeetingServiceTests.cs` (new),
  `GHCAA.Tests/Controllers/MeetingControllerTests.cs` (new), `docs/API_CONTRACT_REGISTRY.md`,
  `docs/TODO.md`.
- Do not touch: `PollService.cs`, `NewsService`, `NetworkingService.cs` (read the rule, do not move
  it), agenda, RSVP, decisions and to-do lists (S36, S37), any client.
- Steps:
  1. `MeetingService.CanSeeAsync(meeting, memberId, isAdmin, ct)`, public on the interface, the one
     audience check: admins see all; Draft is admin-only; AllMembers is any member; EcMembers is a
     member with an `ECMember` row in the active period with `EndDate` null; InviteesOnly is a member
     with a `MeetingParticipant` row. -> check: one test per audience and state.
  2. Series: create, edit, list, deactivate. -> check: service tests.
  3. Meeting create and edit with data annotations on the DTOs: venue required for InPerson and
     Hybrid, link required for Online and Hybrid, type and category must be active lookup values of
     their groups (a retired value stays valid on an existing meeting). The number in the series is
     the count of earlier meetings in it plus one. -> check: validation tests.
  4. State changes: Draft to Scheduled, Scheduled to Held, any open state to Cancelled with a reason.
     Other moves are refused with 409. -> check: one test per move.
  5. A change of start, end, venue, link or mode on a Scheduled meeting, and a cancel, notify the
     audience: `BroadcastNotificationAsync` for AllMembers, a loop over `CreateNotificationAsync` for
     the other two. -> check: tests with a recording notification service.
  6. Announcement block (`PollAnnouncementDto` reused): refused with 400 unless the audience is
     AllMembers; the text is built from title, type, date, time, mode and venue and never holds the
     link; the news id is saved on `Meeting.AnnouncementId`. Reschedule and cancel take a flag that
     updates the post. -> check: the announcement tests.
  7. Audit row through `IActivityService.LogActivityAsync` for create, edit, schedule and cancel,
     with types from `Constants.Meetings`. -> check: tests read the activity rows.
  8. Member reads: list (Scheduled, Held, Closed and Cancelled of the member's audiences) and detail;
     outside the audience is 404. Both lists take `category`, `type`, `seriesId` and `status`,
     applied after the audience check. -> check: filter tests.
- Contract: `api/admin/meeting-series` (AdminOnly): GET, POST, PUT `{id}`. `api/admin/meetings`
  (AdminOnly): GET with the four filters, GET `{id}`, POST, PUT `{id}`, POST `{id}/schedule`, POST
  `{id}/held`, POST `{id}/cancel` (reason required). `api/meetings` (signed-in member): GET with the
  filters, GET `{id}`. Replies: 200, 201 with the id, 400 validation, 404 unknown or outside the
  audience, 409 wrong state. No `DbContext` in a controller.
- Tests: `MeetingServiceTests.cs` :: `CanSee_EachAudienceAndState`, `Create_VenueAndLinkRulesPerMode`,
  `Create_RetiredLookupValueRefused`, `StateMoves_AllowedAndRefused`, `Reschedule_NotifiesAudience`,
  `Cancel_NotifiesAndKeepsReason`, `Announcement_CreatedWithoutLink`,
  `Announcement_RefusedForEcAndInvitees`, `List_EachFilter`, `List_RetiredCategoryStillFilters`.
  `MeetingControllerTests.cs` :: `Member_OutsideAudience_404`, `Member_Draft_404`,
  `Admin_Routes_NeedAdminPolicy`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~MeetingServiceTests|FullyQualifiedName~MeetingControllerTests"`;
  then `python scripts/ci-local.py --only backend`.
- Done when: the routes are in the registry; T002a (routes), T004, T005, T005a and T006 ticked; 84.19
  stays IN PROGRESS with an evidence note.
- Stop and ask if: `PollAnnouncementDto` cannot be reused without changing its fields. Default: add
  `MeetingAnnouncementDto` with the same fields and leave the poll DTO alone.

<a id="s36"></a>
**S36 · 84.19 (T007 to T009) · M**
- Goal: a meeting has an ordered agenda, invitees, RSVP answers, attendance, decisions, an outcome
  and minutes that the audience sees only after they are published.
- Before you start: S35 closed and its tests pass.
- Read (only these): FR-9 to FR-15 of the spec 026 `spec.md`; `IMeetingService.cs`,
  `MeetingService.cs` and `MeetingDtos.cs` from S35.
- Change (only these files): `MeetingDtos.cs`, `IMeetingService.cs`,
  `GHCAA.Infrastructure/Services/MeetingService_Agenda.cs` (new, a partial of `MeetingService`),
  `MeetingService_Participants.cs` (new), `MeetingService_Decisions.cs` (new), `MeetingService.cs`
  (make the class partial), `AdminMeetingController.cs`, `MeetingController.cs`, `Constants.cs`,
  `MeetingServiceTests.cs`, `MeetingControllerTests.cs`, `docs/API_CONTRACT_REGISTRY.md`,
  `docs/TODO.md`.
- Do not touch: to-do lists and "create next meeting" (S37), polls and comments (S38), any client.
- Steps:
  1. Agenda: add, edit, delete, and reorder by a full list of ids in order. After the meeting is Held
     each item is set to Discussed, Deferred or Dropped. -> check: reorder and status tests.
  2. Invitees for InviteesOnly: the admin sets the member list; removing a member removes their
     answer. -> check: service test.
  3. RSVP: a member in the audience answers Yes, No or Maybe and may change it until `StartsAt`; after
     that 409. A row is created on first answer for the AllMembers and EcMembers audiences.
     -> check: `Rsvp_AfterStart_Refused`.
  4. Attendance: the admin marks members present on a Held meeting. The detail DTO carries counts of
     each answer and of attendance. -> check: counts test.
  5. Decisions: text, optional agenda item, optional poll, optional for, against and abstain counts;
     listed on the meeting and by series. -> check: service test.
  6. Outcome and minutes: saved by the admin on a Held meeting. Publish sets `MinutesPublishedAt` and
     moves the meeting to Closed. A member read returns no minutes text before that. A Closed meeting
     refuses every edit of this card. -> check: `Minutes_HiddenUntilPublished`, `Closed_IsReadOnly`.
  7. Audit rows for attendance and publish. -> check: tests read the activity rows.
- Contract: under `api/admin/meetings/{id}`: POST `agenda`, PUT and DELETE `agenda/{itemId}`, PUT
  `agenda/order`, PUT `agenda/{itemId}/status`, PUT `invitees`, PUT `attendance`, POST `decisions`,
  PUT and DELETE `decisions/{decisionId}`, PUT `outcome`, PUT `minutes`, POST `minutes/publish`. GET
  `api/admin/meeting-series/{id}/decisions`. Member: PUT `api/meetings/{id}/rsvp`. Replies as in S35.
- Tests: `MeetingServiceTests.cs` :: `Agenda_Reorder_KeepsEveryItem`, `Agenda_StatusOnlyAfterHeld`,
  `Rsvp_ChangeBeforeStart`, `Rsvp_AfterStart_Refused`, `Rsvp_OutsideAudience_NotFound`,
  `Attendance_Counts`, `Decision_LinksItemAndPoll`, `Minutes_HiddenUntilPublished`,
  `Publish_MovesToClosed`, `Closed_IsReadOnly`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~MeetingServiceTests|FullyQualifiedName~MeetingControllerTests"`;
  then `python scripts/ci-local.py --only backend`.
- Done when: the routes are in the registry; T007, T008 and T009 ticked; 84.19 stays IN PROGRESS.
- Stop and ask if: the owner wants minutes edited after publish. Default: no; a Closed meeting is
  read-only except its action items (FR-4).

<a id="s37"></a>
**S37 · 84.20 (T010 to T012) · M**
- Goal: a meeting holds named to-do lists with action items, an owner sees their open items as "My
  to-dos", and "create next meeting" copies a meeting and carries its open work.
- Before you start: S36 closed and its tests pass.
- Read (only these): FR-7 and FR-18 to FR-20 of the spec 026 `spec.md`; `IMeetingService.cs`,
  `MeetingService.cs` (`CanSeeAsync`), `MeetingService_Agenda.cs`.
- Change (only these files): `MeetingDtos.cs`, `IMeetingService.cs`,
  `GHCAA.Infrastructure/Services/MeetingService_Todos.cs` (new), `MeetingService.cs`,
  `AdminMeetingController.cs`, `MeetingController.cs`, `Constants.cs`,
  `GHCAA.Tests/Services/MeetingTodoTests.cs` (new), `MeetingControllerTests.cs`,
  `docs/API_CONTRACT_REGISTRY.md`, `docs/TODO.md`.
- Do not touch: standalone tasks or notes (not built, FR-20), the dashboard clients (S39, S40),
  polls and comments (S38).
- Steps:
  1. Lists: add, rename, reorder, delete an empty list; category defaults to the meeting's; an
     admins-only flag. -> check: `SeveralLists_OnOneMeeting`.
  2. Items: title, notes, optional owner, optional due date, priority, status, optional decision;
     reorder inside a list; move to another list of the same meeting (another meeting is 400).
     -> check: reorder and move tests.
  3. Status: an admin or the item's owner may change it, also on a Closed meeting. Anyone else is
     refused with 403. Each change writes an audit row. -> check: `Status_OwnerAndAdminOnly`.
  4. Reads: the audience reads the lists; an admins-only list is left out for a member, but that
     member still gets the items they own. Filters `status`, `ownerId`, `unassigned`, `due`
     (`overdue`, `week`, `later`), `priority`, `category`. Progress is done out of total, Dropped not
     counted. -> check: one test per filter and `AdminsOnlyList_ShowsOwnItems`.
  5. "My to-dos": the caller's Open and InProgress items across meetings, same filters.
     -> check: `MyTodos_OnlyOwnOpenItems`.
  6. Create next meeting: a Draft copy with series, type, category, mode, venue, link, audience and
     the comment setting; Deferred agenda items copied with `CarriedFromItemId`; lists that hold open
     items copied with those items and `CarriedFromItemId`. The source is left as it is.
     -> check: `CreateNext_CopiesFieldsAndCarriesOpenWork`.
- Contract: under `api/admin/meetings/{id}`: POST `todo-lists`, PUT and DELETE `todo-lists/{listId}`,
  PUT `todo-lists/order`, POST `todo-lists/{listId}/items`, PUT and DELETE `items/{itemId}`, PUT
  `todo-lists/{listId}/items/order`, PUT `items/{itemId}/move`, POST `next` (201 with the new id).
  Member: GET `api/meetings/{id}/todo-lists` with the filters, PUT
  `api/meetings/items/{itemId}/status`, GET `api/meetings/my-todos` with the filters.
- Tests: `MeetingTodoTests.cs` :: the names in the steps, plus `Item_NoOwner_FiltersAsUnassigned`,
  `Move_ToOtherMeeting_Refused`, `Progress_IgnoresDropped`, `CreateNext_SourceUnchanged`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~MeetingTodoTests|FullyQualifiedName~MeetingControllerTests"`;
  then `python scripts/ci-local.py --only backend`.
- Done when: the routes are in the registry; T010, T010a, T011 and T012 ticked; 84.20 gets an
  evidence note and stays open until S41.
- Stop and ask if: "create next meeting" should close the carried items on the old meeting. Default:
  no, the old items stay as they are and the copies point back at them.

<a id="s38"></a>
**S38 · 84.19 (T013 to T015) · M**
- Goal: a poll attached to a meeting is seen and voted on only by the meeting's audience, and a
  meeting has a comment thread through the existing discussion service.
- Before you start: S37 closed. `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~PollServiceTests|FullyQualifiedName~DiscussionServiceTests"`
  passes.
- Read (only these): FR-16 and FR-17 of the spec 026 `spec.md`; `IPollService.cs`;
  `PollService.GetActivePollsAsync`, `GetPollByIdAsync`, `VoteAsync`, `CreatePollAsync`;
  `DiscussionService.ResolveContextAsync` and `FindOrCreateTopicAsync`; the comment actions in
  `PollController.cs` and `AdminPollController.cs`; `Constants.Discussion`.
- Change (only these files): `GHCAA.Application/DTOs/PollDtos.cs` (`MeetingId` on the create and
  read DTOs), `PollService.cs`, `DiscussionService.cs`, `IMeetingService.cs`, `MeetingService.cs`,
  `AdminMeetingController.cs`, `MeetingController.cs`, `MeetingDtos.cs`,
  `GHCAA.Tests/Services/PollServiceTests.cs`, `DiscussionServiceTests.cs`, `MeetingServiceTests.cs`,
  `docs/API_CONTRACT_REGISTRY.md`, `docs/TODO.md`.
- Do not touch: `IDiscussionService.cs` (no signature change), the poll comment routes, the forum
  tables the discussion service writes to, polls with no meeting (they behave as today).
- Steps:
  1. Attach and detach: an admin sets `Poll.MeetingId` on an existing poll or creates a poll with it.
     A poll already on another meeting is refused with 409. -> check: `Attach_OnePollOneMeeting`.
  2. `PollService` reads and `VoteAsync` call `IMeetingService.CanSeeAsync` when `MeetingId` is set.
     A poll outside the caller's audience is left out of lists and is 404 by id and on vote.
     -> check: one test per audience, and `PollWithoutMeeting_Unchanged`.
  3. The meeting detail DTO lists its polls; the list takes `category`. -> check: service test.
  4. `ResolveContextAsync` gains the `Constants.Discussion.ContextMeeting` branch: it loads the
     meeting, calls `CanSeeAsync`, returns null outside the audience, and gives a closed reason when
     comments are off, the meeting is Draft, Closed or Cancelled. -> check: discussion tests.
  5. Comment routes on the meeting controllers call `IDiscussionService` with `ContextMeeting`, the
     way the poll controllers do. -> check: controller tests.
- Contract: `api/admin/meetings/{id}/polls` PUT `{pollId}` (attach), DELETE `{pollId}` (detach).
  `api/meetings/{id}/comments` GET and POST; `api/admin/meetings/{id}/comments/{commentId}` DELETE;
  `api/admin/meetings/{id}/comments/lock` PUT. Request and reply bodies are the spec 028 DTOs
  (`CommentThreadDto`, `CreateCommentDto`, `RemoveCommentDto`, `SetThreadLockDto`). `CreatePollDto`
  and the poll read DTO gain nullable `meetingId`.
- Tests: `PollServiceTests.cs` :: `MeetingPoll_HiddenOutsideAudience`, `MeetingPoll_VoteOutsideAudience_NotFound`,
  `PollWithoutMeeting_Unchanged`, `Attach_OnePollOneMeeting`. `DiscussionServiceTests.cs` ::
  `MeetingThread_AudienceOnly`, `MeetingThread_ClosedWhenMeetingClosed`,
  `MeetingThread_OffWhenCommentsOff`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~PollServiceTests|FullyQualifiedName~DiscussionServiceTests|FullyQualifiedName~MeetingServiceTests"`;
  then `python scripts/ci-local.py --only backend`.
- Done when: every poll test that passed before still passes; T013, T014 and T015 ticked; the routes
  are in the registry; 84.19 stays IN PROGRESS.
- Stop and ask if: `PollService` cannot take `IMeetingService` without a circular dependency.
  Default: move the audience check to a small class both services use, `MeetingAudience` rules in
  `GHCAA.Infrastructure/Services/MeetingAccess.cs` (new), and keep `CanSeeAsync` as a thin call to it.

<a id="s39"></a>
**S39 · 84.19 (T016 to T018, T002a web) · M**
- Goal: on the web an admin manages series, meetings and their parts, and a member finds, reads and
  answers meetings and sees "My to-dos".
- Before you start: S38 closed. The local stack runs (`.\GHCAA.Tools\run-app.ps1`).
- Read (only these): `GHCAA.Web/src/app/admin/polls/polls.component.ts` and its `polls.html` (list,
  editor, the announcement block); `GHCAA.Web/src/app/member/polls/polls.component.ts`;
  `GHCAA.Web/src/app/core/services/admin-poll.service.ts` and `poll.service.ts`;
  `GHCAA.Web/src/app/common/comment-thread/comment-thread.ts` (inputs);
  `GHCAA.Web/src/app/core/services/lookup.service.ts`; the admin and member blocks of
  `GHCAA.Web/src/app/app.routes.ts`; `ADMIN_NAV_SECTIONS` in
  `GHCAA.Web/src/app/core/services/nav.service.ts`; `GHCAA.Web/src/app/core/constants/app.constants.ts`
  (the API path groups).
- Change (only these files): in `GHCAA.Web/src/app/admin/meetings/` (new folder):
  `meetings.component.ts` (new), `meetings.component.spec.ts` (new),
  `meeting-editor.component.ts` (new), `meeting-editor.component.spec.ts` (new), each component
  with its `.html` and `.scss`; in `GHCAA.Web/src/app/member/meetings/` (new folder): a second
  `meetings.component.ts` and `meetings.component.spec.ts`, `meeting-detail.component.ts` (new),
  `meeting-detail.component.spec.ts` (new), each component with its `.html` and `.scss`;
  `GHCAA.Web/src/app/core/services/meeting.service.ts` (new), `admin-meeting.service.ts` (new) and
  their specs; `app.routes.ts`, `nav.service.ts`, `app.constants.ts`, `lookup.service.ts`;
  `GHCAA.Web/src/app/common/events/events.ts` and `events.html`;
  `GHCAA.Web/src/app/member/dashboard/dashboard.ts` and its template; `docs/TODO.md`.
- Do not touch: `comment-thread.ts` beyond passing it the meeting context, the poll pages, theme
  tokens (use the ones there), the backend.
- Steps:
  1. Services and models for every route of S35 to S38, paths in `app.constants.ts`.
     -> check: service specs with the HTTP testing controller.
  2. Admin list at `admin/meetings` with filters for type, category, series and status, kept in the
     query string; a nav entry in `ADMIN_NAV_SECTIONS`. -> check: component spec.
  3. Editor with tabs: details (with the announcement block, shown only for All members), agenda
     (reorder), participants and attendance, decisions and minutes, to-do lists (reorder, move,
     filters, progress), polls. Series are managed from the same page. -> check: component spec and
     a look in the browser.
  4. Lookup values: a small panel on the admin meetings page adds, renames and retires `MeetingType`
     and `DiscussionCategory` values through the `api/lookups` admin routes; `lookup.service.ts`
     gains the three write calls. -> check: spec.
  5. Member list at `member/meetings` (upcoming and past, category and type filter) and detail with
     RSVP, agenda, polls, `<app-comment-thread>`, decisions, to-do lists and published minutes. The
     category shows as a link to the filtered list. -> check: component specs.
  6. `events.ts` shows the meetings linked to an event; the dashboard shows "My to-dos" with the
     filters. -> check: specs.
  7. Dates as `dd-MM-yyyy` and times as `HH:mm` through the shared date helpers. -> check: spec.
- Contract: none new. The clients use the routes of S35 to S38.
- Tests: `meetings.component.spec.ts` (admin) :: filters reach the service, a Draft row shows its
  state; `meeting-editor.component.spec.ts` :: the announcement block hides for EC and invitee
  audiences, venue and link fields follow the mode; `meetings.component.spec.ts` (member) :: the
  filter and the two lists; `meeting-detail.component.spec.ts` :: RSVP disabled after start, minutes
  absent before publish; the two service specs.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/admin/meetings src/app/member/meetings src/app/core/services/meeting.service.spec.ts src/app/core/services/admin-meeting.service.spec.ts`;
  then `python scripts/ci-local.py --only web`.
- Done when: both areas work in the browser against the local API in the light and dark themes;
  T016, T017 and T018 ticked; 84.19 stays IN PROGRESS.
- Stop and ask if: the owner wants the lookup panel as its own page under Settings. Default: the
  panel on the admin meetings page, since no lookup page exists.

<a id="s40"></a>
**S40 · 84.19 (T019 to T020, T002a mobile) · M**
- Goal: on mobile a member uses meetings and "My to-dos", and an admin manages meetings; only admins
  reach the admin screens.
- Before you start: S39 closed. `cd GHCAA.Mobile; flutter analyze` is clean.
- Read (only these): `GHCAA.Mobile/lib/features/polls/polls_screen.dart`, `poll_service.dart` and
  `poll_comment_thread.dart`; `GHCAA.Mobile/lib/core/router/app_router.dart` (the redirect, the
  `/polls` route and the admin routes); `GHCAA.Mobile/lib/screens/admin/admin_modules.dart`;
  `GHCAA.Mobile/lib/screens/member/dashboard_screen.dart` and `event_details_screen.dart`;
  `GHCAA.Mobile/test/poll_comment_thread_test.dart` (the test setup to copy).
- Change (only these files): in `GHCAA.Mobile/lib/features/meetings/` (new folder):
  `meeting_service.dart` (new), `meeting_models.dart` (new), `meetings_screen.dart` (new),
  `meeting_detail_screen.dart` (new), `my_todos_screen.dart` (new),
  `admin_meetings_screen.dart` (new), `admin_meeting_editor_screen.dart` (new); `poll_comment_thread.dart` (take a context type
  and id; the poll use keeps working), `app_router.dart`, `admin_modules.dart`,
  `dashboard_screen.dart`, `event_details_screen.dart`;
  `GHCAA.Mobile/test/meetings_screen_test.dart` (new), `meeting_detail_screen_test.dart` (new), `admin_meetings_screen_test.dart` (new);
  `docs/TODO.md`.
- Do not touch: the other admin routes' guards (a wider guard is its own tracker item), web
  patterns that do not fit mobile, goldens, the backend.
- Steps:
  1. Service and models for the member and admin routes. -> check: `flutter analyze`.
  2. Member list (upcoming and past, category and type filter) and detail with RSVP, agenda, polls,
     the comment thread, decisions, to-do lists and minutes. -> check: widget tests at phone width.
  3. "My to-dos" with the filters, reached from the dashboard; linked meetings on the event screen.
     -> check: widget test.
  4. Admin list and editor: series, meeting fields, agenda reorder, attendance, decisions, minutes,
     to-do lists, the announcement block for All members only. -> check: widget test.
  5. The two admin routes redirect a signed-in user without the Admin or SuperAdmin role to the
     dashboard, and the admin module tile shows only for those roles. -> check:
     `AdminRoute_NonAdmin_Redirected`.
  6. Wrap widget tests in `ProviderScope`, use a phone viewport and poll for content; do not rely on
     `pumpAndSettle` with a duration. -> check: the tests pass three runs in a row.
- Contract: none new.
- Tests: `meetings_screen_test.dart` :: lists and the filter; `meeting_detail_screen_test.dart` ::
  RSVP disabled after start, minutes absent before publish, a server message shown on 409;
  `admin_meetings_screen_test.dart` :: `AdminRoute_NonAdmin_Redirected`, the announcement block
  hidden for EC and invitee audiences.
- Run: `cd GHCAA.Mobile; flutter analyze; flutter test test/meetings_screen_test.dart test/meeting_detail_screen_test.dart test/admin_meetings_screen_test.dart test/poll_comment_thread_test.dart --exclude-tags golden`;
  then `python scripts/ci-local.py --only mobile`.
- Done when: T019, T019a and T020 ticked; the poll comment test still passes; 84.19 stays IN PROGRESS.
- Stop and ask if: the role is not readable in the router redirect without a new provider. Default:
  guard inside the two admin screens (show "Admins only" and go back) and note it in the tracker.

<a id="s41"></a>
**S41 · 84.19, 84.20 (T021) · S**
- Goal: the meetings work is recorded in every doc that lists features and routes, and both tracker
  items close on a full local run.
- Before you start: S40 closed.
- Read (only these): the meeting entries added to `docs/API_CONTRACT_REGISTRY.md` by S35 to S38; the
  polls rows of `docs/PROJECT_MAP.md` and `docs/FEATURES.md` (the form to copy); the acceptance part
  of the spec 026 `spec.md`.
- Change (only these files): `docs/API_CONTRACT_REGISTRY.md`, `docs/PROJECT_MAP.md`,
  `docs/FEATURES.md`, `docs/api/swagger.json` (regenerated by `ci-local.py`, not by hand),
  `docs/specs/026-meetings-and-action-items/tasks.md` and `spec.md` (status line), `docs/TODO.md`,
  card H00 in this plan (one line: meetings exist, for the book).
- Do not touch: `docs/book/`, code (a fault found here goes back to its card or to a new tracker
  item).
- Steps:
  1. Compare the registry with the controllers: every meeting route and DTO is listed once.
     -> check: `grep -c "api/meetings\|api/admin/meeting" docs/API_CONTRACT_REGISTRY.md` against the
     action count of the three controllers.
  2. Add the classes, routes, pages and screens to `PROJECT_MAP.md` and the feature to `FEATURES.md`.
     -> check: each new file of S34 to S40 is named once in the map.
  3. Run each layer, one at a time: `python scripts/ci-local.py --only backend`, then `web`, `mobile`,
     `contract`. The contract run refreshes the swagger snapshot. -> check: four green runs.
  4. Walk the seven acceptance points of the spec against the tests and write the test name beside
     each in the tracker entry. -> check: no point without a test.
- Contract: none.
- Tests: none new.
- Run: the four `ci-local.py --only` runs of step 3; then `graphify update .`.
- Done when: T021 ticked and the spec status says built; 84.19 DONE and 84.20 DONE with the commands
  and counts; the plan check reports 0 problems.
- Stop and ask if: an acceptance point has no test. Default: reopen the card that owns it; do not
  close the items.

<a id="s42"></a>
**S42 · 84.59 · M**
- Goal: six service methods that return `IEnumerable<object>` return typed DTOs, the JSON a client
  receives is unchanged, and the shapes are in Swagger.
- Before you start: S41 closed. `grep -rn "IEnumerable<object>" GHCAA.Application/Interfaces` shows
  six lines: `IMentorshipService.cs` (three), `INetworkingService.cs`, `IContactService.cs`,
  `IChatService.cs`.
- Read (only these): `MentorshipService.GetSentRequestsAsync`, `GetReceivedRequestsAsync` and
  `GetAllForAdminAsync`; `NetworkingService.GetECPeriodsAsync`; `ContactService.GetMessagesAsync` (it
  returns `ContactMessage` entities); `ChatService.GetRecentChatsAsync` (an anonymous shape sorted
  through a `dynamic` cast); the actions that call them in `MentorshipController.cs`,
  `NetworkingController.cs` (`GetPeriods`), `AdminController.cs` (`GetContactMessages`) and
  `MessagingController.cs` (the `recent` and `conversations` action); the client models in
  `GHCAA.Web/src/app/core/services/chat.service.ts`,
  `GHCAA.Web/src/app/admin/contact-messages/contact-messages.ts`,
  `GHCAA.Mobile/lib/features/networking/mentorship_service.dart`, `networking_service.dart` and
  `GHCAA.Mobile/lib/features/admin/admin_service.dart`.
- Change (only these files): `GHCAA.Application/DTOs/MentorshipRequestDtos.cs` (new),
  `GHCAA.Application/DTOs/EcPeriodDto.cs` (new), `ContactMessageDto.cs` (new),
  `RecentChatDto.cs` (new); the four interfaces; `MentorshipService.cs`, `NetworkingService.cs`, `ContactService.cs`,
  `ChatService.cs`; the four controllers only where a return type is declared;
  `GHCAA.Tests/Services/TypedReplyShapeTests.cs` (new); `docs/api/swagger.json` (regenerated),
  `docs/API_CONTRACT_REGISTRY.md`, `docs/TODO.md`. A client file only if step 5 finds a mismatch.
- Do not touch: routes, policies, query filters and ordering; any JSON property name; the typed
  methods of the four services.
- Steps:
  1. Before any change, write `TypedReplyShapeTests` so each of the six methods is serialized with
     the API's JSON options and the property names are compared with a fixed list. -> check: the
     tests pass on the old code. This pins today's shape.
  2. Add the DTOs with exactly those properties, nested ones included (the mentorship rows carry a
     `Mentor` or `Requester` object with `Id`, `FullName`, `PhotoPath`, `MembershipNumber`; the admin
     list has both without `PhotoPath`, either may be null). -> check: build.
  3. Change the six signatures and projections. `ContactMessageDto` holds every property the entity
     sends today and no navigation property. `GetRecentChatsAsync` sorts on the typed property and
     drops the `dynamic` cast. -> check: the step 1 tests pass unchanged, and the existing service
     and controller tests pass.
  4. Regenerate the swagger snapshot with `python scripts/ci-local.py --only contract`. -> check:
     the six replies name their DTO in `docs/api/swagger.json`.
  5. Compare each web and mobile model with its DTO, field by field. -> check: a line per client
     model in the tracker entry; fix a client only where it reads a field the API never sent.
- Contract: no route or field changes. Six replies gain a schema: `api/mentorship/sent`,
  `api/mentorship/received`, `api/mentorship/admin/all`, `api/networking/periods`,
  `api/admin/contact-messages`, `api/messaging/recent` (also served as `api/messaging/conversations`
  and under `api/chat`).
- Tests: `TypedReplyShapeTests.cs` :: one test per method, named `<Method>_KeepsPropertyNames`;
  existing `MentorshipServiceTests`, `MentorshipControllerTests`, `ContactServiceTests`,
  `ChatServiceTests` and `NetworkingServiceTests` unchanged.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~TypedReplyShapeTests|FullyQualifiedName~Mentorship|FullyQualifiedName~ContactService|FullyQualifiedName~ChatService|FullyQualifiedName~NetworkingService"`;
  then `python scripts/ci-local.py --only backend` and `--only contract`.
- Done when: `grep -rn "IEnumerable<object>" GHCAA.Application/Interfaces` shows nothing; the registry
  has the six shapes; 84.59 DONE.
- Stop and ask if: a property name or casing must change to make a DTO (for example the entity
  sends a field the admin page should not get). Default: keep the name and the field, and record
  the finding as a new tracker item.

<a id="s43"></a>
**S43 · 84.84 (backend) · L · migration 8**
- Goal: a member has a home city and country, signed-in members can search the directory by either,
  and a member who hides their address is never found or shown by location.
- Before you start: S41 closed and migration 7 of the ledger (section 7, the migration ledger) is
  applied locally. `grep -n "City\|Country" GHCAA.Domain/Models/Member.cs` shows nothing.
- Read (only these): `Member` (`PresentAddress`, `PermanentAddress`, `IsAddressPublic`) in
  `GHCAA.Domain/Models/Member.cs`; `GHCAA.Application/DTOs/BaseMemberDto.cs`, `UpdateProfileDto.cs`,
  `MemberSummaryDto.cs`; `MemberSearchFilterDto` in `GHCAA.Application/Interfaces/INetworkingService.cs`;
  `NetworkingService.SearchMembersAsync`, `SearchPublicDirectoryAsync`, `ToPublicListing` and
  `MapToDto` (where the addresses become "Confidential"); the two `IsAddressPublic` lines in
  `GHCAA.Infrastructure/Services/MemberService_Search.cs` and in `MemberService_Profile.cs`;
  `NetworkingController.Search`.
- Change (only these files): `Member.cs`, `Constants.cs`, `BaseMemberDto.cs`, `MemberSummaryDto.cs`,
  `INetworkingService.cs`, `NetworkingService.cs`, `MemberService_Profile.cs`,
  `MemberService_Search.cs`, the `Member` configuration class; a new migration `AddMemberLocation`
  with its `.Designer.cs`, and `PgSqlApplicationDbContextModelSnapshot.cs`;
  `GHCAA.Tests/Services/NetworkingServiceTests.cs`, `GHCAA.Tests/Services/MemberSearchOrderTests.cs`
  only if a fixture needs the fields; `docs/API_CONTRACT_REGISTRY.md`, `docs/TODO.md`, the ledger
  row in this plan.
- Do not touch: `PresentAddress` and `PermanentAddress` (no parsing of old addresses into city and
  country), the other privacy switches, `ToPublicListing` (the public directory sends five fields
  and stays that way), `Seed/*.json`, any client (S44).
- Steps:
  1. `Member.City` and `Member.Country`, nullable, max 100 from a new constant; the same two on
     `BaseMemberDto` with `[MaxLength]`. -> check: build.
  2. Migration per rule 5 of section 2 (the migration rule): two nullable columns, no data step.
     -> check: the dry run in rule 5 on a fresh database and on a copy with members.
  3. Profile save (`UpdateProfileAsync`) stores both, trimmed, empty as null. Profile read for
     another member blanks both when `IsAddressPublic` is off, in `MapToDto` beside the address
     lines; the owner and an admin still see them. -> check: profile tests.
  4. `MemberSearchFilterDto.City` and `.Country`: trimmed, case-insensitive whole-value match that
     works on both providers (compare lower-cased values). When either is set, members with
     `IsAddressPublic` off are left out. -> check: search tests.
  5. `MemberSummaryDto` gains `City` and `Country`, filled in `SearchMembersAsync` only for members
     with `IsAddressPublic` on. `SearchPublicDirectoryAsync` ignores both filters and sends neither
     field. -> check: `PublicDirectory_IgnoresLocation`.
- Contract: `GET api/networking/search` and `api/networking/directory` take `city` and `country`
  (signed-in callers only; ignored for anonymous ones). `MemberSummaryDto`, the profile read and
  `UpdateProfileDto` gain nullable `city` and `country`. No new route.
- Tests: `NetworkingServiceTests.cs` :: `Search_ByCity_Matches`, `Search_ByCountry_IgnoresCaseAndSpaces`,
  `Search_ByLocation_SkipsHiddenAddress`, `Search_NoLocationFilter_BlanksHiddenCityAndCountry`,
  `Search_CityAndCountry_BothMustMatch`, `PublicDirectory_IgnoresLocation`,
  `Profile_HiddenAddress_BlanksCityAndCountry`, `Profile_Save_TrimsAndStoresNullForEmpty`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~NetworkingServiceTests|FullyQualifiedName~MemberSearchOrderTests"`;
  then `python scripts/ci-local.py --only backend`.
- Done when: ledger row 8 has its timestamp; the registry has the two filters and fields; 84.84
  gets an evidence note and stays open until S44.
- Stop and ask if: the admin member search should also filter by location. Default: no, only the
  member directory; record the wish as a new tracker item.

<a id="s44"></a>
**S44 · 84.84 (clients) · M**
- Goal: members set city and country on their profile and filter the directory by them, on web and
  on mobile.
- Before you start: S43 closed. The local API returns `city` on `GET api/networking/search`.
- Read (only these): `GHCAA.Web/src/app/member/profile/profile.ts` and `profile.html` (the address
  fields and the address privacy switch); `GHCAA.Web/src/app/common/directory/directory.ts` and
  `directory.html` (the filter form and how filters reach the query);
  `GHCAA.Mobile/lib/screens/member/profile_edit_screen.dart`, `directory_screen.dart`;
  `GHCAA.Mobile/lib/features/networking/networking_service.dart`;
  `GHCAA.Mobile/test/directory_filters_test.dart` and `profile_edit_screen_test.dart`.
- Change (only these files): `profile.ts`, `profile.html`, `profile.spec.ts`, `directory.ts`,
  `directory.html`, `directory.spec.ts`, the web member model file those two import;
  `profile_edit_screen.dart`, `directory_screen.dart`, `networking_service.dart`, the mobile member
  model those import, `directory_filters_test.dart`, `profile_edit_screen_test.dart`;
  `docs/FEATURES.md`, `docs/TODO.md`.
- Do not touch: `GHCAA.Web/src/app/public/directory/public-directory.ts` (no location there), the
  backend, the other profile fields, goldens.
- Steps:
  1. Web profile: two text inputs, max 100, beside the address fields, with a hint that the address
     switch also hides them. -> check: spec, and a save in the browser.
  2. Web directory: city and country filter inputs sent as `city` and `country`; a card shows them
     when the API sends them. -> check: spec.
  3. Mobile profile edit and directory: the same two fields and filters. -> check: widget tests at
     phone width.
  4. Labels come from the translation or label source the neighbouring fields use, not from string
     literals in the template. -> check: no new literal label in the four screens.
- Contract: none new.
- Tests: `profile.spec.ts` :: city and country are sent on save; `directory.spec.ts` :: the two
  filters reach the service call and clear with the rest; `profile_edit_screen_test.dart` :: the
  fields save; `directory_filters_test.dart` :: the filters reach the request.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/member/profile src/app/common/directory`;
  `cd GHCAA.Mobile; flutter analyze; flutter test test/directory_filters_test.dart test/profile_edit_screen_test.dart --exclude-tags golden`;
  then `python scripts/ci-local.py --only web` and `--only mobile`.
- Done when: a member with a hidden address is not found by city on either client; 84.84 DONE with
  the commands and counts.
- Stop and ask if: the owner wants country as a pick list. Default: free text on both clients; a
  list is a new tracker item.

### Phase E. Election sessions 7 to 9

<a id="s45"></a>
**S45 · 95.2 (N1) · L · migration 9**
- Steps: migration `AddCommitteePositionsAndTerms`: `EcEndReason` table (seeded with today's five
  `VacancyReason` values under the same ids; TermEnded and Died marked system), `ECMember` end reason
  FK, `Election.Kind` (General 0, ByElection 1), `ECPeriod.DefaultTermMonths` (nullable), rebuild
  `IX_Elections_ECPeriodId_Live` on period and kind. Rollback refuses while admin-made end reasons
  exist (raise in a DO block).
- Tests: model on SQLite; migration rule 5; rollback refusal on PostgreSQL.
- Done when: N1 built; 95.2 stays PARTIAL until G-REL confirms preprod and the invalid-rows report.

<a id="s46"></a>
**S46 · 95.4 · M** One vacant-seat query in `GovernanceService` (holders in the active period below
the position's seat count); filter on the web governance page and the mobile governance screen.
Tests: ended term appears, filled seat leaves. Done when: 95.4 DONE.

<a id="s47"></a>
**S47 · 95.12 (backend) · M · security** `TermEndSweep` like `DataRetentionSweep`, off by default
(`OrgConfig` governance setting); default term length fills a new period's end date; end-reason
admin routes; system reasons cannot be removed; `end-term` takes a reason id and a note.
Tests: on and off, early end with each reason, system reason delete refused.

<a id="s48"></a>
**S48 · 95.12 (clients) · M** Web end-reason admin next to positions, reason picker on web and
mobile end-term. Done when: 95.12 DONE.

<a id="s49"></a>
**S49 · 95.5 (backend) · M** Kind on create; a by-election offers vacant seats only and refuses a held
seat at create and at declare; declare never ends a holder (owner 2026-10-14, the older tracker text
about ending a holder is superseded); `GetCurrentAsync` returns the general election and any live
by-election. Tests: declare fills only the vacant seats, guards, two live elections.

<a id="s50"></a>
**S50 · 95.5 (clients) · M** Kind shown on ballot, results and ER forms; both live elections on web
and mobile; admin create on web only. Done when: 95.5 DONE.

<a id="s51"></a>
**S51 · 37.13f · S** Close the 11 *[to confirm]* points in doc 09 against free Neon and free Render
public docs (link and date for each); mark availability as a known limit with no SLA; backup schedule
with RTO and RPO; branch warning in `docs/DEPLOYMENT_CHECKLIST.md`; a log line for a failed count with
a test. Points that need the owner's dashboard values name the owner as the source still owed.

### Phase F. P2 items

<a id="s52"></a>
**S52 · 84.4 · S** Parity table in spec 004 (or `docs/API_CONTRACT_REGISTRY.md` appendix): for each
endpoint in the spec 004 contract table, grep `GHCAA.Web/src` and `GHCAA.Mobile/lib` for the route; columns web,
mobile, response difference. Script it in `scripts/` if it takes more than one pass.

<a id="s53"></a>
**S53 · 93.3 · S** Find why `registerNewMemberViaUi` gets "Registration failed" on the local API (read
`api_error.log` tail and the registration response body; the membership walkthrough records the
API's answer). Fix the cause (seed, OrgConfig or test data), run `portal-mobile-nav.spec.ts` and
`gallery.spec.ts`.

<a id="s54"></a>
**S54 · 84.27 (API tests) · S** `ArchiveControllerTests` for each route: 200, 401, 403, 404.

<a id="s55"></a>
**S55 · 84.27 (web page) · M** `admin/archive` moderation page with the shared admin components and
an `ADMIN_ARCHIVE` endpoint block. Vitest. Done when: 84.27 DONE.

<a id="s56"></a>
**S56 · 84.55 · M** `CreateGalleryDto` with caller fields only, mapped in the service; test that
extra fields are ignored; web and mobile callers checked; registry.

<a id="s57"></a>
**S57 · 84.93 · M** `POST api/admin/governance/constitution` (AdminOnly policy) with
version, text, PDF, effective date, change summary; on the effective date the new one becomes active
and the old gets `SupersededDate` (computed on read, so no job); public page shows the version in
force. Tests: future date, two on the same date (refused). Web admin screen. Unblocks Secretary
Task 5 (78.9).

<a id="s58"></a>
**S58 · 84.97 · M** Status change to Terminated or InactiveResigned requires a reason; write
`MembershipHistory` (from, to, admin, reason); admin member page shows the history. Tests: missing
reason 400, history row. Walkthrough: membership walkthrough terminates the new member at the
end (missing reason refused, history shown).

<a id="s59"></a>
**S59 · 84.67 · M** Mobile Change Password screen under settings calling the web route, step-up
dialog, `serverMessage`. Widget tests: success, wrong current password, step-up cancel.

<a id="s60"></a>
**S60 · 34.D8 · M** Fix the local admin role through the protected SuperAdmin seeder path (memory
`feedback_protected_superadmin_pattern`), not SQL. Then the browser walk on the tracker list,
including a member refused when creating a Notice. Record each step.

<a id="s61"></a>
**S61 · 84.65 · M** Web `admin/scholarships` page on the existing routes; list, create, edit, close,
review applications. Vitest; spec 015.

<a id="s62"></a>
**S62 · 49.3.C · L · migration 10** `User.ContactEmail` (nullable, unique when set), migration
`AddUserContactEmail`; SuperAdmin sets it on the roles page; reset for a no-email account sends there
when set, else the current copy and share flow. Tests on both paths.

<a id="s63"></a>
**S63 · 97.1 (backend) · M** `OtpPurpose.EmailVerify` appended; profile routes to send and verify;
changing the email clears `EmailVerified`; reuse the OTP service and its lock. Tests: OTP path, lock,
clear on change, social link refused while unverified.

<a id="s64"></a>
**S64 · 97.1 (clients) · M** Badge and Verify action on web and mobile profile. Walkthrough: membership
walkthrough shows the badge after the first login and verifies by code. Done when: 97.1 DONE.

<a id="s65"></a>
**S65 · 84.83 (backend) · L · migration 11** `Member.IsMentor` and `Member.MentorTopics` (one row per
person, so not on `ProfessionalRecord`); migration `AddMentorOptIn`; requests only to
opted-in members; directory filter. Tests incl. opting out with a request pending (pending stays,
new ones refused).

<a id="s66"></a>
**S66 · 84.83 (clients) · M** Opt-in setting and directory filter on web and mobile.

<a id="s67"></a>
**S67 · 84.82 (backend) · L · migration 12** `EventFeedback` (event, member, rating 1-5, comment,
created at; unique event and member), migration `AddEventFeedback`; only approved attendees after the
event starts; admin summary. Tests: non-attendee, repeat.

<a id="s68"></a>
**S68 · 84.82 (clients) · M** Rating form on web and mobile; admin summary on web.

<a id="s69"></a>
**S69 · 84.72 (backend) · L · migration 13** Admin forum routes (categories CRUD, order, retire;
pin, lock, remove with reason to the audit log, reusing spec 028 moderation); migration
`SeedForumCategories` with a few defaults (data step). Tests.

<a id="s70"></a><a id="s71"></a>
**S70 · 84.72 (web) · M** Admin forum section. **S71 · 84.72 (mobile) · M** Same tools behind a role
guard (memory `gotcha_mobile_admin_routes_no_role_guard`). Done when: 84.72 DONE; spec 017 updated.

<a id="s72"></a>
**S72 · 92.1 · S** Write spec 022 decisions with the owner: settings in an OrgConfig `Approvals`
section (recommended) or a table; "off" means created approved with an audit row (recommended);
SuperAdmin always approves (yes, protected rule); member import stays SuperAdmin and Admin.

<a id="s73"></a>
**S73 · 92.2 · L** Per spec 022. If the owner chose a table, it takes ledger row 14. Policy handler
reading the flow's roles; unknown flow refused and logged; `MemberApprovalMode` mapped. Tests per
the tracker; reflection tests updated.

<a id="s74"></a><a id="s75"></a>
**S74 · 92.3 · M** Web settings screen and queue filtering. **S75 · 92.4, 92.5 · S** Mobile gating
with widget tests; FR and NFR entries, spec 002 tables, `business_flow` notes, `PROJECT_MAP.md`.
The book part of 92.5 (§8.4, the role matrix, and its figure) goes to H00.

<a id="s76"></a>
**S76 · 37.7 (web mechanism) · M** `I18nService`, `en.ts`, `bn.ts`, `TranslatePipe`, toggle beside
the theme toggle, self-hosted Bengali font, `<html lang>`; key-drift test. Admin stays English.

<a id="s77"></a>
**S77 · 37.7 (web strings) · M** Public site and portal nav, buttons, labels, validation messages.
Bengali strings are drafts; 37.7 stays PARTIAL until gate G-BN.

<a id="s78"></a>
**S78 · 8.8 · M** Mobile: same keys in `app_localizations.dart` (flat dictionary, no ARB). 8.8 stays
PARTIAL until G-BN.

<a id="s79"></a>
**S79 · 62.2 · M** Write the missing profile files for `profiles/default/` and `profiles/ghc/`:
`email_templates.json`, `site_content.json`, `themes.json`, `membership-tiers.json`,
`governance.json`, and the GHC `constitution.json`; match the shapes the seeders read. Tests: the
profile pack loader test per file.

<a id="s80"></a>
**S80 · 51.4 · S** Add the excluded-type matrix (`PaymentProof`, `Certificate`, `Signature`,
`NoticeDocument` not compressed) and the non-image fallback to `LocalFileStorageServiceTests`.

<a id="s81"></a>
**S81 · 27.1, 27.8 · M** Script the low-coverage report from `coverage.cobertura.xml` (backend), the
Vitest V8 summary and Flutter LCOV. Question to the owner first: enforce 80 % per file now (fails
today) or a ratchet (no file drops below its current figure, new files at least 80 %)
(recommended). Wire the chosen gate into `ci-local.py`.

<a id="s82"></a>
**S82 · 73.5 · M** Extend `traceability.py` to generate the whole matrix from the FR and NFR tags.
The book claim it enables goes to H00.

<a id="s83"></a>
**S83 · 73.6 · S** Test pinning the seeded constitution version and a SHA-256 of its clause text.

### Phase G. P3 and P4 items

<a id="s84"></a>
**S84 · 84.57, 84.62 · S** Named upload limits in `Constants.Uploads` used at every call site listed in
84.57; a per-user `Assistant` rate-limit policy (default 20 per hour from OrgConfig) with a 429 test,
checked against the global limiter.

<a id="s85"></a>
**S85 · 84.64, 93.4 · S** Mobile "mark all read" on the notifications screen (widget test). 93.4:
ask the owner which of `/communications`, `/legacy`, `/professionals` stay (recommended: add the
ones with a working screen to the drawer, remove the rest), and gate drawer entries on the OrgConfig
flags.

<a id="s86"></a>
**S86 · 84.69 · M** Mobile site content list and detail from the public route; widget tests and a
phone-viewport check.

<a id="s87"></a>
**S87 · 84.23 · M** Question to the owner: mobile signature upload (recommended yes, `POST
api/profile/signature` exists) and the unused PDF routes (recommended: mobile downloads
`id-card/pdf` and `certificate/pdf` and drops its own PDF builder, which also fixes Bengali names).

<a id="s88"></a>
**S88 · 96.3 · M** Messages page header and composer, directory search input, campaign form inputs
onto the shared components.

<a id="s89"></a>
**S89 · 84.90 · M** Owner question: reuse the newest live code per type (recommended) or keep minting.
Build the chosen rule; spec 013 FR-010.

<a id="s90"></a>
**S90 · 84.95 · S** Applied date and days waiting on the approval list, past-limit mark, both
clients; tests for day 30 and 31; membership walkthrough checks the applied date and days waiting
in the queue. Runs after the 78.9 sessions unless the owner says first.

<a id="s91"></a>
**S91 · 84.94 · M** Arrears report with CSV export; tests for no dues, part payment, waived due.
Same timing rule as S90.

<a id="s92"></a>
**S92 · 84.66 · M** OrgConfig cap on pending archive submissions (default 5); API refusal message;
both clients show it; golden snapshot.

<a id="s93"></a>
**S93 · 51.5 · M** OrgConfig `fileStorage` section read by the storage service with `Constants`
fallback; admin settings tab.

<a id="s94"></a>
**S94 · 84.98 · M** The seven theme gaps on the tracker; color logic tests; browser check both modes
with a light custom primary.

<a id="s95"></a>
**S95 · 90.1, 90.2 · L · migration 15** `SavedReport` with `CreatedByUserId` (spec 010 wins over the
tracker's `CreatedBy`), migration `AddSavedReports`; `ReportService` with field whitelist and row cap.

<a id="s96"></a><a id="s97"></a>
**S96 · 90.3, 90.5 · M** Controller and tests. **S97 · 90.4 · M** Web page on the shared grid from
S18.

<a id="s99"></a><a id="s100"></a><a id="s101"></a><a id="s102"></a><a id="s103"></a>
**S99 to S103 · 37.4, 37.5, 37.10** Build as the tracker items describe; migrations 16 and 17 for
cohorts and memorial; 37.10 after 37.4.

<a id="s104"></a><a id="s105"></a><a id="s106"></a><a id="s107"></a>
**S104 to S107 · 62.56** Spec 025 tasks T001 to T036 in four parts: section map and key uniqueness
(a, g); export (b); upload, mapping and preview (c, d); modes, transaction, tests and docs (e, f).

<a id="s108"></a>
**S108 · 60.3, 6.2, 8.5, 8.7, 37.9 · S** One owner question round, each with a recommended default:
60.3 read both clients and close as a naming difference if they hit the same routes; 6.2 fold into
84.83 (mentor and referral) or drop; 8.5 drop unless a measured slowness exists; 8.7 drop with 8.5;
37.9 keep at P4 for after 37.4. Record each answer; build cards are added only for a "build" answer.

<a id="s109"></a>
**S109 · 37.11 · S** Refresh the WP37 baselines from a full run and update `FEATURES.md`,
`PROJECT_MAP.md`, `SRS.md` and `ARCHITECTURE.md`. Close 37.11 when every other WP37 item is DONE or
WONTDO; if gate items remain (G-DEV, G-BN), set it PARTIAL naming them.

### Phase I. Public site, admin and member experience gaps

Added 2026-11-17 from the public and admin site reviews. Design: `docs/specs/029-public-admin-member-experience-gaps/`
(spec, plan and task list for Work Package 100). Owner, 2026-11-17: the sessions run by their
dependencies, so their rows sit in phases B, F and G of section 3 and only the cards are kept
together here. S111 runs before S14 (84.81, dues and event reminders) so reminders use the same
mail check: dues reminders always send and event reminders follow the member's preference. The
owner answered D1 to D8 of spec 029 on 2026-11-17. Short form; rewrite into the eleven-part form
before running.

<a id="s110"></a>
**S110 · 100.1, 100.2 · S** Point the home page news and jobs previews at public routes, add a Join
button to the public header, and set the canonical link and JSON-LD in `GHCAA.Web/src/index.html`
at run time. The `author` meta tag stays (owner, 2026-11-17). Guard spec for `/portal` links in public templates;
white-label guard covers `index.html`. Browser check at phone and desktop width.

<a id="s111"></a>
**S111 · 100.3 (API) · M** First record whether the send path reads the `Notify*` flags. Save
`NotifyRelevantUpdates`; add one mail preference check used by every batch and reminder send; add
the signed unsubscribe route and its undo. Four kinds always send (owner, 2026-11-17): account and
security mail, payment receipts, election notices the rules require, and membership status mail
(approval, rejection, expiry, dues owed). Tests for each kind, for a changed token and for the
rate limit. Registry updated.

<a id="s112"></a>
**S112 · 100.3 (clients) · M** "Email preferences" on the web member profile, the same five toggles
on mobile profile edit, and the public unsubscribe page. Specs and a widget test. Closes 100.3.

<a id="s113"></a>
**S113 · 100.4 · L** Preview route with recipient and opted-out counts; batch and recipient rows
with one migration (ledger row 2a in section 7); hosted worker; status
route. Admin confirm dialog and progress. Tests: failed recipient, retry without a duplicate.
`code-reviewer` on the migration. Split into API and web sessions if the card runs long.

<a id="s114"></a>
**S114 · 100.6 (API, web), 100.7, 100.8 · M** Three site content keys by a migration data step
(ledger row 14a); `/privacy`, `/terms`, `/faq` on web with footer and consent links. Owner,
2026-11-17, second answer: the pages ship with drafted standard text that an admin can edit, and
this part is built ahead of the row's place in section 3. Skip link and `aria-expanded` in the three web layouts. Membership fees section
on the home page from the anonymous fees route. The mobile links are S130, and 100.6 closes there.
Built 2026-11-17: the 100.6 web and API part (migration, `PolicyContentSeeder`, `public/policy-page`,
footer and consent links). 100.7 and 100.8 are still open in this card.

<a id="s115"></a>
**S115 · 100.5 · L** Owner, 2026-11-17: prerender the public routes at build time. Titles and Open
Graph tags per route, a sitemap from published content that leaves out members-only news, and a
share action in the portal and the app. A post published after the last build has no preview until
the next build; record that on 100.5 and in the deploy notes.

<a id="s116"></a>
**S116 · 100.9 · M** Notification link constant; bell in the portal and admin layouts;
`/portal/notifications` with mark read and mark all read; mobile maps the link to its notification
screen. Sending push stays with 84.61.

<a id="s117"></a>
**S117 · 100.10 · L** After S13 (84.80, registration cancellation on clients). First record whether the member registration DTO returns `TicketCode`. QR on
the pass in the portal and the app; admin web check-in page; attendance list route with CSV on web
and a list on mobile; check-in refuses a cancelled registration. Event walkthrough gains a check-in
step and a refusal probe, README table updated.

<a id="s118"></a>
**S118 · 100.11 · M** `AdminStatsDto` that keeps the existing field names (the mobile admin
dashboard reads the same route) and adds monthly members, monthly dues, pending approvals by kind
and upcoming events. Web charts with theme tokens; mobile tiles; the status badge reads the health
endpoint or goes. Contract test pins the old names.

<a id="s119"></a>
**S119 · 100.12 · L** After S18 (88.5, editable import grid): decide whether that grid serves as
the shared table. Build the table in `src/app/common/` with sort, paging, selection and CSV. Move
the members and payments admin lists and the three portal tables. Guard spec on the count of
hand-built tables; the rest of the admin lists move a few per later session and 100.12 stays
PARTIAL until the count is zero.

<a id="s120"></a>
**S120 · 100.13 · M** `canDeactivate` guard and dirty-form contract on web, applied to the admin
editors, the member profile and registration; back confirm on mobile profile edit and admin
editors. Specs and a widget test.

<a id="s121"></a>
**S121 · 100.14 · S** After S88 (96.3, shared header and search): the admin header search returns
members, payments and events through existing search routes, limited by role.

<a id="s122"></a>
**S122 · owner decisions · S** Done 2026-11-17. The owner answered all five; the answers are on the
closed decision item of Work Package 100 and the build cards are S123 to S129. No card for an
installable web app: the answer was no.

<a id="s123"></a>
**S123 · 100.16 · L** After S115 (100.5, sitemap and previews). `IsPublic` on the article, off by
default, with one migration (ledger row 18); anonymous list and read routes for approved public
articles; public articles pages on web, in the sitemap; the switch on the portal and mobile article
forms and on the admin moderation screen. Tests: members-only, pending and rejected articles are
refused signed out. `code-reviewer` on the migration. Split API and clients if the card runs long.

<a id="s124"></a>
**S124 · 100.17 · M** After S110 (100.1, public news links). First read how news categories are
held. Add "Spotlight" the same way; category filter on the public news route; a home page section
with the latest three, hidden when empty; the label and filter in the portal and the app.

<a id="s125"></a>
**S125 · 100.18 (API) · L** After S112 (100.3, the signed token) and S113 (100.4, the batch worker).
Subscriber table with consent times, one migration (ledger row 19); sign-up, confirm and
unsubscribe routes, anonymous and rate limited; the sign-up answer does not reveal a known address;
newsletter subscribers as a batch audience, confirmed only, a shared address mailed once; unconfirmed
addresses deleted after the OrgConfig number of days. `code-reviewer` and `security-reviewer`.

<a id="s126"></a>
**S126 · 100.18 (web) · M** Footer sign-up field, confirm and unsubscribe pages, admin subscriber
list with CSV and delete, and the audience on the batch screen with its count. The privacy page
names the list. No portal or mobile change. Closes 100.18.

<a id="s127"></a>
**S127 · 100.19 (API) · L** After S11 (84.92, payment review on clients). First read which approval
mechanism can hold a refund and record it on 100.19. Refund record with one migration (ledger row
14b); amount limited to the remainder; the recorder cannot approve; only an approved refund changes
the payment status and report totals; audit rows. No gateway call. Stop and ask if no existing
approval mechanism fits; default, wait for S73 (92.2, approval policies).

<a id="s128"></a>
**S128 · 100.19 (clients) · M** "Record refund" on the admin payment detail, the request in the
treasurer's approval queue, refunds and net totals in the reports; the refund in the member's
payment history on the portal and the app; approve or reject on mobile admin if its approval list
exists. Closes 100.19.

<a id="s129"></a>
**S129 · 100.20 · M** After S115 (100.5, public addresses that are shared). `autoVerify` web link
filter with the host from the build environment; router mapping for news, event and job addresses;
`/.well-known/assetlinks.json` under `GHCAA.Web/public/`. Stop and ask for the SHA-256 fingerprint
of the release signing key; it is never guessed. The phone check is the owner's. Record the steps
to repeat for the production domain.

<a id="s130"></a>
**S130 · 100.6 (mobile) · S** After S86 (84.69, site content on mobile) and S114. The registration
consent line and the drawer open the privacy, terms and FAQ content. Widget test. Closes 100.6.

### Track H. Book, on the owner's word

<a id="h00"></a>
H00 keeps the list of book facts changed by feature sessions. It starts with: 92.5 role matrix
(§8.4, the role matrix, and its figure), 73.5 matrix claim (§3.9 and §12.2), every repository count.
Work Package 100 (added 2026-11-17) changes the tracker counts quoted in chapter 11.

<a id="h01"></a><a id="h02"></a><a id="h03"></a><a id="h04"></a><a id="h05"></a><a id="h06"></a><a id="h07"></a><a id="h08"></a><a id="h09"></a>
H01 78.10 (coverage summary, static analysis, latency, ASVS walkthrough, each with its command and
date). H02 63.8 and 84.35 (re-take every count `wbs.py` quotes). H03 84.96 (owner restates the DC-15
claim) and 64.8. H04 64.9 and 67.3 (diagrams). H05 to H07 67.2 placeholders chapter by chapter, and
67.4. Chapters 9, 10, 12 and 13 have had prose since 16 November 2026 (67.10), so H05 to H07 fill
their numbers and figures and do not write them from headings. H08 67.7 prose joins and 67.5 page
tightening, to the 80 to 90 pages the owner set on 16 November. H09 63.34, 63.18 (only if asked), 74.3. Each H
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
| 2a | `AddMailBatches` (batch and recipient rows) | S113 | |
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
| 14a | `SeedPolicySiteContent` (three site content keys, data step) | S114 | Written 2026-11-17 with `PolicyContentSeeder` for databases built without migrations; not yet run on PostgreSQL |
| 14b | `AddRefunds` | S127 | |
| 15 | `AddSavedReports` | S95 | |
| 16, 17 | cohorts and reunions; memorial | S99, S101 | |
| 18 | `AddArticleIsPublic` | S123 | |
| 19 | `AddNewsletterSubscribers` | S125 | |

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
| `docs/specs/029-public-admin-member-experience-gaps/plan.md` | Per-client shape and order for the public, admin and member gaps | 100.1 to 100.14, 100.16 to 100.20 |

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
| `scripts/backlog_inventory.py` | At the start of a session, and `--check` after any tracker status change or card edit. `--card <id>` before writing or rewriting a card. |
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
| 2026-11-15 | 99.10 (added) | Card rewrite, phases A to C: S01 to S33 in the eleven-part form, each file, route and test name checked against the code; `python scripts/backlog_inventory.py --check docs/MASTER_EXECUTION_PLAN.md`: 0 problems; `python -m unittest discover -s scripts -p test_backlog_inventory.py`: 34 of 34 OK; tracker notes on 37.16i, 84.80, 84.92, 88.4 and the phase C items | this plan, `scripts/backlog_inventory.py`, `scripts/test_backlog_inventory.py`, `docs/TODO.md`, spec 008 `spec.md`, spec 023 `delivery-plan.md`, `CLAUDE.md`, `AGENTS.md`, `GEMINI.md`, `.github/copilot-instructions.md`, both `ghcaa-project-orchestrator` skill copies |
| 2026-11-15 | 99.10 | Card rewrite, phase D: S34 to S44 in the eleven-part form, each file, route and test name checked against the code; `python scripts/backlog_inventory.py --check docs/MASTER_EXECUTION_PLAN.md`: 0 problems; `python -m unittest discover -s scripts -p test_backlog_inventory.py`: 34 of 34 OK; tracker notes on 84.19, 84.20, 84.59 and 84.84 | this plan, `docs/TODO.md` |
| 2026-11-17 | 100.1 to 100.15 (added) | Work Package 100 opened from the public and admin site reviews, each item with its member portal and mobile part; spec 029 written (spec, plan, tasks); phase I (S110 to S122) added in the short form. No code changed. | `docs/TODO.md`, `docs/specs/029-public-admin-member-experience-gaps/`, this file, `docs/book/build/wbs.py` |
| 2026-11-17 | 100.15 (closed), 100.16 to 100.20 (added) | Owner answered D1 to D8 of spec 029 in the question dialog; answers written on 100.3, 100.5, 100.6 and 100.15; five build items opened; Work Package 100 rows moved into phases B, F and G by dependency on the owner's word; ledger rows 2a, 14a, 14b, 18, 19 added; `backlog_inventory.py --check` clean | `docs/TODO.md`, `docs/MASTER_EXECUTION_PLAN.md`, `docs/specs/029-public-admin-member-experience-gaps/` |
| 2026-11-17 | 100.6 (web and API built, item open) | Privacy, terms and FAQ pages built ahead of S114 on the owner's word: migration 14a and a startup seeder that runs only while the policy group is empty, `/privacy`, `/terms` and `/faq` on one page component, footer and consent links. Drafted text is not legal advice; admins edit or hide it in Site Content. Backend and web specs pass; mobile part stays in S130 | `GHCAA.Infrastructure/Data/`, `GHCAA.Web/src/app/public/policy-page/`, `docs/API_CONTRACT_REGISTRY.md`, this file, `docs/TODO.md`, spec 029 tasks |

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
