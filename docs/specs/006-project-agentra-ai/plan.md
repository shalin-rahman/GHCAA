# Implementation Plan: Project Agentra AI

**Branch**: `006-project-agentra-ai` | **Date**: 2026-09-21  
**Spec**: [spec.md](spec.md)

## Summary

Build Agentra as a configuration-first, host-neutral runtime. Start with a
secure HTTPS/JSON integration and a transport-neutral capability contract.
Design the adapter boundary so gRPC can be added later without changing
authorization, schemas, audit events, or host business rules. Evaluate pgvector
as an optional retrieval adapter with access filtering, embedding versioning,
freshness, deletion, backup, and fallback controls.

The first delivery should be a modular boundary and conformance suite, not a
distributed platform with every optional provider. GHCAA remains the reference
host adapter. A second independent adapter or test host is required before
claiming reuse.

## Technical context

**Language/Version**: Host-neutral contracts; reference integration follows the
repository's .NET 9 API, Angular 21 Web, Flutter, and existing test runners.

**Primary Dependencies**: HTTPS REST/JSON first; JSON Schema or equivalent;
secret manager; optional PostgreSQL/pgvector; gRPC later through a separate
transport adapter.

**Storage**: Host-owned data. Agentra metadata, audit, configuration, queues,
and retrieval indexes are separate concerns. pgvector is optional.

**Testing**: Contract and conformance tests, host integration tests, security
tests, provider test doubles, client tests, deployment smoke tests, and
repository-standard backend/Web/Mobile checks where GHCAA is changed.

**Target Platform**: HTTPS-capable server deployment; embedded, modular
monolith, shared service, or isolated service.

**Project Type**: Reusable platform runtime with host adapters and client
contracts.

**Performance Goals**: Configurable per host. Every call has bounded connect,
send, receive, total-operation, queue, response-size, and model limits. Targets
must be measured per deployment rather than hard-coded globally.

**Constraints**: Deny by default, HTTPS in production, certificate and
hostname validation, no plaintext secrets, host authorization on every
capability, tenant isolation, bounded retries, and safe failure.

**Scale/Scope**: Begin with one GHCAA adapter and one independent reference
adapter or test double. Add providers, gRPC, and pgvector only behind
configuration and conformance tests.

## Constitution check

- Existing GHCAA behavior remains the source of truth for its adapter.
- API and business-rule changes are checked in Web and Mobile clients.
- Host identity, privacy, and transactions remain host-owned.
- Secrets and live payment or provider keys are not added.
- Configuration and documentation changes are followed by `graphify update .`.
- No deployment, commit, push, or migration is authorized by this plan.

## Architecture and phases

### Phase 0: Contract and threat model

Define the versioned application, principal, capability, source, invocation,
response, error, audit, and configuration schemas. Record HTTPS threats,
SSRF, token forwarding, redirect downgrade, certificate validation, replay,
tenant escape, prompt injection, and data leakage controls.

### Phase 1: Secure HTTPS reference adapter

Implement the transport-neutral adapter interface and an HTTPS client with
configured TLS validation, optional mTLS, allowlisted destinations, bounded
timeouts, response-size and content-type checks, correlation, idempotency, and
safe redaction. Do not put host routes or provider names in compiled logic.

### Phase 2: Configuration and operations

Implement schema validation, secret references, environment overlays, atomic
activation, immutable versions, rollback, health/readiness, audit, metrics,
rate limits, quotas, circuit breakers, and safe degraded states.

### Phase 3: Conformance and client integration

Provide language-neutral adapter fixtures and tests. Integrate GHCAA Web and
Mobile through typed contracts without changing the existing route until a
coordinated versioned migration is approved. Add a second independent host
adapter or test double. Implement the Angular service/interceptor and Flutter
Dio/Riverpod adapters with the same contract, authentication, correlation,
cancellation, offline, error, and feature-negotiation semantics.

### Phase 4: Retrieval and pgvector evaluation

Implement the retrieval interface first. Add pgvector only when compatibility,
classification, filtering, embedding versioning, deletion, index maintenance,
backup, restore, and measurable retrieval goals are accepted. Keep a
non-vector or deterministic fallback.

### Phase 5: gRPC transport

Add gRPC after the HTTPS contract is stable. Map the same schemas, deadlines,
cancellation, identity, capability authorization, audit, idempotency, and
terminal statuses. Run the same conformance suite against both transports.

### Client acceptance

Verify Web and Mobile separately and then compare both against the same
fixtures. Both clients must pass authenticated, expired-session, denied,
timeout, unavailable, disconnected, partial-response, feature-disabled, and
contract-version cases.

### Phase 6: Production readiness

Run security, load, failure-injection, provider-outage, certificate-rotation,
rollback, backup/restore, tenant-isolation, and incident-response tests.
Promote only immutable artifacts with approved configuration.

## Project structure

```text
docs/specs/006-project-agentra-ai/
├── spec.md
├── integration-contract.md
├── deployment.md
├── plan.md
└── tasks.md

Future implementation boundary:
agentra/
├── contracts/
├── configuration/
├── policy/
├── transports/
│   ├── https/
│   └── grpc/
├── adapters/
│   ├── host-api/
│   ├── events/
│   ├── documents/
│   ├── read-only-data/
│   ├── models/
│   └── retrieval/
├── audit/
├── observability/
└── tests/
    ├── conformance/
    ├── security/
    ├── integration/
    └── recovery/
```

The tree is a target boundary, not a request to add a new project immediately.
The first implementation may live inside an existing host or modular monolith.

## Risks and mitigations

| Risk | Mitigation |
|---|---|
| Insecure outbound call | HTTPS-only production policy, certificate validation, egress allowlist, SSRF checks, and token-origin scoping |
| Transport drift | One transport-neutral schema and shared conformance suite |
| gRPC added as a second business path | Keep gRPC as an adapter over the same capability and policy services |
| Vector search treated as authorization | Filter by policy before similarity search and test tenant isolation |
| pgvector operational burden | Optional adapter, compatibility gate, fallback, index/recovery runbook |
| Configuration drift | Immutable versions, checksums, atomic activation, promotion and rollback |
| Provider cost or outage | Per-scope budgets, bounded retries, cooldown, and deterministic fallback |
| False reuse claim | Require two independent adapters and evidence from conformance tests |
