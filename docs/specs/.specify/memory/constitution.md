# GHCAA Specification Constitution

## Core Principles

### I. Preserve the existing product contract

Specifications describe the current GHCAA requirements and architecture before
they propose change. Existing behavior, user changes, and the additive API
compatibility rule must be preserved unless a requirement explicitly says
otherwise.

### II. Trace shared behavior across all clients

Any API or business-rule requirement must be traced through Domain,
Application, Infrastructure, API, Angular, Flutter, and the relevant tests.
The Web and Mobile clients consume the same contract and must not drift.

### III. Keep boundaries clear

Domain remains framework-independent. Application owns contracts and
validation. Infrastructure owns persistence and integrations. API controllers
remain thin. Clients call typed services rather than bypassing the API
contract.

### IV. Treat security and privacy as requirements

Authentication, authorization, rate limits, soft deletion, auditability,
security-stamp invalidation, upload validation, and member privacy toggles are
part of the acceptance criteria. Manual payment configuration must not require
live gateway credentials.

### V. Verify with repository checks

Specifications name the checks that can verify each requirement. Changes to
code or maintained documentation require the smallest relevant test suites,
client checks where contracts are shared, and a final `graphify update .`.

## Project Constraints

- The canonical displayed date format is `dd-MM-yyyy`. API date-only values
  use ISO `yyyy-MM-dd`.
- Archived records use the established `IsArchived` flag and EF query filters.
- Angular uses standalone components, signals, typed services, and the shared
  design tokens in `GHCAA.Web/src/styles.scss`.
- Flutter uses Riverpod, `go_router`, `Dio`, `AppConfig`, and shared theme and
  widget layers.
- Secrets, payment gateway keys, credentials, environment files, and
  diagnostic logs must not be added to specifications or source.
- This specification set is documentation-only. It does not authorize
  implementation, migration, deployment, commit, or push work.

## Workflow

1. Use the repository graph before source searches.
2. Read the Domain, Application, Infrastructure, API, Web, Mobile, and test
   projects before defining cross-layer scope.
3. Record existing behavior and unresolved gaps from code and tests instead of
   inventing behavior.
4. For a contract change, verify both clients and the relevant tests.
5. Keep all Spec Kit artifacts for this baseline under `docs/specs/`.

## Governance

The codebase is authoritative for implementation. This constitution governs the
Spec Kit artifacts only. Any conflict is resolved by preserving the
repository's established architecture and compatibility rules, then recording
the conflict in the relevant specification.

**Version**: 1.0.0 | **Ratified**: 2026-09-20 | **Last Amended**: 2026-09-20
