# Agentra AI Integration Specification

**Parent specification:** [spec.md](spec.md)  
**Status:** Draft  
**Created:** 2026-09-21

## Purpose

This document defines how another system integrates with Agentra without
adopting GHCAA's domain, framework, database, identity provider, or client
technology. The integration is a set of versioned contracts and adapters.
Agentra must not require host-specific branches in its runtime.

## Integration modes

| Mode | Host provides | Agentra provides | Use |
|---|---|---|---|
| Embedded | In-process adapter and host policy | Agent orchestration and normalized response | Small applications or shared deployments |
| Host API | HTTPS or gRPC endpoints and trusted service identity | Capability registry, policy checks, and invocation | Preferred default |
| Events | Signed events and replay or reconciliation endpoint | Indexing, deduplication, retry, and freshness state | Asynchronous knowledge updates |
| Documents | Pull or push source adapter with classification | Ingestion, metadata, access filtering, and deletion | Policies, manuals, approved content |
| Read-only data | Restricted views and read-only credentials | Query policy, masking, limits, and audit | Reporting where no API exists |
| Client | Request and response adapter | Stable response envelope and negotiated features | Web, Mobile, desktop, voice, or other clients |

Hosts may use several modes. Enabling one mode must not require unrelated
dependencies.

## Host registration

An onboarding request must define:

```json
{
  "applicationId": "example-host",
  "environment": "production",
  "owner": "platform-team",
  "status": "draft",
  "contractVersions": ["agent.v1", "capability.v1"],
  "identity": {
    "issuer": "https://identity.example",
    "audience": "agentra",
    "claims": {
      "subject": "sub",
      "tenant": "tenant_id",
      "roles": "roles",
      "scopes": "scope"
    }
  },
  "features": {
    "streaming": false,
    "retrieval": true,
    "modelGeneration": false
  }
}
```

The values are examples only. The host must supply its own identifiers and
claims through configuration. Agentra must reject duplicate IDs, unsupported
versions, invalid identity settings, and enabled features without registered
adapters.

## Capability definition

Each capability must be registered with:

- stable `id` and semantic `version`;
- owner and application scope;
- human-readable description;
- input and output JSON Schema or equivalent;
- risk classification: `read`, `write`, `sensitive`, or `administrative`;
- authorization policy and required scopes;
- adapter reference and transport;
- timeout, cancellation, retry, response-size, and rate limits;
- idempotency requirement;
- approval policy;
- data classification and redaction policy.

Example:

```json
{
  "id": "directory.search",
  "version": "1.0.0",
  "risk": "read",
  "adapter": "host-api",
  "operation": "POST",
  "path": "/api/directory/search",
  "inputSchema": "schemas/directory-search-1.json",
  "outputSchema": "schemas/directory-result-1.json",
  "authorization": {
    "requiredScopes": ["directory:read"],
    "hostMustReauthorize": true
  },
  "limits": {
    "timeoutMs": 5000,
    "maxResponseBytes": 262144,
    "maxAttempts": 1
  }
}
```

The runtime must never infer a capability from a URL, SQL statement, prompt,
or model output. The registered definition is the only executable authority.

## Invocation contract

Every invocation carries:

```json
{
  "contractVersion": "agent.v1",
  "applicationId": "example-host",
  "tenantId": "tenant-1",
  "principal": {
    "subject": "user-123",
    "roles": ["member"],
    "scopes": ["directory:read"],
    "authTime": "2026-09-21T07:00:00Z"
  },
  "capability": {
    "id": "directory.search",
    "version": "1.0.0"
  },
  "input": {
    "term": "engineering"
  },
  "correlationId": "request-456",
  "idempotencyKey": "optional-for-write-capabilities"
}
```

The host must revalidate identity, tenant, authorization, input, and business
rules. Agentra must not treat `tenantId`, roles, or scopes supplied by an
untrusted client as authoritative.

## Response contract

```json
{
  "contractVersion": "agent.v1",
  "status": "success",
  "data": {},
  "sources": [],
  "warnings": [],
  "usage": {
    "units": 0
  },
  "correlationId": "request-456"
}
```

Allowed terminal statuses are `success`, `partial`, `denied`, `invalid`,
`timeout`, `unavailable`, and `failed`. Errors use a stable safe category and
must not include credentials, SQL, stack traces, internal URLs, or unredacted
private data.

## Transport rules

HTTPS REST/JSON is the initial interoperability transport. Plain HTTP is
forbidden outside explicitly isolated local development. gRPC is a later
transport option, but the capability and response contracts must be transport
neutral now so adding it does not change authorization or business behavior.
Messaging and in-process transports must preserve the same logical fields and
terminal statuses. Each adapter declares:

- authentication and service identity;
- TLS version and certificate validation policy;
- hostname verification and trusted CA or certificate reference;
- mTLS requirement and client certificate reference where applicable;
- allowed egress destination and redirect policy;
- serialization and schema version;
- timeout and cancellation behavior;
- retryable error categories;
- idempotency and duplicate handling;
- maximum request and response size;
- ordering and delivery guarantees;
- health and readiness behavior.

### HTTPS requirements

- All production integration calls MUST use HTTPS with certificate and
  hostname verification enabled.
- The client MUST use an approved TLS policy and MUST reject expired,
  untrusted, mismatched, or revoked certificates according to the deployment
  policy.
- Redirects MUST be disabled by default or restricted to configured HTTPS
  destinations. A redirect MUST NOT downgrade to HTTP or cross an unapproved
  host.
- Host URLs, CA bundles, client certificates, proxy settings, and TLS options
  MUST come from validated environment configuration or secret references.
- Requests MUST use bounded connect, send, receive, and total-operation
  timeouts. A timeout MUST produce a safe categorized error.
- Requests MUST include a correlation ID and, where configured, an idempotency
  key. Sensitive values MUST NOT be placed in URLs.
- Authentication tokens MUST be scoped to the target host and MUST NOT be
  forwarded to a different origin.
- Outbound responses MUST be size-limited, content-type checked, schema
  validated, and treated as untrusted data.
- Retries MUST be limited to configured transient categories and MUST respect
  idempotency. POST or write operations MUST NOT be retried unless the
  capability explicitly declares them idempotent.
- Logs MUST record destination identity, status, duration, and correlation ID
  without recording authorization headers, cookies, payload secrets, or
  personal data.

### gRPC readiness

The initial implementation need not ship gRPC, but it MUST keep:

- protobuf or equivalent message definitions separate from HTTP route names;
- deadlines, cancellation, metadata, status mapping, and message-size limits
  explicit in the contract;
- TLS/mTLS and certificate references configurable in the same trust model;
- service and method authorization independent of generated stubs;
- a conformance suite that runs against both REST and gRPC adapters later.

An eventual gRPC adapter MUST map to the same capability ID, version,
principal context, correlation ID, idempotency rules, audit events, and safe
terminal status as the HTTPS adapter.

### pgvector considerations

pgvector is an optional retrieval backend, not a required Agentra dependency.
The retrieval contract MUST remain independent of PostgreSQL and vector
extension details. A pgvector adapter must:

- store application, tenant, source, record, classification, policy, version,
  and deletion metadata beside each embedding;
- apply access filtering before similarity search results enter model context;
- use a configured embedding model and record its version;
- validate vector dimensions and reject mismatched embeddings;
- configure distance metric, index type, `topK`, similarity threshold, and
  freshness rules outside code;
- support re-indexing when the embedding model or chunking policy changes;
- remove or disable embeddings when source records are deleted or access is
  revoked;
- use bounded candidate counts, query time, and result size;
- avoid treating similarity as authorization;
- provide a non-vector adapter or deterministic fallback when pgvector is
  unavailable;
- define backup, restore, vacuum, index rebuild, and migration procedures.

The deployment must verify PostgreSQL and pgvector extension compatibility
before activation. Embeddings and derived indexes must follow the host's data
classification, residency, retention, and deletion policy.

Streaming is an opt-in versioned contract. A stream must end with exactly one
terminal event:

```text
status | capability_started | capability_result | content | source |
warning | error | usage | done
```

Clients must not treat partial content as a complete answer after disconnect.

## Knowledge and event integration

Source records must include:

```json
{
  "applicationId": "example-host",
  "tenantId": "tenant-1",
  "sourceId": "policy-documents",
  "recordId": "document-123",
  "recordVersion": "7",
  "sourceUpdatedAt": "2026-09-21T07:00:00Z",
  "classification": "internal",
  "accessPolicy": {
    "roles": ["member"]
  },
  "deleted": false,
  "contentHash": "configured-hash"
}
```

Events must be signed or authenticated, carry a unique event ID, identify the
source version, and support replay or reconciliation. Duplicate events are
ignored after successful processing. Older versions must not overwrite newer
versions. A deletion event removes or disables derived content according to the
host retention policy.

## Adapter conformance

An adapter is conformant only when it passes tests for:

1. valid request and response;
2. unknown application and capability;
3. unsupported version;
4. invalid input and output;
5. missing or insufficient scope;
6. cross-tenant access;
7. timeout, cancellation, and provider failure;
8. duplicate and out-of-order events;
9. redaction and classification;
10. correlation and audit metadata.

The suite must be runnable by hosts written in different languages. Test
fixtures must not require GHCAA entities or database tables.

## Client integration

Client adapters must consume typed contract models, show loading, empty,
partial, denied, unavailable, and error states, and preserve correlation IDs
for support. Clients must not implement authorization decisions locally as a
replacement for the host or Agentra policy.

### Angular Web integration

The Web adapter must:

- use a typed Angular service and the shared HTTP/interceptor pipeline;
- obtain the API base URL and feature flags from environment or organization
  configuration, not component literals;
- use the existing authentication, refresh, logout, and error-notification
  behavior;
- send the authenticated request through the configured HTTPS host and retain
  the correlation ID;
- negotiate contract version, response mode, locale, and enabled features;
- render loading, empty, partial, denied, unavailable, timeout, and error
  states using shared controls and design tokens;
- never store provider credentials, capability secrets, or unrestricted
  prompts in browser source or local storage;
- cancel requests when the route or component is destroyed and prevent stale
  responses from replacing newer state;
- apply accessible keyboard, focus, screen-reader, contrast, and responsive
  behavior;
- test request shape, authorization headers, refresh behavior, response
  mapping, error states, and feature-disabled behavior.

### Flutter Mobile integration

The Mobile adapter must:

- use a typed Dio service and the existing authentication, refresh, retry, and
  connectivity pipeline;
- obtain the API base URL, TLS settings, feature flags, and organization
  configuration from `AppConfig` or the configured environment, never from a
  screen or hard-coded URL;
- preserve bearer-token scoping and correlation IDs through interceptors;
- negotiate contract version, response mode, locale, and enabled features;
- expose loading, empty, partial, denied, unavailable, timeout, offline, and
  error states through Riverpod providers and shared widgets;
- cancel requests when providers or screens are disposed and avoid updating
  stale provider state;
- keep credentials and provider configuration out of the APK, source, logs,
  and local storage;
- use the shared theme, spacing, accessibility, and responsive widget layers;
- test request fields, auth refresh, TLS/error mapping, offline behavior,
  retry limits, response parsing, and feature-disabled behavior.

### Web/Mobile parity

Web and Mobile must use the same capability IDs, contract versions, request
field names, response envelope, authorization semantics, error categories, and
source-disclosure rules. Platform-specific presentation may differ, but
business decisions must not.

Web and Mobile clients must use the same field names and response versions.
GHCAA's current route remains `{ "query": "..." }` until a coordinated
versioned migration updates the API, Angular, Flutter, OpenAPI, and tests.
