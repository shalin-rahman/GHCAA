# Implementation Plan: Performance pass across database, API, web and mobile

**Spec**: [spec.md](./spec.md)
**Tracker**: `docs/TODO.md` Work Package 98

## Order

The work runs cheapest-first by effect on users. Each phase stands alone and ends with the full
local gate (`python scripts/ci-local.py`) and `graphify update .`.

1. **Web first paint (98.1, 98.2).** Two small edits to `index.html` and `styles.scss`. They help
   every visitor on every page.
2. **Static caching (98.6).** One change in `StaticFilesExtensions.cs`. Hashed chunks get a one-year
   immutable header. `index.html` keeps `no-cache`. Uploads get a one-day header only if their file
   names are unique per upload, which is checked first.
3. **Asset weight (98.3).** Recompress the shipped images and drop the unused spinner after a grep
   across web, mobile and the API. The originals stay in git history.
4. **Database (98.11, 98.13, 98.14, 98.15).** The rank count becomes one grouped query. The email
   lookup change waits on a check of how emails are written. The retention job is one hosted
   service using `ExecuteDeleteAsync`, with day counts in `Constants`, and it never touches
   election tables (37.12s, the ten-year rule). Index changes come as one PgSql migration with its
   Designer file, checked by `EveryMigrationClass_IsDiscoveredByEf`.
5. **Mobile images (98.16, 98.19).** Add decode-size parameters to `CustomNetworkImage` and move the
   plain `Image.network` and `NetworkImage` uses onto it. Lower the Sentry trace rate outside debug.
6. **Correctness (98.20).** Narrow the `ElectionApprovalService` catch with the same
   `PostgresException.SqlState` check `MigrationBootstrapper` already uses, plus the SQLite code for
   tests. Log the exception.
7. **Web lists (98.4, 98.5).** A shared lazy-image directive and OnPush on the list-heavy screens.
8. **API remainder (98.8, 98.9, 98.10).** 98.8 changes how session revocation works and needs the
   user's approval before it starts. 98.10 needs the Render health-check path, which is not in the
   repo.
9. **Paging and caching on mobile (98.17, 98.18), and the low items (98.7, 98.12).** 98.17 changes
   API contracts that web also reads, so all three clients move together.

## Decisions to confirm before the phase that needs them

- 98.8: is a revocation delay of up to 30 seconds acceptable?
- 98.10: what path does Render's health check call?
- 98.13: are emails stored lowercased on every write path?
- 98.14: how many days to keep each table? Proposed: Otps 7, RefreshTokens 30 after expiry or
  revoke, EmailLogs 180, ActivityLogs 365, read Notifications 180.

## Measuring

Where a number can be taken, the task records it before and after: transfer size and request count
from the browser network panel for web, query count from EF logging for the database, and file
sizes for assets. A task with no number says so.

## Risk

- Long cache headers on a file whose name is not hashed would serve stale content after a deploy.
  The rule matches the hash pattern only, and `index.html` stays `no-cache`.
- The retention job deletes data. It runs per table with a fixed list and a test pins that election
  tables are not on it.
- Narrowing the approval catch turns some errors from "busy" into a 500. That is the point: they
  were bugs reported as clashes.
