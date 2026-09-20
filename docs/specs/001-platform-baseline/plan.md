# Implementation Plan: GHCAA Platform Baseline

**Branch**: `001-platform-baseline` | **Date**: 2026-09-20 |
**Spec**: [spec.md](./spec.md)

## Summary

Define the behavior of the full GHCAA platform as independently testable
requirements. The work is documentation-only. The specification covers
membership, authentication, profiles, events, finance, networking, messaging,
governance, CMS, assistant, and operational concerns. The implementation trace
is kept separate so requirement statements describe what the system shall do,
not what has already been delivered.

## Technical Context

**Language/Version**: ASP.NET Core 9 / C#, Angular 21 / TypeScript, Flutter 3.x /
Dart

**Primary Dependencies**: EF Core, FluentValidation, JWT and BCrypt,
SignalR, Angular standalone components and signals, Dio, Riverpod, and
`go_router`

**Storage**: PostgreSQL in production, SQLite for local and test scenarios,
local file storage with cloud abstraction readiness

**Testing**: `GHCAA.Tests`, Angular unit tests, Flutter tests, API contract
snapshot comparison, and the standard CI checks

**Target Platform**: ASP.NET Core API, browser SPA, iOS and Android clients

**Project Type**: Full-stack web service, SPA, and mobile application

**Performance Goals**: Preserve the documented rate limits, image-size rules,
mobile retry cap, and fast-loading shared UI behavior

**Constraints**: Additive API compatibility, privacy-aware DTO mapping,
`IsArchived` query filters, `dd-MM-yyyy` display dates, no live payment keys,
shared Web/Mobile behavior, and no source changes for this baseline

**Scale/Scope**: 10 independently testable user stories, 30 functional and
cross-layer requirements, and the implemented controller/client route surface across authentication,
membership, events, finance, networking, governance, content, messaging,
administration, and health

## Constitution Check

- [x] The current codebase and executable tests are the source of truth.
- [x] All shared API behavior is traced across API, Web, Mobile, and tests.
- [x] No implementation abstraction, dependency, endpoint, or migration is
  introduced.
- [x] Security, privacy, date, payment, and archive constraints are explicit.
- [x] Verification commands are recorded in `tasks.md`.
- [x] All generated and authored artifacts remain under `docs/specs/`.

## Project Structure

```text
docs/specs/
├── .github/                         # Spec Kit Copilot skills
├── .specify/                        # Spec Kit templates, scripts, workflows
└── 001-platform-baseline/
    ├── spec.md                      # Requirements baseline
    ├── plan.md                      # This plan
    ├── tasks.md                     # Verification and maintenance tasks
    └── contracts/
        └── api-cross-layer.md       # API route-domain and client trace
    └── evidence/
        └── implementation-inventory.md # Full implementation evidence

GHCAA.Domain/                        # Entities, enums, constants
GHCAA.Application/                   # DTOs, interfaces, validators
GHCAA.Infrastructure/                # EF, services, integrations
GHCAA.API/                            # Controllers, middleware, hubs
GHCAA.Web/src/app/core/               # Angular constants, models, services
GHCAA.Mobile/lib/                     # Flutter API and feature services
GHCAA.Tests/                          # Backend controller, service, validator tests
Generated API/client contract surfaces
```

**Structure Decision**: Keep the existing Clean Architecture and client
directories. Store only the Spec Kit baseline under `docs/specs/`; do not move
or duplicate production code.

## Cross-Layer Design Notes

The API contract begins in Application DTOs and service interfaces, is exposed
by thin API controllers, and is consumed by Angular typed services/constants
and Flutter Dio services. Backend controller and service tests provide the
server-side contract evidence. Angular service specs and Flutter tests cover
client request and response behavior where present. The generated Swagger
snapshot is the route-level compatibility artifact.

The detailed domain-to-source map is in
[contracts/api-cross-layer.md](./contracts/api-cross-layer.md). It records the
implementation surfaces inspected for this baseline. The networking cursor
path is an additive compatibility constraint: the backend accepts an optional
cursor, Mobile uses it for directory pagination, and Web callers that omit it
retain the legacy page/pageSize behavior.

The broader source and implementation inventory is in
[evidence/implementation-inventory.md](./evidence/implementation-inventory.md).
It is based on the backend layers, Web routes and services, Mobile routes and
screens, project files, generated client contract surfaces, and executable
tests.

The endpoint and workflow-level feature catalog is in
[evidence/implementation-feature-catalog.md](./evidence/implementation-feature-catalog.md).
It expands each implemented domain into actors, routes, contracts,
validation, authorization, state transitions, client surfaces, failures, and
tests. Details that cannot be verified from code are marked as gaps rather
than inferred from external documentation.
