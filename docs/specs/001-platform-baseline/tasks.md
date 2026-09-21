# Tasks: GHCAA Platform Baseline

**Input**: [spec.md](./spec.md) and [plan.md](./plan.md)

**Purpose**: Verification and traceability tasks for the platform requirements.
These tasks do not authorize application changes.

## Phase 1: Source and contract evidence

- [x] T001 Read the Domain, Application, Infrastructure, API, Web, Mobile,
  and test projects that implement the platform.
- [x] T002 Query `graphify` for implementation boundaries, API consumers, and
  test relationships before source searches.
- [x] T003 Inspect the generated API/client contract surfaces produced by the
  current projects.
- [x] T004 Trace the 2026-09-17 cursor pagination change through the backend,
  Flutter directory client, Web compatibility path, and backend tests in
  `contracts/api-cross-layer.md`.
- [x] T005 Split the requirements into independently testable stories for
  membership, authentication, profiles, events, finance, networking,
  messaging, governance/CMS, assistant/support, and platform operations.
- [x] T006 Map each story to .NET, Angular, Flutter, and test surfaces without
  changing the application contract.
- [x] T007 Review the actual source layers, generated contract surfaces, and
  executable tests; record the evidence in
  `evidence/implementation-inventory.md`.
- [x] T008 Produce an endpoint and workflow-level catalog from the actual
  backend, Web, Mobile, and test code, marking unverifiable details as gaps.
- [x] T009 Compare the baseline with active and archived TODO work and record
  the implemented/planned status boundary in the 002 evidence alignment
  review.

## Phase 2: Required repository checks

- [x] T010 Run `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --configuration
  Release`.
- [x] T011 Run `cd GHCAA.Web; npm run type-check`.
- [x] T012 Run `cd GHCAA.Web; npm run test:unit`.
- [x] T013 Run `cd GHCAA.Mobile; flutter analyze`.
- [x] T014 Run `cd GHCAA.Mobile; flutter test --exclude-tags golden --reporter
  expanded`.
- [x] T015 Run the API snapshot comparison used by
  `.github/workflows/ghcaa-ci-standard.yml`.
- [x] T016 Run `graphify update .` after the specification artifacts are
  created.

## Phase 3: Maintenance rules

- [ ] T016 When a future API endpoint or DTO changes, update the Application
  contract, API mapping, Angular typed client, Flutter service, and relevant
  tests together.
- [ ] T017 When a future implementation behavior changes, update this
  baseline's affected requirement and cross-layer trace together.
- [ ] T018 Keep future feature specifications in a new numbered directory under
  `docs/specs/` and leave this baseline as the record of the existing system.

## Verification Notes

The checks in Phase 2 validate the existing codebase as well as the context in
which these specifications were written. A failure is evidence about the
current repository and must not be hidden by weakening the specification.
