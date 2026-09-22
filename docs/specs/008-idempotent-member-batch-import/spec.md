# Idempotent member batch import

## Purpose

Give historical member batches (starting with the May 2026 Haraganga alumni batch, Work Package
46.1) a delivery path into an already-populated database that is safe to re-run. The path that
delivered 46.1 originally — a dedicated EF migration alongside the seed JSON — no longer exists in
the tree: the seed files moved to `profiles/ghc/demo-data/` under the WP62 white-label work, and the
class that now loads them, `InstitutionDataSeeder`, seeds a table only when that table is completely
empty. On any real deployment, `Members` already holds rows, so `InstitutionDataSeeder` never inserts
the batch. This spec does not touch `InstitutionDataSeeder` or the Tier 3 seed model — that model is
correct for its own job (bootstrapping a brand-new institution) and wrong for this one (adding rows to
an institution that already exists). Instead, it extends the existing admin bulk-import feature
(`MemberImportController` / `MemberImportService`), which already performs a NID-keyed upsert across
Member, AcademicRecord, ProfessionalRecord, User, and the UserRoles join, to become the supported way
to deliver a batch like 46.1 to a live database, and to close the gaps that keep it short of that job
today.

## Actors and boundaries

- An admin uploads a spreadsheet through the existing `POST /api/admin/members/import` endpoint.
- `MemberImportService.ImportMembersAsync` reads the file, matches rows to existing members by NID or
  email, and writes Member, AcademicRecord, ProfessionalRecord, User, and UserRoles rows.
- This spec adds PaymentHistory to that set, since the original 46.1 batch recorded a membership-fee
  payment per member and the current importer does not touch that table.
- The feature covers the backend import path and, per FR-6, a new preview step in the existing
  `member-import-modal` flow: a dry-run endpoint plus an editable grid shown between file upload and
  commit. It does not change `InstitutionDataSeeder`, the seed JSON files, or any EF migration.

## Requirements

### FR-1 CSV input

The import endpoint shall accept a `.csv` file in addition to the `.xlsx` files it already accepts,
so a Google Forms export (the source format of the original 46.1 batch) does not need a manual
conversion step before upload.

### FR-2 Idempotency by natural key

The service shall continue to treat `Member.NID` and `Member.Email` as the existing-row match key.
Rows whose NID already exists in the database shall be treated as updates, never as inserts, matching
current behaviour.

`MembershipNumber` shall gain a unique database index (it does not have one today, despite being
derived deterministically from NID by convention: `"GHC-" + NID`). The service shall reject an import
row whose computed `MembershipNumber` collides with a different member's existing value, the same way
it already reports NID/email collisions.

### FR-3 Cascaded idempotency across related tables

For a row that resolves to an existing member, the service shall insert AcademicRecord,
ProfessionalRecord, PaymentHistory, User, and UserRoles rows only when a matching row is not already
present for that member — never a second time for a member who already has one. The existing
find-or-create pattern used for AcademicRecord (`GetGhcRecord`/`GetHscRecord`/`GetHighestRecord`,
matching by shape: institution + degree + record type, not by primary key) is the model to extend to
ProfessionalRecord and PaymentHistory. A member who already has a User account shall not get a second
one; a member whose User already holds the `Member` role shall not get the role added twice.

PaymentHistory idempotency shall match on `(MemberId, TransactionId)` when the row supplies a
transaction id, or on `(MemberId, FinancialCategory, Amount, PaymentDate)` when it does not, following
the disambiguation convention already recorded for the original 46.1 batch (Work Package 46.2's
collision handling).

### FR-4 Atomicity

The service shall wrap the full set of writes for an import run — every row's Member,
AcademicRecord, ProfessionalRecord, PaymentHistory, User, and UserRoles changes — in a single database
transaction. A failure partway through the batch shall roll back every change made by that run, not
leave a Member row committed with its related rows missing. This replaces the current two-phase save
(one `SaveChangesAsync` for Members, a second loop for User/Role/Photo work that swallows per-row
errors), which does not give this guarantee today.

A row-level parsing or validation error (bad data in one spreadsheet row) shall still be reported per
row and shall not by itself fail the transaction for the rest of the batch; only a write failure (a
constraint violation, a database error) shall trigger the rollback.

### FR-5 DB-state verification

The import response shall report, per run, how many rows were inserted, how many were matched as
updates, and how many related-table rows (by type) were created versus already present and skipped.
This is the mechanism for validating idempotency against the live database after a real run, rather
than inferring it from the seed JSON.

### FR-6 Preview grid with in-grid editing and per-cell validation

Today the import endpoint is commit-only: an admin uploads a file and finds out what was wrong only
after the write already happened (a flat `Errors: List<string>` with no row or column pointer,
confirmed absent of any preview step — `member-import-modal.component.ts` goes straight from file
select to `POST /api/admin/members/import`). This is the gap the user flagged against 88.4: DB-state
verification after the fact is not enough when the admin had no chance to fix a bad cell before commit.

The service shall add a preview (dry-run) endpoint that runs the same column-mapping, parsing, and
validation as a real import — including the FR-2/FR-3 idempotency checks — but writes nothing. It
shall return one row per input row, each with its mapped field values and a list of validation errors,
each error naming the specific row index and column key it belongs to (not a flat message), covering
at minimum: required-field-missing, malformed value for its target type (date, number, email), and a
`MembershipNumber`/NID collision (FR-2).

The Angular import flow shall show this preview in an editable grid before the admin can commit:
each cell that has a validation error is marked at that exact cell, not in a separate error list, and
the admin can edit a cell's value directly in the grid. Editing a cell shall re-validate just that
row against the same rules the preview endpoint used (client-side re-check of format/required rules;
collision checks that need a DB round-trip are re-verified server-side on commit). The admin commits
only once every row is free of validation errors, or explicitly excludes still-invalid rows from the
commit.

This grid shall be built as a generic, reusable component (not specific to member import) taking rows,
column definitions, and per-cell errors as inputs, and emitting cell-edit and commit events — since no
reusable grid or table component exists anywhere in `GHCAA.Web` today (every admin list page hand-rolls
its own `<table>` markup) and a second bulk-data feature needing preview-and-edit is a realistic near
future, not a hypothetical one.

## Non-functional requirements

- Re-running the same file against a database that already holds its data is a no-op: it inserts
  nothing, changes nothing it doesn't need to, and returns the same success response with all-zero
  insert counts.
- The transaction (FR-4) does not hold locks for longer than the batch actually needs; large batches
  should still complete within the existing request timeout used by the import endpoint today.
- `Member` and `User` stay Class A entities (soft-delete only, per `docs/ARCHITECTURE.md` §4); nothing
  in this import path hard-deletes or bypasses `IsArchived`.
- The Tier 3 seed classification in `docs/SEED_CLASSIFICATION.md` and `InstitutionDataSeeder`'s
  empty-table-only behaviour are left exactly as they are; this spec adds a second, independent
  delivery path rather than modifying the first.

## Dependencies

- `GHCAA.API/Controllers/MemberImportController.cs` and `GHCAA.Infrastructure/Services/MemberImportService.cs`
  — the existing endpoint and service this spec extends.
- `GHCAA.Tests/Services/MemberImportServiceTests.cs` and `GHCAA.Tests/Controllers/MemberImportControllerTests.cs`
  — existing coverage for insert, update-by-NID, and duplicate-NID-in-file behaviour that the new
  tests build alongside.
- `GHCAA.Tests/TestBase.cs` — the SQLite in-memory fixture pattern used by existing service tests.
- `profiles/ghc/demo-data/members.json` — holds the 631-row dataset (584 original + the 47-row May
  2026 batch) that can be exported back to a spreadsheet as the source file for a real import run,
  since the original CSV and the abandoned migration are not present in the tracked history.
- Work Package 46 in `docs/TODO_ARCHIVE.md` (46.1-46.5) — the closed tracker entries this spec's
  Work Package addendum in `docs/TODO.md` points back to.
- `GHCAA.Web/src/app/admin/members/member-import-modal/member-import-modal.component.ts` and its
  template — the existing upload/column-mapping flow FR-6's preview step slots into.
- `GHCAA.Web/src/styles.scss` and `theme.service.ts` — the token system the new grid component must
  use instead of hardcoded colors/spacing.
- `GHCAA.Web/src/app/core/utils/table-pagination.util.ts` and `GHCAA.Web/src/app/common/pagination/
  pagination.component.ts` — the closest existing precedent for shared table-adjacent state, though
  neither supports cell editing or cell-level errors; the new grid is a new component, not an
  extension of these.

## Acceptance scenarios

1. Given a CSV file with a member row whose NID already exists in the database, when the import runs,
   then no new Member row is created, the existing row is updated, and the response counts it as an
   update, not an insert.
2. Given a file with a mix of brand-new NIDs and already-present NIDs, when the import runs, then only
   the new NIDs produce new Member rows and every related table (AcademicRecord, ProfessionalRecord,
   PaymentHistory, User, UserRoles) gets exactly one row per member per record type, never more.
3. Given a member who already has a User account and the `Member` role, when a file containing that
   member's row is imported again, then no second User row and no duplicate UserRoles row are created.
4. Given the same file imported twice in a row, when the second run completes, then the response
   reports zero inserts across every table and the database's row counts (Member, AcademicRecord,
   ProfessionalRecord, PaymentHistory, User, UserRoles) are identical before and after the second run.
5. Given a batch where one row's write fails partway through (a simulated constraint violation), when
   the transaction rolls back, then none of that run's rows — including ones that would otherwise have
   succeeded — are left committed in the database.
6. Given a row whose computed `MembershipNumber` would collide with a different existing member's
   value, when the import runs, then that row is rejected with a reported error and no row for it is
   written to any table.
7. Given an uploaded file with one row missing a required field, when the preview runs, then the grid
   shows that row's cell for that column marked invalid, with no error shown against any other cell in
   that row or against other rows, and nothing is written to the database.
8. Given a preview row marked invalid for a malformed date, when the admin edits that cell to a valid
   date in the grid, then the cell's error clears without a full re-upload and without affecting any
   other row's state.
9. Given a preview grid with at least one row still showing a validation error, when the admin attempts
   to commit, then the commit is blocked (or that row is excluded, per the admin's choice) and only
   valid rows reach the import endpoint.
