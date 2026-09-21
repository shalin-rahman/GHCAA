# Tasks: Workflow Contracts and Validation Detail

**Input**: [spec.md](./spec.md)

**Purpose**: Build the three tables scoped in spec.md from source and tests.
These tasks do not authorize application changes.

## Phase 1: State-transition tables (Story 1)

- [ ] T001 Query `graphify` for each domain's status enum and every service
  method/controller action that assigns it, before reading source.
- [x] T002 Build the events registration-status transition table from
  `EventRegistrationStatus` and `EventService`. Done in
  [evidence/events-domain.md](./evidence/events-domain.md).
- [x] T003 Build the news/article approval transition table from the news
  approval status enum and `NewsService`. Done in
  [evidence/news-domain.md](./evidence/news-domain.md).
- [x] T004 Build the job-moderation and mentorship-request transition tables
  from their respective controllers/services. Done in
  [evidence/jobs-domain.md](./evidence/jobs-domain.md) and
  [evidence/mentorship-domain.md](./evidence/mentorship-domain.md).
- [x] T005 Build the payment/financial status transition table, including the
  gateway callback orchestrator's states. Done in
  [evidence/payments-domain.md](./evidence/payments-domain.md).
- [x] T006 Build the poll and governance-period transition tables. Done in
  [evidence/polls-domain.md](./evidence/polls-domain.md) and
  [evidence/governance-domain.md](./evidence/governance-domain.md).
- [x] T007 Mark any enum value or transition not reachable through a current
  endpoint as unreachable rather than omitting it. Done per-domain in each
  evidence file's transition table and "Gaps carried forward" list (e.g.
  `Draft` in jobs, `Refunded` in payments, dead-code constitution
  create/activate in governance).

  **Scope note**: Phases 1-3 cover the 7 domains named in `spec.md` Story 1
  (events, news/article, jobs, mentorship, payments, polls, governance) —
  the controllers/services backing those domains — not all 35 `GHCAA.API`
  controllers. This scope cap was a deliberate decision to keep the
  deliverable to what Story 1 actually names; controllers outside these
  7 domains are not covered by this specification pass.

## Phase 2: Validation matrix (Story 2)

- [x] T008 Enumerate every DTO bound to a `[HttpPost]`, `[HttpPut]`, or
  `[HttpPatch]` action across the 7 in-scope domains' controllers (see scope
  note above). Done per-domain in each evidence file's §1/§2.
- [x] T009 For each DTO field, record the validation source: FluentValidation
  validator class, inline controller check, or database constraint. Done
  per-domain in each evidence file's §2 validation matrix.
- [x] T010 Flag fields with no enforced rule found in code. Done per-domain;
  every evidence file's §2 marks unenforced fields "Flagged: no enforced
  rule found".

## Phase 3: Endpoint contract tables (Story 3)

- [x] T011 For each in-scope controller, record route, HTTP method, request
  DTO, response DTO (with nullability of key fields), success status,
  documented failure statuses, and authorization policy/role. Done per-domain
  in each evidence file's §3, with several sub-routes flagged "not read in
  this pass" where response-body internals weren't opened.
- [x] T012 Record pagination shape (offset vs cursor) per endpoint, using
  `001-platform-baseline/contracts/api-cross-layer.md` as the existing
  reference for the cursor path. Done per-domain: each evidence file's §3
  ends with a pagination note; all 7 domains use full-array/offset responses,
  none use cursor pagination.

## Phase 4: Follow-on catalogs (Stories 4–5, after Phases 1–3)

- [ ] T013 Cross-reference each endpoint against Web and Mobile call sites to
  build the client parity table.
- [ ] T014 Build the authorization-policy catalog and the problem-details
  error-code catalog from source and existing tests.

## Phase 5: Verification

- [ ] T015 Re-run the relevant backend, Web, and Mobile test suites referenced
  by each table to confirm no documented behavior contradicts a passing test.
- [ ] T016 Run `graphify update .` after the specification artifacts are
  created.

## Maintenance rules

- [ ] T017 When a future API endpoint or DTO changes, update the affected row
  in the relevant table from this specification in the same change.
- [x] T018 Resolve 84.6 in `spec.md`: develop the persisted online election
  engine already scoped by the repository's election-engine backlog, retain
  static election documents as supporting governance artifacts, and distinguish
  election ballots from constitution amendment voting.
- [x] T019 Compare `001` and `002` against active `docs/TODO.md` and archived
  `docs/TODO_ARCHIVE.md`; record status mismatches and the implemented/planned
  boundary in `evidence/todo-alignment.md`.
