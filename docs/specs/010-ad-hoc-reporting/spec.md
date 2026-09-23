# Ad-hoc reporting for admins

## Purpose

Give admins a way to build a filtered, columned view of the platform's own data — members,
financial records, scholarships, campaigns, election turnout — without asking a developer to
write a one-off query. This is the one genuine gap in the campaigns/reporting/mentoring review
raised 2026-09-23: no `IReportService`, no reports controller, and no report entity exist
anywhere in the tree today. Everything else in that review (Campaigns, Mentoring) turned out to
be already built and just short of a rollout; this spec is a ground-up build for the remaining
gap.

Admin-only. No member, public, or mobile surface — this is an internal tool for the same
audience that already uses the admin CRUD screens, and no other admin-only backend feature in
this repo (election management, scholarship review) has a Flutter counterpart either.

## Actors and boundaries

- An admin picks an entity (Members, Financial Records, Scholarships, Campaigns, Election
  turnout), a set of filters, and a set of columns, and gets a result grid back.
- The admin can export that result set to Excel.
- The admin can save a report definition (name, entity, filters, columns) and re-run it later
  without rebuilding it from scratch.
- Filters are restricted to a whitelisted set of fields per entity — there is no free-text SQL
  box and no raw SQL path anywhere in the feature.
- The feature does not touch existing entities beyond reading them; it adds one new table,
  `SavedReports`, to hold saved definitions.

## Requirements

### FR-1 Entity and field discovery

The system shall expose the list of reportable entities and, per entity, the whitelisted set of
filterable and displayable fields, so the admin UI can build its filter form and column picker
without hardcoding field lists in Angular.

### FR-2 Filtered, columned query execution

The system shall accept an entity name, a set of filters (field, operator, value) drawn only
from that entity's whitelist, and a set of requested columns, and return matching rows. Filters
outside the whitelist shall be rejected, not silently ignored.

### FR-3 Excel export

The system shall export the current result set to an `.xlsx` file, reusing the Excel package
already referenced by `MemberImportService.cs` / `FileValidationService.cs` rather than adding a
new dependency. Confirm the exact package name and version in use before writing the export
service — do not assume it matches any particular library without checking the `.csproj`.

### FR-4 Saved report definitions

The system shall let an admin save a report definition (name, entity, filters, columns) tied to
the admin who created it, list their saved definitions, and re-run a saved definition by id.

### FR-5 Admin-only access

The system shall restrict every report route to authenticated admins, following the same
`[Authorize(Roles = ...)]` posture already used by `CampaignsController`'s admin routes and
`MentorshipController`'s `admin/all` route.

## Non-functional requirements

- **NFR-1 No arbitrary SQL.** Query building shall compose EF `IQueryable` expressions from the
  whitelist, never string-concatenated or raw SQL, so a filter payload cannot become an
  injection vector.
- **NFR-2 Per-entity field whitelist, not per-request trust.** The whitelist is enforced
  server-side on every request; a field name arriving from the client that is not on that
  entity's whitelist is a validation failure, not a filtered-out no-op.
- **NFR-3 No new Excel dependency.** Export reuses the existing package already in the
  dependency graph.
- **NFR-4 Result sets stay bounded.** A report run shall apply a maximum row cap (matching
  whatever paging convention the existing admin list endpoints use) so an unfiltered "all
  members" report cannot return an unbounded result set in one response.

## Acceptance criteria

1. `GET api/admin/reports/entities` returns the reportable entities and, per entity, its
   whitelisted filter and column fields.
2. Running a report with a filter field not on that entity's whitelist returns a validation
   error, not a 500 or a silently-ignored filter.
3. Running a report with valid filters and columns returns rows scoped to those filters,
   containing only the requested columns.
4. Exporting a result set produces a downloadable `.xlsx` file with a header row matching the
   requested columns and one row per result.
5. Saving a report definition and then re-running it by id returns the same result shape as
   running the original filter/column set directly.
6. A non-admin request to any `api/admin/reports/*` route is rejected before it reaches the
   query-building logic.

## Evidence required

- `GHCAA.Tests/Services/ReportServiceTests.cs`: filter-whitelist rejection, a saved-report
  round trip (save → list → re-run), and export producing the expected row/column shape.
- Angular vitest spec for the report builder page (entity picker, filter form, result grid,
  export action).
- API contract entry in `docs/API_CONTRACT_REGISTRY.md` for the new `api/admin/reports/*`
  routes.
- Migration or persistence validation for the new `SavedReports` table.
- FR/NFR tags on the new tests per the project's tagging convention.

## Implementation notes

- **Domain** (`GHCAA.Domain/Models/`): `SavedReport` — Id, Name, EntityType, FiltersJson,
  ColumnsJson, CreatedByUserId, CreatedAt.
- **Application** (`GHCAA.Application/`): `IReportService` with `GetEntityFields`, `RunReport`,
  `SaveReport`, `GetSavedReports`, `ExportToExcel`; request/response DTOs; a filter validator
  that rejects non-whitelisted fields.
- **Infrastructure** (`GHCAA.Infrastructure/`): `ReportService` implementation composing EF
  `IQueryable` per entity; `Data/Configurations/SavedReportConfiguration.cs`; a new migration
  `AddSavedReports`.
- **API**: `AdminReportsController` at `api/admin/reports`, admin-only, following the
  `CampaignsController`/`MentorshipController` authorization pattern.
- **Web (admin only)**: `GHCAA.Web/src/app/admin/reports/` — entity picker, filter form,
  `.data-table` result grid (reuse the existing table-card pattern from Directory), export
  button; `core/services/report.service.ts`; an `ADMIN_REPORTS` block in `API_ENDPOINTS`
  (`core/constants/app.constants.ts`), following the shape of the existing `ADMIN_ELECTIONS`
  block.
- **Web (member/public)**: none — out of scope by design.
- **Mobile**: none — out of scope by design, matching the convention that admin-only backend
  tools have no Flutter screen.

Tracked as `docs/TODO.md` Work Package 90.
