# Tasks: Ad-hoc Reporting for Admins

**Input**: [spec.md](./spec.md)

**Purpose**: Track delivery of the one confirmed gap from the 2026-09-23
campaigns/reporting/mentoring review. Every task below is future implementation
work — none of it exists in the codebase yet. This file authorizes planning
and sequencing, not a claim of completion. Admin-only feature: no member,
public, or mobile task appears here by design.

## Phase 1: Domain and persistence

- [ ] T001 Add `SavedReport` model (`GHCAA.Domain/Models/`) — Id, Name,
  EntityType, FiltersJson, ColumnsJson, CreatedByUserId, CreatedAt.
- [ ] T002 Add `SavedReportConfiguration` (`GHCAA.Infrastructure/Data/Configurations/`)
  and EF migration `AddSavedReports`.

## Phase 2: Application and query layer

- [ ] T003 Define the per-entity field whitelist (Members, Financial Records,
  Scholarships, Campaigns, Election turnout) — filterable and displayable
  fields, and the operators allowed per field type.
- [ ] T004 Add `IReportService` (`GetEntityFields`, `RunReport`, `SaveReport`,
  `GetSavedReports`, `ExportToExcel`) plus request/response DTOs.
- [ ] T005 Add a filter validator that rejects any field not on that entity's
  whitelist before query composition runs.

## Phase 3: Infrastructure and export

- [ ] T006 Implement `ReportService`, composing EF `IQueryable` per entity from
  validated filters — no raw SQL, no string-built queries.
- [ ] T007 Confirm the Excel package already referenced by
  `MemberImportService.cs` / `FileValidationService.cs` (check the `.csproj`,
  do not assume) and implement `ExportToExcel` against it — no new dependency.
- [ ] T008 Apply a max-row cap to `RunReport`, matching the paging convention
  used by existing admin list endpoints.

## Phase 4: API

- [ ] T009 Add `AdminReportsController` at `api/admin/reports`
  (`GET entities`, `POST run`, `POST export`, `POST saved`, `GET saved`,
  `POST saved/{id}/run`), `[Authorize(Roles = ...)]` matching
  `CampaignsController`/`MentorshipController`'s admin-route posture.
- [ ] T010 Register the new routes in `docs/API_CONTRACT_REGISTRY.md`.

## Phase 5: Web (admin only)

- [ ] T011 Add `ADMIN_REPORTS` block to `API_ENDPOINTS`
  (`core/constants/app.constants.ts`), shaped like the existing
  `ADMIN_ELECTIONS` block.
- [ ] T012 Add `core/services/report.service.ts`.
- [ ] T013 Add `GHCAA.Web/src/app/admin/reports/` — entity picker, filter
  form, `.data-table` result grid (reuse the Directory table-card pattern),
  export button, saved-report list and re-run action. Route behind the
  existing admin guard.

## Phase 6: Tests

- [ ] T014 NUnit `ReportServiceTests.cs`: filter outside the whitelist is
  rejected; saved-report round trip (save → list → re-run) returns the same
  shape as the original run; export produces the expected row/column count.
  Tag FR-1 through FR-5 and NFR-1/NFR-2 per test.
- [ ] T015 Vitest spec for the report builder page: entity switch resets
  filters, invalid filter shows a validation error, export button triggers
  the expected request.
