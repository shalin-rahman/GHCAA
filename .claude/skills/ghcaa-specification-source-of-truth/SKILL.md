---
name: ghcaa-specification-source-of-truth
description: Use when creating or revising GHCAA specifications; derive behavior from the implemented backend, Web, Mobile, generated contracts, and tests.
---

# GHCAA Specification Source of Truth

## Authority

For implementation specifications, treat the current codebase as authoritative:

- `GHCAA.Domain` contains entities, enums, constants, and relationships.
- `GHCAA.Application` contains DTOs, interfaces, validators, mappings, and
  security contracts.
- `GHCAA.Infrastructure` contains persistence, EF configurations, services,
  storage, providers, integrations, and migrations.
- `GHCAA.API` contains controllers, middleware, policies, hubs, health, and
  generated API contracts.
- `GHCAA.Web` contains Angular routes, standalone components, typed services,
  guards, interceptors, models, and unit tests.
- `GHCAA.Mobile` contains the Dio client, feature services, Riverpod
  providers, routes, screens, shared widgets, and tests.
- `GHCAA.Tests`, Angular specs, and Flutter tests are executable evidence.

SRS files, feature catalogs, architecture notes, project maps, TODO files,
book chapters, and API registries may provide context, but they are not
normative when the user asks for codebase-derived specifications.

## Workflow

1. Run a focused `graphify query`, `graphify explain`, or `graphify path` before
   reading raw source files.
2. Trace each feature from Domain/Application contracts through Infrastructure
   and API to Angular and Flutter consumers.
3. Read the route/controller, DTO, validator, service, persistence, client, and
   test surfaces before writing requirements.
4. Document concrete actors, routes, request/response DTOs, validation,
   authorization, state transitions, persistence behavior, client surfaces,
   error behavior, and tests.
5. Mark details that cannot be verified in code as gaps or unknowns. Never
   infer them from a documentation-only requirement.
6. Keep requirements buildable and implementation-aligned. Distinguish
   implemented behavior from compatibility behavior and uncovered behavior.
7. For shared API behavior, check .NET, Angular, Flutter, and tests together.
8. Store specification artifacts under `docs/specs/` and run `graphify update .`
   after documentation changes.

## Current platform map

The implemented feature domains currently include registration and
authentication, member profiles and secure documents, approvals and roles,
events and attendance, fees/payments/ledger/gateways, directory/jobs/
mentorship/family links, messaging/notifications/SignalR, governance and
constitution, election documents, news/notices, CMS and organization
configuration, gallery/albums, themes/polls, assistant/support, campaigns and
activity, imports/exports, communication, audit/error operations, lookups,
health, storage, retry/connectivity, and client loading/error/empty states.

The existing baseline specification groups these domains into ten user stories,
but a complete implementation catalog must expand each domain to endpoint and
workflow level. A domain summary or controller list alone is not sufficient.
