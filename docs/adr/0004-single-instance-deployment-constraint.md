# ADR-0004: Single-instance deployment constraint

Status: Accepted

## Context

The platform runs as one Render web service instance today, and three in-process mechanisms only
work correctly under that assumption:

- **Output cache** — `Program.cs:74` registers `AddOutputCache()` with no distributed backing (no
  Redis, no SQL store). Cached responses live in that one process's memory.
- **`OrgConfigService` / `ThemeService`** — both take an `IMemoryCache` constructor dependency
  directly (`OrgConfigService.cs:25`, `ThemeService.cs:24,37`), not through a cache abstraction that
  could be swapped for a distributed provider. An admin edit invalidates the cache entry in whichever
  process handled that request.
- **`ChatHub`** — `GHCAA.API/Hubs/ChatHub.cs:17` tracks `userId → connectionId` in a `static
  ConcurrentDictionary`, and `Program.cs:170` registers `AddSignalR()` with no backplane (no
  Redis backplane, no Azure SignalR). Group membership and the connection map exist only in the
  process that holds the WebSocket.

None of this is a bug. At one instance, an in-memory cache and a process-local connection map are the
correct, cheapest choice — adding Redis-backed caching and a SignalR backplane now, with nothing that
needs them, would be exactly the overengineering this project's review process rules out elsewhere
(see `docs/ARCHITECTURE_AUDIT_2026-09.md` and `docs/TODO.md` item 82.20).

## Decision

The platform is documented as supporting exactly one running instance. Scaling out to two or more
instances is not supported without code changes first, and this file plus
`docs/RENDER_DEPLOYMENT.md`'s "Scaling beyond one instance" section are where that constraint is
recorded, so it isn't rediscovered by trial and error during an incident.

## Consequences

Running two or more instances today would fail silently in three specific ways, not with an error:

1. **Output cache** — the same URL can return a stale response from one instance and a fresh one from
   another, because each instance's cache fills and expires independently.
2. **Config/theme cache** — an admin's edit through `OrgConfigService` or `ThemeService` invalidates
   the `IMemoryCache` entry only on the instance that handled the write. Every other instance keeps
   serving the old value until its own cache entry happens to expire.
3. **SignalR / `ChatHub`** — a client connected to instance A never receives a push sent by code
   running on instance B, because `Clients.Group(...)` only reaches connections held by the process
   that issued the call, and `_connections` on instance B has no idea the user is online through A.

Before scaling to 2+ instances, in this order:

1. Give the output cache a distributed backing (a Redis- or SQL-backed output cache store) instead of
   the default in-memory one.
2. Replace the direct `IMemoryCache` dependency in `OrgConfigService` and `ThemeService` with a
   distributed cache (or drop caching and rely on the database, if the read volume doesn't need it).
3. Add a SignalR backplane (Redis backplane is the standard .NET option) so group membership and
   message delivery are shared across instances, and rework `ChatHub`'s connection map to not rely on
   process-local state.

None of this is built. Doing it before there's a second instance to serve would be exactly the kind
of premature work this decision explicitly avoids.
