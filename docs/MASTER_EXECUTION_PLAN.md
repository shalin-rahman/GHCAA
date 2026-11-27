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

- **Date:** 2026-11-27. **Open:** 133 items (`python scripts/backlog_inventory.py`), held 12
  (`--state ONHOLD`).
<!-- backlog-check: off -->
- **Closed 2026-11-27 and committed:** S132 (84.103), S10 (84.92 backend; the item stays PARTIAL
  for S11) and S133 (84.104). S11 (84.92 clients) closed 2026-11-28. See the progress log.
<!-- backlog-check: on -->
- **Added 2026-11-17:** Work Package 100 (public site, admin and member experience gaps), 20 items,
  19 of them open. Owner, 2026-11-17: place its sessions by their dependencies, not last. The rows
  sit in section 3 by priority and after what they need: S110 to S113 (P1) in phase B before S14,
  S114 to S120, S127 and S128 (P2) at the end of phase F, the rest (P3) in phase G. Their cards
  stay together under "Phase I" in section 4. The owner answered decisions D1 to D8 of spec 029 the same day, so no session of the
  package waits on a gate. The decision item closed with the answers (S122, see the progress
  log); 100.16 to 100.20 are the build items.
- **Cards:** every open phase (B to I; phase A closed 2026-11-26) and track H (H00 to H09) is in the eleven-part form, checked
  against the code; track H landed 2026-11-21. A new card uses the same form.
  `python scripts/backlog_inventory.py --card <session or item id>` prints a card skeleton and
  the tracker entry.
- **Next session:** S12 (84.80 backend). S11 closed 2026-11-28. Run the sequence in section 3 top to bottom. Do not skip ahead
  unless a card's "After" column is met and the owner asks.
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
the ledger row in section 7. Each session's Summary links to its detailed card in section 4. Keep the
summary text and link in sync with that card. Run top to bottom.

| # | Items | Summary | Size | Layers | Mig | After |
|---|---|---|---|---|---|---|
| **A** | **CI health and the rest of session 2b** | | | | | |
| S01 | done 2026-11-21, see log | [Completed; see progress log](#12-progress-log) | | | | |
| S02 | done 2026-11-21, see log | [Completed; see progress log](#12-progress-log) | | | | |
| S03 | done 2026-11-21, see log | [Completed; see progress log](#12-progress-log) | | | | |
| S04 | done 2026-11-25, see log | [Completed; see progress log](#12-progress-log) | | | | |
| S05 | done 2026-11-25, see log | [Completed; see progress log](#12-progress-log) | | | | |
| S06 | done 2026-11-26, see log | [Completed; see progress log](#12-progress-log) | | | | |
| S07 | done 2026-11-26, see log | [Completed; see progress log](#12-progress-log) | | | | |
| **B** | **P1 items with no owner gate** | | | | | |
| S08 | done 2026-11-26, see log | [Completed; see progress log](#12-progress-log) | | | | |
| S09 | done 2026-11-26, see log | [Completed; see progress log](#12-progress-log) | | | | |
| S132 | done 2026-11-27, see log | [Completed; see progress log](#12-progress-log) | | | | |
| S10 | done 2026-11-27, see log | [Completed; see progress log](#12-progress-log) | | | | |
| S133 | done 2026-11-27, see log | [Completed; see progress log](#12-progress-log) | | | | |
| S11 | done 2026-11-28, see log | [Completed; see progress log](#12-progress-log) | | | | |
| S12 | 84.80 (backend) | [Cancel event registrations and promote the waitlist](#s12) | M | API | | |
| S13 | 84.80 (clients) | [Add registration cancellation to both clients](#s13) | M | web, mobile | | S12 |
| S110 | 100.1, 100.2 | [Fix public links, add Join, clean index.html](#s110) | S | web | | |
| S111 | 100.3 (API) | [Save all mail flags, add the mail check and unsubscribe](#s111) | M | API | | |
| S112 | 100.3 (clients) | [Email preferences on web and mobile, unsubscribe page](#s112) | M | web, mobile | | S111 |
| S113 | 100.4 | [Preview, confirm and queue batch email](#s113) | L | API, web | 2a | S111 |
| S14 | 84.81 | [Send due and event reminders on schedule](#s14) | L | API, OrgConfig | 3 | S111, S13 |
| S15 | 88.1 | [Make member imports unique, CSV-capable and idempotent](#s15) | L | API | 4 | S14 |
| S16 | 88.2, 88.3 | [Make batch imports transactional and report row counts](#s16) | M | API | | S15 |
| S17 | 88.5 (backend) | [Preview member imports and report cell errors](#s17) | M | API | | S16 |
| S18 | 88.5 (grid) | [Build the shared editable import data grid](#s18) | M | web | | S17 |
| S19 | 88.4 | [Verify member import counts on PostgreSQL](#s19) | S | local PostgreSQL | | S16 |
| **C** | **Election sessions 3 to 6** | | | | | |
| S20 | 37.16f | [Share the voting-member eligibility rule](#s20) | S | backend | | |
| S21 | 37.16g | [Assign election winners through governance](#s21) | M | backend | | S20 |
| S22 | 37.16h (backend) | [Add nomination photo upload and approval APIs](#s22) | L | API | 5 | S21 |
| S23 | 37.16h (clients) | [Add approved nomination photos to both clients](#s23) | M | web, mobile | | S22 |
| S24 | 37.14d and the F2 schema | [Add election safeguards and audit schema](#s24) | L | migration | 6 | S23 |
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
| S35 | 84.19 (T004 to T006, T002a routes) | [Add meeting services and API endpoints](#s35) | M | API | | S34 |
| S36 | 84.19 (T007 to T009) | [Add meeting agendas, participants and RSVPs](#s36) | M | API | | S35 |
| S37 | 84.20 (T010 to T012) | [Add meeting to-do lists](#s37) | M | API | | S36 |
| S38 | 84.19 (T013 to T015) | [Link meeting polls and audience rules](#s38) | M | API | | S37 |
| S39 | 84.19 (T016 to T018, T002a web) | [Build meeting admin and member web screens](#s39) | M | web | | S38 |
| S40 | 84.19 (T019 to T020, T002a mobile) | [Build meeting screens on mobile](#s40) | M | mobile | | S39 |
| S41 | 84.19, 84.20 (T021) | [Register and document meeting contracts](#s41) | S | docs | | S40 |
| S42 | 84.59 | [Type shared API DTOs and compare both clients](#s42) | M | API, both clients | | S41 |
| S43 | 84.84 (backend) | [Add private member locations and directory filters](#s43) | L | API | 8 | S41 |
| S44 | 84.84 (clients) | [Add location fields and filters to both clients](#s44) | M | web, mobile | | S43 |
| **E** | **Election sessions 7 to 9** | | | | | |
| S45 | 95.2 (N1) | [Add election kinds, end reasons and term settings](#s45) | L | migration | 9 | S33 |
| S46 | 95.4 | [Show vacant governance seats](#s46) | M | API, web, mobile | | S45 |
| S47 | 95.12 (backend) | [Automate term endings and manage end reasons](#s47) | M | API, security | | S46 |
| S48 | 95.12 (clients) | [Add end-reason controls to both clients](#s48) | M | web, mobile | | S47 |
| S49 | 95.5 (backend) | [Support by-elections for vacant seats](#s49) | M | API | | S48 |
| S50 | 95.5 (clients) | [Show election kinds and live elections in clients](#s50) | M | web, mobile | | S49 |
| S51 | 37.13f | [Verify hosting limits and document backup evidence](#s51) | S | docs, backend log | | S50 |
| **F** | **P2 items** | | | | | |
| S52 | 84.4 | [Script a parity table of spec 002 endpoints against both clients](#s52) | S | scripts, docs | | |
| S53 | 93.3 | [Diagnose and fix the member-registration E2E failure](#s53) | S | e2e | | |
| S54 | 84.27 (API tests) | [Test archive API status and authorization paths](#s54) | S | backend | | |
| S55 | 84.27 (web page) | [Build the admin archive moderation page](#s55) | M | web | | S54 |
| S56 | 84.55 | [Limit gallery creation to supported fields](#s56) | M | API, clients check | | |
| S57 | 84.93 | [Manage effective-dated constitution versions](#s57) | M | API, web | | |
| S58 | 84.97 | [Require termination reasons and show history](#s58) | M | API, web | | |
| S59 | 84.67 | [Add mobile password change; owner decides on step-up](#s59) | M | mobile | | |
| S60 | 34.D8 | [Repair the local admin role through the protected seeder](#s60) | M | local stack | | |
| S61 | 84.65 | [Build the scholarship admin page](#s61) | M | web | | |
| S62 | 49.3.C | [Add account contact email and reset delivery](#s62) | L | API, web | 10 | |
| S63 | 97.1 (backend) | [Add email-verification OTP endpoints](#s63) | M | API | | |
| S64 | 97.1 (clients) | [Show and complete email verification in both clients](#s64) | M | web, mobile | | S63 |
| S65 | 84.83 (backend) | [Add mentor opt-in and directory filtering](#s65) | L | API | 11 | |
| S66 | 84.83 (clients) | [Add mentor opt-in on profile edit and a mentors filter in both clients](#s66) | M | web, mobile | | S65 |
| S67 | 84.82 (backend) | [Add event feedback storage and admin summary](#s67) | L | API | 12 | |
| S68 | 84.82 (clients) | [Add event ratings to both clients](#s68) | M | web, mobile | | S67 |
| S69 | 84.72 (backend) | [Add forum moderation APIs and default categories](#s69) | L | API | 13 | |
| S70 | 84.72 (web) | [Build the admin forum tools for web](#s70) | M | web | | S69 |
| S71 | 84.72 (mobile) | [Build role-guarded forum tools for mobile](#s71) | M | mobile | | S69 |
| S72 | 92.1 | [Set approval-flow rules with the owner](#s72) | S | spec | | |
| S73 | 92.2 | [Implement configurable approval policies](#s73) | L | API | 14 if table | S72 |
| S74 | 92.3 | [Add web approval settings and queue filters](#s74) | M | web | | S73 |
| S75 | 92.4, 92.5 | [Add mobile approval gating and update specs](#s75) | S | mobile, docs | | S73 |
| S76 | 37.7 (web mechanism) | [Add Bengali translation support to web](#s76) | M | web | | |
| S77 | 37.7 (web strings) | [Translate web interface strings to Bengali](#s77) | M | web | | S76 |
| S78 | 8.8 | [Add Bengali translations to mobile](#s78) | M | mobile | | S77 |
| S79 | 62.2 | [Complete default and GHC profile data packs](#s79) | M | profiles | | |
| S80 | 51.4 | [Test file-storage exclusions and image fallback](#s80) | S | backend tests | | |
| S81 | 27.1, 27.8 | [Add cross-client coverage reporting and a local gate](#s81) | M | scripts | | |
| S82 | 73.5 | [Generate the FR and NFR traceability matrix](#s82) | M | book build script (owner allows) | | |
| S83 | 73.6 | [Pin seeded constitution text in tests](#s83) | S | backend test | | |
| S114 | 100.7, 100.8 | [Skip links, menu state and fees on the home page](#s114) | M | API, web | 14a | S110 |
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
| S134 | 84.105 | [Name or settle pending payments of archived members](#s134) | S | API, web | | |
| S135 | 84.106 | [Show the mobile receipt link only on paid rows](#s135) | S | mobile | | |
| S95 | 90.1, 90.2 | [Add saved-report storage and query service](#s95) | L | API | 15 | S18 |
| S96 | 90.3, 90.5 | [Add saved-report API endpoints](#s96) | M | API | | S95 |
| S97 | 90.4 | [Build the saved-report web page](#s97) | M | web | | S96 |
| S98 | done 2026-11-13, see log | [Completed; see progress log](#12-progress-log) | | | | |
| S99 | 37.4 (backend) | [Add cohort support to the API](#s99) | L | API | 16 | |
| S100 | 37.4 (clients) | [Add cohort features to web and mobile](#s100) | M | web, mobile | | S99 |
| S101 | 37.5 (backend) | [Add the memorial register to the API](#s101) | L | API | 17 | |
| S102 | 37.5 (clients) | [Add the memorial register to both clients](#s102) | M | web, mobile | | S101 |
| S103 | 37.10 | [Build the annual impact report across layers](#s103) | L | API, web, mobile | | S100 |
| S104 | 62.56 (a) | [Map profile-pack sections and check key uniqueness](#s104) | M | API | | S79 |
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
| S131 | 8.9 | [Move mobile to material_ui and lift the Flutter pin](#s131) | M | mobile, CI | | |
| **H** | **Book track, on the owner's word** | | | | | |
| H00 | book facts list | [Track book facts changed by feature work](#h00) | S | docs | | |
| H01 | 78.10 | [Measure coverage, analysis and operational quality](#h01) | M | measurements | | |
| H02 | 63.8, 84.35 | [Refresh the counts used in the book](#h02) | S | book counts | | H01 |
| H03 | 84.96, 64.8 | [Update the owner-approved claims](#h03) | S | book | | |
| H04 | 64.9, 67.3 | [Update book diagrams](#h04) | M | book figures | | |
| H05 | 67.2 (chapters 7, 10) | [Complete book placeholders in chapters 7 and 10](#h05) | M | book | | H01, H04 |
| H06 | 67.2 (chapter 9) | [Complete book placeholders in chapter 9](#h06) | M | book | | H05 |
| H07 | 67.2 (chapters 11 to 13), 67.4, 67.10 | [Complete book chapters 11 to 13 and 67.4](#h07) | M | book | | H06 |
| H08 | 67.7, 67.5 | [Join book prose and tighten page layout](#h08) | M | book | | H07 |
| H09 | 63.34, 63.18, 74.3 | [Complete the remaining book updates](#h09) | S | book | | H08 |

Owner-gated items and where they re-enter: section 5.

## 4. Session cards

Rules in section 2 apply to every card. A card in a rewritten phase has the eleven parts below, in
this order, and the plan check fails when one is missing or when a named file does not exist. A file
the card creates is marked "(new)". Phases still in the short form are listed by the check as owed.

<!-- card-template: B C D E F G H I -->

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

### Phase B. P1 items with no owner gate

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
- Before you start: S111 closed (it adds the mail preference check this job must ask) and S13
  closed (it adds `Cancelled`, which this job must skip). Migration 2a of the ledger (section 7) is
  applied locally.
- Read (only these): `GHCAA.API/Services/ExpiredRevokeSweep.cs` (the pattern to copy, and where it
  is registered in `Program.cs`); `GHCAA.Domain/Models/MembershipDue.cs`, `EventRegistration.cs`;
  `WorkflowDto` in `GHCAA.Application/DTOs/OrgConfigDto.cs` (line 144); `TemplateCodes` and
  `MailKinds` in `GHCAA.Domain/Constants.cs` (line 80); `IMailPreferenceService.MayReceiveAsync`
  from S111; the newest migration `20261112120000_AddElectionFormPrints`
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
     resend the ones already sent. A failed send leaves the mark empty. A due reminder uses the
     always-sent membership status kind (D8). An event reminder first calls
     `MayReceiveAsync(memberId, kind)` with the kind for `NotifyRegistrationUpdate`, the flag
     `NotificationService` already uses for registration notices; if it refuses, skip the member
     and leave the mark empty. -> check: tests.
  5. Register it in `Program.cs` beside `ExpiredRevokeSweep`. -> check: the API starts.
- Contract: no route. `WorkflowDto` gains two fields; record them in `docs/API_CONTRACT_REGISTRY.md`.
- Tests: `ReminderSweepTests.cs` :: `Due_DayBeforeWindow_NoMail`, `Due_FirstDayOfWindow_OneMail`,
  `Due_LastDayOfWindow_OneMail`, `Due_Paid_NoMail`, `SecondRun_SendsNothing`, `LeadZero_SendsNothing`,
  `Event_ApprovedInsideWindow_OneMail`, `Event_PendingOrCancelled_NoMail`,
  `FailedSend_LeavesMarkEmpty`, `Event_MemberOptedOut_NoMail`, `Due_MemberOptedOut_StillSent`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ReminderSweepTests|FullyQualifiedName~OrgConfigGoldenSnapshotTests"`;
  `python scripts/ci-local.py --only backend`; `python scripts/ci-local.py --only contract`; then
  the code-reviewer pass on the migration.
- Done when: the eleven tests pass; the migration dry run is recorded; 84.81 DONE; ledger row 3 marked.
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

Design reference for S45 to S50: `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`,
N1 (end reasons and election kind) and F10 (vacant seats, by-election, term ending). S51 follows F7.
Where the design reference disagrees with a card, the card wins.

Names in these cards were checked against the code on 2026-11-20. What the older cards and the
delivery plan assumed that the code does not have, or already has:

- `ECMember.EndReason` is a `VacancyReason?` enum (`GHCAA.Domain/Enums.cs`, values Died, Resigned,
  Removed, TermEnded, Other) stored as a string of at most 20 characters
  (`ECMemberConfiguration.cs`). The FK change has to map those strings to ids.
- Committee positions are already a table (`EcPosition`, `EcPositionIds`, migration
  `20261021100000_AddEcPositions`), and `ECMember.Position` is already an `EcPosition` id. Migration 9
  adds end reasons and the election kind only, so its ledger name becomes `AddEndReasonsAndElectionKind`.
- `GovernanceService.GetCommitteeSeatsAsync(periodId)` already works out empty seats with the last
  holder's reason, and the web governance page already has a "Vacant seats" table built from it.
- The default term length is an OrgConfig setting, not an `ECPeriod` column. There is no governance
  group in `OrgConfigDto` yet. The OrgConfig row is stored as JSON, and a missing group reads as its
  defaults.
- `ElectionService.DeclareAsync` refuses when the period already has any `ECMember`, so a by-election
  in a running period cannot declare today.
- `GET api/elections/current` (`ElectionService.GetCurrentAsync`) returns one election or none.
- Background jobs live in `GHCAA.API/Services/` (`DataRetentionSweep`, `ExpiredRevokeSweep`) and are
  registered with `AddHostedService` in `GHCAA.API/Program.cs`.
- The vacancy reason lists are hard-coded on both clients: `VACANCY_REASON_OPTIONS` in
  `GHCAA.Web/src/app/core/constants/app.constants.ts` and `GovernanceConstants.vacancyReasonOptions`
  in `GHCAA.Mobile/lib/core/constants/app_constants.dart`.

Tests carry the FR or NFR category of spec 023 where the delivery plan names one.

<a id="s45"></a>
**S45 · 95.2 (N1) · L · migration 9**
- Goal: the end reason of a committee term is a row in a new `EcEndReason` table instead of an enum,
  and an election has a kind (General or ByElection) with the live-election index keyed on period and
  kind. One migration, no client change.
- Before you start: S33 is closed and migration 8 of the ledger (section 7, the migration ledger) is
  applied locally. The owner has confirmed that `20261021100000_AddEcPositions` is applied on preprod
  and that `GET api/admin/governance/positions/invalid-rows` there returns no rows (or the repair has
  run). Without that answer, ask through AskUserQuestion and stop.
- Read (only these): the N1 part of the delivery plan; `GHCAA.Domain/Models/EcPosition.cs` (the model
  and `EcPositionIds.Seed()`); `GHCAA.Domain/Models/ECMember.cs`; `GHCAA.Domain/Models/Election.cs`;
  `GHCAA.Infrastructure/Data/Configurations/EcPositionConfiguration.cs`, `ECMemberConfiguration.cs`
  and `ElectionConfigurations.cs`; `GHCAA.Infrastructure/Data/Migrations/PgSql/20261021100000_AddEcPositions.cs`
  (guards, `setval`, the orphan check and the refusing Down); the live index SQL in
  `20261009130000_AddBallotOrderAndTestElection.cs`; `WarnIfLiveElectionIndexMissingAsync` and
  `LiveElectionsSharingAPeriodAsync` in `GHCAA.Infrastructure/Data/MigrationBootstrapper.cs`;
  `GHCAA.Tests/Data/EcPositionSeedSyncTests.cs` and `GHCAA.Tests/Data/LiveElectionIndexCheckTests.cs`.
- Change (only these files): `GHCAA.Domain/Enums.cs`, `GHCAA.Domain/Models/EcEndReason.cs` (new),
  `ECMember.cs`, `Election.cs`, `GHCAA.Domain/Constants.cs`;
  `GHCAA.Infrastructure/Data/Configurations/EcEndReasonConfiguration.cs` (new), `ECMemberConfiguration.cs`,
  `ElectionConfigurations.cs`, `ApplicationDbContext.cs`, `MigrationBootstrapper.cs`; every file that
  uses `VacancyReason` today: `ECHistoryDto.cs`, `GovernanceDto.cs`, `IGovernanceService.cs`,
  `GovernanceService.cs`, `MemberService_Approval.cs`, `MemberService_Profile.cs`,
  `MemberService_Search.cs`, `AdminGovernanceController.cs`; a new PgSql migration
  `AddEndReasonsAndElectionKind` with its `.Designer.cs`, and `PgSqlApplicationDbContextModelSnapshot.cs`;
  `GHCAA.Tests/Data/EcEndReasonSeedSyncTests.cs` (new), `LiveElectionIndexCheckTests.cs`,
  `GovernanceServiceTests.cs`; `docs/TODO.md`, the ledger row in this plan.
- Do not touch: `Seed/*.json` (the `ec_members.json` files carry no end reason), any client, the
  election create and declare logic (S49), the end-term job (S47).
- Steps:
  1. `EcEndReason` with `Id`, `Key`, `Name`, `NameBn`, `DisplayOrder`, `IsSystem`, `IsActive`, shaped
     like `EcPosition`. `EcEndReasonIds` with `Died = 1`, `Resigned = 2`, `Removed = 3`,
     `TermEnded = 4`, `Other = 5` (the enum order, one-based) and a `Seed()` the context feeds to
     `HasData`. `TermEnded` and `Died` are `IsSystem`: an admin cannot retire or rename them.
     Lengths come from new constants in `Constants.Governance`. -> check: the project builds.
  2. `ECMember.EndReason` becomes `int? EndReasonId` with a navigation `EndReason`, FK restrict on
     delete. Add `ElectionKind { General = 0, ByElection = 1 }` to `Enums.cs` and
     `Election.Kind` (default General). Remove `VacancyReason` from `Enums.cs`. -> check: the build
     lists every old use; each one now reads or writes an id from `EcEndReasonIds`.
  3. `GovernanceService.EndTerm` takes an end reason id. A note is still required for `Other`. An
     unknown or retired id is refused with the error the end-term route already returns for a bad
     reason. `CommitteeSeatDto` and `ECHistoryDto` carry the id and the reason name. -> check:
     `GovernanceServiceTests` pass after the rename.
  4. The live index moves to (`ECPeriodId`, `Kind`) in `ElectionConfigurations.cs`, keeping
     `Constants.Elections.LiveIndexName` and the same filter. `LiveElectionsSharingAPeriodAsync`
     groups by period and kind, and the hint in `WarnIfLiveElectionIndexMissingAsync` names both
     columns. -> check: `LiveElectionIndexCheckTests` updated and passing.
  5. Migration per rule 5 of section 2 (the migration rule), every statement guarded:
     create `EcEndReasons`, insert the five rows with `ON CONFLICT DO NOTHING`, advance the identity
     sequence with `setval(pg_get_serial_sequence(...))`; add `ECMembers.EndReasonId`, fill it from
     the old string column with a `CASE` on the five names, then drop the string column; add the FK
     unless an unmapped string remains, in which case `RAISE WARNING` and leave the column without
     the FK, as `AddEcPositions` does for positions; add `Elections.Kind` with default 0; drop and
     recreate the live index on the two columns inside the same clash-checked `DO` block the
     older migration uses. Down refuses with `RAISE EXCEPTION` while any `EcEndReasons` row has an
     id above 5 or any `ECMembers.EndReasonId` points at one; otherwise it restores the string column
     from the ids. -> check: the dry run in rule 5 on a fresh database and on a copy that has ended
     terms; a second run changes nothing; Down refuses after an admin-made reason is added.
- Contract: `POST api/admin/governance/members/{ecMemberId}/end-term` and the assign request take
  `endReasonId` instead of the reason name; `GET periods/{id}/seats` returns `vacancyReasonId` and
  `vacancyReasonName`. Record it in `docs/API_CONTRACT_REGISTRY.md` as a breaking change that S48
  takes up on both clients.
- Tests: `EcEndReasonSeedSyncTests.cs` :: `Seed_MatchesIds` (the five ids and keys match
  `EcEndReasonIds`), `SystemReasons_AreTermEndedAndDied`; `GovernanceServiceTests.cs` ::
  `EndTerm_UnknownReasonId_IsRefused`, `EndTerm_RetiredReason_IsRefused`,
  `EndTerm_Other_NeedsANote`; `LiveElectionIndexCheckTests.cs` ::
  `GeneralAndByElection_InOnePeriod_AreNotAClash`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~EcEndReason|FullyQualifiedName~GovernanceService|FullyQualifiedName~LiveElectionIndex"`;
  then `python scripts/ci-local.py --only backend`. Check the SQLite test database builds and
  `dotnet ef migrations has-pending-model-changes` reports none for PgSql.
- Done when: the migration applies twice on local PostgreSQL and rolls back; the SQLite tests pass;
  there are no pending model changes; `code-reviewer` has read the diff; ledger row 9 has its
  timestamp and the new name; 95.2 gets its evidence and closes if nothing else in its scope is open.
- Stop and ask if: the old string column holds a value outside the five names on a real copy; the
  live index clash check finds two live elections of one kind in a period; the owner has not
  confirmed the preprod positions migration.

<a id="s46"></a>
**S46 · 95.4 · M**
- Goal: one service method returns the empty seats of a period, the seats route can return only
  those, and both clients show them on the admin governance screens.
- Before you start: S45 is closed. `grep -n "GetVacantSeatsAsync" GHCAA.Infrastructure` shows nothing.
- Read (only these): `GetCommitteeSeatsAsync` in `GHCAA.Infrastructure/Services/GovernanceService.cs`;
  `IGovernanceService.cs`; the seats action in `GHCAA.API/Controllers/AdminGovernanceController.cs`;
  the four `GetCommitteeSeatsAsync_*` tests in `GHCAA.Tests/Services/GovernanceServiceTests.cs`;
  `GHCAA.Web/src/app/admin/governance/admin-governance.ts` and `.html` (the `vacantSeats` table);
  `GHCAA.Web/src/app/core/services/admin.service.ts` (the seats call);
  `GHCAA.Mobile/lib/screens/admin/governance_registry_screen.dart`;
  `GHCAA.Mobile/lib/features/admin/admin_service.dart`.
- Change (only these files): `IGovernanceService.cs`, `GovernanceService.cs`,
  `AdminGovernanceController.cs`, `GovernanceServiceTests.cs`, `GHCAA.Tests/Controllers/AdminGovernanceControllerTests.cs`; `admin-governance.ts`, `.html`, `.spec.ts`, `admin.service.ts`;
  `governance_registry_screen.dart`, `admin_service.dart`,
  `GHCAA.Mobile/test/governance_registry_positions_test.dart`;
  `docs/API_CONTRACT_REGISTRY.md`, `docs/TODO.md`.
- Do not touch: the public `GovernanceController` and the member governance screens; the end-term
  dialog (S48 changes its reason list).
- Steps:
  1. `GetVacantSeatsAsync(int periodId, CancellationToken ct)` returns the seats of active positions
     where the current holders are fewer than `EcPosition.SeatCount`, one row per empty seat, with
     the last holder's reason as `GetCommitteeSeatsAsync` gives it. Build it on the same query; do
     not copy it. A retired position is never vacant. -> check: the new tests below.
  2. `GET periods/{id}/seats?vacantOnly=true` returns only those rows; without the flag the route
     answers as today. An unknown period stays 404. -> check: the controller test.
  3. Web: the "Vacant seats" table reads the existing `vacantSeats` signal; add the count to the
     section heading and an empty state line from the existing empty-state style. No new route call.
     -> check: `admin-governance.spec.ts`.
  4. Mobile: the registry screen gets a "Vacant seats" section fed by the seats route with
     `vacantOnly=true` for the selected period, with the empty state the screen already uses.
     -> check: the widget test.
- Contract: `GET api/admin/governance/periods/{id}/seats` gains the optional `vacantOnly` query.
  Registry entry; both clients updated in this card.
- Tests: `GovernanceServiceTests.cs` :: `GetVacantSeatsAsync_TwoSeatPositionWithOneHolder_ReturnsOneSeat`,
  `GetVacantSeatsAsync_RetiredPosition_IsNeverVacant`, `GetVacantSeatsAsync_FullPosition_ReturnsNothing`,
  `GetVacantSeatsAsync_CarriesTheLastHoldersReason`; controller test `Seats_VacantOnly_ReturnsOnlyEmptySeats`;
  web spec "shows the vacant seat count"; mobile widget test "lists vacant seats for the period".
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~GovernanceService"`;
  `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/admin/governance/admin-governance.spec.ts`;
  `cd GHCAA.Mobile; flutter analyze; flutter test test/governance_registry_positions_test.dart --exclude-tags golden`.
- Done when: the tests pass; the page and screen show the vacant seats on the local stack; 95.4 closes
  with its evidence; `code-reviewer` has read the diff.
- Stop and ask if: the owner wants vacant seats on the public or member committee page too (that is
  a new public route and not in this card).

<a id="s47"></a>
**S47 · 95.12 (backend) · M · security**
- Goal: a term ends in two ways. A daily job, off by default, ends every open term of a period whose
  end date has passed with reason `TermEnded`. An admin ends one term early through the existing
  end-term route with a reason and a note.
- Before you start: S46 is closed. `grep -rn "TermEndSweep" GHCAA.API` shows nothing.
- Read (only these): the F10 part of the delivery plan; `GHCAA.API/Services/DataRetentionSweep.cs`
  and its registration in `GHCAA.API/Program.cs`; `GHCAA.Tests/Services/DataRetentionSweepTests.cs`;
  `ElectionSettingsDto` and `OrgConfigDto` in `GHCAA.Application/DTOs/OrgConfigDto.cs`; the save path
  in `GHCAA.Infrastructure/Services/OrgConfigService.cs`; the setting read at
  `ElectionService.cs` (`_orgConfig == null ? new ElectionSettingsDto()... : ...Elections...`);
  `GHCAA.Tests/OrgConfig/golden/org-config-ghc.golden.json` with `OrgConfigGoldenSnapshotTests.cs`;
  `EndTerm` and `RemoveMemberFromCommitteeAsync` in `GovernanceService.cs`; the end-term action in
  `AdminGovernanceController.cs`.
- Change (only these files): `OrgConfigDto.cs`, `OrgConfigService.cs` (defaults and audit only),
  `GHCAA.Domain/Constants.cs`, `IGovernanceService.cs`, `GovernanceService.cs`,
  `GHCAA.API/Services/TermEndSweep.cs` (new), `Program.cs`, `AdminGovernanceController.cs`;
  `GHCAA.Tests/Services/TermEndSweepTests.cs` (new), `GovernanceServiceTests.cs`,
  `AdminGovernanceControllerTests.cs`, the golden file and
  `OrgConfigServiceTests.cs`; `docs/API_CONTRACT_REGISTRY.md`, `docs/TODO.md`.
- Do not touch: the profile JSON under `profiles/` (hands-off rule; the defaults in the DTO cover a
  missing group), the clients (S48), `ECPeriod` (no new column, no migration).
- Steps:
  1. `GovernanceSettingsDto Governance` on `OrgConfigDto` with `TermEndJobEnabled = false` and
     `DefaultTermMonths`. The value of `DefaultTermMonths` comes from the owner (see the last part).
     It is not under `Elections`, so the election freeze does not block it. The save writes one
     activity row when either value changes. -> check: `OrgConfigServiceTests`, golden test updated.
  2. `EndExpiredTermsAsync(DateTime today, CancellationToken ct)` in the governance service: for each
     period whose end (its `EndDate`, else `StartDate` plus `DefaultTermMonths`) is before today, end
     every open `ECMember` row with `EcEndReasonIds.TermEnded` and that end date. A row already ended
     is left alone. Returns the number ended. -> check: the service tests.
  3. `TermEndSweep` copies `DataRetentionSweep`: one pass at start, then every
     `Constants.Retention.SweepHours`, a `RunOnceAsync(ct)` that reads the setting each pass and does
     nothing when it is off, and logs a failure without stopping the host. Register it beside
     `ExpiredRevokeSweep`. -> check: `TermEndSweepTests`.
  4. The end-term route refuses `TermEnded` from an admin; that reason belongs to the job. `Died`
     stays open to an admin, who records a death by hand. The note limit stays
     `Constants.Governance.VacancyNoteMaxLength`. The action stays under `AdminOnly`. -> check: the
     controller test.
- Contract: no new route. `end-term` refuses `TermEnded`; OrgConfig gains the `governance` group.
  Registry entry.
- Tests: `TermEndSweepTests.cs` :: `RunOnce_SettingOff_EndsNothing`, `RunOnce_PastPeriodEnd_EndsOpenTerms`,
  `RunOnce_NoEndDate_UsesDefaultTermMonths`, `RunOnce_SecondPass_ChangesNothing`;
  `AdminGovernanceControllerTests.cs` :: `EndTerm_AdminCannotUseTermEnded`; `OrgConfigServiceTests.cs` ::
  `Governance_MissingGroup_ReadsDefaults`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~TermEndSweep|FullyQualifiedName~GovernanceService|FullyQualifiedName~OrgConfig"`;
  then `python scripts/ci-local.py --only backend`.
- Done when: the tests pass; `security-reviewer` has read the end-term route and the job (F10 asks
  for it); 95.12 gets an evidence note and stays IN PROGRESS until S48.
- Stop and ask if: the constitution names a term length (use it as `DefaultTermMonths` only with the
  owner's yes); the owner wants the job on by default; a period has neither an end date nor a start.

<a id="s48"></a>
**S48 · 95.12 (clients) · M**
- Goal: both clients take the end reasons from the server, send `endReasonId`, never offer
  `TermEnded`, and the web settings page edits the two governance settings.
- Before you start: S47 is closed.
- Read (only these): `VACANCY_REASON_OPTIONS`, `VACANCY_REASON_OTHER`, `VACANCY_NOTE_MAX_LENGTH` and
  `getVacancyReasonLabel` in `GHCAA.Web/src/app/core/constants/app.constants.ts`;
  `admin-governance.ts` and `.html`; `admin.service.ts` (seats and end-term calls);
  `GHCAA.Web/src/app/admin/org-config/org-config.html` and its `.ts`;
  `GovernanceConstants.vacancyReasonOptions` in `GHCAA.Mobile/lib/core/constants/app_constants.dart`;
  the end-term dialog in `governance_registry_screen.dart`; `admin_service.dart`;
  `GHCAA.Mobile/lib/core/config/org_config.dart`.
- Change (only these files): `AdminGovernanceController.cs` (the reasons list action),
  `IGovernanceService.cs`, `GovernanceService.cs`, `AdminGovernanceControllerTests.cs`; `app.constants.ts`, `admin-governance.ts`, `.html`,
  `.spec.ts`, `admin.service.ts`, the governance model file the seats type lives in,
  `org-config.html`, `org-config.ts`, its spec; `app_constants.dart`,
  `governance_registry_screen.dart`, `admin_service.dart`, `org_config.dart`,
  `GHCAA.Mobile/test/governance_registry_positions_test.dart`;
  `docs/API_CONTRACT_REGISTRY.md`, `docs/TODO.md`.
- Do not touch: the public and member governance screens; any backend logic beyond the list action.
- Steps:
  1. `GET api/admin/governance/end-reasons` returns the active reasons (id, key, name, `isSystem`)
     in display order, under `AdminOnly`. -> check: controller test.
  2. Web: the end-term select loads that list, leaves out `TermEnded`, sends `endReasonId`, and keeps
     the note rule for `Other` by key. Remove `VACANCY_REASON_OPTIONS` and `getVacancyReasonLabel`;
     the seats table shows the name the server sends. Keep `VACANCY_NOTE_MAX_LENGTH`. -> check:
     `admin-governance.spec.ts` and `npm run type-check`.
  3. Web settings: a "Committee terms" block on the org-config page with the job switch and the
     default term in months, using the page's existing field components. -> check: its spec.
  4. Mobile: the dialog loads the list the same way, sends `endReasonId`; remove
     `GovernanceConstants.vacancyReasonOptions`; `org_config.dart` reads the new group with the same
     defaults. -> check: widget test and `flutter analyze`.
- Contract: new `GET api/admin/governance/end-reasons`. Registry entry; both clients in this card.
- Tests: controller `EndReasons_ReturnsActiveInOrder`; web spec "end-term list comes from the server
  and has no Term ended"; web spec "saves the committee term settings"; mobile widget test "end-term
  dialog sends the reason id".
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/admin/governance/admin-governance.spec.ts`;
  `cd GHCAA.Mobile; flutter analyze; flutter test test/governance_registry_positions_test.dart --exclude-tags golden`;
  then `python scripts/ci-local.py --only contract`.
- Done when: the tests pass; ending a term works on the local stack from both clients; 95.12 closes
  with its evidence; `code-reviewer` has read the diff.
- Stop and ask if: the owner wants admins to add their own end reasons from a screen (that is a CRUD
  page like positions and a new card).

<a id="s49"></a>
**S49 · 95.5 (backend) · M**
- Goal: an admin can create a by-election in a running period. It offers vacant seats only, refuses
  a held seat at create and again at declare, and declare never ends a holder. The live election
  route returns the general election and any live by-election.
- Before you start: S48 is closed. `grep -n "ElectionKind" GHCAA.Infrastructure/Services/ElectionService.cs`
  shows nothing.
- Read (only these): the F10 part of the delivery plan; `CreateAsync`, `GetCurrentAsync`,
  `AddSeatAsync`, `CheckSeatAsync` and `DeclareAsync` in
  `GHCAA.Infrastructure/Services/ElectionService.cs`; `IElectionService.cs`; `CreateElectionDto`,
  `CreateAdminElectionRequest`, `ElectionSummaryDto` and `AdminElectionDto` in
  `GHCAA.Application/DTOs/ElectionDtos.cs`; the create action in
  `GHCAA.API/Controllers/AdminElectionsController.cs`; the `current` action in `ElectionsController.cs`;
  the field rows near "Election title" in `GHCAA.Infrastructure/Services/ElectionDocumentService.cs`;
  `GetVacantSeatsAsync` from S46; `GHCAA.Tests/Services/ElectionServiceTests.cs`.
- Change (only these files): `ElectionDtos.cs`, `IElectionService.cs`, `ElectionService.cs`,
  `ElectionDocumentService.cs`, `AdminElectionsController.cs`, `ElectionsController.cs`,
  `GHCAA.Domain/Constants.cs`; `ElectionServiceTests.cs`, `ElectionDocumentServiceTests.cs`,
  `AdminElectionsControllerTests.cs`, `ElectionsControllerTests.cs`; `docs/API_CONTRACT_REGISTRY.md`, `docs/TODO.md`.
- Do not touch: the ballot seal, count and vacuum code; the approval service; the clients (S50).
- Steps:
  1. `Kind` on `CreateElectionDto`, `CreateAdminElectionRequest`, `ElectionSummaryDto` and
     `AdminElectionDto`, default General. -> check: the build and the existing tests.
  2. Create, by-election: the period must be the active one and may already have a committee (the
     "no committee yet" and "starts after polling" checks apply to General only). Each seat it asks
     for must be vacant by `GetVacantSeatsAsync`, and no more seats of a position than are vacant;
     otherwise a new error code `seat-held`. A General create is unchanged. -> check: service tests.
  3. `AddSeatAsync` and `CheckSeatAsync` apply the same rule to a by-election. -> check: service test.
  4. Declare, by-election: skip the "period has members" refusal; inside the existing transaction,
     check each elected seat is still vacant and, if one is not, roll back and return false with a
     logged reason. Winners start on the declare date, not the period start. A General declare is
     unchanged. Declare never sets `EndDate` on any row. -> check: service tests.
  5. `GetLiveAsync` returns every non-test election in a live phase, General first; `GetCurrentAsync`
     keeps its shape and returns the General one if live, else the by-election, so old clients keep
     working. New `GET api/elections/live` returns the list. -> check: controller test.
  6. The ER forms show the kind in the field rows beside "Election title". -> check: a document
     service test reads the row.
- Contract: create requests and summary DTOs gain `kind`; new `GET api/elections/live`; new create
  error `seat-held`. Registry entry; S50 takes it up on both clients.
- Tests: `ElectionServiceTests.cs` :: `Create_ByElection_HeldSeat_IsRefused`,
  `Create_ByElection_VacantSeat_IsAccepted`, `Create_ByElection_MoreSeatsThanVacant_IsRefused`,
  `Declare_ByElection_DoesNotEndAnyHolder`, `Declare_ByElection_SeatFilledSinceCreate_IsRefused`,
  `Declare_General_StillRefusesWhenPeriodHasMembers`, `GetLive_ReturnsGeneralAndByElection`;
  `ElectionsControllerTests.cs` :: `Live_SkipsTestElections`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ElectionService|FullyQualifiedName~ElectionsController|FullyQualifiedName~ElectionDocument"`;
  then `python scripts/ci-local.py --only backend`.
- Done when: the tests pass, and the existing create and declare tests pass unchanged; `code-reviewer`
  has read the diff; 95.5 gets an evidence note and stays IN PROGRESS until S50.
- Stop and ask if: a by-election needs its own nomination or polling rules beyond the general ones;
  the owner wants a by-election in a period that is not active.

<a id="s50"></a>
**S50 · 95.5 (clients) · M**
- Goal: both clients create a by-election from the vacant seats, label the kind on the election
  pages, ballot and results, and show the general election and a live by-election side by side.
- Before you start: S49 is closed.
- Read (only these): `GHCAA.Web/src/app/admin/elections/admin-elections.ts` and `.html` (the create
  form beside `isTest`); `GHCAA.Web/src/app/core/services/elections.service.ts`;
  `ElectionSummaryDto` in `GHCAA.Web/src/app/core/models/election.models.ts`; the `CURRENT` route in
  `app.constants.ts`; `GHCAA.Web/src/app/public/elections/elections.ts` and `.html`;
  `GHCAA.Mobile/lib/features/elections/election_service.dart` (`ElectionSummary`);
  `GHCAA.Mobile/lib/screens/admin/election_management_screen.dart` (the create dialog beside
  `isTest`); `GHCAA.Mobile/lib/screens/member/election_screen.dart` and `election_results_screen.dart`.
- Change (only these files): the web files above and their specs; the four mobile files above and
  `GHCAA.Mobile/test/election_management_screen_test.dart`, `election_screen_test.dart`,
  `election_results_screen_test.dart`; `docs/API_CONTRACT_REGISTRY.md` (client column),
  `docs/TODO.md`; `GHCAA.Web/tests/e2e/election-walkthrough/election.walkthrough.ts` and its
  `README.md` (one step and one table row).
- Do not touch: any backend file; the ballot sealing code on either client; the walkthrough's
  `scenario.json` (hands-off JSON data).
- Steps:
  1. Web admin create: a kind choice. For a by-election, the seats come from
     `GET .../seats?vacantOnly=true` of the active period and only those can be picked; the
     `seat-held` error shows its message. -> check: `admin-elections.spec.ts`.
  2. Web public elections page: read `GET api/elections/live` and show each election with a
     "By-election" label where the kind says so; the label also shows on the ballot and results
     views of that page. -> check: `elections.spec.ts`.
  3. Mobile admin create: the same kind choice and vacant-seat list. -> check: widget test.
  4. Mobile member election and results screens: read the live list, show both, label the kind.
     -> check: widget tests and `flutter analyze`.
  5. Walkthrough: add a by-election step after the declare step and a row in the README's "What the
     run covers" table. -> check: the step runs on the local stack.
- Contract: no change; both clients switch to `GET api/elections/live`.
- Tests: web spec "by-election offers only vacant seats"; web spec "shows the by-election label";
  mobile widget tests "create dialog lists vacant seats for a by-election" and "member screen shows
  two live elections".
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/admin/elections/admin-elections.spec.ts src/app/public/elections/elections.spec.ts`;
  `cd GHCAA.Mobile; flutter analyze; flutter test test/election_management_screen_test.dart test/election_screen_test.dart test/election_results_screen_test.dart --exclude-tags golden`;
  then the walkthrough as its `README.md` says (`npm run test:e2e:walkthrough` with the stage variables).
- Done when: the tests and the walkthrough step pass; 95.5 closes with its evidence; `code-reviewer`
  has read the diff.
- Stop and ask if: showing two live elections on the member screen needs a layout the owner has not
  seen; the walkthrough step needs a change to `scenario.json`.

<a id="s51"></a>
**S51 · 37.13f · S**
- Goal: every *[to confirm]* mark in `docs/Elections/09-Election-Security-and-Compliance.md` is
  answered or turned into a stated known limit, and a failed count writes a log line.
- Before you start: S50 closed (run order only; no shared files). `grep -c "to confirm" docs/Elections/09-Election-Security-and-Compliance.md`
  gives 13 lines today (11 open points plus the line that explains the mark and one repeat).
- Read (only these): the 37.13f entry in `docs/TODO.md`; doc 09; the F7 part of the delivery plan;
  `docs/DEPLOYMENT_CHECKLIST.md` (the branch and backup lines); `docs/RECOVERY_RUNBOOK.md` Scenario 2;
  `CountAsync` in `GHCAA.Infrastructure/Services/ElectionService.cs` (the `wrong-key`, `integrity`,
  `below-threshold` and `unreadable` returns).
- Change (only these files): doc 09, `docs/DEPLOYMENT_CHECKLIST.md`, `ElectionService.cs`,
  `GHCAA.Tests/Services/ElectionServiceTests.cs`, `docs/TODO.md`.
- Do not touch: the constitution and regulation text; `docs/book/`.
- Steps:
  1. Doc 09: Render and Neon points take the owner's 2026-11-12 decision (free Neon, free Render):
     availability is a known limit with no SLA and no DDoS cover beyond the platform's; log retention
     and backup are what the free plans give, with RTO and RPO stated from them and the manual Neon
     branch backup in the checklist. -> check: no Render or Neon *[to confirm]* left.
  2. The point at the "regulations do not name this case" line goes to the owner as a regulation
     change, as F7 says: replace the mark with "Open with the owner as a regulation change" and the
     tracker id. -> check: grep shows only the explaining line.
  3. `DEPLOYMENT_CHECKLIST.md`: a warning that a push to `preprod` deploys, beside the existing
     branch line. -> check: read it back.
  4. `CountAsync` logs one warning with the election id and the error code on each failed return,
     with no key, ballot or voter data in the line. -> check: the test below.
- Contract: none.
- Tests: `ElectionServiceTests.cs` :: `Count_WrongKey_LogsAWarningWithoutTheKey`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ElectionServiceTests.Count"`;
  then read the changed lines of doc 09 back against the tone rules.
- Done when: doc 09 has no open *[to confirm]* mark; the test passes; 37.13f closes with its evidence
  or keeps only the owner's regulation point open.
- Stop and ask if: a free-plan figure (log retention, backup window) cannot be found in the Render or
  Neon docs; the owner wants a paid plan named.

### Phase F. P2 items

Names in these cards were checked against the code on 2026-11-20. What the older cards assumed that
the code does not have, or already has:

- Spec 004 has no endpoint table. The endpoint tables are the domain files in
  `docs/specs/002-workflow-contracts-and-validation/evidence/`, and spec 002 Story 4 defines the
  parity table.
- `MembershipHistory` already exists (`RecordMembershipChangeAsync`, `GET` behind
  `FinancialsController.GetMembershipHistory`), but `AdminUpdateMemberAsync` writes it only for a
  membership type change, not for a status change.
- `POST api/profile/change-password` has no `[RequireStepUp]`. Mobile has no change-password call
  and no settings screen.
- The scholarship API has no edit or close routes.
- `User` already has `Email`. A member cannot change their own email; only the admin update can.
- `ForumTopic` already has `IsPinned` and `IsLocked`. The mobile admin routes have no role guard.
- `EventRegistration` has no "attended" status: attendance is `Approved` or `IsCheckedIn`.
- Spec 022 lists its nine flows in the "In scope" line and its open questions under "Decisions to
  confirm before implementation". `MemberApprovalMode` is read by no C# code.
- The web app has no i18n layer and no Bengali font. Mobile has a flat dictionary of 29 keys in
  `app_localizations.dart`.
- `membership-tiers.json` and `governance.json` have no reader anywhere. `ProfilePackFiles` reads
  `email_templates.json`, `site_content.json`, `themes.json` and `constitution.json`, falling back to
  `GHCAA.Infrastructure/Data/Seed/`.
- `traceability.py` lives in `docs/book/build/` and scans C# tags only. `ci-local.py` collects no
  coverage; the CI workflow does.

<a id="s52"></a>
**S52 · 84.4 · S**
- Goal: one parity table that says, for each endpoint in the spec 002 domain contract tables, whether
  web and mobile call it and how each reads the response.
- Before you start: no card dependency. The domain tables in spec 002 `evidence/` are complete.
- Read (only these): spec 002 `spec.md` Story 4 (the parity table definition); the seven domain files
  in `docs/specs/002-workflow-contracts-and-validation/evidence/` (`events-domain.md`,
  `governance-domain.md`, `jobs-domain.md`, `mentorship-domain.md`, `news-domain.md`,
  `payments-domain.md`, `polls-domain.md`); `docs/api/diff_swagger.py` for its route parsing.
- Change (only these files): `scripts/client_parity.py` (new);
  `docs/specs/002-workflow-contracts-and-validation/evidence/client-parity.md` (new);
  `scripts/test_client_parity.py` (new); `docs/TODO.md`.
- Do not touch: client code; the domain tables (report a wrong row instead of fixing it here).
- Steps:
  1. The script reads each domain table's method and route, turns route parameters into a pattern,
     and searches `GHCAA.Web/src` (`.ts`) and `GHCAA.Mobile/lib` (`.dart`) for the route literal or
     its `API_ENDPOINTS` constant. -> check: it finds the known web and mobile callers of
     `GET api/events`.
  2. It writes `client-parity.md` with columns endpoint, web file, mobile file, response difference.
     The difference column is filled by reading the two call sites for each row both clients call.
     -> check: no row has an empty web and mobile cell without a "not called" mark.
  3. The script has `--check`, which exits non-zero when the table is stale. -> check: run twice; the
     second run reports no change.
- Contract: none.
- Tests: `scripts/test_client_parity.py` :: `test_route_with_parameter_matches`,
  `test_constant_reference_matches`, `test_uncalled_route_is_marked`.
- Run: `python scripts/client_parity.py --check </dev/null`;
  `python -m unittest discover -s scripts -p test_client_parity.py`.
- Done when: the table exists and the check passes; 84.4 closes with the table as evidence; each
  mismatch found becomes its own tracker line.
- Stop and ask if: a domain table route does not exist in the controllers.

<a id="s53"></a>
**S53 · 93.3 · S**
- Goal: `registerNewMemberViaUi` registers a member on the local API again, so the five specs that use
  it run.
- Before you start: the local stack runs (`.\GHCAA.Tools\run-app.ps1`).
- Read (only these): the 93.3 entry; `GHCAA.Web/tests/e2e/utils/ui-helper.ts` (`registerNewMemberViaUi`
  at line 41); the tail of `api_error.log` after one failing run; the registration response body the
  helper receives; `GHCAA.Web/tests/e2e/membership-walkthrough/membership.walkthrough.ts` (the
  registration step that works).
- Change (only these files): `GHCAA.Web/tests/e2e/utils/ui-helper.ts`, or the one source file the cause
  is in; `docs/TODO.md`.
- Do not touch: `Seed/*.json`; `scenario.json`; secrets and connection strings.
- Steps:
  1. Run `portal-mobile-nav.spec.ts` once and capture the API's answer. -> check: the cause is named
     (a field, a setting or a validation message).
  2. Fix the cause where it lives. Test data in the helper is fixed in the helper; a server bug is
     fixed in the server with a unit test. -> check: the same run passes.
- Contract: none, unless the cause is a server bug that changes a response.
- Tests: the five specs that call the helper; a unit test only when the fix is server code.
- Run: `cd GHCAA.Web; npx playwright test tests/e2e/portal-mobile-nav.spec.ts tests/e2e/gallery.spec.ts tests/e2e/admin-workflow.spec.ts tests/e2e/article-editorial.spec.ts tests/e2e/job-hub.spec.ts`.
- Done when: the five specs pass locally; 93.3 closes with the cause and the run.
- Stop and ask if: the fix needs an OrgConfig change on the shared database; the cause is a seed
  file.

<a id="s54"></a>
**S54 · 84.27 (API tests) · S**
- Goal: every `ArchiveController` route has controller tests for success, no login, wrong role and
  not found.
- Before you start: no card dependency.
- Read (only these): `GHCAA.API/Controllers/ArchiveController.cs`; `IArchiveService`;
  `GHCAA.Tests/Controllers/GalleryControllerTests.cs` (the pattern: NUnit, Moq, controller built with
  `new`, a `ClaimsPrincipal`); `GHCAA.Tests/Controllers/AuthorizationPolicyReflectionTests.cs`.
- Change (only these files): `GHCAA.Tests/Controllers/ArchiveControllerTests.cs` (new).
- Do not touch: the controller and service, unless a test finds a bug; then stop and ask.
- Steps:
  1. One test per route for the success result: GET public, GET `items/{id}`, the eight `admin`
     routes and POST items. -> check: each test passes.
  2. Not found: `items/{id}`, the admin item and collection PUT, DELETE and moderate return 404 when
     the service returns null or false. -> check: passes.
  3. Roles: a reflection test that the admin routes carry `AdminOnly`, POST items carries
     `[Authorize]` and the two public GETs allow anonymous. 401 and 403 come from the policy, so they
     are checked this way, not by calling the action. -> check: passes.
  4. POST items by a non-admin is passed to the service as Draft or Pending, whatever status the body
     asks for. -> check: passes.
- Contract: none.
- Tests: `ArchiveControllerTests.cs` :: `GetPublic_ReturnsOk`, `GetItem_Missing_ReturnsNotFound`,
  `AdminRoutes_RequireAdminOnly`, `PublicRoutes_AllowAnonymous`,
  `CreateItem_ByMember_IsNeverPublished`, and one success test per admin route.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ArchiveControllerTests"`.
- Done when: the tests pass; the 84.27 entry records the API part done.
- Stop and ask if: a test shows a route that leaks a non-public item to anonymous callers.

<a id="s55"></a>
**S55 · 84.27 (web page) · M**
- Goal: admins moderate archive items and collections on an `admin/archive` page.
- Before you start: S54 is closed.
- Read (only these): `ArchiveController.cs` (admin routes); `GHCAA.Web/src/app/core/services/archive.service.ts`;
  `GHCAA.Web/src/app/admin/polls/polls.component.ts` and its template (the model page);
  `app.routes.ts` around line 295; `API_ENDPOINTS` in `core/constants/app.constants.ts` (`ADMIN` at
  line 582, `ARCHIVE` at 643); `ADMIN_NAV_SECTIONS` in `core/services/nav.service.ts`.
- Change (only these files): `app.constants.ts` (an `ADMIN_ARCHIVE` block); `archive.service.ts`;
  `GHCAA.Web/src/app/admin/archive/admin-archive.ts` (new), `admin-archive.html` (new),
  `admin-archive.scss` (new), `admin-archive.spec.ts` (new); `app.routes.ts`; `nav.service.ts`;
  `docs/TODO.md`.
- Do not touch: the public archive page; mobile; the API.
- Steps:
  1. `ADMIN_ARCHIVE` endpoints and admin methods in `archive.service.ts`. -> check: type-check.
  2. The page uses `PageHeaderComponent`, `SearchBarComponent`, `LoadingPanelComponent`,
     `ModalHeaderComponent` and `Icon`, as the polls page does: an items list with a status filter,
     moderate (approve or reject with a reason), edit and delete; a collections tab with create, edit
     and delete. -> check: the page renders against the local API in both themes.
  3. Route under the admin routes and a nav entry in the content section. -> check: the nav spec
     passes.
- Contract: none (routes exist).
- Tests: `admin-archive.spec.ts` :: loads items, moderates with a reason, refuses an empty reason,
  deletes after confirm; `nav.service.spec.ts` :: the archive entry is present.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/admin/archive src/app/core/services/nav.service.spec.ts`.
- Done when: tests pass; the page was checked in the browser; 84.27 DONE.
- Stop and ask if: a moderation action the page needs has no route.

<a id="s56"></a>
**S56 · 84.55 · M**
- Goal: `POST api/gallery/admin` binds a DTO with the fields a caller may set, not the `EventGallery`
  entity.
- Before you start: no card dependency.
- Read (only these): `GalleryController.cs` (`CreateGallery` at line 106); `GalleryService.cs`
  (`CreateEventGalleryAsync` at line 27); `GHCAA.Domain/Models/EventGallery.cs`; web
  `core/services/gallery.service.ts` (`createGallery`) and `admin/gallery/admin-gallery.ts` (`onSubmit`);
  mobile `features/content/content_service.dart` (`createGallery`) and `screens/admin/admin_modules.dart`
  (`_showGalleryDialog`).
- Change (only these files): `GHCAA.Application/DTOs/GalleryDtos.cs` (new); `IGalleryService.cs`;
  `GalleryService.cs`; `GalleryController.cs`; `GHCAA.Tests/Services/GalleryServiceTests.cs`;
  `GHCAA.Tests/Controllers/GalleryControllerTests.cs`; `gallery.service.ts`;
  `docs/API_CONTRACT_REGISTRY.md`; `docs/TODO.md`.
- Do not touch: the update route; other gallery routes; the model.
- Steps:
  1. List the fields both clients send today. `CreateGalleryDto` holds those and nothing else, with
     DataAnnotations lengths from `Constants`. -> check: build.
  2. The service maps the DTO to a new entity; status, ids, counts and dates are set by the server.
     -> check: the test below.
  3. Type the web `createGallery` argument. Mobile sends a map with the same keys; change it only if a
     key name differs. -> check: web type-check; `flutter analyze`.
- Contract: request body of `POST api/gallery/admin` becomes `CreateGalleryDto`; record it in the
  registry (same keys, extra keys ignored).
- Tests: `GalleryServiceTests.cs` :: `CreateGallery_IgnoresServerFields`;
  `GalleryControllerTests.cs` :: `CreateGallery_PassesTheDto`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~Gallery"`;
  `cd GHCAA.Web; npm run type-check`; `cd GHCAA.Mobile; flutter analyze`.
- Done when: tests pass; one gallery is created from each client locally; 84.55 DONE.
- Stop and ask if: a client sends a field the DTO should not accept but the page needs.

<a id="s57"></a>
**S57 · 84.93 · M**
- Goal: an admin publishes a new constitution version with an effective date; the public page shows
  the version in force.
- Before you start: no card dependency.
- Read (only these): `GHCAA.Domain/Models/Constitution.cs`; `GovernanceController.cs` (constitution
  routes at lines 50 to 68); `GetActiveConstitutionAsync` in `IGovernanceService.cs` and
  `GovernanceService.cs` (line 355); `GHCAA.Infrastructure/Data/ConstitutionSeeder.cs`;
  `AdminGovernanceController.cs`; web `public/constitution/constitution.ts` and
  `admin/governance/admin-governance.ts`.
- Change (only these files): `GHCAA.Application/DTOs/ConstitutionDtos.cs` (new);
  `IGovernanceService.cs`; `GovernanceService.cs`; `AdminGovernanceController.cs`;
  `GHCAA.Tests/Services/GovernanceServiceTests.cs`;
  `GHCAA.Tests/Controllers/AdminGovernanceControllerTests.cs`; web `app.constants.ts`,
  `admin-governance.ts`, its template and `admin-governance.spec.ts`;
  `docs/API_CONTRACT_REGISTRY.md`; `docs/TODO.md`.
- Do not touch: `ConstitutionSeeder.cs` and the seed file; the amendment vote routes; mobile.
- Steps:
  1. `POST api/admin/governance/constitution` (AdminOnly) takes version, content, PDF URL, effective
     date and change summary. A second version on the same effective date is refused. -> check: the
     tests below.
  2. The version in force is the newest with an effective date on or before today; a superseded
     version's `SupersededDate` is the next version's effective date, computed on read, so no job.
     `GetActiveConstitutionAsync` follows that rule. -> check: a future version is not shown yet.
  3. A "New version" form on the admin governance page and a list of versions. -> check: browser.
  4. Check that the seeder does not undo an admin version on the next boot. -> check: restart the
     API locally and read the page.
- Contract: new route; record it in the registry.
- Tests: `GovernanceServiceTests.cs` :: `PublishConstitution_FutureDate_NotActiveYet`,
  `PublishConstitution_SameEffectiveDate_IsRefused`, `ActiveConstitution_IsNewestInForce`;
  `AdminGovernanceControllerTests.cs` :: `PublishConstitution_RequiresAdminOnly`;
  `admin-governance.spec.ts` :: submits the form.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~Governance"`;
  `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/admin/governance`.
- Done when: tests pass; 84.93 DONE; Secretary Task 5 (78.9, the secretary's constitution task) is
  told it is unblocked in its entry.
- Stop and ask if: the seeder overwrites admin versions and changing it is needed.

<a id="s58"></a>
**S58 · 84.97 · M**
- Goal: an admin status change to Terminated or InactiveResigned needs a reason and writes a
  `MembershipHistory` row; the admin member page shows the history.
- Before you start: no card dependency.
- Read (only these): `AdminMemberUpdateDto`; `AdminUpdateMemberAsync` in
  `GHCAA.Infrastructure/Services/MemberService_Search.cs` (line 243, the type-change history and line
  304); `RecordMembershipChangeAsync` and `GetMemberMembershipHistoryAsync` in `IFinancialService`;
  `FinancialsController.GetMembershipHistory` (line 193); web `admin/members/admin-members.ts`;
  `MembershipHistory` in `core/models/business.models.ts`; the membership walkthrough `README.md`.
- Change (only these files): `MemberService_Search.cs`; `GHCAA.Tests/Services/MemberServiceTests.cs`;
  `GHCAA.Tests/Controllers/AdminControllerTests.cs`; web `admin-members.ts`, its template and
  `admin-members.spec.ts`; `GHCAA.Web/tests/e2e/membership-walkthrough/membership.walkthrough.ts` and
  `README.md`; `docs/TODO.md`.
- Do not touch: the `MembershipHistory` model and table; `scenario.json`; mobile.
- Steps:
  1. When `dto.Status` moves to Terminated or InactiveResigned and `MembershipChangeReason` is empty,
     refuse with 400 before any write. -> check: test.
  2. Write a history row for every status change, from and to the status, with the admin and the
     reason, through `RecordMembershipChangeAsync`. -> check: test.
  3. The member detail on the admin page shows the history from the existing route, newest first,
     dates `dd-MM-yyyy`; the status select asks for a reason when one is needed. -> check: browser.
  4. Walkthrough: a last step terminates the new member, first without a reason (refused), then with
     one, and reads the history. Add the row to "What the run covers". -> check: walkthrough run.
- Contract: `PUT api/admin/members/{id}` now returns 400 for the case above; record it.
- Tests: `MemberServiceTests.cs` :: `AdminUpdate_TerminateWithoutReason_IsRefused`,
  `AdminUpdate_StatusChange_WritesHistory`; `admin-members.spec.ts` :: shows history, asks for reason.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~MemberService|FullyQualifiedName~AdminController"`;
  `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/admin/members`; the walkthrough.
- Done when: tests and walkthrough pass; 84.97 DONE with a note that the table already existed.
- Stop and ask if: the history route is not readable by the admin role the page uses.

<a id="s59"></a>
**S59 · 84.67 · M**
- Goal: a member changes their password in the mobile app.
- Before you start: no card dependency. Ask the owner (AskUserQuestion) whether the route gets
  `[RequireStepUp]`: it has none today, and adding it changes the web flow too. Recommended: no
  step-up, as today, so web stays as it is; the current password is the check.
- Read (only these): `ProfileController.cs` lines 73 to 104; web `profile.service.ts` (line 84) and
  `member/change-password/change-password.ts`; mobile `screens/member/profile_screen.dart` (the
  "Security & Audits" section, lines 82 to 89), `core/router/app_router.dart` lines 158 to 166,
  `core/api/api_exception.dart` (`serverMessage`), `core/widgets/step_up_dialog.dart`.
- Change (only these files): `GHCAA.Mobile/lib/screens/member/change_password_screen.dart` (new);
  `GHCAA.Mobile/lib/features/auth/auth_service.dart` (it already holds the profile calls);
  `profile_screen.dart`; `app_router.dart`; `GHCAA.Mobile/test/change_password_screen_test.dart` (new);
  `docs/TODO.md`. If the owner chose step-up: `ProfileController.cs`, `ProfileControllerTests.cs`,
  `change-password.ts` and `docs/API_CONTRACT_REGISTRY.md` too.
- Do not touch: the web page unless step-up was chosen.
- Steps:
  1. A service method calling `POST api/profile/change-password` with the web's body. -> check:
     `flutter analyze`.
  2. Screen with current, new and confirm fields, the password rules the API enforces, and errors
     through `serverMessage`. -> check: widget tests.
  3. A "Change password" row in the Security section and a `/profile/change-password` route in the
     shell. -> check: `router_test.dart` passes.
  4. If step-up: the attribute on the route; the interceptor shows the dialog on mobile; web handles
     the step-up answer. -> check: both clients locally.
- Contract: none unless step-up was chosen.
- Tests: `change_password_screen_test.dart` :: success, wrong current password shows the server
  message, mismatch refused before sending; with step-up, cancel leaves the form.
- Run: `cd GHCAA.Mobile; flutter analyze; flutter test test/change_password_screen_test.dart test/router_test.dart --exclude-tags golden`.
- Done when: tests pass; the password is changed on a phone build against the local API; 84.67 DONE.
- Stop and ask if: the call does not fit `auth_service.dart`; ask before creating a new service.

<a id="s60"></a>
**S60 · 34.D8 · M**
- Goal: the local admin account gets its role through the protected SuperAdmin seeder, then the
  browser walk in the 34.D8 entry is done and recorded.
- Before you start: the local stack runs.
- Read (only these): the 34.D8 entry (the walk list); `GHCAA.Infrastructure/Data/ProtectedSuperAdminSeeder.cs`;
  `DatabaseBootstrapperExtensions.cs` lines 167 to 197; `ProtectedSuperAdmins` in
  `ConfigurationOptions.cs` (line 139); `adminGuard` in `core/guards/auth.guard.ts`.
- Change (only these files): `docs/TODO.md`, and the local user secrets for `ProtectedSuperAdmins`
  if the owner adds them (the owner edits secrets, not the session).
- Do not touch: SQL on any database; `appsettings*.json`; the seeder code.
- Steps:
  1. Find why the local admin lacks the role: the configured address, or a bootstrap that did not run.
     -> check: the cause is named.
  2. If config is the cause, give the owner the exact key and value to set, then restart. -> check:
     the admin pages open.
  3. Walk each step in the entry, including a member refused when creating a Notice. Record pass or
     fail per step in the entry. -> check: every step has a result.
- Contract: none.
- Tests: none new; a failed step becomes its own tracker line.
- Run: `.\GHCAA.Tools\run-app.ps1`; the browser.
- Done when: every walk step is recorded; 34.D8 closes or names the failed steps.
- Stop and ask if: the fix needs a SQL change or a seeder code change.

<a id="s61"></a>
**S61 · 84.65 · M**
- Goal: an admin manages scholarships on a web `admin/scholarships` page using the existing routes.
- Before you start: the API has no edit or close routes for funds or calls. Ask the owner
  (AskUserQuestion): build the page on the existing routes only (recommended; edit and close become a
  new tracker line), or add `PUT` routes first in this card.
- Read (only these): `ScholarshipsController.cs`; `IScholarshipService.cs`; `ScholarshipDtos.cs`; web
  `core/services/scholarship.service.ts`; `public/scholarships/scholarships.ts`; the polls admin page
  (model); `docs/specs/015-payments-finance/spec.md` (the scholarship part).
- Change (only these files): `app.constants.ts`; `scholarship.service.ts`;
  `GHCAA.Web/src/app/admin/scholarships/admin-scholarships.ts` (new), `admin-scholarships.html` (new),
  `admin-scholarships.scss` (new), `admin-scholarships.spec.ts` (new); `app.routes.ts`;
  `nav.service.ts`; spec 015; `docs/TODO.md`.
- Do not touch: the public page; mobile; the API unless the owner chose new routes.
- Steps:
  1. Admin methods in the service for the five admin routes. -> check: type-check.
  2. Tabs: applications (list, open, award), funds (create), calls (create), awards (disburse). Shared
     admin components; amounts formatted as the public page does. -> check: browser in both themes.
  3. Route and nav entry. -> check: nav spec.
  4. Spec 015 lists the page and what it does not do yet. -> check: read back.
- Contract: none unless routes were added.
- Tests: `admin-scholarships.spec.ts` :: loads applications, creates a fund, creates a call, disburses
  an award after confirm.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/admin/scholarships`.
- Done when: tests pass; browser check done; 84.65 DONE, or PARTIAL naming edit and close.
- Stop and ask if: the review queue (MemberOnly) is wanted on this admin page.

<a id="s62"></a>
**S62 · 49.3.C · L · migration 10**
- Goal: a SuperAdmin can set a contact email on a user account, and a password reset for that account
  is sent there; without one, the copy-and-share link flow stays.
- Before you start: migration 9 of the ledger (section 7) is applied locally.
- Read (only these): `GHCAA.Domain/Models/User.cs`; its EF configuration; `RolesController.cs`
  (`reset-password-admin`, lines 139 to 148); `UserService.SendAdminPasswordResetLinkAsync` (lines 226
  to 248); the templated email pattern in `MemberService_Profile.cs` lines 406 to 478; web
  `admin/roles/admin-roles.ts`.
- Change (only these files): `User.cs`; its configuration; `GHCAA.Domain/Constants.cs`; a new PgSql
  migration `AddUserContactEmail` with its `.Designer.cs`; `PgSqlApplicationDbContextModelSnapshot.cs`;
  `UserService.cs` and its interface; `RolesController.cs`; a DTO for the set route;
  `GHCAA.Tests/Services/UserServiceTests.cs`; `GHCAA.Tests/Controllers/RolesControllerTests.cs`;
  web `admin-roles.ts`, its template and `admin-roles.spec.ts`; `docs/API_CONTRACT_REGISTRY.md`;
  `docs/TODO.md`; ledger row 10.
- Do not touch: `User.Email`; member accounts; `Seed/*.json`.
- Steps:
  1. `User.ContactEmail` nullable, length from `Constants`, unique index filtered to non-null.
     -> check: build.
  2. Migration per rule 5 of section 2 (the migration rule), guarded, with a Down that drops the
     index and column. -> check: applies twice and rolls back locally.
  3. `PUT api/roles/users/{id}/contact-email` (SuperAdminOnly) sets or clears it; a duplicate is
     refused. -> check: test.
  4. The reset sends the email to `ContactEmail` when set and returns no URL; otherwise it returns
     `ResetUrl` as today. -> check: tests on both paths.
  5. Roles page: a contact email field per user; the reset button says which path it will take.
     -> check: browser.
- Contract: new route; reset response has no `ResetUrl` when an email was sent. Record both.
- Tests: `UserServiceTests.cs` :: `AdminReset_WithContactEmail_SendsEmail`,
  `AdminReset_WithoutContactEmail_ReturnsUrl`, `SetContactEmail_Duplicate_IsRefused`;
  `admin-roles.spec.ts` :: saves the field.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~UserService|FullyQualifiedName~RolesController"`;
  `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/admin/roles`.
- Done when: tests pass; no pending model changes; `code-reviewer` has read the migration; ledger
  row 10 has its timestamp; 49.3.C DONE.
- Stop and ask if: the email template needs a new seed row (seed JSON is hands off).

<a id="s63"></a>
**S63 · 97.1 (backend) · M**
- Goal: a member verifies their email from the profile with a one-time code.
- Before you start: no card dependency. A member cannot change their own email today; only
  `AdminUpdateMemberAsync` can, so "a change clears the flag" applies there.
- Read (only these): `OtpPurpose` in `GHCAA.Domain/Enums.cs`; `IOtpService` and `OtpService`;
  `OtpMaxAttempts` in `Constants.cs`; `RegistrationController` `verify-email` and `resend-otp`;
  `MemberService.VerifyEmailAsync`; `Member.EmailVerified`; `AuthService.cs` lines 272 and 285;
  `AdminUpdateMemberAsync`; `ProfileController.cs`; `GHCAA.Tests/Services/OtpPurposeIsolationTests.cs`.
- Change (only these files): `Enums.cs`; `IMemberService` and `MemberService_Profile.cs`;
  `MemberService_Search.cs`; `ProfileController.cs`; `GHCAA.Tests/Services/MemberServiceTests.cs`;
  `GHCAA.Tests/Services/OtpPurposeIsolationTests.cs`; `GHCAA.Tests/Controllers/ProfileControllerTests.cs`;
  `docs/API_CONTRACT_REGISTRY.md`; `docs/TODO.md`.
- Do not touch: the registration verify routes; the OTP lock code.
- Steps:
  1. Append `OtpPurpose.EmailVerify` at the end of the enum. -> check: isolation test covers it.
  2. `POST api/profile/email/send-code` and `POST api/profile/email/verify` for the logged-in member,
     using the OTP service and its attempt lock. -> check: tests.
  3. `AdminUpdateMemberAsync` clears `EmailVerified` when the email changes. -> check: test.
  4. Linking a social login is refused while the email is unverified. -> check: test.
- Contract: two new routes; record them.
- Tests: `MemberServiceTests.cs` :: `VerifyEmail_RightCode_SetsFlag`, `VerifyEmail_Locked_IsRefused`,
  `AdminUpdate_EmailChange_ClearsVerified`, `SocialLink_Unverified_IsRefused`;
  `OtpPurposeIsolationTests.cs` :: the new purpose cannot use a registration code.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~MemberService|FullyQualifiedName~Otp|FullyQualifiedName~ProfileController"`.
- Done when: tests pass; 97.1 records the backend part.
- Stop and ask if: social login linking lives somewhere that does not know the member.

<a id="s64"></a>
**S64 · 97.1 (clients) · M**
- Goal: web and mobile profiles show a verified badge or a Verify action that runs the code flow.
- Before you start: S63 is closed.
- Read (only these): the S63 routes in the registry; web `member/profile/profile.ts`; mobile
  `screens/member/profile_screen.dart` and `profile_edit_screen.dart`; the membership walkthrough
  `README.md` and `membership.walkthrough.ts`.
- Change (only these files): web `profile.service.ts`, `profile.ts`, its template and `profile.spec.ts`;
  `app.constants.ts`; mobile `profile_screen.dart`, its service, `GHCAA.Mobile/test/profile_verify_email_test.dart` (new);
  the walkthrough and its `README.md`; `docs/TODO.md`.
- Do not touch: the API.
- Steps:
  1. Web: badge when verified; otherwise "Verify email", send code, code box, verify, errors from the
     server. -> check: spec.
  2. Mobile: the same with `serverMessage`. -> check: widget test.
  3. Walkthrough: after the first login the badge is absent, the step verifies by code (read from the
     local email log as the walkthrough does for registration), then the badge shows. Update "What
     the run covers". -> check: walkthrough run.
- Contract: none.
- Tests: `profile.spec.ts` :: shows the badge, runs verify; `profile_verify_email_test.dart` :: badge,
  wrong code message.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/member/profile`;
  `cd GHCAA.Mobile; flutter analyze; flutter test test/profile_verify_email_test.dart --exclude-tags golden`; the walkthrough.
- Done when: tests and walkthrough pass; 97.1 DONE.
- Stop and ask if: the walkthrough cannot read the code locally.

<a id="s65"></a>
**S65 · 84.83 (backend) · L · migration 11**
- Goal: members opt in as mentors with topics; mentorship requests go only to opted-in members; the
  directory filters by mentor.
- Before you start: migration 10 of the ledger is applied locally.
- Read (only these): `Member.cs` (visibility flags, lines 41 to 45); `MentorshipController.cs`;
  `IMentorshipService.SendRequestAsync` and `MentorshipService.cs` (line 23); `MemberSearchFilterDto`
  (`INetworkingService.cs`, line 15) and `SearchPublicDirectoryAsync`; `UpdateProfileAsync` in
  `MemberService_Profile.cs`.
- Change (only these files): `Member.cs`; `MemberConfiguration.cs`; `Constants.cs`; a new PgSql migration
  `AddMentorOptIn` with its `.Designer.cs`; `PgSqlApplicationDbContextModelSnapshot.cs`; the profile
  update DTO and `MemberService_Profile.cs`; `MentorshipService.cs`; `INetworkingService.cs` and its
  implementation; `GHCAA.Tests/Services/MentorshipServiceTests.cs`;
  `GHCAA.Tests/Services/NetworkingServiceTests.cs`; `docs/API_CONTRACT_REGISTRY.md`; `docs/TODO.md`;
  ledger row 11.
- Do not touch: `ProfessionalRecord`; mentorship statuses; clients.
- Steps:
  1. `Member.IsMentor` (default false) and `Member.MentorTopics` (nullable, length from `Constants`).
     -> check: build.
  2. Migration per rule 5, guarded, Down drops both columns. -> check: applies twice, rolls back.
  3. Profile update sets both fields. -> check: test.
  4. `SendRequestAsync` refuses a mentor who has not opted in. Opting out leaves pending requests
     as they are. -> check: tests.
  5. `MemberSearchFilterDto.MentorsOnly`; the directory returns only opted-in members when set.
     -> check: test.
- Contract: profile update and directory filter gain fields; record them.
- Tests: `MentorshipServiceTests.cs` :: `SendRequest_ToNonMentor_IsRefused`,
  `OptOut_WithPendingRequest_KeepsIt`; `NetworkingServiceTests.cs` :: `Directory_MentorsOnly_Filters`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~Mentorship|FullyQualifiedName~Networking"`.
- Done when: tests pass; no pending model changes; `code-reviewer` has read the migration; ledger row
  11 has its timestamp.
- Stop and ask if: existing requests point at members who would be refused and the owner has not said
  to keep them.

<a id="s66"></a>
**S66 · 84.83 (clients) · M**
- Goal: members set the mentor opt-in on their profile and filter the directory by mentors, on web and
  mobile.
- Before you start: S65 is closed. Neither client has a settings screen; the opt-in goes on the
  profile edit form.
- Read (only these): S65's registry entries; web `member/profile/profile.ts`, `common/directory/directory.ts`;
  mobile `screens/member/profile_edit_screen.dart`, `screens/member/directory_screen.dart`.
- Change (only these files): web `profile.ts` and template, `profile.spec.ts`, `directory.ts` and
  template, `directory.spec.ts`; mobile `profile_edit_screen.dart`, `directory_screen.dart`,
  `GHCAA.Mobile/test/profile_edit_screen_test.dart`, `GHCAA.Mobile/test/directory_filters_test.dart`;
  `docs/TODO.md`.
- Do not touch: the public directory page; the API.
- Steps:
  1. Profile edit: an "Open to mentoring" switch and a topics field shown when it is on. -> check:
     specs and widget test.
  2. Directory: a "Mentors only" filter on both clients. -> check: specs and widget test.
- Contract: none.
- Tests: `profile.spec.ts`, `directory.spec.ts`, `profile_edit_screen_test.dart`,
  `directory_filters_test.dart` :: one case each for the new control.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/member/profile src/app/common/directory`;
  `cd GHCAA.Mobile; flutter analyze; flutter test test/profile_edit_screen_test.dart test/directory_filters_test.dart --exclude-tags golden`.
- Done when: tests pass; both clients checked locally; 84.83 DONE.
- Stop and ask if: labels need new OrgConfig keys.

<a id="s67"></a>
**S67 · 84.82 (backend) · L · migration 12**
- Goal: a member who attended an event rates it once (1 to 5, optional comment); admins see a summary.
- Before you start: migration 11 of the ledger is applied locally. "Attended" means an `Approved`
  registration; ask the owner whether check-in (`IsCheckedIn`) is required instead.
- Read (only these): `AlumniEvent` and `EventRegistration` models; `EventsController.cs`;
  `IEventService` and `EventService.cs`; the `AlumniEvent` global query filter gotcha (admin reads use
  `IgnoreQueryFilters()`).
- Change (only these files): `GHCAA.Domain/Models/EventFeedback.cs` (new);
  `GHCAA.Infrastructure/Data/Configurations/EventFeedbackConfiguration.cs` (new); `ApplicationDbContext.cs`;
  `Constants.cs`; a new PgSql migration `AddEventFeedback` with its `.Designer.cs`; the snapshot;
  `GHCAA.Application/DTOs/EventFeedbackDtos.cs` (new); `IEventService.cs`; `EventService.cs`;
  `EventsController.cs`; `GHCAA.Tests/Services/EventServiceTests.cs`;
  `GHCAA.Tests/Controllers/EventsControllerTests.cs`; `docs/API_CONTRACT_REGISTRY.md`; `docs/TODO.md`;
  ledger row 12.
- Do not touch: registration statuses; clients.
- Steps:
  1. `EventFeedback` with event, member, rating, comment, created at; unique on event and member;
     comment length from `Constants`. -> check: build.
  2. Migration per rule 5, guarded, Down drops the table. -> check: applies twice, rolls back.
  3. `POST api/events/{id}/feedback` for members: refused before the event starts, for a member with
     no approved registration, and for a repeat. -> check: tests.
  4. `GET api/events/admin/{id}/feedback-summary` (AdminOnly): count, average, rating counts and
     comments. -> check: test.
- Contract: two new routes; record them.
- Tests: `EventServiceTests.cs` :: `Feedback_NonAttendee_IsRefused`, `Feedback_BeforeStart_IsRefused`,
  `Feedback_Repeat_IsRefused`, `FeedbackSummary_Averages`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~Event"`.
- Done when: tests pass; no pending model changes; `code-reviewer` has read the migration; ledger row
  12 has its timestamp.
- Stop and ask if: the owner wants anonymous feedback.

<a id="s68"></a>
**S68 · 84.82 (clients) · M**
- Goal: members rate an event on web and mobile; admins see the summary on web.
- Before you start: S67 is closed.
- Read (only these): S67's registry entries; web `common/events/events.ts` (detail) and
  `admin/events/admin-events.ts`; mobile `screens/member/event_details_screen.dart`; the event
  walkthrough `README.md`.
- Change (only these files): web `core/services/events.service.ts`, `events.service.spec.ts`, `events.ts` and template, `events.spec.ts`,
  `admin-events.ts` and template, `admin-events.spec.ts`; mobile `event_details_screen.dart`, its
  service, `GHCAA.Mobile/test/event_feedback_test.dart` (new); the event walkthrough and `README.md`;
  `docs/TODO.md`.
- Do not touch: the API.
- Steps:
  1. Detail page: a rating form after the event starts for an attendee; a "Thanks" state after
     sending. -> check: spec and widget test.
  2. Admin events: a feedback summary panel per event. -> check: spec.
  3. Walkthrough: the attendee rates the event; add the row to "What the run covers". -> check: run.
- Contract: none.
- Tests: `events.spec.ts` :: form shown only to attendees; `admin-events.spec.ts` :: summary renders;
  `event_feedback_test.dart` :: send and repeat message.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/common/events src/app/admin/events`;
  `cd GHCAA.Mobile; flutter analyze; flutter test test/event_feedback_test.dart --exclude-tags golden`; the walkthrough.
- Done when: tests and walkthrough pass; 84.82 DONE.
- Stop and ask if: the walkthrough's event has not started at the step's time.

<a id="s69"></a>
**S69 · 84.72 (backend) · L · migration 13**
- Goal: admins manage forum categories and moderate topics and posts, with removals audited as in spec
  028.
- Before you start: migration 12 of the ledger is applied locally. `ForumTopic` already has
  `IsPinned`, `IsLocked` and `IsActive`; no model change is needed for pin and lock.
- Read (only these): `ForumCategory`, `ForumTopic`, `ForumPost` models; `ForumController.cs`;
  `IForumService` and `ForumService.cs` (lock check at line 153); spec 028 moderation in
  `DiscussionService.cs` (`RemoveCommentAsync` line 227, `SetLockedAsync` line 255),
  `RemoveCommentDto` and `Constants.Discussion.AuditTypes`.
- Change (only these files): `GHCAA.Application/DTOs/ForumAdminDtos.cs` (new); `IForumService.cs`;
  `ForumService.cs`; `GHCAA.API/Controllers/AdminForumController.cs` (new); `Constants.cs` (forum audit
  types); a new PgSql migration `SeedForumCategories` with its `.Designer.cs`;
  `GHCAA.Infrastructure/Data/ForumCategorySeeder.cs` (new) for databases built without migrations;
  `GHCAA.Tests/Services/ForumServiceTests.cs`; `GHCAA.Tests/Controllers/AuthorizationPolicyReflectionTests.cs`;
  `docs/API_CONTRACT_REGISTRY.md`; `docs/TODO.md`; ledger row 13.
- Do not touch: member forum routes; `Seed/*.json`; clients.
- Steps:
  1. `api/admin/forum` (AdminOnly): categories list, create, edit, reorder, retire (sets `IsActive`
     false); topic pin, lock and unlock; topic and post remove with a required reason. -> check: tests.
  2. Each remove, lock and unlock writes an activity row through `IActivityService.LogActivityAsync`
     with new forum audit types beside `Constants.Discussion.AuditTypes`. -> check: test.
  3. Data migration inserts three default categories only when the table is empty, with
     `ON CONFLICT DO NOTHING` and the sequence advanced; the startup seeder does the same for an empty
     table. -> check: runs twice; a database with categories is unchanged.
- Contract: the new admin routes; record them.
- Tests: `ForumServiceTests.cs` :: `RemovePost_NeedsReason`, `RemovePost_WritesAudit`,
  `RetireCategory_HidesIt`, `ReorderCategories_SavesOrder`, `Lock_BlocksNewPosts`;
  `AuthorizationPolicyReflectionTests.cs` covers the new controller.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~Forum|FullyQualifiedName~AuthorizationPolicyReflection"`.
- Done when: tests pass; `code-reviewer` has read the migration and seeder; ledger row 13 has its
  timestamp.
- Stop and ask if: the owner has not named the default categories (offer three in the question).

<a id="s70"></a>
**S70 · 84.72 (web) · M**
- Goal: an admin forum section on web for categories and moderation.
- Before you start: S69 is closed.
- Read (only these): S69's registry entries; `member/forum/forum.ts` and `topic-detail.ts`;
  `core/services/forum.service.ts`; the polls admin page (model); `app.routes.ts` (the `enableForum`
  `featureGuard`, line 285); `nav.service.ts`.
- Change (only these files): `app.constants.ts`; `forum.service.ts`;
  `GHCAA.Web/src/app/admin/forum/admin-forum.ts` (new), `admin-forum.html` (new), `admin-forum.scss` (new),
  `admin-forum.spec.ts` (new); `topic-detail.ts` and template (admin pin, lock, remove buttons);
  `GHCAA.Web/src/app/member/forum/topic-detail.spec.ts` (new);
  `app.routes.ts`; `nav.service.ts`; `docs/TODO.md`.
- Do not touch: mobile; the API.
- Steps:
  1. Admin methods in the service. -> check: type-check.
  2. Categories page: list, add, edit, drag or up and down reorder, retire. -> check: browser.
  3. Topic detail shows pin, lock and remove (reason required) to admins only. -> check: spec.
  4. Route behind the admin guard and the forum feature flag; nav entry. -> check: nav spec.
- Contract: none.
- Tests: `admin-forum.spec.ts` :: list, create, retire;
  `GHCAA.Web/src/app/member/forum/topic-detail.spec.ts` (new) :: admin buttons hidden for members,
  remove needs a reason.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/admin/forum src/app/member/forum`.
- Done when: tests pass; browser check done; 84.72 records the web part.
- Stop and ask if: the forum flag is off locally and turning it on needs an OrgConfig change.

<a id="s71"></a>
**S71 · 84.72 (mobile) · M**
- Goal: admins moderate the forum on mobile, behind a role check the mobile admin routes lack today.
- Before you start: S69 is closed.
- Read (only these): `core/router/app_router.dart` (admin routes, lines 244 to 258); `rolesInclude` in
  `core/models/auth_models.dart`; `screens/member/forum/*`; `features/forum/forum_service.dart`;
  `docs/specs/017-communication/spec.md`.
- Change (only these files): `forum_service.dart`; the forum topic screen in `screens/member/forum/`;
  `GHCAA.Mobile/lib/screens/admin/admin_forum_screen.dart` (new); `app_router.dart`;
  `GHCAA.Mobile/test/admin_forum_screen_test.dart` (new); `GHCAA.Mobile/test/router_test.dart`;
  spec 017; `docs/TODO.md`.
- Do not touch: other admin screens' routes beyond adding the shared guard to the forum route.
- Steps:
  1. Admin methods in the forum service. -> check: `flutter analyze`.
  2. Admin forum screen: categories list, add, edit, retire. Topic screen shows pin, lock and remove
     with a reason to admins. -> check: widget test.
  3. The admin forum route redirects a non-admin, using `rolesInclude`. -> check: router test.
  4. Spec 017 describes the moderation tools on both clients. -> check: read back.
- Contract: none.
- Tests: `admin_forum_screen_test.dart` :: list, retire, remove needs reason; `router_test.dart` ::
  member is redirected from the admin forum route.
- Run: `cd GHCAA.Mobile; flutter analyze; flutter test test/admin_forum_screen_test.dart test/router_test.dart --exclude-tags golden`.
- Done when: tests pass; 84.72 DONE; the missing guard on the other admin routes stays its own
  tracker line.
- Stop and ask if: the guard needs a change to the shell route that touches other screens.

<a id="s72"></a>
**S72 · 92.1 · S**
- Goal: the four open questions of spec 022 are answered by the owner and written into the spec.
- Before you start: no card dependency.
- Read (only these): `docs/specs/022-configurable-approval-workflows/spec.md` (the "In scope" line and
  "Decisions to confirm before implementation", lines 30 to 39), `plan.md`, `tasks.md`; the 92.1 entry.
- Change (only these files): spec 022 `spec.md`, `plan.md`, `tasks.md`; ledger row 14 (filled or marked
  not needed); `docs/TODO.md`.
- Do not touch: code.
- Steps:
  1. Ask through AskUserQuestion, one question per open point, with these recommended answers: settings
     in an OrgConfig `Approvals` group, not a table; "off" means the item is created approved with an
     audit row; SuperAdmin can always approve; member import stays SuperAdmin and Admin. -> check:
     every point has an answer.
  2. Rename the section to "Decisions taken" with the date and the answers; list the nine flows with
     their keys. -> check: no open question left.
  3. Ledger row 14 says "not needed" if the owner chose OrgConfig. -> check: read back.
- Contract: none.
- Tests: none.
- Run: `python scripts/backlog_inventory.py --check docs/MASTER_EXECUTION_PLAN.md`.
- Done when: spec 022 has its decisions; 92.1 DONE.
- Stop and ask if: the owner wants a flow outside the nine.

<a id="s73"></a>
**S73 · 92.2 · L · migration 14 (only if S72 chose a table)**
- Goal: each approval flow can be off or given to other roles; approve and reject routes check the
  flow's roles instead of `AdminOnly`.
- Before you start: S72 is closed. If it chose a table, migration 13 is applied locally.
- Read (only these): spec 022 decisions; the 92.2 entry; `MemberApprovalMode` in
  `GHCAA.Application/DTOs/OrgConfigDto.cs` (line 146); the approve and reject actions in
  `AdminController.cs` (lines 91 and 135), `EventsController.cs` (239), `GalleryController.cs`
  (238 to 262), `JobHubController.cs` (105, 113), `NewsController.cs` (135, 144);
  `AddAppAuthorization` in `GHCAA.API/Extensions/ServiceExtensions.cs`;
  `GHCAA.Tests/Controllers/AuthorizationPolicyReflectionTests.cs`.
- Change (only these files): `OrgConfigDto.cs` (an `Approvals` group, or the table files if chosen);
  `Constants.cs` (flow keys and defaults); `GHCAA.Application/Interfaces/IApprovalSettings.cs` (new);
  `GHCAA.Infrastructure/Services/ApprovalSettingsService.cs` (new);
  `GHCAA.API/Authorization/ApprovalFlowRequirement.cs` (new) and its handler; `ServiceExtensions.cs`;
  the five controllers above; the create paths of the nine flows' services; a SuperAdmin settings
  route in `RolesController.cs` or a new `AdminApprovalsController.cs` (new);
  `GHCAA.Tests/Services/ApprovalSettingsServiceTests.cs` (new); `AuthorizationPolicyReflectionTests.cs`;
  `docs/API_CONTRACT_REGISTRY.md`; `docs/TODO.md`.
- Do not touch: election nomination approval (it uses `ElectionStaff` and election permissions);
  `PendingApprovalsController.cs` except its counts; clients.
- Steps:
  1. Flow keys and defaults (on, SuperAdmin and Admin) in `Constants`. Missing settings read as the
     defaults, so nothing changes on deploy. -> check: default test.
  2. `MemberApprovalMode` maps to the member registration flow: Auto is off, ManualReview is on.
     Registration reads it. -> check: test.
  3. The requirement handler reads the flow's roles; an unknown key is refused, logged and grants
     nothing. -> check: tests.
  4. Each approve and reject route uses the flow policy. When a flow is off, the create path sets the
     approved state and writes an audit row. -> check: tests per flow.
  5. SuperAdmin route to read and change the settings, each change audited. -> check: test.
- Contract: the settings routes; approve routes' 403 depends on settings. Record both.
- Tests: `ApprovalSettingsServiceTests.cs` :: per flow on, off, allowed role, refused role, defaults;
  `UnknownFlow_IsRefusedAndLogged`; `AuthorizationPolicyReflectionTests.cs` updated for the new policy.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~Approval|FullyQualifiedName~AuthorizationPolicyReflection"`;
  `python scripts/ci-local.py --only backend`.
- Done when: tests pass; `code-reviewer` and `security-reviewer` have read the diff; 92.2 DONE.
- Stop and ask if: a flow's create path has no single place to set the approved state.

<a id="s74"></a>
**S74 · 92.3 · M**
- Goal: SuperAdmin edits approval settings on web, and the pending queue shows only flows the viewer
  may approve.
- Before you start: S73 is closed.
- Read (only these): S73's registry entries; `admin/org-config/org-config.ts` and its template
  (`MemberApprovalMode` at line 172 of the template); the pending approvals page and service on web.
- Change (only these files): `app.constants.ts`; `GHCAA.Web/src/app/admin/approval-settings/approval-settings.ts` (new),
  `approval-settings.html` (new), `approval-settings.spec.ts` (new); the pending approvals component
  and its spec; `app.routes.ts`; `nav.service.ts`; `org-config.html` (remove the old switch, link to
  the new page); `GHCAA.Web/scripts/generate-org-config-fallback.mjs` if the field moves;
  `docs/TODO.md`.
- Do not touch: mobile; the API.
- Steps:
  1. Settings page: one row per flow with on or off and role checkboxes; SuperAdmin only. -> check:
     spec.
  2. Queue hides flows the viewer may not approve. -> check: spec.
  3. The member approval switch moves to the new page. -> check: org-config spec.
- Contract: none.
- Tests: `approval-settings.spec.ts` :: saves a change, hidden from Admin; queue spec :: filters.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/admin/approval-settings src/app/admin/org-config`.
- Done when: tests pass; browser check done; 92.3 DONE.
- Stop and ask if: the fallback generator needs a shape change the API does not send.

<a id="s75"></a>
**S75 · 92.4, 92.5 · S**
- Goal: mobile approve and reject controls follow the flow settings; requirements and docs record the
  feature.
- Before you start: S73 is closed.
- Read (only these): S73's registry entries; mobile `screens/admin/approval_queue_screen.dart`,
  `screens/member/member_details_screen.dart`, `screens/admin/article_approval_screen.dart`,
  `gallery_approval_screen.dart`, `job_approval_screen.dart`,
  `screens/officials/officials_approvals_screen.dart`; spec 002 authorization catalogue;
  `docs/PROJECT_MAP.md`.
- Change (only these files): the six mobile screens above; a settings read in the mobile service the
  queue uses; `GHCAA.Mobile/test/approval_gating_test.dart` (new);
  `docs/specs/002-workflow-contracts-and-validation/evidence/authorization-catalog.md`; spec 022;
  `docs/PROJECT_MAP.md`; `docs/TODO.md`; card H00 in this plan.
- Do not touch: `docs/book/`; `election_form_records_sheet.dart` (nomination approval is not a flow).
- Steps:
  1. Mobile reads the settings and hides approve and reject where the user's role is not allowed.
     -> check: widget test.
  2. FR and NFR rows in spec 022; spec 002 authorization catalogue; `PROJECT_MAP.md` and the
     `business_flow` memory notes. -> check: read back.
  3. Add a line to H00 for the book's role matrix and its figure. -> check: H00 has it.
- Contract: none.
- Tests: `approval_gating_test.dart` :: control hidden for a refused role, shown for an allowed one.
- Run: `cd GHCAA.Mobile; flutter analyze; flutter test test/approval_gating_test.dart --exclude-tags golden`.
- Done when: tests pass; 92.4 and 92.5 DONE with the book part handed to H00.
- Stop and ask if: a screen gets its role from somewhere other than the session.

<a id="s76"></a>
**S76 · 37.7 (web mechanism) · M**
- Goal: the web app has a runtime English and Bengali switch with a translate pipe; admin stays
  English.
- Before you start: no card dependency. No i18n code exists. Do not install `@angular/localize` or
  `ngx-translate` (37.7 entry).
- Read (only these): the 37.7 entry; `common/theme-toggle/theme-toggle.ts`; `common/user-menu/user-menu.html`;
  `layouts/public-layout/public-layout.html` (line 59); `src/index.html` (font link, line 47);
  `src/styles.scss`.
- Change (only these files): `GHCAA.Web/src/app/core/services/i18n.service.ts` (new),
  `i18n.service.spec.ts` (new); `GHCAA.Web/src/app/core/i18n/en.ts` (new), `bn.ts` (new);
  `GHCAA.Web/src/app/core/pipes/translate.pipe.ts` (new), `translate.pipe.spec.ts` (new);
  `GHCAA.Web/src/app/common/language-toggle/language-toggle.ts` (new); `user-menu.html` and
  `public-layout.html`; `styles.scss`;
  `GHCAA.Web/public/assets/fonts/` (new), holding one Bengali font file and its licence; `docs/TODO.md`.
- Do not touch: admin templates; mobile.
- Steps:
  1. Service with a `signal<'en' | 'bn'>` saved in `localStorage` (inside try and catch), setting
     `<html lang>`. -> check: spec.
  2. `en.ts` is the key source; `bn.ts` is a partial record. The pure `t` pipe returns Bengali, then
     English, then the key. -> check: spec.
  3. A language toggle beside the theme toggle in the user menu and the public layout. -> check:
     browser.
  4. Self-host a Bengali font with an open licence; apply it under `:lang(bn)`. -> check: an offline
     `ng build` succeeds.
- Contract: none.
- Tests: `translate.pipe.spec.ts` :: Bengali, English fallback, key fallback, every `en` key resolves;
  `i18n.service.spec.ts` :: choice survives a new instance.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/core/i18n src/app/core/pipes src/app/core/services/i18n.service.spec.ts`.
- Done when: tests pass; the toggle works in the browser; 37.7 records the mechanism.
- Stop and ask if: no Bengali font with an open licence fits under the page weight limit.

<a id="s77"></a>
**S77 · 37.7 (web strings) · M**
- Goal: public site and member portal nav, buttons, labels and validation messages go through the
  pipe, with draft Bengali strings.
- Before you start: S76 is closed.
- Read (only these): `en.ts`, `bn.ts`; the public and portal layout templates; the shared form error
  component.
- Change (only these files): `en.ts`, `bn.ts`; public and portal templates under `GHCAA.Web/src/app/public/`,
  `GHCAA.Web/src/app/member/` and `GHCAA.Web/src/app/layouts/`; `docs/TODO.md` (list of draft strings).
- Do not touch: admin; server content (constitution, news, election documents); OrgConfig labels.
- Steps:
  1. Replace literals with keys, area by area: layouts, public pages, portal pages, validation
     messages. -> check: the drift test after each area.
  2. Write Bengali drafts; list them in the 37.7 entry as unreviewed. -> check: entry updated.
- Contract: none.
- Tests: the drift test in `translate.pipe.spec.ts`; existing specs of changed components still pass.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit`.
- Done when: tests pass; both languages checked in the browser; 37.7 stays PARTIAL until gate G-BN.
- Stop and ask if: a label comes from OrgConfig (it stays as it is).

<a id="s78"></a>
**S78 · 8.8 · M**
- Goal: mobile uses the same keys as web in its flat dictionary.
- Before you start: S77 is closed.
- Read (only these): `GHCAA.Mobile/lib/core/services/app_localizations.dart`; `en.ts` and `bn.ts`;
  the 8.8 entry.
- Change (only these files): `app_localizations.dart`; the member-facing screens whose strings move;
  `GHCAA.Mobile/test/app_localizations_test.dart` (new); `docs/TODO.md`.
- Do not touch: admin screens; no ARB files.
- Steps:
  1. Rename the 29 existing keys to the web keys where they mean the same thing, and add the rest the
     member screens need. -> check: test that `en` and `bn` hold the same keys.
  2. Replace literals in member screens. -> check: `flutter analyze`; widget tests pass.
- Contract: none.
- Tests: `app_localizations_test.dart` :: same keys in both maps; missing Bengali falls back to
  English; unknown key returns the key.
- Run: `cd GHCAA.Mobile; flutter analyze; flutter test --exclude-tags golden`.
- Done when: tests pass; 8.8 stays PARTIAL until G-BN.
- Stop and ask if: golden tests change; ask before updating them.

<a id="s79"></a>
**S79 · 62.2 · M**
- Goal: both profiles have the four files `ProfilePackFiles` reads, so a profile no longer falls back
  to the shared seed by accident.
- Before you start: no card dependency. The two other files the 62.2 entry asks for (membership
  tiers and governance) have no reader, so they are dropped from 62.2, not written.
- Read (only these): `ProfilePackFiles`, `ResolveProfilePackPath` and `LoadSeed<T>` in
  `ApplicationDbContext.cs` (lines 95 to 132); `GHCAA.Infrastructure/Data/Seed/email_templates.json`,
  `themes.json`, `site_content.json`; `profiles/default/` and `profiles/ghc/`;
  `GHCAA.Tests/Services/InstitutionProfileProviderTests.cs`; `GHCAA.Tests/Data/SeedDataIntegrityTests.cs`.
- Change (only these files): `profiles/default/email_templates.json` (new), `profiles/default/themes.json` (new),
  `profiles/default/site_content.json` (new); `profiles/ghc/email_templates.json` (new),
  `profiles/ghc/themes.json` (new), `profiles/ghc/site_content.json` (new),
  `profiles/ghc/constitution.json` (new); `GHCAA.Tests/Data/ProfilePackFilesTests.cs` (new);
  `docs/TODO.md`.
- Do not touch: `GHCAA.Infrastructure/Data/Seed/*.json`; `ApplicationDbContext.cs`.
- Steps:
  1. The default profile files are byte copies of the shared seed files. -> check: test compares them.
  2. GHC files: the owner supplies or approves any GHC-specific text; until then they are copies of
     the default. -> check: owner answer recorded.
  3. A test loads each file through `LoadSeed<T>` for both profiles and checks it parses into the
     model. -> check: passes.
- Contract: none.
- Tests: `ProfilePackFilesTests.cs` :: `EveryProfile_HasEachPackFile`, `PackFile_ParsesIntoItsModel`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ProfilePack|FullyQualifiedName~InstitutionProfileProvider"`.
- Done when: tests pass; 62.2 DONE with the two dropped files named.
- Stop and ask if: a copy would put GHC text in the default profile (white-label rule, WP62).

<a id="s80"></a>
**S80 · 51.4 · S**
- Goal: storage tests prove that excluded types are never compressed and that a non-image tagged
  with a compressible type is saved as sent.
- Before you start: no card dependency.
- Read (only these): `GHCAA.Tests/Services/LocalFileStorageServiceTests.cs`; `IsCompressibleImageType`
  in `GHCAA.Infrastructure/Services/FileStorageServiceBase.cs` (lines 56 to 61); `FileUploadType`.
- Change (only these files): `LocalFileStorageServiceTests.cs`; `docs/TODO.md`.
- Do not touch: storage code, unless a test fails; then stop and ask.
- Steps:
  1. A `[TestCase]` per excluded type (`PaymentProof`, `Certificate`, `Signature`, `NoticeDocument`)
     saving a large JPEG with compression on: the stored bytes equal the input. -> check: passes.
  2. A `Photo` and a `NewsImage` with non-image bytes and compression on: saved unchanged, no
     exception. -> check: passes.
- Contract: none.
- Tests: `LocalFileStorageServiceTests.cs` :: `SaveFileAsync_ExcludedType_IsNeverCompressed`,
  `SaveFileAsync_CompressibleTypeThatIsNotAnImage_SavesTheOriginal`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~LocalFileStorageServiceTests"`.
- Done when: tests pass; 51.4 DONE.
- Stop and ask if: a test fails.

<a id="s81"></a>
**S81 · 27.1, 27.8 · M**
- Goal: one script reports low-coverage files for backend, web and mobile, and `ci-local.py` runs the
  gate the owner chose.
- Before you start: ask the owner (AskUserQuestion): enforce 80 % per file now (fails today), or a
  ratchet where no file drops below its recorded figure and new files need 80 % (recommended).
- Read (only these): the 27.1 and 27.8 entries; `scripts/ci-local.py`; the coverage steps in
  `.github/workflows/ghcaa-ci-standard.yml`; `GHCAA.Web/vitest.config.ts` (it already writes a
  `json-summary` report); `docs/TEST_COVERAGE_PLAN.md`.
- Change (only these files): `scripts/coverage_report.py` (new); `scripts/test_coverage_report.py` (new);
  `scripts/ci-coverage-baseline.json` (new, ratchet only; a name starting "coverage" is gitignored);
  `scripts/ci-local.py`; `docs/TEST_COVERAGE_PLAN.md`; `docs/TODO.md`.
- Do not touch: the CI workflow (the owner owns it); test thresholds in `vitest.config.ts`.
- Steps:
  1. The script parses the backend Cobertura XML, the web `json-summary` file and the Flutter
     `lcov.info`, and prints the files under 80 % per layer. -> check: unit tests
     on small fixtures.
  2. Ratchet: `--update-baseline` writes the per-file figures; `--check` fails on a drop or a new file
     under 80 %. Fixed: `--check` fails on any file under 80 %. -> check: unit tests.
  3. `ci-local.py --coverage` collects coverage for the chosen layers and runs the check. -> check: one
     local run per layer.
- Contract: none.
- Tests: `scripts/test_coverage_report.py` :: parses each format, flags a drop, flags a new low file,
  passes an unchanged baseline.
- Run: `python -m unittest discover -s scripts -p test_coverage_report.py`;
  `python scripts/ci-local.py --only backend --coverage`.
- Done when: tests pass; the baseline is recorded with the date; 27.1 DONE; 27.8 DONE or PARTIAL
  naming the files still under the line.
- Stop and ask if: coverage collection needs a new package.

<a id="s82"></a>
**S82 · 73.5 · M**
- Goal: `traceability.py` builds the whole requirements matrix from the FR, NFR and DC tags in C#,
  TypeScript and Dart tests.
- Before you start: the script is in `docs/book/build/`, so rule 13 (the book rule) applies. Ask the
  owner whether this card may change the build script outside track H; otherwise it moves to track H.
  The chapter text stays in H00 either way.
- Read (only these): `docs/book/build/traceability.py`; the requirements table in
  `docs/book/03-requirements.md` (read only); the tag forms: C# `[Category("FR-39")]`, TS `// FR-39:` comments, Dart test names
  starting `'FR-34: '`.
- Change (only these files): `traceability.py`; `docs/book/build/test_traceability.py` (new);
  `docs/TODO.md`; card H00 in this plan.
- Do not touch: chapter files and the outline.
- Steps:
  1. Scan `GHCAA.Web/src` spec files and `GHCAA.Mobile/test` beside `GHCAA.Tests`. -> check: a known
     tag from each layer is found.
  2. `--markdown` prints the full matrix (requirement, tests per layer, count). `--check` keeps its
     meaning. -> check: unit tests.
  3. Add a line to H00: the book can restore the §3.9 claim (the requirements traceability section)
     from this output. -> check: H00 has it.
- Contract: none.
- Tests: `test_traceability.py` :: each tag form parsed; a requirement with no tests is listed.
- Run: `python -m unittest discover -s docs/book/build -p test_traceability.py`;
  `python docs/book/build/traceability.py --markdown </dev/null`.
- Done when: tests pass; 73.5 DONE.
- Stop and ask if: the owner keeps the script frozen until track H.

<a id="s83"></a>
**S83 · 73.6 · S**
- Goal: a test fails when the seeded constitution text or version changes without the test changing
  too.
- Before you start: no card dependency.
- Read (only these): `GHCAA.Infrastructure/Data/ConstitutionSeeder.cs`;
  `GHCAA.Infrastructure/Data/Seed/constitution.json` (read only); `LoadSeed<T>` in
  `ApplicationDbContext.cs`; `GHCAA.Tests/Data/SeedDataIntegrityTests.cs`.
- Change (only these files): `GHCAA.Tests/Data/ConstitutionPinTests.cs` (new); `docs/TODO.md`.
- Do not touch: the seed file and the seeder.
- Steps:
  1. Load the active row from the seed as the seeder does. -> check: version read.
  2. Pin the version string and the SHA-256 of `Content` with line endings normalised to LF.
     -> check: passes; changing one character fails it.
  3. The failure message says to update the pin only after the text is ratified. -> check: read it.
- Contract: none.
- Tests: `ConstitutionPinTests.cs` :: `SeededConstitution_MatchesPinnedVersionAndHash`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ConstitutionPin"`.
- Done when: the test passes; 73.6 DONE.
- Stop and ask if: the seed holds more than one active version.

### Phase G. P3 and P4 items

Names in these cards were checked against the code on 2026-11-20. What the older cards assumed that
the code does not have, or already has:

- `Constants.Uploads` does not exist. The 19 `* 1024 * 1024` sites are all `ValidateFormFile`
  arguments, and the tracker's line numbers for `AdminController` and `NewsController` have moved.
- Every rate-limit policy is keyed by IP; there is no per-user partition, and
  `Constants.RateLimitPolicies.Api` is not used anywhere. The limiter fixes a partition's options
  when the partition is created, so the assistant limit is a `Constants.Defaults` value, not an
  OrgConfig key.
- Mobile has no site content code and no HTML rendering package. Mobile's credential download
  (`member_details_screen.dart` `_downloadCredential`) opens the PDF route without the auth header.
- `MemberSummaryDto.AppliedDate` is already sent; neither client shows it. The 30-day rule (DC-08)
  is only in the docs. Dues have no waived state and no part payment.
- No web or mobile screen submits archive items, so the S92 refusal is API-only for now.
- File storage settings are bound through `FileStorageOptions`; the `Constants.ConfigKeys`
  FileStorage keys are not read.
- Nothing for saved reports, cohorts, reunions or memorial entries exists yet. `ProfilePackFiles`
  does not exist yet; S79 creates it.
- Spec 025 tasks T033 to T036 (62.56 part g) are already checked, so S104 covers part a only.
- `api/messaging` and `api/chat` are two prefixes on the same `MessagingController`.

<a id="s84"></a>
**S84 · 84.57, 84.62 · S**
- Goal: every upload size limit is a named constant, and the AI assistant has a per-user limit that
  returns 429 past 20 asks an hour.
- Before you start: no card dependency.
- Read (only these): `GHCAA.Domain/Constants.cs` (`ConfigKeys`, `RateLimitPolicies`, `Defaults`,
  `ErrorCodes`); `GHCAA.API/Extensions/RateLimitingExtensions.cs`;
  `GHCAA.API/Controllers/AssistantController.cs`; `GHCAA.Tests/Integration/ErrorResponseShapeTests.cs`
  (the 429 test and how it builds its client).
- Change (only these files): `GHCAA.Domain/Constants.cs`; the eight controllers with a
  `* 1024 * 1024` argument (`AdminController`, `EventsController`, `FinancialsController`,
  `GalleryController`, `MemberImportController`, `NewsController`, `ProfileController`,
  `RegistrationController`, all in `GHCAA.API/Controllers/`); `RateLimitingExtensions.cs`;
  `AssistantController.cs`; `GHCAA.Tests/Integration/AssistantRateLimitTests.cs` (new);
  `GHCAA.Tests/Services/FileValidationServiceTests.cs`; `docs/TODO.md`.
- Do not touch: `Constants.Defaults.MaxFileSizeBytes` (the storage limit); the other limiter
  policies; `Constants.RateLimitPolicies.Api` (report it as dead, do not delete it).
- Steps:
  1. Add `Constants.Uploads` with `ImageMaxBytes` (5 MB), `SignatureMaxBytes` (2 MB) and
     `DocumentMaxBytes` (10 MB) as `long` values. -> check: build passes.
  2. Replace each of the 19 `N * 1024 * 1024` arguments with the matching constant, and the
     `[RequestSizeLimit(10_485_760)]` on `RegistrationController` with `DocumentMaxBytes` if the
     attribute takes a constant. -> check: `grep -rn "1024 \* 1024" GHCAA.API/Controllers` prints
     nothing.
  3. Add `Constants.RateLimitPolicies.Assistant` and `Constants.Defaults.AssistantAsksPerHour = 20`.
     Register a fixed-window policy partitioned by the `ClaimTypes.NameIdentifier` claim, falling
     back to the IP when the claim is missing, using the same Development key handling as the
     other policies. -> check: the limiter still builds in Development and Production.
  4. Put `[EnableRateLimiting(Constants.RateLimitPolicies.Assistant)]` on `Ask`. -> check: the 21st
     ask by one user in an hour gets 429 with `Constants.ErrorCodes.RateLimited`; a second user is
     not blocked.
  5. Confirm the global limiter (100 a minute per IP) still applies to `Ask`. -> check: a test or a
     read of the chained limiter shows both run.
- Contract: no route change. `POST api/assistant/ask` can now return 429; add that to its row in
  `docs/API_CONTRACT_REGISTRY.md`.
- Tests: `GHCAA.Tests/Integration/AssistantRateLimitTests.cs` ::
  `Ask_AfterUserLimit_Returns429WithRateLimitedCode`, `Ask_OtherUser_NotBlockedByFirstUsersLimit`;
  `GHCAA.Tests/Services/FileValidationServiceTests.cs` :: `ValidateFormFile_AtUploadConstant_Passes`,
  `ValidateFormFile_OneByteOverConstant_Fails`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~AssistantRateLimit|FullyQualifiedName~FileValidationService"`;
  `python scripts/ci-local.py --only contract`.
- Done when: no literal size math is left in controllers; the per-user 429 test passes; 84.57 and
  84.62 close with the test names as evidence.
- Stop and ask if: the owner wants the assistant limit changeable at run time (that needs a custom
  partitioned limiter, which is a bigger card); the Development test key makes per-user testing
  impossible without changing the shared limiter setup.

<a id="s85"></a>
**S85 · 84.64, 93.4 · S**
- Goal: mobile can mark every notification read in one tap, and the drawer reaches the three
  routed screens it is missing, each hidden when its feature is off.
- Before you start: ask the owner 93.4's question with this default: add `/legacy`,
  `/communications` and `/professionals` to the drawer, and gate `/legacy` on
  `enableLegacyArchive`. The other two have no flag and stay always shown.
- Read (only these): `GHCAA.Mobile/lib/screens/member/notification_screen.dart`;
  `GHCAA.Mobile/lib/features/notifications/notification_service.dart`;
  `GHCAA.Mobile/lib/core/widgets/app_drawer.dart`; `GHCAA.Mobile/lib/core/router/app_router.dart`
  (the three routes); `FeatureToggles` in `GHCAA.Mobile/lib/core/config/org_config.dart`;
  `orgConfigProvider` in `org_config_service.dart`.
- Change (only these files): `notification_service.dart`; `notification_screen.dart`;
  `app_drawer.dart`; `GHCAA.Mobile/test/notification_screen_test.dart` (new);
  `GHCAA.Mobile/test/app_drawer_test.dart` (new); `docs/TODO.md`.
- Do not touch: the backend (`POST api/notifications/read-all` exists); the web client; the router.
- Steps:
  1. Add `markAllAsRead()` to `notification_service.dart`, posting to `'/notifications/read-all'`
     the same way `markAsRead` posts. -> check: `flutter analyze` is clean.
  2. Add a "Mark all read" action to the screen's app bar, shown only when an unread item exists;
     on success invalidate `notificationsListProvider`. -> check: the widget test sees no unread
     marks after the tap.
  3. Make `AppDrawer` watch `orgConfigProvider` and add the three entries, hiding `/legacy` when
     `enableLegacyArchive` is false. While config loads, show entries without a flag only.
     -> check: the drawer test passes with the flag on and off.
  4. Refresh the drawer golden in `comprehensive_visual_freeze_test.dart` only if it fails because
     of the new entries. -> check: the diff shows only the new rows.
- Contract: none (the route exists and web already calls it).
- Tests: `GHCAA.Mobile/test/notification_screen_test.dart` ::
  `mark all read clears unread items`, `mark all read hidden when nothing unread`;
  `GHCAA.Mobile/test/app_drawer_test.dart` :: `legacy entry hidden when flag off`,
  `communications and professionals always shown`. Wrap each in `ProviderScope` and use a phone
  viewport.
- Run: `cd GHCAA.Mobile; flutter analyze; flutter test test/notification_screen_test.dart test/app_drawer_test.dart --exclude-tags golden`.
- Done when: both tests pass; 84.64 and 93.4 close with the owner's answer recorded on 93.4.
- Stop and ask if: the owner wants a screen removed instead of added; the drawer has no place for a
  provider read without turning other drawer tests red.

<a id="s86"></a>
**S86 · 84.69 · M**
- Goal: mobile shows the site content pages (about, policy, contact groups) from the public route.
- Before you start: no card dependency. S130 in phase I builds on this card.
- Read (only these): `GHCAA.API/Controllers/SiteContentController.cs` (`GetByGroup`);
  `SiteContentDto`; the web readers `about.ts`, `policy-page.ts` (`POLICY_PAGES`) and `contact.ts`;
  `GHCAA.Mobile/lib/screens/member/about_screen.dart`; `GHCAA.Mobile/pubspec.yaml`.
- Change (only these files): `GHCAA.Mobile/lib/models/site_content.dart` (new);
  `GHCAA.Mobile/lib/features/site_content/site_content_service.dart` (new);
  `GHCAA.Mobile/lib/screens/member/site_content_screen.dart` (new); `about_screen.dart`;
  `GHCAA.Mobile/lib/core/router/app_router.dart`; `GHCAA.Mobile/lib/core/constants/app_constants.dart`;
  `GHCAA.Mobile/test/site_content_screen_test.dart` (new); `docs/TODO.md`.
- Do not touch: the API; the web pages; `pubspec.yaml` unless the owner approves a package.
- Steps:
  1. Add the model with the DTO's fields and a service method `getByGroup(String group)` calling
     `'/site-content?group=$group'`. Put the group names in `app_constants.dart`, matching the web
     values. -> check: a unit test parses a sample response.
  2. Render `BodyHtml` as plain text: strip tags, turn `<p>` and `<br>` into line breaks and
     `<li>` into bullets. -> check: a test shows a paragraph and a list come out readable.
  3. Build a list screen (titles in `DisplayOrder`, active items only) and a detail view, with
     loading, empty and error states. -> check: the widget test covers all three states.
  4. Route to it from `about_screen.dart` and add a route for each group. -> check: tapping an
     item opens its detail on a phone viewport.
- Contract: none (reads an existing public route). Add the mobile caller to the route's row in
  `docs/API_CONTRACT_REGISTRY.md`.
- Tests: `GHCAA.Mobile/test/site_content_screen_test.dart` :: `lists active items in display order`,
  `shows empty state`, `shows error state`, `strips html to readable text`.
- Run: `cd GHCAA.Mobile; flutter analyze; flutter test test/site_content_screen_test.dart --exclude-tags golden`.
- Done when: the tests pass and a phone-viewport run shows each group; 84.69 closes.
- Stop and ask if: plain text loses content the owner cares about (tables, links, images); that
  needs an HTML package, which the owner must approve.

<a id="s87"></a>
**S87 · 84.23 · M**
- Goal: mobile uploads a signature and downloads the server ID card and certificate PDFs, and stops
  building its own PDF.
- Before you start: ask the owner with these defaults: yes to signature upload; yes to using the
  server PDFs and removing the mobile PDF builder (the server PDF already renders Bengali names).
- Read (only these): `ProfileController` signature and PDF actions; `GHCAA.Mobile/lib/screens/member/digital_id_screen.dart`
  (`_handlePrint`); `member_details_screen.dart` (`_downloadCredential`);
  `GHCAA.Mobile/lib/features/files/file_service.dart` (`uploadProfilePhoto`, `UploadFileNaming`).
- Change (only these files): `file_service.dart`; `digital_id_screen.dart`;
  `member_details_screen.dart`; the profile edit screen that hosts the photo upload;
  `GHCAA.Mobile/test/file_service_test.dart` (new);
  `GHCAA.Mobile/test/digital_id_screen_test.dart` (new); `docs/TODO.md`.
- Do not touch: the API; the web client; `pubspec.yaml` (`pdf` and `printing` stay if anything
  else imports them; check with grep).
- Steps:
  1. Add `uploadSignature(File)` beside `uploadProfilePhoto`, posting to `'/profile/signature'` with
     jpeg, png or webp and a 2 MB check before upload. -> check: a test rejects a 3 MB file
     without a network call.
  2. Add `downloadCredentialPdf(String path)` that fetches bytes through the authenticated Dio
     client. -> check: a test sees the auth header on the request.
  3. Make `_handlePrint` and `_downloadCredential` use it and pass the bytes to `Printing`. Remove
     the local PDF building code. -> check: grep finds no `pw.Document` left in the two screens.
  4. Add the signature picker to the profile edit screen. -> check: phone-viewport run uploads and
     shows the new signature on the ID card.
- Contract: none (routes exist). Add the mobile callers in `docs/API_CONTRACT_REGISTRY.md`.
- Tests: `GHCAA.Mobile/test/file_service_test.dart` :: `signature over 2 MB rejected before upload`,
  `credential download sends auth header`; `GHCAA.Mobile/test/digital_id_screen_test.dart` ::
  `print uses server pdf bytes`.
- Run: `cd GHCAA.Mobile; flutter analyze; flutter test test/file_service_test.dart test/digital_id_screen_test.dart --exclude-tags golden`.
- Done when: the tests pass; the admin download on `member_details_screen.dart` works on a real run;
  84.23 closes with the owner's answer.
- Stop and ask if: the owner says no to either part; the server PDF fails for a member the mobile
  builder handled.

<a id="s88"></a>
**S88 · 96.3 · M**
- Goal: the messages page, directory search and campaign form use the shared components.
- Before you start: no card dependency.
- Read (only these): `GHCAA.Web/src/app/common/page-header/page-header.component.ts`;
  the shared `app-search-bar` and `app-date-input` components in `common/`;
  `GHCAA.Web/src/app/admin/ledger/ledger.html` (the model page);
  `GHCAA.Web/src/app/member/messages/messages.html`;
  `GHCAA.Web/src/app/common/directory/directory.html`;
  `GHCAA.Web/src/app/admin/campaigns/admin-campaigns.html`.
- Change (only these files): `messages.html`, `messages.ts`, `messages.scss`; `directory.html`,
  `directory.ts`; `admin-campaigns.html`, `admin-campaigns.ts`;
  `GHCAA.Web/src/app/member/messages/messages.spec.ts` (new);
  `GHCAA.Web/src/app/admin/campaigns/admin-campaigns.spec.ts` (new);
  `GHCAA.Web/src/app/common/directory/directory.spec.ts`; `docs/TODO.md`.
- Do not touch: the shared components themselves; theme files. There is no `app-input`, so plain
  inputs keep the shared form classes.
- Steps:
  1. Messages: add `app-page-header`; style the composer with the shared form classes the ledger
     page uses. -> check: the spec finds the header and the composer.
  2. Directory: replace the raw search input with `app-search-bar`, keeping the same debounce and
     query behaviour. -> check: the directory spec still filters on typed text.
  3. Campaigns: the two date fields become `app-date-input`; the other fields take the shared form
     classes. -> check: the spec shows a picked date saves as before and displays `dd-MM-yyyy`.
  4. Browser check each page in light and dark mode. -> check: no hardcoded colour was added.
- Contract: none.
- Tests: `messages.spec.ts` :: `renders page header`, `sends from composer`;
  `directory.spec.ts` :: `filters through shared search bar`; `admin-campaigns.spec.ts` ::
  `dates use app-date-input`, `saves campaign with picked dates`.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/member/messages src/app/common/directory src/app/admin/campaigns`.
- Done when: the specs pass and the browser check is done; 96.3 closes.
- Stop and ask if: a shared component lacks an input these pages need (change the shared component
  in its own card instead).

<a id="s89"></a>
**S89 · 84.90 · M**
- Goal: generating an ID card or certificate reuses the member's newest live code for that type
  instead of minting a new one each time.
- Before you start: ask the owner with this default: reuse the newest unrevoked, unexpired code per
  type; mint only when none exists. The revoke and reissue work closed 2026-11-07 stays as is.
- Read (only these): `GHCAA.Infrastructure/Services/IDCardService.cs` (`IssueCredentialAsync`,
  `NewCredentialAsync`, `ReissueCredentialAsync`, `VerifyCredentialAsync`); `IssuedCredential`;
  spec 013 FR-010.
- Change (only these files): `IDCardService.cs`;
  `GHCAA.Tests/Services/IDCardServiceCredentialTests.cs`;
  `docs/specs/013-*/spec.md` (FR-010 text); `docs/TODO.md`.
- Do not touch: `ReissueCredentialAsync` and revoke; `CredentialCodeGenerator`; the verify controller.
- Steps:
  1. In `IssueCredentialAsync`, look up the newest row for the member and type that is not revoked
     and not expired; return it when found. -> check: two calls return the same short code.
  2. Keep minting through `NewCredentialAsync` when no live row exists, and after a revoke or
     expiry. -> check: a revoked code leads to a new code on the next call.
  3. All four generators go through the changed method. -> check: grep shows no direct
     `NewCredentialAsync` call outside it and the reissue path.
  4. Update FR-010 to say codes are reused. -> check: the spec text matches the code.
- Contract: none (response shape unchanged).
- Tests: `GHCAA.Tests/Services/IDCardServiceCredentialTests.cs` ::
  `Issue_TwiceSameType_ReusesLiveCode`, `Issue_AfterRevoke_MintsNewCode`,
  `Issue_AfterExpiry_MintsNewCode`, `Issue_DifferentType_GetsOwnCode`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~IDCardService|FullyQualifiedName~CredentialVerification"`.
- Done when: tests pass; a code-reviewer pass is clean; 84.90 closes with the owner's answer.
- Stop and ask if: the owner chooses to keep minting; two concurrent first calls can mint two codes
  (decide between a unique index and accepting it).

<a id="s90"></a>
**S90 · 84.95 · S**
- Goal: the approval queue on both clients shows the applied date and days waiting, and marks an
  application past the review limit.
- Before you start: runs after the 78.9 sessions (gate G-EVAL) unless the owner says first.
- Read (only these): `MemberSummaryDto` (`AppliedDate`); `WorkflowDto` in `OrgConfigDto.cs`
  (`NewsImageUploadsPerDay` as the example key);
  `GHCAA.Web/src/app/admin/member-approval/member-approval.ts` and `.html`;
  `GHCAA.Mobile/lib/screens/admin/approval_queue_screen.dart`; `OrgWorkflow` in
  `GHCAA.Mobile/lib/core/config/org_config.dart`;
  `GHCAA.Web/tests/e2e/membership-walkthrough/README.md` and `membership.walkthrough.ts`.
- Change (only these files): `OrgConfigDto.cs` (`ApprovalReviewDays`, default 30);
  `GHCAA.Domain/Constants.cs` (the default); the OrgConfig defaults and golden
  `GHCAA.Tests/OrgConfig/golden/org-config-ghc.golden.json`; `member-approval.ts`, `.html`,
  `member-approval.spec.ts`; `org_config.dart`; `approval_queue_screen.dart`;
  `GHCAA.Mobile/test/approval_queue_screen_test.dart` (new); the walkthrough README and
  `membership.walkthrough.ts`; `docs/CONFIG_DRIVEN_FRAMEWORK.md`; `docs/TODO.md`.
- Do not touch: the approval actions; the members search query.
- Steps:
  1. Add `ApprovalReviewDays` to the workflow settings with default 30. -> check: the golden
     snapshot test passes after the golden update.
  2. Web: show the applied date (`dd-MM-yyyy`) and days waiting; mark rows where days waiting is
     more than the setting. -> check: the spec marks day 31 and not day 30.
  3. Mobile: the same on the queue screen, reading the setting from `orgConfigProvider`. -> check:
     the widget test marks day 31 and not day 30.
  4. Walkthrough: check the applied date and days waiting in the queue, and add the row to the
     README table. -> check: the walkthrough step passes locally.
- Contract: OrgConfig gains a field; record it in `docs/API_CONTRACT_REGISTRY.md` and run the
  contract check.
- Tests: `member-approval.spec.ts` :: `shows applied date and days waiting`,
  `marks day 31 not day 30`; `GHCAA.Mobile/test/approval_queue_screen_test.dart` ::
  `marks day 31 not day 30`; `OrgConfigGoldenSnapshotTests`.
- Run: `cd GHCAA.Web; npm run test:unit -- src/app/admin/member-approval`;
  `cd GHCAA.Mobile; flutter test test/approval_queue_screen_test.dart --exclude-tags golden`;
  `python scripts/ci-local.py --only contract`.
- Done when: both clients mark past-limit rows; walkthrough updated; 84.95 closes.
- Stop and ask if: the owner wants the limit fixed rather than a setting.

<a id="s91"></a>
**S91 · 84.94 · M**
- Goal: a SuperAdmin arrears report listing unpaid dues per member, with CSV export.
- Before you start: same timing rule as S90. Ask the owner with this default: waived dues and part
  payments are not modelled today, so leave them out and add a tracker line if wanted.
- Read (only these): `GHCAA.API/Controllers/FinancialLedgerController.cs` (`export/csv`);
  `IFinancialLedgerService.cs`; `FinancialLedgerService.cs`; `MembershipDue`;
  `GHCAA.Web/src/app/admin/ledger/ledger.html` and `ledger.service.ts`.
- Change (only these files): `IFinancialLedgerService.cs`; `FinancialLedgerService.cs`;
  `FinancialLedgerController.cs`; an `ArrearsRowDto` in the Application DTOs folder (new);
  `ledger.html`, `ledger.ts`, `ledger.service.ts`; `GHCAA.Web/src/app/core/constants/app.constants.ts`;
  `GHCAA.Tests/Services/FinancialLedgerServiceTests.cs`;
  `GHCAA.Tests/Controllers/FinancialLedgerControllerTests.cs`; `ledger.spec.ts`,
  `ledger.service.spec.ts`; `docs/API_CONTRACT_REGISTRY.md`; `docs/TODO.md`.
- Do not touch: `MembershipDue`; the existing ledger export.
- Steps:
  1. `GetArrearsAsync(DateTime asOf)` returns members with unpaid dues past `DueDate`: member,
     count, total and oldest due date. -> check: the service test with no dues returns an empty list.
  2. `GET api/ledger/arrears` and `GET api/ledger/arrears/csv`, SuperAdminOnly like the rest of the
     controller. -> check: a non-SuperAdmin gets 403.
  3. Web: an arrears tab on the ledger page with the CSV button through the shared export
     component. -> check: the spec downloads a file named from a constant.
  4. Run `python scripts/authz_catalog.py --write`. -> check: the two routes appear.
- Contract: two new routes; record them and run the contract check.
- Tests: `FinancialLedgerServiceTests.cs` :: `Arrears_NoDues_ReturnsEmpty`,
  `Arrears_PaidDue_Excluded`, `Arrears_NotYetDue_Excluded`, `Arrears_TwoUnpaid_SummedPerMember`;
  `FinancialLedgerControllerTests.cs` :: `ArrearsCsv_ReturnsCsvFile`; `ledger.spec.ts` ::
  `shows arrears tab`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~FinancialLedger"`;
  `cd GHCAA.Web; npm run test:unit -- src/app/admin/ledger`; `python scripts/ci-local.py --only contract`.
- Done when: tests pass; code-reviewer pass clean; 84.94 closes, with the waived and part-payment
  answer recorded.
- Stop and ask if: the owner wants waived or part-paid dues (that needs a migration and its own card).

<a id="s92"></a>
**S92 · 84.66 · M**
- Goal: a member cannot have more than a set number of pending archive submissions.
- Before you start: no card dependency.
- Read (only these): `ArchiveController.CreateItem`; `ArchiveService.CreateItemAsync`;
  `ArchiveItem` (`CreatedByMemberId`); `Enums.ArchiveModerationState`; `WorkflowDto`;
  `GHCAA.Tests/OrgConfig/OrgConfigGoldenSnapshotTests.cs`.
- Change (only these files): `OrgConfigDto.cs` (`ArchivePendingLimit`, default 5);
  `GHCAA.Domain/Constants.cs` (the default and an `ErrorCodes.ArchivePendingLimit` code);
  `ArchiveService.cs`; `ArchiveController.cs`; `org-config-ghc.golden.json`;
  `GHCAA.Tests/Services/ArchiveServiceTests.cs`; `docs/CONFIG_DRIVEN_FRAMEWORK.md`;
  `docs/API_CONTRACT_REGISTRY.md`; `docs/TODO.md`.
- Do not touch: admin submissions (they skip moderation and the cap); client code.
- Steps:
  1. Add the setting with default 5. -> check: golden snapshot passes after the golden update.
  2. In `CreateItemAsync`, for a non-admin, count the member's items in `Pending`; refuse at the
     limit with the new error code. -> check: the sixth pending item is refused.
  3. The controller maps the refusal to 409 with the standard error body. -> check: controller test.
- Contract: `POST api/archive/items` can return 409 with the new code; record it.
- Tests: `ArchiveServiceTests.cs` :: `CreateItem_AtPendingLimit_Refused`,
  `CreateItem_AfterOneApproved_Allowed`, `CreateItem_Admin_NotCapped`; `OrgConfigGoldenSnapshotTests`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~Archive|FullyQualifiedName~OrgConfigGolden"`;
  `python scripts/ci-local.py --only contract`.
- Done when: tests pass; 84.66 closes noting that no client submits archive items yet.
- Stop and ask if: `ArchiveServiceTests.cs` does not exist and the archive tests live elsewhere
  (use that file instead and say so).

<a id="s93"></a>
**S93 · 51.5 · M**
- Goal: file storage limits and image settings are read from OrgConfig at run time, with
  `Constants.Defaults` as fallback, and edited on an admin settings tab.
- Before you start: after S80.
- Read (only these): `FileStorageOptions` and `ImageCompressionOptions` in
  `GHCAA.Infrastructure/Options/ConfigurationOptions.cs`; `FileStorageServiceBase.cs`;
  `DependencyInjection.cs` (storage registration); `IOrgConfigService`;
  `GHCAA.Web/src/app/admin/org-config/org-config.ts` (tabs).
- Change (only these files): `OrgConfigDto.cs` (a `FileStorage` section: `MaxFileSizeBytes`,
  `ImageQuality`, `FallbackImageQuality`, `TargetImageSizeKB`, `ImageMaxDimensionPx`);
  `FileStorageServiceBase.cs`; `LocalFileStorageService.cs`; `S3FileStorageService.cs`;
  `org-config.ts`, `org-config.html`; the golden json; `LocalFileStorageServiceTests.cs`,
  `S3FileStorageServiceTests.cs`; `org-config.spec.ts`; `docs/CONFIG_DRIVEN_FRAMEWORK.md`;
  `docs/TODO.md`.
- Do not touch: `appsettings*.json` (the options binding stays as the base); S3 credentials.
- Steps:
  1. Add the section with null meaning "use the bound option, then `Constants.Defaults`".
     -> check: golden passes after update.
  2. `FileStorageServiceBase` takes `IOrgConfigService` and reads the section per call (both
     storages are scoped). -> check: a test with a set OrgConfig value uses it; a null falls back.
  3. Admin tab "File storage" with number inputs and range checks matching the server's.
     -> check: the spec saves a value and rejects an out-of-range one.
  4. Remove the unused `Constants.ConfigKeys` FileStorage keys only if this change made them
     replaceable; otherwise report them. -> check: build passes.
- Contract: OrgConfig gains a section; record it and run the contract check.
- Tests: `LocalFileStorageServiceTests.cs` :: `Save_UsesOrgConfigMaxSize`,
  `Save_NullOrgConfig_FallsBackToOptions`; `S3FileStorageServiceTests.cs` ::
  `Save_UsesOrgConfigImageQuality`; `org-config.spec.ts` :: `saves file storage tab`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~FileStorage|FullyQualifiedName~OrgConfigGolden"`;
  `cd GHCAA.Web; npm run test:unit -- src/app/admin/org-config`; `python scripts/ci-local.py --only contract`.
- Done when: tests pass; code-reviewer pass clean; 51.5 closes.
- Stop and ask if: reading OrgConfig in the storage service creates a dependency cycle in DI.

<a id="s94"></a>
**S94 · 84.98 · M**
- Goal: the seven theme gaps on 84.98 are each fixed or recorded as intended.
- Before you start: no card dependency.
- Read (only these): `GHCAA.Web/src/app/core/utils/color-math.ts`; `org-config.service.ts`
  (accent checks); `GHCAA.Web/src/styles/themes/_light.scss` and `_dark.scss`;
  `GHCAA.Mobile/lib/core/theme/app_theme.dart`;
  `GHCAA.Mobile/lib/features/theme/dynamic_theme_service.dart`.
- Change (only these files): `color-math.ts`, `color-math.spec.ts`; `org-config.service.ts`,
  `org-config.service.spec.ts`; `_light.scss`, `_dark.scss`; `directory.scss`; the component SCSS
  files that `grep -rln "text-on-dark" GHCAA.Web/src/app` lists beside `--primary-color`; the files
  with `:focus` box-shadow from `grep -rln ":focus\b" GHCAA.Web/src`; `dynamic_theme_service.dart`;
  `GHCAA.Mobile/test/special_theme_parse_test.dart`; `docs/TODO.md`.
- Do not touch: OrgConfig data; the theme token names.
- Steps:
  1. Light accent check uses the darkest light surface (`--surface-color`), not `#ffffff`.
     -> check: the spec fails an accent that passes on white but not on `#f8f9fa`.
  2. Pick one 8-digit hex order (RRGGBBAA, the CSS order) and make Dart parse the same; both
     `readableTextOn` treat alpha by blending on the surface. -> check: web and Dart tests give the
     same result for one 8-digit value.
  3. `--accent-gold-bright` follows the narrowed gradient spread. -> check: `.applied` text meets
     AA contrast.
  4. Disabled `.btn-primary` visible on a dark card. -> check: browser check in default dark mode.
  5. Swap `--text-on-dark` for `--text-on-primary` where the fill is `--primary-color`. -> check:
     a light custom primary keeps nav and footer text readable.
  6. Move `:focus` box-shadow rules to `:focus-visible`. -> check: mouse click shows no ring;
     Tab shows it.
  7. Tab through the admin table and a modal. -> check: focus is visible and stays in the modal.
- Contract: none.
- Tests: `color-math.spec.ts` :: `readable text counts alpha`, `light check uses surface colour`;
  `org-config.service.spec.ts` :: `accent check against surface`;
  `GHCAA.Mobile/test/special_theme_parse_test.dart` :: `eight digit hex is RRGGBBAA`.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/core/utils src/app/core/services/org-config.service.spec.ts`;
  `cd GHCAA.Mobile; flutter test test/special_theme_parse_test.dart test/app_theme_contrast_test.dart --exclude-tags golden`.
- Done when: each of the seven gaps has a fix or a written reason; browser check in both modes with
  a light custom primary; 84.98 closes.
- Stop and ask if: changing Dart's hex order breaks stored themes (check the seed and OrgConfig
  values first).

<a id="s134"></a>
**S134 · 84.105 · S**
- Goal: an archived member's pending payment no longer shows as "Unknown member" on
  `admin/payments-pending`.
- Before you start: no card dependency. Ask the owner first: should archiving reject the member's
  pending payments with a reason, or should the pending list keep them and name the member?
- Read (only these): `GHCAA.Infrastructure/Services/FinancialService.cs` (`GetPendingPaymentsAsync`,
  `RejectPaymentAsync`); the member service's `ArchiveMemberAsync`; the `Member` global query filter
  in `ApplicationDbContext`; `GHCAA.Web/src/app/admin/payments-pending/payments-pending.html`.
- Change (only these files): `FinancialService.cs` or the member service, by the owner's answer;
  `payments-pending.html` and `.spec.ts` if the list shows an archived tag;
  `GHCAA.Tests/Services/FinancialServiceTests.cs`; `docs/TODO.md`.
- Do not touch: the `Member` query filter itself; the reject rule of 3 to 500 visible characters.
- Steps:
  1. Keep and name: read the member name with `IgnoreQueryFilters()` in the pending query and mark
     the row archived. -> check: the test sees the archived member's name.
  2. Or settle on archive: reject each Pending payment of the member with a fixed reason from
     `Constants`, in the archive transaction. -> check: the test sees no Pending row left.
- Contract: if the pending DTO gains a field, record it in `docs/API_CONTRACT_REGISTRY.md`.
- Tests: `FinancialServiceTests` :: `PendingPayments_NameArchivedMember` or
  `ArchiveMember_RejectsPendingPayments`, by the owner's answer.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~FinancialService"`;
  web spec when the page changes.
- Done when: the chosen case has a test and 84.105 closes.
- Stop and ask if: settling on archive would also need a member notice the owner did not ask for.

<a id="s135"></a>
**S135 · 84.106 · S**
- Goal: the mobile financial portal shows "GET RECEIPT" only on Completed payments.
- Before you start: no card dependency.
- Read (only these): `GHCAA.Mobile/lib/screens/member/financial_portal_screen.dart` (the history
  row and `_downloadReceipt`); `GHCAA.Mobile/test/financial_portal_rejection_test.dart` (how it
  builds the screen).
- Change (only these files): `financial_portal_screen.dart`;
  `GHCAA.Mobile/test/financial_portal_rejection_test.dart`; `docs/TODO.md`.
- Do not touch: the receipt route; the web payments page.
- Steps:
  1. Build the link only when the row's status is Completed. -> check: a Failed and a Pending row
     have no link.
- Contract: none.
- Tests: `financial_portal_rejection_test.dart` :: `receipt link only on completed payments`.
- Run: `cd GHCAA.Mobile; flutter analyze; flutter test test/financial_portal_rejection_test.dart --exclude-tags golden`.
- Done when: the test passes and 84.106 closes.
- Stop and ask if: the status in the history map is not the enum name the test expects.

<a id="s95"></a>
**S95 · 90.1, 90.2 · L · migration 15**
- Goal: saved reports are stored, and `ReportService` runs a whitelisted, capped query per entity and
  exports to Excel.
- Before you start: after S18. Read spec 010 and its `tasks.md`; spec 010 wins over the tracker
  (`CreatedByUserId`).
- Read (only these): `docs/specs/010-ad-hoc-reporting/spec.md` and `tasks.md`; one entity
  configuration in `GHCAA.Infrastructure/Data/Configurations/`; the latest PgSql migration
  (`20261117120000_SeedPolicySiteContent`) and its `.Designer.cs`; how `MemberImportService.cs`
  uses ClosedXML.
- Change (only these files): `GHCAA.Domain/Models/SavedReport.cs` (new);
  `GHCAA.Infrastructure/Data/Configurations/SavedReportConfiguration.cs` (new);
  `ApplicationDbContext.cs` (DbSet); `GHCAA.Infrastructure/Data/Migrations/PgSql/<timestamp>_AddSavedReports.cs`
  (new) and its `.Designer.cs` (new); the model snapshot;
  `GHCAA.Application/Interfaces/IReportService.cs` (new); `GHCAA.Application/DTOs/Reports/` (new);
  `GHCAA.Infrastructure/Services/ReportService.cs` (new); `DependencyInjection.cs`;
  `GHCAA.Domain/Constants.cs` (`Reports.RowCap`, entity names);
  `GHCAA.Tests/Services/ReportServiceTests.cs` (new); the plan's migration ledger row 15; `docs/TODO.md`.
- Do not touch: other migrations; the import service.
- Steps:
  1. Model with `Id`, `Name`, `EntityType`, `FiltersJson`, `ColumnsJson`, `CreatedByUserId`,
     `CreatedAt`; configuration and hand-written migration. -> check: `dotnet ef migrations
     has-pending-model-changes` reports none.
  2. `IReportService` with `GetEntityFields`, `RunReport`, `SaveReport`, `GetSavedReports`,
     `ExportToExcel`. Entities: Members, Financial Records, Scholarships, Campaigns, Election
     turnout. -> check: build passes.
  3. Each entity has a field whitelist; any other field is refused. Queries compose `IQueryable`,
     never raw SQL. -> check: test with an unknown field fails with a validation error.
  4. Results stop at `Constants.Reports.RowCap` and say so. -> check: a test with more rows is cut
     and flagged.
  5. Excel export with ClosedXML, one sheet, header row from the chosen columns. -> check: test
     reads back the row count.
- Contract: none yet (S96 adds routes).
- Tests: `GHCAA.Tests/Services/ReportServiceTests.cs` :: `Run_UnknownField_Rejected`,
  `Run_OverRowCap_TruncatedAndFlagged`, `SaveReport_RoundTrip`, `Export_RowCountMatches`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ReportService"`.
- Done when: tests pass; migration has its Designer and snapshot edit; code-reviewer pass clean;
  ledger row 15 marked built; 90.1 and 90.2 close.
- Stop and ask if: an entity in the spec has no clear field list; election turnout needs data that
  breaks ballot secrecy (stop and ask, never expose per-voter rows).

<a id="s96"></a>
**S96 · 90.3, 90.5 · M**
- Goal: admins call the report service through `api/admin/reports`, with tests for every route.
- Before you start: after S95.
- Read (only these): `IReportService.cs`; `CampaignsController.cs` for the `Constants.Policies.AdminOnly`
  pattern; spec 010 `tasks.md` Phase 4.
- Change (only these files): `GHCAA.API/Controllers/AdminReportsController.cs` (new);
  `GHCAA.Tests/Controllers/AdminReportsControllerTests.cs` (new); `ReportServiceTests.cs` (new in S95, FR tags);
  `docs/API_CONTRACT_REGISTRY.md`; `docs/TODO.md`.
- Do not touch: `ReportService` logic, unless a test shows a bug (then fix it and say so).
- Steps:
  1. Routes `GET entities`, `POST run`, `POST export`, `POST saved`, `GET saved`,
     `POST saved/{id}/run` under `api/admin/reports`, all `AdminOnly`. -> check: build passes.
  2. Saved reports belong to the caller's user id; another admin's id gives 404. -> check: test.
  3. Export returns the xlsx content type and a file name from a constant. -> check: test.
  4. Run `python scripts/authz_catalog.py --write`. -> check: six routes listed.
- Contract: six new routes; record them and run the contract check.
- Tests: `AdminReportsControllerTests.cs` :: `Run_NonAdmin_Forbidden`, `Run_ValidRequest_ReturnsRows`,
  `SavedRun_OtherUsersReport_NotFound`, `Export_ReturnsXlsx`. FR-tag new tests.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~Report"`;
  `python scripts/ci-local.py --only contract`.
- Done when: tests pass; code-reviewer pass clean; 90.3 and 90.5 close (the vitest part of 90.5
  closes in S97).
- Stop and ask if: saved reports should be shared between admins (spec says per user).

<a id="s97"></a>
**S97 · 90.4 · M**
- Goal: an admin reports page with entity picker, filter form, result grid and export.
- Before you start: after S96, and S18's `DataGridComponent` must exist in
  `GHCAA.Web/src/app/common/data-grid/`.
- Read (only these): `DataGridComponent`; `app-page-header`, `app-search-bar`, `app-pagination`,
  `app-export-buttons`, `app-loading-panel` in `common/`; `AdminReportsController` routes;
  `GHCAA.Web/src/app/core/services/nav.service.ts` (admin sections).
- Change (only these files): `GHCAA.Web/src/app/admin/reports/` (new: component, template, styles,
  spec); `GHCAA.Web/src/app/core/services/report.service.ts` (new);
  `report.service.spec.ts` (new);
  `app.constants.ts` (`ADMIN_REPORTS` in `API_ENDPOINTS`); the admin routes file; `nav.service.ts`;
  `docs/TODO.md`.
- Do not touch: the shared components; other admin pages.
- Steps:
  1. Service methods for the six routes using `ADMIN_REPORTS`. -> check: service spec hits each URL.
  2. Page: pick entity, then fields and filters from `GET entities`; run shows rows in the grid with
     pagination; a cut result shows the row-cap note. -> check: component spec.
  3. Save and re-run saved reports; export downloads xlsx. -> check: spec.
  4. Nav entry for admins and a guarded route. -> check: a member cannot open it.
  5. Browser check in light and dark mode. -> check: no hardcoded colours.
- Contract: none new (client of S96 routes).
- Tests: `GHCAA.Web/src/app/admin/reports/reports.spec.ts` :: `runs report into grid`,
  `shows row cap note`, `saves and reruns report`; `report.service.spec.ts` :: `calls run route`.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/admin/reports src/app/core/services/report.service.spec.ts`.
- Done when: specs pass; browser check done; 90.4 closes and the vitest part of 90.5 is noted.
- Stop and ask if: the shared grid cannot show typed columns (dates, money) without a change to it.

<a id="s99"></a>
**S99 · 37.4 (backend) · L · migration 16**
- Goal: batch cohorts and reunions exist as data with admin create and edit routes.
- Before you start: no card dependency. Read the 37.4 tracker entry for the field list.
- Read (only these): `AcademicRecord` (`PassingYear`, `IsOrgProfile`); `AlumniEvent`; one
  configuration file; the latest PgSql migration and its `.Designer.cs`.
- Change (only these files): `GHCAA.Domain/Models/BatchCohort.cs` (new);
  `GHCAA.Domain/Models/Reunion.cs` (new); their configurations in
  `GHCAA.Infrastructure/Data/Configurations/` (new); `ApplicationDbContext.cs`;
  `GHCAA.Infrastructure/Data/Migrations/PgSql/<timestamp>_AddBatchCohortsAndReunions.cs` (new) and
  its `.Designer.cs` (new); the snapshot; `GHCAA.Application/Interfaces/ICohortService.cs` (new);
  `GHCAA.Infrastructure/Services/CohortService.cs` (new);
  `GHCAA.API/Controllers/CohortsController.cs` (new); `DependencyInjection.cs`; `GHCAA.Tests/Services/CohortServiceTests.cs` (new); ledger row 16;
  `docs/API_CONTRACT_REGISTRY.md`; `docs/TODO.md`.
- Do not touch: `AcademicRecord`; event code.
- Steps:
  1. `BatchCohort` (`Id`, `PassingYear` unique, `Title`, `Story?`, `CoverImagePath?`,
     `RepresentativeMemberId?`, `IsActive`) and `Reunion` (`Id`, `BatchCohortId?`, `AlumniEventId`,
     `Theme`, `SouvenirUrl?`); migration by hand. -> check: no pending model changes.
  2. Service: list active cohorts with member count from `AcademicRecord.PassingYear` where
     `IsOrgProfile`; get one with its reunions; admin create and update; `Story` sanitized with the
     service's static HtmlSanitizer. -> check: tests.
  3. Routes: public `GET api/cohorts`, `GET api/cohorts/{year}`; admin `POST` and `PUT`. Run
     `authz_catalog.py --write`. -> check: a member cannot create.
- Contract: four new routes; record them and run the contract check.
- Tests: `CohortServiceTests.cs` :: `List_CountsOrgProfileMembersByYear`,
  `Create_DuplicateYear_Refused`, `Update_StorySanitized`, `Get_IncludesReunions`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~Cohort"`;
  `python scripts/ci-local.py --only contract`.
- Done when: tests pass; code-reviewer pass clean; ledger row 16 marked built.
- Stop and ask if: a reunion should exist without an event.

<a id="s100"></a>
**S100 · 37.4 (clients) · M**
- Goal: a public cohort list and cohort page on web and mobile, and an admin cohort editor on web.
- Before you start: after S99.
- Read (only these): `CohortsController`; one public list page on web (`common/directory/`) and on
  mobile; the shared components in `common/`.
- Change (only these files): `GHCAA.Web/src/app/public/cohorts/` (new: list and detail, with specs);
  `GHCAA.Web/src/app/admin/cohorts/` (new, with spec);
  `GHCAA.Web/src/app/core/services/cohort.service.ts` (new); `app.constants.ts`; the web routes and `nav.service.ts`;
  `GHCAA.Mobile/lib/screens/member/cohorts_screen.dart` (new);
  `GHCAA.Mobile/lib/features/cohorts/cohort_service.dart` (new); `app_router.dart`;
  `GHCAA.Mobile/test/cohorts_screen_test.dart` (new); `docs/TODO.md`.
- Do not touch: the API.
- Steps:
  1. Web service and public list (year, title, member count) and detail (story, reunions).
     -> check: specs.
  2. Web admin editor with cover image staged upload (upload on Save only). -> check: spec.
  3. Mobile list and detail. -> check: widget test on a phone viewport.
- Contract: none new.
- Tests: `cohort-list.spec.ts` (new) :: `lists cohorts by year`; `admin-cohorts.spec.ts` (new) ::
  `saves cohort with cover on save`; `GHCAA.Mobile/test/cohorts_screen_test.dart` ::
  `lists cohorts`, `opens cohort detail`.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/public/cohorts src/app/admin/cohorts`;
  `cd GHCAA.Mobile; flutter test test/cohorts_screen_test.dart --exclude-tags golden`.
- Done when: tests pass; browser and phone checks done; 37.4 closes.
- Stop and ask if: the owner wants cohorts visible to members only.

<a id="s101"></a>
**S101 · 37.5 (backend) · L · migration 17**
- Goal: In Memoriam entries and condolences exist with moderation, and linking a member to an entry
  deactivates the member.
- Before you start: no card dependency. Read spec 003 Story 3 (In Memoriam).
- Read (only these): spec 003 `spec.md` Story 3; `Member` (status and directory visibility);
  `SubmissionStatus`; one configuration file; the latest PgSql migration and its `.Designer.cs`.
- Change (only these files): `GHCAA.Domain/Models/MemorialEntry.cs` (new);
  `GHCAA.Domain/Models/Condolence.cs` (new); their configurations (new); `ApplicationDbContext.cs`;
  `GHCAA.Infrastructure/Data/Migrations/PgSql/<timestamp>_AddMemorialEntries.cs` (new) and its
  `.Designer.cs` (new); the snapshot; `GHCAA.Application/Interfaces/IMemorialService.cs` (new);
  `GHCAA.Infrastructure/Services/MemorialService.cs` (new);
  `GHCAA.API/Controllers/MemorialController.cs` (new); `DependencyInjection.cs`;
  `GHCAA.Tests/Services/MemorialServiceTests.cs` (new); ledger row 17;
  `docs/API_CONTRACT_REGISTRY.md`; `docs/TODO.md`.
- Do not touch: member delete code.
- Steps:
  1. `MemorialEntry` (`Id`, `MemberId?`, `FullName`, `PassingYear?`, `DateOfBirth?`,
     `DateOfDeath`, `PhotoPath?`, `Tribute`, `IsPublished`, `SubmittedByMemberId?`, `Status`,
     `CreatedAt`) and `Condolence` (`Id`, `MemorialEntryId`, `MemberId`, `Message`, `PostedAt`,
     `IsApproved`); migration by hand. -> check: no pending model changes.
  2. Members submit entries as Pending; admins approve and publish. Tribute and messages sanitized.
     -> check: tests.
  3. Approving an entry with `MemberId` deactivates that member and hides them from the directory in
     the same transaction. -> check: test shows both changes or neither.
  4. Condolences need approval before they show. Routes: public list and detail; member submit and
     condole; admin approve. Run `authz_catalog.py --write`. -> check: public sees only published
     entries and approved condolences.
- Contract: new routes; record them and run the contract check.
- Tests: `MemorialServiceTests.cs` :: `Approve_LinkedMember_DeactivatesAndHides`,
  `Approve_FailsMidway_RollsBackBoth`, `PublicList_OnlyPublished`, `Condolence_HiddenUntilApproved`,
  `Submit_TributeSanitized`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~Memorial"`;
  `python scripts/ci-local.py --only contract`.
- Done when: tests pass; code-reviewer pass clean; ledger row 17 marked built.
- Stop and ask if: deactivation should also end roles, dues or election rights (owner decision).

<a id="s102"></a>
**S102 · 37.5 (clients) · M**
- Goal: In Memoriam pages on web and mobile, a submit form for members and an admin moderation page.
- Before you start: after S101.
- Read (only these): `MemorialController`; spec 003 Story 3 acceptance lines; the staged upload
  pattern on an existing web admin page.
- Change (only these files): `GHCAA.Web/src/app/public/in-memoriam/` (new, with spec);
  `GHCAA.Web/src/app/admin/memorial/` (new, with spec);
  `GHCAA.Web/src/app/core/services/memorial.service.ts` (new); `app.constants.ts`; web routes and `nav.service.ts`;
  `GHCAA.Mobile/lib/screens/member/in_memoriam_screen.dart` (new);
  `GHCAA.Mobile/lib/features/memorial/memorial_service.dart` (new); `app_router.dart`;
  `GHCAA.Mobile/test/in_memoriam_screen_test.dart` (new); `docs/TODO.md`.
- Do not touch: the API.
- Steps:
  1. Public list and entry page with approved condolences; members post a condolence. -> check: spec.
  2. Member submit form with staged photo upload. -> check: spec.
  3. Admin page: approve entries and condolences; link a member with a warning that the member will
     be deactivated. -> check: spec.
  4. Mobile list, entry and condolence. -> check: widget test.
- Contract: none new.
- Tests: `in-memoriam.spec.ts` (new) :: `shows approved condolences only`; `admin-memorial.spec.ts` (new) ::
  `warns before linking member`; `GHCAA.Mobile/test/in_memoriam_screen_test.dart` ::
  `lists entries`, `posts condolence`.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/public/in-memoriam src/app/admin/memorial`;
  `cd GHCAA.Mobile; flutter test test/in_memoriam_screen_test.dart --exclude-tags golden`.
- Done when: tests pass; browser and phone checks done; 37.5 closes.
- Stop and ask if: the owner wants condolences without moderation.

<a id="s103"></a>
**S103 · 37.10 · L**
- Goal: a yearly impact report built from the ledger, scholarships, campaigns and membership, shown on
  web and mobile and downloadable as a PDF.
- Before you start: after S100. Read spec 003 Story 2 (impact report).
- Read (only these): spec 003 `spec.md` Story 2; `IFinancialLedgerService.GetSummaryAsync`;
  `ScholarshipAward`; `CampaignPledge`; `MembershipHistory`; `IDCardService.GenerateIDCardPdfAsync`
  (QuestPDF pattern); the SiteContent keys.
- Change (only these files): `GHCAA.Application/DTOs/ImpactReportDto.cs` (new);
  `GHCAA.Application/Interfaces/IImpactReportService.cs` (new);
  `GHCAA.Infrastructure/Services/ImpactReportService.cs` (new); `ImpactReportController.cs` (new) in
  `GHCAA.API/Controllers/`; `DependencyInjection.cs`;
  `GHCAA.Tests/Services/ImpactReportServiceTests.cs` (new); `GHCAA.Web/src/app/public/impact-report/` (new, with spec); `app.constants.ts`; web routes;
  `GHCAA.Mobile/lib/screens/member/impact_report_screen.dart` (new);
  `GHCAA.Mobile/lib/features/impact_report/impact_report_service.dart` (new); `app_router.dart`;
  `GHCAA.Mobile/test/impact_report_screen_test.dart` (new); `docs/API_CONTRACT_REGISTRY.md`;
  `docs/TODO.md`.
- Do not touch: the ledger service logic; the Seed json (foreword and cover are site content added by
  an admin).
- Steps:
  1. `GetImpactReportAsync(int year, CancellationToken cancellationToken = default)` returns income
     and spend by category, scholarships awarded, campaign totals, new members and new cohorts, or
     null for a year with no data. -> check: tests.
  2. PDF with QuestPDF; foreword and cover from SiteContent when present. -> check: test gets a
     non-empty PDF.
  3. Routes `GET api/impact-report/{year}` and `.../pdf`, public. Run `authz_catalog.py --write`.
     -> check: unknown year gives 404.
  4. Web page with year picker and download. -> check: spec.
  5. Mobile screen with year picker; the PDF opens through `Printing` after an authenticated
     download, the S87 pattern. -> check: widget test on a phone viewport.
- Contract: two new routes; record them and run the contract check.
- Tests: `ImpactReportServiceTests.cs` :: `Report_SumsLedgerByCategory`, `Report_EmptyYear_ReturnsNull`,
  `Pdf_GeneratesBytes`; `impact-report.spec.ts` (new) :: `downloads pdf for year`;
  `GHCAA.Mobile/test/impact_report_screen_test.dart` :: `shows totals for year`, `shows empty year`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ImpactReport"`;
  `cd GHCAA.Web; npm run test:unit -- src/app/public/impact-report`;
  `cd GHCAA.Mobile; flutter test test/impact_report_screen_test.dart --exclude-tags golden`;
  `python scripts/ci-local.py --only contract`.
- Done when: tests pass; code-reviewer pass clean; 37.10 closes.
- Stop and ask if: any figure would show an individual donor or member amount (the owner decides
  what is public).

<a id="s104"></a>
**S104 · 62.56 (a) · M**
- Goal: the profile pack has a section map and a check that every key is unique across sections.
- Before you start: after S79 (`ProfilePackFiles` exists). Spec 025 tasks T001 to T004.
- Read (only these): `docs/specs/025-*/spec.md` and `tasks.md` (T001 to T004); `ProfilePackFiles`;
  `GHCAA.Infrastructure/Data/Seed/profiles/default` and `profiles/ghc`.
- Change (only these files): the files T001 to T004 name; `docs/specs/025-*/tasks.md` (tick the
  tasks); `docs/TODO.md`.
- Do not touch: seed data values.
- Steps:
  1. Build T001 to T004 in order, each with the check the task gives. -> check: each task's test
     passes.
- Contract: as the tasks say (none expected).
- Tests: the test files T001 to T004 name, by path.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ProfilePack"`.
- Done when: T001 to T004 are ticked; 62.56 part a is noted on the tracker.
- Stop and ask if: a task names a file S79 did not create.

<a id="s105"></a>
**S105 · 62.56 (b) · M**
- Goal: an admin can export the profile pack.
- Before you start: after S104. Spec 025 tasks T005 to T008 and the export part of T022.
- Read (only these): spec 025 `tasks.md` (T005 to T008, T022); `ProfilePackFiles`.
- Change (only these files): the files those tasks name; `tasks.md`; `docs/API_CONTRACT_REGISTRY.md`;
  `docs/TODO.md`.
- Do not touch: import code (S106).
- Steps:
  1. Build T005 to T008, then the web export button from T022. -> check: each task's test passes.
  2. Run `authz_catalog.py --write` for any new route. -> check: route listed.
- Contract: the export route; record it and run the contract check.
- Tests: the test files the tasks name, by path.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ProfilePack"`;
  `python scripts/ci-local.py --only contract`.
- Done when: tasks ticked; code-reviewer pass clean; part b noted.
- Stop and ask if: export would include secrets or connection strings (it must not).

<a id="s106"></a>
**S106 · 62.56 (c, d) · L**
- Goal: an admin uploads a pack, maps its sections and sees a preview before anything changes.
- Before you start: after S105. Spec 025 tasks T009 to T014 and T022 to T025.
- Read (only these): spec 025 `tasks.md` (T009 to T014, T022 to T025).
- Change (only these files): the files those tasks name; `tasks.md`; `docs/API_CONTRACT_REGISTRY.md`;
  `docs/TODO.md`.
- Do not touch: apply and transaction code (S107).
- Steps:
  1. Build T009 to T014 (upload, validation, mapping, preview) and T022 to T025 (web). -> check:
     each task's test passes; preview writes nothing to the database.
- Contract: upload and preview routes; record them and run the contract check.
- Tests: the test files the tasks name, by path.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ProfilePack"`;
  `cd GHCAA.Web; npm run test:unit -- <the spec paths the tasks name>`; `python scripts/ci-local.py --only contract`.
- Done when: tasks ticked; code-reviewer pass clean; parts c and d noted.
- Stop and ask if: an uploaded file could overwrite data outside the pack's sections.

<a id="s107"></a>
**S107 · 62.56 (e, f) · M**
- Goal: import applies in merge or replace mode inside one transaction, with tests and docs.
- Before you start: after S106. Spec 025 tasks T015 to T021 and T026 to T032.
- Read (only these): spec 025 `tasks.md` (T015 to T021, T026 to T032).
- Change (only these files): the files those tasks name; `tasks.md`; `docs/TODO.md`.
- Do not touch: export and preview code, unless a task names it.
- Steps:
  1. Build T015 to T021 (modes, transaction) and T026 to T032 (tests, docs). -> check: a failed
     import leaves the database unchanged.
- Contract: as the tasks say; record any route change.
- Tests: the test files the tasks name, by path.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~ProfilePack"`.
- Done when: all spec 025 tasks ticked; code-reviewer pass clean; 62.56 closes.
- Stop and ask if: replace mode would delete rows other tables point to.

<a id="s108"></a>
**S108 · 60.3, 6.2, 8.5, 8.7, 37.9 · S**
- Goal: one owner question round closes or keeps five small items.
- Before you start: no card dependency.
- Read (only these): the five tracker entries.
- Change (only these files): `docs/TODO.md`.
- Do not touch: code.
- Steps:
  1. Ask the owner, with these defaults: 60.3 close as a naming difference (web uses
     `api/messaging`, mobile `api/chat`, both on `MessagingController`; forum routes match); 6.2 fold
     into 84.83 or drop; 8.5 drop unless a measured slowness exists; 8.7 drop with 8.5; 37.9 keep at
     P4 for after 37.4. -> check: each item has an answer.
  2. Record each answer on its entry. -> check: tracker check passes.
- Contract: none.
- Tests: none (tracker only).
- Run: `python scripts/backlog_inventory.py --check docs/MASTER_EXECUTION_PLAN.md`.
- Done when: five items have recorded answers; a build card is added only for a "build" answer.
- Stop and ask if: an answer needs code; write a new card instead of building it here.

<a id="s109"></a>
**S109 · 37.11 · S**
- Goal: WP37 baselines and the main docs match the code after the WP37 items.
- Before you start: after S103.
- Read (only these): the WP37 tracker block; `docs/FEATURES.md`, `docs/PROJECT_MAP.md`,
  `docs/SRS.md`, `docs/ARCHITECTURE.md`.
- Change (only these files): those four docs; the WP37 baseline files the tracker names;
  `docs/TODO.md`.
- Do not touch: code; `docs/book/`.
- Steps:
  1. Run `python scripts/ci-local.py` and refresh the baselines from it. -> check: the run is green.
  2. Update the four docs for cohorts, memorial and the impact report. -> check: each new route and
     page appears once.
  3. Close 37.11 if every other WP37 item is DONE or WONTDO; otherwise set it PARTIAL naming the gate
     items (G-DEV, G-BN). -> check: tracker check passes.
- Contract: none.
- Tests: none new.
- Run: `python scripts/ci-local.py`; `python scripts/backlog_inventory.py --check docs/MASTER_EXECUTION_PLAN.md`.
- Done when: docs match the code and 37.11 has its status.
- Stop and ask if: ci-local fails for a reason outside WP37.

<a id="s131"></a>
**S131 · 8.9 · M**
- Goal: mobile moves from `package:flutter/material.dart` to `material_ui` and `cupertino_ui` when a
  Flutter release deprecates it, with CI pinned to that release.
- Before you start: only once the pinned Flutter release deprecates `package:flutter/material.dart`. Today's pin is
  the one in the CI workflows.
- Read (only these): the Flutter release notes for the new version; `GHCAA.Mobile/pubspec.yaml`;
  `GHCAA.Mobile/lib/main.dart` (the delegates); the `flutter-version` lines in
  `.github/workflows/ghcaa-ci-preprod.yml`, `ghcaa-ci-standard.yml` and `mobile_deployment.yml`.
- Change (only these files): `pubspec.yaml`; the 142 files that import `package:flutter/material.dart` (104 in `lib`,
  33 in `test`, 5 in `integration_test`); `main.dart`; the three workflow files (seven pin lines);
  golden images only if they change; `docs/TODO.md`.
- Do not touch: app logic.
- Steps:
  1. Add `material_ui` and `cupertino_ui`; swap the imports with one scripted replace. -> check:
     `grep -rln "package:flutter/material.dart" GHCAA.Mobile` prints nothing.
  2. Swap the two localization delegates in `main.dart`. -> check: `flutter analyze` with no infos.
  3. Run golden tests; review each changed image before accepting it. -> check: changes are only
     rendering noise.
  4. Raise all seven `flutter-version` pins. -> check: grep shows one version in all three files.
- Contract: none.
- Tests: the existing suite; `@Tags(['golden'])` tests run separately.
- Run: `cd GHCAA.Mobile; flutter analyze; flutter test --exclude-tags golden; flutter test --tags golden`.
- Done when: the pinned CI analyze job passes on the new version; 8.9 closes.
- Stop and ask if: a golden changes layout, not only pixels; a package in pubspec does not support the
  new Flutter.

### Phase I. Public site, admin and member experience gaps

Added 2026-11-17 from the public and admin site reviews. Design: `docs/specs/029-public-admin-member-experience-gaps/`
(spec, plan and task list for Work Package 100). Owner, 2026-11-17: the sessions run by their
dependencies, so their rows sit in phases B, F and G of section 3 and only the cards are kept
together here. S111 runs before S14 (84.81, dues and event reminders) so reminders use the same
mail check: dues reminders always send and event reminders follow the member's preference. The
owner answered D1 to D8 of spec 029 on 2026-11-17.

Names in these cards were checked against the code on 2026-11-20. What the spec and the older
cards assumed that the code does not have, or already has:

- The communication routes live at `api/admin/comm` (`CommunicationController`, admin only), not
  `api/communication` as the spec 029 plan said. Email sends in `CommunicationService` never read
  the `Notify*` flags; only in-app notifications in `NotificationService` do.
- `MemberService_Profile.cs` saves four flags (lines 260 to 263) and leaves out
  `NotifyRelevantUpdates`. There are no mail kind constants; `Constants.TemplateCodes` holds the six
  template codes.
- `FinancialService` sets a notification link to `/finance/history`, which is no web route.
- `EventRegistrationDto` (`GHCAA.Application/DTOs/EventDto.cs`) has no `TicketCode`.
  `CheckInByTicketCodeAsync` already refuses any registration that is not Approved, so a cancelled
  pass (S12 adds `Cancelled`) is refused already and needs only a test.
- `GET api/admin/stats` returns an anonymous object from `GetDashboardStatsAsync`.
- The web app has no server rendering or prerender setup, and the public site has no `/news/:id`
  page; the API has `GET api/news/{id:int}`.
- Articles are `NewsPost` rows. `NewsPost.MembersOnly` exists, defaults to false, and the create
  path sets `dto.MembersOnly ?? false`, so an approved member article is public today. The planned
  `IsPublic` column would duplicate `MembersOnly`; S123 uses `MembersOnly`.
- `GET api/news` already filters by `articleCategory`, and categories are the `ArticleCategory`
  enum, so a spotlight is a new enum value.
- The fees route `GET api/financials/fees/applicable` returns one amount for one category and
  type; there is no anonymous list of fees.
- There is no general approval mechanism. `IElectionApprovalService` is election only and
  `PendingApprovalsController` only counts queues, so S127 needs an owner answer first.
- 31 web templates build their own `data-table` (27 admin, two member pages, four shared
  components under `common/`). There is no `canDeactivate` guard.
- The Android manifest has only the launcher intent filter. The mobile router has `/news/:id`,
  `/events/:id` and `/jobs/:id`.

<a id="s110"></a>
**S110 · 100.1, 100.2 · S**
- Goal: no public page links into the sign-in wall, the public header has a Join button for
  visitors, and `index.html` names no fixed host.
- Before you start: no card dependency.
- Read (only these): `GHCAA.Web/src/app/public/landing/sections/news-preview/news-preview.html`;
  `GHCAA.Web/src/app/public/landing/sections/jobs-preview/jobs-preview.html`;
  `GHCAA.Web/src/app/layouts/public-layout/public-layout.html` and `public-layout.ts`;
  `GHCAA.Web/src/app/app.routes.ts` (the `/news`, `/jobs` and `/register` routes);
  `GHCAA.Web/src/index.html`; `GHCAA.Web/src/app/core/strategies/branding-title.strategy.ts`;
  `GHCAA.Web/tests/e2e/generic-profile-acceptance.spec.ts`.
- Change (only these files): `news-preview.html`; `jobs-preview.html`; `public-layout.html`;
  `public-layout.ts`; `GHCAA.Web/src/app/layouts/public-layout/public-layout.spec.ts` (new);
  `GHCAA.Web/src/app/public/public-links.guard.spec.ts` (new);
  `GHCAA.Web/src/index.html`; `GHCAA.Web/src/app/core/services/page-meta.service.ts` (new);
  `GHCAA.Web/src/app/core/services/page-meta.service.spec.ts` (new); `docs/TODO.md`.
- Do not touch: the `author` meta tag in `index.html` (owner, 2026-11-17); the portal routes;
  `BrandingTitleStrategy`; the college name, which belongs to Work Package 62.
- Steps:
  1. Point the news preview at `/news` and the jobs preview at `/jobs`. Hide the jobs link when
     `enableJobHub` is off, as the route guard does. -> check: neither template holds `/portal`.
  2. Add a Join link to `/register` in the public header, shown only when no one is signed in,
     using the shared button classes. -> check: spec for both states.
  3. Remove the fixed canonical link and the JSON-LD block from `index.html`. Add
     `PageMetaService`, called on navigation from the public layout, that writes the canonical link
     from the current address and the JSON-LD from the OrgConfig name and logo. -> check:
     `grep -n "onrender" GHCAA.Web/src/index.html` prints nothing; the author tag is still there.
  4. Add a guard spec that reads every `.html` under `app/public/` and `layouts/public-layout/`
     and fails on a `/portal` link other than `/login`. -> check: the spec fails if step 1 is
     undone.
  5. Browser check at phone and desktop width, signed out and signed in. -> check: screenshots
     show the Join button and both preview links open public pages.
- Contract: no API change.
- Tests: `public-links.guard.spec.ts` :: `public templates hold no portal link`;
  `public-layout.spec.ts` :: `shows Join when signed out`, `hides Join when signed in`;
  `page-meta.service.spec.ts` :: `canonical follows the current route`,
  `JSON-LD uses the OrgConfig name and logo`.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/public src/app/layouts/public-layout src/app/core/services/page-meta.service.spec.ts`.
- Done when: the specs pass, the browser check is recorded, and 100.1 and 100.2 close with the
  spec names as evidence.
- Stop and ask if: the e2e white-label test fails on `index.html` for a reason other than the
  host; the OrgConfig logo has no absolute address to put in JSON-LD.

<a id="s111"></a>
**S111 · 100.3 (API) · M**
- Goal: all five mail flags save, every batch and reminder email asks one check before it goes,
  and a member can unsubscribe from one kind of mail from a signed link without signing in.
- Before you start: no card dependency. S14 (84.81) waits for this card.
- Read (only these): `GHCAA.Domain/Models/Member.cs` (the five `Notify*` flags);
  `GHCAA.Infrastructure/Services/MemberService_Profile.cs`;
  `GHCAA.Infrastructure/Services/CommunicationService.cs` (`SendBatchEmailAsync`,
  `SendTypeEmailAsync`, `SendCustomEmailAsync`, `SendEmailByCodeAsync`, `GetEmailFooterAsync`);
  `GHCAA.Infrastructure/Services/NotificationService.cs` (how it maps a kind to a flag);
  `GHCAA.Infrastructure/Services/AuthenticatorService.cs` (the data protection protector);
  `GHCAA.API/Extensions/RateLimitingExtensions.cs`; `GHCAA.Domain/Constants.cs`
  (`TemplateCodes`, `RateLimitPolicies`, `ErrorCodes`).
- Change (only these files): `MemberService_Profile.cs`; `GHCAA.Domain/Constants.cs`;
  `GHCAA.Application/Interfaces/IMailPreferenceService.cs` (new);
  `GHCAA.Infrastructure/Services/MailPreferenceService.cs` (new); `CommunicationService.cs`;
  `GHCAA.API/Controllers/UnsubscribeController.cs` (new); `RateLimitingExtensions.cs`;
  `GHCAA.Infrastructure/DependencyInjection.cs`;
  `GHCAA.Tests/Services/MailPreferenceServiceTests.cs` (new);
  `GHCAA.Tests/Controllers/UnsubscribeControllerTests.cs` (new);
  `GHCAA.Tests/Services/MemberServiceTests.cs`; `GHCAA.Tests/Services/CommunicationServiceTests.cs`;
  `docs/API_CONTRACT_REGISTRY.md`; `docs/TODO.md`;
  `docs/specs/029-public-admin-member-experience-gaps/plan.md`.
- Do not touch: `NotificationService` gating; the OTP and password reset send paths except to mark
  them always sent; the email templates in the database.
- Steps:
  1. Record on 100.3 that email sends do not read the flags today. -> check: the note names
     `CommunicationService`.
  2. Save `NotifyRelevantUpdates` with the other four. -> check: a profile save round trips all
     five.
  3. Add `Constants.MailKinds` with one value per flag and the four always-sent kinds of D8
     (account and security, payment receipt, required election notice, membership status), and a
     set of the always-sent ones. -> check: build passes.
  4. Add `IMailPreferenceService.MayReceiveAsync(memberId, kind)`: true for an always-sent kind
     without reading flags, else the matching flag. It is the only reader of the flags for email.
     -> check: unit tests per kind.
  5. Make the batch, type and custom sends skip a member the check refuses and return the skipped
     count. -> check: an opted-out member gets no mail; an OTP still goes with every flag off.
  6. Add the unsubscribe token: a protector with a new purpose constant, holding member id, kind and
     issue time. `GET api/unsubscribe?token=` turns the kind off and `POST api/unsubscribe/undo`
     turns it back on; both anonymous, on a new `Unsubscribe` rate-limit policy. A changed or
     unknown token returns 400 with an error code; a reused token is harmless. -> check: controller
     tests.
  7. Add the link to the footer of non-always-sent mail only. -> check: an OTP mail has no link.
  8. Fix the route name in the spec 029 plan to `api/admin/comm`. -> check: the plan matches the
     controller.
- Contract: `GET api/unsubscribe`, `POST api/unsubscribe/undo` (new, anonymous); the three send
  routes report a skipped count. Record both in `docs/API_CONTRACT_REGISTRY.md`.
- Tests: `MailPreferenceServiceTests.cs` :: `MayReceive_FlagOff_ReturnsFalse`,
  `MayReceive_AlwaysSentKind_IgnoresFlags`; `CommunicationServiceTests.cs` ::
  `SendBatch_SkipsOptedOutMember`, `SendOtp_SentWithEveryFlagOff`; `MemberServiceTests.cs` ::
  `UpdateProfile_SavesAllFiveNotifyFlags`; `UnsubscribeControllerTests.cs` ::
  `Unsubscribe_ValidToken_TurnsKindOff`, `Unsubscribe_ChangedToken_Returns400`,
  `Unsubscribe_ReusedToken_StaysOff`, `Undo_TurnsKindBackOn`, `Unsubscribe_HasRateLimitPolicy`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~MailPreference|FullyQualifiedName~Unsubscribe|FullyQualifiedName~CommunicationService|FullyQualifiedName~MemberServiceTests"`;
  `python scripts/ci-local.py --only contract`.
- Done when: the tests pass, the routes are in the registry, and 100.3 carries the API part as
  built with the client part open for S112.
- Stop and ask if: a send path in `CommunicationService` has no member id to check; the owner's
  four always-sent kinds do not map onto the existing template codes.

<a id="s112"></a>
**S112 · 100.3 (clients) · M**
- Goal: a member can see and change all five mail flags on web and mobile, and the unsubscribe link
  opens a page that confirms and offers undo.
- Before you start: S111 done.
- Read (only these): `GHCAA.Web/src/app/member/profile/profile.ts` and `profile.html`;
  `GHCAA.Web/src/app/common/notify-toggle/notify-toggle.component.ts`;
  `GHCAA.Web/src/app/core/services/profile.service.ts`; `GHCAA.Web/src/app/app.routes.ts`;
  `GHCAA.Mobile/lib/screens/member/profile_edit_screen.dart`;
  `GHCAA.Mobile/test/profile_edit_screen_test.dart`.
- Change (only these files): `profile.ts`; `profile.html`; `GHCAA.Web/src/app/member/profile/profile.spec.ts`;
  `app.routes.ts`; `GHCAA.Web/src/app/public/unsubscribe/unsubscribe.ts` (new);
  `GHCAA.Web/src/app/public/unsubscribe/unsubscribe.html` (new);
  `GHCAA.Web/src/app/public/unsubscribe/unsubscribe.spec.ts` (new);
  `GHCAA.Web/src/app/core/constants/app.constants.ts` (`API_ENDPOINTS`); `profile_edit_screen.dart`;
  `profile_edit_screen_test.dart`; `docs/TODO.md`.
- Do not touch: the API; `notify-toggle` behaviour for its other callers.
- Steps:
  1. Add an "Email preferences" section to the web profile with five `<app-notify-toggle>`
     controls, saved with the profile. -> check: spec saves all five.
  2. Add `/unsubscribe` as a public route: it calls the S111 route with the token from the query,
     shows the result, and offers Undo. A bad token shows the error message from the server.
     -> check: spec for success, undo and bad token.
  3. Add the five switches to mobile profile edit, saved with the profile. -> check: widget test
     toggles one and sees it in the saved body.
- Contract: no API change.
- Tests: `profile.spec.ts` :: `saves all five email preferences`; `unsubscribe.spec.ts` ::
  `confirms unsubscribe`, `undo turns the kind back on`, `shows the server error for a bad token`;
  `profile_edit_screen_test.dart` :: `email preference switch is saved`.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/member/profile src/app/public/unsubscribe`;
  `cd GHCAA.Mobile; flutter analyze; flutter test test/profile_edit_screen_test.dart --exclude-tags golden`.
- Done when: the specs and widget test pass, a browser check in light and dark is recorded, and
  100.3 closes.
- Stop and ask if: the profile save body on either client cannot carry the flags without an API
  change.

<a id="s113"></a>
**S113 · 100.4 · L**
- Goal: an admin sees how many will get a batch email and how many opted out before sending, the
  send runs in the background, and a retry never mails the same member twice.
- Before you start: S111 done. The mobile admin has no batch send screen (checked 2026-11-20);
  record that on 100.4. Split into an API session and a web session if the card runs long.
- Read (only these): `GHCAA.API/Controllers/CommunicationController.cs`;
  `CommunicationService.cs` (`SendBatchEmailAsync`); `IMailPreferenceService.cs`;
  `GHCAA.API/Services/DataRetentionSweep.cs` (hosted service pattern);
  `GHCAA.Infrastructure/Data/ApplicationDbContext.cs`;
  `GHCAA.Web/src/app/admin/comm/admin-comm.ts` and `admin-comm.html`;
  `GHCAA.Web/src/app/core/services/admin-comm.service.ts`.
- Change (only these files): `GHCAA.Domain/Models/MailBatch.cs` (new);
  `GHCAA.Domain/Models/MailBatchRecipient.cs` (new); `ApplicationDbContext.cs`; the migration
  `AddMailBatches` with its `.Designer.cs` and the model snapshot (ledger row 2a);
  `CommunicationService.cs`; `CommunicationController.cs`;
  `GHCAA.API/Services/MailBatchWorker.cs` (new); `GHCAA.API/Program.cs` (register the worker next
  to `DataRetentionSweep`);
  `GHCAA.Tests/Services/MailBatchWorkerTests.cs` (new);
  `GHCAA.Tests/Services/CommunicationServiceTests.cs`;
  `GHCAA.Tests/Controllers/CommunicationControllerTests.cs`; `admin-comm.ts`; `admin-comm.html`;
  `GHCAA.Web/src/app/admin/comm/admin-comm.spec.ts`; `admin-comm.service.ts`;
  `GHCAA.Web/src/app/core/services/admin-comm.service.spec.ts`;
  `docs/API_CONTRACT_REGISTRY.md`; `docs/TODO.md`.
- Do not touch: the single-member send routes; scheduled sending (left out by 100.4).
- Steps:
  1. Add `POST api/admin/comm/send-batch/preview` returning recipient and opted-out counts from
     the same query the send uses. -> check: test for both counts.
  2. Add the batch row (subject, body, created by, status, counts) and one recipient row per
     member with a unique index on batch and member, in one migration with its Designer file.
     -> check: `dotnet ef migrations list` shows it; the snapshot is not truncated.
  3. Make `send-batch` write the rows and return 202 with the batch id. The worker sends pending
     recipient rows, marks each sent or failed, and skips sent rows on a retry or a restart.
     -> check: tests for a failed recipient, a retry, and a restart mid-batch.
  4. Add `GET api/admin/comm/batches/{id}` for status and counts. Make the send response text say
     the mail is queued only now that it is. -> check: controller test.
  5. Web: the send button opens a confirm dialog with the counts, then shows progress from the
     status route. Modal content sits in `.modal-body` and `.modal-footer`. -> check: spec; no
     mail call before confirm.
  6. Run `code-reviewer` on the migration. -> check: findings fixed or recorded.
- Contract: preview and status routes (new); `send-batch` returns 202 with a batch id. Record in
  `docs/API_CONTRACT_REGISTRY.md`.
- Tests: `CommunicationServiceTests.cs` :: `PreviewBatch_ReturnsRecipientAndOptedOutCounts`;
  `MailBatchWorkerTests.cs` :: `Worker_FailedRecipient_OthersStillSent`,
  `Worker_Retry_SendsNoDuplicate`, `Worker_RestartMidBatch_ResumesPending`;
  `CommunicationControllerTests.cs` :: `SendBatch_Returns202WithBatchId`,
  `GetBatch_ReturnsCounts`; `admin-comm.spec.ts` :: `shows counts before sending`,
  `shows progress after confirm`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~MailBatch|FullyQualifiedName~Communication"`;
  `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/admin/comm src/app/core/services/admin-comm.service.spec.ts`;
  `python scripts/ci-local.py --only contract`.
- Done when: tests pass on SQLite, the migration applies on PostgreSQL, the browser check is
  recorded, and 100.4 closes.
- Stop and ask if: the worker cannot run in the Render plan the owner uses (one instance, may
  sleep); the send-type or send-custom routes also need queuing (not in this card).

<a id="s114"></a>
**S114 · 100.7, 100.8 · M**
- Goal: each web layout has a skip link and menu toggles that report their state, and the home page
  shows what membership costs.
- Before you start: S110 done. The 100.6 web and API part was built on 2026-11-17 (migration 14a,
  `PolicyContentSeeder`, `public/policy-page`); its mobile part is S130. Migration 14a is written
  but has never run on PostgreSQL; this card runs it and changes no migration file.
- Read (only these): the three layouts in `GHCAA.Web/src/app/layouts/` (`.html` and `.ts`);
  `GHCAA.Web/src/app/layouts/admin-layout/admin-layout.spec.ts`;
  `GHCAA.Web/src/app/public/landing/sections/membership/` (all files);
  `GHCAA.Web/src/app/core/services/financial.service.ts`;
  `GHCAA.API/Controllers/FinancialsController.cs` (`fees/applicable`);
  `GHCAA.Mobile/lib/screens/auth/register_screen.dart` (first step).
- Change (only these files): the three layout `.html` and `.ts` files;
  `admin-layout.spec.ts`; `GHCAA.Web/src/app/layouts/portal-layout/portal-layout.spec.ts` (new);
  `GHCAA.Web/src/app/layouts/public-layout/public-layout.spec.ts`; the `membership` section files
  and its spec `membership.spec.ts` (new if missing); `financial.service.ts`; `docs/TODO.md`;
  ledger row 14a in section 7.
- Do not touch: the migration 14a files and `PolicyContentSeeder`; the fees API; mobile code (record what its first step shows on 100.8 instead).
- Steps:
  1. Add a skip link as the first focusable element of each layout, targeting the main content
     with `tabindex="-1"`. -> check: spec shows the first Tab lands on it and Enter moves focus.
  2. Add `aria-expanded` and `aria-controls` to each menu toggle. -> check: spec reads the value
     before and after opening.
  3. Replace the fixed fee text in the membership section with a list from the anonymous fees
     route, one call per membership type in OrgConfig, labels from OrgConfig; hide the section when
     none returns a fee. -> check: spec for a list and for none.
  4. Read the first mobile registration step and record on 100.8 whether it shows fees. -> check:
     the note names the screen.
  5. Run migration 14a on a fresh local PostgreSQL database, then rerun it (rule 5). -> check: the
     three policy rows exist once and the identity sequence is past the highest id; record the
     run on 100.6 and mark ledger row 14a.
- Contract: no API change.
- Tests: `admin-layout.spec.ts`, `portal-layout.spec.ts`, `public-layout.spec.ts` ::
  `first tab lands on the skip link`, `menu toggle reports aria-expanded`; `membership.spec.ts` ::
  `lists live fees`, `hides with no fees`.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/layouts src/app/public/landing/sections/membership`.
- Done when: specs pass, a browser check at phone and desktop width is recorded, the 14a run is
  recorded, and 100.7 and 100.8 close.
- Stop and ask if: the membership types in OrgConfig do not match the fee configuration keys, so
  one call per type cannot be built without a new anonymous list route; migration 14a fails or
  duplicates rows on PostgreSQL (do not edit it here).

<a id="s115"></a>
**S115 · 100.5 · L**
- Goal: a shared public news, event or job link shows a title and image in a link preview, the
  sitemap lists published public content, and the portal and app can share the public address.
- Before you start: S110 done. Owner, 2026-11-17 (D1): prerender at build time. The repo has no
  server rendering setup; this card adds `@angular/ssr` prerendering for public routes only. Split
  into a prerender session and a share session if the card runs long.
- Read (only these): `GHCAA.Web/angular.json`; `GHCAA.Web/package.json`; `app.routes.ts`;
  `GHCAA.Web/src/app/common/news/news.ts`; `GHCAA.Web/public/sitemap.xml`;
  `GHCAA.API/Controllers/NewsController.cs` (`GET api/news`, `GET api/news/{id:int}`);
  `page-meta.service.ts` (from S110); `GHCAA.Mobile/lib/screens/member/news_details_screen.dart`.
- Change (only these files): `angular.json`; `package.json` and `package-lock.json`;
  `GHCAA.Web/src/app/app.config.server.ts` (new); `GHCAA.Web/src/app/app.routes.server.ts` (new);
  `GHCAA.Web/scripts/prerender-routes.mjs` (new, writes the route list and the sitemap from the
  API); `app.routes.ts`; `GHCAA.Web/src/app/public/news-detail/news-detail.ts` (new) with its
  `.html` and `news-detail.spec.ts` (new); `page-meta.service.ts` and its spec;
  `GHCAA.Web/public/sitemap.xml` (removed, now generated);
  `GHCAA.Web/src/app/common/share-button/share-button.component.ts` (new) and
  `share-button.component.spec.ts` (new); `common/news/news.html`; `common/events/events.html`;
  `common/jobs/jobs.html`; `news_details_screen.dart`; `GHCAA.Mobile/lib/screens/member/event_details_screen.dart`;
  `GHCAA.Mobile/lib/screens/member/job_details_screen.dart`;
  `GHCAA.Mobile/test/share_action_test.dart` (new); `GHCAA.Tests/Controllers/NewsControllerTests.cs`;
  the deploy notes in `docs/`; `docs/TODO.md`.
- Do not touch: portal and admin routes (not prerendered); the API host.
- Steps:
  1. Add `@angular/ssr` in prerender-only mode for the static public routes. -> check:
     `npm run build` writes HTML for `/`, `/news` and `/about` with their titles.
  2. Add the public `/news/:id` page and a prerender list built from `GET api/news` at build time,
     with the API address from the build environment. -> check: the build writes one HTML file per
     public post.
  3. Extend `PageMetaService` with description and Open Graph tags per route. -> check: the
     prerendered HTML holds `og:title` and `og:image`.
  4. Generate the sitemap in the same script from the public lists. -> check: a members-only post
     is absent; a backend test pins that `GET api/news` signed out leaves it out.
  5. Add a share button (`navigator.share`, copy link as fallback) on news, events and jobs, and
     the same action on the three mobile detail screens with `share_plus`, always the public
     address. -> check: spec and widget test.
  6. Record on 100.5 and in the deploy notes that a post published after the last build has no
     preview until the next build. Paste one news link in a preview checker. -> check: the result
     is on 100.5.
- Contract: no route change; the build now needs the API address.
- Tests: `NewsControllerTests.cs` :: `GetNews_SignedOut_LeavesOutMembersOnly`;
  `news-detail.spec.ts` :: `sets title and og tags from the post`; `share-button.component.spec.ts`
  :: `uses navigator.share`, `copies the link when share is missing`; `share_action_test.dart` ::
  `share uses the public address`.
- Run: `cd GHCAA.Web; npm run type-check; npm run build; npm run test:unit -- src/app/public/news-detail src/app/common/share-button src/app/core/services/page-meta.service.spec.ts`;
  `cd GHCAA.Mobile; flutter analyze; flutter test test/share_action_test.dart --exclude-tags golden`;
  `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~NewsController"`.
- Done when: the build prerenders the public routes, the preview checker result is recorded, the
  tests pass, and 100.5 closes.
- Stop and ask if: the Render build cannot reach the API at build time; prerendering breaks a
  browser-only call in a public component that cannot be guarded with `isPlatformBrowser`.

<a id="s116"></a>
**S116 · 100.9 · M**
- Goal: every notification link points at a page that exists, the web portal and admin have a bell
  and a notifications page, and a mobile tap opens the right screen.
- Before you start: no card dependency. Sending push stays with 84.61.
- Read (only these): `GHCAA.API/Controllers/NotificationController.cs`;
  `GHCAA.API/Controllers/CommunicationController.cs` (the `/portal/notifications` link);
  `GHCAA.Infrastructure/Services/FinancialService.cs` (the `/portal/payments` and
  `/finance/history` links); `MemberService_Approval.cs` (`/portal/dashboard`);
  `GHCAA.Web/src/app/core/services/alert.service.ts`; `member/dashboard/dashboard.ts`;
  portal and admin layouts; `GHCAA.Mobile/lib/screens/member/notification_screen.dart`.
- Change (only these files): `GHCAA.Domain/Constants.cs` (`NotificationLinks`);
  `CommunicationController.cs`; `FinancialService.cs`; `MemberService_Approval.cs`;
  `GHCAA.Tests/Services/NotificationServiceTests.cs`;
  `GHCAA.Web/src/app/common/notification-bell/notification-bell.component.ts` (new) and
  `notification-bell.component.spec.ts` (new); `GHCAA.Web/src/app/member/notifications/notifications.ts` (new)
  with `.html` and `notifications.spec.ts` (new); `app.routes.ts`; `alert.service.ts` and
  `alert.service.spec.ts`; the portal and admin layout `.html` files; `dashboard.ts`;
  `notification_screen.dart`; `GHCAA.Mobile/test/notification_link_test.dart` (new);
  `docs/TODO.md`.
- Do not touch: `core/services/notification.service.ts` (toasts); the other services that pass a
  link (they keep passing strings until they need a new page).
- Steps:
  1. Add `Constants.NotificationLinks` with the four links in use and fix `/finance/history` to the
     payments page. -> check: `grep -rn "\"/finance/history\"" GHCAA.Infrastructure` prints nothing.
  2. Add the bell with the unread count from `AlertService`, in the portal and admin layouts.
     -> check: spec; count matches the list.
  3. Add `/portal/notifications` with mark read and mark all read through `AlertService`.
     -> check: spec.
  4. On mobile, map each link constant to its screen on tap after marking read; an unknown link
     stays on the list. -> check: widget test.
  5. Browser check in light and dark themes. -> check: screenshots recorded.
- Contract: no route change; link values are now constants.
- Tests: `NotificationServiceTests.cs` :: `PaymentNotification_UsesPaymentsLink`;
  `notification-bell.component.spec.ts` :: `shows the unread count`; `notifications.spec.ts` ::
  `marks one read`, `marks all read`; `notification_link_test.dart` :: `tap opens the mapped screen`,
  `unknown link stays on the list`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~NotificationService"`;
  `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/common/notification-bell src/app/member/notifications src/app/core/services/alert.service.spec.ts`;
  `cd GHCAA.Mobile; flutter analyze; flutter test test/notification_link_test.dart --exclude-tags golden`.
- Done when: tests pass, the browser check is recorded, and 100.9 closes.
- Stop and ask if: an admin has no member record, so `api/notifications` returns nothing for them
  and the admin bell has no data source.

<a id="s117"></a>
**S117 · 100.10 · L**
- Goal: a member's pass shows a QR code, admins can check in from the web and see an attendance
  list with CSV, and the event walkthrough proves check-in and its refusal.
- Before you start: S13 (84.80 clients) done, so `Cancelled` exists. Record on 100.10 that
  `EventRegistrationDto` lacks `TicketCode` and that check-in already refuses any non-Approved
  registration (checked 2026-11-20).
- Read (only these): `GHCAA.Application/DTOs/EventDto.cs` (`EventRegistrationDto`, `QrCheckInDto`);
  `GHCAA.Infrastructure/Services/EventService.cs` (`GetRegistrationsByMemberAsync`,
  `CheckInByTicketCodeAsync`); `GHCAA.API/Controllers/EventsController.cs`;
  `GHCAA.Web/src/app/common/events/events.html` (the pass); `GHCAA.Web/src/app/admin/events/`;
  `GHCAA.Mobile/lib/screens/admin/gatekeeper_screen.dart`;
  `GHCAA.Mobile/lib/screens/member/event_details_screen.dart`;
  `GHCAA.Web/tests/e2e/event-walkthrough/` (README, `event.walkthrough.ts`, `scenario.json`).
- Change (only these files): `EventDto.cs` (`TicketCode` on the member's own registration DTO only,
  and `AttendanceRowDto`); `EventService.cs`; `IEventService.cs`; `EventsController.cs`;
  `GHCAA.Tests/Services/EventServiceTests.cs`; `GHCAA.Tests/Controllers/EventsControllerTests.cs`;
  `events.html` and `events.ts`; `common/events/events.spec.ts`;
  `GHCAA.Web/src/app/admin/events/admin-event-checkin.ts` (new) with `.html` and
  `admin-event-checkin.spec.ts` (new); `app.routes.ts`; `core/services/events.service.ts`;
  `event_details_screen.dart`; `GHCAA.Mobile/lib/screens/admin/attendance_screen.dart` (new);
  `GHCAA.Mobile/lib/core/router/app_router.dart`; `GHCAA.Mobile/test/attendance_screen_test.dart` (new);
  the three event walkthrough files; `docs/API_CONTRACT_REGISTRY.md`; `docs/TODO.md`.
- Do not touch: `scenario.json` data other than the new step; `AlumniEvent`'s query filter (admin
  reads use `IgnoreQueryFilters()`).
- Steps:
  1. Return `TicketCode` from `my-registrations` only; admin lists do not change. -> check: test
     that another member's code is never returned.
  2. Add `GET api/events/admin/{id}/attendance` with name, status and checked-in time.
     -> check: test with a checked-in and an unchecked registration.
  3. Add tests that a second scan and a cancelled pass are refused. -> check: both pass with no
     service change.
  4. Web pass: a QR code of the ticket code, replacing the CSS bars. Mobile pass: the same with
     `qr_flutter`. -> check: spec and widget test.
  5. Admin web check-in page: code entry (camera scan only if a package is already present), and
     the attendance list with CSV through `app-export-buttons`. -> check: spec.
  6. Mobile admin attendance list next to the gatekeeper screen. -> check: widget test.
  7. Event walkthrough: a check-in step and a second-scan refusal probe; README "What the run
     covers" table updated. -> check: the walkthrough runs locally.
- Contract: `GET api/events/admin/{id}/attendance` (new); `TicketCode` on `my-registrations`.
  Record both in `docs/API_CONTRACT_REGISTRY.md`.
- Tests: `EventServiceTests.cs` :: `GetMyRegistrations_ReturnsOwnTicketCode`,
  `CheckIn_SecondScan_Refused`, `CheckIn_CancelledRegistration_Refused`;
  `EventsControllerTests.cs` :: `Attendance_ReturnsCheckedInTimes`, `Attendance_NonAdmin_Forbidden`;
  `admin-event-checkin.spec.ts` :: `checks in by code`, `exports attendance CSV`;
  `attendance_screen_test.dart` :: `lists checked-in members`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~EventService|FullyQualifiedName~EventsController"`;
  `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/admin/events src/app/common/events`;
  `cd GHCAA.Mobile; flutter analyze; flutter test test/attendance_screen_test.dart --exclude-tags golden`.
- Done when: tests pass, the walkthrough step passes, and 100.10 closes.
- Stop and ask if: the web needs a QR package that is not in `package.json` (name the package and
  its size first); the walkthrough cannot reach the admin check-in route locally.

<a id="s118"></a>
**S118 · 100.11 · M**
- Goal: the admin stats route returns a typed DTO with the old fields plus monthly trends and
  pending counts, the web dashboard draws them, and the status badge reflects real health.
- Before you start: no card dependency. Mobile reads the same route, so old field names stay.
- Read (only these): `GHCAA.API/Controllers/AdminController.cs` (`stats`);
  `GHCAA.Infrastructure/Services/MemberService_Search.cs` (`GetDashboardStatsAsync`);
  `GHCAA.Application/Interfaces/IMemberService.cs`; `GHCAA.API/Controllers/PendingApprovalsController.cs`
  (`admin/summary`); `GHCAA.Web/src/app/admin/dashboard/admin-dashboard.ts` and `.html`;
  `GHCAA.Web/src/app/core/services/admin.service.ts`;
  `GHCAA.Mobile/lib/features/admin/admin_service.dart`;
  `GHCAA.Mobile/lib/screens/admin/admin_dashboard_screen.dart`.
- Change (only these files): `GHCAA.Application/DTOs/AdminStatsDto.cs` (new); `IMemberService.cs`;
  `MemberService_Search.cs`; `AdminController.cs`; `GHCAA.Tests/Services/MemberServiceTests.cs`;
  `GHCAA.Tests/Controllers/AdminControllerTests.cs`; `admin-dashboard.ts`; `admin-dashboard.html`;
  `GHCAA.Web/src/app/admin/dashboard/admin-dashboard.spec.ts`; `admin.service.ts`;
  `admin_service.dart`; `admin_dashboard_screen.dart`;
  `GHCAA.Mobile/test/admin_dashboard_tiles_test.dart` (new); `docs/API_CONTRACT_REGISTRY.md`;
  `docs/TODO.md`.
- Do not touch: the pending summary route; any chart package not already in `package.json`.
- Steps:
  1. Add `AdminStatsDto` with `TotalMembers`, `Applied`, `Active`, `Inactive`, `Balance`,
     `LastUpdated` unchanged, plus new members per month and dues per month for the last 12 months,
     pending approvals by kind and upcoming events. Grouped queries must run on SQLite and
     PostgreSQL. -> check: contract test pins the old names in the JSON.
  2. A month with no data returns zero, not a gap. -> check: test.
  3. Web: simple bar charts drawn with theme tokens (no new package), a "needs attention" list
     linking to each queue, and the badge reading `/health`, red on failure. -> check: spec for
     both badge states.
  4. Mobile: the new figures as plain tiles. -> check: widget test.
- Contract: `GET api/admin/stats` gains fields; old fields unchanged. Update its registry row.
- Tests: `AdminControllerTests.cs` :: `Stats_KeepsOldFieldNames`; `MemberServiceTests.cs` ::
  `DashboardStats_EmptyMonth_ReturnsZero`; `admin-dashboard.spec.ts` ::
  `badge turns red when health fails`, `lists items needing attention`;
  `admin_dashboard_tiles_test.dart` :: `shows the new figures`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~AdminController|FullyQualifiedName~MemberServiceTests"`;
  `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/admin/dashboard`;
  `cd GHCAA.Mobile; flutter analyze; flutter test test/admin_dashboard_tiles_test.dart --exclude-tags golden`.
- Done when: tests pass, the browser check in light and dark is recorded, and 100.11 closes.
- Stop and ask if: dues per month cannot be told apart from other income in the ledger without a
  new rule; the owner wants a chart package.

<a id="s119"></a>
**S119 · 100.12 · L**
- Goal: one shared table component with sort, paging, selection and CSV, used by the admin members
  list and the member payments and giving tables, with a guard that stops new hand-built tables.
- Before you start: S18 (88.5, editable import grid) done. Decide on 100.12 whether that grid can
  serve as the shared table and record why.
- Read (only these): the 88.5 grid component; `GHCAA.Web/src/app/common/pagination/`;
  `GHCAA.Web/src/app/common/export-buttons/`; `GHCAA.Web/src/app/admin/members/admin-members.html`
  and `.ts`; `GHCAA.Web/src/app/member/payments/payments.html` and `.ts`;
  `GHCAA.Web/src/app/member/giving/giving.html` and `.ts`.
- Change (only these files): `GHCAA.Web/src/app/common/data-table/data-table.component.ts` (new)
  with its `.html`, `.scss` and `data-table.component.spec.ts` (new); `admin-members.html` and
  `.ts`; `payments.html` and `.ts`; `giving.html` and `.ts`;
  `GHCAA.Web/src/app/common/data-table/hand-built-tables.guard.spec.ts` (new); `docs/TODO.md`.
- Do not touch: the other admin lists (later sessions); API list routes (a route without paging is
  recorded as its own item).
- Steps:
  1. Build the component on the 88.5 grid if it fits, else on its own, with theme tokens.
     -> check: specs for sort, paging, select-all on the current page and export.
  2. Move the admin members list. -> check: its existing spec still passes.
  3. Move the member payments and giving tables. -> check: their specs pass.
  4. Add the guard spec that counts templates holding `data-table` outside the component and fails
     when the count rises above the number after step 3. -> check: adding one fails it.
  5. Browser check. -> check: screenshots at phone and desktop width.
- Contract: no API change.
- Tests: `data-table.component.spec.ts` :: `sorts by a column`, `pages`, `selects all on the page`,
  `exports CSV`; `hand-built-tables.guard.spec.ts` :: `hand-built table count does not rise`.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/common/data-table src/app/admin/members src/app/member/payments src/app/member/giving`.
- Done when: the specs pass and 100.12 is PARTIAL with the remaining count recorded; it closes when
  the count is zero.
- Stop and ask if: the admin payments list the tracker names is not `admin/payments-pending` (built in S11);
  the 88.5 grid fits only with changes to the import page.

<a id="s120"></a>
**S120 · 100.13 · M**
- Goal: leaving a changed form asks first, on web and mobile.
- Before you start: no card dependency.
- Read (only these): `GHCAA.Web/src/app/core/guards/auth.guard.ts` (guard style);
  `app.routes.ts`; `GHCAA.Web/src/app/member/profile/profile.ts`;
  `GHCAA.Web/src/app/public/register/register.ts`; `GHCAA.Web/src/app/admin/news/admin-news.ts`;
  `GHCAA.Web/src/app/admin/events/admin-events.ts`; `GHCAA.Web/src/app/admin/site-content/site-content.ts`;
  `GHCAA.Web/src/app/core/services/confirm-dialog.service.ts`;
  `GHCAA.Mobile/lib/screens/member/profile_edit_screen.dart`.
- Change (only these files): `GHCAA.Web/src/app/core/guards/unsaved-changes.guard.ts` (new) and
  `unsaved-changes.guard.spec.ts` (new); `GHCAA.Web/src/app/common/directives/dirty-form.ts` (new,
  the `HasUnsavedChanges` contract); `app.routes.ts`; `profile.ts`; `register.ts`;
  `admin-news.ts`; `admin-events.ts`; `site-content.ts`; `profile_edit_screen.dart`;
  `GHCAA.Mobile/test/profile_edit_screen_test.dart`; `docs/TODO.md`.
- Do not touch: other admin editors (each moves with its own later change); the browser's own
  unload prompt.
- Steps:
  1. Add the contract (`hasUnsavedChanges(): boolean`) and a `canDeactivate` guard that asks
     through `ConfirmDialogService`. -> check: spec for changed, untouched and saved forms.
  2. Apply it to the profile, registration and the three admin editors named above. -> check:
     each route has the guard.
  3. Mobile: `PopScope` on profile edit with a confirm when the form changed. -> check: widget
     test.
- Contract: no API change.
- Tests: `unsaved-changes.guard.spec.ts` :: `asks on a changed form`, `passes an untouched form`,
  `passes a saved form`; `profile_edit_screen_test.dart` :: `back asks when changed`,
  `back leaves when unchanged`.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/core/guards`;
  `cd GHCAA.Mobile; flutter analyze; flutter test test/profile_edit_screen_test.dart --exclude-tags golden`.
- Done when: tests pass and 100.13 closes with the list of covered forms; the mobile admin editors
  are recorded as left for later.
- Stop and ask if: a named editor is a dialog, not a route, so the guard cannot cover it.

<a id="s121"></a>
**S121 · 100.14 · S**
- Goal: the admin header search also finds members, events and payments, within the admin's role.
- Before you start: S88 (96.3, shared header and search) done. The search routes today:
  `GET api/admin/members?searchQuery=` and `GET api/events/admin/all`. There is no payment search
  route (S11 added none, only `GET api/financials/pending`), so record the gap on 100.14 and leave payments out.
- Read (only these): `GHCAA.Web/src/app/layouts/admin-layout/admin-layout.html` and `.ts` (the
  `app-search-bar`); `GHCAA.Web/src/app/common/search-bar/search-bar.component.ts`;
  `GHCAA.Web/src/app/core/services/admin.service.ts`; `core/services/events.service.ts`.
- Change (only these files): `admin-layout.html`; `admin-layout.ts`; `admin-layout.spec.ts`;
  `docs/TODO.md`.
- Do not touch: the API; `search-bar.component.ts` behaviour for other callers.
- Steps:
  1. After the menu matches, show member and event results from the two routes, debounced, at most
     five each, and only for a role that can open those pages. -> check: spec with a mocked API.
  2. Selecting a member opens that member. -> check: spec with a membership number.
- Contract: no API change.
- Tests: `admin-layout.spec.ts` :: `finds a member by membership number`,
  `hides member results without the role`.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/layouts/admin-layout`.
- Done when: specs pass and 100.14 closes, or stays PARTIAL with the payment route gap recorded.
- Stop and ask if: S88 moved the search into a different component than the one named here.

S122 (owner decisions) was done 2026-11-17; see the progress log. The owner answered all five; the answers are on the
closed decision item of Work Package 100 and the build cards are S123 to S129. No card for an
installable web app: the answer was no.

<a id="s123"></a>
**S123 · 100.16 · L**
- Goal: an article is members only unless its author or an admin marks it public, and visitors can
  read public approved articles.
- Before you start: S115 done (public `/news/:id` page and sitemap). Articles are `NewsPost` rows
  and `MembersOnly` already exists, defaulting to false, so approved member articles are public
  today. Ask the owner two things: (1) use the existing `MembersOnly` flag instead of a new
  `IsPublic` column (default: yes; ledger row 18 then becomes a data step only); (2) whether
  existing approved member articles become members only (default: yes, by a data migration that
  sets `MembersOnly` on rows created through the submit route, if they can be told apart).
- Read (only these): `GHCAA.Domain/Models/NewsPost.cs`; `GHCAA.Application/DTOs/NewsDto.cs`;
  `GHCAA.Infrastructure/Services/NewsService.cs` (create, update, `GetActiveNewsAsync`);
  `GHCAA.API/Controllers/NewsController.cs` (`submit`, `GET`, `GET {id}`);
  `GHCAA.Web/src/app/member/articles/articles.ts` and `.html`;
  `GHCAA.Web/src/app/admin/article-approval/article-approval.ts` and `.html`;
  `GHCAA.Mobile/lib/screens/member/submit_article_screen.dart`.
- Change (only these files): `NewsService.cs`; `NewsController.cs` (only if the submit route sets
  the default); a data migration with its `.Designer.cs` if the owner says yes to (2) (ledger
  row 18); `GHCAA.Tests/Services/NewsServiceTests.cs`;
  `GHCAA.Tests/Controllers/NewsControllerTests.cs`; `articles.ts`; `articles.html`;
  `GHCAA.Web/src/app/member/articles/articles.spec.ts` (new if missing); `article-approval.ts`;
  `article-approval.html`; `GHCAA.Web/src/app/admin/article-approval/article-approval.spec.ts` (new
  if missing); `submit_article_screen.dart`; `GHCAA.Mobile/test/submit_article_public_test.dart` (new);
  the section 7 ledger row; `docs/API_CONTRACT_REGISTRY.md`; `docs/TODO.md`.
- Do not touch: admin news posts (their `MembersOnly` stays as set); the member routes.
- Steps:
  1. Member submit defaults `MembersOnly` to true when the body leaves it out. -> check: test.
  2. The anonymous `GET api/news` and `GET api/news/{id}` return approved public articles only;
     members-only, pending and rejected are refused signed out. -> check: tests (most hold today;
     add the missing ones).
  3. Web: a "Show to visitors" switch on the author's article form with a line saying who can read
     it, and the same switch plus a public filter on the admin approval screen. -> check: specs.
  4. Mobile: the switch on the submit form. -> check: widget test.
  5. Data step per the owner's answer, then `code-reviewer` on it. -> check: rollback restores by
     content, not by id range.
- Contract: no new route; the submit default changes. Record it in `docs/API_CONTRACT_REGISTRY.md`.
- Tests: `NewsServiceTests.cs` :: `SubmitArticle_DefaultsToMembersOnly`;
  `NewsControllerTests.cs` :: `GetById_SignedOut_MembersOnly_Returns404`,
  `GetById_SignedOut_Pending_Returns404`, `GetById_SignedOut_PublicApproved_Returns200`;
  `articles.spec.ts` :: `saves the visitor switch`; `article-approval.spec.ts` ::
  `filters public articles`; `submit_article_public_test.dart` :: `visitor switch is sent`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~News"`;
  `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/member/articles src/app/admin/article-approval`;
  `cd GHCAA.Mobile; flutter analyze; flutter test test/submit_article_public_test.dart --exclude-tags golden`.
- Done when: tests pass, the public page shows a public article signed out, and 100.16 closes.
- Stop and ask if: the owner wants a separate `IsPublic` column after all; member articles cannot be
  told apart from admin news for the data step.

<a id="s124"></a>
**S124 · 100.17 · M**
- Goal: an admin can save a news post as a spotlight, the home page shows the latest three when any
  exist, and the portal and app show the label.
- Before you start: S110 done. Categories are the `ArticleCategory` enum (`Event`, `Magazine`,
  `Regular`) and `GET api/news` already filters by `articleCategory`; record that on 100.17.
- Read (only these): `GHCAA.Domain/Enums.cs` (`ArticleCategory`); how `ArticleCategory` is stored
  in `ApplicationDbContext.cs`; `GHCAA.Web/src/app/core/constants/app.constants.ts`
  (`ARTICLE_CATEGORIES`); `GHCAA.Web/src/app/admin/news/admin-news.ts`;
  `GHCAA.Web/src/app/public/landing/` (section list); `GHCAA.Mobile/lib/screens/member/news_screen.dart`.
- Change (only these files): `Enums.cs` (append `Spotlight`); `app.constants.ts`;
  `GHCAA.Web/src/app/public/landing/sections/spotlights-preview/spotlights-preview.ts` (new) with
  its `.html` and `spotlights-preview.spec.ts` (new); the landing page that lists the sections;
  `common/news/news.html`; `news_screen.dart`; `GHCAA.Mobile/test/news_category_label_test.dart` (new);
  `GHCAA.Tests/Services/NewsServiceTests.cs`; `docs/TODO.md`.
- Do not touch: the order of existing enum values (append only); the news API shape.
- Steps:
  1. Append `Spotlight` to `ArticleCategory` and to `ARTICLE_CATEGORIES`. -> check: a test saves
     and filters a spotlight; existing values keep their numbers.
  2. Home page section with the latest three spotlights signed out, each linking to `/news/:id`,
     absent when none. -> check: spec for both.
  3. Label and filter in the portal news list and the mobile news screen. -> check: spec and
     widget test.
- Contract: one new enum value on the existing news routes. Record it in
  `docs/API_CONTRACT_REGISTRY.md`.
- Tests: `NewsServiceTests.cs` :: `GetActiveNews_FiltersSpotlight`; `spotlights-preview.spec.ts`
  :: `shows the latest three`, `absent with none`; `news_category_label_test.dart` ::
  `shows the spotlight label`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~NewsService"`;
  `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/public/landing src/app/common/news`;
  `cd GHCAA.Mobile; flutter analyze; flutter test test/news_category_label_test.dart --exclude-tags golden`.
- Done when: tests pass, the browser check is recorded, and 100.17 closes.
- Stop and ask if: the enum is stored as text and a value list elsewhere (a lookup table or a check
  constraint) must change too.

<a id="s125"></a>
**S125 · 100.18 (API) · L**
- Goal: a visitor can sign up for the newsletter with double opt-in and unsubscribe, and an admin
  batch can go to confirmed subscribers only.
- Before you start: S112 (signed token) and S113 (batch rows and worker) done.
- Read (only these): `MailPreferenceService.cs` and `UnsubscribeController.cs` (S111);
  `MailBatch.cs`, `MailBatchRecipient.cs` and `MailBatchWorker.cs` (S113);
  `GHCAA.API/Services/DataRetentionSweep.cs`; `RateLimitingExtensions.cs`;
  `GHCAA.Domain/Constants.cs` (`ConfigKeys`); the OrgConfig settings model.
- Change (only these files): `GHCAA.Domain/Models/NewsletterSubscriber.cs` (new);
  `ApplicationDbContext.cs`; the migration `AddNewsletterSubscribers` with its `.Designer.cs` and
  snapshot (ledger row 19); `GHCAA.Application/Interfaces/INewsletterService.cs` (new);
  `GHCAA.Infrastructure/Services/NewsletterService.cs` (new);
  `GHCAA.API/Controllers/NewsletterController.cs` (new); `CommunicationService.cs` (audience);
  `MailBatchWorker.cs`; `DataRetentionSweep.cs` (cleanup); `Constants.cs`; the OrgConfig model
  and DTO for the cleanup days; `RateLimitingExtensions.cs`;
  `GHCAA.Tests/Services/NewsletterServiceTests.cs` (new);
  `GHCAA.Tests/Controllers/NewsletterControllerTests.cs` (new); `docs/API_CONTRACT_REGISTRY.md`;
  `docs/TODO.md`.
- Do not touch: member mail preferences; `Seed/*.json`.
- Steps:
  1. Subscriber row: email, state (pending, confirmed, unsubscribed), time and source of each
     consent step, unique email. One migration with its Designer file. -> check: migration lists.
  2. `POST api/newsletter/subscribe` (anonymous, rate limited) sends a confirm mail and gives the
     same answer for a known address. `GET api/newsletter/confirm?token=` and
     `GET api/newsletter/unsubscribe?token=` use a protector with its own purpose. -> check:
     tests.
  3. Add subscribers as a batch audience, confirmed only, with an address that is also a member
     mailed once. -> check: test.
  4. Cleanup of pending addresses after the OrgConfig number of days (default 14) in the retention
     sweep. -> check: test.
  5. `code-reviewer` on the migration and `security-reviewer` on the routes. -> check: findings
     fixed or recorded.
- Contract: three anonymous newsletter routes (new); the batch preview counts subscribers. Record
  in `docs/API_CONTRACT_REGISTRY.md`.
- Tests: `NewsletterServiceTests.cs` :: `Unconfirmed_GetsNoNewsletter`, `Confirmed_GetsNewsletter`,
  `Unsubscribed_GetsNoNewsletter`, `SharedAddress_MailedOnce`, `Cleanup_DeletesOldPending`;
  `NewsletterControllerTests.cs` :: `Subscribe_KnownAddress_SameAnswer`,
  `Confirm_ChangedToken_Returns400`, `Subscribe_HasRateLimitPolicy`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~Newsletter"`;
  `python scripts/ci-local.py --only contract`.
- Done when: tests pass on SQLite, the migration applies on PostgreSQL, both reviews are done, and
  100.18 carries the API part as built.
- Stop and ask if: the confirm mail needs a new template row (that is data, owner's call); the
  OrgConfig settings have no place for the cleanup days.

<a id="s126"></a>
**S126 · 100.18 (web) · M**
- Goal: visitors sign up from the footer and land on confirm and unsubscribe pages; admins see the
  list and can send to it.
- Before you start: S125 done.
- Read (only these): `GHCAA.Web/src/app/common/footer/footer.ts` and `footer.html`;
  `admin-comm.ts` and `admin-comm.html`; `app.routes.ts`;
  `GHCAA.Web/src/app/core/constants/app.constants.ts` (`API_ENDPOINTS`); the privacy page text key in site content.
- Change (only these files): `footer.ts`; `footer.html`; `GHCAA.Web/src/app/common/footer/footer.spec.ts` (new);
  `GHCAA.Web/src/app/public/newsletter/newsletter-confirm.ts` (new) with `.html`
  and `newsletter-confirm.spec.ts` (new); `GHCAA.Web/src/app/admin/newsletter/admin-newsletter.ts` (new)
  with `.html` and `admin-newsletter.spec.ts` (new); `app.routes.ts`; `app.constants.ts` (`API_ENDPOINTS`);
  `admin-comm.ts`; `admin-comm.html`; `admin-comm.spec.ts`; `GHCAA.Web/src/app/core/services/nav.service.ts` (`ADMIN_NAV_SECTIONS`);
  `docs/TODO.md`.
- Do not touch: portal and mobile (members get mail by their own preferences); the site content
  rows (the admin edits the privacy text; record the needed sentence on 100.18).
- Steps:
  1. Footer email field with the same message for any address. -> check: spec.
  2. Confirm and unsubscribe pages reading the token from the query. -> check: spec for success
     and a bad token.
  3. Admin subscriber list with state, CSV through `app-export-buttons` and delete one. -> check:
     spec.
  4. "Newsletter subscribers" audience on the batch screen, counted in the preview. -> check: spec.
- Contract: no API change.
- Tests: `footer.spec.ts` :: `shows the same message after sign-up`; `newsletter-confirm.spec.ts`
  :: `confirms`, `shows the error for a bad token`; `admin-newsletter.spec.ts` :: `lists and deletes`,
  `exports CSV`; `admin-comm.spec.ts` :: `counts subscribers in the preview`.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/common/footer src/app/public/newsletter src/app/admin/newsletter src/app/admin/comm`.
- Done when: specs pass, the browser check is recorded, and 100.18 closes once the privacy text
  names the list.
- Stop and ask if: the admin nav needs a new permission for the subscriber list.

<a id="s127"></a>
**S127 · 100.19 (API) · L**
- Goal: an admin records a refund against a payment, a second person approves it, and only an
  approved refund changes the payment and the reports.
- Before you start: S11 is done (2026-11-28). There is no general approval mechanism in the code
  (`IElectionApprovalService` is election only; `PendingApprovalsController` only counts queues),
  so record that on 100.19 and ask the owner: (a) a refund-only approve and reject route with the
  recorder barred, counted in the pending summary (default), or (b) wait for S73 (92.2, approval
  policies).
- Read (only these): `GHCAA.Domain/Models/PaymentHistory.cs`; `GHCAA.Domain/Enums.cs`
  (`PaymentStatus.Refunded`, `FinancialCategory.Refund`); `GHCAA.Infrastructure/Services/FinancialService.cs`;
  `GHCAA.API/Controllers/FinancialsController.cs`; `PendingApprovalsController.cs`; the audit
  writer used by `FinancialService`; the dues report query.
- Change (only these files): `GHCAA.Domain/Models/Refund.cs` (new); `ApplicationDbContext.cs`;
  the migration `AddRefunds` with its `.Designer.cs` and snapshot (ledger row 14b);
  `GHCAA.Application/DTOs/RefundDto.cs` (new); `IFinancialService.cs`; `FinancialService.cs`;
  `FinancialsController.cs`; `PendingApprovalsController.cs`; the dues report query file;
  `GHCAA.Tests/Services/RefundTests.cs` (new); `GHCAA.Tests/Controllers/FinancialsControllerTests.cs`;
  `docs/API_CONTRACT_REGISTRY.md`; `docs/TODO.md`.
- Do not touch: payment gateways (no gateway call); `Seed/*.json`.
- Steps:
  1. Refund row: payment, amount, reason, date returned, recorded by, approved by, state
     (requested, approved, rejected). One migration with its Designer file. -> check: migration
     lists.
  2. `POST api/financials/payments/{id}/refunds` refuses an amount over what is left. -> check:
     test.
  3. Approve and reject routes per the owner's answer; the recorder cannot approve. -> check:
     test.
  4. Only an approved refund sets `Refunded` (full) or reduces the net (part) and shows in the dues
     report. Each step writes an audit row. -> check: tests.
  5. `code-reviewer` on the migration. -> check: findings fixed or recorded.
- Contract: refund record, approve and reject routes (new). Record in
  `docs/API_CONTRACT_REGISTRY.md`.
- Tests: `RefundTests.cs` :: `FullRefund_SetsRefunded`, `PartRefund_NetsAmount`,
  `AmountOverRemainder_Refused`, `SelfApproval_Refused`, `Rejected_LeavesPaymentUnchanged`,
  `EachStep_WritesAudit`, `DuesReport_NetsApprovedRefund`.
- Run: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~Refund|FullyQualifiedName~FinancialsController"`;
  `python scripts/ci-local.py --only contract`.
- Done when: tests pass on SQLite, the migration applies on PostgreSQL, and 100.19 carries the API
  part as built.
- Stop and ask if: the owner picks (b); the treasurer role does not exist as a role or permission.

<a id="s128"></a>
**S128 · 100.19 (clients) · M**
- Goal: admins record and approve refunds on web, members see refunds in their history on web and
  mobile, and mobile admin can approve from its approval queue.
- Before you start: S127 done.
- Read (only these): the admin payment review page S11 built; `GHCAA.Web/src/app/member/payments/payments.html`
  and `.ts`; `GHCAA.Web/src/app/core/services/financial.service.ts`;
  `GHCAA.Mobile/lib/screens/member/financial_portal_screen.dart`;
  `GHCAA.Mobile/lib/screens/admin/approval_queue_screen.dart`.
- Change (only these files): the admin payment review page files; its spec; `payments.html`;
  `payments.ts`; `GHCAA.Web/src/app/member/payments/payments.spec.ts`; `financial.service.ts`;
  `financial.service.spec.ts`; `financial_portal_screen.dart`; `approval_queue_screen.dart`;
  `GHCAA.Mobile/test/refund_display_test.dart` (new); `docs/TODO.md`.
- Do not touch: the API.
- Steps:
  1. "Record refund" on the admin payment detail, with amount and reason. -> check: spec.
  2. The request shows in the approval queue with approve and reject; the recorder sees it without
     the buttons. -> check: spec.
  3. Member payment history on web and mobile shows the refund amount and date. -> check: spec and
     widget test.
  4. Mobile approval queue lists refund requests with approve and reject. -> check: widget test.
- Contract: no API change.
- Tests: admin payment page spec :: `records a refund`, `recorder cannot approve`;
  `payments.spec.ts` :: `shows a refund`; `refund_display_test.dart` :: `history shows the refund`,
  `queue approves a refund`.
- Run: `cd GHCAA.Web; npm run type-check; npm run test:unit -- src/app/member/payments src/app/core/services/financial.service.spec.ts`;
  `cd GHCAA.Mobile; flutter analyze; flutter test test/refund_display_test.dart --exclude-tags golden`.
- Done when: tests pass, the browser check is recorded, and 100.19 closes.
- Stop and ask if: S11 put the payment review somewhere this card does not name.

<a id="s129"></a>
**S129 · 100.20 · M**
- Goal: a tapped public news, event or job link opens the app on Android, and an unknown path opens
  the browser.
- Before you start: S115 done. Ask the owner for the SHA-256 fingerprint of the release signing key
  and the package name; never guess either.
- Read (only these): `GHCAA.Mobile/android/app/src/main/AndroidManifest.xml`;
  `GHCAA.Mobile/android/app/build.gradle` (or `.kts`); `GHCAA.Mobile/lib/core/router/app_router.dart`;
  `GHCAA.Mobile/test/router_test.dart`.
- Change (only these files): `AndroidManifest.xml`; the Gradle build file (host placeholder from the
  build environment); `app_router.dart`; `router_test.dart`;
  `GHCAA.Web/public/.well-known/assetlinks.json` (new); `docs/TODO.md`.
- Do not touch: iOS; the launcher intent filter.
- Steps:
  1. Add an `autoVerify` intent filter for `/news`, `/events` and `/jobs` paths with the host from a
     manifest placeholder set by the build. -> check: the merged manifest holds the preprod host.
  2. Map those paths to `/news/:id`, `/events/:id`, `/jobs/:id`; send any other path to the
     browser. -> check: router test.
  3. Write `assetlinks.json` with the owner's package name and fingerprint. -> check: served at
     `/.well-known/assetlinks.json` locally.
  4. Owner checks on a phone with the preprod build and `adb shell pm get-app-links`. Record the
     steps to repeat for the production domain on 100.20. -> check: the note is there.
- Contract: no API change.
- Tests: `router_test.dart` :: `news link opens the post`, `unknown path goes to the browser`.
- Run: `cd GHCAA.Mobile; flutter analyze; flutter test test/router_test.dart --exclude-tags golden`.
- Done when: the router test passes, the owner's phone check is recorded, and 100.20 closes for
  preprod.
- Stop and ask if: the web server does not serve dot-folders from `public/` (check before writing);
  the owner has no release keystore yet.

<a id="s130"></a>
**S130 · 100.6 (mobile) · S**
- Goal: the mobile registration consent line and the drawer open the privacy, terms and FAQ
  content.
- Before you start: S86 (84.69, site content on mobile) and S114 done.
- Read (only these): `GHCAA.Mobile/lib/screens/auth/register_screen.dart` (consent line near line
  552); `GHCAA.Mobile/lib/core/widgets/app_drawer.dart`; the site content screen S86 added;
  `app_router.dart`.
- Change (only these files): `register_screen.dart`; `app_drawer.dart`;
  `GHCAA.Mobile/test/policy_links_test.dart` (new); `docs/TODO.md`.
- Do not touch: the S86 screen beyond passing it a key; the consent rule on step advance.
- Steps:
  1. Make "privacy" and "terms" in the consent line tappable, opening the S86 screen for the
     matching key. -> check: widget test.
  2. Add Privacy, Terms and FAQ to the drawer. -> check: widget test.
  3. An empty page shows "Not published yet". -> check: widget test.
- Contract: no API change.
- Tests: `policy_links_test.dart` :: `consent line opens privacy`, `drawer opens FAQ`,
  `empty page shows not published`.
- Run: `cd GHCAA.Mobile; flutter analyze; flutter test test/policy_links_test.dart --exclude-tags golden`.
- Done when: tests pass and 100.6 closes, with the association's review of the text still noted.
- Stop and ask if: S86 did not add a screen that takes a content key.

### Track H. Book, on the owner's word

The book changes only here, and only when the owner names the card. Each card ends with
`python docs/book/build/build.py --pdf --strict` clean, apart from the placeholders the card says
stay open. Read `docs/book/README.md` (house style, IEEE rules, A4 figure rules) before the first
chapter edit of a sitting; `.claude/rules/book.md` holds the same rules in short.

Names in these cards were checked against the tree on 2026-11-21. What the older text got wrong or
left out:

- The ASVS walkthrough is §8.11, OWASP ASVS conformance and residual risks, not §8.13 as 78.10 says.
- The traceability matrix is the matrix table in §3.6, prioritisation and traceability. §3.9 is the Spec
  Kit section; 73.5's "§3.9 claim" is the claim that the matrix is generated, now in §3.6.
- The strict build on 2026-11-21 (`build.py --strict --no-folios`) reported six count drifts:
  384 commits quoted in chapters 4, 7 and 11 against 408 in the tree, and 99 work packages quoted
  in chapters 4 and 11 against 100. It also listed 45 open placeholders (chapter 7: 6, 9: 16,
  10: 5, 11: 1, 12: 14, 13: 3) and 15 uncited references: 19, 20, 22, 31, 38, 39, 40, 42, 43,
  44, 59, 60, 62, 63 and 68.
- Chapters 7 and 9 to 13 hold no diagram. The outline lists 24 figures for chapter 11, 8 for
  chapter 7, and 20, 10, 10 and 2 artefacts for chapters 9, 10, 12 and 13. All of them would not
  fit the 80 to 90 pages the owner set on 2026-11-16, so H04 asks which to draw first.
- No tool in the repo measures latency, complexity, coupling or maintainability index. Backend
  coverage files exist under the test project's TestResults folder (Cobertura, from
  `coverlet.collector`); the web client has `npm run test:coverage`.
- `docs/RECOVERY_RUNBOOK.md` states that no database backup or restore is documented, which is what
  chapter 10 says.

**The H00 list.** Book facts changed by feature sessions. A card that changes a number or claim the
book quotes adds one line here (rule 13 in section 2, the book rule) and leaves the book alone.

- 92.5: the role matrix in §8.4, the authorisation model, with its role figure and role table, follows
  the role changes S75 makes.
- 73.5: once S82 lands, the claim that the traceability matrix is generated can be restored in §3.6,
  the matrix table, and closed from evidence in §12.2, functional evaluation.
- Meetings (spec 026): the book does not mention meetings; S41 adds them, so chapters 3, 6 and 7
  each need one line where the modules are listed.
- 99.8 (gate G-TOOLS): whatever the owner removes, the book must not name a removed tool.
- Work Package 100 (added 2026-11-17) changes the tracker counts quoted in chapter 11; H02 takes them.
- Every repository count the book quotes; H02 takes them.

<a id="h00"></a>
**H00 · book facts list · S**
- Goal: every line of the H00 list above is true in the book, or is moved to the card that owns it.
- Before you start: the owner names this card. The sessions that added a line are closed; check with
  `python scripts/backlog_inventory.py --check docs/MASTER_EXECUTION_PLAN.md`.
- Read (only these): the H00 list above; for each line, the section it names in
  `docs/book/03-requirements.md`, `docs/book/06-architecture.md`, `docs/book/07-implementation.md`,
  `docs/book/08-security.md` or `docs/book/12-results.md`; the closing tracker entry in
  `docs/TODO.md` or `docs/TODO_ARCHIVE.md`.
- Change (only these files): the chapter files named by the list lines; `docs/DOCUMENTATION_BOOK_OUTLINE.md`
  (only when a section or artefact changes); `docs/TODO.md`; `docs/MASTER_EXECUTION_PLAN.md`.
- Do not touch: repository counts (H02); placeholders (H05 to H07); the DC-15 paragraph (H03).
- Steps:
  1. For each line, find the closing evidence in the tracker. A line whose session is still open
     stays on the list. -> check: each line is marked "ready" or "waits on Snn" in a note.
  2. Edit the book for each ready line, quoting only what the evidence shows. -> check: the
     sentence names the same roles, routes or modules as the closing entry.
  3. Remove the lines done from the H00 list. -> check: the list holds only waiting lines.
  4. Build. -> check: `build.py --pdf --strict` shows no new finding beyond the count drifts and
     placeholders listed above.
- Contract: none.
- Tests: none; the strict build and `python docs/book/build/prose.py refs` are the checks.
- Run: `python docs/book/build/prose.py refs </dev/null`; `python docs/book/build/build.py --pdf --strict </dev/null`.
- Done when: the list holds only lines whose session is open, and each book edit names its tracker
  item in a tracker note.
- Stop and ask if: a line's evidence conflicts with what the chapter argues (default: leave the
  chapter, note the conflict on the item, ask).

<a id="h01"></a>
**H01 · 78.10 · M**
- Goal: one measurement record holds coverage, static analysis, latency and the ASVS walkthrough,
  each with its command, date and result, so H05 to H07 can quote it.
- Before you start: no card dependency. `dotnet --version` and `python --version` work; free space on
  C: is over 5 GB.
- Read (only these): `docs/book/04-methodology.md` §4.5.3, the performance evaluation (the latency
  method); `docs/book/08-security.md` §8.11 and its ASVS mapping table;
  `docs/book/09-verification.md` lines with `*[To be measured`; `GHCAA.Web/vitest.config.ts`;
  `GHCAA.Tests/GHCAA.Tests.csproj`; `docs/RENDER_DEPLOYMENT.md` (the preprod host).
- Change (only these files): `docs/book/measurements.md` (new); `scripts/measure_latency.py` (new);
  `scripts/test_measure_latency.py` (new); `docs/TODO.md`.
- Do not touch: chapter files (H05 to H07 quote the record); project files and package lists (a
  static analysis tool runs outside the repo).
- Steps:
  1. Coverage: run `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --collect:"XPlat Code Coverage" -o GHCAA.Tests/bin/alt-out`
     and `cd GHCAA.Web; npm run test:coverage`. Record line and branch coverage per layer.
     -> check: the record has both numbers, the command and the date.
  2. Static analysis: ask the owner for the tool (default below). Run it outside the repo against
     the four backend projects; record cyclomatic complexity per method (distribution and top ten),
     class coupling, and maintainability index per project. A metric the tool does not give is
     written "not measured" with the reason. -> check: each chapter 9 "To be measured" line has a
     value or a reason.
  3. Write `scripts/measure_latency.py`: GET each listed public endpoint N times in sequence, print
     the 95th percentile per endpoint. Endpoints, N and host are arguments; no secrets. -> check:
     `python -m unittest scripts/test_measure_latency.py` passes.
  4. Run it against preprod with the endpoints named by §4.5.3 (directory and event listings). Wake
     the host first and drop the first request. -> check: the record has p95 per endpoint, N, host
     tier and date.
  5. ASVS: walk each ASVS 4.0.3 control in the §8.11 mapping table and mark met, not met or
     not applicable, with the file or test that shows it. -> check: counts by chapter add up to the rows in that table.
  6. Note on 78.10 what each figure is and where it lives. -> check: tracker entry names the record.
- Contract: none.
- Tests: `scripts/test_measure_latency.py` :: `test_p95_of_known_list`, `test_single_sample`,
  `test_empty_list_raises`, `test_rejects_non_http_host`.
- Run: `python -m unittest scripts/test_measure_latency.py </dev/null`; then the commands in steps 1 to 4.
- Done when: the record holds all four sets with commands and dates, and 78.10 is DONE with the
  record as evidence. Test totals stay a test result, not a claim about real use.
- Stop and ask if: the static analysis tool needs a package added to a project (default: run
  `Microsoft.CodeAnalysis.Metrics` from a scratch copy outside the repo, or `lizard` from a scratch
  virtual environment); preprod is down or the host tier changed (default: record what was
  measured and say the run is partial).

<a id="h02"></a>
**H02 · 63.8, 84.35 · S**
- Goal: every repository count the book quotes matches the tree on one stated date, and the strict
  build reports no count drift.
- Before you start: H01 closed (test totals come from the same day's runs). Run
  `python docs/book/build/build.py --strict --no-folios </dev/null` and note the drift lines.
- Read (only these): `docs/book/README.md` under "Keeping the numbers true"; `docs/PROJECT_MAP.md`
  (the count commands); the output of `python docs/book/build/wbs.py --markdown </dev/null`; the
  Chapter 11 block of `docs/DOCUMENTATION_BOOK_OUTLINE.md`.
- Change (only these files): `docs/book/04-methodology.md`; `docs/book/06-architecture.md`;
  `docs/book/07-implementation.md`; `docs/book/11-project-management.md`; `docs/book/README.md`;
  `docs/DOCUMENTATION_BOOK_OUTLINE.md`; `docs/book/build/wbs.py` (only the component-to-area map);
  `docs/TODO.md`.
- Do not touch: chapter 5's analysis-time count of seventy-two `DbSet` properties (a figure for that
  stage); placeholders.
- Steps:
  1. Run `python docs/book/build/wbs.py --check </dev/null`; map any unmapped area. -> check: exit 0.
  2. Re-take each README count with its command (`DbSet`, configurations, enumerations, endpoint
     attributes, controllers, interfaces, services, migrations, `styles.scss` lines) and the test
     totals from `python scripts/ci-local.py`. -> check: the README paragraph carries the new
     figures and today's date.
  3. Run `wbs.py --markdown` once and copy its figures into chapter 11 and the outline's Chapter 11
     block (work packages, tasks, reactive share, commit-days, critical path, line counts, effort).
     -> check: chapter 11 and the outline quote the same run date.
  4. Fix the commit and work package counts in chapters 4, 7 and 11, and chapter 6's set from step 2.
     -> check: the strict build lists no "repository counts" line.
- Contract: none.
- Tests: `python -m unittest discover -s docs/book/build -p test_wbs.py` stays green.
- Run: `python docs/book/build/wbs.py --check </dev/null`; `python docs/book/build/build.py --pdf --strict </dev/null`.
- Done when: no count drift; 84.35 DONE with the build line as evidence; 63.8 stays TODO with a note
  of today's re-take, since it is re-taken again just before submission.
- Stop and ask if: a count moves enough to change an argument in the text (default: change the
  number only and ask about the sentence).

<a id="h03"></a>
**H03 · 84.96, 64.8 · S**
- Goal: chapter 3's DC-15 paragraph agrees with §8.8 and §12.6, and §4.7 carries the 1 to 5 impact
  scale and RE = P × C in the §4.7 risk table.
- Before you start: no card dependency. `grep -n "does nothing else" docs/book/03-requirements.md`
  shows the paragraph.
- Read (only these): `docs/book/03-requirements.md` (the DC-15 row and the Authority-class paragraph
  after it); `docs/book/08-security.md` §8.8, governance integrity; `docs/book/12-results.md` §12.6,
  where the research questions are answered; `docs/book/04-methodology.md` §4.7 and its risk table;
  `docs/book/11-project-management.md` §11.9, the project risk register (RE = P × C already used).
- Change (only these files): `docs/book/03-requirements.md`; `docs/book/04-methodology.md`;
  `docs/TODO.md`.
- Do not touch: the DC-15 table row itself, which already says the conduct of a ballot rests with
  the Election Commission; §11.9.
- Steps:
  1. Ask the owner how the "does nothing else" claim is restated (84.96 says the owner decides).
     Offer the default: the platform runs the ballot mechanics the Commission directs; authority
     over conduct and result stays with the Commission. -> check: the answer is noted on 84.96.
  2. Rewrite only that paragraph to the answer. -> check: the paragraph and §8.8 name the same
     boundary; `prose.py refs` reports no broken reference.
  3. Add an impact column on the 1 to 5 scale and an RE column to the §4.7 risk table, using the 1 to 5
     values and the method of §11.9. -> check: each row's RE equals P × C.
  4. Build. -> check: strict build clean of new findings.
- Contract: none.
- Tests: none; strict build and `prose.py refs`.
- Run: `python docs/book/build/prose.py refs </dev/null`; `python docs/book/build/build.py --pdf --strict </dev/null`.
- Done when: 84.96 DONE with the owner's wording date; 64.8 DONE with the §4.7 risk table as evidence.
- Stop and ask if: a §4.7 risk has no probability in §11.9 to reuse (default: ask for it; do
  not invent one).

<a id="h04"></a>
**H04 · 64.9, 67.3 · M**
- Goal: the figure list for chapters 7 and 9 to 13 is cut to what fits the page target, and the
  chapter 11 figures that survive the cut are drawn and print at 7pt or larger.
- Before you start: no card dependency. `python docs/book/build/build.py --audit </dev/null` runs.
- Read (only these): the figure lists for chapters 7 and 9 to 13 in
  `docs/DOCUMENTATION_BOOK_OUTLINE.md` and its page budget block; `docs/book/README.md` (A4 figure
  rules and the fix order for a diagram that will not print); `docs/book/11-project-management.md`;
  the output of `python docs/book/build/wbs.py --markdown </dev/null`.
- Change (only these files): `docs/book/11-project-management.md`;
  `docs/DOCUMENTATION_BOOK_OUTLINE.md`; `docs/TODO.md`.
- Do not touch: chapters 7, 9, 10, 12 and 13 (their figures are drawn in H05 to H07 from the list
  this card fixes); diagrams in chapters 1 to 6 and 8.
- Steps:
  1. Count the figures and tables listed for chapters 7 and 9 to 13 and measure current pages with
     `python docs/book/build/pages.py </dev/null`. Ask the owner which to keep, with the default
     below. -> check: the outline lists only the kept artefacts and names the reason for each cut.
  2. Draw each kept chapter 11 diagram as mermaid, left to right, one-line box labels, from the
     `wbs.py` output. -> check: `build.py --audit` reports none of them as too small.
  3. Name every new figure in the body text and run `python docs/book/build/renumber.py --apply`.
     -> check: strict build reports no figure the body never names.
- Contract: none.
- Tests: none; `build.py --audit` and the strict build.
- Run: `python docs/book/build/build.py --audit </dev/null`; `python docs/book/build/build.py --pdf --strict </dev/null`.
- Done when: 64.9 DONE; 67.3 PARTIAL with the chapters still to draw named (H05 to H07), or DONE if
  the owner cut every other figure.
- Stop and ask if: always at step 1. Default: keep the seventeen per-component diagrams (owner,
  2026-09-27) and the critical-path graph, and set the rest as tables or drop them; then any
  diagram that will not print after the README fix order (default: set it as a table).

<a id="h05"></a>
**H05 · 67.2 (chapters 7, 10) · M**
- Goal: chapters 7 and 10 hold no open placeholder and carry the figures H04 kept for them.
- Before you start: H01 and H04 closed. `grep -c '\*\[' docs/book/07-implementation.md docs/book/10-deployment.md`
  prints 6 and 5.
- Read (only these): `docs/book/07-implementation.md` (the dependency table);
  `docs/book/10-deployment.md`; `docs/book/measurements.md`; `docs/RECOVERY_RUNBOOK.md`;
  `docs/RENDER_DEPLOYMENT.md`; the chapter 7 and 10 lists in `docs/DOCUMENTATION_BOOK_OUTLINE.md`.
- Change (only these files): `docs/book/07-implementation.md`; `docs/book/10-deployment.md`;
  `docs/DOCUMENTATION_BOOK_OUTLINE.md` (only if a figure is cut); `docs/TODO.md`.
- Do not touch: the backup statement in chapter 10 unless the owner gives the Neon plan's terms;
  library versions in the dependency table (they come from the project files).
- Steps:
  1. Ask the owner, in one question set, for the six "alternative considered" answers in the dependency table
     and the chapter 10 facts: production host, Neon backup terms with the date checked, monthly
     costs, maintenance hours. Default for a missing answer: "none recorded". -> check: each answer
     is noted on 67.2 with its date.
  2. Replace each placeholder with the answer or the H01 figure. -> check: the grep prints 0 for both.
  3. Draw the kept figures for both chapters and run `renumber.py --apply`. -> check: `build.py --audit`
     passes for them.
  4. Build. -> check: strict build shows no placeholder in these files.
- Contract: none.
- Tests: none; strict build.
- Run: `python docs/book/build/build.py --pdf --strict </dev/null`.
- Done when: both chapters have no placeholder; 67.2 stays PARTIAL with chapters 9, 11, 12 and 13 left.
- Stop and ask if: the owner's backup answer contradicts the runbook (default: the runbook is
  updated in a separate tracker item and the book quotes the owner's checked fact with its date).

<a id="h06"></a>
**H06 · 67.2 (chapter 9) · M**
- Goal: chapter 9's placeholders that do not need participants are filled from tests and the H01
  record, and its kept figures are drawn.
- Before you start: H05 closed. `grep -c '\*\[' docs/book/09-verification.md` prints 16.
- Read (only these): `docs/book/09-verification.md`; `docs/book/measurements.md`;
  `docs/book/05-system-analysis.md` §5.5, the rules catalogue; test names found by
  `grep -rn "Category(\"FR-" GHCAA.Tests --include=*.cs`; `docs/book/instruments/README.md`.
- Change (only these files): `docs/book/09-verification.md`; `docs/DOCUMENTATION_BOOK_OUTLINE.md`
  (only if a figure is cut); `docs/TODO.md`.
- Do not touch: the usability and expert review placeholders (method detail and participants);
  they wait on gate G-EVAL (78.9) and stay open.
- Steps:
  1. Fill the boundary tests, decision table, state-transition counts and constitutional-rule
     counts from test files, naming each test by file and name. -> check: every named test exists.
  2. Fill metrics and coverage from the H01 record. -> check: each value matches the record.
  3. For the exploratory charters, ask the owner whether to reconstruct a list from the tracker
     (default: say they were not recorded as charters, plainly). -> check: the paragraph says which.
  4. Draw the kept figures and run `renumber.py --apply`. -> check: `build.py --audit` passes.
- Contract: none.
- Tests: none; strict build.
- Run: `python docs/book/build/build.py --pdf --strict </dev/null`.
- Done when: only the G-EVAL placeholders remain in chapter 9, each naming the gate; 67.2 stays PARTIAL.
- Stop and ask if: a counted figure needs a script that does not exist (default: count by `grep`
  and record the command in the record).

<a id="h07"></a>
**H07 · 67.2 (chapters 11 to 13), 67.4, 67.10 · M**
- Goal: chapters 11 to 13 hold no placeholder except those waiting on G-EVAL, every reference in
  `docs/book/99-references.md` is cited or removed with a reason, and 67.10's numbers pass is done.
- Before you start: H06 closed. The strict build lists 15 uncited references.
- Read (only these): `docs/book/11-project-management.md` (the hours-per-function-point placeholder);
  `docs/book/12-results.md`; `docs/book/13-conclusion.md`; `docs/book/measurements.md`;
  `docs/book/99-references.md`; the chapter 12 and 13 lists in `docs/DOCUMENTATION_BOOK_OUTLINE.md`;
  the open items in `docs/TODO.md` (for §13's ranked list).
- Change (only these files): `docs/book/11-project-management.md`; `docs/book/12-results.md`;
  `docs/book/13-conclusion.md`; `docs/book/99-references.md`; `docs/DOCUMENTATION_BOOK_OUTLINE.md`;
  `docs/TODO.md`.
- Do not touch: chapter 2's literature text; references cited elsewhere.
- Steps:
  1. Ask the owner for the hours-per-function-point source and the ranked open items for §13
     (default: no rate; the paragraph says the cost is not converted, and the list is the five P1
     items with the oldest dates). -> check: answers noted on 67.2.
  2. Fill §12.2 to §12.8 from H01, H02 and the H00 traceability line; answer each research question
     in §12.6 from those results only. -> check: no result in chapter 12 lacks a source in the
     record or an earlier chapter.
  3. Fill chapter 13's costs and list. -> check: grep prints no placeholder outside G-EVAL ones.
  4. For each uncited reference, cite it where its chapter uses it, or remove it and write the
     reason on 67.4. -> check: the strict build lists no uncited reference.
  5. Draw the kept chapter 12 and 13 figures; `renumber.py --apply`. -> check: `build.py --audit` passes.
- Contract: none.
- Tests: none; strict build and `prose.py refs`.
- Run: `python docs/book/build/prose.py refs </dev/null`; `python docs/book/build/build.py --pdf --strict </dev/null`.
- Done when: 67.4 DONE; 67.10 DONE; 67.2 DONE if no G-EVAL placeholder is left, else PARTIAL
  naming the gate.
- Stop and ask if: a research question cannot be answered from the evidence (default: say so in
  §12.6 and in the threats to validity, §12.9).

<a id="h08"></a>
**H08 · 67.7, 67.5 · M**
- Goal: the prose reads as one text at each merged heading, and the book prints in 80 to 90 pages
  from chapter 1 to the references.
- Before you start: H07 closed. `python docs/book/build/pages.py </dev/null` gives the page count.
- Read (only these): the 24 merged headings named in 67.7; the page budget block of
  `docs/DOCUMENTATION_BOOK_OUTLINE.md`; the output of `python docs/book/build/prose.py prose NN`
  for each chapter.
- Change (only these files): `docs/book/01-introduction.md` to `docs/book/13-conclusion.md`;
  `docs/DOCUMENTATION_BOOK_OUTLINE.md`; `docs/TODO.md`.
- Do not touch: figures and tables (cut only with the owner's yes); numbers taken in H01 and H02.
- Steps:
  1. At each merged heading, join the two parts into one passage. -> check: no paragraph repeats
     its neighbour; `prose.py refs` clean.
  2. Tighten chapter by chapter, worst over budget first. -> check: `pages.py` after each chapter.
  3. List each cut by name and ask the owner before making it (owner rule, 2026-09-27). -> check:
     each cut is noted on 67.5 with the answer.
- Contract: none.
- Tests: none; `pages.py` and the strict build.
- Run: `python docs/book/build/pages.py </dev/null`; `python docs/book/build/build.py --pdf --strict </dev/null`.
- Done when: 80 to 90 pages; 67.7 and 67.5 DONE.
- Stop and ask if: 90 pages cannot be reached without a cut (default: present the cut list and wait).

<a id="h09"></a>
**H09 · 63.34, 63.18, 74.3 · S**
- Goal: every chapter fits its page budget or the budget is changed with a reason; 63.18 is decided;
  wbs markers are backfilled only where real dates differ from commit dates.
- Before you start: H08 closed.
- Read (only these): the page budget block of `docs/DOCUMENTATION_BOOK_OUTLINE.md`; the output of
  `python docs/book/build/pages.py </dev/null`; the U1 to U5 rows of chapter 11;
  `docs/book/build/wbs.py` (how a `wbs:` marker is read).
- Change (only these files): `docs/DOCUMENTATION_BOOK_OUTLINE.md`; `docs/TODO.md`;
  `docs/book/11-project-management.md` (only if step 3 moves a figure).
- Do not touch: `docs/book/build/build.py` and its print templates (63.18 needs a PDF library; only
  if the owner asks).
- Steps:
  1. Compare pages per chapter to budget; change any budget that no longer fits, with the reason.
     -> check: `pages.py` shows no chapter over budget.
  2. Ask the owner about 63.18 (default: leave it open; it is worth doing only if a supervisor
     asks). -> check: answer noted on 63.18.
  3. Add `wbs:` markers for U1 to U5 areas whose real dates differ from commit dates, then re-run
     `wbs.py --markdown`; refresh chapter 11 and the outline's Chapter 11 block if a figure moved.
     -> check: `wbs.py --check` exit 0.
- Contract: none.
- Tests: `python -m unittest discover -s docs/book/build -p test_wbs.py`.
- Run: `python docs/book/build/wbs.py --check </dev/null`; `python docs/book/build/build.py --pdf --strict --no-placeholders </dev/null`.
- Done when: 63.34 and 74.3 DONE; 63.18 per the owner; the strict build passes with
  `--no-placeholders` unless a G-EVAL placeholder is still open.
- Stop and ask if: a backfilled date moves the critical path (default: record it and ask before
  changing chapter 11's argument).

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
| 9 | `AddEndReasonsAndElectionKind` | S45 | |
| 10 | `AddUserContactEmail` | S62 | |
| 11 | `AddMentorOptIn` | S65 | |
| 12 | `AddEventFeedback` | S67 | |
| 13 | `SeedForumCategories` | S69 | |
| 14 | approvals table, only if chosen in S72 | S73 | |
| 14a | `SeedPolicySiteContent` (three site content keys, data step) | S114 (run only) | Written 2026-11-17 for 100.6 with `PolicyContentSeeder` for databases built without migrations; S114 runs it on PostgreSQL |
| 14b | `AddRefunds` | S127 | |
| 15 | `AddSavedReports` | S95 | |
| 16, 17 | AddBatchCohortsAndReunions; AddMemorialEntries | S99, S101 | |
| 18 | data step on `NewsPost.MembersOnly`, only if the owner says yes (no new column) | S123 | |
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
| 2026-11-17 | 8.9 (added) | Preprod CI mobile analyze failed on 144 `material.dart` deprecation infos from a newer stable Flutter. All seven Flutter setup steps now pin the local Flutter version; the import migration is S131 in phase G | `.github/workflows/`, `docs/TODO.md`, this file |
| 2026-11-20 | 99.10 (phase E) | Phase E cards S45 to S51 in the eleven-part form, names checked against the code. Migration 9 renamed `AddEndReasonsAndElectionKind`. `backlog_inventory.py --check`: 0 problems; `test_backlog_inventory.py`: 34 of 34 OK | this file, `docs/TODO.md` |
| 2026-11-20 | 99.10 (phase F) | Phase F cards S52 to S83 in the eleven-part form, names checked against the code; S70, S71, S74 and S75 are their own cards. Old card errors fixed: S52 points at spec 002, S58 reuses `MembershipHistory`, S59 and S61 ask the owner first, S69 reuses the pin and lock fields, S79 drops two files with no reader. `backlog_inventory.py --check`: 0 problems; `test_backlog_inventory.py`: 34 of 34 OK | this file, `docs/TODO.md` |
| 2026-11-20 | 99.10 (phase G) | Phase G cards S84 to S97, S99 to S109 and S131 in the eleven-part form, names checked against the code; S97 is its own card. Old card errors fixed: S84 adds the missing `Constants.Uploads` and a per-user limiter, S86 renders plain text on mobile, S90 reuses the `AppliedDate` already sent, S93 reads `FileStorageOptions`, S103 gains its mobile screen, S104 drops part g (already done). Migration ledger rows 16 and 17 named. Card notes on nine tracker entries. `backlog_inventory.py --check`: 0 problems; `test_backlog_inventory.py`: 34 of 34 OK | this file, `docs/TODO.md` |
| 2026-11-20 | 99.10 (phase I) | Phase I cards S110 to S130 in the eleven-part form, names checked against the code; S122 stays a done note. Old card errors fixed: S111 and S113 use the real `api/admin/comm` routes and put unsubscribe at `api/unsubscribe`, S113 drops the mobile batch screen that does not exist, S117 keeps the existing Approved-only check-in rule, S118 types the anonymous stats object, S121 records the missing payment search route, S123 uses the existing `NewsPost.MembersOnly` flag instead of a new column (ledger row 18 changed), S124 extends the `ArticleCategory` enum. Spec 029 plan and tasks corrected to match. Check 0 problems; 34 of 34 script tests OK. No code change |
| 2026-11-21 | 99.10 (track H, closed) | Track H cards H00 to H09 in the eleven-part form, names checked against the book and tree. The H00 list moved into the track intro. Old text corrected: the ASVS walkthrough is §8.11 (78.10 said §8.13); the traceability claim sits in §3.6 (73.5 said §3.9). H01 gets a measurement record and a latency script; H04 asks the owner which figures fit 80 to 90 pages before any is drawn. Refreshing the chapter 11 tracker figures moved to H02, since the book changes only in track H. Check 0 problems; 34 of 34 script tests OK. No code or book change | this file, `docs/TODO.md` |
| 2026-11-21 | 99.10 (card review) | All 139 cards read against the section 3 rows, the ledger and the tree. S14 waits for S111 and migration 2a and asks the mail preference check for event reminders; After column and card dependencies agree for S02, S14, S15, H05 and S51; seven item labels synced; S114 drops 100.6 and runs migration 14a on PostgreSQL; S81's coverage file renamed out of a gitignore pattern; 100.6 set PARTIAL | this file, `docs/TODO.md` |
| 2026-11-21 | 84.100 (S01) | The load failure did not come back: `npm run test:unit` gave 130 of 130 files and 1149 of 1149 tests, twice, so no spec needed a fix. The ci-local `unit-tests` step now fails on a failed or missing "Test Files" count, even when the exit code is 0. A temporary throwing spec made `ci-local.py --only web` fail; after it was deleted the layer passed 4 of 4. `test_ci_local.py`: 6 of 6 OK. One reviewer finding (the tests overwrote the real step log) fixed | `scripts/ci-local.py`, `scripts/test_ci_local.py` (new), `docs/TODO.md`, this file |
| 2026-11-21 | 84.101, 84.99 (S02) | ci-local runs `dotnet tool restore` in `GHCAA.API`, lists the tool manifests nearest first, and names a stray one above the repo when swagger is missing; the stray `C:\Users\dotnet-tools.json` was already gone. Six migrations (the card's five and `SeedPolicySiteContent`, on the owner's word) wrap `setval` in `DO $sv$`. The idempotent script applied twice on an empty PostgreSQL database, and `Migrate()` built a second one with all 38 migrations. `docs/api/swagger.json` regenerated on the owner's word for the 37.14t print-record routes; contract layer 4 of 4. `test_ci_local.py`: 14 of 14 OK; backend layer 7 of 7 | `scripts/ci-local.py`, `scripts/test_ci_local.py`, six PgSql migrations, `docs/api/swagger.json`, `docs/TODO.md`, this file |
| 2026-11-21 | 37.16j (S03) | `RevokePrint_TwoContextsOnPostgres_OneRevokeOneAuditEntry` (NFR-R5) runs two revokes from two contexts that both read the print before either saves, on its own database on the server in `GHCAA_TEST_PG`. Local PostgreSQL 18: one done, one already recorded, one `RevokedAt`, one `FormPrintRevoked` entry, audit chain valid. Skipped without the variable. `ElectionDocumentServiceTests` 89 passed, 1 skipped without it; 90 of 90 with it. Decision written: a failed audit write rolls the claim back. No service change. Backend layer 7 of 7. `GHCAA.Infrastructure/DependencyInjection.cs` had picked up CRLF line endings in the working copy; it was put back to the LF bytes in HEAD | `GHCAA.Tests/Services/ElectionDocumentServiceTests.cs`, `docs/TODO.md`, this file |
| 2026-11-25 | 37.16k (S04) | Print verification has its own `PrintVerification` policy, its limit and window read on each request from OrgConfig through the cache (owner choice), with a fallback to the defaults if the read fails and the numbers in the partition key. A second identical filled download by the same official returns the first unrevoked record; a printer with no user id never matches. A real-pipeline test gets 429 on the third print check with a limit of 2 while the card check still answers. Reviewer findings fixed (fallback and clamp, null user, comments, the 37.14t supersede note). Targeted run 209 passed, 1 skipped; backend 7 of 7; contract 4 of 4. Known gap: two simultaneous downloads can still add two rows | `RateLimitingExtensions.cs`, `ElectionsController.cs`, `ElectionDocumentService.cs`, `OrgConfigDto.cs`, `Constants.cs`, two test files, golden OrgConfig, `election.models.ts`, registry, `CONFIG_DRIVEN_FRAMEWORK.md`, authorization catalog, `docs/TODO.md`, this file |
| 2026-11-25 | 37.16i (S05) | `GET api/elections/{id}/print-records` lists an election's prints newest first, revoked ones included, for officials with Count and ManageSetup, no step-up. The web officials view gets a Print records table with Revoke behind a reason and the confirm dialog; the reason field sits on the page below the table, since the confirm dialog takes no input and a colspan row hides the table headers. Mobile lists the same rows read only. Tests written after the code. Filtered backend run 169 passed, 1 skipped (PostgreSQL only); web specs 57 of 57; Flutter sheet test 3 of 3; browser check with a mocked API. Not yet seen on the live stack: a revoke followed by the verify page; S06 covers it | `ElectionsController.cs`, `ElectionDocumentService.cs`, `IElectionDocumentService.cs`, `ElectionDtos.cs`, three test files, `election-print-records/`, `admin-elections.*`, `elections.service.ts`, `election.models.ts`, `app.constants.ts`, `election_print_records_sheet.dart` and its test, `election_service.dart`, `election_management_screen.dart`, `app_constants.dart`, `verify-election.py`, registry, swagger, authorization catalog, `docs/TODO.md`, this file |
| 2026-11-25 | 37.14n, 37.14t (S06, steps 1 to 3) | Local walkthrough green against the real API: 17 of 17 sections, 156 steps, no findings, 3.7 minutes in demo mode; Print records 9 of 9 (51 records for 50 filled downloads, verify page issued then revoked, blank reason refused, second revoke refused, unknown id not found). Print migration already applied on `GHCAADB_v2`. The bat's wait loop fails with redirected input, so the run used its clean step and `npm run test:e2e:walkthrough` directly. Phone scan left (owner choice); both items PARTIAL and the card stays open for step 4 | `docs/TODO.md`, this file |
| 2026-11-26 | 37.14n, 37.14t (S06) | Steps 1 to 3: the local walkthrough ran green on 2026-11-25 and again on 2026-11-26, Print records section included. Step 4: the owner scanned the QR code on a filled ER-01 print with a phone on the same Wi-Fi; the verify page opened and showed the print record. Phone model and browser not given. The web server needed `ng serve --host 0.0.0.0` and an inbound rule for port 4200, since `npm run start` listens on localhost only | `docs/TODO.md`, this file |
| 2026-11-26 | 37.16m (found after S06) | The walkthrough now drives the 37.16i officials Print records table: list route matches the database (51 of 51), table shows every print, Revoke disabled with a blank reason, newest print revoked through the confirm dialog. Local run green in demo mode: 17 of 17 sections, 161 steps, no findings, 4.3 minutes | `election.walkthrough.ts`, walkthrough README, `docs/TODO.md`, this file |
| 2026-11-26 | 37.16n (owner review) | Member ballot restyled with compact seat cards and choice rows, fingerprint wraps on a phone; public officials block spaces its groups apart. Type check 0, specs 61 of 61, mock browser check dark and light, desktop and phone | `member/election/election.{html,ts,scss}`, `public/elections/elections.{html,scss}`, `docs/TODO.md`, this file |
| 2026-11-26 | 99.3 (S07) | Full `ci-local.py` run passed 17 of 17 steps across backend, contract, web and mobile, `vulnerable-packages` and `audit` included; no failures, so no new items. Owner accepted wave 1 the same day. 98.34 stays on hold with a note that its gate passed | `docs/TODO.md`, this file |
| 2026-11-26 | 84.25, 28.25 (S08) | Mentorship refusal cases now all tested: added a 401 on send without a member claim, the admin-close note limit behind the 400, and the action's own AdminOnly policy refusing a member through `AddAppAuthorization`; reviewer pass made the 400 and 403 test names match what they assert. Focused run 30 of 30, `ci-local.py --only backend` 7 of 7. 28.25 closed as covered by `config-regression.spec.ts`: 6 of 6 against the local stack, no snapshot update | `MentorshipControllerTests.cs`, `docs/TODO.md`, spec 004 `tasks.md`, this file |

| 2026-11-26 | 84.49 API part, 84.51 run (S09) | Local API run. 84.49: member import 200; profile save with NID `12345` 400 with the NID reason, then a 10-digit NID 200 and stored. The save needs the imported member's empty required fields filled first, and a first academic record for another college gives 500 yet stores the NID; noted in 84.49. 84.49 stays PARTIAL for S33 and G-DEV. 84.51: five wrong setup passwords 400, the sixth 423, unlock 204, login after unlock 200; login while locked gave 401, not 423, so 84.51 stays PARTIAL on S09 for the owner's answer | `docs/TODO.md`, this file |
| 2026-11-26 | 84.51 (S09) | Owner kept 401 for login on a locked account, so an outsider cannot tell the account is locked; the running-API lockout and unlock check passed | `docs/TODO.md`, this file |
| 2026-11-27 | 84.103 (added), S132 (added) | The profile-save 500 found in the S09 run now has its own item and card: a first academic record for another college answers 500 and the NID is still stored. Placed after S09 and run before S10 | `docs/TODO.md`, this file |
| 2026-11-27 | 84.103 (S132) | A refused profile save answers 400 with the reason and stores nothing. The NID was kept by `ErrorLogService.LogAsync`, whose save runs on the request's DbContext after `ExceptionMiddleware` catches the throw; `UpdateProfileAsync` now checks before it changes the member. Registration answered 500 and admin 409 for the same rule; both now answer 400. Nine new tests passed, fixtures 90/90, ci-local backend 7 of 7, and the member 794 check against the local API gave 400 with the NID unchanged | `GHCAA.Infrastructure/Services/MemberService*.cs`, `ProfileController.cs`, `RegistrationController.cs`, `AdminController.cs`, tests, `docs/API_CONTRACT_REGISTRY.md`, `docs/TODO.md`, this file |
| 2026-11-27 | 84.104 (added), S133 (added) | The error log save found in S132 gets its own item and card: it writes a failed request's tracked changes. The admin member edit that changes fields before the check and the mobile admin edit that hides the reason go with it. Placed after S10 | `docs/TODO.md`, this file |
| 2026-11-27 | 84.92 backend (S10) | Three AdminOnly routes: GET `financials/pending`, POST `{id}/verify` (200 `{ dueId }`, not 204, so the admin sees whether a due was settled), POST `{id}/reject` (204, reason of 3 to 500 visible characters). A payment has no due link, so the due is the single open membership due of the same member and amount (owner choice). Code review fixes: verify claims the payment and the due in one transaction, a due already linked is never relinked, and both reject 400s share the validation problem shape. `update-status` deprecated in the registry. Focused fixtures 61 of 61, ci-local backend 7 of 7 and contract 4 of 4 after the swagger snapshot was regenerated. Walkthrough step type-checks; not run, S11 runs it with the screen | `FinancialsController.cs`, `FinancialService.cs`, `FinancialDtos.cs`, `IFinancialService.cs`, `Constants.cs`, tests, membership walkthrough and README, `docs/api/swagger.json`, authorization catalog, `docs/API_CONTRACT_REGISTRY.md`, `docs/TODO.md`, this file |
| 2026-11-27 | 84.104 (S133) | The error log write clears the change tracker first, so a failed request's edits are not saved with the error row; safe while `ExceptionMiddleware` is the only caller. Admin member edit checks the Lifelong Patron and academic rules before changing the member, and records a membership type change only after the committee seat checks. Mobile admin edit shows the server's reason. Card filter 64/64, `ci-local.py --only backend` 7 of 7, `--only mobile` 2 of 2, new widget test passed | `ErrorLogService.cs`, `MemberService_Search.cs`, `admin_service.dart`, `profile_edit_screen.dart`, tests, `docs/TODO.md`, this file |
| 2026-11-28 | 84.92 clients (S11); 84.105 and 84.106 (added), S134 and S135 (added) | Admin web page `admin/payments-pending` verifies and rejects declared payments; the member sees the reason on web and mobile. Owner, 2026-11-27: the reason gets its own `PaymentHistory.RejectionReason` column, so the session also changed the API, DTO and tests and added a migration that copies older reasons from their audit rows; the card listed only client files. Membership walkthrough passed with the reason check; CI backend, web, mobile and contract all pass. Register form loading flag fixed. Archived members' pending payments (84.105) and the mobile receipt link on unpaid rows (84.106) logged | `docs/TODO.md`, this file, `docs/API_CONTRACT_REGISTRY.md`, `docs/PROJECT_MAP.md` |
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
