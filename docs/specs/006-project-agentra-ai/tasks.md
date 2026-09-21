# Tasks: Project Agentra AI

**Input**: [spec.md](spec.md), [integration-contract.md](integration-contract.md),
[deployment.md](deployment.md), [plan.md](plan.md)

**Status**: Planned. These tasks define implementation scope; they do not
authorize deployment, commits, or provider-key acquisition.

## Phase 1: Contract and threat-model foundation

- [ ] T001 Define versioned schemas for application registration, principal
      context, capability, source, invocation, response, error, audit, and
      configuration in `agentra/contracts/`.
- [ ] T002 Define configuration schema validation, environment overlay, secret
      reference, checksum, activation, rollback, and retirement behavior in
      `agentra/configuration/`.
- [ ] T003 [P] Write the integration threat model covering SSRF, TLS
      downgrade, certificate trust, token forwarding, replay, tenant escape,
      prompt injection, and sensitive logging in
      `docs/specs/006-project-agentra-ai/`.
- [ ] T004 [P] Define stable error categories, retry classification,
      idempotency rules, cancellation semantics, and correlation requirements.
- [ ] T005 [P] Create language-neutral conformance fixtures without GHCAA
      entities or database assumptions.

## Phase 2: Secure HTTPS integration

- [ ] T006 Implement a transport-neutral capability adapter interface.
- [ ] T007 Implement HTTPS request execution using configured base URLs,
      certificate and hostname validation, optional mTLS, proxy policy, and
      egress allowlists.
- [ ] T008 Add separate connect, send, receive, total-operation, response-size,
      and cancellation limits from configuration.
- [ ] T009 Add content-type, response-schema, redirect, origin, and
      correlation-ID validation.
- [ ] T010 Add scoped authentication and idempotency handling. Never forward
      bearer tokens or cookies to a different configured origin.
- [ ] T011 Add bounded retries, circuit breaking, cooldowns, and safe
      categorized failures. Do not retry non-idempotent writes by default.
- [ ] T012 [P] Add security tests for certificate failure, hostname mismatch,
      HTTP downgrade, redirect to an unapproved host, SSRF attempts, timeout,
      oversized response, token redaction, and correlation propagation.
- [ ] T013 [P] Add adapter tests for valid, invalid, denied, unavailable,
      cancelled, and timed-out capability calls.

## Phase 3: Configuration and operational controls

- [ ] T014 Implement deny-by-default policy evaluation for application,
      tenant, principal, capability, source, provider, and client scope.
- [ ] T015 Implement atomic configuration activation that retains the last
      valid version when validation or secret resolution fails.
- [ ] T016 Implement configuration export without secrets, version comparison,
      audit history, approval metadata, and rollback.
- [ ] T017 Implement liveness, readiness, dependency health, configuration
      checksum, and degraded optional-dependency reporting.
- [ ] T018 Implement metrics and traces for latency, authorization, retries,
      quotas, provider calls, retrieval freshness, queue state, and config
      changes without private payloads.
- [ ] T019 [P] Add secret scanning and tests proving keys, tokens, passwords,
      connection strings, and private payloads do not enter logs or exports.

## Phase 4: GHCAA and second-host integration

- [ ] T020 Define a GHCAA adapter around the existing assistant contract
      without changing `POST /api/assistant/ask` until coordinated migration.
- [ ] T021 Align the GHCAA Mobile request field with the API contract and add
      Web/Mobile contract tests if implementation work is approved.
- [ ] T022 Apply GHCAA privacy projection before assistant member data enters
      an Agentra response.
- [ ] T023 [P] Create a second independent host adapter or language-neutral
      test host and run the same conformance suite.
- [ ] T024 [P] Verify client loading, empty, partial, denied, unavailable,
      error, and correlation states for Web and Mobile.
- [ ] T024a [P] Define the Angular Agentra service, typed contract models,
      environment configuration, interceptor behavior, cancellation, and
      feature negotiation under `GHCAA.Web/src/app/core/`.
- [ ] T024b [P] Define the Flutter Agentra Dio service, typed contract models,
      `AppConfig` integration, Riverpod state, connectivity handling,
      cancellation, and feature negotiation under `GHCAA.Mobile/lib/`.
- [ ] T024c [P] Add Angular tests for HTTPS request shape, auth refresh,
      correlation IDs, response mapping, denied/timeout/unavailable states, and
      disabled features under `GHCAA.Web/src/app/`.
- [ ] T024d [P] Add Flutter tests for request fields, auth refresh, response
      parsing, offline/retry limits, timeout/unavailable states, and disabled
      features under `GHCAA.Mobile/test/`.
- [ ] T024e Compare Web and Mobile fixtures to prove identical capability IDs,
      contract versions, error categories, authorization semantics, and source
      disclosure.

## Phase 5: pgvector retrieval evaluation

- [ ] T025 Define a provider-neutral retrieval interface and source metadata
      contract before adding pgvector-specific code.
- [ ] T026 Define configurable embedding provider alias, embedding version,
      vector dimensions, distance metric, chunking, `topK`, similarity
      threshold, freshness, and result-size limits.
- [ ] T027 Implement pgvector adapter with application, tenant, source,
      classification, access-policy, version, and deletion metadata.
- [ ] T028 Enforce access filtering before similarity results enter model
      context. Prove similarity search cannot authorize access.
- [ ] T029 Implement re-indexing for embedding or chunking changes and
      deletion or access-revocation propagation.
- [ ] T030 [P] Test pgvector extension compatibility, dimension mismatch,
      filtered retrieval, stale data, deletion, index rebuild, backup, and
      restore behavior.
- [ ] T031 [P] Implement and test a non-vector or deterministic fallback for
      pgvector outage or unsupported hosts.

## Phase 6: gRPC transport readiness

- [ ] T032 Define protobuf or equivalent messages from the existing
      transport-neutral schemas, not from HTTP route names.
- [ ] T033 Implement gRPC deadlines, cancellation, TLS/mTLS, message-size
      limits, metadata mapping, and status mapping behind configuration.
- [ ] T034 Map gRPC calls to the same capability ID, version, authorization,
      audit, idempotency, and terminal statuses as HTTPS.
- [ ] T035 [P] Run the conformance suite against HTTPS and gRPC and prove
      equivalent outcomes for valid, denied, timeout, cancellation, and
      failure cases.

## Phase 7: Deployment and production readiness

- [ ] T036 Define embedded, modular-monolith, shared-service, and isolated
      deployment profiles without code-specific configuration branches.
- [ ] T037 Add environment promotion checks for immutable artifacts,
      configuration compatibility, secret references, smoke tests, and
      rollback.
- [ ] T038 Add network policy tests for HTTPS-only production egress,
      certificate rotation, mTLS, DNS/proxy restrictions, and redirect policy.
- [ ] T039 [P] Add load, queue, quota, retry-storm, provider-outage, and
      failure-injection tests using test doubles.
- [ ] T040 [P] Document backup, restore, key rotation, incident response,
      pgvector recovery, and operator suspension procedures.
- [ ] T041 Run repository-standard checks for each changed host client and
      run `graphify update .` after implementation or specification changes.

## Dependencies and execution order

- Phase 1 blocks every implementation phase.
- Phase 2 blocks production integration and provides the first secure transport.
- Phase 3 is required before environment promotion or shared-service operation.
- Phase 4 depends on the stable contract and HTTPS adapter.
- Phase 5 depends on the provider-neutral retrieval contract; it is optional.
- Phase 6 depends on the transport-neutral contract and can follow HTTPS.
- Phase 7 depends on the enabled features and their conformance tests.
- Tasks marked `[P]` may run in parallel when their files and fixtures do not
  overlap.
