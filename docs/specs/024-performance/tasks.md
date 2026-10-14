# Tasks: Performance pass across database, API, web and mobile

**Input**: [spec.md](./spec.md) and [plan.md](./plan.md)
**Purpose**: The checklist for Work Package 98. Task ids map to tracker items in brackets.

## Phase 1: Web first paint

- [x] T001 [98.1] Remove the Quill script and stylesheet from `GHCAA.Web/src/index.html` and load
  them once, with the same SRI hashes, from the admin editor that needs them.
- [x] T002 [98.2] Delete the `@import` of Outfit in `styles.scss`. Trim the `<link>` to the weights
  the stylesheets use (grep `font-weight`).
- [x] T003 Check the public pages and an admin editor in the browser. Record request count and
  transfer size for the landing page before and after.
  After, on a production build with the API mocked (2026-10-14): 37 requests, about 1.14 MB
  including images, 0 cdnjs and 0 Google Fonts stylesheet requests. No before figure was taken.

## Phase 2: Static caching

- [x] T004 [98.6] In `StaticFilesExtensions.cs`, give hashed `*.js` and `*.css` files
  `public, max-age=31536000, immutable`. Keep `no-cache` on `index.html`.
- [x] T005 [98.6] Check how upload file names are built. If unique per upload, give `/api/uploads`
  a one-day cache. Serve the missing-upload placeholder with `no-cache`.
  Names are `{prefix}_{guid}_{name}` (`LocalFileStorageService.cs`), so they are unique.
- [x] T006 Tests for the header rules (hashed chunk, `index.html`, placeholder).

## Phase 3: Asset weight

- [x] T007 [98.3] Recompress `public/assets/logo.png`, `logo.jpg`, `images/event-placeholder.jpg`,
  the gallery seed images and the campus images. Target under 100 KB each.
  The gallery photos stay above 100 KB. The grids load a 480 px thumbnail instead, so the target is met where it matters.
- [x] T008 [98.3] Grep web, mobile and API for `logo-spinner.svg`. Delete it if unreferenced.
- [x] T009 [98.3] Recompress `GHCAA.Mobile/assets/logo.png`.

## Phase 4: Database

- [x] T010 [98.11] Replace the per-member rank count with one query for the page.
- [x] T011 [98.13] Confirm how emails are written. Then compare with `==` on a normalised value and
  add an index on `Users.Email`.
- [x] T012 [98.14] Add a retention hosted service for Otps, RefreshTokens, EmailLogs, ActivityLogs
  and read Notifications. Day counts go in `Constants`. Add an index on the EmailLogs date column.
- [x] T013 [98.14] Test that the retention list has no election table (37.12s, the ten-year rule).
- [x] T014 [98.15] Load the year's existing dues member ids into a set before the generation loop.
- [x] T015 One PgSql migration with its Designer for the new indexes. Dry-run it on a copy of the
  local database.

## Phase 5: Mobile images and tracing

- [x] T016 [98.16] Add decode-size parameters to `CustomNetworkImage`.
- [x] T017 [98.16] Move the plain `Image.network` and `NetworkImage` uses in gallery, articles,
  event details, magazine, chats, org logo and the avatars onto the shared widget.
- [x] T018 [98.19] Lower Sentry `tracesSampleRate` outside debug builds.

## Phase 6: Correctness

- [x] T019 [98.20] Narrow the three `DbUpdateException` catches in `ElectionApprovalService` to a
  unique violation and log the exception. Keep the existing busy test and add one for a non-unique
  error.

## Phase 7: Web lists

- [x] T020 [98.4] Add a shared directive for `loading="lazy"` and `decoding="async"` and apply it
  to list and grid images. Set width and height where the size is fixed.
- [x] T021 [98.5] Add OnPush to the directory, admin tables, gallery and news. Change `track $index`
  to `track item.id` on server-backed lists that change.

## Phase 8: API remainder (needs decisions)

- [x] T022 [98.8] Cache the security stamp in `IMemoryCache` for up to 30 seconds and evict it when
  the stamp rotates. Waits on approval.
- [x] T023 [98.9] Stream secure file downloads with `PhysicalFile`. Lower the multipart buffer
  threshold.
- [x] T024 [98.10] Split a database-free liveness endpoint from `/health` and add Npgsql retry on
  failure. Waits on the Render health-check path.

## Phase 9: Later

- [x] T025 [98.17] Page the events, jobs and gallery endpoints and move web and mobile together.
- [x] T026 [98.18] Add a short in-session cache on mobile for lookups, events and jobs.
- [ ] T027 [98.7] Serve pre-compressed static assets.
- [x] T028 [98.12] Project the admin member list to the fields it shows.

## Every phase

- [ ] Run `python scripts/ci-local.py` and `graphify update .`. Tick the tracker item in the same
  change.
