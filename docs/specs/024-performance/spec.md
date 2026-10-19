# Feature Specification: Performance pass across database, API, web and mobile

**Feature Branch**: 024-performance
**Created**: 2026-10-14
**Status**: Mostly built. As of 2026-10-20 the tracker items under Work Package 98 run to 98.43. Of the items this spec raised only 98.7 (pre-compressed static files) is still open, and 98.34 is on hold
**Input**: User request of 2026-10-14: "i want all specs plans, tasks for performance improvement,
that mat db, api, FE,design or else first, than start fix". Four read-only audits of the same day
(database and EF, API, Angular web with its assets, Flutter mobile) supplied the findings.
**Depends on**: [001-platform-baseline](../001-platform-baseline/spec.md) (the as-built platform)
**Tracker**: `docs/TODO.md` items 98.1 to 98.20 for the findings below, under Work Package 98. Later items in that package (to 98.43) came from other reviews

## Purpose and scope

The audits looked for slow paths at the scale the platform runs at today: hundreds to low
thousands of members, a few elections a year, and traffic spikes on election day and after event
announcements. Many users are on mid-range Android phones on mobile data. Nothing found will fail
at that scale. The findings below are the ones that cost real time or data on every visit, or that
grow without bound.

In scope: EF queries and indexes, table retention, API static-file and response headers, the
per-request auth check, file streaming, the web first paint (scripts, fonts, images), web change
detection on long lists, mobile image decoding and caching, mobile list paging and in-session
caching, and one correctness fix found in the same review.

Out of scope: a CDN, a service worker, server-side rendering, and the mobile local database
(tracker 8.5, Isar or Drift) and state hydration (tracker 8.7). Those stay where they are.

Already done, not repeated here: directory cursor paging (8.3), mobile background JSON decode
(8.4), mobile retry with backoff (8.6), the web initial-bundle budget (96.13), the org config cache
(28.4 web and API, 28.18 mobile), output caching on public reads (44.17), error log retention
(45.6), and the earlier index set (archive items 24.33 to 24.38).

## Findings

Each finding has an id from the audit, the evidence, and the tracker item that carries the fix.
Impact is rated against the scale above.

### Database and EF

| Id | Finding | Impact | Item |
|---|---|---|---|
| DB-1 | The admin member list runs one rank count per member (`GetMemberGainsAsync` in `MemberService_Helpers.cs`, called in a loop in `MemberService_Search.cs`). `ContributionPoints` has no index. | Medium | 98.11 |
| DB-2 | The admin member list loads four related collections for every row to show a few summary fields. | Low | 98.12 |
| DB-4 | Password reset compares `Email.ToLower()`, so the unique index on `Members.Email` is not used. `Users.Email` has no index. | Low speed, needs a check on how emails are stored | 98.13 |
| DB-5 | EmailLogs, ActivityLogs, Otps, RefreshTokens and Notifications are never trimmed. EmailLogs has no date index. | Grows with time | 98.14 |
| DB-6 | Dues generation checks one member at a time (`FinancialService.cs`). | Low, yearly | 98.15 |

Election tables are kept for ten years (tracker 37.12s, where that rule is pinned with a test).
No retention job may touch them.

### API

| Id | Finding | Impact | Item |
|---|---|---|---|
| API-1 | Hashed JS and CSS chunks and uploaded files get no `Cache-Control`, so browsers re-check every file on each visit (`StaticFilesExtensions.cs`). The missing-upload placeholder is served as 200 and cached. | Medium | 98.6 |
| API-2 | Static assets are compressed on each request. Nothing serves pre-compressed files. | Low once 98.6 is in | 98.7 |
| API-3 | Every signed-in request reads the user's security stamp from the database (`SecurityStampMiddleware.cs`). | Medium on Neon | 98.8 |
| API-4 | Secure file downloads read the whole file into memory. The multipart buffer threshold equals the body limit, so uploads up to that size stay in memory. | Low to Medium | 98.9 |
| API-6 | `/health` opens a database connection, which keeps Neon awake if Render polls it. Npgsql has no retry for the first request after a Neon suspend. | Low, cost and cold start | 98.10 |

### Web client and design assets

| Id | Finding | Impact | Item |
|---|---|---|---|
| WEB-1 | Quill's script and stylesheet load in `index.html` on every page and block first paint. Only admin editors use Quill. | High | 98.1 |
| WEB-2 | The Outfit font is requested twice, once by `<link>` and once by `@import` in `styles.scss`, with different weights. | High | 98.2 |
| WEB-3 to WEB-6 | `logo.png` is 528 KB, `event-placeholder.jpg` 707 KB, gallery seed images up to 404 KB used as thumbnails, and `logo-spinner.svg` is 1.2 MB with no reference found. | High on mobile data | 98.3 |
| WEB-7 | One of 86 images is lazy-loaded. None carry width and height. | Medium | 98.4 |
| WEB-8, WEB-9 | Seven of 103 components use OnPush. Some long mutable lists track by `$index`. | Low to Medium | 98.5 |

### Mobile client

| Id | Finding | Impact | Item |
|---|---|---|---|
| MOB-1, MOB-2 | No image sets a decode size, so avatars and thumbnails decode at full size. Only the directory, events and governance screens use the shared cached image widget. The rest re-download after a restart. | High on low-end phones | 98.16 |
| MOB-3 | Events, jobs and gallery load everything in one call. | Medium, grows with data | 98.17 |
| MOB-4 | No in-session cache. The same lists are fetched on every screen open. | Low to Medium | 98.18 |
| MOB-5 | `assets/logo.png` is 527 KB. | Low to Medium | 98.3 |
| MOB-6 | Sentry samples every trace in release builds. | Low to Medium | 98.19 |

### Correctness found in the same review

`ElectionApprovalService` catches any `DbUpdateException` while an audit row is pending and reports
"busy". A foreign key or length error would be reported the same way and the exception is not
logged. Tracker 98.20 narrows the catch to a unique violation and logs the rest.

## Requirements

- **PR-1** A returning visitor's browser does not re-request hashed web assets it already holds.
- **PR-2** The public pages do not block first paint on a third-party script or a second font request.
- **PR-3** No shipped image used as a fallback or thumbnail is larger than 100 KB.
- **PR-4** List images on web load lazily and reserve their space.
- **PR-5** Mobile images decode at roughly the size they are shown and persist across restarts.
- **PR-6** No admin list issues one query per row.
- **PR-7** Log and token tables are trimmed on a schedule. Election records are never trimmed early.
- **PR-8** A signed-in request does not need a database round trip only to check the security
  stamp, and a revoked session ends within 30 seconds.
- **PR-9** Each change keeps the existing tests green and adds tests for any new rule.

## Not adopted

- A CDN in front of Render. It would solve API-1 and API-2 together, but it is an infrastructure
  decision outside the code.
- A service worker. It adds a new dependency and a cache-invalidation problem for a portal that is
  mostly signed-in data.
- `NoTracking` as the EF default. Many services update the entities they load, so a blanket switch
  is risky for microseconds of gain.
- Trigram indexes for member search. A full scan of a few thousand rows takes a few milliseconds.

## Known limits

The audits read code. They did not run `EXPLAIN`, profile a live request or measure a page load on
a phone. Each task records a before and after measurement where one can be taken, and the impact
ratings above should be read as estimates until then.
