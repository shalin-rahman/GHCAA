# API Contract Registry

A changelog of changes to the API surface: what changed, when, and whether the
Web and Mobile clients were updated to match. Add an entry here whenever a
route, request DTO, or response DTO changes — see the "Add a new API
endpoint" row in `docs/PROJECT_MAP.md`'s Impact Guide and the workflow note
in `.github/copilot-instructions.md`.

This complements ADR-0005 (`docs/adr/0005-no-api-versioning-compatibility-rule-instead.md`),
which sets the *rule* for how endpoints may change (additive only, never
remove or rename). This file is the *record* of each time that rule was
applied — one entry per change, so a client left behind is visible at a
glance.

## Current endpoint surface

The full route list lives in one place and this file doesn't duplicate it
line by line: `GHCAA.Web/src/app/core/constants/app.constants.ts`, the
`API_ENDPOINTS` constant (currently ~40 routes across Admin, Auth, Events,
Gallery, News, Financials, Messaging, Governance, Polls, and more). The
Flutter side calls the same routes from `GHCAA.Mobile/lib/**/*_service.dart`
files, without a matching constants file — when adding an endpoint there,
check `API_ENDPOINTS` first so the two clients agree on the literal path.

The CI pipeline (`.github/workflows/ghcaa-ci-standard.yml`) already runs an
API contract snapshot comparison against `docs/api/swagger.json`
(`docs/api/diff_swagger.py`) — that catches an accidental breaking change.
This registry is the complementary manual record: it captures *why* a change
was made and *which client picked it up*, which a snapshot diff can't tell
you.

## Entry format

One entry per endpoint change, newest first:

```
### YYYY-MM-DD — <endpoint path or DTO name>
- Change: <what changed — new field, new route, deprecated field, etc.>
- Reason: <why>
- Web: <updated in <file> | not applicable | pending>
- Mobile: <updated in <file> | not applicable | pending>
```

Only log a change once it's actually shipped (merged), not while still in
review. If a change lands on the API but a client update is still pending,
say so — that's the point of the registry: a change with no client entry is
a gap someone still needs to close.

## Log

### 2026-09-17 — /networking/search, /networking/directory
- Change: added an optional `cursor` param to `MemberSearchFilterDto` and a
  populated `NextCursor` on `PagedResult<T>`. Sending a cursor switches the
  server from offset Skip/Take to keyset pagination ordered by
  `(FullName, Id)`. Omitting it keeps the old `page`/`pageSize` behavior, so
  this is additive per ADR-0005.
- Reason: TODO 8.3 — offset pagination re-scans and re-sorts the full result
  set on every page for the Alumni Registry list, which gets slower as the
  member count grows. Cursor pagination reads only the next page.
- Web: not applicable — `member-approval`/admin list screens on the Web side
  weren't touched; they still use `page`/`pageSize` and keep working
  unchanged.
- Mobile: updated in `GHCAA.Mobile/lib/features/networking/networking_service.dart`
  and `GHCAA.Mobile/lib/screens/member/directory_screen.dart`
  (`professional_hub_screen.dart` shares the same endpoint but wasn't
  touched — it still sends no cursor and gets the legacy offset behavior).

### 2026-09-17 — docs/API_CONTRACT_REGISTRY.md (this file)
- Change: registry created. No endpoint changed; this establishes the format
  above and the cross-references from `docs/PROJECT_MAP.md` and
  `.github/copilot-instructions.md`.
- Reason: TODO 12.2 — WP12 (Process & Engineering Standards) asked for a
  changelog of endpoint changes and which clients picked each one up. Past
  endpoint changes weren't logged anywhere in this format, so the log starts
  here rather than backfilling guesses.
- Web: n/a
- Mobile: n/a
