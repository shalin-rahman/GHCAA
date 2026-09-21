# Project Agentra AI Deployment and Operations Specification

**Parent specification:** [spec.md](spec.md)  
**Status:** Draft  
**Created:** 2026-09-21

## Deployment profiles

Agentra must support the same contracts in four profiles:

1. **Embedded:** runtime modules execute inside the host process.
2. **Modular monolith:** Agentra modules run beside the host API.
3. **Shared service:** several registered hosts use one isolated Agentra
   service.
4. **Isolated service:** one host receives a separately deployed runtime.

Embedded or modular-monolith deployment is the default starting point.
Extraction requires evidence from load, security, availability, or operational
requirements. Deployment profile is configuration, not a code fork.

## Environments

Every deployment declares an environment such as `development`, `test`,
`staging`, or `production`. Environment configuration must control:

- registered applications and enabled features;
- provider and model aliases;
- adapter endpoints and service identities;
- limits, quotas, retry and cooldown policies;
- data classification, retention, residency, and redaction;
- logging, metrics, tracing, and alert thresholds;
- allowed origins, egress destinations, and network boundaries.
- TLS policy, certificate trust, mTLS references, and outbound proxy policy;
- vector backend, embedding version, index policy, and retrieval limits.

The same artifact should be promoted between environments. Environment
differences belong in external configuration and secret references, not source
conditionals.

## Configuration promotion

Configuration moves through:

```text
Draft -> Validate -> Review -> Approve -> Activate -> Observe
                                              |
                                      Roll back or suspend
```

Each version records owner, reason, environment, created time, activation time,
checksum, and approval. Promotion must:

1. validate schemas and references;
2. check least privilege and deny-by-default;
3. verify secret references without printing secret values;
4. run adapter health and conformance checks;
5. compare resource limits with environment policy;
6. support atomic activation and immediate rollback.

Invalid configuration must leave the last valid version active and produce an
explicit health and audit signal.

## Runtime services

A deployment may contain these independently scalable roles:

| Role | Responsibility |
|---|---|
| API gateway | Authentication boundary, rate limits, request size, routing |
| Agent API | Request validation, policy evaluation, orchestration, response |
| Adapter workers | Bounded capability, event, document, and synchronization work |
| Model gateway | Provider adapters, quotas, retries, cooldowns, usage metadata |
| Retrieval service | Index access, filtering, freshness, source references |
| Configuration service | Versioning, validation, activation, rollback |
| Audit and telemetry | Safe audit records, metrics, traces, health, alerts |

These roles may initially run as one process. Splitting them must preserve
correlation IDs, authorization context, cancellation, timeout, and audit
semantics.

## Retrieval storage

The retrieval interface must not depend on one vector database. pgvector is the
first PostgreSQL-oriented option to evaluate when the host already operates
PostgreSQL. It is suitable only when:

- the required PostgreSQL and pgvector versions are supported;
- vector dimensions, embedding model version, distance metric, and index
  strategy are configured and recorded;
- tenant, application, classification, deletion, and source-version filters
  are applied before results reach a model;
- index rebuild, vacuum, backup, restore, and migration time fit the recovery
  objectives;
- the host accepts the operational and storage cost.

The adapter must expose retrieval health separately from core Agentra health.
If pgvector is unavailable, Agentra must return a controlled degraded state or
use a configured non-vector fallback. It must not silently return stale private
content.

## Secrets and network

- Store credentials, signing keys, certificates, and connection strings in the
  deployment's secret manager.
- Reference secrets by stable identifier; never place secret values in source,
  ordinary configuration, container images, logs, metrics, or exports.
- Use separate identities and least-privilege network access for each adapter.
- Deny outbound traffic by default and allow only configured provider and host
  destinations.
- Require HTTPS for production host, provider, configuration, telemetry, and
  secret-manager calls. Plain HTTP is allowed only for explicitly isolated
  local development.
- Validate certificate chains and hostnames. Do not disable TLS validation to
  make an integration pass.
- Configure mTLS where the host or provider requires service identity. Keep
  client certificates and private keys in the secret manager.
- Disable cross-origin redirects by default and block HTTPS-to-HTTP
  downgrade.
- Restrict DNS resolution, proxy use, and egress destinations to configured
  policy. Protect against SSRF when a URL is supplied by configuration or
  host data.
- Rotate secrets without rebuilding the application where the deployment
  platform supports it.
- Test that a failed secret lookup stops the dependent integration safely.

## Health and readiness

The deployment must expose:

- liveness: the process can respond;
- readiness: required configuration and dependencies are usable;
- dependency status: host adapters, model providers, queues, indexes, and
  secret references;
- configuration version and checksum, without secret data.

An optional dependency may be degraded without taking down the core runtime.
A required dependency must prevent readiness from being reported as healthy.
Health responses must not reveal credentials, private payloads, SQL, or internal
network details.

## Reliability and scaling

All limits are configurable and bounded:

- request and response bytes;
- model context and output units;
- concurrent requests and queue depth;
- adapter timeout and cancellation;
- retry count, backoff, and cooldown;
- per-application, tenant, principal, provider, and model quotas.

Use idempotency keys for sensitive or write operations. Use circuit breakers
and back-pressure to avoid retry storms. Workers must support cancellation and
must expose retry, terminal failure, and dead-letter state. Scaling must not
break tenant isolation or duplicate a non-idempotent operation.

## Delivery pipeline

The delivery pipeline must:

1. restore dependencies from lockfiles;
2. run formatting, lint, type, unit, contract, security, and adapter
   conformance checks;
3. scan dependencies and artifacts for known vulnerabilities and secrets;
4. build an immutable artifact;
5. run migration and configuration compatibility checks;
6. deploy to a disposable environment;
7. execute health, smoke, authorization, and rollback tests;
8. promote only an approved artifact and configuration version.

Deployment must not depend on live provider keys in tests. Provider and host
adapters use test doubles or isolated test credentials.
HTTPS tests must cover certificate validation, hostname mismatch, redirect
downgrade, origin scoping, timeout, response-size limits, and token redaction.
When gRPC is enabled, the same tests must cover TLS or mTLS, deadlines,
cancellation, message limits, and status mapping.
When pgvector is enabled, the pipeline must test extension compatibility,
dimension mismatch, access filters, deletion propagation, index rebuild, and
restore behavior.

## Observability

Record metrics and traces for:

- request count, latency, cancellation, and terminal status;
- capability authorization decisions and duration;
- provider attempts, cooldowns, usage, and failures;
- retrieval count, latency, freshness, and filtered records;
- queue depth, retries, dead letters, and synchronization lag;
- configuration activation, rollback, and validation failures;
- rate limits, quota exhaustion, and abuse signals.

Use application, environment, tenant, capability, and provider dimensions only
when they do not reveal private content. Do not log raw prompts, model context,
tool results, database values, secrets, or uploaded documents by default.

## Backup and recovery

Define recovery objectives for configuration, audit records, queues, source
metadata, indexes, and conversation data. Backups must be encrypted, access
controlled, tested for restore, and free of avoidable secret duplication.

Recovery tests must verify:

- last valid configuration can be restored;
- deleted source records do not return after index restore;
- in-flight work is replayed idempotently;
- audit correlation survives recovery;
- key rotation and provider replacement remain possible.

## Operational acceptance

A deployment is ready only when:

- configuration can be validated, promoted, observed, and rolled back;
- every enabled adapter passes conformance and health checks;
- required secrets are resolved without appearing in logs or exports;
- authorization, tenant isolation, redaction, rate, timeout, and failure tests
  pass;
- smoke tests cover each enabled client and transport;
- an operator can identify and suspend a failed host, capability, provider, or
  configuration version;
- outbound HTTPS trust and egress policy are verified in the target environment;
- enabled retrieval backends pass freshness, authorization, deletion, and
  recovery tests;
- backup, restore, incident, and provider-outage procedures are documented.
