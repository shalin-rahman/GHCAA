# Specification and TODO alignment

**Reviewed**: 2026-09-21

This review compares the specification packages with `docs/TODO.md` and
`docs/TODO_ARCHIVE.md`. The codebase remains the authority for implemented
behavior. TODO files provide delivery status, planned scope, and historical
context; they do not turn an unimplemented item into implemented behavior.

## Status rules

- A `[DONE]` item in `TODO_ARCHIVE.md` is historical delivery evidence. The
  baseline may describe that behavior when it is also present in code and
  tests.
- An active `[TODO]`, `[IN PROGRESS]`, `[PARTIAL]`, or `[ONHOLD]` item is not
  represented as completed baseline behavior unless the code independently
  proves it.
- A specification decision can resolve a product question without completing
  its implementation. The decision and implementation status are separate.

## Baseline alignment

`001-platform-baseline` describes the current implemented platform. Its
feature catalog covers the domains delivered through the archived work,
including mobile parity, member registry, communication, events, finance,
governance, constitution, polls, security, testing, and public election
documents. Archived work must not be reintroduced as open implementation work
without evidence of a regression.

The baseline does not claim completion for active planned work, including:

- Career referrals (`6.2`).
- Mobile security and persistence hardening and localization (`7.16`,
  `8.5`, `8.7`, `8.8`).
- Coverage-threshold improvement (`27.8`).
- The election engine and other alumni-association depth features (`37.1` and
  related items).
- Remaining white-label/profile-pack work in Work Package 62.
- Evaluation/documentation work in Work Packages 75 and 78.
- Member communication history and related follow-up in Work Package 81.
- Workflow-contract documentation in Work Package 84.
- Election form generation, candidate evidence, and verification documents in
  Work Package 37.1e.

Where the baseline mentions one of these areas, it distinguishes the
implemented subset from the planned remainder. In particular, static election
documents and constitution voting are implemented; the persisted election
engine is planned.

## Workflow-contract alignment

The current status in `docs/TODO.md` is:

| Item | Tracker status | Specification status |
|---|---|---|
| 84.1 | Done within the 7-domain scope named in spec.md Story 1 (events, news/article, jobs, mentorship, payments, polls, governance) | `002` Story 1 complete for the 7 named domains |
| 84.2 | Done within the same 7-domain scope | `002` Story 2 complete for the 7 named domains |
| 84.3 | Done within the same 7-domain scope; several sub-routes flagged "not read in this pass" per evidence file | `002` Story 3 complete for the 7 named domains |
| 84.4 | TODO, depends on 84.3 (now unblocked) | `002` Story 4 remains pending |
| 84.5 | TODO, depends on 84.3 (now unblocked) | `002` Story 5 remains pending |
| 84.6 | Done: product decision accepted | Decision recorded in `002`; implementation completed under 37.1 |
| 84.7 | TODO in tracker | Decision remains open in `002` |

The 7-domain scope cap is a deliberate decision: Phases 1-3 cover the
controllers/services backing the 7 domains spec.md Story 1 names, not all 35
`GHCAA.API` controllers. See `002-workflow-contracts-and-validation/tasks.md`
T007's scope note.

The 84.6 decision and its 37.1 implementation are closed in the tracker.
The persisted engine, claim-bound identity, officer and step-up controls,
concurrency-safe voting, reusable QuestPDF forms, FR-tagged API, client
contract parity, and full test gates were verified on 2026-09-21.

## Archived work used as context

The archive records completed work that explains why the baseline includes
features such as registration/mobile parity, event waitlists and QR attendance,
payments and gateways, governance and constitution voting, polls, API aliases,
security hardening, visual testing, and public constitution/election documents.
The archive also records completed fixes and known residual gaps. Those residual
gaps remain relevant when they are still visible in active code or active TODO
items; archival status alone is not proof that every historical defect remains
or is still applicable.

## Maintenance rule

When an active TODO item changes status, update the affected specification
status or scope note. When a completed item is moved to the archive, do not
duplicate it as a new requirement; verify the code and tests, then retain it
only as implementation evidence.
