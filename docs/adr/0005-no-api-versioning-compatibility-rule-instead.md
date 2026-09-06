# ADR-0005: No API versioning — an additive-only compatibility rule instead

Status: Accepted

## Context

`GHCAA.API` has no versioning today (`grep -rn "ApiVersion" GHCAA.API` returns nothing). Every route
is a single, unversioned surface shared by the Angular web client and the Flutter mobile client.

The two clients are not in the same position. The web client is served from the same deployment as
the API — a breaking route or DTO change ships to both at once, so there's never a version mismatch
window. The Flutter client ships through the Google Play Store and Apple App Store: a build submitted
today can still be running against a live API weeks or months from now, through store review delay,
staged rollout, and users who simply don't update. A renamed route or a repurposed DTO field breaks
those installed builds with no way to signal the mismatch and no way to push a fix — the only recourse
is an emergency store submission, which itself takes days.

URL-segment or header-based versioning (`/api/v2/...`, `Accept: application/vnd.ghcaa.v2+json`) would
address this, but at a cost this project doesn't need yet: two parallel route/DTO surfaces to maintain,
version negotiation logic, and a policy for how long an old version stays live. There is exactly one
mobile build lineage today, not multiple client versions intentionally pinned to different contract
versions — the actual problem is narrower than what full versioning solves.

## Decision

The API stays unversioned. In its place, one rule governs every change to a route or a response/request
DTO from this point on:

**Never remove or rename an existing route or field. Only ever add.**

Concretely:

- A field that's no longer meaningful is deprecated in a code comment and left in place returning a
  stable (possibly default/empty) value — not deleted, not renamed, not repurposed to mean something
  else under the same name.
- A route that needs a materially different shape gets a *new* route (or a new optional field on the
  existing DTO); the old route keeps serving the old shape until telemetry or a store-side minimum
  version shows no installed build still calls it.
- Enum values are additive only. An existing enum member's underlying value never changes meaning
  once a mobile build has shipped reading it.
- A required field is never added to an existing request DTO — new fields on requests are always
  optional with a server-side default, since an old mobile build cannot supply a field it doesn't know
  about.

This is enforceable by review, not by a compiler: 82.4 (one error contract) and 82.9 (correlation id)
are additive by construction and don't strain the rule; the discipline this ADR asks for is on future
work that touches an existing route or DTO shape.

## Consequences

- No version negotiation code, no parallel route surfaces, no version-lifecycle policy to maintain —
  the cost side of full versioning is avoided entirely.
- The cost moves to code review: a PR that renames or removes a field/route on anything the mobile
  client might call is the thing this rule exists to catch. `docs/PROJECT_MAP.md` and the DTO files
  under `GHCAA.Application/DTOs` are the places to check what the mobile client currently reads before
  changing a shape it might depend on.
- If the API and mobile client ever diverge enough that additive-only stops being workable (e.g. a
  field genuinely must change meaning, or two incompatible mobile builds must be supported
  simultaneously), that is the trigger to revisit this decision and adopt real versioning — not before.
