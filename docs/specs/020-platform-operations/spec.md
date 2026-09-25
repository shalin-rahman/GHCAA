# Feature Specification: Platform and Operations

**Feature Branch**: `020-platform-operations`

**Created**: 25-09-2026

**Status**: Draft (as-built)

**Input**: As-built documentation of the platform-operations domain: organisation configuration,
seasonal theming, reference lookups, health checks, the admin member-management console, member
activity logging, error-log review, and the dev-tracker admin screen. Controllers covered:
`OrgConfigController`, `ThemeController`, `LookupsController`, `HealthController`,
`AdminController`, `ActivityController`, `AdminErrorLogsController`, `AdminDevTrackerController`.

## Purpose and scope

This spec documents the parts of GHCAA that keep the platform running rather than serve a single
member-facing feature: organisation-wide configuration, the seasonal theme engine, shared lookup
lists, a health probe, the admin member-management console (approve, reject, archive, photo,
signature, ID card, certificate, contact messages), member activity logs, the error-log viewer, and
the dev-tracker screen that reads `docs/TODO.md`. It does not cover registration, payments,
elections, or any of the other domains that have their own spec under `docs/specs`.

## User Scenarios & Testing

### User Story 1 - Public pages load organisation branding and reference data (Priority: P1)

**Why this priority**: Every public page depends on org config (branding, contact details,
currency) and lookup lists (member types, districts, and so on). If these fail, the whole site
looks broken.

**Independent Test**: Call `GET api/config` and `GET api/lookups` with no auth header and confirm
both return `200 OK` with populated data.

**Acceptance Scenarios**:

1. **Given** no `OrganizationConfig` row exists yet, **When** `GET api/config` is called, **Then**
   the response is the hardcoded GHCAA default built by `OrgConfigService.BuildGhcaaDefaults()`.
2. **Given** an admin has saved a config, **When** any client calls `GET api/config` within 10
   minutes, **Then** the cached row is served without a database round trip.
3. **Given** a lookup group such as `MembershipType` exists, **When** `GET api/lookups/MembershipType`
   is called, **Then** only active items for that group are returned, ordered by `DisplayOrder`.

---

### User Story 2 - SuperAdmin edits organisation configuration (Priority: P1)

**Why this priority**: Branding, contact details, gateway method list, and workflow settings are
all controlled from one config row; a bad edit affects every member-facing page at once.

**Independent Test**: As a SuperAdmin, `PUT api/config` with a valid `OrgConfigDto` and confirm the
change is visible on the next `GET api/config` after the 10-minute cache window, or immediately if
the cache was already invalidated by the write.

**Acceptance Scenarios**:

1. **Given** a caller without the `SuperAdminOnly` policy, **When** they call `PUT api/config`,
   **Then** the request is rejected before reaching the handler (policy-based authorization).
2. **Given** a null request body, **When** `PUT api/config` is called, **Then** the response is
   `400` with a `ProblemDetails` payload.
3. **Given** a valid `OrgConfigDto`, **When** `PUT api/config` is called, **Then** the service
   writes a new `RowVersion`, records `UpdatedByAdminId` from the caller's JWT claim, and clears the
   config cache.

---

### User Story 3 - Admin reviews and approves pending members (Priority: P1)

**Why this priority**: Member approval is the gate between registration and an active membership;
it is the single highest-traffic admin workflow in this domain.

**Independent Test**: As an Admin, list members with `GET api/admin/members`, open one pending
member, and call `POST api/admin/members/{id}/approve`.

**Acceptance Scenarios**:

1. **Given** a member in a pending state, **When** an Admin calls
   `POST api/admin/members/{id}/approve`, **Then** the admin identity is read from
   `this.CurrentMemberIdRaw()` (the JWT claim), never from the request body.
2. **Given** a member id that does not exist, **When** approve is called, **Then** the controller
   catches `KeyNotFoundException` from the service and returns `404`.
3. **Given** a member already approved, **When** approve is called again, **Then** the service
   throws `InvalidOperationException` and the controller returns `400`.
4. **Given** a caller without SuperAdmin, **When** `GET api/admin/members?includeArchived=true` is
   called, **Then** the controller returns `403 Forbid` before querying archived rows.

---

### User Story 4 - Admin manages a member's photo, signature, and documents (Priority: P2)

**Why this priority**: These uploads feed the ID card and certificate generation and are a
recurring admin task after approval.

**Independent Test**: `POST api/admin/members/{id}/photo` with a file under 5 MB and confirm `200`;
repeat with a 6 MB file and confirm the size-limit rejection.

**Acceptance Scenarios**:

1. **Given** a photo under 5 MB, **When** `POST api/admin/members/{id}/photo` is called, **Then**
   the file is stored under `FileCategory.Image` and the response is `200`.
2. **Given** a signature under 2 MB, **When** `POST api/admin/members/{id}/signature` is called,
   **Then** the file is stored under `FileCategory.Image`.
3. **Given** a certificate or payment-proof file under 10 MB each, **When**
   `PATCH api/admin/members/{id}/documents` is called, **Then** the files are stored under
   `FileCategory.Document`.
4. **Given** an oversized or wrong-type file, **When** any of the three endpoints above is called,
   **Then** the controller returns `400` from the upload validation.

---

### User Story 5 - SuperAdmin reviews member activity, error logs, and the dev tracker (Priority: P2)

**Why this priority**: These three screens are the operational visibility tools a SuperAdmin uses
to check what members are doing, what is failing, and what work remains, without opening the
database or the raw `docs/TODO.md` file.

**Independent Test**: As a SuperAdmin, call `GET api/activity/admin/global`,
`GET api/admin/error-logs`, and `GET api/admin/dev-tracker` and confirm each returns `200` with a
list.

**Acceptance Scenarios**:

1. **Given** activity has been logged for several members, **When**
   `GET api/activity/admin/global` is called, **Then** the 50 most recent `ActivityLog` rows across
   all members are returned, each including the related `Member`.
2. **Given** error rows exist with different levels, **When**
   `GET api/admin/error-logs?level=Error` is called, **Then** only rows matching that level are
   returned, page size clamped to `Constants.ErrorLogs.MaxPageSize` (100).
3. **Given** `docs/TODO.md` has items marked `TODO` or `PARTIAL`, **When**
   `GET api/admin/dev-tracker` is called, **Then** only those items are returned, ordered by work
   package number then priority; items marked `DONE` are excluded.
4. **Given** `docs/TODO.md` is missing from both the content root and its parent directory, **When**
   `GET api/admin/dev-tracker` is called, **Then** the service logs a warning and returns an empty
   list rather than throwing.

---

### User Story 6 - Anyone checks platform health (Priority: P3)

**Why this priority**: Used by uptime monitors and deploy scripts, not by end users, so it is lower
priority than the workflows above but still needs to report accurately.

**Independent Test**: `GET healthz` with no auth header and confirm `200` when the database, file
storage, and email config are all reachable, and `503` when the database is down.

**Acceptance Scenarios**:

1. **Given** the database is unreachable, **When** `GET healthz` is called, **Then** the response is
   `503` with `Status: "Degraded"`, and the raw connection exception text is never included in the
   response (only a generic message, to avoid leaking host or credential details from a Npgsql
   failure).
2. **Given** the SMS config key `SmsSettings:Token` is empty, **When** `GET healthz` is called,
   **Then** the response still reports healthy, because SMS is optional in the current phase.
3. **Given** the upload directory configured at `FileStorage:BasePhysicalPath` (or the `wwwroot`
   fallback) does not exist, **When** `GET healthz` is called, **Then** the response is `503`.

---

### User Story 7 - Admin manages seasonal themes (Priority: P3)

**Why this priority**: Cosmetic and low-frequency; only touched around holidays or special events.

**Independent Test**: As an Admin, `POST api/Theme` a new `SpecialDayTheme` with a date range, then
confirm `GET api/Theme/active` returns it once the current date falls inside that range.

**Acceptance Scenarios**:

1. **Given** no `SpecialDayTheme` row has `IsEnabled=true` with a date range covering today (in
   Bangladesh local time), **When** `GET api/Theme/active` is called, **Then** the response is
   `204 No Content`.
2. **Given** two enabled themes both cover today, **When** `GET api/Theme/active` is called,
   **Then** the one with the later `StartDate` wins.
3. **Given** any theme is created, updated, or deleted, **When** the write completes, **Then** the
   5-minute `GHCAA_ActiveTheme` cache entry is cleared so the next read is not stale.

### Edge Cases

- `GET api/lookups`, `GET api/lookups/stats`, and `GET api/lookups/{group}` all carry the
  `PublicReference` output-cache policy (`LookupsController.cs:28,37,46`); a lookup item added by an
  Admin will not appear to anonymous callers until that cache entry expires or is invalidated.
- `AdminController.PUT members/{id}` blocks an Admin (not SuperAdmin) from editing a fellow
  SuperAdmin's own linked member record, to prevent an email rewrite that could be used for a
  self-serve password reset and privilege escalation (`AdminController.cs:193`).
- `ErrorLogService.LogAsync` never throws back into `ExceptionMiddleware`; a failure to write an
  error row is itself only logged as a warning (`ErrorLogService.cs`, `LogAsync`).
- `ErrorLogService` has no scheduled cleanup job; old rows are deleted opportunistically on
  roughly 1 in 20 writes (`ErrorLogService.cs:21-25`), so retention is approximate, not exact.
- `DevTrackerService.ResolveTodoPath` tries the content root first, then its parent directory,
  because the Docker and local `dotnet run` content roots differ (`DevTrackerService.cs`,
  `ResolveTodoPath`, comment mirrors the same pattern used by the institution-profile provider).
- `ThemeService.GetBangladeshTimeNow()` falls back through three strategies: the IANA `Asia/Dhaka`
  zone, the Windows `Bangladesh Standard Time` ID, then a plain `utcNow.AddHours(6)` if both time
  zone databases are unavailable (`ThemeService.cs`, `GetBangladeshTimeNow`).
- `OrgConfigService.UpdateConfigAsync` regenerates `RowVersion` as a new GUID on every write, because
  Npgsql and SQLite do not auto-generate a concurrency token and the column is `NOT NULL`
  (`OrgConfigService.cs`, `UpdateConfigAsync`).

## Requirements

### Functional Requirements

**Organisation configuration**

- FR-001: The system shall serve `GET api/config` to any caller, returning the current
  `OrganizationConfig` row or the built-in GHCAA defaults if none exists, cached for 10 minutes.
  [code+test]
- FR-002: The system shall require the `SuperAdminOnly` policy for `PUT api/config`, return `400`
  with a `ProblemDetails` body when the request body is null, and otherwise persist the config,
  record the admin id from the JWT claim, and invalidate the config cache. [code+test]

**Seasonal theming**

- FR-003: The system shall serve `GET api/Theme/active` to any caller, returning `204` when no
  `SpecialDayTheme` covers the current Bangladesh-local date, or the matching theme otherwise.
  [code+test]
- FR-004: The system shall require the `AdminOnly` policy for `GET api/Theme/all`, returning every
  `SpecialDayTheme` row. [code+test]
- FR-005: The system shall require the `AdminOnly` policy for `POST api/Theme`, creating a new
  `SpecialDayTheme` and returning `201 Created`. [code+test]
- FR-006: The system shall require the `AdminOnly` policy for `PUT api/Theme/{id}`, returning `400`
  when the route id does not match `theme.Id`, and otherwise updating the row. [code+test]
- FR-007: The system shall require the `AdminOnly` policy for `DELETE api/Theme/{id}`, deleting the
  matching `SpecialDayTheme` row. [code+test]

**Reference lookups**

- FR-008: The system shall serve `GET api/lookups/stats` to any caller under the `PublicReference`
  output-cache policy. [code+test]
- FR-009: The system shall serve `GET api/lookups` to any caller, returning active lookup items
  grouped by `LookupGroup` and ordered by `DisplayOrder`, under the `PublicReference` cache policy.
  [code+test]
- FR-010: The system shall serve `GET api/lookups/{group}` to any caller, returning active items for
  that group only, under the `PublicReference` cache policy. [code+test]
- FR-011: The system shall require the `AdminOnly` policy for `POST api/lookups`, creating a lookup
  item. [code+test]
- FR-012: The system shall require the `AdminOnly` policy for `PUT api/lookups/{id}`, returning
  `404` when the id does not exist. [code+test]
- FR-013: The system shall require the `AdminOnly` policy for `DELETE api/lookups/{id}`, returning
  `404` when the id does not exist. [code+test]

**Health**

- FR-014: The system shall serve `GET healthz` to any caller, checking database connectivity via
  `IDatabaseHealthService.CanConnectAsync`, returning `503` with `Status: "Degraded"` on failure
  without including the raw exception text. [code+test]
- FR-015: The system shall check that the directory named by `FileStorage:BasePhysicalPath` (or
  `wwwroot` if unset) exists as part of `GET healthz`, marking the response `503` if it does not.
  [code]
- FR-016: The system shall check that both `GmailSettings:Email` and `GmailSettings:AppPassword`
  are non-empty as part of `GET healthz`, marking the response `503` if either is missing. [code]
- FR-017: The system shall check that `SmsSettings:Token` is non-empty as part of `GET healthz`, but
  shall not fail the overall health status when it is empty, since SMS is optional in the current
  phase. [code]

**Admin member management**

- FR-018: The system shall require the `AdminOnly` policy for `GET api/admin/stats`, returning
  `GetDashboardStatsAsync` results scoped by the caller's privilege level. [code]
- FR-019: The system shall require the `AdminOnly` policy for `GET api/admin/analytics`, returning
  the same `GetDashboardStatsAsync` call as FR-018 (`AdminController.cs:32-43`). [code]
- FR-020: The system shall require the `AdminOnly` policy and the `RequireStepUp` filter for
  `POST api/admin/sync-members`, returning the count of synced records. [code+test]
- FR-021: The system shall require the `AdminOnly` policy for `GET api/admin/members`, returning a
  paged and filtered member list, and shall return `403 Forbid` when `includeArchived=true` is
  requested by a caller who is not SuperAdmin. [code+test]
- FR-022: The system shall require the `AdminOnly` policy for `GET api/admin/members/{id}`,
  returning a single member's detail. [code]
- FR-023: The system shall require the `AdminOnly` policy for `GET api/admin/members/{id}/documents`,
  returning that member's uploaded document metadata. [code]
- FR-024: The system shall require the `AdminOnly` policy for
  `POST api/admin/members/{id}/approve`, reading the admin id from the JWT claim (never the request
  body), returning `404` on `KeyNotFoundException` and `400` on `InvalidOperationException`.
  [code+test]
- FR-025: The system shall require the `AdminOnly` policy for
  `POST api/admin/members/{id}/revert-approval`, reverting an approved member to pending state.
  [code+test]
- FR-026: The system shall require the `AdminOnly` policy for `POST api/admin/members/{id}/reject`,
  rejecting a pending member. [code+test]
- FR-027: The system shall require the `SuperAdminOnly` policy and the `RequireStepUp` filter for
  `DELETE api/admin/members/{id}`, archiving the member. [code+test]
- FR-028: The system shall require the `SuperAdminOnly` policy and the `RequireStepUp` filter for
  `POST api/admin/members/bulk-archive-inactive`, archiving all inactive members and returning the
  count archived. [code+test]
- FR-029: The system shall require the `SuperAdminOnly` policy for
  `POST api/admin/members/{id}/restore`, returning `404` when the member does not exist. [code+test]
- FR-030: The system shall require the `AdminOnly` policy for
  `POST api/admin/members/{id}/reactivate`. [code+test]
- FR-031: The system shall require the `AdminOnly` policy for `PUT api/admin/members/{id}`, and
  shall reject an edit by an Admin (not SuperAdmin) to a fellow SuperAdmin's own linked member
  record. [code+test]
- FR-032: The system shall require the `AdminOnly` policy for `POST api/admin/members/{id}/photo`,
  accepting a file up to 5 MB and storing it under `FileCategory.Image`. [code+test]
- FR-033: The system shall require the `AdminOnly` policy for
  `POST api/admin/members/{id}/signature`, accepting a file up to 2 MB and storing it under
  `FileCategory.Image`. [code+test]
- FR-034: The system shall require the `AdminOnly` policy for
  `PATCH api/admin/members/{id}/documents`, accepting a certificate and a payment-proof file up to
  10 MB each and storing them under `FileCategory.Document`. [code+test]
- FR-035: The system shall require the `AdminOnly` policy for
  `GET api/admin/members/{id}/id-card` and `GET api/admin/members/{id}/id-card/pdf`, generating the
  ID card via `IIDCardService.GenerateIDCardDataUriAsync` or `GenerateIDCardPdfAsync`. [code]
- FR-036: The system shall require the `AdminOnly` policy and the `RequireStepUp` filter for
  `POST api/admin/members/{id}/reset-password-admin`, returning a confirmation message that a reset
  link was sent, without returning the link or a temporary password in the response body.
  [code+test]
- FR-037: The system shall require the `AdminOnly` policy for
  `GET api/admin/members/{id}/certificate` and `GET api/admin/members/{id}/certificate/pdf`,
  generating the certificate via `IIDCardService.GenerateCertificateDataUriAsync` or
  `GenerateCertificatePdfAsync`. [code]
- FR-038: The system shall require the `AdminOnly` policy for `GET api/admin/contact-messages`,
  returning all submitted contact messages via `IContactService.GetMessagesAsync`. [code]
- FR-039: The system shall require the `AdminOnly` policy for
  `POST api/admin/contact-messages/{id}/read`, marking a message as read via
  `IContactService.MarkAsReadAsync`. [code]
- FR-040: The system shall require the `AdminOnly` policy for
  `DELETE api/admin/contact-messages/{id}`, deleting a message via
  `IContactService.DeleteMessageAsync`. [code]

**Member activity**

- FR-041: The system shall require an authenticated caller for `GET api/activity/me`, reading the
  member id from the JWT claim via `this.CurrentMemberIdRaw()`, returning `400` via `ProblemDetails`
  when the claim is not a valid integer, and otherwise returning that member's 20 most recent
  `ActivityLog` rows. [code]
- FR-042: The system shall require the `AdminOnly` policy for `GET api/activity/admin/{memberId}`,
  returning that member's 50 most recent `ActivityLog` rows. [code]
- FR-043: The system shall require the `SuperAdminOnly` policy for `GET api/activity/admin/global`,
  returning the 50 most recent `ActivityLog` rows across all members, each including its related
  `Member`. [code]

**Error logs and dev tracker**

- FR-044: The system shall require the `SuperAdminOnly` policy for `GET api/admin/error-logs`,
  filtering by `fromDate`, `toDate`, `level` (skipped when `"all"`), and a case-insensitive
  substring `query` across `Message`, `ExceptionType`, and `StackTrace`, clamping `pageSize` to
  `Constants.ErrorLogs.MaxPageSize` (100) and defaulting to 20 when `pageSize` is 0 or less.
  [code+test]
- FR-045: The system shall require the `SuperAdminOnly` policy for `GET api/admin/dev-tracker`,
  parsing `docs/TODO.md` and returning only items with status `TODO` or `PARTIAL`, filtered by
  `priority` when given, ordered by work package number then priority, and returning an empty list
  (not an error) when the file cannot be found. [code+test]

### Key Entities

- **OrganizationConfig**: single-row-per-org table holding `ConfigJson` (branding, contact,
  currency, features, workflow, localization), `RowVersion`, `UpdatedAt`, `UpdatedByAdminId`. Read
  and written entirely through `OrgConfigService`.
- **SpecialDayTheme**: `IsEnabled`, `StartDate`, `EndDate`, and styling fields (gradient, sidebar
  colour). Bound directly as the request body of `ThemeController.CreateTheme` and `UpdateTheme`,
  with no separate DTO.
- **LookupItem**: `LookupGroup`, `Value`, `Label`, `DisplayOrder`, `IsActive`. Backs every
  dropdown-style reference list across the app.
- **ActivityLog**: `MemberId`, `Timestamp`, auto-detected `IpAddress`, `UserAgent`, and `Source`
  (`"Mobile"` or `"Web"`, inferred from the user agent string). Written by `ActivityService.LogActivityAsync`.
- **ErrorLog**: `Message`, `ExceptionType`, `StackTrace`, `Level`, `CreatedAt`. Written by
  `ErrorLogService.LogAsync` from `ExceptionMiddleware`; self-prunes rows older than
  `Constants.ErrorLogs.RetentionDays` (90 days).
- **docs/TODO.md items**: not a database entity. `DevTrackerService` parses work-package headers and
  item lines from the file at request time; there is no persisted `DevTrackerItem` table.
- **ContactMessage**: read through `IContactService`, whose `GetMessagesAsync` method returns
  `IEnumerable<object>` rather than a named DTO (see Gaps).

## Evidence

| FR | Route | Service | Angular file | Flutter file | Test |
|----|-------|---------|---------------|---------------|------|
| FR-001 | GET api/config | OrgConfigService.GetConfigAsync | GHCAA.Web/src/app/core/services/org-config.service.ts | GHCAA.Mobile/lib/core/services/org_config_service.dart | GHCAA.Tests/Integration/OrgConfigControllerTests.cs:17 |
| FR-002 | PUT api/config | OrgConfigService.UpdateConfigAsync | GHCAA.Web/src/app/admin/org-config/org-config.ts | none found | GHCAA.Tests/Integration/OrgConfigControllerTests.cs:37,54 |
| FR-003 | GET api/Theme/active | ThemeService.GetActiveThemeAsync | GHCAA.Web/src/app/core/services/theme.service.ts | GHCAA.Mobile/lib/features/theme/dynamic_theme_service.dart | GHCAA.Tests/Controllers/ThemeControllerTests.cs:44,59 |
| FR-004 | GET api/Theme/all | ThemeService.GetAllThemesAsync | GHCAA.Web/src/app/admin/themes/admin-themes.ts | GHCAA.Mobile/lib/screens/admin/theme_management_screen.dart | GHCAA.Tests/Controllers/ThemeControllerTests.cs:69 |
| FR-005 | POST api/Theme | ThemeService.CreateThemeAsync | GHCAA.Web/src/app/admin/themes/admin-themes.ts | GHCAA.Mobile/lib/screens/admin/theme_management_screen.dart | GHCAA.Tests/Controllers/ThemeControllerTests.cs:84 |
| FR-006 | PUT api/Theme/{id} | ThemeService.UpdateThemeAsync | GHCAA.Web/src/app/admin/themes/admin-themes.ts | GHCAA.Mobile/lib/screens/admin/theme_management_screen.dart | GHCAA.Tests/Controllers/ThemeControllerTests.cs:99,110 |
| FR-007 | DELETE api/Theme/{id} | ThemeService.DeleteThemeAsync | GHCAA.Web/src/app/admin/themes/admin-themes.ts | GHCAA.Mobile/lib/screens/admin/theme_management_screen.dart | GHCAA.Tests/Controllers/ThemeControllerTests.cs:122 |
| FR-008 | GET api/lookups/stats | LookupService.GetPublicStatsAsync | GHCAA.Web/src/app/core/services/lookup.service.ts | GHCAA.Mobile/lib/features/lookups/lookup_service.dart | GHCAA.Tests/Controllers/LookupsControllerTests.cs:32,44 |
| FR-009 | GET api/lookups | LookupService.GetAllLookupsAsync | GHCAA.Web/src/app/core/services/lookup.service.ts | GHCAA.Mobile/lib/features/lookups/lookup_service.dart | GHCAA.Tests/Controllers/LookupsControllerTests.cs:55,70 |
| FR-010 | GET api/lookups/{group} | LookupService.GetByGroupAsync | GHCAA.Web/src/app/core/services/lookup.service.ts | GHCAA.Mobile/lib/features/lookups/lookup_service.dart | GHCAA.Tests/Controllers/LookupsControllerTests.cs:82,94 |
| FR-011 | POST api/lookups | LookupService.AddLookupItemAsync | GHCAA.Web/src/app/core/services/lookup.service.ts | none found | GHCAA.Tests/Controllers/LookupsControllerTests.cs:106 |
| FR-012 | PUT api/lookups/{id} | LookupService.UpdateLookupItemAsync | GHCAA.Web/src/app/core/services/lookup.service.ts | none found | GHCAA.Tests/Controllers/LookupsControllerTests.cs:132,143 |
| FR-013 | DELETE api/lookups/{id} | LookupService.DeleteLookupItemAsync | GHCAA.Web/src/app/core/services/lookup.service.ts | none found | GHCAA.Tests/Controllers/LookupsControllerTests.cs:154,164 |
| FR-014 | GET healthz | DatabaseHealthService.CanConnectAsync | GHCAA.Web/src/app/common/health/health.ts | none found | GHCAA.Tests/Controllers/HealthControllerTests.cs:35,47 |
| FR-015 | GET healthz | HealthController (inline check) | GHCAA.Web/src/app/common/health/health.ts | none found | none found |
| FR-016 | GET healthz | HealthController (inline check) | GHCAA.Web/src/app/common/health/health.ts | none found | none found |
| FR-017 | GET healthz | HealthController (inline check) | GHCAA.Web/src/app/common/health/health.ts | none found | none found |
| FR-018 | GET api/admin/stats | MemberService_Search.GetDashboardStatsAsync | GHCAA.Web/src/app/core/services/admin.service.ts | GHCAA.Mobile/lib/screens/admin/admin_dashboard_screen.dart | none found |
| FR-019 | GET api/admin/analytics | MemberService_Search.GetDashboardStatsAsync | GHCAA.Web/src/app/core/services/admin.service.ts | none found | none found |
| FR-020 | POST api/admin/sync-members | MemberService_Sync (not read in this pass) | GHCAA.Web/src/app/core/services/admin.service.ts | none found | GHCAA.Tests/Controllers/AdminControllerTests.cs:159,169 |
| FR-021 | GET api/admin/members | MemberService_Search (not fully read in this pass) | GHCAA.Web/src/app/core/services/admin.service.ts | GHCAA.Mobile/lib/screens/admin/approval_queue_screen.dart | GHCAA.Tests/Controllers/AdminControllerTests.cs:40 |
| FR-022 | GET api/admin/members/{id} | MemberService (not read in this pass) | GHCAA.Web/src/app/core/services/admin.service.ts | GHCAA.Mobile/lib/screens/admin/approval_queue_screen.dart | none found |
| FR-023 | GET api/admin/members/{id}/documents | MemberService (not read in this pass) | GHCAA.Web/src/app/core/services/admin.service.ts | none found | none found |
| FR-024 | POST api/admin/members/{id}/approve | MemberService_Approval.ApproveMemberAsync | GHCAA.Web/src/app/core/services/admin.service.ts | GHCAA.Mobile/lib/screens/admin/approval_queue_screen.dart | GHCAA.Tests/Controllers/AdminControllerTests.cs:54 |
| FR-025 | POST api/admin/members/{id}/revert-approval | MemberService_Approval (not fully read in this pass) | GHCAA.Web/src/app/core/services/admin.service.ts | none found | GHCAA.Tests/Controllers/AdminControllerTests.cs:79,90,101 |
| FR-026 | POST api/admin/members/{id}/reject | MemberService_Approval.RejectMemberAsync | GHCAA.Web/src/app/core/services/admin.service.ts | GHCAA.Mobile/lib/screens/admin/approval_queue_screen.dart | GHCAA.Tests/Controllers/AdminControllerTests.cs:67 |
| FR-027 | DELETE api/admin/members/{id} | MemberService_Approval.ArchiveMemberAsync | GHCAA.Web/src/app/core/services/admin.service.ts | none found | GHCAA.Tests/Controllers/AdminControllerTests.cs:114 |
| FR-028 | POST api/admin/members/bulk-archive-inactive | MemberService_Approval (not fully read in this pass) | GHCAA.Web/src/app/core/services/admin.service.ts | none found | GHCAA.Tests/Controllers/AdminControllerTests.cs:179,189 |
| FR-029 | POST api/admin/members/{id}/restore | MemberService (not read in this pass) | GHCAA.Web/src/app/core/services/admin.service.ts | none found | GHCAA.Tests/Controllers/AdminControllerTests.cs:199,209 |
| FR-030 | POST api/admin/members/{id}/reactivate | MemberService (not read in this pass) | GHCAA.Web/src/app/core/services/admin.service.ts | none found | GHCAA.Tests/Controllers/AdminControllerTests.cs:125 |
| FR-031 | PUT api/admin/members/{id} | MemberService_Profile (not read in this pass) | GHCAA.Web/src/app/core/services/admin.service.ts | none found | GHCAA.Tests/Controllers/AdminControllerTests.cs:147 |
| FR-032 | POST api/admin/members/{id}/photo | MemberService_Profile (not read in this pass) | GHCAA.Web/src/app/core/services/admin.service.ts | none found | GHCAA.Tests/Controllers/AdminControllerTests.cs:219,231 |
| FR-033 | POST api/admin/members/{id}/signature | MemberService_Profile (not read in this pass) | GHCAA.Web/src/app/core/services/admin.service.ts | none found | GHCAA.Tests/Controllers/AdminControllerTests.cs:244,256 |
| FR-034 | PATCH api/admin/members/{id}/documents | MemberService_Profile (not read in this pass) | GHCAA.Web/src/app/core/services/admin.service.ts | none found | GHCAA.Tests/Controllers/AdminControllerTests.cs:269,281 |
| FR-035 | GET api/admin/members/{id}/id-card(/pdf) | IIDCardService (implementation not read in this pass) | GHCAA.Web/src/app/core/services/admin.service.ts | none found | none found |
| FR-036 | POST api/admin/members/{id}/reset-password-admin | MemberService (not read in this pass) | GHCAA.Web/src/app/core/services/admin.service.ts | none found | GHCAA.Tests/Controllers/AdminControllerTests.cs:136 |
| FR-037 | GET api/admin/members/{id}/certificate(/pdf) | IIDCardService (implementation not read in this pass) | GHCAA.Web/src/app/core/services/admin.service.ts | none found | none found |
| FR-038 | GET api/admin/contact-messages | IContactService (implementation not read in this pass) | GHCAA.Web/src/app/admin/contact-messages/contact-messages.ts | GHCAA.Mobile/lib/screens/admin/contact_messages_screen.dart | none found |
| FR-039 | POST api/admin/contact-messages/{id}/read | IContactService (implementation not read in this pass) | GHCAA.Web/src/app/admin/contact-messages/contact-messages.ts | GHCAA.Mobile/lib/screens/admin/contact_messages_screen.dart | none found |
| FR-040 | DELETE api/admin/contact-messages/{id} | IContactService (implementation not read in this pass) | GHCAA.Web/src/app/admin/contact-messages/contact-messages.ts | GHCAA.Mobile/lib/screens/admin/contact_messages_screen.dart | none found |
| FR-041 | GET api/activity/me | ActivityService.GetMemberActivityAsync | none found | GHCAA.Mobile/lib/screens/member/member_activity_history_screen.dart | none found |
| FR-042 | GET api/activity/admin/{memberId} | ActivityService.GetMemberActivityAsync | none found | none found | none found |
| FR-043 | GET api/activity/admin/global | ActivityService.GetRecentGlobalActivityAsync | GHCAA.Web/src/app/admin/audit/admin-audit.ts | GHCAA.Mobile/lib/screens/admin/audit_screen.dart | none found |
| FR-044 | GET api/admin/error-logs | ErrorLogService.GetPagedAsync | GHCAA.Web/src/app/admin/error-logs/admin-error-logs.ts | none found | GHCAA.Tests/Services/ErrorLogServiceTests.cs:67,80,93,114,131 |
| FR-045 | GET api/admin/dev-tracker | DevTrackerService.GetOpenItemsAsync | GHCAA.Web/src/app/admin/dev-tracker/admin-dev-tracker.ts | none found | GHCAA.Tests/Services/DevTrackerServiceTests.cs:52,72,88,104,114 |

## Gaps

- `GetStats` and `GetAnalytics` call the identical `MemberService_Search.GetDashboardStatsAsync`
  with the same arguments and return the same shape (`AdminController.cs:32-43`).
  [NEEDS CLARIFICATION: were these meant to return different data, or is one route dead and should
  be removed?]
- `IContactService.GetMessagesAsync` returns `Task<IEnumerable<object>>` rather than a typed DTO
  (`GHCAA.Application/Interfaces/IContactService.cs:10`), so `GET api/admin/contact-messages` has no
  documented response contract beyond "some list of objects."
  [NEEDS CLARIFICATION: what is the actual shape of a contact message record returned here?]
- `ThemeController.CreateTheme` and `UpdateTheme` bind the raw `SpecialDayTheme` domain entity
  directly from the request body (`ThemeController.cs:40,48`), with no request DTO in between. Every
  other write endpoint in this domain (OrgConfig, Lookups) goes through a DTO first.
- `AdminErrorLogsController` and `AdminDevTrackerController` have no controller-level test file
  (`GHCAA.Tests/Controllers/` has no `AdminErrorLogsControllerTests.cs` or
  `AdminDevTrackerControllerTests.cs`); the only coverage is at the service layer
  (`ErrorLogServiceTests.cs`, `DevTrackerServiceTests.cs`), so routing, policy enforcement, and query
  binding for these two controllers are untested.
- `ActivityController` has no test file at all (no `ActivityControllerTests.cs` found under
  `GHCAA.Tests/Controllers/`), so none of its three endpoints, including the JWT-claim parsing on
  `GET api/activity/me`, are covered by a named test.
- No Angular file was found that calls `GET api/activity/me` or `GET api/activity/admin/{memberId}`;
  only the global-activity route (`ADMIN_GLOBAL`) is wired up, in
  `GHCAA.Web/src/app/core/services/admin.service.ts:236` and consumed by
  `GHCAA.Web/src/app/admin/audit/admin-audit.ts:42`.

## Enhancements: modularisation and reusability

### Reuse across layers

- ENH-001 (P2): `AdminController.GetStats` (`AdminController.cs:32`) and `GetAnalytics`
  (`AdminController.cs:40`) both call `_memberService.GetDashboardStatsAsync(isPrivileged, ct)` with
  no difference in arguments or handling. One of the two routes is a duplicate; keeping both invites
  the two to drift apart if only one is ever updated.

### Entity-based module shape

- ENH-002 (P3): `ErrorLogService.CleanupOldRowsAsync` and `DevTrackerService.GetOpenItemsAsync`
  both read from a flat text-derived or time-derived source with no repository abstraction; this is
  consistent with their small, admin-only scope and does not need a shared module.

### Existing reusable components

- ENH-003 (P3): `GHCAA.Web/src/app/admin/dev-tracker`, `admin/error-logs`, `admin/themes`, and
  `admin/contact-messages` were not checked in this pass for use of the shared `app-page-header`,
  `app-search-bar`, or `app-breadcrumb` components named in project memory. This needs a follow-up
  read of those four component files before it can be confirmed either way.
  [NEEDS CLARIFICATION: do the four admin list screens above reuse the shared list-page shell
  components, or did each hand-roll its own header and search markup?]

### Hard-coded behaviour that should be configuration

- ENH-004 (P2): `OrgConfigService.BuildGhcaaDefaults()` (`OrgConfigService.cs:127-326`) hardcodes the
  entire GHCAA branding, contact, currency, gateway-method list, workflow, and localization defaults
  in code. This is already tracked as a planned removal once `ORG_PROFILE=ghc` is set and
  `IInstitutionProfileProvider` takes over (per the class-level comment at `OrgConfigService.cs:13-22`
  and the `OrgConfigGoldenSnapshotTests` that pin the two paths as byte-identical), so this is not a
  new finding, only a confirmation that the migration is still pending.
- ENH-005 (P3): The upload size limits (5 MB photo, 2 MB signature, 10 MB per document) are literal
  numbers in `AdminController.cs` at the photo, signature, and documents endpoints, rather than named
  constants. `Constants.ErrorLogs.RetentionDays` and `MaxPageSize` show the codebase already has a
  convention for this in `GHCAA.Domain/Constants.cs:125-130`; the upload limits do not yet follow it.

## Success Criteria

### Measurable Outcomes

- SC-001: `GET api/config` and `GET api/lookups*` respond without a database query for any caller
  within the same cache window (10 minutes for config, the `PublicReference` output-cache duration
  for lookups).
- SC-002: A member approval, rejection, or archive action always records the acting admin's id from
  the JWT claim, never from a client-supplied field.
- SC-003: `GET healthz` returns `503` whenever the database is unreachable, and never includes the
  raw database exception text in its response body.
- SC-004: `GET api/admin/dev-tracker` never throws when `docs/TODO.md` is missing; it returns an
  empty list and logs a warning instead.

## Assumptions

- The 10-minute config cache and 5-minute theme cache are in-process `IMemoryCache` entries; on a
  multi-instance deployment each instance would cache independently. Whether GHCAA currently runs
  more than one API instance was not checked in this pass.
- `Constants.Policies.AdminOnly` and `SuperAdminOnly` (`GHCAA.Domain/Constants.cs:18-19`) are the
  same two policies used across every other domain spec in this set; this spec did not re-verify
  their registration in `Program.cs`.
- The exact bodies of `MemberService_Search.GetDashboardStatsAsync`,
  `MemberService_Approval.ApproveMemberAsync`/`RejectMemberAsync`/`ArchiveMemberAsync`, and
  `IIDCardService`/`IContactService`'s concrete implementations were not opened in this pass; the
  Evidence table marks their service cells accordingly rather than guessing their internals.
