# Specification: Workflow Contracts and Validation Detail

**Status**: Draft | **Date**: 2026-09-21 | **Depends on**: [001-platform-baseline](../001-platform-baseline/spec.md)

## Purpose and scope

The 001 baseline documents the platform at story and domain level and records,
as gaps, the places where state transitions, validation rules, and endpoint
contracts are not centralized. This specification closes those gaps by
producing three artifacts, each derived from the current source and tests,
never inferred:

1. A state-transition table for every domain with a status enum that changes
   over an entity's lifetime.
2. A validation matrix covering every write DTO across the platform.
3. An endpoint contract table covering every controller action, generated
   from the actual request/response DTOs, route attributes, and policies.

This is documentation-only work. It does not authorize source, schema,
dependency, or deployment changes, and it does not change the 001 baseline —
per `docs/specs/001-platform-baseline/tasks.md` T018, it lives in this new
numbered directory instead.

The white-label second-institution question remains out of scope here because
it is a separate product decision. The election question is resolved below.
Active and archived delivery status is reconciled in
[evidence/todo-alignment.md](./evidence/todo-alignment.md); an active TODO
item is not treated as implemented merely because it is named by a
specification.

## User stories

### Story 1 — State-transition tables (P1)

As a developer changing event registration, news/article approval, job
moderation, mentorship, or poll status, I need one table per domain listing
every valid status value, the actions that move an entity between them, who
can trigger each move, and any side effects (notifications, ledger entries,
audit rows), so I don't have to reconstruct the state machine by reading every
service method that touches the enum.

**Acceptance**: for each of the domains listed in
`implementation-feature-catalog.md`'s gap list (events, news/article approval,
jobs, mentorship, payments, polls, governance periods), a table exists giving
every enum value, the triggering action/endpoint, the required role, and the
side effects, traced to the actual enum and service code. Values or
transitions not reachable through any current endpoint are marked unreachable
rather than omitted.

### Story 2 — Validation matrix (P1)

As a developer adding a field to a registration, event, payment, or content
form, I need a single table listing every write DTO, its fields, and the
validation rule applied to each (required, format, range, cross-field,
uniqueness), so I can tell whether a rule lives in a FluentValidation
validator, a controller check, a database constraint, or nowhere.

**Acceptance**: a table exists covering every DTO accepted by a `[Http Post]`,
`[HttpPut]`, or `[HttpPatch]` action across the API, with a column recording
where its validation is enforced (validator class, controller, DB constraint)
and a column flagging fields with no enforced rule found in code.

### Story 3 — Endpoint contract tables (P1)

As a client developer (Web or Mobile) integrating a new screen, I need the
exact request DTO, response DTO, HTTP status codes, problem-details error
shape, pagination format, and authorization policy for each endpoint, so I
don't have to read the controller source to build a client call correctly.

**Acceptance**: a table exists per controller, listing route, HTTP method,
request DTO, response DTO (including nullability of key fields), success
status code, documented failure status codes, authorization policy/role, and
pagination shape where applicable (offset vs cursor, per
`contracts/api-cross-layer.md`).

### Story 4 — Client parity check (P2, follows Stories 1–3)

As a maintainer, I need to know where Web and Mobile diverge in which
endpoints they call and how they handle the same response, so client-specific
bugs are found by inspection instead of by a user report.

**Acceptance**: a table cross-references each endpoint from Story 3 against
whether Web calls it, whether Mobile calls it, and any observed difference in
how each client interprets the response (e.g. Web using offset pagination
where Mobile uses the cursor).

### Story 5 — Generic error and authorization catalog (P3, follows Stories 1–3)

As a developer, I need one place listing the authorization policy required by
each endpoint and the shape of its error responses, instead of that
information being implicit in each controller's attributes.

**Acceptance**: a table lists every `[Authorize]` policy/role combination in
use and which endpoints use it, plus a catalog of the problem-details error
codes actually returned, both derived from source and existing tests.

## Decision: election ballot workflow (84.6)

**Decision: develop a persisted online election engine, following the existing
election-engine scope already recorded in Work Package 37.1.**

The current implementation contains static election regulations, operational
manuals, ballot/counting certificates, and forms, plus persisted constitution
amendment voting. Those assets are retained as supporting governance
documents, not treated as a substitute for an election engine.

The election engine is implemented as active Work Package 37.1 work and must
meet the following persisted workflow scope before that work package can be
closed:

- Election creation, scheduling, and explicit phase control from announcement
  through archival.
- Election seats, appointed election officers, and a frozen voter-roll
  snapshot.
- Candidate nomination, proposer/seconder eligibility, scrutiny decisions,
  withdrawal, and final candidate lists.
- Secret-ballot issuance and vote storage separated from the member identity
  recorded on the voter roll.
- Duplicate-vote prevention, polling closure, counting, rejected ballots,
  recount handling, result publication, and immutable audit history.
- Declaration that writes elected members into the applicable executive
  committee period.
- Generated election forms/PDFs with verification data.
- Feature-flagged public, member, and administrator Web surfaces, with Mobile
  parity where the existing backlog requires it.

This decision does not make Work Package 84 the implementation owner. It
records the product decision and leaves implementation acceptance with Work
Package 37.1. The existing constitution amendment vote remains
its own workflow and must not be conflated with election ballots. The current
static election documents remain the legal and operational reference material
until the engine is implemented and adopted.

## Open question (product decision, not a documentation task)

- **White-label deployability**: is removing the second-institution migration
  blocker in scope for this project, or does white-label stay a
  single-institution profile-pack mechanism? No specification work should
  claim white-label readiness until decided.

## Out of scope

- Any change to source code, migrations, DTOs, or configuration.
- Resolving the white-label question above.
- Building the parity/error/authorization catalogs (Stories 4–5) before
  Stories 1–3 exist, since they read from those tables.

## Scope notes

This specification records behavior evidenced by the implementation, the same
constraint 001 operates under. Details that cannot be verified from source or
tests are marked as gaps, not inferred from external documentation.
