# Master execution plan

Started 2026-11-01 under Work Package 99. This file orders all open work, election and non-election.
`docs/TODO.md` and `docs/TODO_ARCHIVE.md` stay the only record of status and acceptance. This plan
names item IDs and does not copy their acceptance text. Read the tracker item before starting it.

This is the only open plan. The owner made it so on 2026-11-03. Every other plan file in `docs/`
and `docs/specs/` is now a closed record or a design reference. A design reference says how to
build an item, never when. Section 3 lists them under "Design references", and the order they used
to give for open items is now in section 3 too: the stream E order and the order notes.

This plan replaced `NON_ELECTION_BACKLOG_EXECUTION_PLAN.md` (its eight phases are streams N1 to N8
below) and `BACKLOG_EXECUTION_PLAN.md`. Both were removed on 2026-11-03 and stay in git history.
On the same day the open order in `docs/AUTH_AND_DEVICE_FIX_PLAN.md`, the spec 023 delivery plan
and `docs/WHITE_LABEL_PLAN.md` moved here.

Check this file against the tracker after any status change:

```
python scripts/backlog_inventory.py --check docs/MASTER_EXECUTION_PLAN.md
```

It exits 1 when an open item is missing here or a closed one is still scheduled. Text between the
`backlog-check: off` and `on` markers (gap report, wave 1, progress log) is history and is not checked.

## 1. Current state and next actions

- **Date:** 2026-11-02. **Wave:** wave 1 built. The owner accepted this plan on 2026-11-02 and asked for a
  code review of wave 1 and a full `ci-local.py` run before 99.3 closes. Do not start wave 2 before that.
- **Open:** 120 items on 2026-11-07, 1 of them this plan's own upkeep in Work Package 99 (`python scripts/backlog_inventory.py`). Held: 12 (`--state ONHOLD`).
- **Spec review, 2026-11-03.** The specs were checked against the tracker and the code. Fourteen
  verified gaps became items: 37.16f to 37.16h in stream E, and eleven Work Package 84 items in streams N2, N3
  and N6. Their priorities are proposals; the owner can change them. That review's one P1 item and
  the three P1 items the owner added from the admin sidebar work all closed on 2026-11-05;
  the progress log in section 9 lists them. The owner answered the six
  product questions from the same review on 2026-11-03; they became 84.65 to 84.69 (section 5,
  the owner gates).
- **Closed 2026-11-07.** The client option lists, poll comments, the server's 400 message on both
  clients and the stale mobile goldens; the progress log in section 9 lists them.
- **Owner order and review update, 2026-11-07.** The configured OrgConfig branding regression is
  fixed in both modes. The security review found no vulnerabilities. The web theme audit and the
  mobile label colors both closed the same day, after browser checks with default and custom
  branding in Light and Dark mode; section 9, the progress log, lists them.
- **Owner order, 2026-11-06.** In progress: 84.19 (meetings with comments) and 84.59, then the
  rest of the partial items 84.49 and 84.51 once those finish. Credential expiry and
  the web revoke and reissue screen closed on 2026-11-07 (section 9 logs them); the gap they left
  (every card view mints a new code) is 84.90 in stream N2.
  84.84 (directory search by city or country) is ordered too; the poll comments migration it
  waited on has landed. The
  admin sidebar regrouping and the spec 001 index with its gap review closed the same day; the five
  gaps raised from that review are new items in stream N4. 84.42, 84.48, 84.50 and 84.61 are
  on hold. 84.7 came off hold on 2026-11-07 for its plan only. The book strict build is the last item of this run. It fails on three counts, each already an
  item: stale repository counts (63.8, 384 commits quoted against 388 in the tree), chapters over
  their page budgets (63.34), and 93 open placeholders in the chapter items 67.2, 67.3, 67.5 and
  67.7. A placeholder closes only with a sourced number, never a guess.
- **Next work:** take the remaining Wave 2 items in the order shown in section 4: 84.4 (web and mobile parity table), 93.3 (portal
  mobile nav e2e), and 84.27 (archive API tests and admin screen).
- **Owner gates:** the owner answered every gate on 2026-11-02. What still needs the owner: the
  inbox check for preprod email (7.26), the upload-then-redeploy check for preprod uploads (7.28),
  the 4 secret or connection keys in `appsettings.json` (98.43), the white-label seeding plan
  (84.7) and the evaluation sessions (78.9). Owner answers 2026-11-07: 7.26 and 7.28 not done yet;
  the docs archive is done (files in `docs/archive/`); 98.43 partial (8 non-secret keys commented out); 84.7 gets
  its plan written, no build; 78.9 gets its session pack checked against the current app. Order:
  the 78.9 pack (checked 2026-11-07; section 9 logs it), then the 84.7 plan (written 2026-11-07 in `docs/SEED_PROFILE_PLAN.md`; waits on owner gates
  G1 to G4), then the two theme items (both closed 2026-11-07; section 9 logs them). The pack check found six gaps, 84.92 to 84.97. 84.92 (no screen to verify a declared
  payment) is P1 and blocks two Treasurer tasks. See section 5, the owner gates.
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
| G1 | The non-election plan names 5 closed items and leaves 33 open items out. | `backlog_inventory.py --check docs/NON_ELECTION_BACKLOG_EXECUTION_PLAN.md`: 38 problems. Closed: 34.D17, 84.5, 84.8, 96.15, 98.6. That file has since been deleted; to rerun the check, restore it from git history first (`git show HEAD:docs/NON_ELECTION_BACKLOG_EXECUTION_PLAN.md`). | That plan is now history. This file names every open item and passes `--check`. |
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
| E | Spec 023 delivery plan sessions F2 to F11 and N1, auth plan Part A and B4-B5. Order below. | 37.1m, 37.14o, 37.1p, 37.16e, 37.16f, 37.16g, 37.16h, 37.12y, 37.13f, 37.13g, 37.14a, 37.14d, 37.14f, 37.14m, 37.14n, 37.14t, 95.2, 95.4, 95.5, 95.12, 95.6, 84.42, 84.50 |
| N1 | Non-election plan phase 1, release and environment blockers | 7.26, 7.28, 34.D8, 62.41, 84.48 |
| N2 | Phase 2, shared contract and identity foundations | 84.4, 84.7, 84.23, 84.25, 84.27, 84.35, 84.39, 84.49, 84.51, 84.55, 84.57, 84.62, 84.90, 97.1 |
| N3 | Phase 3, mobile foundations and shared UX | 8.5, 8.7, 8.8, 28.25, 28.33, 37.7, 60.3, 84.61, 84.64, 84.67, 84.69, 93.3, 93.4, 96.3 |
| N4 | Phase 4, member and alumni product work | 6.2, 37.4, 37.5, 37.9, 37.10, 49.3.C, 84.72, 84.19, 84.20, 84.59, 84.65, 84.66, 84.80, 84.81, 84.82, 84.83, 84.84, 88.1, 88.2, 88.3, 88.4, 88.5, 90.1, 90.2, 90.3, 90.4, 90.5, 92.1, 92.2, 92.3, 92.4, 92.5, 84.92, 84.93, 84.94, 84.95 |
| N5 | Phase 5, white-label profile and reusable modules | 62.2, 62.42, 62.56 |
| N6 | Phase 6, performance and quality | 27.1, 27.8, 37.11, 51.4, 51.5, 84.98, 98.7 |
| N7 | Phase 7, platform governance, security and operations | 7.16, 12.9, 73.5, 73.6, 74.3, 98.43, 84.97, 84.99, 99.8 |
| N8 | Phase 8, dissertation evidence and evaluation | 63.8, 63.18, 63.34, 64.8, 64.9, 67.2, 67.3, 67.4, 67.5, 67.7, 78.9, 78.10, 84.96 |
| P | This plan's own upkeep (Work Package 99) | 99.3 |

Order notes kept from the non-election plan:

- 88.5's shared read-only grid comes before 90.4. No shared Angular table exists, so 88.5 builds it
  and 90.4 reuses it.
- 62.2 and its prerequisites come before 62.56 and 62.42. 62.42 also waits for 62.41.
- 8.5 is a measured spike. No local database is picked before its evidence. 8.7 follows it.
- 28.33 is the same scope as 8.8 and 37.7. Do not add an ARB pipeline.
- 92.1 (spec 022) and 84.19 (spec 026) are written before their code. The owner approved specs
  026 and 028 on 2026-11-04. 84.72 (forum admin) reuses the moderation rules of the poll comments
  (spec 028, done). 84.59 (typed DTOs) moved here from N2 on the owner's word of 2026-11-04 and
  runs after 84.19.
- 37.16f (one voting-member rule for elections and amendments) goes before 37.1p, so the new rules
  in 37.1p are written once.
- 84.61 (FCM push) waits for the owner's Firebase service key. Do not put it in a wave before then.

Order notes moved from `WHITE_LABEL_PLAN.md` on 2026-11-03:

- 62.41 is the generic run on an empty, throwaway PostgreSQL database. 62.42 makes `brand-lint`
  blocking only after 62.41 passes.
- 62.2 (one profile file set, hyphen-named) comes before 62.56 (profile export and import, spec 025).
- Nothing here adds a `TenantId` column. One deployment serves one institution (the plan's ADR-1).

### Stream E order

Moved on 2026-11-03 from the spec 023 delivery plan ("Sessions") and the auth plan ("Part A",
"B4-B5"). Only open items are kept. Rewritten as sessions on 2026-11-12 (owner). Each item's
Scope and Acceptance live in its tracker entry; this table only gives the order and a one-line
scope. Run the sessions in order.

| Session | Items | Scope in one line | Migration |
|---|---|---|---|
| 1 (on hold) | 84.50 (covers 95.6, 84.42, 37.1m, 37.14o) | Device checks on the Windows desktop build and a phone. Chrome and Edge print passed 2026-11-12; only the phone check is left. Held by the owner. | none |
| 2 | 37.14m, 37.14t, 37.14n | Release the form-fields migration to preprod; server print record with a hash and a QR code holding its id; then the browser check of the verify page. | 37.14t print record |
| 2b | 84.100, 84.101, 37.14v, 37.14w, 37.14x, 37.14u, then 37.14n | Fix what session 2 left. First make every web spec and the contract check run in CI; then one audit entry per revoke, an own rate limit and no repeat rows for prints, the ER-17 gate once the owner answers, and the revoke screen; last, the 37.14n run on the local stack with a real official and a phone scan with `PortalBaseUrl` set. | none |
| 3 | 37.16f, 37.16g, 37.16h | One voting-member rule for elections and amendments; declare through governance; moderated candidate photo. | photo approval columns |
| 4 | 37.1p (recount, consent), 37.16e, 37.14d | F2: recount and consent schema, officer conflict declarations, audit chain FK to Restrict, and the 37.12y per-election limit fields. | F2 |
| 5a | 37.14a, 37.12y | F3: key copy count and holder confirmation gate polling; polling extension with per-election limits under `OrgConfig` defaults. | none |
| 5b | 37.1p (unopposed, spoiled), 37.14f | F3: unopposed seats and spoiled counts; ten-year purge by request with its pin test. | none |
| 6 | screens for sessions 4 to 5b, 84.49 B4-B5 | F5 web and F6 mobile in separate worktrees; the mobile session also closes the key dialog on dispose and adds the 60 s key timeout. | none |
| 7 | 95.2, then N1 | Owner releases the positions migrations to preprod; then `AddCommitteePositionsAndTerms` (end reasons, `Election.Kind`, live index). | N1 |
| 8 | 95.4, 95.12, 95.5 | F10 in that order: vacant-seat query and filter; term ending sweep and end reasons; by-election. | none |
| 9 | 37.13f, 37.13g | Close doc 09 against free Neon and free Render; run the accessibility checklist after session 6. | none |

Migrations land one at a time, in session order: 37.14t, then the photo columns, then F2, then
N1. Each takes the next timestamp and builds from the current model snapshot.

### Design references

These files hold the how for open items. The tracker holds status, and this plan holds the order.
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

### Wave 2 (proposed, starts after the owner reviews wave 1)

The theme audit ran first and closed on 2026-11-07. The rest are ordered cheapest first. One `--only backend` run, one `--only web` run, one `--e2e` run and one `--only
mobile` run cover the whole wave.

| ID | Size | Stream | Why ready | Layers | Test evidence required | Gate |
|---|---|---|---|---|---|---|
| 84.4 | S | N2 | Parity table built from the routes both clients call. | Docs | Every row traced to a client file | None |
| 93.3 | S | N3 | The portal mobile nav e2e misses the cases the item lists. | Web e2e | Playwright spec passes in `ci-local.py --e2e` | None |
| 84.27 | L | N2 | `ArchiveController` has no tests and no web admin screen. Crosses API tests and web, so it runs last. | API tests, Web | New NUnit controller tests; Vitest spec for the admin page | None |

84.27 is L under rule 3, the size rule, so it runs last and may take its own wave.

### Later waves

Later waves are drawn from the streams in this order: N2, N3, N4, N6, N5, N7, N8. Stream E follows
the stream E order in section 3. Its steps 1 and 2 can join any wave; the rest waits for the
election sequence. Owner-gated items enter a wave only after the
gate clears.

## 5. Owner gates

Engineering does not start these. Each stays at its tracker status until the owner records evidence.

Answers recorded 2026-11-02. The "Next" column is what is left, and who does it.

| Gate | Items | Answer and what is next |
|---|---|---|
| Vulnerable packages | 98.34 (on hold) | `npm audit` in the web gate fails on undici (through `@angular/build`), `@vitest/mocker`, glob and minimatch. Bump when fixed versions work with the Angular and Vitest toolchain. Until then the web/audit gate stays red and is named in each wave report. |
| Preprod services | 7.26, 7.28, 84.48 | Owner says the Render settings are in. `/health` returns 200 with all four checks Healthy, which does not prove Brevo or S3. Next (owner): a sign-in code that reaches an inbox, and a photo that still loads after a redeploy. Not done as of 2026-11-07. |
| Generic profile | 62.41 | Runs against a throwaway local PostgreSQL database, dropped afterwards. Next: engineering. |
| Local data | 34.D8 | Verify on the local stack. Next: engineering. (The other item here closed on 2026-11-05.) |
| Device checks | 84.42, 84.50 | Run on the Flutter Windows desktop build against a local API. 37.1m and 37.14o are covered inside 84.50. Next: engineering. |
| Product decisions | 84.7, 49.3.C, 98.43 | 84.7: support a second institution; a seeding plan goes to the owner before any build. Off hold 2026-11-07; plan written the same day (`docs/SEED_PROFILE_PLAN.md`); next (owner): gates G1 to G4 in its section 8. 49.3.C: optional `ContactEmail` on the user. Docs archive: done 2026-11-07 (see the progress log); finished reviews, snapshots and old drafts moved to `docs/archive/`, and all three secret files are untracked and ignored. 98.43: partial 2026-11-07; the 8 non-secret unread keys are commented out; the 4 secret or connection keys (ClientId, ClientSecret, ApiKey, MySqlConnection) wait on the owner. |
| Participants | 78.9 | The owner runs the sessions once 7.26 and 7.28 pass. The runbook and an empty response log exist. Session pack checked 2026-11-07. Treasurer Tasks 1-2 wait on 84.92 and Secretary Task 5 on 84.93. |
| Spec review questions | 84.65 to 84.69 | Answered 2026-11-03. Scholarships get a web admin page (84.65). Archive submissions get a per-member cap set in Settings (84.66). Mobile gets password change (84.67) and read-only site content (84.69). The web admin gets credential revoke and reissue (closed 2026-11-07). Elections: mobile carries the voter screens (elections, nominate, vote, results) and web carries those plus the officials' screens; recorded in specs 018 and 023, no item. The low-value refactors are recorded as accepted in their specs. The `MemberService` split the review listed was already done (closed 2026-09-06; the largest part is now 483 lines). Next: engineering. |

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
