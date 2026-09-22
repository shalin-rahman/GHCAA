# Idempotent member batch import plan

## Goal

Make the existing member bulk-import feature the supported, DB-safe way to deliver a historical
batch like Work Package 46.1 to a database that already has data in it, closing the gaps (CSV input,
PaymentHistory, cascaded idempotency, atomicity, per-run verification) that keep it short of that job
today, without touching the Tier 3 seed model that correctly handles brand-new-institution bootstrap.

## Dependencies

1. Confirm no other in-flight change is touching `MemberImportService.cs` or `MemberImportController.cs`
   before starting, since this plan edits both directly.
2. `MembershipNumber`'s new unique index (FR-2) is a schema change — it needs an EF migration of its
   own, generated and applied the normal way. This is a schema-definition migration, not a data-seed
   migration, so it is not in tension with the "no new migration for the batch" direction; only the
   *data* delivery moves off migrations.
3. Confirm the 631-row `profiles/ghc/demo-data/members.json` is still the best available source for
   the 47-row May 2026 batch before spending time re-deriving it from the CSV mentioned in Work
   Package 46's original heading, since neither the original CSV nor the abandoned migration is
   present in tracked history.

## Execution plan

### 1. Add the `MembershipNumber` unique index

Add the index in `MemberConfiguration.cs` and generate the accompanying EF migration. Run it against
a local copy of the database first and confirm no existing row collides (631 rows, each `"GHC-" + NID`
with NID itself already unique, so a collision would indicate corrupt data rather than an expected
case).

### 2. Accept CSV in the import endpoint

Add a CSV branch to `MemberImportService`'s file-reading step alongside the existing ClosedXML
`.xlsx` path — either parse CSV directly into the same row shape the Excel path produces, or convert
CSV to an in-memory `XLWorkbook` and reuse the existing parsing code unchanged. Keep the column-mapping
and default-values inputs the same for both formats so the controller and DTO don't need to change.

### 3. Extend cascaded idempotency to PaymentHistory and ProfessionalRecord

Add a `GetOrCreatePaymentHistory` helper matching the FR-3 key
(`TransactionId`, or `FinancialCategory`+`Amount`+`PaymentDate` when no transaction id is supplied),
following the same shape as the existing `GetGhcRecord`-style helpers. Confirm the existing
`ProfessionalRecord` helper already skips correctly for a member who has no organization/designation
data in the row (the original batch had this for 37 of 47 rows) rather than creating an empty record.

### 4. Wrap the batch in one transaction

Replace the current two-phase save (bulk Member save, then a per-row loop for User/Role/Photo with
swallowed per-row errors) with a single `IDbContextTransaction` spanning every write for the run.
Keep row-level validation errors (bad spreadsheet data) reported per row and non-fatal to the rest of
the batch; only a write-time failure triggers `RollbackAsync`. Photo file writes to disk happen outside
the DB transaction already (they're not transactional storage) — keep them ordered after a successful
commit so a rolled-back run doesn't leave orphaned photo files.

### 5. Report per-run counts

Extend the import response DTO with insert/update/skip counts broken out by table (Member,
AcademicRecord, ProfessionalRecord, PaymentHistory, User, UserRoles), so a real run's admin-facing
response is itself the DB-state verification the spec's FR-5 asks for.

### 6. Write the re-run integration test

Add a test to `GHCAA.Tests/Services/MemberImportServiceTests.cs` using the existing `TestBase` SQLite
fixture: seed a member with a full set of related rows (AcademicRecord, ProfessionalRecord,
PaymentHistory, User, UserRoles), run the import twice against a file containing that member plus new
members, and assert the second run's response reports zero inserts across every table and that the
raw row counts in the SQLite database are identical after the second run. Add a companion test that
forces a write failure mid-batch and asserts nothing from that run is left committed.

### 7. Validate against a real database, not just the test suite

Export the current member registry via the existing `GET /api/admin/members/import/export` endpoint
against a local Postgres copy seeded from `profiles/ghc/demo-data/`, then run the extended import
against a file built from that batch. Record the before/after row counts for all six tables directly
from the database (not from the seed JSON) and confirm they match FR-4/FR-5's expectations. This step
is the one the plan cannot complete inside this environment — no live database connection is reachable
from here — and needs to be run wherever the app's dev or preprod database is reachable before the
Work Package below is marked done.

### 8. Add a preview (dry-run) endpoint

Add `POST /api/admin/members/import/preview` to `MemberImportController`, backed by a
`MemberImportService.PreviewImportAsync` that reuses the same column-mapping, parsing, and FR-2/FR-3
idempotency-check code path as `ImportMembersAsync` but wraps its work in a transaction that always
rolls back (or skips `SaveChangesAsync` entirely, whichever keeps the code path closest to the real
one). Return a new `MemberImportPreviewResultDto`: one entry per row, each with its mapped field
values and a `List<MemberImportCellError>` (`RowIndex`, `ColumnKey`, `Message`) — replacing the flat
`Errors: List<string>` shape for this endpoint only; `MemberImportResultDto` (the commit endpoint's
response) is unchanged.

### 9. Build the reusable editable-grid component

Add `GHCAA.Web/src/app/shared/data-grid/` (or wherever the project's shared-component convention
places cross-feature components — check `common/` vs `shared/` naming before creating it) as a
standalone Angular component, generic over row shape: inputs for `columns` (key, label, edit type),
`rows`, and `cellErrors` (keyed by row index + column key, matching the new DTO); outputs for
`cellEdit` and `commit`. Style it from `styles.scss` tokens, following the project's existing
per-field error convention (`field-error` class, gated on touch/invalid) scaled to a per-cell version
keyed by row+column instead of a single form control.

### 10. Wire the preview step into the import modal

In `member-import-modal.component.ts`, insert the preview grid between column-mapping and the existing
commit action: call the new preview endpoint after mapping is confirmed, render its rows through the
new grid component, let the admin edit cells (re-validating client-side per FR-6), and only call the
existing `executeImport()` once no row has an outstanding error (or after the admin explicitly excludes
the remaining invalid rows). Type `admin.service.ts`'s `importMembers()` and the new preview call with
real interfaces instead of `Observable<any>`.

### 11. Update the tracker

Add a Work Package entry to `docs/TODO.md` noting that 46.1-46.5 in `docs/TODO_ARCHIVE.md` remain
correctly `[DONE]` for the work they describe, but that their delivery mechanism (the EF migration) no
longer exists in the tree, and that this spec's bulk-import path is the new way to (re-)deliver that
batch or any future one like it. Mark the new Work Package `[DONE]` only after step 7 has actually run
against a real database and the counts confirm no duplication.

## Delivery checkpoints

- CSV upload works end to end through the same endpoint as the existing `.xlsx` path.
- Re-running an identical import file a second time reports zero inserts everywhere and leaves row
  counts unchanged, proven both by the SQLite integration test and by a real run against a live
  database.
- A forced mid-batch write failure leaves no partial rows committed.
- `MembershipNumber` collisions are rejected with a reported error, not a silent overwrite.
- PaymentHistory rows exist for every imported member exactly once, matching the original 46.1 batch's
  scope (previously missing from the importer entirely).
- A file with a bad cell shows that cell, and only that cell, marked invalid in the preview grid before
  anything is written; fixing it in the grid clears the error without a re-upload; nothing commits while
  any row is still invalid.
