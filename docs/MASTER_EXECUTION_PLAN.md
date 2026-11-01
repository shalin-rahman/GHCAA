# Master execution plan

Started 2026-11-01 under Work Package 99. This file orders all open work, election and non-election.
`docs/TODO.md` and `docs/TODO_ARCHIVE.md` stay the only record of status and acceptance. This plan
names item IDs and does not copy their acceptance text. Read the tracker item before starting it.

Older plans are history and point here: `docs/NON_ELECTION_BACKLOG_EXECUTION_PLAN.md` (its eight
phases are streams N1 to N8 below), `docs/BACKLOG_EXECUTION_PLAN.md`, and
`docs/AUTH_AND_DEVICE_FIX_PLAN.md` (it keeps the Part A and B4-B5 steps, which wait for the
elections). Election work follows `docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md`.

Check this file against the tracker after any status change:

```
python scripts/backlog_inventory.py --check docs/MASTER_EXECUTION_PLAN.md
```

It exits 1 when an open item is missing here or a closed one is still scheduled. Text between the
`backlog-check: off` and `on` markers (gap report, wave 1, progress log) is history and is not checked.

## 1. Current state and next actions

- **Date:** 2026-11-02. **Wave:** wave 1 built. The owner accepted this plan on 2026-11-02 and asked for a
  code review of wave 1 and a full `ci-local.py` run before 99.3 closes. Do not start wave 2 before that.
- **Open:** 103 items on 2026-11-02, 1 of them this plan's own upkeep in Work Package 99 (`python scripts/backlog_inventory.py`). Held: 8 (`--state ONHOLD`).
- **Next three items** once wave 1 is accepted, cheapest first as in the wave 2 table: 84.4 (web
  and mobile parity table), 93.3 (portal mobile nav e2e), 34.D7 (stale mobile goldens). 84.27
  (archive API tests and admin screen) follows. See section 4, the waves.
- **Owner gates:** the owner answered every gate on 2026-11-02. What still needs the owner: the
  inbox check for preprod email (7.26), the upload-then-redeploy check for preprod uploads (7.28),
  approval of the docs archive list (12.8), the list of unused config keys (98.43), the white-label
  seeding plan (84.7) and the evaluation sessions (78.9). See section 5, the owner gates.
- **Commands:**
  - Open items: `python scripts/backlog_inventory.py [--wp N] [--state PARTIAL]`
  - Plan check: the `--check` command above.
  - All gates: `python scripts/ci-local.py` (`--only backend|web|mobile|contract`, `--quick`, `--e2e`).
  - After code changes: `graphify update .`
  - After a book chapter edit: `python docs/book/build/build.py --pdf --strict`.

<!-- backlog-check: off -->

## 2. Gap report (2026-11-01)

| ID | Finding | Evidence | Fix |
|---|---|---|---|
| G1 | The non-election plan names 5 closed items and leaves 33 open items out. | `backlog_inventory.py --check docs/NON_ELECTION_BACKLOG_EXECUTION_PLAN.md`: 38 problems. Closed: 34.D17, 84.5, 84.8, 96.15, 98.6. | That plan is now history. This file names every open item and passes `--check`. |
| G2 | Seven tracker lines use a status the parser does not read, so they are not counted as open or closed. | `[IN-PROGRESS]` with a hyphen: 7.16 and 27.10 in `TODO.md`; 20.3, 20.4, 51.4 and 51.5 in `TODO_ARCHIVE.md`. `[OPEN]`: 98.43. The parser in `docs/book/build/tracker_page.py` reads `IN PROGRESS` with a space. | Fixed 2026-11-01 (99.4). 27.10, 20.3 and 20.4 closed, since 82.86, 82.82 and 82.83 are DONE. 7.16, 51.4 and 51.5 are `[IN PROGRESS]` and back in `TODO.md`. 98.43 is `[ONHOLD]` behind the config hold. The archive now holds only DONE and WONTDO items. |
| G3 | Five wave 1 candidates from the plan were stale. | 46.5, 82.4, 44.16 and 61.3 are DONE. 51.4 sits in the archive with an unreadable status (G2). 84.46 is used twice. | Dropped from wave 1. 84.46 stays in stream E with an owner gate. |
| G4 | The spec 001 index has no row for spec 026, the meetings spec. | `docs/specs/001-platform-baseline/spec.md`, feature spec table. | Add the row with 84.19. |
| G5 | Docs written before 84.21 still name `FamilyService`. | `BACKLOG_EXECUTION_PLAN.md`, the 57.1 audit, the 001 feature catalogue, `TEST_COVERAGE_PLAN.md`. | Left alone: they are dated records. |
| G6 | The ci-local web unit failure ("reading 'config'") did not come back. | `python scripts/ci-local.py --only web` on 2026-11-01: 938 of 938 passed, exit 0. | Closed. Watch for it in later runs. |
| G7 | The Priority Index at the top of `TODO.md` matches its items. | Each single-ID index line compared with its item state on 2026-11-01: no disagreement. | None. Recheck with each wave. |
| G8 | Some PARTIAL items have only an owner step left. | 7.26, 7.28, 37.1m, 37.14o, 84.42, 84.48. | Listed as owner gates in section 5, not as engineering work. |

## Wave 1 (built 2026-11-01)

Wave 1 was meant to hold defects and PARTIAL items that code alone could close. After the stale
candidates were dropped (G3), four items were left. It was not padded with items that need owner input.

| ID | Stream | Why ready | Layers | Test evidence | Gate |
|---|---|---|---|---|---|
| 7.22 | N1 | One config value, no code path. | Mobile config | grep of the four preprod files | Mobile sign-in on preprod stays an owner check. |
| 84.21 | N2 | `FamilyService` duplicated `FamilyLinkService` search. | API, Application, Infrastructure | `FamilyLinkControllerTests`, `MemberSearchOrderTests`: 20 of 20 | None |
| 84.31 | N2 | Two spec folders numbered 010. | Docs | grep for `010-election-engine-fixes` outside history rows | None |
| 90.4 | N4 | Text named a component that does not exist. | Tracker text | Spec 010 and the non-election plan already name the 88.5 grid | None |

<!-- backlog-check: on -->

## 3. Streams

Each stream keeps the order and constraints written in the phase it comes from. Within a stream,
work in the order the tracker's `Depends on` tags give.

| Stream | Source | Items |
|---|---|---|
| E | Election delivery plan (spec 023 `delivery-plan.md`), `AUTH_AND_DEVICE_FIX_PLAN.md` Part A and B4-B5 | 37.1m, 37.14o, 37.1p, 37.16e, 37.12y, 37.13f, 37.13g, 37.14a, 37.14d, 37.14f, 37.14m, 37.14n, 37.14t, 95.2, 95.4, 95.5, 95.12, 95.6, 84.42, 84.46, 84.50 |
| N1 | Non-election plan phase 1, release and environment blockers | 7.26, 7.28, 34.D8, 34.D19, 62.41, 62.6b, 84.48 |
| N2 | Phase 2, shared contract and identity foundations | 84.4, 84.7, 84.9, 84.18, 84.23, 84.25, 84.27, 84.35, 84.39, 84.49, 84.51, 97.1 |
| N3 | Phase 3, mobile foundations and shared UX | 8.5, 8.7, 8.8, 28.25, 28.33, 37.7, 60.3, 93.3, 93.4, 96.3 |
| N4 | Phase 4, member and alumni product work | 6.2, 37.4, 37.5, 37.9, 37.10, 49.3.C, 84.19, 84.20, 88.1, 88.2, 88.3, 88.4, 88.5, 90.1, 90.2, 90.3, 90.4, 90.5, 92.1, 92.2, 92.3, 92.4, 92.5 |
| N5 | Phase 5, white-label profile and reusable modules | 62.2, 62.42, 62.56 |
| N6 | Phase 6, performance and quality | 27.1, 27.8, 34.D7, 37.11, 51.4, 51.5, 98.7 |
| N7 | Phase 7, platform governance, security and operations | 7.16, 12.8, 12.9, 73.5, 73.6, 74.3, 98.43 |
| N8 | Phase 8, dissertation evidence and evaluation | 63.8, 63.18, 63.34, 64.8, 64.9, 67.2, 67.3, 67.4, 67.5, 67.7, 78.9, 78.10 |
| P | This plan's own upkeep (Work Package 99) | 99.3 |

Order notes kept from the non-election plan:

- 88.5's shared read-only grid comes before 90.4. No shared Angular table exists, so 88.5 builds it
  and 90.4 reuses it.
- 62.2 and its prerequisites come before 62.56 and 62.42. 62.42 also waits for 62.41.
- 8.5 is a measured spike. No local database is picked before its evidence. 8.7 follows it.
- 28.33 is the same scope as 8.8 and 37.7. Do not add an ARB pipeline.
- 92.1 (spec 022) and 84.19 (spec 026) are written before their code.

## 4. Waves

A wave holds items that are ready: no owner gate, dependencies done, and the remaining step is code
or docs. Each item is checked with `graphify query` at the start, because tracker text can lag the code.

### Cost and time rules

Every wave follows these rules. They cut test runs, rework and owner round trips, which is where
most of the time goes.

1. **Check before building.** Run `graphify query` on the item first. If the code already does what
   the item asks, close it with the evidence command instead of writing it again.
2. **Cheapest first.** Order a wave docs-only, then test-only, then one-layer code, then cross-layer.
   Small items close early and a wave that runs short still ships something.
3. **Size each item.** S is one file or one test file, M is one layer, L crosses layers or needs a
   migration. An L item gets its own wave, or is split, so it does not hold up the small ones.
4. **Smallest test per item, full gates once per wave.** While working an item, run only its test
   file (`dotnet test --filter`, one Vitest file, one flutter test file). Run
   `ci-local.py --only <layer>` once per layer touched, and the full `ci-local.py` (and `--e2e`
   only when a wave has e2e work) once at the end of the wave. The full run takes long; do not
   repeat it per item.
5. **Reuse first.** Use the shared component, service, helper or script that already exists. No new
   abstraction or dependency unless two places need it now.
6. **One review per wave.** One code-reviewer pass over the whole wave, then one fix pass.
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

<!-- backlog-check: on -->

If a cost rule and the floor disagree, the floor wins.

### Wave 2 (proposed, starts after the owner reviews wave 1)

Ordered cheapest first. One `--only backend` run, one `--only web` run, one `--e2e` run and one
`--only mobile` run cover the whole wave.

| ID | Size | Stream | Why ready | Layers | Test evidence required | Gate |
|---|---|---|---|---|---|---|
| 84.4 | S | N2 | Parity table built from the routes both clients call. | Docs | Every row traced to a client file | None |
| 93.3 | S | N3 | The portal mobile nav e2e misses the cases the item lists. | Web e2e | Playwright spec passes in `ci-local.py --e2e` | None |
| 34.D7 | S | N6 | Stale mobile goldens. | Mobile | `flutter test` clean after `--update-goldens`, diffs reviewed | The owner looks at the changed images, batched with the wave report |
| 84.27 | L | N2 | `ArchiveController` has no tests and no web admin screen. Crosses API tests and web, so it runs last. | API tests, Web | New NUnit controller tests; Vitest spec for the admin page | None |

84.27 is L under rule 3, the size rule, so it runs last and may take its own wave.

### Later waves

Later waves are drawn from the streams in this order: N2, N3, N4, N6, N5, N7, N8. Stream E runs on
its own plan and waits for the election sequence. Owner-gated items enter a wave only after the
gate clears.

## 5. Owner gates

Engineering does not start these. Each stays at its tracker status until the owner records evidence.

| Gate | Items | What the owner does |
|---|---|---|
| Vulnerable packages | 98.34 (on hold) | `npm audit` in the web gate fails on undici (through `@angular/build`), `@vitest/mocker`, glob and minimatch. Bump when fixed versions work with the Angular and Vitest toolchain. Until then the web/audit gate stays red and is named in each wave report. |
Answers recorded 2026-11-02. The "Next" column is what is left, and who does it.

| Gate | Items | Answer and what is next |
|---|---|---|
| Vulnerable packages | 98.34 (on hold) | `npm audit` in the web gate fails on undici (through `@angular/build`), `@vitest/mocker`, glob and minimatch. Bump when fixed versions work with the Angular and Vitest toolchain. Until then the web/audit gate stays red and is named in each wave report. |
| Preprod services | 7.26, 7.28, 84.48 | Owner says the Render settings are in. `/health` returns 200 with all four checks Healthy, which does not prove Brevo or S3. Next (owner): a sign-in code that reaches an inbox, and a photo that still loads after a redeploy. |
| Generic profile | 62.41, 62.6b | 62.41 runs against a throwaway local PostgreSQL database, dropped afterwards. 62.6b: Render is the only host, so the GHCAA fallback in `OrgConfigService` goes. Next: engineering. |
| Local data | 34.D8, 34.D19 | Verify on the local stack. Next: engineering. |
| Device checks | 84.42, 84.50 | Run on the Flutter Windows desktop build against a local API. 37.1m and 37.14o are covered inside 84.50. Next: engineering. |
| Key material | 84.46 | The test vector key stays as is. Next: election handbook text on how the real ballot key pair is made and used. |
| Product decisions | 84.7, 49.3.C, 12.8 | 84.7: support a second institution; a seeding plan goes to the owner before any build. 49.3.C: optional `ContactEmail` on the user. 12.8: two secret files untracked; the archive list goes to the owner. |
| Participants | 78.9 | The owner runs the sessions once 7.26 and 7.28 pass. Next: a session runbook and an empty response log. |

## 6. On hold

From `python scripts/backlog_inventory.py --state ONHOLD`. Never placed in a wave.

| Items | Waiting for |
|---|---|
| 91.3, 91.4, 91.5, 91.6 | A second host (school or club) or a second consuming app. |
| 98.34 | A fixed undici that works with the Angular toolchain (owner gate above). |
| 82.31 | The data-protection decision it records. |
| 7.21 | The domain to be bought and pointed at Render. |
| 72.4 | The first Dependabot pull requests. |

## 7. Test strategy

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

## 8. Automation

| Script | When to run it |
|---|---|
| `scripts/backlog_inventory.py` | At the start of a session, and `--check` after any tracker status change. |
| `scripts/ci-local.py` | `--only <layer>` once per layer a wave touches; the full run once at the end of a wave, before handing it to the owner. `--quick` for a fast format and type check. |
| `scripts/archive_done_todos.py` | When DONE items pile up in `TODO.md`. |
| `scripts/verify-election.py` | After election changes, before a preprod release. |
| `scripts/visual-check.ps1` | After a web UI change, for screenshots. |
| `docs/book/build/tracker_page.py` | Run by the book build. Its parser is also used by `backlog_inventory.py`. |
| `docs/book/build/wbs.py --check` | After a work package is added or renumbered. |

<!-- backlog-check: off -->

## 9. Progress log

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
| 2026-11-02 | 12.8 (part) | `git ls-files` no longer lists `docs/appsettings.txt` or `docs/dgepay_info.txt`; both kept on disk and ignored | `.gitignore` |
