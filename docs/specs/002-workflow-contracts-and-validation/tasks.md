# Tasks: Workflow Contracts and Validation Detail

**Input**: [spec.md](./spec.md)

**Purpose**: Build the three tables scoped in spec.md from source and tests.
These tasks do not authorize application changes.

## Phase 1: State-transition tables (Story 1)

- [ ] T001 Query `graphify` for each domain's status enum and every service
  method/controller action that assigns it, before reading source.
- [ ] T002 Build the events registration-status transition table from
  `EventRegistrationStatus` and `EventService`.
- [ ] T003 Build the news/article approval transition table from the news
  approval status enum and `NewsService`.
- [ ] T004 Build the job-moderation and mentorship-request transition tables
  from their respective controllers/services.
- [ ] T005 Build the payment/financial status transition table, including the
  gateway callback orchestrator's states.
- [ ] T006 Build the poll and governance-period transition tables.
- [ ] T007 Mark any enum value or transition not reachable through a current
  endpoint as unreachable rather than omitting it.

## Phase 2: Validation matrix (Story 2)

- [ ] T008 Enumerate every DTO bound to a `[HttpPost]`, `[HttpPut]`, or
  `[HttpPatch]` action across `GHCAA.API` controllers.
- [ ] T009 For each DTO field, record the validation source: FluentValidation
  validator class, inline controller check, or database constraint.
- [ ] T010 Flag fields with no enforced rule found in code.

## Phase 3: Endpoint contract tables (Story 3)

- [ ] T011 For each controller, record route, HTTP method, request DTO,
  response DTO (with nullability of key fields), success status, documented
  failure statuses, and authorization policy/role.
- [ ] T012 Record pagination shape (offset vs cursor) per endpoint, using
  `001-platform-baseline/contracts/api-cross-layer.md` as the existing
  reference for the cursor path.

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
- [x] T018 Resolve 84.6 in `spec.md`: retain static election documents for
  the current scope and distinguish them from the persisted constitution
  amendment vote workflow.
